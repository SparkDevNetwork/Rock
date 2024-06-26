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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Enums.Lms;

namespace Rock.Model
{
    /// <summary>
    /// Represents a <see cref="Rock.Model.LearningProgram">learning program's</see> completion by a student.
    /// </summary>
    [RockDomain( "LMS" )]
    [Table( "LearningProgramCompletion" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( SystemGuid.EntityType.LEARNING_PROGRAM_COMPLETION )]
    public partial class LearningProgramCompletion : Model<LearningProgramCompletion>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the related <see cref="Rock.Model.LearningProgram"/>
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> of the <see cref="Rock.Model.LearningProgram"/> associated with the completion.
        /// </value>
        [DataMember]
        public int LearningProgramId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.PersonAlias"/> who this completion is for.
        /// </summary>
        /// <value>
        /// The PersonAliasId of <see cref="Rock.Model.PersonAlias"/> associated with the completion.
        /// </value>
        [Required]
        [DataMember]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Campus"/> that the student's enrolled program relates to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Campus"/> that was attended.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.CAMPUS )]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the date the student started the <see cref="Rock.Model.LearningProgram"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date the student started the LearningProgram.
        /// </value>
        [DataMember]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the date the student completed the <see cref="Rock.Model.LearningProgram"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the student's completion date of the <see cref="Rock.Model.LearningProgram"/>
        /// </value>
        [DataMember]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets the start date key used for indexing. Only the date portion
        /// of <see cref="StartDate"/> is used when calculating this.
        /// </summary>
        /// <value>
        /// The start date key used for indexing.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.DATE )]
        public int StartDateKey
        {
            get => StartDate.ToString( "yyyyMMdd" ).AsInteger();
            private set { /* Required for EF. */ }
        }

        /// <summary>
        /// Gets the end date key used for indexing. Only the date portion
        /// of <see cref="EndDate"/> is used when calculating this.
        /// </summary>
        /// <value>
        /// The end date key used for indexing.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.DATE )]
        public int? EndDateKey
        {
            get => EndDate.HasValue ? EndDate.Value.ToString( "yyyyMMdd" ).AsIntegerOrNull() : null;
            private set { /* Required for EF. */ }
        }

        /// <summary>
        /// Gets or sets the student's completion status for the program.
        /// </summary>
        /// <value>
        /// A <see cref="System.Enum"/> for the completion status (i.e Pending, Completed or Expired).
        /// </value>
        [Required]
        [DataMember]
        public CompletionStatus CompletionStatus { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="LearningProgram"/> of the student program instance.
        /// </summary>
        [DataMember]
        public virtual LearningProgram LearningProgram { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Campus"/> where the program takes place.
        /// </summary>
        [DataMember]
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="PersonAlias"/> that's completing the program.
        /// </summary>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this LearningProgramCompletion.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this LearningProgramCompletion.
        /// </returns>
        public override string ToString()
        {
            return EnumExtensions.ConvertToStringSafe( CompletionStatus );
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// LearningProgramCompletion Configuration class.
    /// </summary>
    public partial class LearningProgramCompletionConfiguration : EntityTypeConfiguration<LearningProgramCompletion>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LearningProgramCompletion" /> class.
        /// </summary>
        public LearningProgramCompletionConfiguration()
        {
            this.HasRequired( a => a.LearningProgram ).WithMany( a => a.LearningProgramCompletions ).HasForeignKey( a => a.LearningProgramId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.Campus ).WithMany().HasForeignKey( a => a.CampusId ).WillCascadeOnDelete( false );
            this.HasRequired( a => a.PersonAlias ).WithMany().HasForeignKey( a => a.PersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
