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
using Rock.Web.Cache;

namespace com.centralaz.LifeGroupFinder.Migrations
{
    [MigrationNumber( 7, "1.3.4" )]
    public class PendingGroupMembers : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
                        // Page: Life Groups
            RockMigrationHelper.AddPage("B0F4B33D-DD11-4CCC-B79D-9342831B8701","D65F783D-87A9-4CC9-8110-E83466A0EADB","Life Groups","","223AB1FA-2BBD-43AF-A1EF-B48816B24880",""); // Site:Rock RMS
            RockMigrationHelper.AddBlock("223AB1FA-2BBD-43AF-A1EF-B48816B24880","","CACB9D1A-A820-4587-986A-D66A69EE9948","Page Menu","Main","","",0,"6CC7C9AA-0147-4E76-9DC8-4400D730BA2A"); 
            RockMigrationHelper.AddBlockAttributeValue("6CC7C9AA-0147-4E76-9DC8-4400D730BA2A","7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22",@""); // CSS File
            RockMigrationHelper.AddBlockAttributeValue("6CC7C9AA-0147-4E76-9DC8-4400D730BA2A","EEE71DDE-C6BC-489B-BAA5-1753E322F183",@"False"); // Include Current Parameters
            RockMigrationHelper.AddBlockAttributeValue("6CC7C9AA-0147-4E76-9DC8-4400D730BA2A","1322186A-862A-4CF1-B349-28ECB67229BA",@"{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}"); // Template
            RockMigrationHelper.AddBlockAttributeValue("6CC7C9AA-0147-4E76-9DC8-4400D730BA2A","41F1C42E-2395-4063-BD4F-031DF8D5B231",@"223ab1fa-2bbd-43af-a1ef-b48816b24880"); // Root Page
            RockMigrationHelper.AddBlockAttributeValue("6CC7C9AA-0147-4E76-9DC8-4400D730BA2A","6C952052-BC79-41BA-8B88-AB8EA3E99648",@"1"); // Number of Levels
            RockMigrationHelper.AddBlockAttributeValue("6CC7C9AA-0147-4E76-9DC8-4400D730BA2A","E4CF237D-1D12-4C93-AFD7-78EB296C4B69",@"False"); // Include Current QueryString
            RockMigrationHelper.AddBlockAttributeValue("6CC7C9AA-0147-4E76-9DC8-4400D730BA2A","2EF904CD-976E-4489-8C18-9BA43885ACD9",@"False"); // Enable Debug
            RockMigrationHelper.AddBlockAttributeValue("6CC7C9AA-0147-4E76-9DC8-4400D730BA2A","C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2",@"False"); // Is Secondary Block
            RockMigrationHelper.AddBlockAttributeValue("6CC7C9AA-0147-4E76-9DC8-4400D730BA2A","0A49DABE-42EE-40E5-9E06-0E6530944865",@""); // Include Page List
            RockMigrationHelper.AddBlockAttributeValue("6CC7C9AA-0147-4E76-9DC8-4400D730BA2A","216AFE54-82CF-400C-83C2-83C6B2723C49",@"Jan 31 2011 12:00AM"); // First Activity

            // Page: Pending Group Members
            RockMigrationHelper.AddPage( "223AB1FA-2BBD-43AF-A1EF-B48816B24880", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Pending Group Members", "", "1069486E-3221-4536-93AE-8429C084914A", "fa fa-group" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Pending Group Member List", "Lists all the pending members of a group type", "~/Plugins/com_centralaz/LifeGroupFinder/PendingGroupMemberList.ascx", "com_centralaz > Groups", "9CB552A7-4DDB-4DB9-A6F4-6FD326ED384D" );
            RockMigrationHelper.AddBlock( "1069486E-3221-4536-93AE-8429C084914A", "", "9CB552A7-4DDB-4DB9-A6F4-6FD326ED384D", "Pending Group Member List", "Main", "", "", 0, "BE689C2C-9799-4214-9D04-6B61631D1978" );

            RockMigrationHelper.AddBlockTypeAttribute( "9CB552A7-4DDB-4DB9-A6F4-6FD326ED384D", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Group Type", "GroupType", "", "Pending members from groups of this grouptype will be displayed", 0, @"", "2EE9B769-C9D8-450A-9B33-AD153C041139" );

            RockMigrationHelper.AddBlockTypeAttribute( "9CB552A7-4DDB-4DB9-A6F4-6FD326ED384D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "", "", 0, @"", "218C5FC4-0B0C-4F4F-9F5C-981A50427622" );

            RockMigrationHelper.AddBlockTypeAttribute( "9CB552A7-4DDB-4DB9-A6F4-6FD326ED384D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Member Detail Page", "GroupMemberDetailPage", "", "", 0, @"", "E1A13B50-2EC7-4DAF-AAE7-1C96DCD62972" );

            RockMigrationHelper.AddBlockTypeAttribute( "9CB552A7-4DDB-4DB9-A6F4-6FD326ED384D", "F4399CEF-827B-48B2-A735-F7806FCFE8E8", "Root Group", "RootGroup", "", "Pending members from groups under this group will be displayed", 0, @"", "1C80E09A-6984-4C52-BA4A-0500EAEF7A75" );

            RockMigrationHelper.AddBlockAttributeValue( "BE689C2C-9799-4214-9D04-6B61631D1978", "216AFE54-82CF-400C-83C2-83C6B2723C49", @"Jan 31 2011 12:00AM" ); // First Activity

            RockMigrationHelper.AddBlockAttributeValue( "BE689C2C-9799-4214-9D04-6B61631D1978", "2EE9B769-C9D8-450A-9B33-AD153C041139", @"7f76ae15-c5c4-490e-bf3a-50fb0591a60f" ); // Group Type

            RockMigrationHelper.AddBlockAttributeValue( "BE689C2C-9799-4214-9D04-6B61631D1978", "218C5FC4-0B0C-4F4F-9F5C-981A50427622", @"4e237286-b715-4109-a578-c1445ec02707" ); // Group Detail Page

            RockMigrationHelper.AddBlockAttributeValue( "BE689C2C-9799-4214-9D04-6B61631D1978", "E1A13B50-2EC7-4DAF-AAE7-1C96DCD62972", @"3905c63f-4d57-40f0-9721-c60a2f681911" ); // Group Member Detail Page

            RockMigrationHelper.AddBlockAttributeValue( "BE689C2C-9799-4214-9D04-6B61631D1978", "1C80E09A-6984-4C52-BA4A-0500EAEF7A75", @"3b822249-1cdd-4e6f-88b8-d76dc62f8caf" ); // Root Group

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "1C80E09A-6984-4C52-BA4A-0500EAEF7A75" );
            RockMigrationHelper.DeleteAttribute( "E1A13B50-2EC7-4DAF-AAE7-1C96DCD62972" );
            RockMigrationHelper.DeleteAttribute( "218C5FC4-0B0C-4F4F-9F5C-981A50427622" );
            RockMigrationHelper.DeleteAttribute( "2EE9B769-C9D8-450A-9B33-AD153C041139" );
            RockMigrationHelper.DeleteBlock( "BE689C2C-9799-4214-9D04-6B61631D1978" );
            RockMigrationHelper.DeleteBlockType( "9CB552A7-4DDB-4DB9-A6F4-6FD326ED384D" );
            RockMigrationHelper.DeletePage( "1069486E-3221-4536-93AE-8429C084914A" ); //  Page: Pending Group Members

            RockMigrationHelper.DeleteBlock( "6CC7C9AA-0147-4E76-9DC8-4400D730BA2A" );
            RockMigrationHelper.DeletePage( "223AB1FA-2BBD-43AF-A1EF-B48816B24880" ); //  Page: Life Groups
        }
    }
}
