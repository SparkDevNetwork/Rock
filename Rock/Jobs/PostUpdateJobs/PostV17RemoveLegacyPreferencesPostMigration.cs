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

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs.PostUpdateJobs
{
    /// <summary>
    /// This job removes the legacy user preferences from the Attribute and AttributeValue tables.
    /// </summary>
    [DisplayName( "Rock Update Helper v17.0 - Remove Legacy Preferences Post Migration Job." )]
    [Description( "This job removes the legacy user preferences from the Attribute and AttributeValue tables." )]

    [IntegerField(
    "Command Timeout",
    Key = AttributeKey.CommandTimeout,
    Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of data, this could take several minutes or more.",
    IsRequired = false,
    DefaultIntegerValue = 14400 )]
    public class PostV17RemoveLegacyPreferencesPostMigration : PostUpdateJobs.PostUpdateJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <inheritdoc />
        public override void Execute()
        {
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;
            var chunkSize = 3;

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;
                var entityTypeId = EntityTypeCache.Get( Person.USER_VALUE_ENTITY, false, rockContext )?.Id;

                if ( !entityTypeId.HasValue )
                {
                    DeleteJob();
                    return;
                }

                var attributesQry = new AttributeService( rockContext )
                    .Queryable()
                    .Where( a => a.EntityTypeId.HasValue && a.EntityTypeId == entityTypeId.Value );
                int index = 0;
                int total = attributesQry.Count();

                while ( attributesQry.Any() )
                {
                    var count = rockContext.BulkDelete( attributesQry.Take( chunkSize ) );
                    index += count;

                    UpdateLastStatusMessage( $"Completed execution till {index}. {total - index} more to go." );
                }

                UpdateLastStatusMessage( $"Removed {total:N0} legecy preference attributes." );
            }

            DeleteJob();
        }

        /// <summary>
        /// Deletes the job.
        /// </summary>
        private void DeleteJob()
        {
            using ( var rockContext = new RockContext() )
            {
                var jobService = new ServiceJobService( rockContext );
                var job = jobService.Get( GetJobId() );

                if ( job != null )
                {
                    jobService.Delete( job );
                    rockContext.SaveChanges();
                }
            }
        }
    }
}
