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
using Rock.ViewModels.Entities;

namespace Rock.ViewModels.Blocks.Core.ScheduleDetail
{
    public class ScheduleDetailOptionsBag
    {
        public DateTimeOffset? NextOccurrence { get; set; }

        public List<ScheduleCategoryExclusionBag> Exclusions { get; set; }

        public bool HasScheduleWarning { get; set; }

        public bool CanDelete { get; set; }

        public bool HasAttendance { get; set; }

        public string HelpText { get; set; }
    }
}
