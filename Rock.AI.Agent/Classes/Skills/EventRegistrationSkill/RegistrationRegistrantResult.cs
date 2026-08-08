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
/// Represents a single registrant that has been registered for an event registration instance.
/// </summary>
internal class RegistrationRegistrantResult : EntityResultBase
{
    /// <summary>
    /// The person that was registered.
    /// </summary>
    public PersonResult Person { get; set; }

    /// <summary>
    /// The registration instance they were registered for.
    /// </summary>
    public RegistrationInstanceResult RegistrationInstance { get; set; }

    /// <summary>
    /// The date and time they were registered.
    /// </summary>
    public DateTime? RegisteredDateTime { get; set; }

    /// <summary>
    /// The person that registered them.
    /// </summary>
    public PersonResult RegisteredBy { get; set; }

    /// <summary>
    /// Determines if the person is currently on the wait list.
    /// </summary>
    public bool? IsOnWaitList { get; set; }

    /// <summary>
    /// The base cost of the registration for this individual. This does not
    /// include any fees.
    /// </summary>
    public decimal? BaseRegistrationCost { get; set; }
}
