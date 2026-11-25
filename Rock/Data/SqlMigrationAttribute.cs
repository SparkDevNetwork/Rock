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

namespace Rock.Data;

/// <summary>
/// Identifies a <see cref="Migration"/> and says where it sorts. Pending
/// migrations run in order of <see cref="TargetVersion"/>, then <see cref="Id"/>.
/// </summary>
[AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = false )]
internal sealed class SqlMigrationAttribute : System.Attribute
{
    /// <summary>
    /// The migration ID: a <c>yyyyMMddHHmmss</c> timestamp. A migration
    /// converted from EF6 keeps the first 14 digits of its EF6 ID.
    /// </summary>
    public long Id { get; }

    /// <summary>
    /// The Rock version the migration belongs to, such as <c>21.0</c>.
    /// A sort key only; it does not decide whether the migration runs.
    /// </summary>
    public string TargetVersion { get; }

    /// <summary>
    /// The lowest Rock version that runs the migration, or <c>null</c>
    /// for no minimum. A migration below its minimum stays pending.
    /// </summary>
    public string MinimumRockVersion { get; set; }

    /// <summary>
    /// The <c>Rock.Plugin.HotFixes</c> migration number this migration was
    /// converted from, or <c>0</c> for none. On a database where that HotFix
    /// already ran, the migration is recorded as applied instead of run again.
    /// </summary>
    public int ReplacesHotFixNumber { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlMigrationAttribute"/> class.
    /// </summary>
    /// <param name="id">The <c>yyyyMMddHHmmss</c> migration ID.</param>
    /// <param name="targetVersion">The Rock version the migration belongs to.</param>
    public SqlMigrationAttribute( long id, string targetVersion )
    {
        Id = id;
        TargetVersion = targetVersion;
    }
}
