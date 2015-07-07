// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

using Rock.Chart;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// MetricValue POCO Entity.
    /// </summary>
    [Table( "MetricValue" )]
    [DataContract]
    public partial class MetricValue : Model<MetricValue>, IOrdered, IChartData
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the type of the metric value.
        /// </summary>
        /// <value>
        /// The type of the metric value.
        /// </value>
        [DataMember]
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
        public decimal? YValue { get; set; }

        /// <summary>
        /// Gets or sets the Order.
        /// </summary>
        /// <value>
        /// Order.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

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
        public DateTime? MetricValueDateTime { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        [Index]
        [DataMember]
        public int? EntityId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the metric.
        /// </summary>
        /// <value>
        /// The metric.
        /// </value>
        public virtual Metric Metric { get; set; }

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
        /// Gets the series identifier.
        /// </summary>
        /// <value>
        /// The series identifier.
        /// </value>
        [DataMember]
        public string SeriesId
        {
            get
            {
                return string.Format( "{0}", this.EntityId ?? 0 );
            }
        }

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
            return this.XValue;
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