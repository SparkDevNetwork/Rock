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
    [Description( "Check-In Group Select block" )]
    public partial class GroupSelect : CheckInBlock
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
                            var groupType = person.GroupTypes.Where( g => g.Selected ).FirstOrDefault();
                            if ( groupType != null )
                            {
                                var location = groupType.Locations.Where( l => l.Selected ).FirstOrDefault();
                                if ( location != null )
                                {
                                    lLocationName.Text = location.ToString();

                                    //if ( location.Groups.Count == 1 )
                                    //{
                                    //    if ( UserBackedUp )
                                    //    {
                                    //        GoBack();
                                    //    }
                                    //    else
                                    //    {
                                    //        foreach ( var group in location.Groups )
                                    //        {
                                    //            group.Selected = true;
                                    //        }

                                    //        ProcessSelection();
                                    //    }
                                    //}
                                    //else
                                    //{
                                        foreach ( var group in location.Groups )
                                        {
                                            lbGroups.Items.Add( new ListItem( group.ToString(), group.Group.Id.ToString() ) );
                                        }
                                    //}
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
                if ( lbGroups.SelectedItem != null )
                {
                    var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                    if ( family != null )
                    {
                        var person = family.People.Where( p => p.Selected ).FirstOrDefault();
                        if ( person != null )
                        {
                            var groupType = person.GroupTypes.Where( g => g.Selected ).FirstOrDefault();
                            if ( groupType != null )
                            {
                                var location = groupType.Locations.Where( l => l.Selected ).FirstOrDefault();
                                if ( location != null )
                                {
                                    int id = Int32.Parse( lbGroups.SelectedItem.Value );
                                    var group = location.Groups.Where( g => g.Group.Id == id ).FirstOrDefault();
                                    if ( group != null )
                                    {
                                        group.Selected = true;
                                        ProcessSelection();
                                    }
                                }
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
                    foreach ( var groupType in person.GroupTypes )
                    {
                        foreach ( var location in groupType.Locations )
                        {
                            location.Selected = false;
                            location.Groups = new List<CheckInGroup>();
                        }
                    }
                }
            }

            SaveState();

            GoToLocationSelectPage( true );
        }

        private void ProcessSelection()
        {
            var errors = new List<string>();
            if ( ProcessActivity( "Schedule Search", out errors ) )
            {
                SaveState();
                GoToTimeSelectPage();
            }
            else
            {
                string errorMsg = "<ul><li>" + errors.AsDelimited( "</li><li>" ) + "</li></ul>";
                maWarning.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
            }
        }


    }
}