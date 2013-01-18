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
    [Description( "Check-In Family Select block" )]
    public partial class FamilySelect : CheckInBlock
    {
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( CurrentWorkflow == null || CurrentCheckInState == null )
            {
                GoToWelcomePage();
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
                        foreach ( var family in CurrentCheckInState.CheckIn.Families )
                        {
                            lbFamilies.Items.Add( new ListItem( family.ToString(), family.Group.Id.ToString() ) );
                        }
                    }
                }
            }
        }

        protected void lbSelect_Click( object sender, EventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                if ( lbFamilies.SelectedItem != null )
                {
                    int id = Int32.Parse( lbFamilies.SelectedItem.Value );
                    var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Group.Id == id ).FirstOrDefault();
                    if ( family != null )
                    {
                        family.Selected = true;
                        ProcessSelection();
                    }
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
            CurrentCheckInState.CheckIn.SearchType = null;
            CurrentCheckInState.CheckIn.SearchValue = string.Empty;
            CurrentCheckInState.CheckIn.Families = new List<CheckInFamily>();

            SaveState();

            if ( CurrentCheckInState.CheckIn.UserEnteredSearch )
            {
                GoToSearchPage( true );
            }
            else
            {
                GoToWelcomePage();
            }
        }

        private void ProcessSelection()
        {
            var errors = new List<string>();
            if ( ProcessActivity( "Person Search", out errors ) )
            {
                SaveState();
                GoToPersonSelectPage();
            }
            else
            {
                string errorMsg = "<ul><li>" + errors.AsDelimited( "</li><li>" ) + "</li></ul>";
                maWarning.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
            }
        }


    }
}