// Browser checks for the WebGL build served by `python -m http.server 8080` (no special headers).
const puppeteer = require('puppeteer-core');
const path = require('path');

// Override with TEST_URL to test an unpacked production ZIP served elsewhere.
const URL = process.env.TEST_URL || 'http://localhost:8080/';
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
  page.on('pageerror', e => logs.push(`PAGEERROR ${(e && e.message) || JSON.stringify(e)}`));
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
  const audio = () => phone.page.evaluate(() => window.scramblySfx.state() + (window.scramblySfx.isMuted() ? '/muted' : ''));
  const audioBefore = await audio();
  // Card 0 (Pop Blaster) then slot 0 (top slot), in CSS px.
  await phone.page.touchscreen.tap(73, 766);
  await sleep(300);
  await phone.page.screenshot({ path: path.join(OUT, 'b-390-selected.png') });
  await phone.page.touchscreen.tap(165, 182);
  await sleep(600);
  await phone.page.touchscreen.tap(165, 182); // tap the new tower: upgrade
  await sleep(800);
  await phone.page.screenshot({ path: path.join(OUT, 'b-390-built.png') });

  const audioAfterTap = await audio();
  const music = () => phone.page.evaluate(() => window.scramblySfx.musicStep());
  const musicA = await music(); await sleep(1200); const musicB = await music();
  await setHidden(phone.page, true);
  await sleep(1000);
  const audioHidden = await audio();
  const musicH1 = await music(); await sleep(1200); const musicH2 = await music();
  await sleep(2000);
  await setHidden(phone.page, false);
  await sleep(1000);
  const audioShown = await audio();
  await phone.page.touchscreen.tap(112, 26); // mute button
  await sleep(500);
  const audioMuted = await audio();
  await phone.page.touchscreen.tap(46, 26);  // restart: mute must survive the scene reload
  await sleep(2500);
  const audioAfterRestart = await audio();
  await phone.page.screenshot({ path: path.join(OUT, 'b-390-restarted.png') });
  report.push(`[music] playing after tap: step ${musicA} -> ${musicB}; while hidden: step ${musicH1} -> ${musicH2} (frozen = ${musicH1 === musicH2})`);
  report.push(`[audio] before tap=${audioBefore} after tap=${audioAfterTap} hidden=${audioHidden} shown=${audioShown} mute=${audioMuted} after restart=${audioAfterRestart}`);
  await phone.page.screenshot({ path: path.join(OUT, 'b-390-resumed.png') });

  const origin = new globalThis.URL(URL).origin;
  const foreign = phone.requests.filter(u => !u.startsWith(origin + '/') && !u.startsWith('blob:' + origin + '/'));
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

  // 4) FULL=1: play a whole session with no input, then CTA and Play again (real browser, real input).
  if (process.env.FULL) {
    const full = await open(browser, { width: 390, height: 844, deviceScaleFactor: 2, isMobile: true, hasTouch: true }, 'full');
    const urlBefore = full.page.url();
    // The opening tutorial blocks the session: wait 10 s to prove it does not start on its own, then do it.
    await sleep(10000);
    const stillWaiting = await full.page.evaluate(() => document.querySelector('canvas') !== null);
    await full.page.touchscreen.tap(195, 508); // a non-tutorial tap (centre of the board): must be ignored
    await full.page.touchscreen.tap(73, 766);  // tutorial step 1: first card
    await sleep(300);
    await full.page.touchscreen.tap(165, 182); // tutorial step 2: first slot -> wave 1 starts
    await sleep(70000); // one tower: the fox falls in wave 3 (~60 s) and the end card opens
    await full.page.screenshot({ path: path.join(OUT, 'b-full-endcard.png') });
    await full.page.touchscreen.tap(195, 508); // Explore Scrambly (panel centre - 10 px, button at -96 px)
    await sleep(800);
    await full.page.screenshot({ path: path.join(OUT, 'b-full-cta.png') });
    const ctaLogged = full.logs.some(l => l.includes('CTA clicked — demo only'));
    const urlAfter = full.page.url();
    await full.page.touchscreen.tap(195, 572); // Play again
    await sleep(2500);
    await full.page.screenshot({ path: path.join(OUT, 'b-full-again.png') });
    report.push(`[full] CTA logged=${ctaLogged}, url unchanged=${urlBefore === urlAfter}; screenshots b-full-*.png`);
  }

  console.log(report.join('\n'));
  await browser.close();
})().catch(e => { console.error('TEST FAILED', e); process.exit(1); });
