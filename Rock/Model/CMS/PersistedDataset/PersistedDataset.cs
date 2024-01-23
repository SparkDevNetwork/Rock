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
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a reporting dataset which is persisted in the database. These are typically
    /// used for resource-intensive queries that take a longer time to fetch. They are then
    /// cached and re-persisted according to the refresh interval.
    /// </summary>
    [RockDomain( "CMS" )]
    [Table( "PersistedDataset" )]
    [DataContract]
    [HideFromReporting]
    [Rock.SystemGuid.EntityTypeGuid( "9C3064C0-CF9C-4549-9A80-022514B7FF83")]
    public partial class PersistedDataset : Entity<PersistedDataset>, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the unique key to use to access this persisted dataset.
        /// </summary>
        /// <value>
        /// The access key.
        /// </value>
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        [HideFromReporting]
        [Index( IsUnique = true )]
        public string AccessKey { get; set; }

        /// <summary>
        /// Gets or sets the Name of the PersistedDataset.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the PersistedDataset.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a user defined description of the PersistedDataset.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the PersistedDataset.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the refresh interval minutes.
        /// </summary>
        /// <value>
        /// The refresh interval minutes.
        /// </value>
        [DataMember]
        public int? RefreshIntervalMinutes { get; set; }

        /// <summary>
        /// Gets or sets the persisted last refresh date time.
        /// </summary>
        /// <value>
        /// The persisted last refresh date time.
        /// </value>
        [DataMember]
        public DateTime? LastRefreshDateTime { get; set; }

        /// <summary>
        /// Gets or sets a comma-delimited list of enabled LavaCommands.
        /// </summary>
        /// <value>
        /// The enabled lava commands.
        /// </value>
        [DataMember]
        public string EnabledLavaCommands { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [allow manual refresh].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow manual refresh]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowManualRefresh { get; set; } = true;

        /// <summary>
        /// Gets or sets the serialized data of the dataset. See <seealso cref="ResultFormat"/>.
        /// </summary>
        /// <value>
        /// The result data.
        /// </value>
        [DataMember]
        public string ResultData { get; set; }

        /// <summary>
        /// Gets or sets the result format.
        /// </summary>
        /// <value>
        /// The result format.
        /// </value>
        [DataMember]
        public PersistedDatasetDataFormat ResultFormat { get; set; } = PersistedDatasetDataFormat.JSON;

        /// <summary>
        /// Gets or sets the memory cache duration ms.
        /// </summary>
        /// <value>
        /// The memory cache duration ms.
        /// </value>
        [DataMember]
        public int? MemoryCacheDurationMS { get; set; }

        /// <summary>
        /// Gets or sets the build script. See <seealso cref="BuildScriptType"/>.
        /// </summary>
        /// <value>
        /// The build script.
        /// </value>
        [DataMember]
        public string BuildScript { get; set; }

        /// <summary>
        /// Gets or sets the type of the build script.
        /// </summary>
        /// <value>
        /// The type of the build script.
        /// </value>
        [DataMember]
        public PersistedDatasetScriptType BuildScriptType { get; set; } = PersistedDatasetScriptType.Lava;

        /// <summary>
        /// Gets or sets a flag indicating that this PersistedDataset is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this PersistedDataset is part of the core system/framework, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// The amount of time that it took to persist the <see cref="ResultData"/>.
        /// </summary>
        /// <value>
        /// The time to build ms.
        /// </value>
        [DataMember]
        public double? TimeToBuildMS { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Web.Cache.EntityTypeCache"/> (<see cref="Rock.Model.EntityType"/>) that this PersistedDataset is used for.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.EntityType"/>
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// The DateTime when to stop updating the <see cref="ResultData"/>.
        /// </summary>
        /// <value>
        /// The expire date time.
        /// </value>
        [DataMember]
        public DateTime? ExpireDateTime { get; set; }

        /// <summary>
        /// Gets or sets the persisted schedule interval minutes.
        /// If this is null, then the DataView is not persisted.
        /// </summary>
        /// <value>
        /// The persisted schedule interval minutes.
        /// </value>
        [DataMember]
        public int? PersistedScheduleIntervalMinutes { get; set; }

        #endregion Entity Properties

        #region Navigation properties

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the ID of the schedule for this PersistedDataset.
        /// </summary>
        /// <value>
        /// The persisted schedule identifier.
        /// </value>
        [DataMember]
        public int? PersistedScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the schedule associated with this PersistedDataset.
        /// </summary>
        /// <value>
        /// A schedule entity.
        /// </value>
        public virtual Schedule PersistedSchedule { get; set; }

        #endregion
    }

    /// <summary>
    /// <see cref="PersistedDataset"/> configuration
    /// </summary>
    public partial class PesistedDatasetConfiguration : EntityTypeConfiguration<PersistedDataset>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PesistedDatasetConfiguration"/> class.
        /// </summary>
        public PesistedDatasetConfiguration()
        {
            this.HasOptional( a => a.EntityType ).WithMany().HasForeignKey( a => a.EntityTypeId ).WillCascadeOnDelete( false );

            this.HasOptional( a => a.PersistedSchedule ).WithMany().HasForeignKey( a => a.PersistedScheduleId ).WillCascadeOnDelete( false );
        }
    }
}
