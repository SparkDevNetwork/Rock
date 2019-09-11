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

namespace Rock.Security
{
    /// <summary>
    /// MEF Container class for Binary File BackgroundCheck Components
    /// </summary>
    public class BackgroundCheckContainer : Container<BackgroundCheckComponent, IComponentData>
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<BackgroundCheckContainer> instance =
            new Lazy<BackgroundCheckContainer>( () => new BackgroundCheckContainer() );

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static BackgroundCheckContainer Instance
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static BackgroundCheckComponent GetComponent( string entityType )
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
        public static BackgroundCheckComponent GetActiveComponent()
        {
            foreach ( var indexType in BackgroundCheckContainer.Instance.Components )
            {
                var component = indexType.Value.Value;
                if ( component.IsActive )
                {
                    return component;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets or sets the MEF components.
        /// </summary>
        /// <value>
        /// The MEF components.
        /// </value>
        [ImportMany( typeof( BackgroundCheckComponent ) )]
        protected override IEnumerable<Lazy<BackgroundCheckComponent, IComponentData>> MEFComponents { get; set; }

    }
}
