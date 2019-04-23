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

using Rock.Web.Cache;

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to display a list of values that can be selected.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class ValueListFieldAttribute : FieldAttribute
    {
        private const string VALUE_PROMPT_KEY = "valueprompt";
        private const string DEFINED_TYPE_KEY = "definedtype";
        private const string CUSTOM_VALUES = "customvalues";
        private const string ALLOW_HTML = "allowhtml";

        /* Developer Note: When adding new params to a Field Attribute, we could just add new Properties instead to avoid backwards compatibility issues. 
         * See AllowHtml below as an example, and the GroupList block for how to initialize it
         */

        /// <summary>
        /// Sets a value indicating whether [allow HTML].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow HTML]; otherwise, <c>false</c>.
        /// </value>
        public bool AllowHtml
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( ALLOW_HTML ).AsBoolean();
            }
            set
            {
                FieldConfigurationValues.AddOrReplace( ALLOW_HTML, new Field.ConfigurationValue( value.ToString() ) );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueListFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="valuePrompt">The text to display as a prompt in the label textbox.</param>
        /// <param name="definedTypeGuid">An Optional Defined Type Guid to select values from, otherwise values will be free-form text fields..</param>
        /// <param name="customValues">Optional list of options to use for the values. Format is either 'value1,value2,value3,...', 'value1^text1,value2^text2,value3^text3,...', or a SQL Select statement that returns result set with a 'Value' and 'Text' column.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        /// <param name="fieldTypeClass">The field type class.</param>
        internal ValueListFieldAttribute( string name = "", string description = "", bool required = true, string defaultValue = "", string valuePrompt = "", 
            string definedTypeGuid = "", string customValues = "", string category = "", int order = 0, string key = null, string fieldTypeClass = null )
            : base( name, description, required, defaultValue, category, order, key, fieldTypeClass )
         {
            if ( !string.IsNullOrWhiteSpace( valuePrompt ) )
            {
                var configValue = new Field.ConfigurationValue( valuePrompt );
                FieldConfigurationValues.Add( VALUE_PROMPT_KEY, configValue );
            }

            Guid? guid = definedTypeGuid.AsGuidOrNull();
            if ( guid.HasValue )
            {
                var definedType = DefinedTypeCache.Get( guid.Value );
                if ( definedType != null )
                {
                    var definedTypeConfigValue = new Field.ConfigurationValue( definedType.Id.ToString() );
                    FieldConfigurationValues.Add( DEFINED_TYPE_KEY, definedTypeConfigValue );
                }
            }

            if ( !string.IsNullOrWhiteSpace( customValues ) )
            {
                var configValue = new Field.ConfigurationValue( customValues );
                FieldConfigurationValues.Add( CUSTOM_VALUES, configValue );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedValueFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="valuePrompt">The text to display as a prompt in the label textbox.</param>
        /// <param name="definedTypeGuid">An Optional Defined Type Guid to select values from, otherwise values will be free-form text fields..</param>
        /// <param name="customValues">Optional list of options to use for the values.  Format is either 'value1,value2,value3,...', or 'value1^text1,value2^text2,value3^text3,...'.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public ValueListFieldAttribute( string name = "", string description = "", bool required = true, string defaultValue = "",
            string valuePrompt = "", string definedTypeGuid = "", string customValues = "", string category = "", int order = 0, string key = null )
           : this( name, description, required, defaultValue, valuePrompt, definedTypeGuid, customValues, category, order, key, 
            typeof( Rock.Field.Types.ValueListFieldType ).FullName )
        {
        }
    }
}