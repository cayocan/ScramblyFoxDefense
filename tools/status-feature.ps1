# Uso: .\tools\status-feature.ps1   (mostra tempo ativo e se o relogio esta ligado)
$ErrorActionPreference = 'Stop'
. "$PSScriptRoot\feature-time.ps1"
$branch = Get-FeatureBranch
$state = if (git config "branch.$branch.resumedAt") { 'ligado' } else { 'pausado' }
Write-Host "$branch | relogio $state | tempo ativo: $(Format-Duration (Get-ActiveSeconds $branch))"
