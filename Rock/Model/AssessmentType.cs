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
using System.Text;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents an assessment.
    /// </summary>
    [RockDomain( "CRM" )]
    [Table( "AssessmentType" )]
    [DataContract]
    public class AssessmentType : Model<AssessmentType>, IHasActiveFlag
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Title of the <see cref="Rock.Model.AssessmentType"/>  
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> for the Title.
        /// </value>
        [Required]
        [MaxLength(100)]
        [DataMember( IsRequired = true )]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the Description of the <see cref="Rock.Model.AssessmentType"/>
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> for the Decsription.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the AssessmentPath of the <see cref="Rock.Model.AssessmentType"/>  
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> for the AssessmentPath.
        /// </value>
        [Required]
        [MaxLength( 250 )]
        [DataMember( IsRequired = true )]
        public string AssessmentPath { get; set; }

        /// <summary>
        /// Gets or sets the AssessmentResultsPath of the <see cref="Rock.Model.Assessment"/> or the <see cref="Rock.Model.AssessmentType"/> if no requestor required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> for the AssessmentResultsPath
        /// </value>
        [MaxLength( 250 )]
        [DataMember]
        public string AssessmentResultsPath { get; set; }

        /// <summary>
        /// Gets or sets the IsActive flag for the <see cref="Rock.Model.AssessmentType"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> for the IsActive flag.
        /// </value>
        [Required]
        [DataMember]
        public Boolean IsActive { get; set; }

        /// <summary>
        /// Gets or sets the RequiresRequest flag for the <see cref="Rock.Model.AssessmentType"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> for the RequiresRequest.
        /// </value>
        [Required]
        [DataMember]
        public Boolean RequiresRequest { get; set; }

        /// <summary>
        /// Gets or sets the number of days given for the <see cref="Rock.Model.AssessmentType"/>. to be retaken.
        /// </summary>
        /// <value>
        /// A <see cref="System.int"/> for the minimum days allowed to retake the <see cref="Rock.Model.AssessmentType"/>.
        /// </value>
        [DataMember]
        public int MinimumDaysToRetake { get; set; }

        /// <summary>
        /// Gets or sets the number for valid duration of the <see cref="Rock.Model.AssessmentType"/>.
        /// How long is this assessment valid before it must be taken again.
        /// </summary>
        /// <value>
        /// A <see cref="System.int"/> for the valid duration of the <see cref="Rock.Model.AssessmentType"/>..
        /// </value>
        [DataMember]
        public int ValidDuration { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this <see cref="Rock.Model.AssessmentType"/> is a part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this <see cref="Rock.Model.AssessmentType"/>. is part of the Rock core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the Collection of Assessments for each <see cref="Rock.Model.AssessmentType"/>.
        /// </summary>
        [DataMember]
        public virtual ICollection<Assessment> Assessments { get; set; } = new Collection<Assessment>();

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance Title.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance Title.
        /// </returns>
        public override string ToString()
        {
            return this.Title;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// AssessmentType Configuration class.
    /// </summary>
    public partial class AssessmentTypeConfiguration : EntityTypeConfiguration<AssessmentType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssessmentTypeConfiguration" /> class.
        /// </summary>
        public AssessmentTypeConfiguration()
        { 
        }
    }

    #endregion
}
