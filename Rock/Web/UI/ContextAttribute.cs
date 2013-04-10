//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;

namespace Rock.Web.UI
{
    /// <summary>
    /// Custom attribute used to decorate Rock Blocks that require context.  If entity type is not 
    /// included in the attribute, a block property will automatically be added for user to set 
    /// the entity type when block is placed on a page
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ContextAwareAttribute : System.Attribute
    {
        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        public string EntityType { get; set; }
        
        /// <summary>
        /// Gets the default name of the parameter.
        /// </summary>
        /// <value>
        /// The default name of the parameter.
        /// </value>
        public string DefaultParameterName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextAwareAttribute" /> class.
        /// </summary>
        public ContextAwareAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextAwareAttribute" /> class.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        public ContextAwareAttribute( Type entityType )
        {
            EntityType = entityType.FullName;
            DefaultParameterName = entityType.Name + "Id";
        }
    }
}