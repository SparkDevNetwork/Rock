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

namespace Rock.ViewModels.Blocks.Engagement.StepBulkEntry
{
    /// <summary>
    /// The per-person validation result returned by the ValidatePeople block action.
    /// </summary>
    public class StepBulkEntryValidatePersonResultBag
    {
        /// <summary>
        /// Gets or sets the full name of the person for display.
        /// </summary>
        public string PersonName { get; set; }

        /// <summary>
        /// Gets or sets the person alias unique identifier, used to match
        /// and remove people from the selection.
        /// </summary>
        public Guid PersonAliasGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a step can be created for this person.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the validation error messages for this person.
        /// Empty when the person is valid.
        /// </summary>
        public List<string> Errors { get; set; }
    }
}
