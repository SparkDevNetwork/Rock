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

namespace Rock.ViewModels.Blocks.Event.RegistrationEntry
{
    /// <summary>
    /// RegistrationEntryBlockFeeItemViewModel
    /// </summary>
    public sealed class RegistrationEntryFeeItemBag
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the cost.
        /// </summary>
        /// <value>
        /// The cost.
        /// </value>
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>
        /// The unique identifier.
        /// </value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the remaining fee items when the registration was started. Used to calcualte items remaining as fees are added/removed across registrants during a registration.
        /// </summary>
        /// <value>
        /// The original count remaining.
        /// </value>
        public int? OriginalCountRemaining { get; set; }

        /// <summary>
        /// Gets or sets the count remaining.
        /// </summary>
        /// <value>
        /// The count remaining.
        /// </value>
        public int? CountRemaining { get; set; }
    }
}
