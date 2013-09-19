//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
    [Description( "Check-in Family Search block" )]
    [IntegerField( "Minimum Phone Number Length", "Minimum length for phone number searches (defaults to 4).", false, 4 )]
    [IntegerField( "Maximum Phone Number Length", "Maximum length for phone number searches (defaults to 10).", false, 10 )]
    public partial class Search : CheckInBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if (!KioskCurrentlyActive)
            {
                NavigateToHomePage();
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