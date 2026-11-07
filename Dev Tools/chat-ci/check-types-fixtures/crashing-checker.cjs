// Stands in for the type checker the way a crash after it had reported would look: it names an
// error outside the chat, reports neither probe error and lists nothing, and node ends it 1.
// It is CommonJS as vue-tsc is, so node opens the crash on stderr with the script's absolute path.
const { readFileSync } = require('node:fs');
const { dirname, join, resolve } = require('node:path');

// The gate checks a config of its own that extends the project's, so the project is behind it.
const config = resolve(process.argv[process.argv.indexOf('-p') + 1]);
const root = dirname(resolve(dirname(config), JSON.parse(readFileSync(config, 'utf8')).extends));

console.log(`${join(root, 'Other', 'upstreamError.ts')}(2,14): error TS2322: Type 'string' is not assignable to type 'number'.`);

throw new Error('the checker crashed');
