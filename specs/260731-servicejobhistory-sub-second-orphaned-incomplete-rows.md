---
author: Jason Hendee
date_created: 2026-07-31
summary: >-
  Sub-second job runs leave orphaned "Incomplete" ServiceJobHistory rows and
  mis-attributed run durations because the completion callback re-finds the
  "started" row by timestamp guessing. Fix by passing the row Id through
  Quartz's execution context and completing the row by primary key.
contributors: []
---

# ServiceJobHistory: Sub-Second Jobs Produce Orphaned "Incomplete" Rows

## Summary

Rock writes one ServiceJobHistory row per job run: it inserts the row when the job starts and updates that same row when the job finishes. Nothing links the two callbacks, so the completion side re-finds the row by comparing timestamps. For jobs that finish in under ~1.7 ms the stored start time (SQL `datetime`, 3.33 ms resolution) can round past the in-memory finish time, the lookup misses, and Rock inserts a duplicate row while abandoning the original. The fix passes the row's primary key through Quartz's execution context so the completion callback updates the exact row, with the timestamp logic retained only as a fallback.

The fix itself lands in the v19.x hotfix branch; this spec lives in develop as the decision record.

## Problem Statement

Jobs that complete in under a few milliseconds produce two ServiceJobHistory rows per run: one later stamped "Incomplete" (never actually incomplete) and one with a rewritten start time whose duration does not reflect the real run. History grids show phantom failures and inflated or mis-attributed durations. Confirmed by Steve on ordinary sub-second jobs (GitHub issue #6935).

## Reproduction

Natural-timing reproduction is generally not feasible on a dev environment. The listener overhead between the captured start and the completion timestamp (insert `SaveChanges`, attribute load, Quartz dispatch) has a floor of roughly 16-20 ms on a typical dev box, and the miss only fires when that entire gap is smaller than the SQL `datetime` round-up of at most ~1.7 ms. Environments that hit this in the wild have a much faster path between the two callbacks.

Confirmed deterministically on 2026-08-03 by exaggerating the round-up: temporarily shift the stored start forward 25 ms (`AddStartedServiceJobHistory( job, now.AddMilliseconds( 25 ) )` in `JobToBeExecuted`), then run a trivial job (Run SQL, `SELECT 1`) every 10 seconds. Any run whose start-to-completion gap is under the skew misses the pairing, which produced all three symptoms within minutes:

1. Orphaned rows with `StopDateTime` null, stamped "Incomplete" by the next run.
2. Mis-attributed closures: a later run's completion closing an earlier run's orphan through the timestamp fallback, recording interval-length durations (10, 20, 30 seconds) for a ~40 ms job.
3. Fabricated 0 ms rows (start equals stop) when no candidate matched at all.

An alternative without touching the listener: call `AddStartedServiceJobHistory` directly with a start time whose sub-millisecond component rounds up as SQL `datetime` (e.g. `12:00:00.0016` stores as `12:00:00.003`), then complete with a `LastRunDateTime` 1 ms later.

Affects all versions since the "started row" behavior was introduced in May 2023 (see the JMH engineering note at Rock/Jobs/RockJobListener.cs:122).

## Root Cause

- `JobToBeExecuted` inserts a partial "started" row via `AddStartedServiceJobHistory` (Rock/Jobs/RockJobListener.cs:141). `StartDateTime` is persisted as SQL `datetime`, which rounds to 3.33 ms steps and can round up.
- `JobWasExecuted` re-finds that row with `GetIncompleteServiceJobHistoryForLastRun` (Rock/Model/Core/ServiceJobHistory/ServiceJobHistoryService.cs:231), which requires `StartDateTime <= job.LastRunDateTime`. `LastRunDateTime` is held in memory at full precision.
- For a run shorter than ~1.7 ms, the stored (rounded-up) start can exceed the in-memory finish. No row matches, so `AddCompletedServiceJobHistory` (ServiceJobHistoryService.cs:117) inserts a second row, rewriting its start as `LastRunDateTime - LastRunDurationSeconds` (whole seconds, ServiceJobHistoryService.cs:221).
- A November 2024 patch then stamps the abandoned original row "Incomplete" (Rock/Jobs/RockJobListener.cs:340), using `GetServiceJobHistoryForLastRun` (ServiceJobHistoryService.cs:185), itself another timestamp guess that can mis-attribute rows.
- The "Incomplete" stamp is not terminal. It changes only the status and leaves `StopDateTime` null, so a stamped orphan remains a candidate for a later run's fallback matching. This is the primary source of the inflated durations observed in reproduction: a later run's completion closes a stale orphan and records the interval between the two runs as the run time.

Historically, no one chose timestamps over an Id: before May 2023 Rock wrote a single row at completion, so there was nothing to pair. The pairing requirement appeared inside a change scoped as a UI improvement (show a job as running), was solved with timestamps, and the November 2024 patch added a second guess to clean up the rows the first guess abandoned.

## Affected Code Paths

Primary (where the fix lands):

- Rock/Jobs/RockJobListener.cs:92 (`JobToBeExecuted`): put the new history row's Id into the Quartz execution context after `SaveChanges`.
- Rock/Jobs/RockJobListener.cs:198 (`JobWasExecuted`): read the Id back, complete that row by primary key, and remove the November 2024 "Incomplete" stamping (lines 337-343).
- Rock/Model/Core/ServiceJobHistory/ServiceJobHistoryService.cs: new complete-by-Id path; existing `GetIncompleteServiceJobHistoryForLastRun` becomes the fallback.

Secondary (verify, likely unchanged):

- Rock/Jobs/RunNowRockJobListener.cs inherits `RockJobListener` and calls `base.JobWasExecuted`, so it is covered by the base-class fix. Quartz passes the same execution context to every listener for a given run.
- Rock/Model/Core/ServiceJob/ServiceJobService.cs:185 (`RunNowAsync` catch block) calls `AddErrorServiceJobHistory`, which uses the same timestamp lookup. This path has no Quartz context (the job may never have started), so it keeps the fallback behavior.

## Proposed Fix

Option 1 from the PO review: pass the row Id through Quartz's execution context. Quartz creates one context object per run and hands it to both callbacks; its `Put`/`Get` scratch dictionary exists for exactly this.

```csharp
// JobToBeExecuted, after SaveChanges assigns the Id
context.Put( ServiceJobHistoryIdKey, jobHistory.Id );

// JobWasExecuted
var jobHistoryId = context.Get( ServiceJobHistoryIdKey ) as int?;
```

Then complete that row by primary key and delete the November 2024 "Incomplete" stamping rather than adding a third guess on top.

Resilience requirements from the PO review:

- If the Id is missing from the context, or the row is not found by Id, log it.
- Wait briefly and retry the lookup once before treating it as a failure.
- Only then fall back to the existing timestamp logic, so a transient problem does not quietly become a bad history row and a real one shows up in the log.

`AddStartedServiceJobHistory` already returns the created entity (ServiceJobHistoryService.cs:93), so the Id is available immediately after `SaveChanges`.

As implemented on the v19.x hotfix branch:

- The brief wait is a 250 ms `Thread.Sleep` between the two lookup attempts, with a distinct log warning before the retry and another if the retry also misses.
- The missing-Id warning is suppressed for the pulse job. `JobWasExecuted` fires for it every 30 seconds, but `JobToBeExecuted` never creates a started row for it, so a literal warning would be continuous noise; the pulse job silently uses the fallback as it always has.
- The completion field assignments (stop, status, message, worker) are extracted from the tail of `AddCompletedServiceJobHistory` into a shared `CompleteServiceJobHistory( jobHistory, job )` so the primary-key path and the timestamp fallback cannot drift apart.

## Fix Risks

- Relies on Quartz handing the same context instance to `JobToBeExecuted` and `JobWasExecuted`. True and documented in the vendored Quartz today; a future re-vendor could change it. The fallback path covers that case.
- The context value is untyped: a magic-string key and a cast. Mitigated with a `private const string` key and a null-tolerant cast.
- Leaves no trace in the database. If the mechanism misbehaves there is nothing in the data to debug with, only the log entries required above.
- Rows created by older code (mid-upgrade, or by `AddErrorServiceJobHistory`) still resolve through the fallback timestamp logic, so behavior for existing data is unchanged.
- With the stamping removed, a row orphaned by a double miss (Id lookup fails twice AND the timestamp fallback creates a new row instead of matching it) stays "Running" until history cleanup purges it. Both misses are logged, so the scenario is diagnosable; nothing marks it "Incomplete" anymore.

## Verification Steps

1. Re-run the reproduction harness (Run SQL job, `SELECT 1`, every 10 seconds) with the temporary +25 ms start skew applied on top of the fix; confirm exactly one "Success" row per run with millisecond-scale durations, no "Incomplete" rows, and no interval-length durations. Remove the skew afterward.
2. Run a normal multi-second job; confirm the single row's start/stop times bracket the actual run.
3. Throw from a job; confirm the single row is updated to "Exception" with the message.
4. Run Now path: run a job via Run Now and confirm the same single-row behavior (RunNowRockJobListener inherits the fix).
5. Fallback path: temporarily simulate a missing context Id (or delete the started row mid-run in a debugger); confirm the miss is logged, the retry happens, and the old logic produces a completed row.
6. Confirm the "Run Now fails to load" error path (`ServiceJobService.RunNowAsync` catch) still writes a history row.

## Out of Scope

- Whole-second duration rounding (`Convert.ToInt32( context.JobRunTime.TotalSeconds )` at Rock/Jobs/RockJobListener.cs:242) remains; durations are still reported in whole seconds.
- Schema changes of any kind (see Considered but Rejected).
- Cleaning up historical orphaned "Incomplete" rows already in customer databases.

## Considered but Rejected

### Option 2: Persist Quartz's FireInstanceId in a new column

Quartz assigns every run a unique `FireInstanceId` before it starts. Persisting it (`ALTER TABLE [dbo].[ServiceJobHistory] ADD [FireInstanceId] NVARCHAR(50) NULL`) and matching on it would also replace the guess with an exact match, with the added benefits of being visible in the data for debugging and not depending on Quartz's context lifetime.

Rejected because it requires a schema migration, which takes a migration token and would have to ship in a major version. Option 1 fixes the problem the same way without touching the database, so it can ship in a hotfix. PO decided on Option 1 on 2026-07-29.

## Related

- [GitHub issue #6935](https://github.com/SparkDevNetwork/Rock/issues/6935) (source report, confirmed by Steve)
- [Asana task DEV-14366](https://app.asana.com/1/20866866924293/project/1208321217019996/task/1216801879097299) (pipeline task; requirements live in this spec)
- [Asana PO Review subtask](https://app.asana.com/1/20866866924293/task/1216921191640319) (options analysis and decision; Option 1 chosen 2026-07-29)
- May 2023 engineering note documenting the started-row design: Rock/Jobs/RockJobListener.cs:122
