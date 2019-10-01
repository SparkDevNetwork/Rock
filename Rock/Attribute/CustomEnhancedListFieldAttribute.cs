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
    /// Field Attribute for selecting items from a checkbox list. Value is saved as a comma-delimited list
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class CustomEnhancedListFieldAttribute : FieldAttribute
    {
        private const string VALUES_KEY = "values";
        private const string ENHANCED_SELECTION_KEY = "enhancedselection";


        /// <summary>
        /// Initializes a new instance of the <see cref="CustomEnhancedListFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="listSource">The list source.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public CustomEnhancedListFieldAttribute( string name, string description, string listSource, bool required = false, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.SelectMultiFieldType ).FullName)
        {
            FieldConfigurationValues.Add( VALUES_KEY, new Field.ConfigurationValue( listSource ) );
            FieldConfigurationValues.Add( ENHANCED_SELECTION_KEY, new Field.ConfigurationValue( "True" ) );
        }
    }
}