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
    /// User control for creating a new communication.  This block should be used on same page as the NewCommunication block and only visible when editing an existing non-transient communication
    /// </summary>
    [DisplayName( "Communication Status" )]
    [Category( "Communication" )]
    [Description( "Used for displaying status of a communication that has already been created." )]

    [SecurityAction( Authorization.APPROVE, "The roles and/or users that have access to approve new communications." )]

    public partial class CommunicationDetail : RockBlock
    {

        #region Fields

        private Rock.Model.Communication _communication = null;

        #endregion

        #region Properties

        protected int? CommunicationId
        {
            get { return ViewState["CommunicationId"] as int?; }
            set { ViewState["CommunicationId"] = value; }
        }

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                CommunicationId = PageParameter( "CommunicationId" ).AsInteger( false );
            }

            // Check if communication was loaded by GetBreadCrumbs()
            var communication = RockPage.GetSharedItem( "Communication" ) as Rock.Model.Communication;
            if ( communication != null && CommunicationId.HasValue && communication.Id == CommunicationId.Value)
            {
                _communication = communication;
            }

            if (_communication == null && CommunicationId.HasValue)
            {
                _communication = new CommunicationService( new RockContext() ).Get( CommunicationId.Value );
            }

            if (!Page.IsPostBack)
            {
                if ( _communication == null ||
                    _communication.Status == CommunicationStatus.Transient ||
                    _communication.Status == CommunicationStatus.Draft )
                {
                    // If viewing a new, transient or draft communication, hide this block and use NewCommunication block
                    this.Visible = false;
                }
                else
                {
                    // Otherwise, use this block 
                    CommunicationId = _communication.Id;
                    ShowDetail();
                }

            }

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

            int? commId = PageParameter( "CommunicationId" ).AsInteger( false );
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
                    var prevStatus = communication.Status;
                    if ( IsUserAuthorized( "Approve" ) )
                    {
                        communication.Status = CommunicationStatus.Approved;
                        communication.ReviewedDateTime = RockDateTime.Now;
                        communication.ReviewerPersonId = CurrentPersonId;
                    }

                    rockContext.SaveChanges();

                    // TODO: Send notice to sneder that communication was approved

                    ShowResult( "The communication has been approved", communication );
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
                    var prevStatus = communication.Status;
                    if ( IsUserAuthorized( "Approve" ) )
                    {
                        communication.Status = CommunicationStatus.Denied;
                        communication.ReviewedDateTime = RockDateTime.Now;
                        communication.ReviewerPersonId = CurrentPersonId;
                    }

                    rockContext.SaveChanges();

                    // TODO: Send notice to sneder that communication was denied
                        
                    ShowResult( "The communicaiton has been denied", communication );
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
                    if ( !communication.Recipients
                        .Where( r => r.Status == CommunicationRecipientStatus.Delivered )
                        .Any() )
                    {
                        communication.Status = CommunicationStatus.Draft;
                    }
                    else
                    {
                        communication.Recipients
                            .Where( r => r.Status == CommunicationRecipientStatus.Pending )
                            .ToList()
                            .ForEach( r => r.Status = CommunicationRecipientStatus.Cancelled );
                    }

                    rockContext.SaveChanges();

                    ShowResult( "The communication has been cancelled", communication );
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
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        private void ShowDetail()
        {
            if ( _communication != null )
            {
                var rockContext = new RockContext();
                var service = new CommunicationService( rockContext );
                var communication = service
                    .Queryable( "Recipients" )
                    .Where( c => c.Id == CommunicationId.Value )
                    .FirstOrDefault();

                if ( communication != null )
                {
                    ShowStatus( communication );
                    lTitle.Text = ( communication.Subject ?? "Communication" ).FormatAsHtmlTitle();
                    lDetails.Text = communication.ChannelDataJson;

                    SetRecipientButton( aPending, lPending, communication.Recipients.Count( r => r.Status == CommunicationRecipientStatus.Pending ) );
                    SetRecipientButton( aDelivered, lDelivered, communication.Recipients.Count( r => r.Status == CommunicationRecipientStatus.Delivered ) );
                    SetRecipientButton( aFailed, lFailed, communication.Recipients.Count( r => r.Status == CommunicationRecipientStatus.Failed ) );
                    SetRecipientButton( aCanceled, lCancelled, communication.Recipients.Count( r => r.Status == CommunicationRecipientStatus.Cancelled ) );
                    SetRecipientButton( aOpened, lOpened, communication.Recipients.Count( r => r.Status == CommunicationRecipientStatus.Opened ) );

                    if (communication.ChannelEntityTypeId.HasValue)
                    {
                        var channelEntityType = EntityTypeCache.Read( communication.ChannelEntityTypeId.Value );
                        if (channelEntityType != null)
                        {
                            var channel = ChannelContainer.GetComponent( channelEntityType.Name );
                            if (channel != null)
                            {
                                lDetails.Text = channel.GetMessageDetails( communication );
                            }
                        } 
                    }
                }
            }

            ShowActions();
        }

        private void SetRecipientButton(HtmlAnchor htmlAnchor, Literal literalControl, int count)
        {
            if (count <= 0)
            {
                htmlAnchor.Attributes["disabled"] = "disabled";
            }
            else
            {
                htmlAnchor.Attributes.Remove( "disabled" );
            }

            literalControl.Text = count.ToString( "N0" );
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
        private void ShowActions()
        {
            bool canApprove = IsUserAuthorized( "Approve" );

            // Set default visibility
            btnApprove.Visible = false;
            btnDeny.Visible = false;
            btnCancel.Visible = false;
            btnCopy.Visible = false;
            
            if ( _communication != null )
            {
                switch ( _communication.Status )
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
                            btnCancel.Visible = _communication.Recipients
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
        private void ShowResult( string message, Rock.Model.Communication communication )
        {
            ShowStatus( communication );

            pnlDetails.Visible = false;

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

    }
}
