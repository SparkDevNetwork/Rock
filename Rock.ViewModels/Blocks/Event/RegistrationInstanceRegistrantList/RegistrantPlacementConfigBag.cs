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

namespace Rock.ViewModels.Blocks.Event.RegistrationInstanceRegistrantList
{
    /// <summary>
    /// Describes one registration template placement that should be rendered
    /// as a button in the Placements column of the Registration Instance -
    /// Registrant List block.
    /// </summary>
    public class RegistrantPlacementConfigBag
    {
        /// <summary>
        /// Gets or sets the identifier of the registration template placement.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the placement.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class to display on the placement button.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a registrant may be placed
        /// into more than one group for this placement. When true the button
        /// also displays the number of groups the registrant is placed in.
        /// </summary>
        public bool IsMultiplePlacementAllowed { get; set; }
    }
}
