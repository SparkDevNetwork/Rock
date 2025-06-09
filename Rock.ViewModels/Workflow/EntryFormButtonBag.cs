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

namespace Rock.ViewModels.Workflow
{
    /// <summary>
    /// Defines a single button to display on an entry form.
    /// </summary>
    public class EntryFormButtonBag
    {
        /// <summary>
        /// The name of the action to perform when the button is clicked.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// The title of the button that can be used when rendering an HTML
        /// button is not possible.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The HTML that will render the button.
        /// </summary>
        public string Html { get; set; }
    }
}
