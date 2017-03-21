// <copyright>
// Copyright by Central Christian Church
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

using com.centralaz.RoomManagement.Model;

using Rock;
using Rock.Web.UI.Controls;

namespace com.centralaz.RoomManagement.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class ReservationStatusPicker : RockDropDownList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationStatusPicker" /> class.
        /// </summary>
        public ReservationStatusPicker()
            : base()
        {
            Label = "ReservationStatus";
        }

        /// <summary>
        /// Gets or sets the reservationStatuses.
        /// </summary>
        /// <value>
        /// The reservationStatuses.
        /// </value>
        public List<ReservationStatus> ReservationStatuses
        {
            set
            {
                this.Items.Clear();
                this.Items.Add( new ListItem() );

                foreach ( ReservationStatus reservationStatus in value )
                {
                    this.Items.Add( new ListItem( reservationStatus.Name, reservationStatus.Id.ToString() ) );
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected reservationStatus identifier.
        /// </summary>
        /// <value>
        /// The selected reservationStatus identifier.
        /// </value>
        public int? SelectedReservationStatusId
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