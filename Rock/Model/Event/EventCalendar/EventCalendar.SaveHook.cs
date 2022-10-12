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
    public partial class EventCalendar
    {
        /// <summary>
        /// Save hook implementation for <see cref="EventCalendar"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<EventCalendar>
        {
            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                // Keep the indexed Event Items correct
                if ( State == EntityContextState.Deleted && Entity.IsIndexEnabled )
                {
                    Entity.DeleteIndexedDocumentsByCalendarId( Entity.Id );
                }
                else if ( State == EntityContextState.Modified )
                {
                    var rockContext = ( RockContext ) this.RockContext;
                    var changeEntry = rockContext.ChangeTracker.Entries<EventCalendar>().Where( a => a.Entity == Entity ).FirstOrDefault();
                    if ( changeEntry != null )
                    {
                        var originalIndexState = ( bool ) changeEntry.OriginalValues[nameof( Entity.IsIndexEnabled )];

                        if ( originalIndexState == true && Entity.IsIndexEnabled == false )
                        {
                            // clear out index items
                            Entity.DeleteIndexedDocumentsByCalendarId( Entity.Id );
                        }
                        else if ( Entity.IsIndexEnabled == true )
                        {
                            // if indexing is enabled then bulk index - needed as an attribute could have changed from IsIndexed
                            Entity.BulkIndexDocumentsByCalendar( Entity.Id );
                        }
                    }
                }

                base.PreSave();
            }
        }
    }
}
