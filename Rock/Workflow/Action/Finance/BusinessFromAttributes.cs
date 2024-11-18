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
using System.ComponentModel.Composition;
using System.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow.Action
{
    /// <summary>
    /// Sets an attribute to a business based on a set of fields with the ability to also link a person to the business.
    /// </summary>
    [ActionCategory( "Finance" )]
    [Description( "Sets an attribute to a business based on a set of fields with the ability to also link a person to the business." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Business From Attributes" )]

    #region Attributes

    [WorkflowTextOrAttribute(
        "Business Name",
        "Attribute Value",
        Description = "The name or an attribute that contains the name of the business. <span class='tip tip-lava'></span>",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.BusinessName,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType" } )]

    [WorkflowTextOrAttribute(
        "Email",
        "Attribute Value",
        Description = "The email address or an attribute that contains the email address of the business. <span class='tip tip-lava'></span>",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.Email,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.EmailFieldType" } )]

    [WorkflowTextOrAttribute(
        "Phone Number",
        "Attribute Value",
        Description = "The phone number or an attribute that contains the phone number of the business. <span class='tip tip-lava'></span>",
        IsRequired = true,
        Order = 2,
        Key = AttributeKey.PhoneNumber,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.PhoneNumberFieldType" } )]

    [WorkflowAttribute(
        "SMS Enabled",
        Description = "A boolean attribute to determine if the provided phone number (required) has SMS enabled. If not provided this will be set to true.",
        IsRequired = false,
        Order = 3,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.BooleanFieldType" },
        Key = AttributeKey.SMSEnabled )]

    [WorkflowAttribute(
        "Address",
        "Attribute Value",
        Description = "",
        IsRequired = false,
        Order = 4,
        Key = AttributeKey.Address,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.AddressFieldType" } )]

    [WorkflowAttribute(
        "Campus",
        "Attribute Value",
        Description = "",
        IsRequired = false,
        Order = 5,
        Key = AttributeKey.Campus,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.CampusFieldType" } )]

    [WorkflowAttribute(
        "Contact",
        "Attribute Value",
        Description = "A optional person to connect to the business.",
        IsRequired = false,
        Order = 5,
        Key = AttributeKey.Contact,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.PersonFieldType" } )]

    [WorkflowAttribute(
        "Business",
        "Attribute Value",
        Description = "The resulting business that was either matched or created. This will return as a person attribute since businesses are people in the database.",
        IsRequired = false,
        Order = 6,
        Key = AttributeKey.Business,
        FieldTypeClassNames = new string[] { "Rock.Field.Types.TextFieldType", "Rock.Field.Types.PersonFieldType" } )]

    #endregion Attributes
    [Rock.SystemGuid.EntityTypeGuid( "BE13854E-BBD9-4334-85D6-0E3C1D01F575")]
    public class BusinessFromAttributes : ActionComponent
    {
        /// <summary>
        /// Keys for the attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string BusinessName = "BusinessName";
            public const string Email = "Email";
            public const string PhoneNumber = "PhoneNumber";
            public const string SMSEnabled = "SMSEnabled";
            public const string Address = "Address";
            public const string Campus = "Campus";
            public const string Contact = "Contact";
            public const string Business = "Business";
        }

        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Execute( RockContext rockContext, WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var mergeFields = GetMergeFields( action );
            string businessName = GetAttributeValue( action, AttributeKey.BusinessName, true ).ResolveMergeFields( mergeFields );
            if ( businessName.IsNullOrWhiteSpace() )
            {
                errorMessages.Add( "Business Name is required and could not be found." );
                errorMessages.ForEach( m => action.AddLogEntry( m, true ) );
                return false;
            }

            var email = GetAttributeValue( action, AttributeKey.Email, true ).ResolveMergeFields( mergeFields );
            var phoneNumber = GetAttributeValue( action, AttributeKey.PhoneNumber, true ).ResolveMergeFields( mergeFields ) ?? string.Empty;
            var address = GetLocationFromSelectedAttribute( AttributeKey.Address, rockContext, action );

            if ( email.IsNullOrWhiteSpace() && phoneNumber.IsNullOrWhiteSpace() && address == null )
            {
                errorMessages.Add( "Either Email, Phone Number Or Address must be present." );
                errorMessages.ForEach( m => action.AddLogEntry( m, true ) );
                return false;
            }

            var personService = new PersonService( rockContext );
            var business = personService.FindBusiness( businessName, email, phoneNumber, address?.Street1 );
            if ( business == null )
            {
                business = new Person();
                // Record Type - this is always "business". it will never change.
                business.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_BUSINESS.AsGuid() ).Id;
                personService.Add( business );
            }

            // Business Name
            business.LastName = businessName;
            business.Email = email;

            if ( !string.IsNullOrWhiteSpace( PhoneNumber.CleanNumber( phoneNumber ) ) )
            {
                // Phone Number
                var businessPhoneTypeId = new DefinedValueService( rockContext ).GetByGuid( new Guid( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK ) ).Id;
                var businessPhoneNumber = business.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == businessPhoneTypeId );
                if ( businessPhoneNumber == null )
                {
                    businessPhoneNumber = new PhoneNumber { NumberTypeValueId = businessPhoneTypeId };
                    business.PhoneNumbers.Add( businessPhoneNumber );
                }

                var smsEnabled = GetBooleanFromSelectedAttribute( AttributeKey.SMSEnabled, action );

                // TODO handle country code here
                businessPhoneNumber.Number = PhoneNumber.CleanNumber( phoneNumber );
                if ( smsEnabled.HasValue )
                {
                    businessPhoneNumber.IsMessagingEnabled = smsEnabled.Value;
                }
            }

            // Record Status
            business.RecordStatusValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE.AsGuid() ).Id;
            if ( !business.IsValid )
            {
                errorMessages.Add( string.Format( "Error creating the business {0}", businessName ) );
                errorMessages.ForEach( m => action.AddLogEntry( m, true ) );
                return false;
            }

            rockContext.WrapTransaction( () =>
            {
                rockContext.SaveChanges();

                var defaultCampusGuid = GetAttributeValue( action, AttributeKey.Campus, true ).AsGuidOrNull();
                var defaultCampus = defaultCampusGuid.HasValue ? CampusCache.Get( defaultCampusGuid.Value ) : null;
                var defaultCampusId = defaultCampus?.Id;

                // Add/Update Family Group
                var familyGroupType = GroupTypeCache.GetFamilyGroupType();
                int adultRoleId = familyGroupType.Roles
                    .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) )
                    .Select( r => r.Id )
                    .FirstOrDefault();
                var adultFamilyMember = UpdateGroupMember( business.Id, familyGroupType, business.LastName + " Business", defaultCampusId, adultRoleId, rockContext );
                business.GivingGroup = adultFamilyMember.Group;

                // Add/Update Known Relationship Group Type
                var knownRelationshipGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS.AsGuid() );
                int knownRelationshipOwnerRoleId = knownRelationshipGroupType.Roles
                    .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid() ) )
                    .Select( r => r.Id )
                    .FirstOrDefault();
                var knownRelationshipOwner = UpdateGroupMember( business.Id, knownRelationshipGroupType, "Known Relationship", null, knownRelationshipOwnerRoleId, rockContext );

                rockContext.SaveChanges();

                // Location
                int workLocationTypeId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_WORK ).Id;

                var groupLocationService = new GroupLocationService( rockContext );
                var workLocation = groupLocationService.Queryable( "Location" )
                    .Where( gl =>
                        gl.GroupId == adultFamilyMember.Group.Id &&
                        gl.GroupLocationTypeValueId == workLocationTypeId )
                    .FirstOrDefault();

                if ( address != null && address.Street1.IsNotNullOrWhiteSpace() )
                {
                    if ( workLocation == null )
                    {
                        workLocation = new GroupLocation();
                        groupLocationService.Add( workLocation );
                        workLocation.GroupId = adultFamilyMember.Group.Id;
                        workLocation.GroupLocationTypeValueId = workLocationTypeId;
                    }
                    else
                    {
                        // Save this to history if the box is checked and the new info is different than the current one.
                        if ( address.Id != workLocation.Location.Id )
                        {
                            new GroupLocationHistoricalService( rockContext ).Add( GroupLocationHistorical.CreateCurrentRowFromGroupLocation( workLocation, RockDateTime.Now ) );
                        }
                    }

                    workLocation.Location = address;
                    workLocation.IsMailingLocation = true;
                }

                var contactPerson = GetPersonFromSelectedAttribute( AttributeKey.Contact, rockContext, action );
                if ( contactPerson != null )
                {
                    personService.AddContactToBusiness( business.Id, contactPerson.Id );
                }

                rockContext.SaveChanges();
            } );

            var businessWorkflowAttributeGuid = GetAttributeValue( action, AttributeKey.Business ).AsGuidOrNull();
            if ( businessWorkflowAttributeGuid.HasValue )
            {
                var attribute = AttributeCache.Get( businessWorkflowAttributeGuid.Value, rockContext );
                if ( attribute != null && business != null )
                {
                    SetWorkflowAttributeValue( action, attribute.Guid, business.PrimaryAlias.Guid.ToString() );
                    action.AddLogEntry( string.Format( "Set '{0}' attribute to '{1}'.", attribute.Name, business.FullName ) );
                    return true;
                }
            }

            return true;
        }

        /// <summary>
        /// Updates the group member.
        /// </summary>
        /// <param name="businessId">The business identifier.</param>
        /// <param name="groupType">Type of the group.</param>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="groupRoleId">The group role identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private GroupMember UpdateGroupMember( int businessId, GroupTypeCache groupType, string groupName, int? campusId, int groupRoleId, RockContext rockContext )
        {
            var groupMemberService = new GroupMemberService( rockContext );

            GroupMember groupMember = groupMemberService.Queryable( "Group" )
                .Where( m =>
                    m.PersonId == businessId &&
                    m.GroupRoleId == groupRoleId )
                .FirstOrDefault();

            if ( groupMember == null )
            {
                groupMember = new GroupMember();
                groupMember.Group = new Group();
            }

            groupMember.PersonId = businessId;
            groupMember.GroupRoleId = groupRoleId;
            groupMember.GroupMemberStatus = GroupMemberStatus.Active;

            groupMember.Group.GroupTypeId = groupType.Id;
            groupMember.Group.Name = groupName;
            groupMember.Group.CampusId = campusId;

            if ( groupMember.Id == 0 )
            {
                groupMemberService.Add( groupMember );

                /*
                     6/20/2022 - SMC

                     We need to save the new Group to the database so that an Id is assigned.  This
                     Id is necessary to calculate the correct GivingId for the business, otherwise
                     all new businesses are given a GivingId of "G0" until the Rock Cleanup Job runs,
                     which causes the transactions to appear on any new records (because they all
                     have the same GivingId).

                     Reason: Transactions showing up on records they don't belong to.
                */
                rockContext.SaveChanges();
            }

            return groupMember;
        }

        /// <summary>
        /// Get a bool value from a workflow attribute
        /// </summary>
        /// <param name="attributeKey"></param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        private bool? GetBooleanFromSelectedAttribute( string attributeKey, WorkflowAction action )
        {
            return GetAttributeValue( action, attributeKey, true ).AsBooleanOrNull();
        }

        /// <summary>
        /// Get the person value from a workflow attribute
        /// </summary>
        /// <param name="attributeKey"></param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        private Person GetPersonFromSelectedAttribute( string attributeKey, RockContext rockContext, WorkflowAction action )
        {
            var personAliasGuid = GetAttributeValue( action, attributeKey, true ).AsGuidOrNull();
            Person person = null;
            if ( personAliasGuid.HasValue )
            {
                person = new PersonAliasService( rockContext ).GetPerson( personAliasGuid.Value );
            }

            return person;
        }

        /// <summary>
        /// Get a location value from a workflow attribute
        /// </summary>
        /// <param name="attributeKey"></param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        private Location GetLocationFromSelectedAttribute( string attributeKey, RockContext rockContext, WorkflowAction action )
        {
            var locationGuid = GetAttributeValue( action, attributeKey, true ).AsGuidOrNull();
            Location location = null;
            if ( locationGuid.HasValue )
            {
                location = new LocationService( rockContext ).Get( locationGuid.Value );
            }

            return location;
        }

    }
}