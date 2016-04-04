﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Web.Cache;

namespace RockWeb.Blocks.CheckIn
{
    [DisplayName("Search")]
    [Category("Check-in")]
    [Description("Displays keypad for searching on phone numbers.")]
    [IntegerField( "Minimum Phone Number Length", "Minimum length for phone number searches (defaults to 4).", false, 4 )]
    [IntegerField( "Maximum Phone Number Length", "Maximum length for phone number searches (defaults to 10).", false, 10 )]
    [TextField("Search Regex", "Regular Expression to run the search input through before sending it to the workflow. Useful for stripping off characters.", false)]
    [DefinedValueField(Rock.SystemGuid.DefinedType.CHECKIN_SEARCH_TYPE, "Search Type", "The type of search to use for check-in (default is phone number).", true, false, Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER, order: 4 )]
    public partial class Search : CheckInBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddScriptLink( "~/Scripts/iscroll.js" );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );

            if (!KioskCurrentlyActive)
            {
                NavigateToHomePage();
            }
        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            if ( !Page.IsPostBack )
            {
                this.Page.Form.DefaultButton = lbSearch.UniqueID;

                // set search type
                var searchTypeValue = GetAttributeValue( "SearchType" ).AsGuid();
                if ( searchTypeValue == Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER.AsGuid() )
                {
                    pnlSearchName.Visible = false;
                    pnlSearchPhone.Visible = true;
                    lPageTitle.Text = "Search By Phone";
                }
                else if ( searchTypeValue == Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME.AsGuid() )
                {
                    pnlSearchName.Visible = true;
                    pnlSearchPhone.Visible = false;
                    lPageTitle.Text = "Search By Name";
                }
                else
                {
                    pnlSearchName.Visible = true;
                    pnlSearchPhone.Visible = false;
                    txtName.Label = "Name or Phone";
                    lPageTitle.Text = "Search By Name or Phone";
                }
            }
        }

        protected void lbSearch_Click( object sender, EventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                // check search type
                var searchTypeValue = GetAttributeValue( "SearchType" ).AsGuid();

                if ( searchTypeValue == Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER.AsGuid() )
                {
                    SearchByPhone();
                }
                else if ( searchTypeValue == Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME.AsGuid() )
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

        private void SearchByName()
        {
            CurrentCheckInState.CheckIn.UserEnteredSearch = true;
            CurrentCheckInState.CheckIn.ConfirmSingleFamily = true;
            CurrentCheckInState.CheckIn.SearchType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME );
            CurrentCheckInState.CheckIn.SearchValue = txtName.Text;

            ProcessSelection();
        }

        private void SearchByPhone()
        {
            // TODO: Validate text entered
            int minLength = int.Parse( GetAttributeValue( "MinimumPhoneNumberLength" ) );
            int maxLength = int.Parse( GetAttributeValue( "MaximumPhoneNumberLength" ) );
            if ( tbPhone.Text.Length >= minLength && tbPhone.Text.Length <= maxLength )
            {
                string searchInput = tbPhone.Text;

                // run regex expression on input if provided
                if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "SearchRegex" ) ) )
                {
                    Regex regex = new Regex( GetAttributeValue( "SearchRegex" ) );
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
                CurrentCheckInState.CheckIn.SearchType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER );
                CurrentCheckInState.CheckIn.SearchValue = searchInput;

                ProcessSelection();
            }
            else
            {
                string errorMsg = ( tbPhone.Text.Length > maxLength )
                    ? string.Format( "<p>Please enter no more than {0} numbers</p>", maxLength )
                    : string.Format( "<p>Please enter at least {0} numbers</p>", minLength );

                maWarning.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
            }
        }

        protected void ProcessSelection()
        {
            ProcessSelection( maWarning, () => CurrentCheckInState.CheckIn.Families.Count <= 0 , "<p>There are not any families with the selected phone number</p>" );
        }

        protected void lbBack_Click( object sender, EventArgs e )
        {
            CancelCheckin();
        }

        protected void lbCancel_Click( object sender, EventArgs e )
        {
            CancelCheckin();
        }
    }
}