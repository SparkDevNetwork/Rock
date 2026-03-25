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

using System;

using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.EventRegistrationSkill;

/// <summary>
/// Represents a single registration instance.
/// </summary>
internal class RegistrationInstanceResult : EntityResultBase
{
    /// <summary>
    /// The name of the registration instance.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The template that defines the structure of this instance.
    /// </summary>
    public RegistrationTemplateResult RegistrationTemplate { get; set; }

    /// <summary>
    /// The date and time that the registration opens.
    /// </summary>
    public DateTime? StartDateTime { get; set; }

    /// <summary>
    /// The date and time that the registration closes.
    /// </summary>
    public DateTime? EndDateTime { get; set; }

    /// <summary>
    /// The maximum number of attendees allowed to register for this instance.
    /// </summary>
    public int? MaximumAttendees { get; set; }

    /// <summary>
    /// The person listed as the contact for this registration instance.
    /// </summary>
    public PersonResult ContactPerson { get; set; }

    /// <summary>
    /// The phone number listed as the contact for this registration instance.
    /// </summary>
    public string ContactPhoneNumber { get; set; }

    /// <summary>
    /// The email address listed as the contact for this registration instance.
    /// </summary>
    public string ContactEmail { get; set; }

    /// <summary>
    /// The account that any payments for this registration will be deposited into.
    /// </summary>
    public FinancialAccountResult PaymentAccount { get; set; }
}
