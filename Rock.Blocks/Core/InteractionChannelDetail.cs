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
using Rock.ViewModels.Utility;
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
    public class InteractionChannelDetail : RockEntityDetailBlockType<InteractionChannel, InteractionChannelBag>
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
            var box = new DetailBlockBox<InteractionChannelBag, InteractionChannelDetailOptionsBag>();

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
        private InteractionChannelDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new InteractionChannelDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the InteractionChannel for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="interactionChannel">The InteractionChannel to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the InteractionChannel is valid, <c>false</c> otherwise.</returns>
        private bool ValidateInteractionChannel( InteractionChannel interactionChannel, out string errorMessage )
        {
            errorMessage = null;

            if ( !interactionChannel.IsValid )
            {
                errorMessage = interactionChannel.ValidationResults.ConvertAll( a => a.ErrorMessage ).AsDelimited( "<br />" );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<InteractionChannelBag, InteractionChannelDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {InteractionChannel.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) || entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) || entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( InteractionChannel.FriendlyTypeName );
                }
            }
            else
            {
                box.ErrorMessage = @"
<strong>Missing Channel Information </strong>
<p>Make sure you have navigated to this page from Channel Listing page.</p>";
            }

            PrepareDetailBox( box, entity );
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
                ComponentCacheDuration = entity.ComponentCacheDuration,
                ComponentDetailTemplate = entity.ComponentDetailTemplate,
                ComponentListTemplate = entity.ComponentListTemplate,
                EngagementStrength = entity.EngagementStrength,
                InteractionCustom1Label = entity.InteractionCustom1Label,
                InteractionCustom2Label = entity.InteractionCustom2Label,
                InteractionCustomIndexed1Label = entity.InteractionCustomIndexed1Label,
                InteractionDetailTemplate = entity.InteractionDetailTemplate,
                InteractionListTemplate = entity.InteractionListTemplate,
                IsActive = entity.IsActive,
                Name = entity.Name,
                RetentionDuration = entity.RetentionDuration,
                SessionListTemplate = entity.SessionListTemplate
            };
        }

        /// <inheritdoc/>
        protected override InteractionChannelBag GetEntityBagForView( InteractionChannel entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: false );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, GetCurrentPerson() );
            mergeFields.TryAdd( "CurrentPerson", GetCurrentPerson() );
            mergeFields.Add( "InteractionChannel", entity );

            string template = GetAttributeValue( AttributeKey.DefaultTemplate );
            if ( !string.IsNullOrEmpty( entity.ChannelDetailTemplate ) )
            {
                template = entity.ChannelDetailTemplate;
            }

            bag.Content = template.ResolveMergeFields( mergeFields );
            bag.CanAdministrate = BlockCache.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) || entity.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson );

            return bag;
        }

        /// <inheritdoc/>
        protected override InteractionChannelBag GetEntityBagForEdit( InteractionChannel entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: false );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( InteractionChannel entity, ValidPropertiesBox<InteractionChannelBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.ChannelDetailTemplate ),
                () => entity.ChannelDetailTemplate = box.Bag.ChannelDetailTemplate );

            box.IfValidProperty( nameof( box.Bag.ChannelListTemplate ),
                () => entity.ChannelListTemplate = box.Bag.ChannelListTemplate );

            box.IfValidProperty( nameof( box.Bag.ComponentCacheDuration ),
                () => entity.ComponentCacheDuration = box.Bag.ComponentCacheDuration );

            box.IfValidProperty( nameof( box.Bag.ComponentDetailTemplate ),
                () => entity.ComponentDetailTemplate = box.Bag.ComponentDetailTemplate );

            box.IfValidProperty( nameof( box.Bag.ComponentListTemplate ),
                () => entity.ComponentListTemplate = box.Bag.ComponentListTemplate );

            box.IfValidProperty( nameof( box.Bag.EngagementStrength ),
                () => entity.EngagementStrength = box.Bag.EngagementStrength );

            box.IfValidProperty( nameof( box.Bag.InteractionCustom1Label ),
                () => entity.InteractionCustom1Label = box.Bag.InteractionCustom1Label );

            box.IfValidProperty( nameof( box.Bag.InteractionCustom2Label ),
                () => entity.InteractionCustom2Label = box.Bag.InteractionCustom2Label );

            box.IfValidProperty( nameof( box.Bag.InteractionCustomIndexed1Label ),
                () => entity.InteractionCustomIndexed1Label = box.Bag.InteractionCustomIndexed1Label );

            box.IfValidProperty( nameof( box.Bag.InteractionDetailTemplate ),
                () => entity.InteractionDetailTemplate = box.Bag.InteractionDetailTemplate );

            box.IfValidProperty( nameof( box.Bag.InteractionListTemplate ),
                () => entity.InteractionListTemplate = box.Bag.InteractionListTemplate );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.RetentionDuration ),
                () => entity.RetentionDuration = box.Bag.RetentionDuration );

            box.IfValidProperty( nameof( box.Bag.SessionListTemplate ),
                () => entity.SessionListTemplate = box.Bag.SessionListTemplate );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: false );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override InteractionChannel GetInitialEntity()
        {
            return GetInitialEntity<InteractionChannel, InteractionChannelService>( RockContext, PageParameterKey.InteractionChannelId );
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
        protected override bool TryGetEntityForEditAction( string idKey, out InteractionChannel entity, out BlockActionResult error )
        {
            var entityService = new InteractionChannelService( RockContext );
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
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<InteractionChannelBag>()
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
        public BlockActionResult Save( ValidPropertiesBox<InteractionChannelBag> box )
        {
            var entityService = new InteractionChannelService( RockContext );

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
            if ( !ValidateInteractionChannel( entity, out var validationMessage ) )
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
                    [PageParameterKey.InteractionChannelId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForView( entity );

            return ActionOk( new ValidPropertiesBox<InteractionChannelBag>()
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
            var entityService = new InteractionChannelService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
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
            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl() );
        }

        #endregion
    }
}
