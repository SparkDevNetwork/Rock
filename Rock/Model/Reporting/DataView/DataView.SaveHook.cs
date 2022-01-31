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

using Rock.Data;

namespace Rock.Model
{
    public partial class DataView
    {
        internal class SaveHook : EntitySaveHook<DataView>
        {
            /// <summary>
            /// Method that will be called on an entity immediately before the item is saved by context
            /// </summary>
            protected override void PreSave()
            {
                if ( Entry.State != EntityContextState.Deleted )
                {
                    if ( Entity.DataViewFilter != null )
                    {
                        Entity.DataViewFilter.DataView = Entity;
                        SetDataViewOnChildFilters( Entity.DataViewFilter.ChildFilters, Entity );
                    }
                }

                if ( Entry.State == EntityContextState.Added )
                {
                    Entity.RunCountLastRefreshDateTime = RockDateTime.Now;
                }

                base.PreSave();
            }

            private void SetDataViewOnChildFilters( ICollection<DataViewFilter> dataViewFilters, DataView dataView )
            {
                if ( dataViewFilters == null )
                {
                    return;
                }

                foreach ( var filter in dataViewFilters )
                {
                    filter.DataView = dataView;
                    SetDataViewOnChildFilters( filter.ChildFilters, dataView );
                }
            }
        }
    }
}
