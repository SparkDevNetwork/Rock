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

using Rock.Data;
using Rock.Extension;
using Rock.Web.Cache;

namespace Rock.Follow
{
    /// <summary>
    /// MEF Container class for Binary File Event Components
    /// </summary>
    public class EventContainer : Container<EventComponent, IComponentData>
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<EventContainer> instance =
            new Lazy<EventContainer>( () => new EventContainer() );

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static EventContainer Instance
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static EventComponent GetComponent( string entityType )
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
        [ImportMany( typeof( EventComponent ) )]
        protected override IEnumerable<Lazy<EventComponent, IComponentData>> MEFComponents { get; set; }

        /// <inheritdoc/>
        public override void Refresh()
        {
            base.Refresh();

            // Load all the Attributes to the Following Event Type so that they may not be loaded every time by the Detail Block in the remote device.
            var FollowingEventTypeEntityType = EntityTypeCache.Get( "Rock.Model.FollowingEventType" );
            foreach (var component in MEFComponents )
            {
                var EventComponentEntityType = component.Value.EntityType;
                using ( var rockContext = new RockContext() )
                {
                    Rock.Attribute.Helper.UpdateAttributes( EventComponentEntityType.GetEntityType(), FollowingEventTypeEntityType.Id, "EntityTypeId", EventComponentEntityType.Id.ToString(), rockContext );
                }
            }
        }
    }
}
