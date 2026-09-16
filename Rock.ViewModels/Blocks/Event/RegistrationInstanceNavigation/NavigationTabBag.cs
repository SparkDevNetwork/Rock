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

namespace Rock.ViewModels.Blocks.Event.RegistrationInstanceNavigation
{
    /// <summary>
    /// Represents a single navigation tab rendered by the Registration Instance Navigation block.
    /// </summary>
    public class NavigationTabBag
    {
        /// <summary>
        /// The IdKey of the page this tab points to. Used by the Vue template
        /// as a stable <c>:key</c> so the list diffs correctly even if two
        /// tabs were to share the same URL.
        /// </summary>
        public string PageIdKey { get; set; }

        /// <summary>
        /// The title displayed on the tab.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The URL the tab navigates to. Includes the current Registration
        /// Instance page parameters, with route-specific parameters removed.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Whether this tab represents the page currently being viewed.
        /// </summary>
        public bool IsActive { get; set; }
    }
}
