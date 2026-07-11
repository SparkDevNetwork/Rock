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

namespace Rock.ViewModels.Blocks.Event.CalendarNavigation
{
    /// <summary>
    /// Represents a single wizard step rendered by the Calendar Navigation block.
    /// </summary>
    public class CalendarNavigationItemBag
    {
        /// <summary>
        /// The CSS class for the step's icon.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// The text displayed beneath the step's icon.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The URL this step navigates to, or <c>null</c> when the step is not a
        /// reachable ancestor and should render as plain text.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Whether this step represents the context currently being viewed.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Whether this step is an ancestor of the current context (rendered as completed).
        /// </summary>
        public bool IsComplete { get; set; }
    }
}
