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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Metric POCO Entity.
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "Metric" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "1C5489C6-82F9-4967-8425-52545CE8AF90")]
    public partial class Metric : Model<Metric>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this Metric is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if the Metric is part of the core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Title of this Metric.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the user defined title of this Metric. This property is required.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the Subtitle of the Metric.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Subtitle of the Metric.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string Subtitle { get; set; }

        /// <summary>
        /// Gets or sets a user defined description of the Metric.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the Metric.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [is cumulative].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [is cumulative]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsCumulative { get; set; }

        /// <summary>
        /// Gets or sets the source value type identifier.
        /// </summary>
        /// <value>
        /// The source value type identifier.
        /// </value>
        [DataMember]
        public int? SourceValueTypeId { get; set; }

        /// <summary>
        /// Gets or sets the SQL query that returns the data for the Metric.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> that represents the SQL Query that returns the data for the Metric.
        /// </value>
        [DataMember]
        public string SourceSql { get; set; }

        /// <summary>
        /// Gets or sets the Lava code that returns the data for the Metric.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> that represents the Lava code that returns the data for the Metric.
        /// </value>
        [DataMember]
        public string SourceLava { get; set; }

        /// <summary>
        /// Gets or sets the data view identifier.
        /// </summary>
        /// <value>
        /// The data view identifier.
        /// </value>
        [DataMember]
        public int? DataViewId { get; set; }

        /// <summary>
        /// Gets or sets the x axis label.
        /// Note that in Rock, graphs typically actually use the MetricValue.MetricValueDateTime as the graph's X Axis.
        /// Therefore, in most cases, Metric.XAxisLabel and MetricValue.XAxis are NOT used
        /// </summary>
        /// <value>
        /// The x axis label.
        /// </value>
        [DataMember]
        public string XAxisLabel { get; set; }

        /// <summary>
        /// Gets or sets the y axis label.
        /// </summary>
        /// <value>
        /// The y axis label.
        /// </value>
        [DataMember]
        public string YAxisLabel { get; set; }

        /// <summary>
        /// Gets or sets the metric champion person alias identifier.
        /// </summary>
        /// <value>
        /// The metric champion person alias identifier.
        /// </value>
        [DataMember]
        public int? MetricChampionPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the admin person alias identifier.
        /// </summary>
        /// <value>
        /// The admin person alias identifier.
        /// </value>
        [DataMember]
        public int? AdminPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the schedule identifier.
        /// </summary>
        /// <value>
        /// The schedule identifier.
        /// </value>
        [DataMember]
        public int? ScheduleId { get; set; }

        /// <summary>
        /// For SQL or DataView based Metrics, this is the DateTime that the MetricValues where scheduled to be updated according to Schedule
        /// </summary>
        /// <value>
        /// The last run date time.
        /// </value>
        [DataMember]
        public DateTime? LastRunDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable analytics].
        /// If this is enabled, a SQL View named 'AnalyticsFactMetric{{Metric.Name}}' will be made available that can be used by Analytic tools, such as Power BI
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable analytics]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool EnableAnalytics { get; set; }

        /// <summary>
        /// If set to true this feature will auto partition the individuals in the data view based on their primary campus.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [automatic partition on primary campus]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AutoPartitionOnPrimaryCampus { get; set; }

        /// <summary>
        /// Gets or sets the type of the unit.
        /// </summary>
        /// <value>
        /// The type of the unit.
        /// </value>
        [DataMember]
        public UnitType UnitType { get; set; }

        /// <summary>
        /// Gets or sets the measurement classification value identifier.
        /// </summary>
        /// <value>
        /// The measurement classification value identifier.
        /// </value>
        [DataMember]
        public int? MeasurementClassificationValueId { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the metric partitions.
        /// </summary>
        /// <value>
        /// The metric partitions.
        /// </value>
        [LavaVisible]
        public virtual ICollection<MetricPartition> MetricPartitions { get; set; }

        /// <summary>
        /// Gets or sets the metric values.
        /// </summary>
        /// <value>
        /// The metric values.
        /// </value>
        [LavaVisible]
        public virtual ICollection<MetricValue> MetricValues { get; set; }

        /// <summary>
        /// Gets or sets the type of the source value.
        /// </summary>
        /// <value>
        /// The type of the source value.
        /// </value>
        [DataMember]
        public virtual DefinedValue SourceValueType { get; set; }

        /// <summary>
        /// Gets or sets the data view.
        /// </summary>
        /// <value>
        /// The data view.
        /// </value>
        [LavaVisible]
        public virtual DataView DataView { get; set; }

        /// <summary>
        /// Gets or sets the metric champion person alias.
        /// </summary>
        /// <value>
        /// The metric champion person alias.
        /// </value>
        [LavaVisible]
        public virtual PersonAlias MetricChampionPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the admin person alias.
        /// </summary>
        /// <value>
        /// The admin person alias.
        /// </value>
        [LavaVisible]
        public virtual PersonAlias AdminPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the schedule.
        /// </summary>
        /// <value>
        /// The schedule.
        /// </value>
        [LavaVisible]
        public virtual Schedule Schedule { get; set; }

        /// <summary>
        /// Gets or sets the metric categories.
        /// </summary>
        /// <value>
        /// The metric categories.
        /// </value>
        [DataMember]
        public virtual ICollection<MetricCategory> MetricCategories { get; set; }

        /// <summary>
        /// Gets or sets the type of the numeric data that the values represent. Although all values
        /// are stored as a decimal, specifying the type here allows entry screens to use appropriate
        /// controls/validation when entering values.
        /// </summary>
        /// <value>
        /// The type of the numeric data.
        /// </value>
        [LavaVisible]
        public MetricNumericDataType NumericDataType { get; set; }

        /// <summary>
        /// Gets or sets the measurement classification value.
        /// </summary>
        /// <value>
        /// The measurement classification value.
        /// </value>
        [DataMember]
        public virtual DefinedValue MeasurementClassificationValue { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this Metric
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this Metric
        /// </returns>
        public override string ToString()
        {
            return this.Title;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Metric Configuration class.
    /// </summary>
    public partial class MetricConfiguration : EntityTypeConfiguration<Metric>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetricConfiguration"/> class.
        /// </summary>
        public MetricConfiguration()
        {
            this.HasOptional( p => p.SourceValueType ).WithMany().HasForeignKey( p => p.SourceValueTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.DataView ).WithMany().HasForeignKey( p => p.DataViewId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.CreatedByPersonAlias ).WithMany().HasForeignKey( p => p.CreatedByPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.MetricChampionPersonAlias ).WithMany().HasForeignKey( p => p.MetricChampionPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.AdminPersonAlias ).WithMany().HasForeignKey( p => p.AdminPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Schedule ).WithMany().HasForeignKey( p => p.ScheduleId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.MeasurementClassificationValue ).WithMany().HasForeignKey( p => p.MeasurementClassificationValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
