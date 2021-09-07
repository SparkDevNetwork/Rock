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

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Stores the values for a group and demographic type
    /// </summary>
    [RockDomain( "Group" )]
    [Table( "GroupDemographicValue" )]
    [DataContract]
    public partial class GroupDemographicValue : Model<GroupDemographicValue>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Group ID that this GroupDemographicValue is for.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the GroupDemographicType ID that this GroupDemographicValue is for.
        /// </summary>
        /// <value>
        /// The group demographic type identifier.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
        public int GroupDemographicTypeId { get; set; }

        /// <summary>
        /// Gets or sets the related EntityTypeID this value if for. e.g. DefinedValue.
        /// </summary>
        /// <value>
        /// The related entity type identifier.
        /// </value>
        [DataMember]
        public int? RelatedEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the related entity identifier. e.g. the ID of the DefinedValue
        /// </summary>
        /// <value>
        /// The related entity identifier.
        /// </value>
        [DataMember]
        public int? RelatedEntityId { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [DataMember]
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the last date and time when this GroupDemographicValue was calculated.
        /// </summary>
        /// <value>
        /// The last calculated date time.
        /// </value>
        [DataMember]
        public DateTime? LastCalculatedDateTime { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        [DataMember]
        public virtual Group Group { get; set; }

        /// <summary>
        /// Gets or sets the type of the group demographic.
        /// </summary>
        /// <value>
        /// The type of the group demographic.
        /// </value>
        [DataMember]
        public virtual GroupDemographicType GroupDemographicType { get; set; }

        /// <summary>
        /// Gets or sets the type of the related entity.
        /// </summary>
        /// <value>
        /// The type of the related entity.
        /// </value>
        [DataMember]
        public virtual EntityType RelatedEntityType { get; set; }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class GroupDemographicValueConfiguration : EntityTypeConfiguration<GroupDemographicValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupDemographicValueConfiguration"/> class.
        /// </summary>
        public GroupDemographicValueConfiguration()
        {
            this.HasRequired( x => x.Group ).WithMany().HasForeignKey( x => x.GroupId ).WillCascadeOnDelete( true );
            this.HasRequired( x => x.GroupDemographicType ).WithMany().HasForeignKey( x => x.GroupDemographicTypeId ).WillCascadeOnDelete( true );
            this.HasOptional( x => x.RelatedEntityType ).WithMany().HasForeignKey( x => x.RelatedEntityTypeId ).WillCascadeOnDelete( false );
        }
    }

}
