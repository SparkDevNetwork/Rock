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

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInAreasAndGroups
{
    /// <summary>
    /// A bag that contains information about a check-in label for the Check-in Areas and Groups block.
    /// </summary>
    public class CheckInLabelBag
    {
        /// <summary>
        /// Gets or sets the hashed identifier of the check-in label entity attached to the area.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the label's display name, shown in the grid row.
        /// </summary>
        public string Name { get; set; }
    }
}
