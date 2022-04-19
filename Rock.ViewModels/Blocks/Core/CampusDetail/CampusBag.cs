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

namespace Rock.ViewModels.Blocks.Core.CampusDetail
{
    public class CampusBag : EntityBagBase
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsSystem { get; set; }

        public bool? IsActive { get; set; }

        public string ShortCode { get; set; }

        public string Url { get; set; }

        public string PhoneNumber { get; set; }

        public List<ListItemBag> ServiceTimes { get; set; }

        public string TimeZoneId { get; set; }

        public List<CampusScheduleBag> CampusSchedules { get; set; }

        public ListItemBag Location { get; set; }

        public ListItemBag LeaderPersonAlias { get; set; }

        public ListItemBag CampusStatusValue { get; set; }

        public ListItemBag CampusTypeValue { get; set; }
    }
}
