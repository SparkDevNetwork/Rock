﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
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
    public class WorkflowAttributeAttribute : FieldAttribute
    {
        private const string ATTRIBUTE_FIELD_TYPES_KEY = "attributefieldtypes";

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowAttributeAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        /// <param name="fieldTypeClassNames">The field type class names which are used to filter the pickable workflow attributes</param>
        public WorkflowAttributeAttribute( string name, string description = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null, string[] fieldTypeClassNames = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.WorkflowAttributeFieldType ).FullName )
        {
            if ( fieldTypeClassNames != null && fieldTypeClassNames.Length > 0 )
            {
                var workflowTypeConfigValue = new Field.ConfigurationValue( fieldTypeClassNames.ToList().AsDelimited( "|" ) );
                FieldConfigurationValues.Add( ATTRIBUTE_FIELD_TYPES_KEY, workflowTypeConfigValue );
            }
        }
    }
}