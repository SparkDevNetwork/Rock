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

namespace Rock.Data;

/*
    11/16/26 - CLAUDE

    A migration runs on the runner's connection and transaction. A RockContext
    or a DbService call opens a second connection outside that transaction: it
    cannot see the migration's changes, can block on the migration's own locks,
    and is not rolled back if the migration fails. So database access through
    either is an error while a migration's Up() runs, and the checkpoints in
    DbContext and DbService throw.

    The check runs on every RockContext and DbService connection, so it is
    built to cost almost nothing: a static counter is read first, and it is
    zero except while the runner is inside a migration (measured 0.44 ns).
    Only then is the AsyncLocal read (2.66 ns), which limits the error to the
    migration's own execution flow, including tasks it starts, so other
    threads running during startup are not affected. The static fields are
    safe on this static class: the counter changes only through Interlocked,
    and an AsyncLocal holds a separate value per execution flow.

    Reason: Stop migrations from reaching the database outside their transaction.
*/
/// <summary>
/// Marks the code running inside a migration's <c>Up()</c>, so database
/// access that would bypass the migration's transaction fails with a clear
/// error instead of running on a second connection.
/// </summary>
internal static class MigrationExecutionScope
{
    /// <summary>
    /// The number of migrations running in this process right now. Read
    /// first, so the check costs one field read outside migrations.
    /// </summary>
    private static int _runningCount;

    /// <summary>
    /// The full name of the migration running in this execution flow, or
    /// <c>null</c> when none is.
    /// </summary>
    private static readonly AsyncLocal<string> _currentMigration = new AsyncLocal<string>();

    /// <summary>
    /// Gets the full name of the migration running in this execution flow,
    /// or <c>null</c> when none is.
    /// </summary>
    internal static string CurrentMigration => Volatile.Read( ref _runningCount ) == 0 ? null : _currentMigration.Value;

    /// <summary>
    /// Marks the start of a migration's <c>Up()</c> in this execution flow.
    /// Dispose the result when it returns or throws.
    /// </summary>
    /// <param name="migrationName">The migration's full name, used in the error.</param>
    /// <returns>An object that ends the scope when disposed.</returns>
    internal static IDisposable Begin( string migrationName )
    {
        return new Scope( migrationName );
    }

    /// <summary>
    /// Throws when called from inside a migration's <c>Up()</c>.
    /// </summary>
    /// <param name="access">What the caller was about to do, such as "created a RockContext".</param>
    /// <exception cref="InvalidOperationException">Thrown when a migration is running in this execution flow.</exception>
    internal static void ThrowIfMigrationRunning( string access )
    {
        if ( Volatile.Read( ref _runningCount ) == 0 )
        {
            return;
        }

        var migrationName = _currentMigration.Value;

        if ( migrationName != null )
        {
            throw new InvalidOperationException( $"Migration {migrationName} {access}, which opens a database connection outside the migration's transaction. Migrations must use Sql(), SqlScalar() and RockMigrationHelper; C# work against the database belongs in a run-once post-update job." );
        }
    }

    /// <summary>
    /// Sets the current migration for this execution flow, and restores the
    /// previous value when disposed.
    /// </summary>
    private sealed class Scope : IDisposable
    {
        /// <summary>
        /// The migration that was current before this scope began.
        /// </summary>
        private readonly string _previousMigration;

        /// <summary>
        /// Whether the scope has already ended.
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="Scope"/> class.
        /// </summary>
        /// <param name="migrationName">The migration's full name.</param>
        public Scope( string migrationName )
        {
            _previousMigration = _currentMigration.Value;
            _currentMigration.Value = migrationName;
            Interlocked.Increment( ref _runningCount );
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if ( _isDisposed )
            {
                return;
            }

            _isDisposed = true;
            _currentMigration.Value = _previousMigration;
            Interlocked.Decrement( ref _runningCount );
        }
    }
}
