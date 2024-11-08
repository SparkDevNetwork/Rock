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
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Engagement.SignUp.SignUpFinder
{
    /// <summary>
    /// The additional information required to build the custom settings UI for the Sign-Up Finder block.
    /// </summary>
    public class SignUpFinderCustomSettingsOptionsBag
    {
        /// <summary>
        /// Gets or sets the available sign-up project group type guids for an individual to filter the results by.
        /// </summary>
        /// <value>
        /// The available sign-up project group type guids for an individual to filter the results by.
        /// </value>
        public List<Guid> AvailableProjectTypeGuids { get; set; }

        /// <summary>
        /// Gets or sets the available group attributes for an individual to filter the results by.
        /// </summary>
        /// <value>
        /// The available group attributes for an individual to filter the results by.
        /// </value>
        public List<ListItemBag> AvailableDisplayAttributeFilters { get; set; }

        /// <summary>
        /// Gets or sets the available campuses for an individual to filter the results by.
        /// </summary>
        /// <value>
        /// The available campuses for an individual to filter the results by.
        /// </value>
        public List<ListItemBag> AvailableCampuses { get; set; }
    }
}
