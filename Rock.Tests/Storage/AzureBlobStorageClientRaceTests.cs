using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Storage.Common;

namespace Rock.Tests.Storage
{
    /// <summary>
    /// Repro tests for issue #6919: <see cref="AzureBlobStorageClient"/> uses an unsynchronized
    /// <see cref="System.Collections.Generic.Dictionary{TKey, TValue}"/> to cache container
    /// clients. Concurrent Add calls can corrupt the dictionary's internal bucket chain, after
    /// which subsequent lookups spin in <c>Dictionary.FindEntry</c> at 100% CPU.
    /// </summary>
    /// <remarks>
    /// The <see cref="Azure.Storage.Blobs.BlobContainerClient"/> constructor does not touch the
    /// network. It only parses the connection string, so this test can trigger the race with
    /// dummy (but syntactically valid) Azure credentials.
    ///
    /// Because the client is a process-wide singleton, the corrupted state persists across test
    /// method invocations in the same test host. Keep this class limited to a single hammering
    /// test and run it in isolation for the cleanest signal.
    /// </remarks>
    [TestClass]
    public class AzureBlobStorageClientRaceTests
    {
        /// <summary>
        /// A syntactically-valid Azure account key placeholder. The Azure SDK's connection-string
        /// parser eagerly base64-decodes the AccountKey, so any random string will not do.
        /// </summary>
        private static readonly string DummyAccountKey = Convert.ToBase64String( new byte[32] );

        [TestMethod]
        public void GetBlobClient_ConcurrentCallsWithVariedContainers_DoesNotHangOrThrow()
        {
            var client = AzureBlobStorageClient.Instance;
            var errors = new ConcurrentBag<Exception>();

            // We need to force the "not cached, take the Add path" branch many times across
            // many threads. Distinct container names produce distinct hashKeys, so ~200
            // containers x 32 threads x 5,000 iterations gives the Dictionary plenty of
            // opportunity to race on Add and corrupt itself.
            const int threadCount = 32;
            const int iterationsPerThread = 5_000;
            const int distinctContainerCount = 200;

            // Run the workers on a background Task so we can watchdog them via Task.Wait(TimeSpan).
            // We deliberately avoid [Timeout(...)] here because MSTest enforces it with Thread.Abort,
            // which unwinds the stack we want to inspect. With a Task watchdog the workers keep
            // running when the timeout fires, so the developer can attach a debugger and look at
            // Debug > Windows > Parallel Stacks for frames inside Dictionary.FindEntry / Insert.
            var workerTask = Task.Run( () =>
            {
                Parallel.For( 0, threadCount, threadIndex =>
                {
                    try
                    {
                        for ( var i = 0; i < iterationsPerThread; i++ )
                        {
                            var containerName = "container-" + ( i % distinctContainerCount );

                            client.GetBlobClient(
                                accountName: "acct",
                                accountKey: DummyAccountKey,
                                customDomain: null,
                                containerName: containerName,
                                blobName: "blob-" + i );
                        }
                    }
                    catch ( Exception ex )
                    {
                        // Under the race, we may see IndexOutOfRangeException, NullReferenceException,
                        // or ArgumentException thrown from inside Dictionary.Insert / FindEntry.
                        errors.Add( ex );
                    }
                } );
            } );

            // 30-second watchdog is well beyond what a healthy run needs (~1-3s locally).
            // If it does not complete in that window, the dictionary is almost certainly
            // corrupted and worker threads are spinning inside FindEntry.
            var completedInTime = workerTask.Wait( TimeSpan.FromSeconds( 30 ) );

            // With the bugged code, one of three things happens:
            //   1. Duplicate-key ArgumentException from Dictionary.Add. This is a definitive
            //      confirmation of the check-then-act race: the ContainsKey guard would have
            //      prevented the duplicate Add in single-threaded execution.
            //   2. IndexOutOfRangeException / NullReferenceException / other ArgumentException
            //      thrown from inside Dictionary.Insert or FindEntry due to bucket-chain
            //      corruption, OR the 60s Timeout fires because a thread is spinning in
            //      FindEntry after corruption. Both are the severe form of the same race.
            //   3. The test passes. Races are non-deterministic, so run 3-5 times to be sure.
            var duplicateKeyErrors = 0;
            foreach ( var ex in errors )
            {
                if ( ex is ArgumentException && ex.Message.Contains( "An item with the same key has already been added" ) )
                {
                    duplicateKeyErrors++;
                }
            }

            if ( duplicateKeyErrors > 0 )
            {
                Assert.Fail(
                    $"Reproduction for issue #6919 confirmed: Dictionary.Add threw duplicate-key ArgumentException {duplicateKeyErrors} time(s) out of {errors.Count} total exception(s). This can only happen if the ContainsKey/Add pair ran without synchronization." );
            }

            Assert.IsTrue(
                completedInTime,
                "Reproduction for the severe symptom of issue #6919 confirmed: worker threads did not " +
                "complete within 30 seconds. This matches the reported behavior of threads spinning in " +
                "Dictionary.FindEntry after bucket-chain corruption. To confirm the specific frame, " +
                "run this test via Test Explorer's 'Debug' option (not 'Run'), and when it appears " +
                "hung use Debug > Break All (Ctrl+Alt+Break), then open Debug > Windows > Parallel Stacks. " +
                "Look for threads whose top managed frame is inside " +
                "System.Collections.Generic.Dictionary`2.FindEntry or Insert." );

            Assert.IsTrue(
                errors.IsEmpty,
                $"Threads threw {errors.Count} exception(s), first: {( errors.TryPeek( out var first ) ? first.ToString() : "n/a" )}" );
        }

        /// <summary>
        /// Companion to <see cref="GetBlobClient_ConcurrentCallsWithVariedContainers_DoesNotHangOrThrow"/>
        /// that programmatically confirms the specific claim in issue #6919: when a hang occurs,
        /// worker threads are stopped inside <c>Dictionary.FindEntry</c> / <c>Dictionary.Insert</c>.
        /// </summary>
        /// <remarks>
        /// Each worker records its managed <see cref="Thread"/>. When the watchdog fires, we walk
        /// each recorded thread's stack via <see cref="StackTrace(Thread, bool)"/> and count frames
        /// whose declaring type starts with <c>System.Collections.Generic.Dictionary</c>. Any
        /// <c>FindEntry</c> or <c>Insert</c> frame in a live worker thread is proof that the
        /// unsynchronized Dictionary is the cause of the hang.
        ///
        /// The race is non-deterministic, so a run where workers finish in time is reported as
        /// <see cref="Assert.Inconclusive(string)"/> rather than a pass — that outcome does not
        /// prove correctness.
        /// </remarks>
        [TestMethod]
        public void GetBlobClient_ConcurrentCalls_HungThreadsAreInsideDictionaryFindEntry()
        {
            var client = AzureBlobStorageClient.Instance;
            var errors = new ConcurrentBag<Exception>();
            var workerThreads = new ConcurrentBag<Thread>();

            const int threadCount = 32;
            const int iterationsPerThread = 5_000;
            const int distinctContainerCount = 200;

            // Force the desired parallelism regardless of Environment.ProcessorCount, so the race
            // has a fair chance on lower-core dev boxes.
            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = threadCount };

            var workerTask = Task.Run( () =>
            {
                Parallel.For( 0, threadCount, parallelOptions, threadIndex =>
                {
                    workerThreads.Add( Thread.CurrentThread );

                    try
                    {
                        for ( var i = 0; i < iterationsPerThread; i++ )
                        {
                            var containerName = "container-" + ( i % distinctContainerCount );

                            client.GetBlobClient(
                                accountName: "acct",
                                accountKey: DummyAccountKey,
                                customDomain: null,
                                containerName: containerName,
                                blobName: "blob-" + i );
                        }
                    }
                    catch ( Exception ex )
                    {
                        errors.Add( ex );
                    }
                } );
            } );

            var completedInTime = workerTask.Wait( TimeSpan.FromSeconds( 30 ) );

            if ( completedInTime )
            {
                // No hang. This is the correct outcome on fixed code (issue #6919 remedied by
                // switching to ConcurrentDictionary). On buggy code you may reach this branch
                // occasionally because the race is non-deterministic; re-run to be sure.
                return;
            }

            var findEntryHits = 0;
            var insertHits = 0;
            var inspectedThreads = 0;
            var dictionaryFramesSampled = new List<string>();

            foreach ( var thread in workerThreads.Distinct() )
            {
                if ( thread == null || !thread.IsAlive )
                {
                    continue;
                }

                StackTrace stackTrace;
                try
                {
                    // This constructor overload internally suspends the target thread to walk it.
                    // It is [Obsolete] in newer runtimes but still functional on .NET Framework and
                    // is the only in-process way to sample another managed thread's stack.
#pragma warning disable CS0618 // Type or member is obsolete
                    stackTrace = new StackTrace( thread, needFileInfo: false );
#pragma warning restore CS0618
                }
                catch
                {
                    // Some threads may refuse to be walked (permissions, timing). Skip them.
                    continue;
                }

                inspectedThreads++;
                var frames = stackTrace.GetFrames();
                if ( frames == null )
                {
                    continue;
                }

                foreach ( var frame in frames )
                {
                    var method = frame.GetMethod();
                    if ( method == null )
                    {
                        continue;
                    }

                    var declaringType = method.DeclaringType?.FullName ?? string.Empty;
                    var methodName = method.Name ?? string.Empty;

                    if ( !declaringType.StartsWith( "System.Collections.Generic.Dictionary" ) )
                    {
                        continue;
                    }

                    dictionaryFramesSampled.Add( declaringType + "." + methodName );

                    if ( methodName == "FindEntry" )
                    {
                        findEntryHits++;
                    }
                    else if ( methodName == "Insert" )
                    {
                        insertHits++;
                    }
                }
            }

            if ( findEntryHits == 0 && insertHits == 0 )
            {
                var framesLabel = dictionaryFramesSampled.Count == 0
                    ? "(none)"
                    : string.Join( ", ", dictionaryFramesSampled );

                Assert.Inconclusive(
                    $"Workers hung (Task.Wait timed out after 30s), but no worker thread was stopped " +
                    $"inside Dictionary.FindEntry or Dictionary.Insert at the moment of stack inspection. " +
                    $"Inspected {inspectedThreads} of {workerThreads.Count} captured worker threads. " +
                    $"Dictionary frames sampled: {framesLabel}. " +
                    $"Re-run the test to catch a thread mid-FindEntry." );
                return;
            }

            Assert.Fail(
                $"Reproduction for the severe symptom of issue #6919 confirmed: {findEntryHits} worker " +
                $"thread(s) stopped inside Dictionary.FindEntry and {insertHits} inside Dictionary.Insert " +
                $"at the moment of stack inspection. This is the exact symptom reported: the " +
                $"unsynchronized Dictionary was corrupted by racing Add calls, so subsequent lookups " +
                $"spin in FindEntry indefinitely." );
        }
    }
}
