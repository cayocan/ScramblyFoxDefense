# Scrambly Fox Defense

Playable web (Unity 6000.3.19f1, C#, WebGL) para o take-home da Simula, anunciante Scrambly. Limite de 6 h de trabalho.

## Language rule (mandatory)
Everything in the project MUST be written in English: code, comments, commit messages, branch names, UI text, documentation, variable names, logs, error messages — absolutely everything. No exceptions.
Tower defense enxuto de 45–60 s: a raposa lidera Cube Pets contra predadores (leão, tigre, urso-polar) que roubam moedas demo. Fluxo Discover → Play → Redeem, termina com o CTA "Explore Scrambly" (simulado).

## Fluxo de trabalho (obrigatório)
- Cada feature vive em `feature/<nome>`. Crie com `.\tools\start-feature.ps1 <nome>`; isso registra a hora de início.
- Toda feature termina em um **commit de merge** na `main` que informa a duração (do início ao fim, sem pausas): `.\tools\finish-feature.ps1 "resumo"` (merge `--no-ff` com "Duracao da feature (inicio ate o fim): Xh YYmin").
- Nunca faça merge de feature sem esse commit. Nunca commite direto na `main`, exceto o setup inicial.
- Registre também em `TIME_LOG.md` (a entrega pede tempo gasto e decisões de IA).
- Os scripts usam só ASCII nas mensagens, para evitar texto corrompido no PowerShell 5.
- O Unity gera arquivos `.meta` ao importar assets; versione-os junto com o asset.

## Restrições do briefing (não negociáveis)
- ZIP de produção ≤ 5.000.000 bytes, com `index.html` na raiz, build + código legível + assets + instruções.
- Roda em servidor HTTP estático, sem requisições externas, login ou backend. Sem Decompression headers: usar Decompression Fallback.
- Toque e mouse. Testar 320×568 e 390×844 (retrato) com prompt de rotação em paisagem. Sem scroll de página.
- CTA com rótulo claro; ao clicar mostra "CTA clicked — demo only", loga no console e não navega.
- Pausar jogo e relógios quando a página fica oculta e retomar sem salto de tempo (usar só tempo escalado).
- Restart reseta tudo sem timers/listeners/efeitos duplicados.
- Saldo é demo: nunca prometer ganho real nem pagamento.
- Áudio: ainda indefinido (hoje, sem áudio). Se entrar, exige início após interação, mute e silêncio com a página oculta.
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

## Unity editor automation (unattended / remote sessions)
- Drive the open editor with the Unity CLI + Pipeline: `unity --no-banner command <name> ...` (`recompile`, `recompile_status`, `menu --path "Scrambly/Build Main Scene"`, `build --target WebGL --outputPath Builds/WebGL --confirm true` then poll `build_status`, `eval`/`eval_file`, `editor_play`/`editor_stop`, `capture_game_view`).
- Compile check: trust `recompile_status` ("completed" with no errors) plus a type lookup via `eval`; `console_status.compilationFailed` can lag.
- If commands time out ("Main thread operation timed out"), the editor is either throttled in the background or blocked by a modal dialog. Use `tools/unity-window.ps1` (Win32, works when the Pipeline is stuck): `status` (lists dialogs and buttons), `click "<button>"`, `focus`, `shot <png>`.
- During long or unattended sessions run `tools/unity-window.ps1 watch keepfocus` in the background: it auto-answers known-safe dialogs (scene modified externally → Reload), logs any other dialog to `Logs/unity-window-watch.log` as NEEDS DECISION, keeps Unity in front every 30 s and keeps the PC awake. Never auto-click unknown dialogs: read them with `status`/`shot` and decide.
- Play Mode does not advance frames while Unity is in the background; for logic checks use `Assets/Editor/SessionSimulator.cs` (ticks the real services) and `tools/browser-test` (headless Chrome against `python -m http.server 8080` in `Builds/WebGL`).

## Contexto adicional
Coloque documentos de contexto (GDD, notas, referências) em `docs/rag/`. Leia `docs/rag/README.md` antes de decidir escopo.

# context-mode — MANDATORY routing rules

You have context-mode MCP tools available. These rules are NOT optional — they protect your context window from flooding. A single unrouted command can dump 56 KB into context and waste the entire session.

## BLOCKED commands — do NOT attempt these

### curl / wget — BLOCKED
Any Bash command containing `curl` or `wget` is intercepted and replaced with an error message. Do NOT retry.
Instead use:
- `ctx_fetch_and_index(url, source)` to fetch and index web pages
- `ctx_execute(language: "javascript", code: "const r = await fetch(...)")` to run HTTP calls in sandbox

### Inline HTTP — BLOCKED
Any Bash command containing `fetch('http`, `requests.get(`, `requests.post(`, `http.get(`, or `http.request(` is intercepted and replaced with an error message. Do NOT retry with Bash.
Instead use:
- `ctx_execute(language, code)` to run HTTP calls in sandbox — only stdout enters context

### WebFetch — BLOCKED
WebFetch calls are denied entirely. The URL is extracted and you are told to use `ctx_fetch_and_index` instead.
Instead use:
- `ctx_fetch_and_index(url, source)` then `ctx_search(queries)` to query the indexed content

## REDIRECTED tools — use sandbox equivalents

### Bash (>20 lines output)
Bash is ONLY for: `git`, `mkdir`, `rm`, `mv`, `cd`, `ls`, `npm install`, `pip install`, and other short-output commands.
For everything else, use:
- `ctx_batch_execute(commands, queries)` — run multiple commands + search in ONE call
- `ctx_execute(language: "shell", code: "...")` — run in sandbox, only stdout enters context

### Read (for analysis)
If you are reading a file to **Edit** it → Read is correct (Edit needs content in context).
If you are reading to **analyze, explore, or summarize** → use `ctx_execute_file(path, language, code)` instead. Only your printed summary enters context. The raw file content stays in the sandbox.

### Grep (large results)
Grep results can flood context. Use `ctx_execute(language: "shell", code: "grep ...")` to run searches in sandbox. Only your printed summary enters context.

## Tool selection hierarchy

1. **GATHER**: `ctx_batch_execute(commands, queries)` — Primary tool. Runs all commands, auto-indexes output, returns search results. ONE call replaces 30+ individual calls.
2. **FOLLOW-UP**: `ctx_search(queries: ["q1", "q2", ...])` — Query indexed content. Pass ALL questions as array in ONE call.
3. **PROCESSING**: `ctx_execute(language, code)` | `ctx_execute_file(path, language, code)` — Sandbox execution. Only stdout enters context.
4. **WEB**: `ctx_fetch_and_index(url, source)` then `ctx_search(queries)` — Fetch, chunk, index, query. Raw HTML never enters context.
5. **INDEX**: `ctx_index(content, source)` — Store content in FTS5 knowledge base for later search.

## Subagent routing

When spawning subagents (Agent/Task tool), the routing block is automatically injected into their prompt. Bash-type subagents are upgraded to general-purpose so they have access to MCP tools. You do NOT need to manually instruct subagents about context-mode.

## Output constraints

- Keep responses under 500 words.
- Write artifacts (code, configs, PRDs) to FILES — never return them as inline text. Return only: file path + 1-line description.
- When indexing content, use descriptive source labels so others can `ctx_search(source: "label")` later.

## ctx commands

| Command | Action |
|---------|--------|
| `ctx stats` | Call the `ctx_stats` MCP tool and display the full output verbatim |
| `ctx doctor` | Call the `ctx_doctor` MCP tool, run the returned shell command, display as checklist |
| `ctx upgrade` | Call the `ctx_upgrade` MCP tool, run the returned shell command, display as checklist |
