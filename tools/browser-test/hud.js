// Stage 2 top HUD checks (run: node hud.js, with the build served on :8080): screenshots idle and mid-wave at
// 390x844 and 320x568, then Mute and Restart. HUD positions are reference px scaled by min(w/390, h/844).
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
    const k = Math.min(width / 390, h / 844);
    const restart = [56 * k, 30 * k], mute = [130 * k, 30 * k];
    const page = await browser.newPage();
    const logs = [];
    page.on('console', m => logs.push(m.text()));
    page.on('pageerror', e => logs.push(`PAGEERROR ${(e && e.message) || e}`));
    await page.emulate({ viewport: { width, height: h, deviceScaleFactor: 2, isMobile: true, hasTouch: true }, userAgent: await browser.userAgent() });
    await page.goto(URL, { waitUntil: 'load' });
    await page.waitForFunction(() => document.getElementById('loading').style.display === 'none', { timeout: 90000 });
    await sleep(1500);
    await page.screenshot({ path: path.join(OUT, `hud-${width}-idle.png`) });
    await page.touchscreen.tap(width / 2, h / 2); await sleep(600); // tap to start

    const cdp = await page.createCDPSession();
    const to = [spot[0], spot[1] + 80];
    const send = (type, p) => cdp.send('Input.dispatchTouchEvent', { type, touchPoints: p ? [{ x: p[0], y: p[1], id: 0 }] : [] });
    await send('touchStart', card);
    for (let i = 1; i <= 8; i++) { await send('touchMove', [card[0] + (to[0] - card[0]) * i / 8, card[1] + (to[1] - card[1]) * i / 8]); await sleep(40); }
    await sleep(150); await send('touchEnd');
    await sleep(6000);
    await page.screenshot({ path: path.join(OUT, `hud-${width}-midwave.png`) });

    await page.touchscreen.tap(...mute); await sleep(500);
    const muted = await page.evaluate(() => window.scramblySfx.isMuted());
    await page.touchscreen.tap(...mute); await sleep(500);
    const unmuted = !(await page.evaluate(() => window.scramblySfx.isMuted()));
    const introsBefore = logs.filter(l => /\[Intro\]/.test(l)).length;
    await page.touchscreen.tap(...restart); await sleep(3000);
    await page.touchscreen.tap(width / 2, h / 2); await sleep(800); // tap to start again after the reload
    const restarted = logs.filter(l => /\[Intro\]/.test(l)).length > introsBefore;
    console.log(`[${width}x${h}] mute=${muted} unmute=${unmuted} restart=${restarted} errors=${logs.filter(l => /PAGEERROR/.test(l)).length} drag=${logs.filter(l => /\[Drag\] built/.test(l)).length ? 'built' : 'none'}`);
    await page.close();
  }
  await browser.close();
})();
