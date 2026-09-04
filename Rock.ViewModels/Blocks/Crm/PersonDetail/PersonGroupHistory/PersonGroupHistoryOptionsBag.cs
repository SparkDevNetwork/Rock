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

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.PersonGroupHistory
{
    /// <summary>
    /// The block configuration options for the Person Group History block.
    /// </summary>
    public class PersonGroupHistoryOptionsBag
    {
        /// <summary>
        /// Gets or sets the group type unique identifiers the filter picker is allowed to choose from.
        /// This is constrained by the block's Group Types setting, or all history-enabled group types when that setting is blank.
        /// </summary>
        public List<Guid> AvailableGroupTypeGuids { get; set; }

        /// <summary>
        /// Gets or sets the initial timeline view mode ("Year" or "Month").
        /// </summary>
        public string DefaultViewMode { get; set; }
    }
}
