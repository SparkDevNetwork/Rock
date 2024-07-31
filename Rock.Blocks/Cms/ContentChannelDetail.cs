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
using Rock.Model;
using Rock.Security;
using Rock.Store;
using Rock.UniversalSearch;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.ContentChannelDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays the details of a particular content channel.
    /// </summary>
    [DisplayName( "Content Channel Detail" )]
    [Category( "CMS" )]
    [Description( "Displays the details for a content channel." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "c7c776c4-f1db-477d-87e3-62f8f82ba773" )]
    [Rock.SystemGuid.BlockTypeGuid( "2bad2ab9-86ad-480e-bf38-c54f2c5c03a8" )]
    public class ContentChannelDetail : RockDetailBlockType
    {
        // This is a cache backing field and should not be accessed directly , instead use the GetItemAttributes method to access this field. 
        private List<Rock.Model.Attribute> _itemAttributes;

        #region Keys

        private static class PageParameterKey
        {
            public const string ContentChannelId = "ContentChannelId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        private static class SettingsKey
        {
            public const string EnablePersonalization = "EnablePersonalization";
            public const string EnableIndexing = "EnableIndexing";
            public const string ItemsManuallyOrdered = "ItemsManuallyOrdered";
            public const string ChildItemsManuallyOrdered = "ChildItemsManuallyOrdered";
            public const string ItemsRequireApproval = "ItemsRequireApproval";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<ContentChannelBag, ContentChannelDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<ContentChannel>();

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="entity">The content channel.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private ContentChannelDetailOptionsBag GetBoxOptions( ContentChannel entity, RockContext rockContext )
        {
            var options = new ContentChannelDetailOptionsBag
            {
                IsIndexEnabled = IndexContainer.IndexingEnabled,
                IsApproverConfigured = IsApproverConfigured( entity, rockContext ),
                ContentChannelTypes = GetContentChannelTypes( rockContext ),
                ContentControlTypes = typeof( ContentControlType ).ToEnumListItemBag(),
                SettingsOptions = GetSettingsOptions( entity ),
                IsOrganizationConfigured = StoreService.OrganizationIsConfigured(),
                AvailableLicenses = GetLicenses(),
                ContentLibraryAttributes = GetContentLibraryAttributes( rockContext, entity.ContentChannelTypeId, GetItemAttributes( entity.Id, rockContext ) ),
                ContentChannelList = GetContentChannelList( rockContext ),
                CurrentPageUrl = this.GetCurrentPageUrl().UrlEncode(),
                DisableContentField = entity.ContentChannelType?.DisableContentField ?? false
            };

            return options;
        }

        /// <summary>
        /// Gets the content channel list for the add Child Content Channel dropdown.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<ListItemBag> GetContentChannelList( RockContext rockContext )
        {
            var contentChannelList = new ContentChannelService( rockContext )
                .Queryable()
                .OrderBy( t => t.Name )
                .Select( cc => new ListItemBag()
                {
                    Text = cc.Name,
                    Value = cc.Guid.ToString()
                } )
                .ToList();

            return contentChannelList;
        }

        /// <summary>
        /// Gets the available licenses.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        private List<ListItemBag> GetLicenses()
        {
            // License
            var availableLicenseGuids = new[]
            {
                Rock.SystemGuid.DefinedValue.LIBRARY_LICENSE_TYPE_OPEN.AsGuid(),
                Rock.SystemGuid.DefinedValue.LIBRARY_LICENSE_TYPE_AUTHOR_ATTRIBUTION.AsGuid(),
                Rock.SystemGuid.DefinedValue.LIBRARY_LICENSE_TYPE_ORGANIZATION_ATTRIBUTION.AsGuid()
            };

            var licenses = new List<ListItemBag>();
            foreach ( var licenseGuid in availableLicenseGuids )
            {
                var license = DefinedValueCache.Get( licenseGuid );

                if ( license != null )
                {
                    licenses.Add( new ListItemBag() { Text = license.Value, Value = license.Guid.ToString() } );
                }
            }

            return licenses;
        }

        /// <summary>
        /// Gets the settings options.
        /// </summary>
        /// <returns></returns>
        private List<ListItemBag> GetSettingsOptions( ContentChannel entity )
        {
            var settings = new List<ListItemBag>()
            {
                new ListItemBag()
                {
                    Text = SettingsKey.EnablePersonalization.SplitCase(),
                    Value = SettingsKey.EnablePersonalization
                },
                new ListItemBag()
                {
                    Text = SettingsKey.ItemsManuallyOrdered.SplitCase(),
                    Value = SettingsKey.ItemsManuallyOrdered
                },
                new ListItemBag()
                {
                    Text = SettingsKey.ChildItemsManuallyOrdered.SplitCase(),
                    Value = SettingsKey.ChildItemsManuallyOrdered
                },
            };

            if ( entity.ContentChannelType?.DisableStatus == false )
            {
                settings.Add( new ListItemBag()
                {
                    Text = SettingsKey.ItemsRequireApproval.SplitCase(),
                    Value = SettingsKey.ItemsRequireApproval
                } );
            }

            if ( IndexContainer.IndexingEnabled )
            {
                settings.Add( new ListItemBag()
                {
                    Text = SettingsKey.EnableIndexing.SplitCase(),
                    Value = SettingsKey.EnableIndexing
                } );
            }

            return settings;
        }

        /// <summary>
        /// Gets the content channel types.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<ListItemBag> GetContentChannelTypes( RockContext rockContext )
        {
            var contentChannelTypeService = new ContentChannelTypeService( rockContext );

            var visibleContentChannelTypeList = contentChannelTypeService
                .Queryable()
                .AsNoTracking()
                .Where( a => a.ShowInChannelList )
                .OrderBy( c => c.Name )
                .Select( a => new ListItemBag()
                {
                    Value = a.Guid.ToString(),
                    Text = a.Name
                } )
                .ToList();

            return visibleContentChannelTypeList;
        }

        /// <summary>
        /// Validates the ContentChannel for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="contentChannel">The ContentChannel to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the ContentChannel is valid, <c>false</c> otherwise.</returns>
        private bool ValidateContentChannel( ContentChannel contentChannel, out string errorMessage )
        {
            errorMessage = null;

            if ( !contentChannel.IsValid )
            {
                errorMessage = contentChannel.ValidationResults.ConvertAll( a => a.ErrorMessage ).AsDelimited( "<br />" );
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
        private void SetBoxInitialEntityState( DetailBlockBox<ContentChannelBag, ContentChannelDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {ContentChannel.FriendlyTypeName} was not found.";
                return;
            }

            box.Options = GetBoxOptions( entity, rockContext );

            var isViewable = entity.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( ContentChannel.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity, rockContext );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( ContentChannel.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="ContentChannelBag"/> that represents the entity.</returns>
        private ContentChannelBag GetCommonEntityBag( ContentChannel entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new ContentChannelBag
            {
                IdKey = entity.IdKey,
                Categories = entity.Categories.ToListItemBagList(),
                ChannelUrl = entity.ChannelUrl,
                ChildContentChannels = entity.ChildContentChannels.ToListItemBagList(),
                ChildItemsManuallyOrdered = entity.ChildItemsManuallyOrdered,
                ContentChannelType = entity.ContentChannelType.ToListItemBag(),
                ContentControlType = entity.ContentControlType.ConvertToInt(),
                Description = entity.Description,
                EnablePersonalization = entity.EnablePersonalization,
                EnableRss = entity.EnableRss,
                IconCssClass = entity.IconCssClass,
                IsIndexEnabled = entity.IsIndexEnabled,
                IsStructuredContent = entity.Id == 0 || entity.IsStructuredContent,
                IsTaggingEnabled = entity.IsTaggingEnabled,
                ItemsManuallyOrdered = entity.ItemsManuallyOrdered,
                ItemTagCategory = entity.ItemTagCategory.ToListItemBag(),
                ItemUrl = entity.ItemUrl,
                Name = entity.Name,
                RequiresApproval = entity.RequiresApproval,
                RootImageDirectory = entity.RootImageDirectory,
                StructuredContentToolValue = entity.StructuredContentToolValue.ToListItemBag(),
                TimeToLive = entity.TimeToLive.ToString()
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="ContentChannelBag"/> that represents the entity.</returns>
        private ContentChannelBag GetEntityBagForView( ContentChannel entity )
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
        /// <returns>A <see cref="ContentChannelBag"/> that represents the entity.</returns>
        private ContentChannelBag GetEntityBagForEdit( ContentChannel entity, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );
            bag.Settings = GetSettings( entity );
            bag.IsContentLibraryEnabled = entity.ContentLibraryConfiguration?.IsEnabled ?? false;
            bag.LicenseTypeGuid = entity.ContentLibraryConfiguration?.LicenseTypeValueGuid;
            bag.SummaryAttributeGuid = entity.ContentLibraryConfiguration?.SummaryAttributeGuid;
            bag.AuthorAttributeGuid = entity.ContentLibraryConfiguration?.AuthorAttributeGuid;
            bag.ImageAttributeGuid = entity.ContentLibraryConfiguration?.ImageAttributeGuid;
            bag.ItemAttributes = GetItemAttributes( entity.Id, rockContext ).ConvertAll( a => PublicAttributeHelper.GetPublicEditableAttributeViewModel( a ) );
            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        private List<ListItemBag> GetContentLibraryAttributes( RockContext rockContext, int? contentChannelTypeId, List<Rock.Model.Attribute> itemAttributes )
        {
            var listItems = itemAttributes.ToListItemBagList();

            if ( contentChannelTypeId.HasValue )
            {
                var attributeService = new AttributeService( rockContext );
                var inheritedAttributes = attributeService
                    .GetByEntityTypeId( new ContentChannelItem().TypeId, true )
                    .AsQueryable()
                    .Where( a =>
                        a.EntityTypeQualifierColumn.Equals( "ContentChannelTypeId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( contentChannelTypeId.Value.ToString() ) )
                    .Select( a => new ListItemBag() { Text = a.Name, Value = a.Guid.ToString() } );

                listItems.AddRange( inheritedAttributes );
            }

            return listItems;
        }

        private List<Rock.Model.Attribute> GetItemAttributes( int id, RockContext rockContext )
        {
            if ( _itemAttributes != null )
            {
                return _itemAttributes;
            }
            else
            {
                _itemAttributes = new List<Rock.Model.Attribute>();
                AttributeService attributeService = new AttributeService( rockContext );

                attributeService.GetByEntityTypeId( new ContentChannelItem().TypeId, true ).AsQueryable()
                    .Where( a =>
                        a.EntityTypeQualifierColumn.Equals( "ContentChannelId", StringComparison.OrdinalIgnoreCase ) &&
                        a.EntityTypeQualifierValue.Equals( id.ToString() ) )
                    .OrderBy( a => a.Order )
                    .ToList()
                    .ForEach( a => _itemAttributes.Add( a ) );

                return _itemAttributes;
            }
        }

        private List<string> GetSettings( ContentChannel entity )
        {
            var settings = new List<string>();

            if ( entity.RequiresApproval )
            {
                settings.Add( SettingsKey.ItemsRequireApproval );
            }

            if ( entity.EnablePersonalization )
            {
                settings.Add( SettingsKey.EnablePersonalization );
            }

            if ( entity.ItemsManuallyOrdered )
            {
                settings.Add( SettingsKey.ItemsManuallyOrdered );
            }

            if ( entity.ChildItemsManuallyOrdered )
            {
                settings.Add( SettingsKey.ChildItemsManuallyOrdered );
            }

            if ( entity.IsIndexEnabled && IndexContainer.IndexingEnabled )
            {
                settings.Add( SettingsKey.EnableIndexing );
            }

            return settings;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( ContentChannel entity, DetailBlockBox<ContentChannelBag, ContentChannelDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.ChannelUrl ),
                () => entity.ChannelUrl = box.Entity.ChannelUrl );

            box.IfValidProperty( nameof( box.Entity.ChildItemsManuallyOrdered ),
                () => entity.ChildItemsManuallyOrdered = box.Entity.ChildItemsManuallyOrdered );

            box.IfValidProperty( nameof( box.Entity.ContentChannelType ),
                () => entity.ContentChannelTypeId = box.Entity.ContentChannelType.GetEntityId<ContentChannelType>( rockContext ).Value );

            box.IfValidProperty( nameof( box.Entity.ContentControlType ),
                () => entity.ContentControlType = ( ContentControlType ) box.Entity.ContentControlType );

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.EnablePersonalization ),
                () => entity.EnablePersonalization = box.Entity.EnablePersonalization );

            box.IfValidProperty( nameof( box.Entity.EnableRss ),
                () => entity.EnableRss = box.Entity.EnableRss );

            box.IfValidProperty( nameof( box.Entity.IconCssClass ),
                () => entity.IconCssClass = box.Entity.IconCssClass );

            box.IfValidProperty( nameof( box.Entity.IsIndexEnabled ),
                () => entity.IsIndexEnabled = box.Entity.IsIndexEnabled );

            box.IfValidProperty( nameof( box.Entity.IsStructuredContent ),
                () => entity.IsStructuredContent = box.Entity.IsStructuredContent );

            box.IfValidProperty( nameof( box.Entity.IsTaggingEnabled ),
                () => entity.IsTaggingEnabled = box.Entity.IsTaggingEnabled );

            box.IfValidProperty( nameof( box.Entity.ItemsManuallyOrdered ),
                () => entity.ItemsManuallyOrdered = box.Entity.ItemsManuallyOrdered );

            box.IfValidProperty( nameof( box.Entity.ItemTagCategory ),
                () => entity.ItemTagCategoryId = box.Entity.ItemTagCategory.GetEntityId<Category>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.ItemUrl ),
                () => entity.ItemUrl = box.Entity.ItemUrl );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.RequiresApproval ),
                () => entity.RequiresApproval = box.Entity.RequiresApproval );

            box.IfValidProperty( nameof( box.Entity.RootImageDirectory ),
                () => entity.RootImageDirectory = box.Entity.RootImageDirectory );

            box.IfValidProperty( nameof( box.Entity.StructuredContentToolValue ),
                () => entity.StructuredContentToolValueId = box.Entity.StructuredContentToolValue.GetEntityId<DefinedValue>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.TimeToLive ),
                () => entity.TimeToLive = box.Entity.TimeToLive.AsIntegerOrNull() );

            box.IfValidProperty( nameof( box.Entity.Settings ),
                () => UpdateContentChannelSettings( box.Entity, entity ) );

            box.IfValidProperty( nameof( box.Entity.Categories ),
                () => UpdateCategories( box.Entity, entity, rockContext ) );

            box.IfValidProperty( nameof( box.Entity.ChildContentChannels ),
                () => UpdateChildContentChannels( box.Entity, entity, rockContext ) );

            if ( StoreService.OrganizationIsConfigured() )
            {
                var contentLibraryConfiguration = entity.ContentLibraryConfiguration ?? new Rock.Cms.ContentLibraryConfiguration();

                box.IfValidProperty( nameof( box.Entity.IsContentLibraryEnabled ),
                    () => contentLibraryConfiguration.IsEnabled = box.Entity.IsContentLibraryEnabled );

                box.IfValidProperty( nameof( box.Entity.LicenseTypeGuid ),
                    () => contentLibraryConfiguration.LicenseTypeValueGuid = box.Entity.LicenseTypeGuid );

                box.IfValidProperty( nameof( box.Entity.SummaryAttributeGuid ),
                    () => contentLibraryConfiguration.SummaryAttributeGuid = box.Entity.SummaryAttributeGuid );

                box.IfValidProperty( nameof( box.Entity.AuthorAttributeGuid ),
                    () => contentLibraryConfiguration.AuthorAttributeGuid = box.Entity.AuthorAttributeGuid );

                box.IfValidProperty( nameof( box.Entity.ImageAttributeGuid ),
                    () => contentLibraryConfiguration.ImageAttributeGuid = box.Entity.ImageAttributeGuid );

                entity.ContentLibraryConfiguration = contentLibraryConfiguration;
            }

            box.IfValidProperty( nameof( box.Entity.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( rockContext );

                    entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <summary>
        /// Updates the child content channels for the current content channel.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <param name="contentChannel">The content channel.</param>
        /// <param name="rockContext">The rock context.</param>
        private void UpdateChildContentChannels( ContentChannelBag bag, ContentChannel contentChannel, RockContext rockContext )
        {
            var contentChannelService = new ContentChannelService( rockContext );
            contentChannel.ChildContentChannels = new List<ContentChannel>();
            contentChannel.ChildContentChannels.Clear();
            foreach ( var item in bag.ChildContentChannels.Select( cc => cc.Value.AsGuid() ) )
            {
                var childContentChannel = contentChannelService.Get( item );
                if ( childContentChannel != null )
                {
                    contentChannel.ChildContentChannels.Add( childContentChannel );
                }
            }
        }

        /// <summary>
        /// Updates the content channel categories.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <param name="contentChannel">The content channel.</param>
        /// <param name="rockContext">The rock context.</param>
        private void UpdateCategories( ContentChannelBag bag, ContentChannel contentChannel, RockContext rockContext )
        {
            contentChannel.Categories.Clear();
            var categoryService = new CategoryService( rockContext );
            foreach ( var categoryGuid in bag.Categories.Select( x => x.Value.AsGuid() ) )
            {
                var category = categoryService.Get( categoryGuid );
                if ( category != null )
                {
                    contentChannel.Categories.Add( category );
                }
            }

            // Since changes to Categories isn't tracked by ChangeTracker, set the ModifiedDateTime just in case Categories changed.
            contentChannel.ModifiedDateTime = RockDateTime.Now;
        }

        /// <summary>
        /// Updates the content channel settings.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <param name="contentChannel">The content channel.</param>
        private void UpdateContentChannelSettings( ContentChannelBag bag, ContentChannel contentChannel )
        {
            contentChannel.RequiresApproval = bag.Settings.Contains( SettingsKey.ItemsRequireApproval );
            contentChannel.IsIndexEnabled = bag.Settings.Contains( SettingsKey.EnableIndexing );
            contentChannel.ItemsManuallyOrdered = bag.Settings.Contains( SettingsKey.ItemsManuallyOrdered );
            contentChannel.ChildItemsManuallyOrdered = bag.Settings.Contains( SettingsKey.ChildItemsManuallyOrdered );
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="ContentChannel"/> to be viewed or edited on the page.</returns>
        private ContentChannel GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<ContentChannel, ContentChannelService>( rockContext, PageParameterKey.ContentChannelId );
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
        private string GetSecurityGrantToken( ContentChannel entity )
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out ContentChannel entity, out BlockActionResult error )
        {
            var entityService = new ContentChannelService( rockContext );
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
                entity = new ContentChannel();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{ContentChannel.FriendlyTypeName} not found." );
                return false;
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${ContentChannel.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Check if there is any approver configured.
        /// </summary>
        /// <param name="contentChannel">The content channel.</param>
        /// <param name="rockContext">The Rock Context.</param>
        private bool IsApproverConfigured( ContentChannel contentChannel, RockContext rockContext )
        {
            var authService = new AuthService( rockContext );
            var contentChannelEntityTypeId = EntityTypeCache.Get<Rock.Model.ContentChannel>().Id;

            var approvalAuths = authService.GetAuths( contentChannelEntityTypeId, contentChannel.Id, Rock.Security.Authorization.APPROVE );

            // Get a list of all PersonIds that are allowed that are included in the Auths
            // Then, when we get a list of all the allowed people that are in the auth as a specific Person or part of a Role (Group), we'll run all those people thru NoteType.IsAuthorized
            // That way, we don't have to figure out all the logic of Allow/Deny based on Order, etc
            bool isValid = approvalAuths.Any( a => a.AllowOrDeny == "A" && ( a.PersonAlias != null || a.GroupId != null ) );

            return isValid;
        }

        /// <summary>
        /// Saves the attributes.
        /// </summary>
        /// <param name="channelId">The channel identifier.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="attributes">The attributes.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SaveAttributes( int channelId, int entityTypeId, List<PublicEditableAttributeBag> attributes, RockContext rockContext )
        {
            string qualifierColumn = "ContentChannelId";
            string qualifierValue = channelId.ToString();

            AttributeService attributeService = new AttributeService( rockContext );

            // Get the existing attributes for this entity type and qualifier value
            var existingAttributes = attributeService.GetByEntityTypeQualifier( entityTypeId, qualifierColumn, qualifierValue, true );

            // Delete any of those attributes that were removed in the UI
            var selectedAttributeGuids = attributes.Select( a => a.Guid );
            foreach ( var attr in existingAttributes.Where( a => !selectedAttributeGuids.Contains( a.Guid ) ) )
            {
                attributeService.Delete( attr );
            }

            rockContext.SaveChanges();

            // Update the Attributes that were assigned in the UI
            foreach ( var attr in attributes )
            {
                Rock.Attribute.Helper.SaveAttributeEdits( attr, entityTypeId, qualifierColumn, qualifierValue, rockContext );
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

                var box = new DetailBlockBox<ContentChannelBag, ContentChannelDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, rockContext )
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
        public BlockActionResult Save( DetailBlockBox<ContentChannelBag, ContentChannelDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new ContentChannelService( rockContext );

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
                if ( !ValidateContentChannel( entity, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                var isNew = entity.Id == 0;

                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );

                    foreach ( var item in new ContentChannelItemService( rockContext )
                        .Queryable()
                        .Where( i =>
                            i.ContentChannelId == entity.Id &&
                            i.ContentChannelTypeId != entity.ContentChannelTypeId
                        ) )
                    {
                        item.ContentChannelTypeId = entity.ContentChannelTypeId;
                    }

                    rockContext.SaveChanges();

                    // Save the Item Attributes
                    int entityTypeId = EntityTypeCache.Get( typeof( ContentChannelItem ) ).Id;
                    SaveAttributes( entity.Id, entityTypeId, box.Entity.ItemAttributes, rockContext );
                } );

                if ( isNew )
                {
                    return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                    {
                        [PageParameterKey.ContentChannelId] = entity.IdKey
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
                var entityService = new ContentChannelService( rockContext );

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
        public BlockActionResult RefreshAttributes( DetailBlockBox<ContentChannelBag, ContentChannelDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<ContentChannelBag, ContentChannelDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, rockContext )
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
        /// Gets the content channel status.
        /// </summary>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetContentChannelStatus( string guid )
        {
            using ( var rockContext = new RockContext() )
            {
                var contentChannelType = new ContentChannelTypeService( new RockContext() ).Get( guid.AsGuid() );
                return ActionOk( new { contentChannelType?.DisableStatus, contentChannelType?.DisableContentField } );
            }
        }

        /// <summary>
        /// Gets the relevant message concerning the license used for uploaded Content based on the selected LicenseType.
        /// </summary>
        /// <param name="selectedLicenseType">Type of the selected license.</param>
        /// <param name="idKey">The identifier key.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetLicenseMessage( Guid? selectedLicenseType, string idKey )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( selectedLicenseType.HasValue )
                {
                    var contentChannelId = Rock.Utility.IdHasher.Instance.GetId( idKey );

                    if ( contentChannelId.HasValue )
                    {
                        var contentChannel = new ContentChannelService( rockContext )
                            .Queryable( "ContentChannelType" )
                            .Where( t => t.Id == contentChannelId )
                            .FirstOrDefault();

                        var contentLibraryConfiguration = contentChannel?.ContentLibraryConfiguration;

                        if ( contentLibraryConfiguration != null && contentLibraryConfiguration.LicenseTypeValueGuid.HasValue && contentLibraryConfiguration.LicenseTypeValueGuid != selectedLicenseType )
                        {
                            var oldLicenseType = DefinedValueCache.Get( contentLibraryConfiguration.LicenseTypeValueGuid.Value );
                            var newLicenseType = DefinedValueCache.Get( selectedLicenseType.Value );
                            string message;

                            if ( newLicenseType.Guid != Rock.SystemGuid.DefinedValue.LIBRARY_LICENSE_TYPE_OPEN.AsGuid() )
                            {
                                message = $"Future items will be uploaded with the license of \"{newLicenseType.Value}\". Items previously uploaded will retain the \"{oldLicenseType.Value}\" license.";
                            }
                            else
                            {
                                message = $"Future items will be uploaded with the license of \"{newLicenseType.Value}\". Items previously uploaded will retain the \"{oldLicenseType.Value}\" license. If you would like to change your existing items to \"{newLicenseType.Value}\", please reach out to us at <a href=\"mailto:info@sparkdevnetwork.org\">info@sparkdevnetwork.org</a>.";
                            }

                            return ActionOk( new { IsMessageVisible = true, Message = message } );
                        }
                    }
                }

                return ActionOk( new { IsMessageVisible = false, Message = "" } );
            }
        }

        #endregion
    }
}
