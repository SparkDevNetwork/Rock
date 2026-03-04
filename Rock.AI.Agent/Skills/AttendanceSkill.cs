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
using System.ComponentModel;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    /// <summary>
    /// Provides data lookup and analytics functions focused on attendance data.
    /// </summary>

    [Description( "This skill provides access to attendance related data." )]
    [AgentUsage( "The term 'room' is synonymous with 'location' for attendance data." )]
    [AgentSkillGuid( "79beff06-9ae4-402e-a29a-9f2d0c53a592" )]
    [EntityTypeGuid( "7b6a564f-49bf-4005-9173-97d97e3da02c" )]
    internal sealed partial class AttendanceSkill : AgentSkillComponent
    {
        #region Fields

        /// <summary>
        /// The logger for this instance.
        /// </summary>
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        /// <summary>
        /// The constructor for the Attendance Skill.
        /// </summary>
        /// <param name="logger">Logger for diagnostics and error reporting.</param>
        public AttendanceSkill( ILogger<ConnectionSkill> logger )
        {
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion
    }
}
