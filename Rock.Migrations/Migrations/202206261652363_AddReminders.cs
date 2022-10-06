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
    /// Adds Reminders to Rock.
    /// </summary>
    public partial class AddReminders : RockMigration
    {
        private const string REMINDER_LIST_BLOCK_INSTANCE = "8CD5AA01-88F3-4D7B-B25A-92280792451E";
        private const string REMINDER_EDIT_BLOCK_INSTANCE = "8987B121-AA92-4562-B3CD-196CE0CC3B15";
        private const string REMINDER_LINKS_BLOCK_INSTANCE = "A5F41693-2A52-4C78-AC3C-69A504D896D3";
        private const string REMINDER_TYPES_BLOCK_INSTANCE = "64BED7FE-2F5B-4270-B045-9AE069E98DDD";
        private const string CRON_EXPRESSION = "0 0 4 1/1 * ? *"; // 4am daily.

        private const string SHIFT_BLOCK_ORDER_QUERY = @"
UPDATE	[Block]
SET		[Order] = [Order] + 1
WHERE	[SiteId] = (SELECT [Id] FROM [Site] WHERE [Guid] = 'C2D29296-6A87-47A9-A753-EE4E9159C4C4')
	AND	[Zone] = 'Header'
";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateReminderTable();
            CreateReminderTypeTable();
            AddPersonReminderCountField();

            AddBlockTypes();
            AddReminderLinksBlock();
            AddReminderListPage();
            AddReminderEditPage();
            AddReminderTypesPage();
            AddReminderJob();
        }

        #region Model Migrations

        private void CreateReminderTable()
        {
            CreateTable(
                "dbo.Reminder",
                c => new
                    {
                        Id = c.Int( nullable: false, identity: true ),
                        ReminderTypeId = c.Int( nullable: false ),
                        PersonAliasId = c.Int( nullable: false ),
                        EntityId = c.Int( nullable: false ),
                        Note = c.String(),
                        ReminderDate = c.DateTime( nullable: false ),
                        IsComplete = c.Boolean( nullable: false ),
                        RenewPeriodDays = c.Int(),
                        RenewMaxCount = c.Int(),
                        RenewCurrentCount = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid( nullable: false ),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String( maxLength: 100 ),
                    } )
                .PrimaryKey(t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.PersonAliasId, cascadeDelete: true )
                .ForeignKey( "dbo.ReminderType", t => t.ReminderTypeId, cascadeDelete: true )
                .Index( t => t.ReminderTypeId )
                .Index( t => t.PersonAliasId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );
        }

        private void CreateReminderTypeTable()
        {
            CreateTable(
                "dbo.ReminderType",
                c => new
                    {
                        Id = c.Int( nullable: false, identity: true ),
                        Name = c.String(),
                        Description = c.String(),
                        IsActive = c.Boolean( nullable: false ),
                        NotificationType = c.Int( nullable: false ),
                        NotificationWorkflowTypeId = c.Int(),
                        ShouldShowNote = c.Boolean( nullable: false ),
                        Order = c.Int( nullable: false ),
                        EntityTypeId = c.Int( nullable: false ),
                        ShouldAutoCompleteWhenNotified = c.Boolean( nullable: false ),
                        HighlightColor = c.String( maxLength: 50 ),
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
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.EntityType", t => t.EntityTypeId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.EntityTypeId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );
        }

        private void AddPersonReminderCountField()
        {
            AddColumn( "dbo.Person", "ReminderCount", c => c.Int() );
        }

        #endregion Model Migrations

        #region Blocks, Pages, and Routes

        private void AddBlockTypes()
        {
            RockMigrationHelper.UpdateBlockType(
                "Reminder Links",
                "This block is used to show reminder links.",
                "~/Blocks/Reminders/ReminderLinks.ascx",
                "Core",
                SystemGuid.BlockType.REMINDER_LINKS );

            RockMigrationHelper.UpdateBlockType(
                "Reminder List",
                "Block to show a list of reminders.",
                "~/Blocks/Reminders/ReminderList.ascx",
                "Core",
                SystemGuid.BlockType.REMINDER_LIST );

            RockMigrationHelper.UpdateBlockType(
                "Reminder Edit",
                "Block for editing reminders.",
                "~/Blocks/Reminders/ReminderEdit.ascx",
                "Core",
                SystemGuid.BlockType.REMINDER_EDIT );

            RockMigrationHelper.UpdateBlockType(
                "Reminder Types",
                "Block for editing reminder types.",
                "~/Blocks/Reminders/ReminderTypes.ascx",
                "Core",
                SystemGuid.BlockType.REMINDER_TYPES );
        }

        private void AddReminderLinksBlock()
        {
            // Shifts any blocks in the internal site header zone (e.g., the Personal Links block) by one
            // so that the reminders link will be first.
            Sql( SHIFT_BLOCK_ORDER_QUERY );

            // Add Block Reminder Links to  Site: Rock RMS        
            RockMigrationHelper.AddBlock(
                true,
                null,
                null,
                SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(),
                SystemGuid.BlockType.REMINDER_LINKS.AsGuid(),
                "Reminder Links",
                "Header",
                string.Empty,
                string.Empty,
                0,
                REMINDER_LINKS_BLOCK_INSTANCE );
        }

        private void AddReminderListPage()
        {
            RockMigrationHelper.AddPage(
                SystemGuid.Page.MY_DASHBOARD,
                SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE,
                "View Reminders",
                string.Empty,
                SystemGuid.Page.REMINDER_LIST,
                "fa fa-bell" );

            RockMigrationHelper.AddPageRoute( SystemGuid.Page.REMINDER_LIST, "reminders" );
            RockMigrationHelper.AddPageRoute( SystemGuid.Page.REMINDER_LIST, "reminders/{EntityTypeId}" );
            RockMigrationHelper.AddPageRoute( SystemGuid.Page.REMINDER_LIST, "reminders/{EntityTypeId}/{EntityId}" );
            RockMigrationHelper.AddPageRoute( SystemGuid.Page.REMINDER_LIST, "reminders/{EntityTypeId}/{ReminderTypeId}" );
            RockMigrationHelper.AddPageRoute( SystemGuid.Page.REMINDER_LIST, "reminders/{EntityTypeId}/{ReminderTypeId}/{EntityId}" );

            RockMigrationHelper.AddBlock(
                SystemGuid.Page.REMINDER_LIST,
                string.Empty,
                SystemGuid.BlockType.REMINDER_LIST,
                "Reminder List",
                "Main",
                string.Empty,
                string.Empty,
                0,
                REMINDER_LIST_BLOCK_INSTANCE );
        }

        private void AddReminderEditPage()
        {
            RockMigrationHelper.AddPage(
                SystemGuid.Page.REMINDER_LIST,
                SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE,
                "Edit Reminder",
                string.Empty,
                SystemGuid.Page.REMINDER_EDIT,
                "fa fa-bell" );

            RockMigrationHelper.AddBlock(
                SystemGuid.Page.REMINDER_EDIT,
                string.Empty,
                SystemGuid.BlockType.REMINDER_EDIT,
                "Edit Reminder",
                "Main",
                string.Empty,
                string.Empty,
                0,
                REMINDER_EDIT_BLOCK_INSTANCE );
        }

        private void AddReminderTypesPage()
        {
            RockMigrationHelper.AddPage(
                SystemGuid.Page.GENERAL_SETTINGS,
                SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE,
                "Reminder Types",
                string.Empty,
                SystemGuid.Page.REMINDER_TYPES,
                "fa fa-bell" );

            RockMigrationHelper.AddBlock(
                SystemGuid.Page.REMINDER_TYPES,
                string.Empty,
                SystemGuid.BlockType.REMINDER_TYPES,
                "Reminder Types",
                "Main",
                string.Empty,
                string.Empty,
                0,
                REMINDER_TYPES_BLOCK_INSTANCE );
        }

        #endregion Blocks, Pages, and Routes

        private void AddReminderJob()
        {
            var jobClass = "Rock.Jobs.CalculateReminderCounts";

            Sql( $@"
            IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = '{jobClass}' AND [Guid] = '{SystemGuid.ServiceJob.CALCULATE_REMINDER_COUNTS}' ) 
            BEGIN 
                INSERT INTO [ServiceJob] (
                    [IsSystem],
                    [IsActive],
                    [Name],
                    [Description],
                    [Class],
                    [CronExpression],
                    [NotificationStatus],
                    [Guid] )
                VALUES (
                    0,
                    1,
                    'Calculate Reminder Counts',
                    'A job that calculates the updates the reminder count value for people with active reminders.',
                    '{jobClass}',
                    '{CRON_EXPRESSION}',
                    1,
                    '{SystemGuid.ServiceJob.CALCULATE_REMINDER_COUNTS}' );
            END" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.Reminder", "ReminderTypeId", "dbo.ReminderType" );
            DropForeignKey( "dbo.ReminderType", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.ReminderType", "EntityTypeId", "dbo.EntityType" );
            DropForeignKey( "dbo.ReminderType", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Reminder", "PersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Reminder", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Reminder", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropIndex( "dbo.ReminderType", new[] { "Guid" } );
            DropIndex( "dbo.ReminderType", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.ReminderType", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.ReminderType", new[] { "EntityTypeId" } );
            DropIndex( "dbo.Reminder", new[] { "Guid" } );
            DropIndex( "dbo.Reminder", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.Reminder", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.Reminder", new[] { "PersonAliasId" } );
            DropIndex( "dbo.Reminder", new[] { "ReminderTypeId" } );
            DropColumn( "dbo.Person", "ReminderCount" );
            DropTable( "dbo.ReminderType" );
            DropTable( "dbo.Reminder" );
        }
    }
}
