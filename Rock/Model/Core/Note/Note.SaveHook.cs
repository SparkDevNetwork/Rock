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
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class Note
    {
        /// <summary>
        /// Save hook implementation for <see cref="Note"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<Note>
        {
            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                CreateAutoWatchForAuthor();

                base.PreSave();
            }

            /// <summary>
            /// Creates a <see cref="NoteWatch"/> for the author of this <see cref="Note"/> when appropriate.
            /// </summary>
            private void CreateAutoWatchForAuthor()
            {
                if ( State != EntityContextState.Added )
                {
                    return; // only create new watches for new notes.
                }

                var noteType = NoteTypeCache.Get( this.Entity.NoteTypeId );
                if ( noteType == null )
                {
                    return; // Probably an error, but let's avoid creating another one.
                }

                if ( noteType.AutoWatchAuthors != true )
                {
                    return; // Don't auto watch.
                }

                if ( !this.Entity.CreatedByPersonAliasId.HasValue )
                {
                    return; // No author to add a watch for.
                }

                // add a NoteWatch so the author will get notified when there are any replies
                var noteWatch = new NoteWatch
                {
                    // we don't know the Note.Id yet, so just assign the NoteWatch.Note and EF will populate the NoteWatch.NoteId automatically
                    Note = this.Entity,
                    IsWatching = true,
                    WatcherPersonAliasId = this.Entity.CreatedByPersonAliasId.Value
                };

                new NoteWatchService( RockContext ).Add( noteWatch );
            }
        }
    }
}
