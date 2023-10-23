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
using System.Data.SqlClient;
using System.Diagnostics;

namespace Rock.Tasks
{
    /// <summary>
    /// Updated the <see cref="Model.Workflow.WorkflowId"/> of all workflows of the specified WorkflowType with the new Prefix.
    /// </summary>
    public sealed class UpdateWorkflowIds : BusStartedTask<UpdateWorkflowIds.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            using ( var rockContext = new RockContext() )
            {
                var sql = $@"
DECLARE @batchId INT
DECLARE @batchSize INT
DECLARE @results INT
DECLARE @prefix varchar(10)
DECLARE @workflowTypeId INT

SET @results = 1
SET @batchSize = 10000
SET @batchId = 0
SET @prefix = '{message.Prefix}'
SET @workflowTypeId = {message.WorkflowTypeId}

-- when 0 rows returned, exit the loop
WHILE (@results > 0)
	BEGIN
		UPDATE Workflow
		SET WorkflowId = COALESCE( @Prefix + RIGHT( '00000' + CAST( Workflow.[WorkflowIdNumber] AS varchar(5) ), 5 ), '' )
		WHERE WorkflowTypeId = @WorkflowTypeId
		AND (Workflow.Id > @batchId
		AND Workflow.Id <= @batchId + @batchSize)
	
		SET @results = @@ROWCOUNT
	
		-- next batch
		SET @batchId = @batchId + @batchSize
	END
";
                var result = rockContext.Database.ExecuteSqlCommand( sql );
            }
        }

        /// <summary>
        /// Message that Identifies the updated WorkflowType and the new Prefix.
        /// </summary>
        /// <seealso cref="Rock.Tasks.BusStartedTaskMessage" />
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the workflow type identifier.
            /// </summary>
            /// <value>
            /// The workflow type identifier.
            /// </value>
            public int WorkflowTypeId { get; set; }

            /// <summary>
            /// Gets or sets the prefix.
            /// </summary>
            /// <value>
            /// The prefix.
            /// </value>
            public string Prefix { get; set; }
        }
    }
}
