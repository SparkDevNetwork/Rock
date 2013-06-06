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
using System.Runtime.InteropServices;

namespace RockWeb.Blocks.CheckIn.Attended
{
    [Description( "Attended Check-In Family Select Block" )]
    public partial class FamilySelect : CheckInBlock
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
                    //rFamily.DataSource = CurrentCheckInState.CheckIn.Families;
                    //rFamily.DataBind();
                    lvFamily.DataSource = CurrentCheckInState.CheckIn.Families;
                    lvFamily.DataBind();

                    if ( CurrentCheckInState.CheckIn.Families.Count == 0 )
                    {
                        familyTitle.InnerText = "No Search Results Found";
                        lbNext.Enabled = false;
                        lbNext.Visible = false;
                        familyDiv.Visible = false;
                        personDiv.Visible = false;
                        emptyDiv.Visible = false;
                        nothingFoundMessage.Visible = true;
                    }
                    else
                    {
                        nothingFoundMessage.Visible = false;
                    }

                    if ( CurrentCheckInState.CheckIn.Families.Count == 1 &&
                        !CurrentCheckInState.CheckIn.ConfirmSingleFamily )
                    {
                        if ( UserBackedUp )
                        {
                            GoBack();
                        }
                        else
                        {
                            foreach ( var family in CurrentCheckInState.CheckIn.Families )
                            {
                                family.Selected = true;
                            }

                            ProcessFamily();
                        }
                    }
                }
            }
        }

        #endregion

        #region Edit Events

        //protected void rFamily_ItemCommand( object source, RepeaterCommandEventArgs e )
        //{
        //    if ( KioskCurrentlyActive )
        //    {
        //        int id = int.Parse( e.CommandArgument.ToString() );
        //        var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Group.Id == id ).FirstOrDefault();
        //        if ( family != null )
        //        {
        //            if ( family.Selected )
        //            {
        //                family.Selected = false;
        //                var control = (LinkButton)e.Item.FindControl( "lbSelectFamily" );
        //                control.RemoveCssClass( "active" );
        //                rPerson.DataSource = null;
        //                rPerson.DataBind();
        //                SaveState();
        //            }
        //            else
        //            {
        //                // make sure there are no other families selected
        //                foreach ( var f in CurrentCheckInState.CheckIn.Families )
        //                {
        //                    f.Selected = false;
        //                }
        //                
        //                // make sure no other families look like they're selected
        //                foreach ( RepeaterItem ri in rFamily.Items )
        //                {
        //                    ( (LinkButton)ri.FindControl("lbSelectFamily") ).RemoveCssClass( "active" );
        //                }

        //                // select the clicked on family
        //                family.Selected = true;
        //                ( (LinkButton)e.Item.FindControl( "lbSelectFamily" ) ).AddCssClass( "active" );
        //                ProcessFamily();
        //            }
        //        }
        //    }
        //}

        protected void rPerson_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                if ( family != null )
                {
                    int id = int.Parse( e.CommandArgument.ToString() );
                    var familyMember = family.People.Where( m => m.Person.Id == id ).FirstOrDefault();
                    if ( familyMember != null )
                    {
                        if ( familyMember.Selected )
                        {
                            familyMember.Selected = false;
                            var control = (LinkButton)e.Item.FindControl( "lbSelectPerson" );
                            control.RemoveCssClass( "active" );
                            SaveState();
                        }
                        else
                        {
                            familyMember.Selected = true;
                            ( (LinkButton)e.Item.FindControl( "lbSelectPerson" ) ).AddCssClass( "active" );
                            SaveState();
                        }
                    }
                }
            }
        }

        protected void lbBack_Click( object sender, EventArgs e )
        {
            GoBack();
        }
                
        protected void lbNext_Click( object sender, EventArgs e )
        {
            var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
            if ( family == null )
            {
                maWarning.Show( "You need to pick a family.", ModalAlertType.Warning );
                return;
            }

            var people = family.People.Where( p => p.Selected ).FirstOrDefault();
            if ( people == null )
            {
                maWarning.Show( "You need to pick at least one family member.", ModalAlertType.Warning );
                return;
            }
            
            // set the number of people that need to be checked in. set the counter of those actually checked in to zero. this is important on the activity select page.
            CheckInPersonCount = family.People.Where( p => p.Selected ).Count();
            PeopleCheckedIn = 0;
            List<int> peopleIds = new List<int>();
            foreach ( var person in family.People.Where( p => p.Selected ) )
            {
                peopleIds.Add( person.Person.Id );
            }

            CheckInPeopleIds = peopleIds;
            CheckedInPeopleIds = new List<int>();
            CheckInTimeAndActivityList = new List<List<int>>();
            GoNext();
        }

        protected void lbAddFamily_Click( object sender, EventArgs e)
        {
        }

        protected void lbAddPerson_Click( object sender, EventArgs e)
        {
        }

        protected void lbAddVisitor_Click( object sender, EventArgs e)
        {
        }

        #endregion

        #region Internal Methods 

        private void GoBack()
        {
            CurrentCheckInState.CheckIn.SearchType = null;
            CurrentCheckInState.CheckIn.SearchValue = string.Empty;
            CurrentCheckInState.CheckIn.Families = new List<CheckInFamily>();

            SaveState();
            NavigateToPreviousPage();
        }

        private void ProcessFamily()
        {
            var errors = new List<string>();
            if ( ProcessActivity( "Person Search", out errors ) )
            {
                ProcessPerson();
            }
            else
            {
                string errorMsg = "<ul><li>" + errors.AsDelimited( "</li><li>" ) + "</li></ul>";
                maWarning.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
            }
        }

        private void ProcessPerson()
        {
            var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
            if ( family != null )
            {
                if ( family.People.Count == 1 )
                {
                    if ( UserBackedUp )
                    {
                        GoBack();
                    }
                    else
                    {
                        foreach ( var familyMember in family.People )
                        {
                            familyMember.Selected = true;
                        }
                    }
                }

                rPerson.DataSource = family.People;
                rPerson.DataBind();               
            }
        }

        private void GoNext()
        {
            SaveState();
            NavigateToNextPage();
        }

        #endregion
        protected void lvFamily_PagePropertiesChanging( object sender, PagePropertiesChangingEventArgs e )
        {
            //set current page startindex, max rows and rebind to false
            Pager.SetPageProperties( e.StartRowIndex, e.MaximumRows, false );
            //rebind List View
            lvFamily.DataSource = CurrentCheckInState.CheckIn.Families;
            lvFamily.DataBind();
        }
        protected void lvFamily_ItemCommand( object sender, ListViewCommandEventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                int id = int.Parse( e.CommandArgument.ToString() );
                var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Group.Id == id ).FirstOrDefault();
                if ( family != null )
                {
                    if ( family.Selected )
                    {
                        family.Selected = false;
                        var control = (LinkButton)e.Item.FindControl( "lSelectFamily" );
                        control.RemoveCssClass( "active" );
                        rPerson.DataSource = null;
                        rPerson.DataBind();
                        SaveState();
                    }
                    else
                    {
                        // make sure there are no other families selected
                        foreach ( var f in CurrentCheckInState.CheckIn.Families )
                        {
                            f.Selected = false;
                        }

                        // make sure no other families look like they're selected
                        foreach(ListViewDataItem li in lvFamily.Items)
                        //foreach ( RepeaterItem ri in rFamily.Items )
                        {
                            ( (LinkButton)li.FindControl( "lSelectFamily" ) ).RemoveCssClass( "active" );
                            //( (LinkButton)ri.FindControl( "lSelectFamily" ) ).RemoveCssClass( "active" );
                        }

                        // select the clicked on family
                        family.Selected = true;
                        ( (LinkButton)e.Item.FindControl( "lSelectFamily" ) ).AddCssClass( "active" );
                        ProcessFamily();
                    }
                }
            }
        }
}
}