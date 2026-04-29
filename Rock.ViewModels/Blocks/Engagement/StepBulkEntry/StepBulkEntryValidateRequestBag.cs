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
    /// The request bag for the ValidatePeople block action.
    /// </summary>
    public class StepBulkEntryValidateRequestBag
    {
        /// <summary>
        /// Gets or sets the list of person alias unique identifiers to validate.
        /// </summary>
        public List<Guid> PersonAliasGuids { get; set; }

        /// <summary>
        /// Gets or sets the step type unique identifier to validate against.
        /// </summary>
        public Guid StepTypeGuid { get; set; }
    }
}
