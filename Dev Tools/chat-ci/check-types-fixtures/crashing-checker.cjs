// Stands in for the type checker the way a crash after it had reported would look. tsc prints every
// diagnostic and only then lists the program's files, so a checker that dies having named an error
// outside the chat has listed nothing, and node ends it 1, a status a finished run can end with too.
// It is CommonJS as vue-tsc is, so node opens the crash on stderr with the script's absolute path.
const { dirname, join, resolve } = require('node:path');

const project = resolve(process.argv[process.argv.indexOf('-p') + 1]);
const root = dirname(project);

console.log(`${join(root, 'Other', 'upstreamError.ts')}(2,14): error TS2322: Type 'string' is not assignable to type 'number'.`);

throw new Error('the checker crashed');
