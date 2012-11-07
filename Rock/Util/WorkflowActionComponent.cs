//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

using Rock.Extension;

namespace Rock.Util
{
    /// <summary>
    /// Base class for components that perform actions for a workflow
    /// </summary>
    public abstract class WorkflowActionComponent : Component
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public abstract Boolean Execute( Action action );
    }
}