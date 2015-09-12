// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm.PersonDetail
{
    /// <summary>
    /// Block for adding new families
    /// </summary>
    [DisplayName( "Add Family" )]
    [Category( "CRM > Person Detail" )]
    [Description( "Allows for adding new families." )]

    [GroupLocationTypeField( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, "Location Type",
        "The type of location that address should use", false, Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME, "", 0 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Default Connection Status",
        "The connection status that should be set by default", false, false, Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR, "", 1 )]
    [BooleanField( "Gender", "Require a gender for each person", "Don't require", "Should Gender be required for each person added?", false, "", 2 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS, "Adult Marital Status", "The default marital status for adults in the family.", false, false, "", "", 3 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS, "Child Marital Status", "The marital status to use for children in the family.", false, false,
        Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_SINGLE, "", 4 )]
    [BooleanField( "Marital Status Confirmation", "Should user be asked to confirm saving an adults without a marital status?", true, "", 5 )]
    [BooleanField( "Grade", "Require a grade for each child", "Don't require", "Should Grade be required for each child added?", false, "", 6 )]
    [BooleanField("SMS", "SMS is enabled by default", "SMS is not enabled by default", "Should SMS be enabled for cell phone numbers by default?", false, "", 7)]
    [AttributeCategoryField( "Attribute Categories", "The Attribute Categories to display attributes from", true, "Rock.Model.Person", false, "", "", 8 )]

    public partial class AddFamily : Rock.Web.UI.RockBlock
    {

        #region Fields

        private bool _confirmMaritalStatus = true;
        private int _childRoleId = 0;
        private List<NewFamilyAttributes> attributeControls = new List<NewFamilyAttributes>();
        private DefinedValueCache _homePhone = null;
        private DefinedValueCache _cellPhone = null;
        private bool _SMSEnabled = false;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the index of the current category.
        /// </summary>
        /// <value>
        /// The index of the current category.
        /// </value>
        protected int CurrentPageIndex { get; set; }

        /// <summary>
        /// Gets or sets the family members that have been added by user
        /// </summary>
        /// <value>
        /// The family members.
        /// </value>
        protected List<GroupMember> FamilyMembers { get; set; }

        /// <summary>
        /// Gets or sets any possible duplicates for each family member
        /// </summary>
        /// <value>
        /// The duplicates.
        /// </value>
        protected Dictionary<Guid, List<Person>> Duplicates { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            CurrentPageIndex = ViewState["CurrentPageIndex"] as int? ?? 0;

            string json = ViewState["FamilyMembers"] as string;
            if ( string.IsNullOrWhiteSpace( json ))
            {
                FamilyMembers = new List<GroupMember>();
            }
            else
            {
                FamilyMembers = JsonConvert.DeserializeObject<List<GroupMember>>( json );
            }

            json = ViewState["Duplicates"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                Duplicates = new Dictionary<Guid, List<Person>>();
            }
            else
            {
                Duplicates = JsonConvert.DeserializeObject<Dictionary<Guid, List<Person>>>( json );
            }

            CreateControls( false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            ddlMaritalStatus.BindToDefinedType( DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid() ), true );
            var AdultMaritalStatus = DefinedValueCache.Read( GetAttributeValue( "AdultMaritalStatus" ).AsGuid() );
            if ( AdultMaritalStatus != null )
            {
                ddlMaritalStatus.SetValue( AdultMaritalStatus.Id );
            }

            var campusi = CampusCache.All();
            cpCampus.Campuses = campusi;
            cpCampus.Visible = campusi.Any();
            if ( campusi.Count == 1 )
            {
                cpCampus.SelectedCampusId = campusi.FirstOrDefault().Id;
            }

            var familyGroupType = GroupTypeCache.GetFamilyGroupType();
            if ( familyGroupType != null )
            {
                _childRoleId = familyGroupType.Roles
                    .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ) )
                    .Select( r => r.Id )
                    .FirstOrDefault();
            }

            nfmMembers.RequireGender = GetAttributeValue( "Gender" ).AsBoolean();
            nfmMembers.RequireGrade = GetAttributeValue( "Grade" ).AsBoolean();
            _SMSEnabled = GetAttributeValue("SMS").AsBoolean();

            lTitle.Text = ( "Add Family" ).FormatAsHtmlTitle();

            _homePhone = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME );
            _cellPhone = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE );

            _confirmMaritalStatus = GetAttributeValue( "MaritalStatusConfirmation" ).AsBoolean();
            if ( _confirmMaritalStatus )
            {
                string script = string.Format( @"
    $('a.js-confirm-marital-status').click(function( e ){{

        var anyAdults = false;
        $(""input[id$='_rblRole_0']"").each(function() {{
            if ( $(this).prop('checked') ) {{
                anyAdults = true;
            }}
        }});

        if ( anyAdults ) {{
            if ( $('#{0}').val() == '' ) {{
                e.preventDefault();
                Rock.dialogs.confirm('You have not selected a marital status for the adults in this new family. Are you sure you want to continue?', function (result) {{
                    if (result) {{
                        window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                    }}
                }});
            }}
        }}
    }});
", ddlMaritalStatus.ClientID );
                ScriptManager.RegisterStartupScript( btnNext, btnNext.GetType(), "confirm-marital-status", script, true );

            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                FamilyMembers = new List<GroupMember>();
                Duplicates = new Dictionary<Guid, List<Person>>();
                AddFamilyMember();
                CreateControls( true );
            }
            else
            {
                GetControlData();
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["CurrentPageIndex"] = CurrentPageIndex;
            ViewState["FamilyMembers"] = JsonConvert.SerializeObject( FamilyMembers, Formatting.None, jsonSetting );
            ViewState["Duplicates"] = JsonConvert.SerializeObject( Duplicates, Formatting.None, jsonSetting );

            return base.SaveViewState();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            var adults = FamilyMembers.Where( m => m.GroupRoleId != _childRoleId ).ToList();
            ddlMaritalStatus.Visible = adults.Any();

            base.OnPreRender( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the AddFamilyMemberClick event of the nfmMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void nfmMembers_AddFamilyMemberClick( object sender, EventArgs e )
        {
            AddFamilyMember();
            CreateControls( true );
        }

        /// <summary>
        /// Handles the RoleUpdated event of the familyMemberRow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void familyMemberRow_RoleUpdated( object sender, EventArgs e )
        {
            NewFamilyMembersRow row = sender as NewFamilyMembersRow;
            row.ShowGrade = row.RoleId == _childRoleId;
        }

        /// <summary>
        /// Handles the DeleteClick event of the familyMemberRow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void familyMemberRow_DeleteClick( object sender, EventArgs e )
        {
            NewFamilyMembersRow row = sender as NewFamilyMembersRow;
            var familyMember = FamilyMembers.FirstOrDefault( m => m.Person.Guid.Equals( row.PersonGuid ) );
            if ( familyMember != null )
            {
                FamilyMembers.Remove( familyMember );
            }

            CreateControls( true );
        }

        protected void btnPrevious_Click( object sender, EventArgs e )
        {
            if ( CurrentPageIndex > 0 )
            {
                CurrentPageIndex--;
                CreateControls( true );
            }
        }

        protected void btnNext_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                if ( CurrentPageIndex < ( attributeControls.Count + 1 ) )
                {
                    CurrentPageIndex++;
                    CreateControls( true );
                }
                else
                {
                    if ( FamilyMembers.Any() )
                    {
                        if ( CurrentPageIndex == ( attributeControls.Count + 1 ) && FindDuplicates() )
                        {
                            CurrentPageIndex++;
                            CreateControls( true );
                        }
                        else
                        {
                            var rockContext = new RockContext();
                            rockContext.WrapTransaction( () =>
                            {
                                var familyGroup = GroupService.SaveNewFamily( rockContext, FamilyMembers, cpCampus.SelectedValueAsInt(), true );
                                if ( familyGroup != null )
                                {
                                    GroupService.AddNewFamilyAddress( rockContext, familyGroup, GetAttributeValue( "LocationType" ),
                                        acAddress.Street1, acAddress.Street2, acAddress.City, acAddress.State, acAddress.PostalCode, acAddress.Country );
                                }
                            } );

                            Response.Redirect( string.Format( "~/Person/{0}", FamilyMembers[0].Person.Id ), false );
                        }
                    }
                }
            }
        }

        #endregion

        #region Methods

        private void CreateControls( bool setSelection )
        {
            // Load all the attribute controls
            attributeControls.Clear();
            pnlAttributes.Controls.Clear();
            phDuplicates.Controls.Clear();

            var rockContext = new RockContext();
            var attributeService = new AttributeService( rockContext );
            var locationService = new LocationService( rockContext );

            foreach ( string categoryGuid in GetAttributeValue( "AttributeCategories" ).SplitDelimitedValues( false ) )
            {
                Guid guid = Guid.Empty;
                if ( Guid.TryParse( categoryGuid, out guid ) )
                {
                    var category = CategoryCache.Read( guid );
                    if ( category != null )
                    {
                        var attributeControl = new NewFamilyAttributes();
                        attributeControl.ClearRows();
                        pnlAttributes.Controls.Add( attributeControl );
                        attributeControls.Add( attributeControl );
                        attributeControl.ID = "familyAttributes_" + category.Id.ToString();
                        attributeControl.CategoryId = category.Id;

                        foreach ( var attribute in attributeService.GetByCategoryId( category.Id ) )
                        {
                            if ( attribute.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                            {
                                attributeControl.AttributeList.Add( AttributeCache.Read( attribute ) );
                            }
                        }
                    }
                }
            }

            nfmMembers.ClearRows();
            nfciContactInfo.ClearRows();

            var groupMemberService = new GroupMemberService(rockContext);
            var familyGroupType = GroupTypeCache.GetFamilyGroupType();
            int adultRoleId = familyGroupType.Roles.First( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;
            var homeLocationGuid = Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid();

            var location = locationService.Get( acAddress.Street1, acAddress.Street2, acAddress.City, acAddress.State, acAddress.PostalCode, acAddress.Country );

            foreach ( var familyMember in FamilyMembers )
            {
                string familyMemberGuidString = familyMember.Person.Guid.ToString().Replace( "-", "_" );

                var familyMemberRow = new NewFamilyMembersRow();
                nfmMembers.Controls.Add( familyMemberRow );
                familyMemberRow.ID = string.Format( "row_{0}", familyMemberGuidString );
                familyMemberRow.RoleUpdated += familyMemberRow_RoleUpdated;
                familyMemberRow.DeleteClick += familyMemberRow_DeleteClick;
                familyMemberRow.PersonGuid = familyMember.Person.Guid;
                familyMemberRow.RequireGender = nfmMembers.RequireGender;
                familyMemberRow.RequireGrade = nfmMembers.RequireGrade;
                familyMemberRow.RoleId = familyMember.GroupRoleId;
                familyMemberRow.ShowGrade = familyMember.GroupRoleId == _childRoleId;
                familyMemberRow.ValidationGroup = BlockValidationGroup;

                var contactInfoRow = new NewFamilyContactInfoRow();
                nfciContactInfo.Controls.Add( contactInfoRow );
                contactInfoRow.ID = string.Format( "ci_row_{0}", familyMemberGuidString );
                contactInfoRow.PersonGuid = familyMember.Person.Guid;
                contactInfoRow.IsMessagingEnabled = _SMSEnabled;
                contactInfoRow.PersonName = familyMember.Person.FullName;

                if ( _homePhone != null )
                {
                    var homePhoneNumber = familyMember.Person.PhoneNumbers.Where( p => p.NumberTypeValueId == _homePhone.Id ).FirstOrDefault();
                    if ( homePhoneNumber != null )
                    {
                        contactInfoRow.HomePhoneNumber = PhoneNumber.FormattedNumber( homePhoneNumber.CountryCode, homePhoneNumber.Number );
                        contactInfoRow.HomePhoneCountryCode = homePhoneNumber.CountryCode;
                    }
                }

                if ( _cellPhone != null )
                {
                    var cellPhoneNumber = familyMember.Person.PhoneNumbers.Where( p => p.NumberTypeValueId == _cellPhone.Id ).FirstOrDefault();
                    if ( cellPhoneNumber != null )
                    {
                        contactInfoRow.CellPhoneNumber = PhoneNumber.FormattedNumber( cellPhoneNumber.CountryCode, cellPhoneNumber.Number );
                        contactInfoRow.CellPhoneCountryCode = cellPhoneNumber.CountryCode;                     
                    }
                }

                contactInfoRow.Email = familyMember.Person.Email;

                if ( setSelection )
                {
                    if ( familyMember.Person != null )
                    {
                        familyMemberRow.TitleValueId = familyMember.Person.TitleValueId;
                        familyMemberRow.FirstName = familyMember.Person.FirstName;
                        familyMemberRow.LastName = familyMember.Person.LastName;
                        familyMemberRow.SuffixValueId = familyMember.Person.SuffixValueId;
                        familyMemberRow.Gender = familyMember.Person.Gender;
                        familyMemberRow.BirthDate = familyMember.Person.BirthDate;
                        familyMemberRow.ConnectionStatusValueId = familyMember.Person.ConnectionStatusValueId;
                        familyMemberRow.GradeOffset = familyMember.Person.GradeOffset;
                    }
                }

                foreach ( var attributeControl in attributeControls )
                {
                    var attributeRow = new NewFamilyAttributesRow();
                    attributeControl.Controls.Add( attributeRow );
                    attributeRow.ID = string.Format( "{0}_{1}", attributeControl.ID, familyMemberGuidString );
                    attributeRow.AttributeList = attributeControl.AttributeList;
                    attributeRow.PersonGuid = familyMember.Person.Guid;
                    attributeRow.PersonName = familyMember.Person.FullName;

                    if ( setSelection )
                    {
                        attributeRow.SetEditValues( familyMember.Person );
                    }
                }

                if ( Duplicates.ContainsKey( familyMember.Person.Guid ) )
                {
                    var dupRow = new HtmlGenericControl( "div" );
                    dupRow.AddCssClass( "row" );
                    dupRow.ID = string.Format( "dupRow_{0}", familyMemberGuidString );
                    phDuplicates.Controls.Add( dupRow );

                    var newPersonCol = new HtmlGenericControl( "div" );
                    newPersonCol.AddCssClass( "col-md-6" );
                    newPersonCol.ID = string.Format( "newPersonCol_{0}", familyMemberGuidString );
                    dupRow.Controls.Add( newPersonCol );

                    newPersonCol.Controls.Add( PersonHtmlPanel(
                        familyMemberGuidString,
                        familyMember.Person,
                        familyMember.GroupRole,
                        location,
                        rockContext ) );

                    LinkButton lbRemoveMember = new LinkButton();
                    lbRemoveMember.ID = string.Format( "lbRemoveMember_{0}", familyMemberGuidString );
                    lbRemoveMember.AddCssClass( "btn btn-danger btn-xs" );
                    lbRemoveMember.Text = "Remove";
                    lbRemoveMember.Click += lbRemoveMember_Click;
                    newPersonCol.Controls.Add( lbRemoveMember );

                    var dupPersonCol = new HtmlGenericControl( "div" );
                    dupPersonCol.AddCssClass( "col-md-6" );
                    dupPersonCol.ID = string.Format( "dupPersonCol_{0}", familyMemberGuidString );
                    dupRow.Controls.Add( dupPersonCol );

                    var duplicateHeader = new HtmlGenericControl( "h4" );
                    duplicateHeader.InnerText = "Possible Duplicate Records";
                    dupPersonCol.Controls.Add( duplicateHeader );

                    foreach( var duplicate in Duplicates[familyMember.Person.Guid] )
                    {
                        GroupTypeRole groupTypeRole = null;
                        Location duplocation = null;

                        var familyGroupMember = groupMemberService.Queryable()
                            .Where( a => a.PersonId == duplicate.Id )
                            .Where( a => a.Group.GroupTypeId == familyGroupType.Id )
                            .Select( s => new
                            {
                                s.GroupRole,
                                GroupLocation = s.Group.GroupLocations.Where( a => a.GroupLocationTypeValue.Guid == homeLocationGuid ).Select( a => a.Location ).FirstOrDefault()
                            } )
                            .FirstOrDefault();
                        if ( familyGroupMember != null )
                        {
                            groupTypeRole = familyGroupMember.GroupRole;
                            duplocation = familyGroupMember.GroupLocation;
                        }

                        dupPersonCol.Controls.Add( PersonHtmlPanel(
                            familyMemberGuidString,
                            duplicate,
                            groupTypeRole,
                            duplocation,
                            rockContext ) );
                    }
                }
            }

            ShowPage();
        }

        void lbRemoveMember_Click( object sender, EventArgs e )
        {
            Guid personGuid = ( (LinkButton)sender ).ID.Substring( 15 ).Replace( "_", "-" ).AsGuid();
            var familyMember = FamilyMembers.Where( f => f.Person.Guid.Equals( personGuid ) ).FirstOrDefault();
            if ( familyMember != null )
            {
                FamilyMembers.Remove( familyMember );
                Duplicates.Remove( personGuid );
                if ( !FamilyMembers.Any() )
                {
                    AddFamilyMember();
                    CurrentPageIndex = 0;
                }
                CreateControls( true );
            }
        }

        private Panel PersonHtmlPanel( 
            string familyMemberGuidString, 
            Person person,
            GroupTypeRole GroupTypeRole,
            Location location,
            RockContext rockContext )
        {
            var personInfoHtml = new StringBuilder();

            Guid? recordTypeValueGuid = null;
            if ( person.RecordTypeValueId.HasValue )
            {
                recordTypeValueGuid = DefinedValueCache.Read( person.RecordTypeValueId.Value, rockContext ).Guid;
            }

            string personName = string.Format("{0} <small>(New Record)</small>", person.FullName);
            if ( person.Id > 0 )
            {
                string personUrl = ResolveRockUrl( string.Format( "~/person/{0}", person.Id ) );
                personName = string.Format( "<a href='{0}' target='_blank'>{1}</a>", personUrl, person.FullName );
            }

            personInfoHtml.Append( "<div class='row margin-b-lg'>" );

            // add photo if it's not the new record
            if ( person.Id > 0 )
            {
                personInfoHtml.Append( "<div class='col-md-2'>" );
                if ( person.PhotoId.HasValue )
                {
                    personInfoHtml.AppendFormat(
                        "<img src='{0}'>",
                        Person.GetPhotoUrl( person.PhotoId.Value, person.Age, person.Gender, recordTypeValueGuid, 65, 65 ) );
                }
                personInfoHtml.Append( "</div>" );
            }

            personInfoHtml.Append( "<div class='col-md-10'>" );
            personInfoHtml.AppendFormat( "<h4 class='margin-t-none'>{0}</h4>", personName );

            if ( GroupTypeRole != null )
            { 
                personInfoHtml.Append( GroupTypeRole.Name );
            }

            int? personAge = person.Age;
            if ( personAge.HasValue )
            {
                personInfoHtml.AppendFormat( " <em>({0} yrs old)</em>", personAge.Value ); ;
            }

            var familyMembers = person.GetFamilyMembers( false, rockContext );
            if ( familyMembers != null && familyMembers.Any() )
            {
                personInfoHtml.AppendFormat( "<p><strong>Family Members:</strong> {0}",
                    familyMembers.Select( m => m.Person.NickName ).ToList().AsDelimited( ", " ) );
            }

            if ( location != null )
            {
                personInfoHtml.AppendFormat( "<p><strong>Address</strong><br/>{0}</p>", location.GetFullStreetAddress().ConvertCrLfToHtmlBr() );
            }

            // Generate the HTML for Email and PhoneNumbers
            if ( !string.IsNullOrWhiteSpace( person.Email ) || person.PhoneNumbers.Any() )
            {
                string emailAndPhoneHtml = "<p class='margin-t-sm'>";
                emailAndPhoneHtml += person.Email;
                string phoneNumberList = string.Empty;
                foreach ( var phoneNumber in person.PhoneNumbers )
                {
                    var phoneType = DefinedValueCache.Read( phoneNumber.NumberTypeValueId ?? 0, rockContext );
                    phoneNumberList += string.Format(
                        "<br>{0} <small>{1}</small>",
                        phoneNumber.IsUnlisted ? "Unlisted" : phoneNumber.NumberFormatted,
                        phoneType != null ? phoneType.Value : string.Empty );
                }

                emailAndPhoneHtml += phoneNumberList + "<p>";

                personInfoHtml.Append( emailAndPhoneHtml );
            }

            personInfoHtml.Append( "</div>" );
            personInfoHtml.Append( "</div>" );

            var dupPersonPnl = new Panel();
            dupPersonPnl.ID = string.Format( "dupPersonPnl_{0}_{1}", familyMemberGuidString, person.Id );
            dupPersonPnl.Controls.Add( new LiteralControl( personInfoHtml.ToString() ) );

            return dupPersonPnl;
        }

        private void GetControlData()
        {
            FamilyMembers = new List<GroupMember>();

            int? childMaritalStatusId = null;
            var childMaritalStatus = DefinedValueCache.Read( GetAttributeValue( "ChildMaritalStatus" ).AsGuid() );
            if ( childMaritalStatus != null )
            {
                childMaritalStatusId = childMaritalStatus.Id;
            }
            int? adultMaritalStatusId = ddlMaritalStatus.SelectedValueAsInt();

            int recordTypePersonId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
            int recordStatusActiveId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;

            foreach ( NewFamilyMembersRow row in nfmMembers.FamilyMemberRows )
            {
                var groupMember = new GroupMember();
                groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                groupMember.Person = new Person();
                groupMember.Person.Guid = row.PersonGuid.Value;
                groupMember.Person.RecordTypeValueId = recordTypePersonId;
                groupMember.Person.RecordStatusValueId = recordStatusActiveId;

                if ( row.RoleId.HasValue )
                {
                    groupMember.GroupRoleId = row.RoleId.Value;

                    if ( groupMember.GroupRoleId == _childRoleId )
                    {
                        groupMember.Person.MaritalStatusValueId = childMaritalStatusId;
                    }
                    else
                    {
                        groupMember.Person.MaritalStatusValueId = adultMaritalStatusId;
                    }
                }

                groupMember.Person.TitleValueId = row.TitleValueId;
                groupMember.Person.FirstName = row.FirstName;
                groupMember.Person.NickName = groupMember.Person.FirstName;
                groupMember.Person.LastName = row.LastName;
                groupMember.Person.SuffixValueId = row.SuffixValueId;
                groupMember.Person.Gender = row.Gender;

                var birthday = row.BirthDate;
                if ( birthday.HasValue )
                {
                    // If setting a future birthdate, subtract a century until birthdate is not greater than today.
                    var today = RockDateTime.Today;
                    while ( birthday.Value.CompareTo( today ) > 0 )
                    {
                        birthday = birthday.Value.AddYears( -100 );
                    }
                    groupMember.Person.BirthMonth = birthday.Value.Month;
                    groupMember.Person.BirthDay = birthday.Value.Day;
                    if ( birthday.Value.Year != DateTime.MinValue.Year )
                    {
                        groupMember.Person.BirthYear = birthday.Value.Year;
                    }
                    else
                    {
                        groupMember.Person.BirthYear = null;
                    }
                }
                else
                {
                    groupMember.Person.SetBirthDate( null );
                }

                groupMember.Person.ConnectionStatusValueId = row.ConnectionStatusValueId;
                groupMember.Person.GradeOffset = row.GradeOffset;

                NewFamilyContactInfoRow contactInfoRow = nfciContactInfo.ContactInfoRows.FirstOrDefault( c => c.PersonGuid == row.PersonGuid );
                if ( contactInfoRow != null )
                {
                    string homeNumber = PhoneNumber.CleanNumber( contactInfoRow.HomePhoneNumber );
                    if ( _homePhone != null && !string.IsNullOrWhiteSpace( homeNumber ) )
                    {
                        var homePhoneNumber = new PhoneNumber();
                        homePhoneNumber.NumberTypeValueId = _homePhone.Id;
                        homePhoneNumber.Number = homeNumber;
                        homePhoneNumber.CountryCode = PhoneNumber.CleanNumber( contactInfoRow.HomePhoneCountryCode );
                        homePhoneNumber.NumberFormatted = PhoneNumber.FormattedNumber( homePhoneNumber.CountryCode, homeNumber );
                        groupMember.Person.PhoneNumbers.Add( homePhoneNumber );
                    }

                    string cellNumber = PhoneNumber.CleanNumber( contactInfoRow.CellPhoneNumber );
                    if ( _cellPhone != null && !string.IsNullOrWhiteSpace( cellNumber ) )
                    {
                        var cellPhoneNumber = new PhoneNumber();
                        cellPhoneNumber.NumberTypeValueId = _cellPhone.Id;
                        cellPhoneNumber.Number = cellNumber;
                        cellPhoneNumber.CountryCode = PhoneNumber.CleanNumber( contactInfoRow.CellPhoneCountryCode );
                        cellPhoneNumber.NumberFormatted = PhoneNumber.FormattedNumber( cellPhoneNumber.CountryCode, cellNumber );
                        cellPhoneNumber.IsMessagingEnabled = contactInfoRow.IsMessagingEnabled;
                        groupMember.Person.PhoneNumbers.Add( cellPhoneNumber );
                    }

                    groupMember.Person.Email = contactInfoRow.Email;
                }
                groupMember.Person.IsEmailActive = true;
                groupMember.Person.EmailPreference = EmailPreference.EmailAllowed;

                groupMember.Person.LoadAttributes();

                foreach ( var attributeControl in attributeControls )
                {
                    NewFamilyAttributesRow attributeRow = attributeControl.AttributesRows.FirstOrDefault( r => r.PersonGuid == row.PersonGuid );
                    if ( attributeRow != null )
                    {
                        attributeRow.GetEditValues( groupMember.Person );
                    }
                }

                FamilyMembers.Add( groupMember );
            }
        }

        private void AddFamilyMember()
        {
            var familyGroupType = GroupTypeCache.GetFamilyGroupType();
            if ( familyGroupType != null && familyGroupType.DefaultGroupRoleId != null )
            {
                int recordTypePersonId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                int recordStatusActiveId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
                var ConnectionStatusValue = DefinedValueCache.Read( GetAttributeValue( "DefaultConnectionStatus" ).AsGuid() );

                var person = new Person();
                person.Guid = Guid.NewGuid();
                person.RecordTypeValueId = recordTypePersonId;
                person.RecordStatusValueId = recordStatusActiveId;
                person.Gender = Gender.Unknown;
                person.ConnectionStatusValueId = ( ConnectionStatusValue != null ) ? ConnectionStatusValue.Id : (int?)null;

                var familyMember = new GroupMember();
                familyMember.GroupMemberStatus = GroupMemberStatus.Active;
                familyMember.GroupRoleId = familyGroupType.DefaultGroupRoleId.Value;
                familyMember.Person = person;

                FamilyMembers.Add( familyMember );
            }
        }

        public bool FindDuplicates()
        {
            Duplicates = new Dictionary<Guid, List<Person>>();

            var rockContext = new RockContext();
            var locationService = new LocationService( rockContext );
            var groupService = new GroupService( rockContext );
            var personService = new PersonService( rockContext );

            // Find any other family members (any family) that have same location
            var othersAtAddress = new List<int>();
            var familyGroupType = GroupTypeCache.GetFamilyGroupType();
            var location = locationService.Get( acAddress.Street1, acAddress.Street2, acAddress.City, acAddress.State, acAddress.PostalCode, acAddress.Country );
            if ( location != null )
            {
                othersAtAddress = groupService
                    .Queryable().AsNoTracking()
                    .Where( g =>
                        g.GroupTypeId == familyGroupType.Id &&
                        g.GroupLocations.Any( l => l.LocationId == location.Id ) )
                    .SelectMany( g => g.Members )
                    .Select( m => m.PersonId )
                    .ToList();
            }

            foreach ( var person in FamilyMembers
                .Where( m =>
                    m.Person != null &&
                    m.Person.FirstName != "" )
                .Select( m => m.Person ) )
            {
                bool otherCriteria = false;
                var personQry = personService
                    .Queryable().AsNoTracking()
                    .Where( p =>
                        p.FirstName == person.FirstName ||
                        p.NickName == person.FirstName );

                if ( othersAtAddress.Any() )
                {
                    personQry = personQry
                        .Where( p => othersAtAddress.Contains( p.Id ) );
                }

                if ( person.BirthDate.HasValue )
                {
                    otherCriteria = true;
                    personQry = personQry
                        .Where( p =>
                            p.BirthDate.HasValue &&
                            p.BirthDate.Value == person.BirthDate.Value );
                }

                if ( _homePhone != null )
                {
                    var homePhoneNumber = person.PhoneNumbers.Where( p => p.NumberTypeValueId == _homePhone.Id ).FirstOrDefault();
                    if ( homePhoneNumber != null )
                    {
                        otherCriteria = true;
                        personQry = personQry
                            .Where( p =>
                                p.PhoneNumbers.Any( n =>
                                    n.NumberTypeValueId == _homePhone.Id &&
                                    n.Number == homePhoneNumber.Number ) );
                    }
                }

                if ( _cellPhone != null )
                {
                    var cellPhoneNumber = person.PhoneNumbers.Where( p => p.NumberTypeValueId == _cellPhone.Id ).FirstOrDefault();
                    if ( cellPhoneNumber != null )
                    {
                        otherCriteria = true;
                        personQry = personQry
                            .Where( p =>
                                p.PhoneNumbers.Any( n =>
                                    n.NumberTypeValueId == _cellPhone.Id &&
                                    n.Number == cellPhoneNumber.Number ) );
                    }
                }

                if ( !string.IsNullOrWhiteSpace( person.Email ) )
                {
                    otherCriteria = true;
                    personQry = personQry
                        .Where( p => p.Email == person.Email );
                }

                var dups = new List<Person>();
                if ( otherCriteria )
                {
                    // If a birthday, email, phone, or address was entered, find anyone with same info and same first name
                    dups = personQry.ToList();
                }
                else
                {
                    // otherwise find people with same first and last name
                    dups = personQry
                        .Where( p => p.LastName == person.LastName )
                        .ToList();

                }
                if ( dups.Any() )
                {
                    Duplicates.Add( person.Guid, dups );
                }

            }

            return Duplicates.Any();
        }

        private void ShowPage()
        {
            pnlFamilyData.Visible = ( CurrentPageIndex == 0 );
            pnlContactInfo.Visible = ( CurrentPageIndex == 1 );
            pnlAttributes.Visible = CurrentPageIndex > 1 && CurrentPageIndex <= attributeControls.Count + 1;
            pnlDuplicateWarning.Visible = CurrentPageIndex > attributeControls.Count + 1;

            attributeControls.ForEach( c => c.Visible = false );
            if ( CurrentPageIndex > 1 && attributeControls.Count >= ( CurrentPageIndex - 1 ) )
            {
                attributeControls[CurrentPageIndex - 2].Visible = true;
            }

            if ( _confirmMaritalStatus && CurrentPageIndex == 0 )
            {
                btnNext.AddCssClass( "js-confirm-marital-status" );
            }
            else
            {
                btnNext.RemoveCssClass( "js-confirm-marital-status" );
            }

            btnPrevious.Visible = CurrentPageIndex > 0;
            btnNext.Text = CurrentPageIndex > attributeControls.Count ?
                ( CurrentPageIndex > ( attributeControls.Count + 1 ) ? "Confirm" : "Finish" ) : "Next";
        }

        #endregion
    }
}