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
using System.ComponentModel;

namespace Rock.Enums.Engagement
{
    /// <summary>
    /// Defines how the steps in a chart should be measured.
    /// </summary>
    public enum StepChartMeasure
    {
        /// <summary>
        /// Steps by Step Type
        /// </summary>
        Steps = 0,

        /// <summary>
        /// Total Steps
        /// </summary>
        TotalSteps = 1,

        /// <summary>
        /// The total step-adjusted impact
        /// </summary>
        [Description( "Total Step-Adjusted Impact" )]
        TotalStepAdjustedImpact = 2,

        /// <summary>
        /// Impact-Adjusted Steps
        /// </summary>
        [Description( "Impact-Adjusted Steps" )]
        ImpactAdjustedSteps = 3,

        /// <summary>
        /// Steps by Engagement Type
        /// </summary>
        EngagementType = 4,

        /// <summary>
        /// Steps by Organization Objective
        /// </summary>
        OrganizationObjective = 5,

        /// <summary>
        /// Steps by Program Completions
        /// </summary>
        ProgramCompletions = 6,

        /// <summary>
        /// Average Total Steps Per Weekend Attendee
        /// </summary>
        [Description( "Avg. Total Steps Per Weekend Attendee" )]
        AvgTotalStepsPerWeekendAttendee = 7,
    }
}
