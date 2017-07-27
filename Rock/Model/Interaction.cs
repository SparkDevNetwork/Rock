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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{

    /// <summary>
    /// Represents a interaction <see cref="Rock.Model.Interaction"/>.
    /// </summary>
    [RockDomain( "Core" )]
    [NotAudited]
    [Table( "Interaction" )]
    [DataContract]
    public partial class Interaction : Model<Interaction>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the interaction datetime.
        /// </summary>
        /// <value>
        /// The interaction datetime.
        /// </value>
        [DataMember]
        public DateTime InteractionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the operation.
        /// </summary>
        /// <value>
        /// The operation.
        /// </value>
        [DataMember]
        [MaxLength( 25 )]
        public string Operation { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.InteractionComponent"/> Component that that is associated with this Interaction.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.InteractionComponent"/> component that this Interaction is associated with.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
        public int InteractionComponentId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the entity that this interaction component is related to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the entity (object) that this interaction component is related to.
        /// </value>
        [DataMember]
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.InteractionSession"/> Session that that is associated with this Interaction.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.InteractionSession"/> session that this Interaction is associated with.
        /// </value>
        [DataMember]
        public int? InteractionSessionId { get; set; }

        /// <summary>
        /// Gets or sets the interaction summary.
        /// </summary>
        /// <value>
        /// The interaction summary.
        /// </value>
        [DataMember]
        [MaxLength( 500 )]
        public string InteractionSummary { get; set; }

        /// <summary>
        /// Gets or sets the interaction data.
        /// </summary>
        /// <value>
        /// The interaction data.
        /// </value>
        [DataMember]
        public string InteractionData { get; set; }

        /// <summary>
        /// Gets or sets the personal device identifier.
        /// </summary>
        /// <value>
        /// The personal device identifier.
        /// </value>
        [DataMember]
        public int? PersonalDeviceId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the interaction componenent.
        /// </summary>
        /// <value>
        /// The interaction componenent.
        /// </value>
        [DataMember]
        public virtual InteractionComponent InteractionComponent { get; set; }

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the interaction componenent.
        /// </summary>
        /// <value>
        /// The interaction componenent.
        /// </value>
        [DataMember]
        public virtual InteractionSession InteractionSession { get; set; }

        /// <summary>
        /// Gets or sets the personal device.
        /// </summary>
        /// <value>
        /// The personal device.
        /// </value>
        [LavaInclude]
        public virtual PersonalDevice PersonalDevice { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the interaction details.
        /// </summary>
        /// <returns></returns>
        public string GetInteractionDetails()
        {
            var interaction = this;
            string interactionDetails = string.Empty;
            string deviceTypeDetails = $"{interaction.InteractionSession.DeviceType.OperatingSystem} {interaction.InteractionSession.DeviceType.DeviceTypeData} {interaction.InteractionSession.DeviceType.Application} {interaction.InteractionSession.DeviceType.ClientType}";
            if ( interaction.Operation == "Opened" )
            {
                interactionDetails = $"Opened from {interaction.InteractionSession.IpAddress} using {deviceTypeDetails}";
            }
            else if ( interaction.Operation == "Click" )
            {
                interactionDetails = $"Clicked the address {interaction.InteractionData} from {interaction.InteractionSession.IpAddress} using {deviceTypeDetails}";
            }
            else
            {
                interactionDetails = $"{interaction.Operation} using {deviceTypeDetails}"; 
            }

            return interactionDetails;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class InteractionConfiguration : EntityTypeConfiguration<Interaction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionConfiguration"/> class.
        /// </summary>
        public InteractionConfiguration()
        {
            this.HasOptional( r => r.PersonAlias ).WithMany().HasForeignKey( r => r.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.InteractionComponent ).WithMany().HasForeignKey( r => r.InteractionComponentId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.InteractionSession ).WithMany( r => r.Interactions ).HasForeignKey( r => r.InteractionSessionId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.PersonalDevice ).WithMany().HasForeignKey( r => r.PersonalDeviceId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
