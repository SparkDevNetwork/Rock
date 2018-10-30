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
using System.Linq;

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
            return GetComponentByPredicate( channelId, entityId, name, c =>
                c.ChannelId == channelId &&
                c.EntityId == entityId );
        }

        /// <summary>
        /// Gets the component by component name, and creates it if it doesn't exist
        /// </summary>
        /// <param name="channelId">The channel identifier.</param>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public InteractionComponent GetComponentByComponentName( int channelId, string name )
        {
            return GetComponentByPredicate( channelId, null, name, c =>
                    c.ChannelId == channelId &&
                    c.Name == name );
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
            var channel = InteractionChannelCache.Get( channelGuid );
            if ( channel != null )
            {
                return GetComponentByEntityId( channel.Id, entityId, name );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the component by predicate.
        /// </summary>
        /// <param name="channelId">The channel identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="name">The name.</param>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        private InteractionComponent GetComponentByPredicate( int channelId, int? entityId, string name, System.Linq.Expressions.Expression<Func<InteractionComponent, bool>> predicate )
        {
            var component = this.Queryable()
                .FirstOrDefault( predicate );

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

            return component;
        }
    }
}
