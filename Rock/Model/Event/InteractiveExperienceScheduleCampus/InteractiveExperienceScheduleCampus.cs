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
    /// Represents a Interactive Schedule Campus.
    /// </summary>
    [RockDomain( "Event" )]
    [Table( "InteractiveExperienceScheduleCampus" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "ABEF4137-F25B-4B2E-AF01-2CEFF704FC11" )]
    public partial class InteractiveExperienceScheduleCampus : Model<InteractiveExperienceScheduleCampus>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.InteractiveExperienceSchedule"/> that this InteractiveExperienceScheduleCampus is associated with. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.InteractiveExperienceSchedule"/> that the InteractiveExperienceScheduleCampus is associated with.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int InteractiveExperienceScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Campus"/> associated with InteractiveExperienceSchedule.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> that represents the Id of the campus.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int CampusId { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.InteractiveExperienceSchedule"/> that the InteractiveExperienceScheduleCampus belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.InteractiveExperienceSchedule"/> representing the InteractiveExperienceSchedule that the InteractiveExperienceScheduleCampus is a part of.
        /// </value>
        [DataMember]
        public virtual InteractiveExperienceSchedule InteractiveExperienceSchedule { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Campus"/> that is associated with this InteractiveExperienceSchedule.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Campus"/> that is associated with this InteractiveExperienceSchedule.
        /// </value>
        [DataMember]
        public virtual Campus Campus { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Interactive Experience Schedule Campus Configuration class.
    /// </summary>
    public partial class InteractiveExperienceScheduleCampusConfiguration : EntityTypeConfiguration<InteractiveExperienceScheduleCampus>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveExperienceScheduleCampusConfiguration" /> class.
        /// </summary>
        public InteractiveExperienceScheduleCampusConfiguration()
        {
            this.HasRequired( sc => sc.InteractiveExperienceSchedule ).WithMany( s => s.InteractiveExperienceScheduleCampuses ).HasForeignKey( sc => sc.InteractiveExperienceScheduleId ).WillCascadeOnDelete( true );
            this.HasRequired( sc => sc.Campus ).WithMany().HasForeignKey( sc => sc.CampusId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
