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
using Rock.Web.UI.Controls;

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
                        var personId = CheckInPeopleIds.FirstOrDefault();
                        var person = new PersonService().Get( personId );
                        
                        if ( person != null )
                        {
                            lblPersonName.Text = person.FullName;
                            LoadStuff(person);
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

        //protected void LoadStuff(CheckInPerson person)
        protected void LoadStuff( Person person )
        {
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
            foreach ( var groupType in CurrentGroupTypeIds )
            {
                List<Schedule> scheduleList = new List<Schedule>();
                GroupType gt = new GroupType();
                GroupTypeService gts = new GroupTypeService();
                gt = gts.Get( groupType );
                foreach ( var group in gt.Groups )
                {
                    foreach ( var groupLocation in group.GroupLocations )
                    {
                        foreach ( var schedule in groupLocation.Schedules )
                        {
                            if ( !scheduleList.Contains( schedule ) )
                            {
                                scheduleList.Add( schedule );
                            }
                        }
                    }
                }

                rTime.DataSource = scheduleList;
                rTime.DataBind();
            }

            // fill the activity repeater
            List<GroupType> activityGroupTypeList = new List<GroupType>();
            foreach ( var activityGroupType in CurrentRoomGroupTypeIds )
            {
                GroupType activityGroupTypeSelected = new GroupTypeService().Get( activityGroupType );
                activityGroupTypeList.Add( activityGroupTypeSelected );
            }

            rActivity.DataSource = activityGroupTypeList;
            rActivity.DataBind();
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
                    }
                }
            }
        }

        protected void rTime_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int id = Int32.Parse( e.CommandArgument.ToString() );
            if ( HasActiveClass( (LinkButton)e.Item.FindControl( "lbSelectTime" ) ) )
            {
                // the button is already selected, so unselect it.
                ( (LinkButton)e.Item.FindControl( "lbSelectTime" ) ).RemoveCssClass( "active" );
            }
            else
            {
                // the button isn't already selected. Select it.
                ( (LinkButton)e.Item.FindControl( "lbSelectTime" ) ).AddCssClass( "active" );
            }
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
            // variables for holding specific choices so we can hold these for each person checking in. will need these on the confirmation page.
            int chosenTimeId = 0;
            int chosenActivityId = 0;

            // make sure a ministry was chosen.
            var ministryChosen = false;
            foreach ( RepeaterItem item in rMinistry.Items )
            {
                var linky = (LinkButton)item.FindControl( "lbSelectMinistry" );
                if ( HasActiveClass( linky ) )
                {
                    ministryChosen = true;
                }
            }

            if ( !ministryChosen )
            {
                maWarning.Show( "You have to choose a ministry.", ModalAlertType.Warning );
                return;
            }

            // make sure a time was chosen.
            var timeChosen = false;
            foreach ( RepeaterItem item in rTime.Items )
            {
                var linky = (LinkButton)item.FindControl( "lbSelectTime" );
                if ( HasActiveClass( linky ) )
                {
                    timeChosen = true;
                    chosenTimeId = int.Parse(linky.CommandArgument);
                }
            }

            if ( !timeChosen )
            {
                maWarning.Show( "You have to choose a time.", ModalAlertType.Warning );
                return;
            }

            // make sure an activity was chosen.
            var activityChosen = false;
            foreach ( RepeaterItem item in rActivity.Items )
            {
                var linky = (LinkButton)item.FindControl( "lbSelectActivity" );
                if ( HasActiveClass( linky ) )
                {
                    activityChosen = true;
                    chosenActivityId = int.Parse( linky.CommandArgument );
                }
            }

            if ( !activityChosen )
            {
                maWarning.Show( "You have to choose an activity.", ModalAlertType.Warning );
                return;
            }

            // Increment the counter of number of people checked in. 
            PeopleCheckedIn++;
            // Remove the person just checked in on this page from the list of those needing to be checked in.
            var personJustCheckedIn = CheckInPeopleIds.FirstOrDefault();
            CheckInPeopleIds.Remove( personJustCheckedIn );
            // Add the person just checked in to the list of those that were checked in.
            CheckedInPeopleIds.Add( personJustCheckedIn );
            // Add this person's time and activity to the list for the confirmation page.
            List<int> temp = new List<int>();
            temp.Add( personJustCheckedIn );
            temp.Add( chosenActivityId );
            temp.Add( chosenTimeId );
            CheckInTimeAndActivityList.Add( temp );
            SaveState();

            if ( PeopleCheckedIn != CheckInPersonCount )
            {
                NavigateToCurrentPage();
            }
            else
            {
                NavigateToNextPage();
            }
        }

        private void ProcessActivities()
        {
            var errors = new List<string>();
            //if ( ProcessActivity( "Location Search", out errors ) )
            if ( ProcessActivity( "Activity Search", out errors ) )
            {
                SaveState();
                //NavigateToNextPage();
            }
            else
            {
                string errorMsg = "<ul><li>" + errors.AsDelimited( "</li><li>" ) + "</li></ul>";
                maWarning.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
            }
        }

        #endregion
    }
}