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
using Rock.Checkr.Constants;
using Rock.Plugin;
using Rock.SystemGuid;

namespace Rock.Migrations
{
    [MigrationNumber( 10, "1.11.0" )]
    public class Checkr_CustomActionsConfigsAttributes : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// This add "PMM - " to any Protect My Ministry packages that the previous migration missed. The previous migration only updated the build in packages
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: Checkr Request List:core.CustomActionsConfigs
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "53A28B56-B7B4-472C-9305-1DC66693A6C6", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomActionsConfigs", "core.CustomActionsConfigs", "core.CustomActionsConfigs", @"", 0, @"", "675111BA-BE98-45EB-B13E-838544AD9EF2" );
            // Attrib for BlockType: Checkr Request List:core.EnableDefaultWorkflowLauncher
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "53A28B56-B7B4-472C-9305-1DC66693A6C6", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", "core.EnableDefaultWorkflowLauncher", @"", 0, @"True", "42EAC532-5905-446F-B374-67994EDB0634" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}