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

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// User controls for managing signature document types
    /// </summary>
    [DisplayName( "Signature Document Type Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of the given signature document type." )]

    [BinaryFileTypeField( "Default File Type", "The default file type to use when creating new document types.", false, 
        Rock.SystemGuid.BinaryFiletype.SIGNED_DOCUMENT_FILE_TYPE, "", 0 )]
    public partial class SignatureDocumentTypeDetail : RockBlock, IDetailBlock
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

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}');", SignatureDocumentType.FriendlyTypeName );
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
                ShowDetail( PageParameter( "signatureDocumentTypeId" ).AsInteger() );
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
            ShowDetail( PageParameter( "signatureDocumentTypeId" ).AsInteger() );
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
        /// Handles the Click event of the btnSaveType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSaveType_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            SignatureDocumentType signatureDocumentType = null;
            SignatureDocumentTypeService typeService = new SignatureDocumentTypeService( rockContext );

            int signatureDocumentTypeId = hfSignatureDocumentTypeId.ValueAsInt();

            if ( signatureDocumentTypeId == 0 )
            {
                signatureDocumentType = new SignatureDocumentType();
                typeService.Add( signatureDocumentType );
            }
            else
            {
                signatureDocumentType = typeService.Get( signatureDocumentTypeId );
            }

            signatureDocumentType.Name = tbTypeName.Text;
            signatureDocumentType.Description = tbTypeDescription.Text;
            signatureDocumentType.BinaryFileTypeId = bftpFileType.SelectedValueAsInt();
            signatureDocumentType.ProviderEntityTypeId = cpProvider.SelectedEntityTypeId;
            signatureDocumentType.ProviderTemplateKey = ddlTemplate.SelectedValue;

            if ( !signatureDocumentType.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            rockContext.SaveChanges();

            var qryParams = new Dictionary<string, string>();
            qryParams["signatureDocumentTypeId"] = signatureDocumentType.Id.ToString();
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
            SignatureDocumentTypeService signatureDocumentTypeService = new SignatureDocumentTypeService( rockContext );
            SignatureDocumentType signatureDocumentType = signatureDocumentTypeService.Get( int.Parse( hfSignatureDocumentTypeId.Value ) );

            if ( signatureDocumentType != null )
            {
                if ( !signatureDocumentType.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "Sorry, You are not authorized to delete this document Type.", ModalAlertType.Information );
                    return;
                }

                string errorMessage;
                if ( !signatureDocumentTypeService.CanDelete( signatureDocumentType, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                signatureDocumentTypeService.Delete( signatureDocumentType );

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
            if ( hfSignatureDocumentTypeId.IsZero() )
            {
                // Cancelling on Add.  Return to Grid
                NavigateToParentPage();
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                SignatureDocumentTypeService signatureDocumentTypeService = new SignatureDocumentTypeService(new RockContext());
                SignatureDocumentType signatureDocumentType = signatureDocumentTypeService.Get( hfSignatureDocumentTypeId.ValueAsInt() );
                ShowReadonlyDetails( signatureDocumentType );
            }
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="signatureDocumentType">Type of the defined.</param>
        private void ShowReadonlyDetails( SignatureDocumentType signatureDocumentType )
        {
            SetEditMode( false );

            hfSignatureDocumentTypeId.SetValue( signatureDocumentType.Id );

            lTitle.Text = signatureDocumentType.Name.FormatAsHtmlTitle();
            lDescription.Text = signatureDocumentType.Description;

            if ( signatureDocumentType.BinaryFileType != null )
            {
                lLeftDetails.Text = new DescriptionList()
                    .Add( "File Type", signatureDocumentType.BinaryFileType.Name )
                    .Html;
            }

            lRightDetails.Text = new DescriptionList()
                .Add( "Provider Template Key", signatureDocumentType.ProviderTemplateKey )
                .Html;

        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="signatureDocumentType">Type of the defined.</param>
        private void ShowEditDetails( SignatureDocumentType signatureDocumentType )
        {
            if ( signatureDocumentType.Id > 0 )
            {
                lTitle.Text = ActionTitle.Edit( SignatureDocumentType.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lTitle.Text = ActionTitle.Add( SignatureDocumentType.FriendlyTypeName ).FormatAsHtmlTitle();
            }

            SetEditMode( true );

            tbTypeName.Text = signatureDocumentType.Name;
            tbTypeDescription.Text = signatureDocumentType.Description;
            bftpFileType.SetValue( signatureDocumentType.BinaryFileTypeId );
            cpProvider.SetValue( signatureDocumentType.ProviderEntityType != null ? signatureDocumentType.ProviderEntityType.Guid.ToString().ToUpper() : string.Empty );
            SetTemplates();
            ddlTemplate.SetValue( signatureDocumentType.ProviderTemplateKey );
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            SignatureDocumentTypeService signatureDocumentTypeService = new SignatureDocumentTypeService( new RockContext() );
            SignatureDocumentType signatureDocumentType = signatureDocumentTypeService.Get( hfSignatureDocumentTypeId.ValueAsInt() );
            ShowEditDetails( signatureDocumentType );
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
        /// Sets the templates.
        /// </summary>
        private void SetTemplates()
        {
            ddlTemplate.Items.Clear();
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
        /// <param name="signatureDocumentTypeId">The signature document type identifier.</param>
        public void ShowDetail( int signatureDocumentTypeId )
        {
            pnlDetails.Visible = true;
            SignatureDocumentType signatureDocumentType = null;

            using ( var rockContext = new RockContext() )
            {
                if ( !signatureDocumentTypeId.Equals( 0 ) )
                {
                    signatureDocumentType = new SignatureDocumentTypeService( rockContext ).Get( signatureDocumentTypeId );
                }

                if ( signatureDocumentType == null )
                {
                    signatureDocumentType = new SignatureDocumentType { Id = 0 };
                    var components = DigitalSignatureContainer.Instance.Components;
                    var entityType = components.Where( c => c.Value.Value.IsActive ).OrderBy( c => c.Value.Value.Order ).Select( c => c.Value.Value.EntityType ).FirstOrDefault();
                    if ( entityType != null )
                    {
                        signatureDocumentType.ProviderEntityType = new EntityTypeService( rockContext ).Get( entityType.Id );
                    }

                    Guid? fileTypeGuid = GetAttributeValue( "DefaultFileType" ).AsGuidOrNull();
                    if ( fileTypeGuid.HasValue )
                    {
                        var binaryFileType = new BinaryFileTypeService( rockContext ).Get( fileTypeGuid.Value );
                        if ( binaryFileType != null )
                        {
                            signatureDocumentType.BinaryFileType = binaryFileType;
                            signatureDocumentType.BinaryFileTypeId = binaryFileType.Id;
                        }
                    }
                }

                hfSignatureDocumentTypeId.SetValue( signatureDocumentType.Id );


                // render UI based on Authorized and IsSystem
                bool readOnly = false;

                nbEditModeMessage.Text = string.Empty;
                bool canEdit = UserCanEdit || signatureDocumentType.IsAuthorized( Authorization.EDIT, CurrentPerson );
                bool canView = canEdit || signatureDocumentType.IsAuthorized( Authorization.VIEW, CurrentPerson );

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
                        nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( SignatureDocumentType.FriendlyTypeName );
                    }

                    if ( readOnly )
                    {
                        btnEdit.Visible = false;
                        btnDelete.Visible = false;
                        ShowReadonlyDetails( signatureDocumentType );
                    }
                    else
                    {
                        btnEdit.Visible = true;
                        btnDelete.Visible = false;
                        if ( signatureDocumentType.Id > 0 )
                        {
                            ShowReadonlyDetails( signatureDocumentType );
                        }
                        else
                        {
                            ShowEditDetails( signatureDocumentType );
                        }
                    }
                }
            }
        }

        #endregion

    }
}