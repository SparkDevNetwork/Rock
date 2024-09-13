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
using System.Linq;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.ElectronicSignature;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.SignatureDocumentDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays the details of a particular signature document.
    /// </summary>

    [DisplayName( "Signature Document Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of a particular signature document." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "bca3d113-8a98-4757-8471-a737011226a9" )]
    [Rock.SystemGuid.BlockTypeGuid( "b80e8563-41f2-4528-81e5-c62cf1ece9de" )]
    public class SignatureDocumentDetail : RockEntityDetailBlockType<SignatureDocument, SignatureDocumentBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string SignatureDocumentId = "SignatureDocumentId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<SignatureDocumentBag, SignatureDocumentDetailOptionsBag>();

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
        private SignatureDocumentDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new SignatureDocumentDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the SignatureDocument for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="signatureDocument">The SignatureDocument to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the SignatureDocument is valid, <c>false</c> otherwise.</returns>
        private bool ValidateSignatureDocument( SignatureDocument signatureDocument, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<SignatureDocumentBag, SignatureDocumentDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {SignatureDocument.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( SignatureDocument.FriendlyTypeName );
                }
            }
            else
            {
                // Creating Signature Documents in this way is not allowed
                box.ErrorMessage = EditModeMessage.ReadOnlyEditActionNotAllowed(SignatureDocument.FriendlyTypeName );
                //// New entity is being created, prepare for edit mode by default.
                //if ( box.IsEditable )
                //{
                //    box.Entity = GetEntityBagForEdit( entity );
                //}
                //else
                //{
                //    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( SignatureDocument.FriendlyTypeName );
                //}
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="SignatureDocumentBag"/> that represents the entity.</returns>
        private SignatureDocumentBag GetCommonEntityBag( SignatureDocument entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new SignatureDocumentBag
            {
                IdKey = entity.IdKey,
                AppliesToPersonAlias = entity.AppliesToPersonAlias.ToListItemBag(),
                AppliesToPersonAliasId = entity.AppliesToPersonAliasId,
                AssignedToPersonAlias = entity.AssignedToPersonAlias.ToListItemBag(),
                AssignedToPersonAliasId = entity.AssignedToPersonAliasId,
                BinaryFile = entity.BinaryFile.ToListItemBag(),
                BinaryFileId = entity.BinaryFileId,
                CompletionEmailSentDateTime = entity.CompletionEmailSentDateTime,
                DocumentKey = entity.DocumentKey,
                EntityId = entity.EntityId,
                EntityType = entity.EntityType.ToListItemBag(),
                EntityTypeId = entity.EntityTypeId,
                InviteCount = entity.InviteCount,
                LastInviteDate = entity.LastInviteDate,
                LastStatusDate = entity.LastStatusDate,
                Name = entity.Name,
                SignatureDataEncrypted = entity.SignatureDataEncrypted,
                SignatureDocumentTemplate = entity.SignatureDocumentTemplate.ToListItemBag(),
                SignatureDocumentTemplateId = entity.SignatureDocumentTemplateId,
                SignatureVerificationHash = entity.SignatureVerificationHash,
                SignedByEmail = entity.SignedByEmail,
                SignedByPersonAlias = entity.SignedByPersonAlias.ToListItemBag(),
                SignedByPersonAliasId = entity.SignedByPersonAliasId,
                SignedClientIp = entity.SignedClientIp,
                SignedClientUserAgent = entity.GetFormattedUserAgent().ConvertCrLfToHtmlBr(),
                SignedDateTime = entity.SignedDateTime,
                SignedDocumentText = entity.SignedDocumentText,
                SignedName = entity.SignedName,
                Status = entity.Status
            };
        }

        /// <inheritdoc/>
        protected override SignatureDocumentBag GetEntityBagForView( SignatureDocument entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        //// <inheritdoc/>
        protected override SignatureDocumentBag GetEntityBagForEdit( SignatureDocument entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( SignatureDocument entity, ValidPropertiesBox<SignatureDocumentBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }
            RockContext rockContext = new RockContext();

            box.IfValidProperty( nameof( box.Bag.AppliesToPersonAlias ),
                () => entity.AppliesToPersonAliasId = box.Bag.AppliesToPersonAlias.GetEntityId<PersonAlias>( rockContext ) );

            box.IfValidProperty( nameof( box.Bag.AppliesToPersonAliasId ),
                () => entity.AppliesToPersonAliasId = box.Bag.AppliesToPersonAliasId );

            box.IfValidProperty( nameof( box.Bag.AssignedToPersonAlias ),
                () => entity.AssignedToPersonAliasId = box.Bag.AssignedToPersonAlias.GetEntityId<PersonAlias>( rockContext ) );

            box.IfValidProperty( nameof( box.Bag.AssignedToPersonAliasId ),
                () => entity.AssignedToPersonAliasId = box.Bag.AssignedToPersonAliasId );

            box.IfValidProperty( nameof( box.Bag.BinaryFile ),
                () => entity.BinaryFileId = box.Bag.BinaryFile.GetEntityId<BinaryFile>( rockContext ) );

            box.IfValidProperty( nameof( box.Bag.BinaryFileId ),
                () => entity.BinaryFileId = box.Bag.BinaryFileId );

            box.IfValidProperty( nameof( box.Bag.CompletionEmailSentDateTime ),
                () => entity.CompletionEmailSentDateTime = box.Bag.CompletionEmailSentDateTime );

            box.IfValidProperty( nameof( box.Bag.DocumentKey ),
                () => entity.DocumentKey = box.Bag.DocumentKey );

            box.IfValidProperty( nameof( box.Bag.EntityId ),
                () => entity.EntityId = box.Bag.EntityId );

            box.IfValidProperty( nameof( box.Bag.EntityType ),
                () => entity.EntityTypeId = box.Bag.EntityType.GetEntityId<EntityType>( rockContext ) );

            box.IfValidProperty( nameof( box.Bag.EntityTypeId ),
                () => entity.EntityTypeId = box.Bag.EntityTypeId );

            box.IfValidProperty( nameof( box.Bag.InviteCount ),
                () => entity.InviteCount = box.Bag.InviteCount );

            box.IfValidProperty( nameof( box.Bag.LastInviteDate ),
                () => entity.LastInviteDate = box.Bag.LastInviteDate );

            box.IfValidProperty( nameof( box.Bag.LastStatusDate ),
                () => entity.LastStatusDate = box.Bag.LastStatusDate );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.SignatureDataEncrypted ),
                () => entity.SignatureDataEncrypted = box.Bag.SignatureDataEncrypted );

            box.IfValidProperty( nameof( box.Bag.SignatureDocumentTemplate ),
                () => entity.SignatureDocumentTemplateId = box.Bag.SignatureDocumentTemplate.GetEntityId<SignatureDocumentTemplate>( rockContext ).Value );

            box.IfValidProperty( nameof( box.Bag.SignatureDocumentTemplateId ),
                () => entity.SignatureDocumentTemplateId = box.Bag.SignatureDocumentTemplateId );

            box.IfValidProperty( nameof( box.Bag.SignatureVerificationHash ),
                () => entity.SignatureVerificationHash = box.Bag.SignatureVerificationHash );

            box.IfValidProperty( nameof( box.Bag.SignedByEmail ),
                () => entity.SignedByEmail = box.Bag.SignedByEmail );

            box.IfValidProperty( nameof( box.Bag.SignedByPersonAlias ),
                () => entity.SignedByPersonAliasId = box.Bag.SignedByPersonAlias.GetEntityId<PersonAlias>( rockContext ) );

            box.IfValidProperty( nameof( box.Bag.SignedByPersonAliasId ),
                () => entity.SignedByPersonAliasId = box.Bag.SignedByPersonAliasId );

            box.IfValidProperty( nameof( box.Bag.SignedClientIp ),
                () => entity.SignedClientIp = box.Bag.SignedClientIp );

            box.IfValidProperty( nameof( box.Bag.SignedClientUserAgent ),
                () => entity.SignedClientUserAgent = box.Bag.SignedClientUserAgent );

            box.IfValidProperty( nameof( box.Bag.SignedDateTime ),
                () => entity.SignedDateTime = box.Bag.SignedDateTime );

            box.IfValidProperty( nameof( box.Bag.SignedDocumentText ),
                () => entity.SignedDocumentText = box.Bag.SignedDocumentText );

            box.IfValidProperty( nameof( box.Bag.SignedName ),
                () => entity.SignedName = box.Bag.SignedName );

            box.IfValidProperty( nameof( box.Bag.Status ),
                () => entity.Status = box.Bag.Status );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override SignatureDocument GetInitialEntity()
        {
            return GetInitialEntity<SignatureDocument, SignatureDocumentService>( RockContext, PageParameterKey.SignatureDocumentId );
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
        protected override bool TryGetEntityForEditAction( string idKey, out SignatureDocument entity, out BlockActionResult error )
        {
            var entityService = new SignatureDocumentService( RockContext );
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
                entity = new SignatureDocument();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{SignatureDocument.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${SignatureDocument.FriendlyTypeName}." );
                return false;
            }

            return true;
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

            return ActionOk( new ValidPropertiesBox<SignatureDocumentBag>
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
        public BlockActionResult Save( ValidPropertiesBox<SignatureDocumentBag> box )
        {
            var entityService = new SignatureDocumentService( RockContext );

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
            if ( !ValidateSignatureDocument( entity, out var validationMessage ) )
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
                    [PageParameterKey.SignatureDocumentId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<SignatureDocumentBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        ///// <summary>
        ///// Deletes the specified entity.
        ///// </summary>
        ///// <param name="key">The identifier of the entity to be deleted.</param>
        ///// <returns>A string that contains the URL to be redirected to on success.</returns>
        //[BlockAction]
        //public BlockActionResult Delete( string key )
        //{
        //    var entityService = new SignatureDocumentService( RockContext );

        //    if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
        //    {
        //        return actionError;
        //    }

        //    if ( !entityService.CanDelete( entity, out var errorMessage ) )
        //    {
        //        return ActionBadRequest( errorMessage );
        //    }

        //    entityService.Delete( entity );
        //    RockContext.SaveChanges();

        //    return ActionOk( this.GetParentPageUrl() );
        //}

        /// <summary>
        /// </summary>
        [BlockAction]
        public BlockActionResult ResendCompletionEmail( string key )
        {
            var entityService = new SignatureDocumentService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            List<string> errorMessages;
            bool successfullySent = ElectronicSignatureHelper.SendSignatureCompletionCommunication( entity.Id, out errorMessages );

            if ( successfullySent )
            {
                return ActionOk();
            }
            else
            {
                return ActionBadRequest( String.Join( ", ", errorMessages ) );
            }
        }
        #endregion
    }
}
