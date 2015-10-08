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
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;

namespace RockWeb.Blocks.CheckIn
{
    [DisplayName("Location Select")]
    [Category("Check-in")]
    [Description("Displays a list of locations a person is able to checkin to.")]
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

            RockPage.AddScriptLink( "~/Scripts/iscroll.js" );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );

            if ( CurrentWorkflow == null || CurrentCheckInState == null )
            {
                NavigateToHomePage();
            }
            else
            {
                if ( !Page.IsPostBack )
                {
                    ClearSelection();

                    CheckInPerson person = null;
                    CheckInGroup group = null;

                    person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected )
                        .SelectMany( f => f.People.Where( p => p.Selected ) )
                        .FirstOrDefault();

                    if ( person != null )
                    {
                        group = person.GroupTypes.Where( t => t.Selected )
                                .SelectMany( t => t.Groups.Where( g => g.Selected ) )
                                .FirstOrDefault();
                    }

                    if (group == null)
                    {
                        GoBack();
                    }

                    lTitle.Text = person.ToString();
                    lSubTitle.Text = group.ToString();

                    var availLocations = group.Locations.Where( l => !l.ExcludedByFilter ).ToList();
                    if ( availLocations.Count == 1 )
                    {
                        if ( UserBackedUp )
                        {
                            GoBack();
                        }
                        else
                        {
                            availLocations.FirstOrDefault().Selected = true;
                            ProcessSelection();
                        }
                    }
                    else
                    {
                        rSelection.DataSource = availLocations
                            .OrderBy( l => l.Location.Name )
                            .ToList();

                        rSelection.DataBind();
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
                var group = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected )
                    .SelectMany( f => f.People.Where( p => p.Selected ) 
                        .SelectMany( p => p.GroupTypes.Where( t => t.Selected ) 
                            .SelectMany( t => t.Groups.Where( g => g.Selected ) ) ) )
                    .FirstOrDefault();

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

        protected void ProcessSelection()
        {
            ProcessSelection( maWarning, () => CurrentCheckInState.CheckIn.Families.Where( f => f.Selected )
                .SelectMany( f => f.People.Where( p => p.Selected )
                    .SelectMany( p => p.GroupTypes.Where( t => t.Selected )
                        .SelectMany( t => t.Groups.Where( g => g.Selected )
                            .SelectMany( g => g.Locations.Where( l => l.Selected )
                                .SelectMany( l => l.Schedules.Where( s => !s.ExcludedByFilter ) ) ) ) ) )
                .Count() <= 0,
                "<p>Sorry, based on your selection, there are currently not any available times that can be checked into.</p>" );
        }

    }
}