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
using Rock.Attribute;
using Rock.Extension;

namespace Rock.IpAddress
{
    /// <summary>
    /// Internal IP Address Lookup Container
    /// </summary>
    [RockObsolete( "1.17" )]
    [Obsolete( "Use IpGeoLookup instead." )]
    public class IpAddressLookupContainer : Container<IpAddressLookupComponent, IComponentData>
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<IpAddressLookupContainer> instance =
            new Lazy<IpAddressLookupContainer>( () => new IpAddressLookupContainer() );

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static IpAddressLookupContainer Instance
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static IpAddressLookupComponent GetComponent( string entityType )
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
        /// Gets or sets the MEF components.
        /// </summary>
        /// <value>
        /// The MEF components.
        /// </value>
        [ImportMany( typeof( IpAddressLookupComponent ) )]
        protected override IEnumerable<Lazy<IpAddressLookupComponent, IComponentData>> MEFComponents { get; set; }
    }
}
