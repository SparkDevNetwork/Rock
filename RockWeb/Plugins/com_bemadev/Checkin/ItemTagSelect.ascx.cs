// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;

namespace RockWeb.Plugins.com_bemadev.CheckIn
{
    [DisplayName( "Item Tag Select" )]
    [Category( "com_bemadev > Check-in" )]
    [Description( "Displays a number box to enter how many item tags you would like printed." )]

    [TextField( "Title", "Title to display. Use {0} for person/schedule.", false, "{0}", "Text", 8 )]
    [TextField( "Sub Title", "Sub-Title to display. Use {0} for selected group name.", false, "{0}", "Text", 9 )]
    [TextField( "Caption", "", false, "Select Location", "Text", 10 )]

    [TextField( "No Option Message", "Message to display when there are not any options available. Use {0} for person's name, and {1} for schedule name.", false,
        "Sorry, there are currently not any available locations that {0} can check into at {1}.", "Text", 11 )]
    [TextField( "No Option After Select Message", "Message to display when there are not any options available after location is selected. Use {0} for person's name", false,
        "Sorry, based on your selection, there are currently not any available times that {0} can check into.", "Text", 12 )]

    public partial class ItemTagSelect : CheckInBlockMultiPerson
    {
        /// <summary>
        /// Determines if the block requires that a selection be made. This is used to determine if user should
        /// be redirected to this block or not.
        /// </summary>
        /// <param name="backingUp">if set to <c>true</c> [backing up].</param>
        /// <returns></returns>
        public override bool RequiresSelection( bool backingUp )
        {
            if ( CurrentWorkflow == null || CurrentCheckInState == null )
            {
                NavigateToHomePage();
                return false;
            }
            else
            {
                ClearSelection();

                var person = CurrentCheckInState.CheckIn.CurrentPerson;
                if ( person == null )
                {
                    GoBack( true );
                    return false;
                }

                var schedule = person.CurrentSchedule;

                var groupTypes = person.SelectedGroupTypes( schedule );
                if ( groupTypes == null || !groupTypes.Any() )
                {
                    GoBack( true );
                    return false;
                }

                var group = groupTypes.SelectMany( t => t.SelectedGroups( schedule ) ).FirstOrDefault();
                if ( group == null )
                {
                    GoBack( true );
                    return false;
                }

                var areItemTagsAllowed = CurrentCheckInState.Kiosk.Device.GetAttributeValue( "AllowItemTags" ).AsBoolean();
                var areItemTagsOffered = group.Group.GetAttributeValue( "AreItemTagsOffered" ).AsBoolean();
                if ( areItemTagsAllowed && areItemTagsOffered )
                {
                    if ( backingUp )
                    {
                        GoBack( true );
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    if ( backingUp )
                    {
                        GoBack( true );
                        return false;
                    }
                    else
                    {
                        return !ProcessSelection( person, schedule );
                    }
                }
            }

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );

            var bodyTag = this.Page.Master.FindControl( "bodyTag" ) as HtmlGenericControl;
            if ( bodyTag != null )
            {
                bodyTag.AddCssClass( "checkin-locationselect-bg" );
            }

            if ( CurrentWorkflow == null || CurrentCheckInState == null )
            {
                NavigateToHomePage();
            }
            else
            {
                if ( !Page.IsPostBack )
                {
                    ClearSelection();

                    var person = CurrentCheckInState.CheckIn.CurrentPerson;
                    if ( person == null )
                    {
                        GoBack();
                    }

                    var schedule = person.CurrentSchedule;

                    var groupTypes = person.SelectedGroupTypes( schedule );
                    if ( groupTypes == null || !groupTypes.Any() )
                    {
                        GoBack();
                    }

                    var group = groupTypes.SelectMany( t => t.SelectedGroups( schedule ) ).FirstOrDefault();
                    if ( group == null )
                    {
                        GoBack();
                    }

                    lTitle.Text = string.Format( GetAttributeValue( "Title" ), GetPersonScheduleSubTitle() );
                    lSubTitle.Text = string.Format( GetAttributeValue( "SubTitle" ), group.ToString() );
                    lCaption.Text = GetAttributeValue( "Caption" );

                    var areItemTagsAllowed = CurrentCheckInState.Kiosk.Device.GetAttributeValue( "AllowItemTags" ).AsBoolean();
                    var areItemTagsOffered = group.Group.GetAttributeValue( "AreItemTagsOffered" ).AsBoolean();
                    if ( areItemTagsAllowed && areItemTagsOffered )
                    {
                        if ( UserBackedUp )
                        {
                            GoBack();
                        }
                    }
                    else
                    {
                        if ( UserBackedUp )
                        {
                            GoBack();
                        }
                        else
                        {
                            NavigateToNextPage( false );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clears any previously selected locations.
        /// </summary>
        private void ClearSelection()
        {
            var person = CurrentCheckInState.CheckIn.CurrentPerson;
            if ( person != null )
            {
                var schedule = person.CurrentSchedule;
                foreach ( var groupType in person.SelectedGroupTypes( schedule ) )
                {
                    foreach ( var group in groupType.SelectedGroups( schedule ) )
                    {
                        foreach ( var location in group.SelectedLocations( schedule ) )
                        {
                            location.Selected = false;
                            location.SelectedForSchedule = schedule != null ?
                                location.SelectedForSchedule.Where( s => s != schedule.Schedule.Id ).ToList() :
                                new List<int>();
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
        protected void lbSelect_Click( object sender, EventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                var person = CurrentCheckInState.CheckIn.CurrentPerson;
                if ( person != null )
                if ( person != null )
                {
                    var schedule = person.CurrentSchedule;

                    var groupTypes = schedule == null ?
                        person.GroupTypes.Where( t => t.Selected ).ToList() :
                        person.GroupTypes.Where( t => t.SelectedForSchedule.Contains( schedule.Schedule.Id ) ).ToList();

                    if ( groupTypes != null && groupTypes.Any() )
                    {
                        var group = schedule == null ?
                            groupTypes.SelectMany( t => t.Groups.Where( g => g.Selected ) ).FirstOrDefault() :
                            groupTypes.SelectMany( t => t.Groups.Where( g => g.SelectedForSchedule.Contains( schedule.Schedule.Id ) ) ).FirstOrDefault();

                        if ( group != null )
                        {
                            if ( nbItemTags.Value > 0 )
                            {
                                person.StateParameters.AddOrReplace( "ItemTags", nbItemTags.Value.ToString() );
                            }

                            ProcessSelection( person, schedule );
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnNoOptionOk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnNoOptionOk_Click( object sender, EventArgs e )
        {
            ProcessNoOption();
        }

        /// <summary>
        /// Handles the Click event of the lbBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbBack_Click( object sender, EventArgs e )
        {
            GoBack( true );
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
            if ( CurrentCheckInType != null && CurrentCheckInType.DisplayLocationCount )
            {
                return string.Format( " <span class='checkin-sub-title'> Count: {0}</span>", KioskLocationAttendance.Get( locationId ).CurrentCount );
            }

            return string.Empty;
        }

        /// <summary>
        /// Processes the selection.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="schedule">The schedule.</param>
        /// <returns></returns>
        protected bool ProcessSelection( CheckInPerson person, CheckInSchedule schedule )
        {
            if ( person != null )
            {
                string msg = string.Format( GetAttributeValue( "NoOptionAfterSelectMessage" ), person.Person.FullName );
                if ( !ProcessSelection(
                    maWarning,
                    () => false,
                    string.Format( "<p>{0}</p>", msg ),
                    CurrentCheckInState.CheckInType.TypeOfCheckin == TypeOfCheckin.Family ) )
                {
                    ClearSelection();
                }
                else
                {
                    return true;
                }
            }

            return false;
        }
    }
}