// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Rock.Communication.Chat.Platform.Configuration;
using Rock.Data;
using Rock.Jobs;
using Rock.Tests.Integration.TestFramework.Database;

namespace Rock.Tests.Integration.Communication.Chat.Platform.Sync
{
    /// <summary>
    /// Times the whole chat sync projection against a large seeded church, so a change to it can be
    /// held to the same payload and no more time or memory than the code before it.
    /// </summary>
    /// <remarks>
    /// Runs only when <c>ROCK_CHAT_SYNC_BENCHMARK</c> is set, against the database the test
    /// connection string names, because the figures mean something only at the scale of a real
    /// church. Set <c>ROCK_CHAT_SYNC_BENCHMARK_HASH</c> to fail on a payload that differs from a
    /// known one. The hash sorts each section's rows first, because three section queries carry no
    /// ordering and a correct change to a plan may reorder rows.
    /// </remarks>
    [TestClass]
    [TestCategory( "Benchmark" )]
    public class ProjectionBenchmark : DatabaseTestsBase
    {
        private const int MeasuredRuns = 5;

        [TestMethod]
        public void TheWholeProjectionAtScale()
        {
            if ( Environment.GetEnvironmentVariable( "ROCK_CHAT_SYNC_BENCHMARK" ).IsNullOrWhiteSpace() )
            {
                Assert.Inconclusive( "Set ROCK_CHAT_SYNC_BENCHMARK and point the test connection string at a seeded church." );
            }

            // Net472 has no per-thread allocation counter, so this is app-domain-wide; nothing else
            // runs in the host while a call is in flight.
            AppDomain.MonitoringIsEnabled = true;

            var configuration = new ChatPlatformConfiguration
            {
                AreChatProfilesVisible = true,
                IsOpenDirectMessagingAllowed = true,
                ChatBadgeDataViewGuids = SeededBadgeGuids()
            };

            // The first call in a process loads Rock's caches and compiles; it is not the projection.
            Measure( configuration );
            var runs = Enumerable.Range( 0, MeasuredRuns ).Select( _ => Measure( configuration ) ).ToList();

            var first = runs.First();
            TestContext.WriteLine( string.Join( Environment.NewLine,
                "elapsed ms: " + string.Join( ", ", runs.Select( r => r.ElapsedMs.ToString( "F1" ) ) ),
                "allocated bytes: " + string.Join( ", ", runs.Select( r => r.AllocatedBytes.ToString( "N0" ) ) ),
                $"median elapsed ms: {Median( runs.Select( r => r.ElapsedMs ) ):F1}",
                $"median allocated bytes: {Median( runs.Select( r => ( double ) r.AllocatedBytes ) ):N0}",
                $"max gen2 delta: {runs.Max( r => r.Gen2Collections )}",
                $"peak working set bytes: {runs.Max( r => r.PeakWorkingSetBytes ):N0}",
                $"payload bytes: {first.PayloadBytes:N0}",
                "rows: " + string.Join( ", ", first.RowCounts.Select( c => $"{c.Key} {c.Value:N0}" ) ),
                $"sorted payload sha256: {first.SortedSha256}" ) );

            // A benchmark whose output drifts between readings of one database has nothing to compare.
            Assert.AreEqual( 1, runs.Select( r => r.SortedSha256 ).Distinct().Count(), "the readings disagree with each other" );

            var expected = Environment.GetEnvironmentVariable( "ROCK_CHAT_SYNC_BENCHMARK_HASH" );
            if ( expected.IsNotNullOrWhiteSpace() )
            {
                Assert.AreEqual( expected.Trim().ToLowerInvariant(), first.SortedSha256, "the payload differs from the expected one" );
            }
        }

        /// <summary>
        /// The projection itself. This is the only line that names the code under measurement.
        /// </summary>
        private static ArraySegment<byte> Project( RockContext rockContext, ChatPlatformConfiguration configuration, out IDictionary<string, int> rowCounts )
        {
            var result = ChatPlatformSync.Project( rockContext, configuration );
            rowCounts = result.RowCounts;

            return result.Payload;
        }

        /// <summary>
        /// Takes one reading and the time and memory around it.
        /// </summary>
        private static Run Measure( ChatPlatformConfiguration configuration )
        {
            // Collected before, so a run's generation two count is its own and not the last run's debris.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            using ( var rockContext = new RockContext() )
            {
                var gen2Before = GC.CollectionCount( 2 );
                var allocatedBefore = AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize;
                var stopwatch = Stopwatch.StartNew();

                var payload = Project( rockContext, configuration, out var rowCounts );

                stopwatch.Stop();
                var allocated = AppDomain.CurrentDomain.MonitoringTotalAllocatedMemorySize - allocatedBefore;
                var process = Process.GetCurrentProcess();
                process.Refresh();

                return new Run
                {
                    ElapsedMs = stopwatch.Elapsed.TotalMilliseconds,
                    AllocatedBytes = allocated,
                    Gen2Collections = GC.CollectionCount( 2 ) - gen2Before,
                    PeakWorkingSetBytes = process.PeakWorkingSet64,
                    PayloadBytes = payload.Count,
                    RowCounts = new Dictionary<string, int>( rowCounts ),
                    SortedSha256 = SortedHash( payload )
                };
            }
        }

        /// <summary>
        /// The badge Data Views the seeded church defines, in the order its seed ranks them.
        /// </summary>
        private static List<Guid> SeededBadgeGuids()
        {
            using ( var rockContext = new RockContext() )
            {
                return rockContext.Database.SqlQuery<Guid>(
                    "IF OBJECT_ID( 'dbo.zzBadge' ) IS NOT NULL SELECT [DvGuid] FROM [dbo].[zzBadge] ORDER BY [Ord] ELSE SELECT CAST( NULL AS UNIQUEIDENTIFIER ) WHERE 1 = 0" )
                    .ToList();
            }
        }

        /// <summary>
        /// SHA-256 over each section's name and then its rows, each re-serialised compactly and sorted
        /// ordinally, one per line. Dates are left as the text the payload carried.
        /// </summary>
        private static string SortedHash( ArraySegment<byte> payload )
        {
            JObject body;

            using ( var stream = new MemoryStream( payload.Array ?? new byte[0], payload.Offset, payload.Count, false ) )
            using ( var text = new StreamReader( stream, new UTF8Encoding( false ) ) )
            using ( var reader = new JsonTextReader( text ) { DateParseHandling = DateParseHandling.None, FloatParseHandling = FloatParseHandling.Decimal } )
            {
                body = JObject.Load( reader );
            }

            var canonical = new StringBuilder();

            foreach ( var property in body.Properties() )
            {
                canonical.Append( property.Name ).Append( '\n' );

                var rows = property.Value is JArray array
                    ? array.Select( r => r.ToString( Formatting.None ) ).ToList()
                    : new List<string> { property.Value.ToString( Formatting.None ) };

                rows.Sort( StringComparer.Ordinal );
                rows.ForEach( row => canonical.Append( row ).Append( '\n' ) );
            }

            using ( var sha = SHA256.Create() )
            {
                return string.Concat( sha.ComputeHash( Encoding.UTF8.GetBytes( canonical.ToString() ) ).Select( b => b.ToString( "x2" ) ) );
            }
        }

        private static double Median( IEnumerable<double> values )
        {
            var sorted = values.OrderBy( v => v ).ToList();
            var middle = sorted.Count / 2;

            return sorted.Count % 2 == 1 ? sorted[middle] : ( sorted[middle - 1] + sorted[middle] ) / 2;
        }

        private sealed class Run
        {
            public double ElapsedMs { get; set; }
            public long AllocatedBytes { get; set; }
            public int Gen2Collections { get; set; }
            public long PeakWorkingSetBytes { get; set; }
            public int PayloadBytes { get; set; }
            public Dictionary<string, int> RowCounts { get; set; }
            public string SortedSha256 { get; set; }
        }
    }
}
