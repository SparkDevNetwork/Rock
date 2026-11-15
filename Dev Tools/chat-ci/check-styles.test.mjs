// The chat draws every color and every timing from one token sheet, which is what lets dark mode
// and any church's color work everywhere at once. A single literal in a component quietly breaks
// that for the one place it sits, so this gate refuses one. Each pattern it carries is planted
// here on its own line and the line is asserted, and the fixtures beside this file prove the gate
// bites when it is run the way CI runs it.
import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { mkdtempSync, readFileSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, resolve } from 'node:path';
import test from 'node:test';
import { fileURLToPath } from 'node:url';

import { findStyleDebt, scan, scanKeyframeNames, styleFiles, STYLE_RULES } from './check-styles.mjs';

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

const sheet = 'RockWeb/Styles/Blocks/Communication/Chat/chat.css';

/** The lines one rule found, in one file when named, sorted. */
function ruleLines(findings, rule, file) {
  return findings.filter((found) => found.rule === rule && (!file || found.file === file)).map((found) => found.line).sort((a, b) => a - b);
}

/** The tokens or classes one rule named, sorted. */
function ruleNames(findings, rule, pattern) {
  return findings.filter((found) => found.rule === rule).map((found) => pattern.exec(found.what)?.[0]).sort();
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

test('it refuses an all-digit hex in any script string, and passes a hash glued to a word or an entity', () => {
  const planted = [
    'el.style.background = "linear-gradient(#111, #222)";',
    'const pair = "#000 #111";',
    'const words = "fill #222 now";',
    'const label = "Issue #1234";',
    'const entity = "&#1234;";',
    'const glued = "abc#123";',
  ].join('\n');

  const hits = scan(`${shell}/composables/example.partial.ts`, planted);

  // A script string cannot say whether it ends up in a style, so a hash and digits is a color
  // even in words; only a hash glued to a word or an entity's ampersand starts no color.
  assert.deepEqual(linesOf(hits), [1, 2, 3, 4], hits.join(' | '));
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

// A comment beside a template's root makes the built component a fragment, which drops the
// attributes and scoped rules a parent gives it; the test renderer keeps one root, so only the
// built block shows it, and only this gate catches it before then.
test('it refuses a comment beside a template root and passes one inside it', () => {
  const planted = [
    '<template>',
    '    <!-- beside the root -->',
    '    <div class="chat-row">',
    '        <!-- inside the root -->',
    '        <template v-if="isShown">',
    '            <!-- inside a slot template -->',
    '        </template>',
    '    </div>',
    '    <!-- after the root -->',
    '</template>',
  ].join('\n');

  const hits = scan(`${shell}/components/example.partial.obs`, planted);

  assert.deepEqual(linesOf(hits), [2, 9], hits.join(' | '));
  assert.match(hits[0], /comment beside the template's root/);
});

// A comment the built block keeps as a child of a component's slot content, or of a Teleport, is
// patched in place by Vue's production runtime without its element; when the slot then empties
// to nothing but comments, removing it throws, the render stops halfway, and every reload after
// leaves another copy of the slot's content in the page. The test renderer runs the development
// runtime, which keeps the element, so only this gate sees it.
test('it refuses a comment directly in slot content or a Teleport and passes one in an element, a conditional, or beside slot templates', () => {
  const planted = [
    '<template>',
    '    <PanelHost title="Thread">',
    '        <!-- beside slot templates, which the compiler drops -->',
    '        <template #actions>',
    '            <!-- in a named slot -->',
    '            <template v-if="isShown">',
    '                <!-- in a conditional inside the slot -->',
    '                <button type="button"',
    '                        class="chat-th-follow">',
    '                    <!-- in an element -->',
    '                </button>',
    '                <Teleport to="body">',
    '                    <!-- in a teleport -->',
    '                    <div v-if="isTipShown"></div>',
    '                </Teleport>',
    '            </template>',
    '        </template>',
    '        <div class="chat-body">',
    '            <!-- in an element of the default slot -->',
    '            <component :is="tag">',
    '                <!-- in a dynamic component -->',
    '            </component>',
    '            <SettingGroup>',
    '                <!-- in a default slot written straight inside -->',
    '                <SettingRow title="Members" />',
    '            </SettingGroup>',
    '        </div>',
    '    </PanelHost>',
    '</template>',
  ].join('\n');

  const hits = scan(`${shell}/components/example.partial.obs`, planted);

  assert.deepEqual(linesOf(hits), [5, 13, 21, 24], hits.join(' | '));
  assert.match(hits[0], /comment directly in slot content/);
});

test('it refuses an icon sized by a rule of its caller, in a component or the token sheet', () => {
  // Two rules of one specificity fight by the order the built styles load in, which no test sees:
  // the icon's own size won that fight once and drew a group mark's glyph too large. A caller
  // sizes an icon through the custom properties the icon reads, never against its rules.
  const component = [
    '<style scoped>',
    '.chat-row :deep(.chat-ic) {',
    '    width: 16px;',
    '    height: 16px;',
    '    color: var(--chat-icon);',
    '}',
    '.chat-row {',
    '    --chat-ic-size: 16px;',
    '    --chat-ic-stroke: 2.2;',
    '}',
    '.chat-row > .chat-ic { margin-top: 2px; }',
    '</style>',
  ].join('\n');
  const sheet = [
    '.chat-shell .chat-badge .chat-ic {',
    '    stroke-width: 2.2;',
    '}',
    '.chat-badge {',
    '    --chat-ic-size: 12px;',
    '}',
  ].join('\n');

  const componentHits = scan(`${shell}/components/example.partial.obs`, component);
  const sheetHits = scan('RockWeb/Styles/Blocks/Communication/Chat/chat.css', sheet);

  assert.deepEqual(linesOf(componentHits), [3, 4], componentHits.join(' | '));
  assert.match(componentHits[0], /--chat-ic-size/);
  assert.deepEqual(linesOf(sheetHits), [2], sheetHits.join(' | '));
});

test('the icon itself may size its own drawing', () => {
  const icon = [
    '<style scoped>',
    '.chat-ic {',
    '    width: var(--chat-ic-size, 18px);',
    '    stroke-width: var(--chat-ic-stroke, 1.75);',
    '}',
    '</style>',
  ].join('\n');

  assert.deepEqual(scan(`${shell}/ui/icon.partial.obs`, icon), []);
});

test('it refuses an icon reached as an svg, through :deep(), by its class attribute, or sized by its logical size', () => {
  // The icon is the svg every icon draws, so a rule on an svg sizes one as surely as a rule on
  // its class does; and inline-size and block-size are its width and height by other names.
  const component = [
    '<style scoped>',
    '.chat-row svg { width: 16px; }',
    '.chat-row :deep(svg) { height: 16px; }',
    '.chat-row [class~="chat-ic"] { width: 16px; }',
    '.chat-row :deep(.chat-ic) { inline-size: 16px; }',
    '.chat-row .chat-ic { block-size: 16px; }',
    '.chat-row > svg.is-big { max-inline-size: 20px; }',
    '.chat-row :is(.chat-ic, .other) { width: 16px; }',
    '</style>',
  ].join('\n');

  const hits = scan(`${shell}/components/example.partial.obs`, component);

  assert.deepEqual(linesOf(hits), [2, 3, 4, 5, 6, 7, 8], hits.join(' | '));
});

test('it refuses an icon given its size by an inline style, bound or not', () => {
  const component = [
    '<template>',
    '    <div class="chat-row">',
    '        <Icon name="bell" :style="{ width: size + \'px\' }" />',
    '        <Icon name="bell" style="height: 12px" />',
    '        <svg class="chat-ic" :style="{ \'stroke-width\': 2 }"></svg>',
    '        <Icon name="bell" :style="{ \'--chat-ic-size\': size + \'px\' }" />',
    '        <div :style="{ width: size + \'px\' }"></div>',
    '    </div>',
    '</template>',
  ].join('\n');

  const hits = scan(`${shell}/components/example.partial.obs`, component);

  assert.deepEqual(linesOf(hits), [3, 4, 5], hits.join(' | '));
});

test('a rule on what holds an icon, or beside one, is not a rule on the icon', () => {
  // Only the last compound of a selector is the element its rule sizes, so an icon named earlier
  // in it, or inside :has() or :not(), names what the rule's element holds, follows or is not.
  const component = [
    '<style scoped>',
    '.chat-tool:has(.chat-ic) { width: 28px; }',
    '.chat-ic + span { width: 28px; }',
    '.chat-ic ~ .chat-label { min-width: 0; }',
    '.chat-row .chat-ic.is-big > .chat-x { height: 4px; }',
    'svg + .chat-tool { width: 28px; }',
    '.chat-tool:not(.chat-ic) { width: 28px; }',
    '.chat-svg-box { width: 28px; }',
    '.chat-row :deep(.chat-ic) + .chat-label { width: 28px; }',
    '</style>',
  ].join('\n');

  const hits = scan(`${shell}/components/example.partial.obs`, component);

  assert.deepEqual(hits, []);
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

test('it refuses a keyframe a script finds by name when a scoped block defines it, since the build renames it there', () => {
  const component = [
    '<template><div class="chat-feed"></div></template>',
    '<style scoped>',
    '.chat-feed.is-entering {',
    '    animation: chat-feed-enter var(--chat-d-view) var(--chat-ease-in);',
    '}',
    '@keyframes chat-feed-enter {',
    '    from { opacity: 0; }',
    '}',
    '@keyframes chat-unread-flash {',
    '    from { opacity: 0; }',
    '}',
    '</style>',
  ].join('\n');
  const script = [
    'const entrance = animations.find(animation => (animation as CSSAnimation).animationName === "chat-feed-enter");',
    'const isFlash = event.animationName.includes("chat-unread-flash");',
  ].join('\n');
  const sheet = '@keyframes chat-enter {\n    from { opacity: 0; }\n}\n';
  const reader = ['<script setup lang="ts">', "if (event.animationName === 'chat-enter') done();", '</script>'].join('\n');

  const hits = scanKeyframeNames([
    { file: `${shell}/components/feed.partial.obs`, text: component },
    { file: `${shell}/composables/useTail.partial.ts`, text: script },
    { file: 'RockWeb/Styles/Blocks/Communication/Chat/chat.css', text: sheet },
    { file: `${shell}/ui/host.partial.obs`, text: reader },
  ]);

  // only the scoped definition of a name read exactly: a name read by part, or defined in the sheet, is not renamed out of reach
  assert.deepEqual(hits.map((hit) => hit.replace(/:\d+:.*/, '')), [`${shell}/components/feed.partial.obs`], hits.join(' | '));
  assert.match(hits[0], /:6:.*chat-feed-enter/);
});

test('it reads the chat block and every file under its folder', () => {
  const files = styleFiles();

  assert.ok(files.includes('Rock.JavaScript.Obsidian.Blocks/src/Communication/Chat/chatShell.obs'), files.join('\n'));
  assert.ok(files.includes(`${shell}/components/messageRow.partial.obs`), files.join('\n'));
  assert.ok(files.includes(`${shell}/composables/useSend.partial.ts`), files.join('\n'));
  assert.ok(files.includes('RockWeb/Styles/Blocks/Communication/Chat/chat.css'), files.join('\n'));
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

// The rules below come from one conviction: styles have three homes, the token sheet, a part's own
// file and a screen's layout. Each is held by the ratchet in known-debt.json, so today's code passes
// while no file may gain a finding. Every cell plants the real spellings found in the chat today.

test('every style rule carries its mode and its reason', () => {
  const ids = STYLE_RULES.map((rule) => rule.id);

  assert.deepEqual(ids, ['S1', 'S2', 'S3', 'S4', 'S5', 'S6', 'S7', 'S8', 'S9', 'S10', 'S11', 'S12']);
  for (const rule of STYLE_RULES) {
    assert.match(rule.mode, /^(ratchet|report)$/, rule.id);
    assert.ok(rule.reason.length > 20, `${rule.id} states no reason`);
  }
  assert.deepEqual(STYLE_RULES.filter((rule) => rule.mode === 'report').map((rule) => rule.id), ['S10', 'S11']);
});

test('S1 refuses a literal font size in a style block, an inline style and script, and passes tokens', () => {
  const component = [
    '<template>',
    '    <div class="chat-row" style="font-size: 12px"></div>',
    '    <div class="chat-row" :style="{ fontSize: \'11px\' }"></div>',
    '    <div class="chat-row" :style="{ fontSize: `${Math.round(size * 0.42 * 100) / 100}px` }"></div>',
    '</template>',
    '<style scoped>',
    '.chat-row {',
    '    font-size: 12.5px;',
    '    font: 600 13px/1.4 inherit;',
    '    font-size: var(--chat-fs-2, 12px);',
    '    font-size: 9px !important;',
    '    font-size: var(--chat-fs-3);',
    '    font-size: inherit;',
    '    font: inherit;',
    '    font-size: 0;',
    '    font-size: calc(var(--chat-fs-3) * 1.1);',
    '}',
    '</style>',
    '<script setup lang="ts">',
    '    element.style.fontSize = "13px";',
    '    element.style.setProperty("font-size", "14px");',
    '    const label = "font-size: 12px is what the old row used";',
    '</script>',
  ].join('\n');

  const findings = findStyleDebt([{ file: `${shell}/components/example.partial.obs`, text: component }]);

  assert.deepEqual(ruleLines(findings, 'S1'), [2, 3, 8, 9, 10, 11, 20, 21], JSON.stringify(findings));
});

test('S1 to S3 read the token sheet\'s rules but not its tokens', () => {
  const css = [
    ':root {',
    '    --chat-fs-3: 13px;',
    '    --chat-r-md: 12px;',
    '    --chat-z-popover: 1050;',
    '}',
    '.chat-shell .chat-x {',
    '    font-size: 13px;',
    '    border-radius: 6px;',
    '    z-index: 1060;',
    '}',
  ].join('\n');

  const findings = findStyleDebt([{ file: sheet, text: css }]);

  assert.deepEqual(ruleLines(findings, 'S1'), [7]);
  assert.deepEqual(ruleLines(findings, 'S2'), [8]);
  assert.deepEqual(ruleLines(findings, 'S3'), [9]);
});

test('S2 refuses a literal radius, a fallback included, and passes tokens, inherit and zero', () => {
  const component = [
    '<template><div class="chat-dot" :style="{ borderRadius: \'8px\' }"></div></template>',
    '<style scoped>',
    '.chat-dot {',
    '    border-radius: 50%;',
    '    border-top-left-radius: 6px;',
    '    border-radius: var(--chat-r-item, 6px);',
    '    border-radius: calc(var(--chat-r-md) - 1px);',
    '    border-radius: var(--chat-r-full);',
    '    border-radius: inherit;',
    '    border-radius: 0;',
    '    border-radius: var(--chat-r-sheet) var(--chat-r-sheet) 0 0;',
    '    border-start-start-radius: var(--chat-r-sm);',
    '}',
    '</style>',
  ].join('\n');

  const findings = findStyleDebt([{ file: `${shell}/components/example.partial.obs`, text: component }]);

  assert.deepEqual(ruleLines(findings, 'S2'), [1, 4, 5, 6, 7], JSON.stringify(findings));
});

test('S3 refuses a literal layer other than auto, -1, 0 and 1, in style and script', () => {
  const component = [
    '<template><div class="chat-pop" style="z-index: 1050"></div></template>',
    '<style scoped>',
    '.chat-pop {',
    '    z-index: 1060;',
    '    z-index: 6;',
    '    z-index: var(--chat-z-popover, 50);',
    '    z-index: auto;',
    '    z-index: -1;',
    '    z-index: 0;',
    '    z-index: 1;',
    '    z-index: var(--chat-z-popover);',
    '}',
    '</style>',
    '<script setup lang="ts">',
    '    const look = { zIndex: 60 };',
    '    element.style.zIndex = "70";',
    '    const place = { zIndex: 1 };',
    '</script>',
  ].join('\n');

  const findings = findStyleDebt([{ file: `${shell}/components/example.partial.obs`, text: component }]);

  assert.deepEqual(ruleLines(findings, 'S3'), [1, 4, 5, 6, 15, 16], JSON.stringify(findings));
});

test('S4 refuses a px distance inside keyframes, in a style block, the sheet or a script animation', () => {
  const component = [
    '<style scoped>',
    '@keyframes chat-rise {',
    '    from { transform: translateY(4px); opacity: 0; }',
    '    50% { transform: translate(0, -2.5px) scale(1.02); }',
    '    to { transform: translateY(0px); }',
    '}',
    '@keyframes chat-sheet-in {',
    '    from { transform: translateY(100%); }',
    '    to { transform: translateY(var(--chat-rise-sm)); }',
    '}',
    '.chat-row { margin-top: 4px; }',
    '</style>',
    '<script setup lang="ts">',
    '    element.animate([{ transform: "translateY(8px)" }, { transform: "none" }], { duration: tokenDuration("--chat-t-msg") });',
    '    element.animate([{ opacity: 0 }, { opacity: 1 }], { duration: tokenDuration("--chat-t-msg") });',
    '</script>',
  ].join('\n');
  const css = [
    '@keyframes chat-pop {',
    '    from { transform: translateY(6px) scale(.96); }',
    '}',
    '.chat-row { margin-top: 4px; }',
  ].join('\n');

  const findings = findStyleDebt([
    { file: `${shell}/components/example.partial.obs`, text: component },
    { file: sheet, text: css },
  ]);

  assert.deepEqual(ruleLines(findings, 'S4', `${shell}/components/example.partial.obs`), [3, 4, 14], JSON.stringify(findings));
  assert.deepEqual(ruleLines(findings, 'S4', sheet), [2]);
});

test('S5 refuses keyframes outside the token sheet', () => {
  const component = [
    '<style scoped>',
    '.chat-row { animation: chat-rise var(--chat-t-msg) var(--chat-ease-in); }',
    '@keyframes chat-rise {',
    '    from { opacity: 0; }',
    '}',
    '@-webkit-keyframes chat-old {',
    '    from { opacity: 0; }',
    '}',
    '</style>',
  ].join('\n');
  const css = '@keyframes chat-spin {\n    to { transform: rotate(1turn); }\n}\n';

  const findings = findStyleDebt([
    { file: `${shell}/components/example.partial.obs`, text: component },
    { file: sheet, text: css },
  ]);

  assert.deepEqual(ruleLines(findings, 'S5', `${shell}/components/example.partial.obs`), [3, 6]);
  assert.deepEqual(ruleLines(findings, 'S5', sheet), []);
});

test('S6 refuses reaching into another component\'s styles, and passes a comment about it', () => {
  const component = [
    '<style scoped>',
    '.chat-row :deep(.chat-avatar) { margin: 0; }',
    '.chat-row ::v-deep .chat-x { margin: 0; }',
    ':global(.chat-open) .chat-row { margin: 0; }',
    '.chat-row >>> .chat-y { margin: 0; }',
    '/* :deep() was here once */',
    '.chat-row:hover { margin: 0; }',
    '</style>',
  ].join('\n');

  const findings = findStyleDebt([{ file: `${shell}/components/example.partial.obs`, text: component }]);

  assert.deepEqual(ruleLines(findings, 'S6'), [2, 3, 4, 5]);
});

test('S7 refuses !important anywhere, the token sheet and script included', () => {
  const component = [
    '<template><div class="chat-row" style="display: none !important"></div></template>',
    '<style scoped>',
    '.chat-row { width: 0 !important; }',
    '/* never !important */',
    '</style>',
    '<script setup lang="ts">',
    '    element.style.setProperty("display", "none", "important");',
    '    element.style.cssText = "display: none !important";',
    '    // not !important here',
    '</script>',
  ].join('\n');
  const css = '.chat-x { color: var(--chat-text) !important; }\n';

  const findings = findStyleDebt([
    { file: `${shell}/components/example.partial.obs`, text: component },
    { file: sheet, text: css },
  ]);

  assert.deepEqual(ruleLines(findings, 'S7', `${shell}/components/example.partial.obs`), [1, 3, 7, 8]);
  assert.deepEqual(ruleLines(findings, 'S7', sheet), [1]);
});

test('S8 refuses a reduced-motion query in a component style block, not in the sheet', () => {
  const component = [
    '<style scoped>',
    '@media (prefers-reduced-motion: reduce) {',
    '    .chat-row { animation: none; }',
    '}',
    '</style>',
    '<script setup lang="ts">',
    '    const isReduced = window.matchMedia?.("(prefers-reduced-motion: reduce)").matches ?? false;',
    '</script>',
  ].join('\n');
  const css = '@media (prefers-reduced-motion: reduce) {\n    .chat-x { animation: none; }\n}\n';

  const findings = findStyleDebt([
    { file: `${shell}/components/example.partial.obs`, text: component },
    { file: sheet, text: css },
  ]);

  assert.deepEqual(ruleLines(findings, 'S8', `${shell}/components/example.partial.obs`), [2]);
  assert.deepEqual(ruleLines(findings, 'S8', sheet), []);
});

test('S9 refuses a token nothing reads; S10 reports a token read once', () => {
  const css = [
    ':root {',
    '    --chat-used: 1px;',
    '    --chat-unused: 2px;',
    '    --chat-read-by-script: 200ms;',
    '    --chat-tone-brand: var(--chat-brand);',
    '    --chat-alias: var(--chat-base);',
    '    --chat-base: 4px;',
    '}',
    '[data-theme="dark"] .chat-shell {',
    '    --chat-unused: 3px;',
    '}',
    '.chat-a { margin: var(--chat-used); }',
    '.chat-b { padding: var(--chat-used); }',
  ].join('\n');
  const component = '<style scoped>\n.chat-row { width: var(--chat-used); }\n</style>\n';
  const script = [
    'const fast = tokenDuration("--chat-read-by-script");',
    'const tone = `var(--chat-tone-${kind})`;',
    'element.style.setProperty("--chat-unused", "1px");',
  ].join('\n');

  const findings = findStyleDebt([
    { file: sheet, text: css },
    { file: `${shell}/components/example.partial.obs`, text: component },
    { file: `${shell}/composables/example.partial.ts`, text: script },
  ]);

  assert.deepEqual(ruleNames(findings, 'S9', /--[\w-]+/), ['--chat-alias', '--chat-unused'], JSON.stringify(findings));
  assert.deepEqual(ruleLines(findings, 'S9', sheet), [3, 6]);
  assert.deepEqual(ruleNames(findings, 'S10', /--[\w-]+/), ['--chat-base', '--chat-read-by-script'], JSON.stringify(findings));
});

test('S11 reports a chat class the sheet styles that no chat file uses', () => {
  const css = [
    '.chat-orphan { margin: 0; }',
    '.panel-body .chat-used { margin: 0; }',
  ].join('\n');
  const component = '<template><div class="chat-used"></div></template>\n';

  const findings = findStyleDebt([
    { file: sheet, text: css },
    { file: `${shell}/components/example.partial.obs`, text: component },
  ]);

  assert.deepEqual(ruleNames(findings, 'S11', /chat-[\w-]+/), ['chat-orphan']);
});

test('S12 refuses restyling a part from outside it, and passes layout around it and shared classes', () => {
  const avatar = [
    '<template>',
    '    <span class="chat-avatar"><span class="chat-avatar-initials chat-sr-only">A</span></span>',
    '</template>',
    '<style scoped>',
    '.chat-avatar { display: inline-flex; }',
    '</style>',
  ].join('\n');
  const iconButton = [
    '<template>',
    '    <button class="chat-icon-btn" type="button"><slot /></button>',
    '</template>',
  ].join('\n');
  const row = [
    '<template>',
    '    <div class="chat-row">',
    '        <Avatar class="chat-row-face" />',
    '        <span class="chat-sr-only">x</span>',
    '    </div>',
    '</template>',
    '<style scoped>',
    '.chat-row :deep(.chat-avatar) { margin: 0; }',
    '.chat-row .chat-avatar-initials { margin: 0; }',
    '.chat-avatar .chat-row-face { margin: 0; }',
    '.chat-row-face { margin: 0; }',
    '</style>',
  ].join('\n');
  const css = [
    '.chat-avatar { width: 0; }',
    '.chat-sr-only { position: absolute; }',
    'button.chat-icon-btn.chat-fail-btn { margin: 0; }',
  ].join('\n');

  const findings = findStyleDebt([
    { file: `${shell}/ui/avatar.partial.obs`, text: avatar },
    { file: `${shell}/ui/iconButton.partial.obs`, text: iconButton },
    { file: `${shell}/components/row.partial.obs`, text: row },
    { file: sheet, text: css },
  ]);

  assert.deepEqual(ruleLines(findings, 'S12', `${shell}/components/row.partial.obs`), [8, 9], JSON.stringify(findings));
  assert.deepEqual(ruleLines(findings, 'S12', sheet), [1, 3], JSON.stringify(findings));
  assert.deepEqual(ruleLines(findings, 'S12', `${shell}/ui/avatar.partial.obs`), []);
});

/** Writes a debt file into a fresh folder and returns its path. */
function debtFile(entries) {
  const folder = mkdtempSync(join(tmpdir(), 'chat-debt-'));
  const path = join(folder, 'known-debt.json');
  writeFileSync(path, JSON.stringify(entries));
  return path;
}

const ratchetRow = 'Dev Tools/chat-ci/check-styles-fixtures/ratchet/components/row.partial.obs';

function runRatchet(debt, landing, ...extra) {
  return spawnSync(process.execPath, ['check-styles.mjs', '--root', 'check-styles-fixtures/ratchet', '--debt', debt, ...extra], {
    cwd: here,
    encoding: 'utf8',
    env: { ...process.env, CHAT_DEBT_LANDING: landing ? '1' : '0', GITHUB_STEP_SUMMARY: '' },
  });
}

test('run on the ratchet fixture with its exact debt, it passes and prints the table', () => {
  const result = runRatchet(debtFile({ S1: { [ratchetRow]: 1 }, S2: { [ratchetRow]: 1 } }), true);

  assert.equal(result.status, 0, result.stdout + result.stderr);
  assert.match(result.stdout, /\|\s*S1\s*\|\s*ratchet\s*\|\s*1\s*\|\s*1\s*\|/);
});

test('run on the ratchet fixture with less debt recorded than found, it fails', () => {
  const result = runRatchet(debtFile({ S1: { [ratchetRow]: 1 } }), false);

  assert.equal(result.status, 1, result.stdout + result.stderr);
  assert.match(result.stderr, /S2/);
});

test('run on the ratchet fixture with more debt recorded than found, it warns on a branch and fails where it lands', () => {
  const debt = debtFile({ S1: { [ratchetRow]: 3 }, S2: { [ratchetRow]: 1 } });

  const branch = runRatchet(debt, false);
  assert.equal(branch.status, 0, branch.stdout + branch.stderr);
  assert.match(branch.stdout + branch.stderr, /lower/i);

  const landing = runRatchet(debt, true);
  assert.equal(landing.status, 1, landing.stdout + landing.stderr);
});

test('run on the ratchet fixture, it fails a debt file that grew against its base', () => {
  const debt = debtFile({ S1: { [ratchetRow]: 1 }, S2: { [ratchetRow]: 1 } });
  const base = debtFile({ S1: { [ratchetRow]: 1 } });

  const result = runRatchet(debt, false, '--base-debt', base);

  assert.equal(result.status, 1, result.stdout + result.stderr);
  assert.match(result.stderr, /rose|grew/i);
});

test('the debt table is written to the step summary when the workflow gives one', () => {
  const summary = join(mkdtempSync(join(tmpdir(), 'chat-summary-')), 'summary.md');
  const debt = debtFile({ S1: { [ratchetRow]: 1 }, S2: { [ratchetRow]: 1 } });

  const result = spawnSync(process.execPath, ['check-styles.mjs', '--root', 'check-styles-fixtures/ratchet', '--debt', debt], {
    cwd: here,
    encoding: 'utf8',
    env: { ...process.env, CHAT_DEBT_LANDING: '0', GITHUB_STEP_SUMMARY: summary },
  });

  assert.equal(result.status, 0, result.stdout + result.stderr);
  assert.match(readFileSync(summary, 'utf8'), /\|\s*S2\s*\|\s*ratchet\s*\|\s*1\s*\|\s*1\s*\|/);
});
