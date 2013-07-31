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
//using Rock.ExtensionMethods;
using Rock.Model;
using Rock.Web.Cache;
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
                    if ( CurrentCheckInState.CheckIn.Families.Count == 0 )
                    {
                        familyTitle.InnerText = "No Search Results";
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
        /// Handles the ItemDataBound event of the repPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void repPerson_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            hfSelectedPerson.Value = "";
            var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
            var familyMemberIds = family.People.Where( p => p.Selected && p.FamilyMember ).Select( p => p.Person.Id ).ToList();

            if ( familyMemberIds.Count > 0 )
            {
                if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
                {
                    if ( familyMemberIds.Contains( ( (CheckInPerson)e.Item.DataItem ).Person.Id ) )
                    {
                        hfSelectedPerson.Value += ( (CheckInPerson)e.Item.DataItem ).Person.Id + ",";
                        ( (LinkButton)e.Item.FindControl( "lbSelectPerson" ) ).AddCssClass( "active" );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the repVisitors control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void repVisitors_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            hfSelectedVisitor.Value = "";
            var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
            var visitorIds = family.People.Where( p => p.Selected && !p.FamilyMember ).Select( p => p.Person.Id ).ToList();

            if ( visitorIds.Count > 0 )
            {
                if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
                {
                    if ( visitorIds.Contains( ( (CheckInPerson)e.Item.DataItem ).Person.Id ) )
                    {
                        hfSelectedVisitor.Value += ( (CheckInPerson)e.Item.DataItem ).Person.Id + ",";
                        ( (LinkButton)e.Item.FindControl( "lbSelectVisitor" ) ).AddCssClass( "active" );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the repAddFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void repAddFamily_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var ddlGender = ( (DropDownList)e.Item.FindControl( "ddlGender" ) );
                ddlGender.BindToEnum( typeof( Gender ), true );

                var ddlAbilityGrade = ( (DropDownList)e.Item.FindControl( "ddlAbilityGrade" ) );

                List<AttributeValue> abilityList;
                var ability = new AttributeService().GetGlobalAttribute( "AbilityLevel" );
                if (ability != null )
                {
                    abilityList = new AttributeValueService().GetByAttributeId( ability.Id ).ToList();
                }
                
                var gradeList = Enum.GetValues( typeof( GradeLevel ) ).Cast<GradeLevel>()
                    .Select( gl => gl.GetDescription() );

                var bothList = new { Group = "Ability", Name = "Cuddler", ID = 1 };

                ddlAbilityGrade.DataSource = bothList;
                ddlAbilityGrade.DataValueField = bothList.ID.ToString();
                //ddlAbilityGrade.DataGroupField = bothList.Group;
                ddlAbilityGrade.DataTextField = bothList.Name;
                ddlAbilityGrade.DataBind();                
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
            repAddFamily.DataSource = Enumerable.Repeat( new Person(), 5 );
            repAddFamily.DataBind();
            mpeAddFamily.Show();
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
            var checkInFamily = new CheckInFamily();
            var familyGroup = new Group();            

            foreach ( RepeaterItem item in repAddFamily.Items )
            {
                var givenName = ( (TextBox)item.FindControl( "tbFirstName" ) ).Text;
                var lastName = ( (TextBox)item.FindControl( "tbLastName" ) ).Text;
                var birthDate = ( (DatePicker)item.FindControl( "dpBirthDate" ) ).SelectedDate;
                var gender = ( (DropDownList)item.FindControl( "ddlGender" ) ).SelectedValueAsInt();
                var abilityGrade = ( (DropDownList)item.FindControl( "ddlAbilityGrade" ) ).SelectedValue;

                if ( givenName != string.Empty && lastName != string.Empty && birthDate.HasValue
                    && gender.HasValue && abilityGrade != string.Empty )
                {
                    var newPerson = AddPerson( givenName, lastName, birthDate, abilityGrade );

                    if ( familyGroup != null )
                    {
                        AddGroupMember( familyGroup, newPerson );
                        var checkInPerson = new CheckInPerson();
                        checkInPerson.Person = newPerson;
                        checkInPerson.Selected = true;
                        checkInFamily.People.Add( checkInPerson );
                    }
                    else
                    {
                        var gs = new GroupService();
                        familyGroup = new Group();
                        familyGroup.IsSystem = false;
                        familyGroup.GroupTypeId = new GroupTypeService().Get( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ) ).Id;
                        familyGroup.Name = lastName + " Family";
                        familyGroup.IsSecurityRole = false;
                        familyGroup.IsActive = true;
                        Rock.Data.RockTransactionScope.WrapTransaction( () =>
                        {
                            gs.Add( familyGroup, CurrentPersonId );
                            gs.Save( familyGroup, CurrentPersonId );
                        } );

                        AddGroupMember( familyGroup, newPerson );
                    }
                }
            }

            checkInFamily.Group = familyGroup;
            //checkInFamily.Caption = familyGroup.Members.Select
            checkInFamily.SubCaption = familyGroup.Members.SelectMany( gm => gm.Person.FirstName ).ToString();
            checkInFamily.Selected = true;

            // don't clear in case there are several "smith" families
            // CurrentCheckInState.CheckIn.Families.Clear();
            CurrentCheckInState.CheckIn.Families.Add( checkInFamily );
            lvFamily.DataSource = CurrentCheckInState.CheckIn.Families;
            lvFamily.DataBind();

            repPerson.DataSource = checkInFamily.People;
            repPerson.DataBind();

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
            
            //PreviousButton.Visible = false;
            
            //MoreButton.Visible = true;

            //mpeAddFamily.Show();
        }

        /// <summary>
        /// Handles the Click event of the MoreButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void MoreButton_Click( object sender, EventArgs e )
        {
            
            //PreviousButton.Visible = true;
            
            //MoreButton.Visible = false;
            
            //mpeAddFamily.Show();
        }

        /// <summary>
        /// Handles the ServerValidate event of the cvDOBValidator control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="ServerValidateEventArgs"/> instance containing the event data.</param>
        protected void cvBirthDateValidator_ServerValidate( object source, ServerValidateEventArgs args )
        {
            args.IsValid = false;
            DateTime someDate;            
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
                GroupMemberService groupMemberService = new GroupMemberService();
                int index = int.Parse( e.CommandArgument.ToString() );
                int personId = int.Parse( grdPersonSearchResults.DataKeys[index].Value.ToString() );

                var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                if ( family != null )
                {                    
                    var checkInPerson = new CheckInPerson();
                    checkInPerson.Person = new PersonService().Get( personId );
                    checkInPerson.Selected = true;
                    if ( personVisitorType.Value == "Person" )
                    {
                        // this came from Add Person
                        var isPersonInFamily = family.People.Any( p => p.Person.Id == checkInPerson.Person.Id );
                        if ( !isPersonInFamily )
                        {
                            // because this person is being added to this family, we should make sure his/her groupmember record reflects that.                            
                            var groupMember = groupMemberService.GetByPersonId( personId ).FirstOrDefault();
                            groupMember.GroupId = family.Group.Id;
                            Rock.Data.RockTransactionScope.WrapTransaction( () =>
                            {
                                groupMemberService.Save( groupMember, CurrentPersonId );
                            } );

                            checkInPerson.FamilyMember = true;
                        }
                    }
                    else
                    {
                        // this came from Add Visitor
                        AddVisitorGroupMemberRoles( family, personId );

                        // add the visitor to the checkin group
                        checkInPerson.FamilyMember = false;

                    }
                    
                    family.People.Add( checkInPerson );
                    repPerson.DataSource = family.People.Where( p => p.FamilyMember );
                    repPerson.DataBind();

                    repVisitors.DataSource = family.People.Where( p => !p.FamilyMember );
                    repVisitors.DataBind();

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
                var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                if ( family != null )
                {
                    // Add the Person record.
                    var person = AddPerson( tbFirstNameSearch.Text, tbLastNameSearch.Text, dtpDOBSearch.SelectedDate, tbGradeSearch.Text );

                    // put the person into place to be checked in.
                    var checkInPerson = new CheckInPerson();
                    checkInPerson.Person = person;
                    checkInPerson.Selected = true;

                    if ( personVisitorType.Value == "Person" )
                    {
                        // Add the Person's GroupMember data so that they can be part of the family
                        var groupMember = AddGroupMember( family.Group, person );
                        checkInPerson.FamilyMember = true;
                    }
                    else
                    {
                        // This is a visitor
                        AddVisitorGroupMemberRoles( family, person.Id );
                        //Person.CreateCheckinRelationship( family.People.FirstOrDefault().Person.Id, person.Id, CurrentPersonId );
                        checkInPerson.FamilyMember = false;
                    }
                    family.People.Add( checkInPerson );
                    repPerson.DataSource = family.People.Where( p => p.FamilyMember );
                    repPerson.DataBind();

                    repVisitors.DataSource = family.People.Where( p => !p.FamilyMember );
                    repVisitors.DataBind();

                    SaveState();
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

            family.People.Clear();
            if ( !string.IsNullOrEmpty( hfSelectedPerson.Value ) )
            {
                var selectedFamilyMembers = hfSelectedPerson.Value.SplitDelimitedValues().Select( int.Parse ).ToList();
                foreach( var familyMember in selectedFamilyMembers)
                {
                    var personToAdd = new PersonService().Get( familyMember );
                    var checkInPerson = new CheckInPerson();
                    checkInPerson.Person = personToAdd;
                    checkInPerson.Selected = true;
                    checkInPerson.FamilyMember = false;
                    family.People.Add( checkInPerson );
                }
            }

            if ( !string.IsNullOrEmpty( hfSelectedVisitor.Value ) )
            {
                var selectedVisitors = hfSelectedVisitor.Value.SplitDelimitedValues().Select( int.Parse ).ToList();
                foreach ( var visitor in selectedVisitors )
                {
                    var personToAdd = new PersonService().Get( visitor );
                    var checkInPerson = new CheckInPerson();
                    checkInPerson.Person = personToAdd;
                    checkInPerson.Selected = true;
                    checkInPerson.FamilyMember = false;
                    family.People.Add( checkInPerson );
                }
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
        //protected Group AddFamilyGroup( string familyLastName )
        //{
        //    Group group = new Group();
        //    GroupService gs = new GroupService();
        //    group.IsSystem = false;
        //    group.GroupTypeId = new GroupTypeService().Get( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY ) ).Id;
        //    group.Name = familyLastName + " Family";
        //    group.IsSecurityRole = false;
        //    group.IsActive = true;
        //    Rock.Data.RockTransactionScope.WrapTransaction( () =>
        //    {
        //        gs.Add( group, CurrentPersonId );
        //        gs.Save( group, CurrentPersonId );
        //    } );

        //    return group;
        //}

        /// <summary>
        /// Adds a new person.
        /// </summary>
        /// <param name="firstName">The first name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="DOB">The DOB.</param>
        /// <param name="grade">The grade.</param>
        protected Person AddPerson( string firstName, string lastName, DateTime? dob, string grade )
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
        /// Adds the visitor group member roles.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <param name="personId">The person id.</param>
        protected void AddVisitorGroupMemberRoles( CheckInFamily family, int personId )
        {
            GroupMemberService groupMemberService = new GroupMemberService();
            GroupRoleService groupRoleService = new GroupRoleService();
            int ownerRoleId = groupRoleService.Get( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER ) ).Id;
            int canCheckInId = groupRoleService.Get( new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN ) ).Id;
            foreach ( var familyMember in family.People )
            {
                var group = groupMemberService.Queryable()
                .Where( m =>
                    m.PersonId == familyMember.Person.Id &&
                    m.GroupRoleId == ownerRoleId )
                .Select( m => m.Group )
                .FirstOrDefault();

                if ( group == null )
                {
                    var role = new GroupRoleService().Get( ownerRoleId );
                    if ( role != null && role.GroupTypeId.HasValue )
                    {
                        var groupMember = new GroupMember();
                        groupMember.PersonId = familyMember.Person.Id;
                        groupMember.GroupRoleId = role.Id;

                        group = new Group();
                        group.Name = role.GroupType.Name;
                        group.GroupTypeId = role.GroupTypeId.Value;
                        group.Members.Add( groupMember );

                        var groupService = new GroupService();
                        groupService.Add( group, CurrentPersonId );
                        groupService.Save( group, CurrentPersonId );
                    }
                }

                // add the visitor to this group with CanCheckIn
                Person.CreateCheckinRelationship( familyMember.Person.Id, personId, CurrentPersonId );
            }
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

        #endregion        
    }
}