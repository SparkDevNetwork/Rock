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

namespace Rock.AI.Agent.Classes.Skills.StreakSkill;

/// <summary>
/// Represents a single streak record.
/// </summary>
internal class StreakResult : EntityResultBase
{
    /// <summary>
    /// The type of streak.
    /// </summary>
    public StreakTypeResult StreakType { get; set; }

    /// <summary>
    /// The person that is working on the streak.
    /// </summary>
    public PersonResult Person { get; set; }

    /// <summary>
    /// The date the person was enrolled in this streak.
    /// </summary>
    public DateTime? EnrollmentDate { get; set; }

    /// <summary>
    /// The date the current streak started.
    /// </summary>
    public DateTime? CurrentStreakStartDate { get; set; }

    /// <summary>
    /// The number of sequential occurrences for the current streak.
    /// </summary>
    public int? CurrentStreakCount { get; set; }

    /// <summary>
    /// The date the longest streak started.
    /// </summary>
    public DateTime? LongestStreakStartDate { get; set; }

    /// <summary>
    /// The number of sequential occurrences for the longest streak.
    /// </summary>
    public int? LongestStreakCount { get; set; }
}
