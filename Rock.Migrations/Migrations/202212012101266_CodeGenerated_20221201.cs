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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class CodeGenerated_20221201 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Block 
            //  Block Name: Membership
            //  Page Name: Extended Attributes V1
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "5BD28C0C-DBE0-4E01-844C-C49A12F21E7A".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"D70A59DC-16BE-43BE-9880-59598FA7A94C".AsGuid(), "Membership","SectionB1",@"",@"",0,"260CCD16-E664-4F92-BE01-28AB6BBFEE78"); 

            // Attribute for BlockType
            //   BlockType: Communication List Subscribe
            //   Category: Communication
            //   Attribute: Filter Groups By Campus Context
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "52E0AA5B-B08B-42E4-8180-DD7925BAA57F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Filter Groups By Campus Context", "FilterGroupsByCampusContext", "Filter Groups By Campus Context", @"When enabled, will filter the listed Communication Lists by the campus context of the page. Groups with no campus will always be shown.", 4, @"False", "9E7A7B99-3857-40FA-BDF6-EDF469319D30" );

            // Attribute for BlockType
            //   BlockType: Communication List Subscribe
            //   Category: Communication
            //   Attribute: Always Include Subscribed Lists
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "52E0AA5B-B08B-42E4-8180-DD7925BAA57F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Always Include Subscribed Lists", "AlwaysIncludeSubscribedLists", "Always Include Subscribed Lists", @"When filtering is enabled this setting will include lists that the person is subscribed to even if they don't match the current campus context. (note this would still filter by the category though, so lists not in the configured category would not show even if subscribed to them)", 5, @"True", "82E07AB3-79CE-4C98-AEB8-0E8E46FC08CF" );

            // Attribute for BlockType
            //   BlockType: Connection Opportunity List
            //   Category: Mobile > Connection
            //   Attribute: Include Inactive
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "0015A574-C10A-4530-897C-F7B7C3D9393E", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include Inactive", "IncludeInactive", "Include Inactive", @"Whether or not to filter out inactive opportunities.", 3, @"False", "0B2E22FF-9CA9-49E7-B68D-76B25937DF07" );

            // Attribute for BlockType
            //   BlockType: Communication List Subscribe
            //   Category: Mobile > Communication
            //   Attribute: Filter Groups By Campus Context
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D0C51784-71ED-46F3-86AB-972148B78BE8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Filter Groups By Campus Context", "FilterGroupsByCampusContext", "Filter Groups By Campus Context", @"When enabled will filter the listed Communication Lists by the campus context of the page. Groups with no campus will always be shown.", 4, @"False", "C87A2BF6-6005-43A7-AC9B-F9D4D6C792F7" );

            // Attribute for BlockType
            //   BlockType: Communication List Subscribe
            //   Category: Mobile > Communication
            //   Attribute: Always Include Subscribed Lists
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "D0C51784-71ED-46F3-86AB-972148B78BE8", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Always Include Subscribed Lists", "AlwaysIncludeSubscribedLists", "Always Include Subscribed Lists", @"When filtering is enabled this setting will include lists that the person is subscribed to even if they don't match the current campus context. (note this would still filter by the category though, so lists not in the configured category would not show even if subscribed to them)", 5, @"True", "256D22FB-C2A6-4B05-A339-F8BB9BB90370" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Entry
            //   Category: Mobile > Groups
            //   Attribute: Show Pending Members
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "08AE409C-9E4C-42D1-A93C-A554A3EEA0C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Pending Members", "ShowPendingMembers", "Show Pending Members", @"Shows pending members of the group.", 7, @"False", "4E33108B-699D-4A8B-803B-31D943B2610E" );

            // Attribute for BlockType
            //   BlockType: Group Attendance Entry
            //   Category: Mobile > Groups
            //   Attribute: Show Inactive Members
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "08AE409C-9E4C-42D1-A93C-A554A3EEA0C3", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Inactive Members", "ShowInactiveMembers", "Show Inactive Members", @"Shows inactive members of the group.", 8, @"False", "8CD55026-6B8F-4068-9141-80493100571A" );

            // Add Block Attribute Value
            //   Block: Membership
            //   BlockType: Attribute Values
            //   Category: CRM > Person Detail
            //   Block Location: Page=Extended Attributes V1, Site=Rock RMS
            //   Attribute: Category
            /*   Attribute Value: e919e722-f895-44a4-b86d-38db8fba1844 */
            RockMigrationHelper.AddBlockAttributeValue("260CCD16-E664-4F92-BE01-28AB6BBFEE78","EC43CF32-3BDF-4544-8B6A-CE9208DD7C81",@"e919e722-f895-44a4-b86d-38db8fba1844");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attribute for BlockType
            //   BlockType: Group Attendance Entry
            //   Category: Mobile > Groups
            //   Attribute: Show Inactive Members
            RockMigrationHelper.DeleteAttribute("8CD55026-6B8F-4068-9141-80493100571A");

            // Attribute for BlockType
            //   BlockType: Group Attendance Entry
            //   Category: Mobile > Groups
            //   Attribute: Show Pending Members
            RockMigrationHelper.DeleteAttribute("4E33108B-699D-4A8B-803B-31D943B2610E");

            // Attribute for BlockType
            //   BlockType: Communication List Subscribe
            //   Category: Mobile > Communication
            //   Attribute: Always Include Subscribed Lists
            RockMigrationHelper.DeleteAttribute("256D22FB-C2A6-4B05-A339-F8BB9BB90370");

            // Attribute for BlockType
            //   BlockType: Communication List Subscribe
            //   Category: Mobile > Communication
            //   Attribute: Filter Groups By Campus Context
            RockMigrationHelper.DeleteAttribute("C87A2BF6-6005-43A7-AC9B-F9D4D6C792F7");

            // Attribute for BlockType
            //   BlockType: Connection Opportunity List
            //   Category: Mobile > Connection
            //   Attribute: Include Inactive
            RockMigrationHelper.DeleteAttribute("0B2E22FF-9CA9-49E7-B68D-76B25937DF07");

            // Attribute for BlockType
            //   BlockType: Communication List Subscribe
            //   Category: Communication
            //   Attribute: Always Include Subscribed Lists
            RockMigrationHelper.DeleteAttribute("82E07AB3-79CE-4C98-AEB8-0E8E46FC08CF");

            // Attribute for BlockType
            //   BlockType: Communication List Subscribe
            //   Category: Communication
            //   Attribute: Filter Groups By Campus Context
            RockMigrationHelper.DeleteAttribute("9E7A7B99-3857-40FA-BDF6-EDF469319D30");

            // Remove Block
            //  Name: Membership, from Page: Extended Attributes V1, Site: Rock RMS
            //  from Page: Extended Attributes V1, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("260CCD16-E664-4F92-BE01-28AB6BBFEE78");
        }
    }
}
