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
using System.Threading.Tasks;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Core.Automation.Events
{
    /// <summary>
    /// Handles execution for the <see cref="LaunchWorkflow"/> event component.
    /// </summary>
    class LaunchWorkflowExecutor : AutomationEventExecutor
    {
        #region Fields

        /// <summary>
        /// The unique identifier of the workflow type that will be launched.
        /// </summary>
        private readonly Guid _workflowTypeGuid;

        /// <summary>
        /// The optional Lava template to use as the name of the workflow.
        /// </summary>
        private readonly string _nameTemplate;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LaunchWorkflowExecutor"/> class.
        /// </summary>
        /// <param name="workflowTypeGuid">The unique identifier of the workflow type that will be launched.</param>
        /// <param name="nameTemplate">The optional Lava template to use as the name of the workflow.</param>
        public LaunchWorkflowExecutor( Guid workflowTypeGuid, string nameTemplate )
        {
            _workflowTypeGuid = workflowTypeGuid;
            _nameTemplate = nameTemplate;
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override void Execute( AutomationRequest request )
        {
            var entity = request.Entity;
            var entityTypeId = entity?.TypeId;
            var entityId = entity?.Id;
            var mergeFields = new Dictionary<string, object>();

            foreach ( var value in request.Values )
            {
                mergeFields[value.Key] = value.Value;
            }

            var name = _nameTemplate.ResolveMergeFields( mergeFields );

            Task.Run( () =>
            {
                using ( var rockContext = new RockContext() )
                {
                    using ( var activity = Observability.ObservabilityHelper.StartActivity( $"WT:" ) )
                    {
                        var entityTypeCache = entityTypeId.HasValue
                            ? EntityTypeCache.Get( entityTypeId.Value, rockContext )
                            : null;

                        if ( entityTypeCache != null )
                        {
                            activity.DisplayName = $"WT: {entityTypeCache?.FriendlyName}";
                        }

                        var workflowType = WorkflowTypeCache.Get( _workflowTypeGuid, rockContext );

                        if ( workflowType == null )
                        {
                            return;
                        }

                        var workflow = Model.Workflow.Activate( workflowType, name );
                        var workflowService = new WorkflowService( rockContext );

                        workflowService.Process( workflow, entity, out _ );
                    }
                }
            } );
        }

        #endregion
    }
}
