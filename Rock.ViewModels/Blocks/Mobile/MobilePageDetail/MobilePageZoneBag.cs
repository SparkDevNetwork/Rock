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

namespace Rock.ViewModels.Blocks.Mobile.MobilePageDetail
{
    /// <summary>
    /// A zone in the layout (right column) and the blocks it contains.
    /// </summary>
    public class MobilePageZoneBag
    {
        /// <summary>
        /// Gets or sets the name of the zone as defined in the layout.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the blocks placed in this zone, in display order.
        /// </summary>
        public List<MobilePageBlockBag> Blocks { get; set; }
    }
}
