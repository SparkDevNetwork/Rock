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
using System.Data.Entity;
using System.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModel.Blocks;
using Rock.ViewModel.Controls;
using Rock.Web.Cache;

namespace Rock.Obsidian.Blocks.Event
{
    /// <summary>
    /// Registration Entry.
    /// </summary>
    /// <seealso cref="Rock.Blocks.ObsidianBlockType" />

    [DisplayName( "Registration Entry" )]
    [Category( "Obsidian > Event" )]
    [Description( "Block used to register for a registration instance." )]
    [IconCssClass( "fa fa-clipboard-list" )]

    public class RegistrationEntry : ObsidianBlockType
    {
        /// <summary>
        /// Page Parameter
        /// </summary>
        private static class PageParameterKey
        {
            public const string RegistrationInstanceId = "RegistrationInstanceId";
        }

        /// <summary>
        /// Gets the property values that will be sent to the browser.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetObsidianConfigurationValues()
        {
            var currentPerson = GetCurrentPerson();
            var registrationInstanceId = PageParameter( PageParameterKey.RegistrationInstanceId ).AsInteger();

            using ( var rockContext = new RockContext() )
            {
                var now = RockDateTime.Now;

                var registrationInstance = new RegistrationInstanceService( rockContext )
                    .Queryable( "RegistrationTemplate.Forms.Fields, RegistrationTemplate.Fees.FeeItems" )
                    .AsNoTracking()
                    .Where( r =>
                        r.Id == registrationInstanceId &&
                        r.IsActive &&
                        r.RegistrationTemplate != null &&
                        r.RegistrationTemplate.IsActive &&
                        ( !r.StartDateTime.HasValue || r.StartDateTime <= now ) &&
                        ( !r.EndDateTime.HasValue || r.EndDateTime > now ) )
                    .FirstOrDefault();

                var registrationTemplate = registrationInstance?.RegistrationTemplate;
                var formModels = registrationTemplate?.Forms?.OrderBy( f => f.Order ).ToList() ?? new List<RegistrationTemplateForm>();

                // Get family members
                var familyMembers = registrationTemplate.ShowCurrentFamilyMembers ?
                    currentPerson.GetFamilyMembers( true, rockContext )
                        .Select( gm => new
                        {
                            FamilyGuid = gm.Group.Guid,
                            Person = gm.Person
                        } )
                        .ToList()
                        .Select( gm => new RegistrationEntryBlockFamilyMemberViewModel
                        {
                            Guid = gm.Person.Guid,
                            FamilyGuid = gm.FamilyGuid,
                            FullName = gm.Person.FullName,
                            FieldValues = GetCurrentValueFieldValues( rockContext, gm.Person, formModels )
                        } )
                        .ToList() :
                        new List<RegistrationEntryBlockFamilyMemberViewModel>();

                // Get the instructions
                var instructions = registrationInstance?.RegistrationInstructions;

                if ( instructions.IsNullOrWhiteSpace() )
                {
                    instructions = registrationTemplate?.RegistrationInstructions ?? string.Empty;
                }

                // Get the fee term
                var feeTerm = registrationTemplate?.FeeTerm;

                if ( feeTerm.IsNullOrWhiteSpace() )
                {
                    feeTerm = "Fee";
                }

                feeTerm = feeTerm.ToLower();
                var pluralFeeTerm = feeTerm.Pluralize();

                // Get the registrant term
                var registrantTerm = registrationTemplate?.RegistrantTerm;

                if ( registrantTerm.IsNullOrWhiteSpace() )
                {
                    registrantTerm = "Person";
                }

                registrantTerm = registrantTerm.ToLower();
                var pluralRegistrantTerm = registrantTerm.Pluralize();

                // Get the fees
                var feeModels = registrationTemplate?.Fees?.OrderBy( f => f.Order ).ToList() ?? new List<RegistrationTemplateFee>();
                var fees = new List<RegistrationEntryBlockFeeViewModel>();

                foreach ( var feeModel in feeModels )
                {
                    var feeViewModel = new RegistrationEntryBlockFeeViewModel
                    {
                        Guid = feeModel.Guid,
                        Name = feeModel.Name,
                        AllowMultiple = feeModel.AllowMultiple,
                        IsRequired = feeModel.IsRequired,
                        Items = feeModel.FeeItems.Select( fi => new RegistrationEntryBlockFeeItemViewModel
                        {
                            Cost = fi.Cost,
                            Name = fi.Name,
                            Guid = fi.Guid,
                            CountRemaining = fi.GetUsageCountRemaining( registrationInstance, null )
                        } )
                    };

                    fees.Add( feeViewModel );
                }

                // Get forms with fields
                var formViewModels = new List<RegistrationEntryBlockFormViewModel>();

                foreach ( var formModel in formModels )
                {
                    var form = new RegistrationEntryBlockFormViewModel();
                    var fieldModels = formModel.Fields.Where( f => !f.IsInternal ).OrderBy( f => f.Order );
                    var fields = new List<RegistrationEntryBlockFormFieldViewModel>();

                    foreach ( var fieldModel in fieldModels )
                    {
                        var field = new RegistrationEntryBlockFormFieldViewModel();
                        var attribute = fieldModel.AttributeId.HasValue ? AttributeCache.Get( fieldModel.AttributeId.Value ) : null;

                        field.Guid = fieldModel.Guid;
                        field.Attribute = attribute?.ToViewModel();
                        field.FieldSource = ( int ) fieldModel.FieldSource;
                        field.PersonFieldType = ( int ) fieldModel.PersonFieldType;
                        field.IsRequired = fieldModel.IsRequired;
                        field.VisibilityRuleType = ( int ) fieldModel.FieldVisibilityRules.FilterExpressionType;
                        field.PreHtml = fieldModel.PreText;
                        field.PostHtml = fieldModel.PostText;

                        field.VisibilityRules = fieldModel.FieldVisibilityRules
                            .RuleList
                            .Where( vr => vr.ComparedToRegistrationTemplateFormFieldGuid.HasValue )
                            .Select( vr => new RegistrationEntryBlockVisibilityViewModel
                            {
                                ComparedToRegistrationTemplateFormFieldGuid = vr.ComparedToRegistrationTemplateFormFieldGuid.Value,
                                ComparedToValue = vr.ComparedToValue,
                                ComparisonType = ( int ) vr.ComparisonType
                            } );

                        fields.Add( field );
                    }

                    form.Fields = fields;
                    formViewModels.Add( form );
                }

                // Get the registration attributes term
                var registrationAttributeTitleStart = ( registrationTemplate?.RegistrationAttributeTitleStart ).IsNullOrWhiteSpace() ?
                    "Registration Information" :
                    registrationTemplate.RegistrationAttributeTitleStart;

                var registrationAttributeTitleEnd = ( registrationTemplate?.RegistrationAttributeTitleEnd ).IsNullOrWhiteSpace() ?
                    "Registration Information" :
                    registrationTemplate.RegistrationAttributeTitleEnd;

                // Get the registration attributes
                var registrationEntityTypeId = EntityTypeCache.Get<Registration>().Id;
                var registrationAttributes = AttributeCache.All()
                    .Where( a =>
                        a.EntityTypeId == registrationEntityTypeId &&
                        a.EntityTypeQualifierColumn.Equals( "RegistrationTemplateId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( registrationTemplate?.Id.ToStringSafe() ) &&
                        a.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                    .OrderBy( a => a.Order )
                    .ThenBy( a => a.Name );

                // only show the Registration Attributes Before Registrants that have a category of REGISTRATION_ATTRIBUTE_START_OF_REGISTRATION
                var beforeAttributes = registrationAttributes
                    .Where( a =>
                        a.Categories.Any( c => c.Guid == Rock.SystemGuid.Category.REGISTRATION_ATTRIBUTE_START_OF_REGISTRATION.AsGuid() ) )
                    .Select( a => a.ToViewModel( currentPerson, false ) )
                    .ToList();

                // only show the Registration Attributes After Registrants that have don't have a category or have a category of REGISTRATION_ATTRIBUTE_END_OF_REGISTRATION
                var afterAttributes = registrationAttributes
                    .Where( a =>
                        !a.Categories.Any() ||
                        a.Categories.Any( c => c.Guid == Rock.SystemGuid.Category.REGISTRATION_ATTRIBUTE_END_OF_REGISTRATION.AsGuid() ) )
                    .Select( a => a.ToViewModel( currentPerson, false ) )
                    .ToList();

                // Get the maximum number of registrants
                var maxRegistrants = ( registrationTemplate?.AllowMultipleRegistrants == true ) ?
                    ( registrationTemplate.MaxRegistrants ?? 1 ) :
                    1;

                return new RegistrationEntryBlockViewModel
                {
                    RegistrationAttributesStart = beforeAttributes,
                    RegistrationAttributesEnd = afterAttributes,
                    RegistrationAttributeTitleStart = registrationAttributeTitleStart,
                    RegistrationAttributeTitleEnd = registrationAttributeTitleEnd,
                    InstructionsHtml = instructions,
                    RegistrantTerm = registrantTerm,
                    PluralRegistrantTerm = pluralRegistrantTerm,
                    PluralFeeTerm = pluralFeeTerm,
                    RegistrantForms = formViewModels,
                    Fees = fees,
                    FamilyMembers = familyMembers,
                    MaxRegistrants = maxRegistrants,
                    DoAskForFamily = registrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask
                };
            }
        }

        /// <summary>
        /// Gets the field values.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="person">The person.</param>
        /// <param name="forms">The forms.</param>
        /// <returns></returns>
        private IDictionary<Guid, object> GetCurrentValueFieldValues( RockContext rockContext, Person person, IEnumerable<RegistrationTemplateForm> forms )
        {
            var fieldValues = new Dictionary<Guid, object>();

            foreach ( var form in forms )
            {
                var fields = form.Fields.Where( f =>
                    ( f.ShowCurrentValue && !f.IsInternal ) ||
                    f.PersonFieldType == RegistrationPersonFieldType.FirstName ||
                    f.PersonFieldType == RegistrationPersonFieldType.LastName
                );

                foreach ( var field in fields )
                {
                    var value = GetCurrentFieldValue( rockContext, person, field );

                    if ( value != null )
                    {
                        fieldValues[field.Guid] = value;
                    }
                }
            }

            return fieldValues;
        }

        /// <summary>
        /// Gets the current field value.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="person">The person.</param>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        private object GetCurrentFieldValue( RockContext rockContext, Person person, RegistrationTemplateFormField field )
        {
            switch ( field.FieldSource )
            {
                case RegistrationFieldSource.PersonField:
                    return GetPersonCurrentFieldValue( rockContext, person, field );
                case RegistrationFieldSource.PersonAttribute:
                    return GetPersonCurrentAttributeValue( rockContext, person, field );
            }

            return null;
        }

        /// <summary>
        /// Gets the current person field value.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="person">The person.</param>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        private object GetPersonCurrentFieldValue( RockContext rockContext, Person person, RegistrationTemplateFormField field )
        {
            switch ( field.PersonFieldType )
            {
                case RegistrationPersonFieldType.FirstName:
                    return person.NickName.IsNullOrWhiteSpace() ? person.FirstName : person.NickName;
                case RegistrationPersonFieldType.LastName:
                    return person.LastName;
                case RegistrationPersonFieldType.MiddleName:
                    return person.MiddleName;
                case RegistrationPersonFieldType.Campus:
                    var family = person.GetFamily( rockContext );
                    return family?.Guid;
                case RegistrationPersonFieldType.Birthdate:
                    return new BirthdayPickerViewModel
                    {
                        Year = person.BirthYear ?? 0,
                        Month = person.BirthMonth ?? 0,
                        Day = person.BirthDay ?? 0
                    };
                case RegistrationPersonFieldType.Address:
                    var location = person.GetHomeLocation( rockContext );

                    return new AddressControlViewModel
                    {
                        Street1 = location?.Street1 ?? string.Empty,
                        Street2 = location?.Street2 ?? string.Empty,
                        City = location?.City ?? string.Empty,
                        State = location?.State ?? string.Empty,
                        PostalCode = location?.PostalCode ?? string.Empty
                    };
            }

            return null;
        }

        /// <summary>
        /// Gets the current person attribute value.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="person">The person.</param>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        private object GetPersonCurrentAttributeValue( RockContext rockContext, Person person, RegistrationTemplateFormField field )
        {
            var attribute = AttributeCache.Get( field.AttributeId ?? 0 );

            if ( attribute is null )
            {
                return null;
            }

            person.LoadAttributes( rockContext );
            return person.GetAttributeValue( attribute.Key );
        }
    }
}
