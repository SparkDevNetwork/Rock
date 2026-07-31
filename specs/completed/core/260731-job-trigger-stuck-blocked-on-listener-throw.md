---
author: Jason Hendee
date_created: 2026-07-31
summary: >-
  A scheduled job stops firing permanently (trigger stuck in Blocked) if
  RockJobListener.JobWasExecuted throws after the job runs, because the
  unguarded listener exception escapes into Quartz before the trigger lock is
  cleared. The failure is completely silent: the job shows "Success" and
  nothing is logged. Fix by guarding the listener's bookkeeping steps so they
  log instead of throwing into the scheduler.
contributors: []
---

# Scheduled Job Trigger Stuck in Blocked When RockJobListener Throws

## Summary

Every Rock job is marked `DisallowConcurrentExecution` (Rock/Jobs/RockJob.cs:37), so Quartz blocks a job's trigger while it runs and only releases it when the run-completion pipeline finishes. `RockJobListener.JobWasExecuted` sits in that pipeline with no exception handling. If its post-run bookkeeping (the ServiceJobHistory write or the notification email preparation) throws, the exception escapes into Quartz, the pipeline aborts before the trigger lock is cleared, and the job never fires again until Rock restarts. The job's status still reads "Success" and the exception is swallowed without a trace, so the job dies silently. The fix guards each bookkeeping step in the listener so failures are logged to the Exception Log instead of propagating into the scheduler.

The fix itself lands in the v19.x hotfix branch; this spec lives in develop as the decision record.

## Problem Statement

A job whose listener bookkeeping fails once stops running on its schedule forever, while displaying "Success" for its last run. Nothing appears in the Exception Log or Rock Logs. "Run Now" also refuses with "already running" because the job is never removed from the scheduler's currently-executing list. Only an application restart recovers the job. Reported as GitHub issue #6934 against Rock 18.4; the affected code is unchanged in develop.

## Reproduction

The repro steps suggested in the GitHub issue (break the mail transport) and in the Asana task (delete the job notification SystemCommunication) do not work: `RockMessage.Send` catches its own exceptions and returns false (Rock/Communication/RockMessage.cs:228), and `RockEmailMessage( Guid )` silently no-ops when the SystemCommunication is not found (Rock/Communication/RockEmailMessage.cs:211). The reliable throw point is the history write.

Verified 2026-07-31 on the v19.4 hotfix branch:

1. Add a temporary `throw new Exception( "Repro Issue #6934" );` at the end of `ServiceJobHistoryService.AddCompletedServiceJobHistory`.
2. Set `RunJobsInIISContext` to `True` in RockWeb/web.config so the local instance runs the main scheduler.
3. Let jobs fire on their schedules.
4. Each job runs once: its ServiceJobHistory row is left stuck at Status "Running" with a NULL StopDateTime, the ServiceJob row shows "Success", and the job never fires again. "Run Now" refuses with "already running".

## Root Cause

Traced end to end:

1. `RockJobListener.JobWasExecuted` (Rock/Jobs/RockJobListener.cs:188) has no try/catch. The job's "Success" status is saved at line 307; after that, the history write (lines 312-319) or the notification email preparation (line 324) can throw.
2. `QuartzScheduler.NotifyJobListenersWasExecuted` wraps the listener exception in a `SchedulerException` and rethrows (Quartz/Core/QuartzScheduler.cs:1861).
3. `JobRunShell.NotifyJobListenersComplete` catches it, reports it to scheduler listeners, and returns false, which makes the run loop break (Quartz/Core/JobRunShell.cs:206) before `NotifyJobStoreJobComplete` (Quartz/Core/JobRunShell.cs:261) ever runs.
4. `NotifyJobStoreJobComplete` is what calls `IJobStore.TriggeredJobComplete`, the only place a `DisallowConcurrentExecution` trigger is moved out of the Blocked state. The trigger stays Blocked until the process restarts.

Two independent effects make the failure invisible and total:

- **Silent.** The exception's only destination is `NotifySchedulerListenersError`. Rock registers no `ISchedulerListener`, and the vendored Quartz's internal `ErrorLogger.SchedulerError` has an empty method body (Quartz/Core/QuartzScheduler.cs:2361). `ExceptionLogService` is never reached.
- **Run Now also breaks.** Quartz's currently-executing list is maintained by `ExecutingJobsManager`, an internal job listener notified after external listeners (Quartz/Core/QuartzScheduler.cs:1638 puts externals first, and the notify loop rethrows on first failure). When `RockJobListener` throws, the job is never removed from the list, so `ServiceJobService.RunNowAsync` refuses (Rock/Model/Core/ServiceJob/ServiceJobService.cs:118).

The same unguarded pattern exists in `JobToBeExecuted` (Rock/Jobs/RockJobListener.cs:92): a throw there (its `SaveChanges` at line 138, or the "Job Pulse" history insert) breaks the run loop at Quartz/Core/JobRunShell.cs:145, stranding the trigger the same way before the job even runs.

Known throw sources in `JobWasExecuted`: the two `SaveChanges` calls on lines 307 and 319 (deadlocks, timeouts, constraint violations; at scale these are triggered by the duplicate-scheduler collisions of issues #6932/#6933) and Lava resolution in the notification path. On its own the failure is rare; issues #6932/#6933 make it common.

## Affected Code Paths

Primary (where the fix lands):

- Rock/Jobs/RockJobListener.cs:188 (`JobWasExecuted`): guard the history write and the notification send.
- Rock/Jobs/RockJobListener.cs:92 (`JobToBeExecuted`): guard the status/history bookkeeping; the job should still run if bookkeeping fails.

Secondary (covered by the primary fix, verify only):

- Rock/Jobs/RunNowRockJobListener.cs inherits `RockJobListener` and calls the base methods, so the Run Now path is covered by the base-class guards.
- No Quartz files change.

## Workarounds

User-side: restart Rock (recycle the app pool). All triggers are rebuilt on startup, which unsticks every blocked job until the next occurrence.

## Proposed Fix

Guard the listener so its bookkeeping can never throw into the scheduler. In `JobWasExecuted`, run the three steps independently so an early failure does not skip the later steps:

1. Save the job's last-run status (existing behavior).
2. Write the ServiceJobHistory completion in its own try/catch, on its own `RockContext`, so a failed status save cannot poison the history write.
3. Prepare and send the notification email in its own try/catch.

Each catch logs via `ExceptionLogService.LogException` so today's silent failures become visible Exception Log entries. Apply the same guard to `JobToBeExecuted` so a bookkeeping failure there degrades to a logged error and a missing "started" history row instead of a stranded trigger.

No public API changes, no schema changes, no migration; ships in a hotfix. The fix is cause-agnostic: it protects the trigger no matter what threw.

Coordination note: the fix for issue #6935 (spec 260731-servicejobhistory-sub-second-orphaned-incomplete-rows.md) modifies the same two methods. The guards from this spec wrap around that fix's complete-by-Id logic; implement them together or rebase whichever lands second.

## Fix Risks

- **Masking real errors.** Failures that previously (silently) killed the job now log and continue. Mitigated by logging every caught exception to the Exception Log, which is strictly more visible than today's behavior.
- **History rows can remain incomplete.** If the history write fails, the run's row stays "Running"/"Incomplete". Acceptable: today the same failure kills the job entirely.
- **A notification email can be skipped** when the history write failure and email preparation failure are related. Acceptable for the same reason.

## Verification Steps

1. Repeat the verified repro (temporary throw in `AddCompletedServiceJobHistory`) with the fix in place: the job must keep firing every minute, each failure must appear in the Exception Log, and "Run Now" must keep working.
2. Repeat with the throw moved into `AddStartedServiceJobHistory` to exercise the `JobToBeExecuted` guard: the job must still execute and keep firing, with the failure logged.
3. Remove the temporary throw and confirm normal behavior: history rows complete with "Success"/"Exception", notifications send per the job's Notification Status, no new Exception Log entries.
4. Confirm a job that itself throws still records "Exception" status and an Exception Log entry (existing behavior, must not regress).
5. Run Now path: run a job via Run Now with and without the forced throw; the throwaway scheduler must shut down cleanly and subsequent Run Now clicks must work.

## Out of Scope

- The vendored Quartz behavior that lets a listener exception strand a trigger (see Considered but Rejected).
- The empty `ErrorLogger.SchedulerError` body in vendored Quartz. Filling it in would give scheduler-level errors a log destination, but it touches forked internals and is not needed once the listener stops throwing.
- The duplicate-scheduler collisions of issues #6932/#6933, which are the common cause of the history-write failures at scale. This fix stops those collisions from silently killing jobs; it does not stop the collisions.
- The orphaned "Incomplete" history rows of issue #6935 (separate spec, same methods; see Coordination note).

## Considered but Rejected

### Patch vendored Quartz so the trigger lock always clears

Change `JobRunShell.Run` (or `QuartzScheduler.NotifyJobListenersWasExecuted`) so a listener exception cannot skip `TriggeredJobComplete`. Closer to the true defect and would protect any future listener, but it edits forked Quartz internals, changes completion semantics for every job in Rock, and would likely be lost if Quartz is ever re-vendored. The listener guard fixes the problem in one file that Rock owns.

## Related

- [GitHub issue #6934](https://github.com/SparkDevNetwork/Rock/issues/6934) (source report; its mail-transport repro does not throw, see Reproduction)
- [Asana task DEV-14364](https://app.asana.com/1/20866866924293/project/1208321217019996/task/1216801719295796) (pipeline task; requirements live in this spec)
- [GitHub issue #6932](https://github.com/SparkDevNetwork/Rock/issues/6932) and [#6933](https://github.com/SparkDevNetwork/Rock/issues/6933) (duplicate schedulers colliding on the history write; the common trigger for this failure at scale)
- Spec 260731-servicejobhistory-sub-second-orphaned-incomplete-rows.md (issue #6935; modifies the same listener methods, coordinate the two fixes)
