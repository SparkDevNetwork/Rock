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

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute for selecting an item from a drop down list.
    /// listSource Format is either 'value1,value2,value3,...', 'value1^text1,value2^text2,value3^text3,...', or a SQL Select statement that returns result set with a 'Value' and 'Text' column.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class CustomDropdownListFieldAttribute : FieldAttribute
    {
        private const string VALUES_KEY = "values";
        private const string FIELDTYPE_KEY = "fieldtype";

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomDropdownListFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="listSource">The source of the values to display in a list. Format is either 'value1,value2,value3,...', 'value1^text1,value2^text2,value3^text3,...', or a SQL Select statement that returns result set with a 'Value' and 'Text' column.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.  If multiple values are supported (i.e. checkbox) each value should be delimited by a comma</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public CustomDropdownListFieldAttribute( string name, string description, string listSource, bool required = false, string defaultValue = "", string category = "", int order = 0, string key = null )
            : this( name, description, listSource, false, required, defaultValue, category, order, key )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomDropdownListFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="listSource">The list source.</param>
        /// <param name="enhanced">if set to <c>true</c> [enhanced].</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public CustomDropdownListFieldAttribute( string name, string description, string listSource, bool enhanced, bool required, string defaultValue, string category, int order, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.SelectSingleFieldType ).FullName )
        {
            FieldConfigurationValues.Add( VALUES_KEY, new Field.ConfigurationValue( listSource ) );
            FieldConfigurationValues.Add( FIELDTYPE_KEY, new Field.ConfigurationValue( enhanced ? "ddl_enhanced" : "ddl" ) );
        }
    }
}