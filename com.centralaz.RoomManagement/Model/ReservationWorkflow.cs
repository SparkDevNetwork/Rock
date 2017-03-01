// <copyright>
// Copyright by the Central Christian Church
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
using Rock.Model;

namespace com.centralaz.RoomManagement.Model
{
    /// <summary>
    /// A Reservation Workflow
    /// </summary>
    /// <seealso cref="Rock.Data.Model{com.centralaz.RoomManagement.Model.ReservationWorkflow}" />
    /// <seealso cref="Rock.Data.IRockEntity" />
    [Table( "_com_centralaz_RoomManagement_ReservationWorkflow" )]
    [DataContract]
    public class ReservationWorkflow : Rock.Data.Model<ReservationWorkflow>, Rock.Data.IRockEntity
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the reservation identifier.
        /// </summary>
        /// <value>
        /// The reservation identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int ReservationId { get; set; }

        /// <summary>
        /// Gets or sets the reservation workflow trigger identifier.
        /// </summary>
        /// <value>
        /// The reservation workflow trigger identifier.
        /// </value>
        [Required]
        [DataMember]
        public int ReservationWorkflowTriggerId { get; set; }

        /// <summary>
        /// Gets or sets the workflow identifier.
        /// </summary>
        /// <value>
        /// The workflow identifier.
        /// </value>
        [Required]
        [DataMember]
        public int WorkflowId { get; set; }

        /// <summary>
        /// Gets or sets the type of the trigger.
        /// </summary>
        /// <value>
        /// The type of the trigger.
        /// </value>
        [DataMember]
        public ReservationWorkflowTriggerType TriggerType { get; set; }

        /// <summary>
        /// Gets or sets the trigger qualifier.
        /// </summary>
        /// <value>
        /// The trigger qualifier.
        /// </value>
        [DataMember]
        public String TriggerQualifier { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the reservation.
        /// </summary>
        /// <value>
        /// The reservation.
        /// </value>
        public virtual Reservation Reservation { get; set; }

        /// <summary>
        /// Gets or sets the reservation workflow trigger.
        /// </summary>
        /// <value>
        /// The reservation workflow trigger.
        /// </value>
        public virtual ReservationWorkflowTrigger ReservationWorkflowTrigger { get; set; }

        /// <summary>
        /// Gets or sets the workflow.
        /// </summary>
        /// <value>
        /// The workflow.
        /// </value>
        public virtual Workflow Workflow { get; set; }

        #endregion

    }

    #region Entity Configuration


    public partial class ReservationWorkflowConfiguration : EntityTypeConfiguration<ReservationWorkflow>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationWorkflowConfiguration"/> class.
        /// </summary>
        public ReservationWorkflowConfiguration()
        {
            this.HasRequired( p => p.ReservationWorkflowTrigger ).WithMany().HasForeignKey( p => p.ReservationWorkflowTriggerId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.Reservation ).WithMany( p => p.ReservationWorkflows ).HasForeignKey( p => p.ReservationId ).WillCascadeOnDelete( true );
            this.HasRequired( p => p.Workflow ).WithMany().HasForeignKey( p => p.WorkflowId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
