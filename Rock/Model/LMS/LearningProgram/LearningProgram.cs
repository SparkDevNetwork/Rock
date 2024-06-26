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
using Rock.Enums.Lms;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Learning Program containing one or more <see cref="Rock.Model.LearningCourse">Courses</see>.
    /// </summary>
    [RockDomain( "LMS" )]
    [Table( "LearningProgram" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( SystemGuid.EntityType.LEARNING_PROGRAM )]
    public partial class LearningProgram : Model<LearningProgram>, IHasActiveFlag, ICategorized
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the LearningProgram.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the LearningProgram.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the public name of the LearningProgram.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the LearningProgram shown to the public.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string PublicName { get; set; }

        /// <summary>
        /// Gets or sets the summary text of the LearningProgram.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Summary text of the LearningProgram.
        /// </value>
        [MaxLength( 500 )]
        [DataMember]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the Description of the LearningProgram.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the full description of the LearningProgram.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string IconCssClass { get; set; } = "fa fa-university";

        /// <summary>
        /// Gets or sets the color of the highlight.
        /// </summary>
        /// <value>
        /// The color of the highlight.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string HighlightColor { get; set; }

        /// <summary>
        /// Gets or sets the binary file id of the image for the LearningProgram.
        /// </summary>
        /// <value>
        /// The image binary file id.
        /// </value>
        [DataMember]
        public int? ImageBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the configuration mode of the LearningProgram.
        /// </summary>
        /// <value>
        /// A <see cref="System.Enum"/> for the configuration mode (i.e Academic Calendar, On-Demand Learning).
        /// </value>
        [Required]
        [DataMember]
        public ConfigurationMode ConfigurationMode { get; set; }

        /// <summary>
        /// Indicates whether or not this LearningProgram should
        /// be displayed in public contexts (e.g. on a public site).
        /// </summary>
        /// <value>
        ///   <c>true</c> if this LearningProgram should be publicly visible; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsPublic { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Model.Category"/> identifier.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        [DataMember]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets a value indicating whether this LearningProgram is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this LearningProgram is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets a value indicating whether this LearningProgram tracks student's completion of the program.
        /// Once a LearningProgram begins this value cannot be changed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this LearningProgram tracks completion status; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsCompletionStatusTracked { get; set; }

        /// <summary>
        /// Gets or sets theid of the system communication.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.SystemCommunication"/> identifier.
        /// </value>
        public int SystemCommunicationId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.WorkflowType"/> that is triggered when the program is completed by a student.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.WorkflowType"/> identifier.
        /// </value>
        [DataMember]
        public int? CompletionWorkflowTypeId { get; set; }

        /// <summary>
        /// Gets or sets the number of absences at which a warning should be triggered.
        /// </summary>
        /// <value>
        /// The <see cref="System.Int32" /> representing the number of absences.
        /// </value>
        [DataMember]
        public int? AbsencesWarningCount { get; set; }

        /// <summary>
        /// Gets or sets the number of absences at which a critical alert should be triggered.
        /// </summary>
        /// <value>
        /// The <see cref="System.Int32" /> representing the number of absences.
        /// </value>
        [DataMember]
        public int? AbsencesCriticalCount { get; set; }

        /// <summary>
        /// Gets or sets the additional settings json.
        /// </summary>
        /// <value>
        /// The additional settings json.
        /// </value>
        [DataMember]
        public string AdditionalSettingsJson { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the related <see cref="Rock.Model.BinaryFile"/> for the program.
        /// </summary>
        [DataMember]
        public virtual BinaryFile ImageBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the related <see cref="Rock.Model.Category"/> for the program.
        /// </summary>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets the related <see cref="Rock.Model.SystemCommunication"/> for the program.
        /// </summary>
        [DataMember]
        public virtual SystemCommunication SystemCommunication { get; set; }

        /// <summary>
        /// Gets or sets the related completion <see cref="Rock.Model.WorkflowType">WorkflowType</see> for the program.
        /// </summary>
        [DataMember]
        public virtual WorkflowType CompletionWorkflowType { get; set; }

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.LearningProgramCompletion">LearningProgramCompletions</see> for this LearningProgram.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.LearningProgramCompletion">LearningProgramCompletions</see> for this LearningProgram.
        /// </value>
        public virtual ICollection<LearningProgramCompletion> LearningProgramCompletions
        {
            get { return _learningProgramCompletions ?? ( _learningProgramCompletions = new Collection<LearningProgramCompletion>() ); }
            set { _learningProgramCompletions = value; }
        }

        private ICollection<LearningProgramCompletion> _learningProgramCompletions;

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.LearningSemester">LearningSemesters</see> for this LearningProgram.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.LearningSemester">LearningSemesters</see> for this LearningProgram.
        /// </value>
        public virtual ICollection<LearningSemester> LearningSemesters
        {
            get { return _learningSemesters ?? ( _learningSemesters = new Collection<LearningSemester>() ); }
            set { _learningSemesters = value; }
        }

        private ICollection<LearningSemester> _learningSemesters;

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.LearningCourse">LearningCourses</see> for this LearningProgram.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.LearningCourse">LearningCourses</see> for this LearningProgram.
        /// </value>
        public virtual ICollection<LearningCourse> LearningCourses
        {
            get { return _learningCourses ?? ( _learningCourses = new Collection<LearningCourse>() ); }
            set { _learningCourses = value; }
        }

        private ICollection<LearningCourse> _learningCourses;

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this LearningProgram.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this LearningProgram.
        /// </returns>
        public override string ToString()
        {
            return this.PublicName;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// LearningProgram Configuration class.
    /// </summary>
    public partial class LearningProgramConfiguration : EntityTypeConfiguration<LearningProgram>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LearningProgramConfiguration" /> class.
        /// </summary>
        public LearningProgramConfiguration()
        {
            this.HasOptional( a => a.CompletionWorkflowType ).WithMany().HasForeignKey( a => a.CompletionWorkflowTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.ImageBinaryFile ).WithMany().HasForeignKey( a => a.ImageBinaryFileId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.Category ).WithMany().HasForeignKey( a => a.CategoryId ).WillCascadeOnDelete( false );
            this.HasRequired( a => a.SystemCommunication ).WithMany().HasForeignKey( a => a.SystemCommunicationId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
