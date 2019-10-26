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

    [EntityTypeField( "Entity Type Context",
        Description = "Which entity type to show documents for.",
        IsRequired = true,
        Order = 4,
        Key = AttributeKeys.EntityTypeContext )]

    #endregion Block Attributes
    public partial class Documents : RockBlock
    {
        private static class AttributeKeys
        {
            public const string HeadingTitle = "HeadingTitle";
            public const string HeadingIconCssClass = "HeadingIconCssClass";
            public const string DocumentTypes = "DocumentTypes";
            public const string ShowSecurityButton = "ShowSecurityButton";
            public const string EntityTypeContext = "EntityTypeContext";
        }

        protected string icon = string.Empty;
        protected string title = string.Empty;

        #region Control Overrides

        protected override void OnInit( EventArgs e )
        {
            // Get the context entity
            //Rock.Data.IEntity contextEntity = this.ContextEntity();

            icon = GetAttributeValue( AttributeKeys.HeadingIconCssClass );
            title = GetAttributeValue( AttributeKeys.HeadingTitle );

            this.BlockUpdated += Block_BlockUpdated;

            gFileList.DataKeyNames = new string[] { "Id" };
            // TODO: Add security
            gFileList.Actions.ShowAdd = true;
            gFileList.IsDeleteEnabled = true;

            gFileList.GridRebind += gFileList_GridRebind;
        }

        protected override void OnLoad( EventArgs e )
        {
            if ( !IsPostBack )
            {
                PopulateDdlDocumentType();

                
                BindGrid();
            }
        }

        #endregion Control Overrides

        #region Control Events
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            // Rebind the grid
            BindGrid();
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
                var entityType = EntityTypeCache.Get( GetAttributeValue( AttributeKeys.EntityTypeContext ) );

                var documentService = new DocumentService( rockContext );
                var documents = documentService
                    .Queryable()
                    .AsNoTracking()
                    .Where( d => d.DocumentType.EntityTypeId == entityType.Id );

                gFileList.DataSource = documents.ToList();
                gFileList.DataBind();
            }
        }

        protected void gFileList_RowSelected( object sender, RowEventArgs e )
        {

        }

        protected void gFileListDelete_Click( object sender, RowEventArgs e )
        {

        }

        #endregion Grid Events

        protected void ddlDocumentType_SelectedIndexChanged( object sender, EventArgs e )
        {

        }

        private void PopulateDdlDocumentType()
        {
            List<DocumentTypeCache> docTypes = new List<DocumentTypeCache>();

            //IEntity contextEntity = GetAttributeValue( AttributeKeys.EntityTypeContext)
            //if ( contextEntity == null )
            //{
            //    docTypes = DocumentTypeCache.All().ToList();
            //}
            //else
            //{
            //    docTypes = DocumentTypeCache.GetByEntity( contextEntity.TypeId, string.Empty, string.Empty );
            //}

            string contextEntityGuid = GetAttributeValue( AttributeKeys.EntityTypeContext );
            if ( contextEntityGuid.IsNullOrWhiteSpace() )
            {
                docTypes = DocumentTypeCache.All().ToList();
            }
            else
            {
                docTypes = DocumentTypeCache.GetByEntity( EntityTypeCache.Get( contextEntityGuid ).Id, string.Empty, string.Empty );
            }

            var temp = docTypes.Select( d => new { d.Id, d.Name } );

            ddlDocumentType.DataSource = docTypes.Select( d => new { d.Id, d.Name } );
            ddlDocumentType.DataValueField = "Id";
            ddlDocumentType.DataTextField = "Name";
            ddlDocumentType.DataBind();

        }
    }
}