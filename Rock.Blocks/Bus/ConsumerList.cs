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

using System.ComponentModel;
using Rock.Attribute;
using Rock.Bus;
using Rock.Bus.Queue;
using Rock.Bus.Consumer;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Bus.ConsumerList;
using System.Linq;
using Rock.Obsidian.UI;

namespace Rock.Blocks.Bus
{
    /// <summary>
    /// Displays a list of consumers.
    /// </summary>

    [DisplayName( "Consumer List" )]
    [Category( "Bus" )]
    [Description( "Displays a list of consumers." )]
    [IconCssClass( "fa fa-list" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.BlockTypeGuid( "63f5509a-3d71-4f0f-a074-fa5869856038" )]

    [Rock.SystemGuid.EntityTypeGuid( "444BD66E-A715-4367-A3A6-5C0BBD6E93B4")]
    public class ConsumerList : RockListBlockType<ConsumerListBag>
    {
        #region Keys

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
            var box = new ListBlockBox<ConsumerListOptionsBag>();

            var builder = GetGridBuilder();

            box.ExpectedRowCount = null;
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the queue.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private ConsumerListOptionsBag GetBoxOptions()
        {
            var options = new ConsumerListOptionsBag();

            return options;
        }


        /// <inheritdoc/>
        protected override IQueryable<ConsumerListBag> GetListQueryable( RockContext rockContext )
        {
            var queueKey = PageParameter( PageParameterKey.QueueKey );
            var queue = RockQueue.Get( queueKey );

            if ( queue == null )
            {
                return Enumerable.Empty<ConsumerListBag>().AsQueryable();
            }

            var consumers = RockConsumer.GetConsumerTypes()
                .Where( ct => RockConsumer.GetQueue( ct ) == queue )
                .Select( ct =>
                {
                    var messageType = RockConsumer.GetMessageType( ct );

                    return new ConsumerListBag
                    {
                        ConsumerName = Reflection.GetFriendlyName( ct ),
                        QueueName = queue.Name,
                        MessageName = Reflection.GetFriendlyName( messageType )
                    };
                });

            return consumers.AsQueryable();
        }

        /// <inheritdoc/>
        protected override GridBuilder<ConsumerListBag> GetGridBuilder()
        {
            return new GridBuilder<ConsumerListBag>()
                .WithBlock( this )
                .AddTextField( "consumerName", a => a.ConsumerName )
                .AddTextField( "queueName", a => a.QueueName )
                .AddTextField( "messageName", a => a.MessageName );
        }

        #endregion
    }
}
