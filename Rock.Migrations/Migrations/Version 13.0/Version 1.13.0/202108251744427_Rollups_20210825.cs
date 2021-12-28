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
    public partial class Rollups_20210825 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            FixAchievementTypeCapitalization();
            RegistrationTemplateTreeviewHeadingChange();
            AddDataViewDetailPagesToGroupMemberListBlocks();
            UpdateRockThemeFooterText();
            AddUrlParameterToKPIs();
            UpdateAttributeMatrixTemplate();
            UpdateRockShopPageBlockOrder();
            UpStoreHeaderBlock();
            HideLegacyEmailPage();
            UpdateGroupSchedulingConfirmationEmail_Up();
            UpdateWebFarmFontAwesomeIcon();
            MobileNotesBlockUp();
            MediaElementFieldTypesUp();
            CreateLocationSelectionStrategy();
            CreateWorkflowActionFilterLocationsByLocationSelectionStrategy();
            WorkflowUpdateUnattendedCheckin();
            WindowsCheckinClientDownloadLinkUp();
            FixCommunicationListSubPagesBreadcrumbs();
            AddAttributeHideRequestStatusToConnectionRequestBlock();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DownStoreHeaderBlock();
            MobileNotesBlockDown();
            WindowsCheckinClientDownloadLinkDown();
        }

        /// <summary>
        /// NA: Migration to Fix Achievement Type Capitalization
        /// </summary>
        private void FixAchievementTypeCapitalization()
        {
            Sql( @"
                DECLARE  @AchievementTypeGuidTenWeeksInARow UNIQUEIDENTIFIER = '21E6CC63-702B-4A5D-BC92-503B0F5CAF5D'

                UPDATE [AchievementType]
                SET [Name] = 'Ten Weeks in a Row'
                WHERE [Guid] = @AchievementTypeGuidTenWeeksInARow" );
        }

        /// <summary>
        /// GJ: Registration Template treeview heading change
        /// </summary>
        private void RegistrationTemplateTreeviewHeadingChange()
        {
            // Attrib Value for Block:Category Tree View, Attribute:Entity Type Friendly Name Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("C9C18C22-6D23-4F96-AB40-296E66EE4142","07213E2C-C239-47CA-A781-F7A908756DC2",@"Templates");
        }

        /// <summary>
        /// GJ: Add Data View Detail Pages to Group Member List Blocks
        /// </summary>
        private void AddDataViewDetailPagesToGroupMemberListBlocks()
        {
            // Add Block Attribute Value
            //   Block: Group Member List
            //   BlockType: Group Member List
            //   Block Location: Page=Security Roles Detail, Site=Rock RMS
            //   Attribute: Data View Detail Page
            //   Attribute Value: 4011cb37-28aa-46c4-99d5-826f4a9cadf5
            RockMigrationHelper.AddBlockAttributeValue("E71D3062-286A-49D2-A0BB-84B385EFAD50","D8C39172-1C5A-4F47-9733-273117ADD7B3",@"4011cb37-28aa-46c4-99d5-826f4a9cadf5");

            // Add Block Attribute Value
            //   Block: Group Member List
            //   BlockType: Group Member List
            //   Block Location: Page=Application Group Detail, Site=Rock RMS
            //   Attribute: Data View Detail Page
            //   Attribute Value: 4011cb37-28aa-46c4-99d5-826f4a9cadf5
            RockMigrationHelper.AddBlockAttributeValue("AB47ABE2-B9BB-4C89-B35A-ABCECA2098C6","D8C39172-1C5A-4F47-9733-273117ADD7B3",@"4011cb37-28aa-46c4-99d5-826f4a9cadf5");

            // Add Block Attribute Value
            //   Block: Group Member List
            //   BlockType: Group Member List
            //   Block Location: Page=Photo Request Application Group, Site=Rock RMS
            //   Attribute: Data View Detail Page
            //   Attribute Value: 4011cb37-28aa-46c4-99d5-826f4a9cadf5
            RockMigrationHelper.AddBlockAttributeValue("B99901FD-E852-4FCF-8F9B-0870984D59AE","D8C39172-1C5A-4F47-9733-273117ADD7B3",@"4011cb37-28aa-46c4-99d5-826f4a9cadf5");

            // Add Block Attribute Value
            //   Block: Group Member List
            //   BlockType: Group Member List
            //   Block Location: Page=Org Chart, Site=Rock RMS
            //   Attribute: Data View Detail Page
            //   Attribute Value: 4011cb37-28aa-46c4-99d5-826f4a9cadf5
            RockMigrationHelper.AddBlockAttributeValue("DB536038-A421-408F-8A33-1B95ADB8A51E","D8C39172-1C5A-4F47-9733-273117ADD7B3",@"4011cb37-28aa-46c4-99d5-826f4a9cadf5");

            // Add Block Attribute Value
            //   Block: Group Member List
            //   BlockType: Group Member List
            //   Block Location: Page=Communication List Detail, Site=Rock RMS
            //   Attribute: Data View Detail Page
            //   Attribute Value: 4011cb37-28aa-46c4-99d5-826f4a9cadf5
            RockMigrationHelper.AddBlockAttributeValue("B906C477-BFA2-4617-BCE4-B7A1D3D8042C","D8C39172-1C5A-4F47-9733-273117ADD7B3",@"4011cb37-28aa-46c4-99d5-826f4a9cadf5");

            // Add Block Attribute Value
            //   Block: Group Member List
            //   BlockType: Group Member List
            //   Block Location: Page=Campus Detail, Site=Rock RMS
            //   Attribute: Data View Detail Page
            //   Attribute Value: 4011cb37-28aa-46c4-99d5-826f4a9cadf5
            RockMigrationHelper.AddBlockAttributeValue("318B80EE-7349-4BF4-82F2-64FC38A5AB0B","D8C39172-1C5A-4F47-9733-273117ADD7B3",@"4011cb37-28aa-46c4-99d5-826f4a9cadf5");
        }

        /// <summary>
        /// GJ: Update Footer Text
        /// </summary>
        private void UpdateRockThemeFooterText()
        {
            // Update Rock Theme Footer Text
            RockMigrationHelper.UpdateHtmlContentBlock( "9BBF6F1D-261F-4E95-8652-1C34BD42C1A8", @"<p>Crafted by <a href=""http://www.rockrms.com"" tabindex=""0"">Spark Development Network</a> / <a href=""~/License.aspx"" tabindex=""0"">License</a></p>", "0AF1D334-13F8-4799-9158-304B9FDA235F" );
        }

        /// <summary>
        /// GJ: Add URL parameter to KPIs
        /// </summary>
        private void AddUrlParameterToKPIs()
        {
            Sql( MigrationSQL._202108251744427_Rollups_20210825_kpilinkshortcode );
        }

        /// <summary>
        /// GJ: Update Attribute Matrix Template
        /// </summary>
        private void UpdateAttributeMatrixTemplate()
        {
            Sql( @"
                UPDATE [AttributeMatrixTemplate]
                SET [FormattedLava] = REPLACE([FormattedLava], '<a class=""btn btn-xs btn-link"" href=""{{ articleLink }}"">More Info</a>', '<a class=""btn btn-xs btn-link p-0"" href=""{{ articleLink }}"">More Info</a>')
                WHERE ([Guid] = '1D24694E-445C-4852-B5BC-64CDEA6F7175')" );
        }

        /// <summary>
        /// MB: Rock Shop Block Order
        /// </summary>
        private void UpdateRockShopPageBlockOrder()
        {
            Sql( $@"
                UPDATE [Block]
                SET [Order] = 0
                WHERE [Guid] = '5C5D96E2-7EA1-40A4-8C70-CDDCC66DD955' -- Promo Ad Rotator Block

                UPDATE [Block]
                SET [Order] = 1
                WHERE [Guid] = 'A239E904-3E32-462E-B97D-388E7E87C37F' -- Package Category Header List" );
        }

        /// <summary>
        /// MB: Add Rock Shop Header Block Up()
        /// </summary>
        private void UpStoreHeaderBlock()
        {
            // Add/Update BlockType Store Header
            RockMigrationHelper.UpdateBlockType( "Store Header", "Shows the Organization information used by the Rock Shop.", "~/Blocks/Store/StoreHeader.ascx", "Store", "91355804-4B64-434F-949B-6180E5CC31D9" );

            // Add Block Store Header to Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "B093E7A0-5E7E-4A5F-AEF3-CE397D342BFA".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "91355804-4B64-434F-949B-6180E5CC31D9".AsGuid(), "Store Header", "Main", @"", @"", 0, "5C198E72-2E0F-4FD0-9F24-9B263FAD1789" );

            // update block order for pages with new blocks if the page,zone has multiple blocks
            // Update Order for Page: Rock Shop,  Zone: Main,  Block: Featured Promos
            Sql( @"UPDATE [Block] SET [Order] = 3 WHERE [Guid] = '8D23BB71-69D9-4409-8368-1D965A3C5128'" );
            // Update Order for Page: Rock Shop,  Zone: Main,  Block: Package Category Header List
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = 'A239E904-3E32-462E-B97D-388E7E87C37F'" );
            // Update Order for Page: Rock Shop,  Zone: Main,  Block: Promo Rotator
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '5C5D96E2-7EA1-40A4-8C70-CDDCC66DD955'" );
            // Update Order for Page: Rock Shop,  Zone: Main,  Block: Sponsored Apps
            Sql( @"UPDATE [Block] SET [Order] = 4 WHERE [Guid] = 'FA0152C9-71E1-47FF-9704-8D5EB39261DA'" );
            // Update Order for Page: Rock Shop,  Zone: Main,  Block: Store Header
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '5C198E72-2E0F-4FD0-9F24-9B263FAD1789'" );
            // Update Order for Page: Rock Shop,  Zone: Main,  Block: Top Free
            Sql( @"UPDATE [Block] SET [Order] = 5 WHERE [Guid] = 'C9CFEF38-AF44-4F7D-B2A6-EBC4986527D8'" );

            // Attribute for BlockType: Store Header:Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "91355804-4B64-434F-949B-6180E5CC31D9", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "Lava Template", @"Lava template to use to display the packages", 1, @"{% include '~/Assets/Lava/Store/StoreHeader.lava' %}", "25199FB1-FD8C-4A40-9A99-DFB096FE91B4" );
            // Attribute for BlockType: Store Header:Link Organization Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "91355804-4B64-434F-949B-6180E5CC31D9", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Link Organization Page", "LinkOrganizationPage", "Link Organization Page", @"Page to allow the user to link an organization to the store.", 0, @"", "3DBCAD71-5AC8-4728-8C48-DB3CCD0F966D" );
            // Attribute for BlockType: Lava Tester:Enabled Lava Commands
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "AD092814-8253-4E7B-AC5A-B84A9B811A95", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Enabled Lava Commands", "EnabledLavaCommands", "Enabled Lava Commands", @"The Lava commands that should be enabled.", 0, @"", "DCF68F80-083C-4AA5-A60B-D4F71378E045" );
            // Block Attribute Value for Store Header ( Page: Rock Shop, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "5C198E72-2E0F-4FD0-9F24-9B263FAD1789", "25199FB1-FD8C-4A40-9A99-DFB096FE91B4", @"{% include '~/Assets/Lava/Store/StoreHeader.lava' %}" );
            // Block Attribute Value for Store Header ( Page: Rock Shop, Site: Rock RMS )
            RockMigrationHelper.AddBlockAttributeValue( "5C198E72-2E0F-4FD0-9F24-9B263FAD1789", "3DBCAD71-5AC8-4728-8C48-DB3CCD0F966D", @"6e029432-56f4-46ad-9d9c-c122f3d3c55c" );
        }

        /// <summary>
        /// MB: Add Rock Shop Header Block Down()
        /// </summary>
        private void DownStoreHeaderBlock()
        {
            // Attribute for BlockType: Store Header:Lava Template
            RockMigrationHelper.DeleteAttribute( "25199FB1-FD8C-4A40-9A99-DFB096FE91B4" );
            // Attribute for BlockType: Store Header:Link Organization Page
            RockMigrationHelper.DeleteAttribute( "3DBCAD71-5AC8-4728-8C48-DB3CCD0F966D" );
            // Attribute for BlockType: Lava Tester:Enabled Lava Commands
            RockMigrationHelper.DeleteAttribute( "DCF68F80-083C-4AA5-A60B-D4F71378E045" );

            // Add Block Store Header to Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "5C198E72-2E0F-4FD0-9F24-9B263FAD1789" );

            // Add/Update BlockType Store Header
            RockMigrationHelper.DeleteBlockType( "91355804-4B64-434F-949B-6180E5CC31D9" );

            // update block order for pages with new blocks if the page,zone has multiple blocks
            // Update Order for Page: Rock Shop,  Zone: Main,  Block: Promo Rotator
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '5C5D96E2-7EA1-40A4-8C70-CDDCC66DD955'" );
            // Update Order for Page: Rock Shop,  Zone: Main,  Block: Package Category Header List
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'A239E904-3E32-462E-B97D-388E7E87C37F'" );
            // Update Order for Page: Rock Shop,  Zone: Main,  Block: Featured Promos
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '8D23BB71-69D9-4409-8368-1D965A3C5128'" );
            // Update Order for Page: Rock Shop,  Zone: Main,  Block: Sponsored Apps
            Sql( @"UPDATE [Block] SET [Order] = 3 WHERE [Guid] = 'FA0152C9-71E1-47FF-9704-8D5EB39261DA'" );
            // Update Order for Page: Rock Shop,  Zone: Main,  Block: Top Free
            Sql( @"UPDATE [Block] SET [Order] = 4 WHERE [Guid] = 'C9CFEF38-AF44-4F7D-B2A6-EBC4986527D8'" );
        }

        /// <summary>
        /// ED: Hide Legacy Email Page
        /// </summary>
        private void HideLegacyEmailPage()
        {
            Sql( @"
                UPDATE [Page]
                SET [DisplayInNavWhen] = 2
                WHERE [Guid] = '89B7A631-EA6F-4DA3-9380-04EE67B63E9E'" );
        }

        /// <summary>
        /// MP: Update Group Scheduling Confirmation Email
        /// </summary>
        private void UpdateGroupSchedulingConfirmationEmail_Up()
        {
            Sql( @"
                UPDATE SystemCommunication
                SET Body = REPLACE(body, '<p>Hi {{ Scheduler.NickName }}!</p>', '<p>Hi {{ Recipient.NickName }}!</p>')
                WHERE [Guid] = 'D095F78D-A5CF-4EF6-A038-C7B07E250611' AND [Body] LIKE '%<p>Hi {{ Scheduler.NickName }}!</p>%'" );

            Sql( @"
                UPDATE SystemCommunication
                SET Body = REPLACE(body, '<h2>{{OccurrenceDate | Date:''dddd, MMMM d, yyyy''}}</h2>', '<h4>{{OccurrenceDate | Date:''dddd, MMMM d, yyyy''}}</h4>')
                WHERE [Guid] = 'D095F78D-A5CF-4EF6-A038-C7B07E250611' and [Body] like '%<h2>{{OccurrenceDate | Date:''dddd, MMMM d, yyyy''}}</h2>%'" );
        }

        /// <summary>
        /// SK: Update WebFarm Font Awesome Icon
        /// </summary>
        private void UpdateWebFarmFontAwesomeIcon()
        {
            Sql( @"
                DECLARE @AttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '75A0C88F-7F5B-48A2-88A4-C3A62F0EDF9A' )
                
                UPDATE [AttributeValue]
                SET [Value]=Replace([Value],'WebFarm^fa fa-network-wired','WebFarm^fa fa-server' )
                WHERE [AttributeId] = @AttributeId" );
        }

        /// <summary>
        /// DH: New Mobile Notes Block Type Up()
        /// </summary>
        private void MobileNotesBlockUp()
        {
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Types.Mobile.Core.Notes", "Notes", "Rock.Blocks.Types.Mobile.Core.Notes, Rock, Version=1.13.0.5, Culture=neutral, PublicKeyToken=null", false, false, Rock.SystemGuid.EntityType.MOBILE_CORE_NOTES_BLOCK_TYPE );

            RockMigrationHelper.UpdateMobileBlockType( "Notes", "Displays entity notes to the user and allows adding new notes.", "Rock.Blocks.Types.Mobile.Core.Notes", "Mobile > Core", "5B337D89-A298-4620-A0BE-078A41BC054B" );
        }

        /// <summary>
        /// DH: New Mobile Notes Block Type Down()
        /// </summary>
        private void MobileNotesBlockDown()
        {
            RockMigrationHelper.DeleteBlockType( "5B337D89-A298-4620-A0BE-078A41BC054B" );

            RockMigrationHelper.DeleteEntityType( Rock.SystemGuid.EntityType.MOBILE_CORE_NOTES_BLOCK_TYPE );
        }

        /// <summary>
        /// DH: Media Element Field Types
        /// </summary>
        private void MediaElementFieldTypesUp()
        {
            RockMigrationHelper.UpdateFieldType( "Media Element", "", "Rock", "Rock.Field.Types.MediaElementFieldType", "A17D5AAC-B7AE-4587-B703-A0FC3625F0F8" );
            RockMigrationHelper.UpdateFieldType( "Media Watch", "", "Rock", "Rock.Field.Types.MediaWatchFieldType", "98180C6F-5167-45E1-8ADE-E1A31EC4930D" );
        }

        /// <summary>
        /// ED: Location Selection Strategy
        /// </summary>
        private void CreateLocationSelectionStrategy()
        {
            // insert the attribute
            Sql( @"
                DECLARE @GroupTypeEntityId int = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.GroupType')
                DECLARE @GroupTypeId int = (SELECT [Id] FROM [GroupType] WHERE [Guid] = '0572A5FE-20A4-4BF1-95CD-C71DB5281392')
                DECLARE @FieldTypeId int = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '7525C4CB-EE6B-41D4-9B64-A08048D5A5C0')

                IF NOT EXISTS (
                    SELECT [Id]
                    FROM [Attribute]
                    WHERE [EntityTypeId] = @GroupTypeEntityId
                        AND [EntityTypeQualifierColumn] = 'Id'
                        AND [EntityTypeQualifierValue] = @GroupTypeId
                        AND [Key] = 'core_LocationSelectionStrategy' )
                BEGIN
                    INSERT INTO [Attribute] (
                            [IsSystem]
                        , [FieldTypeId]
                        , [EntityTypeId]
                        , [EntityTypeQualifierColumn]
                        , [EntityTypeQualifierValue]
                        , [Key]
                        , [Name]
                        , [Description]
                        , [Order]
                        , [IsGridColumn]
                        , [DefaultValue]
                        , [IsMultiValue]
                        , [IsRequired]
                        , [Guid])
                    VALUES (
                            1
                        , @FieldTypeId
                        , @GroupTypeEntityId
                        , 'Id'
                        , @GroupTypeId
                        , 'core_LocationSelectionStrategy'
                        , 'Location Selection Strategy'
                        , 'Determines how the location for the group will selected. Ask will offer all available scheduled locations that are open. Load balance will fill rooms in an even manner. Fill In Order will fill locations in their configured order until they are full. The location balancing feature is intended for use with Family check-in because it asks for service time selection before room selection.  If you attempt to use this feature with Individual check-in, you are likely to experience unexpected results.'
                        , 0
                        , 0
                        , 0
                        , 0
                        , 1
                        , '9C626619-2FBD-4157-8789-896944C3C60E')
                END" );

            // Insert the attribute qualifiers
            RockMigrationHelper.AddAttributeQualifier( "9C626619-2FBD-4157-8789-896944C3C60E", "fieldtype", "rb", "86FF0539-9F2D-4BCD-A634-506B8DBB03DF" );
            RockMigrationHelper.AddAttributeQualifier( "9C626619-2FBD-4157-8789-896944C3C60E", "repeatColumns", "", "D617CDD6-B2BC-4040-8338-09F498A1F0C4" );
            RockMigrationHelper.AddAttributeQualifier( "9C626619-2FBD-4157-8789-896944C3C60E", "values", "0^Ask, 1^Load Balance, 2^Fill In Order", "9374899C-50C4-4FAB-9FDE-0B9E5DB502BB" );
        }

        /// <summary>
        /// ED: Location Selection Strategy - WorkflowActionFilter LocationsByLocationSelectionStrategy
        /// </summary>
        private void CreateWorkflowActionFilterLocationsByLocationSelectionStrategy()
        {
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.CheckIn.FilterLocationsByLocationSelectionStrategy", "176E0639-6482-4AED-957F-FDAA7AAA44FA", false, true );

            // Rock.Workflow.Action.CheckIn.FilterLocationsByLocationSelectionStrategy:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "176E0639-6482-4AED-957F-FDAA7AAA44FA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, "False", "B5FEDF04-C70A-4E07-906B-DD7DFE901A38" );

            // Rock.Workflow.Action.CheckIn.FilterLocationsByLocationSelectionStrategy:Remove
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "176E0639-6482-4AED-957F-FDAA7AAA44FA", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Remove", "Remove", "Select 'Yes' if locations should be be removed. Select 'No' if they should just be marked as excluded.", 0, "True", "E08A9DB5-711D-4079-BEE9-4E5959109C7D" );

            // Rock.Workflow.Action.CheckIn.FilterLocationsByLocationSelectionStrategy:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "176E0639-6482-4AED-957F-FDAA7AAA44FA", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, "", "76C9C5B8-093A-4F00-A257-AEE0D7A780E9" );
        }

        /// <summary>
        /// ED: Location Selection Strategy - Update UnattendedCheckin Workflow
        /// </summary>
        private void WorkflowUpdateUnattendedCheckin()
        {
            // Unattended Check-in:Ability Level Search:Location Strategy Filter
            RockMigrationHelper.UpdateWorkflowActionType( "0E2F5EBA-2204-4C2F-845A-92C25AB67474", "Location Strategy Filter", 2, "176E0639-6482-4AED-957F-FDAA7AAA44FA", true, false, "", "", 1, "", "90F1ACA5-0AD8-4F37-B269-EE6EF4428F9C" ); 
            
            // Update the order number for the WorkflowActionTypes that come after the new Location Strategy Filter WFA
            Sql( "UPDATE [WorkflowActionType] SET [Order] = [Order] + 1 WHERE [Guid] = '5ADD8020-B869-4ECF-A1C0-C3D38F907DB1'" );
            Sql( "UPDATE [WorkflowActionType] SET [Order] = [Order] + 1 WHERE [Guid] = '81755F3B-96C1-4517-A019-04B16E8B5B51'" );
            Sql( "UPDATE [WorkflowActionType] SET [Order] = [Order] + 1 WHERE [Guid] = '902931D2-6326-4A6A-967C-C9F65F8C1386'" );

            // Unattended Check-in:Ability Level Search:Location Strategy Filter:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "90F1ACA5-0AD8-4F37-B269-EE6EF4428F9C", "B5FEDF04-C70A-4E07-906B-DD7DFE901A38", "False" );

            // Unattended Check-in:Ability Level Search:Location Strategy Filter:Remove
            RockMigrationHelper.AddActionTypeAttributeValue( "90F1ACA5-0AD8-4F37-B269-EE6EF4428F9C", "E08A9DB5-711D-4079-BEE9-4E5959109C7D", "False" );
        }

        /// <summary>
        /// ED: Update Windows Check-in Client Download Link
        /// </summary>
        private void WindowsCheckinClientDownloadLinkUp()
        {
            Sql( @"
                DECLARE @winCheckinClientDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = 'C162F21E-7D86-4BB4-A72C-7F4A0E5B02C3')
                DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')

                UPDATE [AttributeValue]
                SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/windowscheckin/1.12.4/checkinclient.msi'
                WHERE AttributeId = @downloadUrlAttributeId and EntityId = @winCheckinClientDefinedValueId" );
        }

        /// <summary>
        /// ED: Update Windows Check-in Client Download Link - Restores the old Rock Windows Check-in Client download link.
        /// </summary>
        private void WindowsCheckinClientDownloadLinkDown()
        {
            Sql( @"
                DECLARE @winCheckinClientDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = 'C162F21E-7D86-4BB4-A72C-7F4A0E5B02C3')
                DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')

                UPDATE [AttributeValue]
                SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/windowscheckin/1.11.1/checkinclient.msi'
                WHERE AttributeId = @downloadUrlAttributeId and EntityId = @winCheckinClientDefinedValueId" );
        }

        /// <summary>
        /// SK: Removed extraneous page names from the breadcrumbs under two Communication List sub-pages.
        /// </summary>
        private void FixCommunicationListSubPagesBreadcrumbs()
        {
            Sql( "UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] IN ('60216406-5BD6-4253-B891-262717C07A00','FB3FCA8D-2011-42B5-A9F4-2657C4F856AC')" );
        }

        /// <summary>
        /// ED: ConnectionRequestBlock HideRequestStatus
        /// </summary>
        private void AddAttributeHideRequestStatusToConnectionRequestBlock()
        {
            // Attribute for BlockType: Connection Requests:Hide Connection Requests With These States
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "39C53B93-C75A-45DE-B9E7-DFA4EE6B7027", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Hide Connection Requests With These States", "HideRequestStates", "Hide Connection Requests With These States", @"Any of the states you select here will be excluded from the list.", 0, @"", "9ABD8CC2-7951-4147-A229-5316F298208A" );

            Sql( @"
            DECLARE @HideInactiveRequstStatusAttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '259854C4-C01B-455E-B462-009E6DE430EE')
            DECLARE @HideRequestStatesAttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '9ABD8CC2-7951-4147-A229-5316F298208A')

            --INSERT INTO AttributeValue([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
            SELECT 1, @HideRequestStatesAttributeId, a.[EntityTypeId], '1', NEWID()
            FROM Attribute a 
            left JOIN AttributeValue av ON a.Id = av.AttributeId
            WHERE a.[Id] = @HideInactiveRequstStatusAttributeId AND av.[Value] = '1'");
        }
    }
}
