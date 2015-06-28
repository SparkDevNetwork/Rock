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

    [ComponentsField( "Rock.Communication.MediumContainer, Rock", "Mediums", "The Mediums that should be available to user to send through (If none are selected, all active mediums will be available).", false, "", "", 0  )]
    [CommunicationTemplateField("Default Template", "The default template to use for a new communication.  (Note: This will only be used if the template is for the same medium as the communication.)", false, "", "", 1)]
    [IntegerField( "Maximum Recipients", "The maximum number of recipients allowed before communication will need to be approved", false, 0, "", 2 )]
    [IntegerField( "Display Count", "The initial number of recipients to display prior to expanding list", false, 0, "", 3  )]
    [BooleanField( "Send When Approved", "Should communication be sent once it's approved (vs. just being queued for scheduled job to send)?", true, "", 4 )]
    [CustomDropdownListField("Mode", "The mode to use ( 'Simple' mode will prevent uers from searching/adding new people to communication).", "Full,Simple", true, "Full", "", 5)]
    public partial class CommunicationEntry : RockBlock
    {

        #region Fields

        private bool _fullMode = true;

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
        /// <value>
        /// The medium entity type id.
        /// </value>
        protected int? MediumEntityTypeId
        {
            get { return ViewState["MediumEntityTypeId"] as int?; }
            set { ViewState["MediumEntityTypeId"] = value; }
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
        /// Gets or sets the medium data.
        /// </summary>
        /// <value>
        /// The medium data.
        /// </value>
        protected Dictionary<string, string> MediumData
        {
            get 
            {
                var mediumData = ViewState["MediumData"] as Dictionary<string, string>;
                if ( mediumData == null )
                {
                    mediumData = new Dictionary<string, string>();
                    ViewState["MediumData"] = mediumData;
                }
                return mediumData;
            }

            set { ViewState["MediumData"] = value; }
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

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string script = @"
    $('a.remove-all-recipients').click(function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to remove all of the pending recipients from this communication?', function (result) {
            if (result) {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
        });
    });
";
            ScriptManager.RegisterStartupScript(lbRemoveAllRecipients, lbRemoveAllRecipients.GetType(), "ConfirmRemoveAll", script, true );

            string mode = GetAttributeValue( "Mode" );
            _fullMode = string.IsNullOrWhiteSpace( mode ) || mode != "Simple";
            ppAddPerson.Visible = _fullMode;
            cbBulk.Visible = _fullMode;
            ddlTemplate.Visible = _fullMode;
            dtpFutureSend.Visible = _fullMode;
            btnTest.Visible = _fullMode;
            btnSave.Visible = _fullMode;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

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

                    LoadMediumControl( true );
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

                        if ( recipient.IsDeceased )
                        {
                            textClass = "text-danger";
                            textTooltip = "Deceased";
                        }
                        else
                        {
                            if ( MediumEntityTypeId == EntityTypeCache.Read( "Rock.Communication.Medium.Email" ).Id )
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
                                            var mediumData = MediumData;
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
                            else if ( MediumEntityTypeId == EntityTypeCache.Read( "Rock.Communication.Medium.Sms" ).Id )
                            {
                                if ( !recipient.HasSmsNumber )
                                {
                                    // No SMS Number
                                    textClass = "text-danger";
                                    textTooltip = "No phone number with SMS enabled.";
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
        protected void btnTest_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid && CurrentPersonAliasId.HasValue )
            {
                // Get existing or new communication record
                var communication = UpdateCommunication( new RockContext() );

                if ( communication != null  )
                {
                    // Using a new context (so that changes in the UpdateCommunication() are not persisted )
                    var testCommunication = new Rock.Model.Communication();
                    testCommunication.SenderPersonAliasId = communication.SenderPersonAliasId;
                    testCommunication.Subject = communication.Subject;
                    testCommunication.IsBulkCommunication = communication.IsBulkCommunication;
                    testCommunication.MediumEntityTypeId = communication.MediumEntityTypeId;
                    testCommunication.MediumDataJson = communication.MediumDataJson;
                    testCommunication.AdditionalMergeFieldsJson = communication.AdditionalMergeFieldsJson;

                    testCommunication.FutureSendDateTime = null;
                    testCommunication.Status = CommunicationStatus.Approved;
                    testCommunication.ReviewedDateTime = RockDateTime.Now;
                    testCommunication.ReviewerPersonAliasId = CurrentPersonAliasId;

                    var testRecipient = new CommunicationRecipient();
                    if (communication.Recipients.Any())
                    {
                        var recipient = communication.Recipients.FirstOrDefault();
                        testRecipient.AdditionalMergeValuesJson = recipient.AdditionalMergeValuesJson;
                    }
                    testRecipient.Status = CommunicationRecipientStatus.Pending;
                    testRecipient.PersonAliasId = CurrentPersonAliasId.Value;
                    testCommunication.Recipients.Add( testRecipient );

                    var rockContext = new RockContext();
                    var communicationService = new CommunicationService( rockContext );
                    communicationService.Add( testCommunication );
                    rockContext.SaveChanges();

                    var medium = testCommunication.Medium;
                    if ( medium != null )
                    {
                        medium.Send( testCommunication );
                    }

                    communicationService.Delete( testCommunication );
                    rockContext.SaveChanges();

                    nbTestResult.Visible = true;
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
                        communication.ReviewerPersonAliasId = CurrentPersonAliasId;

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

            if ( communication != null )
            {
                this.AdditionalMergeFields = communication.AdditionalMergeFields.ToList();
                lTitle.Text = ( communication.Subject ?? "New Communication" ).FormatAsHtmlTitle();

                foreach ( var recipient in new CommunicationRecipientService( new RockContext() )
                    .Queryable( "PersonAlias.Person.PhoneNumbers" )
                    .Where( r => r.CommunicationId == communication.Id ) )
                {
                    Recipients.Add( new Recipient( recipient.PersonAlias.Person, recipient.Status, recipient.StatusNote, recipient.OpenedClient, recipient.OpenedDateTime ) );
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

            MediumEntityTypeId = communication.MediumEntityTypeId;
            BindMediums();

            MediumData = communication.MediumData;
            MediumData.Add( "Subject", communication.Subject );

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

                if (template != null && template.MediumEntityTypeId == MediumEntityTypeId)
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

            MediumControl control = LoadMediumControl( true );
            if ( control != null && CurrentPerson != null )
            {
                control.InitializeFromSender( CurrentPerson );
            }

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
            GetAttributeValue( "Mediums" ).SplitDelimitedValues()
                .ToList()
                .ForEach( v => selectedGuids.Add( v.AsGuid() ) );

            var mediums = new Dictionary<int, string>();
            foreach ( var item in MediumContainer.Instance.Components.Values )
            {
                if ( item.Value.IsActive &&
                    ( !selectedGuids.Any() || selectedGuids.Contains( item.Value.EntityType.Guid ) ) )
                {
                    var entityType = item.Value.EntityType;
                    mediums.Add( entityType.Id, item.Metadata.ComponentName );
                    if ( !MediumEntityTypeId.HasValue )
                    {
                        MediumEntityTypeId = entityType.Id;
                    }
                }
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

            if (MediumEntityTypeId.HasValue)
            {
                foreach ( var template in new CommunicationTemplateService( new RockContext() ).Queryable()
                    .Where( t => t.MediumEntityTypeId == MediumEntityTypeId.Value )
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

            ddlTemplate.Visible = _fullMode && visible;
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
        /// Shows the medium.
        /// </summary>
        private MediumControl LoadMediumControl(bool setData)
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
                entityType = EntityTypeCache.Read( MediumEntityTypeId.Value );
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

            if (component != null)
            {
                var mediumControl = component.GetControl( !_fullMode );
                mediumControl.ID = "commControl";
                mediumControl.IsTemplate = false;
                mediumControl.AdditionalMergeFields = this.AdditionalMergeFields.ToList();
                mediumControl.ValidationGroup = btnSubmit.ValidationGroup;
                phContent.Controls.Add( mediumControl );

                if ( setData  )
                {
                    mediumControl.MediumData = MediumData;
                }
                
                // Set the medium in case it wasn't already set or the previous component type was not found
                MediumEntityTypeId = component.EntityType.Id;

                if (component.Transport == null || !component.Transport.IsActive)
                {
                    nbInvalidTransport.Text = string.Format( "The {0}medium does not have an active transport configured. The communication will not be delivered until the transport is configured correctly.", mediumName );
                    nbInvalidTransport.Visible = true;
                }
                else
                {
                    nbInvalidTransport.Visible = false;
                }

                cbBulk.Visible = _fullMode && component.SupportsBulkCommunication;

                return mediumControl;
            }

           return null;
        }

        /// <summary>
        /// Gets the medium data.
        /// </summary>
        private void GetMediumData()
        {
            if ( phContent.Controls.Count == 1 )
            {
                var mediumControl = phContent.Controls[0] as MediumControl;
                if ( mediumControl != null )
                {
                    // If using simple mode, the control should be re-initialized from sender since sender fields 
                    // are not presented for editing and user shouldn't be able to change them
                    if ( !_fullMode && CurrentPerson != null )
                    {
                        mediumControl.InitializeFromSender( CurrentPerson );
                    }

                    foreach ( var dataItem in mediumControl.MediumData )
                    {
                        if ( MediumData.ContainsKey( dataItem.Key ) )
                        {
                            MediumData[dataItem.Key] = dataItem.Value;
                        }
                        else
                        {
                            MediumData.Add( dataItem.Key, dataItem.Value );
                        }
                    }
                }
            }
        }

        private void GetTemplateData(int templateId, bool loadControl = true)
        {
            var template = new CommunicationTemplateService( new RockContext() ).Get( templateId );
            if ( template != null )
            {
                var mediumData = template.MediumData;
                if ( !mediumData.ContainsKey( "Subject" ) )
                {
                    mediumData.Add( "Subject", template.Subject );
                }

                foreach ( var dataItem in mediumData )
                {
                    if ( !string.IsNullOrWhiteSpace( dataItem.Value ) )
                    {
                        if ( MediumData.ContainsKey( dataItem.Key ) )
                        {
                            MediumData[dataItem.Key] = dataItem.Value;
                        }
                        else
                        {
                            MediumData.Add( dataItem.Key, dataItem.Value );
                        }
                    }
                }

                if ( loadControl )
                {
                    LoadMediumControl( true );
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
                ( CurrentPersonAliasId.HasValue && CurrentPersonAliasId == communication.SenderPersonAliasId ) ||
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

                // Remove any deleted recipients
                foreach(var recipient in recipientService.GetByCommunicationId( CommunicationId.Value ) )
                {
                    if (!Recipients.Any( r => recipient.PersonAlias != null && r.PersonId == recipient.PersonAlias.PersonId))
                    {
                        recipientService.Delete(recipient);
                        communication.Recipients.Remove( recipient );
                    }
                }
            }

            if (communication == null)
            {
                communication = new Rock.Model.Communication();
                communication.Status = CommunicationStatus.Transient;
                communication.SenderPersonAliasId = CurrentPersonAliasId;
                communicationService.Add( communication );
            }

            // Add any new recipients
            foreach(var recipient in Recipients )
            {
                if ( !communication.Recipients.Any( r => r.PersonAlias != null && r.PersonAlias.PersonId == recipient.PersonId ) )
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

            communication.IsBulkCommunication = cbBulk.Checked;

            communication.MediumEntityTypeId = MediumEntityTypeId;
            communication.MediumData.Clear();
            GetMediumData();
            foreach ( var keyVal in MediumData )
            {
                if ( !string.IsNullOrEmpty( keyVal.Value ) )
                {
                    communication.MediumData.Add( keyVal.Key, keyVal.Value );
                }
            }

            if ( communication.MediumData.ContainsKey( "Subject" ) )
            {
                communication.Subject = communication.MediumData["Subject"];
                communication.MediumData.Remove( "Subject" );
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

            CurrentPageReference.Parameters.AddOrReplace( "CommunicationId", communication.Id.ToString() );
            hlViewCommunication.NavigateUrl = CurrentPageReference.BuildUrl();
            hlViewCommunication.Visible = this.Page.ControlsOfTypeRecursive<RockWeb.Blocks.Communication.CommunicationDetail>().Any();

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
                IsDeceased = person.IsDeceased;
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
