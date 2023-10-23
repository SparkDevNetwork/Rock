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

namespace Rock.ViewModels.Blocks.Cms.MediaAccountDetail
{
    /// <summary>
    /// Class MediaAccountBag.
    /// Implements the <see cref="Rock.ViewModels.Utility.EntityBagBase" />
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class MediaAccountBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the type of the component entity.
        /// </summary>
        public ListItemBag ComponentEntityType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the Name of the MediaAccount. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the last refresh.
        /// </summary>
        /// <value>
        /// The last refresh.
        /// </value>
        public string LastRefresh { get; set; }

        /// <summary>
        /// Gets or sets the metric data.
        /// </summary>
        /// <value>
        /// The metric data.
        /// </value>
        public string MetricData { get; set; }
    }
}
