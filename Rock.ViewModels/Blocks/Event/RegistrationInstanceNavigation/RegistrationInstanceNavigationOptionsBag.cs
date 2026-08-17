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

using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Event.RegistrationInstanceNavigation
{
    /// <summary>
    /// The initialization options for the Registration Instance Navigation block.
    /// </summary>
    public class RegistrationInstanceNavigationOptionsBag
    {
        /// <summary>
        /// The navigation tabs to display for the current Registration Instance,
        /// in the order they should be rendered. Already filtered by the current
        /// person's access and the template's Wait List configuration.
        /// </summary>
        public List<NavigationTabBag> Tabs { get; set; } = new List<NavigationTabBag>();
    }
}
