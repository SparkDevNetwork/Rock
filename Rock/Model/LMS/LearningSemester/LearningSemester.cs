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
    /// Represents a Learning Semester.
    /// </summary>
    [RockDomain( "LMS" )]
    [Table( "LearningSemester" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( SystemGuid.EntityType.LEARNING_SEMESTER )]
    public partial class LearningSemester : Model<LearningSemester>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the LearningSemester.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the LearningSemester.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Id of the related <see cref="Rock.Model.LearningProgram"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> of the <see cref="Rock.Model.LearningProgram"/> associated with the semester.
        /// </value>
        [DataMember]
        public int LearningProgramId { get; set; }

        /// <summary>
        /// Gets or sets the date the semester starts
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date the semester begins.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the date the semester ends.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date the semester ends.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the date that students must enroll by.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date that enrollment will close.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? EnrollmentCloseDate { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="LearningProgram"/> of the semester.
        /// </summary>
        [DataMember]
        public virtual LearningProgram LearningProgram { get; set; }

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.LearningClass">LearningClasses</see> for this semester.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.LearningClass">LearningClasses</see> for this semester.
        /// </value>
        public virtual ICollection<LearningClass> LearningClasses
        {
            get { return _learningClasses ?? ( _learningClasses = new Collection<LearningClass>() ); }
            set { _learningClasses = value; }
        }

        private ICollection<LearningClass> _learningClasses;

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the <see cref="System.String" /> representation of the LearningSemester.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this LearningSemester.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// LearningSemester Configuration class.
    /// </summary>
    public partial class LearningSemesterConfiguration : EntityTypeConfiguration<LearningSemester>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LearningSemesterConfiguration" /> class.
        /// </summary>
        public LearningSemesterConfiguration()
        {
            this.HasRequired( a => a.LearningProgram ).WithMany( a => a.LearningSemesters ).HasForeignKey( a => a.LearningProgramId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}
