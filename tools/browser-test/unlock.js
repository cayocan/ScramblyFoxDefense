// Stage 2 milestone unlock checks (run: node unlock.js, with the build served on :8080), 390x844 touch.
// Reads "[Unlock]" and "[Drag]" console lines; screenshots go to out/u-*.png.
const puppeteer = require('puppeteer-core');
const path = require('path');

const URL = process.env.TEST_URL || 'http://localhost:8080/';
const OUT = path.join(__dirname, 'out');
require('fs').mkdirSync(OUT, { recursive: true });
const sleep = ms => new Promise(r => setTimeout(r, ms));
const CARDS = [[73, 766], [195, 766], [317, 766]];
const SPOT = [168, 205], SPOT2 = [225, 340], RESTART = [46, 26];

async function open(browser) {
  const page = await browser.newPage();
  const logs = [];
  page.on('console', m => logs.push(m.text()));
  page.on('pageerror', e => logs.push(`PAGEERROR ${(e && e.message) || e}`));
  await page.emulate({ viewport: { width: 390, height: 844, deviceScaleFactor: 2, isMobile: true, hasTouch: true }, userAgent: await browser.userAgent() });
  await page.goto(URL, { waitUntil: 'load' });
  await page.waitForFunction(() => document.getElementById('loading').style.display === 'none', { timeout: 90000 });
  await sleep(1500);
  await page.mouse.click(195, 420); await sleep(600); // opening hook: tap to start
  return { page, logs, cdp: await page.createCDPSession() };
}

async function drag(s, from, target) {
  const to = [target[0], target[1] + 80]; // the ghost floats 80 CSS px above the finger
  const send = (type, p) => s.cdp.send('Input.dispatchTouchEvent', { type, touchPoints: p ? [{ x: p[0], y: p[1], id: 0 }] : [] });
  await send('touchStart', from);
  for (let i = 1; i <= 8; i++) { await send('touchMove', [from[0] + (to[0] - from[0]) * i / 8, from[1] + (to[1] - from[1]) * i / 8]); await sleep(40); }
  await sleep(150);
  await send('touchEnd');
  await sleep(400);
}

async function waitFor(s, pattern, timeoutMs) {
  const t0 = Date.now();
  while (Date.now() - t0 < timeoutMs) { if (s.logs.some(l => pattern.test(l))) return true; await sleep(100); }
  return false;
}

async function setHidden(page, hidden) {
  await page.evaluate(h => {
    Object.defineProperty(document, 'hidden', { configurable: true, get: () => h });
    Object.defineProperty(document, 'visibilityState', { configurable: true, get: () => (h ? 'hidden' : 'visible') });
    document.dispatchEvent(new Event('visibilitychange'));
  }, hidden);
}

const lines = (s, re) => s.logs.filter(l => re.test(l)).join(' || ');

(async () => {
  const browser = await puppeteer.launch({ executablePath: 'C:/Program Files/Google/Chrome/Application/chrome.exe', headless: 'new', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  const report = [];

  // 1) Play to both unlocks with two towers and one upgrade (the Task 1 winning plan, by hand).
  const a = await open(browser);
  await a.page.screenshot({ path: path.join(OUT, 'u-start-locked.png') });
  await drag(a, CARDS[0], SPOT);
  await drag(a, CARDS[1], SPOT2); // locked: must be refused
  report.push(`[locked card] ${lines(a, /\[Drag\]/)}`);
  const first = await waitFor(a, /\[Unlock\]/, 90000);
  for (let i = 0; i < 6; i++) { await a.page.screenshot({ path: path.join(OUT, `u-celebrate-${i}.png`) }); await sleep(200); }
  await drag(a, CARDS[1], SPOT2); // the newly unlocked Puzzle Pulse
  const second = await waitFor(a, /\[Unlock\].*card=2/, 120000);
  await sleep(600);
  await a.page.screenshot({ path: path.join(OUT, 'u-second-unlock.png') });
  report.push(`[play] first unlock=${first} second unlock=${second}: ${lines(a, /\[Unlock\]|PAGEERROR/)}`);
  report.push(`[play] drags: ${lines(a, /\[Drag\] (built|invalid)/)}`);
  await a.page.close();

  // 2) Restart during the first celebration: back to the locked state, no stuck banner or card.
  const b = await open(browser);
  await drag(b, CARDS[0], SPOT);
  await waitFor(b, /\[Unlock\]/, 90000);
  await sleep(300);
  await b.page.touchscreen.tap(...RESTART);
  await sleep(3500);
  await b.page.screenshot({ path: path.join(OUT, 'u-restart-mid-celebration.png') });
  const before = b.logs.length;
  await drag(b, CARDS[1], SPOT2); // locked again after the restart: refused, nothing logged as a drag start
  report.push(`[restart mid-celebration] after restart, card 1 drag: ${b.logs.slice(before).filter(l => /\[Drag\]/.test(l)).join(' || ') || 'refused (no drag started)'}`);
  await b.page.close();

  // 3) Hide the tab during the celebration, then come back: it finishes normally.
  const c = await open(browser);
  await drag(c, CARDS[0], SPOT);
  await waitFor(c, /\[Unlock\]/, 90000);
  await setHidden(c.page, true);
  await sleep(2500);
  await c.page.screenshot({ path: path.join(OUT, 'u-hidden-mid-celebration.png') });
  await setHidden(c.page, false);
  await sleep(3500);
  await c.page.screenshot({ path: path.join(OUT, 'u-after-hidden.png') });
  report.push(`[hidden mid-celebration] ${lines(c, /PagePause|\[Unlock\]|PAGEERROR/)}`);
  await c.page.close();

  console.log(report.join('\n'));
  await browser.close();
})();
