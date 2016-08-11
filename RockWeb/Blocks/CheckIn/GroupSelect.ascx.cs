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
using System.Web.UI.WebControls;

using Rock;
using Rock.CheckIn;
using Rock.Model; 

namespace RockWeb.Blocks.CheckIn
{
    [DisplayName("Group Select")]
    [Category("Check-in")]
    [Description("Displays a list of groups that a person is configured to checkin to.")]
    public partial class GroupSelect : CheckInBlock
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

                var availGroups = groupTypes.SelectMany( t => t.GetAvailableGroups( schedule ) ).ToList();
                if ( availGroups.Count == 1 )
                {
                    if ( backingUp )
                    {
                        GoBack( true );
                        return false;
                    }
                    else
                    {
                        var group = availGroups.First();
                        if ( schedule == null )
                        {
                            group.Selected = true;
                        }
                        else
                        {
                            group.SelectedForSchedule.Add( schedule.Schedule.Id );
                        }

                        return !ProcessSelection( person, schedule );
                    }
                }
                else
                {
                    return true;
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

                    lTitle.Text = GetPersonScheduleSubTitle();

                    lSubTitle.Text = groupTypes
                        .Where( t => t.GroupType != null )
                        .Select( t => t.GroupType.Name )
                        .ToList().AsDelimited( ", " );

                    var availGroups = groupTypes.SelectMany( t => t.GetAvailableGroups( schedule ) ).ToList();
                    if ( availGroups.Count == 1 )
                    {
                        if ( UserBackedUp )
                        {
                            GoBack();
                        }
                        else
                        {
                            var group = availGroups.First();
                            if ( schedule == null )
                            {
                                group.Selected = true;
                            }
                            else
                            {
                                group.SelectedForSchedule.Add( schedule.Schedule.Id );
                            }

                            ProcessSelection( person, schedule );
                        }
                    }
                    else
                    {
                        rSelection.DataSource = availGroups
                            .OrderBy( g => g.Group.Order )
                            .ToList();

                        rSelection.DataBind();
                    }
                }
            }
        }

        /// <summary>
        /// Clear any previously selected groups.
        /// </summary>
        private void ClearSelection()
        {
            var person = CurrentCheckInState.CheckIn.CurrentPerson;
            if ( person != null )
            {
                var schedule = person.CurrentSchedule;
                foreach ( var groupType in person.SelectedGroupTypes( schedule ) )
                {
                    foreach( var group in groupType.SelectedGroups( schedule ) )
                    {
                        group.Selected = false;
                        group.SelectedForSchedule = schedule != null ?
                            group.SelectedForSchedule.Where( s => s != schedule.Schedule.Id ).ToList() :
                            new List<int>();
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

                    var groupTypes = person.SelectedGroupTypes( schedule );
                    if ( groupTypes != null && groupTypes.Any() )
                    {
                        int id = Int32.Parse( e.CommandArgument.ToString() );
                        var group = groupTypes
                            .SelectMany( t => t.Groups )
                            .Where( g => g.Group.Id == id )
                            .FirstOrDefault();

                        // deselect any group types that don't contain the group
                        foreach ( var groupType in groupTypes )
                        {
                            if ( schedule == null )
                            {
                                groupType.Selected = groupType.Groups.Contains( group );
                            }
                            else
                            {
                                if ( !groupType.SelectedForSchedule.Contains( schedule.Schedule.Id ) )
                                {
                                    groupType.SelectedForSchedule.Remove( schedule.Schedule.Id );
                                }
                            }
                        }

                        if ( group != null )
                        {
                            if ( schedule == null )
                            {
                                group.Selected = true;
                            }
                            else
                            {
                                group.SelectedForSchedule.Add( schedule.Schedule.Id );
                            }

                            ProcessSelection( person, schedule );
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
        /// Processes the selection.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="schedule">The schedule.</param>
        /// <returns></returns>
        protected bool ProcessSelection( CheckInPerson person, CheckInSchedule schedule )
        {
            if ( person != null )
            {
                if ( !ProcessSelection(
                    maWarning,
                    () => person.SelectedGroupTypes( schedule )
                        .SelectMany( t => t.SelectedGroups( schedule )
                            .SelectMany( g => g.Locations.Where( l => !l.ExcludedByFilter ) ) )
                        .Count() <= 0,
                    "<p>Sorry, based on your selection, there are currently not any available locations that can be checked into.</p>",
                    true ) )
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