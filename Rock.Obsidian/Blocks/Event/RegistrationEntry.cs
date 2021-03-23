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
using Rock.Blocks;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.Util;
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

    #region Block Attributes

    [DefinedValueField( "Connection Status",
        Key = AttributeKey.ConnectionStatus,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        Description = "The connection status to use for new individuals (default: 'Web Prospect'.)",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_WEB_PROSPECT,
        Order = 0 )]

    [DefinedValueField( "Record Status",
        Key = AttributeKey.RecordStatus,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_RECORD_STATUS,
        Description = "The record status to use for new individuals (default: 'Pending'.)",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING,
        Order = 1 )]

    [DefinedValueField( "Source",
        Key = AttributeKey.Source,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE,
        Description = "The Financial Source Type to use when creating transactions",
        IsRequired = false,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_WEBSITE,
        Order = 2 )]

    [TextField( "Batch Name Prefix",
        Key = AttributeKey.BatchNamePrefix,
        Description = "The batch prefix name to use when creating a new batch",
        IsRequired = false,
        DefaultValue = "Event Registration",
        Order = 3 )]

    [BooleanField( "Display Progress Bar",
        Key = AttributeKey.DisplayProgressBar,
        Description = "Display a progress bar for the registration.",
        DefaultBooleanValue = true,
        Order = 4 )]

    [BooleanField( "Allow InLine Digital Signature Documents",
        Key = AttributeKey.SignInline,
        Description = "Should inline digital documents be allowed? This requires that the registration template is configured to display the document inline",
        DefaultBooleanValue = true,
        Order = 6 )]

    [SystemCommunicationField( "Confirm Account Template",
        Description = "Confirm Account Email Template",
        DefaultSystemCommunicationGuid = Rock.SystemGuid.SystemCommunication.SECURITY_CONFIRM_ACCOUNT,
        Order = 7,
        Key = AttributeKey.ConfirmAccountTemplate )]

    [TextField( "Family Term",
        Description = "The term to use for specifying which household or family a person is a member of.",
        IsRequired = true,
        DefaultValue = "immediate family",
        Order = 8,
        Key = AttributeKey.FamilyTerm )]

    [BooleanField( "Force Email Update",
        Description = "Force the email to be updated on the person's record.",
        DefaultBooleanValue = false,
        Order = 9,
        Key = AttributeKey.ForceEmailUpdate )]

    [BooleanField( "Show Field Descriptions",
        Description = "Show the field description as help text",
        DefaultBooleanValue = true,
        Order = 10,
        Key = AttributeKey.ShowFieldDescriptions )]

    [BooleanField( "Enabled Saved Account",
        Key = AttributeKey.EnableSavedAccount,
        Description = "Set this to false to disable the using Saved Account as a payment option, and to also disable the option to create saved account for future use.",
        DefaultBooleanValue = true,
        Order = 11 )]

    #endregion Block Attributes

    public class RegistrationEntry : ObsidianBlockType
    {
        #region Keys

        /// <summary>
        /// Attribute Key
        /// </summary>
        private static class AttributeKey {
            public const string ConnectionStatus = "ConnectionStatus";
            public const string RecordStatus = "RecordStatus";
            public const string Source = "Source";
            public const string BatchNamePrefix = "BatchNamePrefix";
            public const string DisplayProgressBar = "DisplayProgressBar";
            public const string SignInline = "SignInline";
            public const string ConfirmAccountTemplate = "ConfirmAccountTemplate";
            public const string FamilyTerm = "FamilyTerm";
            public const string ForceEmailUpdate = "ForceEmailUpdate";
            public const string ShowFieldDescriptions = "ShowFieldDescriptions";
            public const string EnableSavedAccount = "EnableSavedAccount";
        }

        /// <summary>
        /// Page Parameter
        /// </summary>
        private static class PageParameterKey
        {
            public const string RegistrationInstanceId = "RegistrationInstanceId";
        }

        #endregion Keys

        #region Obsidian Block Type Overrides

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
                var registrationInstance = GetRegistrationInstanceQuery( rockContext, "RegistrationTemplate.Forms.Fields, RegistrationTemplate.Fees.FeeItems" )
                    .AsNoTracking()
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

                // Force the registrar to update their email?
                var forceEmailUpdate = GetAttributeValue( AttributeKey.ForceEmailUpdate ).AsBoolean();

                // Load the gateway control settings
                var financialGatewayId = registrationTemplate?.FinancialGatewayId ?? 0;
                var financialGateway = new FinancialGatewayService( rockContext ).GetNoTracking( financialGatewayId );
                var financialGatewayComponent = financialGateway?.GetGatewayComponent() as IObsidianFinancialGateway;

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
                    DoAskForFamily = registrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask,
                    ForceEmailUpdate = forceEmailUpdate,
                    RegistrarOption = ( int ) registrationTemplate.RegistrarOption,
                    Cost = registrationTemplate.SetCostOnInstance == true ?
                        ( registrationInstance.Cost ?? registrationTemplate.Cost ) :
                        registrationTemplate.Cost,
                    GatewayControl = new GatewayControlViewModel {
                        FileUrl = financialGatewayComponent?.GetObsidianControlFileUrl( financialGateway ) ?? string.Empty,
                        Settings = financialGatewayComponent?.GetObsidianControlSettings( financialGateway ) ?? new object()
                    }
                };
            }
        }

        #endregion Obsidian Block Type Overrides

        #region Block Actions

        /// <summary>
        /// Checks the discount code.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult CheckDiscountCode( string code )
        {
            // Return a common "not found" result if any condition means the code is not valid
            Func<BlockActionResult> getNotFoundResult = () => new BlockActionResult( System.Net.HttpStatusCode.NotFound );
            var today = RockDateTime.Today;

            if ( code.IsNullOrWhiteSpace() )
            {
                return getNotFoundResult();
            }

            using ( var rockContext = new RockContext() )
            {
                var registrationInstance = GetRegistrationInstanceQuery( rockContext, string.Empty )
                    .Include( "RegistrationTemplate" )
                    .AsNoTracking()
                    .FirstOrDefault();

                var registrationTemplate = registrationInstance?.RegistrationTemplate;

                if ( registrationTemplate == null )
                {
                    return getNotFoundResult();
                }

                var discount = registrationTemplate.Discounts
                    .Where( d => d.Code.Equals( code, StringComparison.OrdinalIgnoreCase ) )
                    .FirstOrDefault();

                if ( discount == null )
                {
                    // The code is not found
                    return getNotFoundResult();
                }

                if ( discount.StartDate.HasValue && today < discount.StartDate.Value )
                {
                    // Before the discount starts
                    return getNotFoundResult();
                }

                if ( discount.EndDate.HasValue && today > discount.EndDate.Value )
                {
                    // Discount has expired
                    return getNotFoundResult();
                }

                int? usagesRemaining = null;

                if ( discount.MaxUsage.HasValue )
                {
                    // Check the number of people that have already used this code
                    var usageCount = new RegistrationService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Count( r =>
                            r.RegistrationInstanceId == registrationInstance.Id &&
                            r.DiscountCode.Equals( code, StringComparison.OrdinalIgnoreCase ) );

                    usagesRemaining = discount.MaxUsage.Value - usageCount;

                    if ( usagesRemaining <= 0 )
                    {
                        // Discount has been used up
                        return getNotFoundResult();
                    }
                }

                return new BlockActionResult( System.Net.HttpStatusCode.OK, new {
                    DiscountCode = discount.Code,
                    UsagesRemaining = usagesRemaining,
                    discount.DiscountAmount,
                    discount.DiscountPercentage
                } );
            }
        }

        #endregion Block Actions

        #region Helpers

        /// <summary>
        /// Gets the registration instance query.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="includes">The includes.</param>
        /// <returns></returns>
        private IQueryable<RegistrationInstance> GetRegistrationInstanceQuery( RockContext rockContext, string includes )
        {
            var registrationInstanceId = PageParameter( PageParameterKey.RegistrationInstanceId ).AsInteger();
            var now = RockDateTime.Now;

            var query = new RegistrationInstanceService( rockContext )
                .Queryable( includes )
                .Where( r =>
                    r.Id == registrationInstanceId &&
                    r.IsActive &&
                    r.RegistrationTemplate != null &&
                    r.RegistrationTemplate.IsActive &&
                    ( !r.StartDateTime.HasValue || r.StartDateTime <= now ) &&
                    ( !r.EndDateTime.HasValue || r.EndDateTime > now ) );

            return query;
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

        #endregion Helpers
    }
}
