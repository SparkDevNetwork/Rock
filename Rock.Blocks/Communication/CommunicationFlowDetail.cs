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
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using Rock.Attribute;
using Rock.Cms.StructuredContent;
using Rock.Communication;
using Rock.Constants;
using Rock.Data;
using Rock.Enums.Communication;
using Rock.Jobs;
using Rock.Model;
using Rock.Security;
using Rock.Security.SecurityGrantRules;
using Rock.ViewModels.Blocks.Cms.LibraryViewer;
using Rock.ViewModels.Blocks.Communication.CommunicationEntryWizard;
using Rock.ViewModels.Blocks.Communication.CommunicationFlowDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Displays the details of a particular communication flow.
    /// </summary>

    [DisplayName( "Communication Flow Detail" )]
    [Category( "Communication" )]
    [Description( "Displays the details of a particular communication flow." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [BooleanField( "Hide Step Indicator",
        Key = AttributeKey.HideStepIndicator,
        Description = "Hides the visual step indicator at the top of the block to simplify the interface.",
        DefaultBooleanValue = false,
        Order = 0 )]

    #endregion
    
    [Rock.SystemGuid.EntityTypeGuid( "27774747-4CA6-4694-A860-078A1EC7BC79" )]
    [Rock.SystemGuid.BlockTypeGuid( "CDD8DD26-E2DC-4D4A-8842-59FE00490651" )]
    public class CommunicationFlowDetail : RockBlockType, IBreadCrumbBlock
    {
        #region Keys

        private static class AttributeKey
        {
            public const string HideStepIndicator = "HideStepIndicator";
        }

        private static class PageParameterKey
        {
            public const string CommunicationFlow = "CommunicationFlow";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the visual step indicator at the top of the block should be hidden to simplify the interface.
        /// </summary>
        /// <remarks>
        /// When enabled, this hides the visual step indicator displayed at the top of the Communication Flow editor. This element helps individuals understand their current place in the process of building a communication campaign but is not interactive. Disabling it can create a cleaner, less distracting layout.
        /// </remarks>
        private bool IsStepIndicatorHidden => GetAttributeValue( AttributeKey.HideStepIndicator ).AsBoolean();
        
        #endregion

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new CommunicationFlowDetailInitializationBox();
            var currentPerson = GetCurrentPerson();

            var entity = GetInitialEntity( this.RockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {CommunicationFlow.FriendlyTypeName} was not found.";
            }
            else
            {
                if ( entity.IsAuthorized( Authorization.EDIT, currentPerson ) )
                {
                    box.CommunicationTemplates = GetSelectableCommunicationTemplates( this.RockContext, currentPerson );
                    box.Entity = GetEntityBag( entity );
                    var pushTransport = MediumContainer.GetActiveMediumComponentsWithActiveTransports().FirstOrDefault( m => m.TypeGuid == SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() )?.Transport;
                    box.IsRockMobilePushTransportConfigured = pushTransport != null && pushTransport is IRockMobilePush;
                    box.IsStepIndicatorHidden = this.IsStepIndicatorHidden;
                    box.PushMobileApplications = SiteCache.GetAllActiveSites().Where( s => s.SiteType == SiteType.Mobile ).ToListItemBagList();
                    box.SecurityGrantToken = GetSecurityGrantToken( entity, currentPerson );
                    box.SmsFromSystemPhoneNumbers = SystemPhoneNumberCache.All( false )
                        .Where( spn => spn.IsAuthorized( Authorization.VIEW, currentPerson ) )
                        .OrderBy( spn => spn.Order )
                        .ThenBy( spn => spn.Name )
                        .ThenBy( spn => spn.Id )
                        .ToListItemBagList();
                    box.TestEmailAddress = currentPerson.Email;
                    box.TestSmsPhoneNumber = currentPerson.GetPhoneNumber( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid(), this.RockContext )?.Number;

                    if ( entity.Id != 0 )
                    {
                        this.ResponseContext.SetPageTitle( entity.Name.ToStringOrDefault( CommunicationFlow.FriendlyTypeName ) );
                    }
                    else
                    {
                        this.ResponseContext.SetPageTitle( "New Flow" );
                    }
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( CommunicationFlow.FriendlyTypeName );
                }
            }

            box.NavigationUrls = GetBoxNavigationUrls();

            return box;
        }

        #region Block Actions

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( CommunicationFlowBag bag )
        {
            var entityService = new CommunicationFlowService( RockContext );

            if ( !TryGetEntityForEditAction( this.RockContext, GetCurrentPerson(), bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBag( this.RockContext, entity, bag ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidateCommunicationFlow( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( this.RockContext );
            } );

            return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
            {
                [PageParameterKey.CommunicationFlow] = entity.IdKey
            } ) );
        }

        /// <summary>
        /// Saves a communication template with the Communication Flows usage type.
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        [BlockAction( "SaveCommunicationTemplate" )]
        public BlockActionResult SaveCommunicationTemplateBlockAction( CommunicationFlowDetailCommunicationTemplateBag bag )
        {
            if ( !bag.Validate( "Communication Template" ).IsNotNull( out var validationResult ) )
            {
                return ActionBadRequest( validationResult.ErrorMessage );
            }

            var currentPerson = GetCurrentPerson();

            if ( SaveCommunicationTemplate( this.RockContext, currentPerson, bag, out var communicationTemplate, out var errorResult ) )
            {
                // Return the saved communication back to the caller.
                return ActionOk( GetCommunicationTemplateAsBag( communicationTemplate ) );
            }
            else
            {
                // Return the error message.
                return ActionBadRequest( errorResult.ErrorMessage );
            }
        }

        /// <summary>
        /// Generates and returns an HTML preview of the email content for a given communication.
        /// </summary>
        /// <param name="bag">The communication details used to generate the email preview.</param>
        /// <returns>A <see cref="BlockActionResult"/> containing the resolved HTML preview of the email communication.</returns>
        [BlockAction( "GetEmailPreviewHtml" )]
        public BlockActionResult GetEmailPreviewHtml( CommunicationFlowCommunicationBag bag )
        {
            // Validate the request.
            if ( !bag.Validate( "Request" ).IsNotNull( out var validationResult )
                 || !bag.CommunicationTemplate.Validate( "Template" ).IsNotNull( out validationResult ) )
            {
                return ActionBadRequest( validationResult.ErrorMessage );
            }

            var currentPerson = GetCurrentPerson();

            // Build the communication template (without saving).
            var communicationTemplate = GetOrCreateFlowCommunicationTemplate( this.RockContext, bag.CommunicationTemplate );
            ApplyBagToCommunicationTemplate( this.RockContext, communicationTemplate, bag.CommunicationTemplate );

            // Build the communication (without saving).
            var communication = CreateSampleCommunication( this.RockContext, currentPerson );
            ApplyBagToSampleCommunication( this.RockContext, currentPerson, communicationTemplate, communication, bag );

            // Generate the preview message.
            var commonMergeFields = this.RequestContext.GetCommonMergeFields( currentPerson );
            var mergeFields = communication.Recipients.First().CommunicationMergeValues( commonMergeFields );
            var previewHtml = GenerateEmailHtmlPreview( communication, currentPerson, mergeFields );

            return ActionOk( previewHtml );
        }

        [BlockAction( "SendTest" )]
        public BlockActionResult SendTest( CommunicationFlowCommunicationBag bag )
        {
            if ( !bag.Validate( "Request" ).IsNotNull( out var validationResult )
                || ( bag.CommunicationType == Enums.Communication.CommunicationType.Email && !bag.TestEmailAddress.Validate( "Test Email" ).IsNotNullOrWhiteSpace( out validationResult ) )
                || ( bag.CommunicationType == Enums.Communication.CommunicationType.SMS && !bag.TestSmsPhoneNumber.Validate( "Test SMS Number" ).IsNotNullOrWhiteSpace( out validationResult ) ) )
            {
                return ActionBadRequest( validationResult.ErrorMessage );
            }

            if ( SendTestCommunication( this.RockContext, GetCurrentPerson(), bag, out var communicationTemplate, out validationResult ) )
            { 
                return ActionOk( GetCommunicationTemplateAsBag( communicationTemplate ) );
            }
            else
            {
                return ActionBadRequest( validationResult.ErrorMessage );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sends a test communication based on the given communication details, creating a temporary communication record and sending it to the current user.
        /// </summary>
        /// <param name="bag">The communication details used for the test communication.</param>
        private bool SendTestCommunication( RockContext rockContext, Person currentPerson, CommunicationFlowCommunicationBag bag, out CommunicationTemplate communicationTemplate, out ValidationResult validationResult )
        {
            // First step is to save the template.
            if ( !SaveCommunicationTemplate( rockContext, currentPerson, bag.CommunicationTemplate, out communicationTemplate, out validationResult ) )
            {
                return false;
            }

            var testPersonId = currentPerson.Id;
            var testPersonOriginalEmailAddress = currentPerson.Email;
            var testPersonOriginalSMSPhoneNumber = currentPerson.PhoneNumbers
                .Where( p => p.IsMessagingEnabled )
                .Select( a => a.Number )
                .FirstOrDefault();

            // Next step is to create the sample communication to send.
            void PrepareCommunicationForSendTest( Model.Communication c )
            {
                c.Status = CommunicationStatus.Approved;
                c.ReviewedDateTime = RockDateTime.Now;
                c.ReviewerPersonAliasId = currentPerson.PrimaryAliasId;

                if ( c.Recipients.Any() )
                {
                    var testRecipient = c.Recipients.First();

                    testRecipient.MediumEntityTypeId = bag.CommunicationType == Enums.Communication.CommunicationType.SMS
                        ? EntityTypeCache.GetId( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS )
                        : EntityTypeCache.GetId( SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL );
                    testRecipient.Status = CommunicationRecipientStatus.Pending;
                }
                
                if ( bag.CommunicationType == Enums.Communication.CommunicationType.Email )
                {
                    c.Subject = $"[Test] {c.Subject}".TrimForMaxLength( c, nameof( Model.Communication.Subject ) );
                    currentPerson.Email = bag.TestEmailAddress.ToStringOrDefault( currentPerson.Email );
                }
                else if ( bag.CommunicationType == Enums.Communication.CommunicationType.SMS )
                {
                    c.SMSMessage = $"[Test] {c.SMSMessage}";

                    var smsPhoneNumber = currentPerson.PhoneNumbers.FirstOrDefault( a => a.IsMessagingEnabled == true );

                    if ( smsPhoneNumber == null )
                    {
                        var mobilePhoneValueId = DefinedValueCache.Get( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;
                        var testPhoneNumber = new PhoneNumber
                        {
                            // Add an SMS-enabled number to send the test message to.
                            IsMessagingEnabled = true,
                            CountryCode = PhoneNumber.DefaultCountryCode(),
                            NumberTypeValueId = mobilePhoneValueId,
                            Number = bag.TestSmsPhoneNumber,
                            NumberFormatted = PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), bag.TestSmsPhoneNumber ),
                            ForeignKey = "_ForTestCommunication_"
                        };

                        currentPerson.PhoneNumbers.Add( testPhoneNumber );
                    }
                    else
                    {
                        if ( smsPhoneNumber.Number != bag.TestSmsPhoneNumber )
                        {
                            smsPhoneNumber.Number = bag.TestSmsPhoneNumber;
                            smsPhoneNumber.NumberFormatted = PhoneNumber.FormattedNumber( smsPhoneNumber.CountryCode, smsPhoneNumber.Number );
                        }
                    }
                }
            }

            if ( !SaveSampleCommunication( rockContext, currentPerson, communicationTemplate, bag, PrepareCommunicationForSendTest, out var testCommunication, out validationResult ) )
            {
                return false;
            }

            try
            {
                foreach ( var medium in testCommunication.GetMediums() )
                {
                    medium.Send( testCommunication );
                }

                var testRecipient = new CommunicationRecipientService( rockContext )
                    .Queryable().AsNoTracking()
                    .Include( r => r.PersonAlias )
                    .Include( r => r.PersonAlias.Person )
                    .Where( r => r.CommunicationId == testCommunication.Id )
                    .FirstOrDefault();

                if ( testRecipient != null
                        && testRecipient.Status == CommunicationRecipientStatus.Failed
                        && testRecipient.PersonAlias != null
                        && testRecipient.PersonAlias.Person != null )
                {
                    validationResult = new ValidationResult($"Test communication to <strong>{testRecipient.PersonAlias.Person.FullName}</strong> failed: {testRecipient.StatusNote}.");
                    return false;
                }
            }
            finally
            {
                try
                {
                    // Delete the test communication record we created to send the test.
                    var communicationService = new CommunicationService( rockContext );

                    var testCommunicationId = testCommunication.Id;
                    var pushMediumEntityTypeGuid = SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid();

                    if ( testCommunication.GetMediums().Any( a => a.EntityType.Guid == pushMediumEntityTypeGuid ) )
                    {
                        // We can't actually delete the test communication since if it is an
                        // action type of "Show Details" then they won't be able to view the
                        // communication on their device to see how it looks. Instead we switch
                        // the communication to be transient so the cleanup job will take care
                        // of it later.
                        testCommunication.Status = CommunicationStatus.Transient;
                    }
                    else
                    {
                        communicationService.Delete( testCommunication );
                    }

                    rockContext.SaveChanges( disablePrePostProcessing: true );

                    // Delete any Person History that was created for the Test Communication.
                    var categoryId = CategoryCache.Get( Rock.SystemGuid.Category.HISTORY_PERSON_COMMUNICATIONS.AsGuid() ).Id;
                    var communicationEntityTypeId = EntityTypeCache.Get( "Rock.Model.Communication" ).Id;
                    var historyService = new HistoryService( rockContext );
                    var communicationHistoryQuery = historyService.Queryable()
                        .Where( a =>
                            a.CategoryId == categoryId
                            && a.RelatedEntityTypeId == communicationEntityTypeId
                            && a.RelatedEntityId == testCommunicationId
                            );

                    foreach ( var communicationHistory in communicationHistoryQuery )
                    {
                        historyService.Delete( communicationHistory );
                    }

                    rockContext.SaveChanges( disablePrePostProcessing: true );
                }
                catch ( Exception ex )
                {
                    // just log the exception, don't show it
                    ExceptionLogService.LogException( ex );
                }

                try
                {
                    // make sure we restore the CurrentPerson's email/SMS number if it was changed for the test
                    var restorePersonService = new PersonService( rockContext );
                    var personToUpdate = restorePersonService.Get( testPersonId );

                    if ( bag.CommunicationType == Enums.Communication.CommunicationType.Email )
                    {
                        if ( personToUpdate.Email != testPersonOriginalEmailAddress )
                        {
                            personToUpdate.Email = testPersonOriginalEmailAddress;
                            rockContext.SaveChanges( disablePrePostProcessing: true );
                        }
                    }
                    else if ( bag.CommunicationType == Enums.Communication.CommunicationType.SMS )
                    {
                        var defaultSMSNumber = personToUpdate.PhoneNumbers.Where( p => p.IsMessagingEnabled ).FirstOrDefault();
                        if ( defaultSMSNumber != null
                             && defaultSMSNumber.Number != testPersonOriginalSMSPhoneNumber )
                        {
                            if ( testPersonOriginalSMSPhoneNumber == null )
                            {
                                if ( defaultSMSNumber.ForeignKey == "_ForTestCommunication_" )
                                {
                                    new PhoneNumberService( rockContext ).Delete( defaultSMSNumber );
                                    rockContext.SaveChanges( disablePrePostProcessing: true );
                                }
                            }
                            else
                            {
                                defaultSMSNumber.Number = testPersonOriginalSMSPhoneNumber;
                                defaultSMSNumber.NumberFormatted = PhoneNumber.FormattedNumber( defaultSMSNumber.CountryCode, defaultSMSNumber.Number );
                                rockContext.SaveChanges( disablePrePostProcessing: true );
                            }
                        }
                    }
                }
                catch ( Exception ex )
                {
                    // just log the exception, don't show it
                    ExceptionLogService.LogException( ex );
                }
            }

            return true;
        }

        private string GenerateEmailHtmlPreview( Model.Communication communication, Person currentPerson, Dictionary<string, object> mergeFields )
        {
            var emailMediumWithActiveTransport = MediumContainer
                .GetActiveMediumComponentsWithActiveTransports()
                .Where( a => a.EntityType.Guid == Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() )
                .FirstOrDefault();

            var previewHtml = communication.Message;

            if ( emailMediumWithActiveTransport != null )
            {
                var mediumAttributes = new Dictionary<string, string>();

                foreach ( var attr in emailMediumWithActiveTransport.Attributes.Select( a => a.Value ) )
                {
                    var value = emailMediumWithActiveTransport.GetAttributeValue( attr.Key );

                    if ( value.IsNotNullOrWhiteSpace() )
                    {
                        mediumAttributes.Add( attr.Key, value );
                    }
                }

                var publicAppRoot = GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" );

                // Add HTML view
                // Get the unsubscribe content and add a merge field for it
                if ( communication.IsBulkCommunication && mediumAttributes.ContainsKey( "UnsubscribeHTML" ) )
                {
                    var unsubscribeHtml = emailMediumWithActiveTransport.Transport.ResolveText( mediumAttributes["UnsubscribeHTML"], currentPerson, communication.EnabledLavaCommands, mergeFields, publicAppRoot );
                    mergeFields.AddOrReplace( "UnsubscribeOption", unsubscribeHtml );
                    previewHtml = emailMediumWithActiveTransport.Transport.ResolveText( previewHtml, currentPerson, communication.EnabledLavaCommands, mergeFields, publicAppRoot );

                    // Resolve special syntax needed if option was included in global attribute
                    if ( Regex.IsMatch( previewHtml, @"\[\[\s*UnsubscribeOption\s*\]\]" ) )
                    {
                        previewHtml = Regex.Replace( previewHtml, @"\[\[\s*UnsubscribeOption\s*\]\]", unsubscribeHtml );
                    }

                    // Add the unsubscribe option at end if it wasn't included in content
                    if ( !previewHtml.Contains( unsubscribeHtml ) )
                    {
                        previewHtml += unsubscribeHtml;
                    }
                }
                else
                {
                    previewHtml = emailMediumWithActiveTransport.Transport.ResolveText( previewHtml, currentPerson, communication.EnabledLavaCommands, mergeFields, publicAppRoot );
                    previewHtml = Regex.Replace( previewHtml, @"\[\[\s*UnsubscribeOption\s*\]\]", string.Empty );
                }

                if ( communication.CommunicationTemplate != null && communication.CommunicationTemplate.CssInliningEnabled )
                {
                    previewHtml = previewHtml.ConvertHtmlStylesToInlineAttributes();
                }
            }

            return previewHtml;
        }

        private bool SaveCommunicationTemplate( RockContext rockContext, Person currentPerson, CommunicationFlowDetailCommunicationTemplateBag bag, out CommunicationTemplate communicationTemplate, out ValidationResult errorResult )
        {
            communicationTemplate = GetOrCreateFlowCommunicationTemplate( this.RockContext, bag );

            ApplyBagToCommunicationTemplate( this.RockContext, communicationTemplate, bag );

            if ( !ValidateCommunicationTemplate( this.RockContext, currentPerson, communicationTemplate, out errorResult ) )
            {
                return false;
            }

            SaveCommunicationTemplateToDatabase( this.RockContext, currentPerson, ref communicationTemplate );

            return true;
        }

        private Model.Communication CreateSampleCommunication( RockContext rockContext, Person currentPerson )
        {
            var communicationService = new CommunicationService( rockContext );

            var communication = new Model.Communication
            {
                Status = CommunicationStatus.Transient,
                SenderPersonAliasId = currentPerson.PrimaryAliasId
            };
            communicationService.Add( communication );

            return communication;
        }

        private PushData GetPushDataFromCommunicationTemplateBag( CommunicationFlowDetailCommunicationTemplateBag bag )
        {
            var newMobileApplicationGuid = bag.PushMobileApplication?.Value?.AsGuidOrNull();
            var mobileApplicationId =
                newMobileApplicationGuid.HasValue
                ? SiteCache.GetId( newMobileApplicationGuid.Value )
                : null;
            var newMobilePageGuid = bag.PushMobilePage?.Page?.Value.AsGuid();
            var mobilePage =
                newMobilePageGuid.HasValue
                ? PageCache.Get( newMobilePageGuid.Value )
                : null;

            switch ( bag.PushOpenAction )
            {
                case PushOpenActionType.LinkToUrl:
                    return new PushData
                    {
                        MobileApplicationId = mobileApplicationId,
                        Url = bag.PushUrl
                    };

                case PushOpenActionType.LinkToMobilePage:
                    return new PushData
                    {
                        // Get the mobile application from the page.
                        MobileApplicationId = mobilePage?.SiteId,
                        MobilePageId = mobilePage?.Id,
                        MobilePageQueryString = bag.PushMobilePageQueryString
                    };

                case PushOpenActionType.ShowDetails:
                    return new PushData
                    {
                        MobileApplicationId = mobileApplicationId
                    };

                case PushOpenActionType.NoAction:
                default:
                    return new PushData
                    {
                        MobileApplicationId = mobileApplicationId
                    };
            }
        }

        private void ApplyBagToSampleCommunication( RockContext rockContext, Person currentPerson, CommunicationTemplate communicationTemplate, Model.Communication communication, CommunicationFlowCommunicationBag bag )
        {
            var binaryFileService = new BinaryFileService( rockContext );
            var communicationAttachmentService = new CommunicationAttachmentService( rockContext );

            // Entity properties.
            communication.BCCEmails = null;
            communication.CCEmails = null;
            communication.CommunicationType = ( Model.CommunicationType ) ( int ) bag.CommunicationType;
            communication.EnabledLavaCommands = null;
            communication.ExcludeDuplicateRecipientAddress = false;
            communication.FromEmail = bag.CommunicationTemplate.FromEmail.TrimForMaxLength( communication, nameof( Model.Communication.FromEmail ) );
            communication.FromName = bag.CommunicationTemplate.FromName.TrimForMaxLength( communication, nameof( Model.Communication.FromName ) );
            communication.FutureSendDateTime = null;
            communication.IsBulkCommunication = false;
            communication.Message = bag.CommunicationTemplate.Message;
            communication.Name = bag.Name.TrimForMaxLength( communication, nameof( Model.Communication.Name ) );
            communication.PersonalizationSegments = null;
            communication.PushData = GetPushDataFromCommunicationTemplateBag( bag.CommunicationTemplate ).ToJson();
            communication.PushMessage = bag.CommunicationTemplate.PushMessage;
            communication.PushOpenAction = ( Rock.Utility.PushOpenAction? ) ( int? ) bag.CommunicationTemplate.PushOpenAction;
            communication.PushOpenMessage = bag.CommunicationTemplate.PushOpenMessage;
            communication.PushOpenMessageJson = bag.CommunicationTemplate.PushOpenMessageJson;
            communication.PushTitle = bag.CommunicationTemplate.PushTitle.TrimForMaxLength( communication, nameof( Model.Communication.PushTitle ) );
            communication.ReplyToEmail = bag.CommunicationTemplate.ReplyToEmail.TrimForMaxLength( communication, nameof( Model.Communication.ReplyToEmail ) );
            communication.Segments = null;
            communication.SegmentCriteria = SegmentCriteria.All;
            communication.SMSMessage = bag.CommunicationTemplate.SmsMessage;
            communication.Subject = bag.CommunicationTemplate.Subject.TrimForMaxLength( communication, nameof( Model.Communication.Subject ) );

            // Navigation properties.
            var smsFromSystemPhoneNumberGuid = bag.CommunicationTemplate.SmsFromSystemPhoneNumber?.Value?.AsGuidOrNull();

            communication.CommunicationTemplate = communicationTemplate;
            communication.CommunicationTemplateId = communicationTemplate.Id;
            communication.CommunicationTopicValue = null;
            communication.CommunicationTopicValueId = null;
            communication.ListGroup = null; // No list group for sample communications.
            communication.ListGroupId = null;
            communication.PushImageBinaryFileId = null;
            communication.SmsFromSystemPhoneNumberId = smsFromSystemPhoneNumberGuid.HasValue
                ? SystemPhoneNumberCache.GetId( smsFromSystemPhoneNumberGuid.Value )
                : null;

            // Email attachments.
            var newEmailAttachmentBinaryFileGuids = bag.CommunicationTemplate.EmailAttachmentBinaryFiles
                ?.Select( a => a.Value.AsGuid() )
                .Where( a => !a.IsEmpty() )
                .ToList() ?? new List<Guid>();

            if ( newEmailAttachmentBinaryFileGuids.Any() )
            {
                // Delete old email attachments.
                var newEmailAttachmentBinaryFiles = binaryFileService.Queryable()
                    .Where( c => newEmailAttachmentBinaryFileGuids.Contains( c.Guid ) )
                    .ToList();
                var newEmailAttachmentBinaryFileIds = newEmailAttachmentBinaryFiles.Select( a => a.Id ).ToList();
                var oldEmailAttachmentsToDelete = communication.GetAttachments( Model.CommunicationType.Email )
                    .Where( oldEmailAttachment => !newEmailAttachmentBinaryFileIds.Contains( oldEmailAttachment.BinaryFileId ) )
                    .ToList();

                foreach ( var oldEmailAttachmentToDelete in oldEmailAttachmentsToDelete )
                {
                    // Mark the attachment's binary file as temporary so it gets deleted by the clean-up job.
                    if ( oldEmailAttachmentToDelete.BinaryFile != null )
                    {
                        oldEmailAttachmentToDelete.BinaryFile.IsTemporary = true;
                    }

                    communication.Attachments.Remove( oldEmailAttachmentToDelete );
                    communicationAttachmentService.Delete( oldEmailAttachmentToDelete );
                }

                // Add new email attachments.
                var oldAttachmentBinaryFileIds = communication.GetAttachments( Model.CommunicationType.Email )
                    .Select( a => a.BinaryFileId )
                    .ToList();
                var newEmailAttachmentsToAdd = newEmailAttachmentBinaryFiles
                    .Where( newEmailAttachmentBinaryFile => !oldAttachmentBinaryFileIds.Contains( newEmailAttachmentBinaryFile.Id ) )
                    .Select( newAttachmentBinaryFile => new CommunicationAttachment
                    {
                        BinaryFile = newAttachmentBinaryFile,
                        BinaryFileId = newAttachmentBinaryFile.Id,
                        CommunicationType = Model.CommunicationType.Email
                    } )
                    .ToList();

                foreach ( var newEmailAttachmentToAdd in newEmailAttachmentsToAdd )
                {
                    communication.Attachments.Add( newEmailAttachmentToAdd );
                }
            }
            else
            {
                // Delete all email attachments.
                var oldEmailAttachmentsToDelete = communication.GetAttachments( Model.CommunicationType.Email );
                foreach ( var oldEmailAttachmentToDelete in oldEmailAttachmentsToDelete )
                {
                    // Mark the attachment's binary file as temporary so it gets deleted by the clean-up job.
                    if ( oldEmailAttachmentToDelete.BinaryFile != null )
                    {
                        oldEmailAttachmentToDelete.BinaryFile.IsTemporary = true;
                    }

                    communication.Attachments.Remove( oldEmailAttachmentToDelete );
                    communicationAttachmentService.Delete( oldEmailAttachmentToDelete );
                }
            }

            // SMS attachments.
            var newSmsAttachmentBinaryFileGuid = bag.CommunicationTemplate.SmsAttachmentBinaryFile?.Value.AsGuidOrNull();
            var newSmsAttachmentBinaryFile = newSmsAttachmentBinaryFileGuid.HasValue
                ? binaryFileService.Get( newSmsAttachmentBinaryFileGuid.Value )
                : null;

            if ( newSmsAttachmentBinaryFile != null )
            {
                // Delete old SMS attachments.
                var oldSmsAttachmentsToDelete = communication.GetAttachments( Model.CommunicationType.SMS )
                    .Where( oldSmsAttachment => oldSmsAttachment.BinaryFileId != newSmsAttachmentBinaryFile.Id )
                    .ToList();

                foreach ( var oldSmsAttachmentToDelete in oldSmsAttachmentsToDelete )
                {
                    // Mark the attachment's binary file as temporary so it gets deleted by the clean-up job.
                    if ( oldSmsAttachmentToDelete.BinaryFile != null )
                    {
                        oldSmsAttachmentToDelete.BinaryFile.IsTemporary = true;
                    }

                    communication.Attachments.Remove( oldSmsAttachmentToDelete );
                    communicationAttachmentService.Delete( oldSmsAttachmentToDelete );
                }

                // Add new SMS attachment.
                communication.Attachments.Add( new CommunicationAttachment
                {
                    BinaryFile = newSmsAttachmentBinaryFile,
                    BinaryFileId = newSmsAttachmentBinaryFile.Id,
                    CommunicationType = Rock.Model.CommunicationType.SMS
                } );
            }
            else
            {
                // Delete all SMS attachments.
                var oldSmsAttachmentsToDelete = communication.GetAttachments( Model.CommunicationType.SMS );
                foreach ( var oldSmsAttachmentToDelete in oldSmsAttachmentsToDelete )
                {
                    // Mark the attachment's binary file as temporary so it gets deleted by the clean-up job.
                    if ( oldSmsAttachmentToDelete.BinaryFile != null )
                    {
                        oldSmsAttachmentToDelete.BinaryFile.IsTemporary = true;
                    }

                    communication.Attachments.Remove( oldSmsAttachmentToDelete );
                    communicationAttachmentService.Delete( oldSmsAttachmentToDelete );
                }
            }

            // TODO JMH Recipients.
            communication.Recipients.Add( new CommunicationRecipient
            {
                PersonAlias = currentPerson.PrimaryAlias
            } );
        }

        private bool ValidateCommunication( Model.Communication communication, out ValidationResult validationResult )
        {
            // Standard entity validation.
            if ( !communication.IsValid )
            {
                var errorMessage = communication.ValidationResults.Select( v => v.ErrorMessage ).JoinStrings( "\n" );
                validationResult = new ValidationResult( errorMessage );
                return false;
            }
            else
            {
                validationResult = ValidationResult.Success;
                return true;
            }
        }

        private void SaveSampleCommunicationToDatabase( RockContext rockContext )
        {
            rockContext.SaveChanges( disablePrePostProcessing: true );
        }

        private bool SaveSampleCommunication( RockContext rockContext, Person currentPerson, CommunicationTemplate communicationTemplate, CommunicationFlowCommunicationBag bag, out Model.Communication communication, out ValidationResult errorResult )
        {
            return SaveSampleCommunication( rockContext, currentPerson, communicationTemplate, bag, null, out communication, out errorResult );
        }

        private bool SaveSampleCommunication(
            RockContext rockContext,
            Person currentPerson,
            CommunicationTemplate communicationTemplate,
            CommunicationFlowCommunicationBag bag,
            Action<Model.Communication> preSaveOperation,
            out Model.Communication communication,
            out ValidationResult errorResult )
        {
            communication = CreateSampleCommunication( rockContext, currentPerson );

            ApplyBagToSampleCommunication( rockContext, currentPerson, communicationTemplate, communication, bag );

            preSaveOperation?.Invoke( communication );

            if ( !ValidateCommunication( communication, out errorResult ) )
            {
                return false;
            }

            SaveSampleCommunicationToDatabase( rockContext );

            return true;
        }
        
        private CommunicationTemplate GetOrCreateFlowCommunicationTemplate( RockContext rockContext, CommunicationFlowDetailCommunicationTemplateBag bag )
        {
            var communicationTemplateService = new CommunicationTemplateService( rockContext );
            CommunicationTemplate communicationTemplate = null;

            if ( !bag.Guid.IsEmpty() )
            {
                communicationTemplate = communicationTemplateService
                    .Queryable()
                    .Include( c => c.Attachments )
                    .FirstOrDefault( c =>
                        c.Guid == bag.Guid
                        // Ensure this is a "CommunicationFlows", non-Legacy template
                        // so it can be used with this block. If the guid matches, but the other
                        // requirements are not met, then we'll create a new template with a new guid.
                        && c.Version != CommunicationTemplateVersion.Legacy
                        && c.UsageType == CommunicationTemplateUsageType.CommunicationFlows );
            }

            if ( communicationTemplate == null )
            {
                communicationTemplate = new CommunicationTemplate
                {
                    // Use Beta version at a minimum so the Obsidian
                    // Email Editor can be used to edit the email message.
                    Version = CommunicationTemplateVersion.Beta,
                    UsageType = CommunicationTemplateUsageType.CommunicationFlows
                };

                communicationTemplateService.Add( communicationTemplate );
            }

            return communicationTemplate;
        }

        private void ApplyBagToCommunicationTemplate( RockContext rockContext, CommunicationTemplate communicationTemplate, CommunicationFlowDetailCommunicationTemplateBag bag )
        {
            var binaryFileService = new BinaryFileService( rockContext );
            var communicationTemplateAttachmentService = new CommunicationTemplateAttachmentService( rockContext );

            var newImageFileGuid = bag.ImageFile?.Value?.AsGuidOrNull();
            if ( communicationTemplate.ImageFile?.Guid != newImageFileGuid )
            {
                if ( communicationTemplate.ImageFile != null )
                {
                    // The old image template preview won't be needed anymore, so make it IsTemporary and have it get cleaned up later.
                    communicationTemplate.ImageFile.IsTemporary = true;
                }

                if ( newImageFileGuid.HasValue )
                {
                    communicationTemplate.ImageFile = binaryFileService.Get( newImageFileGuid.Value );
                    communicationTemplate.ImageFileId = communicationTemplate.ImageFile?.Id;
                }
                else
                {
                    communicationTemplate.ImageFile = null;
                    communicationTemplate.ImageFileId = null;
                }
            }

            if ( communicationTemplate.ImageFile != null )
            {
                // Ensure that the ImagePreview is not set as IsTemporary=True so it does not get deleted,
                // whether it is a new image or an existing image.
                communicationTemplate.ImageFile.IsTemporary = false;
            }

            if ( communicationTemplate.LogoBinaryFile != null )
            {
                // Ensure that the LogoBinaryFile is not set as IsTemporary=True so it does not get deleted,
                // whether it is a new image or an existing image.
                communicationTemplate.LogoBinaryFile.IsTemporary = false;
            }

            // Only allow these edits if this is not a system communication.
            if ( !communicationTemplate.IsSystem )
            {
                communicationTemplate.Name = bag.Name;
                communicationTemplate.FromName = bag.FromName;
                communicationTemplate.FromEmail = bag.FromEmail;
                communicationTemplate.ReplyToEmail = bag.ReplyToEmail;
                communicationTemplate.Subject = bag.Subject;
                communicationTemplate.Message = bag.Message;

                var binaryFileGuids = bag.EmailAttachmentBinaryFiles?.Select( a => a.Value ).AsGuidList() ?? new List<Guid>();
                var binaryFileIds = binaryFileService.Queryable()
                    .Where( b => binaryFileGuids.Contains( b.Guid ) )
                    .Select( b => b.Id )
                    .ToList();

                // Delete any attachments that are no longer included.
                foreach ( var attachment in communicationTemplate.Attachments
                    .Where( a => !binaryFileIds.Contains( a.BinaryFileId ) ).ToList() )
                {
                    communicationTemplate.Attachments.Remove( attachment );
                    communicationTemplateAttachmentService.Delete( attachment );
                }

                // Add any new attachments that were added.
                foreach ( var binaryFileId in binaryFileIds
                    .Where( a => communicationTemplate.Attachments.All( x => x.BinaryFileId != a ) ) )
                {
                    communicationTemplate.Attachments.Add( new CommunicationTemplateAttachment
                    {
                        BinaryFileId = binaryFileId,
                        CommunicationType = Rock.Model.CommunicationType.Email
                    } );
                }
                
                var newSmsFromSystemPhoneNumberGuid = bag.SmsFromSystemPhoneNumber?.Value?.AsGuidOrNull();
                if ( communicationTemplate.SmsFromSystemPhoneNumber?.Guid != newSmsFromSystemPhoneNumberGuid )
                {
                    if ( newSmsFromSystemPhoneNumberGuid.HasValue )
                    {
                        communicationTemplate.SmsFromSystemPhoneNumber = new SystemPhoneNumberService( this.RockContext ).Get( newSmsFromSystemPhoneNumberGuid.Value );
                        communicationTemplate.SmsFromSystemPhoneNumberId = communicationTemplate.SmsFromSystemPhoneNumber?.Id;
                    }
                    else
                    {
                        communicationTemplate.SmsFromSystemPhoneNumber = null;
                        communicationTemplate.SmsFromSystemPhoneNumberId = null;
                    }
                }
                
                communicationTemplate.SMSMessage = bag.SmsMessage;
            }

            // No "Is System" push restrictions in the old block,
            // so we'll just allow edits if the user can edit the communication template.
            communicationTemplate.PushTitle = bag.PushTitle;
            communicationTemplate.PushMessage = bag.PushMessage;
            communicationTemplate.PushOpenAction = bag.PushOpenAction.HasValue ? ( Rock.Utility.PushOpenAction ) ( int ) bag.PushOpenAction : default;

            var pushData = new PushData();
            var newPushOpenMessageJson = default( string );
            var newPushOpenMessage = default( string );

            if ( communicationTemplate.PushOpenAction == Rock.Utility.PushOpenAction.ShowDetails )
            {
                if ( bag.PushOpenMessageJson.IsNotNullOrWhiteSpace() )
                {
                    newPushOpenMessageJson = bag.PushOpenMessageJson;
                    newPushOpenMessage = new StructuredContentHelper( bag.PushOpenMessageJson ).Render();
                }
                else
                {
                    newPushOpenMessage = bag.PushOpenMessage;
                }

                var newMobileApplicationGuid = bag.PushMobileApplication?.Value?.AsGuidOrNull();
                if ( newMobileApplicationGuid.HasValue )
                {
                    pushData.MobileApplicationId = SiteCache.GetId( newMobileApplicationGuid.Value );
                }
                else
                {
                    pushData.MobileApplicationId = null;
                }
            }
            else if ( communicationTemplate.PushOpenAction == Rock.Utility.PushOpenAction.LinkToMobilePage )
            {
                pushData.MobilePageQueryString = bag.PushMobilePageQueryString;

                var newMobilePageGuid = bag.PushMobilePage?.Page?.Value.AsGuid();
                if ( newMobilePageGuid.HasValue )
                {
                    var newMobilePage = PageCache.Get( newMobilePageGuid.Value ); 
                    pushData.MobilePageId = newMobilePage?.Id;
                    pushData.MobileApplicationId = newMobilePage?.SiteId;
                }
                else
                {
                    pushData.MobilePageId = null;
                    pushData.MobileApplicationId = null;
                }
            }
            else if ( communicationTemplate.PushOpenAction == Rock.Utility.PushOpenAction.LinkToUrl )
            {
                pushData.Url = bag.PushUrl;
            }

            communicationTemplate.PushOpenMessage = newPushOpenMessage;
            communicationTemplate.PushOpenMessageJson = newPushOpenMessageJson;
            communicationTemplate.PushData = pushData.ToJson();
        }

        private bool ValidateCommunicationTemplate( RockContext rockContext, Person currentPerson, CommunicationTemplate communicationTemplate, out ValidationResult errorResult )
        {
            // Logic validation.
            var isNewTemplate = communicationTemplate.Id == 0;

            if ( !isNewTemplate && !communicationTemplate.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                errorResult = new ValidationResult( "You do not have permission to edit this communication template." );
                return false;
            }

            if ( communicationTemplate.UsageType != CommunicationTemplateUsageType.CommunicationFlows )
            {
                errorResult = new ValidationResult( "This communication template is not a Communication Flow template." );
                return false;
            }

            // Standard entity validation.
            if ( !communicationTemplate.IsValid )
            {
                var validationErrorMessage = communicationTemplate.ValidationResults
                    .Select( a => a.ErrorMessage )
                    .ToList()
                    .AsDelimited( "\n" );
                errorResult = new ValidationResult( validationErrorMessage );
                return false;
            }

            errorResult = null;
            return true;
        }

        private void SaveCommunicationTemplateToDatabase( RockContext rockContext, Person currentPerson, ref CommunicationTemplate communicationTemplate )
        {
            var isNewTemplate = communicationTemplate.Id == 0;

            rockContext.SaveChanges();

            if ( isNewTemplate )
            {
                var communicationTemplateService = new CommunicationTemplateService( rockContext );

                // Read the saved communication template to ensure persisted properties are populated.
                communicationTemplate = communicationTemplateService.GetWithCommunicationTemplateBagIncludes( communicationTemplate.Id );

                if ( communicationTemplate != null )
                {
                    // Make sure logged-in person can view and edit the new template.
                    if ( !communicationTemplate.IsAuthorized( Authorization.VIEW, currentPerson ) )
                    {
                        communicationTemplate.AllowPerson( Authorization.VIEW, currentPerson );
                    }

                    // Make sure logged-in person can edit the new template.
                    if ( !communicationTemplate.IsAuthorized( Authorization.EDIT, currentPerson ) )
                    {
                        communicationTemplate.AllowPerson( Authorization.EDIT, currentPerson );
                    }

                    // Always make sure RSR-Admin and Communication Admin can see the new template.
                    var groupService = new GroupService( this.RockContext );
                    var communicationAdministrators = groupService.Get( SystemGuid.Group.GROUP_COMMUNICATION_ADMINISTRATORS.AsGuid() );
                    if ( communicationAdministrators != null )
                    {
                        communicationTemplate.AllowSecurityRole( Authorization.VIEW, communicationAdministrators, this.RockContext );
                        communicationTemplate.AllowSecurityRole( Authorization.EDIT, communicationAdministrators, this.RockContext );
                        communicationTemplate.AllowSecurityRole( Authorization.ADMINISTRATE, communicationAdministrators, this.RockContext );
                    }
                }
            }
        }

        private CommunicationFlowDetailCommunicationTemplateBag GetCommunicationTemplateAsBag( CommunicationTemplate communicationTemplate )
        {
            if ( communicationTemplate == null )
            {
                return null;
            }

            var bag = new CommunicationFlowDetailCommunicationTemplateBag();
            var pushCommunication = new CommunicationDetails();

            bag.Guid = communicationTemplate.Guid;

            pushCommunication = new CommunicationDetails
            {
                PushData = communicationTemplate.PushData,
                PushImageBinaryFileId = communicationTemplate.PushImageBinaryFileId,
                PushMessage = communicationTemplate.PushMessage,
                PushTitle = communicationTemplate.PushTitle,
                PushOpenMessage = communicationTemplate.PushOpenMessage,
                PushOpenMessageJson = communicationTemplate.PushOpenMessageJson,
                PushOpenAction = communicationTemplate.PushOpenAction
            };

            bag.Name = communicationTemplate.Name;
            bag.ImageFile = communicationTemplate.ImageFile.ToListItemBag();

            // Email Fields
            bag.FromName = communicationTemplate.FromName;
            bag.FromEmail = communicationTemplate.FromEmail;
            bag.ReplyToEmail = communicationTemplate.ReplyToEmail;
            bag.Subject = communicationTemplate.Subject;
            bag.Message = communicationTemplate.Message;
            bag.EmailAttachmentBinaryFiles = communicationTemplate.Attachments.Select( a => a.BinaryFile ).ToListItemBagList();

            // SMS Fields
            bag.SmsFromSystemPhoneNumber = communicationTemplate.SmsFromSystemPhoneNumber.ToListItemBag();
            bag.SmsMessage = communicationTemplate.SMSMessage;

            // Push Fields
            var pushTransport = MediumContainer.GetActiveMediumComponentsWithActiveTransports().FirstOrDefault( m => m.TypeGuid == SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() )?.Transport;
            bag.PushTitle = pushCommunication.PushTitle;
            bag.PushMessage = pushCommunication.PushMessage;

            if ( communicationTemplate.PushOpenAction != null )
            {
                bag.PushOpenAction = ( PushOpenActionType )( int )communicationTemplate.PushOpenAction.Value;
            }

            PushData pushData;
            if ( communicationTemplate.PushData.IsNotNullOrWhiteSpace() )
            {
                pushData = communicationTemplate.PushData.FromJsonOrNull<PushData>() ?? new PushData();
            }
            else
            {
                pushData = new PushData();
            }

            if ( communicationTemplate.PushOpenAction == Rock.Utility.PushOpenAction.ShowDetails )
            {
                if ( pushData.MobileApplicationId.HasValue )
                {
                    var mobileApplication = SiteCache.Get( pushData.MobileApplicationId.Value );

                    if ( mobileApplication != null )
                    {
                        bag.PushMobileApplication = mobileApplication.ToListItemBag();
                    }
                }

                bag.PushOpenMessageJson = communicationTemplate.PushOpenMessageJson;

                if ( communicationTemplate.PushOpenMessageJson.IsNotNullOrWhiteSpace() )
                {
                    // Ensure the HTML is up-to-date with the structured content.
                    bag.PushOpenMessage = new StructuredContentHelper( communicationTemplate.PushOpenMessageJson ).Render();
                }
                else
                {
                    bag.PushOpenMessage = communicationTemplate.PushOpenMessage;
                }
            }
            else if ( communicationTemplate.PushOpenAction == Rock.Utility.PushOpenAction.LinkToMobilePage )
            {
                if ( pushData.MobilePageId.HasValue )
                {
                    bag.PushMobilePage = PageCache.GetGuid( pushData.MobilePageId.Value )?.ToString()?.ToPageRouteValueBag();
                }

                bag.PushMobilePageQueryString = pushData.MobilePageQueryString;
            }
            else if ( communicationTemplate.PushOpenAction == Rock.Utility.PushOpenAction.LinkToUrl )
            {
                bag.PushUrl = pushData.Url;
            }

            return bag;
        }
 
        private List<CommunicationFlowDetailCommunicationTemplateBag> GetSelectableCommunicationTemplates( RockContext rockContext, Person currentPerson )
        {
            var templateQuery = new CommunicationTemplateService( rockContext )
                .Queryable()
                .AsNoTracking()
                .IncludeCommunicationTemplateBagEntities()
                .Where( a =>
                    a.IsActive
                    && a.Version != CommunicationTemplateVersion.Legacy
                    && a.UsageType == CommunicationTemplateUsageType.CommunicationFlows
                );

            // Get templates that the currently logged in person is authorized to view.
            var communicationTemplateListItemBags = templateQuery
                .ToList()
                .Where( a => a.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .Select( GetCommunicationTemplateAsBag )
                .ToList();

            return communicationTemplateListItemBags;
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( CommunicationFlow entity, Person currentPerson )
        {
            return new SecurityGrant()
                .AddRulesForAttributes( entity, currentPerson )
                .AddRule( new AssetAndFileManagerSecurityGrantRule( Authorization.VIEW ) )
                .AddRule( new AssetAndFileManagerSecurityGrantRule( Authorization.EDIT ) )
                .AddRule( new AssetAndFileManagerSecurityGrantRule( Authorization.DELETE ) )
                .AddRule( new EmailEditorSecurityGrantRule() )
                .ToToken();
        }

        /// <summary>
        /// Validates the CommunicationFlow for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="communicationFlow">The CommunicationFlow to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the CommunicationFlow is valid, <c>false</c> otherwise.</returns>
        private bool ValidateCommunicationFlow( CommunicationFlow communicationFlow, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        private CommunicationFlowDetailConversionGoalSettingsBag GetConversionGoalSettingsBag( CommunicationFlow entity )
        {
            var settings = entity.GetConversionGoalSettings();

            if ( settings == null )
            {
                return null;
            }

            var bag = new CommunicationFlowDetailConversionGoalSettingsBag();

            if ( settings.CompletedFormSettings != null )
            {
                bag.CompletedFormSettings = new CommunicationFlowDetailCompletedFormSettingsBag
                {
                    WorkflowType = WorkflowTypeCache.Get( settings.CompletedFormSettings.WorkflowTypeGuid ).ToListItemBag(),
                };
            }

            if ( settings.JoinedGroupTypeSettings != null )
            {
                bag.JoinedGroupTypeSettings = new CommunicationFlowDetailJoinedGroupTypeSettingsBag
                {
                    GroupType = GroupTypeCache.Get( settings.JoinedGroupTypeSettings.GroupTypeGuid ).ToListItemBag()
                };
            }

            if ( settings.JoinedGroupSettings != null )
            {
                bag.JoinedGroupSettings = new CommunicationFlowDetailJoinedGroupSettingsBag
                {
                    Group = GroupCache.Get( settings.JoinedGroupSettings.GroupGuid ).ToListItemBag()
                };
            }

            if ( settings.RegisteredSettings != null )
            {
                var registrationInstance = new RegistrationInstanceService( this.RockContext )
                    .Queryable()
                    .Where( ri => ri.Guid == settings.RegisteredSettings.RegistrationInstanceGuid )
                    .Select( ri => new ListItemBag
                    {
                        Text = ri.Name,
                        Value = ri.Guid.ToString()
                    } )
                    .FirstOrDefault();

                bag.RegisteredSettings = new CommunicationFlowDetailRegisteredSettingsBag
                {
                    RegistrationInstance = registrationInstance
                };
            }

            if ( settings.TookStepSettings != null )
            {
                bag.TookStepSettings = new CommunicationFlowDetailTookStepSettingsBag
                {
                    StepType = StepTypeCache.Get( settings.TookStepSettings.StepTypeGuid ).ToListItemBag()
                };
            }

            if ( settings.EnteredDataViewSettings != null )
            {
                bag.EnteredDataViewSettings = new CommunicationFlowDetailEnteredDataViewSettingsBag
                {
                    DataView = DataViewCache.Get( settings.EnteredDataViewSettings.DataViewGuid ).ToListItemBag()
                };
            }

            return bag;
        }

        private CommunicationFlowBag GetEntityBag( CommunicationFlow entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new CommunicationFlowBag
            {
                IdKey = entity.IdKey,
                Category = entity.Category.ToListItemBag(),
                Communications = entity.CommunicationFlowCommunications?.Select( c => GetEntityBag( c ) )?.ToList(),
                ConversionGoalSettings = GetConversionGoalSettingsBag( entity ),
                ConversionGoalTargetPercent = entity.ConversionGoalTargetPercent,
                ConversionGoalTimeframeInDays = entity.ConversionGoalTimeframeInDays,
                ConversionGoalType = entity.ConversionGoalType,
                Description = entity.Description,
                ExitConditionType = entity.ExitConditionType,
                iCalendarContent = entity.Schedule?.iCalendarContent,
                IsActive = entity.IsActive,
                Name = entity.Name,
                TargetAudienceDataView = entity.TargetAudienceDataView.ToListItemBag(),
                TriggerType = entity.TriggerType,
                UnsubscribeMessage = entity.UnsubscribeMessage
            };

            if ( entity.Attributes == null )
            {
                entity.LoadAttributes( RockContext );
            }

            bag.LoadAttributesAndValuesForPublicEdit( entity, GetCurrentPerson(), enforceSecurity: true );

            return bag;
        }

        private CommunicationFlowCommunicationBag GetEntityBag( CommunicationFlowCommunication entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new CommunicationFlowCommunicationBag
            {
                Guid = entity.Guid,
                CommunicationTemplate = GetCommunicationTemplateAsBag( entity.CommunicationTemplate ),
                DaysToWait = entity.DaysToWait,
                CommunicationType = ( Enums.Communication.CommunicationType ) ( int ) entity.CommunicationType,
                Name = entity.Name,
                Order = entity.Order,
                TimeToSend = entity.TimeToSend
            };

            return bag;
        }

        private CommunicationFlow.ConversionGoalSettings GetConversionGoalSettings( CommunicationFlowDetailConversionGoalSettingsBag bag )
        {
            if ( bag == null )
            {
                return null;
            }

            var settings = new CommunicationFlow.ConversionGoalSettings();

            if ( bag.CompletedFormSettings != null )
            {
                // Ensure a real Workflow Type GUID is used.
                var workflowType = WorkflowTypeCache.Get( bag.CompletedFormSettings.WorkflowType?.Value );

                if ( workflowType != null )
                {
                    settings.CompletedFormSettings = new CommunicationFlow.CompletedFormConversionGoalSettings
                    {
                        WorkflowTypeGuid = workflowType.Guid
                    };
                }
            }

            if ( bag.JoinedGroupTypeSettings != null )
            {
                // Ensure a real Group Type GUID is used.
                var groupType = GroupTypeCache.Get( bag.JoinedGroupTypeSettings.GroupType?.Value );

                if ( groupType != null )
                {
                    settings.JoinedGroupTypeSettings = new CommunicationFlow.JoinedGroupTypeConversionGoalSettings
                    {
                        GroupTypeGuid = groupType.Guid
                    };
                }
            }

            if ( bag.JoinedGroupSettings != null )
            {
                // Ensure a real Group GUID is used.
                var group = GroupCache.Get( bag.JoinedGroupSettings.Group?.Value );

                if ( group != null )
                {
                    settings.JoinedGroupSettings = new CommunicationFlow.JoinedGroupConversionGoalSettings
                    {
                        GroupGuid = group.Guid
                    };
                }
            }

            if ( bag.RegisteredSettings != null )
            {
                // Ensure a real Registration Instance GUID is used.
                var registrationInstance = new RegistrationInstanceService( this.RockContext )
                    .GetQueryableByKey( bag.RegisteredSettings.RegistrationInstance.Value )
                    .Select( ri => new
                    {
                        ri.Guid
                    } )
                    .FirstOrDefault();

                if ( registrationInstance != null )
                {
                    settings.RegisteredSettings = new CommunicationFlow.RegisteredConversionGoalSettings
                    {
                        RegistrationInstanceGuid = registrationInstance.Guid
                    };
                }
            }

            // Other settings can be converted here.

            return settings;
        }

        private decimal? WithinBoundary( decimal? source, decimal minInclusive, decimal maxInclusive )
        {
            if ( !source.HasValue )
            {
                return null;
            }

            if ( source.Value < minInclusive )
            {
                return minInclusive;
            }

            if (source.Value > maxInclusive)
            {
                return maxInclusive;
            }

            return source;
        }

        /// <inheritdoc/>
        private bool UpdateEntityFromBag( RockContext rockContext, CommunicationFlow entity, CommunicationFlowBag bag )
        {
            var communicationTemplateService = new CommunicationTemplateService( rockContext );
            var communicationFlowCommunicationService = new CommunicationFlowCommunicationService( rockContext );

            entity.SetConversionGoalSettings( GetConversionGoalSettings( bag.ConversionGoalSettings ) );
            entity.CategoryId = bag.Category.GetEntityId<Category>( RockContext );
            entity.ConversionGoalTargetPercent = WithinBoundary( bag.ConversionGoalTargetPercent, 0m, 100m );
            entity.ConversionGoalTimeframeInDays = bag.ConversionGoalTimeframeInDays;
            entity.ConversionGoalType = bag.ConversionGoalType;
            entity.Description = bag.Description;
            entity.ExitConditionType = bag.ExitConditionType;
            entity.IsActive = bag.IsActive;
            entity.Name = bag.Name;
            entity.TargetAudienceDataViewId = bag.TargetAudienceDataView.GetEntityId<DataView>( RockContext );
            entity.TriggerType = bag.TriggerType;

            if ( entity.Schedule != null )
            {
                entity.Schedule.iCalendarContent = bag.iCalendarContent;
            }
            else
            {
                entity.Schedule = new Schedule
                {
                    iCalendarContent = bag.iCalendarContent
                };
                new ScheduleService( this.RockContext ).Add( entity.Schedule );
            }

            entity.UnsubscribeMessage = bag.UnsubscribeMessage;

            // Attributes
            entity.LoadAttributes( RockContext );
            entity.SetPublicAttributeValues( bag.AttributeValues, GetCurrentPerson(), enforceSecurity: true );

            // Communications
            var updatedEntityToBagMappings = new Dictionary<CommunicationFlowCommunication, CommunicationFlowCommunicationBag>();

            // Remove old.
            foreach ( var communicationFlowCommunication in entity.CommunicationFlowCommunications.ToList())
            {
                var communicationFlowCommunicationBag = bag.Communications
                    .FirstOrDefault( c => c.Guid == communicationFlowCommunication.Guid );

                if ( communicationFlowCommunicationBag == null )
                {
                    entity.CommunicationFlowCommunications.Remove( communicationFlowCommunication );

                    // Fully delete this flow's communication from the DB.
                    communicationFlowCommunicationService.Delete( communicationFlowCommunication );
                }
                else
                {
                    updatedEntityToBagMappings.Add( communicationFlowCommunication, communicationFlowCommunicationBag );
                }
            }

            // Update existing.
            foreach ( var communicationFlowCommunication in entity.CommunicationFlowCommunications )
            {
                var communicationFlowCommunicationBag = updatedEntityToBagMappings[communicationFlowCommunication];
                
                // TODO JMH Add validation code before this that ensures real templates are used.
                if ( communicationFlowCommunication.CommunicationTemplate.Guid != communicationFlowCommunicationBag.CommunicationTemplate.Guid )
                {
                    var communicationTemplate = communicationTemplateService.GetWithCommunicationTemplateBagIncludes( communicationFlowCommunicationBag.CommunicationTemplate.Guid );
                    if ( communicationTemplate != null )
                    {
                        communicationFlowCommunication.CommunicationTemplateId = communicationTemplate.Id;
                        communicationFlowCommunication.CommunicationTemplate = communicationTemplate;
                    }
                }

                communicationFlowCommunication.CommunicationType = ( Rock.Model.CommunicationType ) ( int ) communicationFlowCommunicationBag.CommunicationType;
                communicationFlowCommunication.DaysToWait = communicationFlowCommunicationBag.DaysToWait;
                communicationFlowCommunication.Name = communicationFlowCommunicationBag.Name;
                communicationFlowCommunication.Order = communicationFlowCommunicationBag.Order;
                communicationFlowCommunication.TimeToSend = communicationFlowCommunicationBag.TimeToSend;
            }

            // Add new.
            var newCommunicationFlowCommunicationBags = bag.Communications
                .Where( communicationFlowCommunicationBag =>
                    !entity.CommunicationFlowCommunications.Any( communicationFlowCommunication => communicationFlowCommunication.Guid == communicationFlowCommunicationBag.Guid )
                )
                .ToList();

            foreach ( var communicationFlowCommunicationBag in newCommunicationFlowCommunicationBags )
            {
                var communicationTemplate = communicationTemplateService.Get( communicationFlowCommunicationBag.CommunicationTemplate.Guid );

                entity.CommunicationFlowCommunications.Add( new CommunicationFlowCommunication
                {
                    Guid = communicationFlowCommunicationBag.Guid.IsEmpty() ? Guid.NewGuid() : communicationFlowCommunicationBag.Guid,
                    CommunicationTemplateId = communicationTemplate.Id,
                    CommunicationTemplate = communicationTemplate,
                    CommunicationType = ( Rock.Model.CommunicationType ) ( int ) communicationFlowCommunicationBag.CommunicationType,
                    DaysToWait = communicationFlowCommunicationBag.DaysToWait,
                    Name = communicationFlowCommunicationBag.Name,
                    Order = communicationFlowCommunicationBag.Order,
                    TimeToSend = communicationFlowCommunicationBag.TimeToSend
                } );
            }

            return true;
        }

        /// <inheritdoc/>
        private CommunicationFlow GetInitialEntity( RockContext rockContext )
        {
            var entityKey = PageParameter( PageParameterKey.CommunicationFlow );
            var arePredictableIdsEnabled = !this.PageCache.Layout.Site.DisablePredictableIds;
            var id = arePredictableIdsEnabled ? entityKey.AsIntegerOrNull() : null;
            var guid = entityKey.AsGuidOrNull();

            // If a zero identifier is specified then create a new entity.
            if ( ( id == 0 ) || ( guid == Guid.Empty ) || ( !id.HasValue && !guid.HasValue && entityKey.IsNullOrWhiteSpace() ) )
            {
                return new CommunicationFlow
                {
                    Id = 0,
                    Guid = Guid.Empty,
                    // Inactive until the individual explicitly enables it
                    // so that the flow isn't started before it's fully created.
                    IsActive = false
                };
            }

            return new CommunicationFlowService( rockContext )
                .GetQueryableByKey( entityKey, arePredictableIdsEnabled )
                .IncludeCommunicationFlowBagEntities()
                .AsNoTracking()
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <inheritdoc/>
        private bool TryGetEntityForEditAction( RockContext rockContext, Person currentPerson, string idKey, out CommunicationFlow entity, out BlockActionResult error )
        {
            var entityService = new CommunicationFlowService( rockContext );
            error = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.GetQueryableByKey( idKey, !this.PageCache.Layout.Site.DisablePredictableIds )
                    .Include( e => e.Schedule )
                    .FirstOrDefault();
            }
            else
            {
                // Create a new entity.
                entity = new CommunicationFlow();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{CommunicationFlow.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${CommunicationFlow.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageReference.Parameters );
            var entityKey = pageReference.GetPageParameter( PageParameterKey.CommunicationFlow );
            var arePredictableIdsEnabled = !this.PageCache.Layout.Site.DisablePredictableIds;

            // If a zero identifier is specified then create a new entity.
            if ( entityKey.IsNullOrWhiteSpace() )
            {
                return new BreadCrumbResult
                {
                    BreadCrumbs = new List<IBreadCrumb>
                    {
                        new BreadCrumbLink( "New Communication Flow", breadCrumbPageRef )
                    }
                };
            }
            else
            {
                var title = new CommunicationFlowService( this.RockContext )
                    .GetSelect( entityKey, f => f.Name, arePredictableIdsEnabled );

                return new BreadCrumbResult
                {
                    BreadCrumbs = new List<IBreadCrumb>
                    {
                        new BreadCrumbLink( title ?? "New Communication Flow", breadCrumbPageRef )
                    }
                };
            }
        }

        #endregion
    }
}
