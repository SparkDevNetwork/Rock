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
using System.Data.Entity;
using System.Linq;
using Rock.Data;
using Rock.Tasks;
using Rock.Web.Cache;

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
            private History.HistoryChangeList PersonHistoryChangeList { get; set; }

            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                PersonHistoryChangeList = new History.HistoryChangeList();
                var rockContext = ( RockContext ) this.RockContext;

                if ( Entity.Caption.IsNullOrWhiteSpace() )
                {
                    Entity.Caption = new StepTypeService( rockContext ).Get( Entity.StepTypeId ).Name;
                }

                switch ( State )
                {
                    case EntityContextState.Added:
                        {
                            var historyChange = PersonHistoryChangeList.AddChange( History.HistoryVerb.StepAdded, History.HistoryChangeType.Record, Entity.Caption );
                            var campusName = string.Empty;
                            if ( Entity.Campus != null )
                            {
                                campusName = Entity.Campus.Name;
                            }

                            if ( Entity.CampusId.HasValue && campusName.IsNullOrWhiteSpace() )
                            {
                                var campus = CampusCache.Get( Entity.CampusId.Value );
                                campusName = campus.Name;
                            }

                            if ( campusName.IsNotNullOrWhiteSpace() )
                            {
                                historyChange.SetRelatedData( campusName, null, null );
                            }
                            break;
                        }

                    case EntityContextState.Modified:
                        {
                            var originalStepStatusId = Entry.OriginalValues[nameof( Step.StepStatusId )].ToStringSafe().AsIntegerOrNull();
                            var stepStatusId = Entity.StepStatus != null ? Entity.StepStatus.Id : Entity.StepStatusId;
                            if ( !originalStepStatusId.Equals( stepStatusId ) )
                            {
                                string origStepStatus = History.GetValue<StepStatus>( null, originalStepStatusId, rockContext );
                                string stepStatus = History.GetValue<StepStatus>( Entity.StepStatus, stepStatusId, rockContext );
                                PersonHistoryChangeList.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Record, "Step Status" )
                                    .SetOldValue( origStepStatus )
                                    .SetNewValue( stepStatus );
                            }

                            var originalCampusId = Entry.OriginalValues[nameof( Step.CampusId )].ToStringSafe().AsIntegerOrNull();
                            var campusId = Entity.Campus != null ? Entity.Campus.Id : Entity.CampusId;
                            if ( !originalCampusId.Equals( campusId ) )
                            {
                                string origCampus = History.GetValue<Campus>( null, originalCampusId, rockContext );
                                string campus = History.GetValue<Campus>( Entity.Campus, campusId, rockContext );
                                PersonHistoryChangeList.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Record, "Campus" )
                                .SetOldValue( origCampus )
                                .SetNewValue( campus );
                            }

                            var originalStartDateTime = Entry.OriginalValues[nameof( Step.StartDateTime )].ToStringSafe().AsDateTime();
                            if ( Entity.StartDateTime != originalStartDateTime )
                            {
                                PersonHistoryChangeList.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Record, "Step Start" )
                                .SetOldValue( originalStartDateTime.ToShortDateString() )
                                .SetNewValue( Entity.StartDateTime.ToShortDateString() );
                            }

                            var originalEndDateTime = Entry.OriginalValues[nameof( Step.EndDateTime )].ToStringSafe().AsDateTime();
                            if ( Entity.EndDateTime != originalEndDateTime )
                            {
                                PersonHistoryChangeList.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Record, "Step End" )
                                .SetOldValue( originalEndDateTime.ToShortDateString() )
                                .SetNewValue( Entity.EndDateTime.ToShortDateString() );
                            }

                            var originalCompletedDateTime = Entry.OriginalValues[nameof( Step.CompletedDateTime )].ToStringSafe().AsDateTime();
                            if ( Entity.CompletedDateTime != originalCompletedDateTime )
                            {
                                PersonHistoryChangeList.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Record, "Completed" )
                                 .SetOldValue( originalCompletedDateTime.ToShortDateString() )
                                 .SetNewValue( Entity.CompletedDateTime.ToShortDateString() );
                            }

                            break;
                        }

                    case EntityContextState.Deleted:
                        {
                            PersonHistoryChangeList.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Step Type" ).SetOldValue( Entity.Caption );
                            break;
                        }
                }

                base.PreSave();
            }

            /// <summary>
            /// Called after the save operation has been executed
            /// </summary>
            protected override void PostSave()
            {
                int? previousStepStatusId = null;

                if ( State == EntityContextState.Modified )
                {
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
                }.SendWhen( this.DbContext.WrappedTransactionCompletedTask );

                var rockContext = ( RockContext ) this.RockContext;

                if ( PersonHistoryChangeList?.Any() == true )
                {
                    var personAlias = Entity.PersonAlias ?? new PersonAliasService( rockContext ).Get( Entity.PersonAliasId );
                    HistoryService.SaveChanges(
                                rockContext,
                                typeof( Person ),
                                Rock.SystemGuid.Category.HISTORY_PERSON_STEP.AsGuid(),
                                personAlias.PersonId,
                                PersonHistoryChangeList,
                                Entity.Caption,
                                typeof( Step ),
                                Entity.Id,
                                true,
                                Entity.ModifiedByPersonAliasId,
                                ( rockContext.SourceOfChange ) );
                }

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
