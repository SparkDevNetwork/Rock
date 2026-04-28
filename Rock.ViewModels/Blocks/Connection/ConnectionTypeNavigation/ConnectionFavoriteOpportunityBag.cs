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

namespace Rock.ViewModels.Blocks.Connection.ConnectionTypeNavigation
{
    /// <summary>
    /// A bag that contains information about a single favorited connection opportunity
    /// for the Connection Type Navigation block.
    /// </summary>
    public class ConnectionFavoriteOpportunityBag
    {
        /// <summary>
        /// Gets or sets the IdKey of this connection opportunity.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the name of this connection opportunity.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class for this connection opportunity.
        /// </summary>
        public string IconCssClass { get; set; }
    }
}
