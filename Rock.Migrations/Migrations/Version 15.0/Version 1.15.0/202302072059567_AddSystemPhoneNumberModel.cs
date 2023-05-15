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
    public partial class AddSystemPhoneNumberModel : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ModelChangesUp();
            ConvertDefinedValuesToPhoneNumbers();
            PagesAndBlocksUp();
            PageRoutesUp();
            FieldTypesUp();
            PostUpdateJobUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            PostUpdateJobDown();
            FieldTypesDown();
            PageRoutesDown();
            PagesAndBlocksDown();
            ModelChangesDown();
        }

        /// <summary>
        /// Applies the model changes required to add the SystemPhoneNumber feature.
        /// </summary>
        private void ModelChangesUp()
        {
            CreateTable(
                "dbo.SystemPhoneNumber",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 100 ),
                    Description = c.String(),
                    Number = c.String( nullable: false, maxLength: 20 ),
                    IsActive = c.Boolean( nullable: false ),
                    Order = c.Int( nullable: false ),
                    AssignedToPersonAliasId = c.Int(),
                    IsSmsEnabled = c.Boolean( nullable: false ),
                    IsSmsForwardingEnabled = c.Boolean( nullable: false ),
                    SmsReceivedWorkflowTypeId = c.Int(),
                    SmsNotificationGroupId = c.Int(),
                    MobileApplicationSiteId = c.Int(),
                    ProviderIdentifier = c.String( maxLength: 50 ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.AssignedToPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.Site", t => t.MobileApplicationSiteId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.Group", t => t.SmsNotificationGroupId )
                .ForeignKey( "dbo.WorkflowType", t => t.SmsReceivedWorkflowTypeId )
                .Index( t => t.AssignedToPersonAliasId )
                .Index( t => t.SmsReceivedWorkflowTypeId )
                .Index( t => t.SmsNotificationGroupId )
                .Index( t => t.MobileApplicationSiteId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            // Moved the index creation on some of the existing tables to job
            // because they can be large tables and it might take a while.
            AddColumn( "dbo.SystemCommunication", "SmsFromSystemPhoneNumberId", c => c.Int() );
            AddColumn( "dbo.Communication", "SmsFromSystemPhoneNumberId", c => c.Int() );
            AddColumn( "dbo.CommunicationTemplate", "SmsFromSystemPhoneNumberId", c => c.Int() );
            AddColumn( "dbo.CommunicationResponse", "RelatedSmsFromSystemPhoneNumberId", c => c.Int() );
            CreateIndex( "dbo.SystemCommunication", "SmsFromSystemPhoneNumberId" );
            CreateIndex( "dbo.CommunicationTemplate", "SmsFromSystemPhoneNumberId" );
            //CreateIndex("dbo.Communication", "SmsFromSystemPhoneNumberId");
            //CreateIndex( "dbo.CommunicationResponse", "RelatedSmsFromSystemPhoneNumberId");
            AddForeignKey( "dbo.SystemCommunication", "SmsFromSystemPhoneNumberId", "dbo.SystemPhoneNumber", "Id" );
            AddForeignKey( "dbo.CommunicationTemplate", "SmsFromSystemPhoneNumberId", "dbo.SystemPhoneNumber", "Id" );
            AddForeignKey( "dbo.Communication", "SmsFromSystemPhoneNumberId", "dbo.SystemPhoneNumber", "Id" );
            AddForeignKey( "dbo.CommunicationResponse", "RelatedSmsFromSystemPhoneNumberId", "dbo.SystemPhoneNumber", "Id" );

            // Remove old foreign keys that are no longer wanted. We want to be
            // able to delete the defined values.
            DropForeignKey( "dbo.Communication", "SMSFromDefinedValueId", "dbo.DefinedValue" );
            DropForeignKey( "dbo.CommunicationTemplate", "SMSFromDefinedValueId", "dbo.DefinedValue" );

            RockMigrationHelper.UpdateEntityType( "Rock.Model.SystemPhoneNumber", "System Phone Number", "Rock.Model.SystemPhoneNumber, Rock, Version=1.15.0.11, Culture=neutral, PublicKeyToken=null", true, true, "66D62A9F-13CD-4160-8653-211B2A4ABF16" );
        }

        /// <summary>
        /// Removes the model changes required to remove the SystemPhoneNumber feature.
        /// </summary>
        private void ModelChangesDown()
        {
            // Restore the help text.
            Sql( @"
UPDATE [DefinedType]
SET [HelpText] = ''
WHERE [Guid] = '611bde1f-7405-4d16-8626-ccfedb0e62be'
" );

            // Restore foreign keys we removed during the Up operation.
            AddForeignKey( "dbo.CommunicationTemplate", "SMSFromDefinedValueId", "dbo.DefinedValue", "Id" );
            AddForeignKey( "dbo.Communication", "SMSFromDefinedValueId", "dbo.DefinedValue", "Id" );

            DropForeignKey( "dbo.CommunicationResponse", "RelatedSmsFromSystemPhoneNumberId", "dbo.SystemPhoneNumber" );
            DropForeignKey( "dbo.Communication", "SmsFromSystemPhoneNumberId", "dbo.SystemPhoneNumber" );
            DropForeignKey( "dbo.CommunicationTemplate", "SmsFromSystemPhoneNumberId", "dbo.SystemPhoneNumber" );
            DropForeignKey( "dbo.SystemCommunication", "SmsFromSystemPhoneNumberId", "dbo.SystemPhoneNumber" );
            DropForeignKey( "dbo.SystemPhoneNumber", "SmsReceivedWorkflowTypeId", "dbo.WorkflowType" );
            DropForeignKey( "dbo.SystemPhoneNumber", "SmsNotificationGroupId", "dbo.Group" );
            DropForeignKey( "dbo.SystemPhoneNumber", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.SystemPhoneNumber", "MobileApplicationSiteId", "dbo.Site" );
            DropForeignKey( "dbo.SystemPhoneNumber", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.SystemPhoneNumber", "AssignedToPersonAliasId", "dbo.PersonAlias" );
            DropIndex( "dbo.CommunicationResponse", new[] { "RelatedSmsFromSystemPhoneNumberId" } );
            DropIndex( "dbo.CommunicationTemplate", new[] { "SmsFromSystemPhoneNumberId" } );
            DropIndex( "dbo.Communication", new[] { "SmsFromSystemPhoneNumberId" } );
            DropIndex( "dbo.SystemPhoneNumber", new[] { "Guid" } );
            DropIndex( "dbo.SystemPhoneNumber", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.SystemPhoneNumber", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.SystemPhoneNumber", new[] { "MobileApplicationSiteId" } );
            DropIndex( "dbo.SystemPhoneNumber", new[] { "SmsNotificationGroupId" } );
            DropIndex( "dbo.SystemPhoneNumber", new[] { "SmsReceivedWorkflowTypeId" } );
            DropIndex( "dbo.SystemPhoneNumber", new[] { "AssignedToPersonAliasId" } );
            DropIndex( "dbo.SystemCommunication", new[] { "SmsFromSystemPhoneNumberId" } );
            DropColumn( "dbo.CommunicationResponse", "RelatedSmsFromSystemPhoneNumberId" );
            DropColumn( "dbo.CommunicationTemplate", "SmsFromSystemPhoneNumberId" );
            DropColumn( "dbo.Communication", "SmsFromSystemPhoneNumberId" );
            DropColumn( "dbo.SystemCommunication", "SmsFromSystemPhoneNumberId" );
            DropTable( "dbo.SystemPhoneNumber" );
        }

        /// <summary>
        /// Converts the defined values to phone numbers.
        /// </summary>
        private void ConvertDefinedValuesToPhoneNumbers()
        {
            Sql( @"
DECLARE @SystemPhoneNumberEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.SystemPhoneNumber')
DECLARE @DefinedValueEntityTypeId INT = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.DefinedValue')
DECLARE @PhoneNumberDefinedTypeId INT = (SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = '611bde1f-7405-4d16-8626-ccfedb0e62be')
DECLARE @ResponseRecipientAttributeId INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Key] = 'ResponseRecipient' AND [EntityTypeId] = @DefinedValueEntityTypeId AND [EntityTypeQualifierColumn] = 'DefinedTypeId' AND [EntityTypeQualifierValue] = @PhoneNumberDefinedTypeId)
DECLARE @EnableResponseRecipientForwardingAttributeId INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Key] = 'EnableResponseRecipientForwarding' AND [EntityTypeId] = @DefinedValueEntityTypeId AND [EntityTypeQualifierColumn] = 'DefinedTypeId' AND [EntityTypeQualifierValue] = @PhoneNumberDefinedTypeId)
DECLARE @LaunchWorkflowOnResponseReceivedAttributeId INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Key] = 'LaunchWorkflowOnResponseReceived' AND [EntityTypeId] = @DefinedValueEntityTypeId AND [EntityTypeQualifierColumn] = 'DefinedTypeId' AND [EntityTypeQualifierValue] = @PhoneNumberDefinedTypeId)

INSERT INTO [SystemPhoneNumber] (
    [Guid],
    [Name],
    [Number],
    [Description],
    [CreatedDateTime],
    [ModifiedDateTime],
    [CreatedByPersonAliasId],
    [ModifiedByPersonAliasId],
    [IsActive],
    [Order],
    [AssignedToPersonAliasId],
    [IsSmsEnabled],
    [IsSmsForwardingEnabled],
    [SmsReceivedWorkflowTypeId]
)
SELECT
    [DV].[Guid],
    CASE ISNULL([DV].[Description], '') WHEN '' THEN [DV].[Value] ELSE SUBSTRING([DV].[Description], 1, 100) END,
    [DV].[Value],
    [DV].[Description],
    [DV].[CreatedDateTime],
    [DV].[ModifiedDateTime],
    [DV].[CreatedByPersonAliasId],
    [DV].[ModifiedByPersonAliasId],
    [DV].[IsActive],
    [DV].[Order],
    (SELECT TOP 1
        [PA].[Id]
        FROM [AttributeValue] AS [AV]
        LEFT JOIN [PersonAlias] AS [PA] ON [PA].[Guid] = TRY_CAST([AV].[Value] AS uniqueidentifier)
        WHERE [AV].[EntityId] = [DV].[Id] AND [AV].[AttributeId] = @ResponseRecipientAttributeId
    ) AS [RecipientAliasId],
    1,
    ISNULL((SELECT TOP 1
        CASE [AV].[Value] WHEN 'True' THEN 1 ELSE 0 END
        FROM [AttributeValue] AS [AV]
        WHERE [AV].[EntityId] = [DV].[Id] AND [AV].[AttributeId] = @EnableResponseRecipientForwardingAttributeId
    ), 1) AS [EnableForwarding],
    (SELECT TOP 1
        [WT].[Id]
        FROM [AttributeValue] AS [AV]
        LEFT JOIN [WorkflowType] AS [WT] ON [WT].[Guid] = TRY_CAST([AV].[Value] AS uniqueidentifier)
        WHERE [AV].[EntityId] = [DV].[Id] AND [AV].[AttributeId] = @LaunchWorkflowOnResponseReceivedAttributeId
    ) AS [WorkflowTypeId]
FROM [DefinedValue] AS [DV]
WHERE [DV].[DefinedTypeId] = @PhoneNumberDefinedTypeId

-- Copy explicit authorization rules from DefinedValue to SystemPhoneNumber.
-- Any inherited rules from the DefinedType will be ignored for this use case.
INSERT INTO [Auth] (
    [EntityTypeId],
    [EntityId],
    [Order],
    [Action],
    [AllowOrDeny],
    [SpecialRole],
    [GroupId],
    [PersonAliasId],
    [Guid],
    [CreatedDateTime],
    [ModifiedDateTime],
    [CreatedByPersonAliasId],
    [ModifiedByPersonAliasId],
    [ForeignGuid]
)
SELECT
    @SystemPhoneNumberEntityTypeId,
    (SELECT
        TOP 1 [SPN].[Id]
        FROM [SystemPhoneNumber] AS [SPN]
        INNER JOIN [DefinedValue] AS [DV] ON [DV].[Guid] = [SPN].[Guid]
        WHERE [DV].[Id] = [A].[EntityId]
    ),
    [Order],
    [Action],
    [AllowOrDeny],
    [SpecialRole],
    [GroupId],
    [PersonAliasId],
    NEWID(),
    [CreatedDateTime],
    [ModifiedDateTime],
    [CreatedByPersonAliasId],
    [ModifiedByPersonAliasId],
    [Guid]
FROM [Auth] AS [A]
WHERE [A].[EntityTypeId] = @DefinedValueEntityTypeId
  AND [A].[EntityId] IN (SELECT [Id] FROM [DefinedValue] WHERE [DefinedTypeId] = @PhoneNumberDefinedTypeId)
" );

            Sql( @"
UPDATE [DefinedType]
SET [HelpText] = '<div class=""alert alert-warning"">
    The phone numbers here are obsolete and have been migrated to their own <a href=""/admin/communications/system-phone-numbers"">page</a>. Changes made here will be lost.
</div>'
WHERE [Guid] = '611bde1f-7405-4d16-8626-ccfedb0e62be'
" );
        }

        /// <summary>
        /// Makes the pages and blocks changes required to add the SystemPhoneNumber feature.
        /// </summary>
        private void PagesAndBlocksUp()
        {
            // Remove old blocks from the SMS Phone Numbers page.
            RockMigrationHelper.DeleteBlock( "6DF11D72-96ED-415B-BACA-1A4390CAA4D7" );
            RockMigrationHelper.DeleteBlock( "BD89E08E-97E3-4EFC-A299-8F0956A1B5EF" );

            // Update Page
            //  Internal Name: SMS Phone Numbers
            //  New Name: System Phone Numbers
            //  New Icon: fa-phone-square
            Sql( @"
UPDATE [Page]
SET [InternalName] = 'System Phone Numbers',
    [PageTitle] = 'System Phone Numbers',
    [BrowserTitle] = 'System Phone Numbers',
    [IconCssClass] = 'fa fa-phone-square'
WHERE [Guid] = '3F1EA6E5-6C61-444A-A80E-5B66F96F521B'" );

            // Add Page 
            //  Internal Name: System Phone Number Detail
            //  Site: Rock RMS
            RockMigrationHelper.AddPage( true, "3F1EA6E5-6C61-444A-A80E-5B66F96F521B", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "System Phone Number Detail", "", "B2F7C5C3-2AD0-4CF5-B297-D66E97CCD306", "" );

            // Add/Update BlockType 
            //   Name: System Phone Number List
            //   Category: Communication
            //   Path: ~/Blocks/Communication/SystemPhoneNumberList.ascx
            //   EntityType: -
            RockMigrationHelper.UpdateBlockType( "System Phone Number List", "Lists the phone numbers currently in the system.", "~/Blocks/Communication/SystemPhoneNumberList.ascx", "Communication", "72C74D98-D80F-4EEE-BD14-6308EA565D7A" );

            // Add/Update Obsidian Block Entity Type
            //   EntityType:Rock.Blocks.Communication.SystemPhoneNumberDetail
            RockMigrationHelper.UpdateEntityType( "Rock.Blocks.Communication.SystemPhoneNumberDetail", "System Phone Number Detail", "Rock.Blocks.Communication.SystemPhoneNumberDetail, Rock.Blocks, Version=1.15.0.11, Culture=neutral, PublicKeyToken=null", false, false, "C6D57E79-B8B1-40E7-89B1-C0DB133DD263" );

            // Add/Update Obsidian Block Type
            //   Name:System Phone Number Detail
            //   Category:Communication
            //   EntityType:Rock.Blocks.Communication.SystemPhoneNumberDetail
            RockMigrationHelper.UpdateMobileBlockType( "System Phone Number Detail", "Displays the details of a particular system phone number.", "Rock.Blocks.Communication.SystemPhoneNumberDetail", "Communication", "D02BCBFB-B148-4073-8AFC-8419B48A1B16" );

            // Add Block 
            //  Block Name: System Phone Number List
            //  Page Name: System Phone Numbers
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "3F1EA6E5-6C61-444A-A80E-5B66F96F521B".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "72C74D98-D80F-4EEE-BD14-6308EA565D7A".AsGuid(), "System Phone Number List", "Main", @"", @"", 2, "33CB0853-3689-4391-8F19-F46DEE5A8678" );

            // Add Block 
            //  Block Name: System Phone Number Detail
            //  Page Name: System Phone Number Detail
            //  Layout: -
            //  Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "B2F7C5C3-2AD0-4CF5-B297-D66E97CCD306".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "D02BCBFB-B148-4073-8AFC-8419B48A1B16".AsGuid(), "System Phone Number Detail", "Main", @"", @"", 0, "0C0533BD-3618-48CF-87DB-AF76A6002ED6" );

            // Attribute for BlockType
            //   BlockType: System Phone Number List
            //   Category: Communication
            //   Attribute: System Phone Number Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "72C74D98-D80F-4EEE-BD14-6308EA565D7A", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "System Phone Number Detail Page", "SystemPhoneNumberDetailPage", "System Phone Number Detail Page", @"", 0, @"", "A944D80F-04BB-40F6-B426-A674A07F2384" );

            // Add Block Attribute Value
            //   Block: System Phone Number List
            //   BlockType: System Phone Number List
            //   Category: Communication
            //   Block Location: Page=System Phone Numbers, Site=Rock RMS
            //   Attribute: System Phone Number Detail Page
            /*   Attribute Value: b2f7c5c3-2ad0-4cf5-b297-d66e97ccd306 */
            RockMigrationHelper.AddBlockAttributeValue( "33CB0853-3689-4391-8F19-F46DEE5A8678", "A944D80F-04BB-40F6-B426-A674A07F2384", @"b2f7c5c3-2ad0-4cf5-b297-d66e97ccd306" );

            RockMigrationHelper.UpdateFieldType( "System Phone Number", "", "Rock", "Rock.Field.Types.SystemPhoneNumberFieldType", "B8C35BA7-85E9-4512-B99C-12DE697DE14E" );
        }

        /// <summary>
        /// Makes the pages and blocks changes required to remove the SystemPhoneNumber feature.
        /// </summary>
        private void PagesAndBlocksDown()
        {
            // Attribute for BlockType
            //   BlockType: System Phone Number List
            //   Category: Communication
            //   Attribute: System Phone Number Detail Page
            RockMigrationHelper.DeleteAttribute( "A944D80F-04BB-40F6-B426-A674A07F2384" );

            // Remove Block
            //  Name: System Phone Number Detail, from Page: System Phone Number Detail, Site: Rock RMS
            //  from Page: System Phone Number Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "0C0533BD-3618-48CF-87DB-AF76A6002ED6" );

            // Remove Block
            //  Name: System Phone Number List, from Page: System Phone Numbers, Site: Rock RMS
            //  from Page: System Phone Numbers, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "33CB0853-3689-4391-8F19-F46DEE5A8678" );

            // Delete BlockType 
            //   Name: System Phone Number List
            //   Category: Communication
            //   Path: ~/Blocks/Communication/SystemPhoneNumberList.ascx
            //   EntityType: -
            RockMigrationHelper.DeleteBlockType( "72C74D98-D80F-4EEE-BD14-6308EA565D7A" );

            // Delete BlockType 
            //   Name: System Phone Number Detail
            //   Category: Communication
            //   Path: -
            //   EntityType: System Phone Number Detail
            RockMigrationHelper.DeleteBlockType( "D02BCBFB-B148-4073-8AFC-8419B48A1B16" );

            // Delete Page 
            //  Internal Name: System Phone Number Detail
            //  Site: Rock RMS
            //  Layout: Full Width
            RockMigrationHelper.DeletePage( "B2F7C5C3-2AD0-4CF5-B297-D66E97CCD306" );

            // Restore Page
            //  Internal Name: SMS Phone Numbers
            Sql( @"
UPDATE [Page]
SET [InternalName] = 'SMS Phone Numbers',
    [PageTitle] = 'SMS Phone Numbers',
    [BrowserTitle] = 'SMS Phone Numbers',
    [IconCssClass] = 'fa fa-mobile-phone'
WHERE [Guid] = '3F1EA6E5-6C61-444A-A80E-5B66F96F521B'" );

            // Restore Block
            //  Block Name: Defined Type Detail
            //  Page Name: SMS Phone Numbers
            RockMigrationHelper.AddBlock( true, "3F1EA6E5-6C61-444A-A80E-5B66F96F521B".AsGuid(), null, null, "08C35F15-9AF7-468F-9D50-CDFD3D21220C".AsGuid(), "Defined Type Detail", "Main", "", "", 0, "BD89E08E-97E3-4EFC-A299-8F0956A1B5EF" );

            // Restore Block
            //  Block Name: Defined Value List
            //  Page Name: SMS Phone Numbers
            RockMigrationHelper.AddBlock( true, "3F1EA6E5-6C61-444A-A80E-5B66F96F521B".AsGuid(), null, null, "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE".AsGuid(), "Defined Value List", "Main", "", "", 0, "6DF11D72-96ED-415B-BACA-1A4390CAA4D7" );

            // Restore Block Attribute Value
            //   Block: Defined Type Detail
            //   Attribute: Defined Type
            RockMigrationHelper.AddBlockAttributeValue( "BD89E08E-97E3-4EFC-A299-8F0956A1B5EF", "0305ef98-c791-4626-9996-f189b9bb674c", @"611BDE1F-7405-4D16-8626-CCFEDB0E62BE" );

            // Restore Block Attribute Value
            //   Block: Defined Value List
            //   Attribute: Defined Type
            RockMigrationHelper.AddBlockAttributeValue( "6DF11D72-96ED-415B-BACA-1A4390CAA4D7", "9280d61f-c4f3-4a3e-a9bb-bcd67ff78637", "611BDE1F-7405-4D16-8626-CCFEDB0E62BE" );
        }

        /// <summary>
        /// Makes the page route changes required to add the SystemPhoneNumber feature.
        /// </summary>
        private void PageRoutesUp()
        {
            // If the old system route exists then delete it. We then re-create
            // it after our new route so the new route becomes the default route.
            Sql( @"
DECLARE @PageId INT = (SELECT [Id] FROM [Page] WHERE [Guid] = '3f1ea6e5-6c61-444a-a80e-5b66f96f521b')
DECLARE @OldRoute NVARCHAR(500) = (SELECT [Route] FROM [PageRoute] WHERE [Guid] = '44b077c9-64ac-5d18-58dd-16da7890a71c')

IF EXISTS (SELECT [Id] FROM [PageRoute] WHERE [Guid] = '44b077c9-64ac-5d18-58dd-16da7890a71c')
BEGIN
    DELETE FROM [PageRoute] WHERE [Guid] = '44b077c9-64ac-5d18-58dd-16da7890a71c'
END

INSERT INTO [PageRoute] ([IsSystem], [PageId], [Route], [Guid], [IsGlobal])
VALUES
    (1, @PageId, 'admin/communications/system-phone-numbers', 'eaad1efc-0e4c-49e9-8fd0-a6c1bca7e64c', 0),
    (1, @PageId, ISNULL(@OldRoute, 'admin/communications/sms-numbers'), '44b077c9-64ac-5d18-58dd-16da7890a71c', 0)
" );
        }

        /// <summary>
        /// Makes the page route changes required to remove the SystemPhoneNumber feature.
        /// </summary>
        private void PageRoutesDown()
        {
            // Delete the new page route, this will make the old page route
            // the default again.
            Sql( @"DELETE FROM [PageRoute] WHERE [Guid] = 'eaad1efc-0e4c-49e9-8fd0-a6c1bca7e64c'" );
        }

        /// <summary>
        /// Makes the field type changes required to add the SystemPhoneNumber feature.
        /// </summary>
        private void FieldTypesUp()
        {
            RockMigrationHelper.UpdateFieldType( "System Phone Number", "", "Rock", "Rock.Field.Types.SystemPhoneNumberFieldType", "B8C35BA7-85E9-4512-B99C-12DE697DE14E" );
        }

        /// <summary>
        /// Makes the field type changes required to remove the SystemPhoneNumber feature.
        /// </summary>
        private void FieldTypesDown()
        {
            // Do not delete the field type since there could be references to it.
        }

        /// <summary>
        /// Adds the job required to add the SystemPhoneNumber feature.
        /// </summary>
        private void PostUpdateJobUp()
        {
            // add ServiceJob: Rock Update Helper v15.0 - System Phone Numbers
            Sql( $@"IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.PostV15DataMigrationsSystemPhoneNumbers' AND [Guid] = '6DFE731E-F28B-40B3-8383-84212A301214' )
            BEGIN
               INSERT INTO [ServiceJob] (
                  [IsSystem]
                  ,[IsActive]
                  ,[Name]
                  ,[Description]
                  ,[Class]
                  ,[CronExpression]
                  ,[NotificationStatus]
                  ,[Guid] )
               VALUES ( 
                  0
                  ,1
                  ,'Rock Update Helper v15.0 - System Phone Numbers'
                  ,'This job will add indexes related to SystemPhoneNumber conversion.'
                  ,'Rock.Jobs.PostV15DataMigrationsSystemPhoneNumbers'
                  ,'0 0 21 1/1 * ? *'
                  ,1
                  ,'{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_150_SYSTEM_PHONE_NUMBERS}'
                  );
            END" );
        }

        /// <summary>
        /// Removes the job required to add the SystemPhoneNumber feature.
        /// </summary>
        private void PostUpdateJobDown()
        {
            Sql( $"DELETE FROM [ServiceJob] WHERE [Guid] = '{Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_150_SYSTEM_PHONE_NUMBERS}'" );
        }
    }
}
