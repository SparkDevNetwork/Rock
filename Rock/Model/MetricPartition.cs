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

namespace Rock.Model
{
    /// <summary>
    /// MetricPartition POCO Entity.
    /// </summary>
    [Table( "MetricPartition" )]
    [DataContract]
    public partial class MetricPartition : Model<MetricPartition>, IOrdered
    {
        #region Entity Properties

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
        /// Gets or sets the label.
        /// </summary>
        /// <value>
        /// The label.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the entity type identifier.
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is required.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is required; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier column that contains the value (see <see cref="EntityTypeQualifierValue"/>) that is used narrow the scope of the MetricPartition to a subset or specific instance of an EntityType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the Qualifier Column/Property that contains the <see cref="EntityTypeQualifierValue"/> that is used to narrow the scope of the MetricPartition.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier value that is used to narrow the scope of the Attribute to a subset or specific instance of an EntityType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the value that is used to narrow the scope of the Attribute.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string EntityTypeQualifierValue { get; set; }

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
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Metric Configuration class.
    /// </summary>
    public partial class MetricPartitionConfiguration : EntityTypeConfiguration<MetricPartition>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MetricPartitionConfiguration" /> class.
        /// </summary>
        public MetricPartitionConfiguration()
        {
            this.HasOptional( a => a.EntityType ).WithMany().HasForeignKey( a => a.EntityTypeId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.Metric ).WithMany( p => p.MetricPartitions ).HasForeignKey( p => p.MetricId ).WillCascadeOnDelete( true );
        }
    }

    #endregion
}
