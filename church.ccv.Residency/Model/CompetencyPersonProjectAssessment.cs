// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using church.ccv.Residency.Data;

namespace church.ccv.Residency.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "_com_ccvonline_Residency_CompetencyPersonProjectAssessment" )]
    [DataContract]
    public class CompetencyPersonProjectAssessment : Model<CompetencyPersonProjectAssessment>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the competency person project id.
        /// </summary>
        /// <value>
        /// The competency person project id.
        /// </value>
        [DataMember]
        [Required]
        public int CompetencyPersonProjectId { get; set; }

        /// <summary>
        /// Gets or sets the assessor person id.
        /// </summary>
        /// <value>
        /// The assessor person id.
        /// </value>
        [DataMember]
        public int? AssessorPersonId { get; set; }

        /// <summary>
        /// Gets or sets the assessment date time.
        /// </summary>
        /// <value>
        /// The assessment date time.
        /// </value>
        [DataMember]
        public DateTime? AssessmentDateTime { get; set; }

        /// <summary>
        /// Gets or sets the rating.
        /// </summary>
        /// <value>
        /// The rating.
        /// </value>
        [DataMember]
        public decimal? OverallRating { get; set; }

        /// <summary>
        /// Gets or sets the rating notes.
        /// </summary>
        /// <value>
        /// The rating notes.
        /// </value>
        [DataMember]
        public string RatingNotes { get; set; }

        /// <summary>
        /// Gets or sets the resident comments.
        /// </summary>
        /// <value>
        /// The resident comments.
        /// </value>
        [DataMember]
        public string ResidentComments { get; set; }
        
        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the competency person project.
        /// </summary>
        /// <value>
        /// The competency person project.
        /// </value>
        public virtual CompetencyPersonProject CompetencyPersonProject { get; set; }

        /// <summary>
        /// Gets or sets the assessor person.
        /// </summary>
        /// <value>
        /// The assessor person.
        /// </value>
        public virtual Rock.Model.Person AssessorPerson { get; set; }

        /// <summary>
        /// Gets or sets the competency person project assessment point of assessments.
        /// </summary>
        /// <value>
        /// The competency person project assessment point of assessments.
        /// </value>
        public virtual List<CompetencyPersonProjectAssessmentPointOfAssessment> CompetencyPersonProjectAssessmentPointOfAssessments { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "Assessment: {0}", CompetencyPersonProject );
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class CompetencyPersonProjectAssessmentConfiguration : EntityTypeConfiguration<CompetencyPersonProjectAssessment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompetencyPersonProjectAssessmentConfiguration"/> class.
        /// </summary>
        public CompetencyPersonProjectAssessmentConfiguration()
        {
            this.HasRequired( a => a.CompetencyPersonProject ).WithMany( a => a.CompetencyPersonProjectAssessments ).HasForeignKey( a => a.CompetencyPersonProjectId ).WillCascadeOnDelete( true );
            this.HasOptional( a => a.AssessorPerson ).WithMany().HasForeignKey( a => a.AssessorPersonId ).WillCascadeOnDelete( false );
            
            // limit OverallRating to one decimal point
            this.Property( m => m.OverallRating ).HasPrecision( 2, 1 );
        }
    }
}
