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
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Reporting;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.HistoryLog;
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Block for displaying the history of changes to a particular entity.
    /// </summary>
    [DisplayName( "History Log" )]
    [Category( "Core" )]
    [Description( "Block for displaying the history of changes to a particular entity." )]
    [SupportedSiteTypes( Model.SiteType.Web )]
    [ContextAware]

    #region Block Attributes

    [TextField( "Heading",
        Description = "The Lava template to use for the heading. <span class='tip tip-lava'></span>",
        IsRequired = false,
        DefaultValue = "{{ Entity.EntityStringValue }} (ID:{{ Entity.Id }})",
        Order = 0,
        Key = AttributeKey.Heading )]

    [CategoryField( "Category",
        Description = "When selected, only history for this category will be shown and the Category column will be hidden.",
        IsRequired = false,
        AllowMultiple = false,
        Order = 1,
        Key = AttributeKey.Category,
        EntityType = typeof( Rock.Model.History ) )]

    #endregion Block Attributes

    [Rock.Cms.DefaultBlockRole( Rock.Enums.Cms.BlockRole.Secondary )]
    [Rock.SystemGuid.EntityTypeGuid( "BF092DB1-4A3E-4A57-B57A-EAA9D131742E" )]
    [Rock.SystemGuid.BlockTypeGuid( "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "82B83030-04AE-4618-BB61-D562814BDE32" )]
    [CustomizedGrid]
    public class HistoryLog : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string Heading = "Heading";
            public const string Category = "Category";
        }

        private static class PreferenceKey
        {
            public const string FilterCategory = "filter-category";
            public const string FilterDateRangeLowerValue = "filter-date-range-lower-value";
            public const string FilterDateRangeUpperValue = "filter-date-range-upper-value";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<HistoryLogOptionsBag>();
            var builder = GetGridBuilder();
            var contextEntity = GetContextEntity();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.GridDefinition = builder.BuildDefinition();
            box.Options = GetBoxOptions( contextEntity );

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the block.
        /// </summary>
        /// <param name="contextEntity">The current context entity.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private HistoryLogOptionsBag GetBoxOptions( IEntity contextEntity )
        {
            var options = new HistoryLogOptionsBag
            {
                HasContextEntity = contextEntity != null,
                IsCategoryColumnVisible = !GetConfiguredCategoryId().HasValue,
                IsCategoryFilterVisible = !GetConfiguredCategoryId().HasValue
            };

            if ( contextEntity == null )
            {
                return options;
            }

            var mergeFields = RequestContext.GetCommonMergeFields();
            mergeFields.Add( "Entity", contextEntity );

            options.PanelTitle = GetAttributeValue( AttributeKey.Heading ).ResolveMergeFields( mergeFields );
            options.ExportFileName = this.PageCache?.PageTitle;

            if ( contextEntity is IModel model && model.CreatedDateTime.HasValue )
            {
                options.CreatedDateText = $"Date Created: {model.CreatedDateTime.Value.ToShortDateString()}";
            }

            return options;
        }

        /// <summary>
        /// Gets the configured category identifier from the block settings.
        /// </summary>
        /// <returns>The configured category identifier, if any.</returns>
        private int? GetConfiguredCategoryId()
        {
            var categoryGuid = GetAttributeValue( AttributeKey.Category ).AsGuidOrNull();
            return categoryGuid.HasValue
                ? CategoryCache.GetId( categoryGuid.Value )
                : null;
        }

        /// <summary>
        /// Gets the selected category identifier from the individual's preferences.
        /// </summary>
        /// <returns>The selected category identifier, if any.</returns>
        private int? GetFilterCategoryId()
        {
            var category = GetBlockPersonPreferences()
                .GetValue( PreferenceKey.FilterCategory )
                .FromJsonOrNull<ListItemBag>();

            var categoryGuid = category?.Value?.AsGuidOrNull();
            return categoryGuid.HasValue
                ? CategoryCache.GetId( categoryGuid.Value )
                : null;
        }

        /// <summary>
        /// Gets the effective category identifier to use when filtering.
        /// </summary>
        /// <returns>The effective category identifier, if any.</returns>
        private int? GetEffectiveCategoryId()
        {
            return GetConfiguredCategoryId() ?? GetFilterCategoryId();
        }

        /// <summary>
        /// Gets the lower date bound from the grid settings.
        /// </summary>
        /// <returns>The lower date bound, if any.</returns>
        private DateTime? GetFilterDateRangeLowerValue()
        {
            return GetBlockPersonPreferences().GetValue( PreferenceKey.FilterDateRangeLowerValue ).AsDateTime();
        }

        /// <summary>
        /// Gets the upper date bound from the grid settings.
        /// </summary>
        /// <returns>The upper date bound, if any.</returns>
        private DateTime? GetFilterDateRangeUpperValue()
        {
            return GetBlockPersonPreferences().GetValue( PreferenceKey.FilterDateRangeUpperValue ).AsDateTime();
        }

        /// <summary>
        /// Gets the grid builder that will provide all the details and values of the grid.
        /// </summary>
        /// <returns>An instance of <see cref="GridBuilder{T}"/>.</returns>
        private GridBuilder<HistoryLogListItemInfo> GetGridBuilder()
        {
            return new GridBuilder<HistoryLogListItemInfo>()
                .WithBlock( this )
                .AddTextField( "idKey", i => i.FirstHistoryId.AsIdKey() )
                .AddTextField( "categoryName", i => i.CategoryName )
                .AddField( "createdByPerson", i => i.CreatedByPerson )
                .AddField( "historyList", i => i.HistoryList )
                .AddTextField( "historySummaryText", i => i.HistorySummaryText )
                .AddTextField( "historySummarySortValue", i => i.HistorySummarySortValue )
                .AddTextField( "formattedCaption", i => i.FormattedCaption )
                .AddTextField( "captionText", i => i.CaptionText )
                .AddDateTimeField( "createdDateTime", i => i.CreatedDateTime );
        }

        /// <summary>
        /// Gets the list of grouped history items for the current context entity.
        /// </summary>
        /// <param name="rockContext">The database context.</param>
        /// <param name="targetEntity">The entity whose history is being queried.</param>
        /// <returns>A list of history rows formatted for the grid.</returns>
        private List<HistoryLogListItemInfo> GetHistoryItems( RockContext rockContext, IEntity targetEntity )
        {
            var historyQry = GetFilteredHistoryQuery( rockContext, targetEntity )
                .OrderByDescending( h => h.CreatedDateTime );

            var qryPerson = new PersonService( rockContext ).Queryable( true, true );

            /*
                08/21/2024 - JSC

                Refactored the group join queryable below to select only necessary columns
                and to remove sorts from the materialized result set.
                Due to the volume of data in the table we want to optimize this query
                as much as possible.

                Reason: Performance
            */

            // Apply the History record Grouping to get the number of History list items.
            // History records are grouped by Date Created, Entity, Category, etc)
            var historyActionGroupQuery = historyQry
                .GroupJoin( qryPerson, h => h.CreatedByPersonAlias.PersonId, p => p.Id,
                    ( h, p ) => new
                    {
                        History = h,
                        Person = p
                    } )
                .SelectMany( o => o.Person.DefaultIfEmpty(),
                    ( g, p ) => new
                    {
                        CreatedDateTime = g.History.CreatedDateTime.Value,
                        EntityId = g.History.EntityId,
                        CategoryId = g.History.CategoryId,
                        CategoryName = g.History.Category.Name,
                        RelatedEntityTypeId = g.History.RelatedEntityTypeId,
                        RelatedEntityId = g.History.RelatedEntityId,
                        CreatedByPersonId = g.History.CreatedByPersonAlias.PersonId,
                        CreatedByPersonNickName = p.NickName,
                        CreatedByPersonLastName = p.LastName,
                        History = g.History
                    } )
                .GroupBy( a => new
                    {
                        a.CreatedDateTime,
                        a.EntityId,
                        a.CategoryId,
                        a.CategoryName,
                        a.RelatedEntityTypeId,
                        a.RelatedEntityId,
                        a.CreatedByPersonId,
                        a.CreatedByPersonNickName,
                        a.CreatedByPersonLastName
                    } )
                .Select( x => new HistorySummaryGroupResultItem
                    {
                        CreatedDateTime = x.Key.CreatedDateTime,
                        EntityId = x.Key.EntityId,
                        CategoryId = x.Key.CategoryId,
                        CategoryName = x.Key.CategoryName,
                        RelatedEntityTypeId = x.Key.RelatedEntityTypeId,
                        RelatedEntityId = x.Key.RelatedEntityId,
                        CreatedByPersonId = ( int? ) x.Key.CreatedByPersonId,
                        CreatedByPersonNickName = x.Key.CreatedByPersonNickName,
                        CreatedByPersonLastName = x.Key.CreatedByPersonLastName,
                        HistoryEntries = x.Select( h => h.History )
                    } )
                // Apply ordering to the grouped history items.
                .OrderByDescending( x => x.CreatedDateTime )
                .ToList();

            // Materialize the result.
            var historySummaryList = historyActionGroupQuery
                .Select( x =>
                {
                    var orderedSourceEntries = x.HistoryEntries
                        .OrderBy( h => h.Id )
                        .ToList();

                    var historyList = orderedSourceEntries
                        .Select( h => new History
                        {
                            Verb = h.Verb,
                            ChangeType = h.ChangeType,
                            IsSensitive = h.IsSensitive,
                            NewValue = h.NewValue,
                            ValueName = h.ValueName,
                            OldValue = h.OldValue,
                            RelatedData = h.RelatedData,
                            EntityTypeId = h.EntityTypeId
                        }.SummaryHtml )
                        .ToList();

                    var firstHistoryEntry = orderedSourceEntries.FirstOrDefault();

                    return new HistoryLogListItemInfo
                    {
                        CreatedDateTime = x.CreatedDateTime,
                        FirstHistoryId = firstHistoryEntry?.Id ?? 0,
                        CategoryName = x.CategoryName,
                        CreatedByPerson = x.CreatedByPersonId.HasValue
                            ? new PersonFieldBag
                            {
                                IdKey = x.CreatedByPersonId.Value.AsIdKey(),
                                NickName = x.CreatedByPersonNickName,
                                LastName = x.CreatedByPersonLastName
                            }
                            : null,
                        HistoryList = historyList,
                        HistorySummaryText = historyList
                            .Where( h => h.IsNotNullOrWhiteSpace() )
                            .Select( h => h.StripHtml() )
                            .ToList()
                            .AsDelimited( ", " ),
                        HistorySummarySortValue = $"{firstHistoryEntry?.Verb} {firstHistoryEntry?.ValueName}".Trim(),
                        CaptionText = firstHistoryEntry?.Caption ?? string.Empty,
                        FormattedCaption = GetFormattedCaption(
                            firstHistoryEntry?.Caption,
                            x.CategoryId,
                            x.EntityId,
                            x.RelatedEntityTypeId,
                            x.RelatedEntityId )
                    };
                } )
                .ToList();

            return historySummaryList;
        }

        /// <summary>
        /// Gets the filtered history query for the specified target entity.
        /// </summary>
        /// <param name="rockContext">The database context.</param>
        /// <param name="targetEntity">The entity whose history is being queried.</param>
        /// <returns>An <see cref="IQueryable{T}"/> containing the filtered history entries.</returns>
        private IQueryable<History> GetFilteredHistoryQuery( RockContext rockContext, IEntity targetEntity )
        {
            if ( targetEntity == null )
            {
                throw new Exception( "A context entity is required." );
            }

            var targetEntityTypeId = targetEntity.TypeId;
            var targetEntityId = targetEntity.Id;

            if ( targetEntityTypeId == 0 )
            {
                throw new Exception( "The target entity type must be specified." );
            }

            if ( targetEntityId == 0 )
            {
                throw new Exception( "The target entity identifier must be specified." );
            }

            var historyService = new HistoryService( rockContext );
            var historyQry = historyService.Queryable().AsNoTracking();

            if ( targetEntityTypeId == EntityTypeCache.GetId<Person>() )
            {
                // Person history also includes family history, while still honoring
                // attribute and related-entity security for the current viewer.
                // If this is History for a Person, also include any History for any of their Families
                int? groupEntityTypeId = EntityTypeCache.GetId<Rock.Model.Group>();

                List<int> familyIds = new PersonService( rockContext )
                    .GetFamilies( targetEntityId )
                    .Select( a => a.Id )
                    .ToList();

                historyQry = historyQry.Where( h =>
                    ( h.EntityTypeId == targetEntityTypeId && h.EntityId == targetEntityId )
                    || ( h.EntityTypeId == groupEntityTypeId && familyIds.Contains( h.EntityId ) ) );

                // as per issue #1594, if relatedEntityType is an Attribute then check View Authorization
                var attributeEntity = EntityTypeCache.Get( Rock.SystemGuid.EntityType.ATTRIBUTE.AsGuid() );
                if ( attributeEntity != null )
                {
                    var allowedAttributeIds = GetAuthorizedPersonAttributes( rockContext )
                        .Select( a => a.Id )
                        .ToList();

                    historyQry = historyQry.Where( a =>
                        a.RelatedEntityTypeId == attributeEntity.Id
                            ? allowedAttributeIds.Contains( a.RelatedEntityId.Value )
                            : true );
                }

                // as per issue #5332(https://github.com/SparkDevNetwork/Rock/issues/5332), ensure user is Authorized to view related entity.
                var allowedRelatedEntityIds = GetAuthorizedRelatedEntityIds( historyService, historyQry ).ToList();

                historyQry = historyQry.Where( a =>
                    !a.RelatedEntityId.HasValue
                    || allowedRelatedEntityIds.Contains( a.RelatedEntityId.Value ) );
            }
            else
            {
                historyQry = historyQry.Where( h =>
                    h.EntityTypeId == targetEntityTypeId
                    && h.EntityId == targetEntityId );
            }

            var historyCategories = CategoryCache.AllForEntityType<Rock.Model.History>();
            var allowedCategoryIds = historyCategories
                .Where( a => a.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .Select( a => a.Id )
                .ToList();

            historyQry = historyQry.Where( a => allowedCategoryIds.Contains( a.CategoryId ) );

            var effectiveCategoryId = GetEffectiveCategoryId();
            if ( effectiveCategoryId.HasValue )
            {
                historyQry = historyQry.Where( a => a.CategoryId == effectiveCategoryId.Value );
            }

            var lowerDate = GetFilterDateRangeLowerValue();
            if ( lowerDate.HasValue )
            {
                historyQry = historyQry.Where( h => h.CreatedDateTime >= lowerDate.Value );
            }

            var upperDate = GetFilterDateRangeUpperValue();
            if ( upperDate.HasValue )
            {
                var exclusiveUpperDate = upperDate.Value.Date.AddDays( 1 );
                historyQry = historyQry.Where( h => h.CreatedDateTime < exclusiveUpperDate );
            }

            return historyQry;
        }

        /// <summary>
        /// Gets the person attributes that the current user is authorized to view.
        /// </summary>
        /// <param name="rockContext">The database context.</param>
        /// <returns>A list of authorized person attributes.</returns>
        private List<AttributeCache> GetAuthorizedPersonAttributes( RockContext rockContext )
        {
            // Start with the more obvious attributes that are directly for a person
            var allPersonAttributes = AttributeCache.AllForEntityType<Person>();

            // Filter these down to the attributes that the current person is allowed to view
            var allowedPersonAttributes = allPersonAttributes
                .Where( a => a.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                .ToList();

            // Add the attributes that are part of a matrix that is for a person
            // We know which attributes are matrices according to the field type
            var matrixFieldType = FieldTypeCache.Get( Rock.SystemGuid.FieldType.MATRIX );
            var personMatrixAttributes = allowedPersonAttributes
                .Where( pa => pa.FieldType == matrixFieldType );

            if ( personMatrixAttributes.Any() )
            {
                // Each matrix has a template. The template defines which attributes make up the values of the matrix
                var templateKey = "attributematrixtemplate";
                var templateIds = personMatrixAttributes
                    .Select( a => a.QualifierValues.ContainsKey( templateKey ) ? a.QualifierValues[templateKey].Value : null )
                    .Where( i => i.IsNotNullOrWhiteSpace() );

                if ( templateIds.Any() )
                {
                    var matrixItemEntityTypeId = EntityTypeCache.GetId<AttributeMatrixItem>();
                    var allMatrixAttributes = new AttributeService( rockContext )
                        .GetByEntityTypeId( matrixItemEntityTypeId )
                        .AsNoTracking()
                        .Where( a =>
                            a.EntityTypeQualifierColumn == "AttributeMatrixTemplateId"
                            && templateIds.Contains( a.EntityTypeQualifierValue ) )
                        .ToList()
                        .Select( AttributeCache.Get );

                    // Of the attributes within the person matrix templates, add those that are authorized to view
                    var allowedMatrixAttributes = allMatrixAttributes
                        .Where( a => a.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                        .ToList();

                    allowedPersonAttributes.AddRange( allowedMatrixAttributes );
                }
            }

            return allowedPersonAttributes;
        }

        /// <summary>
        /// Gets the identifiers of related entities the current user is authorized to view.
        /// </summary>
        /// <param name="historyService">The history service.</param>
        /// <param name="historyQry">The current filtered history query.</param>
        /// <returns>A list of authorized related entity identifiers.</returns>
        private List<int> GetAuthorizedRelatedEntityIds( HistoryService historyService, IQueryable<History> historyQry )
        {
            var relatedEntityIds = new List<int>();
            var relatedEntityTypeIdList = historyQry
                .Where( a => a.RelatedEntityTypeId.HasValue )
                .Select( a => a.RelatedEntityTypeId.Value )
                .Distinct()
                .ToList();

            // find all the EntityTypes that are used as the History.RelatedEntityTypeId records
            foreach ( var relatedEntityTypeId in relatedEntityTypeIdList )
            {
                // for each entityType, query whatever it is (for example Person) so that we can get that Entity and its Id to check if the current user can view it.
                var entityLookup = historyService.GetEntityQuery( relatedEntityTypeId )
                    .AsNoTracking()
                    .Where( a => historyQry.Any( h => h.RelatedEntityTypeId == relatedEntityTypeId && h.RelatedEntityId == a.Id ) )
                    .AsEnumerable()
                    .ToDictionary( k => k.Id, v => v );

                var authorizedEntitiesLookup = entityLookup
                    .Where( el => !( el.Value is ISecured secured ) || secured.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
                    .ToList();

                relatedEntityIds.AddRange( authorizedEntitiesLookup.Select( e => e.Key ) );
            }

            return relatedEntityIds;
        }

        /// <summary>
        /// Gets the formatted caption HTML for the history row.
        /// </summary>
        /// <param name="caption">The caption text.</param>
        /// <param name="categoryId">The category identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="relatedEntityTypeId">The related entity type identifier.</param>
        /// <param name="relatedEntityId">The related entity identifier.</param>
        /// <returns>The HTML string to display in the What column.</returns>
        private static string GetFormattedCaption( string caption, int categoryId, int entityId, int? relatedEntityTypeId, int? relatedEntityId )
        {
            caption = caption ?? string.Empty;
            var encodedCaption = caption.EncodeHtml();

            if ( categoryId == 0 )
            {
                return encodedCaption;
            }

            var categoryCache = CategoryCache.Get( categoryId );
            var urlMask = categoryCache?.GetAttributeValue( "UrlMask" );

            if ( urlMask.IsNullOrWhiteSpace() )
            {
                return encodedCaption;
            }

            string virtualUrl = string.Empty;
            IEntity iEntity = null;

            if ( relatedEntityTypeId.HasValue && relatedEntityId.HasValue )
            {
                var relatedEntityType = EntityTypeCache.Get( relatedEntityTypeId.Value );
                if ( relatedEntityType != null )
                {
                    iEntity = Reflection.GetIEntityForEntityType( relatedEntityType.GetEntityType(), relatedEntityId.Value );
                }
            }

            if ( urlMask.Contains( "{0}" ) && iEntity != null )
            {
                virtualUrl = string.Format( urlMask, relatedEntityId.Value.ToString(), entityId.ToString() );
            }

            if ( virtualUrl.IsNullOrWhiteSpace() )
            {
                return encodedCaption;
            }

            string resolvedUrl = System.Web.HttpContext.Current == null
                ? virtualUrl
                : System.Web.VirtualPathUtility.ToAbsolute( virtualUrl );

            return $"<a href='{resolvedUrl}'>{encodedCaption}</a>";
        }

        #endregion Methods

        #region Block Actions

        /// <summary>
        /// Gets the grid data to be displayed in the block.
        /// </summary>
        /// <returns>An action result that contains the grid data.</returns>
        [BlockAction]
        public BlockActionResult GetGridData()
        {
            var contextEntity = GetContextEntity();

            if ( contextEntity == null )
            {
                return ActionOk( new GridDataBag
                {
                    Rows = new List<Dictionary<string, object>>()
                } );
            }

            try
            {
                var gridDataBag = GetGridBuilder().Build( GetHistoryItems( RockContext, contextEntity ) );
                return ActionOk( gridDataBag );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );

                var sqlException = ReportingHelper.FindSqlTimeoutException( ex );
                var errorMessage = sqlException?.Message ?? ex.Message;

                return ActionBadRequest( $"An error occurred trying to retrieve the history. Please adjust the filter settings and try again. Error: {errorMessage}" );
            }
        }

        #endregion Block Actions

        #region Support Classes

        /// <summary>
        /// An interim data structure used for internal query processing.
        /// </summary>
        private class HistorySummaryGroupResultItem
        {
            public DateTime? CreatedDateTime { get; set; }

            public int EntityId { get; set; }

            public int CategoryId { get; set; }

            public string CategoryName { get; set; }

            public int? RelatedEntityTypeId { get; set; }

            public int? RelatedEntityId { get; set; }

            public int? CreatedByPersonId { get; set; }

            public string CreatedByPersonNickName { get; set; }

            public string CreatedByPersonLastName { get; set; }

            public IEnumerable<History> HistoryEntries { get; set; }
        }

        /// <summary>
        /// Represents an item in the History Log grid.
        /// </summary>
        private class HistoryLogListItemInfo
        {
            public DateTime? CreatedDateTime { get; set; }

            public int FirstHistoryId { get; set; }

            public string CategoryName { get; set; }

            public PersonFieldBag CreatedByPerson { get; set; }

            public List<string> HistoryList { get; set; }

            public string HistorySummaryText { get; set; }

            public string HistorySummarySortValue { get; set; }

            public string CaptionText { get; set; }

            public string FormattedCaption { get; set; }
        }

        #endregion Support Classes
    }
}