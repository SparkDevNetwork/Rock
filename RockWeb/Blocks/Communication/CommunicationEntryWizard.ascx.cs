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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Observability;
using Rock.Security;
using Rock.Tasks;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Web.UI.Controls.Communication;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    /// A block for creating and sending a new communication such as email, SMS, etc. to recipients.
    /// </summary>
    [DisplayName( "Communication Entry Wizard" )]
    [Category( "Communication" )]
    [Description( "Used for creating and sending a new communications such as email, SMS, etc. to recipients." )]

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

    [IntegerField( "Character Limit",
        Key = AttributeKey.CharacterLimit,
        Description = "Set this to show a character limit countdown for SMS communications. Set to 0 to disable",
        IsRequired = false,
        DefaultIntegerValue = 160,
        Order = 3 )]

    [LavaCommandsField( "Enabled Lava Commands",
        Key = AttributeKey.EnabledLavaCommands,
        Description = "The Lava commands that should be enabled for this HTML block.",
        IsRequired = false,
        Order = 4 )]

    [CustomCheckboxListField( "Communication Types",
        Key = AttributeKey.CommunicationTypes,
        Description = "The communication types that should be available to use for the communication. (If none are selected, all will be available.) Selecting 'Recipient Preference' will automatically enable Email and SMS as mediums. Push is not an option for selection as a communication preference as delivery is not as reliable as other mediums based on an individual's privacy settings.",
        ListSource = "Recipient Preference,Email,SMS,Push",
        IsRequired = false,
        Order = 5 )]

    [IntegerField( "Maximum Recipients",
        Key = AttributeKey.MaximumRecipients,
        Description = "The maximum number of recipients allowed before communication will need to be approved.",
        IsRequired = false,
        DefaultIntegerValue = 300,
        Order = 6 )]

    [BooleanField( "Send When Approved",
        Key = AttributeKey.SendWhenApproved,
        Description = "Should communication be sent once it's approved (vs. just being queued for scheduled job to send)?",
        DefaultBooleanValue = true,
        Order = 7 )]

    [IntegerField( "Max SMS Image Width",
        Key = AttributeKey.MaxSMSImageWidth,
        Description = "The maximum width (in pixels) of an image attached to a mobile communication. If its width is over the max, Rock will automatically resize image to the max width.",
        IsRequired = false,
        DefaultIntegerValue = 600,
        Order = 8 )]

    [SystemPhoneNumberField( "Allowed SMS Numbers",
        Key = AttributeKey.AllowedSMSNumbers,
        Description = "Set the allowed FROM numbers to appear when in SMS mode (if none are selected all numbers will be included). ",
        IsRequired = false,
        AllowMultiple = true,
        Order = 9 )]

    [LinkedPage( "Simple Communication Page",
        Key = AttributeKey.SimpleCommunicationPage,
        Description = "The page to use if the 'Use Simple Editor' panel heading icon is clicked. Leave this blank to not show the 'Use Simple Editor' heading icon",
        IsRequired = false,
        Order = 10 )]

    [BooleanField( "Show Duplicate Prevention Option",
        Key = AttributeKey.ShowDuplicatePreventionOption,
        Description = "Set this to true to show an option to prevent communications from being sent to people with the same email/SMS addresses. Typically, in Rock you’d want to send two emails as each will be personalized to the individual.",
        DefaultBooleanValue = false,
        Order = 11 )]

    [BooleanField( "Default As Bulk",
        Key = AttributeKey.DefaultAsBulk,
        Description = "Should new entries be flagged as bulk communication by default?",
        DefaultBooleanValue = false,
        Order = 12 )]

    [BooleanField( "Enable Person Parameter",
        Key = AttributeKey.EnablePersonParameter,
        Description = "When enabled, allows passing a 'Person' or 'PersonId' querystring parameter with a person Id to the block to create a communication for that person.",
        DefaultBooleanValue = true,
        IsRequired = false,
        Order = 13 )]

    [BooleanField( "Disable Adding Individuals to Recipient Lists",
        Key = AttributeKey.DisableAddingIndividualsToRecipientLists,
        Description = "When set to 'Yes' the person picker will be hidden so that additional individuals cannot be added to the recipient list.",
        DefaultBooleanValue = false,
        IsRequired = false,
        Order = 14 )]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.COMMUNICATION_ENTRY_WIZARD )]
    public partial class CommunicationEntryWizard : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string ImageBinaryFileType = "ImageBinaryFileType";
            public const string AttachmentBinaryFileType = "AttachmentBinaryFileType";
            public const string CharacterLimit = "CharacterLimit";
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
        }

        #endregion Attribute Keys

        #region PageParameterKeys

        private static class PageParameterKey
        {
            public const string CommunicationId = "CommunicationId";
            public const string Edit = "Edit";
            public const string Person = "Person";
            public const string PersonId = "PersonId";
            public const string TemplateGuid = "TemplateGuid";
        }

        #endregion PageParameterKeys

        #region Fields

        private const string CATEGORY_COMMUNICATION_TEMPLATE = "CategoryCommunicationTemplate";

        private bool _smsTransportEnabled = false;
        private bool _emailTransportEnabled = false;
        private bool _pushTransportEnabled = false;
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

        private CommunicationType SelectedCommunicationType
        {
            get
            {
                return ( CommunicationType ) rblCommunicationMedium.SelectedValue.AsInteger();
            }

            set
            {
                rblCommunicationMedium.SelectedValue = value.ConvertToInt().ToString();
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the individual recipient person ids.
        /// </summary>
        /// <remarks>
        /// These are the manually selected recipients.
        /// </remarks>
        /// <value>
        /// The individual recipient person ids.
        /// </value>
        protected ObservableCollection<int> IndividualRecipientPersonIds
        {
            get
            {
                if ( ViewState["IndividualRecipientPersonIds"] is ObservableCollection<int> recipientPersonIds )
                {
                    // Make sure the change event handlers trigger the property changed event.
                    recipientPersonIds.CollectionChanged -= IndividualRecipientPersonIds_CollectionChanged;
                    recipientPersonIds.CollectionChanged += IndividualRecipientPersonIds_CollectionChanged;
                }
                else
                {
                    recipientPersonIds = new ObservableCollection<int>();
                    recipientPersonIds.CollectionChanged += IndividualRecipientPersonIds_CollectionChanged;
                    SetViewState( recipientPersonIds );
                }

                return recipientPersonIds;
            }

            set
            {
                ViewState["IndividualRecipientPersonIds"] = value;
            }
        }

        private int RecipientCount
        {
            get
            {
                return (ViewState[nameof(RecipientCount)] as int?) ?? 0;
            }
            set
            {
                SetViewState( value );
            }
        }

        /// <summary>
        /// Handles the CollectionChanged event of the IndividualRecipientPersonIds collection.
        /// </summary>
        /// <remarks>
        /// When an item is added, removed, or when the collection is cleared,
        /// the <see cref="OnPropertyChanged"/> event is raised indicating
        /// the <see cref="IndividualRecipientPersonIds"/> property changed.
        /// </remarks>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void IndividualRecipientPersonIds_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            RaisePropertyChanged( nameof( IndividualRecipientPersonIds ) );
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
            
            this.OnPropertyChanged -= CommunicationEntryWizard_OnPropertyChanged;
            this.OnPropertyChanged += CommunicationEntryWizard_OnPropertyChanged;

            RockPage.AddCSSLink( "~/Styles/Blocks/Communication/EmailEditor.css", true );
            RockPage.AddCSSLink( "~/Styles/Blocks/Shared/Devices.css", true );

            // Tell the browsers to not cache. This will help prevent browser using stale communication wizard stuff after navigating away from this page
            Page.Response.Cache.SetCacheability( System.Web.HttpCacheability.NoCache );
            Page.Response.Cache.SetExpires( DateTime.UtcNow.AddHours( -1 ) );
            Page.Response.Cache.SetNoStore();

            _smsTransportEnabled = MediumContainer.HasActiveAndAuthorizedSmsTransport( CurrentPerson );
            _emailTransportEnabled = MediumContainer.HasActiveAndAuthorizedEmailTransport( CurrentPerson );
            _pushTransportEnabled = MediumContainer.HasActiveAndAuthorizedPushTransport( CurrentPerson );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            componentImageUploader.BinaryFileTypeGuid = this.GetAttributeValue( AttributeKey.ImageBinaryFileType ).AsGuidOrNull() ?? Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid();
            fupEmailAttachments.BinaryFileTypeGuid = this.GetAttributeValue( AttributeKey.AttachmentBinaryFileType ).AsGuidOrNull() ?? Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid();
            fupMobileAttachment.BinaryFileTypeGuid = this.GetAttributeValue( AttributeKey.AttachmentBinaryFileType ).AsGuidOrNull() ?? Rock.SystemGuid.BinaryFiletype.DEFAULT.AsGuid();

            componentAssetManager.PickerButtonTemplate = @"
{% assign iconPath = SelectedValue | FromJSON | Property:'IconPath' %}
{% assign fileName = SelectedValue | FromJSON | Property:'Name' %}

{% if iconPath != '' and fileName != '' %}
    {% assign escFileName = fileName | UrlEncode %}
    {% assign imageTypeUrl = iconPath | Replace: fileName, escFileName %}
{% endif %}

<div class='js-asset-thumbnail fileupload-thumbnail{% if imageTypeUrl contains '/Assets/Icons/FileTypes/' %} fileupload-thumbnail-icon{% endif %}' {% if fileName != '' %}style='background-image:url({{ imageTypeUrl }}) !important;' title='{{ fileName }}'{% endif %}>
    {% if fileName != '' %}
        <span class='js-asset-thumbnail-name file-link' style='background-color: transparent'>{{ fileName }}</span>
    {% else %}
        <span class='js-asset-thumbnail-name file-link file-link-default'></span>
    {% endif %}
</div>
<div class='imageupload-dropzone'>
    <span>
        Select Asset
    </span>
</div>";

            componentAssetManager.JsScriptToRegister = @"
    Sys.Application.add_load(function (e) {
        var data = '{{ SelectedValue }}';
        handleAssetUpdate(e, data);
    });";

            var videoProviders = Rock.Communication.VideoEmbed.VideoEmbedContainer.Instance.Dictionary.Select( c => c.Value.Key );
            lbVideoUrlHelpText.Attributes["data-original-title"] += ( videoProviders.Count() > 1 ? string.Join( ", ", videoProviders.Take( videoProviders.Count() - 1 ) ) + " and " + videoProviders.Last() : videoProviders.FirstOrDefault() ) + ".";
            hfSMSCharLimit.Value = ( this.GetAttributeValue( AttributeKey.CharacterLimit ).AsIntegerOrNull() ?? 160 ).ToString();

            gIndividualRecipients.DataKeyNames = new string[] { "Id" };
            gIndividualRecipients.GridRebind += gIndividualRecipients_GridRebind;
            gIndividualRecipients.Actions.ShowAdd = false;
            gIndividualRecipients.ShowActionRow = false;
            gIndividualRecipients.RowItemText = "Recipient";

            gRecipientList.DataKeyNames = new string[] { "Id" };
            gRecipientList.GridRebind += gRecipientList_GridRebind;
            gRecipientList.Actions.ShowAdd = false;
            gRecipientList.ShowActionRow = false;

            btnUseSimpleEditor.Visible = !string.IsNullOrEmpty( this.GetAttributeValue( AttributeKey.SimpleCommunicationPage ) );
            pnlHeadingLabels.Visible = btnUseSimpleEditor.Visible;

            var mediumControl = MediumControl.GetMediumControl( CommunicationType.PushNotification );

            mediumControl.ID = "mediumControl";
            mediumControl.IsTemplate = false;
            mediumControl.ValidationGroup = vsPushEditor.ValidationGroup;

            phPushControl.Controls.Add( mediumControl );

            RegisterTaskActivityScript();
        }

        /// <summary>
        /// Handles the <see cref="OnPropertyChanged"/> event of the CommunicationEntryWizard control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void CommunicationEntryWizard_OnPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            var isBulkDependencies = new List<string> { nameof( RecipientCount ), nameof( SelectedCommunicationType ) };
            if ( isBulkDependencies.Contains( e.PropertyName ) )
            {
                // If one of the "Is Bulk" dependencies change, then show or hide the bulk option.
                ShowHideIsBulkOption();
            }
        }

        private void RegisterTaskActivityScript()
        {
            // Create a callback function to update the View Communication link when the task is complete.
            var taskCompletedCallbackScript = @"
function onTaskCompleted( resultData )
{
    if ( resultData != null ) {
        $( ""[id$='_hlViewCommunication']"" ).attr( 'href', resultData.ViewCommunicationUrl );
    }
}
";
            RockPage.ClientScript.RegisterStartupScript( this.GetType(), "TaskCompletedCallbackScript", taskCompletedCallbackScript, true );

            // Add the Task Activity scripts to the page.
            SignalRTaskActivityUiHelper.AddActivityReporterScripts( this.RockPage, "onTaskCompleted" );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            
            this.OnPropertyChanged -= CommunicationEntryWizard_OnPropertyChanged;
            this.OnPropertyChanged += CommunicationEntryWizard_OnPropertyChanged;

            // register navigation event to enable support for the back button
            var scriptManager = ScriptManager.GetCurrent( Page );
            scriptManager.Navigate += scriptManager_Navigate;
            ShowHideIsBulkOption();

            if ( !Page.IsPostBack )
            {
                hfNavigationHistoryInstance.Value = Guid.NewGuid().ToString();
                ShowDetail( PageParameter( PageParameterKey.CommunicationId ).AsInteger() );
            }

            // set the email preview visible = false on every load so that it doesn't stick around after previewing then navigating
            pnlEmailPreview.Visible = false;

            // hide person picker if the block attribute setting for DisableAddingIndividualsToRecipientLists is true.
            ppAddPerson.Visible = !GetAttributeValue( AttributeKey.DisableAddingIndividualsToRecipientLists ).AsBoolean();

            // Reset the Task Activity controls on the page.
            SignalRTaskActivityUiHelper.SetTaskActivityControlMode( this.RockPage, SignalRTaskActivityUiHelper.ControlModeSpecifier.Hidden );
        }

        /// <summary>
        /// Handles the Navigate event of the scriptManager control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="HistoryEventArgs"/> instance containing the event data.</param>
        private void scriptManager_Navigate( object sender, HistoryEventArgs e )
        {
            var navigationPanelId = e.State["navigationPanelId"];
            var navigationHistoryInstance = e.State["navigationHistoryInstance"];
            Panel navigationPanel = null;

            if ( navigationHistoryInstance == null & navigationPanelId == null )
            {
                // if scriptManager_Navigate was called with no state, that is probably a navigation to the first step
                navigationHistoryInstance = hfNavigationHistoryInstance.Value;
                navigationPanelId = pnlListSelection.ID;
            }

            if ( navigationPanelId != null )
            {
                navigationPanel = this.FindControl( navigationPanelId ) as Panel;
            }

            if ( navigationHistoryInstance == hfNavigationHistoryInstance.Value && navigationPanel != null )
            {
                if ( navigationPanel == pnlConfirmation )
                {
                    // if navigating history is pnlConfirmation, the communication has already been submitted, so reload the page with the communicationId (if known)
                    Dictionary<string, string> qryParams = new Dictionary<string, string>();
                    if ( hfCommunicationId.Value != "0" )
                    {
                        qryParams.Add( PageParameterKey.CommunicationId, hfCommunicationId.Value );
                    }

                    this.NavigateToCurrentPageReference( qryParams );
                }

                var navigationPanels = this.ControlsOfTypeRecursive<Panel>().Where( a => a.CssClass.Contains( "js-navigation-panel" ) );
                foreach ( var pnl in navigationPanels )
                {
                    pnl.Visible = false;
                }

                navigationPanel.Visible = true;
                if ( navigationPanel == pnlTemplateSelection )
                {
                    BindTemplatePicker();
                }

                // upnlContent has UpdateMode = Conditional, so we have to update manually
                upnlContent.Update();
            }
            else
            {
                // mismatch navigation hash, so just reload the without the history hash
                this.NavigateToCurrentPageReference( new Dictionary<string, string>() );
            }
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="communicationId">The communication identifier.</param>
        public void ShowDetail( int communicationId )
        {
            Rock.Model.Communication communication = null;
            var rockContext = new RockContext();
            var pushCommunication = new CommunicationDetails();
            var emailMedium = GetEmailMediumWithActiveTransport();

            if ( communicationId != 0 )
            {
                communication = new CommunicationService( rockContext ).Get( communicationId );
            }

            var editingApproved = PageParameter( PageParameterKey.Edit ).AsBoolean() && IsUserAuthorized( "Approve" );

            nbCommunicationNotWizardCompatible.Visible = false;

            if ( communication == null )
            {
                communication = new Rock.Model.Communication
                {
                    Status = CommunicationStatus.Transient,
                    CreatedByPersonAlias = this.CurrentPersonAlias,
                    CreatedByPersonAliasId = this.CurrentPersonAliasId,
                    SenderPersonAlias = this.CurrentPersonAlias,
                    SenderPersonAliasId = CurrentPersonAliasId,
                    CommunicationType = CommunicationType.Email,
                    IsBulkCommunication = _isBulkCommunicationForced
                };
            }
            else
            {
                if ( !string.IsNullOrEmpty( communication.Message ) )
                {
                    if ( !communication.CommunicationTemplateId.HasValue || !communication.CommunicationTemplate.SupportsEmailWizard() )
                    {
                        // If this communication was previously created, but doesn't have a CommunicationTemplateId or uses a template that doesn't support the EmailWizard,
                        // it is a communication (or a copy of a communication) that was created using the 'Simple Editor' or the editor prior to v7.
                        // So, if they use the wizard, the main HTML Content will be reset when they get to the Select Template step
                        // since the wizard requires that the communication uses a Template that supports the Email Wizard.
                        // So, if this is the case, warn them and explain that they can continue with the wizard but start over on the content,
                        // or to use the 'Use Simple Editor' to keep the content, but not use the wizard
                        nbCommunicationNotWizardCompatible.Visible = true;
                    }
                }

                pushCommunication = new CommunicationDetails
                {
                    PushData = communication.PushData,
                    PushImageBinaryFileId = communication.PushImageBinaryFileId,
                    PushMessage = communication.PushMessage,
                    PushTitle = communication.PushTitle,
                    PushOpenMessage = communication.PushOpenMessage,
                    PushOpenAction = communication.PushOpenAction
                };
            }

            // If the communication is not yet edited by a user, apply any appropriate block default settings.
            // This occurs for new communications or those passed in from another process, such as an action on a Person list from a data grid or report.
            if ( communication.Status == CommunicationStatus.Transient )
            {
                communication.EnabledLavaCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands );
                communication.IsBulkCommunication = _isBulkCommunicationForced || GetAttributeValue( AttributeKey.DefaultAsBulk ).AsBoolean();
            }

            var allowedCommunicationTypes = GetAllowedCommunicationTypes();
            if ( !allowedCommunicationTypes.Contains( communication.CommunicationType ) )
            {
                communication.CommunicationType = allowedCommunicationTypes.First();
            }

            // If viewing a new, transient, draft, or are the approver of a pending-approval communication, use this block
            // otherwise, set this block visible=false and if there is a communication detail block on this page, it'll be shown instead
            CommunicationStatus[] editableStatuses = new CommunicationStatus[] { CommunicationStatus.Transient, CommunicationStatus.Draft, CommunicationStatus.Denied };
            if ( editableStatuses.Contains( communication.Status ) || ( communication.Status == CommunicationStatus.PendingApproval && editingApproved ) )
            {
                // Make sure they are authorized to edit, or the owner, or the approver/editor
                bool isAuthorizedEditor = communication.IsAuthorized( Rock.Security.Authorization.EDIT, CurrentPerson );
                bool isCreator = communication.CreatedByPersonAlias != null && CurrentPersonId.HasValue && communication.CreatedByPersonAlias.PersonId == CurrentPersonId.Value;
                bool isApprovalEditor = communication.Status == CommunicationStatus.PendingApproval && editingApproved;
                if ( isAuthorizedEditor || isCreator || isApprovalEditor )
                {
                    // communication is either new or OK to edit
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
                return;
            }

            LoadDropDowns( communication );
            SetupCommunicationMediumSection();

            hfCommunicationId.Value = communication.Id.ToString();
            lTitle.Text = ( communication.Name ?? communication.Subject ?? "New Communication" ).FormatAsHtmlTitle();
            cbDuplicatePreventionOption.Visible = this.GetAttributeValue( AttributeKey.ShowDuplicatePreventionOption ).AsBoolean();
            cbDuplicatePreventionOption.Checked = communication.ExcludeDuplicateRecipientAddress;
            tbCommunicationName.Text = communication.Name;
            swBulkCommunication.Checked = _isBulkCommunicationForced || communication.IsBulkCommunication;

            var segmentDataviewGuids = communication.Segments.SplitDelimitedValues().AsGuidList();
            if ( segmentDataviewGuids.Any() )
            {
                var segmentDataviewIds = new DataViewService( rockContext ).GetByGuids( segmentDataviewGuids ).Select( a => a.Id ).ToList();
                cblCommunicationGroupSegments.SetValues( segmentDataviewIds );
            }

            if ( communication.ListGroupId == null )
            {
                var recipientIds = new CommunicationRecipientService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( r => r.CommunicationId == communication.Id )
                    .Select( a => a.PersonAlias.PersonId )
                    .ToList();
                IndividualRecipientPersonIds = new ObservableCollection<int>( recipientIds );
            }

            int? personId = null;
            if ( GetAttributeValue( AttributeKey.EnablePersonParameter ).AsBoolean() )
            {
                // if either 'Person' or 'PersonId' is specified add that person to the communication
                personId = PageParameter( PageParameterKey.Person ).AsIntegerOrNull()
                    ?? PageParameter( PageParameterKey.PersonId ).AsIntegerOrNull();
            }

            if ( personId.HasValue && !communication.ListGroupId.HasValue )
            {
                communication.IsBulkCommunication = _isBulkCommunicationForced;
                var context = new RockContext();
                var person = new PersonService( context ).Get( personId.Value );
                if ( person != null )
                {
                    if ( !this.IndividualRecipientPersonIds.Contains( person.Id ) )
                    {
                        this.IndividualRecipientPersonIds.Add( person.Id );
                    }
                }
            }

            // If a template guid was passed in and this is a new communication, set that as the selected template
            Guid? templateGuid = PageParameter( PageParameterKey.TemplateGuid ).AsGuidOrNull();

            if ( communication.Id > 0 && templateGuid.HasValue )
            {
                var template = new CommunicationTemplateService( rockContext ).Get( templateGuid.Value );

                // NOTE: Only set the selected template if the user has auth for this template
                // and the template supports the Email Wizard
                if ( template != null && template.IsAuthorized( Rock.Security.Authorization.VIEW, this.CurrentPerson ) && template.SupportsEmailWizard() )
                {
                    communication.CommunicationTemplateId = template.Id;
                    this.InitializeFieldsFromCommunicationTemplate( template.Id );
                }
            }

            UpdateRecipientListCount();

            // Set the visibility of the Individual Recipients panel.
            var showIndividualRecipientsPanel = false;

            if ( IndividualRecipientPersonIds.Count > 0 )
            {
                showIndividualRecipientsPanel = true;
            }
            else
            {
                // If there aren't any Communication Groups, hide the option and only show the Individual Recipient selection
                if ( ddlCommunicationGroupList.Items.Count <= 1 || ( communication.Id != 0 && communication.ListGroupId == null ) )
                {
                    showIndividualRecipientsPanel = true;
                }
            }

            if ( showIndividualRecipientsPanel )
            {
                BindIndividualRecipientsPanelControls();

                pnlListSelection.Visible = false;
                pnlIndividualRecipientPanel.Visible = true;
            }
            else
            {
                pnlListSelection.Visible = true;
                pnlIndividualRecipientPanel.Visible = false;
            }

            // Note: Javascript takes care of making sure the buttons are set up based on this
            SelectedCommunicationType = communication.CommunicationType;

            chkSendImmediately.Checked = !communication.FutureSendDateTime.HasValue;
            dtpSendCommunicationDateTime.SelectedDateTime = communication.FutureSendDateTime;
            UpdateSendScheduleButton();

            hfSelectedCommunicationTemplateId.Value = communication.CommunicationTemplateId.ToString();

            // Email Summary fields
            tbFromName.Text = communication.FromName;
            ebFromAddress.Text = communication.FromEmail;

            // Email Summary fields: additional fields
            ebReplyToAddress.Text = communication.ReplyToEmail;
            ebCCList.Text = communication.CCEmails;
            ebBCCList.Text = communication.BCCEmails;

            hfShowAdditionalFields.Value = ( !string.IsNullOrEmpty( communication.ReplyToEmail ) || !string.IsNullOrEmpty( communication.CCEmails ) || !string.IsNullOrEmpty( communication.BCCEmails ) ).ToTrueFalse().ToLower();

            tbEmailSubject.Text = communication.Subject;

            //// NOTE: tbEmailPreview will be populated by parsing the HTML of the Email/Template

            hfEmailAttachedBinaryFileIds.Value = communication.GetAttachmentBinaryFileIds( CommunicationType.Email ).AsDelimited( "," );
            UpdateEmailAttachedFiles( false );

            // Mobile Text Editor
            var valueItem = ddlSMSFrom.Items.FindByValue( communication.SmsFromSystemPhoneNumberId.ToString() );
            if ( valueItem == null && communication.SmsFromSystemPhoneNumberId != null )
            {
                var lookupSystemPhoneNumber = SystemPhoneNumberCache.Get( communication.SmsFromSystemPhoneNumberId.GetValueOrDefault() );
                if ( lookupSystemPhoneNumber != null && lookupSystemPhoneNumber.IsAuthorized( Rock.Security.Authorization.VIEW, this.CurrentPerson ) )
                {
                    ddlSMSFrom.Items.Add( new ListItem( lookupSystemPhoneNumber.Name, lookupSystemPhoneNumber.Id.ToString() ) );
                }
            }

            ddlSMSFrom.SetValue( communication.SmsFromSystemPhoneNumberId );
            tbSMSTextMessage.Text = communication.SMSMessage;

            fupMobileAttachment.BinaryFileId = communication.GetAttachmentBinaryFileIds( CommunicationType.SMS ).FirstOrDefault();
            UpdateMobileAttachedFiles( false );

            // Email Editor
            hfEmailEditorHtml.Value = communication.Message;

            htmlEditor.MergeFields.Clear();
            htmlEditor.MergeFields.Add( "GlobalAttribute" );
            htmlEditor.MergeFields.Add( "Rock.Model.Person" );
            htmlEditor.MergeFields.Add( "Communication.Subject|Subject" );
            htmlEditor.MergeFields.Add( "Communication.FromName|From Name" );
            htmlEditor.MergeFields.Add( "Communication.FromEmail|From Address" );
            htmlEditor.MergeFields.Add( "Communication.ReplyTo|Reply To" );
            htmlEditor.MergeFields.Add( "UnsubscribeOption" );
            if ( communication.AdditionalMergeFields.Any() )
            {
                htmlEditor.MergeFields.AddRange( communication.AdditionalMergeFields );
            }

            var pushNotificationControl = phPushControl.Controls[0] as PushNotification;
            if ( pushNotificationControl != null )
            {
                pushNotificationControl.SetFromCommunication( pushCommunication );
                pushNotificationControl.AdditionalMergeFields = communication.AdditionalMergeFields;
            }
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        /// <param name="communication">The communication.</param>
        private void LoadDropDowns( Rock.Model.Communication communication )
        {
            var rockContext = new RockContext();

            // load communication group list
            var groupTypeCommunicationGroupId = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST.AsGuid() ).Id;
            var groupService = new GroupService( rockContext );

            var communicationGroupList = groupService.Queryable().AsNoTracking().Where( a => a.GroupTypeId == groupTypeCommunicationGroupId && a.IsActive ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            var authorizedCommunicationGroupList = communicationGroupList.Where( g => g.IsAuthorized( Rock.Security.Authorization.VIEW, this.CurrentPerson ) ).ToList();

            ddlCommunicationGroupList.Items.Clear();
            ddlCommunicationGroupList.Items.Add( new ListItem() );
            foreach ( var communicationGroup in authorizedCommunicationGroupList )
            {
                ddlCommunicationGroupList.Items.Add( new ListItem( communicationGroup.Name, communicationGroup.Id.ToString() ) );
            }

            ddlCommunicationGroupList.SetValue( communication.ListGroupId );

            LoadCommunicationSegmentFilters();

            rblCommunicationGroupSegmentFilterType.Items.Clear();
            rblCommunicationGroupSegmentFilterType.Items.Add( new ListItem( "All segment filters", SegmentCriteria.All.ToString() ) { Selected = communication.SegmentCriteria == SegmentCriteria.All } );
            rblCommunicationGroupSegmentFilterType.Items.Add( new ListItem( "Any segment filters", SegmentCriteria.Any.ToString() ) { Selected = communication.SegmentCriteria == SegmentCriteria.Any } );

            UpdateRecipientListCount();

            var selectedNumberGuids = GetAttributeValue( AttributeKey.AllowedSMSNumbers ).SplitDelimitedValues( true ).AsGuidList();
            var systemPhoneNumbers = SystemPhoneNumberCache.All( false )
                .Where( spn => spn.IsAuthorized( Authorization.VIEW, this.CurrentPerson ) )
                .OrderBy( spn => spn.Order )
                .ThenBy( spn => spn.Name )
                .ThenBy( spn => spn.Id )
                .ToList();
            if ( selectedNumberGuids.Any() )
            {
                systemPhoneNumbers = systemPhoneNumbers.Where( spn => selectedNumberGuids.Contains( spn.Guid ) ).ToList();
            }

            ddlSMSFrom.Items.Clear();
            ddlSMSFrom.Items.Add( new ListItem() );
            foreach ( var item in systemPhoneNumbers )
            {
                ddlSMSFrom.Items.Add( new ListItem( item.Name, item.Id.ToString() ) );
            }

            ddlSMSFrom.SelectedIndex = -1;
        }

        /// <summary>
        /// Loads the communication types that are configured for this block
        /// </summary>
        private List<CommunicationType> GetAllowedCommunicationTypes( bool forSelector = false )
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
            if ( !forSelector && communicationTypes.Contains( "Recipient Preference" ) )
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
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            this.NavigateToCurrentPageReference( new Dictionary<string, string>() );
        }

        /// <summary>
        /// Sets the navigation history.
        /// </summary>
        /// <param name="currentPanel">The panel that should be navigated to when this history point is loaded
        private void SetNavigationHistory( Panel navigateToPanel )
        {
            this.AddHistory( "navigationPanelId", navigateToPanel.ID );
            this.AddHistory( "navigationHistoryInstance", hfNavigationHistoryInstance.Value );
        }

        /// <summary>
        /// Handles the Click event of the btnUseSimpleEditor control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUseSimpleEditor_Click( object sender, EventArgs e )
        {
            var simpleCommunicationPageRef = new Rock.Web.PageReference( this.GetAttributeValue( AttributeKey.SimpleCommunicationPage ), this.CurrentPageReference.Parameters, this.CurrentPageReference.QueryString );
            NavigateToPage( simpleCommunicationPageRef );
        }

        #endregion

        #region Recipient Selection

        /// <summary>
        /// Shows the recipient selection.
        /// </summary>
        private void ShowRecipientSelection()
        {
            pnlListSelection.Visible = true;
            SetNavigationHistory( pnlListSelection );
        }

        /// <summary>
        /// Loads the common communication segment filters along with any additional filters that are defined for the selected communication list
        /// </summary>
        private void LoadCommunicationSegmentFilters()
        {
            var rockContext = new RockContext();

            // load common communication segments (each communication list may have additional segments)
            var dataviewService = new DataViewService( rockContext );
            var categoryIdCommunicationSegments = CategoryCache.Get( Rock.SystemGuid.Category.DATAVIEW_COMMUNICATION_SEGMENTS.AsGuid() ).Id;
            var commonSegmentDataViewList = dataviewService.Queryable().AsNoTracking().Where( a => a.CategoryId == categoryIdCommunicationSegments ).OrderBy( a => a.Name ).ToList();

            var selectedDataViewIds = cblCommunicationGroupSegments.Items.OfType<ListItem>().Where( a => a.Selected ).Select( a => a.Value ).AsIntegerList();

            cblCommunicationGroupSegments.Items.Clear();
            foreach ( var commonSegmentDataView in commonSegmentDataViewList )
            {
                if ( commonSegmentDataView.IsAuthorized( Rock.Security.Authorization.VIEW, this.CurrentPerson ) )
                {
                    cblCommunicationGroupSegments.Items.Add( new ListItem( commonSegmentDataView.Name, commonSegmentDataView.Id.ToString() ) );
                }
            }

            // reselect any segments that they previously select (if they exist)
            cblCommunicationGroupSegments.SetValues( selectedDataViewIds );

            int? communicationGroupId = ddlCommunicationGroupList.SelectedValue.AsIntegerOrNull();
            List<Guid> segmentDataViewGuids = null;
            if ( communicationGroupId.HasValue )
            {
                btnRecipientList.Text = "View List";
                var communicationGroup = new GroupService( rockContext ).Get( communicationGroupId.Value );
                if ( communicationGroup != null )
                {
                    communicationGroup.LoadAttributes();
                    var segmentAttribute = AttributeCache.Get( Rock.SystemGuid.Attribute.GROUP_COMMUNICATION_LIST_SEGMENTS.AsGuid() );
                    segmentDataViewGuids = communicationGroup.GetAttributeValue( segmentAttribute.Key ).SplitDelimitedValues().AsGuidList();
                    var additionalSegmentDataViewList = dataviewService.GetByGuids( segmentDataViewGuids ).OrderBy( a => a.Name ).ToList();

                    foreach ( var additionalSegmentDataView in additionalSegmentDataViewList )
                    {
                        if ( additionalSegmentDataView.IsAuthorized( Rock.Security.Authorization.VIEW, this.CurrentPerson ) )
                        {
                            if ( commonSegmentDataViewList.Where( v => v.Guid == additionalSegmentDataView.Guid ).Any() )
                            {
                                // This was already added so just move along...
                                continue;
                            }

                            cblCommunicationGroupSegments.Items.Add( new ListItem( additionalSegmentDataView.Name, additionalSegmentDataView.Id.ToString() ) );
                        }
                    }
                }
            }
            else
            {
                btnRecipientList.Text = "Manual List";
            }

            pnlCommunicationGroupSegments.Visible = cblCommunicationGroupSegments.Items.Count > 0;
        }

        /// <summary>
        /// Handles the Click event of the btnRecipientSelectionNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRecipientSelectionNext_Click( object sender, EventArgs e )
        {
            nbRecipientsAlert.Visible = false;
            pnlHeadingLabels.Visible = false;
            var recipients = GetRecipientFromListSelection();
            recipients.ToList();
            if ( !recipients.Any() )
            {
                nbRecipientsAlert.Text = "The selected list doesn't have any people. <span>At least one recipient is required.</span>";
                nbRecipientsAlert.Visible = true;
                return;
            }
            else
            {
                hfRSVPPersonIDs.Value = recipients.Select( m => m.PersonId ).ToList().AsDelimited( "," );
            }

            pnlListSelection.Visible = false;
            ShowCommunicationDelivery();
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppAddPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppAddPerson_SelectPerson( object sender, EventArgs e )
        {
            if ( ppAddPerson.PersonId.HasValue )
            {
                if ( !IndividualRecipientPersonIds.Contains( ppAddPerson.PersonId.Value ) )
                {
                    IndividualRecipientPersonIds.Add( ppAddPerson.PersonId.Value );
                    BindIndividualRecipientsPanelControls();
                }

                // clear out the personpicker and have it say "Add Person" again since they are added to the list
                ppAddPerson.SetValue( null );
                ppAddPerson.PersonName = "Add Person";
            }

            UpdateRecipientListCount();
        }

        /// <summary>
        /// Handles the GridRebind event of the gIndividualRecipients control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        private void gIndividualRecipients_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindIndividualRecipientsPanelControls();
        }

        /// <summary>
        /// Handles the GridRebind event of the gRecipientList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        private void gRecipientList_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindCommunicationListRecipientsGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gRecipientList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gRecipientList_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var recipientPerson = e.Row.DataItem as Person;
            var lRecipientListAlert = e.Row.FindControl( "lRecipientListAlert" ) as Literal;
            var lRecipientListAlertEmail = e.Row.FindControl( "lRecipientListAlertEmail" ) as Literal;
            var lRecipientListAlertSMS = e.Row.FindControl( "lRecipientListAlertSMS" ) as Literal;
            if ( recipientPerson != null && lRecipientListAlert != null )
            {
                string alertClass = string.Empty;
                string alertMessage = string.Empty;
                string alertClassEmail = string.Empty;
                string alertMessageEmail = recipientPerson.Email;
                string alertClassSMS = string.Empty;
                string alertMessageSMS = string.Format( "{0}", recipientPerson.PhoneNumbers.FirstOrDefault( a => a.IsMessagingEnabled ) );

                // General alert info about recipient
                if ( recipientPerson.IsDeceased )
                {
                    alertClass = "text-danger";
                    alertMessage = "Deceased";
                }

                // Email related
                if ( string.IsNullOrWhiteSpace( recipientPerson.Email ) )
                {
                    alertClassEmail = "text-danger";
                    alertMessageEmail = "No Email." + recipientPerson.EmailNote;
                }
                else if ( !recipientPerson.IsEmailActive )
                {
                    // if email is not active, show reason why as tooltip
                    alertClassEmail = "text-danger";
                    alertMessageEmail = "Email is Inactive. " + recipientPerson.EmailNote;
                }
                else
                {
                    // Email is active
                    if ( recipientPerson.EmailPreference != EmailPreference.EmailAllowed )
                    {
                        alertMessageEmail = string.Format( "{0} <span class='label label-warning'>{1}</span>", recipientPerson.Email, recipientPerson.EmailPreference.ConvertToString( true ) );
                        if ( recipientPerson.EmailPreference == EmailPreference.NoMassEmails )
                        {
                            alertClassEmail = "js-no-bulk-email";
                            if ( swBulkCommunication.Checked )
                            {
                                // This is a bulk email and user does not want bulk emails
                                alertClassEmail += " text-danger";
                            }
                        }
                        else
                        {
                            // Email preference is 'Do Not Email'
                            alertClassEmail = "text-danger";
                        }
                    }
                }

                // SMS Related
                if ( !recipientPerson.PhoneNumbers.Any( a => a.IsMessagingEnabled ) )
                {
                    // No SMS Number
                    alertClassSMS = "text-danger";
                    alertMessageSMS = "No phone number with SMS enabled.";
                }

                lRecipientListAlert.Text = string.Format( "<span class=\"{0}\">{1}</span>", alertClass, alertMessage );
                lRecipientListAlertEmail.Text = string.Format( "<span class=\"{0}\">{1}</span>", alertClassEmail, alertMessageEmail );
                lRecipientListAlertSMS.Text = string.Format( "<span class=\"{0}\">{1}</span>", alertClassSMS, alertMessageSMS );
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the gIndividualRecipients control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gIndividualRecipients_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var recipientPerson = e.Row.DataItem as IndividualRecipientListInfo;
            var lRecipientAlert = e.Row.FindControl( "lRecipientAlert" ) as Literal;
            var lRecipientAlertEmail = e.Row.FindControl( "lRecipientAlertEmail" ) as Literal;
            var lRecipientAlertSMS = e.Row.FindControl( "lRecipientAlertSMS" ) as Literal;
            if ( recipientPerson != null && lRecipientAlert != null )
            {
                string alertClass = string.Empty;
                string alertMessage = string.Empty;
                string alertClassEmail = string.Empty;
                string alertMessageEmail = recipientPerson.Email;
                string alertClassSMS = string.Empty;
                string alertMessageSMS = string.Format( "{0}", recipientPerson.SmsPhoneNumber );

                // General alert info about recipient
                if ( recipientPerson.IsDeceased )
                {
                    alertClass = "text-danger";
                    alertMessage = "Deceased";
                }

                // Email related
                if ( string.IsNullOrWhiteSpace( recipientPerson.Email ) )
                {
                    alertClassEmail = "text-danger";
                    alertMessageEmail = "No Email." + recipientPerson.EmailNote;
                }
                else if ( !recipientPerson.IsEmailActive )
                {
                    // if email is not active, show reason why as tooltip
                    alertClassEmail = "text-danger";
                    alertMessageEmail = "Email is Inactive. " + recipientPerson.EmailNote;
                }
                else
                {
                    // Email is active
                    if ( recipientPerson.EmailPreference != EmailPreference.EmailAllowed )
                    {
                        alertMessageEmail = string.Format( "{0} <span class='label label-warning'>{1}</span>", recipientPerson.Email, recipientPerson.EmailPreference.ConvertToString( true ) );
                        if ( recipientPerson.EmailPreference == EmailPreference.NoMassEmails )
                        {
                            alertClassEmail = "js-no-bulk-email";
                            if ( swBulkCommunication.Checked )
                            {
                                // This is a bulk email and user does not want bulk emails
                                alertClassEmail += " text-danger";
                            }
                        }
                        else
                        {
                            // Email preference is 'Do Not Email'
                            alertClassEmail = "text-danger";
                        }
                    }
                }

                // SMS Related
                if ( recipientPerson.SmsPhoneNumber.IsNullOrWhiteSpace() )
                {
                    // No SMS Number
                    alertClassSMS = "text-danger";
                    alertMessageSMS = "No phone number with SMS enabled.";
                }

                lRecipientAlert.Text = string.Format( "<span class=\"{0}\">{1}</span>", alertClass, alertMessage );
                lRecipientAlertEmail.Text = string.Format( "<span class=\"{0}\">{1}</span>", alertClassEmail, alertMessageEmail );
                lRecipientAlertSMS.Text = string.Format( "<span class=\"{0}\">{1}</span>", alertClassSMS, alertMessageSMS );
            }
        }

        /// <summary>
        /// Binds the individual recipients grid.
        /// </summary>
        private void BindIndividualRecipientsPanelControls()
        {
            var panelModel = new IndividualRecipientsPanelViewModel
            {
                RecipientPersonIdList = this.IndividualRecipientPersonIds.ToList(),
                SortProperty = gIndividualRecipients.SortProperty
            };

            var showIndividualRecipientsSummary = panelModel.RecipientPersonIdList.Count > 1000;

            if ( showIndividualRecipientsSummary )
            {
                // Show the Summary Panel.
                lIndividualRecipientPanelCaption.Text = "Below is a summary of your current recipients. You can add individuals to this list before continuing.";

                var panelInfo = panelModel.GetRecipientStatusSummaryValues();

                var template = @"
{% assign dangerItems = SummaryItems | Where:'StatusName','Danger' %}
{% for item in dangerItems %}
    <div class='row'>
        <div class='col col-md-4'></div>
        <div class='col col-md-1 text-right'>
            <span class='badge badge-danger'>{{ item.Value }}</span>
        </div>
        <div class='col col-md-3'>
            {{ item.Label }}
        </div>
    </div>
{% endfor %}
<br>
{% assign warningItems = SummaryItems | Where:'StatusName','Warn' %}
{% for item in warningItems %}
<div class='row'>
    <div class='col col-md-4'></div>
    <div class='col col-md-1 text-right'>
        <span class='badge badge-warning'>{{ item.Value }}</span>
    </div>
    <div class='col col-md-3'>
        {{ item.Label }}
    </div>
</div>
{% endfor %}
";
                var lavaDictionary = new LavaDataDictionary();
                lavaDictionary.Add( "SummaryItems", panelInfo );
                lRecipientSummary.Text = template.ResolveMergeFields( lavaDictionary );

                pnlIndividualRecipientSummary.Visible = true;
            }
            else
            {
                // Show the Individual Recipients List.
                lIndividualRecipientPanelCaption.Text = "Below is a listing of your current recipients. You can add or remove individuals from this list before continuing.";

                var qryRecipients = panelModel.GetListItems();

                // Bind the list items to the grid.
                gIndividualRecipients.SetLinqDataSource( qryRecipients );
                gIndividualRecipients.DataBind();

            }

            pnlIndividualRecipientSummary.Visible = showIndividualRecipientsSummary;
            pnlIndividualRecipientList.Visible = !showIndividualRecipientsSummary;
        }

        /// <summary>
        /// Binds the communication list recipients grid.
        /// </summary>
        private void BindCommunicationListRecipientsGrid()
        {
            nbListWarning.Visible = true;
            var listGroupId = ddlCommunicationGroupList.SelectedValue.AsIntegerOrNull();

            if ( listGroupId != null )
            {
                var listGroupName = ddlCommunicationGroupList.SelectedItem.Text;

                nbListWarning.Text = string.Format( "Below are the current members of the \"{0}\" List with segment filters applied.\nIf this message is sent at a future date, it is possible that the list may change between now and then.", listGroupName );
            }

            // Get the list of recipients.
            var rockContext = new RockContext();
            var segmentDataViewIds = cblCommunicationGroupSegments.Items.OfType<ListItem>().Where( a => a.Selected ).Select( a => a.Value ).AsIntegerList();
            var segmentCriteria = rblCommunicationGroupSegmentFilterType.SelectedValueAsEnum<SegmentCriteria>( SegmentCriteria.Any );
            var recipientIdList = Rock.Model.Communication.GetCommunicationListMembers( rockContext, listGroupId, segmentCriteria, segmentDataViewIds )
                .Select( x => x.PersonId )
                .ToList();
            var personService = new PersonService( rockContext );
            var qryPersons = personService
                .Queryable( true )
                .AsNoTracking()
                .Where( a => recipientIdList.Contains( a.Id ) )
                .Include( a => a.PhoneNumbers )
                .OrderBy( a => a.LastName )
                .ThenBy( a => a.NickName );

            // Bind the list items to the grid.
            gRecipientList.SetLinqDataSource( qryPersons );
            gRecipientList.DataBind();
        }

        /// <summary>
        /// Handles the DeleteClick event of the gIndividualRecipients control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gIndividualRecipients_DeleteClick( object sender, RowEventArgs e )
        {
            this.IndividualRecipientPersonIds.Remove( e.RowKeyId );
            BindIndividualRecipientsPanelControls();
            UpdateRecipientListCount();

            // upnlContent has UpdateMode = Conditional and this is a modal, so we have to update manually
            upnlContent.Update();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblCommunicationGroupSegments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblCommunicationGroupSegments_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateRecipientListCount();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlCommunicationGroupList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlCommunicationGroupList_SelectedIndexChanged( object sender, EventArgs e )
        {
            // reload segments in case the communication list has additional segments
            LoadCommunicationSegmentFilters();

            UpdateRecipientListCount();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblCommunicationGroupSegmentFilterType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblCommunicationGroupSegmentFilterType_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateRecipientListCount();
        }

        /// <summary>
        /// Updates the recipient list count.
        /// </summary>
        private void UpdateRecipientListCount()
        {
            bool isCommunicationGroup = IndividualRecipientPersonIds.Count == 0;

            int listCount = 0;

            if ( isCommunicationGroup )
            {
                // Communication List Count
                IQueryable<GroupMember> groupMemberQuery = GetRecipientFromListSelection();

                if ( groupMemberQuery != null )
                {
                    listCount = groupMemberQuery.Count();
                    pnlRecipientFromListCount.Visible = true;

                    lRecipientFromListCount.Text = string.Format( "Recipients: {0}", listCount );
                }
                else
                {
                    pnlRecipientFromListCount.Visible = false;
                }

                pnlRecipientFromListCount.Visible = listCount > 0;
            }
            else
            {
                // Individuals Selection Count.
                listCount = this.IndividualRecipientPersonIds.Count();
            }

            // Refresh the individual list count, to address the case where the last item in the selection list has been removed.
            lIndividualRecipientListCount.Text = string.Format( "Recipients: {0}", listCount );

            pnlIndividualRecipientListCount.Visible = listCount > 0;
            
            // Keep track of the recipient count.
            this.RecipientCount = listCount;
        }

        /// <summary>
        /// Gets the GroupMember Query for the recipients selected on the 'Select From List' tab
        /// </summary>
        /// <param name="groupMemberQuery">The group member query.</param>
        /// <returns></returns>
        private IQueryable<GroupMember> GetRecipientFromListSelection()
        {
            int? communicationGroupId = ddlCommunicationGroupList.SelectedValue.AsIntegerOrNull();
            var segmentFilterType = rblCommunicationGroupSegmentFilterType.SelectedValueAsEnum<SegmentCriteria>();
            var segmentDataViewIds = cblCommunicationGroupSegments.Items.OfType<ListItem>().Where( a => a.Selected ).Select( a => a.Value.AsInteger() ).ToList();
            var rockContext = new RockContext();

            return Rock.Model.Communication.GetCommunicationListMembers( rockContext, communicationGroupId, segmentFilterType, segmentDataViewIds );
        }

        /// <summary>
        /// Handles the Click event of the btnDeleteSelectedRecipients control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDeleteSelectedRecipients_Click( object sender, EventArgs e )
        {
            // get the selected personIds
            bool removeAll = false;
            var selectField = gIndividualRecipients.ColumnsOfType<SelectField>().First();
            if ( selectField != null && selectField.HeaderCheckbox != null )
            {
                // if the 'Select All' checkbox in the header is checked, and they haven't unselected anything, then assume they want to remove all recipients
                removeAll = selectField.HeaderCheckbox.Checked && gIndividualRecipients.SelectedKeys.Count == gIndividualRecipients.PageSize;
            }

            if ( removeAll )
            {
                this.IndividualRecipientPersonIds.Clear();
            }
            else
            {
                var selectedPersonIds = gIndividualRecipients.SelectedKeys.OfType<int>().ToList();
                this.IndividualRecipientPersonIds.RemoveAll( selectedPersonIds );
            }

            BindIndividualRecipientsPanelControls();

            UpdateRecipientListCount();

            // upnlContent has UpdateMode = Conditional and this is a modal, so we have to update manually
            upnlContent.Update();
        }

        /// <summary>
        /// Handles the Click event of the btnRecipientList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRecipientList_Click( object sender, EventArgs e )
        {
            var communicationGroupListId = ddlCommunicationGroupList.SelectedValue.AsIntegerOrNull();
            if ( !communicationGroupListId.HasValue )
            {
                pnlHeadingLabels.Visible = false;
                pnlListSelection.Visible = false;
                ShowManualList();
            }
            else
            {
                BindCommunicationListRecipientsGrid();
                mdCommunicationListRecipients.Show();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnRecipientListNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRecipientListNext_Click( object sender, EventArgs e )
        {
            pnlHeadingLabels.Visible = false;
            nbRecipientsAlert.Visible = false;

            if ( !this.IndividualRecipientPersonIds.Any() )
            {
                nbIndividualListWarning.Text = "At least one recipient is required.";
                nbIndividualListWarning.Visible = true;
                return;
            }
            else
            {
                hfRSVPPersonIDs.Value = this.IndividualRecipientPersonIds.ToList().AsDelimited( "," );
            }

            pnlIndividualRecipientPanel.Visible = false;
            ShowCommunicationDelivery();
        }

        /// <summary>
        /// Shows the manual list.
        /// </summary>
        private void ShowManualList()
        {
            pnlIndividualRecipientPanel.Visible = true;
            SetNavigationHistory( pnlIndividualRecipientPanel );
        }
        #endregion Recipient Selection

        #region Communication Delivery, Medium Selection

        /// <summary>
        /// Shows the Communication Delivery, Medium Selection
        /// </summary>
        private void ShowCommunicationDelivery()
        {
            pnlCommunicationDelivery.Visible = true;
            SetNavigationHistory( pnlCommunicationDelivery );
        }

        private void SetupCommunicationMediumSection()
        {
            if ( rblCommunicationMedium.Items.Count > 0 )
            {
                return;
            }

            // See what is allowed by the block settings
            var allowedCommunicationTypes = GetAllowedCommunicationTypes( true );
            var emailTransportEnabled = _emailTransportEnabled && allowedCommunicationTypes.Contains( CommunicationType.Email );
            var smsTransportEnabled = _smsTransportEnabled && allowedCommunicationTypes.Contains( CommunicationType.SMS );
            var pushTransportEnabled = _pushTransportEnabled && allowedCommunicationTypes.Contains( CommunicationType.PushNotification );
            var recipientPreferenceEnabled = allowedCommunicationTypes.Contains( CommunicationType.RecipientPreference );

            // only prompt for Medium Type if more than one will be visible
            if ( emailTransportEnabled )
            {
                rblCommunicationMedium.Items.Add( new ListItem( "Email", CommunicationType.Email.ConvertToInt().ToString() ) );
            }

            if ( smsTransportEnabled )
            {
                rblCommunicationMedium.Items.Add( new ListItem( "SMS", CommunicationType.SMS.ConvertToInt().ToString() ) );
            }

            if ( pushTransportEnabled )
            {
                rblCommunicationMedium.Items.Add( new ListItem( "Push", CommunicationType.PushNotification.ConvertToInt().ToString() ) );
            }

            // Only add recipient preference if at least two options exists.
            if ( recipientPreferenceEnabled && ( emailTransportEnabled || smsTransportEnabled ) )
            {
                rblCommunicationMedium.Items.Add( new ListItem( "Recipient Preference", CommunicationType.RecipientPreference.ConvertToInt().ToString() ) );
            }

            rblCommunicationMedium.Visible = rblCommunicationMedium.Items.Count > 1;

            // make sure that either EMAIL, SMS, or PUSH is enabled
            if ( !( emailTransportEnabled || smsTransportEnabled || pushTransportEnabled || recipientPreferenceEnabled ) )
            {
                nbNoCommunicationTransport.Text = "There are no active Email, SMS, or Push communication transports configured.";
                nbNoCommunicationTransport.Visible = true;
                btnCommunicationDeliveryNext.Enabled = false;
            }
            else
            {
                rblCommunicationMedium.SelectedIndex = 0;
                nbNoCommunicationTransport.Visible = false;
                btnCommunicationDeliveryNext.Enabled = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCommunicationDeliveryPrevious control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCommunicationDeliveryPrevious_Click( object sender, EventArgs e )
        {
            pnlCommunicationDelivery.Visible = false;

            if ( IndividualRecipientPersonIds.Count > 0 )
            {
                ShowManualList();
            }
            else
            {
                ShowRecipientSelection();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCommunicationDeliveryNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCommunicationDeliveryNext_Click( object sender, EventArgs e )
        {
            if ( !chkSendImmediately.Checked )
            {
                if ( !dtpSendCommunicationDateTime.SelectedDateTime.HasValue )
                {
                    nbNoCommunicationTransport.NotificationBoxType = NotificationBoxType.Danger;
                    nbNoCommunicationTransport.Text = "Send Date Time is required";
                    nbNoCommunicationTransport.Visible = true;
                    return;
                }
                else if ( dtpSendCommunicationDateTime.SelectedDateTime.Value < RockDateTime.Now )
                {
                    nbNoCommunicationTransport.NotificationBoxType = NotificationBoxType.Danger;
                    nbNoCommunicationTransport.Text = "Send Date Time must be immediate or a future date/time";
                    nbNoCommunicationTransport.Visible = true;
                }
            }

            lTitle.Text = tbCommunicationName.Text.FormatAsHtmlTitle();

            nbNoCommunicationTransport.Visible = false;

            pnlCommunicationDelivery.Visible = false;

            ShowTemplateSelection();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdScheduleSend control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdScheduleSend_SaveClick( object sender, EventArgs e )
        {
            if ( !chkSendImmediately.Checked )
            {
                if ( !dtpSendCommunicationDateTime.SelectedDateTime.HasValue )
                {
                    nbSendCommunicationDateTimeWarning.NotificationBoxType = NotificationBoxType.Danger;
                    nbSendCommunicationDateTimeWarning.Text = "Send Date Time is required";
                    nbSendCommunicationDateTimeWarning.Visible = true;
                    return;
                }
                else if ( dtpSendCommunicationDateTime.SelectedDateTime.Value < RockDateTime.Now )
                {
                    nbSendCommunicationDateTimeWarning.NotificationBoxType = NotificationBoxType.Danger;
                    nbSendCommunicationDateTimeWarning.Text = "Send Date Time must be immediate or a future date/time";
                    nbSendCommunicationDateTimeWarning.Visible = true;
                    return;
                }
            }

            UpdateSendScheduleButton();
            mdScheduleSend.Hide();
        }

        /// <summary>
        /// Updates the send schedule button.
        /// </summary>
        private void UpdateSendScheduleButton()
        {
            lbScheduleSend.Text = "<i class='fa fa-calendar' aria-hidden='true'></i> Send: " + GetScheduleText( chkSendImmediately.Checked, dtpSendCommunicationDateTime.SelectedDateTime );
        }

        /// <summary>
        /// Gets the schedule text.
        /// </summary>
        /// <param name="sendImmediately">if set to <c>true</c> [send immediately].</param>
        /// <param name="sendDateTime">The send date time.</param>
        /// <returns></returns>
        private string GetScheduleText( bool sendImmediately, DateTime? sendDateTime )
        {
            if ( sendImmediately || sendDateTime == null )
            {
                return "Immediately";
            }

            return sendDateTime.Value.ToString( "dddd, MMMM dd" ) + " at " + sendDateTime.Value.ToShortTimeString().ToLower();
        }

        /// <summary>
        /// Handles the Click event of the lbScheduleSend control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbScheduleSend_Click( object sender, EventArgs e )
        {
            mdScheduleSend.Show();
        }

        #endregion Communication Delivery, Medium Selection

        #region Template Selection

        /// <summary>
        /// Shows the template selection.
        /// </summary>
        private void ShowTemplateSelection()
        {
            var preferences = GetBlockPersonPreferences();

            cpCommunicationTemplate.SetValue( preferences.GetValue( CATEGORY_COMMUNICATION_TEMPLATE ).AsIntegerOrNull() );
            pnlTemplateSelection.Visible = true;
            nbTemplateSelectionWarning.Visible = false;
            SetNavigationHistory( pnlTemplateSelection );
            BindTemplatePicker();
        }

        /// <summary>
        /// Binds the template picker.
        /// </summary>
        private void BindTemplatePicker()
        {
            var rockContext = new RockContext();

            var templateQuery = new CommunicationTemplateService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( a => a.IsActive );

            int? categoryId = cpCommunicationTemplate.SelectedValue.AsIntegerOrNull();
            if ( categoryId.HasValue && categoryId > 0 )
            {
                templateQuery = templateQuery.Where( a => a.CategoryId == categoryId );
            }

            templateQuery = templateQuery.OrderBy( a => a.Name );

            // get list of templates that the current user is authorized to View
            IEnumerable<CommunicationTemplate> templateList = templateQuery.AsNoTracking().ToList().Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, this.CurrentPerson ) ).ToList();

            // If this is an Email (or RecipientPreference) communication, limit to templates that support the email wizard
            var communicationType = SelectedCommunicationType;
            if ( communicationType == CommunicationType.Email || communicationType == CommunicationType.RecipientPreference )
            {
                templateList = templateList.Where( a => a.SupportsEmailWizard() );
            }
            else
            {
                templateList = templateList.Where( a => a.HasSMSTemplate() || a.Guid == Rock.SystemGuid.Communication.COMMUNICATION_TEMPLATE_BLANK.AsGuid() );
            }

            rptSelectTemplate.DataSource = templateList;
            rptSelectTemplate.DataBind();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptSelectTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptSelectTemplate_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            CommunicationTemplate communicationTemplate = e.Item.DataItem as CommunicationTemplate;

            if ( communicationTemplate != null )
            {
                Literal lTemplateImagePreview = e.Item.FindControl( "lTemplateImagePreview" ) as Literal;
                Literal lTemplateName = e.Item.FindControl( "lTemplateName" ) as Literal;
                Literal lTemplateDescription = e.Item.FindControl( "lTemplateDescription" ) as Literal;
                LinkButton btnSelectTemplate = e.Item.FindControl( "btnSelectTemplate" ) as LinkButton;

                if ( communicationTemplate.ImageFileId.HasValue )
                {
                    var imageUrl = string.Format( "~/GetImage.ashx?id={0}", communicationTemplate.ImageFileId );
                    lTemplateImagePreview.Text = string.Format( "<img src='{0}' width='100%'/>", this.ResolveRockUrl( imageUrl ) );
                }
                else
                {
                    lTemplateImagePreview.Text = string.Format( "<img src='{0}'/>", this.ResolveRockUrl( "~/Assets/Images/communication-template-default.svg" ) );
                }

                lTemplateName.Text = communicationTemplate.Name;
                lTemplateDescription.Text = communicationTemplate.Description;
                btnSelectTemplate.CommandName = "CommunicationTemplateId";
                btnSelectTemplate.CommandArgument = communicationTemplate.Id.ToString();

                if ( hfSelectedCommunicationTemplateId.Value == communicationTemplate.Id.ToString() )
                {
                    btnSelectTemplate.AddCssClass( "template-selected" );
                }
                else
                {
                    btnSelectTemplate.RemoveCssClass( "template-selected" );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSelectTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSelectTemplate_Click( object sender, EventArgs e )
        {
            int communicationTemplateId = ( sender as LinkButton ).CommandArgument.AsInteger();

            InitializeFieldsFromCommunicationTemplate( communicationTemplateId );

            btnTemplateSelectionNext_Click( sender, e );
        }

        /// <summary>
        /// Initializes the fields from communication template.
        /// </summary>
        /// <param name="communicationTemplateId">The communication template identifier.</param>
        private void InitializeFieldsFromCommunicationTemplate( int communicationTemplateId )
        {
            hfSelectedCommunicationTemplateId.Value = communicationTemplateId.ToString();
            var communicationTemplate = new CommunicationTemplateService( new RockContext() ).Get( hfSelectedCommunicationTemplateId.Value.AsInteger() );

            // this change is being made explicitly as discussed in #3516
            tbFromName.Text = communicationTemplate.FromName.ResolveMergeFields( Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson ) );
            ebFromAddress.Text = communicationTemplate.FromEmail.ResolveMergeFields( Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson ) );

            // only set the ReplyToEmail, CCEMails, and BCCEmails if the template has one (just in case they already filled these in for this communication
            if ( communicationTemplate.ReplyToEmail.IsNotNullOrWhiteSpace() )
            {
                ebReplyToAddress.Text = communicationTemplate.ReplyToEmail;
            }

            if ( communicationTemplate.CCEmails.IsNotNullOrWhiteSpace() )
            {
                ebCCList.Text = communicationTemplate.CCEmails;
            }

            if ( communicationTemplate.BCCEmails.IsNotNullOrWhiteSpace() )
            {
                ebBCCList.Text = communicationTemplate.BCCEmails;
            }

            hfShowAdditionalFields.Value = ( !string.IsNullOrEmpty( communicationTemplate.ReplyToEmail ) || !string.IsNullOrEmpty( communicationTemplate.CCEmails ) || !string.IsNullOrEmpty( communicationTemplate.BCCEmails ) ).ToTrueFalse().ToLower();

            // set the subject from the template even if it is null so we don't accidentally keep a subject doesn't make sense for the newly selected template
            tbEmailSubject.Text = communicationTemplate.Subject;

            // if this communication already has an Email Content specified, since they picked (or re-picked) a template,
            // we'll have to start over on the EmailEditorHtml since the content is dependent on the template
            hfEmailEditorHtml.Value = communicationTemplate.Message.ResolveMergeFields( Rock.Lava.LavaHelper.GetCommonMergeFields( null ) );

            hfEmailAttachedBinaryFileIds.Value = communicationTemplate.GetAttachments( CommunicationType.Email ).Select( a => a.BinaryFileId ).ToList().AsDelimited( "," );
            UpdateEmailAttachedFiles( false );

            // SMS Fields
            if ( communicationTemplate.SmsFromSystemPhoneNumberId.HasValue )
            {
                var valueItem = ddlSMSFrom.Items.FindByValue( communicationTemplate.SmsFromSystemPhoneNumberId.ToString() );
                if ( valueItem == null )
                {
                    var lookupSystemPhoneNumber = SystemPhoneNumberCache.Get( communicationTemplate.SmsFromSystemPhoneNumberId.GetValueOrDefault() );
                    ddlSMSFrom.Items.Add( new ListItem( lookupSystemPhoneNumber.Name, lookupSystemPhoneNumber.Id.ToString() ) );
                }

                ddlSMSFrom.SetValue( communicationTemplate.SmsFromSystemPhoneNumberId.Value );
            }

            // only set the SMSMessage if the template has one (just in case they already typed in an SMSMessage for this communication
            if ( communicationTemplate.SMSMessage.IsNotNullOrWhiteSpace() )
            {
                tbSMSTextMessage.Text = communicationTemplate.SMSMessage;
            }

            var pushCommunication = new CommunicationDetails
            {
                PushData = communicationTemplate.PushData,
                PushImageBinaryFileId = communicationTemplate.PushImageBinaryFileId,
                PushMessage = communicationTemplate.PushMessage,
                PushTitle = communicationTemplate.PushTitle,
                PushOpenMessage = communicationTemplate.PushOpenMessage,
                PushOpenAction = communicationTemplate.PushOpenAction
            };

            var pushNotificationControl = phPushControl.Controls[0] as PushNotification;
            if ( pushNotificationControl != null )
            {
                pushNotificationControl.SetFromCommunication( pushCommunication );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnTemplateSelectionPrevious control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnTemplateSelectionPrevious_Click( object sender, EventArgs e )
        {
            pnlTemplateSelection.Visible = false;

            ShowCommunicationDelivery();
        }

        /// <summary>
        /// Handles the Click event of the btnTemplateSelectionNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnTemplateSelectionNext_Click( object sender, EventArgs e )
        {
            var communicationType = SelectedCommunicationType;
            CommunicationTemplate selectedTemplate = null;
            int? selectedTemplateId = hfSelectedCommunicationTemplateId.Value.AsIntegerOrNull();
            if ( selectedTemplateId.HasValue )
            {
                selectedTemplate = new CommunicationTemplateService( new RockContext() ).Get( selectedTemplateId.Value );
            }

            if ( selectedTemplate == null ||
                ( ( communicationType == CommunicationType.Email || communicationType == CommunicationType.RecipientPreference ) && !selectedTemplate.SupportsEmailWizard() ) )
            {
                nbTemplateSelectionWarning.Text = "Please select a template.";
                nbTemplateSelectionWarning.Visible = true;
                BindTemplatePicker();
                return;
            }

            nbTemplateSelectionWarning.Visible = false;
            pnlTemplateSelection.Visible = false;

            // The next page should be ShowEmailSummary since this is the Select Email Template Page, but just in case...
            if ( ShouldShowEmail() )
            {
                ShowEmailSummary();
            }
            else if ( ShouldShowSms() )
            {
                ShowMobileTextEditor();
            }
            else if ( ShouldShowPush() )
            {
                ShowPushEditor();
            }
            else
            {
                ShowConfirmation();
            }
        }

        private bool CommunicationOptionCanBeShown( CommunicationType communicationType )
        {
            var allowedCommunicationTypes = GetAllowedCommunicationTypes();
            var communicationTypeIsAllowed = !allowedCommunicationTypes.Any() || allowedCommunicationTypes.Contains( communicationType );

            var selectedCommunicationType = SelectedCommunicationType;
            var communicationTypeIsSelected = selectedCommunicationType == communicationType || ( selectedCommunicationType == CommunicationType.RecipientPreference && communicationType != CommunicationType.PushNotification );

            return communicationTypeIsAllowed && communicationTypeIsSelected;
        }

        private bool ShouldShowEmail()
        {
            return _emailTransportEnabled && CommunicationOptionCanBeShown( CommunicationType.Email );
        }

        private bool ShouldShowSms()
        {
            return _smsTransportEnabled && CommunicationOptionCanBeShown( CommunicationType.SMS );
        }

        private bool ShouldShowPush()
        {
            return _pushTransportEnabled && CommunicationOptionCanBeShown( CommunicationType.PushNotification );
        }

        /// <summary>
        /// Handles the SelectItem event of the cpCommunicationTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpCommunicationTemplate_SelectItem( object sender, EventArgs e )
        {
            var preferences = GetBlockPersonPreferences();

            preferences.SetValue( CATEGORY_COMMUNICATION_TEMPLATE, cpCommunicationTemplate.SelectedValue );
            preferences.Save();

            BindTemplatePicker();
        }

        #endregion Template Selection

        #region Email Editor

        /// <summary>
        /// Shows the email editor.
        /// </summary>
        private void ShowEmailEditor()
        {
            lTitle.Text = "Email Editor";
            tbTestEmailAddress.Text = this.CurrentPerson.Email;

            ifEmailDesigner.Attributes["srcdoc"] = hfEmailEditorHtml.Value;
            pnlEmailEditor.Visible = true;
            upEmailSendTest.Visible = true;
            nbEmailTestResult.Visible = false;
            SetNavigationHistory( pnlEmailEditor );
        }

        /// <summary>
        /// Handles the Click event of the btnEmailEditorPrevious control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEmailEditorPrevious_Click( object sender, EventArgs e )
        {
            pnlEmailEditor.Visible = false;
            upEmailSendTest.Visible = false;
            ShowEmailSummary();
        }

        /// <summary>
        /// Handles the Click event of the btnEmailEditorNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEmailEditorNext_Click( object sender, EventArgs e )
        {
            ifEmailDesigner.Attributes["srcdoc"] = hfEmailEditorHtml.Value;
            pnlEmailEditor.Visible = false;
            upEmailSendTest.Visible = false;
            lTitle.Text = tbCommunicationName.Text.FormatAsHtmlTitle();
            if ( ShouldShowSms() )
            {
                ShowMobileTextEditor();
            }
            else if ( ShouldShowPush() )
            {
                ShowPushEditor();
            }
            else
            {
                ShowConfirmation();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnEmailSendTest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEmailSendTest_Click( object sender, EventArgs e )
        {
            SendTestCommunication( EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() ).Id, nbEmailTestResult );

            // make sure the email designer keeps the HTML source that was there
            ifEmailDesigner.Attributes["srcdoc"] = hfEmailEditorHtml.Value;

            // upnlContent has UpdateMode = Conditional, so we have to update manually
            upnlContent.Update();
        }

        /// <summary>
        /// Sends the test communication.
        /// </summary>
        /// <param name="mediumEntityTypeId">The medium entity type identifier.</param>
        private void SendTestCommunication( int mediumEntityTypeId, NotificationBox nbResult )
        {
            if ( !ValidateSendDateTime( nbResult ) )
            {
                return;
            }

            var communication = UpdateCommunication( new RockContext() );

            if ( communication != null )
            {
                // Using a new context (so that changes in the UpdateCommunication() are not persisted )
                using ( var rockContext = new RockContext() )
                {
                    // store the CurrentPerson's current Email and SMS number so we can restore it after changing them to the Test Email/SMS Number
                    int testPersonId = CurrentPerson.Id;
                    string testPersonOriginalEmailAddress = CurrentPerson.Email;
                    var testPersonOriginalSMSPhoneNumber = CurrentPerson.PhoneNumbers
                                            .Where( p => p.IsMessagingEnabled )
                                            .Select( a => a.Number )
                                            .FirstOrDefault();

                    Rock.Model.Communication testCommunication = null;
                    CommunicationService communicationService = null;

                    try
                    {
                        testCommunication = communication.CloneWithoutIdentity();
                        testCommunication.CreatedByPersonAliasId = this.CurrentPersonAliasId;

                        // removed the AsNoTracking() from the next line because otherwise the Person/PersonAlias is attempted (but fails) to be added as new.
                        testCommunication.CreatedByPersonAlias = new PersonAliasService( rockContext ).Queryable().Where( a => a.Id == this.CurrentPersonAliasId.Value ).Include( a => a.Person ).FirstOrDefault();
                        testCommunication.EnabledLavaCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands );
                        testCommunication.FutureSendDateTime = null;
                        testCommunication.Status = CommunicationStatus.Approved;
                        testCommunication.ReviewedDateTime = RockDateTime.Now;
                        testCommunication.ReviewerPersonAliasId = CurrentPersonAliasId;

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

                        var sendTestToPerson = new PersonService( rockContext ).Get( CurrentPerson.Id );
                        if ( mediumEntityTypeId == EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() ).Id )
                        {
                            testCommunication.Subject = string.Format( "[Test] {0}", communication.Subject );
                            if ( sendTestToPerson.Email != tbTestEmailAddress.Text )
                            {
                                sendTestToPerson.Email = tbTestEmailAddress.Text;
                            }
                        }
                        else if ( mediumEntityTypeId == EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() ).Id )
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
                                    Number = tbTestSMSNumber.Text,
                                    NumberFormatted = PhoneNumber.FormattedNumber( PhoneNumber.DefaultCountryCode(), tbTestSMSNumber.Text ),
                                    ForeignKey = "_ForTestCommunication_"
                                };

                                sendTestToPerson.PhoneNumbers.Add( testPhoneNumber );
                            }
                            else
                            {
                                if ( smsPhoneNumber.Number != tbTestSMSNumber.Text )
                                {
                                    smsPhoneNumber.Number = tbTestSMSNumber.Text;
                                    smsPhoneNumber.NumberFormatted = PhoneNumber.FormattedNumber( smsPhoneNumber.CountryCode, smsPhoneNumber.Number );
                                }
                            }
                        }

                        testRecipient.PersonAliasId = sendTestToPerson.PrimaryAliasId.Value;

                        testRecipient.MediumEntityTypeId = mediumEntityTypeId;

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
                            nbResult.NotificationBoxType = NotificationBoxType.Danger;
                            nbResult.Text = string.Format( "Test communication to <strong>{0}</strong> failed: {1}.", testRecipient.PersonAlias.Person.FullName, testRecipient.StatusNote );
                        }
                        else
                        {
                            nbResult.NotificationBoxType = NotificationBoxType.Success;
                            nbResult.Text = "Test communication has been sent.";
                        }

                        nbResult.Dismissable = true;
                        nbResult.Visible = true;
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
                                if ( mediumEntityTypeId == EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() ).Id )
                                {
                                    if ( personToUpdate.Email != testPersonOriginalEmailAddress )
                                    {
                                        personToUpdate.Email = testPersonOriginalEmailAddress;
                                        restorePersonContext.SaveChanges( disablePrePostProcessing: true );
                                    }
                                }
                                else if ( mediumEntityTypeId == EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() ).Id )
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
        /// Handles the Click event of the btnEmailPreview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEmailPreview_Click( object sender, EventArgs e )
        {
            // make sure the ifEmailDesigner srcdoc gets updated from what was in the email editor
            ifEmailDesigner.Attributes["srcdoc"] = hfEmailEditorHtml.Value;

            upnlEmailPreview.Update();

            var communicationHtml = string.Empty;
            using ( var rockContext = new RockContext() )
            {
                var communication = UpdateCommunication( rockContext );
                var sampleCommunicationRecipient = GetSampleCommunicationRecipient( communication, rockContext );

                Person currentPerson;
                if ( communication.CreatedByPersonAlias != null && communication.CreatedByPersonAlias.Person != null )
                {
                    currentPerson = communication.CreatedByPersonAlias.Person;
                }
                else
                {
                    currentPerson = this.CurrentPerson;
                }

                var commonMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, currentPerson );
                var mergeFields = sampleCommunicationRecipient.CommunicationMergeValues( commonMergeFields );

                communicationHtml = GetEmailPreviewHtml( communication, currentPerson, mergeFields );
            }

            ifEmailPreview.Attributes["srcdoc"] = communicationHtml;

            pnlEmailPreview.Visible = true;
            mdEmailPreview.Show();
        }

        private Rock.Communication.Medium.Email GetEmailMediumWithActiveTransport()
        {
            var emailMediumWithActiveTransport = MediumContainer
                .GetActiveMediumComponentsWithActiveTransports()
                .Where( a => a.EntityType.Guid == Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() )
                .FirstOrDefault();

            if ( emailMediumWithActiveTransport == null )
            {
                return null;
            }

            return emailMediumWithActiveTransport as Rock.Communication.Medium.Email;
        }

        private string GetEmailPreviewHtml( Rock.Model.Communication communication, Person currentPerson, Dictionary<string, object> mergeFields )
        {
            var emailMediumWithActiveTransport = MediumContainer
                .GetActiveMediumComponentsWithActiveTransports()
                .Where( a => a.EntityType.Guid == Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() )
                .FirstOrDefault();

            string communicationHtml = hfEmailEditorHtml.Value;

            if ( emailMediumWithActiveTransport != null )
            {
                var mediumAttributes = new Dictionary<string, string>();
                foreach ( var attr in emailMediumWithActiveTransport.Attributes.Select( a => a.Value ) )
                {
                    string value = emailMediumWithActiveTransport.GetAttributeValue( attr.Key );
                    if ( value.IsNotNullOrWhiteSpace() )
                    {
                        mediumAttributes.Add( attr.Key, value );
                    }
                }

                string publicAppRoot = GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" );

                // Add HTML view
                // Get the unsubscribe content and add a merge field for it
                if ( communication.IsBulkCommunication && mediumAttributes.ContainsKey( "UnsubscribeHTML" ) )
                {
                    string unsubscribeHtml = emailMediumWithActiveTransport.Transport.ResolveText( mediumAttributes["UnsubscribeHTML"], currentPerson, communication.EnabledLavaCommands, mergeFields, publicAppRoot );
                    mergeFields.AddOrReplace( "UnsubscribeOption", unsubscribeHtml );
                    communicationHtml = emailMediumWithActiveTransport.Transport.ResolveText( communicationHtml, currentPerson, communication.EnabledLavaCommands, mergeFields, publicAppRoot );

                    // Resolve special syntax needed if option was included in global attribute
                    if ( Regex.IsMatch( communicationHtml, @"\[\[\s*UnsubscribeOption\s*\]\]" ) )
                    {
                        communicationHtml = Regex.Replace( communicationHtml, @"\[\[\s*UnsubscribeOption\s*\]\]", unsubscribeHtml );
                    }

                    // Add the unsubscribe option at end if it wasn't included in content
                    if ( !communicationHtml.Contains( unsubscribeHtml ) )
                    {
                        communicationHtml += unsubscribeHtml;
                    }
                }
                else
                {
                    communicationHtml = emailMediumWithActiveTransport.Transport.ResolveText( communicationHtml, currentPerson, communication.EnabledLavaCommands, mergeFields, publicAppRoot );
                    communicationHtml = Regex.Replace( communicationHtml, @"\[\[\s*UnsubscribeOption\s*\]\]", string.Empty );
                }

                if ( communication.CommunicationTemplate != null && communication.CommunicationTemplate.CssInliningEnabled )
                {
                    communicationHtml = communicationHtml.ConvertHtmlStylesToInlineAttributes();
                }
            }

            return communicationHtml;
        }
        #endregion Email Editor

        #region Email Summary

        /// <summary>
        /// Shows the email summary.
        /// </summary>
        private void ShowEmailSummary()
        {
            lTitle.Text = tbCommunicationName.Text.FormatAsHtmlTitle();

            // See if the template supports preview-text
            HtmlAgilityPack.HtmlDocument templateDoc = new HtmlAgilityPack.HtmlDocument();
            templateDoc.LoadHtml( hfEmailEditorHtml.Value );
            var preheaderTextNode = templateDoc.GetElementbyId( "preheader-text" );
            tbEmailPreview.Visible = preheaderTextNode != null;
            tbEmailPreview.Text = preheaderTextNode != null ? preheaderTextNode.InnerHtml : string.Empty;

            pnlEmailSummary.Visible = true;
            SetNavigationHistory( pnlEmailSummary );
        }

        /// <summary>
        /// Handles the Click event of the btnEmailSummaryPrevious control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEmailSummaryPrevious_Click( object sender, EventArgs e )
        {
            pnlTemplateSelection.Visible = true;
            BindTemplatePicker();

            pnlEmailSummary.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the btnEmailSummaryNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEmailSummaryNext_Click( object sender, EventArgs e )
        {
            if ( tbEmailPreview.Visible )
            {
                // set the preheader-text of our email html
                HtmlAgilityPack.HtmlDocument templateDoc = new HtmlAgilityPack.HtmlDocument();
                templateDoc.LoadHtml( hfEmailEditorHtml.Value );
                var preheaderTextNode = templateDoc.GetElementbyId( "preheader-text" );
                if ( preheaderTextNode != null )
                {
                    preheaderTextNode.InnerHtml = tbEmailPreview.Text;
                    hfEmailEditorHtml.Value = templateDoc.DocumentNode.OuterHtml;
                }

                tbEmailPreview.Text = preheaderTextNode != null ? preheaderTextNode.InnerHtml : string.Empty;
            }

            pnlEmailSummary.Visible = false;

            ShowEmailEditor();
        }

        /// <summary>
        /// Handles the FileUploaded event of the fupEmailAttachments control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FileUploaderEventArgs"/> instance containing the event data.</param>
        protected void fupEmailAttachments_FileUploaded( object sender, FileUploaderEventArgs e )
        {
            UpdateEmailAttachedFiles( true );
        }

        /// <summary>
        /// Updates the HTML for the Attached Files and optionally adds the file that was just uploaded using the file uploader
        /// </summary>
        private void UpdateEmailAttachedFiles( bool addUploadedFile )
        {
            List<int> attachmentList = hfEmailAttachedBinaryFileIds.Value.SplitDelimitedValues().AsIntegerList();
            if ( addUploadedFile && fupEmailAttachments.BinaryFileId.HasValue )
            {
                if ( !attachmentList.Contains( fupEmailAttachments.BinaryFileId.Value ) )
                {
                    attachmentList.Add( fupEmailAttachments.BinaryFileId.Value );
                }
            }

            hfEmailAttachedBinaryFileIds.Value = attachmentList.AsDelimited( "," );
            fupEmailAttachments.BinaryFileId = null;

            // pre-populate dictionary so that the attachments are listed in the order they were added
            Dictionary<int, string> binaryFileAttachments = attachmentList.ToDictionary( k => k, v => string.Empty );
            using ( var rockContext = new RockContext() )
            {
                var binaryFileInfoList = new BinaryFileService( rockContext ).Queryable()
                        .Where( f => attachmentList.Contains( f.Id ) )
                        .Select( f => new
                        {
                            f.Id,
                            f.FileName
                        } );

                foreach ( var binaryFileInfo in binaryFileInfoList )
                {
                    binaryFileAttachments[binaryFileInfo.Id] = binaryFileInfo.FileName;
                }
            }

            StringBuilder sbAttachmentsHtml = new StringBuilder();
            sbAttachmentsHtml.AppendLine( "<div class='attachment'>" );
            sbAttachmentsHtml.AppendLine( "  <ul class='attachment-content'>" );

            foreach ( var binaryFileAttachment in binaryFileAttachments )
            {
                var attachmentUrl = string.Format( "{0}GetFile.ashx?id={1}", System.Web.VirtualPathUtility.ToAbsolute( "~" ), binaryFileAttachment.Key );
                var removeAttachmentJS = string.Format( "removeAttachment( this, '{0}', '{1}' );", hfEmailAttachedBinaryFileIds.ClientID, binaryFileAttachment.Key );
                sbAttachmentsHtml.AppendLine( string.Format( "    <li><a href='{0}' target='_blank' rel='noopener noreferrer'>{1}</a> <a><i class='fa fa-times' onclick=\"{2}\"></i></a></li>", attachmentUrl, binaryFileAttachment.Value, removeAttachmentJS ) );
            }

            sbAttachmentsHtml.AppendLine( "  </ul>" );
            sbAttachmentsHtml.AppendLine( "</div>" );

            lEmailAttachmentListHtml.Text = sbAttachmentsHtml.ToString();
        }

        #endregion Email Summary

        #region Mobile Text Editor

        /// <summary>
        /// Shows the mobile text editor.
        /// </summary>
        private void ShowMobileTextEditor()
        {
            if ( !ddlSMSFrom.SelectedValue.AsIntegerOrNull().HasValue )
            {
                InitializeSMSFromSender( this.CurrentPerson );
            }

            mfpSMSMessage.MergeFields.Clear();
            mfpSMSMessage.MergeFields.Add( "GlobalAttribute" );
            mfpSMSMessage.MergeFields.Add( "Rock.Model.Person" );

            var currentPersonSMSNumber = this.CurrentPerson.PhoneNumbers.FirstOrDefault( a => a.IsMessagingEnabled );
            tbTestSMSNumber.Text = currentPersonSMSNumber != null ? currentPersonSMSNumber.NumberFormatted : string.Empty;

            // make the PersonId of the First Recipient available to Javascript so that we can do some Lava processing using REST and Javascript
            var rockContext = new RockContext();
            Rock.Model.Communication communication = UpdateCommunication( rockContext );
            var firstRecipient = GetSampleCommunicationRecipient( communication, rockContext );
            if ( firstRecipient != null )
            {
                firstRecipient.PersonAlias = firstRecipient.PersonAlias ?? new PersonAliasService( rockContext ).Get( firstRecipient.PersonAliasId.Value );
                hfSMSSampleRecipientPersonId.Value = firstRecipient.PersonAlias.PersonId.ToString();
            }

            nbSMSTestResult.Visible = false;
            pnlMobileTextEditor.Visible = true;
            SetNavigationHistory( pnlMobileTextEditor );
        }

        /// <summary>
        /// Shows or hides the bulk option.
        /// </summary>
        private void ShowHideIsBulkOption()
        {
            if ( this.SelectedCommunicationType == CommunicationType.Email
                 && GetEmailMediumWithActiveTransport() is Rock.Communication.Medium.Email emailMediumComponent
                 && emailMediumComponent.IsBulkEmailThresholdExceeded( this.RecipientCount ) )
            {
                // Override to unchecked when bulk communication is prevented.
                swBulkCommunication.Checked = false;
                swBulkCommunication.Visible = false;

                // Force bulk communication since the recipient count has exceeded the threshold.
                _isBulkCommunicationForced = true;
            }
            else
            {
                swBulkCommunication.Visible = true;

                // Do not force bulk communication since the recipient count has not exceeded the threshold.
                _isBulkCommunicationForced = false;
            }
        }

        /// <summary>
        /// Handles the FileUploaded event of the fupMobileAttachment control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FileUploaderEventArgs"/> instance containing the event data.</param>
        protected void fupMobileAttachment_FileUploaded( object sender, FileUploaderEventArgs e )
        {
            UpdateMobileAttachedFiles( true );
        }

        /// <summary>
        /// Handles the FileRemoved event of the fupMobileAttachment control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FileUploaderEventArgs"/> instance containing the event data.</param>
        protected void fupMobileAttachment_FileRemoved( object sender, FileUploaderEventArgs e )
        {
            // ensure that the binaryfileid is set to null if the file was removed
            fupMobileAttachment.BinaryFileId = null;
            UpdateMobileAttachedFiles( true );
        }

        /// <summary>
        /// Updates the mobile attached files.
        /// </summary>
        /// <param name="resizeImageIfNeeded">if set to <c>true</c> [resize image if needed].</param>
        protected void UpdateMobileAttachedFiles( bool resizeImageIfNeeded )
        {
            nbMobileAttachmentFileTypeWarning.Visible = false;
            nbMobileAttachmentSizeWarning.Visible = false;
            if ( !fupMobileAttachment.BinaryFileId.HasValue )
            {
                imgSMSImageAttachment.Visible = false;
                imgConfirmationSmsImageAttachment.Visible = false;
                return;
            }

            var rockContext = new RockContext();
            BinaryFileService binaryFileService = new BinaryFileService( rockContext );

            // load binary file from database
            var binaryFile = binaryFileService.Get( fupMobileAttachment.BinaryFileId.Value );
            if ( binaryFile != null )
            {
                if ( binaryFile.MimeType.StartsWith( "image/", StringComparison.OrdinalIgnoreCase ) )
                {
                    System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap( binaryFile.ContentStream );

                    // intentionally tell imageResizer to ignore the 3200x3200 size limit so that we can crop it first before limiting the size.
                    var sizingPlugin = ImageResizer.Configuration.Config.Current.Plugins.Get<ImageResizer.Plugins.Basic.SizeLimiting>();
                    var origLimit = sizingPlugin.Limits.TotalBehavior;
                    sizingPlugin.Limits.TotalBehavior = ImageResizer.Plugins.Basic.SizeLimits.TotalSizeBehavior.IgnoreLimits;

                    var imageStream = binaryFile.ContentStream;

                    try
                    {
                        // Make sure Image is no bigger than maxwidth
                        int maxWidth = this.GetAttributeValue( AttributeKey.MaxSMSImageWidth ).AsIntegerOrNull() ?? 600;
                        imageStream.Seek( 0, SeekOrigin.Begin );
                        System.Drawing.Bitmap croppedBitmap = new System.Drawing.Bitmap( imageStream );

                        if ( croppedBitmap.Width > maxWidth )
                        {
                            string resizeParams = string.Format( "width={0}", maxWidth );
                            MemoryStream croppedAndResizedStream = new MemoryStream();
                            ImageResizer.ResizeSettings resizeSettings = new ImageResizer.ResizeSettings( resizeParams );
                            imageStream.Seek( 0, SeekOrigin.Begin );
                            ImageResizer.ImageBuilder.Current.Build( imageStream, croppedAndResizedStream, resizeSettings );

                            binaryFile.ContentStream = croppedAndResizedStream;

                            rockContext.SaveChanges();
                        }
                    }
                    finally
                    {
                        // set the size limit behavior back to what it was
                        sizingPlugin.Limits.TotalBehavior = origLimit;
                    }

                    string publicAppRoot = GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" );
                    imgSMSImageAttachment.ImageUrl = string.Format( "{0}GetImage.ashx?guid={1}", publicAppRoot, binaryFile.Guid );
                    divAttachmentLoadError.InnerText = "Unable to load attachment from " + imgSMSImageAttachment.ImageUrl;
                    imgSMSImageAttachment.Visible = true;
                    imgSMSImageAttachment.Width = new Unit( 50, System.Web.UI.WebControls.UnitType.Percentage );

                    imgConfirmationSmsImageAttachment.ImageUrl = string.Format( "{0}GetImage.ashx?guid={1}", publicAppRoot, binaryFile.Guid );
                    divConfirmationSmsImageAttachmentLoadError.InnerText = "Unable to load attachment from " + imgSMSImageAttachment.ImageUrl;
                    imgConfirmationSmsImageAttachment.Visible = true;
                    imgConfirmationSmsImageAttachment.Width = new Unit( 50, System.Web.UI.WebControls.UnitType.Percentage );
                }
                else
                {
                    // get a thumbnail based on the file extension
                    var virtualThumbnailFilePath = "~/Assets/Icons/FileTypes/other.png";
                    if ( !string.IsNullOrEmpty( binaryFile.FileName ) )
                    {
                        string fileExtension = Path.GetExtension( binaryFile.FileName ).TrimStart( '.' );
                        virtualThumbnailFilePath = string.Format( "~/Assets/Icons/FileTypes/{0}.png", fileExtension );
                        string physicalFilePath = HttpContext.Current.Request.MapPath( virtualThumbnailFilePath );
                        if ( !File.Exists( physicalFilePath ) )
                        {
                            virtualThumbnailFilePath = "~/Assets/Icons/FileTypes/other.png";
                        }
                    }

                    string publicAppRoot = GlobalAttributesCache.Get().GetValue( "PublicApplicationRoot" );
                    imgSMSImageAttachment.ImageUrl = virtualThumbnailFilePath.Replace( "~/", publicAppRoot );
                    divAttachmentLoadError.InnerText = "Unable to load preview icon from " + imgSMSImageAttachment.ImageUrl;
                    imgSMSImageAttachment.Visible = true;
                    imgSMSImageAttachment.Width = new Unit( 10, System.Web.UI.WebControls.UnitType.Percentage );

                    imgConfirmationSmsImageAttachment.ImageUrl = string.Format( "{0}GetImage.ashx?guid={1}", publicAppRoot, binaryFile.Guid );
                    divConfirmationSmsImageAttachmentLoadError.InnerText = "Unable to load attachment from " + imgSMSImageAttachment.ImageUrl;
                    imgConfirmationSmsImageAttachment.Visible = true;
                    imgConfirmationSmsImageAttachment.Width = new Unit( 50, System.Web.UI.WebControls.UnitType.Percentage );
                }

                if ( Rock.Communication.Transport.Twilio.SupportedMimeTypes.Any( a => binaryFile.MimeType.Equals( a, StringComparison.OrdinalIgnoreCase ) ) )
                {
                    // fully supported, no warning or info about filetype is needed
                    nbMobileAttachmentFileTypeWarning.Visible = false;
                }
                else if ( Rock.Communication.Transport.Twilio.AcceptedMimeTypes.Any( a => binaryFile.MimeType.Equals( a, StringComparison.OrdinalIgnoreCase ) ) )
                {
                    // accepted, but not fully supported. Show an info message
                    nbMobileAttachmentFileTypeWarning.NotificationBoxType = NotificationBoxType.Info;
                    nbMobileAttachmentFileTypeWarning.Text = string.Format( "When sending attachments with MMS; jpg, gif, and png images are supported for all carriers. Files of type <small>{0}</small> are also accepted, but support is dependent upon each carrier and device. So make sure to test sending this to different carriers and phone types to see if it will work as expected.", binaryFile.MimeType );
                    nbMobileAttachmentFileTypeWarning.Visible = true;
                }
                else
                {
                    // might not be accepted, and definitely not fully supported. Show a warning message
                    nbMobileAttachmentFileTypeWarning.Text = string.Format( "When sending attachments with MMS; jpg, gif, and png images are supported for all carriers. However, files of type <small>{0}</small> might not be accepted, and support is dependent upon each carrier and device. So make sure to test sending this to different carriers and phone types to see if it will work as expected.", binaryFile.MimeType );
                    nbMobileAttachmentFileTypeWarning.NotificationBoxType = NotificationBoxType.Warning;
                    nbMobileAttachmentFileTypeWarning.Visible = true;
                }

                if ( binaryFile.FileSize > Rock.Communication.Transport.Twilio.MediaSizeLimitBytes )
                {
                    // bigger than what twilio says it supports
                    nbMobileAttachmentSizeWarning.Visible = true;
                    nbMobileAttachmentSizeWarning.Text = string.Format(
                        "The attached file is {0}MB, which is over the {1}MB media size limit.",
                        binaryFile.FileSize / 1024 / 1024,
                        Rock.Communication.Transport.Twilio.MediaSizeLimitBytes / 1024 / 1024 );
                }

                upnlContent.Update();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnMobileTextEditorPrevious control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMobileTextEditorPrevious_Click( object sender, EventArgs e )
        {
            pnlMobileTextEditor.Visible = false;

            if ( ShouldShowEmail() )
            {
                ShowEmailEditor();
            }
            else
            {
                ShowTemplateSelection();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnMobileTextEditorNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMobileTextEditorNext_Click( object sender, EventArgs e )
        {
            pnlMobileTextEditor.Visible = false;
            if ( ShouldShowPush() )
            {
                ShowPushEditor();
            }
            else
            {
                ShowConfirmation();
            }
        }

        private void ShowPushEditor()
        {
            var rockContext = new RockContext();
            Rock.Model.Communication communication = UpdateCommunication( rockContext );

            pnlPushEditor.Visible = true;
            SetNavigationHistory( pnlPushEditor );
        }

        /// <summary>
        /// Initializes the SMS from sender.
        /// </summary>
        /// <param name="sender">The sender.</param>
        public void InitializeSMSFromSender( Person sender )
        {
            var numbers = SystemPhoneNumberCache.All( false );
            if ( numbers != null )
            {
                foreach ( var number in numbers )
                {
                    var personAliasId = number.AssignedToPersonAliasId;
                    if ( personAliasId.HasValue && sender.Aliases.Any( a => a.Id == personAliasId.Value ) )
                    {
                        ddlSMSFrom.SetValue( number.Id );
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the SelectItem event of the mfpMessage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mfpMessage_SelectItem( object sender, EventArgs e )
        {
            tbSMSTextMessage.Text += mfpSMSMessage.SelectedMergeField;
            mfpSMSMessage.SetValue( string.Empty );
        }

        /// <summary>
        /// Handles the Click event of the btnSMSSendTest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSMSSendTest_Click( object sender, EventArgs e )
        {
            if ( ddlSMSFrom.SelectedValue.IsNullOrWhiteSpace() )
            {
                nbSMSTestResult.NotificationBoxType = NotificationBoxType.Danger;
                nbSMSTestResult.Text = "A 'From' number must be specified.";
                nbSMSTestResult.Dismissable = true;
                nbSMSTestResult.Visible = true;
                return;
            }

            SendTestCommunication( EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() ).Id, nbSMSTestResult );
        }

        #endregion Mobile Text Editor

        #region Push Editor
        /// <summary>
        /// Handles the Click event of the btnPushEditorPrevious control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnPushEditorPrevious_Click( object sender, EventArgs e )
        {
            pnlPushEditor.Visible = false;

            if ( ShouldShowSms() )
            {
                ShowMobileTextEditor();
            }
            else if ( ShouldShowEmail() )
            {
                ShowEmailEditor();
            }
            else
            {
                ShowTemplateSelection();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnPushEditorNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnPushEditorNext_Click( object sender, EventArgs e )
        {
            pnlPushEditor.Visible = false;
            ShowConfirmation();
        }
        #endregion

        #region Confirmation

        /// <summary>
        /// Shows the confirmation.
        /// </summary>
        private void ShowConfirmation()
        {
            pnlConfirmation.Visible = true;
            SetNavigationHistory( pnlConfirmation );

            var sendCount = this.IndividualRecipientPersonIds.Count;
            var recipientList = "Custom List";
            if ( sendCount == 0 )
            {
                sendCount = this.GetRecipientFromListSelection().Count();
                recipientList = ddlCommunicationGroupList.SelectedItem.Text;
            }

            litRecipientCount.Text = sendCount.ToString();
            litCommunicationName.Text = tbCommunicationName.Text;
            litSchedule.Text = GetScheduleText( chkSendImmediately.Checked, dtpSendCommunicationDateTime.SelectedDateTime );
            litRecipientList.Text = recipientList;

            var communicationTemplate = new CommunicationTemplateService( new RockContext() ).Get( hfSelectedCommunicationTemplateId.Value.AsInteger() );
            litTemplate.Text = communicationTemplate.Name;

            litCommunicationMedium.Text = rblCommunicationMedium.SelectedItem.Text;

            SetupConfirmationPreview();

            if ( this.IndividualRecipientPersonIds.Count == 0 )
            {
                sendCount = this.GetRecipientFromListSelection().Count();
            }
            else
            {
                sendCount = this.IndividualRecipientPersonIds.Count();
            }

            var maxRecipients = GetAttributeValue( AttributeKey.MaximumRecipients ).AsIntegerOrNull() ?? int.MaxValue;
            var userCanApprove = IsUserAuthorized( "Approve" );

            if ( sendCount > maxRecipients && !userCanApprove )
            {
                btnSend.Text = "Submit";
            }
            else
            {
                btnSend.Text = "Send";
            }
        }

        /// <summary>
        /// Handles the Click event of the btnConfirmationPrevious control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnConfirmationPrevious_Click( object sender, EventArgs e )
        {
            pnlConfirmation.Visible = false;

            if ( ShouldShowPush() )
            {
                ShowPushEditor();
            }
            else if ( ShouldShowSms() )
            {
                ShowMobileTextEditor();
            }
            else if ( ShouldShowEmail() )
            {
                ShowEmailEditor();
            }
            else
            {
                ShowTemplateSelection();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSend control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSend_Click( object sender, EventArgs e )
        {
            SendCommunication();
        }

        private void SendCommunication()
        {
            if ( !ValidateSendDateTime( nbConfirmation ) )
            {
                return;
            }

            var progressReporter = new SignalRTaskActivityReporter();

            // Define a background task for the bulk send process, because it may take considerable time.
            var taskSend = new Task( () =>
            {
                // Wait for the browser to finish loading the page.
                Task.Delay( 1000 ).Wait();

                progressReporter.StartTask();

                progressReporter.Report( 0, "Working..." );

                var rockContext = new RockContext();

                Rock.Model.Communication communication = null;
                using ( var activity = ObservabilityHelper.StartActivity( "COMMUNICATION: Entry Wizard > Send Communication > Update Communication and Recipients" ) )
                {
                    communication = UpdateCommunication( rockContext );

                    UpdateCommunicationRecipients( communication, rockContext, progressReporter );
                }

                string finalMessage = string.Empty;
                using ( var activity = ObservabilityHelper.StartActivity( "COMMUNICATION: Entry Wizard > Send Communication > Finalizing Communication" ) )
                {
                    progressReporter.Report( 90, "Finalizing Communication..." );

                    int maxRecipients = GetAttributeValue( AttributeKey.MaximumRecipients ).AsIntegerOrNull() ?? int.MaxValue;
                    bool userCanApprove = IsUserAuthorized( "Approve" );
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
                        communication.ReviewerPersonAliasId = CurrentPersonAliasId;

                        if ( communication.FutureSendDateTime.HasValue &&
                                        communication.FutureSendDateTime > RockDateTime.Now )
                        {
                            finalMessage = string.Format(
                                "Communication will be sent {0}.",
                                communication.FutureSendDateTime.Value.ToRelativeDateString( 0 ) );
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

                hfCommunicationId.Value = communication.Id.ToString();

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

                dynamic result = new { ViewCommunicationUrl = _viewCommunicationTemplateUrl.Replace( _viewCommunicationIdPlaceholder, communication.Id.ToString() ) };

                progressReporter.StopTask( finalMessage, false, false, result );
            } );

            // Add a continuation task to handle any exceptions during the send process.
            taskSend.ContinueWith(
                t =>
                {
                    if ( t.Exception != null )
                    {
                        ExceptionLogService.LogException( new Exception( "Send Communication failed.", t.Exception.Flatten().InnerException ) );
                    }

                    progressReporter.StopTask( "Communication send failed. Check the Exception Log for further details.", true, false, null );
                },
                TaskContinuationOptions.OnlyOnFaulted );

            // Show the processing panel.
            pnlConfirmation.Visible = false;
            pnlResult.Visible = true;

            SignalRTaskActivityUiHelper.SetTaskActivityControlMode( pnlResult, SignalRTaskActivityUiHelper.ControlModeSpecifier.Progress );

            // Store the absolute page URL before the request terminates.
            // Set a placeholder value for the navigation URL, to be replaced using client-side script when the task completed notification is sent.
            this.CurrentPageReference.Parameters.AddOrReplace( PageParameterKey.CommunicationId, _viewCommunicationIdPlaceholder );

            var uri = new Uri( Request.UrlProxySafe().ToString() );

            _viewCommunicationTemplateUrl = uri.Scheme + "://" + uri.GetComponents( UriComponents.HostAndPort, UriFormat.UriEscaped ) + CurrentPageReference.BuildUrl();

            hlViewCommunication.NavigateUrl = _viewCommunicationTemplateUrl;

            hlViewCommunication.Visible = this.PageCache.Blocks.Any( a => a.BlockType.Guid == Rock.SystemGuid.BlockType.COMMUNICATION_DETAIL.AsGuid() );

            // Start the background processing task and complete this request.
            // The task will continue to run until complete, delivering client status notifications via the SignalR hub.
            taskSend.Start();
        }

        private string _viewCommunicationIdPlaceholder = Guid.NewGuid().ToString();
        private string _viewCommunicationTemplateUrl = string.Empty;

        /// <summary>
        /// Handles the Click event of the btnSaveAsDraft control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveAsDraft_Click( object sender, EventArgs e )
        {
            if ( !ValidateSendDateTime( nbConfirmation ) )
            {
                return;
            }

            Rock.Model.Communication communication = SaveAsDraft();

            ShowResult( "The communication has been saved.", communication );
        }

        /// <summary>
        /// Handles the Click event of the btnEmailEditorSaveDraft control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEmailEditorSaveDraft_Click( object sender, EventArgs e )
        {
            EditorSaveDraft( nbEmailTestResult, isEmailEditor: true );
        }

        /// <summary>
        /// Handles the Click event of the btnSMSEditorSaveDraft control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSMSEditorSaveDraft_Click( object sender, EventArgs e )
        {
            EditorSaveDraft( nbSMSTestResult );
        }

        /// <summary>
        /// Saves the draft communication and sets an appropriate notification message.
        /// </summary>
        /// <param name="isEmailEditor">if set to <c>true</c> if the editor is the email editor (not SMS).</param>
        private void EditorSaveDraft( NotificationBox notificationBox, bool isEmailEditor = false )
        {
            if ( !ValidateSendDateTime( notificationBox ) )
            {
                return;
            }

            try
            {
                Rock.Model.Communication communication = SaveAsDraft();

                if ( isEmailEditor )
                {
                    ifEmailDesigner.Attributes["srcdoc"] = hfEmailEditorHtml.Value;
                }

                upnlContent.Update();

                notificationBox.NotificationBoxType = NotificationBoxType.Success;
                notificationBox.Text = "The communication has been saved.";
            }
            catch ( Exception ex )
            {
                notificationBox.NotificationBoxType = NotificationBoxType.Danger;
                notificationBox.Text = "The communication could not be saved.";
                ExceptionLogService.LogException( ex );
            }

            notificationBox.Dismissable = true;
            notificationBox.Visible = true;
        }

        /// <summary>
        /// Shows a result for a completed task.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="communication">The communication.</param>
        private void ShowResult( string message, Rock.Model.Communication communication )
        {
            SetNavigationHistory( pnlResult );

            pnlConfirmation.Visible = false;
            pnlResult.Visible = true;

            SignalRTaskActivityUiHelper.SetTaskActivityControlMode( pnlResult, SignalRTaskActivityUiHelper.ControlModeSpecifier.Result );

            TaskActivityNotificationBox.Text = message;

            CurrentPageReference.Parameters.AddOrReplace( PageParameterKey.CommunicationId, communication.Id.ToString() );
            hlViewCommunication.NavigateUrl = CurrentPageReference.BuildUrl();

            // only show the Link if there is a CommunicationDetail block type on this page
            hlViewCommunication.Visible = this.PageCache.Blocks.Any( a => a.BlockType.Guid == Rock.SystemGuid.BlockType.COMMUNICATION_DETAIL.AsGuid() );
        }

        /// <summary>
        /// Displays a message if the send datetime is not valid, and returns true if send datetime is valid
        /// </summary>
        /// <returns></returns>
        private bool ValidateSendDateTime( NotificationBox notificationBox )
        {
            if ( dtpSendCommunicationDateTime.Visible )
            {
                if ( !dtpSendCommunicationDateTime.SelectedDateTime.HasValue )
                {
                    notificationBox.NotificationBoxType = NotificationBoxType.Danger;
                    notificationBox.Text = "Send Date Time is required";
                    notificationBox.Visible = true;
                    return false;
                }
                else if ( dtpSendCommunicationDateTime.SelectedDateTime.Value < RockDateTime.Now )
                {
                    notificationBox.NotificationBoxType = NotificationBoxType.Danger;
                    notificationBox.Text = "Send Date Time must be immediate or a future date/time";
                    notificationBox.Visible = true;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Saves the communication as Draft
        /// </summary>
        /// <returns></returns>
        private Rock.Model.Communication SaveAsDraft()
        {
            using ( var rockContext = new RockContext() )
            using ( var activity = ObservabilityHelper.StartActivity( "COMMUNICATION: Entry Wizard > Save As Draft" ) )
            {
                Rock.Model.Communication communication = UpdateCommunication( rockContext );
                UpdateCommunicationRecipients( communication, rockContext );
                communication.Status = CommunicationStatus.Draft;

                /*
                    1/2/2024 - JPH

                    Rather than leveraging the default EF behavior of inserting each new recipient one-by-one,
                    let's remove them from change tracking and perform a BULK INSERT operation instead, after
                    saving the parent Communication record.

                    We can get away with this because none of the downstream processes (callers of this method)
                    further reference the Communication.Recipients collection. If this changes, we will need
                    to rethink this strategy.

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
                    using ( var bulkInsertActivity = ObservabilityHelper.StartActivity( "COMMUNICATION: Entry Wizard > Save As Draft > Bulk-Insert New Communication Recipients" ) )
                    {
                        foreach ( var recipient in newRecipients )
                        {
                            recipient.CommunicationId = communication.Id;
                        }

                        rockContext.BulkInsert<CommunicationRecipient>( newRecipients );
                    }
                }

                activity?.AddTag( "rock-communication-id", communication.Id );
                activity?.AddTag( "rock-communication-name", communication.Name );

                hfCommunicationId.Value = communication.Id.ToString();

                return communication;
            }
        }

        /// <summary>
        /// Handles the Click event of the lnkEmail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lnkEmail_Click( object sender, EventArgs e )
        {
            ShowHideTabPanel( true, false, false );
        }

        /// <summary>
        /// Handles the Click event of the lnkSms control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lnkSms_Click( object sender, EventArgs e )
        {
            ShowHideTabPanel( false, true, false );
        }

        /// <summary>
        /// Handles the Click event of the lnkPush control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lnkPush_Click( object sender, EventArgs e )
        {
            ShowHideTabPanel( false, false, true );
        }

        /// <summary>
        /// Setups the confirmation preview.
        /// </summary>
        private void SetupConfirmationPreview()
        {
            var communicationEmailHtml = string.Empty;
            var communicationSmsMessage = string.Empty;
            var communicationsTo = string.Empty;

            Rock.Model.Communication communication = null;
            using ( var rockContext = new RockContext() )
            {
                communication = UpdateCommunication( rockContext );
                var sampleCommunicationRecipient = GetSampleCommunicationRecipient( communication, rockContext );

                Person currentPerson;
                if ( communication.CreatedByPersonAlias != null && communication.CreatedByPersonAlias.Person != null )
                {
                    currentPerson = communication.CreatedByPersonAlias.Person;
                }
                else
                {
                    currentPerson = this.CurrentPerson;
                }

                var commonMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, currentPerson );
                var mergeFields = sampleCommunicationRecipient.CommunicationMergeValues( commonMergeFields );
                communicationsTo = sampleCommunicationRecipient.PersonAlias.Person.FullName;
                communicationEmailHtml = GetEmailPreviewHtml( communication, currentPerson, mergeFields );

                if ( communication.SMSMessage.IsNotNullOrWhiteSpace() )
                {
                    communicationSmsMessage = communication.SMSMessage.ResolveMergeFields( mergeFields, currentPerson );
                }
            }

            switch ( SelectedCommunicationType )
            {
                case CommunicationType.Email:
                    UpdateConfirmationEmailTab( communication, communicationEmailHtml );

                    ShowHideConfirmationTabLinks( true, false, false );
                    ShowHideTabPanel( true, false, false );

                    break;
                case CommunicationType.SMS:
                    UpdateConfirmationSmsTab( communication, communicationSmsMessage, communicationsTo );

                    ShowHideConfirmationTabLinks( false, true, false );
                    ShowHideTabPanel( false, true, false );
                    break;
                case CommunicationType.PushNotification:
                    UpdateConfirmationPushTab( communication );

                    ShowHideConfirmationTabLinks( false, false, true );
                    ShowHideTabPanel( false, false, true );
                    break;
                default: // Recipient Preference
                    var allowedCommunicationTypes = GetAllowedCommunicationTypes();
                    var emailTransportEnabled = _emailTransportEnabled && allowedCommunicationTypes.Contains( CommunicationType.Email );
                    var smsTransportEnabled = _smsTransportEnabled && allowedCommunicationTypes.Contains( CommunicationType.SMS );

                    // Recipient preference should not use push.
                    // _pushTransportEnabled && allowedCommunicationTypes.Contains( CommunicationType.PushNotification );
                    var pushTransportEnabled = false;

                    if ( emailTransportEnabled )
                    {
                        UpdateConfirmationEmailTab( communication, communicationEmailHtml );
                    }

                    if ( smsTransportEnabled )
                    {
                        UpdateConfirmationSmsTab( communication, communicationSmsMessage, communicationsTo );
                    }

                    if ( pushTransportEnabled )
                    {
                        UpdateConfirmationPushTab( communication );
                    }

                    ShowHideTabPanel( emailTransportEnabled, !emailTransportEnabled && smsTransportEnabled, !emailTransportEnabled && !smsTransportEnabled && pushTransportEnabled );
                    ShowHideConfirmationTabLinks( emailTransportEnabled, smsTransportEnabled, pushTransportEnabled );
                    break;
            }
        }

        /// <summary>
        /// Updates the confirmation push tab.
        /// </summary>
        /// <param name="communication">The communication.</param>
        private void UpdateConfirmationPushTab( Rock.Model.Communication communication )
        {
            litConfirmationPushTitle.Text = communication.PushTitle;
            litConfirmationPushMessage.Text = communication.PushMessage;

            litConfirmationPushOpenAction.Visible = communication.PushOpenAction != null;
            litConfirmationPushOpenAction.Text = ConvertNameToWords( communication.PushOpenAction.ToStringSafe() );

            var pushData = communication.PushData.FromJsonOrNull<PushData>();

            var openActionDetails = new StringBuilder();
            if ( pushData.MobilePageId != null )
            {
                var pageCache = PageCache.Get( pushData.MobilePageId.Value );
                if ( pageCache != null )
                {
                    openActionDetails.Append( string.Format( "<b>Mobile Page:</b> {0}<br />", pageCache.InternalName ) );
                }
            }

            if ( pushData.MobilePageQueryString != null && pushData.MobilePageQueryString.Keys.Count > 0 )
            {
                openActionDetails.Append( "<b>Mobile Page Query String:</b><br />" );
                foreach ( string key in pushData.MobilePageQueryString.Keys )
                {
                    openActionDetails.Append( string.Format( "{0}: {1}<br />", key, pushData.MobilePageQueryString[key] ) );
                }
            }

            if ( pushData.MobileApplicationId != null )
            {
                openActionDetails.Append( string.Format( "<b>Mobile Application Id:</b> {0}<br />", pushData.MobileApplicationId.Value ) );
            }

            if ( pushData.Url.IsNotNullOrWhiteSpace() )
            {
                openActionDetails.Append( string.Format( "<b>URL:</b> {0}<br />", pushData.Url ) );
            }

            if ( communication.PushOpenMessage.IsNotNullOrWhiteSpace() )
            {
                openActionDetails.Append( string.Format( "<b>Additional Details:</b> {0}<br />", communication.PushOpenMessage ) );
            }

            litConfirmationPushOpenActionDetails.Visible = openActionDetails.Length > 0;
            litConfirmationPushOpenActionDetails.Text = openActionDetails.ToString();
        }

        /// <summary>
        /// Converts the name to words.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        private string ConvertNameToWords( string name )
        {
            return Regex.Replace( name, @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " 1" );
        }

        /// <summary>
        /// Updates the confirmation SMS tab.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="messageText">The message text.</param>
        /// <param name="to">To.</param>
        private void UpdateConfirmationSmsTab( Rock.Model.Communication communication, string messageText, string to )
        {
            litConfirmationSmsFromNumber.Visible = false;
            lblConfirmationSmsMessage.Text = messageText;
            lblConfirmationSmsTo.Text = to;

            var lookupSystemPhoneNumber = SystemPhoneNumberCache.Get( communication.SmsFromSystemPhoneNumberId.GetValueOrDefault() );
            if ( lookupSystemPhoneNumber != null )
            {
                litConfirmationSmsFromNumber.Visible = true;
                litConfirmationSmsFromNumber.Text = string.Format( "{0} ({1})", lookupSystemPhoneNumber.Name, lookupSystemPhoneNumber.Number );
            }
        }

        /// <summary>
        /// Updates the confirmation email tab.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="communicationHtml">The communication HTML.</param>
        private void UpdateConfirmationEmailTab( Rock.Model.Communication communication, string communicationHtml )
        {
            if ( communication != null )
            {
                litEmailConfirmationFrom.Text = string.Format( "{0} ({1})", communication.FromName, communication.FromEmail );
                litEmailConfirmationSubject.Text = communication.Subject;

                litEmailConfirmationReplyTo.Visible = communication.ReplyToEmail.IsNotNullOrWhiteSpace();
                litEmailConfirmationReplyTo.Text = communication.ReplyToEmail;

                litEmailConfirmationCc.Visible = communication.CCEmails.IsNotNullOrWhiteSpace();
                litEmailConfirmationCc.Text = communication.CCEmails;

                litEmailConfirmationBcc.Visible = communication.BCCEmails.IsNotNullOrWhiteSpace();
                litEmailConfirmationBcc.Text = communication.BCCEmails;
            }

            ifConfirmationEmailPreview.Attributes.Add( "onload", "resizeIframe(this)" );
            ifConfirmationEmailPreview.Attributes["srcdoc"] = communicationHtml;
        }

        /// <summary>
        /// Shows the hide confirmation tab links.
        /// </summary>
        /// <param name="showEmail">if set to <c>true</c> [show email].</param>
        /// <param name="showSms">if set to <c>true</c> [show SMS].</param>
        /// <param name="showPush">if set to <c>true</c> [show push].</param>
        private void ShowHideConfirmationTabLinks( bool showEmail, bool showSms, bool showPush )
        {
            lnkEmail.Visible = showEmail;
            lnkSms.Visible = showSms;
            lnkPush.Visible = showPush;
        }

        /// <summary>
        /// Shows the hide tab panel.
        /// </summary>
        /// <param name="isEmail">if set to <c>true</c> [is email].</param>
        /// <param name="isSms">if set to <c>true</c> [is SMS].</param>
        /// <param name="isPush">if set to <c>true</c> [is push].</param>
        private void ShowHideTabPanel( bool isEmail, bool isSms, bool isPush )
        {
            pnlEmailTab.Visible = isEmail;
            pnlSmsTab.Visible = isSms;
            pnlPush.Visible = isPush;

            tabEmail.Attributes["class"] = isEmail ? "active" : string.Empty;
            tabSMS.Attributes["class"] = isSms ? "active" : string.Empty;
            tabPush.Attributes["class"] = isPush ? "active" : string.Empty;
        }

        #endregion

        /// <summary>
        /// Create or update a Communication by applying the current selections and settings.
        /// </summary>
        /// <param name="rockContext"></param>
        /// <param name="activityReporter"></param>
        /// <returns></returns>
        private Rock.Model.Communication UpdateCommunication( RockContext rockContext )
        {
            // Get the current wizard settings for the new Communication.
            var settings = new CommunicationOperationsService.CommunicationProperties();

            settings.CommunicationId = hfCommunicationId.Value.AsInteger();

            settings.SenderPersonAliasId = CurrentPersonAliasId;
            settings.EnabledLavaCommands = GetAttributeValue( AttributeKey.EnabledLavaCommands );

            settings.CommunicationName = tbCommunicationName.Text;
            settings.IsBulkCommunication = _isBulkCommunicationForced || swBulkCommunication.Checked;
            settings.MediumType = SelectedCommunicationType;

            if ( IndividualRecipientPersonIds.Count == 0 )
            {
                settings.CommunicationListGroupId = ddlCommunicationGroupList.SelectedValue.AsIntegerOrNull();
            }

            settings.ExcludeDuplicateRecipientAddress = cbDuplicatePreventionOption.Checked;
            settings.CommunicationGroupSegmentDataViewIds = cblCommunicationGroupSegments.Items.OfType<ListItem>().Where( a => a.Selected ).Select( a => a.Value.AsInteger() ).ToList();

            settings.CommunicationGroupSegmentFilterType = rblCommunicationGroupSegmentFilterType.SelectedValueAsEnum<SegmentCriteria>();

            settings.CommunicationTemplateId = hfSelectedCommunicationTemplateId.Value.AsIntegerOrNull();

            settings.EmailBinaryFileIds = hfEmailAttachedBinaryFileIds.Value.SplitDelimitedValues().AsIntegerList();

            if ( fupMobileAttachment.BinaryFileId.HasValue )
            {
                settings.SmsBinaryFileIds = new List<int> { fupMobileAttachment.BinaryFileId.Value };
            }

            if ( chkSendImmediately.Checked )
            {
                settings.FutureSendDateTime = null;
            }
            else
            {
                settings.FutureSendDateTime = dtpSendCommunicationDateTime.SelectedDateTime;
            }

            var details = new CommunicationDetails();

            details.Subject = tbEmailSubject.Text;
            details.Message = hfEmailEditorHtml.Value;

            details.FromName = tbFromName.Text;
            details.FromEmail = ebFromAddress.Text;
            details.ReplyToEmail = ebReplyToAddress.Text;
            details.CCEmails = ebCCList.Text;
            details.BCCEmails = ebBCCList.Text;

            details.SmsFromSystemPhoneNumberId = ddlSMSFrom.SelectedValue.AsIntegerOrNull();
            details.SMSMessage = tbSMSTextMessage.Text;

            // Get Push notification settings.
            var pushNotificationControl = phPushControl.Controls[0] as PushNotification;

            if ( pushNotificationControl != null )
            {
                pushNotificationControl.UpdateCommunication( details );
            }

            settings.Details = details;

            // Update the Communication by applying the new settings.
            var operationsService = new CommunicationOperationsService();

            var communication = operationsService.CreateOrUpdateCommunication( settings, rockContext );

            return communication;
        }

        /// <summary>
        /// Create or update the recipients of a Communication by applying the current selections and settings.
        /// </summary>
        private void UpdateCommunicationRecipients( Rock.Model.Communication communication, RockContext rockContext, SignalRTaskActivityReporter activityReporter = null )
        {
            if ( communication == null )
            {
                return;
            }

            using ( var activity = ObservabilityHelper.StartActivity( "COMMUNICATION: Entry Wizard > Update Communication Recipients" ) )
            {
                IEnumerable<int> recipientPersonIdList;

                if ( IndividualRecipientPersonIds.Count == 0 )
                {
                    recipientPersonIdList = GetRecipientFromListSelection().Select( a => a.PersonId ).ToList();
                }
                else
                {
                    recipientPersonIdList = IndividualRecipientPersonIds;
                }

                var operationsService = new CommunicationOperationsService();

                operationsService.UpdateCommunicationRecipients( rockContext, communication, recipientPersonIdList, activityReporter );

                // rockContext.SaveChanges() is called deep within the UpdateCommunicationRecipients() call above,
                // so wait until we get back from that method to add the ID tag.
                activity?.AddTag( "rock-communication-id", communication.Id );
                activity?.AddTag( "rock-communication-name", communication.Name );
            }
        }

        /// <summary>
        /// Gets an individual communication recipient for preview and testing purposes.
        /// </summary>
        private CommunicationRecipient GetSampleCommunicationRecipient( Rock.Model.Communication communication, RockContext rockContext )
        {
            using ( var activity = ObservabilityHelper.StartActivity( "COMMUNICATION: Entry Wizard > Get Sample Communication Recipient" ) )
            {
                // Update the recipients in the communication
                UpdateCommunicationRecipients( communication, rockContext );

                // If we have recipients in the communication then just return the first one.
                if ( communication.Recipients.Any() )
                {
                    return communication.Recipients.First();
                }

                var recipientPersonId = 0;

                if ( IndividualRecipientPersonIds.Count == 0 )
                {
                    recipientPersonId = GetRecipientFromListSelection().Select( a => a.PersonId ).FirstOrDefault();
                }
                else
                {
                    recipientPersonId = this.IndividualRecipientPersonIds.FirstOrDefault();
                }

                if ( recipientPersonId == 0 )
                {
                    // If we can't find a recipient, try the current user.
                    recipientPersonId = this.CurrentPersonId.GetValueOrDefault();
                }

                // Create and return a temporary Recipient record.
                var recipient = new CommunicationRecipient();

                recipient.Communication = communication;
                recipient.PersonAlias = new PersonAliasService( rockContext ).GetPrimaryAlias( recipientPersonId );

                // If there are additional merge fields, get the merge values connected to the sample recipient.
                if ( communication.AdditionalMergeFields.Any() )
                {
                    recipient.AdditionalMergeValues = new CommunicationRecipientService( rockContext )
                        .GetByCommunicationId( communication.Id )
                        .Where( cr => cr.PersonAlias.Id == recipient.PersonAlias.Id )
                        .FirstOrDefault()?
                        .AdditionalMergeValues;
                }

                return recipient;
            }
        }

        #region Support Classes

        /// <summary>
        /// A View Model for the Individual Recipients Panel.
        /// </summary>
        private class IndividualRecipientsPanelViewModel
        {
            public SortProperty SortProperty { get; set; } = null;

            public List<int> RecipientPersonIdList { get; set; } = null;

            private IQueryable<Person> GetListQuery()
            {
                List<int> recipientIdList = this.RecipientPersonIdList;

                // Apply sort parameters.
                var rockContext = new RockContext();

                var personService = new PersonService( rockContext );
                var qryPersons = personService
                    .Queryable( true )
                    .AsNoTracking()
                    .Where( a => recipientIdList.Contains( a.Id ) );

                if ( this.SortProperty != null )
                {
                    qryPersons = qryPersons.Sort( this.SortProperty );
                }
                else
                {
                    qryPersons = qryPersons.OrderBy( a => a.LastName ).ThenBy( r => r.NickName );
                }

                return qryPersons;
            }

            /// <summary>
            /// Binds the individual recipients grid.
            /// </summary>
            public IQueryable<IndividualRecipientListInfo> GetListItems()
            {

                var qryPersons = this.GetListQuery();
                List<int> recipientIdList = this.RecipientPersonIdList;

                using ( var rockContext = new RockContext() )
                {
                    var personService = new PersonService( rockContext );
                    var qryRecipients = qryPersons
                        .Include( a => a.PhoneNumbers )
                        .Select( p => new IndividualRecipientListInfo
                        {
                            Id = p.Id,
                            NickName = p.NickName,
                            LastName = p.LastName,
                            FullName = ( p.NickName + " " + p.LastName ).Trim(),
                            Email = p.Email,
                            EmailNote = p.EmailNote,
                            IsEmailActive = p.IsEmailActive,
                            SmsPhoneNumber = p.PhoneNumbers
                                .Where( a => a.IsMessagingEnabled )
                                .Select( pn => pn.NumberFormatted )
                                .FirstOrDefault(),
                            IsDeceased = p.IsDeceased,
                            EmailPreference = p.EmailPreference
                        } );

                    if ( this.SortProperty != null )
                    {
                        qryRecipients = qryRecipients.Sort( this.SortProperty );
                    }
                    else
                    {
                        qryRecipients = qryRecipients.OrderBy( a => a.LastName ).ThenBy( r => r.NickName );
                    }

                    return qryRecipients;
                }
            }

            /// <summary>
            /// Binds the individual recipients grid.
            /// </summary>
            public List<SummaryValueInfo> GetRecipientStatusSummaryValues()
            {
                var values = new List<SummaryValueInfo>();

                var qryPersons = this.GetListQuery();

                var qryRecipients = qryPersons
                    .Select( p => new
                    {
                        Id = p.Id,
                        HasEmailAddress = ( p.Email != null && p.Email.Trim() != string.Empty ),
                        IsEmailActive = p.IsEmailActive,
                        HasSmsPhone = p.PhoneNumbers.Any( a => a.IsMessagingEnabled ),
                        IsDeceased = p.IsDeceased,
                        EmailPreference = p.EmailPreference
                    } ).ToList();

                AddSummaryValue( values, "No Email Address", qryRecipients.Count( r => r.HasEmailAddress == false ), "Danger" );
                AddSummaryValue( values, "No SMS-Enabled Phone", qryRecipients.Count( r => r.HasSmsPhone == false ), "Danger" );
                AddSummaryValue( values, "No Email/SMS", qryRecipients.Count( r => r.HasEmailAddress == false && r.HasSmsPhone == false ), "Danger" );

                AddSummaryValue( values, "Do Not Email", qryRecipients.Count( r => r.EmailPreference == EmailPreference.DoNotEmail ), "Warn" );
                AddSummaryValue( values, "No Mass Email", qryRecipients.Count( r => r.EmailPreference == EmailPreference.NoMassEmails ), "Warn" );
                AddSummaryValue( values, "Inactive Email", qryRecipients.Count( r => r.IsEmailActive == false ), "Warn" );

                return values;
            }

            private void AddSummaryValue( List<SummaryValueInfo> valueCollection, string label, int? value, string statusName = "Info" )
            {
                var info = new SummaryValueInfo
                {
                    Key = label.Replace( " ", string.Empty ),
                    Label = label,
                    Value = value,
                    StatusName = statusName
                };
                valueCollection.Add( info );
            }

            /// <summary>
            /// A line item in the Individual Recipients summary.
            /// </summary>
            public class SummaryValueInfo : RockDynamic
            {
                public string Key { get; set; }
                public string Label { get; set; }
                public int? Value { get; set; }
                public string StatusName { get; set; }
            }
        }

        /// <summary>
        /// Information about an individual recipient that is displayed in the recipient list.
        /// </summary>
        private class IndividualRecipientListInfo
        {
            public int Id { get; set; }
            public string NickName { get; set; }
            public string LastName { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string EmailNote { get; set; }
            public bool IsEmailActive { get; set; }
            public EmailPreference EmailPreference { get; set; }
            public string SmsPhoneNumber { get; set; }
            public bool IsDeceased { get; set; }
        }

        #endregion

        #region Service Classes

        /// <summary>
        /// A service that executes actions and operations related to Communications.
        /// </summary>
        public class CommunicationOperationsService
        {
            /// <summary>
            /// Creates or updates the properties of an existing Communication.
            /// </summary>
            /// <param name="settings"></param>
            /// <returns></returns>
            public Rock.Model.Communication CreateOrUpdateCommunication( CommunicationProperties settings )
            {
                var rockContext = new RockContext();

                var communication = this.CreateOrUpdateCommunication( settings, rockContext );

                rockContext.SaveChanges();

                return communication;
            }

            /// <summary>
            /// Creates or updates the properties of an existing Communication in the provided data context.
            /// Changes to the data context are not persisted.
            /// </summary>
            /// <param name="rockContext"></param>
            /// <param name="settings"></param>
            /// <returns></returns>
            public Rock.Model.Communication CreateOrUpdateCommunication( CommunicationProperties settings, RockContext rockContext )
            {
                var communicationService = new CommunicationService( rockContext );
                var communicationAttachmentService = new CommunicationAttachmentService( rockContext );
                var communicationRecipientService = new CommunicationRecipientService( rockContext );

                Rock.Model.Communication communication = null;

                if ( settings.CommunicationId.GetValueOrDefault( 0 ) > 0 )
                {
                    communication = communicationService.Get( settings.CommunicationId.Value );
                }

                if ( communication == null )
                {
                    communication = new Rock.Model.Communication
                    {
                        Status = CommunicationStatus.Transient,
                        SenderPersonAliasId = settings.SenderPersonAliasId,
                    };
                    communicationService.Add( communication );
                }

                communication.EnabledLavaCommands = settings.EnabledLavaCommands;

                communication.Name = settings.CommunicationName.TrimForMaxLength( communication, "Name" );
                communication.IsBulkCommunication = settings.IsBulkCommunication;
                communication.CommunicationType = settings.MediumType;

                if ( settings.CommunicationListGroupId.HasValue )
                {
                    communication.ListGroupId = settings.CommunicationListGroupId;
                }
                else
                {
                    communication.ListGroup = null;
                    communication.ListGroupId = null;
                }

                communication.ExcludeDuplicateRecipientAddress = settings.ExcludeDuplicateRecipientAddress;
                var segmentDataViewIds = settings.CommunicationGroupSegmentDataViewIds;

                var segmentDataViewGuids = new DataViewService( rockContext ).GetByIds( segmentDataViewIds ).Select( a => a.Guid ).ToList();

                communication.Segments = segmentDataViewGuids.AsDelimited( "," );
                communication.SegmentCriteria = settings.CommunicationGroupSegmentFilterType;

                communication.CommunicationTemplateId = settings.CommunicationTemplateId;

                if ( communication.CommunicationTemplateId.HasValue )
                {
                    communication.CommunicationTemplate = new CommunicationTemplateService( rockContext ).Get( communication.CommunicationTemplateId.Value );
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
                        activity?.AddTag( "rock-communication-id", communication.Id );
                    }

                    activity?.AddTag( "rock-communication-name", communication.Name );

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
                communication.PushTitle = settings.Details.PushTitle;

                return communication;
            }

            /// <summary>
            /// Update the recipients of an existing Communication.
            /// </summary>
            /// <param name="rockContext"></param>
            /// <param name="communicationId"></param>
            /// <param name="recipientPersonIdList"></param>
            /// <param name="progressReporter"></param>
            /// <returns></returns>
            public Rock.Model.Communication UpdateCommunicationRecipients( RockContext rockContext, int communicationId, IEnumerable<int> recipientPersonIdList, IProgress<TaskProgressMessage> progressReporter = null )
            {
                var communicationService = new CommunicationService( rockContext );

                var communication = communicationService.Get( communicationId );

                return UpdateCommunicationRecipients( rockContext, communication, recipientPersonIdList, progressReporter );
            }

            /// <summary>
            /// Update the recipients of a Communication instance.
            /// </summary>
            /// <param name="rockContext"></param>
            /// <param name="communication"></param>
            /// <param name="recipientPersonIdList"></param>
            /// <param name="progressReporter"></param>
            /// <returns></returns>
            public Rock.Model.Communication UpdateCommunicationRecipients( RockContext rockContext, Rock.Model.Communication communication, IEnumerable<int> recipientPersonIdList, IProgress<TaskProgressMessage> progressReporter = null )
            {
                if ( communication == null )
                {
                    throw new ArgumentException( "UpdateCommunicationRecipients failed. A Communication instance is required." );
                }

                ReportProgress( progressReporter, 0, activityMessage: "Creating Recipients List..." );

                var communicationRecipientService = new CommunicationRecipientService( rockContext );

                HashSet<int> communicationPersonIdHash;

                // Get the initial list of Recipients.
                if ( communication.Id != 0 )
                {
                    // This is an existing Communication, so load the recipients list.
                    var personIdHash = new HashSet<int>( recipientPersonIdList );
                    var qryRecipients = communication.GetRecipientsQry( rockContext );

                    // Remove recipients that do not exist in the current selection.
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

                    if ( qryRecipients == null )
                    {
                        qryRecipients = communication.GetRecipientsQry( rockContext );
                    }

                    using ( var personIdHashActivity = ObservabilityHelper.StartActivity( "COMMUNICATION: Entry Wizard > Update Communication Recipients > Select Person ID Hash" ) )
                    {
                        communicationPersonIdHash = new HashSet<int>( qryRecipients.Select( a => a.PersonAlias.PersonId ) );

                        personIdHashActivity?.AddTag( "rock-communication-person-id-hash-count", communicationPersonIdHash.Count );
                    }
                }
                else
                {
                    // This is a new communication with no pre-existing recipient list.
                    communicationPersonIdHash = new HashSet<int>();
                }

                // Add new recipients
                ReportProgress( progressReporter, 5, activityMessage: "Creating Recipients List..." );

                var recipientPersonIdQuery = GetRecipientPersonIdPersistedList( recipientPersonIdList, rockContext );

                if ( recipientPersonIdQuery == null )
                {
                    return null;
                }

                using ( var recipientPersonLookupActivity = ObservabilityHelper.StartActivity( "COMMUNICATION: Entry Wizard > Update Communication Recipients > Create Recipient Person Lookup Dictionary" ) )
                {
                    var recipientPersonsLookup = new PersonService( rockContext ).Queryable().Where( a => recipientPersonIdQuery.Contains( a.Id ) )
                        .Select( a => new
                        {
                            PersonId = a.Id,
                            a.CommunicationPreference,
                            PrimaryAlias = a.Aliases.Where( x => x.AliasPersonId == x.PersonId ).Select( pa => pa ).FirstOrDefault()
                        } )
                        .ToDictionary( k => k.PersonId, v => new { v.CommunicationPreference, v.PrimaryAlias } );

                    ReportProgress( progressReporter, 10, activityMessage: "Creating Recipients List..." );

                    foreach ( var recipientPersonLookup in recipientPersonsLookup )
                    {
                        if ( !communicationPersonIdHash.Contains( recipientPersonLookup.Key ) )
                        {
                            var communicationRecipient = new CommunicationRecipient();
                            communicationRecipient.PersonAlias = recipientPersonLookup.Value.PrimaryAlias;
                            communicationRecipient.PersonAliasId = recipientPersonLookup.Value.PrimaryAlias.Id;
                            communication.Recipients.Add( communicationRecipient );
                        }
                    }

                    ReportProgress( progressReporter, 15, activityMessage: "Creating Recipients List..." );

                    var emailMediumEntityType = EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() );
                    var smsMediumEntityType = EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() );
                    var pushMediumEntityType = EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() );

                    var communicationListGroupMemberCommunicationTypeLookup = new Dictionary<int, CommunicationType>();

                    if ( communication.CommunicationType == CommunicationType.RecipientPreference )
                    {
                        using ( var getRecipientPreferenceActivity = ObservabilityHelper.StartActivity( "COMMUNICATION: Entry Wizard > Update Communication Recipients > Get Recipient Preferences" ) )
                        {
                            var communicationListGroupMemberCommunicationTypeList = new GroupMemberService( rockContext ).Queryable()
                                .Where( a => a.GroupId == communication.ListGroupId.Value && a.GroupMemberStatus == GroupMemberStatus.Active )
                                .ToList();

                            foreach ( var communicationListGroupMemberCommunicationType in communicationListGroupMemberCommunicationTypeList )
                            {
                                var recipientPreference = communicationListGroupMemberCommunicationType.CommunicationPreference;
                                communicationListGroupMemberCommunicationTypeLookup.AddOrIgnore( communicationListGroupMemberCommunicationType.PersonId, recipientPreference );
                            }
                        }
                    }

                    ReportProgress( progressReporter, 20, activityMessage: "Creating Recipients List..." );

                    int totalCount = communication.Recipients.Count;
                    int currentCount = 0;

                    foreach ( var recipient in communication.Recipients )
                    {
                        // GetValueOrNull will default to CommunicationType.RecipientPreference if not found in the dictionary.
                        var groupMemberPreference = communicationListGroupMemberCommunicationTypeLookup.GetValueOrNull( recipient.PersonAlias.PersonId );

                        var recipientPreference = recipientPersonsLookup.ContainsKey( recipient.PersonAlias.PersonId ) ?
                            recipientPersonsLookup[recipient.PersonAlias.PersonId].CommunicationPreference :
                            groupMemberPreference;

                        recipient.MediumEntityTypeId = Rock.Model.Communication.DetermineMediumEntityTypeId(
                            emailMediumEntityType.Id,
                            smsMediumEntityType.Id,
                            pushMediumEntityType.Id,
                            communication.CommunicationType,
                            groupMemberPreference,
                            recipientPreference );

                        currentCount++;

                        ReportProgress( progressReporter, 20 + ( decimal.Divide( currentCount, totalCount ) * 70 ), 0, "Processing Recipients ({0} of {1})...", currentCount, totalCount );
                    }
                }

                return communication;
            }

            private int _lastCompletionPercentage = 0;
            private string _lastActivityMessage = null;

            private void ReportProgress( IProgress<TaskProgressMessage> progressReporter, decimal completionPercentage, long elapsedMilliseconds = 0, string activityMessage = null, params object[] args )
            {
                if ( progressReporter == null )
                {
                    return;
                }

                var message = string.Format( activityMessage, args );

                if ( ( int ) completionPercentage == _lastCompletionPercentage
                     && message == _lastActivityMessage )
                {
                    return;
                }

                progressReporter.Report( TaskProgressMessage.New( completionPercentage, elapsedMilliseconds, message ) );

                _lastCompletionPercentage = ( int ) completionPercentage;
                _lastActivityMessage = message;
            }

            /// <summary>
            /// Creates a new EntitySet containing the list of Recipient Person records and returns a queryable of the identifiers.
            /// The result can be referenced as a subquery, thereby avoiding the need to pass a large list of keys in the query string
            /// that may break the limits of the query parser.
            /// </summary>
            /// <param name="rockContext">The rock context.</param>
            private IQueryable<int> GetRecipientPersonIdPersistedList( IEnumerable<int> personIdList, RockContext rockContext )
            {
                if ( personIdList == null
                     || !personIdList.Any() )
                {
                    return null;
                }

                var service = new EntitySetService( rockContext );

                using ( var activity = ObservabilityHelper.StartActivity( "COMMUNICATION: Entry Wizard > Get Recipient Person Id Persisted List (add new EntitySet)" ) )
                {
                    var args = new AddEntitySetActionOptions
                    {
                        Name = "RecipientPersonEntitySet_Communication",
                        EntityTypeId = Rock.Web.Cache.EntityTypeCache.Get<Rock.Model.Person>().Id,
                        EntityIdList = personIdList,
                        ExpiryInMinutes = 20
                    };
                    var entitySetId = service.AddEntitySet( args );

                    activity?.AddTag( "rock-communication-entity-set-id", entitySetId );

                    var entityQuery = service.GetEntityQuery( entitySetId ).Select( x => x.Id );

                    return entityQuery;
                }
            }

            #region Helper Classes

            /// <summary>
            /// The properties and settings used to create a new Communication.
            /// </summary>
            public class CommunicationProperties
            {
                public int? CommunicationId { get; set; }
                public int? SenderPersonAliasId { get; set; }
                public string EnabledLavaCommands { get; set; }
                public string CommunicationName { get; set; }
                public bool IsBulkCommunication { get; set; }
                public CommunicationType MediumType { get; set; }

                public int? CommunicationListGroupId { get; set; }
                public bool ExcludeDuplicateRecipientAddress { get; set; }
                public List<int> CommunicationGroupSegmentDataViewIds { get; set; }

                public SegmentCriteria CommunicationGroupSegmentFilterType { get; set; }
                public int? CommunicationTemplateId { get; set; }
                public List<int> EmailBinaryFileIds { get; set; }
                public List<int> SmsBinaryFileIds { get; set; }

                public DateTime? FutureSendDateTime { get; set; }

                public CommunicationDetails Details { get; set; } = new CommunicationDetails();
            }

            #endregion
        }

        #endregion
    }
}