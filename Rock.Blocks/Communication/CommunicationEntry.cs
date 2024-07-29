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

using Rock;
using Rock.Attribute;
using Rock.Cms.StructuredContent;
using Rock.Communication;
using Rock.Data;
using Rock.Enums.Blocks.Communication.CommunicationEntry;
using Rock.Model;
using Rock.Net;
using Rock.Security;
using Rock.Tasks;
using Rock.Utility;
using Rock.ViewModels.Blocks.Communication.CommunicationEntry;
using Rock.ViewModels.Utility;
using Rock.ViewModels.Rest.Controls;
using Rock.Web.Cache;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// User control for creating a new communication.  This block should be used on same page as the CommunicationDetail block and only visible when editing a new or transient communication
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockBlockType" />
    [DisplayName( "Communication Entry" )]
    [Category( "Communication" )]
    [Description( "Used for creating and sending a new communications such as email, SMS, etc. to recipients." )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [SecurityAction( Authorization.APPROVE, "The roles and/or users that have access to approve new communications." )]

    [BooleanField( "Enable Lava",
        Key = AttributeKey.EnableLava,
        Description = "When enabled, allows lava in the message. When disabled, lava is removed from the message without resolving it.",
        DefaultBooleanValue = false,
        IsRequired = true,
        Order = 0 )]

    [LavaCommandsField( "Enabled Lava Commands",
        Key = AttributeKey.EnabledLavaCommands,
        Description = "The Lava commands that should be enabled for this block if Enable Lava is checked.",
        IsRequired = false,
        Order = 1 )]

    [BooleanField( "Enable Person Parameter",
        Key = AttributeKey.EnablePersonParameter,
        Description = "When enabled, allows passing a 'Person' or 'PersonId' querystring parameter with a person Id to the block to create a communication for that person.",
        DefaultBooleanValue = false,
        IsRequired = false,
        Order = 2 )]

    [ComponentsField( "Rock.Communication.MediumContainer, Rock",
        Name = "Mediums",
        Key = AttributeKey.Mediums,
        Description = "The Mediums that should be available to user to send through (If none are selected, all active mediums will be available).",
        IsRequired = false,
        Order = 3 )]

    [CommunicationTemplateField( "Default Template",
        Key = AttributeKey.DefaultTemplate,
        Description = "The default template to use for a new communication. (Note: This will only be used if the template is for the same medium as the communication.)",
        IsRequired = false,
        Order = 4 )]

    [IntegerField( "Maximum Recipients",
        Key = AttributeKey.MaximumRecipients,
        Description = "The maximum number of recipients allowed before communication will need to be approved.",
        IsRequired = false,
        DefaultIntegerValue = 0,
        Order = 5 )]

    [BooleanField( "Send When Approved",
        Key = AttributeKey.SendWhenApproved,
        Description = "Should communication be sent once it's approved (vs. just being queued for scheduled job to send)?",
        DefaultBooleanValue = true,
        Order = 6 )]

    [CustomDropdownListField( "Mode",
        "The mode to use ('Simple' mode will prevent users from searching/adding new people to communication).",
        "Full,Simple",
        Key = AttributeKey.Mode,
        IsRequired = true,
        DefaultValue = "Full",
        Order = 7 )]

    [BooleanField( "Allow CC/Bcc",
        Key = AttributeKey.AllowCcBcc,
        Description = "Allow CC and BCC addresses to be entered for email communications?",
        DefaultBooleanValue = false,
        Order = 8 )]

    [BooleanField( "Show Attachment Uploader",
        Key = AttributeKey.ShowAttachmentUploader,
        Description = "Should the attachment uploader be shown for email communications?",
        DefaultBooleanValue = true,
        Order = 9 )]

    [SystemPhoneNumberField( "Allowed SMS Numbers",
        Key = AttributeKey.AllowedSMSNumbers,
        Description = "Set the allowed FROM numbers to appear when in SMS mode (if none are selected all numbers will be included).",
        IsRequired = false,
        AllowMultiple = true,
        Order = 10 )]

    [BooleanField( "Simple Communications Are Bulk",
        Key = AttributeKey.SendSimpleAsBulk,
        Description = "Should simple mode communications be sent as a bulk communication?",
        DefaultBooleanValue = true,
        Order = 11 )]

    [BinaryFileTypeField( "Attachment Binary File Type",
        Key = AttributeKey.AttachmentBinaryFileType,
        Description = "The FileType to use for files that are attached to an sms or email communication.",
        IsRequired = true,
        DefaultBinaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.COMMUNICATION_ATTACHMENT,
        Order = 12 )]

    [BooleanField( "Default As Bulk",
        Key = AttributeKey.DefaultAsBulk,
        Description = "Should new entries be flagged as bulk communication by default?",
        DefaultBooleanValue = false,
        Order = 13 )]

    [BooleanField( "Show Email Metrics Reminder Options",
        Key = AttributeKey.ShowEmailMetricsReminderOptions,
        Description = "Should the email metrics reminder options be shown after a communication is sent?",
        DefaultBooleanValue = false,
        Order = 14 )]

    [TextField( "Document Root Folder",
        Key = AttributeKey.DocumentRootFolder,
        Description = "The folder to use as the root when browsing or uploading documents.",
        IsRequired = false,
        DefaultValue = "~/Content",
        Category = AttributeCategory.HtmlEditorSettings,
        Order = 15 )]

    [TextField( "Image Root Folder",
        Key = AttributeKey.ImageRootFolder,
        Description = "The folder to use as the root when browsing or uploading images.",
        IsRequired = false,
        DefaultValue = "~/Content",
        Category = AttributeCategory.HtmlEditorSettings,
        Order = 16 )]

    [BooleanField( "User Specific Folders",
        Key = AttributeKey.UserSpecificFolders,
        Description = "Should the root folders be specific to current user?",
        DefaultBooleanValue = false,
        Category = AttributeCategory.HtmlEditorSettings,
        Order = 17 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "26C0C9A1-1383-48D5-A062-E05622A1CBF2" )]
    [Rock.SystemGuid.BlockTypeGuid( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3" )]
    public class CommunicationEntry : RockBlockType
    {
        #region Categories

        private static class AttributeCategory
        {
            public const string HtmlEditorSettings = "HTML Editor Settings";
        }

        #endregion

        #region Keys

        /// <summary>
        /// Keys to use for Block Attributes.
        /// </summary>
        private static class AttributeKey
        {
            public const string AllowCcBcc = "AllowCcBcc";
            public const string AttachmentBinaryFileType = "AttachmentBinaryFileType";
            public const string EnabledLavaCommands = "EnabledLavaCommands";
            public const string MaximumRecipients = "MaximumRecipients";
            public const string SendWhenApproved = "SendWhenApproved";
            public const string AllowedSMSNumbers = "AllowedSMSNumbers";
            public const string SendSimpleAsBulk = "IsBulk";
            public const string ImageRootFolder = "ImageRootFolder";
            public const string DocumentRootFolder = "DocumentRootFolder";
            public const string Mode = "Mode";
            public const string UserSpecificFolders = "UserSpecificFolders";
            public const string DefaultAsBulk = "DefaultAsBulk";
            public const string ShowAttachmentUploader = "ShowAttachmentUploader";
            public const string Mediums = "Mediums";
            public const string DefaultTemplate = "DefaultTemplate";
            public const string EnableLava = "EnableLava";
            public const string EnablePersonParameter = "EnablePersonParameter";
            public const string ShowEmailMetricsReminderOptions = "ShowEmailMetricsReminderOptions";
        }

        /// <summary>
        /// Keys to use for SMS Medium Attributes.
        /// </summary>
        private static class SmsMediumAttributeKey
        {
            public const string CharacterLimit = "CharacterLimit";
        }
        
        /// <summary>
        /// Keys to use for Page parameters.
        /// </summary>
        private static class PageParameterKey
        {
            public const string CommunicationId = "CommunicationId";
            public const string Edit = "Edit";
            public const string Person = "Person";
            public const string PersonId = "PersonId";
            public const string TemplateGuid = "TemplateGuid";
            public const string MediumId = "MediumId";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the mode to use ( 'Simple' mode will prevent users from searching/adding new people to communication).
        /// </summary>
        private Mode Mode
        {
            get
            {
                var mode = GetAttributeValue( AttributeKey.Mode );

                // Full mode when mode is empty or when mode == "Full".
                if ( mode.IsNullOrWhiteSpace() || mode.Equals( "Full", StringComparison.OrdinalIgnoreCase ) )
                {
                    return Mode.Full;
                }
                else
                {
                    return Mode.Simple;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether lava is supported.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if lava in the message should be resolved; otherwise, <see langword="false"/> if lava should be removed from the message without resolving it.
        /// </value>
        private bool IsLavaEnabled => GetAttributeValue( AttributeKey.EnableLava ).AsBoolean();

        /// <summary>
        /// Gets the maximum number of recipients allowed before communication will need to be approved.
        /// </summary>
        private int MaximumRecipients => GetAttributeValue( AttributeKey.MaximumRecipients ).AsInteger();

        /// <summary>
        /// Gets a value indicating whether new communications should be flagged as bulk communications.
        /// </summary>
        private bool DefaultAsBulk => GetAttributeValue( AttributeKey.DefaultAsBulk ).AsBoolean();

        /// <summary>
        /// Should simple mode communications be sent as a bulk communication?
        /// </summary>
        private bool IsSendSimpleAsBulkEnabled => GetAttributeValue( AttributeKey.SendSimpleAsBulk ).AsBoolean();

        /// <summary>
        /// When enabled, allows passing a 'Person' or 'PersonId' querystring parameter with a person Id to the block to create a communication for that person.
        /// </summary>
        private bool IsPersonPageParameterEnabled => GetAttributeValue( AttributeKey.EnablePersonParameter ).AsBoolean();

        /// <summary>
        /// Gets the Mediums that should be available to user to send through (If none are selected, all active mediums will be available).
        /// </summary>
        private List<Guid> DisplayedMediumGuids => GetAttributeValue( AttributeKey.Mediums ).SplitDelimitedValues().AsGuidList();

        /// <summary>
        /// Gets the default template to use for a new communication.
        /// </summary>
        /// <remarks>
        /// (Note: This will only be used if the template is for the same medium as the communication.)
        /// </remarks>
        private Guid? DefaultTemplateGuid => GetAttributeValue( AttributeKey.DefaultTemplate ).AsGuidOrNull();

        /// <summary>
        /// Gets the Lava commands that should be enabled for this HTML block if Enable Lava is checked.
        /// </summary>
        private string EnabledLavaCommands => GetAttributeValue( AttributeKey.EnabledLavaCommands );

        /// <summary>
        /// Should communication be sent once it's approved (vs. just being queued for scheduled job to send)?
        /// </summary>
        private bool IsSendWhenApprovedEnabled => GetAttributeValue( AttributeKey.SendWhenApproved ).AsBoolean();

        /// <summary>
        /// Gets the FileType to use for files that are attached to an sms or email communication.
        /// </summary>
        private Guid AttachmentBinaryFileTypeGuid => GetAttributeValue( AttributeKey.AttachmentBinaryFileType ).AsGuidOrNull() ?? Rock.SystemGuid.BinaryFiletype.COMMUNICATION_ATTACHMENT.AsGuid();

        /// <summary>
        /// Gets the allowed FROM numbers that appear when in SMS mode (if none are selected all numbers will be included).
        /// </summary>
        private List<Guid> AllowedSmsNumbers => GetAttributeValue( AttributeKey.AllowedSMSNumbers ).SplitDelimitedValues( whitespace: true ).AsGuidList();

        /// <summary>
        /// Gets the folder to use as the root when browsing or uploading documents.
        /// </summary>
        private string DocumentRootFolder => GetAttributeValue( AttributeKey.DocumentRootFolder );

        /// <summary>
        /// Gets the folder to use as the root when browsing or uploading images.
        /// </summary>
        private string ImageFolderRoot => GetAttributeValue( AttributeKey.ImageRootFolder );

        /// <summary>
        ///Should the attachment uploader be shown for email communications?
        /// </summary>
        private bool IsAttachmentUploaderShown => GetAttributeValue( AttributeKey.ShowAttachmentUploader ).AsBoolean();

        /// <summary>
        /// Should the root folders be specific to current user?
        /// </summary>
        private bool IsUserSpecificRoot => GetAttributeValue( AttributeKey.UserSpecificFolders ).AsBoolean();

        /// <summary>
        /// Allow CC and Bcc addresses to be entered for email communications?
        /// </summary>
        private bool IsCcBccEntryAllowed => GetAttributeValue( AttributeKey.AllowCcBcc ).AsBoolean();

        /// <summary>
        /// Should the email metrics reminder options be shown after a communication is sent?
        /// </summary>
        private bool AreEmailMetricsReminderOptionsShown => GetAttributeValue( AttributeKey.ShowEmailMetricsReminderOptions ).AsBoolean();

        /// <summary>
        /// Gets the Edit page parameter indicating whether the block should be in edit mode.
        /// </summary>
        private bool EditPageParameter => PageParameter( PageParameterKey.Edit ).AsBoolean();

        /// <summary>
        /// 'Person' or 'PersonId' querystring parameter with a person Id to the block to create a communication for that person.
        /// </summary>
        private int? PersonOrPersonIdPageParameter => PageParameter( PageParameterKey.Person ).AsIntegerOrNull() ?? PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull();

        /// <summary>
        /// Gets the CommunicationId page parameter.
        /// </summary>
        private int? CommunicationIdPageParameter => PageParameter( PageParameterKey.CommunicationId ).AsIntegerOrNull();

        /// <summary>
        /// Gets the MediumId page parameter.
        /// </summary>
        private int? MediumIdPageParameter => PageParameter( PageParameterKey.MediumId ).AsIntegerOrNull();

        /// <summary>
        /// Gets the TemplateGuid page parameter.
        /// </summary>
        private Guid? TemplateGuidPageParameter => PageParameter( PageParameterKey.TemplateGuid ).AsGuidOrNull();

        #endregion

        #region Base Control Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var currentPerson = GetCurrentPerson();
                var communication = LoadCommunication( rockContext );
                var authorization = GetAuthorization( currentPerson, communication );

                var box = new CommunicationEntryInitializationBox();

                if ( authorization.CanViewBlock )
                {
                    // Communication is either new or can be editted.

                    box.AreEmailMetricsReminderOptionsShown = this.AreEmailMetricsReminderOptionsShown;
                    box.Authorization = authorization;
                    box.IsCcBccEntryAllowed = this.IsCcBccEntryAllowed;
                    box.IsHidden = false;
                    box.IsEditMode = this.EditPageParameter;
                    box.IsLavaEnabled = this.IsLavaEnabled;
                    box.MaximumRecipientsBeforeApprovalRequired = this.MaximumRecipients;
                    box.Mediums = GetMediums( currentPerson );
                    box.Mode = this.Mode;
                    box.Title = GetTitle( communication );

                    var communicationData = GetCommunicationData( rockContext, communication, currentPerson, box.Mediums.Select( m => m.Value.AsGuid() ) );
                    box.Communication = communicationData.Communication;
                    box.MediumOptions = communicationData.MediumOptions;
                }
                else
                {
                    // The person does not have authorization to view the block.
                    // If there is a CommunicationDetail block on this page, it'll be shown instead.
                    box.IsHidden = true;
                }

                return box;
            }
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the options for a specific medium.
        /// </summary>
        /// <param name="mediumEntityTypeGuid">The medium entity type unique identifier.</param>
        [BlockAction( "GetMediumOptions" )]
        public BlockActionResult GetMediumOptions( Guid mediumEntityTypeGuid )
        {
            if ( !mediumEntityTypeGuid.Validate( "Medium Type" ).IsNotEmpty( out var validationResult ) )
            {
                return ActionBadRequest( validationResult.ErrorMessage );
            }

            var mediumOptions = GetMediumOptions( mediumEntityTypeGuid, GetCurrentPerson() );
            return ActionOk( mediumOptions );
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

            using ( var rockContext = new RockContext() )
            {
                var recipient = GetRecipientBags(
                    rockContext,
                    new RecipientQueryOptions
                    {
                        PersonAliasGuids = new[] { personAliasGuid },
                        Limit = 1
                    } ).FirstOrDefault();

                if ( recipient != null )
                {
                    return ActionOk( recipient );
                }
                else
                {
                    return ActionNotFound();
                }
            }
        }

        /// <summary>
        /// Gets the recipient data for multiple people.
        /// </summary>
        /// <param name="bag">The bag containing the information to retrieve the recipient data.</param>
        [BlockAction( "GetRecipients" )]
        public BlockActionResult GetRecipients( CommunicationEntryGetRecipientsRequestBag bag )
        {
            if ( !bag.Validate( "Recipient Information" ).IsNotNull( out var validationResult )
                 || !bag.PersonAliasGuids.Validate( "People" ).IsNotEmpty( out validationResult ) )
            {
                return ActionBadRequest( validationResult.ErrorMessage );
            }

            using ( var rockContext = new RockContext() )
            {
                var recipients = GetRecipientBags(
                    rockContext,
                    new RecipientQueryOptions
                    {
                        PersonAliasGuids = bag.PersonAliasGuids
                    } );

                return ActionOk( recipients );
            }
        }

        /// <summary>
        /// Gets template data.
        /// </summary>
        /// <param name="templateGuid">The template unique identifier.</param>
        [BlockAction( "GetTemplate" )]
        public BlockActionResult GetTemplate( Guid templateGuid )
        {
            if ( !templateGuid.Validate( "Template" ).IsNotEmpty( out var validationResult ) )
            {
                return ActionBadRequest( validationResult.ErrorMessage );
            }

            using ( var rockContext = new RockContext() )
            {
                var template = new CommunicationTemplateService( rockContext ).Get( templateGuid );

                // Copy the template to the bag.
                var bag = new CommunicationEntryCommunicationBag();
                var copyTarget = new CommunicationDetailsAdapter( bag, rockContext );
                CommunicationEntryHelper.CopyTemplate( template, copyTarget, this.RequestContext );

                return ActionOk( bag );
            }
        }

        /// <summary>
        /// Saves the communication.
        /// </summary>
        [BlockAction( "Save" )]
        public BlockActionResult Save( CommunicationEntrySaveRequestBag bag )
        {
            if ( !IsSaveRequestValid( bag, out var validationResult ) )
            {
                return ActionBadRequest( validationResult.ErrorMessage );
            }

            using ( var rockContext = new RockContext() )
            {
                var communication = UpdateCommunication( rockContext, bag );

                if ( communication == null )
                {
                    // This should not happen, but adding just in case.
                    return ActionBadRequest( "An error occurred while saving. Please try again." );
                }
                else
                {
                    var mediumDataService = GetMediumDataService( bag.MediumEntityTypeGuid );
                    if ( mediumDataService != null )
                    {
                        mediumDataService.OnCommunicationSave( rockContext, bag );
                    }

                    communication.Status = CommunicationStatus.Draft;
                    rockContext.SaveChanges();

                    var responseBag = new CommunicationEntrySendResponseBag
                    {
                        CommunicationGuid = communication.Guid,
                        CommunicationStatus = communication.Status,
                        CommunicationId = communication.Id,
                        HasDetailBlockOnCurrentPage = this.PageCache.Blocks.Any( a => a.BlockType.Guid == Rock.SystemGuid.BlockType.COMMUNICATION_DETAIL.AsGuid() ),
                        Message = "The communication has been saved",
                        RedirectToViewMode = false,
                    };

                    return ActionOk( responseBag );
                }
            }
        }

        /// <summary>
        /// Tests the communication by sending it to the currently logged in person.
        /// </summary>
        [BlockAction( "Test" )]
        public BlockActionResult Test( CommunicationEntryTestRequestBag bag )
        {
            if ( !IsTestRequestValid( bag, out var validationResult ) )
            {
                return ActionBadRequest( validationResult.ErrorMessage );
            }

            var currentPerson = GetCurrentPerson();
            var primaryAliasId = currentPerson?.PrimaryAliasId;

            if ( !primaryAliasId.HasValue )
            {
                return ActionBadRequest( "You must be authenticated to send a test communication." );
            }

            // Get existing or new communication record.
            // Use a separate context so that changes in UpdateCommunication() are not persisted.
            var communication = UpdateCommunication( new RockContext(), bag );

            if ( communication == null )
            {
                return ActionBadRequest( "Unable to send test communication." );
            }

            using ( var rockContext = new RockContext() )
            {
                var testCommunication = communication.CloneWithoutIdentity();
                testCommunication.CreatedByPersonAliasId = primaryAliasId;
                testCommunication.CreatedByPersonAlias = new PersonAliasService( rockContext )
                    .Queryable()
                    .Where( a => a.Id == primaryAliasId.Value )
                    .Include( a => a.Person )
                    .FirstOrDefault();
                testCommunication.EnabledLavaCommands = this.EnabledLavaCommands;
                testCommunication.FutureSendDateTime = null;
                testCommunication.Status = CommunicationStatus.Approved;
                testCommunication.ReviewedDateTime = RockDateTime.Now;
                testCommunication.ReviewerPersonAliasId = primaryAliasId;
                testCommunication.Subject = string.Format( "[Test] {0}", testCommunication.Subject );

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

                var testRecipient = new CommunicationRecipient();
                if ( communication.Recipients.Any() )
                {
                    var recipient = communication.Recipients.FirstOrDefault();
                    testRecipient.AdditionalMergeValuesJson = recipient.AdditionalMergeValuesJson;
                }

                testRecipient.Status = CommunicationRecipientStatus.Pending;
                testRecipient.PersonAliasId = primaryAliasId.Value;
                testRecipient.MediumEntityTypeId = EntityTypeCache.GetId( bag.MediumEntityTypeGuid );
                testCommunication.Recipients.Add( testRecipient );

                var communicationService = new CommunicationService( rockContext );
                communicationService.Add( testCommunication );
                rockContext.SaveChanges();

                foreach ( var medium in testCommunication.GetMediums() )
                {
                    medium.Send( testCommunication );
                }

                testRecipient = new CommunicationRecipientService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( r => r.CommunicationId == testCommunication.Id )
                    .FirstOrDefault();

                var response = new CommunicationEntryTestResponseBag();
                if ( testRecipient != null && testRecipient.Status == CommunicationRecipientStatus.Failed && testRecipient.PersonAlias != null && testRecipient.PersonAlias.Person != null )
                {
                    response.MessageType = "danger";
                    response.MessageHtml = $"Test communication to <strong>{testRecipient.PersonAlias.Person.FullName}</strong> failed: {testRecipient.StatusNote}.";
                }
                else
                {
                    response.MessageType = "success";
                    response.MessageHtml = "Test communication has been sent.";
                }

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

                rockContext.SaveChanges();

                return ActionOk( response );
            }
        }

        /// <summary>
        /// Sends the communication.
        /// </summary>
        [BlockAction( "Send" )]
        public BlockActionResult Send( CommunicationEntrySendRequestBag bag )
        {
            if ( !IsSendRequestValid( bag, out var validationResult ) )
            {
                return ActionBadRequest( validationResult.ErrorMessage );
            }

            using ( var rockContext = new RockContext() )
            {
                var communication = UpdateCommunication( rockContext, bag );

                if ( communication == null )
                {
                    // This should not be able to happen but adding just in case.
                    return ActionBadRequest( "Communication failed to save. Please try again." );
                }

                var mediumBehavior = GetMediumDataService( bag.MediumEntityTypeGuid );
                if ( mediumBehavior != null )
                {
                    mediumBehavior.OnCommunicationSave( rockContext, bag );
                }

                var currentPerson = GetCurrentPerson();
                var authorization = GetAuthorization( currentPerson, communication );
                if ( communication.Status == CommunicationStatus.PendingApproval && this.EditPageParameter && authorization.IsBlockApproveActionAuthorized )
                {
                    rockContext.SaveChanges();

                    return ActionOk( new CommunicationEntrySendResponseBag
                    {
                        RedirectToViewMode = true,
                    } );
                }

                var responseMessage = string.Empty;

                // Save the communication as a draft prior to checking recipients.
                communication.Status = CommunicationStatus.Draft;
                rockContext.SaveChanges();

                if ( communication.Recipients.Count() > this.MaximumRecipients && !authorization.IsBlockApproveActionAuthorized )
                {
                    // Change the status to pending approval as the current person is not authorized to approve the communication.
                    communication.Status = CommunicationStatus.PendingApproval;
                    responseMessage = "Communication has been submitted for approval.";
                }
                else
                {
                    // Approval is not required or the current person can approve.
                    communication.Status = CommunicationStatus.Approved;
                    communication.ReviewedDateTime = RockDateTime.Now;
                    communication.ReviewerPersonAliasId = currentPerson.PrimaryAliasId;

                    if ( communication.FutureSendDateTime.HasValue
                            && communication.FutureSendDateTime > RockDateTime.Now )
                    {
                        responseMessage = $"Communication will be sent {communication.FutureSendDateTime.Value.ToRelativeDateString( 0 )}.";
                    }
                    else
                    {
                        responseMessage = "Communication has been queued for sending.";
                    }
                }

                rockContext.SaveChanges();

                // Send approval email if needed (now that we have a communication id).
                if ( communication.Status == CommunicationStatus.PendingApproval )
                {
                    var approvalTransactionMsg = new ProcessSendCommunicationApprovalEmail.Message
                    {
                        CommunicationId = communication.Id
                    };
                    approvalTransactionMsg.Send();
                }

                if ( communication.Status == CommunicationStatus.Approved
                     && ( !communication.FutureSendDateTime.HasValue || communication.FutureSendDateTime.Value <= RockDateTime.Now ) )
                {
                    if ( this.IsSendWhenApprovedEnabled )
                    {
                        var transactionMsg = new ProcessSendCommunication.Message
                        {
                            CommunicationId = communication.Id
                        };
                        transactionMsg.Send();
                    }
                }

                return ActionOk( new CommunicationEntrySendResponseBag
                {
                    CommunicationStatus = communication.Status,
                    Message = responseMessage,
                    RedirectToViewMode = false,
                    CommunicationId = communication.Id,
                    CommunicationGuid = communication.Guid,
                    HasDetailBlockOnCurrentPage = this.PageCache.Blocks.Any( a => a.BlockType.Guid == Rock.SystemGuid.BlockType.COMMUNICATION_DETAIL.AsGuid() ),
                } );
            }
        }

        /// <summary>
        /// Saves a metrics reminder communication.
        /// </summary>
        /// <param name="bag"></param>
        /// <returns></returns>
        [BlockAction( "SaveMetricsReminder" )]
        public BlockActionResult SaveMetricsReminder( CommunicationEntrySaveMetricsReminderRequestBag bag )
        {
            if ( !IsSaveMetricsReminderRequestValid( bag, out var validationResult ) )
            {
                return ActionBadRequest( validationResult.ErrorMessage );
            }

            using ( var rockContext = new RockContext() )
            {
                var communicationService = new CommunicationService( rockContext );
                var communication = communicationService.Get( bag.CommunicationGuid );

                if ( communication == null )
                {
                    return ActionNotFound();
                }

                if ( !communication.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) )
                {
                    return ActionUnauthorized();
                }

                communication.EmailMetricsReminderOffsetDays = bag.DaysUntilReminder;

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Cancels a metrics reminder communication.
        /// </summary>
        /// <param name="communicationGuid"></param>
        /// <returns></returns>
        [BlockAction( "CancelMetricsReminder" )]
        public BlockActionResult CancelMetricsReminder( Guid communicationGuid )
        {
            if ( !IsCancelMetricsReminderRequestValid( communicationGuid, out var validationResult ) )
            {
                return ActionBadRequest( validationResult.ErrorMessage );
            }

            using ( var rockContext = new RockContext() )
            {
                var communicationService = new CommunicationService( rockContext );
                var communication = communicationService.Get( communicationGuid );

                if ( communication == null )
                {
                    return ActionNotFound();
                }

                if ( !communication.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) )
                {
                    return ActionUnauthorized();
                }

                communication.EmailMetricsReminderOffsetDays = null;

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads the communication based on the CommunicationId page parameter.
        /// </summary>
        /// <param name="rockContext">The Rock context.</param>
        /// <returns>The loaded <see cref="Model.Communication"/> object or <see langword="null"/> if the communication doesn't exist or a new one is being created.</returns>
        private Model.Communication LoadCommunication( RockContext rockContext )
        {
            // Check page parameter for existing communication.
            Model.Communication communication = null;
            var communicationId = this.CommunicationIdPageParameter;
            if ( communicationId.HasValue )
            {
                communication = new CommunicationService( rockContext ).Get( communicationId.Value );
                communication?.GetAttachments( communication.CommunicationType );
            }

            return communication;
        }

        /// <summary>
        /// The Mediums that should be available to user to send through (If none are selected, all active mediums will be available).
        /// </summary>
        private List<ListItemBag> GetMediums( Person currentPerson )
        {
            var displayedMediumGuids = this.DisplayedMediumGuids;
            var mediums = new List<(string ComponentName, MediumComponent Medium)>();
            foreach ( var item in MediumContainer.Instance.Components.Values )
            {
                var mediumComponent = item.Value;
                if ( ( !displayedMediumGuids.Any() || displayedMediumGuids.Contains( mediumComponent.EntityType.Guid ) )
                        && mediumComponent.IsActive
                        && mediumComponent.IsAuthorized( Authorization.VIEW, currentPerson ) )
                {
                    mediums.Add( (item.Metadata.ComponentName, mediumComponent) );
                }
            }

            return mediums
                .Select( medium => new ListItemBag
                {
                    Text = medium.ComponentName,
                    Value = medium.Medium.EntityType.Guid.ToString()
                } )
                .ToList();
        }

        /// <summary>
        /// Gets the authorization for the current person and communication.
        /// </summary>
        /// <param name="currentPerson">The logged in person.</param>
        /// <param name="communication">The communication to authorize. <see langword="null"/> is allowed here and represents a brand new communication.</param>
        /// <returns></returns>
        private CommunicationEntryAuthorizationBag GetAuthorization( Person currentPerson, Model.Communication communication )
        {
            if ( communication == null )
            {
                // If this is a new communication, create a communication object temporarily so we can do the auth and edit logic.
                communication = new Rock.Model.Communication
                {
                    CreatedByPersonAlias = currentPerson.PrimaryAlias,
                    CreatedByPersonAliasId = currentPerson.PrimaryAliasId,
                    SenderPersonAlias = currentPerson.PrimaryAlias,
                    SenderPersonAliasId = currentPerson.PrimaryAliasId,
                    Status = CommunicationStatus.Transient,
                };
            }

            var isBlockApproveActionAuthorized = this.BlockCache.IsAuthorized( Authorization.APPROVE, currentPerson );
            var isBlockEditActionAuthorized = this.BlockCache.IsAuthorized( Authorization.EDIT, currentPerson );
            var isCommunicationEditActionAuthorized = communication.IsAuthorized( Authorization.EDIT, currentPerson );
            var isCommunicationCreator = communication.CreatedByPersonAlias != null && communication.CreatedByPersonAlias.PersonId == currentPerson.Id;
            var isEditableStatus =
                communication.Status == CommunicationStatus.Transient
                || communication.Status == CommunicationStatus.Draft
                || communication.Status == CommunicationStatus.Denied;

            return new CommunicationEntryAuthorizationBag
            {
                IsBlockApproveActionAuthorized = isBlockApproveActionAuthorized,
                IsBlockEditActionAuthorized = isBlockEditActionAuthorized,
                IsCommunicationEditActionAuthorized = isCommunicationEditActionAuthorized,
                CanViewBlock = ( communication.Status == CommunicationStatus.PendingApproval && this.EditPageParameter && isBlockApproveActionAuthorized )
                    || (isEditableStatus && ( isCommunicationEditActionAuthorized || isCommunicationCreator ) ),
            };
        }

        /// <summary>
        /// Gets a medium component for a medium unique identifier.
        /// </summary>
        /// <param name="mediumGuid">The medium unique identifier.</param>
        private (string ComponentName, MediumComponent Medium) GetMediumComponent( Guid mediumGuid )
        {
            var mediumData = MediumContainer.Instance
                .Components
                .Where( kvp => kvp.Value.Value.TypeGuid == mediumGuid )
                .Select( kvp => (kvp.Value.Metadata.ComponentName, kvp.Value.Value) )
                .FirstOrDefault();

            if ( mediumData == default )
            {
                return (null, null);
            }

            return mediumData;
        }

        /// <summary>
        /// Gets the medium options for a medium unique identifier and sender.
        /// </summary>
        /// <param name="mediumGuid">The medium unique identifier.</param>
        /// <param name="sender">The person sending the communication.</param>
        private CommunicationEntryMediumOptionsBaseBag GetMediumOptions( Guid mediumGuid, Person sender )
        {
            if ( mediumGuid.IsEmpty() )
            {
                // No medium is selected so nothing to set.
                return CommunicationEntryMediumOptionsBaseBag.Unknown;
            }

            var (_, medium) = GetMediumComponent( mediumGuid );

            if ( medium == null )
            {
                return CommunicationEntryMediumOptionsBaseBag.Unknown;
            }

            if ( medium is Rock.Communication.Medium.Email emailMedium )
            {
                using ( var rockContext = new RockContext() )
                {
                    var communication = LoadCommunication( rockContext );
                    return new CommunicationEntryEmailMediumOptionsBag
                    {
                        AdditionalMergeFields = communication?.AdditionalMergeFields,
                        BinaryFileTypeGuid = this.AttachmentBinaryFileTypeGuid,
                        BulkEmailThreshold = emailMedium.GetBulkEmailThreshold(),
                        DocumentFolderRoot = this.DocumentRootFolder,
                        FromName = sender?.FullName,
                        FromAddress = sender?.Email,
                        HasActiveTransport = emailMedium.Transport?.IsActive ?? false,
                        ImageFolderRoot = this.ImageFolderRoot,
                        IsAttachmentUploaderShown = this.IsAttachmentUploaderShown,
                        IsUserSpecificRoot = this.IsUserSpecificRoot,
                        MediumEntityTypeId = medium.EntityType.Id,
                        Templates = GetCommunicationTemplates( emailMedium, rockContext ).ToListItemBagList(),
                        MediumEntityTypeGuid = mediumGuid,
                    };
                }
            }
            else if ( medium is Rock.Communication.Medium.Sms smsMedium )
            {
                var currentPerson = GetCurrentPerson();
                var allowedSmsFromNumberGuids = this.AllowedSmsNumbers;
                return new CommunicationEntrySmsMediumOptionsBag
                {
                    BinaryFileTypeGuid = this.AttachmentBinaryFileTypeGuid,
                    CharacterLimit = smsMedium.GetAttributeValue( SmsMediumAttributeKey.CharacterLimit ).AsIntegerOrNull() ?? 160,
                    HasActiveTransport = smsMedium.Transport?.IsActive ?? false,
                    MediumEntityTypeId = medium.EntityType.Id,
                    MediumEntityTypeGuid = mediumGuid,
                    SmsFromNumbers = SystemPhoneNumberCache
                        .All( includeInactive: false )
                        .Where( spn => spn.IsAuthorized( Authorization.VIEW, currentPerson ) && allowedSmsFromNumberGuids.ContainsOrEmpty( spn.Guid ) )
                        .OrderBy( spn => spn.Order )
                        .ThenBy( spn => spn.Name )
                        .ThenBy( spn => spn.Id )
                        .ToListItemBagList(),
                };
            }
            else if ( medium is Rock.Communication.Medium.PushNotification pushMedium )
            {
                using ( var rockContext = new RockContext() )
                {
                    var mobileApplications = new SiteService( rockContext )
                        .Queryable()
                        .Where( s => s.SiteType == SiteType.Mobile )
                        .Select( s => new ListItemBag
                        {
                            Text = s.Name,
                            Value = s.Guid.ToString(),
                        } )
                        .ToList();

                    return new CommunicationEntryPushMediumOptionsBag
                    {
                        CharacterLimit = 1024,
                        HasActiveTransport = pushMedium.Transport?.IsActive ?? false,
                        MediumEntityTypeId = medium.EntityType.Id,
                        MediumEntityTypeGuid = mediumGuid,
                        Applications = mobileApplications,
                        IsUsingRockMobilePushTransport = pushMedium.Transport is IRockMobilePush
                    };
                }
            }

            return CommunicationEntryMediumOptionsBaseBag.Unknown;
        }

        /// <summary>
        /// Get the medium-specific data service.
        /// </summary>
        /// <param name="mediumEntityTypeGuid">The medium type unique identifier.</param>
        private IMediumDataService GetMediumDataService( Guid mediumEntityTypeGuid )
        {
            var (_, medium) = GetMediumComponent( mediumEntityTypeGuid );

            if ( medium == null )
            {
                return new NoOpMediumDataService();
            }

            if ( medium is Rock.Communication.Medium.Email )
            {
                return new EmailMediumDataService();
            }

            // Return no-op behavior by default.
            return new NoOpMediumDataService();
        }

        /// <summary>
        /// Get recipients as bags.
        /// </summary>
        /// <param name="rockContext">The Rock context.</param>
        /// <param name="options">The recipient query options.</param>
        private List<CommunicationEntryRecipientBag> GetRecipientBags( RockContext rockContext, RecipientQueryOptions options )
        {
            var query = GetRecipientQuery( rockContext, options );
            return ConvertToBags( query );
        }

        /// <summary>
        /// Gets a recipient query for the specified query options.
        /// </summary>
        /// <param name="rockContext">The Rock context.</param>
        /// <param name="options">The recipient query options.</param>
        private IQueryable<CommunicationEntryRecipientDto> GetRecipientQuery( RockContext rockContext, RecipientQueryOptions options )
        {
            var mobilePhoneDefinedValueId = DefinedValueCache.GetId( SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
            var personalDeviceQuery = new PersonalDeviceService( rockContext ).Queryable().AsNoTracking();

            IQueryable<PersonAlias> personAliasQuery;
            if ( options?.CommunicationId.HasValue == true )
            {
                // Get the person aliases from existing communication recipients.
                personAliasQuery = new CommunicationRecipientService( rockContext ).Queryable().AsNoTracking()
                   .Where( communicationRecipient => communicationRecipient.CommunicationId == options.CommunicationId.Value )
                   .Select( communicationRecipient => communicationRecipient.PersonAlias );
            }
            else
            {
                // Get the primary person aliases directly from the person alias table.
                personAliasQuery = new PersonAliasService( rockContext ).GetPrimaryAliasQuery().AsNoTracking();
            }

            // Filter person aliases.
            if ( options?.PersonAliasGuids?.Any() == true )
            {
                personAliasQuery = personAliasQuery.Where( personAlias => options.PersonAliasGuids.Contains( personAlias.Guid ) );
            }

            // Limit the results.
            if ( options?.Limit.HasValue == true )
            {
                personAliasQuery = personAliasQuery.Take( options.Limit.Value );
            }

            return personAliasQuery
                .Select( personAlias => new
                {
                    personAlias.Id,
                    personAlias.Guid,
                    personAlias.Person,
                    MobilePhone = personAlias.Person.PhoneNumbers.FirstOrDefault( phoneNumber => phoneNumber.NumberTypeValueId.HasValue && phoneNumber.NumberTypeValueId.Value == mobilePhoneDefinedValueId ),
                } )
                .Select( personAlias => new CommunicationEntryRecipientDto
                {
                    MobilePhone = personAlias.MobilePhone,
                    Person = personAlias.Person,
                    Recipient = new CommunicationEntryRecipientBag
                    {
                        Email = personAlias.Person.Email,
                        EmailPreference = personAlias.Person.EmailPreference.ToString(),
                        IsEmailActive = personAlias.Person.IsEmailActive,
                        IsPushAllowed = personalDeviceQuery.Any( personalDevice =>
                            personalDevice.PersonAliasId.HasValue
                            && personalDevice.NotificationsEnabled
                            && personalDevice.PersonAliasId == personAlias.Id
                        ),
                        IsDeceased = personAlias.Person.IsDeceased,
                        PersonAliasGuid = personAlias.Guid,
                        SmsNumber = personAlias.MobilePhone.Number,
                        // Set name using the full Person entity.
                        // Name = person.FullName,
                    },
                } );
        }

        /// <summary>
        /// Converts recipients to bags.
        /// </summary>
        /// <param name="recipients">The recipients to convert.</param>
        private List<CommunicationEntryRecipientBag> ConvertToBags( IEnumerable<CommunicationEntryRecipientDto> recipients )
        {
            CommunicationEntryRecipientBag ConvertToBag( CommunicationEntryRecipientDto data )
            {
                var recipient = data.Recipient;
                var person = data.Person;
                var mobilePhone = data.MobilePhone;

                // Set name using the full Person entity.
                recipient.Name = person.FullName;
                recipient.PhotoUrl = Person.GetPersonPhotoUrl( person.Initials, person.PhotoId, person.Age, person.Gender, person.RecordTypeValueId, person.AgeClassification, 24 );
                recipient.IsEmailAllowed = person.CanReceiveEmail( isBulk: false );
                recipient.IsBulkEmailAllowed = person.CanReceiveEmail( isBulk: true );
                recipient.IsSmsAllowed = mobilePhone?.Number.IsNotNullOrWhiteSpace() == true && mobilePhone.IsMessagingEnabled;

                return recipient;
            }

            IEnumerable<CommunicationEntryRecipientBag> ConvertAll()
            {
                foreach ( var data in recipients )
                {
                    yield return ConvertToBag( data );
                }
            }

            return ConvertAll().ToList();
        }

        /// <summary>
        /// Gets the communication title.
        /// </summary>
        /// <param name="communication">The communication</param>
        private string GetTitle( Model.Communication communication )
        {
            if ( communication == null || communication.Id == 0 )
            {
                return "New Communication".FormatAsHtmlTitle();
            }
            else
            {
                return ( communication.Name ?? communication.Subject ?? "New Communication" ).FormatAsHtmlTitle();
            }
        }

        /// <summary>
        /// Gets initial communication data.
        /// </summary>
        /// <param name="rockContext">The Rock context.</param>
        /// <param name="communication">The communication.</param>
        /// <param name="currentPerson">The logged in person.</param>
        /// <param name="validMediumGuids">The valid medium unique identifiers.</param>
        private ( CommunicationEntryCommunicationBag Communication, CommunicationEntryMediumOptionsBaseBag MediumOptions ) GetCommunicationData( RockContext rockContext, Model.Communication communication, Person currentPerson, IEnumerable<Guid> validMediumGuids )
        {
            if (communication == null)
            {
                // A new communication is being created.
                communication = new Rock.Model.Communication
                {
                    EnabledLavaCommands = this.EnabledLavaCommands,
                    FromEmail = currentPerson.Email,
                    FromName = currentPerson.FullName,
                    Id = 0,
                    IsBulkCommunication = this.DefaultAsBulk || ( this.Mode == Mode.Simple && this.IsSendSimpleAsBulkEnabled ),
                    SenderPersonAliasId = currentPerson.PrimaryAliasId,
                    Status = CommunicationStatus.Transient,
                };
            }

            // Copy the communication data from the new/existing communication.
            var communicationBag = new CommunicationEntryCommunicationBag();
            var communicationCopyTarget = new CommunicationDetailsAdapter( communicationBag, rockContext );
            CommunicationEntryHelper.Copy( communication, communicationCopyTarget );

            // Override the sender information to the logged in person since they are creating/editing the communication.
            communicationBag.FromAddress = currentPerson.Email;
            communicationBag.FromName = currentPerson.FullName;

            // These props are not copied in the CommunicationEntryHelper.Copy method,
            // so copy them here.
            communicationBag.CommunicationId = communication.Id;
            communicationBag.CommunicationGuid = communication.Guid;
            communicationBag.FutureSendDateTime = communication.FutureSendDateTime;
            communicationBag.IsBulkCommunication = communication.IsBulkCommunication;
            communicationBag.Status = communication.Status;

            // Get the recipients.
            communicationBag.Recipients = new List<CommunicationEntryRecipientBag>();
            if ( communication.Id == 0 )
            {
                if ( this.IsPersonPageParameterEnabled )
                {
                    var personId = this.PersonOrPersonIdPageParameter;
                    if ( personId.HasValue )
                    {
                        // Try to use the Person/PersonId page parameter to set the single recipient.
                        var personAlias = new PersonAliasService( rockContext )
                            .GetPrimaryAliasQuery()
                            .Where( p => p.PersonId == personId.Value )
                            .Select( p => new
                            {
                                p.Guid
                            } )
                            .FirstOrDefault();

                        if ( personAlias != null )
                        {
                            communicationBag.Recipients = GetRecipientBags(
                                rockContext,
                                new RecipientQueryOptions
                                {
                                    PersonAliasGuids = new[] { personAlias.Guid },
                                    Limit = 1
                                } );
                        }

                        // Set bulk to false since this communication is only being sent to one person.
                        communication.IsBulkCommunication = false;
                    }
                }
            }
            else
            {
                // This is an existing communication so load the current recipients list.
                communicationBag.Recipients = GetRecipientBags(
                    rockContext,
                    new RecipientQueryOptions
                    {
                        CommunicationId = communication.Id
                    } );
            }

            // Get the medium.
            var mediumId = this.MediumIdPageParameter;
            if ( communication.Id > 0 )
            {
                // Use the medium type on the existing communication.
                communicationBag.MediumEntityTypeGuid = GetMediumEntityTypeGuid( communication.CommunicationType ) ?? Guid.Empty;
            }
            else if ( mediumId.HasValue )
            {
                // Use the medium page parameter.
                var mediumGuid = EntityTypeCache.Get( mediumId.Value, rockContext )?.Guid;

                if ( mediumGuid.HasValue && validMediumGuids.Contains( mediumGuid.Value) )
                {
                    // The supplied medium has a valid Guid so use it.
                    communicationBag.MediumEntityTypeGuid = mediumGuid.Value;
                }
            }

            // Ensure the medium is one of the valid selections.
            if ( !validMediumGuids.Contains( communicationBag.MediumEntityTypeGuid ) )
            {
                communicationBag.MediumEntityTypeGuid = validMediumGuids.FirstOrDefault();
            }

            // Get the medium options (this is needed here to find the valid templates for the selected medium).
            var mediumOptions = GetMediumOptions( communicationBag.MediumEntityTypeGuid, currentPerson );

            // Get the template.
            var template = communication.CommunicationTemplate;
            if ( template == null )
            {
                var communicationTemplateGuid = this.TemplateGuidPageParameter;
                if ( communicationTemplateGuid.HasValue )
                {
                    // The communication has no template so use the template associated with the page parameter.
                    template = new CommunicationTemplateService( rockContext ).Get( communicationTemplateGuid.Value );
                }

                if ( template == null )
                {
                    // Use the default template from the block setting.
                    var defaultCommunicationTemplateGuid = this.DefaultTemplateGuid;
                    if ( defaultCommunicationTemplateGuid.HasValue )
                    {
                        template = new CommunicationTemplateService( rockContext ).Get( defaultCommunicationTemplateGuid.Value );
                    }
                }
            }
            communicationBag.CommunicationTemplateGuid = template?.Guid;

            if ( template != null && communication.Status == CommunicationStatus.Transient )
            {
                if ( mediumOptions.Templates?.Any( t => t.Value.AsGuid() == template.Guid ) == true )
                {
                    // Copy communication data from the template.
                    CommunicationEntryHelper.CopyTemplate( template, communicationCopyTarget, this.RequestContext );
                    
                    // Override the sender information to the logged in person since they are creating/editing the communication.
                    communicationBag.FromAddress = currentPerson.Email;
                    communicationBag.FromName = currentPerson.FullName;
                }
            }

            return ( communicationBag, mediumOptions );
        }

        /// <summary>
        /// Gets the medium entity type unique identifier for a communication type.
        /// </summary>
        /// <param name="communicationType">The communication type.</param>
        private Guid? GetMediumEntityTypeGuid( CommunicationType communicationType )
        {
            switch ( communicationType )
            {
                case CommunicationType.Email:
                    return SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid();
                case CommunicationType.SMS:
                    return SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid();
                case CommunicationType.PushNotification:
                    return SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid();
                case CommunicationType.RecipientPreference:
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the communication templates for a given medium.
        /// </summary>
        /// <param name="medium">The medium.</param>
        /// <param name="rockContext">The Rock context.</param>
        private List<CommunicationTemplate> GetCommunicationTemplates( MediumComponent medium, RockContext rockContext )
        {
            var templates = new List<CommunicationTemplate>();

            if ( medium == null )
            {
                return templates;
            }

            var currentPerson = GetCurrentPerson();

            foreach ( var template in new CommunicationTemplateService( rockContext )
                .Queryable().AsNoTracking()
                .Where( a => a.IsActive )
                .OrderBy( t => t.Name ) )
            {
                if ( template == null || !template.IsAuthorized( Authorization.VIEW, currentPerson ) )
                {
                    continue;
                }

                /*
                   DV 26-JAN-2022
                  
                   If this is a Simple Email communication then filter out the Communication Wizard Templates.
                   If this is an SMS then only include templates that have SMS templates. 
                   
                   Reason: GitHub Issue #4888
                 */
                var isValidEmailTemplate = medium.CommunicationType == CommunicationType.Email && template.HasEmailTemplate() && !template.SupportsEmailWizard();
                var isValidSmsTemplate = medium.CommunicationType == CommunicationType.SMS && template.HasSMSTemplate();
                if ( isValidEmailTemplate || isValidSmsTemplate )
                {
                    templates.Add( template );
                }
            }

            return templates;
        }

        /// <summary>
        /// Creates a lazy instance of any type, including anonymous types.
        /// </summary>
        /// <typeparam name="T">The generic lazy type.</typeparam>
        /// <param name="lazyInitializer">The lazy initializer.</param>
        private Lazy<T> CreateLazy<T>( Func<T> lazyInitializer )
        {
            return new Lazy<T>( lazyInitializer );
        }

        /// <summary>
        /// Updates a communication model with the user-entered values.
        /// </summary>
        /// <param name="rockContext">The Rock context.</param>
        /// <param name="bag">The communication bag.</param>
        private Rock.Model.Communication UpdateCommunication( RockContext rockContext, CommunicationEntryCommunicationBag bag )
        {
            var communicationService = new CommunicationService( rockContext );
            var communicationAttachmentService = new CommunicationAttachmentService( rockContext );
            var communicationRecipientService = new CommunicationRecipientService( rockContext );
            var communicationTemplateService = new CommunicationTemplateService( rockContext );
            var primaryPersonAliasQuery = new PersonAliasService( rockContext ).GetPrimaryAliasQuery();

            var currentPersonAliasId = GetCurrentPerson().PrimaryAliasId;
            var newRecipientPersonAliasGuids = new HashSet<Guid>( bag.Recipients.Select( a => a.PersonAliasGuid ) );

            Rock.Model.Communication communication = null;

            var currentRecipients = CreateLazy(
                () =>
                {
                    // Tracking is required so recipients can be added/removed when updating a communication.
                    return communication.GetRecipientsQry( rockContext )
                        .Select( r => new
                        {
                            Recipient = r,
                            PersonAliasGuid = r.PersonAlias.Guid
                        } )
                        .ToList();
                } );

            var communicationId = this.CommunicationIdPageParameter;
            if ( communicationId.HasValue && communicationId.Value != 0 )
            {
                communication = communicationService.Get( communicationId.Value );
            }

            if ( communication == null )
            {
                communication = new Rock.Model.Communication
                {
                    Status = CommunicationStatus.Transient,
                    SenderPersonAliasId = currentPersonAliasId
                };
                communicationService.Add( communication );
            }
            else
            {
                communication.GetAttachments( communication.CommunicationType );

                // Remove any deleted recipients.                
                foreach ( var currentRecipient in currentRecipients.Value )
                {
                    if ( !newRecipientPersonAliasGuids.Contains( currentRecipient.PersonAliasGuid ) )
                    {
                        communicationRecipientService.Delete( currentRecipient.Recipient );
                        communication.Recipients.Remove( currentRecipient.Recipient );
                    }
                }
            }

            // Add any new recipients.
            foreach ( var newRecipient in bag.Recipients )
            {
                if ( !currentRecipients.Value.Any( currentRecipient => currentRecipient.PersonAliasGuid != newRecipient.PersonAliasGuid ) )
                {
                    var primaryPersonAlias = primaryPersonAliasQuery.FirstOrDefault( p => p.Guid == newRecipient.PersonAliasGuid );
                    if ( primaryPersonAlias != null )
                    {
                        var communicationRecipient = new CommunicationRecipient
                        {
                            PersonAlias = primaryPersonAlias
                        };
                        communication.Recipients.Add( communicationRecipient );
                    }
                }
            }

            communication.EnabledLavaCommands = this.EnabledLavaCommands;
            communication.IsBulkCommunication = bag.IsBulkCommunication;

            var (_, medium) = GetMediumComponent( bag.MediumEntityTypeGuid );
            if ( medium != null )
            {
                communication.CommunicationType = medium.CommunicationType;
            }

            if ( bag.CommunicationTemplateGuid.HasValue && bag.CommunicationTemplateGuid.Value != Guid.Empty )
            {
                communication.CommunicationTemplateId = communicationTemplateService.GetId( bag.CommunicationTemplateGuid.Value );
            }

            // Ensure the medium is correct for all communication recipients.
            foreach ( var recipient in communication.Recipients )
            {
                recipient.MediumEntityTypeId = medium?.EntityType?.Id;
            }

            // Copy the communication data in the request to the Communication object.
            CommunicationDetails.Copy( new CommunicationDetailsAdapter( bag, rockContext ), communication );

            var communicationType = communication.CommunicationType;
            List<ListItemBag> binaryFileAttachments;
            if ( communicationType == CommunicationType.Email )
            {
                binaryFileAttachments = bag.EmailAttachmentBinaryFiles;
            }
            else if ( communicationType == CommunicationType.SMS )
            {
                binaryFileAttachments = bag.SmsAttachmentBinaryFiles;
            }
            else
            {
                binaryFileAttachments = new List<ListItemBag>();
            }

            var binaryFileAttachmentGuids = binaryFileAttachments?
                .Select( b => b.Value.AsGuid() )
                .Where( g => !g.IsEmpty() )
                .ToList() ?? new List<Guid>();

            // Delete any attachments that are no longer included.
            var attachmentsToRemove = communication.Attachments
                .Where( a => !binaryFileAttachmentGuids.Contains( a.BinaryFile.Guid ) )
                .ToList();
            foreach ( var attachment in attachmentsToRemove )
            {
                communication.Attachments.Remove( attachment );
                communicationAttachmentService.Delete( attachment );
            }

            // Add any new attachments.
            if ( binaryFileAttachmentGuids.Any() )
            {
                var attachmentIdMap = new BinaryFileService( rockContext )
                    .Queryable()
                    .Where( b => binaryFileAttachmentGuids.Contains( b.Guid ) )
                    .Select( b => new
                    {
                        b.Id,
                        b.Guid
                    } )
                    .ToDictionary( a => a.Guid, a => a.Id );
                foreach ( var attachmentBinaryFileId in attachmentIdMap )
                {
                    if ( !communication.Attachments.Any( x => x.BinaryFileId == attachmentBinaryFileId.Value ) )
                    {
                        communication.AddAttachment( new CommunicationAttachment { BinaryFileId = attachmentBinaryFileId.Value }, communicationType );
                    }
                }
            }

            if ( bag.FutureSendDateTime.HasValue )
            {
                communication.FutureSendDateTime = bag.FutureSendDateTime.Value.DateTime;
            }
            else
            {
                communication.FutureSendDateTime = null;
            }

            // If we are not allowing lava then remove the syntax
            if ( !this.IsLavaEnabled )
            {
                communication.Message = communication.Message.SanitizeLava();
                communication.PushMessage = communication.PushMessage.SanitizeLava();
                communication.Subject = communication.Subject.SanitizeLava();
                communication.BCCEmails = communication.BCCEmails.SanitizeLava();
                communication.CCEmails = communication.CCEmails.SanitizeLava();
                communication.FromEmail = communication.FromEmail.SanitizeLava();
                communication.FromName = communication.FromName.SanitizeLava();
                communication.ReplyToEmail = communication.ReplyToEmail.SanitizeLava();
            }

            return communication;
        }

        /// <summary>
        /// Validates a save request.
        /// </summary>
        private static bool IsSaveRequestValid( CommunicationEntrySaveRequestBag bag, out ValidationResult validationResult )
        {
            // Validation for all medium types.
            if ( !bag.Validate( "Communication Information" ).IsNotNull( out validationResult )
                    || !bag.MediumEntityTypeGuid.Validate( "Medium Type" ).IsNotEmpty( out validationResult ) )
            {
                return false;
            }

            // Validation for specific medium types.
            if ( bag.MediumEntityTypeGuid == SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() )
            {
                // Email
                return bag.FromName.Validate( "From Name" ).IsNotNullOrWhiteSpace( out validationResult )
                    && bag.FromAddress.Validate( "From Address" ).IsNotNullOrWhiteSpace( out validationResult )
                    && ( !bag.FutureSendDateTime.HasValue || bag.FutureSendDateTime.Value.Validate( "Schedule Send" ).IsNowOrFutureDateTime( out validationResult ) );
            }
            else if ( bag.MediumEntityTypeGuid == SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() )
            {
                // SMS
                return bag.SmsFromSystemPhoneNumberGuid.Validate( "From Phone" ).IsNotNull( out validationResult )
                    && bag.SmsMessage.Validate( "Message" ).IsNotNullOrWhiteSpace( out validationResult )
                    && ( !bag.FutureSendDateTime.HasValue || bag.FutureSendDateTime.Value.Validate( "Schedule Send" ).IsNowOrFutureDateTime( out validationResult ) );
            }
            else if ( bag.MediumEntityTypeGuid == SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() )
            {
                // Push
                return !bag.FutureSendDateTime.HasValue || bag.FutureSendDateTime.Value.Validate( "Schedule Send" ).IsNowOrFutureDateTime( out validationResult );
            }
            else
            {
                // Unsupported medium type.
                validationResult = new ValidationResult( "Medium Type is not supported." );
                return false;
            }
        }

        /// <summary>
        /// Validates a send request.
        /// </summary>
        private static bool IsSendRequestValid( CommunicationEntrySendRequestBag bag, out ValidationResult validationResult )
        {
            // Validation for all medium types.
            if ( !bag.Validate( "Communication Information" ).IsNotNull( out validationResult )
                    || !bag.MediumEntityTypeGuid.Validate( "Medium Type" ).IsNotEmpty( out validationResult ) )
            {
                return false;
            }

            // Validation for specific medium types.
            if ( bag.MediumEntityTypeGuid == SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() )
            {
                // Email
                return bag.FromName.Validate( "From Name" ).IsNotNullOrWhiteSpace( out validationResult )
                    && bag.FromAddress.Validate( "From Address" ).IsNotNullOrWhiteSpace( out validationResult )
                    && bag.Recipients.Validate( "Recipients" ).IsNotEmpty( out validationResult )
                    && ( !bag.FutureSendDateTime.HasValue || bag.FutureSendDateTime.Value.Validate( "Schedule Send" ).IsNowOrFutureDateTime( out validationResult ) );
            }
            else if ( bag.MediumEntityTypeGuid == SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() )
            {
                // SMS
                return bag.SmsFromSystemPhoneNumberGuid.Validate( "From Phone" ).IsNotNull( out validationResult )
                    && bag.SmsMessage.Validate( "Message" ).IsNotNullOrWhiteSpace( out validationResult )
                    && bag.Recipients.Validate( "Recipients" ).IsNotEmpty( out validationResult )
                    && ( !bag.FutureSendDateTime.HasValue || bag.FutureSendDateTime.Value.Validate( "Schedule Send" ).IsNowOrFutureDateTime( out validationResult ) );
            }
            else if ( bag.MediumEntityTypeGuid == SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() )
            {
                // Push
                return bag.PushTitle.Validate( "Title" ).HasMaxLength( 100, out validationResult )
                    && bag.PushMessage.Validate( "Message" ).HasMaxLength( 1024, out validationResult )
                    && bag.Recipients.Validate( "Recipients" ).IsNotEmpty( out validationResult )
                    && ( !bag.FutureSendDateTime.HasValue || bag.FutureSendDateTime.Value.Validate( "Schedule Send" ).IsNowOrFutureDateTime( out validationResult ) );
            }
            else
            {
                // Unsupported medium type.
                validationResult = new ValidationResult( "Medium Type is not supported." );
                return false;
            }
        }

        /// <summary>
        /// Validates a test request.
        /// </summary>
        private static bool IsTestRequestValid( CommunicationEntryTestRequestBag bag, out ValidationResult validationResult )
        {
            // Validation for all medium types.
            if ( !bag.Validate( "Communication Information" ).IsNotNull( out validationResult )
                    || !bag.MediumEntityTypeGuid.Validate( "Medium Type" ).IsNotEmpty( out validationResult ) )
            {
                return false;
            }

            // Validation for specific medium types.
            if ( bag.MediumEntityTypeGuid == SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() )
            {
                // Email
                return bag.FromName.Validate( "From Name" ).IsNotNullOrWhiteSpace( out validationResult )
                    && bag.FromAddress.Validate( "From Address" ).IsNotNullOrWhiteSpace( out validationResult )
                    && bag.Recipients.Validate( "Recipients" ).IsNotEmpty( out validationResult )
                    && ( !bag.FutureSendDateTime.HasValue || bag.FutureSendDateTime.Value.Validate( "Schedule Send" ).IsNowOrFutureDateTime( out validationResult ) );
            }
            else if ( bag.MediumEntityTypeGuid == SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() )
            {
                // SMS
                return bag.SmsFromSystemPhoneNumberGuid.Validate( "From Phone" ).IsNotNull( out validationResult )
                    && bag.SmsMessage.Validate( "Message" ).IsNotNullOrWhiteSpace( out validationResult )
                    && bag.Recipients.Validate( "Recipients" ).IsNotEmpty( out validationResult )
                    && ( !bag.FutureSendDateTime.HasValue || bag.FutureSendDateTime.Value.Validate( "Schedule Send" ).IsNowOrFutureDateTime( out validationResult ) );
            }
            else if ( bag.MediumEntityTypeGuid == SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() )
            {
                // Push
                return bag.PushTitle.Validate( "Title" ).HasMaxLength( 100, out validationResult )
                    && bag.PushMessage.Validate( "Message" ).HasMaxLength( 1024, out validationResult )
                    && bag.Recipients.Validate( "Recipients" ).IsNotEmpty( out validationResult )
                    && ( !bag.FutureSendDateTime.HasValue || bag.FutureSendDateTime.Value.Validate( "Schedule Send" ).IsNowOrFutureDateTime( out validationResult ) );
            }
            else
            {
                // Unsupported medium type.
                validationResult = new ValidationResult( "Medium Type is not supported." );
                return false;
            }
        }
        
        /// <summary>
        /// Validates a save metrics reminder request.
        /// </summary>
        private bool IsSaveMetricsReminderRequestValid( CommunicationEntrySaveMetricsReminderRequestBag bag, out ValidationResult validationResult )
        {
            return this.AreEmailMetricsReminderOptionsShown.Validate( "Email Metrics Reminder Feature" ).WithErrorMessage( v => $"{v.FriendlyName} is not enabled." ).IsTrue( out validationResult )
                && bag.Validate( "Save Metrics Reminder Information" ).IsNotNull( out validationResult )
                && bag.CommunicationGuid.Validate( "Communication" ).IsNotEmpty( out validationResult )
                && bag.DaysUntilReminder.Validate( "Days Until Reminder" ).IsGreaterThanOrEqualTo( 1, out validationResult );
        }

        /// <summary>
        /// Validates a cancel metrics reminder request.
        /// </summary>
        private bool IsCancelMetricsReminderRequestValid( Guid communicationGuid, out ValidationResult validationResult )
        {
            // The Email Metrics Reminder feature does not need to be enabled to cancel an email reminder.
            return communicationGuid.Validate( "Communication" ).IsNotEmpty( out validationResult );
        }

        #endregion

        #region Helper Types

        private class RecipientQueryOptions
        {
            public IEnumerable<Guid> PersonAliasGuids { get; set; }

            public int? CommunicationId { get; set; }

            public int? Limit { get; set; }
        }

        private interface IMediumDataService
        {
            void OnCommunicationSave( RockContext rockContext, CommunicationEntryCommunicationBag communication );
        }

        private class EmailMediumDataService : IMediumDataService
        {
            public void OnCommunicationSave( RockContext rockContext, CommunicationEntryCommunicationBag communication )
            {
                // On saving the email communication, mark all the file attachments as not temporary.
                var binaryFileGuids = communication.EmailAttachmentBinaryFiles.Select( bf => bf.Value.AsGuid() ).Where( g => !g.IsEmpty() ).ToList();
                if ( binaryFileGuids.Any() )
                {
                    var binaryFilesQuery = new BinaryFileService( rockContext )
                        .Queryable()
                        .Where( f => binaryFileGuids.Contains( f.Guid ) );
                    foreach ( var binaryFile in binaryFilesQuery )
                    {
                        binaryFile.IsTemporary = false;
                    }
                }
            }
        }

        /// <summary>
        /// A no-op medium data service that does not do anything.
        /// </summary>
        private class NoOpMediumDataService : IMediumDataService
        {
            public void OnCommunicationSave( RockContext rockContext, CommunicationEntryCommunicationBag communication )
            {
                // Do nothing.
            }
        }

        private interface ICommunicationAttachments
        {
            IEnumerable<AttachmentDto> EmailAttachments { get; set; }

            void SetEmailAttachments( IEnumerable<int> binaryFileIds );

            IEnumerable<AttachmentDto> SmsAttachments { get; set; }

            void SetSmsAttachments( IEnumerable<int> binaryFileIds );
        }

        private static class CommunicationEntryHelper
        {
            public static void Copy( ICommunicationDetails source, ICommunicationDetails target )
            {
                CommunicationDetails.Copy( source, target );

                if ( target is ICommunicationAttachments attachmentsTarget )
                {
                    attachmentsTarget.SetEmailAttachments( source.EmailAttachmentBinaryFileIds );
                    attachmentsTarget.SetSmsAttachments( source.SMSAttachmentBinaryFileIds );
                }
            } 

            public static void CopyTemplate( CommunicationTemplate source, ICommunicationDetails target, RockRequestContext requestContext )
            {
                // Save what was entered for fields in case the template blanks them out.
                var originalFromEmail = target.FromEmail;
                var originalFromName = target.FromName;
                var originalReplyToEmail = target.ReplyToEmail;

                Copy( source, target );

                // Resolve lava-enabled fields from the template.
                target.FromName = source.FromName.ResolveMergeFields( requestContext.GetCommonMergeFields() );
                target.FromEmail = source.FromEmail.ResolveMergeFields( requestContext.GetCommonMergeFields() );
                target.ReplyToEmail = source.ReplyToEmail.ResolveMergeFields( requestContext.GetCommonMergeFields() );

                // If FromName was cleared by the template,
                // then use the original value (similar logic to CommunicationEntryWizard).
                if ( target.FromName.IsNullOrWhiteSpace() )
                {
                    target.FromName = originalFromName;
                }

                // If FromEmail was cleared by the template,
                // then use the original value (similar logic to CommunicationEntryWizard).
                if ( target.FromEmail.IsNullOrWhiteSpace() )
                {
                    target.FromEmail = originalFromEmail;
                }

                // If ReplyToEmail was cleared by the template,
                // then use the original value.
                if ( target.ReplyToEmail.IsNullOrWhiteSpace() )
                {
                    target.ReplyToEmail = originalReplyToEmail;
                }
            }
        }

        private static class AttachmentHelper
        {
            public static List<AttachmentDto> GetAttachments( RockContext rockContext, IEnumerable<int> binaryFileIds )
            {
                var query = new BinaryFileService( rockContext )
                    .Queryable()
                    .Where( bf => binaryFileIds.Contains( bf.Id ) );

                return GetAttachments( query );
            }

            public static List<AttachmentDto> GetAttachments( RockContext rockContext, IEnumerable<Guid> binaryFileGuids )
            {
                var query = new BinaryFileService( rockContext )
                   .Queryable()
                   .Where( bf => binaryFileGuids.Contains( bf.Guid ) );

                return GetAttachments( query );
            }

            public static List<AttachmentDto> GetAttachments( IQueryable<BinaryFile> binaryFileQuery )
            {
                return binaryFileQuery?
                    .Select( bf => new AttachmentDto
                    {
                        Id = bf.Id,
                        Guid = bf.Guid,
                        FileName = bf.FileName
                    } )
                    .ToList();
            }

            public static List<ListItemBag> ToListItemBags( IEnumerable<AttachmentDto> attachmentDtos )
            {
                return attachmentDtos?.Select( s => new ListItemBag
                {
                    Text = s.FileName,
                    Value = s.Guid.ToString()
                } )
                    .ToList();
            }
        }

        private class AttachmentDto
        {
            public int Id { get; set; }

            public Guid Guid { get; set; }

            public string FileName { get; set; }

            public ListItemBag ToListItemBag()
            {
                return new ListItemBag
                {
                    Text = FileName,
                    Value = Guid.ToString()
                };
            }
        }

        private class CommunicationEntryRecipientDto
        {
            public Person Person { get; set; }

            public CommunicationEntryRecipientBag Recipient { get; set; }

            public PhoneNumber MobilePhone { get; set; }
        }

        /// <summary>
        /// Adapts a communication bag to an ICommunicationDetails instance.
        /// </summary>
        /// <remarks>Used for copying data from a communication/template to a communication bag.</remarks>
        /// <seealso cref="Rock.Communication.ICommunicationDetails" />
        private class CommunicationDetailsAdapter : ICommunicationDetails, ICommunicationAttachments
        {
            private readonly CommunicationEntryCommunicationBag _bag;
            private readonly RockContext _rockContext;

            public CommunicationDetailsAdapter( CommunicationEntryCommunicationBag bag, RockContext rockContext )
            {
                _bag = bag ?? throw new ArgumentNullException( nameof( bag ) );
                _rockContext = rockContext ?? throw new ArgumentNullException( nameof( rockContext ) );
            }

            public Guid CommunicationGuid { get => _bag.CommunicationGuid; set => _bag.CommunicationGuid = value; }
            public Guid MediumEntityTypeGuid { get => _bag.MediumEntityTypeGuid; set => _bag.MediumEntityTypeGuid = value; }
            public Guid? CommunicationTemplateGuid { get => _bag.CommunicationTemplateGuid; set => _bag.CommunicationTemplateGuid = value; }
            public bool IsBulkCommunication { get => _bag.IsBulkCommunication; set => _bag.IsBulkCommunication = value; }
            public string FromName { get => _bag.FromName; set => _bag.FromName = value; }
            public string FromEmail { get => _bag.FromAddress; set => _bag.FromAddress = value; }
            public string ReplyToEmail { get => _bag.ReplyAddress; set => _bag.ReplyAddress = value; }
            public string CCEmails { get => _bag.CCAddresses; set => _bag.CCAddresses = value; }
            public string BCCEmails { get => _bag.BCCAddresses; set => _bag.BCCAddresses = value; }
            public string Subject { get => _bag.Subject; set => _bag.Subject = value; }
            public string Message { get => _bag.Message; set => _bag.Message = value; }
            public string MessageMetaData { get => _bag.MessageMetaData; set => _bag.MessageMetaData = value; }
            public string PushTitle { get => _bag.PushTitle; set => _bag.PushTitle = value; }
            public string PushMessage { get => _bag.PushMessage; set => _bag.PushMessage = value; }
            public string PushSound { get => _bag.PushSound; set => _bag.PushSound = value; }
            public string PushData
            {
                get
                {
                    return PushOptionsAsPushData( _bag.PushData )?.ToJson();
                }
                set
                {
                    _bag.PushData = PushDataAsPushOptions( value.FromJsonOrNull<PushData>() ) ?? value.FromJsonOrNull<CommunicationEntryPushNotificationOptionsBag>();
                }
            }
            public int? PushImageBinaryFileId { get => _bag.PushImageBinaryFileId; set => _bag.PushImageBinaryFileId = value; }
            public string PushOpenMessage
            {
                get
                {
                    if ( _bag.PushOpenMessageJson.IsNotNullOrWhiteSpace() )
                    {
                        return new StructuredContentHelper( _bag.PushOpenMessageJson ).Render();
                    }
                    else
                    {
                        return null;
                    }
                }
                set
                {
                    /* Do nothing */
                }
            }
            public string PushOpenMessageJson { get => _bag.PushOpenMessageJson; set => _bag.PushOpenMessageJson = value; }
            public string SMSMessage { get => _bag.SmsMessage; set => _bag.SmsMessage = value; }
            public IEnumerable<int> EmailAttachmentBinaryFileIds { get => this.EmailAttachments?.Select( a => a.Id ); }
            public DateTimeOffset? FutureSendDateTime { get => _bag.FutureSendDateTime; set => _bag.FutureSendDateTime = value; }
            public IEnumerable<int> SMSAttachmentBinaryFileIds { get => this.SmsAttachments?.Select( a => a.Id ); }
            public CommunicationStatus Status { get => _bag.Status; set => _bag.Status = value; }

            public PushOpenAction? PushOpenAction
            {
                get
                {
                    return _bag.PushOpenAction.HasValue
                        ? ( PushOpenAction ) ( int ) _bag.PushOpenAction.Value
                        : ( PushOpenAction? ) null;
                }
                set
                {
                    _bag.PushOpenAction = value.HasValue
                        ? ( PushOpenActionType ) ( int ) value.Value
                        : ( PushOpenActionType? ) null;
                }
            }

            public int? SmsFromSystemPhoneNumberId
            {
                get
                {
                    if ( _bag.SmsFromSystemPhoneNumberGuid.HasValue )
                    {
                        return SystemPhoneNumberCache.GetId( _bag.SmsFromSystemPhoneNumberGuid.Value );
                    }
                    else
                    {
                        return null;
                    }
                }
                set
                {
                    if ( value.HasValue )
                    {
                        _bag.SmsFromSystemPhoneNumberGuid = SystemPhoneNumberCache.GetGuid( value.Value );
                    }
                    else
                    {
                        _bag.SmsFromSystemPhoneNumberGuid = null;
                    }
                }
            }

            public IEnumerable<AttachmentDto> EmailAttachments
            {
                get
                {
                    // Convert the bag data to attachments.
                    return AttachmentHelper.GetAttachments( _rockContext, _bag.EmailAttachmentBinaryFiles?.Select( b => b.Value.AsGuid() ).Where( g => !g.IsEmpty() ) );
                }
                set
                {
                    // Update the binary files in the bag from the attachments.
                    _bag.EmailAttachmentBinaryFiles = AttachmentHelper.ToListItemBags( value );
                }
            }

            public IEnumerable<AttachmentDto> SmsAttachments
            {
                get
                {
                    // Convert the bag data to attachments.
                    return AttachmentHelper.GetAttachments( _rockContext, _bag.SmsAttachmentBinaryFiles?.Select( b => b.Value.AsGuid() ).Where( g => !g.IsEmpty() ) );
                }
                set
                {
                    // Update the binary files in the bag from the attachments.
                    _bag.SmsAttachmentBinaryFiles = AttachmentHelper.ToListItemBags( value );
                }
            }

            /// <inheritdoc/>
            public int? SMSFromDefinedValueId { get; set; }

            /// <inheritdoc/>
            public void SetEmailAttachments( IEnumerable<int> binaryFileIds )
            {
                if ( _bag.EmailAttachmentBinaryFiles == null )
                {
                    _bag.EmailAttachmentBinaryFiles = new List<ListItemBag>();
                }

                _bag.EmailAttachmentBinaryFiles.AddRange( AttachmentHelper.ToListItemBags( AttachmentHelper.GetAttachments( _rockContext, binaryFileIds ) ) );
            }

            /// <inheritdoc/>
            public void SetSmsAttachments( IEnumerable<int> binaryFileIds )
            {
                if ( _bag.SmsAttachmentBinaryFiles == null )
                {
                    _bag.SmsAttachmentBinaryFiles = new List<ListItemBag>();
                }

                _bag.SmsAttachmentBinaryFiles.AddRange( AttachmentHelper.ToListItemBags( AttachmentHelper.GetAttachments( _rockContext, binaryFileIds ) ) );
            }

            private PushData PushOptionsAsPushData( CommunicationEntryPushNotificationOptionsBag pushOptionsBag )
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

            private CommunicationEntryPushNotificationOptionsBag PushDataAsPushOptions( PushData pushData )
            {
                PageRouteValueBag mobilePage = null;
                if ( pushData?.MobilePageId.HasValue == true )
                {
                    mobilePage = Rock.Web.Cache.PageCache.GetGuid( pushData.MobilePageId.Value )?.ToString()?.ToPageRouteValueBag();
                }

                return pushData == null
                    ? null
                    : new CommunicationEntryPushNotificationOptionsBag
                    {
                        MobileApplicationGuid = pushData.MobileApplicationId.HasValue ? SiteCache.GetGuid( pushData.MobileApplicationId.Value ) : null,
                        MobilePage = mobilePage,
                        MobilePageQueryString = pushData.MobilePageQueryString,
                        LinkToPageUrl = pushData.Url
                    };
            }
        }

        #endregion
    }
}