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
using System.Linq;

using Rock.CheckIn.v2;
using Rock.Data;

namespace Rock.Model
{
    public partial class GroupType
    {
        /// <summary>
        /// Save hook implementation for <see cref="GroupType"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<GroupType>
        {
            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                var rockContext = ( RockContext ) this.RockContext;

                if ( State == EntityContextState.Deleted )
                {
                    Entity.ChildGroupTypes.Clear();

                    // manually delete any grouprequirements of this grouptype since it can't be cascade deleted
                    var groupRequirementService = new GroupRequirementService( rockContext );
                    var groupRequirements = groupRequirementService.Queryable().Where( a => a.GroupTypeId.HasValue && a.GroupTypeId == Entity.Id ).ToList();
                    if ( groupRequirements.Any() )
                    {
                        groupRequirementService.DeleteRange( groupRequirements );
                    }
                }

                // clean up the index
                if ( State == EntityContextState.Deleted && Entity.IsIndexEnabled )
                {
                    Entity.DeleteIndexedDocumentsByGroupType( Entity.Id );
                }
                else if ( State == EntityContextState.Modified )
                {
                    // check if indexing is enabled
                    var changeEntry = rockContext.ChangeTracker.Entries<GroupType>().Where( a => a.Entity == Entity ).FirstOrDefault();
                    if ( changeEntry != null )
                    {
                        var originalIndexState = ( bool ) changeEntry.OriginalValues[nameof( GroupType.IsIndexEnabled )];

                        if ( originalIndexState == true && Entity.IsIndexEnabled == false )
                        {
                            // clear out index items
                            Entity.DeleteIndexedDocumentsByGroupType( Entity.Id );
                        }
                        else if ( Entity.IsIndexEnabled == true )
                        {
                            // if indexing is enabled then bulk index - needed as an attribute could have changed from IsIndexed
                            Entity.BulkIndexDocumentsByGroupType( Entity.Id );
                        }
                    }
                }

                base.PreSave();
            }

            /// <inheritdoc/>
            protected override void PostSave()
            {
                if ( PreSaveState == EntityContextState.Modified )
                {
                    CheckInDirector.SendRefreshKioskConfiguration();
                }

                base.PostSave();
            }
        }
    }
}
