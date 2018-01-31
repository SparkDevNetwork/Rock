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
using System.Linq;
using System.Runtime.Caching;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data access and service class for <see cref="Rock.Model.InteractionComponent"/> entity objects.
    /// </summary>
    public partial class InteractionComponentService
    {

        /// <summary>
        /// Gets components by channel identifier.
        /// </summary>
        /// <param name="channelId">The channel identifier.</param>
        /// <returns></returns>
        public IOrderedQueryable<InteractionComponent> GetByChannelId( int channelId )
        {
            return Queryable()
                .Where( c => c.ChannelId == channelId )
                .OrderBy( c => c.Name );
        }

        /// <summary>
        /// Gets the component by entity identifier, and creates it if it doesn't exist
        /// </summary>
        /// <param name="channelId">The channel identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public InteractionComponent GetComponentByEntityId( int channelId, int entityId, string name )
        {

            var component = this.Queryable()
                .FirstOrDefault( c =>
                    c.ChannelId == channelId &&
                    c.EntityId == entityId );
            if ( component != null )
            {
                component.Name = name;
            }
            else
            {
                component = new InteractionComponent();
                component.EntityId = entityId;
                component.ChannelId = channelId;
                component.Name = name;
                this.Add( component );
            }

            component.Name = name;

            return component;
        }

        /// <summary>
        /// Gets the component by component name, and creates it if it doesn't exist
        /// </summary>
        /// <param name="channelId">The channel identifier.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public InteractionComponent GetComponentByComponentName( int channelId, string name )
        {
            var component = this.Queryable()
                .FirstOrDefault( c =>
                    c.ChannelId == channelId &&
                    c.Name == name );
            if ( component != null )
            {
                component.Name = name;
            }
            else
            {
                component = new InteractionComponent();
                component.EntityId = null;
                component.ChannelId = channelId;
                component.Name = name;
                this.Add( component );
            }

            component.Name = name;

            return component;
        }

        /// <summary>
        /// Gets the component by entity identifier, and creates it if it doesn't exist
        /// </summary>
        /// <param name="channelGuid">The channel unique identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public InteractionComponent GetComponentByEntityId( Guid channelGuid, int entityId, string name )
        {
            var component = this.Queryable()
                .FirstOrDefault( c =>
                    c.Channel.Guid == channelGuid &&
                    c.EntityId == entityId );
            if ( component != null )
            {
                component.Name = name;
            }
            else
            {
                var channel = new InteractionChannelService( (RockContext)this.Context ).Get( channelGuid );
                if ( channel != null )
                {
                    component = new InteractionComponent();
                    component.EntityId = entityId;
                    component.ChannelId = channel.Id;
                    component.Name = name;
                    this.Add( component );
                }
            }

            component.Name = name;

            return component;
        }

    }
}
