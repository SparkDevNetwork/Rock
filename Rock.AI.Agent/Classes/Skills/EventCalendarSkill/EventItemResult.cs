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

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.EventCalendarSkill;

internal class EventItemResult : EntityResultBase
{
    public string Name { get; set; }

    public string Summary { get; set; }

    public bool? IsApproved { get; set; }

    public PersonResult ApprovedByPerson { get; set; }

    public List<KeyNameResult> Audiences { get; set; }

    public List<KeyNameResult> Calendars { get; set; }

}
