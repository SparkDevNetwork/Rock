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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// Represents a learning class (an instance of a course for a given semester).
    /// </summary>
    [CodeGenExclude( CodeGenFeature.DefaultRestController )] // Due to inheritance from Group.
    [RockDomain( "LMS" )]
    [Table( "LearningClass" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( SystemGuid.EntityType.LEARNING_CLASS )]
    public partial class LearningClass : Group
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the id of the related <see cref="Rock.Model.LearningCourse"/> for the class.
        /// </summary>
        /// <value>
        /// The identifier for the related <see cref="Rock.Model.LearningCourse"/>.
        /// </value>
        [DataMember]
        public int LearningCourseId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the related <see cref="Rock.Model.LearningSemester"/> for the class.
        /// </summary>
        /// <value>
        /// The identifier for the related <see cref="Rock.Model.LearningSemester"/>.
        /// </value>
        [DataMember]
        public int? LearningSemesterId { get; set; }

        /// <summary>
        /// Gets or sets the id of the related <see cref="Rock.Model.LearningGradingSystem"/> for the class.
        /// </summary>
        /// <value>
        /// The identifier for the related <see cref="Rock.Model.LearningGradingSystem"/>.
        /// </value>
        [DataMember]
        public int LearningGradingSystemId { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the related <see cref="Rock.Model.LearningCourse"/>.
        /// </summary>
        [DataMember]
        public virtual LearningCourse LearningCourse { get; set; }

        /// <summary>
        /// Gets or sets the related <see cref="Rock.Model.LearningSemester"/>.
        /// </summary>
        [DataMember]
        public virtual LearningSemester LearningSemester { get; set; }

        /// <summary>
        /// Gets or sets the related <see cref="Rock.Model.LearningGradingSystem"/>.
        /// </summary>
        [DataMember]
        public virtual LearningGradingSystem LearningGradingSystem { get; set; }

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.LearningActivity">activities</see> for the class.
        /// </summary>
        /// <value>
        /// A collection of the <see cref="Rock.Model.LearningActivity">activities</see> for the LearningClass.
        /// </value>
        public virtual ICollection<LearningActivity> LearningActivities
        {
            get { return _learningActivities ?? ( _learningActivities = new Collection<LearningActivity>() ); }
            set { _learningActivities = value; }
        }

        private ICollection<LearningActivity> _learningActivities;

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.LearningParticipant">participants</see> for the class.
        /// </summary>
        /// <value>
        /// A collection of the <see cref="Rock.Model.LearningParticipant">participants</see> for the LearningClass.
        /// </value>
        public virtual ICollection<LearningParticipant> LearningParticipants
        {
            get { return _learningParticipants ?? ( _learningParticipants = new Collection<LearningParticipant>() ); }
            set { _learningParticipants = value; }
        }

        private ICollection<LearningParticipant> _learningParticipants;

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.LearningClassContentPage">content pages</see> for the class.
        /// </summary>
        /// <value>
        /// A collection of the <see cref="Rock.Model.LearningClassContentPage">content pages</see> for the LearningClass.
        /// </value>
        public virtual ICollection<LearningClassContentPage> ContentPages
        {
            get { return _contentPages ?? ( _contentPages = new Collection<LearningClassContentPage>() ); }
            set { _contentPages = value; }
        }

        private ICollection<LearningClassContentPage> _contentPages;

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.LearningClassAnnouncement">announcements</see> for the class.
        /// </summary>
        /// <value>
        /// A collection of the <see cref="Rock.Model.LearningClassAnnouncement">announcements</see> for the LearningClass.
        /// </value>
        public virtual ICollection<LearningClassAnnouncement> Announcements
        {
            get { return _announcements ?? ( _announcements = new Collection<LearningClassAnnouncement>() ); }
            set { _announcements = value; }
        }

        private ICollection<LearningClassAnnouncement> _announcements;

        #endregion

        #region Public Methods

        /// <summary>
        /// The name of the learning class.
        /// </summary>
        /// <returns>The name of the learning class.</returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// LearningClass Configuration class.
    /// </summary>
    public partial class LearningClassConfiguration : EntityTypeConfiguration<LearningClass>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LearningClassConfiguration" /> class.
        /// </summary>
        public LearningClassConfiguration()
        {
            this.HasOptional( a => a.LearningSemester ).WithMany( a => a.LearningClasses ).HasForeignKey( a => a.LearningSemesterId ).WillCascadeOnDelete( false );
            this.HasRequired( a => a.LearningCourse ).WithMany( a => a.LearningClasses ).HasForeignKey( a => a.LearningCourseId ).WillCascadeOnDelete( true );

            // We don't intend to allow deletion of an in-use grading system at this time, therefore do not cascade delete as an additiona protection.
            this.HasRequired( a => a.LearningGradingSystem ).WithMany( a => a.LearningClasses ).HasForeignKey( a => a.LearningGradingSystemId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
