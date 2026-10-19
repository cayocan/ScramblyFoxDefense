// Stage 2 audio checks (run: node audio.js, with the build served on :8080), 390x844 touch.
// Reads window.scramblySfx test hooks: state(), isMuted(), samplesLoaded(), musicLevel(), musicStep().
const puppeteer = require('puppeteer-core');

const URL = process.env.TEST_URL || 'http://localhost:8080/';
const sleep = ms => new Promise(r => setTimeout(r, ms));

async function open(browser, blockSamples) {
  const page = await browser.newPage();
  const logs = [];
  page.on('console', m => logs.push(m.text()));
  page.on('pageerror', e => logs.push(`PAGEERROR ${(e && e.message) || e}`));
  if (blockSamples) {
    await page.setRequestInterception(true);
    page.on('request', r => (r.url().includes('/sfx/') ? r.abort() : r.continue()));
  }
  await page.emulate({ viewport: { width: 390, height: 844, deviceScaleFactor: 2, isMobile: true, hasTouch: true }, userAgent: await browser.userAgent() });
  await page.goto(URL, { waitUntil: 'load' });
  await page.waitForFunction(() => document.getElementById('loading').style.display === 'none', { timeout: 90000 });
  await sleep(1500);
  return { page, logs };
}

async function tutorialDrag(page) {
  const cdp = await page.createCDPSession();
  const from = [73, 766], to = [168, 285]; // ghost lands on the suggested spot (80 px above the finger)
  const send = (type, p) => cdp.send('Input.dispatchTouchEvent', { type, touchPoints: p ? [{ x: p[0], y: p[1], id: 0 }] : [] });
  await send('touchStart', from);
  for (let i = 1; i <= 8; i++) { await send('touchMove', [from[0] + (to[0] - from[0]) * i / 8, from[1] + (to[1] - from[1]) * i / 8]); await sleep(40); }
  await sleep(150);
  await send('touchEnd');
}

async function setHidden(page, hidden) {
  await page.evaluate(h => {
    Object.defineProperty(document, 'hidden', { configurable: true, get: () => h });
    Object.defineProperty(document, 'visibilityState', { configurable: true, get: () => (h ? 'hidden' : 'visible') });
    document.dispatchEvent(new Event('visibilitychange'));
  }, hidden);
}

const audio = page => page.evaluate(() => {
  const s = window.scramblySfx;
  return `state=${s.state()} muted=${s.isMuted()} samples=${s.samplesLoaded()} level=${s.musicLevel()} step=${s.musicStep()} music=${s.musicSource()}`;
});

(async () => {
  const browser = await puppeteer.launch({ executablePath: 'C:/Program Files/Google/Chrome/Application/chrome.exe', headless: 'new', args: ['--use-angle=swiftshader', '--enable-unsafe-swiftshader', '--autoplay-policy=user-gesture-required'] });
  const report = [];

  const a = await open(browser, false);
  report.push(`before any gesture: ${await audio(a.page)}`);
  for (let i = 0; i < 10; i++) { await a.page.screenshot({ path: require('path').join(__dirname, 'out', 'h-hook-' + i + '.png') }); await sleep(500); } // first 5 s
  await a.page.touchscreen.tap(195, 420); await sleep(600); // opening hook: tap to start (also the first gesture)
  report.push(`after the start tap: ${await audio(a.page)}; ${a.logs.filter(l => /\[Intro\]/.test(l)).join('')}`);
  await tutorialDrag(a.page);
  await sleep(2500);
  report.push(`after the first drag (wave 1): ${await audio(a.page)}`);
  await a.page.touchscreen.tap(112, 26); await sleep(500); // mute
  report.push(`muted: ${await audio(a.page)}`);
  await a.page.touchscreen.tap(112, 26); await sleep(500); // unmute
  report.push(`unmuted: ${await audio(a.page)}`);
  await setHidden(a.page, true); await sleep(1000);
  const s1 = await audio(a.page); await sleep(1200); const s2 = await audio(a.page);
  report.push(`hidden: ${s1} | 1.2 s later: ${s2}`);
  await setHidden(a.page, false); await sleep(800);
  report.push(`shown again: ${await audio(a.page)}`);
  await a.page.touchscreen.tap(46, 26); await sleep(3000); // restart
  report.push(`after restart (intro): ${await audio(a.page)}`);
  report.push(`errors: ${a.logs.filter(l => /PAGEERROR|error/i.test(l)).join(' | ') || 'none'}`);
  await a.page.close();

  const b = await open(browser, true);
  await b.page.touchscreen.tap(195, 420); await sleep(600);
  await tutorialDrag(b.page);
  await sleep(2500);
  report.push(`OGGs blocked (synth fallback): ${await audio(b.page)}; drag: ${b.logs.filter(l => /\[Drag\] built/.test(l)).join('') || 'not built'}; errors: ${b.logs.filter(l => /PAGEERROR/.test(l)).join(' | ') || 'none'}`);
  await b.page.close();

  console.log(report.join('\n'));
  await browser.close();
})();
