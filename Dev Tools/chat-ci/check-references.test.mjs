// A gate is worth having only if it can be shown to refuse something, so each pattern it carries
// is planted here on its own line and the line number is asserted with it.
import assert from 'node:assert/strict';
import { execFileSync } from 'node:child_process';
import test from 'node:test';
import { fileURLToPath } from 'node:url';

import { scan } from './check-references.mjs';

const here = fileURLToPath(new URL('.', import.meta.url));

test('it refuses each kind of reference to material that is not in this repository', () => {
  const planted = [
    '// this comment cites D-31, which a reader here cannot open',
    '// this one names the work item P0-S5',
    '// this one points at STRUCTURE.md',
    '// and this one carries a section sign, §4.6',
  ].join('\n');

  const hits = scan('Rock/Communication/Chat/Platform/Example.cs', planted);

  assert.equal(hits.length, 4, `expected one hit per planted line, got ${hits.length}: ${hits.join(' | ')}`);
  assert.match(hits[0], /:1:/);
  assert.match(hits[0], /D-31/);
  assert.match(hits[1], /:2:/);
  assert.match(hits[1], /P0-S5/);
  assert.match(hits[2], /:3:/);
  assert.match(hits[2], /STRUCTURE\.md/);
  assert.match(hits[3], /:4:/);
  assert.match(hits[3], /§/);
});

test('it refuses the lane names the plan gave the two syncs, spaced or joined', () => {
  const planted = [
    '// the Lane A push runs after the save commits',
    'var changes = new ChatPlatformSyncHelper.LaneAChanges();',
    '// and lane-b restates the whole church',
  ].join('\n');

  const hits = scan('Rock/Communication/Chat/Platform/Example.cs', planted);

  assert.equal(hits.length, 3, `expected one hit per planted line, got ${hits.length}: ${hits.join(' | ')}`);
  assert.match(hits[0], /:1:.*Lane A/);
  assert.match(hits[1], /:2:.*LaneA/);
  assert.match(hits[2], /:3:.*lane-b/);
});

test('it passes prose that explains itself in place', () => {
  const clean = [
    '// The column order here is a wire: the payload is positional, so two columns of the same',
    '// width swapped shift every value one place and no row-width check can see it.',
    '// The immediate sync pushes a save at once; the full sync restates the church lane by lane.',
  ].join('\n');

  assert.deepEqual(scan('Rock/Communication/Chat/Platform/Example.cs', clean), []);
});

test('the chat files in this branch are clean today', () => {
  // Spawned rather than called, because what has to hold is that the gate CI runs exits zero.
  const result = execFileSync(process.execPath, ['check-references.mjs'], { cwd: here, encoding: 'utf8' });

  assert.match(result, /no references/i);
});
