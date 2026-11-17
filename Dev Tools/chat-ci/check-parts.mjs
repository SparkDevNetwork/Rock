// Every behaviour in the chat has one home, and a screen uses it rather than rebuilding it: when two
// screens place a popover or trap focus each their own way, a fix lands in one and the other keeps
// the old behaviour. This check finds each rebuilt behaviour by the call that gives it away, outside
// the files that own it, and two rules for names: one type name defined in two files (T1), and one
// set of allowed string values spelled out in two files (T2). Each rule's reason is in PART_RULES,
// and a failure prints it. Counts are held by the ratchet in known-debt.mjs.
//
//   node check-parts.mjs                 the chat block and every script and component under its folder
//   node check-parts.mjs --root <dir>    another folder, which is how the tests prove it bites
//   node check-parts.mjs --record        records today's counts in known-debt.json
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

import { codeOf, lineFinder, regionsOf, styleFiles, walk } from './check-styles.mjs';
import { runRatchet } from './known-debt.mjs';

const repoRoot = fileURLToPath(new URL('../../', import.meta.url));

export const PART_RULES = [
  { id: 'P1', mode: 'report', reason: 'a screen draws a button with the button parts, so every button has one look and one set of states' },
  { id: 'P2', mode: 'ratchet', reason: 'placing a floating surface (floating-ui) has one home, the popover part' },
  { id: 'P3', mode: 'ratchet', reason: 'closing a surface on a press outside it has one home: the popover, the scrim and the dialog' },
  { id: 'P4', mode: 'ratchet', reason: 'reading phone, hover and reduced motion (matchMedia) has one home, the viewport part' },
  { id: 'P5', mode: 'ratchet', reason: 'announcing (aria-live, role status or alert) has one home, the announcer, so two live regions never talk over each other' },
  { id: 'P6', mode: 'ratchet', reason: 'watching what scrolls into view has three homes: paging, marking seen and revealing an image' },
  { id: 'P7', mode: 'report', reason: 'dropping a stale answer has one home, the latest-search helper, and saving one, the save rule' },
  { id: 'P8', mode: 'ratchet', reason: 'keeping focus inside a surface (Tab) has one home, the focus trap' },
  { id: 'P9', mode: 'ratchet', reason: 'the phone flag and the signed-in person are provided once by the session context, never passed down' },
  { id: 'P10', mode: 'ratchet', reason: 'production code never finds an element by its test id, which belongs to the tests' },
  { id: 'P15', mode: 'ratchet', reason: 'finding a room in the channel rows has one home, the channel store' },
  { id: 'T1', mode: 'ratchet', reason: 'a type has one home: types.partial.ts when two files use it, its own file when one does' },
  { id: 'T2', mode: 'ratchet', reason: 'a set of allowed values is one string union, defined once and referred to by name' },
];

/**
 * The files that own each behaviour, by path below the chat's folder without the extension. A part
 * not built yet is named for the step that builds it, which may rename it in the same change.
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
    // Tab past a popover's end leaves it for its anchor's place in the page; it keeps no focus in.
    'ui/popover.partial',
  ],
  P15: ['stores/channelStore.partial'],
};

/** Type names two files may each define for unrelated jobs, each with the reason a review agreed. */
export const UNRELATED_TYPE_NAMES = new Map([]);

function isOwner(rule, file) {
  return (OWNERS[rule] ?? []).some((stem) => file.endsWith(`/${stem}.ts`) || file.endsWith(`/${stem}.obs`));
}

/** What each per-file rule finds, and where in a file it looks. */
const PATTERNS = [
  { rule: 'P2', where: 'script', re: /["'](?:@Obsidian\/Libs\/floating-ui|@floating-ui\/[\w-]+)["']/g, what: 'floating-ui imported' },
  { rule: 'P3', where: 'script', re: /\bdocument\s*\.\s*addEventListener\(\s*["'](?:pointerdown|pointerup|mousedown|mouseup|touchstart|touchend|click)["']/g, what: 'a press listener on the document' },
  { rule: 'P4', where: 'all', re: /\bmatchMedia\s*(?:\?\.)?\s*\(/g, what: 'a media query read' },
  { rule: 'P5', where: 'all', re: /(?<![\w-])aria-live(?![\w-])/g, what: 'a live region' },
  // role status and alert make a live region as surely as aria-live does.
  { rule: 'P5', where: 'template', re: /(?<![\w:-])role\s*=\s*(["'])(?:status|alert)\1/g, what: 'a live region' },
  { rule: 'P5', where: 'template', re: /\s(?::|v-bind:)role\s*=\s*"[^"]*'(?:status|alert)'[^"]*"/g, what: 'a live region' },
  { rule: 'P5', where: 'script', re: /setAttribute\(\s*(["'])role\1\s*,\s*(["'])(?:status|alert)\2/g, what: 'a live region' },
  { rule: 'P6', where: 'script', re: /\bnew\s+IntersectionObserver\s*\(/g, what: 'an IntersectionObserver' },
  { rule: 'P7', where: 'script', re: /\b(?:let|var|const)\s+\w*[Gg]eneration\w*\s*(?::[^=]+)?=\s*(?:0\b|new\s+Map)|\b\w*[Gg]eneration\s*:\s*0\b/g, what: 'a generation counter' },
  // Tab compared either way round, matched in a switch, or found in a list or set of keys.
  { rule: 'P8', where: 'script', re: /\b(?:key|code)\s*[!=]==?\s*(["'])Tab\1|(["'])Tab\2\s*[!=]==?\s*[\w.]*\b(?:key|code)\b|\bcase\s+(["'])Tab\3\s*:|\[[^\]\n]*(["'])Tab\4[^\]\n]*\]\s*\)?\s*\.\s*(?:includes|has)\(/g, what: 'Tab handled' },
  { rule: 'P9', where: 'template', re: /\s(?::|v-bind:)(?:isPhone|is-phone|personAliasGuid|person-alias-guid)\s*=/g, what: 'passed down as a binding' },
  { rule: 'P9', where: 'obs-script', re: /\b(?:isPhone|personAliasGuid)\s*:\s*\{\s*type\s*:/g, what: 'declared as a prop' },
  // A type argument between the name and its bracket (querySelector<HTMLInputElement>) is the same lookup.
  { rule: 'P10', where: 'script', re: /\b(?:querySelector(?:All)?|closest)\s*(?:<[^<>()]*>\s*)?\(\s*(["'`])[^"'`]*\[data-testid/g, what: 'an element found by its test id' },
  // Only the channel rows: the directory's and the threads' rows are other lists with other owners.
  { rule: 'P15', where: 'script', re: /\bchannels\s*\.\s*rows\s*\.\s*(?:find|findIndex|findLast|some)\s*\(\s*\(?\s*(\w+)\s*\)?\s*=>[^\n]*?(?:\b\1\s*\.\s*channel_id\s*===?|===?\s*\1\s*\.\s*channel_id\b)/g, what: 'a room looked up in the channel rows' },
];

const TYPE_DEFINITION = /(?:^|[\s;{}])(?:export\s+)?(?:declare\s+)?(?:type|interface|(?:const\s+)?enum)\s+([A-Z]\w*)\s*(?:[=<{]|extends\b)/g;

const STRING = String.raw`"(?:[^"\\\n]|\\.)*"|'(?:[^'\\\n]|\\.)*'`;
const MEMBER = String.raw`(?:${STRING}|null|undefined)`;
const UNION = new RegExp(String.raw`(\|\s*)?${MEMBER}(?:\s*\|\s*${MEMBER})+`, 'g');

/**
 * Each union of string values in a stretch of comment-free code: its sorted set and where it is
 * named. null and undefined are not values. A union that also holds a type by name is part of a
 * larger set, and the keys Pick and Omit take name fields, so both are skipped.
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
    if (/\b(?:Pick|Omit)\s*<[^<>]*,$/.test(before)) continue;
    const values = [...new Set([...match[0].matchAll(new RegExp(STRING, 'g'))].map((one) => one[0].slice(1, -1)))].sort();
    // A union opened by a pipe on its own line is reported where it is named, on the line above.
    if (values.length >= 2) out.push({ values, offset: start + (match[1] ? Math.max(0, before.length - 1) : match.index) });
  }
  return out;
}

/**
 * Everything the chat files hold against the parts rules, as findings of rule, file, line and
 * what: each file alone for the behaviours, then all together for the types and value sets.
 */
export function findPartDebt(files, { unrelatedTypeNames = UNRELATED_TYPE_NAMES } = {}) {
  const findings = [];
  const types = new Map();
  const unions = new Map();
  const note = (map, key, file, line) => {
    if (!map.has(key)) map.set(key, new Map());
    if (!map.get(key).has(file)) map.get(key).set(file, line);
  };

  for (const { file, text } of files) {
    if (file.endsWith('.css')) continue;
    const code = codeOf(file, text);
    const lineOf = lineFinder(text);
    const { scripts, template } = regionsOf(file, text);
    const templates = template ? [template] : [];
    const ranges = { script: scripts, 'obs-script': file.endsWith('.obs') ? scripts : [], template: templates, all: [...scripts, ...templates] };

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
        note(types, match[1], file, lineOf(start + match.index + match[0].indexOf(match[1])));
      }
      for (const { values, offset } of unionsIn(code, start, end)) note(unions, JSON.stringify(values), file, lineOf(offset));
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
      findings.push({ rule: 'T2', file, line, what: `the values ${spelled} are spelled out here and in ${where.size - 1} other file(s)` });
    }
  }
  return findings;
}

/** The scripts and components this check reads; the token sheet holds no behaviour. */
export function partFiles(root) {
  return (root ? walk(resolve(root)) : styleFiles()).filter((file) => /\.(obs|ts)$/.test(file));
}

const args = process.argv.slice(2);
const invokedDirectly = process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url);

if (invokedDirectly) {
  const rootFlag = args.indexOf('--root');
  const files = partFiles(rootFlag >= 0 ? args[rootFlag + 1] : undefined);
  // One kind of line ending keeps every line number and lets a pattern stop at a line feed.
  const read = files.map((file) => ({ file, text: readFileSync(resolve(repoRoot, file), 'utf8').replace(/\r\n/g, '\n') }));

  if (runRatchet({ title: 'parts convictions', rules: PART_RULES, findings: findPartDebt(read), args })) process.exit(1);
  console.log(`one owner per job: ${files.length} chat files checked`);
}
