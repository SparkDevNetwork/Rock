﻿// <copyright>
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
using Rock.ViewModel.NonEntities;

namespace Rock.ViewModel.Blocks.Groups.GroupScheduleUnavailability
{
    /// <summary>
    /// Gets the content bag to be passed into mobile.
    /// </summary>
    public class ContentBag
    {
        // Gets or sets the XAML.
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets a list containing group member names and Guids
        /// </summary>
        public List<ListItemViewModel> GroupInformation { get; set; }

        /// <summary>
        /// Gets or sets a list containing family member's names and Guids
        /// </summary>
        public List<ListItemViewModel> FamilyMemberInformation { get; set; }

        /// <summary>
        /// Gets or sets a boolean representing whether or not a description is required.
        /// </summary>
        public bool IsDescriptionRequired { get; set; }
    }
}
