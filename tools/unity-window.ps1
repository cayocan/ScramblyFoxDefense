# Usage:
#   .\tools\unity-window.ps1 status              # Unity windows + open dialogs (message and buttons)
#   .\tools\unity-window.ps1 click "Save"        # click a dialog button by its text (ignores '&')
#   .\tools\unity-window.ps1 focus               # bring the editor to the foreground (unfreezes throttled ticks)
#   .\tools\unity-window.ps1 shot out.png        # capture the editor (or the open dialog) to a PNG
#   .\tools\unity-window.ps1 watch               # background loop: auto-answer known-safe dialogs, log the rest
#                                                # to Logs\unity-window-watch.log, keep the PC awake while it runs
#   .\tools\unity-window.ps1 watch keepfocus     # same, and bring Unity to the front every 30 s (unattended only)
# Talks to Windows directly, so it works when the Unity Pipeline server is stuck on the main thread.
param(
    [Parameter(Mandatory = $true)][ValidateSet('status', 'click', 'focus', 'shot', 'watch')][string]$Action,
    [string]$Arg = ''
)
$ErrorActionPreference = 'Stop'

Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @'
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

public static class UnityWin
{
    delegate bool EnumProc(IntPtr hwnd, IntPtr lParam);
    [DllImport("user32.dll")] static extern bool EnumWindows(EnumProc cb, IntPtr lParam);
    [DllImport("user32.dll")] static extern bool EnumChildWindows(IntPtr parent, EnumProc cb, IntPtr lParam);
    [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int GetWindowText(IntPtr hwnd, StringBuilder text, int max);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)] static extern int GetClassName(IntPtr hwnd, StringBuilder text, int max);
    [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr hwnd);
    [DllImport("user32.dll")] static extern bool IsWindowEnabled(IntPtr hwnd);
    [DllImport("user32.dll")] static extern bool IsIconic(IntPtr hwnd);
    [DllImport("user32.dll")] static extern bool ShowWindow(IntPtr hwnd, int cmd);
    [DllImport("user32.dll")] static extern bool SetForegroundWindow(IntPtr hwnd);
    [DllImport("user32.dll")] static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] static extern bool AttachThreadInput(uint a, uint b, bool attach);
    [DllImport("kernel32.dll")] static extern uint GetCurrentThreadId();
    [DllImport("user32.dll")] static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);
    [DllImport("user32.dll")] static extern bool BringWindowToTop(IntPtr hwnd);
    [DllImport("user32.dll")] static extern IntPtr PostMessage(IntPtr hwnd, uint msg, IntPtr w, IntPtr l);
    [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);
    [DllImport("user32.dll")] static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);

    [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
    [DllImport("kernel32.dll")] public static extern uint SetThreadExecutionState(uint flags);
    [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left, Top, Right, Bottom; }

    const uint BM_CLICK = 0x00F5;

    public static string Text(IntPtr h) { var sb = new StringBuilder(512); GetWindowText(h, sb, 512); return sb.ToString(); }
    public static string Class(IntPtr h) { var sb = new StringBuilder(256); GetClassName(h, sb, 256); return sb.ToString(); }
    public static bool Visible(IntPtr h) { return IsWindowVisible(h); }
    public static bool Enabled(IntPtr h) { return IsWindowEnabled(h); }

    public static List<IntPtr> TopWindows(uint pid)
    {
        var list = new List<IntPtr>();
        EnumWindows((h, l) => { uint p; GetWindowThreadProcessId(h, out p); if (p == pid && IsWindowVisible(h)) list.Add(h); return true; }, IntPtr.Zero);
        return list;
    }

    public static List<IntPtr> Children(IntPtr parent)
    {
        var list = new List<IntPtr>();
        EnumChildWindows(parent, (h, l) => { list.Add(h); return true; }, IntPtr.Zero);
        return list;
    }

    /// <summary>PostMessage, so a stuck dialog owner can never block this script.</summary>
    public static void Click(IntPtr button) { PostMessage(button, BM_CLICK, IntPtr.Zero, IntPtr.Zero); }

    /// <summary>Windows only lets the foreground process hand over focus: attach input + Alt tap trick.</summary>
    public static bool Focus(IntPtr hwnd)
    {
        if (IsIconic(hwnd)) ShowWindow(hwnd, 9); // SW_RESTORE
        uint pid;
        uint target = GetWindowThreadProcessId(hwnd, out pid);
        uint current = GetCurrentThreadId();
        uint fg = GetWindowThreadProcessId(GetForegroundWindow(), out pid);
        keybd_event(0x12, 0, 0, UIntPtr.Zero);       // Alt down
        keybd_event(0x12, 0, 2, UIntPtr.Zero);       // Alt up
        AttachThreadInput(current, fg, true);
        AttachThreadInput(current, target, true);
        BringWindowToTop(hwnd);
        bool ok = SetForegroundWindow(hwnd);
        AttachThreadInput(current, target, false);
        AttachThreadInput(current, fg, false);
        return ok && GetForegroundWindow() == hwnd;
    }

    public static bool Shot(IntPtr hwnd, string path)
    {
        RECT r; GetWindowRect(hwnd, out r);
        int w = r.Right - r.Left, h = r.Bottom - r.Top;
        if (w <= 0 || h <= 0) return false;
        using (var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb))
        using (var g = Graphics.FromImage(bmp))
        {
            IntPtr hdc = g.GetHdc();
            bool ok = PrintWindow(hwnd, hdc, 2); // PW_RENDERFULLCONTENT: works for background windows
            g.ReleaseHdc(hdc);
            bmp.Save(path, ImageFormat.Png);
            return ok;
        }
    }
}
'@

# Physical pixels, so window captures are not cropped on scaled displays.
[void][UnityWin]::SetProcessDPIAware()

$project = Split-Path -Leaf (Split-Path -Parent $PSScriptRoot)
$unity = Get-Process -Name Unity -ErrorAction SilentlyContinue |
    Where-Object { $_.MainWindowTitle -like "*$project*" } | Select-Object -First 1
if (-not $unity) {
    # The main title can be empty while a modal dialog is up: fall back to any Unity owning a window for the project.
    $unity = Get-Process -Name Unity -ErrorAction SilentlyContinue |
        Where-Object { [UnityWin]::TopWindows([uint32]$_.Id) | Where-Object { [UnityWin]::Text($_) -like "*$project*" } } |
        Select-Object -First 1
}
if (-not $unity) { throw "No Unity editor found for project '$project'." }

$windows = [UnityWin]::TopWindows([uint32]$unity.Id)
$main = $windows | Where-Object { [UnityWin]::Text($_) -like "*$project*" } | Select-Object -First 1
# Native modal dialogs (EditorUtility.DisplayDialog, save prompts, safe mode) use the #32770 class.
$dialogs = @($windows | Where-Object { [UnityWin]::Class($_) -eq '#32770' })

function Describe-Dialog($hwnd) {
    $kids = [UnityWin]::Children($hwnd)
    $message = ($kids | Where-Object { [UnityWin]::Class($_) -eq 'Static' } | ForEach-Object { [UnityWin]::Text($_) } | Where-Object { $_ }) -join ' / '
    $buttons = ($kids | Where-Object { [UnityWin]::Class($_) -eq 'Button' -and [UnityWin]::Visible($_) } | ForEach-Object { '[' + ([UnityWin]::Text($_) -replace '&', '') + ']' }) -join ' '
    "DIALOG '$([UnityWin]::Text($hwnd))': $message  buttons: $buttons"
}

# Progress bars ("Hold on...", "Building Player") are #32770 windows too: no buttons, or only Cancel.
function IsQuestion($hwnd) {
    $buttons = @([UnityWin]::Children($hwnd) | Where-Object { [UnityWin]::Class($_) -eq 'Button' -and [UnityWin]::Visible($_) } |
        ForEach-Object { [UnityWin]::Text($_) -replace '&', '' })
    return ($buttons.Count -gt 1) -or ($buttons.Count -eq 1 -and $buttons[0] -ne 'Cancel')
}

# Dialogs that are always safe to answer without a human. Anything else is only logged.
$SafeAnswers = @(
    @{ Title = '*modified externally*'; Button = 'Reload' }  # git changed the open scene: take the committed version
)

if ($Action -eq 'watch') {
    $log = Join-Path (Split-Path -Parent $PSScriptRoot) 'Logs\unity-window-watch.log'
    # ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED: no sleep or idle lock while this loop runs.
    [void][UnityWin]::SetThreadExecutionState([uint32]2147483651)
    Add-Content $log "$(Get-Date -Format s) watch started (pid $PID)"
    $seen = @{}
    $lastFocus = [DateTime]::MinValue
    while ($true) {
        if (-not (Get-Process -Id $unity.Id -ErrorAction SilentlyContinue)) {
            Add-Content $log "$(Get-Date -Format s) Unity exited, watch stops"
            break
        }
        foreach ($d in @([UnityWin]::TopWindows([uint32]$unity.Id) | Where-Object { [UnityWin]::Class($_) -eq '#32770' })) {
            $title = [UnityWin]::Text($d)
            $rule = $SafeAnswers | Where-Object { $title -like $_.Title } | Select-Object -First 1
            if ($rule) {
                $button = [UnityWin]::Children($d) | Where-Object {
                    [UnityWin]::Class($_) -eq 'Button' -and ([UnityWin]::Text($_) -replace '&', '') -eq $rule.Button
                } | Select-Object -First 1
                if ($button) {
                    [UnityWin]::Click($button)
                    Add-Content $log "$(Get-Date -Format s) auto [$($rule.Button)] on '$title'"
                }
            }
            elseif (-not $seen.ContainsKey([string]$d) -and (IsQuestion $d)) {
                $seen[[string]$d] = $true
                Add-Content $log "$(Get-Date -Format s) NEEDS DECISION: $(Describe-Dialog $d)"
            }
        }
        # Unattended mode: a background Unity stops ticking after domain reloads, so keep it in front.
        if ($Arg -eq 'keepfocus' -and ((Get-Date) - $lastFocus).TotalSeconds -ge 30) {
            $lastFocus = Get-Date
            $mainNow = [UnityWin]::TopWindows([uint32]$unity.Id) | Where-Object { [UnityWin]::Text($_) -like "*$project*" } | Select-Object -First 1
            if ($mainNow) { [void][UnityWin]::Focus($mainNow) }
        }
        Start-Sleep -Seconds 3
    }
    return
}

switch ($Action) {
    'status' {
        Write-Host "Unity pid $($unity.Id), main window: '$(if ($main) { [UnityWin]::Text($main) } else { 'not found' })', enabled=$(if ($main) { [UnityWin]::Enabled($main) })"
        foreach ($w in $windows) {
            $cls = [UnityWin]::Class($w)
            if ($cls -eq '#32770') { Write-Host (Describe-Dialog $w) }
            elseif ($w -ne $main) { Write-Host "window '$([UnityWin]::Text($w))' class=$cls" }
        }
        if ($dialogs.Count -eq 0) { Write-Host 'No modal dialogs open.' }
    }
    'click' {
        if (-not $Arg) { throw 'Give the button text, e.g. click "OK".' }
        foreach ($d in $dialogs) {
            $button = [UnityWin]::Children($d) | Where-Object {
                [UnityWin]::Class($_) -eq 'Button' -and ([UnityWin]::Text($_) -replace '&', '') -like $Arg
            } | Select-Object -First 1
            if ($button) {
                Write-Host "Clicking [$Arg] in: $(Describe-Dialog $d)"
                [UnityWin]::Click($button)
                return
            }
        }
        throw "No open dialog has a button '$Arg'. Run status first."
    }
    'focus' {
        $target = if ($dialogs.Count -gt 0) { $dialogs[0] } else { $main }
        Write-Host "Focus: $([UnityWin]::Focus($target))"
    }
    'shot' {
        $out = if ($Arg) { $Arg } else { Join-Path $env:TEMP 'unity-window.png' }
        $target = if ($dialogs.Count -gt 0) { $dialogs[0] } else { $main }
        [void][UnityWin]::Shot($target, $out)
        Write-Host "Saved $out"
    }
}
