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

namespace Rock.Storage
{
    /// <summary>
    /// MEF Container class for Binary File Storage Components
    /// </summary>
    public class ProviderContainer : Container<ProviderComponent, IComponentData>
    {
        /// <summary>
        /// The fully qualified class name of the default provider.
        /// </summary>
        private const string DEFAULT_COMPONENT_NAME = "Rock.Storage.Provider.Database, Rock";

        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<ProviderContainer> instance = new Lazy<ProviderContainer>( () => new ProviderContainer() );

        /// <summary>
        /// Prevents a default instance of the <see cref="ProviderContainer"/> class from being created.
        /// </summary>
        private ProviderContainer()
        {
            Refresh();
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static ProviderContainer Instance
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// Gets the default component.
        /// </summary>
        /// <value>
        /// The default component.
        /// </value>
        public static ProviderComponent DefaultComponent
        {
            get
            {
                return Instance.Components
                    .Select( serviceEntry => serviceEntry.Value.Value )
                    .Single( component => component.TypeName == DEFAULT_COMPONENT_NAME );
            }
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityTypeName">Name of the entity type.</param>
        /// <returns></returns>
        public static ProviderComponent GetComponent( string entityTypeName )
        {
            return Instance.Components
                .Select( serviceEntry => serviceEntry.Value.Value )
                .FirstOrDefault( component => component.TypeName == entityTypeName );
        }

        /// <summary>
        /// Gets or sets the MEF components.
        /// </summary>
        /// <value>
        /// The MEF components.
        /// </value>
        [ImportMany( typeof (ProviderComponent) )]
        protected override IEnumerable<Lazy<ProviderComponent, IComponentData>> MEFComponents { get; set; }
    }
}
