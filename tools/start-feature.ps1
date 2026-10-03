# Uso: .\tools\start-feature.ps1 nome-da-feature
# Cria feature/<nome> a partir da main e liga o relogio de tempo ativo.
param([Parameter(Mandatory = $true)][string]$Name)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\feature-time.ps1"

$branch = "feature/$Name"
if (git status --porcelain --untracked-files=no) { throw 'Ha mudancas nao commitadas. Faca commit antes de iniciar a feature.' }

git checkout main
git checkout -b $branch
git config "branch.$branch.activeSeconds" 0
git config "branch.$branch.resumedAt" (Get-Now)
Write-Host "Feature '$branch' iniciada. Relogio ligado. Use pause-feature.ps1 ao sair."
