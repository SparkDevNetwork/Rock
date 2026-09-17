// Runs every chat gate locally, in the order .github/workflows/chat-ci.yml runs them, and stops at
// the first failure. Prints one line per gate with its wall time and a total, so the numbers can be
// recorded. The integration suite is not here: it needs a database and runs on a label.
//
// MSBuild is used when it is on the path, because that is what builds this solution in the
// pipeline; otherwise the .NET SDK builds the same project, which is the same compiler with a
// different front door.
import { spawnSync } from 'node:child_process';
import { resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = fileURLToPath(new URL('.', import.meta.url));
const repoRoot = resolve(here, '../..');

const testProject = 'Rock.Tests/Rock.Tests.csproj';
// The unit test project does not reference the blocks project, so a chat block would
// otherwise never be compiled by this run.
const blocksProject = 'Rock.Blocks/Rock.Blocks.csproj';
const chatTests = 'FullyQualifiedName~Communication.Chat.Platform';
// The chat card enables itself through the connected services provider Rock IQ and Knowledge Base
// also use, and their tests are what catches a change to it. None of them are in the chat
// namespace, so they get a run of their own rather than a wider filter that would stop meaning
// "chat".
//
// The full namespace, not a loose substring: "Configuration.ConnectedServices" on its own also
// matches the chat entry tests the run above already counts, and a floor set with those included
// would let real provider tests disappear unnoticed.
const connectedServicesTests = 'FullyQualifiedName~Rock.Tests.Configuration.ConnectedServices';
const clientProject = resolve(repoRoot, 'Rock.JavaScript.Obsidian.Blocks');

function has(command) {
  const probe = spawnSync(command, ['-version'], { stdio: 'ignore', shell: true });
  return probe.status === 0;
}

const msbuild = has('msbuild');

const gates = [
  {
    name: 'the chat files name nothing a reader here cannot open',
    cmd: process.execPath,
    args: ['check-references.mjs'],
    cwd: here,
  },
  {
    name: 'chat view models have matching TypeScript members',
    cmd: process.execPath,
    args: ['check-chat-types.mjs'],
    cwd: here,
  },
  {
    name: 'the gates keep the shape the pipeline assumes',
    cmd: process.execPath,
    args: ['--test'],
    cwd: here,
  },
  msbuild
    ? {
        name: 'build the unit test project and what it depends on',
        cmd: 'msbuild',
        args: [testProject, '/t:Restore,Build', '/p:Configuration=Debug', '/m', '/v:minimal', '/nologo'],
        cwd: repoRoot,
        shell: true,
      }
    : {
        name: 'build the unit test project and what it depends on (no msbuild on the path)',
        cmd: 'dotnet',
        args: ['build', testProject, '--configuration', 'Debug', '--verbosity', 'minimal'],
        cwd: repoRoot,
      },
  msbuild
    ? {
        name: 'build the blocks project, which holds the chat blocks',
        cmd: 'msbuild',
        args: [blocksProject, '/t:Restore,Build', '/p:Configuration=Debug', '/m', '/v:minimal', '/nologo'],
        cwd: repoRoot,
        shell: true,
      }
    : {
        name: 'build the blocks project, which holds the chat blocks (no msbuild on the path)',
        cmd: 'dotnet',
        args: ['build', blocksProject, '--configuration', 'Debug', '--verbosity', 'minimal'],
        cwd: repoRoot,
      },
  {
    name: 'unit tests',
    cmd: 'dotnet',
    args: [
      'test',
      testProject,
      '--no-build',
      '--filter',
      chatTests,
      '--logger',
      'trx;LogFileName=unit.trx',
      '--results-directory',
      'TestResults',
    ],
    cwd: repoRoot,
  },
  {
    // A filter that matches nothing exits zero, so the run is accounted for from its own results.
    name: 'the unit run ran something',
    cmd: process.execPath,
    args: [resolve(here, 'assert-tests-ran.mjs'), 'TestResults/unit.trx', '--minimum', '1'],
    cwd: repoRoot,
  },
  {
    name: 'connected services tests, the shared-provider tripwire',
    cmd: 'dotnet',
    args: [
      'test',
      testProject,
      '--no-build',
      '--filter',
      connectedServicesTests,
      '--logger',
      'trx;LogFileName=connected-services.trx',
      '--results-directory',
      'TestResults',
    ],
    cwd: repoRoot,
  },
  {
    // 78 is what that namespace holds today, measured: 59 provider tests and 19 encoder tests.
    name: 'the tripwire still has its tests',
    cmd: process.execPath,
    args: [resolve(here, 'assert-tests-ran.mjs'), 'TestResults/connected-services.trx', '--minimum', '78'],
    cwd: repoRoot,
  },
  {
    name: 'client tests',
    cmd: 'npx',
    args: ['jest', 'tests/Communication/Chat', 'tests/Administration/sparkConnectedServices'],
    cwd: clientProject,
    shell: true,
  },
];

let total = 0;

for (const gate of gates) {
  const started = Date.now();
  // Only the commands that are shell scripts on Windows go through a shell: node's own path holds
  // a space, and a shell splits it.
  const result = spawnSync(gate.cmd, gate.args, { cwd: gate.cwd, stdio: 'inherit', shell: gate.shell === true });
  const seconds = ((Date.now() - started) / 1000).toFixed(1);
  total += Number(seconds);

  const status = result.status ?? 1;
  console.log(`${status === 0 ? 'ok  ' : 'FAIL'}  ${seconds}s  ${gate.name}`);

  if (status !== 0) {
    console.error(`\nstopped at: ${gate.name}`);
    process.exit(status);
  }
}

console.log(`\nevery chat gate passed in ${total.toFixed(1)}s`);
