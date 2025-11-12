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
using Rock.ViewModels.Blocks.Group.GroupArchivedList;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Core.Grid;
using Rock.Web.Cache;

namespace Rock.Blocks.Group
{
    /// <summary>
    /// Displays a list of groups.
    /// </summary>
    [DisplayName( "Group Archived List" )]
    [Category( "Utility" )]
    [Description( "Lists Groups that have been archived." )]
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "b67a0c89-1550-4960-8aaf-baa713be3277" )]
    [Rock.SystemGuid.BlockTypeGuid( "972ad143-8294-4462-b2a7-1b36ea127374" )]
    [CustomizedGrid]
    public class GroupArchivedList : RockListBlockType<GroupArchivedList.ArchivedGroupRow>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PreferenceKey
        {
            public const string FilterArchivedDateRange = "filter-archived-date-range";
        }

        #endregion Keys

        #region Fields

        private PersonPreferenceCollection _personPreferences;

        /// <summary>
        /// The Group attributes configured to show on the grid.
        /// </summary>
        private readonly Lazy<List<AttributeCache>> _gridAttributes = new System.Lazy<List<AttributeCache>>( BuildGridAttributes );

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

        private SlidingDateRangeBag FilterArchivedDateRange => PersonPreferences
            .GetValue( PreferenceKey.FilterArchivedDateRange )
            .ToSlidingDateRangeBagOrNull();

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<GroupArchivedListOptionsBag>();
            var builder = GetGridBuilder();

            if ( FilterArchivedDateRange == null )
            {
                var defaultSlidingDateRange = new SlidingDateRangeBag
                {
                    RangeType = SlidingDateRangeType.Last,
                    TimeUnit = TimeUnitType.Month,
                    TimeValue = 6
                };

                this.PersonPreferences.SetValue( PreferenceKey.FilterArchivedDateRange, defaultSlidingDateRange.ToDelimitedSlidingDateRangeOrNull() );
                this.PersonPreferences.Save();
            }

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = false;
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
        private GroupArchivedListOptionsBag GetBoxOptions()
        {
            var options = new GroupArchivedListOptionsBag();

            return options;
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "GroupId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<ArchivedGroupRow> GetListQueryable( RockContext rockContext )
        {
            var queryable = new GroupService( rockContext ).GetArchived()
                .AsNoTracking()
                .Select( a => new ArchivedGroupRow
                {
                    Group = a,
                    GroupTypeName = a.GroupType != null ? a.GroupType.Name : string.Empty,
                    PersonProjection = new PersonProjection
                    {
                        NickName = a.ArchivedByPersonAlias.Person.NickName,
                        LastName = a.ArchivedByPersonAlias.Person.LastName,
                        PhotoId = a.ArchivedByPersonAlias.Person.PhotoId,
                        Age = a.ArchivedByPersonAlias.Person.Age,
                        Gender = a.ArchivedByPersonAlias.Person.Gender,
                        RecordTypeValueId = a.ArchivedByPersonAlias.Person.RecordTypeValueId,
                        AgeClassification = a.ArchivedByPersonAlias.Person.AgeClassification,
                        ConnectionStatusValueId = a.ArchivedByPersonAlias.Person.ConnectionStatusValueId,
                        Id = a.ArchivedByPersonAlias.Person.Id,
                    }
                } );

            queryable = FilterByArchivedDate( queryable );

            return queryable;
        }

        protected override IQueryable<ArchivedGroupRow> GetOrderedListQueryable( IQueryable<ArchivedGroupRow> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( g => g.Group.ArchivedDateTime );
        }

        protected override List<ArchivedGroupRow> GetListItems( IQueryable<ArchivedGroupRow> queryable, RockContext rockContext )
        {
            var archivedGroups = queryable.ToList();

            foreach ( var archivedGroup in archivedGroups )
            {
                if ( archivedGroup.PersonProjection.Id.HasValue )
                {
                    archivedGroup.Person = new PersonFieldBag
                    {
                        IdKey = IdHasher.Instance.GetHash( archivedGroup.PersonProjection.Id.Value ),
                        NickName = archivedGroup.PersonProjection.NickName,
                        LastName = archivedGroup.PersonProjection.LastName
                    };

                    var initials = $"{archivedGroup.Person.NickName.Truncate( 1, false )}{archivedGroup.Person.LastName.Truncate( 1, false )}";
                    archivedGroup.Person.PhotoUrl = Rock.Model.Person.GetPersonPhotoUrl(
                        initials,
                        archivedGroup.PersonProjection.PhotoId,
                        archivedGroup.PersonProjection.Age,
                        archivedGroup.PersonProjection.Gender ?? Gender.Unknown,
                        archivedGroup.PersonProjection.RecordTypeValueId,
                        archivedGroup.PersonProjection.AgeClassification
                    );

                    if ( archivedGroup.PersonProjection.ConnectionStatusValueId.HasValue )
                    {
                        var connectionStatusValue = DefinedValueCache.Get( archivedGroup.PersonProjection.ConnectionStatusValueId.Value );
                        if ( connectionStatusValue != null )
                        {
                            archivedGroup.Person.ConnectionStatus = connectionStatusValue.Value;
                        }
                    }
                }
            }

            // Load attribute values for the grid-selected attributes on Group
            GridAttributeLoader.LoadFor( archivedGroups, g => g.Group, _gridAttributes.Value, rockContext );

            return archivedGroups;
        }

        /// <inheritdoc/>
        protected override GridBuilder<ArchivedGroupRow> GetGridBuilder()
        {
            return new GridBuilder<ArchivedGroupRow>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.Group.IdKey )
                .AddTextField( "groupType", a => a.GroupTypeName )
                .AddTextField( "name", a => a.Group.Name )
                .AddTextField( "description", a => a.Group.Description )
                .AddDateTimeField( "createdDate", a => a.Group.CreatedDateTime?.Date )
                .AddDateTimeField( "archivedDate", a => a.Group.ArchivedDateTime?.Date )
                .AddField( "archivedBy", a => a.Person )
                .AddField( "isSystem", a => a.Group.IsSystem )
                .AddAttributeFieldsFrom( a => a.Group, _gridAttributes.Value );
        }

        /// <summary>
        /// Builds the list of grid attributes that should be included on the Grid.
        /// </summary>
        /// <returns>A list of <see cref="AttributeCache"/> objects.</returns>
        private static List<AttributeCache> BuildGridAttributes()
        {
            var entityTypeId = EntityTypeCache.Get<Rock.Model.Group>( false )?.Id;

            if ( entityTypeId.HasValue )
            {
                return AttributeCache.GetOrderedGridAttributes( entityTypeId.Value, string.Empty, string.Empty );
            }

            return new List<AttributeCache>();
        }

        /// <summary>
        /// Filters the queryable by the Archived Date
        /// </summary>
        /// <param name="queryable">The <see cref="ArchivedGroupRow"/> queryable</param>
        /// <returns></returns>
        private IQueryable<ArchivedGroupRow> FilterByArchivedDate( IQueryable<ArchivedGroupRow> queryable )
        {
            // Default to the last 180 days if a null/invalid range was selected.
            var defaultSlidingDateRange = new SlidingDateRangeBag
            {
                RangeType = SlidingDateRangeType.Last,
                TimeUnit = TimeUnitType.Month,
                TimeValue = 6
            };

            var dateRange = FilterArchivedDateRange.Validate( defaultSlidingDateRange ).ActualDateRange;
            var dateTimeStart = dateRange.Start;
            var dateTimeEnd = dateRange.End;

            queryable = queryable
                .Where( g =>
                    g.Group.ArchivedDateTime >= dateTimeStart &&
                    g.Group.ArchivedDateTime <= dateTimeEnd );

            return queryable;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Unarchives the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be unarchived.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult UndoArchive( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new GroupService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{Rock.Model.Group.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to edit ${Rock.Model.Group.FriendlyTypeName}." );
                }

                entity.IsArchived = false;
                entity.ArchivedByPersonAliasId = null;
                entity.ArchivedDateTime = null;

                rockContext.SaveChanges();

                return ActionOk();
            }
        }

        #endregion

        #region Helper Classes

        public class ArchivedGroupRow
        {
            public Rock.Model.Group Group { get; set; }

            public string GroupTypeName { get; set; }

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
