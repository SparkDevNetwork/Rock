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
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Extension;

namespace Rock.Reporting
{
    /// <summary>
    /// MEF Container class for data selects
    /// </summary>
    public class DataSelectContainer : Container<DataSelectComponent, IComponentData>
    {
        /// <summary>
        /// The instance
        /// </summary>
        private static DataSelectContainer instance;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static DataSelectContainer Instance
        {
            get
            {
                if ( instance == null )
                {
                    instance = new DataSelectContainer();
                }

                return instance;
            }
        }

        /// <summary>
        /// Prevents a default instance of the <see cref="DataSelectContainer"/> class from being created.
        /// </summary>
        private DataSelectContainer()
        {
            Refresh();
        }

        /// <summary>
        /// Gets the available select entity type names.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAvailableSelectEntityTypeNames()
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
        /// Gets the component with the matching Entity Type Name
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <returns></returns>
        public static DataSelectComponent GetComponent( string entityTypeName )
        {
            foreach ( var serviceEntry in Instance.Components )
            {
                var component = serviceEntry.Value.Value;
                if ( component.TypeName == entityTypeName )
                {
                    return component;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the components that are for transformed a given entity type name
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <returns></returns>
        public static List<DataSelectComponent> GetComponentsBySelectedEntityTypeName( string entityTypeName )
        {
            return Instance.Components
                .Where( c => 
                    c.Value.Value.AppliesToEntityType == entityTypeName ||
                    string.IsNullOrEmpty(c.Value.Value.AppliesToEntityType)
                    )
                .Select( c => c.Value.Value )
                .OrderBy( c => c.Order )
                .ToList();
        }

        // MEF Import Definition
#pragma warning disable
        [ImportMany( typeof( DataSelectComponent ) )]
        protected override IEnumerable<Lazy<DataSelectComponent, IComponentData>> MEFComponents { get; set; }
#pragma warning restore
    }
}