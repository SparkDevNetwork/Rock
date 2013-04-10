//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to select a workflow type
    /// </summary>
    public class WorkflowTypeFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CampusFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultWorkflowTypeGuid">The default binary file type guid.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public WorkflowTypeFieldAttribute( string name = "Workflow", string description = "", bool required = false, string defaultWorkflowTypeGuid = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultWorkflowTypeGuid, category, order, key, typeof( Rock.Field.Types.WorkflowTypeFieldType ).FullName )
        {
        }
    }
}