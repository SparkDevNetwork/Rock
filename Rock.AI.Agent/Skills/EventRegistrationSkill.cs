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
using System.ComponentModel;

using Microsoft.Extensions.Logging;

using Rock.AI.Agent.Annotations;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    /// <summary>
    /// This skill provides access to working with event registrations.
    /// </summary>

    [Description( "This skill provides access to working with event registrations" )]
    [AgentUsage( "This skill provides access to working with event registrations" )]

    [AgentSkillGuid( "127a9726-5922-47e6-ae2d-a3901f60367b" )]
    [EntityTypeGuid( "07712e0b-efa9-4775-a74c-75f6c58d5210" )]
    internal sealed partial class EventRegistrationSkill : AgentSkillComponent
    {
        #region Fields

        /// <summary>
        /// The logger for this instance.
        /// </summary>
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        /// <summary>
        /// The constructor for the Event Registration Skill.
        /// </summary>
        /// <param name="logger">Logger for diagnostics and error reporting.</param>
        public EventRegistrationSkill( ILogger<EventCalendarSkill> logger )
        {
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion
    }
}
