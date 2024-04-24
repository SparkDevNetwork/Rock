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
    public partial class AchievementType
    {
        /// <summary>
        /// Save hook implementation for <see cref="AchievementType"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<AchievementType>
        {
            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                // This only makes sense if we have attributes, otherwise it
                // implies we are changing configuration unrelated to the
                // target count. And UpdateTargetCount requires attributes so
                // we don't load them if we don't need to.
                if ( Entity.AttributeValues != null )
                {
                    Entity.UpdateTargetCount( RockContext );
                }

                base.PreSave();
            }
        }
    }
}
