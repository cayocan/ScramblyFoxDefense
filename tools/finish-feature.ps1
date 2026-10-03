# Uso: .\tools\finish-feature.ps1 "Resumo curto do que foi feito"
# Merge --no-ff da feature atual na main, com a duracao (inicio ate o fim) no commit de merge.
param([Parameter(Mandatory = $true)][string]$Summary)
$ErrorActionPreference = 'Stop'

$branch = git rev-parse --abbrev-ref HEAD
if ($branch -notlike 'feature/*') { throw "Voce esta em '$branch'. Rode dentro de uma branch feature/*." }
if (git status --porcelain --untracked-files=no) { throw 'Ha mudancas nao commitadas. Faca commit antes de finalizar a feature.' }

$started = git config "branch.$branch.startedAt"
if (-not $started) { throw "Sem hora de inicio para '$branch'. Crie a branch com tools\start-feature.ps1." }

$elapsed = [DateTimeOffset]::Now - [DateTimeOffset]::FromUnixTimeSeconds([long]$started)
$duration = '{0}h {1:00}min' -f [int][math]::Floor($elapsed.TotalHours), $elapsed.Minutes

git checkout main
git merge --no-ff $branch -m "Merge ${branch}: $Summary" -m "Duracao da feature (inicio ate o fim): $duration"
git config --unset "branch.$branch.startedAt"
Write-Host "Feature '$branch' finalizada. Duracao: $duration. Registre em TIME_LOG.md."
