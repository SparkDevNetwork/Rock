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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Transactions;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Job to trigger a workflow for each of the entities returned by the specified Data View.
    /// </summary>
    [DisplayName( "DataView to Workflow" )]
    [Category( "Workflows" )]
    [Description( "Starts a workflow for each entity in the specified Data View." )]

    #region Job Attributes

    [DataViewField( "Data View", "The data view to find entities from", required: true, key: AttributeKey.DataView )]
    [WorkflowTypeField( "Workflow", "The workflow to be fired for each entity", required: true, key: AttributeKey.Workflow )]

    #endregion

    public class DataViewToWorkflow : RockJob
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The unique identifier (GUID) of the Data View that will supply the list of entities.
            /// </summary>
            public const string DataView = "DataView";

            /// <summary>
            /// The unique identifier (Guid) of the Workflow that will be launched for each item in the list of entities.
            /// </summary>
            public const string Workflow = "Workflow";
        }

        #endregion

        /// <inheritdoc cref="RockJob.Execute()"/>
        public override void Execute()
        {
            // Get the configuration settings for this job instance.
            var workflowTypeGuid = GetAttributeValue( AttributeKey.Workflow ).AsGuidOrNull();
            var dataViewGuid = GetAttributeValue( AttributeKey.DataView ).AsGuidOrNull();

            if ( dataViewGuid == null )
            {
                throw new Exception( "Data view not selected" );
            }

            var dataView = DataViewCache.Get( dataViewGuid.Value );
            if ( dataView == null )
            {
                throw new Exception( "Data view not found" );
            }

            // Get the set of entity key values returned by the Data View.
            var entityTypeId = dataView.EntityTypeId.Value;

            var entityIds = dataView.GetEntityIds();

            // For each entity, create a new transaction to launch a workflow.
            var workflowsLaunched = 0;
            foreach ( var entityId in entityIds )
            {
                var transaction = new LaunchEntityWorkflowTransaction( workflowTypeGuid.Value, string.Empty, entityTypeId, entityId );

                transaction.Enqueue();

                workflowsLaunched++;
            }

            this.Result = string.Format( "{0} workflows launched", workflowsLaunched );
        }

        /// <summary>
        /// A transaction that launches a workflow for a specific entity.
        /// </summary>
        private class LaunchEntityWorkflowTransaction : LaunchWorkflowTransaction
        {
            private int _EntityTypeId;
            private int _EntityId;

            public LaunchEntityWorkflowTransaction( Guid workflowTypeGuid, string workflowName, int entityTypeId, int entityId )
                : base( workflowTypeGuid, workflowName )
            {
                _EntityTypeId = entityTypeId;
                _EntityId = entityId;
            }

            /// <summary>
            /// Retrieve the entity that is the target of the workflow.
            /// </summary>
            /// <returns></returns>
            public override IEntity GetEntity()
            {
                var entityType = EntityTypeCache.Get( _EntityTypeId );

                var systemType = entityType.GetEntityType();

                var dbContext = Reflection.GetDbContextForEntityType( systemType );

                var serviceInstance = Reflection.GetServiceForEntityType( systemType, dbContext );

                var getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( int ) } );

                var entity = getMethod.Invoke( serviceInstance, new object[] { _EntityId } );

                return entity as IEntity;
            }
        }
    }
}

