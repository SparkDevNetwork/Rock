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

using System;

namespace Rock.ViewModels.Blocks.Group.Scheduling.GroupScheduleToolbox
{
    /// <summary>
    /// A bag that contains information about the outcome of a "save sign-up" request for the group schedule toolbox block.
    /// </summary>
    public class SaveSignUpResponseBag
    {
        /// <summary>
        /// Gets or sets a friendly error message to describe any problems encountered while saving.
        /// </summary>
        public string SaveError { get; set; }

        /// <summary>
        /// Gets or sets the current sign-ups; will only be provided if the save failed.
        /// </summary>
        public SignUpsBag SignUps { get; set; }

        /// <summary>
        /// Gets or sets the updated sign-up occurrence; will only be provided if the save succeeded.
        /// </summary>
        public SignUpOccurrenceBag SignUpOccurrence { get; set; }

        /// <summary>
        /// Gets or sets the selected location unique identifier; will only be provided if the save succeeded.
        /// </summary>
        public Guid SelectedLocationGuid { get; set; }
    }
}
