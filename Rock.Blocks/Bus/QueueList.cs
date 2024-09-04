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
using Rock.Attribute;
using Rock.Bus.Queue;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Bus.QueueList;
using System.Linq;
using Rock.Obsidian.UI;

namespace Rock.Blocks.Bus
{
    /// <summary>
    /// Displays the details of a particular achievement type.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockDetailBlockType" />

    [DisplayName( "Queue List" )]
    [Category( "Bus" )]
    [Description( "Displays the details of bus queues." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the page short link details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.BlockTypeGuid( "8a5785fc-3094-4c2c-929a-3fd6d21da7f8" )]

    [Rock.SystemGuid.EntityTypeGuid( "BE20153D-8462-403D-B18D-8E8AFC274EE5")]
    public class QueueList : RockListBlockType<QueueListBag>
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

        /// <summary>
        /// Page Param Keys
        /// </summary>
        private static class PageParameterKey
        {
            public const string QueueKey = "QueueKey";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<QueueListOptionsBag>();

            var builder = GetGridBuilder();

            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the queue.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private QueueListOptionsBag GetBoxOptions()
        {
            var options = new QueueListOptionsBag();

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
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, "QueueKey", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<QueueListBag> GetListQueryable( RockContext rockContext )
        {
            var queueTypes = RockQueue.GetQueueTypes();

            var queues = queueTypes.Select( queueType =>
            {
                var queueInstance = RockQueue.Get( queueType ) as IRockQueue;
                var queueTypeString =
                   queueInstance is ISendCommandQueue ? "Command" :
                   queueInstance is IPublishEventQueue ? "Event" :
                   "Unknown";

                return new QueueListBag
                {
                    IdKey = queueType.FullName,
                    QueueName = queueInstance.Name,
                    QueueType = queueTypeString,
                    TimeToLiveSeconds = queueInstance.TimeToLiveSeconds,
                    RatePerMinute = queueInstance.StatLog?.MessagesConsumedLastMinute,
                    RatePerHour = queueInstance.StatLog?.MessagesConsumedLastHour,
                    RatePerDay = queueInstance.StatLog?.MessagesConsumedLastDay,
                    QueueTypeName = queueType.Name
                };
            } );

            return queues.AsQueryable();
        }

        /// <inheritdoc/>
        protected override GridBuilder<QueueListBag> GetGridBuilder()
        {
            return new GridBuilder<QueueListBag>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "queueName", a => a.QueueName )
                .AddTextField( "queueType", a => a.QueueType )
                .AddTextField( "queueTypeName", a => a.QueueTypeName )
                .AddField( "timeToLiveSeconds", a => a.TimeToLiveSeconds )
                .AddField( "ratePerMinute", a => a.RatePerMinute )
                .AddField( "ratePerHour", a => a.RatePerHour )
                .AddField( "ratePerDay", a => a.RatePerDay );
        }

        #endregion
    }
}
