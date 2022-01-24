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
        /// Gets or sets the Id of the entity that this interaction component is related to (determined by <see cref="Rock.Model.InteractionChannel.ComponentEntityTypeId"/> )
        /// <list type="bullet">
        /// <item>
        ///     <term>Page Views</term>
        ///     <description><see cref="Rock.Model.Site" /> Id</description></item>
        /// <item>
        ///     <term>Communication Recipient Activity</term>
        ///     <description><see cref="Rock.Model.Communication" /> Id</description></item>
        /// <item>
        ///     <term>Content Channel Activity</term>
        ///     <description><see cref="Rock.Model.ContentChannel" /> Id</description></item>
        /// <item>
        ///     <term>System Events, like Workflow Form Entry</term>
        ///     <description>Depends on <see cref="ContentChannelType"></see></description></item>
        /// </list>
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the entity (object) that this interaction component is related to.
        /// </value>
        [DataMember]
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.InteractionChannel"/> channel that is associated with this Component.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.InteractionChannel"/> channel that this Component is associated with.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
        public int InteractionChannelId { get; set; }

        /// <summary>
        /// Gets or sets the channel custom 1.
        /// </summary>
        /// <value>
        /// The channel custom 1.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string ChannelCustom1 { get; set; }

        /// <summary>
        /// Gets or sets the channel custom 2.
        /// </summary>
        /// <value>
        /// The channel custom 2.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string ChannelCustom2 { get; set; }

        /// <summary>
        /// Gets or sets the channel custom indexed 1.
        /// </summary>
        /// <value>
        /// The channel custom indexed 1.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string ChannelCustomIndexed1 { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the channel.
        /// </summary>
        /// <value>
        /// The channel.
        /// </value>
        [DataMember]
        public virtual InteractionChannel InteractionChannel { get; set; }

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
            this.HasRequired( r => r.InteractionChannel ).WithMany().HasForeignKey( r => r.InteractionChannelId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
