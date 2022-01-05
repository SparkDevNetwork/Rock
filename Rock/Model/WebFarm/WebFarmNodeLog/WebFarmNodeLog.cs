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
    /// Represents WebFarmNodeLog in Rock.
    /// </summary>
    [RockDomain( "WebFarm" )]
    [Table( "WebFarmNodeLog" )]
    [DataContract]
    public partial class WebFarmNodeLog : Model<WebFarmNodeLog>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the severity.
        /// </summary>
        /// <value>
        /// The severity.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
        public SeverityLevel Severity { get; set; }

        /// <summary>
        /// Gets or sets the web farm node identifier.
        /// </summary>
        /// <value>
        /// The web farm node identifier.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
        public int WebFarmNodeId { get; set; }

        /// <summary>
        /// Gets or sets the writer web farm node identifier.
        /// </summary>
        /// <value>
        /// The web farm node identifier.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
        public int WriterWebFarmNodeId { get; set; }

        /// <summary>
        /// Gets or sets the type of the event.
        /// </summary>
        /// <value>
        /// The type of the event.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
        [MaxLength( 50 )]
        public string EventType { get; set; }

        /// <summary>
        /// Gets or sets the event date time.
        /// </summary>
        /// <value>
        /// The event date time.
        /// </value>
        [DataMember]
        public DateTime EventDateTime { get; set; } = RockDateTime.Now;

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [DataMember]
        public string Message { get; set; }

        #endregion Entity Properties

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.WebFarmNode"/>.
        /// </summary>
        /// <value>
        /// The web farm node.
        /// </value>
        [DataMember]
        public virtual WebFarmNode WebFarmNode { get; set; }

        /// <summary>
        /// Gets or sets the writer <see cref="Rock.Model.WebFarmNode"/>.
        /// </summary>
        /// <value>
        /// The writer web farm node.
        /// </value>
        [DataMember]
        public virtual WebFarmNode WriterWebFarmNode { get; set; }

        #endregion Virtual Properties

        #region Entity Configuration

        /// <summary>
        /// WebFarmNodeLog Configuration class.
        /// </summary>
        public partial class WebFarmNodeLogConfiguration : EntityTypeConfiguration<WebFarmNodeLog>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WebFarmNodeLogConfiguration"/> class.
            /// </summary>
            public WebFarmNodeLogConfiguration()
            {
                HasRequired( wfnl => wfnl.WebFarmNode ).WithMany( wfn => wfn.WebFarmNodeLogs ).HasForeignKey( wfnl => wfnl.WebFarmNodeId ).WillCascadeOnDelete( true );
                HasRequired( wfnl => wfnl.WriterWebFarmNode ).WithMany( wfn => wfn.WrittenWebFarmNodeLogs ).HasForeignKey( wfnl => wfnl.WriterWebFarmNodeId ).WillCascadeOnDelete( false );
            }
        }

        #endregion Entity Configuration

        #region Enumerations

        /// <summary>
        /// Represents the severity of the log entry.
        /// </summary>
        public enum SeverityLevel
        {
            /// <summary>
            /// An informative log entry that may be useful for debugging
            /// </summary>
            Info = 0,

            /// <summary>
            /// A warning log entry that may require DevOps attention
            /// </summary>
            Warning = 1,

            /// <summary>
            /// A critical log entry that requires DevOps attention
            /// </summary>
            Critical = 2
        }

        #endregion Enumerations
    }
}
