// <copyright>
// Copyright by BEMA Information Technologies
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.CheckIn.Registration;
using Rock.Data;
using Rock.Model;
using Rock.Transactions;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

/* * BEMA Modified Core Block (v9.4)
 * Version Number based off of RockVersion.RockHotFixVersion.BemaFeatureVersion
 *
 * Additional Features:
 * - FE1) Added Ability to select a group attribute so that new families can be assigned to groups
 * - UI1) Made lots of things configurable and added support for Address entry
 */

/// <summary>
///
/// </summary>
namespace RockWeb.Plugins.com_bemaservices.Checkin
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "BEMA Check-In Edit Family" )]
    [Category( "BEMA Services > Check-in" )]
    [Description( "Block to Add or Edit a Family during the Check-in Process." )]

    /* BEMA.FE1.Start */
    [AttributeField (Rock.SystemGuid.EntityType.PERSON, "Check-In Group Attribute", "A person attribute specifying the check-in group to pass to the new person workflow.", false, false, "", "", 5)]
    [BooleanField ( "Only Display Active Groups", "Should the Check-In Groups selector only display currently active check-in groups?", false, "", 5 )]
    [BooleanField ( "Require Check-In Group for Child", "Should the Check-In Group be required for child?", false, "", 5 )]
    /* BEMA.FE1.End */
    /* BEMA.UI1.Start */
    [BooleanField ( "Require Adult in New Family", "When adding a family, is an adult required in the family?", false, "", 8 )]
    [BooleanField ( "Require Mobile for Adult", "When adding an adult, is Mobile Phone required?", true, "", 6 )]
    [BooleanField ( "Require Email for Adult", "When adding an adult, is email required?", true, "", 6 )]
    [BooleanField ( "Show Birthdate for Adult", "When adding an adult is Birthdate shown?", false, "", 6 )]
    [BooleanField ( "Require Birthdate for Adult", "When adding an adult is Birthdate required?", false, "", 6 )]
    [BooleanField ( "Require Birthdate for Child", "When adding a child, is Birthdate required?", true, "", 7 )]
    [BooleanField ( "Show Mobile for Child", "When adding a child, is Birthdate required?", false, "", 7 )]
    [BooleanField ( "Require Grade for Child", "When adding a child is Grade required?", false, "", 7 )]
    [BooleanField ( "Show Address for New Family", "When adding a family is Address shown?", false, "", 9 )]
    [BooleanField ( "Require Address for New Family", "When adding a family is Address required?", false, "", 9 )]
    [BooleanField ( "Show Marital Status for Adult", "When adding an adult is Birthdate shown?", true, "", 10 )]
    [DefinedValueField ( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS, "New Person Marital Status", "The Marital Status for new people.", false, false, Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_SINGLE, order:10 )]
    /* BEMA.UI1.End */
    /* BEMA.FE1.Start */
    [GroupTypesField ( "Group Types Exclude", "Select group types to exclude from this displaying.", false, key: "GroupTypesExclude", order: 11 )]
    /* BEMA.FE1.End */

    public partial class EditFamily : CheckInEditFamilyBlock
    {
        /* BEMA.Start */
        #region Bema Attribute Keys
        private static class BemaAttributeKey
        {
            public const string CheckInGroupAttribute = "Check-InGroupAttribute";
            public const string OnlyDisplayActiveGroups = "OnlyDisplayActiveGroups";
            public const string RequireCheckInGroupforChild = "RequireCheck-InGroupforChild";
            public const string RequireAdultInNewFamily = "RequireAdultinNewFamily";
            public const string RequireMobileForAdult = "RequireMobileforAdult";
            public const string RequireEmailForAdult = "RequireEmailforAdult";
            public const string ShowBirthdateForAdult = "ShowBirthdateforAdult";
            public const string RequireBirthdateForAdult = "RequireBirthdateforAdult";
            public const string RequireBirthdateForChild = "RequireBirthdateforChild";
            public const string ShowMobileForChild = "ShowMobileforChild";
            public const string RequireGradeForChild = "RequireGradeforChild";
            public const string ShowAddressForNewFamily = "ShowAddressforNewFamily";
            public const string RequireAddressForNewFamily = "RequireAddressforNewFamily";
            public const string ShowMaritalStatusForAdult = "ShowMaritalStatusforAdult";
            public const string NewPersonMaritalStatus = "NewPersonMaritalStatus";
            public const string GroupTypesExclude = "GroupTypesExclude";
        }
        #endregion
        /* BEMA.End */

        #region private fields

        /// <summary>
        /// A hash of EditFamilyState before any editing. Use this to determine if there were any changes
        /// </summary>
        /// <value>
        /// The initial state hash.
        /// </value>
        private int _initialEditFamilyStateHash
        {
            get
            {
                return ViewState["_initialEditFamilyStateHash"] as int? ?? 0;
            }
            set
            {
                ViewState["_initialEditFamilyStateHash"] = value;
            }
        }

        #endregion

        /// <summary>
        /// Gets or sets the state of the edit family.
        /// </summary>
        /// <value>
        /// The state of the edit family.
        /// </value>
        public FamilyRegistrationState EditFamilyState { get; set; }

        /// <summary>
        /// Gets or sets the required attributes for adults.
        /// </summary>
        /// <value>
        /// The required attributes for adults.
        /// </value>
        private List<AttributeCache> RequiredAttributesForAdults { get; set; }

        /// <summary>
        /// Gets or sets the optional attributes for adults.
        /// </summary>
        /// <value>
        /// The optional attributes for adults.
        /// </value>
        private List<AttributeCache> OptionalAttributesForAdults { get; set; }

        /// <summary>
        /// Gets or sets the required attributes for children.
        /// </summary>
        /// <value>
        /// The required attributes for children.
        /// </value>
        private List<AttributeCache> RequiredAttributesForChildren { get; set; }

        /// <summary>
        /// Gets or sets the optional attributes for children.
        /// </summary>
        /// <value>
        /// The optional attributes for children.
        /// </value>
        private List<AttributeCache> OptionalAttributesForChildren { get; set; }

        /// <summary>
        /// Gets or sets the required attributes for families.
        /// </summary>
        /// <value>
        /// The required attributes for families.
        /// </value>
        private List<AttributeCache> RequiredAttributesForFamilies { get; set; }

        /// <summary>
        /// Gets or sets the optional attributes for families.
        /// </summary>
        /// <value>
        /// The optional attributes for families.
        /// </value>
        private List<AttributeCache> OptionalAttributesForFamilies { get; set; }

        /// <summary>
        /// The group type role adult identifier
        /// </summary>
        private static int _groupTypeRoleAdultId = GroupTypeCache.GetFamilyGroupType().Roles.FirstOrDefault( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;

        /// <summary>
        /// The person record status active identifier
        /// </summary>
        private static int _personRecordStatusActiveId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;

        /// <summary>
        /// The person search alternate value identifier (barcode search key)
        /// </summary>
        private static int _personSearchAlternateValueId = DefinedValueCache.Get ( Rock.SystemGuid.DefinedValue.PERSON_SEARCH_KEYS_ALTERNATE_ID.AsGuid () ).Id;

        #region Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            if ( CurrentCheckInState == null )
            {
                return;
            }

            gFamilyMembers.DataKeyNames = new string[] { "GroupMemberGuid" };
            gFamilyMembers.GridRebind += gFamilyMembers_GridRebind;

            RequiredAttributesForAdults = CurrentCheckInState.CheckInType.Registration.RequiredAttributesForAdults.Where( a => a.IsAuthorized( Rock.Security.Authorization.EDIT, this.CurrentPerson ) ).ToList();
            OptionalAttributesForAdults = CurrentCheckInState.CheckInType.Registration.OptionalAttributesForAdults.Where( a => a.IsAuthorized( Rock.Security.Authorization.EDIT, this.CurrentPerson ) ).ToList();
            RequiredAttributesForChildren = CurrentCheckInState.CheckInType.Registration.RequiredAttributesForChildren.Where( a => a.IsAuthorized( Rock.Security.Authorization.EDIT, this.CurrentPerson ) ).ToList();
            OptionalAttributesForChildren = CurrentCheckInState.CheckInType.Registration.OptionalAttributesForChildren.Where( a => a.IsAuthorized( Rock.Security.Authorization.EDIT, this.CurrentPerson ) ).ToList();
            RequiredAttributesForFamilies = CurrentCheckInState.CheckInType.Registration.RequiredAttributesForFamilies.Where( a => a.IsAuthorized( Rock.Security.Authorization.EDIT, this.CurrentPerson ) ).ToList();
            OptionalAttributesForFamilies = CurrentCheckInState.CheckInType.Registration.OptionalAttributesForFamilies.Where( a => a.IsAuthorized( Rock.Security.Authorization.EDIT, this.CurrentPerson ) ).ToList();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( CurrentCheckInState == null )
            {
                NavigateToHomePage();
            }

            if ( this.IsPostBack )
            {
                // make sure the ShowAddFamilyPrompt is disabled so that it doesn't show again until explicitly enabled after doing a Search
                hfShowCancelEditPrompt.Value = "0";

                if ( this.Request.Params["__EVENTARGUMENT"] == "ConfirmCancelFamily" )
                {
                    CancelFamilyEdit( false );
                }
            }
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            if ( CurrentCheckInState == null )
            {
                return;
            }

            EditFamilyState = ( this.ViewState["EditFamilyState"] as string ).FromJsonOrNull<FamilyRegistrationState>();

            CreateDynamicFamilyControls( FamilyRegistrationState.FromGroup( new Group() { GroupTypeId = GroupTypeCache.GetFamilyGroupType().Id } ), false );

            CreateDynamicPersonControls( FamilyRegistrationState.FamilyPersonState.FromTemporaryPerson(), false );
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            this.ViewState["EditFamilyState"] = EditFamilyState.ToJson();
            return base.SaveViewState();
        }

        /// <summary>
        /// Creates the dynamic family controls.
        /// </summary>
        /// <param name="editFamilyState">State of the edit family.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void CreateDynamicFamilyControls( FamilyRegistrationState editFamilyState, bool setValues )
        {
            phFamilyAttributes.Controls.Clear();

            var fakeFamily = new Group() { GroupTypeId = GroupTypeCache.GetFamilyGroupType().Id };
            var attributeList = editFamilyState.FamilyAttributeValuesState.Select( a => AttributeCache.Get( a.Value.AttributeId ) ).ToList();
            fakeFamily.Attributes = attributeList.ToDictionary( a => a.Key, v => v );
            fakeFamily.AttributeValues = editFamilyState.FamilyAttributeValuesState;
            var familyAttributeKeysToEdit = this.RequiredAttributesForFamilies.OrderBy( a => a.Order ).Select( a => a.Key ).ToList();
            familyAttributeKeysToEdit.AddRange( this.OptionalAttributesForFamilies.OrderBy( a => a.Order ).Select( a => a.Key ).ToList() );

            Rock.Attribute.Helper.AddEditControls(
                string.Empty,
                familyAttributeKeysToEdit,
                fakeFamily,
                phFamilyAttributes,
                btnSaveFamily.ValidationGroup,
                setValues,
                new List<string>(),
                2 );

            // override the attribute's IsRequired and set Required based on whether the attribute is part of the Required or Optional set of attributes for the Registration
            foreach ( Control attributeControl in phFamilyAttributes.ControlsOfTypeRecursive<Control>().OfType<Control>() )
            {
                if ( attributeControl is IHasRequired && attributeControl.ID.IsNotNullOrWhiteSpace() )
                {
                    int? attributeControlAttributeId = attributeControl.ID.Replace( "attribute_field_", string.Empty ).AsIntegerOrNull();
                    if ( attributeControlAttributeId.HasValue )
                    {
                        ( attributeControl as IHasRequired ).Required = this.RequiredAttributesForFamilies.Any( a => a.Id == attributeControlAttributeId.Value );
                    }
                }
            }
        }

        /// <summary>
        /// Creates the dynamic person controls.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void CreateDynamicPersonControls( FamilyRegistrationState.FamilyPersonState familyPersonState, bool setValues )
        {
            List<Group> resultList = null;

            phAdultAttributes.Controls.Clear();
            phChildAttributes.Controls.Clear();
            var fakePerson = new Person();
            var attributeList = familyPersonState.PersonAttributeValuesState.Select( a => AttributeCache.Get( a.Value.AttributeId ) ).ToList();
            fakePerson.Attributes = attributeList.ToDictionary( a => a.Key, v => v );
            fakePerson.AttributeValues = familyPersonState.PersonAttributeValuesState;

            var adultAttributeKeysToEdit = this.RequiredAttributesForAdults.OrderBy( a => a.Order ).Select( a => a.Key ).ToList();
            adultAttributeKeysToEdit.AddRange( this.OptionalAttributesForAdults.OrderBy( a => a.Order ).Select( a => a.Key ).ToList() );

            Rock.Attribute.Helper.AddEditControls(
                string.Empty,
                adultAttributeKeysToEdit,
                fakePerson,
                phAdultAttributes,
                btnDonePerson.ValidationGroup,
                setValues,
                new List<string>(),
                2 );

            var childAttributeKeysToEdit = this.RequiredAttributesForChildren.OrderBy( a => a.Order ).Select( a => a.Key ).ToList();
            childAttributeKeysToEdit.AddRange( this.OptionalAttributesForChildren.OrderBy( a => a.Order ).Select( a => a.Key ).ToList() );

            /* BEMA.FE1.Start */
            // add check-in group attribute if not there for children
            var attributeCheckInGroupGuid = GetAttributeValue ( BemaAttributeKey.CheckInGroupAttribute ).AsGuidOrNull();
            var groupAttribute = new AttributeCache();
            GroupMember groupMember = null;

            if (attributeCheckInGroupGuid.HasValue)
            {
                groupAttribute = AttributeCache.Get ( GetAttributeValue ( BemaAttributeKey.CheckInGroupAttribute ).AsGuid () );
                if ( !childAttributeKeysToEdit.Contains(groupAttribute.Key))
                {
                    childAttributeKeysToEdit.Add ( groupAttribute.Key );
                }

                //Exclude excluded group types
                List<Guid> groupTypeExcludeGuids = GetAttributeValue ( BemaAttributeKey.GroupTypesExclude ).SplitDelimitedValues ().AsGuidList ();
                resultList = CurrentCheckInState.Kiosk.KioskGroupTypes.Where ( t => t.IsCheckInActive == true )
                                .Where ( t => CurrentCheckInState.ConfiguredGroupTypes.Contains ( t.GroupType.Id ) && !groupTypeExcludeGuids.Contains ( t.GroupType.Guid ) )
                                .SelectMany ( t => t.KioskGroups ).Select ( g => g.Group ).ToList ();
                var idList = resultList.Select ( g => g.Id ).ToList ();

                // pre-populate check-in group to first valid group by getting the group member
                if ( attributeCheckInGroupGuid.HasValue && familyPersonState.PersonId.HasValue )
                {
                    var groupMemberService = new GroupMemberService ( new RockContext() );
                    groupMember = groupMemberService.Queryable ( false )
                        .Where ( a => idList.Contains ( a.GroupId ) && a.PersonId == familyPersonState.PersonId )
                        .FirstOrDefault ();
                }
            }
            /* BEMA.FE1.End */

            Rock.Attribute.Helper.AddEditControls(
                string.Empty,
                childAttributeKeysToEdit,
                fakePerson,
                phChildAttributes,
                btnDonePerson.ValidationGroup,
                setValues,
                new List<string>(),
                2 );

            // override the attribute's IsRequired and set Required based on whether the attribute is part of the Required or Optional set of attributes for the Registration
            foreach ( Control attributeControl in phAdultAttributes.ControlsOfTypeRecursive<Control>().OfType<Control>() )
            {
                if ( attributeControl is IHasRequired && attributeControl.ID.IsNotNullOrWhiteSpace() )
                {
                    int? attributeControlAttributeId = attributeControl.ID.Replace( "attribute_field_", string.Empty ).AsIntegerOrNull();
                    if ( attributeControlAttributeId.HasValue )
                    {
                        ( attributeControl as IHasRequired ).Required = this.RequiredAttributesForAdults.Any( a => a.Id == attributeControlAttributeId.Value );
                    }
                }
                var field = "attribute_field_" + groupAttribute.Id.ToString ();
                if ( field == attributeControl.ID )
                {
                    /* BEMA.FE1.End */
                    var limitCheckInGroup = GetAttributeValue ( BemaAttributeKey.OnlyDisplayActiveGroups ).AsBoolean ();
                    if ( ( attributeCheckInGroupGuid.HasValue ) && ( limitCheckInGroup == true ) )
                    {

                        ( (DropDownList) attributeControl ).Items.Clear ();
                        ( (DropDownList) attributeControl ).Items.Add ( new ListItem ( String.Empty, String.Empty ) );

                        foreach ( var group in resultList )
                        {
                            ( (DropDownList) attributeControl ).Items.Add ( new ListItem ( group.Name, group.Id.ToString () ) );
                            if (groupMember != null && group.Id == groupMember.Group.Id)
                            {
                                ( (DropDownList) attributeControl ).Items[( (DropDownList) attributeControl ).Items.Count - 1].Selected = true;
                            }
                        }
                    }
                    /* BEMA.FE1.End */

                }
            }

            foreach (Control attributeControl in phChildAttributes.ControlsOfTypeRecursive<Control>().OfType<Control>())
            {
                if (attributeControl is IHasRequired && attributeControl.ID.IsNotNullOrWhiteSpace())
                {
                    int? attributeControlAttributeId = attributeControl.ID.Replace("attribute_field_", string.Empty).AsIntegerOrNull();
                    if (attributeControlAttributeId.HasValue)
                    {
                        (attributeControl as IHasRequired).Required = this.RequiredAttributesForChildren.Any(a => a.Id == attributeControlAttributeId.Value);
                    }
                }

                var field = "attribute_field_" + groupAttribute.Id.ToString ();
                if ( field == attributeControl.ID )
                {
                    /* BEMA.FE1.End */
                    ( attributeControl as IHasRequired ).Required = GetAttributeValue ( BemaAttributeKey.RequireCheckInGroupforChild ).AsBoolean ();

                    var limitCheckInGroup = GetAttributeValue(BemaAttributeKey.OnlyDisplayActiveGroups).AsBoolean();
                    if ((attributeCheckInGroupGuid.HasValue) && (limitCheckInGroup == true))
                    {

                        ((DropDownList)attributeControl).Items.Clear();
                        ((DropDownList)attributeControl).Items.Add(new ListItem(String.Empty, String.Empty));

                        foreach (var group in resultList)
                        {
                            ((DropDownList)attributeControl).Items.Add(new ListItem(group.Name, group.Id.ToString()));

                            if ( groupMember != null && group.Id == groupMember.Group.Id )
                            {
                                ( (DropDownList) attributeControl ).Items[( (DropDownList) attributeControl ).Items.Count - 1].Selected = true;
                            }
                        }

                    }
                    /* BEMA.FE1.End */

                }
            }
        }

        #endregion Methods

        #region Edit Family

        /// <summary>
        /// Shows the Edit Family block in Edit Family mode
        /// </summary>
        /// <param name="checkInFamily">The check in family.</param>
        public override void ShowEditFamily( CheckInFamily checkInFamily )
        {
            ShowFamilyDetail( checkInFamily );
        }

        /// <summary>
        /// Shows the Edit Family block in Add Family mode
        /// </summary>
        public override void ShowAddFamily()
        {
            ShowFamilyDetail( null );
        }

        /// <summary>
        /// Shows edit UI fo the family (or null adding a new family)
        /// </summary>
        /// <param name="checkInFamily">The check in family.</param>
        private void ShowFamilyDetail( CheckInFamily checkInFamily )
        {
            if ( checkInFamily != null && checkInFamily.Group != null )
            {
                this.EditFamilyState = FamilyRegistrationState.FromGroup( checkInFamily.Group );
                hfGroupId.Value = checkInFamily.Group.Id.ToString();
                mdEditFamily.Title = checkInFamily.Group.Name;

                int groupId = hfGroupId.Value.AsInteger();
                var rockContext = new RockContext();
                var groupMemberService = new GroupMemberService( rockContext );
                var groupMembersQuery = groupMemberService.Queryable( false )
                    .Include( a => a.Person )
                    .Where( a => a.GroupId == groupId )
                    .OrderBy( m => m.GroupRole.Order )
                    .ThenBy( m => m.Person.BirthYear )
                    .ThenBy( m => m.Person.BirthMonth )
                    .ThenBy( m => m.Person.BirthDay )
                    .ThenBy( m => m.Person.Gender );

                var groupMemberList = groupMembersQuery.ToList();

                foreach ( var groupMember in groupMemberList )
                {
                    var familyPersonState = FamilyRegistrationState.FamilyPersonState.FromPerson( groupMember.Person, 0, true );
                    familyPersonState.GroupMemberGuid = groupMember.Guid;
                    familyPersonState.GroupId = groupMember.GroupId;
                    familyPersonState.IsAdult = groupMember.GroupRoleId == _groupTypeRoleAdultId;
                    this.EditFamilyState.FamilyPersonListState.Add( familyPersonState );
                }

                var adultIds = this.EditFamilyState.FamilyPersonListState.Where( a => a.IsAdult && a.PersonId.HasValue ).Select( a => a.PersonId.Value ).ToList();
                var roleIds = CurrentCheckInState.CheckInType.Registration.KnownRelationships.Where( a => a.Key != 0 ).Select( a => a.Key ).ToList();
                IEnumerable<GroupMember> personRelationships = new PersonService( rockContext ).GetRelatedPeople( adultIds, roleIds );
                foreach ( GroupMember personRelationship in personRelationships )
                {
                    if ( !this.EditFamilyState.FamilyPersonListState.Any( a => a.PersonId == personRelationship.Person.Id ) )
                    {
                        var familyPersonState = FamilyRegistrationState.FamilyPersonState.FromPerson( personRelationship.Person, personRelationship.GroupRoleId, false );
                        familyPersonState.GroupMemberGuid = Guid.NewGuid();
                        var relatedFamily = personRelationship.Person.GetFamily();
                        if ( relatedFamily != null )
                        {
                            familyPersonState.GroupId = relatedFamily.Id;
                        }

                        familyPersonState.IsAdult = false;
                        familyPersonState.ChildRelationshipToAdult = personRelationship.GroupRoleId;
                        familyPersonState.CanCheckIn = CurrentCheckInState.CheckInType.Registration.KnownRelationshipsCanCheckin.Any( k => k.Key == familyPersonState.ChildRelationshipToAdult );
                        this.EditFamilyState.FamilyPersonListState.Add( familyPersonState );
                    }
                }

                BindFamilyMembersGrid();
                CreateDynamicFamilyControls( EditFamilyState, true );

                /* BEMA.UI1.Start */
                // not a new family, don't require address
                acFamilyAddress.Visible = false;
                acFamilyAddress.Required = false;
                /* BEMA.UI1.End */

                ShowFamilyView ();
            }
            else
            {
                this.EditFamilyState = FamilyRegistrationState.FromGroup( new Group() { GroupTypeId = GroupTypeCache.GetFamilyGroupType().Id } );
                CreateDynamicFamilyControls( EditFamilyState, true );
                hfGroupId.Value = "0";
                mdEditFamily.Title = "Add Family";

                /* BEMA.UI1.Start */
                acFamilyAddress.Visible = GetAttributeValue ( BemaAttributeKey.ShowAddressForNewFamily ).AsBoolean () || GetAttributeValue ( BemaAttributeKey.RequireAddressForNewFamily ).AsBoolean ();
                acFamilyAddress.Required = GetAttributeValue ( BemaAttributeKey.RequireAddressForNewFamily ).AsBoolean ();
                /* BEMA.UI1.End */

                EditGroupMember ( null );
            }

            _initialEditFamilyStateHash = this.EditFamilyState.GetStateHash();

            // disable any idle redirect blocks that are on the page when the mdEditFamily modal is open
            DisableIdleRedirectBlocks( true );

            upContent.Update();
            mdEditFamily.Show();
        }

        /// <summary>
        /// Handles the GridRebind event of the gFamilyMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.GridRebindEventArgs"/> instance containing the event data.</param>
        private void gFamilyMembers_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindFamilyMembersGrid();
        }

        /// <summary>
        /// The index of the 'DeleteField' column in the grid for gFamilyMembers_RowDataBound
        /// </summary>
        private int _deleteFieldIndex;

        /// <summary>
        /// The known relationship lookup for gFamilyMembers_RowDataBound
        /// </summary>
        private Dictionary<int, string> _knownRelationshipLookup = null;

        /// <summary>
        /// Handles the RowDataBound event of the gFamilyMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gFamilyMembers_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            var familyPersonState = e.Row.DataItem as FamilyRegistrationState.FamilyPersonState;
            if ( familyPersonState != null )
            {
                Literal lGroupRoleAndRelationship = e.Row.FindControl( "lGroupRoleAndRelationship" ) as Literal;
                if ( lGroupRoleAndRelationship != null )
                {
                    lGroupRoleAndRelationship.Text = familyPersonState.GroupRole;
                    if ( familyPersonState.ChildRelationshipToAdult > 0 && _knownRelationshipLookup != null )
                    {
                        var relationshipText = _knownRelationshipLookup.GetValueOrNull( familyPersonState.ChildRelationshipToAdult );
                        lGroupRoleAndRelationship.Text = string.Format( "{0}<br/>{1}", familyPersonState.GroupRole, relationshipText );
                    }
                }

                Literal lRequiredAttributes = e.Row.FindControl( "lRequiredAttributes" ) as Literal;
                if ( lRequiredAttributes != null )
                {
                    List<AttributeCache> requiredAttributes;
                    if ( familyPersonState.IsAdult )
                    {
                        requiredAttributes = RequiredAttributesForAdults;
                    }
                    else
                    {
                        requiredAttributes = RequiredAttributesForChildren;
                    }

                    if ( requiredAttributes.Any() )
                    {
                        DescriptionList descriptionList = new DescriptionList();
                        foreach ( var requiredAttribute in requiredAttributes )
                        {
                            var attributeValue = familyPersonState.PersonAttributeValuesState.GetValueOrNull( requiredAttribute.Key );
                            var requiredAttributeDisplayValue = requiredAttribute.FieldType.Field.FormatValue( lRequiredAttributes, attributeValue != null ? attributeValue.Value : null, requiredAttribute.QualifierValues, true );
                            descriptionList.Add( requiredAttribute.Name, requiredAttributeDisplayValue );
                        }

                        lRequiredAttributes.Text = descriptionList.Html;
                    }
                }

                var deleteCell = ( e.Row.Cells[_deleteFieldIndex] as DataControlFieldCell ).Controls[0];
                if ( deleteCell != null )
                {
                    // only support deleting people that haven't been saved to the database yet
                    deleteCell.Visible = !familyPersonState.PersonId.HasValue;
                }
            }
        }

        /// <summary>
        /// Binds the family members grid.
        /// </summary>
        private void BindFamilyMembersGrid()
        {
            var deleteField = gFamilyMembers.ColumnsOfType<DeleteField>().FirstOrDefault();
            _deleteFieldIndex = gFamilyMembers.Columns.IndexOf( deleteField );

            _knownRelationshipLookup = CurrentCheckInState.CheckInType.Registration.KnownRelationships.ToDictionary( k => k.Key, v => v.Value );
            gFamilyMembers.DataSource = this.EditFamilyState.FamilyPersonListState.Where( a => a.IsDeleted == false ).ToList();
            gFamilyMembers.DataBind();
        }

        /// <summary>
        /// Handles the Click event of the btnSaveFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveFamily_Click( object sender, EventArgs e )
        {
            if ( CurrentCheckInState == null )
            {
                // OnLoad would have started a 'NavigateToHomePage', so just jump out
                return;
            }

            if ( !EditFamilyState.FamilyPersonListState.Any( x => !x.IsDeleted ) )
            {
                // Saving a new family, but nobody added to family, so just exit
                CancelFamilyEdit( false );
            }

            if ( !this.Page.IsValid )
            {
                return;
            }

            var rockContext = new RockContext();

            // Set the Campus for new families to the Campus of this Kiosk
            int? kioskCampusId = null;
            var deviceLocation = new DeviceService( rockContext ).GetSelect( CurrentCheckInState.Kiosk.Device.Id, a => a.Locations.FirstOrDefault() );
            if ( deviceLocation != null )
            {
                kioskCampusId = deviceLocation.CampusId;
            }

            /* BEMA.UI1.Start */
            // Check if Adult required in family 
            if ( GetAttributeValue( BemaAttributeKey.RequireAdultInNewFamily ).AsBoolean() && EditFamilyState.FamilyPersonListState.AsQueryable ().Where ( a => a.IsAdult == true ).Count () == 0 )
            {
                // Error message
                maWarning.Show ( "At least one adult is required in the family.", Rock.Web.UI.Controls.ModalAlertType.None );
                return;
            }
            /* BEMA.UI1.End */

            UpdateFamilyAttributesState ();

            SetEditableStateAttributes();

            FamilyRegistrationState.SaveResult saveResult = null;

            rockContext.WrapTransaction( () =>
            {
                saveResult = SaveFamilyAndPersonsToDatabase ( EditFamilyState, kioskCampusId, rockContext );
            } );

            /* BEMA.FE1.Start */
            // If we are adding to a group as well, update group membership
            var attributeCheckInGroup = GetAttributeValue( BemaAttributeKey.CheckInGroupAttribute );
            if (attributeCheckInGroup.AsGuidOrNull() != null)
            {
                var groupAttribute = AttributeCache.Get(GetAttributeValue( BemaAttributeKey.CheckInGroupAttribute ).AsGuid());
                var groupService = new GroupService(rockContext);

                foreach (var editPerson in EditFamilyState.FamilyPersonListState)
                {
                    var groupId = editPerson.PersonAttributeValuesState.GetValueOrNull(groupAttribute.Key);
                    if (groupId.Value.AsIntegerOrNull() != null)
                    {
                        var group = groupService.Get(groupId.Value.AsInteger());
                        // ensure that the person is not already in the group with role
                        if (group.Members.Where(m => m.PersonId == editPerson.PersonId && m.GroupRoleId == group.GroupType.DefaultGroupRole.Id).Count() == 0)
                        {
                            GroupMember groupMember = new GroupMember();
                            groupMember.GroupId = group.Id;
                            groupMember.PersonId = editPerson.PersonId.Value;
                            groupMember.GroupRoleId = group.GroupType.DefaultGroupRole.Id;
                            groupMember.GroupMemberStatus = GroupMemberStatus.Active;
                            group.Members.Add(groupMember);

                            try
                            {
                                rockContext.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                            }

                        }
                    }
                }
            }
            /* BEMA.UI1.End */

            // Queue up any Workflows that are configured to fire after a new person and/or family is added
            if ( saveResult.NewFamilyList.Any() )
            {
                var addFamilyWorkflowTypes = CurrentCheckInState.CheckInType.Registration.AddFamilyWorkflowTypes;

                // only fire a NewFamily workflow if the Primary family is new (don't fire workflows for any 'Can Checkin' families that were created)
                var newPrimaryFamily = saveResult.NewFamilyList.FirstOrDefault( a => a.Id == EditFamilyState.GroupId.Value );
                if ( newPrimaryFamily != null )
                {
                    foreach ( var addFamilyWorkflowType in addFamilyWorkflowTypes )
                    {
                        LaunchWorkflowTransaction launchWorkflowTransaction = new LaunchWorkflowTransaction<Group>( addFamilyWorkflowType.Id, newPrimaryFamily.Id );
                        launchWorkflowTransaction.Enqueue();
                    }
                }

                /* BEMA.UI1.Start */
                // Add address if given
                if ( acFamilyAddress.IsValid && acFamilyAddress.Street1.Length > 0 && acFamilyAddress.City.Length > 0 && acFamilyAddress.PostalCode.Length > 0 )
                {
                    var family = new GroupService ( rockContext ).Get ( newPrimaryFamily.Id );
                    var location = new LocationService(rockContext).Get(acFamilyAddress.Street1, acFamilyAddress.Street2, acFamilyAddress.City , acFamilyAddress.State , 
                        acFamilyAddress.PostalCode , acFamilyAddress.Country );
                    var groupLocation = new GroupLocation (  );
                    groupLocation.GroupLocationTypeValueId = DefinedValueCache.Get ( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid () ).Id;
                    groupLocation.LocationId = location.Id;
                    groupLocation.GroupId = family.Id;

                    family.GroupLocations.Add ( groupLocation );
                    rockContext.SaveChanges ();
                }
                /* BEMA.UI1.End */
            }

            if ( saveResult.NewPersonList.Any() )
            {
                var addPersonWorkflowTypes = CurrentCheckInState.CheckInType.Registration.AddPersonWorkflowTypes;
                foreach ( var newPerson in saveResult.NewPersonList )
                {
                    foreach ( var addPersonWorkflowType in addPersonWorkflowTypes )
                    {
                        LaunchWorkflowTransaction launchWorkflowTransaction = new LaunchWorkflowTransaction<Person>( addPersonWorkflowType.Id, newPerson.Id );
                        launchWorkflowTransaction.Enqueue();
                    }
                }
            }

            if ( CurrentCheckInState.CheckInType.Registration.EnableCheckInAfterRegistration )
            {
                upContent.Update();
                mdEditFamily.Hide();

                // un-disable any IdleRedirect blocks
                DisableIdleRedirectBlocks( false );

                var currentFamily = CurrentCheckInState.CheckIn.Families.FirstOrDefault( a => a.Group.Id == EditFamilyState.GroupId );

                if ( currentFamily == null )
                {
                    // if this is a new family, add it to the Checkin.Families so that the CurrentFamily wil be set to the new family
                    currentFamily = new CheckInFamily() { Selected = true };
                    currentFamily.Group = new GroupService( rockContext ).GetNoTracking( EditFamilyState.GroupId.Value ).Clone( false );
                    CurrentCheckInState.CheckIn.Families.Add( currentFamily );
                }

                if ( currentFamily.Selected )
                {
                    currentFamily.People.Clear();

                    // execute the workflow activity that is configured for this block (probably 'Person Search') so that
                    // the checkin state gets updated with any changes we made in Edit Family
                    string workflowActivity = GetAttributeValue( "WorkflowActivity" );
                    List<string> errorMessages;
                    if ( !string.IsNullOrEmpty( workflowActivity ) )
                    {
                        // just in case this is a new family, or family name or phonenumber was changed, update the search to match the updated values
                        if (  CurrentCheckInState.CheckIn.SearchType.Guid == Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_NAME.AsGuid())
                        {
                            var firstFamilyPerson = EditFamilyState.FamilyPersonListState.OrderBy( a => a.IsAdult ).FirstOrDefault();
                            if ( firstFamilyPerson != null )
                            {
                                CurrentCheckInState.CheckIn.SearchValue = firstFamilyPerson.FullNameForSearch;
                            }
                        }

                        if ( CurrentCheckInState.CheckIn.SearchType.Guid == Rock.SystemGuid.DefinedValue.CHECKIN_SEARCH_TYPE_PHONE_NUMBER.AsGuid())
                        {
                            var firstFamilyPersonWithPhone = EditFamilyState.FamilyPersonListState.Where(a => a.MobilePhoneNumber.IsNotNullOrWhiteSpace()).OrderBy( a => a.IsAdult ).FirstOrDefault();
                            if ( firstFamilyPersonWithPhone != null )
                            {
                                CurrentCheckInState.CheckIn.SearchValue = firstFamilyPersonWithPhone.MobilePhoneNumber;
                            }
                        }


                        ProcessActivity( workflowActivity, out errorMessages );
                    }
                }

                // if the searchBlock is on this page, have it re-search using the person's updated full name
                var searchBlock = this.RockPage.ControlsOfTypeRecursive<CheckInSearchBlock>().FirstOrDefault();

                if ( searchBlock != null )
                {
                    var firstFamilyPerson = EditFamilyState.FamilyPersonListState.OrderBy( a => a.IsAdult ).FirstOrDefault();
                    string searchString;
                    if ( firstFamilyPerson != null )
                    {
                        searchString = firstFamilyPerson.FullNameForSearch;
                    }
                    else
                    {
                        searchString = CurrentCheckInState.CheckIn.SearchValue;
                    }

                    searchBlock.ProcessSearch( searchString );
                }
                else
                {
                    // reload the current page so that other blocks will get updated correctly
                    NavigateToCurrentPageReference();
                }
            }
            else
            {
                upContent.Update();
                NavigateToHomePage();
            }
        }

        /// <summary>
        /// Updates the EditFamilyState.FamilyAttributeValuesState from any values that were viewable/editable in the UI
        /// </summary>
        private void UpdateFamilyAttributesState()
        {
            var fakeFamily = new Group() { GroupTypeId = GroupTypeCache.GetFamilyGroupType().Id, Id = EditFamilyState.GroupId ?? 0 };
            fakeFamily.LoadAttributes();
            Rock.Attribute.Helper.GetEditValues( phFamilyAttributes, fakeFamily );

            EditFamilyState.FamilyAttributeValuesState = fakeFamily.AttributeValues.ToDictionary( k => k.Key, v => v.Value );
        }

        /// <summary>
        /// Sets the editable state attributes for the EditFamilyState and each FamilyPersonState, so that only Editable attributes are saved to the database
        /// </summary>
        private void SetEditableStateAttributes()
        {
            EditFamilyState.EditableFamilyAttributes = RequiredAttributesForFamilies.Union( OptionalAttributesForFamilies ).Select( a => a.Id ).ToList();

            // only include the attributes for each person that are included in Required/Optional attributes so that we don't accidentally delete values for attributes are aren't included
            foreach ( var familyPersonState in EditFamilyState.FamilyPersonListState )
            {
                if ( familyPersonState.IsAdult )
                {
                    familyPersonState.EditableAttributes = RequiredAttributesForAdults.Union( OptionalAttributesForAdults ).Select( a => a.Id ).ToList();
                }
                else
                {
                    familyPersonState.EditableAttributes = RequiredAttributesForChildren.Union( OptionalAttributesForChildren ).Select( a => a.Id ).ToList();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancelFamily control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelFamily_Click( object sender, EventArgs e )
        {
            CancelFamilyEdit( true );
        }

        /// <summary>
        /// Cancels the family edit.
        /// </summary>
        /// <param name="promptIfChangesMade">if set to <c>true</c> [prompt if changes made].</param>
        private void CancelFamilyEdit( bool promptIfChangesMade )
        {
            if ( promptIfChangesMade )
            {
                UpdateFamilyAttributesState();
                int currentEditFamilyStateHash = EditFamilyState.GetStateHash();
                if ( _initialEditFamilyStateHash != currentEditFamilyStateHash )
                {
                    hfShowCancelEditPrompt.Value = "1";
                    upContent.Update();
                    return;
                }
            }

            upContent.Update();
            mdEditFamily.Hide();

            // un-disable any IdleRedirect blocks
            DisableIdleRedirectBlocks( false );
        }

        /// <summary>
        /// Handles the Click event of the DeleteFamilyMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void DeleteFamilyMember_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            if ( CurrentCheckInState == null )
            {
                // OnLoad would have started a 'NavigateToHomePage', so just jump out
                return;
            }

            var familyPersonState = EditFamilyState.FamilyPersonListState.FirstOrDefault( a => a.GroupMemberGuid == ( Guid ) e.RowKeyValue );
            familyPersonState.IsDeleted = true;
            BindFamilyMembersGrid();
        }

        /// <summary>
        /// Handles the Click event of the EditFamilyMember control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void EditFamilyMember_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            EditGroupMember( ( Guid ) e.RowKeyValue );
        }

        /// <summary>
        /// Handles the Click event of the btnAddPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void btnAddPerson_Click( object sender, System.EventArgs e )
        {
            EditGroupMember( null );
        }

        /// <summary>
        /// Shows the family view.
        /// </summary>
        private void ShowFamilyView()
        {
            pnlEditPerson.Visible = false;
            pnlEditFamily.Visible = true;
            mdEditFamily.Title = EditFamilyState.FamilyName;
            upContent.Update();
        }

        #endregion Edit Family

        #region Edit Person

        /// <summary>
        /// Edits the group member.
        /// </summary>
        /// <param name="groupMemberId">The group member identifier.</param>
        private void EditGroupMember( Guid? groupMemberGuid )
        {
            var rockContext = new RockContext();

            FamilyRegistrationState.FamilyPersonState familyPersonState = null;

            if ( groupMemberGuid.HasValue )
            {
                familyPersonState = EditFamilyState.FamilyPersonListState.FirstOrDefault( a => a.GroupMemberGuid == groupMemberGuid );
            }

            if ( familyPersonState == null )
            {
                // create a new temp record so we can set the defaults for the new person
                familyPersonState = FamilyRegistrationState.FamilyPersonState.FromTemporaryPerson();
                familyPersonState.GroupMemberGuid = Guid.NewGuid();

                // default Gender to Unknown so that it'll prompt to select gender if it hasn't been selected yet
                familyPersonState.Gender = Gender.Unknown;
                familyPersonState.IsAdult = false;
                familyPersonState.IsMarried = false;
                familyPersonState.RecordStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
                familyPersonState.ConnectionStatusValueId = CurrentCheckInState.CheckInType.Registration.DefaultPersonConnectionStatusId;

                var firstFamilyMember = EditFamilyState.FamilyPersonListState.FirstOrDefault();
                if ( firstFamilyMember != null )
                {
                    // if this family already has a person, default the LastName to the first person
                    familyPersonState.LastName = firstFamilyMember.LastName;
                }
            }

            hfGroupMemberGuid.Value = familyPersonState.GroupMemberGuid.ToString();
            tglAdultChild.Checked = familyPersonState.IsAdult;

            // only allow Adult/Child and Relationship to be changed for newly added people
            tglAdultChild.Visible = !familyPersonState.PersonId.HasValue;

            ddlChildRelationShipToAdult.Visible = !familyPersonState.PersonId.HasValue;
            lChildRelationShipToAdultReadOnly.Visible = familyPersonState.PersonId.HasValue;

            ShowControlsForRole( tglAdultChild.Checked );
            if ( familyPersonState.Gender == Gender.Unknown )
            {
                bgGender.SelectedValue = null;
            }
            else
            {
                bgGender.SetValue( familyPersonState.Gender.ConvertToInt() );
            }

            /* BEMA.UI1.Start */
            var showMaritalStatus = GetAttributeValue ( BemaAttributeKey.ShowMaritalStatusForAdult ).AsBoolean ();
            tglAdultMaritalStatus.Visible = familyPersonState.IsAdult & showMaritalStatus;
            if (showMaritalStatus)
            {
                tglAdultMaritalStatus.Checked = familyPersonState.IsMarried;
            }
            /* BEMA.UI1.End */

            ddlChildRelationShipToAdult.Items.Clear();

            foreach ( var relationShipType in CurrentCheckInState.CheckInType.Registration.KnownRelationships )
            {
                ddlChildRelationShipToAdult.Items.Add( new ListItem( relationShipType.Value, relationShipType.Key.ToString() ) );
            }

            ddlChildRelationShipToAdult.SetValue( familyPersonState.ChildRelationshipToAdult );
            lChildRelationShipToAdultReadOnly.Text = CurrentCheckInState.CheckInType.Registration.KnownRelationships.GetValueOrNull( familyPersonState.ChildRelationshipToAdult );

            // Only show the RecordStatus if they aren't currently active
            dvpRecordStatus.Visible = false;
            if ( familyPersonState.PersonId.HasValue )
            {
                var personRecordStatusValueId = new PersonService( rockContext ).GetSelect( familyPersonState.PersonId.Value, a => a.RecordStatusValueId );
                if ( personRecordStatusValueId.HasValue )
                {
                    dvpRecordStatus.Visible = personRecordStatusValueId != _personRecordStatusActiveId;
                }
            }

            tbFirstName.Focus();
            tbFirstName.Text = familyPersonState.FirstName;
            tbLastName.Text = familyPersonState.LastName;
            tbAlternateID.Text = familyPersonState.AlternateID;

            dvpSuffix.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_SUFFIX.AsGuid() ).Id;
            dvpSuffix.SetValue( familyPersonState.SuffixValueId );

            dvpRecordStatus.DefinedTypeId = DefinedTypeCache.Get( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS.AsGuid() ).Id;
            dvpRecordStatus.SetValue( familyPersonState.RecordStatusValueId );
            hfConnectionStatus.Value = familyPersonState.ConnectionStatusValueId.ToString();

            var mobilePhoneNumber = familyPersonState.MobilePhoneNumber;
            if ( mobilePhoneNumber != null )
            {
                pnMobilePhone.CountryCode = familyPersonState.MobilePhoneCountryCode;
                pnMobilePhone.Number = mobilePhoneNumber;
            }
            else
            {
                pnMobilePhone.CountryCode = string.Empty;
                pnMobilePhone.Number = string.Empty;
            }

            tbEmail.Text = familyPersonState.Email;
            dpBirthDate.SelectedDate = familyPersonState.BirthDate;
            if ( familyPersonState.GradeOffset.HasValue )
            {
                gpGradePicker.SetValue( familyPersonState.GradeOffset );
            }
            else
            {
                gpGradePicker.SelectedValue = null;
            }

            CreateDynamicPersonControls( familyPersonState, true );

            ShowPersonView( familyPersonState );
        }

        /// <summary>
        /// Shows the person view.
        /// </summary>
        /// <param name="familyPersonState">State of the family person.</param>
        private void ShowPersonView( FamilyRegistrationState.FamilyPersonState familyPersonState )
        {
            pnlEditFamily.Visible = false;
            pnlEditPerson.Visible = true;
            mdEditFamily.Title = familyPersonState.FullName;
            upContent.Update();
        }

        /// <summary>
        /// Shows the controls for role.
        /// </summary>
        /// <param name="isAdult">if set to <c>true</c> [is adult].</param>
        private void ShowControlsForRole( bool isAdult )
        {
            if ( CurrentCheckInState == null )
            {
                // OnLoad would have started a 'NavigateToHomePage', so just jump out
                return;
            }

            /* BEMA.UI1.Start */
            pnMobilePhone.Visible = isAdult | GetAttributeValue ( BemaAttributeKey.ShowMobileForChild ).AsBoolean (); ;
            pnMobilePhone.Required = isAdult & GetAttributeValue ( BemaAttributeKey.RequireMobileForAdult ).AsBoolean ();
            tbEmail.Required = isAdult & GetAttributeValue ( BemaAttributeKey.RequireEmailForAdult ).AsBoolean ();
            tbEmail.Visible = isAdult;
            var showMaritalStatus = GetAttributeValue ( BemaAttributeKey.ShowMaritalStatusForAdult ).AsBoolean ();
            tglAdultMaritalStatus.Visible = isAdult & showMaritalStatus;


            //tglAdultMaritalStatus.Visible = isAdult; 
            dpBirthDate.Required = (!isAdult & GetAttributeValue ( BemaAttributeKey.RequireBirthdateForChild ).AsBoolean ()) || ( isAdult & GetAttributeValue ( BemaAttributeKey.RequireBirthdateForAdult ).AsBoolean () );
            dpBirthDate.Visible = !isAdult | GetAttributeValue ( BemaAttributeKey.ShowBirthdateForAdult ).AsBoolean (); 
            gpGradePicker.Required = !isAdult & GetAttributeValue ( BemaAttributeKey.RequireGradeForChild ).AsBoolean ();
            gpGradePicker.Visible = !isAdult;
            pnlChildRelationshipToAdult.Visible = !isAdult;

            tbAlternateID.Visible = ( isAdult && CurrentCheckInState.CheckInType.Registration.DisplayAlternateIdFieldforAdults ) || ( !isAdult && CurrentCheckInState.CheckInType.Registration.DisplayAlternateIdFieldforChildren );
            phAdultAttributes.Visible = isAdult;
            phChildAttributes.Visible = !isAdult;
            /* BEMA.UI1.End */
        }

        /// <summary>
        /// Handles the CheckedChanged event of the tglAdultChild control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void tglAdultChild_CheckedChanged( object sender, EventArgs e )
        {
            ShowControlsForRole( tglAdultChild.Checked );
        }

        /// <summary>
        /// Handles the Click event of the btnCancelPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelPerson_Click( object sender, EventArgs e )
        {
            if ( CurrentCheckInState == null )
            {
                // OnLoad would have started a 'NavigateToHomePage', so just jump out
                return;
            }

            ShowFamilyView();

            if ( !EditFamilyState.FamilyPersonListState.Any() )
            {
                // cancelling on adding first person to family, so cancel adding the family too
                CancelFamilyEdit( false );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDonePerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDonePerson_Click( object sender, EventArgs e )
        {
            if ( CurrentCheckInState == null )
            {
                // OnLoad would have started a 'NavigateToHomePage', so just jump out
                return;
            }

            Guid groupMemberGuid = hfGroupMemberGuid.Value.AsGuid();
            var familyPersonState = EditFamilyState.FamilyPersonListState.FirstOrDefault( a => a.GroupMemberGuid == groupMemberGuid );
            if ( familyPersonState == null )
            {
                // new person added
                familyPersonState = FamilyRegistrationState.FamilyPersonState.FromTemporaryPerson();
                familyPersonState.GroupMemberGuid = groupMemberGuid;
                familyPersonState.PersonId = null;
                EditFamilyState.FamilyPersonListState.Add( familyPersonState );
            }

            familyPersonState.RecordStatusValueId = dvpRecordStatus.SelectedValue.AsIntegerOrNull();
            familyPersonState.ConnectionStatusValueId = hfConnectionStatus.Value.AsIntegerOrNull();
            familyPersonState.IsAdult = tglAdultChild.Checked;

            familyPersonState.Gender = bgGender.SelectedValueAsEnumOrNull<Gender>() ?? Gender.Unknown;
            familyPersonState.ChildRelationshipToAdult = ddlChildRelationShipToAdult.SelectedValue.AsInteger();

            familyPersonState.InPrimaryFamily = CurrentCheckInState.CheckInType.Registration.KnownRelationshipsSameFamily.Any( k => k.Key == familyPersonState.ChildRelationshipToAdult );
            familyPersonState.CanCheckIn = ( familyPersonState.InPrimaryFamily == false )
                && CurrentCheckInState.CheckInType.Registration.KnownRelationshipsCanCheckin.Any( k => k.Key == familyPersonState.ChildRelationshipToAdult );

            var showMaritalStatus = GetAttributeValue ( BemaAttributeKey.ShowMaritalStatusForAdult ).AsBoolean ();
            if ( showMaritalStatus )
            {
                familyPersonState.IsMarried = tglAdultMaritalStatus.Checked;
            }

            familyPersonState.FirstName = tbFirstName.Text.FixCase();
            /* BEMA.UI1.Start */
            // Format Initials
            if ( familyPersonState.FirstName[1] == '.' && familyPersonState.FirstName[3] == '.' )
            {
                familyPersonState.FirstName = tbFirstName.Text.ToUpper ();
            }
            /* BEMA.UI1.End */

            familyPersonState.LastName = tbLastName.Text.FixCase();
            familyPersonState.SuffixValueId = dvpSuffix.SelectedValue.AsIntegerOrNull();

            familyPersonState.MobilePhoneNumber = pnMobilePhone.Number;
            familyPersonState.MobilePhoneCountryCode = pnMobilePhone.CountryCode;
            familyPersonState.BirthDate = dpBirthDate.SelectedDate;
            familyPersonState.Email = tbEmail.Text;

            if ( gpGradePicker.SelectedGradeValue != null )
            {
                familyPersonState.GradeOffset = gpGradePicker.SelectedGradeValue.Value.AsIntegerOrNull();
            }
            else
            {
                familyPersonState.GradeOffset = null;
            }

            familyPersonState.AlternateID = tbAlternateID.Text;
            var fakePerson = new Person() { Id = familyPersonState.PersonId ?? 0 };
            fakePerson.LoadAttributes();

            if ( familyPersonState.IsAdult )
            {
                Rock.Attribute.Helper.GetEditValues( phAdultAttributes, fakePerson );
            }
            else
            {
                Rock.Attribute.Helper.GetEditValues( phChildAttributes, fakePerson );
            }

            familyPersonState.PersonAttributeValuesState = fakePerson.AttributeValues.ToDictionary( k => k.Key, v => v.Value );

            ShowFamilyView();

            BindFamilyMembersGrid();
        }
        /// <summary>
        /// Saves the family and persons to the database
        /// </summary>
        /// <param name="kioskCampusId">The kiosk campus identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public FamilyRegistrationState.SaveResult SaveFamilyAndPersonsToDatabase( FamilyRegistrationState editFamilyState, int? kioskCampusId, RockContext rockContext )
        {
            FamilyRegistrationState.SaveResult saveResult = new FamilyRegistrationState.SaveResult ();

            var personService = new PersonService ( rockContext );
            var groupService = new GroupService ( rockContext );
            var recordTypePersonId = DefinedValueCache.Get ( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid () ).Id;
            var maritalStatusMarried = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid() );
            var maritalStatusSingle = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_SINGLE.AsGuid() );

            /* BEMA.UI1.Start */
            var defaultMaritalStatus = GetAttributeValue ( BemaAttributeKey.NewPersonMaritalStatus ).AsGuidOrNull ();
            var showMaritalStatus = GetAttributeValue ( BemaAttributeKey.ShowMaritalStatusForAdult ).AsBoolean ();
            /* BEMA.UI1.End */

            var numberTypeValueMobile = DefinedValueCache.Get ( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid () );
            int groupTypeRoleAdultId = GroupTypeCache.GetFamilyGroupType ().Roles.FirstOrDefault ( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid () ).Id;
            int groupTypeRoleChildId = GroupTypeCache.GetFamilyGroupType ().Roles.FirstOrDefault ( a => a.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid () ).Id;
            int? groupTypeRoleCanCheckInId = null;
            if (
                GroupTypeCache.Get ( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid () ) != null &&
                GroupTypeCache.Get ( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid () ).Roles.FirstOrDefault ( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN.AsGuid () ) != null
            )
            {
                groupTypeRoleCanCheckInId = GroupTypeCache.Get ( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid () ).Roles.FirstOrDefault ( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN.AsGuid () ).Id;
            }

            Group primaryFamily = null;

            if ( editFamilyState.GroupId.HasValue )
            {
                primaryFamily = groupService.Get ( editFamilyState.GroupId.Value );
            }

            // see if we can find matches for new people that were added, and also set the primary family if this is a new family, but a matching family was found
            foreach ( var familyPersonState in editFamilyState.FamilyPersonListState.Where ( a => !a.PersonId.HasValue && !a.IsDeleted ) )
            {
                var personQuery = new PersonService.PersonMatchQuery ( familyPersonState.FirstName, familyPersonState.LastName, familyPersonState.Email, familyPersonState.MobilePhoneNumber, familyPersonState.Gender, familyPersonState.BirthDate, familyPersonState.SuffixValueId );
                var matchingPerson = personService.FindPerson ( personQuery, true );
                if ( matchingPerson != null )
                {
                    // newly added person, but a match was found, so set the PersonId, GroupId, and ConnectionStatusValueID to the matching person instead of creating a new person
                    familyPersonState.PersonId = matchingPerson.Id;
                    if ( matchingPerson.GetFamily ( rockContext ) != null )
                    {
                        familyPersonState.GroupId = matchingPerson.GetFamily ( rockContext ).Id;
                    }

                    familyPersonState.RecordStatusValueId = matchingPerson.RecordStatusValueId;
                    familyPersonState.ConnectionStatusValueId = matchingPerson.ConnectionStatusValueId;
                    familyPersonState.ConvertedToMatchedPerson = true;
                    if ( primaryFamily == null && familyPersonState.IsAdult )
                    {
                        // if this is a new family, but we found a matching adult person, use that person's family as the family
                        primaryFamily = matchingPerson.GetFamily ( rockContext );
                    }
                }
            }

            // loop thru all people and add/update as needed
            foreach ( var familyPersonState in editFamilyState.FamilyPersonListState.Where ( a => !a.IsDeleted ) )
            {
                Person person;
                if ( !familyPersonState.PersonId.HasValue )
                {
                    person = new Person ();
                    personService.Add ( person );
                    saveResult.NewPersonList.Add ( person );
                    person.RecordTypeValueId = recordTypePersonId;
                    if ( !showMaritalStatus && defaultMaritalStatus.HasValue && !familyPersonState.IsAdult )
                    {
                        var maritalStatusDefault = DefinedValueCache.Get ( GetAttributeValue ( BemaAttributeKey.NewPersonMaritalStatus ).AsGuid () );
                        person.MaritalStatusValueId = maritalStatusDefault.Id;
                    }
                }
                else
                {
                    person = personService.Get ( familyPersonState.PersonId.Value );
                }

                // NOTE, Gender, MaritalStatusValueId, NickName, LastName are required fields so, always updated them to match the UI (even if a matched person was found)
                person.Gender = familyPersonState.Gender;
                person.NickName = familyPersonState.FirstName;
                person.LastName = familyPersonState.LastName;

                // if the familyPersonState was converted to a Matched Person, don't overwrite existing values with blank values
                var saveEmptyValues = !familyPersonState.ConvertedToMatchedPerson;

                if ( familyPersonState.SuffixValueId.HasValue || saveEmptyValues )
                {
                    person.SuffixValueId = familyPersonState.SuffixValueId;
                }

                if ( familyPersonState.BirthDate.HasValue || saveEmptyValues )
                {
                    person.SetBirthDate ( familyPersonState.BirthDate );
                }

                if ( familyPersonState.Email.IsNotNullOrWhiteSpace () || saveEmptyValues )
                {
                    person.Email = familyPersonState.Email;
                }

                if ( familyPersonState.GradeOffset.HasValue || saveEmptyValues )
                {
                    person.GradeOffset = familyPersonState.GradeOffset;
                }

                // if a matching person was found, the familyPersonState's RecordStatusValueId and ConnectinoStatusValueId was already updated to match the matched person
                person.RecordStatusValueId = familyPersonState.RecordStatusValueId;
                person.ConnectionStatusValueId = familyPersonState.ConnectionStatusValueId;

                rockContext.SaveChanges ();

                bool isNewPerson = !familyPersonState.PersonId.HasValue;
                if ( !familyPersonState.PersonId.HasValue )
                {
                    // if we added a new person, we know now the personId after SaveChanges, so set it
                    familyPersonState.PersonId = person.Id;
                }

                if ( familyPersonState.AlternateID.IsNotNullOrWhiteSpace () )
                {
                    PersonSearchKey personAlternateValueIdSearchKey;
                    PersonSearchKeyService personSearchKeyService = new PersonSearchKeyService ( rockContext );
                    if ( isNewPerson )
                    {
                        // if we added a new person, a default AlternateId was probably added in the service layer. If a specific Alternate ID was specified, make sure that their SearchKey is updated
                        personAlternateValueIdSearchKey = person.GetPersonSearchKeys ( rockContext ).Where ( a => a.SearchTypeValueId == _personSearchAlternateValueId ).FirstOrDefault ();
                    }
                    else
                    {
                        // see if the key already exists. If if it doesn't already exist, let a new one get created
                        personAlternateValueIdSearchKey = person.GetPersonSearchKeys ( rockContext ).Where ( a => a.SearchTypeValueId == _personSearchAlternateValueId && a.SearchValue == familyPersonState.AlternateID ).FirstOrDefault ();
                    }

                    if ( personAlternateValueIdSearchKey == null )
                    {
                        personAlternateValueIdSearchKey = new PersonSearchKey ();
                        personAlternateValueIdSearchKey.PersonAliasId = person.PrimaryAliasId;
                        personAlternateValueIdSearchKey.SearchTypeValueId = _personSearchAlternateValueId;
                        personSearchKeyService.Add ( personAlternateValueIdSearchKey );
                    }

                    if ( personAlternateValueIdSearchKey.SearchValue != familyPersonState.AlternateID )
                    {
                        personAlternateValueIdSearchKey.SearchValue = familyPersonState.AlternateID;
                        rockContext.SaveChanges ();
                    }
                }

                person.LoadAttributes ();
                foreach ( var attributeValue in familyPersonState.PersonAttributeValuesState )
                {
                    // only set attribute values that are editable so we don't accidently delete any attribute values
                    if ( familyPersonState.EditableAttributes.Contains ( attributeValue.Value.AttributeId ) )
                    {
                        if ( attributeValue.Value.Value.IsNotNullOrWhiteSpace () || saveEmptyValues )
                        {
                            person.SetAttributeValue ( attributeValue.Key, attributeValue.Value.Value );
                        }
                    }
                }

                person.SaveAttributeValues ( rockContext );

                if ( familyPersonState.MobilePhoneNumber.IsNotNullOrWhiteSpace () || saveEmptyValues )
                {
                    person.UpdatePhoneNumber ( numberTypeValueMobile.Id, familyPersonState.MobilePhoneCountryCode, familyPersonState.MobilePhoneNumber, true, false, rockContext );
                }

                rockContext.SaveChanges ();
            }

            if ( primaryFamily == null )
            {
                // new family and no family found by looking up matching adults, so create a new family
                primaryFamily = new Group ();
                var familyLastName = editFamilyState.FamilyPersonListState.OrderByDescending ( a => a.IsAdult ).Where ( a => !a.IsDeleted ).Select ( a => a.LastName ).FirstOrDefault ();
                primaryFamily.Name = familyLastName + " Family";
                primaryFamily.GroupTypeId = GroupTypeCache.GetFamilyGroupType ().Id;

                // Set the Campus to the Campus of this Kiosk
                primaryFamily.CampusId = kioskCampusId;

                groupService.Add ( primaryFamily );
                saveResult.NewFamilyList.Add ( primaryFamily );
                rockContext.SaveChanges ();
            }

            if ( !editFamilyState.GroupId.HasValue )
            {
                editFamilyState.GroupId = primaryFamily.Id;
            }

            primaryFamily.LoadAttributes ();
            foreach ( var familyAttribute in editFamilyState.FamilyAttributeValuesState )
            {
                // only set attribute values that are editable so we don't accidently delete any attribute values
                if ( editFamilyState.EditableFamilyAttributes.Contains ( familyAttribute.Value.AttributeId ) )
                {
                    primaryFamily.SetAttributeValue ( familyAttribute.Key, familyAttribute.Value.Value );
                }
            }

            primaryFamily.SaveAttributeValues ( rockContext );

            var groupMemberService = new GroupMemberService ( rockContext );

            // loop thru all people that are part of the same family (in the UI) and ensure they are all in the same primary family (in the database)
            foreach ( var familyPersonState in editFamilyState.FamilyPersonListState.Where ( a => !a.IsDeleted && a.InPrimaryFamily ) )
            {
                var currentFamilyMember = primaryFamily.Members.FirstOrDefault ( m => m.PersonId == familyPersonState.PersonId.Value );

                if ( currentFamilyMember == null )
                {
                    currentFamilyMember = new GroupMember
                    {
                        GroupId = primaryFamily.Id,
                        PersonId = familyPersonState.PersonId.Value,
                        GroupMemberStatus = GroupMemberStatus.Active
                    };

                    if ( familyPersonState.IsAdult )
                    {
                        currentFamilyMember.GroupRoleId = groupTypeRoleAdultId;
                    }
                    else
                    {
                        currentFamilyMember.GroupRoleId = groupTypeRoleChildId;
                    }

                    groupMemberService.Add ( currentFamilyMember );

                    rockContext.SaveChanges ();
                }
            }

            // make a dictionary of new related families (by lastname) so we can combine any new related children into a family with the same last name
            Dictionary<string, Group> newRelatedFamilies = new Dictionary<string, Group> ( StringComparer.OrdinalIgnoreCase );

            // loop thru all people that are NOT part of the same family
            foreach ( var familyPersonState in editFamilyState.FamilyPersonListState.Where ( a => !a.IsDeleted && a.InPrimaryFamily == false ) )
            {
                if ( !familyPersonState.GroupId.HasValue )
                {
                    // related person not in a family yet
                    Group relatedFamily = newRelatedFamilies.GetValueOrNull ( familyPersonState.LastName );
                    if ( relatedFamily == null )
                    {
                        relatedFamily = new Group ();
                        relatedFamily.Name = familyPersonState.LastName + " Family";
                        relatedFamily.GroupTypeId = GroupTypeCache.GetFamilyGroupType ().Id;

                        // Set the Campus to the Campus of this Kiosk
                        relatedFamily.CampusId = kioskCampusId;

                        newRelatedFamilies.Add ( familyPersonState.LastName, relatedFamily );
                        groupService.Add ( relatedFamily );
                        saveResult.NewFamilyList.Add ( relatedFamily );
                    }

                    rockContext.SaveChanges ();

                    familyPersonState.GroupId = relatedFamily.Id;

                    var familyMember = new GroupMember
                    {
                        GroupId = relatedFamily.Id,
                        PersonId = familyPersonState.PersonId.Value,
                        GroupMemberStatus = GroupMemberStatus.Active
                    };

                    if ( familyPersonState.IsAdult )
                    {
                        familyMember.GroupRoleId = groupTypeRoleAdultId;
                    }
                    else
                    {
                        familyMember.GroupRoleId = groupTypeRoleChildId;
                    }

                    groupMemberService.Add ( familyMember );
                }

                // ensure there are known relationships between each adult in the primary family to this person that isn't in the primary family
                foreach ( var primaryFamilyAdult in editFamilyState.FamilyPersonListState.Where ( a => a.IsAdult && a.InPrimaryFamily ) )
                {
                    groupMemberService.CreateKnownRelationship ( primaryFamilyAdult.PersonId.Value, familyPersonState.PersonId.Value, familyPersonState.ChildRelationshipToAdult );

                    // if this is something other than the CanCheckIn relationship, but is a relationship that should ensure a CanCheckIn relationship, create a CanCheckinRelationship
                    if ( groupTypeRoleCanCheckInId.HasValue && familyPersonState.CanCheckIn && groupTypeRoleCanCheckInId != familyPersonState.ChildRelationshipToAdult )
                    {
                        groupMemberService.CreateKnownRelationship ( primaryFamilyAdult.PersonId.Value, familyPersonState.PersonId.Value, groupTypeRoleCanCheckInId.Value );
                    }
                }
            }

            return saveResult;
        }

        #endregion Edit Person
    }
}