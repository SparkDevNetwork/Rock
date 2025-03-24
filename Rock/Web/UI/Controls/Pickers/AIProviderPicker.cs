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
using System.Web.UI.WebControls;

using Rock.AI.Provider;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.Cache.Entities;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// Control than can be used to select an AI provider.
    /// </summary>
    class AIProviderPicker : RockDropDownList
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

            foreach ( var aiProviderCache in AIProviderCache.All()
                .Where( a => a.EntityTypeId.HasValue )
                .OrderBy( a => a.Name )
                .ToList() )
            {
                var entityType = EntityTypeCache.Get( aiProviderCache.EntityTypeId.Value );
                AIProviderComponent component = aiProviderCache.AIComponent;
                if ( showAll || ( aiProviderCache.IsActive && component != null && component.IsActive ) )
                {
                    this.Items.Add( new ListItem( aiProviderCache.Name, aiProviderCache.Id.ToString() ) );
                }
            }

            this.SetValue( selectedItem );
        }
    }
}