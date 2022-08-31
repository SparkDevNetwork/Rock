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
    /// Represents a Interactive Experience Schedule.
    /// </summary>
    [RockDomain( "Event" )]
    [Table( "InteractiveExperienceSchedule" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "D23B4DCF-545A-490F-AEAD-BA78A8FB4028" )]
    public partial class InteractiveExperienceSchedule : Model<InteractiveExperienceSchedule>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.InteractiveExperience"/> that this InteractiveExperienceSchedule is associated with. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.InteractiveExperience"/> that the InteractiveExperienceSchedule is associated with.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int InteractiveExperienceId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Schedule"/> that is associated with this Interactive Experience. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> referencing the Id of the <see cref="Rock.Model.Schedule"/> that is associated with this Interactive Experience. 
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DataView"/> identifier.
        /// </summary>
        /// <value>
        /// The data view identifier.
        /// </value>
        [DataMember]
        public int? DataViewId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group"/> identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        [DataMember]
        public int? GroupId { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.InteractiveExperience"/> that the InteractiveExperienceSchedule belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.InteractiveExperience"/> representing the InteractiveExperience that the InteractiveExperienceSchedule is a part of.
        /// </value>
        [DataMember]
        public virtual InteractiveExperience InteractiveExperience { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Schedule"/> that is associated with this Interactive Experience.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Schedule"/> that is associated with this Interactive Experience.
        /// </value>
        [DataMember]
        public virtual Schedule Schedule { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.DataView"/>.
        /// </summary>
        /// <value>
        /// The data view.
        /// </value>
        [DataMember]
        public virtual DataView DataView { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group"/>.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [DataMember]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.InteractiveExperienceScheduleCampus">InteractiveExperienceScheduleCampuses</see>  for this Interactive Experience Schedule.
        /// </summary>
        /// <value>
        /// A collection containing the <see cref="Rock.Model.InteractiveExperienceScheduleCampus" /> InteractiveExperienceScheduleCampuses for this Interactive Experience Schedule.
        /// </value>
        [DataMember]
        public virtual ICollection<InteractiveExperienceScheduleCampus> InteractiveExperienceScheduleCampuses
        {
            get { return _interactiveExperienceScheduleCampuses ?? ( _interactiveExperienceScheduleCampuses = new Collection<InteractiveExperienceScheduleCampus>() ); }
            set { _interactiveExperienceScheduleCampuses = value; }
        }
        private ICollection<InteractiveExperienceScheduleCampus> _interactiveExperienceScheduleCampuses;


        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Interactive Experience Schedule Configuration class.
    /// </summary>
    public partial class InteractiveExperienceScheduleConfiguration : EntityTypeConfiguration<InteractiveExperienceSchedule>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveExperienceScheduleConfiguration" /> class.
        /// </summary>
        public InteractiveExperienceScheduleConfiguration()
        {
            this.HasRequired( ies => ies.InteractiveExperience ).WithMany( ie => ie.InteractiveExperienceSchedules ).HasForeignKey( ies => ies.InteractiveExperienceId ).WillCascadeOnDelete( true );
            this.HasRequired( ies => ies.Schedule ).WithMany().HasForeignKey( ies => ies.ScheduleId ).WillCascadeOnDelete( false );
            this.HasOptional( ies => ies.DataView ).WithMany().HasForeignKey( ies => ies.DataViewId ).WillCascadeOnDelete( false );
            this.HasOptional( ies => ies.Group ).WithMany().HasForeignKey( ies => ies.GroupId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
