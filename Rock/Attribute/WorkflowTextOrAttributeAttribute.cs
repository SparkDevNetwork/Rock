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
    /// Field Attribute to select a workflow attribute from a workflow type.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class WorkflowTextOrAttributeAttribute : FieldAttribute
    {
        private const string WORKFLOW_TYPE_KEY = "workflowtype";

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowAttributeAttribute" /> class.
        /// </summary>
        /// <param name="textLabel">The text label.</param>
        /// <param name="attributeLabel">The attribute label.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        /// <param name="workflowTypeGuid">The workflow type unique identifier.</param>
        public WorkflowTextOrAttributeAttribute( string textLabel, string attributeLabel, string description = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null, string workflowTypeGuid = "" )
            : base( textLabel + "|" + attributeLabel, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.WorkflowTextOrAttributeFieldType ).FullName )
        {
            if ( !string.IsNullOrWhiteSpace( workflowTypeGuid ) )
            {
                var workflowType = Rock.Web.Cache.DefinedTypeCache.Read( workflowTypeGuid.AsGuid() );
                if ( workflowType != null )
                {
                    var workflowTypeConfigValue = new Field.ConfigurationValue( workflowType.Id.ToString() );
                    FieldConfigurationValues.Add( WORKFLOW_TYPE_KEY, workflowTypeConfigValue );
                }
            }
        }
    }
}