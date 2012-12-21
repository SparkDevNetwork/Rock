//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

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

                var service = new WorkflowTriggerService();

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