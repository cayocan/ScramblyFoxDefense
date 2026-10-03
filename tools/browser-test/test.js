// Browser checks for the WebGL build served by `python -m http.server 8080` (no special headers).
const puppeteer = require('puppeteer-core');
const path = require('path');

const URL = 'http://localhost:8080/';
const CHROME = 'C:/Program Files/Google/Chrome/Application/chrome.exe';
const OUT = path.join(__dirname, 'out');
require('fs').mkdirSync(OUT, { recursive: true });
const sleep = ms => new Promise(r => setTimeout(r, ms));

async function open(browser, viewport, label) {
  const page = await browser.newPage();
  const requests = [];
  const logs = [];
  page.on('request', r => requests.push(r.url()));
  page.on('requestfailed', r => logs.push(`REQUEST FAILED ${r.url()} ${r.failure() && r.failure().errorText}`));
  page.on('response', r => { if (r.status() >= 400) logs.push(`HTTP ${r.status()} ${r.url()}`); });
  page.on('console', m => logs.push(`${m.type()}: ${m.text()}`));
  page.on('pageerror', e => logs.push(`PAGEERROR ${e.message}`));
  await page.emulate({ viewport, userAgent: (await browser.userAgent()) });
  const t0 = Date.now();
  await page.goto(URL, { waitUntil: 'load' });
  await page.waitForFunction(() => document.getElementById('loading').style.display === 'none', { timeout: 90000 });
  const loadMs = Date.now() - t0;
  await sleep(1500);
  return { page, requests, logs, loadMs, label };
}

async function setHidden(page, hidden) {
  await page.evaluate(h => {
    Object.defineProperty(document, 'hidden', { configurable: true, get: () => h });
    Object.defineProperty(document, 'visibilityState', { configurable: true, get: () => (h ? 'hidden' : 'visible') });
    document.dispatchEvent(new Event('visibilitychange'));
  }, hidden);
}

(async () => {
  const browser = await puppeteer.launch({ executablePath: CHROME, headless: 'new', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader'] });
  const report = [];

  // 1) Portrait phone 390x844: load, network, touch build, visibility pause.
  const phone = await open(browser, { width: 390, height: 844, deviceScaleFactor: 2, isMobile: true, hasTouch: true }, '390x844');
  await phone.page.screenshot({ path: path.join(OUT, 'b-390-start.png') });
  // Card 0 (Pop Blaster) then slot 0 (top slot), in CSS px.
  await phone.page.touchscreen.tap(73, 766);
  await sleep(300);
  await phone.page.screenshot({ path: path.join(OUT, 'b-390-selected.png') });
  await phone.page.touchscreen.tap(165, 182);
  await sleep(600);
  await phone.page.touchscreen.tap(165, 182); // tap the new tower: upgrade
  await sleep(800);
  await phone.page.screenshot({ path: path.join(OUT, 'b-390-built.png') });

  await setHidden(phone.page, true);
  await sleep(3000);
  await setHidden(phone.page, false);
  await sleep(1000);
  await phone.page.screenshot({ path: path.join(OUT, 'b-390-resumed.png') });

  const foreign = phone.requests.filter(u => !u.startsWith('http://localhost:8080/'));
  report.push(`[390x844] load ${phone.loadMs} ms, ${phone.requests.length} requests, foreign: ${foreign.length ? foreign.join(' ') : 'none'}`);
  report.push(`[390x844] requests: ${phone.requests.map(u => u.replace(URL, '/')).join(' ')}`);
  report.push(`[390x844] logs:\n  ${phone.logs.filter(l => !/^debug/.test(l)).join('\n  ')}`);

  // 2) Landscape phone: rotate prompt + pause.
  const land = await open(browser, { width: 844, height: 390, deviceScaleFactor: 2, isMobile: true, hasTouch: true }, '844x390');
  const rotateShown = await land.page.evaluate(() => getComputedStyle(document.getElementById('rotate')).display);
  const pausedFlag = await land.page.evaluate(() => window.scramblyPaused);
  await land.page.screenshot({ path: path.join(OUT, 'b-landscape.png') });
  await land.page.setViewport({ width: 390, height: 844, deviceScaleFactor: 2, isMobile: true, hasTouch: true });
  await sleep(800);
  const rotateAfter = await land.page.evaluate(() => getComputedStyle(document.getElementById('rotate')).display);
  report.push(`[landscape] rotate overlay=${rotateShown} paused=${pausedFlag}; after rotating back overlay=${rotateAfter}`);
  report.push(`[landscape] logs: ${land.logs.filter(l => /PagePause|error|FAILED|HTTP/i.test(l)).join(' | ')}`);

  // 3) Small phone and desktop layouts.
  const small = await open(browser, { width: 320, height: 568, deviceScaleFactor: 2, isMobile: true, hasTouch: true }, '320x568');
  await small.page.screenshot({ path: path.join(OUT, 'b-320.png') });
  const desk = await open(browser, { width: 1280, height: 720, deviceScaleFactor: 1, isMobile: false, hasTouch: false }, 'desktop');
  await desk.page.screenshot({ path: path.join(OUT, 'b-desktop.png') });
  const scrollable = await desk.page.evaluate(() => document.documentElement.scrollHeight > innerHeight || document.documentElement.scrollWidth > innerWidth);
  report.push(`[320x568] load ${small.loadMs} ms; [desktop] load ${desk.loadMs} ms, page scrollable=${scrollable}`);

  console.log(report.join('\n'));
  await browser.close();
})().catch(e => { console.error('TEST FAILED', e); process.exit(1); });
