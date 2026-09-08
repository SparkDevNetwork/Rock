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
using System.Text.Json.Serialization;

using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.CommunicationSkill;

/// <summary>
/// A single active member of a communication list, with the channels the member
/// can actually be reached on resolved.
/// </summary>
/// <remarks>
/// The <see cref="EntityResultBase.IdKey"/> and <see cref="EntityResultBase.Guid"/>
/// identify the group member record, so a caller can update or remove the
/// membership. The <see cref="PersonIdKey"/> and <see cref="PersonGuid"/> identify
/// the person the membership is for.
/// </remarks>
internal class CommunicationListRecipientResult : EntityResultBase
{
    /// <summary>
    /// The person id. This is not shown in the JSON output; <see cref="PersonIdKey"/>
    /// is returned instead.
    /// </summary>
    [JsonIgnore]
    internal int PersonId { get; set; }

    /// <summary>
    /// The identifier of the person the membership is for.
    /// </summary>
    public string PersonIdKey => PersonId != 0 ? PersonId.AsIdKey() : null;

    /// <summary>
    /// The unique identifier of the person the membership is for.
    /// </summary>
    public Guid? PersonGuid { get; set; }

    /// <summary>
    /// The full name of the person.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The person's email address, when one is on file.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// The channel the person prefers to be contacted on. When a member has no
    /// preference of their own this resolves to the person's own preference, which
    /// may still be "RecipientPreference" when neither is set.
    /// </summary>
    public string CommunicationPreference { get; set; }

    /// <summary>
    /// Indicates that the person can actually receive a bulk email (an active
    /// email address that is not opted out of mass email).
    /// </summary>
    public bool CanReceiveEmail { get; set; }

    /// <summary>
    /// Indicates that the person has an SMS enabled phone number and can receive a
    /// text message.
    /// </summary>
    public bool CanReceiveSms { get; set; }
}
