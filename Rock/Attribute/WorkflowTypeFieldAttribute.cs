// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to select a workflow type. Stored as either a single WorkflowType Guid or a comma-delimited list of WorkflowType Guids
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class WorkflowTypeFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowTypeFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="allowMultiple">if set to <c>true</c> [allow multiple].</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultWorkflowTypeGuid">The default binary file type guid.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public WorkflowTypeFieldAttribute( string name = "Workflow", string description = "", bool allowMultiple = false, bool required = false, string defaultWorkflowTypeGuid = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultWorkflowTypeGuid, category, order, key, 
            allowMultiple ? 
                typeof( Rock.Field.Types.WorkflowTypesFieldType ).FullName : 
                typeof( Rock.Field.Types.WorkflowTypeFieldType ).FullName )
        {
        }
    }
}