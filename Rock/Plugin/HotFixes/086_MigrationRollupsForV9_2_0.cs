﻿// <copyright>
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
    /// Plugin Migration. The migration number jumps to 83 because 75-82 were moved to EF migrations and deleted.
    /// </summary>
    [MigrationNumber( 86, "1.9.0" )]
    public class MigrationRollupsForV9_2_0 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //UpdateGroupTypeForGroupConnections();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // Not yet used by hotfix migrations.
        }

        /// <summary>
        /// JE: Group Connections Project
        /// </summary>
        private void UpdateGroupTypeForGroupConnections()
        {
            Sql( @"
                UPDATE [GroupType]
                SET [AllowSpecificGroupMemberAttributes] = 1
                WHERE [Guid] = '3981CF6D-7D15-4B57-AACE-C0E25D28BD49'
                    AND [AllowSpecificGroupMemberAttributes] = 0" );
        }

    }
}
