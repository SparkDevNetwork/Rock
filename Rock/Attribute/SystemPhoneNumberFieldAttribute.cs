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
    /// Field attribute to select 0 or more system phone numbers.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class SystemPhoneNumberFieldAttribute : FieldAttribute
    {
        private const string INCLUDE_INACTIVE_KEY = "includeInactive";
        private const string ALLOW_MULTIPLE_KEY = "allowMultiple";

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemPhoneNumberFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">If set to <c>true</c> then a selection will be required.</param>
        /// <param name="allowMultiple">If set to <c>true</c> then multiple values can be selected.</param>
        /// <param name="defaultValue">The default selection value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public SystemPhoneNumberFieldAttribute( string name, string description = "", bool required = true, bool allowMultiple = false, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.SystemPhoneNumberFieldType ).FullName )
        {
            var includeInactiveConfigValue = new Field.ConfigurationValue( "False" );
            FieldConfigurationValues.Add( INCLUDE_INACTIVE_KEY, includeInactiveConfigValue );

            var allowMultipleConfigKey = new Field.ConfigurationValue( allowMultiple.ToString() );
            FieldConfigurationValues.Add( ALLOW_MULTIPLE_KEY, allowMultipleConfigKey );
        }

        /// <summary>
        /// Gets or sets a value indicating if multiple items can be selected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if multiple items can be selected; otherwise, <c>false</c>.
        /// </value>
        public bool AllowMultiple
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( ALLOW_MULTIPLE_KEY ).AsBoolean();
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( ALLOW_MULTIPLE_KEY, new Field.ConfigurationValue( value.ToString() ) );
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if inactive items will be included.
        /// </summary>
        /// <value>
        ///   <c>true</c> if inactive items will be included; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeInactive
        {
            get
            {
                return FieldConfigurationValues.GetValueOrNull( INCLUDE_INACTIVE_KEY ).AsBoolean();
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( INCLUDE_INACTIVE_KEY, new Field.ConfigurationValue( value.ToString() ) );
            }
        }
    }
}