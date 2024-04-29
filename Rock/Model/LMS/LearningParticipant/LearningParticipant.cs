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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Enums.Lms;
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// Represents a participant in a <see cref="Rock.Model.LearningClass"/>.
    /// </summary>
    [CodeGenExclude( CodeGenFeature.DefaultRestController )] // Due to inheritance from GroupMember.
    [RockDomain( "LMS" )]
    [Table( "LearningParticipant" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( SystemGuid.EntityType.LEARNING_PARTICIPANT )]
    public partial class LearningParticipant : GroupMember
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the completion status for the participant's <see cref="LearningClass"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Enum"/> for the completion status (i.e Incomplete, Fail, Pass).
        /// </value>
        [DataMember]
        public LearningCompletionStatus LearningCompletionStatus { get; set; }

        /// <summary>
        /// Gets or sets the date the student completed the <see cref="Rock.Model.LearningClass"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date the student completed the LearningClass.
        /// </value>
        [DataMember]
        public DateTime? LearningCompletionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the id of the <see cref="Rock.Model.LearningGradingSystemScale"/> for this class participant.
        /// </summary>
        /// <value>
        /// The identifier for the related <see cref="Rock.Model.LearningGradingSystemScale"/>.
        /// </value>
        [DataMember]
        public int? LearningGradingSystemScaleId { get; set; }

        /// <summary>
        /// Gets or sets the grade percent achieved for this participant.
        /// </summary>
        /// <value>
        /// LearningGradePercent for this learning Participant.
        /// </value>
        [DecimalPrecision( 18, 3 )]
        [DataMember]
        public decimal LearningGradePercent { get; set; }

        /// <summary>
        /// Gets or sets the id of the related <see cref="Rock.Model.LearningProgramCompletion"/> for this particpant.
        /// </summary>
        /// <value>
        /// The identifier for the related <see cref="Rock.Model.LearningProgramCompletion"/>.
        /// </value>
        [DataMember]
        public int? LearningProgramCompletionId { get; set; }

        /// <summary>
        /// Gets or sets the id of the related <see cref="Rock.Model.LearningClass"/> for this class particpant.
        /// </summary>
        /// <value>
        /// The identifier for the related <see cref="Rock.Model.LearningClass"/>.
        /// </value>
        [DataMember]
        public int LearningClassId { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the related <see cref="Rock.Model.LearningProgramCompletion"/>.
        /// </summary>
        [DataMember]
        public virtual LearningProgramCompletion LearningProgramCompletion { get; set; }

        /// <summary>
        /// Gets or sets the related <see cref="Rock.Model.LearningGradingSystemScale"/>.
        /// </summary>
        [DataMember]
        public virtual LearningGradingSystemScale LearningGradingSystemScale { get; set; }

        /// <summary>
        /// Gets or sets the related <see cref="Rock.Model.LearningClass"/>.
        /// </summary>
        [DataMember]
        public virtual LearningClass LearningClass { get; set; }

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.LearningActivityCompletion">activities</see> for this participant.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.LearningActivityCompletion">activities</see> for this participant.
        /// </value>
        public virtual ICollection<LearningActivityCompletion> LearningActivities
        {
            get { return _learningActivities ?? ( _learningActivities = new Collection<LearningActivityCompletion>() ); }
            set { _learningActivities = value; }
        }

        private ICollection<LearningActivityCompletion> _learningActivities;

        #endregion

        #region Public Methods

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// LearningParticipant Configuration class.
    /// </summary>
    public partial class LearningParticipantConfiguration : EntityTypeConfiguration<LearningParticipant>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LearningParticipantConfiguration" /> class.
        /// </summary>
        public LearningParticipantConfiguration()
        {
            this.HasRequired( a => a.LearningClass ).WithMany( a => a.LearningParticipants ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.LearningGradingSystemScale ).WithMany().HasForeignKey( a => a.LearningGradingSystemScaleId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.LearningProgramCompletion ).WithMany().HasForeignKey( a => a.LearningProgramCompletionId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}