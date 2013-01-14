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
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;
using Rock.Web.Cache;

namespace RockWeb.Blocks.CheckIn
{
    [Description( "Check-In Family Select block" )]
    public partial class PersonSelect : CheckInBlock
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
                    var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                    if ( family != null )
                    {
                        lFamilyName.Text = family.ToString();

                        //if ( family.People.Count == 1 )
                        //{
                        //    if ( UserBackedUp )
                        //    {
                        //        GoBack();
                        //    }
                        //    else
                        //    {
                        //        foreach ( var familyMember in family.People )
                        //        {
                        //            familyMember.Selected = true;
                        //        }

                        //        ProcessSelection();
                        //    }
                        //}
                        //else
                        //{
                            foreach ( var familyMember in family.People )
                            {
                                lbMembers.Items.Add( new ListItem( familyMember.ToString(), familyMember.Person.Id.ToString() ) );
                            }
                        //}
                    }
                    else
                    {
                        GoBack();
                    }
                }
            }
        }

        protected void lbSelect_Click( object sender, EventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                if ( lbMembers.SelectedItem != null )
                {
                    var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                    if ( family != null )
                    {
                        int id = Int32.Parse( lbMembers.SelectedItem.Value );
                        var familyMember = family.People.Where( m => m.Person.Id == id ).FirstOrDefault();
                        if ( familyMember != null )
                        {
                            familyMember.Selected = true;
                            ProcessSelection();
                        }
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
            foreach ( var family in CurrentCheckInState.CheckIn.Families )
            {
                family.Selected = false;
                family.People = new List<CheckInPerson>();
            }

            SaveState();

            GoToFamilySelectPage( true );
        }

        private void ProcessSelection()
        {
            SaveState();
            GoToGroupTypeSelectPage();
        }


    }
}