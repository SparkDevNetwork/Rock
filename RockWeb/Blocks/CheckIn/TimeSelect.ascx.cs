﻿// <copyright>
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

namespace RockWeb.Blocks.CheckIn
{
    [DisplayName("Time Select")]
    [Category("Check-in")]
    [Description("Displays a list of times to checkin for.")]

    [TextField( "Title", "Title to display. Use {0} for family/person name.", false, "{0}", "Text", 5 )]
    [TextField( "Sub Title", "Sub-Title to display. Use {0} for selected group/location name.", false, "{0}", "Text", 6 )]
    [TextField( "Caption", "", false, "Select Time(s)", "Text", 7 )]

    public partial class TimeSelect : CheckInBlock
    {
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );

            var bodyTag = this.Page.Master.FindControl( "bodyTag" ) as HtmlGenericControl;
            if ( bodyTag != null )
            {
                bodyTag.AddCssClass( "checkin-timeselect-bg" );
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

                    var personSchedules = new List<CheckInSchedule>();
                    var distinctSchedules = new List<CheckInSchedule>();
                    if ( CurrentCheckInType != null && CurrentCheckInType.TypeOfCheckin == TypeOfCheckin.Family )
                    {
                        CheckInFamily family = CurrentCheckInState.CheckIn.CurrentFamily;
                        if ( family != null )
                        {
                            foreach( var schedule in family.GetPeople( true ).SelectMany( p => p.PossibleSchedules ).ToList() )
                            {
                                personSchedules.Add( schedule );
                                if ( !distinctSchedules.Any( s => s.Schedule.Id == schedule.Schedule.Id ) )
                                {
                                    distinctSchedules.Add( schedule );
                                }

                            }

                        }
                        else
                        {
                            GoBack();
                        }

                        lTitle.Text = string.Format( GetAttributeValue( "Title" ), family.ToString() );

                        lbSelect.Text = "Next";
                        lbSelect.Attributes.Add( "data-loading-text", "Loading..." );
                    }
                    else
                    { 
                        CheckInPerson person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected )
                            .SelectMany( f => f.People.Where( p => p.Selected ) )
                            .FirstOrDefault();

                        CheckInGroup group = null;
                        CheckInLocation location = null;

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

                        lTitle.Text = string.Format( GetAttributeValue( "Title" ), person.ToString() );
                        lSubTitle.Text = string.Format( GetAttributeValue( "SubTitle"), string.Format( "{0} - {1}", group.ToString(), location.Location.Name ) );

                        lbSelect.Text = "Check In";
                        lbSelect.Attributes.Add( "data-loading-text", "Printing..." );

                        personSchedules = location.Schedules.Where( s => !s.ExcludedByFilter ).ToList();
                        distinctSchedules = personSchedules;
                    }

                    lCaption.Text = GetAttributeValue( "Caption" );

                    if ( distinctSchedules.Count == 1 )
                    {
                        personSchedules.ForEach( s => s.Selected = true );
                        ProcessSelection( maWarning );
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

                        rSelection.DataSource = distinctSchedules
                            .OrderBy( s => s.StartTime.Value.TimeOfDay )
                            .ThenBy( s => s.Schedule.Name )
                            .ToList();

                        rSelection.DataBind();
                    }
                }
            }
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
                    foreach ( var schedule in person.PossibleSchedules )
                    {
                        schedule.Selected = false;
                        schedule.Processed = false;
                    }

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
                var schedules = new List<CheckInSchedule>();
                bool validateSelection = false; 

                var selectedIDs = hfTimes.Value.SplitDelimitedValues().AsIntegerList();
                if ( CurrentCheckInType != null && CurrentCheckInType.TypeOfCheckin == TypeOfCheckin.Family )
                {
                    schedules = CurrentCheckInState.CheckIn.GetFamilies( true )
                        .SelectMany( f => f.GetPeople( true )
                            .SelectMany( p => p.PossibleSchedules.Where( s => selectedIDs.Contains( s.Schedule.Id ) ) ) )
                        .ToList();
                    validateSelection = true;
                }
                else
                {
                    schedules = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected )
                        .SelectMany( f => f.People.Where( p => p.Selected )
                            .SelectMany( p => p.GroupTypes.Where( t => t.Selected )
                                .SelectMany( t => t.Groups.Where( g => g.Selected )
                                    .SelectMany( g => g.Locations.Where( l => l.Selected )
                                        .SelectMany( l => l.Schedules.Where( s => selectedIDs.Contains( s.Schedule.Id ) ) ) ) ) ) )
                        .ToList();
                }

                if ( schedules != null && schedules.Any() )
                {
                    schedules.ForEach( s => s.Selected = true );
                    ProcessSelection( maWarning, validateSelection );
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