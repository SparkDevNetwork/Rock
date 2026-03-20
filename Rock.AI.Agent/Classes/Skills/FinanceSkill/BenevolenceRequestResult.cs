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
using System.Text.Json.Serialization;
using System.Windows.Documents;

using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.FinanceSkill;

internal class BenevolenceRequestResult : EntityResultBase
{
    public PersonResult Person { get; set; }

    [JsonIgnore]
    public string FirstName { get; set; }

    [JsonIgnore]
    public string LastName { get; set; }

    public DateTime? RequestDateTime { get; set; }

    public string RequestText { get; set; }

    public PersonResult AssignedToPerson { get; set; }

    public string ResultSummary { get; set; }

    public KeyNameResult RequestStatus { get; set; }

    public string NextSteps { get; set; }

    public List<BenevolenceDocumentResult> Documents { get; set; }

    public List<BenevolenceResultResult> Results { get; set; }
}
