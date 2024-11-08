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

using System.Threading.Tasks;

using Rock.Net;
using Rock.Web.Cache;

namespace Rock.Blocks
{
    /// <summary>
    /// Defines the basic elements that are required for all rock blocks to implement.
    /// </summary>
    public interface IRockBlockType
    {
        /// <summary>
        /// Gets the block identifier.
        /// </summary>
        /// <value>
        /// The block identifier.
        /// </value>
        int BlockId { get; }

        /// <summary>
        /// Gets or sets the block cache.
        /// </summary>
        /// <value>
        /// The block cache.
        /// </value>
        BlockCache BlockCache { get; set; }

        /// <summary>
        /// Gets or sets the page cache.
        /// </summary>
        /// <value>
        /// The page cache.
        /// </value>
        PageCache PageCache { get; set; }

        /// <summary>
        /// Gets or sets the request context.
        /// </summary>
        /// <value>
        /// The request context.
        /// </value>
        RockRequestContext RequestContext { get; set; }

        /// <summary>
        /// Gets the object that will be used to initialize the block by the client.
        /// </summary>
        /// <param name="clientType">The type of client that is requesting the configuration data.</param>
        /// <returns>An object that will be JSON encoded and sent to the client.</returns>
        Task<object> GetBlockInitializationAsync( RockClientType clientType );
    }
}
