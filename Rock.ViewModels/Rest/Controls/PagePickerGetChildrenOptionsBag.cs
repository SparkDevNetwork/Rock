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
    /// The options that can be passed to the GetChildren API action of
    /// the PagePicker control.
    /// </summary>
    public class PagePickerGetChildrenOptionsBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the parent page whose
        /// children are to be enumerated.
        /// </summary>
        /// <value>The unique identifier of the parent page.</value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the root page whose
        /// children are to be enumerated.
        /// </summary>
        /// <value>The unique identifier of the root page.</value>
        public Guid? RootPageGuid { get; set; }

        /// <summary>
        /// Gets or sets the site type to filter results by.
        /// </summary>
        /// <value>The root location unique identifier.</value>
        public int? SiteType { get; set; }

        /// <summary>
        /// Gets or sets the list of unique identifiers of pages that should
        /// be excluded from the results
        /// </summary>
        /// <value>The security grant token.</value>
        public List<Guid> HidePageGuids { get; set; }

        /// <summary>
        /// Gets or sets the security grant token to use when performing
        /// authorization checks.
        /// </summary>
        /// <value>The security grant token.</value>
        public string SecurityGrantToken { get; set; }
    }
}
