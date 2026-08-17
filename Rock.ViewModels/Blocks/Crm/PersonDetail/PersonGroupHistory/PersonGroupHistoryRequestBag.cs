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
    /// The request sent to the GetGroupHistory block action.
    /// </summary>
    public class PersonGroupHistoryRequestBag
    {
        /// <summary>
        /// Gets or sets the group type unique identifiers selected in the filter. An empty list means no user filter is applied.
        /// </summary>
        public List<Guid> SelectedGroupTypeGuids { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the selected group types should be persisted as the person's block preference.
        /// </summary>
        public bool IsSavingPreference { get; set; }
    }
}
