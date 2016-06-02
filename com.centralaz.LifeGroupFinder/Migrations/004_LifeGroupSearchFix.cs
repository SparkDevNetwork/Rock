// <copyright>
// Copyright by Central Christian Church
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
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;

namespace com.centralaz.LifeGroupFinder.Migrations
{
    [MigrationNumber( 4, "1.0.14" )]
    public class LifeGroupSearchFix : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateBlockType( "Life Group Search", "Central custom group search block.", "~/Plugins/com_centralaz/LifeGroupFinder/LifeGroupSearch.ascx", "com_centralaz > Groups", "205531A1-C1BC-494C-911E-EE88D29969FB" );
            RockMigrationHelper.AddBlockTypeAttribute( "205531A1-C1BC-494C-911E-EE88D29969FB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Information Security Page", "InformationSecurityPage", "", "The page describing why your information is safe with us.", 0, @"", "97717892-A27D-4709-81FF-DF834DD3B730" );
            RockMigrationHelper.AddBlockTypeAttribute( "205531A1-C1BC-494C-911E-EE88D29969FB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Life Group List Page", "LifeGroupListPage", "", "The page to navigate to for the group list.", 0, @"", "3EA4AC07-3ADD-467A-A90F-83F8C6135F3B" );
            RockMigrationHelper.AddBlockTypeAttribute( "57D90EE5-8425-448A-82F6-292D35CEAEAE", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "Lava template to use to display the list of events.", 2, @"{% include '~/Plugins/com_centralaz/LifeGroupFinder/Lava/LifeGroupList.lava' %}", "2BD1BE36-829C-4F5B-A4B0-DA58B345D347" );
            RockMigrationHelper.AddBlockTypeAttribute( "57D90EE5-8425-448A-82F6-292D35CEAEAE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Display a list of merge fields available for lava.", 3, @"False", "1586A752-E5F8-4BDA-95E0-F43D6669BC44" );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
