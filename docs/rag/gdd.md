# Scrambly Fox Defense — GDD

Oct 1, 2026 · @Cayo

> Source document kept verbatim (Portuguese) as RAG context. All in-game text, code and project docs are in English.

Tower defense enxuto em Unity/C#, de 45 a 60 segundos e com uma mão só: a raposa lidera Cube Pets contra predadores selvagens (leão, tigre e urso-polar) que roubam moedas, seguindo o fluxo Discover → Play → Redeem do Scrambly, com assets do Kenney recoloridos na paleta laranja–roxo.

## 1. Visão geral

**Pitch:** a raposa do Scrambly lidera um esquadrão de Cube Pets que protege o **Reward Vault** de predadores selvagens que querem roubar as moedas. O jogador escolhe "cartões de jogo" (cada um com um pet, que vira a torre), posiciona e melhora em 3 ondas curtas, junta moedas de demonstração, destrava 3 recompensas e termina com um convite claro: **Explore Scrambly**.

**Pilares de design**

1. **Entendível em 3 segundos.** Uma ação só (tocar), uma mão, sem texto longo. Uma mão-guia ensina os dois primeiros toques.
2. **Progresso que se vê.** A torre cresce por peças, o saldo demo sobe e 3 cadeados de recompensa destravam a cada onda.
3. **O produto é a estrutura.** Cada fase da sessão corresponde a um passo do Scrambly (Discover, Play, Redeem), não é um enfeite sobre um TD genérico.
4. **Promessa honesta.** Todo saldo é "demo". Nenhum valor em dinheiro, nenhuma marca real.

**Público e meta (do briefing):** adultos que curtem jogos casuais de celular e têm curiosidade sobre recompensas por descobrir e jogar. A meta é deixar a ligação entre jogar, progredir e o Scrambly fácil de entender.

**Escopo em uma linha:** 1 mapa, 1 caminho, 4 slots, 3 torres (Cube Pets), 3 predadores (leão, tigre, urso-polar), 3 ondas, 3 recompensas a destravar, 1 final, 1 CTA, 1 restart. Sem áudio, sem tela de derrota, sem menus. **Assets:** só os 4 pacotes do Kenney escolhidos (Tower Defense Kit, Cube Pets, UI Pack, Game Icons, todos CC0); detalhes na seção 6.

**Idioma:** este GDD em português; todo texto dentro do jogo em inglês (o Scrambly opera em US, UK e Canadá).

## 2. Conexão com o Scrambly

O briefing define o produto em três passos. A sessão inteira é organizada nesses mesmos três passos, e cada um tem uma mecânica própria e uma tela que o jogador reconhece.

| Passo do Scrambly | Fase da sessão | O que o jogador faz | O que ele vê |
| --- | --- | --- | --- |
| **Discover** | Escolha dos cartões (0–10 s) | Escolhe entre 3 "cartões de jogo" (cada um com um pet e um ícone de gênero) e posiciona o primeiro | Cartões com rosto de pet; rótulo "Discover" na faixa de fase |
| **Play and progress** | Ondas 1 a 3 (10–45 s) | Constrói e melhora torres, derrota os predadores, junta moedas demo | 3 cadeados no topo; a torre ganha peças e o pet comemora |
| **Redeem** | Reward Vault (45–55 s) | Assiste e toca para acelerar | Moedas voam para o Vault, os cadeados destravam e surgem 3 cartões de recompensa genéricos (troféu, medalha, cesta) |
| **Convite** | Tela final | Toca em **Explore Scrambly** | Botão grande; ao clicar, mensagem local "CTA clicked — demo only" |

**Por que um tower defense encaixa.** Em um TD, "jogar bem" já significa evoluir (mais torres, torres melhores), então o progresso é o próprio jogo. Os cadeados que destravam são a recompensa tangível e a raposa dá o rosto da marca. Predadores que tentam roubar moedas dão um motivo para proteger o saldo demo.

**Regras de promessa honesta (do briefing)**

- Todo saldo aparece como **Demo coins** e a tela final traz a linha "Demo only — not real earnings".
- Nenhum valor em dinheiro, nenhuma frase de ganho garantido, nenhum prazo de saque.
- Cartões de recompensa são ícones genéricos (troféu, medalha, cesta), sem logos de varejistas ou meios de pagamento.
- Os "jogos" dos cartões são fictícios. Nenhum jogo real é citado.
- Não usar números de marketing do site (bônus, média diária, número de usuários), porque o briefing não os fornece e eles poderiam soar como promessa.

## 3. Loop e fluxo da sessão

A sessão dura cerca de 55 segundos e sempre chega ao final, mesmo que o jogador não construa nada. Isso evita jogo travado na revisão.

| Tempo aprox. | Estado | O que acontece | Entrada do jogador |
| --- | --- | --- | --- |
| 0–8 s | **Intro / Discover** | A raposa entra no Reward Vault, 70 demo coins, 3 cartões de jogo (cada um com um pet) na base, mão-guia aponta o primeiro cartão | Tocar um cartão, tocar um slot brilhante |
| ~8–20 s | **Onda 1** | 6 Snatchers (leões) seguem o caminho; os pets atiram; moedas pulam dos predadores | Construir mais torres |
| ~20–23 s | Respiro | O cadeado 1 destrava, faixa "Wave 2"; mão-guia aponta o primeiro upgrade se houver moedas | Melhorar torre (tocar nela) |
| ~23–37 s | **Onda 2** | 5 Snatchers + 4 Darts (tigres) | Construir/melhorar |
| ~37–40 s | Respiro | O cadeado 2 destrava, faixa "Final wave" | Melhorar |
| ~40–54 s | **Onda 3** | 6 Snatchers + 4 Darts + 2 Haulers (ursos-polares) | Construir/melhorar |
| ~54–60 s | **Redeem** | O cadeado 3 destrava, as moedas voam para o Vault, que brilha, e surgem 3 cartões de recompensa genéricos | Opcional: tocar para acelerar |
| fim | **End card** | "Discover games. Play and progress. Redeem rewards." + botão **Explore Scrambly** + **Play again** | Tocar no CTA ou reiniciar |

**Regras do ritmo**

- **A onda 1 começa** quando o primeiro slot é ocupado ou após 8 s de tempo de jogo na intro, o que vier primeiro. Assim a sessão nunca espera o jogador indefinidamente.
- **Cada onda termina** quando todos os inimigos gerados morreram ou chegaram ao Vault. Não há contagem regressiva, então não existe caso de "tempo esgotado".
- **Inatividade:** se o jogador ficar 5 s sem tocar durante um respiro, a mão-guia reaparece apontando a melhor ação.
- **Tempo do jogo = tempo escalado** (`Time.deltaTime` com `timeScale`). Isso permite pausar tudo de uma vez quando a página fica oculta.

## 4. Mecânicas e controles

Uma única primitiva de entrada: **toque (ou clique) em um ponto**. Nada de arrastar, segurar, hover ou gestos, para funcionar igual em touch e mouse.

**Construir (Discover)**

1. Tocar em um cartão de jogo na base seleciona a torre. Os slots livres pulsam em roxo.
2. Tocar em um slot pulsante constrói a torre (animação de "pop") e desconta as moedas.
3. Tocar de novo no cartão, em área vazia ou perder o ponteiro cancela a seleção.
4. Sem moedas suficientes: o cartão treme e o preço pisca. Nada é construído.

**Melhorar (Play and progress)**

- Tocar em uma torre sobe um nível (níveis 1 a 3), se houver moedas. O custo aparece em um selo com o ícone de chave inglesa acima da torre; sem saldo, o selo fica cinza.
- A torre ganha uma peça modular nova (mais alta) e um anel, e o pet comemora com confete curto. É o feedback principal de progresso.
- Vender e mover torres ficam **fora do escopo**.

**Mão-guia (tutorial sem texto)**

1. Passo 1: aponta o primeiro cartão.
2. Passo 2: aponta o slot mais próximo do início do caminho.
3. Passo 3: na primeira vez que o saldo cobrir um upgrade, aponta a torre. Depois disso nunca mais aparece, exceto no aviso de inatividade.

**Sem tela de derrota.** Um predador que chega ao Vault leva 5 demo coins (nunca abaixo de zero) e foge do mapa. O final é sempre o mesmo caminho até o Redeem; só a frase de abertura da tela final muda ("Perfect defense!" sem vazamentos, "Nice defense!" com vazamentos). Isso simplifica estados, reduz casos extremos e mantém o tom acolhedor do briefing.

**Feedback de resposta (clareza e sensação)**

- Toque aceito: cartão e slot reagem em até 1 frame (escala + brilho).
- Acerto: flash branco curto no predador. Morte: estouro de partículas e moeda que salta até o contador.
- Onda: faixa animada no topo. Marco: o cadeado destrava com pulso e brilho.
- Alvos de toque com no mínimo 48×48 px CSS; seleção de slot por raio de 44 px em volta do slot.

**Entrada interrompida:** apenas o primeiro ponteiro conta. Cancelamento de ponteiro, perda de foco ou saída da janela descartam a seleção atual sem construir nada.

## 5. Conteúdo e balanceamento inicial

Valores de partida, todos em um `GameConfig` (ScriptableObject) para ajustar sem mexer em código. Reserve cerca de 1 h de playtest para afinar.

**Torres ("Pet Posts": cada uma é um cartão de jogo fictício, com um Cube Pet como artilheiro)**

| Torre / cartão | Pet | Papel | Custo | Dano | Cadência (tiros/s) | Alcance (u) | Nível 2 / Nível 3 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| **Pop Blaster** | Cão (citado nas tags do pacote) | Tiro único rápido | 30 | 4 | 2,0 | 3,2 | custo 40: dano 6, 2,4/s · custo 60: dano 9, 2,8/s |
| **Puzzle Pulse** | Gato (citado nas tags do pacote) | Dano em área | 45 | 5 (raio 1,4) | 0,9 | 3,0 | custo 50: dano 8 · custo 70: dano 12, raio 1,8 |
| **Racer Zap** | Qualquer outro pet do pacote | Tiro longo e forte | 55 | 14 | 0,6 | 4,5 | custo 60: dano 20 · custo 80: dano 28, 0,7/s |

**Inimigos (predadores do Cube Pets: leão, tigre e urso-polar, recoloridos em roxo)**

| Inimigo | Vida | Velocidade (u/s) | Moedas ao morrer | Função | Visual (variante do kit a definir ao abrir o pacote) |
| --- | --- | --- | --- | --- | --- |
| Snatcher | 10 | 1,6 | 6 | Base | Leão, animal-lion (padrão) |
| Dart | 6 | 2,8 | 7 | Rápido, testa alcance e cadência | Tigre, animal-tiger (rápido e frágil) |
| Hauler | 40 | 1,0 | 15 | Pesado, recompensa upgrade | Urso-polar, animal-polar (tanque lento, escala maior) |

**Ondas**

| Onda | Composição | Intervalo entre inimigos | Vida total |
| --- | --- | --- | --- |
| 1 | 6 Snatchers | 1,2 s | 60 |
| 2 | 5 Snatchers + 4 Darts | 0,9 s | 74 |
| 3 | 6 Snatchers + 4 Darts + 2 Haulers | 0,8 s | 164 |

**Economia**

- Saldo inicial: **70 demo coins** (cobre uma torre e sobra para quase uma segunda).
- Quatro slots no mapa. Construir tudo e melhorar tudo não cabe no orçamento, então há escolhas reais.
- Dois contadores: **Wallet** (gastável, no HUD) e **Collected** (soma de tudo que caiu, usado nas moedas que voam para o Vault e na tela final).
- Vazamento: −5 na Wallet por predador que chega ao Vault, mínimo 0.
- Curva pretendida: uma torre nível 1 quase limpa a onda 1; a onda 3 exige duas ou três torres, ou upgrades.
- Caminho de cerca de 20 u, então um Snatcher leva cerca de 12 s para atravessar.

## 6. Tema, direção de arte e UI

O briefing pede um tratamento **quente, expressivo e acolhedor**, com formas arredondadas e contraste laranja–roxo. A raposa e as cores fornecidas são ponto de partida, não layout obrigatório. Eu ainda não vi o kit de referência (`Scrambly-Assessment-Reference-Kit-v3.zip`, 20,1 KiB), então os detalhes da raposa dependem dele.

**Tema escolhido para casar com os 4 pacotes.** A raposa do Scrambly lidera um esquadrão de **Cube Pets** que protege o **Reward Vault** de **predadores selvagens**, que invadem o Cantinho dos Bichos para roubar as moedas e fugir com elas. Nada agressivo: **predadores desajeitados e fofos (leão, tigre e urso-polar do Cube Pets), sem sangue nem mordida. Laranja = o seu lado (raposa, pets, Vault), roxo = os predadores**. É o contraste que o briefing sugere, lido sem texto.

**Mapa de assets (os 4 pacotes escolhidos, todos CC0)**

| Pacote | Uso no jogo | O que as páginas confirmam | O que conferir ao abrir o ZIP |
| --- | --- | --- | --- |
| [Tower Defense Kit](https://kenney.nl/assets/tower-defense-kit) | Tabuleiro e caminho (tiles), base das torres (peças modulares), arma como origem do tiro, **predadores do Cube Pets como inimigos (o kit não fornece inimigos animais)**, peças de castelo para o Vault | 160 objetos; tiles, peças modulares de torre e armas (os UFOs do kit não são usados); FBX, OBJ e glTF | Verificado: o kit tem 4 UFOs (não usados), feixe, marcadores de seleção e cristais; todos os modelos usam uma textura de paleta compartilhada (colormap.png). Não há peça de portão ou baú |
| [Cube Pets](https://kenney.nl/assets/cube-pets) | 3 **artilheiros** (um por torre) e o rosto de cada cartão de jogo | Versão 2.0, 24 arquivos, modelos 3D **animados**; as tags citam cão e gato | Lista completa de pets (há raposa? se houver, pode ser a raposa 3D do jogo), nomes dos clipes (idle, ataque ou dança), formato, peso |
| [UI Pack](https://kenney.nl/assets/ui-pack) | Cartões, painéis, barra de progresso, botões **Explore Scrambly** e **Play again** | 430 assets: botões, painéis, sliders | Se o estilo é arredondado e tingível pela paleta, se há estrelas ou moedas |
| [Game Icons](https://kenney.nl/assets/game-icons) | Cadeados das 3 recompensas, troféu e medalha nos cartões de recompensa, chave inglesa para upgrade, checkmark, play | Ícones monocromáticos. A lista abaixo vem de uma cópia de terceiros do pacote ([Korge](https://store.korge.org/gfx/kenney_game_icons/)): cadeado e destrava, checkmark, troféu, medalha, cesta, chave inglesa, setas, play, pause, alvo, engrenagem | A contagem diverge (105 na página do Kenney, "125+" na cópia): conferir no ZIP oficial. Não vi moeda, presente nem estrela: **desenhar como formas simples** |

**Recolor com uma textura só.** Os kits do Kenney costumam usar uma pequena textura de paleta compartilhada. Se for o caso, trocar essa única textura por outra com as cores do briefing recolore o kit inteiro sem retexturizar nada. Se não for o caso, sobrescrever os materiais por cor chapada. Confirmar na Fase 0.

**Paleta (do briefing) e uso**

| Cor | Hex | Uso no jogo |
| --- | --- | --- |
| Orange | `#F58324` | Raposa, torres, Vault, caminho (versão mais clara), botão do CTA, moedas |
| Purple | `#7845D8` | Predadores, anéis dos slots, destaques de UI secundários |
| Deep ink | `#201338` | Fundo fora do tabuleiro, texto sobre claro, detalhes das torres |
| Warm white | `#FFF6E8` | Chão do tabuleiro, painéis, olhos e detalhes dos predadores, texto sobre escuro |

**Peças do jogo**

- **Pet Post (torre):** nível 1 = base modular do kit; nível 2 = mais uma peça; nível 3 = mais uma peça e arma maior. O pet fica na plataforma do topo, em idle, e toca o clipe de ataque (ou dança) quando atira. Se os clipes não tiverem nomes úteis, o pet recebe squash and stretch por código.
- **Predador:** tint roxo no Cube Pets (animações walk e run). Morrer = gesture-negative, estouro de partículas e moeda que salta. Vazar = o predador foge com a moeda sobre a cabeça e a Wallet cai 5.
- **Reward Vault:** montado com peças de torre ou castelo do kit em laranja e creme, com a raposa em cima. O kit pode não ter baú, então a recompensa **não depende de animar uma tampa**: o Vault brilha, os cadeados destravam e os cartões de recompensa surgem.
- **Moedas e gemas:** primitivas do Unity (cilindro achatado), sem custo de tamanho.
- **Raposa:** é a mascote e o rosto do final. Usar a do kit de referência: se for arte 2D, aparece como sprite ou billboard no Vault, no HUD e na tela final, animada por tween; se for modelo, versão low-poly simples. Não inventar outra mascote.

**Fonte:** Fredoka (SIL OFL 1.1, [licença no repositório do Google Fonts](https://github.com/google/fonts/blob/main/ofl/fredoka/OFL.txt)), empacotada no projeto, sem requisição externa, só os caracteres necessários.

**Câmera e layout (retrato, referência 390×844):**

- Câmera fixa em perspectiva leve (cerca de 55° de inclinação), tabuleiro ocupando a largura.
- **Faixa superior** (~90 px): **3 cadeados** de recompensa que destravam a cada onda, saldo **Demo coins**, botão de restart pequeno.
- **Meio:** tabuleiro com caminho em S, de cima para baixo. Spawn dos predadores no topo, Reward Vault com a raposa embaixo.
- **Base** (~150 px, respeitando safe area): 3 cartões de jogo, cada um com o rosto de um pet e um ícone de gênero.
- Em celular deitado: aviso "Rotate your phone" e jogo pausado. Em janela larga de desktop: o jogo fica em coluna retrato centralizada, com fundo Deep ink dos lados.

**Textos na tela (em inglês)**

| Onde | Texto |
| --- | --- |
| Faixa de fase | Discover · Play · Redeem |
| Saldo | Demo coins |
| Faixas de onda | Wave 1 · Wave 2 · Final wave! |
| Cartões | Pop Blaster · Puzzle Pulse · Racer Zap |
| Abertura do Vault | Demo rewards unlocked! |
| Título final | Perfect defense! / Nice defense! |
| Frase final | Discover games. Play and progress. Redeem rewards. |
| CTA | Explore Scrambly |
| Secundário | Play again |
| Rodapé | Demo only — not real earnings |
| Confirmação do CTA | CTA clicked — demo only |

Acessibilidade básica: texto Deep ink sobre Warm white (contraste alto), significado nunca só pela cor (cada cartão tem pet, ícone e forma própria; cada recompensa tem cadeado).

## 7. Arquitetura técnica (Unity/C#, WebGL)

Uma cena (`Main`), poucos scripts, nenhuma dependência de pacote que não seja necessária. Cada pacote a mais custa megabytes do orçamento (seção 8).

**Módulos (scripts)**

| Script | Responsabilidade |
| --- | --- |
| `GameConfig` (ScriptableObject) | Todos os números das seções 3 e 5, nada fixo no código |
| `GameFlow` | Máquina de estados: Intro → Wave → Breather → Redeem → EndCard |
| `WaveSpawner` | Gera inimigos por onda, avisa quando a onda termina |
| `EnemyAgent` | Segue os pontos do caminho, vida, morte, vazamento |
| `Tower` | Escolhe alvo, atira, níveis 1–3 (cada nível acrescenta uma peça modular) e comanda o pet que anima o disparo |
| `Projectile` | Projétil simples, vindo de um pool |
| `Economy` | Wallet e Collected, custos, eventos de mudança |
| `SlotManager` | Slots, estado livre/ocupado, seleção por distância em tela |
| `InputRouter` | Um ponteiro por vez, cancelamento, filtro de toque duplicado |
| `HudView` / `TutorialHand` | Barra de progresso, saldo, cartões, mão-guia |
| `RedeemSequence` / `EndCard` | Moedas voando, Vault, tela final, CTA e restart |
| `PageVisibility` (+ `.jslib`) | Recebe "página oculta/visível" do navegador e pausa o jogo |
| `ObjectPool` | Inimigos, projéteis, moedas, partículas: nada de `Instantiate` durante a onda |

**Entrada.** Botões de UI usam o `EventSystem`, que já trata mouse e toque. Escolha de slot e de torre usa **distância em tela** (projetar a posição do slot e comparar com o ponteiro), sem Physics, o que dispensa o módulo de física no build. Só o primeiro ponteiro é aceito, e `pointer cancel` descarta a seleção.

**Tempo e pausa.** Todo movimento, spawn e animação usa tempo escalado (`Time.deltaTime`), e tweens são coroutines próprias, sem biblioteca de tween. O template HTML registra **uma vez** o `visibilitychange` e avisa o jogo; o jogo põe `Time.timeScale = 0` ao ocultar e volta a 1 ao exibir. Além disso, `Time.maximumDeltaTime = 0.1` para que nunca haja salto de tempo na volta.

**Restart.** Recarregar a cena (`SceneManager.LoadScene`) com o botão desabilitado durante a recarga. Regras para não duplicar nada:

- sem campos `static` com estado (ou resetados no `Awake`);
- todo evento assinado em `OnEnable` é cancelado em `OnDisable`;
- `timeScale` volta a 1 no início da cena;
- os listeners de JavaScript ficam no `index.html`, que não recarrega, então não se acumulam.

**Resize e layout.** Canvas Scaler em "Scale With Screen Size" (referência 390×844). A câmera recalcula o enquadramento quando a proporção muda. O template coloca o canvas em um contêiner retrato centralizado (largura = `min(100vw, 100dvh × 0,5625)`), com fundo Deep ink.

**Template WebGL (`index.html` próprio)**

- `html, body { overflow: hidden; overscroll-behavior: none; touch-action: none; }` e `viewport` com `user-scalable=no, viewport-fit=cover`.
- `touchmove` com `preventDefault` no canvas e menu de contexto desabilitado, para a página não rolar durante o jogo.
- Aviso "Rotate your phone" quando o ponteiro é grosso (celular) e a tela está deitada.
- Sem logos, fontes ou scripts de CDN: **nenhuma requisição externa**.

**CTA.** Botão no `EndCard`. No clique: mensagem na tela "CTA clicked — demo only" e `Debug.Log` (que o Unity WebGL envia ao console do navegador). Nenhuma chamada a `Application.OpenURL` nem navegação.

**Áudio.** Não haverá. Pelo briefing, sem áudio não precisa de botão de mudo, e o módulo de áudio pode ser desabilitado para economizar tamanho.

**Configurações de build para tamanho (verificar cada uma no teste da seção 8)**

- Pipeline simples (Built-in RP com shaders Unlit/Standard simples; URP tende a aumentar o build, então comparar).
- IL2CPP, Managed Stripping Level alto, Engine Code Stripping ligado, exceções desligadas.
- Compressão com **Decompression Fallback** ligado, porque um servidor estático simples não envia os cabeçalhos de compressão.
- Módulos nativos que o jogo não usa (física, áudio, vídeo, etc.) desativados no Package Manager.
- Texturas ≤ 512 px, malhas low-poly, sem mipmaps onde não precisa.

## 8. Orçamento de 5 MB e teste de tamanho

O briefing limita o **ZIP inteiro** a 5.000.000 bytes, e esse ZIP precisa conter o build, o código-fonte legível, bibliotecas, fontes, assets, instruções e créditos. O risco real de Unity WebGL é o tamanho, não a jogabilidade. Os números abaixo são minhas metas de trabalho, **não medições**; o teste decide.

| Parte do ZIP | Meta | Limite |
| --- | --- | --- |
| Build WebGL (arquivos já comprimidos) | ≤ 2,8 MB | 3,5 MB |
| Código-fonte (scripts, cena, prefabs, template, ajustes essenciais) | ≤ 0,3 MB | 0,4 MB |
| Assets originais incluídos como fonte (malhas, texturas, fonte, raposa) | ≤ 0,5 MB | 0,6 MB |
| README, CREDITS, nota do projeto, registro de testes | ≤ 0,05 MB | 0,1 MB |
| **Folga** | ≥ 0,5 MB |  |

Dois cuidados: arquivos `.br`/`.gz` do build não encolhem de novo no ZIP, e os assets aparecem **duas vezes** (empacotados no build e crus na pasta de fonte), então cada 100 KB de asset custa cerca de 200 KB.

**Cube Pets pesam mais que o resto.** Eles são animados (malha com esqueleto e clipes), ao contrário das peças estáticas do kit. Importar só 3 pets e só os clipes usados (idle e ataque ou dança), e medir a Fase 0 com um pet animado dentro, não só com um cubo.

**Fase 0 — teste de tamanho (primeiros 30 minutos, antes de qualquer arte)**

1. Projeto com uma cena: câmera, 1 torre do kit, 1 Cube Pet animado, 1 sprite (a raposa), 1 botão de UI, 1 texto, tudo com as configurações da seção 7.
2. Build WebGL, zipar junto com a pasta de fonte e medir em bytes.
3. Servir com `python3 -m http.server` (o servidor simples do briefing, sem cabeçalhos especiais) e abrir no navegador. Conferir na aba Network que **só há requisições do mesmo domínio**.
4. Medir também o tempo até a primeira interação em um celular, se tiver um.

**Regra de decisão (às 0:45 no máximo)**

| Resultado do teste | Decisão |
| --- | --- |
| ZIP ≤ 3,0 MB | Seguir em Unity, com orçamento folgado |
| 3,0 a 3,8 MB | Seguir em Unity cortando: sem TextMeshPro (fonte própria mais simples), sem URP, menos malhas |
| > 3,8 MB, ou não abre em servidor estático | **Plano B:** mesmo GDD em TypeScript + Three.js (ou Canvas 2D), um bundle com Vite. A tabela de módulos da seção 7 vira módulos TS quase um para um |

O sample de playable ads da Unity só vale se passar nesse mesmo teste e gerar `index.html` na raiz. Qualquer código reaproveitado precisa constar em `CREDITS.md`.

## 9. Requisitos do briefing e como cada um é atendido

Cada linha vem do texto do briefing (Technical requirements e Stage 1). A coluna da direita é o que você deve conseguir mostrar na gravação.

| Requisito do briefing | Como atendemos | Como verificar |
| --- | --- | --- |
| **Production ZIP** com `index.html` na raiz, build completo, código legível, bibliotecas, fontes, assets, instruções | Estrutura da seção 12; template WebGL próprio gera `index.html` na raiz | Abrir o ZIP em outra pasta e rodar do zero |
| **Tamanho** ≤ 5.000.000 bytes | Orçamento e Fase 0 (seção 8) | Medir o ZIP final em bytes, anotar no README |
| **Runtime**: servidor estático simples, sem requisições externas, login, backend ou chaves | Decompression Fallback; nenhuma fonte, script ou imagem de CDN | `python3 -m http.server`; aba Network só com mesmo domínio |
| **Layout**: toque e mouse; retrato 320×568 e 390×844; sem rolagem competindo | Canvas Scaler 390×844, CSS sem rolagem, `preventDefault` no toque; aviso de rotação em celular deitado | Emular os dois tamanhos e testar em mouse e toque |
| **CTA**: confirmação local e log no console, sem sair da página | Mensagem "CTA clicked — demo only" + `Debug.Log`; sem `OpenURL` | Clicar e abrir o console; confirmar que a URL não muda |
| **Visibilidade**: pausar jogo e relógios com a página oculta; voltar sem salto | `visibilitychange` → `timeScale = 0`; tempo escalado em tudo; `maximumDeltaTime` 0,1 | Ocultar a aba no meio de uma onda, esperar 10 s, voltar |
| **Confiabilidade**: resize, entrada interrompida, desfechos do design, restart limpo | Seções 4 e 7; recarga de cena sem estado estático | Matriz de testes (seção 11) |
| **Handoff**: instruções, navegadores e dispositivos testados (real ou emulado), limitações, o que não foi testado | `README.md` com tabela de testes | Preencher durante os testes, não no fim |
| **Interação entendível e responsiva** | Uma primitiva de toque, mão-guia, feedback em 1 frame | Alguém que nunca viu joga sem explicação |
| **Progressão com propósito e fim claro** | Três ondas, três cadeados, Redeem, end card | Jogar do início ao fim sem tocar em nada: ainda chega ao fim |
| **Conexão significativa com o Scrambly** | Seção 2 | Dizer em uma frase como cada fase mapeia um passo do produto |
| **Restart fácil de revisar** | Botão pequeno sempre visível no topo, mais "Play again" na tela final | Reiniciar 10 vezes seguidas, inclusive no meio de uma onda |

**Requisitos condicionais (o briefing diz que só valem se o design os usa):** sem áudio, **não há botão de mudo**; sem contagem regressiva, **não há caso de tempo esgotado**. Os relógios de spawn e de onda ainda são pausados pela regra de visibilidade.

## 10. Plano de 6 horas, pontos de decisão e cortes

O limite é de 6 horas de trabalho. Tempos abaixo contam do início do projeto, com a gravação de tela ligada desde o minuto 0 (o briefing quer ver desde a interpretação do briefing).

| Janela | Entrega | Ponto de decisão |
| --- | --- | --- |
| 0:00–0:45 | Fase 0: teste de tamanho (seção 8) com raposa, 1 torre, 1 pet animado, UI e build em servidor estático | **G0:** ZIP dentro da meta? Se não, Plano B (TS + Three.js) já |
| 0:45–1:45 | Núcleo em cinza: tabuleiro, caminho, leão andando, slots, 1 torre atirando, moedas | Toque funciona em mouse e em toque emulado |
| 1:45–2:45 | 3 torres, upgrades, 3 ondas, economia, barra de progresso, vazamento | **G1:** do início ao fim, jogável em cinza |
| 2:45–3:45 | Arte: materiais da paleta, raposa, UI final, partículas, feedback de acerto e de upgrade | Legível em 320×568? |
| 3:45–4:30 | Redeem, end card, CTA, restart, mão-guia | Fluxo completo em uma jogada |
| 4:30–5:30 | Endurecimento: visibilidade, resize, entrada interrompida, testes 320×568 e 390×844, ZIP final medido | Matriz de testes verde |
| 5:30–6:00 | README, `CREDITS.md`, nota do projeto, registro de testes, conferir a lista "Before you submit" | Entregar o que existe e anotar o que falta |

**G1 em detalhe.** Se na marca de 2:45 o loop não estiver divertido, ajuste números por até 30 minutos. Se ainda não funcionar, **simplifique** (2 torres, 2 ondas) em vez de trocar de jogo: com as exigências de ZIP, gravações e testes, uma troca depois do G1 não é realista. Eu havia sugerido o runner como plano B de jogo; ele só vale antes do G1.

**Ordem de cortes se atrasar (de cima para baixo)**

1. Terceira torre (Racer Zap): ficam duas.
2. Nível 3 das torres: ficam dois níveis.
3. Partículas extras e tremor de tela.
4. Animação elaborada do Vault: troca por brilho simples com fade.
5. Urso-polar (Hauler): a onda 3 usa só leões e tigres.
6. Passo 3 da mão-guia.

**Nunca cortar:** restart, CTA com confirmação e log, pausa por visibilidade, teste de tamanho do ZIP, README com tabela de testes, `CREDITS.md`.

**Registro de tempo:** manter um `TIME_LOG.md` com horário de início e fim de cada bloco. O roteiro de 5 minutos pede "tempo gasto", e o briefing avisa que, ao chegar nas 6 horas, deve-se entregar a versão atual e listar o que ficou inacabado ou sem teste.

## 11. Testes

O critério "Technical execution" (25%) cita testes críveis. Cada linha abaixo vira uma linha do README, com o ambiente e se foi **real ou emulado**. O que não for testado deve ser dito como não testado.

| Área | Caso | Esperado |
| --- | --- | --- |
| Tela | Retrato 320×568 e 390×844 | Tudo visível, cartões tocáveis, nada cortado |
| Tela | Janela larga de desktop | Coluna retrato centralizada, fundo Deep ink |
| Tela | Celular deitado | Aviso "Rotate your phone", jogo pausado, volta ao girar |
| Tela | Redimensionar a janela no meio da onda | Enquadramento e UI se ajustam, jogo continua |
| Entrada | Mouse e toque | Mesmo comportamento nos dois |
| Entrada | Dois dedos ao mesmo tempo; toque cancelado | Só o primeiro ponteiro conta; seleção descartada |
| Entrada | Tocar cartão sem moedas | Cartão treme, nada é construído |
| Entrada | Rolagem da página durante o jogo | Página não rola |
| Fluxo | Não construir nada | Ondas passam, jogo chega ao final |
| Fluxo | Construir e melhorar tudo que o saldo permite | Sem erro, saldo nunca negativo |
| Restart | No meio da onda, 10 vezes seguidas, na tela final | Estado limpo, sem inimigos ou eventos duplicados, ritmo igual ao primeiro |
| Visibilidade | Ocultar a aba no meio de uma onda por 10 s e voltar | Inimigos parados enquanto oculto, retomada sem salto de tempo |
| CTA | Clicar em **Explore Scrambly** | Mensagem "CTA clicked — demo only", log no console, URL não muda |
| Rede | Aba Network com o servidor estático simples | Só requisições do mesmo domínio, sem erros |
| Pacote | ZIP em pasta limpa | Abre, `index.html` na raiz, ≤ 5.000.000 bytes |

**Caso extremo para mostrar no roteiro:** ocultar a aba no meio de uma onda e voltar. É o que o briefing descreve de forma mais específica e é fácil de provar na gravação.

**Ambientes a registrar (preencher o que de fato usar):** Chrome no desktop; emulação de dispositivo em 320×568 e 390×844; um celular Android ou iPhone real, se possível; um segundo navegador (Firefox ou Safari). Para cada um, anotar real ou emulado e a data do teste.

## 12. Entrega e processo

São **três entregas** (Stage 1): o ZIP de produção, a gravação do processo inteiro e o passeio de 5 minutos. Os links precisam ser públicos.

**Estrutura do ZIP**

```text
index.html            <- raiz, obrigatório
Build/ TemplateData/  <- build WebGL
README.md             <- como rodar, ambientes testados (real/emulado), limitações, o que não foi testado, tamanho
CREDITS.md            <- assets, fonte, ferramentas, código reaproveitado, uso de IA
PROJECT_NOTE.md       <- o que usei, o que contribuí, uma decisão/correção/saída rejeitada
TIME_LOG.md           <- tempo por bloco
source/               <- Assets/Scripts, cena, prefabs, template WebGL, ajustes essenciais (sem Library/Temp)
```

**Nota do projeto (exigida):** dizer o que foi usado (Unity, pacote de assets, IA), o que você contribuiu e **uma decisão, correção ou saída rejeitada que melhorou o resultado**. Você precisa entender a implementação e conseguir modificá-la. Duas candidatas reais desta sessão, se você quiser usá-las com suas palavras:

- Uma sugestão inicial de IA usava o ovo como mascote (vindo do site público); o briefing fornece uma **raposa**, e a direção de arte foi corrigida antes de começar.
- A restrição de 5 MB tornou Unity condicional, e isso virou o teste de tamanho da Fase 0 com regra de decisão.

**Gravação do processo completo.** Ligar a gravação de tela no início e deixar rodando. O briefing quer ver, desde a interpretação do briefing até o build final: ferramentas, prompts, iterações, saídas aceitas e rejeitadas, correções e onde você aplicou julgamento próprio. Mostre como você avaliou e melhorou o que a IA entregou, não só que usou.

**Passeio de 5 minutos (roteiro)**

| Tempo | Conteúdo |
| --- | --- |
| 0:00–0:30 | Conceito e como Discover, Play e Redeem viram mecânica |
| 0:30–1:45 | Jogada ao vivo: construir, melhorar, ondas |
| 1:45–2:30 | Progressão e final: cadeados, Vault, tela final |
| 2:30–2:50 | CTA, com o console do navegador visível |
| 2:50–3:10 | Restart |
| 3:10–3:55 | Caso extremo: ocultar a aba no meio de uma onda e voltar |
| 3:55–5:00 | Decisões, tempo gasto, ferramentas e reuso, testes, limitações e a mudança mais importante |

**Stage 2 (se for convidado).** Marcar o Stage 1 com uma tag Git (por exemplo `stage1-final`) e manter os números no `GameConfig`, porque o pedido de ajuste costuma ser de balanceamento, clareza ou polimento. A revisão vai em nova versão ou tag, sem alterar o Stage 1, e o escopo é o mesmo jogo: jogo novo ou troca de engine está fora. Extras não são recompensados.

**Pontos do briefing para conferir antes de enviar**

- [ ] ZIP abre, tem `index.html` na raiz e está dentro de 5 MB.
- [ ] ZIP inclui código legível, instruções e créditos de assets e ferramentas.
- [ ] Entrada, layout, CTA, restart, visibilidade e casos extremos foram testados.
- [ ] Gravação do processo completo está acessível e mostra o uso de IA.
- [ ] Passeio de 5 minutos cobre decisões, tempo, ferramentas, testes, limitações e mudanças.

## 13. Riscos e dúvidas para o contato da Simula

| Risco | Mitigação |
| --- | --- |
| Build Unity WebGL estoura os 5 MB (build + fonte + assets) | Fase 0, regra de decisão da seção 8 e Plano B em TS + Three.js |
| Servidor estático simples não entrega o build comprimido do Unity | Decompression Fallback e teste com `python3 -m http.server` já na Fase 0 |
| Escopo do TD cresce além do tempo | Escopo fechado da seção 1, G1 em 2:45, lista de cortes da seção 10 |
| Balanceamento ruim deixa o jogo fácil demais ou injusto | Tudo no `GameConfig`; cerca de 1 h de playtest; sem derrota, o pior caso é só vazar moedas |
| Cube Pets animados pesam mais e os nomes dos clipes são desconhecidos | Importar só 3 pets e só os clipes usados; medir na Fase 0 com um pet animado; plano de reserva: squash and stretch por código |
| Os 3 predadores somam 444 KB (leão 173, tigre 172, urso 99), e o total bruto de assets vai a ~1,04 MB | Cortar o gato (165 KB), depois os detalhes do cenário; reaproveitar o mesmo modelo recolorido e escalado; medir na Fase 0 |
| Game Icons sem moeda, presente nem estrela, e contagem divergente (105 na página do Kenney, "125+" em cópia de terceiros) | Desenhar essas formas como sprites simples; conferir o ZIP oficial |
| A raposa do kit de referência destoa dos pets cúbicos, ou não funciona em 3D | Raposa como mascote 2D no Vault, no HUD e na tela final, e os pets cúbicos como o time que ela lidera (kit ainda não visto) |
| Memória do WebGL em iPhone | Memória inicial baixa, texturas pequenas, teste em aparelho real se houver |
| Licença de asset incerta | Os 4 pacotes escolhidos são CC0 (conferido nas páginas do Kenney) e a fonte Fredoka é OFL 1.1; mesmo assim, tudo listado em `CREDITS.md` |
| Parecer "anúncio genérico" (critério de 30%) | Três fases rotuladas Discover/Play/Redeem, cadeados de recompensa e tela final com a frase do produto |
| Fim do prazo sem tempo de documentar | Bloco fixo de 30 min no fim e `TIME_LOG.md` ao longo do trabalho |

**Perguntas para o contato da Simula (o briefing manda perguntar quando algo não está claro)**

1. A gravação do passeio de 5 minutos conta dentro das 6 horas de trabalho?
2. O "código-fonte legível" pode ser só `Assets/Scripts`, cena e template, em vez do projeto Unity completo, dado o limite de 5 MB para tudo?
3. O kit de referência tem restrições de uso ou formato da raposa que eu deva respeitar?

## Fontes

- [Simula Playable Game Developer Take-Home (briefing)](https://simula-ad.notion.site/Simula-Playable-Game-Developer-Take-Home-321af70f6f0d80f4b560ef057587f77b)
- [Scrambly](https://scrambly.io/)
