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

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to display a list of key values that can be selected.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class KeyValueListFieldAttribute : ValueListFieldAttribute
    {
        private const string KEY_PROMPT_KEY = "keyprompt";
        private const string DISPLAY_VALUE_FIRST = "displayvaluefirst";

        /// <summary>
        /// Initializes a new instance of the <see cref="ValueListFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="keyPrompt">The text to display as a prompt in the key textbox.</param>
        /// <param name="valuePrompt">The text to display as a prompt in the label textbox.</param>
        /// <param name="definedTypeGuid">An Optional Defined Type Guid to select values from, otherwise values will be free-form text fields..</param>
        /// <param name="customValues">Optional list of options to use for the values.  Format is either 'value1,value2,value3,...', or 'value1:text1,value2:text2,value3:text3,...'.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        /// <param name="fieldTypeClass">The field type class.</param>
        /// <param name="displayValueFirst">if set to <c>true</c> [display value first].</param>
        public KeyValueListFieldAttribute( string name = "", string description = "", bool required = true, string defaultValue = "", 
            string keyPrompt = "", string valuePrompt = "", string definedTypeGuid = "", string customValues = "",
            string category = "", int order = 0, string key = null, string fieldTypeClass = null, bool displayValueFirst = false )
           : base( name, description, required, defaultValue, valuePrompt, definedTypeGuid, customValues, category, order, key, 
            typeof( Rock.Field.Types.KeyValueListFieldType ).FullName )
        {
            if ( !string.IsNullOrWhiteSpace( keyPrompt ) )
            {
                var configValue = new Field.ConfigurationValue( keyPrompt );
                FieldConfigurationValues.Add( KEY_PROMPT_KEY, configValue );
            }

            var displayValueFirstConfigValue = new Field.ConfigurationValue( displayValueFirst.ToString() );
            FieldConfigurationValues.Add( DISPLAY_VALUE_FIRST, displayValueFirstConfigValue );
        }
    }
}