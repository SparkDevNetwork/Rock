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

namespace Rock.ViewModels.Blocks.Event.RegistrationTemplateDetail
{
    /// <summary>
    /// A single option of a registration fee.
    /// </summary>
    public class RegistrationTemplateFeeItemBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the fee item.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the display order of the fee item within its fee.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the name of the fee item.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the cost of the fee item.
        /// </summary>
        public decimal? Cost { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of times the fee item can be used per registration instance.
        /// </summary>
        public int? MaximumUsageCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a registrant has already used this fee item.
        /// Fee items that are in use cannot be removed.
        /// </summary>
        public bool IsInUse { get; set; }
    }
}
