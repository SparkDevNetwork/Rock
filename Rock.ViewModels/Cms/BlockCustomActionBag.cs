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

namespace Rock.ViewModels.Cms
{
    /// <summary>
    /// Identifies a single custom configuration action/button that will be
    /// made available in a block's configuration bar.
    /// </summary>
    public class BlockCustomActionBag
    {
        /// <summary>
        /// Gets or sets the icon CSS class used to display the button.
        /// </summary>
        /// <value>
        /// The icon CSS class used to display the button.
        /// </value>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the tooltip text of the button.
        /// </summary>
        /// <value>
        /// The tooltip text of the button.
        /// </value>
        public string Tooltip { get; set; }

        /// <summary>
        /// Gets or sets the URL of the component that will be loaded when
        /// the button is clicked. The component will be added to a hidden
        /// div container. It is expected that the component will display a
        /// modal to handle its user interface.
        /// </summary>
        /// <value>
        /// The URL of the component that will be loaded when the button is clicked.
        /// </value>
        public string ComponentFileUrl { get; set; }
    }
}
