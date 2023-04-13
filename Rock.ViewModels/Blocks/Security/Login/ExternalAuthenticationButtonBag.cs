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
namespace Rock.ViewModels.Blocks.Security.Login
{
    /// <summary>
    /// A bag that contains the required information to render an external authentication button.
    /// </summary>
    public class ExternalAuthenticationButtonBag
    {
        /// <summary>
        /// Gets or sets the text to display.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the authentication entity type guid.
        /// </summary>
        public string AuthenticationType { get; set; }

        /// <summary>
        /// Gets or sets button's the image URL.
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the CSS class to add to the button.
        /// </summary>
        public string CssClass { get; set; }
    }
}
