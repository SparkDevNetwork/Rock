//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using System.Xml.Xsl;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;

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
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( Page.IsPostBack )
            {
                LoadChannelControl();
            }
            else
            {
                string itemId = PageParameter( "Id" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "communicationId", int.Parse( itemId ) );
                }
                else
                {
                    ShowDetail( "communicationId", 0 );
                }
            }
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
            var linkButton = sender as LinkButton;
            if ( linkButton != null )
            {
                int channelId = int.MinValue;
                if ( int.TryParse( linkButton.CommandArgument, out channelId ) )
                {
                    hfChannelId.Value = channelId.ToString();
                    LoadChannelControl();
                    BindChannels();
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
            var service = new CommunicationService();
            var communication = GetCommunication( service );
            if ( communication != null )
            {
                var prevStatus = communication.Status;
                if ( RequireApproval( communication ) && !IsUserAuthorized( "Approve" ) )
                {
                    communication.Status = CommunicationStatus.Submitted;
                }
                else
                {
                    communication.Status = CommunicationStatus.Approved;
                    communication.ReviewedDateTime = DateTime.Now;
                    communication.ReviewerPersonId = CurrentPersonId;
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
                ProcessStatusChange( prevStatus, communication );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnApprove control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnApprove_Click( object sender, EventArgs e )
        {
            var service = new CommunicationService();
            var communication = GetCommunication( service );
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
                ProcessStatusChange( prevStatus, communication );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDeny control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDeny_Click( object sender, EventArgs e )
        {
            var service = new CommunicationService();
            var communication = GetCommunication( service );
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
                ProcessStatusChange( prevStatus, communication );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var service = new CommunicationService();
            var communication = GetCommunication( service );
            if ( communication != null )
            {
                var prevStatus = communication.Status;
                if ( communication.Status == CommunicationStatus.Transient )
                {
                    communication.Status = CommunicationStatus.Draft;
                }
                service.Save( communication, CurrentPersonId );
                ProcessStatusChange( prevStatus, communication );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            var service = new CommunicationService();
            var communication = GetCommunication(service);
            if ( communication != null )
            {
                var prevStatus = communication.Status;

                communication.Recipients
                    .Where( r => r.Status == CommunicationRecipientStatus.Pending )
                    .ToList()
                    .ForEach( r => r.Status = CommunicationRecipientStatus.Cancelled );

                service.Save( communication, CurrentPersonId );

                communication = service.Get( communication.Id );

                if ( !communication.Recipients
                    .Where( r => r.Status == CommunicationRecipientStatus.Success )
                    .Any() )
                {
                    communication.Status = CommunicationStatus.Draft;
                }

                service.Save( communication, CurrentPersonId );
                ProcessStatusChange( prevStatus, communication );
            }

        }

        /// <summary>
        /// Handles the Click event of the btnCopy control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCopy_Click( object sender, EventArgs e )
        {
            var service = new CommunicationService();
            var communication = GetCommunication(service);
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
                
                if (CurrentPageReference.Parameters.ContainsKey("Id"))
                {
                    CurrentPageReference.Parameters["Id"] = newCommunication.Id.ToString();
                }
                else
                {
                    CurrentPageReference.Parameters.Add( "Id", newCommunication.Id.ToString() );
                }

                Response.Redirect( CurrentPageReference.BuildUrl() );
                Context.ApplicationInstance.CompleteRequest();
            }

        }

        #endregion

        #region Private Methods

        private void ShowDetail( string itemKey, int itemKeyValue )
        {
            if ( !itemKey.Equals( "communicationId" ) )
            {
                return;
            }

            Rock.Model.Communication communication = null;

            if ( !itemKeyValue.Equals( 0 ) )
            {
                communication = new CommunicationService().Get( itemKeyValue );
            }
            else
            {
                communication = new Rock.Model.Communication { Id = 0, Status = CommunicationStatus.Transient };
            }

            if ( communication == null )
            {
                return;
            }

            hfCommunicationId.Value = communication.Id.ToString();
            hfChannelId.Value = communication.ChannelEntityTypeId.HasValue ? communication.ChannelEntityTypeId.Value.ToString() : "0";
            BindRecipients( communication );
            LoadChannelControl(communication);
            BindChannels();
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
                }
            }

            rptChannels.DataSource = channels;
            rptChannels.DataBind();
        }

        /// <summary>
        /// Binds the recipients.
        /// </summary>
        /// <param name="communication">The communication.</param>
        private void BindRecipients(Rock.Model.Communication communication)
        {
            int displayCount = int.MaxValue;
            int.TryParse(GetAttributeValue("DisplayCount"), out displayCount);
            if ( displayCount < int.MaxValue && displayCount > 0 )
            {
                rptRecipients.DataSource = communication.Recipients.Take( displayCount ).ToList();
            }
            else
            {
                rptRecipients.DataSource = communication.Recipients.ToList();
            }

            rptRecipients.DataBind();
        }

        /// <summary>
        /// Shows the channel.
        /// </summary>
        private void LoadChannelControl( Rock.Model.Communication communication = null )
        {
            phContent.Controls.Clear();

            int channelId = hfChannelId.ValueAsInt();

            // The component to load control for
            ChannelComponent component = null;

            var entityType = EntityTypeCache.Read( channelId );
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
                var control = phContent.LoadControl( component.ControlPath );
                phContent.Controls.Add( control );

                if ( control is CommunicationChannelControl && communication != null )
                {
                    ( (CommunicationChannelControl)control ).SetControlProperties( communication );
                }

                // Set the channel in case it wasn't already set or the previous component type was not found
                hfChannelId.Value = component.EntityType.Id.ToString();
            }
        }

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
            btnSubmit.Text = ( RequireApproval(communication) && !canApprove ? "Submit" : "Send" ) + " Communication";

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

        private bool RequireApproval(Rock.Model.Communication communication)
        {
            int maxRecipients = int.MaxValue;
            int.TryParse( GetAttributeValue( "MaximumRecipients" ), out maxRecipients );
            return communication.Recipients.Count() > maxRecipients;
        }

        private Rock.Model.Communication GetCommunication(CommunicationService service)
        {
            Rock.Model.Communication communication;

            int communicationId = int.MinValue;
            if ( int.TryParse( hfCommunicationId.Value, out communicationId ) )
            {
                if ( communicationId != int.MinValue )
                {
                    communication = service.Get(communicationId);
                }
                else
                {
                    communication = new Rock.Model.Communication();
                    service.Add(communication, CurrentPersonId);
                }

                int channelId = int.MinValue;
                if ( int.TryParse( hfChannelId.Value, out channelId ) )
                {
                    communication.ChannelEntityTypeId = channelId;

                    if ( phContent.Controls.Count == 1 && phContent.Controls[0] is CommunicationChannelControl )
                    {
                        ( (CommunicationChannelControl)phContent.Controls[0] ).GetControlProperties( communication );
                    }

                    return communication;
                }
            }

            return null;
        }

        private void ProcessStatusChange( CommunicationStatus previousStatus, Rock.Model.Communication communication )
        {
            if ( communication.Status == previousStatus )
            {
                return;
            }

            switch ( communication.Status )
            {
                case CommunicationStatus.Submitted:

                    // Send Notifiction to approvers...

                    break;

                case CommunicationStatus.Approved:

                    // Send notice to sender that communication was approved

                    bool sendNow = false;
                    if ( bool.TryParse( GetAttributeValue( "SendWhenApproved" ), out sendNow ) && sendNow )
                    {
                        var channel = communication.Channel;
                        if ( channel != null )
                        {
                            var transport = channel.Transport;
                            if ( transport != null )
                            {
                                transport.Send( communication, CurrentPersonId );
                            }
                        }
                    }

                    break;

                case CommunicationStatus.Denied:

                    // Send notice to sender that communication was denied

                    break;
            }

            ShowDetail( "communicationId", communication.Id );
        }

        #endregion

    }
}
