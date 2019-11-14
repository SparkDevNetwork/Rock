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

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace RockWeb.Blocks.Crm
{
    [DisplayName( "Documents" )]
    [Category( "CRM" )]
    [Description( "Add documents to the current context object." )]
    [ContextAware]
    #region Block Attributes

    [TextField( "Heading Title",
        Description = "The title of the heading.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKeys.HeadingTitle )]

    [TextField( "Heading Icon Css Class",
        Description = "The Icon CSS Class for use in the heading.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKeys.HeadingIconCssClass )]

    [DocumentTypeField( "Document Types",
        Description = "The document types that should be displayed.",
        IsRequired = false,
        Order = 2,
        Key = AttributeKeys.DocumentTypes)]

    [BooleanField( "Show Security Button",
        Description = "Show or hide the security button to add or edit security for the document.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Order = 3,
        Key = AttributeKeys.ShowSecurityButton )]
    #endregion Block Attributes
    public partial class Documents : RockBlock
    {
        private static class AttributeKeys
        {
            public const string HeadingTitle = "HeadingTitle";
            public const string HeadingIconCssClass = "HeadingIconCssClass";
            public const string DocumentTypes = "DocumentTypes";
            public const string ShowSecurityButton = "ShowSecurityButton";
        }

        protected string icon = string.Empty;
        protected string title = string.Empty;
        protected bool canEditBlock = false;

        #region Control Overrides

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            icon = GetAttributeValue( AttributeKeys.HeadingIconCssClass );
            title = GetAttributeValue( AttributeKeys.HeadingTitle );

            this.BlockUpdated += Block_BlockUpdated;

            gFileList.DataKeyNames = new string[] { "Id" };
            gFileList.GridRebind += gFileList_GridRebind;

            // Verify block authorization
            canEditBlock = IsUserAuthorized( Authorization.EDIT );
            bool canAdministrateBlock = IsUserAuthorized( Authorization.ADMINISTRATE );

            gFileList.Actions.ShowAdd = canEditBlock;
            gFileList.IsDeleteEnabled = canEditBlock;
            
            if ( canEditBlock )
            {
                gFileList.Actions.AddClick += gFileList_Add;
                gFileList.RowSelected += gFileList_RowSelected;
            }

            // Configure and show/hide security button
            var securityColumn = gFileList.ColumnsOfType<SecurityField>().FirstOrDefault();
            securityColumn.EntityTypeId = this.ContextEntity().TypeId;
            securityColumn.Visible = canAdministrateBlock;
        }

        protected override void OnLoad( EventArgs e )
        {

            if ( this.ContextEntity() == null )
            {
                return;
            }

            if ( !IsPostBack )
            {
                PopulateDocumentTypeDropDownLists();
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion Control Overrides

        #region Control Events

        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            PopulateDocumentTypeDropDownLists();
            BindGrid();
        }

        #endregion Control Events


        #region Private Methods

        /// <summary>
        /// Registers the download buttons as post back controls.
        /// This is because the page has to do a full postback to download the file.
        /// </summary>
        private void RegisterDownloadButtonsAsPostBackControls()
        {
            foreach ( GridViewRow row in gFileList.Rows)
            {
                LinkButton downloadLinkButton = ( LinkButton ) row.FindControl("lbDownload");
                ScriptManager.GetCurrent( this.Page ).RegisterPostBackControl( downloadLinkButton );
            }
        }
        
        /// <summary>
        /// Clears the Add/Edit form.
        /// </summary>
        private void ClearForm()
        {
            ddlAddEditDocumentType.SelectedIndex = 0;
            tbDocumentName.Text = string.Empty;
            tbDescription.Text = string.Empty;
            pnlAddEdit.Visible = false;
            pnlList.Visible = true;
            hfDocumentId.Value = string.Empty;
            fuUploader.BinaryFileId = null;
        }

        /// <summary>
        /// Creates a list of valid DocumentTypes for the ContextEntity and populates the document type drop down lists.
        /// </summary>
        private void PopulateDocumentTypeDropDownLists()
        {
            var contextEntity = this.ContextEntity();
            var entityTypeId = contextEntity.TypeId;
            List<DocumentTypeCache> documentypesForContextEntityType = DocumentTypeCache.GetByEntity( entityTypeId, false );

            // Get the document types allowed from the block settings and only have those in the list of document types for the entity
            if ( GetAttributeValue( AttributeKeys.DocumentTypes).IsNotNullOrWhiteSpace() )
            {
                var blockAttributeFilteredDocumentTypes = GetAttributeValue( AttributeKeys.DocumentTypes ).Split( ',' ).Select( int.Parse ).ToList();
                documentypesForContextEntityType = documentypesForContextEntityType.Where( d => blockAttributeFilteredDocumentTypes.Contains( d.Id ) ).ToList();
            }

            // Remove document types from the list that do not match the EntityTypeQualifiers
            var entityTypeQualifierFilteredDocumentTypes = new List<int>();
            foreach( var documentType in documentypesForContextEntityType )
            {
                // If the document does not have a qualifier column specified then allow it by default
                if ( documentType.EntityTypeQualifierColumn.IsNotNullOrWhiteSpace() )
                {
                    // Check that the EntityTypeQualifierColumn is a property for this entity, if not then remove it by default
                    if ( contextEntity.GetType().GetProperty(documentType.EntityTypeQualifierColumn) == null )
                    {
                        entityTypeQualifierFilteredDocumentTypes.Add( documentType.Id );
                        continue;
                    }

                    // Get the value of the property specified in DocumentType.EntityTypeQualifierColumn from the current ContextEntity
                    string entityPropVal = this.ContextEntity().GetPropertyValue( documentType.EntityTypeQualifierColumn ).ToString();

                    // If the entity property values does not match DocumentType.EntityTypeQualifierValue then it should be removed.
                    if(entityPropVal != documentType.EntityTypeQualifierValue)
                    {
                        entityTypeQualifierFilteredDocumentTypes.Add( documentType.Id );
                    }
                }
            }

            // Create the final list of document types that are valid for this entity and instance of this entity.
            var filteredDocumentTypes = documentypesForContextEntityType
                .Where( d => !entityTypeQualifierFilteredDocumentTypes.Contains( d.Id ) )
                .ToList();

            PopulateDdlDocumentType( filteredDocumentTypes );
            PopulateDdlAddEditDocumentType( filteredDocumentTypes );
        }

        /// <summary>
        /// Populates the DocumentType DDL on the add/edit panel
        /// </summary>
        /// <param name="documentTypes">The document types.</param>
        private void PopulateDdlAddEditDocumentType( List<DocumentTypeCache> documentTypes )
        {
            ddlAddEditDocumentType.Items.Add( new ListItem( string.Empty, string.Empty ) );

            foreach( var documentType in documentTypes )
            {
                ddlAddEditDocumentType.Items.Add( new ListItem( documentType.Name, documentType.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Populates the DocumentType DDL on the grid filter
        /// </summary>
        /// <param name="documentTypes">The document types.</param>
        private void PopulateDdlDocumentType( List<DocumentTypeCache> documentTypes )
        {
            ddlDocumentType.Items.Add( new ListItem( "All Document Types", string.Empty ) );

            foreach( var documentType in documentTypes )
            {
                ddlDocumentType.Items.Add( new ListItem( documentType.Name, documentType.Id.ToString() ) );
            }
        }

        #endregion Private Methods

        #region Grid Events

        protected void gFileList_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var document = ( Document ) e.Row.DataItem;
            if ( document == null )
            {
                return;
            }

            bool canView = document.IsAuthorized( Authorization.VIEW, this.CurrentPerson );
            bool canEdit = document.IsAuthorized( Authorization.EDIT, this.CurrentPerson ) && canEditBlock;

            if (!canView)
            {
                // Hide the download button
                var downloadButton = ( LinkButton ) e.Row.FindControl( "lbDownload" );
                downloadButton.Visible = false;
            }

            if ( !canEdit )
            {
                // disable edit click for row and delete button
                e.Row.Enabled = false;
            }
        }

        protected void gFileList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        private void BindGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                var contextEntity = this.ContextEntity();
                var entityTypeId = contextEntity.TypeId;

                var documentService = new DocumentService( rockContext );
                var documents = documentService
                    .Queryable()
                    .AsNoTracking()
                    .Where( d => d.DocumentType.EntityTypeId == entityTypeId )
                    .Where( d => d.EntityId == contextEntity.Id );

                if ( ddlDocumentType.SelectedIndex > 0 )
                {
                    int filterDocumentTypeId = ddlDocumentType.SelectedValueAsInt().Value;
                    documents = documents.Where( d => d.DocumentTypeId == filterDocumentTypeId );
                }

                if ( GetAttributeValue( AttributeKeys.DocumentTypes).IsNotNullOrWhiteSpace() )
                {
                    var filteredDocumentTypes = GetAttributeValue( AttributeKeys.DocumentTypes ).Split( ',' ).Select( int.Parse ).ToList();
                    documents = documents.Where( d => filteredDocumentTypes.Contains( d.Id ) );
                }

                gFileList.DataSource = documents.ToList();
                gFileList.DataBind();
            }

            // register download buttons as PostBackControls since they are returning a File download
            RegisterDownloadButtonsAsPostBackControls();
        }

        protected void gFileList_RowSelected( object sender, RowEventArgs e )
        {
            ClearForm();
            hfDocumentId.Value = e.RowKeyId.ToString();
            using ( var rockContext = new RockContext() )
            {
                var documentService = new DocumentService( rockContext );
                var document = documentService.Get( e.RowKeyId );

                ddlAddEditDocumentType.SelectedValue = document.DocumentTypeId.ToString();
                tbDocumentName.Text = document.Name;
                tbDescription.Text = document.Description;
                fuUploader.BinaryFileId = document.BinaryFile.Id;
            }

            pnlAddEdit.Visible = true;
            pnlList.Visible = false;
        }

        protected void gFileList_DeleteClick( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var documentService = new DocumentService( rockContext );
                var document = documentService.Get( e.RowKeyId );
                documentService.Delete( document );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        protected void gFileList_Add( object sender, EventArgs e )
        {
            ClearForm();
            pnlAddEdit.Visible = true;
            pnlList.Visible = false;
        }

        protected void ddlDocumentType_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindGrid();
        }

        protected void gFileListDownload_Click( object sender, EventArgs e )
        {
            var gridRow = ( ( sender as LinkButton ).NamingContainer as GridViewRow );
            int documentId = ( ( HiddenField ) gridRow.FindControl( "hfDocumentId" ) ).ValueAsInt();
            var rockContext = new RockContext();
            var documentService = new DocumentService( rockContext );
            var document = documentService.Get( documentId );

            byte[] bytes = document.BinaryFile.ContentStream.ReadBytesToEnd();

            Response.ContentType = "application/octet-stream";
            Response.AddHeader( "content-disposition", "attachment; filename=" + document.BinaryFile.FileName );
            Response.BufferOutput = true;
            Response.BinaryWrite( bytes );
            Response.Flush();
            Response.SuppressContent = true;
            System.Web.HttpContext.Current.ApplicationInstance.CompleteRequest();
        }

        #endregion Grid Events


        #region Add/Edit Methods

        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( !fuUploader.BinaryFileId.HasValue )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var documentService = new DocumentService( rockContext );
                var document = new Document();

                if ( hfDocumentId.Value.IsNotNullOrWhiteSpace() )
                {
                    document = documentService.Get( hfDocumentId.ValueAsInt() );
                }
                else
                {
                    documentService.Add( document );
                }

                document.DocumentTypeId = ddlAddEditDocumentType.SelectedValueAsInt().Value;
                document.EntityId = this.ContextEntity().Id;
                document.Name = tbDocumentName.Text;
                document.Description = tbDescription.Text;
                document.SetBinaryFile( fuUploader.BinaryFileId.Value, rockContext );

                rockContext.SaveChanges();
            }

            pnlAddEdit.Visible = false;
            BindGrid();
            pnlList.Visible = true;
            ClearForm();
        }

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlAddEdit.Visible = false;
            pnlList.Visible = true;
            ClearForm();
        }

        protected void ddlAddEditDocumentType_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( tbDocumentName.Text.IsNotNullOrWhiteSpace() || ddlAddEditDocumentType.SelectedIndex == 0 )
            {
                // If there is already a name or nothing is selected then do do anything.
                return;
            }

            // Get the selected DocumentType from cache
            var documentTypeCache = DocumentTypeCache.Get( ddlAddEditDocumentType.SelectedValueAsInt() ?? 0 );
            string template = documentTypeCache.DefaultDocumentNameTemplate;
            if ( template.IsNotNullOrWhiteSpace() )
            {
                // If there is a template then apply it
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                tbDocumentName.Text = template.ResolveMergeFields( mergeFields );
            }
        }

        #endregion Add/Edit Methods

        
    }
}