// <copyright>
// Copyright by the Spark Development Network
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
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Humanizer;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Crm
{
    [DisplayName( "Family Pre Registration" )]
    [Category( "CRM" )]
    [Description( "Provides a way to allow people to pre-register their families for weekend check-in." )]

    [BooleanField( "Show Campus", "Should the campus field be displayed?", true, "", 0 )]
    [CampusField( "Default Campus", "An optional campus to use by default when adding a new family.", false, "", "", 1 )]
    [CustomDropdownListField( "Planned Visit Date", "How should the Planned Visit Date field be displayed (this value is only used when starting a workflow)?", "Hide,Optional,Required", false, "Optional", "", 2 )]
    [AttributeField( Rock.SystemGuid.EntityType.GROUP, "GroupTypeId", Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY, "Family Attributes", "The Family attributes that should be displayed", false, true, "", "", 3 )]
    [BooleanField( "Allow Updates", "If the person visiting this block is logged in, should the block be used to update their family? If not, a new family will always be created unless 'Auto Match' is enabled and the information entered matches an existing person.", false, "", 4 )]
    [BooleanField( "Auto Match", "Should this block attempt to match people to to current records in the database.", true, "", 5)]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS, "Connection Status", "The connection status that should be used when adding new people.", false, false, Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR, "", 6 )]
    [DefinedValueField( Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS, "Record Status", "The record status that should be used when adding new people.", false, false, Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE, "", 7 )]
    [WorkflowTypeField( "Workflow Types", @"
The workflow type(s) to launch when a family is added. The primary family will be passed to each workflow as the entity. Additionally if the workflow type has any of the 
following attribute keys defined, those attribute values will also be set: ParentIds, ChildIds, PlannedVisitDate.
", true, false, "", "", 8 )]
    [CodeEditorField( "Redirect URL", @"
The URL to redirect user to when they have completed the registration. The merge fields that are available includes 'Family', which is an object for the primary family 
that is created/updated; 'RelatedChildren', which is a list of the children who have a relationship with the family, but are not in the family; 'ParentIds' which is a
comma-delimited list of the person ids for each adult; 'ChildIds' which is a comma-delimited list of the person ids for each child; and 'PlannedVisitDate' which is 
the value entered for the Planned Visit Date field if it was displayed.
", CodeEditorMode.Lava, CodeEditorTheme.Rock, 200, false, "", "", 9 )]

    [CustomDropdownListField( "Suffix", "How should Suffix be displayed for adults?", "Hide,Optional", false, "Hide", "Adult Fields", 0, "AdultSuffix" )]
    [CustomDropdownListField( "Gender", "How should Gender be displayed for adults?", "Hide,Optional,Required", false, "Optional", "Adult Fields", 1, "AdultGender" )]
    [CustomDropdownListField( "Birth Date", "How should Gender be displayed for adults?", "Hide,Optional,Required", false, "Optional", "Adult Fields", 2, "AdultBirthdate" )]
    [CustomDropdownListField( "Marital Status", "How should Marital Status be displayed for adults?", "Hide,Optional,Required", false, "Required", "Adult Fields", 3, "AdultMaritalStatus" )]
    [CustomDropdownListField( "Email", "How should Email be displayed for adults?", "Hide,Optional,Required", false, "Required", "Adult Fields", 4, "AdultEmail" )]
    [CustomDropdownListField( "Mobile Phone", "How should Mobile Phone be displayed for adults?", "Hide,Optional,Required", false, "Required", "Adult Fields", 5, "AdultMobilePhone" )]
    [AttributeCategoryField( "Attribute Categories", "The adult Attribute Categories to display attributes from", true, "Rock.Model.Person", false, "", "Adult Fields", 6, "AdultAttributeCategories" )]

    [CustomDropdownListField( "Suffix", "How should Suffix be displayed for children?", "Hide,Optional", false, "Hide", "Child Fields", 0, "ChildSuffix" )]
    [CustomDropdownListField( "Gender", "How should Gender be displayed for children?", "Hide,Optional,Required", false, "Optional", "Child Fields", 1, "ChildGender" )]
    [CustomDropdownListField( "Birth Date", "How should Gender be displayed for children?", "Hide,Optional,Required", false, "Required", "Child Fields", 2, "ChildBirthdate" )]
    [CustomDropdownListField( "Grade", "How should Grade be displayed for children?", "Hide,Optional,Required", false, "Optional", "Child Fields", 3, "ChildGrade" )]
    [CustomDropdownListField( "Mobile Phone", "How should Mobile Phone be displayed for children?", "Hide,Optional,Required", false, "Hide", "Child Fields", 4, "ChildMobilePhone" )]
    [AttributeCategoryField( "Attribute Categories", "The children Attribute Categories to display attributes from.", true, "Rock.Model.Person", false, "", "Child Fields", 5, "ChildAttributeCategories" )]

    [CustomEnhancedListField( "Known Relationship Types", "The known relationship types that should be displayed as the possible ways that a child can be related to the adult(s).", @"
SELECT 
	R.[Id] AS [Value],
	R.[Name] AS [Text]
FROM [GroupType] T
INNER JOIN [GroupTypeRole] R ON R.[GroupTypeId] = T.[Id]
WHERE T.[Guid] = 'E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF'
AND R.[Name] <> 'Child'
UNION ALL
SELECT 0, 'Child'
ORDER BY [Text]", false, "0", "Child Relationship", 0, "Relationships" )]
    [CustomEnhancedListField( "Same Family Known Relationship Types", "The known relationship types that if selected for a child should just add child to same family as the adult(s) rather than actually creating the know relationship.", @"
SELECT 
	R.[Id] AS [Value],
	R.[Name] AS [Text]
FROM [GroupType] T
INNER JOIN [GroupTypeRole] R ON R.[GroupTypeId] = T.[Id]
WHERE T.[Guid] = 'E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF'
AND R.[Name] <> 'Child'
UNION ALL
SELECT 0, 'Child'
ORDER BY [Text]", false, "0", "Child Relationship", 1, "FamilyRelationships" )]
    [CustomEnhancedListField( "Can Check-in Known Relationship Types", "The known relationship types that if selected for a child should also create the 'Can Check-in' known relationship type.", @"
SELECT 
	R.[Id] AS [Value],
	R.[Name] AS [Text]
FROM [GroupType] T
INNER JOIN [GroupTypeRole] R ON R.[GroupTypeId] = T.[Id]
WHERE T.[Guid] = 'E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF'
AND R.[Name] <> 'Child'
UNION ALL
SELECT 0, 'Child'
ORDER BY [Text]", false, "", "Child Relationship", 2, "CanCheckinRelationships" )]

    public partial class FamilyPreRegistration : RockBlock
    {
        #region Fields

        private RockContext _rockContext = null;
        private Dictionary<int, string>  _relationshipTypes = new Dictionary<int, string>();

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the child members that have been added by user
        /// </summary>
        /// <value>
        /// The group members.
        /// </value>
        protected List<PreRegistrationChild> Children { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            Children = ViewState["Children"] as List<PreRegistrationChild> ?? new List<PreRegistrationChild>();

            BuildAdultAttributes( false, null, null );
            BuildFamilyAttributes( false, null );
            CreateChildrenControls( false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _rockContext = new RockContext();

            // Get the allowed relationship types that have been selected
            var selectedRelationshipTypeIds = GetAttributeValue( "Relationships" ).SplitDelimitedValues().AsIntegerList();
            var knownRelationshipGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );
            if ( knownRelationshipGroupType != null )
            {
                _relationshipTypes = knownRelationshipGroupType.Roles
                    .Where( r => selectedRelationshipTypeIds.Contains( r.Id ) )
                    .ToDictionary( k => k.Id, v => v.Name );
            }
            if ( selectedRelationshipTypeIds.Contains( 0 ) )
            {
                _relationshipTypes.Add( 0, "Child" );
            }

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbError.Visible = false;

            if ( !Page.IsPostBack )
            {
                SetControls();
            }
            else
            {
                GetChildrenData();
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
            ViewState["Children"] = Children;

            return base.SaveViewState();
        }

        protected override void OnPreRender( EventArgs e )
        {
            base.OnPreRender( e );

            string script = string.Format( @"
    testRequiredFields();

    $('#{0}').on('blur', function () {{
        testRequiredFields();
    }})

    $('#{1}').on('blur', function () {{
        testRequiredFields();
    }})

    function testRequiredFields() {{
        var hasValue = $('#{0}').val() != '' || $('#{1}').val() != '';
        enableRequiredFields( hasValue );

    }}
",
                tbFirstName2.ClientID,
                tbLastName2.ClientID
            );

            ScriptManager.RegisterStartupScript( tbFirstName2, tbFirstName2.GetType(), "adult2-validation", script, true );

        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            SetControls();
        }

        /// <summary>
        /// Handles the AddChildClick event of the prChildren control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void prChildren_AddChildClick( object sender, EventArgs e )
        {
            AddChild();
            CreateChildrenControls( true );
        }

        /// <summary>
        /// Handles the DeleteClick event of the ChildRow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void ChildRow_DeleteClick( object sender, EventArgs e )
        {
            var row = sender as PreRegistrationChildRow;
            var child = Children.FirstOrDefault( m => m.Guid.Equals( row.PersonGuid ) );
            if ( child != null )
            {
                Children.Remove( child );
            }

            CreateChildrenControls( true );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( ValidateInfo() )
            {
                // Get some system values
                var familyGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );
                var adultRoleId = familyGroupType.Roles.FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ).Id;
                var childRoleId = familyGroupType.Roles.FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ).Id;

                var recordTypePersonId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
                var recordStatusValue = DefinedValueCache.Read( GetAttributeValue( "RecordStatus" ).AsGuid() ) ?? DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
                var connectionStatusValue = DefinedValueCache.Read( GetAttributeValue( "ConnectionStatus" ).AsGuid() ) ?? DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR.AsGuid() );

                var knownRelationshipGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );
                var canCheckInRole = knownRelationshipGroupType.Roles.FirstOrDefault( r => r.Guid == Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_CAN_CHECK_IN.AsGuid() );
                var knownRelationshipOwnerRoleGuid = Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid();


                // ...and some block settings
                var familyRelationships = GetAttributeValue( "FamilyRelationships" ).SplitDelimitedValues().AsIntegerList();
                var canCheckinRelationships = GetAttributeValue( "CanCheckinRelationships" ).SplitDelimitedValues().AsIntegerList();

                // ...and some service objects
                var personService = new PersonService( _rockContext );
                var groupService = new GroupService( _rockContext );
                var groupMemberService = new GroupMemberService( _rockContext );
                var groupLocationService = new GroupLocationService( _rockContext );

                // Check to see if we're viewing an existing family
                Group primaryFamily = null;
                Guid? familyGuid = hfFamilyGuid.Value.AsGuidOrNull();
                if ( familyGuid.HasValue )
                {
                    primaryFamily = groupService.Get( familyGuid.Value );
                }

                // If editing an existing family, we'll need to handle any children/relationships that they remove
                bool processRemovals = primaryFamily != null && GetAttributeValue( "AllowUpdates" ).AsBoolean();

                // If editing an existing family, we should also save any empty family values (campus, address)
                bool saveEmptyValues = primaryFamily != null;

                // Save the adults
                var adultIds = new List<int>();
                SaveAdult( ref primaryFamily, adultIds, 1, hfAdultGuid1, tbFirstName1, tbLastName1, dvpSuffix1, ddlGender1, dpBirthDate1, dvpMaritalStatus1, tbEmail1, pnMobilePhone1, phAttributes1 );
                SaveAdult( ref primaryFamily, adultIds, 2, hfAdultGuid2, tbFirstName2, tbLastName2, dvpSuffix2, ddlGender2, dpBirthDate2, dvpMaritalStatus2, tbEmail2, pnMobilePhone2, phAttributes2 );

                // If two adults were entered, let's check to see if we should assume they're married
                if ( adultIds.Count == 2 )
                {
                    var marriedStatusValue = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_MARITAL_STATUS_MARRIED.AsGuid() );
                    if ( marriedStatusValue != null )
                    {
                        var adults = personService.Queryable().Where( p => adultIds.Contains( p.Id ) ).ToList();

                        // as long as neither of the adults has a marital status
                        if ( !adults.Any( a => a.MaritalStatusValueId.HasValue ) )
                        {
                            // Set them all to married
                            foreach ( var adult in adults )
                            {
                                adult.MaritalStatusValueId = marriedStatusValue.Id;
                            }
                            _rockContext.SaveChanges();
                        }
                    }
                }

                // If we do have an existing family, set it's campus if the campus selection was visible
                if ( primaryFamily != null )
                {
                    if ( pnlCampus.Visible )
                    {
                        primaryFamily.CampusId = cpCampus.SelectedValueAsInt();
                    }
                }
                else
                {
                    // Otherwise, create a new family and save it
                    primaryFamily = CreateNewFamily( familyGroupType.Id, ( tbLastName1.Text.IsNotNullOrWhitespace() ? tbLastName1.Text : tbLastName2.Text ) );
                    groupService.Add( primaryFamily );
                    saveEmptyValues = true;
                }

                // Save the family
                _rockContext.SaveChanges();

                // Make sure adults are part of the primary family, and if not, add them.
                foreach( int id in adultIds )
                {
                    var currentFamilyMember = primaryFamily.Members.FirstOrDefault( m => m.PersonId == id );
                    if ( currentFamilyMember == null )
                    {
                        currentFamilyMember = new GroupMember();
                        groupMemberService.Add( currentFamilyMember );

                        currentFamilyMember.GroupId = primaryFamily.Id;
                        currentFamilyMember.PersonId = id;
                        currentFamilyMember.GroupRoleId = adultRoleId;
                        currentFamilyMember.GroupMemberStatus = GroupMemberStatus.Active;

                        _rockContext.SaveChanges();
                    }
                }

                // Save the family address
                var homeLocationType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
                if ( homeLocationType != null )
                {
                    // Find a location record for the address that was entered
                    var loc = new Location();
                    acAddress.GetValues( loc );
                    if ( acAddress.Street1.IsNotNullOrWhitespace() && loc.City.IsNotNullOrWhitespace() )
                    {
                        loc = new LocationService( _rockContext ).Get(
                            loc.Street1, loc.Street2, loc.City, loc.State, loc.PostalCode, loc.Country, primaryFamily, true );
                    }
                    else
                    {
                        loc = null;
                    }

                    // Check to see if family has an existing home address
                    var groupLocation = primaryFamily.GroupLocations
                        .FirstOrDefault( l =>
                            l.GroupLocationTypeValueId.HasValue &&
                            l.GroupLocationTypeValueId.Value == homeLocationType.Id );

                    if ( loc != null )
                    {
                        if ( groupLocation == null || groupLocation.LocationId != loc.Id )
                        {
                            // If family does not currently have a home address or it is different than the one entered, add a new address (move old address to prev)
                            GroupService.AddNewGroupAddress( _rockContext, primaryFamily, homeLocationType.Guid.ToString(), loc, true, string.Empty, true, true );
                        }
                    }
                    else
                    {
                        if ( groupLocation != null && saveEmptyValues )
                        {
                            // If an address was not entered, and family has one on record, update it to be a previous address
                            var prevLocationType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_PREVIOUS.AsGuid() );
                            if ( prevLocationType != null )
                            {
                                groupLocation.GroupLocationTypeValueId = prevLocationType.Id;
                            }
                        }
                    }

                    _rockContext.SaveChanges();
                }

                // Save any family attribute values
                primaryFamily.LoadAttributes( _rockContext );
                Helper.GetEditValues( phFamilyAttributes, primaryFamily );
                primaryFamily.SaveAttributeValues( _rockContext );

                // Get the adult known relationship groups
                var knownRelationshipGroupIds = groupMemberService.Queryable()
                    .Where( m =>
                        m.GroupRole.Guid == knownRelationshipOwnerRoleGuid &&
                        adultIds.Contains( m.PersonId ) )
                    .Select( m => m.GroupId )
                    .ToList();

                // Variables for tracking the new children/relationships that should exist
                var newChildIds = new List<int>();
                var newRelationships = new Dictionary<int, List<int>>();

                // Loop through each of the children
                var newFamilyIds = new Dictionary<string, int>();
                foreach ( var child in Children )
                {
                    // Save the child's person information
                    Person person = personService.Get( child.Guid );
                    if ( person == null )
                    {
                        person = new Person();
                        personService.Add( person );

                        person.Guid = child.Guid;
                        person.RecordTypeValueId = recordTypePersonId;
                        person.RecordStatusReasonValueId = recordStatusValue != null ? recordStatusValue.Id : (int?)null;
                        person.ConnectionStatusValueId = connectionStatusValue != null ? connectionStatusValue.Id : (int?)null;
                    }

                    person.NickName = child.NickName;
                    person.LastName = child.LastName;
                    person.SuffixValueId = child.SuffixValueId;
                    person.Gender = child.Gender;
                    person.SetBirthDate( child.BirthDate );
                    person.GradeOffset = child.GradeOffset;

                    _rockContext.SaveChanges();

                    // Save the attributes for the child
                    person.LoadAttributes();
                    foreach( var keyVal in child.AttributeValues )
                    {
                        person.SetAttributeValue( keyVal.Key, keyVal.Value );
                    }
                    person.SaveAttributeValues( _rockContext );

                    // Get the child's current family state
                    bool inPrimaryFamily = primaryFamily.Members.Any( m => m.PersonId == person.Id );

                    // Get what the family/relationship state should be for the child
                    bool shouldBeInPrimaryFamily = familyRelationships.Contains( child.RelationshipType ?? 0 );
                    int? newRelationshipId = shouldBeInPrimaryFamily ? (int?)null : child.RelationshipType;
                    bool canCheckin = !shouldBeInPrimaryFamily && canCheckinRelationships.Contains( child.RelationshipType ?? -1 );

                    // Check to see if child needs to be added to the primary family or not
                    if ( shouldBeInPrimaryFamily )
                    {
                        // If so, add to list of children
                        newChildIds.Add( person.Id );

                        if ( !inPrimaryFamily )
                        {
                            var familyMember = new GroupMember();
                            groupMemberService.Add( familyMember );

                            familyMember.GroupId = primaryFamily.Id;
                            familyMember.PersonId = person.Id;
                            familyMember.GroupRoleId = childRoleId;
                            familyMember.GroupMemberStatus = GroupMemberStatus.Active;

                            _rockContext.SaveChanges();
                        }
                    }
                    else
                    {
                        // Make sure they have another family
                        EnsurePersonInOtherFamily( familyGroupType.Id, primaryFamily.Id, person.Id, person.LastName, childRoleId, newFamilyIds );

                        // If the selected relationship for this person, should also create the can-check in relationship, make sure to add it
                        if ( canCheckinRelationships.Contains( child.RelationshipType ?? -1 ) )
                        {
                            foreach ( var adultId in adultIds )
                            {
                                groupMemberService.CreateKnownRelationship( adultId, person.Id, canCheckInRole.Id );
                                newRelationships.AddOrIgnore( person.Id, new List<int>() );
                                newRelationships[person.Id].Add( canCheckInRole.Id );
                            }
                        }
                    }

                    // Check to see if child needs to be removed from the primary family
                    if ( !shouldBeInPrimaryFamily && inPrimaryFamily )
                    {
                        RemovePersonFromFamily( familyGroupType.Id, primaryFamily.Id, person.Id );
                    }

                    // If child has a relationship type, make sure they belong to a family, and ensure that they have that relationship with each adult
                    if ( newRelationshipId.HasValue )
                    {
                        foreach ( var adultId in adultIds )
                        {
                            groupMemberService.CreateKnownRelationship( adultId, person.Id, newRelationshipId.Value );
                            newRelationships.AddOrIgnore( person.Id, new List<int>() );
                            newRelationships[person.Id].Add( newRelationshipId.Value );
                        }
                    }
                }

                // If editing an existing family, check for any people that need to be removed from the family or relationships
                if ( processRemovals )
                {
                    // Find all the existing children that were removed and make sure they have another family, and then remove them from this family.
                    var na = new Dictionary<string, int>();
                    foreach ( var removedChild in groupMemberService.Queryable()
                        .Where( m =>
                            m.GroupId == primaryFamily.Id &&
                            m.GroupRoleId == childRoleId &&
                            !newChildIds.Contains( m.PersonId ) ) )
                    {
                        EnsurePersonInOtherFamily( familyGroupType.Id, primaryFamily.Id, removedChild.Id, removedChild.Person.LastName, childRoleId, na );
                        groupMemberService.Delete( removedChild );
                    }
                    _rockContext.SaveChanges();

                    // Find all the existing relationships that were removed and delete them
                    var roleIds = _relationshipTypes.Select( r => r.Key ).ToList();
                    foreach ( var groupMember in new PersonService( _rockContext )
                        .GetRelatedPeople( adultIds, roleIds ) )
                    {
                        if ( !newRelationships.ContainsKey(groupMember.PersonId) || !newRelationships[groupMember.PersonId].Contains( groupMember.GroupRoleId ) )
                        {
                            foreach ( var adultId in adultIds )
                            {
                                groupMemberService.DeleteKnownRelationship( adultId, groupMember.PersonId, groupMember.GroupRoleId );
                                if ( canCheckinRelationships.Contains( groupMember.GroupRoleId ) )
                                {
                                    groupMemberService.DeleteKnownRelationship( adultId, groupMember.PersonId, canCheckInRole.Id );
                                }
                            }
                        }
                    }
                }

                List<Guid> workflows = GetAttributeValue( "WorkflowTypes" ).SplitDelimitedValues().AsGuidList();
                string redirectUrl = GetAttributeValue( "RedirectURL" );
                if ( workflows.Any() || redirectUrl.IsNotNullOrWhitespace() )
                {
                    var family = groupService.Get( primaryFamily.Id );

                    var childIds = new List<int>( newChildIds );
                    childIds.AddRange( newRelationships.Select( r => r.Key ).ToList() );

                    // Create parameters
                    var parameters = new Dictionary<string, string>();
                    parameters.Add( "ParentIds", adultIds.AsDelimited( "," ) );
                    parameters.Add( "ChildIds", childIds.AsDelimited( "," ) );

                    if ( pnlPlannedDate.Visible )
                    {
                        DateTime? visitDate = dpPlannedDate.SelectedDate;
                        if ( visitDate.HasValue )
                        {
                            parameters.Add( "PlannedVisitDate", visitDate.Value.ToString( "o" ) );
                        }
                    }

                    // Look for any workflows
                    if ( workflows.Any() )
                    {
                        // Launch all the workflows
                        foreach ( var wfGuid in workflows )
                        {
                            family.LaunchWorkflow( wfGuid, family.Name, parameters );
                        }
                    }

                    if ( redirectUrl.IsNotNullOrWhitespace() )
                    {
                        var relatedPersonIds = newRelationships.Select( r => r.Key ).ToList();
                        var relatedChildren = personService.Queryable().Where( p => relatedPersonIds.Contains( p.Id ) ).ToList();

                        var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                        mergeFields.Add( "Family", family );
                        mergeFields.Add( "RelatedChildren", relatedChildren );
                        foreach( var keyval in parameters )
                        {
                            mergeFields.Add( keyval.Key, keyval.Value );
                        }

                        var url = ResolveUrl( redirectUrl.ResolveMergeFields( mergeFields ) );

                        Response.Redirect( url, false );
                        Context.ApplicationInstance.CompleteRequest();
                    }
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the intial visibility/required properties of controls based on block attribute values
        /// </summary>
        private void SetControls()
        {
            pnlVisit.Visible = true;

            // Campus 
            if ( GetAttributeValue( "ShowCampus" ).AsBoolean() )
            {
                cpCampus.Campuses = CampusCache.All( false );
                pnlCampus.Visible = true;
            }
            else
            {
                pnlCampus.Visible = false;
            }

            // Planned Visit Date
            dpPlannedDate.Required = SetControl( "PlannedVisitDate", pnlPlannedDate, null );

            // Visit Info
            pnlVisit.Visible = pnlCampus.Visible || pnlPlannedDate.Visible;

            // Adult Suffix
            bool isRequired = SetControl( "AdultSuffix", pnlSuffix1, pnlSuffix2 );
            dvpSuffix1.Required = isRequired;
            dvpSuffix2.Required = isRequired;
            var suffixDt = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.PERSON_SUFFIX.AsGuid() );
            dvpSuffix1.BindToDefinedType( suffixDt, true );
            dvpSuffix2.BindToDefinedType( suffixDt, true );

            // Adult Gender
            isRequired = SetControl( "AdultGender", pnlGender1, pnlGender2 );
            ddlGender1.Required = isRequired;
            ddlGender2.Required = isRequired;
            ddlGender1.BindToEnum<Gender>( true, new Gender[] { Gender.Unknown } );
            ddlGender2.BindToEnum<Gender>( true, new Gender[] { Gender.Unknown } );

            // Adult Birthdate
            isRequired = SetControl( "AdultBirthDate", pnlBirthDate1, pnlBirthDate2 );
            dpBirthDate1.Required = isRequired;
            dpBirthDate2.Required = isRequired;

            // Adult Marital Status
            isRequired = SetControl( "AdultMaritalStatus", pnlMaritalStatus1, pnlMaritalStatus2 );
            dvpMaritalStatus1.Required = isRequired;
            dvpMaritalStatus2.Required = isRequired;
            var MaritalStatusDt = DefinedTypeCache.Read( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid() );
            dvpMaritalStatus1.BindToDefinedType( MaritalStatusDt, true );
            dvpMaritalStatus2.BindToDefinedType( MaritalStatusDt, true );

            // Adult Email
            isRequired = SetControl( "AdultEmail", pnlEmail1, pnlEmail2 );
            tbEmail1.Required = isRequired;
            tbEmail2.Required = isRequired;

            // Adult Mobile Phone
            isRequired = SetControl( "AdultMobilePhone", pnlMobilePhone1, pnlMobilePhone2 );
            pnMobilePhone1.Required = isRequired;
            pnMobilePhone2.Required = isRequired;

            // Check for Current Family
            SetCurrentFamilyValues();

            // Build the dynamic children controls
            CreateChildrenControls( true );
        }

        /// <summary>
        /// Sets the current family values.
        /// </summary>
        private void SetCurrentFamilyValues()
        {
            Group family = null;
            Person adult1 = null;
            Person adult2 = null;
            int? maritalStatusId = null;

            // If there is a logged in person, attempt to find their family and spouse.
            if ( GetAttributeValue("AllowUpdates").AsBoolean() && CurrentPerson != null )
            {
                Person spouse = null;

                // Get all their families
                var families = CurrentPerson.GetFamilies( _rockContext );
                if ( families.Any() )
                {
                    // Get their spouse
                    spouse = CurrentPerson.GetSpouse( _rockContext );
                    if ( spouse != null )
                    {
                        // If spouse was found, find the first family that spouse beongs to also.
                        family = families.Where( f => f.Members.Any( m => m.PersonId == spouse.Id ) ).FirstOrDefault();
                        if ( family == null )
                        {
                            // If there was not family with spouse, something went wrong and assume there is no spouse.
                            spouse = null;
                        }
                    }

                    // If we didn't find a family yet (by checking spouses family), assume the first family.
                    if ( family == null )
                    {
                        family = families.FirstOrDefault();
                    }

                    // Assume Adult1 is the current person
                    adult1 = CurrentPerson;
                    if ( spouse != null )
                    {
                        // and Adult2 is the spouse
                        adult2 = spouse;

                        // However, if spouse is actually head of family, make them Adult1 and current person Adult2
                        var headOfFamilyId = family.Members
                            .OrderBy( m => m.GroupRole.Order )
                            .ThenBy( m => m.Person.Gender )
                            .Select( m => m.PersonId )
                            .FirstOrDefault();
                        if ( headOfFamilyId != 0 && headOfFamilyId == spouse.Id )
                        {
                            adult1 = spouse;
                            adult2 = CurrentPerson;
                        }
                    }
                }
            }

            // Set First Adult's Values
            hfAdultGuid1.Value = adult1 != null ? adult1.Id.ToString() : string.Empty;

            lFirstName1.Visible = adult1 != null;
            tbFirstName1.Visible = adult1 == null;
            lFirstName1.Text = adult1 != null ? adult1.NickName : String.Empty;
            tbFirstName1.Text = adult1 != null ? adult1.NickName : String.Empty;

            lLastName1.Visible = adult1 != null;
            tbLastName1.Visible = adult1 == null;
            lLastName1.Text = adult1 != null ? adult1.LastName : String.Empty;
            tbLastName1.Text = adult1 != null ? adult1.LastName : String.Empty;

            dvpSuffix1.SetValue( adult1 != null ? adult1.SuffixValueId : (int?)null );
            ddlGender1.SetValue( adult1 != null ? adult1.Gender.ConvertToInt() : 0 );
            dpBirthDate1.SelectedDate = ( adult1 != null ? adult1.BirthDate : (DateTime?)null );
            dvpMaritalStatus1.SetValue( adult1 != null ? adult1.MaritalStatusValueId : (int?)null );
            tbEmail1.Text = ( adult1 != null ? adult1.Email : string.Empty );
            SetPhoneNumber( adult1, pnMobilePhone1 );

            // Set Second Adult's Values
            hfAdultGuid2.Value = adult2 != null ? adult2.Guid.ToString() : string.Empty;

            lFirstName2.Visible = adult2 != null;
            tbFirstName2.Visible = adult2 == null;
            lFirstName2.Text = adult2 != null ? adult2.NickName : String.Empty;
            tbFirstName2.Text = adult2 != null ? adult2.NickName : String.Empty;

            lLastName2.Visible = adult2 != null;
            tbLastName2.Visible = adult2 == null;
            lLastName2.Text = adult2 != null ? adult2.LastName : String.Empty;
            tbLastName2.Text = adult2 != null ? adult2.LastName : String.Empty;

            dvpSuffix2.SetValue( adult2 != null ? adult2.SuffixValueId : (int?)null );
            ddlGender2.SetValue( adult2 != null ? adult2.Gender.ConvertToInt() : 0 );
            dpBirthDate2.SelectedDate = ( adult2 != null ? adult2.BirthDate : (DateTime?)null );
            dvpMaritalStatus2.SetValue( adult2 != null ? adult2.MaritalStatusValueId : (int?)null );
            tbEmail2.Text = ( adult2 != null ? adult2.Email : string.Empty );
            SetPhoneNumber( adult2, pnMobilePhone2 );

            Children = new List<PreRegistrationChild>();

            if ( family != null )
            {
                // Set the campus from the family
                cpCampus.SetValue( family.CampusId );

                // Set the address from the family
                var homeLocationType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
                if ( homeLocationType != null )
                {
                    var location = family.GroupLocations
                        .Where( l =>
                            l.GroupLocationTypeValueId.HasValue &&
                            l.GroupLocationTypeValueId.Value == homeLocationType.Id )
                        .Select( l => l.Location )
                        .FirstOrDefault();
                    acAddress.SetValues( location );
                }
                else
                {
                    acAddress.SetValues( null );
                }

                // Find all the children in the family
                var childRoleGuid = Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid();
                foreach( var groupMember in family.Members
                    .Where( m => m.GroupRole.Guid == childRoleGuid )
                    .OrderByDescending( m => m.Person.Age ) )
                {
                    var child = new PreRegistrationChild( groupMember.Person );
                    child.RelationshipType = 0;
                    Children.Add( child );
                }

                // Find all the related people.
                var adultIds = new List<int>();
                if ( adult1 != null )
                {
                    adultIds.Add( adult1.Id );
                }
                if ( adult2 != null )
                {
                    adultIds.Add( adult2.Id );
                }
                var roleIds = _relationshipTypes.Select( r => r.Key ).ToList();
                foreach ( var groupMember in new PersonService( _rockContext )
                    .GetRelatedPeople( adultIds, roleIds )
                    .OrderBy( m => m.GroupRole.Order )
                    .ThenByDescending( m => m.Person.Age ) )
                {
                    if ( !Children.Any( c => c.Id == groupMember.PersonId ) )
                    {
                        var child = new PreRegistrationChild( groupMember.Person );
                        child.RelationshipType = groupMember.GroupRoleId;
                        Children.Add( child );
                    }
                }

                hfFamilyGuid.Value = family.Guid.ToString();
            }
            else
            {
                // Set campus to the default
                Guid? campusGuid = GetAttributeValue( "DefaultCampus" ).AsGuidOrNull();
                if ( campusGuid.HasValue )
                {
                    var defaultCampus = CampusCache.Read( campusGuid.Value );
                    if ( defaultCampus != null )
                    {
                        cpCampus.SetValue( defaultCampus.Id );
                    }
                }

                // Clear the address
                acAddress.SetValues( null );

                hfFamilyGuid.Value = string.Empty;
            }

            // Adult Attributes
            BuildAdultAttributes( true, adult1, adult2 );

            // Family Attributes
            BuildFamilyAttributes( true, family );
        }

        /// <summary>
        /// Helper method to set visibilty of adult controls and return if it's required or not.
        /// </summary>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="adultControl1">The adult control1.</param>
        /// <param name="adultControl2">The adult control2.</param>
        /// <returns></returns>
        private bool SetControl( string attributeKey, WebControl adultControl1, WebControl adultControl2 )
        {
            string attributeValue = GetAttributeValue( attributeKey );
            if ( adultControl1 != null )
            {
                adultControl1.Visible = attributeValue != "Hide";
            }
            if ( adultControl2 != null )
            {
                adultControl2.Visible = attributeValue != "Hide";
            }
            return attributeValue == "Required";
        }

        /// <summary>
        /// Builds the adult attributes.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void BuildAdultAttributes( bool setValues, Person adult1, Person adult2 )
        {
            phAttributes1.Controls.Clear();
            phAttributes2.Controls.Clear();

            var attributeList = GetCategoryAttributeList( "AdultAttributeCategories" );

            if ( adult1 != null )
            {
                adult1.LoadAttributes();
            }
            if ( adult2 != null )
            {
                adult2.LoadAttributes();
            }

            foreach( var attribute in attributeList )
            {
                string value1 = adult1 != null ? adult1.GetAttributeValue( attribute.Key ) : string.Empty;
                var div1 = new HtmlGenericControl( "Div" );
                phAttributes1.Controls.Add( div1 );
                div1.AddCssClass( "col-sm-3" );
                var ctrl1 = attribute.AddControl( div1.Controls, value1, this.BlockValidationGroup, setValues, false, attribute.IsRequired );
                ctrl1.ID = string.Format( "attribute_field_{0}_1", attribute.Id );

                string value2 = adult2 != null ? adult2.GetAttributeValue( attribute.Key ) : string.Empty;
                var div2 = new HtmlGenericControl( "Div" );
                phAttributes2.Controls.Add( div2 );
                div2.AddCssClass( "col-sm-3" );
                var ctrl2 = attribute.AddControl( div2.Controls, value2, this.BlockValidationGroup, setValues, false, attribute.IsRequired );
                ctrl2.ID = string.Format( "attribute_field_{0}_2", attribute.Id );
            }
        }

        /// <summary>
        /// Builds the family attributes.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void BuildFamilyAttributes( bool setValues, Group family )
        {
            phFamilyAttributes.Controls.Clear();

            var attributeList = GetAttributeList( "FamilyAttributes" );

            if ( family != null )
            {
                family.LoadAttributes();
            }

            foreach ( var attribute in attributeList )
            {
                string value = family != null ? family.GetAttributeValue( attribute.Key ) : string.Empty;
                attribute.AddControl( phFamilyAttributes.Controls, value, this.BlockValidationGroup, setValues, true, attribute.IsRequired );
            }
        }

        /// <summary>
        /// Helper method to set a phone number control's value.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="pnb">The PNB.</param>
        private void SetPhoneNumber( Person person, PhoneNumberBox pnb )
        {
            if ( person != null )
            {
                var pn = person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
                if ( pn != null )
                {
                    pnb.CountryCode = pn.CountryCode;
                    pnb.Number = pn.ToString();
                }
                else
                {
                    pnb.CountryCode = PhoneNumber.DefaultCountryCode();
                    pnb.Number = string.Empty;
                }
            }
            else
            {
                pnb.CountryCode = PhoneNumber.DefaultCountryCode();
                pnb.Number = string.Empty;
            }
        }

        /// <summary>
        /// Adds a new child.
        /// </summary>
        private void AddChild()
        {
            var person = new Person();
            person.Guid = Guid.NewGuid();
            person.Gender = Gender.Unknown;
            person.LastName = tbLastName1.Text;
            person.GradeOffset = null;

            var child = new PreRegistrationChild( person );

            Children.Add( child );
        }

        /// <summary>
        /// Creates the children controls.
        /// </summary>
        private void CreateChildrenControls( bool setSelection )
        {
            prChildren.ClearRows();

            var showSuffix = GetAttributeValue( "ChildSuffix" ) != "Hide";
            var showGender = GetAttributeValue( "ChildGender" ) != "Hide";
            var requireGender = GetAttributeValue( "ChildGender" ) == "Required";
            var showBirthDate = GetAttributeValue( "ChildBirthdate" ) != "Hide";
            var requireBirthDate = GetAttributeValue( "ChildBirthdate" ) == "Required";
            var showGrade = GetAttributeValue( "ChildGrade" ) != "Hide";
            var requireGrade = GetAttributeValue( "ChildGrade" ) == "Required";
            var showMobilePhone = GetAttributeValue( "ChildMobilePhone" ) != "Hide";
            var requireMobilePhone = GetAttributeValue( "ChildMobilePhone" ) == "Required";

            var attributeList = GetCategoryAttributeList( "ChildAttributeCategories" );

            foreach ( var child in Children )
            {
                if ( child != null )
                {
                    var childRow = new PreRegistrationChildRow();
                    childRow.ValidationGroup = this.BlockValidationGroup;

                    prChildren.Controls.Add( childRow );

                    childRow.DeleteClick += ChildRow_DeleteClick;
                    string childGuidString = child.Guid.ToString().Replace( "-", "_" );
                    childRow.ID = string.Format( "row_{0}", childGuidString );
                    childRow.PersonId = child.Id;
                    childRow.PersonGuid = child.Guid;

                    childRow.ShowSuffix = showSuffix;
                    childRow.ShowGender = showGender;
                    childRow.RequireGender = requireGender;
                    childRow.ShowBirthDate = showBirthDate;
                    childRow.RequireBirthDate = requireBirthDate;
                    childRow.ShowGrade = showGrade;
                    childRow.RequireGrade = requireGrade;
                    childRow.ShowMobilePhone = showMobilePhone;
                    childRow.RequireMobilePhone = requireMobilePhone;
                    childRow.RelationshipTypeList = _relationshipTypes;
                    childRow.AttributeList = attributeList;

                    childRow.ValidationGroup = BlockValidationGroup;

                    if ( setSelection )
                    {
                        childRow.NickName = child.NickName;
                        childRow.LastName = child.LastName;
                        childRow.SuffixValueId = child.SuffixValueId;
                        childRow.Gender = child.Gender;
                        childRow.BirthDate = child.BirthDate;
                        childRow.GradeOffset = child.GradeOffset;
                        childRow.RelationshipType = child.RelationshipType;

                        childRow.SetAttributeValues( child );
                    }

                }
            }
        }

        private void SaveAdult( ref Group primaryFamily, List<int> adultIds, int adultNumber,
            HiddenField hfAdultGuid,
            RockTextBox tbFirstName,
            RockTextBox tbLastName,
            DefinedValuePicker dvpSuffix,
            RockDropDownList ddlGender,
            DatePicker dpBirthDate,
            DefinedValuePicker dvpMaritalStatus,
            EmailBox tbEmail,
            PhoneNumberBox pnMobilePhone,
            DynamicPlaceholder phAttributes )
        {
            var familyGroupType = GroupTypeCache.Read( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY.AsGuid() );

            var recordTypePersonId = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;
            var recordStatusValue = DefinedValueCache.Read( GetAttributeValue( "RecordStatus" ).AsGuid() ) ?? DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() );
            var connectionStatusValue = DefinedValueCache.Read( GetAttributeValue( "ConnectionStatus" ).AsGuid() ) ?? DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_VISITOR.AsGuid() );

            var showSuffix = GetAttributeValue( "AdultSuffix" ) != "Hide";
            var showGender = GetAttributeValue( "AdultGender" ) != "Hide";
            var showBirthDate = GetAttributeValue( "AdultBirthdate" ) != "Hide";
            var showMaritalStatus = GetAttributeValue( "AdultMaritalStatus" ) != "Hide";
            var showEmail = GetAttributeValue( "AdultEmail" ) != "Hide";
            var showMobilePhone = GetAttributeValue( "AdultMobilePhone" ) != "Hide";
            bool autoMatch = GetAttributeValue( "AutoMatch" ).AsBoolean();

            var personService = new PersonService( _rockContext );

            // Get the adult if we're editing an existing person
            Person adult = null;
            Guid? adultGuid = hfAdultGuid.Value.AsGuidOrNull();
            if ( adultGuid.HasValue )
            {
                adult = personService.Get( adultGuid.Value );
            }

            // Check to see if this is an existing person, or a name was entered for this adult
            if ( adult != null || ( tbFirstName.Text.IsNotNullOrWhitespace() && tbLastName.Text.IsNotNullOrWhitespace() ) )
            {
                // Flag indicating if empty values should be saved to person record (Should not do this if a matched record was found)
                bool saveEmptyValues = true;

                // If not editing an existing person, attempt to match them to existing (if configured to do so)
                if ( adult == null && showEmail && autoMatch )
                {
                    var people = personService.GetByMatch( tbFirstName.Text, tbLastName.Text, tbEmail.Text );
                    if ( people.Count() == 1 )
                    {
                        adult = people.First();
                        saveEmptyValues = false;
                        if ( primaryFamily == null )
                        {
                            primaryFamily = adult.GetFamily( _rockContext );
                        }
                    }
                }

                // If this is a new person, add them.
                if ( adult == null )
                {
                    adult = new Person();
                    personService.Add( adult );

                    adult.NickName = tbFirstName.Text;
                    adult.LastName = tbLastName.Text;
                    adult.RecordTypeValueId = recordTypePersonId;
                    adult.RecordStatusReasonValueId = recordStatusValue != null ? recordStatusValue.Id : (int?)null;
                    adult.ConnectionStatusValueId = connectionStatusValue != null ? connectionStatusValue.Id : (int?)null;
                }

                // Set the properties from UI
                if ( showSuffix )
                {
                    int? suffix = dvpSuffix.SelectedValueAsInt();
                    if ( suffix.HasValue || saveEmptyValues )
                    {
                        adult.SuffixValueId = suffix;
                    }
                }

                if ( showGender )
                {
                    var gender = ddlGender.SelectedValueAsEnumOrNull<Gender>();
                    if ( gender.HasValue || saveEmptyValues )
                    {
                        adult.Gender = gender ?? Gender.Unknown;
                    }
                }

                if ( showBirthDate )
                {
                    var birthDate = dpBirthDate.SelectedDate;
                    if ( birthDate.HasValue || saveEmptyValues )
                    {
                        adult.SetBirthDate( birthDate );
                    }
                }

                if ( showMaritalStatus )
                {
                    int? maritalStatus = dvpMaritalStatus.SelectedValueAsInt();
                    if ( maritalStatus.HasValue || saveEmptyValues )
                    {
                        adult.MaritalStatusValueId = maritalStatus;
                    }
                }

                if ( showEmail )
                {
                    if ( tbEmail.Text.IsNotNullOrWhitespace() || saveEmptyValues )
                    {
                        adult.Email = tbEmail.Text;
                    }
                }

                // Save the person
                _rockContext.SaveChanges();

                // Save the mobile phone number
                if ( showMobilePhone )
                {
                    SavePhoneNumber( adult.Id, pnMobilePhone1 );
                }

                // Save any attribute values
                adult.LoadAttributes( _rockContext );
                GetAdultAttributeValues( phAttributes, adult, adultNumber, saveEmptyValues );
                adult.SaveAttributeValues( _rockContext );

                adultIds.Add( adult.Id );
            }

        }

        /// <summary>
        /// Gets the adult attribute values.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="person">The person.</param>
        /// <param name="adultNumber">The adult number.</param>
        private void GetAdultAttributeValues( Control parentControl, Person person, int adultNumber, bool setEmptyValue )
        {
            if ( person.Attributes != null )
            {
                foreach ( var attribute in person.Attributes )
                {
                    Control control = parentControl.FindControl( string.Format( "attribute_field_{0}_{1}", attribute.Value.Id, adultNumber ) );
                    if ( control != null )
                    {
                        var value = new AttributeValueCache();
                        value.Value = attribute.Value.FieldType.Field.GetEditValue( control, attribute.Value.QualifierValues );
                        if ( setEmptyValue || value.Value.IsNotNullOrWhitespace() )
                        {
                            person.AttributeValues[attribute.Key] = value;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the children data.
        /// </summary>
        private void GetChildrenData()
        {
            Children = new List<PreRegistrationChild>();

            foreach( var childRow in prChildren.ChildRows )
            {
                var person = new Person();
                person.Id = childRow.PersonId;
                person.Guid = childRow.PersonGuid ?? Guid.NewGuid();
                person.NickName = childRow.NickName;
                person.LastName = childRow.LastName;
                person.SuffixValueId = childRow.SuffixValueId;
                person.Gender = childRow.Gender;
                person.SetBirthDate( childRow.BirthDate );
                person.GradeOffset = childRow.GradeOffset;
                person.LoadAttributes();

                var child = new PreRegistrationChild( person );
                child.RelationshipType = childRow.RelationshipType;

                var attributeKeys = GetCategoryAttributeList( "ChildAttributeCategories" ).Select( a => a.Key ).ToList();
                child.AttributeValues = person.AttributeValues
                    .Where( a => attributeKeys.Contains( a.Key ) )
                    .ToDictionary( v => v.Key, v => v.Value.Value );

                childRow.GetAttributeValues( child );

                Children.Add( child );
            }
        }

        /// <summary>
        /// Gets the attributes based on a block setting of attribute categories (adult/child attributes).
        /// </summary>
        /// <param name="attributeKey">The attribute key.</param>
        /// <returns></returns>
        private List<AttributeCache> GetCategoryAttributeList( string attributeKey )
        {
            var AttributeList = new List<AttributeCache>();
            foreach ( Guid categoryGuid in GetAttributeValue( attributeKey ).SplitDelimitedValues( false ).AsGuidList() )
            {
                var category = CategoryCache.Read( categoryGuid );
                if ( category != null )
                {
                    foreach ( var attribute in new AttributeService( _rockContext ).GetByCategoryId( category.Id ) )
                    {
                        if ( attribute.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                        {
                            AttributeList.Add( AttributeCache.Read( attribute ) );
                        }
                    }
                }
            }

            return AttributeList;
        }

        /// <summary>
        /// Gets the attributes based on a block setting of attributes (family attributes).
        /// </summary>
        /// <param name="attributeKey">The attribute key.</param>
        /// <returns></returns>
        private List<AttributeCache> GetAttributeList( string attributeKey )
        {
            var AttributeList = new List<AttributeCache>();
            foreach ( Guid attributeGuid in GetAttributeValue( attributeKey ).SplitDelimitedValues( false ).AsGuidList() )
            {
                var attribute = AttributeCache.Read( attributeGuid );
                if ( attribute != null && attribute.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                {
                    AttributeList.Add( attribute );
                }
            }

            return AttributeList;
        }

        /// <summary>
        /// Validates the information being updated
        /// </summary>
        /// <returns></returns>
        private bool ValidateInfo()
        {
            var errorMessages = new List<string>();

            if ( tbFirstName1.Text.IsNullOrWhiteSpace() && tbFirstName2.Text.IsNullOrWhiteSpace() )
            {
                errorMessages.Add( "The name of at least one adult needs to be entered." );
            }

            if (
                ( tbFirstName1.Text.IsNotNullOrWhitespace() && tbLastName1.Text.IsNullOrWhiteSpace() ) ||
                ( tbLastName1.Text.IsNullOrWhiteSpace() && tbLastName1.Text.IsNotNullOrWhitespace() ) ||
                ( tbFirstName2.Text.IsNotNullOrWhitespace() && tbLastName2.Text.IsNullOrWhiteSpace() ) ||
                ( tbLastName2.Text.IsNullOrWhiteSpace() && tbLastName2.Text.IsNotNullOrWhitespace() ) 
            )
            {
                errorMessages.Add( "A First and Last name is required for each person." );
            }

            ValidateRequiredField( "AdultGender", "Gender is required for each adult.", ddlGender1.SelectedValueAsEnumOrNull<Gender>() != null, ddlGender2.SelectedValueAsEnumOrNull<Gender>() != null, errorMessages );
            ValidateRequiredField( "AdultBirthdate", "Birthdate is required for each adult.", dpBirthDate1.SelectedDate != null, dpBirthDate2.SelectedDate != null, errorMessages );
            ValidateRequiredField( "AdultEmail", "Email is required for each adult.", tbEmail1.Text.IsNotNullOrWhitespace(), tbEmail2.Text.IsNotNullOrWhitespace(), errorMessages );
            ValidateRequiredField( "AdultMobilePhone", "Mobile Phone is required for each adult.", PhoneNumber.CleanNumber( pnMobilePhone1.Number ).IsNotNullOrWhitespace(), PhoneNumber.CleanNumber( pnMobilePhone2.Number ).IsNotNullOrWhitespace(), errorMessages );

            if ( errorMessages.Any() )
            {
                nbError.Title = "Please Correct the Following";
                nbError.Text = string.Format( "<ul><li>{0}</li></ul>", errorMessages.AsDelimited( "</li><li>" ) );
                nbError.Visible = true;

                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates the required field.
        /// </summary>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <param name="adult1HasValue">if set to <c>true</c> [adult1 has value].</param>
        /// <param name="adult2HasValue">if set to <c>true</c> [adult2 has value].</param>
        /// <param name="errorMessages">The error messages.</param>
        private void ValidateRequiredField( string attributeKey, string errorMessage, bool adult1HasValue, bool adult2HasValue, List<String> errorMessages )
        {
            if ( GetAttributeValue( attributeKey ) == "Required" )
            {
                if (
                    ( tbFirstName1.Text.IsNotNullOrWhitespace() && !adult1HasValue ) ||
                    ( tbFirstName2.Text.IsNotNullOrWhitespace() && !adult2HasValue )
                )
                {
                    errorMessages.Add( errorMessage );
                }
            }
        }

        /// <summary>
        /// Creates a new family group.
        /// </summary>
        /// <param name="familyGroupTypeId">The family group type identifier.</param>
        /// <returns></returns>
        private Group CreateNewFamily( int familyGroupTypeId, string lastName )
        {
            // If we don't have an existing family, create a new family
            var family = new Group();
            family.Name = lastName + " Family";
            family.GroupTypeId = familyGroupTypeId;

            // If the campus selection was visible, set the families campus based on selection, otherwise, use default campus value
            if ( pnlCampus.Visible )
            {
                family.CampusId = cpCampus.SelectedValueAsInt();
            }
            else
            {
                Guid? campusGuid = GetAttributeValue( "DefaultCampus" ).AsGuidOrNull();
                if ( campusGuid.HasValue )
                {
                    var defaultCampus = CampusCache.Read( campusGuid.Value );
                    if ( defaultCampus != null )
                    {
                        family.CampusId = defaultCampus.Id;
                    }
                }
            }

            return family;
        }

        /// <summary>
        /// Ensures the person in other family.
        /// </summary>
        /// <param name="familyGroupTypeId">The family group type identifier.</param>
        /// <param name="primaryfamilyId">The primaryfamily identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <param name="lastName">The last name.</param>
        /// <param name="childRoleId">The child role identifier.</param>
        /// <param name="newFamilyIds">The new family ids.</param>
        private void EnsurePersonInOtherFamily( int familyGroupTypeId, int primaryfamilyId, int personId, string lastName, int childRoleId, Dictionary<string, int> newFamilyIds )
        {
            var groupMemberService = new GroupMemberService( _rockContext );

            // Get any other family memberships.
            if ( groupMemberService.Queryable()
                .Any( m =>
                    m.Group.GroupTypeId == familyGroupTypeId &&
                    m.PersonId == personId &&
                    m.GroupId != primaryfamilyId ) )
            {
                // They have other families, so just return
                return;
            }

            // Check to see if we've already created a family with someone who has same last name
            string key = lastName.ToLower();
            int? newFamilyId = newFamilyIds.ContainsKey( key ) ? newFamilyIds[key] : (int?)null;

            // If not, create a new family
            if ( !newFamilyId.HasValue )
            {
                var family = CreateNewFamily( familyGroupTypeId, lastName );
                new GroupService( _rockContext ).Add( family );
                _rockContext.SaveChanges();

                newFamilyId = family.Id;
                newFamilyIds.Add( key, family.Id );
            }

            // Add the person to the family
            var familyMember = new GroupMember();
            groupMemberService.Add( familyMember );

            familyMember.GroupId = newFamilyId.Value;
            familyMember.PersonId = personId;
            familyMember.GroupRoleId = childRoleId;
            familyMember.GroupMemberStatus = GroupMemberStatus.Active;

            _rockContext.SaveChanges();
        }

        /// <summary>
        /// Removes a person from a family.
        /// </summary>
        /// <param name="familyGroupTypeId">The family group type identifier.</param>
        /// <param name="familyId">The family identifier.</param>
        /// <param name="personId">The person identifier.</param>
        private void RemovePersonFromFamily( int familyGroupTypeId, int familyId, int personId )
        {
            var groupMemberService = new GroupMemberService( _rockContext );

            // Get all their current group memberships.
            var groupMembers = groupMemberService.Queryable()
                .Where( m =>
                    m.Group.GroupTypeId == familyGroupTypeId &&
                    m.PersonId == personId )
                .ToList();

            // Find their membership in current family, if not found, skip processing, as something is amiss.
            var currentFamilyMembership = groupMembers.FirstOrDefault( m => m.GroupId == familyId );
            if ( currentFamilyMembership != null )
            {
                // If the person does not currently belong to any other families, we'll have to create a new family for them, and move them to that new group
                if ( !groupMembers.Where( m => m.GroupId != familyId ).Any() )
                {
                    var newGroup = new Group();
                    newGroup.Name = currentFamilyMembership.Person.LastName + " Family";
                    newGroup.GroupTypeId = familyGroupTypeId;
                    newGroup.CampusId = currentFamilyMembership.Group.CampusId;
                    new GroupService( _rockContext ).Add( newGroup );
                    _rockContext.SaveChanges();

                    // If person's previous giving group was this family, set it to their new family id
                    if ( currentFamilyMembership.Person.GivingGroupId.HasValue && currentFamilyMembership.Person.GivingGroupId == currentFamilyMembership.GroupId )
                    {
                        currentFamilyMembership.Person.GivingGroupId = newGroup.Id;
                    }

                    currentFamilyMembership.Group = newGroup;
                    _rockContext.SaveChanges();
                }
                else
                {
                    // Otherwise, just remove them from the current family
                    groupMemberService.Delete( currentFamilyMembership );
                    _rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Saves the phone number.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="pnb">The PNB.</param>
        private void SavePhoneNumber( int personId, PhoneNumberBox pnb )
        {
            string phone = PhoneNumber.CleanNumber( pnb.Number );

            var phoneNumberService = new PhoneNumberService( _rockContext );

            var phType = DefinedValueCache.Read( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
            if ( phType != null )
            {
                var phoneNumber = phoneNumberService.Queryable()
                    .Where( n =>
                        n.PersonId == personId &&
                        n.NumberTypeValueId.HasValue &&
                        n.NumberTypeValueId.Value == phType.Id )
                    .FirstOrDefault();

                if ( phone.IsNotNullOrWhitespace() )
                {
                    if ( phoneNumber == null )
                    {
                        phoneNumber = new PhoneNumber();
                        phoneNumberService.Add( phoneNumber );

                        phoneNumber.PersonId = personId;
                        phoneNumber.NumberTypeValueId = phType.Id;
                    }

                    phoneNumber.CountryCode = PhoneNumber.CleanNumber( pnb.CountryCode );
                    phoneNumber.Number = phone;
                }
                else
                {
                    if ( phoneNumber != null )
                    {
                        phoneNumberService.Delete( phoneNumber );
                    }
                }

                _rockContext.SaveChanges();
            }
        }

        #endregion

    }

}


