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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{

    /// <summary>
    /// Represents a Interation Channel.
    /// </summary>
    [NotAudited]
    [Table( "InteractionChannel" )]
    [DataContract]
    public partial class InteractionChannel : Model<InteractionChannel>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the interaction channel name.
        /// </summary>
        /// <value>
        /// The interaction channel name.
        /// </value>
        [DataMember]
        [MaxLength( 250 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the channel data.
        /// </summary>
        /// <value>
        /// The channel data.
        /// </value>
        [DataMember]
        public string ChannelData { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of entity that was modified.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of the entity that was modified.
        /// </value>
        [DataMember]
        public int? ComponentEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of entity that was modified.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of the entity that was modified.
        /// </value>
        [DataMember]
        public int? InteractionEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the channel entity identifier.
        /// Note, the ChannelEntityType is inferred based on what the ChannelTypeMediumValue is 
        /// INTERACTIONCHANNELTYPE_WEBSITE = Rock.Model.Site
        /// INTERACTIONCHANNELTYPE_COMMUNICATION = Rock.Model.Communication
        /// INTERACTIONCHANNELTYPE_CONTENTCHANNEL = Rock.Model.ContentChannel
        /// </summary>
        /// <value>
        /// The channel entity identifier.
        /// </value>
        [DataMember]
        public int? ChannelEntityId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Channel Type <see cref="Rock.Model.DefinedValue" /> representing what type of Interaction Channel this is.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.DefinedValue"/> identifying the interaction channel type. If no value is selected this can be null.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.INTERACTION_CHANNEL_MEDIUM )]
        public int? ChannelTypeMediumValueId { get; set; }


        /// <summary>
        /// Gets or sets the retention days.
        /// </summary>
        /// <value>
        /// The retention days.
        /// </value>
        [DataMember]
        public int? RetentionDuration { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the Component Entity.
        /// </summary>
        /// <value>
        /// The type of the component entity.
        /// </value>
        [DataMember]
        public virtual Model.EntityType ComponentEntityType { get; set; }

        /// <summary>
        /// Gets or sets the type of the interaction Entity.
        /// </summary>
        /// <value>
        /// The type of the interaction entity.
        /// </value>
        [DataMember]
        public virtual Model.EntityType InteractionEntityType { get; set; }

        /// <summary>
        /// Gets or sets the channel medium value.
        /// </summary>
        /// <value>
        /// The channel medium value.
        /// </value>
        [DataMember]
        public virtual DefinedValue ChannelTypeMediumValue { get; set; }

        #endregion

        #region Public Methods


        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class InteractionChannelConfiguration : EntityTypeConfiguration<InteractionChannel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionChannelConfiguration"/> class.
        /// </summary>
        public InteractionChannelConfiguration()
        {
            this.HasOptional( r => r.ComponentEntityType ).WithMany( ).HasForeignKey( r => r.ComponentEntityTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.InteractionEntityType ).WithMany().HasForeignKey( r => r.InteractionEntityTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.ChannelTypeMediumValue ).WithMany().HasForeignKey( r => r.ChannelTypeMediumValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
