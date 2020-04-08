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

namespace Rock.Web.HttpModules
{
    /// <summary>
    /// MEF Container class for HTTP Module Components
    /// </summary>
    public class HttpModuleContainer : Container<HttpModuleComponent, IComponentData>
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<HttpModuleContainer> instance =
            new Lazy<HttpModuleContainer>( () => new HttpModuleContainer() );

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static HttpModuleContainer Instance
        {
            get {
                if ( instance != null )
                {
                    return instance.Value;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static HttpModuleComponent GetComponent( string entityType )
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
        /// Gets the active component.
        /// </summary>
        /// <returns></returns>
        public static List<HttpModuleComponent> GetActiveComponents()
        {
            var activeComponents = new List<HttpModuleComponent>();

            foreach ( var indexType in HttpModuleContainer.Instance.Components )
            {
                var component = indexType.Value.Value;
                if ( component.IsActive )
                {
                    activeComponents.Add( component );
                }
            }

            return activeComponents.OrderBy( c => c.Order ).ToList();           ;
        }

        /// <summary>
        /// Gets or sets the MEF components.
        /// </summary>
        /// <value>
        /// The MEF components.
        /// </value>
        [ImportMany( typeof( HttpModuleComponent ) )]
        protected override IEnumerable<Lazy<HttpModuleComponent, IComponentData>> MEFComponents { get; set; }
    }
}
