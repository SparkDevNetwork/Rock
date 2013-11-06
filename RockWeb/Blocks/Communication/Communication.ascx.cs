//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
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

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// User control for creating a new communication
    /// </summary>
    [AdditionalActions( new string[] { "Approve" } )]
    [BooleanField( "Send When Approved", "Should communication be sent once it's approved (vs. just being queued for scheduled job to send)?")]
    [IntegerField( "Maximum Recipients", "The maximum number of recipients allowed before communication will need to be approved" )]
    [IntegerField( "Display Count", "The initial number of recipients to display prior to expanding list" )]
    public partial class Communication : RockBlock
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

        private string PageTitle
        {
            get { return ViewState["PageTitle"] as string ?? "a New Communication"; }
            set { ViewState["PageTitle"] = value; }
        }

        #endregion

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            string script = @"
    $('a.remove-all-recipients').click(function(){
        return confirm('Are you sure you want to remove all of the pending recipients from this communication?');
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

                string itemId = PageParameter( "CommunicationId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "CommunicationId", int.Parse( itemId ) );
                }
                else
                {
                    ShowDetail( "CommunicationId", 0 );
                }
            }

            BreadCrumbs.Add( new BreadCrumb( PageTitle, true ) );
        }

        #endregion

        #region Events

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
                    var Person = new PersonService().Get( ppAddPerson.PersonId.Value );
                    if ( Person != null )
                    {
                        Recipients.Add( new Recipient( Person.Id, Person.FullName, CommunicationRecipientStatus.Pending ) );
                        ShowAllRecipients = true;
                        BindRecipients();
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
                if ( recipient != null && recipient.Status != CommunicationRecipientStatus.Pending )
                {
                    var lbRemove = e.Item.FindControl( "lbRemoveRecipient" ) as LinkButton;
                    if ( lbRemove != null )
                    {
                        lbRemove.Visible = false;
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
                BindRecipients();
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
            BindRecipients();
        }

        /// <summary>
        /// Handles the Click event of the lbRemoveAllRecipients control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbRemoveAllRecipients_Click( object sender, EventArgs e )
        {
            Recipients = Recipients.Where( r => r.Status != CommunicationRecipientStatus.Pending).ToList();
            BindRecipients();
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
                using ( new UnitOfWorkScope() )
                {
                    var service = new CommunicationService();
                    var communication = UpdateCommunication( service );

                    if ( communication != null )
                    {
                        string message = string.Empty;

                        var prevStatus = communication.Status;
                        if ( CheckApprovalRequired( communication.Recipients.Count ) && !IsUserAuthorized( "Approve" ) )
                        {
                            communication.Status = CommunicationStatus.Submitted;
                            message = "Communication has been submitted for approval.";
                        }
                        else
                        {
                            communication.Status = CommunicationStatus.Approved;
                            communication.ReviewedDateTime = DateTime.Now;
                            communication.ReviewerPersonId = CurrentPersonId;
                            message = "Communication has been queued for sending.";

                            // TODO: Send notice to sender that communication was approved
                        }

                        communication.Recipients
                            .Where( r =>
                                r.Status == CommunicationRecipientStatus.Cancelled ||
                                r.Status == CommunicationRecipientStatus.Failed )
                            .ToList()
                            .ForEach( r =>
                            {
                                r.Status = CommunicationRecipientStatus.Pending;
                                r.StatusNote = string.Empty;
                            }
                        );

                        service.Save( communication, CurrentPersonId );

                        if ( communication.Status == CommunicationStatus.Approved )
                        {
                            bool sendNow = false;
                            if ( bool.TryParse( GetAttributeValue( "SendWhenApproved" ), out sendNow ) && sendNow )
                            {
                                var transaction = new Rock.Transactions.SendCommunicationTransaction();
                                transaction.CommunicationId = communication.Id;
                                transaction.PersonId = CurrentPersonId;
                                Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );
                            }
                        }

                        ShowResult( message, communication );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnApprove control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnApprove_Click( object sender, EventArgs e )
        {
            if ( Page.IsValid )
            {
                using ( new UnitOfWorkScope() )
                {
                    var service = new CommunicationService();
                    var communication = UpdateCommunication( service );

                    if ( communication != null )
                    {
                        var prevStatus = communication.Status;
                        if ( IsUserAuthorized( "Approve" ) )
                        {
                            communication.Status = CommunicationStatus.Approved;
                            communication.ReviewedDateTime = DateTime.Now;
                            communication.ReviewerPersonId = CurrentPersonId;
                        }

                        service.Save( communication, CurrentPersonId );

                        ShowResult( "The communication has been approved", communication );
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
            if ( Page.IsValid )
            {
                using ( new UnitOfWorkScope() )
                {
                    var service = new CommunicationService();
                    var communication = UpdateCommunication( service );

                    if ( communication != null )
                    {
                        var prevStatus = communication.Status;
                        if ( IsUserAuthorized( "Approve" ) )
                        {
                            communication.Status = CommunicationStatus.Denied;
                            communication.ReviewedDateTime = DateTime.Now;
                            communication.ReviewerPersonId = CurrentPersonId;
                        }

                        service.Save( communication, CurrentPersonId );

                        // TODO: Send notice to sneder that communication was denied
                        
                        ShowResult( "The communicaiton has been denied", communication );
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
            if ( Page.IsValid )
            {
                using ( new UnitOfWorkScope() )
                {
                    var service = new CommunicationService();
                    var communication = UpdateCommunication( service );

                    if ( communication != null )
                    {
                        var prevStatus = communication.Status;
                        if ( communication.Status == CommunicationStatus.Transient )
                        {
                            communication.Status = CommunicationStatus.Draft;
                        }

                        service.Save( communication, CurrentPersonId );

                        ShowResult( "The communication has been saved", communication );
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
            if ( Page.IsValid )
            {
                if ( CommunicationId.HasValue )
                {
                    var service = new CommunicationService();
                    var communication = service.Get( CommunicationId.Value );
                    if ( communication != null )
                    {
                        var prevStatus = communication.Status;

                        communication.Recipients
                            .Where( r => r.Status == CommunicationRecipientStatus.Pending )
                            .ToList()
                            .ForEach( r => r.Status = CommunicationRecipientStatus.Cancelled );

                        // Save and re-read communication to reload recipient statuses
                        service.Save( communication, CurrentPersonId );
                        communication = service.Get( communication.Id );

                        if ( !communication.Recipients
                            .Where( r => r.Status == CommunicationRecipientStatus.Success )
                            .Any() )
                        {
                            communication.Status = CommunicationStatus.Draft;
                        }

                        ShowResult( "The communication has been cancelled", communication );
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
            using ( new UnitOfWorkScope() )
            {
                var service = new CommunicationService();
                var communication = UpdateCommunication( service );
                if ( communication != null )
                {
                    var newCommunication = communication.Clone( false );
                    newCommunication.Id = 0;
                    newCommunication.Guid = Guid.Empty;
                    newCommunication.SenderPersonId = CurrentPersonId;
                    newCommunication.Status = CommunicationStatus.Transient;
                    newCommunication.ReviewerPersonId = null;
                    newCommunication.ReviewedDateTime = null;
                    newCommunication.ReviewerNote = string.Empty;

                    communication.Recipients.ToList().ForEach( r =>
                        newCommunication.Recipients.Add( new CommunicationRecipient()
                        {
                            PersonId = r.PersonId,
                            Status = CommunicationRecipientStatus.Pending,
                            StatusNote = string.Empty
                        } ) );

                    service.Add( newCommunication, CurrentPersonId );
                    service.Save( newCommunication, CurrentPersonId );

                    // Redirect to new communication
                    if ( CurrentPageReference.Parameters.ContainsKey( "CommunicationId" ) )
                    {
                        CurrentPageReference.Parameters["CommunicationId"] = newCommunication.Id.ToString();
                    }
                    else
                    {
                        CurrentPageReference.Parameters.Add( "CommunicationId", newCommunication.Id.ToString() );
                    }
                    Response.Redirect( CurrentPageReference.BuildUrl() );
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        private void ShowDetail( string itemKey, int itemKeyValue )
        {
            if ( !itemKey.Equals( "CommunicationId" ) )
            {
                return;
            }

            Rock.Model.Communication communication = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                communication = new CommunicationService().Get( itemKeyValue );
                this.Page.Title = string.Format( "Communication #{0}", communication.Id );
                this.AdditionalMergeFields = communication.AdditionalMergeFields.ToList();
            }
            else
            {
                communication = new Rock.Model.Communication() { Status = CommunicationStatus.Transient };
                this.Page.Title = "New Communication";
            }

            PageTitle = this.Page.Title;

            if ( communication == null )
            {
                return;
            }

            ShowDetail( communication );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="communication">The communication.</param>
        private void ShowDetail (Rock.Model.Communication communication)
        {
            CommunicationId = communication.Id;

            pnlStatus.Visible = communication.Status != CommunicationStatus.Transient;
            lStatus.Text = string.Format("Communication Status: {0}", communication.Status.ConvertToString());

            ChannelEntityTypeId = communication.ChannelEntityTypeId;
            BindChannels();

            Recipients.Clear();
            communication.Recipients.ToList().ForEach( r => Recipients.Add( new Recipient( r.Person.Id, r.Person.FullName, r.Status ) ) );
            BindRecipients();

            ChannelData = communication.ChannelData;
            ChannelData.Add( "Subject", communication.Subject );

            ChannelControl control = LoadChannelControl( true );
            if ( control != null && CurrentPerson != null )
            {
                control.InitializeFromSender( CurrentPerson );
            }

            dtpFutureSend.SelectedDateTime = communication.FutureSendDateTime;

            ShowActions( communication );
        }

        /// <summary>
        /// Binds the channels.
        /// </summary>
        private void BindChannels()
        {
            var channels = new Dictionary<int, string>();
            foreach ( var item in ChannelContainer.Instance.Components.Values )
            {
                if ( item.Value.IsActive )
                {
                    var entityType = item.Value.EntityType;
                    channels.Add( entityType.Id, entityType.FriendlyName );
                    if ( !ChannelEntityTypeId.HasValue )
                    {
                        ChannelEntityTypeId = entityType.Id;
                    }
                }
            }

            rptChannels.DataSource = channels;
            rptChannels.DataBind();
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

            lRecipientStatus.Text = "Recipient Status: " + Recipients
                .GroupBy( r => r.Status )
                .Where( g => g.Count() > 0 )
                .Select( g => g.Key.ToString() + " (" + g.Count().ToString( "N0" ) + ")" )
                .ToList().AsDelimited( ", " );
                        
            CheckApprovalRequired( Recipients.Count );
        }

        /// <summary>
        /// Shows the channel.
        /// </summary>
        private ChannelControl LoadChannelControl(bool setData)
        {
            phContent.Controls.Clear();

            // The component to load control for
            ChannelComponent component = null;

            // Get the current channel type
            EntityTypeCache entityType = null;
            if ( ChannelEntityTypeId.HasValue )
            {
                entityType = EntityTypeCache.Read( ChannelEntityTypeId.Value );
            }

            foreach ( var serviceEntry in ChannelContainer.Instance.Components )
            {
                var channelComponent = serviceEntry.Value.Value;

                // Default to first component
                if ( component == null )
                {
                    component = channelComponent;
                }

                // If invalid entity type, exit (and use first component found)
                if ( entityType == null )
                {
                    break;
                }
                else if ( entityType.Id == channelComponent.EntityType.Id )
                {
                    component = channelComponent;
                    break;
                }
            }

            if (component != null)
            {
                phContent.Controls.Clear();
                var channelControl = component.Control;
                channelControl.ID = "commControl";
                channelControl.AdditionalMergeFields = this.AdditionalMergeFields.ToList();
                phContent.Controls.Add( channelControl );

                if ( setData  )
                {
                    channelControl.ChannelData = ChannelData;
                }
                
                // Set the channel in case it wasn't already set or the previous component type was not found
                ChannelEntityTypeId = component.EntityType.Id;

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

        /// <summary>
        /// Shows the actions.
        /// </summary>
        /// <param name="communication">The communication.</param>
        private void ShowActions(Rock.Model.Communication communication)
        {
            bool canApprove = IsUserAuthorized( "Approve" );

            // Set default visibility
            btnSubmit.Visible = false;
            btnApprove.Visible = false;
            btnDeny.Visible = false;
            btnSave.Visible = false;
            btnCancel.Visible = false;
            btnCopy.Visible = false;
            
            // Determine if user is allowed to save changes, if not, disable 
            // submit and save buttons (they won't see the approve/deny buttons)
            if ( canApprove ||
                CurrentPersonId == communication.SenderPersonId ||
                IsUserAuthorized( "Edit" ) )
            {
                btnSubmit.Enabled = true;
                btnSave.Enabled = true;
            }
            else
            {
                btnSubmit.Enabled = false;
                btnSave.Enabled = false;
            }

            // Determine if communication requires approval
            CheckApprovalRequired( communication.Recipients.Count );

            switch(communication.Status)
            {
                case CommunicationStatus.Transient:

                    btnSubmit.Visible = true;
                    btnSave.Visible = true;

                    break;

                case CommunicationStatus.Draft:
                    
                    btnSubmit.Visible = true;
                    btnSave.Visible = true;
                    btnCopy.Visible = true;
                    break;
                
                case CommunicationStatus.Submitted:


                    if ( canApprove )
                    {
                        btnApprove.Visible = true;
                        btnDeny.Visible = true;
                        btnSave.Visible = true;
                    }

                    btnCopy.Visible = true;

                    break;
                
                case CommunicationStatus.Approved:

                    // If there are still any pending recipients, allow canceling of send
                    btnCancel.Visible = communication.Recipients
                        .Where( r => r.Status == CommunicationRecipientStatus.Pending )
                        .Any();

                    btnCopy.Visible = true;

                    break;

                case CommunicationStatus.Denied:

                    if ( canApprove )
                    {
                        btnApprove.Visible = true;
                        btnSave.Visible = true;
                    }

                    btnCopy.Visible = true;

                    break;

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
        /// <param name="service">The service.</param>
        /// <returns></returns>
        private Rock.Model.Communication UpdateCommunication(CommunicationService service)
        {
            Rock.Model.Communication communication = null;
            if ( CommunicationId.HasValue )
            {
                communication = service.Get( CommunicationId.Value );
            }

            if (communication == null)
            {
                communication = new Rock.Model.Communication();
                communication.Status = CommunicationStatus.Transient;
                communication.SenderPersonId = CurrentPersonId;
                service.Add( communication, CurrentPersonId );
            }

            communication.ChannelEntityTypeId = ChannelEntityTypeId;

            foreach(var recipient in Recipients)
            {
                if ( !communication.Recipients.Where( r => r.PersonId == recipient.PersonId ).Any() )
                {
                    var communicationRecipient = new CommunicationRecipient();
                    communicationRecipient.Person = new PersonService().Get( recipient.PersonId );
                    communicationRecipient.Status = CommunicationRecipientStatus.Pending;
                    communication.Recipients.Add( communicationRecipient );
                }
            }

            GetChannelData();
            communication.ChannelData = ChannelData;
            if ( communication.ChannelData.ContainsKey( "Subject" ) )
            {
                communication.Subject = communication.ChannelData["Subject"];
                communication.ChannelData.Remove( "Subject" );
            }

            communication.FutureSendDateTime = dtpFutureSend.SelectedDateTime;

            return communication;
        }

        /// <summary>
        /// Shows the result.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="communication">The communication.</param>
        private void ShowResult( string message, Rock.Model.Communication communication )
        {
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
            /// Gets or sets the status.
            /// </summary>
            /// <value>
            /// The status.
            /// </value>
            public CommunicationRecipientStatus Status { get; set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Recipient" /> class.
            /// </summary>
            /// <param name="personId">The person id.</param>
            /// <param name="personName">Name of the person.</param>
            /// <param name="status">The status.</param>
            public Recipient( int personId, string personName, CommunicationRecipientStatus status )
            {
                PersonId = personId;
                PersonName = personName;
                Status = status;
            }
        }

        #endregion

    }
}
