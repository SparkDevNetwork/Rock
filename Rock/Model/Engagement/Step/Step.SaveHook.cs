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
using System.Data.Entity;
using System.Linq;
using Rock.Data;
using Rock.Tasks;

namespace Rock.Model
{
    public partial class Step
    {
        /// <summary>
        /// Save hook implementation for <see cref="Step"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<Step>
        {
            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                int? previousStepStatusId = null;

                if ( State == EntityContextState.Modified )
                {
                    var dbProperty = Entity.GetPropertyValue( nameof( StepStatusId ) );
                    previousStepStatusId = OriginalValues[nameof( StepStatusId )] as int?;
                }

                // Send a task to process workflows associated with changes to this Step.
                new LaunchStepChangeWorkflows.Message
                {
                    EntityGuid = Entity.Guid,
                    EntityContextState = State,
                    StepTypeId = Entity.StepTypeId,
                    CurrentStepStatusId = Entity.StepStatusId,
                    PreviousStepStatusId = previousStepStatusId
                }.Send();

                base.PreSave();
            }

            /// <summary>
            /// Called after the save operation has been executed
            /// </summary>
            protected override void PostSave()
            {
                var rockContext = ( RockContext ) this.RockContext;
                base.PostSave();

                UpdateStepProgramCompletion( rockContext );
            }

            private void UpdateStepProgramCompletion( RockContext rockContext )
            {
                if ( !Entity.CompletedDateTime.HasValue )
                {
                    return;
                }

                var stepTypeService = new StepTypeService( rockContext );
                var stepType = Entity.StepType ?? stepTypeService.Get( Entity.StepTypeId );

                if ( stepType == null )
                {
                    return;
                }

                var programStepTypeIds = stepTypeService
                    .Queryable()
                    .Where( a => a.StepProgramId == stepType.StepProgramId && a.IsActive )
                    .Select( a => a.Id )
                    .ToList();

                var steps = new StepService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Where( a => a.PersonAliasId == Entity.PersonAliasId && !a.StepProgramCompletionId.HasValue && a.CompletedDateTime.HasValue )
                    .OrderBy( a => a.CompletedDateTime )
                    .ToList();

                while ( steps.Any() && programStepTypeIds.All( a => steps.Any( b => b.StepTypeId == a ) ) )
                {
                    var stepSet = new List<Step>();
                    foreach ( var programStepTypeId in programStepTypeIds )
                    {
                        var step = steps.Where( a => a.StepTypeId == programStepTypeId ).FirstOrDefault();
                        if ( step == null )
                        {
                            continue;
                        }

                        stepSet.Add( step );
                        steps.RemoveAll( a => a.Id == step.Id );
                    }

                    StepService.UpdateStepProgramCompletion( stepSet, Entity.PersonAliasId, stepType.StepProgramId, rockContext );
                }
            }
        }
    }
}
