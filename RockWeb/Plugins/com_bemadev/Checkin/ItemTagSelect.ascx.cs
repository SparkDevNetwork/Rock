// <copyright>
// Copyright by LCBC Church
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
    [TextField( "Caption", "", false, "How many item tags would you like?", "Text", 10 )]

    [TextField( "No Option Message", "Message to display when there are not any options available. Use {0} for person's name, and {1} for schedule name.", false,
        "Sorry, there are currently not any available locations that {0} can check into at {1}.", "Text", 11 )]
    [TextField( "No Option After Select Message", "Message to display when there are not any options available after location is selected. Use {0} for person's name", false,
        "Sorry, based on your selection, there are currently not any available times that {0} can check into.", "Text", 12 )]

    [LinkedPage( "Auto Select Previous Page", "The page to navigate back to if none of the people and schedules have been processed.", false, "", "", 13, "FamilyAutoSelectPreviousPage" )]
    [LinkedPage( "Auto Select Last Page", "The last page for each person during family check-in.", false, "", "", 14, "FamilyAutoSelectLastPage" )]

    public partial class ItemTagSelect : CheckInBlock
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
                        var itemTagKey = String.Format( "ItemTag_ScheduleId_{0}", schedule.Schedule.Id );
                        var itemTagExists = person.StateParameters.Any( sp => sp.Key.Contains( "ItemTag" ) && sp.Value.IsNotNullOrWhiteSpace() && sp.Key != itemTagKey );
                        if ( itemTagExists )
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
                            var itemTagKey = String.Format( "ItemTag_ScheduleId_{0}", schedule.Schedule.Id );
                            var itemTagExists = person.StateParameters.Any( sp => sp.Key.Contains( "ItemTag" ) && sp.Value.IsNotNullOrWhiteSpace() && sp.Key != itemTagKey );
                            if ( itemTagExists )
                            {
                                GoBack();
                            }
                            else
                            {
                                rSelection.DataSource = new List<String> { "0", "1", "2", "3", "4", "5" };
                                rSelection.DataBind();
                            }
                        }
                        else
                        {
                            var itemTagKey = String.Format( "ItemTag_ScheduleId_{0}", schedule.Schedule.Id );
                            var itemTagExists = person.StateParameters.Any( sp => sp.Key.Contains( "ItemTag" ) && sp.Value.IsNotNullOrWhiteSpace() && sp.Key != itemTagKey );
                            if ( itemTagExists )
                            {
                                NavigateToNextPage( false );
                            }
                            else
                            {
                                rSelection.DataSource = new List<String> { "0", "1", "2", "3", "4", "5" };
                                rSelection.DataBind();
                            }
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
        /// Handles the ItemCommand event of the rSelection control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rSelection_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                var person = CurrentCheckInState.CheckIn.CurrentPerson;
                if ( person != null )
                {
                    var schedule = person.CurrentSchedule;
                    int tagNumber = Int32.Parse( e.CommandArgument.ToString() );
                    var itemTagKey = String.Format( "ItemTag_ScheduleId_{0}", schedule.Schedule.Id );
                    person.StateParameters.AddOrReplace( itemTagKey, tagNumber.ToString() );
                    ProcessSelection( person, schedule );
                }
            }
        }

        private void ClearSelection()
        {
            if ( CurrentCheckInState != null )
            {
                var person = CurrentCheckInState.CheckIn.CurrentPerson;
                if ( person != null )
                {
                    var schedule = person.CurrentSchedule;
                    var itemTagKey = String.Format( "ItemTag_ScheduleId_{0}", schedule.Schedule.Id );
                    person.StateParameters.Remove( itemTagKey );
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

        /// <summary>
        /// Navigates to previous page.
        /// </summary>
        /// <param name="queryParams">The query parameters.</param>
        /// <param name="validateSelectionRequired">if set to <c>true</c> will check that block on previous page has a selection required before redirecting.</param>
        protected override void NavigateToPreviousPage( Dictionary<string, string> queryParams, bool validateSelectionRequired )
        {
            if ( CurrentCheckInState.CheckInType.TypeOfCheckin == TypeOfCheckin.Family && CurrentCheckInState.CheckInType.AutoSelectOptions.HasValue && CurrentCheckInState.CheckInType.AutoSelectOptions == 1 )
            {
                bool anythingProcessed = false;

                queryParams = CheckForOverride( queryParams );

                // First check for first unprocessed person
                var currentPerson = CurrentCheckInState.CheckIn.CurrentPerson;
                if ( currentPerson != null )
                {
                    var itemTagKey = String.Format( "ItemTag_ScheduleId_{0}", currentPerson.CurrentSchedule.Schedule.Id );
                    currentPerson.StateParameters.Remove( itemTagKey );

                    var lastSchedule = currentPerson.PossibleSchedules.Where( p => p.Processed ).LastOrDefault();
                    if ( lastSchedule != null )
                    {
                        // Current person has a processed schedule, unmark that one and continue.
                        lastSchedule.Processed = false;
                        anythingProcessed = true;
                    }
                    else
                    {
                        // current person did not have any processed schedules, so find last processed person, and 
                        // mark them and their last schedule as not processed.
                        var family = CurrentCheckInState.CheckIn.CurrentFamily;
                        if ( family != null )
                        {
                            var lastPerson = family.People.Where( p => p.Processed ).LastOrDefault();
                            if ( lastPerson != null )
                            {
                                lastPerson.Processed = false;
                                lastSchedule = lastPerson.PossibleSchedules.Where( p => p.Processed ).LastOrDefault();
                                if ( lastSchedule != null )
                                {
                                    lastSchedule.Processed = false;
                                }

                                anythingProcessed = true;
                            }
                        }
                    }

                    SaveState();
                }

                if ( anythingProcessed )
                {
                    if ( validateSelectionRequired )
                    {
                        var nextBlock = GetCheckInBlock( "FamilyAutoSelectLastPage" );
                        if ( nextBlock != null && nextBlock.RequiresSelection( true ) )
                        {
                            NavigateToLinkedPage( "FamilyAutoSelectLastPage", queryParams );
                        }
                    }
                    else
                    {
                        NavigateToLinkedPage( "FamilyAutoSelectLastPage", queryParams );
                    }
                }
                else
                {
                    // If the current person did not have any processed schedules, then this would be the first person
                    // and we should navigate to previous page (person selection)
                    NavigateToLinkedPage( "FamilyAutoSelectPreviousPage", queryParams );
                }
            }
            else
            {
                base.NavigateToPreviousPage( queryParams, validateSelectionRequired );
            }
        }
    }
}