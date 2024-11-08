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

using Rock.ViewModels.CheckIn;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInSimulator
{
    /// <summary>
    /// The configuration options for the Check-in Simulator block.
    /// </summary>
    public class CheckInSimulatorOptionsBag
    {
        /// <summary>
        /// Gets or sets the configuration templates that are available to
        /// choose from.
        /// </summary>
        /// <value>The configuration templates.</value>
        public List<ConfigurationTemplateBag> Templates { get; set; }

        /// <summary>
        /// Gets or sets the kiosks that are available to choose from.
        /// </summary>
        /// <value>The kiosks.</value>
        public List<ListItemBag> Kiosks { get; set; }
    }
}
