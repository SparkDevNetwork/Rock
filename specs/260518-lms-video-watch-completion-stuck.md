---
author: Jon Edmiston
date_created: 2026-05-18
summary: >-
  Students using LMS Video Watch activities with a Completion Threshold above
  0% are intermittently unable to click the Complete button even after watching
  enough of the video. Root cause is that the MediaPlayer's cumulative watch
  state never propagates to the LMS activity component on initial load, so the
  Complete button stays disabled until the user actively plays the video and a
  progress tick fires.
contributors: []
---

# LMS Video Watch Completion Stuck

## Summary

When a Video Watch learning activity is configured with a Completion Threshold above 0%, some students are unable to click the Complete button even after watching enough of the video to meet the threshold. The Media Element interaction data on the server correctly shows the user has watched the video (sometimes to 100%), but the LMS activity's `completionValues` in the browser remain empty, so `hasPassedThreshold` stays false and the button stays disabled. The bug is intermittent in the field but the underlying cause is a deterministic state-sync gap on initial component load.

## Problem Statement

Students enrolled in an LMS class with Video Watch activities (Completion Threshold > 0%) sometimes find that the Complete button is permanently disabled. Once a student is in this state, refreshing the page, switching computers, or trying a different browser does not resolve it. The MediaPlayer correctly tracks and persists their watch progress to the Media Element interaction record, but the LMS activity does not see it.

## Reproduction

Field reports are intermittent. Two reliable repro paths have been confirmed:

**Path 1 (chead4, intermittent in field):**

1. Configure a Video Watch activity with a Completion Threshold > 0% (e.g. 22%).
2. As a student, navigate to the activity and watch past the threshold.
3. Navigate away or refresh, then return to the activity.
4. Observe: Complete button is disabled, even though the student watched past the threshold.
5. Clicking Play briefly (any tick of the video) re-enables the Complete button.

**Path 2 (Colleen Head, parallel students):**

1. Configure a class with 7 Video Watch activities, thresholds varying from 5% to 100%.
2. Have two students complete activities in parallel from different browsers.
3. Observe: the second student's 6th video (configured at 100% threshold) sometimes shows Complete as disabled even after reaching 100% watch progress, without any refresh or navigation.

**Versions affected:** Rock 17.6, 18.2, and Alpha 19.0.9. Reported on Chrome, Safari, mobile Safari, Mac OS, Windows. Reported with both HTML5 (.mp4, .mov on S3) and Vimeo sources.

## Root Cause

The bug is a state-sync gap between two independent tracking systems on the page:

1. **Media Element Interaction tracking** (per-user, persisted continuously by `mediaplayer.ts`). On page load this restores the user's previous watch map (last 60 days) and `percentWatchedInternal` is computed correctly from that map.
2. **LMS activity completion tracking** (in-memory in `videoWatchLearningActivity.obs`, persisted only when the student clicks Complete via the `CompleteActivity` block action).

The Video Watch component initializes its local `watchedPercentage` ref from the LMS `completionValues.watchedPercentage` field, falling back to `0` when empty:

```ts
// Rock.JavaScript.Obsidian/Framework/Controls/Internal/LearningActivity/videoWatchLearningActivity.obs:192
watchedPercentage.value = toNumberOrNull(props.completionBag?.completionValues?.[CompletionKey.WatchedPercentage]) ?? 0;
```

For a student who has never *successfully* clicked Complete on this activity, `completionValues` is empty, so `watchedPercentage` starts at `0`. The Complete button is bound to `:disabled="!hasPassedThreshold"` where `hasPassedThreshold = watchedPercentage.value >= completionThreshold.value`, so it stays disabled.

The MediaPlayer child component DOES know the user has watched, but it only emits `update:watchedPercentage` from inside `markBitWatched`, which only runs when `trackPlay()` ticks during active playback:

```ts
// RockWeb/Scripts/Rock/UI/mediaplayer/mediaplayer.ts:642-647
this.percentWatchedInternal = watchedItemCount / this.watchBits.length;
this.previousPlayBit = playBit;
this.emit(EventType.Progress);
```

And the Obsidian wrapper only listens for that event, never for the initial post-load state:

```ts
// Rock.JavaScript.Obsidian/Framework/Controls/mediaPlayer.obs:306-308
player?.on("progress", () => {
    updateRefValue(watchedPercentage, player?.percentWatched ?? 0);
});
```

This explains the symptom precisely. A student who watched a video on a prior visit, never landed Complete (for any reason — transient failure, closed tab, distracted, second bug below), and returns to the page sees a MediaPlayer that "knows" they're at 100%, but the LMS Complete button stays disabled because the LMS-side ref is `0`. Field reporters confirmed they could not unstick themselves with refreshes or different machines; only re-playing the video (which fires a `progress` tick) re-enables the button.

The parallel-students repro is incidental: under load, sampling jitter increases and any number of transient causes can leave `completionValues` empty in the DB, after which any return visit hits this initial-load gap.

### Secondary issue: off-by-one for 100% threshold

A separate but related bug affects videos configured at exactly 100% Completion Threshold (Colleen's Video 6). `percentWatched` is computed as `watchedItemCount / watchBits.length` where `watchBits.length = Math.ceil(duration)`. To reach exactly `1.0`, every one-second bit must be marked. `markBitWatched` runs every 250 ms via `setInterval` during playback, and the final bit requires `Math.floor(currentTime) === length - 1` to be sampled at least once before the video ends. For a video of duration N + ε seconds, the window between currentTime=N and the `ended` event is small (the ε), and any buffering, seek, or pause near the end can drop the final bit. With a 100% threshold, missing even one bit means the button never enables. This is distinct from the primary cause but produces the same user-visible symptom and should be fixed in the same change.

## Affected Code Paths

Primary (where the fix lands):

- [mediaPlayer.obs:300-317](Rock.JavaScript.Obsidian/Framework/Controls/mediaPlayer.obs:300) — MediaPlayer's `onMounted` registers the `progress` handler but does not emit the player's initial `percentWatched` after the player has finished loading the existing watch map.
- [mediaplayer.ts:624-647](RockWeb/Scripts/Rock/UI/mediaplayer/mediaplayer.ts:624) — `markBitWatched` calculates `percentWatched` and never accounts for the final bit when playback ends mid-bit.

Secondary (consumer that benefits from the fix without needing changes):

- [videoWatchLearningActivity.obs:146,151,192](Rock.JavaScript.Obsidian/Framework/Controls/Internal/LearningActivity/videoWatchLearningActivity.obs:146) — local `watchedPercentage` ref, `hasPassedThreshold` computed, and `updateLocalValues` that reads from `completionValues`. Will automatically reflect the propagated initial state once the MediaPlayer emits it.

Related (no change needed, included for context):

- [PublicLearningClassWorkspace.cs:622-779](Rock.Blocks/Lms/Public/PublicLearningClassWorkspace.cs:622) — `CompleteActivity` block action that persists `completionValues` to the DB when the Complete button is clicked.
- [MediaPlayerOptionsExtensions.cs:42-169](Rock/Utility/ExtensionMethods/MediaPlayerOptionsExtensions.cs:42) — loads the user's prior watch map from `MediaElementInteraction` for the player to resume from.

## Workarounds

User-side workaround in use today: set the Completion Threshold to 0%. With threshold = 0, `hasPassedThreshold` is always true and the Complete button is always enabled. This is what Tikool's church did after 8 to 10 reports. It defeats the purpose of the threshold but unblocks students.

For an individual student already in the stuck state: clicking Play on the video for any duration (even a fraction of a second) fires a `progress` tick, which emits `update:watchedPercentage` with the cumulative percent, which then exceeds the threshold and enables the button. This is undiscoverable without telling the user, and many users never figure it out.

## Proposed Fix

Two changes in the MediaPlayer layer:

**Fix 1: Emit initial state after the player loads.**

In `mediaPlayer.obs`, after `prepareForPlay` has run inside the underlying player (i.e. once `watchBits` is initialized from the resumed map), emit `update:watchedPercentage` with `player.percentWatched`. The cleanest place is the existing `ready` event on the underlying player, OR a new `ready` event surfaced from `mediaplayer.ts` that fires after `prepareForPlay` completes. The Vue wrapper subscribes and emits the initial value upward.

Sketch:

```ts
// mediaPlayer.obs onMounted, after player is constructed:
player?.on("ready", () => {
    updateRefValue(watchedPercentage, player?.percentWatched ?? 0);
});
```

`player.on("ready", ...)` already exists in `mediaplayer.ts:912` and fires after `prepareForPlay` is called. The Obsidian wrapper just needs to listen for it and propagate the initial value. The downstream `videoWatchLearningActivity.obs` needs no change — its existing v-model binding will pick the value up automatically, and `hasPassedThreshold` will re-evaluate.

**Fix 2: Ensure the final watch bit is marked when playback ends.**

In `mediaplayer.ts`, in the `ended` event handler (currently at line 903), mark the final bit before emitting `Completed`. Either set `watchBits[watchBits.length - 1] = max(watchBits[watchBits.length - 1], 1)` directly, or call a new `markFinalBit()` helper that handles the boundary case. This guarantees that a user who watches the video to its natural end reaches `percentWatched === 1.0`.

Sketch:

```ts
this.player.on("ended", () => {
    // Ensure the final bit is marked so percentWatched can reach 1.0.
    if (this.watchBits.length > 0) {
        const lastIndex = this.watchBits.length - 1;
        if (this.watchBits[lastIndex] === 0) {
            this.watchBits[lastIndex] = 1;
            this.watchBitsDirty = true;
            const watchedItemCount = this.watchBits.filter(item => item > 0).length;
            this.percentWatchedInternal = watchedItemCount / this.watchBits.length;
            this.emit(EventType.Progress);
        }
    }
    this.emit(EventType.Completed);
});
```

Both fixes are localized to the MediaPlayer layer and do not require changes to the LMS block, the C# `CompleteActivity` action, or the database schema.

## Fix Risks

- **Fix 1** introduces an additional `update:watchedPercentage` emit on every MediaPlayer mount. Consumers that drive `:disabled` off this value will re-evaluate once on load. Audited consumers: `videoWatchLearningActivity.obs` (the LMS use case — this is the desired behavior) and `contentArticleItemVideo.obs` (does not bind to `watchedPercentage`, no impact). No known third-party consumers in the Obsidian framework. Low blast radius.
- **Fix 1** fires the emit even when the player loads with `percentWatched === 0` (a brand-new viewer). That's a no-op for the LMS consumer since the local ref is already `0`, but worth confirming during testing.
- **Fix 2** silently marks one watch bit that the user did not technically watch (the final second). For analytics that depend on exact bit-level accuracy of `MediaElementInteraction.InteractionData.WatchMap`, this is a small distortion. The trade-off is preferable to the current behavior of the user-facing completion percentage never reaching 100%. If analytics accuracy is a concern, an alternative is to round `percentWatched` up to 1.0 only when the `ended` event has fired, without mutating `watchBits`. State the chosen approach in the implementation PR.
- Neither fix changes server-side behavior, persistence format, or block-action contracts. Backward compatible for plugins and downstream consumers.

## Verification Steps

1. **Returning to a previously-watched video shows Complete enabled.** Configure a Video Watch activity at 50% threshold. As a student, watch past 50%, navigate away without clicking Complete. Return to the activity. Verify that the Complete button is enabled within ~1 second of the MediaPlayer loading, without clicking Play.
2. **Fresh viewer can still complete normally.** As a different student, open the same activity for the first time. Watch past the threshold. Verify the Complete button enables progressively during playback and successfully persists on click.
3. **100% threshold reaches 1.0 on natural end.** Configure a Video Watch activity at 100% threshold. As a student, play the video uninterrupted to its end (do not seek, pause, or buffer). Verify that `percentWatched === 1.0` at the `ended` event and the Complete button is enabled.
4. **100% threshold after seeking still reaches 1.0.** As a student, play to the middle, seek to the end, let it end naturally. Verify the bit-coverage logic still gates correctly (intermediate bits not falsely marked) but the final bit is marked.
5. **Refresh mid-playback does not reset progress.** Watch past the threshold, refresh the page, return. Confirm the Complete button is enabled on load (Fix 1) and `completionValues.watchedPercentage` in dev tools reflects the actual watch percent within ~1 second of MediaPlayer load.
6. **Parallel students.** Reproduce Colleen's two-student parallel test on the 7-video class. Confirm both students can complete all 7 activities, including Video 6 at 100% threshold.
7. **Field regression.** No new client-side console errors, no visible flicker in the Complete button state (it should enable cleanly, not toggle).

## Out of Scope

- The `notificationAlertType` computed bug in [videoWatchLearningActivity.obs:152](Rock.JavaScript.Obsidian/Framework/Controls/Internal/LearningActivity/videoWatchLearningActivity.obs:152) where `hasPassedThreshold ? "success" : "warning"` is missing `.value` and always returns `"success"`. This is cosmetic, only affects the Summary screen, and is unrelated to the Complete-button bug. Address separately.
- The prop watcher in [videoWatchLearningActivity.obs:261-263](Rock.JavaScript.Obsidian/Framework/Controls/Internal/LearningActivity/videoWatchLearningActivity.obs:261) that re-runs `updateLocalValues` on every parent prop change. It is benign in normal flow and the proposed fix does not require touching it. Revisit only if a follow-up issue emerges.
- Server-side persistence model changes. The current design of persisting completion values only on Complete-button click is preserved.
- Plyr / HLS.js version upgrades. The fix is in Rock's wrapper code, not the third-party libraries.

## Considered but Rejected

### Persist `watchedPercentage` progressively during playback

Rejected. Would require a new block action and continuous server writes from every Video Watch activity. The current design (persist only on Complete) is fine; the bug is purely client-side state sync. A progressive-persist approach also wouldn't fix the 100% threshold off-by-one.

### Have `videoWatchLearningActivity.obs` initialize its local `watchedPercentage` from `MediaElementInteraction` directly

Rejected. The activity component would need to call a media-element API on mount and duplicate logic that already lives inside the MediaPlayer. The MediaPlayer is the right source of truth for "how much of this media has this user watched"; the activity component should consume it, not re-derive it.

### Change `hasPassedThreshold` to use `>` instead of `>=`, or add a tolerance

Rejected. Would mask the off-by-one without fixing it, and would change behavior for activities at thresholds other than 100% (a student exactly at threshold should pass, not fail). Mark the final bit cleanly instead.

### Roll the threshold default to 0 in core

Rejected. That is the current workaround and it defeats the feature. Fix the bug instead of removing the configurability.

## Related

- GitHub issue: [SparkDevNetwork/Rock#6828](https://github.com/SparkDevNetwork/Rock/issues/6828)
- Asana task: ISSUE #6828 (DEV-12843)
- Rocket.Chat thread: https://chat.rockrms.com/channel/lms?msg=8Qnb7DQHmewTBj2Kr
- Affected files:
  - [mediaPlayer.obs](Rock.JavaScript.Obsidian/Framework/Controls/mediaPlayer.obs)
  - [mediaplayer.ts](RockWeb/Scripts/Rock/UI/mediaplayer/mediaplayer.ts)
  - [videoWatchLearningActivity.obs](Rock.JavaScript.Obsidian/Framework/Controls/Internal/LearningActivity/videoWatchLearningActivity.obs)
