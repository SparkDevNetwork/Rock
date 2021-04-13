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
using System.Threading.Tasks;
using Rock.Attribute;
using Rock.Blocks;
using Rock.Data;
using Rock.Financial;
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
        private static class AttributeKey
        {
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
            public const string CampusId = "CampusId";
            public const string Slug = "Slug";
            public const string GroupId = "GroupId";
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
            using ( var rockContext = new RockContext() )
            {
                return GetViewModel( rockContext );
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
                var discount = GetDiscountByCode( rockContext, code );

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
                    var registrationInstanceId = PageParameter( PageParameterKey.RegistrationInstanceId ).AsInteger();

                    // Check the number of people that have already used this code
                    var usageCount = new RegistrationService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Count( r =>
                            r.RegistrationInstanceId == registrationInstanceId &&
                            r.DiscountCode.Equals( code, StringComparison.OrdinalIgnoreCase ) );

                    usagesRemaining = discount.MaxUsage.Value - usageCount;

                    if ( usagesRemaining <= 0 )
                    {
                        // Discount has been used up
                        return getNotFoundResult();
                    }
                }

                return new BlockActionResult( System.Net.HttpStatusCode.OK, new
                {
                    DiscountCode = discount.Code,
                    UsagesRemaining = usagesRemaining,
                    discount.DiscountAmount,
                    discount.DiscountPercentage
                } );
            }
        }

        /// <summary>
        /// Submits the registration.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult SubmitRegistration( RegistrationEntryBlockArgs args )
        {
            var currentPerson = GetCurrentPerson();

            // Basic check on the args to see that they appear valid
            if ( args == null )
            {
                return new BlockActionResult( System.Net.HttpStatusCode.BadRequest, "The args cannot be null" );
            }

            if ( args.AmountToPayNow < 0 )
            {
                return new BlockActionResult( System.Net.HttpStatusCode.BadRequest, "The amount to pay now cannot be less than zero" );
            }

            if ( args.Registrants?.Any() != true )
            {
                return new BlockActionResult( System.Net.HttpStatusCode.BadRequest, "At least one registrant is required" );
            }

            if ( args.Registrar == null )
            {
                return new BlockActionResult( System.Net.HttpStatusCode.BadRequest, "A registrar is required" );
            }

            using ( var rockContext = new RockContext() )
            {
                // Load the instance and template
                var includes = "RegistrationTemplate.Forms.Fields, RegistrationTemplate.FinancialGateway, Account";
                var registrationInstance = GetRegistrationInstanceQuery( rockContext, includes ).FirstOrDefault();
                var registrationTemplate = registrationInstance?.RegistrationTemplate;

                if ( registrationTemplate == null )
                {
                    return new BlockActionResult( System.Net.HttpStatusCode.NotFound, "The registration template or instance was not found" );
                }

                // Validate that there are enough spots left for this registration
                var waitListEnabled = registrationTemplate.WaitListEnabled;
                var spotsRemaining = registrationInstance.MaxAttendees;

                if ( !waitListEnabled && spotsRemaining < args.Registrants.Count )
                {
                    return new BlockActionResult( System.Net.HttpStatusCode.BadRequest, "There are not enough spots left for this many registrants" );
                }

                // Look up and validate the discount by the code
                var discount = GetDiscountByCode( rockContext, args.DiscountCode );

                if ( !args.DiscountCode.IsNullOrWhiteSpace() && discount == null )
                {
                    return new BlockActionResult( System.Net.HttpStatusCode.BadRequest, "The discount code is not valid" );
                }

                // Get or create the registration
                var registrationChanges = new History.HistoryChangeList();
                var registrationService = new RegistrationService( rockContext );
                var registration = args.RegistrationGuid.HasValue ? registrationService.Get( args.RegistrationGuid.Value ) : null;
                Person registrar = null;

                if ( registration == null && args.RegistrationGuid.HasValue )
                {
                    return new BlockActionResult( System.Net.HttpStatusCode.NotFound, "The registration was not found" );
                }

                var isNewRegistration = registration == null;

                if ( isNewRegistration )
                {
                    // This is a new registration
                    registration = new Registration
                    {
                        RegistrationInstanceId = registrationInstance.Id,
                        PersonAliasId = currentPerson?.PrimaryAliasId
                    };

                    registrationService.Add( registration );
                    registrationChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Registration" );
                    registrar = currentPerson;
                }
                else if ( registration.PersonAliasId.HasValue && registration.PersonAliasId.Value != currentPerson?.PrimaryAliasId )
                {
                    // This existing registration does not belong to this person
                    return new BlockActionResult( System.Net.HttpStatusCode.Unauthorized, "Your existing registration was not found" );
                }
                else if ( registration.RegistrationInstanceId != registrationInstance.Id )
                {
                    // This existing registration is not for this instance
                    return new BlockActionResult( System.Net.HttpStatusCode.NotFound, "Your existing registration was not found" );
                }
                else
                {
                    // This is an existing registration
                    registrar = registration.PersonAlias.Person;
                }

                // Apply the registrar values to the registration record
                History.EvaluateChange( registrationChanges, "First Name", registration.FirstName, args.Registrar.NickName );
                registration.FirstName = args.Registrar.NickName;

                History.EvaluateChange( registrationChanges, "Last Name", registration.LastName, args.Registrar.LastName );
                registration.LastName = args.Registrar.LastName;

                History.EvaluateChange( registrationChanges, "Confirmation Email", registration.ConfirmationEmail, args.Registrar.Email );
                registration.ConfirmationEmail = args.Registrar.Email;

                History.EvaluateChange( registrationChanges, "Discount Code", registration.DiscountCode, args.DiscountCode );
                registration.DiscountCode = args.DiscountCode;

                var discountPercentage = discount?.DiscountPercentage ?? 0;
                History.EvaluateChange( registrationChanges, "Discount Percentage", registration.DiscountPercentage, discountPercentage );
                registration.DiscountPercentage = discountPercentage;

                var discountAmount = discount?.DiscountAmount ?? 0;
                History.EvaluateChange( registrationChanges, "Discount Amount", registration.DiscountAmount, discountAmount );
                registration.DiscountAmount = discountAmount;

                // If the registrar person record does not exist, find or create that record
                var personService = new PersonService( rockContext );

                if ( registrar == null )
                {
                    registrar = personService.FindPerson( registration.FirstName, registration.LastName, registration.ConfirmationEmail, true );

                    if ( registrar != null )
                    {
                        registration.PersonAliasId = registrar.PrimaryAliasId;
                    }
                    else
                    {
                        registrar = null;
                        registration.PersonAlias = null;
                        registration.PersonAliasId = null;
                    }
                }

                // Load some attribute values about family roles and statuses
                var dvcConnectionStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid() );
                var dvcRecordStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordStatus ).AsGuid() );
                var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
                var adultRoleId = familyGroupType.Roles
                    .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) )
                    .Select( r => r.Id )
                    .FirstOrDefault();
                var childRoleId = familyGroupType.Roles
                    .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ) )
                    .Select( r => r.Id )
                    .FirstOrDefault();

                // Make sure there's an actual person associated to registration
                var campusId = PageParameter( PageParameterKey.CampusId ).AsIntegerOrNull();

                // variables to keep track of the family that new people should be added to
                int? singleFamilyId = null;
                var multipleFamilyGroupIds = new Dictionary<Guid, int>();

                if ( !registration.PersonAliasId.HasValue )
                {
                    // If a match was not found, create a new person
                    var person = new Person
                    {
                        FirstName = registration.FirstName,
                        LastName = registration.LastName,
                        IsEmailActive = true,
                        Email = registration.ConfirmationEmail,
                        EmailPreference = EmailPreference.EmailAllowed,
                        RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id,
                        ConnectionStatusValueId = dvcConnectionStatus?.Id
                    };

                    if ( dvcConnectionStatus != null )
                    {
                        person.ConnectionStatusValueId = dvcConnectionStatus.Id;
                    }

                    if ( dvcRecordStatus != null )
                    {
                        person.RecordStatusValueId = dvcRecordStatus.Id;
                    }

                    registrar = SavePerson( rockContext, registrationTemplate, person, Guid.NewGuid(), campusId, null, adultRoleId, childRoleId, multipleFamilyGroupIds, ref singleFamilyId );
                    registration.PersonAliasId = registrar != null ? registrar.PrimaryAliasId : ( int? ) null;
                    History.EvaluateChange( registrationChanges, "Registrar", string.Empty, registrar.FullName );
                }

                // Update the registrar's email if applicable
                var forceEmailUpdate = GetAttributeValue( AttributeKey.ForceEmailUpdate ).AsBoolean();

                if ( forceEmailUpdate && registration.PersonAliasId.HasValue )
                {
                    var person = personService.Get( registration.PersonAliasId.Value );

                    if ( person != null )
                    {
                        person.Email = registration.ConfirmationEmail;
                        rockContext.SaveChanges();
                    }
                }

                // Determine the campus
                var registrarFamily = registrar.GetFamily( rockContext );
                campusId = campusId ?? registrarFamily.CampusId;

                // Set the family guid for any other registrants that were selected to be in the same family
                multipleFamilyGroupIds.AddOrIgnore( registrarFamily.Guid, registrarFamily.Id );

                if ( !singleFamilyId.HasValue )
                {
                    singleFamilyId = registrarFamily.Id;
                }

                // If the Registration Instance linkage specified a group, load it now
                var groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
                var registrationSlug = PageParameter( PageParameterKey.Slug );

                if ( !groupId.HasValue && !registrationSlug.IsNullOrWhiteSpace() )
                {
                    var dateTime = RockDateTime.Now;
                    var linkage = new EventItemOccurrenceGroupMapService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( l =>
                            l.UrlSlug == registrationSlug &&
                            l.RegistrationInstance != null &&
                            l.RegistrationInstance.IsActive &&
                            l.RegistrationInstance.RegistrationTemplate != null &&
                            l.RegistrationInstance.RegistrationTemplate.IsActive &&
                            ( !l.RegistrationInstance.StartDateTime.HasValue || l.RegistrationInstance.StartDateTime <= dateTime ) &&
                            ( !l.RegistrationInstance.EndDateTime.HasValue || l.RegistrationInstance.EndDateTime > dateTime ) )
                        .FirstOrDefault();

                    if ( linkage != null )
                    {
                        groupId = linkage.GroupId;
                    }
                }

                Group group = null;

                if ( groupId.HasValue )
                {
                    group = new GroupService( rockContext ).Get( groupId.Value );

                    if ( group != null && ( !registration.GroupId.HasValue || registration.GroupId.Value != group.Id ) )
                    {
                        registration.GroupId = group.Id;
                        History.EvaluateChange( registrationChanges, "Group", string.Empty, group.Name );
                    }
                }

                // if this registration was marked as temporary (started from another page, then specified in the url), set IsTemporary to False now that we are done
                registration.IsTemporary = false;

                // Set attribute values on the registration
                registration.LoadAttributes( rockContext );

                foreach ( var value in args.FieldValues )
                {
                    var attributeGuid = value.Key;
                    var attribute = AttributeCache.Get( attributeGuid );

                    if ( attribute == null )
                    {
                        continue;
                    }

                    registration.SetAttributeValue( attribute.Key, value.Value.ToStringSafe() );
                }

                // Save the registration ( so we can get an id )
                rockContext.SaveChanges();
                registration.SaveAttributeValues( rockContext );

                // Save the history
                Task.Run( () => HistoryService.SaveChanges(
                    new RockContext(),
                    typeof( Registration ),
                    Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                    registration.Id,
                    registrationChanges,
                    true,
                    currentPerson?.PrimaryAliasId ) );

                try
                {
                    // Get each registrant
                    var index = 0;

                    foreach ( var registrantInfo in args.Registrants )
                    {
                        var forceWaitlist = spotsRemaining < 1;

                        UpsertRegistrant(
                            rockContext,
                            registrationTemplate,
                            registrationInstance,
                            registration,
                            registrar,
                            registrarFamily.Guid,
                            registrantInfo,
                            discount,
                            index,
                            multipleFamilyGroupIds,
                            ref singleFamilyId,
                            forceWaitlist );

                        index++;
                    }

                    rockContext.SaveChanges();
                }
                catch ( Exception )
                {
                    using ( var newRockContext = new RockContext() )
                    {
                        // Cleanup any new records created since there was an error
                        if ( isNewRegistration )
                        {
                            var newRegistrationService = new RegistrationService( newRockContext );
                            var savedRegistration = new RegistrationService( newRockContext ).Get( registration.Id );

                            if ( savedRegistration != null )
                            {
                                HistoryService.DeleteChanges( newRockContext, typeof( Registration ), savedRegistration.Id );

                                newRegistrationService.Delete( savedRegistration );
                                newRockContext.SaveChanges();
                            }
                        }
                    }

                    throw;
                }

                if ( args.AmountToPayNow > registration.TotalCost )
                {
                    args.AmountToPayNow = registration.TotalCost;
                }

                var transactionGuid = args.AmountToPayNow > 0 ?
                    ProcessPayment( rockContext, registrationTemplate, registrationInstance, registration, args, registrar, out var errorMessage ) :
                    null;

                var successViewModel = GetSuccessViewModel( registration.Id );
                return new BlockActionResult( System.Net.HttpStatusCode.Created, successViewModel );
            }
        }

        #endregion Block Actions

        #region Helpers

        /// <summary>
        /// Gets a person field value.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="personFieldType">Type of the person field.</param>
        /// <returns></returns>
        private object GetPersonFieldValue( RegistrationTemplate template, RegistrationPersonFieldType personFieldType, Dictionary<Guid, object> fieldValues )
        {
            if ( template == null || template.Forms == null )
            {
                return null;
            }

            var fieldGuid = template.Forms
                .SelectMany( t => t.Fields
                    .Where( f =>
                        f.FieldSource == RegistrationFieldSource.PersonField &&
                        f.PersonFieldType == personFieldType )
                    .Select( f => f.Guid ) )
                .FirstOrDefault();

            return fieldValues.GetValueOrNull( fieldGuid );
        }

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

        /// <summary>
        /// Gets the discount.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        private RegistrationTemplateDiscount GetDiscountByCode( RockContext rockContext, string code )
        {
            if ( code.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var registrationInstanceQuery = GetRegistrationInstanceQuery( rockContext, "RegistrationTemplate" )
                    .AsNoTracking();

            var discount = registrationInstanceQuery
                .SelectMany( ri => ri.RegistrationTemplate.Discounts )
                .FirstOrDefault( d => d.Code.Equals( code, StringComparison.OrdinalIgnoreCase ) );

            return discount;
        }

        /// <summary>
        /// Saves the person.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="registrationTemplate">The registration template.</param>
        /// <param name="person">The person.</param>
        /// <param name="familyGuid">The family unique identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="location">The location.</param>
        /// <param name="adultRoleId">The adult role identifier.</param>
        /// <param name="childRoleId">The child role identifier.</param>
        /// <param name="multipleFamilyGroupIds">The multiple family group ids.</param>
        /// <param name="singleFamilyId">The single family identifier.</param>
        /// <returns></returns>
        private Person SavePerson( RockContext rockContext, RegistrationTemplate registrationTemplate, Person person, Guid familyGuid, int? campusId, Location location, int adultRoleId, int childRoleId, Dictionary<Guid, int> multipleFamilyGroupIds, ref int? singleFamilyId )
        {
            if ( !person.PrimaryCampusId.HasValue && campusId.HasValue )
            {
                person.PrimaryCampusId = campusId;
                rockContext.SaveChanges();
            }

            int? familyId = null;

            if ( person.Id > 0 )
            {
                rockContext.SaveChanges();

                // Set the family guid for any other registrants that were selected to be in the same family
                var family = person.GetFamily( rockContext );
                if ( family != null )
                {
                    familyId = family.Id;
                    multipleFamilyGroupIds.AddOrIgnore( familyGuid, family.Id );
                    if ( !singleFamilyId.HasValue )
                    {
                        singleFamilyId = family.Id;
                    }
                }
            }
            else
            {
                // If we've created the family already for this registrant, add them to it
                if (
                        ( registrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask && multipleFamilyGroupIds.ContainsKey( familyGuid ) ) ||
                        ( registrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Yes && singleFamilyId.HasValue )
                    )
                {
                    // Add person to existing family
                    var age = person.Age;
                    int familyRoleId = age.HasValue && age < 18 ? childRoleId : adultRoleId;

                    familyId = registrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask ?
                        multipleFamilyGroupIds[familyGuid] :
                        singleFamilyId.Value;
                    PersonService.AddPersonToFamily( person, true, familyId.Value, familyRoleId, rockContext );
                }
                else
                {
                    // otherwise create a new family
                    // Create Person/Family
                    var familyGroup = PersonService.SaveNewPerson( person, rockContext, campusId, false );
                    if ( familyGroup != null )
                    {
                        familyId = familyGroup.Id;

                        // Store the family id for next person
                        multipleFamilyGroupIds.AddOrIgnore( familyGuid, familyGroup.Id );
                        if ( !singleFamilyId.HasValue )
                        {
                            singleFamilyId = familyGroup.Id;
                        }
                    }
                }
            }

            // If we have family ID and a meaninful location then update that info
            if ( familyId.HasValue )
            {
                var familyGroup = new GroupService( rockContext ).Get( familyId.Value );

                if ( !familyGroup.CampusId.HasValue && campusId.HasValue )
                {
                    familyGroup.CampusId = campusId;
                    rockContext.SaveChanges();
                }

                if ( location != null && location.IsMinimumViableAddress() )
                {

                    var existingLocation = new LocationService( rockContext ).Get(
                        location.Street1,
                        location.Street2,
                        location.City,
                        location.State,
                        location.PostalCode,
                        location.Country,
                        familyGroup,
                        true,
                        false );

                    var homeLocationType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME.AsGuid() );
                    if ( homeLocationType != null && familyGroup != null )
                    {
                        if ( existingLocation != null )
                        {
                            // A location exists but is not associated with this family group
                            GroupService.AddNewGroupAddress( rockContext, familyGroup, Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME, existingLocation );
                        }
                        else
                        {
                            // Create a new location and save it to the family group
                            GroupService.AddNewGroupAddress(
                                rockContext,
                                familyGroup,
                                Rock.SystemGuid.DefinedValue.GROUP_LOCATION_TYPE_HOME,
                                location.Street1,
                                location.Street2,
                                location.City,
                                location.State,
                                location.PostalCode,
                                location.Country,
                                true );
                        }
                    }
                }
            }

            return new PersonService( rockContext ).Get( person.Id );
        }

        /// <summary>
        /// Saves the phone.
        /// </summary>
        /// <param name="fieldValue">The field value.</param>
        /// <param name="person">The person.</param>
        /// <param name="phoneTypeGuid">The phone type unique identifier.</param>
        /// <param name="changes">The changes.</param>
        private void SavePhone( object fieldValue, Person person, Guid phoneTypeGuid, History.HistoryChangeList changes )
        {
            var phoneNumber = fieldValue as PhoneNumber;
            if ( phoneNumber != null )
            {
                string cleanNumber = PhoneNumber.CleanNumber( phoneNumber.Number );
                if ( !string.IsNullOrWhiteSpace( cleanNumber ) )
                {
                    var numberType = DefinedValueCache.Get( phoneTypeGuid );
                    if ( numberType != null )
                    {
                        var phone = person.PhoneNumbers.FirstOrDefault( p => p.NumberTypeValueId == numberType.Id );
                        string oldPhoneNumber = string.Empty;
                        if ( phone == null )
                        {
                            phone = new PhoneNumber();
                            person.PhoneNumbers.Add( phone );
                            phone.NumberTypeValueId = numberType.Id;
                        }
                        else
                        {
                            oldPhoneNumber = phone.NumberFormattedWithCountryCode;
                        }

                        phone.CountryCode = PhoneNumber.CleanNumber( phoneNumber.CountryCode );
                        phone.Number = cleanNumber;

                        History.EvaluateChange(
                            changes,
                            string.Format( "{0} Phone", numberType.Value ),
                            oldPhoneNumber,
                            phoneNumber.NumberFormattedWithCountryCode );
                    }
                }
            }
        }

        /// <summary>
        /// Gets the registrant cost.
        /// </summary>
        /// <param name="registrationTemplate">The registration template.</param>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <param name="discount">The discount.</param>
        /// <returns></returns>
        private decimal GetBaseRegistrantCost( RegistrationTemplate registrationTemplate, RegistrationInstance registrationInstance )
        {
            var cost = registrationTemplate.SetCostOnInstance == true ?
                        ( registrationInstance.Cost ?? registrationTemplate.Cost ) :
                        registrationTemplate.Cost;

            return cost;
        }

        /// <summary>
        /// Upserts the registrant.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="registrationTemplate">The registration template.</param>
        /// <param name="registrationInstance">The registration instance.</param>
        /// <param name="registration">The registration.</param>
        /// <param name="registrar">The registrar.</param>
        /// <param name="registrarFamilyGuid">The registrar family unique identifier.</param>
        /// <param name="registrantInfo">The registrant information.</param>
        /// <param name="discount">The discount.</param>
        /// <param name="index">The index.</param>
        /// <param name="multipleFamilyGroupIds">The multiple family group ids.</param>
        /// <param name="singleFamilyId">The single family identifier.</param>
        /// <param name="isWaitlist">if set to <c>true</c> [is waitlist].</param>
        private void UpsertRegistrant(
            RockContext rockContext,
            RegistrationTemplate registrationTemplate,
            RegistrationInstance registrationInstance,
            Registration registration,
            Person registrar,
            Guid registrarFamilyGuid,
            ViewModel.Blocks.RegistrantInfo registrantInfo,
            RegistrationTemplateDiscount discount,
            int index,
            Dictionary<Guid, int> multipleFamilyGroupIds,
            ref int? singleFamilyId,
            bool isWaitlist )
        {
            // Force waitlist if specified by param, but allow waitlist if requested
            isWaitlist |= ( registrationTemplate.WaitListEnabled && registrantInfo.IsOnWaitList );

            var personService = new PersonService( rockContext );
            var registrantService = new RegistrationRegistrantService( rockContext );

            var registrantChanges = new History.HistoryChangeList();
            var personChanges = new History.HistoryChangeList();

            RegistrationRegistrant registrant = null;
            Person person = null;

            var firstName = GetPersonFieldValue( registrationTemplate, RegistrationPersonFieldType.FirstName, registrantInfo.FieldValues ).ToStringSafe();
            var lastName = GetPersonFieldValue( registrationTemplate, RegistrationPersonFieldType.LastName, registrantInfo.FieldValues ).ToStringSafe();
            var email = GetPersonFieldValue( registrationTemplate, RegistrationPersonFieldType.Email, registrantInfo.FieldValues ).ToStringSafe();
            var birthday = GetPersonFieldValue( registrationTemplate, RegistrationPersonFieldType.Birthdate, registrantInfo.FieldValues ).ToStringSafe().FromJsonOrNull<BirthdayPickerViewModel>().ToDateTime();
            var mobilePhone = GetPersonFieldValue( registrationTemplate, RegistrationPersonFieldType.MobilePhone, registrantInfo.FieldValues ).ToStringSafe();

            registrant = registration.Registrants.FirstOrDefault( r => r.Guid == registrantInfo.Guid );

            if ( registrant != null )
            {
                person = registrant.Person;
                if ( person != null && (
                    ( registrant.Person.FirstName.Equals( firstName, StringComparison.OrdinalIgnoreCase ) || registrant.Person.NickName.Equals( firstName, StringComparison.OrdinalIgnoreCase ) ) &&
                    registrant.Person.LastName.Equals( lastName, StringComparison.OrdinalIgnoreCase ) ) )
                {
                    // Do nothing
                }
                else
                {
                    person = null;
                    registrant.PersonAlias = null;
                    registrant.PersonAliasId = null;
                }
            }
            else if ( registrantInfo.PersonGuid.HasValue )
            {
                // This can happen if the page has reloaded due to an error. The person was saved to the DB and we don't want to add them again.
                person = personService.Get( registrantInfo.PersonGuid.Value );
            }
            else
            {
                if ( registrantInfo.PersonGuid.HasValue && registrationTemplate.ShowCurrentFamilyMembers )
                {
                    person = personService.Get( registrantInfo.PersonGuid.Value );
                }
            }

            if ( person == null )
            {
                // Try to find a matching person based on name, email address, mobile phone, and birthday. If these were not provided they are not considered.
                var personQuery = new PersonService.PersonMatchQuery( firstName, lastName, email, mobilePhone, gender: null, birthDate: birthday );
                person = personService.FindPerson( personQuery, true );

                // Try to find a matching person based on name within same family as registrar
                if ( person == null && registrar != null && registrantInfo.FamilyGuid == registrarFamilyGuid )
                {
                    var familyMembers = registrar.GetFamilyMembers( true, rockContext )
                        .Where( m => ( m.Person.FirstName == firstName || m.Person.NickName == firstName ) && m.Person.LastName == lastName )
                        .Select( m => m.Person )
                        .ToList();

                    if ( familyMembers.Count() == 1 )
                    {
                        person = familyMembers.First();
                        if ( !string.IsNullOrWhiteSpace( email ) )
                        {
                            person.Email = email;
                        }
                    }

                    if ( familyMembers.Count() > 1 && !string.IsNullOrWhiteSpace( email ) )
                    {
                        familyMembers = familyMembers
                            .Where( m =>
                                m.Email != null &&
                                m.Email.Equals( email, StringComparison.OrdinalIgnoreCase ) )
                            .ToList();
                        if ( familyMembers.Count() == 1 )
                        {
                            person = familyMembers.First();
                        }
                    }
                }
            }

            var dvcConnectionStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid() );
            var dvcRecordStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordStatus ).AsGuid() );

            var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
            var adultRoleId = familyGroupType.Roles
                .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) )
                .Select( r => r.Id )
                .FirstOrDefault();
            var childRoleId = familyGroupType.Roles
                .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ) )
                .Select( r => r.Id )
                .FirstOrDefault();

            if ( person == null )
            {
                // If a match was not found, create a new person
                person = new Person();
                person.FirstName = firstName;
                person.LastName = lastName;
                person.IsEmailActive = true;
                person.Email = email;
                person.EmailPreference = EmailPreference.EmailAllowed;
                person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;

                if ( dvcConnectionStatus != null )
                {
                    person.ConnectionStatusValueId = dvcConnectionStatus.Id;
                }

                if ( dvcRecordStatus != null )
                {
                    person.RecordStatusValueId = dvcRecordStatus.Id;
                }
            }

            Location location = null;
            var campusId = PageParameter( PageParameterKey.CampusId ).AsIntegerOrNull();

            // Set any of the template's person fields
            foreach ( var field in registrationTemplate.Forms
                .SelectMany( f => f.Fields
                    .Where( t => t.FieldSource == RegistrationFieldSource.PersonField ) ) )
            {
                // Find the registrant's value
                var fieldValue = GetPersonFieldValue( registrationTemplate, field.PersonFieldType, registrantInfo.FieldValues );

                if ( fieldValue != null )
                {
                    switch ( field.PersonFieldType )
                    {
                        case RegistrationPersonFieldType.Campus:
                            campusId = fieldValue.ToString().AsIntegerOrNull();
                            break;

                        case RegistrationPersonFieldType.MiddleName:
                            string middleName = fieldValue.ToString().Trim();
                            History.EvaluateChange( personChanges, "Middle Name", person.MiddleName, middleName );
                            person.MiddleName = middleName;
                            break;

                        case RegistrationPersonFieldType.Address:
                            location = fieldValue as Location;
                            break;

                        case RegistrationPersonFieldType.Birthdate:
                            var oldBirthMonth = person.BirthMonth;
                            var oldBirthDay = person.BirthDay;
                            var oldBirthYear = person.BirthYear;

                            person.SetBirthDate( fieldValue.ToStringSafe().FromJsonOrNull<BirthdayPickerViewModel>().ToDateTime() );

                            History.EvaluateChange( personChanges, "Birth Month", oldBirthMonth, person.BirthMonth );
                            History.EvaluateChange( personChanges, "Birth Day", oldBirthDay, person.BirthDay );
                            History.EvaluateChange( personChanges, "Birth Year", oldBirthYear, person.BirthYear );
                            break;

                        case RegistrationPersonFieldType.Grade:
                            var newGraduationYear = fieldValue.ToString().AsIntegerOrNull();
                            History.EvaluateChange( personChanges, "Graduation Year", person.GraduationYear, newGraduationYear );
                            person.GraduationYear = newGraduationYear;
                            break;

                        case RegistrationPersonFieldType.Gender:
                            var newGender = fieldValue.ToString().ConvertToEnumOrNull<Gender>() ?? Gender.Unknown;
                            History.EvaluateChange( personChanges, "Gender", person.Gender, newGender );
                            person.Gender = newGender;
                            break;

                        case RegistrationPersonFieldType.MaritalStatus:
                            if ( fieldValue != null )
                            {
                                int? newMaritalStatusId = fieldValue.ToString().AsIntegerOrNull();
                                History.EvaluateChange( personChanges, "Marital Status", DefinedValueCache.GetName( person.MaritalStatusValueId ), DefinedValueCache.GetName( newMaritalStatusId ) );
                                person.MaritalStatusValueId = newMaritalStatusId;
                            }

                            break;

                        case RegistrationPersonFieldType.AnniversaryDate:
                            var oldAnniversaryDate = person.AnniversaryDate;
                            person.AnniversaryDate = fieldValue.ToString().AsDateTime();
                            History.EvaluateChange( personChanges, "Anniversary Date", oldAnniversaryDate, person.AnniversaryDate );
                            break;

                        case RegistrationPersonFieldType.MobilePhone:
                            SavePhone( fieldValue, person, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid(), personChanges );
                            break;

                        case RegistrationPersonFieldType.HomePhone:
                            SavePhone( fieldValue, person, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid(), personChanges );
                            break;

                        case RegistrationPersonFieldType.WorkPhone:
                            SavePhone( fieldValue, person, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid(), personChanges );
                            break;

                        case RegistrationPersonFieldType.ConnectionStatus:
                            var newConnectionStatusId = fieldValue.ToString().AsIntegerOrNull() ?? dvcConnectionStatus.Id;
                            History.EvaluateChange( personChanges, "Connection Status", DefinedValueCache.GetName( person.ConnectionStatusValueId ), DefinedValueCache.GetName( newConnectionStatusId ) );
                            person.ConnectionStatusValueId = newConnectionStatusId;
                            break;
                    }
                }
            }

            // Save the person ( and family if needed )
            SavePerson( rockContext, registrationTemplate, person, registrantInfo.FamilyGuid ?? Guid.NewGuid(), campusId, location, adultRoleId, childRoleId, multipleFamilyGroupIds, ref singleFamilyId );

            // Load the person's attributes
            person.LoadAttributes();

            // Set any of the template's person fields
            foreach ( var field in registrationTemplate.Forms
                .SelectMany( f => f.Fields
                    .Where( t =>
                        t.FieldSource == RegistrationFieldSource.PersonAttribute &&
                        t.AttributeId.HasValue ) ) )
            {
                // Find the registrant's value
                var fieldValue = registrantInfo.FieldValues.GetValueOrNull( field.Guid );

                if ( fieldValue != null )
                {
                    var attribute = AttributeCache.Get( field.AttributeId.Value );
                    if ( attribute != null )
                    {
                        string originalValue = person.GetAttributeValue( attribute.Key );
                        string newValue = fieldValue.ToString();
                        person.SetAttributeValue( attribute.Key, fieldValue.ToString() );

                        // DateTime values must be stored in ISO8601 format as http://www.rockrms.com/Rock/Developer/BookContent/16/16#datetimeformatting
                        if ( attribute.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.DATE.AsGuid() ) ||
                            attribute.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.DATE_TIME.AsGuid() ) )
                        {
                            DateTime aDateTime;
                            if ( DateTime.TryParse( newValue, out aDateTime ) )
                            {
                                newValue = aDateTime.ToString( "o" );
                            }
                        }

                        if ( ( originalValue ?? string.Empty ).Trim() != ( newValue ?? string.Empty ).Trim() )
                        {
                            string formattedOriginalValue = string.Empty;
                            if ( !string.IsNullOrWhiteSpace( originalValue ) )
                            {
                                formattedOriginalValue = attribute.FieldType.Field.FormatValue( originalValue, attribute.QualifierValues, false );
                            }

                            string formattedNewValue = string.Empty;
                            if ( !string.IsNullOrWhiteSpace( newValue ) )
                            {
                                formattedNewValue = attribute.FieldType.Field.FormatValue( newValue, attribute.QualifierValues, false );
                            }

                            Helper.SaveAttributeValue( person, attribute, newValue, rockContext );
                            History.EvaluateChange( personChanges, attribute.Name, formattedOriginalValue, formattedNewValue );
                        }
                    }
                }
            }

            var registrantName = person.FullName + ": ";

            personChanges.ForEach( c => registrantChanges.Add( c ) );

            if ( registrant == null )
            {
                registrant = new RegistrationRegistrant();
                registrant.Guid = registrantInfo.Guid;
                registrantService.Add( registrant );
                registrant.RegistrationId = registration.Id;
            }

            registrant.OnWaitList = isWaitlist;
            registrant.PersonAliasId = person.PrimaryAliasId;
            registrant.Cost = isWaitlist ? 0 : GetBaseRegistrantCost( registrationTemplate, registrationInstance );

            if ( discount?.MaxRegistrants.HasValue == true )
            {
                registrant.DiscountApplies = index < discount.MaxRegistrants.Value;
            }
            else
            {
                registrant.DiscountApplies = discount != null;
            }

            var registrantFeeService = new RegistrationRegistrantFeeService( rockContext );
            var registrationTemplateFeeItemService = new RegistrationTemplateFeeItemService( rockContext );

            // Delete any existing fees that were removed
            foreach ( var dbFee in registrant.Fees.ToList() )
            {
                var feeItemGuid = dbFee.RegistrationTemplateFeeItem.Guid;
                var quantity = isWaitlist ? 0 : ( registrantInfo.FeeItemQuantities.GetValueOrNull( feeItemGuid ) ?? 0 );

                if ( quantity < 1 )
                {
                    var oldFeeValue = $"'{dbFee.RegistrationTemplateFee.Name}' Fee (Quantity:{dbFee.Quantity:N0}, Cost:{dbFee.Cost:C2}, Option:{dbFee.Option}";
                    registrantChanges.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Fee" ).SetOldValue( oldFeeValue );

                    registrant.Fees.Remove( dbFee );
                    registrantFeeService.Delete( dbFee );
                }
            }

            // Upsert fees if not on the waiting list
            if ( !isWaitlist )
            {
                var feeModels = registrationTemplate.Fees?.OrderBy( f => f.Order ).ToList() ?? new List<RegistrationTemplateFee>();

                foreach ( var feeModel in feeModels )
                {
                    var totalFeeQuantity = 0;
                    var feeItemModels = feeModel.FeeItems.ToList();

                    for ( var i = 0; i < feeItemModels.Count; i++ )
                    {
                        var feeItemModel = feeItemModels[i];
                        var isLastFeeItemModel = i == ( feeItemModels.Count - 1 );

                        var quantity = registrantInfo.FeeItemQuantities.GetValueOrNull( feeItemModel.Guid ) ?? 0;
                        var registrantFee = registrant.Fees
                                .FirstOrDefault( f =>
                                    f.RegistrationTemplateFeeId == feeModel.Id &&
                                    f.RegistrationTemplateFeeItemId == feeItemModel.Id );

                        // If this fee is required and this is the last item, then make sure at least 1 is selected
                        if ( isLastFeeItemModel && totalFeeQuantity < 1 && quantity < 1 )
                        {
                            quantity = 1;
                        }

                        // If there is a limited supply, ensure that more are not ordered than available
                        var countRemaining = feeItemModel.GetUsageCountRemaining( registrationInstance, null );

                        // Don't allow quantity to be more than supply
                        if ( countRemaining.HasValue && countRemaining < quantity )
                        {
                            quantity = countRemaining.Value;
                        }

                        // Don't allow selecting more than 1 if not allowed
                        if ( !feeModel.AllowMultiple && quantity > 1 )
                        {
                            quantity = 1;
                        }

                        // Don't allow selecting any if other items of this fee are already selected
                        if ( !feeModel.AllowMultiple && totalFeeQuantity > 0 )
                        {
                            quantity = 0;
                        }

                        // Check if the item is selected (either actually selected or not allowed to be selected)
                        if ( quantity < 1 )
                        {
                            // The item is not selected, so remove it if it already exists
                            if ( registrantFee != null )
                            {
                                var oldFeeValue = $"'{registrantFee.RegistrationTemplateFee.Name}' Fee (Quantity:{registrantFee.Quantity:N0}, Cost:{registrantFee.Cost:C2}, Option:{registrantFee.Option}";
                                registrantChanges.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Fee" ).SetOldValue( oldFeeValue );

                                registrant.Fees.Remove( registrantFee );
                                registrantFeeService.Delete( registrantFee );
                            }

                            continue;
                        }

                        // Update the total quantity for this fee
                        totalFeeQuantity += quantity;

                        var feeName = $"{feeModel.Name} ({feeItemModel.Name})";

                        // Create the fee record if needed
                        if ( registrantFee == null )
                        {
                            registrantFee = new RegistrationRegistrantFee
                            {
                                RegistrationTemplateFeeId = feeModel.Id,
                                RegistrationTemplateFeeItemId = feeItemModel.Id,
                                Option = feeItemModel.Name
                            };

                            registrant.Fees.Add( registrantFee );
                            registrantChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Fee" ).SetNewValue( feeName );
                        }

                        // Update the cost and quantity of the fee record
                        History.EvaluateChange( registrantChanges, feeName + " Quantity", registrantFee.Quantity, quantity );
                        registrantFee.Quantity = quantity;

                        History.EvaluateChange( registrantChanges, feeName + " Cost", registrantFee.Cost, feeItemModel.Cost );
                        registrantFee.Cost = feeItemModel.Cost;
                    }
                }
            }

            rockContext.SaveChanges();
            registrantInfo.Guid = registrant.Guid;

            // Set any of the template's registrant attributes
            registrant.LoadAttributes();
            var attributeFields = registrationTemplate.Forms.SelectMany( f => f.Fields.Where( ff => ff.AttributeId.HasValue ) ).ToList();

            foreach ( var field in attributeFields )
            {
                var attribute = AttributeCache.Get( field.AttributeId.Value );

                if ( attribute is null )
                {
                    continue;
                }

                var newValue = registrantInfo.FieldValues.GetValueOrNull( field.Guid );
                var originalValue = registrant.GetAttributeValue( attribute.Key );
                var newValueAsString = newValue.ToStringSafe();
                registrant.SetAttributeValue( attribute.Key, newValueAsString );

                // DateTime values must be stored in ISO8601
                var isDateAttribute =
                    attribute.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.DATE.AsGuid() ) ||
                    attribute.FieldType.Guid.Equals( Rock.SystemGuid.FieldType.DATE_TIME.AsGuid() );

                if ( isDateAttribute && DateTime.TryParse( newValueAsString, out var asDateTime ) )
                {
                    newValueAsString = asDateTime.ToISO8601DateString();
                }

                if ( ( originalValue ?? string.Empty ).Trim() != ( newValueAsString ?? string.Empty ).Trim() )
                {
                    var formattedOriginalValue = string.Empty;
                    if ( !string.IsNullOrWhiteSpace( originalValue ) )
                    {
                        formattedOriginalValue = attribute.FieldType.Field.FormatValue( originalValue, attribute.QualifierValues, false );
                    }

                    string formattedNewValue = string.Empty;
                    if ( !string.IsNullOrWhiteSpace( newValueAsString ) )
                    {
                        formattedNewValue = attribute.FieldType.Field.FormatValue( newValueAsString, attribute.QualifierValues, false );
                    }

                    Helper.SaveAttributeValue( registrant, attribute, newValueAsString, rockContext );
                    History.EvaluateChange( registrantChanges, attribute.Name, formattedOriginalValue, formattedNewValue );
                }
            }

            var currentPerson = GetCurrentPerson();
            var currentPersonAliasId = currentPerson?.PrimaryAliasId;

            Task.Run( () =>
                HistoryService.SaveChanges(
                    new RockContext(),
                    typeof( Registration ),
                    Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                    registration.Id,
                    registrantChanges,
                    "Registrant: " + person.FullName,
                    null,
                    null,
                    true,
                    currentPersonAliasId ) );

            // Clear this registrant's family guid so it's not updated again
            registrantInfo.FamilyGuid = Guid.Empty;
            registrantInfo.PersonGuid = person.Guid;
        }

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <returns></returns>
        private RegistrationEntryBlockViewModel GetViewModel( RockContext rockContext )
        {
            var currentPerson = GetCurrentPerson();
            var registrationInstanceId = PageParameter( PageParameterKey.RegistrationInstanceId ).AsInteger();

            var registrationInstance = GetRegistrationInstanceQuery( rockContext, "RegistrationTemplate.Forms.Fields, RegistrationTemplate.Fees.FeeItems" )
                    .AsNoTracking()
                    .FirstOrDefault();

            var registrationTemplate = registrationInstance?.RegistrationTemplate;

            if ( registrationTemplate == null )
            {
                return null;
            }

            var formModels = registrationTemplate.Forms?.OrderBy( f => f.Order ).ToList() ?? new List<RegistrationTemplateForm>();

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
                instructions = registrationTemplate.RegistrationInstructions ?? string.Empty;
            }

            // Get the fee term
            var feeTerm = registrationTemplate.FeeTerm;

            if ( feeTerm.IsNullOrWhiteSpace() )
            {
                feeTerm = "Fee";
            }

            feeTerm = feeTerm.ToLower();
            var pluralFeeTerm = feeTerm.Pluralize();

            // Get the registrant term
            var registrantTerm = registrationTemplate.RegistrantTerm;

            if ( registrantTerm.IsNullOrWhiteSpace() )
            {
                registrantTerm = "Person";
            }

            registrantTerm = registrantTerm.ToLower();
            var pluralRegistrantTerm = registrantTerm.Pluralize();

            // Get the fees
            var feeModels = registrationTemplate.Fees?.OrderBy( f => f.Order ).ToList() ?? new List<RegistrationTemplateFee>();
            var fees = new List<RegistrationEntryBlockFeeViewModel>();

            foreach ( var feeModel in feeModels )
            {
                var feeViewModel = new RegistrationEntryBlockFeeViewModel
                {
                    Guid = feeModel.Guid,
                    Name = feeModel.Name,
                    AllowMultiple = feeModel.AllowMultiple,
                    IsRequired = feeModel.IsRequired,
                    DiscountApplies = feeModel.DiscountApplies,
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
                    field.ShowOnWaitList = fieldModel.ShowOnWaitlist;

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
            var registrationAttributeTitleStart = ( registrationTemplate.RegistrationAttributeTitleStart ).IsNullOrWhiteSpace() ?
                "Registration Information" :
                registrationTemplate.RegistrationAttributeTitleStart;

            var registrationAttributeTitleEnd = ( registrationTemplate.RegistrationAttributeTitleEnd ).IsNullOrWhiteSpace() ?
                "Registration Information" :
                registrationTemplate.RegistrationAttributeTitleEnd;

            // Get the registration term
            var registrationTerm = ( registrationTemplate.RegistrationTerm ).IsNullOrWhiteSpace() ?
                "Registration" :
                registrationTemplate.RegistrationTerm;

            // Get the registration term plural
            var pluralRegistrationTerm = registrationTerm.Pluralize();

            // Get the registration attributes
            var registrationAttributes = GetRegistrationAttributes( registrationTemplate.Id );

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
            var maxRegistrants = ( registrationTemplate.AllowMultipleRegistrants == true ) ?
                ( registrationTemplate.MaxRegistrants ?? 1 ) :
                1;

            // Get the number of slots available and if the waitlist is available
            var waitListEnabled = registrationTemplate.WaitListEnabled;
            var spotsRemaining = registrationInstance.MaxAttendees;

            if ( spotsRemaining.HasValue )
            {
                var otherRegistrantsCount = new RegistrationRegistrantService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Count( a =>
                        a.Registration.RegistrationInstanceId == registrationInstanceId &&
                        !a.Registration.IsTemporary );

                spotsRemaining = spotsRemaining.Value - otherRegistrantsCount;
            }

            if ( spotsRemaining < 0 )
            {
                spotsRemaining = 0;
            }

            // Force the registrar to update their email?
            var forceEmailUpdate = GetAttributeValue( AttributeKey.ForceEmailUpdate ).AsBoolean();

            // Load the gateway control settings
            var financialGatewayId = registrationTemplate.FinancialGatewayId ?? 0;
            var financialGateway = new FinancialGatewayService( rockContext ).GetNoTracking( financialGatewayId );
            var financialGatewayComponent = financialGateway?.GetGatewayComponent() as IObsidianFinancialGateway;

            // Get the amount due today and the initial amount to recommend paying
            var amountDueToday = registrationTemplate.SetCostOnInstance == true ?
                registrationInstance.MinimumInitialPayment :
                registrationTemplate.MinimumInitialPayment;

            var initialAmountToPay = registrationTemplate.SetCostOnInstance == true ?
                registrationInstance.DefaultPayment :
                registrationTemplate.DefaultPayment;

            return new RegistrationEntryBlockViewModel
            {
                RegistrationGuid = null,
                RegistrationAttributesStart = beforeAttributes,
                RegistrationAttributesEnd = afterAttributes,
                RegistrationAttributeTitleStart = registrationAttributeTitleStart,
                RegistrationAttributeTitleEnd = registrationAttributeTitleEnd,
                InstructionsHtml = instructions,
                RegistrantTerm = registrantTerm,
                PluralRegistrantTerm = pluralRegistrantTerm,
                PluralFeeTerm = pluralFeeTerm,
                RegistrationTerm = registrationTerm,
                RegistrantForms = formViewModels,
                Fees = fees,
                FamilyMembers = familyMembers,
                MaxRegistrants = maxRegistrants,
                DoAskForFamily = registrationTemplate.RegistrantsSameFamily == RegistrantsSameFamily.Ask,
                ForceEmailUpdate = forceEmailUpdate,
                RegistrarOption = ( int ) registrationTemplate.RegistrarOption,
                Cost = GetBaseRegistrantCost( registrationTemplate, registrationInstance ),
                GatewayControl = new GatewayControlViewModel
                {
                    FileUrl = financialGatewayComponent?.GetObsidianControlFileUrl( financialGateway ) ?? string.Empty,
                    Settings = financialGatewayComponent?.GetObsidianControlSettings( financialGateway ) ?? new object()
                },
                SpotsRemaining = spotsRemaining,
                WaitListEnabled = waitListEnabled,
                InstanceName = registrationInstance.Name,
                PluralRegistrationTerm = pluralRegistrationTerm,
                AmountDueToday = amountDueToday,
                InitialAmountToPay = initialAmountToPay
            };
        }

        /// <summary>
        /// Gets the registration attributes.
        /// </summary>
        /// <param name="registrationTemplateId">The registration template identifier.</param>
        /// <returns></returns>
        private List<AttributeCache> GetRegistrationAttributes( int registrationTemplateId )
        {
            var currentPerson = GetCurrentPerson();
            var registrationEntityTypeId = EntityTypeCache.Get<Registration>().Id;

            var registrationAttributes = AttributeCache.All()
                .Where( a =>
                    a.IsActive &&
                    a.EntityTypeId == registrationEntityTypeId &&
                    a.EntityTypeQualifierColumn.Equals( nameof( RegistrationInstance.RegistrationTemplateId ), StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( registrationTemplateId.ToStringSafe() ) &&
                    a.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();

            return registrationAttributes;
        }

        /// <summary>
        /// Processes the payment.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="registration">The registration.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private Guid? ProcessPayment(
            RockContext rockContext,
            RegistrationTemplate registrationTemplate,
            RegistrationInstance registrationInstance,
            Registration registration,
            RegistrationEntryBlockArgs args,
            Person registrar,
            out string errorMessage )
        {
            var gateway = registrationTemplate?.FinancialGateway?.GetGatewayComponent();

            if ( gateway == null )
            {
                errorMessage = "There was a problem creating the payment gateway information";
                return null;
            }

            if ( !registrationInstance.AccountId.HasValue || registrationInstance.Account == null )
            {
                errorMessage = "There was a problem with the account configuration for this " + registrationTemplate.RegistrationTerm.ToLower();
                return null;
            }

            var paymentInfo = new ReferencePaymentInfo
            {
                Amount = args.AmountToPayNow,
                Email = args.Registrar.Email,
                FirstName = args.Registrar.NickName,
                LastName = args.Registrar.LastName,
                ReferenceNumber = args.GatewayToken,
                Comment1 = string.Format( "{0} ({1})", registrationInstance.Name, registrationInstance.Account.GlCode )
            };

            var transaction = gateway.Charge( registrationTemplate.FinancialGateway, paymentInfo, out errorMessage );
            return SaveTransaction( gateway, registrationTemplate, registrationInstance, registration, transaction, paymentInfo, rockContext, args.AmountToPayNow );
        }

        /// <summary>
        /// Saves the transaction.
        /// </summary>
        /// <param name="gateway">The gateway.</param>
        /// <param name="registration">The registration.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Guid? SaveTransaction(
            GatewayComponent gateway,
            RegistrationTemplate registrationTemplate,
            RegistrationInstance registrationInstance,
            Registration registration,
            FinancialTransaction transaction,
            PaymentInfo paymentInfo,
            RockContext rockContext,
            decimal amount )
        {
            if ( transaction is null )
            {
                return null;
            }

            var currentPerson = GetCurrentPerson();

            transaction.AuthorizedPersonAliasId = registration.PersonAliasId;
            transaction.TransactionDateTime = RockDateTime.Now;
            transaction.FinancialGatewayId = registrationTemplate.FinancialGatewayId;

            var txnType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION ) );
            transaction.TransactionTypeValueId = txnType.Id;

            if ( transaction.FinancialPaymentDetail == null )
            {
                transaction.FinancialPaymentDetail = new FinancialPaymentDetail();
            }

            DefinedValueCache currencyType = null;
            DefinedValueCache creditCardType = null;

            if ( paymentInfo != null )
            {
                transaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, gateway, rockContext );
                currencyType = paymentInfo.CurrencyTypeValue;
                creditCardType = paymentInfo.CreditCardTypeValue;
            }

            Guid sourceGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( AttributeKey.Source ), out sourceGuid ) )
            {
                var source = DefinedValueCache.Get( sourceGuid );
                if ( source != null )
                {
                    transaction.SourceTypeValueId = source.Id;
                }
            }

            transaction.Summary = registration.GetSummary( registrationInstance );

            var transactionDetail = new FinancialTransactionDetail();
            transactionDetail.Amount = amount;
            transactionDetail.AccountId = registrationInstance.AccountId.Value;
            transactionDetail.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Registration ) ).Id;
            transactionDetail.EntityId = registration.Id;
            transaction.TransactionDetails.Add( transactionDetail );

            var batchChanges = new History.HistoryChangeList();

            rockContext.WrapTransaction( () =>
            {
                var batchService = new FinancialBatchService( rockContext );

                // determine batch prefix
                string batchPrefix = string.Empty;
                if ( !string.IsNullOrWhiteSpace( registrationTemplate.BatchNamePrefix ) )
                {
                    batchPrefix = registrationTemplate.BatchNamePrefix;
                }
                else
                {
                    batchPrefix = GetAttributeValue( AttributeKey.BatchNamePrefix );
                }

                // Get the batch
                var batch = batchService.Get(
                batchPrefix,
                currencyType,
                creditCardType,
                transaction.TransactionDateTime.Value,
                registrationTemplate.FinancialGateway.GetBatchTimeOffset() );

                if ( batch.Id == 0 )
                {
                    batchChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Batch" );
                    History.EvaluateChange( batchChanges, "Batch Name", string.Empty, batch.Name );
                    History.EvaluateChange( batchChanges, "Status", null, batch.Status );
                    History.EvaluateChange( batchChanges, "Start Date/Time", null, batch.BatchStartDateTime );
                    History.EvaluateChange( batchChanges, "End Date/Time", null, batch.BatchEndDateTime );
                }

                var financialTransactionService = new FinancialTransactionService( rockContext );

                // If this is a new Batch, SaveChanges so that we can get the Batch.Id
                if ( batch.Id == 0 )
                {
                    rockContext.SaveChanges();
                }

                transaction.BatchId = batch.Id;

                // use the financialTransactionService to add the transaction instead of batch.Transactions to avoid lazy-loading the transactions already associated with the batch
                financialTransactionService.Add( transaction );
                rockContext.SaveChanges();

                batchService.IncrementControlAmount( batch.Id, transaction.TotalAmount, batchChanges );
                rockContext.SaveChanges();
            } );

            if ( transaction.BatchId.HasValue )
            {
                Task.Run( () =>
                    HistoryService.SaveChanges(
                        new RockContext(),
                        typeof( FinancialBatch ),
                        Rock.SystemGuid.Category.HISTORY_FINANCIAL_BATCH.AsGuid(),
                        transaction.BatchId.Value,
                        batchChanges,
                        true,
                        currentPerson?.PrimaryAliasId ) );
            }

            var registrationChanges = new History.HistoryChangeList();
            registrationChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Payment" ).SetNewValue( string.Format( "{0} payment", transaction.TotalAmount.FormatAsCurrency() ) );
            Task.Run( () =>
                HistoryService.SaveChanges(
                    new RockContext(),
                    typeof( Registration ),
                    Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                    registration.Id,
                    registrationChanges,
                    true,
                    currentPerson?.PrimaryAliasId ) );

            return transaction.Guid;
        }

        /// <summary>
        /// Gets the success view model.
        /// </summary>
        /// <param name="registrationId">The registration identifier.</param>
        /// <returns></returns>
        private RegistrationEntryBlockSuccessViewModel GetSuccessViewModel( int registrationId )
        {
            var currentPerson = GetCurrentPerson();

            // Create a view model with default values in case anything goes wrong
            var viewModel = new RegistrationEntryBlockSuccessViewModel
            {
                TitleHtml = "Congratulations",
                MessageHtml = "You have successfully completed this registration."
            };

            try
            {
                var rockContext = new RockContext();
                var registration = new RegistrationService( rockContext )
                    .Queryable( "RegistrationInstance.RegistrationTemplate" )
                    .FirstOrDefault( r => r.Id == registrationId );

                if ( registration != null &&
                    registration.RegistrationInstance != null &&
                    registration.RegistrationInstance.RegistrationTemplate != null )
                {
                    var template = registration.RegistrationInstance.RegistrationTemplate;

                    var mergeFields = new Dictionary<string, object>
                    {
                        { "CurrentPerson", currentPerson },
                        { "RegistrationInstance", registration.RegistrationInstance },
                        { "Registration", registration }
                    };

                    if ( template != null && !string.IsNullOrWhiteSpace( template.SuccessTitle ) )
                    {
                        viewModel.TitleHtml = template.SuccessTitle.ResolveMergeFields( mergeFields );
                    }
                    else
                    {
                        viewModel.TitleHtml = "Congratulations";
                    }

                    if ( template != null && !string.IsNullOrWhiteSpace( template.SuccessText ) )
                    {
                        viewModel.MessageHtml = template.SuccessText.ResolveMergeFields( mergeFields );
                    }
                    else
                    {
                        viewModel.MessageHtml = "You have successfully completed this " + template.RegistrationTerm.ToLower();
                    }
                }
            }
            catch ( Exception ex )
            {
                // Log the exception, but continue since we need to display the confirmation page. The person has been registered and this
                // error just means the success lava went wrong somehow.
                ExceptionLogService.LogException( ex );
            }

            return viewModel;
        }

        #endregion Helpers
    }
}
