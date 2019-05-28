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
using System.Text;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents an assessment.
    /// </summary>
    [RockDomain( "CRM" )]
    [Table( "Assessment" )]
    [DataContract]
    public partial class Assessment : Model<Assessment>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the person <see cref="Rock.Model.Person"/> who is associated with the assesment.
        /// </summary>
        /// <value>
        /// A <see cref="System.int"/> representing the PersonAliasId of <see cref="Rock.Model.PersonAlias"/> associated with the Assessment.
        /// </value>
        [Required]
        [DataMember]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.AssessmentType"/>
        /// </summary>
        /// <value>
        /// A <see cref="System.int"/> of the <see cref="Rock.Model.AssessmentType"/> associated with the Assessment.
        /// </value>
        [Required]
        [DataMember]
        public int AssessmentTypeId { get; set; }

        /// <summary>
        /// Gets or sets the RequesterPersonAliasId of the <see cref="Rock.Model.Person"/> that requested the assessment.
        /// </summary>
        /// <value>
        /// A <see cref="System.int"/> of the <see cref="Rock.Model.PersonAlias"/> making the request for the Assessment.
        /// </value>
        [DataMember]
        public int? RequesterPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the date when the assessment was requested.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> of when the request for the assessment was made..
        /// </value>
        [DataMember]
        public DateTime? RequestedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date of the requested due date.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> of when the Assessment is due.
        /// </value>
        [DataMember]
        public DateTime? RequestedDueDate { get; set; }

        /// <summary>
        /// Gets or sets the enum of the assessment status.
        /// </summary>
        /// <value>
        /// A <see cref="System.Enum"/> for the Assessment status (i.e Complete, Pending).
        /// </value>
        [Required]
        [DataMember]
        public AssessmentRequestStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the date of when the Assessment was completed.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> for when the Assessment was complete.
        /// </value>
        [DataMember]
        public DateTime? CompletedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the result data for the Assessment taken.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> a JSON return data string of the Assessment result..
        /// </value>
        [DataMember]
        public string AssessmentResultData { get; set; }

        /// <summary>
        /// Gets or sets the result last reminder date.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> of the last reminder for the Assessment.
        /// </value>
        [DataMember]
        public DateTime? LastReminderDate { get; set; }

        #endregion

        #region Virtual Properties
        
        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.AssessmentType"/> that represents the type of the assessment.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.AssessmentType"/> that represents the type of the assessment.
        /// </value>
        [DataMember]
        public virtual AssessmentType AssessmentType { get; set; }

        /// <summary>
        /// Gets or sets the person alias <see cref="Rock.Model.Person"/> associated with the Assessment.
        /// </summary>
        /// <value>
        /// A person alias <see cref="Rock.Model.PersonAlias"/> asssociated to this Assessment.
        /// </value>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the person alias <see cref="Rock.Model.Person"/> requesting the Assessment.
        /// </summary>
        /// <value>
        /// The person alias.<see cref="Rock.Model.PersonAlias"/>
        /// </value>
        [LavaInclude]
        public virtual PersonAlias RequesterPersonAlias { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance AssessmentResultData.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance AssessmentResultData.
        /// </returns>
        public override string ToString()
        {
            return this.AssessmentResultData;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Assessment Configuration class.
    /// </summary>
    public partial class AssessmentConfiguration : EntityTypeConfiguration<Assessment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssessmentConfiguration" /> class.
        /// </summary>
        public AssessmentConfiguration()
        {
            this.HasRequired( a => a.AssessmentType ).WithMany( a => a.Assessments ).HasForeignKey( a => a.AssessmentTypeId ).WillCascadeOnDelete( true );
            this.HasRequired( a => a.PersonAlias ).WithMany().HasForeignKey( a => a.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.RequesterPersonAlias ).WithMany().HasForeignKey( a => a.RequesterPersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Enumerations
    /// <summary>
    /// Gets the status of the Assessment  (i.e. Pending, Complete)
    /// </summary>
    public enum AssessmentRequestStatus
    {
        /// <summary>
        /// Pending Status
        /// </summary>
        Pending = 0,

        /// <summary>
        /// Complete Status
        /// </summary>
        Complete = 1,
    }
    #endregion
}
