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
    [MigrationNumber( 73, "1.8.8" )]
    public class MigrationRollupsForV8_8 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //RemoveWindowsJobSchedulerServiceValue();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // Not yet used by hotfix migrations.
        }


        /// <summary>
        ///NA: Remove Windows Job Scheduler Service Defined Value
        /// </summary>
        private void RemoveWindowsJobSchedulerServiceValue()
        {
            RockMigrationHelper.DeleteDefinedValue( "98E421D8-0F9D-484B-B2EA-2F6FEE8E785D" );
        }
    }
}
