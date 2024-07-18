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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Bus.Message;
using Rock.Model;
using Rock.Utility;
using Rock.ViewModels.Utility;

namespace Rock.Blocks.CheckIn
{
    /// <summary>
    /// Monitors the cloud printing connections in Rock.
    /// </summary>

    [DisplayName( "Cloud Print Monitor" )]
    [Category( "Check-in" )]
    [Description( "Monitors the cloud printing connections in Rock." )]
    [IconCssClass( "fa fa-print" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

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
            var deviceService = new DeviceService( RockContext );
            var proxyIdQry = deviceService
                .Queryable()
                .Where( d => d.ProxyDeviceId.HasValue )
                .Select( d => d.ProxyDeviceId.Value );

            var devices = deviceService.Queryable()
                .Where( d => proxyIdQry.Contains( d.Id ) )
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
        /// Find all proxy connections in the web farm that exist for the set of
        /// encrypted proxyd <see cref="Device"/> identifiers.
        /// </summary>
        /// <param name="proxyIds">The encrypted proxy device identifiers.</param>
        /// <returns>A list of proxy connections.</returns>
        [BlockAction]
        public async Task<BlockActionResult> GetProxyConnections( List<string> proxyIds )
        {
            var results = new List<ProxyConnectionBag>();
            var cts = new CancellationTokenSource( 2_500 );

            var tasks = proxyIds.Select( p => IdHasher.Instance.GetId( p ) )
                .Where( p => p.HasValue )
                .Select( p =>
                {
                    return CloudPrintProxyStatusMessage.RequestAsync( p.Value, cts.Token );
                } );

            // Wait for each task to complete, ignoring any that never gave
            // a response (signalled by TaskCancelledException).
            foreach ( var task in tasks )
            {
                try
                {
                    var response = await task;

                    if ( response != null && response.Connections != null )
                    {
                        foreach ( var connection in response.Connections )
                        {
                            results.Add( new ProxyConnectionBag
                            {
                                ProxyId = IdHasher.Instance.GetHash( response.Id ),
                                ProxyName = connection.Name,
                                Priority = connection.Priority,
                                ServerName = response.NodeName
                            } );
                        }
                    }
                }
                catch ( TaskCanceledException )
                {
                    continue;
                }
            }

            return ActionOk( results );
        }

        #endregion

        private class ProxyConnectionBag
        {
            public string ProxyId { get; set; }

            public string ProxyName { get; set; }

            public int Priority { get; set; }

            public string ServerName { get; set; }
        }
    }
}
