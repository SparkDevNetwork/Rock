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
using System.ComponentModel;
using System.Linq;
using System.Text;

using Quartz;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisplayName( "Update Personalization Data" )]
    [Description( "Job that updates Personalization Data." )]

    [IntegerField(
        "Command Timeout",
        Key = AttributeKey.CommandTimeoutSeconds,
        Description = "Maximum amount of time (in seconds) to wait for the sql operations to complete. Leave blank to use the default for this job (180).",
        IsRequired = false,
        DefaultIntegerValue = 60 * 3,
        Order = 1 )]

    [DisallowConcurrentExecution]
    public class UpdatePersonalizationData : IJob
    {
        /// <summary>
        /// Keys to use for Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string CommandTimeoutSeconds = "CommandTimeoutSeconds";
        }

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public UpdatePersonalizationData()
        {
        }

        /// <summary>
        /// Executes the UpdatePersonalizationData Job
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            var includeSegmentsWithNonPersistedDataViews = false;
            var segmentList = PersonalizationSegmentCache.GetActiveSegments( includeSegmentsWithNonPersistedDataViews );

            if ( !segmentList.Any() )
            {
                context.UpdateLastStatusMessage( "No Personalization Segments configured." );
                return;
            }

            var resultsBuilder = new StringBuilder();
            int commandTimeoutSeconds = context.JobDetail.JobDataMap.GetString( AttributeKey.CommandTimeoutSeconds ).AsIntegerOrNull() ?? 180;

            foreach ( var segment in segmentList.OrderBy( s => s.Name ) )
            {
                context.UpdateLastStatusMessage( $"{segment.Name}: Updating..." );
                using ( var rockContext = new RockContext() )
                {
                    rockContext.Database.CommandTimeout = commandTimeoutSeconds;
                    var segmentUpdateResults = new PersonalizationSegmentService( rockContext ).UpdatePersonAliasPersonalizationDataForSegment( segment );
                    if ( segmentUpdateResults.CountAddedSegment == 0 && segmentUpdateResults.CountRemovedFromSegment == 0 )
                    {
                        resultsBuilder.AppendLine( $"{segment.Name} - No changes." );
                    }
                    else
                    {
                        resultsBuilder.AppendLine( $"{segment.Name} - {segmentUpdateResults.CountAddedSegment} added and {segmentUpdateResults.CountRemovedFromSegment} removed." );
                    }
                }
            }

            var cleanupRockContext = new RockContext();
            cleanupRockContext.Database.CommandTimeout = commandTimeoutSeconds;
            var cleanedUpCount = new PersonalizationSegmentService( cleanupRockContext ).CleanupPersonAliasPersonalizationDataForSegmentsThatDontExist();
            if ( cleanedUpCount > 0 )
            {
                resultsBuilder.AppendLine( $"Cleaned up {cleanedUpCount}" );
            }

            context.UpdateLastStatusMessage( resultsBuilder.ToString() );
        }
    }
}
