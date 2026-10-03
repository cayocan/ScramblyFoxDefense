# Scrambly Fox Defense

Playable web (Unity 6000.3.19f1, C#, WebGL) para o take-home da Simula, anunciante Scrambly. Limite de 6 h de trabalho.
Tower defense enxuto de 45–60 s: a raposa lidera Cube Pets contra predadores (leão, tigre, urso-polar) que roubam moedas demo. Fluxo Discover → Play → Redeem, termina com o CTA "Explore Scrambly" (simulado).

## Fluxo de trabalho (obrigatório)
- Cada feature vive em `feature/<nome>`. Crie com `.\tools\start-feature.ps1 <nome>`; isso liga o relógio de **tempo ativo**.
- Toda feature termina em um **commit de merge** na `main` que informa o tempo ativo: `.\tools\finish-feature.ps1 "resumo"` (merge `--no-ff` com "Duracao da feature (tempo ativo): Xh YYmin").
- Conta só o tempo ativo. Quando o usuário avisar que vai sair, parar ou fazer pausa, rode `.\tools\pause-feature.ps1`; ao voltar, `.\tools\resume-feature.ps1`. `.\tools\status-feature.ps1` mostra o acumulado. Antes de finalizar, confirme que o relógio não ficou ligado durante uma pausa.
- Nunca faça merge de feature sem esse commit. Nunca commite direto na `main`, exceto o setup inicial.
- Registre também em `TIME_LOG.md` (a entrega pede tempo gasto e decisões de IA).
- Os scripts usam só ASCII nas mensagens, para evitar texto corrompido no PowerShell 5.

## Restrições do briefing (não negociáveis)
- ZIP de produção ≤ 5.000.000 bytes, com `index.html` na raiz, build + código legível + assets + instruções.
- Roda em servidor HTTP estático, sem requisições externas, login ou backend. Sem Decompression headers: usar Decompression Fallback.
- Toque e mouse. Testar 320×568 e 390×844 (retrato) com prompt de rotação em paisagem. Sem scroll de página.
- CTA com rótulo claro; ao clicar mostra "CTA clicked — demo only", loga no console e não navega.
- Pausar jogo e relógios quando a página fica oculta e retomar sem salto de tempo (usar só tempo escalado).
- Restart reseta tudo sem timers/listeners/efeitos duplicados.
- Saldo é demo: nunca prometer ganho real nem pagamento.
- Sem áudio (portanto sem mute).
- Paleta: Orange #F58324, Purple #7845D8, Deep ink #201338, Warm white #FFF6E8.

## Orçamento de tamanho
Build alvo ≤ 2,8 MB (duro 3,5), código ≤ 0,4 MB, assets brutos ≤ ~1 MB, margem ≥ 0,5 MB. Fase 0: medir um build WebGL vazio nos primeiros 45 min. Se passar de 3,8 MB, plano B: TypeScript + Three.js.

## Assets (Kenney, CC0): Tower Defense Kit, Cube Pets, UI Pack, Game Icons
- Já importados em `Assets/Art/`: Cube Pets (raposa, cachorro, gato, leão, tigre, urso-polar) e Tower Defense Kit (tile, tile-straight, tile-corner-round, tile-spawn, tile-end, tower-round-base, tower-square-bottom-a, selection-a). UI Pack e Game Icons ainda não.
- Os arquivos são `.glb`. O Unity 6 não importa glTF/GLB sozinho: é preciso um importador (por exemplo o pacote glTFast, só no editor) ou converter para FBX. Decidir e medir o efeito no tamanho na Fase 0.
- Cada GLB usa a textura única `colormap.png`; trocar a textura recolore o kit.
- Defensores: `animal-fox`, `animal-dog`, `animal-cat`, cada um **em cima de uma base de torre** (animal visível sobre a torre).
- Inimigos: `animal-lion` (padrão), `animal-tiger` (rápido), `animal-polar` (tanque). Não usar os UFOs do kit.
- Cortes de tamanho, em ordem: gato, detalhes do cenário.
- Os pacotes originais ficam fora do repositório; só os arquivos usados entram em `Assets/`.

## Contexto adicional
Coloque documentos de contexto (GDD, notas, referências) em `docs/rag/`. Leia `docs/rag/README.md` antes de decidir escopo.
