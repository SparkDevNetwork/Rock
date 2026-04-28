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
    /// Represents an individual exception log entry in the exception hierarchy.
    /// </summary>
    public class ExceptionLogItemBag
    {
        /// <summary>
        /// Gets or sets the identifier of this exception log entry.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the formatted date/time when this exception occurred.
        /// </summary>
        public string ExceptionDate { get; set; }

        /// <summary>
        /// Gets or sets the exception class name (e.g., "System.Web.HttpException").
        /// </summary>
        public string ExceptionType { get; set; }

        /// <summary>
        /// Gets or sets the source application or object that threw the exception.
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Gets or sets the exception message.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the call stack trace for this exception.
        /// </summary>
        public string StackTrace { get; set; }

        /// <summary>
        /// Gets or sets the name of the site where this exception occurred.
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// Gets or sets the internal name of the page where this exception occurred.
        /// </summary>
        public string PageName { get; set; }

        /// <summary>
        /// Gets or sets the URL of the page where this exception occurred.
        /// </summary>
        public string PageUrl { get; set; }

        /// <summary>
        /// Gets or sets the parsed query string items as key-value pairs.
        /// </summary>
        public List<ListItemBag> QueryStringItems { get; set; }

        /// <summary>
        /// Gets or sets the full name of the person who triggered this exception.
        /// </summary>
        public string PersonFullName { get; set; }

        /// <summary>
        /// Gets or sets the IdKey of the person who triggered this exception,
        /// used for linking to the person's profile page.
        /// </summary>
        public string PersonIdKey { get; set; }
    }
}
