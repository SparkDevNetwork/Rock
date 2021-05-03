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
    using System.Collections.Generic;

    /// <summary>
    /// Modify the data model to rename SystemEmail to SystemCommunication.
    /// </summary>
    public partial class AddSystemCommunication : Rock.Migrations.RockMigration
    {
        private const string ObsoleteSystemGuid_Page_SYSTEM_EMAILS = "89B7A631-EA6F-4DA3-9380-04EE67B63E9E";
        private const string ObsoleteSystemGuid_Page_SYSTEM_EMAIL_DETAILS = "588C72A8-7DEC-405F-BA4A-FE64F87CB817";
        private const string ObsoleteSystemGuid_FieldType_SYSTEM_EMAIL = "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF";

        private List<Guid> _WellKnownEntryGuidList;
        private List<string> _BlocksHavingSystemEmailFieldTypeAttributeList;

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            InitializeLocalContext();
            SystemCommunicationApplyTableChangesUp();
            SystemCommunicationAddSupportDataUp();
            SystemCommunicationAddCategoriesUp();
            UpdateAttributeFieldTypesUp();
            SystemCommunicationPagesAndBlocksUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            InitializeLocalContext();
            SystemCommunicationModifyDatabaseObjectsDown();
            UpdateAttributeFieldTypesDown();
            SystemCommunicationPagesAndBlocksDown();
            SystemCommunicationDeleteDatabaseObjectsDown();
        }

        /// <summary>
        /// Initialize the local execution context for this migration.
        /// </summary>
        private void InitializeLocalContext()
        {
            // Get the list of SystemCommunication identifiers that are well-known by Rock.
            // These identifiers are the same for the corresponding SystemEmail template.
            _WellKnownEntryGuidList = new List<Guid>()
            {
                SystemGuid.SystemCommunication.ASSESSMENT_REQUEST.AsGuid(),
                SystemGuid.SystemCommunication.ATTENDANCE_NOTIFICATION.AsGuid(),
                SystemGuid.SystemCommunication.CONFIG_EXCEPTION_NOTIFICATION.AsGuid(),
                SystemGuid.SystemCommunication.CONFIG_JOB_NOTIFICATION.AsGuid(),
                SystemGuid.SystemCommunication.FINANCE_PLEDGE_CONFIRMATION.AsGuid(),
                SystemGuid.SystemCommunication.GROUP_ATTENDANCE_REMINDER.AsGuid(),
                SystemGuid.SystemCommunication.NOTE_WATCH_NOTIFICATION.AsGuid(),
                SystemGuid.SystemCommunication.NOTE_APPROVAL_NOTIFICATION.AsGuid(),
                SystemGuid.SystemCommunication.PRAYER_REQUEST_COMMENTS_NOTIFICATION.AsGuid(),
                SystemGuid.SystemCommunication.REGISTRATION_NOTIFICATION.AsGuid(),
                SystemGuid.SystemCommunication.SCHEDULING_CONFIRMATION.AsGuid(),
                SystemGuid.SystemCommunication.SCHEDULING_REMINDER.AsGuid(),
                SystemGuid.SystemCommunication.SCHEDULING_RESPONSE.AsGuid(),
                SystemGuid.SystemCommunication.SECURITY_ACCOUNT_CREATED.AsGuid(),
                SystemGuid.SystemCommunication.SECURITY_CONFIRM_ACCOUNT.AsGuid(),
                SystemGuid.SystemCommunication.SECURITY_FORGOT_USERNAME.AsGuid(),
                SystemGuid.SystemCommunication.SPARK_DATA_NOTIFICATION.AsGuid(),
                SystemGuid.SystemCommunication.WORKFLOW_FORM_NOTIFICATION.AsGuid(),
                SystemGuid.SystemCommunication.COMMUNICATION_QUEUE.AsGuid(),
                SystemGuid.SystemCommunication.FINANCE_EXPIRING_CREDIT_CARD.AsGuid(),
                SystemGuid.SystemCommunication.FINANCE_FAILED_PAYMENT.AsGuid(),
                SystemGuid.SystemCommunication.FINANCE_GIVING_RECEIPT.AsGuid(),
                SystemGuid.SystemCommunication.FOLLOWING_EVENT.AsGuid(),
                SystemGuid.SystemCommunication.FOLLOWING_SUGGESTION.AsGuid(),
                SystemGuid.SystemCommunication.GROUP_MEMBER_ABSENCE.AsGuid(),
                SystemGuid.SystemCommunication.GROUP_REQUIREMENTS.AsGuid(),
                SystemGuid.SystemCommunication.GROUP_PENDING_MEMBERS.AsGuid(),
                SystemGuid.SystemCommunication.GROUP_SYNC_WELCOME.AsGuid(),
                SystemGuid.SystemCommunication.GROUP_SYNC_EXIT.AsGuid(),
                SystemGuid.SystemCommunication.KIOSK_INFO_UPDATE.AsGuid()
            };

            // Get the list of blocks that use the SystemEmailFieldType as a configuration setting.
            _BlocksHavingSystemEmailFieldTypeAttributeList = new List<string>
            {
                "~/Blocks/Crm/PersonUpdate.Kiosk.ascx",
                "~/Blocks/Event/RegistrationEntry.ascx",
                "~/Blocks/Finance/PledgeEntry.ascx",
                "~/Blocks/Finance/ScheduledPaymentDownload.ascx",
                "~/Blocks/Finance/TransactionEntry.ascx",
                "~/Blocks/Finance/TransactionEntry.Kiosk.ascx",
                "~/Blocks/Finance/TransactionEntryV2.ascx",
                "~/Blocks/Groups/GroupAttendanceDetail.ascx",
                "~/Blocks/Groups/GroupScheduleConfirmation.ascx",
                "~/Blocks/Groups/GroupSimpleRegister.ascx",
                "~/Blocks/GroupScheduling/GroupScheduleConfirmation.ascx",
                "~/Blocks/Security/AccountEntry.ascx",
                "~/Blocks/Security/ForgotUserName.ascx",
                "~/Blocks/Security/Login.ascx"
            };
        }

        #region Migration Up

        /// <summary>
        /// Add the SystemCommunication table to the database and transfer entries from the SystemEmail table.
        /// </summary>
        private void SystemCommunicationApplyTableChangesUp()
        {
            // Changes for: SystemCommunication
            CreateTable(
                "dbo.SystemCommunication",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    IsSystem = c.Boolean( nullable: false ),
                    IsActive = c.Boolean(),
                    CategoryId = c.Int(),
                    Title = c.String( nullable: false, maxLength: 100 ),
                    From = c.String( maxLength: 200 ),
                    FromName = c.String( maxLength: 200 ),
                    To = c.String(),
                    Cc = c.String(),
                    Bcc = c.String(),
                    Subject = c.String( nullable: false, maxLength: 1000 ),
                    Body = c.String( nullable: false ),
                    SMSMessage = c.String(),
                    SMSFromDefinedValueId = c.Int(),
                    PushTitle = c.String( maxLength: 100 ),
                    PushMessage = c.String(),
                    PushSound = c.String( maxLength: 100 ),
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
                .ForeignKey( "dbo.Category", t => t.CategoryId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.CategoryId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CopyEmailsToCommunications();
            SetSystemCommunicationProperties();

            /*
             * Modify names and foreign key references from SystemEmail to SystemCommunication.
             */

            // GroupType Table
            AddColumn( "dbo.GroupType", "ScheduleConfirmationSystemCommunicationId", c => c.Int() );
            AddColumn( "dbo.GroupType", "ScheduleReminderSystemCommunicationId", c => c.Int() );
            AddForeignKey( "dbo.GroupType", "ScheduleConfirmationSystemCommunicationId", "dbo.SystemCommunication", "Id" );
            AddForeignKey( "dbo.GroupType", "ScheduleReminderSystemCommunicationId", "dbo.SystemCommunication", "Id" );
            AddColumn( "dbo.GroupType", "RSVPReminderSystemCommunicationId", c => c.Int() );
            AddColumn( "dbo.GroupType", "RSVPReminderOffsetDays", c => c.Int() );

            CopyEmailKeysToCommunicationKeysUp( "dbo.GroupType", "ScheduleConfirmationSystemEmailId", "ScheduleConfirmationSystemCommunicationId" );
            CopyEmailKeysToCommunicationKeysUp( "dbo.GroupType", "ScheduleReminderSystemEmailId", "ScheduleReminderSystemCommunicationId" );

            // Group Table
            AddColumn( "dbo.Group", "RSVPReminderSystemCommunicationId", c => c.Int() );
            AddColumn( "dbo.Group", "RSVPReminderOffsetDays", c => c.Int() );

            // GroupSync Table
            AddColumn( "dbo.GroupSync", "WelcomeSystemCommunicationId", c => c.Int() );
            AddColumn( "dbo.GroupSync", "ExitSystemCommunicationId", c => c.Int() );
            AddForeignKey( "dbo.GroupSync", "WelcomeSystemCommunicationId", "dbo.SystemCommunication" );
            AddForeignKey( "dbo.GroupSync", "ExitSystemCommunicationId", "dbo.SystemCommunication" );

            CopyEmailKeysToCommunicationKeysUp( "dbo.GroupSync", "WelcomeSystemEmailId", "WelcomeSystemCommunicationId" );
            CopyEmailKeysToCommunicationKeysUp( "dbo.GroupSync", "ExitSystemEmailId", "ExitSystemCommunicationId" );

            // SignatureDocumentTemplate Table
            AddColumn( "dbo.SignatureDocumentTemplate", "InviteSystemCommunicationId", c => c.Int() );
            AddForeignKey( "dbo.SignatureDocumentTemplate", "InviteSystemCommunicationId", "dbo.SystemCommunication" );

            CopyEmailKeysToCommunicationKeysUp( "dbo.SignatureDocumentTemplate", "InviteSystemEmailId", "InviteSystemCommunicationId" );

            // WorkflowActionForm Table
            AddColumn( "dbo.WorkflowActionForm", "NotificationSystemCommunicationId", c => c.Int() );
            AddForeignKey( "dbo.WorkflowActionForm", "NotificationSystemCommunicationId", "dbo.SystemCommunication" );

            CopyEmailKeysToCommunicationKeysUp( "dbo.WorkflowActionForm", "NotificationSystemEmailId", "NotificationSystemCommunicationId" );

            RockMigrationHelper.UpdateFieldType( "System Communication", "Field Type to select a system communication", "Rock", "Rock.Field.Types.SystemCommunicationFieldType", SystemGuid.FieldType.SYSTEM_COMMUNICATION );
        }

        /// <summary>
        /// Duplicate the SystemEmail Category hierarchy for the SystemCommunication entity type.
        /// </summary>
        private void SystemCommunicationAddCategoriesUp()
        {
            Sql( $@"
DECLARE @sourceCategoryID int
DECLARE @targetCategoryID int

DECLARE @IdMap TABLE
(
    SourceId int
    ,TargetId int
)

DECLARE @SourceCategoriesTable TABLE
(
    [EntryNumber] int identity(1,1)
    ,[CategoryId] int
    ,[Name] nvarchar(100)
    ,[ParentCategoryId] int
    ,[IsSystem] bit
    ,[Order] int
    ,[IconCssClass] nvarchar(100)
    ,[Description] nvarchar(MAX)
    ,[CreatedDateTime] datetime
    ,[ForeignKey] nvarchar(100)
    ,[HighlightColor] nvarchar(50)
    ,[ForeignGuid] uniqueidentifier
    ,[ForeignId] int
)

DECLARE @emailEntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.SystemEmail');
DECLARE @communicationEntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.SystemCommunication');

-- Copy all Categories that relate to Entity Type 'System Email' to a temporary table for processing.
INSERT INTO @SourceCategoriesTable
(
    [CategoryId]
    ,[Name]
    ,[ParentCategoryId]
    ,[IsSystem]
    ,[Order]
    ,[IconCssClass]
    ,[Description]
    ,[CreatedDateTime]
    ,[ForeignKey]
    ,[HighlightColor]
    ,[ForeignGuid]
    ,[ForeignId]
)
SELECT [Id]
    ,[Name]
    ,[ParentCategoryId]
    ,[IsSystem]
    ,[Order]
    ,[IconCssClass]
    ,[Description]
    ,[CreatedDateTime]
    ,[ForeignKey]
    ,[HighlightColor]
    ,[ForeignGuid]
    ,[ForeignId]
FROM [Category]
WHERE [EntityTypeId] = @emailEntityTypeId

-- Create new Category records from the temporary table.
-- The new Categories are associated with the 'System Communication' Entity Type.
DECLARE @counter int = 1

WHILE @counter <= (SELECT MAX([EntryNumber]) from @SourceCategoriesTable)
BEGIN
    INSERT INTO [Category]
    (  
        [Name]
        ,[ParentCategoryId]
        ,[EntityTypeId]
        ,[Guid]
        ,[IsSystem]
        ,[Order]
        ,[IconCssClass]
        ,[Description]
        ,[CreatedDateTime]
        ,[ForeignKey]
        ,[HighlightColor]
        ,[ForeignGuid]
        ,[ForeignId]
    )  
    SELECT TOP 1
        [Name]
        ,[ParentCategoryId]
        ,@communicationEntityTypeId
        ,NEWID()
        ,[IsSystem]
        ,[Order]
        ,[IconCssClass]
        ,[Description]
        ,ISNULL([CreatedDateTime], GETDATE())
        ,[ForeignKey]
        ,[HighlightColor]
        ,[ForeignGuid]
        ,[ForeignId]
    FROM @SourceCategoriesTable
    WHERE [EntryNumber] = @counter

    -- Get Id for source record and new record.
    SET @targetCategoryID = SCOPE_IDENTITY()

    SELECT TOP 1
        @sourceCategoryID = [CategoryId]
    FROM @SourceCategoriesTable
    WHERE [EntryNumber] = @counter

    -- Store a map entry for source Category ID to target Category ID.
    INSERT INTO @IdMap
    (
        [SourceId]
        ,[TargetId]
    )
    VALUES
    (
        @sourceCategoryID
        ,@targetCategoryID
    )

    SET @counter = @counter + 1
END

-- Update the parent links for the new Categories using the stored mappings.
UPDATE [Category]
	SET [ParentCategoryId] = parentMap.[TargetId]
FROM Category
	INNER JOIN @IdMap AS targetMap ON [Category].[Id] = targetMap.[TargetId]
	INNER JOIN @IdMap AS parentMap ON [Category].[ParentCategoryId] = parentMap.[SourceId]

-- Update the Categories for the new System Communication records using the stored mappings.
UPDATE [SystemCommunication]
	SET [CategoryId] = targetMap.[TargetId]
FROM SystemCommunication
	INNER JOIN @IdMap AS targetMap ON [SystemCommunication].[CategoryId] = targetMap.[SourceId]
" );

            // Add new RSVP Confirmation Category
            RockMigrationHelper.UpdateCategory( SystemGuid.EntityType.SYSTEM_COMMUNICATION, "RSVP Confirmation", "", "", SystemGuid.Category.SYSTEM_COMMUNICATION_RSVP_CONFIRMATION );
        }

        /// <summary>
        /// Copy records in the SystemEmail table to the SystemCommunication table.
        /// </summary>
        private void CopyEmailsToCommunications()
        {
            Sql( $@"
SET IDENTITY_INSERT [SystemCommunication] ON
" );

            Sql( $@"
INSERT INTO [SystemCommunication]
    (
      [Id]
      ,[IsSystem]
      ,[Title]
      ,[From]
      ,[To]
      ,[Cc]
      ,[Bcc]
      ,[Subject]
      ,[Body]
      ,[Guid]
      ,[CreatedDateTime]
      ,[ModifiedDateTime]
      ,[CreatedByPersonAliasId]
      ,[ModifiedByPersonAliasId]
      ,[FromName]
      ,[ForeignKey]
      ,[CategoryId]
      ,[ForeignGuid]
      ,[ForeignId]
    )
    SELECT
      [Id]
      ,[IsSystem]
      ,[Title]
      ,[From]
      ,[To]
      ,[Cc]
      ,[Bcc]
      ,[Subject]
      ,[Body]
      ,[Guid]
      ,[CreatedDateTime]
      ,[ModifiedDateTime]
      ,[CreatedByPersonAliasId]
      ,[ModifiedByPersonAliasId]
      ,[FromName]
      ,[ForeignKey]
      ,[CategoryId]
      ,[ForeignGuid]
      ,[ForeignId]
    FROM [SystemEmail]
" );

            Sql( $@"
    SET IDENTITY_INSERT [SystemCommunication] OFF
" );
        }

        /// <summary>
        /// Copy foreign key references from an EmailId field to a CommunicationId field for well-known system templates.
        /// [Id] values are identical for records in both the SystemEmail and SystemCommunication tables.
        /// </summary>
        private void CopyEmailKeysToCommunicationKeysUp( string tableName, string emailIdField, string communicationIdField )
        {
            string sql;

            sql = $@"
UPDATE { tableName }
        SET { communicationIdField } = { emailIdField }
WHERE [Guid] IN ( '{ _WellKnownEntryGuidList.AsDelimited( "','" ) }' )
            ";

            Sql( sql );
        }

        /// <summary>
        /// Returns a list of guid identifiers for core blocks that have an Attribute of type "SystemEmailFieldType".
        /// </summary>
        private void UpdateAttributeFieldTypesUp()
        {
            string sql;

            // Update core Block Attributes that reference SystemEmailFieldType to use SystemCommunicationFieldType.
            sql = $@"
DECLARE @oldFieldTypeId int = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{ ObsoleteSystemGuid_FieldType_SYSTEM_EMAIL }')
DECLARE @newFieldTypeId int = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{ SystemGuid.FieldType.SYSTEM_COMMUNICATION }')

UPDATE [Attribute]
SET [FieldTypeId] = @newFieldTypeId
WHERE [Id] IN (
    SELECT a.[Id]
    FROM [Attribute] a
        LEFT JOIN [BlockType] bt ON TRY_CAST( a.[EntityTypeQualifierValue] as int ) = bt.[Id]
    WHERE a.[FieldTypeId] = @oldFieldTypeId
        AND a.[EntityTypeQualifierColumn] = 'BlockTypeId'
        AND bt.[Path] IN ( '{ _BlocksHavingSystemEmailFieldTypeAttributeList.AsDelimited( "','" ) }' )
    )    
";

            Sql( sql );

            // Update core Job Attributes that reference SystemEmailFieldType to use SystemCommunicationFieldType.
            sql = $@"
DECLARE @oldFieldTypeId int = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{ ObsoleteSystemGuid_FieldType_SYSTEM_EMAIL }')
DECLARE @newFieldTypeId int = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '{ SystemGuid.FieldType.SYSTEM_COMMUNICATION }')

UPDATE [Attribute]
SET [FieldTypeId] = @newFieldTypeId
WHERE [Id] IN (
    SELECT [Id]
    FROM [Attribute]
    WHERE [FieldTypeId] = @oldFieldTypeId
        AND [EntityTypeQualifierColumn] = 'Class'
        AND [EntityTypeQualifierValue] LIKE 'Rock.Jobs.%'
    )
";

            Sql( sql );

            // Update core Workflow Action Attributes that reference SystemEmailFieldType to use SystemCommunicationFieldType.
            var sendSystemEmailWorkflowActionGuid = "4487702A-BEAF-4E5A-92AD-71A1AD48DFCE";

            sql = $@"
DECLARE @oldFieldTypeId int = (SELECT [Id] FROM FieldType WHERE [Guid] = '{ ObsoleteSystemGuid_FieldType_SYSTEM_EMAIL }')
DECLARE @newFieldTypeId int = (SELECT [Id] FROM FieldType WHERE [Guid] = '{ SystemGuid.FieldType.SYSTEM_COMMUNICATION }')
DECLARE @actionEntityTypeId int = (SELECT [Id] FROM EntityType WHERE [Guid] = '{ SystemGuid.EntityType.WORKFLOW_ACTION_TYPE }')
DECLARE @sendActionEntityTypeId int = (SELECT [Id] FROM EntityType WHERE [Guid] = '{ sendSystemEmailWorkflowActionGuid }')

UPDATE [Attribute]
SET [FieldTypeId] = @newFieldTypeId
WHERE [Id] IN (
    SELECT [Id]
    FROM [Attribute]
    WHERE [FieldTypeId] = @oldFieldTypeId
        AND [EntityTypeId] = @actionEntityTypeId
        AND [EntityTypeQualifierColumn] = 'EntityTypeId'
        AND [EntityTypeQualifierValue] = LTRIM(STR(@sendActionEntityTypeId))
    )    
";

            Sql( sql );
        }

        /// <summary>
        /// Update SystemCommunication default property values.
        /// </summary>
        private void SetSystemCommunicationProperties()
        {
            // Update SystemCommunication entries that are well-known by Rock to ensure the IsSystem flag is set.
            Sql( $@"
UPDATE [SystemCommunication]
       SET [IsSystem] = 1
WHERE [Guid] IN ( '{ _WellKnownEntryGuidList.AsDelimited( "','" ) }' )
" );

            // Set the IsActive flag for all SystemCommunication entries.
            Sql( $@"
UPDATE [SystemCommunication]
       SET [IsActive] = 1
" );
        }

        /// <summary>
        /// Add support data for the System Communications feature.
        /// </summary>
        private void SystemCommunicationAddSupportDataUp()
        {
            RockMigrationHelper.UpdateEntityType( "Rock.Model.SystemCommunication", "System Communication", "Rock.Model.SystemCommunication, Rock, Version=1.10.2.1, Culture=neutral, PublicKeyToken=null", true, true, SystemGuid.EntityType.SYSTEM_COMMUNICATION );
        }

        /// <summary>
        /// Add SystemCommunication pages and blocks and rename SystemEmail pages and blocks.
        /// </summary>
        private void SystemCommunicationPagesAndBlocksUp()
        {
            // Add New Block Types.
            RockMigrationHelper.UpdateBlockType( "System Communication List", "Lists the system communications that can be configured for use by the system and other automated (non-user) tasks.", "~/Blocks/Communication/SystemCommunicationList.ascx", "Communication", SystemGuid.BlockType.SYSTEM_COMMUNICATION_LIST );
            RockMigrationHelper.AddNewBlockTypeAttribute( SystemGuid.BlockType.SYSTEM_COMMUNICATION_LIST, SystemGuid.FieldType.PAGE_REFERENCE, "Detail Page", "DetailPage", "Detail Page", "", 0, SystemGuid.Page.SYSTEM_COMMUNICATION_DETAIL, SystemGuid.Attribute.SYSTEM_COMMUNICATION_LIST_DETAIL_PAGE, false );
            RockMigrationHelper.UpdateBlockType( "System Communication Detail", "Allows the administration of a system communication.", "~/Blocks/Communication/SystemCommunicationDetail.ascx", "Communication", SystemGuid.BlockType.SYSTEM_COMMUNICATION_DETAIL );

            // Add System Communication List.
            RockMigrationHelper.AddPage( true, SystemGuid.Page.COMMUNICATIONS_ROCK_SETTINGS, SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "System Communications", "", SystemGuid.Page.SYSTEM_COMMUNICATION_LIST, "fa fa-comments-o", SystemGuid.Page.COMMUNICATION_TEMPLATES_COMMUNICATIONS );
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.SYSTEM_COMMUNICATION_LIST, null, SystemGuid.BlockType.SYSTEM_COMMUNICATION_LIST, "System Communication List", "Main", @"", @"", 0, SystemGuid.Block.SYSTEM_COMMUNICATION_LIST );
            RockMigrationHelper.AddBlockAttributeValue( SystemGuid.Block.SYSTEM_COMMUNICATION_LIST, SystemGuid.Attribute.SYSTEM_COMMUNICATION_LIST_DETAIL_PAGE, SystemGuid.Page.SYSTEM_COMMUNICATION_DETAIL );

            RockMigrationHelper.AddPageRoute( SystemGuid.Page.SYSTEM_COMMUNICATION_LIST, "Communications/System", "AB7E233D-B45E-4394-BA3A-C059FA75B29F" );

            // Add System Communication Detail.
            RockMigrationHelper.AddPage( true, SystemGuid.Page.SYSTEM_COMMUNICATION_LIST, SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE, "System Communication Details", "", SystemGuid.Page.SYSTEM_COMMUNICATION_DETAIL, "fa fa-comments-o" );
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.SYSTEM_COMMUNICATION_DETAIL, null, SystemGuid.BlockType.SYSTEM_COMMUNICATION_DETAIL, "System Communication Detail", "Main", @"", @"", 0, SystemGuid.Block.SYSTEM_COMMUNICATION_DETAIL );

            RockMigrationHelper.AddPageRoute( SystemGuid.Page.SYSTEM_COMMUNICATION_DETAIL, "Communications/System/{CommunicationId}", "5F7B7998-E83D-45D6-A3C6-1C866C509C28" );

            // Update System Communication Categories
            SetBlockAttributeValue( SystemGuid.Block.SYSTEM_COMMUNICATION_CATEGORIES, "EntityType", SystemGuid.EntityType.SYSTEM_COMMUNICATION );

            // Rename existing Pages.
            RockMigrationHelper.RenamePage( ObsoleteSystemGuid_Page_SYSTEM_EMAILS, "System Emails (Legacy)" );
            RockMigrationHelper.RenamePage( ObsoleteSystemGuid_Page_SYSTEM_EMAIL_DETAILS, "System Email Details (Legacy)" );
            RockMigrationHelper.RenamePage( SystemGuid.Page.SYSTEM_EMAIL_CATEGORIES_COMMUNICATIONS, "System Communication Categories" );
            RockMigrationHelper.RenamePage( SystemGuid.Page.SYSTEM_EMAIL_CATEGORIES_SYSTEM_EMAILS, "System Communication Categories" );
        }

        #endregion

        #region Migration Down

        /// <summary>
        /// Remove the SystemCommunication table from the database and return entries to the SystemEmail table.
        /// </summary>
        private void SystemCommunicationModifyDatabaseObjectsDown()
        {
            // Revert changes for GroupType table.
            DropColumn( "dbo.GroupType", "RSVPReminderSystemCommunicationId" );
            DropColumn( "dbo.GroupType", "RSVPReminderOffsetDays" );
            DropForeignKey( "dbo.GroupType", "ScheduleConfirmationSystemCommunicationId", "dbo.SystemCommunication" );
            DropForeignKey( "dbo.GroupType", "ScheduleReminderSystemCommunicationId", "dbo.SystemCommunication" );
            DropColumn( "dbo.GroupType", "ScheduleConfirmationSystemCommunicationId" );
            DropColumn( "dbo.GroupType", "ScheduleReminderSystemCommunicationId" );

            // Revert changes for Group table.
            DropColumn( "dbo.Group", "RSVPReminderSystemCommunicationId" );
            DropColumn( "dbo.Group", "RSVPReminderOffsetDays" );

            // Revert changes for GroupSync table.
            DropForeignKey( "dbo.GroupSync", "WelcomeSystemCommunicationId", "dbo.SystemCommunication" );
            DropForeignKey( "dbo.GroupSync", "ExitSystemCommunicationId", "dbo.SystemCommunication" );
            DropColumn( "dbo.GroupSync", "WelcomeSystemCommunicationId" );
            DropColumn( "dbo.GroupSync", "ExitSystemCommunicationId" );

            // Revert changes for SignatureDocumentTemplate table.
            DropForeignKey( "dbo.SignatureDocumentTemplate", "InviteSystemCommunicationId", "dbo.SystemCommunication" );
            DropColumn( "dbo.SignatureDocumentTemplate", "InviteSystemCommunicationId" );

            // Revert changes for WorkflowActionForm table.
            DropForeignKey( "dbo.WorkflowActionForm", "NotificationSystemCommunicationId", "dbo.SystemCommunication" );
            DropColumn( "dbo.WorkflowActionForm", "NotificationSystemCommunicationId" );
        }

        /// <summary>
        /// Remove database objects that are not required if this migration is rolled back.
        /// </summary>
        private void SystemCommunicationDeleteDatabaseObjectsDown()
        {
            // Revert changes for SystemCommunication table.
            DropTable( "dbo.SystemCommunication" );

            RockMigrationHelper.DeleteFieldType( SystemGuid.FieldType.SYSTEM_COMMUNICATION );
        }

        /// <summary>
        /// Restore pages and blocks modified by this migration.
        /// </summary>
        private void SystemCommunicationPagesAndBlocksDown()
        {
            RockMigrationHelper.RenamePage( ObsoleteSystemGuid_Page_SYSTEM_EMAILS, "System Emails" );
            RockMigrationHelper.RenamePage( ObsoleteSystemGuid_Page_SYSTEM_EMAIL_DETAILS, "System Email Details" );
            RockMigrationHelper.RenamePage( SystemGuid.Page.SYSTEM_EMAIL_CATEGORIES_COMMUNICATIONS, "System Email Categories" );
            RockMigrationHelper.RenamePage( SystemGuid.Page.SYSTEM_EMAIL_CATEGORIES_SYSTEM_EMAILS, "System Email Categories" );

            DeleteBlockTypeEx( SystemGuid.BlockType.SYSTEM_COMMUNICATION_DETAIL );
            DeleteBlockTypeEx( SystemGuid.BlockType.SYSTEM_COMMUNICATION_LIST );

            RockMigrationHelper.DeletePageRoute( "5F7B7998-E83D-45D6-A3C6-1C866C509C28" );
            RockMigrationHelper.DeletePage( SystemGuid.BlockType.SYSTEM_COMMUNICATION_DETAIL );
            RockMigrationHelper.DeletePageRoute( "AB7E233D-B45E-4394-BA3A-C059FA75B29F" );
            RockMigrationHelper.DeletePage( SystemGuid.BlockType.SYSTEM_COMMUNICATION_LIST );

            RockMigrationHelper.DeleteCategory( SystemGuid.Category.SYSTEM_COMMUNICATION_RSVP_CONFIRMATION );
        }

        /// <summary>
        /// Gets a list of guid identifiers for core blocks that have an Attribute of type "SystemEmailFieldType".
        /// </summary>
        private void UpdateAttributeFieldTypesDown()
        {
            string sql;

            // Update core Block Attributes that reference SystemEmailFieldType to use SystemCommunicationFieldType.
            sql = $@"
DECLARE @oldFieldTypeId int = (SELECT [Id] FROM FieldType WHERE [Guid] = '{ ObsoleteSystemGuid_FieldType_SYSTEM_EMAIL }')
DECLARE @newFieldTypeId int = (SELECT [Id] FROM FieldType WHERE [Guid] = '{ SystemGuid.FieldType.SYSTEM_COMMUNICATION }')

UPDATE [Attribute]
SET [FieldTypeId] = @oldFieldTypeId
WHERE [Id] IN (
    SELECT a.[Id]
    FROM [Attribute] a
        LEFT JOIN [BlockType] bt ON TRY_CAST( a.[EntityTypeQualifierValue] as int ) = bt.[Id]
    WHERE a.[FieldTypeId] = @newFieldTypeId
    AND bt.[Path] IN ( '{ _BlocksHavingSystemEmailFieldTypeAttributeList.AsDelimited( "','" ) }' )
    )    
";

            Sql( sql );

            // Update core Job Attributes that reference SystemEmailFieldType to use SystemCommunicationFieldType.
            sql = $@"
DECLARE @oldFieldTypeId int = (SELECT [Id] FROM FieldType WHERE [Guid] = '{ ObsoleteSystemGuid_FieldType_SYSTEM_EMAIL }')
DECLARE @newFieldTypeId int = (SELECT [Id] FROM FieldType WHERE [Guid] = '{ SystemGuid.FieldType.SYSTEM_COMMUNICATION }')

UPDATE [Attribute]
SET [FieldTypeId] = @oldFieldTypeId
WHERE [Id] IN (
    SELECT [Id]
    FROM [Attribute]
    WHERE [FieldTypeId] = @newFieldTypeId
        AND [EntityTypeQualifierColumn] = 'Class'
        AND [EntityTypeQualifierValue] LIKE 'Rock.Jobs.%'
    )    
";

            Sql( sql );

            // Update core Workflow Action Attributes that reference SystemEmailFieldType to use SystemCommunicationFieldType.
            var sendSystemEmailWorkflowActionGuid = "4487702A-BEAF-4E5A-92AD-71A1AD48DFCE";

            sql = $@"
DECLARE @oldFieldTypeId int = (SELECT [Id] FROM FieldType WHERE [Guid] = '{ ObsoleteSystemGuid_FieldType_SYSTEM_EMAIL }')
DECLARE @newFieldTypeId int = (SELECT [Id] FROM FieldType WHERE [Guid] = '{ SystemGuid.FieldType.SYSTEM_COMMUNICATION }')
DECLARE @actionEntityTypeId int = (SELECT [Id] FROM EntityType WHERE [Guid] = '{ SystemGuid.EntityType.WORKFLOW_ACTION_TYPE }')
DECLARE @sendActionEntityTypeId int = (SELECT [Id] FROM EntityType WHERE [Guid] = '{ sendSystemEmailWorkflowActionGuid }')

UPDATE [Attribute]
SET [FieldTypeId] = @oldFieldTypeId
WHERE [Id] IN (
    SELECT [Id]
    FROM [Attribute]
    WHERE [FieldTypeId] = @newFieldTypeId
        AND [EntityTypeId] = @actionEntityTypeId
        AND [EntityTypeQualifierColumn] = 'EntityTypeId'
        AND [EntityTypeQualifierValue] = LTRIM(STR(@sendActionEntityTypeId))
    )    
";

            Sql( sql );
        }

        #endregion

        #region Support functions

        /// <summary>
        /// Delete a Block Type and its associated Attributes.
        /// </summary>
        /// <param name="blockTypeGuid"></param>
        private void DeleteBlockTypeEx( string blockTypeGuid )
        {
            Sql( $@"
DECLARE @BlockTypeId int = (SELECT [Id] FROM [BlockType] WHERE [Guid] = '{blockTypeGuid}')
DECLARE @EntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')

-- Delete all Attributes associated with block
DELETE [Attribute]
WHERE [EntityTypeId] = @EntityTypeId
    AND [EntityTypeQualifierColumn] = 'BlockTypeId'
    AND [EntityTypeQualifierValue] = CAST(@BlockTypeId as varchar);

-- Delete the Block Type
DELETE [BlockType] WHERE [Id] = @BlockTypeId;
            " );
        }

        /// <summary>
        /// Set an Attribute Value for a Block.
        /// </summary>
        /// <param name="blockGuid"></param>
        /// <param name="attributeKey"></param>
        /// <param name="newValue"></param>
        private void SetBlockAttributeValue( string blockGuid, string attributeKey, string newValue )
        {
            Sql( $@"
DECLARE @blockId int = (SELECT [Id] FROM [Block] WHERE [Guid] = '550d7229-2788-4c0e-bfe6-4aae95d28267')
DECLARE @blockTypeId int = (SELECT [BlockTypeId] FROM [Block] WHERE [Id] = @blockId)
DECLARE @entityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block')

DECLARE @attributeId int =
(
	SELECT [Id]
    FROM [Attribute]
    WHERE [EntityTypeId] = @entityTypeId
	 AND [EntityTypeQualifierColumn] = 'BlockTypeId'
	 AND [Key] = '{ attributeKey }'
	 AND [EntityTypeQualifierValue] = @blockTypeId
)

DECLARE @attributeValueId int = (SELECT [Id] FROM [AttributeValue] WHERE [AttributeId] = @attributeId AND [EntityId] = @blockId);

UPDATE [AttributeValue] SET [Value] = '{newValue}' WHERE [Id] = @attributeValueId;
" );
        }
        #endregion
    }
}
