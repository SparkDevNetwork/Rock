// A test runner that matches nothing reports success. "dotnet test --filter X" prints "No test
// matches the given testcase filter" and exits 0, measured against this solution, so a renamed
// namespace or a moved folder would leave a pipeline green while running nothing at all. Neither
// TreatNoTestsAsError nor VSTestTreatNoTestsAsError changes that exit code here.
//
// This reads the run's own results file and refuses a run that executed too few tests, or that
// executed some and failed any.
//
//   node assert-tests-ran.mjs <results.trx> [--minimum N]   N defaults to 1
import { readFileSync } from 'node:fs';

const args = process.argv.slice(2);
const file = args.find((arg) => !arg.startsWith('--'));
const minimumFlag = args.indexOf('--minimum');
const minimum = minimumFlag >= 0 && args[minimumFlag + 1] ? Number(args[minimumFlag + 1]) : 1;

if (!file) {
  console.error('assert-tests-ran: name the results file to read.');
  process.exit(1);
}

let text;
try {
  text = readFileSync(file, 'utf8');
} catch {
  console.error(`assert-tests-ran: ${file} was not written, so no test run can be accounted for.`);
  process.exit(1);
}

const counters = /<Counters\b([^>]*)\/?>/.exec(text);
if (!counters) {
  console.error(`assert-tests-ran: ${file} carries no result summary.`);
  process.exit(1);
}

/**
 * Reads one count out of the summary element.
 *
 * @param {string} name the attribute name
 * @returns {number} the count, or zero when the attribute is absent
 */
function count(name) {
  const match = new RegExp(`${name}="(\\d+)"`).exec(counters[1]);
  return match ? Number(match[1]) : 0;
}

const executed = count('executed');
const failed = count('failed') + count('error') + count('timeout') + count('aborted');

if (executed < minimum) {
  console.error(
    `assert-tests-ran: ${executed} tests ran and at least ${minimum} was expected. A filter that matches nothing still exits zero, so this is what a silently empty run looks like.`,
  );
  process.exit(1);
}

if (failed > 0) {
  console.error(`assert-tests-ran: ${failed} of ${executed} tests did not pass.`);
  process.exit(1);
}

console.log(`assert-tests-ran: ${executed} tests ran and passed.`);
