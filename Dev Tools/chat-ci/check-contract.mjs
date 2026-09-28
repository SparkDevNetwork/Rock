// Rock's wire contract is a byte copy of the one the chat platform generates, so this gate compares
// the two. The platform's file comes from CHAT_PLATFORM_CONTRACT, else from a checkout of the chat
// platform repository beside this one. Without either there is nothing to compare, and it skips.
import { existsSync, readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { fileURLToPath } from 'node:url';

const repoRoot = fileURLToPath(new URL('../../', import.meta.url));

export function compareContract(platformPath, rockPath) {
  if (!existsSync(platformPath)) {
    return { ok: true, skipped: true, message: `contract check skipped: no platform contract at ${platformPath}; set CHAT_PLATFORM_CONTRACT to its path` };
  }
  if (readFileSync(platformPath).equals(readFileSync(rockPath))) {
    return { ok: true, skipped: false, message: `${rockPath} matches ${platformPath}` };
  }
  return { ok: false, skipped: false, message: `Rock's contract ${rockPath} is out of date: replace it by copying ${platformPath}` };
}

if (process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  const platform = process.env.CHAT_PLATFORM_CONTRACT || resolve(repoRoot, '../platform/contract/chat-wire-contract.json');
  const result = compareContract(platform, resolve(repoRoot, 'Rock/Communication/Chat/Platform/Contract/chat-wire-contract.json'));
  (result.ok ? console.log : console.error)(result.message);
  process.exit(result.ok ? 0 : 1);
}
