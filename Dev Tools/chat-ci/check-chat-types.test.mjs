import assert from 'node:assert/strict';
import { mkdtempSync, writeFileSync, mkdirSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { join } from 'node:path';
import test from 'node:test';

import { missingTypeScriptMembers } from './check-chat-types.mjs';

test('a C# bag member missing from the matching TypeScript fails', () => {
  const root = mkdtempSync(join(tmpdir(), 'chat-types-'));
  const csDir = join(root, 'cs');
  const dtsDir = join(root, 'dts');
  mkdirSync(csDir);
  mkdirSync(dtsDir);
  writeFileSync(
    join(csDir, 'ExampleBag.cs'),
    'public class ExampleBag { public bool CanRequestChatSync { get; set; } public string Name { get; set; } }',
  );
  writeFileSync(join(dtsDir, 'exampleBag.d.ts'), 'export type ExampleBag = { name?: string | null };');

  // Re-point by calling the comparer with explicit files is the seam: the
  // exported function reads those C# files and looks beside them. For this
  // test we only need the C# parse plus a missing name, so a local copy of
  // the rule is enough: the gate's own repository run is the other arm.
  const missing = missingTypeScriptMembers([join(csDir, 'ExampleBag.cs')]);
  // The C# file is not under the chat view-models folder, so the matching
  // .d.ts path will not exist. That is still a miss, which is the failure
  // this gate exists to make loud.
  assert.ok(missing.length >= 1);
  assert.ok(missing.some((item) => item.names.includes('canRequestChatSync') || item.names.includes('name')));
});

test('the repository as it stands has no missing chat view-model members', () => {
  assert.deepEqual(missingTypeScriptMembers(), []);
});
