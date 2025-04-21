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
using System.Data.Entity;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
using Rock.Tests.Shared;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using static Rock.Tests.Integration.Crm.HistoryLogGridDataSource;

namespace Rock.Tests.Integration.Crm
{
    /// <summary>
    /// Tests that verify the operation of the History Log.
    /// </summary>
    [TestClass]
    [Ignore("These tests specifically target historical data in the Spark database.")]
    public class HistoryLogTests
    {
        #region Tests

        [TestMethod]
        public void HistoryLogGridDataSource_GetItems()
        {
            var dataSource = GetTestDataSource();

            GetTotalItemCount( dataSource );
            GetPageItems( dataSource, 0 );
            GetPageItems( dataSource, 9 );
        }

        [TestMethod]
        public void HistoryLogGridDataSource_FilterByCreatedBy_ReturnsMatchingItemsOnly()
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            var dataSource = GetTestDataSource();

            var createdByPerson = personService.Queryable()
                .Where( x => x.NickName == "Nick" && x.LastName == "Airdo" )
                .FirstOrDefault();
            dataSource.CreatedByPersonId = createdByPerson.Id;

            GetTotalItemCount( dataSource );
            var pageItems = GetPageItems( dataSource, 0 );

            Assert.IsTrue( pageItems.Any( i => i.CreatedByPersonName == "Nick Airdo" ) );
            Assert.IsFalse( pageItems.Any( i => i.CreatedByPersonName != "Nick Airdo" ) );
        }

        [TestMethod]
        public void HistoryLogGridDataSource_FilterByCategory_ReturnsMatchingItemsOnly()
        {
            var dataSource = GetTestDataSource();

            var communicationCategoryId = CategoryCache.GetId( SystemGuid.Category.HISTORY_PERSON_COMMUNICATIONS.AsGuid() );
            dataSource.CategoryId = communicationCategoryId;

            GetTotalItemCount( dataSource );
            var pageItems = GetPageItems( dataSource, 0 );

            Assert.IsTrue( pageItems.Any( i => i.CategoryName == "Communications" ) );
            Assert.IsFalse( pageItems.Any( i => i.CategoryName != "Communications" ) );
        }

        [TestMethod]
        public void HistoryLogGridDataSource_FilterBySummary_ReturnsMatchingItemsOnly()
        {
            var dataSource = GetTestDataSource();

            dataSource.SummarySearchText = "Communication";

            var pageItems = GetPageItems( dataSource, 0 );

            var items0 = pageItems.Where( i => i.HistoryList.Any( h => h.IndexOf( "Communication", StringComparison.OrdinalIgnoreCase ) >= 0 ) == false ).ToList();

            Assert.IsTrue( pageItems.Any( i => i.HistoryList.Any( h => h.IndexOf( "Communication", StringComparison.OrdinalIgnoreCase ) >= 0 ) ) );
            Assert.IsFalse( pageItems.Any( i => i.HistoryList.Any( h => h.IndexOf( "Communication", StringComparison.OrdinalIgnoreCase ) >= 0 ) == false ) );

        }

        [TestMethod]
        public void HistoryLogGridDataSource_FilterByCreatedDate_ReturnsMatchingItemsOnly()
        {
            var dataSource = GetTestDataSource();
            dataSource.LowerDateTime = RockDateTime.New( 2020, 1, 1 );
            dataSource.UpperDateTime = RockDateTime.New( 2020, 1, 31 );

            GetTotalItemCount( dataSource );
            var pageItems = GetPageItems( dataSource, 0 );

            Assert.IsFalse( pageItems.Any( i => i.CreatedDateTime < dataSource.LowerDateTime || i.CreatedDateTime >= dataSource.UpperDateTime.Value.Date.AddDays( 1 ) ) );
        }

        [TestMethod]
        public void HistoryLogGridDataSource_FilterByCreatedDateLowerBound_ReturnsMatchingItemsOnly()
        {
            var dataSource = GetTestDataSource();
            dataSource.LowerDateTime = RockDateTime.New( 2020, 1, 1 );
            dataSource.UpperDateTime = null;

            GetTotalItemCount( dataSource );
            var pageItems = GetPageItems( dataSource, 0 );

            Assert.IsFalse( pageItems.Any( i => i.CreatedDateTime < dataSource.LowerDateTime ) );
        }

        [TestMethod]
        public void HistoryLogGridDataSource_FilterByCreatedDateUpperBound_ReturnsMatchingItemsOnly()
        {
            var dataSource = GetTestDataSource();
            dataSource.LowerDateTime = null;
            dataSource.UpperDateTime = RockDateTime.New( 2020, 1, 1 );

            GetTotalItemCount( dataSource );
            var pageItems = GetPageItems( dataSource, 0 );

            Assert.IsFalse( pageItems.Any( i => i.CreatedDateTime >= dataSource.UpperDateTime.Value.Date.AddDays( 1 ) ) );
        }

        #endregion

        private HistoryLogGridDataSource GetTestDataSource()
        {
            var rockContext = new RockContext();
            var personService = new PersonService( rockContext );

            var targetPerson = personService.Queryable()
                .Where( x => x.NickName == "Jon" && x.LastName == "Edmiston" )
                .FirstOrDefault();
            var loginPerson = personService.Queryable()
                .Where( x => x.NickName == "Nick" && x.LastName == "Airdo" )
                .FirstOrDefault();

            var dataSource = new HistoryLogGridDataSource();
            dataSource.CurrentPerson = loginPerson;
            dataSource.TargetEntityTypeId = EntityTypeCache.GetId<Rock.Model.Person>() ?? 0;
            dataSource.TargetEntityId = targetPerson.Id;
            dataSource.DefaultPageSize = 500;

            return dataSource;
        }

        private int GetTotalItemCount( HistoryLogGridDataSource dataSource )
        {
            TestHelper.StartTimer( $"Get Total Count" );

            var totalCount = dataSource.GetTotalItemCount();

            TestHelper.Log( $"Total Item Count = {totalCount} " );
            TestHelper.EndTimer( "Get Total Count" );

            return totalCount;
        }

        private List<HistoryLogListItemInfo> GetPageItems( HistoryLogGridDataSource dataSource, int pageIndex )
        {
            var pageNumber = pageIndex + 1;
            TestHelper.StartTimer( $"Get Page {pageNumber}" );
            var pageItems = dataSource.GetItems( pageIndex );
            TestHelper.EndTimer( $"Get Page {pageNumber}" );

            TestHelper.Log( $"Page Item Count = {pageItems.Count()} " );

            foreach ( var item in pageItems )
            {
                TestHelper.Log( $"[{item.CreatedDateTime}] ({item.CreatedByPersonName}) {item.CategoryName}/{item.Verb}/{item.ValueName}/{item.FormattedCaption}" );
                TestHelper.Log( $"{item.HistoryList.AsDelimited( "\n" )}" );
            };

            return pageItems;
        }
    }

    #region DataSource (Duplicated from HistoryLog.ascx.cs)

    /*
     * This class is duplicated from the HistoryLog.ascx block for testing purposes, because classes defined in Web Forms projects cannot be instantiated from other projects.
     * Changes should be synchronized between these two sources to ensure these unit tests are valid.
     */

    #region HistoryLogGridDataSource

    /// <summary>
    /// Provides data rows for the History Log list.
    /// </summary>
    /// <remarks>
    /// [2023-09-11 DL] Component Added.
    /// </remarks>
    internal class HistoryLogGridDataSource : PaginatedDataSourceBase<HistoryLogListItemInfo>
    {
        public enum SortSpecifier
        {
            CreatedDateTime = 0,
            Category,
            CreatedByPerson,
            Caption
        }

        /// <summary>
        /// The type of entity for which History is being queried.
        /// </summary>
        public int TargetEntityTypeId;

        /// <summary>
        /// The identifier of the entity for which History is being queried.
        /// </summary>
        public int TargetEntityId;

        /// <summary>
        /// An optional filter specifying the Person who created the History record.
        /// </summary>
        public int? CreatedByPersonId;

        /// <summary>
        /// Specifies the sort type for this data source.
        /// </summary>
        /// <remarks>
        /// Sorting is constrained for this data source because the data items represented groupings of related history records.
        /// </remarks>
        public SortSpecifier SortType = SortSpecifier.CreatedDateTime;
        public bool SortIsDescendingOrder = true;

        public DateTime? LowerDateTime;
        public DateTime? UpperDateTime;
        public string SummarySearchText;
        public int? CategoryId;

        /// <summary>
        /// The current user by whom History is being queried.
        /// </summary>
        public Person CurrentPerson;

        /// <summary>
        /// Returns the total number of items that match the filter criteria.
        /// </summary>
        /// <returns></returns>
        public override int GetTotalItemCount()
        {
            int itemCount;

            var hasSummaryTextFilter = !string.IsNullOrWhiteSpace( this.SummarySearchText );
            if ( hasSummaryTextFilter )
            {
                // If the data is filtered by summary text, we need to materialize the entire result set and calculate the summary field to finalize the filter operation.
                // This filter should be removed or reimplemented in the future, because it is a relatively expensive and inefficient operation.
                var items = GetItemsInternal();
                itemCount = items.Count();
            }
            else
            {
                // Apply the History record Grouping to the filtered items to determine the number of History list items.
                var rockContext = new RockContext();
                var historyQry = GetFilteredHistoryQuery( rockContext );
                var historySummaryQry = historyQry
                    .GroupBy( a => new
                    {
                        CreatedDateTime = a.CreatedDateTime.Value,
                        EntityTypeId = a.EntityTypeId,
                        EntityId = a.EntityId,
                        CategoryId = a.CategoryId,
                        RelatedEntityTypeId = a.RelatedEntityTypeId,
                        RelatedEntityId = a.RelatedEntityId,
                        CreatedByPersonAliasId = a.CreatedByPersonAliasId
                    } );

                itemCount = historySummaryQry.Count();
            }

            return itemCount;
        }

        private IQueryable<History> GetOrderedHistoryQuery( RockContext rockContext )
        {
            var historyQry = GetFilteredHistoryQuery( rockContext );

            IQueryable<History> orderedResults;

            var sortProperty = new SortProperty();

            sortProperty.Direction = this.SortIsDescendingOrder ? System.Web.UI.WebControls.SortDirection.Descending : System.Web.UI.WebControls.SortDirection.Ascending;

            if ( this.SortType == SortSpecifier.Category )
            {
                sortProperty.Property = "CategoryName";
            }
            else if ( this.SortType == SortSpecifier.CreatedByPerson )
            {
                sortProperty.Property = "CreatedByPersonName";
            }
            else if ( this.SortType == SortSpecifier.Caption )
            {
                sortProperty.Property = "Caption";
            }
            else
            {
                sortProperty.Property = "CreatedDateTime";
            }

            orderedResults = historyQry.Sort( sortProperty );

            return orderedResults;
        }

        public override List<HistoryLogListItemInfo> GetItems( int pageIndex, int pageSize )
        {
            return GetItemsInternal( pageIndex, pageSize );
        }

        private List<HistoryLogListItemInfo> GetItemsInternal( int? pageIndex = null, int? pageSize = null )
        {
            var rockContext = new RockContext();
            var historyQry = GetOrderedHistoryQuery( rockContext );

            var qryPerson = new PersonService( rockContext ).Queryable( true, true );

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
                    EntityTypeId = g.History.EntityTypeId,
                    EntityId = g.History.EntityId,
                    CategoryId = g.History.CategoryId,
                    CategoryName = g.History.Category.Name,
                    RelatedEntityTypeId = g.History.RelatedEntityTypeId,
                    RelatedEntityId = g.History.RelatedEntityId,
                    CreatedByPersonId = g.History.CreatedByPersonAlias.PersonId,
                    CreatedByPersonNickName = p.NickName,
                    CreatedByPersonLastName = p.LastName,
                    CreatedByPersonSuffixValueId = p.SuffixValueId,

                    History = g.History,
                } )
                .GroupBy( a => new
                {
                    CreatedDateTime = a.CreatedDateTime,
                    EntityTypeId = a.EntityTypeId,
                    EntityId = a.EntityId,
                    CategoryId = a.CategoryId,
                    CategoryName = a.CategoryName,
                    RelatedEntityTypeId = a.RelatedEntityTypeId,
                    RelatedEntityId = a.RelatedEntityId,
                    CreatedByPersonId = a.CreatedByPersonId,

                    CreatedByPersonNickName = a.CreatedByPersonNickName,
                    CreatedByPersonLastName = a.CreatedByPersonLastName,
                    CreatedByPersonSuffixValueId = a.CreatedByPersonSuffixValueId
                } )
            .Select( x => new HistorySummaryGroupResultItem
            {
                CreatedDateTime = x.Key.CreatedDateTime,
                EntityTypeId = x.Key.EntityTypeId,
                EntityId = x.Key.EntityId,
                CategoryId = x.Key.CategoryId,
                CategoryName = x.Key.CategoryName,
                RelatedEntityTypeId = x.Key.RelatedEntityTypeId,
                RelatedEntityId = x.Key.RelatedEntityId,
                CreatedByPersonId = ( int? ) x.Key.CreatedByPersonId,

                CreatedByPersonNickName = x.Key.CreatedByPersonNickName,
                CreatedByPersonLastName = x.Key.CreatedByPersonLastName,
                CreatedByPersonSuffixValueId = x.Key.CreatedByPersonSuffixValueId,

                HistoryEntries = x.Select( h => h.History ).OrderBy( h => h.Id ).ToList(),

                FirstHistoryEntry = x.Select( h => h.History ).OrderBy( h => h.Id ).FirstOrDefault()
            } );

            var hasSummaryTextFilter = !string.IsNullOrWhiteSpace( this.SummarySearchText );
            if ( hasSummaryTextFilter )
            {
                // If the data is filtered by summary text, we will need to materialize the entire result set and calculate the summary field to complete the filter process.
                // This filter should be removed or reimplemented in the future, because is a very expensive and inefficient operation.
                historyActionGroupQuery = historyActionGroupQuery
                    .ToList()
                    .Where( h => h.HistoryEntries.Any( x => x.SummaryHtml != null && x.SummaryHtml.IndexOf( this.SummarySearchText, StringComparison.OrdinalIgnoreCase ) >= 0 ) )
                    .AsQueryable();
            }

            // Apply ordering and pagination to the grouped history items.
            historyActionGroupQuery = historyActionGroupQuery.OrderByDescending( t => t.CreatedDateTime );

            if ( pageIndex != null && pageSize != null )
            {
                historyActionGroupQuery = historyActionGroupQuery.Skip( pageIndex.Value * pageSize.Value ).Take( pageSize.Value );
            }

            // Materialize the result.
            var historySummaryList = historyActionGroupQuery
                .ToList()
                .Select( x => new HistoryLogListItemInfo
                {
                    CreatedDateTime = x.CreatedDateTime,
                    EntityTypeId = x.EntityTypeId,
                    EntityId = x.EntityId,
                    CategoryId = x.CategoryId,
                    CategoryName = x.CategoryName,
                    RelatedEntityTypeId = x.RelatedEntityTypeId,
                    RelatedEntityId = x.RelatedEntityId,
                    CreatedByPersonId = x.CreatedByPersonId,
                    CreatedByPersonName = Person.FormatFullName( x.CreatedByPersonNickName, x.CreatedByPersonLastName, x.CreatedByPersonSuffixValueId ),

                    HistoryList = x.HistoryEntries.Select( h => h.SummaryHtml ).ToList(),

                    FirstHistoryId = x.FirstHistoryEntry.Id,
                    Verb = x.FirstHistoryEntry.Verb,
                    Caption = x.FirstHistoryEntry.Caption,

                    FormattedCaption = HistoryLogListItemInfo.GetFormattedCaption( x.FirstHistoryEntry.Caption, x.CategoryId, x.EntityId, x.RelatedEntityTypeId, x.RelatedEntityId ),

                    ValueName = x.FirstHistoryEntry.ValueName,
                } )
                .ToList();

            return historySummaryList;
        }

        private IQueryable<History> GetFilteredHistoryQuery( RockContext rockContext )
        {
            if ( TargetEntityTypeId == 0 )
            {
                throw new Exception( "The TargetEntityTypeId must be specified." );
            }
            if ( TargetEntityId == 0 )
            {
                throw new Exception( "The TargetEntityId must be specified." );
            }

            var historyService = new HistoryService( rockContext );
            var historyQry = historyService.Queryable().AsNoTracking();

            // Apply Filter: Target Entity.
            if ( TargetEntityTypeId == EntityTypeCache.GetId<Rock.Model.Person>() )
            {
                // If this is History for a Person, also include any History for any of their Families
                int? groupEntityTypeId = EntityTypeCache.GetId<Rock.Model.Group>();

                List<int> familyIds = new PersonService( rockContext ).GetFamilies( this.TargetEntityId ).Select( a => a.Id ).ToList();

                historyQry = historyQry.Where( h => ( h.EntityTypeId == TargetEntityTypeId && h.EntityId == TargetEntityId )
                    || ( h.EntityTypeId == groupEntityTypeId && familyIds.Contains( h.EntityId ) ) );

                // as per issue #1594, if relatedEntityType is an Attribute then check View Authorization
                var attributeEntity = EntityTypeCache.Get( Rock.SystemGuid.EntityType.ATTRIBUTE.AsGuid() );
                var personAttributes = new AttributeService( rockContext ).GetByEntityTypeId( TargetEntityTypeId ).ToList().Select( a => AttributeCache.Get( a ) );
                var allowedAttributeIds = GetAuthorizedPersonAttributes( rockContext ).Select( a => a.Id ).ToList();
                historyQry = historyQry.Where( a => ( a.RelatedEntityTypeId == attributeEntity.Id ) ? allowedAttributeIds.Contains( a.RelatedEntityId.Value ) : true );
            }
            else
            {
                historyQry = historyQry.Where( h => ( h.EntityTypeId == TargetEntityTypeId && h.EntityId == TargetEntityId ) );
            }

            // Apply Filter: Category.
            var historyCategories = CategoryCache.AllForEntityType<Rock.Model.History>();

            var allowedCategoryIds = historyCategories.Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, this.CurrentPerson ) ).Select( a => a.Id ).ToList();

            historyQry = historyQry.Where( a => allowedCategoryIds.Contains( a.CategoryId ) );

            if ( this.CategoryId.HasValue )
            {
                historyQry = historyQry.Where( a => a.CategoryId == this.CategoryId.Value );
            }

            // Apply Filter: Created By.
            if ( this.CreatedByPersonId.HasValue )
            {
                historyQry = historyQry.Where( h => h.CreatedByPersonAlias.PersonId == this.CreatedByPersonId.Value );
            }

            // Apply Filter: Created Date.
            if ( this.LowerDateTime.HasValue )
            {
                historyQry = historyQry.Where( h => h.CreatedDateTime >= this.LowerDateTime.Value );
            }
            if ( this.UpperDateTime.HasValue )
            {
                DateTime upperDate = this.UpperDateTime.Value.Date.AddDays( 1 );
                historyQry = historyQry.Where( h => h.CreatedDateTime < upperDate );
            }

            return historyQry;
        }

        /// <summary>
        /// Gets the person attributes that the current user is authorized to view.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="entityTypeCache">The entity type cache.</param>
        /// <returns></returns>
        private List<AttributeCache> GetAuthorizedPersonAttributes( RockContext rockContext )
        {
            var personEntityTypeId = EntityTypeCache.GetId<Person>();

            // Start with the more obvious attributes that are directly for a person
            var allPersonAttributes = AttributeCache.AllForEntityType<Person>();

            // Filter these down to the attributes that the current person is allowed to view
            var allowedPersonAttributes = allPersonAttributes.Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) ).ToList();

            // Add the attributes that are part of a matrix that is for a person
            // We know which attributes are matrices according to the field type
            var matrixFieldType = FieldTypeCache.Get( Rock.SystemGuid.FieldType.MATRIX );
            var personMatrixAttributes = allowedPersonAttributes.Where( pa => pa.FieldType == matrixFieldType );

            if ( personMatrixAttributes.Any() )
            {
                // Each matrix has a template. The template defines which attributes make up the values of the matrix
                var templateKey = MatrixFieldType.ATTRIBUTE_MATRIX_TEMPLATE;
                var templateIds = personMatrixAttributes
                    .Select( a => a.QualifierValues.ContainsKey( templateKey ) ? a.QualifierValues[templateKey].Value : null )
                    .Where( i => !i.IsNullOrWhiteSpace() );

                if ( templateIds.Any() )
                {
                    var matrixItemEntityTypeId = EntityTypeCache.GetId<AttributeMatrixItem>();
                    var allMatrixAttributes = new AttributeService( rockContext )
                        .GetByEntityTypeId( matrixItemEntityTypeId )
                        .AsNoTracking()
                        .Where( a => a.EntityTypeQualifierColumn == "AttributeMatrixTemplateId" && templateIds.Contains( a.EntityTypeQualifierValue ) )
                        .ToList()
                        .Select( a => AttributeCache.Get( a ) );

                    // Of the attributes within the person matrix templates, add those that are authorized to view
                    var allowedMatrixAttributes = allMatrixAttributes.Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) ).ToList();
                    allowedPersonAttributes.AddRange( allowedMatrixAttributes );
                }
            }

            return allowedPersonAttributes;
        }

        #region Support Classes

        /// <summary>
        /// An interim data structure used for internal query processing.
        /// </summary>
        private class HistorySummaryGroupResultItem
        {
            public DateTime? CreatedDateTime;
            public int EntityTypeId;
            public int EntityId;
            public int CategoryId;
            public string CategoryName;
            public int? RelatedEntityTypeId;
            public int? RelatedEntityId;
            public int? CreatedByPersonId;
            public string CreatedByPersonNickName;
            public string CreatedByPersonLastName;
            public int? CreatedByPersonSuffixValueId;

            public List<History> HistoryEntries;
            public History FirstHistoryEntry;
        }

        /// <summary>
        /// Represents an item in a list of HistoryLog entries.
        /// </summary>
        public class HistoryLogListItemInfo : RockDynamic
        {
            /// <summary>
            /// Gets or sets the created date time.
            /// </summary>
            /// <value>
            /// The created date time.
            /// </value>
            public DateTime? CreatedDateTime { get; set; }

            /// <summary>
            /// Gets the Id of the First History Record (the record that the others are summarized under)
            /// </summary>
            /// <value>
            /// The first history identifier.
            /// </value>
            public int FirstHistoryId { get; set; }

            /// <summary>
            /// Gets or sets the Summary verb (the Verb of the first history record in this summary)
            /// </summary>
            /// <value>
            /// The verb.
            /// </value>
            public string Verb { get; set; }

            /// <summary>
            /// Gets the caption.
            /// </summary>
            /// <value>
            /// The caption.
            /// </value>
            public string Caption { get; set; }

            /// <summary>
            /// Gets the name of the value.
            /// </summary>
            /// <value>
            /// The name of the value.
            /// </value>
            public string ValueName { get; set; }

            /// <summary>
            /// Gets the related data.
            /// </summary>
            /// <value>
            /// The related data.
            /// </value>
            public string RelatedData { get; set; }

            /// <summary>
            /// Gets the name of the created by person.
            /// </summary>
            /// <value>
            /// The name of the created by person.
            /// </value>
            public string CreatedByPersonName { get; set; }

            /// <summary>
            /// Gets the created by person alias identifier.
            /// </summary>
            /// <value>
            /// The created by person alias identifier.
            /// </value>
            public int? CreatedByPersonId { get; set; }

            /// <summary>
            /// Gets or sets the entity type identifier.
            /// </summary>
            /// <value>
            /// The entity type identifier.
            /// </value>
            public int EntityTypeId { get; set; }

            /// <summary>
            /// Gets the name of the entity type.
            /// </summary>
            /// <value>
            /// The name of the entity type.
            /// </value>
            public string EntityTypeName
            {
                get
                {
                    return EntityTypeCache.Get( this.EntityTypeId )?.FriendlyName;
                }
            }

            /// <summary>
            /// Gets or sets the entity identifier.
            /// </summary>
            /// <value>
            /// The entity identifier.
            /// </value>
            public int EntityId { get; set; }

            /// <summary>
            /// Gets or sets the category identifier.
            /// </summary>
            /// <value>
            /// The category identifier.
            /// </value>
            public int CategoryId { get; set; }

            /// <summary>
            /// Gets the category.
            /// </summary>
            /// <value>
            /// The category.
            /// </value>
            public string CategoryName { get; set; }

            /// <summary>
            /// Gets or sets the related entity type identifier.
            /// </summary>
            /// <value>
            /// The related entity type identifier.
            /// </value>
            public int? RelatedEntityTypeId { get; set; }

            /// <summary>
            /// Gets the name of the related entity type.
            /// </summary>
            /// <value>
            /// The name of the related entity type.
            /// </value>
            public string RelatedEntityTypeName
            {
                get
                {
                    if ( RelatedEntityTypeId.HasValue )
                    {
                        return EntityTypeCache.Get( this.RelatedEntityTypeId.Value )?.FriendlyName;
                    }

                    return null;
                }
            }

            /// <summary>
            /// Gets or sets the related entity identifier.
            /// </summary>
            /// <value>
            /// The related entity identifier.
            /// </value>
            public int? RelatedEntityId { get; set; }

            /// <summary>
            /// Gets the formatted caption.
            /// </summary>
            /// <value>
            /// The formatted caption.
            /// </value>
            public string FormattedCaption { get; set; }

            public static string GetFormattedCaption( string caption, int categoryId, int entityId, int? relatedEntityTypeId, int? relatedEntityId )
            {
                if ( categoryId == 0 )
                {
                    return caption;
                }

                var urlMask = CategoryCache.Get( categoryId ).GetAttributeValue( "UrlMask" );
                if ( string.IsNullOrWhiteSpace( urlMask ) )
                {
                    return caption;
                }

                string virtualUrl = string.Empty;
                IEntity iEntity = null;
                if ( relatedEntityTypeId.HasValue && relatedEntityId.HasValue )
                {
                    var relatedEntityType = EntityTypeCache.Get( relatedEntityTypeId.Value );
                    iEntity = Reflection.GetIEntityForEntityType( relatedEntityType.GetEntityType(), relatedEntityId.Value );
                }

                if ( urlMask.Contains( "{0}" ) && iEntity != null )
                {
                    string p1 = relatedEntityId.Value.ToString();
                    string p2 = entityId.ToString();
                    virtualUrl = string.Format( urlMask, p1, p2 );
                }

                string formattedCaption = caption;
                if ( virtualUrl.IsNotNullOrWhiteSpace() )
                {
                    string resolvedUrl;

                    if ( System.Web.HttpContext.Current == null )
                    {
                        resolvedUrl = virtualUrl;
                    }
                    else
                    {
                        resolvedUrl = System.Web.VirtualPathUtility.ToAbsolute( virtualUrl );
                    }

                    formattedCaption = string.Format( "<a href='{0}'>{1}</a>", resolvedUrl, caption );
                }

                return formattedCaption;
            }

            /// <summary>
            /// Gets or sets the list of history summary texts.
            /// </summary>
            /// <value>
            /// The history list.
            /// </value>
            public List<string> HistoryList { get; set; }
        }

        #endregion
    }

    #endregion

    #endregion
}
