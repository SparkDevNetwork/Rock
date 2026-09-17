// Comments explain in place. This gate fails when a chat file refers to planning material that
// lives outside this repository: decision, guardrail or open-item numbers (D-12, G-03, BP-07,
// SY-17), work-item ids (P0-S1), the planning file names, or section signs. A reader of this
// repository has none of those to open, so the reason belongs in the comment itself.
//
// It scans the chat paths rather than every tracked file, because this is a branch of Rock and
// almost nothing in it is ours to hold to this rule. The gate skips itself and its test, which
// hold the patterns as data, and skips binaries and the lockfile.
//
//   node check-references.mjs             checks every chat file this branch tracks
//   node check-references.mjs --path <f>  checks one file, which is how the test proves it bites
import { execFileSync } from 'node:child_process';
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const repoRoot = fileURLToPath(new URL('../../', import.meta.url));

// Where the chat platform's own code lives. A path listed here that holds nothing yet costs
// nothing; a path missing from here is a file the gate never reads.
//
// Three folders are shared with the chat that already shipped in Rock, which this work neither
// touches nor references: Rock.Blocks/Communication/Chat, Rock.ViewModels/Blocks/Communication/Chat
// and Rock.JavaScript.Obsidian.Blocks/src/Communication/Chat all hold files that are not ours. They
// are deliberately absent below. When a file of ours lands in one of them it is named here by its
// own path, so the rule never fires on somebody else's prose.
export const CHAT_PATHS = [
  'Rock/Communication/Chat/Platform/',
  // Shared with upstream, and named here because this work wrote a chat comment into it.
  'Rock/Rock.csproj',
  'Rock/Plugin/HotFixes/323_AddChatPeopleGroup.cs',
  'Rock/SystemKey/SystemSetting.cs',
  // Named one by one because the three folders they sit in are shared with the chat that
  // already shipped, and those files are not ours to read.
  'Rock.Blocks/Communication/Chat/ChatConfiguration.cs',
  'Rock.ViewModels/Blocks/Communication/Chat/ChatConfiguration/',
  'Rock.JavaScript.Obsidian.Blocks/src/Communication/Chat/chatConfiguration.obs',
  'Rock.JavaScript.Obsidian.Blocks/src/Communication/Chat/ChatConfiguration/',
  'Rock.JavaScript.Obsidian/Framework/ViewModels/Blocks/Communication/Chat/ChatConfiguration/',
  'Rock/Jobs/ChatPlatform',
  // Enabling chat goes through the connected services provider Rock already had, so these are
  // upstream files this work wrote chat comments into, plus the chat card beside Rock IQ's.
  'Rock/Configuration/IInitializationSettings.cs',
  'Rock/Configuration/InitializationSettings.cs',
  'Rock/Configuration/WebFormsInitializationSettings.cs',
  'Rock/Configuration/ConnectedServices/ConnectedServicesProvider.cs',
  'Rock.Blocks/Administration/SparkConnectedServices.cs',
  'Rock.ViewModels/Blocks/Administration/SparkConnectedServices/ChatConfigurationBag.cs',
  'Rock.ViewModels/Blocks/Administration/SparkConnectedServices/InitializationBag.cs',
  'Rock.JavaScript.Obsidian/Framework/ViewModels/Blocks/Administration/SparkConnectedServices/chatConfigurationBag.d.ts',
  'Rock.JavaScript.Obsidian/Framework/ViewModels/Blocks/Administration/SparkConnectedServices/initializationBag.d.ts',
  'Rock.JavaScript.Obsidian.Blocks/src/Administration/sparkConnectedServices.obs',
  'Rock.JavaScript.Obsidian.Blocks/src/Administration/SparkConnectedServices/chat.partial.obs',
  'Rock.JavaScript.Obsidian.Blocks/src/Administration/SparkConnectedServices/chatViewModel.partial.ts',
  'Rock.JavaScript.Obsidian.Blocks/tests/Administration/sparkConnectedServices.spec.ts',
  'Rock.JavaScript.Obsidian.Blocks/tests/Communication/Chat/',
  'Rock.Tests/Communication/Chat/',
  'Rock.Tests.Integration/Communication/Chat/',
  'Dev Tools/chat-ci/',
  '.github/workflows/chat-ci.yml',
];

const SKIP = new Set([
  'Dev Tools/chat-ci/check-references.mjs',
  'Dev Tools/chat-ci/check-references.test.mjs',
  'Dev Tools/chat-ci/package-lock.json',
]);

const BINARY = /\.(png|jpe?g|gif|ico|woff2?|ttf|otf|pdf|zip|wasm|dll|exe)$/i;

export const PATTERNS = [
  { name: 'decision, guardrail or open-item number', re: /\b(?:D|BP|G|SY|OQ)-\d{1,3}\b/ },
  { name: 'work-item id', re: /\bP\d{1,2}-S\d{1,2}\b/ },
  {
    name: 'planning file name',
    re: /\b(?:STRUCTURE|PHASES|DECISIONS|OPEN|GUARDRAILS|EVIDENCE|PROVENANCE)\.md\b/,
  },
  { name: 'planning folder', re: /\bdocs\/(?:planes|slices|archive|context|product|evidence|schema)\b/ },
  { name: 'section sign', re: /§/ },
];

/**
 * Finds every reference in one file's text.
 *
 * @param {string} relPath the path to name in the report
 * @param {string} text the file's contents
 * @returns {string[]} one line per hit, with the line number and what matched
 */
export function scan(relPath, text) {
  const hits = [];

  text.split(/\r?\n/).forEach((line, index) => {
    for (const { name, re } of PATTERNS) {
      const match = re.exec(line);
      if (match) hits.push(`${relPath}:${index + 1}: ${name} "${match[0]}"`);
    }
  });

  return hits;
}

/**
 * The chat files this branch tracks.
 *
 * @returns {string[]} repository-relative paths
 */
export function chatFiles() {
  const out = execFileSync('git', ['ls-files', '-z', '--', ...CHAT_PATHS], {
    cwd: repoRoot,
    encoding: 'utf8',
  });

  return out
    .split('\0')
    .filter(Boolean)
    .filter((file) => !SKIP.has(file) && !BINARY.test(file));
}

const args = process.argv.slice(2);
const invokedDirectly = process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url);

if (invokedDirectly) {
  const pathFlag = args.indexOf('--path');
  const files = pathFlag >= 0 && args[pathFlag + 1] ? [args[pathFlag + 1]] : chatFiles();

  const hits = [];
  for (const file of files) {
    const absolute = resolve(repoRoot, file);
    let text;
    try {
      text = readFileSync(absolute, 'utf8');
    } catch {
      continue;
    }
    hits.push(...scan(file, text));
  }

  if (hits.length > 0) {
    console.error('references to documents outside this repository:');
    for (const hit of hits) console.error(`  ${hit}`);
    console.error('\nState the reason in the comment itself. A reader here cannot open any of these.');
    process.exit(1);
  }

  console.log(`no references to documents outside this repository: ${files.length} chat files checked`);
}
