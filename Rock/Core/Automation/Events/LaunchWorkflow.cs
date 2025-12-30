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
using System.Collections.Generic;
using System.ComponentModel;

using Rock.Data;
using Rock.Net;
using Rock.Security;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Core.Automation.Events
{
    /// <summary>
    /// Launches a workflow in response to a trigger.
    /// </summary>
    [DisplayName( "Launch Workflow" )]

    [Rock.SystemGuid.EntityTypeGuid( "c1599b50-8403-4310-8e2e-34948cac385b" )]
    internal partial class LaunchWorkflow : AutomationEventComponent
    {
        #region Keys

        private static class ConfigurationKey
        {
            /// <summary>
            /// The unique identifier of the workflow type that will be launched.
            /// </summary>
            public const string WorkflowType = "workflowType";

            /// <summary>
            /// The optional name to assign to the workflow, Lava enabled.
            /// </summary>
            public const string WorkflowName = "workflowName";
        }

        #endregion Keys

        #region Properties

        /// <inheritdoc/>
        public override string IconCssClass => "ti ti-settings-cog";

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override string GetEventName( Dictionary<string, string> privateConfiguration, RockContext rockContext )
        {
            var workflowTypeGuid = privateConfiguration.GetValueOrNull( ConfigurationKey.WorkflowType )?.AsGuidOrNull();

            if ( workflowTypeGuid.HasValue )
            {
                var workflowType = WorkflowTypeCache.Get( workflowTypeGuid.Value, rockContext );

                if ( workflowType != null )
                {
                    return $"Launch Workflow: {workflowType.Name}";
                }
            }

            return "Launch Workflow";
        }

        /// <inheritdoc/>
        public override string GetEventDescription( Dictionary<string, string> privateConfiguration, RockContext rockContext )
        {
            var workflowTypeGuid = privateConfiguration.GetValueOrNull( ConfigurationKey.WorkflowType )?.AsGuidOrNull();

            if ( workflowTypeGuid.HasValue )
            {
                var workflowType = WorkflowTypeCache.Get( workflowTypeGuid.Value, rockContext );

                return workflowType?.Description ?? string.Empty;
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public override AutomationEventExecutor CreateExecutor( int automationEventId, Dictionary<string, string> privateConfiguration, RockContext rockContext )
        {
            var workflowTypeGuid = privateConfiguration.GetValueOrNull( ConfigurationKey.WorkflowType )?.AsGuidOrNull();
            var workflowName = privateConfiguration.GetValueOrNull( ConfigurationKey.WorkflowName );

            if ( workflowTypeGuid.HasValue )
            {
                var workflowType = WorkflowTypeCache.Get( workflowTypeGuid.Value, rockContext );

                if ( workflowType != null )
                {
                    return new LaunchWorkflowExecutor( workflowType.Guid, workflowName );
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public override DynamicComponentDefinitionBag GetComponentDefinition( Dictionary<string, string> privateConfiguration, RockContext rockContext, RockRequestContext requestContext )
        {
            return new DynamicComponentDefinitionBag
            {
                Url = requestContext.ResolveRockUrl( "~/Obsidian/Controls/Internal/Automation/Events/launchWorkflow.obs" ),
                Options = new Dictionary<string, string>
                {
                },
            };
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPublicConfiguration( Dictionary<string, string> privateConfiguration, RockContext rockContext, RockRequestContext requestContext )
        {
            var publicConfiguration = new Dictionary<string, string>( privateConfiguration );
            var workflowTypeGuid = privateConfiguration.GetValueOrNull( ConfigurationKey.WorkflowType )?.AsGuidOrNull();
            ListItemBag workflowTypeBag = null;

            if ( workflowTypeGuid.HasValue )
            {
                workflowTypeBag = WorkflowTypeCache.Get( workflowTypeGuid.Value, rockContext )
                    ?.ToListItemBag();
            }

            publicConfiguration[ConfigurationKey.WorkflowType] = workflowTypeBag?.ToCamelCaseJson( false, false );

            return publicConfiguration;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> GetPrivateConfiguration( Dictionary<string, string> publicConfiguration, RockContext rockContext, RockRequestContext requestContext )
        {
            var privateConfiguration = new Dictionary<string, string>();

            if ( publicConfiguration.TryGetValue( ConfigurationKey.WorkflowType, out var workflowTypeJson ) )
            {
                privateConfiguration[ConfigurationKey.WorkflowType] = workflowTypeJson.FromJsonOrNull<ListItemBag>()
                    ?.Value
                    ?.AsGuidOrNull()
                    ?.ToString() ?? string.Empty;
            }

            if ( publicConfiguration.TryGetValue( ConfigurationKey.WorkflowName, out var workflowName ) )
            {
                privateConfiguration[ConfigurationKey.WorkflowName] = workflowName;
            }

            return privateConfiguration;
        }

        /// <inheritdoc/>
        public override Dictionary<string, string> ExecuteComponentRequest( Dictionary<string, string> request, SecurityGrant securityGrant, RockContext rockContext, RockRequestContext requestContext )
        {
            return null;
        }

        #endregion
    }
}
