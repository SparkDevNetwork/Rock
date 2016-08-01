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
using Rock.Web.Cache;
using System.Web.UI.WebControls;
using Newtonsoft.Json.Linq;
using System.Data.Entity;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// User controls for managing signature document templates
    /// </summary>
    [DisplayName( "Signature Document Template Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of the given signature document template." )]

    [BinaryFileTypeField( "Default File Type", "The default file type to use when creating new documents.", false, 
        Rock.SystemGuid.BinaryFiletype.SIGNED_DOCUMENT_FILE_TYPE, "", 0 )]
    [BinaryFileTypeField( "Default Invite Email", "The default system email to use when creating new document types.", false,
        Rock.SystemGuid.SystemEmail.DIGITAL_SIGNATURE_INVITE, "", 1 )]
    public partial class SignatureDocumentTemplateDetail : RockBlock, IDetailBlock
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

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", SignatureDocumentTemplate.FriendlyTypeName );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbEditModeMessage.Visible = false;

            if ( !Page.IsPostBack )
            {
                LoadDropDowns();
                ShowDetail( PageParameter( "SignatureDocumentTemplateId" ).AsInteger() );
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
            ShowDetail( PageParameter( "SignatureDocumentTemplateId" ).AsInteger() );
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cpProvider control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpProvider_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetTemplates();
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            SignatureDocumentTemplateService SignatureDocumentTemplateService = new SignatureDocumentTemplateService( new RockContext() );
            SignatureDocumentTemplate SignatureDocumentTemplate = SignatureDocumentTemplateService.Get( hfSignatureDocumentTemplateId.ValueAsInt() );
            ShowEditDetails( SignatureDocumentTemplate );
        }

        /// <summary>
        /// Handles the Click event of the btnSaveType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveType_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            SignatureDocumentTemplate SignatureDocumentTemplate = null;
            SignatureDocumentTemplateService typeService = new SignatureDocumentTemplateService( rockContext );

            int SignatureDocumentTemplateId = hfSignatureDocumentTemplateId.ValueAsInt();

            if ( SignatureDocumentTemplateId == 0 )
            {
                SignatureDocumentTemplate = new SignatureDocumentTemplate();
                typeService.Add( SignatureDocumentTemplate );
            }
            else
            {
                SignatureDocumentTemplate = typeService.Get( SignatureDocumentTemplateId );
            }

            SignatureDocumentTemplate.Name = tbTypeName.Text;
            SignatureDocumentTemplate.Description = tbTypeDescription.Text;
            SignatureDocumentTemplate.BinaryFileTypeId = bftpFileType.SelectedValueAsInt();
            SignatureDocumentTemplate.ProviderEntityTypeId = cpProvider.SelectedEntityTypeId;
            SignatureDocumentTemplate.ProviderTemplateKey = ddlTemplate.SelectedValue;
            SignatureDocumentTemplate.InviteSystemEmailId = ddlSystemEmail.SelectedValueAsInt();

            if ( !SignatureDocumentTemplate.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            rockContext.SaveChanges();

            var qryParams = new Dictionary<string, string>();
            qryParams["SignatureDocumentTemplateId"] = SignatureDocumentTemplate.Id.ToString();
            NavigateToCurrentPage( qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            SignatureDocumentTemplateService SignatureDocumentTemplateService = new SignatureDocumentTemplateService( rockContext );
            SignatureDocumentTemplate SignatureDocumentTemplate = SignatureDocumentTemplateService.Get( int.Parse( hfSignatureDocumentTemplateId.Value ) );

            if ( SignatureDocumentTemplate != null )
            {
                if ( !SignatureDocumentTemplate.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "Sorry, You are not authorized to delete this document template.", ModalAlertType.Information );
                    return;
                }

                string errorMessage;
                if ( !SignatureDocumentTemplateService.CanDelete( SignatureDocumentTemplate, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                SignatureDocumentTemplateService.Delete( SignatureDocumentTemplate );

                rockContext.SaveChanges();

            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancelType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancelType_Click( object sender, EventArgs e )
        {
            if ( hfSignatureDocumentTemplateId.IsZero() )
            {
                // Cancelling on Add.  Return to Grid
                NavigateToParentPage();
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                SignatureDocumentTemplateService SignatureDocumentTemplateService = new SignatureDocumentTemplateService(new RockContext());
                SignatureDocumentTemplate SignatureDocumentTemplate = SignatureDocumentTemplateService.Get( hfSignatureDocumentTemplateId.ValueAsInt() );
                ShowReadonlyDetails( SignatureDocumentTemplate );
            }
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="SignatureDocumentTemplate">Type of the defined.</param>
        private void ShowReadonlyDetails( SignatureDocumentTemplate SignatureDocumentTemplate )
        {
            SetEditMode( false );

            hfSignatureDocumentTemplateId.SetValue( SignatureDocumentTemplate.Id );

            lTitle.Text = SignatureDocumentTemplate.Name.FormatAsHtmlTitle();
            lDescription.Text = SignatureDocumentTemplate.Description;

            if ( SignatureDocumentTemplate.BinaryFileType != null )
            {
                lLeftDetails.Text = new DescriptionList()
                    .Add( "File Type", SignatureDocumentTemplate.BinaryFileType.Name )
                    .Html;
            }

            lRightDetails.Text = new DescriptionList()
                .Add( "Provider Template Key", SignatureDocumentTemplate.ProviderTemplateKey )
                .Html;

        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="signatureDocumentTemplate">Type of the defined.</param>
        private void ShowEditDetails( SignatureDocumentTemplate signatureDocumentTemplate )
        {
            if ( signatureDocumentTemplate.Id > 0 )
            {
                lTitle.Text = ActionTitle.Edit( SignatureDocumentTemplate.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lTitle.Text = ActionTitle.Add( SignatureDocumentTemplate.FriendlyTypeName ).FormatAsHtmlTitle();
            }

            SetEditMode( true );

            tbTypeName.Text = signatureDocumentTemplate.Name;
            tbTypeDescription.Text = signatureDocumentTemplate.Description;
            bftpFileType.SetValue( signatureDocumentTemplate.BinaryFileTypeId );
            cpProvider.SetValue( signatureDocumentTemplate.ProviderEntityType != null ? signatureDocumentTemplate.ProviderEntityType.Guid.ToString().ToUpper() : string.Empty );
            SetTemplates();
            ddlTemplate.SetValue( signatureDocumentTemplate.ProviderTemplateKey );
            ddlSystemEmail.SetValue( signatureDocumentTemplate.InviteSystemEmailId );
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
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            ddlSystemEmail.Items.Clear();
            using ( var rockContext = new RockContext()  )
            {
                foreach( var systemEmail in new SystemEmailService( rockContext )
                    .Queryable().AsNoTracking()
                    .OrderBy( e => e.Title ) 
                    .Select( e => new
                    {
                        e.Id,
                        e.Title
                    } ) )
                {
                    ddlSystemEmail.Items.Add( new ListItem( systemEmail.Title, systemEmail.Id.ToString() ) );
                }
            }
        }

        /// <summary>
        /// Sets the templates.
        /// </summary>
        private void SetTemplates()
        {
            ddlTemplate.Items.Clear();
            ddlTemplate.Items.Add( new ListItem() );

            int? entityTypeId = cpProvider.SelectedEntityTypeId;
            if ( entityTypeId.HasValue )
            {
                var entityType = EntityTypeCache.Read( entityTypeId.Value );
                if ( entityType != null )
                {
                    var component = DigitalSignatureContainer.GetComponent( entityType.Name );
                    if ( component != null )
                    {
                        var errors = new List<string>();
                        var templates = component.GetTemplates( out errors );
                        if ( templates != null )
                        {
                            foreach ( var keyVal in templates.OrderBy( d => d.Value ) )
                            {
                                ddlTemplate.Items.Add( new ListItem( keyVal.Value, keyVal.Key ) );
                            }
                        }
                        else
                        {
                            nbEditModeMessage.Text = string.Format( "<ul><li>{0}</li></ul>", errors.AsDelimited( "</li><li>" ) );
                            nbEditModeMessage.Visible = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="SignatureDocumentTemplateId">The signature document type identifier.</param>
        public void ShowDetail( int SignatureDocumentTemplateId )
        {
            pnlDetails.Visible = true;
            SignatureDocumentTemplate SignatureDocumentTemplate = null;

            using ( var rockContext = new RockContext() )
            {
                if ( !SignatureDocumentTemplateId.Equals( 0 ) )
                {
                    SignatureDocumentTemplate = new SignatureDocumentTemplateService( rockContext ).Get( SignatureDocumentTemplateId );
                }

                if ( SignatureDocumentTemplate == null )
                {
                    SignatureDocumentTemplate = new SignatureDocumentTemplate { Id = 0 };
                    var components = DigitalSignatureContainer.Instance.Components;
                    var entityType = components.Where( c => c.Value.Value.IsActive ).OrderBy( c => c.Value.Value.Order ).Select( c => c.Value.Value.EntityType ).FirstOrDefault();
                    if ( entityType != null )
                    {
                        SignatureDocumentTemplate.ProviderEntityType = new EntityTypeService( rockContext ).Get( entityType.Id );
                    }

                    Guid? fileTypeGuid = GetAttributeValue( "DefaultFileType" ).AsGuidOrNull();
                    if ( fileTypeGuid.HasValue )
                    {
                        var binaryFileType = new BinaryFileTypeService( rockContext ).Get( fileTypeGuid.Value );
                        if ( binaryFileType != null )
                        {
                            SignatureDocumentTemplate.BinaryFileType = binaryFileType;
                            SignatureDocumentTemplate.BinaryFileTypeId = binaryFileType.Id;
                        }
                    }

                    Guid? inviteEmailGuid = GetAttributeValue( "DefaultInviteEmail" ).AsGuidOrNull();
                    if ( inviteEmailGuid.HasValue )
                    {
                        var systemEmail = new SystemEmailService( rockContext ).Get( inviteEmailGuid.Value );
                        if ( systemEmail != null )
                        {
                            SignatureDocumentTemplate.InviteSystemEmail = systemEmail;
                            SignatureDocumentTemplate.InviteSystemEmailId = systemEmail.Id;
                        }
                    }

                }

                hfSignatureDocumentTemplateId.SetValue( SignatureDocumentTemplate.Id );


                // render UI based on Authorized and IsSystem
                bool readOnly = false;

                nbEditModeMessage.Text = string.Empty;
                bool canEdit = UserCanEdit || SignatureDocumentTemplate.IsAuthorized( Authorization.EDIT, CurrentPerson );
                bool canView = canEdit || SignatureDocumentTemplate.IsAuthorized( Authorization.VIEW, CurrentPerson );

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
                        nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( SignatureDocumentTemplate.FriendlyTypeName );
                    }

                    if ( readOnly )
                    {
                        btnEdit.Visible = false;
                        btnDelete.Visible = false;
                        ShowReadonlyDetails( SignatureDocumentTemplate );
                    }
                    else
                    {
                        btnEdit.Visible = true;
                        btnDelete.Visible = false;
                        if ( SignatureDocumentTemplate.Id > 0 )
                        {
                            ShowReadonlyDetails( SignatureDocumentTemplate );
                        }
                        else
                        {
                            ShowEditDetails( SignatureDocumentTemplate );
                        }
                    }
                }
            }
        }

        #endregion

    }
}