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
using System.ComponentModel;
using System.Linq;

using Rock.Achievement;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs.PostUpdateJobs
{
    /// <summary>
    /// Run once job for v18.3 to update Achievement Types using this pseudo code:
    /// For each AchievementType:
    /// If the AchievementType.SourceEntityTypeId is NULL
    ///    * using the records ComponentEntityTypeId
    ///    * fetch the component: var component = GetAchievementComponent( componentEntityTypeGuid );
    ///       * set the records SourceEntityTypeId from the component.SupportedConfiguration.SourceEntityTypeCache.Id
    ///       * set the records AchieverEntityTypeId from the component.SupportedConfiguration.AchieverEntityTypeCache.Id
    /// </summary>
    [DisplayName( "Rock Update Helper v18.3 - Update Broken Achievement Types" )]
    [Description( "This job will update Achievement Types that are missing a SourceEntityTypeId and AchieverEntityTypeId." )]

    [IntegerField(
    "Command Timeout",
    Key = AttributeKey.CommandTimeout,
    Description = "Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.",
    IsRequired = false,
    DefaultIntegerValue = 14400 )]
    public class PostV183UpdateAchievementTypes : PostUpdateJobs.PostUpdateJob
    {
        private static class AttributeKey
        {
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <inheritdoc />
        public override void Execute()
        {
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? 14400;

            using ( var rockContext = new RockContext() )
            {
                rockContext.Database.CommandTimeout = commandTimeout;

                var achievementTypeService = new AchievementTypeService( rockContext );
                var brokenAchievementTypes = achievementTypeService.Queryable()
                    .Where( at => at.SourceEntityTypeId == null )
                    .ToList();

                foreach ( var achievementType in brokenAchievementTypes )
                {
                    var component = GetAchievementComponent( achievementType.ComponentEntityTypeId );
                    if ( component == null )
                    {
                        continue;
                    }

                    var configuration = component.SupportedConfiguration;
                    if ( configuration != null )
                    {
                        achievementType.SourceEntityTypeId = configuration.SourceEntityTypeCache?.Id;
                        achievementType.AchieverEntityTypeId = configuration.AchieverEntityTypeCache.Id;
                    }
                    else
                    {
                        // If the component doesn't have a SupportedConfiguration then log it and skip it since we won't be able to fix it.
                        var componentEntityType = EntityTypeCache.Get( achievementType.ComponentEntityTypeId );
                        var componentName = componentEntityType != null ? componentEntityType.FriendlyName : achievementType.ComponentEntityTypeId.ToString();
                        var message = $"Achievement Type '{achievementType.Name}' is using component '{componentName}' that does not have a SupportedConfiguration. Cannot update SourceEntityTypeId or AchieverEntityTypeId for this Achievement Type.";
                        ExceptionLogService.LogException( new Exception( message ) );
                        continue;
                    }
                }

                if ( brokenAchievementTypes.Any() )
                {
                    rockContext.SaveChanges();
                }
            }

            DeleteJob();
        }

        private AchievementComponent GetAchievementComponent( int componentEntityTypeId )
        {
            var componentEntityType = EntityTypeCache.Get( componentEntityTypeId );
            return componentEntityType == null ? null : AchievementContainer.GetComponent( componentEntityType.Name );
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
