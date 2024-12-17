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
    /// GroupMemberScheduleTemplate is the table used to make patterns that indicates the type of schedule a Scheduled GroupMember follows ( like Every Week, Every Other Week, etc )
    /// </summary>
    [RockDomain( "Group" )]
    [Table( "GroupMemberScheduleTemplate" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "D84ED719-B659-433C-BFA0-E798E52C6B24")]
    public partial class GroupMemberScheduleTemplate : Model<GroupMemberScheduleTemplate>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupType"/> that is allowed to use this template (or null if any GroupType can use it)
        /// </summary>
        /// <value>
        /// The group type identifier.
        /// </value>
        [DataMember]
        public int? GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Schedule"/>, which indicates the Schedule that a GroupMember is associated with (Every Week, Every Other Week, etc)
        /// </summary>
        /// <value>
        /// The schedule identifier.
        /// </value>
        [DataMember]
        public int ScheduleId { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupType"/> that is allowed to use this template (or null if any GroupType can use it)
        /// </summary>
        /// <value>
        /// The type of the group.
        /// </value>
        [DataMember]
        public virtual GroupType GroupType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Schedule"/>, which indicates the Schedule that a GroupMember is associated with (Every Week, Every Other Week, etc)
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        [DataMember]
        public virtual Schedule Schedule { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String"/> containing the GroupMemberScheduleTemplate's Name that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> containing the GroupMemberScheduleTemplate's Name that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion Public Methods
    }

    #region Entity Configuration

    /// <summary>
    /// Configuration Class
    /// </summary>
    public partial class GroupMemberScheduleTemplateConfiguration : EntityTypeConfiguration<GroupMemberScheduleTemplate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupMemberScheduleTemplateConfiguration" /> class.
        /// </summary>
        public GroupMemberScheduleTemplateConfiguration()
        {
            this.HasOptional( a => a.GroupType ).WithMany().HasForeignKey( a => a.GroupTypeId ).WillCascadeOnDelete( false );
            this.HasRequired( a => a.Schedule ).WithMany().HasForeignKey( a => a.ScheduleId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
