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
using Rock.Web.Cache;

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v14 to update missing media element interactions
    /// </summary>
    [DisplayName( "Rock Update Helper v14.0 - Add missing Media Element interactions." )]
    [Description( "This job will update the interation length of media element interactions. After all the operations are done, this job will delete itself." )]

    [IntegerField(
    "Command Timeout",
    AttributeKey.CommandTimeout,
    Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of interactions, this could take several minutes or more.",
    IsRequired = false,
    DefaultIntegerValue = 60 * 60 )]
    public class PostV14DataMigrationsAddMissingMediaElementInteractions : RockJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            // get the configured timeout, or default to 60 minutes if it is blank
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 3600;
            System.Collections.Generic.List<int> mediaInteractionWithoutInteractionIdList;

            // First, get a list of all the Interaction IDs that we'll need to update because later each interaction needs to be updated.
            using ( var rockContext = new Rock.Data.RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                var interactionChannelIdMediaEvent = InteractionChannelCache.GetId( Rock.SystemGuid.InteractionChannel.MEDIA_EVENTS.AsGuid() );
                if ( !interactionChannelIdMediaEvent.HasValue )
                {
                    DeleteJob( this.GetJobId() );
                    return;
                }

                var interactionService = new InteractionService( rockContext ).Queryable();
                var mediaInteractionWithoutInteractionLengthQuery = interactionService.Where( a => a.InteractionComponent.InteractionChannelId == interactionChannelIdMediaEvent.Value && !a.InteractionLength.HasValue );
                mediaInteractionWithoutInteractionIdList = mediaInteractionWithoutInteractionLengthQuery.Select( a => a.Id ).OrderBy( a => a ).ToList();
            }

            double updatedCount = 0;
            int totalCount = mediaInteractionWithoutInteractionIdList.Count();
            var lastUpdateDateTime = RockDateTime.Now;

            // One interaction at time, parse the JSON and update InteractionLength. Doing it one
            // as a time is still pretty fast, and has consistent performance. Maybe 30000 per minute.
            // Note that to support SQL 2014, we can't use SQL JSON commands since that isn't supported until SQL 2016.
            foreach ( var mediaInteractionId in mediaInteractionWithoutInteractionIdList )
            {
                using ( var rockContext = new Rock.Data.RockContext() )
                {
                    rockContext.Database.CommandTimeout = commandTimeout;
                    var mediaInteraction = new InteractionService( rockContext ).Get( mediaInteractionId );

                    var mediaEventInteractionData = mediaInteraction?.InteractionData.FromJsonOrNull<MediaEventInteractionData>();
                    if ( mediaEventInteractionData?.WatchedPercentage != null && !mediaInteraction.InteractionLength.HasValue )
                    {
                        mediaInteraction.InteractionLength = mediaEventInteractionData.WatchedPercentage;
                        rockContext.SaveChanges( disablePrePostProcessing: true );

                    }

                    updatedCount++;

                    if ( ( RockDateTime.Now - lastUpdateDateTime ).TotalSeconds > 3 )
                    {
                        var percentProgress = updatedCount * 100 / totalCount;
                        this.UpdateLastStatusMessage( $"Update Progress {Math.Round( percentProgress, 2 )}%" );
                        lastUpdateDateTime = RockDateTime.Now;
                    }
                }
            }

            DeleteJob( this.GetJobId() );
        }

        private class MediaEventInteractionData
        {
            public string WatchMap { get; set; }

            public float WatchedPercentage { get; set; }
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
