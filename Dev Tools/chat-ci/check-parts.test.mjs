// Every behaviour in the chat has one home, and every type and every set of allowed values is
// defined once. These cells plant the real spellings found in the chat today, one rule at a time,
// and assert both that the rule finds them and that it passes the look-alikes beside them. The
// fixtures beside this file prove the check bites when it is run the way CI runs it.
import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { mkdtempSync, readFileSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, resolve } from 'node:path';
import test from 'node:test';
import { fileURLToPath } from 'node:url';

import { findPartDebt, partFiles, PART_RULES } from './check-parts.mjs';

const here = fileURLToPath(new URL('.', import.meta.url));
const repoRoot = fileURLToPath(new URL('../../', import.meta.url));

function readFixture(file) {
  return readFileSync(resolve(repoRoot, file), 'utf8');
}
const shell = 'Rock.JavaScript.Obsidian.Blocks/src/Communication/Chat/ChatShell';

/** The lines one rule found, in one file when named, sorted. */
function ruleLines(findings, rule, file) {
  return findings.filter((found) => found.rule === rule && (!file || found.file === file)).map((found) => found.line).sort((a, b) => a - b);
}

/** The files one rule found anything in, sorted. */
function ruleFiles(findings, rule) {
  return [...new Set(findings.filter((found) => found.rule === rule).map((found) => found.file))].sort();
}

function one(file, text) {
  return findPartDebt([{ file, text }]);
}

test('every parts rule carries its mode and its reason', () => {
  const ids = PART_RULES.map((rule) => rule.id);

  assert.deepEqual(ids, ['P1', 'P2', 'P3', 'P4', 'P5', 'P6', 'P7', 'P8', 'P9', 'P10', 'P15', 'T1', 'T2']);
  for (const rule of PART_RULES) {
    assert.match(rule.mode, /^(ratchet|report)$/, rule.id);
    assert.ok(rule.reason.length > 20, `${rule.id} states no reason`);
  }
  assert.deepEqual(PART_RULES.filter((rule) => rule.mode === 'report').map((rule) => rule.id), ['P1', 'P7']);
});

test('it reads the chat block and every script and component under its folder, not the token sheet', () => {
  const files = partFiles();

  assert.ok(files.includes('Rock.JavaScript.Obsidian.Blocks/src/Communication/Chat/chatShell.obs'));
  assert.ok(files.includes(`${shell}/types.partial.ts`));
  assert.ok(files.includes(`${shell}/ui/menu.partial.obs`));
  assert.ok(!files.some((file) => file.endsWith('.css')));
});

test('P1 reports a raw button in a screen, not in a part', () => {
  const screen = '<template>\n    <div>\n        <button type="button" class="chat-link" @click="open">Open</button>\n    </div>\n</template>\n';
  const part = '<template>\n    <button class="chat-icon-btn" type="button"><slot /></button>\n</template>\n';

  assert.deepEqual(ruleLines(one(`${shell}/components/joinBar.partial.obs`, screen), 'P1'), [3]);
  assert.deepEqual(ruleLines(one(`${shell}/ui/iconButton.partial.obs`, part), 'P1'), []);
});

test('P2 refuses floating-ui outside the popover part, by the import Rock gives it', () => {
  const component = [
    '<script setup lang="ts">',
    '    import { autoUpdate, flip, offset, shift, useFloating } from "@Obsidian/Libs/floating-ui";',
    '    import { computePosition } from "@floating-ui/dom";',
    '    // floating-ui is what places the card',
    '</script>',
  ].join('\n');
  const tooltip = 'import { autoUpdate, flip, offset, shift, useFloating } from "@Obsidian/Libs/floating-ui";\n';

  assert.deepEqual(ruleLines(one(`${shell}/components/mentionPicker.partial.obs`, component), 'P2'), [2, 3]);
  assert.deepEqual(ruleLines(one(`${shell}/ui/tooltip.partial.ts`, tooltip), 'P2'), [1]);
  assert.deepEqual(ruleLines(one(`${shell}/ui/popover.partial.ts`, tooltip), 'P2'), []);
});

test('P3 refuses a press listener on the document outside the popover, scrim and dialog', () => {
  const script = [
    'document.addEventListener("pointerdown", onDocumentPointerDown, true);',
    'document.addEventListener("mousedown", onDocumentPress, true);',
    'document.addEventListener("keydown", onKeydown, true);',
    'targets.document.addEventListener("visibilitychange", onVisibility);',
    'targets.window.addEventListener(type, onInput, { capture: true, passive: true });',
    'document.removeEventListener("pointerdown", onDocumentPointerDown, true);',
  ].join('\n');

  assert.deepEqual(ruleLines(one(`${shell}/components/profileCard.partial.ts`, script), 'P3'), [1, 2]);
  for (const owner of ['ui/popover.partial.ts', 'ui/scrim.partial.ts', 'ui/dialog.partial.ts']) {
    assert.deepEqual(ruleLines(one(`${shell}/${owner}`, script), 'P3'), [], owner);
  }
});

test('P4 refuses a media query read outside the viewport part, optional call included', () => {
  const script = [
    'const isTouch = window.matchMedia?.("(hover: none)").matches ?? false;',
    'const isHoverless = typeof window !== "undefined" && typeof window.matchMedia === "function"',
    '    && window.matchMedia("(hover: none)").matches;',
    'const isReduced = window.matchMedia?.("(prefers-reduced-motion: reduce)").matches;',
    '// window.matchMedia("(hover: none)") is read once, in the viewport part',
  ].join('\n');

  assert.deepEqual(ruleLines(one(`${shell}/composables/useFrame.partial.ts`, script), 'P4'), [1, 3, 4]);
  assert.deepEqual(ruleLines(one(`${shell}/ui/viewport.partial.ts`, script), 'P4'), []);
});

test('P5 refuses a live region outside the announcer, the roles that make one included', () => {
  const component = [
    '<template>',
    '    <div class="chat-sr-only" aria-live="polite">{{ said }}</div>',
    '    <div :aria-live="isOpen ? \'polite\' : \'off\'"></div>',
    '    <!-- aria-live is announced once, by the announcer -->',
    '    <div class="chat-toast" role="alert">{{ text }}</div>',
    '    <p class="chat-field-error" role=\'status\'>{{ error }}</p>',
    '    <div role="dialog" aria-modal="true"></div>',
    '    <div :role="isOpen ? \'menu\' : undefined"></div>',
    '    <div :role="connectionBanner === \'warning\' ? \'alert\' : \'status\'">{{ banner }}</div>',
    '    <div v-bind:role="\'status\'"></div>',
    '</template>',
    '<script setup lang="ts">',
    '    element.setAttribute("aria-live", "polite");',
    '    const role = "status";',
    '</script>',
  ].join('\n');

  assert.deepEqual(ruleLines(one(`${shell}/components/composer.partial.obs`, component), 'P5'), [2, 3, 5, 6, 9, 10, 13]);
  assert.deepEqual(ruleLines(one(`${shell}/ui/announcer.partial.obs`, component), 'P5'), []);
});

test('P6 refuses a new intersection observer outside the paging, seen and image-reveal owners', () => {
  const script = [
    'let observer: IntersectionObserver | null = null;',
    'if (typeof IntersectionObserver === "undefined") return;',
    'observer = new IntersectionObserver(entries => {',
    '});',
  ].join('\n');

  assert.deepEqual(ruleLines(one(`${shell}/components/threadsPanel.partial.ts`, script), 'P6'), [3]);
  for (const owner of ['composables/usePagingTrigger.partial.ts', 'composables/useSeenObserver.partial.ts', 'ui/imageReveal.partial.ts']) {
    assert.deepEqual(ruleLines(one(`${shell}/${owner}`, script), 'P6'), [], owner);
  }
});

test('P7 reports a hand-rolled generation counter outside the latest-search and save owners', () => {
  const script = [
    'let searchGeneration = 0;',
    'const searches = { search: { generation: 0, timer: null } };',
    'const rightsGenerations = new Map<string, number>();',
    'const generation = ++searchGeneration;',
    'if (generation !== searchGeneration) return;',
  ].join('\n');

  assert.deepEqual(ruleLines(one(`${shell}/composables/useMembers.partial.ts`, script), 'P7'), [1, 2, 3]);
  assert.deepEqual(ruleLines(one(`${shell}/composables/saveRule.partial.ts`, script), 'P7'), []);
  assert.deepEqual(ruleLines(one(`${shell}/composables/useLatestSearch.partial.ts`, script), 'P7'), []);
});

test('P8 refuses Tab handling outside the focus trap, and passes the reviewed exceptions', () => {
  const script = [
    'if (event.key === "Escape") close();',
    'else if (event.key === "Tab") {',
    'switch (event.key) { case \'Tab\': trap(); }',
    'type Tab = "all" | "following";',
    'const tab = ref<Tab>("all");',
    'const label = "Tab";',
    'if (event.key !== "Tab") return;',
    'if ("Tab" !== e.code) return;',
    'if (["Tab", "Enter"].includes(event.key)) pick();',
    'if (new Set(["Tab"]).has(event.key)) trap();',
  ].join('\n');

  assert.deepEqual(ruleLines(one(`${shell}/ui/menu.partial.ts`, script), 'P8'), [2, 3, 7, 8, 9, 10]);
  for (const allowed of ['ui/focusTrap.partial.ts', 'components/mentionPicker.partial.ts', 'ui/tooltip.partial.ts', 'ui/peoplePicker.partial.ts']) {
    assert.deepEqual(ruleLines(one(`${shell}/${allowed}`, script), 'P8'), [], allowed);
  }
});

test('P9 refuses the phone flag or the person passed down as a prop or a binding', () => {
  const component = [
    '<template>',
    '    <MessageBody :text="text" :personAliasGuid="personAliasGuid" />',
    '    <ChannelIntro',
    '        :is-phone="isPhone"',
    '        :isPhone="isPhone" />',
    '    <Row v-bind:person-alias-guid="me" />',
    '</template>',
    '<script setup lang="ts">',
    '    const props = defineProps({',
    '        isPhone: {',
    '            type: Boolean,',
    '            default: false',
    '        },',
    '        personAliasGuid: { type: String, required: true }',
    '    });',
    '    const row = { isPhone: isTouchLayout.value, personAliasGuid: props.personAliasGuid };',
    '</script>',
  ].join('\n');
  const composable = 'export type FrameOptions = {\n    isPhone: Ref<boolean>;\n    personAliasGuid: string;\n};\n';

  assert.deepEqual(ruleLines(one(`${shell}/components/blockCard.partial.obs`, component), 'P9'), [2, 4, 5, 6, 10, 14]);
  assert.deepEqual(ruleLines(one(`${shell}/composables/useFrame.partial.ts`, composable), 'P9'), []);
});

test('P10 refuses finding an element by its test id in production code', () => {
  const script = [
    'const place = rootElement.value?.querySelector("[data-testid=\'frame-main\']");',
    'const bar = rootElement.value?.querySelector("[data-testid=\'frame-main\'] [data-testid=\'join-bar\']");',
    'return !!element.closest("[data-testid=\'main-view\']");',
    'const rows = list.querySelectorAll(`[data-testid="row"]`);',
    'const row = list.querySelector(".chat-row");',
  ].join('\n');
  const template = '<template>\n    <div data-testid="frame-main"></div>\n</template>\n';

  assert.deepEqual(ruleLines(one(`${shell}/composables/useMessageActionsView.partial.ts`, script), 'P10'), [1, 2, 3, 4]);
  assert.deepEqual(ruleLines(one(`${shell}/components/x.partial.obs`, template), 'P10'), []);
});

// A lookup typed with the element it expects is the same lookup: the type between the name and
// its bracket must not hide it.
test('P10 refuses a typed lookup by test id, as an untyped one', () => {
  const script = [
    'const input = rootElement.value?.querySelector<HTMLInputElement>("[data-testid=\'composer-input\']");',
    'const rows = list.querySelectorAll<HTMLElement>(`[data-testid="row"]`);',
    'const view = element.closest<HTMLElement>("[data-testid=\'main-view\']");',
    'const tile = grid.querySelector < HTMLButtonElement > ("[data-testid=\'gif-topic\']");',
    'const row = list.querySelector<HTMLElement>(".chat-row");',
    'const mark = list.querySelector<HTMLElement>("[data-focus-stop=\'member\']");',
  ].join('\n');

  assert.deepEqual(ruleLines(one(`${shell}/composables/useMessageActionsView.partial.ts`, script), 'P10'), [1, 2, 3, 4]);
});

test('P15 refuses looking a room up in the channel rows outside the channel store, not other rows', () => {
  const script = [
    'const row = shell.value.channels.rows.find(r => r.channel_id === channelId);',
    'const index = channels.rows.findIndex(r => r.channel_id === channelId);',
    'const name = state.directory.rows.find(row => row.channel_id === channelId)?.name;',
    'const held = rows.find(r => r.localId === key);',
    'const root = state.list.rows.find(r => r.channel_id === channelId && r.root.id === rootId);',
    'const isHeld = dependencies.channels.rows.some(row => row.channel_id === channelId);',
    'const back = channels.rows.find(r => channelId === r.channel_id);',
  ].join('\n');

  assert.deepEqual(ruleLines(one(`${shell}/composables/useRooms.partial.ts`, script), 'P15'), [1, 2, 6, 7]);
  assert.deepEqual(ruleLines(one(`${shell}/stores/channelStore.partial.ts`, script), 'P15'), []);
});

test('T1 refuses one type name defined in two files, whatever its kind', () => {
  const panel = [
    '<script setup lang="ts">',
    '    import { RoomKind } from "../types.partial";',
    '    type DrawnRow = { key: string };',
    '    export interface Section { title: string }',
    '    const kind = { type: String as PropType<RoomKind> };',
    '    import type { Tab } from "./tabs";',
    '</script>',
  ].join('\n');
  const directory = 'export type DrawnRow = {\n    key: string;\n};\nexport enum Section { One }\ntype OnlyHere = string;\n';

  const findings = findPartDebt([
    { file: `${shell}/components/mentionsPanel.partial.obs`, text: panel },
    { file: `${shell}/components/channelDirectory.partial.ts`, text: directory },
  ]);

  assert.deepEqual(ruleLines(findings, 'T1', `${shell}/components/mentionsPanel.partial.obs`), [3, 4], JSON.stringify(findings));
  assert.deepEqual(ruleLines(findings, 'T1', `${shell}/components/channelDirectory.partial.ts`), [1, 4]);
  assert.ok(findings.filter((found) => found.rule === 'T1').every((found) => /DrawnRow|Section/.test(found.what)));
});

test('T1 passes a name on the reviewed list of unrelated names', () => {
  const files = [
    { file: `${shell}/components/a.partial.ts`, text: 'type Props = { a: string };\n' },
    { file: `${shell}/components/b.partial.ts`, text: 'type Props = { b: number };\n' },
  ];

  assert.deepEqual(ruleFiles(findPartDebt(files), 'T1'), [`${shell}/components/a.partial.ts`, `${shell}/components/b.partial.ts`]);
  assert.deepEqual(ruleFiles(findPartDebt(files, { unrelatedTypeNames: new Map([['Props', 'each component\'s own inputs']]) }), 'T1'), []);
});

test('T2 refuses one set of allowed values spelled out in two files, under any name or none', () => {
  const types = 'export type RoomKind = "channel" | "person" | "group";\n';
  const header = [
    '<script setup lang="ts">',
    '    export type HeaderRoomKind = "channel" | "person" | "group";',
    '</script>',
  ].join('\n');
  const panel = [
    'const props = {',
    '    kind: { type: String as PropType<"group" | "channel" | "person" | null> },',
    '    side: { type: String as PropType<',
    '        | "start"',
    '        | "end">',
    '    },',
    '    pair: String as PropType<"channel" | "person">,',
    '    mixed: String as PropType<RoomKind | "none" | "all">,',
    '};',
  ].join('\n');
  const other = 'type Side = "start" | "middle";\nconst blank = "a" + "b";\n';

  const findings = findPartDebt([
    { file: `${shell}/types.partial.ts`, text: types },
    { file: `${shell}/components/chatHeader.partial.obs`, text: header },
    { file: `${shell}/components/threadsPanel.partial.ts`, text: panel },
    { file: `${shell}/components/other.partial.ts`, text: other },
  ]);

  assert.deepEqual(ruleFiles(findings, 'T2'), [
    `${shell}/components/chatHeader.partial.obs`,
    `${shell}/components/threadsPanel.partial.ts`,
    `${shell}/types.partial.ts`,
  ], JSON.stringify(findings));
  assert.deepEqual(ruleLines(findings, 'T2', `${shell}/components/threadsPanel.partial.ts`), [2]);
  assert.match(findings.find((found) => found.rule === 'T2').what, /channel.*group.*person|"channel" \| "group" \| "person"/);
});

test('T2 leaves the keys Pick and Omit take, which name fields rather than allowed values', () => {
  const a = 'type Draft = Pick<Storage, "getItem" | "setItem">;\ntype Face = Omit<Person,\n    | "nick_name"\n    | "last_name">;\n';
  const b = 'function store(storage: Pick<Storage, "setItem" | "getItem">, face: Omit<Person, "last_name" | "nick_name">) {}\n';

  const findings = findPartDebt([
    { file: `${shell}/a.partial.ts`, text: a },
    { file: `${shell}/b.partial.ts`, text: b },
  ]);

  assert.deepEqual(ruleFiles(findings, 'T2'), [], JSON.stringify(findings));
});

test('T2 reads a union written over several lines as one set', () => {
  const a = 'type Place =\n    | "above"\n    | "below";\n';
  const b = 'function place(side: "below" | "above") {}\n';

  const findings = findPartDebt([
    { file: `${shell}/a.partial.ts`, text: a },
    { file: `${shell}/b.partial.ts`, text: b },
  ]);

  assert.deepEqual(ruleLines(findings, 'T2', `${shell}/a.partial.ts`), [1]);
  assert.deepEqual(ruleLines(findings, 'T2', `${shell}/b.partial.ts`), [1]);
});

/** Writes a debt file into a fresh folder and returns its path. */
function debtFile(entries) {
  const folder = mkdtempSync(join(tmpdir(), 'chat-debt-'));
  const path = join(folder, 'known-debt.json');
  writeFileSync(path, JSON.stringify(entries));
  return path;
}

function run(...args) {
  return spawnSync(process.execPath, ['check-parts.mjs', ...args], {
    cwd: here,
    encoding: 'utf8',
    env: { ...process.env, CHAT_DEBT_LANDING: '0', GITHUB_STEP_SUMMARY: '' },
  });
}

const fixture = 'Dev Tools/chat-ci/check-parts-fixtures/red';

test('run on the red fixtures with no debt, it fails and names each rule', () => {
  const result = run('--root', 'check-parts-fixtures/red', '--debt', debtFile({}));

  assert.equal(result.status, 1, result.stdout + result.stderr);
  for (const rule of ['P2', 'P3', 'P4', 'P5', 'P6', 'P8', 'P9', 'P10', 'P15', 'T1', 'T2']) {
    assert.match(result.stderr, new RegExp(`\\b${rule}\\b`), `${rule} should bite: ${result.stderr}`);
  }
});

test('run on the red fixtures with their debt recorded, it passes and reports the rest', () => {
  const findings = findPartDebt(partFiles(`${here}check-parts-fixtures/red`).map((file) => ({ file, text: readFixture(file) })));
  const debt = {};
  for (const found of findings) {
    if (PART_RULES.find((rule) => rule.id === found.rule).mode !== 'ratchet') continue;
    debt[found.rule] ??= {};
    debt[found.rule][found.file] = (debt[found.rule][found.file] ?? 0) + 1;
  }
  assert.ok(Object.keys(debt).length >= 11, JSON.stringify(debt));
  assert.ok(Object.values(debt).every((files) => Object.keys(files).every((file) => file.startsWith(fixture))));

  const result = run('--root', 'check-parts-fixtures/red', '--debt', debtFile(debt));

  assert.equal(result.status, 0, result.stdout + result.stderr);
  assert.match(result.stdout, /\|\s*P1\s*\|\s*report\s*\|/);
});

test('run on the green fixtures with no debt, it passes', () => {
  const result = run('--root', 'check-parts-fixtures/green', '--debt', debtFile({}));

  assert.equal(result.status, 0, result.stdout + result.stderr);
});

test('the chat files in this branch hold no more than their recorded debt', () => {
  const result = run();

  assert.equal(result.status, 0, result.stdout + result.stderr);
});
