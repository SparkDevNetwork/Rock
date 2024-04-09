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

using Rock.Enums.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Controls
{
    /// <summary>
    /// Represents the Cache-Control settings used inside Rock.
    /// </summary>
    public class RockCacheabilityBag
    {
        /// <summary>
        /// Gets or sets the type of the rock cacheability type.
        /// </summary>
        /// <value>
        /// The type of the rock cacheability.
        /// </value>
        public RockCacheabilityType RockCacheabilityType { get; set; }

        /// <summary>
        /// Gets or sets the maximum age.
        /// </summary>
        /// <value>
        /// The maximum age.
        /// </value>
        public TimeIntervalBag MaxAge { get; set; }

        /// <summary>
        /// Gets or sets the shared maximum age.
        /// </summary>
        /// <value>
        /// The shared maximum age.
        /// </value>
        public TimeIntervalBag SharedMaxAge { get; set; }
    }
}
