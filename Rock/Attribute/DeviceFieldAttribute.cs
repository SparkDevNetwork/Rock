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
    /// Field Attribute to select 0 or more DefinedValues for the given DefinedType Guid.
    /// Stored as either a single DefinedValue.Guid or a comma-delimited list of DefinedValue.Guids (if AllowMultiple)
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class DeviceFieldAttribute : FieldAttribute
    {
        private class AttributeKey
        {
            public const string DeviceType = "DeviceType";
        }

        /// <summary>
        /// Gets or sets the device type unique identifier.
        /// </summary>
        public string DeviceTypeGuid
        {
            get => FieldConfigurationValues.GetValueOrNull( AttributeKey.DeviceType );
            set => FieldConfigurationValues[AttributeKey.DeviceType] = new Field.ConfigurationValue( value );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeviceFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name of the field.</param>
        public DeviceFieldAttribute( string name )
            : base( name, fieldTypeClass: typeof( Rock.Field.Types.DeviceFieldType ).FullName )
        {
        }
    }
}
