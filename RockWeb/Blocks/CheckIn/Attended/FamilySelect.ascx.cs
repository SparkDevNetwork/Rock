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
                    if ( CurrentCheckInState.CheckIn.Families.Count > 0 )
                    {
                        var familyList = CurrentCheckInState.CheckIn.Families.OrderBy( f => f.Caption ).ToList();
                        familyList.First().Selected = true;
                        lvFamily.DataSource = familyList;
                        lvFamily.DataBind();
                        ProcessFamily();
                    }
                    else
                    {
                        familyTitle.InnerText = "No Search Results";
                        lbNext.Enabled = false;
                        lbNext.Visible = false;
                        pnlSelectFamily.Visible = false;
                        pnlSelectPerson.Visible = false;
                        pnlSelectVisitor.Visible = false;
                        actions.Visible = false;
                        divNothingFound.Visible = true;
                    }
                }
            }
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the ItemCommand event of the lvFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewCommandEventArgs"/> instance containing the event data.</param>
        protected void lvFamily_ItemCommand( object sender, ListViewCommandEventArgs e )
        {
            int id = int.Parse( e.CommandArgument.ToString() );
            var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Group.Id == id ).FirstOrDefault();
            
            if ( !family.Selected )
            {                    
                CurrentCheckInState.CheckIn.Families.ForEach( f => f.Selected = false );
                family.Selected = true;
                ProcessFamily();

                foreach ( ListViewDataItem li in lvFamily.Items )
                {
                    ( (LinkButton)li.FindControl( "lbSelectFamily" ) ).RemoveCssClass( "active" );
                }
                ( (LinkButton)e.Item.FindControl( "lbSelectFamily" ) ).AddCssClass( "active" );
            }
            else
            {
                repPerson.DataSource = null;
                repPerson.DataBind();
                pnlSelectPerson.Update();
                family.Selected = false;                    
                foreach ( ListViewDataItem li in lvFamily.Items )
                {
                    ( (LinkButton)li.FindControl( "lbSelectFamily" ) ).RemoveCssClass( "active" );
                }
            }            
        }

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
            lvFamily.DataSource = CurrentCheckInState.CheckIn.Families.OrderBy( f => f.Caption ).ToList();
            lvFamily.DataBind();
            pnlSelectFamily.Update();
        }

        /// <summary>
        /// Handles the DataBound event of the lvFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lvFamily_ItemDataBound( object sender, ListViewItemEventArgs e )
        {
            if ( e.Item.ItemType == ListViewItemType.DataItem )
            {
                if ( ( (CheckInFamily)e.Item.DataItem ).Selected )
                {
                    ( (LinkButton)e.Item.FindControl( "lbSelectFamily" ) ).AddCssClass( "active" );
                }
            }            
        }

        /// <summary>
        /// Handles the ItemDataBound event of the repPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void repPerson_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var familyMemberIds = hfSelectedPerson.Value.SplitDelimitedValues().Select( int.Parse ).ToList();
            if ( familyMemberIds.Count > 0 )
            {
                if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
                {
                    if ( familyMemberIds.Contains( ( (CheckInPerson)e.Item.DataItem ).Person.Id ) )
                    {
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
            var visitorIds = hfSelectedVisitor.Value.SplitDelimitedValues().Select( int.Parse ).ToList();

            if ( visitorIds.Count > 0 )
            {
                if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
                {
                    if ( visitorIds.Contains( ( (CheckInPerson)e.Item.DataItem ).Person.Id ) )
                    {
                        ( (LinkButton)e.Item.FindControl( "lbSelectVisitor" ) ).AddCssClass( "active" );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the DataBound event of the lvAddFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lvAddFamily_DataBound( object sender, EventArgs e )
        {
            foreach ( var item in lvAddFamily.Items )
            {
                var ddlGender = ( (DropDownList)item.FindControl( "ddlGender" ) );
                ddlGender.BindToEnum( typeof( Gender ), true );

                var ddlAbilityGrade = ( (RockDropDownList)item.FindControl( "ddlAbilityGrade" ) );
                ddlAbilityGrade.DataSource = GetAbilityGradeList();
                ddlAbilityGrade.DataBind();
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the lvAddFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewItemEventArgs"/> instance containing the event data.</param>
        protected void lvAddFamily_ItemDataBound( object sender, ListViewItemEventArgs e )
        {
            //if ( e.Item.ItemType == ListViewItemType.DataItem )
            //{
            //    var firstName = ( (TextBox)e.Item.FindControl( "tbFirstName" ) );
            //    var lastName = ( (TextBox)e.Item.FindControl( "tbLastName" ) );
            //    var birthDate = ( (DatePicker)e.Item.FindControl( "dpBirthDate" ) );
            //    var ddlGender = ( (DropDownList)e.Item.FindControl( "ddlGender" ) );
            //    var ddlAbilityGrade = ( (RockDropDownList)e.Item.FindControl( "ddlAbilityGrade" ) );
                
            //    ddlGender.BindToEnum( typeof( Gender ), true );
            //    ddlAbilityGrade.DataSource = GetAbilityGradeList();
            //    ddlAbilityGrade.DataBind();
            //}
        }

        /// <summary>
        /// Handles the PagePropertiesChanging event of the lvAddFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PagePropertiesChangingEventArgs"/> instance containing the event data.</param>
        protected void lvAddFamily_PagePropertiesChanging( object sender, PagePropertiesChangingEventArgs e )
        {
            var newFamilyList = new System.Data.DataTable();
            if ( ViewState["newFamily"] != null )
            {
                newFamilyList = (System.Data.DataTable)ViewState["newFamily"];
                foreach ( ListViewItem item in lvAddFamily.Items )
                {
                    var givenName = ( (TextBox)item.FindControl( "tbFirstName" ) ).Text;
                    var lastName = ( (TextBox)item.FindControl( "tbLastName" ) ).Text;
                    var birthDate = ( (DatePicker)item.FindControl( "dpBirthDate" ) ).SelectedDate;
                    var gender = ( (DropDownList)item.FindControl( "ddlGender" ) ).SelectedValueAsInt();
                    var abilityGrade = ( (DropDownList)item.FindControl( "ddlAbilityGrade" ) ).SelectedValue;

                    newFamilyList.Rows.Add( givenName, lastName, birthDate, gender, abilityGrade );
                }
            }

            dpAddFamily.SetPageProperties( e.StartRowIndex, e.MaximumRows, false );
            lvAddFamily.DataSource = newFamilyList;
            lvAddFamily.DataBind();
            mpeAddFamily.Show();
        }

        /// <summary>
        /// Handles the Click event of the lbAddPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddPerson_Click( object sender, EventArgs e )
        {           
            SetAddPersonFields();
            lblAddPersonHeader.Text = "Add Person";
            personVisitorType.Value = "Person";
            tbFirstNameSearch.Focus();
            mpeAddPerson.Show();
        }

        /// <summary>
        /// Handles the Click event of the lbAddVisitor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddVisitor_Click( object sender, EventArgs e)
        {
            SetAddPersonFields();
            lblAddPersonHeader.Text = "Add Visitor";
            personVisitorType.Value = "Visitor";
            tbFirstNameSearch.Focus();
            mpeAddPerson.Show();
        }

        /// <summary>
        /// Sets the add person fields.
        /// </summary>
        protected void SetAddPersonFields()
        {
            tbFirstNameSearch.Text = "";
            tbLastNameSearch.Text = "";
            dpDOBSearch.Text = "";
            ddlGenderSearch.BindToEnum( typeof( Gender ) );
            ddlGenderSearch.SelectedIndex = 0;
            ddlAbilitySearch.DataSource = GetAbilityGradeList();
            ddlAbilitySearch.DataBind();
            ddlAbilitySearch.SelectedIndex = 0;
            rGridPersonResults.Visible = false;
            lbAddNewPerson.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the lbAddFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddFamily_Click( object sender, EventArgs e )
        {
            var newFamilyList = new System.Data.DataTable();
            newFamilyList.Columns.Add( "FirstName", typeof( System.String ) );
            newFamilyList.Columns.Add( "LastName", typeof( System.String ) );
            newFamilyList.Columns.Add( "BirthDate", typeof( DateTime ) );
            newFamilyList.Columns.Add( "Gender", typeof( Gender ) );
            newFamilyList.Columns.Add( "AbilityGrade", typeof( System.String ) );

            newFamilyList.Rows.Add( string.Empty, string.Empty, new DateTime(), Gender.Unknown, string.Empty );
            newFamilyList.Rows.Add( string.Empty, string.Empty, new DateTime(), Gender.Unknown, string.Empty );
            newFamilyList.Rows.Add( string.Empty, string.Empty, new DateTime(), Gender.Unknown, string.Empty );
            newFamilyList.Rows.Add( string.Empty, string.Empty, new DateTime(), Gender.Unknown, string.Empty );
            newFamilyList.Rows.Add( string.Empty, string.Empty, new DateTime(), Gender.Unknown, string.Empty );
            ViewState["newFamily"] = newFamilyList;
            lvAddFamily.DataSource = newFamilyList;
            lvAddFamily.DataBind();
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

            foreach ( ListViewItem item in lvAddFamily.Items )
            {
                var givenName = ( (TextBox)item.FindControl( "tbFirstName" ) ).Text;
                var lastName = ( (TextBox)item.FindControl( "tbLastName" ) ).Text;
                var birthDate = ( (DatePicker)item.FindControl( "dpBirthDate" ) ).SelectedDate;
                var gender = ( (DropDownList)item.FindControl( "ddlGender" ) ).SelectedValueAsInt();
                var abilityGrade = ( (DropDownList)item.FindControl( "ddlAbilityGrade" ) ).SelectedValue;
                var abilityGroup = ( (DropDownList)item.FindControl( "ddlAbilityGrade" ) ).SelectedItem.Attributes["optiongroup"];

                if ( givenName != string.Empty && lastName != string.Empty && birthDate.HasValue
                    && gender.HasValue && abilityGrade != string.Empty )
                {
                    var newPerson = CreatePerson( givenName, lastName, birthDate.ToString(), gender, abilityGrade, abilityGroup );

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
            checkInFamily.Caption = familyGroup.Name;
            checkInFamily.SubCaption = string.Join( ",", familyGroup.Members.Select( gm => gm.Person.FirstName ) );
            checkInFamily.Selected = true;

            // don't clear in case there are several "smith" families
            // CurrentCheckInState.CheckIn.Families.Clear();
            CurrentCheckInState.CheckIn.Families.Add( checkInFamily );
            lvFamily.DataSource = CurrentCheckInState.CheckIn.Families.OrderBy( f => f.Caption ).ToList(); ;
            lvFamily.DataBind();
            pnlSelectFamily.Update();

            repPerson.DataSource = checkInFamily.People.OrderBy( p => p.Person.FullNameLastFirst ).ToList();
            repPerson.DataBind();
            pnlSelectPerson.Update();

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
            var test = dpDOBSearch.SelectedDate;
            lbAddNewPerson.Visible = true;
            rGridPersonResults.PageIndex = 0;
            BindPersonGrid();
            rGridPersonResults.Visible = true;
            dpDOBSearch.SelectedDate = test;
            mpeAddPerson.Show();
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
        protected void rGridPersonResults_AddExistingPerson( object sender, GridViewCommandEventArgs e )
        {
            if ( e.CommandName == "Add" )
            {
                GroupMemberService groupMemberService = new GroupMemberService();
                int index = int.Parse( e.CommandArgument.ToString() );
                int personId = int.Parse( rGridPersonResults.DataKeys[index].Value.ToString() );

                var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                if ( family != null )
                {
                    var selectedPeopleList = ( hfSelectedPerson.Value + hfSelectedVisitor.Value ).SplitDelimitedValues().Select( int.Parse ).ToList();
                    var alreadyInList = selectedPeopleList.Contains( personId );
                    if ( !alreadyInList )
                    {
                        var checkInPerson = new CheckInPerson();
                        checkInPerson.Person = new PersonService().Get( personId ).Clone( false );
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
                                hfSelectedPerson.Value += personId + ",";
                            }
                        }
                        else
                        {
                            // this came from Add Visitor
                            AddVisitorGroupMemberRoles( family, personId );

                            // add the visitor to the checkin group
                            checkInPerson.FamilyMember = false;
                            hfSelectedVisitor.Value += personId + ",";

                        }

                        family.People.Add( checkInPerson );
                        repPerson.DataSource = family.People.Where( p => p.FamilyMember ).OrderBy( p => p.Person.FullNameLastFirst ).ToList();
                        repPerson.DataBind();
                        pnlSelectPerson.Update();

                        repVisitors.DataSource = family.People.Where( p => !p.FamilyMember ).OrderBy( p => p.Person.FullNameLastFirst ).ToList();
                        repVisitors.DataBind();
                        pnlSelectVisitor.Update();

                        SaveState();
                    }
                }
                else
                {
                    mpeAddPerson.Show();
                    string errorMsg = "<ul><li>You have to pick a family to add this person to.</li></ul>";
                    maAddPerson.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
                }
            }
            else
            {
                mpeAddPerson.Show();
                BindPersonGrid();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddSearchedForPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddNewPerson_Click( object sender, EventArgs e )
        {
            var test = dpDOBSearch.SelectedDate;

            if ( string.IsNullOrEmpty( tbFirstNameSearch.Text ) || string.IsNullOrEmpty( tbLastNameSearch.Text ) || string.IsNullOrEmpty( dpDOBSearch.Text ) || ddlGenderSearch.SelectedValueAsInt() == 0 )
            {
                mpeAddPerson.Show();
                string errorMsg = "<ul><li>Please fill out the First Name, Last Name, DOB, and Gender fields.</li></ul>";
                maAddPerson.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
            }
            else
            {
                var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                if ( family != null )
                {                    
                    var person = CreatePerson( tbFirstNameSearch.Text, tbLastNameSearch.Text, dpDOBSearch.Text, ddlGenderSearch.SelectedValueAsInt(), 
                        ddlAbilitySearch.SelectedValue, ddlAbilitySearch.SelectedItem.Attributes["optiongroup"] );

                    var checkInPerson = new CheckInPerson();
                    checkInPerson.Person = person.Clone( false );
                    checkInPerson.Selected = true;

                    if ( personVisitorType.Value == "Person" )
                    {   // Family Member
                        var groupMember = AddGroupMember( family.Group, person );
                        checkInPerson.FamilyMember = true;
                        hfSelectedPerson.Value += person.Id + ",";
                    }
                    else
                    {   // Visitor
                        AddVisitorGroupMemberRoles( family, person.Id );
                        checkInPerson.FamilyMember = false;
                        hfSelectedVisitor.Value += person.Id + ",";
                    }
                    
                    family.People.Add( checkInPerson );
                    repPerson.DataSource = family.People.Where( p => p.FamilyMember ).OrderBy( p => p.Person.FullNameLastFirst ).ToList();
                    repPerson.DataBind();
                    pnlSelectPerson.Update();

                    repVisitors.DataSource = family.People.Where( p => !p.FamilyMember ).OrderBy( p => p.Person.FullNameLastFirst ).ToList();
                    repVisitors.DataBind();
                    pnlSelectVisitor.Update();

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
            var selectedPeopleList = (hfSelectedPerson.Value + hfSelectedVisitor.Value).SplitDelimitedValues();
            var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
            if ( family == null )
            {
                maWarning.Show( "Please pick a family.", ModalAlertType.Warning );
                return;
            }

            if ( selectedPeopleList != null )
            {
                var selectedPeopleIds = selectedPeopleList.Select( int.Parse ).ToList();
                family.People.ForEach( p => p.Selected = false );
                foreach (var person in family.People.Where( p => selectedPeopleIds.Contains( p.Person.Id ) ).ToList() )
                {
                    person.Selected = true;
                    person.GroupTypes.ForEach( g => g.Selected = true );
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
            else
            {
                maWarning.Show( "Please pick at least one person.", ModalAlertType.Warning );
                return;
            }
        }

        #endregion

        #region Internal Methods 
        
        /// <summary>
        /// Goes back one page.
        /// </summary>
        private void GoBack()
        {
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
                var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                if ( family.People.Where( f => f.FamilyMember ).Count() > 0 )
                {
                    hfSelectedPerson.Value = string.Join( ",", family.People.Select( f => f.Person.Id ) ) + ",";
                    repPerson.DataSource = family.People.Where( f => f.FamilyMember ).OrderBy( p => p.Person.FullNameLastFirst );
                    repPerson.DataBind();
                    pnlSelectPerson.Update();                   
                }
                
                if ( family.People.Where( f => !f.FamilyMember ).Count() > 0 )
                {
                    repVisitors.DataSource = family.People.Where( f => !f.FamilyMember ).OrderBy( p => p.Person.FullNameLastFirst );
                    repVisitors.DataBind();
                    pnlSelectVisitor.Update();
                }
                SaveState();
            }
            else
            {
                string errorMsg = "<ul><li>" + errors.AsDelimited( "</li><li>" ) + "</li></ul>";
                maWarning.Show( errorMsg, Rock.Web.UI.Controls.ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Adds a new person.
        /// </summary>
        /// <param name="firstName">The first name.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="DOB">The DOB.</param>
        /// <param name="gender">The gender</param>
        /// <param name="attribute">The attribute.</param>
        protected Person CreatePerson( string firstName, string lastName, string dob, int? gender, string ability, string abilityGroup )
        {
            Person person = new Person().Clone( false );
            person.GivenName = firstName;
            person.LastName = lastName;
            person.BirthDate = Convert.ToDateTime( dob );
            if ( gender != null )
            {
                person.Gender = (Gender)gender;
            }

            PersonService ps = new PersonService();
            Rock.Data.RockTransactionScope.WrapTransaction( () =>
            {
                ps.Add( person, CurrentPersonId );
                ps.Save( person, CurrentPersonId );
            } );

            if ( !string.IsNullOrWhiteSpace( ability ) )
            {
                if ( abilityGroup == "Grade" )
                {
                    person.Grade = int.Parse( ability );
                }
                else if ( abilityGroup == "Ability" )
                {   
                    person.SetAttributeValue( "AbilityLevel", ability );
                    Rock.Attribute.Helper.SaveAttributeValues( person, CurrentPersonId );
                }
            }
            
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
            GroupMember groupMember = new GroupMember().Clone( false );
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
                        var groupMember = new GroupMember().Clone( false );
                        groupMember.PersonId = familyMember.Person.Id;
                        groupMember.GroupRoleId = role.Id;

                        group = new Group().Clone( false );
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
        /// Binds the person search results grid.
        /// </summary>
        private void BindPersonGrid()
        {
            var personService = new PersonService();
            var people = personService.Queryable();            

            if ( !string.IsNullOrEmpty( tbFirstNameSearch.Text ) && !string.IsNullOrEmpty( tbLastNameSearch.Text ) )
            {
                people = personService.GetByFullName( tbFirstNameSearch.Text + " " + tbLastNameSearch.Text );
            }
            else if ( !string.IsNullOrEmpty( tbLastNameSearch.Text ) )
            {
                people = people.Where( p => p.LastName.ToLower().StartsWith( tbLastNameSearch.Text ) );
            }
            else if ( !string.IsNullOrEmpty( tbFirstNameSearch.Text ) )
            {
                people = people.Where( p => p.FirstName.ToLower().StartsWith(  tbFirstNameSearch.Text ) );
            }

            if ( !string.IsNullOrEmpty( dpDOBSearch.Text ) )
            {
                DateTime searchDate;
                if ( DateTime.TryParse( dpDOBSearch.Text, out searchDate ) )
                {
                    people = people.Where( p => p.BirthYear == searchDate.Year 
                        && p.BirthMonth == searchDate.Month && p.BirthDay == searchDate.Day );
                }
            }

            if ( ddlGenderSearch.SelectedValueAsEnum<Gender>() != 0 )
            {
                var gender = ddlGenderSearch.SelectedValueAsEnum<Gender>();
                people = people.Where( p => p.Gender == gender );
            }
            
            if ( ddlAbilitySearch.SelectedIndex != 0 )
            {
                var optionGroup = ddlAbilitySearch.SelectedItem.Attributes["optiongroup"];
                if ( optionGroup.Equals( "Group" ) )
                {
                    var grade = ddlAbilitySearch.SelectedValueAsEnum<GradeLevel>();
                    people = people.Where( p => p.Grade == (int)grade );
                }
                else
                {
                    var ability = ddlAbilitySearch.SelectedValue;
                    people = people.Where( p => p.GetAttributeValue( "AbilityLevel" ).Contains( ability ) );
                }
            }                

            var peopleList = people.OrderBy( p => p.LastName ).ThenBy( p => p.FirstName ).ToList();
            rGridPersonResults.DataSource = peopleList.Select( p => new { p.Id, p.FirstName, p.LastName, p.BirthDate, p.Gender, Attribute = p.GetAttributeValue( "AbilityLevel" ) } ).ToList();                
            rGridPersonResults.DataBind();  
        }

        /// <summary>
        /// Gets a list of ability levels and grades.
        /// </summary>
        /// <returns>List Items</returns>
        protected List<ListItem> GetAbilityGradeList()
        {
            var dropDownList = new List<ListItem>();

            var abilityId = new AttributeService().Queryable().Where( a => a.Key == "AbilityLevel"
                && a.Categories.Any( c => c.Name == "CheckIn" ) ).Select( a => a.Id ).FirstOrDefault();

            if ( abilityId != null )
            {
                var abilityList = new AttributeValueService().GetByAttributeId( abilityId )
                    .Select( av => new ListItem( av.Value ) ).Distinct().ToList();
                foreach ( var ability in abilityList )
                {
                    ability.Attributes.Add( "optiongroup", "Ability" );                    
                    dropDownList.Add( ability );
                }                
            }

            var gradeList = Enum.GetValues( typeof( GradeLevel ) ).Cast<GradeLevel>()
                .Select( gl => new ListItem( gl.GetDescription(), gl.ToString() ) ).ToList();
            foreach ( var grade in gradeList )
            {
                grade.Attributes.Add( "optiongroup", "Grade" );
                dropDownList.Add( grade );
            }                

            dropDownList.Insert( 0, Rock.Constants.None.ListItem );
            return dropDownList;
        }
       
        #endregion                    
        
    }
}