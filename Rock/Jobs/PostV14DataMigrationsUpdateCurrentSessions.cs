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
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using System.ComponentModel;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v14 to update current sessions
    /// </summary>
    [DisplayName( "Rock Update Helper v14.0 - Update current sessions." )]
    [Description( "This job will update the current sessions to have the duration of the session as well as the interaction count." )]

    [IntegerField(
    "Command Timeout",
    AttributeKey.CommandTimeout,
    Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.",
    IsRequired = false,
    DefaultIntegerValue = 240 * 60 )]
    public class PostV14DataMigrationsUpdateCurrentSessions : RockJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            // get the configured timeout, or default to 240 minutes if it is blank
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;

            using ( var rockContext = new Rock.Data.RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;
                rockContext.Database.ExecuteSqlCommand( @"
UPDATE xs
SET xs.[DurationSeconds] = sq.[DurationSeconds]
    , xs.[InteractionCount] = sq.[InteractionCount]
    , xs.[DurationLastCalculatedDateTime] = GETDATE() 
FROM [InteractionSession] xs
INNER JOIN (
        SELECT 
            s.[Id] AS [SessionId]
            , COUNT( i.[Id] ) AS [InteractionCount]
            , CASE WHEN COUNT( i.[Id] ) = 1 THEN 60
                ELSE DATEDIFF(s,MIN(i.[InteractionDateTime]), MAX(i.[InteractionDateTime]) ) + 60
            END AS [DurationSeconds]
        FROM [InteractionSession] s
            INNER JOIN [Interaction] i ON i.[InteractionSessionId] = s.[Id]
            INNER JOIN [InteractionComponent] ic ON ic.[Id] = i.[InteractionComponentId]
            INNER JOIN [InteractionChannel] ich ON ich.[Id] = ic.[InteractionChannelId]
            INNER JOIN [DefinedValue] mdv ON mdv.[Id] = ich.[ChannelTypeMediumValueId] 
        WHERE
            mdv.[Guid] = 'e503e77d-cf35-e09f-41a2-b213184f48e8'
            AND s.[InteractionSessionLocationId] IS NULL
        GROUP BY s.[Id]
) AS sq ON sq.[SessionId] = xs.[Id]
" );

                rockContext.Database.ExecuteSqlCommand( @"
            IF NOT EXISTS(
                SELECT 1
                FROM [ServiceJob]
                WHERE [Class] = 'Rock.Jobs.PopulateInteractionSessionData'
                    AND [Guid] = 'C0A57A17-1FE9-4C52-96B9-2F6EA1433D00'
            )
            BEGIN
                INSERT INTO[ServiceJob](
                      [IsSystem]
                    , [IsActive]
                    , [Name]
                    , [Description]
                    , [Class]
                    , [CronExpression]
                    , [NotificationStatus]
                    , [Guid]
                ) VALUES(
                      1
                    , 1
                    , 'Populate Interaction Session Data'
                    ,'This job will perform operations on the InteractionSession data and, if enabled, Geocode new InteractionSessionLocation records or link existing records to InteractionSession records.'
                    , 'Rock.Jobs.PopulateInteractionSessionData'
                    , '30 0/10 * 1/1 * ? *'
                    , 1
                    , 'C0A57A17-1FE9-4C52-96B9-2F6EA1433D00'
                );
                END

                DECLARE @EntityId INT =  (SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.PopulateInteractionSessionData' AND [Guid] = 'C0A57A17-1FE9-4C52-96B9-2F6EA1433D00')
                DECLARE @CommandTimeoutAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '7A02A62F-3B6E-495D-983B-F888B971B7E8')
                IF NOT EXISTS(SELECT [Id] FROM [dbo].[AttributeValue] WHERE [AttributeId] = @CommandTimeoutAttributeId AND [EntityId] = @EntityId)
                BEGIN
                    INSERT INTO [AttributeValue] (
                          [IsSystem]
		                , [AttributeId]
		                , [EntityId]
		                , [Value]
		                , [Guid])
                    VALUES(
                          1
		                , @CommandTimeoutAttributeId
		                , @EntityId
		                , '3600'
		                , NEWID())
                END


                DECLARE @LookbackMaximumAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'BF23E452-603F-41F9-A7BF-FB68E8296686')
                IF NOT EXISTS(SELECT [Id] FROM [dbo].[AttributeValue] WHERE [AttributeId] = @LookbackMaximumAttributeId AND [EntityId] = @EntityId)
                BEGIN
                    INSERT INTO [AttributeValue] (
                          [IsSystem]
		                , [AttributeId]
		                , [EntityId]
		                , [Value]
		                , [Guid])
                    VALUES(
                          1
		                , @LookbackMaximumAttributeId
		                , @EntityId
		                , '90'
		                , NEWID())
                END

                DECLARE @MaximumRecordsAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'C1096F04-0ECF-4519-9ABF-0CBB58BFFEA8')
                IF NOT EXISTS(SELECT [Id] FROM [dbo].[AttributeValue] WHERE [AttributeId] = @MaximumRecordsAttributeId AND [EntityId] = @EntityId)
                BEGIN
                    INSERT INTO [AttributeValue] (
                          [IsSystem]
		                , [AttributeId]
		                , [EntityId]
		                , [Value]
		                , [Guid])
                    VALUES(
                          1
		                , @MaximumRecordsAttributeId
		                , @EntityId
		                , '5000'
		                , NEWID())
                END
" );
            }

            DeleteJob( this.GetJobId() );
        }

        /// <summary>
        /// Deletes the job.
        /// </summary>
        /// <param name="jobId">The job identifier.</param>
        public static void DeleteJob( int jobId )
        {
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );
                var job = jobService.Get( jobId );

                if ( job != null )
                {
                    jobService.Delete( job );
                    rockContext.SaveChanges();
                }
            }
        }
    }
}
