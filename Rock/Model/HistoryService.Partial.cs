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
using System.Reflection;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// The data access/service class for the <see cref="Rock.Model.History"/> entity. This inherits from the Service class
    /// </summary>
    public partial class HistoryService
    {
        #region HistorySummary methods

        /// <summary>
        /// Gets the timeline HTML.
        /// </summary>
        /// <param name="timelineLavaTemplate">The timeline lava template.</param>
        /// <param name="primaryEntityType">Type of the primary entity.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="secondaryEntityType">Type of the secondary entity.</param>
        /// <param name="additionalMergeFields">The additional merge fields.</param>
        /// <returns></returns>
        public string GetTimelineHtml( string timelineLavaTemplate, EntityTypeCache primaryEntityType, int entityId, EntityTypeCache secondaryEntityType, Dictionary<string, object> additionalMergeFields )
        {
            RockContext rockContext = this.Context as RockContext;
            HistoryService historyService = new HistoryService( rockContext );

            // change this to adjust the granularity of the GetHistorySummaryByDateTime
            TimeSpan dateSummaryGranularity = TimeSpan.FromDays( 1 );

            if ( primaryEntityType == null )
            {
                return null;
            }

            var entityTypeIdPrimary = primaryEntityType.Id;

            var primaryEntity = historyService.GetEntityQuery( entityTypeIdPrimary ).FirstOrDefault( a => a.Id == entityId );
            var historyQry = historyService.Queryable().Where( a => a.CreatedDateTime.HasValue );

            if ( secondaryEntityType == null )
            {
                // get history records where the primaryentity is the Entity
                historyQry = historyQry.Where( a => a.EntityTypeId == entityTypeIdPrimary && a.EntityId == entityId );
            }
            else
            {
                // get history records where the primaryentity is the Entity OR the primaryEntity is the RelatedEntity and the Entity is the Secondary Entity
                // For example, for GroupHistory, Set PrimaryEntityType to Group and SecondaryEntityType to GroupMember, then get history where the Group is History.Entity or the Group is the RelatedEntity and GroupMember is the EntityType
                var entityTypeIdSecondary = secondaryEntityType.Id;
                historyQry = historyQry.Where( a =>
                    ( a.EntityTypeId == entityTypeIdPrimary && a.EntityId == entityId )
                    || ( a.RelatedEntityTypeId == entityTypeIdPrimary && a.EntityTypeId == entityTypeIdSecondary && a.RelatedEntityId == entityId ) );
            }

            var historySummaryList = historyService.GetHistorySummary( historyQry );
            var historySummaryByDateList = historyService.GetHistorySummaryByDateTime( historySummaryList, dateSummaryGranularity );
            historySummaryByDateList = historySummaryByDateList.OrderByDescending( a => a.SummaryDateTime ).ToList();
            var historySummaryByDateByVerbList = historyService.GetHistorySummaryByDateTimeAndVerb( historySummaryByDateList );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, null, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
            mergeFields.Add( "PrimaryEntity", primaryEntity );
            mergeFields.Add( "PrimaryEntityTypeName", primaryEntityType.FriendlyName );
            if ( secondaryEntityType != null )
            {
                mergeFields.Add( "SecondaryEntityTypeName", secondaryEntityType.FriendlyName );
            }

            mergeFields.Add( "HistorySummaryByDateByVerbList", historySummaryByDateByVerbList );
            if ( additionalMergeFields != null )
            {
                foreach ( var additionalMergeField in additionalMergeFields )
                {
                    mergeFields.AddOrIgnore( additionalMergeField.Key, additionalMergeField.Value );
                }
            }
            string timelineHtml = timelineLavaTemplate.ResolveMergeFields( mergeFields );
            return timelineHtml;
        }

        /// <summary>
        /// Gets the entity query for the specified EntityTypeId
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <returns></returns>
        public IQueryable<IEntity> GetEntityQuery( int entityTypeId )
        {
            EntityTypeCache entityTypeCache = EntityTypeCache.Get( entityTypeId );

            var rockContext = this.Context as RockContext;

            if ( entityTypeCache.AssemblyName != null )
            {
                Type entityType = entityTypeCache.GetEntityType();
                if ( entityType != null )
                {
                    Type[] modelType = { entityType };
                    Type genericServiceType = typeof( Rock.Data.Service<> );
                    Type modelServiceType = genericServiceType.MakeGenericType( modelType );
                    Rock.Data.IService serviceInstance = Activator.CreateInstance( modelServiceType, new object[] { rockContext } ) as IService;

                    MethodInfo qryMethod = serviceInstance.GetType().GetMethod( "Queryable", new Type[] { } );
                    var entityQry = qryMethod.Invoke( serviceInstance, new object[] { } ) as IQueryable<IEntity>;

                    return entityQry;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the history summary by date.
        /// </summary>
        /// <param name="historySummaryList">The history summary list.</param>
        /// <param name="roundingInterval">The rounding interval.</param>
        /// <returns></returns>
        public List<HistorySummaryByDateTime> GetHistorySummaryByDateTime( List<HistorySummary> historySummaryList, TimeSpan roundingInterval )
        {
            IEnumerable<IGrouping<DateTime, HistorySummary>> groupByQry;
            if ( roundingInterval == TimeSpan.FromDays( 1 ) )
            {
                // if rounding by date, just group by the date without Time
                groupByQry = historySummaryList.GroupBy( a => a.CreatedDateTime.Date );
            }
            else
            {
                groupByQry = historySummaryList.GroupBy( a => a.CreatedDateTime.Round( roundingInterval ) );
            }

            var result = groupByQry.Select( a => new HistorySummaryByDateTime
            {
                SummaryDateTime = a.Key,
                HistorySummaryList = a.ToList()
            } ).ToList();

            return result;
        }

        /// <summary>
        /// Gets the history summary by date time and verb.
        /// </summary>
        /// <param name="historySummaryByDateTimeList">The history summary by date time list.</param>
        /// <returns></returns>
        public List<HistorySummaryByDateTimeAndVerb> GetHistorySummaryByDateTimeAndVerb( List<HistorySummaryByDateTime> historySummaryByDateTimeList )
        {
            List<HistorySummaryByDateTimeAndVerb> historySummaryByDateTimeAndVerbList = new List<HistorySummaryByDateTimeAndVerb>();

            foreach ( var historySummaryByDateTime in historySummaryByDateTimeList )
            {
                HistorySummaryByDateTimeAndVerb historySummaryByDateTimeAndVerb = new HistorySummaryByDateTimeAndVerb();
                historySummaryByDateTimeAndVerb.SummaryDateTime = historySummaryByDateTime.SummaryDateTime;
                historySummaryByDateTimeAndVerb.HistorySummaryListByEntityTypeAndVerbList = historySummaryByDateTime.HistorySummaryList.GroupBy( a => new { a.Verb, a.EntityTypeId } ).Select( x => new HistorySummaryListByEntityTypeAndVerb
                {
                    Verb = x.Key.Verb,
                    EntityTypeId = x.Key.EntityTypeId,
                    HistorySummaryList = x.ToList()
                } ).ToList();

                historySummaryByDateTimeAndVerbList.Add( historySummaryByDateTimeAndVerb );
            }

            return historySummaryByDateTimeAndVerbList;
        }

        /// <summary>
        /// Converts a history query grouped into a List of HistorySummary objects
        /// </summary>
        /// <param name="historyQry">The history qry.</param>
        /// <returns></returns>
        public List<HistorySummary> GetHistorySummary( IQueryable<History> historyQry )
        {
            // group the history into into summaries of records that were saved at the same time (for the same Entity, Category, etc)
            var historySummaryQry = historyQry.Where( a => a.CreatedDateTime.HasValue )
                .GroupBy( a => new
                {
                    CreatedDateTime = a.CreatedDateTime.Value,
                    EntityTypeId = a.EntityTypeId,
                    EntityId = a.EntityId,
                    CategoryId = a.CategoryId,
                    RelatedEntityTypeId = a.RelatedEntityTypeId,
                    RelatedEntityId = a.RelatedEntityId,
                    CreatedByPersonAliasId = a.CreatedByPersonAliasId
                } )
                .OrderBy( a => a.Key.CreatedDateTime )
                .Select( x => new HistorySummary
                {
                    CreatedDateTime = x.Key.CreatedDateTime,
                    EntityTypeId = x.Key.EntityTypeId,
                    EntityId = x.Key.EntityId,
                    CategoryId = x.Key.CategoryId,
                    RelatedEntityTypeId = x.Key.RelatedEntityTypeId,
                    RelatedEntityId = x.Key.RelatedEntityId,
                    CreatedByPersonAliasId = x.Key.CreatedByPersonAliasId,
                    HistoryList = x.OrderBy( h => h.Id ).ToList()
                } );

            // load the query into a list
            var historySummaryList = historySummaryQry.ToList();

            PopulateHistorySummaryEntities( historyQry, historySummaryList );

            return historySummaryList;
        }

        /// <summary>
        /// Populates the history summary entities.
        /// </summary>
        /// <param name="historyQry">The history qry.</param>
        /// <param name="historySummaryList">The history summary list.</param>
        private void PopulateHistorySummaryEntities( IQueryable<History> historyQry, List<HistorySummary> historySummaryList )
        {
            // find all the EntityTypes that are used as the History.EntityTypeId records
            var entityTypeIdList = historyQry.Select( a => a.EntityTypeId ).Distinct().ToList();
            foreach ( var entityTypeId in entityTypeIdList )
            {
                // for each entityType, query whatever it is (for example Person) so that we can populate the HistorySummary with that Entity
                var entityLookup = this.GetEntityQuery( entityTypeId ).AsNoTracking()
                    .Where( a => historyQry.Any( h => h.EntityTypeId == entityTypeId && h.EntityId == a.Id ) )
                    .ToList().ToDictionary( k => k.Id, v => v );

                foreach ( var historySummary in historySummaryList.Where( a => a.EntityTypeId == entityTypeId ) )
                {
                    // set the History.Entity to the Entity referenced by History.EntityTypeId/EntityId. If EntityType is Rock.Model.Person, then Entity would be the full Person record where Person.Id = EntityId
                    historySummary.Entity = entityLookup.GetValueOrNull( historySummary.EntityId );
                }
            }

            // find all the EntityTypes that are used as the History.RelatedEntityTypeId records
            var relatedEntityTypeIdList = historyQry.Where( a => a.RelatedEntityTypeId.HasValue ).Select( a => a.RelatedEntityTypeId.Value ).Distinct().ToList();
            foreach ( var relatedEntityTypeId in relatedEntityTypeIdList )
            {
                // for each relatedEntityType, query whatever it is (for example Group) so that we can populate the HistorySummary with that RelatedEntity
                var relatedEntityLookup = this.GetEntityQuery( relatedEntityTypeId ).AsNoTracking()
                    .Where( a => historyQry.Any( h => h.RelatedEntityTypeId == relatedEntityTypeId && h.RelatedEntityId == a.Id ) )
                    .ToList().ToDictionary( k => k.Id, v => v );

                foreach ( var historySummary in historySummaryList.Where( a => a.RelatedEntityTypeId == relatedEntityTypeId && a.RelatedEntityId.HasValue ) )
                {
                    // set the History.RelatedEntity to the Entity referenced by History.RelatedEntityTypeId/RelatedEntityId. If RelatedEntityType is Rock.Model.Group, then RelatedEntity would be the full Group record where Group.Id = RelatedEntityId
                    historySummary.RelatedEntity = relatedEntityLookup.GetValueOrNull( historySummary.RelatedEntityId.Value );
                }
            }
        }

        #endregion

        #region HistorySummary classes

        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="DotLiquid.Drop" />
        public class HistorySummaryByDateTimeAndVerb : DotLiquid.Drop
        {
            /// <summary>
            /// Gets or sets the date time 
            /// </summary>
            /// <value>
            /// The date time.
            /// </value>
            public DateTime SummaryDateTime { get; set; }

            /// <summary>
            /// Gets the date/time of the first history log in this summary's summarylist
            /// </summary>
            /// <value>
            /// The first history date time.
            /// </value>
            public DateTime? FirstHistoryDateTime => this.HistorySummaryListByEntityTypeAndVerbList.FirstOrDefault()?.FirstHistoryDateTime;

            /// <summary>
            /// Gets the date/time of the last history log in this summary's summarylist
            /// </summary>
            /// <value>
            /// The last history date time.
            /// </value>
            public DateTime? LastHistoryDateTime => this.HistorySummaryListByEntityTypeAndVerbList.FirstOrDefault()?.LastHistoryDateTime;

            /// <summary>
            /// Gets or sets the history summary list group by EntityType and Verb
            /// </summary>
            /// <value>
            /// The history summary list by verb list.
            /// </value>
            public List<HistorySummaryListByEntityTypeAndVerb> HistorySummaryListByEntityTypeAndVerbList { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="DotLiquid.Drop" />
        public class HistorySummaryListByEntityTypeAndVerb : DotLiquid.Drop
        {
            /// <summary>
            /// Gets or sets the verb.
            /// </summary>
            /// <value>
            /// The verb.
            /// </value>
            public string Verb { get; set; }

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
            /// Gets the date/time of the first history log in this summary's summarylist
            /// </summary>
            /// <value>
            /// The first history date time.
            /// </value>
            public DateTime? FirstHistoryDateTime => this.HistorySummaryList?.FirstOrDefault()?.CreatedDateTime;

            /// <summary>
            /// Gets the date/time of the first history log in this summary's summarylist
            /// </summary>
            /// <value>
            /// The first history date time.
            /// </value>
            public DateTime? LastHistoryDateTime => this.HistorySummaryList?.LastOrDefault()?.CreatedDateTime;

            /// <summary>
            /// Gets or sets the history summary list.
            /// </summary>
            /// <value>
            /// The history summary list.
            /// </value>
            public List<HistorySummary> HistorySummaryList { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class HistorySummaryByDateTime : DotLiquid.Drop
        {
            /// <summary>
            /// Gets or sets the date time.
            /// </summary>
            /// <value>
            /// The date time.
            /// </value>
            public DateTime SummaryDateTime { get; set; }

            /// <summary>
            /// Gets or sets the history summary list.
            /// </summary>
            /// <value>
            /// The history summary list.
            /// </value>
            public List<HistorySummary> HistorySummaryList { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="DotLiquid.Drop" />
        public class HistorySummary : DotLiquid.Drop
        {
            /// <summary>
            /// Gets or sets the created date time.
            /// </summary>
            /// <value>
            /// The created date time.
            /// </value>
            public DateTime CreatedDateTime { get; set; }

            /// <summary>
            /// Gets the first history record.
            /// </summary>
            /// <value>
            /// The first history record.
            /// </value>
            private History FirstHistoryRecord
            {
                get
                {
                    if ( _firstHistoryRecord == null )
                    {
                        _firstHistoryRecord = this.HistoryList?.FirstOrDefault();
                    }

                    return _firstHistoryRecord;
                }
            }

            private History _firstHistoryRecord = null;

            /// <summary>
            /// Gets the Id of the First History Record (the record that the others are summarized under)
            /// </summary>
            /// <value>
            /// The first history identifier.
            /// </value>
            public int FirstHistoryId => this.FirstHistoryRecord?.Id ?? 0;

            /// <summary>
            /// Gets or sets the Summary verb (the Verb of the first history record in this summary)
            /// </summary>
            /// <value>
            /// The verb.
            /// </value>
            public string Verb => this.FirstHistoryRecord?.Verb;

            /// <summary>
            /// Gets the caption.
            /// </summary>
            /// <value>
            /// The caption.
            /// </value>
            public string Caption => this.FirstHistoryRecord?.Caption;

            /// <summary>
            /// Gets the name of the value.
            /// </summary>
            /// <value>
            /// The name of the value.
            /// </value>
            public string ValueName => this.FirstHistoryRecord?.ValueName;

            /// <summary>
            /// Gets the related data.
            /// </summary>
            /// <value>
            /// The related data.
            /// </value>
            public string RelatedData => this.FirstHistoryRecord?.RelatedData;

            /// <summary>
            /// Gets the created by person identifier.
            /// </summary>
            /// <value>
            /// The created by person identifier.
            /// </value>
            public int? CreatedByPersonId => this.FirstHistoryRecord?.CreatedByPersonId;

            /// <summary>
            /// Gets or sets the created by person.
            /// </summary>
            /// <value>
            /// The created by person.
            /// </value>
            public Person CreatedByPerson => this.FirstHistoryRecord?.CreatedByPersonAlias.Person;

            /// <summary>
            /// Gets the name of the created by person.
            /// </summary>
            /// <value>
            /// The name of the created by person.
            /// </value>
            public string CreatedByPersonName => this.FirstHistoryRecord?.CreatedByPersonName;

            /// <summary>
            /// Gets the created by person alias identifier.
            /// </summary>
            /// <value>
            /// The created by person alias identifier.
            /// </value>
            public int? CreatedByPersonAliasId { get; internal set; }

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
            /// Gets or sets the entity.
            /// </summary>
            /// <value>
            /// The entity.
            /// </value>
            public IEntity Entity { get; set; }

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
            public CategoryCache Category
            {
                get
                {
                    return CategoryCache.Get( this.CategoryId );
                }
            }

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
            /// Gets or sets the related entity.
            /// </summary>
            /// <value>
            /// The related entity.
            /// </value>
            public IEntity RelatedEntity { get; set; }



            /// <summary>
            /// Gets the formatted caption.
            /// </summary>
            /// <value>
            /// The formatted caption.
            /// </value>
            public string FormattedCaption
            {
                get
                {
                    var category = this.Category;
                    var caption = this.Caption;
                    if ( category != null )
                    {
                        string urlMask = category.GetAttributeValue( "UrlMask" );
                        string virtualUrl = string.Empty;
                        if ( !string.IsNullOrWhiteSpace( urlMask ) )
                        {
                            if ( urlMask.Contains( "{0}" ) )
                            {
                                string p1 = this.RelatedEntityId.HasValue ? this.RelatedEntityId.Value.ToString() : "";
                                string p2 = this.EntityId.ToString();
                                virtualUrl = string.Format( urlMask, p1, p2 );
                            }

                            string resolvedUrl;

                            if ( System.Web.HttpContext.Current == null )
                            {
                                resolvedUrl = virtualUrl;
                            }
                            else
                            {
                                resolvedUrl = System.Web.VirtualPathUtility.ToAbsolute( virtualUrl );
                            }

                            return string.Format( "<a href='{0}'>{1}</a>", resolvedUrl, caption );
                        }
                    }

                    return caption;
                }
            }

            /// <summary>
            /// Gets or sets the history list.
            /// </summary>
            /// <value>
            /// The history list.
            /// </value>
            public List<History> HistoryList { get; set; }
        }

        #endregion

        /// <summary>
        /// Adds the changes.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="categoryGuid">The category unique identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="changes">The changes.</param>
        /// <param name="modifiedByPersonAliasId">The modified by person alias identifier.</param>
        [RockObsolete( "1.8" )]
        [Obsolete( History.HISTORY_METHOD_OBSOLETE_MESSAGE )]
        public static void AddChanges( RockContext rockContext, Type modelType, Guid categoryGuid, int entityId, List<string> changes, int? modifiedByPersonAliasId = null )
        {
            AddChanges( rockContext, modelType, categoryGuid, entityId, changes, null, null, null, modifiedByPersonAliasId );
        }

        /// <summary>
        /// Adds the changes.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="categoryGuid">The category unique identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="changes">The changes.</param>
        /// <param name="modifiedByPersonAliasId">The modified by person alias identifier.</param>
        public static void AddChanges( RockContext rockContext, Type modelType, Guid categoryGuid, int entityId, History.HistoryChangeList changes, int? modifiedByPersonAliasId = null )
        {
            AddChanges( rockContext, modelType, categoryGuid, entityId, changes, null, null, null, modifiedByPersonAliasId );
        }

        /// <summary>
        /// Adds the changes.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="categoryGuid">The category unique identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="changes">The changes.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="relatedModelType">Type of the related model.</param>
        /// <param name="relatedEntityId">The related entity identifier.</param>
        /// <param name="modifiedByPersonAliasId">The modified by person alias identifier.</param>
        [RockObsolete( "1.8" )]
        [Obsolete( History.HISTORY_METHOD_OBSOLETE_MESSAGE )]
        public static void AddChanges( RockContext rockContext, Type modelType, Guid categoryGuid, int entityId, List<string> changes, string caption, Type relatedModelType, int? relatedEntityId, int? modifiedByPersonAliasId = null )
        {
            var historyChanges = new History.HistoryChangeList();
            historyChanges.AddRange( changes.Select( a => new History.HistoryChange( a ) ).ToList() );

            AddChanges( rockContext, modelType, categoryGuid, entityId, historyChanges, caption, relatedModelType, relatedEntityId, modifiedByPersonAliasId );
        }

        /// <summary>
        /// Adds the changes.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="categoryGuid">The category unique identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="changes">The changes.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="relatedModelType">Type of the related model.</param>
        /// <param name="relatedEntityId">The related entity identifier.</param>
        /// <param name="modifiedByPersonAliasId">The modified by person alias identifier.</param>
        public static void AddChanges( RockContext rockContext, Type modelType, Guid categoryGuid, int entityId, History.HistoryChangeList changes, string caption, Type relatedModelType, int? relatedEntityId, int? modifiedByPersonAliasId = null )
        {
            var entityType = EntityTypeCache.Get( modelType );
            var category = CategoryCache.Get( categoryGuid );
            var creationDate = RockDateTime.Now;

            int? relatedEntityTypeId = null;
            if ( relatedModelType != null )
            {
                var relatedEntityType = EntityTypeCache.Get( relatedModelType );
                if ( relatedModelType != null )
                {
                    relatedEntityTypeId = relatedEntityType.Id;
                }
            }

            if ( entityType != null && category != null )
            {
                var historyService = new HistoryService( rockContext );

                foreach ( var historyChange in changes.Where( m => m != null ) )
                {
                    var history = new History();
                    history.EntityTypeId = entityType.Id;
                    history.CategoryId = category.Id;
                    history.EntityId = entityId;
                    history.Caption = caption.Truncate( 200 );
                    history.RelatedEntityTypeId = relatedEntityTypeId;
                    history.RelatedEntityId = relatedEntityId;

                    historyChange.CopyToHistory( history );

                    if ( modifiedByPersonAliasId.HasValue )
                    {
                        history.CreatedByPersonAliasId = modifiedByPersonAliasId;
                    }

                    // Manually set creation date on these history items so that they will be grouped together
                    history.CreatedDateTime = creationDate;

                    historyService.Add( history );
                }
            }
        }

        /// <summary>
        /// Saves a list of history messages.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="categoryGuid">The category unique identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="changes">The changes.</param>
        /// <param name="commitSave">if set to <c>true</c> [commit save].</param>
        /// <param name="modifiedByPersonAliasId">The modified by person alias identifier.</param>
        [RockObsolete( "1.8" )]
        [Obsolete( History.HISTORY_METHOD_OBSOLETE_MESSAGE )]
        public static void SaveChanges( RockContext rockContext, Type modelType, Guid categoryGuid, int entityId, List<string> changes, bool commitSave = true, int? modifiedByPersonAliasId = null )
        {
            SaveChanges( rockContext, modelType, categoryGuid, entityId, changes, null, null, null, commitSave, modifiedByPersonAliasId );
        }

        /// <summary>
        /// Saves a list of history messages.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="categoryGuid">The category unique identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="changes">The changes.</param>
        /// <param name="commitSave">if set to <c>true</c> [commit save].</param>
        /// <param name="modifiedByPersonAliasId">The modified by person alias identifier.</param>
        /// <param name="sourceOfChange">The source of change.</param>
        public static void SaveChanges( RockContext rockContext, Type modelType, Guid categoryGuid, int entityId, History.HistoryChangeList changes, bool commitSave = true, int? modifiedByPersonAliasId = null, string sourceOfChange = null )
        {
            SaveChanges( rockContext, modelType, categoryGuid, entityId, changes, null, null, null, commitSave, modifiedByPersonAliasId, sourceOfChange );
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="categoryGuid">The category unique identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="changes">The changes.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="relatedModelType">Type of the related model.</param>
        /// <param name="relatedEntityId">The related entity identifier.</param>
        /// <param name="commitSave">if set to <c>true</c> [commit save].</param>
        /// <param name="modifiedByPersonAliasId">The modified by person alias identifier.</param>
        [RockObsolete( "1.8" )]
        [Obsolete( History.HISTORY_METHOD_OBSOLETE_MESSAGE )]
        public static void SaveChanges( RockContext rockContext, Type modelType, Guid categoryGuid, int entityId, List<string> changes, string caption, Type relatedModelType, int? relatedEntityId, bool commitSave = true, int? modifiedByPersonAliasId = null )
        {
            if ( changes.Any() )
            {
                AddChanges( rockContext, modelType, categoryGuid, entityId, changes, caption, relatedModelType, relatedEntityId, modifiedByPersonAliasId );
                if ( commitSave )
                {
                    rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="categoryGuid">The category unique identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="changes">The changes.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="relatedModelType">Type of the related model.</param>
        /// <param name="relatedEntityId">The related entity identifier.</param>
        /// <param name="commitSave">if set to <c>true</c> [commit save].</param>
        /// <param name="modifiedByPersonAliasId">The modified by person alias identifier.</param>
        /// <param name="sourceOfChange">The source of change to be recorded on the history record. If this is not provided the RockContext source of change will be used instead.</param>
        public static void SaveChanges( RockContext rockContext, Type modelType, Guid categoryGuid, int entityId, History.HistoryChangeList changes, string caption, Type relatedModelType, int? relatedEntityId, bool commitSave = true, int? modifiedByPersonAliasId = null, string sourceOfChange = null )
        {
            if ( changes.Any() )
            {
                changes.ForEach( a => a.SourceOfChange = sourceOfChange ?? rockContext.SourceOfChange );
                AddChanges( rockContext, modelType, categoryGuid, entityId, changes, caption, relatedModelType, relatedEntityId, modifiedByPersonAliasId );
                if ( commitSave )
                {
                    rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Deletes any saved history items.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="modelType">Type of the model.</param>
        /// <param name="entityId">The entity identifier.</param>
        public static void DeleteChanges( RockContext rockContext, Type modelType, int entityId )
        {
            var entityType = EntityTypeCache.Get( modelType );
            if ( entityType != null )
            {
                var historyService = new HistoryService( rockContext );
                foreach ( var history in historyService.Queryable()
                    .Where( h =>
                        h.EntityTypeId == entityType.Id &&
                        h.EntityId == entityId ) )
                {
                    historyService.Delete( history );
                }

                rockContext.SaveChanges();
            }
        }
    }
}