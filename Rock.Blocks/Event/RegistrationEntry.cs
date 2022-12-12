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
using Rock.Data;
using Rock.ElectronicSignature;
using Rock.Financial;
using Rock.Model;
using Rock.Pdf;
using Rock.Tasks;
using Rock.ViewModels.Blocks.Event.RegistrationEntry;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Finance;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Event
{
    /// <summary>
    /// Registration Entry.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianBlockType" />

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

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.OBSIDIAN_EVENT_REGISTRATION_ENTRY )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.OBSIDIAN_EVENT_REGISTRATION_ENTRY )]
    public class RegistrationEntry : RockObsidianBlockType
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
        }

        /// <summary>
        /// The prefix to look for when identifying if any page parameters are
        /// for a return-from-payment redirect.
        /// </summary>
        public const string ReturnUrlSessionPrefix = "sessionGuid";

        #endregion Keys

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
                var viewModel = GetViewModel( rockContext );
                return viewModel;
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
        public BlockActionResult CheckDiscountCode( string code, int registrantCount )
        {
            using ( var rockContext = new RockContext() )
            {
                var registrationInstanceId = GetRegistrationInstanceId( rockContext );
                var registrationTemplateDiscountService = new RegistrationTemplateDiscountService( rockContext );
                RegistrationTemplateDiscountWithUsage discount = null;

                // If the code isn't provided then check for an auto apply discount.
                if ( code.IsNullOrWhiteSpace() )
                {
                    var registrationTemplateDiscountCodes = registrationTemplateDiscountService
                        .GetDiscountsForRegistrationInstance( registrationInstanceId )
                        .Where( d => d.AutoApplyDiscount )
                        .OrderBy( d => d.Order )
                        .Select( d => d.Code )
                        .ToList();

                    foreach( var registrationTemplateDiscountCode in registrationTemplateDiscountCodes )
                    {
                        discount = registrationTemplateDiscountService.GetDiscountByCodeIfValid( registrationInstanceId, registrationTemplateDiscountCode );

                        if ( discount == null || ( discount.RegistrationTemplateDiscount.MinRegistrants.HasValue && registrantCount < discount.RegistrationTemplateDiscount.MinRegistrants.Value ) )
                        {
                            continue;
                        }

                        // use the first discount that is valid
                        break;
                    }
                }
                else
                {
                    discount = registrationTemplateDiscountService.GetDiscountByCodeIfValid( registrationInstanceId, code );
                }

                if ( discount == null )
                {
                    // The code is not found
                    return ActionNotFound();
                }

                return ActionOk( new
                {
                    DiscountCode = discount.RegistrationTemplateDiscount.Code,
                    UsagesRemaining = discount.UsagesRemaining,
                    DiscountAmount = discount.RegistrationTemplateDiscount.DiscountAmount,
                    DiscountPercentage = discount.RegistrationTemplateDiscount.DiscountPercentage
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
        public BlockActionResult GetPaymentRedirect( RegistrationEntryBlockArgs args, string returnUrl )
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
        public BlockActionResult PersistSession( RegistrationEntryBlockArgs args )
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
        public BlockActionResult CalculateCost( RegistrationEntryBlockArgs args )
        {
            using ( var rockContext = new RockContext() )
            {
                var context = GetContext( rockContext, args, out var errorMessage );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    return ActionBadRequest( errorMessage );
                }

                var registrationInstanceService = new RegistrationInstanceService( rockContext );
                var costs = registrationInstanceService.GetRegistrationCostSummaryInfo( context, args );

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

                return ActionOk( new SessionRenewalResult
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
        public BlockActionResult SubmitRegistration( RegistrationEntryBlockArgs args )
        {
            using ( var rockContext = new RockContext() )
            {
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
        /// <returns>An instance of <see cref="RegistrationEntrySignatureDocument"/> that contains the document information.</returns>
        [BlockAction]
        public BlockActionResult GetSignatureDocumentData( RegistrationEntryBlockArgs args, Guid registrantGuid )
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
                        RegistrationInstanceId = context.RegistrationSettings.RegistrationInstanceId
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
                var (campusId, location, _) = UpdatePersonFromRegistrant( person, registrantInfo, new History.HistoryChangeList(), context.RegistrationSettings );

                if ( person.Attributes == null )
                {
                    person.LoadAttributes( rockContext );
                }

                UpdatePersonAttributes( person, new History.HistoryChangeList(), registrantInfo, context.RegistrationSettings );

                // Process the GroupMember so we have data for the Lava merge.
                GroupMember groupMember = null;
                var groupId = GetRegistrationGroupId( rockContext );

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
                    { "Registrant", new LavaSignatureRegistrant( person, location, campusCache, groupMember ) }
                };

                var html = ElectronicSignatureHelper.GetSignatureDocumentHtml( documentTemplate.LavaTemplate, mergeFields );

                // Calculate a document hash from the registrant information to
                // be used later to validate the document after signing.
                var fieldHashToken = GetRegistrantSignatureHashToken( registrantInfo );
                var unencryptedSecurityToken = new[] { RockDateTime.Now.ToString( "o" ), GetSha256Hash( fieldHashToken + html ) }.ToJson();
                var encryptedSecurityToken = Security.Encryption.EncryptString( unencryptedSecurityToken );

                return ActionOk( new RegistrationEntrySignatureDocument
                {
                    DocumentHtml = html,
                    SecurityToken = encryptedSecurityToken
                } );
            }
        }

        /// <summary>
        /// Signs the document previously returned by <see cref="GetSignatureDocumentData(RegistrationEntryBlockArgs, Guid)"/>.
        /// </summary>
        /// <param name="args">The registration entry arguments.</param>
        /// <param name="registrantGuid">The unique identifier of the registrant this document will apply to.</param>
        /// <param name="documentHtml">The document HTML that was signed.</param>
        /// <param name="securityToken">The security token to validate the request.</param>
        /// <param name="signature">The signature of the person that signed the document.</param>
        /// <returns>A string that contains the encoded signed document details.</returns>
        [BlockAction]
        public BlockActionResult SignDocument( RegistrationEntryBlockArgs args, Guid registrantGuid, string documentHtml, string securityToken, ElectronicSignatureValueViewModel signature )
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
                var unencryptedSecurityToken = Security.Encryption.DecryptString( securityToken ).FromJsonOrNull<List<string>>();
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

                return ActionOk( Security.Encryption.EncryptString( signedData.ToJson() ) );
            }
        }

        #endregion Block Actions

        #region Helpers

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
            RegistrationEntryBlockArgs args,
            SessionStatus sessionStatus,
            out string errorMessage )
        {
            var sessionData = new RegistrationEntryBlockSession
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
                RegistrationSessionGuid = args.RegistrationSessionGuid
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
        private Registration SubmitRegistration( RockContext rockContext, RegistrationContext context, RegistrationEntryBlockArgs args, out string errorMessage )
        {
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
                    RegistrationInstanceId = context.RegistrationSettings.RegistrationInstanceId
                };

                var registrationService = new RegistrationService( rockContext );
                registrationService.Add( context.Registration );
                registrationChanges.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Registration" );

                if ( context.RegistrationSettings.RegistrarOption == RegistrarOption.UseLoggedInPerson && currentPerson != null )
                {
                    registrar = currentPerson;
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

            var discountPercentage = context.Discount?.RegistrationTemplateDiscount.DiscountPercentage ?? 0;
            History.EvaluateChange( registrationChanges, "Discount Percentage", context.Registration.DiscountPercentage, discountPercentage );
            context.Registration.DiscountPercentage = discountPercentage;

            var discountAmount = context.Discount?.RegistrationTemplateDiscount.DiscountAmount ?? 0;
            History.EvaluateChange( registrationChanges, "Discount Amount", context.Registration.DiscountAmount, discountAmount );
            context.Registration.DiscountAmount = discountAmount;

            // If the registrar person record does not exist, try to find the record.
            if ( registrar == null )
            {
                registrar = GetExistingRegistrarPerson( context, currentPerson, rockContext );
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

                registrar = SavePerson(
                    rockContext,
                    context.RegistrationSettings,
                    person,
                    args.Registrar.FamilyGuid ?? Guid.NewGuid(),
                    campusId,
                    null,
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
                    var isEmailDifferent = !context.Registration.ConfirmationEmail.Trim().Equals( registrar.Email.Trim(), StringComparison.OrdinalIgnoreCase );

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
            multipleFamilyGroupIds.AddOrIgnore( registrarFamily.Guid, registrarFamily.Id );

            if ( !singleFamilyId.HasValue )
            {
                singleFamilyId = registrarFamily.Id;
            }

            // If the Registration Instance linkage specified a group, load it now
            var groupId = GetRegistrationGroupId( rockContext );

            Group group = null;

            if ( groupId.HasValue )
            {
                group = new GroupService( rockContext ).Get( groupId.Value );

                if ( group != null && ( !context.Registration.GroupId.HasValue || context.Registration.GroupId.Value != group.Id ) )
                {
                    context.Registration.GroupId = group.Id;
                    History.EvaluateChange( registrationChanges, "Group", string.Empty, group.Name );
                }
            }

            var registrationSlug = PageParameter( PageParameterKey.Slug );
            var linkage = GetRegistrationLinkage( registrationSlug, rockContext );

            if ( linkage?.CampusId.HasValue == true )
            {
                campusId = linkage.CampusId;
            }

            if ( campusId.HasValue )
            {
                context.Registration.CampusId = campusId;
                History.EvaluateChange( registrationChanges, "Campus", string.Empty, CampusCache.Get( (int) campusId ).Name );
            }

            // if this registration was marked as temporary (started from another page, then specified in the url), set IsTemporary to False now that we are done
            context.Registration.IsTemporary = false;

            // Set attribute values on the registration
            var registrationAttributes = GetRegistrationAttributes( context.RegistrationSettings.RegistrationTemplateId );
            context.Registration.LoadAttributes( rockContext );

            foreach ( var attribute in registrationAttributes )
            {
                var value = args.FieldValues.GetValueOrNull( attribute.Guid );
                context.Registration.SetAttributeValue( attribute.Key, value.ToStringSafe() );
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

                foreach ( var registrantInfo in args.Registrants )
                {
                    var forceWaitlist = context.SpotsRemaining < 1;
                    bool isCreatedAsRegistrant = context.RegistrationSettings.RegistrarOption == RegistrarOption.UseFirstRegistrant && registrantInfo == args.Registrants.FirstOrDefault();

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

                    index++;
                }

                rockContext.SaveChanges();

                var transactionGuid = args.AmountToPayNow > 0 ?
                    ProcessPayment( rockContext, context, args, out errorMessage ) :
                    null;

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    throw new Exception( errorMessage );
                }

                if ( args.AmountToPayNow > 0 && !transactionGuid.HasValue )
                {
                    throw new Exception( "There was a problem with the payment" );
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
                registrar = currentPerson;
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
        /// <returns>The <see cref="Group"/> identifier or <c>null</c> if one is not available.</returns>
        private int? GetRegistrationGroupId( RockContext rockContext )
        {
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

            return groupId;
        }

        /// <summary>
        /// Gets the registration linkage.
        /// </summary>
        /// <param name="slug">The slug.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private EventItemOccurrenceGroupMap GetRegistrationLinkage( string slug, RockContext rockContext )
        {
            var dateTime = RockDateTime.Now;

            var linkage = new EventItemOccurrenceGroupMapService( rockContext ?? new RockContext() )
                .Queryable().AsNoTracking()
                .Include( m => m.Campus )
                .Where( l =>
                    l.UrlSlug == slug &&
                    l.RegistrationInstance != null &&
                    l.RegistrationInstance.IsActive &&
                    l.RegistrationInstance.RegistrationTemplate != null &&
                    l.RegistrationInstance.RegistrationTemplate.IsActive &&
                    ( !l.RegistrationInstance.StartDateTime.HasValue || l.RegistrationInstance.StartDateTime <= dateTime ) &&
                    ( !l.RegistrationInstance.EndDateTime.HasValue || l.RegistrationInstance.EndDateTime > dateTime ) )
                .FirstOrDefault();

            return linkage;
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
        private string GetRegistrantFirstName( RegistrationContext context, ViewModels.Blocks.Event.RegistrationEntry.RegistrantInfo registrantInfo )
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
        private string GetRegistrantLastName( RegistrationContext context, ViewModels.Blocks.Event.RegistrationEntry.RegistrantInfo registrantInfo )
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
        private string GetRegistrantFullName( RegistrationContext context, ViewModels.Blocks.Event.RegistrationEntry.RegistrantInfo registrantInfo )
        {
            var firstName = GetRegistrantFirstName( context, registrantInfo );
            var lastName = GetRegistrantLastName( context, registrantInfo );
            return $"{firstName} {lastName}";
        }

        /// <summary>
        /// Gets the field values.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="person">The person.</param>
        /// <param name="registrant">The registrant to use when retrieving registrant attribute values..</param>
        /// <param name="forms">The forms.</param>
        /// <returns></returns>
        private Dictionary<Guid, object> GetCurrentValueFieldValues( RockContext rockContext, Person person, RegistrationRegistrant registrant, IEnumerable<RegistrationTemplateForm> forms )
        {
            var fieldValues = new Dictionary<Guid, object>();

            if ( person is null )
            {
                return fieldValues;
            }

            foreach ( var form in forms )
            {
                var fields = form.Fields.Where( f =>
                {
                    if ( f.ShowCurrentValue && !f.IsInternal && ( f.Attribute == null || f.Attribute.IsActive ) )
                    {
                        return true;
                    }

                    if ( f.FieldSource == RegistrationFieldSource.PersonField )
                    {
                        return f.PersonFieldType == RegistrationPersonFieldType.FirstName
                            || f.PersonFieldType == RegistrationPersonFieldType.LastName;
                    }

                    if ( f.FieldSource == RegistrationFieldSource.RegistrantAttribute )
                    {
                        return true;
                    }

                    return false;
                } );

                foreach ( var field in fields )
                {
                    var value = GetCurrentFieldValue( rockContext, person, registrant, field );

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
        private object GetCurrentFieldValue( RockContext rockContext, Person person, RegistrationRegistrant registrant, RegistrationTemplateFormField field )
        {
            switch ( field.FieldSource )
            {
                case RegistrationFieldSource.PersonField:
                    return GetPersonCurrentFieldValue( rockContext, person, field );
                case RegistrationFieldSource.PersonAttribute:
                    return GetEntityCurrentClientAttributeValue( rockContext, person, field );
                case RegistrationFieldSource.RegistrantAttribute:
                    return GetEntityCurrentClientAttributeValue( rockContext, registrant, field );
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
                        PostalCode = location?.PostalCode ?? string.Empty
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
                    return person.GetPhoneNumber( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() )?.Number;

                case RegistrationPersonFieldType.WorkPhone:
                    return person.GetPhoneNumber( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() )?.Number;

                case RegistrationPersonFieldType.MobilePhone:
                    return person.GetPhoneNumber( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() )?.Number;
            }

            return null;
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
                return PublicAttributeHelper.GetPublicEditValue( attribute, attribute.DefaultValue );
            }

            entity.LoadAttributes( rockContext );

            return PublicAttributeHelper.GetPublicEditValue( attribute, entity.GetAttributeValue( attribute.Key ) );
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
                        multipleFamilyGroupIds.AddOrIgnore( familyGuid, familyGroup.Id );
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
            var phoneNumber = fieldValue as string;
            if ( phoneNumber != null )
            {
                string cleanNumber = PhoneNumber.CleanNumber( phoneNumber );
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

                        phone.Number = cleanNumber;

                        History.EvaluateChange(
                            changes,
                            string.Format( "{0} Phone", numberType.Value ),
                            oldPhoneNumber,
                            phone.NumberFormattedWithCountryCode );
                    }
                }
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
        private (Person person, RegistrationRegistrant registrant) GetExistingOrCreatePerson( RegistrationContext context, ViewModels.Blocks.Event.RegistrationEntry.RegistrantInfo registrantInfo, Person registrar, Guid registrarFamilyGuid, bool isCreatedAsRegistrant, RockContext rockContext )
        {
            RegistrationRegistrant registrant = null;
            Person person = null;
            var personService = new PersonService( rockContext );

            var firstName = GetPersonFieldValue( context.RegistrationSettings, RegistrationPersonFieldType.FirstName, registrantInfo.FieldValues ).ToStringSafe();
            var lastName = GetPersonFieldValue( context.RegistrationSettings, RegistrationPersonFieldType.LastName, registrantInfo.FieldValues ).ToStringSafe();
            var email = GetPersonFieldValue( context.RegistrationSettings, RegistrationPersonFieldType.Email, registrantInfo.FieldValues ).ToStringSafe();
            var birthday = GetPersonFieldValue( context.RegistrationSettings, RegistrationPersonFieldType.Birthdate, registrantInfo.FieldValues ).ToStringSafe().FromJsonOrNull<BirthdayPickerBag>().ToDateTime();
            var mobilePhone = GetPersonFieldValue( context.RegistrationSettings, RegistrationPersonFieldType.MobilePhone, registrantInfo.FieldValues ).ToStringSafe();

            registrant = context.Registration.Registrants.FirstOrDefault( r => r.Guid == registrantInfo.Guid );

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
                if ( isCreatedAsRegistrant )
                {
                    person = registrar;
                }
                else
                {
                    var dvcConnectionStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.ConnectionStatus ).AsGuid() );
                    var dvcRecordStatus = DefinedValueCache.Get( GetAttributeValue( AttributeKey.RecordStatus ).AsGuid() );

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
            }

            return (person, registrant);
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
        private (int? campusId, Location location, bool updateExistingCampus) UpdatePersonFromRegistrant( Person person, ViewModels.Blocks.Event.RegistrationEntry.RegistrantInfo registrantInfo, History.HistoryChangeList personChanges, RegistrationSettings settings )
        {
            Location location = null;
            var campusId = PageParameter( PageParameterKey.CampusId ).AsIntegerOrNull();
            var updateExistingCampus = false;

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
                        case RegistrationPersonFieldType.Campus:
                            var campusGuid = fieldValue.ToString().AsGuidOrNull();
                            updateExistingCampus = campusGuid.HasValue;
                            campusId = campusGuid.HasValue ? CampusCache.Get( campusGuid.Value )?.Id ?? campusId : campusId;
                            break;

                        case RegistrationPersonFieldType.MiddleName:
                            string middleName = fieldValue.ToString().Trim();
                            History.EvaluateChange( personChanges, "Middle Name", person.MiddleName, middleName );
                            person.MiddleName = middleName;
                            break;

                        case RegistrationPersonFieldType.Address:
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

                            break;

                        case RegistrationPersonFieldType.Birthdate:
                            var oldBirthMonth = person.BirthMonth;
                            var oldBirthDay = person.BirthDay;
                            var oldBirthYear = person.BirthYear;

                            person.SetBirthDate( fieldValue.ToStringSafe().FromJsonOrNull<BirthdayPickerBag>().ToDateTime() );

                            History.EvaluateChange( personChanges, "Birth Month", oldBirthMonth, person.BirthMonth );
                            History.EvaluateChange( personChanges, "Birth Day", oldBirthDay, person.BirthDay );
                            History.EvaluateChange( personChanges, "Birth Year", oldBirthYear, person.BirthYear );
                            break;

                        case RegistrationPersonFieldType.Gender:
                            var newGender = fieldValue.ToString().ConvertToEnumOrNull<Gender>() ?? Gender.Unknown;
                            History.EvaluateChange( personChanges, "Gender", person.Gender, newGender );
                            person.Gender = newGender;
                            break;

                        case RegistrationPersonFieldType.AnniversaryDate:
                            var oldAnniversaryDate = person.AnniversaryDate;
                            person.AnniversaryDate = fieldValue.ToStringSafe().FromJsonOrNull<BirthdayPickerBag>().ToDateTime();
                            History.EvaluateChange( personChanges, "Anniversary Date", oldAnniversaryDate, person.AnniversaryDate );
                            break;

                        case RegistrationPersonFieldType.MaritalStatus:
                            {
                                var newMaritalStatusValueGuid = fieldValue.ToStringSafe().AsGuidOrNull();
                                var newMaritalStatusValueId = newMaritalStatusValueGuid.HasValue ? DefinedValueCache.Get( newMaritalStatusValueGuid.Value )?.Id : null;
                                var oldMaritalStatusValueId = person.MaritalStatusValueId;
                                person.MaritalStatusValueId = newMaritalStatusValueId;
                                History.EvaluateChange( personChanges, "Marital Status", DefinedValueCache.GetName( oldMaritalStatusValueId ), DefinedValueCache.GetName( person.MaritalStatusValueId ) );
                                break;
                            }

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
                            {
                                var newConnectionStatusValueGuid = fieldValue.ToStringSafe().AsGuidOrNull();
                                var newConnectionStatusValueId = newConnectionStatusValueGuid.HasValue ? DefinedValueCache.Get( newConnectionStatusValueGuid.Value )?.Id : null;
                                var oldConnectionStatusValueId = person.ConnectionStatusValueId;
                                person.ConnectionStatusValueId = newConnectionStatusValueId;
                                History.EvaluateChange( personChanges, "Connection Status", DefinedValueCache.GetName( oldConnectionStatusValueId ), DefinedValueCache.GetName( person.ConnectionStatusValueId ) );
                                break;
                            }

                        case RegistrationPersonFieldType.Grade:
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

                                break;
                            }
                    }
                }
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
        private bool UpdatePersonAttributes( Person person, History.HistoryChangeList personChanges, ViewModels.Blocks.Event.RegistrationEntry.RegistrantInfo registrantInfo, RegistrationSettings settings )
        {
            bool isChanged = false;
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
                        string originalValue = person.GetAttributeValue( attribute.Key );
                        string newValue = PublicAttributeHelper.GetPrivateValue( attribute, fieldValue.ToString() );
                        person.SetAttributeValue( attribute.Key, newValue );

                        if ( ( originalValue ?? string.Empty ).Trim() != ( newValue ?? string.Empty ).Trim() )
                        {
                            string formattedOriginalValue = string.Empty;
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
                            History.EvaluateChange( personChanges, attribute.Name, formattedOriginalValue, formattedNewValue );
                        }
                    }
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
            ViewModels.Blocks.Event.RegistrationEntry.RegistrantInfo registrantInfo,
            int index,
            Dictionary<Guid, int> multipleFamilyGroupIds,
            ref int? singleFamilyId,
            bool isWaitlist,
            bool isCreatedAsRegistrant,
            bool isNewRegistration,
            List<Action> postSaveActions )
        {
            // Force waitlist if specified by param, but allow waitlist if requested
            isWaitlist |= ( context.RegistrationSettings.IsWaitListEnabled && registrantInfo.IsOnWaitList );

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
                registrant = new RegistrationRegistrant();
                registrant.Guid = registrantInfo.Guid;
                registrantService.Add( registrant );
                registrant.RegistrationId = context.Registration.Id;
            }

            registrant.OnWaitList = isWaitlist;
            registrant.PersonAliasId = person.PrimaryAliasId;
            registrant.Cost = isWaitlist ? 0 : context.RegistrationSettings.PerRegistrantCost;

            // Check if discount applies
            var maxRegistrants = context.Discount?.RegistrationTemplateDiscount.MaxRegistrants;
            var isWithinMaxRegistrants = !maxRegistrants.HasValue || index < maxRegistrants.Value;
            var usesRemaining = context.Discount?.UsagesRemaining ?? int.MaxValue;
            var isWithinUsageCap = usesRemaining >= 1;
            registrant.DiscountApplies = isWithinMaxRegistrants && isWithinUsageCap;

            if ( registrant.DiscountApplies && context.Discount?.UsagesRemaining != null )
            {
                context.Discount.UsagesRemaining--;
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
                var feeModels = context.RegistrationSettings.Fees?
                    .Where( f => f.IsActive )
                    .OrderBy( f => f.Order )
                    .ToList() ?? new List<RegistrationTemplateFee>();

                foreach ( var feeModel in feeModels )
                {
                    var totalFeeQuantity = 0;
                    var feeItemModels = feeModel.FeeItems.Where( f => f.IsActive ).ToList();

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
                        if ( isLastFeeItemModel && totalFeeQuantity < 1 && quantity < 1 && feeModel.IsRequired )
                        {
                            quantity = 1;
                        }

                        // If there is a limited supply, ensure that more are not ordered than available
                        var countRemaining = context.FeeItemsCountRemaining.GetValueOrNull( feeItemModel.Guid );

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
                var documentTemplate = new SignatureDocumentTemplateService( rockContext ).Get( context.RegistrationSettings.SignatureDocumentTemplateId ?? 0 );
                var signedData = Security.Encryption.DecryptString( registrantInfo.SignatureData ).FromJsonOrThrow<SignedDocumentData>();
                var signedBy = RequestContext.CurrentPerson ?? registrar;

                var document = CreateSignatureDocument( documentTemplate, signedData, registrant, signedBy, registrar, person, registrant.Person.FullName, context.RegistrationSettings.Name);

                new SignatureDocumentService( rockContext ).Add( document );
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
        /// Updates the registrant attribute values with those provided by the
        /// registrant information.
        /// </summary>
        /// <param name="registrant">The registrant to be updated.</param>
        /// <param name="registrantInfo">The registrant information.</param>
        /// <param name="registrantChanges">The registrant changes that were made.</param>
        /// <param name="settings">The registration settings.</param>
        /// <returns><c>true</c> if any attributes were modified, <c>false</c> otherwise.</returns>
        private bool UpdateRegistrantAttributes( RegistrationRegistrant registrant, ViewModels.Blocks.Event.RegistrationEntry.RegistrantInfo registrantInfo, History.HistoryChangeList registrantChanges, RegistrationSettings settings )
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
            }

            return isChanged;
        }

        /// <summary>
        /// Gets the view model.
        /// </summary>
        /// <returns></returns>
        private RegistrationEntryBlockViewModel GetViewModel( RockContext rockContext )
        {
            // Get the registration context (template, instance, actual registration (if existing))
            var context = GetContext( rockContext, out var errorMessage );

            if ( context is null )
            {
                return new RegistrationEntryBlockViewModel
                {
                    RegistrationInstanceNotFoundMessage = errorMessage
                };
            }

            // If the registration is existing, then add the args that describe it to the view model
            var session = GetRegistrationEntryBlockSession( rockContext, context.RegistrationSettings );

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
            RegistrationEntryBlockSuccessViewModel successViewModel = null;

            if ( session != null )
            {
                var args = new RegistrationEntryBlockArgs
                {
                    AmountToPayNow = session.AmountToPayNow,
                    DiscountCode = session.DiscountCode,
                    FieldValues = session.FieldValues,
                    Registrants = session.Registrants,
                    Registrar = session.Registrar,
                    RegistrationGuid = session.RegistrationGuid, // See engineering note from 9/7/2022 above.
                    RegistrationSessionGuid = session.RegistrationSessionGuid
                };

                // Get a new context with the args
                context = GetContext( rockContext, args, out errorMessage );

                var financialGatewayService = new FinancialGatewayService( rockContext );
                var paymentFinancialGateway = financialGatewayService.Get( context.RegistrationSettings.FinancialGatewayId ?? 0 );
                var paymentGateway = paymentFinancialGateway?.GetGatewayComponent() as IPaymentTokenGateway;

                string paymentToken = null;
                var wasRedirectedFromPayment = paymentGateway?.TryGetPaymentTokenFromParameters( paymentFinancialGateway, RequestContext.GetPageParameters(), out paymentToken ) ?? false;

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
                        var transactionGuid = ProcessPayment( rockContext, context, args, out errorMessage );

                        if ( !errorMessage.IsNullOrWhiteSpace() )
                        {
                            throw new Exception( errorMessage );
                        }

                        if ( !transactionGuid.HasValue )
                        {
                            throw new Exception( "There was a problem with the payment" );
                        }
                    }

                    successViewModel = GetSuccessViewModel( context.Registration.Id, context.TransactionCode, context.GatewayPersonIdentifier );
                }
            }

            // Get models needed for the view model
            var hasDiscountsAvailable = context.RegistrationSettings.Discounts?.Any() == true;
            var formModels = context.RegistrationSettings.Forms?.OrderBy( f => f.Order ).ToList() ?? new List<RegistrationTemplateForm>();

            // Get family members
            var currentPerson = GetCurrentPerson();
            var familyMembers = context.RegistrationSettings.AreCurrentFamilyMembersShown ?
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
                        FieldValues = GetCurrentValueFieldValues( rockContext, gm.Person, null, formModels )
                    } )
                    .ToList() :
                    new List<RegistrationEntryBlockFamilyMemberViewModel>();

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

            // Get the fees
            var feeModels = context.RegistrationSettings.Fees?.Where( f => f.IsActive ).OrderBy( f => f.Order ).ToList() ?? new List<RegistrationTemplateFee>();
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
                    Items = feeModel.FeeItems.Where( f => f.IsActive ).Select( fi => new RegistrationEntryBlockFeeItemViewModel
                    {
                        Cost = fi.Cost,
                        Name = fi.Name,
                        Guid = fi.Guid,
                        CountRemaining = context.FeeItemsCountRemaining.GetValueOrNull( fi.Guid )
                    } )
                };

                fees.Add( feeViewModel );
            }

            // Get forms with fields
            var formViewModels = new List<RegistrationEntryBlockFormViewModel>();
            var allAttributeFields = formModels.SelectMany( fm => fm.Fields.Where( f => !f.IsInternal && f.Attribute?.IsActive == true ) ).ToList();

            foreach ( var formModel in formModels )
            {
                var form = new RegistrationEntryBlockFormViewModel();
                var fieldModels = formModel.Fields.Where( f => !f.IsInternal && ( f.Attribute == null || f.Attribute.IsActive ) ).OrderBy( f => f.Order );
                var fields = new List<RegistrationEntryBlockFormFieldViewModel>();

                foreach ( var fieldModel in fieldModels )
                {
                    var field = new RegistrationEntryBlockFormFieldViewModel();
                    var attribute = fieldModel.AttributeId.HasValue ? AttributeCache.Get( fieldModel.AttributeId.Value ) : null;

                    field.Guid = fieldModel.Guid;
                    field.Attribute = attribute != null ? PublicAttributeHelper.GetPublicAttributeForEdit( attribute ) : null;
                    field.FieldSource = ( int ) fieldModel.FieldSource;
                    field.PersonFieldType = ( int ) fieldModel.PersonFieldType;
                    field.IsRequired = fieldModel.IsRequired;
                    field.IsSharedValue = fieldModel.IsSharedValue;
                    field.VisibilityRuleType = ( int ) fieldModel.FieldVisibilityRules.FilterExpressionType;
                    field.PreHtml = fieldModel.PreText;
                    field.PostHtml = fieldModel.PostText;
                    field.ShowOnWaitList = fieldModel.ShowOnWaitlist;

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

                            var filterValues = new List<string>();
                            var fieldAttribute = AttributeCache.Get( comparedToField.AttributeId.Value );
                            var fieldType = fieldAttribute?.FieldType?.Field;

                            if ( fieldType == null )
                            {
                                return null;
                            }

                            var comparisonTypeValue = vr.ComparisonType.ConvertToString( false );
                            if ( comparisonTypeValue != null )
                            {
                                // only add the comparisonTypeValue if it is specified, just like
                                // the logic at https://github.com/SparkDevNetwork/Rock/blob/22f64416b2461c8a988faf4b6e556bc3dcb209d3/Rock/Field/FieldType.cs#L558
                                filterValues.Add( comparisonTypeValue );
                            }

                            filterValues.Add( vr.ComparedToValue );

                            var comparisonValue = fieldType.GetPublicFilterValue( filterValues.ToJson(), fieldAttribute.ConfigurationValues );

                            return new RegistrationEntryBlockVisibilityViewModel
                            {
                                ComparedToRegistrationTemplateFormFieldGuid = vr.ComparedToFormFieldGuid.Value,
                                ComparisonValue = new PublicComparisonValueBag
                                {
                                    ComparisonType = ( int? ) comparisonValue.ComparisonType,
                                    Value = comparisonValue.Value
                                }
                            };
                        } )
                        .Where( vr => vr != null );

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
                    CurrencyTypeGuids = GetAllowedCurrencyTypes( gatewayComponent ).Select( a => a.Guid ).ToList()
                };

                savedAccounts = savedAccountClientService.GetSavedFinancialAccountsForPersonAsAccountListItems( RequestContext.CurrentPerson.Id, accountOptions );
            }

            // If we don't have a session that means we are starting new. Create
            // an empty session.
            if ( session == null && currentPerson != null )
            {
                session = new RegistrationEntryBlockSession
                {
                    RegistrationSessionGuid = Guid.NewGuid()
                };

                session.Registrants = new List<ViewModels.Blocks.Event.RegistrationEntry.RegistrantInfo>();
                var isOnWaitList = context.SpotsRemaining.HasValue && context.SpotsRemaining.Value == 0;

                if ( context.RegistrationSettings.AreCurrentFamilyMembersShown )
                {
                    // Fill in first registrant info as a member of the family.
                    session.Registrants.Add( new ViewModels.Blocks.Event.RegistrationEntry.RegistrantInfo
                    {
                        Guid = Guid.NewGuid(),
                        FamilyGuid = currentPerson.PrimaryFamily.Guid,
                        IsOnWaitList = isOnWaitList,
                        PersonGuid = currentPerson.Guid,
                        FeeItemQuantities = new Dictionary<Guid, int>(),
                        FieldValues = GetCurrentValueFieldValues( rockContext, currentPerson, null, formModels )
                    } );
                }
                else
                {
                    // Only fill in the first registrant with existing values
                    // as a "new" person if family members are not shown.
                    session.Registrants.Add( new ViewModels.Blocks.Event.RegistrationEntry.RegistrantInfo
                    {
                        Guid = Guid.NewGuid(),
                        FamilyGuid = Guid.NewGuid(),
                        IsOnWaitList = isOnWaitList,
                        PersonGuid = null,
                        FeeItemQuantities = new Dictionary<Guid, int>(),
                        FieldValues = !isOnWaitList ? GetCurrentValueFieldValues( rockContext, currentPerson, null, formModels ) : new Dictionary<Guid, object>()
                    } );
                }
            }

            // Determine the starting point. External registration updates are
            // currently only supported if we are not doing inline signatures.
            var allowExternalRegistrationUpdates = context.RegistrationSettings.AllowExternalRegistrationUpdates && !context.RegistrationSettings.IsInlineSignatureRequired;
            var allowRegistrationUpdates = !isExistingRegistration || allowExternalRegistrationUpdates;
            var startAtBeginning = !isExistingRegistration ||
                ( allowExternalRegistrationUpdates && PageParameter( PageParameterKey.StartAtBeginning ).AsBoolean() );

            var viewModel = new RegistrationEntryBlockViewModel
            {
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
                MaxRegistrants = context.RegistrationSettings.MaxRegistrants ?? 25,
                RegistrantsSameFamily = ( int ) context.RegistrationSettings.RegistrantsSameFamily,
                ForceEmailUpdate = forceEmailUpdate,
                RegistrarOption = ( int ) context.RegistrationSettings.RegistrarOption,
                Cost = baseCost,
                GatewayControl = isRedirectGateway ? null : new GatewayControlBag
                {
                    FileUrl = financialGatewayComponent?.GetObsidianControlFileUrl( financialGateway ) ?? string.Empty,
                    Settings = financialGatewayComponent?.GetObsidianControlSettings( financialGateway, null ) ?? new object()
                },
                IsRedirectGateway = isRedirectGateway,
                SpotsRemaining = context.SpotsRemaining,
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
                TimeoutMinutes = timeoutMinutes,
                AllowRegistrationUpdates = allowRegistrationUpdates,
                IsExistingRegistration = isExistingRegistration,
                StartAtBeginning = startAtBeginning,
                GatewayGuid = financialGateway?.Guid,
                Campuses = campusClientService.GetCampusesAsListItems(),
                MaritalStatuses = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_MARITAL_STATUS )
                    .DefinedValues
                    .OrderBy( v => v.Order )
                    .Select( v => new ListItemBag
                    {
                        Value = v.Guid.ToString(),
                        Text = v.Value
                    } )
                    .ToList(),
                ConnectionStatuses = DefinedTypeCache.Get( SystemGuid.DefinedType.PERSON_CONNECTION_STATUS )
                    .DefinedValues
                    .OrderBy( v => v.Order )
                    .Select( v => new ListItemBag
                    {
                        Value = v.Guid.ToString(),
                        Text = v.Value
                    } )
                    .ToList(),
                Grades = DefinedTypeCache.Get( SystemGuid.DefinedType.SCHOOL_GRADES )
                    .DefinedValues
                    .OrderBy( v => v.Order )
                    .Select( v => new ListItemBag
                    {
                        Value = v.Guid.ToString(),
                        Text = v.GetAttributeValue( "Abbreviation" )
                    } )
                    .ToList(),

                EnableSaveAccount = enableSavedAccount,
                SavedAccounts = savedAccounts
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
            RegistrarInfo registrar,
            List<ViewModels.Blocks.Event.RegistrationEntry.RegistrantInfo> registrants,
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

        /// <summary>
        /// Processes the payment.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="context">The context.</param>
        /// <param name="args">The arguments.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        private Guid? ProcessPayment(
            RockContext rockContext,
            RegistrationContext context,
            RegistrationEntryBlockArgs args,
            out string errorMessage )
        {
            errorMessage = string.Empty;
            var financialGatewayService = new FinancialGatewayService( rockContext );
            var financialGateway = financialGatewayService.Get( context.RegistrationSettings.FinancialGatewayId ?? 0 );
            var gateway = financialGateway?.GetGatewayComponent();
            var redirectGateway = gateway as IRedirectionGatewayComponent;

            if ( gateway == null )
            {
                errorMessage = "There was a problem creating the payment gateway information";
                return null;
            }

            var financialAccountService = new FinancialAccountService( rockContext );
            var financialAccount = financialAccountService.Get( context.RegistrationSettings.FinancialAccountId ?? 0 );

            if ( financialAccount == null )
            {
                errorMessage = "There was a problem with the financial account configuration for this registration instance";
                return null;
            }

            var comment = redirectGateway == null ?
                $"{context.RegistrationSettings.Name} ({financialAccount.GlCode})" :
                context.RegistrationSettings.Name;

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

            return SaveTransaction( financialGateway, gateway, context, transaction, paymentInfo, rockContext, paymentInfo.Amount, paymentInfo.GatewayPersonIdentifier );
        }

        /// <summary>
        /// Saves the transaction.
        /// </summary>
        /// <param name="financialGateway">The financial gateway.</param>
        /// <param name="gateway">The gateway.</param>
        /// <param name="context">The context.</param>
        /// <param name="transaction">The transaction.</param>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="gatewayPersonIdentifier">The gateway person identifier.</param>
        /// <returns></returns>
        private Guid? SaveTransaction(
            FinancialGateway financialGateway,
            GatewayComponent gateway,
            RegistrationContext context,
            FinancialTransaction transaction,
            PaymentInfo paymentInfo,
            RockContext rockContext,
            decimal amount,
            string gatewayPersonIdentifier )
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

            var transactionDetail = transaction.TransactionDetails?.FirstOrDefault() ?? new FinancialTransactionDetail();
            transactionDetail.Amount = amount;
            transactionDetail.AccountId = context.RegistrationSettings.FinancialAccountId ?? transactionDetail.AccountId;
            transactionDetail.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.Registration ) ).Id;
            transactionDetail.EntityId = context.Registration.Id;
            transaction.TransactionDetails.Add( transactionDetail );

            var batchChanges = new History.HistoryChangeList();

            rockContext.WrapTransaction( () =>
            {
                var batchService = new FinancialBatchService( rockContext );

                // determine batch prefix
                var batchPrefix = context.RegistrationSettings.BatchNamePrefix.IsNullOrWhiteSpace() ?
                    GetAttributeValue( AttributeKey.BatchNamePrefix ) :
                    context.RegistrationSettings.BatchNamePrefix;

                // Get the batch
                var batch = batchService.Get(
                batchPrefix,
                currencyTypeValue,
                creditCardTypeValue,
                transaction.TransactionDateTime.Value,
                financialGateway.GetBatchTimeOffset() );

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
                    context.Registration.Id,
                    registrationChanges,
                    true,
                    currentPerson?.PrimaryAliasId ) );

            context.TransactionCode = transaction.TransactionCode;
            context.GatewayPersonIdentifier = gatewayPersonIdentifier;
            return transaction.Guid;
        }

        /// <summary>
        /// Gets the success view model.
        /// </summary>
        /// <param name="registrationId">The registration identifier.</param>
        /// <param name="transactionCode">The transaction code.</param>
        /// <param name="gatewayPersonIdentifier">The gateway person identifier.</param>
        /// <returns></returns>
        private RegistrationEntryBlockSuccessViewModel GetSuccessViewModel( int registrationId, string transactionCode, string gatewayPersonIdentifier )
        {
            var currentPerson = GetCurrentPerson();

            // Create a view model with default values in case anything goes wrong
            var viewModel = new RegistrationEntryBlockSuccessViewModel
            {
                TitleHtml = "Congratulations",
                MessageHtml = "You have successfully completed this registration.",
                TransactionCode = transactionCode,
                GatewayPersonIdentifier = gatewayPersonIdentifier
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

        /// <summary>
        /// Gets the allowed currency types supported by both the block and the
        /// financial gateway.
        /// </summary>
        /// <param name="gatewayComponent">The gateway component that must support the currency types.</param>
        /// <returns>A list of <see cref="DefinedValueCache"/> objects that represent the currency types.</returns>
        private List<DefinedValueCache> GetAllowedCurrencyTypes( GatewayComponent gatewayComponent )
        {
            var enableACH = true;// this.GetAttributeValue( AttributeKey.EnableACH ).AsBoolean();
            var enableCreditCard = true;// this.GetAttributeValue( AttributeKey.EnableCreditCard ).AsBoolean();
            var creditCardCurrency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD.AsGuid() );
            var achCurrency = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_ACH.AsGuid() );
            var allowedCurrencyTypes = new List<DefinedValueCache>();

            // Conditionally enable credit card.
            if ( enableCreditCard && gatewayComponent.SupportsSavedAccount( creditCardCurrency ) )
            {
                allowedCurrencyTypes.Add( creditCardCurrency );
            }

            // Conditionally enable ACH.
            if ( enableACH && gatewayComponent.SupportsSavedAccount( achCurrency ) )
            {
                allowedCurrencyTypes.Add( achCurrency );
            }

            return allowedCurrencyTypes;
        }

        /// <summary>
        /// Gets the registration session page parameter value from all possible sources.
        /// </summary>
        /// <returns>The session unique identifier or <c>null</c> if it could not be obtained.</returns>
        private Guid? GetRegistrationSessionPageParameter()
        {
            var sessionGuid = PageParameter( PageParameterKey.RegistrationSessionGuid ).AsGuidOrNull();

            if ( sessionGuid.HasValue )
            {
                return sessionGuid;
            }

            var prefixedSessionValue = RequestContext.GetPageParameters()
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
            // The page param is the least costly since there is no database call, so try that first
            var registrationInstanceId = PageParameter( PageParameterKey.RegistrationInstanceId ).AsIntegerOrNull();

            if ( registrationInstanceId.HasValue )
            {
                return registrationInstanceId.Value;
            }

            // Try a session. This is typically from a redirect
            var registrationSessionGuid = GetRegistrationSessionPageParameter();

            if ( registrationSessionGuid.HasValue )
            {
                var registrationSessionService = new RegistrationSessionService( rockContext );
                var registrationSession = registrationSessionService.Queryable()
                    .AsNoTracking()
                    .Where( rs => rs.Guid == registrationSessionGuid.Value )
                    .Select( rs => new
                    {
                        rs.RegistrationInstanceId
                    } )
                    .FirstOrDefault();

                if ( registrationSession != null )
                {
                    return registrationSession.RegistrationInstanceId;
                }
            }

            // Try a url slug
            var slug = PageParameter( PageParameterKey.Slug );

            if ( !slug.IsNullOrWhiteSpace() )
            {
                var linkage = new EventItemOccurrenceGroupMapService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( l =>
                        l.UrlSlug == slug &&
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
            var registrationId = PageParameter( PageParameterKey.RegistrationId ).AsIntegerOrNull();

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
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        private RegistrationEntryBlockSession GetRegistrationEntryBlockSession( RockContext rockContext, RegistrationSettings settings )
        {
            // Try to restore the session from the RegistrationSessionGuid, which is typically a PushPay redirect
            var registrationSessionGuid = GetRegistrationSessionPageParameter();

            if ( registrationSessionGuid.HasValue )
            {
                var registrationSessionService = new RegistrationSessionService( rockContext );
                var registrationSession = registrationSessionService.Queryable()
                    .AsNoTracking()
                    .Where( rs => rs.Guid == registrationSessionGuid.Value )
                    .Select( rs => new
                    {
                        rs.RegistrationData
                    } )
                    .FirstOrDefault();

                if ( registrationSession != null )
                {
                    return registrationSession.RegistrationData.FromJsonOrNull<RegistrationEntryBlockSession>();
                }
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
            var registration = registrationService
                .Queryable( "Registrants.PersonAlias.Person, Registrants.Fees" )
                .Include( r => r.Registrants )
                .AsNoTracking()
                .FirstOrDefault( r =>
                    r.Id == registrationId.Value && (
                        ( r.PersonAliasId.HasValue && authorizedAliasIds.Contains( r.PersonAliasId.Value ) ) ||
                        ( r.CreatedByPersonAliasId.HasValue && authorizedAliasIds.Contains( r.CreatedByPersonAliasId.Value ) )
                    ) &&
                    r.RegistrationInstanceId == settings.RegistrationInstanceId );

            if ( registration is null )
            {
                return null;
            }

            var alreadyPaid = registrationService.GetTotalPayments( registration.Id );
            var balanceDue = registration.DiscountedCost - alreadyPaid;

            if ( balanceDue < 0 )
            {
                balanceDue = 0;
            }

            // Create the base args data
            var session = new RegistrationEntryBlockSession
            {
                RegistrationSessionGuid = Guid.NewGuid(),
                AmountToPayNow = balanceDue,
                DiscountCode = registration.DiscountCode,
                DiscountAmount = registration.DiscountAmount,
                DiscountPercentage = registration.DiscountPercentage,
                FieldValues = new Dictionary<Guid, object>(),
                GatewayToken = string.Empty,
                Registrants = new List<ViewModels.Blocks.Event.RegistrationEntry.RegistrantInfo>(),
                Registrar = new RegistrarInfo(),
                RegistrationGuid = registration.Guid,
                PreviouslyPaid = alreadyPaid
            };

            // Add attributes about the registration itself
            var registrationAttributes = GetRegistrationAttributes( settings.RegistrationTemplateId );
            registration.LoadAttributes( rockContext );

            foreach ( var attribute in registrationAttributes )
            {
                var value = registration.GetAttributeValue( attribute.Key );
                session.FieldValues[attribute.Guid] = value;
            }

            // Add information about the registrants
            foreach ( var registrant in registration.Registrants )
            {
                var person = registrant.PersonAlias?.Person;
                person.LoadAttributes( rockContext );
                registrant.LoadAttributes( rockContext );

                var registrantInfo = new ViewModels.Blocks.Event.RegistrationEntry.RegistrantInfo
                {
                    FamilyGuid = person?.GetFamily( rockContext )?.Guid,
                    Guid = registrant.Guid,
                    PersonGuid = person?.Guid,
                    FieldValues = GetCurrentValueFieldValues( rockContext, person, registrant, settings.Forms ),
                    FeeItemQuantities = new Dictionary<Guid, int>(),
                    IsOnWaitList = registrant.OnWaitList
                };

                // Person fields and person attribute fields are already loaded via GetCurrentValueFieldValues, but we still need
                // to get registrant attributes
                foreach ( var form in settings.Forms )
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
                foreach ( var fee in settings.Fees.Where( f => f.IsActive ) )
                {
                    foreach ( var feeItem in fee.FeeItems.Where( f => f.IsActive ) )
                    {
                        var registrantFee = registrant.Fees.FirstOrDefault( f => f.RegistrationTemplateFeeItemId == feeItem.Id );
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
        private RegistrationContext GetContext( RockContext rockContext, RegistrationEntryBlockArgs args, out string errorMessage )
        {
            var currentPerson = GetCurrentPerson();
            var registrationInstanceId = GetRegistrationInstanceId( rockContext );
            var registrationService = new RegistrationService( rockContext );

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

            // Cannot pay more than is owed
            if ( args.AmountToPayNow > amountDue )
            {
                args.AmountToPayNow = amountDue;
            }

            var isNewRegistration = context.Registration == null;

            // Validate the charge amount is not too low according to the initial payment amount
            if ( isNewRegistration && amountDue > 0 )
            {
                var minimumInitialPayment = context.RegistrationSettings.PerRegistrantMinInitialPayment.HasValue
                    ? context.RegistrationSettings.PerRegistrantMinInitialPayment.Value * args.Registrants.Count
                    : amountDue;

                if ( args.AmountToPayNow < minimumInitialPayment )
                {
                    args.AmountToPayNow = minimumInitialPayment;
                }
            }

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
        private static decimal CalculateTotalAmountDue( RockContext rockContext, RegistrationContext context, RegistrationEntryBlockArgs args )
        {
            var registrationService = new RegistrationService( rockContext );
            var registrationInstanceService = new RegistrationInstanceService( rockContext );

            var costs = registrationInstanceService.GetRegistrationCostSummaryInfo( context, args );
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

            return registrationService.GetRegistrationContext( registrationInstanceId, PageParameter( PageParameterKey.RegistrationId ).AsIntegerOrNull(), out errorMessage );
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
        private void ProcessPostSave( RockContext rockContext, RegistrationSettings settings, RegistrationEntryBlockArgs args, bool isNewRegistration, Registration registration, List<int> previousRegistrantPersonIds, List<Action> postSaveActions )
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
                RegistrationId = registration.Id
            };

            processSendRegistrationConfirmationMsg.Send();

            if ( isNewRegistration )
            {
                // Send notice of a new registration
                new ProcesSendRegistrationNotification.Message
                {
                    RegistrationId = registration.Id
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
                        newRegistration.LaunchWorkflow( settings.RegistrantWorkflowTypeId, newRegistration.ToString(), parameters, null );
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
        private GroupMember BuildGroupMember( Person person, Group group, RegistrationSettings settings )
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
        private bool UpdateGroupMemberAttributes( GroupMember groupMember, ViewModels.Blocks.Event.RegistrationEntry.RegistrantInfo registrantInfo, RegistrationSettings settings )
        {
            bool isChanged = false;
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
                        string originalValue = groupMember.GetAttributeValue( attribute.Key );
                        string newValue = PublicAttributeHelper.GetPrivateValue( attribute, fieldValue.ToString() );
                        groupMember.SetAttributeValue( attribute.Key, newValue );

                        if ( ( originalValue ?? string.Empty ).Trim() != ( newValue ?? string.Empty ).Trim() )
                        {
                            isChanged = true;
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
        private void AddRegistrantsToGroup( RockContext rockContext, RegistrationSettings settings, Registration registration, RegistrationEntryBlockArgs args )
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
        private string GetRegistrantSignatureHashToken( ViewModels.Blocks.Event.RegistrationEntry.RegistrantInfo registrantInfo )
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
        private static SignatureDocument CreateSignatureDocument( SignatureDocumentTemplate signatureDocumentTemplate, SignedDocumentData documentData, IEntity entity, Person signedBy, Person assignedTo, Person appliesTo, String registrantName, String registrationInstanceName )
        {
            // Glue stuff into the signature document
            var signatureDocument = new SignatureDocument
            {
                SignatureDocumentTemplateId = signatureDocumentTemplate.Id,
                Status = SignatureDocumentStatus.Signed,
                Name = $"{registrantName} ({registrationInstanceName})",
                EntityTypeId = entity != null ? EntityTypeCache.GetId( entity.GetType() ) : null,
                EntityId = entity?.Id,
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

            public DateTime? AnniversaryDate { get; }

            public DateTime? BirthDate { get; }

            public CampusCache Campus { get; }

            public DefinedValueCache ConnectionStatus { get; }

            public string Email { get; }

            public string FirstName { get; }

            public Gender Gender { get; }

            public string GradeFormatted { get; }

            public int? GradeOffset { get; }

            public int? GraduationYear { get; }

            public string HomePhone { get; }

            public string LastName { get; }

            public DefinedValueCache MaritalStatus { get; }

            public string MiddleName { get; }

            public string MobilePhone { get; }

            public string WorkPhone { get; }

            public LavaHasAttributes Person { get; }

            public LavaHasAttributes GroupMember { get; }

            public LavaSignatureRegistrant( RegistrationRegistrant registrant )
                : this( registrant.Person, null, null, registrant.GroupMember )
            {
                FirstName = registrant.FirstName;
                LastName = registrant.LastName;
                Email = registrant.Email;

                Address = registrant.Person.GetHomeLocation();

                var campus = registrant.Person.GetCampus();
                if ( campus != null )
                {
                    Campus = CampusCache.Get( campus.Id );
                }
            }

            public LavaSignatureRegistrant( Person person, Location homeLocation, CampusCache campus, GroupMember groupMember )
            {
                Address = homeLocation;
                AnniversaryDate = person.AnniversaryDate;
                BirthDate = person.BirthDate;
                Campus = campus;
                ConnectionStatus = person.ConnectionStatusValueId.HasValue ? DefinedValueCache.Get( person.ConnectionStatusValueId.Value ) : null;
                Email = person.Email;
                FirstName = person.FirstName;
                Gender = person.Gender;
                GradeFormatted = person.GradeFormatted;
                GradeOffset = person.GradeOffset;
                GraduationYear = person.GraduationYear;
                HomePhone = person.GetPhoneNumber( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_HOME.AsGuid() )?.NumberFormatted;
                LastName = person.LastName;
                MaritalStatus = person.MaritalStatusValueId.HasValue ? DefinedValueCache.Get( person.MaritalStatusValueId.Value ) : null;
                MiddleName = person.MiddleName;
                MobilePhone = person.GetPhoneNumber( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() )?.NumberFormatted;
                WorkPhone = person.GetPhoneNumber( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_WORK.AsGuid() )?.NumberFormatted;

                if ( person != null )
                {
                    if ( person.Attributes == null )
                    {
                        person.LoadAttributes();
                    }

                    Person = new LavaHasAttributes
                    {
                        Attributes = person.Attributes,
                        AttributeValues = person.AttributeValues
                    };
                }

                if ( groupMember != null )
                {
                    if ( groupMember.Attributes == null )
                    {
                        groupMember.LoadAttributes();
                    }

                    GroupMember = new LavaHasAttributes
                    {
                        Attributes = groupMember.Attributes,
                        AttributeValues = groupMember.AttributeValues
                    };
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

            public Dictionary<string, string> AttributeValueDefaults => throw new NotImplementedException();

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
