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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// MetricValuePartition POCO Entity.
    /// </summary>
    [Table( "MetricValuePartition" )]
    [DataContract]
    public partial class MetricValuePartition : Model<MetricValuePartition>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the metric partition identifier.
        /// </summary>
        /// <value>
        /// The metric partition identifier.
        /// </value>
        [DataMember]
        public int? MetricPartitionId { get; set; }

        /// <summary>
        /// Gets or sets the metric value identifier.
        /// </summary>
        /// <value>
        /// The metric value identifier.
        /// </value>
        [DataMember]
        [IgnoreCanDelete]
        public int? MetricValueId { get; set; }

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
        /// Gets or sets the metric partition.
        /// </summary>
        /// <value>
        /// The metric partition.
        /// </value>
        [LavaInclude]
        public virtual MetricPartition MetricPartition { get; set; }

        /// <summary>
        /// Gets or sets the metric value.
        /// </summary>
        /// <value>
        /// The metric value.
        /// </value>
        [LavaInclude]
        public virtual MetricValue MetricValue { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Metric Configuration class.
    /// </summary>
    public partial class MetricValuePartitionConfiguration : EntityTypeConfiguration<MetricValuePartition>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetricValuePartitionConfiguration"/> class.
        /// </summary>
        public MetricValuePartitionConfiguration()
        {
            this.HasRequired( p => p.MetricPartition ).WithMany().HasForeignKey( p => p.MetricPartitionId ).WillCascadeOnDelete( true );

            // have to set Cascade to false due to 'may cause cycles or multiple cascade paths'
            this.HasRequired( p => p.MetricValue ).WithMany( a => a.MetricValuePartitions ).HasForeignKey( p => p.MetricValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
