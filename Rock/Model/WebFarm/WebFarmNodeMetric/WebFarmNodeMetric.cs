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
using Newtonsoft.Json;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents WebFarmNodeMetric in Rock.
    /// </summary>
    [RockDomain( "WebFarm" )]
    [Table( "WebFarmNodeMetric" )]
    [DataContract]
    [CodeGenerateRest( Enums.CodeGenerateRestEndpoint.ReadOnly, DisableEntitySecurity = true )]
    [Rock.SystemGuid.EntityTypeGuid( "3194A5E3-F352-4CD0-A585-F1DD37008B9C")]
    public partial class WebFarmNodeMetric : Model<WebFarmNodeMetric>
    {
        private const string IndexName = "IX_WebFarmNode_MetricType_MetricValueDateTime";

        #region Entity Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WebFarmNode"/> identifier.
        /// </summary>
        /// <value>
        /// The web farm node identifier.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
#if REVIEW_WEBFORMS
        [Index( IndexName, Order = 1 )]
#endif
        public int WebFarmNodeId { get; set; }

        /// <summary>
        /// Gets or sets the type of the metric.
        /// </summary>
        /// <value>
        /// The type of the metric.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
#if REVIEW_WEBFORMS
        [Index( IndexName, Order = 2 )]
#endif
        public TypeOfMetric MetricType { get; set; }

        /// <summary>
        /// Gets or sets the metric value.
        /// </summary>
        /// <value>
        /// The metric value.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
        public decimal MetricValue { get; set; }

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
        [DataMember]
#if REVIEW_WEBFORMS
        [Index( IndexName, Order = 3 )]
#endif
        public DateTime MetricValueDateTime { get; set; } = RockDateTime.Now;

        /// <summary>
        /// Gets the metric value date key.
        /// </summary>
        /// <value>
        /// The metric value date key.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.DATE )]
        public int MetricValueDateKey
        {
            get => MetricValueDateTime.ToString( "yyyyMMdd" ).AsInteger();
            private set { }
        }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WebFarmNode"/>.
        /// </summary>
        /// <value>
        /// The web farm node.
        /// </value>
        [DataMember]
        public virtual WebFarmNode WebFarmNode { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// WebFarmNodeMetric Configuration class.
    /// </summary>
    public partial class WebFarmNodeMetricConfiguration : EntityTypeConfiguration<WebFarmNodeMetric>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebFarmNodeMetricConfiguration"/> class.
        /// </summary>
        public WebFarmNodeMetricConfiguration()
        {
            HasRequired( wfnm => wfnm.WebFarmNode ).WithMany( wfn => wfn.WebFarmNodeMetrics ).HasForeignKey( wfnm => wfnm.WebFarmNodeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion Entity Configuration
}
