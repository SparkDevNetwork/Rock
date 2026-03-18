using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Attribute;
using Rock.Configuration;
using Rock.Data;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class AttendeeSkill
{
    #region Tool(s)

    [Description( "Updates the profile details of a member of the current person's family." )]
    [AgentPurpose( "Updates the profile details of a member of the current person's family." )]
    [AgentUsage( "Any argument ending with 'ValueIdKey' must be a valid IdKey or the literal 'lookup' to retrieve allowed values. After lookup, call again with the chosen IdKey." )]
    [AgentToolGuid( "10dc67a3-cf4f-4581-a80c-1b999e9767f2" )]
    public IAgentToolResult UpdateFamilyMemberProfile(
        string personIdKey = null,

        string nickName = null,
        string firstName = null,
        string middleName = null,
        string lastName = null,

        SetOrClear<string> email = null,

        SetOrClear<int> birthYear = null,
        SetOrClear<int> birthDay = null,
        SetOrClear<int> birthMonth = null,

        SetOrClear<string> phoneNumber = null,
        string phoneTypeValueIdKey = null,
        [Description( "When true and the phone type specifies the Mobile phone number type, the phone number will be enabled for SMS messaging." )]
        bool? isMessagingEnabled = false,

        SetOrClear<string> street1 = null,
        string street2 = null,
        string city = null,
        string state = null,
        string postalCode = null,
        string country = null,
        string county = null,
        bool? isPhysicalAddress = null,
        bool? isMailingAddress = null,

        string campusIdKey = null,

        Gender? gender = null,
        SetOrClear<string> raceValueIdKey = null,
        SetOrClear<string> ethnicityValueIdKey = null,

        List<AttributeValueResult> attributeValues = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();
        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        if ( AgentRequestContext.CurrentPerson == null )
        {
            return Error( "A user must be logged in to list their profile." );
        }

        var editablePersonAttributeGuids = ConfigurationValues.GetReadOnlyValueOrDefault( ConfigurationKey.EditablePersonAttributes, string.Empty ).SplitDelimitedValues().AsGuidList();

        var person = helper.GetRequiredEntity<Model.Person>( personIdKey );

        if ( AgentRequestContext.CurrentPerson.PrimaryFamily.Members.FirstOrDefault( m => m.PersonId == person.Id ) == null )
        {
            return Error( "The specified person is not a member of the current user's family." );
        }

        UpdateName( helper, person, nickName, firstName, middleName, lastName );
        UpdateEmail( helper, person, email );
        UpdateBirthdate( helper, person, birthYear, birthDay, birthMonth );
        UpdatePhoneNumber( helper, rockContext, person, phoneNumber, phoneTypeValueIdKey, isMessagingEnabled );
        UpdateAddress( helper, rockContext, person, street1, street2, city, state, postalCode, country, isPhysicalAddress, isMailingAddress );
        UpdateCampus( helper, person, campusIdKey );
        UpdateDemographics( helper, person, gender, raceValueIdKey, ethnicityValueIdKey );
        UpdateAttributes( helper, rockContext, person, attributeValues );

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        return Success();
    }

    #endregion

    private void UpdateName( AgentToolHelper helper, Model.Person person, string nickName, string firstName, string middleName, string lastName )
    {
        var canEditName = !ConfigurationValues.GetReadOnlyValueOrDefault( ConfigurationKey.DisableNameEdit, string.Empty ).AsBoolean();

        if ( !canEditName )
        {
            if ( new[] { nickName, firstName, middleName, lastName }.Any( n => n.IsNotNullOrWhiteSpace() ) )
            {
                helper.AddError( "Updating name is not supported." );
            }

            return;
        }

        helper.UpdateProperty( person, p => p.NickName, nickName );
        helper.UpdateProperty( person, p => p.FirstName, firstName );
        helper.UpdateProperty( person, p => p.MiddleName, middleName );
        helper.UpdateProperty( person, p => p.LastName, lastName );
    }

    private void UpdateEmail( AgentToolHelper helper, Model.Person person, SetOrClear<string> email )
    {
        var canEditEmail = !ConfigurationValues.GetReadOnlyValueOrDefault( ConfigurationKey.DisableEmailEdit, string.Empty ).AsBoolean();

        if ( !canEditEmail )
        {
            if ( email != null )
            {
                helper.AddError( "Updating email is not supported." );
            }

            return;
        }

        helper.UpdateProperty( person, p => p.Email, email );

        if ( email != null && email.Value.IsNotNullOrWhiteSpace() )
        {
            person.IsEmailActive = true;
        }
    }

    private void UpdateBirthdate( AgentToolHelper helper, Model.Person person, SetOrClear<int> birthYear, SetOrClear<int> birthDay, SetOrClear<int> birthMonth )
    {
        var canEditBirthdate = !ConfigurationValues.GetReadOnlyValueOrDefault( ConfigurationKey.DisableBirthdateEdit, string.Empty ).AsBoolean();

        if ( !canEditBirthdate )
        {
            if ( birthYear != null || birthDay != null || birthMonth != null )
            {
                helper.AddError( "Updating birthdate is not supported." );
            }

            return;
        }

        helper.UpdateProperty( person, p => p.BirthYear, birthYear );
        helper.UpdateProperty( person, p => p.BirthDay, birthDay );
        helper.UpdateProperty( person, p => p.BirthMonth, birthMonth );
    }

    private void UpdatePhoneNumber( AgentToolHelper helper, RockContext rockContext, Model.Person person, SetOrClear<string> phoneNumber, string phoneTypeValueIdKey, bool? isMessagingEnabled )
    {
        var canEditPhone = !ConfigurationValues.GetReadOnlyValueOrDefault( ConfigurationKey.DisablePhoneNumberEdit, string.Empty ).AsBoolean();
        var phoneNumberTypeGuids = ConfigurationValues.GetReadOnlyValueOrDefault( ConfigurationKey.PhoneNumberTypes, string.Empty ).SplitDelimitedValues().AsGuidList();

        if ( !canEditPhone )
        {
            if ( phoneNumber != null )
            {
                helper.AddError( "Updating phone number is not supported." );
            }

            return;
        }

        if ( phoneNumber == null )
        {
            return;
        }

        var phoneTypeValueId = IdHasher.Instance.GetId( phoneTypeValueIdKey );
        var phoneTypeValue = DefinedValueCache.Get( phoneTypeValueId ?? 0 );
        var phoneTypeGuids = ConfigurationValues.GetReadOnlyValueOrDefault( ConfigurationKey.PhoneNumberTypes, string.Empty )
            .SplitDelimitedValues()
            .AsGuidList();

        // Check for valid phone type
        if ( !phoneTypeValueId.HasValue || phoneTypeValue == null || !phoneTypeGuids.Contains( phoneTypeValue.Guid ) )
        {
            var phoneTypes = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_PHONE_TYPE ).DefinedValues
                .Select( dv => new KeyNameResult { Id = dv.Id, Name = dv.Value } )
                .ToList();

            helper.AddError( "Phone number type must be specified." );
            helper.AddMetadata( "phoneTypeValueIdKeyLookup", phoneTypes );

            return;
        }

        var personPhoneService = new PhoneNumberService( rockContext );
        var personPhone = personPhoneService.Queryable()
            .FirstOrDefault( ph => ph.PersonId == person.Id
                && ph.NumberTypeValueId == phoneTypeValueId );

        if ( personPhone == null )
        {
            if ( phoneNumber.ClearValue )
            {
                return;
            }

            personPhone.PersonId = person.Id;
            personPhone.NumberTypeValueId = phoneTypeValueId;
            personPhoneService.Add( personPhone );
        }

        if ( phoneNumber.ClearValue )
        {
            personPhoneService.Delete( personPhone );
        }
        else
        {
            personPhone.Number = phoneNumber.Value;

            if ( isMessagingEnabled.HasValue && phoneTypeValue.Guid == SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() )
            {
                personPhone.IsMessagingEnabled = isMessagingEnabled.Value;
            }
        }
    }

    private void UpdateAddress( AgentToolHelper helper, RockContext rockContext, Model.Person person, SetOrClear<string> street1, string street2, string city, string state, string postalCode, string country, bool? isPhysicalAddress, bool? isMailingAddress )
    {
        var canEditAddress = !ConfigurationValues.GetReadOnlyValueOrDefault( ConfigurationKey.DisableAddressEdit, string.Empty ).AsBoolean();
        var addressTypeGuid = ConfigurationValues.GetReadOnlyValueOrDefault( ConfigurationKey.AddressType, string.Empty ).AsGuidOrNull();

        if ( !canEditAddress )
        {
            if ( street1 != null )
            {
                helper.AddError( "Updating address is not supported." );
            }

            return;
        }

        if ( street1 == null )
        {
            return;
        }

        var locationTypeValue = DefinedValueCache.Get( addressTypeGuid ?? Guid.Empty, rockContext );

        if ( locationTypeValue == null )
        {
            helper.AddError( "Updating address is not supported." );

            return;
        }

        if ( street1.ClearValue )
        {
            PersonSkill.RemoveAddress( rockContext, person, locationTypeValue );
        }
        else
        {
            PersonSkill.AddOrUpdateAddress( helper,
                rockContext,
                person,
                locationTypeValue,
                street1.Value,
                street2,
                city,
                state,
                postalCode,
                country,
                isMappedLocation: isPhysicalAddress,
                isMailingLocation: isMailingAddress );
        }
    }

    private void UpdateCampus( AgentToolHelper helper, Model.Person person, string campusIdKey )
    {
        var canEditCampus = !ConfigurationValues.GetReadOnlyValueOrDefault( ConfigurationKey.DisableCampusEdit, string.Empty ).AsBoolean();

        if ( !canEditCampus )
        {
            if ( campusIdKey.IsNotNullOrWhiteSpace() )
            {
                helper.AddError( "Updating campus is not supported." );
            }

            return;
        }

        helper.UpdateNavigationProperty( person, p => p.PrimaryCampus, campusIdKey );
    }

    private void UpdateDemographics( AgentToolHelper helper, Model.Person person, Gender? gender, SetOrClear<string> raceValueIdKey, SetOrClear<string> ethnicityValueIdKey )
    {
        var canEditDemographics = !ConfigurationValues.GetReadOnlyValueOrDefault( ConfigurationKey.DisableDemographicsEdit, string.Empty ).AsBoolean();

        if ( !canEditDemographics )
        {
            if ( gender.HasValue )
            {
                helper.AddError( "Updating gender is not supported." );
            }

            if ( raceValueIdKey != null )
            {
                helper.AddError( "Updating race is not supported." );
            }

            if ( ethnicityValueIdKey != null )
            {
                helper.AddError( "Updating ethnicity is not supported." );
            }

            return;
        }

        helper.UpdateProperty( person, p => p.Gender, gender );
        helper.UpdateNavigationProperty( person, p => p.RaceValue, raceValueIdKey );
        helper.UpdateNavigationProperty( person, p => p.EthnicityValue, ethnicityValueIdKey );
    }

    private void UpdateAttributes( AgentToolHelper helper, RockContext rockContext, Model.Person person, List<AttributeValueResult> attributeValues )
    {
        if ( attributeValues == null || !attributeValues.Any() )
        {
            return;
        }

        var editablePersonAttributeGuids = ConfigurationValues.GetReadOnlyValueOrDefault( ConfigurationKey.EditablePersonAttributes, string.Empty ).SplitDelimitedValues().AsGuidList();
        var availableAttributes = AttributeCache.GetMany( editablePersonAttributeGuids, rockContext ).ToList();

        Helper.LoadAttributes( person, rockContext, availableAttributes );

        // Don't enforce security since the configuraiton explicitely listed
        // which attributes could be edited.
        helper.SetAttributeValues( person, attributeValues, enforceSecurity: false );
    }
}
