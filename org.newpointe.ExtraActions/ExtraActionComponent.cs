using System;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Workflow;

namespace org.newpointe.ExtraActions
{
    public abstract class ExtraActionComponent : ActionComponent
    {
        /// <summary>
        /// Gets the entity from the specified service using the guid directly from an Attribute or from a Workflow Attribute.
        /// </summary>
        /// <typeparam name="T">The type of entity to get.</typeparam>
        /// <param name="action">The workflow action to look at.</param>
        /// <param name="directAttributeKey">The key of the Attribute that points directly to the entity.</param>
        /// <param name="workflowAttributeKey">The key of the Attribute that points to a Workflow Attribute that points to the entity.</param>
        /// <param name="entityService">The service to use to get the entity.</param>
        /// <returns></returns>
        public T GetEntityFromWorkflowAttributes<T>( WorkflowAction action, string directAttributeKey, string workflowAttributeKey, Service<T> entityService ) where T : Entity<T>, new()
        {
            Guid? entityAttributeGuid = GetAttributeValue( action, workflowAttributeKey ).AsGuidOrNull();
            return entityService.Get( ( entityAttributeGuid.HasValue ? action.GetWorklowAttributeValue( entityAttributeGuid.Value ).AsGuidOrNull() : null ) ?? GetAttributeValue( action, directAttributeKey ).AsGuid() );
        }
    }
}
