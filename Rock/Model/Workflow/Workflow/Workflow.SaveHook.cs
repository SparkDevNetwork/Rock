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
using Rock;
using Rock.Data;
using System.Data.Entity;
using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// Workflow SaveHook
    /// </summary>
    public partial class Workflow
    {
        /// <summary>
        /// Save hook implementation for <see cref="Workflow"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<Workflow>
        {
            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                var logEntries = this.Entity._logEntries;
                if ( RockContext != null )
                {
                    if ( logEntries?.Any() == true )
                    {
                        var workflowLogs = RockContext.Set<WorkflowLog>();

                        foreach ( var logEntry in logEntries )
                        {
                            workflowLogs.Add( new WorkflowLog { LogDateTime = logEntry.LogDateTime, LogText = logEntry.LogText, WorkflowId = this.Entity.Id } );
                        }

                        logEntries.Clear();
                    }

                    // Set the workflow number
                    if ( State == EntityContextState.Added )
                    {
                        // Get the next WorkflowIdNumber.
                        // Ordering by the indexed [Id] column significantly improves performance over using Max([WorkflowIdNumber]),
                        // However, be aware that this introduces the possibility of duplicate sequence numbers if the most recently added WorkflowIdNumber
                        // is set to a value less than the current maximum by some external process.
                        int maxNumber = new WorkflowService( RockContext )
                            .Queryable().AsNoTracking()
                            .Where( w => w.WorkflowTypeId == this.Entity.WorkflowTypeId )
                            .OrderByDescending( x => x.Id )
                            .Select( x => x.WorkflowIdNumber )
                            .FirstOrDefault();

                        this.Entity.WorkflowIdNumber = maxNumber + 1;
                        this.Entity.WorkflowId = $"{this.Entity.WorkflowTypeCache.WorkflowIdPrefix}{this.Entity.WorkflowIdNumber:00000}";
                    }

                    if ( State == EntityContextState.Deleted )
                    {
                        DeleteConnectionRequestWorkflows();
                    }
                }

                base.PreSave();
            }

            /// <summary>
            /// Deletes any connection request workflows tied to this workflow.
            /// </summary>
            private void DeleteConnectionRequestWorkflows()
            {
                var connectionRequestWorkflowService = new ConnectionRequestWorkflowService( RockContext );
                var connectionRequestWorkflows = connectionRequestWorkflowService.Queryable().Where( c => c.WorkflowId == this.Entity.Id );

                if ( connectionRequestWorkflows.Any() )
                {
                    RockContext.BulkDelete( connectionRequestWorkflows );
                }
            }
        }
    }
}

