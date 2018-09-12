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
    public partial class GroupHistory : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpSchemaChanges();

            UpAddServiceJobs();

            UpPagesBlocks();

            UpAddHistoryCategories();

            // Update PersonMerge stored procs to work with Groups/GroupMembers that have GroupHistory 
            Sql( MigrationSQL._201805072022522_GroupHistory_spCrm_PersonMerge );
            Sql( MigrationSQL._201805072022522_GroupHistory_spCrm_PersonMergeRelationships );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DownAddHistoryCategories();

            DownPagesBlocks();

            DownAddServiceJobs();

            DownSchemaChanges();
        }

        private void UpSchemaChanges()
        {
            CreateTable(
                            "dbo.AttributeValueHistorical",
                            c => new
                            {
                                Id = c.Int( nullable: false, identity: true ),
                                AttributeValueId = c.Int( nullable: false ),
                                Value = c.String(),
                                ValueFormatted = c.String(),
                                ValueAsNumeric = c.Decimal( precision: 18, scale: 2 ),
                                ValueAsDateTime = c.DateTime(),
                                ValueAsBoolean = c.Boolean(),
                                ValueAsPersonId = c.Int(),
                                EffectiveDateTime = c.DateTime( nullable: false ),
                                ExpireDateTime = c.DateTime( nullable: false ),
                                CurrentRowIndicator = c.Boolean( nullable: false ),
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
                            .ForeignKey( "dbo.AttributeValue", t => t.AttributeValueId, cascadeDelete: true )
                            .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                            .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                            .Index( t => t.AttributeValueId )
                            .Index( t => t.CreatedByPersonAliasId )
                            .Index( t => t.ModifiedByPersonAliasId )
                            .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.GroupHistorical",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    GroupId = c.Int( nullable: false ),
                    GroupName = c.String( maxLength: 100 ),
                    GroupTypeId = c.Int( nullable: false ),
                    GroupTypeName = c.String( maxLength: 100 ),
                    CampusId = c.Int(),
                    ParentGroupId = c.Int(),
                    Description = c.String(),
                    ScheduleId = c.Int(),
                    ScheduleName = c.String(),
                    ScheduleModifiedDateTime = c.DateTime(),
                    IsArchived = c.Boolean( nullable: false ),
                    ArchivedDateTime = c.DateTime(),
                    ArchivedByPersonAliasId = c.Int(),
                    IsActive = c.Boolean( nullable: false ),
                    InactiveDateTime = c.DateTime(),
                    EffectiveDateTime = c.DateTime( nullable: false ),
                    ExpireDateTime = c.DateTime( nullable: false ),
                    CurrentRowIndicator = c.Boolean( nullable: false ),
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
                .ForeignKey( "dbo.PersonAlias", t => t.ArchivedByPersonAliasId )
                .ForeignKey( "dbo.Campus", t => t.CampusId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.Group", t => t.GroupId )
                .ForeignKey( "dbo.GroupType", t => t.GroupTypeId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.Group", t => t.ParentGroupId )
                .ForeignKey( "dbo.Schedule", t => t.ScheduleId )
                .Index( t => t.GroupId )
                .Index( t => t.GroupTypeId )
                .Index( t => t.CampusId )
                .Index( t => t.ParentGroupId )
                .Index( t => t.ScheduleId )
                .Index( t => t.ArchivedByPersonAliasId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.GroupLocationHistorical",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    GroupLocationId = c.Int( nullable: false ),
                    GroupId = c.Int( nullable: false ),
                    GroupLocationTypeValueId = c.Int(),
                    GroupLocationTypeName = c.String( maxLength: 250 ),
                    LocationId = c.Int( nullable: false ),
                    LocationName = c.String(),
                    LocationModifiedDateTime = c.DateTime(),
                    EffectiveDateTime = c.DateTime( nullable: false ),
                    ExpireDateTime = c.DateTime( nullable: false ),
                    CurrentRowIndicator = c.Boolean( nullable: false ),
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
                .ForeignKey( "dbo.Group", t => t.GroupId )
                .ForeignKey( "dbo.GroupLocation", t => t.GroupLocationId )
                .ForeignKey( "dbo.Location", t => t.LocationId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.GroupLocationId )
                .Index( t => t.GroupId )
                .Index( t => t.LocationId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.GroupLocationHistoricalSchedule",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    GroupLocationHistoricalId = c.Int( nullable: false ),
                    ScheduleId = c.Int( nullable: false ),
                    ScheduleName = c.String(),
                    ScheduleModifiedDateTime = c.DateTime(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.GroupLocationHistorical", t => t.GroupLocationHistoricalId, cascadeDelete: true )
                .ForeignKey( "dbo.Schedule", t => t.ScheduleId )
                .Index( t => t.GroupLocationHistoricalId )
                .Index( t => t.ScheduleId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.GroupMemberHistorical",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    GroupMemberId = c.Int( nullable: false ),
                    GroupId = c.Int( nullable: false ),
                    GroupRoleId = c.Int( nullable: false ),
                    GroupRoleName = c.String( maxLength: 100 ),
                    IsLeader = c.Boolean( nullable: false ),
                    GroupMemberStatus = c.Int( nullable: false ),
                    IsArchived = c.Boolean( nullable: false ),
                    ArchivedDateTime = c.DateTime(),
                    ArchivedByPersonAliasId = c.Int(),
                    InactiveDateTime = c.DateTime(),
                    EffectiveDateTime = c.DateTime( nullable: false ),
                    ExpireDateTime = c.DateTime( nullable: false ),
                    CurrentRowIndicator = c.Boolean( nullable: false ),
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
                .ForeignKey( "dbo.PersonAlias", t => t.ArchivedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.Group", t => t.GroupId )
                .ForeignKey( "dbo.GroupMember", t => t.GroupMemberId )
                .ForeignKey( "dbo.GroupTypeRole", t => t.GroupRoleId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.GroupMemberId )
                .Index( t => t.GroupId )
                .Index( t => t.GroupRoleId )
                .Index( t => t.ArchivedByPersonAliasId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            AddColumn( "dbo.Group", "InactiveDateTime", c => c.DateTime() );
            AddColumn( "dbo.Group", "IsArchived", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.Group", "ArchivedDateTime", c => c.DateTime() );
            AddColumn( "dbo.Group", "ArchivedByPersonAliasId", c => c.Int() );
            AddColumn( "dbo.GroupType", "EnableGroupHistory", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.GroupType", "GroupTypeColor", c => c.String( maxLength: 100 ) );
            AddColumn( "dbo.Attribute", "IsActive", c => c.Boolean( nullable: false, defaultValue: true ) );
            AddColumn( "dbo.Attribute", "EnableHistory", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.GroupMember", "InactiveDateTime", c => c.DateTime() );
            AddColumn( "dbo.GroupMember", "IsArchived", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.GroupMember", "ArchivedDateTime", c => c.DateTime() );
            AddColumn( "dbo.GroupMember", "ArchivedByPersonAliasId", c => c.Int() );
            AddColumn( "dbo.History", "ChangeType", c => c.String( maxLength: 20 ) );
            AddColumn( "dbo.History", "ValueName", c => c.String( maxLength: 250 ) );
            AddColumn( "dbo.History", "NewValue", c => c.String() );
            AddColumn( "dbo.History", "OldValue", c => c.String() );
            AddColumn( "dbo.History", "IsSensitive", c => c.Boolean() );
            AddColumn( "dbo.History", "SourceOfChange", c => c.String() );
            CreateIndex( "dbo.Group", "ArchivedByPersonAliasId" );
            CreateIndex( "dbo.GroupMember", "ArchivedByPersonAliasId" );
            AddForeignKey( "dbo.Group", "ArchivedByPersonAliasId", "dbo.PersonAlias", "Id" );
            AddForeignKey( "dbo.GroupMember", "ArchivedByPersonAliasId", "dbo.PersonAlias", "Id" );

            Sql( "UPDATE [Attribute] set [IsActive] = 1" );

            // Enforce that there isn't more than one CurrentRow per AttributeValue in AttributeValueHistorical
            Sql( @"
CREATE UNIQUE NONCLUSTERED  INDEX [IX_AttributeValueIdCurrentRow] ON [dbo].[AttributeValueHistorical]
(
	[AttributeValueId] ASC,
	[CurrentRowIndicator]
) where CurrentRowIndicator = 1 
" );

            // Enforce that there isn't more than one CurrentRow per GroupMemberId in GroupMemberHistorical
            Sql( @"
CREATE UNIQUE NONCLUSTERED  INDEX [IX_GroupMemberIdCurrentRow] ON [dbo].[GroupMemberHistorical]
(
	[GroupMemberId] ASC,
	[CurrentRowIndicator]
) where CurrentRowIndicator = 1 
" );

            // Enforce that there isn't more than one CurrentRow per GroupMemberId in GroupHistorical
            Sql( @"
CREATE UNIQUE NONCLUSTERED  INDEX [IX_GroupIdCurrentRow] ON [dbo].[GroupHistorical]
(
	[GroupId] ASC,
	[CurrentRowIndicator]
) where CurrentRowIndicator = 1 
" );

            // Enforce that there isn't more than one CurrentRow per GroupLocationId in GroupLocationHistorical
            Sql( @"
CREATE UNIQUE NONCLUSTERED  INDEX [IX_LocationIdCurrentRow] ON [dbo].[GroupLocationHistorical]
(
	[GroupLocationId] ASC,
	[CurrentRowIndicator]
) where CurrentRowIndicator = 1 
" );
        }

        private void DownSchemaChanges()
        {
            DropForeignKey( "dbo.GroupMemberHistorical", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.GroupMemberHistorical", "GroupRoleId", "dbo.GroupTypeRole" );
            DropForeignKey( "dbo.GroupMemberHistorical", "GroupMemberId", "dbo.GroupMember" );
            DropForeignKey( "dbo.GroupMemberHistorical", "GroupId", "dbo.Group" );
            DropForeignKey( "dbo.GroupMemberHistorical", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.GroupMemberHistorical", "ArchivedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.GroupLocationHistorical", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.GroupLocationHistorical", "LocationId", "dbo.Location" );
            DropForeignKey( "dbo.GroupLocationHistoricalSchedule", "ScheduleId", "dbo.Schedule" );
            DropForeignKey( "dbo.GroupLocationHistoricalSchedule", "GroupLocationHistoricalId", "dbo.GroupLocationHistorical" );
            DropForeignKey( "dbo.GroupLocationHistorical", "GroupLocationId", "dbo.GroupLocation" );
            DropForeignKey( "dbo.GroupLocationHistorical", "GroupId", "dbo.Group" );
            DropForeignKey( "dbo.GroupLocationHistorical", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.GroupHistorical", "ScheduleId", "dbo.Schedule" );
            DropForeignKey( "dbo.GroupHistorical", "ParentGroupId", "dbo.Group" );
            DropForeignKey( "dbo.GroupHistorical", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.GroupHistorical", "GroupTypeId", "dbo.GroupType" );
            DropForeignKey( "dbo.GroupHistorical", "GroupId", "dbo.Group" );
            DropForeignKey( "dbo.GroupHistorical", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.GroupHistorical", "CampusId", "dbo.Campus" );
            DropForeignKey( "dbo.GroupHistorical", "ArchivedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AttributeValueHistorical", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AttributeValueHistorical", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AttributeValueHistorical", "AttributeValueId", "dbo.AttributeValue" );
            DropForeignKey( "dbo.GroupMember", "ArchivedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Group", "ArchivedByPersonAliasId", "dbo.PersonAlias" );
            DropIndex( "dbo.GroupMemberHistorical", new[] { "Guid" } );
            DropIndex( "dbo.GroupMemberHistorical", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.GroupMemberHistorical", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.GroupMemberHistorical", new[] { "ArchivedByPersonAliasId" } );
            DropIndex( "dbo.GroupMemberHistorical", new[] { "GroupRoleId" } );
            DropIndex( "dbo.GroupMemberHistorical", new[] { "GroupId" } );
            DropIndex( "dbo.GroupMemberHistorical", new[] { "GroupMemberId" } );
            DropIndex( "dbo.GroupLocationHistoricalSchedule", new[] { "Guid" } );
            DropIndex( "dbo.GroupLocationHistoricalSchedule", new[] { "ScheduleId" } );
            DropIndex( "dbo.GroupLocationHistoricalSchedule", new[] { "GroupLocationHistoricalId" } );
            DropIndex( "dbo.GroupLocationHistorical", new[] { "Guid" } );
            DropIndex( "dbo.GroupLocationHistorical", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.GroupLocationHistorical", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.GroupLocationHistorical", new[] { "LocationId" } );
            DropIndex( "dbo.GroupLocationHistorical", new[] { "GroupId" } );
            DropIndex( "dbo.GroupLocationHistorical", new[] { "GroupLocationId" } );
            DropIndex( "dbo.GroupHistorical", new[] { "Guid" } );
            DropIndex( "dbo.GroupHistorical", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.GroupHistorical", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.GroupHistorical", new[] { "ArchivedByPersonAliasId" } );
            DropIndex( "dbo.GroupHistorical", new[] { "ScheduleId" } );
            DropIndex( "dbo.GroupHistorical", new[] { "ParentGroupId" } );
            DropIndex( "dbo.GroupHistorical", new[] { "CampusId" } );
            DropIndex( "dbo.GroupHistorical", new[] { "GroupTypeId" } );
            DropIndex( "dbo.GroupHistorical", new[] { "GroupId" } );
            DropIndex( "dbo.AttributeValueHistorical", new[] { "Guid" } );
            DropIndex( "dbo.AttributeValueHistorical", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.AttributeValueHistorical", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.AttributeValueHistorical", new[] { "AttributeValueId" } );
            DropIndex( "dbo.GroupMember", new[] { "ArchivedByPersonAliasId" } );
            DropIndex( "dbo.Group", new[] { "ArchivedByPersonAliasId" } );
            DropColumn( "dbo.History", "SourceOfChange" );
            DropColumn( "dbo.History", "IsSensitive" );
            DropColumn( "dbo.History", "OldValue" );
            DropColumn( "dbo.History", "NewValue" );
            DropColumn( "dbo.History", "ValueName" );
            DropColumn( "dbo.History", "ChangeType" );
            DropColumn( "dbo.GroupMember", "ArchivedByPersonAliasId" );
            DropColumn( "dbo.GroupMember", "ArchivedDateTime" );
            DropColumn( "dbo.GroupMember", "IsArchived" );
            DropColumn( "dbo.GroupMember", "InactiveDateTime" );
            DropColumn( "dbo.Attribute", "EnableHistory" );
            DropColumn( "dbo.Attribute", "IsActive" );
            DropColumn( "dbo.GroupType", "EnableGroupHistory" );
            DropColumn( "dbo.GroupType", "GroupTypeColor" );
            DropColumn( "dbo.Group", "ArchivedByPersonAliasId" );
            DropColumn( "dbo.Group", "ArchivedDateTime" );
            DropColumn( "dbo.Group", "IsArchived" );
            DropColumn( "dbo.Group", "InactiveDateTime" );
            DropTable( "dbo.GroupMemberHistorical" );
            DropTable( "dbo.GroupLocationHistoricalSchedule" );
            DropTable( "dbo.GroupLocationHistorical" );
            DropTable( "dbo.GroupHistorical" );
            DropTable( "dbo.AttributeValueHistorical" );
        }

        private void UpAddServiceJobs()
        {
            /* Service Jobs*/
            // add ServiceJob: Process Group History
            // Code Generated using Rock\Dev Tools\Sql\CodeGen_ServiceJobWithAttributes_ForAJob.sql
            Sql( @"IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.ProcessGroupHistory' AND [Guid] = 'D81E577D-2D87-4CEB-9585-7BA8DBA0F556' )
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
                  ,'Process Group History'
                  ,'Creates Historical snapshots of Groups and Group Members for any group types that have history enabled.'
                  ,'Rock.Jobs.ProcessGroupHistory'
                  ,'0 30 3 1/1 * ? *'
                  ,1
                  ,'D81E577D-2D87-4CEB-9585-7BA8DBA0F556'
                  );
            END" );


            // add ServiceJob: Migrate History Summary Data (schedule for 9pm to avoid conflict with AppPoolRecycle)
            // Code Generated using Rock\Dev Tools\Sql\CodeGen_ServiceJobWithAttributes_ForAJob.sql
            Sql( @"IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.MigrateHistorySummaryData' AND [Guid] = 'CF2221CC-1E0A-422B-B0F7-5D81AF1DDB14' )
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
                  ,'Migrate History Summary Data'
                  ,'Migrates History Summary Text into separate columns: Verb, ChangeType, OldValue, NewValue, etc. This will enable the new v8 History UIs to do cool things. When this job is done migrating all the data, it will delete itself.'
                  ,'Rock.Jobs.MigrateHistorySummaryData'
                  ,'0 0 21 1/1 * ? *'
                  ,1
                  ,'CF2221CC-1E0A-422B-B0F7-5D81AF1DDB14'
                  );
            END" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "Rock.Jobs.MigrateHistorySummaryData", "How Many Records", "The number of history records to process on each run of this job.", 0, @"500000", "FAC124D5-7C16-4AE9-9631-EF087349476F", "HowMany" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "Rock.Jobs.MigrateHistorySummaryData", "Command Timeout", "Maximum amount of time (in seconds) to wait for the SQL Query to complete. Leave blank to use the default for this job (3600). Note, it could take several minutes, so you might want to set it at 3600 (60 minutes) or higher", 1, @"3600", "1F28861F-99C2-4EAC-B7F7-4AE60018D97E", "CommandTimeout" );
            RockMigrationHelper.AddAttributeValue( "FAC124D5-7C16-4AE9-9631-EF087349476F", 39, @"500000", "FAC124D5-7C16-4AE9-9631-EF087349476F" ); // Migrate History Summary Data: How Many Records
            RockMigrationHelper.AddAttributeValue( "1F28861F-99C2-4EAC-B7F7-4AE60018D97E", 39, @"3600", "1F28861F-99C2-4EAC-B7F7-4AE60018D97E" ); // Migrate History Summary Data: Command Timeout
        }

        private void DownAddServiceJobs()
        {
            // remove ServiceJob: Migrate History Summary Data
            Sql( @"IF EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.MigrateHistorySummaryData' AND [Guid] = 'CF2221CC-1E0A-422B-B0F7-5D81AF1DDB14' )
            BEGIN
               DELETE [ServiceJob]  WHERE [Guid] = 'CF2221CC-1E0A-422B-B0F7-5D81AF1DDB14';
            END" );

            // remove ServiceJob: ProcessGroupHistory
            Sql( @"IF EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.ProcessGroupHistory' AND [Guid] = 'D81E577D-2D87-4CEB-9585-7BA8DBA0F556' )
            BEGIN
               DELETE [ServiceJob]  WHERE [Guid] = 'D81E577D-2D87-4CEB-9585-7BA8DBA0F556';
            END" );
        }

        private void UpPagesBlocks()
        {
            /* Pages/Blocks*/
            RockMigrationHelper.AddPage( true, "0B213645-FA4E-44A5-8E4C-B2D8EF054985", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Archived Groups", "", "93C79597-2274-4291-BE4F-E84569BB9B27", "fa fa-archive" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Group Archived List", "Lists Groups that have been archived", "~/Blocks/Groups/GroupArchivedList.ascx", "Utility", "AD5B3A8A-2111-4FC4-A026-51EEB4929CBA" );
            // Add Block to Page: Archived Groups, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "93C79597-2274-4291-BE4F-E84569BB9B27", "", "AD5B3A8A-2111-4FC4-A026-51EEB4929CBA", "Group Archived List", "Main", @"", @"", 0, "5522523B-15F7-49EA-B6FF-374F2BD101C1" );
            // Attrib for BlockType: Group Archived List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "AD5B3A8A-2111-4FC4-A026-51EEB4929CBA", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", @"", 0, @"", "1F2041D0-946F-4F8B-9E43-2E7E080C7FC0" );


            RockMigrationHelper.AddPage( true, "4E237286-B715-4109-A578-C1445EC02707", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Group History", "", "FCCF2570-DC09-4129-87BE-F1CAE25F1B9D", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Group History", "Displays a timeline of history for a group and it's members", "~/Blocks/Groups/GroupHistory.ascx", "Groups", "E916D65E-5D30-4086-9A11-8E891CCD930E" );
            // Add Block to Page: Group History, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "FCCF2570-DC09-4129-87BE-F1CAE25F1B9D", "", "E916D65E-5D30-4086-9A11-8E891CCD930E", "Group History", "Main", @"", @"", 0, "C27FF3C0-D7BF-4CAD-B33D-C0A6953370FC" );
            // Attrib for BlockType: Group Detail:Group History Page
            RockMigrationHelper.UpdateBlockTypeAttribute( "582BEEA1-5B27-444D-BC0A-F60CEB053981", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group History Page", "GroupHistoryPage", "", @"The page to display group history.", 15, @"", "F52CC072-7004-4A93-860F-65F6308374A4" );
            // Attrib for BlockType: Group History:Timeline Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "E916D65E-5D30-4086-9A11-8E891CCD930E", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Timeline Lava Template", "TimelineLavaTemplate", "", @"The Lava Template to use when rendering the timeline view of the history.", 1, @"{% include '~~/Assets/Lava/GroupHistoryTimeline.lava' %}", "71E281C3-0046-4B0C-B3DC-6D776F5F053E" );
            // Attrib Value for Block:GroupDetailRight, Attribute:Group History Page Page: Group Viewer, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "88344FE3-737E-4741-A38D-D2D3A1653818", "F52CC072-7004-4A93-860F-65F6308374A4", @"fccf2570-dc09-4129-87be-f1cae25f1b9d" );
            // Attrib Value for Block:Group History, Attribute:Timeline Lava Template Page: Group History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C27FF3C0-D7BF-4CAD-B33D-C0A6953370FC", "71E281C3-0046-4B0C-B3DC-6D776F5F053E", @"{% include '~~/Assets/Lava/GroupHistoryTimeline.lava' %}" );
        }

        private void DownPagesBlocks()
        {
            /* Pages/Blocks*/
            // Remove Block: Group Archived List, from Page: Archived Groups, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "5522523B-15F7-49EA-B6FF-374F2BD101C1" );
            RockMigrationHelper.DeleteBlockType( "AD5B3A8A-2111-4FC4-A026-51EEB4929CBA" ); // Group Archived List
            RockMigrationHelper.DeletePage( "93C79597-2274-4291-BE4F-E84569BB9B27" ); //  Page: Archived Groups, Layout: Full Width, Site: Rock RMS

            // Remove Block: Group History, from Page: Group History, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "C27FF3C0-D7BF-4CAD-B33D-C0A6953370FC" );
            RockMigrationHelper.DeleteBlockType( "E916D65E-5D30-4086-9A11-8E891CCD930E" ); // Group History
            RockMigrationHelper.DeletePage( "FCCF2570-DC09-4129-87BE-F1CAE25F1B9D" ); //  Page: Group History, Layout: Full Width, Site: Rock RMS


            // Code Generated using Rock\Dev Tools\Sql\CodeGen_ServiceJobWithAttributes_ForAJob.sql
            RockMigrationHelper.DeleteAttribute( "FAC124D5-7C16-4AE9-9631-EF087349476F" ); // Rock.Jobs.MigrateHistorySummaryData: How Many Records
            RockMigrationHelper.DeleteAttribute( "1F28861F-99C2-4EAC-B7F7-4AE60018D97E" ); // Rock.Jobs.MigrateHistorySummaryData: Command Timeout
        }

        private void UpAddHistoryCategories()
        {

            // Add History Categories for Group changes
            Sql( @"
DECLARE @CategoryId INT
DECLARE @HistoryEntityTypeId INT

SET @HistoryEntityTypeId = (
		SELECT [Id]
		FROM [EntityType]
		WHERE [Name] = 'Rock.Model.History'
		)

INSERT INTO [Category] (
	[IsSystem]
	,[EntityTypeId]
	,[Name]
	,[Guid]
	,[Order]
	)
VALUES (
	1
	,@HistoryEntityTypeId
	,'Group'
	,'180C5767-8769-4C51-865F-FEE29AEF80A0'
	,0
	)

SET @CategoryId = SCOPE_IDENTITY()

INSERT INTO [Category] (
	[IsSystem]
	,[ParentCategoryId]
	,[EntityTypeId]
	,[Name]
	,[Guid]
	,[Order]
	)
VALUES (
	1
	,@CategoryId
	,@HistoryEntityTypeId
	,'Group Changes'
	,'089EB47D-D0EF-493E-B867-DC51BCDEF319'
	,0
	)
" );
        }

        private void DownAddHistoryCategories()
        {
            // Remove History Categories for Group Changes
            Sql( @"
DELETE
FROM Category
WHERE [Guid] = '089EB47D-D0EF-493E-B867-DC51BCDEF319'

DELETE
FROM Category
WHERE [Guid] = '180C5767-8769-4C51-865F-FEE29AEF80A0'" );
        }
    }
}
