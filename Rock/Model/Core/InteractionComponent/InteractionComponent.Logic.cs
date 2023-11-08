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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using Rock.Lava;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// InteractionComponent Logic
    /// </summary>
    public partial class InteractionComponent
    {

        [NotMapped]
        private EntityState SaveState { get; set; }

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return InteractionComponentCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            InteractionComponentCache.UpdateCachedEntity( this.Id, this.SaveState );
        }

        #endregion

        #region Obsolete Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.InteractionChannel"/> channel that that is associated with this Component.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.InteractionChannel"/> channel that this Component is associated with.
        /// </value>
        [LavaVisible]
        [NotMapped]
        [RockObsolete( "1.11" )]
        [Obsolete( "Use InteractionChannelId instead", false )]
        public int ChannelId
        {
            get { return InteractionChannelId; }
            set { InteractionChannelId = value; }
        }

        /// <summary>
        /// Gets or sets the channel.
        /// </summary>
        /// <value>
        /// The channel.
        /// </value>
        [LavaVisible]
        [NotMapped]
        [RockObsolete( "1.11" )]
        [Obsolete( "Use InteractionChannel instead", false )]
        public virtual InteractionChannel Channel
        {
            get { return InteractionChannel; }
            set { InteractionChannel = value; }
        }

        #endregion
    }
}
