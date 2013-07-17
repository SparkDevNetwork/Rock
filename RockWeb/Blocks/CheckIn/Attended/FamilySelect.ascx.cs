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

        /// <summary>
        /// Handles the PagePropertiesChanging event of the lvFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PagePropertiesChangingEventArgs"/> instance containing the event data.</param>
        protected void lvFamily_PagePropertiesChanging( object sender, PagePropertiesChangingEventArgs e )
        {
            // set current page startindex, max rows and rebind to false
            Pager.SetPageProperties( e.StartRowIndex, e.MaximumRows, false );
            
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
                        SaveState();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rPerson control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Handles the ItemCommand event of the rVisitors control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rVisitors_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                if ( family != null )
                {
                    int id = int.Parse( e.CommandArgument.ToString() );

                    // no matter the id (if it's 0, this person is brand new and hasn't been added to the system yet), add the person to this family for this check in
                    if ( HasActiveClass((LinkButton)e.Item.FindControl( "lbSelectVisitor" ) ) )
                    {
                        // this visitor is already selected. unselect it.
                    }
                    else
                    {
                        // this visitor is not selected. select it.
                        CheckInPerson CIP = new CheckInPerson();
                    }

                    // ************************************ NOT DONE NOT DONE NOT DONE *************************************************
                }
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

        /// <summary>
        /// Handles the Click event of the lbAddFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddFamily_Click( object sender, EventArgs e)
        {
            // open up a modal window & display the add family panel.
            for ( var i = 1; i <= 12; i++ )
            {
                var FirstNameControl = "tbFirstName" + i.ToString();
                var LastNameControl = "tbLastName" + i.ToString();
                var DOBAgeControl = "dpDOBAge" + i.ToString();
                var GradeControl = "tbGrade" + i.ToString();
                DataTextBox FirstName = (DataTextBox)FindControl( FirstNameControl );
                DataTextBox LastName = (DataTextBox)FindControl( LastNameControl );
                DatePicker DOBAge = (DatePicker)FindControl( DOBAgeControl );
                DataTextBox Grade = (DataTextBox)FindControl( GradeControl );
                FirstName.Text = "";
                LastName.Text = "";
                DOBAge.Text = "";
                Grade.Text = "";
            }

            valSummaryBottom.DataBind();
            mpe.Show();
        }

        /// <summary>
        /// Handles the Click event of the lbAddPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddPerson_Click( object sender, EventArgs e )
        {
            ResetAddPersonFields();
            AddPersonVisitorLabel.Text = "Add Person";
            PersonVisitorType.Value = "Person";
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
            AddPersonVisitorLabel.Text = "Add Visitor";
            PersonVisitorType.Value = "Visitor";
            mpePerson.Show();
        }

        /// <summary>
        /// Resets the add person fields.
        /// </summary>
        protected void ResetAddPersonFields()
        {
            tbFirstNameSearch.Text = "";
            tbLastNameSearch.Text = "";
            dpDOBAgeSearch.Text = "";
            tbGradeSearch.Text = "";
            gridPersonSearchResults.DataBind();
            gridPersonSearchResults.Visible = false;
            lbAddSearchedForPerson.Visible = false;
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
        /// Handles the Click event of the lbAddFamilyAdd control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddFamilyAdd_Click( object sender, EventArgs e )
        {
            // Handle getting the data from the modal window and creating a family out of it.
            List<Person> FamilyList = new List<Person>();
            int familyGroupId = 0;
            Group familyGroup = new Group();
            for ( var i = 1; i <= 12; i++ )
            {
                var FirstNameControl = "tbFirstName" + i.ToString();
                var LastNameControl = "tbLastName" + i.ToString();
                var DOBAgeControl = "dpDOBAge" + i.ToString();
                var GradeControl = "tbGrade" + i.ToString();
                DataTextBox FirstName = (DataTextBox)FindControl( FirstNameControl );
                DataTextBox LastName = (DataTextBox)FindControl( LastNameControl );
                DatePicker DOBAge = (DatePicker)FindControl( DOBAgeControl );
                DataTextBox Grade = (DataTextBox)FindControl( GradeControl );

                // if the first name, last name and DOB/Age are not blank for this set of controls, add this person to the family list.
                if ( FirstName.Text != "" && LastName.Text != "" && DOBAge.Text != "" )
                {
                    // if this is the first person then we can assume that we're going to need a family group and add it here.
                    if (i == 1)
                    {
                        Group group = new Group();
                        GroupService gs = new GroupService();
                        group.IsSystem = false;
                        group.GroupTypeId = new GroupTypeService().Get( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ) ).Id;
                        group.Name = LastName.Text + " Family";
                        group.IsSecurityRole = false;
                        group.IsActive = true;
                        Rock.Data.RockTransactionScope.WrapTransaction( () =>
                        {
                            gs.Add( group, CurrentPersonId );
                            gs.Save( group, CurrentPersonId );
                        } );
                        familyGroupId = group.Id;
                        familyGroup = group;
                    }

                    Person person = new Person();
                    person.GivenName = FirstName.Text;
                    person.LastName = LastName.Text;
                    if ( DOBAge.Text.Length <= 3 )
                    {
                        // person.Age = DOBAge.Text;
                    }
                    else
                    {
                        person.BirthDate = Convert.ToDateTime(DOBAge.Text);
                    }
                    DateTime transitionDate;
                    var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();
                    if ( !string.IsNullOrEmpty( Grade.Text ) )
                    {
                        if ( DateTime.TryParse( globalAttributes.GetValue( "GradeTransitionDate" ), out transitionDate ) )
                        {
                            int yearsLeft;
                            if ( DateTime.Now < transitionDate )
                            {
                                yearsLeft = 12 - int.Parse( Grade.Text );
                            }
                            else
                            {
                                yearsLeft = 13 - int.Parse( Grade.Text );
                            }
                            person.GraduationDate = Convert.ToDateTime( transitionDate.Month.ToString() + "/" + transitionDate.Day.ToString() + "/" + ( DateTime.Now.Year + yearsLeft ).ToString() );
                        }
                        else
                        {
                            person.GraduationDate = null;
                        }
                    }
                    else
                    {
                        person.GraduationDate = null;
                    }

                    // let's save the new person right here.
                    PersonService ps = new PersonService();
                    Rock.Data.RockTransactionScope.WrapTransaction( () =>
                    {
                        ps.Add( person, CurrentPersonId );
                        ps.Save( person, CurrentPersonId );
                    } );

                    // Add the Person's GroupMember data so that they can be part of the family
                    GroupMember gm = new GroupMember();
                    GroupMemberService gms = new GroupMemberService();
                    gm.IsSystem = false;
                    gm.GroupId = familyGroupId;
                    gm.PersonId = person.Id;
                    int age = 0;
                    if ( person.BirthDate == null )
                    {
                        // set the age to the age from the date picker
                        age = int.Parse( DOBAge.Text );
                    }
                    else
                    {
                        age = int.Parse(person.Age.ToString());
                    }
                    if ( age >= 18)
                    {
                        gm.GroupRoleId = new GroupRoleService().Get( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ).Id;
                    }
                    else
                    {
                        gm.GroupRoleId = new GroupRoleService().Get( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD ) ).Id;
                    }
                    Rock.Data.RockTransactionScope.WrapTransaction( () =>
                    {
                        gms.Add( gm, CurrentPersonId );
                        gms.Save( gm, CurrentPersonId );
                    } );

                    FamilyList.Add( person );
                }
                
            }

            if ( FamilyList.Count == 0 )
            {
                return;
            }

            CheckInFamily CIF = new CheckInFamily();

            // For a family, we need a Group for them. Each person needs to have a GroupMember record (which ties them back to the family group and adds their role) as well as their Person record.
            CIF.Group = familyGroup;

            string subCaption = "";
            foreach(Person person in FamilyList)
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
            CIF.Caption = FamilyList.FirstOrDefault().LastName;
            CIF.SubCaption = subCaption;
            CIF.Selected = true;

            CurrentCheckInState.CheckIn.Families.Clear();
            CurrentCheckInState.CheckIn.Families.Add( CIF );
            lvFamily.DataSource = CurrentCheckInState.CheckIn.Families;
            lvFamily.DataBind();
            rPerson.DataSource = CIF.People;
            rPerson.DataBind();
            foreach ( ListViewDataItem li in lvFamily.Items )
            {
                ( (LinkButton)li.FindControl( "lbSelectFamily" ) ).AddCssClass( "active" );
                
            }
            foreach ( RepeaterItem item in rPerson.Items )
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
            System.Data.DataTable dt = LoadAllThePeople();
            gridPersonSearchResults.DataSource = dt;
            gridPersonSearchResults.PageIndex = 0;
            gridPersonSearchResults.DataBind();
            gridPersonSearchResults.Visible = true;

            lbAddSearchedForPerson.Visible = true;

            mpePerson.Show();
        }

        /// <summary>
        /// Loads all the people.
        /// </summary>
        /// <returns></returns>
        protected System.Data.DataTable LoadAllThePeople()
        {
            var personService = new PersonService();
            IEnumerable<Person> person = Enumerable.Empty<Person>();

            if ( !string.IsNullOrEmpty( tbFirstNameSearch.Text ) && !string.IsNullOrEmpty( tbLastNameSearch.Text ) )
            {
                person = personService.GetByFullName( tbFirstNameSearch.Text + " " + tbLastNameSearch.Text );
            }
            else if ( !string.IsNullOrEmpty( tbFirstNameSearch.Text ) )
            {
                person = personService.GetByFirstName( tbFirstNameSearch.Text );
            }
            else if ( !string.IsNullOrEmpty( tbLastNameSearch.Text ) )
            {
                person = personService.GetByLastName( tbLastNameSearch.Text );
            }

            if ( !string.IsNullOrEmpty( dpDOBAgeSearch.Text ) )
            {
                if ( dpDOBAgeSearch.Text.Length <= 3 )
                {
                    person = person.Where( p => p.Age == int.Parse( dpDOBAgeSearch.Text ) );
                }
                else
                {
                    DateTime someDate;
                    if ( DateTime.TryParse( dpDOBAgeSearch.Text, out someDate ) )
                    {
                        string[] dateInfo = dpDOBAgeSearch.Text.Split( new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries );
                        if ( dateInfo.Count() == 3 )
                        {
                            person = person.Where( p => p.BirthDate == Convert.ToDateTime( dpDOBAgeSearch.Text ) );
                        }
                        else if ( dateInfo.Count() == 2 )
                        {
                            if ( dateInfo[1].Length == 2 )
                            {
                                dateInfo[1] = System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.ToFourDigitYear( int.Parse( dateInfo[1] ) ).ToString();
                            }
                            else if ( dateInfo[1].Length == 1 )
                            {
                                dateInfo[1] = System.Threading.Thread.CurrentThread.CurrentCulture.Calendar.ToFourDigitYear( int.Parse( "0" + dateInfo[1] ) ).ToString();
                            }
                            person = person.Where( p => p.BirthMonth == int.Parse( dateInfo[0] ) && p.BirthYear == int.Parse( dateInfo[1] ) );
                        }
                    }
                }

            }

            if ( !string.IsNullOrEmpty( tbGradeSearch.Text ) )
            {
                person = person.Where( p => p.Grade == int.Parse( tbGradeSearch.Text ) );
            }

            System.Data.DataTable dt = new System.Data.DataTable();

            var column = new System.Data.DataColumn();
            column.DataType = System.Type.GetType( "System.String" );
            column.ColumnName = "ThePersonsId";
            column.ReadOnly = true;
            dt.Columns.Add( column );

            column = new System.Data.DataColumn();
            column.DataType = System.Type.GetType( "System.String" );
            column.ColumnName = "ThePersonsFirstName";
            column.ReadOnly = false;
            dt.Columns.Add( column );

            column = new System.Data.DataColumn();
            column.DataType = System.Type.GetType( "System.String" );
            column.ColumnName = "ThePersonsLastName";
            column.ReadOnly = false;
            dt.Columns.Add( column );

            column = new System.Data.DataColumn();
            column.DataType = System.Type.GetType( "System.String" );
            column.ColumnName = "ThePersonsDOB";
            column.ReadOnly = false;
            dt.Columns.Add( column );

            column = new System.Data.DataColumn();
            column.DataType = System.Type.GetType( "System.String" );
            column.ColumnName = "ThePersonsGrade";
            column.ReadOnly = false;
            dt.Columns.Add( column );

            foreach ( var p in person )
            {
                System.Data.DataRow row;
                row = dt.NewRow();
                row["ThePersonsId"] = p.Id;
                row["ThePersonsFirstName"] = p.FirstName;
                row["ThePersonsLastName"] = p.LastName;
                row["ThePersonsDOB"] = Convert.ToDateTime( p.BirthDate ).ToString( "d" );
                row["ThePersonsGrade"] = p.Grade;
                dt.Rows.Add( row );
            }

            System.Data.DataView dv = new System.Data.DataView( dt );
            dv.Sort = "ThePersonsLastName ASC, ThePersonsFirstName ASC";
            System.Data.DataTable dt2 = dv.ToTable();
            return dt2;
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
            mpe.Show();
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
            mpe.Show();
        }

        /// <summary>
        /// Handles the ServerValidate event of the cvDOBAgeValidator control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="ServerValidateEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Handles the RowCommand event of the gridPersonSearchResults control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewCommandEventArgs"/> instance containing the event data.</param>
        protected void gridPersonSearchResults_RowCommand( object sender, GridViewCommandEventArgs e )
        {
            if ( e.CommandName == "Add" )
            {
                int index = int.Parse( e.CommandArgument.ToString() );
                GridViewRow row = gridPersonSearchResults.Rows[index];
                int personId = int.Parse(row.Cells[0].Text);
                PersonService personService = new PersonService();
                Person person = personService.Get( personId );
                var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                if ( family == null )
                {
                    family = new CheckInFamily();
                    foreach( var group in person.Members.Where( m => m.Group.GroupType.Guid == new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ) ).Select( m => m.Group ) )
                    {
                        family = new CheckInFamily();
                        family.Group = group.Clone( false );
                        family.Group.LoadAttributes();
                        family.Caption = group.ToString();
                        GroupMemberService gms = new GroupMemberService();
                        family.SubCaption = gms.GetFirstNames( group.Id ).ToList().AsDelimited( ", " );
                        family.Selected = true;
                        CurrentCheckInState.CheckIn.Families.Clear();
                        CurrentCheckInState.CheckIn.Families.Add( family );
                        lvFamily.DataSource = CurrentCheckInState.CheckIn.Families;
                        lvFamily.DataBind();
                        foreach ( ListViewDataItem li in lvFamily.Items )
                        {
                            ( (LinkButton)li.FindControl( "lbSelectFamily" ) ).AddCssClass( "active" );

                        }
                    }
                }
                CheckInPerson CIP = new CheckInPerson();
                CIP.Person = person;
                CIP.Selected = true;
                if ( PersonVisitorType.Value == "Person" )
                {
                    // this came from Add Person
                    var isPersonInFamily = false;
                    foreach ( var familyPerson in family.People )
                    {
                        if ( familyPerson.Person.Id == person.Id )
                        {
                            isPersonInFamily = true;
                        }
                    }
                    // only add the person to the family if they aren't listed as part of the family.
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

                        family.People.Add( CIP );
                        rPerson.DataSource = family.People;
                        rPerson.DataBind();
                        foreach ( RepeaterItem item in rPerson.Items )
                        {
                            ( (LinkButton)item.FindControl( "lbSelectPerson" ) ).AddCssClass( "active" );
                        }
                    }
                }
                else
                {
                    // this came from Add Visitor
                    // Add CanCheckIn record to GroupMember
                    Person.CreateCheckinRelationship( family.People.FirstOrDefault().Person.Id, person.Id, CurrentPersonId );
                    //GroupMember gm = new GroupMember();
                    //GroupMemberService gms = new GroupMemberService();
                    //gm.IsSystem = false;
                    //gm.GroupId = family.Group.Id;
                    //gm.PersonId = person.Id;
                    //gm.GroupRoleId = new GroupRoleService().Get( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN ) ).Id;
                    //Rock.Data.RockTransactionScope.WrapTransaction( () =>
                    //{
                    //    gms.Add( gm, CurrentPersonId );
                    //    gms.Save( gm, CurrentPersonId );
                    //} );

                    family.People.Add( CIP );

                    // load the visitors list
                    List<CheckInPerson> visitors = new List<CheckInPerson>();

                    // need to get the people that have the CanCheckIn GroupRole on a GroupMember record associated with the selected Family Group.
                    GroupMemberService gms = new GroupMemberService();
                    //var knownRelationshipGroup = gms.Queryable()
                    //    .Where( m =>
                    //        m.PersonId == personId &&
                    //        m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER ) ) )
                    //    .Select( m => m.Group )
                    //    .FirstOrDefault();
                    var canCheckInGroup = gms.Queryable()
                        .Where( m =>
                            m.PersonId == personId &&
                            m.GroupRole.Guid.Equals( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN ) ) )
                        .Select( m => m.Group )
                        .FirstOrDefault();
                    var groupMembers = gms.GetByGroupId( canCheckInGroup.Id );
                    foreach (var groupMember in groupMembers)
                    {
                        if ( groupMember.GroupRoleId == new GroupRoleService().Get( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN ) ).Id )
                        {
                            CheckInPerson checkInPerson = new CheckInPerson();
                            Person per = new PersonService().Get(groupMember.PersonId);
                            checkInPerson.Person = per;
                            visitors.Add( checkInPerson );
                        }
                    }
                    rVisitors.DataSource = visitors;
                    rVisitors.DataBind();

                    // make sure the one person you just added is selected
                    foreach ( RepeaterItem item in rVisitors.Items )
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
                System.Data.DataTable dt = LoadAllThePeople();
                gridPersonSearchResults.DataSource = dt;
                gridPersonSearchResults.DataBind();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddSearchedForPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddSearchedForPerson_Click( object sender, EventArgs e )
        {
            if ( string.IsNullOrEmpty( tbFirstNameSearch.Text ) || string.IsNullOrEmpty( tbLastNameSearch.Text ) || string.IsNullOrEmpty( dpDOBAgeSearch.Text ) )
            {
                mpePerson.Show();
                string errorMsg = "<ul><li>You have to fill out the First Name, Last Name, and DOB fields first.</li></ul>";
                AddPersonAlert.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
            }
            else
            {
                Person person = new Person();
                person.GivenName = tbFirstNameSearch.Text;
                person.LastName = tbLastNameSearch.Text;
                if ( dpDOBAgeSearch.Text.Length <= 3 )
                {
                    //person.Age = dpDOBAgeSearch.Text;
                }
                else
                {
                    person.BirthDate = Convert.ToDateTime( dpDOBAgeSearch.Text );
                }
                DateTime transitionDate;
                var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();
                if ( !string.IsNullOrEmpty( tbGradeSearch.Text ) )
                {
                    if ( DateTime.TryParse( globalAttributes.GetValue( "GradeTransitionDate" ), out transitionDate ) )
                    {
                        int yearsLeft;
                        if ( DateTime.Now < transitionDate )
                        {
                            yearsLeft = 12 - int.Parse( tbGradeSearch.Text );
                        }
                        else
                        {
                            yearsLeft = 13 - int.Parse( tbGradeSearch.Text );
                        }
                        person.GraduationDate = Convert.ToDateTime( transitionDate.Month.ToString() + "/" + transitionDate.Day.ToString() + "/" + ( DateTime.Now.Year + yearsLeft ).ToString() );
                    }
                    else
                    {
                        person.GraduationDate = null;
                    }
                }
                else
                {
                    person.GraduationDate = null;
                }

                // let's save the new person right here.
                PersonService ps = new PersonService();
                Rock.Data.RockTransactionScope.WrapTransaction( () =>
                {
                    ps.Add( person, CurrentPersonId );
                    ps.Save( person, CurrentPersonId );
                } );

                var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                if ( family == null )
                {
                    // There isn't a current family
                    family = new CheckInFamily();
                    family.Selected = true;
                    CurrentCheckInState.CheckIn.Families.Add( family );

                    // Add a Group for the new family
                    Group group = new Group();
                    GroupService gs = new GroupService();
                    group.IsSystem = false;
                    group.GroupTypeId = new GroupTypeService().Get( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ) ).Id;
                    group.Name = person.LastName + " Family";
                    group.IsSecurityRole = false;
                    group.IsActive = true;
                    Rock.Data.RockTransactionScope.WrapTransaction( () =>
                    {
                        gs.Add( group, CurrentPersonId );
                        gs.Save( group, CurrentPersonId );
                    } );
                    family.Group = group;
                }

                // put the person into place to be checked in.
                CheckInPerson CIP = new CheckInPerson();
                CIP.Person = person;
                CIP.Selected = true;
                family.People.Add( CIP );
                
                // Add the Person's GroupMember data so that they can be part of the family
                GroupMember gm = new GroupMember();
                GroupMemberService gms = new GroupMemberService();
                gm.IsSystem = false;
                gm.GroupId = family.Group.Id;
                gm.PersonId = person.Id;
                int age = 0;
                if ( person.BirthDate == null )
                {
                    // set the age to the age from the date picker
                    age = int.Parse( dpDOBAgeSearch.Text );
                }
                else
                {
                    age = int.Parse( person.Age.ToString() );
                }
                if ( age >= 18 )
                {
                    gm.GroupRoleId = new GroupRoleService().Get( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ).Id;
                }
                else
                {
                    gm.GroupRoleId = new GroupRoleService().Get( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD ) ).Id;
                }
                Rock.Data.RockTransactionScope.WrapTransaction( () =>
                {
                    gms.Add( gm, CurrentPersonId );
                    gms.Save( gm, CurrentPersonId );
                } );

                SaveState();

                rPerson.DataSource = family.People;
                rPerson.DataBind();
                foreach ( RepeaterItem item in rPerson.Items )
                {
                    ( (LinkButton)item.FindControl( "lbSelectPerson" ) ).AddCssClass( "active" );
                }
            }
        }

        #endregion

        #region Internal Methods 

        /// <summary>
        /// Goes the back.
        /// </summary>
        private void GoBack()
        {
            CurrentCheckInState.CheckIn.SearchType = null;
            CurrentCheckInState.CheckIn.SearchValue = string.Empty;
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
            var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
            if ( family != null )
            {
                if ( family.People.Count == 1 )
                {
                    if ( UserBackedUp )
                    {
                        GoBack();
                    }                    
                }
                else
                {
                    foreach ( var familyMember in family.People )
                    {
                        familyMember.Selected = true;
                    }
                }

                rPerson.DataSource = family.People;
                rPerson.DataBind();
            }
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