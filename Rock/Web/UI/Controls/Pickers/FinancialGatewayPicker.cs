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
using Rock.Financial;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class FinancialGatewayPicker : RockDropDownList
    {
        /// <summary>
        /// Gets or sets a value indicating whether all gateways should be included. If set to false, only gateways
        /// that are active and support rock initiated transactions will be included.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show all]; otherwise, <c>false</c>.
        /// </value>
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

        /// <summary>
        /// Handles the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            LoadItems( ShowAll );
        }

        /// <summary>
        /// Loads the items.
        /// </summary>
        /// <param name="showAll">if set to <c>true</c> [show all].</param>
        private void LoadItems( bool showAll )
        {
            int? selectedItem = this.SelectedValueAsInt();

            this.Items.Clear();
            this.Items.Add( new ListItem() );

            using ( var rockContext = new RockContext() )
            {
                foreach ( var gateway in new FinancialGatewayService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( g => g.EntityTypeId.HasValue )
                    .OrderBy( g => g.Name )
                    .ToList() )
                {
                    var entityType = EntityTypeCache.Get( gateway.EntityTypeId.Value );
                    GatewayComponent component = GatewayContainer.GetComponent( entityType.Name );
                    if ( showAll || ( gateway.IsActive && component != null && component.IsActive && component.SupportsRockInitiatedTransactions ) )
                    {
                        this.Items.Add( new ListItem( gateway.Name, gateway.Id.ToString() ) );
                    }
                }
            }

            this.SetValue( selectedItem );

        }
    }
}