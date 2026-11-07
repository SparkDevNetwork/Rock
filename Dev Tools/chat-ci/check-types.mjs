// Holds the chat client to its types, in two halves.
//
// The type check runs the blocks package's own vue-tsc over Rock's Communication project, which
// holds the chat and Spark's other Communication blocks, and checks templates as well as scripts.
// An error in a chat file fails; an error anywhere else is printed as a note and does not fail,
// because the rest of the project is upstream Rock's and a merge from it must never turn the chat
// red. It reads the framework through its built declarations, so the framework's types are built
// first (npm run build:types in Rock.JavaScript.Obsidian).
//
// The bag comparison holds each chat view model bag in C# to its TypeScript declaration by member
// name, both ways. The declarations are written by a code generator a person runs by hand, so a
// member added in C# and never generated is invisible to the type check: nothing typed reads it.
// The generator writes every public instance property, inherited ones included, under its name in
// Rock's camelCase, which lowers a leading acronym whole (RSVPReminderOffsetDays is
// rsvpReminderOffsetDays); the comparison applies the same two rules.
//
//   node check-types.mjs                                     both halves over the chat
//   node check-types.mjs --project <tsconfig> --chat <dir>   the type check of another project
//   node check-types.mjs --cs <dir> --ts <dir> --folder <f>  the bag comparison of another tree
import { spawnSync } from 'node:child_process';
import { existsSync, readdirSync, readFileSync } from 'node:fs';
import { basename, dirname, isAbsolute, join, relative, resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const repoRoot = fileURLToPath(new URL('../../', import.meta.url));
const blocksPackage = resolve(repoRoot, 'Rock.JavaScript.Obsidian.Blocks');
const vueTsc = resolve(blocksPackage, 'node_modules/vue-tsc/bin/vue-tsc.js');
const frameworkTypes = resolve(repoRoot, 'Rock.JavaScript.Obsidian/dist/Framework');

const chatProject = resolve(blocksPackage, 'src/Communication/tsconfig.json');
const chatFolder = resolve(blocksPackage, 'src/Communication/Chat');

const bagsCs = resolve(repoRoot, 'Rock.ViewModels');
const bagsTs = resolve(repoRoot, 'Rock.JavaScript.Obsidian/Framework/ViewModels');
const bagsFolder = 'Blocks/Communication/Chat';

/** One diagnostic as tsc prints it: `file(line,col): error TSnnnn: message`. */
const DIAGNOSTIC = /^(.+?)\((\d+),(\d+)\): error (TS\d+): (.*)$/;

// #region Type check

/**
 * Runs the type check over a project and sorts its errors into the chat's and everyone else's.
 * Paths are printed relative to the blocks package, where the checker runs.
 */
function checkTypes(project, chat) {
  if (!existsSync(vueTsc)) {
    return { failed: [`no type checker at ${vueTsc}: install the blocks package (npm ci in Rock.JavaScript.Obsidian.Blocks)`], notes: [] };
  }

  const run = spawnSync(process.execPath, [vueTsc, '--noEmit', '-p', project], { cwd: blocksPackage, encoding: 'utf8' });
  const lines = `${run.stdout ?? ''}${run.stderr ?? ''}`.split(/\r?\n/);
  const errors = [];
  for (const line of lines) {
    const match = DIAGNOSTIC.exec(line);
    if (match) {
      const [, file, row, col, code, message] = match;
      errors.push({ file, text: `${file}(${row},${col}): ${code} ${message}` });
    }
    else if (errors.length > 0 && /^\s/.test(line) && line.trim()) {
      // tsc explains an error on indented lines under it
      errors[errors.length - 1].text += `\n      ${line.trim()}`;
    }
  }

  const inChat = (file) => {
    const fromChat = relative(chat, resolve(blocksPackage, file));
    return fromChat !== '' && !fromChat.startsWith('..') && !isAbsolute(fromChat);
  };
  const failed = errors.filter((error) => inChat(error.file)).map((error) => error.text);
  const notes = errors.filter((error) => !inChat(error.file)).map((error) => error.text);

  // A checker that fails without naming an error (a missing config, a crash) has checked nothing.
  if (run.status !== 0 && errors.length === 0) {
    failed.push(`the type checker exited ${run.status} without naming an error:\n${lines.join('\n').trim()}`);
  }

  return { failed, notes };
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
 * A C# class's name, the class it extends and its public instance properties. A base list that
 * names only interfaces gives no base: an interface's members are declared again on the class.
 */
function readClass(text) {
  const source = code(text);
  const head = /\bclass\s+(\w+)(?:<[^>]*>)?\s*(?::\s*([^{]+))?\{/.exec(source);
  if (!head) {
    return null;
  }

  const first = head[2]?.split(',')[0].trim().replace(/<.*$/, '').split('.').pop();
  const base = first && !/^I[A-Z]/.test(first) ? first : null;

  const properties = [];
  for (const match of source.matchAll(/^\s*public\s+[\w.<>,?\[\]\s]+?\s+(\w+)\s*\{\s*(?:get|set|init)\b/gm)) {
    // the generator reads instance properties only
    if (!/\bstatic\b/.test(match[0])) {
      properties.push(match[1]);
    }
  }

  return { name: head[1], base, properties };
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

  /** A class's properties with every base class's, as the generator reads them. */
  const propertiesOf = (cls, where) => {
    const own = [...cls.properties];
    let base = cls.base;
    while (base) {
      const file = classFiles.get(base);
      const parent = file ? readClass(readFileSync(file, 'utf8')) : null;
      if (!parent) {
        problems.push(`${where}: its base ${base} is not a class under ${csRoot}`);
        return null;
      }
      own.push(...parent.properties);
      base = parent.base;
    }
    return own;
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

  const { failed, notes } = checkTypes(project, chat);
  if (notes.length > 0) {
    console.log('type errors outside the chat, not failing:');
    for (const note of notes) console.log(`  ${note}`);
  }
  if (failed.length > 0) {
    console.error('type errors in the chat client:');
    for (const error of failed) console.error(`  ${error}`);
    isFailed = true;
  }
  else {
    console.log('no type errors in the chat client');
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
