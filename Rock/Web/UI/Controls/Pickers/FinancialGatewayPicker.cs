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
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Financial;
using Rock.Model;

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
        /// Initializes a new instance of the <see cref="FinancialGatewayPicker" /> class.
        /// </summary>
        public FinancialGatewayPicker()
        {
            LoadItems( false );
        }

        private void LoadItems( bool showAll )
        {
            int? selectedItem = this.SelectedValueAsInt();

            this.Items.Clear();
            this.Items.Add( new ListItem() );

            using ( var rockContext = new RockContext() )
            {
                foreach ( var gateway in new FinancialGatewayService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( g => g.EntityType != null )
                    .OrderBy( g => g.Name )
                    .ToList() )
                {
                    GatewayComponent component = GatewayContainer.GetComponent( gateway.EntityType.Name );
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