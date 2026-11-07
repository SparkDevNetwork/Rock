// Holds the chat client to its types, in two halves.
//
// The type check runs the blocks package's own vue-tsc over Rock's Communication project, which
// holds the chat and Spark's other Communication blocks, and checks templates as well as scripts.
// An error in a chat file fails; an error anywhere else is printed as a note and does not fail,
// because the rest of the project is upstream Rock's and a merge from it must never turn the chat
// red by its types. It reads the framework through its built declarations, so the framework's
// types are built first (npm run build:types in Rock.JavaScript.Obsidian).
//
// What a pass proves: the run checked scripts and templates, under the project's own options, for
// files in the chat's program, and every chat file was in that program. The first comes from two
// probe files with one known error each, checked in the same program: the gate passes only if both
// are reported, so anything that stops tsc checking (a syntax error anywhere, an option it no
// longer accepts, a crash, a setup without templates) fails it. What it does not report: a
// declaration error in a chat file (TS4xxx, TS2742). tsc computes those only when no other error
// exists, and the probe always has one; the blocks are not published as declarations, so they are
// not type errors in the chat's use.
//
// The bag comparison holds each chat view model bag in C# to its TypeScript declaration by member
// name, both ways. The declarations are written by a code generator a person runs by hand, so a
// member added in C# and never generated is invisible to the type check: nothing typed reads it.
// The generator writes every property reflection calls public (the class's own, static ones
// included, and the instance ones it inherits, whatever their body), under its name in Rock's
// camelCase, which lowers a leading acronym whole (RSVPReminderOffsetDays is
// rsvpReminderOffsetDays); the comparison applies the same rules.
//
//   node check-types.mjs                                     both halves over the chat
//   node check-types.mjs --project <tsconfig> --chat <dir>   the type check of another project
//   node check-types.mjs ... --probe <dir>                   its probe written in another folder
//   node check-types.mjs ... --checker <script>              run by a stand-in for vue-tsc
//   node check-types.mjs --cs <dir> --ts <dir> --folder <f>  the bag comparison of another tree
import { spawnSync } from 'node:child_process';
import { createRequire } from 'node:module';
import { existsSync, mkdirSync, readdirSync, readFileSync, realpathSync, writeFileSync } from 'node:fs';
import { basename, dirname, isAbsolute, join, relative, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const repoRoot = fileURLToPath(new URL('../../', import.meta.url));
const blocksPackage = resolve(repoRoot, 'Rock.JavaScript.Obsidian.Blocks');
const vueTsc = resolve(blocksPackage, 'node_modules/vue-tsc/bin/vue-tsc.js');
const frameworkTypes = resolve(repoRoot, 'Rock.JavaScript.Obsidian/dist/Framework');

const chatProject = resolve(blocksPackage, 'src/Communication/tsconfig.json');
const chatFolder = resolve(blocksPackage, 'src/Communication/Chat');

// Under dist the probe is ignored by git, separate per checkout, in no build's input (the build
// reads src) and under the base config's rootDir, and vue resolves from it as from the project. A
// dot folder is matched by no ** include, so it never joins another program.
const probeFolder = resolve(blocksPackage, 'dist/.chat-type-probe');

const bagsCs = resolve(repoRoot, 'Rock.ViewModels');
const bagsTs = resolve(repoRoot, 'Rock.JavaScript.Obsidian/Framework/ViewModels');
const bagsFolder = 'Blocks/Communication/Chat';

/** One diagnostic as tsc prints it: `file(line,col): error TSnnnn: message`. */
const DIAGNOSTIC = /^(.+?)\((\d+),(\d+)\): error (TS\d+): (.*)$/;

/** A diagnostic in no file, as tsc prints a global or option error: `error TSnnnn: message`. */
const FILELESS = /^error (TS\d+): (.*)$/;

/**
 * The probe files, each with the one error it must be reported for. The script's is a strict-null
 * error, so a config that loses strict says why; the template reads a member named unlike any its
 * prop declares, so tsc reports it as missing (TS2339) rather than as a misspelling (TS2551).
 */
const PROBES = [
  {
    kind: 'scripts',
    file: 'scriptProbe.ts',
    code: 'TS2322',
    text: 'declare const maybe: number | undefined;\nexport const probe: number = maybe;\n',
  },
  {
    kind: 'templates',
    file: 'templateProbe.obs',
    code: 'TS2339',
    text: '<template>\n    <div>{{ box.notDeclaredOnTheProbe }}</div>\n</template>\n\n'
      + '<script setup lang="ts">\n    defineProps<{ box: { title: string } }>();\n</script>\n',
  },
];

// #region Type check

/**
 * A path as the file system knows it, so a listed file and a walked one meet whatever their
 * slashes, case or short Windows names (a temp folder is often reached as MAXWEL~1).
 */
function pathKey(file) {
  let real = resolve(file);
  try {
    real = realpathSync.native(real);
  }
  catch {
    // a listed file that is gone is compared as written
  }
  return process.platform === 'win32' ? real.toLowerCase() : real;
}

/**
 * Writes the probe folder: the two probe files and a config that is the project's own with the
 * probes added. It extends the project and copies its references, the one field extends does not
 * carry, so it is checked as the project is in every setting; its files are the project's own and
 * the probes. Returns the config, or why the project could not be read or the probe written.
 */
function writeProbe(project, probe) {
  // tsc's own reader, so a config with comments or trailing commas reads as tsc reads it.
  const ts = createRequire(join(blocksPackage, 'package.json'))('typescript');
  const { config, error } = ts.readConfigFile(project, ts.sys.readFile);
  if (error) {
    return { failure: `cannot read the project config ${project}: ${ts.flattenDiagnosticMessageText(error.messageText, '\n')}` };
  }

  const fromProject = (path) => resolve(dirname(project), path);
  const wrapper = {
    extends: project,
    ...(config.references ? { references: config.references.map((reference) => ({ ...reference, path: fromProject(reference.path) })) } : {}),
    files: [...(config.files ?? []).map(fromProject), ...PROBES.map(({ file }) => join(probe, file))],
  };
  const tsconfig = join(probe, 'tsconfig.json');
  try {
    mkdirSync(probe, { recursive: true });
    for (const { file, text } of PROBES) {
      writeFileSync(join(probe, file), text);
    }
    writeFileSync(tsconfig, `${JSON.stringify(wrapper, null, 2)}\n`);
  }
  catch (writeError) {
    return { failure: `cannot write the probe folder ${probe}: ${writeError.message}` };
  }
  return { tsconfig };
}

/**
 * Runs the type check over a project and sorts its errors into the chat's and everyone else's.
 * Paths are printed relative to the blocks package, where the checker runs.
 *
 * Counting errors cannot tell a clean chat from a chat never checked, so the same run proves it
 * checked: both probe errors must be reported, and every chat file must be in the program it lists.
 */
function checkTypes(project, chat, probe, checker) {
  if (!existsSync(checker)) {
    return { failed: [`no type checker at ${checker}: install the blocks package (npm ci in Rock.JavaScript.Obsidian.Blocks)`], notes: [], read: 0 };
  }

  const failed = [];
  const chatFiles = walk(chat, (name) => name.endsWith('.ts') || name.endsWith('.obs'));
  if (chatFiles.length === 0) {
    failed.push(`no chat files under ${chat}: the type check would pass having read none`);
  }

  const { tsconfig, failure } = writeProbe(project, probe);
  if (!tsconfig) {
    failed.push(failure);
    return { failed, notes: [], read: 0 };
  }

  const run = runChecker(checker, tsconfig);

  // A probe error is found by its file and code; any other error in a probe file is reported.
  const isProbe = (error, { file, code }) => error.file !== null && error.code === code && pathKey(resolve(blocksPackage, error.file)) === pathKey(join(probe, file));
  const missing = PROBES.filter((expected) => !run.errors.some((error) => isProbe(error, expected)));
  if (missing.length > 0) {
    const kinds = missing.map(({ kind }) => kind).join(' and ');
    failed.push(`the type check did not check ${kinds}: its probe error was not reported, so no chat error could have been either. What the checker printed, the likely cause:\n${run.said}`);
    return { failed, notes: [], read: 0 };
  }
  const errors = run.errors.filter((error) => !PROBES.some((expected) => isProbe(error, expected)));

  const unread = chatFiles.filter((file) => !run.listed.has(pathKey(file)));
  if (unread.length > 0) {
    failed.push(`chat files the type check never read (the project's include no longer reaches them):\n${unread.map((file) => `      ${relative(blocksPackage, file)}`).join('\n')}`);
  }

  const inChat = (file) => {
    const fromChat = relative(chat, resolve(blocksPackage, file));
    return fromChat !== '' && !fromChat.startsWith('..') && !isAbsolute(fromChat);
  };
  failed.push(...errors.filter((error) => error.file && inChat(error.file)).map((error) => error.text));
  const notes = errors.filter((error) => !error.file || !inChat(error.file)).map((error) => error.text);

  return { failed, notes, read: chatFiles.length - unread.length };
}

/**
 * Runs the checker once and reads what it printed: its errors, each with its file (none for a global
 * or option error) and code, and the program it listed.
 */
function runChecker(checker, tsconfig) {
  // --noCheck false is the gate's own, as --noEmit is: whether tsc checks at all is the one
  // setting a config must never decide for it. So is the output's shape, since the gate parses tsc's
  // plain output: pretty output wraps each diagnostic in colour and context, and explainFiles prints
  // the program with relative paths in place of the absolute list. The file list is over a thousand
  // lines, near 150 KB, and node kills a child whose output passes its 1 MB default; the room above
  // is for growth.
  const args = [checker, '--noEmit', '--noCheck', 'false', '--pretty', 'false', '--explainFiles', 'false', '--listFiles', '-p', tsconfig];
  const run = spawnSync(process.execPath, args, { cwd: blocksPackage, encoding: 'utf8', maxBuffer: 64 * 1024 * 1024 });
  // tsc writes its diagnostics and the file list to stdout; stderr holds only what node says, such
  // as a crash's stack, whose first line is an absolute path that must never be read as a listed file.
  const errors = [];
  const listed = new Set();
  const other = [];
  for (const line of (run.stdout ?? '').split(/\r?\n/)) {
    const match = DIAGNOSTIC.exec(line);
    const fileless = FILELESS.exec(line);
    if (match) {
      const [, file, row, col, code, message] = match;
      errors.push({ file, code, text: `${file}(${row},${col}): ${code} ${message}` });
    }
    else if (fileless) {
      errors.push({ file: null, code: fileless[1], text: `${fileless[1]} ${fileless[2]}` });
    }
    else if (errors.length > 0 && /^\s/.test(line) && line.trim()) {
      // tsc explains an error on indented lines under it
      errors[errors.length - 1].text += `\n      ${line.trim()}`;
    }
    else if (isAbsolute(line.trim())) {
      // --listFiles prints each file in the program on a line of its own, as an absolute path
      listed.add(pathKey(line.trim()));
    }
    else if (line.trim()) {
      other.push(line);
    }
  }

  const ended = `exited ${run.status ?? run.error?.message ?? run.signal}`;
  const said = [...errors.map((error) => error.text), ...other, (run.stderr ?? '').trim(), ended].filter(Boolean);
  return { errors, listed, said: said.join('\n').split('\n').map((line) => `      ${line}`).join('\n') };
}

// #endregion

// #region Bag comparison

/** Rock's string ToCamelCase, as the declaration generator calls it: a leading run of capitals is lowered whole. */
function toCamelCase(value) {
  const isUpper = (c) => c !== undefined && c !== c.toLowerCase() && c === c.toUpperCase();
  if (!value || !isUpper(value[0])) {
    return value;
  }

  const chars = [...value];
  for (let i = 0; i < chars.length; i++) {
    if (i === 1 && !isUpper(chars[i])) {
      break;
    }
    const hasNext = i + 1 < chars.length;
    if (i > 0 && hasNext && !isUpper(chars[i + 1])) {
      if (/\s/.test(chars[i + 1])) {
        chars[i] = chars[i].toLowerCase();
      }
      break;
    }
    chars[i] = chars[i].toLowerCase();
  }

  return chars.join('');
}

/** Source text with its comments taken out, so a word in a doc comment is never read as code. */
function code(text) {
  return text.replace(/\/\*[\s\S]*?\*\//g, '').replace(/\/\/.*$/gm, '');
}

/** Every file under a folder whose name passes the test, as absolute paths. */
function walk(folder, isWanted) {
  if (!existsSync(folder)) {
    return [];
  }
  return readdirSync(folder, { withFileTypes: true }).flatMap((entry) => {
    const full = join(folder, entry.name);
    return entry.isDirectory() ? walk(full, isWanted) : isWanted(entry.name) ? [full] : [];
  });
}

/**
 * One public property declaration: any modifiers and type, then a name or an indexer, then a body
 * of accessors (any of them may carry its own access modifier) or an expression.
 */
const PROPERTY = /^[ \t]*(?:\[[^\]\n]*\][ \t]*)*public\s+([\w.<>,?\[\]()\s]+?)\s+(\w+|this\s*\[[^\]]*\])\s*(?:\{\s*(?:(?:private|protected|internal)\s+)*(?:get|set|init)\b|=>)/gm;

/**
 * A C# class's name, the class it extends and its public properties, instance and static apart. A
 * base list that names only interfaces gives no base: an interface's members are declared again on
 * the class.
 */
function readClass(text) {
  const source = code(text);
  const head = /\bclass\s+(\w+)(?:<[^>]*>)?\s*(?::\s*([^{]+))?\{/.exec(source);
  if (!head) {
    return null;
  }

  const first = head[2]?.split(',')[0].trim().replace(/<.*$/, '').split('.').pop();
  const base = first && !/^I[A-Z]/.test(first) ? first : null;

  const instance = [];
  const statics = [];
  for (const [, prefix, name] of source.matchAll(PROPERTY)) {
    // An indexer is a property named Item to reflection, so the generator writes it as item.
    const property = name.startsWith('this') ? 'Item' : name;
    (/\bstatic\b/.test(prefix) ? statics : instance).push(property);
  }

  return { name: head[1], base, instance, statics };
}

/** The members of a declaration's exported object type. */
function readDeclaration(text) {
  const source = code(text);
  const body = /export\s+type\s+\w+\s*=\s*\{([\s\S]*)\}\s*;?\s*$/.exec(source)?.[1] ?? '';
  return [...body.matchAll(/^\s*(\w+)\??\s*:/gm)].map((match) => match[1]);
}

/** Compares every bag under a folder of the C# tree with its declaration in the TypeScript tree. */
function compareBags(csRoot, tsRoot, folder) {
  const classFiles = new Map(walk(csRoot, (name) => name.endsWith('.cs')).map((file) => [basename(file, '.cs'), file]));
  const problems = [];
  let agreed = 0;

  /**
   * A class's properties as the generator reads them, through Type.GetProperties() with no flags:
   * the class's own, static ones included, and the public instance properties of every class it
   * extends. A base class's statics are left out, as reflection leaves them out without
   * FlattenHierarchy. An override is one property, so the names are a set.
   */
  const propertiesOf = (cls, where) => {
    const own = new Set([...cls.instance, ...cls.statics]);
    let base = cls.base;
    while (base) {
      const file = classFiles.get(base);
      const parent = file ? readClass(readFileSync(file, 'utf8')) : null;
      if (!parent) {
        problems.push(`${where}: its base ${base} is not a class under ${csRoot}`);
        return null;
      }
      parent.instance.forEach((property) => own.add(property));
      base = parent.base;
    }
    return [...own];
  };

  for (const file of walk(resolve(csRoot, folder), (name) => name.endsWith('.cs')).sort()) {
    const where = relative(csRoot, file).replace(/\\/g, '/');
    const cls = readClass(readFileSync(file, 'utf8'));
    if (!cls) {
      problems.push(`${where}: no class found`);
      continue;
    }

    const declarationFile = resolve(tsRoot, relative(csRoot, dirname(file)), `${toCamelCase(cls.name)}.d.ts`);
    if (!existsSync(declarationFile)) {
      problems.push(`${where}: no declaration at ${relative(repoRoot, declarationFile).replace(/\\/g, '/')}`);
      continue;
    }

    const properties = propertiesOf(cls, where);
    if (!properties) {
      continue;
    }

    const declared = new Set(readDeclaration(readFileSync(declarationFile, 'utf8')));
    const expected = new Set(properties.map(toCamelCase));
    const before = problems.length;
    for (const property of properties) {
      if (!declared.has(toCamelCase(property))) {
        problems.push(`${where}: ${property} is not declared as ${toCamelCase(property)}`);
      }
    }
    for (const member of declared) {
      if (!expected.has(member)) {
        problems.push(`${where}: ${member} is declared but is not a member`);
      }
    }
    if (problems.length === before) {
      agreed++;
    }
  }

  return { problems, agreed };
}

// #endregion

const args = process.argv.slice(2);
const flag = (name) => {
  const at = args.indexOf(name);
  return at >= 0 ? args[at + 1] : undefined;
};

const typeOnly = flag('--project') !== undefined;
const bagsOnly = flag('--cs') !== undefined;
let isFailed = false;

if (!bagsOnly) {
  const project = resolve(flag('--project') ?? chatProject);
  const chat = resolve(flag('--chat') ?? chatFolder);

  if (!typeOnly && !existsSync(frameworkTypes)) {
    console.error(`no framework types at ${frameworkTypes}: build them first (npm run build:types in Rock.JavaScript.Obsidian)`);
    process.exit(1);
  }

  const probe = resolve(flag('--probe') ?? probeFolder);
  const { failed, notes, read } = checkTypes(project, chat, probe, resolve(flag('--checker') ?? vueTsc));
  if (notes.length > 0) {
    console.log('type errors outside the chat, not failing:');
    for (const note of notes) console.log(`  ${note}`);
  }
  if (failed.length > 0) {
    console.error('the chat client fails its type check:');
    for (const error of failed) console.error(`  ${error}`);
    isFailed = true;
  }
  else {
    console.log(`no type errors in the chat client, ${read} chat files read, templates included`);
  }
}

if (!typeOnly) {
  const { problems, agreed } = compareBags(resolve(flag('--cs') ?? bagsCs), resolve(flag('--ts') ?? bagsTs), flag('--folder') ?? bagsFolder);
  if (problems.length > 0) {
    console.error('chat view model bags and their declarations disagree:');
    for (const problem of problems) console.error(`  ${problem}`);
    console.error('\nRun the view model generator (Rock.CodeGeneration) for the bag, or correct the declaration by hand to match it.');
    isFailed = true;
  }
  else {
    console.log(`${agreed} bags agree with their declarations`);
  }
}

process.exit(isFailed ? 1 : 0);
