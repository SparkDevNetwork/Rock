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
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Storage.AssetStorage;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    class AssetStorageSystemPicker : RockDropDownList
    {
        public bool ShowAll
        {
            get
            {
                return ViewState["ShowAll"] as bool? ?? false;
            }
            set
            {
                if ( ShowAll != value )
                {
                    ViewState["ShowAll"] = value;
                    LoadItems( value );
                }
            }
        }

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            LoadItems( false );
        }

        private void LoadItems( bool showAll )
        {
            int? selectedItem = this.SelectedValueAsInt();

            this.Items.Clear();
            this.Items.Add( new ListItem() );

            using ( var rockContext = new RockContext() )
            {
                foreach ( var assetStorageSystem in new AssetStorageSystemService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( g => g.EntityTypeId.HasValue )
                    .OrderBy( g => g.Name )
                    .ToList() )
                {
                    var entityType = EntityTypeCache.Get( assetStorageSystem.EntityTypeId.Value );
                    AssetStorageComponent component = AssetStorageContainer.GetComponent( entityType.Name );
                    if ( showAll || ( assetStorageSystem.IsActive && component != null && component.IsActive ) )
                    {
                        this.Items.Add( new ListItem( assetStorageSystem.Name, assetStorageSystem.Id.ToString() ) );
                    }
                }
            }

            this.SetValue( selectedItem );

        }

    }
}
