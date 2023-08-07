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
using Rock.Data;
using Rock.Tasks;
using System.Data.Entity;
using System.Linq;

namespace Rock.Model
{
    public partial class WorkflowType
    {
        /// <summary>
        /// Save hook implementation for <see cref="WorkflowType"/>.
        /// </summary>
        /// <seealso cref="Rock.Data.EntitySaveHook{TEntity}" />
        internal class SaveHook : EntitySaveHook<WorkflowType>
        {
            /// <summary>
            /// Called before the save operation is executed.
            /// </summary>
            protected override void PreSave()
            {
                if ( State == EntityContextState.Deleted )
                {
                    // manually clear any registrations using this workflow type
                    var templates = new RegistrationTemplateService( RockContext ).Queryable().Where( r =>
                        r.RegistrationWorkflowTypeId.HasValue &&
                        r.RegistrationWorkflowTypeId.Value == this.Entity.Id );

                    foreach ( var template in templates )
                    {
                        template.RegistrationWorkflowTypeId = null;
                    }

                    var instances = new RegistrationInstanceService( RockContext ).Queryable().Where( r =>
                        r.RegistrationWorkflowTypeId.HasValue &&
                        r.RegistrationWorkflowTypeId.Value == this.Entity.Id );

                    foreach ( var instance in instances )
                    {
                        instance.RegistrationWorkflowTypeId = null;
                    }

                    /*
                        7/6/2020 - JH
                        The deletion of the Workflow object graph is accomplished by a combination of SQL cascade deletes and
                        PreSaveChanges() implementations.

                        When a WorkflowType is deleted, any child Workflows will be cascade-deleted. Most children of the deleted
                        Workflows will also be cascade-deleted, with the exception of any ConnectionRequestWorkflow records. Therefore,
                        we need to manually delete those here in order to prevent foreign key violations within the database.

                        Reason: GitHub Issue #4068
                        https://github.com/SparkDevNetwork/Rock/issues/4068#issuecomment-648908315
                    */
                    var connectionRequestWorkflows = new ConnectionRequestWorkflowService( RockContext )
                        .Queryable()
                        .Where( c => c.Workflow.WorkflowTypeId == this.Entity.Id );

                    if ( connectionRequestWorkflows.Any() )
                    {
                        DbContext.BulkDelete( connectionRequestWorkflows );
                    }
                }

                if ( Entry.State == EntityContextState.Modified )
                {
                    var workflowIdPrefix = Entry.OriginalValues[nameof( WorkflowIdPrefix )].ToStringSafe();
                    if ( workflowIdPrefix != Entity.WorkflowIdPrefix )
                    {
                        var message = new UpdateWorkflowIds.Message()
                        {
                            Prefix = Entity.WorkflowIdPrefix,
                            WorkflowTypeId = Entity.Id
                        };

                        message.SendWhen( DbContext.WrappedTransactionCompletedTask );
                    }
                }

                base.PreSave();
            }
        }
    }
}