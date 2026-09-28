// The contract gate compares bytes, so each case is two real files on disk: the same bytes, one
// byte apart, and a platform file that is not there.
import assert from 'node:assert/strict';
import { mkdtempSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import test from 'node:test';

import { compareContract } from './check-contract.mjs';

function files(platformText, rockText) {
  const dir = mkdtempSync(join(tmpdir(), 'chat-contract-'));
  const platform = join(dir, 'platform.json');
  const rock = join(dir, 'rock.json');
  if (platformText !== undefined) writeFileSync(platform, platformText);
  writeFileSync(rock, rockText);
  return { platform, rock };
}

test('identical copies pass', () => {
  const { platform, rock } = files('{"a":1}\n', '{"a":1}\n');

  assert.deepEqual(compareContract(platform, rock), { ok: true, skipped: false, message: `${rock} matches ${platform}` });
});

test("a copy that differs fails and says to copy the platform's file over Rock's", () => {
  // A line ending alone is a difference: the bytes are what both sides hash.
  const { platform, rock } = files('{"a":1}\n', '{"a":1}\r\n');
  const result = compareContract(platform, rock);

  assert.equal(result.ok, false);
  assert.equal(result.skipped, false);
  assert.match(result.message, /out of date/);
  assert.ok(result.message.includes(platform) && result.message.includes(rock), result.message);
  assert.doesNotMatch(result.message, /\n/);
});

test('a missing platform file skips rather than fails', () => {
  const { platform, rock } = files(undefined, '{"a":1}\n');
  const result = compareContract(platform, rock);

  assert.equal(result.ok, true);
  assert.equal(result.skipped, true);
  assert.match(result.message, /skipped/);
  assert.ok(result.message.includes(platform), result.message);
});
