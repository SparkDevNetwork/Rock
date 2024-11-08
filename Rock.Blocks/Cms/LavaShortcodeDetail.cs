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
using System.Text;
using DotLiquid;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.LavaShortcodeDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays the details of a particular lava shortcode.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianDetailBlockType" />

    [DisplayName( "Lava Shortcode Detail" )]
    [Category( "CMS" )]
    [Description( "Displays the details of a particular lava shortcode." )]
    [IconCssClass( "fa fa-question" )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "76de9139-63c8-4e38-adfe-f38c1ef1021a" )]
    [Rock.SystemGuid.BlockTypeGuid( "3852e96a-9270-4c0e-a0d0-3cd9601f183e" )]
    public class LavaShortcodeDetail : RockDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string LavaShortcodeId = "LavaShortcodeId";
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
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<LavaShortcodeBag, LavaShortcodeDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<LavaShortcode>();

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private LavaShortcodeDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new LavaShortcodeDetailOptionsBag();
            options.TagTypes = Enum.GetNames( typeof( TagType ) ).Select( t => new ViewModels.Utility.ListItemBag() { Text = t, Value = t } ).ToList();
            return options;
        }

        /// <summary>
        /// Validates the LavaShortcode for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="lavaShortcode">The LavaShortcode to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the LavaShortcode is valid, <c>false</c> otherwise.</returns>
        private bool ValidateLavaShortcode( LavaShortcode lavaShortcode, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            var lavaShortCodeService = new LavaShortcodeService( rockContext );

            if ( lavaShortCodeService.Queryable().Any( a => a.TagName == lavaShortcode.TagName && a.Id != lavaShortcode.Id ) )
            {
                errorMessage = "Tag with the same name is already in use.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<LavaShortcodeBag, LavaShortcodeDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {LavaShortcode.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = entity.IsAuthorized( Rock.Security.Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( rockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( Snippet.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( Snippet.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="LavaShortcodeBag"/> that represents the entity.</returns>
        private LavaShortcodeBag GetCommonEntityBag( LavaShortcode entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new LavaShortcodeBag
            {
                IdKey = entity.IdKey,
                Categories = entity.Categories.ToListItemBagList(),
                Description = entity.Description,
                Documentation = entity.Documentation,
                IsActive = entity.IsActive,
                IsSystem = entity.IsSystem,
                Markup = entity.Markup,
                Name = entity.Name,
                TagName = entity.TagName,
                TagType = entity.TagType.ToString(),
            };

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="SnippetBag"/> that represents the entity.</returns>
        private LavaShortcodeBag GetEntityBagForView( LavaShortcode entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="SnippetBag"/> that represents the entity.</returns>
        private LavaShortcodeBag GetEntityBagForEdit( LavaShortcode entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );
            bag.EnabledCommands = entity.EnabledLavaCommands.SplitDelimitedValues().Select( lc => new ViewModels.Utility.ListItemBag() { Text = lc, Value = lc } ).ToList();
            bag.Parameters = GetParameterValues( entity.Parameters );

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( LavaShortcode entity, DetailBlockBox<LavaShortcodeBag, LavaShortcodeDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            if ( !entity.IsSystem )
            {
                box.IfValidProperty( nameof( box.Entity.Description ),
                    () => entity.Description = box.Entity.Description );

                box.IfValidProperty( nameof( box.Entity.Documentation ),
                    () => entity.Documentation = box.Entity.Documentation );

                box.IfValidProperty( nameof( box.Entity.IsActive ),
                    () => entity.IsActive = box.Entity.IsActive );

                box.IfValidProperty( nameof( box.Entity.Markup ),
                    () => entity.Markup = box.Entity.Markup );

                box.IfValidProperty( nameof( box.Entity.Name ),
                    () => entity.Name = box.Entity.Name );

                box.IfValidProperty( nameof( box.Entity.TagName ),
                    () => entity.TagName = box.Entity.TagName );

                box.IfValidProperty( nameof( box.Entity.TagType ),
                    () => entity.TagType = box.Entity.TagType.ConvertToEnum<TagType>() );

                box.IfValidProperty( nameof( box.Entity.IsActive ),
                    () => entity.IsActive = box.Entity.IsActive );

                box.IfValidProperty( nameof( box.Entity.Parameters ),
                    () => entity.Parameters = JoinParameters( box.Entity.Parameters ) );

                box.IfValidProperty( nameof( box.Entity.EnabledCommands ),
                    () => entity.EnabledLavaCommands = box.Entity.EnabledCommands.Select( p => p.Value ).JoinStrings( "," ) );
            }

            box.IfValidProperty( nameof( box.Entity.Categories ),
                () => UpdateCategories( rockContext, entity, box.Entity ) );

            // Since changes to Categories isn't tracked by ChangeTracker, set the ModifiedDateTime just in case Categories changed
            entity.ModifiedDateTime = RockDateTime.Now;

            box.IfValidProperty( nameof( box.Entity.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( rockContext );

                    entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <summary>
        /// Converts the parameters value from a list of ListItemBag items to a | delimited kpv sting
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private string JoinParameters( List<ListItemBag> parameters )
        {
            var sb = new StringBuilder();

            foreach ( var listItemBag in parameters )
            {
                sb.Append( listItemBag.Text ).Append( '^' ).Append( listItemBag.Value ).Append( '|' );
            }

            return sb.ToString().Trim( '|' );
        }

        /// <summary>
        /// Gets the Key Value pair values from the delimited string.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private List<ListItemBag> GetParameterValues( string parameters )
        {
            var listBagItems = new List<ListItemBag>();
            foreach ( var value in parameters.SplitDelimitedValues( "|" ) )
            {
                var kvp = value.SplitDelimitedValues( "^" );
                if ( kvp.Length == 2 )
                {
                    listBagItems.Add( new ListItemBag { Text = kvp[0], Value = kvp[1] } );
                }
                else
                {
                    listBagItems.Add( new ListItemBag { Text = kvp[0] } );
                }
            }

            return listBagItems;
        }

        /// <summary>
        /// Updates the categories.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="bag">The bag.</param>
        private void UpdateCategories( RockContext rockContext, LavaShortcode entity, LavaShortcodeBag bag )
        {
            entity.Categories.Clear();

            var categoryService = new CategoryService( rockContext );
            foreach ( var categoryGuid in bag.Categories.Select( c => c.Value.AsGuid() ) )
            {
                var category = categoryService.Get( categoryGuid );

                if ( category != null )
                {
                    entity.Categories.Add( category );
                }
            }
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="LavaShortcode"/> to be viewed or edited on the page.</returns>
        private LavaShortcode GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<LavaShortcode, LavaShortcodeService>( rockContext, PageParameterKey.LavaShortcodeId );
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
        protected override string RenewSecurityGrantToken()
        {
            using ( var rockContext = new RockContext() )
            {
                var entity = GetInitialEntity( rockContext );

                if ( entity != null )
                {
                    entity.LoadAttributes( rockContext );
                }

                return GetSecurityGrantToken( entity );
            }
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( LavaShortcode entity )
        {
            var securityGrant = new Rock.Security.SecurityGrant();

            securityGrant.AddRulesForAttributes( entity, RequestContext.CurrentPerson );

            return securityGrant.ToToken();
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="rockContext">The database context to load the entity from.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out LavaShortcode entity, out BlockActionResult error )
        {
            var entityService = new LavaShortcodeService( rockContext );
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
                entity = new LavaShortcode();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{LavaShortcode.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Rock.Security.Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${LavaShortcode.FriendlyTypeName}." );
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
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                entity.LoadAttributes( rockContext );

                var box = new DetailBlockBox<LavaShortcodeBag, LavaShortcodeDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
                };

                return ActionOk( box );
            }
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( DetailBlockBox<LavaShortcodeBag, LavaShortcodeDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new LavaShortcodeService( rockContext );

                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                var oldTagName = entity.TagName;

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Ensure everything is valid before saving.
                if ( !ValidateLavaShortcode( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );

                if ( LavaService.RockLiquidIsEnabled )
                {
                    // unregister shortcode
                    if ( oldTagName.IsNotNullOrWhiteSpace() )
                    {
                        Template.UnregisterShortcode( oldTagName );
                    }

                    // Register the new shortcode definition. Note that RockLiquid shortcode tags are case-sensitive.
                    if ( entity.TagType == TagType.Block )
                    {
                        Template.RegisterShortcode<Rock.Lava.Shortcodes.DynamicShortcodeBlock>( entity.TagName );
                    }
                    else
                    {
                        Template.RegisterShortcode<Rock.Lava.Shortcodes.DynamicShortcodeInline>( entity.TagName );
                    }

                    // (bug fix) Now we have to clear the entire LavaTemplateCache because it's possible that some other
                    // usage of this shortcode is cached with a key we can't predict.
#pragma warning disable CS0618 // Type or member is obsolete
                    // This obsolete code can be deleted when support for the DotLiquid Lava implementation is removed.
                    LavaTemplateCache.Clear();
#pragma warning restore CS0618 // Type or member is obsolete
                }

                if ( oldTagName.IsNotNullOrWhiteSpace() )
                {
                    LavaService.DeregisterShortcode( oldTagName );
                }

                // Register the new shortcode definition.
                LavaService.RegisterShortcode( entity.TagName, ( shortcodeName ) => WebsiteLavaShortcodeProvider.GetShortcodeDefinition( shortcodeName ) );

                LavaService.ClearTemplateCache();

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.LavaShortcodeId] = entity.IdKey
                    } ) );
                }

                // Ensure navigation properties will work now.
                entity = entityService.Get( entity.Id );
                entity.LoadAttributes( rockContext );

                return ActionOk( GetEntityBagForView( entity ) );
            }
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new LavaShortcodeService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                // unregister the shortcode
                LavaService.DeregisterShortcode( entity.TagName );
                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk( this.GetParentPageUrl() );
            }
        }

        /// <summary>
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( DetailBlockBox<LavaShortcodeBag, LavaShortcodeDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Reload attributes based on the new property values.
                entity.LoadAttributes( rockContext );

                var refreshedBox = new DetailBlockBox<LavaShortcodeBag, LavaShortcodeDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity )
                };

                var oldAttributeGuids = box.Entity.Attributes.Values.Select( a => a.AttributeGuid ).ToList();
                var newAttributeGuids = refreshedBox.Entity.Attributes.Values.Select( a => a.AttributeGuid );

                // If the attributes haven't changed then return a 204 status code.
                if ( oldAttributeGuids.SequenceEqual( newAttributeGuids ) )
                {
                    return ActionStatusCode( System.Net.HttpStatusCode.NoContent );
                }

                // Replace any values for attributes that haven't changed with
                // the value sent by the client. This ensures any unsaved attribute
                // value changes are not lost.
                foreach ( var kvp in refreshedBox.Entity.Attributes )
                {
                    if ( oldAttributeGuids.Contains( kvp.Value.AttributeGuid ) )
                    {
                        refreshedBox.Entity.AttributeValues[kvp.Key] = box.Entity.AttributeValues[kvp.Key];
                    }
                }

                return ActionOk( refreshedBox );
            }
        }

        #endregion
    }
}
