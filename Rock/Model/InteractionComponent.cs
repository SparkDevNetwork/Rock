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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents Component for <see cref="Rock.Model.Interaction">Interaction</see>
    /// </summary>
    [RockDomain( "Core" )]
    [NotAudited]
    [Table( "InteractionComponent" )]
    [DataContract]
    public partial class InteractionComponent : Model<InteractionComponent>, ICacheable
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the interaction component name.
        /// </summary>
        /// <value>
        /// The interaction component name.
        /// </value>
        [DataMember]
        [MaxLength( 250 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the interaction component data.
        /// </summary>
        /// <value>
        /// The interaction component data.
        /// </value>
        [DataMember]
        public string ComponentData { get; set; }

        /// <summary>
        /// Gets or sets the component summary.
        /// </summary>
        /// <value>
        /// The component summary.
        /// </value>
        [DataMember]
        public string ComponentSummary { get; set; }

        /// <summary>
        /// Gets or sets the Id of the entity that this interaction component is related to (determined by Channel.ComponentEntityType)
        /// For example:
        ///  if this is a Page View:
        ///     InteractionComponent.EntityId is the SiteId of the page that was viewed
        ///  if this is a Communication Recipient activity:
        ///     InteractionComponent.EntityId is the Communication.Id
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the entity (object) that this interaction component is related to.
        /// </value>
        [DataMember]
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.InteractionChannel"/> channel that that is associated with this Component.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.InteractionChannel"/> channel that this Component is associated with.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
        public int ChannelId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the channel.
        /// </summary>
        /// <value>
        /// The channel.
        /// </value>
        [DataMember]
        public virtual InteractionChannel Channel { get; set; }

        [NotMapped]
        private EntityState SaveState { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Method that will be called on an entity immediately before the item is saved by context
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="entry"></param>
        public override void PreSaveChanges( Data.DbContext dbContext, DbEntityEntry entry )
        {
            this.SaveState = entry.State;
            base.PreSaveChanges( dbContext, entry );
        }

        /// <summary>
        /// Method that will be called on an entity immediately after the item is saved by context
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public override void PostSaveChanges( Data.DbContext dbContext )
        {
            if ( this.SaveState == EntityState.Added || this.SaveState == EntityState.Deleted )
            {
                var channel = InteractionChannelCache.Get( this.ChannelId );
                if ( channel != null )
                {
                    if ( this.SaveState == EntityState.Added )
                    {
                        channel.AddComponentId( this.Id );
                    }
                    else
                    {
                        channel.RemoveComponentId( this.Id );
                    }
                }
            }

            base.PostSaveChanges( dbContext );
        }

        #endregion

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

    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class InteractionComponentConfiguration : EntityTypeConfiguration<InteractionComponent>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionComponentConfiguration"/> class.
        /// </summary>
        public InteractionComponentConfiguration()
        {
            this.HasRequired( r => r.Channel ).WithMany().HasForeignKey( r => r.ChannelId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
