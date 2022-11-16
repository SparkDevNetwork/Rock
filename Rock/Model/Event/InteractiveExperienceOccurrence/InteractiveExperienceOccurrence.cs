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
    [Table( "InteractiveExperienceOccurrence" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "2D1263A1-A3E7-4568-AA4B-C1234824188D" )]
    public partial class InteractiveExperienceOccurrence : Model<InteractiveExperienceOccurrence>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.InteractiveExperienceSchedule"/> that this InteractiveExperienceOccurrence is associated with. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.InteractiveExperienceSchedule"/> that the InteractiveExperienceOccurrence is associated with.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Index( "IX_InteractiveExperienceScheduleIdCampusIdOccurrenceDateTime", IsUnique = true, Order = 0 )]
        public int InteractiveExperienceScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Campus"/> that is associated with this Interactive Experience Occurrence.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> referencing the Id of the <see cref="Rock.Model.Campus"/> that is associated with this Interactive Experience Occurrence. 
        /// </value>
        [DataMember]
        [Index( "IX_InteractiveExperienceScheduleIdCampusIdOccurrenceDateTime", IsUnique = true, Order = 1 )]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> for when this occurrence started.
        /// </summary>
        /// <value>
        /// The <see cref="DateTime"/> for when this occurrence started.
        /// </value>
        [DataMember]
        [Index( "IX_OccurrenceDateTime" )]
        [Index( "IX_InteractiveExperienceScheduleIdCampusIdOccurrenceDateTime", IsUnique = true, Order = 2 )]
        public DateTime OccurrenceDateTime { get; set; }

        /// <summary>
        /// Gets the occurrence date key used for indexing. Only the date portion
        /// of <see cref="OccurrenceDateTime"/> is used when calculating this.
        /// </summary>
        /// <value>
        /// The occurrence date key used for indexing.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.DATE )]
        public int OccurrenceDateKey
        {
            get => OccurrenceDateTime.ToString( "yyyyMMdd" ).AsInteger();
            private set { /* Required for EF. */ }
        }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.InteractiveExperienceAction"/> identifier
        /// that is currently being displayed.
        /// </summary>
        /// <value>
        /// The action identifier currently being displayed.
        /// </value>
        [DataMember]
        public int? CurrentlyShownActionId { get; set; }


        /// <summary>
        /// Gets or sets the state json. This is used to store general state
        /// information about this occurrence that will be defined later.
        /// </summary>
        /// <value>The state json.</value>
        [DataMember]
        public string StateJson { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.InteractiveExperienceSchedule"/> that the InteractiveExperienceOccurrence belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.InteractiveExperienceSchedule"/> that the InteractiveExperienceOccurrence belongs to.
        /// </value>
        [DataMember]
        public virtual InteractiveExperienceSchedule InteractiveExperienceSchedule { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Campus"/> that is associated with this Interactive Experience Occurrence.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Campus"/> that is associated with this Interactive Experience Occurrence.
        /// </value>
        [DataMember]
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.InteractiveExperienceAction"/> that
        /// is currently being displayed.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.InteractiveExperienceAction"/> that is currently
        /// being displayed.
        /// </value>
        [DataMember]
        public virtual InteractiveExperienceAction CurrentlyShownAction { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Interactive Experience Occurrence Configuration class.
    /// </summary>
    public partial class InteractiveExperienceOccurrenceConfiguration : EntityTypeConfiguration<InteractiveExperienceOccurrence>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveExperienceOccurrenceConfiguration" /> class.
        /// </summary>
        public InteractiveExperienceOccurrenceConfiguration()
        {
            this.HasRequired( ieo => ieo.InteractiveExperienceSchedule ).WithMany( ies => ies.InteractiveExperienceOccurrences ).HasForeignKey( ieo => ieo.InteractiveExperienceScheduleId ).WillCascadeOnDelete( true );
            this.HasOptional( ieo => ieo.Campus ).WithMany().HasForeignKey( ieo => ieo.CampusId ).WillCascadeOnDelete( false );
            this.HasOptional( ieo => ieo.CurrentlyShownAction ).WithMany().HasForeignKey( ieo => ieo.CurrentlyShownActionId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
