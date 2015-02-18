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
    [Table( "_church_ccv_Residency_Project")]
    [DataContract]
    public class Project : NamedModel<Project>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the residency competency id.
        /// </summary>
        /// <value>
        /// The residency competency id.
        /// </value>
        [Required]
        [DataMember]
        public int CompetencyId { get; set; }

        /// <summary>
        /// Gets or sets the min assessment count default.
        /// </summary>
        /// <value>
        /// The min assessment count default.
        /// </value>
        [DataMember]
        public int? MinAssessmentCountDefault { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the residency competency.
        /// </summary>
        /// <value>
        /// The residency competency.
        /// </value>
        public virtual Competency Competency { get; set; }

        /// <summary>
        /// Gets or sets the residency project point of assessments.
        /// </summary>
        /// <value>
        /// The residency project point of assessments.
        /// </value>
        public virtual List<ProjectPointOfAssessment> ProjectPointOfAssessments { get; set; }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class ProjectConfiguration : EntityTypeConfiguration<Project>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectConfiguration"/> class.
        /// </summary>
        public ProjectConfiguration()
        {
            this.HasRequired( p => p.Competency ).WithMany(a => a.Projects).HasForeignKey( p => p.CompetencyId ).WillCascadeOnDelete( true );
        }
    }
}
