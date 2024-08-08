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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Tasks;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web.UI.Controls.Communication;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    /// User control for creating a new communication.  This block should be used on same page as the CommunicationDetail block and only visible when editing a new or transient communication
    /// </summary>
    [DisplayName( "Communication Entry" )]
    [Category( "Communication" )]
    [Description( "Used for creating and sending a new communications such as email, SMS, etc. to recipients." )]

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
    [BooleanField("Enable Person Parameter",
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
    [BooleanField( "Allow CC/Bcc",
        Key = AttributeKey.AllowCcBcc,
        Description = "Allow CC and Bcc addresses to be entered for email communications?",
        DefaultBooleanValue = false,
        Order = 9 )]
    [BooleanField( "Show Attachment Uploader",
        Key = AttributeKey.ShowAttachmentUploader,
        Description = "Should the attachment uploader be shown for email communications.",
        DefaultBooleanValue = true,
        Order = 10 )]
    [SystemPhoneNumberField( "Allowed SMS Numbers",
        Key = AttributeKey.AllowedSMSNumbers,
        Description = "Set the allowed FROM numbers to appear when in SMS mode (if none are selected all numbers will be included).",
        IsRequired = false,
        AllowMultiple = true,
        Order = 11 )]
    [BooleanField( "Simple Communications Are Bulk",
        Key = AttributeKey.SendSimpleAsBulk,
        Description = "Should simple mode communications be sent as a bulk communication?",
        DefaultBooleanValue = true,
        Order = 12 )]
    [BinaryFileTypeField( "Attachment Binary File Type",
        Key = AttributeKey.AttachmentBinaryFileType,
        Description = "The FileType to use for files that are attached to an sms or email communication",
        IsRequired = true,
        DefaultBinaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.COMMUNICATION_ATTACHMENT,
        Order = 13 )]
    [BooleanField( "Default As Bulk",
        Key = AttributeKey.DefaultAsBulk,
        Description = "Should new entries be flagged as bulk communication by default?",
        DefaultBooleanValue = false,
        Order = 14 )]
    [TextField( "Document Root Folder",
        Key =  AttributeKey.DocumentRootFolder,
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
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.COMMUNICATION_ENTRY )]
    public partial class CommunicationEntry : RockBlock
    {
        #region Attribute Keys

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

        #endregion Attribute Keys

        private static class PageParameterKey
        {
            public const string CommunicationId = "CommunicationId";
            public const string Edit = "Edit";
            public const string Person = "Person";
            public const string PersonId = "PersonId";
            public const string TemplateGuid = "TemplateGuid";
            public const string MediumId = "MediumId";
        }

        #region Fields

        private bool _fullMode = true;
        private bool _editingApproved = false;
        private bool _isBulkCommunicationForced = false;

        #endregion

        #region Events

        private delegate void OnPropertyChangedHandler( object sender, PropertyChangedEventArgs e );

        private event OnPropertyChangedHandler OnPropertyChanged;

        /// <summary>
        /// Sets a view state property and raises the <see cref="OnPropertyChanged"/> event.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="propertyName">Name of the property.</param>
        private void SetViewState<T>( T value, [CallerMemberName] string propertyName = null )
        {
            ViewState[propertyName] = value;
            RaisePropertyChanged( propertyName );
        }

        /// <summary>
        /// Raises the <see cref="OnPropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        private void RaisePropertyChanged( [CallerMemberName] string propertyName = null )
        {
            OnPropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        #endregion

        #region Properties

        protected int? CommunicationId
        {
            get { return ViewState["CommunicationId"] as int?; }
            set { ViewState["CommunicationId"] = value; }
        }

        /// <summary>
        /// Gets or sets the medium entity type id.
        /// </summary>
        /// <remarks>
        /// The <see cref="OnPropertyChanged"/> event is raised when the value changes.
        /// </remarks>
        /// <value>
        /// The medium entity type id.
        /// </value>
        protected int? MediumEntityTypeId
        {
            get { return ViewState["MediumEntityTypeId"] as int?; }
            set { SetViewState( value ); }
        }

        /// <summary>
        /// Gets or sets the entity types that have been viewed. If entity type has not been viewed, the control will be initialized to current person
        /// </summary>
        /// <value>
        /// The initialized entity types.
        /// </value>
        protected List<int> ViewedEntityTypes
        {
            get
            {
                var viewedEntityTypes = ViewState["ViewedEntityTypes"] as List<int>;
                if ( viewedEntityTypes == null )
                {
                    viewedEntityTypes = new List<int>();
                    ViewedEntityTypes = viewedEntityTypes;
                }
                return viewedEntityTypes;
            }
            set { ViewState["ViewedEntityTypes"] = value; }
        }

        /// <summary>
        /// Gets or sets the recipients.
        /// </summary>
        /// <remarks>
        /// The <see cref="OnPropertyChanged"/> event is raised when the value changes or when items are added/removed.
        /// </remarks>
        /// <value>
        /// The recipient ids.
        /// </value>
        protected ObservableCollection<Recipient> Recipients
        {
            get
            {
                if ( !( ViewState[nameof( Recipients )] is ObservableCollection<Recipient> recipients ) )
                {
                    recipients = new ObservableCollection<Recipient>();
                    recipients.CollectionChanged += Recipients_CollectionChanged;

                    SetViewState( recipients );
                }
                else
                {
                    // Make sure the event handlers are set up in case the recipients were deserialized from view state.
                    recipients.CollectionChanged -= Recipients_CollectionChanged;
                    recipients.CollectionChanged += Recipients_CollectionChanged;
                } 
                
                return recipients;
            }

            set
            {
                if ( ViewState[nameof( Recipients )] is ObservableCollection<Recipient> recipients )
                {
                    // Stop listening for changes to the collection.
                    recipients.CollectionChanged -= Recipients_CollectionChanged;
                }

                // Start listening for changes to the collection.
                if ( value != null )
                {
                    value.CollectionChanged -= Recipients_CollectionChanged;
                    value.CollectionChanged += Recipients_CollectionChanged;
                }

                SetViewState( value );
            }
        }

        /// <summary>
        /// Handles the CollectionChanged event of the Recipients collection.
        /// </summary>
        /// <remarks>
        /// When an item is added, removed, or when the collection is cleared,
        /// the <see cref="OnPropertyChanged"/> event is raised indicating
        /// the <see cref="Recipients"/> property changed.
        /// </remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void Recipients_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            RaisePropertyChanged( nameof( Recipients ) );
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show all recipients].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show all recipients]; otherwise, <c>false</c>.
        /// </value>
        protected bool ShowAllRecipients
        {
            get { return ViewState["ShowAllRecipients"] as bool? ?? false; }
            set { ViewState["ShowAllRecipients"] = value; }
        }

        /// <summary>
        /// Gets or sets the communication data.
        /// </summary>
        /// <value>
        /// The communication data.
        /// </value>
        protected CommunicationDetails CommunicationData
        {
            get
            {
                var communicationData = ViewState["CommunicationData"] as CommunicationDetails;
                if ( communicationData == null )
                {
                    communicationData = new CommunicationDetails();
                    ViewState["CommunicationData"] = communicationData;
                }
                return communicationData;
            }

            set { ViewState["CommunicationData"] = value; }
        }

        /// <summary>
        /// Gets or sets any additional merge fields.
        /// </summary>
        public List<string> AdditionalMergeFields
        {
            get
            {
                var mergeFields = ViewState["AdditionalMergeFields"] as List<string>;
                if ( mergeFields == null )
                {
                    mergeFields = new List<string>();
                    ViewState["AdditionalMergeFields"] = mergeFields;
                }
                return mergeFields;
            }

            set { ViewState["AdditionalMergeFields"] = value; }
        }

        public bool ApproverEditing
        {
            get { return ViewState["ApproverEditing"] as bool? ?? false; }
            set { ViewState["ApproverEditing"] = value; }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string script = @"
    $('a.remove-all-recipients').on('click', function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to remove all of the pending recipients from this communication?', function (result) {
            if (result) {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( lbRemoveAllRecipients, lbRemoveAllRecipients.GetType(), "ConfirmRemoveAll", script, true );
            
            this.OnPropertyChanged -= CommunicationEntry_OnPropertyChanged;
            this.OnPropertyChanged += CommunicationEntry_OnPropertyChanged;

            string mode = GetAttributeValue( AttributeKey.Mode );
            _fullMode = string.IsNullOrWhiteSpace( mode ) || mode != "Simple";
            ppAddPerson.Visible = _fullMode;
            ShowHideIsBulkOption();
            
            ddlTemplate.Visible = _fullMode;
            dtpFutureSend.Visible = _fullMode;
            btnTest.Visible = _fullMode;
            btnSave.Visible = _fullMode;

            _editingApproved = PageParameter( PageParameterKey.Edit ).AsBoolean() && IsUserAuthorized( "Approve" );
            if( PageParameter( PageParameterKey.MediumId ).IsNotNullOrWhiteSpace() )
            {
                MediumEntityTypeId = PageParameter( PageParameterKey.MediumId ).AsIntegerOrNull();
            }
        }

        /// <summary>
        /// Handles the <see cref="OnPropertyChanged"/> event of the CommunicationEntry control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void CommunicationEntry_OnPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            var isBulkViewStateDependencies = new List<string> { nameof( Recipients ), nameof( MediumEntityTypeId ) };
            if ( isBulkViewStateDependencies.Contains( e.PropertyName ) )
            {
                // If one of the "Is Bulk" dependencies change, then show or hide the bulk option.
                ShowHideIsBulkOption();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {            
            this.OnPropertyChanged -= CommunicationEntry_OnPropertyChanged;
            this.OnPropertyChanged += CommunicationEntry_OnPropertyChanged;

            nbTestResult.Visible = false;

            if ( Page.IsPostBack )
            {
                LoadMediumControl( false );
            }
            else
            {
                ShowAllRecipients = false;

                // Check if CommunicationDetail has already loaded existing communication
                var communication = RockPage.GetSharedItem( "Communication" ) as Rock.Model.Communication;
                if ( communication == null )
                {
                    // If not, check page parameter for existing communication
                    int? communicationId = PageParameter( PageParameterKey.CommunicationId ).AsIntegerOrNull();
                    if ( communicationId.HasValue )
                    {
                        communication = new CommunicationService( new RockContext() ).Get( communicationId.Value );
                    }
                }
                else
                {
                    CommunicationId = communication.Id;
                }

                if ( communication == null )
                {
                    // if this is a new communication, create a communication object temporarily so we can do the auth and edit logic
                    communication = new Rock.Model.Communication() { Status = CommunicationStatus.Transient };
                    communication.CreatedByPersonAlias = this.CurrentPersonAlias;
                    communication.CreatedByPersonAliasId = this.CurrentPersonAliasId;
                    communication.SenderPersonAlias = this.CurrentPersonAlias;
                    communication.SenderPersonAliasId = CurrentPersonAliasId;
                }

                // If viewing a new, transient, draft, or are the approver of a pending-approval communication, use this block
                // otherwise, set this block visible=false and if there is a communication detail block on this page, it'll be shown instead
                CommunicationStatus[] editableStatuses = new CommunicationStatus[] { CommunicationStatus.Transient, CommunicationStatus.Draft, CommunicationStatus.Denied };
                if ( editableStatuses.Contains( communication.Status ) || ( communication.Status == CommunicationStatus.PendingApproval && _editingApproved ) )
                {
                    // Make sure they are authorized to edit, or the owner, or the approver/editor
                    bool isAuthorizedEditor = communication.IsAuthorized( Rock.Security.Authorization.EDIT, CurrentPerson );
                    bool isCreator = ( communication.CreatedByPersonAlias != null && CurrentPersonId.HasValue && communication.CreatedByPersonAlias.PersonId == CurrentPersonId.Value );
                    bool isApprovalEditor = communication.Status == CommunicationStatus.PendingApproval && _editingApproved;

                    // If communication was just created only for authorization, set it to null so that Showing of details works correctly.
                    if ( communication.Id == 0 )
                    {
                        communication = null;
                    }

                    if ( isAuthorizedEditor || isCreator || isApprovalEditor )
                    {
                        // communication is either new or ok to edit
                        ShowDetail( communication );
                    }
                    else
                    {
                        // not authorized, so hide this block
                        this.Visible = false;
                    }
                }
                else
                {
                    // Not an editable communication, so hide this block. If there is a CommunicationDetail block on this page, it'll be shown instead
                    this.Visible = false;
                }
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            BindRecipients();
        }

        #endregion

        #region Events

        protected void ddlTemplate_SelectedIndexChanged( object sender, EventArgs e )
        {
            GetMediumData();
            int? templateId = ddlTemplate.SelectedValue.AsIntegerOrNull();
            if ( templateId.HasValue )
            {
                GetTemplateData( templateId.Value );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbMedium control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbMedium_Click( object sender, EventArgs e )
        {
            GetMediumData();
            var linkButton = sender as LinkButton;
            if ( linkButton != null )
            {
                int mediumId = int.MinValue;
                if ( int.TryParse( linkButton.CommandArgument, out mediumId ) )
                {
                    MediumEntityTypeId = mediumId;
                    BindMediums();

                    var control = LoadMediumControl( true );
                    InitializeControl( control );
                }
            }
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppAddPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ppAddPerson_SelectPerson( object sender, EventArgs e )
        {
            if ( ppAddPerson.PersonId.HasValue )
            {
                if ( !Recipients.Any( r => r.PersonId == ppAddPerson.PersonId.Value ) )
                {
                    var context = new RockContext();
                    var Person = new PersonService( context ).Get( ppAddPerson.PersonId.Value );
                    if ( Person != null )
                    {
                        var HasPersonalDevice = new PersonalDeviceService( context ).Queryable()
                            .Where( pd =>
                                pd.PersonAliasId.HasValue &&
                                pd.PersonAliasId == Person.PrimaryAliasId &&
                                pd.NotificationsEnabled )
                            .Any();
                        Recipients.Add( new Recipient( Person, Person.PhoneNumbers.Any( a => a.IsMessagingEnabled ), HasPersonalDevice, CommunicationRecipientStatus.Pending ) );
                        ShowAllRecipients = true;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptRecipients control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs" /> instance containing the event data.</param>
        protected void rptRecipients_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            // Hide the remove button for any recipient that is not pending.
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var recipient = e.Item.DataItem as Recipient;
                if ( recipient != null )
                {
                    var lRecipientName = e.Item.FindControl( "lRecipientName" ) as Literal;
                    if ( lRecipientName != null )
                    {
                        string textClass = string.Empty;
                        string textTooltip = string.Empty;

                        if ( recipient.IsDeceased )
                        {
                            textClass = "text-danger";
                            textTooltip = "Deceased";
                        }
                        else
                        {
                            if ( MediumEntityTypeId == EntityTypeCache.Get( "Rock.Communication.Medium.Email" ).Id )
                            {
                                if ( string.IsNullOrWhiteSpace( recipient.Email ) )
                                {
                                    textClass = "text-danger";
                                    textTooltip = "No Email." + recipient.EmailNote;
                                }
                                else if ( !recipient.IsEmailActive )
                                {
                                    // if email is not active, show reason why as tooltip
                                    textClass = "text-danger";
                                    textTooltip = "Email is Inactive. " + recipient.EmailNote;
                                }
                                else
                                {
                                    // Email is active
                                    if ( recipient.EmailPreference != EmailPreference.EmailAllowed )
                                    {
                                        textTooltip = Recipient.PreferenceMessage( recipient );

                                        if ( recipient.EmailPreference == EmailPreference.NoMassEmails )
                                        {
                                            textClass = "js-no-bulk-email";
                                            if ( cbBulk.Checked || _isBulkCommunicationForced )
                                            {
                                                // This is a bulk email and user does not want bulk emails
                                                textClass += " text-danger";
                                            }
                                        }
                                        else
                                        {
                                            // Email preference is 'Do Not Email'
                                            textClass = "text-danger";
                                        }
                                    }
                                }
                            }
                            else if ( MediumEntityTypeId == EntityTypeCache.Get( "Rock.Communication.Medium.Sms" ).Id )
                            {
                                if ( !recipient.HasSmsNumber )
                                {
                                    // No SMS Number
                                    textClass = "text-danger";
                                    textTooltip = "No phone number with SMS enabled.";
                                }
                            }
                            else if ( MediumEntityTypeId == EntityTypeCache.Get( "Rock.Communication.Medium.PushNotification" ).Id )
                            {
                                if ( !recipient.HasNotificationsEnabled )
                                {
                                    // No Notifications Enabled
                                    textClass = "text-danger";
                                    textTooltip = "Notifications not enabled for this number.";
                                }
                            }
                        }

                        lRecipientName.Text = String.Format( "<span data-toggle=\"tooltip\" data-placement=\"top\" title=\"{0}\" class=\"{1}\">{2}</span>",
                            textTooltip, textClass, recipient.PersonName );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptRecipients control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs" /> instance containing the event data.</param>
        protected void rptRecipients_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int personId = int.MinValue;
            if ( int.TryParse( e.CommandArgument.ToString(), out personId ) )
            {
                Recipients = new ObservableCollection<Recipient>( Recipients.Where( r => r.PersonId != personId ) );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbShowAllRecipients control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbShowAllRecipients_Click( object sender, EventArgs e )
        {
            ShowAllRecipients = true;
        }

        /// <summary>
        /// Handles the Click event of the lbRemoveAllRecipients control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbRemoveAllRecipients_Click( object sender, EventArgs e )
        {
            Recipients = new ObservableCollection<Recipient>( Recipients.Where( r => r.Status != CommunicationRecipientStatus.Pending ).ToList() );
        }

        /// <summary>
        /// Handles the ServerValidate event of the valRecipients control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="ServerValidateEventArgs" /> instance containing the event data.</param>
        protected void valRecipients_ServerValidate( object source, ServerValidateEventArgs args )
        {
            args.IsValid = Recipients.Any();
        }

        /// <summary>
        /// Handles the Click event of the btnSubmit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnTest_Click( object sender, EventArgs e )
        {
            ValidateFutureDelayDateTime();
            if ( Page.IsValid && CurrentPersonAliasId.HasValue && cvDelayDateTime.IsValid )
            {
                // Get existing or new communication record
                var communication = UpdateCommunication( new RockContext() );
                if ( communication != null )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        // Using a new context (so that changes in the UpdateCommunication() are not persisted )
                        var testCommunication = communication.CloneWithoutIdentity();
                        testCommunication.CreatedByPersonAliasId = this.CurrentPersonAliasId;
                        testCommunication.CreatedByPersonAlias = new PersonAliasService( rockContext ).Queryable().Where( a => a.Id == this.CurrentPersonAliasId.Value ).Include( a => a.Person ).FirstOrDefault();
                        testCommunication.EnabledLavaCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands );
                        testCommunication.FutureSendDateTime = null;
                        testCommunication.Status = CommunicationStatus.Approved;
                        testCommunication.ReviewedDateTime = RockDateTime.Now;
                        testCommunication.ReviewerPersonAliasId = CurrentPersonAliasId;
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
                        testRecipient.PersonAliasId = CurrentPersonAliasId.Value;
                        testRecipient.MediumEntityTypeId = MediumEntityTypeId;
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

                        if ( testRecipient != null && testRecipient.Status == CommunicationRecipientStatus.Failed && testRecipient.PersonAlias != null && testRecipient.PersonAlias.Person != null )
                        {
                            nbTestResult.NotificationBoxType = NotificationBoxType.Danger;
                            nbTestResult.Text = string.Format( "Test communication to <strong>{0}</strong> failed: {1}.", testRecipient.PersonAlias.Person.FullName, testRecipient.StatusNote );
                        }
                        else
                        {
                            nbTestResult.NotificationBoxType = NotificationBoxType.Success;
                            nbTestResult.Text = "Test communication has been sent.";
                        }
                        nbTestResult.Visible = true;

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
                    }
                }
            }
        }


        /// <summary>
        /// Handles the Click event of the btnSubmit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSubmit_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                ValidateFutureDelayDateTime();
                if ( !cvDelayDateTime.IsValid )
                {
                    return;
                }
                var rockContext = new RockContext();
                var communication = UpdateCommunication( rockContext );

                if ( communication != null )
                {
                    var mediumControl = GetMediumControl();
                    if ( mediumControl != null )
                    {
                        mediumControl.OnCommunicationSave( rockContext );
                    }

                    if ( _editingApproved && communication.Status == CommunicationStatus.PendingApproval )
                    {
                        rockContext.SaveChanges();

                        // Redirect back to same page without the edit param
                        var pageRef = new Rock.Web.PageReference();
                        pageRef.PageId = CurrentPageReference.PageId;
                        pageRef.RouteId = CurrentPageReference.RouteId;
                        pageRef.Parameters = new Dictionary<string, string>();
                        pageRef.Parameters.Add( PageParameterKey.CommunicationId, communication.Id.ToString() );
                        Response.Redirect( pageRef.BuildUrl() );
                        Context.ApplicationInstance.CompleteRequest();
                    }
                    else
                    {
                        string message = string.Empty;

                        // Save the communication prior to checking recipients.
                        communication.Status = CommunicationStatus.Draft;
                        rockContext.SaveChanges();

                        if ( CheckApprovalRequired( communication.Recipients.Count() ) && !IsUserAuthorized( "Approve" ) )
                        {
                            communication.Status = CommunicationStatus.PendingApproval;
                            message = "Communication has been submitted for approval.";
                        }
                        else
                        {
                            communication.Status = CommunicationStatus.Approved;
                            communication.ReviewedDateTime = RockDateTime.Now;
                            communication.ReviewerPersonAliasId = CurrentPersonAliasId;

                            if ( communication.FutureSendDateTime.HasValue &&
                                communication.FutureSendDateTime > RockDateTime.Now )
                            {
                                message = string.Format( "Communication will be sent {0}.",
                                    communication.FutureSendDateTime.Value.ToRelativeDateString( 0 ) );
                            }
                            else
                            {
                                message = "Communication has been queued for sending.";
                            }
                        }

                        rockContext.SaveChanges();

                        // send approval email if needed (now that we have a communication id)
                        if ( communication.Status == CommunicationStatus.PendingApproval )
                        {
                            var approvalTransactionMsg = new ProcessSendCommunicationApprovalEmail.Message()
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
                                var transactionMsg = new ProcessSendCommunication.Message()
                                {
                                    CommunicationId = communication.Id
                                };
                                transactionMsg.Send();
                            }
                        }

                        ShowResult( message, communication );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            ValidateFutureDelayDateTime();
            if ( !cvDelayDateTime.IsValid )
            {
                return;
            }
            var rockContext = new RockContext();
            var communication = UpdateCommunication( rockContext );

            if ( communication != null )
            {
                var mediumControl = GetMediumControl();
                if ( mediumControl != null )
                {
                    mediumControl.OnCommunicationSave( rockContext );
                }

                communication.Status = CommunicationStatus.Draft;
                rockContext.SaveChanges();

                ShowResult( "The communication has been saved", communication );
            }
        }

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( _editingApproved )
            {
                var communicationService = new CommunicationService( new RockContext() );
                var communication = communicationService.Get( CommunicationId.Value );
                if ( communication != null && communication.Status == CommunicationStatus.PendingApproval )
                {
                    // Redirect back to same page without the edit param
                    var pageRef = new Rock.Web.PageReference();
                    pageRef.PageId = CurrentPageReference.PageId;
                    pageRef.RouteId = CurrentPageReference.RouteId;
                    pageRef.Parameters = new Dictionary<string, string>();
                    pageRef.Parameters.Add( PageParameterKey.CommunicationId, communication.Id.ToString() );
                    Response.Redirect( pageRef.BuildUrl() );
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="communication">The communication.</param>
        private void ShowDetail( Rock.Model.Communication communication )
        {
            Recipients.Clear();
            int? mediumEntityTypeId = null;
            if ( communication != null && communication.Id > 0 )
            {
                this.AdditionalMergeFields = communication.AdditionalMergeFields.ToList();
                lTitle.Text = ( communication.Name ?? communication.Subject ?? "New Communication" ).FormatAsHtmlTitle();
                var context = new RockContext();
                var personalDeviceService = new PersonalDeviceService( context ).Queryable();
                var recipientList = new CommunicationRecipientService( context )
                    .Queryable()
                    .Where( r => r.CommunicationId == communication.Id )
                    .Select( a => new
                    {
                        a.PersonAlias.Person,
                        PersonHasSMS = a.PersonAlias.Person.PhoneNumbers.Any( p => p.IsMessagingEnabled ),
                        HasPersonalDevice = (
                             personalDeviceService
                                 .Where( pd => pd.PersonAliasId.HasValue && pd.PersonAliasId == a.PersonAliasId )
                                 .Any( pd => pd.NotificationsEnabled )
                         ),
                        a.Status,
                        a.StatusNote,
                        a.OpenedClient,
                        a.OpenedDateTime,
                        a.MediumEntityTypeId
                    } ).ToList();

                mediumEntityTypeId = PageParameter( PageParameterKey.MediumId ).AsIntegerOrNull() ?? recipientList.Where( a => a.MediumEntityTypeId.HasValue ).Select( a => a.MediumEntityTypeId ).FirstOrDefault();
                Recipients = new ObservableCollection<Recipient>( recipientList.Select( recipient => new Recipient( recipient.Person, recipient.PersonHasSMS, recipient.HasPersonalDevice, recipient.Status, recipient.StatusNote, recipient.OpenedClient, recipient.OpenedDateTime ) ).ToList() );
            }
            else
            {
                communication = new Rock.Model.Communication() { Status = CommunicationStatus.Transient };
                communication.SenderPersonAliasId = CurrentPersonAliasId;
                communication.EnabledLavaCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands );
                communication.IsBulkCommunication = _isBulkCommunicationForced || GetAttributeValue( AttributeKey.DefaultAsBulk ).AsBoolean();

                lTitle.Text = "New Communication".FormatAsHtmlTitle();
                if ( GetAttributeValue( AttributeKey.EnablePersonParameter ).AsBoolean() )
                {
                    // if either 'Person' or 'PersonId' is specified add that person to the communication
                    var personId = PageParameter( PageParameterKey.Person ).AsIntegerOrNull()
                        ?? PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull();

                    if ( personId.HasValue )
                    {
                        communication.IsBulkCommunication = _isBulkCommunicationForced;
                        var context = new RockContext();
                        var person = new PersonService( context ).Get( personId.Value );
                        if ( person != null )
                        {
                            var HasPersonalDevice = new PersonalDeviceService( context ).Queryable()
                                .Where( pd => pd.PersonAliasId.HasValue && pd.PersonAliasId == person.PrimaryAliasId && pd.NotificationsEnabled ).Any();
                            Recipients.Add( new Recipient( person, person.PhoneNumbers.Any( p => p.IsMessagingEnabled ), HasPersonalDevice, CommunicationRecipientStatus.Pending, string.Empty, string.Empty, null ) );
                        }
                    }
                }
            }

            CommunicationId = communication.Id;

            BindMediums();
            if ( mediumEntityTypeId.HasValue && !ViewedEntityTypes.Contains( mediumEntityTypeId.Value ) )
            {
                ViewedEntityTypes.Add( mediumEntityTypeId.Value );
            }

            CommunicationData = new CommunicationDetails();
            CommunicationDetails.Copy( communication, CommunicationData );
            CommunicationData.EmailAttachmentBinaryFileIds = communication.EmailAttachmentBinaryFileIds;

            var template = communication.CommunicationTemplate;

            if ( template == null && !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.DefaultTemplate ) ) )
            {
                template = new CommunicationTemplateService( new RockContext() ).Get( GetAttributeValue( AttributeKey.DefaultTemplate ).AsGuid() );
            }

            // If a template guid was passed in, it overrides any default template.
            string templateGuid = PageParameter( PageParameterKey.TemplateGuid );
            if ( !string.IsNullOrEmpty( templateGuid ) )
            {
                var guid = new Guid( templateGuid );
                template = new CommunicationTemplateService( new RockContext() ).Queryable().Where( t => t.Guid == guid ).FirstOrDefault();
            }

            if ( template != null )
            {
                foreach ( ListItem item in ddlTemplate.Items )
                {
                    if ( item.Value == template.Id.ToString() )
                    {
                        item.Selected = true;
                        if ( communication.Status == CommunicationStatus.Transient )
                        {
                            GetTemplateData( template.Id, false );
                        }
                    }
                    else
                    {
                        item.Selected = false;
                    }
                }
            }

            cbBulk.Checked = _isBulkCommunicationForced || communication.IsBulkCommunication;

            if ( !_fullMode )
            {
                cbBulk.Checked = _isBulkCommunicationForced || GetAttributeValue( AttributeKey.SendSimpleAsBulk ).AsBoolean();
            }

            MediumControl control = LoadMediumControl( true );
            InitializeControl( control );

            dtpFutureSend.SelectedDateTime = communication.FutureSendDateTime;

            ShowStatus( communication );
            ShowActions( communication );
        }

        /// <summary>
        /// Binds the mediums.
        /// </summary>
        private void BindMediums()
        {
            var selectedGuids = new List<Guid>();
            GetAttributeValue( AttributeKey.Mediums ).SplitDelimitedValues()
                .ToList()
                .ForEach( v => selectedGuids.Add( v.AsGuid() ) );

            var mediums = new Dictionary<int, string>();
            foreach ( var item in MediumContainer.Instance.Components.Values )
            {
                if ( ( !selectedGuids.Any() || selectedGuids.Contains( item.Value.EntityType.Guid ) ) &&
                    item.Value.IsActive &&
                    item.Value.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    var entityType = item.Value.EntityType;
                    mediums.Add( entityType.Id, item.Metadata.ComponentName );
                    if ( !MediumEntityTypeId.HasValue )
                    {
                        MediumEntityTypeId = entityType.Id;
                    }
                }
            }
            if ( !MediumEntityTypeId.HasValue || MediumEntityTypeId.Value == 0 && mediums.Any() )
            {
                MediumEntityTypeId = mediums.First().Key;
            }

            LoadTemplates();

            divMediums.Visible = mediums.Count() > 1;

            rptMediums.DataSource = mediums;
            rptMediums.DataBind();
        }

        private void LoadTemplates()
        {
            bool visible = false;

            string prevSelection = ddlTemplate.SelectedValue;

            ddlTemplate.Items.Clear();
            ddlTemplate.Items.Add( new ListItem( string.Empty, string.Empty ) );

            if ( MediumEntityTypeId.HasValue )
            {
                var medium = MediumContainer.GetComponentByEntityTypeId( MediumEntityTypeId );
                if ( medium != null )
                {
                    foreach ( var template in new CommunicationTemplateService( new RockContext() )
                        .Queryable().AsNoTracking()
                        .Where( a => a.IsActive )
                        .OrderBy( t => t.Name ) )
                    {
                        /*
                         * DV 26-JAN-2022
                         *
                         * If this is a Simple Email communication then filter out the Communication Wizard Templates.
                         * If this is an SMS then only include templates that have SMS templates. #4888
                         *
                         */
                        if ( null != template &&
                             ( ( medium.CommunicationType == CommunicationType.Email && !template.SupportsEmailWizard() && template.HasEmailTemplate() ) ||
                               ( medium.CommunicationType == CommunicationType.SMS && template.HasSMSTemplate() ) ) &&
                             template.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                        {
                            visible = true;
                            var li = new ListItem( template.Name, template.Id.ToString() );
                            li.Selected = template.Id.ToString() == prevSelection;
                            ddlTemplate.Items.Add( li );
                        }
                    }
                }
            }

            ddlTemplate.Visible = _fullMode && visible;
        }

        /// <summary>
        /// Binds the recipients.
        /// </summary>
        private void BindRecipients()
        {
            int recipientCount = Recipients.Count();
            lNumRecipients.Text = recipientCount.ToString( "N0" ) +
                ( recipientCount == 1 ? " Person" : " People" );

            // Reset the PersonPicker control selection.
            ppAddPerson.SetValue( null );
            ppAddPerson.PersonName = "Add Person";

            int displayCount = int.MaxValue;

            if ( !ShowAllRecipients )
            {
                int.TryParse( GetAttributeValue( AttributeKey.DisplayCount ), out displayCount );
            }

            if ( displayCount > 0 && displayCount < Recipients.Count )
            {
                rptRecipients.DataSource = Recipients.Take( displayCount ).ToList();
                lbShowAllRecipients.Visible = true;
            }
            else
            {
                rptRecipients.DataSource = Recipients.ToList();
                lbShowAllRecipients.Visible = false;
            }

            lbRemoveAllRecipients.Visible = _fullMode && Recipients.Where( r => r.Status == CommunicationRecipientStatus.Pending ).Any();

            rptRecipients.DataBind();

            StringBuilder rStatus = new StringBuilder();

            CheckApprovalRequired( Recipients.Count );
        }

        /// <summary>
        /// Shows the control for the currently selected medium (or the first medium if none selected).
        /// </summary>
        /// <param name="setData">When <see langword="true"/>, populates the medium control with the current communication data.</param>
        private MediumControl LoadMediumControl( bool setData )
        {
            if ( setData )
            {
                phContent.Controls.Clear();
            }

            // The component to load control for
            MediumComponent component = null;
            string mediumName = string.Empty;

            // Get the current medium type
            EntityTypeCache entityType = null;
            if ( MediumEntityTypeId.HasValue )
            {
                entityType = EntityTypeCache.Get( MediumEntityTypeId.Value );
            }

            foreach ( var serviceEntry in MediumContainer.Instance.Components )
            {
                var mediumComponent = serviceEntry.Value;

                // Default to first component
                if ( component == null )
                {
                    component = mediumComponent.Value;
                    mediumName = mediumComponent.Metadata.ComponentName + " ";
                }

                // If invalid entity type, exit (and use first component found)
                if ( entityType == null )
                {
                    break;
                }
                else if ( entityType.Id == mediumComponent.Value.EntityType.Id )
                {
                    component = mediumComponent.Value;
                    mediumName = mediumComponent.Metadata.ComponentName + " ";
                    break;
                }
            }

            if ( component != null )
            {
                var mediumControl = component.GetControl( !_fullMode );
                if ( mediumControl is Rock.Web.UI.Controls.Communication.Email )
                {
                    ( (Rock.Web.UI.Controls.Communication.Email)mediumControl ).AllowCcBcc = GetAttributeValue( AttributeKey.AllowCcBcc ).AsBoolean();
                }
                else if ( mediumControl is Rock.Web.UI.Controls.Communication.Sms )
                {
                    var allowedSmsNumbersGuids = GetAttributeValue( AttributeKey.AllowedSMSNumbers ).SplitDelimitedValues( true ).AsGuidList();

                    ( ( Sms ) mediumControl ).SelectedNumbers = SystemPhoneNumberCache.All( false )
                        .Where( spn => spn.IsAuthorized( Authorization.VIEW, this.CurrentPerson ) && allowedSmsNumbersGuids.ContainsOrEmpty( spn.Guid ) )
                        .Select( spn => spn.Guid )
                        .ToList();
                }

                mediumControl.ID = "commControl";
                mediumControl.IsTemplate = false;
                mediumControl.AdditionalMergeFields = this.AdditionalMergeFields.ToList();
                mediumControl.ValidationGroup = btnSubmit.ValidationGroup;
                var fupEmailAttachments = ( Rock.Web.UI.Controls.FileUploader ) mediumControl.FindControl( "fupEmailAttachments_commControl" );

                if ( fupEmailAttachments != null )
                {
                    if ( !GetAttributeValue( AttributeKey.ShowAttachmentUploader ).AsBoolean() )
                    {
                        if ( fupEmailAttachments != null )
                        {
                            fupEmailAttachments.Visible = false;
                        }
                    }
                    else
                    {
                        fupEmailAttachments.BinaryFileTypeGuid = this.GetAttributeValue( AttributeKey.AttachmentBinaryFileType ).AsGuidOrNull() ?? Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid();
                    }
                }
                // if this is an email with an HTML control and there are block settings to provide updated content directories set them
                if ( mediumControl is Rock.Web.UI.Controls.Communication.Email )
                {
                    var htmlControl = (HtmlEditor)mediumControl.FindControl( "htmlMessage_commControl" );

                    if ( htmlControl != null )
                    {
                        if ( GetAttributeValue( AttributeKey.DocumentRootFolder ).IsNotNullOrWhiteSpace() )
                        {
                            htmlControl.DocumentFolderRoot = GetAttributeValue( AttributeKey.DocumentRootFolder );
                        }

                        if ( GetAttributeValue( AttributeKey.ImageRootFolder ).IsNotNullOrWhiteSpace() )
                        {
                            htmlControl.ImageFolderRoot = GetAttributeValue( AttributeKey.ImageRootFolder );
                        }

                        if ( GetAttributeValue( AttributeKey.UserSpecificFolders ).AsBooleanOrNull().HasValue )
                        {
                            htmlControl.UserSpecificRoot = GetAttributeValue( AttributeKey.UserSpecificFolders ).AsBoolean();
                        }
                    }
                }

                phContent.Controls.Add( mediumControl );

                if ( setData )
                {
                    mediumControl.SetFromCommunication( CommunicationData );
                }

                // Set the medium in case it wasn't already set or the previous component type was not found
                MediumEntityTypeId = component.EntityType.Id;

                if ( component.Transport == null || !component.Transport.IsActive )
                {
                    nbInvalidTransport.Text = string.Format( "The {0}medium does not have an active transport configured. The communication will not be delivered until the transport is configured correctly.", mediumName );
                    nbInvalidTransport.Visible = true;
                }
                else
                {
                    nbInvalidTransport.Visible = false;
                }

                return mediumControl;
            }

            return null;
        }

        /// <summary>
        /// Initializes the control with current persons information if this is first time that this medium is being viewed
        /// </summary>
        /// <param name="control">The control.</param>
        private void InitializeControl( MediumControl control )
        {
            if ( MediumEntityTypeId.HasValue && !ViewedEntityTypes.Contains( MediumEntityTypeId.Value ) )
            {
                if ( control != null && CurrentPerson != null )
                {
                    control.InitializeFromSender( CurrentPerson );
                }

                ViewedEntityTypes.Add( MediumEntityTypeId.Value );
            }
        }

        /// <summary>
        /// Shows or hides the bulk option.
        /// </summary>
        private void ShowHideIsBulkOption()
        {
            if ( MediumEntityTypeId.HasValue
                 && MediumContainer.GetComponentByEntityTypeId( MediumEntityTypeId ) is Rock.Communication.Medium.Email emailMediumComponent
                 && emailMediumComponent.IsBulkEmailThresholdExceeded( Recipients?.Count ?? 0 ) )
            {
                // Override to unchecked when bulk communication is prevented.
                cbBulk.Visible = false;

                // Force bulk communication since the recipient count has exceeded the threshold.
                _isBulkCommunicationForced = true;
            }
            else
            {
                cbBulk.Visible = _fullMode;
                
                // Do not force bulk communication since the recipient count has not exceeded the threshold.
                _isBulkCommunicationForced = false;
            }
        }

        private MediumControl GetMediumControl()
        {
            if ( phContent.Controls.Count == 1 )
            {
                return phContent.Controls[0] as MediumControl;
            }
            return null;
        }

        /// <summary>
        /// Updates the communication data from the current medium control.
        /// </summary>
        private void GetMediumData()
        {
            var mediumControl = GetMediumControl();
            if ( mediumControl != null )
            {
                // If using simple mode, the control should be re-initialized from sender since sender fields
                // are not presented for editing and user shouldn't be able to change them
                if ( !_fullMode && CurrentPerson != null )
                {
                    mediumControl.InitializeFromSender( CurrentPerson );
                }

                mediumControl.UpdateCommunication( CommunicationData );
            }
        }

        private void GetTemplateData( int templateId, bool loadControl = true )
        {
            var template = new CommunicationTemplateService( new RockContext() ).Get( templateId );
            if ( template != null )
            {
                // save what was entered for FromEmail and FromName in case the template blanks it out
                var enteredFromEmail = CommunicationData.FromEmail;
                var enteredFromName = CommunicationData.FromName;

                // copy all communication details from the Template to CommunicationData
                CommunicationDetails.Copy( template, CommunicationData );
                CommunicationData.FromName = template.FromName.ResolveMergeFields( Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson ) );
                CommunicationData.FromEmail = template.FromEmail.ResolveMergeFields( Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson ) );

                // if the FromName was cleared by the template, use the one that was there before the template was changed (similar logic to CommunicationEntryWizard)
                // Otherwise, if the template does have a FromName, we want to template's FromName to overwrite it (which CommunicationDetails.Copy already did)
                if ( CommunicationData.FromName.IsNullOrWhiteSpace() )
                {
                    CommunicationData.FromName = enteredFromName;
                }

                // if the FromEmail was cleared by the template, use the one that was there before the template was changed (similar logic to CommunicationEntryWizard)
                // Otherwise, if the template does have a FromEmail, we want to template's fromemail to overwrite it (which CommunicationDetails.Copy already did)
                if ( CommunicationData.FromEmail.IsNullOrWhiteSpace() )
                {
                    CommunicationData.FromEmail = enteredFromEmail;
                }

                CommunicationData.EmailAttachmentBinaryFileIds = template.EmailAttachmentBinaryFileIds;

                if ( loadControl )
                {
                    var mediumControl = LoadMediumControl( true );
                }
            }
        }

        /// <summary>
        /// Shows the actions.
        /// </summary>
        /// <param name="communication">The communication.</param>
        private void ShowActions( Rock.Model.Communication communication )
        {
            // Determine if user is allowed to save changes, if not, disable
            // submit and save buttons
            if ( IsUserAuthorized( "Approve" ) ||
                ( CurrentPersonAliasId.HasValue && CurrentPersonAliasId == communication.SenderPersonAliasId ) ||
                IsUserAuthorized( Authorization.EDIT ) )
            {
                btnSubmit.Enabled = true;
                btnSave.Enabled = true && _fullMode;
            }
            else
            {
                btnSubmit.Enabled = false;
                btnSave.Enabled = false;
            }

            if ( _editingApproved && communication.Status == CommunicationStatus.PendingApproval )
            {
                btnSubmit.Text = "Save";
                btnSave.Visible = false;
                btnCancel.Visible = true;
            }
            else
            {
                btnSubmit.Text = "Submit";
                btnSave.Visible = true && _fullMode;
                btnCancel.Visible = false;
            }
        }

        /// <summary>
        /// Determines whether approval is required, and sets the submit button text appropriately
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <returns>
        ///   <c>true</c> if [is approval required] [the specified communication]; otherwise, <c>false</c>.
        /// </returns>
        private bool CheckApprovalRequired( int numberOfRecipients )
        {
            int maxRecipients = int.MaxValue;
            int.TryParse( GetAttributeValue( AttributeKey.MaximumRecipients ), out maxRecipients );
            bool approvalRequired = numberOfRecipients > maxRecipients;

            if ( _editingApproved )
            {
                btnSubmit.Text = "Save";
            }
            else
            {
                btnSubmit.Text = ( approvalRequired && !IsUserAuthorized( "Approve" ) ? "Submit" : "Send" ) + " Communication";
            }

            return approvalRequired;
        }

        /// <summary>
        /// Updates a communication model with the user-entered values
        /// </summary>
        /// <param name="communicationService">The service.</param>
        /// <returns></returns>
        private Rock.Model.Communication UpdateCommunication( RockContext rockContext )
        {
            var communicationService = new CommunicationService( rockContext );
            var communicationAttachmentService = new CommunicationAttachmentService( rockContext );
            var communicationRecipientService = new CommunicationRecipientService( rockContext );

            Rock.Model.Communication communication = null;
            IQueryable<CommunicationRecipient> qryRecipients = null;

            if ( CommunicationId.HasValue && CommunicationId.Value != 0 )
            {
                communication = communicationService.Get( CommunicationId.Value );
            }

            if ( communication != null )
            {
                // Remove any deleted recipients
                HashSet<int> personIdHash = new HashSet<int>( Recipients.Select( a => a.PersonId ) );
                qryRecipients = communication.GetRecipientsQry( rockContext );

                foreach ( var item in qryRecipients.Select( a => new
                {
                    Id = a.Id,
                    PersonId = a.PersonAlias.PersonId
                } ) )
                {
                    if ( !personIdHash.Contains( item.PersonId ) )
                    {
                        var recipient = qryRecipients.Where( a => a.Id == item.Id ).FirstOrDefault();
                        communicationRecipientService.Delete( recipient );
                        communication.Recipients.Remove( recipient );
                    }
                }
            }

            if ( communication == null )
            {
                communication = new Rock.Model.Communication();
                communication.Status = CommunicationStatus.Transient;
                communication.SenderPersonAliasId = CurrentPersonAliasId;
                communicationService.Add( communication );
            }

            communication.EnabledLavaCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands );

            if ( qryRecipients == null )
            {
                qryRecipients = communication.GetRecipientsQry( rockContext );
            }

            // Add any new recipients
            HashSet<int> communicationPersonIdHash = new HashSet<int>( qryRecipients.Select( a => a.PersonAlias.PersonId ) );
            foreach ( var recipient in Recipients )
            {
                if ( !communicationPersonIdHash.Contains( recipient.PersonId ) )
                {
                    var person = new PersonService( rockContext ).Get( recipient.PersonId );
                    if ( person != null )
                    {
                        var communicationRecipient = new CommunicationRecipient();
                        communicationRecipient.PersonAlias = person.PrimaryAlias;
                        communication.Recipients.Add( communicationRecipient );
                    }
                }
            }

            communication.IsBulkCommunication = _isBulkCommunicationForced || cbBulk.Checked;
            var medium = MediumContainer.GetComponentByEntityTypeId( MediumEntityTypeId );
            if ( medium != null )
            {
                communication.CommunicationType = medium.CommunicationType;
            }

            communication.CommunicationTemplateId = ddlTemplate.SelectedValue.AsIntegerOrNull();

            GetMediumData();

            foreach( var recipient in communication.Recipients )
            {
                recipient.MediumEntityTypeId = MediumEntityTypeId;
            }

            CommunicationDetails.Copy( CommunicationData, communication );

            // delete any attachments that are no longer included
            foreach ( var attachment in communication.Attachments.Where( a => !CommunicationData.EmailAttachmentBinaryFileIds.Contains( a.BinaryFileId ) ).ToList() )
            {
                communication.Attachments.Remove( attachment );
                communicationAttachmentService.Delete( attachment );
            }

            // add any new attachments that were added
            foreach ( var attachmentBinaryFileId in CommunicationData.EmailAttachmentBinaryFileIds.Where( a => !communication.Attachments.Any( x => x.BinaryFileId == a ) ) )
            {
                communication.AddAttachment( new CommunicationAttachment { BinaryFileId = attachmentBinaryFileId }, CommunicationType.Email );
            }

            DateTime? futureSendDate = dtpFutureSend.SelectedDateTime;
            if ( futureSendDate.HasValue && futureSendDate.Value.CompareTo( RockDateTime.Now ) > 0 )
            {
                communication.FutureSendDateTime = futureSendDate;
            }
            else
            {
                communication.FutureSendDateTime = null;
            }

            // If we are not allowing lava then remove the syntax
            if ( !GetAttributeValue( AttributeKey.EnableLava ).AsBooleanOrNull() ?? false )
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

        private void ValidateFutureDelayDateTime()
        {
            DateTime? futureSendDate = dtpFutureSend.SelectedDateTime;
            TimeSpan? futureTime = dtpFutureSend.SelectedTime;

            if ( ( futureTime.HasValue && !futureSendDate.HasValue ) || ( !futureTime.HasValue && futureSendDate.HasValue ) )
            {
                cvDelayDateTime.IsValid = false;
                cvDelayDateTime.ErrorMessage = "The Delay Send Until value requires a future date and time.";
                return;
            }

            if ( futureSendDate.HasValue && futureSendDate.Value.CompareTo( RockDateTime.Now ) < 0 )
            {
                cvDelayDateTime.IsValid = false;
                cvDelayDateTime.ErrorMessage = "The Delay Send Until value must be a future date/time";
                return;
            }
        }

        private void ShowStatus( Rock.Model.Communication communication )
        {
            var status = communication != null ? communication.Status : CommunicationStatus.Draft;
            switch ( status )
            {
                case CommunicationStatus.Transient:
                case CommunicationStatus.Draft:
                    {
                        hlStatus.Text = "Draft";
                        hlStatus.LabelType = LabelType.Default;
                        break;
                    }
                case CommunicationStatus.PendingApproval:
                    {
                        hlStatus.Text = "Pending Approval";
                        hlStatus.LabelType = LabelType.Warning;
                        break;
                    }
                case CommunicationStatus.Approved:
                    {
                        hlStatus.Text = "Approved";
                        hlStatus.LabelType = LabelType.Success;
                        break;
                    }
                case CommunicationStatus.Denied:
                    {
                        hlStatus.Text = "Denied";
                        hlStatus.LabelType = LabelType.Danger;
                        break;
                    }
            }
        }

        /// <summary>
        /// Shows the result.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="communication">The communication.</param>
        private void ShowResult( string message, Rock.Model.Communication communication )
        {
            ShowStatus( communication );

            pnlEdit.Visible = false;

            nbResult.Text = message;

            CurrentPageReference.Parameters.AddOrReplace( PageParameterKey.CommunicationId, communication.Id.ToString() );
            hlViewCommunication.NavigateUrl = CurrentPageReference.BuildUrl();

            // only show the Link if there is a CommunicationDetail block type on this page
            hlViewCommunication.Visible = this.PageCache.Blocks.Any( a => a.BlockType.Guid == Rock.SystemGuid.BlockType.COMMUNICATION_DETAIL.AsGuid() );

            pnlResult.Visible = true;

            ScriptManager.RegisterStartupScript(
                Page,
                GetType(),
                "scrollToResults",
                "scrollToResults();",
                true );

        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Helper class used to maintain state of recipients
        /// </summary>
        [Serializable]
        protected class Recipient
        {
            /// <summary>
            /// Gets or sets the person id.
            /// </summary>
            /// <value>
            /// The person id.
            /// </value>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the name of the person.
            /// </summary>
            /// <value>
            /// The name of the person.
            /// </value>
            public string PersonName { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether this person is deceased.
            /// </summary>
            /// <value>
            /// <c>true</c> if this instance is deceased; otherwise, <c>false</c>.
            /// </value>
            public bool IsDeceased { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [has SMS number].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [has SMS number]; otherwise, <c>false</c>.
            /// </value>
            public bool HasSmsNumber { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [has notifications enabled].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [has notifications enabled]; otherwise, <c>false</c>.
            /// </value>
            public bool HasNotificationsEnabled { get; set; }

            /// <summary>
            /// Gets or sets the email.
            /// </summary>
            /// <value>
            /// The email.
            /// </value>
            public string Email { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether email is active.
            /// </summary>
            /// <value>
            ///  <c>true</c> if email id active; otherwise, <c>false</c>.
            /// </value>
            public bool IsEmailActive { get; set; }

            /// <summary>
            /// Gets or sets the email note.
            /// </summary>
            /// <value>
            /// The email note.
            /// </value>
            public string EmailNote { get; set; }

            /// <summary>
            /// Gets or sets the email preference.
            /// </summary>
            /// <value>
            /// The email preference.
            /// </value>
            public EmailPreference EmailPreference { get; set; }

            /// <summary>
            /// Gets or sets the status.
            /// </summary>
            /// <value>
            /// The status.
            /// </value>
            public CommunicationRecipientStatus Status { get; set; }

            /// <summary>
            /// Gets or sets the status note.
            /// </summary>
            /// <value>
            /// The status note.
            /// </value>
            public string StatusNote { get; set; }

            /// <summary>
            /// Gets or sets the client the email was opened on.
            /// </summary>
            /// <value>
            /// The opened email client.
            /// </value>
            public string OpenedClient { get; set; }

            /// <summary>
            /// Gets or sets the date/time the email was opened on.
            /// </summary>
            /// <value>
            /// The date/time the email was opened.
            /// </value>
            public DateTime? OpenedDateTime { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Recipient" /> class.
            /// </summary>
            /// <param name="personId">The person id.</param>
            /// <param name="personName">Name of the person.</param>
            /// <param name="status">The status.</param>
            public Recipient( Person person, bool personHasSMS, bool personHasNotificationsEnabled, CommunicationRecipientStatus status, string statusNote = "", string openedClient = "", DateTime? openedDateTime = null )
            {
                PersonId = person.Id;
                PersonName = person.FullName;
                IsDeceased = person.IsDeceased;
                HasSmsNumber = personHasSMS;
                HasNotificationsEnabled = personHasNotificationsEnabled;
                Email = person.Email;
                IsEmailActive = person.IsEmailActive;
                EmailNote = person.EmailNote;
                EmailPreference = person.EmailPreference;
                Status = status;
                StatusNote = statusNote;
                OpenedClient = openedClient;
                OpenedDateTime = openedDateTime;
            }

            public static string PreferenceMessage( Recipient recipient )
            {
                switch ( recipient.EmailPreference )
                {
                    case EmailPreference.DoNotEmail:
                        return "Email Preference is set to 'Do Not Email!'";
                    case EmailPreference.NoMassEmails:
                        return "Email Preference is set to 'No Mass Emails!'";
                }

                return string.Empty;
            }
        }

        #endregion

    }
}