// Every behaviour in the chat has one home, and a screen uses it rather than rebuilding it. When
// two screens each place a popover, trap focus or page a list in their own way, a fix lands in
// one and the other keeps the old behaviour, and a reader cannot tell which copy is meant. This
// check finds each rebuilt behaviour by the call that gives it away, outside the part that owns it:
//
//   P1  a raw <button> in a screen, where the button parts give one look and one set of states
//       (report only: some screens still need their own control while the parts are built)
//   P2  floating-ui imported outside the popover part, which owns placing a floating surface
//   P3  a press listener on the document outside the popover, scrim and dialog, which own closing
//       a surface on a press outside it
//   P4  a media query read outside the viewport part, which owns phone, hover and reduced motion
//   P5  a live region outside the announcer, since two live regions talking at once drown each
//       other out and a screen reader reads neither well
//   P6  a new IntersectionObserver outside the paging, seen and image-reveal owners
//   P7  a generation counter outside the latest-search and save owners, the hand-rolled way of
//       dropping a stale answer (report only: some are not searches)
//   P8  Tab handled outside the focus trap, which owns keeping focus inside a surface; three
//       files handle Tab for a different job and are listed below with theirs
//   P9  the phone flag or the signed-in person passed down as a prop or a binding, where the
//       session context provides them once to every component that reads them
//   P10 an element found by its test id in production code, which ties the product to its tests
//       and breaks when a test id moves
//   P15 a room looked up by scanning the rows outside the channel store, which owns the lookup
//
// And two rules for the names code uses, since one meaning with two homes drifts apart:
//
//   T1  one type name defined in two files. A type two files use lives once, in types.partial.ts;
//       a type one file uses stays in that file. A name that two files define for unrelated jobs
//       is allowed only from the reviewed list below, with its reason.
//   T2  one set of allowed string values spelled out in two files, under any name or none. The
//       set is a string union defined once and referred to by its name.
//
// The style rules sit in check-styles.mjs, beside the literals they share a parser with. Both
// scripts hold today's code to the counts recorded in known-debt.json, which may only fall; see
// known-debt.mjs for how.
//
//   node check-parts.mjs                 checks the chat block and every script and component under its folder
//   node check-parts.mjs --root <dir>    checks every component under a folder, which is how the test proves it bites
//   node check-parts.mjs --record        records today's counts in known-debt.json
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

import { codeOf, lineFinder, regionsOf, scriptStrings, styleFiles, walk } from './check-styles.mjs';
import { runRatchet } from './known-debt.mjs';

const repoRoot = fileURLToPath(new URL('../../', import.meta.url));

/** The rules, each with the reason a reader here needs, in the order the table prints them. */
export const PART_RULES = [
  { id: 'P1', mode: 'report', reason: 'a screen draws a button with the button parts, so every button has one look and one set of states' },
  { id: 'P2', mode: 'ratchet', reason: 'placing a floating surface has one home, the popover part' },
  { id: 'P3', mode: 'ratchet', reason: 'closing a surface on a press outside it has one home: the popover, the scrim and the dialog' },
  { id: 'P4', mode: 'ratchet', reason: 'reading phone, hover and reduced motion has one home, the viewport part' },
  { id: 'P5', mode: 'ratchet', reason: 'announcing has one home, the announcer, so two live regions never talk over each other' },
  { id: 'P6', mode: 'ratchet', reason: 'watching what scrolls into view has three homes: paging, marking seen and revealing an image' },
  { id: 'P7', mode: 'report', reason: 'dropping a stale answer has one home, the latest-search helper, and saving one, the save rule' },
  { id: 'P8', mode: 'ratchet', reason: 'keeping focus inside a surface has one home, the focus trap' },
  { id: 'P9', mode: 'ratchet', reason: 'the phone flag and the signed-in person are provided once by the session context, never passed down' },
  { id: 'P10', mode: 'ratchet', reason: 'production code never finds an element by its test id, which belongs to the tests' },
  { id: 'P15', mode: 'ratchet', reason: 'finding a room\'s row has one home, the channel store' },
  { id: 'T1', mode: 'ratchet', reason: 'a type has one home: types.partial.ts when two files use it, its own file when one does' },
  { id: 'T2', mode: 'ratchet', reason: 'a set of allowed values is one string union, defined once and referred to by name' },
];

/**
 * The files that own each behaviour, by their path below the chat's folder without the extension.
 * A part named here that does not exist yet is the home the step building it uses; that step may
 * rename it in the same change.
 */
const OWNERS = {
  P2: ['ui/popover.partial'],
  P3: ['ui/popover.partial', 'ui/scrim.partial', 'ui/dialog.partial'],
  P4: ['ui/viewport.partial'],
  P5: ['ui/announcer.partial'],
  P6: ['composables/usePagingTrigger.partial', 'composables/useSeenObserver.partial', 'ui/imageReveal.partial'],
  P7: ['composables/useLatestSearch.partial', 'composables/saveRule.partial'],
  P8: [
    'ui/focusTrap.partial',
    // Completing a mention on Tab, as Enter does, is the picker's own key, not a trap.
    'components/mentionPicker.partial',
    // An interactive tip moves focus from its anchor into the tip and back out in reading order.
    'ui/tooltip.partial',
    // Tab hands the person picker's key to the dialog holding it, which decides what comes next.
    'ui/peoplePicker.partial',
  ],
  P15: ['stores/channelStore.partial'],
};

/**
 * Type names two files may each define because the jobs are unrelated, each with its reason. A
 * name goes here only after a review agrees the two are not one meaning.
 */
export const UNRELATED_TYPE_NAMES = new Map([]);

/** Whether a file is one of a rule's owners. */
function isOwner(rule, file) {
  return (OWNERS[rule] ?? []).some((stem) => file.endsWith(`/${stem}.ts`) || file.endsWith(`/${stem}.obs`));
}

/** The pattern each per-file rule finds, and where in a file it looks. */
const PATTERNS = [
  { rule: 'P2', where: 'script', re: /["'](?:@Obsidian\/Libs\/floating-ui|@floating-ui\/[\w-]+)["']/g, what: 'floating-ui imported' },
  { rule: 'P3', where: 'script', re: /\bdocument\s*\.\s*addEventListener\(\s*["'](?:pointerdown|pointerup|mousedown|mouseup|touchstart|touchend|click)["']/g, what: 'a press listener on the document' },
  { rule: 'P4', where: 'all', re: /\bmatchMedia\s*(?:\?\.)?\s*\(/g, what: 'a media query read' },
  { rule: 'P5', where: 'all', re: /(?<![\w-])aria-live(?![\w-])/g, what: 'a live region' },
  { rule: 'P6', where: 'script', re: /\bnew\s+IntersectionObserver\s*\(/g, what: 'an IntersectionObserver' },
  { rule: 'P7', where: 'script', re: /\b(?:let|var|const)\s+\w*[Gg]eneration\w*\s*(?::[^=]+)?=\s*(?:0\b|new\s+Map)|\b\w*[Gg]eneration\s*:\s*0\b/g, what: 'a generation counter' },
  { rule: 'P8', where: 'script', re: /\b(?:key|code)\s*===?\s*(["'])Tab\1|(["'])Tab\2\s*===?\s*[\w.]*\b(?:key|code)\b|\bcase\s+(["'])Tab\3\s*:/g, what: 'Tab handled' },
  { rule: 'P9', where: 'template', re: /\s(?::|v-bind:)(?:isPhone|is-phone|personAliasGuid|person-alias-guid)\s*=/g, what: 'passed down as a binding' },
  { rule: 'P9', where: 'obs-script', re: /\b(?:isPhone|personAliasGuid)\s*:\s*\{\s*type\s*:/g, what: 'declared as a prop' },
  { rule: 'P10', where: 'script', re: /\b(?:querySelector(?:All)?|closest)\s*\(\s*(["'`])[^"'`]*\[data-testid/g, what: 'an element found by its test id' },
  { rule: 'P15', where: 'script', re: /\brows\s*\.\s*find(?:Index)?\s*\(\s*\(?\s*(\w+)\s*\)?\s*=>[^\n]*?\b\1\s*\.\s*channel_id\s*===/g, what: 'a room looked up in the rows' },
];

const TYPE_DEFINITION = /(?:^|[\s;{}])(?:export\s+)?(?:declare\s+)?(?:type|interface|(?:const\s+)?enum)\s+([A-Z]\w*)\s*(?:[=<{]|extends\b)/g;

const STRING = String.raw`"(?:[^"\\\n]|\\.)*"|'(?:[^'\\\n]|\\.)*'`;
const MEMBER = String.raw`(?:${STRING}|null|undefined)`;
const UNION = new RegExp(String.raw`(\|\s*)?${MEMBER}(?:\s*\|\s*${MEMBER})+`, 'g');

/**
 * Each union of string values in a stretch of code whose comments are blanked: the sorted set and
 * where it starts. null and undefined members are not values and are left out of the set. A union
 * that also holds a type by name is part of a larger set, not a set of its own, so it is skipped.
 */
function unionsIn(code, start, end) {
  const out = [];
  const stretch = code.slice(start, end);
  for (const match of stretch.matchAll(UNION)) {
    const before = stretch.slice(0, match.index).trimEnd();
    const after = stretch.slice(match.index + match[0].length).trimStart();
    if (after.startsWith('|')) continue;
    // A leading pipe opens a union after =, <, (, a colon or a comma; any other neighbour is a type.
    if (match[1] ? !/(^|[=<(:,[])$/.test(before) : before.endsWith('|')) continue;
    const values = [...new Set([...match[0].matchAll(new RegExp(STRING, 'g'))].map((one) => one[0].slice(1, -1)))].sort();
    // A union opened by a pipe on its own line is reported where it is named, on the line above.
    if (values.length >= 2) out.push({ values, offset: start + (match[1] ? Math.max(0, before.length - 1) : match.index) });
  }
  return out;
}

/**
 * Everything the chat files hold against the parts rules, as findings of rule, file, line and
 * what. Each file is read alone for the behaviours, then all of them together for the types.
 *
 * @param {{ file: string, text: string }[]} files
 * @param {{ unrelatedTypeNames?: Map<string, string> }} [options]
 */
export function findPartDebt(files, { unrelatedTypeNames = UNRELATED_TYPE_NAMES } = {}) {
  const findings = [];
  const types = new Map();
  const unions = new Map();

  for (const { file, text } of files) {
    if (file.endsWith('.css')) continue;
    const code = codeOf(file, text);
    const lineOf = lineFinder(text);
    const { scripts, template } = regionsOf(file, text);
    const ranges = {
      script: scripts,
      'obs-script': file.endsWith('.obs') ? scripts : [],
      template: template ? [template] : [],
      all: [...scripts, ...(template ? [template] : [])],
    };

    if (file.includes('/components/') && template) {
      for (const match of code.slice(template.start, template.end).matchAll(/<button(?![\w-])/g)) {
        findings.push({ rule: 'P1', file, line: lineOf(template.start + match.index), what: 'a raw button' });
      }
    }

    for (const { rule, where, re, what } of PATTERNS) {
      if (isOwner(rule, file)) continue;
      for (const { start, end } of ranges[where]) {
        for (const match of code.slice(start, end).matchAll(re)) {
          findings.push({ rule, file, line: lineOf(start + match.index), what: `${what}: ${match[0].trim().slice(0, 80)}` });
        }
      }
    }

    for (const { start, end } of scripts) {
      for (const match of code.slice(start, end).matchAll(TYPE_DEFINITION)) {
        const name = match[1];
        if (!types.has(name)) types.set(name, new Map());
        if (!types.get(name).has(file)) types.get(name).set(file, lineOf(start + match.index + match[0].indexOf(name)));
      }
      for (const { values, offset } of unionsIn(code, start, end)) {
        const key = JSON.stringify(values);
        if (!unions.has(key)) unions.set(key, new Map());
        if (!unions.get(key).has(file)) unions.get(key).set(file, lineOf(offset));
      }
    }
  }

  for (const [name, where] of types) {
    if (where.size < 2 || unrelatedTypeNames.has(name)) continue;
    for (const [file, line] of where) {
      const others = [...where.keys()].filter((other) => other !== file).map((other) => other.replace(/.*\//, ''));
      findings.push({ rule: 'T1', file, line, what: `type ${name} is also defined in ${others.join(', ')}` });
    }
  }

  for (const [key, where] of unions) {
    if (where.size < 2) continue;
    const spelled = JSON.parse(key).map((value) => `"${value}"`).join(' | ');
    for (const [file, line] of where) {
      findings.push({ rule: 'T2', file, line, what: `the values ${spelled} are spelled out here and in ${where.size - 1} other file(s); name them once` });
    }
  }

  return findings;
}

/**
 * The files this check reads: the chat block and every script and component under its folder, or
 * under another folder when one is named. The token sheet holds no behaviour, so it is left out.
 */
export function partFiles(root) {
  return (root ? walk(resolve(root)) : styleFiles()).filter((file) => /\.(obs|ts)$/.test(file));
}

const args = process.argv.slice(2);
const invokedDirectly = process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url);

if (invokedDirectly) {
  const rootFlag = args.indexOf('--root');
  const files = partFiles(rootFlag >= 0 ? args[rootFlag + 1] : undefined);
  // One kind of line ending, which keeps every line number and lets a pattern stop at a line feed.
  const read = files.map((file) => ({ file, text: readFileSync(resolve(repoRoot, file), 'utf8').replace(/\r\n/g, '\n') }));

  const isFailed = runRatchet({ title: 'parts convictions', rules: PART_RULES, findings: findPartDebt(read), args });
  if (isFailed) process.exit(1);
  console.log(`one owner per job: ${files.length} chat files checked`);
}
