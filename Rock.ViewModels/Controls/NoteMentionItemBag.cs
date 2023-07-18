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

namespace Rock.ViewModels.Controls
{
    /// <summary>
    /// A single mention source item used to display the possible matches
    /// when adding a new mention reference.
    /// </summary>
    public class NoteMentionItemBag
    {
        /// <summary>
        /// Gets or sets the identifier to store with the mention.
        /// </summary>
        /// <value>The mention identifier.</value>
        public string Identifier { get; set; }

        /// <summary>
        /// Gets or sets the name to display for this mention source.
        /// </summary>
        /// <value>The name to display.</value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the e-mail address for this mention source.
        /// </summary>
        /// <value>The e-mail address for this mention source.</value>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the image to display with name.
        /// </summary>
        /// <value>The image to display or <c>null</c> if no image is available.</value>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the name of the campus this mention belongs to.
        /// </summary>
        /// <value>
        /// The name of the campus this mention belongs to.
        /// </value>
        public string CampusName { get; set; }
    }
}
