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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Crm.DocumentList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Crm
{
    /// <summary>
    /// Displays the documents that are attached to the page's context entity and
    /// allows them to be added, edited, downloaded, and deleted.
    /// </summary>

    [DisplayName( "Documents" )]
    [Category( "CRM" )]
    [Description( "Add documents to the current context object." )]
    [IconCssClass( "ti ti-file" )]
    [SupportedSiteTypes( SiteType.Web )]
    [Rock.Web.UI.ContextAware]

    #region Block Attributes

    [TextField( "Heading Title",
        Description = "The title of the heading.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.HeadingTitle )]

    [DocumentTypeField( "Document Types",
        Description = "The document types that should be displayed.",
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.DocumentTypes )]

    [BooleanField( "Show Security Button",
        Description = "Show or hide the security button to add or edit security for the document.",
        IsRequired = true,
        DefaultBooleanValue = true,
        Order = 2,
        Key = AttributeKey.ShowSecurityButton )]

    #endregion Block Attributes

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Secondary )]
    [Rock.SystemGuid.EntityTypeGuid( "6D61C0D7-DCB7-46A7-A18C-ADAAB8494BB3" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "46E47EF0-3366-499C-8A94-685923EAE63A" )]
    [Rock.SystemGuid.BlockTypeGuid( "A8456E2D-1930-4FF7-8A46-FB0800AC31E0" )]
    [CustomizedGrid]
    public class DocumentList : RockEntityListBlockType<Document>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string HeadingTitle = "HeadingTitle";
            public const string DocumentTypes = "DocumentTypes";
            public const string ShowSecurityButton = "ShowSecurityButton";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// The resolved context entity for this request. Use
        /// <see cref="GetContextEntityCached"/> to access it.
        /// </summary>
        private IEntity _contextEntity;

        /// <summary>
        /// Indicates whether <see cref="_contextEntity"/> has been resolved yet
        /// (the resolved value may legitimately be <c>null</c>).
        /// </summary>
        private bool _isContextEntityResolved;

        /// <summary>
        /// The document types valid for the current context entity, cached for
        /// the duration of the request.
        /// </summary>
        private List<DocumentTypeCache> _filteredDocumentTypes;

        /// <summary>
        /// The extensions that can be previewed inline in a browser.
        /// </summary>
        private static readonly string[] _viewableFileExtensions = new[] { ".pdf", ".gif", ".jpg", ".png" };

        #endregion Fields

        #region Properties

        /// <inheritdoc/>
        protected override bool DisableAttributes => true;

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<DocumentListOptionsBag>
            {
                Options = new DocumentListOptionsBag
                {
                    Title = GetAttributeValue( AttributeKey.HeadingTitle )
                }
            };

            // The context entity type must be configured for the block to do anything.
            var contextEntityType = GetContextEntityType();

            if ( contextEntityType == null )
            {
                box.Options.WarningMessage = "The block context entity has not been configured. Go to block settings and select the Entity Type in the 'Context' drop-down list.";
                box.Options.IsBlockVisible = false;

                return box;
            }

            // The page must supply context for the configured entity type.
            if ( !RequestContext.GetContextEntityTypes().Contains( contextEntityType ) )
            {
                box.Options.WarningMessage = "The page context entity has not been configured for this block. Go to Page Properties and click Advanced and enter a valid parameter name under 'Context Parameters'.";
                box.Options.IsBlockVisible = false;

                return box;
            }

            // The page supplies the context type but there is no entity yet
            // (e.g. a new record is being created), so hide the block.
            if ( GetContextEntityCached() == null )
            {
                box.Options.IsBlockVisible = false;

                return box;
            }

            box.Options.IsBlockVisible = true;
            box.Options.IsSecurityColumnVisible = GetAttributeValue( AttributeKey.ShowSecurityButton ).AsBoolean();
            box.IsAddEnabled = GetAddableDocumentTypes().Any();
            box.IsDeleteEnabled = true;
            box.ExpectedRowCount = null;
            box.GridDefinition = GetGridBuilder().BuildDefinition();

            return box;
        }

        /// <inheritdoc/>
        protected override IQueryable<Document> GetListQueryable( RockContext rockContext )
        {
            var contextEntity = GetContextEntityCached();

            if ( contextEntity == null )
            {
                return Enumerable.Empty<Document>().AsQueryable();
            }

            var allowedDocumentTypeIds = GetFilteredDocumentTypes().Select( t => t.Id ).ToList();
            var entityTypeId = contextEntity.TypeId;
            var entityId = contextEntity.Id;

            return new DocumentService( rockContext )
                .Queryable()
                .Include( d => d.DocumentType )
                .Include( d => d.BinaryFile )
                .Include( "CreatedByPersonAlias.Person" )
                .Where( d => d.EntityId == entityId
                    && d.DocumentType.EntityTypeId == entityTypeId
                    && allowedDocumentTypeIds.Contains( d.DocumentTypeId ) );
        }

        /// <inheritdoc/>
        protected override IQueryable<Document> GetOrderedListQueryable( IQueryable<Document> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( d => d.Name );
        }

        /// <inheritdoc/>
        protected override List<Document> GetListItems( IQueryable<Document> queryable, RockContext rockContext )
        {
            var currentPerson = RequestContext.CurrentPerson;

            // Document type view security is already applied by the queryable
            // filter; this applies per-document view security in memory.
            return queryable
                .ToList()
                .Where( d => d.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .ToList();
        }

        /// <inheritdoc/>
        protected override GridBuilder<Document> GetGridBuilder()
        {
            var currentPerson = RequestContext.CurrentPerson;

            return new GridBuilder<Document>()
                .WithBlock( this )
                .AddTextField( "idKey", d => d.IdKey )
                .AddTextField( "name", d => d.Name )
                .AddField( "iconCssClass", d => DocumentTypeCache.Get( d.DocumentTypeId )?.IconCssClass )
                .AddTextField( "documentType", d => DocumentTypeCache.Get( d.DocumentTypeId )?.Name )
                .AddPersonField( "createdBy", d => d.CreatedByPersonAlias?.Person )
                .AddDateTimeField( "createdDateTime", d => d.CreatedDateTime )
                .AddField( "isViewable", d => IsViewableDocument( d ) )
                .AddTextField( "viewUrl", d => GetInlineFileUrl( d ) )
                .AddTextField( "downloadUrl", d => GetDownloadFileUrl( d ) )
                .AddField( "isEditDisabled", d => !CanEditDocument( d, currentPerson ) )
                .AddField( "isDeleteDisabled", d => !CanEditDocument( d, currentPerson ) )
                .AddField( "isSecurityDisabled", d => !CanEditDocument( d, currentPerson ) );
        }

        /// <summary>
        /// Gets the context entity for this request, resolving it only once.
        /// </summary>
        /// <returns>The context <see cref="IEntity"/>, or <c>null</c> if there is none.</returns>
        private IEntity GetContextEntityCached()
        {
            if ( !_isContextEntityResolved )
            {
                _contextEntity = GetContextEntity();
                _isContextEntityResolved = true;
            }

            return _contextEntity;
        }

        /// <summary>
        /// Gets the document types that are valid for the current context
        /// entity. This honors the block's document type setting, per-type view
        /// security, and the document type's entity qualifier column/value.
        /// </summary>
        /// <returns>A cached list of <see cref="DocumentTypeCache"/>.</returns>
        private List<DocumentTypeCache> GetFilteredDocumentTypes()
        {
            if ( _filteredDocumentTypes != null )
            {
                return _filteredDocumentTypes;
            }

            var contextEntity = GetContextEntityCached();

            if ( contextEntity == null )
            {
                _filteredDocumentTypes = new List<DocumentTypeCache>();

                return _filteredDocumentTypes;
            }

            var documentTypes = DocumentTypeCache.GetByEntity( contextEntity.TypeId, true );

            // Limit to the document types selected in the block settings, if any.
            var configuredDocumentTypeIds = GetAttributeValue( AttributeKey.DocumentTypes ).SplitDelimitedValues().AsIntegerList();

            if ( configuredDocumentTypeIds.Any() )
            {
                documentTypes = documentTypes.Where( d => configuredDocumentTypeIds.Contains( d.Id ) ).ToList();
            }

            var currentPerson = RequestContext.CurrentPerson;
            var allowedDocumentTypes = new List<DocumentTypeCache>();

            foreach ( var documentType in documentTypes )
            {
                if ( !documentType.IsAuthorized( Authorization.VIEW, currentPerson ) )
                {
                    continue;
                }

                // A document type with a qualifier column only applies when the
                // context entity has that property and its value matches.
                if ( documentType.EntityTypeQualifierColumn.IsNotNullOrWhiteSpace() )
                {
                    var qualifierProperty = contextEntity.GetType().GetProperty( documentType.EntityTypeQualifierColumn );

                    if ( qualifierProperty == null )
                    {
                        continue;
                    }

                    var entityPropertyValue = contextEntity.GetPropertyValue( documentType.EntityTypeQualifierColumn )?.ToString();

                    if ( entityPropertyValue != documentType.EntityTypeQualifierValue )
                    {
                        continue;
                    }
                }

                allowedDocumentTypes.Add( documentType );
            }

            _filteredDocumentTypes = allowedDocumentTypes;

            return _filteredDocumentTypes;
        }

        /// <summary>
        /// Gets the document types the current person may select when adding a
        /// new document (user-selectable and edit-authorized).
        /// </summary>
        /// <returns>A list of <see cref="DocumentTypeCache"/>.</returns>
        private List<DocumentTypeCache> GetAddableDocumentTypes()
        {
            var currentPerson = RequestContext.CurrentPerson;

            return GetFilteredDocumentTypes()
                .Where( t => t.UserSelectable )
                .Where( t => t.IsAuthorized( Authorization.EDIT, currentPerson ) )
                .ToList();
        }

        /// <summary>
        /// Builds the editor metadata for each document type the current person
        /// may select when adding a new document.
        /// </summary>
        /// <returns>A list of <see cref="DocumentTypeListItemBag"/>.</returns>
        private List<DocumentTypeListItemBag> GetAddableDocumentTypeBags()
        {
            var mergeFields = RequestContext.GetCommonMergeFields();

            return GetAddableDocumentTypes()
                .Select( t => new DocumentTypeListItemBag
                {
                    Value = t.Guid.ToString(),
                    Text = t.Name,
                    BinaryFileTypeGuid = BinaryFileTypeCache.Get( t.BinaryFileTypeId )?.Guid ?? Guid.Empty,
                    NameTemplate = t.DefaultDocumentNameTemplate.IsNotNullOrWhiteSpace()
                        ? t.DefaultDocumentNameTemplate.ResolveMergeFields( mergeFields )
                        : string.Empty,
                    IconCssClass = t.IconCssClass
                } )
                .ToList();
        }

        /// <summary>
        /// Determines whether the current person may edit (and therefore delete
        /// or secure) the specified document. This requires edit rights on both
        /// the document type and the document itself.
        /// </summary>
        /// <param name="document">The document to check.</param>
        /// <param name="person">The person whose rights are being checked.</param>
        /// <returns><c>true</c> if the person may edit the document.</returns>
        private bool CanEditDocument( Document document, Person person )
        {
            var documentType = DocumentTypeCache.Get( document.DocumentTypeId );

            return documentType != null
                && documentType.IsAuthorized( Authorization.EDIT, person )
                && document.IsAuthorized( Authorization.EDIT, person );
        }

        /// <summary>
        /// Determines whether the document's file can be previewed inline in a
        /// browser (based on its extension).
        /// </summary>
        /// <param name="document">The document to check.</param>
        /// <returns><c>true</c> if the file can be previewed inline.</returns>
        private bool IsViewableDocument( Document document )
        {
            var fileName = document.BinaryFile?.FileName;

            if ( fileName.IsNullOrWhiteSpace() )
            {
                return false;
            }

            var extension = System.IO.Path.GetExtension( fileName );

            return _viewableFileExtensions.Contains( extension, StringComparer.OrdinalIgnoreCase );
        }

        /// <summary>
        /// Gets the URL that opens the document's file inline.
        /// </summary>
        /// <param name="document">The document whose file URL is needed.</param>
        /// <returns>The inline file URL, or an empty string if there is no file.</returns>
        private string GetInlineFileUrl( Document document )
        {
            var binaryFileId = document.BinaryFile?.Id;

            return binaryFileId.HasValue ? FileUrlHelper.GetFileUrl( binaryFileId.Value ) : string.Empty;
        }

        /// <summary>
        /// Gets the URL that forces the document's file to download as an
        /// attachment.
        /// </summary>
        /// <param name="document">The document whose file URL is needed.</param>
        /// <returns>The download file URL, or an empty string if there is no file.</returns>
        private string GetDownloadFileUrl( Document document )
        {
            var binaryFileId = document.BinaryFile?.Id;

            if ( !binaryFileId.HasValue )
            {
                return string.Empty;
            }

            return $"{FileUrlHelper.GetFileUrl( binaryFileId.Value )}&attachment=true";
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the information needed to add a new document or edit an existing
        /// one in the modal.
        /// </summary>
        /// <param name="key">The identifier of the document to edit, or empty to add a new one.</param>
        /// <returns>A box containing the document information.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            var contextEntity = GetContextEntityCached();

            if ( contextEntity == null )
            {
                return ActionBadRequest( "No context entity is available for documents." );
            }

            DocumentBag bag;

            if ( key.IsNullOrWhiteSpace() )
            {
                // Adding a new document.
                bag = new DocumentBag
                {
                    IsDocumentTypeReadOnly = false,
                    AvailableDocumentTypes = GetAddableDocumentTypeBags()
                };
            }
            else
            {
                var document = new DocumentService( RockContext ).Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( document == null )
                {
                    return ActionBadRequest( "Document not found." );
                }

                if ( !CanEditDocument( document, RequestContext.CurrentPerson ) )
                {
                    return ActionForbidden( "Not authorized to edit this document." );
                }

                var documentType = DocumentTypeCache.Get( document.DocumentTypeId );

                bag = new DocumentBag
                {
                    IdKey = document.IdKey,
                    Name = document.Name,
                    Description = document.Description,
                    DocumentType = documentType != null
                        ? new ListItemBag { Value = documentType.Guid.ToString(), Text = documentType.Name }
                        : null,
                    BinaryFile = document.BinaryFile != null
                        ? new ListItemBag { Value = document.BinaryFile.Guid.ToString(), Text = document.BinaryFile.FileName }
                        : null,
                    BinaryFileTypeGuid = documentType != null ? BinaryFileTypeCache.Get( documentType.BinaryFileTypeId )?.Guid : null,
                    IsDocumentTypeReadOnly = true
                };
            }

            return ActionOk( new ValidPropertiesBox<DocumentBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Saves the document represented by the supplied box.
        /// </summary>
        /// <param name="box">The box containing the document information.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<DocumentBag> box )
        {
            var contextEntity = GetContextEntityCached();

            if ( contextEntity == null )
            {
                return ActionBadRequest( "No context entity is available for documents." );
            }

            var bag = box?.Bag;

            if ( bag == null )
            {
                return ActionBadRequest( "No document data was provided." );
            }

            var binaryFileGuid = bag.BinaryFile?.Value.AsGuidOrNull();

            if ( !binaryFileGuid.HasValue )
            {
                return ActionBadRequest( "A document file is required." );
            }

            var documentService = new DocumentService( RockContext );
            var binaryFileService = new BinaryFileService( RockContext );
            var currentPerson = RequestContext.CurrentPerson;

            var binaryFile = binaryFileService.Get( binaryFileGuid.Value );

            if ( binaryFile == null )
            {
                return ActionBadRequest( "The uploaded file could not be found." );
            }

            Document document;

            if ( bag.IdKey.IsNotNullOrWhiteSpace() )
            {
                // Editing an existing document. Its type cannot be changed.
                document = documentService.Get( bag.IdKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( document == null )
                {
                    return ActionBadRequest( "Document not found." );
                }

                if ( !CanEditDocument( document, currentPerson ) )
                {
                    return ActionForbidden( "Not authorized to edit this document." );
                }
            }
            else
            {
                // Adding a new document.
                var documentTypeGuid = bag.DocumentType?.Value.AsGuidOrNull();
                var documentType = documentTypeGuid.HasValue ? DocumentTypeCache.Get( documentTypeGuid.Value ) : null;

                if ( documentType == null )
                {
                    return ActionBadRequest( "A valid document type is required." );
                }

                if ( !documentType.IsAuthorized( Authorization.EDIT, currentPerson ) )
                {
                    return ActionForbidden( "Not authorized to add a document of this type." );
                }

                document = new Document
                {
                    DocumentTypeId = documentType.Id
                };

                documentService.Add( document );
            }

            document.EntityId = contextEntity.Id;
            document.Name = bag.Name;
            document.Description = bag.Description;
            document.SetBinaryFile( binaryFile.Id, RockContext );

            if ( !document.IsValidDocument( RockContext, out var errorMessage ) )
            {
                // Surface every validation message, not just the first one.
                var validationMessage = document.ValidationResults
                    .Select( result => result.ErrorMessage )
                    .Where( message => message.IsNotNullOrWhiteSpace() )
                    .ToList()
                    .AsDelimited( "\n" );

                return ActionBadRequest( validationMessage.IsNotNullOrWhiteSpace() ? validationMessage : errorMessage );
            }

            RockContext.SaveChanges();

            // Re-parent the file to the document so it adopts the document's
            // security and is no longer treated as a temporary upload.
            document.BinaryFile.ParentEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.DOCUMENT );
            document.BinaryFile.ParentEntityId = document.Id;
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Deletes the specified document.
        /// </summary>
        /// <param name="key">The identifier of the document to delete.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var documentService = new DocumentService( RockContext );
            var document = documentService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( document == null )
            {
                return ActionBadRequest( "Document not found." );
            }

            if ( !CanEditDocument( document, RequestContext.CurrentPerson ) )
            {
                return ActionForbidden( "Not authorized to delete this document." );
            }

            if ( !documentService.CanDelete( document, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            documentService.Delete( document );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion Block Actions
    }
}
