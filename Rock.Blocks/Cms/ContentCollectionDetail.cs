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
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Cms.ContentCollection;
using Rock.Cms.ContentCollection.IndexDocuments;
using Rock.Cms.ContentCollection.Search;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Cms.ContentCollectionDetail;
using Rock.ViewModels.Cms;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    /// Displays the details of a particular content collection.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />

    [DisplayName( "Content Collection Detail" )]
    [Category( "CMS" )]
    [Description( "Displays the details of a particular content collection." )]
    [IconCssClass( "fa fa-book-open" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "5C8A2E36-6CCC-42C7-8AAF-1C0B4A42B48B" )]
    [Rock.SystemGuid.BlockTypeGuid( "D840AE7E-7226-4D84-AFA9-3F2C84947BDD" )]
    public class ContentCollectionDetail : RockDetailBlockType
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string ContentCollectionId = "ContentCollectionId";
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
                var box = new DetailBlockBox<ContentCollectionBag, ContentCollectionDetailOptionsBag>();

                SetBoxInitialEntityState( box, true, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<ContentCollection>();

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private ContentCollectionDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new ContentCollectionDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the ContentCollection for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="contentCollection">The ContentCollection to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the ContentCollection is valid, <c>false</c> otherwise.</returns>
        private bool ValidateContentCollection( ContentCollection contentCollection, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            if ( contentCollection.CollectionKey.IsNullOrWhiteSpace() )
            {
                errorMessage = "Collection Key is required.";
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
        private void SetBoxInitialEntityState( DetailBlockBox<ContentCollectionBag, ContentCollectionDetailOptionsBag> box, bool loadAttributes, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity != null )
            {
                var isViewable = BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
                box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

                if ( loadAttributes )
                {
                    entity.LoadAttributes( rockContext );
                }

                if ( entity.Id != 0 )
                {
                    // Existing entity was found, prepare for view mode by default.
                    if ( isViewable )
                    {
                        box.Entity = GetEntityBagForView( entity, loadAttributes, rockContext );
                        box.SecurityGrantToken = GetSecurityGrantToken( entity );
                    }
                    else
                    {
                        box.ErrorMessage = EditModeMessage.NotAuthorizedToView( ContentCollection.FriendlyTypeName );
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
                        box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( ContentCollection.FriendlyTypeName );
                    }
                }
            }
            else
            {
                box.ErrorMessage = $"The {ContentCollection.FriendlyTypeName} was not found.";
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="ContentCollectionBag"/> that represents the entity.</returns>
        private ContentCollectionBag GetCommonEntityBag( ContentCollection entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new ContentCollectionBag
            {
                IdKey = entity.IdKey,
                Description = entity.Description,
                EnableRequestFilters = entity.EnableRequestFilters,
                EnableSegments = entity.EnableSegments,
                LastIndexDateTime = entity.LastIndexDateTime,
                LastIndexItemCount = entity.LastIndexItemCount,
                CollectionKey = entity.CollectionKey,
                Name = entity.Name,
                TrendingEnabled = entity.TrendingEnabled,
                TrendingGravity = entity.TrendingGravity,
                TrendingMaxItems = entity.TrendingMaxItems,
                TrendingWindowDay = entity.TrendingWindowDay
            };
        }

        /// <summary>
        /// Gets the bag for viewing the specied entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <param name="loadAttributes"><c>true</c> if attributes and values should be loaded; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>A <see cref="ContentCollectionBag"/> that represents the entity.</returns>
        private ContentCollectionBag GetEntityBagForView( ContentCollection entity, bool loadAttributes, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            if ( loadAttributes )
            {
                bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );
            }

            bag.Sources = entity.ContentCollectionSources
                .OrderBy( s => s.Order )
                .ThenBy( s => s.Id )
                .Select( s => GetContentSourceBag( s, rockContext ) )
                .Where( s => s != null )
                .ToList();

            bag.FilterSettings = GetFilterSettingsBag( entity, rockContext );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specied entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <param name="loadAttributes"><c>true</c> if attributes and values should be loaded; otherwise <c>false</c>.</param>
        /// <returns>A <see cref="ContentCollectionBag"/> that represents the entity.</returns>
        private ContentCollectionBag GetEntityBagForEdit( ContentCollection entity, bool loadAttributes )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

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
        private bool UpdateEntityFromBox( ContentCollection entity, DetailBlockBox<ContentCollectionBag, ContentCollectionDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            var contentCollectionService = new ContentCollectionService( rockContext );

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.EnableRequestFilters ),
                () => entity.EnableRequestFilters = box.Entity.EnableRequestFilters );

            box.IfValidProperty( nameof( box.Entity.EnableSegments ),
                () => entity.EnableSegments = box.Entity.EnableSegments );

            box.IfValidProperty( nameof( box.Entity.CollectionKey ), () =>
            {
                var collectionId = entity.Id != 0 ? ( int? ) entity.Id : null;
                entity.CollectionKey = contentCollectionService.GetUniqueSlug( box.Entity.CollectionKey, collectionId );
            } );

            box.IfValidProperty( nameof( box.Entity.Name ),
                () => entity.Name = box.Entity.Name );

            box.IfValidProperty( nameof( box.Entity.TrendingEnabled ),
                () => entity.TrendingEnabled = box.Entity.TrendingEnabled );

            box.IfValidProperty( nameof( box.Entity.TrendingMaxItems ),
                () => entity.TrendingMaxItems = box.Entity.TrendingMaxItems );

            box.IfValidProperty( nameof( box.Entity.TrendingWindowDay ),
                () => entity.TrendingWindowDay = box.Entity.TrendingWindowDay );

            box.IfValidProperty( nameof( box.Entity.TrendingGravity ),
                () => entity.TrendingGravity = box.Entity.TrendingGravity );

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
        /// <returns>The <see cref="ContentCollection"/> to be viewed or edited on the page.</returns>
        private ContentCollection GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<ContentCollection, ContentCollectionService>( rockContext, PageParameterKey.ContentCollectionId );
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
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out ContentCollection entity, out BlockActionResult error, Func<IQueryable<ContentCollection>, IQueryable<ContentCollection>> qryAdditions = null )
        {
            var entityService = new ContentCollectionService( rockContext );
            IQueryable<ContentCollection> qry;
            error = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                qry = entityService.GetQueryableByKey( idKey, !PageCache.Layout.Site.DisablePredictableIds );

                if ( qryAdditions != null )
                {
                    qry = qryAdditions( qry );
                }

                entity = qry.SingleOrDefault();
            }
            else
            {
                // Create a new entity.
                entity = new ContentCollection();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{ContentCollection.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${ContentCollection.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Translates the <paramref name="source"/> into a bag that can be
        /// sent to the client to display and edit the source.
        /// </summary>
        /// <param name="source">The collection source that will be sent to the client.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>A new <see cref="ContentSourceBag"/> instance that represents <paramref name="source"/>.</returns>
        private static ContentSourceBag GetContentSourceBag( ContentCollectionSource source, RockContext rockContext )
        {
            var contentChannelEntityTypeId = EntityTypeCache.GetId<ContentChannel>() ?? 0;
            var eventCalendarEntityTypeId = EntityTypeCache.GetId<EventCalendar>() ?? 0;
            string name;
            Guid entityGuid;
            string color;
            string iconCssClass;
            int itemCount;

            // Process the entity as a content channel source.
            if ( source.EntityTypeId == contentChannelEntityTypeId )
            {
                var contentChannel = new ContentChannelService( rockContext ).Get( source.EntityId );

                if ( contentChannel == null )
                {
                    return null;
                }

                name = contentChannel.Name;
                entityGuid = contentChannel.Guid;
                itemCount = new ContentChannelItemService( rockContext ).Queryable()
                    .Where( cci => cci.ContentChannelId == contentChannel.Id )
                    .Count();
                color = "#009ce3";
                iconCssClass = contentChannel.IconCssClass.ToStringOrDefault( "fa fa-bullhorn" );
            }

            // Process the entity as an event calendar source.
            else if ( source.EntityTypeId == eventCalendarEntityTypeId )
            {
                var eventCalendar = new EventCalendarService( rockContext ).Get( source.EntityId );

                if ( eventCalendar == null )
                {
                    return null;
                }

                name = eventCalendar.Name;
                entityGuid = eventCalendar.Guid;
                itemCount = new EventCalendarItemService( rockContext ).Queryable()
                    .Where( cci => cci.EventCalendarId == eventCalendar.Id )
                    .Count();
                color = "#09ae77";
                iconCssClass = eventCalendar.IconCssClass.ToStringOrDefault( "fa fa-calendar-alt" );
            }
            else
            {
                return null;
            }

            // Get the additional settings or a default object.
            var additionalSettings = source.AdditionalSettings
                    .FromJsonOrNull<ContentCollectionSourceAdditionalSettingsBag>() ?? new ContentCollectionSourceAdditionalSettingsBag();

            // Create the bag that represents the source.
            return new ContentSourceBag
            {
                Guid = source.Guid,
                Name = name,
                EntityTypeGuid = EntityTypeCache.Get( source.EntityTypeId ).Guid,
                EntityGuid = entityGuid,
                Color = color,
                IconCssClass = iconCssClass,
                OccurrencesToShow = source.OccurrencesToShow,
                ItemCount = itemCount,
                Attributes = additionalSettings?.AttributeGuids
                    .Select( g => AttributeCache.Get( g ) )
                    .Where( g => g != null )
                    .Select( g => new ListItemBag
                    {
                        Value = g.Guid.ToString(),
                        Text = g.Name
                    } )
                    .ToList() ?? new List<ListItemBag>(),
                CustomFields = additionalSettings?.CustomFields ?? new List<ContentCollectionCustomFieldBag>()
            };
        }

        /// <summary>
        /// Get the filter settings bag object that represents the filter
        /// settings for the given content collection.
        /// </summary>
        /// <param name="collection">The content collection that contains the filter settings.</param>
        /// <param name="rockContext">The context to use for accessing the database.</param>
        /// <returns>A <see cref="FilterSettingsBag"/> instance that represents the filter settings of the content collection.</returns>
        private static FilterSettingsBag GetFilterSettingsBag( ContentCollection collection, RockContext rockContext )
        {
            var filterSettings = collection.FilterSettings.FromJsonOrNull<ContentCollectionFilterSettingsBag>() ?? new ContentCollectionFilterSettingsBag();
            var attributeFilters = GetAttributeFilters( collection, filterSettings, rockContext );
            var customFieldFilters = GetCustomFieldFilters( collection, filterSettings, rockContext );

            return new FilterSettingsBag
            {
                FullTextSearchEnabled = filterSettings.FullTextSearchEnabled,
                YearSearchEnabled = filterSettings.YearSearchEnabled,
                YearSearchLabel = filterSettings.YearSearchLabel,
                YearSearchFilterControl = filterSettings.YearSearchFilterControl,
                YearSearchFilterIsMultipleSelection = filterSettings.YearSearchFilterIsMultipleSelection,
                AttributeFilters = attributeFilters,
                CustomFieldFilters = customFieldFilters
            };
        }

        /// <summary>
        /// Gets the attribute filters for the content collection. This is an
        /// amalgamation of the attributes enabled on all the sources as well
        /// as the current filter settings.
        /// </summary>
        /// <param name="collection">The content collection whose filters should be retrieved.</param>
        /// <param name="filterSettings">The current filter settings of the collection.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>A collection of <see cref="AttributeFilterBag"/> objects that represent the filters.</returns>
        private static List<AttributeFilterBag> GetAttributeFilters( ContentCollection collection, ContentCollectionFilterSettingsBag filterSettings, RockContext rockContext )
        {
            var contentChannelEntityTypeId = EntityTypeCache.GetId<ContentChannel>() ?? 0;
            var eventCalendarEntityTypeId = EntityTypeCache.GetId<EventCalendar>() ?? 0;

            // Get a list of all source attributes that are enabled
            // for indexing.
            var sourceAttributes = collection.ContentCollectionSources
                .SelectMany( cls =>
                {
                    var sourceSettings = cls.AdditionalSettings.FromJsonOrNull<ContentCollectionSourceAdditionalSettingsBag>();
                    string name;

                    if ( sourceSettings == null )
                    {
                        return null;
                    }

                    // Get the name of the source entity.
                    if ( cls.EntityTypeId == contentChannelEntityTypeId )
                    {
                        name = new ContentChannelService( rockContext )
                            .GetSelect( cls.EntityId, cc => cc.Name );
                    }
                    else if ( cls.EntityTypeId == eventCalendarEntityTypeId )
                    {
                        name = new EventCalendarService( rockContext )
                            .GetSelect( cls.EntityId, cc => cc.Name );
                    }
                    else
                    {
                        name = null;
                    }

                    // No name means something is invalid about the source.
                    if ( name.IsNullOrWhiteSpace() )
                    {
                        return null;
                    }

                    // Get all the attributes that are enabled for this source.
                    var attributes = sourceSettings.AttributeGuids != null
                        ? sourceSettings.AttributeGuids
                            .Select( g => AttributeCache.Get( g ) )
                            .Where( g => g != null )
                            .ToList()
                        : new List<AttributeCache>();

                    // Return a set that associates the source name with the attribute(s).
                    return attributes.Select( a => new Tuple<string, AttributeCache>( name, a ) );
                } )
                .ToList();

            // Group the source attributes by attribute key and then get the
            // attribute filter associated with that key.
            return sourceAttributes.GroupBy( a => a.Item2.Key )
                .Select( ga => GetAttributeFilterBag( ga.ToList(), ga.Key, filterSettings.AttributeFilters?.GetValueOrNull( ga.Key ) ) )
                .ToList();
        }

        /// <summary>
        /// Gets the custom field filters for the content collection. This is an
        /// amalgamation of the custom fields on all the sources as well as the
        /// current filter settings.
        /// </summary>
        /// <param name="collection">The content collection whose filters should be retrieved.</param>
        /// <param name="filterSettings">The current filter settings of the collection.</param>
        /// <param name="rockContext">The context to use when accessing the database.</param>
        /// <returns>A collection of <see cref="CustomFieldFilterBag"/> objects that represent the filters.</returns>
        private static List<CustomFieldFilterBag> GetCustomFieldFilters( ContentCollection collection, ContentCollectionFilterSettingsBag filterSettings, RockContext rockContext )
        {
            var contentChannelEntityTypeId = EntityTypeCache.GetId<ContentChannel>() ?? 0;
            var eventCalendarEntityTypeId = EntityTypeCache.GetId<EventCalendar>() ?? 0;

            // Get a list of all source custom fields.
            var sourceFields = collection.ContentCollectionSources
                .SelectMany( cls =>
                {
                    var sourceSettings = cls.AdditionalSettings.FromJsonOrNull<ContentCollectionSourceAdditionalSettingsBag>();
                    string name;

                    if ( sourceSettings == null )
                    {
                        return null;
                    }

                    // Get the name of the source entity.
                    if ( cls.EntityTypeId == contentChannelEntityTypeId )
                    {
                        name = new ContentChannelService( rockContext )
                            .GetSelect( cls.EntityId, cc => cc.Name );
                    }
                    else if ( cls.EntityTypeId == eventCalendarEntityTypeId )
                    {
                        name = new EventCalendarService( rockContext )
                            .GetSelect( cls.EntityId, cc => cc.Name );
                    }
                    else
                    {
                        name = null;
                    }

                    // No name means something is invalid about the source.
                    if ( name.IsNullOrWhiteSpace() )
                    {
                        return null;
                    }

                    return sourceSettings.CustomFields
                        .Select( cf => new Tuple<string, ContentCollectionCustomFieldBag>( name, cf ) );
                } )
                .ToList();

            // Group the source fields by key and then get the filter
            // associated with that key.
            return sourceFields.GroupBy( a => a.Item2.Key )
                .Select( ga => GetCustomFieldFilterBag( ga.ToList(), ga.Key, filterSettings.CustomFieldFilters?.GetValueOrNull( ga.Key ) ) )
                .ToList();
        }

        /// <summary>
        /// Gets the filter bag that represents a single attribute key.
        /// </summary>
        /// <param name="attributes">The attributes that represent this filter key.</param>
        /// <param name="attributeKey">The common key to the attributes.</param>
        /// <param name="settings">The previously saved settings for this filter or <c>null</c>.</param>
        /// <returns>An <see cref="AttributeFilterBag"/> that represents the filter for these attributes.</returns>
        private static AttributeFilterBag GetAttributeFilterBag( List<Tuple<string, AttributeCache>> attributes, string attributeKey, ContentCollectionAttributeFilterSettingsBag settings )
        {
            var firstAttribute = attributes.First();
            bool isInconsistent = false;

            // Skip the first attribute and then compare it to all the others
            // to see if there are any inconsistencies.
            for ( int i = 1; i < attributes.Count; i++ )
            {
                if ( !IsAttributeConfigurationIdentical( firstAttribute.Item2, attributes[i].Item2 ) )
                {
                    isInconsistent = true;
                }
            }

            var filterBag = new AttributeFilterBag
            {
                AttributeKey = attributeKey,
                AttributeName = firstAttribute.Item2.Name,
                IsEnabled = settings?.IsEnabled ?? false,
                IsInconsistent = isInconsistent,
                FieldTypeName = firstAttribute.Item2.FieldType.Name,
                FieldTypeGuid = firstAttribute.Item2.FieldType.Guid,
                SourceNames = attributes.Select( a => a.Item1 ).ToList(),
                FilterLabel = (settings?.Label).ToStringOrDefault( firstAttribute.Item2.Name ),
                FilterControl = settings?.FilterControl ?? Enums.Cms.ContentCollectionFilterControl.Pills,
                IsMultipleSelection = settings?.IsMultipleSelection ?? false
            };

            return filterBag;
        }

        /// <summary>
        /// Gets the filter bag that represents a single custom field key.
        /// </summary>
        /// <param name="fields">The fields that represent this filter key.</param>
        /// <param name="fieldKey">The common key to the fields.</param>
        /// <param name="settings">The previously saved settings for this filter or <c>null</c>.</param>
        /// <returns>An <see cref="AttributeFilterBag"/> that represents the filter for these fields.</returns>
        private static CustomFieldFilterBag GetCustomFieldFilterBag( List<Tuple<string, ContentCollectionCustomFieldBag>> fields, string fieldKey, ContentCollectionCustomFieldFilterSettingsBag settings )
        {
            var firstField = fields.First();

            var filterBag = new CustomFieldFilterBag
            {
                Key = fieldKey,
                Title = firstField.Item2.Title,
                IsEnabled = settings?.IsEnabled ?? false,
                SourceNames = fields.Select( a => a.Item1 ).ToList(),
                FilterLabel = ( settings?.Label ).ToStringOrDefault( firstField.Item2.Title ),
                FilterControl = settings?.FilterControl ?? Enums.Cms.ContentCollectionFilterControl.Pills,
                IsMultipleSelection = settings?.IsMultipleSelection ?? false
            };

            return filterBag;
        }

        /// <summary>
        /// Checks if two attributes have identical configuration. This is a
        /// rough comparison but should be good enough for our use case.
        /// </summary>
        /// <param name="attribute">The first attribute to be compared.</param>
        /// <param name="otherAttribute">The second attribute to be compared.</param>
        /// <returns><c>true</c> if both attributes can be considered identical; otherwise <c>false</c>.</returns>
        private static bool IsAttributeConfigurationIdentical( AttributeCache attribute, AttributeCache otherAttribute )
        {
            if ( attribute.FieldTypeId != otherAttribute.FieldTypeId )
            {
                return false;
            }

            // Build a rough comparison of the configuration values. Ignore
            // any blank or whitespace values, then sort by key, then concat
            // into one giant string.
            var attributeConfig = attribute.ConfigurationValues
                .Where( cv => cv.Value.IsNotNullOrWhiteSpace() )
                .OrderBy( cv => cv.Key )
                .Select( cv => $"{cv.Key}{cv.Value}" )
                .JoinStrings( string.Empty );

            var otherAttributeConfig = otherAttribute.ConfigurationValues
                .Where( cv => cv.Value.IsNotNullOrWhiteSpace() )
                .OrderBy( cv => cv.Key )
                .Select( cv => $"{cv.Key}{cv.Value}" )
                .JoinStrings( string.Empty );

            return attributeConfig == otherAttributeConfig;
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

                var box = new DetailBlockBox<ContentCollectionBag, ContentCollectionDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, true )
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
        public BlockActionResult Save( DetailBlockBox<ContentCollectionBag, ContentCollectionDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new ContentCollectionService( rockContext );

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
                if ( !ValidateContentCollection( entity, rockContext, out var validationMessage ) )
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
                        [PageParameterKey.ContentCollectionId] = entity.IdKey
                    } ) );
                }

                // Ensure navigation properties will work now.
                entity = entityService.Get( entity.Id );
                entity.LoadAttributes( rockContext );

                return ActionOk( GetEntityBagForView( entity, true, rockContext ) );
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
                var entityService = new ContentCollectionService( rockContext );

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
        public BlockActionResult RefreshAttributes( DetailBlockBox<ContentCollectionBag, ContentCollectionDetailOptionsBag> box )
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

                var refreshedBox = new DetailBlockBox<ContentCollectionBag, ContentCollectionDetailOptionsBag>
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
        /// Gets a list of all the content channels that are available for the
        /// individual to use when adding or editing a content channel source.
        /// </summary>
        /// <returns>A collection of <see cref="AvailableContentSourceBag"/> objects.</returns>
        [BlockAction]
        public BlockActionResult GetAvailableContentChannels()
        {
            using ( var rockContext = new RockContext() )
            {
                var contentChannels = new ContentChannelService( rockContext )
                    .Queryable()
                    .OrderBy( c => c.Name )
                    .ToList()
                    .Where( c => c.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                    .ToList();

                var availableSources = contentChannels
                    .Select( c =>
                    {
                        var item = new ContentChannelItem
                        {
                            ContentChannelId = c.Id,
                            ContentChannelTypeId = c.ContentChannelTypeId
                        };

                        item.LoadAttributes( rockContext );

                        return new AvailableContentSourceBag
                        {
                            Guid = c.Guid,
                            Name = c.Name,
                            Attributes = item.Attributes.Select( a => new ListItemBag
                            {
                                Value = a.Value.Guid.ToString(),
                                Text = a.Value.Name,
                                Category = a.Value.FieldType.Name
                            } ).ToList()
                        };
                    } );

                return ActionOk( availableSources );
            }
        }

        /// <summary>
        /// Gets a list of all the event calendars that are available for the
        /// individual to use when adding or editing a event calendar source.
        /// </summary>
        /// <returns>A collection of <see cref="AvailableContentSourceBag"/> objects.</returns>
        [BlockAction]
        public BlockActionResult GetAvailableEventCalendars()
        {
            using ( var rockContext = new RockContext() )
            {
                var eventCalendars = new EventCalendarService( rockContext )
                    .Queryable()
                    .OrderBy( ec => ec.Name )
                    .ToList()
                    .Where( ec => ec.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                    .ToList();

                var availableSources = eventCalendars
                    .Select( ec =>
                    {
                        var item = new EventCalendarItem
                        {
                            EventCalendarId = ec.Id
                        };

                        item.LoadAttributes( rockContext );

                        return new AvailableContentSourceBag
                        {
                            Guid = ec.Guid,
                            Name = ec.Name,
                            Attributes = item.Attributes.Select( a => new ListItemBag
                            {
                                Value = a.Value.Guid.ToString(),
                                Text = a.Value.Name,
                                Category = a.Value.FieldType.Name
                            } ).ToList()
                        };
                    } );

                return ActionOk( availableSources );
            }
        }

        /// <summary>
        /// Saves the edits to a collection source. This will either update an
        /// existing collection source or create a new one.
        /// </summary>
        /// <param name="key">The identifier of the content collection to be modified.</param>
        /// <param name="bag">The source to be added or updated.</param>
        /// <returns>The detail box that contains the new entity information to be displayed.</returns>
        [BlockAction]
        public async Task<BlockActionResult> SaveCollectionSource( string key, ContentSourceBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var contentCollectionService = new ContentCollectionService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var collection, out var actionError, qry => qry.Include( l => l.ContentCollectionSources ) ) )
                {
                    return actionError;
                }

                int entityTypeId = 0;
                int entityId = 0;

                // Find the source entity to be added. Either a content channel
                // or an event calendar.
                if ( bag.EntityTypeGuid == SystemGuid.EntityType.CONTENT_CHANNEL.AsGuid() )
                {
                    // Verify the content channel specified exists and that the
                    // person has access to it.
                    var contentChannel = new ContentChannelService( rockContext ).Get( bag.EntityGuid );

                    if ( contentChannel == null )
                    {
                        return ActionNotFound( "Content channel was not found." );
                    }

                    if ( !contentChannel.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                    {
                        return ActionForbidden( "Not authorized to view this content channel." );
                    }

                    entityTypeId = EntityTypeCache.GetId<ContentChannel>().Value;
                    entityId = contentChannel.Id;
                }
                else if ( bag.EntityTypeGuid == SystemGuid.EntityType.EVENT_CALENDAR.AsGuid() )
                {
                    // Verify the event calendar specified exists and that the
                    // person has access to it.
                    var eventCalendar = new EventCalendarService( rockContext ).Get( bag.EntityGuid );

                    if ( eventCalendar == null )
                    {
                        return ActionNotFound( "Event calendar was not found." );
                    }

                    if ( !eventCalendar.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                    {
                        return ActionForbidden( "Not authorized to view this event calendar." );
                    }

                    entityTypeId = EntityTypeCache.GetId<EventCalendar>().Value;
                    entityId = eventCalendar.Id;
                }
                else
                {
                    return ActionBadRequest( "Invalid source type." );
                }

                // Find the existing matching source or create a new one.
                var source = collection.ContentCollectionSources
                    .Where( s => s.EntityTypeId == entityTypeId && s.EntityId == entityId )
                    .FirstOrDefault();

                if ( source == null )
                {
                    source = new ContentCollectionSource
                    {
                        EntityTypeId = entityTypeId,
                        EntityId = entityId
                    };
                    collection.ContentCollectionSources.Add( source );

                    source.Order = collection.ContentCollectionSources
                        .Select( cls => cls.Order + 1 )
                        .Max();
                }

                // Update the source with the new settings.
                var additionalSettings = new ContentCollectionSourceAdditionalSettingsBag
                {
                    AttributeGuids = bag.Attributes?.Select( a => a.Value.AsGuid() ).ToList() ?? new List<Guid>(),
                    CustomFields = bag.CustomFields
                };

                source.OccurrencesToShow = bag.OccurrencesToShow;
                source.AdditionalSettings = additionalSettings.ToJson();

                rockContext.SaveChanges();

                // Load the attributes on the collection and return the new entity bag.
                collection.LoadAttributes( rockContext );

                var box = new DetailBlockBox<ContentCollectionBag, ContentCollectionDetailOptionsBag>
                {
                    Entity = GetEntityBagForView( collection, true, rockContext )
                };

                // Make sure the indexes are created and ready to go.
                await ContentIndexContainer.CreateAllIndexesAsync();

                return ActionOk( box );
            }
        }

        /// <summary>
        /// Deletes an existing collection source from the content collection.
        /// </summary>
        /// <param name="key">The identifier of the content collection to be modified.</param>
        /// <param name="sourceGuid">The unique identifier of the source to be deleted.</param>
        /// <returns>The detail box that contains the new entity information to be displayed.</returns>
        [BlockAction]
        public BlockActionResult DeleteCollectionSource( string key, Guid sourceGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var contentCollectionService = new ContentCollectionService( rockContext );
                var contentCollectionSourceService = new ContentCollectionSourceService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var collection, out var actionError, qry => qry.Include( l => l.ContentCollectionSources ) ) )
                {
                    return actionError;
                }

                // Find the existing matching source or create a new one.
                var source = collection.ContentCollectionSources
                    .Where( s => s.Guid == sourceGuid )
                    .SingleOrDefault();

                if ( source == null )
                {
                    return ActionNotFound( "Source was not found." );
                }

                // Delete the source.
                contentCollectionSourceService.Delete( source );
                rockContext.SaveChanges();

                // Send back a new box with the entity to display.
                collection.LoadAttributes( rockContext );

                var box = new DetailBlockBox<ContentCollectionBag, ContentCollectionDetailOptionsBag>
                {
                    Entity = GetEntityBagForView( collection, true, rockContext )
                };

                return ActionOk( box );
            }
        }

        /// <summary>
        /// Changes the ordered position of a single source in the content collection.
        /// </summary>
        /// <param name="key">The identifier of the content collection whose sources will be reordered.</param>
        /// <param name="guid">The unique identifier of the source that will be moved.</param>
        /// <param name="beforeGuid">The unique identifier of the source it will be placed before.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult ReorderSource( string key, Guid guid, Guid? beforeGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var contentCollectionService = new ContentCollectionService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var collection, out var actionError, qry => qry.Include( l => l.ContentCollectionSources ) ) )
                {
                    return actionError;
                }

                // Put them in a properly ordered list.
                var sources = collection.ContentCollectionSources
                    .OrderBy( s => s.Order )
                    .ThenBy( s => s.Id )
                    .ToList();

                if ( !sources.ReorderEntity( guid.ToString(), beforeGuid?.ToString() ) )
                {
                    return ActionBadRequest( "Invalid reorder attempt." );
                }

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        /// <summary>
        /// Saves the filter settings for the content collection.
        /// </summary>
        /// <param name="key">The identifier that of the content collection that should be updated.</param>
        /// <param name="box">The details about the filter settings that should be updated.</param>
        /// <returns>A new content collection bag that represents the new state of the content collection.</returns>
        [BlockAction]
        public BlockActionResult SaveFilterSettings( string key, DetailBlockBox<FilterSettingsBag, ContentCollectionDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var contentCollectionService = new ContentCollectionService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var collection, out var actionError, qry => qry.Include( l => l.ContentCollectionSources ) ) )
                {
                    return actionError;
                }

                var filterSettings = collection.FilterSettings.FromJsonOrNull<ContentCollectionFilterSettingsBag>() ?? new ContentCollectionFilterSettingsBag();
                filterSettings.AttributeFilters = filterSettings.AttributeFilters ?? new Dictionary<string, ContentCollectionAttributeFilterSettingsBag>();
                filterSettings.CustomFieldFilters = filterSettings.CustomFieldFilters ?? new Dictionary<string, ContentCollectionCustomFieldFilterSettingsBag>();
                var attributeFilters = GetAttributeFilters( collection, filterSettings, rockContext );
                var customFieldFilters = GetCustomFieldFilters( collection, filterSettings, rockContext );

                // Update all the basic properties.
                box.IfValidProperty( nameof( box.Entity.FullTextSearchEnabled ),
                    () => filterSettings.FullTextSearchEnabled = box.Entity.FullTextSearchEnabled );

                box.IfValidProperty( nameof( box.Entity.YearSearchEnabled ),
                    () => filterSettings.YearSearchEnabled = box.Entity.YearSearchEnabled );

                box.IfValidProperty( nameof( box.Entity.YearSearchFilterControl ),
                    () => filterSettings.YearSearchFilterControl = box.Entity.YearSearchFilterControl );

                box.IfValidProperty( nameof( box.Entity.YearSearchFilterIsMultipleSelection ),
                    () => filterSettings.YearSearchFilterIsMultipleSelection = box.Entity.YearSearchFilterIsMultipleSelection );

                box.IfValidProperty( nameof( box.Entity.YearSearchLabel ),
                    () => filterSettings.YearSearchLabel = box.Entity.YearSearchLabel );

                // Update the attribute filters.
                var hasInvalidAttributeFilter = false;
                box.IfValidProperty( nameof( box.Entity.AttributeFilters ),
                    () =>
                    {
                        var booleanFieldTypeGuid = SystemGuid.FieldType.BOOLEAN.AsGuid();

                        foreach ( var attributeFilter in box.Entity.AttributeFilters )
                        {
                            var filter = attributeFilters.FirstOrDefault( f => f.AttributeKey == attributeFilter.AttributeKey );

                            // This should only happen if internal data changed
                            // while they were editing, but catch it so they know
                            // their changes were not saved.
                            if ( filter == null )
                            {
                                hasInvalidAttributeFilter = true;
                                return;
                            }

                            // Don't let them modify an inconsistent filter, but
                            // do not throw an error because we don't know they
                            // actually tried to change any values.
                            if ( filter.IsInconsistent )
                            {
                                return;
                            }

                            // Get the existing settings or create a new one.
                            if ( !filterSettings.AttributeFilters.TryGetValue( filter.AttributeKey, out var filterSetting ) )
                            {
                                filterSetting = new ContentCollectionAttributeFilterSettingsBag();
                                filterSettings.AttributeFilters.Add( filter.AttributeKey, filterSetting );
                            }

                            filterSetting.IsEnabled = attributeFilter.IsEnabled;
                            filterSetting.Label = attributeFilter.FilterLabel;

                            if ( filter.FieldTypeGuid == booleanFieldTypeGuid )
                            {
                                // Just force set these values when it's a boolean.
                                filterSetting.FilterControl = Enums.Cms.ContentCollectionFilterControl.Boolean;
                                filterSetting.IsMultipleSelection = false;
                            }
                            else
                            {
                                // Make sure the value is valid.
                                if ( attributeFilter.FilterControl != Enums.Cms.ContentCollectionFilterControl.Pills && attributeFilter.FilterControl != Enums.Cms.ContentCollectionFilterControl.Dropdown )
                                {
                                    hasInvalidAttributeFilter = true;
                                    return;
                                }

                                filterSetting.FilterControl = attributeFilter.FilterControl;
                                filterSetting.IsMultipleSelection = attributeFilter.IsMultipleSelection;
                            }
                        }
                    } );

                if ( hasInvalidAttributeFilter )
                {
                    return ActionBadRequest( "Invalid attribute filter." );
                }

                // Update the custom field filters.
                bool hasInvalidCustomFieldFilter = false;
                box.IfValidProperty( nameof( box.Entity.CustomFieldFilters ),
                    () =>
                    {
                        var newCustomFilters = new Dictionary<string, ContentCollectionCustomFieldFilterSettingsBag>();

                        foreach ( var customFieldFilter in box.Entity.CustomFieldFilters )
                        {
                            var filter = customFieldFilters.FirstOrDefault( f => f.Key == customFieldFilter.Key );

                            // This should only happen if internal data changed
                            // while they were editing, but catch it so they know
                            // their changes were not saved.
                            if ( filter == null )
                            {
                                hasInvalidCustomFieldFilter = true;
                                return;
                            }

                            // Get the existing settings or create a new one.
                            if ( !filterSettings.CustomFieldFilters.TryGetValue( filter.Key, out var filterSetting ) )
                            {
                                filterSetting = new ContentCollectionCustomFieldFilterSettingsBag();
                            }

                            filterSetting.IsEnabled = customFieldFilter.IsEnabled;
                            filterSetting.Label = customFieldFilter.FilterLabel;

                            // Make sure the value is valid.
                            if ( customFieldFilter.FilterControl != Enums.Cms.ContentCollectionFilterControl.Pills && customFieldFilter.FilterControl != Enums.Cms.ContentCollectionFilterControl.Dropdown )
                            {
                                hasInvalidCustomFieldFilter = true;
                                return;
                            }

                            filterSetting.FilterControl = customFieldFilter.FilterControl;
                            filterSetting.IsMultipleSelection = customFieldFilter.IsMultipleSelection;

                            newCustomFilters.Add( filter.Key, filterSetting );
                        }

                        filterSettings.CustomFieldFilters = newCustomFilters;
                    } );

                if ( hasInvalidCustomFieldFilter )
                {
                    return ActionBadRequest( "Invalid custom field filter." );
                }

                // Clean up any attribute filters that no longer exist.
                var validKeys = attributeFilters.Select( f => f.AttributeKey ).ToList();
                var keysToRemove = filterSettings.AttributeFilters.Keys
                    .Where( k => !validKeys.Contains( k ) )
                    .ToList();
                foreach ( var invalidKey in keysToRemove )
                {
                    filterSettings.AttributeFilters.Remove( invalidKey );
                }

                collection.FilterSettings = filterSettings.ToJson();

                rockContext.SaveChanges();

                // Send back a new box with the entity to display.
                collection.LoadAttributes( rockContext );

                var newBox = new DetailBlockBox<ContentCollectionBag, ContentCollectionDetailOptionsBag>
                {
                    Entity = GetEntityBagForView( collection, true, rockContext )
                };

                return ActionOk( newBox );
            }
        }

        /// <summary>
        /// Rebuilds the index for the content collection.
        /// </summary>
        /// <returns>BlockActionResult.</returns>
        [BlockAction]
        public async Task<BlockActionResult> RebuildIndex( string key )
        {
            int contentCollectionId;

            using ( var rockContext = new RockContext() )
            {
                var contentCollectionService = new ContentCollectionService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var collection, out var actionError ) )
                {
                    return actionError;
                }

                // Reset the cached field values.
                var filterSettings = collection.FilterSettings.FromJsonOrNull<ContentCollectionFilterSettingsBag>() ?? new ContentCollectionFilterSettingsBag();
                filterSettings.FieldValues = new Dictionary<string, List<ListItemBag>>();
                filterSettings.AttributeValues = new Dictionary<string, List<ListItemBag>>();
                collection.FilterSettings = filterSettings.ToJson();

                rockContext.SaveChanges();

                contentCollectionId = collection.Id;
            }

            var indexableEntityTypes = EntityTypeCache.All().Where( et => et.IsContentCollectionIndexingEnabled );
            var options = new IndexDocumentOptions
            {
                IsTrendingEnabled = true
            };

            // Create all the indexes if any don't exist.
            await ContentIndexContainer.CreateAllIndexesAsync();

            // Then update all sources for this collection.
            var sources = ContentCollectionSourceCache.All()
                .Where( s => s.ContentCollectionId == contentCollectionId )
                .ToList();

            var rebuildTask = Task.Run( async () =>
            {
                // Delete all the documents associated with each source.
                var deleteQuery = new SearchQuery
                {
                    IsAllMatching = false
                };

                foreach ( var source in sources )
                {
                    deleteQuery.Add( new SearchField
                    {
                        Name = nameof( IndexDocumentBase.SourceId ),
                        Value = source.Id.ToString(),
                        IsPhrase = true,
                        IsWildcard = false
                    } );
                }

                await ContentIndexContainer.DeleteMatchingDocumentsAsync( deleteQuery );

                // Add new documents for each source.
                int indexedCount = 0;
                foreach ( var entityTypeCache in indexableEntityTypes )
                {
                    var indexer = ( IContentCollectionIndexer ) Activator.CreateInstance( entityTypeCache.ContentCollectionIndexerType );

                    foreach ( var source in sources )
                    {
                        indexedCount += await indexer.IndexAllContentCollectionSourceDocumentsAsync( source.Id, options );
                    }
                }

                // Update the last index values.
                using ( var rockContext2 = new RockContext() )
                {
                    var updateContentCollection = new ContentCollectionService( rockContext2 ).Get( contentCollectionId );

                    updateContentCollection.LastIndexDateTime = RockDateTime.Now;
                    updateContentCollection.LastIndexItemCount = indexedCount;

                    rockContext2.SaveChanges();
                }
            } );

            // Wait up to 10 seconds for the index rebuild to complete. It will
            // keep running but we won't wait for it anymore.
            if ( await Task.WhenAny( rebuildTask, Task.Delay( 10000 ) ) == rebuildTask )
            {
                return ActionOk( "Content collection index has been rebuilt." );
            }
            else
            {
                return ActionOk( "Content collection index rebuild is taking longer than expected, it will continue to process in the background." );
            }
        }

        #endregion
    }
}
