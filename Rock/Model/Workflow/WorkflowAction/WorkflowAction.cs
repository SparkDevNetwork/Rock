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
using Rock.Data;
using Rock.Lava;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// Represents a persisted WorkflowAction in Rock.
    /// </summary>
    [RockDomain( "Workflow" )]
    [Table( "WorkflowAction" )]
    [DataContract]
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( "9CBF4BEC-5653-47F9-8E87-0D31C6CA5947")]
    public partial class WorkflowAction : Model<WorkflowAction>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the WorkflowActivityId of the <see cref="Rock.Model.WorkflowActivity"/> that this WorkflowAction is a part of.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> represents the WorflowActivityId that this WorkflowAction is a part of.
        /// </value>
        [DataMember]
        public int ActivityId { get; set; }

        /// <summary>
        /// Gets or sets the ActionTypeId of the <see cref="Rock.Model.WorkflowAction"/> that is being executed by this instance.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the ActionTypeId of the <see cref="Rock.Model.WorkflowActionType"/> that is being executed on this instance.
        /// </value>
        [DataMember]
        public int ActionTypeId { get; set; }

        /// <summary>
        /// Gets or sets the date and time that this WorkflowAction was last processed.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that this WorkflowAction was last processed.
        /// </value>
        [DataMember]
        [NotAudited]
        public DateTime? LastProcessedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the WorkflowAction completed.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that the WorkflowAction completed.
        /// </value>
        [DataMember]
        public DateTime? CompletedDateTime { get; set; }

        /// <summary>
        /// If ActionType is a UserEntryForm Gets or sets the form action.
        /// </summary>
        /// <value>
        /// The form action.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string FormAction { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WorkflowActivity"/> that contains the WorkflowAction.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.WorkflowActivity"/> that contains this WorkflowAction.
        /// </value>
        [LavaVisible]
        public virtual WorkflowActivity Activity { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WorkflowActionType"/> that is being executed by this WorkflowAction.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.WorkflowActionType"/> that is being executed.
        /// </value>
        [LavaVisible]
        public virtual WorkflowActionType ActionType { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// Action Configuration class.
    /// </summary>
    public partial class WorkflowActionConfiguration : EntityTypeConfiguration<WorkflowAction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowActionConfiguration"/> class.
        /// </summary>
        public WorkflowActionConfiguration()
        {
            this.HasRequired( m => m.Activity ).WithMany( m => m.Actions ).HasForeignKey( m => m.ActivityId ).WillCascadeOnDelete( true );
            this.HasRequired( m => m.ActionType ).WithMany().HasForeignKey( m => m.ActionTypeId).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration

}

