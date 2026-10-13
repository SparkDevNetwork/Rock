// The chat draws every color and every timing from one token sheet, which is what lets dark mode
// and any church's color work everywhere at once. A single literal in a component quietly breaks
// that for the one place it sits, so this gate refuses one. Each pattern it carries is planted
// here on its own line and the line is asserted, and the fixtures beside this file prove the gate
// bites when it is run the way CI runs it.
import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import test from 'node:test';
import { fileURLToPath } from 'node:url';

import { scan, styleFiles } from './check-styles.mjs';

const here = fileURLToPath(new URL('.', import.meta.url));
const repoRoot = fileURLToPath(new URL('../../', import.meta.url));
const shell = 'Rock.JavaScript.Obsidian.Blocks/src/Communication/Chat/ChatShell';

/** The line numbers a scan reported, once each, so a line that trips two patterns counts once. */
function linesOf(hits) {
  return [...new Set(hits.map((hit) => Number(/:(\d+):/.exec(hit)?.[1])))].sort((a, b) => a - b);
}

function run(...args) {
  return spawnSync(process.execPath, ['check-styles.mjs', ...args], { cwd: here, encoding: 'utf8' });
}

test('it refuses each kind of color, duration and curve written into a component style block', () => {
  const planted = [
    '<template><div class="chat-row"></div></template>',
    '<style scoped>',
    '.chat-row {',
    '    color: #dfe0e1;',
    '    background: rgb(1, 2, 3);',
    '    border-color: rgba(0, 0, 0, .1);',
    '    outline-color: hsl(210 50% 50%);',
    '    fill: hsla(210, 50%, 50%, .5);',
    '    caret-color: white;',
    '    border-top: 1px solid red;',
    '    transition: opacity 200ms;',
    '    animation: chat-spin 0.7s linear infinite;',
    '    transition-timing-function: cubic-bezier(.1, .9, .2, 1);',
    '    background: var(--color-interface-softer, #f4f5f6);',
    '}',
    '</style>',
  ].join('\n');

  const hits = scan(`${shell}/components/example.partial.obs`, planted);

  assert.deepEqual(linesOf(hits), [4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14], hits.join(' | '));
});

test('it lets a component mix tokens but not a token with a literal', () => {
  const planted = [
    '<style scoped>',
    '.chat-row {',
    '    background: color-mix(in srgb, var(--chat-brand) 12%, transparent);',
    '    border-color: color-mix(in srgb, var(--chat-brand-fill) 88%, #000);',
    '    outline-color: color-mix(in srgb, var(--chat-brand) 40%, black);',
    '}',
    '</style>',
  ].join('\n');

  const hits = scan(`${shell}/components/example.partial.obs`, planted);

  assert.deepEqual(linesOf(hits), [4, 5], hits.join(' | '));
});

test('it passes tokens, the color keywords that are not colors, and words that only look like colors', () => {
  const clean = [
    '<template>',
    '    <div class="chat-row" data-testid="row">Mark as read, the red one</div>',
    '    <svg><use href="#chat-i-bell"></use></svg>',
    '</template>',
    '<style scoped>',
    '#bead .chat-row.danger {',
    '    color: var(--chat-text);',
    '    background: transparent;',
    '    border-color: currentColor;',
    '    fill: inherit;',
    '    transition: opacity var(--chat-t-fast) var(--chat-ease-state), transform var(--chat-t-msg) var(--chat-ease-in);',
    '    animation: chat-spin var(--chat-t-spin) linear infinite;',
    '    transform: scale(var(--chat-pop-scale));',
    '    width: 34px;',
    '    line-height: 1.45;',
    '    font-weight: 600;',
    '}',
    '</style>',
    '<script setup lang="ts">',
    '    // the red fixture beside this file is what proves the gate bites',
    '    const sentence = "Turn on notifications";',
    '    const tone = "warning";',
    '</script>',
  ].join('\n');

  assert.deepEqual(scan(`${shell}/components/example.partial.obs`, clean), []);
});

test('it refuses a literal in an inline style, bound or not', () => {
  const planted = [
    '<template>',
    '    <div class="chat-row" style="color: #fff"></div>',
    '    <div class="chat-row" :style="{ background: \'rgb(1, 2, 3)\' }"></div>',
    '    <div class="chat-row" :style="{ transition: \'opacity 200ms\' }"></div>',
    '    <div class="chat-row" :style="{ \'--chat-av-hue\': hue, width: `${size}px` }"></div>',
    '</template>',
  ].join('\n');

  const hits = scan(`${shell}/components/example.partial.obs`, planted);

  assert.deepEqual(linesOf(hits), [2, 3, 4], hits.join(' | '));
});

test('it refuses a style object, a style property or an animation in script', () => {
  const planted = [
    'const look = { backgroundColor: "#ff0000" };',
    'element.style.color = "red";',
    'element.style.setProperty("--chat-shade", "rgba(0, 0, 0, .5)");',
    'element.style.transition = "transform 300ms ease";',
    'element.animate([{ opacity: 0 }, { opacity: 1 }], { duration: 200, easing: "cubic-bezier(.1, .9, .2, 1)" });',
    'const label = "Marked as read";',
    'element.style.setProperty("--chat-av-hue", String(hue));',
  ].join('\n');

  const hits = scan(`${shell}/composables/example.partial.ts`, planted);

  assert.deepEqual(linesOf(hits), [1, 2, 3, 4, 5], hits.join(' | '));
});

test('it reads the script of a component as well as its style', () => {
  const planted = [
    '<template><div class="chat-row"></div></template>',
    '<script setup lang="ts">',
    '    const look = { color: "#ff0000" };',
    '</script>',
  ].join('\n');

  const hits = scan(`${shell}/components/example.partial.obs`, planted);

  assert.deepEqual(linesOf(hits), [3], hits.join(' | '));
});

test('it refuses a hex color anywhere inside a script string, not only a string that is one', () => {
  const planted = [
    'element.style.border = "1px solid #fff";',
    'const ring = `0 0 0 1px #0D1117`;',
    'element.style.setProperty("--chat-line", "solid 2px #abcdef80");',
    'const icon = "#chat-i-bell";',
    'const anchor = `#${id}`;',
  ].join('\n');

  const hits = scan(`${shell}/composables/example.partial.ts`, planted);

  assert.deepEqual(linesOf(hits), [1, 2, 3], hits.join(' | '));
});

test('it refuses a named color inside a script string that reads as a CSS value, and passes prose', () => {
  const planted = [
    'element.style.boxShadow = "0 0 0 2px red";',
    'const edge = "solid white";',
    'const look = "color: navy";',
    'element.style.setProperty("--chat-x", "var(--chat-line) black");',
    'const label = "Mark the red one as read";',
    'const tone = "white";',
    'const words = "Green room at 2";',
  ].join('\n');

  const hits = scan(`${shell}/composables/example.partial.ts`, planted);

  // A lone named color is refused already; a sentence that mentions one is not a style.
  assert.deepEqual(linesOf(hits), [1, 2, 3, 4, 6], hits.join(' | '));
});

test('it refuses a literal inside a bound style value and in a static style in single quotes', () => {
  const planted = [
    '<template>',
    '    <div class="chat-row" :style="{ border: \'1px solid #abc\' }"></div>',
    "    <div class=\"chat-row\" style='color: #123456'></div>",
    "    <div class=\"chat-row\" style='box-shadow: 0 0 0 2px red'></div>",
    "    <div class=\"chat-row\" style='width: 34px'></div>",
    '</template>',
  ].join('\n');

  const hits = scan(`${shell}/components/example.partial.obs`, planted);

  assert.deepEqual(linesOf(hits), [2, 3, 4], hits.join(' | '));
});

test('run on the red fixtures, it names each hole a whole-string check would miss', () => {
  const result = run('--root', 'check-styles-fixtures/red');

  assert.equal(result.status, 1, result.stdout + result.stderr);
  for (const fixture of ['hexInString.partial.ts', 'namedInValue.partial.ts', 'boundStyle.partial.obs', 'singleQuoted.partial.obs']) {
    assert.match(result.stderr, new RegExp(fixture.replace('.', '\\.')), `${fixture} should be refused: ${result.stderr}`);
  }
});

test('the token sheet is the one file allowed to hold literals', () => {
  const sheet = [
    '.chat-shell {',
    '    --chat-surface: var(--color-interface-softest, #fdfdfd);',
    '    --chat-hover: rgba(13, 17, 23, .045);',
    '    --chat-ease-in: cubic-bezier(.1, .9, .2, 1);',
    '    --chat-t-msg: 260ms;',
    '}',
  ].join('\n');

  assert.deepEqual(scan('RockWeb/Styles/Blocks/Communication/Chat/chat.css', sheet), []);
});

test('the brand script may hold literals only inside its color arithmetic', () => {
  const brand = [
    '// #region Color arithmetic',
    'const rockOrange = "#FF791D";',
    'const ring = (rgb) => `rgba(${rgb[0]}, ${rgb[1]}, ${rgb[2]}, .32)`;',
    '// #endregion Color arithmetic',
    'export function paint(root) {',
    '    root.style.setProperty("--chat-x", "#0D1117");',
    '    root.style.transition = "opacity 200ms";',
    '}',
  ].join('\n');

  const hits = scan(`${shell}/ui/brand.partial.ts`, brand);

  assert.deepEqual(linesOf(hits), [6, 7], hits.join(' | '));
});

test('the brand script without its color arithmetic region is read whole', () => {
  const brand = 'const rockOrange = "#FF791D";';

  assert.deepEqual(linesOf(scan(`${shell}/ui/brand.partial.ts`, brand)), [1]);
});

test('run on the red fixtures, it fails and names each file', () => {
  const result = run('--root', 'check-styles-fixtures/red');

  assert.equal(result.status, 1, result.stdout + result.stderr);
  assert.match(result.stderr, /literals\.partial\.obs/);
  assert.match(result.stderr, /look\.partial\.ts/);
});

test('run on the green fixtures, which hold a token sheet full of literals, it passes', () => {
  const result = run('--root', 'check-styles-fixtures/green');

  assert.equal(result.status, 0, result.stdout + result.stderr);
});

test('it reads the chat block and every file under its folder', () => {
  const files = styleFiles();

  assert.ok(files.includes('Rock.JavaScript.Obsidian.Blocks/src/Communication/Chat/chatShell.obs'), files.join('\n'));
  assert.ok(files.includes(`${shell}/components/messageRow.partial.obs`), files.join('\n'));
  assert.ok(files.includes(`${shell}/composables/useSend.partial.ts`), files.join('\n'));
});

test('the chat files in this branch are clean today', () => {
  // Spawned rather than called, because what has to hold is that the gate CI runs exits zero.
  const result = run();

  assert.equal(result.status, 0, result.stdout + result.stderr);
  assert.match(result.stdout, /no style literals/i);
});

test('the gate runs locally and in the workflow', () => {
  const ci = readFileSync(resolve(here, 'ci.mjs'), 'utf8');
  const workflow = readFileSync(resolve(repoRoot, '.github/workflows/chat-ci.yml'), 'utf8');
  const scripts = JSON.parse(readFileSync(resolve(here, 'package.json'), 'utf8')).scripts ?? {};

  assert.match(ci, /check-styles\.mjs/);
  const runsByScript = /npm run check:styles/.test(workflow) && /check-styles\.mjs/.test(scripts['check:styles'] ?? '');
  assert.ok(runsByScript || /check-styles\.mjs/.test(workflow), 'the workflow should run the style gate');
});
