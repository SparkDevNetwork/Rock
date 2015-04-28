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
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to select 0 or more DefinedValues for the given DefinedType Guid.
    /// Stored as either a single DefinedValue.Guid or a comma-delimited list of DefinedValue.Guids (if AllowMultiple)
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class DefinedValueFieldAttribute : FieldAttribute
    {
        private const string DEFINED_TYPE_KEY = "definedtype";
        private const string ALLOW_MULTIPLE_KEY = "allowmultiple";

        /// <summary>
        /// Initializes a new instance of the <see cref="DefinedValueFieldAttribute" /> class.
        /// </summary>
        /// <param name="definedTypeGuid">The defined type GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="allowMultiple">if set to <c>true</c> [allow multiple].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public DefinedValueFieldAttribute( string definedTypeGuid, string name = "", string description = "", bool required = true, bool allowMultiple = false, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.DefinedValueFieldType ).FullName )
        {
            var definedType = Rock.Web.Cache.DefinedTypeCache.Read( new Guid( definedTypeGuid ) );
            if ( definedType != null )
            {
                var definedTypeConfigValue = new Field.ConfigurationValue( definedType.Id.ToString() );
                FieldConfigurationValues.Add( DEFINED_TYPE_KEY, definedTypeConfigValue );

                var allowMultipleConfigValue = new Field.ConfigurationValue( allowMultiple.ToString() );
                FieldConfigurationValues.Add( ALLOW_MULTIPLE_KEY, allowMultipleConfigValue );

                if ( string.IsNullOrWhiteSpace( Name ) )
                {
                    Name = definedType.Name;
                }

                if ( string.IsNullOrWhiteSpace( Key ) )
                {
                    Key = Name.Replace( " ", string.Empty );
                }
            }
        }
    }
}