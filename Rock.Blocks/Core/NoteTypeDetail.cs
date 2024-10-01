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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Constants;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.NoteTypeDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.Cache.Entities;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays the details of a particular note type.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />

    [DisplayName( "Note Type Detail" )]
    [Category( "Core" )]
    [Description( "Displays the details of a particular note type." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "a664c469-985c-4747-80cd-e07501d13f43" )]
    [Rock.SystemGuid.BlockTypeGuid( "9e901a5a-82c2-4788-9623-3720ffc4daec" )]
    public class NoteTypeDetail : RockEntityDetailBlockType<NoteType, NoteTypeBag>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string NoteTypeId = "NoteTypeId";
            public const string EntityTypeId = "EntityTypeId";
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
            var box = new DetailBlockBox<NoteTypeBag, NoteTypeDetailOptionsBag>();

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
        private NoteTypeDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new NoteTypeDetailOptionsBag();

            options.HasActiveAIProviders = AIProviderCache.All( this.RockContext ).Any( a => a.IsActive );

            return options;
        }

        /// <summary>
        /// Validates the NoteType for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="noteType">The NoteType to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the NoteType is valid, <c>false</c> otherwise.</returns>
        private bool ValidateNoteType( NoteType noteType, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<NoteTypeBag, NoteTypeDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {NoteType.FriendlyTypeName} was not found.";
                return;
            }

            if ( entity.Id == 0 )
            {
                entity.FormatType = Enums.Core.NoteFormatType.Structured;
            }

            var isViewable = entity.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( this.RockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( NoteType.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( NoteType.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="NoteTypeBag"/> that represents the entity.</returns>
        private NoteTypeBag GetCommonEntityBag( NoteType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new NoteTypeBag
            {
                IdKey = entity.IdKey,
                AllowsAttachments = entity.AllowsAttachments,
                AllowsReplies = entity.AllowsReplies,
                AllowsWatching = entity.AllowsWatching,
                AutoWatchAuthors = entity.AutoWatchAuthors,
                BinaryFileType = entity.BinaryFileType.ToListItemBag(),
                Color = entity.Color,
                EntityType = new ViewModels.Utility.ListItemBag() { Text = entity.EntityType?.FriendlyName, Value = entity.EntityType?.Guid.ToString() },
                FormatType = entity.FormatType,
                IconCssClass = entity.IconCssClass,
                IsMentionEnabled = entity.IsMentionEnabled,
                IsSystem = entity.IsSystem,
                MaxReplyDepth = entity.MaxReplyDepth.ToStringSafe(),
                Name = entity.Name,
                RequiresApprovals = entity.RequiresApprovals,
                UserSelectable = entity.UserSelectable,
                ShowEntityTypePicker = true
            };
        }

        /// <inheritdoc/>
        protected override NoteTypeBag GetEntityBagForView( NoteType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <inheritdoc/>
        protected override NoteTypeBag GetEntityBagForEdit( NoteType entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            var aiApprovalSettings = entity.GetAdditionalSettings<NoteType.AIApprovalSettings>();
            bag.EnabledAIApprovals = aiApprovalSettings.EnabledAIApprovals;
            bag.AIApprovalGuidelines = aiApprovalSettings.AIApprovalGuidelines;
            var aiProvider = aiApprovalSettings.AIProviderId.HasValue ? AIProviderCache.Get( aiApprovalSettings.AIProviderId.Value ) : null;
            bag.AIProvider = aiProvider.ToListItemBag();

            int? noteTypeId = this.PageParameter( PageParameterKey.NoteTypeId ).AsIntegerOrNull();
            if ( noteTypeId.HasValue )
            {
                int? entityTypeId = PageParameter( PageParameterKey.EntityTypeId ).AsIntegerOrNull();

                if ( entityTypeId.HasValue )
                {
                    var entityType = EntityTypeCache.Get( entityTypeId.Value );
                    bag.EntityType = new ViewModels.Utility.ListItemBag() { Text = entityType?.FriendlyName, Value = entityType?.Guid.ToString() };
                    bag.ShowEntityTypePicker = false;
                }
            }

            if ( entity.IsSystem )
            {
                bag.ShowEntityTypePicker = false;
            }
            else
            {
                if ( entity.Id > 0 )
                {
                    bool hasNotes = new NoteService( this.RockContext ).Queryable().Any( a => a.NoteTypeId == entity.Id );
                    if ( hasNotes )
                    {
                        bag.ShowEntityTypePicker = true;
                    }
                }
            }

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( NoteType entity, ValidPropertiesBox<NoteTypeBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.AllowsAttachments ),
                () => entity.AllowsAttachments = box.Bag.AllowsAttachments );

            box.IfValidProperty( nameof( box.Bag.AllowsReplies ),
                () => entity.AllowsReplies = box.Bag.AllowsReplies );

            box.IfValidProperty( nameof( box.Bag.AllowsWatching ),
                () => entity.AllowsWatching = box.Bag.AllowsWatching );

            box.IfValidProperty( nameof( box.Bag.AutoWatchAuthors ),
                () => entity.AutoWatchAuthors = box.Bag.AutoWatchAuthors );

            box.IfValidProperty( nameof( box.Bag.BinaryFileType ),
                () => entity.BinaryFileTypeId = box.Bag.AllowsAttachments ? box.Bag.BinaryFileType.GetEntityId<BinaryFileType>( this.RockContext ) : null );

            box.IfValidProperty( nameof( box.Bag.Color ),
                () => entity.Color = box.Bag.Color );

            box.IfValidProperty( nameof( box.Bag.EntityType ),
                () => entity.EntityTypeId = box.Bag.EntityType.GetEntityId<EntityType>( this.RockContext ).Value );

            box.IfValidProperty( nameof( box.Bag.FormatType ),
                () => entity.FormatType = box.Bag.FormatType );

            box.IfValidProperty( nameof( box.Bag.IconCssClass ),
                () => entity.IconCssClass = box.Bag.IconCssClass );

            box.IfValidProperty( nameof( box.Bag.IsMentionEnabled ),
                () => entity.IsMentionEnabled = box.Bag.IsMentionEnabled );

            box.IfValidProperty( nameof( box.Bag.MaxReplyDepth ),
                () => entity.MaxReplyDepth = int.TryParse( box.Bag.MaxReplyDepth, out int maxReplyDepth ) ? maxReplyDepth : ( int? ) null );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.UserSelectable ),
                () => entity.UserSelectable = box.Bag.UserSelectable );

            box.IfValidProperty( nameof( box.Bag.RequiresApprovals ), () =>
            {
                entity.RequiresApprovals = box.Bag.RequiresApprovals;
            } );

            var aiApprovalSettings = entity.GetAdditionalSettings<NoteType.AIApprovalSettings>();

            box.IfValidProperty( nameof( box.Bag.EnabledAIApprovals ), () =>
            {
                aiApprovalSettings.EnabledAIApprovals = box.Bag.EnabledAIApprovals;
            } );

            // If AI Approvals are not enabled do not save an AI Provider Id.
            var aiProviderId = aiApprovalSettings.EnabledAIApprovals ? AIProviderCache.GetId( ( box.Bag.AIProvider?.Value ).AsGuid() ) : null;
            box.IfValidProperty( nameof( box.Bag.AIProvider ), () =>
            {
                aiApprovalSettings.AIProviderId = aiProviderId;
            } );

            // If AI Approvals are not enabled do not save AI Approval Guidelines.
            var guidelines = aiApprovalSettings.EnabledAIApprovals ? box.Bag.AIApprovalGuidelines : string.Empty;
            box.IfValidProperty( nameof( box.Bag.AIApprovalGuidelines ), () =>
            {
                aiApprovalSettings.AIApprovalGuidelines = guidelines;
            } );

            entity.SetAdditionalSettings( aiApprovalSettings );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( this.RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <inheritdoc />
        protected override NoteType GetInitialEntity()
        {
            return GetInitialEntity<NoteType, NoteTypeService>( this.RockContext, PageParameterKey.NoteTypeId );
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
        protected override bool TryGetEntityForEditAction( string idKey, out NoteType entity, out BlockActionResult error )
        {
            var entityService = new NoteTypeService( this.RockContext );
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
                entity = new NoteType();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{NoteType.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${NoteType.FriendlyTypeName}." );
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

            entity.LoadAttributes( this.RockContext );

            var bag = GetEntityBagForEdit( entity );
            var box = new ValidPropertiesBox<NoteTypeBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            };

            return ActionOk( box );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<NoteTypeBag> box )
        {
            var entityService = new NoteTypeService( this.RockContext );

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
            if ( !ValidateNoteType( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            if ( isNew )
            {
                int? maxNoteTypeOrderForEntity = entityService.Queryable().Where( t => t.EntityTypeId == entity.EntityTypeId ).Max( a => ( int? ) a.Order );
                entity.Order = ( maxNoteTypeOrderForEntity ?? 0 ) + 1;
            }

            this.RockContext.WrapTransaction( () =>
            {
                this.RockContext.SaveChanges();
                entity.SaveAttributeValues( this.RockContext );
            } );

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.NoteTypeId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( this.RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<NoteTypeBag>
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
            var entityService = new NoteTypeService( this.RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            this.RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl() );
        }

        #endregion
    }
}
