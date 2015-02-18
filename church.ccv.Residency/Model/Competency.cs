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

using Rock.Data;

namespace church.ccv.Residency.Model
{
    [Table( "_church_ccv_Residency_Competency" )]
    [DataContract]
    public class Competency : NamedModel<Competency>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the residency track id.
        /// </summary>
        /// <value>
        /// The residency track id.
        /// </value>
        [Required]
        [DataMember]
        public int TrackId { get; set; }

        /// <summary>
        /// Gets or sets the teacher of record person id.
        /// </summary>
        /// <value>
        /// The teacher of record person id.
        /// </value>
        [DataMember]
        public int? TeacherOfRecordPersonId { get; set; }

        /// <summary>
        /// Gets or sets the facilitator person id.
        /// </summary>
        /// <value>
        /// The facilitator person id.
        /// </value>
        [DataMember]
        public int? FacilitatorPersonId { get; set; }

        /// <summary>
        /// Gets or sets the goals.
        /// </summary>
        /// <value>
        /// The goals.
        /// </value>
        [DataMember]
        public string Goals { get; set; }

        /// <summary>
        /// Gets or sets the credit hours.
        /// </summary>
        /// <value>
        /// The credit hours.
        /// </value>
        [DataMember]
        public int? CreditHours { get; set; }

        /// <summary>
        /// Gets or sets the supervision hours.
        /// </summary>
        /// <value>
        /// The supervision hours.
        /// </value>
        [DataMember]
        public int? SupervisionHours { get; set; }

        /// <summary>
        /// Gets or sets the implementation hours.
        /// </summary>
        /// <value>
        /// The implementation hours.
        /// </value>
        [DataMember]
        public int? ImplementationHours { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the residency track.
        /// </summary>
        /// <value>
        /// The residency track.
        /// </value>
        public virtual Track Track { get; set; }

        /// <summary>
        /// Gets or sets the teacher of record person.
        /// </summary>
        /// <value>
        /// The teacher of record person.
        /// </value>
        public virtual Rock.Model.Person TeacherOfRecordPerson { get; set; }

        /// <summary>
        /// Gets or sets the facilitator person.
        /// </summary>
        /// <value>
        /// The facilitator person.
        /// </value>
        public virtual Rock.Model.Person FacilitatorPerson { get; set; }

        /// <summary>
        /// Gets or sets the residency projects.
        /// </summary>
        /// <value>
        /// The residency projects.
        /// </value>
        public virtual List<Project> Projects { get; set; }
        
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class CompetencyConfiguration : EntityTypeConfiguration<Competency>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompetencyConfiguration"/> class.
        /// </summary>
        public CompetencyConfiguration()
        {
            this.HasRequired( p => p.Track ).WithMany( p => p.Competencies ).HasForeignKey( p => p.TrackId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.TeacherOfRecordPerson ).WithMany().HasForeignKey( p => p.TeacherOfRecordPersonId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.FacilitatorPerson ).WithMany().HasForeignKey( p => p.FacilitatorPersonId ).WillCascadeOnDelete( false );
        }
    }
}
