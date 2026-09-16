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

using Rock.AI.Agent.Classes.Common;

namespace Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;

/// <summary>
/// One group of similar exceptions, matching how Rock's own Exception List
/// summarizes them: exceptions sharing an exception type and the first 95
/// characters of their description are counted together.
/// </summary>
/// <remarks>
/// This is a summary rather than an entity, so it carries no identity of its own.
/// <see cref="SampleException"/> points at the most recent occurrence in the group
/// so a caller can read one in full without listing the whole group first.
/// </remarks>
internal class ExceptionSummaryResult
{
    /// <summary>
    /// The exception class, such as <c>System.NullReferenceException</c>.
    /// </summary>
    public string ExceptionType { get; set; }

    /// <summary>
    /// The shared leading portion of the description these exceptions group on.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// How many times this exception occurred within the requested date range.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// When this exception first occurred within the requested date range.
    /// </summary>
    public DateTime? FirstOccurredDateTime { get; set; }

    /// <summary>
    /// When this exception most recently occurred within the requested date range.
    /// </summary>
    public DateTime? LastOccurredDateTime { get; set; }

    /// <summary>
    /// The most recent occurrence in the group, for reading one in full or for
    /// listing the group's instances.
    /// </summary>
    public KeyNameResult SampleException { get; set; }
}
