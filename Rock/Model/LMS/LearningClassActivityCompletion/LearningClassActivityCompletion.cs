﻿// <copyright>
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
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a student's completion state for a <see cref="Rock.Model.LearningClass"/> <see cref="Rock.Model.LearningClassActivity"/>.
    /// </summary>
    [RockDomain( "LMS" )]
    [Table( "LearningClassActivityCompletion" )]
    [DataContract]
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( SystemGuid.EntityType.LEARNING_CLASS_ACTIVITY_COMPLETION )]
    public partial class LearningClassActivityCompletion : Model<LearningClassActivityCompletion>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the id of the <see cref="Rock.Model.LearningClassActivity"/> this completion relates to.
        /// </summary>
        /// <value>
        /// The identifier of the <see cref="Rock.Model.LearningClassActivity"/> this completion relates to.
        /// </value>
        [DataMember]
        public int LearningClassActivityId { get; set; }

        /// <summary>
        /// Gets or sets the id of the <see cref="Rock.Model.LearningParticipant">student</see> this completion belongs to.
        /// </summary>
        /// <value>
        /// The identifier of the <see cref="Rock.Model.LearningParticipant">student</see> this completion belongs to.
        /// </value>
        [DataMember]
        public int StudentId { get; set; }

        /// <summary>
        /// Gets or sets the id of the <see cref="Rock.Model.PersonAlias"/> that completed this activity.
        /// </summary>
        /// <value>
        /// The identifier of the <see cref="Rock.Model.PersonAlias"/> that completed this activity.
        /// </value>
        [DataMember]
        public int? CompletedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the completion json for the activity component.
        /// This is the JSON configuration for the activity component including
        /// the responses and configuration at the time of completion.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the completion json for the activity component.
        /// </value>
        [DataMember]
        public string ActivityComponentCompletionJson { get; set; }

        /// <summary>
        /// Gets or sets the date the <see cref="Rock.Model.LearningClassActivity"/>
        /// became available for <see cref="Rock.Model.LearningParticipant">student</see> to complete.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date the student could begin working on the <see cref="Model.LearningClassActivity"/>.
        /// </value>
        [DataMember]
        public DateTime? AvailableDateTime { get; set; }

        /// <summary>
        /// Gets or sets the due date of the <see cref="Rock.Model.LearningClassActivity"/>
        /// for the <see cref="Rock.Model.LearningParticipant">student</see>.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the latest date the student should complete the Activity to be considered "on-time".
        /// </value>
        [Column( TypeName = "Date" )]
        [DataMember]
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Gets or sets the date the <see cref="Rock.Model.LearningParticipant">student</see>
        /// completed the related <see cref="Rock.Model.LearningClassActivity"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date the student completed the <see cref="Model.LearningClassActivity" />.
        /// </value>
        [DataMember]
        public DateTime? CompletedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.LearningParticipant">facilitator's</see> comment.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the comment made by the <see cref="Rock.Model.LearningParticipant">facilitator</see>.
        /// </value>
        [DataMember]
        public string FacilitatorComment { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.LearningParticipant">student's</see> comment.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the comment made by the <see cref="Rock.Model.LearningParticipant">student</see>.
        /// </value>
        [DataMember]
        public string StudentComment { get; set; }

        /// <summary>
        /// Gets or sets the number of points the student earned by completing the activity.
        /// This will be <c>null</c> if no score has been set yet.
        /// </summary>
        /// <value>
        /// The <see cref="System.Int32" /> representing the number of points earned for the activity.
        /// </value>
        [DataMember]
        public int? PointsEarned { get; set; }

        /// <summary>
        /// Indicates whether or not the related activity instance has been completed by the <see cref="Rock.Model.LearningParticipant">student</see>.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the related <see cref="Model.LearningClassActivity"/> instance has been completed by the student; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsStudentCompleted { get; set; }

        /// <summary>
        /// Indicates whether or not the related activity instance for the student has been completed by the <see cref="Rock.Model.LearningParticipant">facilitator</see>.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the related <see cref="Model.LearningClassActivity"/> instance for the student has been completed by the facilitator; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsFacilitatorCompleted { get; set; }

        /// <summary>
        /// Indicates whether or not the related <see cref="Rock.Model.LearningClassActivity"/> was completed by this student before the DueDate.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the related <see cref="Rock.Model.LearningClassActivity"/> was completed before the DueDate; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool WasCompletedOnTime { get; set; }

        /// <summary>
        /// Gets or sets the id of the <see cref="Rock.Model.Communication"/> that
        /// was sent as a notification that this activity is available.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Communication"/> identifier.
        /// </value>
        [DataMember]
        public int? SentNotificationCommunicationId { get; set; }

        /// <summary>
        /// Gets or sets the binary file id for use by the activity component.
        /// </summary>
        /// <value>
        /// The binary file id.
        /// </value>
        [DataMember]
        public int? BinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="PersonAlias"/> identifier of the facilitator who graded the activity.
        /// </summary>
        [DataMember]
        public int? GradedByPersonAliasId { get; set; }

        /// <summary>
        /// Indicates whether or not the <see cref="Model.LearningClassActivityCompletion"/> needs to be graded by a Facilitator.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the <see cref="Model.LearningClassActivityCompletion"/> needs to be graded by a Facilitator; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool RequiresGrading { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Model.LearningClassActivity"/> for the student activity instance.
        /// </summary>
        [DataMember]
        public virtual LearningClassActivity LearningClassActivity { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Communication"/> used for notifications by the student activity instance.
        /// </summary>
        [DataMember]
        public virtual Communication SentNotificationCommunication { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="LearningParticipant">student</see> the activity instance is for.
        /// </summary>
        public virtual LearningParticipant Student { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> of the person who completed this.
        /// </summary>
        [DataMember]
        public virtual PersonAlias CompletedByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> of the faciltator that graded this completion.
        /// </summary>
        [DataMember]
        public virtual PersonAlias GradedByPersonAlias { get; set; }

        #endregion

        #region Public Methods

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// <see cref="Model.LearningClassActivityCompletion"/> Configuration class.
    /// </summary>
    public partial class LearningClassActivityCompletionConfiguration : EntityTypeConfiguration<LearningClassActivityCompletion>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LearningClassActivityCompletionConfiguration" /> class.
        /// </summary>
        public LearningClassActivityCompletionConfiguration()
        {
            this.HasRequired( a => a.LearningClassActivity ).WithMany( a => a.LearningClassActivityCompletions ).HasForeignKey( a => a.LearningClassActivityId ).WillCascadeOnDelete( true );
            this.HasRequired( a => a.Student ).WithMany( a => a.LearningClassActivityCompletions ).HasForeignKey( a => a.StudentId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.CompletedByPersonAlias ).WithMany().HasForeignKey( a => a.CompletedByPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.SentNotificationCommunication ).WithMany().HasForeignKey( a => a.SentNotificationCommunicationId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
