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
using Rock.ViewModels.Blocks.Cms.MediaFolderDetail;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays the details of a particular media folder.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />

    [DisplayName( "Media Folder Detail" )]
    [Category( "CMS" )]
    [Description( "Displays the details of a particular media folder." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "29cf7521-2dcd-467a-98fa-1c28c16c8b69" )]
    [Rock.SystemGuid.BlockTypeGuid( "662af7bb-5b61-43c6-bda6-a6e7aab8fc00" )]
    public class MediaFolderDetail : RockDetailBlockType, IBreadCrumbBlock
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string MediaAccountId = "MediaAccountId";
            public const string MediaFolderId = "MediaFolderId";
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
                var box = new DetailBlockBox<MediaFolderBag, MediaFolderDetailOptionsBag>();

                SetBoxInitialEntityState( box, true, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<MediaFolder>();

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
        private MediaFolderDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new MediaFolderDetailOptionsBag();

            if ( isEditable )
            {
                options.ContentChannels = new List<ViewModels.Utility.ListItemBag>();
                options.ContentChannelAttributes = new Dictionary<Guid, List<ListItemBag>>();
                foreach ( var item in ContentChannelCache.All().OrderBy( a => a.Name ) )
                {
                    options.ContentChannels.Add( new ViewModels.Utility.ListItemBag() { Text = item.Name, Value = item.Guid.ToString() } );
                    options.ContentChannelAttributes.Add( item.Guid, GetContentChannelItemAttributes( item, rockContext ) );
                }
            }

            return options;
        }

        /// <summary>
        /// Validates the MediaFolder for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="mediaFolder">The MediaFolder to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the MediaFolder is valid, <c>false</c> otherwise.</returns>
        private bool ValidateMediaFolder( MediaFolder mediaFolder, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            if ( mediaFolder.MediaAccountId == 0 )
            {
                errorMessage = "Please select a media account for this folder.";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="loadAttributes"><c>true</c> if attributes and values should be loaded; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<MediaFolderBag, MediaFolderDetailOptionsBag> box, bool loadAttributes, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity != null )
            {
                var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
                box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

                if ( loadAttributes )
                {
                    entity.LoadAttributes( rockContext );
                }

                if ( entity.Id != 0 )
                {
                    // Existing entity was found, prepare for view mode by default.
                    if ( isViewable )
                    {
                        box.Entity = GetEntityBagForView( entity, loadAttributes );
                        box.SecurityGrantToken = GetSecurityGrantToken( entity );
                    }
                    else
                    {
                        box.ErrorMessage = EditModeMessage.NotAuthorizedToView( MediaFolder.FriendlyTypeName );
                    }
                }
                else
                {
                    // New entity is being created, prepare for edit mode by default.
                    if ( box.IsEditable )
                    {
                        box.Entity = GetEntityBagForEdit( entity, loadAttributes );
                        box.SecurityGrantToken = GetSecurityGrantToken( entity );
                    }
                    else
                    {
                        box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( MediaFolder.FriendlyTypeName );
                    }
                }
            }
            else
            {
                box.ErrorMessage = $"The {MediaFolder.FriendlyTypeName} was not found.";
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="MediaFolderBag"/> that represents the entity.</returns>
        private MediaFolderBag GetCommonEntityBag( MediaFolder entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new MediaFolderBag
            {
                IdKey = entity.IdKey,
                ContentChannel = entity.ContentChannel.ToListItemBag(),
                ContentChannelAttribute = new ListItemBag() { Text = entity.ContentChannelAttribute?.Name, Value = entity.ContentChannelAttribute?.Guid.ToString() },
                MediaAccount = entity.MediaAccount.ToListItemBag(),
                Description = entity.Description,
                IsContentChannelSyncEnabled = entity.IsContentChannelSyncEnabled,
                IsPublic = entity.IsPublic,
                ContentChannelItemAttributes = entity.MediaElements.ToListItemBagList(),
                Name = entity.Name,
                WorkflowType = entity.WorkflowType.ToListItemBag(),
                ContentChannelItemStatus = entity.ContentChannelItemStatus.ConvertToStringSafe()
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specied entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <param name="loadAttributes"><c>true</c> if attributes and values should be loaded; otherwise <c>false</c>.</param>
        /// <returns>A <see cref="MediaFolderBag"/> that represents the entity.</returns>
        private MediaFolderBag GetEntityBagForView( MediaFolder entity, bool loadAttributes )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );
            bag.MetricData = entity.MediaAccount.GetMediaAccountComponent()?.GetFolderHtmlSummary( entity );

            if ( loadAttributes )
            {
                bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );
            }

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specied entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <param name="loadAttributes"><c>true</c> if attributes and values should be loaded; otherwise <c>false</c>.</param>
        /// <returns>A <see cref="MediaFolderBag"/> that represents the entity.</returns>
        private MediaFolderBag GetEntityBagForEdit( MediaFolder entity, bool loadAttributes )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            if ( entity.ContentChannelId.HasValue )
            {
                var channel = ContentChannelCache.Get( entity.ContentChannelId.Value );
                bag.ContentChannelItemAttributes = GetContentChannelItemAttributes( channel, new RockContext() );
            }

            if ( loadAttributes )
            {
                bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );
            }

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( MediaFolder entity, DetailBlockBox<MediaFolderBag, MediaFolderDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.ContentChannel ),
                () => entity.ContentChannelId = box.Entity.ContentChannel.GetEntityId<ContentChannel>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.ContentChannelItemStatus ),
                            () => entity.ContentChannelItemStatus = box.Entity.ContentChannelItemStatus.ConvertToEnumOrNull<ContentChannelItemStatus>() );

            box.IfValidProperty( nameof( box.Entity.ContentChannelAttribute ),
                () => entity.ContentChannelAttributeId = box.Entity.ContentChannelAttribute.GetEntityId<Rock.Model.Attribute>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.IsContentChannelSyncEnabled ),
                () => entity.IsContentChannelSyncEnabled = box.Entity.IsContentChannelSyncEnabled );

            box.IfValidProperty( nameof( box.Entity.IsPublic ),
                () => entity.IsPublic = box.Entity.IsPublic );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.WorkflowType ),
                () => entity.WorkflowTypeId = box.Entity.WorkflowType.GetEntityId<WorkflowType>( rockContext ) );

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
        /// <returns>The <see cref="MediaFolder"/> to be viewed or edited on the page.</returns>
        private MediaFolder GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<MediaFolder, MediaFolderService>( rockContext, PageParameterKey.MediaFolderId );
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
        /// <param name="entity">The entity being viewed or edited on this block.</param>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( IHasAttributes entity )
        {
            return new Rock.Security.SecurityGrant()
                .AddRulesForAttributes( entity, RequestContext.CurrentPerson )
                .ToToken();
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="rockContext">The database context to load the entity from.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out MediaFolder entity, out BlockActionResult error )
        {
            var entityService = new MediaFolderService( rockContext );
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
                entity = new MediaFolder();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{MediaFolder.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${MediaFolder.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the content channel attributes for the specified content channel.
        /// </summary>
        /// <param name="channel">The cached content channel object.</param>
        /// <param name="rockContext">The database context.</param>
        /// <returns>A list of attribute definitions.</returns>
        private static List<ListItemBag> GetContentChannelItemAttributes( ContentChannelCache channel, RockContext rockContext )
        {
            List<ListItemBag> mediaElementAttributes = new List<ListItemBag>();

            // Fake in-memory content channel item so we can properly
            // load all the attributes.
            var contentChannelItem = new ContentChannelItem
            {
                ContentChannelId = channel.Id,
                ContentChannelTypeId = channel.ContentChannelTypeId
            };

            // add content channel item attributes
            contentChannelItem.LoadAttributes( rockContext );
            var channelAttributes = contentChannelItem.Attributes.Select( a => a.Value ).ToList();

            foreach ( var attribute in channelAttributes )
            {
                if ( attribute.FieldType.Class == typeof( Rock.Field.Types.MediaElementFieldType ).FullName )
                {
                    mediaElementAttributes.Add( new ListItemBag() { Text = attribute.Name, Value = attribute.Guid.ToStringSafe() } );
                }
            }

            return mediaElementAttributes;
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            using ( var rockContext = new RockContext() )
            {
                var mediaFolderKey = pageReference.GetPageParameter( PageParameterKey.MediaFolderId );
                var pageParameters = new Dictionary<string, string>();
                var additionalParameters = new Dictionary<string, string>();
                var mediaFolderId = Rock.Utility.IdHasher.Instance.GetId( mediaFolderKey ) ?? mediaFolderKey.AsInteger();

                var data = new MediaFolderService( rockContext )
                    .GetSelect( mediaFolderId, mf => new
                    {
                        mf.Name,
                        mf.MediaAccountId
                    } );

                if ( data != null )
                {
                    pageParameters.Add( PageParameterKey.MediaFolderId, mediaFolderKey );
                    additionalParameters.Add( PageParameterKey.MediaAccountId, data.MediaAccountId.ToString() );
                }

                var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageParameters );
                var breadCrumb = new BreadCrumbLink( data?.Name ?? "New Media Folder", breadCrumbPageRef );

                return new BreadCrumbResult
                {
                    BreadCrumbs = new List<IBreadCrumb> { breadCrumb },
                    AdditionalParameters = additionalParameters
                };
            }
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

                var box = new DetailBlockBox<MediaFolderBag, MediaFolderDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, true ),
                    Options = GetBoxOptions( true, rockContext )
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
        public BlockActionResult Save( DetailBlockBox<MediaFolderBag, MediaFolderDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new MediaFolderService( rockContext );

                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                if ( entity.MediaAccountId == 0 )
                {
                    var mediaAccountKey = RequestContext.GetPageParameter( PageParameterKey.MediaAccountId );
                    entity.MediaAccountId = Rock.Utility.IdHasher.Instance.GetId( mediaAccountKey ) ?? mediaAccountKey.AsInteger();
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Ensure everything is valid before saving.
                if ( !ValidateMediaFolder( entity, rockContext, out var validationMessage ) )
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
                        [PageParameterKey.MediaFolderId] = entity.IdKey
                    } ) );
                }

                // Ensure navigation properties will work now.
                entity = entityService.Get( entity.Id );
                entity.LoadAttributes( rockContext );

                return ActionOk( GetEntityBagForView( entity, true ) );
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
                var entityService = new MediaFolderService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
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
        public BlockActionResult RefreshAttributes( DetailBlockBox<MediaFolderBag, MediaFolderDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<MediaFolderBag, MediaFolderDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, true )
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

        /// <summary>
        /// Updates the media file attribute dropdowns.
        /// </summary>
        /// <param name="channelGuid">The channel unique identifier.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult UpdateMediaFileAttributeDropdowns( Guid channelGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var channel = ContentChannelCache.Get( channelGuid );

                List<ListItemBag> mediaElementAttributes = GetContentChannelItemAttributes( channel, rockContext );

                return ActionOk( new { mediaElementAttributes } );
            }
        }

        #endregion
    }
}