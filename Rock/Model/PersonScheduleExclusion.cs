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

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Blackout Dates for a Person defined for date range (and optional Group) that can be used to indicate when they are unavailable for scheduling
    /// </summary>
    [RockDomain( "Group" )]
    [Table( "PersonScheduleExclusion" )]
    [DataContract]
    public class PersonScheduleExclusion : Model<PersonScheduleExclusion>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the person alias identifier of the Person that this exclusion is for
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the title (optional)
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        /// <example>
        /// "On Summer Vacation"
        /// </example>
        [DataMember]
        [MaxLength( 100 )]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// The GroupId if there is a specific group for this exclusion.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        [DataMember]
        public int? GroupId { get; set; }

        /// <summary>
        /// Gets or sets the parent person schedule exclusion identifier.
        /// Use this to associate this exclusion with another PersonScheduleExclusion.
        /// This can be used support family based blackout dates (A person can indicate a blackout date and also include other members of their family).
        /// </summary>
        /// <value>
        /// The parent person schedule exclusion identifier.
        /// </value>
        [DataMember]
        public int? ParentPersonScheduleExclusionId { get; set; }

        #endregion Entity Properties

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the person alias of the Person that this exclusion is for
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// The Group if there is a specific group for this exclusion.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [DataMember]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the parent person schedule exclusion.
        /// </summary>
        /// <value>
        /// The parent person schedule exclusion.
        /// </value>
        [DataMember]
        public virtual PersonScheduleExclusion ParentPersonScheduleExclusion { get; set; }

        /// <summary>
        /// Gets or sets the child person schedule exclusions.
        /// </summary>
        /// <value>
        /// The child person schedule exclusions.
        /// </value>
        [DataMember]
        public virtual ICollection<PersonScheduleExclusion> ChildPersonScheduleExclusions { get; set; } = new Collection<PersonScheduleExclusion>();

        #endregion Virtual Properties
    }

    /// <summary>
    /// 
    /// </summary>
    public class PersonScheduleExclusionConfiguration : EntityTypeConfiguration<PersonScheduleExclusion>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonScheduleExclusionConfiguration"/> class.
        /// </summary>
        public PersonScheduleExclusionConfiguration()
        {
            this.HasRequired( a => a.PersonAlias ).WithMany().HasForeignKey( a => a.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.Group ).WithMany().HasForeignKey( a => a.GroupId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.ParentPersonScheduleExclusion ).WithMany( a => a.ChildPersonScheduleExclusions ).HasForeignKey( a => a.ParentPersonScheduleExclusionId ).WillCascadeOnDelete( false );
        }
    }
}
