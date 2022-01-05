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
using Newtonsoft.Json;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents WebFarmNode in Rock.
    /// </summary>
    [RockDomain( "WebFarm" )]
    [Table( "WebFarmNode" )]
    [DataContract]
    public partial class WebFarmNode : Model<WebFarmNode>, IHasActiveFlag
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a Node Name.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [node name]; otherwise, <c>false</c>.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
        [MaxLength( 250 )]
        [Index( IsUnique = true )]
        public string NodeName { get; set; }

        /// <summary>
        /// Gets or sets the added date time.
        /// </summary>
        /// <value>
        /// The added date time.
        /// </value>
        [DataMember]
        public DateTime AddedDateTime { get; set; } = RockDateTime.Now;

        /// <summary>
        /// Gets or sets the last restart date time.
        /// </summary>
        /// <value>
        /// The last restart date time.
        /// </value>
        [DataMember]
        public DateTime LastRestartDateTime { get; set; } = RockDateTime.Now;

        /// <summary>
        /// Gets or sets the stopped date time.
        /// </summary>
        /// <value>
        /// The stopped date time.
        /// </value>
        [DataMember]
        public DateTime? StoppedDateTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [jobs allowed].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [jobs allowed]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool JobsAllowed { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is current job runner.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is current job runner; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsCurrentJobRunner { get; set; }

        /// <summary>
        /// Gets or sets the last seen date time.
        /// </summary>
        /// <value>
        /// The last seen date time.
        /// </value>
        [DataMember]
        public DateTime LastSeenDateTime { get; set; } = RockDateTime.Now;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is leader.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is leader; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsLeader { get; set; }

        /// <summary>
        /// Gets or sets the current leadership polling interval seconds.
        /// </summary>
        /// <value>
        /// The current leadership polling interval seconds.
        /// </value>
        [DataMember]
        [Index( IsUnique = true )]
        public decimal CurrentLeadershipPollingIntervalSeconds { get; set; }

        /// <summary>
        /// Gets or sets the configured leadership polling interval seconds.
        /// </summary>
        /// <value>
        /// The configured leadership polling interval seconds.
        /// </value>
        [DataMember]
        public int? ConfiguredLeadershipPollingIntervalSeconds { get; set; }

        #endregion Entity Properties

        #region IHasActiveFlag

        /// <summary>
        /// Gets or sets a flag indicating if this item is active or not.
        /// </summary>
        /// <value>
        /// Active.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; } = true;

        #endregion IHasActiveFlag

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the web farm node logs.
        /// </summary>
        /// <value>
        /// The web farm node logs.
        /// </value>
        [DataMember]
        public virtual ICollection<WebFarmNodeLog> WebFarmNodeLogs
        {
            get => _webFarmNodeLogs ?? ( _webFarmNodeLogs = new Collection<WebFarmNodeLog>() );
            set => _webFarmNodeLogs = value;
        }
        private ICollection<WebFarmNodeLog> _webFarmNodeLogs;

        /// <summary>
        /// Gets or sets the web farm node metrics.
        /// </summary>
        /// <value>
        /// The web farm node metrics.
        /// </value>
        [DataMember]
        public virtual ICollection<WebFarmNodeMetric> WebFarmNodeMetrics
        {
            get => _webFarmNodeMetrics ?? ( _webFarmNodeMetrics = new Collection<WebFarmNodeMetric>() );
            set => _webFarmNodeMetrics = value;
        }
        private ICollection<WebFarmNodeMetric> _webFarmNodeMetrics;

        /// <summary>
        /// Gets or sets the written web farm node logs.
        /// </summary>
        /// <value>
        /// The written web farm node logs.
        /// </value>
        [DataMember]
        public virtual ICollection<WebFarmNodeLog> WrittenWebFarmNodeLogs
        {
            get => _writtenWebFarmNodeLogs ?? ( _writtenWebFarmNodeLogs = new Collection<WebFarmNodeLog>() );
            set => _writtenWebFarmNodeLogs = value;
        }
        private ICollection<WebFarmNodeLog> _writtenWebFarmNodeLogs;

        #endregion Virtual Properties

        #region Methods

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return NodeName.IsNullOrWhiteSpace() ? base.ToString() : NodeName;
        }

        #endregion Methods

        #region Entity Configuration

        /// <summary>
        /// WebFarmNode Configuration class.
        /// </summary>
        public partial class WebFarmNodeConfiguration : EntityTypeConfiguration<WebFarmNode>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WebFarmNodeConfiguration"/> class.
            /// </summary>
            public WebFarmNodeConfiguration()
            {
            }
        }

        #endregion Entity Configuration
    }
}
