//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;

using Rock.Data;
using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Workflow
{
    /// <summary>
    /// Base class for components that perform actions for a workflow
    /// </summary>
    public abstract class ActionComponent : IComponent
    {
        /// <summary>
        /// Gets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public EntityTypeCache EntityType { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionComponent" /> class.
        /// </summary>
        public ActionComponent()
        {
            Type type = this.GetType();

            var ActionTypeEntityType = EntityTypeCache.Read( typeof( WorkflowActionType ).FullName );
            this.EntityType = EntityTypeCache.Read( type.FullName );

            using ( new Rock.Data.UnitOfWorkScope() )
            {
                Rock.Attribute.Helper.UpdateAttributes( type, ActionTypeEntityType.Id, "EntityTypeId", this.EntityType.Id.ToString(), null );
            }
        }

        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public abstract Boolean Execute( WorkflowAction action, IEntity entity, out List<string> errorMessages );

        /// <summary>
        /// Gets the attribute value for the action
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        protected string GetAttributeValue( WorkflowAction action, string key )
        {
            var values = action.ActionType.AttributeValues;
            if ( values.ContainsKey( key ) )
            {
                var keyValues = values[key];
                if ( keyValues.Count == 1 )
                {
                    return keyValues[0].Value;
                }
            }

            return string.Empty;
        }
    }
}