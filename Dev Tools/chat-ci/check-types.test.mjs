// The chat client is held to its types in two halves. The type check reads every chat file,
// templates included, against what it imports, and fails on a chat error while printing an error in
// a file outside the chat as a note, since the rest of the Communication project is upstream Rock's.
// The bag comparison holds each chat view model bag in C# to its TypeScript declaration by member
// name, which is the half the type check cannot do: a member that exists only in C# is never read
// by a typed client, so nothing else notices it. Both halves are run here the way CI runs them.
import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { cpSync, mkdtempSync, rmSync, symlinkSync, unlinkSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join, resolve } from 'node:path';
import test from 'node:test';
import { fileURLToPath } from 'node:url';

const here = fileURLToPath(new URL('.', import.meta.url));
const repoRoot = fileURLToPath(new URL('../../', import.meta.url));
const fixtures = resolve(here, 'check-types-fixtures');
const blocksModules = resolve(repoRoot, 'Rock.JavaScript.Obsidian.Blocks/node_modules');

function run(...args) {
  return spawnSync(process.execPath, ['check-types.mjs', ...args], { cwd: here, encoding: 'utf8' });
}

/**
 * Copies a fixture project somewhere of its own with the blocks package's modules beside it. The
 * template checker only sets itself up when it finds vue in a node_modules folder at or above the
 * project's config, so a fixture checked in place would have its script checked and its template
 * silently passed; that is how an earlier probe concluded templates were never checked.
 */
function withProject(name, use, { linkModules = true } = {}) {
  const dir = mkdtempSync(join(tmpdir(), 'chat-types-'));
  const modules = join(dir, 'node_modules');
  try {
    cpSync(resolve(fixtures, name), dir, { recursive: true });
    if (linkModules) {
      symlinkSync(blocksModules, modules, 'junction');
    }
    return use(dir);
  }
  finally {
    // The link goes first and alone, so removing the copy can never reach through it.
    try {
      unlinkSync(modules);
    }
    catch {
      // never linked
    }
    rmSync(dir, { recursive: true, force: true });
  }
}

// The same run as the workflow's type gate step, as every gate here has a cell for today's tree
// beside its own step: the cell is what turned red on the errors fixed at the source, and the step
// is the gate's own line in the log. Folding one into the other would tie the workflow to a test
// file's name, for a run of seconds.
test('the chat client has no type errors', () => {
  const result = run();

  assert.equal(result.status, 0, `${result.stdout}\n${result.stderr}`);
  assert.match(result.stdout, /no type errors in the chat client/);
  assert.match(result.stdout, /bags agree with their declarations/);
});

test('a chat file reading a member its type does not declare fails, in a script and in a template', () => {
  withProject('red', (dir) => {
    const result = run('--project', join(dir, 'tsconfig.json'), '--chat', join(dir, 'Chat'));

    assert.equal(result.status, 1, `${result.stdout}\n${result.stderr}`);
    assert.match(result.stderr, /Chat[\\/]readsUndeclared\.ts\(5,16\): TS2339/);
    assert.match(result.stderr, /Chat[\\/]templateReadsUndeclared\.obs\(2,\d+\): TS2339/);
  });
});

test('an error in a file outside the chat is printed as a note and does not fail', () => {
  withProject('clean', (dir) => {
    const result = run('--project', join(dir, 'tsconfig.json'), '--chat', join(dir, 'Chat'));

    assert.equal(result.status, 0, `${result.stdout}\n${result.stderr}`);
    assert.match(result.stdout, /outside the chat, not failing/);
    assert.match(result.stdout, /Other[\\/]upstreamError\.ts\(2,14\): TS2322/);
    assert.match(result.stdout, /no type errors in the chat client/);
  });
});

// The type check counts errors, so it cannot see a chat file it never read. Two things it does not
// own decide that: the project's include, which is upstream Rock's file, and whether vue resolves
// from the project's folder, which is what turns template checking on. Each is checked in the run.
test('a chat file the type check never read fails by name, though nothing in it is wrong', () => {
  withProject('left-out', (dir) => {
    const result = run('--project', join(dir, 'tsconfig.json'), '--chat', join(dir, 'Chat'));

    assert.equal(result.status, 1, `${result.stdout}\n${result.stderr}`);
    assert.match(result.stderr, /chat files the type check never read/);
    assert.match(result.stderr, /Chat[\\/]readsDeclared\.ts/);
    assert.match(result.stderr, /Chat[\\/]templateUnread\.obs/);
  });
});

test('a project from whose folder vue does not resolve fails, since its templates would go unchecked', () => {
  withProject('clean', (dir) => {
    const result = run('--project', join(dir, 'tsconfig.json'), '--chat', join(dir, 'Chat'));

    assert.equal(result.status, 1, `${result.stdout}\n${result.stderr}`);
    assert.match(result.stderr, /vue does not resolve from/);
  }, { linkModules: false });
});

test('a chat folder holding no chat files fails, rather than passing on nothing', () => {
  withProject('clean', (dir) => {
    const result = run('--project', join(dir, 'tsconfig.json'), '--chat', join(dir, 'NoSuchFolder'));

    assert.equal(result.status, 1, `${result.stdout}\n${result.stderr}`);
    assert.match(result.stderr, /no chat files under/);
  });
});

test('a checker that dies after naming only errors outside the chat fails, since its run never finished', () => {
  withProject('clean', (dir) => {
    const result = run('--project', join(dir, 'tsconfig.json'), '--chat', join(dir, 'Chat'), '--checker', join(fixtures, 'crashing-checker.mjs'));

    assert.equal(result.status, 1, `${result.stdout}\n${result.stderr}`);
    assert.match(result.stderr, /the type checker exited 1/);
  });
});

test('a type check that fails without naming an error fails, rather than passing on nothing', () => {
  const result = run('--project', join(fixtures, 'no-such-project', 'tsconfig.json'), '--chat', join(fixtures, 'no-such-project'));

  assert.equal(result.status, 1, `${result.stdout}\n${result.stderr}`);
});

// The generator takes a class's public properties with no flags: its own, static ones included,
// and the public instance ones it inherits, whatever the body (accessors or an expression), an
// indexer under the name Item, and an override once.
test('bags agree with their declarations once every property the generator writes is counted, base box members, statics, expression bodies and an indexer included', () => {
  const result = run('--cs', join(fixtures, 'bags/cs'), '--ts', join(fixtures, 'bags/ts'), '--folder', 'Blocks/Chat/Agrees');

  assert.equal(result.status, 0, `${result.stdout}\n${result.stderr}`);
  assert.match(result.stdout, /1 bags agree with their declarations/);
});

test('a bag member its declaration lacks, a declared member the bag lacks and a bag with no declaration each fail by name', () => {
  const result = run('--cs', join(fixtures, 'bags/cs'), '--ts', join(fixtures, 'bags/ts'), '--folder', 'Blocks/Chat');

  assert.equal(result.status, 1, `${result.stdout}\n${result.stderr}`);
  assert.match(result.stderr, /DisagreesBag\.cs: NickName is not declared as nickName/);
  assert.match(result.stderr, /DisagreesBag\.cs: FullName is not declared as fullName/);
  assert.match(result.stderr, /DisagreesBag\.cs: DefaultName is not declared as defaultName/);
  assert.match(result.stderr, /DisagreesBag\.cs: title is declared but is not a member/);
  assert.match(result.stderr, /UndeclaredBag\.cs: no declaration at .*undeclaredBag\.d\.ts/);
  assert.doesNotMatch(result.stderr, /AgreesBox/);
});
