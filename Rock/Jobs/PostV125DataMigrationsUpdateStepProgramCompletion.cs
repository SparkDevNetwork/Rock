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
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Populates Step Program Completion records using existing Step data
    /// </summary>
    [DisplayName( "Rock Update Helper v12.5 - Update Step Program Completion" )]
    [Description( "Populates Step Program Completion records using existing Step data" )]

    [IntegerField(
        "Command Timeout",
        Description = "Maximum amount of time (in seconds) to wait for the SQL Query to complete. Leave blank to use the default for this job (3600). Note, it could take several minutes, so you might want to set it at 3600 (60 minutes) or higher",
        IsRequired = false,
        DefaultIntegerValue = AttributeDefaults.CommandTimeout,
        Category = "General",
        Order = 1,
        Key = AttributeKey.CommandTimeout )]
    public class PostV125DataMigrationsUpdateStepProgramCompletion : RockJob
    {
        #region Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The command timeout
            /// </summary>
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <summary>
        /// Attribute value defaults
        /// </summary>
        private static class AttributeDefaults
        {
            /// <summary>
            /// The command timeout
            /// </summary>
            public const int CommandTimeout = 60 * 60;
        }

        #endregion Keys

        private const string _entitySetGuid = "495BF2AF-931B-495E-B88B-AE1C5E451C32";

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            // get the configured timeout, or default to 60 minutes if it is blank
            var commandTimeout = GetAttributeValue( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? AttributeDefaults.CommandTimeout;
            var isProcessingComplete = false;
            var batchSize = 2;
            var totalBatchSize = 0;
            var currentBatch = 1;

            totalBatchSize = new StepService( new RockContext() )
                .Queryable()
                .Where( a => a.CompletedDateTime.HasValue && !a.StepProgramCompletionId.HasValue )
                .Select( a => a.PersonAlias.PersonId )
                .Distinct()
                .Count();
            var runtime = System.Diagnostics.Stopwatch.StartNew();
            var lastProcessedPersonId = 0;
            while ( !isProcessingComplete )
            {
                using ( var rockContext = new RockContext() )
                {
                    var stepTypeService = new StepTypeService( rockContext );
                    var stepService = new StepService( rockContext );
                    var personQry = stepService
                        .Queryable()
                        .Where( a => a.CompletedDateTime.HasValue && !a.StepProgramCompletionId.HasValue && a.PersonAlias.PersonId > lastProcessedPersonId )
                        .Select( a => a.PersonAlias.PersonId )
                        .Distinct()
                        .OrderBy( a => a )
                        .Take( batchSize );

                    var stepProgramStepTypeMappings = stepTypeService
                        .Queryable()
                        .Where( a => a.IsActive )
                        .GroupBy( a => a.StepProgramId )
                        .ToDictionary( a => a.Key, b => b.Select( c => c.Id ).ToList() );

                    var steps = new StepService( rockContext )
                            .Queryable( "PersonAlias" )
                            .AsNoTracking()
                            .Where( a => personQry.Contains( a.PersonAlias.PersonId ) && !a.StepProgramCompletionId.HasValue && a.CompletedDateTime.HasValue )
                            .ToList();

                    isProcessingComplete = personQry.Count() < batchSize;
                    var batchPersonIds = personQry.ToList();

                    foreach ( var personId in batchPersonIds )
                    {
                        var personSteps = steps.Where( a => a.PersonAlias.PersonId == personId );
                        if ( !personSteps.Any() )
                        {
                            continue;
                        }

                        foreach ( var stepProgramId in stepProgramStepTypeMappings.Keys )
                        {
                            var stepTypeIds = stepProgramStepTypeMappings[stepProgramId];
                            var stepsByProgram = personSteps.Where( a => stepTypeIds.Contains( a.StepTypeId ) ).OrderBy( a => a.CompletedDateTime ).ToList();

                            if ( !stepsByProgram.Any() )
                            {
                                continue;
                            }

                            while ( stepsByProgram.Any() && stepTypeIds.All( a => stepsByProgram.Any( b => b.StepTypeId == a ) ) )
                            {
                                var stepSet = new List<Step>();
                                foreach ( var stepTypeId in stepTypeIds )
                                {
                                    var step = stepsByProgram.Where( a => a.StepTypeId == stepTypeId ).FirstOrDefault();
                                    if ( step == null )
                                    {
                                        continue;
                                    }

                                    stepSet.Add( step );
                                    stepsByProgram.RemoveAll( a => a.Id == step.Id );
                                }

                                var personAliasId = stepSet.Select( a => a.PersonAliasId ).FirstOrDefault();
                                StepService.UpdateStepProgramCompletion( stepSet, personAliasId, stepProgramId );
                            }
                        }
                        lastProcessedPersonId = personId;
                    }

                    var processTime = runtime.ElapsedMilliseconds;
                    var recordsProcessed = ( double ) ( batchSize * currentBatch ) + batchPersonIds.Count;
                    var recordsPerMillisecond = recordsProcessed / processTime;
                    var recordsRemaining = totalBatchSize - recordsProcessed;
                    var minutesRemaining = recordsRemaining / recordsPerMillisecond / 1000 / 60;
                    this.UpdateLastStatusMessage( $"Processing {recordsProcessed} of {totalBatchSize} records. Approximately {minutesRemaining:N0} minutes remaining." );
                    currentBatch++;
                }
            }

            ServiceJobService.DeleteJob( this.ServiceJobId );
        }
    }
}
