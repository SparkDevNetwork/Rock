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
    [Description( "Check-In Time Select block" )]
    public partial class TimeSelect : CheckInBlock
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
                                    var group = location.Groups.Where( g => g.Selected ).FirstOrDefault();
                                    if ( group != null )
                                    {
                                        lTitle.Text = person.ToString() + " <div class=\"checkin-sub-title\"> " + group.ToString() + "</div>";

                                        if ( group.Schedules.Count == 1 )
                                        {
                                            foreach ( var schedule in group.Schedules )
                                            {
                                                schedule.Selected = true;
                                            }

                                            ProcessSelection();
                                        }
                                        else
                                        {
                                            string script = string.Format( @"
    <script>
        function GetTimeSelection() {{
            var ids = '';
            $('div.time-select button.active').each( function() {{
                ids += $(this).attr('schedule-id') + ',';
            }});
            if (ids == '') {{
                alert('Please select at least one time');
                return false;
            }}
            else
            {{
                $('#{0}').val(ids);
                return true;
            }}
        }}
    </script>
", hfTimes.ClientID );
                                            Page.ClientScript.RegisterClientScriptBlock( this.GetType(), "SelectTime", script );

                                            rSelection.DataSource = group.Schedules.OrderBy( s => s.Schedule.StartTime );
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
                                var group = location.Groups.Where( g => g.Selected ).FirstOrDefault();
                                if ( group != null )
                                {
                                    foreach( var scheduleId in hfTimes.Value.SplitDelimitedValues())
                                    {
                                        int id = Int32.Parse( scheduleId );
                                        var schedule = group.Schedules.Where( s => s.Schedule.Id == id).FirstOrDefault();
                                        if (schedule != null)
                                        {
                                            schedule.Selected = true;
                                        }
                                    }

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
                        foreach ( var location in groupType.Locations )
                        {
                            foreach ( var group in location.Groups )
                            {
                                group.Selected = false;
                                group.Schedules = new List<CheckInSchedule>();
                            }
                        }
                    }
                }
            }

            SaveState();

            GoToGroupSelectPage( true );
        }

        private void ProcessSelection()
        {
            var errors = new List<string>();
            if ( ProcessActivity( "Save Attendance", out errors ) )
            {
                SaveState();
                GoToSuccessPage();
            }
            else
            {
                string errorMsg = "<ul><li>" + errors.AsDelimited( "</li><li>" ) + "</li></ul>";
                maWarning.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
            }
        }


    }
}