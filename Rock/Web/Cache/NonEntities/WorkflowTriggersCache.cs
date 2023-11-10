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
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// MEF Container class for WorkflowAction Components
    /// </summary>
    [Serializable]
    [DataContract]
    public class WorkflowTriggersCache : ItemCache<WorkflowTriggersCache>
    {
        private const string KEY = "AllWorkflowTriggers";

        #region Constructors

        /// <summary>
        /// Use Static Get() method to instantiate a new Global Attributes object
        /// </summary>
        private WorkflowTriggersCache()
        {
        }

        #endregion

        /// <summary>
        /// Gets or sets the entity triggers.
        /// </summary>
        /// <value>
        /// The entity triggers.
        /// </value>
        [DataMember]
        public Dictionary<string, List<WorkflowTrigger>> EntityTriggers { get; set; }

        #region Public Methods

        /// <summary>
        /// Gets this instance.
        /// </summary>
        /// <returns></returns>
        public static WorkflowTriggersCache Get()
        {
            return Get( null );
        }

        /// <summary>
        /// Gets the specified rock context.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static WorkflowTriggersCache Get( RockContext rockContext )
        {
            return GetOrAddExisting( KEY, () => QueryDb( rockContext ) );
        }

        /// <summary>
        /// Gets a collection of Workflow Triggers for the specified criteria.
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <returns></returns>
        public static List<WorkflowTrigger> Triggers( string entityTypeName )
        {
            var triggers = Get();

            if ( triggers == null ) return new List<WorkflowTrigger>();

            if ( triggers.EntityTriggers != null && triggers.EntityTriggers.ContainsKey( entityTypeName ) )
            {
                return triggers.EntityTriggers[entityTypeName];
            }

            return new List<WorkflowTrigger>();
        }

        /// <summary>
        /// Gets a collection of Workflow Triggers for the specified criteria.
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <param name="triggerType">Type of the trigger.</param>
        /// <returns></returns>
        public static List<WorkflowTrigger> Triggers( string entityTypeName, WorkflowTriggerType triggerType )
        {
            var triggers = Get();

            if ( triggers == null ) return new List<WorkflowTrigger>();

            if ( triggers.EntityTriggers != null && triggers.EntityTriggers.ContainsKey( entityTypeName ) )
            {
                return triggers.EntityTriggers[entityTypeName]
                    .Where( t => t.WorkflowTriggerType == triggerType )
                    .ToList();
            }

            return new List<WorkflowTrigger>();
        }

        /// <summary>
        /// Removes this instance.
        /// </summary>
        public static void Remove()
        {
            Remove( KEY );
        }

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        public static void Refresh()
        {
            Remove();
            Get();
        }

        #endregion

        #region Private Methods 

        private static WorkflowTriggersCache QueryDb( RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return QueryDbWithContext( rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return QueryDbWithContext( rockContext2 );
            }
        }

        private static WorkflowTriggersCache QueryDbWithContext( RockContext rockContext )
        {
            var workflowTriggers = new WorkflowTriggerService( rockContext )
                .Queryable().AsNoTracking()
                .ToList()
                .GroupBy( t => t.EntityType.Name )
                .Select( i => new
                {
                    EntityTypeId = i.Key,
                    Triggers = i.Select( v => v.Clone( false ) ).ToList()
                } )
                .ToDictionary( k => k.EntityTypeId, v => v.Triggers );

            var value = new WorkflowTriggersCache { EntityTriggers = workflowTriggers };
            return value;
        }

        #endregion

    }
}