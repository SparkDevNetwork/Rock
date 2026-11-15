// The ratchet the style and parts checks share. A rule that today's code already breaks cannot
// block on the day it arrives, or nothing would land; a rule that only reports is forgotten. So
// each rule holds every file to the count of findings recorded for it in known-debt.json, and the
// count may only fall:
//
//   - more findings than recorded fails, which is what stops a copy coming back;
//   - a file with no entry, or a rule with no entries at all, may hold none, so new code and any
//     area already clean block at once;
//   - fewer findings than recorded warns on a branch, where parallel work would otherwise have to
//     edit the same file on every commit, and fails on the branch that lands, so the count is
//     lowered in the change that earned it;
//   - the debt file itself may not grow against the commit it builds on: raising a count to make
//     a run pass is the one move the ratchet exists to refuse;
//   - a report-only rule prints its count and never fails.
//
// Each script owns its own rules and reads only their entries, so the two can share one file.
import { execFileSync } from 'node:child_process';
import { appendFileSync, readFileSync, writeFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const repoRoot = fileURLToPath(new URL('../../', import.meta.url));

/** Where the debt lives, from the repository root, which is also how git names it at another commit. */
export const debtPath = 'Dev Tools/chat-ci/known-debt.json';

/**
 * Holds findings to their recorded counts.
 *
 * @param {object} options
 * @param {{ id: string, mode: 'ratchet' | 'report' }[]} options.rules the rules this script owns
 * @param {{ rule: string, file: string, line: number, what: string }[]} options.findings what the scan found
 * @param {Record<string, Record<string, number>>} options.debt the recorded counts, rule then file
 * @param {Record<string, Record<string, number>> | null} [options.baseDebt] the counts at the commit this builds on, or null when it had none
 * @param {boolean} [options.isLanding] whether this run is on the branch that lands
 * @returns {{ rules: object[], failures: string[], warnings: string[], rows: object[] }}
 */
export function ratchet({ rules, findings, debt, baseDebt = null, isLanding = false }) {
  const failures = [];
  const warnings = [];
  const rows = [];

  for (const rule of rules) {
    const found = new Map();
    for (const one of findings) {
      if (one.rule !== rule.id) continue;
      if (!found.has(one.file)) found.set(one.file, []);
      found.get(one.file).push(one);
    }

    if (rule.mode === 'report') {
      for (const [file, hits] of found) rows.push({ rule: rule.id, mode: rule.mode, file, recorded: null, found: hits.length });
      continue;
    }

    const recorded = debt[rule.id] ?? {};
    const files = [...new Set([...found.keys(), ...Object.keys(recorded)])].sort();
    for (const file of files) {
      const hits = found.get(file) ?? [];
      const allowed = recorded[file] ?? 0;
      rows.push({ rule: rule.id, mode: rule.mode, file, recorded: allowed, found: hits.length });

      if (hits.length > allowed) {
        const where = hits.map((hit) => `    ${hit.file}:${hit.line}: ${hit.what}`).join('\n');
        const why = allowed === 0 ? 'which may hold none' : `which may hold ${allowed}`;
        failures.push(`${rule.id} ${file}: ${hits.length} found, ${why}\n${where}`);
      } else if (hits.length < allowed) {
        const message = `${rule.id} ${file}: ${hits.length} found, ${allowed} recorded; lower the count in ${debtPath} to ${hits.length}`;
        (isLanding ? failures : warnings).push(message);
      }
    }

    if (baseDebt !== null) {
      const before = baseDebt[rule.id] ?? {};
      for (const [file, count] of Object.entries(recorded)) {
        const was = before[file] ?? 0;
        if (count > was) {
          failures.push(`${rule.id} ${file}: the recorded count rose from ${was} to ${count} against the commit this builds on; debt may only fall`);
        }
      }
    }
  }

  return { rules, failures, warnings, rows };
}

/**
 * The run's table in markdown: one line per rule with its totals, recorded against found, then
 * every file a rule holds, so the step summary shows where the debt sits.
 */
export function debtTable({ rules, rows }) {
  const ids = rules.map((rule) => rule.id);
  const modes = new Map(rules.map((rule) => [rule.id, rule.mode]));

  const lines = ['| Rule | Mode | Recorded | Found |', '| --- | --- | --- | --- |'];
  for (const id of ids) {
    const mine = rows.filter((row) => row.rule === id);
    const mode = modes.get(id);
    const recorded = mode === 'report' ? '-' : String(mine.reduce((sum, row) => sum + row.recorded, 0));
    lines.push(`| ${id} | ${mode} | ${recorded} | ${mine.reduce((sum, row) => sum + row.found, 0)} |`);
  }

  const held = rows.filter((row) => row.found > 0 || (row.recorded ?? 0) > 0);
  if (held.length > 0) {
    lines.push('', '| Rule | File | Recorded | Found |', '| --- | --- | --- | --- |');
    for (const row of held) lines.push(`| ${row.rule} | ${row.file} | ${row.recorded ?? '-'} | ${row.found} |`);
  }
  return lines.join('\n');
}

/** The debt file at a path, or an empty one when there is none yet. */
export function readDebt(path) {
  try {
    return JSON.parse(readFileSync(path, 'utf8'));
  } catch (error) {
    if (error.code === 'ENOENT') return {};
    throw error;
  }
}

function git(...args) {
  return execFileSync('git', args, { cwd: repoRoot, encoding: 'utf8', stdio: ['ignore', 'pipe', 'ignore'] }).trim();
}

/**
 * The debt file at the commit this run builds on, or null when that commit had none. The workflow
 * names the commit in CHAT_DEBT_BASE: a pull request's base, or the branch tip before a push. Run
 * by hand, it is where this branch left chat-platform, or the remote's tip when on chat-platform.
 * When no commit can be found the comparison is skipped and says so, rather than failing a run for
 * want of history.
 */
export function baseDebt() {
  let base = process.env.CHAT_DEBT_BASE ?? '';
  if (/^0+$/.test(base)) return { debt: null, note: 'a new branch: no earlier debt file to compare with' };
  try {
    if (!base) {
      const branch = git('rev-parse', '--abbrev-ref', 'HEAD');
      base = branch === 'chat-platform' ? git('rev-parse', 'origin/chat-platform') : git('merge-base', 'HEAD', 'chat-platform');
    }
    git('cat-file', '-e', `${base}^{commit}`);
  } catch {
    return { debt: null, note: `no base commit found${base ? ` for ${base}` : ''}; the debt file was not compared with one` };
  }
  try {
    return { debt: JSON.parse(git('show', `${base}:${debtPath}`)), note: `compared with ${base.slice(0, 10)}` };
  } catch {
    return { debt: null, note: `${base.slice(0, 10)} has no debt file yet` };
  }
}

/**
 * Whether this run is on the branch that lands. CHAT_DEBT_LANDING says so outright; in the
 * workflow it is a push to chat-platform; by hand it is the checked-out branch.
 */
export function isLandingRun() {
  if (process.env.CHAT_DEBT_LANDING) return process.env.CHAT_DEBT_LANDING === '1';
  if (process.env.GITHUB_ACTIONS) return process.env.GITHUB_REF === 'refs/heads/chat-platform';
  try {
    return git('rev-parse', '--abbrev-ref', 'HEAD') === 'chat-platform';
  } catch {
    return false;
  }
}

/**
 * Writes this script's counts into the debt file, leaving other scripts' rules as they are. Used
 * to record the debt once and to lower a count; the base comparison refuses any count it raised.
 */
export function recordDebt(path, rules, findings) {
  const debt = readDebt(path);
  for (const rule of rules) {
    if (rule.mode !== 'ratchet') continue;
    const counts = {};
    for (const one of findings) if (one.rule === rule.id) counts[one.file] = (counts[one.file] ?? 0) + 1;
    debt[rule.id] = Object.fromEntries(Object.entries(counts).sort(([a], [b]) => (a < b ? -1 : 1)));
  }
  const ordered = Object.fromEntries(Object.entries(debt).sort(([a], [b]) => ruleOrder(a) - ruleOrder(b)));
  writeFileSync(path, `${JSON.stringify(ordered, null, 2)}\n`);
}

/** S rules first, then P, then T, each by number, so the file reads in the order the scripts list them. */
function ruleOrder(id) {
  const letter = 'SPT'.indexOf(id[0]);
  return (letter < 0 ? 9 : letter) * 1000 + Number(id.slice(1));
}

/**
 * The command-line half both scripts share: reads the flags, runs the ratchet, prints the table and
 * every message, writes the table to the workflow's step summary, and returns whether it failed.
 *
 *   --debt <path>        another debt file, which is how the tests run a fixture; no base is compared
 *   --base-debt <path>   the debt file to compare with, in place of git's
 *   --record             write today's counts into the debt file instead of checking them
 *
 * A run over another folder (--root) with no --debt holds that folder to no debt at all: the
 * chat's own entries name files the run never reads.
 */
export function runRatchet({ title, rules, findings, args }) {
  const flag = (name) => {
    const at = args.indexOf(name);
    return at >= 0 ? args[at + 1] : undefined;
  };
  const isOtherFolder = flag('--root') !== undefined && flag('--debt') === undefined;
  const debtFile = flag('--debt') ? resolve(flag('--debt')) : resolve(repoRoot, debtPath);

  if (args.includes('--record')) {
    recordDebt(debtFile, rules, findings);
    console.log(`recorded ${title} counts in ${debtFile}`);
    return false;
  }

  let base = { debt: null, note: `${flag('--debt') ?? 'no debt file'} named for this run: no base compared` };
  if (flag('--base-debt')) base = { debt: readDebt(resolve(flag('--base-debt'))), note: `compared with ${flag('--base-debt')}` };
  else if (!flag('--debt') && !isOtherFolder) base = baseDebt();

  const debt = isOtherFolder ? {} : readDebt(debtFile);
  const result = ratchet({ rules, findings, debt, baseDebt: base.debt, isLanding: isLandingRun() });
  const table = debtTable(result);

  console.log(`${title}, against ${isOtherFolder ? 'no debt' : debtPath} (${base.note}):\n\n${table}\n`);
  for (const warning of result.warnings) console.log(`warning: ${warning}`);
  if (result.failures.length > 0) {
    console.error(`${title}: the ratchet failed`);
    for (const failure of result.failures) console.error(`  ${failure}`);
  }

  if (process.env.GITHUB_STEP_SUMMARY) {
    const warned = result.warnings.map((warning) => `- ${warning}`).join('\n');
    appendFileSync(process.env.GITHUB_STEP_SUMMARY, `### ${title}\n\n${base.note}\n\n${table}\n\n${warned}\n`);
  }

  return result.failures.length > 0;
}
