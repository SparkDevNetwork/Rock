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
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Entity;
using Rock.Data;
using Rock.SystemGuid;

namespace Rock.AI.Agent.Skills
{
    /// <summary>
    /// Represents a skill for managing reminders in the Rock application.
    /// </summary>
    /// <remarks>
    /// This skill provides functionality to manage reminders, including adding, updating, deleting, and listing reminders.
    /// It also allows for looking up reminder types.
    /// </remarks>
    [Description( "This skill provides functionality to manage reminders." )]
    [AgentSkillGuid( "A7CDC0C6-DCA6-4E77-9295-245B18556BB1" )]
    [EntityTypeGuid( "41179AB0-702C-435D-94BA-EC6EAE22E39B" )]
    [AgentUsage( "Reminders do not care about time of day. They are always for a specific date." )]
    internal sealed partial class ReminderSkill : AgentSkillComponent
    {
        #region Fields

        /// <summary>
        /// The logger instance for logging messages.
        /// </summary>
        private readonly ILogger<ReminderSkill> _logger;

        /// <summary>
        /// The factory for creating RockContext instances.
        /// </summary>
        private readonly IRockContextFactory _rockContextFactory;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ReminderSkill"/> class.
        /// </summary>
        /// <param name="rockContextFactory">The factory for creating RockContext instances.</param>
        /// <param name="logger">The logger instance for logging messages.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="rockContextFactory"/> or <paramref name="logger"/> is null.</exception>
        public ReminderSkill( IRockContextFactory rockContextFactory, ILogger<ReminderSkill> logger )
        {
            _rockContextFactory = rockContextFactory ?? throw new ArgumentNullException( nameof( rockContextFactory ) );
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion

        #region Shared Helpers

        /// <summary>
        /// Gets the result object for a reminder.
        /// </summary>
        /// <param name="reminder">The reminder object.</param>
        /// <param name="entityName">The name of the entity associated with the reminder.</param>
        /// <returns>A <see cref="ReminderResult"/> object containing the reminder details.</returns>
        private ReminderResult GetReminderResult( Rock.Model.Reminder reminder, string entityName )
        {
            return new ReminderResult
            {
                Id = reminder.Id,
                EntityName = entityName,
                EntityType = new KeyNameResult
                {
                    Id = reminder.ReminderType.EntityTypeId,
                    Name = reminder.ReminderType.EntityType.Name
                },
                ReminderType = new KeyNameResult
                {
                    Id = reminder.ReminderTypeId,
                    Name = reminder.ReminderType.Name
                },
                IsComplete = reminder.IsComplete,
                Note = reminder.Note,
                ReminderDate = reminder.ReminderDate,
                RenewPeriodDays = reminder.RenewPeriodDays,
                RenewMaxCount = reminder.RenewMaxCount,
                RenewCurrentCount = reminder.RenewCurrentCount
            };
        }

        #endregion
    }
}
