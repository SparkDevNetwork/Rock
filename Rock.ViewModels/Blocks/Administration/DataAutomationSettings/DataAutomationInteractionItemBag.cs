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

namespace Rock.ViewModels.Blocks.Administration.DataAutomationSettings
{
    /// <summary>
    /// Represents a single interaction channel row within the reactivate or
    /// inactivate criteria, tracking whether the channel participates and the
    /// number of days used for the interaction window.
    /// </summary>
    public class DataAutomationInteractionItemBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the interaction channel.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the display name of the interaction channel.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this interaction channel is enabled.
        /// </summary>
        public bool IsInteractionTypeEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number of days used for the last-interaction window.
        /// </summary>
        public int? LastInteractionDays { get; set; }
    }
}
