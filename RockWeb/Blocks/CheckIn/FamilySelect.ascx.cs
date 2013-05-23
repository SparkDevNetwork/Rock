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
using System.Web.UI.WebControls;

using Rock;
using Rock.CheckIn;
using Rock.Model;

namespace RockWeb.Blocks.CheckIn
{
    [Description( "Check-In Family Select Block" )]
    public partial class FamilySelect : CheckInBlock
    {
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( CurrentWorkflow == null || CurrentCheckInState == null )
            {
                NavigateToHomePage();
            }
            else
            {
                if ( !Page.IsPostBack )
                {
                    if ( CurrentCheckInState.CheckIn.Families.Count == 1 &&
                        !CurrentCheckInState.CheckIn.ConfirmSingleFamily )
                    {
                        if ( UserBackedUp )
                        {
                            GoBack();
                        }
                        else
                        {
                            foreach ( var family in CurrentCheckInState.CheckIn.Families )
                            {
                                family.Selected = true;
                            }

                            ProcessSelection();
                        }
                    }
                    else
                    {
                        rSelection.DataSource = CurrentCheckInState.CheckIn.Families;
                        rSelection.DataBind();
                    }
                }
            }
        }

        protected void rSelection_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                int id = Int32.Parse( e.CommandArgument.ToString() );
                var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Group.Id == id ).FirstOrDefault();
                if ( family != null )
                {
                    family.Selected = true;
                    ProcessSelection();
                }
            }
        }

        protected void lbBack_Click( object sender, EventArgs e )
        {
            GoBack();
        }

        protected void lbCancel_Click( object sender, EventArgs e )
        {
            CancelCheckin();
        }

        private void GoBack()
        {
            if ( CurrentCheckInState != null && CurrentCheckInState.CheckIn != null )
            {
                CurrentCheckInState.CheckIn.SearchType = null;
                CurrentCheckInState.CheckIn.SearchValue = string.Empty;
                CurrentCheckInState.CheckIn.Families = new List<CheckInFamily>();
            }

            SaveState();

            if ( CurrentCheckInState.CheckIn.UserEnteredSearch )
            {
                NavigateToPreviousPage();
            }
            else
            {
                NavigateToHomePage();
            }
        }

        private void ProcessSelection()
        {
            var errors = new List<string>();
            if ( ProcessActivity( "Person Search", out errors ) )
            {
                if ( CurrentCheckInState.CheckIn.Families.All( f => f.People.Count == 0 ) )
                {
                    string errorMsg = "<ul><li>No one in that family is eligible to check-in.</li></ul>";
                    maWarning.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
                }
                else
                {
                    SaveState();
                    NavigateToNextPage();
                }
            }
            else
            {
                string errorMsg = "<ul><li>" + errors.AsDelimited( "</li><li>" ) + "</li></ul>";
                maWarning.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
            }
        }


    }
}