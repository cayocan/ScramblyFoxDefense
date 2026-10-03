# Uso: .\tools\start-feature.ps1 nome-da-feature
# Cria a branch feature/<nome> a partir da main e registra a hora de início.
param([Parameter(Mandatory = $true)][string]$Name)

$ErrorActionPreference = 'Stop'
$branch = "feature/$Name"

if (git status --porcelain) { throw 'Há mudanças não commitadas. Faça commit ou stash antes de iniciar a feature.' }

git checkout main
git checkout -b $branch
git config "branch.$branch.startedAt" ([DateTimeOffset]::Now.ToUnixTimeSeconds())
Write-Host "Feature '$branch' iniciada em $(Get-Date -Format 'dd/MM/yyyy HH:mm')."
