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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.CheckIn.CheckInScheduleBuilder
{
    /// <summary>
    /// The Check-in Schedule Builder Options Bag
    /// </summary>
    public class CheckInScheduleBuilderOptionsBag
    {
        /// <summary>
        /// The list of GroupTypes that can be chosen from
        /// </summary>
        public List<Guid> GroupTypes { get; set; }

        /// <summary>
        /// The list of Areas that can be chosen from
        /// </summary>
        public List<Guid> Areas { get; set; }

        /// <summary>
        /// The Default Schedule Category
        /// </summary>
        public ListItemBag DefaultScheduleCategory { get; set; }
    }
}
