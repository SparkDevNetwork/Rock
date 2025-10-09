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

using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Enums.Controls;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Utility;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.AuditList;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Core.Grid;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of audits.
    /// </summary>

    [DisplayName( "Audit List" )]
    [Category( "Core" )]
    [Description( "Displays a list of audits." )]
    [IconCssClass( "ti ti-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "8d4a9e56-30f1-4a2d-bd00-7803d7d51909" )]
    [Rock.SystemGuid.BlockTypeGuid( "120552e2-5c36-4220-9a73-fbbbd75b0964" )]
    [CustomizedGrid]
    public class AuditList : RockListBlockType<AuditList.AuditRow>
    {

        #region Keys

        private static class PreferenceKey
        {
            public const string FilterCreatedDateRange = "filter-created-date-range";
        }

        #endregion Keys

        #region Fields

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
            var box = new ListBlockBox<AuditListOptionsBag>();
            var builder = GetGridBuilder();

            if ( FilterCreatedDateRange == null )
            {
                var defaultSlidingDateRange = new SlidingDateRangeBag
                {
                    RangeType = SlidingDateRangeType.Last,
                    TimeUnit = TimeUnitType.Month,
                    TimeValue = 1
                };

                this.PersonPreferences.SetValue( PreferenceKey.FilterCreatedDateRange, defaultSlidingDateRange.ToDelimitedSlidingDateRangeOrNull() );
                this.PersonPreferences.Save();
            }

            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private AuditListOptionsBag GetBoxOptions()
        {
            var options = new AuditListOptionsBag()
            {
                EntityTypeItems = EntityTypeCache.All()
                .Where( e => e.IsEntity )
                .OrderBy( e => e.FriendlyName )
                .ToListItemBagList()
            };

            return options;
        }

        /// <inheritdoc/>
        protected override IQueryable<AuditRow> GetListQueryable( RockContext rockContext )
        {
            var query = new AuditService( rockContext ).Queryable().AsNoTracking();

            query = FilterByCreatedDate( query );

            return query.Select( a => new AuditRow
                {
                    Audit = a,
                    AuditDetailsCount = a.Details.Count(),
                    EntityTypeName = a.EntityType != null ? a.EntityType.FriendlyName : string.Empty,
                    PersonProjection = new PersonProjection
                    {
                        NickName = a.PersonAlias.Person.NickName,
                        LastName = a.PersonAlias.Person.LastName,
                        PhotoId = a.PersonAlias.Person.PhotoId,
                        Age = a.PersonAlias.Person.Age,
                        Gender = a.PersonAlias.Person.Gender,
                        RecordTypeValueId = a.PersonAlias.Person.RecordTypeValueId,
                        AgeClassification = a.PersonAlias.Person.AgeClassification,
                        ConnectionStatusValueId = a.PersonAlias.Person.ConnectionStatusValueId,
                        Id = a.PersonAlias.Person.Id,
                    }
                } );
        }

        /// <inheritdoc/>
        protected override IQueryable<AuditRow> GetOrderedListQueryable( IQueryable<AuditRow> queryable, RockContext rockContext )
        {
            return queryable.AsNoTracking()
                .OrderByDescending( q => q.Audit.Id );
        }

        /// <inheritdoc/>
        protected override List<AuditRow> GetListItems( IQueryable<AuditRow> queryable, RockContext rockContext )
        {
            var audits = queryable.ToList();

            foreach( var audit in audits )
            {
                if ( audit.PersonProjection.Id.HasValue )
                {
                    audit.Person = new PersonFieldBag
                    {
                        IdKey = IdHasher.Instance.GetHash( audit.PersonProjection.Id.Value ),
                        NickName = audit.PersonProjection.NickName,
                        LastName = audit.PersonProjection.LastName,
                    };

                    var initials = $"{audit.Person.NickName.Truncate( 1, false )}{audit.Person.LastName.Truncate( 1, false )}";
                    audit.Person.PhotoUrl = Rock.Model.Person.GetPersonPhotoUrl(
                        initials,
                        audit.PersonProjection.PhotoId,
                        audit.PersonProjection.Age,
                        audit.PersonProjection.Gender ?? Gender.Unknown,
                        audit.PersonProjection.RecordTypeValueId,
                        audit.PersonProjection.AgeClassification
                    );

                    if ( audit.PersonProjection.ConnectionStatusValueId.HasValue )
                    {
                        var connectionStatusValue = DefinedValueCache.Get( audit.PersonProjection.ConnectionStatusValueId.Value );
                        if ( connectionStatusValue != null )
                        {
                            audit.Person.ConnectionStatus = connectionStatusValue.Value;
                        }
                    }
                }
            }

            return audits;
        }

        /// <inheritdoc/>
        protected override GridBuilder<AuditRow> GetGridBuilder()
        {
            return new GridBuilder<AuditRow>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.Audit.IdKey )
                .AddTextField( "auditType", a => a.Audit.AuditType.ConvertToStringSafe() )
                .AddTextField( "entityType", a => a.EntityTypeName )
                .AddTextField( "title", a => a.Audit.Title )
                .AddDateTimeField( "dateTime", a => a.Audit.DateTime )
                .AddField( "entityId", a => a.Audit.EntityId )
                .AddField( "properties", a => a.AuditDetailsCount )
                .AddField( "person", a => a.Person )
                .AddField( "personId", a => a.PersonProjection.Id );
        }

        /// <summary>
        /// Filters the queryable by the Created Date
        /// </summary>
        /// <param name="queryable">The <see cref="Audit"/> queryable</param>
        /// <returns></returns>
        private IQueryable<Audit> FilterByCreatedDate( IQueryable<Audit> queryable )
        {
            // Default to the last 1 month if a null/invalid range was selected.
            var defaultSlidingDateRange = new SlidingDateRangeBag
            {
                RangeType = SlidingDateRangeType.Last,
                TimeUnit = TimeUnitType.Month,
                TimeValue = 1
            };

            var dateRange = FilterCreatedDateRange.Validate( defaultSlidingDateRange ).ActualDateRange;
            var dateTimeStart = dateRange.Start;
            var dateTimeEnd = dateRange.End;

            queryable = queryable
                .Where( c =>
                    c.DateTime >= dateTimeStart &&
                    c.DateTime <= dateTimeEnd );

            return queryable;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets a list of <see cref="Rock.Model.AuditDetail"/> for the specified <paramref name="idKey"/>
        /// </summary>
        /// <param name="idKey">The identifier key of the audit whose details should be loaded.</param>
        /// <returns>A list of <see cref="Rock.Model.AuditDetail"/>
        /// whose <seealso cref="Rock.Model.Audit"/>.IdKey matches the <paramref name="idKey"/>
        /// </returns>
        [BlockAction]
        public BlockActionResult GetAuditDetails( string idKey )
        {
            using ( var rockContext = new RockContext() )
            {
                var auditId = new AuditService( rockContext ).Get( idKey )?.Id;

                if ( auditId == null )
                {
                    return ActionBadRequest( $"Audit Information not found" );
                }

                var auditDetailService = new AuditDetailService( rockContext );

                var auditDetails = auditDetailService
                    .Queryable()
                    .AsNoTracking()
                    .Where( a => a.AuditId == auditId )
                    .Select( a => new {
                        a.Property,
                        a.OriginalValue,
                        a.CurrentValue
                    })
                    .OrderBy( d => d.Property )
                    .ToList();

                if ( !auditDetails.Any() )
                {
                    return ActionBadRequest( $"No Properties" );
                }

                return ActionOk( auditDetails );
            }
        }

        #endregion

        #region Helper Classes

        public class AuditRow
        {
            public Audit Audit { get; set; }

            public int AuditDetailsCount { get; set; }

            public string EntityTypeName { get; set; }

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
