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
using Rock.Chart;
using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// MetricValue POCO Entity.
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "MetricValue" )]
    [DataContract]
    [CodeGenerateRest]
    [Rock.SystemGuid.EntityTypeGuid( "DD0E6F39-3E07-44D0-BE7B-B1AB75AFED2D")]
    public partial class MetricValue : Model<MetricValue>, IChartData
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the type of the metric value.
        /// </summary>
        /// <value>
        /// The type of the metric value.
        /// </value>
        [DataMember]
        [Previewable]
        public MetricValueType MetricValueType { get; set; }

        /// <summary>
        /// Gets or sets the X axis value.
        /// Note that in Rock, graphs typically actually use the MetricValue.MetricValueDateTime as the graph's X Axis.
        /// Therefore, in most cases, Metric.XAxisLabel and MetricValue.XAxis are NOT used
        /// </summary>
        /// <value>
        /// The x value.
        /// </value>
        [DataMember( IsRequired = false )]
        public string XValue { get; set; }

        /// <summary>
        /// Gets or sets the Y axis value.
        /// </summary>
        /// <value>
        /// The y value.
        /// </value>
        [DataMember]
        [IncludeForReporting]
        [Previewable]
        public decimal? YValue { get; set; }

        /// <summary>
        /// Gets or sets the MetricId.
        /// </summary>
        /// <value>
        /// MetricId.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int MetricId { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
        [DataMember]
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the metric value date time.
        /// </summary>
        /// <value>
        /// The metric value date time.
        /// </value>
#if REVIEW_WEBFORMS
        [Index]
#endif
        [DataMember]
        [Previewable]
        public DateTime? MetricValueDateTime { get; set; }

        /// <summary>
        /// Gets the metric value date key.
        /// </summary>
        /// <value>
        /// The metric value date key.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.DATE )]
        public int? MetricValueDateKey
        {
            get => ( MetricValueDateTime == null || MetricValueDateTime.Value == default ) ?
                        ( int? ) null :
                        MetricValueDateTime.Value.ToString( "yyyyMMdd" ).AsInteger();
            private set { }
        }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the metric.
        /// </summary>
        /// <value>
        /// The metric.
        /// </value>
        [LavaVisible]
        public virtual Metric Metric { get; set; }

        /// <summary>
        /// Gets or sets the metric value partitions.
        /// </summary>
        /// <value>
        /// The metric value partitions.
        /// </value>
        [DataMember]
        public virtual ICollection<MetricValuePartition> MetricValuePartitions
        {
            get { return _metricValuePartitions; }
            set { _metricValuePartitions = value; }
        }
        private ICollection<MetricValuePartition> _metricValuePartitions;

        /// <summary>
        /// Gets or sets the metric value source date.
        /// </summary>
        /// <value>
        /// The metric value source date.
        /// </value>
        [DataMember]
        public virtual AnalyticsSourceDate MetricValueSourceDate { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets or sets the name of the series. This will be the default name of the series if MetricValuePartitionEntityIds can't be resolved
        /// </summary>
        /// <value>
        /// The name of the series.
        /// </value>
        [DataMember]
        public string SeriesName
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.YValue.ToString();
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MetricValue"/> class.
        /// </summary>
        public MetricValue()
            : base()
        {
            _metricValuePartitions = new Collection<MetricValuePartition>();
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// MetricValue Configuration class.
    /// </summary>
    public partial class MetricValueConfiguration : EntityTypeConfiguration<MetricValue>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetricValueConfiguration"/> class.
        /// </summary>
        public MetricValueConfiguration()
        {
            this.HasRequired( p => p.Metric ).WithMany( p => p.MetricValues ).HasForeignKey( p => p.MetricId ).WillCascadeOnDelete( true );

            // NOTE: When creating a migration for this, don't create the actual FK's in the database for this just in case there are outlier OccurrenceDates that aren't in the AnalyticsSourceDate table
            // and so that the AnalyticsSourceDate can be rebuilt from scratch as needed
            this.HasOptional( r => r.MetricValueSourceDate ).WithMany().HasForeignKey( r => r.MetricValueDateKey ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}