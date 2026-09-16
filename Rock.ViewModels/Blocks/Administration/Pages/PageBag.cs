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

namespace Rock.ViewModels.Blocks.Administration.Pages
{
    /// <summary>
    /// The information needed to add or edit a child page in the add/edit form.
    /// </summary>
    public class PageBag
    {
        /// <summary>
        /// Gets or sets the identifier key of the page. Empty when adding a new page.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the internal name of the page.
        /// </summary>
        public string InternalName { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the selected layout for the page.
        /// </summary>
        public string Layout { get; set; }
    }
}
