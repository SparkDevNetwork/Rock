// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using System.Xml.Xsl;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls.Communication;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    /// <summary>
    /// User control for creating a new communication.  This block should be used on same page as the CommunicationDetail block and only visible when editing a new or transient communication
    /// </summary>
    [DisplayName( "Communication Entry" )]
    [Category( "Communication" )]
    [Description( "Used for creating and sending a new communications such as email, SMS, etc. to recipients." )]

    [SecurityAction( Authorization.APPROVE, "The roles and/or users that have access to approve new communications." )]

    [ComponentsField( "Rock.Communication.ChannelContainer, Rock", "Channels", "The Channels that should be available to user to send through (If none are selected, all active channels will be available).", false, "", "", 0  )]
    [CommunicationTemplateField("Default Template", "The default template to use for a new communication.  (Note: This will only be used if the template is for the same channel as the communication.)", false, "", "", 1)]
    [IntegerField( "Maximum Recipients", "The maximum number of recipients allowed before communication will need to be approved", false, 0, "", 2 )]
    [IntegerField( "Display Count", "The initial number of recipients to display prior to expanding list", false, 0, "", 3  )]
    [BooleanField( "Send When Approved", "Should communication be sent once it's approved (vs. just being queued for scheduled job to send)?", true, "", 4 )]
    public partial class CommunicationEntry : RockBlock
    {
        #region Properties

        protected int? CommunicationId
        {
            get { return ViewState["CommunicationId"] as int?; }
            set { ViewState["CommunicationId"] = value; }
        }

        /// <summary>
        /// Gets or sets the channel entity type id.
        /// </summary>
        /// <value>
        /// The channel entity type id.
        /// </value>
        protected int? ChannelEntityTypeId
        {
            get { return ViewState["ChannelEntityTypeId"] as int?; }
            set { ViewState["ChannelEntityTypeId"] = value; }
        }

        /// <summary>
        /// Gets or sets the recipients.
        /// </summary>
        /// <value>
        /// The recipient ids.
        /// </value>
        protected List<Recipient> Recipients
        {
            get 
            { 
                var recipients = ViewState["Recipients"] as List<Recipient>;
                if ( recipients == null )
                {
                    recipients = new List<Recipient>();
                    ViewState["Recipients"] = recipients;
                }
                return recipients;
            }

            set { ViewState["Recipients"] = value; }
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
        /// Gets or sets the channel data.
        /// </summary>
        /// <value>
        /// The channel data.
        /// </value>
        protected Dictionary<string, string> ChannelData
        {
            get 
            {
                var channelData = ViewState["ChannelData"] as Dictionary<string, string>;
                if ( channelData == null )
                {
                    channelData = new Dictionary<string, string>();
                    ViewState["ChannelData"] = channelData;
                }
                return channelData;
            }

            set { ViewState["ChannelData"] = value; }
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

        #endregion

        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string script = @"
    $('a.remove-all-recipients').click(function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to remove all of the pending recipients from this communication?', function (result) {
            if (result) {
                eval(e.target.href);
            }
        });
    });
";
            ScriptManager.RegisterStartupScript(lbRemoveAllRecipients, lbRemoveAllRecipients.GetType(), "ConfirmRemoveAll", script, true );
        }
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( Page.IsPostBack )
            {
                LoadChannelControl( false );
            }
            else
            {
                ShowAllRecipients = false;

                // Check if CommunicationDetail has already loaded existing communication
                var communication = RockPage.GetSharedItem( "Communication" ) as Rock.Model.Communication;
                if ( communication == null )
                {
                    // If not, check page parameter for existing communiciaton
                    int? communicationId = PageParameter( "CommunicationId" ).AsIntegerOrNull();
                    if ( communicationId.HasValue )
                    {
                        communication = new CommunicationService( new RockContext() ).Get( communicationId.Value );
                    }
                }
                else
                {
                    CommunicationId = communication.Id;
                }

                if ( communication == null ||
                    communication.Status == CommunicationStatus.Transient ||
                    communication.Status == CommunicationStatus.Draft ||
                    communication.Status == CommunicationStatus.Denied )
                {
                    // If viewing a new, transient, draft, or denied communication, use this block
                    ShowDetail( communication );
                }
                else
                {
                    // Otherwise, use the communication detail block
                    this.Visible = false;
                }

            }
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
            int? templateId = ddlTemplate.SelectedValue.AsIntegerOrNull();
            if ( templateId.HasValue )
            {
                GetTemplateData( templateId.Value );
            }
        }
        
        /// <summary>
        /// Handles the Click event of the lbChannel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbChannel_Click( object sender, EventArgs e )
        {
            GetChannelData();
            var linkButton = sender as LinkButton;
            if ( linkButton != null )
            {
                int channelId = int.MinValue;
                if ( int.TryParse( linkButton.CommandArgument, out channelId ) )
                {
                    ChannelEntityTypeId = channelId;
                    BindChannels();

                    LoadChannelControl( true );
                    LoadTemplates();
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
                    var Person = new PersonService( new RockContext() ).Get( ppAddPerson.PersonId.Value );
                    if ( Person != null )
                    {
                        Recipients.Add( new Recipient( Person, CommunicationRecipientStatus.Pending ) );
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


                        if ( ChannelEntityTypeId == EntityTypeCache.Read( "Rock.Communication.Channel.Email" ).Id )
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
                                        var channelData = ChannelData;
                                        if ( cbBulk.Checked )
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
                        else if ( ChannelEntityTypeId == EntityTypeCache.Read( "Rock.Communication.Channel.Sms" ).Id )
                        {
                            if ( !recipient.HasSmsNumber )
                            {
                                // No SMS Number
                                textClass = "text-danger";
                                textTooltip = "No phone number with SMS enabled.";
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
                Recipients = Recipients.Where( r => r.PersonId != personId ).ToList();
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
            Recipients = Recipients.Where( r => r.Status != CommunicationRecipientStatus.Pending).ToList();
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
        protected void btnSubmit_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                var rockContext = new RockContext();
                var communication = UpdateCommunication( rockContext );

                if ( communication != null )
                {
                    string message = string.Empty;

                    if ( CheckApprovalRequired( communication.Recipients.Count ) && !IsUserAuthorized( "Approve" ) )
                    {
                        communication.Status = CommunicationStatus.PendingApproval;
                        message = "Communication has been submitted for approval.";
                    }
                    else
                    {
                        communication.Status = CommunicationStatus.Approved;
                        communication.ReviewedDateTime = RockDateTime.Now;
                        communication.ReviewerPersonId = CurrentPersonId;

                        if ( communication.FutureSendDateTime.HasValue &&
                            communication.FutureSendDateTime > RockDateTime.Now)
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

                    if ( communication.Status == CommunicationStatus.Approved &&
                        (!communication.FutureSendDateTime.HasValue || communication.FutureSendDateTime.Value <= RockDateTime.Now))
                    {
                        if ( GetAttributeValue( "SendWhenApproved" ).AsBoolean() )
                        {
                            var transaction = new Rock.Transactions.SendCommunicationTransaction();
                            transaction.CommunicationId = communication.Id;
                            transaction.PersonAlias = CurrentPersonAlias;
                            Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
                        }
                    }

                    ShowResult( message, communication );
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
            var rockContext = new RockContext();
            var communication = UpdateCommunication( rockContext );

            if ( communication != null )
            {
                communication.Status = CommunicationStatus.Draft;
                rockContext.SaveChanges();

                ShowResult( "The communication has been saved", communication );
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="communication">The communication.</param>
        private void ShowDetail (Rock.Model.Communication communication)
        {
            Recipients.Clear();

            if (communication != null)
            {
                this.AdditionalMergeFields = communication.AdditionalMergeFields.ToList();
                lTitle.Text = ( communication.Subject ?? "New Communication" ).FormatAsHtmlTitle();

                foreach(var recipient in new CommunicationRecipientService(new RockContext())
                    .Queryable("Person.PhoneNumbers")
                    .Where( r => r.CommunicationId == communication.Id))
                {
                    Recipients.Add( new Recipient( recipient.Person, recipient.Status, recipient.StatusNote, recipient.OpenedClient, recipient.OpenedDateTime));                
                }
            }
            else
            {
                communication = new Rock.Model.Communication() { Status = CommunicationStatus.Transient };
                lTitle.Text = "New Communication".FormatAsHtmlTitle();

                int? personId = PageParameter( "Person" ).AsIntegerOrNull();
                if ( personId.HasValue )
                {
                    communication.IsBulkCommunication = false;
                    var person = new PersonService( new RockContext() ).Get( personId.Value );
                    if ( person != null )
                    {
                        Recipients.Add( new Recipient( person, CommunicationRecipientStatus.Pending, string.Empty, string.Empty, null ) );
                    }
                }
            }

            CommunicationId = communication.Id;

            ChannelEntityTypeId = communication.ChannelEntityTypeId;
            BindChannels();

            ChannelData = communication.ChannelData;
            ChannelData.Add( "Subject", communication.Subject );

            if (communication.Status == CommunicationStatus.Transient && !string.IsNullOrWhiteSpace(GetAttributeValue("DefaultTemplate")))
            {
                var template = new CommunicationTemplateService( new RockContext() ).Get( GetAttributeValue( "DefaultTemplate" ).AsGuid() );
                
                // If a template guid was passed in, it overrides any default template.
                string templateGuid = PageParameter( "templateGuid" );
                if ( !string.IsNullOrEmpty( templateGuid ) )
                {
                    var guid = new Guid( templateGuid );
                    template = new CommunicationTemplateService( new RockContext() ).Queryable().Where( t => t.Guid == guid ).FirstOrDefault();
                }

                if (template != null && template.ChannelEntityTypeId == ChannelEntityTypeId)
                {
                    foreach(ListItem item in ddlTemplate.Items)
                    {
                        if (item.Value == template.Id.ToString())
                        {
                            item.Selected = true;
                            GetTemplateData( template.Id, false );
                        }
                        else
                        {
                            item.Selected = false;
                        }
                    }
                }
            }

            cbBulk.Checked = communication.IsBulkCommunication;

            ChannelControl control = LoadChannelControl( true );
            if ( control != null && CurrentPerson != null )
            {
                control.InitializeFromSender( CurrentPerson );
            }

            dtpFutureSend.SelectedDateTime = communication.FutureSendDateTime;

            ShowStatus( communication );
            ShowActions( communication );
        }

        /// <summary>
        /// Binds the channels.
        /// </summary>
        private void BindChannels()
        {
            var selectedGuids = new List<Guid>();
            GetAttributeValue( "Channels" ).SplitDelimitedValues()
                .ToList()
                .ForEach( v => selectedGuids.Add( v.AsGuid() ) );

            var channels = new Dictionary<int, string>();
            foreach ( var item in ChannelContainer.Instance.Components.Values )
            {
                if ( item.Value.IsActive &&
                    ( !selectedGuids.Any() || selectedGuids.Contains( item.Value.EntityType.Guid ) ) )
                {
                    var entityType = item.Value.EntityType;
                    channels.Add( entityType.Id, item.Metadata.ComponentName );
                    if ( !ChannelEntityTypeId.HasValue )
                    {
                        ChannelEntityTypeId = entityType.Id;
                    }
                }
            }

            LoadTemplates();

            divChannels.Visible = channels.Count() > 1;

            rptChannels.DataSource = channels;
            rptChannels.DataBind();
        }

        private void LoadTemplates()
        {
            bool visible = false;

            string prevSelection = ddlTemplate.SelectedValue;

            ddlTemplate.Items.Clear();
            ddlTemplate.Items.Add( new ListItem( string.Empty, string.Empty ) );

            if (ChannelEntityTypeId.HasValue)
            {
                foreach ( var template in new CommunicationTemplateService( new RockContext() ).Queryable()
                    .Where( t => t.ChannelEntityTypeId == ChannelEntityTypeId.Value )
                    .OrderBy( t => t.Name ) )
                {
                    if ( template.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        visible = true;
                        var li = new ListItem( template.Name, template.Id.ToString() );
                        li.Selected = template.Id.ToString() == prevSelection;
                        ddlTemplate.Items.Add( li );
                    }
                }
            }

            ddlTemplate.Visible = visible;
        }

        /// <summary>
        /// Binds the recipients.
        /// </summary>
        private void BindRecipients()
        {
            int recipientCount = Recipients.Count();
            lNumRecipients.Text = recipientCount.ToString( "N0" ) +
                (recipientCount == 1 ? " Person" : " People");

            ppAddPerson.PersonId = Rock.Constants.None.Id;
            ppAddPerson.PersonName = "Add Person";

            int displayCount = int.MaxValue;

            if ( !ShowAllRecipients )
            {
                int.TryParse( GetAttributeValue( "DisplayCount" ), out displayCount );
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

            lbRemoveAllRecipients.Visible = Recipients.Where( r => r.Status == CommunicationRecipientStatus.Pending).Any();

            rptRecipients.DataBind();

            StringBuilder rStatus = new StringBuilder();
                       
            CheckApprovalRequired( Recipients.Count );
        }

        /// <summary>
        /// Shows the channel.
        /// </summary>
        private ChannelControl LoadChannelControl(bool setData)
        {
            if ( setData )
            {
                phContent.Controls.Clear();
            }

            // The component to load control for
            ChannelComponent component = null;
            string channelName = string.Empty;

            // Get the current channel type
            EntityTypeCache entityType = null;
            if ( ChannelEntityTypeId.HasValue )
            {
                entityType = EntityTypeCache.Read( ChannelEntityTypeId.Value );
            }

            foreach ( var serviceEntry in ChannelContainer.Instance.Components )
            {
                var channelComponent = serviceEntry.Value;
    
                // Default to first component
                if ( component == null )
                {
                    component = channelComponent.Value;
                    channelName = channelComponent.Metadata.ComponentName + " ";
                }

                // If invalid entity type, exit (and use first component found)
                if ( entityType == null )
                {
                    break;
                }
                else if ( entityType.Id == channelComponent.Value.EntityType.Id )
                {
                    component = channelComponent.Value;
                    channelName = channelComponent.Metadata.ComponentName + " ";
                    break;
                }
            }

            if (component != null)
            {
                var channelControl = component.Control;
                channelControl.ID = "commControl";
                channelControl.IsTemplate = false;
                channelControl.AdditionalMergeFields = this.AdditionalMergeFields.ToList();
                channelControl.ValidationGroup = btnSubmit.ValidationGroup;
                phContent.Controls.Add( channelControl );

                if ( setData  )
                {
                    channelControl.ChannelData = ChannelData;
                }
                
                // Set the channel in case it wasn't already set or the previous component type was not found
                ChannelEntityTypeId = component.EntityType.Id;

                if (component.Transport == null || !component.Transport.IsActive)
                {
                    nbInvalidTransport.Text = string.Format( "The {0}channel does not have an active transport configured. The communication will not be delivered until the transport is configured correctly.", channelName );
                    nbInvalidTransport.Visible = true;
                }
                else
                {
                    nbInvalidTransport.Visible = false;
                }

                return channelControl;
            }

           return null;
        }

        /// <summary>
        /// Gets the channel data.
        /// </summary>
        private void GetChannelData()
        {
            if ( phContent.Controls.Count == 1 && phContent.Controls[0] is ChannelControl )
            {
                var channelData = ( (ChannelControl)phContent.Controls[0] ).ChannelData;
                foreach ( var dataItem in channelData )
                {
                    if ( ChannelData.ContainsKey( dataItem.Key ) )
                    {
                        ChannelData[dataItem.Key] = dataItem.Value;
                    }
                    else
                    {
                        ChannelData.Add( dataItem.Key, dataItem.Value );
                    }
                }
            }
        }

        private void GetTemplateData(int templateId, bool loadControl = true)
        {
            var template = new CommunicationTemplateService( new RockContext() ).Get( templateId );
            if ( template != null )
            {
                var channelData = template.ChannelData;
                if ( !channelData.ContainsKey( "Subject" ) )
                {
                    channelData.Add( "Subject", template.Subject );
                }

                foreach ( var dataItem in channelData )
                {
                    if ( !string.IsNullOrWhiteSpace( dataItem.Value ) )
                    {
                        if ( ChannelData.ContainsKey( dataItem.Key ) )
                        {
                            ChannelData[dataItem.Key] = dataItem.Value;
                        }
                        else
                        {
                            ChannelData.Add( dataItem.Key, dataItem.Value );
                        }
                    }
                }

                if ( loadControl )
                {
                    LoadChannelControl( true );
                }
            }
        }

        /// <summary>
        /// Shows the actions.
        /// </summary>
        /// <param name="communication">The communication.</param>
        private void ShowActions(Rock.Model.Communication communication)
        {
            // Determine if user is allowed to save changes, if not, disable 
            // submit and save buttons 
            if ( IsUserAuthorized( "Approve" ) ||
                CurrentPersonId == communication.SenderPersonId ||
                IsUserAuthorized( Authorization.EDIT ) )
            {
                btnSubmit.Enabled = true;
                btnSave.Enabled = true;
            }
            else
            {
                btnSubmit.Enabled = false;
                btnSave.Enabled = false;
            }
        }

        /// <summary>
        /// Determines whether approval is required, and sets the submit button text appropriately
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <returns>
        ///   <c>true</c> if [is approval required] [the specified communication]; otherwise, <c>false</c>.
        /// </returns>
        private bool CheckApprovalRequired(int numberOfRecipients)
        {
            int maxRecipients = int.MaxValue;
            int.TryParse( GetAttributeValue( "MaximumRecipients" ), out maxRecipients );
            bool approvalRequired = numberOfRecipients > maxRecipients;

            btnSubmit.Text = (approvalRequired && !IsUserAuthorized( "Approve" ) ? "Submit" : "Send" ) + " Communication";

            return approvalRequired;
        }

        /// <summary>
        /// Updates a communication model with the user-entered values
        /// </summary>
        /// <param name="communicationService">The service.</param>
        /// <returns></returns>
        private Rock.Model.Communication UpdateCommunication(RockContext rockContext)
        {
            var communicationService = new CommunicationService(rockContext);
            var recipientService = new CommunicationRecipientService(rockContext);

            Rock.Model.Communication communication = null;
            if ( CommunicationId.HasValue )
            {
                communication = communicationService.Get( CommunicationId.Value );
            }

            if (communication == null)
            {
                communication = new Rock.Model.Communication();
                communication.Status = CommunicationStatus.Transient;
                communication.SenderPersonId = CurrentPersonId;
                communicationService.Add( communication );
            }
            else
            {
                // Remove any deleted recipients
                foreach(var recipient in communication.Recipients.ToList())
                {
                    if (!Recipients.Any( r => r.PersonId == recipient.PersonId))
                    {
                        recipientService.Delete(recipient);
                        communication.Recipients.Remove( recipient );
                    }
                }
            }

            // Add any new recipients
            foreach(var recipient in Recipients )
            {
                if ( !communication.Recipients.Any( r => r.PersonId == recipient.PersonId ) )
                {
                    var communicationRecipient = new CommunicationRecipient();
                    communicationRecipient.Person = new PersonService( (RockContext)communicationService.Context ).Get( recipient.PersonId );
                    communication.Recipients.Add( communicationRecipient );
                }
            }

            communication.IsBulkCommunication = cbBulk.Checked;

            communication.ChannelEntityTypeId = ChannelEntityTypeId;
            communication.ChannelData.Clear();
            GetChannelData();
            foreach ( var keyVal in ChannelData )
            {
                if ( !string.IsNullOrEmpty( keyVal.Value ) )
                {
                    communication.ChannelData.Add( keyVal.Key, keyVal.Value );
                }
            }

            if ( communication.ChannelData.ContainsKey( "Subject" ) )
            {
                communication.Subject = communication.ChannelData["Subject"];
                communication.ChannelData.Remove( "Subject" );
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

            return communication;
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

            if ( CurrentPageReference.Parameters.ContainsKey( "CommunicationId" ) )
            {
                CurrentPageReference.Parameters["CommunicationId"] = communication.Id.ToString();
            }
            else
            {
                CurrentPageReference.Parameters.Add( "CommunicationId", communication.Id.ToString() );
            }
            hlViewCommunication.NavigateUrl = CurrentPageReference.BuildUrl();

            pnlResult.Visible = true;

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
            /// Gets or sets a value indicating whether [has SMS number].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [has SMS number]; otherwise, <c>false</c>.
            /// </value>
            public bool HasSmsNumber { get; set; }

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
            public Recipient( Person person, CommunicationRecipientStatus status, string statusNote = "", string openedClient = "", DateTime? openedDateTime = null )
            {
                PersonId = person.Id;
                PersonName = person.FullName;
                HasSmsNumber = person.PhoneNumbers.Any( p => p.IsMessagingEnabled );
                Email = person.Email;
                IsEmailActive = person.IsEmailActive ?? true;
                EmailNote = person.EmailNote;
                EmailPreference = person.EmailPreference;
                Status = status;
                StatusNote = statusNote;
                OpenedClient = openedClient;
                OpenedDateTime = openedDateTime;
            }

            public static string PreferenceMessage( Recipient recipient )
            {
                switch( recipient.EmailPreference )
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
