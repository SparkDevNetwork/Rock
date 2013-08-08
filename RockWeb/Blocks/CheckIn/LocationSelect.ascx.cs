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
    [Description( "Check-in Location Select block" )]
    public partial class LocationSelect : CheckInBlock
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
                    var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                    if ( family != null )
                    {
                        var person = family.People.Where( p => p.Selected ).FirstOrDefault();
                        if ( person != null )
                        {
                            var groupType = person.GroupTypes.Where( g => g.Selected ).FirstOrDefault();
                            if ( groupType != null )
                            {
                                var group = groupType.Groups.Where( g => g.Selected ).FirstOrDefault();
                                if ( group != null )
                                {
                                    lGroupName.Text = group.ToString();

                                    if ( group.Locations.Count == 1 )
                                    {
                                        if ( UserBackedUp )
                                        {
                                            GoBack();
                                        }
                                        else
                                        {
                                            // only one location so why foreach?
                                            foreach ( var location in group.Locations )
                                            {
                                                location.Selected = true;
                                            }

                                            ProcessSelection();
                                        }
                                    }
                                    else
                                    {
                                        rSelection.DataSource = group.Locations;
                                        rSelection.DataBind();
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
                    else
                    {
                        GoBack();
                    }
                }
            }
        }

        protected void rSelection_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( KioskCurrentlyActive )
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
                            var group = groupType.Groups.Where( g => g.Selected ).FirstOrDefault();
                            if ( group != null )
                            {
                                int id = Int32.Parse( e.CommandArgument.ToString() );
                                var location = group.Locations.Where( l => l.Location.Id == id ).FirstOrDefault();
                                if ( location != null )
                                {
                                    location.Selected = true;
                                    ProcessSelection();
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
                        foreach ( var group in groupType.Groups )
                        {
                            group.Selected = false;
                            group.Locations = new List<CheckInLocation>();
                        }
                    }
                }
            }

            SaveState();

            NavigateToPreviousPage();
        }

        private void ProcessSelection()
        {
            var errors = new List<string>();
            if ( ProcessActivity( "Schedule Search", out errors ) )
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


    }
}