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

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute for selecting checkbox options from an enum. Value is stored as a comma delimited list of the numeric value of the Enums.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class EnumsFieldAttribute : SelectFieldAttribute
    {
        private const string VALUES = "values";
        private const string ENUM_SOURCE_TYPE_KEY = "enumSourceTypeKey";

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public EnumsFieldAttribute( string name )
            : base( name )
        {
            FieldTypeClass = typeof( Rock.Field.Types.SelectMultiFieldType ).FullName;
        }

        /// <summary>
        /// Gets or sets the type of the enum source.
        /// FieldConfigurationValues["values"] is populated the a comma seperated string of the enum values
        /// </summary>
        /// <value>
        /// The type of the enum source.
        /// </value>
        public Type EnumSourceType
        {
            get
            {
                string entityTypeName = FieldConfigurationValues.GetValueOrNull( ENUM_SOURCE_TYPE_KEY ) ?? string.Empty;
                return entityTypeName.IsNotNullOrWhiteSpace() ? Type.GetType( entityTypeName ) : null;
            }

            set
            {
                FieldConfigurationValues.AddOrReplace( ENUM_SOURCE_TYPE_KEY, new Field.ConfigurationValue( value.FullName ) );

                var list = new List<string>();
                foreach ( var enumValue in Enum.GetValues( value ) )
                {
                    list.Add( string.Format( "{0}^{1}", ( int ) enumValue, enumValue.ToString().SplitCase() ) );
                }

                var listSource = string.Join( ",", list );
                FieldConfigurationValues.Add( VALUES, new Field.ConfigurationValue( listSource ) );
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="enumSourceType">Type of the enum source.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.  If multiple values are supported (i.e. checkbox) each value should be delimited by a comma</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public EnumsFieldAttribute( string name, string description, Type enumSourceType, bool required = false, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.SelectMultiFieldType ).FullName )
        {
            EnumSourceType = enumSourceType;
        }
    }
}