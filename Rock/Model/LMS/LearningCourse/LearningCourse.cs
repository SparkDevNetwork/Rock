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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a course within a <see cref="Rock.Model.LearningProgram"/>.
    /// </summary>
    [RockDomain( "LMS" )]
    [Table( "LearningCourse" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( SystemGuid.EntityType.LEARNING_COURSE )]
    public partial class LearningCourse : Model<LearningCourse>, IHasActiveFlag, IOrdered, ICategorized
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the course.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the LearningCourse.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the public name of the course.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the LearningCourse shown to the public.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string PublicName { get; set; }

        /// <summary>
        /// Gets or sets the summary text of the course.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Summary text of the LearningCourse.
        /// </value>
        [MaxLength( 500 )]
        [DataMember]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the description of the course.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the full description of the LearningCourse.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the binary file id of the image for the course.
        /// </summary>
        /// <value>
        /// The image binary file id.
        /// </value>
        [DataMember]
        public int? ImageBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the id of the <see cref="Rock.Model.LearningProgram"/> for the course.
        /// </summary>
        /// <value>
        /// The identifier of the LearningProgram.
        /// </value>
        [DataMember]
        public int LearningProgramId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Model.Category"/> id.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        [DataMember]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the code for the course.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the course code.
        /// </value>
        [MaxLength( 12 )]
        [DataMember]
        public string CourseCode { get; set; }

        /// <summary>
        /// Gets or sets the number of students at which to stop accepting enrollments.
        /// </summary>
        /// <value>
        /// The <see cref="System.Int32" /> representing the maximum number of students.
        /// </value>
        [DataMember]
        public int? MaxStudents { get; set; }

        /// <summary>
        /// Gets or sets the number of credits awarded for successful completion of the course.
        /// </summary>
        /// <value>
        /// The <see cref="System.Int32" /> representing the number of credits.
        /// </value>
        [DataMember]
        public int Credits { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this course is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this LearningCourse is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; }

        /// <summary>
        /// Indicates whether or not this course should 
        /// be displayed in public contexts (e.g. on a public site).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this LearningCourse should be publicly visible; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsPublic { get; set; }

        /// <summary>
        /// Gets or sets the order in which the course should be displayed.
        /// </summary>
        /// <value>
        /// The <see cref="System.Int32"/> value representing the order.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.WorkflowType"/> that's triggered when the course is completed by a student.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.WorkflowType"/> identifier.
        /// </value>
        [DataMember]
        public int? CompletionWorkflowTypeId { get; set; }

        /// <summary>
        /// Indicates whether or not this course allows announcements.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this LearningCourse allows announcements; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnableAnnouncements { get; set; }

        /// <summary>
        /// Indicates whether or not this course allows students to access after completion.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this LearningCourse allows historical access; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowHistoricalAccess { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="LearningProgram"/> of the course.
        /// </summary>
        [DataMember]
        public virtual LearningProgram LearningProgram { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="WorkflowType"/> of the LearningCourse.
        /// </summary>
        [DataMember]
        public virtual WorkflowType CompletionWorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Category"/> for the LearningCourse.
        /// </summary>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="BinaryFile">ImageBinaryFile</see> for the LearningCourse.
        /// </summary>
        [DataMember]
        public virtual BinaryFile ImageBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.LearningCourseRequirement">LearningCourseRequirements</see> for the course.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.LearningCourseRequirement">LearningCourseRequirements</see> for the LearningCourse.
        /// </value>
        public virtual ICollection<LearningCourseRequirement> LearningCourseRequirements
        {
            get { return _learningCourseRequirements ?? ( _learningCourseRequirements = new Collection<LearningCourseRequirement>() ); }
            set { _learningCourseRequirements = value; }
        }

        private ICollection<LearningCourseRequirement> _learningCourseRequirements;

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.LearningClass">LearningClasses</see> for the LearningCourse.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.LearningClass">LearningClasses</see> for the LearningCourse.
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
        /// Returns a <see cref="System.String" /> that represents this LearningCourse.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this LearningCourse.
        /// </returns>
        public override string ToString()
        {
            return $"{Name}: {CourseCode}";
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// LearningCourse Configuration class.
    /// </summary>
    public partial class LearningCourseConfiguration : EntityTypeConfiguration<LearningCourse>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LearningCourseConfiguration" /> class.
        /// </summary>
        public LearningCourseConfiguration()
        {
            this.HasRequired( a => a.LearningProgram ).WithMany( a => a.LearningCourses ).HasForeignKey( a => a.LearningProgramId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.CompletionWorkflowType ).WithMany().HasForeignKey( a => a.CompletionWorkflowTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.ImageBinaryFile ).WithMany().HasForeignKey( a => a.ImageBinaryFileId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.Category ).WithMany().HasForeignKey( a => a.CategoryId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
