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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.ElectronicSignature;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// User controls for managing signature documents
    /// </summary>
    [DisplayName( "Signature Document Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of a given signature document." )]
    [Rock.SystemGuid.BlockTypeGuid( "01D23E86-51DC-496D-BB3E-0CEF5094F304" )]
    public partial class SignatureDocumentDetail : RockBlock
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
                ddlDocumentType.Items.Insert( 0, new ListItem( "", "" ) );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbLegacyProviderErrorMessage.Visible = false;

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "SignatureDocumentId" ).AsInteger() );
            }

            base.OnLoad( e );
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
            ShowDetail( PageParameter( "SignatureDocumentId" ).AsInteger() );
        }

        #endregion Events

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="signatureDocument">Type of the defined.</param>
        private void ShowReadonlyDetails( SignatureDocument signatureDocument )
        {
            SetEditMode( false );

            hfSignatureDocumentId.SetValue( signatureDocument.Id );

            lTitle.Text = signatureDocument.Name.FormatAsHtmlTitle();
            lSignedBy.Text = signatureDocument.SignedByPersonAlias?.Person.FullName ?? "-";
            lCompletionSignedByPersonEmailAddress.Text = signatureDocument.SignedByEmail;
            lCompletionLastSentDateTime.Text = signatureDocument.CompletionEmailSentDateTime?.ToString();

            lAppliesTo.Text = signatureDocument.AppliesToPersonAlias?.Person.FullName ?? "-";

            lSignedOnInformation.Text = $@"{signatureDocument.SignedDateTime?.ToString( "f" )}<br>
{signatureDocument.GetFormattedUserAgent().ConvertCrLfToHtmlBr()}<br>
{signatureDocument.SignedClientIp}<br>";

            lRelatedEntity.Visible = signatureDocument.EntityTypeId.HasValue;
            if ( signatureDocument.EntityTypeId.HasValue )
            {
                var relatedEntityType = EntityTypeCache.Get( signatureDocument.EntityTypeId.Value );

                // either show just the EntityType's name, or a URL depending if the EntityType has a LinkUrlLavaTemplate
                lRelatedEntity.Text = relatedEntityType?.FriendlyName;

                var linkUrlLavaTemplate = relatedEntityType?.LinkUrlLavaTemplate;

                if ( linkUrlLavaTemplate.IsNotNullOrWhiteSpace() )
                {
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage );
                    if ( signatureDocument.EntityId.HasValue && relatedEntityType != null )
                    {
                        mergeFields.Add( "EntityId", signatureDocument.EntityId );

                        var relatedEntity = Reflection.GetIEntityForEntityType( relatedEntityType.GetEntityType(), signatureDocument.EntityId.Value );
                        mergeFields.Add( "Entity", relatedEntity );

                        var linkUrl = linkUrlLavaTemplate.ResolveMergeFields( mergeFields );
                        linkUrl = this.ResolveRockUrl( linkUrl );
                        if ( linkUrl.IsNotNullOrWhiteSpace() )
                        {
                            lRelatedEntity.Text = $"<a href='{linkUrl}'>{relatedEntityType.FriendlyName}<a>";
                        }
                    }
                }
            }

            // Only show the Resend if there is a CompletionSystemCommunication defined for this document's template
            btnResendCompletionEmail.Visible = signatureDocument?.SignatureDocumentTemplate?.CompletionSystemCommunicationId != null;

            if ( signatureDocument.BinaryFile != null )
            {
                var getFileUrl = FileUrlHelper.GetFileUrl( signatureDocument.BinaryFile.Guid );

                var usesLegacyDocumentProvider = signatureDocument.UsesLegacyDocumentProvider();

                lLegacyDocumentLink.Visible = usesLegacyDocumentProvider;
                lLegacyDocumentLink.Text = $"<a href='{getFileUrl}'>{signatureDocument.BinaryFile.FileName}</a>";
                pdfSignatureDocument.Visible = !usesLegacyDocumentProvider;
                pdfSignatureDocument.SourceUrl = getFileUrl;
            }
            else
            {
                pdfSignatureDocument.Visible = false;
                lLegacyDocumentLink.Visible = false;
            }
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditLegacyProviderDocumentDetails.Visible = editable;
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
                            signatureDocument.SignatureDocumentTemplate = documentType;
                            signatureDocument.SignatureDocumentTemplateId = documentType.Id;
                        }
                    }
                }

                hfSignatureDocumentId.SetValue( signatureDocument.Id );

                nbEditModeMessage.Text = string.Empty;
                bool canEdit = signatureDocument.IsAuthorized( Authorization.EDIT, CurrentPerson );
                bool canView = canEdit || signatureDocument.IsAuthorized( Authorization.VIEW, CurrentPerson );

                if ( !canView )
                {
                    pnlDetails.Visible = false;
                }
                else
                {
                    pnlDetails.Visible = true;

                    ShowReadonlyDetails( signatureDocument );

                    bool showEditButton = canEdit && signatureDocument.UsesLegacyDocumentProvider();

                    btnEditLegacyProviderDocument.Visible = showEditButton;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnResendCompletionEmail control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnResendCompletionEmail_Click( object sender, EventArgs e )
        {
            var signatureDocumentId = hfSignatureDocumentId.Value.AsInteger();
            List<string> errorMessages;
            bool successfullySent = ElectronicSignatureHelper.SendSignatureCompletionCommunication( signatureDocumentId, out errorMessages );

            if ( successfullySent )
            {
                nbCompletionEmailResult.Title = string.Empty;
                nbCompletionEmailResult.Text = "Signature completion email was successfully sent";
                nbCompletionEmailResult.NotificationBoxType = NotificationBoxType.Success;
                nbCompletionEmailResult.Visible = true;
            }
            else
            {
                nbCompletionEmailResult.Title = "Error sending signature completion email";
                nbCompletionEmailResult.Text = string.Format( "<ul><li>{0}</li></ul>", errorMessages.AsDelimited( "</li><li>" ) );
                nbCompletionEmailResult.NotificationBoxType = NotificationBoxType.Warning;
                nbCompletionEmailResult.Visible = true;
            }

            lCompletionLastSentDateTime.Text = new SignatureDocumentService( new RockContext() ).GetSelect( signatureDocumentId, s => s.CompletionEmailSentDateTime )?.ToString();
        }

        /// <summary>
        /// Returns to parent.
        /// </summary>
        private void ReturnToParent()
        {
            var qryParams = new Dictionary<string, string>();
            int? personId = PageParameter( "PersonId" ).AsIntegerOrNull();
            if ( personId.HasValue )
            {
                qryParams.Add( "PersonId", personId.Value.ToString() );
            }
            else
            {
                qryParams.Add( "SignatureDocumentTemplateId", PageParameter( "SignatureDocumentTemplateId" ) );
            }

            NavigateToParentPage( qryParams );
        }

        #region Legacy Provider Related

        /// <summary>
        /// Handles the Click event of the btnSaveType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveLegacyProviderDocument_Click( object sender, EventArgs e )
        {
            bool inviteCancelled = false;

            var rockContext = new RockContext();

            int? documentTemplateId = ddlDocumentType.SelectedValueAsInt();
            if ( !documentTemplateId.HasValue )
            {
                nbLegacyProviderErrorMessage.Title = string.Empty;
                nbLegacyProviderErrorMessage.Text = "Document Template is Required";
                nbLegacyProviderErrorMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbLegacyProviderErrorMessage.Visible = true;
                return;
            }

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
                inviteCancelled = newStatus == SignatureDocumentStatus.Cancelled;
            }

            signatureDocument.AppliesToPersonAliasId = ppAppliesTo.PersonAliasId;
            signatureDocument.AssignedToPersonAliasId = ppAssignedTo.PersonAliasId;
            signatureDocument.SignedByPersonAliasId = ppSignedBy.PersonAliasId;

            signatureDocument.SignatureDocumentTemplateId = documentTemplateId.Value;

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
                if ( binaryFile != null )
                {
                    /*
                        10/25/2023 - PA
                        Only the persons authorized to view the signature document template should be allowed to view the corresponding binary file of the signed documents created from that template.
                        At the time of the writing the entity security could be set only on the template entities and not on the document entities. So for simplicity we are proceeding with setting
                        the parent entity as the signature document template.

                        Reason: Inherit the entity security of the Signature Document Template to the corresponding binary file
                    */
                    binaryFile.ParentEntityId = signatureDocument.SignatureDocumentTemplate.Id;
                    binaryFile.ParentEntityTypeId = EntityTypeCache.Get<SignatureDocumentTemplate>().Id;
                    binaryFile.IsTemporary = false;
                }
            }

            rockContext.SaveChanges();

            if ( inviteCancelled && !string.IsNullOrWhiteSpace( signatureDocument.DocumentKey ) )
            {
                var errorMessages = new List<string>();
                if ( new SignatureDocumentTemplateService( rockContext ).CancelLegacyProviderDocument( signatureDocument, out errorMessages ) )
                {
                    rockContext.SaveChanges();
                }
            }

            ReturnToParent();
        }

        /// <summary>
        /// Handles the Click event of the btnCancelType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancelLegacyProviderDocument_Click( object sender, EventArgs e )
        {
            ReturnToParent();
        }

        /// <summary>
        /// Handles the Click event of the btnSend control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSendLegacyProviderDocument_Click( object sender, EventArgs e )
        {
            int? signatureDocumentId = hfSignatureDocumentId.Value.AsIntegerOrNull();
            if ( signatureDocumentId.HasValue && signatureDocumentId.Value > 0 )
            {
                using ( var rockContext = new RockContext() )
                {
                    var signatureDocument = new SignatureDocumentService( rockContext ).Get( signatureDocumentId.Value );
                    if ( signatureDocument != null )
                    {
                        List<string> errorMessages;
                        if ( new SignatureDocumentTemplateService( rockContext ).SendLegacyProviderDocument( signatureDocument, string.Empty, out errorMessages ) )
                        {
                            rockContext.SaveChanges();

                            ShowLegacyDocumentEditDetails( signatureDocument, true );

                            nbLegacyProviderErrorMessage.Title = string.Empty;
                            nbLegacyProviderErrorMessage.Text = "Signature Invite Was Successfully Sent";
                            nbLegacyProviderErrorMessage.NotificationBoxType = NotificationBoxType.Success;
                            nbLegacyProviderErrorMessage.Visible = true;
                        }
                        else
                        {
                            nbLegacyProviderErrorMessage.Title = "Error Sending Signature Invite";
                            nbLegacyProviderErrorMessage.Text = string.Format( "<ul><li>{0}</li></ul>", errorMessages.AsDelimited( "</li><li>" ) );
                            nbLegacyProviderErrorMessage.NotificationBoxType = NotificationBoxType.Danger;
                            nbLegacyProviderErrorMessage.Visible = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="signatureDocument">Type of the defined.</param>
        private void ShowLegacyDocumentEditDetails( SignatureDocument signatureDocument, bool onlyStatusDetails )
        {
            if ( !onlyStatusDetails )
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

                btnSendLegacyProviderDocument.Visible = signatureDocument.Id > 0 && signatureDocument.Status != SignatureDocumentStatus.Signed;
                btnSendLegacyProviderDocument.Text = signatureDocument.Status == SignatureDocumentStatus.Sent ? "Resend Invite" : "Send Invite";

                SetEditMode( true );

                tbName.Text = signatureDocument.Name;

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
            }

            rbStatus.SelectedValue = signatureDocument.Status.ConvertToInt().ToString();
            hlStatusLastUpdated.Visible = signatureDocument.LastStatusDate.HasValue;
            hlStatusLastUpdated.Text = signatureDocument.LastStatusDate.HasValue ?
                string.Format( "<span title='{0}'>Last Status Update: {1}</span>", signatureDocument.LastStatusDate.Value.ToString(), signatureDocument.LastStatusDate.Value.ToElapsedString() ) :
                string.Empty;

            lDocumentKey.Text = signatureDocument.DocumentKey;
            lDocumentKey.Visible = !string.IsNullOrWhiteSpace( signatureDocument.DocumentKey );

            lRequestDate.Visible = signatureDocument.LastInviteDate.HasValue;
            lRequestDate.Text = signatureDocument.LastInviteDate.HasValue ?
                string.Format( "<span title='{0}'>{1}</span>", signatureDocument.LastInviteDate.Value.ToString(), signatureDocument.LastInviteDate.Value.ToElapsedString() ) :
                string.Empty;
        }

        /// <summary>
        /// Handles the Click event of the btnEditLegacyProviderDocument control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEditLegacyProviderDocument_Click( object sender, EventArgs e )
        {
            int? signatureDocumentId = hfSignatureDocumentId.Value.AsIntegerOrNull();
            if ( signatureDocumentId.HasValue && signatureDocumentId.Value > 0 )
            {
                using ( var rockContext = new RockContext() )
                {
                    var signatureDocument = new SignatureDocumentService( rockContext ).Get( signatureDocumentId.Value );
                    if ( signatureDocument != null )
                    {
                        ShowLegacyDocumentEditDetails( signatureDocument, false );
                    }
                }
            }
        }

        #endregion Legacy Provider Related
    }
}