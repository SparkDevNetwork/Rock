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

        #region Control Overrides

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            icon = GetAttributeValue( AttributeKeys.HeadingIconCssClass );
            title = GetAttributeValue( AttributeKeys.HeadingTitle );

            this.BlockUpdated += Block_BlockUpdated;

            gFileList.DataKeyNames = new string[] { "Id" };
            // TODO: Add security
            gFileList.Actions.ShowAdd = true;
            gFileList.IsDeleteEnabled = true;

            gFileList.GridRebind += gFileList_GridRebind;
            gFileList.Actions.AddClick += gFileList_Add;
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

        private void RegisterDownloadButtonsAsPostBackControls()
        {
            foreach ( GridViewRow row in gFileList.Rows)
            {
                LinkButton fullPostBackLink = ( LinkButton ) row.FindControl("lbDownload");
                ScriptManager.GetCurrent( this.Page ).RegisterPostBackControl( fullPostBackLink );
            }
        }

        #endregion Control Events


        #region Private Methods
        


        #endregion Private Methods

        #region Grid Events

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

        private void PopulateDocumentTypeDropDownLists()
        {
            var contextEntity = this.ContextEntity();
            var entityTypeId = contextEntity.TypeId;
            List<DocumentTypeCache> documentypesForContextEntityType = DocumentTypeCache.GetByEntity( entityTypeId, false );

            if ( GetAttributeValue( AttributeKeys.DocumentTypes).IsNotNullOrWhiteSpace() )
            {
                var blockAttributeFilteredDocumentTypes = GetAttributeValue( AttributeKeys.DocumentTypes ).Split( ',' ).Select( int.Parse ).ToList();
                documentypesForContextEntityType = documentypesForContextEntityType.Where( d => blockAttributeFilteredDocumentTypes.Contains( d.Id ) ).ToList();
            }

            // Build a list of document types that should not be listed for the entity because of EntityTypeQualifiers
            var entityTypeQualifierFilteredDocumentTypes = new List<int>();
            foreach( var documentType in documentypesForContextEntityType )
            {
                if ( documentType.EntityTypeQualifierColumn.IsNotNullOrWhiteSpace() )
                {
                    if ( contextEntity.GetType().GetProperty(documentType.EntityTypeQualifierColumn) == null )
                    {
                        // if this is true then the qualifier does not match the entity for some reason.
                        entityTypeQualifierFilteredDocumentTypes.Add( documentType.Id );
                        continue;
                    }

                    string entityPropVal = this.ContextEntity().GetPropertyValue( documentType.EntityTypeQualifierColumn ).ToString();
                    if(entityPropVal != documentType.EntityTypeQualifierValue)
                    {
                        entityTypeQualifierFilteredDocumentTypes.Add( documentType.Id );
                    }
                }
            }

            var filteredDocumentTypes = documentypesForContextEntityType
                .Where( d => !entityTypeQualifierFilteredDocumentTypes.Contains( d.Id ) )
                .ToList();

            PopulateDdlDocumentType( filteredDocumentTypes );
            PopulateDdlAddEditDocumentType( filteredDocumentTypes );
        }

        private void PopulateDdlAddEditDocumentType( List<DocumentTypeCache> documentTypes )
        {
            ddlAddEditDocumentType.Items.Add( new ListItem( string.Empty, string.Empty ) );

            foreach( var documentType in documentTypes )
            {
                ddlAddEditDocumentType.Items.Add( new ListItem( documentType.Name, documentType.Id.ToString() ) );
            }
        }

        private void PopulateDdlDocumentType( List<DocumentTypeCache> documentTypes )
        {
            ddlDocumentType.Items.Add( new ListItem( "All Document Types", string.Empty ) );

            foreach( var documentType in documentTypes )
            {
                ddlDocumentType.Items.Add( new ListItem( documentType.Name, documentType.Id.ToString() ) );
            }
        }

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

        #endregion Add/Edit Methods


    }
}