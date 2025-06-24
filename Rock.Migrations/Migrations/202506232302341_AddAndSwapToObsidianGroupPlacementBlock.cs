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

    using Rock.Migrations.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class AddAndSwapToObsidianGroupPlacementBlock : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddAndSwapToObsidianGroupPlacementBlockUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddAndSwapToObsidianGroupPlacementBlockDown();
        }

        private void AddAndSwapToObsidianGroupPlacementBlockUp()
        {
            #region Page, Block, and Attribute Updates

            // Add Page 
            //  Internal Name: Group Placement
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "4E237286-B715-4109-A578-C1445EC02707", "C2467799-BB45-4251-8EE6-F0BF27201535", "Group Placement", "", "C1B0C21F-FF3B-4D79-A11A-75BF689A954A", "" );

            // Add Page Route
            //   Page:Group Placement
            //   Route:GroupPlacement
            RockMigrationHelper.AddOrUpdatePageRoute( "C1B0C21F-FF3B-4D79-A11A-75BF689A954A", "GroupPlacement", "C00ECC2D-8067-44DE-8947-A87B2CC4D575" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Group.GroupPlacement
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Group.GroupPlacement", "Group Placement", "Rock.Blocks.Group.GroupPlacement, Rock.Blocks, Version=17.1.1.0, Culture=neutral, PublicKeyToken=null", false, false, "0AA9BF5D-D72C-41DB-9719-253CE2500122" );

            // Add/Update Obsidian Block Type
            //   Name:Group Placement
            //   Category:Group
            //   EntityType:Rock.Blocks.Group.GroupPlacement
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Group Placement", "Block to manage group placements", "Rock.Blocks.Group.GroupPlacement", "Group", "5FA6DDB6-2A99-4882-8E49-562781D69ECA" );

            // Add Block 
            //  Block Name: Group Placement
            //  Page Name: Group Placement
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "C1B0C21F-FF3B-4D79-A11A-75BF689A954A".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "5FA6DDB6-2A99-4882-8E49-562781D69ECA".AsGuid(), "Group Placement", "Main", @"", @"", 0, "7561B3C0-C469-400D-8142-5520EC2D822F" );

            // Attribute for BlockType
            //   BlockType: Group Detail
            //   Category: Groups
            //   Attribute: Group Placement Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "582BEEA1-5B27-444D-BC0A-F60CEB053981", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Placement Page", "GroupPlacementPage", "Group Placement Page", @"The page used for performing group placements.", 20, @"", "75947EFF-7526-4049-A1E0-357DFF4DF8C4" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Instance Detail
            //   Category: Event
            //   Attribute: Group Placement Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "22B67EDB-6D13-4D29-B722-DF45367AA3CB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Placement Page", "GroupPlacementPage", "Group Placement Page", @"The page for managing group placements.", 2, @"", "388E6A48-3886-439D-89B9-F229EC5B1A7E" );

            // Attribute for BlockType
            //   BlockType: Registration Instance Registrant List
            //   Category: Event
            //   Attribute: Group Placement Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "4D4FBC7B-068C-499A-8BA4-C9209CA9BB6E", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Placement Page", "GroupPlacementPage", "Group Placement Page", @"The page for managing the registrant's group placements", 2, @"c1b0c21f-ff3b-4d79-a11a-75bf689a954a,c00ecc2d-8067-44de-8947-a87b2cc4d575", "4C3801B0-F518-403D-B9EC-B07D1B44BD92" );

            // Attribute for BlockType
            //   BlockType: Group Placement
            //   Category: Group
            //   Attribute: Group Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5FA6DDB6-2A99-4882-8E49-562781D69ECA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "Group Detail Page", @"", 1, @"4E237286-B715-4109-A578-C1445EC02707", "EDA50FE9-B515-47B0-B300-5597B87900A2" );

            // Attribute for BlockType
            //   BlockType: Group Placement
            //   Category: Group
            //   Attribute: Group Member Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5FA6DDB6-2A99-4882-8E49-562781D69ECA", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Member Detail Page", "GroupMemberDetailPage", "Group Member Detail Page", @"", 2, @"3905C63F-4D57-40F0-9721-C60A2F681911", "2109DD7F-C511-4FCF-A331-04F669E97C04" );

            // Attribute for BlockType
            //   BlockType: Group Placement
            //   Category: Group
            //   Attribute: Registration Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "5FA6DDB6-2A99-4882-8E49-562781D69ECA", "E1EBAEE8-AF7E-426D-9A1B-02CBD785E620", "Registration Template", "RegistrationTemplate", "Registration Template", @"If provided, this Registration Template will override any Registration Template specified in a URL parameter.", 0, @"", "0CF6FECA-9C2B-45F2-A8B5-87668AE7112E" );

            // Add Block Attribute Value
            //   Block: GroupDetailRight
            //   BlockType: Group Detail
            //   Category: Groups
            //   Block Location: Page=Group Viewer, Site=Rock RMS
            //   Attribute: Group Placement Page
            /*   Attribute Value: c1b0c21f-ff3b-4d79-a11a-75bf689a954a,c00ecc2d-8067-44de-8947-a87b2cc4d575 */
            RockMigrationHelper.AddBlockAttributeValue( "88344FE3-737E-4741-A38D-D2D3A1653818", "75947EFF-7526-4049-A1E0-357DFF4DF8C4", @"c1b0c21f-ff3b-4d79-a11a-75bf689a954a,c00ecc2d-8067-44de-8947-a87b2cc4d575" );

            // Add Block Attribute Value
            //   Block: Registration Instance Detail
            //   BlockType: Registration Instance - Instance Detail
            //   Category: Event
            //   Block Location: Page=Registration Instance - Registrations, Site=Rock RMS
            //   Attribute: Group Placement Page
            /*   Attribute Value: c1b0c21f-ff3b-4d79-a11a-75bf689a954a,c00ecc2d-8067-44de-8947-a87b2cc4d575 */
            RockMigrationHelper.AddBlockAttributeValue( "5F44A3A8-500B-4C89-95CA-8C4246B53C3F", "388E6A48-3886-439D-89B9-F229EC5B1A7E", @"c1b0c21f-ff3b-4d79-a11a-75bf689a954a,c00ecc2d-8067-44de-8947-a87b2cc4d575" );

            // Add Block Attribute Value
            //   Block: Registration Instance Detail
            //   BlockType: Registration Instance - Instance Detail
            //   Category: Event
            //   Block Location: Page=Registration Instance - Registrants, Site=Rock RMS
            //   Attribute: Group Placement Page
            /*   Attribute Value: c1b0c21f-ff3b-4d79-a11a-75bf689a954a,c00ecc2d-8067-44de-8947-a87b2cc4d575 */
            RockMigrationHelper.AddBlockAttributeValue( "4C06654E-F95F-4115-ACE6-7047BFBE1F7B", "388E6A48-3886-439D-89B9-F229EC5B1A7E", @"c1b0c21f-ff3b-4d79-a11a-75bf689a954a,c00ecc2d-8067-44de-8947-a87b2cc4d575" );

            // Add Block Attribute Value
            //   Block: Registration Instance Detail
            //   BlockType: Registration Instance - Instance Detail
            //   Category: Event
            //   Block Location: Page=Registration Instance - Payments, Site=Rock RMS
            //   Attribute: Group Placement Page
            /*   Attribute Value: c1b0c21f-ff3b-4d79-a11a-75bf689a954a,c00ecc2d-8067-44de-8947-a87b2cc4d575 */
            RockMigrationHelper.AddBlockAttributeValue( "9501DE68-C7E7-4BBC-AC4E-B00783BD85E7", "388E6A48-3886-439D-89B9-F229EC5B1A7E", @"c1b0c21f-ff3b-4d79-a11a-75bf689a954a,c00ecc2d-8067-44de-8947-a87b2cc4d575" );

            // Add Block Attribute Value
            //   Block: Registration Instance - Instance Detail
            //   BlockType: Registration Instance - Instance Detail
            //   Category: Event
            //   Block Location: Page=Registration Instance - Fees, Site=Rock RMS
            //   Attribute: Group Placement Page
            /*   Attribute Value: c1b0c21f-ff3b-4d79-a11a-75bf689a954a,c00ecc2d-8067-44de-8947-a87b2cc4d575 */
            RockMigrationHelper.AddBlockAttributeValue( "884075FE-0A7C-4D39-86EE-120B8A63CBB1", "388E6A48-3886-439D-89B9-F229EC5B1A7E", @"c1b0c21f-ff3b-4d79-a11a-75bf689a954a,c00ecc2d-8067-44de-8947-a87b2cc4d575" );

            // Add Block Attribute Value
            //   Block: Registration Instance Detail
            //   BlockType: Registration Instance - Instance Detail
            //   Category: Event
            //   Block Location: Page=Registration Instance - Discounts, Site=Rock RMS
            //   Attribute: Group Placement Page
            /*   Attribute Value: c1b0c21f-ff3b-4d79-a11a-75bf689a954a,c00ecc2d-8067-44de-8947-a87b2cc4d575 */
            RockMigrationHelper.AddBlockAttributeValue( "C8D19431-76DA-4515-B2F9-14B27F00A89D", "388E6A48-3886-439D-89B9-F229EC5B1A7E", @"c1b0c21f-ff3b-4d79-a11a-75bf689a954a,c00ecc2d-8067-44de-8947-a87b2cc4d575" );

            // Add Block Attribute Value
            //   Block: Registration Instance Detail
            //   BlockType: Registration Instance - Instance Detail
            //   Category: Event
            //   Block Location: Page=Registration Instance - Linkages, Site=Rock RMS
            //   Attribute: Group Placement Page
            /*   Attribute Value: c1b0c21f-ff3b-4d79-a11a-75bf689a954a,c00ecc2d-8067-44de-8947-a87b2cc4d575 */
            RockMigrationHelper.AddBlockAttributeValue( "CBCF0B24-D667-4919-BB63-DD2914EA4B65", "388E6A48-3886-439D-89B9-F229EC5B1A7E", @"c1b0c21f-ff3b-4d79-a11a-75bf689a954a,c00ecc2d-8067-44de-8947-a87b2cc4d575" );

            // Attribute for BlockType
            //   BlockType: Registration Template Detail
            //   Category: Event
            //   Attribute: Registration Template Placement Page
            RockMigrationHelper.DeleteAttribute( "2EC8E93C-45DA-480A-B87B-2DADAFDBAD00" );

            // Attribute for BlockType
            //   BlockType: Registration Template
            //   Category: Event
            //   Attribute: Group Placement Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "91354899-304E-44C7-BD0D-55F42E6505D3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Placement Page", "GroupPlacementPage", "Group Placement Page", @"The page used for performing group placements.", 0, @"c1b0c21f-ff3b-4d79-a11a-75bf689a954a,c00ecc2d-8067-44de-8947-a87b2cc4d575", "E2954A61-6560-4AF7-8836-5E9962EDA0B5" );

            // Add Block Attribute Value
            //   Block: Registration Template Detail
            //   BlockType: Registration Template Detail
            //   Category: Event
            //   Block Location: Page=Event Registration, Site=Rock RMS
            //   Attribute: Group Placement Page
            /*   Attribute Value: c1b0c21f-ff3b-4d79-a11a-75bf689a954a,c00ecc2d-8067-44de-8947-a87b2cc4d575 */
            RockMigrationHelper.AddBlockAttributeValue( "D6372D00-9FA3-49BF-B0F2-0BE67B5F5D39", "E2954A61-6560-4AF7-8836-5E9962EDA0B5", @"c1b0c21f-ff3b-4d79-a11a-75bf689a954a,c00ecc2d-8067-44de-8947-a87b2cc4d575" );

            // Delete Page 
            //  Internal Name: Registration Instance - Placement Groups
            //  Site: Rock RMS
            //  Layout: Full Worksurface
            RockMigrationHelper.DeletePage( "0CD950D7-033D-42B1-A53E-108F311DC5BF" );

            // Delete Page 
            //  Internal Name: Registration Template Placement
            //  Site: Rock RMS
            //  Layout: Full Worksurface
            RockMigrationHelper.DeletePage( "02E7D8EC-E0F1-4632-9641-77772144A4CA" );

            #endregion

            #region Add GroupPlacement Stored Procedures

            // Ensure these settings are set as expected so they persist with the stored procedure.
            // But first, read their current values to restore them after the migration.
            var isAnsiNullsOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('ANSI_NULLS') = 1 THEN 1 ELSE 0 END;" ) );
            var isQuotedIdentifierOn = Convert.ToBoolean( SqlScalar( "SELECT CASE WHEN SESSIONPROPERTY('QUOTED_IDENTIFIER') = 1 THEN 1 ELSE 0 END;" ) );

            Sql( "SET ANSI_NULLS ON;" );
            Sql( "SET QUOTED_IDENTIFIER ON;" );

            // Add [spGetGroupPlacementPeople] (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spGetGroupPlacementPeople]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spGetGroupPlacementPeople];" );

            Sql( RockMigrationSQL._202506232302341_AddAndSwapToObsidianGroupPlacementBlock_spGetGroupPlacementPeople );

            // Add [spGetDestinationGroups] (dropping it first if it already exists).
            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE OBJECT_ID = OBJECT_ID(N'[dbo].[spGetDestinationGroups]') AND TYPE IN (N'P', N'PC'))
    DROP PROCEDURE [dbo].[spGetDestinationGroups];" );

            Sql( RockMigrationSQL._202506232302341_AddAndSwapToObsidianGroupPlacementBlock_spGetDestinationGroups );

            // Restore the original settings.
            Sql( $"SET ANSI_NULLS {( isAnsiNullsOn ? "ON" : "OFF" )};" );
            Sql( $"SET QUOTED_IDENTIFIER {( isQuotedIdentifierOn ? "ON" : "OFF" )};" );

            #endregion
        }

        private void AddAndSwapToObsidianGroupPlacementBlockDown()
        {
            #region Page, Block, and Attribute Updates Down

            // Attribute for BlockType
            //   BlockType: Group Detail
            //   Category: Groups
            //   Attribute: Group Placement Page
            RockMigrationHelper.DeleteAttribute( "75947EFF-7526-4049-A1E0-357DFF4DF8C4" );

            // Attribute for BlockType
            //   BlockType: Registration Instance - Instance Detail
            //   Category: Event
            //   Attribute: Group Placement Page
            RockMigrationHelper.DeleteAttribute( "388E6A48-3886-439D-89B9-F229EC5B1A7E" );

            // Attribute for BlockType
            //   BlockType: Group Placement
            //   Category: Group
            //   Attribute: Group Member Detail Page
            RockMigrationHelper.DeleteAttribute( "2109DD7F-C511-4FCF-A331-04F669E97C04" );

            // Attribute for BlockType
            //   BlockType: Group Placement
            //   Category: Group
            //   Attribute: Group Detail Page
            RockMigrationHelper.DeleteAttribute( "EDA50FE9-B515-47B0-B300-5597B87900A2" );

            // Attribute for BlockType
            //   BlockType: Group Placement
            //   Category: Group
            //   Attribute: Registration Template
            RockMigrationHelper.DeleteAttribute( "0CF6FECA-9C2B-45F2-A8B5-87668AE7112E" );

            // Remove Block
            //  Name: Group Placement, from Page: Group Placement, Site: Rock RMS
            //  from Page: Group Placement, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "7561B3C0-C469-400D-8142-5520EC2D822F" );

            // Delete BlockType 
            //   Name: Group Placement
            //   Category: Group
            //   Path: -
            //   EntityType: Group Placement
            RockMigrationHelper.DeleteBlockType( "5FA6DDB6-2A99-4882-8E49-562781D69ECA" );

            // Delete Page 
            //  Internal Name: Group Placement
            //  Site: Rock RMS
            //  Layout: Full Worksurface
            RockMigrationHelper.DeletePage( "C1B0C21F-FF3B-4D79-A11A-75BF689A954A" );

            #endregion

            #region Delete GroupPlacement Stored Procedures

            // Delete [spGetGroupPlacementPeople].
            Sql( "DROP PROCEDURE [dbo].[spGetGroupPlacementPeople];" );

            // Delete [spGetDestinationGroups].
            Sql( "DROP PROCEDURE [dbo].[spGetDestinationGroups];" );

            #endregion
        }
    }
}
