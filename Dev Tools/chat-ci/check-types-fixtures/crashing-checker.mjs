// Stands in for the type checker the way a crash after it had reported would look: it lists every
// file of the project, names an error outside the chat, then dies, so node exits 1 and not with
// the status tsc gives a finished run that found errors.
import { readdirSync } from 'node:fs';
import { dirname, join, resolve } from 'node:path';

const project = resolve(process.argv[process.argv.indexOf('-p') + 1]);
const root = dirname(project);

const walk = (folder) => readdirSync(folder, { withFileTypes: true }).flatMap((entry) =>
  entry.name === 'node_modules' ? [] : entry.isDirectory() ? walk(join(folder, entry.name)) : [join(folder, entry.name)]);

for (const file of walk(root).filter((name) => /\.(ts|obs)$/.test(name))) {
  console.log(file.replace(/\\/g, '/'));
}
console.log(`${join(root, 'Other', 'upstreamError.ts')}(2,14): error TS2322: Type 'string' is not assignable to type 'number'.`);

throw new Error('the checker crashed');
