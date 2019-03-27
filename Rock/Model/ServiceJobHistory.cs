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
    /// Represents a scheduled job/routine history in Rock.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "ServiceJobHistory" )]
    [DataContract]
    public partial class ServiceJobHistory : Model<ServiceJobHistory>
    {
        #region Entity Properties

        /// <summary>
        /// The Id of the ServiceJob
        /// </summary>
        /// <value>
        /// The ServiceJob identifier.
        /// </value>
        [DataMember( IsRequired = true )]
        public int ServiceJobId { get; set; }

        /// <summary>
        /// Gets or sets the name of the service worker.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the service worker.
        /// </value>
        [MaxLength( 45 )]
        [DataMember]
        public string ServiceWorker { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the Job started.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that the Job started
        /// </value>
        [DataMember]
        public DateTime? StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the job finished.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> that the job finished.
        /// </value>
        [DataMember]
        public DateTime? StopDateTime { get; set; }

        /// <summary>
        /// Gets or sets the completion status that was returned by the Job.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> containing the status that was returned by the Job.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the status message that was returned by the job. In most cases this will be used
        /// in the event of an exception to return the exception message.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Status Message that returned by the job.
        /// </value>
        [DataMember]
        public string StatusMessage { get; set; }
        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the ServiceJob <see cref="Rock.Model.ServiceJob" /> that this ServiceJobHistory provides a History value for.
        /// </summary>
        /// <value>
        /// The service job.
        /// </value>
        [DataMember]
        public virtual ServiceJob ServiceJob { get; set; }

        /// <summary>
        /// Gets the status message as HTML.
        /// </summary>
        /// <value>
        /// The status message as HTML.
        /// </value>
        [LavaInclude]
        public string StatusMessageAsHtml
        {
            get
            {
                return StatusMessage.ConvertCrLfToHtmlBr();
            }
        }

        /// <summary>
        /// Gets the job duration in seconds.
        /// </summary>
        /// <value>
        /// The job duration in seconds.
        /// </value>
        [LavaInclude]
        public int? DurationSeconds
        {
            get
            {
                if ( StartDateTime == null || StopDateTime == null )
                {
                    return null;
                }

                return (int)( (TimeSpan)( StopDateTime - StartDateTime ) ).TotalSeconds;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this Job.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this Job.
        /// </returns>
        public override string ToString()
        {
            return this.StatusMessage;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class ServiceJobHistoryConfiguration : EntityTypeConfiguration<ServiceJobHistory>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceJobHistoryConfiguration"/> class.
        /// </summary>
        public ServiceJobHistoryConfiguration()
        {
            this.HasRequired( t => t.ServiceJob ).WithMany( t => t.ServiceJobHistory ).HasForeignKey( t => t.ServiceJobId );
        }
    }

    #endregion
}
