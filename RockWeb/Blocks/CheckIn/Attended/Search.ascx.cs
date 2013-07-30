//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Constants;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.CheckIn.Attended
{
    [Description( "Attended Check-In Search block" )]
    [LinkedPage( "Admin Page" )]
    [IntegerField( "Minimum Phone Number Length", "Minimum length for phone number searches (defaults to 4).", false, 4 )]
    [IntegerField( "Maximum Phone Number Length", "Maximum length for phone number searches (defaults to 10).", false, 10 )]
    public partial class Search : CheckInBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            tbSearchBox.Focus();
            if ( !Page.IsPostBack && CurrentCheckInState != null )
            {
                string script = string.Format( @"
                <script>
                    $(document).ready(function (e) {{
                        if (localStorage) {{
                            localStorage.checkInKiosk = '{0}';
                            localStorage.checkInGroupTypes = '{1}';
                        }}
                    }});
                </script>
                ", CurrentKioskId, CurrentGroupTypeIds.AsDelimited( "," ) );
                phScript.Controls.Add( new LiteralControl( script ) );

                if ( CurrentCheckInState.Kiosk.KioskGroupTypes.Count == 0 )
                {
                    // throw error
                }
                else if ( !CurrentCheckInState.Kiosk.HasLocations )
                {
                    DateTimeOffset activeAt = CurrentCheckInState.Kiosk.KioskGroupTypes.Select( g => g.NextActiveTime ).Min();
                    // not active yet
                }
                else if ( !CurrentCheckInState.Kiosk.HasActiveLocations )
                {
                    // not available
                }
                else
                {
                    // active
                }

                //if ( CurrentKioskId != null || UserBackedUp )
                if ( CurrentCheckInState.CheckIn.SearchType != null || UserBackedUp )
                {
                    lbAdmin.Visible = false;
                    lbBack.Visible = true;
                }
                else
                {
                    lbAdmin.Visible = true;
                    lbBack.Visible = false;
                }

                CurrentWorkflow = null;
                SaveState();
                //LoadLocations();
            }
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the lbSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSearch_Click( object sender, EventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                CurrentCheckInState.CheckIn.UserEnteredSearch = true;
                CurrentCheckInState.CheckIn.ConfirmSingleFamily = true;

                if ( !string.IsNullOrWhiteSpace( tbSearchBox.Text ) )
                {
                    if ( tbSearchBox.Text.AsNumeric().Length == tbSearchBox.Text.Length )
                    {
                        CurrentCheckInState.CheckIn.SearchType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER );
                    }
                    else 
                    {
                        CurrentCheckInState.CheckIn.SearchType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME );                        
                    }

                    CurrentCheckInState.CheckIn.SearchValue = tbSearchBox.Text;
                    var errors = new List<string>();
                    if ( ProcessActivity( "Family Search", out errors ) )
                    {
                        SaveState();
                        NavigateToNextPage();
                    }
                    else
                    {
                        string errorMsg = "<ul><li>" + errors.AsDelimited( "</li><li>" ) + "</li></ul>";
                        maWarning.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
                    }           
                }
                else
                {
                    maWarning.Show( "Please enter something to search for.", ModalAlertType.Warning );
                    return;
                }               
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAdmin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAdmin_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "AdminPage" );
        }

        /// <summary>
        /// Handles the Click event of the lbBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbBack_Click( object sender, EventArgs e )
        {
            NavigateToPreviousPage();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Loads the locations.
        /// </summary>
        private void LoadLocations()
        {
            List<int> locations = new List<int>();

            foreach ( var groupType in CurrentCheckInState.Kiosk.KioskGroupTypes )
            {
                foreach ( var location in groupType.KioskLocations )
                {
                    if ( !locations.Contains( location.Location.Id ) )
                    {
                        locations.Add( location.Location.Id );
                    }
                }
            }
        }
        
        #endregion
    }
}