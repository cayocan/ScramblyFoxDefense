// Stage 2 grabbing-hand tutorial check (run: node tutorial.js, with the build served on :8080).
// Captures the demo loop frame by frame, then completes the tutorial with a real touch drag at 390x844 and 320x568.
const puppeteer = require('puppeteer-core');
const path = require('path');

const URL = process.env.TEST_URL || 'http://localhost:8080/';
const OUT = path.join(__dirname, 'out');
require('fs').mkdirSync(OUT, { recursive: true });
const sleep = ms => new Promise(r => setTimeout(r, ms));
const SIZES = { 390: { h: 844, card: [73, 766], spot: [168, 205] }, 320: { h: 568, card: [78, 512], spot: [143, 142] } };

(async () => {
  const browser = await puppeteer.launch({ executablePath: 'C:/Program Files/Google/Chrome/Application/chrome.exe', headless: 'new', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  for (const width of [390, 320]) {
    const { h, card, spot } = SIZES[width];
    const page = await browser.newPage();
    const logs = [];
    page.on('console', m => logs.push(m.text()));
    await page.emulate({ viewport: { width, height: h, deviceScaleFactor: 2, isMobile: true, hasTouch: true }, userAgent: await browser.userAgent() });
    await page.goto(URL, { waitUntil: 'load' });
    await page.waitForFunction(() => document.getElementById('loading').style.display === 'none', { timeout: 90000 });
    await sleep(1500);
    if (width === 390) for (let i = 0; i < 12; i++) { await page.screenshot({ path: path.join(OUT, `t-hand-${String(i).padStart(2, '0')}.png`) }); await sleep(250); }

    // Real drag of the first card to the suggested spot (the ghost floats 80 CSS px above the finger).
    const cdp = await page.createCDPSession();
    const to = [spot[0], spot[1] + 80];
    const send = (type, p) => cdp.send('Input.dispatchTouchEvent', { type, touchPoints: p ? [{ x: p[0], y: p[1], id: 0 }] : [] });
    await send('touchStart', card);
    for (let i = 1; i <= 8; i++) { await send('touchMove', [card[0] + (to[0] - card[0]) * i / 8, card[1] + (to[1] - card[1]) * i / 8]); await sleep(40); }
    await page.screenshot({ path: path.join(OUT, `t-${width}-dragging.png`) });
    await send('touchEnd');
    await sleep(1500);
    await page.screenshot({ path: path.join(OUT, `t-${width}-done.png`) });
    console.log(`[${width}x${h}] ${logs.filter(l => /\[Drag\]|\[Tutorial\]|PAGEERROR/.test(l)).join(' || ')}`);
    await page.close();
  }
  await browser.close();
})();
