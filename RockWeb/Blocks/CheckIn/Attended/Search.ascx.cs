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
    [IntegerField( "Minimum Text Length", "Minimum length for text searches (defaults to 4).", false, 4 )]
    [IntegerField( "Maximum Text Length", "Maximum length for text searches (defaults to 20).", false, 20 )]
    public partial class Search : CheckInBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                if ( CurrentKioskId == null  || CurrentGroupTypeIds == null )
                {
                    var queryParams = new Dictionary<string, string>();
                    queryParams.Add( "back", "true" );
                    NavigateToLinkedPage( "AdminPage" );
                }
                else
                {
                    if ( !CurrentCheckInState.Kiosk.HasLocations( CurrentGroupTypeIds ) || !CurrentCheckInState.Kiosk.HasActiveLocations( CurrentGroupTypeIds ) )
                    {
                        DateTimeOffset activeAt = CurrentCheckInState.Kiosk.FilteredGroupTypes( CurrentGroupTypeIds ).Select( g => g.NextActiveTime ).Min();
                        // not active yet, display next active time
                        return;
                    }
                    else if ( CurrentCheckInState.CheckIn.SearchType != null || CurrentCheckInState.CheckIn.Families.Count > 0)
                    {
                        if ( !string.IsNullOrWhiteSpace( CurrentCheckInState.CheckIn.SearchValue ) )
                        {
                            tbSearchBox.Text = CurrentCheckInState.CheckIn.SearchValue;
                        }
                        lbAdmin.Visible = false;
                        lbBack.Visible = true;
                    }

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

                    CurrentWorkflow = null;
                    tbSearchBox.Focus();
                    SaveState();
                }
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
            if ( CurrentCheckInState != null && CurrentCheckInState.Kiosk != null )
            {
                CurrentCheckInState.CheckIn.Families.Clear();
                CurrentCheckInState.CheckIn.UserEnteredSearch = true;
                CurrentCheckInState.CheckIn.ConfirmSingleFamily = true;

                int minLength = int.Parse( GetAttributeValue( "MinimumTextLength" ) );
                int maxLength = int.Parse( GetAttributeValue( "MaximumTextLength" ) );
                if ( tbSearchBox.Text.Length >= minLength && tbSearchBox.Text.Length <= maxLength )
                {
                    int searchNumber;
                    if ( int.TryParse( tbSearchBox.Text, out searchNumber ) )
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
                    string errorMsg = ( tbSearchBox.Text.Length > maxLength )
                        ? string.Format( "<ul><li>Please enter no more than {0} characters</li></ul>", maxLength )
                        : string.Format( "<ul><li>Please enter at least {0} characters</li></ul>", minLength );

                    maWarning.Show( errorMsg, ModalAlertType.Warning );
                    return;
                }
            }
            else
            {
                maWarning.Show( "This kiosk is not currently active.", ModalAlertType.Warning );
                return;
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAdmin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAdmin_Click( object sender, EventArgs e )
        {
            var queryParams = new Dictionary<string, string>();
            queryParams.Add( "back", "true" );
            NavigateToLinkedPage( "AdminPage", queryParams );
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
    }
}