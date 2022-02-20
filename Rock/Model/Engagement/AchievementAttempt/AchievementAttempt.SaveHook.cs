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
using Rock.Data;
using Rock.Tasks;

namespace Rock.Model
{
    public partial class AchievementAttempt
    {
        /// <summary>
        /// SaveHook implementation of <see cref="AchievementAttempt"/>
        /// </summary>
        internal class SaveHook: EntitySaveHook<AchievementAttempt>
        {
            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                var updateAchievementAttemptMsg = GetUpdateAchievementAttemptMessage( this.Entry );
                updateAchievementAttemptMsg.Send();
                base.PreSave();
            }

            /// <summary>
            /// Gets the update achievement attempt message.
            /// </summary>
            /// <param name="entry">The entry.</param>
            /// <returns>UpdateAchievementAttempt.Message.</returns>
            private UpdateAchievementAttempt.Message GetUpdateAchievementAttemptMessage( IEntitySaveEntry entry )
            {
                var updateAchievementAttemptMsg = new UpdateAchievementAttempt.Message();
                if ( entry.State != EntityContextState.Added && entry.State != EntityContextState.Modified )
                {
                    return updateAchievementAttemptMsg;
                }

                var achievementAttempt = entry.Entity as AchievementAttempt;

                bool wasClosed = false;
                bool wasSuccessful = false;

                if ( entry.State != EntityContextState.Added )
                {
                    wasClosed = ( bool ) entry.OriginalValues[nameof(AchievementAttempt.IsClosed)];
                    wasSuccessful = ( bool ) entry.OriginalValues[nameof( AchievementAttempt.IsSuccessful )];
                }

                var currentPersonAliasId = new PersonService( new RockContext() ).GetCurrentPerson().PrimaryAliasId;

                // Add a transaction to process workflows and add steps
                updateAchievementAttemptMsg = new UpdateAchievementAttempt.Message
                {
                    AchievementAttemptGuid = achievementAttempt.Guid,
                    IsNowStarting = entry.State == EntityContextState.Added,
                    IsNowEnding = !wasClosed && achievementAttempt.IsClosed,
                    IsNowSuccessful = !wasSuccessful && achievementAttempt.IsSuccessful,
                    AchievementTypeId = achievementAttempt.AchievementTypeId,
                    StartDate = achievementAttempt.AchievementAttemptStartDateTime,
                    EndDate = achievementAttempt.AchievementAttemptEndDateTime,
                    InitiatorPersonAliasId = currentPersonAliasId
                };

                return updateAchievementAttemptMsg;
            }
        }
    }
}
