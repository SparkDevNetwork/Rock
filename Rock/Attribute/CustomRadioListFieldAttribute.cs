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
    /// Field Attribute used to display a list of options as checkboxes.  Value is saved as a | delimited list
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class CustomRadioListFieldAttribute : SelectFieldAttribute
    {
        private const string VALUES = "values";
        private const string FIELDTYPE = "fieldtype";


        /// <summary>
        /// Initializes a new instance of the <see cref="CustomRadioListFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public CustomRadioListFieldAttribute( string name )
            : this( name, string.Empty, string.Empty )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomRadioListFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="listSource">The source of the values to display in a list.  Format is either 'value1,value2,value3,...', 'value1^text1,value2^text2,value3^text3,...', or a SQL Select statement that returns result set with a 'Value' and 'Text' column.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value. If multiple values are supported (i.e. checkbox) each value should be delimited by a comma</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public CustomRadioListFieldAttribute( string name, string description, string listSource, bool required = false, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.SelectSingleFieldType ).FullName )
        {
            FieldConfigurationValues.Add( VALUES, new Field.ConfigurationValue( listSource ) );
            FieldConfigurationValues.Add( FIELDTYPE, new Field.ConfigurationValue( "rb" ) );
        }

        /// <summary>
        /// The source of the values to display in a list. Format is either 'value1,value2,value3,...', 'value1^text1,value2^text2,value3^text3,...', or a SQL Select statement that returns result set with a 'Value' and 'Text' column.
        /// </summary>
        /// <value>
        /// The list source.
        /// </value>
        public string ListSource
        {
            get => FieldConfigurationValues.GetValueOrNull( VALUES );
            set => FieldConfigurationValues.AddOrReplace( VALUES, new Field.ConfigurationValue( value ) );
        }

        /// <summary>
        /// The default value. If multiple values are supported (i.e. CheckBox) each value should be delimited by a comma.
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public override string DefaultValue
        {
            get => base.DefaultValue;
            set => base.DefaultValue = value;
        }
    }


}