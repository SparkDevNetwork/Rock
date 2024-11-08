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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

namespace Rock.Jobs
{
    /// <summary>
    /// Run once job for v16.7 to populate EntityIntents from AdditionalSettingsJson.
    /// </summary>
    [DisplayName( "Rock Update Helper v16.7 - Populate EntityIntents from AdditionalSettingsJson" )]
    [Description( "This job will migrate Interaction intents from Page and ContentChannelItem AdditionalSettingsJson fields to EntityIntents records." )]

    [IntegerField( "Command Timeout",
        Key = AttributeKey.CommandTimeout,
        Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of communications, this could take several minutes or more.",
        IsRequired = false,
        DefaultIntegerValue = 14400 )]

    public class PostV167PopulateEntityIntentsFromAdditionalSettingsJson : RockJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <inheritdoc />
        public override void Execute()
        {
            using ( var rockContext = new RockContext() )
            {
                // Get the configured timeout, or default to 240 minutes if it is blank.
                rockContext.Database.CommandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;

                var entityIntentService = new EntityIntentService( rockContext );

                // Migrate page intents.
                var pageService = new PageService( rockContext );
                var pages = pageService.Queryable().Where( p => p.AdditionalSettingsJson != null ).ToList();

                foreach ( var page in pages )
                {
                    var intentSettings = page.GetAdditionalSettingsOrNull<IntentSettings>();
                    if ( intentSettings != null )
                    {
                        var intentValueIds = intentSettings.InteractionIntentValueIds;
                        if ( intentValueIds?.Any() == true )
                        {
                            entityIntentService.SetIntents<Page>( page.Id, intentValueIds );
                        }

                        page.RemoveAdditionalSettings<IntentSettings>();
                    }
                }

                // Migrate content channel item intents.
                var contentChannelItemService = new ContentChannelItemService( rockContext );
                var contentChannelItems = contentChannelItemService.Queryable().Where( i => i.AdditionalSettingsJson != null ).ToList();

                foreach ( var contentChannelItem in contentChannelItems )
                {
                    var intentSettings = contentChannelItem.GetAdditionalSettingsOrNull<IntentSettings>();
                    if ( intentSettings != null )
                    {
                        var intentValueIds = intentSettings.InteractionIntentValueIds;
                        if ( intentValueIds?.Any() == true )
                        {
                            entityIntentService.SetIntents<ContentChannelItem>( contentChannelItem.Id, intentValueIds );
                        }

                        contentChannelItem.RemoveAdditionalSettings<IntentSettings>();
                    }
                }

                // Save any changes made above.
                rockContext.SaveChanges();
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

        /// <summary>
        /// A POCO to represent intent settings.
        /// </summary>
        private class IntentSettings
        {
            /// <summary>
            /// Interaction intent defined value identifiers.
            /// </summary>
            public List<int> InteractionIntentValueIds { get; set; }
        }
    }
}
