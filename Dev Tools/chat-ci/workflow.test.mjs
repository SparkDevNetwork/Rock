// The workflow is the only thing that runs these tests where nobody is watching, and it cannot be
// proven on a runner until this branch is pushed. What can be proven here is its shape: that the
// right job builds and runs each suite on a Windows runner, that neither can report success
// without running anything, and that the integration suite stays off until somebody asks for it.
//
// Every assertion below is made against one named job's own steps. An earlier version of this file
// flattened all the jobs into one string, which let the integration job satisfy the assertions
// meant for the unit job.
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import test from 'node:test';
import { fileURLToPath } from 'node:url';

import { parse } from 'yaml';

const repoRoot = fileURLToPath(new URL('../../', import.meta.url));
const workflowPath = resolve(repoRoot, '.github/workflows/chat-ci.yml');

const unitProject = /Rock\.Tests[\\/]Rock\.Tests\.csproj/;
const integrationProject = /Rock\.Tests\.Integration[\\/]Rock\.Tests\.Integration\.csproj/;

function workflow() {
  return parse(readFileSync(workflowPath, 'utf8'));
}

function jobs() {
  return Object.entries(workflow().jobs ?? {});
}

function stepsOf(job) {
  return (job.steps ?? []).map((step) => `${step.name ?? ''} ${step.uses ?? ''} ${step.run ?? ''}`);
}

function labelled() {
  const gated = jobs().filter(([, job]) => String(job.if ?? '').includes('run-integration'));
  assert.equal(gated.length, 1, 'exactly one job should be gated on the label');
  return gated[0][1];
}

function unlabelled() {
  const open = jobs().filter(([, job]) => !String(job.if ?? '').includes('run-integration'));
  assert.equal(open.length, 1, 'exactly one job should run on an ordinary pull request');
  return open[0][1];
}

test('every job runs on a Windows runner', () => {
  const all = jobs();

  assert.ok(all.length > 0, 'the workflow declares no jobs');

  for (const [name, job] of all) {
    assert.match(String(job['runs-on']), /^windows/, `job ${name} runs somewhere other than a Windows runner`);
  }
});

test('the ordinary job builds and runs the unit suite and the client suite', () => {
  const steps = stepsOf(unlabelled());
  const text = steps.join('\n');

  assert.ok(
    steps.some((step) => unitProject.test(step) && /msbuild|dotnet build/i.test(step)),
    'no step builds the unit test project',
  );
  assert.ok(
    steps.some((step) => unitProject.test(step) && /dotnet test/.test(step)),
    'no step runs the unit test project',
  );
  assert.ok(
    steps.some((step) => /\bjest\b/.test(step)),
    'no step runs the client tests',
  );
  assert.match(text, /check:references/, 'the reference gate never runs');
  assert.match(text, /check-chat-types/, 'the chat TypeScript member gate never runs');
  assert.ok(!integrationProject.test(text), 'the ordinary job reaches for the integration project');
});

test('neither suite can report success without running anything', () => {
  // A filter that matches nothing exits zero, so each run is accounted for from its own results
  // file. Without this the jobs stay green when a rename leaves the filter matching no tests.
  for (const job of [unlabelled(), labelled()]) {
    const steps = stepsOf(job);

    assert.ok(
      steps.some((step) => /dotnet test/.test(step) && /--logger/.test(step) && /trx/.test(step)),
      'a test run writes no results file',
    );
    assert.ok(
      steps.some((step) => /assert-tests-ran/.test(step)),
      'a test run is never checked for having run anything',
    );
  }
});

test('the provider the chat card shares keeps its tripwire', () => {
  // The chat card enables itself through the same connected services provider Rock IQ and
  // Knowledge Base use, and their tests are what catches a change to it. None of them are in the
  // chat namespace the unit filter names, so they need a run of their own.
  const steps = stepsOf(unlabelled());

  assert.ok(
    // The full namespace: a loose substring also matches the chat entry tests the unit run
    // already counts, and the floor below would then be met without the provider's own.
    steps.some((step) => /dotnet test/.test(step) && /Rock\.Tests\.Configuration\.ConnectedServices/.test(step)),
    'nothing runs the connected services tests under their own namespace',
  );
  assert.ok(
    steps.some((step) => /assert-tests-ran/.test(step) && /connected-services\.trx/.test(step) && /--minimum \d+/.test(step)),
    'the connected services run is not held to the number of tests it had',
  );
  assert.ok(
    steps.some((step) => /\bjest\b/.test(step) && /Administration/.test(step)),
    'the client run does not reach the card, which lives under Administration',
  );
});

test('the integration suite runs only when the label asks for it', () => {
  const job = labelled();
  const condition = String(job.if);

  assert.match(condition, /labels/, "the condition does not read the pull request's labels");
  assert.ok(
    stepsOf(job).some((step) => integrationProject.test(step) && /dotnet test/.test(step)),
    'the gated job does not run the integration project',
  );

  // Without the labeled event the label cannot start a run, so the job would only ever fire on a
  // later push to an already-labelled pull request.
  const triggers = workflow().on?.pull_request?.types ?? [];
  assert.ok(triggers.includes('labeled'), 'the workflow does not listen for a label being added');
});
