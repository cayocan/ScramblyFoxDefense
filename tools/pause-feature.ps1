# Uso: .\tools\pause-feature.ps1   (pausa o relogio de tempo ativo da feature atual)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\feature-time.ps1"
$branch = Get-FeatureBranch
Stop-Clock $branch
Write-Host "Pausado. Tempo ativo ate agora em '$branch': $(Format-Duration (Get-ActiveSeconds $branch))."
