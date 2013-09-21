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

namespace RockWeb.Blocks.CheckIn
{
    [Description( "Check-in Location Select block" )]
    [TextField( "Current Count Format", "How should current count be displayed", false, " Count: {0}" )]
    public partial class LocationSelect : CheckInBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
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
                    ClearSelection();
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

                                            ProcessSelection( maWarning );
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

        /// <summary>
        /// Clears any previously selected locations.
        /// </summary>
        private void ClearSelection()
        {
            foreach ( var family in CurrentCheckInState.CheckIn.Families )
            {
                foreach ( var person in family.People )
                {
                    foreach ( var groupType in person.GroupTypes )
                    {
                        foreach ( var group in groupType.Groups )
                        {
                            foreach ( var location in group.Locations )
                            {
                                location.Selected = false;
                                //location.Schedules = new List<CheckInSchedule>();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rSelection control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
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
                                    ProcessSelection( maWarning );
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbBack_Click( object sender, EventArgs e )
        {
            GoBack();
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

        /// <summary>
        /// Formats the count.
        /// </summary>
        /// <param name="locationId">The location id.</param>
        /// <returns></returns>
        protected string FormatCount( int locationId )
        {
            string currentCountFormat = GetAttributeValue( "CurrentCountFormat" );
            if (!string.IsNullOrWhiteSpace(currentCountFormat) && currentCountFormat.Contains("{0}"))
            {
                return string.Format( " <span class='checkin-sub-title'>{0}</span>",
                    string.Format( currentCountFormat, KioskLocationAttendance.Read( locationId ).CurrentCount ) );
            }

            return string.Empty;
        }

    }
}