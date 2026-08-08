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
using System.Collections.Generic;
using System.Text.Json.Serialization;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.FinanceSkill;

/// <summary>
/// Represents a single benevolence request.
/// </summary>
internal class BenevolenceRequestResult : EntityResultBase
{
    /// <summary>
    /// The person that is requesting the benevolence assistance.
    /// </summary>
    public PersonResult Person { get; set; }

    /// <summary>
    /// The first name of the person requesting the benevolence assistance.
    /// </summary>
    [JsonIgnore]
    public string FirstName { get; set; }

    /// <summary>
    /// The last name of the person requesting the benevolence assistance.
    /// </summary>
    [JsonIgnore]
    public string LastName { get; set; }

    /// <summary>
    /// The date the request was entered.
    /// </summary>
    public DateTime? RequestDateTime { get; set; }

    /// <summary>
    /// The text that describes the request.
    /// </summary>
    public string RequestText { get; set; }

    /// <summary>
    /// The person that has been assigned to review the benevolence request.
    /// </summary>
    public PersonResult AssignedToPerson { get; set; }

    /// <summary>
    /// The summary of the results of the benevolence request.
    /// </summary>
    public string ResultSummary { get; set; }

    /// <summary>
    /// The current status of the benevolence request.
    /// </summary>
    public KeyNameResult RequestStatus { get; set; }

    /// <summary>
    /// The next steps that were given to the requester.
    /// </summary>
    public string NextSteps { get; set; }

    /// <summary>
    /// A collection of documents that were provided as part of the benevolence request.
    /// </summary>
    public List<BenevolenceDocumentResult> Documents { get; set; }

    /// <summary>
    /// A collection of individual results that each describe something that
    /// was provided to the requester.
    /// </summary>
    public List<BenevolenceResultResult> Results { get; set; }
}
