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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs.PostUpdateJobs
{
    /// <summary>
    /// Run once job for v16 to update media element default urls.
    /// </summary>
    [DisplayName( "Rock Update Helper v16.0 - Update Media Element Default URLs." )]
    [Description( "This job updates media element default urls to match calculated values." )]

    [IntegerField(
    "Command Timeout",
    Key = AttributeKey.CommandTimeout,
    Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of notes, this could take several minutes or more.",
    IsRequired = false,
    DefaultIntegerValue = 14400 )]

    public class PostV16UpdateMediaElementDefaultUrls : PostUpdateJobs.PostUpdateJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <inheritdoc />
        public override void Execute()
        {
            SetDefaultUrls();

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

        /// <summary>
        /// Loads and saves all media elements so that the new calculated
        /// column properties get filled in.
        /// </summary>
        private void SetDefaultUrls()
        {
            List<int> mediaElementIds;

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;

                mediaElementIds = new MediaElementService( rockContext ).Queryable()
                    .Select( me => me.Id )
                    .ToList();
            }

            while ( mediaElementIds.Any() )
            {
                var ids = mediaElementIds.Take( 250 ).ToList();
                mediaElementIds = mediaElementIds.Skip( 250 ).ToList();

                using ( var rockContext = new RockContext() )
                {
                    var mediaElementService = new MediaElementService( rockContext );
                    var mediaElements = mediaElementService.Queryable()
                        .Where( me => ids.Contains( me.Id ) )
                        .ToList();

                    foreach ( var mediaElement in mediaElements )
                    {
                        // These properties are calculated automatically, which means
                        // that EF wouldn't understand they had changed unless the
                        // backing value has changed too. So force the update.
                        rockContext.Database.ExecuteSqlCommand( "UPDATE [MediaElement] SET [DefaultFileUrl] = @p1, [DefaultThumbnailUrl] = @p2 WHERE [Id] = @p0",
                            mediaElement.Id,
                            mediaElement.DefaultFileUrl,
                            mediaElement.DefaultThumbnailUrl );
                    }

                    rockContext.SaveChanges();
                }
            }
        }
    }
}
