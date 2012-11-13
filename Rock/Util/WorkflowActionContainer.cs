//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using Rock.Extension;

namespace Rock.Util
{
    /// <summary>
    /// MEF Container class for WorkflowAction Componenets
    /// </summary>
    public class WorkflowActionContainer : Container<WorkflowActionComponent, IComponentData>
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

        // MEF Import Definition
#pragma warning disable
        [ImportMany( typeof( WorkflowActionComponent ) )]
        protected override IEnumerable<Lazy<WorkflowActionComponent, IComponentData>> MEFComponents { get; set; }
#pragma warning restore
    }
}