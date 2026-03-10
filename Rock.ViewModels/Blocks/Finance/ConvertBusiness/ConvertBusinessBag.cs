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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Finance.ConvertBusiness
{
    /// <summary>
    /// Contains initialization values used when the Convert Business block first loads.
    /// </summary>
    public class ConvertBusinessBag
    {
        /// <summary>
        /// Gets or sets the default person connection status.
        /// </summary>
        /// <value>
        /// The default person connection status.
        /// </value>
        public ListItemBag DefaultPersonConnectionStatus { get; set; }
    }
}
