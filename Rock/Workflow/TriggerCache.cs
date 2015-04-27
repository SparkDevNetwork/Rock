// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.ComponentModel.Composition;
using Rock.Data;
using Rock.Model;

namespace Rock.Workflow
{
    /// <summary>
    /// MEF Container class for WorkflowAction Componenets
    /// </summary>
    public class TriggerCache
    {
        #region Static Fields

        // Locking object
        private static readonly Object obj = new object();

        // All workflow triggers grouped by EntityTypeName
        private static Dictionary<string, List<WorkflowTrigger>> EntityTriggers { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the <see cref="TriggerCache" /> class.
        /// </summary>
        static TriggerCache()
        {
            Refresh();
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        public static void Refresh()
        {
            lock ( obj )
            {
                EntityTriggers = new Dictionary<string, List<WorkflowTrigger>>();

                using ( var rockContext = new RockContext() )
                {
                    var service = new WorkflowTriggerService( rockContext );

                    foreach ( var trigger in service.Queryable() )
                    {
                        if ( !EntityTriggers.ContainsKey( trigger.EntityType.Name ) )
                        {
                            EntityTriggers.Add( trigger.EntityType.Name, new List<WorkflowTrigger>() );
                        }
                        EntityTriggers[trigger.EntityType.Name].Add( trigger.Clone() as WorkflowTrigger );
                    }
                }
            }
        }

        /// <summary>
        /// Triggerses the specified entity type name.
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <param name="triggerType">Type of the trigger.</param>
        /// <returns></returns>
        public static List<WorkflowTrigger> Triggers( string entityTypeName, WorkflowTriggerType triggerType )
        {
            var triggers = new List<WorkflowTrigger>();

            lock ( obj )
            {
                if ( EntityTriggers != null && EntityTriggers.ContainsKey( entityTypeName ) )
                {
                    foreach ( var trigger in EntityTriggers[entityTypeName] )
                    {
                        if ( trigger.WorkflowTriggerType == triggerType )
                        {
                            triggers.Add( trigger );
                        }
                    }
                }
            }

            return triggers;
        }

        #endregion

    }
}