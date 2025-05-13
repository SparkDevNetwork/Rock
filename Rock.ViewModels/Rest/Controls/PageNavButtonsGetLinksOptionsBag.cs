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

using System;
using System.Collections.Generic;

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// The options that can be passed to the GetLinks API action of
    /// the PageNavButtons control.
    /// </summary>
    public class PageNavButtonsGetLinksOptionsBag
    {
        /// <summary>
        /// GUID of the parent page of all the links to show
        /// </summary>
        public Guid RootPageGuid { get; set; }

        /// <summary>
        /// List of query string parameters to add each URL
        /// </summary>
        public Dictionary<string, string> QueryString { get; set; }

        /// <summary>
        /// List of page parameters to add each URL
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; }

        /// <summary>
        /// Guid of the current page, which is used to mark the current page's link active
        /// </summary>
        public Guid CurrentPageGuid { get; set; }
    }
}
