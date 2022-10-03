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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The value you'll receive from the Obsidian PagePicker control.
    /// </summary>
    public class PageRouteValueBag
    {
        /// <summary>
        /// The value representing the page, with a name and a GUID.
        /// </summary>
        /// <value>Representation of a page.</value>
        public ListItemBag Page { get; set; }

        /// <summary>
        /// The value representing the route, with a name and a GUID.
        /// </summary>
        /// <value>Representation of a route.</value>
        public ListItemBag Route { get; set; }
    }
}
