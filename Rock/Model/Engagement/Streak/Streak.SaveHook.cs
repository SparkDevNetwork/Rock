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
using System.Threading.Tasks;
using Rock.Data;

namespace Rock.Model
{
    public partial class Streak
    {
        /// <summary>
        /// Save hook implementation for <see cref="Streak"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<Streak>
        {
            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                Entity._isDeleted = State == EntityContextState.Deleted;
                base.PreSave();
            }

            /// <summary>
            /// Called after the save operation is executed.
            /// </summary>
            protected override void PostSave()
            {
                base.PostSave();

                if ( !Entity._isDeleted )
                {
                    // Running this as a task allows possibly changed streak type properties to be
                    // propagated to the streak type cache. Also there isn't really a reason that
                    // the data context save operation needs to wait while this is done.
                    Task.Run( () => StreakService.HandlePostSaveChanges( Entity.Id ) );
                }
            }
        }
    }
}