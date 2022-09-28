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
    /// Control that can be used to select a financial gateway
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
        [RockObsolete( "1.14" )]
        [Obsolete( "Use IncludeInactive instead" )]
        public bool ShowAll
        {
            get
            {
                return IncludeInactive;
            }

            set
            {
                IncludeInactive = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether inactive gateways should be included.
        /// This checks both the FinancialGateway model and the GatewayComponent.
        /// </summary>
        /// <value><c>true</c> if [show inactive]; otherwise, <c>false</c>.</value>
        public bool IncludeInactive
        {
            get
            {
                return ViewState["IncludeInactive"] as bool? ?? false;
            }

            set
            {
                if ( IncludeInactive != value )
                {
                    ViewState["IncludeInactive"] = value;
                    LoadItems();
                }
            }
        }

        /// <summary>
        /// If set to true then gateways that do not support Rock initiated transactions will be included.
        /// These GatewayComponents are used to download externally created transactions and do not allow Rock
        /// to create the transaction.
        /// <strong>This property does not affect if inactive gateways are shown or not.</strong>
        /// The inclusion or exclusion of inactive gateways is controlled exclusively by the "IncludeInactive" property.
        /// </summary>
        /// <value><c>true</c> if [show all gateway components]; otherwise, <c>false</c>.</value>
        public bool ShowAllGatewayComponents
        {
            get
            {
                return ViewState["ShowAllGatewayComponents"] as bool? ?? false;
            }

            set
            {
                if ( ShowAllGatewayComponents != value )
                {
                    ViewState["ShowAllGatewayComponents"] = value;
                    LoadItems();
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
            LoadItems();
        }

        /// <summary>
        /// Loads the items.
        /// </summary>
        private void LoadItems()
        {
            int? selectedItem = this.SelectedValueAsInt();

            this.Items.Clear();
            this.Items.Add( new ListItem() );

            using ( var rockContext = new RockContext() )
            {
                var gateways = new FinancialGatewayService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( g => g.EntityTypeId.HasValue )
                    .OrderBy( g => g.Name )
                    .ToList();

                foreach ( var gateway in gateways )
                {
                    var entityType = EntityTypeCache.Get( gateway.EntityTypeId.Value );
                    GatewayComponent component = GatewayContainer.GetComponent( entityType.Name );

                    // Add the gateway if was already selected or if the control is configured to show all of the gateways.
                    if ( gateway.Id == selectedItem || ( IncludeInactive && ShowAllGatewayComponents ) )
                    {
                        this.Items.Add( new ListItem( gateway.Name, gateway.Id.ToString() ) );
                        continue;
                    }

                    // Do not add if the component or gateway is not active and the controls has ShowInactive set to false.
                    if ( IncludeInactive == false && ( gateway.IsActive == false || component == null || component.IsActive == false ) )
                    {
                        continue;
                    }

                    if ( ShowAllGatewayComponents == false && ( component == null || component.SupportsRockInitiatedTransactions == false ) )
                    {
                        continue;
                    }

                    // If we get this far add the gateway.
                    this.Items.Add( new ListItem( gateway.Name, gateway.Id.ToString() ) );
                }
            }

            this.SetValue( selectedItem );
        }
    }
}