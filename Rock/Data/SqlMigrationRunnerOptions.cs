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
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Rock.Data;

/// <summary>
/// What <see cref="SqlMigrationRunner"/> needs from its host. The
/// defaults run nothing, so a host states each choice.
/// </summary>
internal sealed class SqlMigrationRunnerOptions
{
    /// <summary>
    /// The connection string of the Rock database.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// The running Rock version. Recorded as each migration's
    /// <c>AppliedVersion</c> and compared with minimum versions.
    /// </summary>
    public Version RockVersion { get; set; }

    /// <summary>
    /// The assemblies searched for <see cref="Migration"/> types.
    /// </summary>
    public IEnumerable<Assembly> Assemblies { get; set; } = Enumerable.Empty<Assembly>();

    /// <summary>
    /// When set, these migrations are used instead of searching
    /// <see cref="Assemblies"/>. Lets tests supply exact sets.
    /// </summary>
    public IEnumerable<SqlMigrationDescriptor> Migrations { get; set; }

    /// <summary>
    /// Receives progress and lock messages, and SQL Server's informational
    /// messages prefixed with <c>WARNING:</c>. Nothing is sent when no
    /// migration is pending. On WebForms this writes <c>MigrationLog.csv</c>.
    /// </summary>
    public Action<string> Log { get; set; } = _ => { };

    /// <summary>
    /// Whether a missing database is created. Only true for a development
    /// environment; elsewhere a missing database fails startup.
    /// </summary>
    public bool IsDevelopmentEnvironment { get; set; }

    /// <summary>
    /// How long to wait for another node's migration lock before failing startup.
    /// </summary>
    public TimeSpan LockTimeout { get; set; } = TimeSpan.FromSeconds( 60 );
}

/// <summary>
/// A discovered migration: its identity from <see cref="SqlMigrationAttribute"/>
/// and how to create it.
/// </summary>
internal sealed class SqlMigrationDescriptor
{
    /// <summary>
    /// The migration ID.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// The target version as declared.
    /// </summary>
    public string TargetVersion { get; set; }

    /// <summary>
    /// The lowest Rock version that runs the migration, or <c>null</c>.
    /// </summary>
    public Version MinimumRockVersion { get; set; }

    /// <summary>
    /// The HotFix migration number this migration replaces, or <c>null</c>.
    /// </summary>
    public int? ReplacesHotFixNumber { get; set; }

    /// <summary>
    /// The class name recorded in the history table.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The full class name used in error messages.
    /// </summary>
    public string FullName { get; set; }

    /// <summary>
    /// Creates an instance of the migration.
    /// </summary>
    public Func<Migration> Create { get; set; }

    /// <summary>
    /// Builds a descriptor from a migration class and its attribute.
    /// </summary>
    /// <param name="type">A non-abstract <see cref="Migration"/> type.</param>
    /// <returns>The descriptor.</returns>
    /// <exception cref="SqlMigrationException">Thrown when the attribute is missing or invalid.</exception>
    public static SqlMigrationDescriptor FromType( Type type )
    {
        var attribute = type.GetCustomAttribute<SqlMigrationAttribute>()
            ?? throw new SqlMigrationException( $"Migration {type.FullName} has no SqlMigration attribute." );

        if ( !Version.TryParse( attribute.TargetVersion, out _ ) )
        {
            throw new SqlMigrationException( $"Migration {type.FullName} has an invalid target version '{attribute.TargetVersion}'." );
        }

        Version minimumRockVersion = null;

        if ( attribute.MinimumRockVersion != null && !Version.TryParse( attribute.MinimumRockVersion, out minimumRockVersion ) )
        {
            throw new SqlMigrationException( $"Migration {type.FullName} has an invalid minimum Rock version '{attribute.MinimumRockVersion}'." );
        }

        return new SqlMigrationDescriptor
        {
            Id = attribute.Id,
            TargetVersion = attribute.TargetVersion,
            MinimumRockVersion = minimumRockVersion,
            ReplacesHotFixNumber = attribute.ReplacesHotFixNumber > 0 ? attribute.ReplacesHotFixNumber : ( int? ) null,
            Name = type.Name,
            FullName = type.FullName,
            Create = () => CreateInstance( type )
        };
    }

    /// <summary>
    /// Creates an instance of a migration type. An exception from its
    /// constructor or field initializers is rethrown as it is, not wrapped
    /// in a <see cref="TargetInvocationException"/>, so the runner's error
    /// and the migration log show the real reason.
    /// </summary>
    /// <param name="type">A non-abstract <see cref="Migration"/> type.</param>
    /// <returns>The migration.</returns>
    private static Migration CreateInstance( Type type )
    {
        try
        {
            return ( Migration ) Activator.CreateInstance( type, nonPublic: true );
        }
        catch ( TargetInvocationException ex ) when ( ex.InnerException != null )
        {
            ExceptionDispatchInfo.Capture( ex.InnerException ).Throw();
            throw;
        }
    }
}

/// <summary>
/// What a run of <see cref="SqlMigrationRunner"/> did.
/// </summary>
internal sealed class SqlMigrationRunResult
{
    /// <summary>
    /// The number of <c>__MigrationHistory</c> rows copied into the history table.
    /// </summary>
    public int ImportedCount { get; set; }

    /// <summary>
    /// The number of migrations recorded as applied because the HotFix they replace already ran.
    /// </summary>
    public int ImportedHotFixCount { get; set; }

    /// <summary>
    /// Whether the baseline ran, building a new database.
    /// </summary>
    public bool IsBaselineRun { get; set; }

    /// <summary>
    /// Whether a baseline row was added without running the baseline, for a database whose history lacked one.
    /// </summary>
    public bool IsBaselineRecorded { get; set; }

    /// <summary>
    /// The migrations applied in this run, baseline included, in the order they ran.
    /// </summary>
    public List<string> AppliedMigrationNames { get; } = new List<string>();
}
