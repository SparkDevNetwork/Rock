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

using System;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// Bag containing the email section information needed for the Email Editor control.
    /// </summary>
    public class EmailEditorEmailSectionBag
    {
        /// <summary>
        /// Gets or sets the email section category.
        /// </summary>
        public ListItemBag Category { get; set; }

        /// <summary>
        /// Gets or sets the email section unique identifier.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is a system email section.
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the email section name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the email section source markup.
        /// </summary>
        public string SourceMarkup { get; set; }

        /// <summary>
        /// Gets or sets the email section thumbnail binary file.
        /// </summary>
        public ListItemBag ThumbnailBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the email section usage summary.
        /// </summary>
        public string UsageSummary { get; set; }

        /// <summary>
        /// Gets or sets the security grant token to use when performing
        /// authorization checks.
        /// </summary>
        /// <value>The security grant token.</value>
        public string SecurityGrantToken { get; set; }
    }
}