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
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;

namespace RockWeb.Blocks.Crm
{
    /// <summary>
    /// Lists Document Types
    /// </summary>
    /// <seealso cref="Rock.Web.UI.RockBlock" />
    /// <seealso cref="DocumentType" />
    [DisplayName( "Document Type List" )]
    [Category( "CRM" )]
    [Description( "Shows a list of all document types." )]

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Order = 1 )]

    #endregion Block Attributes

    [Rock.SystemGuid.BlockTypeGuid( "C679A2C6-8126-4EF5-8C28-269A51EC4407" )]
    public partial class DocumentTypeList : RockBlock
    {
        #region Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// Key for the detail page
            /// </summary>
            public const string DetailPage = "DetailPage";
        }

        /// <summary>
        /// Keys to use for User Preferences
        /// </summary>
        private static class UserPreferenceKey
        {
            /// <summary>
            /// Key for the active user preference
            /// </summary>
            public const string Active = "Active";
        }

        /// <summary>
        /// Keys for page parameters
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// The document type identifier
            /// </summary>
            public const string DocumentTypeId = "DocumentTypeId";
        }

        #endregion Keys

        #region Lifecycle Events

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            InitializeGrid();

            // Set up Block Settings change notification
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upDocumentTypes );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Initializes the grid.
        /// </summary>
        private void InitializeGrid()
        {
            gDocumentTypes.DataKeyNames = new string[] { "Id" };
            gDocumentTypes.GridRebind += gDocumentTypes_GridRebind;
            gDocumentTypes.GridReorder += gDocumentTypes_GridReorder;
            gDocumentTypes.RowItemText = "Document Type";

            var isUserAuthorized = IsUserAuthorized( Authorization.EDIT );
            var isDetailPageSet = IsDetailPageSet();

            var canDelete = isUserAuthorized;
            var canAddAndEdit = isUserAuthorized && isDetailPageSet;

            gDocumentTypes.Actions.ShowAdd = canAddAndEdit;
            gDocumentTypes.IsDeleteEnabled = canDelete;

            if ( canAddAndEdit )
            {
                gDocumentTypes.Actions.AddClick += gDocumentTypes_Add;
                gDocumentTypes.RowSelected += gDocumentTypes_Edit;
            }

            var securityField = gDocumentTypes.ColumnsOfType<SecurityField>().FirstOrDefault();
            securityField.EntityTypeId = EntityTypeCache.Get( typeof( DocumentType ) ).Id;
        }

        #endregion Lifecycle Events

        #region Control Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            InitializeGrid();
            BindGrid();
        }

        #endregion Control Events

        #region Grid Events

        /// <summary>
        /// Handles the Add event of the gDocumentTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gDocumentTypes_Add( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.DocumentTypeId, 0 );
        }

        /// <summary>
        /// Handles the Edit event of the gDocumentTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gDocumentTypes_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.DocumentTypeId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gDocumentTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gDocumentTypes_Delete( object sender, RowEventArgs e )
        {
            var rockContext = GetRockContext();
            var documentTypeService = GetDocumentTypeService();

            var documentTypeId = e.RowKeyId;
            var documentType = documentTypeService.Get( documentTypeId );

            if ( documentType == null )
            {
                mdGridWarning.Show( "The document type could not be found.", ModalAlertType.Information );
                return;
            }

            var errorMessage = string.Empty;
            if ( !documentTypeService.CanDelete( documentType, out errorMessage ) )
            {
                mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                return;
            }

            documentTypeService.Delete( documentType );
            rockContext.SaveChanges();
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gDocumentTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gDocumentTypes_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the gDocumentTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        private void gDocumentTypes_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = GetRockContext();
            var documentTypeService = new DocumentTypeService( rockContext );

            var documentTypes = documentTypeService.Queryable().OrderBy( dt => dt.Order ).ToList();
            documentTypeService.Reorder( documentTypes, e.OldIndex, e.NewIndex );
            rockContext.SaveChanges();

            BindGrid();
        }

        #endregion Grid Events

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            // Don't use the document type cache here since the users will expect to see the instant changes to this
            // query when they add, edit, etc
            var documentTypeQuery = GetDocumentTypeService()
                .Queryable( "BinaryFileType" )
                .AsNoTracking()
                .OrderBy( dt => dt.Order )
                .ThenBy( dt => dt.Name )
                .ThenBy( dt => dt.Id );

            // Create view models to display in the grid
            var viewModelQuery = documentTypeQuery
                .Select( dt => new DocumentTypeViewModel
                {
                    Id = dt.Id,
                    Name = dt.Name,
                    FileTypeName = dt.BinaryFileType.Name,
                    IconCssClass = dt.IconCssClass,
                    EntityName = dt.EntityType.FriendlyName
                } );

            gDocumentTypes.SetLinqDataSource( viewModelQuery );
            gDocumentTypes.DataBind();
        }

        #endregion Internal Methods

        #region Data Interface Methods

        /// <summary>
        /// Get the rock context
        /// </summary>
        /// <returns></returns>
        private RockContext GetRockContext()
        {
            if ( _rockContext == null )
            {
                _rockContext = new RockContext();
            }

            return _rockContext;
        }
        private RockContext _rockContext = null;

        /// <summary>
        /// Get the document type service
        /// </summary>
        /// <returns></returns>
        private DocumentTypeService GetDocumentTypeService()
        {
            if ( _documentTypeService == null )
            {
                var rockContext = GetRockContext();
                _documentTypeService = new DocumentTypeService( rockContext );
            }

            return _documentTypeService;
        }
        private DocumentTypeService _documentTypeService = null;

        #endregion Data Interface Methods

        #region Block Attribute Helpers

        /// <summary>
        /// Determines whether the detail page attribute has a value.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is detail page set]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsDetailPageSet()
        {
            return !GetAttributeValue( AttributeKey.DetailPage ).IsNullOrWhiteSpace();
        }

        #endregion Block Attribute Helpers

        #region View Models

        /// <summary>
        /// Represents an entry in the list of document types shown in this block.
        /// </summary>
        private class DocumentTypeViewModel
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the type of the file.
            /// </summary>
            /// <value>
            /// The type of the file.
            /// </value>
            public string FileTypeName { get; set; }

            /// <summary>
            /// Gets or sets the icon CSS class.
            /// </summary>
            /// <value>
            /// The icon CSS class.
            /// </value>
            public string IconCssClass { get; set; }

            /// <summary>
            /// Gets or sets the name of the entity.
            /// </summary>
            /// <value>
            /// The name of the entity.
            /// </value>
            public string EntityName { get; set; }
        }

        #endregion View Models
    }
}