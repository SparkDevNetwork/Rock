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
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Communication.SnippetDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Displays the details of a particular snippet.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />
    [DisplayName( "Snippet Detail" )]
    [Category( "Communication" )]
    [Description( "Displays the details of a particular snippet." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [CustomDropdownListField( "Snippet Type",
        Description = "Determines what type of snippet to filter on. This is required (only one type can be displayed at a time).",
        ListSource = "SELECT [Guid] as [Value], [Name] as [Text] FROM [SnippetType]",
        IsRequired = true,
        Key = AttributeKey.SnippetType,
        Order = 0 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "4b445492-20e7-41e3-847a-f4d4723e9973" )]
    [Rock.SystemGuid.BlockTypeGuid( "8b0f3048-99ba-4ed1-8de6-6a34f498f556" )]
    public class SnippetDetail : RockEntityDetailBlockType<Snippet, SnippetBag>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string SnippetType = "SnippetType";
        }

        private static class PageParameterKey
        {
            public const string SnippetId = "SnippetId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// Local cache of the snippet type, should be accessed via the <see cref="GetSnippetType(RockContext)"/> method.
        /// </summary>
        private SnippetType _snippetType;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<SnippetBag, SnippetDetailOptionsBag>();

                SetBoxInitialEntityState( box );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable );

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private SnippetDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new SnippetDetailOptionsBag();

            var snippetType = GetSnippetType();
            options.IsAuthorizedToEdit = snippetType?.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) ?? false;
            options.IsPersonalAllowed = snippetType?.IsPersonalAllowed ?? false;

            return options;
        }

        /// <summary>
        /// Validates the Snippet for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="snippet">The Snippet to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the Snippet is valid, <c>false</c> otherwise.</returns>
        private bool ValidateSnippet( Snippet snippet, out string errorMessage )
        {
            errorMessage = null;

            if ( snippet.SnippetTypeId == 0 )
            {
                errorMessage = "Select a snippet type under the block settings.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<SnippetBag, SnippetDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {Snippet.FriendlyTypeName} was not found.";
                return;
            }

            var snippetType = GetSnippetType();
            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) || snippetType.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );

            entity.LoadAttributes( RockContext );

            if ( entity.Id != 0 )
            {
                box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                    box.Entity.CanAdministrate = !entity.OwnerPersonAliasId.HasValue
                        && ( entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) || snippetType?.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) == true );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( Snippet.FriendlyTypeName );
                }
            }
            else
            {
                box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) || snippetType.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Snippet.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="SnippetBag"/> that represents the entity.</returns>
        private SnippetBag GetCommonEntityBag( Snippet entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new SnippetBag
            {
                IdKey = entity.IdKey,
                Category = entity.Category.ToListItemBag(),
                Description = entity.Description,
                IsActive = entity.IsActive,
                Content = entity.Content,
                Name = entity.Name,
                Order = entity.Order,
                OwnerPersonAlias = entity.Id == 0 ? RequestContext.CurrentPerson.PrimaryAlias.ToListItemBag() : entity.OwnerPersonAlias.ToListItemBag()
            };
        }

        /// <inheritdoc/>
        protected override SnippetBag GetEntityBagForView( Snippet entity )
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
        protected override SnippetBag GetEntityBagForEdit( Snippet entity )
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
        protected override bool UpdateEntityFromBox( Snippet entity, ValidPropertiesBox<SnippetBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Category ),
                () => entity.CategoryId = box.Bag.Category.GetEntityId<Category>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.Content ),
                () => entity.Content = box.Bag.Content );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.Order ),
                () => entity.Order = box.Bag.Order );

            box.IfValidProperty( nameof( box.Bag.OwnerPersonAlias ),
                () => entity.OwnerPersonAliasId = box.Bag.OwnerPersonAlias.GetEntityId<PersonAlias>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson );
                } );

            var snippetType = GetSnippetType();
            if ( snippetType != null )
            {
                entity.SnippetTypeId = snippetType.Id;
            }

            return true;
        }

        /// <summary>
        /// Gets the snippet type.
        /// </summary>
        /// <returns></returns>
        private SnippetType GetSnippetType()
        {
            if ( _snippetType == null )
            {
                var snippetTypeGuid = GetAttributeValue( AttributeKey.SnippetType ).AsGuid();
                _snippetType = new SnippetTypeService( RockContext ).Get( snippetTypeGuid );
            }

            return _snippetType;
        }

        /// <inheritdoc/>
        protected override Snippet GetInitialEntity()
        {
            return GetInitialEntity<Snippet, SnippetService>( RockContext, PageParameterKey.SnippetId );
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
        protected override bool TryGetEntityForEditAction( string idKey, out Snippet entity, out BlockActionResult error )
        {
            var entityService = new SnippetService( RockContext );
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
                entity = new Snippet();
                entityService.Add( entity );

                var maxOrder = entityService.Queryable()
                    .Select( t => ( int? ) t.Order )
                    .Max();

                entity.Order = maxOrder.HasValue ? maxOrder.Value + 1 : 0;
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{Snippet.FriendlyTypeName} not found." );
                return false;
            }

            var snippetType = GetSnippetType();
            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) && snippetType?.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) == false )
            {
                error = ActionBadRequest( $"Not authorized to edit ${Snippet.FriendlyTypeName}." );
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

            return ActionOk( new ValidPropertiesBox<SnippetBag>
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
        public BlockActionResult Save( ValidPropertiesBox<SnippetBag> box )
        {
            var entityService = new SnippetService( RockContext );

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
            if ( !ValidateSnippet( entity, out var validationMessage ) )
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
                    [PageParameterKey.SnippetId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<SnippetBag>
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
            var entityService = new SnippetService( RockContext );

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

        #endregion
    }
}
