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
    [Description( "Check-in Time Select block" )]
    public partial class TimeSelect : CheckInBlock
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
                                    var location = group.Locations.Where( l => l.Selected ).FirstOrDefault();
                                    if ( location != null )
                                    {
                                        lTitle.Text = person.ToString();
                                        lSubTitle.Text = group.ToString();

                                        if ( location.Schedules.Count == 1 )
                                        {
                                            foreach ( var schedule in location.Schedules )
                                            {
                                                schedule.Selected = true;
                                            }

                                            ProcessSelection( maWarning );
                                        }
                                        else
                                        {
                                            string script = string.Format(@"
    <script>
        function GetTimeSelection() {{
            var ids = '';
            $('div.checkin-timelist button.active').each( function() {{
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

                                            rSelection.DataSource = location.Schedules.OrderBy( s => s.StartTime );
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
                            var group = groupType.Groups.Where( g => g.Selected ).FirstOrDefault();
                            if ( group != null )
                            {
                                var location = group.Locations.Where( l => l.Selected ).FirstOrDefault();
                                if ( location != null )
                                {
                                    foreach( var scheduleId in hfTimes.Value.SplitDelimitedValues())
                                    {
                                        int id = Int32.Parse( scheduleId );
                                        var schedule = location.Schedules.Where( s => s.Schedule.Id == id).FirstOrDefault();
                                        if (schedule != null)
                                        {
                                            schedule.Selected = true;
                                        }
                                    }

                                    ProcessSelection( maWarning );
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
    }
}