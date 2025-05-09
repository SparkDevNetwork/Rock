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
    /// Represents a activity or task for class.
    /// </summary>
    [RockDomain( "LMS" )]
    [Table( "LearningActivity" )]
    [DataContract]
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( SystemGuid.EntityType.LEARNING_ACTIVITY )]
    public partial class LearningActivity : Model<LearningActivity>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the activity.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the <see cref="Model.LearningActivity"/>.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the activity.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the full description of the <see cref="Model.LearningActivity"/>.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// The id of the related <see cref="EntityType"/> that handles logic for this <see cref="Model.LearningActivity"/>.
        /// </summary>
        /// <value>
        /// The identifier of the related <see cref="EntityType"/> ActivityComponent.
        /// </value>
        [DataMember]
        public int ActivityComponentId { get; set; }

        /// <summary>
        /// Gets or sets the json config for the activity component before completion.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the json configuration of the activity component before completion.
        /// </value>
        [DataMember]
        public string ActivityComponentSettingsJson { get; set; }

        /// <summary>
        /// Indicates whether or not this activity is intended to be shared
        /// as a template by multiple <see cref="LearningClassActivity"/>.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this activity is intended to be shared as a template; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsShared { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="EntityType"/> for the activity.
        /// </summary>
        [DataMember]
        public virtual EntityType ActivityComponent { get; set; }

        /// <summary>
        /// Gets or sets a collection of <see cref="Rock.Model.LearningClassActivity">activities</see> for the activity.
        /// </summary>
        /// <value>
        /// A collection of the <see cref="Rock.Model.LearningClassActivity"/> records for the activity.
        /// </value>
        public virtual ICollection<LearningClassActivity> LearningClassActivities
        {
            get { return _learningClassActivities ?? ( _learningClassActivities = new Collection<LearningClassActivity>() ); }
            set { _learningClassActivities = value; }
        }

        private ICollection<LearningClassActivity> _learningClassActivities;

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this activity.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this <see cref="Model.LearningClassActivity"/>.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// <see cref="Model.LearningActivity"/> Configuration class.
    /// </summary>
    public partial class LearningActivityConfiguration : EntityTypeConfiguration<LearningActivity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LearningActivityConfiguration" /> class.
        /// </summary>
        public LearningActivityConfiguration()
        {
            this.HasRequired( a => a.ActivityComponent ).WithMany().HasForeignKey( a => a.ActivityComponentId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
