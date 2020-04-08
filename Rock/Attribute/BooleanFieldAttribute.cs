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
using Rock.Field.Types;

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute for selecting either true or false.
    /// Stored as "True" or "False"
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class BooleanFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="defaultValue">if set to <c>true</c> [default value].</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key. (null means derive from name)</param>
        public BooleanFieldAttribute( string name, string description = "", bool defaultValue = false, string category = "", int order = 0, string key = null )
            : base( name, description, false, defaultValue.ToTrueFalse(), category, order, key, typeof( Rock.Field.Types.BooleanFieldType ).FullName )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="trueText">The true text.</param>
        /// <param name="falseText">The false text.</param>
        /// <param name="description">The description.</param>
        /// <param name="defaultValue">if set to <c>true</c> [default value].</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public BooleanFieldAttribute( string name, string trueText, string falseText, string description = "", bool defaultValue = false, string category = "", int order = 0, string key = null )
            : base( name, description, false, defaultValue.ToTrueFalse(), category, order, key, typeof( BooleanFieldType ).FullName )
        {
            TrueText = trueText;
            FalseText = falseText;
        }

        /// <summary>
        /// Gets or sets the default value of the attribute.  This is the value that will be used if a specific value has not yet been created
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public bool DefaultBooleanValue
        {
            get
            {
                return base.DefaultValue.AsBoolean();
            }

            set
            {
                base.DefaultValue = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the true text.
        /// </summary>
        /// <value>
        /// The true text.
        /// </value>
        public string TrueText
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( BooleanFieldType.ConfigurationKey.TrueText );
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( BooleanFieldType.ConfigurationKey.TrueText, new Field.ConfigurationValue( value ) );
            }
        }

        /// <summary>
        /// Gets or sets the false text.
        /// </summary>
        /// <value>
        /// The false text.
        /// </value>
        public string FalseText
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( BooleanFieldType.ConfigurationKey.FalseText );
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( BooleanFieldType.ConfigurationKey.FalseText, new Field.ConfigurationValue( value ) );
            }
        }

        /// <summary>
        /// Gets or sets the type of the control (DropDown, CheckBox, Toggle, or Switch) to use to edit the value
        /// </summary>
        /// <value>
        /// The type of the control.
        /// </value>
        public BooleanFieldType.BooleanControlType ControlType
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( BooleanFieldType.ConfigurationKey.BooleanControlType ).ConvertToEnumOrNull<BooleanFieldType.BooleanControlType>() ?? BooleanFieldType.BooleanControlType.DropDown;
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( BooleanFieldType.ConfigurationKey.BooleanControlType, new Field.ConfigurationValue( value.ConvertToString( false ) ) );
            }
        }
    }
}