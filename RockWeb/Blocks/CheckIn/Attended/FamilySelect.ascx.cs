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

        protected void lvFamily_PagePropertiesChanging( object sender, PagePropertiesChangingEventArgs e )
        {
            // set current page startindex, max rows and rebind to false
            Pager.SetPageProperties( e.StartRowIndex, e.MaximumRows, false );
            
            // rebind List View
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
                        var control = (LinkButton)e.Item.FindControl( "lbSelectFamily" );
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
                        foreach ( ListViewDataItem li in lvFamily.Items )
                        {
                            ( (LinkButton)li.FindControl( "lbSelectFamily" ) ).RemoveCssClass( "active" );
                        }

                        // select the clicked on family
                        family.Selected = true;
                        ( (LinkButton)e.Item.FindControl( "lbSelectFamily" ) ).AddCssClass( "active" );
                        ProcessFamily();
                        foreach ( RepeaterItem item in rPerson.Items )
                        {
                            ( (LinkButton)item.FindControl( "lbSelectPerson" ) ).AddCssClass( "active" );
                        }
                        
                    }
                }
            }
        }

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
            // open up a modal window & display the add family panel.
        }

        protected void lbAddPerson_Click( object sender, EventArgs e )
        {
        }

        protected void lbAddVisitor_Click( object sender, EventArgs e)
        {
        }

        protected void lbAddFamilyCancel_Click( object sender, EventArgs e )
        {

        }
        protected void lbAddFamilyAdd_Click( object sender, EventArgs e )
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
        protected void AddButton_Click( object sender, EventArgs e )
        {
            // need to add code here to add a set of text boxes/date pickers as a row on this page.
            Panel childPanel = new Panel();
            childPanel.AddCssClass( "row-fluid attended-checkin-body person" );
            Panel divOne = new Panel();
            divOne.AddCssClass( "span3" );
            DataTextBox FirstName = new DataTextBox();
            FirstName.AddCssClass( "fullBlock" );
            FirstName.ID = this.ID;
            divOne.Controls.Add( FirstName );
            Panel divTwo = new Panel();
            divTwo.AddCssClass( "span3" );
            DataTextBox LastName = new DataTextBox();
            LastName.AddCssClass( "fullBlock" );
            LastName.ID = this.ID;
            divTwo.Controls.Add( LastName );
            Panel divThree = new Panel();
            divThree.AddCssClass( "span3" );
            DatePicker DOBAgePicker = new DatePicker();
            DOBAgePicker.ID = this.ID;
            divThree.Controls.Add( DOBAgePicker );
            Panel divFour = new Panel();
            divFour.AddCssClass( "span3" );
            TextBox Grade = new TextBox();
            Grade.AddCssClass( "fullBlock" );
            Grade.ID = this.ID;
            divFour.Controls.Add( Grade );
            childPanel.Controls.Add( divOne );
            childPanel.Controls.Add( divTwo );
            childPanel.Controls.Add( divThree );
            childPanel.Controls.Add( divFour );
            div1.Controls.Add( childPanel );
            mpe.Show();
        }
        protected void PreviousButton_Click( object sender, EventArgs e )
        {
            if ( div2.Visible )
            {
                div1.Visible = true;
                div2.Visible = false;
                PreviousButton.Visible = false;
            }
            else if ( div3.Visible )
            {
                div2.Visible = true;
                div3.Visible = false;
                MoreButton.Visible = true;
            }
            mpe.Show();
        }

        protected void MoreButton_Click( object sender, EventArgs e )
        {
            if ( div1.Visible )
            {
                div1.Visible = false;
                div2.Visible = true;
                PreviousButton.Visible = true;
            }
            else if ( div2.Visible )
            {
                div2.Visible = false;
                div3.Visible = true;
                MoreButton.Visible = false;
            }
            mpe.Show();
        }

        protected void cvDOBAgeValidator_ServerValidate( object source, ServerValidateEventArgs args )
        {
            // check to see if what is in the field is a number 3 digits or less (age) or a date.
            int someNumber;
            DateTime someDate;
            args.IsValid = false;
            if ( ( args.Value.Length <= 3 && int.TryParse( args.Value, out someNumber ) ) || ( DateTime.TryParse( args.Value, out someDate ) ) )
            {
                args.IsValid = true;
            }
        }
}
}