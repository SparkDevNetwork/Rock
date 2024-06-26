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
using Rock.Enums.CheckIn;

namespace Rock.ViewModels.Rest.CheckIn
{
    /// <summary>
    /// The options that will be provided to the SearchForFamilies check-in
    /// REST endpoint.
    /// </summary>
    public class SearchForFamiliesOptionsBag
    {
        /// <summary>
        /// Gets or sets the check-in configuration template identifier.
        /// This identifies the configuration that should be used when
        /// validating the search and compiling results.
        /// </summary>
        /// <value>The check-in configuration template identifier.</value>
        public string ConfigurationTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the optional kiosk identifier.
        /// </summary>
        /// <value>The optional kiosk identifier.</value>
        public string KioskId { get; set; }

        /// <summary>
        /// Gets or sets the term to search for.
        /// </summary>
        /// <value>The term to search for.</value>
        public string SearchTerm { get; set; }

        /// <summary>
        /// Gets or sets the type of the search to perform.
        /// </summary>
        /// <value>The type of the search to perform.</value>
        public FamilySearchMode SearchType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to prioritize families
        /// that are part of the same campus as the kiosk. This requires that
        /// the KioskGuid parameter be specified.
        /// </summary>
        /// <value><c>true</c> if families in the same campus as the kiosk are prioritized; otherwise, <c>false</c>.</value>
        public bool PrioritizeKioskCampus { get; set; }
    }
}
