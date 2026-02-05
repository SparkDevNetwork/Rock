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
using Rock.Bus;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Bus.BusStatus;

namespace Rock.Blocks.Bus
{
    [DisplayName( "Bus Status" )]
    [Category( "Bus" )]
    [Description( "Gives insight into the message bus." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage(
        "Transport Select Page",
        Key = AttributeKey.TransportSelectPage,
        Description = "The page where the transport for the bus can be selected",
        DefaultValue = Rock.SystemGuid.Page.BUS_TRANSPORT,
        Order = 1 )]

    #endregion Block Attributes
    
    [Rock.SystemGuid.EntityTypeGuid( "9DFA8FD4-C3AA-440A-B1D6-1F8695C4AD5A" )]
    [Rock.SystemGuid.BlockTypeGuid( "A9BB6B68-44CD-4EC2-9B26-CD6C941877EB" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "C472300C-781F-4D73-B530-8C9F8A9927D4" )]
    public class BusStatus : RockBlockType
    {
        #region Attribute Keys

        private static class AttributeKey
        {
            public const string TransportSelectPage = "TransportSelectPage";
        }
        private static class NavigationUrlKey
        {
            public const string TransportSelectPage = "TransportSelectPage";
        }

        #endregion Attribute Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<BusStatusBag, BusStatusOptionsBag>();

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.Entity = GetBusStatusData();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private BusStatusOptionsBag GetBoxOptions()
        {
            var options = new BusStatusOptionsBag();

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
                [NavigationUrlKey.TransportSelectPage] = this.GetLinkedPageUrl( AttributeKey.TransportSelectPage )
            };
        }

        /// <summary>
        /// Gets the bus status data.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private BusStatusBag GetBusStatusData()
        {
            return new BusStatusBag
            {
                IsReady = RockMessageBus.IsReady(),
                TransportName = RockMessageBus.GetTransportName(),
                NodeName = RockMessageBus.NodeName,
                MessagesPerMinute = RockMessageBus.StatLog?.MessagesConsumedLastMinute,
                MessagesPerHour = RockMessageBus.StatLog?.MessagesConsumedLastHour,
                MessagesPerDay = RockMessageBus.StatLog?.MessagesConsumedLastDay
            };
        }

        #endregion Methods
    }
}