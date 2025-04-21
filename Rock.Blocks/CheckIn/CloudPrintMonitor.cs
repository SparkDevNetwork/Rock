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
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Bus.Message;
using Rock.Model;
using Rock.RealTime;
using Rock.RealTime.Topics;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.CheckIn
{
    /// <summary>
    /// Monitors the cloud printing connections in Rock.
    /// </summary>

    [DisplayName( "Cloud Print Monitor" )]
    [Category( "Check-in" )]
    [Description( "Monitors the cloud printing connections in Rock." )]
    [IconCssClass( "fa fa-print" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "3fa3b98c-89d7-4157-b3af-f2c27f8a70aa" )]
    [Rock.SystemGuid.BlockTypeGuid( "8f436a19-482a-41a7-aab3-e5ec34d15d19" )]
    public class CloudPrintMonitor : RockBlockType
    {
        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            return new
            {
                Proxies = GetPrintProxyBags()
            };
        }

        /// <summary>
        /// Gets the bags that represent all known proxy devices registered
        /// in the database.
        /// </summary>
        /// <returns>A list of <see cref="ListItemBag"/> objects that represent the proxy devices.</returns>
        private List<ListItemBag> GetPrintProxyBags()
        {
            var proxyDeviceTypeId = DefinedValueCache.Get( SystemGuid.DefinedValue.DEVICE_TYPE_CLOUD_PRINT_PROXY.AsGuid(), RockContext ).Id;
            var deviceService = new DeviceService( RockContext );

            var devices = deviceService.Queryable()
                .Where( d => d.DeviceTypeValueId == proxyDeviceTypeId )
                .ToList();

            return devices
                .OrderBy( d => d.Name )
                .Select( d => new ListItemBag
                {
                    Value = d.IdKey,
                    Text = d.Name
                } )
                .ToList();
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Subscribes to the real-time channels.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        [BlockAction]
        public async Task<BlockActionResult> SubscribeToRealTime( string connectionId )
        {
            var topicChannels = RealTimeHelper.GetTopicContext<ICloudPrint>().Channels;

            await topicChannels.AddToChannelAsync( connectionId, CloudPrintTopic.ProxyStatusChannel );

            return ActionOk();
        }

        /// <summary>
        /// Requests that all servers send a status update on their connected
        /// proxies.
        /// </summary>
        /// <returns>A status code that indicates if the request succeeded.</returns>
        [BlockAction]
        public async Task<BlockActionResult> UpdateProxyStatus()
        {
            await CloudPrintSendProxyStatusMessage.PublishAsync();

            return ActionOk();
        }

        #endregion
    }
}
