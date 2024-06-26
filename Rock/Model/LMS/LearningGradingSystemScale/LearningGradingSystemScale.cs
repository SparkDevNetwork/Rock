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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a scale used by a grading system.
    /// </summary>
    [RockDomain( "LMS" )]
    [Table( "LearningGradingSystemScale" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( SystemGuid.EntityType.LEARNING_GRADING_SYSTEM_SCALE )]
    public partial class LearningGradingSystemScale : Model<LearningGradingSystemScale>, IOrdered
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the grading system scale.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the LearningGradingSystemScale.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the grading system scale.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the full description of the LearningGradingSystemScale.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// The percentage threshold for this scale.
        /// </summary>
        /// <value>
        /// A <see cref="System.Decimal"/> representing the percentage threshold for the LearningGradingSystemScale.
        /// </value>
        [DecimalPrecision( 8, 3 )]
        public decimal? ThresholdPercentage { get; set; }

        /// <summary>
        /// Gets a value indicating whether this grading system scale is considered passing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this scale is passing; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsPassing { get; set; }

        /// <summary>
        /// Gets or sets the order in which the scale should be displayed.
        /// </summary>
        /// <value>
        /// The <see cref="System.Int32"/> value representing the order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the id of the <see cref="Rock.Model.LearningGradingSystem"/> this scale belongs to.
        /// </summary>
        /// <value>
        /// The identifier for the related <see cref="Rock.Model.LearningGradingSystem"/>.
        /// </value>
        [DataMember]
        public int LearningGradingSystemId { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.LearningGradingSystem"/> this scale belongs to.
        /// </summary>
        [DataMember]
        public virtual LearningGradingSystem LearningGradingSystem { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this LearningGradingSystemScale.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this LearningGradingSystemScale.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// LearningGradingSystemScale Configuration class.
    /// </summary>
    public partial class LearningGradingSystemScaleConfiguration : EntityTypeConfiguration<LearningGradingSystemScale>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LearningGradingSystemScaleConfiguration" /> class.
        /// </summary>
        public LearningGradingSystemScaleConfiguration()
        {
            this.HasRequired( a => a.LearningGradingSystem ).WithMany( a => a.LearningGradingSystemScales ).HasForeignKey( a => a.LearningGradingSystemId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

}
