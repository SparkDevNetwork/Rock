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

using Rock.AI.Agent.Classes.Common;

namespace Rock.AI.Agent.Classes.Skills.FinanceSkill;

/// <summary>
/// Represents the details of a benevolence result.
/// </summary>
internal class BenevolenceResultResult
{
    /// <summary>
    /// The type of result.
    /// </summary>
    public KeyNameResult ResultType { get; set; }

    /// <summary>
    /// The detailed description of the result.
    /// </summary>
    public string Details { get; set; }

    /// <summary>
    /// The amount of money, if any, that was given.
    /// </summary>
    public decimal? Amount { get; set; }
}
