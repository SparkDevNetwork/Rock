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
using System.Web.UI.HtmlControls;
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
    /// Used for displaying details of an existing communication that has already been created.
    /// </summary>
    [DisplayName( "Communication Detail" )]
    [Category( "Communication" )]
    [Description( "Used for displaying details of an existing communication that has already been created." )]

    [SecurityAction( Authorization.APPROVE, "The roles and/or users that have access to approve new communications." )]

    public partial class CommunicationDetail : RockBlock
    {

        #region Properties

        protected int? CommunicationId
        {
            get { return ViewState["CommunicationId"] as int?; }
            set { ViewState["CommunicationId"] = value; }
        }

        #endregion

        #region Base Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gPending.DataKeyNames = new string[] { "Id" };
            gDelivered.DataKeyNames = new string[] { "Id" };
            gFailed.DataKeyNames = new string[] { "Id" };
            gCancelled.DataKeyNames = new string[] { "Id" };
            gOpened.DataKeyNames = new string[] { "Id" };

            gPending.GridRebind += gRecipients_GridRebind;
            gDelivered.GridRebind += gRecipients_GridRebind;
            gFailed.GridRebind += gRecipients_GridRebind;
            gCancelled.GridRebind += gRecipients_GridRebind;
            gOpened.GridRebind += gRecipients_GridRebind;

            gActivity.DataKeyNames = new string[] { "Id" };
            gActivity.GridRebind += gActivity_GridRebind;

            string script = string.Format( @"
    function showRecipients( recipientDiv, show )
    {{
        var $hf = $('#{0}');
        $('.js-communication-recipients').slideUp('slow');
        if ( $hf.val() != recipientDiv ) {{
            $('#' + recipientDiv).slideDown('slow');
            $hf.val(recipientDiv);
        }} else {{
            $hf.val('');
        }}
    }}

    $('#{1}').click( function() {{ showRecipients('{2}'); }});
    $('#{3}').click( function() {{ showRecipients('{4}'); }});
    $('#{5}').click( function() {{ showRecipients('{6}'); }});
    $('#{7}').click( function() {{ showRecipients('{8}'); }});
    $('#{9}').click( function() {{ showRecipients('{10}'); }});
",
    hfActiveRecipient.ClientID,
    aPending.ClientID, sPending.ClientID,
    aDelivered.ClientID, sDelivered.ClientID,
    aFailed.ClientID, sFailed.ClientID,
    aCancelled.ClientID, sCancelled.ClientID,
    aOpened.ClientID, sOpened.ClientID );

            ScriptManager.RegisterStartupScript( pnlDetails, pnlDetails.GetType(), "recipient-toggle-" + this.BlockId.ToString(), script, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                CommunicationId = PageParameter( "CommunicationId" ).AsIntegerOrNull();
                ShowDetail();
            }
        }

        protected override void OnPreRender( EventArgs e )
        {
            sPending.Style["display"] = hfActiveRecipient.Value == sPending.ClientID ? "block" : "none";
            sDelivered.Style["display"] = hfActiveRecipient.Value == sDelivered.ClientID ? "block" : "none";
            sFailed.Style["display"] = hfActiveRecipient.Value == sFailed.ClientID ? "block" : "none";
            sCancelled.Style["display"] = hfActiveRecipient.Value == sCancelled.ClientID ? "block" : "none";
            sOpened.Style["display"] = hfActiveRecipient.Value == sOpened.ClientID ? "block" : "none";
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

            string pageTitle = "New Communication";

            int? commId = PageParameter( "CommunicationId" ).AsIntegerOrNull();
            if ( commId.HasValue )
            {
                var communication = new CommunicationService( new RockContext() ).Get( commId.Value );
                if ( communication != null )
                {
                    RockPage.SaveSharedItem( "communication", communication );

                    switch ( communication.Status )
                    {
                        case CommunicationStatus.Approved:
                        case CommunicationStatus.Denied:
                        case CommunicationStatus.PendingApproval:
                            {
                                pageTitle = string.Format( "Communication #{0}", communication.Id );
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
        /// Handles the GridRebind event of the Recipient grid controls.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void gRecipients_GridRebind( object sender, EventArgs e )
        {
            BindRecipients();
        }

        void gActivity_GridRebind( object sender, EventArgs e )
        {
            BindActivity();
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
                var rockContext = new RockContext();
                var service = new CommunicationService( rockContext );
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

                            rockContext.SaveChanges();

                            // TODO: Send notice to sneder that communication was approved

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
                var rockContext = new RockContext();
                var service = new CommunicationService( rockContext );
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

                            rockContext.SaveChanges();

                            // TODO: Send notice to sneder that communication was denied

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
                var rockContext = new RockContext();
                var service = new CommunicationService( rockContext );
                var communication = service.Get( CommunicationId.Value );
                if ( communication != null )
                {
                    if (communication.Status == CommunicationStatus.Approved || communication.Status == CommunicationStatus.PendingApproval)
                    {
                        if ( !communication.Recipients
                            .Where( r => r.Status == CommunicationRecipientStatus.Delivered )
                            .Any() )
                        {
                            communication.Status = CommunicationStatus.Draft;
                            rockContext.SaveChanges();

                            ShowResult( "This communication has successfully been cancelled without any recipients receiving communication!", communication, NotificationBoxType.Success );
                        }
                        else
                        {
                            communication.Recipients
                                .Where( r => r.Status == CommunicationRecipientStatus.Pending )
                                .ToList()
                                .ForEach( r => r.Status = CommunicationRecipientStatus.Cancelled );
                            rockContext.SaveChanges();

                            int delivered = communication.Recipients.Count( r => r.Status == CommunicationRecipientStatus.Delivered );
                            ShowResult( string.Format("This communication has been cancelled, however the communication was delivered to {0} recipients!", delivered)
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
                var rockContext = new RockContext();
                var service = new CommunicationService( rockContext );
                var communication = service.Get( CommunicationId.Value );
                if ( communication != null )
                {
                    var newCommunication = communication.Clone( false );
                    newCommunication.CreatedByPersonAlias = null;
                    newCommunication.CreatedByPersonAliasId = null;
                    newCommunication.CreatedDateTime = RockDateTime.Now;
                    newCommunication.ModifiedByPersonAlias = null;
                    newCommunication.ModifiedByPersonAliasId = null;
                    newCommunication.ModifiedDateTime = RockDateTime.Now;
                    newCommunication.Id = 0;
                    newCommunication.Guid = Guid.Empty;
                    newCommunication.SenderPersonAliasId = CurrentPersonAliasId;
                    newCommunication.Status = CommunicationStatus.Draft;
                    newCommunication.ReviewerPersonAliasId = null;
                    newCommunication.ReviewedDateTime = null;
                    newCommunication.ReviewerNote = string.Empty;

                    communication.Recipients.ToList().ForEach( r =>
                        newCommunication.Recipients.Add( new CommunicationRecipient()
                        {
                            PersonAliasId = r.PersonAliasId,
                            Status = CommunicationRecipientStatus.Pending,
                            StatusNote = string.Empty
                        } ) );

                    service.Add( newCommunication );
                    rockContext.SaveChanges();

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
        private void ShowDetail()
        {
            Rock.Model.Communication communication = null;

            if (CommunicationId.HasValue)
            {
                communication = new CommunicationService(new RockContext())
                    .Queryable( "CreatedByPersonAlias.Person" )
                    .Where( c => c.Id == CommunicationId.Value)
                    .FirstOrDefault();
            }

            // If not valid for this block, hide contents and return
            if ( communication == null ||
                communication.Status == CommunicationStatus.Transient ||
                communication.Status == CommunicationStatus.Draft )
            {
                // If viewing a new, transient or draft communication, hide this block and use NewCommunication block
                this.Visible = false;
                return;
            }

            ShowStatus( communication );
            lTitle.Text = ( communication.Subject ?? "Communication" ).FormatAsHtmlTitle();

            SetPersonDateValue( lCreatedBy, communication.CreatedByPersonAlias, communication.CreatedDateTime, "Created By" );
            SetPersonDateValue( lApprovedBy, communication.ReviewerPersonAlias, communication.ReviewedDateTime, "Approved By" );

            if ( communication.FutureSendDateTime.HasValue && communication.FutureSendDateTime.Value > RockDateTime.Now )
            {
                lFutureSend.Text = String.Format( "<div class='alert alert-success'><strong>Future Send</strong> This communication is scheduled to be sent {0} <small>({1})</small>.</div>", communication.FutureSendDateTime.Value.ToRelativeDateString(), communication.FutureSendDateTime.Value.ToString() );
            }

            pnlOpened.Visible = false;

            lDetails.Text = communication.MediumDataJson;
            if ( communication.MediumEntityTypeId.HasValue )
            {
                var mediumEntityType = EntityTypeCache.Read( communication.MediumEntityTypeId.Value );
                if (mediumEntityType != null)
                {
                    var medium = MediumContainer.GetComponent( mediumEntityType.Name );
                    if (medium != null && medium.Transport != null)
                    {
                        pnlOpened.Visible = medium.Transport.CanTrackOpens;
                        lDetails.Text = medium.GetMessageDetails( communication );
                    }
                } 
            }

            BindRecipients();

            BindActivity();

            ShowActions( communication );
        }

        private void SetPersonDateValue( Literal literal, PersonAlias personAlias, DateTime? datetime, string labelText ) {

            if ( personAlias != null )
            {
                SetPersonDateValue( literal, personAlias.Person, datetime, labelText );
            }
        }

        private void SetPersonDateValue( Literal literal, Person person, DateTime? datetime, string labelText )
        {
            if ( person != null )
            {
                literal.Text = String.Format("<strong>{0}</strong> {1}", labelText, person.FullName);

                if ( datetime.HasValue )
                {
                    literal.Text += String.Format( " <small class='js-date-rollover' data-toggle='tooltip' data-placement='top' title='{0}'>({1})</small>", datetime.Value.ToString(), datetime.Value.ToRelativeDateString() );
                }
            }
        }

        private void BindRecipients()
        {
            if ( CommunicationId.HasValue )
            {
                var rockContext = new RockContext();
                var recipients = new CommunicationRecipientService( rockContext )
                    .Queryable( "PersonAlias.Person,Activities" )
                    .Where( r => r.CommunicationId == CommunicationId.Value )
                    .ToList();

                SetRecipients( pnlPending, aPending, lPending, gPending,
                    recipients.Where( r => r.Status == CommunicationRecipientStatus.Pending ).ToList() );
                SetRecipients( pnlDelivered, aDelivered, lDelivered, gDelivered,
                    recipients.Where( r => r.Status == CommunicationRecipientStatus.Delivered || r.Status == CommunicationRecipientStatus.Opened ).ToList() );
                SetRecipients( pnlFailed, aFailed, lFailed, gFailed,
                    recipients.Where( r => r.Status == CommunicationRecipientStatus.Failed ).ToList() );
                SetRecipients( pnlCancelled, aCancelled, lCancelled, gCancelled,
                    recipients.Where( r => r.Status == CommunicationRecipientStatus.Cancelled ).ToList() );

                if ( pnlOpened.Visible )
                {
                    SetRecipients( pnlOpened, aOpened, lOpened, gOpened,
                        recipients.Where( r => r.Status == CommunicationRecipientStatus.Opened ).ToList() );
                }
            }
        }

        private void SetRecipients( Panel pnl, HtmlAnchor htmlAnchor, Literal literalControl, 
            Grid grid, List<CommunicationRecipient> recipients )
        {
            pnl.CssClass = pnlOpened.Visible ? "col-md-2-10 margin-b-md" : "col-md-3 margin-b-md";

            int count = recipients.Count();

            if ( count <= 0 )
            {
                htmlAnchor.Attributes["disabled"] = "disabled";
            }
            else
            {
                htmlAnchor.Attributes.Remove( "disabled" );
            }

            literalControl.Text = count.ToString( "N0" );

            var sortProperty = grid.SortProperty;
            if ( sortProperty != null )
            {
                grid.DataSource = recipients.AsQueryable()
                    .Sort( sortProperty )
                    .ToList();
            }
            else
            {
                grid.DataSource = recipients
                    .OrderBy( r => r.PersonAlias.Person.LastName )
                    .ThenBy( r => r.PersonAlias.Person.NickName )
                    .ToList();
            }
            
            grid.DataBind();
        }

        private void BindActivity()
        {
            if ( CommunicationId.HasValue )
            {
                var rockContext = new RockContext();
                var activity = new CommunicationRecipientActivityService( rockContext )
                    .Queryable( "CommunicationRecipient.PersonAlias.Person" )
                    .Where( r => r.CommunicationRecipient.CommunicationId == CommunicationId.Value );

                var sortProperty = gActivity.SortProperty;
                if ( sortProperty != null )
                {
                    activity = activity.Sort( sortProperty );
                }
                else
                {
                    activity = activity.OrderBy( a => a.ActivityDateTime );
                }

                gActivity.DataSource = activity.ToList();
                gActivity.DataBind();
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
                        wpEvents.Expanded = false;
                        wpEvents.Expanded = true;

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
        /// Shows the actions.
        /// </summary>
        /// <param name="communication">The communication.</param>
        private void ShowActions(Rock.Model.Communication communication)
        {
            bool canApprove = IsUserAuthorized( "Approve" );

            // Set default visibility
            btnApprove.Visible = false;
            btnDeny.Visible = false;
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
                            // This block isn't used for transient, draft or denied communicaitons
                            break;
                        }
                    case CommunicationStatus.PendingApproval:
                        {
                            if ( canApprove )
                            {
                                btnApprove.Visible = true;
                                btnDeny.Visible = true;
                            }
                            btnCancel.Visible = true;
                            break;
                        }
                    case CommunicationStatus.Approved:
                        {
                            // If there are still any pending recipients, allow canceling of send
                            btnCancel.Visible = communication.Recipients
                                .Where( r => r.Status == CommunicationRecipientStatus.Pending )
                                .Any();

                            btnCopy.Visible = true;
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

            pnlDetails.Visible = false;

            nbResult.Text = message;
            nbResult.NotificationBoxType = notificationType;

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

    }
}
