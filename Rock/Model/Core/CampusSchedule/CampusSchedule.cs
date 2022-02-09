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
using Rock.Lava;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a <see cref="Rock.Model.Schedule"/> that is associated with a <see cref="Rock.Model.Campus"/>.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "CampusSchedule" )]
    [DataContract]
    public partial class CampusSchedule : Model<CampusSchedule>, IOrdered, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Campus"/> that is associated with this CampusSchedule. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Campus"/> that this CampusSchedule is associated with.
        /// </value>
        [DataMember]
        public int CampusId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Schedule"/> that is associated with this CampusSchedule. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> referencing the Id of the <see cref="Rock.Model.Schedule"/> that is associated with this CampusSchedule. 
        /// </value>
        [DataMember]
        public int ScheduleId { get; set; }

        /// <summary>
        /// The Id of the ScheduleType <see cref="Rock.Model.DefinedValue"/> that is used to identify the type of <see cref="Rock.Model.CampusSchedule"/>
        /// that this is. This property is required.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> referencing the Id of the ScheduleType <see cref="Rock.Model.DefinedValue"/> that identifies the type of schedule that this is.
        /// If a ScheduleType <see cref="Rock.Model.DefinedValue"/> is not associated with this CampusSchedule this value will be null.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.SCHEDULE_TYPE )]
        public int? ScheduleTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the display order of the CampusSchedule in the campus schedule list. The lower the number the higher the 
        /// display priority this CampusSchedule has. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the display order of the CampusSchedule.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Campus"/> that is associated with this CampusSchedule.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Campus"/> that is associated with this CampusSchedule.
        /// </value>
        [LavaVisible]
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Schedule"/> that is associated with this CampusSchedule.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Schedule"/> that is associated with this CampusSchedule.
        /// </value>
        [DataMember]
        public virtual Schedule Schedule { get; set; }

        /// <summary>
        /// Gets or sets the Schedule Type <see cref="Rock.Model.DefinedValue"/> of this CampusSchedule.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.DefinedValue"/> that represents the Schedule Type of this CampusSchedule.
        /// </value>
        [DataMember]
        public virtual DefinedValue ScheduleTypeValue { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Schedule.ToStringSafe() + " at " + Campus.ToStringSafe();
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// CampusSchedule Configuration class
    /// </summary>
    public partial class CampusScheduleConfiguration : EntityTypeConfiguration<CampusSchedule>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CampusScheduleConfiguration"/> class.
        /// </summary>
        public CampusScheduleConfiguration()
        {
            this.HasRequired( t => t.Campus ).WithMany( t => t.CampusSchedules ).HasForeignKey( t => t.CampusId );
            this.HasRequired( t => t.Schedule );
            this.HasRequired( t => t.ScheduleTypeValue ).WithMany().HasForeignKey( t => t.ScheduleTypeValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}