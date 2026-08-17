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
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using Medallion.Threading.SqlServer;

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

using Rock.Configuration;
using Rock.Logging;

namespace Rock.Bus.Locking;

/// <summary>
/// Default <see cref="IDistributedLockProvider"/> implementation backed by
/// SQL Server application locks (<c>sp_getapplock</c>/
/// <c>sp_releaseapplock</c>) via the <c>DistributedLock.SqlServer</c>
/// package. Held locks are session-scoped: SQL Server releases them
/// automatically when the underlying connection closes, which eliminates
/// stale-lock cleanup even when a Rock process crashes.
/// </summary>
/// <remarks>
/// <para>
/// Lock-holding connections use a dedicated connection pool identified by
/// the connection string keyword <c>Application Name=RockDistributedLock</c>
/// so they cannot starve Rock's default EF6 pool. The pool size is
/// tunable via the <c>RockDistributedLockMaxPoolSize</c> app setting;
/// the default is 50 per Rock instance.
/// </para>
/// <para>
/// The provider delegates to <c>Microsoft.Data.SqlClient</c>, which
/// coexists in the process alongside Rock's EF6 workload on
/// <c>System.Data.SqlClient</c> without functional issue. Both drivers
/// publish PerformanceCounters to the same
/// <c>.NET Data Provider for SqlServer</c> category, so Rock's existing
/// <c>rock.database.connections.pooled</c> gauge aggregates activity from
/// both drivers automatically.
/// </para>
/// </remarks>
[RockLoggingCategory]
internal sealed class SqlServerDistributedLockProvider : IDistributedLockProvider
{
    #region Constants

    /// <summary>
    /// The <c>Application Name</c> value used for the dedicated lock
    /// connection pool. Also the value operators see in
    /// <c>sys.dm_exec_sessions.program_name</c> for lock-holding sessions.
    /// </summary>
    internal const string ApplicationName = "RockDistributedLock";

    /// <summary>
    /// The web.config app setting that overrides the default pool size.
    /// </summary>
    internal const string MaxPoolSizeSettingKey = "RockDistributedLockMaxPoolSize";

    /// <summary>
    /// Default per-instance pool size when the app setting is not
    /// specified. Sized against realistic near-term concurrent lock
    /// counts (Quartz jobs plus communication sending) with modest
    /// headroom.
    /// </summary>
    internal const int DefaultMaxPoolSize = 50;

    /// <summary>
    /// SQL Server's own <c>sp_getapplock @Resource</c> nvarchar(255)
    /// limit. Keys longer than this are rejected at the API boundary so
    /// they never trigger the library's SHA512 hashing fallback (which
    /// would destroy the readability of
    /// <c>sys.dm_tran_locks.resource_description</c>).
    /// </summary>
    internal const int MaxLockKeyLength = 255;

    #endregion

    #region Fields

    /// <summary>
    /// The connection string used for every lock-holding connection.
    /// Built once at construction from Rock's primary connection string
    /// with the <c>Application Name</c> and pool size overrides applied.
    /// </summary>
    private readonly string _connectionString;

    /// <summary>
    /// The set of lock keys currently held by the flow this provider
    /// instance is serving. AsyncLocal so state flows across async
    /// continuations; instance-scoped (not static) so two provider
    /// instances in the same process see independent held-key state.
    /// The distinction matters in tests that simulate "two Rock nodes"
    /// by creating two providers in one process — a static AsyncLocal
    /// would erroneously report reentrancy across those independent
    /// providers. In real production Rock the provider is a DI
    /// singleton and only one instance exists per process, so this
    /// scoping has no effect on the intended reentrancy detection.
    /// The set instance itself is mutated under a lock because
    /// AsyncLocal shares the reference across parallel forks within
    /// the same flow.
    /// </summary>
    private readonly AsyncLocal<HashSet<string>> _heldKeys = new AsyncLocal<HashSet<string>>();

    /// <summary>
    /// The logger for this instance.
    /// </summary>
    private ILogger _logger;

    #endregion

    #region Properties

    /// <summary>
    /// Gets the logger for this instance.
    /// </summary>
    private ILogger Logger
    {
        get
        {
            if ( _logger == null )
            {
                _logger = RockLogger.LoggerFactory.CreateLogger( GetType().FullName );
            }

            return _logger;
        }
    }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new <see cref="SqlServerDistributedLockProvider"/>
    /// against Rock's primary connection string.
    /// </summary>
    /// <param name="connectionStringProvider">
    /// Injected by DI. The provider derives its own connection string
    /// from the primary Rock connection string with
    /// <c>Application Name=RockDistributedLock</c> and the configured
    /// max pool size applied.
    /// </param>
    public SqlServerDistributedLockProvider( IConnectionStringProvider connectionStringProvider )
    {
        if ( connectionStringProvider == null )
        {
            throw new ArgumentNullException( nameof( connectionStringProvider ) );
        }

        _connectionString = BuildLockConnectionString( connectionStringProvider.ConnectionString );
    }

    #endregion

    #region IDistributedLockProvider

    /// <inheritdoc/>
    public ILockHandle TryAcquire( Type markerType, string resourceId, TimeSpan timeout )
    {
        var lockKey = ValidateAndBuildKey( markerType, resourceId );
        var heldKeys = EnterFlow( lockKey );

        var sqlLock = CreateSqlLock( heldKeys, lockKey );

        SqlDistributedLockHandle innerHandle;

        try
        {
            innerHandle = sqlLock.TryAcquire( timeout );
        }
        catch ( Exception ex )
        {
            // Infrastructure failure: pool exhausted, tier limit hit,
            // connection could not open, or SQL returned an error the
            // library surfaced as an exception. Per the spec these are
            // logged at Warning and surfaced as "not acquired" rather
            // than propagated, so a single burst does not take down every
            // locked code path.
            RemoveHeldKey( heldKeys, lockKey );
            Logger.LogError( ex, "Distributed lock acquisition failed for key {lockKey} due to an infrastructure error. Reporting as not acquired.", lockKey );
            return UnacquiredLockHandle.Instance;
        }

        if ( innerHandle == null )
        {
            // Contention loss: another node holds the lock or the wait
            // expired. Deliberately silent — every scheduled fire in a
            // multi-node farm produces one of these per losing node, and
            // logging would flood.
            RemoveHeldKey( heldKeys, lockKey );
            return UnacquiredLockHandle.Instance;
        }

        return new SqlServerLockHandle( innerHandle, heldKeys, lockKey );
    }

    /// <inheritdoc/>
    public async Task<ILockHandle> TryAcquireAsync( Type markerType, string resourceId, TimeSpan timeout, CancellationToken cancellationToken = default )
    {
        var lockKey = ValidateAndBuildKey( markerType, resourceId );
        var heldKeys = EnterFlow( lockKey );

        var sqlLock = CreateSqlLock( heldKeys, lockKey );

        SqlDistributedLockHandle innerHandle;

        try
        {
            innerHandle = await sqlLock.TryAcquireAsync( timeout, cancellationToken ).ConfigureAwait( false );
        }
        catch ( OperationCanceledException )
        {
            // The caller canceled while we were waiting for the lock.
            // Treat as "not acquired" rather than propagating so callers
            // can uniformly branch on IsAcquired.
            RemoveHeldKey( heldKeys, lockKey );
            return UnacquiredLockHandle.Instance;
        }
        catch ( Exception ex )
        {
            RemoveHeldKey( heldKeys, lockKey );
            Logger.LogError( ex, "Distributed lock acquisition failed for key {lockKey} due to an infrastructure error. Reporting as not acquired.", lockKey );
            return UnacquiredLockHandle.Instance;
        }

        if ( innerHandle == null )
        {
            RemoveHeldKey( heldKeys, lockKey );
            return UnacquiredLockHandle.Instance;
        }

        return new SqlServerLockHandle( innerHandle, heldKeys, lockKey );
    }

    #endregion

    #region Internal Helpers

    /// <summary>
    /// Validates the caller's inputs and builds the full
    /// <c>sp_getapplock</c> resource name. Shared with the no-op provider
    /// so validation is enforced identically whether the kill switch is
    /// on or off.
    /// </summary>
    /// <param name="markerType">The marker type.</param>
    /// <param name="resourceId">The resource identifier.</param>
    /// <returns>The full lock key.</returns>
    /// <exception cref="ArgumentException">Thrown when the inputs are invalid.</exception>
    internal static string ValidateAndBuildKey( Type markerType, string resourceId )
    {
        if ( markerType == null )
        {
            throw new ArgumentNullException( nameof( markerType ) );
        }

        if ( markerType.IsGenericType )
        {
            // Constructed and open generics both have FullName values that
            // embed the type argument's assembly-qualified name, including
            // Version= and PublicKeyToken=. That would drift across Rock
            // builds and can push past the 255-char limit — silently
            // breaking coordination during rolling upgrades. Reject at the
            // boundary so misuse is loud.
            throw new ArgumentException( $"Marker type '{markerType.Name}' is generic; distributed lock markers must be non-generic types.", nameof( markerType ) );
        }

        if ( string.IsNullOrEmpty( resourceId ) )
        {
            throw new ArgumentException( "Resource id cannot be null or empty.", nameof( resourceId ) );
        }

        // Printable ASCII only, no whitespace. Keep the character set
        // narrow so keys are portable across backends and readable in
        // sys.dm_tran_locks.resource_description.
        for ( int i = 0; i < resourceId.Length; i++ )
        {
            var c = resourceId[i];
            var isValid =
                ( c >= 'A' && c <= 'Z' ) ||
                ( c >= 'a' && c <= 'z' ) ||
                ( c >= '0' && c <= '9' ) ||
                c == '-' || c == '_' || c == '.' || c == ':';

            if ( !isValid )
            {
                throw new ArgumentException( $"Resource id contains invalid character '{c}' at position {i}. Only printable ASCII letters, digits, hyphen, underscore, period, and colon are allowed.", nameof( resourceId ) );
            }
        }

        // FullName is deliberately chosen over AssemblyQualifiedName
        // because AQN embeds Version= and PublicKeyToken= that change
        // across builds. FullName for a non-generic type is a stable
        // "Namespace.Name" (or "Namespace.Outer+Inner" for nested types).
        var typeName = markerType.FullName;

        if ( string.IsNullOrEmpty( typeName ) )
        {
            // Extremely unusual: reflection can produce types with null
            // FullName (open generic parameters, etc.). Reject rather
            // than fabricate a key.
            throw new ArgumentException( $"Marker type '{markerType.Name}' has no FullName and cannot be used as a lock marker.", nameof( markerType ) );
        }

        var key = typeName + ":" + resourceId;

        if ( key.Length > MaxLockKeyLength )
        {
            throw new ArgumentException( $"Lock key '{key}' is {key.Length} characters; distributed lock keys must be {MaxLockKeyLength} characters or fewer.", nameof( resourceId ) );
        }

        return key;
    }

    /// <summary>
    /// Records that the current flow is attempting to acquire
    /// <paramref name="lockKey"/>. Throws
    /// <see cref="DistributedLockReentrancyException"/> if the same flow
    /// already holds it. Returns the AsyncLocal set the caller must
    /// update on release.
    /// </summary>
    private HashSet<string> EnterFlow( string lockKey )
    {
        var heldKeys = _heldKeys.Value;

        if ( heldKeys == null )
        {
            heldKeys = new HashSet<string>( StringComparer.Ordinal );
            _heldKeys.Value = heldKeys;
        }

        lock ( heldKeys )
        {
            if ( heldKeys.Contains( lockKey ) )
            {
                throw new DistributedLockReentrancyException( lockKey );
            }

            heldKeys.Add( lockKey );
        }

        return heldKeys;
    }

    /// <summary>
    /// Constructs the underlying <see cref="SqlDistributedLock"/> for
    /// <paramref name="lockKey"/>, unwinding the flow's held-keys entry
    /// and logging at Error if construction throws. Construction should
    /// never touch SQL Server, so any exception here indicates a
    /// programmer error (bad connection string, invalid arguments,
    /// etc.). Extracted for reuse between the sync and async acquire
    /// paths; marked <see cref="ExcludeFromCodeCoverageAttribute"/>
    /// because the library does not throw during construction under
    /// any input we can craft in tests.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private SqlDistributedLock CreateSqlLock( HashSet<string> heldKeys, string lockKey )
    {
        try
        {
            return new SqlDistributedLock( lockKey, _connectionString, exactName: true );
        }
        catch ( Exception ex )
        {
            RemoveHeldKey( heldKeys, lockKey );
            Logger.LogError( ex, "Failed to construct SqlDistributedLock for key {lockKey}.", lockKey );
            throw;
        }
    }

    /// <summary>
    /// Removes <paramref name="lockKey"/> from the held-keys set. Called
    /// on release, on infrastructure failure, and on cancellation so the
    /// same flow can re-acquire the lock on a subsequent attempt. Every
    /// caller passes a non-null set (either from <see cref="EnterFlow"/>
    /// or from the <see cref="SqlServerLockHandle._heldKeys"/> field
    /// stamped at construction), so no null-guard is needed here.
    /// </summary>
    private static void RemoveHeldKey( HashSet<string> heldKeys, string lockKey )
    {
        lock ( heldKeys )
        {
            heldKeys.Remove( lockKey );
        }
    }

    /// <summary>
    /// Builds the connection string used for lock-holding connections
    /// from Rock's primary connection string. The <c>Application Name</c>
    /// override yields a dedicated <c>SqlConnection</c> pool distinct
    /// from Rock's default EF6 pool. Max pool size is read from the
    /// <c>RockDistributedLockMaxPoolSize</c> app setting when present.
    /// </summary>
    /// <param name="baseConnectionString">Rock's primary connection string.</param>
    /// <returns>The lock-pool connection string.</returns>
    private static string BuildLockConnectionString( string baseConnectionString )
    {
        if ( string.IsNullOrEmpty( baseConnectionString ) )
        {
            throw new InvalidOperationException( "Cannot build the distributed-lock connection string because Rock's primary connection string is not configured." );
        }

        var builder = new SqlConnectionStringBuilder( baseConnectionString )
        {
            ApplicationName = ApplicationName,
            MaxPoolSize = ReadConfiguredMaxPoolSize(),

            // Preserves behavioral parity with Rock's EF6 workload, which
            // uses System.Data.SqlClient and trusts the server certificate
            // by default. Microsoft.Data.SqlClient reversed that default in
            // v4.0, so without this override the lock pool would fail to
            // connect against every SQL Server whose cert isn't in the
            // client trust chain (localhost dev SQL, on-prem with self-
            // signed certs, etc.) while EF6 succeeds. Revisit when Rock
            // itself migrates to Microsoft.Data.SqlClient.
            TrustServerCertificate = true,
        };

        return builder.ConnectionString;
    }

    /// <summary>
    /// Reads the pool size from web.config, falling back to
    /// <see cref="DefaultMaxPoolSize"/> when the setting is missing or
    /// malformed. <see cref="StringExtensions.AsIntegerOrNull(string)"/>
    /// already handles null / empty / whitespace / non-numeric input by
    /// returning null, so the caller-side branching collapses to a
    /// single null-coalesce. Non-positive parsed values propagate to
    /// <see cref="SqlConnectionStringBuilder.MaxPoolSize"/>, which
    /// rejects them at connection-string build time; that's a louder
    /// signal than silently swallowing the operator's mistake.
    /// </summary>
    private static int ReadConfiguredMaxPoolSize()
    {
        return ConfigurationManager.AppSettings[MaxPoolSizeSettingKey].AsIntegerOrNull() ?? DefaultMaxPoolSize;
    }

    #endregion

    #region Handle Implementations

    /// <summary>
    /// Singleton handle returned for every unacquired outcome (contention
    /// loss, infrastructure failure, cancellation). No state to release
    /// so all callers share one instance.
    /// </summary>
    private sealed class UnacquiredLockHandle : ILockHandle
    {
        public static readonly UnacquiredLockHandle Instance = new UnacquiredLockHandle();

        public bool IsAcquired => false;

        public CancellationToken LostToken => CancellationToken.None;

        public void Dispose()
        {
            // Nothing to release.
        }
    }

    /// <summary>
    /// Wraps the <c>DistributedLock.SqlServer</c> library's handle so we
    /// can surface <see cref="ILockHandle"/>, expose the library's
    /// lost-token, and clear the reentrancy-tracking set on dispose.
    /// </summary>
    private sealed class SqlServerLockHandle : ILockHandle
    {
        private readonly SqlDistributedLockHandle _innerHandle;
        private readonly HashSet<string> _heldKeys;
        private readonly string _lockKey;
        private int _disposed;

        public SqlServerLockHandle( SqlDistributedLockHandle innerHandle, HashSet<string> heldKeys, string lockKey )
        {
            _innerHandle = innerHandle;
            _heldKeys = heldKeys;
            _lockKey = lockKey;
        }

        public bool IsAcquired => true;

        public CancellationToken LostToken => _innerHandle.HandleLostToken;

        public void Dispose()
        {
            // Guard against multiple Dispose calls: releasing the inner
            // handle twice would surface as a benign SqlException the
            // second time, but clearing the held-key set twice is
            // unnecessary work.
            if ( Interlocked.Exchange( ref _disposed, 1 ) != 0 )
            {
                return;
            }

            try
            {
                _innerHandle.Dispose();
            }
            finally
            {
                RemoveHeldKey( _heldKeys, _lockKey );
            }
        }
    }

    #endregion
}
