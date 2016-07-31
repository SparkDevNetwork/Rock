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
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;

namespace RockWeb.Blocks.CheckIn
{
    [DisplayName( "Group Type Select" )]
    [Category( "Check-in" )]
    [Description( "Displays a list of group types the person is configured to checkin to." )]

    [BooleanField( "Select All and Skip", "Select this option if end-user should never see screen to select group types, all group types will automatically be selected and all the groups in all types will be available.", false, "", 5, "SelectAll" )]
    public partial class GroupTypeSelect : CheckInBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

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
                    lPersonName.Text = GetPersonScheduleSubTitle();

                    var availGroupTypes = person.GetAvailableGroupTypes( schedule );
                    if ( availGroupTypes.Count == 1 )
                    {
                        if ( UserBackedUp )
                        {
                            GoBack();
                        }
                        else
                        {
                            var groupType = availGroupTypes.First();
                            if ( schedule == null )
                            {
                                groupType.Selected = true;
                            }
                            else
                            {
                                groupType.SelectedForSchedule.Add( schedule.Schedule.Id );
                            }

                            ProcessSelection( person, schedule );
                        }
                    }
                    else
                    {
                        bool SelectAll = GetAttributeValue( "SelectAll" ).AsBoolean( false );
                        if ( SelectAll )
                        {
                            if ( UserBackedUp )
                            {
                                GoBack();
                            }
                            else
                            {
                                availGroupTypes.ForEach( t => t.Selected = true );
                                ProcessSelection( person, schedule );
                            }
                        }
                        else
                        {
                            rSelection.DataSource = availGroupTypes
                                .OrderBy( g => g.GroupType.Order )
                                .ToList();

                            rSelection.DataBind();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Clear any previously group types people.
        /// </summary>
        private void ClearSelection()
        {
            var person = CurrentCheckInState.CheckIn.CurrentPerson;
            if ( person != null )
            {
                var schedule = person.CurrentSchedule;
                foreach ( var groupType in person.SelectedGroupTypes( schedule ) )
                {
                    groupType.Selected = false;
                    groupType.SelectedForSchedule = schedule != null ?
                        groupType.SelectedForSchedule.Where( s => s != schedule.Schedule.Id ).ToList() :
                        new List<int>();
                }
            }
        }

        protected void rSelection_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                var person = CurrentCheckInState.CheckIn.CurrentPerson;
                if ( person != null )
                {
                    int id = Int32.Parse( e.CommandArgument.ToString() );
                    var groupType = person.GroupTypes.Where( g => g.GroupType.Id == id ).FirstOrDefault();
                    if ( groupType != null )
                    {
                        var schedule = person.CurrentSchedule;
                        if ( schedule == null )
                        {
                            groupType.Selected = true;
                        }
                        else
                        { 
                            groupType.SelectedForSchedule.Add( schedule.Schedule.Id );
                        }

                        ProcessSelection( person, schedule );
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

        protected void ProcessSelection( CheckInPerson person, CheckInSchedule schedule )
        {
            if ( person != null )
            {
                if ( !ProcessSelection(
                    maWarning,
                    () => person.SelectedGroupTypes( schedule )
                        .SelectMany( t => t.Groups.Where( g => !g.ExcludedByFilter ) )
                        .Count() <= 0,
                    "<p>Sorry, based on your selection, there are currently not any available locations that can be checked into.</p>" ) )
                {
                    ClearSelection();
                }
            }
        }
    }
}