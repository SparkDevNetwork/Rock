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

using System.Collections.Generic;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Crm.FamilyPreRegistration
{
    /// <summary>
    /// The bag that contains all the schedule information for the Family Pre-Registration block.
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.ListItemBag" />
    public class FamilyPreRegistrationScheduleDateBag : ListItemBag
    {
        /// <summary>
        /// The schedule times associated with this schedule date.
        /// </summary>
        public List<ListItemBag> ScheduleTimes { get; set; }
    }
}
