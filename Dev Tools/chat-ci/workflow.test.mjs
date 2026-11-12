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

test('the client cells that need daylight saving run in their own zone', () => {
  // jest cannot change the zone inside a run, so the main run alone would never reach them
  const steps = (unlabelled().steps ?? []).filter((step) => String(step['working-directory'] ?? '').replace(/^\.\//, '') === 'Rock.JavaScript.Obsidian.Blocks');

  assert.ok(
    steps.some((step) => /\bjest\b/.test(String(step.run ?? '')) && /--config tests\/Communication\/Chat\/jest\.zone\.config\.js/.test(String(step.run ?? ''))),
    'no step runs the chat client cells in a time zone with daylight saving',
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

  // The harness defaults to a container a Windows runner cannot start; the shared script is what
  // points it at LocalDB, and the platform's nightly run calls the same script.
  const steps = stepsOf(job);
  const setup = steps.findIndex((step) => /use-localdb\.ps1/.test(step));
  const run = steps.findIndex((step) => integrationProject.test(step) && /dotnet test/.test(step));
  assert.ok(setup >= 0 && setup < run, 'the gated job tests before the shared script points the harness at LocalDB');

  // The harness names a database image by its last migration alone, so only an exact key over
  // everything the image is built from may restore one.
  const image = (job.steps ?? []).find((step) => /RockUnitTests/.test(String(step.with?.path ?? '')));
  assert.ok(image, 'nothing restores the test database image');
  for (const input of ['Rock.Migrations/Migrations/**', 'Rock.Tests.Integration/TestData/SampleData/**']) {
    assert.ok(String(image.with.key).includes(`'${input}'`), `the image key does not name ${input}`);
  }
  assert.ok(!image.with['restore-keys'], 'an image built from other migrations would stand in for a fresh one');
});

test('every msbuild call restores in its own evaluation, so a package adapter is copied', () => {
  // Restore and Build as targets of one call share an evaluation made before restore wrote the
  // packages' build files, so MSTest's adapter never reaches the output and the test run finds
  // nothing; measured on a runner and reproduced locally. The -restore switch evaluates again.
  const calls = [
    ...jobs().flatMap(([, job]) => (job.steps ?? []).map((step) => String(step.run ?? ''))),
    readFileSync(resolve(repoRoot, 'Dev Tools/chat-ci/ci.mjs'), 'utf8'),
  ];
  for (const call of calls) {
    assert.doesNotMatch(call, /\/t:Restore/i, `restore as a target: ${call.slice(0, 120)}`);
  }
  const builds = jobs().flatMap(([, job]) => (job.steps ?? []).map((step) => String(step.run ?? ''))).filter((run) => /^msbuild /.test(run));
  assert.ok(builds.length >= 3, 'the workflow builds with msbuild');
  for (const build of builds) {
    assert.match(build, / -restore /, `no -restore: ${build}`);
  }
});

test('the client suite runs with the framework package installed as well as the blocks package', () => {
  // The chat components import the shared framework, whose compiled helpers come from tslib in the
  // framework package's own node_modules; with only the blocks package installed, eight suites fail
  // to resolve it on a runner while passing on any machine that has built the framework.
  const steps = unlabelled().steps ?? [];
  const installIn = (dir) => steps.findIndex((step) => /npm ci/.test(String(step.run ?? '')) && String(step['working-directory'] ?? '').replace(/^\.\//, '') === dir);
  const jest = steps.findIndex((step) => /\bjest\b/.test(String(step.run ?? '')));

  assert.ok(installIn('Rock.JavaScript.Obsidian') >= 0, 'nothing installs the framework package');
  assert.ok(installIn('Rock.JavaScript.Obsidian.Blocks') >= 0, 'nothing installs the blocks package');
  assert.ok(installIn('Rock.JavaScript.Obsidian') < jest && installIn('Rock.JavaScript.Obsidian.Blocks') < jest, 'the client tests run before an install');
});

test('the chat client is type-checked once the framework types it imports are built, and the gate tests run after', () => {
  // The type check reads the framework through its built declarations, so they are built after
  // the framework is installed and before the check. The gates' own tests check fixtures with the
  // blocks package's type checker, so they too wait for both installs.
  const steps = unlabelled().steps ?? [];
  const dirOf = (step) => String(step['working-directory'] ?? '').replace(/^\.\//, '');
  const installIn = (dir) => steps.findIndex((step) => /npm ci/.test(String(step.run ?? '')) && dirOf(step) === dir);
  const buildTypes = steps.findIndex((step) => /npm run build:types/.test(String(step.run ?? '')) && dirOf(step) === 'Rock.JavaScript.Obsidian');
  const typeGate = steps.findIndex((step) => /check:types|check-types\.mjs/.test(String(step.run ?? '')));
  const gateTests = steps.findIndex((step) => /npm test/.test(String(step.run ?? '')) && dirOf(step) === 'Dev Tools/chat-ci');
  const framework = installIn('Rock.JavaScript.Obsidian');
  const blocks = installIn('Rock.JavaScript.Obsidian.Blocks');

  assert.ok(buildTypes >= 0, 'nothing builds the framework types');
  assert.ok(typeGate >= 0, 'the type gate never runs');
  assert.ok(framework >= 0 && framework < buildTypes, 'the framework types are built before the framework is installed');
  assert.ok(buildTypes < typeGate && blocks < typeGate, 'the type gate runs before what it reads is in place');
  assert.ok(gateTests > buildTypes && gateTests > blocks, 'the gate tests run before the type checker they use is installed');
});

test('the local run lists the type gate where the workflow runs it', () => {
  const local = readFileSync(resolve(repoRoot, 'Dev Tools/chat-ci/ci.mjs'), 'utf8');
  const at = (text) => local.indexOf(text);

  assert.ok(at("'build:types'") >= 0, 'the local run does not build the framework types');
  assert.ok(at("'check-types.mjs'") >= 0, 'the local run does not run the type gate');
  assert.ok(at("'build:types'") < at("args: ['--test']"), 'the local run tests the gates before the framework types are built');
  assert.ok(at("args: ['--test']") < at("'check-types.mjs'"), 'the local run checks types before testing the gates, unlike the workflow');
  assert.ok(at("'check-types.mjs'") < at('testProject, '), 'the local run builds before the type gate, unlike the workflow');
});

test('an install is skipped only when the cache holds every installed folder for the same lockfiles', () => {
  // A restored folder stands in for an install only while the lockfile it was installed from is
  // unchanged, so the key names all three lockfiles and every install reads the exact hit.
  const steps = unlabelled().steps ?? [];
  const cache = steps.find((step) => String(step.uses ?? '').startsWith('actions/cache') && /node_modules/.test(String(step.with?.path ?? '')));
  assert.ok(cache, 'nothing restores the installed packages');
  for (const dir of ['Dev Tools/chat-ci', 'Rock.JavaScript.Obsidian', 'Rock.JavaScript.Obsidian.Blocks']) {
    assert.ok(String(cache.with.path).split('\n').map((line) => line.trim()).includes(`${dir}/node_modules`), `${dir} is not restored`);
    assert.ok(String(cache.with.key).includes(`'${dir}/package-lock.json'`), `the key does not name ${dir}'s lockfile`);
    // A manifest changed without its lockfile is what npm ci refuses, so a hit must not skip it.
    assert.ok(String(cache.with.key).includes(`'${dir}/package.json'`), `the key does not name ${dir}'s manifest`);
  }
  assert.ok(!cache.with['restore-keys'], 'a partial match would stand in for an install from another lockfile');
  const installs = steps.filter((step) => /npm ci/.test(String(step.run ?? '')));
  assert.equal(installs.length, 3);
  for (const install of installs) {
    assert.match(String(install.if ?? ''), new RegExp(`steps\\.${cache.id}\\.outputs\\.cache-hit != 'true'`), `${install.name} runs whatever the cache held`);
  }
});
