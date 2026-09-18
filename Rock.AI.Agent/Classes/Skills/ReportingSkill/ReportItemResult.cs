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

using System.Collections.Generic;

using Rock.AI.Agent.Classes.Entity;

namespace Rock.AI.Agent.Classes.Skills.ReportingSkill;

/// <summary>
/// One row of a report's output. The IdKey identifies the underlying record; the
/// remaining columns are the report's fields keyed by their column header.
/// </summary>
internal class ReportItemResult : EntityResultBase
{
    /// <summary>
    /// The report's column values for this row, keyed by column header. Standard
    /// value types (boolean, number) are the native typed value; dates are a
    /// round-trip string in the organization's time zone; everything else is the
    /// report's formatted string, with any column the caller may not view masked.
    /// </summary>
    public Dictionary<string, object> Values { get; set; }
}
