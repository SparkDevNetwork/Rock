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
    public partial class AddMobileOutreachToolbox : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.Contact",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    OwnerPersonAliasId = c.Int( nullable: false ),
                    FirstName = c.String( maxLength: 50 ),
                    LastName = c.String( maxLength: 50 ),
                    Gender = c.Int( nullable: false ),
                    PhotoId = c.Int(),
                    BirthDay = c.Int(),
                    BirthMonth = c.Int(),
                    BirthYear = c.Int(),
                    Email = c.String( maxLength: 75 ),
                    MobilePhone = c.String( maxLength: 20 ),
                    RelationshipStrength = c.Int( nullable: false ),
                    WeddingDay = c.Int(),
                    WeddingMonth = c.Int(),
                    WeddingYear = c.Int(),
                    PrayerCadence = c.Int( nullable: false ),
                    ConnectionCadence = c.Int( nullable: false ),
                    RelationshipFocus = c.Int( nullable: false ),
                    ConnectionNote = c.String( maxLength: 500 ),
                    PrayerNote = c.String( maxLength: 500 ),
                    HasAcceptedJesus = c.Boolean(),
                    SalvationDay = c.Int(),
                    SalvationMonth = c.Int(),
                    SalvationYear = c.Int(),
                    HasBeenBaptized = c.Boolean(),
                    BaptismDay = c.Int(),
                    BaptismMonth = c.Int(),
                    BaptismYear = c.Int(),
                    InstagramProfileUrl = c.String( maxLength: 75 ),
                    FacebookProfileUrl = c.String( maxLength: 75 ),
                    LinkedInProfileUrl = c.String( maxLength: 75 ),
                    XProfileUrl = c.String( maxLength: 75 ),
                    TikTokProfileUrl = c.String( maxLength: 75 ),
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
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.OwnerPersonAliasId )
                .ForeignKey( "dbo.BinaryFile", t => t.PhotoId )
                .Index( t => t.OwnerPersonAliasId )
                .Index( t => t.PhotoId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.ContactRelationshipChanges",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    ContactId = c.Int( nullable: false ),
                    PreviousRelationshipStrength = c.Int( nullable: false ),
                    NewRelationshipStrength = c.Int( nullable: false ),
                    HasAcceptedJesus = c.Boolean(),
                    WasAcceptanceInfluencedByApp = c.Boolean(),
                    HasBeenBaptized = c.Boolean(),
                    WasBaptismInfluencedByApp = c.Boolean(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.Contact", t => t.ContactId )
                .Index( t => t.ContactId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.ContactTouchpoint",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    ContactId = c.Int( nullable: false ),
                    Type = c.Int( nullable: false ),
                    ScheduledDateTime = c.DateTime( nullable: false ),
                    CompletedDateTime = c.DateTime(),
                    SystemNote = c.String( maxLength: 1000 ),
                    CommunicationMedium = c.Int(),
                    Note = c.String( maxLength: 500 ),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.Contact", t => t.ContactId )
                .Index( t => t.ContactId )
                .Index( t => t.Guid, unique: true );

            Sql( @"
CREATE NONCLUSTERED INDEX [IX_Type_ScheduledDateTime] ON [dbo].[ContactTouchpoint]
(
    [Type] ASC,
    [ScheduledDateTime] ASC
)
INCLUDE ([ContactId])" );

            Sql( @"
CREATE NONCLUSTERED INDEX [IX_CompletedDateTime_Type] ON [dbo].[ContactTouchpoint]
(
    [CompletedDateTime] ASC,
    [Type] ASC
)
INCLUDE ([ContactId])" );

            AddColumn( "dbo.Person", "OutreachTouchpointSchedule", c => c.Int( nullable: false, defaultValue: 0 ) );
            AddColumn( "dbo.Person", "OutreachTouchpointNotificationsEnabled", c => c.Boolean( nullable: false, defaultValue: false ) );
            AddColumn( "dbo.Person", "OutreachEnableDailyNotification", c => c.Boolean( nullable: false, defaultValue: false ) );
            AddColumn( "dbo.Person", "OutreachEnableSpecialEventsNotification", c => c.Boolean( nullable: false, defaultValue: false ) );
            AddColumn( "dbo.Person", "OutreachNotificationTimeOfDay", c => c.Int() );

            RockMigrationHelper.AddOrUpdateEntityType( "Rock.Model.Contact", SystemGuid.EntityType.CONTACT, true, true );
            RockMigrationHelper.AddOrUpdateEntityType( "Rock.Model.ContactRelationshipChange", SystemGuid.EntityType.CONTACT_RELATIONSHIP_CHANGE, true, true );
            RockMigrationHelper.AddOrUpdateEntityType( "Rock.Model.ContactTouchpoint", SystemGuid.EntityType.CONTACT_TOUCHPOINT, true, true );

            // Add the Interaction Channel that all Share actions from the
            // Outreach Toolbox will use.
            Sql( @"
DECLARE @MediumValueId INT = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '5919214F-9C59-4913-BE4E-0DFB6A05F528')
DECLARE @ContactEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'A2FBB846-2511-4760-B912-928775BFC6D6')

INSERT INTO [InteractionChannel] (
    [Name],
    [ChannelTypeMediumValueId],
    [InteractionEntityTypeId],
    [Guid],
    [UsesSession],
    [IsActive]
)
VALUES (
    'Outreach Toolbox Events',
    @MediumValueId,
    @ContactEntityTypeId,
    '456E9DC7-7AA2-4327-8592-60CC83D114A4',
    0,
    1
)" );

            var jobClass = "Rock.Jobs.UpdateOutreachToolboxTouchpoints";
            var cronSchedule = "0 10 * * * ? *"; // 10 minutes after every hour.

            Sql( $@"
            IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Guid] = '{SystemGuid.ServiceJob.UPDATE_OUTREACH_TOOLBOX_TOUCHPOINTS}' )
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
                    1,
                    1,
                    'Update Outreach Toolbox Touchpoints',
                    'Updates touchpoints for people using the outreach toolbox and sends any required notifications.',
                    '{jobClass}',
                    '{cronSchedule}',
                    1,
                    '{SystemGuid.ServiceJob.UPDATE_OUTREACH_TOOLBOX_TOUCHPOINTS}' );
            END" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( $"DELETE FROM [ServiceJob] WHERE [Guid] = '{SystemGuid.ServiceJob.UPDATE_OUTREACH_TOOLBOX_TOUCHPOINTS}'" );

            Sql( "DELETE FROM [InteractionChannel] WHERE [Guid] = '456E9DC7-7AA2-4327-8592-60CC83D114A4'" );

            DropForeignKey( "dbo.ContactTouchpoint", "ContactId", "dbo.Contact" );
            DropForeignKey( "dbo.ContactRelationshipChanges", "ContactId", "dbo.Contact" );
            DropForeignKey( "dbo.Contact", "PhotoId", "dbo.BinaryFile" );
            DropForeignKey( "dbo.Contact", "OwnerPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Contact", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Contact", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropIndex( "dbo.ContactTouchpoint", new[] { "Type", "ScheduledDateTime" } );
            DropIndex( "dbo.ContactTouchpoint", new[] { "CompletedDateTime", "Type" } );
            DropIndex( "dbo.ContactTouchpoint", new[] { "Guid" } );
            DropIndex( "dbo.ContactTouchpoint", new[] { "ContactId" } );
            DropIndex( "dbo.ContactRelationshipChanges", new[] { "Guid" } );
            DropIndex( "dbo.ContactRelationshipChanges", new[] { "ContactId" } );
            DropIndex( "dbo.Contact", new[] { "Guid" } );
            DropIndex( "dbo.Contact", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.Contact", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.Contact", new[] { "PhotoId" } );
            DropIndex( "dbo.Contact", new[] { "OwnerPersonAliasId" } );
            DropColumn( "dbo.Person", "OutreachNotificationTimeOfDay" );
            DropColumn( "dbo.Person", "OutreachEnableSpecialEventsNotification" );
            DropColumn( "dbo.Person", "OutreachEnableDailyNotification" );
            DropColumn( "dbo.Person", "OutreachTouchpointNotificationsEnabled" );
            DropColumn( "dbo.Person", "OutreachTouchpointSchedule" );
            DropTable( "dbo.ContactTouchpoint" );
            DropTable( "dbo.ContactRelationshipChanges" );
            DropTable( "dbo.Contact" );
        }
    }
}
