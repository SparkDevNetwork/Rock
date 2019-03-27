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
using System.Linq;
using System.Runtime.Serialization;

using Rock.Chart;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// MetricValue POCO Entity.
    /// </summary>
    [RockDomain( "Reporting" )]
    [Table( "MetricValue" )]
    [DataContract]
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
        [Index]
        [DataMember]
        [Previewable]
        public DateTime? MetricValueDateTime { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the metric.
        /// </summary>
        /// <value>
        /// The metric.
        /// </value>
        [LavaInclude]
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

        #endregion

        #region Methods

        /// <summary>
        /// Gets the metric value datetime as a javascript time stamp (handy for chart apis)
        /// </summary>
        /// <value>
        /// The metric value javascript time stamp.
        /// </value>
        [DataMember]
        public long DateTimeStamp
        {
            get
            {
                if ( this.MetricValueDateTime.HasValue )
                {
                    return this.MetricValueDateTime.Value.ToJavascriptMilliseconds();
                }
                else
                {
                    return 0;
                }
            }
        }

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
        /// Gets the metric value partitions as a comma-delimited list of EntityTypeId|EntityId
        /// </summary>
        /// <value>
        /// The metric value entityTypeId,EntityId partitions
        /// </value>
        [DataMember]
        public virtual string MetricValuePartitionEntityIds
        {
            get
            {
                if ( _metricValuePartitionEntityIds == null )
                {
                    var list = this.MetricValuePartitions.Select( a => new { a.MetricPartition.EntityTypeId, a.EntityId } ).ToList();
                    _metricValuePartitionEntityIds = list.Select( a => string.Format( "{0}|{1}", a.EntityTypeId, a.EntityId ) ).ToList().AsDelimited( "," );
                }

                return _metricValuePartitionEntityIds;
            }
        }

        private string _metricValuePartitionEntityIds;

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                return this.Metric != null ? this.Metric : base.ParentAuthority;
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
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// The type of Metric Value that a Metric Value represents
    /// </summary>
    public enum MetricValueType
    {
        /// <summary>
        /// Metric Value represents something that was measured (for example: Fundraising Total)
        /// </summary>
        Measure = 0,

        /// <summary>
        /// Metric Value represents a goal (for example: Fundraising Goal)
        /// </summary>
        Goal = 1
    }

    #endregion
}