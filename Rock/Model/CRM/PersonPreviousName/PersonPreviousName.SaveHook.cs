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
    public partial class PersonPreviousName
    {
        /// <summary>
        /// Save hook implementation for <see cref="PersonPreviousName"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<PersonPreviousName>
        {
            private History.HistoryChangeList HistoryChanges { get; set; }
            private int? HistoryPersonId { get; set; }

            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                var rockContext = ( RockContext ) this.RockContext;
                HistoryPersonId = Entity.PersonAlias != null ? Entity.PersonAlias.PersonId : ( int? ) null;
                if ( !HistoryPersonId.HasValue )
                {
                    var personAlias = new PersonAliasService( rockContext ).Get( Entity.PersonAliasId );
                    if ( personAlias != null )
                    {
                        HistoryPersonId = personAlias.PersonId;
                    }
                }

                if ( HistoryPersonId.HasValue )
                {
                    HistoryChanges = new History.HistoryChangeList();

                    switch ( State )
                    {
                        case EntityContextState.Added:
                            {
                                History.EvaluateChange( HistoryChanges, "Previous Name", string.Empty, Entity.LastName );
                                break;
                            }

                        case EntityContextState.Modified:
                            {
                                History.EvaluateChange( HistoryChanges, "Previous Name", OriginalValues[nameof( PersonPreviousName.LastName )].ToStringSafe(), Entity.LastName );
                                break;
                            }

                        case EntityContextState.Deleted:
                            {
                                History.EvaluateChange( HistoryChanges, "Previous Name", Entity.LastName, string.Empty );
                                return;
                            }
                    }
                }

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
                var rockContext = ( RockContext ) this.RockContext;
                if ( HistoryChanges?.Any() == true && HistoryPersonId.HasValue )
                {
                    HistoryService.SaveChanges( rockContext, typeof( Person ), Rock.SystemGuid.Category.HISTORY_PERSON_DEMOGRAPHIC_CHANGES.AsGuid(), HistoryPersonId.Value, HistoryChanges, true, Entity.ModifiedByPersonAliasId );
                }

                base.PostSave();
            }
        }
    }
}
