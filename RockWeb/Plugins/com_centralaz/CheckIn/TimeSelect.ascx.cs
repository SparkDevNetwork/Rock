// <copyright>
// Copyright by Central Christian Church
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

namespace RockWeb.Plugins.com_centralaz.CheckIn
{
    [DisplayName("Time Select")]
    [Category( "com_centralaz > Check-in" )]
    [Description("Displays a list of times to checkin for.")]
    public partial class TimeSelect : CheckInBlock
    {
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            RockPage.AddScriptLink( "~/Scripts/iscroll.js" );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );
            RockPage.AddScriptLink( "~/Plugins/com_centralaz/CheckIn/Scripts/checkin-core.js" );

            if ( CurrentWorkflow == null || CurrentCheckInState == null )
            {
                NavigateToHomePage();
            }
            else
            {
                if ( !Page.IsPostBack )
                {
                    ClearSelection();
                    SetSelectedPeopleInHiddenList();
                    SetupSelectionScreenForNextPerson();
                }
            }
        }

        /// <summary>
        /// Set each selected person into the list of people to be processed.
        /// This is a queue that will drain as we process each person.
        /// </summary>
        private void SetSelectedPeopleInHiddenList()
        {
            var ids = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected )
                        .SelectMany( f => f.People.Where( p => p.Selected ) )
                        .Select( p => p.Person.Id.ToString() ).ToArray();

            hfPeopleToProcess.Value = string.Join( ",", ids );
        }

        /// <summary>
        /// Builds the selection screen for the next person who needs it
        /// and returns how many people remain to be processed.
        /// When no more remain it will call ProcessSelection()
        /// </summary>
        private void SetupSelectionScreenForNextPerson()
        {
            // if there are people to process, then process them
            if ( !string.IsNullOrEmpty( hfPeopleToProcess.Value ) )
            {
                Queue<string> ids = new Queue<string>( hfPeopleToProcess.Value.SplitDelimitedValues() );

                // Process each person in the stack until there are no more.
                while ( ids.Count > 0 )
                {
                    int personId = ids.Dequeue().AsInteger();
                    hfPeopleToProcess.Value = string.Join( ",", ids );
                    hfPerson.Value = personId.ToString();
                    CheckInPerson person = null;
                    CheckInGroup group = null;
                    CheckInLocation location = null;

                    person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected )
                        .SelectMany( f => f.People.Where( p => p.Selected && p.Person.Id == personId ) )
                        .FirstOrDefault();

                    if ( person != null )
                    {
                        group = person.GroupTypes.Where( t => t.Selected )
                                .SelectMany( t => t.Groups.Where( g => g.Selected ) )
                                .FirstOrDefault();

                        if ( group != null )
                        {
                            location = group.Locations.Where( l => l.Selected )
                                        .FirstOrDefault();
                        }
                    }

                    if ( location == null )
                    {
                        GoBack();
                    }

                    lTitle.Text = person.ToString();
                    lSubTitle.Text = string.Format( "{0} - {1}", group.ToString(), location.ToString() );

                    var availSchedules = location.Schedules.Where( s => !s.ExcludedByFilter ).ToList();
                    if ( availSchedules.Count == 1 )
                    {
                        availSchedules.FirstOrDefault().Selected = true;
                    }
                    else
                    {
                        string script = string.Format( @"
    <script>
        function GetTimeSelection() {{
            var ids = '';
            $('div.checkin-timelist button.active').each( function() {{
                ids += $(this).attr('schedule-id') + ',';
            }});
            if (ids == '') {{
                bootbox.alert('Please select at least one time');
                return false;
            }}
            else
            {{
                $('#{0}').button('loading')
                $('#{1}').val(ids);
                return true;
            }}
        }}
    </script>
", lbSelect.ClientID, hfTimes.ClientID );
                        Page.ClientScript.RegisterClientScriptBlock( this.GetType(), "SelectTime", script );

                        rSelection.DataSource = availSchedules.OrderBy( s => s.StartTime );
                        rSelection.DataBind();
                        return;
                    }
                }
            }

            // No more people, then continue to next step
            ProcessSelection( maWarning );
        }

        /// <summary>
        /// Clears any previously selected schedules.
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
                                foreach ( var schedule in location.Schedules )
                                {
                                    schedule.Selected = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        protected void lbSelect_Click( object sender, EventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                var personId = hfPerson.ValueAsInt();

                var location = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected )
                    .SelectMany( f => f.People.Where( p => p.Selected && p.Person.Id == personId )
                        .SelectMany( p => p.GroupTypes.Where( t => t.Selected )
                            .SelectMany( t => t.Groups.Where( g => g.Selected ) 
                                .SelectMany( g => g.Locations.Where( l => l.Selected ) ) ) ) )
                    .FirstOrDefault();

                if ( location != null )
                {
                    foreach( var scheduleId in hfTimes.Value.SplitDelimitedValues())
                    {
                        int id = Int32.Parse( scheduleId );
                        var schedule = location.Schedules.Where( s => s.Schedule.Id == id ).FirstOrDefault();
                        if (schedule != null)
                        {
                            schedule.Selected = true;
                        }
                    }

                    SetupSelectionScreenForNextPerson();
                    //ProcessSelection( maWarning );
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
    }
}