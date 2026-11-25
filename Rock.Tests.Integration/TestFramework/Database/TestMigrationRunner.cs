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

using Rock.Data;
using Rock.Tests.Shared.TestFramework;
using Rock.Web.Cache;

namespace Rock.Tests.Integration.TestFramework.Database;

/*
    11/16/26 - CLAUDE

    Migrations must reach the database only through their own connection
    (Sql(), SqlScalar(), RockMigrationHelper). The runner checks the direct
    paths at run time, but tests run the real migrations, so they also make
    every RockApp connection string unusable while the runner works. Anything
    a migration does through RockApp (a RockContext, DbService, a cache miss,
    raw ADO.NET reading the connection string) then fails instead of quietly
    using whatever database RockApp points at, which in tests is usually a
    different database from the one being migrated. Caches are cleared first,
    so a cached lookup cannot hide such a call.

    Reason: Prove the real migrations never use RockApp's database.
*/
/// <summary>
/// Runs SQL migrations in tests with RockApp's database made unusable.
/// </summary>
internal static class TestMigrationRunner
{
    /// <summary>
    /// Runs the migrations with every RockApp connection string unusable
    /// and the caches cleared.
    /// </summary>
    /// <param name="options">The runner options, including the database to migrate.</param>
    /// <returns>What the run did.</returns>
    internal static SqlMigrationRunResult Run( SqlMigrationRunnerOptions options )
    {
        RockCache.ClearAllCachedItems( false );

        using ( TestHelper.CreateScopedRockAppWithUnusableDatabase() )
        {
            return SqlMigrationRunner.Run( options );
        }
    }

    /// <summary>
    /// Clears the caches and makes every RockApp connection string unusable
    /// once, around code that calls <see cref="SqlMigrationRunner.Run"/>
    /// itself, for example to migrate several databases in parallel.
    /// </summary>
    /// <param name="runMigrations">The code that runs the migrations.</param>
    internal static void RunInOneScope( Action runMigrations )
    {
        /*
            11/17/26 - CLAUDE

            The scope swaps the process-wide RockApp.Current and puts back
            the app it saw when it started. Two overlapping calls to Run
            could end in the wrong order and leave the unusable app in place
            for later tests, or clear the caches while another run works. One
            scope around every parallel run avoids both, and the runner itself
            is safe in parallel: each database has its own connection and
            lock, and MigrationExecutionScope keeps a separate value per
            execution flow.

            Reason: Let tests migrate several databases at once safely.
        */
        RockCache.ClearAllCachedItems( false );

        using ( TestHelper.CreateScopedRockAppWithUnusableDatabase() )
        {
            runMigrations();
        }
    }
}
