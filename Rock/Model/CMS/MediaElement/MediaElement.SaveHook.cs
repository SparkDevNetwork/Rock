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
    public partial class MediaElement
    {
        /// <summary>
        /// Save hook implementation for <see cref="MediaElement"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<MediaElement>
        {
            private bool SaveOperationIsAdd { get; set; }

            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                SaveOperationIsAdd = Entry.State == EntityContextState.Added;

                base.PreSave();
            }

            /// <summary>
            /// Called after the save operation has been executed
            /// </summary>
            /// <remarks>
            /// This method is only called if <see cref="M:Rock.Data.EntitySaveHook`1.PreSave" /> returns
            /// without error.
            /// </remarks>
            protected override void PostSave()
            {
                if ( SaveOperationIsAdd )
                {
                    // We don't need to wait for this to complete.
                    Task.Run( () => MediaElementService.TriggerPostSaveTasks( Entity.Id ) );
                }

                base.PostSave();
            }
        }
    }
}