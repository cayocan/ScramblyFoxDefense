// Stage 2 drag-placement checks (run: node drag.js, with the build served on :8080).
// Reads the "[Drag] ... wallet=N ghost=visible|hidden towers=N" console lines the game prints for every drag outcome.
const puppeteer = require('puppeteer-core');
const path = require('path');

const URL = process.env.TEST_URL || 'http://localhost:8080/';
const CHROME = 'C:/Program Files/Google/Chrome/Application/chrome.exe';
const OUT = path.join(__dirname, 'out');
require('fs').mkdirSync(OUT, { recursive: true });
const sleep = ms => new Promise(r => setTimeout(r, ms));

// CSS px per viewport: card 0 centre, a point on the path, the suggested spot, a free valid spot, Restart.
const POINTS = {
  390: { card: [73, 766], path: [113, 210], spot: [168, 205], spot2: [225, 340], restart: [46, 26] },
  320: { card: [78, 512], path: [103, 150], spot: [143, 142], spot2: [183, 225], restart: [33, 17] },
};
const TOUCH_OFFSET = 80; // GameConfig.touchGhostOffsetCssPixels: the ghost floats this far above the finger

async function open(browser, width, height, touch) {
  const page = await browser.newPage();
  const logs = [];
  page.on('console', m => logs.push(m.text()));
  page.on('pageerror', e => logs.push(`PAGEERROR ${(e && e.message) || e}`));
  await page.emulate({ viewport: { width, height, deviceScaleFactor: 2, isMobile: touch, hasTouch: touch }, userAgent: await browser.userAgent() });
  await page.goto(URL, { waitUntil: 'load' });
  await page.waitForFunction(() => document.getElementById('loading').style.display === 'none', { timeout: 90000 });
  await sleep(1500);
  const cdp = await page.createCDPSession();
  return { page, logs, cdp };
}

// Low-level pointer helpers: press, move in steps, optionally release.
async function touch(s, type, points) {
  await s.cdp.send('Input.dispatchTouchEvent', { type, touchPoints: points.map(([x, y], id) => ({ x, y, id })) });
}
async function pressMove(s, mode, from, to) {
  if (mode === 'touch') {
    await touch(s, 'touchStart', [from]);
    for (let i = 1; i <= 8; i++) { await touch(s, 'touchMove', [lerp(from, to, i / 8)]); await sleep(40); }
  } else {
    // Hold still a moment: Unity only samples the cursor once per frame, and headless SwiftShader frames are slow.
    await s.page.mouse.move(...from); await s.page.mouse.down(); await sleep(250);
    for (let i = 1; i <= 8; i++) { await s.page.mouse.move(...lerp(from, to, i / 8)); await sleep(40); }
  }
  await sleep(200);
}
async function release(s, mode, at) {
  if (mode === 'touch') { await touch(s, 'touchMove', [at]); await sleep(120); await touch(s, 'touchEnd', []); }
  else { await s.page.mouse.move(...at); await s.page.mouse.up(); }
  await sleep(400);
}
const lerp = (a, b, t) => [a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t];
// The finger goes below the target so the ghost (offset upward) lands on it; the mouse goes straight there.
const aim = (mode, p) => (mode === 'touch' ? [p[0], p[1] + TOUCH_OFFSET] : p);

async function setHidden(page, hidden) {
  await page.evaluate(h => {
    Object.defineProperty(document, 'hidden', { configurable: true, get: () => h });
    Object.defineProperty(document, 'visibilityState', { configurable: true, get: () => (h ? 'hidden' : 'visible') });
    document.dispatchEvent(new Event('visibilitychange'));
  }, hidden);
}

const lastDrag = s => [...s.logs].reverse().find(l => l.startsWith('[Drag]')) || '(no drag line)';

async function run(browser, width, height, mode, report) {
  const P = POINTS[width];
  const s = await open(browser, width, height, mode === 'touch');
  const tag = `[${width}x${height} ${mode}]`;
  const step = async (name, fn) => {
    const before = s.logs.length;
    await fn();
    const lines = s.logs.slice(before).filter(l => l.startsWith('[Drag]') || l.startsWith('PAGEERROR'));
    report.push(`${tag} ${name}: ${lines.length ? lines.join(' || ') : '(no drag line) last=' + lastDrag(s)}`);
  };

  await step('drop on the path (invalid)', async () => {
    await pressMove(s, mode, P.card, aim(mode, P.path));
    await s.page.screenshot({ path: path.join(OUT, `d-${width}-${mode}-invalid.png`) });
    await release(s, mode, aim(mode, P.path));
  });
  await step('drop on the suggested spot (valid)', async () => {
    await pressMove(s, mode, P.card, aim(mode, P.spot));
    await s.page.screenshot({ path: path.join(OUT, `d-${width}-${mode}-valid.png`) });
    await release(s, mode, aim(mode, P.spot));
  });
  await sleep(14000); // wave 1 kills refill the wallet to 30+ for the next drags

  await step('tab hidden mid-drag, then released', async () => {
    await pressMove(s, mode, P.card, aim(mode, P.spot2));
    await setHidden(s.page, true);
    await sleep(800);
    await release(s, mode, aim(mode, P.spot2));
    await setHidden(s.page, false);
    await sleep(600);
  });
  await step('released outside the canvas', async () => {
    const outside = [width / 2, height + 40];
    await pressMove(s, mode, P.card, aim(mode, P.spot2));
    await release(s, mode, outside);
    if (mode === 'touch') await sleep(200);
  });
  await step('Restart pressed mid-drag', async () => {
    await pressMove(s, mode, P.card, aim(mode, P.spot2));
    if (mode === 'touch') {
      // Second finger on Restart while the first one drags: first pointer only, so it must be ignored.
      const finger = aim(mode, P.spot2);
      await touch(s, 'touchStart', [finger, P.restart]); await sleep(80);
      await touch(s, 'touchEnd', [finger]); await sleep(300);
      await s.page.screenshot({ path: path.join(OUT, `d-${width}-${mode}-restart-mid.png`) });
      await release(s, mode, finger);
    } else {
      // A mouse cannot press a second button: drag onto Restart and release there (cancel), then click it.
      await s.page.mouse.move(...P.restart); await sleep(200);
      await release(s, mode, P.restart);
    }
  });
  await step('Restart tapped after the drag, then a new drag start', async () => {
    if (mode === 'touch') { await s.page.touchscreen.tap(...P.restart); } else { await s.page.mouse.click(...P.restart); }
    await sleep(3000);
    await pressMove(s, mode, P.card, P.card);
    await release(s, mode, P.card);
  });
  await s.page.screenshot({ path: path.join(OUT, `d-${width}-${mode}-end.png`) });
  await s.page.close();
}

(async () => {
  const browser = await puppeteer.launch({ executablePath: CHROME, headless: 'new', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  const report = [];
  for (const [w, h] of [[390, 844], [320, 568]])
    for (const mode of ['touch', 'mouse']) await run(browser, w, h, mode, report);
  console.log(report.join('\n'));
  await browser.close();
})();
