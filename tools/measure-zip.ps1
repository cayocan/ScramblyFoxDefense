# Usage: .\tools\measure-zip.ps1 [-Out Builds\package.zip]
# Packs the WebGL build at the ZIP root plus readable source under source/, then prints the size in bytes.
# Brief limit: 5,000,000 bytes for the whole ZIP.
param([string]$Out = 'Builds\package.zip')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem

$root = Split-Path -Parent $PSScriptRoot
$build = Join-Path $root 'Builds\WebGL'
if (-not (Test-Path (Join-Path $build 'index.html'))) { throw 'Builds\WebGL\index.html not found. Build first.' }

$outPath = Join-Path $root $Out
if (Test-Path $outPath) { Remove-Item $outPath }

$sourceItems = @('Assets', 'Packages\manifest.json', 'Packages\packages-lock.json', 'ProjectSettings', 'tools')
$rootDocs = @('README.md', 'CREDITS.md', 'PROJECT_NOTE.md', 'TIME_LOG.md')

$zip = [System.IO.Compression.ZipFile]::Open($outPath, 'Create')
function Add-Entry([string]$file, [string]$entry) {
    [void][System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $file, $entry.Replace('\', '/'), 'Optimal')
}
try {
    Get-ChildItem $build -Recurse -File | ForEach-Object { Add-Entry $_.FullName $_.FullName.Substring($build.Length + 1) }
    foreach ($doc in $rootDocs) {
        $p = Join-Path $root $doc
        if (Test-Path $p) { Add-Entry $p $doc }
    }
    foreach ($item in $sourceItems) {
        $p = Join-Path $root $item
        if (-not (Test-Path $p)) { continue }
        Get-ChildItem $p -Recurse -File -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -notmatch '\\(node_modules|out)\\' } | ForEach-Object {
            Add-Entry $_.FullName ('source\' + $_.FullName.Substring($root.Length + 1))
        }
    }
}
finally { $zip.Dispose() }

$size = (Get-Item $outPath).Length
$buildSize = (Get-ChildItem $build -Recurse -File | Measure-Object Length -Sum).Sum
Write-Host ("Build folder: {0:N0} bytes" -f $buildSize)
Write-Host ("ZIP total:    {0:N0} bytes ({1:P1} of 5,000,000)" -f $size, ($size / 5000000))
