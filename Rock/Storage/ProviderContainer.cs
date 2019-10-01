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

using Rock.Extension;

namespace Rock.Storage
{
    /// <summary>
    /// MEF Container class for Binary File Provider Components
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
        private static readonly Lazy<ProviderContainer> instance =
            new Lazy<ProviderContainer>( () => new ProviderContainer() );

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
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static ProviderComponent GetComponent( string entityType )
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
        /// Gets the default component.
        /// </summary>
        /// <value>
        /// The default component.
        /// </value>
        public static ProviderComponent DefaultComponent
        {
            get
            {
                return GetComponent( DEFAULT_COMPONENT_NAME );
            }
        }
        
        /// <summary>
        /// Gets or sets the MEF components.
        /// </summary>
        /// <value>
        /// The MEF components.
        /// </value>
        [ImportMany( typeof( ProviderComponent ) )]
        protected override IEnumerable<Lazy<ProviderComponent, IComponentData>> MEFComponents { get; set; }

    }
}
