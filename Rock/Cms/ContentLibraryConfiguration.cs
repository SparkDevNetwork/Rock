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

namespace Rock.Cms
{
    /// <summary>
    /// Used by the ContentLibraryConfigurationJson of the <seealso cref="Rock.Model.ContentChannel"/>.
    /// </summary>
    [Serializable]
    public class ContentLibraryConfiguration
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the license type value unique identifier.
        /// </summary>
        public Guid? LicenseTypeValueGuid { get; set; }

        /// <summary>
        /// Gets or sets the summary attribute unique identifier.
        /// </summary>
        public Guid? SummaryAttributeGuid { get; set; }

        /// <summary>
        /// Gets or sets the author attribute unique identifier.
        /// </summary>
        public Guid? AuthorAttributeGuid { get; set; }

        /// <summary>
        /// Gets or sets the image attribute unique identifier.
        /// </summary>
        public Guid? ImageAttributeGuid { get; set; }
    }
}
