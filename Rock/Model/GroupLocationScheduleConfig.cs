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
    /// Gets or sets properties that are specific to Group+Location+Schedule
    /// </summary>
    [RockDomain( "Group" )]
    [Table( "GroupLocationScheduleConfig" )]
    [DataContract]
    public class GroupLocationScheduleConfig
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the group location identifier.
        /// </summary>
        /// <value>
        /// The group location identifier.
        /// </value>
        [Key]
        [Column( Order = 1 )]
        [DataMember]
        public int GroupLocationId { get; set; }

        /// <summary>
        /// Gets or sets the schedule identifier.
        /// </summary>
        /// <value>
        /// The schedule identifier.
        /// </value>
        [Key]
        [Column( Order = 2 )]
        [DataMember]
        public int ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the minimum capacity.
        /// </summary>
        /// <value>
        /// The minimum capacity.
        /// </value>
        [DataMember]
        public int? MinimumCapacity { get; set; }

        /// <summary>
        /// Gets or sets the desired capacity.
        /// </summary>
        /// <value>
        /// The desired capacity.
        /// </value>
        [DataMember]
        public int? DesiredCapacity { get; set; }

        /// <summary>
        /// Gets or sets the maximum capacity.
        /// </summary>
        /// <value>
        /// The maximum capacity.
        /// </value>
        [DataMember]
        public int? MaximumCapacity { get; set; }

        #endregion Entity Properties

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the group location.
        /// </summary>
        /// <value>
        /// The group location.
        /// </value>
        [DataMember]
        public virtual GroupLocation GroupLocation { get; set; }

        /// <summary>
        /// Gets or sets the schedule.
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        [DataMember]
        public virtual Schedule Schedule { get; set; }

        #endregion Virtual Properties
    }

    /// <summary>
    /// GroupLocationSchedule EntityTypeConfiguration
    /// </summary>
    public class GroupLocationScheduleConfiguration : EntityTypeConfiguration<GroupLocationScheduleConfig>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupLocationScheduleConfiguration"/> class.
        /// </summary>
        public GroupLocationScheduleConfiguration()
        {
            this.HasRequired( a => a.GroupLocation ).WithMany( a => a.GroupLocationScheduleConfigs ).HasForeignKey( a => a.GroupLocationId ).WillCascadeOnDelete( false );
            this.HasRequired( a => a.Schedule ).WithMany().HasForeignKey( a => a.ScheduleId ).WillCascadeOnDelete( false );
        }
    }
}
