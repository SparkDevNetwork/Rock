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
using Rock.Security;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Connection.CelebrationsReport;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Connection
{
    /// <summary>
    /// Displays a list of connection celebrations.
    /// </summary>

    [DisplayName( "Connection Celebrations Report" )]
    [Category( "Connection" )]
    [Description( "Displays a list of connection celebrations." )]
    [IconCssClass( "ti ti-trophy" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Connection Request Detail Page",
        Description = "The page that will show the connection request details.",
        Key = AttributeKey.ConnectionRequestDetailPage,
        IsRequired = false )]

    [CustomizedGrid]

    [Rock.SystemGuid.EntityTypeGuid( "B5C0F4D7-2A1E-4C8B-9F3D-6E0A7B2C5D9F" )]
    [Rock.SystemGuid.BlockTypeGuid( "8D3E5A9C-7B2F-4E1D-A6C0-3F9B8D2E5A7C" )]
    public class CelebrationsReport : RockEntityListBlockType<Note>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string ConnectionRequestDetailPage = "ConnectionRequestDetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string ConnectionRequestDetailPage = "ConnectionRequestDetailPage";
        }

        private static class PreferenceKey
        {
            public const string FilterDateRange = "filter-date-range";
            public const string FilterConnectionTypeGuid = "filter-connection-type-guid";
        }

        private static class PageParameterKey
        {
            public const string ConnectionTypeId = "ConnectionTypeId";
        }

        #endregion Keys

        #region Fields

        /// <summary>
        /// Lookup of ConnectionRequest by Id, populated in <see cref="GetListItems"/> and
        /// consumed by <see cref="GetGridBuilder"/> lambdas to avoid N+1 queries.
        /// </summary>
        private Dictionary<int, ConnectionRequest> _connectionRequestMap;

        /// <summary>
        /// Default date range applied when no preference is saved: last 8 months.
        /// </summary>
        private static readonly SlidingDateRangeBag DefaultDateRange = new SlidingDateRangeBag
        {
            RangeType = SlidingDateRangeType.Last,
            TimeUnit = TimeUnitType.Month,
            TimeValue = 8
        };

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the sliding date range used to filter results by connection request created date.
        /// Falls back to <see cref="DefaultDateRange"/> when no preference is stored.
        /// </summary>
        private SlidingDateRangeBag FilterDateRange => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterDateRange )
            .ToSlidingDateRangeBagOrNull() ?? DefaultDateRange;

        /// <summary>
        /// Gets the connection type Guid to filter results by.
        /// The ConnectionTypeId page parameter (IdKey) takes precedence over the saved person preference.
        /// </summary>
        private Guid? FilterConnectionTypeGuid
        {
            get
            {
                var pageParamId = Rock.Utility.IdHasher.Instance.GetId( PageParameter( PageParameterKey.ConnectionTypeId ) );

                if ( pageParamId.HasValue )
                {
                    return ConnectionTypeCache.Get( pageParamId.Value )?.Guid;
                }

                return GetBlockPersonPreferences()
                    .GetValue( PreferenceKey.FilterConnectionTypeGuid )
                    .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();
            }
        }

        #endregion Properties

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<CelebrationsReportOptionsBag>();
            var builder = GetGridBuilder();

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
        private CelebrationsReportOptionsBag GetBoxOptions()
        {
            var connectionTypes = ConnectionTypeCache.All()
                .Where( ct => ct.IsActive && ct.EnabledFeatures.HasFlag( Rock.Enums.Connection.EnabledFeatureFlags.Celebration ) )
                .OrderBy( ct => ct.Name )
                .ToList();

            var filterGuid = FilterConnectionTypeGuid;
            var initialConnectionType = filterGuid.HasValue
                ? connectionTypes
                    .Where( ct => ct.Guid == filterGuid.Value )
                    .Select( ct => new ListItemBag { Text = ct.Name, Value = ct.Guid.ToString() } )
                    .FirstOrDefault()
                : null;

            // Populate the locked title name only when the connection type came from a page parameter
            // (not a saved preference) so the UI can hide the filter and update the panel title.
            string pageConnectionTypeName = null;
            if ( PageParameter( PageParameterKey.ConnectionTypeId ).IsNotNullOrWhiteSpace() && filterGuid.HasValue )
            {
                pageConnectionTypeName = connectionTypes.FirstOrDefault( ct => ct.Guid == filterGuid.Value )?.Name;
            }

            return new CelebrationsReportOptionsBag
            {
                ConnectionTypes = connectionTypes
                    .Select( ct => new ListItemBag { Text = ct.Name, Value = ct.Guid.ToString() } )
                    .ToList(),
                InitialConnectionType = initialConnectionType,
                PageConnectionTypeName = pageConnectionTypeName
            };
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ConnectionRequestDetailPage] = this.GetLinkedPageUrl( AttributeKey.ConnectionRequestDetailPage, new Dictionary<string, string>
                {
                    ["ConnectionRequestId"] = "((Key))"
                } )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<Note> GetListQueryable( RockContext rockContext )
        {
            var celebrationNoteType = NoteTypeCache.Get( Rock.SystemGuid.NoteType.CELEBRATION_NOTE.AsGuid() );

            if ( celebrationNoteType == null )
            {
                return new NoteService( rockContext ).Queryable().Where( n => false );
            }

            // Exclude notes with empty text: the Connections Hub leaves an empty-text note behind
            // when a celebration is cleared and treats those as "no celebration", so they are not real entries.
            var noteQuery = new NoteService( rockContext ).Queryable()
                .Where( n => n.NoteTypeId == celebrationNoteType.Id
                    && n.EntityId != null
                    && n.Text != null
                    && n.Text.Trim() != "" );

            var dateRange = FilterDateRange.ToActualDateRange();
            var connectionTypeGuid = FilterConnectionTypeGuid;

            var crQuery = new ConnectionRequestService( rockContext ).Queryable();

            if ( dateRange?.Start != null )
            {
                crQuery = crQuery.Where( cr => cr.CreatedDateTime >= dateRange.Start );
            }

            if ( dateRange?.End != null )
            {
                crQuery = crQuery.Where( cr => cr.CreatedDateTime < dateRange.End );
            }

            if ( connectionTypeGuid.HasValue )
            {
                crQuery = crQuery.Where( cr => cr.ConnectionOpportunity.ConnectionType.Guid == connectionTypeGuid.Value );
            }

            var validConnectionRequestIds = crQuery.Select( cr => cr.Id );
            noteQuery = noteQuery.Where( n => validConnectionRequestIds.Contains( n.EntityId.Value ) );

            return noteQuery
                .Include( n => n.CreatedByPersonAlias.Person );
        }

        /// <inheritdoc/>
        protected override List<Note> GetListItems( IQueryable<Note> queryable, RockContext rockContext )
        {
            // Use an IQueryable subquery so EF generates a correlated subquery rather than
            // a WHERE IN (...) with a potentially large in-memory list.
            var connectionRequestIdQuery = queryable
                .Where( n => n.EntityId.HasValue )
                .Select( n => n.EntityId.Value );

            _connectionRequestMap = new ConnectionRequestService( rockContext )
                .Queryable()
                .Include( cr => cr.PersonAlias.Person.ConnectionStatusValue )
                .Include( cr => cr.ConnectionOpportunity.ConnectionType )
                .Include( cr => cr.ConnectorPersonAlias )
                .Where( cr => connectionRequestIdQuery.Contains( cr.Id ) )
                .ToDictionary( cr => cr.Id );

            var notes = base.GetListItems( queryable, rockContext );

            return notes
                .Where( n => n.EntityId.HasValue && _connectionRequestMap.ContainsKey( n.EntityId.Value ) )
                .ToList();
        }

        /// <inheritdoc/>
        protected override IQueryable<Note> GetOrderedListQueryable( IQueryable<Note> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( n => n.CreatedDateTime );
        }

        /// <inheritdoc/>
        protected override GridBuilder<Note> GetGridBuilder()
        {
            return new GridBuilder<Note>()
                .WithBlock( this )
                .AddTextField( "idKey", n => n.IdKey )
                .AddDateTimeField( "date", n => n.CreatedDateTime )
                .AddPersonField( "requester", n => GetConnectionRequest( n )?.PersonAlias?.Person )
                .AddTextField( "type", n => GetConnectionRequest( n )?.ConnectionOpportunity?.ConnectionType?.Name )
                .AddTextField( "opportunity", n => GetConnectionRequest( n )?.ConnectionOpportunity?.Name )
                .AddTextField( "storyDetails", n => n.Text )
                .AddTextField( "storyAuthorName", n => n.CreatedByPersonAlias?.Person?.FullName )
                .AddTextField( "storyAuthorPersonAliasGuid", n => n.CreatedByPersonAlias?.Guid.ToString() )
                .AddTextField( "connectionRequestIdKey", n => GetConnectionRequest( n )?.IdKey )
                .AddField( "canEdit", n => GetConnectionRequest( n )?.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) ?? false );
        }

        /// <summary>
        /// Updates the celebration story text and author for the specified note.
        /// </summary>
        [BlockAction]
        public BlockActionResult SaveCelebrationStory( string key, string text, Guid? authorPersonAliasGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var note = new NoteService( rockContext ).Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( note?.EntityId == null )
                {
                    return ActionNotFound();
                }

                // Enforce the same entity-level security the Connections Hub uses: edit rights come from
                // the parent Connection Request (which inherits from its Opportunity/Type and grants the
                // assigned connector edit when request security is enabled), not from the block itself.
                var connectionRequest = new ConnectionRequestService( rockContext ).Queryable()
                    .Include( cr => cr.ConnectionOpportunity.ConnectionType )
                    .Include( cr => cr.ConnectorPersonAlias )
                    .FirstOrDefault( cr => cr.Id == note.EntityId.Value );

                if ( connectionRequest == null )
                {
                    return ActionNotFound();
                }

                if ( !connectionRequest.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionForbidden();
                }

                note.Text = text;

                PersonAlias savedAlias = null;

                if ( authorPersonAliasGuid.HasValue )
                {
                    savedAlias = new PersonAliasService( rockContext ).Get( authorPersonAliasGuid.Value );
                    note.CreatedByPersonAliasId = savedAlias?.Id;
                }
                else
                {
                    note.CreatedByPersonAliasId = RequestContext.CurrentPerson?.PrimaryAliasId;
                }

                rockContext.SaveChanges();

                if ( savedAlias == null && note.CreatedByPersonAliasId.HasValue )
                {
                    savedAlias = new PersonAliasService( rockContext ).Get( note.CreatedByPersonAliasId.Value );
                }

                return ActionOk( new
                {
                    storyAuthorName = savedAlias?.Person?.FullName ?? string.Empty,
                    storyAuthorPersonAliasGuid = savedAlias?.Guid.ToString() ?? string.Empty
                } );
            }
        }

        /// <summary>
        /// Returns the <see cref="ConnectionRequest"/> associated with the given <see cref="Note"/>
        /// using the pre-loaded <see cref="_connectionRequestMap"/>.
        /// </summary>
        /// <param name="note">The celebration note.</param>
        /// <returns>The associated <see cref="ConnectionRequest"/>, or <c>null</c> if not found.</returns>
        private ConnectionRequest GetConnectionRequest( Note note )
        {
            if ( !note.EntityId.HasValue || _connectionRequestMap == null )
            {
                return null;
            }

            _connectionRequestMap.TryGetValue( note.EntityId.Value, out var connectionRequest );
            return connectionRequest;
        }

        #endregion Methods
    }
}
