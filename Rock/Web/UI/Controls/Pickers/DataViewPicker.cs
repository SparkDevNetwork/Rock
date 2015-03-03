// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class DataViewPicker : RockDropDownList
    {
        /// <summary>
        /// Gets or sets the data view entity type identifier.
        /// </summary>
        /// <value>
        /// The data view entity type identifier.
        /// </value>
        public int? EntityTypeId
        {
            get
            {
                return _entityTypeId;
            }

            set
            {
                _entityTypeId = value;
                LoadDropDownItems();
            }
        }

        /// <summary>
        /// The data view entity type identifier
        /// </summary>
        private int? _entityTypeId;

        /// <summary>
        /// Loads the drop down items.
        /// </summary>
        private void LoadDropDownItems()
        {
            this.Items.Clear();

            if ( _entityTypeId.HasValue )
            {
                // add Empty option first
                this.Items.Add( new ListItem() );

                foreach ( var dataView in new DataViewService( new RockContext() ).GetByEntityTypeId( _entityTypeId.Value ) )
                {
                    if ( dataView.IsAuthorized( Authorization.VIEW, this.RockBlock().CurrentPerson ) &&
                        dataView.DataViewFilter.IsAuthorized( Authorization.VIEW, this.RockBlock().CurrentPerson ) )
                    {
                        this.Items.Add( new ListItem( dataView.Name, dataView.Id.ToString() ) );
                    }
                }
            }
        }
    }
}