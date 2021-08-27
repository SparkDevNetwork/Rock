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
using System.Linq;
using Rock.Data;

namespace Rock.Model
{
    public partial class ContentChannel
    {
        /// <summary>
        /// Save hook implementation for <see cref="ContentChannel"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<ContentChannel>
        {
            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                if ( State == EntityContextState.Deleted )
                {
                    Entity.ChildContentChannels.Clear();
                }

                // clean up the index for the current content channel.
                if ( State == EntityContextState.Deleted && Entity.IsIndexEnabled )
                {
                    Entity.DeleteIndexedDocumentsByContentChannel( Entity.Id );
                }
                else if ( State == EntityContextState.Modified )
                {
                    // check if indexing is enabled for this content channel.
                    var changeEntry = RockContext.ChangeTracker.Entries<ContentChannel>().Where( a => a.Entity == Entity ).FirstOrDefault();
                    if ( changeEntry != null )
                    {
                        var originalIndexState = ( bool ) changeEntry.OriginalValues[nameof( ContentChannel.IsIndexEnabled )];

                        if ( originalIndexState == true && Entity.IsIndexEnabled == false )
                        {
                            // clear out index items for the current content channel.
                            Entity.DeleteIndexedDocumentsByContentChannel( Entity.Id );
                        }
                        else if ( Entity.IsIndexEnabled == true )
                        {
                            // if indexing is enabled then bulk index - needed as an attribute could have changed from IsIndexed.
                            Entity.BulkIndexDocumentsByContentChannel( Entity.Id );
                        }
                    }
                }

                base.PreSave();
            }
        }
    }
}