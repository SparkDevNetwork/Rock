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
using System.ComponentModel;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Enums.Blocks.Communication.CommunicationEntry;
using Rock.Tasks;
using Rock.Utility;
using Rock.ViewModels.Utility;
using Rock.ViewModels.Blocks.Communication.CommunicationEntry;
using Rock.Net;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Emit;

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
        Description = "Remove the lava syntax from the message without resolving it.",
        DefaultBooleanValue = false,
        IsRequired = true,
        Order = 0 )]
    [LavaCommandsField( "Enabled Lava Commands",
        Key = AttributeKey.EnabledLavaCommands,
        Description = "The Lava commands that should be enabled for this HTML block if Enable Lava is checked.",
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
        Description = "The default template to use for a new communication.  (Note: This will only be used if the template is for the same medium as the communication.)",
        IsRequired = false,
        Order = 4 )]
    [IntegerField( "Maximum Recipients",
        Key = AttributeKey.MaximumRecipients,
        Description = "The maximum number of recipients allowed before communication will need to be approved",
        IsRequired = false,
        DefaultIntegerValue = 0,
        Order = 5 )]
    [IntegerField( "Display Count",
        Key = AttributeKey.DisplayCount,
        Description = "The initial number of recipients to display prior to expanding list",
        IsRequired = false,
        DefaultIntegerValue = 0,
        Order = 6 )]
    [BooleanField( "Send When Approved",
        Key = AttributeKey.SendWhenApproved,
        Description = "Should communication be sent once it's approved (vs. just being queued for scheduled job to send)?",
        DefaultBooleanValue = true,
        Order = 7 )]
    [CustomDropdownListField( "Mode",
        "The mode to use ( 'Simple' mode will prevent users from searching/adding new people to communication).",
        "Full,Simple",
        Key = AttributeKey.Mode,
        IsRequired = true,
        DefaultValue = "Full",
        Order = 8 )]
    [BooleanField( "Show Attachment Uploader",
        Key = AttributeKey.ShowAttachmentUploader,
        Description = "Should the attachment uploader be shown for email communications.",
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
        Description = "The FileType to use for files that are attached to an sms or email communication",
        IsRequired = true,
        DefaultBinaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.COMMUNICATION_ATTACHMENT,
        Order = 12 )]
    [BooleanField( "Default As Bulk",
        Key = AttributeKey.DefaultAsBulk,
        Description = "Should new entries be flagged as bulk communication by default?",
        DefaultBooleanValue = false,
        Order = 13 )]
    [TextField( "Document Root Folder",
        Key = AttributeKey.DocumentRootFolder,
        Description = "The folder to use as the root when browsing or uploading documents.",
        IsRequired = false,
        DefaultValue = "~/Content",
        Category = "HTML Editor Settings",
        Order = 0 )]
    [TextField( "Image Root Folder",
        Key = AttributeKey.ImageRootFolder,
        Description = "The folder to use as the root when browsing or uploading images.",
        IsRequired = false,
        DefaultValue = "~/Content",
        Category = "HTML Editor Settings",
        Order = 1 )]
    [BooleanField( "User Specific Folders",
        Key = AttributeKey.UserSpecificFolders,
        Description = "Should the root folders be specific to current user?",
        DefaultBooleanValue = false,
        Category = "HTML Editor Settings",
        Order = 2 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "26C0C9A1-1383-48D5-A062-E05622A1CBF2" )]
    [Rock.SystemGuid.BlockTypeGuid( "F6A780EB-66A7-475D-A42E-3C29AD5A89D3" )]
    public partial class CommunicationEntry : RockBlockType
    {
        #region Keys

        /// <summary>
        /// Keys to use for Block Attributes.
        /// </summary>
        private static class AttributeKey
        {
            public const string DisplayCount = "DisplayCount";
            public const string AllowCcBcc = "AllowCcBcc";
            public const string AttachmentBinaryFileType = "AttachmentBinaryFileType";
            public const string EnabledLavaCommands = "EnabledLavaCommands";
            public const string MaximumRecipients = "MaximumRecipients";
            public const string SendWhenApproved = "SendWhenApproved";
            public const string AllowedSMSNumbers = "AllowedSMSNumbers";
            public const string ShowDuplicatePreventionOption = "ShowDuplicatePreventionOption";
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
        }

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

        private static class Mode
        {
            public const string Simple = "Simple";
            public const string Full = "Full";
        }

        #region Properties

        private bool IsFullMode
        {
            get
            {
                var mode = GetAttributeValue( AttributeKey.Mode );

                // Full mode when mode is empty or when mode == "Full".
                return mode.IsNullOrWhiteSpace() || mode == Mode.Full;
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

        private bool IsEditMode => PageParameter( PageParameterKey.Edit ).AsBoolean();

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
        /// 'Person' or 'PersonId' querystring parameter with a person Id to the block to create a communication for that person.
        /// </summary>
        private int? PersonOrPersonIdPageParameter => PageParameter( PageParameterKey.Person ).AsIntegerOrNull() ?? PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull();

        //protected int? CommunicationId
        //{
        //    get { return ViewState["CommunicationId"] as int?; }
        //    set { ViewState["CommunicationId"] = value; }
        //}

        ///// <summary>
        ///// Gets or sets the medium entity type id.
        ///// </summary>
        ///// <value>
        ///// The medium entity type id.
        ///// </value>
        //protected int? MediumEntityTypeId
        //{
        //    get { return ViewState["MediumEntityTypeId"] as int?; }
        //    set { ViewState["MediumEntityTypeId"] = value; }
        //}

        ///// <summary>
        ///// Gets or sets a value indicating whether [show all recipients].
        ///// </summary>
        ///// <value>
        /////   <c>true</c> if [show all recipients]; otherwise, <c>false</c>.
        ///// </value>
        //protected bool ShowAllRecipients
        //{
        //    get { return ViewState["ShowAllRecipients"] as bool? ?? false; }
        //    set { ViewState["ShowAllRecipients"] = value; }
        //}

        ///// <summary>
        ///// Gets or sets the communication data.
        ///// </summary>
        ///// <value>
        ///// The communication data.
        ///// </value>
        //protected CommunicationDetails CommunicationData
        //{
        //    get
        //    {
        //        var communicationData = ViewState["CommunicationData"] as CommunicationDetails;
        //        if ( communicationData == null )
        //        {
        //            communicationData = new CommunicationDetails();
        //            ViewState["CommunicationData"] = communicationData;
        //        }
        //        return communicationData;
        //    }

        //    set { ViewState["CommunicationData"] = value; }
        //}

        ///// <summary>
        ///// Gets or sets any additional merge fields.
        ///// </summary>
        //public List<string> AdditionalMergeFields
        //{
        //    get
        //    {
        //        var mergeFields = ViewState["AdditionalMergeFields"] as List<string>;
        //        if ( mergeFields == null )
        //        {
        //            mergeFields = new List<string>();
        //            ViewState["AdditionalMergeFields"] = mergeFields;
        //        }
        //        return mergeFields;
        //    }

        //    set { ViewState["AdditionalMergeFields"] = value; }
        //}

        //public bool ApproverEditing
        //{
        //    get { return ViewState["ApproverEditing"] as bool? ?? false; }
        //    set { ViewState["ApproverEditing"] = value; }
        //}

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        public override object GetObsidianBlockInitialization()
        {
            var currentPerson = GetCurrentPerson();
            var box = new CommunicationEntryInitializationBox
            {
                IsFullMode = this.IsFullMode,
                IsLavaEnabled = this.IsLavaEnabled,
                MaximumRecipientsBeforeApprovalRequired = this.MaximumRecipients,
            };

            // "Simple" mode will prevent users from searching/adding new people to communication.

            var displayedMediumGuids = GetAttributeValue( AttributeKey.Mediums ).SplitDelimitedValues().AsGuidList();
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

            using ( var rockContext = new RockContext() )
            {
                if ( mediums.Any() )
                {
                    box.Mediums = mediums
                        .Select( medium => new ListItemBag
                        {
                            Text = medium.ComponentName,
                            Value = medium.Medium.EntityType.Guid.ToString()
                        } )
                        .ToList();
                }

                // Check page parameter for existing communication.
                Model.Communication communication = null;
                var communicationId = PageParameter( PageParameterKey.CommunicationId ).AsIntegerOrNull();
                if ( communicationId.HasValue )
                {
                    communication = new CommunicationService( rockContext ).Get( communicationId.Value );
                }

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
                else
                {
                    // Load the communication attachments.
                    communication.GetAttachments( communication.CommunicationType );
                }

                // TODO JMH Does this mean an approved item is being edited
                // or that editing is approved?
                // Editing is only authorized if the current person has block security to "Approve"
                // and if currently editing a communication (indicated via page parameter).
                box.Authorization = GetAuthorizationBag( communication );

                // If communication was just created only for authorization,
                // set it to null so that showing of details works correctly.
                if ( communication.Id == 0 )
                {
                    communication = null;
                }

                if ( box.Authorization.CanEditCommunication )
                {
                    // Communication is either new or ok to edit.
                    SetCommunicationData( box, communication, currentPerson, rockContext );

                    box.IsHidden = false;
                }
                else
                {
                    // Not an editable communication, so hide this block.
                    // If there is a CommunicationDetail block on this page, it'll be shown instead
                    box.IsHidden = true;
                }
            }

            return box;
        }

        private CommunicationEntryAuthorizationBag GetAuthorizationBag( Rock.Model.Communication communication )
        {
            var currentPerson = GetCurrentPerson();

            var isBlockApproveActionAuthorized = BlockCache.IsAuthorized( Authorization.APPROVE, currentPerson );
            var isBlockEditActionAuthorized = BlockCache.IsAuthorized( Authorization.EDIT, currentPerson );
            var isCommunicationEditActionAuthorized = communication.IsAuthorized( Authorization.EDIT, currentPerson );
            var isCommunicationCreator = communication.CreatedByPersonAlias != null && communication.CreatedByPersonAlias.PersonId == currentPerson.Id;
            var canApproveCommunication = communication.Status == Rock.Model.CommunicationStatus.PendingApproval && isBlockApproveActionAuthorized && IsEditMode;
            var isEditableStatus =
                communication.Status == CommunicationStatus.Transient
                || communication.Status == CommunicationStatus.Draft
                || communication.Status == CommunicationStatus.Denied;

            return new CommunicationEntryAuthorizationBag
            {
                IsBlockApproveActionAuthorized = isBlockApproveActionAuthorized,
                IsBlockEditActionAuthorized = isBlockEditActionAuthorized,
                IsCommunicationEditActionAuthorized = isCommunicationEditActionAuthorized,
                CanApproveCommunication = canApproveCommunication,
                CanEditCommunication = canApproveCommunication
                    || (isEditableStatus && (isCommunicationEditActionAuthorized || isCommunicationCreator ) ),
            };
        }

        private (string ComponentName, MediumComponent Medium) GetMediumComponent( Guid mediumGuid )
        {
            var mediumData = MediumContainer.Instance.Components.Where( kvp => kvp.Value.Value.TypeGuid == mediumGuid ).Select( kvp => (kvp.Value.Metadata.ComponentName, kvp.Value.Value) ).FirstOrDefault();

            if ( mediumData == default )
            {
                return (null, null);
            }

            return mediumData;
        }

        [BlockAction( "GetMediumOptions" )]
        public CommunicationEntryMediumOptionsBaseBag GetMediumOptions( CommunicationEntryGetMediumOptionsRequestBag bag )
        {
            return GetMediumOptions( bag.MediumEntityTypeGuid, GetCurrentPerson() );
        }

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
                    return new CommunicationEntryEmailMediumOptionsBag
                    {
                        BinaryFileTypeGuid = GetAttributeValue( AttributeKey.AttachmentBinaryFileType ).AsGuidOrNull() ?? Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid(),
                        BulkEmailThreshold = emailMedium.GetBulkEmailThreshold(),
                        DocumentFolderRoot = GetAttributeValue( AttributeKey.DocumentRootFolder ),
                        FromName = sender?.FullName,
                        FromAddress = sender?.Email,
                        HasActiveTransport = emailMedium.Transport?.IsActive ?? false,
                        ImageFolderRoot = GetAttributeValue( AttributeKey.ImageRootFolder ),
                        IsAttachmentUploaderShown = GetAttributeValue( AttributeKey.ShowAttachmentUploader ).AsBoolean(),
                        IsUserSpecificRoot = GetAttributeValue( AttributeKey.UserSpecificFolders ).AsBoolean(),
                        MediumEntityTypeId = medium.EntityType.Id,
                        Templates = GetCommunicationTemplates( emailMedium, rockContext ).ToListItemBagList(),
                    };
                }
            }
            else if ( medium is Rock.Communication.Medium.Sms smsMedium )
            {
                var allowedSmsFromNumberGuids = GetAttributeValue( AttributeKey.AllowedSMSNumbers )
                    .SplitDelimitedValues( whitespace: true )
                    .AsGuidList();
                var currentPerson = GetCurrentPerson();
                return new CommunicationEntrySmsMediumOptionsBag
                {
                    HasActiveTransport = smsMedium.Transport?.IsActive ?? false,
                    MediumEntityTypeId = medium.EntityType.Id,
                    SmsFromNumbers = SystemPhoneNumberCache
                        .All( includeInactive: false )
                        .Where( spn => spn.IsAuthorized( Authorization.VIEW, currentPerson ) && allowedSmsFromNumberGuids.ContainsOrEmpty( spn.Guid ) )
                        .ToListItemBagList(),
                };
            }

            return CommunicationEntryMediumOptionsBaseBag.Unknown;
        }

        ///// <summary>
        ///// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        ///// </summary>
        ///// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        //protected override void OnPreRender( EventArgs e )
        //{
        //    BindRecipients();
        //}

        #endregion

        #region Block Actions

        /// <summary>
        /// Handles the SelectPerson event of the ppAddPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        [BlockAction( "GetRecipient" )]
        public BlockActionResult GetRecipient( Guid personAliasGuid )
        {
            if ( personAliasGuid == Guid.Empty )
            {
                return ActionBadRequest( "Must specify a person" );
            }

            using ( var rockContext = new RockContext() )
            {
                var recipient = GetRecipientBags( rockContext, RecipientQueryOptions.Filter.ByPersonAlias( personAliasGuid ) ).FirstOrDefault();
                return ActionOk( recipient );
            }
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppAddPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        [BlockAction( "GetRecipients" )]
        public BlockActionResult GetRecipients( CommunicationEntryGetRecipientsRequestBag bag )
        {
            if ( bag?.PersonAliasGuids?.Any() != true )
            {
                return ActionBadRequest( "Must specify a people" );
            }

            using ( var rockContext = new RockContext() )
            {
                var recipients = ConvertToBags( GetRecipientQuery( rockContext, RecipientQueryOptions.Filter.ByPersonAliases( bag.PersonAliasGuids ) ) );

                return ActionOk( recipients );
            }
        }

        #endregion

        #region Events

        [BlockAction( "GetTemplate" )]
        public BlockActionResult GetTemplate( Guid templateGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var template = new CommunicationTemplateService( rockContext ).Get( templateGuid );
                var bag = new CommunicationEntryCommunicationBag();
                var copyTarget = new CommunicationDetailsAdapter( bag, rockContext );
                CommunicationEntryHelper.CopyTemplate( template, copyTarget, this.RequestContext );

                return ActionOk( bag );
            }
        }

        //protected void ddlTemplate_SelectedIndexChanged( object sender, EventArgs e )
        //{
        //    GetMediumData();
        //    int? templateId = ddlTemplate.SelectedValue.AsIntegerOrNull();
        //    if ( templateId.HasValue )
        //    {
        //        GetTemplateData( templateId.Value );
        //    }
        //}

        ///// <summary>
        ///// Handles the Click event of the lbMedium control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        //protected void lbMedium_Click( object sender, EventArgs e )
        //{
        //    GetMediumData();
        //    var linkButton = sender as LinkButton;
        //    if ( linkButton != null )
        //    {
        //        int mediumId = int.MinValue;
        //        if ( int.TryParse( linkButton.CommandArgument, out mediumId ) )
        //        {
        //            MediumEntityTypeId = mediumId;
        //            BindMediums();

        //            var control = LoadMediumControl( true );
        //            InitializeControl( control );
        //        }
        //    }
        //}

        ///// <summary>
        ///// Handles the SelectPerson event of the ppAddPerson control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        //protected void ppAddPerson_SelectPerson( object sender, EventArgs e )
        //{
        //    if ( ppAddPerson.PersonId.HasValue )
        //    {
        //        if ( !Recipients.Any( r => r.PersonId == ppAddPerson.PersonId.Value ) )
        //        {
        //            var context = new RockContext();
        //            var Person = new PersonService( context ).Get( ppAddPerson.PersonId.Value );
        //            if ( Person != null )
        //            {
        //                var HasPersonalDevice = new PersonalDeviceService( context ).Queryable()
        //                    .Where( pd =>
        //                        pd.PersonAliasId.HasValue &&
        //                        pd.PersonAliasId == Person.PrimaryAliasId &&
        //                        pd.NotificationsEnabled )
        //                    .Any();
        //                Recipients.Add( new Recipient( Person, Person.PhoneNumbers.Any( a => a.IsMessagingEnabled ), HasPersonalDevice, CommunicationRecipientStatus.Pending ) );
        //                ShowAllRecipients = true;
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// Handles the ItemDataBound event of the rptRecipients control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="RepeaterItemEventArgs" /> instance containing the event data.</param>
        //protected void rptRecipients_ItemDataBound( object sender, RepeaterItemEventArgs e )
        //{
        //    // Hide the remove button for any recipient that is not pending.
        //    if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
        //    {
        //        var recipient = e.Item.DataItem as Recipient;
        //        if ( recipient != null )
        //        {
        //            var lRecipientName = e.Item.FindControl( "lRecipientName" ) as Literal;
        //            if ( lRecipientName != null )
        //            {
        //                string textClass = string.Empty;
        //                string textTooltip = string.Empty;

        //                if ( recipient.IsDeceased )
        //                {
        //                    textClass = "text-danger";
        //                    textTooltip = "Deceased";
        //                }
        //                else
        //                {
        //                    if ( MediumEntityTypeId == EntityTypeCache.Get( "Rock.Communication.Medium.Email" ).Id )
        //                    {
        //                        if ( string.IsNullOrWhiteSpace( recipient.Email ) )
        //                        {
        //                            textClass = "text-danger";
        //                            textTooltip = "No Email." + recipient.EmailNote;
        //                        }
        //                        else if ( !recipient.IsEmailActive )
        //                        {
        //                            // if email is not active, show reason why as tooltip
        //                            textClass = "text-danger";
        //                            textTooltip = "Email is Inactive. " + recipient.EmailNote;
        //                        }
        //                        else
        //                        {
        //                            // Email is active
        //                            if ( recipient.EmailPreference != EmailPreference.EmailAllowed )
        //                            {
        //                                textTooltip = Recipient.PreferenceMessage( recipient );

        //                                if ( recipient.EmailPreference == EmailPreference.NoMassEmails )
        //                                {
        //                                    textClass = "js-no-bulk-email";
        //                                    if ( cbBulk.Checked )
        //                                    {
        //                                        // This is a bulk email and user does not want bulk emails
        //                                        textClass += " text-danger";
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    // Email preference is 'Do Not Email'
        //                                    textClass = "text-danger";
        //                                }
        //                            }
        //                        }
        //                    }
        //                    else if ( MediumEntityTypeId == EntityTypeCache.Get( "Rock.Communication.Medium.Sms" ).Id )
        //                    {
        //                        if ( !recipient.HasSmsNumber )
        //                        {
        //                            // No SMS Number
        //                            textClass = "text-danger";
        //                            textTooltip = "No phone number with SMS enabled.";
        //                        }
        //                    }
        //                    else if ( MediumEntityTypeId == EntityTypeCache.Get( "Rock.Communication.Medium.PushNotification" ).Id )
        //                    {
        //                        if ( !recipient.HasNotificationsEnabled )
        //                        {
        //                            // No Notifications Enabled
        //                            textClass = "text-danger";
        //                            textTooltip = "Notifications not enabled for this number.";
        //                        }
        //                    }
        //                }

        //                lRecipientName.Text = String.Format( "<span data-toggle=\"tooltip\" data-placement=\"top\" title=\"{0}\" class=\"{1}\">{2}</span>",
        //                    textTooltip, textClass, recipient.PersonName );
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// Handles the ItemCommand event of the rptRecipients control.
        ///// </summary>
        ///// <param name="source">The source of the event.</param>
        ///// <param name="e">The <see cref="RepeaterCommandEventArgs" /> instance containing the event data.</param>
        //protected void rptRecipients_ItemCommand( object source, RepeaterCommandEventArgs e )
        //{
        //    int personId = int.MinValue;
        //    if ( int.TryParse( e.CommandArgument.ToString(), out personId ) )
        //    {
        //        Recipients = Recipients.Where( r => r.PersonId != personId ).ToList();
        //    }
        //}

        ///// <summary>
        ///// Handles the Click event of the lbShowAllRecipients control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        //protected void lbShowAllRecipients_Click( object sender, EventArgs e )
        //{
        //    ShowAllRecipients = true;
        //}

        ///// <summary>
        ///// Handles the Click event of the lbRemoveAllRecipients control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        //protected void lbRemoveAllRecipients_Click( object sender, EventArgs e )
        //{
        //    Recipients = Recipients.Where( r => r.Status != CommunicationRecipientStatus.Pending ).ToList();
        //}

        ///// <summary>
        ///// Handles the ServerValidate event of the valRecipients control.
        ///// </summary>
        ///// <param name="source">The source of the event.</param>
        ///// <param name="args">The <see cref="ServerValidateEventArgs" /> instance containing the event data.</param>
        //protected void valRecipients_ServerValidate( object source, ServerValidateEventArgs args )
        //{
        //    args.IsValid = Recipients.Any();
        //}
        
        private static class Validate
        {
            public static bool IsNotNull( object value, string valueFriendlyName, out ValidationResult validationResult )
            {
                if ( value != null )
                {
                    validationResult = ValidationResult.Success;
                    return true;
                }
                else
                {
                    validationResult = new ValidationResult( $"{valueFriendlyName} is required." );
                    return false;
                }
            }

            public static bool IsNotNullOrWhiteSpace( string value, string valueFriendlyName, out ValidationResult validationResult )
            {
                if ( value.IsNotNullOrWhiteSpace() )
                {
                    validationResult = ValidationResult.Success;
                    return true;
                }
                else
                {
                    validationResult = new ValidationResult( $"{valueFriendlyName} is required." );
                    return false;
                }
            }

            public static bool IsNowOrFutureDateTime( DateTimeOffset value, string valueFriendlyName, out ValidationResult validationResult )
            {
                if ( value.CompareTo( RockDateTime.Now ) >= 0 )
                {
                    validationResult = ValidationResult.Success;
                    return true;
                }
                else
                { 
                    validationResult = new ValidationResult( $"{valueFriendlyName} must be a future date/time." );
                    return false;
                }
            }

            public static bool IsNotEmpty<T>( IEnumerable<T> value, string valueFriendlyName, out ValidationResult validationResult )
            {
                if ( value != null && value.Any() )
                {
                    validationResult = ValidationResult.Success;
                    return true;
                }
                else
                {
                    validationResult = new ValidationResult( $"At least one {valueFriendlyName.Singularize()} is required." );
                    return false;
                }
            }

            public static bool IsNotEmpty( Guid value, string valueFriendlyName, out ValidationResult validationResult )
            {
                if ( !value.IsEmpty() )
                {
                    validationResult = ValidationResult.Success;
                    return true;
                }
                else
                {
                    validationResult = new ValidationResult( $"{valueFriendlyName} is required." );
                    return false;
                }
            }
        }

        private static bool IsTestRequestValid( CommunicationEntryTestRequestBag bag, out ValidationResult validationResult )
        {
            return Validate.IsNotNull( bag, "Communication Information", out validationResult )
                && Validate.IsNotNullOrWhiteSpace( bag.FromName, nameof( bag.FromName ).SplitCase(), out validationResult )
                && Validate.IsNotNullOrWhiteSpace( bag.FromAddress, nameof( bag.FromAddress ).SplitCase(), out validationResult )
                && Validate.IsNotEmpty( bag.MediumEntityTypeGuid, "Medium Type", out validationResult )
                && ( !bag.FutureSendDateTime.HasValue || Validate.IsNowOrFutureDateTime( bag.FutureSendDateTime.Value, "Schedule Send", out validationResult ) )
                && Validate.IsNotEmpty( bag.Recipients, nameof( bag.Recipients ), out validationResult );
        }

        private static bool IsSendRequestValid( CommunicationEntrySendRequestBag bag, out ValidationResult validationResult )
        {
            return Validate.IsNotNull( bag, "Communication Information", out validationResult )
                && Validate.IsNotNullOrWhiteSpace( bag.FromName, nameof( bag.FromName ).SplitCase(), out validationResult )
                && Validate.IsNotNullOrWhiteSpace( bag.FromAddress, nameof( bag.FromAddress ).SplitCase(), out validationResult )
                && Validate.IsNotEmpty( bag.MediumEntityTypeGuid, "Medium Type", out validationResult )
                && ( !bag.FutureSendDateTime.HasValue || Validate.IsNowOrFutureDateTime( bag.FutureSendDateTime.Value, "Schedule Send", out validationResult ) )
                && Validate.IsNotEmpty( bag.Recipients, nameof( bag.Recipients ), out validationResult );
        }

        private static bool IsSaveRequestValid( CommunicationEntrySaveRequestBag bag, out ValidationResult validationResult )
        {
            return Validate.IsNotNull( bag, "Communication Information", out validationResult )
                && Validate.IsNotNullOrWhiteSpace( bag.FromName, nameof( bag.FromName ).SplitCase(), out validationResult )
                && Validate.IsNotNullOrWhiteSpace( bag.FromAddress, nameof( bag.FromAddress ).SplitCase(), out validationResult )
                && Validate.IsNotEmpty( bag.MediumEntityTypeGuid, "Medium Type", out validationResult )
                && ( !bag.FutureSendDateTime.HasValue || Validate.IsNowOrFutureDateTime( bag.FutureSendDateTime.Value, "Schedule Send", out validationResult ) );
        }

        /// <summary>
        /// Tests the communication by sending it to the current person.
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
                testCommunication.EnabledLavaCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands );
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
        /// Determines whether approval is required, and sets the submit button text appropriately
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <returns>
        ///   <c>true</c> if [is approval required] [the specified communication]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsApprovalRequired( int numberOfRecipients )
        {
            int.TryParse( GetAttributeValue( AttributeKey.MaximumRecipients ), out var maxRecipients );
            return numberOfRecipients > maxRecipients;
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
                    // TODO JMH This should not be able to happen but adding just in case.
                    return ActionBadRequest( "Communication failed to save. Please try again." );
                }

                var mediumBehavior = GetMediumDataService( bag.MediumEntityTypeGuid );
                if ( mediumBehavior != null )
                {
                    mediumBehavior.OnCommunicationSave( rockContext, bag );
                }

                var authorization = GetAuthorizationBag( communication );
                if ( authorization.CanApproveCommunication )
                {
                    rockContext.SaveChanges();

                    // TODO JMH Remove the "Edit" page param on the client and reload the page.
                    return ActionOk( new CommunicationEntrySendResponseBag
                    {
                        RedirectToViewMode = true,
                    } );
                }

                var responseMessage = string.Empty;

                // Save the communication as a draft prior to checking recipients.
                communication.Status = CommunicationStatus.Draft;
                rockContext.SaveChanges();

                if ( IsApprovalRequired( communication.Recipients.Count() ) && !authorization.IsBlockApproveActionAuthorized )
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
                    communication.ReviewerPersonAliasId = GetCurrentPerson().PrimaryAliasId;

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
                    if ( GetAttributeValue( AttributeKey.SendWhenApproved ).AsBoolean() )
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
                    // TODO JMH The "View Communication" link should use the communication ID in its URL.
                    CommunicationId = communication.Id,
                    CommunicationGuid = communication.Guid,
                    // TODO JMH only show the Link if there is a CommunicationDetail block type on this page.
                    HasDetailBlockOnCurrentPage = this.PageCache.Blocks.Any( a => a.BlockType.Guid == Rock.SystemGuid.BlockType.COMMUNICATION_DETAIL.AsGuid() ),
                } );
            }
        }

        public interface IMediumDataService
        {
            void OnCommunicationSave( RockContext rockContext, CommunicationEntryCommunicationBag communication );
        }

        public class EmailMediumDataService : IMediumDataService
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
        public class NoOpMediumDataService : IMediumDataService
        {
            public void OnCommunicationSave( RockContext rockContext, CommunicationEntryCommunicationBag communication )
            {
                // Do nothing.
            }
        }

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
                    // TODO JMH This is not parity. Is there something else we should do in this case?
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

                    // TODO JMH Implement the status and label in the Obsidian code.
                    //var status = communication != null ? communication.Status : CommunicationStatus.Draft;
                    //switch ( status )
                    //{
                    //    case CommunicationStatus.Transient:
                    //    case CommunicationStatus.Draft:
                    //        {
                    //            hlStatus.Text = "Draft";
                    //            hlStatus.LabelType = LabelType.Default;
                    //            break;
                    //        }
                    //    case CommunicationStatus.PendingApproval:
                    //        {
                    //            hlStatus.Text = "Pending Approval";
                    //            hlStatus.LabelType = LabelType.Warning;
                    //            break;
                    //        }
                    //    case CommunicationStatus.Approved:
                    //        {
                    //            hlStatus.Text = "Approved";
                    //            hlStatus.LabelType = LabelType.Success;
                    //            break;
                    //        }
                    //    case CommunicationStatus.Denied:
                    //        {
                    //            hlStatus.Text = "Denied";
                    //            hlStatus.LabelType = LabelType.Danger;
                    //            break;
                    //        }
                    //}

                    var responseBag = new CommunicationEntrySendResponseBag
                    {
                        CommunicationGuid = communication.Guid,
                        CommunicationStatus = communication.Status,
                        CommunicationId = communication.Id,
                        HasDetailBlockOnCurrentPage = this.PageCache.Blocks.Any( a => a.BlockType.Guid == Rock.SystemGuid.BlockType.COMMUNICATION_DETAIL.AsGuid() ),
                        Message = "The communication has been saved",
                        RedirectToViewMode = true,
                    };

                    // TODO JMH Auto-scroll to results in Obsidian code.
                    return ActionOk( responseBag );
                }
            }
        }

        //protected void btnCancel_Click( object sender, EventArgs e )
        //{
        //    if ( _editingApproved )
        //    {
        //        var communicationService = new CommunicationService( new RockContext() );
        //        var communication = communicationService.Get( CommunicationId.Value );
        //        if ( communication != null && communication.Status == CommunicationStatus.PendingApproval )
        //        {
        //            // Redirect back to same page without the edit param
        //            var pageRef = new Rock.Web.PageReference();
        //            pageRef.PageId = CurrentPageReference.PageId;
        //            pageRef.RouteId = CurrentPageReference.RouteId;
        //            pageRef.Parameters = new Dictionary<string, string>();
        //            pageRef.Parameters.Add( PageParameterKey.CommunicationId, communication.Id.ToString() );
        //            Response.Redirect( pageRef.BuildUrl() );
        //            Context.ApplicationInstance.CompleteRequest();
        //        }
        //    }
        //}

        #endregion

        #region Private Methods

        class RecipientQueryOptions
        {
            public IEnumerable<Guid> PersonAliasGuids { get; set; }

            public int? CommunicationId { get; set; }

            public static RecipientQueryOptionsBuilder Filter
            {
                get
                {
                    return new RecipientQueryOptionsBuilder();
                }
            }

            public class RecipientQueryOptionsBuilder
            {
                private List<Guid> PersonAliasGuids { get; set; }

                private int? CommunicationId { get; set; }

                public RecipientQueryOptionsBuilder ByCommunication( int communicationId )
                {
                    CommunicationId = communicationId;
                    return this;
                }

                public RecipientQueryOptionsBuilder ByPersonAlias( Guid personAliasGuid )
                {
                    if ( this.PersonAliasGuids == null )
                    {
                        this.PersonAliasGuids = new List<Guid>();
                    }

                    this.PersonAliasGuids.Add( personAliasGuid );
                    return this;
                }

                public RecipientQueryOptionsBuilder ByPersonAliases( IEnumerable<Guid> personAliasGuids )
                {
                    if ( this.PersonAliasGuids == null )
                    {
                        this.PersonAliasGuids = new List<Guid>();
                    }

                    this.PersonAliasGuids.AddRange( personAliasGuids );
                    return this;
                }

                public RecipientQueryOptions Build()
                {
                    return new RecipientQueryOptions
                    {
                        CommunicationId = CommunicationId,
                        PersonAliasGuids = PersonAliasGuids
                    };
                }

                public static implicit operator RecipientQueryOptions( RecipientQueryOptionsBuilder builder )
                {
                    return builder.Build();
                }
            }
        }

        private List<CommunicationEntryRecipientBag> GetRecipientBags( RockContext rockContext, RecipientQueryOptions options )
        {
            return ConvertToBags( GetRecipientQuery( rockContext, options ) );
        }

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

        private List<CommunicationEntryRecipientBag> ConvertToBags( IEnumerable<CommunicationEntryRecipientDto> dataEntries )
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
                foreach ( var data in dataEntries )
                {
                    yield return ConvertToBag( data );
                }
            }

            return ConvertAll().ToList();
        }

        private class BulkConfig
        {
            public bool IsVisible { get; set; }

            private bool _isBulk;
            public bool IsBulk { get => _isBulk || this.IsForced; set => _isBulk = value; }

            public bool IsForced { get; set; }
        }

        private class BulkConfigService
        {
            private readonly CommunicationEntry _block;

            public bool IsFullMode => _block.IsFullMode;
            public bool IsDefaultAsBulkEnabled => _block.DefaultAsBulk;
            public bool IsPersonPageParameterEnabled => _block.IsPersonPageParameterEnabled;
            public int? PersonOrPersonIdPageParameter => _block.PersonOrPersonIdPageParameter;
            public bool IsSendSimpleAsBulkEnabled => _block.IsSendSimpleAsBulkEnabled;

            public BulkConfigService( CommunicationEntry block )
            {
                _block = block;
            }

            public BulkConfig GetBulkConfig( CommunicationEntryInitializationBox box )
            {
                if ( box.MediumOptions.MediumType == MediumType.Email
                     && MediumContainer.GetComponentByEntityTypeId( box.MediumOptions.MediumEntityTypeId ) is Rock.Communication.Medium.Email emailMediumComponent
                     && emailMediumComponent.IsBulkEmailThresholdExceeded( box.Communication?.Recipients?.Count() ?? 0 ) )
                {
                    return new BulkConfig
                    {
                        // Hide when bulk mode is forced.
                        IsVisible = false,

                        // Force bulk communication since the recipient count has exceeded the threshold.
                        IsForced = true,

                        IsBulk = true,
                    };
                }
                else
                {
                    return new BulkConfig
                    {
                        // The bulk option is only visible if full mode is set.
                        IsVisible = this.IsFullMode,

                        // Do not force bulk communication since the recipient count has not exceeded the threshold.
                        IsForced = false,

                        IsBulk =
                            // Bulk if this is a new communication and "default as bulk" is enabled and there is no person page parameter.
                            (
                                box.Communication.CommunicationId == 0
                                && this.IsDefaultAsBulkEnabled
                                && (
                                    !this.IsPersonPageParameterEnabled
                                    || !this.PersonOrPersonIdPageParameter.HasValue
                                )
                            )
                            // Or if this is simple mode and "send simple as bulk" is enabled (this overrides the current communication value).
                            || (
                                !this.IsFullMode
                                && this.IsSendSimpleAsBulkEnabled
                            )
                            // Or if this is an existing bulk communication.
                            || (
                                box.Communication.CommunicationId != 0
                                && box.Communication.IsBulkCommunication
                            ),
                    };
                }
            }

            public void SetBulkConfig( CommunicationEntryInitializationBox box )
            {
                var bulkConfig = GetBulkConfig( box );
                box.IsBulkVisible = bulkConfig.IsVisible;
                box.IsBulkForced = bulkConfig.IsForced;
                box.Communication.IsBulkCommunication = bulkConfig.IsBulk;
            }
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="communication">The communication.</param>
        private void SetCommunicationData( CommunicationEntryInitializationBox box, Rock.Model.Communication communication, Person sender, RockContext rockContext )
        {
            var communicationRecipientBags = new List<CommunicationEntryRecipientBag>();
            if ( communication == null || communication.Id <= 0 )
            {
                communication = new Rock.Model.Communication
                {
                    EnabledLavaCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands ),
                    FromEmail = sender.Email,
                    FromName = sender.FullName,
                    IsBulkCommunication = this.DefaultAsBulk,
                    SenderPersonAliasId = sender.PrimaryAliasId,
                    Status = CommunicationStatus.Transient,
                };

                box.Title = "New Communication".FormatAsHtmlTitle();
                if ( this.IsPersonPageParameterEnabled )
                {
                    // If either 'Person' or 'PersonId' is specified then add that person to the communication.
                    var personId = this.PersonOrPersonIdPageParameter;

                    if ( personId.HasValue )
                    {
                        communication.IsBulkCommunication = false;
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
                            communicationRecipientBags = GetRecipientBags( rockContext, RecipientQueryOptions.Filter.ByPersonAlias( personAlias.Guid ) );
                        }
                    }
                }
            }
            else
            {
                box.AdditionalMergeFields = communication.AdditionalMergeFields.ToList();
                box.Title = ( communication.Name ?? communication.Subject ?? "New Communication" ).FormatAsHtmlTitle();
                communicationRecipientBags = GetRecipientBags( rockContext, RecipientQueryOptions.Filter.ByCommunication( communication.Id ) );
            }

            // Override bulk communication if not full mode.
            if ( !box.IsFullMode )
            {
                communication.IsBulkCommunication = this.IsSendSimpleAsBulkEnabled;
            }

            var template = communication.CommunicationTemplate;

            // If the communication has no template and a template guid was passed in, then use that template.
            if ( template == null )
            {
                var communicationTemplateGuid = PageParameter( PageParameterKey.TemplateGuid ).AsGuidOrNull();
                if ( communicationTemplateGuid.HasValue )
                {
                    // If a template guid was passed in, it overrides any default template.
                    template = new CommunicationTemplateService( rockContext ).Get( communicationTemplateGuid.Value );
                }
            }

            // If the communication has no template and a template guid was not passed in or is null,
            // then try to use the default template.
            if ( template == null )
            {
                var defaultCommunicationTemplateGuid = GetAttributeValue( AttributeKey.DefaultTemplate ).AsGuidOrNull();
                if ( defaultCommunicationTemplateGuid.HasValue )
                {
                    template = new CommunicationTemplateService( rockContext ).Get( defaultCommunicationTemplateGuid.Value );
                }
            }

            // If a medium ID was provided, then use it for the communication.
            var mediumId = PageParameter( PageParameterKey.MediumId ).AsIntegerOrNull();
            Guid? mediumGuid = null;
            if ( mediumId.HasValue )
            {
                mediumGuid = EntityTypeCache.Get( mediumId.Value, rockContext )?.Guid;

                // Make sure the provided medium Guid is valid.
                if ( mediumGuid.HasValue )
                {
                    mediumGuid = box.Mediums.FirstOrDefault( m => m.Value.AsGuid() == mediumGuid.Value )?.Value.AsGuidOrNull();
                }
            }

            // If a medium ID was not provided or was not associated with a medium,
            // then default to the first of the available mediums.
            if ( !mediumGuid.HasValue )
            {
                mediumGuid = box.Mediums.FirstOrDefault()?.Value.AsGuid();
            }

            // Copy the communication to the initialization box.
            // These props are not covered by the CommunicationEntryHelper.Copy method.
            box.Communication = new CommunicationEntryCommunicationBag
            {
                CommunicationId = communication.Id,
                CommunicationGuid = communication.Guid,
                CommunicationTemplateGuid = template?.Guid,
                FutureSendDateTime = communication.FutureSendDateTime,
                IsBulkCommunication = communication.IsBulkCommunication,
                MediumEntityTypeGuid = mediumGuid ?? Guid.Empty,
                Recipients = communicationRecipientBags,
                SMSAttachmentBinaryFileIds = communication.SMSAttachmentBinaryFileIds.ToList(),
                Status = communication.Status,
            };

            // Copy the rest of the communication details.
            var communicationCopyTarget = new CommunicationDetailsAdapter( box.Communication, rockContext );
            CommunicationEntryHelper.Copy( communication, communicationCopyTarget );

            // Get medium options.
            box.MediumOptions = GetMediumOptions( box.Communication.MediumEntityTypeGuid, sender );

            var bulkConfigService = new BulkConfigService( this );
            bulkConfigService.SetBulkConfig( box );

            // If the communication is transient, then override communication data from the template.
            if ( communication.Status == CommunicationStatus.Transient && template != null )
            {
                // Only override if the template is one of the available templates.
                if ( box.MediumOptions?.Templates?.Any( t => t.Value.AsGuid() == template.Guid ) == true )
                {
                    CommunicationEntryHelper.CopyTemplate( template, communicationCopyTarget, this.RequestContext );
                }
            }

            //box.IsCommunicationSender = communication.SenderPersonAliasId.HasValue && communication.SenderPersonAliasId == GetCurrentPerson()?.PrimaryAliasId;
        }

        ///// <summary>
        ///// Binds the mediums.
        ///// </summary>
        //private void BindMediums()
        //{
        //    var selectedGuids = new List<Guid>();
        //    GetAttributeValue( AttributeKey.Mediums ).SplitDelimitedValues()
        //        .ToList()
        //        .ForEach( v => selectedGuids.Add( v.AsGuid() ) );

        //    var mediums = new Dictionary<int, string>();
        //    foreach ( var item in MediumContainer.Instance.Components.Values )
        //    {
        //        if ( ( !selectedGuids.Any() || selectedGuids.Contains( item.Value.EntityType.Guid ) ) &&
        //            item.Value.IsActive &&
        //            item.Value.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
        //        {
        //            var entityType = item.Value.EntityType;
        //            mediums.Add( entityType.Id, item.Metadata.ComponentName );
        //            if ( !MediumEntityTypeId.HasValue )
        //            {
        //                MediumEntityTypeId = entityType.Id;
        //            }
        //        }
        //    }
        //    if ( !MediumEntityTypeId.HasValue || MediumEntityTypeId.Value == 0 && mediums.Any() )
        //    {
        //        MediumEntityTypeId = mediums.First().Key;
        //    }

        //    LoadTemplates();

        //    divMediums.Visible = mediums.Count() > 1;

        //    rptMediums.DataSource = mediums;
        //    rptMediums.DataBind();
        //}

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

        ///// <summary>
        ///// Binds the recipients.
        ///// </summary>
        //private void BindRecipients()
        //{
        //    int recipientCount = Recipients.Count();
        //    lNumRecipients.Text = recipientCount.ToString( "N0" ) +
        //        ( recipientCount == 1 ? " Person" : " People" );

        //    // Reset the PersonPicker control selection.
        //    ppAddPerson.SetValue( null );
        //    ppAddPerson.PersonName = "Add Person";

        //    int displayCount = int.MaxValue;

        //    if ( !ShowAllRecipients )
        //    {
        //        int.TryParse( GetAttributeValue( AttributeKey.DisplayCount ), out displayCount );
        //    }

        //    if ( displayCount > 0 && displayCount < Recipients.Count )
        //    {
        //        rptRecipients.DataSource = Recipients.Take( displayCount ).ToList();
        //        lbShowAllRecipients.Visible = true;
        //    }
        //    else
        //    {
        //        rptRecipients.DataSource = Recipients.ToList();
        //        lbShowAllRecipients.Visible = false;
        //    }

        //    lbRemoveAllRecipients.Visible = IsFullMode && Recipients.Where( r => r.Status == CommunicationRecipientStatus.Pending ).Any();

        //    rptRecipients.DataBind();

        //    StringBuilder rStatus = new StringBuilder();

        //    CheckApprovalRequired( Recipients.Count );
        //}

        ///// <summary>
        ///// Shows the medium.
        ///// </summary>
        //private MediumControl LoadMediumControl( bool setData )
        //{
        //    if ( setData )
        //    {
        //        phContent.Controls.Clear();
        //    }

        //    // The component to load control for
        //    MediumComponent component = null;
        //    string mediumName = string.Empty;

        //    // Get the current medium type
        //    EntityTypeCache entityType = null;
        //    if ( MediumEntityTypeId.HasValue )
        //    {
        //        entityType = EntityTypeCache.Get( MediumEntityTypeId.Value );
        //    }

        //    foreach ( var serviceEntry in MediumContainer.Instance.Components )
        //    {
        //        var mediumComponent = serviceEntry.Value;

        //        // Default to first component
        //        if ( component == null )
        //        {
        //            component = mediumComponent.Value;
        //            mediumName = mediumComponent.Metadata.ComponentName + " ";
        //        }

        //        // If invalid entity type, exit (and use first component found)
        //        if ( entityType == null )
        //        {
        //            break;
        //        }
        //        else if ( entityType.Id == mediumComponent.Value.EntityType.Id )
        //        {
        //            component = mediumComponent.Value;
        //            mediumName = mediumComponent.Metadata.ComponentName + " ";
        //            break;
        //        }
        //    }

        //    if ( component != null )
        //    {
        //        var mediumControl = component.GetControl( !IsFullMode );
        //        if ( mediumControl is Rock.Web.UI.Controls.Communication.Email )
        //        {
        //            ( (Rock.Web.UI.Controls.Communication.Email)mediumControl ).AllowCcBcc = GetAttributeValue( AttributeKey.AllowCcBcc ).AsBoolean();
        //        }
        //        else if ( mediumControl is Rock.Web.UI.Controls.Communication.Sms )
        //        {
        //            var allowedSmsNumbersGuids = GetAttributeValue( AttributeKey.AllowedSMSNumbers ).SplitDelimitedValues( true ).AsGuidList();

        //            ( ( Sms ) mediumControl ).SelectedNumbers = SystemPhoneNumberCache.All()
        //                .Where( spn => spn.IsAuthorized( Authorization.VIEW, this.CurrentPerson ) && allowedSmsNumbersGuids.ContainsOrEmpty( spn.Guid ) )
        //                .Select( spn => spn.Guid )
        //                .ToList();
        //        }

        //        mediumControl.ID = "commControl";
        //        mediumControl.IsTemplate = false;
        //        mediumControl.AdditionalMergeFields = this.AdditionalMergeFields.ToList();
        //        mediumControl.ValidationGroup = btnSubmit.ValidationGroup;
        //        var fupEmailAttachments = ( Rock.Web.UI.Controls.FileUploader ) mediumControl.FindControl( "fupEmailAttachments_commControl" );

        //        if ( fupEmailAttachments != null )
        //        {
        //            if ( !GetAttributeValue( AttributeKey.ShowAttachmentUploader ).AsBoolean() )
        //            {
        //                if ( fupEmailAttachments != null )
        //                {
        //                    fupEmailAttachments.Visible = false;
        //                }
        //            }
        //            else
        //            {
        //                fupEmailAttachments.BinaryFileTypeGuid = this.GetAttributeValue( AttributeKey.AttachmentBinaryFileType ).AsGuidOrNull() ?? Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid();
        //            }
        //        }
        //        // if this is an email with an HTML control and there are block settings to provide updated content directories set them
        //        if ( mediumControl is Rock.Web.UI.Controls.Communication.Email )
        //        {
        //            var htmlControl = (HtmlEditor)mediumControl.FindControl( "htmlMessage_commControl" );

        //            if ( htmlControl != null )
        //            {
        //                if ( GetAttributeValue( AttributeKey.DocumentRootFolder ).IsNotNullOrWhiteSpace() )
        //                {
        //                    htmlControl.DocumentFolderRoot = GetAttributeValue( AttributeKey.DocumentRootFolder );
        //                }

        //                if ( GetAttributeValue( AttributeKey.ImageRootFolder ).IsNotNullOrWhiteSpace() )
        //                {
        //                    htmlControl.ImageFolderRoot = GetAttributeValue( AttributeKey.ImageRootFolder );
        //                }

        //                if ( GetAttributeValue( AttributeKey.UserSpecificFolders ).AsBooleanOrNull().HasValue )
        //                {
        //                    htmlControl.UserSpecificRoot = GetAttributeValue( AttributeKey.UserSpecificFolders ).AsBoolean();
        //                }
        //            }
        //        }

        //        phContent.Controls.Add( mediumControl );

        //        if ( setData )
        //        {
        //            mediumControl.SetFromCommunication( CommunicationData );
        //        }

        //        // Set the medium in case it wasn't already set or the previous component type was not found
        //        MediumEntityTypeId = component.EntityType.Id;

        //        if ( component.Transport == null || !component.Transport.IsActive )
        //        {
        //            nbInvalidTransport.Text = string.Format( "The {0}medium does not have an active transport configured. The communication will not be delivered until the transport is configured correctly.", mediumName );
        //            nbInvalidTransport.Visible = true;
        //        }
        //        else
        //        {
        //            nbInvalidTransport.Visible = false;
        //        }

        //        cbBulk.Visible = IsFullMode;

        //        return mediumControl;
        //    }

        //    return null;
        //}

        ///// <summary>
        ///// Initializes the control with current persons information if this is first time that this medium is being viewed
        ///// </summary>
        ///// <param name="control">The control.</param>
        //private void InitializeControl( MediumControl control )
        //{
        //    if ( MediumEntityTypeId.HasValue && !ViewedEntityTypes.Contains( MediumEntityTypeId.Value ) )
        //    {
        //        if ( control != null && CurrentPerson != null )
        //        {
        //            control.InitializeFromSender( CurrentPerson );
        //        }

        //        ViewedEntityTypes.Add( MediumEntityTypeId.Value );
        //    }
        //}

        //private MediumControl GetMediumControl()
        //{
        //    if ( phContent.Controls.Count == 1 )
        //    {
        //        return phContent.Controls[0] as MediumControl;
        //    }
        //    return null;
        //}

        ///// <summary>
        ///// Updates communication data.
        ///// </summary>
        //private void UpdateCommunicationData( MediumComponent medium, CommunicationDetails communicationDetails, CommunicationEntryCommunicationBag bag )
        //{
        //    if ( mediumControl != null )
        //    {
        //        // If using simple mode, the control should be re-initialized from sender since sender fields
        //        // are not presented for editing and user shouldn't be able to change them
        //        if ( !IsFullMode && CurrentPerson != null )
        //        {
        //            mediumControl.InitializeFromSender( CurrentPerson );
        //        }

        //        mediumControl.UpdateCommunication( CommunicationData );
        //    }
        //}

        ///// <summary>
        ///// Determines whether approval is required, and sets the submit button text appropriately
        ///// </summary>
        ///// <param name="communication">The communication.</param>
        ///// <returns>
        /////   <c>true</c> if [is approval required] [the specified communication]; otherwise, <c>false</c>.
        ///// </returns>
        //private bool CheckApprovalRequired( int numberOfRecipients )
        //{
        //    int maxRecipients = int.MaxValue;
        //    int.TryParse( GetAttributeValue( AttributeKey.MaximumRecipients ), out maxRecipients );
        //    bool approvalRequired = numberOfRecipients > maxRecipients;

        //    if ( _editingApproved )
        //    {
        //        btnSubmit.Text = "Save";
        //    }
        //    else
        //    {
        //        btnSubmit.Text = ( approvalRequired && !IsUserAuthorized( "Approve" ) ? "Submit" : "Send" ) + " Communication";
        //    }

        //    return approvalRequired;
        //}

        private Lazy<T> CreateLazy<T>( Func<T> lazyInitializer )
        {
            return new Lazy<T>( lazyInitializer );
        }

        /// <summary>
        /// Updates a communication model with the user-entered values
        /// </summary>
        /// <param name="communicationService">The service.</param>
        /// <returns></returns>
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
            // TODO JMH Do we try and load the communication using the bag.CommunicationGuid?
            var currentRecipients = CreateLazy(
                () =>
                {
                    return communication.GetRecipientsQry( rockContext )
                        .Select( r => new
                        {
                            Recipient = r,
                            PersonAliasGuid = r.PersonAlias.Guid
                        } )
                        .ToList();
                } );

            if ( !bag.CommunicationGuid.IsEmpty() )
            {
                communication = communicationService.Get( bag.CommunicationGuid );
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
                if ( !currentRecipients.Value.Any( currentRecipient => newRecipientPersonAliasGuids.Contains( currentRecipient.PersonAliasGuid ) ) )
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

            communication.EnabledLavaCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands );
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

            // delete any attachments that are no longer included
            // TODO JMH Does the bag have Guids? If so, we need to map the Communication ids to Guids or vice-versa.
            foreach ( var attachment in communication.Attachments.Where( a => !bag.EmailAttachmentBinaryFiles.Any( bagFile => bagFile.Value.AsGuid() == a.BinaryFile.Guid ) ).ToList() )
            {
                communication.Attachments.Remove( attachment );
                communicationAttachmentService.Delete( attachment );
            }

            // add any new attachments that were added
            // TODO JMH Does the bag have Guids? If so, we need to map the Communication ids to Guids or vice-versa.
            if ( bag.EmailAttachmentBinaryFiles?.Any() == true )
            {
                var guids = bag.EmailAttachmentBinaryFiles.Select( bf => bf.Value.AsGuid() ).Where( g => !g.IsEmpty() ).ToList();
                var attachmentIdMap = new BinaryFileService( rockContext )
                    .Queryable()
                    .Where( b => guids.Contains( b.Guid ) )
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
                        communication.AddAttachment( new CommunicationAttachment { BinaryFileId = attachmentBinaryFileId.Value }, CommunicationType.Email );
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
                communication.Subject = communication.Subject.SanitizeLava();
                communication.BCCEmails = communication.BCCEmails.SanitizeLava();
                communication.CCEmails = communication.CCEmails.SanitizeLava();
                communication.FromEmail = communication.FromEmail.SanitizeLava();
                communication.FromName = communication.FromName.SanitizeLava();
                communication.ReplyToEmail = communication.ReplyToEmail.SanitizeLava();
            }

            return communication;
        }

        ///// <summary>
        ///// Shows the result.
        ///// </summary>
        ///// <param name="message">The message.</param>
        ///// <param name="communication">The communication.</param>
        //private void ShowResult( string message, Rock.Model.Communication communication )
        //{
        //    ShowStatus( communication );

        //    pnlEdit.Visible = false;

        //    nbResult.Text = message;

        //    CurrentPageReference.Parameters.AddOrReplace( PageParameterKey.CommunicationId, communication.Id.ToString() );
        //    hlViewCommunication.NavigateUrl = CurrentPageReference.BuildUrl();

        //    // only show the Link if there is a CommunicationDetail block type on this page
        //    hlViewCommunication.Visible = this.PageCache.Blocks.Any( a => a.BlockType.Guid == Rock.SystemGuid.BlockType.COMMUNICATION_DETAIL.AsGuid() );

        //    pnlResult.Visible = true;

        //    ScriptManager.RegisterStartupScript(
        //        Page,
        //        GetType(),
        //        "scrollToResults",
        //        "scrollToResults();",
        //        true );

        //}

        #endregion

        #region Helper Classes

        private interface ICommunicationAttachments
        {
            IEnumerable<AttachmentDto> Attachments { get; set; }

            void SetAttachments( IEnumerable<int> binaryFileIds );
        }

        private static class CommunicationEntryHelper
        {
            public static void Copy( ICommunicationDetails source, ICommunicationDetails target )
            {
                CommunicationDetails.Copy( source, target );

                if ( target is ICommunicationAttachments attachmentsTarget )
                {
                    attachmentsTarget.SetAttachments( source.EmailAttachmentBinaryFileIds );
                }
            }

            // TODO JMH Is this needed?? Do we only need a method that converts a CommunicationTemplate to a CommunicationEntryCommunicationBag?
            public static void CopyTemplate( CommunicationTemplate source, ICommunicationDetails target, RockRequestContext requestContext )
            {
                // Save what was entered for FromEmail and FromName in case the template blanks it out.
                var originalFromEmail = target.FromEmail;
                var originalFromName = target.FromName;

                Copy( source, target );

                // Resolve lava-enabled fields from the template.
                target.FromName = source.FromName.ResolveMergeFields( requestContext.GetCommonMergeFields() );
                target.FromEmail = source.FromEmail.ResolveMergeFields( requestContext.GetCommonMergeFields() );

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
            public string PushData { get => _bag.PushData; set => _bag.PushData = value; }
            public int? PushImageBinaryFileId { get => _bag.PushImageBinaryFileId; set => _bag.PushImageBinaryFileId = value; }
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
            public string PushOpenMessage { get => _bag.PushOpenMessage; set => _bag.PushOpenMessage = value; }
            public int? SmsFromSystemPhoneNumberId { get => _bag.SmsFromSystemPhoneNumberId; set => _bag.SmsFromSystemPhoneNumberId = value; }
            public string SMSMessage { get => _bag.SMSMessage; set => _bag.SMSMessage = value; }

            public IEnumerable<AttachmentDto> Attachments
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
            public IEnumerable<int> EmailAttachmentBinaryFileIds { get => this.Attachments?.Select( a => a.Id ); }
            public DateTimeOffset? FutureSendDateTime { get => _bag.FutureSendDateTime; set => _bag.FutureSendDateTime = value; }
            public int? SMSFromDefinedValueId { get => _bag.SMSFromDefinedValueId; set => _bag.SMSFromDefinedValueId = value; }
            public IEnumerable<int> SMSAttachmentBinaryFileIds { get => _bag.SMSAttachmentBinaryFileIds; set => _bag.SMSAttachmentBinaryFileIds = value?.ToList(); }
            public CommunicationStatus Status { get => _bag.Status; set => _bag.Status = value; }

            /// <inheritdoc/>
            public void SetAttachments( IEnumerable<int> binaryFileIds )
            {
                this.Attachments = AttachmentHelper.GetAttachments( _rockContext, binaryFileIds );
            }
        }

        #endregion
    }
}