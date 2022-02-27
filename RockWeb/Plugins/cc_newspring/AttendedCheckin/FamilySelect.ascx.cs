using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using cc.newspring.AttendedCheckIn.Utility;
using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.cc_newspring.AttendedCheckin
{
    /// <summary>
    /// Family Select block for Attended Check-in
    /// </summary>
    [DisplayName( "Family Select" )]
    [Category( "Check-in > Attended" )]
    [Description( "Attended Check-In Family Select Block" )]
    [BooleanField( "Enable Add Buttons", "Show the add people/visitor/family buttons on the family select page?", true, "", 1 )]
    [BooleanField( "Enable Override Button", "Show the override button to allow unfiltered checkins?", false, "", 1 )]
    [BooleanField( "Show Contact Info", "Show the phone and email columns on add people/visitor/family modals.", false, "", 2 )]
    [BooleanField( "Hide Special Needs", "Hide the special needs column from add people/visitor/family modals.", false, "", 3 )]
    [BooleanField( "Preselect Family Members", "By default all eligible family members will be selected for checkin.  Toggle this setting to force manual person selections.", true, "", 4)]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Default Connection Status", "Select the default connection status for people added in checkin", true, false, "B91BA046-BC1E-400C-B85D-638C1F4E0CE2", "", 5 )]
    [DefinedValueField( "8345DD45-73C6-4F5E-BEBD-B77FC83F18FD", "Default Phone Type", "By default, the Home Phone type is stored when Show Contact Info is turned on. Select a different type as the default.", false, false, "AA8732FB-2CEA-4C76-8D6D-6AAA2C6A4303", "", 6 )]
    [AttributeField( Rock.SystemGuid.EntityType.PERSON, "Person Special Needs Attribute", "Select the person attribute used to filter kids with special needs.", true, false, "8B562561-2F59-4F5F-B7DC-92B2BB7BB7CF", "", 7 )]
    [TextField( "Not Found Text", "What text should display when the nothing is found?", true, "Please add a person or family.", "", 8 )]
    public partial class FamilySelect : CheckInBlock
    {
        #region Variables

        /// <summary>
        /// Gets the kiosk campus identifier.
        /// </summary>
        /// <value>
        /// The kiosk campus identifier.
        /// </value>
        private int? KioskCampusId
        {
            get
            {
                var campusId = ViewState["CampusId"] as int?;
                if ( campusId != null )
                {
                    return campusId;
                }
                else
                {
                    var kioskCampusId = CurrentCheckInState.Kiosk.KioskGroupTypes
                        .Where( gt => gt.KioskGroups.Any( g => g.KioskLocations.Any( l => l.CampusId.HasValue ) ) )
                        .SelectMany( gt => gt.KioskGroups.SelectMany( g => g.KioskLocations.Select( l => l.CampusId ) ) )
                        .FirstOrDefault();
                    ViewState["CampusId"] = kioskCampusId;
                    return kioskCampusId;
                }
            }
        }

        /// <summary>
        /// Gets the person special needs attribute key.
        /// </summary>
        /// <value>
        /// The special needs key.
        /// </value>
        private string SpecialNeedsKey
        {
            get
            {
                var specialNeedsKey = ViewState["SpecialNeedsKey"] as string;
                if ( !string.IsNullOrWhiteSpace( specialNeedsKey ) )
                {
                    return specialNeedsKey;
                }
                else
                {
                    var personSpecialNeedsGuid = GetAttributeValue( "PersonSpecialNeedsAttribute" ).AsGuid();
                    if ( personSpecialNeedsGuid != Guid.Empty )
                    {
                        var specialNeedsAttribute = AttributeCache.Get( personSpecialNeedsGuid );
                        if ( specialNeedsAttribute != null )
                        {
                            specialNeedsKey = specialNeedsAttribute.Key;
                            ViewState["SpecialNeedsKey"] = specialNeedsKey;
                            return specialNeedsKey;
                        }

                        return string.Empty;
                    }
                    else
                    {
                        throw new Exception( "The Person Special Needs attribute is not selected or invalid on the FamilySelect page." );
                    }
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether check-in is currently in override mode
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is override; otherwise, <c>false</c>.
        /// </value>
        protected bool IsOverride
        {
            get
            {
                return Request["Override"] != null && Request["Override"].AsBoolean();
            }
        }

        #endregion Variables

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( CurrentWorkflow == null || CurrentCheckInState == null )
            {
                NavigateToHomePage();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack && CurrentCheckInState != null )
            {
                if ( CurrentCheckInState.CheckIn.Families.Count > 0 )
                {
                    // Load the family results
                    ProcessFamily();

                    // Load the person/visitor results
                    ProcessPeople();

                    ShowHideResults( true );
                }
                else
                {
                    ShowHideResults( false );
                }
            }
        }

        #endregion Control Methods

        #region Click Events

        /// <summary>
        /// Handles the Click event of the lbBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbBack_Click( object sender, EventArgs e )
        {
            NavigateToPreviousPage();
        }

        /// <summary>
        /// Handles the Click event of the lbNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbNext_Click( object sender, EventArgs e )
        {
            // get people selections from client state
            var selectedPersonIds = hfPersonIds.Value.SplitDelimitedValues()
                .Select( int.Parse ).ToList();

            var selectedFamily = CurrentCheckInState.CheckIn.Families.FirstOrDefault( f => f.Selected );
            if ( selectedFamily == null )
            {
                maWarning.Show( "Please pick or add a family.", ModalAlertType.Warning );
                return;
            }
            else if ( selectedFamily.People.Count == 0 )
            {
                var errorMsg = "No one in this family is eligible to check-in.";
                maWarning.Show( errorMsg, ModalAlertType.Warning );
                return;
            }
            else if ( !selectedPersonIds.Any() )
            {
                // reset client state
                hfPersonIds.Value = ViewState["hfPersonIds"] as string;
                maWarning.Show( "Please pick at least one person.", ModalAlertType.Warning );
                return;
            }
            else
            {
                // transfer people selections to server state
                selectedFamily.People.ForEach( p => p.Selected = selectedPersonIds.Contains( p.Person.Id ) );

                var errors = new List<string>();
                if ( ProcessActivity( "Activity Search", out errors ) )
                {
                    SaveState();
                    NavigateToNextPage();
                }
                else
                {
                    var errorMsg = "<ul><li>" + errors.AsDelimited( "</li><li>" ) + "</li></ul>";
                    maWarning.Show( errorMsg.Replace( "'", @"\'" ), ModalAlertType.Warning );
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the lvFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewCommandEventArgs"/> instance containing the event data.</param>
        protected void lvFamily_ItemCommand( object sender, ListViewCommandEventArgs e )
        {
            var id = int.Parse( e.CommandArgument.ToString() );
            var family = CurrentCheckInState.CheckIn.Families.FirstOrDefault( f => f.Group.Id == id );

            foreach ( ListViewDataItem li in ( (ListView)sender ).Items )
            {
                ( (LinkButton)li.FindControl( "lbSelectFamily" ) ).RemoveCssClass( "active" );
            }

            if ( !family.Selected )
            {
                CurrentCheckInState.CheckIn.Families.ForEach( f => f.Selected = false );
                ( (LinkButton)e.Item.FindControl( "lbSelectFamily" ) ).AddCssClass( "active" );
                family.Selected = true;
                ProcessPeople( family );
            }
            else
            {
                family.Selected = false;
                lvPerson.DataSource = null;
                lvPerson.DataBind();
                dpPersonPager.Visible = false;
                pnlPerson.Update();
                lvVisitor.DataSource = null;
                lvVisitor.DataBind();
                dpVisitorPager.Visible = false;
                pnlVisitor.Update();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddVisitor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddVisitor_Click( object sender, EventArgs e )
        {
            lblAddPersonHeader.Text = "Add Visitor";
            hfNewPersonType.Value = "Visitor";
            LoadPersonFields();
            mdlAddPerson.Show();
        }

        /// <summary>
        /// Handles the Click event of the lbAddFamilyMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddFamilyMember_Click( object sender, EventArgs e )
        {
            lblAddPersonHeader.Text = "Add Family Member";
            hfNewPersonType.Value = "Person";
            LoadPersonFields();
            mdlAddPerson.Show();
        }

        /// <summary>
        /// Handles the Click event of the lbNewFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbNewFamily_Click( object sender, EventArgs e )
        {
            var newFamilyList = new List<SerializedPerson>();
            var familyMembersToAdd = dpNewFamily.PageSize * 2;
            newFamilyList.AddRange( Enumerable.Repeat( new SerializedPerson(), familyMembersToAdd ) );
            ViewState["newFamily"] = newFamilyList;
            lvNewFamily.DataSource = newFamilyList;
            lvNewFamily.DataBind();
            mdlNewFamily.Show();
        }

        protected void lbOverride_Click( object sender, EventArgs e )
        {
            var queryParams = IsOverride ? null : new Dictionary<string, string>{ {"Override", "True" } };
            NavigateToCurrentPage( queryParams );
        }

        /// <summary>
        /// Handles the PagePropertiesChanging event of the lvFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PagePropertiesChangingEventArgs"/> instance containing the event data.</param>
        protected void lvFamily_PagePropertiesChanging( object sender, PagePropertiesChangingEventArgs e )
        {
            dpFamilyPager.SetPageProperties( e.StartRowIndex, e.MaximumRows, false );

            // rebind List View
            lvFamily.DataSource = CurrentCheckInState.CheckIn.Families
                .OrderByDescending( f => f.Group.CampusId == KioskCampusId )
                .ThenBy( f => f.Caption ).Take( 50 ).ToList();
            lvFamily.DataBind();
            pnlFamily.Update();
        }

        /// <summary>
        /// Handles the PagePropertiesChanging event of the dpPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PagePropertiesChangingEventArgs"/> instance containing the event data.</param>
        protected void lvPerson_PagePropertiesChanging( object sender, PagePropertiesChangingEventArgs e )
        {
            dpPersonPager.SetPageProperties( e.StartRowIndex, e.MaximumRows, false );
            BindPager( hfPersonIds.Value, isFamilyMember: true );
        }

        /// <summary>
        /// Handles the PagePropertiesChanging event of the dpVisitor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PagePropertiesChangingEventArgs"/> instance containing the event data.</param>
        protected void lvVisitor_PagePropertiesChanging( object sender, PagePropertiesChangingEventArgs e )
        {
            dpVisitorPager.SetPageProperties( e.StartRowIndex, e.MaximumRows, false );
            BindPager( hfPersonIds.Value, isFamilyMember: false );
        }

        /// <summary>
        /// Binds the pager.
        /// </summary>
        /// <param name="selectedPersonIds">The selected person ids.</param>
        /// <param name="isFamilyMember">if set to <c>true</c> [is family member].</param>
        private void BindPager( string selectedPersonIds, bool isFamilyMember )
        {
            var selectedFamily = CurrentCheckInState.CheckIn.Families.FirstOrDefault( f => f.Selected );
            if ( selectedFamily != null )
            {
                var peopleList = selectedFamily.People.Where( f => f.FamilyMember == isFamilyMember && ( IsOverride || !f.ExcludedByFilter ) )
                    .OrderByDescending( p => p.Person.AgePrecise ).ToList();

                var selectedPeople = selectedPersonIds.SplitDelimitedValues().Select( int.Parse ).ToList();
                peopleList.ForEach( p => p.Selected = selectedPeople.Contains( p.Person.Id ) );

                // rebind List View
                if ( isFamilyMember )
                {
                    lvPerson.DataSource = peopleList;
                    lvPerson.DataBind();
                    pnlPerson.Update();
                }
                else
                {
                    lvVisitor.DataSource = peopleList;
                    lvVisitor.DataBind();
                    pnlVisitor.Update();
                }
            }
        }

        #endregion Click Events

        #region DataBound Methods

        /// <summary>
        /// Handles the DataBound event of the lvFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lvFamily_ItemDataBound( object sender, ListViewItemEventArgs e )
        {
            if ( e.Item.ItemType == ListViewItemType.DataItem )
            {
                var family = (CheckInFamily)e.Item.DataItem;
                var lbSelectFamily = (LinkButton)e.Item.FindControl( "lbSelectFamily" );
                lbSelectFamily.CommandArgument = family.Group.Id.ToString();

                lbSelectFamily.Text = string.Format( @"{0}<br />
                    <span class='checkin-sub-title'>
						{1}
				    </span>
                    <div class='fa fa-refresh fa-spin'></div>
                ", family.Caption, family.SubCaption );

                if ( family.Selected )
                {
                    lbSelectFamily.AddCssClass( "active" );
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the lvPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void lvPeople_ItemDataBound( object sender, ListViewItemEventArgs e )
        {
            if ( e.Item.ItemType == ListViewItemType.DataItem )
            {
                var lbSelectPerson = (LinkButton)e.Item.FindControl( "lbSelectPerson" );
                var person = (CheckInPerson)e.Item.DataItem;

                var ageLabel = "n/a";
                var birthdayLabel = "n/a";
                if ( person.Person.Age != null )
                {
                    birthdayLabel = person.Person.BirthMonth + "/" + person.Person.BirthDay;
                    ageLabel = person.Person.Age <= 18 ? person.Person.Age.ToString() : "Adult";
                }

                lbSelectPerson.Text = string.Format( @"{0}<br />
                    <span class='checkin-sub-title'>
						Birthday: {1} Age: {2}
				    </span>
                ", person.Person.FullName, birthdayLabel, ageLabel );

                if ( person.Selected )
                {
                    lbSelectPerson.AddCssClass( "active" );
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the lvNewFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ListViewItemEventArgs"/> instance containing the event data.</param>
        protected void lvNewFamily_ItemDataBound( object sender, ListViewItemEventArgs e )
        {
            if ( e.Item.ItemType == ListViewItemType.DataItem )
            {
                var person = ( (ListViewDataItem)e.Item ).DataItem as SerializedPerson;

                var ddlSuffix = (RockDropDownList)e.Item.FindControl( "ddlSuffix" );
                ddlSuffix.BindToDefinedType( DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_SUFFIX ) ), true );
                if ( person.SuffixValueId.HasValue )
                {
                    ddlSuffix.SelectedValue = person.SuffixValueId.ToString();
                }

                var ddlGender = (RockDropDownList)e.Item.FindControl( "ddlGender" );
                ddlGender.BindToEnum<Gender>();
                if ( person.Gender != Gender.Unknown )
                {
                    ddlGender.SelectedIndex = person.Gender.ConvertToInt();
                }

                var ddlAbilityGrade = (RockDropDownList)e.Item.FindControl( "ddlAbilityGrade" );
                ddlAbilityGrade.LoadAbilityAndGradeItems();
                if ( !string.IsNullOrWhiteSpace( person.Ability ) )
                {
                    ddlAbilityGrade.SelectedValue = person.Ability;
                }

                // set the header for this section manually since it's generated on the fly
                var famAbilityGrade = (HtmlGenericControl)lvNewFamily.FindControl( "famAbilityGrade" );
                if ( famAbilityGrade != null && ddlAbilityGrade.Items.Count > 0 )
                {
                    var allCategories = ddlAbilityGrade.Items.Cast<ListItem>()
                        .Where( i => i.Attributes.Count > 0 )
                        .Select( i => i.Attributes["optiongroup"] ).Distinct()
                        .OrderBy( i => i ).ToList();

                    famAbilityGrade.InnerText = String.Join( "/", allCategories );
                }

                UpdateModalLayout( e.Item, "div" );
            }
        }

        /// <summary>
        /// Handles the LayoutCreated event of the lvNewFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lvNewFamily_LayoutCreated( object sender, EventArgs e )
        {
            UpdateModalLayout( lvNewFamily, "hdr" );
        }

        #endregion DataBound Methods

        #region Modal Events

        /// <summary>
        /// Handles the Click event of the lbClosePerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbClosePerson_Click( object sender, EventArgs e )
        {
            mdlAddPerson.Hide();
        }

        /// <summary>
        /// Handles the Click event of the lbCloseFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCloseFamily_Click( object sender, EventArgs e )
        {
            mdlNewFamily.Hide();
        }

        /// <summary>
        /// Handles the Click event of the lbPersonSearch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbPersonSearch_Click( object sender, EventArgs e )
        {
            var firstNameIsEmpty = string.IsNullOrEmpty( tbFirstName.Text );
            var lastNameIsEmpty = string.IsNullOrEmpty( tbLastName.Text );

            if ( firstNameIsEmpty && lastNameIsEmpty )
            {
                maWarning.Show( "First or Last Name is required to search.", ModalAlertType.Information );
                return;
            }

            rGridPersonResults.PageIndex = 0;
            rGridPersonResults.Visible = true;
            rGridPersonResults.PageSize = 4;
            lbNewPerson.Visible = true;
            BindPersonGrid();
        }

        /// <summary>
        /// Handles the Click event of the lbNewPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbNewPerson_Click( object sender, EventArgs e )
        {
            var newPerson = new SerializedPerson
            {
                FirstName = tbFirstName.Text,
                LastName = tbLastName.Text,
                SuffixValueId = ddlPersonSuffix.SelectedValueAsId(),
                BirthDate = dpPersonDOB.SelectedDate,
                Gender = ddlPersonGender.SelectedValueAsEnum<Gender>(),
                Ability = ddlPersonAbilityGrade.SelectedValue,
                AbilityGroup = ddlPersonAbilityGrade.SelectedItem.Attributes["optiongroup"]
            };

            if ( GetAttributeValue( "ShowContactInfo" ).AsBoolean() )
            {
                newPerson.PhoneNumber = tbPhone.Text;
                newPerson.Email = tbEmail.Text;
            }

            if ( !GetAttributeValue( "HideSpecialNeeds" ).AsBoolean() )
            {
                newPerson.HasSpecialNeeds = cbPersonSpecialNeeds.Checked;
            }

            if ( newPerson.IsValid() )
            {
                // Person passed validation
                var newPeople = CreatePeople( new List<SerializedPerson>( 1 ) { newPerson } );
                var processFamily = false;
                var checkInPerson = new CheckInPerson
                {
                    Person = newPeople.FirstOrDefault(),
                    FirstTime = true
                };

                var selectedFamily = CurrentCheckInState.CheckIn.Families.FirstOrDefault( f => f.Selected );
                if ( selectedFamily == null )
                {
                    // No family currently selected, create a new one
                    selectedFamily = new CheckInFamily();
                    selectedFamily.Selected = true;

                    selectedFamily.Group = AddGroupMembers( null, newPeople );
                    selectedFamily.Caption = selectedFamily.Group.Name;
                    CurrentCheckInState.CheckIn.Families.Add( selectedFamily );
                    processFamily = true;
                }
                else
                {
                    hfPersonIds.Value += checkInPerson.Person.Id + ",";

                    // Existing family, create the appropriate relationship(s)
                    if ( hfNewPersonType.Value.Equals( "Person" ) )
                    {   // Family Member
                        AddGroupMembers( selectedFamily.Group, newPeople );
                        checkInPerson.FamilyMember = true;
                    }
                    else
                    {   // Visitor
                        checkInPerson.FamilyMember = false;
                        if ( checkInPerson.Person.Age < 18 || checkInPerson.Person.AgeClassification == AgeClassification.Child )
                        {
                            AddVisitorRelationships( selectedFamily, checkInPerson.Person.Id );

                            // Make the family group so the child role is set correctly (Adult is default)
                            AddGroupMembers( null, newPeople );
                        }
                    }
                }

                checkInPerson.Selected = true;
                selectedFamily.People.Add( checkInPerson );
                selectedFamily.SubCaption = string.Join( ",", selectedFamily.People.Select( p => p.Person.FirstName ) );

                if ( processFamily )
                {
                    ShowHideResults( selectedFamily.People.Count > 0 );
                    ProcessFamily( selectedFamily );
                }

                ProcessPeople( selectedFamily );
                mdlAddPerson.Hide();
            }
            else
            {
                maWarning.Show( "Validation: Name and Gender are required.", ModalAlertType.Information );
            }
        }

        /// <summary>
        /// Handles the RowCommand event of the grdPersonSearchResults control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewCommandEventArgs"/> instance containing the event data.</param>
        protected void rGridPersonResults_AddExistingPerson( object sender, GridViewCommandEventArgs e )
        {
            if ( e.CommandName.Equals( "Add" ) )
            {
                var selectedFamily = CurrentCheckInState.CheckIn.Families.FirstOrDefault( f => f.Selected );
                if ( selectedFamily != null )
                {
                    var rowIndex = int.Parse( e.CommandArgument.ToString() );
                    var personId = int.Parse( rGridPersonResults.DataKeys[rowIndex].Value.ToString() );

                    var checkInPerson = selectedFamily.People.FirstOrDefault( p => p.Person.Id == personId );
                    if ( checkInPerson == null )
                    {
                        var rockContext = new RockContext();
                        checkInPerson = new CheckInPerson
                        {
                            Person = new PersonService( rockContext ).Get( personId ).Clone( false )
                        };

                        if ( hfNewPersonType.Value.Equals( "Person" ) )
                        {
                            // New family member, add them to the current family if they don't exist
                            AddGroupMembers( selectedFamily.Group, new List<Person> { checkInPerson.Person } );
                            checkInPerson.FamilyMember = true;
                        }
                        else
                        {
                            // Visitor, associate with current family unless they're an adult
                            checkInPerson.FamilyMember = false;
                            if ( checkInPerson.Person.Age < 18 || checkInPerson.Person.AgeClassification == AgeClassification.Child )
                            {
                                AddVisitorRelationships( selectedFamily, checkInPerson.Person.Id );
                            }
                        }

                        checkInPerson.Selected = true;
                        selectedFamily.People.Add( checkInPerson );
                        ProcessPeople( selectedFamily );
                    }
                    else
                    {
                        maWarning.Show( "That person is already in the existing family", ModalAlertType.Information );
                    }

                    mdlAddPerson.Hide();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSaveFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSaveFamily_Click( object sender, EventArgs e )
        {
            var showContactInfo = GetAttributeValue( "ShowContactInfo" ).AsBoolean();
            var hideSpecialNeeds = GetAttributeValue( "HideSpecialNeeds" ).AsBoolean();
            var newFamilyList = (List<SerializedPerson>)ViewState["newFamily"] ?? new List<SerializedPerson>();
            var currentPage = ViewState["currentPage"] as int?;
            var personOffset = 0;
            var pageOffset = 0;

            // add people from the current page
            foreach ( ListViewItem item in lvNewFamily.Items )
            {
                var newPerson = new SerializedPerson
                {
                    FirstName = ( (TextBox)item.FindControl( "tbFirstName" ) ).Text,
                    LastName = ( (TextBox)item.FindControl( "tbLastName" ) ).Text,
                    SuffixValueId = ( (RockDropDownList)item.FindControl( "ddlSuffix" ) ).SelectedValueAsId(),
                    BirthDate = ( (DatePicker)item.FindControl( "dpBirthDate" ) ).SelectedDate,
                    Gender = ( (RockDropDownList)item.FindControl( "ddlGender" ) ).SelectedValueAsEnum<Gender>(),
                    Ability = ( (RockDropDownList)item.FindControl( "ddlAbilityGrade" ) ).SelectedValue,
                    AbilityGroup = ( (RockDropDownList)item.FindControl( "ddlAbilityGrade" ) ).SelectedItem.Attributes["optiongroup"]
                };

                if ( showContactInfo )
                {
                    newPerson.PhoneNumber = ( (TextBox)item.FindControl( "tbPhone" ) ).Text;
                    newPerson.Email = ( (TextBox)item.FindControl( "tbEmail" ) ).Text;
                }

                if ( !hideSpecialNeeds )
                {
                    newPerson.HasSpecialNeeds = ( (RockCheckBox)item.FindControl( "cbSpecialNeeds" ) ).Checked;
                }

                if ( !string.IsNullOrWhiteSpace( newPerson.FirstName ) && !newPerson.IsValid() )
                {
                    maWarning.Show( "First name, Last name and Gender are required.", ModalAlertType.Information );
                    return;
                }

                if ( currentPage.HasValue )
                {
                    pageOffset = (int)currentPage * lvNewFamily.DataKeys.Count;
                }

                newFamilyList[pageOffset + personOffset] = newPerson;
                personOffset++;
            }

            var newPeople = CreatePeople( newFamilyList.Where( p => p.IsValid() ).ToList() );

            // People passed validation
            if ( newPeople.Any() )
            {
                // Create family group (by passing null) and add group members
                var familyGroup = AddGroupMembers( null, newPeople );

                var checkInFamily = new CheckInFamily();

                foreach ( var person in newPeople )
                {
                    var checkInPerson = new CheckInPerson
                    {
                        Person = person,
                        FirstTime = true,
                        Selected = true,
                        FamilyMember = true
                    };
                    checkInFamily.People.Add( checkInPerson );
                }

                checkInFamily.Group = familyGroup;
                checkInFamily.Caption = familyGroup.Name;
                checkInFamily.SubCaption = string.Join( ",", checkInFamily.People.Select( p => p.Person.FirstName ) );
                checkInFamily.Selected = true;

                CurrentCheckInState.CheckIn.Families.Clear();
                CurrentCheckInState.CheckIn.Families.Add( checkInFamily );

                ShowHideResults( checkInFamily.People.Count > 0 );
                ProcessFamily( checkInFamily );
                ProcessPeople( checkInFamily );
                mdlNewFamily.Hide();
            }
            else
            {
                maWarning.Show( "Validation: Name and Gender are required.", ModalAlertType.Information );
            }
        }

        /// <summary>
        /// Handles the PagePropertiesChanging event of the lvNewFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PagePropertiesChangingEventArgs"/> instance containing the event data.</param>
        protected void lvNewFamily_PagePropertiesChanging( object sender, PagePropertiesChangingEventArgs e )
        {
            var showContactInfo = GetAttributeValue( "ShowContactInfo" ).AsBoolean();
            var hideSpecialNeeds = GetAttributeValue( "HideSpecialNeeds" ).AsBoolean();
            var newFamilyList = (List<SerializedPerson>)ViewState["newFamily"] ?? new List<SerializedPerson>();
            var currentPage = e.StartRowIndex / e.MaximumRows;
            var previousPage = ViewState["currentPage"] as int?;
            var personOffset = 0;
            var pageOffset = 0;

            foreach ( ListViewItem item in lvNewFamily.Items )
            {
                var newPerson = new SerializedPerson
                {
                    FirstName = ( (TextBox)item.FindControl( "tbFirstName" ) ).Text,
                    LastName = ( (TextBox)item.FindControl( "tbLastName" ) ).Text,
                    SuffixValueId = ( (RockDropDownList)item.FindControl( "ddlSuffix" ) ).SelectedValueAsId(),
                    BirthDate = ( (DatePicker)item.FindControl( "dpBirthDate" ) ).SelectedDate,
                    Gender = ( (RockDropDownList)item.FindControl( "ddlGender" ) ).SelectedValueAsEnum<Gender>(),
                    Ability = ( (RockDropDownList)item.FindControl( "ddlAbilityGrade" ) ).SelectedValue,
                    AbilityGroup = ( (RockDropDownList)item.FindControl( "ddlAbilityGrade" ) ).SelectedItem.Attributes["optiongroup"]
                };

                if ( showContactInfo )
                {
                    newPerson.PhoneNumber = ( (TextBox)item.FindControl( "tbPhone" ) ).Text;
                    newPerson.Email = ( (TextBox)item.FindControl( "tbEmail" ) ).Text;
                }

                if ( !hideSpecialNeeds )
                {
                    newPerson.HasSpecialNeeds = ( (RockCheckBox)item.FindControl( "cbSpecialNeeds" ) ).Checked;
                }

                if ( previousPage.HasValue )
                {
                    pageOffset = (int)previousPage * e.MaximumRows;
                }

                if ( e.StartRowIndex + personOffset + e.MaximumRows >= newFamilyList.Count )
                {
                    newFamilyList.AddRange( Enumerable.Repeat( new SerializedPerson() { LastName = newPerson.LastName }, e.MaximumRows ) );
                }

                newFamilyList[pageOffset + personOffset] = newPerson;
                personOffset++;
            }

            ViewState["currentPage"] = currentPage;
            ViewState["newFamily"] = newFamilyList;
            dpNewFamily.SetPageProperties( e.StartRowIndex, e.MaximumRows, false );
            lvNewFamily.DataSource = newFamilyList;
            lvNewFamily.DataBind();
            mdlNewFamily.Show();
        }

        /// <summary>
        /// Handles the GridRebind event of the rGridPersonResults control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGridPersonResults_GridRebind( object sender, EventArgs e )
        {
            BindPersonGrid();
        }

        #endregion Modal Events

        #region Internal Methods

        /// <summary>
        /// Refreshes the family.
        /// </summary>
        /// <param name="selectedFamily">The selected family.</param>
        private void ProcessFamily( CheckInFamily selectedFamily = null )
        {
            selectedFamily = selectedFamily ?? CurrentCheckInState.CheckIn.Families.FirstOrDefault( f => f.Selected );
            var familyList = CurrentCheckInState.CheckIn.Families
                .OrderByDescending( f => f.Group.CampusId == KioskCampusId )
                .ThenBy( f => f.Caption ).ToList();

            // Order families by campus then by caption
            if ( CurrentCheckInState.CheckIn.Families.Count > 1 )
            {
                dpFamilyPager.Visible = true;
                dpFamilyPager.SetPageProperties( 0, dpFamilyPager.MaximumRows, false );
            }

            if ( selectedFamily != null )
            {
                selectedFamily.Selected = true;
            }
            else
            {
                familyList.FirstOrDefault().Selected = true;
            }

            lvFamily.DataSource = familyList;
            lvFamily.DataBind();
            pnlFamily.Update();
        }

        /// <summary>
        /// Processes the family.
        /// </summary>
        /// <param name="selectedFamily">The selected family.</param>
        private void ProcessPeople( CheckInFamily selectedFamily = null )
        {
            var errors = new List<string>();
            if ( ProcessActivity( "Person Search", out errors ) )
            {
                List<CheckInPerson> memberDataSource = null;
                List<CheckInPerson> visitorDataSource = null;

                var preselectFamilyMembers = GetAttributeValue( "PreselectFamilyMembers" ).AsBoolean();
                selectedFamily = selectedFamily ?? CurrentCheckInState.CheckIn.Families.FirstOrDefault( f => f.Selected );

                if ( selectedFamily != null )
                {
                    memberDataSource = selectedFamily.People.Where( f => f.FamilyMember && ( IsOverride || !f.ExcludedByFilter ) )
                        .OrderByDescending( p => p.Person.AgePrecise ).ToList();
                    memberDataSource.ForEach( p => p.Selected = preselectFamilyMembers );

                    visitorDataSource = selectedFamily.People.Where( f => !f.FamilyMember && ( IsOverride || !f.ExcludedByFilter ) )
                        .OrderByDescending( p => p.Person.AgePrecise ).ToList();

                    hfPersonIds.Value = string.Join( ",", memberDataSource.Where( p => p.Selected ).Select( f => f.Person.Id ) ) + ",";
                    ViewState["hfPersonIds"] = hfPersonIds.Value;
                }

                lvPerson.DataSource = memberDataSource;
                lvPerson.DataBind();
                lvVisitor.DataSource = visitorDataSource;
                lvVisitor.DataBind();

                if ( memberDataSource != null )
                {
                    dpPersonPager.Visible = true;
                    dpPersonPager.SetPageProperties( 0, dpPersonPager.MaximumRows, false );
                }

                if ( visitorDataSource != null )
                {
                    dpVisitorPager.Visible = true;
                    dpVisitorPager.SetPageProperties( 0, dpVisitorPager.MaximumRows, false );
                }

                // Force an update
                pnlPerson.Update();
                pnlVisitor.Update();
            }
            else
            {
                var errorMsg = "<ul><li>" + errors.AsDelimited( "</li><li>" ) + "</li></ul>";
                maWarning.Show( errorMsg.Replace( "'", @"\'" ), ModalAlertType.Warning );
            }
        }

        /// <summary>
        /// Sets the display to show or hide panels depending on the search results.
        /// </summary>
        /// <param name="hasValidResults">if set to <c>true</c> [has valid results].</param>
        private void ShowHideResults( bool hasValidResults )
        {
            lbNext.Enabled = hasValidResults;
            lbNext.Visible = hasValidResults;
            pnlFamily.Visible = hasValidResults;
            pnlPerson.Visible = hasValidResults;
            pnlVisitor.Visible = hasValidResults;
            actions.Visible = hasValidResults;

            if ( !hasValidResults )
            {
                // Show a custom message when nothing is found
                var nothingFoundText = GetAttributeValue( "NotFoundText" );
                lblFamilyTitle.InnerText = "No Results";
                divNothingFound.InnerText = nothingFoundText;
                divNothingFound.Visible = true;
            }
            else
            {
                lblFamilyTitle.InnerText = "Search Results";
                divNothingFound.Visible = false;
            }

            // Admin option whether add buttons can be displayed
            var showAddButtons = GetAttributeValue( "EnableAddButtons" ).AsBoolean();

            divActions.Visible = showAddButtons;
            lbAddFamilyMember.Visible = showAddButtons;
            lbAddVisitor.Visible = showAddButtons;
            lbNewFamily.Visible = showAddButtons;
            lbOverride.Visible = GetAttributeValue( "EnableOverrideButton" ).AsBoolean();

            if ( IsOverride )
            {
                lbOverride.AddCssClass( "active" );
            }
        }

        /// <summary>
        /// Adjusts the modal layout.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="prefix">The prefix.</param>
        protected void UpdateModalLayout( Control container, string prefix )
        {
            var showContactInfo = GetAttributeValue( "ShowContactInfo" ).AsBoolean();
            var hideSpecialNeeds = GetAttributeValue( "HideSpecialNeeds" ).AsBoolean();
            if ( container != null && ( showContactInfo || hideSpecialNeeds ) )
            {
                // get control references
                var ctrlFirstName = (HtmlGenericControl)container.FindControl( string.Format( "{0}FirstName", prefix ) );
                var ctrlLastName = (HtmlGenericControl)container.FindControl( string.Format( "{0}LastName", prefix ) );
                var ctrlGender = (HtmlGenericControl)container.FindControl( string.Format( "{0}Gender", prefix ) );
                var ctrlAbilityGrade = (HtmlGenericControl)container.FindControl( string.Format( "{0}AbilityGrade", prefix ) );
                var ctrlPhoneNumber = (HtmlGenericControl)container.FindControl( string.Format( "{0}PhoneNumber", prefix ) );
                var ctrlEmail = (HtmlGenericControl)container.FindControl( string.Format( "{0}Email", prefix ) );
                var ctrlSpecialNeeds = (HtmlGenericControl)container.FindControl( string.Format( "{0}SpecialNeeds", prefix ) );

                if ( showContactInfo )
                {
                    // adjust spacing to account for additional columns
                    ctrlFirstName.RemoveCssClass( "col-xs-2" );
                    ctrlLastName.RemoveCssClass( "col-xs-2" );
                    ctrlGender.RemoveCssClass( "col-xs-2" );
                    ctrlAbilityGrade.RemoveCssClass( "col-xs-2" );

                    ctrlFirstName.AddCssClass( "col-xs-1" );
                    ctrlLastName.AddCssClass( "col-xs-1" );
                    ctrlGender.AddCssClass( "col-xs-1" );
                    ctrlAbilityGrade.AddCssClass( "col-xs-1" );
                    ctrlPhoneNumber.Visible = true;
                    ctrlPhoneNumber.Disabled = false;
                    ctrlEmail.Visible = true;
                    ctrlEmail.Disabled = false;
                }

                if ( hideSpecialNeeds )
                {
                    ctrlSpecialNeeds.Visible = false;
                    ctrlSpecialNeeds.Disabled = true;

                    // make sure ability/grade gets the extra margin
                    var currentWidth = ctrlAbilityGrade.Attributes["class"].Split( ' ' ).FirstOrDefault( c => c.StartsWith("col-xs") );
                    if ( !string.IsNullOrWhiteSpace( currentWidth ))
                    {
                        var newWidth = string.Format( "col-xs-{0}", Char.GetNumericValue( currentWidth.Last() ) + 1 );
                        ctrlAbilityGrade.RemoveCssClass( currentWidth );
                        ctrlAbilityGrade.AddCssClass( newWidth );
                    }
                }

                // allow margin on the right most column
                container.ControlsOfTypeRecursive<HtmlGenericControl>().Last( c => !c.Disabled ).RemoveCssClass( "hard-right" );
            }
        }

        /// <summary>
        /// Loads the person fields.
        /// </summary>
        private void LoadPersonFields()
        {
            tbFirstName.Text = string.Empty;
            tbLastName.Text = string.Empty;
            ddlPersonSuffix.BindToDefinedType( DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_SUFFIX ) ), true );
            ddlPersonSuffix.SelectedIndex = 0;
            ddlPersonGender.BindToEnum<Gender>();
            ddlPersonGender.SelectedIndex = 0;
            ddlPersonAbilityGrade.LoadAbilityAndGradeItems();
            ddlPersonAbilityGrade.SelectedIndex = 0;
            tbPhone.Text = string.Empty;
            tbEmail.Text = string.Empty;
            rGridPersonResults.Visible = false;
            lbNewPerson.Visible = false;

            UpdateModalLayout( mdlAddPerson, "hdr" );
            UpdateModalLayout( mdlAddPerson, "div" );
        }

        /// <summary>
        /// Binds the person search results grid on the New Person/Visitor screen.
        /// </summary>
        private void BindPersonGrid()
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );
            var peopleQry = personService.Queryable().AsNoTracking();
            var personPhoneType = DefinedValueCache.Get( GetAttributeValue( "DefaultPhoneType" ).AsGuid(), rockContext );
            var abilityLevelValues = DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.PERSON_ABILITY_LEVEL_TYPE ), rockContext ).DefinedValues;

            var firstNameIsEmpty = string.IsNullOrEmpty( tbFirstName.Text );
            var lastNameIsEmpty = string.IsNullOrEmpty( tbLastName.Text );
            if ( !firstNameIsEmpty && !lastNameIsEmpty )
            {
                peopleQry = personService.GetByFullName( string.Format( "{0} {1}", tbFirstName.Text, tbLastName.Text ), false );
            }
            else if ( !lastNameIsEmpty )
            {
                peopleQry = peopleQry.Where( p => p.LastName.Equals( tbLastName.Text ) );
            }
            else if ( !firstNameIsEmpty )
            {
                peopleQry = peopleQry.Where( p => p.FirstName.Equals( tbFirstName.Text ) );
            }

            if ( ddlPersonSuffix.SelectedValueAsInt().HasValue )
            {
                var suffixValueId = ddlPersonSuffix.SelectedValueAsId();
                peopleQry = peopleQry.Where( p => p.SuffixValueId == suffixValueId );
            }

            if ( !string.IsNullOrEmpty( dpPersonDOB.Text ) )
            {
                DateTime searchDate;
                if ( DateTime.TryParse( dpPersonDOB.Text, out searchDate ) )
                {
                    peopleQry = peopleQry.Where( p => p.BirthYear == searchDate.Year
                        && p.BirthMonth == searchDate.Month && p.BirthDay == searchDate.Day );
                }
            }

            if ( ddlPersonGender.SelectedValueAsEnum<Gender>() != 0 )
            {
                var gender = ddlPersonGender.SelectedValueAsEnum<Gender>();
                peopleQry = peopleQry.Where( p => p.Gender == gender );
            }

            // Set a filter if an ability/grade was selected
            var optionGroup = ddlPersonAbilityGrade.SelectedItem.Attributes["optiongroup"];
            if ( !string.IsNullOrEmpty( optionGroup ) )
            {
                if ( optionGroup.Equals( "Ability" ) )
                {
                    peopleQry = peopleQry.WhereAttributeValue( rockContext, "AbilityLevel", ddlPersonAbilityGrade.SelectedValue );
                }
                else if ( optionGroup.Equals( "Grade" ) )
                {
                    var grade = ddlPersonAbilityGrade.SelectedValueAsId();
                    peopleQry = peopleQry.WhereGradeOffsetRange( grade, grade, false );
                }
            }

            // Set a filter if special needs was checked
            if ( cbPersonSpecialNeeds.Checked )
            {
                peopleQry = peopleQry.WhereAttributeValue( rockContext, SpecialNeedsKey, "Yes" );
            }

            // call list here to get virtual properties not supported in LINQ
            var peopleList = peopleQry.ToList();

            // load attributes to display additional person info
            peopleList.ForEach( p => p.LoadAttributes( rockContext ) );

            // Load person grid
            rGridPersonResults.DataSource = peopleList.Select( p => new
            {
                p.Id,
                p.FirstName,
                p.LastName,
                p.SuffixValue,
                p.BirthDate,
                p.Age,
                p.Gender,
                Attribute = p.GradeOffset.HasValue
                        ? p.GradeFormatted
                        : abilityLevelValues.Where( dv => dv.Guid.ToString()
                            .Equals( p.AttributeValues["AbilityLevel"].Value, StringComparison.OrdinalIgnoreCase ) )
                            .Select( dv => dv.Value ).FirstOrDefault(),
                Phone = p.PhoneNumbers.Any( n => n.NumberTypeValueId == personPhoneType.Id )
                        ? p.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == personPhoneType.Id ).NumberFormatted
                        : string.Empty,
                p.Email,
                HasSpecialNeeds = p.AttributeValues.Keys.Contains( SpecialNeedsKey )
                         ? p.AttributeValues[SpecialNeedsKey].Value.AsBoolean() ? "Yes" : "No"
                         : string.Empty
            } ).OrderByDescending( p => p.BirthDate ).ToList();

            rGridPersonResults.DataBind();
        }

        /// <summary>
        /// Gets the current person.
        /// </summary>
        /// <param name="parameterPersonId">The parameter person identifier.</param>
        /// <returns></returns>
        private CheckInPerson GetCurrentPerson( int? parameterPersonId = null )
        {
            var personId = parameterPersonId ?? Request.QueryString["personId"].AsType<int?>();
            var family = CurrentCheckInState.CheckIn.Families.FirstOrDefault( f => f.Selected );

            if ( personId == null || personId < 1 || family == null )
            {
                return null;
            }

            return family.People.FirstOrDefault( p => p.Person.Id == personId );
        }

        /// <summary>
        /// Creates the people.
        /// </summary>
        /// <param name="serializedPeople">The new people list.</param>
        /// <returns></returns>
        private List<Person> CreatePeople( List<SerializedPerson> serializedPeople )
        {
            var newPeopleList = new List<Person>();
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            var connectionStatus = DefinedValueCache.Get( GetAttributeValue( "DefaultConnectionStatus" ).AsGuid(), rockContext );
            var personPhoneType = DefinedValueCache.Get( GetAttributeValue( "DefaultPhoneType" ).AsGuid(), rockContext );
            var activeRecord = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
            var personType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() );

            foreach ( SerializedPerson personData in serializedPeople )
            {
                var hasAbility = !string.IsNullOrWhiteSpace( personData.Ability ) && personData.AbilityGroup == "Ability";
                var hasGrade = !string.IsNullOrWhiteSpace( personData.Ability ) && personData.AbilityGroup == "Grade";

                var person = new Person
                {
                    FirstName = personData.FirstName,
                    LastName = personData.LastName,
                    SuffixValueId = personData.SuffixValueId,
                    Gender = personData.Gender,
                    Email = personData.Email
                };

                if ( personData.BirthDate != null )
                {
                    person.BirthDay = ( (DateTime)personData.BirthDate ).Day;
                    person.BirthMonth = ( (DateTime)personData.BirthDate ).Month;
                    person.BirthYear = ( (DateTime)personData.BirthDate ).Year;
                }

                if ( connectionStatus != null )
                {
                    person.ConnectionStatusValueId = connectionStatus.Id;
                }

                if ( activeRecord != null )
                {
                    person.RecordStatusValueId = activeRecord.Id;
                }

                if ( personType != null )
                {
                    person.RecordTypeValueId = personType.Id;
                }

                if ( hasGrade )
                {
                    person.GradeOffset = personData.Ability.AsIntegerOrNull();
                }

                if ( !string.IsNullOrWhiteSpace( personData.PhoneNumber ) )
                {
                    var countryCodes = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.COMMUNICATION_PHONE_COUNTRY_CODE.AsGuid() ).DefinedValues;
                    person.PhoneNumbers.Add( new PhoneNumber
                    {
                        CountryCode = countryCodes.Select( v => v.Value ).FirstOrDefault(),
                        NumberTypeValueId = personPhoneType.Id,
                        Number = personData.PhoneNumber,
                        IsSystem = false,
                        IsMessagingEnabled = true
                    } );
                }

                // Add the person so we can assign an ability (if set)
                personService.Add( person );

                if ( hasAbility || personData.HasSpecialNeeds )
                {
                    person.LoadAttributes( rockContext );

                    if ( hasAbility )
                    {
                        person.SetAttributeValue( "AbilityLevel", personData.Ability );
                        person.SaveAttributeValues( rockContext );
                    }

                    if ( personData.HasSpecialNeeds )
                    {
                        person.SetAttributeValue( SpecialNeedsKey, "Yes" );
                    }

                    person.SaveAttributeValues( rockContext );
                }

                newPeopleList.Add( person );
            }

            rockContext.SaveChanges();
            return newPeopleList;
        }

        /// <summary>
        /// Adds the group member.
        /// </summary>
        /// <param name="familyGroup">The family group.</param>
        /// <param name="newPeople">The new people.</param>
        /// <param name="barcode">The barcode.</param>
        /// <returns></returns>
        private Group AddGroupMembers( Group familyGroup, List<Person> newPeople, string barcode = null )
        {
            var rockContext = new RockContext();
            var familyGroupType = GroupTypeCache.GetFamilyGroupType();

            // Create a new family group if one doesn't exist
            if ( familyGroup == null )
            {
                familyGroup = new Group
                {
                    GroupTypeId = familyGroupType.Id,
                    IsSecurityRole = false,
                    IsSystem = false,
                    IsPublic = true,
                    IsActive = true,
                    CampusId = KioskCampusId
                };

                // Get oldest person's last name
                var familyName = newPeople.Where( p => p.BirthDate.HasValue )
                    .OrderByDescending( p => p.BirthDate )
                    .Select( p => p.LastName ).FirstOrDefault();

                familyGroup.Name = familyName + " Family";
                new GroupService( rockContext ).Add( familyGroup );
            }
            else if ( familyGroup.CampusId == null )
            {
                familyGroup.CampusId = KioskCampusId;
            }

            // Add group members
            var newGroupMembers = new List<GroupMember>();
            foreach ( var person in newPeople )
            {
                var groupMember = new GroupMember
                {
                    IsSystem = false,
                    IsNotified = false,
                    GroupId = familyGroup.Id,
                    PersonId = person.Id,
                    GroupMemberStatus = GroupMemberStatus.Active
                };

                if ( person.Age < 18 || person.AgeClassification == AgeClassification.Child )
                {
                    groupMember.GroupRoleId = familyGroupType.Roles.FirstOrDefault( r =>
                        r.Guid == new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD ) ).Id;
                }
                else
                {
                    groupMember.GroupRoleId = familyGroupType.Roles.FirstOrDefault( r =>
                        r.Guid == new Guid( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT ) ).Id;
                }

                newGroupMembers.Add( groupMember );
            }

            // New family group, save as part of tracked entity
            if ( familyGroup.Id == 0 )
            {
                familyGroup.Members = newGroupMembers;
            }
            else // use GroupMemberService to save to an existing group
            {
                new GroupMemberService( rockContext ).AddRange( newGroupMembers );
            }

            rockContext.SaveChanges();

            // save any barcodes entered during create
            if ( !string.IsNullOrWhiteSpace( tbBarcodes.Text ) )
            {
                var hoh = familyGroup.Members.AsQueryable().HeadOfHousehold();
                if ( hoh != null && hoh.PrimaryAlias != null )
                {
                    var hohBarcode = tbBarcodes.Text.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries );
                    var searchTypeValue = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid() );
                    var personSearchKeyService = new PersonSearchKeyService( rockContext );                             
                    foreach ( var value in hohBarcode )
                    {
                        var searchValue = new PersonSearchKey
                        {
                            PersonAliasId = hoh.PrimaryAlias.Id,
                            SearchTypeValueId = searchTypeValue.Id,
                            SearchValue = value
                        };
                        personSearchKeyService.Add( searchValue );
                    }

                    rockContext.SaveChanges();
                }
            }

            return familyGroup;
        }

        /// <summary>
        /// Adds the visitor / checkin relationship to adults in the family.
        /// </summary>
        /// <param name="family">The family.</param>
        /// <param name="visitorId">The person id.</param>
        /// <param name="rockContext">The rock context.</param>
        private void AddVisitorRelationships( CheckInFamily family, int visitorId, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            foreach ( var familyMember in family.People.Where( p => p.FamilyMember && ( p.Person.Age > 18 || p.Person.AgeClassification == AgeClassification.Adult ) ) )
            {
                Person.CreateCheckinRelationship( familyMember.Person.Id, visitorId, rockContext );
            }
        }

        #endregion Internal Methods

        #region NewPerson Class

        /// <summary>
        /// Lightweight Person model to serialize people to viewstate
        /// </summary>
        [Serializable()]
        protected class SerializedPerson
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public int? SuffixValueId { get; set; }

            public DateTime? BirthDate { get; set; }

            public Gender Gender { get; set; }

            public string Ability { get; set; }

            public string AbilityGroup { get; set; }

            public string PhoneNumber { get; set; }

            public string Email { get; set; }

            public bool HasSpecialNeeds { get; set; }

            public bool IsValid()
            {
                return !string.IsNullOrWhiteSpace( FirstName ) && !string.IsNullOrWhiteSpace( LastName ) && Gender != Gender.Unknown;
            }

            public SerializedPerson()
            {
                FirstName = string.Empty;
                LastName = string.Empty;
                BirthDate = new DateTime?();
                Gender = new Gender();
                Ability = string.Empty;
                AbilityGroup = string.Empty;
                PhoneNumber = string.Empty;
                Email = string.Empty;
                HasSpecialNeeds = false;
            }
        }

        #endregion NewPerson Class
    }
}
