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
    public partial class GivingAchievements : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.StreakType", "StructureSettingsJSON", c => c.String());

            // Checkin Manager Phase-4 rollups
            AddCheckinManagerRosterAttributeCategory_Up();
            AddAttendanceChangesHistoryCategory_Up();
            PagesBlocks_Up();
            PageRoutes_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            PagesBlocks_Down();
            
            DropColumn("dbo.StreakType", "StructureSettingsJSON");
        }

        /// <summary>
        /// Up migration for new attendance changes history category.
        /// </summary>
        private void AddCheckinManagerRosterAttributeCategory_Up()
        {
            RockMigrationHelper.UpdatePersonAttributeCategory( "Check-in Manager Roster Attributes", "fa fa-check-square-o", "", Rock.SystemGuid.Category.PERSON_ATTRIBUTES_CHECK_IN_ROSTER_ALERT_ICON );

            // Add Person Legal Note to the Check-in Manager Roster Attributes category
            Sql( $@"

                DECLARE @LegalNoteAttributeId int
                SET @LegalNoteAttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{Rock.SystemGuid.Attribute.PERSON_LEGAL_NOTE}');

                DECLARE @RosterCategoryId int
                SET @RosterCategoryId = (SELECT [Id] FROM [Category] WHERE [Guid] = '{Rock.SystemGuid.Category.PERSON_ATTRIBUTES_CHECK_IN_ROSTER_ALERT_ICON}')

                IF ( @LegalNoteAttributeId IS NOT NULL AND @RosterCategoryId IS NOT NULL ) BEGIN
                    IF NOT EXISTS (
                        SELECT *
                        FROM [AttributeCategory]
                        WHERE [AttributeId] = @LegalNoteAttributeId
                        AND [CategoryId] = @RosterCategoryId )
                    BEGIN
                        INSERT INTO [AttributeCategory] ( [AttributeId], [CategoryId] )
                        VALUES( @LegalNoteAttributeId, @RosterCategoryId )
                    END
                END
" );

            // Add Person Allergy to the Check-in Manager Roster Attributes category
            Sql( $@"

                DECLARE @PersonAllergyAttributeId int
                SET @PersonAllergyAttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{Rock.SystemGuid.Attribute.PERSON_ALLERGY}');

                DECLARE @RosterCategoryId int
                SET @RosterCategoryId = (SELECT [Id] FROM [Category] WHERE [Guid] = '{Rock.SystemGuid.Category.PERSON_ATTRIBUTES_CHECK_IN_ROSTER_ALERT_ICON}')

                IF ( @PersonAllergyAttributeId IS NOT NULL AND @RosterCategoryId IS NOT NULL ) BEGIN
                    IF NOT EXISTS (
                        SELECT *
                        FROM [AttributeCategory]
                        WHERE [AttributeId] = @PersonAllergyAttributeId
                        AND [CategoryId] = @RosterCategoryId )
                    BEGIN
                        INSERT INTO [AttributeCategory] ( [AttributeId], [CategoryId] )
                        VALUES( @PersonAllergyAttributeId, @RosterCategoryId )
                    END
                END
" );

            // Update IconCSS and AttributeColor on Person Legal Note and Person Allegry
            Sql( $@"
-- Legal Note icon (AttributeColor is not specified)
UPDATE [Attribute]
SET [IconCssClass] = 'fa fa-clipboard'
WHERE [Guid] = '{Rock.SystemGuid.Attribute.PERSON_LEGAL_NOTE}'
    AND isnull([IconCssClass], '') = ''

-- Person Allegry Icon and AttributeColor
UPDATE [Attribute]
SET [IconCssClass] = 'fa fa-notes-medical'
WHERE [Guid] = '{Rock.SystemGuid.Attribute.PERSON_ALLERGY}'
    AND isnull([IconCssClass], '') = ''

UPDATE [Attribute]
SET [AttributeColor] = '#d4442e'
WHERE [Guid] = '{Rock.SystemGuid.Attribute.PERSON_ALLERGY}'
    AND isnull([AttributeColor], '') = ''
" );
        }

        private const string HistoryAttendanceChangeUrlAttributeValueGuid = "9A2C4855-AAD3-4F6C-9ACB-2FAC167D8F3A";

        /// <summary>
        /// Up migration for new attendance changes history category.
        /// </summary>
        private void AddAttendanceChangesHistoryCategory_Up()
        {
            RockMigrationHelper.UpdateCategory( Rock.SystemGuid.EntityType.HISTORY, "Attendance Changes", "", "", Rock.SystemGuid.Category.HISTORY_ATTENDANCE_CHANGES, 9, Rock.SystemGuid.Category.HISTORY_PERSON );
            Sql( $@"
                DECLARE 
                    @AttributeId int,
                    @EntityId int

                -- Attribute for URL Mask on History Category                
                SET @AttributeId = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '0C405062-72BB-4362-9738-90C9ED5ACDDE')
                
                SET @EntityId = (SELECT TOP 1 [ID] FROM [Category] where [Guid] = '{Rock.SystemGuid.Category.HISTORY_ATTENDANCE_CHANGES}')

                DELETE FROM [AttributeValue] WHERE [Guid] = '{HistoryAttendanceChangeUrlAttributeValueGuid}'

                IF ( @AttributeId IS NOT NULL AND @EntityId IS NOT NULL ) BEGIN
                    INSERT INTO [AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Value],[Guid])
                    VALUES(
                        1,@AttributeId,@EntityId,'~/checkinmanager/attendance-detail?attendanceId={{0}}','{HistoryAttendanceChangeUrlAttributeValueGuid}')
                END

" );
        }

        /// <summary>
        /// Up migration for PagesBlocks migration
        /// </summary>
        private void PagesBlocks_Up()
        {
            // Add Page 
            //  Internal Name: Person Attendance History
            //  Site: Rock Check-in Manager
            RockMigrationHelper.AddPage( true, "F3062622-C6AD-48F3-ADD7-7F58E4BD4EF3", "8305704F-928D-4379-967A-253E576E0923", "Person Attendance History", "", Rock.SystemGuid.Page.CHECK_IN_MANAGER_PERSON_ATTENDANCE_CHANGE_HISTORY, "fa fa-history" );

            // Remove page display options for Check-in Manager Attendance Change History page
            Sql( $@"UPDATE [Page]
                SET [PageDisplayBreadCrumb] = 0, [PageDisplayDescription] = 0, [PageDisplayIcon] = 0, [PageDisplayTitle] = 0
                WHERE [Guid] IN ( 
                    '{Rock.SystemGuid.Page.CHECK_IN_MANAGER_PERSON_ATTENDANCE_CHANGE_HISTORY}'
                )" );

            // Add Block 
            //  Block Name: Person Attendance History Log
            //  Page Name: Person Attendance History
            //  Layout: -
            //  Site: Rock Check-in Manager
            RockMigrationHelper.AddBlock( true, "185BFEA9-9E95-4B8D-836B-87AF36BE6109".AsGuid(), null, "A5FA7C3C-A238-4E0B-95DE-B540144321EC".AsGuid(), "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0".AsGuid(), "Person Attendance History Log", "Main", @"", @"", 0, "15186678-4811-4D45-B443-08880B2DA100" );

            // Attribute for BlockType
            //   BlockType: Roster
            //   Category: Check-in > Manager
            //   Attribute: Check-in Roster Alert Icon Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "EA5C2CF9-8602-445F-B2B7-48D0A5CFEA8C", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Check-in Roster Alert Icon Category", "CheckInRosterAlertIconCategory", "Check-in Roster Alert Icon Category", @"The Person Attribute category to get the Alert Icon attributes from", 10, @"367571D1-62D3-4948-B588-C0FDCE00CF27", "9C5C21BF-6DF4-42B2-8E9C-25D0A61F3447" );

            // Attribute for BlockType
            //   BlockType: Search
            //   Category: Check-in > Manager
            //   Attribute: Check-in Roster Alert Icon Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "B10EF525-6F2F-46B8-865C-B4249A297307", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Check-in Roster Alert Icon Category", "CheckInRosterAlertIconCategory", "Check-in Roster Alert Icon Category", @"The Person Attribute category to get the Alert Icon attributes from", 2, @"367571D1-62D3-4948-B588-C0FDCE00CF27", "817239DF-FBA8-446F-9185-718FA80521FC" );

            // Attribute for BlockType
            //   BlockType: En Route
            //   Category: Check-in > Manager
            //   Attribute: Check-in Roster Alert Icon Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "BC86F18C-9F38-4CA3-8CF9-5A837CBC700D", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Check-in Roster Alert Icon Category", "CheckInRosterAlertIconCategory", "Check-in Roster Alert Icon Category", @"The Person Attribute category to get the Alert Icon attributes from", 4, @"367571D1-62D3-4948-B588-C0FDCE00CF27", "7DF9226C-A3FF-46AF-A1C1-52A9FEA3C1F0" );

            // Attribute for BlockType
            //   BlockType: Person Recent Attendances
            //   Category: Check-in > Manager
            //   Attribute: Attendance History Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "486892AE-B5FD-447C-9E27-15A4BF3667CB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Attendance History Page", "PersonAttendanceHistoryPage", "Attendance History Page", @"Page to shows a history of changes to person's attendances.", 8, @"185BFEA9-9E95-4B8D-836B-87AF36BE6109", "21B1B770-9E03-47C1-A90D-548C71581E79" );

            // Attribute for BlockType
            //   BlockType: History Log
            //   Category: Core
            //   Attribute: Category
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Category", "Category", "Category", @"", 1, @"", "44092D4B-213D-4572-A005-C2B35E0B4082" );

            // Add Block Attribute Value
            //   Block: Person Attendance History Log
            //   BlockType: History Log
            //   Category: Core
            //   Block Location: Page=Person Attendance History, Site=Rock Check-in Manager
            //   Attribute: Entity Type
            /*   Attribute Value: 72657ed8-d16e-492e-ac12-144c5e7567e7 */
            RockMigrationHelper.AddBlockAttributeValue( "15186678-4811-4D45-B443-08880B2DA100", "8FB690EC-5299-46C5-8695-AAD23168E6E1", @"72657ed8-d16e-492e-ac12-144c5e7567e7" );

            // Add Block Attribute Value
            //   Block: Person Attendance History Log
            //   BlockType: History Log
            //   Category: Core
            //   Block Location: Page=Person Attendance History, Site=Rock Check-in Manager
            //   Attribute: Heading
            /*   Attribute Value: <i class='fa fa-history'></i> {{ Entity.EntityStringValue }} Attendance Change History */
            RockMigrationHelper.AddBlockAttributeValue( "15186678-4811-4D45-B443-08880B2DA100", "614CD413-DCB7-4DA2-80A0-C7ABE5A11047", @"<i class='fa fa-history'></i> {{ Entity.EntityStringValue }} Attendance Change History" );

            // Add Block Attribute Value
            //   Block: Person Attendance History Log
            //   BlockType: History Log
            //   Category: Core
            //   Block Location: Page=Person Attendance History, Site=Rock Check-in Manager
            //   Attribute: Category
            /*   Attribute Value: bf6abcd3-ad41-4d54-998f-b83c302756e3 */
            RockMigrationHelper.AddBlockAttributeValue( "15186678-4811-4D45-B443-08880B2DA100", "44092D4B-213D-4572-A005-C2B35E0B4082", @"bf6abcd3-ad41-4d54-998f-b83c302756e3" );

            // Add/Update PageContext for Page:Person Attendance History, Entity: Rock.Model.Person, Parameter: PersonId
            RockMigrationHelper.UpdatePageContext( "185BFEA9-9E95-4B8D-836B-87AF36BE6109", "Rock.Model.Person", "PersonId", "F6532D7B-9880-43EF-ABD5-41170F1A88B5" );
        }

        /// <summary>
        /// Up migration for Page Routes migration
        /// </summary>
        private void PageRoutes_Up()
        {
            // Change the /CheckinManager route to /checkinmanager. Routes aren't case sensitive, so that'll make it look consistent with other routes without breaking expected route
            RockMigrationHelper.UpdatePageRoute( Rock.SystemGuid.PageRoute.CHECK_IN_MANAGER, Rock.SystemGuid.Page.CHECK_IN_MANAGER_ROSTER, "checkinmanager" );

#pragma warning disable CS0618 // Type or member is obsolete
            // Add /checkinmanager/attendance-detail route
            RockMigrationHelper.AddPageRoute( Rock.SystemGuid.Page.CHECK_IN_MANAGER_ATTENDANCE_DETAIL, "checkinmanager/attendance-detail", Rock.SystemGuid.PageRoute.CHECK_IN_MANAGER_ATTENDANCE_DETAIL );
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Down migration for PagesBlocks migration
        /// </summary>
        private void PagesBlocks_Down()
        {
            // Attribute for BlockType
            //   BlockType: History Log
            //   Category: Core
            //   Attribute: Category
            RockMigrationHelper.DeleteAttribute( "44092D4B-213D-4572-A005-C2B35E0B4082" );

            // Attribute for BlockType
            //   BlockType: Person Recent Attendances
            //   Category: Check-in > Manager
            //   Attribute: Attendance History Page
            RockMigrationHelper.DeleteAttribute( "21B1B770-9E03-47C1-A90D-548C71581E79" );

            // Attribute for BlockType
            //   BlockType: En Route
            //   Category: Check-in > Manager
            //   Attribute: Check-in Roster Alert Icon Category
            RockMigrationHelper.DeleteAttribute( "7DF9226C-A3FF-46AF-A1C1-52A9FEA3C1F0" );

            // Attribute for BlockType
            //   BlockType: Search
            //   Category: Check-in > Manager
            //   Attribute: Check-in Roster Alert Icon Category
            RockMigrationHelper.DeleteAttribute( "817239DF-FBA8-446F-9185-718FA80521FC" );

            // Attribute for BlockType
            //   BlockType: Roster
            //   Category: Check-in > Manager
            //   Attribute: Check-in Roster Alert Icon Category
            RockMigrationHelper.DeleteAttribute( "9C5C21BF-6DF4-42B2-8E9C-25D0A61F3447" );

            // Remove Block
            //  Name: Person Attendance History Log, from Page: Person Attendance History, Site: Rock Check-in Manager
            //  from Page: Person Attendance History, Site: Rock Check-in Manager
            RockMigrationHelper.DeleteBlock( "15186678-4811-4D45-B443-08880B2DA100" );

            // Delete Page 
            //  Internal Name: Person Attendance History
            //  Site: Rock Check-in Manager
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "185BFEA9-9E95-4B8D-836B-87AF36BE6109" );

            // Delete PageContext for Page:Person Attendance History, Entity: Rock.Model.Person, Parameter: PersonId
            RockMigrationHelper.DeletePageContext( "F6532D7B-9880-43EF-ABD5-41170F1A88B5" );
        }
    }
}
