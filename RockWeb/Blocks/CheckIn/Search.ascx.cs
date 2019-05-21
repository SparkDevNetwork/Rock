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
using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI.HtmlControls;
using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Web.Cache;

namespace RockWeb.Blocks.CheckIn
{
    /// <summary>
    /// Search Block for Checkin
    /// </summary>
    /// <seealso cref="Rock.CheckIn.CheckInSearchBlock" />
    [DisplayName( "Search" )]
    [Category( "Check-in" )]
    [Description( "Searches by name or phone number depending on settings." )]

    [TextField( "Title", "Title to display. Use {0} for search type.", false, "Search", "Text", 5 )]
    [TextField( "No Option Message", "", false, "There were not any families that match the search criteria.", "Text", 6 )]
    public partial class Search : CheckInSearchBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );

            if ( !KioskCurrentlyActive )
            {
                NavigateToHomePage();
            }

            var bodyTag = this.Page.Master.FindControl( "bodyTag" ) as HtmlGenericControl;
            if ( bodyTag != null )
            {
                bodyTag.AddCssClass( "checkin-search-bg" );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            if ( !Page.IsPostBack )
            {
                this.Page.Form.DefaultButton = lbSearch.UniqueID;
                string searchType = "Phone";

                // If mobile and the last phone number is saved in the cookie
                if ( Request.Cookies[CheckInCookie.ISMOBILE] != null && Request.Cookies[CheckInCookie.PHONENUMBER] != null )
                {
                    tbPhone.Text = Request.Cookies[CheckInCookie.PHONENUMBER].Value;
                }

                if ( CurrentCheckInType != null && this.CurrentCheckInState.Kiosk.RegistrationModeEnabled )
                {
                    // If RegistrationMode is enabled for this device, override any SearchType settings and search by Phone or Name
                    pnlSearchName.Visible = true;
                    pnlSearchPhone.Visible = false;
                    txtName.Label = "Phone or Name";
                    searchType = "Phone or Name";
                }
                else
                {
                    if ( CurrentCheckInType == null || CurrentCheckInType.SearchType.Guid == Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER.AsGuid() )
                    {
                        pnlSearchName.Visible = false;
                        pnlSearchPhone.Visible = true;
                        searchType = "Phone";
                    }
                    else if ( CurrentCheckInType.SearchType.Guid == Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME.AsGuid() )
                    {
                        pnlSearchName.Visible = true;
                        pnlSearchPhone.Visible = false;
                        searchType = "Name";
                    }
                    else
                    {
                        pnlSearchName.Visible = true;
                        pnlSearchPhone.Visible = false;
                        txtName.Label = "Phone or Name";
                        searchType = "Phone or Name";
                    }
                }

                lPageTitle.Text = string.Format( GetAttributeValue( "Title" ), searchType );
            }
            else
            {
                // make sure the ShowAddFamilyPrompt is disabled so that it doesn't show again until explicitly enabled after doing a Search
                hfShowAddFamilyPrompt.Value = "0";

                if ( this.Request.Params["__EVENTARGUMENT"] == "AddFamily" )
                {
                    var editFamilyBlock = this.RockPage.ControlsOfTypeRecursive<CheckInEditFamilyBlock>().FirstOrDefault();
                    if ( editFamilyBlock != null )
                    {
                        editFamilyBlock.ShowAddFamily();
                    }
                }
            }
        }

        /// <summary>
        /// Searches for Families based on the searchString
        /// </summary>
        /// <param name="searchString">The search string.</param>
        public override void ProcessSearch( string searchString )
        {
            if ( KioskCurrentlyActive )
            {
                txtName.Text = searchString;
                if ( !searchString.Any( c => char.IsLetter( c ) ) )
                {
                    tbPhone.Text = searchString;
                }

                Guid searchTypeGuid = Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER.AsGuid();
                if ( ( this.CurrentCheckInState != null ) && this.CurrentCheckInState.Kiosk.RegistrationModeEnabled )
                {
                    // If RegistrationMode is enabled for this device, override any SearchType settings and search by Phone or Name
                    searchTypeGuid = Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME_AND_PHONE.AsGuid();
                }
                else if ( CurrentCheckInType != null )
                {
                    searchTypeGuid = CurrentCheckInType.SearchType.Guid;
                }

                // check search type
                if ( searchTypeGuid == Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER.AsGuid() )
                {
                    SearchByPhone();
                }
                else if ( searchTypeGuid == Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME.AsGuid() )
                {
                    SearchByName();
                }
                else
                {
                    // name and phone search (this option uses the name search panel as UI)
                    if ( txtName.Text.Any( c => char.IsLetter( c ) ) )
                    {
                        SearchByName();
                    }
                    else
                    {
                        tbPhone.Text = txtName.Text;
                        SearchByPhone();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSearch_Click( object sender, EventArgs e )
        {
            if ( tbPhone.Visible )
            {
                ProcessSearch( tbPhone.Text );
            }
            else
            {
                ProcessSearch( txtName.Text );
            }
        }

        /// <summary>
        /// Searches for the Family/Individual by Name
        /// </summary>
        private void SearchByName()
        {
            CurrentCheckInState.CheckIn.UserEnteredSearch = true;
            CurrentCheckInState.CheckIn.ConfirmSingleFamily = true;
            CurrentCheckInState.CheckIn.SearchType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME );
            CurrentCheckInState.CheckIn.SearchValue = txtName.Text;

            ProcessSelection();
        }

        /// <summary>
        /// Searches for the Family/Individual by Phone Number
        /// </summary>
        private void SearchByPhone()
        {
            // TODO: Validate text entered
            int minLength = CurrentCheckInType != null ? CurrentCheckInType.MinimumPhoneSearchLength : 4;
            int maxLength = CurrentCheckInType != null ? CurrentCheckInType.MaximumPhoneSearchLength : 10;
            if ( tbPhone.Text.Length >= minLength && tbPhone.Text.Length <= maxLength )
            {
                string searchInput = tbPhone.Text;

                // run regex expression on input if provided
                if ( CurrentCheckInType != null && !string.IsNullOrWhiteSpace( CurrentCheckInType.RegularExpressionFilter ) )
                {
                    Regex regex = new Regex( CurrentCheckInType.RegularExpressionFilter );
                    Match match = regex.Match( searchInput );
                    if ( match.Success )
                    {
                        if ( match.Groups.Count == 2 )
                        {
                            searchInput = match.Groups[1].ToString();
                        }
                    }
                }

                CurrentCheckInState.CheckIn.UserEnteredSearch = true;
                CurrentCheckInState.CheckIn.ConfirmSingleFamily = true;
                CurrentCheckInState.CheckIn.SearchType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER );
                CurrentCheckInState.CheckIn.SearchValue = searchInput;

                if ( ProcessSelection() && Request.Cookies[CheckInCookie.ISMOBILE] != null )
                {
                    SavePhoneCookie( tbPhone.Text );
                }
            }
            else
            {
                string errorMsg = ( tbPhone.Text.Length > maxLength )
                    ? string.Format( "Please enter no more than {0} numbers", maxLength )
                    : string.Format( "Please enter at least {0} numbers", minLength );

                maWarning.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.None );
            }
        }

        /// <summary>
        /// Gets the condition message.
        /// </summary>
        /// <value>
        /// The condition message.
        /// </value>
        protected string ConditionMessage
        {
            get
            {
                string conditionMessage = string.Format( "<p>{0}</p>", GetAttributeValue( "NoOptionMessage" ) );
                return conditionMessage;
            }
        }

        /// <summary>
        /// Processes the selection returning true if it was successful; false otherwise.
        /// </summary>
        /// <returns>true if it was successful; false otherwise.</returns>
        protected bool ProcessSelection()
        {
            var editFamilyBlock = this.RockPage.ControlsOfTypeRecursive<CheckInEditFamilyBlock>().FirstOrDefault();

            hfShowAddFamilyPrompt.Value = "0";

            Func<bool> doNotProceedCondition = () =>
            {
                if ( CurrentCheckInState.CheckIn.Families.Count == 0 )
                {
                    if ( CurrentCheckInState.Kiosk.RegistrationModeEnabled && editFamilyBlock != null )
                    {
                        hfShowAddFamilyPrompt.Value = "1";
                        return true;
                    }
                    else
                    {
                        maWarning.Show( this.ConditionMessage, Rock.Web.UI.Controls.ModalAlertType.None );
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            };

            return ProcessSelection( null, doNotProceedCondition, this.ConditionMessage );
        }

        /// <summary>
        /// Save the phone number in a cookie.
        /// </summary>
        /// <param name="kiosk"></param>
        private void SavePhoneCookie( string phoneNumber )
        {
            HttpCookie phoneCookie = new HttpCookie( CheckInCookie.PHONENUMBER, phoneNumber );
            Response.Cookies.Set( phoneCookie );
        }

        /// <summary>
        /// Handles the Click event of the lbBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbBack_Click( object sender, EventArgs e )
        {
            CancelCheckin();
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            CancelCheckin();
        }
    }
}