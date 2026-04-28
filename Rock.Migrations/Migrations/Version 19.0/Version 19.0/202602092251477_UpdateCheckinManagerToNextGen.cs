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
    /// <summary>
    ///
    /// </summary>
    public partial class UpdateCheckinManagerToNextGen : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            MoveCheckInManagerPagesUp();
            GlobalLayoutBlocksUp();
            SwitchSiteThemeUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            SwitchSiteThemeDown();
            GlobalLayoutBlocksDown();
            MoveCheckInManagerPagesDown();
        }

        private void MoveCheckInManagerPagesUp()
        {
            // Move all child pages of the Room Manager page up to be beneath
            // the Check-in Manager page. This allows us to safely delete the
            // Room Manager page.
            Sql( @"
DECLARE @NewParentPageId INT = (SELECT TOP 1 [ParentPageId] FROM [Page] WHERE [Guid] = 'CECB1460-10D4-4054-B5C3-903991CA40AB')
DECLARE @OldParentPageId INT = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = 'CECB1460-10D4-4054-B5C3-903991CA40AB')

UPDATE [Page] SET [ParentPageId] = @NewParentPageId WHERE [ParentPageId] = @OldParentPageId
" );

            // Delete the Room Manager page since it is no longer needed.
            RockMigrationHelper.DeletePage( SystemGuid.Page.CHECK_IN_MANAGER_ROOM_MANAGER );

            // Delete the Room Settings page since it is no longer needed.
            RockMigrationHelper.DeletePage( SystemGuid.Page.CHECK_IN_MANAGER_ROOM_SETTINGS );

            // Update the name and icon for the old Roster page.
            Sql( @"
UPDATE [Page]
SET [InternalName] = 'Room Manager',
    [PageTitle] = 'Room Manager',
    [BrowserTitle] = 'Room Manager',
    [IconCssClass] = 'ti ti-door-enter'
WHERE [Guid] = 'BA04BF01-5244-4637-B12D-7A962D2A9E77'
" );

            // Update the Order of the pages.
            Sql( @"
DECLARE @CheckinManagerPageId INT = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = 'A4DCE339-9C11-40CA-9A02-D2FE64EA164B')
DECLARE @SearchOrder INT = (SELECT TOP 1 [Order] FROM [Page] WHERE [Guid] = '5BB14114-BE20-4330-943A-5BC7E367116E')

UPDATE [Page]
SET [Order] = [Order] + 1
WHERE [ParentPageId] = @CheckinManagerPageId AND [Order] > @SearchOrder

UPDATE [Page]
SET [Order] = @SearchOrder + 1
WHERE [Guid] = 'BA04BF01-5244-4637-B12D-7A962D2A9E77'
" );
        }

        private void MoveCheckInManagerPagesDown()
        {
            // Restore the name and icon of the Roster page.
            // Update the name and icon for the old Roster page.
            Sql( @"
UPDATE [Page]
SET [InternalName] = 'Roster',
    [PageTitle] = 'Roster',
    [BrowserTitle] = 'Roster',
    [IconCssClass] = ''
WHERE [Guid] = 'BA04BF01-5244-4637-B12D-7A962D2A9E77'
" );

            // NOTE: We don't restore the old Room Settings page block since it would
            // be difficult to get the block settings back.

            // Restore the Room Manager page.
            RockMigrationHelper.AddPage( true,
                SystemGuid.Page.CHECK_IN_MANAGER,
                "8305704F-928D-4379-967A-253E576E0923",
                "Room Manager",
                string.Empty,
                SystemGuid.Page.CHECK_IN_MANAGER_ROOM_MANAGER,
                string.Empty,
                SystemGuid.Page.CHECK_IN_MANAGER_SEARCH );

            // Move the Roster page back under the Room Manager page
            Sql( @"
DECLARE @NewParentPageId INT = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = 'CECB1460-10D4-4054-B5C3-903991CA40AB')

UPDATE [Page] SET [ParentPageId] = @NewParentPageId WHERE [Guid] = 'BA04BF01-5244-4637-B12D-7A962D2A9E77'
" );

            // NOTE: We don't restore the old Room Settings page since it would
            // be difficult to get the block settings back.
        }

        private void GlobalLayoutBlocksUp()
        {
            // Delete the Back Button HTML block since it is now part of
            // the theme.
            RockMigrationHelper.DeleteBlock( "B62CBF17-7FD1-42C8-9E98-00270A34400D" );

            // Delete the old Campus Context Setter block.
            RockMigrationHelper.DeleteBlock( "8B940F43-C38A-4086-80D8-7C33961518E3" );

            // Add/Update Obsidian Block Entity Type
            //   Rock.Blocks.CheckIn.Manager.CheckInContextSetter
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.CheckIn.Manager.CheckInContextSetter",
                "Check-in Context Setter",
                "Rock.Blocks.CheckIn.Manager.CheckInContextSetter, Rock.Blocks, Version=19.0.0.0, Culture=neutral, PublicKeyToken=null",
                false,
                false,
                "a8256bb8-66c8-4038-ad0c-041678ba7278" );

            // Add/Update Obsidian Block Type
            //   Name: Check-in Context Setter
            //   Category: Check-in > Manager
            //   EntityType: Rock.Blocks.CheckIn.Manager.CheckInContextSetter
            RockMigrationHelper.AddOrUpdateEntityBlockType( "Check-in Context Setter",
                "Block that can be used to set the various context values for the check-in manager pages.",
                "Rock.Blocks.CheckIn.Manager.CheckInContextSetter",
                "Check-in > Manager",
                "3364aabf-0c5b-4bfb-8cf3-b1a80fd3ed10" );

            // Add the Check-in Context Setter block to the Header zone of the
            // global layout.
            RockMigrationHelper.AddBlock(
                string.Empty,
                "8305704F-928D-4379-967A-253E576E0923",
                "3364aabf-0c5b-4bfb-8cf3-b1a80fd3ed10",
                "Check-in Context Setter",
                "Header",
                string.Empty,
                string.Empty,
                0,
                "cb28a144-37ec-43eb-9fab-27ea1136fd15" );

            // Add the Login Status block to the Login zone of the global layout.
            RockMigrationHelper.AddBlock(
                string.Empty,
                "8305704F-928D-4379-967A-253E576E0923",
                "04712F3D-9667-4901-A49D-4507573EF7AD",
                "Login Status",
                "Login",
                string.Empty,
                string.Empty,
                0,
                "4f2310ca-f67b-4dd3-b7dc-a8577ce0c7e9" );

            // Ensure the attribute we will be setting a default value for is created.
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "04712F3D-9667-4901-A49D-4507573EF7AD",
                SystemGuid.FieldType.SINGLE_SELECT,
                "Mode",
                "Mode",
                "Mode",
                "The functionality mode to use when rendering the block. Minimal will display just profile photo.",
                4,
                "0",
                "ac52cf41-07d4-4382-a195-2805863de3c4" );

            RockMigrationHelper.AddBlockAttributeValue( "4f2310ca-f67b-4dd3-b7dc-a8577ce0c7e9",
                "ac52cf41-07d4-4382-a195-2805863de3c4",
                "1" );
        }

        private void GlobalLayoutBlocksDown()
        {
            // Delete the Login Status block from the global layout.
            RockMigrationHelper.DeleteBlock( "4f2310ca-f67b-4dd3-b7dc-a8577ce0c7e9" );

            // Delete the Check-in Context Setter block from the global layout.
            RockMigrationHelper.DeleteBlock( "cb28a144-37ec-43eb-9fab-27ea1136fd15" );

            // NOTE: We don't restore the old campus context setter block since
            // it would be difficult to get the block settings back.

            // NOTE: We don't restore the old Back Button block since it would
            // be difficult to get the block settings back.
        }

        private void SwitchSiteThemeUp()
        {
            Sql( @"
UPDATE [Site]
SET [Theme] = 'RockManagerNextGen'
WHERE [Guid] = 'A5FA7C3C-A238-4E0B-95DE-B540144321EC'
" );
        }

        private void SwitchSiteThemeDown()
        {
            Sql( @"
UPDATE [Site]
SET [Theme] = 'RockManager'
WHERE [Guid] = 'A5FA7C3C-A238-4E0B-95DE-B540144321EC'
" );
        }
    }
}
