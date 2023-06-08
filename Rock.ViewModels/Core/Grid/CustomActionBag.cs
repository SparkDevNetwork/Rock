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

namespace Rock.ViewModels.Core.Grid
{
    /// <summary>
    /// Defines a custom action configured by the administrator.
    /// </summary>
    public class CustomActionBag
    {
        /// <summary>
        /// Gets or sets the name of the action. This should be one or two words
        /// that quickly describe the action.
        /// </summary>
        /// <value>The name of the action.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the route. If the route includes <c>{0}</c> then it
        /// will be replaced with the entity set identifier. Otherwise the a
        /// query string parameter of <c>EntitySetId</c> will be appended to
        /// the route.
        /// </summary>
        /// <value>The route.</value>
        public string Route { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>The icon CSS class.</value>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the description. This is the help text that describes
        /// the action in more detail.
        /// </summary>
        /// <value>The description of the action.</value>
        public string Description { get; set; }
    }
}
