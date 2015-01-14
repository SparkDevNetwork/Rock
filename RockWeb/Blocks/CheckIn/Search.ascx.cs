// <copyright>
// Copyright 2013 by the Spark Development Network
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
            }
        }

        protected void lbSearch_Click( object sender, EventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                // TODO: Validate text entered
                int minLength = int.Parse( GetAttributeValue( "MinimumPhoneNumberLength" ) );
                int maxLength = int.Parse( GetAttributeValue( "MaximumPhoneNumberLength" ) );
                if ( tbPhone.Text.Length >= minLength && tbPhone.Text.Length <= maxLength )
                {
                    CurrentCheckInState.CheckIn.UserEnteredSearch = true;
                    CurrentCheckInState.CheckIn.ConfirmSingleFamily = true;
                    CurrentCheckInState.CheckIn.SearchType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER );
                    CurrentCheckInState.CheckIn.SearchValue = tbPhone.Text;

                    ProcessSelection();
                }
                else
                {
                    string errorMsg = ( tbPhone.Text.Length > maxLength )
                        ? string.Format( "<ul><li>Please enter no more than {0} numbers</li></ul>", maxLength )
                        : string.Format( "<ul><li>Please enter at least {0} numbers</li></ul>", minLength );

                    maWarning.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
                }
            }
        }

        protected void ProcessSelection()
        {
            ProcessSelection( maWarning, () => CurrentCheckInState.CheckIn.Families.Count <= 0 , "<ul><li>There are not any families with the selected phone number</li></ul>" );
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