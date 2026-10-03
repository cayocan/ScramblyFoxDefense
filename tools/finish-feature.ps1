# Uso: .\tools\finish-feature.ps1 "Resumo curto do que foi feito"
# Faz o merge --no-ff da branch atual na main, com a duração da feature no commit de merge.
param([Parameter(Mandatory = $true)][string]$Summary)

$ErrorActionPreference = 'Stop'
$branch = git rev-parse --abbrev-ref HEAD
if ($branch -notlike 'feature/*') { throw "Você está em '$branch'. Rode este script dentro de uma branch feature/*." }
if (git status --porcelain) { throw 'Há mudanças não commitadas. Faça commit antes de finalizar a feature.' }

$started = git config "branch.$branch.startedAt"
if (-not $started) { throw "Sem hora de início para '$branch'. Crie a branch com tools\start-feature.ps1." }

$elapsed = [DateTimeOffset]::Now - [DateTimeOffset]::FromUnixTimeSeconds([long]$started)
$duration = '{0}h {1:00}min' -f [int][math]::Floor($elapsed.TotalHours), $elapsed.Minutes

git checkout main
git merge --no-ff $branch -m "Merge $branch`: $Summary" -m "Duração da feature: $duration"
git config --unset "branch.$branch.startedAt"
Write-Host "Feature '$branch' finalizada. Duração: $duration."
