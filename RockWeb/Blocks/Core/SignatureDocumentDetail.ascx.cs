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
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;

using Rock.Model;
using Rock.Web;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Security;
using System.Data.Entity;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// User controls for managing signature documents
    /// </summary>
    [DisplayName( "Signature Document Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of a given signature document." )]

    public partial class SignatureDocumentDetail : RockBlock, IDetailBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlSettings );

            rbStatus.BindToEnum<SignatureDocumentStatus>();

            ddlDocumentType.Items.Clear();
            using ( var rockContext = new RockContext() )
            {
                ddlDocumentType.DataSource = new SignatureDocumentTemplateService( rockContext )
                    .Queryable().AsNoTracking()
                    .OrderBy( t => t.Name )
                    .Select( t => new
                    {
                        t.Id,
                        t.Name
                    } )
                    .ToList();
                ddlDocumentType.DataBind();
                ddlDocumentType.Items.Insert( 0, new ListItem( "", "0" ) );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbSend.Visible = false;

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "signatureDocumentId" ).AsInteger() );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( PageParameter( "signatureDocumentId" ).AsInteger() );
        }

        /// <summary>
        /// Handles the Click event of the btnSaveType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            SignatureDocument signatureDocument = null;
            SignatureDocumentService service = new SignatureDocumentService( rockContext );

            int signatureDocumentId = hfSignatureDocumentId.ValueAsInt();

            int? origBinaryFileId = null;

            if ( signatureDocumentId != 0 )
            {
                signatureDocument = service.Get( signatureDocumentId );
            }

            if ( signatureDocument == null )
            { 
                signatureDocument = new SignatureDocument();
                service.Add( signatureDocument );
            }

            signatureDocument.Name = tbName.Text;

            var newStatus = rbStatus.SelectedValueAsEnum<SignatureDocumentStatus>( SignatureDocumentStatus.None );
            if ( signatureDocument.Status != newStatus )
            {
                signatureDocument.Status = newStatus;
                signatureDocument.LastStatusDate = RockDateTime.Now;
            }

            signatureDocument.AppliesToPersonAliasId = ppAppliesTo.PersonAliasId;
            signatureDocument.AssignedToPersonAliasId = ppAssignedTo.PersonAliasId;
            signatureDocument.SignedByPersonAliasId = ppSignedBy.PersonAliasId;

            signatureDocument.SignatureDocumentTemplateId = ddlDocumentType.SelectedValueAsInt() ?? 0;

            origBinaryFileId = signatureDocument.BinaryFileId;
            signatureDocument.BinaryFileId = fuDocument.BinaryFileId;

            if ( !signatureDocument.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            BinaryFileService binaryFileService = new BinaryFileService( rockContext );
            if ( origBinaryFileId.HasValue && origBinaryFileId.Value != signatureDocument.BinaryFileId )
            {
                // if a new the binaryFile was uploaded, mark the old one as Temporary so that it gets cleaned up
                var oldBinaryFile = binaryFileService.Get( origBinaryFileId.Value );
                if ( oldBinaryFile != null && !oldBinaryFile.IsTemporary )
                {
                    oldBinaryFile.IsTemporary = true;
                }
            }

            // ensure the IsTemporary is set to false on binaryFile associated with this document
            if ( signatureDocument.BinaryFileId.HasValue )
            {
                var binaryFile = binaryFileService.Get( signatureDocument.BinaryFileId.Value );
                if ( binaryFile != null && binaryFile.IsTemporary )
                {
                    binaryFile.IsTemporary = false;
                }
            }

            rockContext.SaveChanges();

            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "SignatureDocumentTemplateId", PageParameter( "SignatureDocumentTemplateId" ) );
            qryParams.Add( "personId", PageParameter( "personId" ) );
            NavigateToParentPage( qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancelType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams.Add( "SignatureDocumentTemplateId", PageParameter( "SignatureDocumentTemplateId" ) );
            qryParams.Add( "personId", PageParameter( "personId" ) );
            NavigateToParentPage( qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnSend control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSend_Click( object sender, EventArgs e )
        {
            int? signatureDocumentId = hfSignatureDocumentId.Value.AsIntegerOrNull();
            if ( signatureDocumentId.HasValue && signatureDocumentId.Value > 0 )
            {
                using ( var rockContext = new RockContext() )
                {
                    var signatureDocument = new SignatureDocumentService( rockContext ).Get( signatureDocumentId.Value );
                    if ( signatureDocument != null && signatureDocument.SignatureDocumentTemplate != null &&
                        signatureDocument.AppliesToPersonAlias != null && signatureDocument.AssignedToPersonAlias != null )
                    {
                        var errorMessages = new List<string>();
                        if ( new SignatureDocumentTemplateService( rockContext ).SendDocument(
                            signatureDocument.SignatureDocumentTemplate, signatureDocument.AssignedToPersonAlias.Person,
                            signatureDocument.AssignedToPersonAlias.Person,
                            signatureDocument.Name, signatureDocument.AppliesToPersonAlias.Person.Email, out errorMessages ) )
                        {
                            var lastInviteDate = RockDateTime.Now;
                            lRequestDate.Text = string.Format( "<span title='{0}'>{1}</span>", lastInviteDate.ToString(), lastInviteDate.ToElapsedString() );

                            nbSend.Title = string.Empty;
                            nbSend.Text = "Signature Invite Was Successfully Sent";
                            nbSend.NotificationBoxType = NotificationBoxType.Success;
                            nbSend.Visible = true;
                        }
                        else
                        {
                            nbSend.Title = "Error Sending Signature Invite";
                            nbSend.Text = string.Format( "<ul><li>{0}</li></ul>", errorMessages.AsDelimited( "</li><li>" ) );
                            nbSend.NotificationBoxType = NotificationBoxType.Danger;
                            nbSend.Visible = true;
                        }
                    }
                    else
                    {
                        nbSend.Title = "Error Sending Signature Invite";
                        nbSend.Text = "<ul><li>'Applies To' and 'Assigned To' values are required</li></ul>";
                        nbSend.NotificationBoxType = NotificationBoxType.Warning;
                        nbSend.Visible = true;
                    }
                }
            }
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="signatureDocument">Type of the defined.</param>
        private void ShowReadonlyDetails( SignatureDocument signatureDocument )
        {
            SetEditMode( false );

            hfSignatureDocumentId.SetValue( signatureDocument.Id );

            lTitle.Text = signatureDocument.Name.FormatAsHtmlTitle();

            var lDetails = new DescriptionList();
            var rDetails = new DescriptionList();

            if ( signatureDocument.BinaryFile != null )
            {
                var getFileUrl = string.Format( "{0}GetFile.ashx?guid={1}", System.Web.VirtualPathUtility.ToAbsolute( "~" ), signatureDocument.BinaryFile.Guid );
                lDetails.Add( "Document", string.Format( "<a href='{0}'>View</a>", getFileUrl ) );
            }

            lDetails.Add( "Document Key", signatureDocument.DocumentKey );

            if ( signatureDocument.LastInviteDate.HasValue )
            {
                lDetails.Add( "Last Request Date", string.Format( "<span title='{0}'>{1}</span>", signatureDocument.LastInviteDate.Value.ToString(), signatureDocument.LastInviteDate.Value.ToElapsedString() ) );
            }

            if ( signatureDocument.AppliesToPersonAlias != null && signatureDocument.AppliesToPersonAlias.Person != null )
            {
                rDetails.Add( "Applies To", signatureDocument.AppliesToPersonAlias.Person.FullName );
            }

            if ( signatureDocument.AssignedToPersonAlias != null && signatureDocument.AssignedToPersonAlias.Person != null )
            {
                rDetails.Add( "Assigned To", signatureDocument.AssignedToPersonAlias.Person.FullName );
            }

            if ( signatureDocument.SignedByPersonAlias != null && signatureDocument.SignedByPersonAlias.Person != null )
            {
                rDetails.Add( "Signed By", signatureDocument.SignedByPersonAlias.Person.FullName );
            }

            if ( signatureDocument.SignatureDocumentTemplate != null )
            {
                lDetails.Add( "Signature Document Type", signatureDocument.SignatureDocumentTemplate.Name );
            }

            lLeftDetails.Text = lDetails.Html;
            lRightDetails.Text = rDetails.Html;

        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="signatureDocument">Type of the defined.</param>
        private void ShowEditDetails( SignatureDocument signatureDocument )
        {
            string titleName = signatureDocument.SignatureDocumentTemplate != null ? signatureDocument.SignatureDocumentTemplate.Name + " Document" : SignatureDocument.FriendlyTypeName;
            if ( signatureDocument.Id > 0 )
            {
                lTitle.Text = ActionTitle.Edit( titleName ).FormatAsHtmlTitle();
            }
            else
            {
                lTitle.Text = ActionTitle.Add( titleName ).FormatAsHtmlTitle();
            }

            btnSend.Visible = signatureDocument.Id > 0 && signatureDocument.Status != SignatureDocumentStatus.Signed;
            btnSend.Text = signatureDocument.Status == SignatureDocumentStatus.Sent ? "Resend Invite" : "Send Invite";

            SetEditMode( true );

            tbName.Text = signatureDocument.Name;

            rbStatus.SelectedValue = signatureDocument.Status.ConvertToInt().ToString();
            lStatusLastUpdated.Visible = signatureDocument.LastStatusDate.HasValue;
            lStatusLastUpdated.Text = signatureDocument.LastStatusDate.HasValue ?
                string.Format( "<span title='{0}'>{1}</span>", signatureDocument.LastStatusDate.Value.ToString(), signatureDocument.LastStatusDate.Value.ToElapsedString() ) :
                string.Empty;

            ppAppliesTo.SetValue( signatureDocument.AppliesToPersonAlias != null ? signatureDocument.AppliesToPersonAlias.Person : null );
            ppAssignedTo.SetValue( signatureDocument.AssignedToPersonAlias != null ? signatureDocument.AssignedToPersonAlias.Person : null );
            ppSignedBy.SetValue( signatureDocument.SignedByPersonAlias != null ? signatureDocument.SignedByPersonAlias.Person : null );

            ddlDocumentType.Visible = signatureDocument.SignatureDocumentTemplate == null;
            ddlDocumentType.SetValue( signatureDocument.SignatureDocumentTemplateId );

            if ( signatureDocument.SignatureDocumentTemplate != null && signatureDocument.SignatureDocumentTemplate.BinaryFileType != null )
            {
                fuDocument.BinaryFileTypeGuid = signatureDocument.SignatureDocumentTemplate.BinaryFileType.Guid;
            }
            fuDocument.BinaryFileId = signatureDocument.BinaryFileId;

            lDocumentKey.Text = signatureDocument.DocumentKey;
            lDocumentKey.Visible = !string.IsNullOrWhiteSpace( signatureDocument.DocumentKey );

            lRequestDate.Visible = signatureDocument.LastInviteDate.HasValue;
            lRequestDate.Text = signatureDocument.LastInviteDate.HasValue ?
                string.Format( "<span title='{0}'>{1}</span>", signatureDocument.LastInviteDate.Value.ToString(), signatureDocument.LastInviteDate.Value.ToElapsedString() ) :
                string.Empty;

        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            vsDetails.Enabled = editable;
            fieldsetViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="signatureDocumentId">The signature document type identifier.</param>
        public void ShowDetail( int signatureDocumentId )
        {
            pnlDetails.Visible = true;
            SignatureDocument signatureDocument = null;

            using ( var rockContext = new RockContext() )
            {
                if ( !signatureDocumentId.Equals( 0 ) )
                {
                    signatureDocument = new SignatureDocumentService( rockContext ).Get( signatureDocumentId );
                }

                if ( signatureDocument == null )
                {
                    signatureDocument = new SignatureDocument { Id = 0 };

                    int? personId = PageParameter( "personId" ).AsIntegerOrNull();
                    if ( personId.HasValue )
                    {
                        var person = new PersonService( rockContext ).Get( personId.Value );
                        if ( person != null )
                        {
                            var personAlias = person.PrimaryAlias;
                            if ( personAlias != null )
                            {
                                signatureDocument.AppliesToPersonAlias = personAlias;
                                signatureDocument.AppliesToPersonAliasId = personAlias.Id;
                                signatureDocument.AssignedToPersonAlias = personAlias;
                                signatureDocument.AssignedToPersonAliasId = personAlias.Id;
                            }
                        }
                    }

                    int? documentTypeId = PageParameter( "SignatureDocumentTemplateId" ).AsIntegerOrNull();
                    if ( documentTypeId.HasValue )
                    {
                        var documentType = new SignatureDocumentTemplateService( rockContext ).Get( documentTypeId.Value );
                        if ( documentType != null )
                        {
                            signatureDocument.SignatureDocumentTemplate= documentType;
                            signatureDocument.SignatureDocumentTemplateId = documentType.Id;
                        }
                    }
                }

                hfSignatureDocumentId.SetValue( signatureDocument.Id );

                // render UI based on Authorized and IsSystem
                bool readOnly = false;

                nbEditModeMessage.Text = string.Empty;
                bool canEdit = UserCanEdit || signatureDocument.IsAuthorized( Authorization.EDIT, CurrentPerson );
                bool canView = canEdit || signatureDocument.IsAuthorized( Authorization.VIEW, CurrentPerson );

                if ( !canView )
                {
                    pnlDetails.Visible = false;
                }
                else
                {
                    pnlDetails.Visible = true;

                    if ( !canEdit )
                    {
                        readOnly = true;
                        nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( SignatureDocument.FriendlyTypeName );
                    }

                    if ( readOnly )
                    {
                        ShowReadonlyDetails( signatureDocument );
                    }
                    else
                    {
                        ShowEditDetails( signatureDocument );
                    }
                }
            }
        }

        #endregion

    }
}