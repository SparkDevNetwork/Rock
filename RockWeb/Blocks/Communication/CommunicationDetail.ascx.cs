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
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Humanizer;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    /// Displays details and analytics for an existing communication.
    /// </summary>
    [DisplayName( "Communication Detail" )]
    [Category( "Communication" )]
    [Description( "Used for displaying details of an existing communication that has already been created." )]
    [SecurityAction( Authorization.APPROVE, "The roles and/or users that have access to approve new communications." )]

    /* 11-Feb-2020 - DL
     * This block contains additional script to handle client-side rendering of Charts and Message Content.
     * It is needed to ensure these elements refresh correctly during postback events.
     */

    #region Block Attributes

    [BooleanField
        ( "Enable Personal Templates",
          Key = AttributeKey.EnablePersonalTemplates,
          Description = "Should support for personal templates be enabled? These are templates that a user can create and are personal to them. If enabled, they will be able to create a new template based on the current communication.",
          DefaultValue = "false",
          Order = 0 )]
    [TextField
        ( "Series Colors",
          Key = AttributeKey.SeriesColors,
          Description = "A comma-delimited list of colors that the Clients chart will use.",
          DefaultValue = SeriesColorsDefaultValue,
          Order = 1 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.COMMUNICATION_DETAIL )]
    public partial class CommunicationDetail : RockBlock
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string SeriesColors = "SeriesColors";
            public const string EnablePersonalTemplates = "EnablePersonalTemplates";
        }

        #endregion

        #region Attribute Default Values

        private const string SeriesColorsDefaultValue = "#5DA5DA,#60BD68,#FFBF2F,#F36F13,#C83013,#676766";

        #endregion

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string CommunicationId = "CommunicationId";
            public const string Edit = "Edit";
        }

        #endregion

        #region User Preference Keys

        /// <summary>
        /// Keys to use for User Preferences
        /// </summary>
        private static class UserPreferenceKey
        {
            public const string RecipientListSettings = "RecipientListSettings";
        }

        #endregion

        #region Fields

        private bool _EditingApproved = false;
        private string _ActivePanel = CommunicationDetailPanels.Analytics;
        private List<CommunicationDetailReportColumnInfo> _Columns;
        private Rock.Model.Communication _Communication = null;
        private List<InteractionInfo> _Interactions = null;
        private Dictionary<string, string> _PanelControlToTabNameMap = null;
        private RockContext _DataContext = null;

        #endregion

        #region Properties

        protected int? CommunicationId
        {
            get { return ViewState["CommunicationId"] as int?; }
            set { ViewState["CommunicationId"] = value; }
        }

        /// <summary>
        /// Gets or sets the line chart data labels json.
        /// </summary>
        /// <value>
        /// The line chart data labels json.
        /// </value>
        public string LineChartDataLabelsJSON { get; set; }

        /// <summary>
        /// Gets or sets the line chart data opens json.
        /// </summary>
        /// <value>
        /// The line chart data opens json.
        /// </value>
        public string LineChartDataOpensJSON { get; set; }

        /// <summary>
        /// Gets or sets the line chart data clicks json.
        /// </summary>
        /// <value>
        /// The line chart data clicks json.
        /// </value>
        public string LineChartDataClicksJSON { get; set; }

        /// <summary>
        /// Gets or sets the line chart data un opened json.
        /// </summary>
        /// <value>
        /// The line chart data un opened json.
        /// </value>
        public string LineChartDataUnOpenedJSON { get; set; }

        /// <summary>
        /// Gets or sets the line chart time format. see http://momentjs.com/docs/#/displaying/format/
        /// </summary>
        /// <value>
        /// The line chart time format.
        /// </value>
        public string LineChartTimeFormat { get; set; }

        /// <summary>
        /// Gets or sets the pie chart data open clicks json.
        /// </summary>
        /// <value>
        /// The pie chart data open clicks json.
        /// </value>
        public string PieChartDataOpenClicksJSON { get; set; }

        /// <summary>
        /// Gets or sets the pie chart data client labels json.
        /// </summary>
        /// <value>
        /// The pie chart data client labels json.
        /// </value>
        public string PieChartDataClientLabelsJSON { get; set; }

        /// <summary>
        /// Gets or sets the pie chart data client counts json.
        /// </summary>
        /// <value>
        /// The pie chart data client counts json.
        /// </value>
        public string PieChartDataClientCountsJSON { get; set; }

        /// <summary>
        /// Gets or sets the series colors.
        /// </summary>
        /// <value>
        /// The series colors.
        /// </value>
        public string SeriesColorsJSON { get; set; }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.OnInit" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddCSSLink( "~/Styles/Blocks/Communication/CommunicationDetail.css", true );
            InitializeAnalyticsPanelControls();

            InitializeInteractionsList();
            InitializeRecipientsList();

            InitializeRecipientsFilter();

            InitializeChartScripts();

            _EditingApproved = PageParameter( PageParameterKey.Edit ).AsBoolean() && IsUserAuthorized( "Approve" );

            if ( GetAttributeValue( AttributeKey.EnablePersonalTemplates ).AsBoolean() )
            {
                btnTemplate.Visible = true;
                mdCreateTemplate.SaveClick += mdSaveTemplate_Click;
            }

            InitializeBlockConfigurationChangeHandler( upPanel );
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            _ActivePanel = ( ViewState["ActivePanel"] as string ) ?? string.Empty;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbTemplateCreated.Visible = false;

            if ( Page.IsPostBack )
            {
                var argument = Request.Params.Get( "__EVENTARGUMENT" );

                if ( argument == "ShowPendingRecipients" )
                {
                    ShowRecipientsListForDeliveryStatus( CommunicationRecipientStatus.Pending );
                }
                else if ( argument == "ShowDeliveredRecipients" )
                {
                    ShowRecipientsListForDeliveryStatus( CommunicationRecipientStatus.Delivered );
                }
                else if ( argument == "ShowFailedRecipients" )
                {
                    ShowRecipientsListForDeliveryStatus( CommunicationRecipientStatus.Failed );
                }
                else if ( argument == "ShowCancelledRecipients" )
                {
                    ShowRecipientsListForDeliveryStatus( CommunicationRecipientStatus.Cancelled );
                }
                else
                {
                    // Set the tab page to the parent of the postback control.
                    var targetControl = GetPostBackControl();

                    if ( targetControl != null )
                    {
                        var parentTab = targetControl.FindFirstParentWhere( x => ( x is WebControl ) && ( ( WebControl ) x ).CssClass == "tab-panel" ) as WebControl;

                        if ( parentTab != null )
                        {
                            var panelToTabMap = this.GetPanelControlToTabNameMap();

                            if ( _PanelControlToTabNameMap.ContainsKey( parentTab.UniqueID ) )
                            {
                                var panelName = _PanelControlToTabNameMap[parentTab.UniqueID];

                                SetActivePanel( panelName );
                            }
                        }
                    }

                    ShowDialog();
                }
            }
            else
            {
                InitializePageFromParameters();

                InitializeActiveCommunication();

                if ( _Communication == null ||
                     _Communication.Status == CommunicationStatus.Transient ||
                     _Communication.Status == CommunicationStatus.Draft ||
                     _Communication.Status == CommunicationStatus.Denied ||
                     ( _Communication.Status == CommunicationStatus.PendingApproval && _EditingApproved ) )
                {
                    // If viewing a new, transient or draft communication, hide this block and show the New Communication block.
                    this.Visible = false;
                }
                else
                {
                    // If user is not authorized to View, don't show details. Just a warning.
                    if ( !_Communication.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) )
                    {
                        nbEditModeMessage.NotificationBoxType = NotificationBoxType.Warning;
                        nbEditModeMessage.Text = EditModeMessage.NotAuthorizedToView( Rock.Model.Communication.FriendlyTypeName );

                        pnlCommunicationView.Visible = false;
                    }
                    else
                    {
                        ShowDetail();
                    }
                }
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["ActivePanel"] = _ActivePanel;

            return base.SaveViewState();
        }

        /// <summary>
        /// Store settings for the Recipient List.
        /// </summary>
        private void SaveRecipientListPreferences()
        {
            // Save Recipients List column selection.
            var settings = new RecipientListPreferences();
            var preferences = GetBlockPersonPreferences();

            settings.SelectedProperties = cblProperties.SelectedValues;
            settings.SelectedAttributes = lbAttributes.SelectedValues;

            preferences.SetValue( UserPreferenceKey.RecipientListSettings, settings.ToJson() );
            preferences.Save();
        }

        /// <summary>
        /// Load settings for the Recipient List.
        /// </summary>
        private void LoadRecipientListPreferences()
        {
            // Load Recipients List column selection.
            var preferences = GetBlockPersonPreferences();
            var settings = preferences.GetValue( UserPreferenceKey.RecipientListSettings ).FromJsonOrNull<RecipientListPreferences>();

            if ( settings == null )
            {
                return;
            }

            cblProperties.SetValues( settings.SelectedProperties );
            lbAttributes.SetValues( settings.SelectedAttributes );
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{BreadCrumb}" /> of block related <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<Rock.Web.UI.BreadCrumb> GetBreadCrumbs( Rock.Web.PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();
            var pageTitle = "New Communication";

            var commId = PageParameter( PageParameterKey.CommunicationId ).AsIntegerOrNull();

            if ( commId.HasValue )
            {
                var dataContext = this.GetDataContext();

                var communication = new CommunicationService( dataContext ).Get( commId.Value );

                if ( communication != null )
                {
                    RockPage.SaveSharedItem( "communication", communication );

                    switch ( communication.Status )
                    {
                        case CommunicationStatus.Approved:
                        case CommunicationStatus.Denied:
                        case CommunicationStatus.PendingApproval:
                            {
                                pageTitle = communication.Name ?? string.Format( "Communication #{0}", communication.Id );
                                break;
                            }
                        default:
                            {
                                pageTitle = "New Communication";
                                break;
                            }
                    }
                }
            }

            breadCrumbs.Add( new BreadCrumb( pageTitle, pageReference ) );
            RockPage.Title = pageTitle;

            return breadCrumbs;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbTab control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbTab_Click( object sender, EventArgs e )
        {
            LinkButton lb = sender as LinkButton;

            if ( lb != null )
            {
                if ( lb == lnkTabActivity )
                {
                    _ActivePanel = CommunicationDetailPanels.Activity;
                }
                else if ( lb == lnkTabAnalytics )
                {
                    _ActivePanel = CommunicationDetailPanels.Analytics;
                }
                if ( lb == lnkTabMessageDetails )
                {
                    _ActivePanel = CommunicationDetailPanels.MessageDetails;
                }
                if ( lb == lnkTabRecipientDetails )
                {
                    _ActivePanel = CommunicationDetailPanels.RecipientDetails;
                }

                InitializeActiveCommunication();

                ShowDetailTab();
            }
        }

        #region Message Panel Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var dataContext = this.GetDataContext();

            var service = new CommunicationService( dataContext );
            var communication = service.Get( CommunicationId.Value );
            if ( communication != null &&
                communication.Status == CommunicationStatus.PendingApproval &&
                IsUserAuthorized( "Approve" ) )
            {
                // Redirect back to same page without the edit param
                var pageRef = CurrentPageReference;
                pageRef.Parameters.Add( "edit", "true" );
                Response.Redirect( pageRef.BuildUrl() );
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        /// <summary>
        /// Handles the Click event of the btnApprove control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnApprove_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid && CommunicationId.HasValue )
            {
                var dataContext = this.GetDataContext();

                var service = new CommunicationService( dataContext );
                var communication = service.Get( CommunicationId.Value );
                if ( communication != null )
                {
                    if ( communication.Status == CommunicationStatus.PendingApproval )
                    {
                        var prevStatus = communication.Status;
                        if ( IsUserAuthorized( "Approve" ) )
                        {
                            communication.Status = CommunicationStatus.Approved;
                            communication.ReviewedDateTime = RockDateTime.Now;
                            communication.ReviewerPersonAliasId = CurrentPersonAliasId;

                            dataContext.SaveChanges();

                            // TODO: Send notice to sender that communication was approved

                            ShowResult( "The communication has been approved", communication, NotificationBoxType.Success );
                        }
                        else
                        {
                            ShowResult( "Sorry, you are not authorized to approve this communication!", communication, NotificationBoxType.Danger );
                        }
                    }
                    else
                    {
                        ShowResult( string.Format( "This communication is already {0}!", communication.Status.ConvertToString() ),
                            communication, NotificationBoxType.Warning );
                    }
                }

            }
        }

        /// <summary>
        /// Handles the Click event of the btnDeny control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDeny_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid && CommunicationId.HasValue )
            {
                var dataContext = this.GetDataContext();

                var service = new CommunicationService( dataContext );
                var communication = service.Get( CommunicationId.Value );
                if ( communication != null )
                {
                    if ( communication.Status == CommunicationStatus.PendingApproval )
                    {
                        if ( IsUserAuthorized( "Approve" ) )
                        {
                            communication.Status = CommunicationStatus.Denied;
                            communication.ReviewedDateTime = RockDateTime.Now;
                            communication.ReviewerPersonAliasId = CurrentPersonAliasId;

                            dataContext.SaveChanges();

                            // TODO: Send notice to sender that communication was denied

                            ShowResult( "The communication has been denied", communication, NotificationBoxType.Warning );
                        }
                        else
                        {
                            ShowResult( "Sorry, you are not authorized to approve or deny this communication!", communication, NotificationBoxType.Danger );
                        }
                    }
                    else
                    {
                        ShowResult( string.Format( "This communication is already {0}!", communication.Status.ConvertToString() ),
                            communication, NotificationBoxType.Warning );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid && CommunicationId.HasValue )
            {
                var dataContext = this.GetDataContext();

                var service = new CommunicationService( dataContext );
                var communication = service.Get( CommunicationId.Value );
                if ( communication != null )
                {
                    if ( communication.Status == CommunicationStatus.Approved || communication.Status == CommunicationStatus.PendingApproval )
                    {
                        if ( !communication.Recipients
                            .Where( r => r.Status == CommunicationRecipientStatus.Delivered )
                            .Any() )
                        {
                            communication.Status = CommunicationStatus.Draft;
                            dataContext.SaveChanges();

                            ShowResult( "This communication has successfully been cancelled without any recipients receiving communication!", communication, NotificationBoxType.Success );
                        }
                        else
                        {
                            communication.Recipients
                                .Where( r => r.Status == CommunicationRecipientStatus.Pending )
                                .ToList()
                                .ForEach( r => r.Status = CommunicationRecipientStatus.Cancelled );
                            dataContext.SaveChanges();

                            int delivered = communication.Recipients.Count( r => r.Status == CommunicationRecipientStatus.Delivered );
                            ShowResult( string.Format( "This communication has been cancelled, however the communication was delivered to {0} recipients!", delivered )
                                , communication, NotificationBoxType.Warning );
                        }
                    }
                    else
                    {
                        ShowResult( "This communication has already been cancelled!", communication, NotificationBoxType.Warning );
                    }
                }
            }

        }

        /// <summary>
        /// Handles the Click event of the btnCopy control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCopy_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid && CommunicationId.HasValue )
            {
                var dataContext = this.GetDataContext();

                var service = new CommunicationService( dataContext );

                var newCommunication = service.Copy( CommunicationId.Value, CurrentPersonAliasId );

                if ( newCommunication != null )
                {
                    service.Add( newCommunication );
                    dataContext.SaveChanges();

                    // Redirect to new communication
                    if ( CurrentPageReference.Parameters.ContainsKey( PageParameterKey.CommunicationId ) )
                    {
                        CurrentPageReference.Parameters[PageParameterKey.CommunicationId] = newCommunication.Id.ToString();
                    }
                    else
                    {
                        CurrentPageReference.Parameters.Add( PageParameterKey.CommunicationId, newCommunication.Id.ToString() );
                    }

                    Response.Redirect( CurrentPageReference.BuildUrl(), false );
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnTemplate_Click( object sender, EventArgs e )
        {
            if ( !CommunicationId.HasValue )
                return;

            using ( var dataContext = new RockContext() )
            {
                var communication = new CommunicationService( dataContext ).Get( CommunicationId.Value );
                if ( communication == null )
                    return;

                tbTemplateName.Text = communication.Name;
                cpTemplateCategory.SetValue( communication.CommunicationTemplate != null
                    ? communication.CommunicationTemplate.Category
                    : null );
                tbTemplateDescription.Text = string.Empty;

                ShowDialog( "Template" );
            }
        }

        /// <summary>
        /// Handles the Click event of the mdSaveTemplate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdSaveTemplate_Click( object sender, EventArgs e )
        {
            if ( !Page.IsValid || !CommunicationId.HasValue )
                return;

            using ( var dataContext = new RockContext() )
            {
                var communication = new CommunicationService( dataContext ).Get( CommunicationId.Value );
                if ( communication == null )
                    return;

                int? categoryId = cpTemplateCategory.SelectedValue.AsIntegerOrNull();

                categoryId = ( categoryId == 0 ) ? null : categoryId;

                var template = new CommunicationTemplate
                {
                    SenderPersonAliasId = CurrentPersonAliasId,
                    Name = tbTemplateName.Text,
                    CategoryId = categoryId,
                    Description = tbTemplateDescription.Text,
                    Subject = communication.Subject,
                    FromName = communication.FromName,
                    FromEmail = communication.FromEmail,
                    ReplyToEmail = communication.ReplyToEmail,
                    CCEmails = communication.CCEmails,
                    BCCEmails = communication.BCCEmails,
                    Message = "{% raw %}" + communication.Message + "{% endraw %}",
                    MessageMetaData = communication.MessageMetaData,
                    SmsFromSystemPhoneNumberId = communication.SmsFromSystemPhoneNumberId,
                    SMSMessage = communication.SMSMessage,
                    PushTitle = communication.PushTitle,
                    PushMessage = communication.PushMessage,
                    PushSound = communication.PushSound
                };

                foreach ( var attachment in communication.Attachments.ToList() )
                {
                    var newAttachment = new CommunicationTemplateAttachment
                    {
                        BinaryFileId = attachment.BinaryFileId,
                        CommunicationType = attachment.CommunicationType
                    };
                    template.Attachments.Add( newAttachment );
                }

                var templateService = new CommunicationTemplateService( dataContext );
                templateService.Add( template );
                dataContext.SaveChanges();

                template = templateService.Get( template.Id );
                if ( template != null )
                {
                    template.MakePrivate( Authorization.VIEW, CurrentPerson, dataContext );
                    template.MakePrivate( Authorization.EDIT, CurrentPerson, dataContext );
                    template.MakePrivate( Authorization.ADMINISTRATE, CurrentPerson, dataContext );

                    var groupService = new GroupService( dataContext );
                    var communicationAdministrators = groupService.Get( Rock.SystemGuid.Group.GROUP_COMMUNICATION_ADMINISTRATORS.AsGuid() );
                    if ( communicationAdministrators != null )
                    {
                        template.AllowSecurityRole( Authorization.VIEW, communicationAdministrators, dataContext );
                        template.AllowSecurityRole( Authorization.EDIT, communicationAdministrators, dataContext );
                        template.AllowSecurityRole( Authorization.ADMINISTRATE, communicationAdministrators, dataContext );
                    }

                    var rockAdministrators = groupService.Get( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() );
                    if ( rockAdministrators != null )
                    {
                        template.AllowSecurityRole( Authorization.VIEW, rockAdministrators, dataContext );
                        template.AllowSecurityRole( Authorization.EDIT, rockAdministrators, dataContext );
                        template.AllowSecurityRole( Authorization.ADMINISTRATE, rockAdministrators, dataContext );
                    }

                }

                nbTemplateCreated.Visible = true;
            }

            HideDialog();
        }

        #endregion

        #region Interactions Grid Events

        /// <summary>
        /// Handles the GridRebind event of the gInteractions grid controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void gInteractions_GridRebind( object sender, EventArgs e )
        {
            BindInteractions();
        }

        /// <summary>
        /// Handles the RowDataBound event of the Interactions grid controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gInteractions_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            Interaction interaction = e.Row.DataItem as Interaction;
            if ( interaction != null )
            {
                Literal lActivityDetails = e.Row.FindControl( "lActivityDetails" ) as Literal;
                if ( lActivityDetails != null )
                {
                    lActivityDetails.Text = CommunicationRecipient.GetInteractionDetails( interaction );
                }
            }
        }

        #endregion

        #region Analytics Tab Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            // reload page if block settings where changed
            this.NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptMostPopularLinks control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs" /> instance containing the event data.</param>
        protected void rptMostPopularLinks_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            TopLinksInfo topLinksInfo = e.Item.DataItem as TopLinksInfo;
            if ( topLinksInfo != null )
            {
                Literal lUrl = e.Item.FindControl( "lUrl" ) as Literal;
                lUrl.Text = topLinksInfo.Url;

                Literal lUrlProgressHTML = e.Item.FindControl( "lUrlProgressHTML" ) as Literal;
                lUrlProgressHTML.Text = string.Format(
                    @"<div class='progress margin-b-none'>
                        <div class='progress-bar progress-bar-link' role='progressbar' aria-valuenow='{0}' aria-valuemin='0' aria-valuemax='100' style='width: {0}%'>
                            <span class='sr-only'>{0}%</span>
                        </div>
                    </div>",
                    Math.Round( topLinksInfo.PercentOfTop, 2 ) );

                Literal lUniquesCount = e.Item.FindControl( "lUniquesCount" ) as Literal;
                lUniquesCount.Text = topLinksInfo.UniquesCount.ToString();

                Literal lCTRPercent = e.Item.FindControl( "lCTRPercent" ) as Literal;
                HtmlGenericControl pnlCTRData = e.Item.FindControl( "pnlCTRData" ) as HtmlGenericControl;
                pnlCTRData.Visible = topLinksInfo.CTRPercent.HasValue;
                if ( topLinksInfo.CTRPercent.HasValue )
                {
                    lCTRPercent.Text = Math.Round( topLinksInfo.CTRPercent.Value, 2 ) + "%";
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptClientApplicationUsage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptClientApplicationUsage_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            ApplicationUsageInfo applicationUsageInfo = e.Item.DataItem as ApplicationUsageInfo;
            if ( applicationUsageInfo != null )
            {
                var lApplicationName = e.Item.FindControl( "lApplicationName" ) as Literal;
                var lUsagePercent = e.Item.FindControl( "lUsagePercent" ) as Literal;
                lApplicationName.Text = applicationUsageInfo.Application ?? "Unknown";
                lUsagePercent.Text = Math.Round( applicationUsageInfo.UsagePercent, 2 ).ToString() + "%";
            }
        }

        #endregion

        #region Recipients Tab Events

        /// <summary>
        /// Handles the Click event of the btnUpdateRecipientsList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUpdateRecipientsList_Click( object sender, EventArgs e )
        {
            this.SetActivePanel( CommunicationDetailPanels.RecipientDetails );

            SaveRecipientListPreferences();

            BindRecipientsGrid();
        }

        #endregion

        #region Recipients Grid Events

        private bool _GridIsExporting = false;
        private bool _GridIsCommunication = false;

        /// <summary>
        /// Handles the GridRebind event of the gRecipients grid controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void gRecipients_GridRebind( object sender, GridRebindEventArgs e )
        {
            _GridIsExporting = e.IsExporting;
            _GridIsCommunication = e.IsCommunication;
            BindRecipientsGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the Recipients grid controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gRecipients_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType != DataControlRowType.DataRow )
            {
                return;
            }

            var listItem = e.Row.DataItem as DataRowView;

            if ( listItem == null )
            {
                return;
            }

            if ( _GridIsExporting || _GridIsCommunication )
            {
                return;
            }

            if ( !listItem[PersonPropertyColumn.IsActive].ToStringSafe().AsBoolean() )
            {
                e.Row.AddCssClass( "inactive" );
            }

            if ( listItem[PersonPropertyColumn.IsDeceased].ToStringSafe().AsBoolean() )
            {
                e.Row.AddCssClass( "deceased" );
            }
        }

        #endregion

        #region Recipient Grid Filter Events

        /// <summary>
        /// Keys to use for Filter Settings
        /// </summary>
        private static class FilterSettingName
        {
            public const string FirstName = "First Name";
            public const string LastName = "Last Name";
            public const string OpenedStatus = "Opened Status";
            public const string ClickedStatus = "Clicked Status";
            public const string DeliveryStatus = "Delivery Status";
            public const string DeliveryStatusNote = "Delivery Status Note";
            public const string CommunicationMedium = "Communication Medium";
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            SaveRecipientsFilterSettings();

            BindRecipientsGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void rFilter_ClearFilterClick( object sender, EventArgs e )
        {
            rFilter.DeleteFilterPreferences();

            BindRecipientsFilter();

            BindRecipientsGrid();
        }

        /// <summary>
        /// ts the filter display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            e.Value = GetRecipientsFilterValueDescription( e.Key );
        }

        /// <summary>
        /// Binds data to the filter controls.
        /// </summary>
        private void BindRecipientsFilter()
        {
            // Get the key/value map with the current values.
            var settings = GetRecipientsFilterSettings();

            if ( settings == null )
            {
                return;
            }

            // Overwrite the map with the settings stored in the user preferences.
            foreach ( var key in settings.Keys.ToList() )
            {
                settings[key] = rFilter.GetFilterPreference( key );
            }

            // Apply the map to update the filter controls.
            ApplyRecipientsFilterSettings( settings );
        }

        /// <summary>
        /// Save the current filter settings.
        /// </summary>
        private void SaveRecipientsFilterSettings()
        {
            var settings = GetRecipientsFilterSettings();

            if ( settings == null )
            {
                return;
            }

            foreach ( var kvp in settings )
            {
                rFilter.SetFilterPreference( kvp.Key, kvp.Value );
            }
        }

        /// <summary>
        /// Apply the filter settings to the filter controls.
        /// </summary>
        /// <param name="settingsKeyValueMap"></param>
        private void ApplyRecipientsFilterSettings( Dictionary<string, string> settingsKeyValueMap )
        {
            txbFirstNameFilter.Text = settingsKeyValueMap[FilterSettingName.FirstName];
            txbLastNameFilter.Text = settingsKeyValueMap[FilterSettingName.LastName];

            cblMedium.SetValues( settingsKeyValueMap[FilterSettingName.CommunicationMedium].SplitDelimitedValues( "," ) );
            cblOpenedStatus.SetValues( settingsKeyValueMap[FilterSettingName.OpenedStatus].SplitDelimitedValues( "," ) );
            cblClickedStatus.SetValues( settingsKeyValueMap[FilterSettingName.ClickedStatus].SplitDelimitedValues( "," ) );
            cblDeliveryStatus.SetValues( settingsKeyValueMap[FilterSettingName.DeliveryStatus].SplitDelimitedValues( "," ) );

            txbDeliveryStatusNote.Text = settingsKeyValueMap[FilterSettingName.DeliveryStatusNote];
        }

        /// <summary>
        /// Get a key/value map of current filter settings to be saved.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetRecipientsFilterSettings()
        {
            var settings = new Dictionary<string, string>();

            settings[FilterSettingName.FirstName] = txbFirstNameFilter.Text;
            settings[FilterSettingName.LastName] = txbLastNameFilter.Text;

            settings[FilterSettingName.CommunicationMedium] = cblMedium.SelectedValues.AsDelimited( "," );
            settings[FilterSettingName.OpenedStatus] = cblOpenedStatus.SelectedValues.AsDelimited( "," );
            settings[FilterSettingName.ClickedStatus] = cblClickedStatus.SelectedValues.AsDelimited( "," );
            settings[FilterSettingName.DeliveryStatus] = cblDeliveryStatus.SelectedValues.AsDelimited( "," );

            settings[FilterSettingName.DeliveryStatusNote] = txbDeliveryStatusNote.Text;

            return settings;
        }

        /// <summary>
        /// Gets the user-friendly description for a filter field setting.
        /// </summary>
        /// <param name="filterSettingName"></param>
        /// <returns></returns>
        private string GetRecipientsFilterValueDescription( string filterSettingName )
        {
            if ( filterSettingName == FilterSettingName.FirstName )
            {
                return string.Format( "Starts With \"{0}\"", txbFirstNameFilter.Text );
            }
            else if ( filterSettingName == FilterSettingName.LastName )
            {
                return string.Format( "Starts With \"{0}\"", txbLastNameFilter.Text );
            }
            else if ( filterSettingName == FilterSettingName.CommunicationMedium )
            {
                return cblMedium.SelectedNames.AsDelimited( ", " );
            }
            else if ( filterSettingName == FilterSettingName.OpenedStatus )
            {
                return cblOpenedStatus.SelectedNames.AsDelimited( ", " );
            }
            else if ( filterSettingName == FilterSettingName.ClickedStatus )
            {
                return cblClickedStatus.SelectedNames.AsDelimited( ", " );
            }
            else if ( filterSettingName == FilterSettingName.DeliveryStatus )
            {
                return cblDeliveryStatus.SelectedNames.AsDelimited( ", " );
            }
            else if ( filterSettingName == FilterSettingName.DeliveryStatusNote )
            {
                return string.Format( "Contains \"{0}\"", txbDeliveryStatusNote.Text );
            }

            return string.Empty;
        }

        #endregion

        #endregion

        #region Modal Dialogs

        /// <summary>
        /// Show the specified modal dialog.
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="setValues"></param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Show the current active modal dialog.
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="setValues"></param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "TEMPLATE":
                    mdCreateTemplate.Show();
                    break;
            }

            upDialog.Update();
        }

        /// <summary>
        /// Hide the current active modal dialog.
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="setValues"></param>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {

                case "TEMPLATE":
                    mdCreateTemplate.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
            upDialog.Update();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Retrieve a singleton data context for data operations in this block.
        /// </summary>
        /// <returns></returns>
        private RockContext GetDataContext()
        {
            if ( _DataContext == null )
            {
                _DataContext = new RockContext();
            }

            return _DataContext;
        }

        /// <summary>
        /// Initialize the filter for the Recipients grid.
        /// </summary>
        private void InitializeRecipientsFilter()
        {
            // Hook up the filter event handlers.
            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;
            rFilter.ClearFilterClick += rFilter_ClearFilterClick;

            // If this is a full page load, initialize the filter control and load the filter values.
            if ( !Page.IsPostBack )
            {
                BindRecipientsFilter();
            }
        }

        /// <summary>
        /// Get page configuration settings from the query string.
        /// </summary>
        private void InitializePageFromParameters()
        {
            // Set the tab for the corresponding view.
            var page = PageParameter( "view" ).ToLower();

            if ( page == "activity" )
            {
                _ActivePanel = CommunicationDetailPanels.Activity;
            }
            else if ( page == "message" || page == "details" )
            {
                _ActivePanel = CommunicationDetailPanels.MessageDetails;
            }
            else if ( page == "recipients" )
            {
                _ActivePanel = CommunicationDetailPanels.RecipientDetails;
            }
            else
            {
                _ActivePanel = CommunicationDetailPanels.Analytics;
            }
        }

        /// <summary>
        /// Create a set of page query parameters to represent the current Page settings.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetQueryParamsFromSettings()
        {
            var queryParams = new Dictionary<string, string>();

            if ( _ActivePanel != CommunicationDetailPanels.Analytics )
            {
                queryParams.Add( "view", _ActivePanel.ToString() );
            }

            return queryParams;
        }

        /// <summary>
        /// Initialize controls on the Analytics Pane.
        /// </summary>
        private void InitializeAnalyticsPanelControls()
        {
            // Initialize Chart Data properties to empty, to prevent syntax errors in the page script.
            LineChartDataLabelsJSON = "[]";
            LineChartDataOpensJSON = "[]";
            LineChartDataClicksJSON = "[]";
            LineChartDataUnOpenedJSON = "[]";
            PieChartDataOpenClicksJSON = "[]";
            PieChartDataClientLabelsJSON = "[]";
            PieChartDataClientCountsJSON = "[]";
            SeriesColorsJSON = "[]";

            // Add handlers for postback events.
            pnlPendingSummary.Attributes["onclick"] = Page.ClientScript.GetPostBackEventReference( pnlPendingSummary, "ShowPendingRecipients" );
            pnlDeliveredSummary.Attributes["onclick"] = Page.ClientScript.GetPostBackEventReference( pnlDeliveredSummary, "ShowDeliveredRecipients" );
            pnlCancelledSummary.Attributes["onclick"] = Page.ClientScript.GetPostBackEventReference( pnlCancelledSummary, "ShowCancelledRecipients" );
            pnlFailedSummary.Attributes["onclick"] = Page.ClientScript.GetPostBackEventReference( pnlFailedSummary, "ShowFailedRecipients" );
        }

        /// <summary>
        /// Set the properties of the Recipients list control.
        /// </summary>
        private void InitializeRecipientsList()
        {
            gRecipients.DataKeyNames = new string[] { "Id" };
            gRecipients.GridRebind += gRecipients_GridRebind;

            gRecipients.RowClickEnabled = false;

            gRecipients.RowDataBound += gRecipients_RowDataBound;
            gRecipients.AllowCustomPaging = true;

            SetDefaultGridPageSize( gRecipients );
        }

        /// <summary>
        /// Set the properties of the Activities list control.
        /// </summary>
        private void InitializeInteractionsList()
        {
            gInteractions.DataKeyNames = new string[] { "Id" };
            gInteractions.GridRebind += gInteractions_GridRebind;

            SetDefaultGridPageSize( gInteractions );
        }

        /// <summary>
        /// Set the default page size for the specified grid.
        /// </summary>
        /// <param name="grid"></param>
        private void SetDefaultGridPageSize( Grid grid )
        {
            // Ensure that the page size is not set to maximum on an initial page load, because it may result in a timeout.
            // This can be confusing or difficult to fix if the user is not aware of the persisted page size settings.
            if ( !this.IsPostBack )
            {
                if ( grid.PageSize > 500 )
                {
                    grid.PageSize = 500;
                }
            }
        }

        /// <summary>
        /// Initialize the scripts required for Chart.js
        /// </summary>
        private void InitializeChartScripts()
        {
            // NOTE: moment.js needs to be loaded before chartjs
            RockPage.AddScriptLink( "~/Scripts/moment.min.js", true );
            RockPage.AddScriptLink( "~/Scripts/Chartjs/Chart.js", true );
        }

        /// <summary>
        /// Load the list of Person properties that are available to show in the Recipients List.
        /// </summary>
        private void PopulatePersonPropertiesSelectionItems()
        {
            var availableColumns = this.GetReportColumns();

            var columns = availableColumns.Where( x => x.ContentType == PersonDataSourceColumnSourceSpecifier.Property
                                                  || x.ContentType == PersonDataSourceColumnSourceSpecifier.Calculated )
                                          .OrderBy( x => x.Name )
                                          .ToList();

            cblProperties.Items.Clear();

            foreach ( var column in columns )
            {
                cblProperties.Items.Add( new ListItem { Text = column.Name, Value = column.Key } );
            }
        }

        /// <summary>
        /// Load the list of Person Attributes that are available to show in the Recipients List.
        /// </summary>
        private void PopulatePersonAttributesSelectionItems()
        {
            var availableColumns = this.GetReportColumns();

            var columns = availableColumns.Where( x => x.ContentType == PersonDataSourceColumnSourceSpecifier.Attribute )
                                          .OrderBy( x => x.Name )
                                          .ToList();

            lbAttributes.Items.Clear();

            foreach ( var column in columns )
            {
                lbAttributes.Items.Add( new ListItem( column.Name, column.Key ) );
            }
        }

        /// <summary>
        /// Show the block detail.
        /// </summary>
        private void ShowDetail()
        {
            ShowStatus( _Communication );

            lTitle.Text = ( string.IsNullOrEmpty( _Communication.Name ) ? ( string.IsNullOrEmpty( _Communication.Subject ) ? _Communication.PushTitle : _Communication.Subject ) : _Communication.Name ).FormatAsHtmlTitle();
            pdAuditDetails.SetEntity( _Communication, ResolveRockUrl( "~" ) );

            ShowDetailTab();
        }

        /// <summary>
        /// Shows the content for the active tab.
        /// </summary>
        private void ShowDetailTab()
        {
            if ( _ActivePanel == CommunicationDetailPanels.Analytics )
            {
                InitializeChartScripts();
                ShowAnalyticsPanel();
            }
            else if ( _ActivePanel == CommunicationDetailPanels.MessageDetails )
            {
                ShowMessageDetails( _Communication );
                ShowMessageActions( _Communication );
            }
            else if ( _ActivePanel == CommunicationDetailPanels.Activity )
            {
                BindInteractions();
            }
            else if ( _ActivePanel == CommunicationDetailPanels.RecipientDetails )
            {
                PopulatePersonPropertiesSelectionItems();
                PopulatePersonAttributesSelectionItems();
                PopulateRecipientFilterSelectionLists();

                LoadRecipientListPreferences();
                BindRecipientsGrid();
            }

            SetActivePanel( _ActivePanel );
        }

        /// <summary>
        /// Show the message panel.
        /// </summary>
        /// <param name="communication"></param>
        private void ShowMessageDetails( Rock.Model.Communication communication )
        {
            SetCommunicationAuditDisplayControlValue( lCreatedBy, communication.CreatedByPersonAlias, communication.CreatedDateTime, "Created By" );
            SetCommunicationAuditDisplayControlValue( lApprovedBy, communication.ReviewerPersonAlias, communication.ReviewedDateTime, "Approved By" );

            if ( communication.FutureSendDateTime.HasValue && communication.FutureSendDateTime.Value > RockDateTime.Now )
            {
                lFutureSend.Text = String.Format( "<div class='alert alert-success'><strong>Future Send</strong> This communication is scheduled to be sent {0} <small>({1})</small>.</div>", communication.FutureSendDateTime.Value.ToRelativeDateString(), communication.FutureSendDateTime.Value.ToString() );
            }

            var details = GetMediumData( communication );

            if ( string.IsNullOrWhiteSpace( details ) )
            {
                details = "<div class='alert alert-warning'>No message details are available for this communication</div>";
            }

            if ( communication.UrlReferrer.IsNotNullOrWhiteSpace() )
            {
                details += string.Format( "<small>Originated from <a href='{0}'>this page</a></small>", communication.UrlReferrer );
            }

            lDetails.Text = details;
        }

        /// <summary>
        /// Sets the value of a control that displays audit information for a communication.
        /// </summary>
        /// <param name="literal"></param>
        /// <param name="personAlias"></param>
        /// <param name="datetime"></param>
        /// <param name="labelText"></param>
        private void SetCommunicationAuditDisplayControlValue( Literal literal, PersonAlias personAlias, DateTime? datetime, string labelText )
        {
            if ( personAlias != null )
            {
                SetPersonDateValue( literal, personAlias.Person, datetime, labelText );
            }
        }

        /// <summary>
        /// Sets the value of a control that displays audit information for a communication.
        /// </summary>
        /// <param name="literal"></param>
        /// <param name="person"></param>
        /// <param name="datetime"></param>
        /// <param name="labelText"></param>
        private void SetPersonDateValue( Literal literal, Person person, DateTime? datetime, string labelText )
        {
            if ( person != null )
            {
                literal.Text = String.Format( "<dt>{0}</dt><dd>{1}", labelText, person.FullName );

                if ( datetime.HasValue )
                {
                    literal.Text += String.Format( " <small class='js-date-rollover' data-toggle='tooltip' data-placement='top' title='{0}'>({1})</small>", datetime.Value.ToString(), datetime.Value.ToRelativeDateString() );
                }

                literal.Text += "</dd>";
            }
        }

        /// <summary>
        /// Build a Report template that contains the columns selected for the Recipients List grid.
        /// </summary>
        /// <returns></returns>
        private Report BuildRecipientsListReportTemplate()
        {
            var availableColumns = this.GetReportColumns();

            var reportBuilder = new ReportTemplateBuilder( typeof( Rock.Model.Person ) );

            // Add default fields.
            if ( !_GridIsExporting )
            {
                var isDeceasedField = reportBuilder.AddPropertyField( "IsDeceased" );

                isDeceasedField.ShowInGrid = false;
            }

            dynamic settings = new { ShowAsLink = !_GridIsExporting, DisplayOrder = 0 };

            reportBuilder.AddDataSelectField( "Rock.Reporting.DataSelect.Person.PersonLinkSelect", settings, "Name" );

            // Add user-selected Properties.
            var selectedProperties = cblProperties.SelectedValues;

            foreach ( var propertyName in selectedProperties )
            {
                var columnInfo = availableColumns.FirstOrDefault( x => x.Key == propertyName );

                if ( columnInfo == null )
                {
                    continue;
                }

                if ( columnInfo.ContentType == PersonDataSourceColumnSourceSpecifier.Property )
                {
                    reportBuilder.AddPropertyField( propertyName );
                }
                else if ( columnInfo.ContentType == PersonDataSourceColumnSourceSpecifier.Calculated )
                {
                    reportBuilder.AddDataSelectField( columnInfo.ColumnSourceIdentifier, columnInfo.Settings, columnInfo.Name );
                }
                else if ( columnInfo.ContentType == PersonDataSourceColumnSourceSpecifier.Attribute )
                {
                    reportBuilder.AddDataSelectField( columnInfo.ColumnSourceIdentifier, columnInfo.Settings, columnInfo.Name );
                }
            }

            // Add user-selected Attributes.
            var selectedAttributes = lbAttributes.SelectedValues;

            foreach ( var attributeGuid in selectedAttributes )
            {
                var columnInfo = availableColumns.FirstOrDefault( x => x.Key == attributeGuid );

                if ( columnInfo == null )
                {
                    continue;
                }

                reportBuilder.AddAttributeField( attributeGuid, columnInfo.Name );
            }

            var report = reportBuilder.Report;

            // Set the sort column.
            DataControlField gridSortColumn = GetGridSortColumn( gRecipients );

            if ( gridSortColumn != null )
            {
                var sortField = report.ReportFields.FirstOrDefault( x => x.ColumnHeaderText == gridSortColumn.HeaderText );

                if ( sortField != null )
                {
                    sortField.SortOrder = 1;
                    sortField.SortDirection = gRecipients.SortProperty.Direction;
                }
            }

            return report;
        }

        /// <summary>
        /// Returns the first column of the grid corresponding to the current sort settings.
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        private DataControlField GetGridSortColumn( Grid grid )
        {
            if ( grid.SortProperty != null )
            {
                var sortProperty = grid.SortProperty.Property;

                foreach ( DataControlField field in grid.Columns )
                {
                    if ( field.SortExpression == sortProperty )
                    {
                        return field;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Create the data source for the Recipients List and bind it to the grid.
        /// </summary>
        /// <remarks>
        /// Given that the Recipients list can include user-selected columns representing either Person Properties or Attributes,
        /// the strategy adopted here is to use a Person Report to generate as much of the content as possible.
        /// After generating the report output, we add some custom fields specifically related to the person as a Communication Recipient.
        /// </remarks>
        private void BindRecipientsGrid()
        {
            if ( !CommunicationId.HasValue )
            {
                return;
            }

            Report report = null;
            ReportOutputBuilder.TabularReportOutputResult result = null;

            var dataContext = new RockContext();

            try
            {
                // Construct a filter expression to select people who are recipients of the communication
                // and match the selected grid filter options for delivery status and interactions.
                var personService = new PersonService( dataContext );

                var parameterExpression = personService.ParameterExpression;

                var whereExpression = GetRecipientFilterExpression( dataContext, personService, parameterExpression );

                // Create a Person Report template containing the user-selected Properties and Attributes.
                report = BuildRecipientsListReportTemplate();

                // Build the output data for the Report by combining the report template with the filter.
                var builder = new ReportOutputBuilder( report, dataContext );

                ReportOutputBuilder.ReportOutputBuilderFieldContentSpecifier contentType = ReportOutputBuilder.ReportOutputBuilderFieldContentSpecifier.RawValue;

                int? pageSize = null;
                int? pageIndex = null;

                if ( _GridIsExporting )
                {
                    contentType = ReportOutputBuilder.ReportOutputBuilderFieldContentSpecifier.FormattedText;
                }
                else if ( !_GridIsCommunication )
                {

                    /* 27-May-2020 - SK
                     * Not allow paging if grid is binded for exporting OR Communication.
                     */

                    // Only retrieve data for the current grid page.
                    pageSize = gRecipients.PageSize;
                    pageIndex = gRecipients.PageIndex;
                }

                result = builder.GetReportData( this.CurrentPerson,
                    whereExpression,
                    parameterExpression,
                    dataContext,
                    contentType,
                    pageIndex,
                    pageSize );

                AddStandardRecipientFieldsToDataSource( dataContext, result.Data, builder );

                // Add report columns to the grid.
                bool preserveExistingColumns = !this.IsPostBack;

                builder.ConfigureReportOutputGrid( gRecipients,
                    this.CurrentPerson,
                    false,
                    dataContext,
                    result.ReportFieldToDataColumnMap,
                    preserveExistingColumns,
                    addSelectionColumn: true );

                // Add communication-specific columns to the grid.
                AddStandardRecipientColumns();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                nbAnalyticsNotAvailable.Visible = true;
                nbAnalyticsNotAvailable.Text = "An unexpected error occurred while building the Recipients List.";
                return;
            }

            // If the grid is sorted by a communication-specific column, apply the sort now.
            var dataView = result.Data.AsDataView();

            try
            {
                var gridSortColumn = GetGridSortColumn( gRecipients );

                if ( gridSortColumn != null )
                {
                    var reportColumn = report.ReportFields.FirstOrDefault( x => x.ColumnHeaderText == gridSortColumn.HeaderText );

                    if ( reportColumn == null )
                    {
                        try
                        {
                            var sortProperty = gRecipients.SortProperty;

                            dataView.Sort = sortProperty.Property + ( sortProperty.Direction == SortDirection.Descending ? " DESC" : string.Empty );
                        }
                        catch ( ArgumentException )
                        {
                            // If the sort property is invalid, return the unsorted list.
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                // If the sort fails, log the error and continue.
                ExceptionLogService.LogException( ex );
            }

            // Show the data set in the grid.
            gRecipients.VirtualItemCount = result.ReportRowCount.GetValueOrDefault();

            gRecipients.DataSource = dataView;

            gRecipients.DataBind();
        }

        /// <summary>
        /// Create the data source for the Recipients List and bind it to the grid.
        /// </summary>
        /// <remarks>
        private void BindInteractions()
        {
            if ( CommunicationId.HasValue )
            {
                var dataContext = this.GetDataContext();

                var interactionChannelGuid = Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid();

                var interactions = new InteractionService( dataContext )
                    .Queryable()
                    .Include( a => a.PersonAlias.Person )
                    .Where( r => r.InteractionComponent.InteractionChannel.Guid == interactionChannelGuid && r.InteractionComponent.EntityId == CommunicationId.Value );

                var sortProperty = gInteractions.SortProperty;
                if ( sortProperty != null )
                {
                    interactions = interactions.Sort( sortProperty );
                }
                else
                {
                    interactions = interactions.OrderBy( a => a.InteractionDateTime );
                }

                gInteractions.SetLinqDataSource( interactions );
                gInteractions.DataBind();
            }
        }

        /// <summary>
        /// Display the status label for the current Communication.
        /// </summary>
        /// <param name="communication"></param>
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
                        //wpEvents.Expanded = false;
                        //wpEvents.Expanded = true;

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

            if ( communication != null && communication.IsBulkCommunication )
            {
                hlBulk.Visible = true;
            }
            else
            {
                hlBulk.Visible = false;
            }
        }

        /// <summary>
        /// Shows the actions.
        /// </summary>
        /// <param name="communication">The communication.</param>
        private void ShowMessageActions( Rock.Model.Communication communication )
        {
            bool canApprove = IsUserAuthorized( "Approve" );

            // Set default visibility
            btnApprove.Visible = false;
            btnDeny.Visible = false;
            btnEdit.Visible = false;
            btnCancel.Visible = false;
            btnCopy.Visible = false;

            if ( communication != null )
            {
                switch ( communication.Status )
                {
                    case CommunicationStatus.Transient:
                    case CommunicationStatus.Draft:
                    case CommunicationStatus.Denied:
                        {
                            // This block isn't used for transient, draft or denied communications
                            break;
                        }
                    case CommunicationStatus.PendingApproval:
                        {
                            if ( canApprove )
                            {
                                btnApprove.Visible = true;
                                btnDeny.Visible = true;
                                btnEdit.Visible = true;
                            }
                            btnCancel.Visible = communication.IsAuthorized( Rock.Security.Authorization.EDIT, CurrentPerson );
                            break;
                        }
                    case CommunicationStatus.Approved:
                        {
                            // If there are still any pending recipients, allow canceling of send
                            var dataContext = this.GetDataContext();

                            var hasPendingRecipients = new CommunicationRecipientService( dataContext ).Queryable()
                            .Where( r => r.CommunicationId == communication.Id ).Where( r => r.Status == CommunicationRecipientStatus.Pending ).Any();


                            btnCancel.Visible = hasPendingRecipients;

                            // Allow then to create a copy if they have VIEW (don't require full EDIT auth)
                            btnCopy.Visible = communication.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson );
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Shows the result.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="communication">The communication.</param>
        private void ShowResult( string message, Rock.Model.Communication communication, NotificationBoxType notificationType )
        {
            ShowStatus( communication );

            pnlAnalyticsDeliveryStatusSummary.Visible = false;

            nbResult.Text = message;
            nbResult.NotificationBoxType = notificationType;

            if ( CurrentPageReference.Parameters.ContainsKey( PageParameterKey.CommunicationId ) )
            {
                CurrentPageReference.Parameters[PageParameterKey.CommunicationId] = communication.Id.ToString();
            }
            else
            {
                CurrentPageReference.Parameters.Add( PageParameterKey.CommunicationId, communication.Id.ToString() );
            }
            hlViewCommunication.NavigateUrl = CurrentPageReference.BuildUrl();

            pnlResult.Visible = true;

        }

        /// <summary>
        /// Get descriptive information about the current Communication.
        /// </summary>
        /// <param name="communication"></param>
        /// <returns></returns>
        private string GetMediumData( Rock.Model.Communication communication )
        {
            StringBuilder sb = new StringBuilder();
            // If the first tab has been rendered set to true.
            bool firstTabRendered = false;

            // Booleans to prevent having to check conditions twice.
            bool showEmailTab = false;
            bool showSmsTab = false;
            bool showPushTab = false;

            sb.AppendLine( "<ul class='nav nav-pills' role='tablist'>" );

            if ( communication.CommunicationType == CommunicationType.Email || communication.CommunicationType == CommunicationType.RecipientPreference && communication.Message.IsNotNullOrWhiteSpace() )
            {
                firstTabRendered = true;
                showEmailTab = true;
                sb.AppendLine( "<li class='active'><a href='#emailTabContent' role='tab' id='email-tab' data-toggle='tab' aria-controls='email'>Email</a></li>" );
            }

            if ( communication.CommunicationType == CommunicationType.SMS || communication.CommunicationType == CommunicationType.RecipientPreference )
            {
                showSmsTab = true;
                if ( firstTabRendered )
                {
                    sb.AppendLine( "<li>" );
                }
                else
                {
                    firstTabRendered = true;
                    sb.AppendLine( "<li class='active'>" );
                }

                sb.AppendLine( "<a href='#smsTabContent' role='tab' id='sms-tab' data-toggle='tab' aria-controls='sms'>SMS</a></li>" );
            }

            if ( communication.CommunicationType == CommunicationType.PushNotification || communication.CommunicationType == CommunicationType.RecipientPreference && communication.PushMessage.IsNotNullOrWhiteSpace() )
            {
                showPushTab = true;
                if ( firstTabRendered )
                {
                    sb.AppendLine( "<li>" );
                }
                else
                {
                    firstTabRendered = true;
                    sb.AppendLine( "<li class='active'>" );
                }

                sb.AppendLine( "<a href='#pushTabContent' role='tab' id='push-tab' data-toggle='tab' aria-controls='push'>Push</a></li>" );
            }

            sb.AppendLine( "</ul><div><hr/></div>" );


            sb.AppendLine( "<div class='tab-content flex-fill'>" );

            if ( showEmailTab )
            {
                sb.AppendLine( "<div id='emailTabContent' class='tab-pane h-100 d-flex flex-column active'>" );
                sb.AppendLine( "<div class='row'>" );

                AppendStaticControlMediumData( sb, "From",
                string.Format( "{0} ({1})", communication.FromName, communication.FromEmail ) );

                AppendStaticControlMediumData( sb, "Subject", communication.Subject, "col-sm-8" );
                sb.AppendLine( "</div>" );

                sb.AppendLine( "<div class='row'>" );
                AppendStaticControlMediumData( sb, "Reply To", communication.ReplyToEmail );
                AppendStaticControlMediumData( sb, "CC", communication.CCEmails );
                AppendStaticControlMediumData( sb, "BCC", communication.BCCEmails );
                sb.AppendLine( "</div>" );

                var emailAttachments = communication.GetAttachments( CommunicationType.Email );
                if ( emailAttachments.Any() )
                {
                    sb.Append( "<div class='row'><div class='col-md-12'><ul>" );
                    foreach ( var binaryFile in emailAttachments.Select( a => a.BinaryFile ).ToList() )
                    {
                        sb.AppendFormat( "<li><a target='_blank' rel='noopener noreferrer' href='{0}GetFile.ashx?id={1}'>{2}</a></li>",
                            System.Web.VirtualPathUtility.ToAbsolute( "~" ), binaryFile.Id, binaryFile.FileName );
                    }
                    sb.Append( "</ul></div></div>" );
                }

                sb.AppendLine( string.Format( @"
            <div class='bg-gray-100 flex-fill position-relative mb-3 mb-sm-0 styled-scroll border border-panel' style='min-height:400px'>
            <div class='position-absolute w-100 h-100 inset-0 overflow-auto'>
            <iframe id='js-email-body-iframe' class='w-100 bg-white' scrolling='yes' onload='resizeIframe(this)'></iframe>
            </div>
            </div>
            <script id='email-body' type='text/template'>{0}</script>
            <script id='load-email-body' type='text/javascript'>
                var doc = document.getElementById('js-email-body-iframe').contentWindow.document;
                doc.open();
                doc.write('<html><head><title></title></head><body>' +  $('#email-body').html() + '</body></html>');
                doc.close();
            </script>
        ", communication.Message ) );


                sb.AppendLine( "</div>" );

            }

            if ( showSmsTab )
            {
                if ( communication.CommunicationType == CommunicationType.SMS )
                {
                    sb.AppendLine( "<div id='smsTabContent' class='tab-pane active h-100'><div class='row'>" );
                }
                else
                {
                    sb.AppendLine( "<div id='smsTabContent' class='tab-pane h-100'><div class='row'>" );
                }

                if ( communication.SmsFromSystemPhoneNumber != null )
                {
                    AppendStaticControlMediumData( sb, "From", string.Format( "{0} ({1})", communication.SmsFromSystemPhoneNumber.Name, communication.SmsFromSystemPhoneNumber.Number), "col-xs-12" );
                }

                AppendStaticControlMediumData( sb, "Message", communication.SMSMessage, "col-xs-12" );
                sb.AppendLine( "</div></div>" );
            }

            if ( showPushTab )
            {
                if ( communication.CommunicationType == CommunicationType.PushNotification || !showSmsTab )
                {
                    sb.AppendLine( "<div id='pushTabContent' class='tab-pane active h-100'><div class='row'>" );
                }
                else
                {
                    sb.AppendLine( "<div id='pushTabContent' class='tab-pane h-100'><div class='row'>" );
                }

                AppendStaticControlMediumData( sb, "Title", communication.PushTitle, "col-sm-8" );

                if ( communication.PushOpenAction != null )
                {
                    AppendMediumData( sb, "Open Action", Regex.Replace( communication.PushOpenAction.ToStringSafe(), @"(\B[A-Z]+?(?=[A-Z][^A-Z])|\B[A-Z]+?(?=[^A-Z]))", " $1" ) );
                }
                AppendStaticControlMediumData( sb, "Message", communication.PushMessage, "col-sm-12" );


                sb.AppendLine( "</div></div>" );
            }

            sb.AppendLine( "</div>" );

            return sb.ToString();
        }

        /// <summary>
        /// Add a value to the Communication description summary.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void AppendMediumData( StringBuilder sb, string key, string value )
        {
            if ( key.IsNotNullOrWhiteSpace() && value.IsNotNullOrWhiteSpace() )
            {
                sb.AppendFormat( "<dt>{0}</dt><dd>{1}</dd>", key, value );
            }
        }

        /// <summary>
        /// Add a value to the Communication description summary.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void AppendStaticControlMediumData( StringBuilder sb, string key, string value, string colclass = "col-sm-4" )
        {
            if ( key.IsNotNullOrWhiteSpace() && value.IsNotNullOrWhiteSpace() )
            {
                var encodedValue = value.EncodeHtml().ConvertCrLfToHtmlBr();
                sb.AppendFormat( "<div class='{2}'><div class='form-group static-control'><span class='control-label'>{0}</span><div class='control-wrapper'><div class='form-control-static'>{1}</div></div></div></div>", key, encodedValue, colclass );
            }
        }

        /// <summary>
        /// Set the active state of the tabs and associated controls.
        /// </summary>
        /// <param name="panelName"></param>
        private void SetActivePanel( string panelName )
        {
            _ActivePanel = panelName;

            if ( _TabList == null )
            {
                _TabList = new Dictionary<string, Tuple<HtmlGenericControl, Panel>>();

                _TabList.Add( CommunicationDetailPanels.Analytics, new Tuple<HtmlGenericControl, Panel>( tabAnalytics, pnlAnalyticsTab ) );
                _TabList.Add( CommunicationDetailPanels.MessageDetails, new Tuple<HtmlGenericControl, Panel>( tabMessageDetails, pnlMessage ) );
                _TabList.Add( CommunicationDetailPanels.Activity, new Tuple<HtmlGenericControl, Panel>( tabActivity, pnlActivity ) );
                _TabList.Add( CommunicationDetailPanels.RecipientDetails, new Tuple<HtmlGenericControl, Panel>( tabRecipientDetails, pnlRecipients ) );
            }

            foreach ( var tab in _TabList )
            {
                tab.Value.Item1.RemoveCssClass( "active" );
                tab.Value.Item2.Visible = false;
            }

            if ( _TabList.ContainsKey( _ActivePanel ) )
            {
                _TabList[_ActivePanel].Item1.AddCssClass( "active" );
                _TabList[_ActivePanel].Item2.Visible = true;
            }
        }

        private Dictionary<string, Tuple<HtmlGenericControl, Panel>> _TabList = null;

        /// <summary>
        /// Shows the content of the Analytics panel.
        /// </summary>
        private void ShowAnalyticsPanel()
        {
            var dataContext = this.GetDataContext();

            bool communicationTypeHasAnalytics = false;

            // Initialize the Communications data.
            hfCommunicationId.Value = this.PageParameter( PageParameterKey.CommunicationId );

            int? communicationId = hfCommunicationId.Value.AsIntegerOrNull();
            string noDataMessageName = string.Empty;
            var analyticsUnavailableMessage = string.Empty;

            if ( communicationId.HasValue )
            {
                // specific communication specified
                var communication = new CommunicationService( dataContext ).Get( communicationId.Value );

                if ( communication != null )
                {
                    if ( communication.CommunicationType == CommunicationType.Email
                         || communication.CommunicationType == CommunicationType.PushNotification
                         || communication.CommunicationType == CommunicationType.RecipientPreference )
                    {
                        communicationTypeHasAnalytics = true;
                    }
                    else
                    {
                        lTitle.Text = "Email Analytics: " + ( communication.Name ?? communication.Subject );
                        noDataMessageName = communication.Name ?? communication.Subject;
                    }
                }
                else
                {
                    // Invalid Communication specified
                    nbCommunicationorCommunicationListFound.Visible = true;
                    nbCommunicationorCommunicationListFound.Text = "Invalid communication specified";
                }
            }

            if ( !communicationTypeHasAnalytics )
            {
                analyticsUnavailableMessage = "Analytics not available for this communication type.";
            }

            // Show Communication Status Summary
            int? pendingRecipientCount = null;
            int? deliveredRecipientCount = null;
            int? failedRecipientCount = null;
            int? cancelledRecipientCount = null;

            if ( communicationId != null )
            {
                var recipientService = new CommunicationRecipientService( dataContext );
                var sentStatus = new CommunicationRecipientStatus[] { CommunicationRecipientStatus.Opened, CommunicationRecipientStatus.Delivered };

                var recipientSummary = recipientService.Queryable()
                    .Where( a => a.CommunicationId == communicationId )
                    .GroupBy( a => a.Status )
                    .Select( g => new
                    {
                        Status = g.Key,
                        Count = g.Count()
                    } )
                    .ToList();

                pendingRecipientCount = recipientSummary.Where( a => a.Status == CommunicationRecipientStatus.Pending ).Sum( a => a.Count );
                deliveredRecipientCount = recipientSummary.Where( a => sentStatus.Contains( a.Status ) ).Sum( a => a.Count );
                failedRecipientCount = recipientSummary.Where( a => a.Status == CommunicationRecipientStatus.Failed ).Sum( a => a.Count );
                cancelledRecipientCount = recipientSummary.Where( a => a.Status == CommunicationRecipientStatus.Cancelled ).Sum( a => a.Count );
            }

            string actionsStatFormatNumber = "<div>{0:#,##0}</div>";

            lPending.Text = string.Format( actionsStatFormatNumber, pendingRecipientCount );
            lDelivered.Text = string.Format( actionsStatFormatNumber, deliveredRecipientCount );
            lFailed.Text = string.Format( actionsStatFormatNumber, failedRecipientCount );
            lCancelled.Text = string.Format( actionsStatFormatNumber, cancelledRecipientCount );

            // Get the set of Interactions for this communication.
            var interactionsList = this.GetCommunicationInteractionsSummaryInfo( dataContext, communicationId );

            int interactionCount = interactionsList.Count();

            // If there are no interactions, analytics are not available.
            if ( interactionCount == 0 )
            {
                if ( string.IsNullOrEmpty( noDataMessageName ) )
                {
                    analyticsUnavailableMessage = "No activity found for this communication.";
                }
                else
                {
                    analyticsUnavailableMessage = string.Format( "No communications activity for {0}.", noDataMessageName );
                }
            }

            var analyticsIsAvailable = string.IsNullOrEmpty( analyticsUnavailableMessage );

            nbAnalyticsNotAvailable.Text = analyticsUnavailableMessage;
            nbAnalyticsNotAvailable.Visible = !analyticsIsAvailable;

            upAnalytics.Visible = analyticsIsAvailable;

            // If analytics are unavailable, display a message and exit.
            if ( !string.IsNullOrEmpty( analyticsUnavailableMessage ) )
            {
                nbAnalyticsNotAvailable.Text = analyticsUnavailableMessage;
                return;
            }

            this.ShowAnalyticsPanelActionsSummary( dataContext, interactionsList, noDataMessageName, deliveredRecipientCount );

            var interactionsQuery = this.GetCommunicationInteractionsQuery( dataContext, communicationId );

            this.ShowClientsInUseData( interactionsQuery, interactionCount, noDataMessageName );

            this.ShowMostPopularLinksData( interactionsList, deliveredRecipientCount.GetValueOrDefault( 0 ) );

            // Store the list of interactions to use for Recipient filtering.
            _Interactions = interactionsList;
        }

        /// <summary>
        /// Get a query that returns the set of Interactions related to a specific Communication.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="communicationId"></param>
        /// <returns></returns>
        private IQueryable<Interaction> GetCommunicationInteractionsQuery( RockContext dataContext, int? communicationId )
        {
            var channelService = new InteractionChannelService( dataContext );
            var interactionService = new InteractionService( dataContext );

            var interactionChannelCommunication = channelService.Get( Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid() );

            var interactionQuery = interactionService.Queryable()
                                    .AsNoTracking()
                                    .Where( a => a.InteractionComponent.InteractionChannelId == interactionChannelCommunication.Id
                                    && a.InteractionComponent.EntityId.Value == communicationId );

            return interactionQuery;
        }

        /// <summary>
        /// Get summary entries for the set of Interactions related to a specific Communication.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="communicationId"></param>
        /// <returns></returns>
        private List<InteractionInfo> GetCommunicationInteractionsSummaryInfo( RockContext dataContext, int? communicationId )
        {
            if ( _Interactions == null )
            {
                if ( communicationId.HasValue )
                {
                    var interactionQuery = this.GetCommunicationInteractionsQuery( dataContext, communicationId );

                    var recipientService = new CommunicationRecipientService( dataContext );

                    var recipientQuery = recipientService.Queryable()
                        .AsNoTracking()
                        .Where( x => x.CommunicationId == communicationId );

                    // Get Unique Interations per Person
                    _Interactions = interactionQuery
                        .OrderBy( x => x.PersonAlias.PersonId )
                        .ThenByDescending( x => x.InteractionDateTime )
                        .Select( pi => new InteractionInfo
                        {
                            InteractionDateTime = pi.InteractionDateTime,
                            Operation = pi.Operation,
                            InteractionData = pi.InteractionData,
                            CommunicationRecipientId = pi.EntityId,
                            PersonId = recipientQuery.Where( x => x.Id == pi.EntityId ).Select( x => x.PersonAlias.PersonId ).FirstOrDefault()
                        } )
                        .ToList();
                }
                else
                {
                    _Interactions = new List<InteractionInfo>();
                }
            }

            return _Interactions;
        }

        /// <summary>
        /// Show details for Analytics: Most Popular Links
        /// </summary>
        /// <param name="interactionsList"></param>
        /// <param name="deliveredRecipientCount"></param>
        private void ShowMostPopularLinksData( List<InteractionInfo> interactionsList, int? deliveredRecipientCount )
        {
            /* Most Popular Links from Clicks*/
            var topClicks = interactionsList
                                .Where( a =>
                                    a.Operation == "Click"
                                    && !string.IsNullOrEmpty( a.InteractionData )
                                    && !a.InteractionData.Contains( "/Unsubscribe/" ) )
                                .GroupBy( a => a.InteractionData )
                                .Select( a => new
                                {
                                    LinkUrl = a.Key,
                                    UniqueClickCount = a.GroupBy( x => x.CommunicationRecipientId ).Count()
                                } )
                                .OrderByDescending( a => a.UniqueClickCount )
                                .Take( 100 )
                                .ToList();

            if ( topClicks.Any() )
            {
                int topLinkCount = topClicks.Max( a => a.UniqueClickCount );

                var mostPopularLinksData = topClicks.Select( a => new TopLinksInfo
                {
                    PercentOfTop = ( decimal ) a.UniqueClickCount * 100 / topLinkCount,
                    Url = a.LinkUrl,
                    UniquesCount = a.UniqueClickCount,
                    CTRPercent = deliveredRecipientCount.GetValueOrDefault( 0 ) > 0 ? a.UniqueClickCount * 100.00M / deliveredRecipientCount : 0
                } ).ToList();

                pnlCTRHeader.Visible = true;

                rptMostPopularLinks.DataSource = mostPopularLinksData;
                rptMostPopularLinks.DataBind();
                pnlMostPopularLinks.Visible = true;
            }
            else
            {
                pnlMostPopularLinks.Visible = false;
            }
        }

        /// <summary>
        /// Show details for Analytics: Clients
        /// </summary>
        /// <param name="interactionQuery"></param>
        /// <param name="totalInteractionCount"></param>
        /// <param name="noDataMessageName"></param>
        private void ShowClientsInUseData( IQueryable<Interaction> interactionQuery, int totalInteractionCount, string noDataMessageName )
        {
            /* Clients-In-Use (Client Type) Pie Chart*/
            var clientsUsageByClientType = interactionQuery
                .GroupBy( a => ( a.InteractionSession.DeviceType.ClientType ?? "Unknown" ).ToLower() ).Select( a => new ClientTypeUsageInfo
                {
                    ClientType = a.Key,
                    UsagePercent = a.Count() * 100.00M / totalInteractionCount
                } ).OrderByDescending( a => a.UsagePercent ).ToList()
                .Where( a => !a.ClientType.Equals( "Robot", StringComparison.OrdinalIgnoreCase ) ) // no robots
                .Select( a => new ClientTypeUsageInfo
                {
                    ClientType = a.ClientType,
                    UsagePercent = Math.Round( a.UsagePercent, 2 )
                } ).ToList();

            this.PieChartDataClientLabelsJSON = clientsUsageByClientType.Select( a => string.IsNullOrEmpty( a.ClientType ) ? "Unknown" : a.ClientType.Transform( To.TitleCase ) ).ToList().ToJson();
            this.PieChartDataClientCountsJSON = clientsUsageByClientType.Select( a => a.UsagePercent ).ToList().ToJson();

            var clientUsageHasData = clientsUsageByClientType.Where( a => a.UsagePercent > 0 ).Any();
            clientsDoughnutChartCanvas.Style[HtmlTextWriterStyle.Display] = clientUsageHasData ? string.Empty : "none";
            nbClientsDoughnutChartMessage.Visible = !clientUsageHasData;
            nbClientsDoughnutChartMessage.Text = "No client usage activity" + ( !string.IsNullOrEmpty( noDataMessageName ) ? " for " + noDataMessageName : string.Empty );

            /* Clients-In-Use (Application) Grid */
            var clientsUsageByApplication = interactionQuery
            .GroupBy( a => a.InteractionSession.DeviceType.Application ).Select( a => new ApplicationUsageInfo
            {
                Application = a.Key,
                UsagePercent = ( a.Count() * 100.00M / totalInteractionCount )
            } ).OrderByDescending( a => a.UsagePercent ).ToList();

            pnlClientApplicationUsage.Visible = clientsUsageByApplication.Any();
            rptClientApplicationUsage.DataSource = clientsUsageByApplication;
            rptClientApplicationUsage.DataBind();
        }

        /// <summary>
        /// Show details for Analytics: Actions Summary.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="interactionsList"></param>
        /// <param name="noDataMessageName"></param>
        /// <param name="deliveredRecipientCount"></param>
        private void ShowAnalyticsPanelActionsSummary( RockContext dataContext, List<InteractionInfo> interactionsList, string noDataMessageName, int? deliveredRecipientCount )
        {
            TimeSpan roundTimeSpan = TimeSpan.FromDays( 1 );

            // Get the interactions summary information
            var interactionsSummary = new List<SummaryInfo>();

            if ( interactionsList.Any() )
            {
                var firstDateTime = interactionsList.Min( a => a.InteractionDateTime );
                var lastDateTime = interactionsList.Max( a => a.InteractionDateTime );
                var weeksCount = ( lastDateTime - firstDateTime ).TotalDays / 7;

                if ( weeksCount > 26 )
                {
                    // if there is more than 26 weeks worth, summarize by week
                    roundTimeSpan = TimeSpan.FromDays( 7 );
                }
                else if ( weeksCount > 3 )
                {
                    // if there is more than 3 weeks worth, summarize by day
                    roundTimeSpan = TimeSpan.FromDays( 1 );
                }
                else
                {
                    // if there is less than 3 weeks worth, summarize by hour
                    roundTimeSpan = TimeSpan.FromHours( 1 );
                    this.LineChartTimeFormat = "LLLL";
                }

                // Get a summary of interactions.
                // If a Click interaction exists without an Open, the Open is implied in the count.
                interactionsSummary = interactionsList.GroupBy( a => new { a.CommunicationRecipientId } )
                    .Select( a => new
                    {
                        InteractionSummaryDateTime = a.Min( b => b.InteractionDateTime ).Round( roundTimeSpan ),
                        a.Key.CommunicationRecipientId,
                        Clicked = a.Any( x => x.Operation == "Click" ),
                        Opened = a.Any( x => x.Operation == "Opened" )
                    } )
                    .GroupBy( a => a.InteractionSummaryDateTime )
                    .Select( x => new SummaryInfo
                    {
                        SummaryDateTime = x.Key,
                        ClickCounts = x.Count( xx => xx.Clicked ),
                        OpenCounts = x.Count( xx => xx.Opened || ( !xx.Opened && xx.Clicked ) )
                    } ).OrderBy( a => a.SummaryDateTime ).ToList();
            }

            var hasInteractions = interactionsSummary.Any();

            var colors = this.GetAttributeValue( AttributeKey.SeriesColors );

            if ( string.IsNullOrEmpty( colors ) )
            {
                colors = SeriesColorsDefaultValue;
            }

            this.SeriesColorsJSON = colors.SplitDelimitedValues().ToArray().ToJson();

            // Opens/Clicks Line Chart
            this.LineChartTimeFormat = "LL";
            this.LineChartDataLabelsJSON = "[" + interactionsSummary.Select( a => "new Date('" + a.SummaryDateTime.ToString( "o" ) + "')" ).ToList().AsDelimited( ",\n" ) + "]";

            List<int> cumulativeClicksList = new List<int>();
            List<int> clickCountsList = interactionsSummary.Select( a => a.ClickCounts ).ToList();
            int clickCountsSoFar = 0;
            foreach ( var clickCounts in clickCountsList )
            {
                clickCountsSoFar += clickCounts;
                cumulativeClicksList.Add( clickCountsSoFar );
            }

            this.LineChartDataClicksJSON = cumulativeClicksList.ToJson();

            List<int> cumulativeOpensList = new List<int>();
            List<int> openCountsList = interactionsSummary.Select( a => a.OpenCounts ).ToList();
            int openCountsSoFar = 0;
            foreach ( var openCounts in openCountsList )
            {
                openCountsSoFar += openCounts;
                cumulativeOpensList.Add( openCountsSoFar );
            }

            this.LineChartDataOpensJSON = cumulativeOpensList.ToJson();

            List<int> unopenedCountsList = new List<int>();
            if ( deliveredRecipientCount.HasValue )
            {
                int unopenedRemaining = deliveredRecipientCount.Value;
                foreach ( var openCounts in openCountsList )
                {
                    unopenedRemaining = unopenedRemaining - openCounts;

                    // NOTE: just in case we have more recipients activity then there are recipient records, don't let it go negative
                    unopenedCountsList.Add( Math.Max( unopenedRemaining, 0 ) );
                }

                this.LineChartDataUnOpenedJSON = unopenedCountsList.ToJson();
            }
            else
            {
                this.LineChartDataUnOpenedJSON = "null";
            }

            /* Actions Pie Chart and Stats */
            var openInteractions = interactionsList.Where( a => a.Operation == "Opened" ).ToList();
            var clickInteractions = interactionsList.Where( a => a.Operation == "Click" ).ToList();

            int totalOpens = openInteractions.Count();
            int totalClicks = clickInteractions.Count();

            var recipientsWithOpens = openInteractions.GroupBy( a => a.CommunicationRecipientId ).Select( x => x.Key ).ToList();
            var recipientsWithClicks = clickInteractions.GroupBy( a => a.CommunicationRecipientId ).Select( x => x.Key ).ToList();

            int recipientsWithClicksNoOpensCount = recipientsWithClicks.Except( recipientsWithOpens ).Count();

            // Unique Clicks is the number of times a Recipient clicked at least once in an email
            int uniqueClicks = recipientsWithClicks.Count();

            // Unique Opens is the number of times a Recipient opened the message at least once.
            int uniqueOpens = recipientsWithOpens.Count();

            // When calculating Opens, include Recipients that have a Click interaction recorded without a corresponding Open interaction
            // to capture the scenario where an email is viewed without loading the image links that are required to trigger the Open event.
            // For Total Opens, only impute a single Open per recipient regardless of the number of Clicks.
            uniqueOpens += recipientsWithClicksNoOpensCount;
            totalOpens += recipientsWithClicksNoOpensCount;

            decimal percentOpened = 0;

            if ( deliveredRecipientCount.HasValue )
            {
                percentOpened = ( deliveredRecipientCount.Value > 0 ? ( decimal ) uniqueOpens / deliveredRecipientCount.Value : 0 );

                // just in case there are more opens then delivered, don't let it go negative
                var unopenedCount = Math.Max( deliveredRecipientCount.Value - uniqueOpens, 0 );
            }

            decimal ctr = 0;

            if ( uniqueOpens > 0 )
            {
                ctr = ( decimal ) uniqueClicks / uniqueOpens;
            }

            string actionsStatFormatNumber = "<div class='js-actions-statistic' title='{0}'>{1:#,##0}</div>";
            string actionsStatFormatPercent = "<div class='js-actions-statistic' title='{0}'>{1:P2}</div>";

            lUniqueOpens.Text = string.Format( actionsStatFormatNumber, "The number of emails that were opened at least once", uniqueOpens );
            lTotalOpens.Text = string.Format( actionsStatFormatNumber, "The total number of times the emails were opened, including ones that were already opened once", totalOpens );
            lPercentOpened.Text = string.Format( actionsStatFormatPercent, "The percent of the delivered emails that were opened at least once", percentOpened );

            lUniqueClicks.Text = string.Format( actionsStatFormatNumber, "The number of times a recipient clicked on a link at least once in any of the opened emails", uniqueClicks );
            lTotalClicks.Text = string.Format( actionsStatFormatNumber, "The total number of times a link was clicked in any of the opened emails", totalClicks );
            lClickThroughRate.Text = string.Format( actionsStatFormatPercent, "The percent of emails that had at least one click", ctr );

            // action stats is [opens,clicks,unopened];
            var actionsStats = new int?[3];

            // "Opens" would be unique number that Clicked Or Opened, so subtract clicks so they aren't counted twice
            actionsStats[0] = uniqueOpens - uniqueClicks;
            actionsStats[1] = uniqueClicks;

            if ( deliveredRecipientCount.HasValue )
            {
                // NOTE: just in case we have more recipients activity then there are recipient records, don't let it go negative
                actionsStats[2] = Math.Max( deliveredRecipientCount.Value - uniqueOpens, 0 );
            }
            else
            {
                actionsStats[2] = null;
            }

            this.PieChartDataOpenClicksJSON = actionsStats.ToJson();

            var pieChartOpenClicksHasData = actionsStats.Sum() > 0;
            opensClicksPieChartCanvas.Style[HtmlTextWriterStyle.Display] = pieChartOpenClicksHasData ? string.Empty : "none";

            nbOpenClicksPieChartMessage.Visible = !pieChartOpenClicksHasData;
            nbOpenClicksPieChartMessage.Text = "No communications activity" + ( !string.IsNullOrEmpty( noDataMessageName ) ? " for " + noDataMessageName : string.Empty );
        }

        #region Recipients List

        /// <summary>
        /// Get the collection of columns for the Recipients List.
        /// </summary>
        /// <returns></returns>
        private List<CommunicationDetailReportColumnInfo> GetReportColumns()
        {
            if ( _Columns == null )
            {
                _Columns = new List<CommunicationDetailReportColumnInfo>();

                AddPropertyColumn( _Columns, PersonPropertyColumn.Gender );
                AddPropertyColumn( _Columns, PersonPropertyColumn.ConnectionStatus, "Connection Status" );
                AddPropertyColumn( _Columns, PersonPropertyColumn.RecordStatus, "Record Status" );
                AddPropertyColumn( _Columns, PersonPropertyColumn.IsDeceased, "Is Deceased" );
                AddPropertyColumn( _Columns, PersonPropertyColumn.Email, "Email" );
                AddPropertyColumn( _Columns, PersonPropertyColumn.AgeClassification, "Age Classification" );
                AddPropertyColumn( _Columns, PersonPropertyColumn.Birthdate, "Birthdate" );

                AddCalculatedColumn( _Columns, PersonPropertyColumn.Age, "Rock.Reporting.DataSelect.Person.AgeSelect" );
                AddCalculatedColumn( _Columns, PersonPropertyColumn.Grade, "Rock.Reporting.DataSelect.Person.GradeSelect" );

                // Add Person Attributes that the current user is authorized to View.
                var person = new Person();

                person.LoadAttributes();

                var allowedAttributes = person.Attributes.Where( a => a.Value.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) ).ToList();

                foreach ( var attribute in allowedAttributes )
                {
                    AddAttributeColumn( _Columns, attribute.Value.Guid.ToString(), attribute.Value.Name );
                }
            }

            return _Columns;
        }

        /// <summary>
        /// Add a Person Property to the column selection for the Recipients List.
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="propertyName"></param>
        /// <param name="name"></param>
        private void AddPropertyColumn( List<CommunicationDetailReportColumnInfo> columns, string propertyName, string name = null )
        {
            var info = new CommunicationDetailReportColumnInfo();

            info.Key = propertyName;
            info.Name = name ?? propertyName;

            info.ContentType = PersonDataSourceColumnSourceSpecifier.Property;
            info.ColumnSourceIdentifier = propertyName;

            columns.Add( info );
        }

        /// <summary>
        /// Add a calculated column to the column selection for the Recipients List.
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="name"></param>
        /// <param name="dataSelectName"></param>
        /// <param name="settings"></param>
        private void AddCalculatedColumn( List<CommunicationDetailReportColumnInfo> columns, string name, string dataSelectName, object settings = null )
        {
            var info = new CommunicationDetailReportColumnInfo();

            info.Key = dataSelectName;
            info.Name = name;

            info.ContentType = PersonDataSourceColumnSourceSpecifier.Calculated;
            info.ColumnSourceIdentifier = dataSelectName;

            if ( settings != null )
            {
                info.Settings = settings.ToJson();
            }

            columns.Add( info );
        }

        /// <summary>
        /// Add a Person Attribute to the column selection for the Recipients List.
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="attributeGuid"></param>
        /// <param name="name"></param>
        private void AddAttributeColumn( List<CommunicationDetailReportColumnInfo> columns, string attributeGuid, string name )
        {
            var info = new CommunicationDetailReportColumnInfo();

            info.Key = attributeGuid;
            info.Name = name;

            info.ContentType = PersonDataSourceColumnSourceSpecifier.Attribute;
            info.ColumnSourceIdentifier = attributeGuid;

            columns.Add( info );
        }

        /// <summary>
        /// Add the required columns to the grid.
        /// </summary>
        /// <param name="skipCount"></param>
        /// <param name="takeCount"></param>
        /// <returns></returns>
        private void AddStandardRecipientColumns()
        {
            // Add the standard columns to the grid, inserted after the Name column.
            // Sorting is disabled for these columns because their data is only added after the initial report data page is retrieved.
            BoundField boundField;

            var nameField = gRecipients.GetColumnByHeaderText( "Name" );

            var insertAtIndex = gRecipients.GetColumnIndex( nameField ) + 1;

            boundField = new BoundField { HeaderText = "Status", DataField = "DeliveryStatus" };
            gRecipients.Columns.Insert( insertAtIndex, boundField );
            insertAtIndex++;

            boundField = new BoundField { HeaderText = "Medium", DataField = "CommunicationMediumName" };
            gRecipients.Columns.Insert( insertAtIndex, boundField );
            insertAtIndex++;

            boundField = new BoundField { HeaderText = "Note", DataField = "DeliveryStatusNote" };
            gRecipients.Columns.Insert( insertAtIndex, boundField );
            insertAtIndex++;

            var openedField = new BoolField();
            openedField.HeaderText = "Opened";
            openedField.DataField = "HasOpened";

            gRecipients.Columns.Insert( insertAtIndex, openedField );
            insertAtIndex++;

            var clickedField = new BoolField();
            clickedField.HeaderText = "Clicked";
            clickedField.DataField = "HasClicked";

            gRecipients.Columns.Insert( insertAtIndex, clickedField );
            insertAtIndex++;
        }

        /// <summary>
        /// Get the query that uniquely identifies the subset records that match the current filter.
        /// At a minimum, this query must return a unique identifier for each record that can be used to retrieve additional details about the item.
        /// Additional information may also be retrieved and cached here if it is
        /// </summary>
        /// <param name="skipCount"></param>
        /// <param name="takeCount"></param>
        /// <returns></returns>
        private void AddStandardRecipientFieldsToDataSource( RockContext dataContext, DataTable dataTable, ReportOutputBuilder builder ) //, int skipCount, int takeCount )
        {
            // Add the standard data to the data source.
            if ( !_GridIsExporting )
            {
                dataTable.Columns.Add( "IsActive", typeof( bool ) );
            }

            dataTable.Columns.Add( "CommunicationMediumName", typeof( string ) );
            dataTable.Columns.Add( "DeliveryStatus", typeof( string ) );
            dataTable.Columns.Add( "DeliveryStatusNote", typeof( string ) );
            dataTable.Columns.Add( "HasOpened", typeof( bool ) );
            dataTable.Columns.Add( "HasClicked", typeof( bool ) );

            // order by ModifiedDateTime to get a consistent result in case a person has received the communication more than once (more than one recipient record for the same person)
            var query = GetRecipientInfoQuery( dataContext ).OrderByDescending( a => a.ModifiedDateTime );
            var queryList = query.ToList();

            // create dictionary
            var recipients = new Dictionary<int, RecipientInfo>();
            foreach ( var recipient in queryList )
            {
                // since we order by ModifiedDateTime this will end up ignoring any order recipient records for the personid
                // NOTE: We tried to do this in SQL but it caused performance issues, so we'll do it in C# instead.
                recipients.AddOrIgnore( recipient.PersonId, recipient );
            }

            builder.FillDataColumnValues( dataTable, recipients );
        }

        /// <summary>
        /// Get a query containing basic information about the Recipients of the communication.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        private IQueryable<RecipientInfo> GetRecipientInfoQuery( RockContext dataContext )
        {
            var inactiveStatusId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_INACTIVE.AsGuid() );

            // Get the set of Click interactions for this Communication, using only the most recent interaction for a Person who is listed as a recipient multiple times.
            var interactions = GetCommunicationInteractionsSummaryInfo( dataContext, this.CommunicationId );

            var clickRecipientsIdList = interactions
                .Where( x => x.PersonId != null && x.Operation == "Click" )
                .OrderByDescending( x => x.InteractionDateTime )
                .GroupBy( k => k.PersonId )
                .Select( kv => kv.FirstOrDefault().PersonId );

            // Get the recipient information.
            // Translate the Message Status to a Delivery Status by converting "Opened" to "Delivered" and "Sending" to "Pending".
            // For each Person, return only the most-recently updated Recipient record.
            var recipientService = new CommunicationRecipientService( dataContext );

            var recipientQuery = recipientService.Queryable()
                    .AsNoTracking()
                    .Where( x => x.CommunicationId == CommunicationId.Value )
                    .Select( x => new RecipientInfo
                    {
                        PersonId = x.PersonAlias.PersonId,
                        IsActive = ( x.PersonAlias.Person.RecordStatusValueId != inactiveStatusId ),
                        IsDeceased = x.PersonAlias.Person.IsDeceased,
                        CommunicationMediumName = x.MediumEntityType.FriendlyName,
                        DeliveryStatus = ( x.Status == CommunicationRecipientStatus.Opened ? "Delivered" : ( x.Status == CommunicationRecipientStatus.Sending ? "Pending" : x.Status.ToString() ) ),
                        DeliveryStatusNote = x.StatusNote,
                        HasOpened = ( x.Status == CommunicationRecipientStatus.Opened ),
                        HasClicked = clickRecipientsIdList.Contains( x.PersonAlias.PersonId ),
                        ModifiedDateTime = x.ModifiedDateTime
                    } );

            return recipientQuery;
        }

        /// <summary>
        /// Populate the selection lists for the Recipient Grid filter.
        /// </summary>
        private void PopulateRecipientFilterSelectionLists()
        {
            cblDeliveryStatus.BindToEnum( ignoreTypes: new CommunicationRecipientStatus[] { CommunicationRecipientStatus.Sending, CommunicationRecipientStatus.Opened } );

            cblMedium.Items.Clear();
            cblMedium.Items.Add( new ListItem { Text = "Email", Value = Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL } );
            cblMedium.Items.Add( new ListItem { Text = "SMS", Value = Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS } );

            cblOpenedStatus.Items.Clear();
            cblOpenedStatus.Items.Add( new ListItem { Text = "Opened", Value = "Opened" } );
            cblOpenedStatus.Items.Add( new ListItem { Text = "Not Opened", Value = "NotOpened" } );

            cblClickedStatus.Items.Clear();
            cblClickedStatus.Items.Add( new ListItem { Text = "Clicked", Value = "Clicked" } );
            cblClickedStatus.Items.Add( new ListItem { Text = "Not Clicked", Value = "NotClicked" } );
        }

        /// <summary>
        /// Get a Linq Expression that represents a predicate suitable for use in the Where clause of an IQueryable<Person>.
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="personService"></param>
        /// <param name="parameterExpression"></param>
        /// <returns></returns>
        private Expression GetRecipientFilterExpression( RockContext dataContext, PersonService personService, ParameterExpression parameterExpression )
        {
            // Get the Person query for the recipients of this communication.
            var filterSettingsKeyValueMap = GetRecipientsFilterSettings();

            //
            // Get a filtered list of Communication Recipients.
            //
            var recipientQuery = new CommunicationRecipientService( dataContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( x => x.CommunicationId == CommunicationId.Value );

            // Filter by: Communication Medium
            var mediumList = filterSettingsKeyValueMap[FilterSettingName.CommunicationMedium].SplitDelimitedValues( "," ).AsGuidList();

            if ( mediumList.Any() )
            {
                recipientQuery = recipientQuery.Where( x => mediumList.Contains( x.MediumEntityType.Guid ) );
            }

            // Filter by: Delivery Status
            var deliveryStatusList = filterSettingsKeyValueMap[FilterSettingName.DeliveryStatus].SplitDelimitedValues( "," ).AsIntegerList();

            if ( deliveryStatusList.Any() )
            {
                // If Status includes "Delivered", "Opened" should also be included.
                if ( deliveryStatusList.Contains( ( int ) CommunicationRecipientStatus.Delivered ) )
                {
                    deliveryStatusList.Add( ( int ) CommunicationRecipientStatus.Opened );
                }

                recipientQuery = recipientQuery.Where( x => deliveryStatusList.Contains( ( int ) x.Status ) );
            }

            // Filter by: Delivery Status Note
            var statusNote = filterSettingsKeyValueMap[FilterSettingName.DeliveryStatusNote].ToStringSafe();

            if ( !string.IsNullOrWhiteSpace( statusNote ) )
            {
                recipientQuery = recipientQuery.Where( x => x.StatusNote.Contains( statusNote ) );
            }

            // Filter by: Has Opened
            var openedStatusList = filterSettingsKeyValueMap[FilterSettingName.OpenedStatus].SplitDelimitedValues( "," );

            if ( openedStatusList.Contains( "Opened" )
                && openedStatusList.Contains( "NotOpened" ) )
            {
                // Ignore
            }
            else if ( openedStatusList.Contains( "Opened" ) )
            {
                recipientQuery = recipientQuery.Where( x => x.Status == CommunicationRecipientStatus.Opened );
            }
            else if ( openedStatusList.Contains( "NotOpened" ) )
            {
                recipientQuery = recipientQuery.Where( x => x.Status != CommunicationRecipientStatus.Opened );
            }

            // Filter by: Has Clicked
            var clickStatusList = filterSettingsKeyValueMap[FilterSettingName.ClickedStatus].SplitDelimitedValues( "," );

            if ( clickStatusList.Contains( "Clicked" )
                && clickStatusList.Contains( "NotClicked" ) )
            {
                // Ignore
            }
            else if ( clickStatusList.Contains( "Clicked" ) )
            {
                var interactions = this.GetCommunicationInteractionsSummaryInfo( dataContext, this.CommunicationId );

                var clickRecipientIdList = interactions.Where( x => x.Operation == "Click" )
                    .Select( x => x.CommunicationRecipientId );

                recipientQuery = recipientQuery.Where( x => clickRecipientIdList.Contains( x.Id ) );
            }
            else if ( clickStatusList.Contains( "NotClicked" ) )
            {
                var interactions = this.GetCommunicationInteractionsSummaryInfo( dataContext, this.CommunicationId );

                var clickRecipientIdList = _Interactions.Where( x => x.Operation == "Click" )
                    .Select( x => x.CommunicationRecipientId );

                recipientQuery = recipientQuery.Where( x => !clickRecipientIdList.Contains( x.Id ) );
            }

            //
            // Get a filtered list of People.
            //
            var personFilterQuery = personService.Queryable().AsNoTracking();

            // Filter for Communication Recipients
            var recipientIdQuery = recipientQuery.Select( x => x.PersonAlias.PersonId );

            personFilterQuery = personFilterQuery.Where( x => recipientIdQuery.Contains( x.Id ) );

            // Filter by: First Name
            var firstName = filterSettingsKeyValueMap[FilterSettingName.FirstName].ToStringSafe();

            if ( !string.IsNullOrWhiteSpace( firstName ) )
            {
                personFilterQuery = personFilterQuery.Where( x => x.FirstName.StartsWith( firstName )
                    || x.NickName.StartsWith( firstName ) );
            }

            // Filter by: Last Name
            var lastName = filterSettingsKeyValueMap[FilterSettingName.LastName].ToStringSafe();

            if ( !string.IsNullOrWhiteSpace( lastName ) )
            {
                personFilterQuery = personFilterQuery.Where( p => p.LastName.StartsWith( lastName ) );
            }

            // Combine the Recipient Query and the Person Query to create the filter.
            var personIdFilterQuery = personFilterQuery.Select( p => p.Id );

            var filterQuery = personService.Queryable().Where( p => personIdFilterQuery.Contains( p.Id ) );

            var filterExpression = FilterExpressionExtractor.Extract<Rock.Model.Person>( filterQuery, parameterExpression, "p" );

            return filterExpression;
        }

        #endregion

        /// <summary>
        /// Gets the ID of the control which trigged a postback.
        /// </summary>
        /// <param name = "page">The page.</param>
        /// <returns></returns>
        private Control GetPostBackControl()
        {
            var page = this.Page;

            if ( !page.IsPostBack )
            {
                return null;
            }

            Control control = null;

            // first we will check the "__EVENTTARGET" because if post back made by the controls
            // which used "_doPostBack" function also available in Request.Form collection.
            string controlName = page.Request.Params["__EVENTTARGET"];

            if ( !string.IsNullOrEmpty( controlName ) )
            {
                control = page.FindControl( controlName );
            }
            else
            {
                // if __EVENTTARGET is null, the control is a button type and we need to
                // iterate over the form collection to find it
                string controlId;
                Control foundControl;

                foreach ( string ctl in page.Request.Form )
                {
                    if ( ctl == null )
                    {
                        continue;
                    }

                    // Handle ImageButton as a special case, because the control Id contains an identifier for the (x,y) coordinates of the click event.
                    if ( ctl.EndsWith( ".x" ) || ctl.EndsWith( ".y" ) )
                    {
                        controlId = ctl.Substring( 0, ctl.Length - 2 );
                        foundControl = page.FindControl( controlId );
                    }
                    else
                    {
                        foundControl = page.FindControl( ctl );
                    }

                    if ( !( foundControl is IButtonControl ) )
                        continue;

                    control = foundControl;
                    break;
                }
            }

            return control;
        }

        /// <summary>
        /// Initialize handlers for block configuration change events.
        /// </summary>
        private void InitializeBlockConfigurationChangeHandler( UpdatePanel panelMain )
        {
            // Handle the Block Settings change notification.
            this.BlockUpdated += Block_BlockUpdated;

            AddConfigurationUpdateTrigger( panelMain );
        }

        /// <summary>
        /// Load the active Communication object, either from cache or the data store.
        /// </summary>
        private void InitializeActiveCommunication()
        {
            // Check if CommunicationDetail has already loaded existing communication
            _Communication = RockPage.GetSharedItem( "Communication" ) as Rock.Model.Communication;

            if ( _Communication == null )
            {
                CommunicationId = PageParameter( PageParameterKey.CommunicationId ).AsIntegerOrNull();
                if ( CommunicationId.HasValue )
                {
                    var dataContext = this.GetDataContext();

                    _Communication = new CommunicationService( dataContext )
                        .Queryable( "CreatedByPersonAlias.Person" )
                        .Where( c => c.Id == CommunicationId.Value )
                        .FirstOrDefault();
                }
            }
            else
            {
                CommunicationId = _Communication.Id;
            }
        }

        /// <summary>
        /// Get a lookup table of Panel control names mapped to tab names.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetPanelControlToTabNameMap()
        {
            if ( _PanelControlToTabNameMap == null )
            {
                _PanelControlToTabNameMap = new Dictionary<string, string>();

                _PanelControlToTabNameMap.Add( pnlAnalyticsTab.UniqueID, "Analytics" );
                _PanelControlToTabNameMap.Add( pnlRecipients.UniqueID, "Recipients" );
                _PanelControlToTabNameMap.Add( pnlMessage.UniqueID, "Message" );
                _PanelControlToTabNameMap.Add( pnlActivity.UniqueID, "Activity" );
            }

            return _PanelControlToTabNameMap;
        }

        /// <summary>
        /// Display the Recipients List filtered by the specified delivery status.
        /// </summary>
        /// <param name="status"></param>
        private void ShowRecipientsListForDeliveryStatus( CommunicationRecipientStatus status )
        {
            SetActivePanel( CommunicationDetailPanels.RecipientDetails );

            InitializeActiveCommunication();

            PopulatePersonPropertiesSelectionItems();
            PopulatePersonAttributesSelectionItems();
            PopulateRecipientFilterSelectionLists();

            LoadRecipientListPreferences();

            // Set the filter.
            var settings = GetRecipientsFilterSettings();

            settings[FilterSettingName.DeliveryStatus] = status.ConvertToInt().ToString();

            ApplyRecipientsFilterSettings( settings );

            SaveRecipientsFilterSettings();

            BindRecipientsGrid();
        }

        #endregion

        #region Support Classes and Enumerations

        public static class CommunicationDetailPanels
        {
            public static string Analytics = "Analytics";
            public static string MessageDetails = "Message";
            public static string Activity = "Activity";
            public static string RecipientDetails = "Recipients";
        }

        #endregion

        #region Block specific classes

        /// <summary>
        ///
        /// </summary>
        public class SummaryInfo
        {
            /// <summary>
            /// Gets or sets the summary date time.
            /// </summary>
            /// <value>
            /// The summary date time.
            /// </value>
            public DateTime SummaryDateTime { get; set; }

            /// <summary>
            /// Gets or sets the click counts.
            /// </summary>
            /// <value>
            /// The click counts.
            /// </value>
            public int ClickCounts { get; set; }

            /// <summary>
            /// Gets or sets the open counts.
            /// </summary>
            /// <value>
            /// The open counts.
            /// </value>
            public int OpenCounts { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public class TopLinksInfo
        {
            /// <summary>
            /// Gets or sets the percent of top.
            /// </summary>
            /// <value>
            /// The percent of top.
            /// </value>
            public decimal PercentOfTop { get; set; }

            /// <summary>
            /// Gets or sets the URL.
            /// </summary>
            /// <value>
            /// The URL.
            /// </value>
            public string Url { get; set; }

            /// <summary>
            /// Gets or sets the uniques count.
            /// </summary>
            /// <value>
            /// The uniques count.
            /// </value>
            public int UniquesCount { get; set; }

            /// <summary>
            /// Gets or sets the CTR percent.
            /// </summary>
            /// <value>
            /// The CTR percent.
            /// </value>
            public decimal? CTRPercent { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public class ClientTypeUsageInfo
        {
            /// <summary>
            /// Gets or sets the type of the client.
            /// </summary>
            /// <value>
            /// The type of the client.
            /// </value>
            public string ClientType { get; set; }

            /// <summary>
            /// Gets or sets the usage percent.
            /// </summary>
            /// <value>
            /// The usage percent.
            /// </value>
            public decimal UsagePercent { get; set; }
        }

        /// <summary>
        ///
        /// </summary>
        public class ApplicationUsageInfo
        {
            /// <summary>
            /// Gets or sets the application.
            /// </summary>
            /// <value>
            /// The application.
            /// </value>
            public string Application { get; set; }

            /// <summary>
            /// Gets or sets the usage percent.
            /// </summary>
            /// <value>
            /// The usage percent.
            /// </value>
            public decimal UsagePercent { get; set; }
        }

        private class CommunicationDetailReportColumnInfo
        {
            public string Key { get; set; }
            public string Name { get; set; }
            public PersonDataSourceColumnSourceSpecifier ContentType { get; set; }

            /// <summary>
            /// A unique identifier for the column source, evaluated in the context of the ContentType.
            /// ContentType=Property: the value is the property name.
            /// ContentType=Attribute: the value is the Attribute Guid.
            /// ContentType=Calculated: the value is the fully-qualified name of a DataSelect component.
            /// </summary>
            public string ColumnSourceIdentifier { get; set; }

            /// <summary>
            /// A JSON representation of a settings object for the selected column.
            /// The structure of the settings object is determined by the ContentType.
            /// </summary>
            public string Settings { get; set; }
        }

        private enum PersonDataSourceColumnSourceSpecifier
        {
            Property = 0,
            Attribute = 1,
            Calculated = 2
        }

        /// <summary>
        /// The set of properties available for display in the Recipients list.
        /// </summary>
        private static class PersonPropertyColumn
        {
            public static string Gender = "Gender";
            public static string ConnectionStatus = "ConnectionStatusValueId";
            public static string RecordStatus = "RecordStatusValueId";
            public static string IsDeceased = "IsDeceased";
            public static string Age = "Age";
            public static string Email = "Email";
            public static string AgeClassification = "AgeClassification";
            public static string Birthdate = "BirthDate";
            public static string Grade = "Grade";
            public static string IsActive = "IsActive";
        }

        /// <summary>
        /// Data structure used to store the result of a communication recipient query.
        /// </summary>
        private class RecipientInfo
        {
            public int PersonId { get; set; }
            public bool IsActive { get; set; }
            public bool IsDeceased { get; set; }
            public bool HasOpened { get; set; }
            public bool HasClicked { get; set; }
            public string DeliveryStatus { get; set; }
            public string DeliveryStatusNote { get; set; }
            public string CommunicationMediumName { get; set; }
            public DateTime? ModifiedDateTime { get; set; }
        }

        /// <summary>
        /// Data structure used to store the results of an Interaction query.
        /// </summary>
        private class InteractionInfo
        {
            public DateTime InteractionDateTime { get; set; }
            public string Operation { get; set; }
            public string InteractionData { get; set; }
            public int? CommunicationRecipientId { get; set; }
            public int? PersonId { get; set; }
        }

        /// <summary>
        /// Data structure used to store settings for the Recipients List.
        /// </summary>
        private class RecipientListPreferences
        {
            public List<string> SelectedProperties = new List<string>();
            public List<string> SelectedAttributes = new List<string>();
        }

        #endregion
    }
}