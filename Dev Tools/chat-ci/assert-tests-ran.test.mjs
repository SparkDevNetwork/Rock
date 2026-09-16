// The whole point of the check is the run that passes while doing nothing, so that case is the
// first one asserted here.
import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { mkdtempSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import test from 'node:test';
import { fileURLToPath } from 'node:url';

const here = fileURLToPath(new URL('.', import.meta.url));
const script = join(here, 'assert-tests-ran.mjs');

function resultsFile(counters) {
  const dir = mkdtempSync(join(tmpdir(), 'assert-tests-ran-'));
  const file = join(dir, 'results.trx');
  writeFileSync(file, `<?xml version="1.0" encoding="UTF-8"?>\n<TestRun>\n<ResultSummary>\n<Counters ${counters} />\n</ResultSummary>\n</TestRun>\n`);
  return file;
}

function run(...args) {
  return spawnSync(process.execPath, [script, ...args], { encoding: 'utf8' });
}

test('it refuses a run that executed nothing', () => {
  const result = run(resultsFile('total="0" executed="0" passed="0" failed="0"'));

  assert.equal(result.status, 1);
  assert.match(result.stderr, /0 tests ran/);
});

test('it refuses a run that executed fewer than asked for', () => {
  const result = run(resultsFile('total="3" executed="3" passed="3" failed="0"'), '--minimum', '4');

  assert.equal(result.status, 1);
  assert.match(result.stderr, /at least 4/);
});

test('it refuses a run that executed tests and failed one', () => {
  const result = run(resultsFile('total="6" executed="6" passed="5" failed="1"'));

  assert.equal(result.status, 1);
  assert.match(result.stderr, /did not pass/);
});

test('it refuses a run whose results file was never written', () => {
  const result = run(join(tmpdir(), 'no-such-results.trx'));

  assert.equal(result.status, 1);
  assert.match(result.stderr, /not written/);
});

test('it passes a run that executed tests and passed them', () => {
  const result = run(resultsFile('total="6" executed="6" passed="6" failed="0"'));

  assert.equal(result.status, 0);
  assert.match(result.stdout, /6 tests ran and passed/);
});

test('the runner it exists for really does exit zero on an empty filter', () => {
  // Measured rather than assumed, because the whole check rests on it. Skipped when the test
  // assembly has not been built, so this file stays runnable on its own.
  const built = spawnSync('dotnet', ['--version'], { encoding: 'utf8', shell: true });
  if (built.status !== 0) return;

  const repoRoot = join(here, '..', '..');
  const probe = spawnSync(
    'dotnet',
    ['test', 'Rock.Tests/Rock.Tests.csproj', '--no-build', '--filter', 'FullyQualifiedName~ZzNoSuchTestNamespace'],
    { cwd: repoRoot, encoding: 'utf8', shell: true },
  );

  if (/MSB1009|could not be found|not been built/i.test(`${probe.stdout}${probe.stderr}`)) return;

  assert.equal(probe.status, 0, 'the premise changed: an empty filter now fails on its own, and this check could be dropped');
  assert.match(probe.stdout, /No test matches the given testcase filter/);
});
