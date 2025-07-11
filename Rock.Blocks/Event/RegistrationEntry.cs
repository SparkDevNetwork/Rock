﻿// <copyright>
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
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.ClientService.Core.Campus;
using Rock.ClientService.Finance.FinancialPersonSavedAccount;
using Rock.ClientService.Finance.FinancialPersonSavedAccount.Options;
using Rock.Crm.RecordSource;
using Rock.Data;
using Rock.ElectronicSignature;
using Rock.Field;
using Rock.Financial;
using Rock.Model;
using Rock.Pdf;
using Rock.Security;
using Rock.Tasks;
using Rock.Utility;
using Rock.ViewModels.Blocks.Event.RegistrationEntry;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Finance;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Registration Entry.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />

    [DisplayName( "Registration Entry" )]
    [Category( "Event" )]
    [Description( "Block used to register for a registration instance." )]
    [IconCssClass( "fa fa-clipboard-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [DefinedValueField( "Connection Status",
        Key = AttributeKey.ConnectionStatus,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS,
        Description = "The connection status to use for new individuals (default: 'Prospect'). If the Registration Template has a connection status set, then this setting will be overridden by that value.",
        IsRequired = true,
        AllowMultiple = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PROSPECT,
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

    [TextField( "Family Term",
        Description = "The term to use for specifying which household or family a person is a member of.",
        IsRequired = true,
        DefaultValue = "immediate family",
        Order = 8,
        Key = AttributeKey.FamilyTerm )]

    [BooleanField( "Force Email Update",
        Description = "If enabled, no checkbox option will be available on the final confirmation screen regarding whether or not to update the  Registrar's email address. Instead, the registrar's email address will be updated to match the supplied Confirmation Email.",
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

    [BooleanField(
        "Enable ACH",
        Key = AttributeKey.EnableACH,
        Description = "Enabling this will also control which type of Saved Accounts can be used during the payment process.  The payment gateway must still be configured to support ACH payments.",
        DefaultBooleanValue = true,
        Order = 12 )]

    [BooleanField(
        "Enable Credit Card",
        Key = AttributeKey.EnableCreditCard,
        Description = "Enabling this will also control which type of Saved Accounts can be used during the payment process.  The payment gateway must still be configured to support Credit Card payments.",
        DefaultBooleanValue = true,
        Order = 13 )]

    [BooleanField(
        "Disable Captcha Support",
        Key = AttributeKey.DisableCaptchaSupport,
        Description = "If set to 'Yes' the CAPTCHA verification step will not be performed.",
        DefaultBooleanValue = false,
        Order = 17 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.OBSIDIAN_EVENT_REGISTRATION_ENTRY )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.OBSIDIAN_EVENT_REGISTRATION_ENTRY )]
    public class RegistrationEntry : RockBlockType, IBreadCrumbBlock
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
            public const string FamilyTerm = "FamilyTerm";
            public const string ForceEmailUpdate = "ForceEmailUpdate";
            public const string ShowFieldDescriptions = "ShowFieldDescriptions";
            public const string EnableSavedAccount = "EnableSavedAccount";
            public const string EnableACH = "EnableACH";
            public const string EnableCreditCard = "EnableCreditCard";
            public const string DisableCaptchaSupport = "DisableCaptchaSupport";
        }

        /// <summary>
        /// Page Parameter
        /// </summary>
        private static class PageParameterKey
        {
            public const string RegistrationId = "RegistrationId";
            public const string RegistrationInstanceId = "RegistrationInstanceId";
            public const string RegistrationSessionGuid = "SessionGuid";
            public const string CampusId = "CampusId";
            public const string Slug = "Slug";
            public const string GroupId = "GroupId";
            public const string StartAtBeginning = "StartAtBeginning";
            public const string EventOccurrenceId = "EventOccurrenceId";
        }

        /// <summary>
        /// The prefix to look for when identifying if any page parameters are
        /// for a return-from-payment redirect.
        /// </summary>
        public const string ReturnUrlSessionPrefix = "sessionGuid";

        #endregion Keys

        #region Properties

        /// <summary>
        /// A diagnostic collection of missing fields, grouped by form ID.
        /// </summary>
        public Dictionary<int, Dictionary<int, string>> MissingFieldsByFormId { get; set; }

        /// <summary>
        /// Gets the registration identifier page parameter.
        /// </summary>
        public int? RegistrationIdPageParameter => PageParameter( PageParameterKey.RegistrationId ).AsIntegerOrNull();

        /// <summary>
        /// Gets the event occurrence identifier page parameter.
        /// </summary>
        public int? EventOccurrenceIdPageParameter => PageParameter( PageParameterKey.EventOccurrenceId ).AsIntegerOrNull();

        #endregion Properties

        #region Obsidian Block Type Overrides

        /// <summary>
        /// Gets the property values that will be sent to the browser.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = GetInitializationBox( rockContext );
                var instanceName = box.InstanceName;

                if ( instanceName.IsNullOrWhiteSpace() && ( box.RegistrationInstanceNotFoundMessage?.Contains( " closed on " ) == true
                    || box.RegistrationInstanceNotFoundMessage?.Contains( " does not open " ) == true ) )
                {
                    // The view model did not have a name filled in even though
                    // we found the registration instance. Get the instance name
                    // only so we can fill in the page name.
                    var registrationInstanceId = GetRegistrationInstanceId( rockContext );
                    instanceName = new RegistrationInstanceService( rockContext )
                        .GetSelect( registrationInstanceId, ri => ri.Name );
                }

                if ( instanceName.IsNotNullOrWhiteSpace() )
                {
                    ResponseContext.SetPageTitle( instanceName );
                    ResponseContext.SetBrowserTitle( instanceName );
                }

                return box;
            }
        }

        #endregion Obsidian Block Type Overrides

        #region Block Actions

        /// <summary>
        /// Checks the discount code provided. If a null/blank string is used then checks for AutoApplied discounts.
        /// </summary>
        /// <param name="code">The code.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult CheckDiscountCode( string code, int registrantCount, Guid? registrationGuid, bool isAutoApply )
        {
            using ( var rockContext = new RockContext() )
            {
                var registrationInstanceId = GetRegistrationInstanceId( rockContext );
                var registrationTemplateDiscountService = new RegistrationTemplateDiscountService( rockContext );
                RegistrationTemplateDiscountWithUsage discount = null;
                var registration = registrationGuid != null ? new RegistrationService( rockContext ).Get( registrationGuid.ToString() ) : null;

                if ( isAutoApply && code.IsNullOrWhiteSpace() && ( registration == null || registration.DiscountCode.IsNullOrWhiteSpace() ) )
                {
                    // if no code is provided and there is no code already saved in the registration check for an auto apply discount, if there are none discount will be null which returns ActionNotFound
                    var registrationTemplateDiscountCodes = registrationTemplateDiscountService
                        .GetDiscountsForRegistrationInstance( registrationInstanceId )
                        .Where( d => d.AutoApplyDiscount )
                        .OrderBy( d => d.Order )
                        .Select( d => d.Code )
                        .ToList();

                    foreach ( var registrationTemplateDiscountCode in registrationTemplateDiscountCodes )
                    {
                        discount = registrationTemplateDiscountService.GetDiscountByCodeIfValid( registrationInstanceId, registrationTemplateDiscountCode );

                        // This means the date is outside the defined date range, or the max usages has already been hit.
                        if ( discount == null )
                        {
                            continue;
                        }

                        // Check if the registration meets the discount's minimum required registrants
                        if ( discount.RegistrationTemplateDiscount.MinRegistrants.HasValue && registrantCount < discount.RegistrationTemplateDiscount.MinRegistrants.Value )
                        {
                            continue;
                        }

                        // use the first discount that is valid
                        break;
                    }
                }
                else if ( code.IsNotNullOrWhiteSpace() && ( registration == null || registration.DiscountCode.IsNullOrWhiteSpace() ) )
                {
                    // if code is provided and there is no code in saved in the registration check the provided code using GetDiscountByCodeIfValid( registrationInstanceId, code )
                    discount = registrationTemplateDiscountService.GetDiscountByCodeIfValid( registrationInstanceId, code );
                }
                else if ( code.IsNotNullOrWhiteSpace() && registration != null && registration.DiscountCode.IsNotNullOrWhiteSpace() && !string.Equals( code, registration.DiscountCode, StringComparison.OrdinalIgnoreCase ) )
                {
                    // if code is provided and there is a code saved in the registration and they are different then check the provided code using GetDiscountByCodeIfValid( registrationInstanceId, code )
                    discount = registrationTemplateDiscountService.GetDiscountByCodeIfValid( registrationInstanceId, code );
                }
                else if ( registration != null && registration.DiscountCode.IsNotNullOrWhiteSpace() )
                {
                    // At this point use the code saved in the registration if it exists without checking in case the code is no longer valid (e.g. expired);
                    // however, try to get the max registrants if the discount is valid.
                    discount = registrationTemplateDiscountService.GetDiscountByCodeIfValid( registrationInstanceId, code );
                    return ActionOk( new
                    {
                        DiscountCode = registration.DiscountCode,
                        RegistrationUsagesRemaining = ( int? ) null,
                        DiscountAmount = registration.DiscountAmount,
                        DiscountPercentage = registration.DiscountPercentage,
                        DiscountMaxRegistrants = discount?.RegistrationTemplateDiscount.MaxRegistrants
                    } );
                }

                if ( discount == null || discount.RegistrationUsagesRemaining < 1 )
                {
                    // The code is not found
                    return ActionNotFound();
                }

                if ( discount.RegistrationTemplateDiscount.MinRegistrants.HasValue && registrantCount < discount.RegistrationTemplateDiscount.MinRegistrants.Value )
                {
                    // Do not show an error if the discount is being auto applied.
                    if ( !isAutoApply || discount.RegistrationTemplateDiscount.AutoApplyDiscount == false )
                    {
                        return ActionForbidden( $"The discount requires a minimum of {discount.RegistrationTemplateDiscount.MinRegistrants.Value} registrants" );
                    }

                    return ActionNotFound();
                }

                return ActionOk( new
                {
                    DiscountCode = discount.RegistrationTemplateDiscount.Code,
                    RegistrationUsagesRemaining = discount.RegistrationUsagesRemaining,
                    DiscountAmount = discount.RegistrationTemplateDiscount.DiscountAmount,
                    DiscountPercentage = discount.RegistrationTemplateDiscount.DiscountPercentage,
                    DiscountMaxRegistrants = discount.RegistrationTemplateDiscount.MaxRegistrants
                } );
            }
        }

        /// <summary>
        /// Gets the payment redirect.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="sessionUrl">The URL currently being viewed in the browser for the registration session.</param>
        /// <returns>The URL to redirect the person to in order to handle payment.</returns>
        [BlockAction]
        public BlockActionResult GetPaymentRedirect( RegistrationEntryArgsBag args, string returnUrl )
        {
            using ( var rockContext = new RockContext() )
            {
                var context = GetContext( rockContext, args, out var errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return ActionBadRequest( errorMessage );
                }

                if ( PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull() == null )
                {
                    var groupId = GetRegistrationGroupId( rockContext, context?.Registration?.RegistrationInstanceId );
                    if ( groupId.HasValue )
                    {
                        RequestContext.PageParameters.Add( PageParameterKey.GroupId, groupId.ToString() );
                    }
                }

                var session = UpsertSession( context, args, SessionStatus.PaymentPending, out errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return ActionBadRequest( errorMessage );
                }

                if ( !Uri.TryCreate( returnUrl, UriKind.Absolute, out var uri ) )
                {
                    return ActionBadRequest( "Invalid return URL specified." );
                }

                // Parse the original query string and replace/insert our session key.
                var queryString = uri.Query.ParseQueryString();
                queryString.Remove( PageParameterKey.RegistrationSessionGuid );
                queryString.Add( PageParameterKey.RegistrationSessionGuid, session.Guid.ToString() );

                // Create the new return URI with the updated query string.
                var returnUri = new UriBuilder( uri )
                {
                    Query = queryString.ToQueryString()
                };

                // Generate the redirect URL
                var redirectUrl = GenerateRedirectUrl( rockContext, context, args.AmountToPayNow, args.Registrar, args.Registrants, session.Guid, returnUri.ToString() );

                return ActionOk( redirectUrl );
            }
        }

        /// <summary>
        /// Persists the session.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult PersistSession( RegistrationEntryArgsBag args )
        {
            using ( var rockContext = new RockContext() )
            {
                var context = GetContext( rockContext, args, out var errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return ActionBadRequest( errorMessage );
                }

                var session = UpsertSession( context, args, SessionStatus.PaymentPending, out errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return ActionBadRequest( errorMessage );
                }

                return ActionOk( new
                {
                    ExpirationDateTime = session.ExpirationDateTime.ToRockDateTimeOffset()
                } );
            }
        }

        /// <summary>
        /// Calculates the cost.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult CalculateCost( RegistrationEntryArgsBag args )
        {
            using ( var rockContext = new RockContext() )
            {
                // Ensure the arguments provided are in their proper format
                // before use (e.g. proper currency formatting of amounts).
                FixRegistrationArguments( args );

                var context = GetContext( rockContext, args, out var errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return ActionBadRequest( errorMessage );
                }

                var costs = GetRegistrationCosts( rockContext, context, args )
                    .AsRegistrationCostSummaryBagListOrNull();

                return ActionOk( costs );
            }
        }

        /// <summary>
        /// Persists the session.
        /// </summary>
        /// <param name="registrationSessionGuid">The registration session unique identifier to be renewed.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult TryToRenewSession( Guid registrationSessionGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var registrationSession = RegistrationSessionService.TryToRenewSession( registrationSessionGuid );

                // Null means the session failed to renew, generally because the
                // Guid wasn't valid.
                if ( registrationSession == null )
                {
                    return ActionNotFound();
                }

                return ActionOk( new SessionRenewalResultBag
                {
                    SpotsSecured = registrationSession.RegistrationCount,
                    ExpirationDateTime = registrationSession.ExpirationDateTime.ToRockDateTimeOffset()
                } );
            }
        }

        /// <summary>
        /// Submits the registration.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult SubmitRegistration( RegistrationEntryArgsBag args )
        {
            var disableCaptcha = GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean();

            if ( !disableCaptcha && !RequestContext.IsCaptchaValid )
            {
                return ActionBadRequest( "Captcha was not valid." );
            }

            using ( var rockContext = new RockContext() )
            {
                FixRegistrationArguments( args );

                if ( args.PaymentPlan != null && !IsPaymentPlanValid( args.PaymentPlan, out var paymentPlanInvalidErrorMessage ) )
                {
                    return ActionBadRequest( paymentPlanInvalidErrorMessage );
                }

                var context = GetContext( rockContext, args, out var errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return ActionBadRequest( errorMessage );
                }

                SubmitRegistration( rockContext, context, args, out errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return ActionBadRequest( errorMessage );
                }

                var successViewModel = GetSuccessViewModel( context.Registration.Id, context.TransactionCode, context.GatewayPersonIdentifier );

                return new BlockActionResult( System.Net.HttpStatusCode.Created, successViewModel );
            }
        }

        /// <summary>
        /// Gets the signature document data for a specific registrant.
        /// </summary>
        /// <param name="args">The registration entry arguments.</param>
        /// <param name="registrantGuid">The unique identifier of the registrant to build the signature document.</param>
        /// <returns>An instance of <see cref="RegistrationEntrySignatureDocumentBag"/> that contains the document information.</returns>
        [BlockAction]
        public BlockActionResult GetSignatureDocumentData( RegistrationEntryArgsBag args, Guid registrantGuid )
        {
            var registrantInfo = args.Registrants.FirstOrDefault( r => r.Guid == registrantGuid );

            if ( registrantInfo == null )
            {
                return ActionBadRequest( "Invalid registrant." );
            }

            using ( var rockContext = new RockContext() )
            {
                var context = GetContext( rockContext, args, out var errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return ActionBadRequest( errorMessage );
                }

                var registrationInstance = new RegistrationInstanceService( rockContext ).Get( context.RegistrationSettings.RegistrationInstanceId );
                var documentTemplate = new SignatureDocumentTemplateService( rockContext ).Get( context.RegistrationSettings.SignatureDocumentTemplateId ?? 0 );

                if ( documentTemplate == null || !documentTemplate.IsActive/* OR IS LEGACY */)
                {
                    return ActionBadRequest( "Invalid signature template." );
                }

                var isNewRegistration = context.Registration == null;
                Person registrar = null;

                if ( isNewRegistration )
                {
                    // This is a new registration, generate a fake registration
                    // for the various support methods used.
                    context.Registration = new Registration
                    {
                        RegistrationInstanceId = context.RegistrationSettings.RegistrationInstanceId,
                        RegistrationTemplateId = context.RegistrationSettings.RegistrationTemplateId
                    };

                    if ( context.RegistrationSettings.RegistrarOption == RegistrarOption.UseLoggedInPerson && RequestContext.CurrentPerson != null )
                    {
                        registrar = RequestContext.CurrentPerson;
                        context.Registration.PersonAliasId = registrar.PrimaryAliasId;
                    }
                }
                else
                {
                    // This is an existing registration, re-use the old registrar.
                    registrar = context.Registration.PersonAlias.Person;

                    var registrationService = new RegistrationService( rockContext );
                    var previousRegistration = registrationService.Get( args.RegistrationGuid.Value );

                    if ( previousRegistration != null )
                    {
                        isNewRegistration = false;
                    }
                }

                // If the registrar person record does not exist, try to find the record.
                if ( registrar == null )
                {
                    registrar = GetExistingRegistrarPerson( context, RequestContext.CurrentPerson, rockContext );
                }

                var registrarFamily = registrar?.GetFamily( rockContext );

                // Process the Person so we have data for the Lava merge.
                bool isCreatedAsRegistrant = context.RegistrationSettings.RegistrarOption == RegistrarOption.UseFirstRegistrant && registrantInfo == args.Registrants.FirstOrDefault();
                var (person, registrant) = GetExistingOrCreatePerson( context, registrantInfo, registrar, registrarFamily?.Guid ?? Guid.Empty, isCreatedAsRegistrant, rockContext );

                var response = new RegistrationEntrySignatureDocumentBag();

                // If the person happens to have a valid signature document of the required template, we may skip this step.
                if ( documentTemplate.IsValidInFuture && documentTemplate.ValidityDurationInDays.HasValue )
                {
                    // When thinking about date comparisons, think in terms of extremes:
                    //  - If they signed a document today, and it's only valid for 1 day, it's still valid (at any point) today.
                    //  - If they signed a document (at any point) yesterday or before, and it's only valid for 1 day, it's no longer valid today.
                    // With this in mind, add one day to the specified ValidityDurationInDays before comparing.
                    var earliestSignatureDate = RockDateTime.Today.AddDays( -documentTemplate.ValidityDurationInDays.ToIntSafe() + 1 );
                    var existingSignatureDocument = new RegistrationRegistrantService( rockContext )
                        .Queryable()
                        .Where( r =>
                            r.PersonAlias.PersonId == person.Id &&
                            r.SignatureDocument.SignatureDocumentTemplateId == documentTemplate.Id &&
                            r.SignatureDocument.SignedDateTime >= earliestSignatureDate )
                        .OrderByDescending( r => r.SignatureDocument.SignedDateTime )
                        .Select( r => new
                        {
                            r.SignatureDocument.Guid
                        } )
                        .FirstOrDefault();

                    if ( existingSignatureDocument != null )
                    {
                        response.ExistingSignatureDocumentGuid = existingSignatureDocument.Guid;

                        return ActionOk( response );
                    }
                }

                var (campusId, location, _) = UpdatePersonFromRegistrant( person, registrantInfo, new History.HistoryChangeList(), context.RegistrationSettings );

                if ( person.Attributes == null )
                {
                    person.LoadAttributes( rockContext );
                }

                UpdatePersonAttributes( person, new History.HistoryChangeList(), registrantInfo, context.RegistrationSettings );

                // Process the GroupMember so we have data for the Lava merge.
                GroupMember groupMember = null;
                var groupId = GetRegistrationGroupId( rockContext, context.Registration.RegistrationInstanceId );

                if ( groupId.HasValue )
                {
                    var group = new GroupService( rockContext ).Get( groupId.Value );

                    groupMember = BuildGroupMember( person, group, context.RegistrationSettings );
                    groupMember.LoadAttributes( rockContext );
                    UpdateGroupMemberAttributes( groupMember, registrantInfo, context.RegistrationSettings );
                }

                // Prepare the merge fields.
                var campusCache = campusId.HasValue ? CampusCache.Get( campusId.Value ) : null;

                var mergeFields = new Dictionary<string, object>
                {
                    { "Registration", new LavaSignatureRegistration( registrationInstance, groupId, args.Registrants.Count ) },
                    { "Registrant", new LavaSignatureRegistrant( person, location, campusCache, groupMember, registrantInfo, registrationInstance ) }
                };

                var html = ElectronicSignatureHelper.GetSignatureDocumentHtml( documentTemplate.LavaTemplate, mergeFields );

                // Calculate a document hash from the registrant information to
                // be used later to validate the document after signing.
                var fieldHashToken = GetRegistrantSignatureHashToken( registrantInfo );
                var unencryptedSecurityToken = new[] { RockDateTime.Now.ToString( "o" ), GetSha256Hash( fieldHashToken + html ) }.ToJson();
                var encryptedSecurityToken = Encryption.EncryptString( unencryptedSecurityToken );

                response.DocumentHtml = html;
                response.SecurityToken = encryptedSecurityToken;

                return ActionOk( response );
            }
        }

        /// <summary>
        /// Signs the document previously returned by <see cref="GetSignatureDocumentData(RegistrationEntryArgsBag, Guid)"/>.
        /// </summary>
        /// <param name="args">The registration entry arguments.</param>
        /// <param name="registrantGuid">The unique identifier of the registrant this document will apply to.</param>
        /// <param name="documentHtml">The document HTML that was signed.</param>
        /// <param name="securityToken">The security token to validate the request.</param>
        /// <param name="signature">The signature of the person that signed the document.</param>
        /// <returns>A string that contains the encoded signed document details.</returns>
        [BlockAction]
        public BlockActionResult SignDocument( RegistrationEntryArgsBag args, Guid registrantGuid, string documentHtml, string securityToken, ElectronicSignatureValueViewModel signature )
        {
            var registrantInfo = args.Registrants.FirstOrDefault( r => r.Guid == registrantGuid );

            if ( registrantInfo == null )
            {
                return ActionBadRequest( "Invalid registrant." );
            }

            using ( var rockContext = new RockContext() )
            {
                var context = GetContext( rockContext, args, out var errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return ActionBadRequest( errorMessage );
                }

                var registrationInstance = new RegistrationInstanceService( rockContext ).Get( context.RegistrationSettings.RegistrationInstanceId );
                var documentTemplate = new SignatureDocumentTemplateService( rockContext ).Get( context.RegistrationSettings.SignatureDocumentTemplateId ?? 0 );

                if ( documentTemplate == null || !documentTemplate.IsActive/* OR IS LEGACY */)
                {
                    return ActionBadRequest( "Invalid signature template." );
                }

                // Validate they did not modify any of the fields or the signed HTML.
                var unencryptedSecurityToken = Encryption.DecryptString( securityToken ).FromJsonOrNull<List<string>>();
                var fieldHashToken = GetRegistrantSignatureHashToken( registrantInfo );
                var hash = GetSha256Hash( fieldHashToken + documentHtml );

                if ( unencryptedSecurityToken == null || unencryptedSecurityToken.Count != 2 || hash != unencryptedSecurityToken[1] )
                {
                    return ActionBadRequest( "Invalid security token." );
                }

                // Create the details of the signed document for later use.
                var signedData = new SignedDocumentData
                {
                    IpAddress = RequestContext.ClientInformation.IpAddress,
                    UserAgent = RequestContext.ClientInformation.UserAgent,
                    DocumentHtml = documentHtml,
                    SignedDateTime = RockDateTime.Now,
                    SignatureData = signature.SignatureData,
                    SignedByName = signature.SignedByName,
                    SignedByEmail = signature.SignedByEmail
                };

                return ActionOk( Encryption.EncryptString( signedData.ToJson() ) );
            }
        }

        /// <summary>
        /// Gets the attribute field values for the registrant, does not get PersonFields which are handled elsewhere. This action is used to get the default attribute values.
        /// </summary>
        /// <param name="args">The registration entry arguments.</param>
        /// <param name="registrantGuid">The registrant unique identifier of the registrant</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetDefaultAttributeFieldValues( RegistrationEntryArgsBag args, RegistrationEntryFormBag[] forms, Guid registrantGuid )
        {
            var registrantInfo = args.Registrants.FirstOrDefault( r => r.Guid == registrantGuid );
            var fieldValues = new Dictionary<Guid, object>();

            if ( forms == null || forms.Length == 0 )
            {
                return ActionOk( fieldValues );
            }

            using ( var rockContext = new RockContext() )
            {
                // A null person is okay here as default values can still be returned.
                Person person = null;
                if ( registrantInfo != null && registrantInfo.PersonGuid.HasValue )
                {
                    person = new PersonService( rockContext ).Get( registrantInfo.PersonGuid.Value );
                }

                // If we already have a saved registrant get it, otherwise a null registrant will get any default values.
                var registrant = new RegistrationRegistrantService( rockContext ).Get( registrantGuid );

                // Load the group member for the registrant if there are any group member attribute form fields.
                // If the group member is not found, the default field values will be used.
                GroupMember groupMember = null;
                if ( forms.Any( form => form.Fields.Any( field => field.FieldSource == RegistrationFieldSource.GroupMemberAttribute ) ) )
                {
                    // Get the group member from the registrant if one has already been saved.
                    groupMember = registrant?.GroupMember;

                    // If the registrant or registrant's group membership has not been saved yet,
                    // try getting the group member for the registrant person.
                    if ( groupMember == null && person != null )
                    {
                        var groupId = GetRegistrationGroupId( rockContext, GetRegistrationInstanceId( rockContext ) );

                        if ( groupId.HasValue )
                        {
                            groupMember = new GroupMemberService( rockContext )
                                .GetByGroupIdAndPersonId( groupId.Value, person.Id )
                                .FirstOrDefault();
                        }
                    }
                }

                // Populate the field values
                foreach ( var form in forms )
                {
                    foreach ( var fieldViewModel in form.Fields )
                    {
                        // There are no default values for a PersonField, so skip those.
                        if ( fieldViewModel.FieldSource == RegistrationFieldSource.PersonField )
                        {
                            continue;
                        }

                        var field = new RegistrationTemplateFormFieldService( rockContext ).Get( fieldViewModel.Guid );

                        // Get the field values
                        if ( fieldViewModel.FieldSource == RegistrationFieldSource.PersonAttribute )
                        {
                            var personAttributeValue = GetEntityCurrentClientAttributeValue( rockContext, person, field );
                            fieldValues.TryAdd( fieldViewModel.Guid, personAttributeValue );
                        }
                        else if ( fieldViewModel.FieldSource == RegistrationFieldSource.RegistrantAttribute )
                        {
                            var registrantAttributeValue = GetEntityCurrentClientAttributeValue( rockContext, registrant, field );
                            fieldValues.TryAdd( fieldViewModel.Guid, registrantAttributeValue );
                        }
                        else if ( fieldViewModel.FieldSource == RegistrationFieldSource.GroupMemberAttribute )
                        {
                            var groupMemberAttributeValue = GetEntityCurrentClientAttributeValue( rockContext, groupMember, field );
                            fieldValues.TryAdd( fieldViewModel.Guid, groupMemberAttributeValue );
                        }
                    }
                }
            }

            return ActionOk( fieldValues );
        }

        /// <summary>
        /// Deletes the payment plan for the current registration.
        /// </summary>
        [BlockAction]
        public BlockActionResult DeletePaymentPlan()
        {
            using ( var rockContext = new RockContext() )
            {
                var context = GetContext( rockContext, out var getContextErrorMessage );

                if ( context is null )
                {
                    return ActionBadRequest( getContextErrorMessage.ToStringOrDefault( "An unknown error occurred while deleting the payment plan" ) );
                }
                else
                {
                    var session = GetRegistrationEntryBlockSession( rockContext, context );

                    if ( session is null )
                    {
                        return ActionBadRequest( "The registration session has expired" );
                    }
                    else if ( session.ActivePaymentPlan == null )
                    {
                        return ActionOk( "This registration has no payment plan or it has already been deleted" );
                    }
                    else
                    {
                        /* 
                           2024-5-7 JMH (copied from mdDeleteTransaction_SaveClick() in ScheduledTransactionListLiquid.ascx.cs)
              
                           2021-08-27 MDP
               
                           We really don't want to actually delete a FinancialScheduledTransaction.
                           Just inactivate it, even if there aren't FinancialTransactions associated with it.
                           It is possible the the Gateway has processed a transaction on it that Rock doesn't know about yet.
                           If that happens, Rock won't be able to match a record for that downloaded transaction!
                           We also might want to match inactive or "deleted" schedules on the Gateway to a person in Rock,
                           so we'll need the ScheduledTransaction to do that.

                           So, don't delete ScheduledTransactions.
                        */

                        // context.Registration is null here. Get the registration from the service.
                        IQueryable<Registration> registrationQuery = null;

                        if ( session.RegistrationGuid.HasValue )
                        {
                            registrationQuery = new RegistrationService( rockContext )
                                .Queryable()
                                .Where( r => r.Guid == session.RegistrationGuid.Value );
                        }
                        else if ( this.RegistrationIdPageParameter.HasValue )
                        {
                            registrationQuery = new RegistrationService( rockContext )
                                .Queryable()
                                .Where( r => r.Id == this.RegistrationIdPageParameter.Value );
                        }
                        else
                        {
                            registrationQuery = Enumerable.Empty<Registration>().AsQueryable();
                        }

                        var registrationData = registrationQuery
                            .Select( r => new
                            {
                                r.PaymentPlanFinancialScheduledTransactionId
                            } )
                            .FirstOrDefault();

                        if ( registrationData == null )
                        {
                            // This should not happen.
                            return ActionInternalServerError( "The registration could not be found" );
                        }
                        else
                        {
                            var financialScheduledTransactionId = registrationData.PaymentPlanFinancialScheduledTransactionId;

                            if ( !financialScheduledTransactionId.HasValue )
                            {
                                return ActionOk( "This registration has no payment plan or it has already been deleted" );
                            }
                            else
                            {
                                var financialScheduledTransactionService = new FinancialScheduledTransactionService( rockContext );
                                var financialScheduledTransaction = financialScheduledTransactionService.Get( registrationData.PaymentPlanFinancialScheduledTransactionId.Value );

                                if ( !financialScheduledTransactionService.Cancel( financialScheduledTransaction, out var cancelErrorMessage ) )
                                {
                                    return ActionInternalServerError( $"An error occurred while canceling your scheduled transaction on the financial gateway. Message: {cancelErrorMessage}" );
                                }
                                else
                                {
                                    try
                                    {
                                        if ( !financialScheduledTransactionService.GetStatus( financialScheduledTransaction, out var getStatusErrorMessage ) )
                                        {
                                            return ActionInternalServerError( $"The scheduled transaction was canceled on the financial gateway but was not marked inactive in Rock. Message: {getStatusErrorMessage}" );
                                        }
                                    }
                                    catch
                                    {
                                        // Ignore
                                    }

                                    rockContext.SaveChanges();
                                    return ActionOk( "The payment plan was deleted successfully" );
                                }
                            }
                        }
                    }
                }
            }
        }

        [BlockAction]
        public BlockActionResult GetScheduledPaymentDates( RegistrationEntryGetScheduledPaymentDatesRequestBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var context = GetContext( rockContext, out var errorMessage );
                if ( errorMessage.IsNotNullOrWhiteSpace() )
                {
                    return ActionBadRequest( errorMessage );
                }

                var scheduledTransactionFrequencyValueId = DefinedValueCache.GetId( bag.ScheduledTransactionFrequencyValueGuid );
                if ( !scheduledTransactionFrequencyValueId.HasValue )
                {
                    return ActionBadRequest( "Payment frequency is required" );
                }

                var financialGateway = new FinancialGatewayService( rockContext ).Get( context.RegistrationSettings.FinancialGatewayId ?? 0 );
                var gateway = financialGateway?.GetGatewayComponent();

                if ( gateway == null )
                {
                    // This will only occur if the gateway isn't configured on the registration template.
                    return ActionBadRequest( "Unable to get scheduled payment dates" );
                }

                var paymentDates = gateway.GetScheduledPaymentDates( scheduledTransactionFrequencyValueId.Value, bag.PaymentStartDate, bag.NumberOfPayments ) ?? new List<DateTime>();

                return ActionOk( paymentDates );
            }
        }

        #endregion Block Actions

        #region Methods

        /// <summary>
        /// Fixes registration arguments, such as, approximated decimal values sent by a browser.
        /// </summary>
        /// <param name="args"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void FixRegistrationArguments( RegistrationEntryArgsBag args )
        {
            // A browser may send approximated values;
            // e.g., 0.020000000000000004 (instead of 0.02) or 0.0699999999999932 (instead of 0.07).
            // Round currency amounts using the organization's currency settings.
            var currencyInfo = new RockCurrencyCodeInfo();

            args.AmountToPayNow = decimal.Round( args.AmountToPayNow, currencyInfo.DecimalPlaces );

            if ( args.PaymentPlan != null )
            {
                args.PaymentPlan.AmountPerPayment = decimal.Round( args.PaymentPlan.AmountPerPayment, currencyInfo.DecimalPlaces );
            }
        }

        /// <summary>
        /// Determines if a payment plan is valid.
        /// </summary>
        /// <param name="paymentPlan">The payment plan to validate.</param>
        /// <param name="errorMessage">The error message if validation fails.</param>
        /// <returns><see langword="true"/> if the payment plan is valid; otherwise, <see langword="false"/> (<paramref name="errorMessage"/> will contain the validation error message).</returns>
        private bool IsPaymentPlanValid( RegistrationEntryCreatePaymentPlanRequestBag paymentPlan, out string errorMessage )
        {
            if ( paymentPlan.TransactionFrequencyGuid == SystemGuid.DefinedValue.TRANSACTION_FREQUENCY_FIRST_AND_FIFTEENTH.AsGuid() )
            {
                bool IsAllowedStartDate( DateTime startDate )
                {
                    if ( startDate.Day == 1 || startDate.Day == 15 )
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                if ( !IsAllowedStartDate( paymentPlan.StartDate.Date ) )
                {
                    errorMessage = $"The payment plan start date {paymentPlan.StartDate:d} is invalid";
                    return false;
                }
            }

            errorMessage = null;
            return true;
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            using ( var rockContext = new RockContext() )
            {
                var registrationInstanceId = GetRegistrationInstanceId( rockContext, pageReference );
                var instanceName = new RegistrationInstanceService( rockContext )
                    .GetSelect( registrationInstanceId, ri => ri.Name );

                if ( instanceName.IsNotNullOrWhiteSpace() )
                {
                    return new BreadCrumbResult
                    {
                        BreadCrumbs = new List<IBreadCrumb>
                        {
                            new BreadCrumbLink( instanceName, pageReference )
                        }
                    };
                }

            }

            return null;
        }

        /// <inheritdoc/>
        protected override string GetPlaceholderContent( RockClientType clientType )
        {
            if ( clientType == RockClientType.Web )
            {
                return @"
   <div class=""skeleton skeleton-block w-lg mx-auto mb-4""></div>
   <div class=""skeleton skeleton-heading w-md mx-auto""></div>
   <div class=""skeleton skeleton-block w-sm mx-auto mt-5""></div>
   <div class=""skeleton skeleton-button ml-auto""></div>
";
            }

            return string.Empty;
        }

        /// <summary>
        /// Updates or Inserts the session.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="sessionStatus">The status to set the session to.</param>
        /// <param name="errorMessage">On exit will contain any error message.</param>
        /// <returns>The <see cref="RegistrationSession"/> or <c>null</c> if an error occurred.</returns>
        private RegistrationSession UpsertSession(
            RegistrationContext context,
            RegistrationEntryArgsBag args,
            SessionStatus sessionStatus,
            out string errorMessage )
        {
            var sessionData = new RegistrationEntrySessionBag
            {
                AmountToPayNow = args.AmountToPayNow,
                DiscountAmount = context.Discount?.RegistrationTemplateDiscount?.DiscountAmount ?? 0,
                DiscountCode = context.Discount?.RegistrationTemplateDiscount?.Code,
                DiscountPercentage = context.Discount?.RegistrationTemplateDiscount?.DiscountPercentage ?? 0,
                FieldValues = args.FieldValues,
                GatewayToken = args.GatewayToken,
                Registrants = args.Registrants,
                Registrar = args.Registrar,
                RegistrationGuid = context.Registration?.Guid,
                RegistrationSessionGuid = args.RegistrationSessionGuid,
                Slug = PageParameter( PageParameterKey.Slug ),
                GroupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull()
            };

            var nonWaitlistRegistrantCount = args.Registrants.Count( r => !r.IsOnWaitList );

            var registrationSession = RegistrationSessionService.CreateOrUpdateSession( args.RegistrationSessionGuid,
                // Create
                () => new RegistrationSession
                {
                    Guid = args.RegistrationSessionGuid,
                    RegistrationInstanceId = context.RegistrationSettings.RegistrationInstanceId,
                    RegistrationData = sessionData.ToJson(),
                    SessionStartDateTime = RockDateTime.Now,
                    RegistrationCount = nonWaitlistRegistrantCount,
                    RegistrationId = context.Registration?.Id,
                    SessionStatus = sessionStatus
                },
                // Update
                session =>
                {
                    session.RegistrationData = sessionData.ToJson();
                    session.SessionStatus = sessionStatus;
                    session.RegistrationCount = nonWaitlistRegistrantCount;
                },
                out errorMessage );

            return registrationSession;
        }

        /// <summary>
        /// Submits the registration.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="context">The context.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        /// <exception cref="Exception">There was a problem with the payment</exception>
        private Registration SubmitRegistration( RockContext rockContext, RegistrationContext context, RegistrationEntryArgsBag args, out string errorMessage )
        {
            // Ensure the arguments provided are in their proper format
            // before use (e.g. proper currency formatting of amounts).
            FixRegistrationArguments( args );

            /*
                8/15/2023 - JPH

                In order to successfully save the registration form values that were provided by the registrar, we must
                have each [RegistrationTemplateForm].[Fields] collection loaded into memory below. Several individuals have
                reported seeing missing registrant data within completed registrations, so it's possible that these Fields
                collections are somehow empty.

                The TryLoadMissingFields() method is a failsafe to ensure we have the data we need to properly save the
                registration. This method will:
                    1) Attempt to load any missing Fields collections;
                    2) Return a list of any Form IDs that were actually missing Fields so we can log them to prove that
                       this was a likely culprit for failed, past registration attempts (and so we can know to look into
                       the issue further from this angle).

                Reason: Registration entries are sometimes missing registration form data.
                https://github.com/SparkDevNetwork/Rock/issues/5091
             */
            var logInstanceOrTemplateName = context?.RegistrationSettings?.Name;
            var logCurrentPersonDetails = $"Current Person Name: {this.RequestContext.CurrentPerson?.FullName} (Person ID: {this.RequestContext.CurrentPerson?.Id});";
            var logMsgPrefix = $"Obsidian{( logInstanceOrTemplateName.IsNotNullOrWhiteSpace() ? $@" ""{logInstanceOrTemplateName}""" : string.Empty )} Registration; {logCurrentPersonDetails}{Environment.NewLine}";

            var (wereFieldsMissing, missingFieldsDetails) = new RegistrationTemplateFormService( rockContext ).TryLoadMissingFields( context?.RegistrationSettings?.Forms );
            if ( wereFieldsMissing )
            {
                var logMissingFieldsMsg = $"{logMsgPrefix}RegistrationTemplateForm(s) missing Fields data when trying to save Registration.{Environment.NewLine}{missingFieldsDetails}";

                ExceptionLogService.LogException( new RegistrationTemplateFormFieldException( logMissingFieldsMsg ) );
            }

            errorMessage = string.Empty;
            var currentPerson = GetCurrentPerson();

            var postSaveActions = new List<Action>();
            var registrationChanges = new History.HistoryChangeList();
            Person registrar = null;
            List<int> previousRegistrantPersonIds = null;
            var isNewRegistration = context.Registration == null;

            if ( isNewRegistration )
            {
                // This is a new registration
                context.Registration = new Registration
                {
                    RegistrationInstanceId = context.RegistrationSettings.RegistrationInstanceId,
                    RegistrationTemplateId = context.RegistrationSettings.RegistrationTemplateId
                };

                var registrationService = new RegistrationService( rockContext );
                registrationService.Add( context.Registration );
                registrationChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Registration" );

                if ( context.RegistrationSettings.RegistrarOption == RegistrarOption.UseLoggedInPerson && currentPerson != null )
                {
                    // Registrar is sometimes used with save operations later on
                    // so we need to load a new person that is in our RockContext.
                    // Fixes #5624.
                    registrar = new PersonService( rockContext ).Get( currentPerson.Id );
                    context.Registration.PersonAliasId = currentPerson.PrimaryAliasId;
                }
                else if ( context.RegistrationSettings.RegistrarOption == RegistrarOption.UseFirstRegistrant )
                {
                    var registrantInfo = args.Registrants.FirstOrDefault();

                    var firstName = GetPersonFieldValue( context.RegistrationSettings, RegistrationPersonFieldType.FirstName, registrantInfo.FieldValues ).ToStringSafe();
                    var lastName = GetPersonFieldValue( context.RegistrationSettings, RegistrationPersonFieldType.LastName, registrantInfo.FieldValues ).ToStringSafe();
                    var email = GetPersonFieldValue( context.RegistrationSettings, RegistrationPersonFieldType.Email, registrantInfo.FieldValues ).ToStringSafe();
                    var birthday = GetPersonFieldValue( context.RegistrationSettings, RegistrationPersonFieldType.Birthdate, registrantInfo.FieldValues ).ToStringSafe().FromJsonOrNull<BirthdayPickerBag>().ToDateTime();
                    var mobilePhone = GetPersonFieldValue( context.RegistrationSettings, RegistrationPersonFieldType.MobilePhone, registrantInfo.FieldValues ).ToStringSafe();
                    bool forceEmailUpdate = GetAttributeValue( AttributeKey.ForceEmailUpdate ).AsBoolean();

                    var personQuery = new PersonService.PersonMatchQuery( firstName, lastName, email, mobilePhone, gender: null, birthDate: birthday );

                    registrar = new PersonService( rockContext ).FindPerson( personQuery, forceEmailUpdate );
                    context.Registration.PersonAliasId = registrar?.PrimaryAliasId;
                }
            }
            else
            {
                // This is an existing registration
                registrar = context.Registration.PersonAlias.Person;

                var registrationService = new RegistrationService( rockContext );
                var previousRegistration = registrationService.Get( args.RegistrationGuid.Value );

                if ( previousRegistration != null )
                {
                    isNewRegistration = false;
                    previousRegistrantPersonIds = previousRegistration.Registrants
                        .Where( r => r.PersonAlias != null )
                        .Select( r => r.PersonAlias.PersonId )
                        .ToList();
                }
            }

            // Apply the registrar values to the registration record
            History.EvaluateChange( registrationChanges, "First Name", context.Registration.FirstName, args.Registrar.NickName );
            context.Registration.FirstName = args.Registrar.NickName;

            History.EvaluateChange( registrationChanges, "Last Name", context.Registration.LastName, args.Registrar.LastName );
            context.Registration.LastName = args.Registrar.LastName;

            History.EvaluateChange( registrationChanges, "Confirmation Email", context.Registration.ConfirmationEmail, args.Registrar.Email );
            context.Registration.ConfirmationEmail = args.Registrar.Email;

            History.EvaluateChange( registrationChanges, "Discount Code", context.Registration.DiscountCode, args.DiscountCode );
            context.Registration.DiscountCode = args.DiscountCode;

            /*
                8/29/2024 - JMH

                Discount handling logic is as follows:

                1. If a discount code is provided, use it.
                2. If not, check for an existing admin-applied discount in context.Registration.
                3. If neither is available, apply a 0 discount (no discount).
               
                https://github.com/SparkDevNetwork/Rock/issues/5691
                https://github.com/SparkDevNetwork/Rock/issues/5885
             */
            var discountPercentage = context.Discount?.RegistrationTemplateDiscount.DiscountPercentage ?? context.Registration?.DiscountPercentage ?? 0;
            History.EvaluateChange( registrationChanges, "Discount Percentage", context.Registration.DiscountPercentage, discountPercentage );
            context.Registration.DiscountPercentage = discountPercentage;

            var discountAmount = context.Discount?.RegistrationTemplateDiscount.DiscountAmount ?? context.Registration?.DiscountAmount ?? 0;
            History.EvaluateChange( registrationChanges, "Discount Amount", context.Registration.DiscountAmount, discountAmount );
            context.Registration.DiscountAmount = discountAmount;

            // If the registrar person record does not exist, try to find the record.
            if ( registrar == null )
            {
                registrar = GetExistingRegistrarPerson( context, currentPerson, rockContext );
            }

            // Load some attribute values about family roles and statuses

            // Get the connection status from the registration settings first.
            // If there is no connection status defined there, then attempt to get
            // it from this block's settings.
            var dvcConnectionStatusId = context.RegistrationSettings.ConnectionStatusValueId
                ?? DefinedValueCache.GetId( GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid() );
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

            if ( currentPerson?.PrimaryFamily != null )
            {
                multipleFamilyGroupIds.AddOrReplace( currentPerson.PrimaryFamily.Guid, currentPerson.PrimaryFamily.Id );
            }

            if ( !context.Registration.PersonAliasId.HasValue )
            {
                // If a match was not found, create a new person
                var person = new Person
                {
                    FirstName = context.Registration.FirstName,
                    LastName = context.Registration.LastName,
                    IsEmailActive = true,
                    Email = context.Registration.ConfirmationEmail,
                    EmailPreference = EmailPreference.EmailAllowed,
                    RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id,
                    ConnectionStatusValueId = dvcConnectionStatusId
                };

                if ( dvcRecordStatus != null )
                {
                    person.RecordStatusValueId = dvcRecordStatus.Id;
                }

                registrar = SavePerson(
                    rockContext,
                    context.RegistrationSettings,
                    person,
                    args.Registrar.FamilyGuid ?? Guid.NewGuid(),
                    campusId,
                    null, // location
                    adultRoleId,
                    childRoleId,
                    multipleFamilyGroupIds,
                    ref singleFamilyId );

                context.Registration.PersonAliasId = registrar != null ? registrar.PrimaryAliasId : ( int? ) null;
                History.EvaluateChange( registrationChanges, "Registrar", string.Empty, registrar.FullName );
            }
            else
            {
                if ( context.Registration.ConfirmationEmail.IsNotNullOrWhiteSpace() )
                {
                    var isEmailDifferent = !context.Registration.ConfirmationEmail.Trim().Equals( registrar.Email?.Trim(), StringComparison.OrdinalIgnoreCase );

                    var forceEmailUpdate = GetAttributeValue( AttributeKey.ForceEmailUpdate ).AsBoolean();

                    // Update the registrar's email if it has changed and either they
                    // requested it be updated or it is forced by the block settings.
                    if ( isEmailDifferent && ( forceEmailUpdate || args.Registrar.UpdateEmail ) )
                    {
                        var person = new PersonAliasService( rockContext ).GetPerson( context.Registration.PersonAliasId.Value );

                        if ( person != null )
                        {
                            person.Email = context.Registration.ConfirmationEmail;
                            rockContext.SaveChanges();
                        }
                    }
                }
            }

            // Determine the campus
            var registrarFamily = registrar.GetFamily( rockContext );
            campusId = campusId ?? registrarFamily.CampusId;

            // Set the family guid for any other registrants that were selected to be in the same family
            multipleFamilyGroupIds.TryAdd( registrarFamily.Guid, registrarFamily.Id );

            if ( !singleFamilyId.HasValue )
            {
                singleFamilyId = registrarFamily.Id;
            }

            // If the Registration Instance linkage specified a group, load it now
            var groupId = GetRegistrationGroupId( rockContext, context.Registration.RegistrationInstanceId );

            if ( groupId.HasValue )
            {
                var group = new GroupService( rockContext ).Get( groupId.Value );

                if ( group != null && ( !context.Registration.GroupId.HasValue || context.Registration.GroupId.Value != group.Id ) )
                {
                    context.Registration.GroupId = group.Id;
                    History.EvaluateChange( registrationChanges, "Group", string.Empty, group.Name );
                }
            }

            var linkage = GetRegistrationLinkage( rockContext, context.Registration.RegistrationInstanceId );

            if ( linkage?.CampusId.HasValue == true )
            {
                campusId = linkage.CampusId;
            }

            if ( campusId.HasValue )
            {
                context.Registration.CampusId = campusId;
                History.EvaluateChange( registrationChanges, "Campus", string.Empty, CampusCache.Get( ( int ) campusId ).Name );
            }

            // if this registration was marked as temporary (started from another page, then specified in the url), set IsTemporary to False now that we are done
            context.Registration.IsTemporary = false;

            // Set attribute values on the registration
            var registrationAttributes = GetRegistrationAttributes( context.RegistrationSettings.RegistrationTemplateId );
            context.Registration.LoadAttributes( rockContext );

            foreach ( var attribute in registrationAttributes )
            {
                var value = args.FieldValues.GetValueOrNull( attribute.Guid );
                var newValue = PublicAttributeHelper.GetPrivateValue( attribute, value.ToStringSafe() );
                context.Registration.SetAttributeValue( attribute.Key, newValue );
            }

            // Save the registration ( so we can get an id )
            rockContext.SaveChanges();
            context.Registration.SaveAttributeValues( rockContext );

            // Save the history
            Task.Run( () => HistoryService.SaveChanges(
                new RockContext(),
                typeof( Registration ),
                Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                context.Registration.Id,
                registrationChanges,
                true,
                currentPerson?.PrimaryAliasId ) );

            try
            {
                // Get each registrant
                var index = 0;

                // Keep track of the registered person IDs to prevent mistakenly merging different
                // people (i.e. twins who share an email address) into the same person record
                // based on an over-confident PersonService.FindPerson(...) match result.
                context.PersonIdsRegisteredWithinThisSession.Clear();

                /*
                    7/5/2024 - JMH

                    If max capacity is reached during registration, one or more registrants
                    may be put on the waitlist automatically.
                    Rock will automatically reduce the payment amount
                    for any **automatically** wait-listed registrants.
                 */
                var forceWaitlistedRegistrantGuids = new List<Guid>();

                foreach ( var registrantInfo in args.Registrants )
                {
                    var forceWaitlist = context.SpotsRemaining < 1 && ( isNewRegistration == true || registrantInfo.IsOnWaitList == true );
                    context.SpotsRemaining -= 1;

                    bool isCreatedAsRegistrant = context.RegistrationSettings.RegistrarOption == RegistrarOption.UseFirstRegistrant && registrantInfo == args.Registrants.FirstOrDefault();

                    MissingFieldsByFormId = new Dictionary<int, Dictionary<int, string>>();

                    UpsertRegistrant(
                        rockContext,
                        context,
                        registrar,
                        registrarFamily.Guid,
                        registrantInfo,
                        index,
                        multipleFamilyGroupIds,
                        ref singleFamilyId,
                        forceWaitlist,
                        isCreatedAsRegistrant,
                        isNewRegistration,
                        postSaveActions );

                    if ( forceWaitlist )
                    {
                        // Do this after upserting so the updated Guid can be used.
                        forceWaitlistedRegistrantGuids.Add( registrantInfo.Guid );
                    }

                    index++;

                    if ( MissingFieldsByFormId?.Any() == true )
                    {
                        /*
                            8/15/2023 - JPH

                            Several individuals have reported seeing missing registrant data within completed registrations. This registrant
                            is missing required, non-conditional Field value(s) that should have been enforced by the UI. Log an exception so
                            we know which values were missing during the saving of this registrant's data (and so we can know to look into
                            the issue further from this angle).

                            Reason: Registration entries are sometimes missing registration form data.
                            https://github.com/SparkDevNetwork/Rock/issues/5091
                         */
                        var logAllMissingFieldsSb = new StringBuilder();
                        logAllMissingFieldsSb.AppendLine( $"{logMsgPrefix}Registrant {index} of {args.Registrants.Count}: The following required (non-conditional) Field values were missing:" );

                        foreach ( var missingFormFields in MissingFieldsByFormId )
                        {
                            var logMissingFormFieldsSb = new StringBuilder( $"[Form ID: {missingFormFields.Key} -" );

                            foreach ( var missingField in missingFormFields.Value )
                            {
                                logMissingFormFieldsSb.Append( $" {missingField.Value} (Field ID: {missingField.Key});" );
                            }

                            logAllMissingFieldsSb.AppendLine( $"{logMissingFormFieldsSb}]" );
                        }

                        ExceptionLogService.LogException( new RegistrationTemplateFormFieldException( logAllMissingFieldsSb.ToString() ) );
                    }
                }

                rockContext.SaveChanges();

                var registrationCosts = GetRegistrationCosts( rockContext, context, args );
                var paymentReductionAmount = GetPaymentReductionAmountForForceWaitListedRegistrants( forceWaitlistedRegistrantGuids, registrationCosts );
                var minimumPaymentAmount = GetMinimumPaymentAmountForNonForceWaitListedRegistrants( rockContext, context, forceWaitlistedRegistrantGuids, registrationCosts );
                ReduceRegistrationPaymentAmount( context.RegistrationSettings, args, paymentReductionAmount, minimumPaymentAmount );

                var isPaymentNeededNow = args.AmountToPayNow > 0;
                var isPaymentPlanNeeded = args.PaymentPlan != null;

                if ( isPaymentNeededNow || isPaymentPlanNeeded )
                {
                    // Get basic payment processing data.
                    var financialGateway = new FinancialGatewayService( rockContext ).Get( context.RegistrationSettings.FinancialGatewayId ?? 0 );
                    var gateway = financialGateway?.GetGatewayComponent();
                    var financialAccount = new FinancialAccountService( rockContext ).Get( context.RegistrationSettings.FinancialAccountId ?? 0 );
                    if ( financialAccount == null )
                    {
                        errorMessage = "There was a problem with the financial account configuration for this registration instance";
                        return null;
                    }

                    // Keep track of the customer token so the customer account is only created once.
                    ReferencePaymentInfo oneTimePaymentInfo = null;
                    if ( isPaymentNeededNow )
                    {
                        oneTimePaymentInfo = GetPaymentInfo( rockContext, context, args, gateway, out errorMessage );
                        if ( errorMessage.IsNotNullOrWhiteSpace() )
                        {
                            throw new Exception( errorMessage );
                        }

                        // Process the payment in the gateway.
                        var financialTransaction = ProcessGatewayPayment( rockContext, context, args, financialGateway, gateway, oneTimePaymentInfo, out errorMessage );

                        if ( !errorMessage.IsNullOrWhiteSpace() )
                        {
                            throw new Exception( errorMessage );
                        }
                        else if ( financialTransaction == null )
                        {
                            throw new Exception( "There was a problem with the payment" );
                        }
                        else
                        {
                            // The payment was processed successfully, so save the transaction in Rock.
                            SaveTransaction( gateway, context, financialTransaction, oneTimePaymentInfo, rockContext );
                        }
                    }

                    if ( isPaymentPlanNeeded )
                    {
                        var paymentInfo = GetPaymentPlanPaymentInfo( rockContext, context, args, gateway, oneTimePaymentInfo, out errorMessage );
                        if ( errorMessage.IsNotNullOrWhiteSpace() )
                        {
                            throw new Exception( errorMessage );
                        }

                        // Create a schedule for the recurring payment plan transactions.
                        var paymentSchedule = new PaymentSchedule
                        {
                            PersonId = context.Registration.PersonId ?? 0,
                            NumberOfPayments = args.PaymentPlan.NumberOfPayments,
                            TransactionFrequencyValue = DefinedValueCache.Get( args.PaymentPlan.TransactionFrequencyGuid ),
                            StartDate = args.PaymentPlan.StartDate.Date,
                            // EndDate is not used for gateways that use payment plans created using number.
                        };

                        var scheduledTransaction = ProcessGatewayPaymentPlan( context, args, financialGateway, gateway, paymentSchedule, paymentInfo, out errorMessage );

                        if ( errorMessage.IsNotNullOrWhiteSpace() )
                        {
                            throw new Exception( errorMessage );
                        }

                        if ( scheduledTransaction == null )
                        {
                            throw new Exception( "There was a problem scheduling the payment" );
                        }

                        PrepareAndSavePaymentPlanScheduledTransaction( rockContext, context, financialGateway, gateway, paymentSchedule, paymentInfo, scheduledTransaction );
                    }
                }
            }
            catch ( Exception )
            {
                using ( var newRockContext = new RockContext() )
                {
                    // Cleanup any new records created since there was an error
                    if ( isNewRegistration )
                    {
                        var newRegistrationService = new RegistrationService( newRockContext );
                        var savedRegistration = new RegistrationService( newRockContext ).Get( context.Registration.Id );

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

            // Now that the registration is submitted, delete the session if any
            try
            {
                var registrationSessionService = new RegistrationSessionService( rockContext );
                var sessionToDeleteQuery = registrationSessionService.Queryable()
                    .Where( s => s.Guid == args.RegistrationSessionGuid );

                registrationSessionService.DeleteRange( sessionToDeleteQuery );
                rockContext.SaveChanges();
            }
            catch ( Exception e )
            {
                ExceptionLogService.LogException( e );
            }

            // If there is a valid registration, and nothing went wrong processing the payment, add registrants to group and send the notifications
            if ( context.Registration != null && !context.Registration.IsTemporary )
            {
                ProcessPostSave( rockContext, context.RegistrationSettings, args, isNewRegistration, context.Registration, previousRegistrantPersonIds, postSaveActions );
            }

            return context.Registration;
        }

        /// <summary>
        /// Gets the reduction amount for force wait-listed registrants.
        /// </summary>
        /// <param name="forceWaitListedRegistrantGuids">The registrants who were forcefully placed on the waitlist.</param>
        /// <param name="costs">The registration costs.</param>
        /// <returns>The amount that shouldn't be charged for any registrants who are forcefully placed on the waitlist.</returns>
        private static decimal GetPaymentReductionAmountForForceWaitListedRegistrants( IEnumerable<Guid> forceWaitListedRegistrantGuids, IEnumerable<RegistrationCostSummaryInfo> costs )
        {
            return costs
                .Where(
                    cost => cost.RegistrationRegistrantGuid.HasValue
                    && forceWaitListedRegistrantGuids.Contains( cost.RegistrationRegistrantGuid.Value )
                )
                .Sum( cost => cost.DiscountedCost );
        }

        /// <summary>
        /// Gets the registration costs.
        /// </summary>
        /// <param name="rockContext">The Rock context.</param>
        /// <param name="registrationContext">The registration context.</param>
        /// <param name="registrationArgs">The registration arguments.</param>
        /// <returns>The registration costs.</returns>
        private static List<RegistrationCostSummaryInfo> GetRegistrationCosts( RockContext rockContext, RegistrationContext registrationContext, RegistrationEntryArgsBag registrationArgs )
        {
            return new RegistrationInstanceService( rockContext )
                .GetRegistrationCostSummaryInfo( registrationContext, registrationArgs.AsArgsOrNull() );
        }

        /// <summary>
        /// Reduces the registration payment amount.
        /// </summary>
        /// <param name="registrationSettings">The registration settings.</param>
        /// <param name="registrationArgs">The registration arguments.</param>
        /// <param name="paymentReductionAmount">The payment reduction amount.</param>
        /// <param name="minimumPaymentAmount">The minimum payment amount.</param>
        private static void ReduceRegistrationPaymentAmount( RegistrationSettings registrationSettings, RegistrationEntryArgsBag registrationArgs, decimal paymentReductionAmount, decimal minimumPaymentAmount )
        {
            if ( paymentReductionAmount <= 0m || registrationSettings == null || registrationArgs == null )
            {
                return;
            }

            // Payment Plan
            if ( registrationArgs.PaymentPlan != null )
            {
                var paymentPlanAmount = registrationArgs.PaymentPlan.NumberOfPayments * registrationArgs.PaymentPlan.AmountPerPayment;

                if ( paymentPlanAmount > paymentReductionAmount )
                {
                    // Calculate a new payment plan with the new, reduced amount.
                    var newPaymentPlanAmount = paymentPlanAmount - paymentReductionAmount;
                    var paymentPlanConfigurationService = new PaymentPlanConfigurationService();
                    var options = new PaymentPlanConfigurationOptions
                    {
                        AmountForPaymentPlan = newPaymentPlanAmount,
                        CurrencyPrecision = new RockCurrencyCodeInfo().DecimalPlaces,
                        DesiredAllowedPaymentFrequencies = registrationSettings.PaymentPlanFrequencyValueIds?.Select( id => DefinedValueCache.Get( id ) ).ToList(),
                        DesiredNumberOfPayments = registrationArgs.PaymentPlan.NumberOfPayments,
                        DesiredPaymentFrequency = DefinedValueCache.Get( registrationArgs.PaymentPlan.TransactionFrequencyGuid ),
                        DesiredStartDate = registrationArgs.PaymentPlan.StartDate.Date,
                        EndDate = registrationSettings.PaymentDeadlineDate?.Date ?? RockDateTime.Today.AddYears( 1 ),
                        IsNumberOfPaymentsLimited = true,
                        MinNumberOfPayments = 2
                    };
                    var paymentPlanConfig = paymentPlanConfigurationService.Get( options );

                    // Overwrite the payment plan.
                    registrationArgs.PaymentPlan = new RegistrationEntryCreatePaymentPlanRequestBag
                    {
                        AmountPerPayment = paymentPlanConfig.AmountPerPayment,
                        NumberOfPayments = paymentPlanConfig.NumberOfPayments,
                        StartDate = paymentPlanConfig.StartDate,
                        TransactionFrequencyGuid = paymentPlanConfig.PaymentFrequencyConfiguration.PaymentFrequency.Guid,
                        TransactionFrequencyText = paymentPlanConfig.PaymentFrequencyConfiguration.PaymentFrequency.ToString(),
                    };

                    // Add remainder to the amount to pay today.
                    var remainderAmountNotCoveredByPlan = newPaymentPlanAmount - paymentPlanConfig.PlannedAmount;
                    registrationArgs.AmountToPayNow += remainderAmountNotCoveredByPlan;

                    // The entire reduction amount was used to reduce the payment plan amount.
                    paymentReductionAmount = 0m;
                }
                else
                {
                    // The reduction amount covers the entire payment plan
                    // and will cover some or all of the one-time payment amount.
                    // Remove the payment plan from the registration and process.
                    registrationArgs.PaymentPlan = null;
                    paymentReductionAmount -= paymentPlanAmount;
                }
            }
            
            // Amount To Pay Now
            if ( paymentReductionAmount > 0m
                 && registrationArgs.AmountToPayNow > 0m
                 && registrationArgs.AmountToPayNow > minimumPaymentAmount )
            {
                var newAmountToPayNow = registrationArgs.AmountToPayNow - paymentReductionAmount;

                if ( newAmountToPayNow < minimumPaymentAmount )
                {
                    // The amount to pay now cannot be less than
                    // the minimum required amount.
                    registrationArgs.AmountToPayNow = minimumPaymentAmount;

                    // Although there may be an reduction amount left over,
                    // the registrar is required to pay the minimum amount.
                    paymentReductionAmount = minimumPaymentAmount - newAmountToPayNow;
                }
                else
                {
                    registrationArgs.AmountToPayNow = newAmountToPayNow;
                    paymentReductionAmount = 0m;
                }
            }
        }

        /// <summary>
        /// Gets the minimum amount due today.
        /// </summary>
        /// <param name="rockContext">The Rock context.</param>
        /// <param name="registrationContext">The registration context.</param>
        /// <param name="forceWaitListedRegistrantGuids">The registrants who were forcefully placed on the waitlist.</param>
        /// <param name="registrationCosts">The registration costs.</param>
        /// <returns>The minimum amount due today.</returns>
        private static decimal GetMinimumPaymentAmountForNonForceWaitListedRegistrants( RockContext rockContext, RegistrationContext registrationContext, List<Guid> forceWaitlistedRegistrantGuids, IEnumerable<RegistrationCostSummaryInfo> registrationCosts )
        {
            var amountPaid = new RegistrationService( rockContext ).GetTotalPayments( registrationContext.Registration.Id );

            if ( amountPaid > 0 )
            {
                // No minimum if any payments have already been made.
                // This assumes that a minimum payment was received in a prior payment.
                return 0m;
            }
            else
            {
                // Otherwise, return the sum of all minimum payment (or discounted payment, if less) amounts for all costs.
                return registrationCosts
                    .Where(
                        cost => cost.RegistrationRegistrantGuid.HasValue
                        && forceWaitlistedRegistrantGuids.Contains( cost.RegistrationRegistrantGuid.Value )
                    )
                    .Sum( c => Math.Min( c.MinPayment, c.DiscountedCost ) );
            }
        }

        /// <summary>
        /// Gets the existing registrar person from the registration.
        /// </summary>
        /// <param name="context">The registration context.</param>
        /// <param name="currentPerson">The current person that is logged in.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="Person"/> that should be used as the registrant or <c>null</c> if unknown.</returns>
        private Person GetExistingRegistrarPerson( RegistrationContext context, Person currentPerson, RockContext rockContext )
        {
            /**
             * 1/26/2022 - DSH
             * 
             * Logic is as follows. If we have a logged in person and the name has
             * not been changed, then just use the current person as the registrar.
             * 
             * Otherwise (no logged in person or the name was changed), perform a
             * standard person match search to try to find an existing person.
             */
            bool currentPersonNamesMatch = false;
            Person registrar;

            if ( currentPerson != null )
            {
                var isFirstNameSame = currentPerson.NickName.Trim().Equals( context.Registration.FirstName, StringComparison.OrdinalIgnoreCase )
                    || currentPerson.FirstName.Trim().Equals( context.Registration.FirstName, StringComparison.OrdinalIgnoreCase );
                var isLastNameSame = currentPerson.LastName.Trim().Equals( context.Registration.LastName, StringComparison.OrdinalIgnoreCase );

                currentPersonNamesMatch = isFirstNameSame && isLastNameSame;
            }

            if ( currentPersonNamesMatch )
            {
                // Registrar is sometimes used with save operations later on
                // so we need to load a new person that is in our RockContext.
                // Fixes #5624.
                registrar = new PersonService( rockContext ).Get( currentPerson.Id );
                context.Registration.PersonAliasId = currentPerson.PrimaryAliasId;
            }
            else
            {
                var personService = new PersonService( rockContext );
                registrar = personService.FindPerson( context.Registration.FirstName, context.Registration.LastName, context.Registration.ConfirmationEmail, true );

                if ( registrar != null )
                {
                    context.Registration.PersonAliasId = registrar.PrimaryAliasId;
                }
                else
                {
                    registrar = null;
                    context.Registration.PersonAlias = null;
                    context.Registration.PersonAliasId = null;
                }
            }

            return registrar;
        }

        /// <summary>
        /// Gets the registration group identifier.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        /// <returns>The <see cref="Group"/> identifier or <c>null</c> if one is not available.</returns>
        private int? GetRegistrationGroupId( RockContext rockContext, int? registrationInstanceId )
        {
            var groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
            var registrationSlug = PageParameter( PageParameterKey.Slug );
            var eventOccurrenceId = this.EventOccurrenceIdPageParameter;

            if ( !groupId.HasValue )
            {
                if ( !registrationSlug.IsNullOrWhiteSpace() )
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
                else if ( eventOccurrenceId.HasValue && registrationInstanceId.HasValue )
                {
                    var linkageGroupId = new EventItemOccurrenceService( rockContext )
                        .Queryable()
                        .Where( o => o.Id == eventOccurrenceId.Value )
                        .SelectMany( o => o.Linkages )
                        .Where( l => l.RegistrationInstanceId == registrationInstanceId.Value )
                        .Select( l => l.GroupId )
                        .FirstOrDefault();

                    if ( linkageGroupId.HasValue )
                    {
                        groupId = linkageGroupId.Value;
                    }
                }
            }

            return groupId;
        }

        /// <summary>
        /// Gets the registration linkage.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="registrationInstanceId">The registration instance identifier.</param>
        /// <returns></returns>
        private EventItemOccurrenceGroupMap GetRegistrationLinkage( RockContext rockContext, int? registrationInstanceId )
        {
            var dateTime = RockDateTime.Now;
            var registrationSlug = PageParameter( PageParameterKey.Slug );
            var eventOccurrenceId = this.EventOccurrenceIdPageParameter;

            if ( !registrationSlug.IsNullOrWhiteSpace() )
            {
                return new EventItemOccurrenceGroupMapService( rockContext ?? new RockContext() )
                    .Queryable().AsNoTracking()
                    .Include( m => m.Campus )
                    .Where( l =>
                        l.UrlSlug == registrationSlug &&
                        l.RegistrationInstance != null &&
                        l.RegistrationInstance.IsActive &&
                        l.RegistrationInstance.RegistrationTemplate != null &&
                        l.RegistrationInstance.RegistrationTemplate.IsActive &&
                        ( !l.RegistrationInstance.StartDateTime.HasValue || l.RegistrationInstance.StartDateTime <= dateTime ) &&
                        ( !l.RegistrationInstance.EndDateTime.HasValue || l.RegistrationInstance.EndDateTime > dateTime ) )
                    .FirstOrDefault();
            }
            else if ( eventOccurrenceId.HasValue && registrationInstanceId.HasValue )
            {
                return new EventItemOccurrenceGroupMapService( rockContext ?? new RockContext() )
                    .Queryable().AsNoTracking()
                    .Include( m => m.Campus )
                    .Where( l =>
                        l.EventItemOccurrence.Id == eventOccurrenceId &&
                        l.RegistrationInstanceId == registrationInstanceId.Value &&
                        l.RegistrationInstance.IsActive &&
                        l.RegistrationInstance.RegistrationTemplate != null &&
                        l.RegistrationInstance.RegistrationTemplate.IsActive &&
                        ( !l.RegistrationInstance.StartDateTime.HasValue || l.RegistrationInstance.StartDateTime <= dateTime ) &&
                        ( !l.RegistrationInstance.EndDateTime.HasValue || l.RegistrationInstance.EndDateTime > dateTime ) )
                    .FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// Gets a person field value.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="personFieldType">Type of the person field.</param>
        /// <returns></returns>
        private object GetPersonFieldValue( RegistrationSettings settings, RegistrationPersonFieldType personFieldType, Dictionary<Guid, object> fieldValues )
        {
            if ( settings == null || settings.Forms == null )
            {
                return null;
            }

            var fieldGuid = settings.Forms
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
            var registrationInstanceId = GetRegistrationInstanceId( rockContext );
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
        /// Gets the first name of the registrant.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="registrantInfo">The registrant information.</param>
        /// <returns></returns>
        private string GetRegistrantFirstName( RegistrationContext context, ViewModels.Blocks.Event.RegistrationEntry.RegistrantBag registrantInfo )
        {
            var fields = context.RegistrationSettings.Forms.SelectMany( f => f.Fields );
            var field = fields.FirstOrDefault( f => f.PersonFieldType == RegistrationPersonFieldType.FirstName );
            return registrantInfo.FieldValues.GetValueOrNull( field.Guid ).ToStringSafe();
        }

        /// <summary>
        /// Gets the last name of the registrant.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="registrantInfo">The registrant information.</param>
        /// <returns></returns>
        private string GetRegistrantLastName( RegistrationContext context, ViewModels.Blocks.Event.RegistrationEntry.RegistrantBag registrantInfo )
        {
            var fields = context.RegistrationSettings.Forms.SelectMany( f => f.Fields );
            var field = fields.FirstOrDefault( f => f.PersonFieldType == RegistrationPersonFieldType.LastName );
            return registrantInfo.FieldValues.GetValueOrNull( field.Guid ).ToStringSafe();
        }

        /// <summary>
        /// Gets the last name of the registrant.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="registrantInfo">The registrant information.</param>
        /// <returns></returns>
        private string GetRegistrantFullName( RegistrationContext context, ViewModels.Blocks.Event.RegistrationEntry.RegistrantBag registrantInfo )
        {
            var firstName = GetRegistrantFirstName( context, registrantInfo );
            var lastName = GetRegistrantLastName( context, registrantInfo );
            return $"{firstName} {lastName}";
        }

        /// <summary>
        /// Gets the field values.
        /// </summary>
        /// <param name="registrationContext">The registration context.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="person">The person.</param>
        /// <param name="registrant">The registrant to use when retrieving registrant attribute values..</param>
        /// <param name="forms">The forms.</param>
        /// <param name="forcePersonValues"><c>true</c> if person field values should always be retrieved from the Person.</param>
        /// <returns></returns>
        private Dictionary<Guid, object> GetCurrentValueFieldValues( RegistrationContext registrationContext, RockContext rockContext, Person person, RegistrationRegistrant registrant, IEnumerable<RegistrationTemplateForm> forms, bool forcePersonValues )
        {
            var fieldValues = new Dictionary<Guid, object>();
            var familySelection = registrationContext?.RegistrationSettings.AreCurrentFamilyMembersShown ?? true;

            foreach ( var form in forms )
            {
                var fields = form.Fields.Where( f =>
                {
                    // ShowCurrentValue means "Default to the person's current value for this field"
                    if ( ( f.ShowCurrentValue || f.IsLockedIfValuesExist ) && !f.IsInternal && ( f.Attribute == null || f.Attribute.IsActive ) )
                    {
                        return true;
                    }

                    // If we are returning to an existing registration then we
                    // want to always pull in the current data from the Person.
                    // Otherwise when returning to an existing registration some
                    // field values (such as person attributes and First/Last name
                    // among others) will be blank.
                    if ( forcePersonValues && ( f.FieldSource == RegistrationFieldSource.PersonField || f.FieldSource == RegistrationFieldSource.PersonAttribute ) )
                    {
                        return true;
                    }

                    if ( ( familySelection || f.ShowCurrentValue || f.IsLockedIfValuesExist ) && f.FieldSource == RegistrationFieldSource.PersonField )
                    {
                        return f.PersonFieldType == RegistrationPersonFieldType.FirstName || f.PersonFieldType == RegistrationPersonFieldType.LastName;
                    }

                    if ( f.FieldSource == RegistrationFieldSource.RegistrantAttribute )
                    {
                        return true;
                    }

                    return false;
                } );

                foreach ( var field in fields )
                {
                    var value = GetCurrentFieldValue( rockContext, person, registrant, field, registrationContext );

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
        /// <param name="registrant">The registrant to use when retrieving registrant attribute values..</param>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        private object GetCurrentFieldValue( RockContext rockContext, Person person, RegistrationRegistrant registrant, RegistrationTemplateFormField field, RegistrationContext registrationContext )
        {
            switch ( field.FieldSource )
            {
                case RegistrationFieldSource.PersonField:
                    return GetPersonCurrentFieldValue( rockContext, person, field, registrationContext );

                case RegistrationFieldSource.PersonAttribute:
                    return GetEntityCurrentClientAttributeValue( rockContext, person, field );

                case RegistrationFieldSource.RegistrantAttribute:
                    return GetEntityCurrentClientAttributeValue( rockContext, registrant, field );

                case RegistrationFieldSource.GroupMemberAttribute:
                    return GetGroupMemberCurrentAttributeValue( rockContext, person, field, registrationContext );
            }

            return null;
        }

        private object GetGroupMemberCurrentAttributeValue( RockContext rockContext, Person person, RegistrationTemplateFormField field, RegistrationContext registrationContext )
        {
            if ( person == null )
            {
                return null;
            }

            var groupId = GetRegistrationGroupId( rockContext, registrationContext?.RegistrationSettings?.RegistrationInstanceId );

            if ( !groupId.HasValue )
            {
                return null;
            }

            var groupMember = new GroupMemberService( rockContext )
                .GetByGroupIdAndPersonId( groupId.Value, person.Id )
                .FirstOrDefault();

            if ( groupMember == null )
            {
                return null;
            }

            return GetEntityCurrentClientAttributeValue( rockContext, groupMember, field );
        }

        /// <summary>
        /// Gets the current person field value.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="person">The person.</param>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        private object GetPersonCurrentFieldValue( RockContext rockContext, Person person, RegistrationTemplateFormField field, RegistrationContext registrationContext )
        {
            if ( person == null )
            {
                return null;
            }

            switch ( field.PersonFieldType )
            {
                case RegistrationPersonFieldType.FirstName:
                    return person.FirstName;

                case RegistrationPersonFieldType.LastName:
                    return person.LastName;

                case RegistrationPersonFieldType.MiddleName:
                    return person.MiddleName;

                case RegistrationPersonFieldType.Email:
                    return person.Email;

                case RegistrationPersonFieldType.Campus:
                    var family = person.GetFamily( rockContext );
                    return family?.Campus?.Guid;

                case RegistrationPersonFieldType.Gender:
                    return person.Gender.ConvertToInt().ToString();

                case RegistrationPersonFieldType.Birthdate:
                    return new BirthdayPickerBag
                    {
                        Year = person.BirthYear ?? 0,
                        Month = person.BirthMonth ?? 0,
                        Day = person.BirthDay ?? 0
                    };

                case RegistrationPersonFieldType.AnniversaryDate:
                    return new BirthdayPickerBag
                    {
                        Year = person.AnniversaryDate?.Year ?? 0,
                        Month = person.AnniversaryDate?.Month ?? 0,
                        Day = person.AnniversaryDate?.Day ?? 0
                    };

                case RegistrationPersonFieldType.Address:
                    var location = person.GetHomeLocation( rockContext );

                    return new AddressControlBag
                    {
                        Street1 = location?.Street1 ?? string.Empty,
                        Street2 = location?.Street2 ?? string.Empty,
                        City = location?.City ?? string.Empty,
                        State = location?.State ?? string.Empty,
                        PostalCode = location?.PostalCode ?? string.Empty,
                        Country = location?.Country ?? string.Empty
                    };

                case RegistrationPersonFieldType.MaritalStatus:
                    {
                        var maritalStatus = person.MaritalStatusValueId.HasValue ? DefinedValueCache.Get( person.MaritalStatusValueId.Value ) : null;

                        return maritalStatus?.Guid.ToString() ?? string.Empty;
                    }

                case RegistrationPersonFieldType.ConnectionStatus:
                    {
                        var connectionStatus = person.ConnectionStatusValueId.HasValue ? DefinedValueCache.Get( person.ConnectionStatusValueId.Value ) : null;

                        return connectionStatus?.Guid.ToString() ?? string.Empty;
                    }

                case RegistrationPersonFieldType.Grade:
                    {
                        var gradeOffset = person.GradeOffset ?? -1;
                        var gradeValue = DefinedTypeCache.Get( SystemGuid.DefinedType.SCHOOL_GRADES )
                            .DefinedValues
                            .FirstOrDefault( v => v.Value == gradeOffset.ToString() );

                        return gradeValue?.Guid.ToString() ?? string.Empty;
                    }

                case RegistrationPersonFieldType.HomePhone:
                    var homePhone = person.GetPhoneNumber( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() );
                    return CreatePhoneNumberBoxWithSmsControlBag( homePhone );

                case RegistrationPersonFieldType.WorkPhone:
                    var workPhone = person.GetPhoneNumber( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() );
                    return CreatePhoneNumberBoxWithSmsControlBag( workPhone );

                case RegistrationPersonFieldType.MobilePhone:
                    var mobilePhone = person.GetPhoneNumber( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
                    return CreatePhoneNumberBoxWithSmsControlBag( mobilePhone );

                case RegistrationPersonFieldType.Race:
                    var race = person.RaceValueId.HasValue ? DefinedValueCache.Get( person.RaceValueId.Value ) : null;
                    return race?.Guid.ToString() ?? string.Empty;

                case RegistrationPersonFieldType.Ethnicity:
                    var ethnicity = person.EthnicityValueId.HasValue ? DefinedValueCache.Get( person.EthnicityValueId.Value ) : null;
                    return ethnicity?.Guid.ToString() ?? string.Empty;
            }

            return null;
        }

        private PhoneNumberBoxWithSmsControlBag CreatePhoneNumberBoxWithSmsControlBag( Rock.Model.PhoneNumber phone )
        {
            if ( phone == null )
            {
                return new PhoneNumberBoxWithSmsControlBag
                {
                    Number = string.Empty,
                    IsMessagingEnabled = false,
                    CountryCode = string.Empty
                };
            }

            return new PhoneNumberBoxWithSmsControlBag
            {
                Number = phone.Number,
                IsMessagingEnabled = phone.IsMessagingEnabled,
                CountryCode = phone.CountryCode
            };
        }

        /// <summary>
        /// Gets the entity's attribute value.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        private string GetEntityCurrentClientAttributeValue( RockContext rockContext, IHasAttributes entity, RegistrationTemplateFormField field )
        {
            var attribute = AttributeCache.Get( field.AttributeId ?? 0 );

            if ( attribute is null )
            {
                return null;
            }

            if ( entity == null )
            {
                return PublicAttributeHelper.GetPublicValueForEdit( attribute, attribute.DefaultValue );
            }

            entity.LoadAttributes( rockContext );

            return PublicAttributeHelper.GetPublicValueForEdit( attribute, entity.GetAttributeValue( attribute.Key ) );
        }

        /// <summary>
        /// Saves the person.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="settings">The registration settings.</param>
        /// <param name="person">The person.</param>
        /// <param name="familyGuid">The family unique identifier.</param>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="location">The location.</param>
        /// <param name="adultRoleId">The adult role identifier.</param>
        /// <param name="childRoleId">The child role identifier.</param>
        /// <param name="multipleFamilyGroupIds">The multiple family group ids.</param>
        /// <param name="singleFamilyId">The single family identifier.</param>
        /// <param name="updateExistingCampus">if set to <c>true</c> updates the existing campus for the family group to the one provided in the campusId parameter.</param>
        /// <returns>Person.</returns>
        private Person SavePerson( RockContext rockContext, RegistrationSettings settings, Person person, Guid familyGuid, int? campusId, Location location, int adultRoleId, int childRoleId, Dictionary<Guid, int> multipleFamilyGroupIds, ref int? singleFamilyId, bool updateExistingCampus = false )
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
                    multipleFamilyGroupIds.TryAdd( familyGuid, family.Id );
                    if ( !singleFamilyId.HasValue )
                    {
                        singleFamilyId = family.Id;
                    }
                }
            }
            else
            {
                if ( !settings.ActualRecordSourceValueId.HasValue )
                {
                    settings.ActualRecordSourceValueId = RecordSourceHelper.GetSessionRecordSourceValueId()
                        ?? settings.RecordSourceValueId
                        ?? DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.RECORD_SOURCE_TYPE_EVENT_REGISTRATION.AsGuid() )?.Id;
                }

                // Assign the record source for new people.
                person.RecordSourceValueId = settings.ActualRecordSourceValueId;

                // If we've created the family already for this registrant, add them to it
                if (
                        ( settings.RegistrantsSameFamily == RegistrantsSameFamily.Ask && multipleFamilyGroupIds.ContainsKey( familyGuid ) ) ||
                        ( settings.RegistrantsSameFamily == RegistrantsSameFamily.Yes && singleFamilyId.HasValue )
                    )
                {
                    // Add person to existing family
                    var age = person.Age;
                    int familyRoleId = age.HasValue && age < 18 ? childRoleId : adultRoleId;

                    familyId = settings.RegistrantsSameFamily == RegistrantsSameFamily.Ask ?
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
                        multipleFamilyGroupIds.TryAdd( familyGuid, familyGroup.Id );
                        if ( !singleFamilyId.HasValue )
                        {
                            singleFamilyId = familyGroup.Id;
                        }
                    }
                }
            }

            // If we have family ID and a meaningful location then update that info
            if ( familyId.HasValue )
            {
                var familyGroup = new GroupService( rockContext ).Get( familyId.Value );

                if ( campusId.HasValue && ( updateExistingCampus || !familyGroup.CampusId.HasValue ) )
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
            string phoneNumber = string.Empty;
            string countryCode = string.Empty;
            bool? isMessagingEnabled = null;

            var phoneData = fieldValue.ToStringSafe().FromJsonOrNull<PhoneNumberBoxWithSmsControlBag>();
            if ( phoneData != null )
            {
                // We got the number and SMS selection, so set both.
                phoneNumber = phoneData.Number;
                isMessagingEnabled = phoneData.IsMessagingEnabled;
                countryCode = phoneData.CountryCode;
            }
            else if ( fieldValue is string )
            {
                // Only got the number, so leave IsMessagingEnabled null so it isn't changed
                phoneNumber = fieldValue.ToStringSafe();
            }
            else
            {
                // No usable data, just return without doing anything.
                return;
            }

            string cleanNumber = PhoneNumber.CleanNumber( phoneNumber );
            var numberType = DefinedValueCache.Get( phoneTypeGuid );

            if ( string.IsNullOrWhiteSpace( cleanNumber ) || numberType == null )
            {
                return;
            }

            string oldPhoneNumber = string.Empty;
            bool oldIsMessagingEnabled = false;
            var phone = person.PhoneNumbers.FirstOrDefault( p => p.NumberTypeValueId == numberType.Id );

            if ( phone == null )
            {
                phone = new PhoneNumber
                {
                    NumberTypeValueId = numberType.Id
                };

                person.PhoneNumbers.Add( phone );
            }
            else
            {
                oldPhoneNumber = phone.NumberFormattedWithCountryCode;
                oldIsMessagingEnabled = phone.IsMessagingEnabled;
            }

            phone.Number = cleanNumber;
            phone.CountryCode = countryCode;

            History.EvaluateChange( changes, $"{numberType.Value} Phone", oldPhoneNumber, phone.NumberFormattedWithCountryCode );

            if ( isMessagingEnabled != null )
            {
                phone.IsMessagingEnabled = isMessagingEnabled.Value;
                History.EvaluateChange( changes, $"{numberType.Value} IsMessagingEnabled", oldIsMessagingEnabled, phone.IsMessagingEnabled );
            }
        }

        /// <summary>
        /// Gets an existing person if possible, otherwise creates a new Person object
        /// which can be later saved to the database.
        /// </summary>
        /// <param name="context">The registration context.</param>
        /// <param name="registrantInfo">The registrant information.</param>
        /// <param name="registrar">The registrar person that is performing the registering.</param>
        /// <param name="registrarFamilyGuid">The registrar family unique identifier.</param>
        /// <param name="isCreatedAsRegistrant">if set to <c>true</c> [is created as registrant].</param>
        /// <param name="rockContext">The rock context for any database lookups.</param>
        /// <returns>A tuple that contains the <see cref="Person" /> object and the optional <see cref="RegistrationRegistrant" /> object.</returns>
        private (Person person, RegistrationRegistrant registrant) GetExistingOrCreatePerson( RegistrationContext context, ViewModels.Blocks.Event.RegistrationEntry.RegistrantBag registrantInfo, Person registrar, Guid registrarFamilyGuid, bool isCreatedAsRegistrant, RockContext rockContext )
        {
            RegistrationRegistrant registrant = null;
            Person person = null;
            var personService = new PersonService( rockContext );

            var firstName = GetPersonFieldValue( context.RegistrationSettings, RegistrationPersonFieldType.FirstName, registrantInfo.FieldValues ).ToStringSafe();
            var lastName = GetPersonFieldValue( context.RegistrationSettings, RegistrationPersonFieldType.LastName, registrantInfo.FieldValues ).ToStringSafe();
            var email = GetPersonFieldValue( context.RegistrationSettings, RegistrationPersonFieldType.Email, registrantInfo.FieldValues ).ToStringSafe();
            var birthday = GetPersonFieldValue( context.RegistrationSettings, RegistrationPersonFieldType.Birthdate, registrantInfo.FieldValues ).ToStringSafe().FromJsonOrNull<BirthdayPickerBag>().ToDateTime();
            var mobilePhone = GetPersonFieldValue( context.RegistrationSettings, RegistrationPersonFieldType.MobilePhone, registrantInfo.FieldValues ).ToStringSafe();

            /*
                8/15/2023 - JPH

                Several individuals have reported seeing missing registrant data within completed registrations. Check
                each person field type to see if it was required, non-conditional & missing, so we know whether to look
                into the issue further from this angle.

                Reason: Registration entries are sometimes missing registration form data.
                https://github.com/SparkDevNetwork/Rock/issues/5091
            */
            if ( MissingFieldsByFormId != null )
            {
                void NotePersonFieldDetailsIfRequiredAndMissing( RegistrationPersonFieldType personFieldType, object fieldValue )
                {
                    var field = context.RegistrationSettings
                        ?.Forms
                        ?.SelectMany( f => f.Fields
                            .Where( ff =>
                                ff.FieldSource == RegistrationFieldSource.PersonField
                                && ff.PersonFieldType == personFieldType
                            )
                        ).FirstOrDefault();

                    if ( field == null )
                    {
                        return;
                    }

                    field.NoteFieldDetailsIfRequiredAndMissing( MissingFieldsByFormId, fieldValue );
                }

                NotePersonFieldDetailsIfRequiredAndMissing( RegistrationPersonFieldType.FirstName, firstName );
                NotePersonFieldDetailsIfRequiredAndMissing( RegistrationPersonFieldType.LastName, lastName );
                NotePersonFieldDetailsIfRequiredAndMissing( RegistrationPersonFieldType.Email, email );
                NotePersonFieldDetailsIfRequiredAndMissing( RegistrationPersonFieldType.Birthdate, birthday );
                NotePersonFieldDetailsIfRequiredAndMissing( RegistrationPersonFieldType.MobilePhone, mobilePhone );
            }

            registrant = context.Registration.Registrants.FirstOrDefault( r => r.Guid == registrantInfo.Guid );

            if ( registrant != null )
            {
                person = registrant.Person;
                if ( person != null )
                {
                    // If the form has first or last name fields and they have data then match the registrant.Person with the form values.
                    // If the form values are blank then this is an existing registration and a payment is being made and we do not want to null out the registrant(s).
                    var firstNameMatch = firstName.IsNullOrWhiteSpace() ? true : ( registrant.Person.FirstName.Equals( firstName, StringComparison.OrdinalIgnoreCase ) || registrant.Person.NickName.Equals( firstName, StringComparison.OrdinalIgnoreCase ) );
                    var lastNameMatch = lastName.IsNullOrWhiteSpace() ? true : registrant.Person.LastName.Equals( lastName, StringComparison.OrdinalIgnoreCase );

                    if ( firstNameMatch && lastNameMatch )
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
            }
            else if ( registrantInfo.PersonGuid.HasValue )
            {
                // This can happen if the page has reloaded due to an error. The person was saved to the DB and we don't want to add them again.
                person = personService.Get( registrantInfo.PersonGuid.Value );
            }
            else
            {
                if ( registrantInfo.PersonGuid.HasValue && context.RegistrationSettings.AreCurrentFamilyMembersShown )
                {
                    person = personService.Get( registrantInfo.PersonGuid.Value );
                }
            }

            if ( person == null )
            {
                // Try to find a matching person based on name, email address, mobile phone, and birthday. If these were not provided they are not considered.
                var personQuery = new PersonService.PersonMatchQuery( firstName, lastName, email, mobilePhone, gender: null, birthDate: birthday );
                person = personService.FindPerson( personQuery, true );

                if ( person != null && context.PersonIdsRegisteredWithinThisSession.Contains( person.Id ) )
                {
                    /*
                        1/8/2024 - JPH

                        We've seen scenarios in which different people (i.e. twins who share an email address) are
                        mistakenly merged into a single person record because of the way our FindPerson(...) method
                        works. Rock is correctly attempting to prevent the creation of duplicate person records,
                        but we need to handle this unique scenario by instead keeping track of the person IDs that
                        have already been tied to a registrant record within this specific registration session,
                        and if the FindPerson(...) method returns the same person more than once, we'll force Rock
                        to create a new person record, at the risk of creating duplicate people. This risk is more
                        tolerable than the risk of failing to save a Person altogether, as in the twin example above.

                        Reason: Attempt to prevent merging different people based on an over-confident match result.
                    */
                    person = null;
                }

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

            /*
                 4/26/2024 - JMH
                
                 If a person match was not made for the registrant at this point,
                 and if the registrar is not the authenticated person,
                 then try to find a match from the authenticated person's family.

                 Duplicate prevention may be enhanced in the future to include suffix
                 or other identifying information.
            */
            var currentPerson = GetCurrentPerson();
            if ( person == null && currentPerson != null && registrar?.PrimaryAliasId != currentPerson.PrimaryAliasId )
            {
                var familyMembers = currentPerson.GetFamilyMembers( true, rockContext )
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
            }

            if ( person == null )
            {
                /**
                  * 06/07/2022 - KA
                  * 
                  * Logic is as follows. If the Template RegistrarOption was set to UseFirstRegistrant
                  * then chances are a Person was created or found for the first Registrant and used
                  * as the Registrar. In that case then we don't create a new Person for the first
                  * Registrant. Otherwise we go ahead and create a new Person. This is of Particular
                  * importance when the AccountProtectionProfilesForDuplicateDetectionToIgnore includes
                  * AccountProtectionProfile.Low. That means the PersonMatch query will return a null
                  * any time it is called. This prevents us from creating duplicate Person entities for
                  * both the Registrar and first Registrant who are the same person in this scenario.
                */
                if ( isCreatedAsRegistrant && registrar != null )
                {
                    person = registrar;
                }
                else
                {
                    // Get the connection status from the registration settings first.
                    // If there is no connection status defined there, then attempt to get
                    // it from this block's settings.
                    var dvcConnectionStatusId = context.RegistrationSettings.ConnectionStatusValueId
                        ?? DefinedValueCache.GetId( GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid() );
                    var dvcRecordStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordStatus ).AsGuid() );

                    // If a match was not found, create a new person
                    person = new Person();
                    person.FirstName = firstName;
                    person.LastName = lastName;
                    person.IsEmailActive = true;
                    person.Email = email;
                    person.EmailPreference = EmailPreference.EmailAllowed;
                    person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;

                    if ( dvcConnectionStatusId.HasValue )
                    {
                        person.ConnectionStatusValueId = dvcConnectionStatusId.Value;
                    }

                    if ( dvcRecordStatus != null )
                    {
                        person.RecordStatusValueId = dvcRecordStatus.Id;
                    }
                }
            }

            return (person, registrant);
        }

        /// <summary>
        /// Determines if a field is unlocked for editing.
        /// </summary>
        /// <param name="field">The field to check.</param>
        /// <param name="currentFieldValue">The current value of the field.</param>
        /// <returns><see langword="true"/> if the field is unlocked; otherwise <see langword="false"/>.</returns>
        private bool IsFieldUnlockedForEditing( RegistrationTemplateFormField field, string currentFieldValue )
        {
            // The field can be updated if it is not "locked" or if it doesn't have a value.
            return !field.IsLockedIfValuesExist || currentFieldValue.IsNullOrWhiteSpace();
        }

        /// <summary>
        /// Determines if a field is unlocked for editing.
        /// </summary>
        /// <param name="field">The field to check.</param>
        /// <param name="currentFieldValue">The current value of the field.</param>
        /// <returns><see langword="true"/> if the field is unlocked; otherwise <see langword="false"/>.</returns>
        private bool IsFieldUnlockedForEditing<T>( RegistrationTemplateFormField field, T? currentFieldValue ) where T : struct
        {
            // The field can be updated if it is not "locked" or if it doesn't have a value.
            return !field.IsLockedIfValuesExist || !currentFieldValue.HasValue;
        }

        /// <summary>
        /// Determines if a field is unlocked for editing.
        /// </summary>
        /// <param name="field">The field to check.</param>
        /// <param name="currentFieldValue">The current value of the field.</param>
        /// <returns><see langword="true"/> if the field is unlocked; otherwise <see langword="false"/>.</returns>
        private bool IsFieldUnlockedForEditing( RegistrationTemplateFormField field, object currentFieldValue )
        {
            // The field can be updated if it is not "locked" or if it doesn't have a value.
            return !field.IsLockedIfValuesExist || currentFieldValue == null;
        }

        /// <summary>
        /// Determines if a field is unlocked for editing.
        /// </summary>
        /// <param name="field">The field to check.</param>
        /// <param name="currentFieldValue">The current value of the field.</param>
        /// <returns><see langword="true"/> if the field is unlocked; otherwise <see langword="false"/>.</returns>
        private bool IsFieldUnlockedForEditing( RegistrationTemplateFormField field, Gender currentFieldValue )
        {
            // The field can be updated if it is not "locked" or if it doesn't have a value.
            return !field.IsLockedIfValuesExist || currentFieldValue == Gender.Unknown;
        }

        /// <summary>
        /// Determines if a field is unlocked for editing.
        /// </summary>
        /// <param name="field">The field to check.</param>
        /// <param name="entity">The entity potentially containing the field's current attribute value.</param>
        /// <param name="attributeKey">The key of the attribute to check.</param>
        /// <returns><see langword="true"/> if the field is unlocked; otherwise <see langword="false"/>.</returns>
        private bool IsFieldUnlockedForEditing( RegistrationTemplateFormField field, IHasAttributes entity, string attributeKey )
        {
            // The field can be updated if it is not "locked" or if it doesn't have a value.
            if ( !field.IsLockedIfValuesExist )
            {
                return true;
            }

            // Check if the entity doesn't have an attribute value.
            return entity?.AttributeValues?.ContainsKey( attributeKey ) != true
                || entity.AttributeValues[attributeKey].Value.IsNullOrWhiteSpace();
        }

        /// <summary>
        /// Updates the person object from information provided in the registrant.
        /// This does not perform the SaveChanges() call, so all changes are made
        /// only to the in-memory object.
        /// </summary>
        /// <param name="person">The person to update.</param>
        /// <param name="registrantInfo">The registrant information.</param>
        /// <param name="personChanges">The person history changes that were made.</param>
        /// <param name="settings">The registration settings.</param>
        /// <returns>A tuple that contains the <see cref="Campus"/> identifier and the <see cref="Location"/> object if either were found.</returns>
        private (int? campusId, Location location, bool updateExistingCampus) UpdatePersonFromRegistrant( Person person, ViewModels.Blocks.Event.RegistrationEntry.RegistrantBag registrantInfo, History.HistoryChangeList personChanges, RegistrationSettings settings )
        {
            Location location = null;
            var campusId = PageParameter( PageParameterKey.CampusId ).AsIntegerOrNull();
            var updateExistingCampus = false;
            var personService = new PersonService( this.RockContext );
            var homeNumberDefinedValue = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() );
            var mobileNumberDefinedValue = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
            var workNumberDefinedValue = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() );

            // Set any of the template's person fields
            foreach ( var field in settings.Forms
                .SelectMany( f => f.Fields
                    .Where( t => t.FieldSource == RegistrationFieldSource.PersonField ) ) )
            {
                // Find the registrant's value
                var fieldValue = GetPersonFieldValue( settings, field.PersonFieldType, registrantInfo.FieldValues );

                if ( fieldValue != null )
                {
                    switch ( field.PersonFieldType )
                    {
                        case RegistrationPersonFieldType.Email:
                            // Only update the person's email if they are in the same family as the logged in person (not the registrar)
                            var currentPersonId = GetCurrentPerson()?.Id;
                            var isFamilyMember = currentPersonId.HasValue && person.GetFamilies().ToList().Select( f => f.ActiveMembers().Where( m => m.PersonId == currentPersonId ) ).Any();
                            if ( isFamilyMember && IsFieldUnlockedForEditing( field, person.Email ) )
                            {
                                var email = fieldValue.ToString().Trim();
                                History.EvaluateChange( personChanges, "Email", person.Email, email );
                                person.Email = email;
                            }

                            break;

                        case RegistrationPersonFieldType.Campus:
                            if ( IsFieldUnlockedForEditing( field, person.PrimaryCampusId ) )
                            {
                                var campusGuid = fieldValue.ToString().AsGuidOrNull();
                                updateExistingCampus = campusGuid.HasValue;
                                campusId = campusGuid.HasValue ? CampusCache.Get( campusGuid.Value )?.Id ?? campusId : campusId;
                            }

                            break;

                        case RegistrationPersonFieldType.MiddleName:
                            if ( IsFieldUnlockedForEditing( field, person.MiddleName ) )
                            {
                                var middleName = fieldValue.ToString().Trim();
                                History.EvaluateChange( personChanges, "Middle Name", person.MiddleName, middleName );
                                person.MiddleName = middleName;
                            }

                            break;

                        case RegistrationPersonFieldType.Address:
                            var existingHomeLocation = person.GetHomeLocation( this.RockContext );

                            if ( IsFieldUnlockedForEditing( field, existingHomeLocation ) )
                            {
                                var addressViewModel = fieldValue.ToStringSafe().FromJsonOrNull<AddressControlBag>();

                                if ( addressViewModel != null )
                                {
                                    // TODO: The default country should be removed once Obsidian has full country support.
                                    location = new Location
                                    {
                                        Street1 = addressViewModel.Street1,
                                        Street2 = addressViewModel.Street2,
                                        City = addressViewModel.City,
                                        State = addressViewModel.State,
                                        PostalCode = addressViewModel.PostalCode,
                                        Country = addressViewModel.Country ?? GlobalAttributesCache.Get().OrganizationCountry
                                    };
                                }
                            }

                            break;

                        case RegistrationPersonFieldType.Birthdate:
                            if ( IsFieldUnlockedForEditing( field, person.BirthDate ) )
                            {
                                var oldBirthMonth = person.BirthMonth;
                                var oldBirthDay = person.BirthDay;
                                var oldBirthYear = person.BirthYear;

                                person.SetBirthDate( fieldValue.ToStringSafe().FromJsonOrNull<BirthdayPickerBag>().ToDateTime() );

                                History.EvaluateChange( personChanges, "Birth Month", oldBirthMonth, person.BirthMonth );
                                History.EvaluateChange( personChanges, "Birth Day", oldBirthDay, person.BirthDay );
                                History.EvaluateChange( personChanges, "Birth Year", oldBirthYear, person.BirthYear );
                            }

                            break;

                        case RegistrationPersonFieldType.Gender:
                            if ( IsFieldUnlockedForEditing( field, person.Gender ) )
                            {
                                var newGender = fieldValue.ToString().ConvertToEnumOrNull<Gender>() ?? Gender.Unknown;
                                History.EvaluateChange( personChanges, "Gender", person.Gender, newGender );
                                person.Gender = newGender;
                            }

                            break;

                        case RegistrationPersonFieldType.AnniversaryDate:
                            if ( IsFieldUnlockedForEditing( field, person.AnniversaryDate ) )
                            {
                                var oldAnniversaryDate = person.AnniversaryDate;
                                person.AnniversaryDate = fieldValue.ToStringSafe().FromJsonOrNull<BirthdayPickerBag>().ToDateTime();
                                History.EvaluateChange( personChanges, "Anniversary Date", oldAnniversaryDate, person.AnniversaryDate );
                            }

                            break;

                        case RegistrationPersonFieldType.MaritalStatus:
                            if ( IsFieldUnlockedForEditing( field, person.MaritalStatusValueId ) )
                            {
                                var newMaritalStatusValueGuid = fieldValue.ToStringSafe().AsGuidOrNull();
                                var newMaritalStatusValueId = newMaritalStatusValueGuid.HasValue ? DefinedValueCache.Get( newMaritalStatusValueGuid.Value )?.Id : null;
                                var oldMaritalStatusValueId = person.MaritalStatusValueId;
                                person.MaritalStatusValueId = newMaritalStatusValueId;
                                History.EvaluateChange( personChanges, "Marital Status", DefinedValueCache.GetName( oldMaritalStatusValueId ), DefinedValueCache.GetName( person.MaritalStatusValueId ) );
                            }

                            break;

                        case RegistrationPersonFieldType.MobilePhone:
                            if ( IsFieldUnlockedForEditing( field, personService.GetPhoneNumber( person, mobileNumberDefinedValue )?.Number ) )
                            {
                                SavePhone( fieldValue, person, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid(), personChanges );
                            }

                            break;

                        case RegistrationPersonFieldType.HomePhone:
                            if ( IsFieldUnlockedForEditing( field, personService.GetPhoneNumber( person, homeNumberDefinedValue )?.Number ) )
                            {
                                SavePhone( fieldValue, person, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid(), personChanges );
                            }

                            break;

                        case RegistrationPersonFieldType.WorkPhone:
                            if ( IsFieldUnlockedForEditing( field, personService.GetPhoneNumber( person, workNumberDefinedValue )?.Number ) )
                            {
                                SavePhone( fieldValue, person, Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid(), personChanges );
                            }

                            break;

                        case RegistrationPersonFieldType.ConnectionStatus:
                            if ( IsFieldUnlockedForEditing( field, person.ConnectionStatusValueId ) )
                            {
                                var newConnectionStatusValueGuid = fieldValue.ToStringSafe().AsGuidOrNull();
                                var newConnectionStatusValueId = newConnectionStatusValueGuid.HasValue
                                    ? DefinedValueCache.Get( newConnectionStatusValueGuid.Value )?.Id
                                    : null;
                                var oldConnectionStatusValueId = person.ConnectionStatusValueId;
                                person.ConnectionStatusValueId = newConnectionStatusValueId;
                                History.EvaluateChange( personChanges, "Connection Status", DefinedValueCache.GetName( oldConnectionStatusValueId ), DefinedValueCache.GetName( person.ConnectionStatusValueId ) );
                            }

                            break;

                        case RegistrationPersonFieldType.Grade:
                            if ( IsFieldUnlockedForEditing( field, person.GradeOffset ) )
                            {
                                var newGradeGuid = fieldValue.ToStringSafe().AsGuidOrNull();
                                var newGradeOffset = newGradeGuid.HasValue ? DefinedValueCache.Get( newGradeGuid.Value )?.Value.AsIntegerOrNull() : null;
                                var newGraduationYear = Person.GraduationYearFromGradeOffset( newGradeOffset );

                                // Don't wipe out a past graduation date if they picked a blank value.
                                if ( newGraduationYear.HasValue || ( person.GradeOffset.HasValue && person.GradeOffset >= 0 ) )
                                {
                                    var oldGraduationYear = person.GraduationYear;
                                    person.GraduationYear = newGraduationYear;
                                    History.EvaluateChange( personChanges, "Graduation Year", oldGraduationYear, person.GraduationYear );
                                }
                            }

                            break;

                        case RegistrationPersonFieldType.Race:
                            if ( IsFieldUnlockedForEditing( field, person.RaceValueId ) )
                            {
                                var newRaceValueGuid = fieldValue.ToStringSafe().AsGuidOrNull();
                                var newRaceValueId = newRaceValueGuid.HasValue ? DefinedValueCache.Get( newRaceValueGuid.Value )?.Id : null;
                                var oldRaceValueId = person.RaceValueId;
                                person.RaceValueId = newRaceValueId;
                                History.EvaluateChange( personChanges, "Race", DefinedValueCache.GetName( oldRaceValueId ), DefinedValueCache.GetName( person.RaceValueId ) );
                            }

                            break;

                        case RegistrationPersonFieldType.Ethnicity:
                            if ( IsFieldUnlockedForEditing( field, person.EthnicityValueId ) )
                            {
                                var newEthnicityValueGuid = fieldValue.ToStringSafe().AsGuidOrNull();
                                var newEthnicityValueId = newEthnicityValueGuid.HasValue ? DefinedValueCache.Get( newEthnicityValueGuid.Value )?.Id : null;
                                var oldEthnicityValueId = person.EthnicityValueId;
                                person.EthnicityValueId = newEthnicityValueId;
                                History.EvaluateChange( personChanges, "Ethnicity", DefinedValueCache.GetName( oldEthnicityValueId ), DefinedValueCache.GetName( person.EthnicityValueId ) );
                            }

                            break;
                    }
                }

                field.NoteFieldDetailsIfRequiredAndMissing( MissingFieldsByFormId, fieldValue );
            }

            return (campusId, location, updateExistingCampus);
        }

        /// <summary>
        /// Updates the person attributes from the information contained in the
        /// registrant information.
        /// </summary>
        /// <param name="person">The person object to be updated.</param>
        /// <param name="personChanges">The person history changes that were made.</param>
        /// <param name="registrantInfo">The registrant information.</param>
        /// <param name="settings">The registration settings.</param>
        /// <returns><c>true</c> if any attributes were modified, <c>false</c> otherwise.</returns>
        private bool UpdatePersonAttributes( Person person, History.HistoryChangeList personChanges, ViewModels.Blocks.Event.RegistrationEntry.RegistrantBag registrantInfo, RegistrationSettings settings )
        {
            var isChanged = false;
            var personAttributes = settings.Forms
                .SelectMany( f => f.Fields
                    .Where( t =>
                        t.FieldSource == RegistrationFieldSource.PersonAttribute &&
                        t.AttributeId.HasValue ) );

            // Set any of the template's person attribute fields
            foreach ( var field in personAttributes )
            {
                // Find the registrant's value
                var fieldValue = registrantInfo.FieldValues.GetValueOrNull( field.Guid );

                if ( fieldValue != null )
                {
                    var attribute = AttributeCache.Get( field.AttributeId.Value );
                    if ( attribute != null )
                    {
                        if ( IsFieldUnlockedForEditing( field, person, attribute.Key ) )
                        {
                            // Note: As per discussion with architecture team, it is correct
                            // behavior that the new value will always overwrite the old
                            // value, even if the new value is blank.
                            var originalValue = person.GetAttributeValue( attribute.Key );
                            var newValue = PublicAttributeHelper.GetPrivateValue( attribute, fieldValue.ToString() );
                            person.SetAttributeValue( attribute.Key, newValue );

                            if ( ( originalValue ?? string.Empty ).Trim() != ( newValue ?? string.Empty ).Trim() )
                            {
                                var formattedOriginalValue = string.Empty;
                                if ( !string.IsNullOrWhiteSpace( originalValue ) )
                                {
                                    formattedOriginalValue = attribute.FieldType.Field.GetTextValue( originalValue, attribute.ConfigurationValues );
                                }

                                var formattedNewValue = string.Empty;
                                if ( !string.IsNullOrWhiteSpace( newValue ) )
                                {
                                    formattedNewValue = attribute.FieldType.Field.GetTextValue( newValue, attribute.ConfigurationValues );
                                }

                                isChanged = true;
                                History.EvaluateChange( personChanges, attribute.Name, formattedOriginalValue, formattedNewValue );
                            }
                        }
                    }

                    field.NoteFieldDetailsIfRequiredAndMissing( MissingFieldsByFormId, fieldValue );
                }
            }

            return isChanged;
        }

        /// <summary>
        /// Upserts the registrant.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="context">The context.</param>
        /// <param name="registrar">The registrar.</param>
        /// <param name="registrarFamilyGuid">The registrar family unique identifier.</param>
        /// <param name="registrantInfo">The registrant information.</param>
        /// <param name="index">The index.</param>
        /// <param name="multipleFamilyGroupIds">The multiple family group ids.</param>
        /// <param name="singleFamilyId">The single family identifier.</param>
        /// <param name="isWaitlist">if set to <c>true</c> then registrant is on the wait list.</param>
        /// <param name="isCreatedAsRegistrant">if set to <c>true</c> [is created as registrant].</param>
        /// <param name="isNewRegistration"><c>true</c> if the registration is new; otherwise <c>false</c>.</param>
        /// <param name="postSaveActions">Additional post save actions that can be appended to.</param>
        private void UpsertRegistrant(
            RockContext rockContext,
            RegistrationContext context,
            Person registrar,
            Guid registrarFamilyGuid,
            RegistrantBag registrantInfo,
            int index,
            Dictionary<Guid, int> multipleFamilyGroupIds,
            ref int? singleFamilyId,
            bool isWaitlist,
            bool isCreatedAsRegistrant,
            bool isNewRegistration,
            List<Action> postSaveActions )
        {
            // Force waitlist if specified by param, but allow waitlist if requested
            isWaitlist |= (context.RegistrationSettings.IsWaitListEnabled && registrantInfo.IsOnWaitList);

            var personService = new PersonService( rockContext );
            var registrationInstanceService = new RegistrationInstanceService( rockContext );
            var registrantService = new RegistrationRegistrantService( rockContext );

            var registrantChanges = new History.HistoryChangeList();
            var personChanges = new History.HistoryChangeList();

            var (person, registrant) = GetExistingOrCreatePerson( context, registrantInfo, registrar, registrarFamilyGuid, isCreatedAsRegistrant, rockContext );

            var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
            var adultRoleId = familyGroupType.Roles
                .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) )
                .Select( r => r.Id )
                .FirstOrDefault();
            var childRoleId = familyGroupType.Roles
                .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ) )
                .Select( r => r.Id )
                .FirstOrDefault();

            // Update the person object from the registrant information.
            var (campusId, location, updateExistingCampus) = UpdatePersonFromRegistrant( person, registrantInfo, personChanges, context.RegistrationSettings );

            // If campus was not provided, then check the page parameter.
            campusId = campusId ?? PageParameter( PageParameterKey.CampusId ).AsIntegerOrNull();

            // Save the person ( and family if needed )
            SavePerson( rockContext, context.RegistrationSettings, person, registrantInfo.FamilyGuid ?? Guid.NewGuid(), campusId, location, adultRoleId, childRoleId, multipleFamilyGroupIds, ref singleFamilyId, updateExistingCampus );

            // Take note of this registered person identifier.
            context.PersonIdsRegisteredWithinThisSession.Add( person.Id );

            // Load the person's attributes
            person.LoadAttributes();

            if ( UpdatePersonAttributes( person, personChanges, registrantInfo, context.RegistrationSettings ) )
            {
                person.SaveAttributeValues( rockContext );
            }

            var registrantName = person.FullName + ": ";

            personChanges.ForEach( c => registrantChanges.Add( c ) );

            if ( registrant == null )
            {
                registrant = new RegistrationRegistrant
                {
                    Guid = registrantInfo.Guid,
                    RegistrationId = context.Registration.Id,
                    RegistrationTemplateId = context.Registration.RegistrationTemplateId.Value,
                    Cost = context.RegistrationSettings.PerRegistrantCost
                };
                registrantService.Add( registrant );
            }

            registrant.OnWaitList = isWaitlist;
            registrant.PersonAliasId = person.PrimaryAliasId;
            registrant.PersonAlias = person.PrimaryAlias;

            // Check if discount applies
            var maxRegistrants = context.Discount?.RegistrationTemplateDiscount.MaxRegistrants;
            var isWithinMaxRegistrants = !maxRegistrants.HasValue || index < maxRegistrants.Value;
            registrant.DiscountApplies = isWithinMaxRegistrants;

            /*
                2023-08-07 edrotnign
                Do not check the value RegistrationTemplateDiscount.MaxUsage here. That is a registration level value and this is a registrant level operation.
             */

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
                // Include `IsActive == false` fees and fee items here, as the registrant might have
                // been registered at a time when currently-inactive fees were active. If the fees
                // were active at the time of registration, they still apply to this registrant.
                var feeModels = context.RegistrationSettings.Fees?
                    .OrderBy( f => f.Order )
                    .ToList() ?? new List<RegistrationTemplateFee>();

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

                        // If there is a limited supply, ensure that more are not ordered than available
                        var countRemaining = context.FeeItemsCountRemaining.GetValueOrNull( feeItemModel.Guid );

                        // Adjust for any existing quantity usage. Meaning, if countRemaining
                        // is 0 but we have 1 quantity used on file (in the database) then we
                        // add that back in. Otherwise the logic below would automatically unselect
                        // it because it thinks no value is available.
                        countRemaining += registrantFee?.Quantity ?? 0;

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
            if ( UpdateRegistrantAttributes( registrant, registrantInfo, registrantChanges, context.RegistrationSettings ) )
            {
                registrant.SaveAttributeValues( rockContext );
            }

            // Save the signed document if we have one. We only process a document
            // if this is a new registration since editing registrations with an
            // inline signature is not currently supported.
            if ( context.RegistrationSettings.SignatureDocumentTemplateId.HasValue && context.RegistrationSettings.IsInlineSignatureRequired && isNewRegistration )
            {
                var signatureDocumentService = new SignatureDocumentService( rockContext );

                // If a previously-signed document was specified for reuse, make a query to the database to get its ID.
                int? existingSignatureDocumentId = null;
                if ( registrantInfo.ExistingSignatureDocumentGuid.HasValue )
                {
                    existingSignatureDocumentId = signatureDocumentService.GetId( registrantInfo.ExistingSignatureDocumentGuid.Value );
                }

                // If the previous document's ID was found, use it to complete the registration. Otherwise, attempt to create a new document.
                if ( existingSignatureDocumentId.HasValue )
                {
                    registrant.SignatureDocumentId = existingSignatureDocumentId.Value;
                    rockContext.SaveChanges();
                }
                else
                {
                    var documentTemplate = new SignatureDocumentTemplateService( rockContext ).Get( context.RegistrationSettings.SignatureDocumentTemplateId ?? 0 );

                    // Get the signed document data and encode any string data that was sent by the front-end.
                    var signedData = Encryption.DecryptString( registrantInfo.SignatureData ).FromJsonOrThrow<SignedDocumentData>();
                    signedData.SignatureData = signedData.SignatureData.EncodeHtml();
                    signedData.SignedByName = signedData.SignedByName.EncodeHtml();
                    signedData.SignedByEmail = signedData.SignedByEmail.EncodeHtml();
                    signedData.IpAddress = signedData.IpAddress.EncodeHtml();
                    signedData.UserAgent = signedData.UserAgent.EncodeHtml();

                    // Don't trust that the DocumentHtml sent to the user hasn't been altered - recreate it for safety.
                    signedData.DocumentHtml = GetDocumentHtml( rockContext, context, registrantInfo, person, campusId, location, documentTemplate );

                    var signedBy = RequestContext.CurrentPerson ?? registrar;

                    var document = CreateSignatureDocument( documentTemplate, signedData, signedBy, registrar, person, registrant.PersonAlias?.Person?.FullName ?? person.FullName, context.RegistrationSettings.Name );

                    signatureDocumentService.Add( document );
                    registrant.SignatureDocument = document;
                    rockContext.SaveChanges();

                    // Send communication after the save is complete.
                    if ( documentTemplate.CompletionSystemCommunication != null )
                    {
                        postSaveActions.Add( () =>
                        {
                            ElectronicSignatureHelper.SendSignatureCompletionCommunication( document.Id, out _ );
                        } );
                    }
                }
            }

            var currentPerson = GetCurrentPerson();
            var currentPersonAliasId = currentPerson?.PrimaryAliasId;

            Task.Run( () =>
                HistoryService.SaveChanges(
                    new RockContext(),
                    typeof( Registration ),
                    Rock.SystemGuid.Category.HISTORY_EVENT_REGISTRATION.AsGuid(),
                    context.Registration.Id,
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
        /// Gets the Document HTML for a SignatureDocumentTemplate.
        /// </summary>
        /// <param name="rockContext">The <see cref="RockContext"/> to use for data access.</param>
        /// <param name="context">The <see cref="RegistrationContext"/> to use for merge fields.</param>
        /// <param name="registrantInfo">The <see cref="RegistrantBag"/> to use for merge fields.</param>
        /// <param name="person">The <see cref="Person"/> to use for merge fields.</param>
        /// <param name="campusId">The identifier of the <see cref="Campus"/> to use for merge fields.</param>
        /// <param name="location">The <see cref="Location"/> to use for merge fields.</param>
        /// <param name="documentTemplate">The <see cref="SignatureDocumentTemplate"/> to get the LavaTemplate from.</param>
        /// <returns>The Lava resolved document HTML to be signed.</returns>
        private string GetDocumentHtml( RockContext rockContext, RegistrationContext context, RegistrantBag registrantInfo, Person person, int? campusId, Location location, SignatureDocumentTemplate documentTemplate )
        {
            // Process the GroupMember so we have data for the Lava merge.
            GroupMember groupMember = null;
            var groupId = GetRegistrationGroupId( rockContext, context.Registration.RegistrationInstanceId );

            if ( groupId.HasValue )
            {
                var group = new GroupService( rockContext ).Get( groupId.Value );

                groupMember = BuildGroupMember( person, group, context.RegistrationSettings );
                groupMember.LoadAttributes( rockContext );
                UpdateGroupMemberAttributes( groupMember, registrantInfo, context.RegistrationSettings );
            }

            // Prepare the merge fields.
            var campusCache = campusId.HasValue ? CampusCache.Get( campusId.Value ) : null;

            var mergeFields = new Dictionary<string, object>
            {
                { "Registration", new LavaSignatureRegistration( context.Registration.RegistrationInstance, context.Registration.GroupId, context.Registration.Registrants.Count() ) },
                { "Registrant", new LavaSignatureRegistrant( person, location, campusCache, groupMember, registrantInfo, context.Registration.RegistrationInstance ) }
            };

            return ElectronicSignatureHelper.GetSignatureDocumentHtml( documentTemplate.LavaTemplate, mergeFields );
        }

        private static (Dictionary<string, AttributeCache>, Dictionary<string, AttributeValueCache>) GetRegistrantAttributesFromRegistration( ViewModels.Blocks.Event.RegistrationEntry.RegistrantBag registrantInfo, RegistrationTemplate template )
        {
            var attributes = new Dictionary<string, AttributeCache>();
            var attributeValues = new Dictionary<string, AttributeValueCache>();
            var registrantAttributeFields = template.Forms
                .SelectMany( f => f.Fields.Where( ff => ff.AttributeId.HasValue && ff.FieldSource == RegistrationFieldSource.RegistrantAttribute ) )
                .ToList();

            foreach ( var field in registrantAttributeFields )
            {
                var attribute = AttributeCache.Get( field.AttributeId.Value );

                if ( attribute is null )
                {
                    continue;
                }

                var newValue = registrantInfo.FieldValues.GetValueOrNull( field.Guid ).ToStringSafe();
                newValue = PublicAttributeHelper.GetPrivateValue( attribute, newValue );

                var attributeValue = new AttributeValueCache( field.AttributeId.Value, null, newValue );
                attributes.Add( attribute.Key, attribute );
                attributeValues.Add( attribute.Key, attributeValue );
            }

            return (attributes, attributeValues);
        }

        /// <summary>
        /// Updates the registrant attribute values with those provided by the
        /// registrant information.
        /// </summary>
        /// <param name="registrant">The registrant to be updated.</param>
        /// <param name="registrantInfo">The registrant information.</param>
        /// <param name="registrantChanges">The registrant changes that were made.</param>
        /// <param name="settings">The registration settings.</param>
        /// <returns><c>true</c> if any attributes were modified, <c>false</c> otherwise.</returns>
        private bool UpdateRegistrantAttributes( RegistrationRegistrant registrant, ViewModels.Blocks.Event.RegistrationEntry.RegistrantBag registrantInfo, History.HistoryChangeList registrantChanges, RegistrationSettings settings )
        {
            var isChanged = false;
            var registrantAttributeFields = settings.Forms
                .SelectMany( f => f.Fields.Where( ff => ff.AttributeId.HasValue && ff.FieldSource == RegistrationFieldSource.RegistrantAttribute ) )
                .ToList();

            foreach ( var field in registrantAttributeFields )
            {
                var attribute = AttributeCache.Get( field.AttributeId.Value );

                if ( attribute is null )
                {
                    continue;
                }

                if ( IsFieldUnlockedForEditing( field, registrant, attribute.Key ) )
                {
                    var originalValue = registrant.GetAttributeValue( attribute.Key );
                    var newValue = registrantInfo.FieldValues.GetValueOrNull( field.Guid ).ToStringSafe();
                    newValue = PublicAttributeHelper.GetPrivateValue( attribute, newValue );

                    registrant.SetAttributeValue( attribute.Key, newValue );

                    if ( ( originalValue ?? string.Empty ).Trim() != ( newValue ?? string.Empty ).Trim() )
                    {
                        var formattedOriginalValue = string.Empty;
                        if ( !string.IsNullOrWhiteSpace( originalValue ) )
                        {
                            formattedOriginalValue = attribute.FieldType.Field.GetTextValue( originalValue, attribute.ConfigurationValues );
                        }

                        string formattedNewValue = string.Empty;
                        if ( !string.IsNullOrWhiteSpace( newValue ) )
                        {
                            formattedNewValue = attribute.FieldType.Field.GetTextValue( newValue, attribute.ConfigurationValues );
                        }

                        isChanged = true;
                        History.EvaluateChange( registrantChanges, attribute.Name, formattedOriginalValue, formattedNewValue );
                    }

                    field.NoteFieldDetailsIfRequiredAndMissing( MissingFieldsByFormId, newValue );
                }
            }

            return isChanged;
        }

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <returns></returns>
        private RegistrationEntryInitializationBox GetInitializationBox( RockContext rockContext )
        {
            // Get the registration context (template, instance, actual registration (if existing))
            var context = GetContext( rockContext, out var errorMessage );

            if ( context is null )
            {
                return new RegistrationEntryInitializationBox
                {
                    RegistrationInstanceNotFoundMessage = errorMessage
                };
            }

            // If the registration is existing, then add the args that describe it to the view model
            var session = GetRegistrationEntryBlockSession( rockContext, context );

            /*
                9/7/2022 - SMC / DSH / NA
                
                isExistingRegistration is true if we have a RegistrationId in the page parameters, OR if we have a saved
                RegistrationGuid in the RegistrationSession temporary table.  This is true because redirection payment gateways
                (like Pushpay) may not return the RegistrationId parameter.

                This will result loading the registration from the database, which previously caused a security error if the
                currently logged in person (i.e., CurrentPerson) does not match the person who created the Registration (the
                "registrar").  This error seems to have been related to the protection profiles and the creation of new person
                records (for the registrar).  We no longer believe this is happening, so it should be okay to set the
                RegistrationGuid argument.

                Reason:  Resolving errors when processing additional payments from redirection gateways.
            */

            var isExistingRegistration = PageParameter( PageParameterKey.RegistrationId ).AsIntegerOrNull().HasValue || session?.RegistrationGuid.HasValue == true;
            var isUnauthorized = isExistingRegistration && session == null;
            RegistrationEntrySuccessBag successViewModel = null;

            if ( session != null )
            {
                var args = new RegistrationEntryArgsBag
                {
                    AmountToPayNow = session.AmountToPayNow,
                    DiscountCode = session.DiscountCode,
                    FieldValues = session.FieldValues,
                    Registrants = session.Registrants,
                    Registrar = session.Registrar,
                    RegistrationGuid = session.RegistrationGuid, // See engineering note from 9/7/2022 above.
                    RegistrationSessionGuid = session.RegistrationSessionGuid
                };

                if ( session.GroupId.HasValue )
                {
                    RequestContext.PageParameters.AddOrReplace( PageParameterKey.GroupId, session.GroupId.ToString() );
                }

                if ( session.Slug.IsNotNullOrWhiteSpace() )
                {
                    RequestContext.PageParameters.AddOrReplace( PageParameterKey.Slug, session.Slug );
                }

                // Get a new context with the args
                context = GetContext( rockContext, args, out errorMessage );

                var financialGatewayService = new FinancialGatewayService( rockContext );
                var paymentFinancialGateway = financialGatewayService.Get( context.RegistrationSettings.FinancialGatewayId ?? 0 );
                var gateway = paymentFinancialGateway?.GetGatewayComponent();
                var paymentToken = string.Empty;
                var wasRedirectedFromPayment = gateway is IPaymentTokenGateway paymentGateway && paymentGateway.TryGetPaymentTokenFromParameters( paymentFinancialGateway, RequestContext.GetPageParameters(), out paymentToken );

                if ( wasRedirectedFromPayment )
                {
                    args.GatewayToken = paymentToken;

                    // This is a redirect from a redirect gateway. The user was sent to
                    // another site, made payment, and has come back after completion.
                    if ( !isExistingRegistration )
                    {
                        SubmitRegistration( rockContext, context, args, out errorMessage );
                    }
                    else
                    {
                        // Existing registration, but they are making another payment.
                        var paymentInfo = GetPaymentInfo( rockContext, context, args, gateway, out errorMessage );
                        if ( errorMessage.IsNotNullOrWhiteSpace() )
                        {
                            throw new Exception( errorMessage );
                        }

                        var transaction = ProcessGatewayPayment( rockContext, context, args, paymentFinancialGateway, gateway, paymentInfo, out errorMessage );

                        if ( !errorMessage.IsNullOrWhiteSpace() )
                        {
                            throw new Exception( errorMessage );
                        }
                        else if ( transaction == null )
                        {
                            throw new Exception( "There was a problem with the payment" );
                        }
                        else
                        {
                            // The payment was processed successfully, so save the transaction in Rock.
                            SaveTransaction( gateway, context, transaction, paymentInfo, rockContext );
                        }
                    }

                    successViewModel = GetSuccessViewModel( context.Registration.Id, context.TransactionCode, context.GatewayPersonIdentifier );
                }
            }

            // Get models needed for the view model
            var hasDiscountsAvailable = context.RegistrationSettings.Discounts?.Any() == true;
            var formModels = context.RegistrationSettings
                .Forms?.OrderBy( f => f.Order ).ToList() ?? new List<RegistrationTemplateForm>();

            // Get family members
            var currentPerson = GetCurrentPerson();
            var familyMembers = context.RegistrationSettings.AreCurrentFamilyMembersShown ?
                currentPerson.GetFamilyMembers( true, rockContext )
                    .Select( gm => new
                    {
                        FamilyGuid = gm.Group.Guid,
                        Person = gm.Person
                    } )
                    .DistinctBy( gm => gm.Person.Guid )
                    .ToList()
                    .Select( gm => new RegistrationEntryFamilyMemberBag
                    {
                        Guid = gm.Person.Guid,
                        FamilyGuid = gm.FamilyGuid,
                        FullName = gm.Person.FullName,
                        FieldValues = GetCurrentValueFieldValues( context, rockContext, gm.Person, null, formModels, false )
                    } )
                    .ToList() :
                    new List<RegistrationEntryFamilyMemberBag>();

            // Get the instructions
            var instructions = context.RegistrationSettings.Instructions;

            // Get the fee term
            var feeTerm = context.RegistrationSettings.FeeTerm;
            feeTerm = feeTerm.ToLower();
            var pluralFeeTerm = feeTerm.Pluralize();

            // Get the registrant term
            var registrantTerm = context.RegistrationSettings.RegistrantTerm;
            registrantTerm = registrantTerm.ToLower();
            var pluralRegistrantTerm = registrantTerm.Pluralize();

            // Get the discount code term
            var discountCodeTerm = context.RegistrationSettings.DiscountCodeTerm.ToLower();
            var pluralDiscountCodeTerm = discountCodeTerm.Pluralize();

            // Get the fees
            var feeModels = context.RegistrationSettings.Fees?.Where( f => f.IsActive ).OrderBy( f => f.Order ).ToList() ?? new List<RegistrationTemplateFee>();
            var fees = new List<RegistrationEntryFeeBag>();

            foreach ( var feeModel in feeModels )
            {
                var feeViewModel = new RegistrationEntryFeeBag
                {
                    Guid = feeModel.Guid,
                    Name = feeModel.Name,
                    AllowMultiple = feeModel.AllowMultiple,
                    IsRequired = feeModel.IsRequired,
                    DiscountApplies = feeModel.DiscountApplies,
                    HideWhenNoneRemaining = feeModel.HideWhenNoneRemaining,
                    Items = feeModel.FeeItems
                        .Where( fi => fi.IsActive )
                        .Select( fi => new RegistrationEntryFeeItemBag
                        {
                            Cost = fi.Cost,
                            Name = fi.Name,
                            Guid = fi.Guid,
                            OriginalCountRemaining = context.FeeItemsCountRemaining.GetValueOrNull( fi.Guid ),
                            CountRemaining = context.FeeItemsCountRemaining.GetValueOrNull( fi.Guid )
                        } )
                        .ToList()
                };

                fees.Add( feeViewModel );
            }

            // Get forms with fields
            var formViewModels = new List<RegistrationEntryFormBag>();
            var allAttributeFields = formModels
                .SelectMany( fm =>
                    fm.Fields.Where( f => !f.IsInternal && f.Attribute?.IsActive == true )
                ).ToList();

            foreach ( var formModel in formModels )
            {
                var form = new RegistrationEntryFormBag();
                var fieldModels = formModel.Fields
                    .Where( f => !f.IsInternal && ( f.Attribute == null || f.Attribute.IsActive ) )
                    .OrderBy( f => f.Order );
                var fields = new List<RegistrationEntryFormFieldBag>();

                foreach ( var fieldModel in fieldModels )
                {
                    var field = new RegistrationEntryFormFieldBag();
                    var attribute = fieldModel.AttributeId.HasValue ? AttributeCache.Get( fieldModel.AttributeId.Value ) : null;

                    field.Guid = fieldModel.Guid;
                    field.Attribute = attribute != null ? PublicAttributeHelper.GetPublicAttributeForEdit( attribute ) : null;
                    field.FieldSource = fieldModel.FieldSource;
                    field.PersonFieldType = fieldModel.PersonFieldType;
                    field.IsRequired = fieldModel.IsRequired;
                    field.IsSharedValue = fieldModel.IsSharedValue;
                    field.VisibilityRuleType = fieldModel.FieldVisibilityRules.FilterExpressionType;
                    field.PreHtml = fieldModel.PreText;
                    field.PostHtml = fieldModel.PostText;
                    field.ShowOnWaitList = fieldModel.ShowOnWaitlist;
                    field.IsLockedIfValuesExist = fieldModel.IsLockedIfValuesExist;

                    field.VisibilityRules = fieldModel.FieldVisibilityRules
                        .RuleList
                        .Select( vr =>
                        {
                            if ( !vr.ComparedToFormFieldGuid.HasValue )
                            {
                                return null;
                            }

                            var comparedToField = allAttributeFields.SingleOrDefault( f => f.Guid == vr.ComparedToFormFieldGuid );
                            if ( comparedToField == null )
                            {
                                return null;
                            }

                            var fieldAttribute = AttributeCache.Get( comparedToField.AttributeId.Value );
                            var ruleBag = FieldVisibilityRule.GetPublicRuleBag( fieldAttribute, vr.ComparisonType, vr.ComparedToValue );

                            return new RegistrationEntryVisibilityBag
                            {
                                ComparedToRegistrationTemplateFormFieldGuid = vr.ComparedToFormFieldGuid.Value,
                                ComparisonValue = new PublicComparisonValueBag
                                {
                                    ComparisonType = ( int? ) ruleBag.ComparisonType,
                                    Value = ruleBag.Value
                                }
                            };
                        } )
                        .Where( vr => vr != null )
                        .ToList();

                    fields.Add( field );
                }

                form.Fields = fields;
                formViewModels.Add( form );
            }

            // Get the registration attributes term
            var registrationAttributeTitleStart = context.RegistrationSettings.AttributeTitleStart;
            var registrationAttributeTitleEnd = context.RegistrationSettings.AttributeTitleEnd;

            // Get the registration term
            var registrationTerm = context.RegistrationSettings.RegistrationTerm;
            var pluralRegistrationTerm = registrationTerm.Pluralize();

            // Get the registration attributes
            var registrationAttributes = GetRegistrationAttributes( context.RegistrationSettings.RegistrationTemplateId );

            // only show the Registration Attributes Before Registrants that have a category of REGISTRATION_ATTRIBUTE_START_OF_REGISTRATION
            var beforeAttributes = registrationAttributes
                .Where( a =>
                    a.Categories.Any( c => c.Guid == Rock.SystemGuid.Category.REGISTRATION_ATTRIBUTE_START_OF_REGISTRATION.AsGuid() ) )
                .Select( a => PublicAttributeHelper.GetPublicAttributeForEdit( a ) )
                .ToList();

            // only show the Registration Attributes After Registrants that have don't have a category or have a category of REGISTRATION_ATTRIBUTE_END_OF_REGISTRATION
            var afterAttributes = registrationAttributes
                .Where( a =>
                    !a.Categories.Any() ||
                    a.Categories.Any( c => c.Guid == Rock.SystemGuid.Category.REGISTRATION_ATTRIBUTE_END_OF_REGISTRATION.AsGuid() ) )
                .Select( a => PublicAttributeHelper.GetPublicAttributeForEdit( a ) )
                .ToList();

            // Get the maximum number of registrants
            var maxRegistrants = context.RegistrationSettings.MaxRegistrants;

            // Force the registrar to update their email?
            var forceEmailUpdate = GetAttributeValue( AttributeKey.ForceEmailUpdate ).AsBoolean();

            // Check if saved accounts should be enabled.
            var enableSavedAccount = GetAttributeValue( AttributeKey.EnableSavedAccount ).AsBoolean();

            // Load the gateway control settings
            var financialGatewayId = context.RegistrationSettings.FinancialGatewayId;
            var financialGateway = financialGatewayId.HasValue ? new FinancialGatewayService( rockContext ).GetNoTracking( financialGatewayId.Value ) : null;
            var gatewayComponent = financialGateway?.GetGatewayComponent();
            var financialGatewayComponent = gatewayComponent as IObsidianHostedGatewayComponent;

            // Determine if this is a redirect gateway and get the redirect URL
            var redirectGateway = gatewayComponent as IRedirectionGatewayComponent;
            var isRedirectGateway = redirectGateway != null;
            var redirectGatewayUrl = string.Empty;

            // Get the amount due today and the initial amount to recommend paying
            var amountDueToday = context.RegistrationSettings.PerRegistrantMinInitialPayment;
            var initialAmountToPay = context.RegistrationSettings.PerRegistrantDefaultInitialPayment;
            var baseCost = context.RegistrationSettings.PerRegistrantCost;

            // Determine the timeout
            int? timeoutMinutes = null;

            if ( context.SpotsRemaining.HasValue && context.RegistrationSettings.TimeoutMinutes.HasValue )
            {
                var hasMetThreshold =
                    !context.RegistrationSettings.TimeoutThreshold.HasValue ||
                    context.SpotsRemaining.Value <= context.RegistrationSettings.TimeoutThreshold.Value;

                if ( hasMetThreshold )
                {
                    timeoutMinutes = context.RegistrationSettings.TimeoutMinutes.Value;
                }
            }

            // Initialize the client services to retrieve data in a way we can send
            // to the client.
            var campusClientService = new CampusClientService( rockContext, RequestContext.CurrentPerson );
            var savedAccountClientService = new FinancialPersonSavedAccountClientService( rockContext, RequestContext.CurrentPerson );

            // If we are using saved accounts and have all the details that we
            // need then attempt to load the current person's saved accounts.
            List<SavedFinancialAccountListItemBag> savedAccounts = null;
            if ( enableSavedAccount && RequestContext.CurrentPerson != null && financialGateway != null )
            {
                var accountOptions = new SavedFinancialAccountOptions
                {
                    FinancialGatewayGuids = new List<Guid> { financialGateway.Guid },
                    CurrencyTypeGuids = GetAllowedCurrencyTypes( gatewayComponent, financialGateway ).Select( a => a.Guid ).ToList()
                };

                savedAccounts = savedAccountClientService.GetSavedFinancialAccountsForPersonAsAccountListItems( RequestContext.CurrentPerson.Id, accountOptions );
            }

            // If we don't have a session that means we are starting new. Create an empty session.
            if ( session == null )
            {
                session = new RegistrationEntrySessionBag
                {
                    RegistrationSessionGuid = Guid.NewGuid()
                };

                session.Registrants = new List<ViewModels.Blocks.Event.RegistrationEntry.RegistrantBag>();
                var isOnWaitList = context.SpotsRemaining.HasValue && context.SpotsRemaining.Value == 0;

                if ( context.RegistrationSettings.AreCurrentFamilyMembersShown && currentPerson != null )
                {
                    // Fill in first registrant info as a member of the family.
                    session.Registrants.Add( new ViewModels.Blocks.Event.RegistrationEntry.RegistrantBag
                    {
                        Guid = Guid.NewGuid(),
                        FamilyGuid = currentPerson.PrimaryFamily.Guid,
                        IsOnWaitList = isOnWaitList,
                        PersonGuid = currentPerson.Guid,
                        FeeItemQuantities = new Dictionary<Guid, int>(),
                        FieldValues = GetCurrentValueFieldValues( context, rockContext, currentPerson, null, formModels, false )
                    } );
                }
                else
                {
                    // Only fill in the first registrant with existing values
                    // as a "new" person if family members are not shown.
                    session.Registrants.Add( new ViewModels.Blocks.Event.RegistrationEntry.RegistrantBag
                    {
                        Guid = Guid.NewGuid(),
                        FamilyGuid = Guid.NewGuid(),
                        IsOnWaitList = isOnWaitList,
                        PersonGuid = null,
                        FeeItemQuantities = new Dictionary<Guid, int>(),
                        FieldValues = !isOnWaitList ? GetCurrentValueFieldValues( context, rockContext, currentPerson, null, formModels, false ) : new Dictionary<Guid, object>()
                    } );
                }

                // Populate all the default registration attribute values.
                session.FieldValues = session.FieldValues ?? new Dictionary<Guid, object>();
                foreach ( var registrationAttribute in registrationAttributes )
                {
                    var defaultEditValue = PublicAttributeHelper.GetPublicValueForEdit( registrationAttribute, registrationAttribute.DefaultValue );

                    session.FieldValues[registrationAttribute.Guid] = defaultEditValue;
                }
            }

            // Determine the starting point. External registration updates are
            // currently only supported if we are not doing inline signatures.
            var allowExternalRegistrationUpdates = context.RegistrationSettings.AllowExternalRegistrationUpdates && !context.RegistrationSettings.IsInlineSignatureRequired;
            var allowRegistrationUpdates = !isExistingRegistration || allowExternalRegistrationUpdates;
            var startAtBeginning = !isExistingRegistration ||
                ( allowExternalRegistrationUpdates && PageParameter( PageParameterKey.StartAtBeginning ).AsBoolean() );

            // Adjust the spots remaining if this is an existing registration. Add to the Spots remaining the number of registrants that are not on the waitlist.
            var adjustedSpotsRemaining = isExistingRegistration && session != null
                ? context.SpotsRemaining + session.Registrants.Where( r => r.IsOnWaitList == false ).Count()
                : context.SpotsRemaining;

            // Adjust the original fee remaining counts to account for any existing
            // registrants that already have a fee item selected. In other words,
            // if the database says we have 2 fees remaining, but we are returning
            // to an existing registration that has 1 of that fee, then we should
            // tell the client that there were 3 original fees remaining.
            foreach ( var fee in fees )
            {
                foreach ( var feeItem in fee.Items )
                {
                    feeItem.OriginalCountRemaining += session.Registrants.Select( r => r.FeeItemQuantities.GetValueOrNull( feeItem.Guid ) ?? 0 ).Sum();
                }
            }

            var isPaymentPlanAllowed = context.RegistrationSettings.IsPaymentPlanAllowed;

            var currencyInfo = new RockCurrencyCodeInfo();
            var viewModel = new RegistrationEntryInitializationBox
            {
                AreCurrentFamilyMembersShown = context.RegistrationSettings.AreCurrentFamilyMembersShown,
                FamilyTerm = GetAttributeValue( AttributeKey.FamilyTerm ),
                RegistrationAttributesStart = beforeAttributes,
                RegistrationAttributesEnd = afterAttributes,
                RegistrationAttributeTitleStart = registrationAttributeTitleStart,
                RegistrationAttributeTitleEnd = registrationAttributeTitleEnd,
                InstructionsHtml = instructions,
                RegistrantTerm = registrantTerm,
                PluralRegistrantTerm = pluralRegistrantTerm,
                FeeTerm = feeTerm,
                PluralFeeTerm = pluralFeeTerm,
                RegistrationTerm = registrationTerm,
                DiscountCodeTerm = discountCodeTerm,
                PluralDiscountCodeTerm = pluralDiscountCodeTerm,
                RegistrantForms = formViewModels,
                Fees = fees,
                HideProgressBar = !GetAttributeValue( AttributeKey.DisplayProgressBar ).AsBoolean(),
                FamilyMembers = familyMembers,
                MaxRegistrants = context.RegistrationSettings.MaxRegistrants ?? 25,
                ShowSmsOptIn = context.RegistrationSettings.ShowSmsOptIn,
                RegistrantsSameFamily = context.RegistrationSettings.RegistrantsSameFamily,
                ForceEmailUpdate = forceEmailUpdate,
                RegistrarOption = context.RegistrationSettings.RegistrarOption,
                Cost = baseCost,
                GatewayControl = isRedirectGateway ? null : new GatewayControlBag
                {
                    FileUrl = financialGatewayComponent?.GetObsidianControlFileUrl( financialGateway ) ?? string.Empty,
                    Settings = financialGatewayComponent?.GetObsidianControlSettings( financialGateway, new HostedPaymentInfoControlOptions {
                        EnableACH = this.GetAttributeValue( AttributeKey.EnableACH ).AsBoolean(),
                        EnableCreditCard = this.GetAttributeValue( AttributeKey.EnableCreditCard ).AsBoolean(),
                        EnableBillingAddressCollection = true
                    } ) ?? new object()
                },
                IsRedirectGateway = isRedirectGateway,
                SpotsRemaining = adjustedSpotsRemaining,
                WaitListEnabled = context.RegistrationSettings.IsWaitListEnabled,
                InstanceName = context.RegistrationSettings.Name,
                PluralRegistrationTerm = pluralRegistrationTerm,
                AmountDueToday = amountDueToday,
                InitialAmountToPay = initialAmountToPay,
                HasDiscountsAvailable = hasDiscountsAvailable,
                RedirectGatewayUrl = redirectGatewayUrl,
                LoginRequiredToRegister = context.RegistrationSettings.IsLoginRequired,
                Session = session,
                IsUnauthorized = isUnauthorized,
                SuccessViewModel = successViewModel,
                CurrentPersonFamilyGuid = RequestContext.CurrentPerson?.PrimaryFamily?.Guid,
                TimeoutMinutes = timeoutMinutes,
                AllowRegistrationUpdates = allowRegistrationUpdates,
                IsExistingRegistration = isExistingRegistration,
                StartAtBeginning = startAtBeginning,
                GatewayGuid = financialGateway?.Guid,
                Campuses = campusClientService.GetCampusesAsListItems(),
                MaritalStatuses = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_MARITAL_STATUS )
                    .DefinedValues
                    .Where( v => v.IsActive )
                    .OrderBy( v => v.Order )
                    .Select( v => new ListItemBag
                    {
                        Value = v.Guid.ToString(),
                        Text = v.Value
                    } )
                    .ToList(),
                ConnectionStatuses = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_CONNECTION_STATUS )
                    .DefinedValues
                    .Where( v => v.IsActive )
                    .OrderBy( v => v.Order )
                    .Select( v => new ListItemBag
                    {
                        Value = v.Guid.ToString(),
                        Text = v.Value
                    } )
                    .ToList(),
                Grades = DefinedTypeCache.Get( SystemGuid.DefinedType.SCHOOL_GRADES )
                    .DefinedValues
                    .Where( v => v.IsActive )
                    .OrderBy( v => v.Order )
                    .Select( v => new ListItemBag
                    {
                        Value = v.Guid.ToString(),
                        Text = v.GetAttributeValue( "Abbreviation" )
                    } )
                    .ToList(),
                Races = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_RACE )
                    .DefinedValues
                    .OrderBy( v => v.Order )
                    .Select( v => new ListItemBag
                    {
                        Value = v.Guid.ToString(),
                        Text = v.Value
                    } )
                    .ToList(),
                Ethnicities = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_ETHNICITY )
                    .DefinedValues
                    .OrderBy( v => v.Order )
                    .Select( v => new ListItemBag
                    {
                        Value = v.Guid.ToString(),
                        Text = v.Value
                    } )
                    .ToList(),

                EnableSaveAccount = enableSavedAccount,
                SavedAccounts = savedAccounts,
                DisableCaptchaSupport = GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean(),

                // Payment plan
                IsPaymentPlanAllowed = isPaymentPlanAllowed,
                PaymentDeadlineDate = isPaymentPlanAllowed ? context.RegistrationSettings.PaymentDeadlineDate : null,
                PaymentPlanFrequencies = isPaymentPlanAllowed ? GetPaymentPlanFrequencyListItemBags( context.RegistrationSettings.PaymentPlanFrequencyValueIds, rockContext ) : null,
                IsPaymentPlanConfigured = context.Registration?.IsPaymentPlanActive ?? false,

                // Currency Code
                CurrencyInfo = new CurrencyInfoBag
                {
                    DecimalPlaces = currencyInfo.DecimalPlaces,
                    Symbol = currencyInfo.Symbol,
                    SymbolLocation = currencyInfo.SymbolLocation,
                },
            };

            if ( context.RegistrationSettings.SignatureDocumentTemplateId.HasValue && context.RegistrationSettings.IsInlineSignatureRequired )
            {
                var documentTemplate = new SignatureDocumentTemplateService( rockContext ).Get( context.RegistrationSettings.SignatureDocumentTemplateId.Value );

                if ( documentTemplate != null && !documentTemplate.IsLegacyProvider() )
                {
                    viewModel.IsInlineSignatureRequired = context.RegistrationSettings.IsInlineSignatureRequired;
                    viewModel.IsSignatureDrawn = context.RegistrationSettings.IsSignatureDrawn;
                    viewModel.SignatureDocumentTerm = context.RegistrationSettings.SignatureDocumentTerm;
                    viewModel.SignatureDocumentTemplateName = context.RegistrationSettings.SignatureDocumentTemplateName;
                }
            }

            return viewModel;
        }

        /// <summary>
        /// Gets the payment plan frequency list item bags.
        /// </summary>
        /// <param name="paymentPlanFrequencyValueIds">The payment plan frequency value ids.</param>
        /// <returns></returns>
        private List<ListItemBag> GetPaymentPlanFrequencyListItemBags( List<int> paymentPlanFrequencyValueIds, RockContext rockContext )
        {
            var frequencies = new Dictionary<int, ListItemBag>();

            foreach ( var paymentPlanFrequencyId in paymentPlanFrequencyValueIds )
            {
                if ( !frequencies.ContainsKey( paymentPlanFrequencyId ) )
                {
                    var frequency = DefinedValueCache.Get( paymentPlanFrequencyId, rockContext );
                    if ( frequency != null )
                    {
                        frequencies.Add( paymentPlanFrequencyId, frequency.ToListItemBag() );
                    }
                }
            }

            return frequencies.Values.ToList();
        }

        /// <summary>
        /// Generates the redirect URL.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="context">The context.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="registrar">The registrar.</param>
        /// <param name="registrants">The registrants.</param>
        /// <param name="registrationSessionGuid">The registration session unique identifier.</param>
        /// <param name="returnUrl">The URL to return to after payment has been made.</param>
        /// <returns>A string that contains the URL the individual should be sent to in order to make payments.</returns>
        private string GenerateRedirectUrl(
            RockContext rockContext,
            RegistrationContext context,
            decimal amount,
            RegistrarBag registrar,
            List<ViewModels.Blocks.Event.RegistrationEntry.RegistrantBag> registrants,
            Guid registrationSessionGuid,
            string returnUrl )
        {
            var financialGatewayId = context.RegistrationSettings.FinancialGatewayId;
            var fundId = context.RegistrationSettings.ExternalGatewayFundId;

            if ( financialGatewayId is null )
            {
                return null;
            }

            var financialGateway = new FinancialGatewayService( rockContext ).GetNoTracking( financialGatewayId.Value );
            var gatewayComponent = financialGateway?.GetGatewayComponent();
            var redirectGateway = gatewayComponent as IRedirectionGatewayComponent;

            if ( redirectGateway is null )
            {
                return null;
            }

            var registrantNames = registrants.Select( r => GetRegistrantFullName( context, r ) ).JoinStringsWithCommaAnd();
            var registrarName = $"{registrar.NickName} {registrar.LastName}";

            return redirectGateway.GetPaymentRedirectUrl( fundId, amount, returnUrl, new Dictionary<string, string>
            {
                { "ReturnToken", $"{ReturnUrlSessionPrefix}:{registrationSessionGuid}" },
                { "FirstName", registrar.NickName },
                { "LastName", registrar.LastName },
                { "EmailAddress", registrar.Email },
                { "RegistrationSessionGuid", registrationSessionGuid.ToString() },
                { "Note", $"Event registration for {context.RegistrationSettings.Name} for {registrantNames} by {registrarName}" }
            } );
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

        private ReferencePaymentInfo GetPaymentInfo(
            RockContext rockContext,
            RegistrationContext context,
            RegistrationEntryArgsBag args,
            GatewayComponent gateway,
            out string errorMessage )
        {
            errorMessage = string.Empty;

            var financialAccount = new FinancialAccountService( rockContext ).Get( context.RegistrationSettings.FinancialAccountId ?? 0 );
            if ( financialAccount == null )
            {
                errorMessage = "There was a problem with the financial account configuration for this registration instance";
                return null;
            }

            var comment = gateway is IRedirectionGatewayComponent
                ? context.RegistrationSettings.Name
                : $"{context.RegistrationSettings.Name} ({financialAccount.GlCode})";

            ReferencePaymentInfo paymentInfo;

            // Get the payment info from either the saved account or the gateway
            // token when using a new payment method.
            if ( args.SavedAccountGuid.HasValue && RequestContext.CurrentPerson != null )
            {
                var savedAccount = new FinancialPersonSavedAccountService( rockContext )
                    .Queryable()
                    .Where( a => a.Guid == args.SavedAccountGuid.Value
                        && a.PersonAlias.PersonId == RequestContext.CurrentPerson.Id )
                    .AsNoTracking()
                    .FirstOrDefault();

                if ( savedAccount != null )
                {
                    paymentInfo = savedAccount.GetReferencePayment();
                }
                else
                {
                    errorMessage = "There was a problem retrieving the saved account";
                    return null;
                }
            }
            else
            {
                paymentInfo = new ReferencePaymentInfo
                {
                    ReferenceNumber = args.GatewayToken,
                };
            }

            // Update payment into with details about this payment.
            paymentInfo.Amount = args.AmountToPayNow;
            paymentInfo.Email = args.Registrar.Email;
            paymentInfo.FirstName = args.Registrar.NickName;
            paymentInfo.LastName = args.Registrar.LastName;
            paymentInfo.Comment1 = comment;
            paymentInfo.TransactionTypeValueId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION ) ).Id;

            return paymentInfo;
        }

        private ReferencePaymentInfo GetPaymentPlanPaymentInfo(
            RockContext rockContext,
            RegistrationContext context,
            RegistrationEntryArgsBag args,
            GatewayComponent gateway,
            ReferencePaymentInfo previousPaymentInfo,
            out string errorMessage )
        {
            errorMessage = string.Empty;

            var financialAccount = new FinancialAccountService( rockContext ).Get( context.RegistrationSettings.FinancialAccountId ?? 0 );
            if ( financialAccount == null )
            {
                errorMessage = "There was a problem with the financial account configuration for this registration instance";
                return null;
            }

            var comment = gateway is IRedirectionGatewayComponent
                ? context.RegistrationSettings.Name
                : $"{context.RegistrationSettings.Name} ({financialAccount.GlCode})";

            ReferencePaymentInfo paymentInfo;

            // Get the payment info from either the saved account or the gateway
            // token when using a new payment method.
            if ( args.SavedAccountGuid.HasValue && RequestContext.CurrentPerson != null )
            {
                var savedAccount = new FinancialPersonSavedAccountService( rockContext )
                    .Queryable()
                    .Where( a => a.Guid == args.SavedAccountGuid.Value
                        && a.PersonAlias.PersonId == RequestContext.CurrentPerson.Id )
                    .AsNoTracking()
                    .FirstOrDefault();

                if ( savedAccount != null )
                {
                    paymentInfo = savedAccount.GetReferencePayment();
                }
                else
                {
                    errorMessage = "There was a problem retrieving the saved account";
                    return null;
                }
            }
            else
            {
                paymentInfo = new ReferencePaymentInfo
                {
                    ReferenceNumber = args.GatewayToken,
                };
            }

            // Update payment into with details about this payment.
            paymentInfo.Amount = args.PaymentPlan.AmountPerPayment;
            paymentInfo.Email = args.Registrar.Email;
            paymentInfo.FirstName = args.Registrar.NickName;
            paymentInfo.LastName = args.Registrar.LastName;
            paymentInfo.Comment1 = comment;
            paymentInfo.TransactionTypeValueId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION ) ).Id;

            // Copy information from a previous payment.
            if ( previousPaymentInfo != null )
            {
                paymentInfo.GatewayPersonIdentifier = previousPaymentInfo.GatewayPersonIdentifier;
                paymentInfo.AdditionalParameters = new Dictionary<string, string>( previousPaymentInfo.AdditionalParameters );
            }

            return paymentInfo;
        }

        /// <summary>
        /// Processes the payment.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="context">The context.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private FinancialTransaction ProcessGatewayPayment(
            RockContext rockContext,
            RegistrationContext context,
            RegistrationEntryArgsBag args,
            FinancialGateway financialGateway,
            GatewayComponent gateway,
            ReferencePaymentInfo paymentInfo,
            out string errorMessage )
        {
            errorMessage = string.Empty;

            if ( gateway == null )
            {
                errorMessage = "There was a problem creating the payment gateway information";
                return null;
            }

            FinancialTransaction transaction;

            if ( gateway is IRedirectionGatewayComponent redirectionGateway )
            {
                // Download the payment from the redirect gateway
                var fundId = context.RegistrationSettings.ExternalGatewayFundId;
                transaction = redirectionGateway.FetchPaymentTokenTransaction( rockContext, financialGateway, fundId, args.GatewayToken );
                paymentInfo.Amount = transaction.TotalAmount;
            }
            else if ( gateway is IObsidianHostedGatewayComponent obsidianGateway )
            {
                if ( paymentInfo.GatewayPersonIdentifier.IsNullOrWhiteSpace() )
                {
                    var customerToken = obsidianGateway.CreateCustomerAccount( financialGateway, paymentInfo, out errorMessage );

                    if ( !errorMessage.IsNullOrWhiteSpace() )
                    {
                        return null;
                    }

                    paymentInfo.GatewayPersonIdentifier = customerToken;
                }

                if ( args.GatewayToken.IsNotNullOrWhiteSpace() && obsidianGateway.IsPaymentTokenCharged( financialGateway, args.GatewayToken ) )
                {
                    // Download the existing payment from the gateway.
                    var fundId = context.RegistrationSettings.ExternalGatewayFundId;

                    transaction = obsidianGateway.FetchPaymentTokenTransaction( rockContext, financialGateway, fundId, args.GatewayToken );
                    paymentInfo.Amount = transaction.TotalAmount;
                }
                else
                {
                    // Charge a new payment with the tokenized payment method
                    transaction = gateway.Charge( financialGateway, paymentInfo, out errorMessage );
                }

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return null;
                }
            }
            else
            {
                // Charge a new payment with the tokenized payment method
                transaction = gateway.Charge( financialGateway, paymentInfo, out errorMessage );
            }

            return transaction;
        }

        private FinancialScheduledTransaction ProcessGatewayPaymentPlan(
            RegistrationContext context,
            RegistrationEntryArgsBag args,
            FinancialGateway financialGateway,
            GatewayComponent gateway,
            PaymentSchedule paymentSchedule,
            ReferencePaymentInfo paymentInfo,
            out string errorMessage )
        {
            if ( args.PaymentPlan == null )
            {
                errorMessage = "There was a problem creating the payment plan";
                return null;
            }

            if ( !( gateway is IScheduledNumberOfPaymentsGateway ) )
            {
                errorMessage = "Payment plans are not supported";
                return null;
            }

            if ( context.Registration?.IsPaymentPlanActive == true )
            {
                errorMessage = "A payment plan is already configured";
                return null;
            }

            if ( gateway is IObsidianHostedGatewayComponent obsidianGateway && paymentInfo.GatewayPersonIdentifier.IsNullOrWhiteSpace() )
            {
                var customerToken = obsidianGateway.CreateCustomerAccount( financialGateway, paymentInfo, out errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return null;
                }

                paymentInfo.GatewayPersonIdentifier = customerToken;
            }

            // Schedule the recurring payment in the financial gateway.
            // The scheduled transaction returned is partially filled out with info needed in Rock,
            // but Rock still needs to fill out the transaction details before saving.
            var scheduledTransaction = gateway.AddScheduledPayment( financialGateway, paymentSchedule, paymentInfo, out errorMessage );

            if ( scheduledTransaction == null )
            {
                errorMessage = "There was a problem scheduling the payment";
                return null;
            }

            // Associate the payment plan scheduled transaction with the registration.
            context.Registration.PaymentPlanFinancialScheduledTransaction = scheduledTransaction;

            return scheduledTransaction;
        }

        /// <summary>
        /// Saves the transaction.
        /// </summary>
        /// <param name="gateway">The gateway.</param>
        /// <param name="context">The context.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Guid? SaveTransaction(
            GatewayComponent gateway,
            RegistrationContext context,
            FinancialTransaction transaction,
            PaymentInfo paymentInfo,
            RockContext rockContext )
        {
            if ( transaction is null )
            {
                return null;
            }

            var currentPerson = GetCurrentPerson();

            transaction.AuthorizedPersonAliasId = context.Registration.PersonAliasId;
            transaction.TransactionDateTime = RockDateTime.Now;
            transaction.FinancialGatewayId = context.RegistrationSettings.FinancialGatewayId;

            var txnType = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION ) );
            transaction.TransactionTypeValueId = txnType.Id;

            if ( transaction.FinancialPaymentDetail == null )
            {
                transaction.FinancialPaymentDetail = new FinancialPaymentDetail();
            }

            /* 02/17/2022 MDP
            Note that after the transaction, the HostedGateway knows more about the FinancialPaymentDetail than Rock does
            since it is the gateway that collects the payment info. But just in case paymentInfo has information the the gateway hasn't set,
            we'll fill in any missing details.
            But then we'll want to use FinancialPaymentDetail as the most accurate values for the payment info. 
            */

            if ( paymentInfo != null )
            {
                transaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, gateway, rockContext );
            }

            var currencyTypeValue = transaction.FinancialPaymentDetail?.CurrencyTypeValueId != null
                ? DefinedValueCache.Get( transaction.FinancialPaymentDetail.CurrencyTypeValueId.Value )
                : null;

            var creditCardTypeValue = transaction.FinancialPaymentDetail?.CreditCardTypeValueId != null
                ? DefinedValueCache.Get( transaction.FinancialPaymentDetail.CreditCardTypeValueId.Value )
                : null;

            Guid sourceGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( AttributeKey.Source ), out sourceGuid ) )
            {
                var source = DefinedValueCache.Get( sourceGuid );
                if ( source != null )
                {
                    transaction.SourceTypeValueId = source.Id;
                }
            }

            transaction.Summary = context.Registration.GetSummary();

            var transactionDetail = transaction.TransactionDetails.FirstOrDefault();
            if ( transactionDetail == null )
            {
                transactionDetail = new FinancialTransactionDetail();
                transaction.TransactionDetails.Add( transactionDetail );
            }

            transactionDetail.Amount = paymentInfo.Amount;
            transactionDetail.AccountId = context.RegistrationSettings.FinancialAccountId ?? transactionDetail.AccountId;
            transactionDetail.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Registration ) ).Id;
            transactionDetail.EntityId = context.Registration.Id;

            var batchChanges = new History.HistoryChangeList();

            rockContext.WrapTransaction( () =>
            {
                var batchService = new FinancialBatchService( rockContext );

                // determine batch prefix
                var batchPrefix = context.RegistrationSettings.BatchNamePrefix.IsNullOrWhiteSpace() ?
                    GetAttributeValue( AttributeKey.BatchNamePrefix ) :
                    context.RegistrationSettings.BatchNamePrefix;

                // Get the batch
                var batch = batchService.GetForNewTransaction( transaction, batchPrefix );
                FinancialBatchService.EvaluateNewBatchHistory( batch, batchChanges );

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
                    context.Registration.Id,
                    registrationChanges,
                    true,
                    currentPerson?.PrimaryAliasId ) );

            context.TransactionCode = transaction.TransactionCode;
            context.GatewayPersonIdentifier = ( paymentInfo as ReferencePaymentInfo )?.GatewayPersonIdentifier;
            return transaction.Guid;
        }

        /// <summary>
        /// This method was copied from UtilityPaymentEntry.SaveScheduledTransaction and repurposed for saving a scheduled recurring
        /// transaction for event registration when the registrar opts for a payment plan.
        /// </summary>
        /// <param name="financialGatewayId"></param>
        /// <param name="gateway"></param>
        /// <param name="context"></param>
        /// <param name="paymentInfo"></param>
        /// <param name="paymentSchedule"></param>
        /// <param name="scheduledTransaction"></param>
        /// <param name="rockContext"></param>
        private void PrepareAndSavePaymentPlanScheduledTransaction(
            RockContext rockContext,
            RegistrationContext context,
            FinancialGateway financialGateway,
            GatewayComponent gateway,
            PaymentSchedule paymentSchedule,
            ReferencePaymentInfo paymentInfo,
            FinancialScheduledTransaction scheduledTransaction )
        {
            // Set basic information needed by Rock.
            scheduledTransaction.NumberOfPayments = paymentSchedule.NumberOfPayments;
            scheduledTransaction.AuthorizedPersonAliasId = context.Registration.PersonAliasId.Value;
            scheduledTransaction.FinancialGatewayId = financialGateway.Id;
            scheduledTransaction.TransactionTypeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_EVENT_REGISTRATION.AsGuid() );
            var sourceGuid = GetAttributeValue( AttributeKey.Source ).AsGuidOrNull();
            if ( sourceGuid.HasValue )
            {
                scheduledTransaction.SourceTypeValueId = DefinedValueCache.GetId( sourceGuid.Value );
            }

            // Set the schedule information.
            scheduledTransaction.TransactionFrequencyValueId = paymentSchedule.TransactionFrequencyValue.Id;
            scheduledTransaction.StartDate = paymentSchedule.StartDate;
            
            // Set the payment information.
            scheduledTransaction.Summary = context.Registration.GetSummary();
            if ( scheduledTransaction.FinancialPaymentDetail == null )
            {
                scheduledTransaction.FinancialPaymentDetail = new FinancialPaymentDetail();
            }
            scheduledTransaction.FinancialPaymentDetail.SetFromPaymentInfo( paymentInfo, gateway, rockContext );
            
            // Use the details from the gateway if it added one;
            // otherwise, create the details here.
            var transactionDetail = scheduledTransaction.ScheduledTransactionDetails.FirstOrDefault();
            if ( transactionDetail == null )
            {
                transactionDetail = new FinancialScheduledTransactionDetail();
                scheduledTransaction.ScheduledTransactionDetails.Add( transactionDetail );
            }

            // Add the individual payment details. For the payment plan, it will be a single value for the total amount of the plan.
            transactionDetail.Amount = paymentInfo.Amount;
            transactionDetail.AccountId = context.RegistrationSettings.FinancialAccountId ?? transactionDetail.AccountId;
            transactionDetail.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Registration ) ).Id;
            transactionDetail.EntityId = context.Registration.Id;

            // Save the scheduled transaction to the DB.
            var transactionService = new FinancialScheduledTransactionService( rockContext );
            transactionService.Add( scheduledTransaction );

            rockContext.SaveChanges();

            // TODO Should message be published to event bus? No. As indicated by the class name, this event is exclusively used for non-event giving (or real "gift" giving).
            //Task.Run( () => Rock.Bus.Message.ScheduledGiftWasModifiedMessage.PublishScheduledTransactionEvent( scheduledTransaction.Id, Rock.Bus.Message.ScheduledGiftEventTypes.ScheduledGiftCreated ) );
        }

        /// <summary>
        /// Gets the success view model.
        /// </summary>
        /// <param name="registrationId">The registration identifier.</param>
        /// <param name="transactionCode">The transaction code.</param>
        /// <param name="gatewayPersonIdentifier">The gateway person identifier.</param>
        /// <returns></returns>
        private RegistrationEntrySuccessBag GetSuccessViewModel( int registrationId, string transactionCode, string gatewayPersonIdentifier )
        {
            // Create a view model with default values in case anything goes wrong
            var viewModel = new RegistrationEntrySuccessBag
            {
                TitleHtml = "Congratulations",
                MessageHtml = "You have successfully completed this registration.",
                TransactionCode = transactionCode,
                GatewayPersonIdentifier = gatewayPersonIdentifier,
                SpotsRemaining = 0,
                RegisteredCount = 0,
                WaitListedCount = 0,
            };

            try
            {
                var rockContext = new RockContext();
                var registration = new RegistrationService( rockContext )
                    .Queryable()
                    .Include( r => r.RegistrationInstance.RegistrationTemplate )
                    .Include( r => r.PaymentPlanFinancialScheduledTransaction.TransactionFrequencyValue )
                    // Needed to get the TotalAmount run-time value.
                    .Include( r => r.PaymentPlanFinancialScheduledTransaction.ScheduledTransactionDetails )
                    .FirstOrDefault( r => r.Id == registrationId );

                if ( registration != null &&
                    registration.RegistrationInstance != null &&
                    registration.RegistrationInstance.RegistrationTemplate != null )
                {
                    var template = registration.RegistrationInstance.RegistrationTemplate;

                    var mergeFields = this.RequestContext.GetCommonMergeFields();
                    mergeFields.Add( "RegistrationInstance", registration.RegistrationInstance );
                    mergeFields.Add( "Registration", registration );

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
                    
                    if ( registration.RegistrationInstance.MaxAttendees.HasValue )
                    {
                        var context = GetContext( rockContext, out var errorMessage );

                        if ( context != null )
                        {
                            viewModel.SpotsRemaining = context.SpotsRemaining;
                        }
                    }

                    viewModel.RegisteredCount = registration.Registrants.Count( r => !r.OnWaitList );
                    viewModel.WaitListedCount = registration.Registrants.Count( r => r.OnWaitList );
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

        /// <summary>
        /// Gets the allowed currency types supported by both the block and the
        /// financial gateway.
        /// </summary>
        /// <param name="gatewayComponent">The gateway component that must support the currency types.</param>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <returns>A list of <see cref="DefinedValueCache"/> objects that represent the currency types.</returns>
        private List<DefinedValueCache> GetAllowedCurrencyTypes( GatewayComponent gatewayComponent, FinancialGateway financialGateway )
        {
            var allowedCurrencyTypes = new List<DefinedValueCache>();
            var enableACH = this.GetAttributeValue( AttributeKey.EnableACH ).AsBoolean();
            var enableCreditCard = this.GetAttributeValue( AttributeKey.EnableCreditCard ).AsBoolean();

            // Conditionally enable credit card.
            var creditCardCurrency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() );
            if ( enableCreditCard && gatewayComponent.SupportsSavedAccount( creditCardCurrency ) )
            {
                allowedCurrencyTypes.Add( creditCardCurrency );
            }

            // Conditionally enable ACH.
            var achCurrency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH.AsGuid() );
            if ( enableACH && gatewayComponent.SupportsSavedAccount( achCurrency ) )
            {
                allowedCurrencyTypes.Add( achCurrency );
            }

            var applePayCurrency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_APPLE_PAY.AsGuid() );
            if ( gatewayComponent.SupportsSavedAccount( applePayCurrency ) )
            {
                allowedCurrencyTypes.Add( applePayCurrency );
            }

            var googlePayCurrency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ANDROID_PAY.AsGuid() );
            if ( gatewayComponent.SupportsSavedAccount( googlePayCurrency ) )
            {
                allowedCurrencyTypes.Add( googlePayCurrency );
            }

            return allowedCurrencyTypes;
        }

        /// <summary>
        /// Gets the registration session page parameter value from all possible sources.
        /// </summary>
        /// <returns>The session unique identifier or <c>null</c> if it could not be obtained.</returns>
        private Guid? GetRegistrationSessionPageParameter( PageReference pageReference )
        {
            var sessionGuid = pageReference != null
                ? pageReference.GetPageParameter( PageParameterKey.RegistrationSessionGuid ).AsGuidOrNull()
                : PageParameter( PageParameterKey.RegistrationSessionGuid ).AsGuidOrNull();

            if ( sessionGuid.HasValue )
            {
                return sessionGuid;
            }

            var pageParameters = pageReference != null
                ? pageReference.GetPageParameters()
                : RequestContext.GetPageParameters();

            var prefixedSessionValue = pageParameters
                .Select( k => k.Value )
                .Where( v => v != null && v.StartsWith( ReturnUrlSessionPrefix ) )
                .FirstOrDefault();

            if ( prefixedSessionValue == null )
            {
                return null;
            }

            return prefixedSessionValue.Substring( ReturnUrlSessionPrefix.Length + 1 ).AsGuidOrNull();
        }

        /// <summary>
        /// Gets the registration instance identifier.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private int GetRegistrationInstanceId( RockContext rockContext )
        {
            return GetRegistrationInstanceId( rockContext, null );
        }

        /// <summary>
        /// Gets the registration instance identifier.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="pageReference">The page reference to use when accessing page parameters.</param>
        /// <returns></returns>
        private int GetRegistrationInstanceId( RockContext rockContext, PageReference pageReference )
        {
            string registrationParameter;
            string registrationInstanceParameter;
            string slugParameter;

            if ( pageReference != null )
            {
                registrationParameter = pageReference.GetPageParameter( PageParameterKey.RegistrationId );
                registrationInstanceParameter = pageReference.GetPageParameter( PageParameterKey.RegistrationInstanceId );
                slugParameter = pageReference.GetPageParameter( PageParameterKey.Slug );
            }
            else
            {
                registrationParameter = PageParameter( PageParameterKey.RegistrationId );
                registrationInstanceParameter = PageParameter( PageParameterKey.RegistrationInstanceId );
                slugParameter = PageParameter( PageParameterKey.Slug );
            }

            // The page param is the least costly since there is no database call, so try that first
            var registrationInstanceId = registrationInstanceParameter.AsIntegerOrNull();

            if ( registrationInstanceId.HasValue )
            {
                return registrationInstanceId.Value;
            }

            // Try a session. This is typically from a redirect
            var registrationSession = GetRegistrationSessionQuery( rockContext, pageReference )
                .AsNoTracking()
                .Select( rs => new
                {
                    rs.RegistrationInstanceId
                } )
                .FirstOrDefault();

            if ( registrationSession != null )
            {
                return registrationSession.RegistrationInstanceId;
            }

            // Try a url slug
            if ( !slugParameter.IsNullOrWhiteSpace() )
            {
                var linkage = new EventItemOccurrenceGroupMapService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( l =>
                        l.UrlSlug == slugParameter &&
                        l.RegistrationInstanceId.HasValue )
                    .Select( l => new
                    {
                        RegistrationInstanceId = l.RegistrationInstanceId.Value
                    } )
                    .FirstOrDefault();

                if ( linkage != null )
                {
                    return linkage.RegistrationInstanceId;
                }
            }

            // Try the registration id
            var registrationId = registrationParameter.AsIntegerOrNull();

            if ( registrationId.HasValue )
            {
                var registration = new RegistrationService( rockContext )
                    .Queryable()
                    .Where( r => r.Id == registrationId.Value )
                    .Select( r => new
                    {
                        r.RegistrationInstanceId
                    } )
                    .FirstOrDefault();

                if ( registration != null )
                {
                    return registration.RegistrationInstanceId;
                }
            }

            // The instance id is unknown
            return default;
        }

        /// <summary>
        /// Gets the registration entry block arguments if this is an existing registration.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="registrationContext">The registration context.</param>
        /// <returns></returns>
        private RegistrationEntrySessionBag GetRegistrationEntryBlockSession( RockContext rockContext, RegistrationContext registrationContext )
        {
            // Try to restore the session from the RegistrationSessionGuid, which is typically a PushPay redirect
            var registrationSessionGuid = GetRegistrationSessionPageParameter( null );

            var registrationSession = GetRegistrationSessionQuery( rockContext )
                .AsNoTracking()
                .Select( rs => new
                {
                    rs.RegistrationData
                } )
                .FirstOrDefault();

            if ( registrationSession != null )
            {
                return registrationSession.RegistrationData.FromJsonOrNull<RegistrationEntrySessionBag>();
            }

            // Try to restore the session from an existing registration
            var currentPerson = GetCurrentPerson();
            var registrationId = PageParameter( PageParameterKey.RegistrationId ).AsIntegerOrNull();

            if ( registrationId is null || currentPerson is null )
            {
                return null;
            }

            var authorizedAliasIds = currentPerson.Aliases?.Select( a => a.Id ).ToList();

            if ( authorizedAliasIds?.Any() != true )
            {
                return null;
            }

            // Query for a registration that matches the ID and is owned or was created by the current person
            var registrationService = new RegistrationService( rockContext );
            var registrationPaymentPlanPair = registrationService
                .Queryable()
                .Include( r => r.Registrants )
                .Include( r => r.Registrants.Select( registrants => registrants.PersonAlias.Person ) )
                .Include( r => r.Registrants.Select( registrants => registrants.Fees ) )
                .AsNoTracking()
                .Where( r =>
                    r.Id == registrationId.Value && (
                        ( r.PersonAliasId.HasValue && authorizedAliasIds.Contains( r.PersonAliasId.Value ) ) ||
                        ( r.CreatedByPersonAliasId.HasValue && authorizedAliasIds.Contains( r.CreatedByPersonAliasId.Value ) )
                    ) &&
                    r.RegistrationInstanceId == registrationContext.RegistrationSettings.RegistrationInstanceId )
                .SelectPaymentPlanPairs()
                .FirstOrDefault();

            var registration = registrationPaymentPlanPair?.Registration;
            var activePaymentPlan = registrationPaymentPlanPair?.PaymentPlan?.IsActive == true ? registrationPaymentPlanPair.PaymentPlan : null;

            if ( registration is null )
            {
                return null;
            }

            var alreadyPaid = registrationService.GetTotalPayments( registration.Id );

            var balanceDue = ( registration.DiscountedCost - alreadyPaid ).AsCurrency();

            if ( balanceDue < 0 )
            {
                balanceDue = 0;
            }

            // Create the base args data
            var session = new RegistrationEntrySessionBag
            {
                RegistrationSessionGuid = Guid.NewGuid(),
                AmountToPayNow = balanceDue,
                DiscountCode = registration.DiscountCode,
                DiscountAmount = registration.DiscountAmount,
                DiscountPercentage = registration.DiscountPercentage,
                FieldValues = new Dictionary<Guid, object>(),
                GatewayToken = string.Empty,
                Registrants = new List<RegistrantBag>(),
                Registrar = new RegistrarBag(),
                RegistrationGuid = registration.Guid,
                ActivePaymentPlan = activePaymentPlan?.AsRegistrationPaymentPlanBag(),
                PreviouslyPaid = alreadyPaid,
                Slug = PageParameter( PageParameterKey.Slug ),
                GroupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull()
            };

            // Add attributes about the registration itself
            var registrationAttributes = GetRegistrationAttributes( registrationContext.RegistrationSettings.RegistrationTemplateId );
            registration.LoadAttributes( rockContext );

            foreach ( var attribute in registrationAttributes )
            {
                var value = registration.GetAttributeValue( attribute.Key );
                value = PublicAttributeHelper.GetPublicValueForEdit( attribute, value );

                session.FieldValues[attribute.Guid] = value;
            }

            // Add information about the registrants
            foreach ( var registrant in registration.Registrants )
            {
                var person = registrant.PersonAlias?.Person;
                person.LoadAttributes( rockContext );
                registrant.LoadAttributes( rockContext );

                var registrantInfo = new ViewModels.Blocks.Event.RegistrationEntry.RegistrantBag
                {
                    FamilyGuid = person?.GetFamily( rockContext )?.Guid,
                    Guid = registrant.Guid,
                    PersonGuid = person?.Guid,
                    FieldValues = GetCurrentValueFieldValues( registrationContext, rockContext, person, registrant, registrationContext.RegistrationSettings.Forms, true ),
                    FeeItemQuantities = new Dictionary<Guid, int>(),
                    IsOnWaitList = registrant.OnWaitList,
                    Cost = registrant.Cost
                };

                // Person fields and person attribute fields are already loaded via GetCurrentValueFieldValues, but we still need
                // to get registrant attributes
                foreach ( var form in registrationContext.RegistrationSettings.Forms )
                {
                    var fields = form.Fields
                        .Where( f =>
                            !f.IsInternal &&
                            f.FieldSource == RegistrationFieldSource.RegistrantAttribute &&
                            ( f.Attribute == null || f.Attribute.IsActive ) &&
                            f.AttributeId.HasValue )
                        .ToList();

                    foreach ( var field in fields )
                    {
                        var attribute = AttributeCache.Get( field.AttributeId.Value );
                        var value = registration.GetAttributeValue( attribute.Key );
                        registrantInfo.FieldValues[attribute.Guid] = value;
                    }
                }

                // Add the fees
                foreach ( var fee in registrationContext.RegistrationSettings.Fees )
                {
                    foreach ( var feeItem in fee.FeeItems )
                    {
                        var registrantFee = registrant.Fees.FirstOrDefault( f => f.RegistrationTemplateFeeItemId == feeItem.Id );
                        if ( registrantFee == null && ( !fee.IsActive || !feeItem.IsActive ) )
                        {
                            // If this fee or fee item is not currently active, only add it to this registrant info
                            // if they already have a record of it. This means the fee or item was active when the
                            // registrant was added, so it still applies to them.
                            continue;
                        }

                        var quantity = registrantFee?.Quantity ?? 0;
                        registrantInfo.FeeItemQuantities[feeItem.Guid] = quantity;
                    }
                }

                session.Registrants.Add( registrantInfo );
            }

            return session;
        }

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private RegistrationContext GetContext( RockContext rockContext, RegistrationEntryArgsBag args, out string errorMessage )
        {
            var currentPerson = GetCurrentPerson();
            var registrationInstanceId = GetRegistrationInstanceId( rockContext );
            var registrationService = new RegistrationService( rockContext );
            var disableCaptcha = GetAttributeValue( AttributeKey.DisableCaptchaSupport ).AsBoolean() || string.IsNullOrWhiteSpace( SystemSettings.GetValue( SystemKey.SystemSetting.CAPTCHA_SITE_KEY ) );

            // Basic check on the args to see that they appear valid
            if ( args == null )
            {
                errorMessage = "The args cannot be null";
                return null;
            }

            if ( args.Registrants?.Any() != true )
            {
                errorMessage = "At least one registrant is required";
                return null;
            }

            if ( args.Registrar == null )
            {
                errorMessage = "A registrar is required";
                return null;
            }

            var context = registrationService.GetRegistrationContext( registrationInstanceId, args.RegistrationGuid, currentPerson, args.DiscountCode, out errorMessage );
            if ( context == null )
            {
                return null;
            }

            // Validate the amount to pay today
            var amountDue = CalculateTotalAmountDue( rockContext, context, args );

            // Cannot pay less than 0
            if ( args.AmountToPayNow < 0 )
            {
                args.AmountToPayNow = 0;
            }

            var isNewRegistration = context.Registration == null;

            // Validate the charge amount is not too low according to the initial payment amount
            if ( isNewRegistration && amountDue > 0 )
            {
                var minimumInitialPayment = context.RegistrationSettings.PerRegistrantMinInitialPayment.HasValue
                    ? context.RegistrationSettings.PerRegistrantMinInitialPayment.Value * args.Registrants.Count
                    : amountDue;

                args.AmountToPayNow = args.AmountToPayNow < minimumInitialPayment ? minimumInitialPayment : args.AmountToPayNow;
            }

            // Cannot pay more than is owed. This check should be the last one performed regarding payment in this method.
            args.AmountToPayNow = args.AmountToPayNow > amountDue ? amountDue : args.AmountToPayNow;

            return context;
        }

        /// <summary>
        /// Calculates the total amount still due on the registration. This takes
        /// into account all costs, fees, discounts and payments already applied.
        /// </summary>
        /// <param name="rockContext">The Rock database context to operate in when loading data.</param>
        /// <param name="context">The registration context that describes the registration details.</param>
        /// <param name="args">The arguments that describe the current registration request.</param>
        /// <returns>The amount still due in dollars and cents.</returns>
        private static decimal CalculateTotalAmountDue( RockContext rockContext, RegistrationContext context, RegistrationEntryArgsBag args )
        {
            var registrationService = new RegistrationService( rockContext );
            var registrationInstanceService = new RegistrationInstanceService( rockContext );

            var costs = registrationInstanceService.GetRegistrationCostSummaryInfo( context, args.AsArgsOrNull() );
            var totalDiscountedCost = costs.Sum( c => c.DiscountedCost );

            if ( context.Registration != null )
            {
                var totalPayments = registrationService.GetTotalPayments( context.Registration.Id );
                totalDiscountedCost -= totalPayments;
            }

            return totalDiscountedCost;
        }

        /// <summary>
        /// Gets the context.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private RegistrationContext GetContext( RockContext rockContext, out string errorMessage )
        {
            var registrationInstanceId = GetRegistrationInstanceId( rockContext );
            var registrationService = new RegistrationService( rockContext );
            var registrationId = PageParameter( PageParameterKey.RegistrationId ).AsIntegerOrNull();

            // If the URL does not have a registrationId then check if there
            // is a registration session. Some redirect gateways drop the
            // RegistrationId parameter from the return URL. So we'll try
            // to get it from the session if we have one.
            if ( !registrationId.HasValue )
            {
                registrationId = GetRegistrationSessionQuery( rockContext )
                    .Select( s => s.RegistrationId )
                    .FirstOrDefault();
            }

            return registrationService.GetRegistrationContext( registrationInstanceId, registrationId, out errorMessage );
        }

        /// <summary>
        /// Gets the query that returns the <see cref="RegistrationSession"/> associated with the session page parameter.
        /// </summary>
        /// <param name="rockContext">The Rock context.</param>
        /// <param name="pageReference">The optional page reference used to get the session page parameter. If <see langword="null"/>, the session page parameter is retrieved from the current page.</param>
        /// <returns>The query that returns the <see cref="RegistrationSession"/> associated with the session page parameter.</returns>
        private IQueryable<RegistrationSession> GetRegistrationSessionQuery( RockContext rockContext, PageReference pageReference = null )
        {
            var sessionGuid = GetRegistrationSessionPageParameter( pageReference );

            if ( sessionGuid.HasValue )
            {
                return new RegistrationSessionService( rockContext )
                    .Queryable()
                    .Where( s => s.Guid == sessionGuid.Value )
                    .Take( 1 );
            }
            else
            {
                return Enumerable
                    .Empty<RegistrationSession>()
                    .AsQueryable();
            }
        }
             
        /// <summary>
        /// Sends notifications after the registration is saved
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="isNewRegistration">if set to <c>true</c> [is new registration].</param>
        /// <param name="registration">The registration.</param>
        /// <param name="previousRegistrantPersonIds">The previous registrant person ids.</param>
        /// <param name="postSaveActions">Additional actions to run during the post save process.</param>
        private void ProcessPostSave( RockContext rockContext, RegistrationSettings settings, RegistrationEntryArgsBag args, bool isNewRegistration, Registration registration, List<int> previousRegistrantPersonIds, List<Action> postSaveActions )
        {
            var currentPerson = GetCurrentPerson();
            var currentPersonAliasId = currentPerson?.PrimaryAliasId;

            if ( registration.PersonAlias != null && registration.PersonAlias.Person != null )
            {
                registration.SavePersonNotesAndHistory( registration.PersonAlias.Person, currentPersonAliasId, previousRegistrantPersonIds );
            }
            // This occurs when the registrar is logged in
            else if ( registration.PersonAliasId.HasValue )
            {
                var registrar = new PersonAliasService( rockContext ).Get( registration.PersonAliasId.Value );
                registration.SavePersonNotesAndHistory( registrar.Person, currentPersonAliasId, previousRegistrantPersonIds );
            }

            AddRegistrantsToGroup( rockContext, settings, registration, args );

            // Send/Resend a confirmation
            var processSendRegistrationConfirmationMsg = new ProcessSendRegistrationConfirmation.Message()
            {
                RegistrationId = registration.Id,
                AppRoot = RequestContext.RootUrlPath.EnsureTrailingForwardslash(),
                ThemeRoot = RequestContext.RootUrlPath + RequestContext.ResolveRockUrl( "~~/" )
            };

            processSendRegistrationConfirmationMsg.Send();

            if ( isNewRegistration )
            {
                // Send notice of a new registration
                new ProcessSendRegistrationNotification.Message
                {
                    RegistrationId = registration.Id,
                    AppRoot = RequestContext.RootUrlPath.EnsureTrailingForwardslash(),
                    ThemeRoot = RequestContext.RootUrlPath + RequestContext.ResolveRockUrl( "~~/" )
                }.Send();

                var registrationService = new RegistrationService( new RockContext() );
                var newRegistration = registrationService.Get( registration.Id );

                if ( newRegistration != null )
                {
                    foreach ( var item in newRegistration.Registrants.Where( r => r.PersonAlias != null && r.PersonAlias.Person != null ) )
                    {
                        var parameters = new Dictionary<string, string>();
                        parameters.Add( "RegistrationId", item.RegistrationId.ToString() );
                        parameters.Add( "RegistrationRegistrantId", item.Id.ToString() );
                        item.LaunchWorkflow( settings.RegistrantWorkflowTypeId, newRegistration.ToString(), parameters, null );
                    }

                    if ( settings.WorkflowTypeIds.Any() )
                    {
                        foreach ( var workflowTypeId in settings.WorkflowTypeIds )
                        {
                            newRegistration.LaunchWorkflow( workflowTypeId, newRegistration.ToString(), null, null );
                        }
                    }
                }
            }

            // Run all the additional post save actions.
            foreach ( var postSaveAction in postSaveActions )
            {
                try
                {
                    postSaveAction();
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( ex );
                }
            }
        }

        /// <summary>
        /// Builds a new <see cref="GroupMember"/> object that will be saved to
        /// the database later.
        /// </summary>
        /// <param name="person">The person that will be added to the group.</param>
        /// <param name="group">The group the person will be added to.</param>
        /// <param name="settings">The registration settings.</param>
        /// <returns>A new <see cref="GroupMember"/> instance.</returns>
        private GroupMember BuildGroupMember( Person person, Rock.Model.Group group, RegistrationSettings settings )
        {
            var groupMember = new GroupMember();
            groupMember.GroupId = group.Id;
            groupMember.Group = group;
            groupMember.PersonId = person.Id;
            groupMember.Person = person;
            groupMember.GroupMemberStatus = settings.GroupMemberStatus;

            if ( settings.GroupTypeId.HasValue &&
                settings.GroupTypeId == group.GroupTypeId &&
                settings.GroupMemberRoleId.HasValue )
            {
                groupMember.GroupRoleId = settings.GroupMemberRoleId.Value;
            }
            else
            {
                if ( group.GroupType.DefaultGroupRoleId.HasValue )
                {
                    groupMember.GroupRoleId = group.GroupType.DefaultGroupRoleId.Value;
                }
                else
                {
                    groupMember.GroupRoleId = group.GroupType.Roles.Select( r => r.Id ).FirstOrDefault();
                }
            }

            return groupMember;
        }

        /// <summary>
        /// Updates the group member attributes from the registrant information.
        /// </summary>
        /// <param name="groupMember">The group member to be updated.</param>
        /// <param name="registrantInfo">The registrant information.</param>
        /// <param name="settings">The registration settings.</param>
        /// <returns><c>true</c> if any attribute value was changed, <c>false</c> otherwise.</returns>
        private bool UpdateGroupMemberAttributes( GroupMember groupMember, RegistrantBag registrantInfo, RegistrationSettings settings )
        {
            var isChanged = false;
            var memberAttributeFields = settings.Forms
                .SelectMany( f => f.Fields
                    .Where( t =>
                        t.FieldSource == RegistrationFieldSource.GroupMemberAttribute &&
                        t.AttributeId.HasValue ) );

            foreach ( var field in memberAttributeFields )
            {
                // Find the registrant's value
                var fieldValue = registrantInfo.FieldValues
                    .Where( f => f.Key == field.Guid )
                    .Select( f => f.Value )
                    .FirstOrDefault();

                if ( fieldValue != null )
                {
                    var attribute = AttributeCache.Get( field.AttributeId.Value );
                    if ( attribute != null )
                    {
                        if ( IsFieldUnlockedForEditing( field, groupMember, attribute.Key ) )
                        {
                            var originalValue = groupMember.GetAttributeValue( attribute.Key );
                            var newValue = PublicAttributeHelper.GetPrivateValue( attribute, fieldValue.ToString() );
                            groupMember.SetAttributeValue( attribute.Key, newValue );

                            if ( ( originalValue ?? string.Empty ).Trim() != ( newValue ?? string.Empty ).Trim() )
                            {
                                isChanged = true;
                            }
                        }
                    }
                }
            }

            return isChanged;
        }

        /// <summary>
        /// Adds the registrants to group.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="registration">The registration.</param>
        /// <param name="args">The arguments.</param>
        private void AddRegistrantsToGroup( RockContext rockContext, RegistrationSettings settings, Registration registration, RegistrationEntryArgsBag args )
        {
            if ( !registration.GroupId.HasValue )
            {
                return;
            }

            // If the registration instance linkage specified a group to add registrant to, add them if they're not already
            // part of that group
            var groupService = new GroupService( rockContext );
            var personAliasService = new PersonAliasService( rockContext );
            var groupMemberService = new GroupMemberService( rockContext );
            var group = groupService.Get( registration.GroupId.Value );

            if ( group is null )
            {
                return;
            }

            foreach ( var registrant in registration.Registrants.Where( r => !r.OnWaitList && r.PersonAliasId.HasValue ).ToList() )
            {
                var personAlias = personAliasService.Get( registrant.PersonAliasId.Value );
                GroupMember groupMember = group.Members.Where( m => m.PersonId == personAlias.PersonId ).FirstOrDefault();
                if ( groupMember == null )
                {
                    groupMember = BuildGroupMember( personAlias.Person, group, settings );

                    groupMemberService.Add( groupMember );
                }
                else
                {
                    groupMember.GroupMemberStatus = settings.GroupMemberStatus;
                }

                rockContext.SaveChanges();

                registrant.GroupMemberId = groupMember != null ? groupMember.Id : ( int? ) null;
                rockContext.SaveChanges();

                // Set any of the template's group member attributes
                groupMember.LoadAttributes();

                var registrantInfo = args.Registrants.FirstOrDefault( r => r.Guid == registrant.Guid );
                if ( registrantInfo != null )
                {
                    if ( UpdateGroupMemberAttributes( groupMember, registrantInfo, settings ) )
                    {
                        groupMember.SaveAttributeValues( rockContext );
                    }
                }
            }
        }

        /// <summary>
        /// Gets the registrant signature hash token. This token contains all the
        /// fields related to a registrant in a deterministic order so it can be
        /// used for validation later.
        /// </summary>
        /// <param name="registrantInfo">The registrant information.</param>
        /// <returns>A <see cref="string"/> that contains the token to be hashed.</returns>
        private string GetRegistrantSignatureHashToken( ViewModels.Blocks.Event.RegistrationEntry.RegistrantBag registrantInfo )
        {
            return registrantInfo.FieldValues
                .OrderBy( kvp => kvp.Key )
                .ThenBy( kvp => kvp.Value.ToStringSafe() )
                .Select( kvp => $"{kvp.Key}:{kvp.ToStringSafe()}" )
                .JoinStrings( "," );
        }

        /// <summary>
        /// Creates a SHA-256 hashed value of the source string. The hash is
        /// then base 64 encoded before it is returned.
        /// </summary>
        /// <param name="source">The source string to be hashed.</param>
        /// <returns>A <see cref="string"/> that contains the base-64 encoded hash value.</returns>
        private string GetSha256Hash( string source )
        {
            using ( var sha256 = SHA256.Create() )
            {
                var hashed = sha256.ComputeHash( Encoding.Unicode.GetBytes( source ) );

                return Convert.ToBase64String( hashed );
            }
        }

        /// <summary>
        /// Creates the signature document object in memory for later saving to
        /// the database. The document and the associated BinaryFile are both
        /// populated.
        /// </summary>
        /// <param name="signatureDocumentTemplate">The signature document template.</param>
        /// <param name="documentData">The document data from a previous signing session.</param>
        /// <param name="entity">The entity that should be associated with the document.</param>
        /// <param name="signedBy">The <see cref="Person"/> that signed the document.</param>
        /// <param name="assignedTo">The <see cref="Person"/> that is the responsible party for signing the document.</param>
        /// <param name="appliesTo">The <see cref="Person"/> that this document will apply to.</param>
        /// <returns>A <see cref="SignatureDocument"/> object that can be saved to the database.</returns>
        private static SignatureDocument CreateSignatureDocument( SignatureDocumentTemplate signatureDocumentTemplate, SignedDocumentData documentData, Person signedBy, Person assignedTo, Person appliesTo, String registrantName, String registrationInstanceName )
        {
            // Glue stuff into the signature document
            var signatureDocument = new SignatureDocument
            {
                SignatureDocumentTemplateId = signatureDocumentTemplate.Id,
                Status = SignatureDocumentStatus.Signed,
                Name = $"{registrantName} ({registrationInstanceName})",
                SignedByPersonAliasId = signedBy.PrimaryAliasId,
                AssignedToPersonAliasId = assignedTo.PrimaryAliasId,
                AppliesToPersonAliasId = appliesTo.PrimaryAliasId,

                SignedDocumentText = documentData.DocumentHtml,
                LastStatusDate = documentData.SignedDateTime,
                SignedDateTime = documentData.SignedDateTime,

                SignatureData = documentData.SignatureData,
                SignedName = documentData.SignedByName,
                SignedByEmail = documentData.SignedByEmail,

                SignedClientIp = documentData.IpAddress,
                SignedClientUserAgent = documentData.UserAgent
            };

            // Needed before determining SignatureInformation (Signed Name, metadata)
            signatureDocument.SignatureVerificationHash = SignatureDocumentService.CalculateSignatureVerificationHash( signatureDocument );

            var signatureInformationHtmlArgs = new GetSignatureInformationHtmlOptions
            {
                SignatureType = signatureDocumentTemplate.SignatureType,
                SignedName = signatureDocument.SignedName,
                DrawnSignatureDataUrl = signatureDocument.SignatureData,
                SignedByPerson = signedBy,
                SignedDateTime = signatureDocument.SignedDateTime,
                SignedClientIp = signatureDocument.SignedClientIp,
                SignatureVerificationHash = signatureDocument.SignatureVerificationHash
            };

            // Helper takes care of generating HTML and combining SignatureDocumentHTML and signedSignatureDocumentHtml into the final Signed Document
            var signatureInformationHtml = ElectronicSignatureHelper.GetSignatureInformationHtml( signatureInformationHtmlArgs );
            var signedSignatureDocumentHtml = ElectronicSignatureHelper.GetSignedDocumentHtml( documentData.DocumentHtml, signatureInformationHtml );

            // Generate the PDF representation of the form.
            using ( var pdfGenerator = new PdfGenerator() )
            {
                var binaryFileTypeId = signatureDocumentTemplate.BinaryFileTypeId;
                if ( !binaryFileTypeId.HasValue )
                {
                    binaryFileTypeId = BinaryFileTypeCache.GetId( Rock.SystemGuid.BinaryFiletype.DIGITALLY_SIGNED_DOCUMENTS.AsGuid() );
                }

                signatureDocument.BinaryFile = pdfGenerator.GetAsBinaryFileFromHtml( binaryFileTypeId ?? 0, signatureDocument.Name, signedSignatureDocumentHtml );
                signatureDocument.BinaryFile.ParentEntityId = signatureDocumentTemplate.Id;
                signatureDocument.BinaryFile.ParentEntityTypeId = EntityTypeCache.Get<SignatureDocumentTemplate>().Id;
                signatureDocument.BinaryFile.IsTemporary = false;
            }

            return signatureDocument;
        }

        #endregion Helpers

        #region Internal Classes

        /// <summary>
        /// Provides a custom registration object that is used during Lava merge
        /// for a signature document.
        /// </summary>
        private class LavaSignatureRegistration : Rock.Lava.LavaDataObject
        {
            public int InstanceId { get; }

            public string InstanceName { get; }

            public int TemplateId { get; }

            public string TemplateName { get; }

            public string RegistrationTerm { get; }

            public string RegistrantTerm { get; }

            public int RegistrantCount { get; }

            public int? GroupId { get; }

            public LavaSignatureRegistration( RegistrationInstance registrationInstance, int? groupId, int registrantCount )
            {
                InstanceId = registrationInstance.Id;
                InstanceName = registrationInstance.Name;
                TemplateId = registrationInstance.RegistrationTemplateId;
                TemplateName = registrationInstance.RegistrationTemplate.Name;
                RegistrationTerm = registrationInstance.RegistrationTemplate.RegistrationTerm;
                RegistrantTerm = registrationInstance.RegistrationTemplate.RegistrantTerm;
                RegistrantCount = registrantCount;
                GroupId = groupId;
            }
        }

        /// <summary>
        /// Provides a custom registrant object that is used during Lava merge
        /// for a signature document.
        /// </summary>
        private class LavaSignatureRegistrant : LavaHasAttributes
        {
            public Location Address { get; }

            public CampusCache Campus { get; }

            public DefinedValueCache ConnectionStatus { get; }

            public DateTime? AnniversaryDate { get; }

            public DateTime? BirthDate { get; }

            public string Email { get; }

            public string FirstName { get; }

            public string MiddleName { get; }

            public string LastName { get; }

            public Gender Gender { get; }

            public string GradeFormatted { get; }

            public int? GradeOffset { get; }

            public int? GraduationYear { get; }

            public DefinedValueCache MaritalStatus { get; }

            public string HomePhone { get; }

            public string MobilePhone { get; }

            public string WorkPhone { get; }

            public Rock.Model.Person Person { get; }

            public Rock.Model.GroupMember GroupMember { get; }

            public LavaSignatureRegistrant( Person person, Location homeLocation, CampusCache campus, GroupMember groupMember, ViewModels.Blocks.Event.RegistrationEntry.RegistrantBag registrantInfo, RegistrationInstance registrationInstance )
            {
                var (registrantAttributes, registrantAttributeValues) = GetRegistrantAttributesFromRegistration( registrantInfo, registrationInstance.RegistrationTemplate );

                Address = homeLocation;
                Campus = campus;
                AnniversaryDate = person.AnniversaryDate;
                BirthDate = person.BirthDate;
                ConnectionStatus = person.ConnectionStatusValueId.HasValue ? DefinedValueCache.Get( person.ConnectionStatusValueId.Value ) : null;
                Email = person.Email;
                FirstName = person.FirstName;
                MiddleName = person.MiddleName;
                LastName = person.LastName;
                Gender = person.Gender;
                GradeFormatted = person.GradeFormatted;
                GradeOffset = person.GradeOffset;
                GraduationYear = person.GraduationYear;

                // We call FormattedNumber here rather than using the property NumberFormatted because at this point NumberFormatted hasn't yet been initialized
                HomePhone = PhoneNumber.FormattedNumber( "", person.GetPhoneNumber( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() )?.Number, false );
                MaritalStatus = person.MaritalStatusValueId.HasValue ? DefinedValueCache.Get( person.MaritalStatusValueId.Value ) : null;
                MobilePhone = PhoneNumber.FormattedNumber( "", person.GetPhoneNumber( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() )?.Number, false );
                WorkPhone = PhoneNumber.FormattedNumber( "", person.GetPhoneNumber( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() )?.Number, false );

                if ( registrantAttributes != null && registrantAttributeValues != null )
                {
                    Attributes = registrantAttributes;
                    AttributeValues = registrantAttributeValues;
                }

                if ( person != null )
                {
                    if ( person.Attributes == null )
                    {
                        person.LoadAttributes();
                    }

                    Person = person;
                }

                if ( groupMember != null )
                {
                    if ( groupMember.Attributes == null )
                    {
                        groupMember.LoadAttributes();
                    }

                    GroupMember = groupMember;
                }
            }
        }

        /// <summary>
        /// Provides a custom object that has attributes that is used during
        /// Lava merge for a signature document.
        /// </summary>
        private class LavaHasAttributes : Rock.Lava.LavaDataObject, IHasAttributes
        {
            #region IHasAttributes

            int IHasAttributes.Id => 0;

            public Dictionary<string, AttributeCache> Attributes { get; set; }

            public Dictionary<string, AttributeValueCache> AttributeValues { get; set; }

            public Dictionary<string, string> AttributeValueDefaults => null;

            public string GetAttributeValue( string key )
            {
                if ( AttributeValues != null &&
                    AttributeValues.ContainsKey( key ) )
                {
                    return this.AttributeValues[key].Value;
                }

                if ( this.Attributes != null &&
                    this.Attributes.ContainsKey( key ) )
                {
                    return this.Attributes[key].DefaultValue;
                }

                return null;
            }

            public List<string> GetAttributeValues( string key )
            {
                string value = GetAttributeValue( key );
                if ( !string.IsNullOrWhiteSpace( value ) )
                {
                    return value.SplitDelimitedValues().ToList();
                }

                return new List<string>();
            }

            public void SetAttributeValue( string key, string value )
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        /// <summary>
        /// Internal structure for the encoded data that contains the signed document.
        /// This is sent to the browser in encrypted form and then included in
        /// the final submission process to be used to generate the actual document.
        /// </summary>
        private class SignedDocumentData
        {
            public string DocumentHtml { get; set; }

            public string SignatureData { get; set; }

            public string SignedByName { get; set; }

            public string SignedByEmail { get; set; }

            public string IpAddress { get; set; }

            public string UserAgent { get; set; }

            public DateTime SignedDateTime { get; set; }
        }

        #endregion
    }
}
