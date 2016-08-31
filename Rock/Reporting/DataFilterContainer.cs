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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Extension;

namespace Rock.Reporting
{
    /// <summary>
    /// MEF Container class for Binary File DataFilter Components
    /// </summary>
    public class DataFilterContainer : Container<DataFilterComponent, IComponentData>
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<DataFilterContainer> instance =
            new Lazy<DataFilterContainer>( () => new DataFilterContainer() );

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static DataFilterContainer Instance
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static DataFilterComponent GetComponent( string entityType )
        {
            return Instance.GetComponentByEntity( entityType );
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static string GetComponentName( string entityType )
        {
            return Instance.GetComponentNameByEntity( entityType );
        }

        /// <summary>
        /// Gets a list of entity type names that have Data Filter components
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAvailableFilteredEntityTypeNames()
        {
            var entityTypeNames = new List<string>();

            foreach ( var serviceEntry in Instance.Components )
            {
                var component = serviceEntry.Value.Value;
                if ( !entityTypeNames.Contains( component.AppliesToEntityType ) )
                {
                    entityTypeNames.Add( component.AppliesToEntityType );
                }
            }

            return entityTypeNames;
        }

        /// <summary>
        /// Gets the components that are for filtering a given entity type name
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <returns></returns>
        public static List<DataFilterComponent> GetComponentsByFilteredEntityName( string entityTypeName )
        {
            return Instance.Components
                .Where( c =>
                    c.Value.Value.AppliesToEntityType == entityTypeName ||
                    string.IsNullOrEmpty( c.Value.Value.AppliesToEntityType ) )
                .Select( c => c.Value.Value )
                .OrderBy( c => c.Order )
                .ToList();
        }

        /// <summary>
        /// Gets or sets the MEF components.
        /// </summary>
        /// <value>
        /// The MEF components.
        /// </value>
        [ImportMany( typeof( DataFilterComponent ) )]
        protected override IEnumerable<Lazy<DataFilterComponent, IComponentData>> MEFComponents { get; set; }

    }
}
