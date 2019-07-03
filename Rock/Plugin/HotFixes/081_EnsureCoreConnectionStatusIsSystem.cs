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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    ///Migration 
    /// </summary>
    [MigrationNumber( 81, "1.9.0" )]
    public class EnsureCoreConnectionStatusIsSystem : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
UPDATE [DefinedValue]
SET [IsSystem] = 1
WHERE [Guid] IN ('39F491C5-D6AC-4A9B-8AC0-C431CB17D588', '41540783-D9EF-4C70-8F1D-C9E83D91ED5F', '8EBC0CEB-474D-4C1B-A6BA-734C3A9AB061', 'B91BA046-BC1E-400C-B85D-638C1F4E0CE2', '368DD475-242C-49C4-A42C-7278BE690CC2' )
AND [IsSystem] = 0" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // Not yet used by hotfix migrations.
        }
    }
}
