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
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Classes.Skills.PersonSkill;

/// <summary>
/// Represents a single communication that was created for a person.
/// </summary>
internal class CommunicationHistoryResult : EntityResultBase
{
    /// <summary>
    /// The name of the communication.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The medium entity type identifier that was used to send the communication.
    /// </summary>
    [JsonIgnore]
    public int? MediumEntityTypeId { get; set; }

    /// <summary>
    /// The name of the medium used to send the communication.
    /// </summary>
    public string Medium => MediumEntityTypeId.HasValue ? EntityTypeCache.Get( MediumEntityTypeId.Value )?.FriendlyName : null;

    /// <summary>
    /// The date the communication was sent.
    /// </summary>
    public DateTime? SentDateTime { get; set; }

    /// <summary>
    /// The person that sent the communication.
    /// </summary>
    public PersonResult Sender { get; set; }

    /// <summary>
    /// The status of the communication.
    /// </summary>
    public CommunicationRecipientStatus? Status { get; set; }

    /// <summary>
    /// A descriptive reason for the communication status, if applicable.
    /// </summary>
    public string StatusMessage { get; set; }
}
