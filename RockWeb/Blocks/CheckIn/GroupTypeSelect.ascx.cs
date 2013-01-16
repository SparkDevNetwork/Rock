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
    [Description( "Check-In Group Type Select block" )]
    public partial class GroupTypeSelect : CheckInBlock
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
                        var person = family.People.Where( p => p.Selected ).FirstOrDefault();
                        if ( person != null )
                        {
                            lPersonName.Text = person.Person.FullName;

                            if ( person.GroupTypes.Count == 1 )
                            {
                                if ( UserBackedUp )
                                {
                                    GoBack();
                                }
                                else
                                {
                                    foreach ( var groupType in person.GroupTypes )
                                    {
                                        groupType.Selected = true;
                                    }

                                    ProcessSelection();
                                }
                            }
                            else
                            {
                                int? defaultId = person.GroupTypes.OrderByDescending( g => g.LastCheckIn ).Select( g => g.GroupType.Id ).FirstOrDefault();
                                foreach ( var groupType in person.GroupTypes )
                                {
                                    ListItem item = new ListItem( groupType.ToString(), groupType.GroupType.Id.ToString() );
                                    if ( defaultId.HasValue && groupType.GroupType.Id == defaultId.Value )
                                    {
                                        item.Selected = true;
                                    }

                                    lbGroupTypes.Items.Add( item );
                                }
                            } 
                        }
                        else
                        {
                            GoBack();
                        }
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
                if ( lbGroupTypes.SelectedItem != null )
                {
                    var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                    if ( family != null )
                    {
                        var person = family.People.Where( p => p.Selected ).FirstOrDefault();
                        if ( person != null )
                        {
                            int id = Int32.Parse( lbGroupTypes.SelectedItem.Value );
                            var groupType = person.GroupTypes.Where( g => g.GroupType.Id == id ).FirstOrDefault();
                            if ( groupType != null )
                            {
                                groupType.Selected = true;
                                ProcessSelection();
                            }
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
                foreach( var person in family.People)
                {
                    person.Selected = false;
                }
            }

            SaveState();

            GoToPersonSelectPage( true );
        }

        private void ProcessSelection()
        {
            var errors = new List<string>();
            if ( ProcessActivity( "Location Search", out errors ) )
            {
                SaveState();
                GoToLocationSelectPage();
            }
            else
            {
                string errorMsg = "<ul><li>" + errors.AsDelimited( "</li><li>" ) + "</li></ul>";
                maWarning.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
            }
        }


    }
}