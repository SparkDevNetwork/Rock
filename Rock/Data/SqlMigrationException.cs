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
/// Thrown by <see cref="SqlMigrationRunner"/> when the database cannot be
/// migrated and the host must not start: invalid migrations, a missing
/// database, a lock timeout, a database older than the baseline, or a
/// failed migration. The message is written for the person installing or
/// upgrading Rock.
/// </summary>
internal sealed class SqlMigrationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SqlMigrationException"/> class.
    /// </summary>
    /// <param name="message">What went wrong and what to do about it.</param>
    public SqlMigrationException( string message )
        : base( message )
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlMigrationException"/> class.
    /// </summary>
    /// <param name="message">What went wrong and what to do about it.</param>
    /// <param name="innerException">The error that caused it, such as a failed SQL statement.</param>
    public SqlMigrationException( string message, Exception innerException )
        : base( message, innerException )
    {
    }
}
