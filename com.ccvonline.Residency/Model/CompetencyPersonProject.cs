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

using com.ccvonline.Residency.Data;

namespace com.ccvonline.Residency.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "_com_ccvonline_Residency_CompetencyPersonProject" )]
    [DataContract]
    public class CompetencyPersonProject : Model<CompetencyPersonProject>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the residency competency person id.
        /// </summary>
        /// <value>
        /// The residency competency person id.
        /// </value>
        [Required]
        [DataMember]
        public int CompetencyPersonId { get; set; }

        /// <summary>
        /// Gets or sets the residency project id.
        /// </summary>
        /// <value>
        /// The residency project id.
        /// </value>
        [Required]
        [DataMember]
        public int ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the overridden min assessment count 
        /// </summary>
        /// <value>
        /// The min assessment count override
        /// </value>
        [DataMember]
        public int? MinAssessmentCount { get; set; }
        
        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the residency competency person.
        /// </summary>
        /// <value>
        /// The residency competency person.
        /// </value>
        public virtual CompetencyPerson CompetencyPerson { get; set; }

        /// <summary>
        /// Gets or sets the residency project.
        /// </summary>
        /// <value>
        /// The residency project.
        /// </value>
        public virtual Project Project { get; set; }

        /// <summary>
        /// Gets or sets the competency person project assessments.
        /// </summary>
        /// <value>
        /// The competency person project assessments.
        /// </value>
        public virtual List<CompetencyPersonProjectAssessment> CompetencyPersonProjectAssessments { get; set; }

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
            return string.Format( "{0}, Project: {1}", CompetencyPerson, Project );
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class CompetencyPersonProjectConfiguration : EntityTypeConfiguration<CompetencyPersonProject>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompetencyPersonProjectConfiguration"/> class.
        /// </summary>
        public CompetencyPersonProjectConfiguration()
        {
            //// Note: SQL Server doesn't let us have a Cascade Delete from CompetencyPerson to CompetencyPersonProject, and from ResidencyProject to CompetencyPersonProject
            //// so just the CascadeDelete on ResidencyProject to CompetencyPersonProject, and we'll manually delete CompetencyPersonProjects when a CompetencyPerson is deleted 
            this.HasRequired( a => a.CompetencyPerson ).WithMany( a => a.CompetencyPersonProjects ).HasForeignKey( a => a.CompetencyPersonId ).WillCascadeOnDelete( false );
            this.HasRequired( a => a.Project ).WithMany().HasForeignKey( a => a.ProjectId ).WillCascadeOnDelete( true );
        }
    }
}
