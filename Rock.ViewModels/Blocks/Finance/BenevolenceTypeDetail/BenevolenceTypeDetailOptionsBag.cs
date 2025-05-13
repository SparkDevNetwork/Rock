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

using Rock.ViewModels.Utility;
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Finance.BenevolenceTypeDetail
{
    /// <summary>
    /// 
    /// </summary>
    public class BenevolenceTypeDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the trigger types.
        /// </summary>
        /// <value>
        /// The trigger types.
        /// </value>
        public List<ListItemBag> TriggerTypes { get; set; }

        /// <summary>
        /// Gets or sets the step statuses.
        /// </summary>
        /// <value>
        /// The step statuses.
        /// </value>
        public List<ListItemBag> Statuses { get; set; }
    }
}
