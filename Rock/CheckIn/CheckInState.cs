//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.CheckIn
{
    /// <summary>
    /// Object for maintaining the state of a check-in kiosk and workflow
    /// </summary>
    public class CheckInState
    {
        /// <summary>
        /// Gets or sets the kiosk status
        /// </summary>
        /// <value>
        /// The kiosk.
        /// </value>
        public KioskStatus Kiosk { get; set; }

        /// <summary>
        /// Gets or sets the check in status
        /// </summary>
        /// <value>
        /// The check in.
        /// </value>
        public CheckInStatus CheckIn { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInState" /> class.
        /// </summary>
        /// <param name="kioskStatus">The kiosk status.</param>
        public CheckInState( KioskStatus kioskStatus )
        {
            Kiosk = kioskStatus;
            CheckIn = new CheckInStatus();
        }
    }
}