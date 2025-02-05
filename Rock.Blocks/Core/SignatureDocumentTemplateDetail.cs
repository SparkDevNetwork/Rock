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

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.ElectronicSignature;
using Rock.Lava;
using Rock.Model;
using Rock.Pdf;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.SignatureDocumentTemplateDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays the details of a particular signature document template.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />
    [DisplayName( "Signature Document Template Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of a particular signature document template." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [BinaryFileTypeField(

        "Default File Type",
        Description = "The default file type to use when creating new documents.",
        Key = AttributeKey.DefaultFileType,
        IsRequired = false,
        DefaultBinaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.SIGNED_DOCUMENT_FILE_TYPE,
        Order = 0 )]

    [BooleanField(
        "Show Legacy Signature Providers",
        "Enable this setting to see the configuration for legacy signature providers. Note that support for these providers will be fully removed in the next full release.",
        Key = AttributeKey.ShowLegacyExternalProviders,
        DefaultBooleanValue = false,
        Order = 1 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "525b6687-964e-4051-94a5-4b20d4575041" )]
    [Rock.SystemGuid.BlockTypeGuid( "e6a5bac5-c34c-421a-b536-eec3d9f1d1b5" )]
    public class SignatureDocumentTemplateDetail : RockEntityDetailBlockType<SignatureDocumentTemplate, SignatureDocumentTemplateBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string SignatureDocumentTemplateId = "SignatureDocumentTemplateId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        public static class AttributeKey
        {
            public const string DefaultFileType = "DefaultFileType";
            public const string ShowLegacyExternalProviders = "ShowLegacyExternalProviders";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<SignatureDocumentTemplateBag, SignatureDocumentTemplateDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( box.IsEditable );

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private SignatureDocumentTemplateDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new SignatureDocumentTemplateDetailOptionsBag
            {
                CommunicationTemplates = GetCommunicationTemplates(),
                ShowLegacyExternalProviders = GetAttributeValue( AttributeKey.ShowLegacyExternalProviders ).AsBoolean()
            };
            return options;
        }

        /// <summary>
        /// Validates the SignatureDocumentTemplate for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="signatureDocumentTemplate">The SignatureDocumentTemplate to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the SignatureDocumentTemplate is valid, <c>false</c> otherwise.</returns>
        private bool ValidateSignatureDocumentTemplate( SignatureDocumentTemplate signatureDocumentTemplate, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<SignatureDocumentTemplateBag, SignatureDocumentTemplateDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {SignatureDocumentTemplate.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( RockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( SignatureDocumentTemplate.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( SignatureDocumentTemplate.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="SignatureDocumentTemplateBag"/> that represents the entity.</returns>
        private SignatureDocumentTemplateBag GetCommonEntityBag( SignatureDocumentTemplate entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new SignatureDocumentTemplateBag
            {
                IdKey = entity.IdKey,
                BinaryFileType = entity.BinaryFileType.ToListItemBag(),
                CompletionSystemCommunication = entity.CompletionSystemCommunication.ToListItemBag(),
                Description = entity.Description,
                DocumentTerm = entity.DocumentTerm,
                IsActive = entity.IsActive,
                LavaTemplate = entity.LavaTemplate,
                Name = entity.Name,
                ProviderEntityType = entity.ProviderEntityType.ToListItemBag(),
                ProviderTemplateKey = entity.ProviderTemplateKey,
                SignatureInputTypes = GetSignatureInputTypes(),
                SignatureType = entity.SignatureType.ConvertToStringSafe(),
                IsValidInFuture = entity.IsValidInFuture,
                ValidityDurationInDays = entity.ValidityDurationInDays,
            };
        }

        /// <summary>
        /// Gets the signature input types as a List of ListBag items for signature type radio button list.
        /// </summary>
        /// <returns></returns>
        private List<ListItemBag> GetSignatureInputTypes()
        {
            var signatureInputTypes = new List<ListItemBag>();
            var names = Enum.GetNames( typeof( SignatureType ) );
            foreach ( var name in names )
            {
                signatureInputTypes.Add( new ListItemBag() { Text = name, Value = name } );
            }

            return signatureInputTypes;
        }

        /// <inheritdoc/>
        protected override SignatureDocumentTemplateBag GetEntityBagForView( SignatureDocumentTemplate entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: true );
            bag.CanAdministrate = BlockCache.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, GetCurrentPerson() );

            return bag;
        }

        /// <inheritdoc/>
        protected override SignatureDocumentTemplateBag GetEntityBagForEdit( SignatureDocumentTemplate entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: true );

            if ( entity.Id == 0 )
            {
                Guid? fileTypeGuid = GetAttributeValue( AttributeKey.DefaultFileType ).AsGuidOrNull();
                if ( fileTypeGuid.HasValue )
                {
                    var binaryFileType = new BinaryFileTypeService( RockContext ).Get( fileTypeGuid.Value );
                    if ( binaryFileType != null )
                    {
                        bag.BinaryFileType = binaryFileType.ToListItemBag();
                        entity.BinaryFileTypeId = binaryFileType.Id;
                    }
                }

                bag.LavaTemplate = SignatureDocumentTemplate.DefaultLavaTemplate;
            }

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( SignatureDocumentTemplate entity, ValidPropertiesBox<SignatureDocumentTemplateBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.BinaryFileType ),
                () => entity.BinaryFileTypeId = box.Bag.BinaryFileType.GetEntityId<BinaryFileType>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.CompletionSystemCommunication ),
                () => entity.CompletionSystemCommunicationId = box.Bag.CompletionSystemCommunication.GetEntityId<SystemCommunication>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.DocumentTerm ),
                () => entity.DocumentTerm = box.Bag.DocumentTerm );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.LavaTemplate ),
                () => entity.LavaTemplate = box.Bag.LavaTemplate );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.ProviderEntityType ),
                () => entity.ProviderEntityTypeId = box.Bag.ProviderEntityType.GetEntityId<EntityType>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.ProviderTemplateKey ),
                () => entity.ProviderTemplateKey = box.Bag.ProviderTemplateKey );

            box.IfValidProperty( nameof( box.Bag.SignatureType ),
                () => entity.SignatureType = box.Bag.SignatureType.ConvertToEnum<SignatureType>() );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: true );
                } );

            box.IfValidProperty( nameof( box.Bag.IsValidInFuture ),
                    () => entity.IsValidInFuture = box.Bag.IsValidInFuture );

            box.IfValidProperty( nameof( box.Bag.ValidityDurationInDays ),
                    () => entity.ValidityDurationInDays = box.Bag.ValidityDurationInDays );

            return true;
        }

        /// <inheritdoc/>
        protected override SignatureDocumentTemplate GetInitialEntity()
        {
            return GetInitialEntity<SignatureDocumentTemplate, SignatureDocumentTemplateService>( RockContext, PageParameterKey.SignatureDocumentTemplateId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out SignatureDocumentTemplate entity, out BlockActionResult error )
        {
            var entityService = new SignatureDocumentTemplateService( RockContext );
            error = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                // Create a new entity.
                entity = new SignatureDocumentTemplate();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{SignatureDocumentTemplate.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${SignatureDocumentTemplate.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the communication templates.
        /// </summary>
        /// <returns></returns>
        private List<ListItemBag> GetCommunicationTemplates()
        {
            var communicationTemplates = new List<ListItemBag>();
            foreach ( var systemEmail in new SystemCommunicationService( RockContext )
                .Queryable().AsNoTracking()
                .OrderBy( e => e.Title )
                .Select( e => new
                {
                    e.Guid,
                    e.Title
                } ) )
            {
                communicationTemplates.Add( new ListItemBag() { Text = systemEmail.Title, Value = systemEmail.Guid.ToString() } );
            }

            return communicationTemplates;
        }

        /// <summary>
        /// Gets the PDF preview URL.
        /// </summary>
        /// <param name="lavaTemplate">The lava template.</param>
        /// <param name="binaryFileTypeId">The binary file type identifier.</param>
        /// <param name="signatureType">Type of the signature.</param>
        /// <returns></returns>
        private string GetPdfPreviewUrl( string lavaTemplate, int binaryFileTypeId, SignatureType signatureType = SignatureType.Typed )
        {
            var mergeFields = LavaHelper.GetCommonMergeFields( null );
            var signatureDocumentHtml = ElectronicSignatureHelper.GetSignatureDocumentHtml( lavaTemplate, mergeFields );
            var fakeRandomHash = Rock.Security.Encryption.GetSHA1Hash( Guid.NewGuid().ToString() );

            var signatureInformationHtml = ElectronicSignatureHelper.GetSignatureInformationHtml( new GetSignatureInformationHtmlOptions
            {
                SignatureType = signatureType,
                DrawnSignatureDataUrl = ElectronicSignatureHelper.SampleSignatureDataURL,
                SignedByPerson = GetCurrentPerson(),
                SignedDateTime = RockDateTime.Now,
                SignedClientIp = this.RequestContext.ClientInformation.IpAddress,
                SignedName = this.GetCurrentPerson()?.FullName,
                SignatureVerificationHash = fakeRandomHash
            } );

            string pdfPreviewUrl;

            using ( var pdfGenerator = new PdfGenerator() )
            {
                var signedDocumentHtml = ElectronicSignatureHelper.GetSignedDocumentHtml( signatureDocumentHtml, signatureInformationHtml );

                // put the pdf into a BinaryFile. We'll mark it IsTemporary so it'll eventually get cleaned up by RockCleanup
                BinaryFile binaryFile = pdfGenerator.GetAsBinaryFileFromHtml( binaryFileTypeId, "preview.pdf", signedDocumentHtml );
                binaryFile.IsTemporary = true;

                using ( var rockContext = new RockContext() )
                {
                    new BinaryFileService( rockContext ).Add( binaryFile );
                    rockContext.SaveChanges();
                }

                pdfPreviewUrl = string.Format( "{0}/GetFile.ashx?guid={1}", this.RequestContext.RootUrlPath, binaryFile.Guid );
            }

            return pdfPreviewUrl;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<SignatureDocumentTemplateBag>()
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<SignatureDocumentTemplateBag> box )
        {
            var entityService = new SignatureDocumentTemplateService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidateSignatureDocumentTemplate( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
            } );

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.SignatureDocumentTemplateId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForView( entity );

            return ActionOk( new ValidPropertiesBox<SignatureDocumentTemplateBag>()
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new SignatureDocumentTemplateService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl() );
        }

        /// <summary>
        /// Gets the PDF preview URL.
        /// </summary>
        /// <param name="requestBag">The request bag.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetPdfPreviewUrl( GetPdfPreviewUrlRequestBag requestBag )
        {
            using ( var rockContext = new RockContext() )
            {
                SignatureType signatureType = requestBag.SignatureType.IsNotNullOrWhiteSpace() ? requestBag.SignatureType.ConvertToEnum<SignatureType>() : SignatureType.Typed;

                var url = GetPdfPreviewUrl( requestBag.LavaTemplate, requestBag.BinaryFileType.GetEntityId<BinaryFileType>( rockContext ) ?? 0, signatureType );

                return ActionOk( new { PreviewUrl = url } );
            }
        }

        /// <summary>
        /// Gets the external providers.
        /// </summary>
        /// <param name="entityTypeGuid">The entity type unique identifier.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetExternalProviders( Guid? entityTypeGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var externalProviders = new List<ListItemBag>();
                var errorMessage = string.Empty;

                if ( !entityTypeGuid.HasValue )
                {
                    return ActionOk(new { externalProviders = externalProviders } );
                }

                var entityType = EntityTypeCache.Get( entityTypeGuid.Value );

                if ( entityType == null )
                {
                    return ActionOk( new { externalProviders = externalProviders } );
                }

                var component = DigitalSignatureContainer.GetComponent( entityType.Name );
                if ( component == null )
                {
                    return ActionOk( new { externalProviders = externalProviders } );
                }

                var errors = new List<string>();
                var templates = component.GetTemplates( out errors );

                if ( templates != null )
                {
                    foreach ( var keyVal in templates.OrderBy( d => d.Value ) )
                    {
                        externalProviders.Add( new ListItemBag() { Text = keyVal.Value, Value = keyVal.Key } );
                    }

                    return ActionOk( new { externalProviders = externalProviders } );
                }
                else
                {
                    errorMessage = string.Format( "<ul><li>{0}</li></ul>", errors.AsDelimited( "</li><li>" ) );

                    return ActionBadRequest( errorMessage );
                }
            }
        }

        #endregion
    }
}
