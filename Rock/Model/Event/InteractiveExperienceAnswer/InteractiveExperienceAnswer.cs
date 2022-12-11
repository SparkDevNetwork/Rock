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
using Rock.Enums.Event;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Interactive Experience Answer.
    /// </summary>
    [RockDomain( "Event" )]
    [Table( "InteractiveExperienceAnswer" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "D11DA9D4-8887-4EC2-B396-78556926DE89" )]
    public partial class InteractiveExperienceAnswer : Model<InteractiveExperienceAnswer>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.InteractiveExperienceOccurrence"/> that this Interactive Experience Answer is associated with. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.InteractiveExperienceOccurrence"/> that the Interactive Experience Answer is associated with.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int InteractiveExperienceOccurrenceId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.InteractiveExperienceAction"/> that this Interactive Experience Answer is associated with. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.InteractiveExperienceAction"/> that the Interactive Experience Answer is associated with.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int InteractiveExperienceActionId { get; set; }

        /// <summary>
        /// Gets or sets the response.
        /// </summary>
        /// <value>
        /// The response.
        /// </value>
        [DataMember]
        public string Response { get; set; }

        /// <summary>
        /// Gets or sets the response date time.
        /// </summary>
        /// <value>
        /// The response date time.
        /// </value>
        [DataMember]
        public DateTime ResponseDateTime { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.InteractionSession"/> Session.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.InteractionSession"/> session.
        /// </value>
        [DataMember]
        public int? InteractionSessionId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the <see cref="Campus"/> the answer
        /// originated from.
        /// </summary>
        /// <value>
        /// The identifier of the <see cref="Campus"/>.
        /// </value>
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the approval status.
        /// </summary>
        /// <value>
        /// The approval status.
        /// </value>
        [DataMember]
        public InteractiveExperienceApprovalStatus ApprovalStatus { get; set; }

        /// <summary>
        /// Gets or sets the custom response data JSON. This will hold additional
        /// information that does not need referential integrity.
        /// </summary>
        /// <value>
        /// The custom response data JSON.
        /// </value>
        [DataMember]
        public string ResponseDataJson { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.InteractiveExperienceOccurrence"/> that the Interactive Experience Answer belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.InteractiveExperienceOccurrence"/> representing the Interactive Experience Schedule that the Interactive Experience Answer is a part of.
        /// </value>
        [DataMember]
        public virtual InteractiveExperienceOccurrence InteractiveExperienceOccurrence { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.InteractiveExperienceAction"/> that the Interactive Experience Answer belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.InteractiveExperienceAction"/> representing the Interactive Experience Action that the Interactive Experience Answer is a part of.
        /// </value>
        [DataMember]
        public virtual InteractiveExperienceAction InteractiveExperienceAction { get; set; }

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="InteractionSession"/> that this answer
        /// is associated with.
        /// </summary>
        /// <value>
        /// The <see cref="InteractionSession"/> that this answer is associated with.</value>
        [DataMember]
        public virtual InteractionSession InteractionSession { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Campus"/> that this answer originated from.
        /// </summary>
        /// <value>
        /// The <see cref="Campus"/> that this answer originated from.
        /// </value>
        [DataMember]
        public virtual Campus Campus { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Interactive Experience Answer Configuration class.
    /// </summary>
    public partial class InteractiveExperienceAnswerConfiguration : EntityTypeConfiguration<InteractiveExperienceAnswer>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveExperienceAnswerConfiguration" /> class.
        /// </summary>
        public InteractiveExperienceAnswerConfiguration()
        {
            this.HasRequired( a => a.InteractiveExperienceOccurrence ).WithMany().HasForeignKey( a => a.InteractiveExperienceOccurrenceId ).WillCascadeOnDelete( false );
            this.HasRequired( a => a.InteractiveExperienceAction ).WithMany().HasForeignKey( a => a.InteractiveExperienceActionId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.PersonAlias ).WithMany().HasForeignKey( a => a.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.InteractionSession ).WithMany().HasForeignKey( a => a.InteractionSessionId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.Campus ).WithMany().HasForeignKey( a => a.CampusId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}