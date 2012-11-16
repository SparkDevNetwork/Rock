//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace Rock.Util
{
    /// <summary>
    /// MEF Container class for WorkflowAction Componenets
    /// </summary>
    public class WorkflowTriggerCache
    {
        private static WorkflowTriggerCache instance;

        private Dictionary<string, List<WorkflowTriggerDto>> EntityTriggers { get; set; }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static WorkflowTriggerCache Instance
        {
            get
            {
                if ( instance == null )
                    instance = new WorkflowTriggerCache();
                return instance;
            }
        }

        private WorkflowTriggerCache()
        {
            Refresh();
        }

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        public void Refresh()
        {
            EntityTriggers = new Dictionary<string, List<WorkflowTriggerDto>>();

            var service = new WorkflowTriggerService();

            foreach ( var trigger in service.Queryable() )
            {
                if ( !EntityTriggers.ContainsKey( trigger.EntityType.Name ) )
                {
                    EntityTriggers.Add( trigger.EntityType.Name, new List<WorkflowTriggerDto>() );
                }
                EntityTriggers[trigger.EntityType.Name].Add( trigger.ToDto() );
            }

        }

        /// <summary>
        /// Triggerses the specified entity type name.
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <param name="triggerType">Type of the trigger.</param>
        /// <returns></returns>
        public List<WorkflowTriggerDto> Triggers( string entityTypeName, WorkflowTriggerType triggerType )
        {
            var triggers = new List<WorkflowTriggerDto>();

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

            return triggers;
        }

    }
}