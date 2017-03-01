using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Rock.Model;
using Rock.Web.Cache;
using com.centralaz.RoomManagement.Model;
using Rock.Web.UI.Controls;
using Rock;
using Rock.Data;

namespace com.centralaz.RoomManagement.Web.UI.Controls
{
    /// <summary>
    /// Select multiple reservation statuses
    /// NOTE: Reservation statuses must be set first (it doesn't automatically load reservation statuses).
    /// </summary>
    public class ReservationStatusesPicker : RockCheckBoxList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationStatusesPicker" /> class.
        /// </summary>
        public ReservationStatusesPicker()
            : base()
        {
            Label = "Reservation Statuses";
            RepeatDirection = RepeatDirection.Horizontal;
        }

        /// <summary>
        /// Gets or sets the reservation statuses.
        /// </summary>
        /// <value>
        /// The reservation statuses.
        /// </value>
        public List<ReservationStatus> ReservationStatuses
        {
            set
            {
                this.Items.Clear();
                foreach ( ReservationStatus reservationStatus in value )
                {
                    ListItem reservationStatusItem = new ListItem();
                    reservationStatusItem.Value = reservationStatus.Id.ToString();
                    reservationStatusItem.Text = reservationStatus.Name;
                    this.Items.Add( reservationStatusItem );
                }
            }
        }

        /// <summary>
        /// Gets the available reservation status ids.
        /// </summary>
        /// <value>
        /// The available reservationStatus ids.
        /// </value>
        public List<int> AvailableReservationStatusIds
        {
            get
            {
                return this.Items.OfType<ListItem>().Select( a => a.Value ).AsIntegerList();
            }
        }

        /// <summary>
        /// Gets the selected reservation status ids.
        /// </summary>
        /// <value>
        /// The selected reservation status ids.
        /// </value>
        public List<int> SelectedReservationStatusIds
        {
            get
            {
                return this.Items.OfType<ListItem>().Where( l => l.Selected ).Select( a => a.Value ).AsIntegerList();
            }

            set
            {
                foreach ( ListItem reservationStatusItem in this.Items )
                {
                    reservationStatusItem.Selected = value.Exists( a => a.Equals( reservationStatusItem.Value.AsInteger() ) );
                }
            }
        }
    }
}