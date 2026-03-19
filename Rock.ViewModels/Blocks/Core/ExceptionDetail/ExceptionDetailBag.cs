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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.ExceptionDetail
{
    /// <summary>
    /// The bag containing the data for the Exception Detail block.
    /// </summary>
    public class ExceptionDetailBag
    {
        /// <summary>
        /// Gets or sets the formatted date/time when the base exception occurred.
        /// </summary>
        public string ExceptionDate { get; set; }

        /// <summary>
        /// Gets or sets the truncated description of the base exception.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the name of the site where the exception occurred.
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// Gets or sets the internal name of the page where the exception occurred.
        /// </summary>
        public string PageName { get; set; }

        /// <summary>
        /// Gets or sets the URL of the page where the exception occurred.
        /// </summary>
        public string PageUrl { get; set; }

        /// <summary>
        /// Gets or sets the parsed query string items as key-value pairs.
        /// The Text property contains the key and Value contains the value.
        /// </summary>
        public List<ListItemBag> QueryStringItems { get; set; }

        /// <summary>
        /// Gets or sets the full name of the person who triggered the exception.
        /// </summary>
        public string PersonFullName { get; set; }

        /// <summary>
        /// Gets or sets the IdKey of the person who triggered the exception,
        /// used for linking to the person's profile page.
        /// </summary>
        public string PersonIdKey { get; set; }

        /// <summary>
        /// Gets or sets the raw HTML content of the cookies at the time of the exception.
        /// </summary>
        public string Cookies { get; set; }

        /// <summary>
        /// Gets or sets the raw HTML content of the server variables at the time of the exception.
        /// </summary>
        public string ServerVariables { get; set; }

        /// <summary>
        /// Gets or sets the ordered list of exception log items in the exception hierarchy.
        /// </summary>
        public List<ExceptionLogItemBag> ExceptionItems { get; set; }
    }
}
