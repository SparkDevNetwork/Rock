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
using System.Collections.Generic;
using System.Linq;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class DataViewFilter
    {
        internal class SaveHook : EntitySaveHook<DataViewFilter>
        {
            /// <summary>
            /// Method that will be called on an entity immediately before the item is saved by context
            /// </summary>
            protected override void PreSave()
            {
                var parentIds = new HashSet<int>();

                if ( PreSaveState == EntityContextState.Added )
                {
                    if ( Entity.ParentId.HasValue )
                    {
                        parentIds.Add( Entity.ParentId.Value );
                    }
                }
                else if ( PreSaveState == EntityContextState.Modified )
                {
                    var oldParentId = ( int? ) OriginalValues[nameof( DataViewFilter.ParentId )];

                    if ( oldParentId != Entity.ParentId )
                    {
                        if ( oldParentId.HasValue )
                        {
                            parentIds.Add( oldParentId.Value );
                        }

                        if ( Entity.ParentId.HasValue )
                        {
                            parentIds.Add( Entity.ParentId.Value );
                        }
                    }
                }
                else if ( PreSaveState == EntityContextState.Deleted )
                {
                    var oldParentId = ( int? ) OriginalValues[nameof( DataViewFilter.ParentId )];

                    if ( oldParentId.HasValue )
                    {
                        parentIds.Add( oldParentId.Value );
                    }
                }

                // If any change was made that could affect the child filters of
                // the parent, then clear the cached children for that parent.
                if ( parentIds.Any() )
                {
                    RockContext.ExecuteAfterCommit( () =>
                    {
                        foreach ( var parentId in parentIds )
                        {
                            DataViewFilterCache.ClearByParentId( parentId );
                        }
                    } );
                }

                base.PreSave();
            }
        }
    }
}
