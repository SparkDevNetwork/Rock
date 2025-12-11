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
using Rock.Enums.Controls;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.NoteWatchList;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Core.Grid;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of note watches.
    /// </summary>

    [DisplayName( "Note Watch List" )]
    [Category( "Core" )]
    [Description( "Displays a list of note watches." )]
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the note watch details.",
        Key = AttributeKey.DetailPage )]

    [EntityTypeField( "Entity Type",
        Description = "Set an Entity Type to limit this block to Note Types and Entities for a specific entity type.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.EntityType )]

    [NoteTypeField( "Note Type",
        Description = "Set Note Type to limit this block to a specific note type",
        AllowMultiple = false,
        IsRequired = false,
        Order = 1,
        Key = AttributeKey.NoteType )]

    [Rock.SystemGuid.EntityTypeGuid( "8fdb4340-bdde-4797-b173-ea456a825b2a" )]
    [Rock.SystemGuid.BlockTypeGuid( "ed4cd6ae-ed86-4607-a252-f15971e4f2e3" )]
    [CustomizedGrid]
    public class NoteWatchList : RockListBlockType<NoteWatchList.NoteWatchRow>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string EntityType = "EntityType";
            public const string NoteType = "NoteType";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PreferenceKey
        {
            public const string FilterCreatedDateRange = "filter-created-date-range";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// The NoteWatch attributes that are configured to show on the grid.
        /// </summary>
        private readonly Lazy<List<AttributeCache>> _gridAttributes = new Lazy<List<AttributeCache>>( BuildGridAttributes );

        private PersonPreferenceCollection _personPreferences;

        #endregion

        #region Properties

        public PersonPreferenceCollection PersonPreferences
        {
            get
            {
                if ( _personPreferences == null )
                {
                    _personPreferences = this.GetBlockPersonPreferences();
                }

                return _personPreferences;
            }
        }

        private SlidingDateRangeBag FilterCreatedDateRange => PersonPreferences
            .GetValue( PreferenceKey.FilterCreatedDateRange )
            .ToSlidingDateRangeBagOrNull();

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<NoteWatchListOptionsBag>();
            var builder = GetGridBuilder();

            if ( FilterCreatedDateRange == null )
            {
                var defaultSlidingDateRange = new SlidingDateRangeBag
                {
                    RangeType = SlidingDateRangeType.Last,
                    TimeUnit = TimeUnitType.Month,
                    TimeValue = 6
                };

                this.PersonPreferences.SetValue( PreferenceKey.FilterCreatedDateRange, defaultSlidingDateRange.ToDelimitedSlidingDateRangeOrNull() );
                this.PersonPreferences.Save();
            }

            box.IsAddEnabled = GetIsAddEnabled();
            box.IsDeleteEnabled = true;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private NoteWatchListOptionsBag GetBoxOptions()
        {
            var options = new NoteWatchListOptionsBag();
            options.EntityTypeGuid = GetAttributeValue( AttributeKey.EntityType ).AsGuidOrNull();
            options.NoteTypeGuid = GetAttributeValue( AttributeKey.NoteType ).AsGuidOrNull();
            return options;
        }

        /// <summary>
        /// Determines if the add button should be enabled in the grid.
        /// <summary>
        /// <returns>A boolean value that indicates if the add button should be enabled.</returns>
        private bool GetIsAddEnabled()
        {
            var entity = new NoteWatch();

            return entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, new Dictionary<string, string> { ["NoteWatchId"] = "((Key))", ["autoEdit"] = "true", ["returnUrl"] = this.GetCurrentPageUrl() } )
            };
        }

        private string FormatEntityType( int? entityTypeId, string entityTypeName, int? entityId )
        {
            if ( entityId.HasValue && entityTypeId.HasValue )
            {
                var rockContext = new RockContext();
                var entity = new EntityTypeService( rockContext ).GetEntity( entityTypeId.Value, entityId.Value );
                if ( entity != null )
                {
                    var entityName = entity.ToString();
                    return $"{entityTypeName} ({entityName})";
                }
            }

            return entityTypeName;
        }

        /// <inheritdoc/>
        protected override IQueryable<NoteWatchRow> GetListQueryable( RockContext rockContext )
        {
            var qry = new NoteWatchService( rockContext ).Queryable().AsNoTracking();

            qry = FilterByCreatedDate( qry );

            Guid? blockEntityTypeGuid = GetAttributeValue( AttributeKey.EntityType ).AsGuidOrNull();
            Guid? blockNoteTypeGuid = GetAttributeValue( AttributeKey.NoteType ).AsGuidOrNull();

            if ( blockNoteTypeGuid.HasValue )
            {
                var noteType = NoteTypeCache.Get( blockNoteTypeGuid.Value );
                if ( noteType != null )
                {
                    int noteTypeId = noteType.Id;
                    qry = qry.Where( a => a.NoteTypeId.HasValue && a.NoteTypeId == noteTypeId );
                }
            }
            else if ( blockEntityTypeGuid.HasValue )
            {
                var entityType = EntityTypeCache.Get( blockEntityTypeGuid.Value );
                if ( entityType != null )
                {
                    int entityTypeId = entityType.Id;
                    qry = qry.Where( a =>
                        ( a.EntityTypeId.HasValue && a.EntityTypeId.Value == entityTypeId ) ||
                        ( a.NoteTypeId.HasValue && a.NoteType.EntityTypeId == entityTypeId ) );
                }
            }

            var contextEntity = GetContextEntity();
            if ( contextEntity is Person contextPerson )
            {
                qry = qry.Where( a => a.WatcherPersonAliasId.HasValue && a.WatcherPersonAlias.PersonId == contextPerson.Id );
            }
            else if ( contextEntity is Model.Group contextGroup )
            {
                qry = qry.Where( a => a.WatcherGroupId.HasValue && a.WatcherGroupId == contextGroup.Id );
            }

            // Add null check for the data source
            if ( qry != null )
            {
                return qry.Select( n => new NoteWatchRow
                {
                    NoteWatch = n,
                    NoteTypeName = n.NoteType != null ? n.NoteType.Name : string.Empty,
                    EntityTypeName = n.EntityType != null ? n.EntityType.FriendlyName : "Unknown",
                    WatcherGroupName = n.WatcherGroup != null ? n.WatcherGroup.Name : string.Empty,
                    PersonProjection = new PersonProjection
                    {
                        NickName = n.WatcherPersonAlias.Person.NickName,
                        LastName = n.WatcherPersonAlias.Person.LastName,
                        PhotoId = n.WatcherPersonAlias.Person.PhotoId,
                        Age = n.WatcherPersonAlias.Person.Age,
                        Gender = n.WatcherPersonAlias.Person.Gender,
                        RecordTypeValueId = n.WatcherPersonAlias.Person.RecordTypeValueId,
                        AgeClassification = n.WatcherPersonAlias.Person.AgeClassification,
                        ConnectionStatusValueId = n.WatcherPersonAlias.Person.ConnectionStatusValueId,
                        Id = n.WatcherPersonAlias.Person.Id,
                    }
                } );
            }
            else
            {
                return Enumerable.Empty<NoteWatchRow>().AsQueryable();
            }
        }

        protected override List<NoteWatchRow> GetListItems( IQueryable<NoteWatchRow> queryable, RockContext rockContext )
        {
            var noteWatches = queryable.ToList();

            // Load attribute values for the grid-selected attributes.
            GridAttributeLoader.LoadFor( noteWatches, a => a.NoteWatch, _gridAttributes.Value, rockContext );

            foreach ( var noteWatch in noteWatches )
            {
                if ( noteWatch.PersonProjection.Id.HasValue )
                {
                    noteWatch.Person = new PersonFieldBag
                    {
                        IdKey = IdHasher.Instance.GetHash( noteWatch.PersonProjection.Id.Value ),
                        NickName = noteWatch.PersonProjection.NickName,
                        LastName = noteWatch.PersonProjection.LastName,
                    };

                    var initials = $"{noteWatch.Person.NickName.Truncate( 1, false )}{noteWatch.Person.LastName.Truncate( 1, false )}";
                    noteWatch.Person.PhotoUrl = Rock.Model.Person.GetPersonPhotoUrl(
                        initials,
                        noteWatch.PersonProjection.PhotoId,
                        noteWatch.PersonProjection.Age,
                        noteWatch.PersonProjection.Gender ?? Gender.Unknown,
                        noteWatch.PersonProjection.RecordTypeValueId,
                        noteWatch.PersonProjection.AgeClassification
                    );

                    if ( noteWatch.PersonProjection.ConnectionStatusValueId.HasValue )
                    {
                        var connectionStatusValue = DefinedValueCache.Get( noteWatch.PersonProjection.ConnectionStatusValueId.Value );
                        if ( connectionStatusValue != null )
                        {
                            noteWatch.Person.ConnectionStatus = connectionStatusValue.Value;
                        }
                    }
                }
            }

            return noteWatches;
        }

        /// <inheritdoc/>
        protected override GridBuilder<NoteWatchRow> GetGridBuilder()
        {
            var blockOptions = new GridBuilderGridOptions<NoteWatchRow>
            {
                LavaObject = row => row.NoteWatch
            };

            return new GridBuilder<NoteWatchRow>()
                .WithBlock( this, blockOptions )
                .AddField( "id", a => a.NoteWatch.Id )
                .AddTextField( "idKey", a => a.NoteWatch.IdKey )
                .AddField( "isWatching", a => a.NoteWatch.IsWatching )
                .AddField( "watcher", a => a.Person )
                .AddTextField( "watcherGroup", a => a.WatcherGroupName )
                .AddTextField( "noteType", a => a.NoteTypeName )
                .AddTextField("entityType", a => FormatEntityType( a.NoteWatch.EntityTypeId, a.EntityTypeName, a.NoteWatch.EntityId ))
                .AddField( "allowOverride", a => a.NoteWatch.AllowOverride )
                .AddAttributeFieldsFrom( a => a.NoteWatch, _gridAttributes.Value );
        }

        /// <summary>
        /// Builds the list of grid attributes that should be included on the Grid.
        /// </summary>
        /// <remarks>
        /// The default implementation returns only attributes that are not qualified.
        /// </remarks>
        /// <returns>A list of <see cref="AttributeCache"/> objects.</returns>
        private static List<AttributeCache> BuildGridAttributes()
        {
            var entityTypeId = EntityTypeCache.Get<NoteWatch>( false )?.Id;
            if ( entityTypeId.HasValue )
            {
                return AttributeCache.GetOrderedGridAttributes( entityTypeId.Value, string.Empty, string.Empty );
            }
            return new List<AttributeCache>();
        }

        /// <summary>
        /// Filters the queryable by the Created Date
        /// </summary>
        /// <param name="queryable">The <see cref="NoteWatch"/> queryable</param>
        /// <returns></returns>
        private IQueryable<NoteWatch> FilterByCreatedDate( IQueryable<NoteWatch> queryable )
        {
            // Default to the last 180 days if a null/invalid range was selected.
            var defaultSlidingDateRange = new SlidingDateRangeBag
            {
                RangeType = SlidingDateRangeType.Last,
                TimeUnit = TimeUnitType.Month,
                TimeValue = 6
            };

            var dateRange = FilterCreatedDateRange.Validate( defaultSlidingDateRange ).ActualDateRange;
            var dateTimeStart = dateRange.Start;
            var dateTimeEnd = dateRange.End;

            queryable = queryable
                .Where( c =>
                    c.CreatedDateTime >= dateTimeStart &&
                    c.CreatedDateTime <= dateTimeEnd );

            return queryable;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new NoteWatchService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{NoteWatch.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete {NoteWatch.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion

        #region Helper Classes

        public class NoteWatchRow
        {
            public NoteWatch NoteWatch { get; set; }

            public string NoteTypeName { get; set; }

            public string EntityTypeName { get; set; }

            public string WatcherGroupName { get; set; }

            public PersonProjection PersonProjection { get; set; }

            public PersonFieldBag Person { get; set; }
        }

        public class PersonProjection
        {
            public string NickName { get; set; }

            public string LastName { get; set; }

            public int? PhotoId { get; set; }

            public int? Age { get; set; }

            public Gender? Gender { get; set; }

            public int? RecordTypeValueId { get; set; }

            public AgeClassification? AgeClassification { get; set; }

            public int? ConnectionStatusValueId { get; set; }

            public int? Id { get; set; }
        }

        #endregion
    }
}
