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
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Model;
using Rock.Web.UI.Controls;
using System.Runtime.InteropServices;

namespace RockWeb.Blocks.CheckIn.Attended
{
    [Description( "Attended Check-In Family Select Block" )]
    [LinkedPage( "Activity Select Page" )]
    public partial class FamilySelect : CheckInBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
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
                    //grdPersonSearchResults.DataKeyNames = new string[] { "personId" }; 
                    if ( CurrentCheckInState.CheckIn.Families.Count == 0 )
                    {
                        familyTitle.InnerText = "No Search Results Found";
                        lbNext.Enabled = false;
                        lbNext.Visible = false;
                        familyDiv.Visible = false;
                        personDiv.Visible = false;
                        visitorDiv.Visible = false;
                        nothingFoundMessage.Visible = true;
                    }
                    else if ( CurrentCheckInState.CheckIn.Families.Count == 1 &&
                             !CurrentCheckInState.CheckIn.ConfirmSingleFamily )
                    {
                        if ( UserBackedUp )
                        {
                            GoBack();
                        }
                        else
                        {
                            CurrentCheckInState.CheckIn.Families.FirstOrDefault().Selected = true;
                            ProcessFamily();
                        }
                    }
                    else
                    {
                        nothingFoundMessage.Visible = false;
                        lvFamily.DataSource = CurrentCheckInState.CheckIn.Families;
                        lvFamily.DataBind();
                    }
                }
            }
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the PagePropertiesChanging event of the lvFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PagePropertiesChangingEventArgs"/> instance containing the event data.</param>
        protected void lvFamily_PagePropertiesChanging( object sender, PagePropertiesChangingEventArgs e )
        {
            // set current page startindex, max rows and rebind to false
            dpPager.SetPageProperties( e.StartRowIndex, e.MaximumRows, false );
            
            // rebind List View
            lvFamily.DataSource = CurrentCheckInState.CheckIn.Families;
            lvFamily.DataBind();
        }

        /// <summary>
        /// Handles the ItemCommand event of the lvFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewCommandEventArgs"/> instance containing the event data.</param>
        protected void lvFamily_ItemCommand( object sender, ListViewCommandEventArgs e )
        {
            int id = int.Parse( e.CommandArgument.ToString() );
            var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Group.Id == id ).FirstOrDefault();
            if ( family != null )
            {
                if ( family.Selected )
                {
                    family.Selected = false;
                    ( (LinkButton)e.Item.FindControl( "lbSelectFamily" ) ).RemoveCssClass( "active" );
                    repPerson.DataSource = null;
                    repPerson.DataBind();
                }
                else
                {
                    // make sure there are no other families selected
                    //foreach ( var fam in CurrentCheckInState.CheckIn.Families )
                    //{
                    //    fam.Selected = false;
                    //}
                    CurrentCheckInState.CheckIn.Families.ForEach( f => f.Selected = false );


                    // make sure no other families look like they're selected
                    foreach ( ListViewDataItem li in lvFamily.Items )
                    {
                        ( (LinkButton)li.FindControl( "lbSelectFamily" ) ).RemoveCssClass( "active" );
                    }

                    // select the clicked on family
                    family.Selected = true;
                    ( (LinkButton)e.Item.FindControl( "lbSelectFamily" ) ).AddCssClass( "active" );
                    ProcessFamily();

                    // i think this will be done in the find family members action...
                    // mark all the peoples as active
                    foreach ( RepeaterItem item in repPerson.Items )
                    {
                        ( (LinkButton)item.FindControl( "lbSelectPerson" ) ).AddCssClass( "active" );
                    }
                }
                SaveState();
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the repPerson control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void repPerson_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            // *************** TAKE ALL OF THIS OUT TO ALLOW FOR THE TOGGLE *******************
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
                        ( (LinkButton)e.Item.FindControl( "lbSelectPerson" ) ).RemoveCssClass( "active" );
                    }
                    else
                    {
                        familyMember.Selected = true;
                        ( (LinkButton)e.Item.FindControl( "lbSelectPerson" ) ).AddCssClass( "active" );
                    }
                    SaveState();
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the repVisitors control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void repVisitors_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            // *************** TAKE ALL OF THIS OUT TO ALLOW FOR THE TOGGLE *******************
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
                        ( (LinkButton)e.Item.FindControl( "lbSelectVisitor" ) ).RemoveCssClass( "active" );
                    }
                    else
                    {
                        familyMember.Selected = true;
                        ( (LinkButton)e.Item.FindControl( "lbSelectVisitor" ) ).AddCssClass( "active" );
                    }
                    SaveState();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddPerson_Click( object sender, EventArgs e )
        {
            ResetAddPersonFields();
            lblAddPersonHeader.Text = "Add Person";
            personVisitorType.Value = "Person";
            mpePerson.Show();
        }

        /// <summary>
        /// Handles the Click event of the lbAddVisitor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddVisitor_Click( object sender, EventArgs e)
        {
            ResetAddPersonFields();
            lblAddPersonHeader.Text = "Add Visitor";
            personVisitorType.Value = "Visitor";
            mpePerson.Show();
        }

        /// <summary>
        /// Resets the add person fields.
        /// </summary>
        protected void ResetAddPersonFields()
        {
            tbFirstNameSearch.Text = "";
            tbLastNameSearch.Text = "";
            dtpDOBSearch.Text = "";
            tbGradeSearch.Text = "";
            grdPersonSearchResults.DataBind();
            grdPersonSearchResults.Visible = false;
            lbAddSearchedForPerson.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the lbAddFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddFamily_Click( object sender, EventArgs e )
        {
            // open up a modal window & display the add family panel.
            for ( var i = 1; i <= 12; i++ )
            {
                ( (DataTextBox)FindControl( "tbFirstName" + i.ToString() ) ).Text = "";
                ( (DataTextBox)FindControl( "tbLastName" + i.ToString() ) ).Text = "";
                ( (DatePicker)FindControl( "dpDOB" + i.ToString() ) ).Text = "";
                ( (DataTextBox)FindControl( "tbGrade" + i.ToString() ) ).Text = "";
            }
            valSummaryBottom.DataBind();
            mpeFamily.Show();
        }

        /// <summary>
        /// Handles the Click event of the lbAddFamilyCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddFamilyCancel_Click( object sender, EventArgs e )
        {
            // if we need to clear out anything when we cancel the modal...this would be the place to do it.
        }

        /// <summary>
        /// Handles the Click event of the lbAddFamilySave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddFamilySave_Click( object sender, EventArgs e )
        {
            // Create the family and person data
            List<Person> familyList = new List<Person>();
            Group familyGroup = new Group();
            for ( var i = 1; i <= 12; i++ )
            {
                DataTextBox firstName = (DataTextBox)FindControl( "tbFirstName" + i.ToString() );
                DataTextBox lastName = (DataTextBox)FindControl( "tbLastName" + i.ToString() );
                DatePicker dob = (DatePicker)FindControl( "dpDOB" + i.ToString() );
                DataTextBox grade = (DataTextBox)FindControl( "tbGrade" + i.ToString() );

                // if the first name, last name and DOB are not blank for this set of controls, add this person to the family list.
                if ( firstName.Text != "" && lastName.Text != "" && dob.Text != "" )
                {
                    // if this is the first person added, create a family group
                    if (familyGroup == null)
                    {
                        familyGroup = AddFamilyGroup( lastName.Text );
                    }

                    // add the new person record
                    var person = AddPerson( firstName.Text, lastName.Text, dob.Text, grade.Text );

                    // add the group member record
                    var groupMember = AddGroupMember( familyGroup, person );

                    familyList.Add( person );
                }                
            }

            if ( familyList.Count == 0 )
            {
                return;
            }

            // Set the family up for check in
            CheckInFamily CIF = new CheckInFamily();
            string subCaption = "";
            foreach( Person person in familyList )
            {
                CheckInPerson CIP = new CheckInPerson();
                CIP.Person = person;
                CIP.Selected = true;
                CIF.People.Add( CIP );
                if ( subCaption == "" )
                {
                    subCaption += person.FirstName;
                }
                else
                {
                    subCaption += ", " + person.FirstName;
                }

            }

            CIF.Group = familyGroup;
            CIF.Caption = familyList.FirstOrDefault().LastName;
            CIF.SubCaption = subCaption;
            CIF.Selected = true;

            CurrentCheckInState.CheckIn.Families.Clear();
            CurrentCheckInState.CheckIn.Families.Add( CIF );
            lvFamily.DataSource = CurrentCheckInState.CheckIn.Families;
            lvFamily.DataBind();
            repPerson.DataSource = CIF.People;
            repPerson.DataBind();
            foreach ( ListViewDataItem li in lvFamily.Items )
            {
                ( (LinkButton)li.FindControl( "lbSelectFamily" ) ).AddCssClass( "active" );
                
            }
            foreach ( RepeaterItem item in repPerson.Items )
            {
                ( (LinkButton)item.FindControl( "lbSelectPerson" ) ).AddCssClass( "active" );
            }
            SaveState();

        }

        /// <summary>
        /// Handles the Click event of the lbAddPersonCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddPersonCancel_Click( object sender, EventArgs e )
        {
            // if we need to do anything when we cancel this modal window...this would be the place to do it.
        }

        /// <summary>
        /// Handles the Click event of the lbAddPersonSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddPersonSearch_Click( object sender, EventArgs e )
        {
            var people = SearchForPeople();
            System.Data.DataTable dt = LoadTheGrid(people);
            grdPersonSearchResults.DataSource = dt;
            grdPersonSearchResults.PageIndex = 0;
            grdPersonSearchResults.DataBind();
            grdPersonSearchResults.Visible = true;
            lbAddSearchedForPerson.Visible = true;
            mpePerson.Show();
        }

        /// <summary>
        /// Handles the Click event of the PreviousButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void PreviousButton_Click( object sender, EventArgs e )
        {
            if ( addFamilyNamesPage2.Visible )
            {
                addFamilyNamesPage1.Visible = true;
                addFamilyNamesPage2.Visible = false;
                PreviousButton.Visible = false;
            }
            else if ( addFamilyNamesPage3.Visible )
            {
                addFamilyNamesPage2.Visible = true;
                addFamilyNamesPage3.Visible = false;
                MoreButton.Visible = true;
            }
            mpeFamily.Show();
        }

        /// <summary>
        /// Handles the Click event of the MoreButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void MoreButton_Click( object sender, EventArgs e )
        {
            if ( addFamilyNamesPage1.Visible )
            {
                addFamilyNamesPage1.Visible = false;
                addFamilyNamesPage2.Visible = true;
                PreviousButton.Visible = true;
            }
            else if ( addFamilyNamesPage2.Visible )
            {
                addFamilyNamesPage2.Visible = false;
                addFamilyNamesPage3.Visible = true;
                MoreButton.Visible = false;
            }
            mpeFamily.Show();
        }

        /// <summary>
        /// Handles the ServerValidate event of the cvDOBValidator control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="ServerValidateEventArgs"/> instance containing the event data.</param>
        protected void cvDOBValidator_ServerValidate( object source, ServerValidateEventArgs args )
        {
            // check to see if what is in the field is a date.
            DateTime someDate;
            args.IsValid = false;
            if ( DateTime.TryParse( args.Value, out someDate ) )
            {
                args.IsValid = true;
            }
        }

        /// <summary>
        /// Handles the RowCommand event of the grdPersonSearchResults control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewCommandEventArgs"/> instance containing the event data.</param>
        protected void grdPersonSearchResults_RowCommand( object sender, GridViewCommandEventArgs e )
        {
            if ( e.CommandName == "Add" )
            {
                int index = int.Parse( e.CommandArgument.ToString() );
                GridViewRow row = grdPersonSearchResults.Rows[index];
                var person = new PersonService().Get( int.Parse( grdPersonSearchResults.DataKeys[index].Value.ToString() ) );
                //var person = new PersonService().Get( int.Parse( row.Cells[0].Text ) );
                var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                if ( family != null )
                {
                    CheckInPerson CIP = new CheckInPerson();
                    CIP.Person = person;
                    CIP.Selected = true;
                    if ( personVisitorType.Value == "Person" )
                    {
                        // this came from Add Person
                        var isPersonInFamily = family.People.Any( p => p.Person.Id == person.Id );
                        if ( !isPersonInFamily )
                        {
                            // because this person is being added to this family, we should make sure his/her groupmember record reflects that.
                            GroupMemberService gms = new GroupMemberService();
                            var groupMember = gms.GetByPersonId( person.Id ).FirstOrDefault();
                            groupMember.GroupId = family.Group.Id;
                            Rock.Data.RockTransactionScope.WrapTransaction( () =>
                            {
                                gms.Save( groupMember, CurrentPersonId );
                            } );

                            CIP.FamilyMember = true;
                            family.People.Add( CIP );
                            repPerson.DataSource = family.People;
                            repPerson.DataBind();
                            foreach ( RepeaterItem item in repPerson.Items )
                            {
                                ( (LinkButton)item.FindControl( "lbSelectPerson" ) ).AddCssClass( "active" );
                            }
                        }
                    }
                    else
                    {
                        // this came from Add Visitor
                        List<CheckInPerson> visitors = new List<CheckInPerson>();

                        // check to see if someone in the family has a GroupRole of OWNER in the GroupMember table.
                        GroupMemberService groupMemberService = new GroupMemberService();
                        var foundAnOwner = false;
                        foreach ( var p in family.People )
                        {
                            if ( groupMemberService.GetByGroupIdAndPersonIdAndGroupRoleId( family.Group.Id, p.Person.Id, new GroupRoleService().Get( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER ) ).Id ) != null )
                            {
                                foundAnOwner = true;
                            }
                        }

                        // nobody with the OWNER GroupRole was found in the family. So add it to everyone in the family.
                        if ( !foundAnOwner )
                        {
                            foreach ( var p in family.People )
                            {
                                AddOwnerGroupRole( family.Group, p.Person );
                            }
                        }

                        //var knownRelationshipGroup = groupMemberService.Queryable()
                        //    .Where( m =>
                        //        m.PersonId == personId &&
                        //        m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER ) ) )
                        //    .Select( m => m.Group )
                        //    .FirstOrDefault();

                        Person.CreateCheckinRelationship( family.People.FirstOrDefault().Person.Id, person.Id, CurrentPersonId );
                        CIP.FamilyMember = false;
                        family.People.Add( CIP );

                        // need to get the people that have the CanCheckIn GroupRole on a GroupMember record associated with the selected Family Group.
                        GroupMemberService gms = new GroupMemberService();
                        var canCheckInGroup = gms.Queryable()
                            .Where( m =>
                                m.PersonId == person.Id &&
                                m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN ) ) )
                            .Select( m => m.Group )
                            .FirstOrDefault();

                        var groupMembers = gms.GetByGroupId( canCheckInGroup.Id );
                        //var groupMember = groupMembers.FirstOrDefault( g => g.GroupRoleId == new GroupRoleService().Get( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN ) ).Id );
                        //var checkInPerson = new CheckInPerson();
                        //checkInPerson.Person = groupMember.Person.Clone( false );
                        //visitors.Add( checkInPerson );
                        foreach ( var groupMember in groupMembers )
                        {
                            if ( groupMember.GroupRoleId == new GroupRoleService().Get( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN ) ).Id )
                            {
                                CheckInPerson checkInPerson = new CheckInPerson();
                                checkInPerson.Person = groupMember.Person.Clone( false );
                                visitors.Add( checkInPerson );
                            }
                        } 

                        repVisitors.DataSource = visitors;
                        repVisitors.DataBind();

                        // make sure the one person you just added is selected
                        foreach ( RepeaterItem item in repVisitors.Items )
                        {
                            if ( person.Id == int.Parse( ( (LinkButton)item.FindControl( "lbSelectVisitor" ) ).CommandArgument ) )
                            {
                                ( (LinkButton)item.FindControl( "lbSelectVisitor" ) ).AddCssClass( "active" );
                            }
                        }
                    }
                    SaveState();
                }
                else
                {
                    mpePerson.Show();
                    string errorMsg = "<ul><li>You have to pick a family to add this person to.</li></ul>";
                    maAddPerson.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
                }
            }
            else
            {
                mpePerson.Show();
                var people = SearchForPeople();
                System.Data.DataTable dt = LoadTheGrid(people);
                grdPersonSearchResults.DataSource = dt;
                grdPersonSearchResults.DataBind();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddSearchedForPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddSearchedForPerson_Click( object sender, EventArgs e )
        {
            if ( string.IsNullOrEmpty( tbFirstNameSearch.Text ) || string.IsNullOrEmpty( tbLastNameSearch.Text ) || string.IsNullOrEmpty( dtpDOBSearch.Text ) )
            {
                mpePerson.Show();
                string errorMsg = "<ul><li>You have to fill out the First Name, Last Name, and DOB fields first.</li></ul>";
                maAddPerson.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
            }
            else
            {
                // Add the Person record.
                var person = AddPerson( tbFirstNameSearch.Text, tbLastNameSearch.Text, dtpDOBSearch.Text, tbGradeSearch.Text );

                var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                if ( family == null )
                {
                    // There isn't a current family
                    family = new CheckInFamily();
                    family.Selected = true;
                    CurrentCheckInState.CheckIn.Families.Add( family );

                    // Add a Group for the new family
                    family.Group = AddFamilyGroup( person.LastName );
                }

                // put the person into place to be checked in.
                CheckInPerson CIP = new CheckInPerson();
                CIP.Person = person;
                CIP.Selected = true;

                if ( personVisitorType.Value == "Person" )
                {
                    // Add the Person's GroupMember data so that they can be part of the family
                    var groupMember = AddGroupMember( family.Group, person );
                    CIP.FamilyMember = true;
                    family.People.Add( CIP );

                    repPerson.DataSource = family.People;
                    repPerson.DataBind();
                    foreach ( RepeaterItem item in repPerson.Items )
                    {
                        ( (LinkButton)item.FindControl( "lbSelectPerson" ) ).AddCssClass( "active" );
                    }
                }
                else
                {
                    // This is a visitor
                    Person.CreateCheckinRelationship( family.People.FirstOrDefault().Person.Id, person.Id, CurrentPersonId );
                    CIP.FamilyMember = false;
                    family.People.Add( CIP );

                    repVisitors.DataSource = family.People.Where( p => p.FamilyMember == false );
                    repVisitors.DataBind();
                    
                    // make sure the one person you just added is selected
                    foreach ( RepeaterItem item in repVisitors.Items )
                    {
                        if ( person.Id == int.Parse( ( (LinkButton)item.FindControl( "lbSelectVisitor" ) ).CommandArgument ) )
                        {
                            ( (LinkButton)item.FindControl( "lbSelectVisitor" ) ).AddCssClass( "active" );
                        }
                    }
                }
                SaveState();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbBack_Click( object sender, EventArgs e )
        {
            GoBack();
        }

        /// <summary>
        /// Handles the Click event of the lbNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
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

            var errors = new List<string>();
            if ( ProcessActivity( "Activity Search", out errors ) )
            {
                SaveState();
                NavigateToNextPage();
            }
            else
            {
                string errorMsg = "<ul><li>" + errors.AsDelimited( "</li><li>" ) + "</li></ul>";
                maWarning.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
            }
        }

        #endregion

        #region Internal Methods 

        /// <summary>
        /// Goes back one page.
        /// </summary>
        private void GoBack()
        {
            CurrentCheckInState.CheckIn.Families = new List<CheckInFamily>();

            SaveState();
            NavigateToPreviousPage();
        }

        /// <summary>
        /// Processes the family.
        /// </summary>
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

        /// <summary>
        /// Processes the person.
        /// </summary>
        private void ProcessPerson()
        {
            List<CheckInPerson> peopleInFamily = new List<CheckInPerson>();
            List<CheckInPerson> relatedPeople = new List<CheckInPerson>();
            var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
            if ( family != null )
            {
                foreach ( var familyMember in family.People )
                {
                    if ( familyMember.FamilyMember )
                    {
                        familyMember.Selected = true;
                        peopleInFamily.Add( familyMember );
                    }
                    else
                    {
                        familyMember.Selected = false;
                        relatedPeople.Add( familyMember );
                    }
                }

                SaveState();

                repPerson.DataSource = peopleInFamily;
                repPerson.DataBind();

                repVisitors.DataSource = relatedPeople;
                repVisitors.DataBind();
            }
        }

        /// <summary>
        /// Adds the family group.
        /// </summary>
        /// <param name="familyLastName">Last name of the family.</param>
        /// <returns></returns>
        protected Group AddFamilyGroup( string familyLastName )
        {
            Group group = new Group();
            GroupService gs = new GroupService();
            group.IsSystem = false;
            group.GroupTypeId = new GroupTypeService().Get( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ) ).Id;
            group.Name = familyLastName + " Family";
            group.IsSecurityRole = false;
            group.IsActive = true;
            Rock.Data.RockTransactionScope.WrapTransaction( () =>
            {
                gs.Add( group, CurrentPersonId );
                gs.Save( group, CurrentPersonId );
            } );

            return group;
        }

        /// <summary>
        /// Adds a new person.
        /// </summary>
        /// <param name="firstName">The first name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="DOB">The DOB.</param>
        /// <param name="grade">The grade.</param>
        protected Person AddPerson( string firstName, string lastName, string dob, string grade )
        {
            Person person = new Person();
            person.GivenName = firstName;
            person.LastName = lastName;
            person.BirthDate = Convert.ToDateTime( dob );
            if ( !string.IsNullOrEmpty( grade ) )
            {
                person.Grade = int.Parse( grade );
            }
            PersonService ps = new PersonService();
            Rock.Data.RockTransactionScope.WrapTransaction( () =>
            {
                ps.Add( person, CurrentPersonId );
                ps.Save( person, CurrentPersonId );
            } );

            return person;
        }

        /// <summary>
        /// Adds the group member.
        /// </summary>
        /// <param name="familyGroup">The family group.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        protected GroupMember AddGroupMember( Group familyGroup, Person person )
        {
            GroupMember groupMember = new GroupMember();
            GroupMemberService groupMemberService = new GroupMemberService();
            groupMember.IsSystem = false;
            groupMember.GroupId = familyGroup.Id;
            groupMember.PersonId = person.Id;
            if ( person.Age >= 18 )
            {
                groupMember.GroupRoleId = new GroupRoleService().Get( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ).Id;
            }
            else
            {
                groupMember.GroupRoleId = new GroupRoleService().Get( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD ) ).Id;
            }
            Rock.Data.RockTransactionScope.WrapTransaction( () =>
            {
                groupMemberService.Add( groupMember, CurrentPersonId );
                groupMemberService.Save( groupMember, CurrentPersonId );
            } );

            return groupMember;
        }

        /// <summary>
        /// Adds the owner group role.
        /// </summary>
        /// <param name="familyGroup">The family group.</param>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        protected GroupMember AddOwnerGroupRole( Group familyGroup, Person person )
        {
            GroupMember groupMember = new GroupMember();
            GroupMemberService groupMemberService = new GroupMemberService();
            groupMember.IsSystem = false;
            groupMember.GroupId = familyGroup.Id;
            groupMember.PersonId = person.Id;
            groupMember.GroupRoleId = new GroupRoleService().Get( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER ) ).Id;
            Rock.Data.RockTransactionScope.WrapTransaction( () =>
            {
                groupMemberService.Add( groupMember, CurrentPersonId );
                groupMemberService.Save( groupMember, CurrentPersonId );
            } );

            return groupMember;
        }
        
        /// <summary>
        /// Searches for people.
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<Person> SearchForPeople()
        {
            var personService = new PersonService();
            IEnumerable<Person> people = Enumerable.Empty<Person>();

            if ( !string.IsNullOrEmpty( tbFirstNameSearch.Text ) && !string.IsNullOrEmpty( tbLastNameSearch.Text ) )
            {
                people = personService.GetByFullName( tbFirstNameSearch.Text + " " + tbLastNameSearch.Text );
            }
            else if ( !string.IsNullOrEmpty( tbLastNameSearch.Text ) )
            {
                people = personService.GetByLastName( tbLastNameSearch.Text );
            }
            else if ( !string.IsNullOrEmpty( tbFirstNameSearch.Text ) )
            {
                people = personService.GetByFirstName( tbFirstNameSearch.Text );
            }

            if ( !string.IsNullOrEmpty( dtpDOBSearch.Text ) )
            {
                DateTime someDate;
                if ( DateTime.TryParse( dtpDOBSearch.Text, out someDate ) )
                {
                    people = people.Where( p => p.BirthDate == Convert.ToDateTime( dtpDOBSearch.Text ) );
                }
            }

            if ( !string.IsNullOrEmpty( tbGradeSearch.Text ) )
            {
                people = people.Where( p => p.Grade == int.Parse( tbGradeSearch.Text ) );
            }
            return people;
        }

        /// <summary>
        /// Loads the grid.
        /// </summary>
        /// <param name="people">The people to load.</param>
        /// <returns></returns>
        protected System.Data.DataTable LoadTheGrid( IEnumerable<Person> people )
        {
            System.Data.DataTable dt = new System.Data.DataTable();

            var column = new System.Data.DataColumn();
            column.DataType = System.Type.GetType( "System.String" );
            column.ColumnName = "personId";
            column.ReadOnly = true;
            dt.Columns.Add( column );

            column = new System.Data.DataColumn();
            column.DataType = System.Type.GetType( "System.String" );
            column.ColumnName = "personFirstName";
            column.ReadOnly = false;
            dt.Columns.Add( column );

            column = new System.Data.DataColumn();
            column.DataType = System.Type.GetType( "System.String" );
            column.ColumnName = "personLastName";
            column.ReadOnly = false;
            dt.Columns.Add( column );

            column = new System.Data.DataColumn();
            column.DataType = System.Type.GetType( "System.String" );
            column.ColumnName = "personDOB";
            column.ReadOnly = false;
            dt.Columns.Add( column );

            column = new System.Data.DataColumn();
            column.DataType = System.Type.GetType( "System.String" );
            column.ColumnName = "personGrade";
            column.ReadOnly = false;
            dt.Columns.Add( column );

            foreach ( var person in people )
            {
                System.Data.DataRow row;
                row = dt.NewRow();
                row["personId"] = person.Id;
                row["personFirstName"] = person.FirstName;
                row["personLastName"] = person.LastName;
                row["personDOB"] = Convert.ToDateTime( person.BirthDate ).ToString( "d" );
                row["personGrade"] = person.Grade;
                dt.Rows.Add( row );
            }

            System.Data.DataView dv = new System.Data.DataView( dt );
            dv.Sort = "personLastName ASC, personFirstName ASC";
            return dv.ToTable();
        }

        /// <summary>
        /// Determines whether the specified webcontrol has an "active" class.
        /// </summary>
        /// <param name="webcontrol">The webcontrol.</param>
        /// <returns>
        ///   <c>true</c> if the specified webcontrol has an "active" class; otherwise, <c>false</c>.
        /// </returns>
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

        #endregion
    }
}