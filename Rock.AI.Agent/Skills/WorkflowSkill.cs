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
using System.ComponentModel;
using System.Linq;

using Microsoft.Extensions.Logging;

using Rock.Attribute;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills
{
    /// <summary>
    /// Provides data lookup and analytics functions focused on site activity in Rock RMS,
    /// particularly person-centric website analytics such as page visits, grouped by site.
    /// </summary>

    [Description( "This skill provides access to working with workflows." )]
    [AgentSkillGuid( "018b20f6-95f8-4942-aa3c-0cd68c4896a5" )]
    [EntityTypeGuid( "32f6c37b-86ef-4873-8293-86beb54a1a59" )]
    [WorkflowTypeField( "Workflow Types",
        Description = "The workflow types that this skill can execute.",
        IsRequired = true,
        AllowMultiple = true,
        Key = ConfigurationKey.WorkflowTypes,
        Order = 0 )]
    internal sealed partial class WorkflowSkill : AgentSkillComponent
    {
        #region Keys

        private static class ConfigurationKey
        {
            public const string WorkflowTypes = "workflowTypes";
        }

        #endregion

        #region Fields

        /// <summary>
        /// The logger for this instance.
        /// </summary>
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowSkill"/> class.
        /// </summary>
        /// <param name="logger">Logger for diagnostics and error reporting.</param>
        public WorkflowSkill( ILogger<WorkflowSkill> logger )
        {
            _logger = logger ?? throw new ArgumentNullException( nameof( logger ) );
        }

        #endregion

        private List<WorkflowTypeCache> GetConfiguredWorkflowTypes()
        {
            if ( !ConfigurationValues.TryGetValue( ConfigurationKey.WorkflowTypes, out var workflowTypesRaw ) )
            {
                workflowTypesRaw = string.Empty;
            }

            var workflowTypeGuids = workflowTypesRaw.SplitDelimitedValues().AsGuidList();

            return WorkflowTypeCache.GetMany( workflowTypeGuids, AgentRequestContext.RockContext ).ToList();
        }
    }
}