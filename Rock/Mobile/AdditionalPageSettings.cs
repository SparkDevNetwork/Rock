﻿// <copyright>
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
    }
}
