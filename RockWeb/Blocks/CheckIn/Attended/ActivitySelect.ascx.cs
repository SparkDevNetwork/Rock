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
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Rock;
using Rock.CheckIn;
using Rock.Model;

namespace RockWeb.Blocks.CheckIn.Attended
{
    [Description( "Attended Check-In Activity Select Block" )]
    public partial class ActivitySelect : CheckInBlock
    {
        #region Control Methods

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
                            lblPersonName.Text = person.Person.FullName;

                            // fill the ministry repeater
                            List<GroupType> groupTypeList = new List<GroupType>();
                            foreach ( var groupType in CurrentGroupTypeIds )
                            {
                                GroupType groupTypeSelected = new GroupTypeService().Get( groupType );
                                groupTypeList.Add( groupTypeSelected );
                            }
                            
                            rMinistry.DataSource = groupTypeList;
                            rMinistry.DataBind();

                            // the list shown under "ministry" are all things they've selected on the admin page, so go ahead and make the buttons "active"
                            foreach ( RepeaterItem item in rMinistry.Items )
                            {
                                ( (LinkButton)item.FindControl( "lbSelectMinistry" ) ).AddCssClass( "active" );
                            }

                            // fill the time repeater
                            //                        var location = groupType.Locations.Where( l => l.Selected ).FirstOrDefault();
                            //                        if ( location != null )
                            //                        {
                            //                            var group = location.Groups.Where( g => g.Selected ).FirstOrDefault();
                            //                            if ( group != null )
                            //                            {
                            //                                if ( group.Schedules.Count == 1 )
                            //                                {
                            //                                    foreach ( var schedule in group.Schedules )
                            //                                    {
                            //                                        schedule.Selected = true;
                            //                                    }
                            foreach ( var groupType in CurrentRoomGroupTypeIds )
                            {
                                CheckInGroupType cigt = new CheckInGroupType();
                                GroupType gt = new GroupType();
                                GroupTypeService gts = new GroupTypeService();
                                gt = gts.Get(groupType);
                                cigt.GroupType = gt;
                                CheckInLocation cil = new CheckInLocation();
                                cil = cigt.Locations.Where( l => l.Selected ).FirstOrDefault();
                                if ( cil != null )
                                {
                                    var group = cil.Groups.Where( g => g.Selected ).FirstOrDefault();
                                    if ( group != null )
                                    {

                                    }
                                }
                                //okay so here i want to finish drilling down to the schedules, then check to see if the id of the schedule is in the list of possible schedules and if not, add it.
                            }

                            //fill the activity repeater
                            List<GroupType> activityGroupTypeList = new List<GroupType>();
                            foreach ( var activityGroupType in CurrentRoomGroupTypeIds )
                            {
                                GroupType activityGroupTypeSelected = new GroupTypeService().Get( activityGroupType );
                                activityGroupTypeList.Add( activityGroupTypeSelected );
                            }

                            rActivity.DataSource = activityGroupTypeList;
                            rActivity.DataBind();

                            if ( person.GroupTypes.Count == 1 )
                            {
                                if ( UserBackedUp )
                                {
                                    GoBack();
                                }
                                else
                                {
                                    foreach ( var groupType in person.GroupTypes )
                                    {
                                        groupType.Selected = true;
                                    }

                                    ProcessMinistry();
                                }
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

        #endregion

        #region Edit Events

        protected void rMinistry_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                if ( family != null )
                {
                    var person = family.People.Where( p => p.Selected ).FirstOrDefault();
                    if ( person != null )
                    {
                        int id = Int32.Parse( e.CommandArgument.ToString() );
                        LoadGroups();
                    }
                }
            }
        }

        protected void rTime_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
        }

        protected void rActivity_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int id = Int32.Parse( e.CommandArgument.ToString() );
            if ( HasActiveClass( (LinkButton)e.Item.FindControl( "lbSelectActivity" ) ) )
            {
                // the button is already selected, so unselect it.
                ( (LinkButton)e.Item.FindControl( "lbSelectActivity" ) ).RemoveCssClass( "active" );
            }
            else
            {
                // the button isn't already selected. Select it.
                ( (LinkButton)e.Item.FindControl( "lbSelectActivity" ) ).AddCssClass( "active" );
            }

        }

        protected void lbBack_Click( object sender, EventArgs e )
        {
            GoBack();
        }
                
        protected void lbNext_Click( object sender, EventArgs e )
        {
            GoNext();   
        }

        #endregion

        #region Internal Methods 

        protected bool HasActiveClass( WebControl webcontrol )
        {
            string match = @"\s*\b" + "active" + @"\b";
            string css = webcontrol.CssClass;
            if ( System.Text.RegularExpressions.Regex.IsMatch( css, match, System.Text.RegularExpressions.RegexOptions.IgnoreCase ) )
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void GoBack()
        {
            CurrentCheckInState.CheckIn.SearchType = null;
            CurrentCheckInState.CheckIn.SearchValue = string.Empty;
            CurrentCheckInState.CheckIn.Families = new List<CheckInFamily>();

            SaveState();

            NavigateToPreviousPage();
        }

        private void GoNext()
        {
            SaveState();
            //GoToSuccessPage();
            NavigateToNextPage();
        }

        private void ProcessMinistry()
        {
            var errors = new List<string>();
            //if ( ProcessActivity( "Location Search", out errors ) )
            if ( ProcessActivity( "Schedule Search", out errors ) )
            {
                SaveState();
                LoadTimes();
                //NavigateToNextPage();
            }
            else
            {
                string errorMsg = "<ul><li>" + errors.AsDelimited( "</li><li>" ) + "</li></ul>";
                maWarning.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
            }
        }

        private void LoadGroups()
        {
            var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
            if ( family != null )
            {
                var person = family.People.Where( p => p.Selected ).FirstOrDefault();
                if ( person != null )
                {
                    //foreach ( var kioskGroupType in CurrentCheckInState.Kiosk.KioskGroupTypes.Where( g => g.KioskLocations.Any( l => l.Location.IsActive ) ) )
                    //{
                    //    if ( !person.GroupTypes.Any( g => g.GroupType.Id == kioskGroupType.GroupType.Id ) )
                    //    {
                    //        var checkinGroupType = new CheckInGroupType();
                    //        checkinGroupType.GroupType = kioskGroupType.GroupType.Clone( false );
                    //        checkinGroupType.GroupType.CopyAttributesFrom( kioskGroupType.GroupType );
                    //        person.GroupTypes.Add( checkinGroupType );
                    //    }
                    //}
                    var kioskGroupTypes = CurrentCheckInState.Kiosk.KioskGroupTypes.Where( g => g.KioskLocations.Any( l => l.Location.IsActive ) );
                    
                    rActivity.DataSource = kioskGroupTypes;
                    rActivity.DataBind();
                }
            }
        }

        private void LoadTimes()
        {
//            var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
//            if ( family != null )
//            {
//                var person = family.People.Where( p => p.Selected ).FirstOrDefault();
//                if ( person != null )
//                {
//                    var groupType = person.GroupTypes.Where( g => g.Selected ).FirstOrDefault();
//                    if ( groupType != null )
//                    {
//                        var location = groupType.Locations.Where( l => l.Selected ).FirstOrDefault();
//                        if ( location != null )
//                        {
//                            var group = location.Groups.Where( g => g.Selected ).FirstOrDefault();
//                            if ( group != null )
//                            {
//                                if ( group.Schedules.Count == 1 )
//                                {
//                                    foreach ( var schedule in group.Schedules )
//                                    {
//                                        schedule.Selected = true;
//                                    }

//                                    //ProcessSelection();
//                                }
//                                string script = string.Format( @"
//                                    <script>
//                                        function GetTimeSelection() {{
//                                            var ids = '';
//                                            $('div.checkin-timelist button.active').each( function() {{
//                                                ids += $(this).attr('schedule-id') + ',';
//                                            }});
//                                            if (ids == '') {{
//                                                alert('Please select at least one time');
//                                                return false;
//                                            }}
//                                            else
//                                            {{
//                                                $('#{0}').val(ids);
//                                                return true;    
//                                            }}
//                                        }}
//                                    </script>
//                                ", hfTimes.ClientID );
//                                Page.ClientScript.RegisterClientScriptBlock( this.GetType(), "SelectTime", script );

//                                rTime.DataSource = group.Schedules.OrderBy( s => s.Schedule.StartTime );
//                                rTime.DataBind();
//                            }
//                        }
//                    }
//                }
//            }
        }

        #endregion
    }
}