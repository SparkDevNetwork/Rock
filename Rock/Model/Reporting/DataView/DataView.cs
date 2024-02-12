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
    /// Represents a filterable DataView in Rock.
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "DataView" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.DATAVIEW )]
    public partial class DataView : Model<DataView>, ICategorized
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this DataView is part of the Rock core system/framework.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if it is part of the Rock core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Name of the DataView.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Name of the DataView.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the user defined description of the DataView
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the DataView.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the CategoryId of the <see cref="Rock.Model.Category"/> that this DataView belongs to. If there is no Category, this value will be null.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the CategoryId of the <see cref="Rock.Model.Category"/> that this DataView belongs to. If it is not part of a Category this value will be null.
        /// </value>
        [DataMember]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> (Rock.Data.IEntity) that this DataView reports on.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that this DataView reports on.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the DataViewFilterId of the root/base <see cref="Rock.Model.DataViewFilter"/> that is used to generate this DataView. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> that represents the DataViewFilterId of the root/base <see cref="Rock.Model.DataViewFilter"/> that is used to generate this DataView. If there is 
        /// not a filter on this DataView, this value will be null.
        /// </value>
        [DataMember]
        public int? DataViewFilterId { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> (MEF Component) that is used for an optional transformation on this DataView.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that is used for an optional transformation on this DataView. If there
        /// is not a transformation on this DataView, this value will be null.
        /// </value>
        [DataMember]
        public int? TransformEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the persisted schedule interval minutes.
        /// If this is null, then the DataView is not persisted.
        /// </summary>
        /// <value>
        /// The persisted schedule interval minutes.
        /// </value>
        [DataMember]
        public int? PersistedScheduleIntervalMinutes { get; set; }

        /// <summary>
        /// Gets or sets the persisted last refresh date time.
        /// </summary>
        /// <value>
        /// The persisted last refresh date time.
        /// </value>
        [DataMember]
        public DateTime? PersistedLastRefreshDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether deceased should be included.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include deceased]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IncludeDeceased { get; set; }

        /// <summary>
        /// Gets or sets the persisted last run duration in milliseconds.
        /// </summary>
        /// <value>
        /// The persisted last run duration in milliseconds.
        /// </value>
        [DataMember]
        public int? PersistedLastRunDurationMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the last run date time.
        /// </summary>
        /// <value>
        /// The last run date time.
        /// </value>
        [DataMember]
        public DateTime? LastRunDateTime { get; set; }

        /// <summary>
        /// Gets or sets the run count.
        /// </summary>
        /// <value>
        /// The run count.
        /// </value>
        [DataMember]
        public int? RunCount { get; set; }

        /// <summary>
        /// The amount of time in milliseconds that it took to run the <see cref="DataView"/>
        /// </summary>
        /// <value>
        /// The time to run in ms.
        /// </value>
        [DataMember]
        public double? TimeToRunDurationMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the DateTime that the Run Count was last reset to 0.
        /// </summary>
        /// <value>
        /// The run count last refresh date time.
        /// </value>
        [DataMember]
        public DateTime? RunCountLastRefreshDateTime { get; set; }

        /// <summary>
        /// Gets or sets whether using a read-only Rock Context is disabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [disable use of read-only]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool DisableUseOfReadOnlyContext { get; set; }

        /// <summary>
        /// Gets or sets the Persisted Schedule Id.
        /// If this is null, then the DataView does not have a persisted schedule.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Schedule"/> for this DataView.
        /// If it does not have a persisted schedule, this value will be null.
        /// </value>
        [DataMember]
        public int? PersistedScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class for entities returned by this DataView.
        /// </summary>
        [MaxLength( 100 )]
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the highlight color for the <see cref="IconCssClass"/>.
        /// </summary>
        [MaxLength( 50 )]
        [DataMember]
        public string HighlightColor { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Category"/> that this DataView belongs to
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Category"/> that this DataView belongs to.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> (Rock.Data.IEntity) that this DataView reports on.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> that this DataView reports on.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the base <see cref="Rock.Model.DataViewFilter"/> that is used to generate this DataView.
        /// </summary>
        /// <value>
        /// The base <see cref="Rock.Model.DataViewFilter"/>.
        /// </value>
        [DataMember]
        public virtual DataViewFilter DataViewFilter { get; set; }

        /// <summary>
        /// Gets or sets the entity type (MEF Component) used for an optional transformation
        /// </summary>
        /// <value>
        /// The transformation type of entity.
        /// </value>
        [DataMember]
        public virtual EntityType TransformEntityType { get; set; }

        /// <summary>
        /// Gets or sets the persisted <see cref="Rock.Model.Schedule"/> that belongs to this DataView.
        /// </summary>
        /// <value>
        /// The persisted <see cref="Rock.Model.Schedule"/> that belongs to this DataView.
        /// </value>
        [DataMember]
        public virtual Schedule PersistedSchedule { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// DataView Configuration class.
    /// </summary>
    public partial class DataViewConfiguration : EntityTypeConfiguration<DataView>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReportConfiguration"/> class.
        /// </summary>
        public DataViewConfiguration()
        {
            this.HasOptional( v => v.Category ).WithMany().HasForeignKey( v => v.CategoryId ).WillCascadeOnDelete( false );
            this.HasOptional( v => v.DataViewFilter ).WithMany().HasForeignKey( v => v.DataViewFilterId ).WillCascadeOnDelete( true );
            this.HasRequired( v => v.EntityType ).WithMany().HasForeignKey( v => v.EntityTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( e => e.TransformEntityType ).WithMany().HasForeignKey( e => e.TransformEntityTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( s => s.PersistedSchedule ).WithMany().HasForeignKey( s => s.PersistedScheduleId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}