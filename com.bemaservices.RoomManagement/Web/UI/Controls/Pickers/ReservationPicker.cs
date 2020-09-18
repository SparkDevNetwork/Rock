// <copyright>
// Copyright by BEMA Software Services
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
using System.Web.UI.WebControls;

using com.bemaservices.RoomManagement.Model;

using Rock;
using Rock.Web.UI.Controls;

namespace com.bemaservices.RoomManagement.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class ReservationPicker : RockDropDownList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationPicker" /> class.
        /// </summary>
        public ReservationPicker()
            : base()
        {
            Label = "Reservation";
        }

        /// <summary>
        /// Gets or sets the reservations.
        /// </summary>
        /// <value>
        /// The reservations.
        /// </value>
        public List<Reservation> Reservations
        {
            set
            {
                this.Items.Clear();
                this.Items.Add( new ListItem() );

                foreach ( Reservation reservation in value )
                {
                    this.Items.Add( new ListItem( reservation.Name, reservation.Id.ToString() ) );
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected reservation identifier.
        /// </summary>
        /// <value>
        /// The selected reservation identifier.
        /// </value>
        public int? SelectedReservationId
        {
            get
            {
                return this.SelectedValueAsInt();
            }

            set
            {
                int id = value.HasValue ? value.Value : 0;
                var li = this.Items.FindByValue( id.ToString() );
                if ( li != null )
                {
                    li.Selected = true;
                }
            }
        }
    }
}