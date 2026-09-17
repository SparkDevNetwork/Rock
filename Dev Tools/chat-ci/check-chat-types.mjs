// The chat pipeline has no compiler behind a hand-written view model. A bag
// member that exists in C# and not in the matching TypeScript reached a
// reviewer instead of a gate. This compares the two for the chat-owned boxes
// and bags, and nothing else: upstream Rock's type cleanliness is unmeasured.
import { readdirSync, readFileSync, existsSync } from 'node:fs';
import { dirname, join, relative } from 'node:path';
import { fileURLToPath } from 'node:url';

const here = dirname(fileURLToPath(import.meta.url));
const repoRoot = join(here, '../..');

const csharpRoot = join(repoRoot, 'Rock.ViewModels/Blocks/Communication/Chat');
const dtsRoot = join(repoRoot, 'Rock.JavaScript.Obsidian/Framework/ViewModels/Blocks/Communication/Chat');

const csharpProperty = /public\s+(?:[\w.?<>[\]]+\s+)+([A-Z]\w*)\s*\{\s*get;/g;

function walk(dir, suffix, found = []) {
  if (!existsSync(dir)) {
    return found;
  }

  for (const entry of readdirSync(dir, { withFileTypes: true })) {
    const path = join(dir, entry.name);
    if (entry.isDirectory()) {
      walk(path, suffix, found);
    } else if (entry.name.endsWith(suffix)) {
      found.push(path);
    }
  }

  return found;
}

function csharpProperties(source) {
  const names = [];
  csharpProperty.lastIndex = 0;
  let match;
  while ((match = csharpProperty.exec(source))) {
    names.push(match[1]);
  }
  return names;
}

function camel(name) {
  return name.length === 0 ? name : name[0].toLowerCase() + name.slice(1);
}

function dtsProperties(source) {
  const names = [];
  const re = /^\s*(?:readonly\s+)?([A-Za-z_]\w*)\??:/gm;
  let match;
  while ((match = re.exec(source))) {
    names.push(match[1]);
  }
  return names;
}

function dtsPathFor(csPath) {
  const rel = relative(csharpRoot, csPath).replace(/\\/g, '/');
  const parts = rel.split('/');
  const file = parts.pop().replace(/\.cs$/, '');
  const camelFile = file[0].toLowerCase() + file.slice(1);
  return join(dtsRoot, ...parts, `${camelFile}.d.ts`);
}

export function missingTypeScriptMembers(csFiles = walk(csharpRoot, '.cs')) {
  const missing = [];
  for (const csPath of csFiles) {
    const source = readFileSync(csPath, 'utf8');
    const expected = csharpProperties(source).map(camel);
    const dtsPath = dtsPathFor(csPath);
    if (!existsSync(dtsPath)) {
      missing.push({ csharp: csPath, typescript: dtsPath, names: expected });
      continue;
    }

    const actual = new Set(dtsProperties(readFileSync(dtsPath, 'utf8')));
    const absent = expected.filter((name) => !actual.has(name));
    if (absent.length > 0) {
      missing.push({ csharp: csPath, typescript: dtsPath, names: absent });
    }
  }

  return missing;
}

const isMain = process.argv[1] && fileURLToPath(import.meta.url) === process.argv[1];
if (isMain) {
  const missing = missingTypeScriptMembers();
  if (missing.length > 0) {
    for (const item of missing) {
      console.error(
        `${relative(repoRoot, item.csharp)}: ${item.names.join(', ')} missing from ${relative(repoRoot, item.typescript)}`,
      );
    }
    process.exit(1);
  }

  console.log('chat view models have matching TypeScript members');
}
