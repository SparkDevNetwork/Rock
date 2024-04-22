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

using Rock.Data;

namespace Rock.Model
{
    public partial class LearningParticipant
    {
        /// <summary>
        /// Save hook implementation for <see cref="LearningParticipant"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        new internal class SaveHook : EntitySaveHook<LearningParticipant>
        {

            /// <summary>
            /// Ensures the Particpiant has the necessary <see cref="LearningActivityCompletion"/> records
            /// for the <see cref="LearningClass"/>.
            /// </summary>
            protected override void PreSave()
            {
                if ( PreSaveState == EntityContextState.Added )
                {
                    // Add the Activity Completion records for the participant (student or facilitator).
                    new LearningParticipantService( RockContext ).AddActivityCompletions(Entity.Id);

                    RockContext.SaveChanges();
                }

                base.PreSave();
            }
        }
    }
}
