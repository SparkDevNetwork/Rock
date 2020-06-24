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

namespace Rock.Communication
{
    /// <summary>
    /// MEF Container class for Communication Medium Components
    /// </summary>
    public class MediumContainer : Container<MediumComponent, IComponentData>
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<MediumContainer> instance =
            new Lazy<MediumContainer>( () => new MediumContainer() );

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static MediumContainer Instance
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static MediumComponent GetComponent( string entityType )
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
        /// Gets the component by entity type identifier.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <returns></returns>
        public static MediumComponent GetComponentByEntityTypeId( int? entityTypeId )
        {
            if ( entityTypeId.HasValue )
            {
                foreach ( var serviceEntry in MediumContainer.Instance.Components )
                {
                    var component = serviceEntry.Value.Value;
                    if ( component.EntityType.Id == entityTypeId )
                    {
                        return component;
                    }
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
        [ImportMany( typeof( MediumComponent ) )]
        protected override IEnumerable<Lazy<MediumComponent, IComponentData>> MEFComponents { get; set; }

        /// <summary>
        /// Gets the medium components with active transports.
        /// </summary>
        /// <returns>A list of active MediumComponents</returns>
        public static IEnumerable<MediumComponent> GetActiveMediumComponentsWithActiveTransports()
        {
            return Instance.Components.Select( a => a.Value.Value ).Where( x => x.IsActive && x.Transport != null && x.Transport.IsActive );
        }

        /// <summary>
        /// Determines whether a transport is active for the specified unique identifier.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if an active transport exists for the specified unique identifier; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasActiveTransport( Guid guid )
        {
            return MediumContainer.GetActiveMediumComponentsWithActiveTransports().Any( a => a.EntityType.Guid == guid );
        }

        /// <summary>
        /// Determines whether an active SMS transport exists.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if an active SMS transport exists; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasActiveSmsTransport()
        {
            return MediumContainer.HasActiveTransport( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() );
        }

        /// <summary>
        /// Determines whether an active email transport exists.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if an active email transport exists; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasActiveEmailTransport()
        {
            return MediumContainer.HasActiveTransport( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() );
        }

        /// <summary>
        /// Determines whether an active push transport exists.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if an active push transport exists; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasActivePushTransport()
        {
            return MediumContainer.HasActiveTransport( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() );
        }
    }
}