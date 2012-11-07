//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

using Rock.Extension;
using Rock.Web.Cache;

namespace Rock.Util
{
    /// <summary>
    /// Base class for components that perform actions for a workflow
    /// </summary>
    public abstract class WorkflowActionComponent : IComponent
    {
        /// <summary>
        /// Gets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public EntityTypeCache EntityType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowActionComponent" /> class.
        /// </summary>
        public WorkflowActionComponent()
        {
            Type type = this.GetType();

            var ActionTypeEntityType = EntityTypeCache.Read( typeof( Rock.Util.ActionType ).FullName );
            this.EntityType = EntityTypeCache.Read( type.FullName );
            Rock.Attribute.Helper.UpdateAttributes( type, ActionTypeEntityType.Id, "EntityTypeId", this.EntityType.Id.ToString(), null );
        }

        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns></returns>
        public abstract Boolean Execute( Action action );
    }
}