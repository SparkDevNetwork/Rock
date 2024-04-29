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
using Rock.Enums.Lms;

namespace Rock.Model
{
    /// <summary>
    /// Represents a activity or task for class.
    /// </summary>
    [RockDomain( "LMS" )]
    [Table( "LearningActivity" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( SystemGuid.EntityType.LEARNING_ACTIVITY )]
    public partial class LearningActivity : Model<LearningActivity>, IOrdered
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the id of the <see cref="Rock.Model.LearningClass"/> this activity belongs to.
        /// </summary>
        /// <value>
        /// The identifier of the <see cref="Rock.Model.LearningClass"/> this activity belongs to.
        /// </value>
        [DataMember]
        public int LearningClassId { get; set; }

        /// <summary>
        /// Gets or sets the name of the activity.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the LearningActivity.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the activity.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the full description of the LearningActivity.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the order in which the activity should be displayed.
        /// </summary>
        /// <value>
        /// The <see cref="System.Int32"/> value representing the order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// The id of the related ActivityComponent for this LearningActivity.
        /// </summary>
        /// <value>
        /// The identifier of the related ActivityComponent.
        /// </value>
        [DataMember]
        public int ActivityComponentId { get; set; }

        /// <summary>
        /// Gets or sets the json config for the activity component before completion.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the json configuration of the activity component before completion.
        /// </value>
        [DataMember]
        public string ActivityComponentSettingsJson { get; set; }

        /// <summary>
        /// The participant type assigned to complete this activity.
        /// </summary>
        /// <value>
        /// A <see cref="System.Enum"/> for the type of learning participant responsible for completing the activity (i.e Facilitator or Student ).
        /// </value>
        [DataMember]
        public AssignTo AssignTo { get; set; }

        /// <summary>
        /// The calculation method used for determing the DueDate of the activity.
        /// </summary>
        /// <value>
        /// A <see cref="System.Enum"/> for the due date calculation method (i.e Specific, Class Start Offset, Enrollment Offset or No Date ).
        /// </value>
        [DataMember]
        public DueDateCalculationMethod DueDateCalculationMethod { get; set; }

        /// <summary>
        /// Gets or sets the default date the <see cref="Rock.Model.LearningActivity">activity</see> is due.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the latest date the learning participant must complete the activity.
        /// </value>
        [Column( TypeName = "Date" )]
        [DataMember]
        public DateTime? DueDateDefault { get; set; }

        /// <summary>
        /// The optional offset to use for calculating the DueDate.
        /// </summary>
        /// <value>
        /// The number of days to offset the DueDate to be used in conjunction with <see cref="DueDateCalculationMethod"/>.
        /// </value>
        [DataMember]
        public int? DueDateOffset { get; set; }

        /// <summary>
        /// The calculation method used for determing the AvailableDate of the activity.
        /// </summary>
        /// <value>
        /// A <see cref="System.Enum"/> for the available date calculation method (i.e Specific, Class Start Offset, Enrollment Offset, No Date or Always Available ).
        /// </value>
        [DataMember]
        public AvailableDateCalculationMethod AvailableDateCalculationMethod { get; set; }

        /// <summary>
        /// Gets or sets the default date the <see cref="Rock.Model.LearningActivity">activity</see>
        /// is available for the <see cref="Rock.Model.LearningParticipant"/> to complete.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date the learning participant could begin working on the activity.
        /// </value>
        [Column( TypeName = "Date" )]
        [DataMember]
        public DateTime? AvailableDateDefault { get; set; }

        /// <summary>
        /// The optional offset to use for calculating the AvailableDate.
        /// </summary>
        /// <value>
        /// The number of days to offset the AvailableDate to be used in conjunction with <see cref="AvailableDateCalculationMethod"/>.
        /// </value>
        [DataMember]
        public int? AvailableDateOffset { get; set; }

        /// <summary>
        /// Gets or sets the id of the <see cref="Rock.Model.BinaryFile">TaskBinaryFile</see> for the activity.
        /// </summary>
        /// <value>
        /// The binary file id.
        /// </value>
        [DataMember]
        public int? TaskBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of points the activity is worth.
        /// </summary>
        /// <value>
        /// The <see cref="System.Int32" /> representing the number of points for the activity.
        /// </value>
        [DataMember]
        public int Points { get; set; }

        /// <summary>
        /// Indicates whether or not this activity allows students to comment.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this activity should allow students to comment; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsStudentCommentingEnabled { get; set; }

        /// <summary>
        /// Indicates whether or not this activity sends a notification.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this activity sends a notification; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool SendNotificationCommunication { get; set; }

        /// <summary>
        /// Gets or sets the id of the <see cref="Rock.Model.WorkflowType"/> that's triggered when the activity is completed.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.WorkflowType"/> identifier.
        /// </value>
        [DataMember]
        public int? CompletionWorkflowTypeId { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="LearningClass"/> for the activity.
        /// </summary>
        [DataMember]
        public virtual LearningClass LearningClass { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="WorkflowType"/> for the activity.
        /// </summary>
        [DataMember]
        public virtual WorkflowType CompletionWorkflowType { get; set; }

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.LearningActivityCompletion">LearningActivityCompletions</see> for this activity.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.LearningActivityCompletion">LearningActivityCompletions</see> for this activity.
        /// </value>
        public virtual ICollection<LearningActivityCompletion> LearningActivityCompletions
        {
            get { return _learningActivityCompletions ?? ( _learningActivityCompletions = new Collection<LearningActivityCompletion>() ); }
            set { _learningActivityCompletions = value; }
        }

        private ICollection<LearningActivityCompletion> _learningActivityCompletions;

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this activity.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this LearningActivity.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// LearningActivity Configuration class.
    /// </summary>
    public partial class LearningActivityConfiguration : EntityTypeConfiguration<LearningActivity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LearningActivityConfiguration" /> class.
        /// </summary>
        public LearningActivityConfiguration()
        {
            this.HasOptional( a => a.CompletionWorkflowType ).WithMany().HasForeignKey( a => a.CompletionWorkflowTypeId ).WillCascadeOnDelete( false );
            this.HasRequired( a => a.LearningClass ).WithMany( a => a.LearningActivities ).HasForeignKey( a => a.LearningClassId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}
