// The workflow is the only thing that runs these tests where nobody is watching, and it cannot be
// proven on a runner until this branch is pushed. What can be proven here is its shape: that it
// builds and runs both suites on a Windows runner, and that the integration suite stays off until
// somebody asks for it by label.
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import test from 'node:test';
import { fileURLToPath } from 'node:url';

import { parse } from 'yaml';

const repoRoot = fileURLToPath(new URL('../../', import.meta.url));
const workflowPath = resolve(repoRoot, '.github/workflows/chat-ci.yml');

function workflow() {
  return parse(readFileSync(workflowPath, 'utf8'));
}

test('it builds and runs both suites on a Windows runner', () => {
  const doc = workflow();
  const jobs = Object.values(doc.jobs ?? {});

  assert.ok(jobs.length > 0, 'the workflow declares no jobs');

  for (const job of jobs) {
    assert.match(String(job['runs-on']), /^windows/, 'a chat job runs somewhere other than a Windows runner');
  }

  const steps = jobs.flatMap((job) => (job.steps ?? []).map((step) => `${step.name ?? ''} ${step.run ?? ''}`));
  const text = steps.join('\n');

  assert.match(text, /Rock\.Tests\.csproj|Rock\.Tests\b/, 'nothing in the workflow builds or runs the unit test project');
  assert.match(text, /\bjest\b|npm (run )?test/, 'nothing in the workflow runs the client tests');
  assert.match(text, /check-references|npm (run )?check:references/, 'the reference gate never runs');
});

test('the integration suite runs only when the label asks for it', () => {
  const doc = workflow();
  const gated = Object.entries(doc.jobs ?? {}).filter(([, job]) => String(job.if ?? '').includes('run-integration'));

  assert.equal(gated.length, 1, 'exactly one job should be gated on the label');

  const [, job] = gated[0];
  const condition = String(job.if);

  assert.match(condition, /labels/, "the condition does not read the pull request's labels");
  assert.match(
    String(JSON.stringify(job.steps ?? [])),
    /Rock\.Tests\.Integration/,
    'the gated job does not run the integration project',
  );

  // Without the labeled event the label cannot start a run, so the job would only ever fire on a
  // later push to an already-labelled pull request.
  const triggers = doc.on?.pull_request?.types ?? [];
  assert.ok(triggers.includes('labeled'), 'the workflow does not listen for a label being added');

  // Everything else has to stay off that condition, or the label silently gates the whole file.
  const ungated = Object.entries(doc.jobs ?? {}).filter(([, other]) => !String(other.if ?? '').includes('run-integration'));
  assert.ok(ungated.length > 0, 'every job is gated on the label, so an ordinary pull request runs nothing');
});
