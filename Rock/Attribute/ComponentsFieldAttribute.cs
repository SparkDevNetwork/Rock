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

using Rock.Web.Cache;

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to select multiple MEF components
    /// Stored as a comma-delimited list of EntityType.Guid
    /// </summary>
    public class ComponentsFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentFieldAttribute" /> class.
        /// </summary>
        /// <param name="mefContainerAssemblyName">Name of the mef container assembly.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public ComponentsFieldAttribute( string mefContainerAssemblyName, string name = "", string description = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.ComponentsFieldType ).FullName )
        {
            var configValue = new Field.ConfigurationValue( mefContainerAssemblyName );
            FieldConfigurationValues.Add( "container", configValue );

            if ( string.IsNullOrWhiteSpace( Name ) )
            {
                try
                {
                    Type containerType = Type.GetType( mefContainerAssemblyName );
                    var entityType = EntityTypeCache.Get( containerType );
                    if ( entityType != null )
                    {
                        Name = entityType.FriendlyName;
                    }
                }
                catch { }
            }

            if ( string.IsNullOrWhiteSpace( Name ) )
            {
                Name = mefContainerAssemblyName;
            }

            if ( string.IsNullOrWhiteSpace( Key ) )
            {
                Key = Name.Replace( " ", string.Empty );
            }
        }
    }
}