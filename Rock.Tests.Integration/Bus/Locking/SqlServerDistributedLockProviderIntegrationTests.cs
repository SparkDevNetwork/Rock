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
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Rock.Bus.Locking;
using Rock.Configuration;

using Testcontainers.MsSql;

namespace Rock.Tests.Integration.Bus.Locking;

/// <summary>
/// End-to-end tests for <see cref="SqlServerDistributedLockProvider"/>
/// against a real SQL Server instance running in a Docker container. These
/// tests exercise the actual <c>sp_getapplock</c> integration provided by
/// the <c>DistributedLock.SqlServer</c> package, so they validate the
/// semantics that unit tests cannot: contention between two independent
/// providers, automatic release on connection close, reentrancy detection
/// via the internal AsyncLocal set, and the dedicated
/// <c>Application Name=RockDistributedLock</c> session pool.
/// </summary>
/// <remarks>
/// A fresh SQL Server container is started once for the entire class.
/// The tests use empty <c>master</c> — no Rock schema is required because
/// the provider talks only to <c>sp_getapplock</c>, which is a system
/// stored procedure available on every database.
/// </remarks>
[TestClass]
[TestCategory( "Distributed Locking" )]
public class SqlServerDistributedLockProviderIntegrationTests
{
    public TestContext TestContext { get; set; }

    #region Marker Types

    /// <summary>
    /// A plain non-generic marker used by tests that do not need to
    /// distinguish between different lock namespaces. Nested types are
    /// intentional: they let each test isolate its lock keyspace from
    /// its siblings without introducing top-level test-only types into
    /// the namespace surface area.
    /// </summary>
    private sealed class TestMarker { }

    /// <summary>
    /// A second marker used to prove that different markers with the
    /// same resource id do not collide in the lock keyspace.
    /// </summary>
    private sealed class OtherMarker { }

    #endregion

    #region Container Lifecycle

    /// <summary>
    /// The SQL Server container shared by every test in the class. Kept
    /// as a static so the ~10-second cold start happens once per class
    /// rather than once per test.
    /// </summary>
    private static MsSqlContainer _container;

    /// <summary>
    /// Base connection string built from the container's endpoint. Every
    /// test derives its own <see cref="IConnectionStringProvider"/> from
    /// this so the provider under test builds the RockDistributedLock
    /// pool connection string with real credentials.
    /// </summary>
    private static string _baseConnectionString;

    [ClassInitialize]
    public static async Task ClassInitialize( TestContext context )
    {
        _container = new MsSqlBuilder().Build();
        await _container.StartAsync( context.CancellationToken );
        _baseConnectionString = _container.GetConnectionString();
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        if ( _container != null )
        {
            await _container.DisposeAsync();
        }
    }

    /// <summary>
    /// Creates a fresh provider bound to the shared container. Each test
    /// gets its own instance so the AsyncLocal reentrancy-tracking state
    /// (which the provider itself does not own but which flows on the
    /// current logical call context) does not bleed across tests.
    /// </summary>
    private static SqlServerDistributedLockProvider CreateProvider()
    {
        return new SqlServerDistributedLockProvider( new TestConnectionStringProvider( _baseConnectionString ) );
    }

    /// <summary>
    /// Generates a unique resource id per test so tests do not race each
    /// other on shared lock keys. The id includes ticks so parallel test
    /// runs cannot collide, and only characters allowed by the provider's
    /// key validation.
    /// </summary>
    private static string UniqueResourceId()
    {
        return "t" + DateTime.UtcNow.Ticks.ToString();
    }

    #endregion

    #region Acquisition and Release

    [TestMethod]
    public void TryAcquire_UncontestedLock_IsAcquired()
    {
        var provider = CreateProvider();
        var resourceId = UniqueResourceId();

        using var handle = provider.TryAcquire( typeof( TestMarker ), resourceId, TimeSpan.Zero );

        Assert.IsTrue( handle.IsAcquired, "An uncontested lock must be acquired immediately." );
    }

    [TestMethod]
    public void TryAcquire_ReleasedLock_CanBeReAcquired()
    {
        // Automatic release is the whole point of session-scoped
        // applocks: after Dispose the lock must be immediately available
        // to the next acquisition, without any manual cleanup.
        var provider = CreateProvider();
        var resourceId = UniqueResourceId();

        using ( var first = provider.TryAcquire( typeof( TestMarker ), resourceId, TimeSpan.Zero ) )
        {
            Assert.IsTrue( first.IsAcquired );
        }

        using var second = provider.TryAcquire( typeof( TestMarker ), resourceId, TimeSpan.Zero );

        Assert.IsTrue( second.IsAcquired, "A previously-released lock must be re-acquirable in the same flow." );
    }

    #endregion

    #region Contention

    [TestMethod]
    public void TryAcquire_ContentionOnSameKey_OnlyOneWins()
    {
        // Two independent providers (simulating two Rock instances)
        // race for the same lock key with a zero timeout. Exactly one
        // must observe IsAcquired=true and the other IsAcquired=false.
        // This is the fundamental correctness property the primitive
        // must guarantee.
        var providerA = CreateProvider();
        var providerB = CreateProvider();
        var resourceId = UniqueResourceId();

        using var winner = providerA.TryAcquire( typeof( TestMarker ), resourceId, TimeSpan.Zero );
        using var loser = providerB.TryAcquire( typeof( TestMarker ), resourceId, TimeSpan.Zero );

        Assert.IsTrue( winner.IsAcquired, "First acquisition must succeed." );
        Assert.IsFalse( loser.IsAcquired, "Second acquisition on the held lock must not succeed with zero timeout." );
    }

    [TestMethod]
    public void TryAcquire_DifferentResourceIds_DoNotCollide()
    {
        // The lock keyspace is (markerType, resourceId), so the same
        // marker with different resource ids must have independent
        // coordination. If this failed, every job would serialize on
        // its class rather than its identity, which would defeat the
        // whole design.
        var provider = CreateProvider();
        var id1 = UniqueResourceId() + "-a";
        var id2 = UniqueResourceId() + "-b";

        using var handle1 = provider.TryAcquire( typeof( TestMarker ), id1, TimeSpan.Zero );
        using var handle2 = provider.TryAcquire( typeof( TestMarker ), id2, TimeSpan.Zero );

        Assert.IsTrue( handle1.IsAcquired );
        Assert.IsTrue( handle2.IsAcquired, "Different resource ids under the same marker must not contend." );
    }

    [TestMethod]
    public void TryAcquire_DifferentMarkers_DoNotCollide()
    {
        // The marker type is part of the key, so the same resource id
        // under different markers must have independent coordination.
        // This is what lets unrelated subsystems (jobs vs communications)
        // reuse the number "42" without stepping on each other.
        var provider = CreateProvider();
        var resourceId = UniqueResourceId();

        using var handle1 = provider.TryAcquire( typeof( TestMarker ), resourceId, TimeSpan.Zero );
        using var handle2 = provider.TryAcquire( typeof( OtherMarker ), resourceId, TimeSpan.Zero );

        Assert.IsTrue( handle1.IsAcquired );
        Assert.IsTrue( handle2.IsAcquired, "Different markers with the same resource id must not contend." );
    }

    [TestMethod]
    public async Task TryAcquireAsync_WithTimeout_WaitsForRelease()
    {
        // A caller passing a non-zero timeout should block up to that
        // duration for the lock to become available. Here we hold the
        // lock briefly on one thread and start a waiting acquisition on
        // another; the waiter should succeed once the holder releases.
        var providerA = CreateProvider();
        var providerB = CreateProvider();
        var resourceId = UniqueResourceId();

        var holdReleased = new TaskCompletionSource<bool>();
        var releaseHold = new TaskCompletionSource<bool>();

        // Start a task that acquires and holds the lock until told to release.
        var holderTask = Task.Run( async () =>
        {
            using var handle = providerA.TryAcquire( typeof( TestMarker ), resourceId, TimeSpan.Zero );

            Assert.IsTrue( handle.IsAcquired, "Holder task must acquire the lock." );

            holdReleased.SetResult( true );
            await releaseHold.Task;
        }, TestContext.CancellationToken );

        await holdReleased.Task;

        // Start the waiter with a 5-second timeout, then release the
        // hold from the other thread. The waiter should acquire cleanly.
        var waiterTask = Task.Run( async () =>
            await providerB.TryAcquireAsync( typeof( TestMarker ), resourceId, TimeSpan.FromSeconds( 5 ), TestContext.CancellationToken ) );

        // Give the waiter a moment to enter its wait, then release.
        await Task.Delay( 200, TestContext.CancellationToken );
        releaseHold.SetResult( true );

        using ( var waited = await waiterTask )
        {
            Assert.IsTrue( waited.IsAcquired, "Waiter should acquire after the holder releases within the timeout window." );
        }

        await holderTask;
    }

    #endregion

    #region Reentrancy

    [TestMethod]
    public void TryAcquire_SameFlowReAcquires_Throws()
    {
        // AsyncLocal-tracked reentrancy detection. The same logical flow
        // holding the lock and attempting a second acquire must throw
        // rather than silently returning IsAcquired=false, so caller
        // bugs surface loudly.
        var provider = CreateProvider();
        var resourceId = UniqueResourceId();

        using var handle = provider.TryAcquire( typeof( TestMarker ), resourceId, TimeSpan.Zero );

        Assert.IsTrue( handle.IsAcquired );

        Assert.ThrowsExactly<DistributedLockReentrancyException>( () =>
            provider.TryAcquire( typeof( TestMarker ), resourceId, TimeSpan.Zero ) );
    }

    [TestMethod]
    public void TryAcquire_AfterRelease_NoReentrancyThrow()
    {
        // Sanity check: reentrancy tracking must be cleared on dispose
        // so a legitimate re-acquire after release does not spuriously
        // throw. Also tests that DistributedLockReentrancyException is
        // not thrown for the second acquisition in a serial pattern.
        var provider = CreateProvider();
        var resourceId = UniqueResourceId();

        using ( var handle = provider.TryAcquire( typeof( TestMarker ), resourceId, TimeSpan.Zero ) )
        {
            Assert.IsTrue( handle.IsAcquired );
        }

        using ( var handle = provider.TryAcquire( typeof( TestMarker ), resourceId, TimeSpan.Zero ) )
        {
            Assert.IsTrue( handle.IsAcquired, "Second acquire after clean release should succeed without a reentrancy throw." );
        }
    }

    #endregion

    #region Application Name / Pool Separation

    [TestMethod]
    public void TryAcquire_UsesRockDistributedLockApplicationName()
    {
        // The dedicated Application Name is the mechanism that gives
        // the lock connections their own SqlConnection pool (isolated
        // from EF6) and lets DBAs identify lock sessions in
        // sys.dm_exec_sessions. Prove the sessions show up with the
        // expected program_name.
        var provider = CreateProvider();
        var resourceId = UniqueResourceId();

        using var handle = provider.TryAcquire( typeof( TestMarker ), resourceId, TimeSpan.Zero );

        Assert.IsTrue( handle.IsAcquired );

        // Query from a separate connection so we don't collide with
        // the lock connection's session.
        var sessionCount = CountSessionsWithProgramName( SqlServerDistributedLockProvider.ApplicationName );

        Assert.IsGreaterThanOrEqualTo( 1, sessionCount, $"Expected at least one active session with program_name '{SqlServerDistributedLockProvider.ApplicationName}' while holding a lock." );
    }

    /// <summary>
    /// Counts active user sessions whose <c>program_name</c> matches
    /// <paramref name="programName"/>. This is the same query pattern
    /// operators would use in production to attribute lock activity to
    /// the distributed-lock subsystem.
    /// </summary>
    private static int CountSessionsWithProgramName( string programName )
    {
        using var connection = new SqlConnection( _baseConnectionString );

        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = "SELECT COUNT(*) FROM sys.dm_exec_sessions WHERE is_user_process = 1 AND program_name = @ProgramName";
        command.Parameters.AddWithValue( "@ProgramName", programName );

        return ( int ) command.ExecuteScalar();
    }

    #endregion

    #region Handle Semantics

    [TestMethod]
    public void Dispose_IsIdempotent()
    {
        // Real handles (not the singleton unacquired handle) should be
        // safely disposable more than once. The provider guards against
        // double-release using an Interlocked exchange because SqlClient
        // will surface a redundant disposal as a benign SqlException on
        // the second try.
        var provider = CreateProvider();
        var resourceId = UniqueResourceId();
        var handle = provider.TryAcquire( typeof( TestMarker ), resourceId, TimeSpan.Zero );

        Assert.IsTrue( handle.IsAcquired );

        handle.Dispose();
        handle.Dispose();
        handle.Dispose();

        // No throw = success.
    }

    [TestMethod]
    public void UnacquiredHandle_IsSafeToDispose()
    {
        // A contested acquisition returns an unacquired handle sentinel.
        // Callers still Dispose it inside their `using` blocks, so the
        // dispose path must not throw or attempt to release anything
        // in SQL Server.
        var providerA = CreateProvider();
        var providerB = CreateProvider();
        var resourceId = UniqueResourceId();

        using var winner = providerA.TryAcquire( typeof( TestMarker ), resourceId, TimeSpan.Zero );

        Assert.IsTrue( winner.IsAcquired );

        var loser = providerB.TryAcquire( typeof( TestMarker ), resourceId, TimeSpan.Zero );
        Assert.IsFalse( loser.IsAcquired );

        // Two Dispose calls on the unacquired handle to prove
        // idempotence on that path too.
        loser.Dispose();
        loser.Dispose();
    }

    [TestMethod]
    public void UnacquiredHandle_LostTokenIsNone()
    {
        // The lost-token contract for unacquired handles is
        // CancellationToken.None so callers watching LostToken uniformly
        // never fire spuriously in the "we didn't get the lock" path.
        var providerA = CreateProvider();
        var providerB = CreateProvider();
        var resourceId = UniqueResourceId();

        using var winner = providerA.TryAcquire( typeof( TestMarker ), resourceId, TimeSpan.Zero );
        using var loser = providerB.TryAcquire( typeof( TestMarker ), resourceId, TimeSpan.Zero );

        Assert.IsFalse( loser.IsAcquired );
        Assert.AreEqual( CancellationToken.None, loser.LostToken );
    }

    #endregion

    #region Async

    [TestMethod]
    public async Task TryAcquireAsync_UncontestedLock_IsAcquired()
    {
        var provider = CreateProvider();
        var resourceId = UniqueResourceId();

        using var handle = await provider.TryAcquireAsync( typeof( TestMarker ), resourceId, TimeSpan.Zero, TestContext.CancellationToken );

        Assert.IsTrue( handle.IsAcquired );
    }

    [TestMethod]
    public async Task TryAcquireAsync_ContentionOnSameKey_LoserReportsUnacquired()
    {
        // Async peer to TryAcquire_ContentionOnSameKey_OnlyOneWins. This
        // exercises the async path's `innerHandle == null` branch (the
        // library returns a null task result on zero-timeout contention),
        // which the earlier tests didn't reach.
        var providerA = CreateProvider();
        var providerB = CreateProvider();
        var resourceId = UniqueResourceId();

        using var winner = await providerA.TryAcquireAsync( typeof( TestMarker ), resourceId, TimeSpan.Zero, TestContext.CancellationToken );
        using var loser = await providerB.TryAcquireAsync( typeof( TestMarker ), resourceId, TimeSpan.Zero, TestContext.CancellationToken );

        Assert.IsTrue( winner.IsAcquired, "First async acquire must succeed." );
        Assert.IsFalse( loser.IsAcquired, "Second async acquire on the held lock must not succeed at zero timeout." );
    }

    [TestMethod]
    public void AcquiredHandle_ExposesUnderlyingHandleLostToken()
    {
        // On a real (acquired) handle the LostToken must forward the
        // underlying DistributedLock library's HandleLostToken, so
        // callers can observe connection death mid-hold. This test
        // exercises the property getter itself (which the unacquired-
        // handle tests don't hit) and verifies that under healthy
        // conditions the token has not fired.
        var provider = CreateProvider();
        var resourceId = UniqueResourceId();

        using var handle = provider.TryAcquire( typeof( TestMarker ), resourceId, TimeSpan.Zero );

        Assert.IsTrue( handle.IsAcquired );

        var lostToken = handle.LostToken;

        // This inequality assertion couples the test to a DistributedLock.SqlServer
        // implementation detail: HandleLostToken is documented to be backed by a
        // real CancellationTokenSource so keepalive/failover cancellation can fire.
        // If a future package version returns CancellationToken.None for a healthy
        // fast-path handle, this assertion will fail even though our forwarding is
        // still correct — investigate the library's behavior before assuming a
        // Rock regression.
        Assert.AreNotEqual( CancellationToken.None, lostToken, "Acquired handle should surface the library's real lost-token, not the None sentinel." );
        Assert.IsFalse( lostToken.IsCancellationRequested, "Lost token must not be fired while the connection is healthy." );
    }

    [TestMethod]
    public void TryAcquire_InfrastructureFailure_ReturnsUnacquired()
    {
        // Point the provider at a connection string that will fail to
        // connect (short connect timeout to a non-routable address). The
        // provider must catch the SqlException at the acquisition step,
        // log at Warning, and return IsAcquired=false — NOT propagate
        // the exception. This is the "pool exhausted / tier limit hit /
        // network down" branch from the Failure Modes table.
        var badConnectionString = "Server=nonexistent.invalid;Database=master;Integrated Security=false;User Id=sa;Password=nope;Connect Timeout=2;TrustServerCertificate=true;Encrypt=false";
        var provider = new SqlServerDistributedLockProvider( new TestConnectionStringProvider( badConnectionString ) );

        using var handle = provider.TryAcquire( typeof( TestMarker ), UniqueResourceId(), TimeSpan.Zero );

        Assert.IsFalse( handle.IsAcquired, "Unreachable server must surface as IsAcquired=false, not a thrown SqlException." );
        Assert.AreEqual( CancellationToken.None, handle.LostToken, "Unacquired handle should have the None sentinel token." );
    }

    [TestMethod]
    public async Task TryAcquireAsync_InfrastructureFailure_ReturnsUnacquired()
    {
        // Async peer to TryAcquire_InfrastructureFailure_ReturnsUnacquired.
        // Exercises the async catch block that handles non-cancellation
        // exceptions.
        var badConnectionString = "Server=nonexistent.invalid;Database=master;Integrated Security=false;User Id=sa;Password=nope;Connect Timeout=2;TrustServerCertificate=true;Encrypt=false";
        var provider = new SqlServerDistributedLockProvider( new TestConnectionStringProvider( badConnectionString ) );

        using var handle = await provider.TryAcquireAsync( typeof( TestMarker ), UniqueResourceId(), TimeSpan.Zero, TestContext.CancellationToken );

        Assert.IsFalse( handle.IsAcquired );
    }

    [TestMethod]
    public async Task TryAcquireAsync_CanceledBeforeAcquire_ReturnsUnacquired()
    {
        // Cancellation of an in-flight acquisition is a caller-directed
        // abort, not a system failure. The provider surfaces it as
        // IsAcquired=false so callers can uniformly branch on the same
        // property regardless of why they didn't get the lock. It must
        // NOT propagate the OperationCanceledException.
        var providerA = CreateProvider();
        var providerB = CreateProvider();
        var resourceId = UniqueResourceId();

        using var holder = providerA.TryAcquire( typeof( TestMarker ), resourceId, TimeSpan.Zero );

        Assert.IsTrue( holder.IsAcquired );

        using var cts = new CancellationTokenSource();

        // Cancel immediately so the waiter's SQL-side wait is
        // interrupted right away.
        cts.Cancel();

        using var canceled = await providerB.TryAcquireAsync( typeof( TestMarker ), resourceId, TimeSpan.FromSeconds( 10 ), cts.Token );

        Assert.IsFalse( canceled.IsAcquired, "Canceled acquisition must report IsAcquired=false, not throw." );
    }

    #endregion

    #region Test Helpers

    /// <summary>
    /// Minimal <see cref="IConnectionStringProvider"/> implementation for
    /// wiring the container's connection string into the provider under
    /// test. Only the primary <see cref="ConnectionString"/> is used by
    /// the lock provider; the read-only and analytics variants are
    /// stubbed to the same value to satisfy the contract.
    /// </summary>
    private sealed class TestConnectionStringProvider : IConnectionStringProvider
    {
        public TestConnectionStringProvider( string connectionString )
        {
            ConnectionString = connectionString;
            ReadOnlyConnectionString = connectionString;
            AnalyticsConnectionString = connectionString;
        }

        public string ConnectionString { get; }

        public string ReadOnlyConnectionString { get; }

        public string AnalyticsConnectionString { get; }
    }

    #endregion
}
