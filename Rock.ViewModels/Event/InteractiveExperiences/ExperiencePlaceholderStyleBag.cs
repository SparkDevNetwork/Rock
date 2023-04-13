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

namespace Rock.ViewModels.Event.InteractiveExperiences
{
    /// <summary>
    /// Content and style for an action placeholder.
    /// </summary>
    public class ExperiencePlaceholderStyleBag
    {
        /// <summary>
        /// Gets or sets the short title to display.
        /// </summary>
        /// <value>The short title to display.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the additional message details to display.
        /// </summary>
        /// <value>The additional message details to display.</value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the header image URL.
        /// </summary>
        /// <value>The header image URL.</value>
        public string HeaderImage { get; set; }
    }
}
