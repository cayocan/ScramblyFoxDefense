# Uso: .\tools\resume-feature.ps1   (religa o relogio de tempo ativo da feature atual)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\feature-time.ps1"
$branch = Get-FeatureBranch
if (-not (git config "branch.$branch.activeSeconds")) { throw "Sem controle de tempo para '$branch'." }
if (-not (git config "branch.$branch.resumedAt")) { git config "branch.$branch.resumedAt" (Get-Now) }
Write-Host "Relogio ligado em '$branch'. Tempo ativo acumulado: $(Format-Duration (Get-ActiveSeconds $branch))."
