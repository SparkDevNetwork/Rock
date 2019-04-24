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
    /// Field Attribute used to specify an EntityType
    /// Value returns EntityType.Guid
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class EntityTypeFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTypeFieldAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public EntityTypeFieldAttribute( string name, string description = "", bool required = true, string category = "", int order = 0, string key = null )
            : this(name, true, description, required, category, order, key)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTypeFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="includeGlobalAttributeOption">if set to <c>true</c> [include global attribute option].</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public EntityTypeFieldAttribute(string name, bool includeGlobalAttributeOption, string description = "", bool required = true, string category = "", int order = 0, string key = null)
            : base( name, description, required, "", category, order, key, typeof( Rock.Field.Types.EntityTypeFieldType ).FullName )
        {
            var configValue = new Field.ConfigurationValue( includeGlobalAttributeOption.ToString() );
            FieldConfigurationValues.Add( "includeglobal", configValue );
        }
    }
}