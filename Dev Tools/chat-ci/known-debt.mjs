// The ratchet the style and parts checks share. A rule today's code already breaks cannot block on
// the day it arrives, and a rule that only reports is forgotten, so each rule holds every file to
// the count recorded for it in known-debt.json, and the count may only fall:
//
//   - more findings than recorded fails, which stops a copy coming back; a file with no entry, or a
//     rule with no entries, may hold none;
//   - fewer than recorded warns on a branch, where parallel work would otherwise edit this file on
//     every commit, and fails on chat-platform, so the change that earned it lowers the count;
//   - the file may not grow against the commit a run builds on, unless the same change edits a
//     check-*.mjs, where a sharper rule finds more of what was always there;
//   - a report-only rule prints its count and never fails.
import { execFileSync } from 'node:child_process';
import { appendFileSync, readFileSync, writeFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const repoRoot = fileURLToPath(new URL('../../', import.meta.url));

/** From the repository root, which is also how git names the file at another commit. */
export const debtPath = 'Dev Tools/chat-ci/known-debt.json';

const ADVICE = 'use the shared owner; never raise the debt file';

/**
 * Holds findings to their recorded counts and returns the failures, the warnings and one row per
 * rule and file. `baseDebt` is the file at the commit this builds on, or null when it had none.
 */
export function ratchet({ rules, findings, debt, baseDebt = null, isLanding = false, isRaiseAllowed = false }) {
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
    for (const file of [...new Set([...found.keys(), ...Object.keys(recorded)])].sort()) {
      const hits = found.get(file) ?? [];
      const allowed = recorded[file] ?? 0;
      rows.push({ rule: rule.id, mode: rule.mode, file, recorded: allowed, found: hits.length });

      if (hits.length > allowed) {
        const where = hits.map((hit) => `    ${hit.file}:${hit.line}: ${hit.what}`).join('\n');
        failures.push(`${rule.id} ${file}: ${hits.length} found, which may hold ${allowed}. Why: ${rule.reason}; ${ADVICE}.\n${where}`);
      } else if (hits.length < allowed) {
        (isLanding ? failures : warnings).push(`${rule.id} ${file}: ${hits.length} found, ${allowed} recorded; lower the count in ${debtPath} to ${hits.length}`);
      }
    }

    if (baseDebt !== null) {
      const before = baseDebt[rule.id] ?? {};
      for (const [file, count] of Object.entries(recorded)) {
        const was = before[file] ?? 0;
        if (count <= was) continue;
        const message = `${rule.id} ${file}: the recorded count rose from ${was} to ${count} against the commit this builds on`;
        if (isRaiseAllowed) warnings.push(`${message}, allowed because this change edits the checks`);
        else failures.push(`${message}; debt may only fall (a renamed file carries its entry as a new one). ${ADVICE}.`);
      }
    }
  }

  return { rules, failures, warnings, rows };
}

/** The run's table in markdown: each rule's totals, then every file a rule holds. */
export function debtTable({ rules, rows }) {
  const lines = ['| Rule | Mode | Recorded | Found |', '| --- | --- | --- | --- |'];
  for (const { id, mode } of rules) {
    const mine = rows.filter((row) => row.rule === id);
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
 * The debt file at the commit this run builds on, and whether this change edits a check. The
 * workflow names the commit in CHAT_DEBT_BASE; by hand it is where the branch left chat-platform,
 * or the remote's tip when on chat-platform. Without a commit the comparison is skipped with a note
 * rather than failing a run for want of history.
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
  let isRaiseAllowed = false;
  try {
    isRaiseAllowed = git('diff', '--name-only', base, '--', 'Dev Tools/chat-ci/check-*.mjs') !== '';
  } catch {
    // A diff git cannot make allows nothing.
  }
  try {
    return { debt: JSON.parse(git('show', `${base}:${debtPath}`)), isRaiseAllowed, note: `compared with ${base.slice(0, 10)}` };
  } catch {
    return { debt: null, note: `${base.slice(0, 10)} has no debt file yet` };
  }
}

/** CHAT_DEBT_LANDING says so outright; in the workflow it is a push to chat-platform; by hand, the checked-out branch. */
export function isLandingRun() {
  if (process.env.CHAT_DEBT_LANDING) return process.env.CHAT_DEBT_LANDING === '1';
  if (process.env.GITHUB_ACTIONS) return process.env.GITHUB_REF === 'refs/heads/chat-platform';
  try {
    return git('rev-parse', '--abbrev-ref', 'HEAD') === 'chat-platform';
  } catch {
    return false;
  }
}

/** Writes this script's counts, leaving the other script's rules as they are. */
export function recordDebt(path, rules, findings) {
  const debt = readDebt(path);
  for (const rule of rules) {
    if (rule.mode !== 'ratchet') continue;
    const counts = {};
    for (const one of findings) if (one.rule === rule.id) counts[one.file] = (counts[one.file] ?? 0) + 1;
    debt[rule.id] = Object.fromEntries(Object.entries(counts).sort(([a], [b]) => (a < b ? -1 : 1)));
  }
  // S rules, then P, then T, each by number: the order the scripts list them.
  const order = (id) => ('SPT'.indexOf(id[0]) + 1) * 1000 + Number(id.slice(1));
  writeFileSync(path, `${JSON.stringify(Object.fromEntries(Object.entries(debt).sort(([a], [b]) => order(a) - order(b))), null, 2)}\n`);
}

/**
 * The command-line half both scripts share; returns whether the run failed.
 *
 *   --debt <path>        another debt file, how the tests run a fixture; no base is compared
 *   --base-debt <path>   the debt file to compare with, in place of git's
 *   --record             write today's counts instead of checking them
 *
 * A run over another folder (--root) with no --debt holds it to no debt: the chat's entries name
 * files that run never reads.
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
  const result = ratchet({ rules, findings, debt, baseDebt: base.debt, isLanding: isLandingRun(), isRaiseAllowed: base.isRaiseAllowed === true });
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
