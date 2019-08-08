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
using System.Data.Entity;
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.Workflow"/> entity objects
    /// </summary>
    public partial class StepWorkflowTriggerService : Service<StepWorkflowTrigger>
    {
        /// <summary>
        /// Get a friendly description of the settings for a Status Change trigger.
        /// </summary>
        /// <param name="stepWorkflowTriggerId">The step workflow trigger identifier.</param>
        /// <returns></returns>
        public string GetTriggerSettingsDescription( int stepWorkflowTriggerId )
        {
            var trigger = this.Queryable().AsNoTracking().FirstOrDefault( x => x.Id == stepWorkflowTriggerId );

            return this.GetTriggerSettingsDescription( trigger );
        }

        /// <summary>
        /// Get a friendly description of the settings for a Status Change trigger.
        /// </summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public string GetTriggerSettingsDescription( StepWorkflowTrigger trigger )
        {
            if ( trigger == null )
            {
                return string.Empty;
            }

            var settings = new StepWorkflowTrigger.StatusChangeTriggerSettings( trigger.TypeQualifier );

            return this.GetTriggerSettingsDescription( trigger.TriggerType, settings );
        }

        /// <summary>
        /// Get a friendly description of the settings for a Status Change trigger.
        /// </summary>
        /// <param name="triggerType">Type of the trigger.</param>
        /// <param name="settings">The settings.</param>
        /// <returns></returns>
        public string GetTriggerSettingsDescription( StepWorkflowTrigger.WorkflowTriggerCondition triggerType, StepWorkflowTrigger.StatusChangeTriggerSettings settings )
        {
            if ( triggerType == StepWorkflowTrigger.WorkflowTriggerCondition.StatusChanged )
            {
                var statusService = new StepStatusService( ( RockContext ) this.Context );

                StepStatus status;

                status = statusService.Queryable().AsNoTracking().FirstOrDefault( x => x.Id == settings.FromStatusId );

                var fromStatus = ( status == null ) ? "[Any]" : status.Name;

                status = statusService.Queryable().AsNoTracking().FirstOrDefault( x => x.Id == settings.ToStatusId );

                var toStatus = ( status == null ) ? "[Any]" : status.Name;

                string description = string.Format( $"Status Change: {fromStatus} to {toStatus}" );

                return description;
            }
            else if ( triggerType == StepWorkflowTrigger.WorkflowTriggerCondition.IsComplete )
            {
                return "Completed";
            }
            else
            {
                return triggerType.ToString();
            }
        }

    }
}