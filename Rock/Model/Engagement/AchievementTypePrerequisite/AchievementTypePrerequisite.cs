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
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Achievement Type Prerequisite in Rock.
    /// </summary>
    [RockDomain( "Engagement" )]
    [Table( "AchievementTypePrerequisite" )]
    [DataContract]
    [CodeGenerateRest( DisableEntitySecurity = true )]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.ACHIEVEMENT_TYPE_PREREQUISITE )]
    public partial class AchievementTypePrerequisite : Model<AchievementTypePrerequisite>, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.AchievementType"/> to which this prerequisite belongs. This property is required.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int AchievementTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.AchievementType"/> that is the prerequisite. This property is required.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int PrerequisiteAchievementTypeId { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.AchievementType"/>.
        /// </summary>
        [DataMember]
        public virtual AchievementType AchievementType { get; set; }

        /// <summary>
        /// Gets or sets the Prerequisite <see cref="Rock.Model.AchievementType"/>.
        /// </summary>
        [DataMember]
        public virtual AchievementType PrerequisiteAchievementType { get; set; }

        #endregion Navigation Properties

        

        #region Entity Configuration

        /// <summary>
        /// Configuration class.
        /// </summary>
        public partial class AchievementTypePrerequisiteConfiguration : EntityTypeConfiguration<AchievementTypePrerequisite>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AchievementTypePrerequisiteConfiguration"/> class.
            /// </summary>
            public AchievementTypePrerequisiteConfiguration()
            {
                HasRequired( statp => statp.AchievementType )
                    .WithMany( stat => stat.Prerequisites )
                    .HasForeignKey( statp => statp.AchievementTypeId )
                    .WillCascadeOnDelete( true );

                HasRequired( statp => statp.PrerequisiteAchievementType )
                    .WithMany( stat => stat.Dependencies )
                    .HasForeignKey( statp => statp.PrerequisiteAchievementTypeId )
                    // This has to be false because otherwise SQL server doesn't like the possibility of dependency cycles
                    .WillCascadeOnDelete( false );
            }
        }

        #endregion
    }
}
