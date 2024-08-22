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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Field.Types;
using Rock.Model;
using Rock.Reporting;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using static Rock.Model.HistoryService;
using static Rock.Web.UI.Controls.Grid;
using static RockWeb.Blocks.Core.HistoryLog.HistoryLogGridDataSource;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Block for displaying the history of changes to a particular entity.
    /// </summary>
    [DisplayName( "History Log" )]
    [Category( "Core" )]
    [Description( "Block for displaying the history of changes to a particular entity." )]

    [ContextAware]

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

    [Rock.SystemGuid.BlockTypeGuid( "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0" )]
    public partial class HistoryLog : RockBlock, ISecondaryBlock
    {
        public static class AttributeKey
        {
            public const string Heading = "Heading";
            public const string Category = "Category";
        }

        #region Fields

        private IEntity _entity = null;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;
            gfSettings.DisplayFilterValue += gfSettings_DisplayFilterValue;

            gHistory.GridRebind += gHistory_GridRebind;
            gHistory.DataKeyNames = new string[] { "FirstHistoryId" };

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbMessage.Visible = false;

            _entity = this.ContextEntity();
            if ( _entity != null )
            {
                if ( !Page.IsPostBack )
                {
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                    mergeFields.Add( "Entity", _entity );
                    lHeading.Text = GetAttributeValue( AttributeKey.Heading ).ResolveMergeFields( mergeFields );

                    BindFilter();
                    BindGrid();

                    IModel model = _entity as IModel;
                    if ( model != null && model.CreatedDateTime.HasValue )
                    {
                        hlDateAdded.Text = String.Format( "Date Created: {0}", model.CreatedDateTime.Value.ToShortDateString() );
                    }
                    else
                    {
                        hlDateAdded.Visible = false;
                    }
                }
            }
            
	        base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            int? categoryId = cpCategory.SelectedValueAsInt();
            gfSettings.SetFilterPreference( "Category", categoryId.HasValue ? categoryId.Value.ToString() : "" );

            gfSettings.SetFilterPreference( "Summary Contains", tbSummary.Text );

            int? personId = ppWhoFilter.PersonId;
            gfSettings.SetFilterPreference( "Who", personId.HasValue ? personId.ToString() : string.Empty );

            gfSettings.SetFilterPreference( "Date Range", drpDates.DelimitedValues );

            BindGrid();
        }

        /// <summary>
        /// Handles the DisplayFilterValue event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void gfSettings_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Category":
                    {
                        int? categoryId = e.Value.AsIntegerOrNull();
                        if ( cpCategory.Visible && categoryId.HasValue )
                        {
                            var category = CategoryCache.Get( categoryId.Value );
                            if ( category != null )
                            {
                                e.Value = category.Name;
                            }
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }
                case "Summary Contains":
                    {
                        break;
                    }
                case "Who":
                    {
                        int personId = int.MinValue;
                        if ( int.TryParse( e.Value, out personId ) )
                        {
                            var person = new PersonService( new RockContext() ).GetNoTracking( personId );
                            if ( person != null )
                            {
                                e.Value = person.FullName;
                            }
                        }
                        break;
                    }
                case "Date Range":
                    {
                        e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                        break;
                    }
                default:
                    {
                        e.Value = string.Empty;
                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gHistory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void gHistory_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the category identifier.
        /// </summary>
        /// <returns>System.Nullable&lt;System.Int32&gt;.</returns>
        private int? GetCategoryId()
        {
            var categoryGuidBlockAttribute = this.GetAttributeValue( AttributeKey.Category ).AsGuidOrNull();
            if ( categoryGuidBlockAttribute.HasValue )
            {
                return CategoryCache.GetId( categoryGuidBlockAttribute.Value );
            }
            else
            {
                return gfSettings.GetFilterPreference( "Category" ).AsIntegerOrNull();
            }
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            var categoryGuidBlockAttribute = this.GetAttributeValue( AttributeKey.Category ).AsGuidOrNull();
            int? categoryId = GetCategoryId();
            if ( categoryGuidBlockAttribute.HasValue )
            {
                cpCategory.Visible = false;
                var categoryGridField = gHistory.ColumnsOfType<BoundField>().Where( a => a.DataField == "Category.Name" ).FirstOrDefault();
                if ( categoryGridField != null )
                {
                    categoryGridField.Visible = false;
                }
            }
            else
            {
                cpCategory.Visible = true;
            }

            cpCategory.SetValue( categoryId );

            tbSummary.Text = gfSettings.GetFilterPreference( "Summary Contains" );
            int personId = int.MinValue;
            if ( int.TryParse( gfSettings.GetFilterPreference( "Who" ), out personId ) )
            {
                var person = new PersonService( new RockContext() ).Get( personId );
                if ( person != null )
                {
                    ppWhoFilter.SetValue( person );
                }
                else
                {
                    gfSettings.SetFilterPreference( "Who", string.Empty );
                }
            }

            drpDates.DelimitedValues = gfSettings.GetFilterPreference( "Date Range" );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            try
            {
                if ( _entity == null )
                {
                    return;
                }

                var dataSource = new HistoryLogGridDataSource();

                dataSource.CurrentPerson = CurrentPerson;
                dataSource.CategoryId = GetCategoryId();
                dataSource.CreatedByPersonId = gfSettings.GetFilterPreference( "Who" ).AsIntegerOrNull();
                dataSource.TargetEntityTypeId = _entity.TypeId;
                dataSource.TargetEntityId = _entity.Id;

                var drp = new DateRangePicker();
                drp.DelimitedValues = gfSettings.GetFilterPreference( "Date Range" );
                dataSource.LowerDateTime = drp.LowerValue;
                dataSource.UpperDateTime = drp.UpperValue;
                dataSource.SummarySearchText = gfSettings.GetFilterPreference( "Summary Contains" );

                if ( gHistory.SortProperty != null )
                {
                    dataSource.SortType = gHistory.SortProperty.Property.ConvertToEnum<SortSpecifier>( SortSpecifier.CreatedDateTime );
                    dataSource.SortIsDescendingOrder = ( gHistory.SortProperty.Direction == SortDirection.Descending );
                }

                gHistory.EntityTypeId = EntityTypeCache.Get<History>().Id;
                gHistory.SetDataSource( dataSource );
                gHistory.DataBind();
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );

                Exception sqlException = ReportingHelper.FindSqlTimeoutException( ex );

                nbMessage.Visible = true;
                nbMessage.Text = string.Format( "<p>An error occurred trying to retrieve the history. Please try adjusting your filter settings and try again.</p><p>Error: {0}</p>",
                    sqlException != null ? sqlException.Message : ex.Message );

                // Clear the previous grid items.
                gHistory.DataSource = null;
                gHistory.EntityTypeId = EntityTypeCache.Get<History>().Id;
                gHistory.DataBind();
            }
        }

        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on its page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlList.Visible = visible;
        }

        #endregion

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

                    // as per issue #5332(https://github.com/SparkDevNetwork/Rock/issues/5332), ensure user is Authorized to view related entity.
                    var allowedRelatedEntityIds = GetAuthorizedRelatedEntityIds( historyService, historyQry ).ToList();
                    historyQry = historyQry.Where( a => !a.RelatedEntityId.HasValue || allowedRelatedEntityIds.Contains( a.RelatedEntityId.Value ) );
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

            /// <summary>
            /// Gets the ids of related entities the current user is authorized to view.
            /// </summary>
            /// <param name="historyService">The history service.</param>
            /// <param name="historyQry">The history qry.</param>
            /// <returns></returns>
            private List<int> GetAuthorizedRelatedEntityIds( HistoryService historyService, IQueryable<History> historyQry )
            {
                var relatedEntityIds = new List<int>();
                var relatedEntityTypeIdList = historyQry.Where( a => a.RelatedEntityTypeId.HasValue ).Select( a => a.RelatedEntityTypeId.Value ).Distinct().ToList();

                // find all the EntityTypes that are used as the History.RelatedEntityTypeId records
                foreach ( var relatedEntityTypeId in relatedEntityTypeIdList )
                {
                    // for each entityType, query whatever it is (for example Person) so that we can get that Entity and its Id to check if the current user can view it.
                    var entityLookup = historyService.GetEntityQuery( relatedEntityTypeId ).AsNoTracking()
                        .Where( a => historyQry.Any( h => h.RelatedEntityTypeId == relatedEntityTypeId && h.RelatedEntityId == a.Id ) )
                        .AsEnumerable()
                        .ToDictionary( k => k.Id, v => v );

                    var authorizedEntitiesLookup = entityLookup.Where( el => !( el.Value is ISecured secured ) || secured.IsAuthorized( Authorization.VIEW, CurrentPerson ) ).ToList();

                    relatedEntityIds.AddRange( authorizedEntitiesLookup.Select( e => e.Key ) );
                }

                return relatedEntityIds;
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
    }
}