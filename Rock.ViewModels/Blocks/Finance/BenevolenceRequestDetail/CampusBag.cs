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

namespace Rock.ViewModels.Blocks.Finance.BenevolenceRequestDetail
{
    /// <summary>
    /// Represents a container for campus-related information, including identifiers and descriptive details
    /// <u>that are relevevant in the context of a benevolence request.</u>
    /// </summary>
    /// <remarks>The <see cref="CampusBag"/> class provides properties to store unique identifiers and
    /// descriptive information about a campus. It includes both a numeric and a GUID identifier, as well as a name and
    /// description.</remarks>
    public class CampusBag
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        public Guid? Guid { get; set; }

        /// <summary>
        /// Gets or sets the name of the Campus. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Campus name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }
    }
}
