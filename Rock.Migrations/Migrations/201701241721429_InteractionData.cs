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
    public partial class InteractionData : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Create InteractionChannels for each Site, and make sure Site.PageViewRetentionPeriodDays is copied with it
            Sql( @"
DECLARE @ChannelTypeMediumValueWebsiteId INT = (
		SELECT TOP 1 Id
		FROM DefinedValue
		WHERE [Guid] = 'E503E77D-CF35-E09F-41A2-B213184F48E8'
		)
	,@EntityTypePageId INT = (
		SELECT TOP 1 Id
		FROM EntityType
		WHERE [Name] = 'Rock.Model.Page'
		)

INSERT INTO [InteractionChannel] (
	[Name]
	,[ComponentEntityTypeId]
	,[ChannelTypeMediumValueId]
	,[ChannelEntityId]
	,[RetentionDuration]
	,[Guid]
	)
SELECT s.[Name] [Site.Name]
	,@EntityTypePageId
	,@ChannelTypeMediumValueWebsiteId
	,s.[Id] [SiteId]
	,s.PageViewRetentionPeriodDays
	,NEWID() AS NewGuid
FROM [Site] s
WHERE s.Id NOT IN (
		SELECT ChannelEntityId
		FROM InteractionChannel
		WHERE ChannelEntityId IS NOT NULL
		)
" );


            Sql( @"
-- Channel is just one hardcoded thing called 'Communication' which is for any Email, SMS, etc
DECLARE @InteractionChannel_COMMUNICATION UNIQUEIDENTIFIER = 'C88A187F-0343-4E7C-AF3F-79A8989DFA65'
    , @entityTypeIdCommunication INT = (
        SELECT TOP 1 Id

        FROM EntityType

        WHERE NAME = 'Rock.Model.Communication'
        )
	,@entityTypeIdCommunicationRecipient INT = (
        SELECT TOP 1 Id
        FROM EntityType
        WHERE NAME = 'Rock.Model.CommunicationRecipient'
		);

            IF NOT EXISTS(
                    SELECT *
                    FROM InteractionChannel
            
                    WHERE Guid = @InteractionChannel_COMMUNICATION
                    )
BEGIN
    INSERT INTO InteractionChannel (
        NAME
        , ComponentEntityTypeId
        , InteractionEntityTypeId
        , Guid
        )

    VALUES(
        'Communication'
        , @entityTypeIdCommunication
        , @entityTypeIdCommunicationRecipient
        , @InteractionChannel_COMMUNICATION
        );
            END;
            " );


            // Job for Migrating Interaction Data
            Sql( @"
INSERT INTO [dbo].[ServiceJob]
           ([IsSystem]
           ,[IsActive]
           ,[Name]
           ,[Description]
           ,[Class]
           ,[CronExpression]
           ,[NotificationStatus]
           ,[Guid])
     VALUES
        (0	
         ,1	
         ,'Move Data from PageViews and Communication Activity to the new Interaction Tables'
         ,'Moves the data from Page Views and Communication Recipient Activity into the Interaction tables. When done, the job will drop the PageView and CommunicationRecipientActivity tables, then the job will remove itself.'
         ,'Rock.Jobs.MigrateInteractionsData'
         ,'0 0 4 1/1 * ? *'
         ,3
         ,'189AE3F1-92E9-4394-ACC5-0F244967F32E')" );

            Sql( @"
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'spCore_PageViewNullPageId') BEGIN
    DROP PROCEDURE spCore_PageViewNullPageId
END" );

            DropColumn("dbo.Site", "PageViewRetentionPeriodDays");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove the Job for Migrating Interaction Data
            Sql( "DELETE FROM [ServiceJob] where [Guid] = '189AE3F1-92E9-4394-ACC5-0F244967F32E'" );

            AddColumn("dbo.Site", "PageViewRetentionPeriodDays", c => c.Int());
        }
    }
}
