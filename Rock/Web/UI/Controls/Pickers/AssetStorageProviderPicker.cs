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

using Rock.Data;
using Rock.Model;
using Rock.Storage.AssetStorage;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    class AssetStorageProviderPicker : RockDropDownList
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
                foreach ( var assetStorageProvider in new AssetStorageProviderService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( g => g.EntityTypeId.HasValue )
                    .OrderBy( g => g.Name )
                    .ToList() )
                {
                    var entityType = EntityTypeCache.Get( assetStorageProvider.EntityTypeId.Value );
                    AssetStorageComponent component = AssetStorageContainer.GetComponent( entityType.Name );
                    if ( showAll || ( assetStorageProvider.IsActive && component != null && component.IsActive ) )
                    {
                        this.Items.Add( new ListItem( assetStorageProvider.Name, assetStorageProvider.Id.ToString() ) );
                    }
                }
            }

            this.SetValue( selectedItem );

        }

    }
}
