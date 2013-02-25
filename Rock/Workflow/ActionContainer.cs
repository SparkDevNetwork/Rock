//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Extension;

namespace Rock.Workflow
{
    /// <summary>
    /// MEF Container class for WorkflowAction Componenets
    /// </summary>
    public class WorkflowActionContainer : ContainerManaged<ActionComponent, IComponentData>
    {
        private static WorkflowActionContainer instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static WorkflowActionContainer Instance
        {
            get
            {
                if ( instance == null )
                    instance = new WorkflowActionContainer();
                return instance;
            }
        }

        private WorkflowActionContainer()
        {
            Refresh();
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <returns></returns>
        public static ActionComponent GetComponent( string entityTypeName )
        {
            foreach ( var serviceEntry in Instance.Components )
            {
                var component = serviceEntry.Value.Value;
                if ( component.TypeName == entityTypeName )
                {
                    return component;
                }
            }

            return null;
        }

        // MEF Import Definition
#pragma warning disable
        [ImportMany( typeof( ActionComponent ) )]
        protected override IEnumerable<Lazy<ActionComponent, IComponentData>> MEFComponents { get; set; }
#pragma warning restore
    }
}