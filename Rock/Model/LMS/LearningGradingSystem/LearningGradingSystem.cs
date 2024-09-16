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

namespace Rock.Model
{
    /// <summary>
    /// Represents a grading system.
    /// </summary>
    [RockDomain( "LMS" )]
    [Table( "LearningGradingSystem" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( SystemGuid.EntityType.LEARNING_GRADING_SYSTEM )]
    public partial class LearningGradingSystem : Model<LearningGradingSystem>, IHasActiveFlag
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the grading system.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the GradingSystem.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the grading system.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the full description of the GradingSystem.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets a value indicating whether this grading system is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this GradingSystem is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.LearningClass">classes</see> using the grading system.
        /// </summary>
        /// <value>
        /// A collection of the <see cref="Rock.Model.LearningClass">classes</see> for the grading system.
        /// </value>
        public virtual ICollection<LearningClass> LearningClasses
        {
            get { return _learningClasses ?? ( _learningClasses = new Collection<LearningClass>() ); }
            set { _learningClasses = value; }
        }

        private ICollection<LearningClass> _learningClasses;

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.LearningGradingSystemScale">scales</see> used by the grading system.
        /// </summary>
        /// <value>
        /// A collection of the <see cref="Rock.Model.LearningGradingSystemScale">scales</see> used by the grading system.
        /// </value>
        public virtual ICollection<LearningGradingSystemScale> LearningGradingSystemScales
        {
            get { return _learningGradingSystemScales ?? ( _learningGradingSystemScales = new Collection<LearningGradingSystemScale>() ); }
            set { _learningGradingSystemScales = value; }
        }

        private ICollection<LearningGradingSystemScale> _learningGradingSystemScales;

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this LearningGradingSystem.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this LearningGradingSystem.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// LearningGradingSystem Configuration class.
    /// </summary>
    public partial class LearningGradingSystemConfiguration : EntityTypeConfiguration<LearningGradingSystem>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LearningGradingSystemConfiguration" /> class.
        /// </summary>
        public LearningGradingSystemConfiguration()
        {

        }
    }

    #endregion
}
