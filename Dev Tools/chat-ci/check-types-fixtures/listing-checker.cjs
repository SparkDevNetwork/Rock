// Stands in for a type checker that lists its whole program and checks nothing, whatever the reason
// (an option that turns checking off, a setup that skips it, a stand-in that only lists): every file
// under the project's folder and the config's own folder is printed as tsc lists a program, and no
// error is reported. With REPORT=scripts it reports one error in the script probe beside its config,
// as a checker that checks scripts and skips templates would, and like tsc only on a run that
// checks types, never on one asked only to list its files.
const { readdirSync, readFileSync } = require('node:fs');
const { dirname, join, resolve } = require('node:path');

const config = resolve(process.argv[process.argv.indexOf('-p') + 1]);
const folders = [dirname(config)];
try {
  folders.push(dirname(resolve(dirname(config), JSON.parse(readFileSync(config, 'utf8')).extends)));
}
catch {
  // a config that extends nothing lists its own folder alone
}

const walk = (folder) => readdirSync(folder, { withFileTypes: true }).flatMap((entry) => {
  const full = join(folder, entry.name);
  return entry.name === 'node_modules' ? [] : entry.isDirectory() ? walk(full) : [full];
});

if (process.env.REPORT === 'scripts' && !process.argv.includes('--listFilesOnly')) {
  console.log(`${join(dirname(config), 'scriptProbe.ts')}(2,14): error TS2322: Type 'number | undefined' is not assignable to type 'number'.`);
}
for (const file of new Set(folders.flatMap(walk))) {
  console.log(file);
}
process.exitCode = process.env.REPORT === 'scripts' ? 2 : 0;
