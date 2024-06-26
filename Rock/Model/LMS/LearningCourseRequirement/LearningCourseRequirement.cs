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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Enums.Lms;

namespace Rock.Model
{
    /// <summary>
    /// Represents a course .
    /// </summary>
    [RockDomain( "LMS" )]
    [Table( "LearningCourseRequirement" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( SystemGuid.EntityType.LEARNING_COURSE_REQUIREMENT )]
    public partial class LearningCourseRequirement : Model<LearningCourseRequirement>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the id of the <see cref="Rock.Model.LearningCourse"/> which this requirement applies to.
        /// </summary>
        /// <value>
        /// The identifier of the <see cref="Rock.Model.LearningCourse"/> which this requirement applies to.
        /// </value>
        [DataMember]
        public int LearningCourseId { get; set; }

        /// <summary>
        /// Gets or sets the id of the required <see cref="Rock.Model.LearningCourse"/>.
        /// </summary>
        /// <value>
        /// The identifier of the required <see cref="Rock.Model.LearningCourse"/>.
        /// </value>
        [DataMember]
        public int RequiredLearningCourseId { get; set; }

        /// <summary>
        /// Gets or sets the requirement type of the required <see cref="Rock.Model.LearningCourse"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Enum"/> for the requirement type (i.e Prerequisite, corequisite or equivalent).
        /// </value>
        [DataMember]
        public RequirementType RequirementType { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="LearningCourse"/> which has the requirement.
        /// </summary>
        [DataMember]
        public virtual LearningCourse LearningCourse { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="LearningCourse"/> which is the requirement.
        /// </summary>
        [DataMember]
        public virtual LearningCourse RequiredLearningCourse { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this LearningCourseRequirement.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this LearningCourseRequirement.
        /// </returns>
        public override string ToString()
        {
            return EnumExtensions.ConvertToStringSafe( RequirementType );
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// LearningCourseRequirement Configuration class.
    /// </summary>
    public partial class LearningCourseRequirementConfiguration : EntityTypeConfiguration<LearningCourseRequirement>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LearningCourseRequirementConfiguration" /> class.
        /// </summary>
        public LearningCourseRequirementConfiguration()
        {
            this.HasRequired( a => a.LearningCourse ).WithMany( a => a.LearningCourseRequirements ).HasForeignKey( a => a.LearningCourseId ).WillCascadeOnDelete( false );
            this.HasRequired( a => a.RequiredLearningCourse ).WithMany().HasForeignKey( a => a.RequiredLearningCourseId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
