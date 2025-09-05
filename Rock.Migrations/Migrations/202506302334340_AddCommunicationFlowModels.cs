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
    public partial class AddCommunicationFlowModels : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JMH_AddCommunicationFlowModelsUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JMH_AddCommunicationFlowModelsDown();
        }

        private void JMH_AddCommunicationFlowModelsUp()
        {
            CreateTable(
                "dbo.CommunicationFlowCommunication",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 100 ),
                    DaysToWait = c.Int( nullable: false ),
                    TimeToSend = c.Time( nullable: false, precision: 7 ),
                    CommunicationType = c.Int( nullable: false ),
                    CommunicationFlowId = c.Int( nullable: false ),
                    CommunicationTemplateId = c.Int( nullable: false ),
                    Order = c.Int( nullable: false ),
                    AdditionalSettingsJson = c.String(),
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
                .ForeignKey( "dbo.CommunicationFlow", t => t.CommunicationFlowId, cascadeDelete: true )
                .ForeignKey( "dbo.CommunicationTemplate", t => t.CommunicationTemplateId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.CommunicationFlowId )
                .Index( t => t.CommunicationTemplateId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.CommunicationFlow",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( nullable: false, maxLength: 100 ),
                    IsActive = c.Boolean( nullable: false ),
                    Description = c.String(),
                    CategoryId = c.Int(),
                    TriggerType = c.Int( nullable: false ),
                    TargetAudienceDataViewId = c.Int(),
                    ScheduleId = c.Int(),
                    ConversionGoalType = c.Int(),
                    ConversionGoalTargetPercent = c.Decimal( precision: 18, scale: 2 ),
                    ConversionGoalTimeframeInDays = c.Int(),
                    ExitConditionType = c.Int( nullable: false ),
                    UnsubscribeMessage = c.String( maxLength: 500 ),
                    AdditionalSettingsJson = c.String(),
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
                .ForeignKey( "dbo.Schedule", t => t.ScheduleId )
                .ForeignKey( "dbo.DataView", t => t.TargetAudienceDataViewId )
                .Index( t => t.CategoryId )
                .Index( t => t.TargetAudienceDataViewId )
                .Index( t => t.ScheduleId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.CommunicationFlowInstance",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    CommunicationFlowId = c.Int( nullable: false ),
                    StartDate = c.DateTime( nullable: false ),
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
                .ForeignKey( "dbo.CommunicationFlow", t => t.CommunicationFlowId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.CommunicationFlowId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.CommunicationFlowInstanceCommunication",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    CommunicationFlowInstanceId = c.Int( nullable: false ),
                    CommunicationFlowCommunicationId = c.Int( nullable: false ),
                    CommunicationId = c.Int( nullable: false ),
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
                .ForeignKey( "dbo.Communication", t => t.CommunicationId )
                .ForeignKey( "dbo.CommunicationFlowCommunication", t => t.CommunicationFlowCommunicationId )
                .ForeignKey( "dbo.CommunicationFlowInstance", t => t.CommunicationFlowInstanceId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.CommunicationFlowInstanceId )
                .Index( t => t.CommunicationFlowCommunicationId )
                .Index( t => t.CommunicationId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.CommunicationFlowInstanceConversionHistory",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    CommunicationFlowInstanceId = c.Int( nullable: false ),
                    Date = c.DateTime( nullable: false ),
                    PersonAliasId = c.Int( nullable: false ),
                    CommunicationFlowCommunicationId = c.Int( nullable: false ),
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
                .ForeignKey( "dbo.CommunicationFlowCommunication", t => t.CommunicationFlowCommunicationId )
                .ForeignKey( "dbo.CommunicationFlowInstance", t => t.CommunicationFlowInstanceId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.PersonAliasId )
                .Index( t => t.CommunicationFlowInstanceId )
                .Index( t => t.PersonAliasId )
                .Index( t => t.CommunicationFlowCommunicationId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.CommunicationFlowInstanceRecipient",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    CommunicationFlowInstanceId = c.Int( nullable: false ),
                    RecipientPersonAliasId = c.Int( nullable: false ),
                    Status = c.Int( nullable: false ),
                    UnsubscribeCommunicationRecipientId = c.Int(),
                    UnsubscribeScope = c.Int(),
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
                .ForeignKey( "dbo.CommunicationFlowInstance", t => t.CommunicationFlowInstanceId )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.RecipientPersonAliasId )
                .ForeignKey( "dbo.CommunicationRecipient", t => t.UnsubscribeCommunicationRecipientId )
                .Index( t => t.CommunicationFlowInstanceId )
                .Index( t => t.RecipientPersonAliasId )
                .Index( t => t.UnsubscribeCommunicationRecipientId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            AddColumn( "dbo.CommunicationTemplate", "UsageType", c => c.Int() );
        }

        private void JMH_AddCommunicationFlowModelsDown()
        {
            DropForeignKey( "dbo.CommunicationFlowCommunication", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.CommunicationFlowCommunication", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.CommunicationFlowCommunication", "CommunicationTemplateId", "dbo.CommunicationTemplate" );
            DropForeignKey( "dbo.CommunicationFlow", "TargetAudienceDataViewId", "dbo.DataView" );
            DropForeignKey( "dbo.CommunicationFlow", "ScheduleId", "dbo.Schedule" );
            DropForeignKey( "dbo.CommunicationFlow", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.CommunicationFlow", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.CommunicationFlowInstance", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.CommunicationFlowInstance", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.CommunicationFlowInstanceRecipient", "UnsubscribeCommunicationRecipientId", "dbo.CommunicationRecipient" );
            DropForeignKey( "dbo.CommunicationFlowInstanceRecipient", "RecipientPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.CommunicationFlowInstanceRecipient", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.CommunicationFlowInstanceRecipient", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.CommunicationFlowInstanceRecipient", "CommunicationFlowInstanceId", "dbo.CommunicationFlowInstance" );
            DropForeignKey( "dbo.CommunicationFlowInstanceConversionHistory", "PersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.CommunicationFlowInstanceConversionHistory", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.CommunicationFlowInstanceConversionHistory", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.CommunicationFlowInstanceConversionHistory", "CommunicationFlowInstanceId", "dbo.CommunicationFlowInstance" );
            DropForeignKey( "dbo.CommunicationFlowInstanceConversionHistory", "CommunicationFlowCommunicationId", "dbo.CommunicationFlowCommunication" );
            DropForeignKey( "dbo.CommunicationFlowInstanceCommunication", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.CommunicationFlowInstanceCommunication", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.CommunicationFlowInstanceCommunication", "CommunicationFlowInstanceId", "dbo.CommunicationFlowInstance" );
            DropForeignKey( "dbo.CommunicationFlowInstanceCommunication", "CommunicationFlowCommunicationId", "dbo.CommunicationFlowCommunication" );
            DropForeignKey( "dbo.CommunicationFlowInstanceCommunication", "CommunicationId", "dbo.Communication" );
            DropForeignKey( "dbo.CommunicationFlowInstance", "CommunicationFlowId", "dbo.CommunicationFlow" );
            DropForeignKey( "dbo.CommunicationFlowCommunication", "CommunicationFlowId", "dbo.CommunicationFlow" );
            DropForeignKey( "dbo.CommunicationFlow", "CategoryId", "dbo.Category" );
            DropIndex( "dbo.CommunicationFlowInstanceRecipient", new[] { "Guid" } );
            DropIndex( "dbo.CommunicationFlowInstanceRecipient", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.CommunicationFlowInstanceRecipient", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.CommunicationFlowInstanceRecipient", new[] { "UnsubscribeCommunicationRecipientId" } );
            DropIndex( "dbo.CommunicationFlowInstanceRecipient", new[] { "RecipientPersonAliasId" } );
            DropIndex( "dbo.CommunicationFlowInstanceRecipient", new[] { "CommunicationFlowInstanceId" } );
            DropIndex( "dbo.CommunicationFlowInstanceConversionHistory", new[] { "Guid" } );
            DropIndex( "dbo.CommunicationFlowInstanceConversionHistory", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.CommunicationFlowInstanceConversionHistory", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.CommunicationFlowInstanceConversionHistory", new[] { "CommunicationFlowCommunicationId" } );
            DropIndex( "dbo.CommunicationFlowInstanceConversionHistory", new[] { "PersonAliasId" } );
            DropIndex( "dbo.CommunicationFlowInstanceConversionHistory", new[] { "CommunicationFlowInstanceId" } );
            DropIndex( "dbo.CommunicationFlowInstanceCommunication", new[] { "Guid" } );
            DropIndex( "dbo.CommunicationFlowInstanceCommunication", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.CommunicationFlowInstanceCommunication", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.CommunicationFlowInstanceCommunication", new[] { "CommunicationId" } );
            DropIndex( "dbo.CommunicationFlowInstanceCommunication", new[] { "CommunicationFlowCommunicationId" } );
            DropIndex( "dbo.CommunicationFlowInstanceCommunication", new[] { "CommunicationFlowInstanceId" } );
            DropIndex( "dbo.CommunicationFlowInstance", new[] { "Guid" } );
            DropIndex( "dbo.CommunicationFlowInstance", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.CommunicationFlowInstance", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.CommunicationFlowInstance", new[] { "CommunicationFlowId" } );
            DropIndex( "dbo.CommunicationFlow", new[] { "Guid" } );
            DropIndex( "dbo.CommunicationFlow", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.CommunicationFlow", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.CommunicationFlow", new[] { "ScheduleId" } );
            DropIndex( "dbo.CommunicationFlow", new[] { "TargetAudienceDataViewId" } );
            DropIndex( "dbo.CommunicationFlow", new[] { "CategoryId" } );
            DropIndex( "dbo.CommunicationFlowCommunication", new[] { "Guid" } );
            DropIndex( "dbo.CommunicationFlowCommunication", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.CommunicationFlowCommunication", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.CommunicationFlowCommunication", new[] { "CommunicationTemplateId" } );
            DropIndex( "dbo.CommunicationFlowCommunication", new[] { "CommunicationFlowId" } );
            DropColumn( "dbo.CommunicationTemplate", "UsageType" );
            DropTable( "dbo.CommunicationFlowInstanceRecipient" );
            DropTable( "dbo.CommunicationFlowInstanceConversionHistory" );
            DropTable( "dbo.CommunicationFlowInstanceCommunication" );
            DropTable( "dbo.CommunicationFlowInstance" );
            DropTable( "dbo.CommunicationFlow" );
            DropTable( "dbo.CommunicationFlowCommunication" );
        }
    }
}
