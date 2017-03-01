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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Model;

namespace com.centralaz.RoomManagement.Model
{
    /// <summary>
    /// A Reservation Workflow Trigger
    /// </summary>
    [Table( "_com_centralaz_RoomManagement_ReservationWorkflowTrigger" )]
    [DataContract]
    public class ReservationWorkflowTrigger : Rock.Data.Model<ReservationWorkflowTrigger>, Rock.Data.IRockEntity
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the workflow type identifier.
        /// </summary>
        /// <value>
        /// The workflow type identifier.
        /// </value>
        [Required]
        [DataMember]
        public int? WorkflowTypeId { get; set; }

        /// <summary>
        /// Gets or sets the type of the trigger.
        /// </summary>
        /// <value>
        /// The type of the trigger.
        /// </value>
        [DataMember]
        public ReservationWorkflowTriggerType TriggerType { get; set; }

        /// <summary>
        /// Gets or sets the qualifier value.
        /// </summary>
        /// <value>
        /// The qualifier value.
        /// </value>
        [DataMember]
        public string QualifierValue { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the workflow.
        /// </summary>
        /// <value>
        /// The type of the workflow.
        /// </value>
        [DataMember]
        public virtual WorkflowType WorkflowType { get; set; }

        #endregion
    }

    #region Entity Configuration


    public partial class ReservationWorkflowTriggerConfiguration : EntityTypeConfiguration<ReservationWorkflowTrigger>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReservationWorkflowTriggerConfiguration"/> class.
        /// </summary>
        public ReservationWorkflowTriggerConfiguration()
        {
            this.HasRequired( p => p.WorkflowType ).WithMany().HasForeignKey( p => p.WorkflowTypeId ).WillCascadeOnDelete( true );
        }
    }
}

#endregion

#region Enumerations

/// <summary>
/// Type of workflow trigger
/// </summary>
public enum ReservationWorkflowTriggerType
{
    /// <summary>
    /// The reservation created
    /// </summary>
    ReservationCreated = 0,

    /// <summary>
    /// The reservation updated
    /// </summary>
    ReservationUpdated = 1,

    /// <summary>
    /// The status changed
    /// </summary>
    StatusChanged = 2,

    /// <summary>
    /// The manual
    /// </summary>
    Manual = 3
}

#endregion
