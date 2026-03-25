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

using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.EventRegistrationSkill;

internal class RegistrationInstanceResult : EntityResultBase
{
    public string Name { get; set; }

    public RegistrationTemplateResult RegistrationTemplate { get; set; }

    public DateTime? StartDateTime { get; set; }

    public DateTime? EndDateTime { get; set; }

    public int? MaximumAttendees { get; set; }

    public PersonResult ContactPerson { get; set; }

    public string ContactPhoneNumber { get; set; }

    public string ContactEmail { get; set; }

    public FinancialAccountResult PaymentAccount { get; set; }
}
