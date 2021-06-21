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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

using Rock.Data;
using Rock.Security;
using Rock.UniversalSearch;
using Rock.UniversalSearch.IndexModels;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "Group" )]
    [Table( "GroupDemographicType" )]
    [DataContract]
    public partial class GroupDemographicType : Model<GroupDemographicType>
    {
        #region Entity Properties

        /// <summary>
        /// The <see cref="Rock.Model.GroupType"/> identifier of the group this Group Demographic Type is associated with.
        /// </summary>
        /// <value>
        /// The group type identifier.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
        public int GroupTypeId { get; set; }

        /// <summary>
        /// The name of the Group Demographic Type. Previewable.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
        [MaxLength( 100 )]
        [Previewable]
        public string Name { get; set; }

        /// <summary>
        /// The description of the Group Demographic Type. Previewable.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        [Previewable]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the component entity type identifier. This is an FK of EntityType.Id.
        /// </summary>
        /// <value>
        /// The component entity type identifier.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
        public int ComponentEntityTypeId { get; set; }

        /// <summary>
        /// A comma delimited list of <see cref="Rock.Model.GroupTypeRole">GroupTypeRoles</see> IDs
        /// </summary>
        /// <value>
        /// The role filter.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string RoleFilter { get; set; }

        /// <summary>
        /// Specify if this GroupDemographicType is automated. If true the UI will not allow manual entry.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is automated; otherwise, <c>false</c>.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
        public bool IsAutomated { get; set; } = false;

        /// <summary>
        /// How long a component took to get its values in seconds.
        /// </summary>
        /// <value>
        /// The duration of the calculation.
        /// </value>
        [DataMember]
        public int? LastRunDurationSeconds { get; set; }

        /// <summary>
        /// Indicates if the component for this Group Demographic Type should be run everytime a person is updated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [run on person update]; otherwise, <c>false</c>.
        /// </value>
        [DataMember( IsRequired = true)]
        [Required]
        [HideFromReporting]
        public bool RunOnPersonUpdate { get; set; } = false;

        #endregion Entity Properties

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.GroupType">type</see> of the group.
        /// </summary>
        /// <value>
        /// The type of the group.
        /// </value>
        [DataMember]
        public virtual GroupType GroupType { get; set; }

        /// <summary>
        /// Gets or sets the type of the component entity.
        /// </summary>
        /// <value>
        /// The type of the component entity.
        /// </value>
        [DataMember]
        public virtual EntityType ComponentEntityType { get; set; }

        #endregion VirtualProperties


    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class GroupDemographicTypeConfiguration : EntityTypeConfiguration<GroupDemographicType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupDemographicTypeConfiguration"/> class.
        /// </summary>
        public GroupDemographicTypeConfiguration()
        {
            this.HasRequired( x => x.GroupType ).WithMany().HasForeignKey( x => x.GroupTypeId ).WillCascadeOnDelete( true );
            this.HasRequired( x => x.ComponentEntityType ).WithMany().HasForeignKey( b => b.ComponentEntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
