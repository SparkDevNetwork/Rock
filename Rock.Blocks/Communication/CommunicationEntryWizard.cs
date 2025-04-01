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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using CommunicationEntryWizardCommunicationType = Rock.Enums.Blocks.Communication.CommunicationEntryWizard.CommunicationType;
using CommunicationType = Rock.Model.CommunicationType;
using CommunicationEntryWizardPushOpenAction = Rock.Enums.Blocks.Communication.CommunicationEntryWizard.PushOpenAction;
using PushOpenAction = Rock.Utility.PushOpenAction;
using Rock.Model;
using Rock.Observability;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks.Communication.CommunicationEntryWizard;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using System.Threading.Tasks;
using Rock.Tasks;
using Rock.RealTime;
using Rock.RealTime.Topics;
using System.ComponentModel.DataAnnotations;
using Rock.Communication.Transport;
using Rock.ViewModels.Rest.Controls;
using Rock.Security.SecurityGrantRules;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SqlClient;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// A block for creating and sending a new communication, such as email, SMS, etc. to recipients.
    /// </summary>
    [DisplayName( "Communication Entry Wizard" )]
    [Category( "Communication" )]
    [Description( "Used for creating and sending a new communication, such as email, SMS, etc. to recipients." )]

    #region Block Attributes

    [SecurityAction( Authorization.APPROVE, "The roles and/or users that have access to approve new communications." )]

    [BinaryFileTypeField( "Image Binary File Type",
        Key = AttributeKey.ImageBinaryFileType,
        Description = "The FileType to use for images that are added to the email using the image component",
        IsRequired = true,
        DefaultBinaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.COMMUNICATION_IMAGE,
        Order = 1 )]

    [BinaryFileTypeField( "Attachment Binary File Type",
        Key = AttributeKey.AttachmentBinaryFileType,
        Description = "The FileType to use for files that are attached to an SMS or email communication",
        IsRequired = true,
        DefaultBinaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.COMMUNICATION_ATTACHMENT,
        Order = 2 )]

    [LavaCommandsField( "Enabled Lava Commands",
        Key = AttributeKey.EnabledLavaCommands,
        Description = "The Lava commands that should be enabled for this HTML block.",
        IsRequired = false,
        Order = 3 )]

    [CustomCheckboxListField( "Communication Types",
        Key = AttributeKey.CommunicationTypes,
        Description = "The communication types that should be available to use for the communication. (If none are selected, all will be available.) Selecting 'Recipient Preference' will automatically enable Email and SMS as mediums. Push is not an option for selection as a communication preference as delivery is not as reliable as other mediums based on an individual's privacy settings.",
        ListSource = "Recipient Preference,Email,SMS,Push",
        IsRequired = false,
        Order = 4 )]

    [IntegerField( "Maximum Recipients",
        Key = AttributeKey.MaximumRecipients,
        Description = "The maximum number of recipients allowed before communication will need to be approved.",
        IsRequired = false,
        DefaultIntegerValue = 300,
        Order = 5 )]

    [BooleanField( "Send When Approved",
        Key = AttributeKey.SendWhenApproved,
        Description = "Should communication be sent once it's approved (vs. just being queued for scheduled job to send)?",
        DefaultBooleanValue = true,
        Order = 6 )]

    [IntegerField( "Max SMS Image Width",
        Key = AttributeKey.MaxSMSImageWidth,
        Description = "The maximum width (in pixels) of an image attached to a mobile communication. If its width is over the max, Rock will automatically resize image to the max width.",
        IsRequired = false,
        DefaultIntegerValue = 600,
        Order = 7 )]

    [SystemPhoneNumberField( "Allowed SMS Numbers",
        Key = AttributeKey.AllowedSMSNumbers,
        Description = "Set the allowed FROM numbers to appear when in SMS mode (if none are selected all numbers will be included). ",
        IsRequired = false,
        AllowMultiple = true,
        Order = 8 )]

    [LinkedPage( "Simple Communication Page",
        Key = AttributeKey.SimpleCommunicationPage,
        Description = "The page to use if the 'Use Simple Editor' panel heading icon is clicked. Leave this blank to not show the 'Use Simple Editor' heading icon",
        IsRequired = false,
        Order = 9 )]

    [BooleanField( "Show Duplicate Prevention Option",
        Key = AttributeKey.ShowDuplicatePreventionOption,
        Description = "Set this to true to show an option to prevent communications from being sent to people with the same email/SMS addresses. Typically, in Rock you’d want to send two emails as each will be personalized to the individual.",
        DefaultBooleanValue = false,
        Order = 10 )]

    [BooleanField( "Default As Bulk",
        Key = AttributeKey.DefaultAsBulk,
        Description = "Should new entries be flagged as bulk communication by default?",
        DefaultBooleanValue = false,
        Order = 11 )]

    [BooleanField( "Enable Person Parameter",
        Key = AttributeKey.EnablePersonParameter,
        Description = "When enabled, allows passing a 'Person' or 'PersonId' querystring parameter with a person Id to the block to create a communication for that person.",
        DefaultBooleanValue = true,
        IsRequired = false,
        Order = 12 )]

    [BooleanField( "Disable Adding Individuals to Recipient Lists",
        Key = AttributeKey.DisableAddingIndividualsToRecipientLists,
        Description = "When set to 'Yes' the person picker will be hidden so that additional individuals cannot be added to the recipient list.",
        DefaultBooleanValue = false,
        IsRequired = false,
        Order = 13 )]

    [CategoryField( "Personalization Segment Category",
        Key = AttributeKey.PersonalizationSegmentCategory,
        Description = "Choose a category of Personalization Segments to be displayed.",
        EntityType = typeof( PersonalizationSegment ),
        DefaultValue = SystemGuid.Category.PERSONALIZATION_SEGMENT_COMMUNICATIONS,
        IsRequired = true,
        Order = 14 )]

    [IntegerField(
        "Minimum Short Link Token Length",
        Key = AttributeKey.MinimumShortLinkTokenLength,
        Description = "The minimum number of characters for short link tokens.",
        IsRequired = false,
        DefaultIntegerValue = 7,
        Order = 15 )]

   [BooleanField( "Disable Navigation Shortcuts",
       Key = AttributeKey.DisableNavigationShortcuts,
       Description = "When enabled, the block will turn off the keyboard shortcuts (arrow keys) used to navigate the steps.",
       DefaultBooleanValue = false,
       IsRequired = false,
       Category = BlockAttributeCategory.Advanced,
       Order = 100 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "26917C58-C8A2-4BF5-98CB-378A02761CD7" )]
    [Rock.SystemGuid.BlockTypeGuid( "9FFC7A4F-2061-4F30-AF79-D68C85EE9F27" )]
    public partial class CommunicationEntryWizard : RockBlockType
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string ImageBinaryFileType = "ImageBinaryFileType";
            public const string AttachmentBinaryFileType = "AttachmentBinaryFileType";
            public const string EnabledLavaCommands = "EnabledLavaCommands";
            public const string CommunicationTypes = "CommunicationTypes";
            public const string MaximumRecipients = "MaximumRecipients";
            public const string SendWhenApproved = "SendWhenApproved";
            public const string MaxSMSImageWidth = "MaxSMSImageWidth";
            public const string AllowedSMSNumbers = "AllowedSMSNumbers";
            public const string SimpleCommunicationPage = "SimpleCommunicationPage";
            public const string ShowDuplicatePreventionOption = "ShowDuplicatePreventionOption";
            public const string DefaultAsBulk = "DefaultAsBulk";
            public const string EnablePersonParameter = "EnablePersonParameter";
            public const string DisableAddingIndividualsToRecipientLists = "DisableAddingIndividualsToRecipientLists";
            public const string PersonalizationSegmentCategory = "PersonalizationSegmentCategory";
            public const string MinimumShortLinkTokenLength = "MinimumShortLinkTokenLength";
            public const string DisableNavigationShortcuts = "DisableNavigationShortcuts";
        }

        /// <summary>
        /// Categories to use for Block Attributes
        /// </summary>
        private static class BlockAttributeCategory
        {
            public const string Advanced = "Advanced";

        }

        #endregion Attribute Keys

        #region Navigation URL Keys

        private static class NavigationUrlKey
        {
            public const string SimpleCommunicationPage = "SimpleCommunicationPage";
        }

        #endregion

        #region Page Parameter Keys

        private static class PageParameterKey
        {
            // "Communication" allows Communication Id, Guid, or IdKey values,
            // while the older "CommunicationId" only supports Id.
            public const string Communication = "Communication";
            public const string CommunicationId = "CommunicationId";

            // "Person" allows Person Id, Guid, or IdKey values,
            // while the older "PersonId" only supports Id.
            public const string Person = "Person";
            public const string PersonId = "PersonId";

            // "CommunicationTemplate" allows CommunicationTemplate Id, Guid, or IdKey values,
            // while the older "TemplateGuid" only supports Guid.
            public const string CommunicationTemplate = "CommunicationTemplate";
            public const string TemplateGuid = "TemplateGuid";

            public const string Edit = "Edit";
        }

        #endregion Page Parameter Keys

        #region Person Preference Keys

        private static class PersonPreferenceKey
        {
            public const string CommunicationTemplateGuid = "CommunicationTemplateGuid";
        }

        #endregion Person Preference Keys

        #region Properties

        private List<Guid> AllowedSmsNumbersAttributeValue => GetAttributeValue( AttributeKey.AllowedSMSNumbers ).SplitDelimitedValues( true ).AsGuidList();

        private Guid AttachmentBinaryFileTypeGuid => GetAttributeValue( AttributeKey.AttachmentBinaryFileType ).AsGuidOrNull() ?? SystemGuid.BinaryFiletype.DEFAULT.AsGuid();

        private bool DisableAddingIndividualsToRecipientLists => GetAttributeValue( AttributeKey.DisableAddingIndividualsToRecipientLists ).AsBoolean();

        private string EnabledLavaCommandsAttributeValue => GetAttributeValue( AttributeKey.EnabledLavaCommands );

        private Guid ImageBinaryFileTypeGuid => GetAttributeValue( AttributeKey.ImageBinaryFileType ).AsGuidOrNull() ?? SystemGuid.BinaryFiletype.DEFAULT.AsGuid();

        private int MaxSmsImageWidth => GetAttributeValue( AttributeKey.MaxSMSImageWidth ).AsIntegerOrNull() ?? 600;

        private bool ShowDuplicatePreventionOption => GetAttributeValue( AttributeKey.ShowDuplicatePreventionOption ).AsBoolean();

        private bool AreNavigationShortcutsDisabled => GetAttributeValue( AttributeKey.DisableNavigationShortcuts ).AsBoolean();

        private Guid PersonalizationSegmentCategoryGuid => GetAttributeValue( AttributeKey.PersonalizationSegmentCategory ).AsGuid();

        private string SimpleCommunicationPageUrl => this.GetLinkedPageUrl( AttributeKey.SimpleCommunicationPage );

        private int MinimumShortLinkTokenLength => this.GetAttributeValue( AttributeKey.MinimumShortLinkTokenLength ).AsInteger();

        /// <summary>
        /// Gets the Communication entity key passed to the "Communication" or "CommunicationId" page parameter.
        /// </summary>
        private string CommunicationOrCommunicationIdPageParameter
        {
            get
            {
                var communicationPageParameter = PageParameter( PageParameterKey.Communication );

                if ( communicationPageParameter.IsNotNullOrWhiteSpace() )
                {
                    return communicationPageParameter;
                }
                else
                {
                    // Only allow the CommunicationId to contain an ID, but return it as a string so it can be used as an entity key.
                    return PageParameter( PageParameterKey.CommunicationId ).AsIntegerOrNull()?.ToString();
                }
            }
        }

        /// <summary>
        /// Gets the CommunicationTemplate entity key passed to the "CommunicationTemplate" or "TemplateGuid" page parameter.
        /// </summary>
        private string CommunicationTemplateOrTemplateGuidPageParameter
        {
            get
            {
                var communicationTemplatePageParameter = PageParameter( PageParameterKey.CommunicationTemplate );

                if ( communicationTemplatePageParameter.IsNotNullOrWhiteSpace() )
                {
                    return communicationTemplatePageParameter;
                }
                else
                {
                    // Only allow the TemplateGuid to contain a Guid, but return it as a string so it can be used as an entity key.
                    return PageParameter( PageParameterKey.TemplateGuid ).AsGuidOrNull()?.ToString();
                }
            }
        }

        /// <summary>
        /// Gets the Person entity key passed to the "Person" or "PersonId" page parameter.
        /// </summary>
        private string PersonOrPersonIdPageParameter
        {
            get
            {
                var personPageParameter = PageParameter( PageParameterKey.Person );

                if ( personPageParameter.IsNotNullOrWhiteSpace() )
                {
                    return personPageParameter;
                }
                else
                {
                    // Only allow the PersonId to contain an ID, but return it as a string so it can be used as an entity key.
                    return PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull()?.ToString();
                }
            }
        }

        #endregion

        public override object GetObsidianBlockInitialization()
        {
            var currentPerson = GetCurrentPerson();
            var communication = LoadCommunicationFromPageParameter( this.RockContext );

            var box = new CommunicationEntryWizardInitializationBox
            {
                IsHidden = IsCommunicationHidden( communication, currentPerson )
            };

            if ( box.IsHidden )
            {
                // Return early if the block should be hidden.
                return box;
            }
            else
            {
                box.AreNavigationShortcutsDisabled = this.AreNavigationShortcutsDisabled;
                box.AttachmentBinaryFileTypeGuid = this.AttachmentBinaryFileTypeGuid;
                box.BulkEmailThreshold = GetBulkEmailThreshold();
                box.CommunicationListGroups = GetCommunicationListGroupBags( this.RockContext, currentPerson );

                var communicationTemplateInfoList = GetCommunicationTemplateInfoList( this.RockContext );
                var communicationTemplateDetailBag = GetCommunicationTemplateDetailBag( communication, communicationTemplateInfoList, currentPerson );
                var communicationBag = GetCommunicationBag( this.RockContext, communication, communicationTemplateDetailBag?.Guid, currentPerson );
                var mediumBags = GetCommunicationMediumBags( currentPerson );

                box.Communication = communicationBag;
                box.CommunicationTemplateDetail = communicationTemplateDetailBag;
                box.CommunicationTopicValues = DefinedTypeCache.Get( SystemGuid.DefinedType.COMMUNICATION_TOPIC ).DefinedValues.ToListItemBagList();
                box.HasDetailBlockOnCurrentPage = this.PageCache.Blocks.Any( a => a.BlockType.Guid == SystemGuid.BlockType.COMMUNICATION_DETAIL.AsGuid() );
                box.ImageComponentBinaryFileTypeGuid = this.ImageBinaryFileTypeGuid;
                box.IsAddingIndividualsToRecipientListsDisabled = this.DisableAddingIndividualsToRecipientLists;
                box.IsDuplicatePreventionOptionShown = this.ShowDuplicatePreventionOption;
                box.IsUsingRockMobilePushTransport = GetIsUsingRockMobilePushTransport( mediumBags );
                box.MaxSmsImageWidth = this.MaxSmsImageWidth;
                box.Mediums = mediumBags;
                box.MergeFields = GetCommunicationMergeFields( communication );
                box.MinimumShortLinkTokenLength = this.MinimumShortLinkTokenLength;
                box.NavigationUrls = GetBoxNavigationUrls();
                box.PersonalizationSegments = GetPersonalizationSegments( this.RockContext );
                box.PushApplications = GetPushApplications( this.RockContext, mediumBags );
                box.Recipients = GetRecipientBags( this.RockContext, communicationBag );
                box.SecurityGrantToken = GetSecurityGrantToken();
                box.ShortLinkSites = GetShortLinkEnabledSites();
                box.SmsFromNumbers = GetSmsFromNumberBags( currentPerson );
                // The Twilio transport was used by the old block to validate SMS attachments.
                box.SmsAcceptedMimeTypes = Twilio.AcceptedMimeTypes.ToList();
                box.SmsMediaSizeLimitBytes = Twilio.MediaSizeLimitBytes;
                box.SmsSupportedMimeTypes = Twilio.SupportedMimeTypes.ToList();
                box.Templates = ConvertToTemplateBags( communicationTemplateInfoList );
                box.VideoProviderNames = Rock.Communication.VideoEmbed.VideoEmbedContainer.Instance.Dictionary.Select( c => c.Value.Key ).ToList();

                return box;
            }
        }

        #region Block Actions

        /// <summary>
        /// Gets a communication template.
        /// </summary>
        /// <param name="communicationTemplateGuid">The communication template unique identifier.</param>
        [BlockAction]
        public BlockActionResult GetCommunicationTemplate( Guid communicationTemplateGuid )
        {
            if ( !communicationTemplateGuid.Validate( "Communication Template Unique Identifier" ).IsNotEmpty( out var validationResult ) )
            {
                return ActionBadRequest( validationResult.ErrorMessage );
            }

            var communicationTemplateInfo = GetCommunicationTemplateInfoList(
                this.RockContext,
                ( query ) =>
                {
                    return query.Where( g => g.Guid == communicationTemplateGuid )
                        .Take( 1 );
                } )
                .FirstOrDefault();

            if ( communicationTemplateInfo == null )
            {
                return ActionNotFound();
            }
            else
            {
                var communicationTemplateDetailBag = GetCommunicationTemplateDetailBag( communicationTemplateInfo );

                return ActionOk( communicationTemplateDetailBag );
            }
        }

        /// <summary>
        /// Sends a test communication.
        /// </summary>
        /// <param name="bag">The bag containing the communication information.</param>
        [BlockAction( "SendTest" )]
        public BlockActionResult SendTest( CommunicationEntryWizardCommunicationBag bag )
        {
            if ( !IsValid( bag, out var validationResult )
                || ( bag.CommunicationType == CommunicationEntryWizardCommunicationType.Email && !bag.TestEmailAddress.Validate( "Test Email" ).IsNotNullOrWhiteSpace( out validationResult ) )
                || ( bag.CommunicationType == CommunicationEntryWizardCommunicationType.SMS && !bag.TestSmsPhoneNumber.Validate( "Test SMS Number" ).IsNotNullOrWhiteSpace( out validationResult ) ) )
            {
                return ActionBadRequest( validationResult.ErrorMessage );
            }

            SendTestCommunication( bag, out var errorMessage );

            if ( errorMessage.IsNotNullOrWhiteSpace() )
            {
                return ActionBadRequest( errorMessage );
            }
            else
            {
                return ActionOk();
            }
        }

        /// <summary>
        /// Saves the communication.
        /// </summary>
        [BlockAction( "Save" )]
        public BlockActionResult Save( CommunicationEntryWizardCommunicationBag bag )
        {
            if ( !IsValid( bag, out var validationResult ) )
            {
                return ActionBadRequest( validationResult.ErrorMessage );
            }

            var currentPerson = GetCurrentPerson();
            var communication = SaveAsDraft( this.RockContext, bag );
            bag = GetCommunicationBag( this.RockContext, communication, communication.CommunicationTemplate?.Guid, currentPerson );

            return ActionOk( new CommunicationEntryWizardSaveResponseBag
            {
                Message = "The communication has been saved.",
                Communication = bag
            } );
        }

        /// <summary>
        /// Saves a metrics reminder communication.
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        [BlockAction( "SaveMetricsReminder" )]
        public BlockActionResult SaveMetricsReminder( CommunicationEntryWizardSaveMetricsReminderRequestBag bag )
        {
            if ( !IsValid( bag, out var validationResult ) )
            {
                return ActionBadRequest( validationResult.ErrorMessage );
            }

            var communicationService = new CommunicationService( this.RockContext );
            var communication = communicationService.Get( bag.CommunicationGuid );

            if ( communication == null )
            {
                return ActionNotFound();
            }

            if ( !communication.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) )
            {
                return ActionForbidden();
            }

            communication.EmailMetricsReminderOffsetDays = bag.DaysUntilReminder;

            this.RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Cancels a metrics reminder communication.
        /// </summary>
        /// <param name="communicationGuid"></param>
        /// <returns></returns>
        [BlockAction( "CancelMetricsReminder" )]
        public BlockActionResult CancelMetricsReminder( Guid communicationGuid )
        {
            if ( !communicationGuid.Validate( "Communication" ).IsNotEmpty( out var validationResult ) )
            {
                return ActionBadRequest( validationResult.ErrorMessage );
            }

            var communicationService = new CommunicationService( this.RockContext );
            var communication = communicationService.Get( communicationGuid );

            if ( communication == null )
            {
                return ActionNotFound();
            }

            if ( !communication.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) )
            {
                return ActionForbidden();
            }

            communication.EmailMetricsReminderOffsetDays = null;

            this.RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Gets the recipient list count for a communication.
        /// </summary>
        [BlockAction( "GetRecipients" )]
        public BlockActionResult GetRecipients( CommunicationEntryWizardCommunicationBag bag )
        {
            if ( !bag.Validate( "Communication" ).IsNotNull( out var validationResult ) )
            {
                return ActionBadRequest( validationResult.ErrorMessage );
            }

            var recipientBags = GetRecipientBags( this.RockContext, bag );

            return ActionOk( recipientBags );
        }

        /// <summary>
        /// Gets recipient data for a person.
        /// </summary>
        /// <param name="personAliasGuid">The person alias unique identifier.</param>
        [BlockAction( "GetRecipient" )]
        public BlockActionResult GetRecipient( Guid personAliasGuid )
        {
            if ( !personAliasGuid.Validate( "Person" ).IsNotEmpty( out var validationResult ) )
            {
                return ActionBadRequest( validationResult.ErrorMessage );
            }

            var recipient = GetRecipientBagsFromPersonAliases( this.RockContext, new List<Guid> { personAliasGuid } ).FirstOrDefault();

            if ( recipient != null )
            {
                return ActionOk( recipient );
            }
            else
            {
                return ActionNotFound();
            }
        }

        /// <summary>
        /// Gets common segment data views or the segment data views for the provided communication list group unique identifier.
        /// </summary>
        /// <param name="communicationListGroupGuid">The communication list group unique identifier.</param>
        [BlockAction( "GetSegmentDataViews" )]
        public BlockActionResult GetSegmentDataViews( Guid? communicationListGroupGuid )
        {
            var currentPerson = GetCurrentPerson();
            var segmentDataViews = GetCommunicationSegmentDataViewBags( this.RockContext, communicationListGroupGuid, currentPerson );

            return ActionOk( segmentDataViews );
        }

        /// <summary>
        /// Subscribes to the real-time progress updates.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="communicationGuid">The Communication identifier.</param>
        [BlockAction( "SubscribeToRealTime" )]
        public async Task<BlockActionResult> SubscribeToRealTime( string connectionId, Guid communicationGuid )
        {
            if ( !connectionId.Validate( "Connection Identifier" ).IsNotNullOrWhiteSpace( out var validationResult )
                || !communicationGuid.Validate( "Communication Unique Identifier" ).IsNotEmpty( out validationResult ) )
            {
                return ActionBadRequest( validationResult.ErrorMessage );
            }

            var communication = new CommunicationService( this.RockContext ).Get( communicationGuid );

            // Authorize the current user.
            if ( communication != null && !communication.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) )
            {
                return ActionStatusCode( System.Net.HttpStatusCode.Forbidden );
            }

            var topicChannels = RealTimeHelper.GetTopicContext<ITaskActivityProgress>().Channels;

            await topicChannels.AddToChannelAsync( connectionId, GetCommunicationSendChannel( communicationGuid ) );

            return ActionOk();
        }

        /// <summary>
        /// Sends the communication.
        /// </summary>
        /// <param name="bag">The communication details.</param>
        [BlockAction( "Send" )]
        public BlockActionResult Send( CommunicationEntryWizardCommunicationBag bag )
        {
            if ( !IsValid( bag, out var validationResult ) )
            {
                return ActionBadRequest( validationResult.ErrorMessage );
            }

            ProcessCommunicationSend( bag );

            var responseBag = new CommunicationEntryWizardSendResponseBag
            {
                HasDetailBlockOnCurrentPage = this.PageCache.Blocks.Any( a => a.BlockType.Guid == Rock.SystemGuid.BlockType.COMMUNICATION_DETAIL.AsGuid() ),
                //Message = "The communication has been saved",
            };

            return ActionOk( responseBag );
        }

        /// <summary>
        /// Generates and returns an HTML preview of the email content for a given communication.
        /// </summary>
        /// <param name="bag">The communication details used to generate the email preview.</param>
        /// <returns>A <see cref="BlockActionResult"/> containing the resolved HTML preview of the email communication.</returns>
        [BlockAction( "GetEmailPreviewHtml" )]
        public BlockActionResult GetEmailPreviewHtml( CommunicationEntryWizardCommunicationBag bag )
        {
            if ( !IsValid( bag, out var validationResult ) )
            {
                return ActionBadRequest( validationResult.ErrorMessage );
            }

            var communication = SaveAsDraft( this.RockContext, bag );
            var currentPerson = GetCurrentPerson();
            var sampleCommunicationRecipient = GetSampleCommunicationRecipient( communication, currentPerson );

            var communicationCreatorOrLoggedInPerson = communication.CreatedByPersonAlias?.Person ?? currentPerson;

            var commonMergeFields = this.RequestContext.GetCommonMergeFields( communicationCreatorOrLoggedInPerson );
            var mergeFields = sampleCommunicationRecipient.CommunicationMergeValues( commonMergeFields );

            var previewHtml = GenerateEmailHtmlPreview( communication, communicationCreatorOrLoggedInPerson, mergeFields );

            // Create response.
            bag = GetCommunicationBag( this.RockContext, communication, communication.CommunicationTemplate?.Guid, currentPerson );

            return ActionOk( new CommunicationEntryWizardGetEmailPreviewHtmlBag
            {
                Communication = bag,
                PreviewHtml = previewHtml
            } );
        }

        /// <summary>
        /// Saves an existing communication template with some overwritten fields.
        /// </summary>
        [BlockAction( "SaveCommunicationTemplate" )]
        public BlockActionResult SaveCommunicationTemplate( CommunicationEntryWizardCommunicationTemplateDetailBag bag )
        {
            // Validate request.
            if ( !bag.Validate( "Communication Template" ).IsNotNull( out var validationResult )
                || !bag.Message.Validate( "Message" ).IsNotNullOrWhiteSpace( out validationResult )
                || !bag.ImageFile.Validate( "Preview Image" ).ValueIsNotNullOrEmptyGuid( out validationResult ) )
            {
                return ActionBadRequest( validationResult.ErrorMessage );
            }

            // Validate entities.
            var currentPerson = GetCurrentPerson();
            var communicationTemplateService = new CommunicationTemplateService( this.RockContext );

            var existingCommunicationTemplate = communicationTemplateService.Get( bag.Guid );

            if ( existingCommunicationTemplate == null )
            {
                return ActionBadRequest( "Existing communication template was not found." );
            }
            else if ( existingCommunicationTemplate.IsSystem || !existingCommunicationTemplate.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                return ActionBadRequest( "You don't have edit access to the existing communication template." );
            }

            var imageFile = new BinaryFileService( this.RockContext ).Get( bag.ImageFile.Value.AsGuid() );

            if ( imageFile == null )
            {
                return ActionBadRequest( "Preview image is required." );
            }
            else if ( !imageFile.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                return ActionBadRequest( "You don't have edit access to this preview image." );
            }

            // Now overwrite with specific values from the request.
            existingCommunicationTemplate.ImageFile = imageFile;
            existingCommunicationTemplate.ImageFileId = imageFile.Id;
            existingCommunicationTemplate.Message = bag.Message;

            if ( !existingCommunicationTemplate.IsValid )
            {
                var errorMessage = "The communication template is invalid.";

                if ( existingCommunicationTemplate.ValidationResults?.Any() == true )
                {
                    errorMessage = string.Join( " ", existingCommunicationTemplate.ValidationResults.Select( vr => vr.ErrorMessage ) );
                }

                return ActionBadRequest( errorMessage );
            }

            // Make sure the image is no longer a temporary binary file.
            imageFile.IsTemporary = false;

            // Save the communication template.
            this.RockContext.SaveChanges();

            // Create the response.
            // The client will receive both the detail and list item information.
            var communicationTemplateInfo = GetCommunicationTemplateInfoList(
                this.RockContext,
                ( query ) =>
                {
                    return query.Where( g => g.Guid == existingCommunicationTemplate.Guid )
                        .Take( 1 );
                } )
                .FirstOrDefault();

            var communicationTemplateListItemBag = ConvertToTemplateBag( communicationTemplateInfo );
            var communicationTemplateDetailBag = GetCommunicationTemplateDetailBag( communicationTemplateInfo );

            return ActionOk( new CommunicationEntryWizardSaveCommunicationTemplateResponseBag
            {
                CommunicationTemplateDetail = communicationTemplateDetailBag,
                CommunicationTemplateListItem = communicationTemplateListItemBag
            } );
        }

        /// <summary>
        /// Saves an existing communication template as a new entity with some overwritten fields.
        /// </summary>
        [BlockAction( "SaveAsCommunicationTemplate" )]
        public BlockActionResult SaveAsCommunicationTemplate( CommunicationEntryWizardCommunicationTemplateDetailBag bag )
        {
            // Validate request.
            if ( !bag.Validate( "Communication Template" ).IsNotNull( out var validationResult )
                || !bag.Message.Validate( "Message" ).IsNotNullOrWhiteSpace( out validationResult )
                || !bag.Category.Validate( "Category" ).ValueIsNotNullOrEmptyGuid( out validationResult )
                || !bag.ImageFile.Validate( "Preview Image" ).ValueIsNotNullOrEmptyGuid( out validationResult ) )
            {
                return ActionBadRequest( validationResult.ErrorMessage );
            }

            // Validate entities.
            var currentPerson = GetCurrentPerson();
            var communicationTemplateService = new CommunicationTemplateService( this.RockContext );

            var existingCommunicationTemplate = communicationTemplateService.Get( bag.Guid );

            if ( existingCommunicationTemplate == null )
            {
                return ActionBadRequest( "Existing communication template was not found." );
            }
            else if ( !existingCommunicationTemplate.IsAuthorized( Authorization.VIEW, currentPerson ) )
            {
                return ActionBadRequest( "You don't have view access to the existing communication template." );
            }

            var category = new CategoryService( this.RockContext ).Get( bag.Category.Value.AsGuid() );

            if ( category == null )
            {
                return ActionBadRequest( "Category is required." );
            }
            else if ( !category.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                return ActionBadRequest( "You don't have edit access to this category." );
            }

            var imageFile = new BinaryFileService( this.RockContext ).Get( bag.ImageFile.Value.AsGuid() );

            if ( imageFile == null )
            {
                return ActionBadRequest( "Preview image is required." );
            }
            else if ( !imageFile.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                return ActionBadRequest( "You don't have edit access to this preview image." );
            }

            // Create the new communication.
            var communicationTemplate = existingCommunicationTemplate.CloneWithoutIdentity();

            // Now overwrite with specific values from the request.
            communicationTemplate.IsSystem = false;
            communicationTemplate.Name = bag.Name;
            communicationTemplate.Description = bag.Description;
            communicationTemplate.Message = bag.Message;
            communicationTemplate.ImageFile = imageFile;
            communicationTemplate.ImageFileId = imageFile.Id;
            communicationTemplate.Category = category;
            communicationTemplate.CategoryId = category.Id;
            communicationTemplate.IsStarter = bag.IsStarter;

            if ( !communicationTemplate.IsValid )
            {
                var errorMessage = "The communication template is invalid.";

                if ( communicationTemplate.ValidationResults?.Any() == true )
                {
                    errorMessage = string.Join( " ", communicationTemplate.ValidationResults.Select( vr => vr.ErrorMessage ) );
                }

                return ActionBadRequest( errorMessage );
            }

            // Make sure the image is no longer a temporary binary file.
            imageFile.IsTemporary = false;

            // Save the communication template.
            communicationTemplateService.Add( communicationTemplate );
            this.RockContext.SaveChanges();

            // Create the response.
            // The client will receive both the detail and list item information.
            var communicationTemplateInfo = GetCommunicationTemplateInfoList(
                this.RockContext,
                ( query ) =>
                {
                    return query.Where( g => g.Guid == communicationTemplate.Guid )
                        .Take( 1 );
                } )
                .FirstOrDefault();

            var communicationTemplateListItemBag = ConvertToTemplateBag( communicationTemplateInfo );
            var communicationTemplateDetailBag = GetCommunicationTemplateDetailBag( communicationTemplateInfo );

            return ActionOk( new CommunicationEntryWizardSaveCommunicationTemplateResponseBag
            {
                CommunicationTemplateDetail = communicationTemplateDetailBag,
                CommunicationTemplateListItem = communicationTemplateListItemBag
            } );
        }

        [BlockAction( "CheckShortLinkToken" )]
        public BlockActionResult CheckShortLinkToken( CommunicationEntryWizardCheckShortLinkTokenBag bag )
        {
            var pageShortLinkService = new PageShortLinkService( this.RockContext );
            var pageShortLink = pageShortLinkService.GetByToken( bag.Token, bag.SiteId );

            if ( pageShortLink == null )
            {
                return ActionOk( bag.Token );
            }
            else
            {
                return ActionOk( pageShortLinkService.GetUniqueToken( bag.SiteId, 7 ) );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the personalization segments for the segments dropdown.
        /// </summary>
        private List<ListItemBag> GetPersonalizationSegments( RockContext rockContext )
        {
            var personalizationSegmentCategory = this.PersonalizationSegmentCategoryGuid;

            return new PersonalizationSegmentService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( p => p.Categories.Any( c => c.Guid == personalizationSegmentCategory ) )
                        .Select( p => new ListItemBag
                        {
                            Value = p.Id.ToString(),
                            Text = p.Name
                        } )
                        .ToList();
        }

        /// <summary>
        /// Gets the communication recipient details for a communication list.
        /// </summary>
        /// <returns></returns>
        private List<CommunicationEntryWizardRecipientInfo> GetCommunicationRecipientDetailsForList( RockContext rockContext, int communicationListGroupId, SegmentCriteria segmentCriteria, List<int> personalizationSegmentIds )
        {
            return GetCommunicationRecipientDetails( rockContext, communicationListGroupId, 1, segmentCriteria == SegmentCriteria.All ? 2 : 1, personalizationSegmentIds == null ? null : string.Join( ",", personalizationSegmentIds ) );
        }

        /// <summary>
        /// Gets the giving analytics transaction data.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="accountIds">The account ids.</param>
        /// <param name="currencyTypeIds">The currency type ids.</param>
        /// <param name="sourceTypeIds">The source type ids.</param>
        /// <param name="transactionTypeIds">The transaction type ids.</param>
        /// <returns></returns>
        private List<CommunicationEntryWizardRecipientInfo> GetCommunicationRecipientDetails( RockContext rockContext, int listId, int listType, int matchType, string personalizationSegmentIds )
        {
            personalizationSegmentIds = personalizationSegmentIds ?? string.Empty;

            return rockContext.Database
                .SqlQuery<CommunicationEntryWizardRecipientInfo>(
                    $"EXEC [dbo].[spCommunicationRecipientDetails] @{nameof( listId )}, @{nameof( listType )}, @{nameof( matchType )}, @{nameof( personalizationSegmentIds )}",
                    new SqlParameter( nameof( listId ), listId ),
                    new SqlParameter( nameof( listType ), listType ),
                    new SqlParameter( nameof( matchType ), matchType ),
                    new SqlParameter( nameof( personalizationSegmentIds ), personalizationSegmentIds )
                    {
                        Size = Math.Max( 1, personalizationSegmentIds.Length )
                    }
                ).ToList();
        }

        /// <summary>
        /// Gets the number of recipients that need to be exceeded to automatically consider the email a bulk email.
        /// </summary>
        private int? GetBulkEmailThreshold()
        {
            var medium = MediumContainer.GetActiveMediumComponentsWithActiveTransports()
                .FirstOrDefault( m => m.TypeGuid == SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() );

            if ( medium is Rock.Communication.Medium.Email emailMedium )
            {
                return emailMedium.GetBulkEmailThreshold();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            var navigationUrls = new Dictionary<string, string>();
            var simpleCommunicationPageUrl = this.SimpleCommunicationPageUrl;

            if ( simpleCommunicationPageUrl.IsNotNullOrWhiteSpace() )
            {
                navigationUrls.Add( NavigationUrlKey.SimpleCommunicationPage, simpleCommunicationPageUrl );
            }

            return navigationUrls;
        }

        /// <inheritdoc/>
        protected override string RenewSecurityGrantToken()
        {
            return GetSecurityGrantToken();
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken()
        {
            var securityGrant = new SecurityGrant();

            securityGrant.AddRule( new AssetAndFileManagerSecurityGrantRule( Authorization.VIEW ) );
            securityGrant.AddRule( new AssetAndFileManagerSecurityGrantRule( Authorization.EDIT ) );
            securityGrant.AddRule( new AssetAndFileManagerSecurityGrantRule( Authorization.DELETE ) );

            return securityGrant.ToToken();
        }

        /// <summary>
        /// Gets the sites enabled for shortening.
        /// </summary>
        private List<ListItemBag> GetShortLinkEnabledSites()
        {
            return SiteCache.All()
                .Where( s => s.EnabledForShortening && s.SiteType == SiteType.Web )
                .Select( s => new ListItemBag
                {
                    // Integer IDs should be passed here since they are used in Lava filters that require ints.
                    Value = s.Id.ToString(),
                    Text = s.Name
                } )
                .ToList();
        }

        /// <summary>
        /// Determines if an already retrieved <paramref name="entity"/> has the supplied <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="entity">The entity.</param>
        /// <param name="key">The key (Guid, Id, or ID Key).</param>
        /// <returns><see langword="true"/> if the key matches, otherwise <see langword="false"/>.</returns>
        private bool EntityHasKey<T>( T entity, string key ) where T : Model<T>, new()
        {
            var id = !this.PageCache.Layout.Site.DisablePredictableIds ? key.AsIntegerOrNull() : null;

            if ( !id.HasValue )
            {
                var guid = key.AsGuidOrNull();

                if ( guid.HasValue )
                {
                    return entity.Guid == guid.Value;
                }

                id = IdHasher.Instance.GetId( key );
            }

            return id.HasValue && entity.Id == id.Value;
        }

        /// <summary>
        /// Retrieves the communication template details for the given communication,
        /// optionally applying a selected template based on parameters.
        /// </summary>
        /// <param name="rockContext">The database context used for querying data.</param>
        /// <param name="communication">The communication entity, which may be null for a new communication.</param>
        /// <param name="communicationTemplateInfoList">A list of available communication template information.</param>
        /// <param name="currentPerson">The currently logged-in person, used for authorization checks.</param>
        /// <returns>
        /// A <see cref="CommunicationEntryWizardCommunicationTemplateDetailBag"/> containing the communication template details,
        /// or <see langword="null"/> if no valid template is found.
        /// </returns>
        private CommunicationEntryWizardCommunicationTemplateDetailBag GetCommunicationTemplateDetailBag( Model.Communication communication, List<CommunicationEntryWizardTemplateInfo> communicationTemplateInfoList, Person currentPerson )
        {
            CommunicationEntryWizardTemplateInfo communicationTemplateInfo = null;

            // If a communication template key was passed in and this is a new communication, set that as the selected template.
            var communicationTemplateKey = this.CommunicationTemplateOrTemplateGuidPageParameter;

            if ( ( communication == null || communication.Id == 0 ) && communicationTemplateKey.IsNotNullOrWhiteSpace() )
            {
                communicationTemplateInfo = communicationTemplateInfoList.FirstOrDefault( d => EntityHasKey( d.CommunicationTemplate, communicationTemplateKey ) );
            }
            else if ( communication?.CommunicationTemplateId.HasValue == true )
            {
                communicationTemplateInfo = communicationTemplateInfoList.FirstOrDefault( d => d.CommunicationTemplate.Id == communication.CommunicationTemplateId.Value );
            }
            else
            {
                var communicationTemplateGuidPersonPreference = GetBlockPersonPreferences().GetValue( PersonPreferenceKey.CommunicationTemplateGuid ).AsGuidOrNull();

                if ( communicationTemplateGuidPersonPreference.HasValue )
                {
                    communicationTemplateInfo = communicationTemplateInfoList.FirstOrDefault( d => d.CommunicationTemplate.Guid == communicationTemplateGuidPersonPreference );
                }
            }
            
            // NOTE: Only set the selected template if the user has auth for this template
            // and the template supports the Email Wizard
            if ( communicationTemplateInfo?.CommunicationTemplate != null
                && communicationTemplateInfo.CommunicationTemplate.IsAuthorized( Authorization.VIEW, currentPerson )
                && communicationTemplateInfo.CommunicationTemplate.SupportsEmailWizard() )
            {
                return GetCommunicationTemplateDetailBag( communicationTemplateInfo );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Constructs a <see cref="CommunicationEntryWizardCommunicationTemplateDetailBag"/> from a given 
        /// communication template information instance.
        /// </summary>
        /// <param name="rockContext">The database context used for querying data.</param>
        /// <param name="communicationTemplateInfo">The communication template information containing details about the template.</param>
        /// <returns>
        /// A <see cref="CommunicationEntryWizardCommunicationTemplateDetailBag"/> containing the details of 
        /// the specified communication template.
        /// </returns>
        private CommunicationEntryWizardCommunicationTemplateDetailBag GetCommunicationTemplateDetailBag( CommunicationEntryWizardTemplateInfo communicationTemplateInfo )
        {
            var mergeFields = this.RequestContext.GetCommonMergeFields();

            return new CommunicationEntryWizardCommunicationTemplateDetailBag
            {
                Guid = communicationTemplateInfo.CommunicationTemplate.Guid,
                IsWizardSupported = communicationTemplateInfo.CommunicationTemplate.SupportsEmailWizard(),
                Name = communicationTemplateInfo.CommunicationTemplate.Name,
                ImageFile = communicationTemplateInfo.ImageFile,
                Category = communicationTemplateInfo.Category,
                IsStarter = communicationTemplateInfo.CommunicationTemplate.IsStarter,
                Description = communicationTemplateInfo.CommunicationTemplate.Description,
                IsSystem = communicationTemplateInfo.CommunicationTemplate.IsSystem,

                // Email fields
                FromEmail = communicationTemplateInfo.CommunicationTemplate.FromEmail?.ResolveMergeFields( mergeFields ),
                FromName = communicationTemplateInfo.CommunicationTemplate.FromName?.ResolveMergeFields( mergeFields ),
                ReplyToEmail = communicationTemplateInfo.CommunicationTemplate.ReplyToEmail?.ResolveMergeFields( mergeFields ),
                CcEmails = communicationTemplateInfo.CommunicationTemplate.CCEmails,
                BccEmails = communicationTemplateInfo.CommunicationTemplate.BCCEmails,
                Subject = communicationTemplateInfo.CommunicationTemplate.Subject,
                Message = communicationTemplateInfo.CommunicationTemplate.Message?.ResolveMergeFields( mergeFields ),
                EmailAttachmentBinaryFiles = communicationTemplateInfo.CommunicationTemplate.GetAttachments( CommunicationType.Email ).ToListItemBagList(),

                // SMS fields
                SmsFromSystemPhoneNumberGuid = communicationTemplateInfo.SmsFromSystemPhoneNumberGuid,
                SmsMessage = communicationTemplateInfo.CommunicationTemplate.SMSMessage,
                SmsAttachmentBinaryFiles = communicationTemplateInfo.CommunicationTemplate.GetAttachments( CommunicationType.SMS ).ToListItemBagList(),

                // Push fields                    
                PushData = ConvertPushData( communicationTemplateInfo.CommunicationTemplate.PushData.FromJsonOrNull<PushData>() ),
                PushImageBinaryFileGuid = communicationTemplateInfo.PushImageBinaryFileGuid,
                PushMessage = communicationTemplateInfo.CommunicationTemplate.PushMessage,
                PushTitle = communicationTemplateInfo.CommunicationTemplate.PushTitle,
                PushOpenMessage = communicationTemplateInfo.CommunicationTemplate.PushOpenMessage,
                PushOpenMessageJson = communicationTemplateInfo.CommunicationTemplate.PushOpenMessageJson,
                PushOpenAction = communicationTemplateInfo.CommunicationTemplate.PushOpenAction.HasValue
                    ? ConvertPushOpenAction( communicationTemplateInfo.CommunicationTemplate.PushOpenAction.Value )
                    : default,
            };
        }

        /// <summary>
        /// Retrieves a list of push-enabled mobile applications as <see cref="ListItemBag"/> objects.
        /// </summary>
        /// <param name="rockContext">The database context used for querying data.</param>
        /// <param name="mediums">A list of available communication mediums to check if push notifications are enabled.</param>
        /// <returns>
        /// A list of <see cref="ListItemBag"/> objects representing active mobile applications 
        /// that support push notifications, or <see langword="null"/> if push notifications are not enabled.
        /// </returns>
        private List<ListItemBag> GetPushApplications( RockContext rockContext, List<ListItemBag> mediums )
        {
            var pushMediumItem = mediums?.FirstOrDefault( m => m.Value.AsGuidOrNull() == SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() );

            if ( pushMediumItem == null )
            {
                // Push is not enabled so return null.
                return null;
            }
            else
            {
                var activePushMediumWithActiveTransport = MediumContainer.GetActiveMediumComponentsWithActiveTransports()
                    .FirstOrDefault( m => m.TypeGuid == SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() );

                if ( activePushMediumWithActiveTransport == null )
                {
                    // There is no active push medium with an active transport.
                    return null;
                }
                else
                {
                    return new SiteService( rockContext )
                        .Queryable()
                        .Where( s => s.SiteType == SiteType.Mobile )
                        .Select(
                            s => new ListItemBag
                            {
                                Text = s.Name,
                                Value = s.Guid.ToString(),
                            } )
                        .ToList();
                }
            }
        }

        /// <summary>
        /// Determines whether the Rock Mobile Push transport is being used for any of the <paramref name="mediums"/>.
        /// </summary>
        /// <param name="mediums">A list of available communication mediums to check.</param>
        /// <returns><see langword="true"/> if the active push medium is using Rock Mobile Push transport; otherwise, <see langword="false"/>.</returns>
        private bool GetIsUsingRockMobilePushTransport( List<ListItemBag> mediums )
        {
            var pushMediumItem = mediums?.FirstOrDefault( m => m.Value.AsGuidOrNull() == SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() );

            if ( pushMediumItem == null )
            {
                // Push is not enabled so return false.
                return false;
            }
            else
            {
                var activePushMediumWithActiveTransport = MediumContainer.GetActiveMediumComponentsWithActiveTransports()
                    .FirstOrDefault( m => m.TypeGuid == SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() );

                if ( activePushMediumWithActiveTransport == null )
                {
                    // There is no active push medium with an active transport.
                    return false;
                }
                else
                {
                    return activePushMediumWithActiveTransport.Transport is RockMobilePush;
                }
            }
        }

        /// <summary>
        /// Loads an existing communication based on the page parameter.
        /// </summary>
        /// <param name="rockContext">The database context used for querying data.</param>
        /// <returns>A <see cref="Model.Communication"/> instance if found; otherwise, <see langword="null"/>.</returns>
        private Model.Communication LoadCommunicationFromPageParameter( RockContext rockContext )
        {
            // Check page parameter for existing communication.
            var communicationKey = this.CommunicationOrCommunicationIdPageParameter;

            if ( communicationKey.IsNotNullOrWhiteSpace() )
            {
                return new CommunicationService( rockContext ).Get( communicationKey, !this.PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Creates and populates a <see cref="CommunicationEntryWizardCommunicationBag"/> from a given communication, 
        /// or initializes a new one with default values if none is provided.
        /// </summary>
        /// <param name="rockContext">The database context used for querying data.</param>
        /// <param name="communication">The communication entity, or <see langword="null"/> if creating a new communication.</param>
        /// <param name="communicationTemplateGuid">The communication template unique identifier, if applicable.</param>
        /// <param name="currentPerson">The currently logged-in person, used for authorization and default values.</param>
        /// <param name="defaultCommunicationListGroupGuid">An optional unique identifier for the default communication list group.</param>
        /// <returns>A <see cref="CommunicationEntryWizardCommunicationBag"/> with the populated communication details.</returns>
        private CommunicationEntryWizardCommunicationBag GetCommunicationBag(
            RockContext rockContext,
            Model.Communication communication,
            Guid? communicationTemplateGuid,
            Person currentPerson,
            Guid? defaultCommunicationListGroupGuid = null )
        {
            // Define local functions at the top for readability.
            string GetEnabledLavaCommands( Model.Communication c )
                => c.Status == CommunicationStatus.Transient
                    ? this.EnabledLavaCommandsAttributeValue
                    : c.EnabledLavaCommands;

            List<Guid> GetIndividualRecipientPersonAliasGuids( Model.Communication c )
            {
                if ( c.ListGroupId.HasValue )
                {
                    return null;
                }
                else
                {
                    var individualRecipientPersonAliasGuids = new CommunicationRecipientService( rockContext )
                        .Queryable()
                        .AsNoTracking()
                        .Where( r => r.CommunicationId == c.Id )
                        .Select( a => a.PersonAlias.Guid )
                        .ToList();

                    if ( GetAttributeValue( AttributeKey.EnablePersonParameter ).AsBoolean() )
                    {
                        // If either 'Person' or 'PersonId' is specified add that person to the communication.
                        var personKey = this.PersonOrPersonIdPageParameter;

                        if ( personKey.IsNotNullOrWhiteSpace() )
                        {
                            var person = new PersonService( rockContext )
                                .GetQueryableByKey( personKey, !this.PageCache.Layout.Site.DisablePredictableIds )
                                .Select(
                                    p => new
                                    {
                                        // Get the primary person alias guid.
                                        Guid = p.PrimaryAliasGuid
                                    } )
                                .FirstOrDefault();

                            if ( person?.Guid.HasValue == true )
                            {
                                if ( !individualRecipientPersonAliasGuids.Contains( person.Guid.Value ) )
                                {
                                    individualRecipientPersonAliasGuids.Add( person.Guid.Value );
                                }
                            }
                        }
                    }

                    return individualRecipientPersonAliasGuids?.Any() == true ? individualRecipientPersonAliasGuids : null;
                }
            }

            bool GetIsBulkCommunication( Model.Communication c )
            {
                if ( c.Status == CommunicationStatus.Transient )
                {
                    var bulkEmailThreshold = GetBulkEmailThreshold();
                    var isBulkCommunicationForced = bulkEmailThreshold.HasValue && ( communication?.Recipients?.Count ?? 0 ) > bulkEmailThreshold.Value;

                    return isBulkCommunicationForced || GetAttributeValue( AttributeKey.DefaultAsBulk ).AsBoolean();
                }
                else
                {
                    return c.IsBulkCommunication;
                }
            }

            CommunicationEntryWizardPushNotificationOptionsBag GetPushData( Model.Communication c )
                => ConvertPushData( c.PushData.FromJsonOrNull<PushData>() );

            Guid? GetPushImageBinaryFileGuid( Model.Communication c )
                => c.PushImageBinaryFileId.HasValue
                    ? new BinaryFileService( rockContext ).GetGuid( c.PushImageBinaryFileId.Value )
                    : null;

            CommunicationEntryWizardPushOpenAction GetPushOpenAction( Model.Communication c )
                => c.PushOpenAction.HasValue
                    ? ConvertPushOpenAction( c.PushOpenAction.Value )
                    : CommunicationEntryWizardPushOpenAction.NoAction;

            Guid? GetSmsFromSystemPhoneNumberGuid( Model.Communication c )
                => c.SmsFromSystemPhoneNumberId.HasValue
                    ? SystemPhoneNumberCache.GetGuid( c.SmsFromSystemPhoneNumberId.Value )
                    : null;

            // Main method logic starts here...

            if ( communication == null )
            {
                // This is a new communication. Create a temporary communication model with defaults so it can be used for auth checks.
                communication = new Model.Communication
                {
                    // Id = 0 identifies this as a new communication.
                    Id = 0,

                    Status = CommunicationStatus.Transient,
                    CreatedByPersonAlias = currentPerson.PrimaryAlias,
                    CreatedByPersonAliasId = currentPerson.PrimaryAliasId,
                    SenderPersonAlias = currentPerson.PrimaryAlias,
                    SenderPersonAliasId = currentPerson.PrimaryAliasId,
                    CommunicationType = CommunicationType.Email
                };
            }

            var bag = new CommunicationEntryWizardCommunicationBag
            {
                BccEmails = communication.BCCEmails,
                CcEmails = communication.CCEmails,
                CommunicationId = communication.Id,
                CommunicationGuid = communication.Guid,
                CommunicationListGroupGuid = communication.ListGroupId.HasValue ? communication.ListGroup?.Guid : defaultCommunicationListGroupGuid,
                CommunicationName = communication.Name,
                CommunicationTemplateGuid = communicationTemplateGuid,
                CommunicationTopicValue = communication.CommunicationTopicValue?.ToListItemBag(),
                CommunicationType = ConvertCommunicationType( communication.CommunicationType ),
                EmailAttachmentBinaryFiles = communication.GetAttachments( CommunicationType.Email ).ToListItemBagList(),
                EnabledLavaCommands = GetEnabledLavaCommands( communication ),
                ExcludeDuplicateRecipientAddress = communication.ExcludeDuplicateRecipientAddress,
                FromEmail = communication.FromEmail,
                FromName = communication.FromName,
                FutureSendDateTime = communication.FutureSendDateTime,
                IndividualRecipientPersonAliasGuids = GetIndividualRecipientPersonAliasGuids( communication ),
                IsBulkCommunication = GetIsBulkCommunication( communication ),
                Message = communication.Message,
                PushData = GetPushData( communication ),
                PushImageBinaryFileGuid = GetPushImageBinaryFileGuid( communication ),
                PushMessage = communication.PushMessage,
                PushOpenAction = GetPushOpenAction( communication ),
                PushOpenMessage = communication.PushOpenMessage,
                PushOpenMessageJson = communication.PushOpenMessageJson,
                PushTitle = communication.PushTitle,
                ReplyToEmail = communication.ReplyToEmail,
                SegmentCriteria = communication.SegmentCriteria,
                PersonalizationSegmentIds = communication.PersonalizationSegments.SplitDelimitedValues().AsIntegerList(),
                SmsAttachmentBinaryFiles = communication.GetAttachments( CommunicationType.SMS ).ToListItemBagList(),
                SmsFromSystemPhoneNumberGuid = GetSmsFromSystemPhoneNumberGuid( communication ),
                SmsMessage = communication.SMSMessage,
                Status = communication.Status,
                Subject = communication.Subject,
                TestEmailAddress = currentPerson.Email,
                TestSmsPhoneNumber = currentPerson.GetPhoneNumber( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() )?.Number
            };

            return bag;
        }

        /// <summary>
        /// Converts a <see cref="CommunicationEntryWizardPushNotificationOptionsBag"/> to a <see cref="PushData"/> object.
        /// </summary>
        /// <param name="pushOptionsBag">The push notification options bag containing data to convert.</param>
        /// <returns>A <see cref="PushData"/> object with the converted values, or <see langword="null"/> if the input is <see langword="null"/>.</returns>
        private PushData ConvertPushData( CommunicationEntryWizardPushNotificationOptionsBag pushOptionsBag )
        {
            return pushOptionsBag == null
                ? null
                : new PushData
                {
                    MobileApplicationId = pushOptionsBag.MobileApplicationGuid.HasValue ? SiteCache.GetId( pushOptionsBag.MobileApplicationGuid.Value ) : null,
                    MobilePageId = pushOptionsBag.MobilePage?.Page?.Value.IsNotNullOrWhiteSpace() == true ? PageCache.GetId( pushOptionsBag.MobilePage.Page.Value.AsGuid() ) : null,
                    MobilePageQueryString = pushOptionsBag.MobilePageQueryString,
                    Url = pushOptionsBag.LinkToPageUrl
                };
        }

        /// <summary>
        /// Converts a <see cref="PushData"/> object to a <see cref="CommunicationEntryWizardPushNotificationOptionsBag"/>.
        /// </summary>
        /// <param name="pushData">The push data object containing data to convert.</param>
        /// <returns>A <see cref="CommunicationEntryWizardPushNotificationOptionsBag"/> with the converted values, or <see langword="null"/> if the input is <see langword="null"/>.</returns>
        private CommunicationEntryWizardPushNotificationOptionsBag ConvertPushData( PushData pushData )
        {
            PageRouteValueBag mobilePage = null;
            if ( pushData?.MobilePageId.HasValue == true )
            {
                mobilePage = PageCache.GetGuid( pushData.MobilePageId.Value )?.ToString()?.ToPageRouteValueBag();
            }

            return pushData == null
                ? null
                : new CommunicationEntryWizardPushNotificationOptionsBag
                {
                    MobileApplicationGuid = pushData.MobileApplicationId.HasValue ? SiteCache.GetGuid( pushData.MobileApplicationId.Value ) : null,
                    MobilePage = mobilePage,
                    MobilePageQueryString = pushData.MobilePageQueryString,
                    LinkToPageUrl = pushData.Url
                };
        }

        /// <summary>
        /// Converts a <see cref="PushOpenAction"/> to a <see cref="CommunicationEntryWizardPushOpenAction"/>.
        /// </summary>
        /// <param name="pushOpenAction">The push open action to convert.</param>
        /// <returns>The equivalent <see cref="CommunicationEntryWizardPushOpenAction"/>.</returns>
        private CommunicationEntryWizardPushOpenAction ConvertPushOpenAction( PushOpenAction pushOpenAction )
        {
            return ( CommunicationEntryWizardPushOpenAction ) pushOpenAction;
        }
        
        /// <summary>
        /// Converts a <see cref="CommunicationEntryWizardPushOpenAction"/> to a <see cref="PushOpenAction"/>.
        /// </summary>
        /// <param name="pushOpenAction">The push open action to convert.</param>
        /// <returns>The equivalent <see cref="PushOpenAction"/>.</returns>
        private PushOpenAction ConvertPushOpenAction( CommunicationEntryWizardPushOpenAction pushOpenAction )
        {
            return ( PushOpenAction ) pushOpenAction;
        }
        
        /// <summary>
        /// Converts a <see cref="CommunicationType"/> to a <see cref="CommunicationEntryWizardCommunicationType"/>.
        /// </summary>
        /// <param name="communicationType">The communication type to convert.</param>
        /// <returns>The equivalent <see cref="CommunicationEntryWizardCommunicationType"/>.</returns>
        private CommunicationEntryWizardCommunicationType ConvertCommunicationType( CommunicationType communicationType )
        {
            return ( CommunicationEntryWizardCommunicationType ) communicationType;
        }
        
        /// <summary>
        /// Converts a <see cref="CommunicationEntryWizardCommunicationType"/> to a <see cref="CommunicationType"/>.
        /// </summary>
        /// <param name="communicationType">The communication type to convert.</param>
        /// <returns>The equivalent <see cref="CommunicationType"/>.</returns>
        private CommunicationType ConvertCommunicationType( CommunicationEntryWizardCommunicationType communicationType )
        {
            return ( CommunicationType ) communicationType;
        }

        /// <summary>
        /// Gets the entity type GUID for the specified communication type.
        /// If the communication type is not allowed, the first allowed type is used as a fallback.
        /// </summary>
        /// <param name="communicationType">The communication type for which to retrieve the corresponding medium entity type GUID.</param>
        /// <returns>The entity type GUID corresponding to the given communication type.</returns>
        private Guid GetMediumEntityTypeGuid( CommunicationType communicationType )
        {
            var allowedCommunicationTypes = GetAllowedCommunicationTypes();

            if ( !allowedCommunicationTypes.Contains( communicationType ) )
            {
                communicationType = allowedCommunicationTypes.First();
            }

            switch ( communicationType )
            {
                case CommunicationType.SMS:
                    return SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid();

                case CommunicationType.PushNotification:
                    return SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid();

                case CommunicationType.Email:
                default:
                    return SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid();
            }
        }
 
        /// <summary>
        /// Retrieves a list of <see cref="CommunicationEntryWizardTemplateInfo"/> objects representing active communication templates.
        /// Allows for optional filtering at both the query and post-query levels.
        /// </summary>
        /// <param name="rockContext">The database context used for querying data.</param>
        /// <param name="queryFilter">An optional function to apply additional filtering to the database query.</param>
        /// <param name="postQueryFilter">An optional function to apply additional filtering after retrieving the results.</param>
        /// <returns>A list of <see cref="CommunicationEntryWizardTemplateInfo"/> objects representing the available communication templates.</returns>
        private List<CommunicationEntryWizardTemplateInfo> GetCommunicationTemplateInfoList(
            RockContext rockContext,
            Func<IQueryable<CommunicationTemplate>, IQueryable<CommunicationTemplate>> queryFilter = null,
            Func<IEnumerable<CommunicationEntryWizardTemplateInfo>, IEnumerable<CommunicationEntryWizardTemplateInfo>> postQueryFilter = null )
        {
            var templateQuery = new CommunicationTemplateService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( a => a.IsActive );

            // Apply external query filters.
            if ( queryFilter != null )
            {
                templateQuery = queryFilter( templateQuery );
            }

            // Get the communication templates with 
            var communicationQuery = new CommunicationService( rockContext ).Queryable().AsNoTracking();
            var communicationTemplateInfoList = templateQuery
                .GroupJoin(
                    communicationQuery,
                    communicationTemplate => communicationTemplate.Id,
                    communication => communication.CommunicationTemplateId,
                    ( communicationTemplate, communications ) => new CommunicationEntryWizardTemplateInfo
                    {
                        CommunicationTemplate = communicationTemplate,
                        CommunicationCount = communications.Count(),
                        Category = !communicationTemplate.CategoryId.HasValue ? null : new ListItemBag { Value = communicationTemplate.Category.Guid.ToString(), Text = communicationTemplate.Category.Name },
                        ImageFile = !communicationTemplate.ImageFileId.HasValue ? null : new ListItemBag { Value = communicationTemplate.ImageFile.Guid.ToString(), Text = communicationTemplate.ImageFile.FileName },
                        PushImageBinaryFileGuid = communicationTemplate.PushImageBinaryFile.Guid,
                        SmsFromSystemPhoneNumberGuid = communicationTemplate.SmsFromSystemPhoneNumber.Guid
                    }
                ).ToList();

            // Get templates that the currently logged in person is authorized to view.
            communicationTemplateInfoList = communicationTemplateInfoList
                .Where( a => a.CommunicationTemplate.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) )
                .ToList();

            // Apply post query filters.
            if ( postQueryFilter != null )
            {
                communicationTemplateInfoList = postQueryFilter( communicationTemplateInfoList )?.ToList();
            }

            return communicationTemplateInfoList;
        }

        /// <summary>
        /// Converts a list of <see cref="CommunicationEntryWizardTemplateInfo"/> objects into a list of <see cref="CommunicationEntryWizardCommunicationTemplateListItemBag"/> objects.
        /// </summary>
        /// <param name="communicationTemplateInfoList">A collection of communication template information objects to convert.</param>
        /// <returns>A list of <see cref="CommunicationEntryWizardCommunicationTemplateListItemBag"/> objects representing the available communication templates.</returns>
        private List<CommunicationEntryWizardCommunicationTemplateListItemBag> ConvertToTemplateBags(
            IEnumerable<CommunicationEntryWizardTemplateInfo> communicationTemplateInfoList )
        {
            var bags = new List<CommunicationEntryWizardCommunicationTemplateListItemBag>();

            if ( communicationTemplateInfoList != null )
            {
                foreach ( var communicationTemplateInfo in communicationTemplateInfoList )
                {
                    bags.Add( ConvertToTemplateBag( communicationTemplateInfo ) );
                }
            }

            return bags;
        }
        
        /// <summary>
        /// Converts a <see cref="CommunicationEntryWizardTemplateInfo"/> into a <see cref="CommunicationEntryWizardCommunicationTemplateListItemBag"/>.
        /// </summary>
        /// <param name="communicationTemplateInfo">The communication template information to convert.</param>
        private CommunicationEntryWizardCommunicationTemplateListItemBag ConvertToTemplateBag(
            CommunicationEntryWizardTemplateInfo communicationTemplateInfo )
        {
            if ( communicationTemplateInfo == null )
            {
                return null;
            }

            return new CommunicationEntryWizardCommunicationTemplateListItemBag
            {
                CategoryGuid = communicationTemplateInfo.Category?.Value.AsGuidOrNull(),
                IsEmailSupported = communicationTemplateInfo.CommunicationTemplate.SupportsEmailWizard(),
                IsSmsSupported = communicationTemplateInfo.CommunicationTemplate.HasSMSTemplate()
                    || communicationTemplateInfo.CommunicationTemplate.Guid == SystemGuid.Communication.COMMUNICATION_TEMPLATE_BLANK.AsGuid(),
                Name = communicationTemplateInfo.CommunicationTemplate.Name,
                Description = communicationTemplateInfo.CommunicationTemplate.Description,
                ImageUrl = communicationTemplateInfo.CommunicationTemplate.ImageFileId.HasValue
                    ? FileUrlHelper.GetImageUrl( communicationTemplateInfo.CommunicationTemplate.ImageFileId )
                    : null,
                Guid = communicationTemplateInfo.CommunicationTemplate.Guid,
                IsStarter = communicationTemplateInfo.CommunicationTemplate.IsStarter,
                CommunicationCount = communicationTemplateInfo.CommunicationCount,
            };
        }

        /// <summary>
        /// Retrieves a list of recipient bags based on the communication details.
        /// </summary>
        /// <remarks>
        /// Determines the recipients from an existing communication, individual person aliases, or a communication list.
        /// </remarks>
        /// <param name="rockContext">The database context used for querying data.</param>
        /// <param name="bag">The communication entry wizard data bag.</param>
        /// <returns>A list of <see cref="CommunicationEntryWizardRecipientBag"/> objects representing the recipients.</returns>
        private List<CommunicationEntryWizardRecipientBag> GetRecipientBags(
            RockContext rockContext,
            CommunicationEntryWizardCommunicationBag bag )
        {
            // If the communication is new and persisted without communication list or segment modifications,
            // then retrieve the current recipients for the communication.
            if ( bag.IndividualRecipientPersonAliasGuids?.Any() == true )
            {
                return GetRecipientBagsFromPersonAliases( rockContext, bag.IndividualRecipientPersonAliasGuids );
            }
            // ... Or if individual recipients have been specified, then retrieve the individual recipients.
            else if ( bag.CommunicationListGroupGuid.HasValue )
            {
                var communicationListGroupId = bag.CommunicationListGroupGuid.HasValue ? GroupCache.GetId( bag.CommunicationListGroupGuid.Value ) : null;

                if ( communicationListGroupId.HasValue )
                {
                    var recipients = GetCommunicationRecipientDetailsForList(
                        rockContext,
                        communicationListGroupId.Value,
                        bag.SegmentCriteria,
                        bag.PersonalizationSegmentIds );

                    return ConvertRecipientInfoListToBagList( recipients );
                }
                else
                { 
                    return new List<CommunicationEntryWizardRecipientBag>();
                }
            }
            else
            { 
                return new List<CommunicationEntryWizardRecipientBag>();
            }
        }

        private static List<CommunicationEntryWizardRecipientBag> ConvertRecipientInfoListToBagList( List<CommunicationEntryWizardRecipientInfo> info )
        {
            return info.Select( i => new CommunicationEntryWizardRecipientBag
            {
                Email = i.Email,
                EmailPreference = i.EmailPreference.ToString(),
                IsBulkEmailAllowed = i.IsBulkEmailEnabled,
                IsEmailActive = i.IsEmailActive,
                IsEmailAllowed = i.IsEmailEnabled,
                IsNameless = Person.IsNameless( i.RecordTypeValueId ),
                IsPushAllowed = i.IsPushEnabled,
                IsSmsAllowed = i.IsSmsEnabled,
                Name = Person.FormatFullName( i.NickName, i.LastName, i.SuffixValueId, i.RecordTypeValueId ),
                PersonAliasGuid = i.PrimaryAliasGuid,
                PersonId = i.Id,
                PhotoUrl = Person.GetPersonPhotoUrl(
                                    // Initials
                                    $"{i.NickName.Truncate( 1, false )}{i.LastName.Truncate( 1, false )}",
                                    i.PhotoId,
                                    i.Age,
                                    i.Gender ?? Gender.Unknown,
                                    i.RecordTypeValueId,
                                    i.AgeClassification,
                                    // Size
                                    24 ),
                SmsNumber = i.MobilePhoneNumber,

            } ).ToList();
        }

        /// <summary>
        /// Retrieves a dictionary mapping person IDs to their primary mobile phone numbers, if messaging is enabled.
        /// </summary>
        /// <param name="rockContext">The database context used for querying data.</param>
        /// <param name="personIds">A list of person IDs to retrieve mobile numbers for.</param>
        /// <returns>A dictionary mapping person IDs to their primary mobile phone numbers.</returns>
        private Dictionary<int, string> FindMobilePhoneNumbers( RockContext rockContext, IQueryable<PersonAlias> personAliasQuery )
        {
            var mobilePhoneDefinedValueId = DefinedValueCache.GetId(SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid());
            var personIdQuery = personAliasQuery.Select( pa => pa.PersonId );

            return new PhoneNumberService(rockContext)
                .Queryable()
                .AsNoTracking()
                .Where( phone =>
                    phone.NumberTypeValueId == mobilePhoneDefinedValueId
                    && phone.IsMessagingEnabled
                    && personIdQuery.Contains( phone.PersonId )
                )
                .Select( phone => new
                {
                    phone.PersonId,
                    phone.Number
                } )
                .ToList()
                .GroupBy( phone => phone.PersonId )
                // Fetch first phone per person.
                .ToDictionary( g => g.Key, g => g.Select( a => a.Number ).FirstOrDefault() );
        }

        /// <summary>
        /// Retrieves a set of person alias IDs that have notification-enabled personal devices.
        /// </summary>
        /// <param name="rockContext">The database context used for querying data.</param>
        /// <param name="personAliasIds">A list of person alias IDs to check for notification-enabled devices.</param>
        /// <returns>A hash set containing person alias IDs with notification-enabled personal devices.</returns>
        private HashSet<int> FindPushEnabledDevices( RockContext rockContext, IQueryable<PersonAlias> personAliasQuery )
        {;
            var personAliasIdQuery = personAliasQuery.Select( pa => pa.Id );

            return new PersonalDeviceService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( device =>
                    device.PersonAliasId.HasValue
                    && device.NotificationsEnabled
                    && personAliasIdQuery.Contains( device.PersonAliasId.Value ) )
                .Select( device => device.PersonAliasId.Value )
                .ToHashSet();
        }

        /// <summary>
        /// Retrieves a list of recipients from a communication list group, applying optional segmentation filters.
        /// </summary>
        /// <param name="rockContext">The database context used for querying data.</param>
        /// <param name="communicationListGroupGuid">The GUID of the communication list group.</param>
        /// <param name="segmentCriterion">The segmentation criteria to apply.</param>
        /// <param name="segmentDataViewGuids">A list of segment data view GUIDs for filtering.</param>
        /// <returns>A list of <see cref="CommunicationEntryWizardRecipientBag"/> objects representing the recipients.</returns>
        private List<CommunicationEntryWizardRecipientBag> GetRecipientBagsFromListGroup(
            RockContext rockContext,
            Guid communicationListGroupGuid,
            SegmentCriteria segmentCriterion,
            List<Guid> segmentDataViewGuids )
        {
            // Get the person aliases from existing communication recipients.
            var personAliasIds = Model.Communication
                .GetCommunicationListMembers(
                    rockContext,
                    communicationListGroupGuid,
                    segmentCriterion,
                    segmentDataViewGuids
                )
                .AsNoTracking()
                .Where( gm => gm.Person.PrimaryAliasId.HasValue )
                // Select the primary PersonAlias for each GroupMember.
                .Select( gm => gm.Person.PrimaryAliasId.Value )
                // Materialize early.
                .ToList();

            if ( !personAliasIds.Any() )
            {
                return new List<CommunicationEntryWizardRecipientBag>();
            }

            var personAliasQuery = new CommunicationOperationsService().LoadPersonAliasEntitySet( rockContext, personAliasIds );

            var mobilePhoneLookup = FindMobilePhoneNumbers( rockContext, personAliasQuery );
            var pushDeviceLookup = FindPushEnabledDevices( rockContext, personAliasQuery );

            return ConvertRecipientsToBags( personAliasQuery, mobilePhoneLookup, pushDeviceLookup );
        }

        /// <summary>
        /// Retrieves recipient bags from an existing communication's recipients.
        /// </summary>
        /// <param name="rockContext">The database context used for querying data.</param>
        /// <param name="communicationId">The ID of the communication to retrieve recipients from.</param>
        /// <returns>A list of <see cref="CommunicationEntryWizardRecipientBag"/> objects representing the recipients.</returns>
        private List<CommunicationEntryWizardRecipientBag> GetRecipientBagsFromCommunication( RockContext rockContext, int communicationId )
        {
            // Get the person aliases from existing communication recipients.
            var personAliasIds = new CommunicationRecipientService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( communicationRecipient =>
                    communicationRecipient.CommunicationId == communicationId
                    && communicationRecipient.PersonAliasId.HasValue )
                .Select( communicationRecipient => communicationRecipient.PersonAliasId.Value )
                // Materialize early.
                .ToList();

            if ( !personAliasIds.Any() )
            {
                return new List<CommunicationEntryWizardRecipientBag>();
            }

            var personAliasQuery = new CommunicationOperationsService().LoadPersonAliasEntitySet( rockContext, personAliasIds );

            var mobilePhoneLookup = FindMobilePhoneNumbers( rockContext, personAliasQuery );
            var pushDeviceLookup = FindPushEnabledDevices( rockContext, personAliasQuery );

            return ConvertRecipientsToBags( personAliasQuery, mobilePhoneLookup, pushDeviceLookup );
        }

        /// <summary>
        /// Retrieves recipient bags for a specified list of individual person aliases.
        /// </summary>
        /// <param name="rockContext">The database context used for querying data.</param>
        /// <param name="personAliasGuids">A list of GUIDs representing the person aliases to retrieve.</param>
        /// <returns>A list of <see cref="CommunicationEntryWizardRecipientBag"/> objects representing the recipients.</returns>
        private List<CommunicationEntryWizardRecipientBag> GetRecipientBagsFromPersonAliases(
            RockContext rockContext,
            List<Guid> personAliasGuids )
        {
            // Get the person aliases from existing communication recipients.
            var personAliasIds = new PersonAliasService( rockContext )
                .GetPrimaryAliasQuery()
                .AsNoTracking()
                .Where( personAlias => personAliasGuids.Contains( personAlias.Guid ) )
                .Select( pa => pa.Id )
                // Materialize early.
                .ToList();

            if ( !personAliasIds.Any() )
            {
                return new List<CommunicationEntryWizardRecipientBag>();
            }

            var personAliasQuery = new CommunicationOperationsService().LoadPersonAliasEntitySet( rockContext, personAliasIds );

            var mobilePhoneLookup = FindMobilePhoneNumbers( rockContext, personAliasQuery );
            var pushDeviceLookup = FindPushEnabledDevices( rockContext, personAliasQuery );

            return ConvertRecipientsToBags( personAliasQuery, mobilePhoneLookup, pushDeviceLookup );
        }

        /// <summary>
        /// Converts a list of person alias information into recipient bags, including details about email, SMS, and push notification capabilities.
        /// </summary>
        /// <param name="recipients">A list of <see cref="PersonAliasInfo"/> objects representing the recipients.</param>
        /// <param name="mobilePhoneLookup">A dictionary mapping person IDs to mobile phone numbers.</param>
        /// <param name="pushDeviceLookup">A hash set containing person alias IDs with notification-enabled personal devices.</param>
        /// <returns>A list of <see cref="CommunicationEntryWizardRecipientBag"/> objects with recipient details.</returns>
        private List<CommunicationEntryWizardRecipientBag> ConvertRecipientsToBags(
            IQueryable<PersonAlias> recipientPersonAliasQuery,
            Dictionary<int, string> mobilePhoneLookup,
            HashSet<int> pushDeviceLookup )
        {
            var recipients = recipientPersonAliasQuery
                .Select( r => new
                {
                    r.Person,
                    r.Id,
                    r.PersonId,
                    r.Guid
                } )
                .ToList();

            return recipients.ToList().Select(
                personAlias => new CommunicationEntryWizardRecipientBag
                {
                    Email = personAlias.Person.Email,
                    EmailPreference = personAlias.Person.EmailPreference.ToString(),
                    IsBulkEmailAllowed = personAlias.Person.CanReceiveEmail( isBulk: true ),
                    IsEmailActive = personAlias.Person.IsEmailActive,
                    IsEmailAllowed = personAlias.Person.CanReceiveEmail( isBulk: false ),
                    IsNameless = personAlias.Person.IsNameless(),
                    IsPushAllowed = pushDeviceLookup.Contains( personAlias.Id ),
                    IsSmsAllowed = mobilePhoneLookup.ContainsKey( personAlias.PersonId ),
                    Name = personAlias.Person.FullName,
                    PersonAliasGuid = personAlias.Guid,
                    PersonId = personAlias.PersonId,
                    PhotoUrl = Person.GetPersonPhotoUrl( personAlias.Person.Initials, personAlias.Person.PhotoId, personAlias.Person.Age, personAlias.Person.Gender, personAlias.Person.RecordTypeValueId, personAlias.Person.AgeClassification, 24 ),
                    SmsNumber = mobilePhoneLookup.TryGetValue( personAlias.PersonId, out var number ) ? number : null,
                } )
                .ToList();
        }

        /// <summary>
        /// Validates a request to save, send, or send a test communication.
        /// </summary>
        /// <param name="bag">The communication request to validate.</param>
        /// <param name="validationResult">Outputs the validation results.</param>
        /// <returns><see langword="true"/> if valid, otherwise <see langword="false"/>.</returns>
        private bool IsValid( CommunicationEntryWizardCommunicationBag bag, out ValidationResult validationResult )
        {
            return bag.Validate( "Request" ).IsNotNull( out validationResult )
                && ( !bag.FutureSendDateTime.HasValue || bag.FutureSendDateTime.Value.Validate( "Send Date Time" ).IsNowOrFuture( out validationResult ) );
        }
        
        /// <summary>
        /// Validates a request to save a metrics reminder.
        /// </summary>
        /// <param name="bag">The save metrics reminder request to validate.</param>
        /// <param name="validationResult">Outputs the validation results.</param>
        /// <returns><see langword="true"/> if valid, otherwise <see langword="false"/>.</returns>
        private bool IsValid( CommunicationEntryWizardSaveMetricsReminderRequestBag bag, out ValidationResult validationResult )
        {
            return bag.Validate( "Save Metrics Reminder Information" ).IsNotNull( out validationResult )
                && bag.CommunicationGuid.Validate( "Communication" ).IsNotEmpty( out validationResult )
                && bag.DaysUntilReminder.Validate( "Days Until Reminder" ).IsGreaterThanOrEqualTo( 1, out validationResult );
        }

        /// <summary>
        /// Retrieves a list of available merge fields for the given communication.
        /// </summary>
        /// <param name="communication">The communication entity to retrieve merge fields from.</param>
        /// <returns>A list of strings representing the available merge fields.</returns>
        private static List<string> GetCommunicationMergeFields( Model.Communication communication )
        {
            var mergeFields = new List<string>
            {
                "GlobalAttribute",
                "Rock.Model.Person",
                "Communication.Subject|Subject",
                "Communication.FromName|From Name",
                "Communication.FromEmail|From Address",
                "Communication.ReplyTo|Reply To",
                "UnsubscribeOption"
            };

            if ( communication?.AdditionalMergeFields.Any() == true )
            {
                mergeFields.AddRange( communication.AdditionalMergeFields );
            }

            return mergeFields;
        }

        /// <summary>
        /// Determines whether the communication should be hidden from the current person based on authorization and status.
        /// </summary>
        /// <param name="communication">The communication entity to evaluate.</param>
        /// <param name="currentPerson">The currently logged-in person for authorization checks.</param>
        /// <returns><see langword="true"/> if the communication should be hidden; otherwise, <see langword="false"/>.</returns>
        private bool IsCommunicationHidden( Model.Communication communication, Person currentPerson )
        {
            if (communication == null)
            {
                // Temporarily initialize a new communication for authorization checks.
                communication = new Model.Communication
                {
                    Status = CommunicationStatus.Transient,
                    CreatedByPersonAlias = currentPerson.PrimaryAlias,
                    CreatedByPersonAliasId = currentPerson.PrimaryAliasId,
                    SenderPersonAlias = currentPerson.PrimaryAlias,
                    SenderPersonAliasId = currentPerson.PrimaryAliasId,
                    CommunicationType = CommunicationType.Email
                };
            }

            var editingApproved = PageParameter( PageParameterKey.Edit ).AsBoolean() && BlockCache.IsAuthorized( "Approve", currentPerson );

            // If viewing a new, transient, draft, or are the approver of a pending-approval communication, use this block
            // otherwise, set this block visible=false and if there is a communication detail block on this page, it'll be shown instead
            var editableStatuses = new CommunicationStatus[]
            {
                CommunicationStatus.Transient,
                CommunicationStatus.Draft,
                CommunicationStatus.Denied
            };

            if ( editableStatuses.Contains( communication.Status ) || ( communication.Status == CommunicationStatus.PendingApproval && editingApproved ) )
            {
                // Make sure they are authorized to edit, or the owner, or the approver/editor
                var isAuthorizedEditor = communication.IsAuthorized( Rock.Security.Authorization.EDIT, currentPerson );
                var isCreator = communication.CreatedByPersonAlias != null && currentPerson?.Id != null && communication.CreatedByPersonAlias.PersonId == currentPerson.Id;
                var isApprovalEditor = communication.Status == CommunicationStatus.PendingApproval && editingApproved;

                if ( isAuthorizedEditor || isCreator || isApprovalEditor )
                {
                    // communication is either new or OK to edit
                    return false;
                }
                else
                {
                    // not authorized, so hide this block
                    return true;
                }
            }
            else
            {
                // Not an editable communication, so hide this block. If there is a CommunicationDetail block on this page, it'll be shown instead
                return true;
            }
        }

        /// <summary>
        /// Retrieves a list of SMS sender numbers available to the current person, filtered by authorization and allowed numbers.
        /// </summary>
        /// <param name="currentPerson">The currently logged-in person for authorization checks.</param>
        /// <returns>A list of <see cref="ListItemBag"/> objects representing available SMS sender numbers.</returns>
        private List<ListItemBag> GetSmsFromNumberBags( Person currentPerson )
        {
            var selectedNumberGuids = this.AllowedSmsNumbersAttributeValue;

            return SystemPhoneNumberCache.All( false )
                .Where( spn => spn.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .Where( spn => !selectedNumberGuids.Any() || selectedNumberGuids.Contains( spn.Guid ) )
                .OrderBy( spn => spn.Order )
                .ThenBy( spn => spn.Name )
                .ThenBy( spn => spn.Id )
                .ToListItemBagList();
        }

        /// <summary>
        /// Retrieves a list of communication list groups available to the current person, filtered by authorization.
        /// </summary>
        /// <param name="rockContext">The database context used for querying data.</param>
        /// <param name="currentPerson">The currently logged-in person for authorization checks.</param>
        /// <returns>A list of <see cref="ListItemBag"/> objects representing the available communication list groups.</returns>
        private List<ListItemBag> GetCommunicationListGroupBags( RockContext rockContext, Person currentPerson )
        {
            var communicationGroupListId = GroupTypeCache.Get( SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST.AsGuid() ).Id;
            var groupService = new GroupService( rockContext );

            var authorizedCommunicationListGroup = groupService
                .Queryable()
                .AsNoTracking()
                .Where( a => a.GroupTypeId == communicationGroupListId && a.IsActive )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList()
                .Where( g => g.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                .ToList();

            return authorizedCommunicationListGroup.ToListItemBagList( communicationListGroup =>
            {
                var name = communicationListGroup.GetAttributeValue( "PublicName" );

                if ( name.IsNotNullOrWhiteSpace() )
                {
                    return name;
                }
                else
                {
                    return communicationListGroup.Name;
                }
            } );
        }

        /// <summary>
        /// Retrieves the allowed communication types based on block configuration and preferences.
        /// </summary>
        /// <param name="forPicker">Indicates whether the allowed types are for a UI picker, affecting the returned results.</param>
        /// <returns>A list of <see cref="CommunicationType"/> values representing the allowed communication types.</returns>
        private List<CommunicationType> GetAllowedCommunicationTypes( bool forPicker = false )
        {
            /*
                JME 8/20/2021
                How the communication type configuration works is tricky. First some background on recipient preference.

                When an individual picks a communication preference they are not given 'Push' as an option. This is
                because push is a very unreliable medium. We often don't know if the person has disabled it and so the
                probability of them getting the message is much lower than email or SMS.

                Before the change below, when the block configuration had 'Recipient Preference' enabled it showed ALL
                mediums. NewSpring did not want that. They wanted 'Recipient Preference' (email and SMS) but not push. We
                made the change below to allow for that.

                At some point we should probably clean up this code a bit to not rely on text values as the keys and make
                the logic more reusable for other places in Rock.
            */

            var communicationTypes = this.GetAttributeValue( AttributeKey.CommunicationTypes ).SplitDelimitedValues( false );

            var result = new List<CommunicationType>();
            if ( !forPicker && communicationTypes.Contains( "Recipient Preference" ) )
            {
                result.Add( CommunicationType.RecipientPreference );

                // Recipient preference requires email and SMS to be shown
                result.Add( CommunicationType.Email );
                result.Add( CommunicationType.SMS );

                // Enabled push only if it is also enabled
                if ( communicationTypes.Contains( "Push" ) )
                {
                    result.Add( CommunicationType.PushNotification );
                }
            }
            else if ( communicationTypes.Any() )
            {
                if ( communicationTypes.Contains( "Email" ) )
                {
                    result.Add( CommunicationType.Email );
                }

                if ( communicationTypes.Contains( "SMS" ) )
                {
                    result.Add( CommunicationType.SMS );
                }

                if ( communicationTypes.Contains( "Push" ) )
                {
                    result.Add( CommunicationType.PushNotification );
                }

                if ( communicationTypes.Contains( "Recipient Preference" ) )
                {
                    result.Add( CommunicationType.RecipientPreference );
                }
            }
            else
            {
                result.Add( CommunicationType.RecipientPreference );
                result.Add( CommunicationType.Email );
                result.Add( CommunicationType.SMS );
                result.Add( CommunicationType.PushNotification );
            }

            return result;
        }

        /// <summary>
        /// Retrieves a list of segment data views available for a given communication list group, including common and additional segments.
        /// </summary>
        /// <param name="rockContext">The database context used for querying data.</param>
        /// <param name="communicationListGroupGuid">The optional GUID of the communication list group to retrieve additional segments for.</param>
        /// <param name="currentPerson">The currently logged-in person for authorization checks.</param>
        /// <returns>A list of <see cref="ListItemBag"/> objects representing authorized segment data views.</returns>
        private List<ListItemBag> GetCommunicationSegmentDataViewBags( RockContext rockContext, Guid? communicationListGroupGuid, Person currentPerson )
        {
            Model.Group communicationListGroup = null;

            if ( communicationListGroupGuid.HasValue )
            {
                communicationListGroup = new GroupService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .FirstOrDefault( c => c.Guid == communicationListGroupGuid.Value );
            }

            // Load common communication segments (each communication list may have additional segments)
            var dataViewCommunicationSegmentsCategoryId = CategoryCache.Get( Rock.SystemGuid.Category.DATAVIEW_COMMUNICATION_SEGMENTS.AsGuid() ).Id;
            var dataViewService = new DataViewService( rockContext );
            var commonAuthorizedSegmentDataViews = dataViewService
                .Queryable()
                .AsNoTracking()
                .Where( a => a.CategoryId == dataViewCommunicationSegmentsCategoryId )
                .OrderBy( a => a.Name )
                .ToList()
                .Where( commonSegmentDataView => commonSegmentDataView.IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson ) )
                .ToList();

            var segmentDataViews = commonAuthorizedSegmentDataViews.ToListItemBagList();

            if ( communicationListGroup != null )
            {
                communicationListGroup.LoadAttributes();
                var segmentAttribute = AttributeCache.Get( SystemGuid.Attribute.GROUP_COMMUNICATION_LIST_SEGMENTS.AsGuid() );

                var additionalSegmentDataViewGuids = communicationListGroup
                    .GetAttributeValue( segmentAttribute.Key )
                    .SplitDelimitedValues()
                    .AsGuidList()
                    // Ignore common segments.
                    .Where( g => !commonAuthorizedSegmentDataViews.Any( c => c.Guid == g ) )
                    .ToList();

                var additionalAuthorizedSegmentDataViews = dataViewService
                    .GetByGuids( additionalSegmentDataViewGuids )
                    .OrderBy( a => a.Name )
                    .ToList()
                    .Where( a => a.IsAuthorized( Authorization.VIEW, currentPerson ) )
                    .ToList();

                segmentDataViews.AddRange( additionalAuthorizedSegmentDataViews.ToListItemBagList() );
            }

            return segmentDataViews;
        }

        /// <summary>
        /// Retrieves a queryable collection of group members for a given communication group, applying optional segmentation criteria.
        /// </summary>
        /// <param name="rockContext">The database context used for querying data.</param>
        /// <param name="communicationGroupGuid">The optional GUID of the communication group to retrieve members from.</param>
        /// <param name="segmentCriteria">The segmentation criteria to apply to the query.</param>
        /// <param name="segmentDataViewGuids">A list of segment data view GUIDs for additional filtering.</param>
        /// <returns>An <see cref="IQueryable{T}"/> of <see cref="GroupMember"/> representing the communication group members.</returns>
        private IQueryable<GroupMember> GetCommunicationGroupMemberQuery( RockContext rockContext, Guid? communicationGroupGuid, SegmentCriteria segmentCriteria, List<Guid> segmentDataViewGuids )
        {
            return Rock.Model.Communication.GetCommunicationListMembers( rockContext, communicationGroupGuid, segmentCriteria, segmentDataViewGuids );
        }

        /// <summary>
        /// Retrieves a list of available communication mediums for the current person based on block settings and transport availability.
        /// </summary>
        /// <param name="currentPerson">The currently logged-in person for authorization checks.</param>
        /// <returns>A list of <see cref="ListItemBag"/> objects representing the available communication mediums.</returns>
        private List<ListItemBag> GetCommunicationMediumBags( Person currentPerson )
        {
            var mediums = new List<ListItemBag>();

            // See what is allowed by the block settings
            var allowedCommunicationTypes = GetAllowedCommunicationTypes( true );

            var isEmailTransportEnabled = MediumContainer.HasActiveAndAuthorizedEmailTransport( currentPerson )
                && allowedCommunicationTypes.Contains( CommunicationType.Email );
            var isSmsTransportEnabled = MediumContainer.HasActiveAndAuthorizedSmsTransport( currentPerson )
                && allowedCommunicationTypes.Contains( CommunicationType.SMS );
            var isPushTransportEnabled = MediumContainer.HasActiveAndAuthorizedPushTransport( currentPerson )
                && allowedCommunicationTypes.Contains( CommunicationType.PushNotification );
            var isRecipientPreferenceEnabled = ( isEmailTransportEnabled || isSmsTransportEnabled || isPushTransportEnabled )
                && allowedCommunicationTypes.Contains( CommunicationType.RecipientPreference );

            // only prompt for Medium Type if more than one will be visible
            if ( isEmailTransportEnabled )
            {
                mediums.Add( new ListItemBag
                {
                    Value = SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL,
                    Text = "Email"
                } );
            }

            if ( isSmsTransportEnabled )
            {
                mediums.Add( new ListItemBag
                {
                    Value = SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS,
                    Text = "SMS"
                } );
            }

            if ( isPushTransportEnabled )
            {
                mediums.Add( new ListItemBag
                {
                    Value = SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION,
                    Text = "Push"
                } );
            }

            // Only add recipient preference if at least two options exists.
            if ( isRecipientPreferenceEnabled && mediums.Count >= 2 )
            {
                mediums.Add( new ListItemBag
                {
                    // If this hard-coded value is changed, then the Obsidian client code should be updated too.
                    Value = "Recipient Preference",
                    Text = "Recipient Preference"
                } );
            }

            return mediums;
        }

        /// <summary>
        /// Sends a test communication based on the given communication details, creating a temporary communication record and sending it to the current user.
        /// </summary>
        /// <param name="bag">The communication details used for the test communication.</param>
        private void SendTestCommunication( CommunicationEntryWizardCommunicationBag bag, out string errorMessage )
        {
            errorMessage = null;
            var communication = SaveCommunication( new RockContext(), bag );

            if ( communication != null )
            {
                // Using a new context (so that changes in the UpdateCommunication() are not persisted )
                using ( var rockContext = new RockContext() )
                {
                    var currentPerson = GetCurrentPerson();
                    // store the CurrentPerson's current Email and SMS number so we can restore it after changing them to the Test Email/SMS Number
                    var testPersonId = currentPerson.Id;
                    var testPersonOriginalEmailAddress = currentPerson.Email;
                    var testPersonOriginalSMSPhoneNumber = currentPerson.PhoneNumbers
                                            .Where( p => p.IsMessagingEnabled )
                                            .Select( a => a.Number )
                                            .FirstOrDefault();

                    Model.Communication testCommunication = null;
                    CommunicationService communicationService = null;

                    try
                    {
                        testCommunication = communication.CloneWithoutIdentity();
                        testCommunication.CreatedByPersonAliasId = currentPerson.PrimaryAliasId;

                        // removed the AsNoTracking() from the next line because otherwise the Person/PersonAlias is attempted (but fails) to be added as new.
                        testCommunication.CreatedByPersonAlias = new PersonAliasService( rockContext ).Queryable().Where( a => a.Id == currentPerson.PrimaryAliasId.Value ).Include( a => a.Person ).FirstOrDefault();
                        testCommunication.EnabledLavaCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands );
                        testCommunication.FutureSendDateTime = null;
                        testCommunication.Status = CommunicationStatus.Approved;
                        testCommunication.ReviewedDateTime = RockDateTime.Now;
                        testCommunication.ReviewerPersonAliasId = currentPerson.PrimaryAliasId;

                        foreach ( var attachment in communication.Attachments )
                        {
                            var cloneAttachment = attachment.Clone( false );
                            cloneAttachment.Id = 0;
                            cloneAttachment.Guid = Guid.NewGuid();
                            cloneAttachment.ForeignGuid = null;
                            cloneAttachment.ForeignId = null;
                            cloneAttachment.ForeignKey = null;

                            testCommunication.Attachments.Add( cloneAttachment );
                        }

                        // for the test email, just use the current person as the recipient, but copy/paste the AdditionalMergeValuesJson to our test recipient so it has the same as the real recipients
                        var testRecipient = new CommunicationRecipient();
                        if ( communication.Recipients.Any() )
                        {
                            var recipient = communication.Recipients.First();
                            testRecipient.AdditionalMergeValuesJson = recipient.AdditionalMergeValuesJson;
                        }

                        testRecipient.Status = CommunicationRecipientStatus.Pending;

                        var sendTestToPerson = new PersonService( rockContext ).Get( currentPerson.Id );
                        if ( bag.CommunicationType == CommunicationEntryWizardCommunicationType.Email )
                        {
                            testCommunication.Subject = $"[Test] {communication.Subject}";
                            sendTestToPerson.Email = bag.TestEmailAddress.ToStringOrDefault( currentPerson.Email );
                        }
                        else if ( bag.CommunicationType == CommunicationEntryWizardCommunicationType.SMS )
                        {
                            testCommunication.SMSMessage = string.Format( "[Test] {0}", communication.SMSMessage );
                            var smsPhoneNumber = sendTestToPerson.PhoneNumbers.FirstOrDefault( a => a.IsMessagingEnabled == true );
                            if ( smsPhoneNumber == null )
                            {
                                var mobilePhoneValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE ).Id;
                                var testPhoneNumber = new PhoneNumber
                                {
                                    IsMessagingEnabled = true,
                                    CountryCode = PhoneNumber.DefaultCountryCode(),
                                    NumberTypeValueId = mobilePhoneValueId,
                                    Number = bag.TestSmsPhoneNumber,
                                    NumberFormatted = PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), bag.TestSmsPhoneNumber ),
                                    ForeignKey = "_ForTestCommunication_"
                                };

                                sendTestToPerson.PhoneNumbers.Add( testPhoneNumber );
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

                        testRecipient.PersonAliasId = sendTestToPerson.PrimaryAliasId.Value;

                        testRecipient.MediumEntityTypeId = EntityTypeCache.GetId( GetMediumEntityTypeGuid( ConvertCommunicationType( bag.CommunicationType ) ) );

                        // If we are just sending a Test Email, don't set it up to use the CommunicationList
                        testCommunication.ListGroup = null;
                        testCommunication.ListGroupId = null;

                        testCommunication.Recipients.Add( testRecipient );

                        communicationService = new CommunicationService( rockContext );
                        communicationService.Add( testCommunication );
                        rockContext.SaveChanges( disablePrePostProcessing: true );

                        foreach ( var medium in testCommunication.GetMediums() )
                        {
                            medium.Send( testCommunication );
                        }

                        testRecipient = new CommunicationRecipientService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( r => r.CommunicationId == testCommunication.Id )
                            .FirstOrDefault();

                        if ( testRecipient != null && testRecipient.Status == CommunicationRecipientStatus.Failed && testRecipient.PersonAlias != null && testRecipient.PersonAlias.Person != null )
                        {
                            errorMessage = $"Test communication to <strong>{testRecipient.PersonAlias.Person.FullName}</strong> failed: {testRecipient.StatusNote}.";
                        }
                    }
                    finally
                    {
                        try
                        {
                            // make sure we delete the test communication record we created to send the test
                            if ( communicationService != null && testCommunication != null )
                            {
                                var testCommunicationId = testCommunication.Id;
                                var pushMediumEntityTypeGuid = Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid();

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

                                // Delete any Person History that was created for the Test Communication
                                using ( var historyContext = new RockContext() )
                                {
                                    var categoryId = CategoryCache.Get( Rock.SystemGuid.Category.HISTORY_PERSON_COMMUNICATIONS.AsGuid() ).Id;
                                    var communicationEntityTypeId = EntityTypeCache.Get( "Rock.Model.Communication" ).Id;
                                    var historyService = new HistoryService( historyContext );
                                    var communicationHistoryQuery = historyService.Queryable().Where( a => a.CategoryId == categoryId && a.RelatedEntityTypeId == communicationEntityTypeId && a.RelatedEntityId == testCommunicationId );
                                    foreach ( History communicationHistory in communicationHistoryQuery )
                                    {
                                        historyService.Delete( communicationHistory );
                                    }

                                    historyContext.SaveChanges( disablePrePostProcessing: true );
                                }
                            }
                        }
                        catch ( Exception ex )
                        {
                            // just log the exception, don't show it
                            ExceptionLogService.LogException( ex );
                        }

                        try
                        {
                            // make sure we restore the CurrentPerson's email/SMS number if it was changed for the test
                            using ( var restorePersonContext = new RockContext() )
                            {
                                var restorePersonService = new PersonService( restorePersonContext );
                                var personToUpdate = restorePersonService.Get( testPersonId );
                                if ( bag.CommunicationType == CommunicationEntryWizardCommunicationType.Email )
                                {
                                    if ( personToUpdate.Email != testPersonOriginalEmailAddress )
                                    {
                                        personToUpdate.Email = testPersonOriginalEmailAddress;
                                        restorePersonContext.SaveChanges( disablePrePostProcessing: true );
                                    }
                                }
                                else if ( bag.CommunicationType == CommunicationEntryWizardCommunicationType.SMS )
                                {
                                    var defaultSMSNumber = personToUpdate.PhoneNumbers.Where( p => p.IsMessagingEnabled ).FirstOrDefault();
                                    if ( defaultSMSNumber != null )
                                    {
                                        if ( defaultSMSNumber.Number != testPersonOriginalSMSPhoneNumber )
                                        {
                                            if ( testPersonOriginalSMSPhoneNumber == null )
                                            {
                                                if ( defaultSMSNumber.ForeignKey == "_ForTestCommunication_" )
                                                {
                                                    new PhoneNumberService( restorePersonContext ).Delete( defaultSMSNumber );
                                                    restorePersonContext.SaveChanges( disablePrePostProcessing: true );
                                                }
                                            }
                                            else
                                            {
                                                defaultSMSNumber.Number = testPersonOriginalSMSPhoneNumber;
                                                defaultSMSNumber.NumberFormatted = PhoneNumber.FormattedNumber( defaultSMSNumber.CountryCode, defaultSMSNumber.Number );
                                                restorePersonContext.SaveChanges( disablePrePostProcessing: true );
                                            }
                                        }
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
                }
            }
        }

        /// <summary>
        /// Generates a preview of the email content for a given communication, resolving merge fields and applying styling if necessary.
        /// </summary>
        /// <param name="communication">The communication entity containing the email content.</param>
        /// <param name="currentPerson">The currently logged-in person for authorization and personalization.</param>
        /// <param name="mergeFields">A dictionary of merge fields used for resolving dynamic content.</param>
        /// <returns>A string containing the resolved HTML preview of the email communication.</returns>
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

        /// <summary>
        /// Retrieves a sample communication recipient from the given communication, defaulting to the current person if no recipients exist.
        /// </summary>
        /// <param name="rockContext">The database context used for querying data.</param>
        /// <param name="communication">The communication entity from which to retrieve a recipient.</param>
        /// <param name="currentPerson">The currently logged-in person, used as a fallback recipient if none exist.</param>
        /// <returns>A <see cref="CommunicationRecipient"/> representing the first recipient or a fallback recipient.</returns>
        private CommunicationRecipient GetSampleCommunicationRecipient( Model.Communication communication, Person currentPerson )
        {
            using ( var activity = ObservabilityHelper.StartActivity( "COMMUNICATION: Entry Wizard > Get Sample Communication Recipient" ) )
            {
                // If we have recipients in the communication then just return the first one.
                if ( communication.Recipients.Any() )
                {
                    return communication.Recipients.First();
                }
                else
                {
                    // If we can't find a recipient, use the logged-in person.
                    var recipient = new CommunicationRecipient
                    {
                        Communication = communication,
                        PersonAlias = currentPerson.PrimaryAlias
                    };

                    return recipient;
                }
            }
        }

        /// <summary>
        /// Generates a communication send channel identifier using the given communication GUID.
        /// </summary>
        /// <param name="communicationGuid">The unique identifier of the communication.</param>
        /// <returns>A string representing the communication send channel.</returns>
        private static string GetCommunicationSendChannel( Guid communicationGuid )
        {
            return $"Communication:Send:{communicationGuid}";
        }

        /// <summary>
        /// Processes and sends a communication asynchronously, updating recipients,
        /// handling approval checks, and queuing the message for delivery.
        /// </summary>
        /// <param name="bag">The communication details used for processing and sending.</param>
        private void ProcessCommunicationSend( CommunicationEntryWizardCommunicationBag bag )
        {
            var currentPerson = GetCurrentPerson();

            var progressReporter = RealTimeHelper.GetTopicContext<ITaskActivityProgress>().Clients.Channels( new[] { GetCommunicationSendChannel( bag.CommunicationGuid ) } );

            // Define a background task for the bulk send process, because it may take considerable time.
            var taskSend = new Task( () =>
            {
                progressReporter.TaskStarted( new TaskActivityProgressStatusBag
                {
                    IsStarted = true
                } );

                progressReporter.UpdateTaskProgress( new TaskActivityProgressUpdateBag
                {
                    CompletionPercentage = 0m,
                    Message = "Working...",
                } );

                var rockContext = new RockContext(); // Create new context within the task so it can remain open.

                Model.Communication communication = null;
                using ( var activity = ObservabilityHelper.StartActivity( "COMMUNICATION: Entry Wizard > Send Communication > Update Communication and Recipients" ) )
                {
                    progressReporter.UpdateTaskProgress( new TaskActivityProgressUpdateBag
                    {
                        CompletionPercentage = 1m,
                        Message = "Updating Communication..."
                    } );

                    communication = SaveCommunication( rockContext, bag );

                    progressReporter.UpdateTaskProgress( new TaskActivityProgressUpdateBag
                    {
                        CompletionPercentage = 2m,
                        Message = "Updating Communication..."
                    } );

                    UpdateCommunicationRecipients( rockContext, bag, communication, progressReporter );
                }

                var finalMessage = string.Empty;
                using ( var activity = ObservabilityHelper.StartActivity( "COMMUNICATION: Entry Wizard > Send Communication > Finalizing Communication" ) )
                {
                    progressReporter.UpdateTaskProgress( new TaskActivityProgressUpdateBag { CompletionPercentage = 99m, Message = "Finalizing Communication..." } );

                    var maxRecipients = GetAttributeValue( AttributeKey.MaximumRecipients ).AsIntegerOrNull() ?? int.MaxValue;
                    var userCanApprove = this.BlockCache.IsAuthorized( "Approve", currentPerson );
                    var recipientCount = communication.Recipients.Count();
                    if ( recipientCount > maxRecipients && !userCanApprove )
                    {
                        communication.Status = CommunicationStatus.PendingApproval;
                        finalMessage = "Communication has been submitted for approval.";
                    }
                    else
                    {
                        communication.Status = CommunicationStatus.Approved;
                        communication.ReviewedDateTime = RockDateTime.Now;
                        communication.ReviewerPersonAliasId = currentPerson.PrimaryAliasId;

                        if ( communication.FutureSendDateTime.HasValue &&
                                        communication.FutureSendDateTime > RockDateTime.Now )
                        {
                            finalMessage = $"Communication will be sent {communication.FutureSendDateTime.Value.ToRelativeDateString( 0 )}.";
                        }
                        else
                        {
                            finalMessage = "Communication has been queued for sending.";
                        }
                    }

                    /*
                        1/2/2024 - JPH

                        Rather than leveraging the default EF behavior of inserting each new recipient one-by-one,
                        let's remove them from change tracking and perform a BULK INSERT operation instead, after
                        saving the parent Communication record.

                        We can get away with this because none of the downstream processes further reference the
                        Communication.Recipients collection. If this changes, we will need to rethink this strategy.

                        Reason: Communications with a large number of recipients time out and don't send.
                        https://github.com/SparkDevNetwork/Rock/issues/5651
                    */
                    var newRecipients = new List<CommunicationRecipient>( communication.Recipients.Where( r => r.Id == 0 ) );

                    // Stop tracking these entities.
                    communication.Recipients.RemoveAll( newRecipients );

                    // Save the communication entity and any updated/deleted recipients.
                    rockContext.SaveChanges();

                    if ( newRecipients.Any() )
                    {
                        using ( var bulkInsertActivity = ObservabilityHelper.StartActivity( "COMMUNICATION: Entry Wizard > Send Communication > Bulk-Insert New Communication Recipients" ) )
                        {
                            foreach ( var recipient in newRecipients )
                            {
                                recipient.CommunicationId = communication.Id;
                            }

                            rockContext.BulkInsert<CommunicationRecipient>( newRecipients );
                        }
                    }
                }

                // send approval email if needed (now that we have a communication id)
                if ( communication.Status == CommunicationStatus.PendingApproval )
                {
                    var approvalTransactionMsg = new ProcessSendCommunicationApprovalEmail.Message
                    {
                        CommunicationId = communication.Id
                    };
                    approvalTransactionMsg.Send();
                }

                if ( communication.Status == CommunicationStatus.Approved &&
                    ( !communication.FutureSendDateTime.HasValue || communication.FutureSendDateTime.Value <= RockDateTime.Now ) )
                {
                    if ( GetAttributeValue( AttributeKey.SendWhenApproved ).AsBoolean() )
                    {
                        var processSendCommunicationMsg = new ProcessSendCommunication.Message
                        {
                            CommunicationId = communication.Id,
                        };
                        processSendCommunicationMsg.Send();
                    }
                }

                progressReporter.TaskCompleted( new TaskActivityProgressStatusBag
                {
                    Message = finalMessage,
                    Data = new
                    {
                        CommunicationId = communication.Id
                    },
                    IsStarted = true,
                    IsFinished = true
                } );
            } );

            // Add a continuation task to handle any exceptions during the send process.
            taskSend.ContinueWith(
                t =>
                {
                    if ( t.Exception != null )
                    {
                        ExceptionLogService.LogException( new Exception( "Send Communication failed.", t.Exception.Flatten().InnerException ) );
                    }

                    progressReporter.TaskCompleted( new TaskActivityProgressStatusBag
                    {
                        Message = "Communication send failed. Check the Exception Log for further details.",
                        Errors = new List<string> { "Communication send failed. Check the Exception Log for further details." },
                        IsStarted = true,
                        IsFinished = true
                    } );
                },
                TaskContinuationOptions.OnlyOnFaulted );

            // Start the background processing task and complete this request.
            // The task will continue to run until complete, delivering client status notifications via the SignalR hub.
            taskSend.Start();
        }

        /// <summary>
        /// Saves the given communication as a draft, updating recipients and storing the changes.
        /// </summary>
        /// <param name="rockContext">The database context used for querying and saving data.</param>
        /// <param name="bag">The communication details to save as a draft.</param>
        /// <returns>The saved <see cref="Model.Communication"/> entity in draft status.</returns>
        private Model.Communication SaveAsDraft( RockContext rockContext, CommunicationEntryWizardCommunicationBag bag )
        {
            using ( var activity = ObservabilityHelper.StartActivity( "COMMUNICATION: Entry Wizard > Save As Draft" ) )
            {
                var communication = SaveCommunication( rockContext, bag );
                UpdateCommunicationRecipients( rockContext, bag, communication );

                communication.Status = CommunicationStatus.Draft;
                rockContext.SaveChanges();

                activity?.AddTag( "rock.communication.id", communication.Id );
                activity?.AddTag( "rock.communication.name", communication.Name );

                return communication;
            }
        }

        /// <summary>
        /// Updates or creates a communication record based on the provided details.
        /// </summary>
        /// <param name="rockContext">The database context used for querying and saving data.</param>
        /// <param name="bag">The communication details to apply to the update or creation process.</param>
        /// <returns>The updated or newly created <see cref="Model.Communication"/> entity.</returns>
        private Model.Communication SaveCommunication( RockContext rockContext, CommunicationEntryWizardCommunicationBag bag )
        {
            var currentPerson = GetCurrentPerson();

            // Get the current wizard settings for the new Communication.
            var communicationInfo = new CommunicationOperationsService.CreateOrUpdateCommunicationInfo
            {
                CommunicationId = bag.CommunicationId,
                CommunicationGuid = bag.CommunicationGuid,
                SenderPersonAliasId = currentPerson.PrimaryAliasId,
                EnabledLavaCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands ),

                CommunicationName = bag.CommunicationName,
                IsBulkCommunication = bag.IsBulkCommunication,
                CommunicationType = ConvertCommunicationType( bag.CommunicationType )
            };

            if ( bag.IndividualRecipientPersonAliasGuids?.Any() != true && bag.CommunicationListGroupGuid.HasValue )
            {
                communicationInfo.CommunicationListGroupGuid = bag.CommunicationListGroupGuid.Value;
                communicationInfo.PersonalizationSegmentIds = bag.PersonalizationSegmentIds;
                communicationInfo.CommunicationGroupSegmentCriteria = bag.SegmentCriteria;
            }            

            communicationInfo.CommunicationTemplateGuid = bag.CommunicationTemplateGuid;
            
            communicationInfo.ExcludeDuplicateRecipientAddress = bag.ExcludeDuplicateRecipientAddress;

            var emailAttachmentBinaryFileGuids = bag.EmailAttachmentBinaryFiles
                ?.Select( b => b.Value.AsGuidOrNull() )
                .Where( g => g.HasValue )
                .Select( g => g.Value )
                .ToList();
            if ( emailAttachmentBinaryFileGuids?.Any() == true )
            {
                communicationInfo.EmailBinaryFileIds = new BinaryFileService( rockContext )
                    .GetByGuids( emailAttachmentBinaryFileGuids )
                    .Select( b => b.Id )
                    .ToList();
            }

            var smsAttachmentBinaryFileGuids = bag.SmsAttachmentBinaryFiles
                ?.Select( b => b.Value.AsGuidOrNull() )
                .Where( g => g.HasValue )
                .Select( g => g.Value )
                .ToList();
            if ( smsAttachmentBinaryFileGuids?.Any() == true )
            {
                communicationInfo.SmsBinaryFileIds = new BinaryFileService( rockContext )
                    .GetByGuids( smsAttachmentBinaryFileGuids )
                    .Select( b => b.Id )
                    .ToList();
            }

            if ( bag.FutureSendDateTime.HasValue )
            {
                communicationInfo.FutureSendDateTime = bag.FutureSendDateTime.Value.DateTime;
            }
            else
            {
                communicationInfo.FutureSendDateTime = null;
            }

            var communicationTopicValueGuid = bag.CommunicationTopicValue?.Value.AsGuidOrNull();

            if ( communicationTopicValueGuid.HasValue )
            {
                var definedValueService = new DefinedValueService( rockContext );
                communicationInfo.CommunicationTopicValueId = definedValueService.GetId( communicationTopicValueGuid.Value );
            }

            var details = new CommunicationDetails
            {
                Subject = bag.Subject,
                Message = bag.Message,

                FromName = bag.FromName,
                FromEmail = bag.FromEmail,
                ReplyToEmail = bag.ReplyToEmail,

                CCEmails = bag.CcEmails,
                BCCEmails = bag.BccEmails,

                SmsFromSystemPhoneNumberId = bag.SmsFromSystemPhoneNumberGuid.HasValue ? SystemPhoneNumberCache.GetId( bag.SmsFromSystemPhoneNumberGuid.Value ) : null,
                SMSMessage = bag.SmsMessage,

                PushTitle = bag.PushTitle,
                PushMessage = bag.PushMessage
            };

            if ( bag.PushImageBinaryFileGuid.HasValue )
            {
                details.PushImageBinaryFileId = new BinaryFileService( rockContext )
                    .GetQueryableByKey( bag.PushImageBinaryFileGuid.Value.ToString() )
                    .Select( b => b.Id )
                    .FirstOrDefault();
            }

            if ( bag.PushOpenAction.HasValue )
            {
                details.PushOpenAction = ConvertPushOpenAction( bag.PushOpenAction.Value );
            }

            if ( details.PushOpenAction == PushOpenAction.ShowDetails )
            {
                details.PushOpenMessage = bag.PushOpenMessage;

                if ( bag.PushOpenMessageJson.IsNotNullOrWhiteSpace() )
                {
                    details.PushOpenMessageJson = bag.PushOpenMessageJson;
                }
            }

            details.PushData = ConvertPushData( bag.PushData )?.ToJson();

            communicationInfo.Details = details;

            // Update the Communication by applying the new settings.
            var operationsService = new CommunicationOperationsService();

            var communication = operationsService.CreateOrUpdateCommunication( rockContext, communicationInfo );

            return communication;
        }

        /// <summary>
        /// Updates the recipients of a communication based on individual recipients or a communication list group.
        /// </summary>
        /// <param name="rockContext">The database context used for querying and saving data.</param>
        /// <param name="bag">The communication details, including recipient information.</param>
        /// <param name="communication">The communication entity to update with recipient information.</param>
        /// <param name="progressReporter">An optional progress reporter for tracking task progress.</param>
        private void UpdateCommunicationRecipients( RockContext rockContext, CommunicationEntryWizardCommunicationBag bag, Model.Communication communication, ITaskActivityProgress progressReporter = null )
        {
            if ( communication == null )
            {
                return;
            }

            using ( var activity = ObservabilityHelper.StartActivity( "COMMUNICATION: Entry Wizard > Update Communication Recipients" ) )
            {
                progressReporter?.UpdateTaskProgress(new TaskActivityProgressUpdateBag { CompletionPercentage = 3m, Message = "Initializing recipient update..." });

                var updatedCommunicationRecipients = GetUpdatedCommunicationRecipients( rockContext, bag );

                var operationsService = new CommunicationOperationsService();
                operationsService.UpdateCommunicationRecipients( rockContext, communication, updatedCommunicationRecipients, progressReporter );

                // rockContext.SaveChanges() is called deep within the UpdateCommunicationRecipients() call above,
                // so wait until we get back from that method to add the ID tag.
                activity?.AddTag( "rock.communication.id", communication.Id );
                activity?.AddTag( "rock.communication.name", communication.Name );
            }
        }

        /// <summary>
        /// Retrieves the list of recipient person aliases.
        /// </summary>
        private List<CommunicationEntryWizardRecipientPersonInfo> GetUpdatedCommunicationRecipients( RockContext rockContext, CommunicationEntryWizardCommunicationBag bag )
        {
            if ( bag.IndividualRecipientPersonAliasGuids?.Any() == true )
            {
                return new PersonAliasService( rockContext )
                    .GetByGuids( bag.IndividualRecipientPersonAliasGuids )
                    .Include( pa => pa.Person )
                    .Select( pa => new CommunicationEntryWizardRecipientPersonInfo
                    {
                        PersonAlias = pa
                    } )
                    .ToList();
            }
            else
            {
                var listId = bag.CommunicationListGroupGuid.HasValue ? GroupCache.GetId( bag.CommunicationListGroupGuid.Value ) : null;

                if ( listId.HasValue )
                {
                    var recipients = GetCommunicationRecipientDetailsForList( rockContext, listId.Value, bag.SegmentCriteria, bag.PersonalizationSegmentIds );
                    var groupMemberMap = recipients.ToDictionary( r => r.PrimaryAliasId, r => r.GroupMemberCommunicationPreference );
                    
                    var personAliases = new PersonAliasService( rockContext )
                        .GetByIds( recipients.Select( r => r.PrimaryAliasId ).ToList() )
                        .Include( pa => pa.Person )
                        .ToList();

                    return personAliases
                        .Select( pa => new CommunicationEntryWizardRecipientPersonInfo
                        {
                            PersonAlias = pa,
                            GroupMemberCommunicationPreference = groupMemberMap.GetValueOrDefault( pa.Id, null )
                        } )
                        .ToList();
                }
                else
                {
                    return new List<CommunicationEntryWizardRecipientPersonInfo>();
                }
            }
        }

        #endregion Methods

        #region Helper Types

        /// <summary>
        /// Represents the details of a communication recipient including contact preferences and identity data.
        /// </summary>
        private class CommunicationEntryWizardRecipientInfo
        {
            /// <summary>
            /// Gets or sets the unique identifier of the person.
            /// </summary>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the person's nickname. May be null if not specified.
            /// </summary>
            public string NickName { get; set; }

            /// <summary>
            /// Gets or sets the person's last name.
            /// </summary>
            public string LastName { get; set; }

            /// <summary>
            /// Gets or sets the ID of the person's profile photo, if available.
            /// </summary>
            public int? PhotoId { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the person is eligible to receive email.
            /// </summary>
            public bool IsEmailEnabled { get; set; }

            /// <summary>
            /// Gets or sets the person's email address. May be null or empty.
            /// </summary>
            public string Email { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the person's email is active.
            /// </summary>
            public bool IsEmailActive { get; set; }

            /// <summary>
            /// Gets or sets any note or comment associated with the email. Optional.
            /// </summary>
            public string EmailNote { get; set; }

            /// <summary>
            /// Gets or sets the connection status defined value ID. Optional.
            /// </summary>
            public int? ConnectionStatusValueId { get; set; }

            /// <summary>
            /// Gets or sets the person's overall communication preference.
            /// </summary>
            public CommunicationType? CommunicationPreference { get; set; }

            /// <summary>
            /// Gets or sets the group member's overall communication preference.
            /// </summary>
            public CommunicationType? GroupMemberCommunicationPreference { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the person can receive SMS messages.
            /// </summary>
            public bool IsSmsEnabled { get; set; }

            /// <summary>
            /// Gets or sets the formatted mobile phone number. May be null.
            /// </summary>
            public string MobilePhoneNumber { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether messaging is enabled for the phone number.
            /// </summary>
            public bool IsMessagingEnabled { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the person has opted out of messaging.
            /// </summary>
            public bool IsMessagingOptedOut { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the person has a registered device with push notifications enabled.
            /// </summary>
            public bool IsPushEnabled { get; set; }

            /// <summary>
            /// Gets or sets the device registration ID used for push notifications, if available.
            /// </summary>
            public string DeviceRegistrationId { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether notifications are enabled on the registered device.
            /// </summary>
            public bool NotificationsEnabled { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the person is eligible to receive bulk emails.
            /// </summary>
            public bool IsBulkEmailEnabled { get; set; }

            /// <summary>
            /// Gets or sets the email preference value for the person.
            /// </summary>
            public int? EmailPreference { get; set; }

            /// <summary>
            /// Gets or sets the record type defined value ID (e.g., individual, business).
            /// </summary>
            public int? RecordTypeValueId { get; set; }

            /// <summary>
            /// Gets or sets the suffix defined value ID (e.g., Jr., Sr.). Optional.
            /// </summary>
            public int? SuffixValueId { get; set; }

            /// <summary>
            /// Gets or sets the person's primary alias GUID. Guaranteed to be non-null.
            /// </summary>
            public Guid PrimaryAliasGuid { get; set; }

            /// <summary>
            /// Gets or sets the ID of the person's primary alias.
            /// </summary>
            public int PrimaryAliasId { get; set; }

            /// <summary>
            /// Gets or sets the person's age classification (e.g., Adult, Child).
            /// </summary>
            public AgeClassification? AgeClassification { get; set; }

            /// <summary>
            /// Gets or sets the person's age, if calculable.
            /// </summary>
            public int? Age { get; set; }

            /// <summary>
            /// Gets or sets the person's gender. Optional.
            /// </summary>
            public Gender? Gender { get; set; }
        }

        /// <summary>
        /// Represents a simplified person alias information structure used for recipient lookups.
        /// </summary>
        private class PersonAliasInfo
        {
            public int Id { get; set; }

            public Guid Guid { get; set; }

            public int PersonId { get; set; }

            public Person Person { get; set; }
        }

        /// <summary>
        /// Represents metadata for a communication template used in the Communication Entry Wizard.
        /// </summary>
        class CommunicationEntryWizardTemplateInfo
        {
            /// <summary>
            /// Gets or sets the communication template associated with this entry.
            /// </summary>
            public CommunicationTemplate CommunicationTemplate { get; set; }

            /// <summary>
            /// Gets or sets the number of communications that have used this template.
            /// </summary>
            public int CommunicationCount { get; set; }

            /// <summary>
            /// Gets or sets the image file associated with this template, if any.
            /// </summary>
            public ListItemBag ImageFile { get; set; }

            /// <summary>
            /// Gets or sets the system phone number GUID used for SMS communication, if applicable.
            /// </summary>
            public Guid? SmsFromSystemPhoneNumberGuid { get; set; }

            /// <summary>
            /// Gets or sets the binary file GUID of the push notification image, if applicable.
            /// </summary>
            public Guid? PushImageBinaryFileGuid { get; set; }
            

            /// <summary>
            /// Gets or sets the category GUID associated with this template, if any.
            /// </summary>
            public ListItemBag Category { get; set; }
        }

        private class CommunicationEntryWizardRecipientPersonInfo
        {
            public PersonAlias PersonAlias { get; set; }
            public CommunicationType? GroupMemberCommunicationPreference { get; set; }
        }

        #endregion Helper Types

        #region Service Classes

        /// <summary>
        /// Provides operations for creating, updating, and managing communications within the Communication Entry Wizard.
        /// </summary>
        /// <remarks>
        /// This service handles communication entity modifications, recipient management,
        /// and attachment processing. Check each method's documentation for persistence information.
        /// </remarks>
        private class CommunicationOperationsService
        {
            /// <summary>
            /// Creates or updates a communication entity in memory based on the provided settings.
            /// <para>
            /// This method persists the changes to the database.
            /// </para>
            /// </summary>
            /// <param name="rockContext">The database context used for querying data.</param>
            /// <param name="settings">The settings containing details for the communication update or creation.</param>
            /// <returns>The updated or newly created <see cref="Model.Communication"/> entity.</returns>
            public Model.Communication CreateOrUpdateCommunication( RockContext rockContext, CreateOrUpdateCommunicationInfo settings )
            {
                var communicationService = new CommunicationService( rockContext );
                var communicationAttachmentService = new CommunicationAttachmentService( rockContext );
                var communicationRecipientService = new CommunicationRecipientService( rockContext );
                var groupService = new GroupService( rockContext );

                Model.Communication communication = null;

                if ( settings.CommunicationId.GetValueOrDefault( 0 ) > 0 )
                {
                    communication = communicationService.Queryable()
                        .Include( c => c.Recipients )
                        .Include( c => c.CommunicationTemplate )
                        .Include( c => c.ListGroup )
                        .FirstOrDefault( c => c.Id == settings.CommunicationId.Value );
                }
                else if ( !settings.CommunicationGuid.IsEmpty() )
                {
                    communication = communicationService.Queryable()
                        .Include( c => c.Recipients )
                        .Include( c => c.CommunicationTemplate )
                        .Include( c => c.ListGroup )
                        .FirstOrDefault( c => c.Guid == settings.CommunicationGuid );
                }

                if ( communication == null )
                {
                    communication = new Model.Communication
                    {
                        Guid = settings.CommunicationGuid,
                        Status = CommunicationStatus.Transient,
                        SenderPersonAliasId = settings.SenderPersonAliasId,
                    };
                    communicationService.Add( communication );
                }

                communication.EnabledLavaCommands = settings.EnabledLavaCommands;

                communication.Name = settings.CommunicationName.TrimForMaxLength( communication, "Name" );
                communication.IsBulkCommunication = settings.IsBulkCommunication;
                communication.CommunicationType = settings.CommunicationType;

                if ( settings.CommunicationListGroupGuid.HasValue )
                {
                    if ( communication.ListGroup?.Guid != settings.CommunicationListGroupGuid.Value )
                    {
                        communication.ListGroup = groupService.Get( settings.CommunicationListGroupGuid.Value );
                        communication.ListGroupId = communication.ListGroup?.Id;
                    }
                }
                else
                {
                    communication.ListGroup = null;
                    communication.ListGroupId = null;
                }

                communication.ExcludeDuplicateRecipientAddress = settings.ExcludeDuplicateRecipientAddress;

                communication.Segments = null;
                communication.SegmentCriteria = settings.CommunicationGroupSegmentCriteria;

                if ( settings.CommunicationTemplateGuid.HasValue && !settings.CommunicationTemplateGuid.Value.IsEmpty() )
                {
                    if ( settings.CommunicationTemplateGuid.Value != communication.CommunicationTemplate?.Guid )
                    {
                        communication.CommunicationTemplate = new CommunicationTemplateService( rockContext ).Get( settings.CommunicationTemplateGuid.Value );
                        communication.CommunicationTemplateId = communication.CommunicationTemplate?.Id;
                    }

                    communication.PersonalizationSegments = settings.PersonalizationSegmentIds?.AsDelimited( "," );
                }
                else
                {
                    communication.CommunicationTemplate = null;
                    communication.CommunicationTemplateId = null;
                }

                communication.FromName = settings.Details.FromName.TrimForMaxLength( communication, "FromName" );
                communication.FromEmail = settings.Details.FromEmail.TrimForMaxLength( communication, "FromEmail" );
                communication.ReplyToEmail = settings.Details.ReplyToEmail.TrimForMaxLength( communication, "ReplyToEmail" );
                communication.CCEmails = settings.Details.CCEmails;
                communication.BCCEmails = settings.Details.BCCEmails;

                var emailBinaryFileIds = settings.EmailBinaryFileIds ?? new List<int>();
                var smsBinaryFileIds = settings.SmsBinaryFileIds ?? new List<int>();

                using ( var activity = ObservabilityHelper.StartActivity( "COMMUNICATION: Entry Wizard > Create Or Update Communication > Add/Remove Attachments" ) )
                {
                    if ( communication.Id > 0 )
                    {
                        activity?.AddTag( "rock.communication.id", communication.Id );
                    }

                    activity?.AddTag( "rock.communication.name", communication.Name );

                    // delete any attachments that are no longer included
                    foreach ( var attachment in communication.Attachments.Where( a => ( !emailBinaryFileIds.Contains( a.BinaryFileId ) && !smsBinaryFileIds.Contains( a.BinaryFileId ) ) ).ToList() )
                    {
                        communication.Attachments.Remove( attachment );
                        communicationAttachmentService.Delete( attachment );
                    }

                    // add any new email attachments that were added
                    foreach ( var attachmentBinaryFileId in emailBinaryFileIds.Where( a => !communication.Attachments.Any( x => x.BinaryFileId == a ) ) )
                    {
                        communication.Attachments.Add( new CommunicationAttachment { BinaryFileId = attachmentBinaryFileId, CommunicationType = CommunicationType.Email } );
                    }

                    // add any new SMS attachments that were added
                    foreach ( var attachmentBinaryFileId in smsBinaryFileIds.Where( a => !communication.Attachments.Any( x => x.BinaryFileId == a ) ) )
                    {
                        communication.Attachments.Add( new CommunicationAttachment { BinaryFileId = attachmentBinaryFileId, CommunicationType = CommunicationType.SMS } );
                    }
                }

                communication.Subject = settings.Details.Subject.TrimForMaxLength( communication, "Subject" );
                communication.Message = settings.Details.Message;

                communication.SmsFromSystemPhoneNumberId = settings.Details.SmsFromSystemPhoneNumberId;
                communication.SMSMessage = settings.Details.SMSMessage;

                communication.FutureSendDateTime = settings.FutureSendDateTime;

                communication.PushData = settings.Details.PushData;
                communication.PushImageBinaryFileId = settings.Details.PushImageBinaryFileId;
                communication.PushMessage = settings.Details.PushMessage;
                communication.PushOpenAction = settings.Details.PushOpenAction;
                communication.PushOpenMessage = settings.Details.PushOpenMessage;
                communication.PushOpenMessageJson = settings.Details.PushOpenMessageJson;
                communication.PushTitle = settings.Details.PushTitle;

                communication.CommunicationTopicValueId = settings.CommunicationTopicValueId;

                rockContext.SaveChanges();

                return communication;
            }

            /// <summary>
            /// Handles adding and removing recipients for a given communication.
            /// This method ensures that recipients are properly updated, persisted, and processed efficiently.
            /// </summary>
            /// <param name="rockContext">The database context.</param>
            /// <param name="communication">The communication entity to update.</param>
            /// <param name="updatedCommunicationRecipientPersonAliasIds">A list of person alias GUIDs representing intended recipients.</param>
            /// <param name="progressReporter">An optional progress reporter for tracking task progress.</param>
            /// <returns>The updated <see cref="Model.Communication"/> entity with modified recipients.</returns>
            public Model.Communication UpdateCommunicationRecipients(
                RockContext rockContext,
                Model.Communication communication,
                List<CommunicationEntryWizardRecipientPersonInfo> updatedCommunicationRecipients,
                ITaskActivityProgress progressReporter = null )
            {
                if ( communication == null )
                {
                    throw new ArgumentException( "UpdateCommunicationRecipients failed. A Communication instance is required." );
                }

                progressReporter?.UpdateTaskProgress( new TaskActivityProgressUpdateBag { CompletionPercentage = 3m, Message = "Initializing recipient update..." } );

                var communicationRecipientService = new CommunicationRecipientService( rockContext );
                var existingRecipients = GetExistingRecipients( communication );
                var updatedCommunicationRecipientPersonAliasIds = updatedCommunicationRecipients.Select( cr => cr.PersonAlias.Id ).ToHashSet();

                RemoveUnselectedRecipients( rockContext, existingRecipients, updatedCommunicationRecipientPersonAliasIds, progressReporter );
                AddNewRecipients( rockContext, communication, updatedCommunicationRecipients, progressReporter );
                
                // Reload the recipients.
                rockContext.Entry( communication )
                    .Collection( c => c.Recipients )
                    .Load();
                
                return communication;
            }

            private class ExistingCommunicationRecipientInfo
            {
                public int CommunicationRecipientId { get; set; }
                public int PersonAliasId { get; set; }
            }

            /// <summary>
            /// Retrieves the existing recipients for a communication.
            /// </summary>
            private List<ExistingCommunicationRecipientInfo> GetExistingRecipients( Model.Communication communication )
            {
                return communication.Recipients
                    .Where( cr => cr.PersonAliasId.HasValue )
                    .Select(
                        cr => new ExistingCommunicationRecipientInfo
                        {
                            CommunicationRecipientId = cr.Id,
                            PersonAliasId = cr.PersonAliasId.Value
                        } )
                    .ToList();
            }

            /// <summary>
            /// Removes recipients from the communication if they are no longer selected.
            /// </summary>
            private void RemoveUnselectedRecipients(
                RockContext rockContext,
                List<ExistingCommunicationRecipientInfo> existingCommunicationRecipients,
                HashSet<int> updatedCommunicationRecipientPersonAliasIds,
                ITaskActivityProgress progressReporter )
            {
                var communicationRecipientIdsToRemove = existingCommunicationRecipients
                    .Where( r => !updatedCommunicationRecipientPersonAliasIds.Contains( r.PersonAliasId ) )
                    .Select( r => r.CommunicationRecipientId )
                    .ToList();

                if ( communicationRecipientIdsToRemove.Any() )
                {
                    // Update DB.
                    var batchSize = 1000;
                    var totalDeleted = 0;
                    var deletedInBatch = 0;

                    do
                    {
                        var batchIdsToDelete = communicationRecipientIdsToRemove.Skip( totalDeleted ).Take( batchSize ).ToList();

                        if ( !batchIdsToDelete.Any() )
                        {
                            break;
                        }

                        var sql = $"DELETE FROM [CommunicationRecipient] WHERE [Id] IN ({string.Join( ", ", batchIdsToDelete )})";
                        deletedInBatch = rockContext.Database.ExecuteSqlCommand( sql );
                        totalDeleted += deletedInBatch;

                    } while ( deletedInBatch > 0 );
                    
                    progressReporter?.UpdateTaskProgress( new TaskActivityProgressUpdateBag { CompletionPercentage = 10m, Message = "Removed unselected recipients..." } );
                }
            }

            /// <summary>
            /// Adds new recipients to the communication.
            /// </summary>
            private void AddNewRecipients(
                RockContext rockContext,
                Model.Communication communication,
                List<CommunicationEntryWizardRecipientPersonInfo> updatedCommunicationRecipientPersonAliases,
                ITaskActivityProgress progressReporter )
            {
                var existingCommunicationRecipientPersonAliasIds = communication.Recipients
                    .Where( r => r.PersonAliasId.HasValue )
                    .Select( r => r.PersonAliasId.Value )
                    .ToHashSet();
                
                var emailMediumEntityTypeId = EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() ).Id;
                var smsMediumEntityTypeId = EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() ).Id;
                var pushMediumEntityTypeId = EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() ).Id;
                
                var newCommunicationRecipients = updatedCommunicationRecipientPersonAliases
                    .Where( r => !existingCommunicationRecipientPersonAliasIds.Contains( r.PersonAlias.Id ) )
                    .Select(
                        r => new CommunicationRecipient
                        {
                            PersonAlias = r.PersonAlias,
                            PersonAliasId = r.PersonAlias.Id,
                            Communication = communication,
                            CommunicationId = communication.Id,
                            MediumEntityTypeId = Rock.Model.Communication.DetermineMediumEntityTypeId(
                                emailMediumEntityTypeId,
                                smsMediumEntityTypeId,
                                pushMediumEntityTypeId,
                                communication.CommunicationType,
                                r.GroupMemberCommunicationPreference ?? default,
                                r.PersonAlias.Person.CommunicationPreference )
                        } )
                    .ToList();

                if ( newCommunicationRecipients.Any() )
                {
                    // Update DB.
                    rockContext.BulkInsert( newCommunicationRecipients );

                    progressReporter?.UpdateTaskProgress( new TaskActivityProgressUpdateBag { CompletionPercentage = 20m, Message = "Added new recipients..." } );
                }
            }

            /// <summary>
            /// Creates a new EntitySet containing the list of PersonAlias records and returns a queryable of the entities.
            /// </summary>
            /// <remarks>
            /// The result can be referenced as a subquery, thereby avoiding the need to pass a large list of keys in the query string
            /// that may break the limits of the query parser.
            /// </remarks>
            /// <param name="rockContext">The rock context.</param>
            /// <param name="personAliasIdList">A collection of person alias IDs to persist in the entity set.</param>
            /// <returns>An <see cref="IQueryable{T}"/> of person alias IDs representing the persisted entity set, or <see langword="null"/> if the list is empty.</returns>
            public IQueryable<PersonAlias> LoadPersonAliasEntitySet( RockContext rockContext, IEnumerable<int> personAliasIdList )
            {
                if ( personAliasIdList == null
                     || !personAliasIdList.Any() )
                {
                    return null;
                }

                var service = new EntitySetService( rockContext );

                using ( var activity = ObservabilityHelper.StartActivity( "COMMUNICATION: Entry Wizard > Get Recipient Person Alias Id Persisted List (add new EntitySet)" ) )
                {
                    var args = new AddEntitySetActionOptions
                    {
                        Name = "RecipientPersonAliasEntitySet_Communication",
                        EntityTypeId = EntityTypeCache.Get<PersonAlias>().Id,
                        EntityIdList = personAliasIdList,
                        ExpiryInMinutes = 20
                    };
                    var entitySetId = service.AddEntitySet( args );

                    activity?.AddTag( "rock.communication.entity_set_id", entitySetId );

                    return service.GetEntityQuery<PersonAlias>( entitySetId );
                }
            }

            #region CommunicationOperationsService Helper Classes

            /// <summary>
            /// Represents the settings required to create or update a communication.
            /// </summary>
            public class CreateOrUpdateCommunicationInfo
            {
                /// <summary>
                /// Gets or sets the communication ID if updating an existing communication; otherwise, <see langword="null"/> for a new communication.
                /// </summary>
                public int? CommunicationId { get; set; }

                /// <summary>
                /// Gets or sets the unique identifier for the communication.
                /// </summary>
                public Guid CommunicationGuid { get; set; }

                /// <summary>
                /// Gets or sets the sender's person alias ID.
                /// </summary>
                public int? SenderPersonAliasId { get; set; }

                /// <summary>
                /// Gets or sets the enabled Lava commands for processing the communication content.
                /// </summary>
                public string EnabledLavaCommands { get; set; }

                /// <summary>
                /// Gets or sets the name of the communication.
                /// </summary>
                public string CommunicationName { get; set; }

                /// <summary>
                /// Gets or sets a value indicating whether the communication is a bulk message.
                /// </summary>
                public bool IsBulkCommunication { get; set; }

                /// <summary>
                /// Gets or sets the type of communication (Email, SMS, Push, etc.).
                /// </summary>
                public CommunicationType CommunicationType { get; set; }

                /// <summary>
                /// Gets or sets the GUID of the communication list group if using a group-based recipient selection.
                /// </summary>
                public Guid? CommunicationListGroupGuid { get; set; }

                /// <summary>
                /// Gets or sets a value indicating whether duplicate recipient addresses should be excluded.
                /// </summary>
                public bool ExcludeDuplicateRecipientAddress { get; set; }

                /// <summary>
                /// Gets or sets the list of personalization segment ids used for filtering communication recipients.
                /// </summary>
                public List<int> PersonalizationSegmentIds { get; set; }

                /// <summary>
                /// Gets or sets the criteria for segmenting the communication group.
                /// </summary>
                public SegmentCriteria CommunicationGroupSegmentCriteria { get; set; }

                /// <summary>
                /// Gets or sets the GUID of the communication template to be applied, if any.
                /// </summary>
                public Guid? CommunicationTemplateGuid { get; set; }

                /// <summary>
                /// Gets or sets a list of binary file IDs representing email attachments.
                /// </summary>
                public List<int> EmailBinaryFileIds { get; set; }

                /// <summary>
                /// Gets or sets a list of binary file IDs representing SMS attachments.
                /// </summary>
                public List<int> SmsBinaryFileIds { get; set; }

                /// <summary>
                /// Gets or sets the scheduled send date and time for the communication, if applicable.
                /// </summary>
                public DateTime? FutureSendDateTime { get; set; }

                /// <summary>
                /// Gets or sets the communication details, including subject, message, and sender information.
                /// </summary>
                public CommunicationDetails Details { get; set; } = new CommunicationDetails();

                /// <summary>
                /// Gets or sets the ID of the communication topic value, if applicable.
                /// </summary>
                public int? CommunicationTopicValueId { get; set; }
            }

            #endregion CommunicationOperationsService Helper Classes
        }

        #endregion Service Classes
    }
}