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
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.ScheduledJobHistoryList;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of service job histories.
    /// </summary>
    [DisplayName( "Scheduled Job History" )]
    [Category( "Core" )]
    [Description( "Lists all scheduled job's History." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the service job history details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "4b46834f-c9d3-43f3-9de2-8990d3a232c2" )]
    [Rock.SystemGuid.BlockTypeGuid( "2306068d-3551-4c10-8db8-133c030fa4fa" )]
    [CustomizedGrid]
    public class ScheduledJobHistoryList : RockEntityListBlockType<ServiceJobHistory>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string ScheduledJobId = "ScheduledJobId";
        }

        #endregion Keys

        #region Fields

        private string _serviceJobName = null;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<ScheduledJobHistoryListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private ScheduledJobHistoryListOptionsBag GetBoxOptions()
        {
            var options = new ScheduledJobHistoryListOptionsBag()
            {
                IsJobIdValid = PageParameter( PageParameterKey.ScheduledJobId ).IsNotNullOrWhiteSpace(),
                ServiceJobName = GetServiceJobName(),
            };

            return options;
        }

        /// <inheritdoc/>
        protected override IQueryable<ServiceJobHistory> GetListQueryable( RockContext rockContext )
        {
            var scheduledJobId = PageParameter( PageParameterKey.ScheduledJobId ).AsIntegerOrNull();

            if ( scheduledJobId.HasValue )
            {
                var jobHistoryService = new ServiceJobHistoryService( rockContext );
                return jobHistoryService.GetServiceJobHistory( scheduledJobId );
            }

            return new List<ServiceJobHistory>().AsQueryable();
        }

        /// <inheritdoc/>
        protected override IQueryable<ServiceJobHistory> GetOrderedListQueryable( IQueryable<ServiceJobHistory> queryable, RockContext rockContext )
        {
            return queryable.OrderByDescending( a => a.StartDateTime );
        }

        /// <inheritdoc/>
        protected override GridBuilder<ServiceJobHistory> GetGridBuilder()
        {
            return new GridBuilder<ServiceJobHistory>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddDateTimeField( "startDateTime", a => a.StartDateTime )
                .AddDateTimeField( "stopDateTime", a => a.StopDateTime )
                .AddTextField( "runDuration", a => FormatDuration( a.DurationSeconds ) )
                .AddTextField( "status", a => a.Status )
                .AddTextField( "statusMessage", a => a.StatusMessageAsHtml );
        }

        /// <summary>
        /// Formats the Duration Seconds as human friendly text.
        /// </summary>
        /// <param name="durationSeconds">The last run duration seconds.</param>
        /// <returns></returns>
        private string FormatDuration( int? durationSeconds )
        {
            var text = string.Empty;

            if ( durationSeconds.HasValue )
            {
                TimeSpan duration = TimeSpan.FromSeconds( durationSeconds.Value );

                if ( duration.Days > 0 )
                {
                    text = $"{duration.TotalHours:F2} hours";
                }
                else if ( duration.Hours > 0 )
                {
                    text = $"{duration:%h}h {duration:%m}m {duration:%s}s";
                }
                else if ( duration.Minutes > 0 )
                {
                    text = $"{duration:%m}m {duration:%s}s";
                }
                else
                {
                    text = $"{duration:%s}s";
                }
            }

            return text;
        }

        /// <summary>
        /// Gets the current service job's name.
        /// </summary>
        /// <returns></returns>
        private string GetServiceJobName()
        {
            if ( _serviceJobName.IsNullOrWhiteSpace() )
            {
                var scheduledJobId = PageParameter( PageParameterKey.ScheduledJobId ).AsIntegerOrNull();

                if ( scheduledJobId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        _serviceJobName = new ServiceJobService( rockContext ).GetSelect( scheduledJobId.Value, j => j.Name );
                    }
                }
            }
            return _serviceJobName;
        }

        #endregion
    }
}
