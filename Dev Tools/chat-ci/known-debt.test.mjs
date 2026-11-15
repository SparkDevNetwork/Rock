// The style and parts checks hold today's code to what it already has: each rule may keep the
// count of findings recorded for a file, never more. These cells prove the ratchet both ways: it
// blocks a new finding, an unlisted file and a rule with no entries; it asks for a lower count once
// a file improves, warning on a branch and failing on the branch that lands; and it refuses a debt
// file that grew against the one it builds on.
import assert from 'node:assert/strict';
import test from 'node:test';

import { debtTable, editsChecks, ratchet } from './known-debt.mjs';

test('only an edit to the style or parts check allows a raise, not their tests or the other gates', () => {
  assert.equal(editsChecks(['Dev Tools/chat-ci/check-styles.mjs']), true);
  assert.equal(editsChecks(['README.md', 'Dev Tools/chat-ci/check-parts.mjs']), true);
  assert.equal(editsChecks([
    'Dev Tools/chat-ci/check-styles.test.mjs',
    'Dev Tools/chat-ci/check-parts.test.mjs',
    'Dev Tools/chat-ci/check-references.mjs',
    'Dev Tools/chat-ci/check-types.mjs',
    'Dev Tools/chat-ci/check-contract.mjs',
    'Dev Tools/chat-ci/known-debt.json',
  ]), false);
});

const rules = [
  { id: 'S1', mode: 'ratchet', reason: 'a font size comes from the type scale' },
  { id: 'S2', mode: 'ratchet', reason: 'a corner comes from the radius tokens' },
  { id: 'S10', mode: 'report', reason: 'a token read once may have no second caller' },
];

function finding(rule, file, line = 1) {
  return { rule, file, line, what: `${rule} at ${line}` };
}

test('a file holding exactly its recorded count passes', () => {
  const result = ratchet({
    rules,
    findings: [finding('S1', 'a.obs', 1), finding('S1', 'a.obs', 2)],
    debt: { S1: { 'a.obs': 2 } },
  });

  assert.deepEqual(result.failures, []);
  assert.deepEqual(result.warnings, []);
});

test('a file holding more than its recorded count fails and names its findings', () => {
  const result = ratchet({
    rules,
    findings: [finding('S1', 'a.obs', 1), finding('S1', 'a.obs', 7)],
    debt: { S1: { 'a.obs': 1 } },
  });

  assert.equal(result.failures.length, 1, result.failures.join('\n'));
  assert.match(result.failures[0], /S1/);
  assert.match(result.failures[0], /a\.obs/);
  assert.match(result.failures[0], /a\.obs:7/);
  // The failure says why the rule exists and what to do, so nobody reaches for the debt file.
  assert.match(result.failures[0], /a font size comes from the type scale/);
  assert.match(result.failures[0], /use the shared owner; never raise the debt file/);
});

test('a raise against the base is allowed only in a change that edits the checks themselves', () => {
  const findings = [finding('S1', 'a.obs'), finding('S1', 'a.obs')];
  const debt = { S1: { 'a.obs': 2 } };
  const baseDebt = { S1: { 'a.obs': 1 } };

  const plain = ratchet({ rules, findings, debt, baseDebt });
  assert.equal(plain.failures.length, 1, plain.failures.join('\n'));

  const checksChanged = ratchet({ rules, findings, debt, baseDebt, isRaiseAllowed: true });
  assert.deepEqual(checksChanged.failures, []);
  assert.match(checksChanged.warnings.join('\n'), /rose.*checks/i);
});

test('an unlisted file blocks at once, and so does a rule with no entries', () => {
  const result = ratchet({
    rules,
    findings: [finding('S1', 'new.obs'), finding('S2', 'a.obs')],
    debt: { S1: { 'a.obs': 3 } },
  });

  const text = result.failures.join('\n');
  assert.match(text, /S1.*new\.obs/);
  assert.match(text, /S2.*a\.obs/);
});

test('a file under its recorded count warns on a branch', () => {
  const result = ratchet({
    rules,
    findings: [finding('S1', 'a.obs')],
    debt: { S1: { 'a.obs': 3, 'gone.obs': 1 } },
    isLanding: false,
  });

  assert.deepEqual(result.failures, []);
  const text = result.warnings.join('\n');
  assert.match(text, /a\.obs.*lower/i);
  assert.match(text, /gone\.obs/);
});

test('a file under its recorded count fails on the branch that lands, so the count is lowered in the same change', () => {
  const result = ratchet({
    rules,
    findings: [finding('S1', 'a.obs')],
    debt: { S1: { 'a.obs': 3 } },
    isLanding: true,
  });

  assert.equal(result.failures.length, 1);
  assert.match(result.failures[0], /lower/i);
});

test('a report-only rule never fails, whatever it finds', () => {
  const result = ratchet({
    rules,
    findings: [finding('S10', 'chat.css', 1), finding('S10', 'chat.css', 2)],
    debt: {},
    isLanding: true,
  });

  assert.deepEqual(result.failures, []);
  assert.ok(result.rows.some((row) => row.rule === 'S10' && row.found === 2 && row.mode === 'report'));
});

test('the debt file may only fall against the one it builds on', () => {
  const findings = [finding('S1', 'a.obs'), finding('S1', 'a.obs'), finding('S1', 'b.obs')];
  const debt = { S1: { 'a.obs': 2, 'b.obs': 1 } };

  const rose = ratchet({ rules, findings, debt, baseDebt: { S1: { 'a.obs': 1, 'b.obs': 1 } } });
  assert.equal(rose.failures.length, 1, rose.failures.join('\n'));
  assert.match(rose.failures[0], /a\.obs.*rose|rose.*a\.obs/i);

  const added = ratchet({ rules, findings, debt, baseDebt: { S1: { 'a.obs': 2 } } });
  assert.equal(added.failures.length, 1, added.failures.join('\n'));
  assert.match(added.failures[0], /b\.obs/);

  const fell = ratchet({ rules, findings, debt, baseDebt: { S1: { 'a.obs': 5, 'b.obs': 1 } } });
  assert.deepEqual(fell.failures, []);

  // A base with no debt file at all is the change that brings the file in.
  const first = ratchet({ rules, findings, debt, baseDebt: null });
  assert.deepEqual(first.failures, []);
});

test('a rule the script does not own is left to the script that does', () => {
  const result = ratchet({
    rules,
    findings: [],
    debt: { P2: { 'a.obs': 4 } },
    baseDebt: { P2: {} },
    isLanding: true,
  });

  assert.deepEqual(result.failures, []);
  assert.deepEqual(result.warnings, []);
});

test('the table totals each rule, recorded against found', () => {
  const result = ratchet({
    rules,
    findings: [finding('S1', 'a.obs'), finding('S1', 'b.obs'), finding('S10', 'chat.css')],
    debt: { S1: { 'a.obs': 1, 'b.obs': 1 } },
  });

  const table = debtTable(result);

  assert.match(table, /\|\s*S1\s*\|\s*ratchet\s*\|\s*2\s*\|\s*2\s*\|/);
  assert.match(table, /\|\s*S2\s*\|\s*ratchet\s*\|\s*0\s*\|\s*0\s*\|/);
  assert.match(table, /\|\s*S10\s*\|\s*report\s*\|\s*-\s*\|\s*1\s*\|/);
});
