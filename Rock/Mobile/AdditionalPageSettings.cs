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

using Rock.Enums.Cms;

namespace Rock.Mobile
{
    /// <summary>
    /// 
    /// </summary>
    public class AdditionalPageSettings
    {
        /// <summary>
        /// Gets or sets the lava event handler.
        /// </summary>
        /// <value>
        /// The lava event handler.
        /// </value>
        public string LavaEventHandler { get; set; }

        /// <summary>
        /// Gets or sets the CSS styles specific to this block.
        /// </summary>
        /// <value>
        /// The CSS styles specific to this block.
        /// </value>
        public string CssStyles { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the navigation bar should be hidden on this page.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the navigation bar should be hidden on this page; otherwise, <c>false</c>.
        /// </value>
        public bool HideNavigationBar { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the page full-screen.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the page should be shown full-screen; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Full-screen in this context means effectively switching to the Blank shell.
        /// This prevents the user from switching to any other pages but will also unload
        /// their navigation stack. If the user also specifies <see cref="HideNavigationBar"/>
        /// then they will get a true full-screen experience like a splash screen.
        /// </remarks>
        public bool ShowFullScreen { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the page will automatically
        /// reload when it becomes visible.
        /// </summary>
        /// <value><c>true</c> if the page automatically reloads when it becomes visible; otherwise, <c>false</c>.</value>
        public bool AutoRefresh { get; set; }

        /// <summary>
        /// Gets or sets the type of the page that will be displayed.
        /// </summary>
        /// <value>The type of the page that will be displayed.</value>
        public MobilePageType PageType { get; set; }

        /// <summary>
        /// Gets or sets the web page URL, this value is used if the <see cref="PageType"/>
        /// is set to <see cref="Rock.Enums.Cms.MobilePageType.InternalWebPage"/> or <see cref="Rock.Enums.Cms.MobilePageType.ExternalWebPage"/>.
        /// </summary>
        /// <value>The web page URL.</value>
        public string WebPageUrl { get; set; }
    }
}
