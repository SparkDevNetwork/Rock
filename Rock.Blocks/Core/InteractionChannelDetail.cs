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
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.InteractionChannelDetail;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays the details of a particular interaction channel.
    /// </summary>

    [DisplayName( "Interaction Channel Detail" )]
    [Category( "Reporting" )]
    [Description( "Displays the details of a particular interaction channel." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [CodeEditorField( "Default Template",
        Key = AttributeKey.DefaultTemplate,
        Description = "Lava template to use to display content",
        EditorMode = CodeEditorMode.Lava,
        EditorTheme = CodeEditorTheme.Rock,
        EditorHeight = 400,
        IsRequired = false,
        DefaultValue = @"
<div class='row'>
    {% if InteractionChannel.Name != '' %}
        <div class='col-md-6'>
            <dl><dt>Name</dt><dd>{{ InteractionChannel.Name }}<dd/></dl>
        </div>
    {% endif %}
    {% if InteractionChannel.ChannelTypeMediumValue != null and InteractionChannel.ChannelTypeMediumValue != '' %}
        <div class='col-md-6'>
            <dl><dt>Medium</dt><dd>{{ InteractionChannel.ChannelTypeMediumValue.Value }}<dd/></dl>
        </div>
    {% endif %}
    {% if InteractionChannel.EngagementStrength != null and InteractionChannel.EngagementStrength != '' %}
      <div class='col-md-6'>
          <dl><dt>Engagement Strength</dt><dd>{{ InteractionChannel.EngagementStrength }}<dd/></dl>
       </div>
    {% endif %}
    {% if InteractionChannel.RetentionDuration != null %}
        <div class='col-md-6'>
            <dl><dt>Retention Duration</dt><dd>{{ InteractionChannel.RetentionDuration }}<dd/></dl>
        </div>
    {% endif %}
    {% if InteractionChannel.ComponentCacheDuration != null %}
        <div class='col-md-6'>
            <dl><dt>Component Cache Duration</dt><dd>{{ InteractionChannel.ComponentCacheDuration }}<dd/></dl>
        </div>
    {% endif %}
</div>
",
       Order = 0 )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "9438e0fe-f7ab-48d5-8ab8-d54336d30fbd" )]
    [Rock.SystemGuid.BlockTypeGuid( "2efa1f9d-7062-466a-a8f3-9dcdbff054e9" )]
    public class InteractionChannelDetail : RockDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string InteractionChannelId = "ChannelId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        private static class AttributeKey
        {
            public const string DefaultTemplate = "DefaultTemplate";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<InteractionChannelBag, InteractionChannelDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<InteractionChannel>();

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
        private InteractionChannelDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new InteractionChannelDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the InteractionChannel for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="interactionChannel">The InteractionChannel to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the InteractionChannel is valid, <c>false</c> otherwise.</returns>
        private bool ValidateInteractionChannel( InteractionChannel interactionChannel, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<InteractionChannelBag, InteractionChannelDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {InteractionChannel.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) || entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) || entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( rockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                    box.Entity.CanAdministrate = BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) || entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( InteractionChannel.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                box.ErrorMessage = @"
<strong>Missing Channel Information </strong>
<p>Make sure you have navigated to this page from Channel Listing page.</p>";
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="InteractionChannelBag"/> that represents the entity.</returns>
        private InteractionChannelBag GetCommonEntityBag( InteractionChannel entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new InteractionChannelBag
            {
                IdKey = entity.IdKey,
                ChannelDetailTemplate = entity.ChannelDetailTemplate,
                ChannelListTemplate = entity.ChannelListTemplate,
                ComponentCacheDuration = entity.ComponentCacheDuration.ToString(),
                ComponentDetailTemplate = entity.ComponentDetailTemplate,
                ComponentListTemplate = entity.ComponentListTemplate,
                EngagementStrength = entity.EngagementStrength.ToString(),
                InteractionCustom1Label = entity.InteractionCustom1Label,
                InteractionCustom2Label = entity.InteractionCustom2Label,
                InteractionCustomIndexed1Label = entity.InteractionCustomIndexed1Label,
                InteractionDetailTemplate = entity.InteractionDetailTemplate,
                InteractionListTemplate = entity.InteractionListTemplate,
                IsActive = entity.IsActive,
                Name = entity.Name,
                RetentionDuration = entity.RetentionDuration.ToString(),
                SessionListTemplate = entity.SessionListTemplate
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="InteractionChannelBag"/> that represents the entity.</returns>
        private InteractionChannelBag GetEntityBagForView( InteractionChannel entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, GetCurrentPerson() );
            mergeFields.TryAdd( "CurrentPerson", GetCurrentPerson() );
            mergeFields.Add( "InteractionChannel", entity );

            string template = GetAttributeValue( AttributeKey.DefaultTemplate );
            if ( !string.IsNullOrEmpty( entity.ChannelDetailTemplate ) )
            {
                template = entity.ChannelDetailTemplate;
            }

            bag.Content = template.ResolveMergeFields( mergeFields );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="InteractionChannelBag"/> that represents the entity.</returns>
        private InteractionChannelBag GetEntityBagForEdit( InteractionChannel entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( InteractionChannel entity, DetailBlockBox<InteractionChannelBag, InteractionChannelDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.ChannelDetailTemplate ),
                () => entity.ChannelDetailTemplate = box.Entity.ChannelDetailTemplate );

            box.IfValidProperty( nameof( box.Entity.ChannelListTemplate ),
                () => entity.ChannelListTemplate = box.Entity.ChannelListTemplate );

            box.IfValidProperty( nameof( box.Entity.ComponentCacheDuration ),
                () => entity.ComponentCacheDuration = box.Entity.ComponentCacheDuration.AsIntegerOrNull() );

            box.IfValidProperty( nameof( box.Entity.ComponentDetailTemplate ),
                () => entity.ComponentDetailTemplate = box.Entity.ComponentDetailTemplate );

            box.IfValidProperty( nameof( box.Entity.ComponentListTemplate ),
                () => entity.ComponentListTemplate = box.Entity.ComponentListTemplate );

            box.IfValidProperty( nameof( box.Entity.EngagementStrength ),
                () => entity.EngagementStrength = box.Entity.EngagementStrength.AsIntegerOrNull() );

            box.IfValidProperty( nameof( box.Entity.InteractionCustom1Label ),
                () => entity.InteractionCustom1Label = box.Entity.InteractionCustom1Label );

            box.IfValidProperty( nameof( box.Entity.InteractionCustom2Label ),
                () => entity.InteractionCustom2Label = box.Entity.InteractionCustom2Label );

            box.IfValidProperty( nameof( box.Entity.InteractionCustomIndexed1Label ),
                () => entity.InteractionCustomIndexed1Label = box.Entity.InteractionCustomIndexed1Label );

            box.IfValidProperty( nameof( box.Entity.InteractionDetailTemplate ),
                () => entity.InteractionDetailTemplate = box.Entity.InteractionDetailTemplate );

            box.IfValidProperty( nameof( box.Entity.InteractionListTemplate ),
                () => entity.InteractionListTemplate = box.Entity.InteractionListTemplate );

            box.IfValidProperty( nameof( box.Entity.IsActive ),
                () => entity.IsActive = box.Entity.IsActive );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.RetentionDuration ),
                () => entity.RetentionDuration = box.Entity.RetentionDuration.AsIntegerOrNull() );

            box.IfValidProperty( nameof( box.Entity.SessionListTemplate ),
                () => entity.SessionListTemplate = box.Entity.SessionListTemplate );

            box.IfValidProperty( nameof( box.Entity.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( rockContext );

                    entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="InteractionChannel"/> to be viewed or edited on the page.</returns>
        private InteractionChannel GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<InteractionChannel, InteractionChannelService>( rockContext, PageParameterKey.InteractionChannelId );
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
        private string GetSecurityGrantToken( InteractionChannel entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out InteractionChannel entity, out BlockActionResult error )
        {
            var entityService = new InteractionChannelService( rockContext );
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
                entity = new InteractionChannel();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{InteractionChannel.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${InteractionChannel.FriendlyTypeName}." );
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

                var box = new DetailBlockBox<InteractionChannelBag, InteractionChannelDetailOptionsBag>
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
        public BlockActionResult Save( DetailBlockBox<InteractionChannelBag, InteractionChannelDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new InteractionChannelService( rockContext );

                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Ensure everything is valid before saving.
                if ( !ValidateInteractionChannel( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.InteractionChannelId] = entity.IdKey
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
                var entityService = new InteractionChannelService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                if ( !entity.IsAuthorized( Authorization.EDIT, this.GetCurrentPerson() ) )
                {
                    return ActionBadRequest( "You are not authorized to delete this interaction channel." );
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

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
        public BlockActionResult RefreshAttributes( DetailBlockBox<InteractionChannelBag, InteractionChannelDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<InteractionChannelBag, InteractionChannelDetailOptionsBag>
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
