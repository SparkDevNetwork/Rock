using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Quartz;

using Rock.Data;
using Rock.Model;


namespace Rock.Jobs
{
    /// <summary>
    /// A run once job for V8
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisallowConcurrentExecution]
    [DisplayName( "Data Migrations for v8.0" )]
    [Description( "This job will take care of any data migrations that need to occur after updating to v80. After all the operations are done, this job will delete itself." )]
    class PostV80DataMigrations : IJob
    {
        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="NotImplementedException"></exception>
        public void Execute( IJobExecutionContext context )
        {
            CreateIndexInteractionsForeignKey();
            UpdateInteractionSummaryForPageViews();
            DeleteJob( context.GetJobId() );
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
                    return;
                }
            }
        }

        /// <summary>
        /// Creates the index on Interactions.ForeignKey.
        /// Includes were reccomended by Query Analyzer
        /// </summary>
        public static void CreateIndexInteractionsForeignKey()
        {
            using ( RockContext rockContext = new RockContext() )
            {
                rockContext.Database.ExecuteSqlCommand( @"IF NOT EXISTS( SELECT * FROM sys.indexes WHERE name = 'IX_ForeignKey' AND object_id = OBJECT_ID( N'[dbo].[Interaction]' ) ) 
                    BEGIN
                    CREATE NONCLUSTERED INDEX [IX_ForeignKey]
                    ON [dbo].[Interaction] ([ForeignKey])
                    INCLUDE ([Id]
	                    ,[InteractionDateTime]
	                    ,[Operation]
	                    ,[InteractionComponentId]
	                    ,[EntityId]
	                    ,[PersonAliasId]
	                    ,[InteractionSessionId]
	                    ,[InteractionSummary]
	                    ,[InteractionData]
	                    ,[CreatedDateTime]
	                    ,[ModifiedDateTime]
	                    ,[CreatedByPersonAliasId]
	                    ,[ModifiedByPersonAliasId]
	                    ,[Guid]
	                    ,[ForeignId]
	                    ,[ForeignGuid]
	                    ,[PersonalDeviceId]
	                    ,[RelatedEntityTypeId]
	                    ,[RelatedEntityId]
	                    ,[Source]
	                    ,[Medium]
	                    ,[Campaign]
	                    ,[Content]
	                    ,[InteractionEndDateTime])
                    END" );
            }
        }

        /// <summary>
        /// Updates the interaction summary on Interactions for page views.
        /// </summary>
        public static void UpdateInteractionSummaryForPageViews()
        {
            using ( RockContext rockContext = new RockContext() )
            {
                string sqlQuery = @"DECLARE @ChannelMediumValueId INT;
                                    SELECT 
	                                    @ChannelMediumValueId = [Id]
                                    FROM 
	                                    [DefinedValue]
                                    WHERE [Guid]='{0}'
                                    UPDATE 
	                                    A
                                    SET
	                                    A.[InteractionSummary] = B.[Name]
                                    FROM
	                                    [Interaction] A INNER JOIN 
	                                    [InteractionComponent] B 
                                    ON
	                                    A.[InteractionComponentId] = B.[Id]
                                    WHERE
	                                    B.[ChannelId]  IN (SELECT
							                                    [Id]
						                                    FROM
							                                    [InteractionChannel]
						                                    WHERE 
							                                    [ChannelTypeMediumValueId]=@ChannelMediumValueId)";

                rockContext.Database.ExecuteSqlCommand( string.Format(sqlQuery, SystemGuid.DefinedValue.INTERACTIONCHANNELTYPE_WEBSITE ) );
            }
        }
    }
}
