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
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.CheckIn.AttendanceList;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.CheckIn
{
    /// <summary>
    /// Displays a list of attendances.
    /// </summary>
    [DisplayName( "Attendance List" )]
    [Category( "Check-in" )]
    [Description( "Block for displaying the attendance history of a person or a group." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [SystemGuid.EntityTypeGuid( "73fd78df-5322-4716-a478-3cd0ea07a942" )]
    [SystemGuid.BlockTypeGuid( "e07607c6-5428-4ccf-a826-060f48cacd32" )]
    [CustomizedGrid]
    public class AttendanceList : RockEntityListBlockType<Attendance>
    {
        #region Keys

        private static class PageParameterKey
        {
            public const string ScheduleId = "ScheduleId";
            public const string LocationId = "LocationId";
            public const string GroupId = "GroupId";
            public const string AttendanceDate = "AttendanceDate";
        }

        private static class PreferenceKey
        {
            public const string FilterEnteredBy = "filter-entered-by";
            public const string FilterDidAttend = "filter-did-attend";
        }

        #endregion Keys

        #region Properties

        /// <summary>
        /// Gets the guid of the Person whose entries should be included in the results.
        /// </summary>
        /// <value>
        /// The entered by filter.
        /// </value>
        protected Guid? FilterEnteredBy => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterEnteredBy )
            .FromJsonOrNull<ListItemBag>()?.Value?.AsGuidOrNull();

        /// <summary>
        /// Gets the filter indicating DidAttend status of the results.
        /// </summary>
        /// <value>
        /// The DidAttend filter.
        /// </value>
        protected string FilterDidAttend => GetBlockPersonPreferences()
            .GetValue( PreferenceKey.FilterDidAttend );

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<AttendanceListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private AttendanceListOptionsBag GetBoxOptions()
        {
            var options = new AttendanceListOptionsBag();
            return options;
        }

        /// <inheritdoc/>
        protected override IQueryable<Attendance> GetListQueryable( RockContext rockContext )
        {
            var attendanceService = new AttendanceService( rockContext );
            IEnumerable<Attendance> attendance = new List<Attendance>();

            var groupId = PageParameter( PageParameterKey.GroupId ).AsIntegerOrNull();
            var scheduleId = PageParameter( PageParameterKey.ScheduleId ).AsIntegerOrNull();
            var locationId = PageParameter( PageParameterKey.LocationId ).AsIntegerOrNull();
            var attendanceDate = PageParameter( PageParameterKey.AttendanceDate ).AsDateTime();

            if ( groupId.HasValue && scheduleId.HasValue && locationId.HasValue && attendanceDate.HasValue )
            {
                var groupLocation = new GroupLocationService( rockContext ).Get( locationId.Value );

                //
                // Check for existing attendance records.
                //
                var attendanceQry = attendanceService.Queryable()
                    .Where( a =>
                        a.Occurrence.GroupId == groupId.Value &&
                        a.Occurrence.OccurrenceDate == attendanceDate.Value &&
                        a.Occurrence.LocationId == groupLocation.LocationId &&
                        a.Occurrence.ScheduleId == scheduleId );

                // Filter by DidAttend
                if ( FilterDidAttend.IsNotNullOrWhiteSpace() )
                {
                    var didAttend = FilterDidAttend.AsBoolean();
                    attendanceQry = attendanceQry.Where( a => a.DidAttend == didAttend );
                }

                // Filter by Entered By
                if ( FilterEnteredBy.HasValue )
                {
                    attendanceQry = attendanceQry.Where( a => a.CreatedByPersonAliasId.HasValue && a.CreatedByPersonAlias.Person.Guid == FilterEnteredBy.Value );
                }

                return attendanceQry;
            }

            return attendance.AsQueryable();
        }

        /// <inheritdoc/>
        protected override IQueryable<Attendance> GetOrderedListQueryable( IQueryable<Attendance> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( a => a.PersonAlias.Person.LastName )
                    .ThenBy( a => a.PersonAlias.Person.FirstName );
        }

        /// <inheritdoc/>
        protected override GridBuilder<Attendance> GetGridBuilder()
        {
            return new GridBuilder<Attendance>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.PersonAlias?.Person?.FullNameReversed )
                .AddField( "didAttend", a => a.DidAttend )
                .AddTextField( "note", a => a.Note )
                .AddTextField( "createdByPersonName", a => a.CreatedByPersonName )
                .AddDateTimeField( "createdDateTime", a => a.CreatedDateTime );
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
            using ( var rockContext = new RockContext() )
            {
                var entityService = new AttendanceService( rockContext );
                var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

                if ( entity == null )
                {
                    return ActionBadRequest( $"{Attendance.FriendlyTypeName} not found." );
                }

                if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
                {
                    return ActionBadRequest( $"Not authorized to delete {Attendance.FriendlyTypeName}." );
                }

                entity.DidAttend = false;
                rockContext.SaveChanges();

                if ( entity.Occurrence.LocationId != null )
                {
                    Rock.CheckIn.KioskLocationAttendance.Remove( entity.Occurrence.LocationId.Value );
                }

                return ActionOk();
            }
        }

        #endregion
    }
}
