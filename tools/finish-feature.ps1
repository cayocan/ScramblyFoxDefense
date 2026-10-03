# Uso: .\tools\finish-feature.ps1 "Resumo curto do que foi feito"
# Merge --no-ff da feature atual na main, com o TEMPO ATIVO no commit de merge.
param([Parameter(Mandatory = $true)][string]$Summary)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\feature-time.ps1"

$branch = Get-FeatureBranch
if (git status --porcelain --untracked-files=no) { throw 'Ha mudancas nao commitadas. Faca commit antes de finalizar a feature.' }

$seconds = Get-ActiveSeconds $branch
$duration = Format-Duration $seconds

git checkout main
git merge --no-ff $branch -m "Merge ${branch}: $Summary" -m "Duracao da feature (tempo ativo): $duration"
git config --unset "branch.$branch.activeSeconds"
git config --unset "branch.$branch.resumedAt" 2>$null
Write-Host "Feature '$branch' finalizada. Tempo ativo: $duration. Registre em TIME_LOG.md."
