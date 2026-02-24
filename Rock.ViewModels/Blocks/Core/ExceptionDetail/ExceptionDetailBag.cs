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

namespace Rock.ViewModels.Blocks.Core.ExceptionDetail
{
    /// <summary>
    /// The view model bag returned on initialization of the Exception Detail block.
    /// Contains the exception summary and the full exception hierarchy.
    /// </summary>
    public class ExceptionDetailBag
    {
        /// <summary>
        /// Gets or sets the ISO 8601 formatted date and time the exception occurred.
        /// </summary>
        public string ExceptionDate { get; set; }

        /// <summary>
        /// Gets or sets the description message of the root-level exception.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the name of the site where the exception occurred.
        /// Will be null if the exception did not occur on a site.
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// Gets or sets the internal name of the page where the exception occurred.
        /// Will be null if a page name is not available.
        /// </summary>
        public string PageName { get; set; }

        /// <summary>
        /// Gets or sets the URL of the page where the exception occurred.
        /// </summary>
        public string PageUrl { get; set; }

        /// <summary>
        /// Gets or sets the raw query string from the page request that threw the exception.
        /// </summary>
        public string QueryString { get; set; }

        /// <summary>
        /// Gets or sets the full name of the user who experienced the exception.
        /// Will be null if the exception was not associated with a user.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the cookies data captured at the time of the exception.
        /// This may contain HTML-formatted content.
        /// </summary>
        public string Cookies { get; set; }

        /// <summary>
        /// Gets or sets the server variables captured at the time of the exception.
        /// This may contain HTML-formatted content.
        /// </summary>
        public string ServerVariables { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether cookie data is available to display.
        /// When false, the Cookies section should be hidden.
        /// </summary>
        public bool HasCookies { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether server variable data is available to display.
        /// When false, the Server Variables section should be hidden.
        /// </summary>
        public bool HasServerVariables { get; set; }

        /// <summary>
        /// Gets or sets the ordered list of exception log entries in the hierarchy,
        /// starting from the outermost exception and including any inner exceptions.
        /// </summary>
        public List<ExceptionDetailItemBag> ExceptionItems { get; set; }
    }
}
