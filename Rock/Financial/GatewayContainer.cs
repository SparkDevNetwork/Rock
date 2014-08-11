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

using Rock.Extension;

namespace Rock.Financial
{
    /// <summary>
    /// MEF Container class for Binary File Gateway Components
    /// </summary>
    public class GatewayContainer : Container<GatewayComponent, IComponentData>
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<GatewayContainer> instance =
            new Lazy<GatewayContainer>( () => new GatewayContainer() );

        /// <summary>
        /// Prevents a default instance of the <see cref="GatewayContainer"/> class from being created.
        /// </summary>
        private GatewayContainer()
        {
            Refresh();
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static GatewayContainer Instance
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static GatewayComponent GetComponent( string entityType )
        {
            foreach ( var serviceEntry in Instance.Components )
            {
                var component = serviceEntry.Value.Value;

                if ( component.TypeName.Equals( entityType, StringComparison.OrdinalIgnoreCase ) ||
                    component.TypeGuid.ToString().Equals( entityType, StringComparison.OrdinalIgnoreCase ) )
                {
                    if ( component.IsActive )
                    {
                        return component;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static string GetComponentName( string entityType )
        {
            foreach ( var serviceEntry in Instance.Components )
            {
                var component = serviceEntry.Value.Value;

                if ( component.TypeName.Equals( entityType, StringComparison.OrdinalIgnoreCase ) ||
                    component.TypeGuid.ToString().Equals( entityType, StringComparison.OrdinalIgnoreCase ) )
                {
                    if ( component.IsActive )
                    {
                        return serviceEntry.Value.Metadata.ComponentName;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets or sets the MEF components.
        /// </summary>
        /// <value>
        /// The MEF components.
        /// </value>
        [ImportMany( typeof (GatewayComponent) )]
        protected override IEnumerable<Lazy<GatewayComponent, IComponentData>> MEFComponents { get; set; }
    }
}
