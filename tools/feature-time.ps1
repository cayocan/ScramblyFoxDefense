# Funcoes compartilhadas de controle de TEMPO ATIVO por feature.
# Estado fica em git config local: branch.<nome>.activeSeconds e branch.<nome>.resumedAt (vazio = pausado).

function Get-Now { [long][DateTimeOffset]::Now.ToUnixTimeSeconds() }

function Get-FeatureBranch {
    $b = git rev-parse --abbrev-ref HEAD
    if ($b -notlike 'feature/*') { throw "Voce esta em '$b'. Use dentro de uma branch feature/*." }
    return $b
}

function Get-ActiveSeconds([string]$Branch) {
    $acc = git config "branch.$Branch.activeSeconds"
    if (-not $acc) { throw "Sem controle de tempo para '$Branch'. Crie com tools\start-feature.ps1." }
    $total = [long]$acc
    $resumed = git config "branch.$Branch.resumedAt"
    if ($resumed) { $total += (Get-Now) - [long]$resumed }
    return $total
}

function Format-Duration([long]$Seconds) {
    '{0}h {1:00}min' -f [int][math]::Floor($Seconds / 3600), [int][math]::Floor(($Seconds % 3600) / 60)
}

function Stop-Clock([string]$Branch) {
    $resumed = git config "branch.$Branch.resumedAt"
    if (-not $resumed) { return }
    $acc = [long](git config "branch.$Branch.activeSeconds")
    git config "branch.$Branch.activeSeconds" ($acc + (Get-Now) - [long]$resumed)
    git config --unset "branch.$Branch.resumedAt"
}
