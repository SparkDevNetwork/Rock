// The chat client is held to its types in two halves. The type check reads every chat file,
// templates included, against what it imports, and fails on a chat error while printing an error in
// a file outside the chat as a note, since the rest of the Communication project is upstream Rock's.
// The bag comparison holds each chat view model bag in C# to its TypeScript declaration by member
// name, which is the half the type check cannot do: a member that exists only in C# is never read
// by a typed client, so nothing else notices it. Both halves are run here the way CI runs them.
import assert from 'node:assert/strict';
import { spawnSync } from 'node:child_process';
import { cpSync, mkdtempSync, readFileSync, rmSync, symlinkSync, unlinkSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { isAbsolute, join, resolve } from 'node:path';
import test from 'node:test';
import { fileURLToPath } from 'node:url';

const here = fileURLToPath(new URL('.', import.meta.url));
const repoRoot = fileURLToPath(new URL('../../', import.meta.url));
const fixtures = resolve(here, 'check-types-fixtures');
const blocksPackage = resolve(repoRoot, 'Rock.JavaScript.Obsidian.Blocks');
const blocksModules = resolve(blocksPackage, 'node_modules');

function run(args, env = {}) {
  return spawnSync(process.execPath, ['check-types.mjs', ...args], { cwd: here, encoding: 'utf8', env: { ...process.env, ...env } });
}

/** The type check of a fixture copy, its probe written inside the copy rather than in the lane. */
function checkProject(dir, extra = [], env = {}) {
  return run(['--project', join(dir, 'tsconfig.json'), '--chat', join(dir, 'Chat'), '--probe', join(dir, '.chat-type-probe'), ...extra], env);
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

/** Adds compiler options to a fixture copy's config. */
function addOptions(dir, options) {
  const config = join(dir, 'tsconfig.json');
  writeFileSync(config, readFileSync(config, 'utf8').replace('"strict": true,', `"strict": true,\n    ${options}`));
}

/** The program a config's checker lists, apart from installed packages. */
function programOf(config) {
  const listed = spawnSync(process.execPath, [resolve(blocksModules, 'vue-tsc/bin/vue-tsc.js'), '--listFilesOnly', '-p', config], { cwd: blocksPackage, encoding: 'utf8', maxBuffer: 64 * 1024 * 1024 });
  return listed.stdout.split(/\r?\n/).map((line) => line.trim()).filter((line) => isAbsolute(line) && !/node_modules/.test(line)).map((line) => resolve(line).toLowerCase());
}

// The same run as the workflow's type gate step, as every gate here has a cell for today's tree
// beside its own step: the cell is what turned red on the errors fixed at the source, and the step
// is the gate's own line in the log. Folding one into the other would tie the workflow to a test
// file's name, for a run of seconds. The probe's program must hold every file of Rock's project, or
// the gate would pass having checked less than the project it names.
test('the chat client has no type errors', () => {
  const result = run([]);

  assert.equal(result.status, 0, `${result.stdout}\n${result.stderr}`);
  assert.match(result.stdout, /no type errors in the chat client/);
  assert.match(result.stdout, /bags agree with their declarations/);
  assert.doesNotMatch(`${result.stdout}\n${result.stderr}`, /chat-type-probe/);

  const project = programOf(resolve(blocksPackage, 'src/Communication/tsconfig.json'));
  const probed = new Set(programOf(resolve(blocksPackage, 'dist/.chat-type-probe/tsconfig.json')));
  assert.ok(project.some((file) => file.endsWith(join('chat', 'chatshell.obs'))), 'the project lists its files');
  assert.deepEqual(project.filter((file) => !probed.has(file)), []);
});

test('a chat file reading a member its type does not declare fails, in a script and in a template', () => {
  withProject('red', (dir) => {
    const result = checkProject(dir);

    assert.equal(result.status, 1, `${result.stdout}\n${result.stderr}`);
    assert.match(result.stderr, /Chat[\\/]readsUndeclared\.ts\(5,16\): TS2339/);
    assert.match(result.stderr, /Chat[\\/]templateReadsUndeclared\.obs\(2,\d+\): TS2339/);
  });
});

test('an error in a file outside the chat is printed as a note and does not fail', () => {
  withProject('clean', (dir) => {
    const result = checkProject(dir);

    assert.equal(result.status, 0, `${result.stdout}\n${result.stderr}`);
    assert.match(result.stdout, /outside the chat, not failing/);
    assert.match(result.stdout, /Other[\\/]upstreamError\.ts\(2,14\): TS2322/);
    assert.match(result.stdout, /no type errors in the chat client/);
    assert.doesNotMatch(`${result.stdout}\n${result.stderr}`, /did not check|chat-type-probe/);
  });
});

// The proof the run checked anything: a probe with one known script error and one known template
// error rides in the chat's own program, and the gate passes only if both are reported. Whatever
// stops tsc checking types (a syntax error in any file, an option it no longer accepts, a stand-in
// that only lists files, a setup that skips templates) stops the probe's errors too.
test('a checker that lists the whole program and reports nothing fails, since it checked nothing', () => {
  withProject('clean', (dir) => {
    const result = checkProject(dir, ['--checker', join(fixtures, 'listing-checker.cjs')]);

    assert.equal(result.status, 1, `${result.stdout}\n${result.stderr}`);
    assert.match(result.stderr, /the type check did not check scripts and templates/);
  });
});

test('a checker that reports the script probe but not the template probe fails naming templates', () => {
  withProject('clean', (dir) => {
    const result = checkProject(dir, ['--checker', join(fixtures, 'listing-checker.cjs')], { REPORT: 'scripts' });

    assert.equal(result.status, 1, `${result.stdout}\n${result.stderr}`);
    assert.match(result.stderr, /the type check did not check templates:/);
  });
});

test('a syntax error outside the chat, which stops tsc checking any types, fails naming its file', () => {
  withProject('red', (dir) => {
    writeFileSync(join(dir, 'Other', 'brokenSyntax.ts'), 'export const broken = ;\n');
    const result = checkProject(dir);

    assert.equal(result.status, 1, `${result.stdout}\n${result.stderr}`);
    assert.match(result.stderr, /the type check did not check scripts and templates/);
    assert.match(result.stderr, /Other[\\/]brokenSyntax\.ts\(1,23\): TS1109/);
  });
});

test('an option the checker no longer accepts, which stops tsc checking any types, fails naming the option', () => {
  withProject('red', (dir) => {
    addOptions(dir, '"preserveValueImports": true,');
    const result = checkProject(dir);

    assert.equal(result.status, 1, `${result.stdout}\n${result.stderr}`);
    assert.match(result.stderr, /the type check did not check scripts and templates/);
    assert.match(result.stderr, /TS5102 Option 'preserveValueImports' has been removed/);
  });
});

// The one option that decides whether tsc checks at all is the gate's own to set, as --noEmit is,
// so a config that turns checking off still has the chat checked.
test('noCheck in the project config does not stop the chat being checked', () => {
  withProject('red', (dir) => {
    addOptions(dir, '"noCheck": true,');
    const result = checkProject(dir);

    assert.equal(result.status, 1, `${result.stdout}\n${result.stderr}`);
    assert.match(result.stderr, /Chat[\\/]readsUndeclared\.ts\(5,16\): TS2339/);
    assert.match(result.stderr, /Chat[\\/]templateReadsUndeclared\.obs\(2,\d+\): TS2339/);
  });
});

test('an option tsc does not know, after which it still checks types, is a note', () => {
  withProject('clean', (dir) => {
    addOptions(dir, '"notARealOption": true,');
    const result = checkProject(dir);

    assert.equal(result.status, 0, `${result.stdout}\n${result.stderr}`);
    assert.match(result.stdout, /outside the chat, not failing/);
    assert.match(result.stdout, /TS5023 Unknown compiler option 'notARealOption'/);
    assert.match(result.stdout, /no type errors in the chat client/);
  });
});

// Rock's config is incremental, as the fixtures' are, so every run after the first reads its cache
// and skips writing it, and tsc then ends a run that found errors with 1 rather than 2. CI runs the
// check twice in a job, so the second, warm run must reach the same verdict as the first, the
// probe's errors replayed from the cache as the chat's are.
test('a warm incremental run reaches the same verdict as a cold one', () => {
  withProject('clean', (dir) => {
    for (const pass of ['cold', 'warm']) {
      const result = checkProject(dir);

      assert.equal(result.status, 0, `${pass}: ${result.stdout}\n${result.stderr}`);
      assert.match(result.stdout, /Other[\\/]upstreamError\.ts\(2,14\): TS2322/, pass);
    }
  });
  withProject('red', (dir) => {
    for (const pass of ['cold', 'warm']) {
      const result = checkProject(dir);

      assert.equal(result.status, 1, `${pass}: ${result.stdout}\n${result.stderr}`);
      assert.match(result.stderr, /Chat[\\/]readsUndeclared\.ts\(5,16\): TS2339/, pass);
      assert.match(result.stderr, /Chat[\\/]templateReadsUndeclared\.obs\(2,\d+\): TS2339/, pass);
      assert.doesNotMatch(result.stderr, /did not check/, pass);
    }
  });
});

// The probe proves the checker checked, but not which files it checked: the project's include,
// upstream Rock's file, decides that, so every chat file must be in the program the run lists.
test('a chat file the type check never read fails by name, though nothing in it is wrong', () => {
  withProject('left-out', (dir) => {
    const result = checkProject(dir);

    assert.equal(result.status, 1, `${result.stdout}\n${result.stderr}`);
    assert.match(result.stderr, /chat files the type check never read/);
    assert.match(result.stderr, /Chat[\\/]readsDeclared\.ts/);
    assert.match(result.stderr, /Chat[\\/]templateUnread\.obs/);
  });
});

// The template checker sets itself up only when vue resolves from the checked config's folder.
test('a project from whose folder vue does not resolve fails, since its templates would go unchecked', () => {
  withProject('clean', (dir) => {
    const result = checkProject(dir);

    assert.equal(result.status, 1, `${result.stdout}\n${result.stderr}`);
    assert.match(result.stderr, /the type check did not check templates:/);
  }, { linkModules: false });
});

test('a chat folder holding no chat files fails, rather than passing on nothing', () => {
  withProject('clean', (dir) => {
    const result = run(['--project', join(dir, 'tsconfig.json'), '--chat', join(dir, 'NoSuchFolder'), '--probe', join(dir, '.chat-type-probe')]);

    assert.equal(result.status, 1, `${result.stdout}\n${result.stderr}`);
    assert.match(result.stderr, /no chat files under/);
  });
});

test('a checker that dies after naming only errors outside the chat fails, printing its crash', () => {
  withProject('clean', (dir) => {
    const result = checkProject(dir, ['--checker', join(fixtures, 'crashing-checker.cjs')]);

    assert.equal(result.status, 1, `${result.stdout}\n${result.stderr}`);
    assert.match(result.stderr, /the type check did not check scripts and templates/);
    assert.match(result.stderr, /the checker crashed/);
  });
});

test('a project config that cannot be read fails, rather than passing on nothing', () => {
  const missing = join(fixtures, 'no-such-project');
  const result = run(['--project', join(missing, 'tsconfig.json'), '--chat', missing, '--probe', join(missing, '.chat-type-probe')]);

  assert.equal(result.status, 1, `${result.stdout}\n${result.stderr}`);
  assert.match(result.stderr, /cannot read the project config/);
});

// The generator takes a class's public properties with no flags: its own, static ones included,
// and the public instance ones it inherits, whatever the body (accessors or an expression), an
// indexer under the name Item, and an override once.
test('bags agree with their declarations once every property the generator writes is counted, base box members, statics, expression bodies and an indexer included', () => {
  const result = run(['--cs', join(fixtures, 'bags/cs'), '--ts', join(fixtures, 'bags/ts'), '--folder', 'Blocks/Chat/Agrees']);

  assert.equal(result.status, 0, `${result.stdout}\n${result.stderr}`);
  assert.match(result.stdout, /1 bags agree with their declarations/);
});

test('a bag member its declaration lacks, a declared member the bag lacks and a bag with no declaration each fail by name', () => {
  const result = run(['--cs', join(fixtures, 'bags/cs'), '--ts', join(fixtures, 'bags/ts'), '--folder', 'Blocks/Chat']);

  assert.equal(result.status, 1, `${result.stdout}\n${result.stderr}`);
  assert.match(result.stderr, /DisagreesBag\.cs: NickName is not declared as nickName/);
  assert.match(result.stderr, /DisagreesBag\.cs: FullName is not declared as fullName/);
  assert.match(result.stderr, /DisagreesBag\.cs: DefaultName is not declared as defaultName/);
  assert.match(result.stderr, /DisagreesBag\.cs: title is declared but is not a member/);
  assert.match(result.stderr, /UndeclaredBag\.cs: no declaration at .*undeclaredBag\.d\.ts/);
  assert.doesNotMatch(result.stderr, /AgreesBox/);
});
