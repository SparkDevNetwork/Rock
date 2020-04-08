// <copyright>
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
using System.Linq;

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to select a workflow attribute from a workflow type.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class WorkflowTextOrAttributeAttribute : FieldAttribute
    {
        private const string ATTRIBUTE_FIELD_TYPES_KEY = "attributefieldtypes";
        private const string TEXTBOX_ROWS_KEY = "textboxRows";

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
        /// <param name="fieldTypeClassNames">The field type class names.</param>
        public WorkflowTextOrAttributeAttribute( string textLabel, string attributeLabel, string description, bool required, string defaultValue, string category, int order, string key, string[] fieldTypeClassNames )
            : this( textLabel, attributeLabel, description, required, defaultValue, category, order, key, fieldTypeClassNames, 1 )
        {
        }
        
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
        /// <param name="fieldTypeClassNames">The field type class names.</param>
        /// <param name="rows">The rows.</param>
        public WorkflowTextOrAttributeAttribute( string textLabel, string attributeLabel, string description = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null, string[] fieldTypeClassNames = null, int rows = 1 )
            : base( textLabel + "|" + attributeLabel, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.WorkflowTextOrAttributeFieldType ).FullName )
        {
            if ( fieldTypeClassNames != null && fieldTypeClassNames.Length > 0 )
            {
                var workflowTypeConfigValue = new Field.ConfigurationValue( fieldTypeClassNames.ToList().AsDelimited("|") );
                FieldConfigurationValues.Add( ATTRIBUTE_FIELD_TYPES_KEY, workflowTypeConfigValue );
            }

            if ( rows > 1 )
            {
                var rowsConfigValue = new Field.ConfigurationValue( rows.ToString() );
                FieldConfigurationValues.Add( TEXTBOX_ROWS_KEY, rowsConfigValue );
            }
        }
    }
}