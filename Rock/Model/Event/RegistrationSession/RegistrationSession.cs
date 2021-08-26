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
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// 
    /// </summary>
    [RockDomain( "Event" )]
    [Table( "RegistrationSession" )]
    [DataContract]
    public partial class RegistrationSession : Model<RegistrationSession>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the registration instance identifier.
        /// </summary>
        /// <value>
        /// The registration instance identifier.
        /// </value>
        [DataMember]
        public int RegistrationInstanceId { get; set; }

        /// <summary>
        /// Gets or sets the registration count. These are registrants that are not slated to be on the waitlist.
        /// </summary>
        /// <value>
        /// The registration count.
        /// </value>
        [DataMember]
        public int RegistrationCount { get; set; }

        /// <summary>
        /// Gets or sets the session start date time.
        /// </summary>
        /// <value>
        /// The session start date time.
        /// </value>
        [DataMember]
        public DateTime SessionStartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the expiration date time.
        /// </summary>
        /// <value>
        /// The expiration date time.
        /// </value>
        [DataMember]
        public DateTime ExpirationDateTime { get; set; }

        /// <summary>
        /// Gets or sets the client ip address.
        /// </summary>
        /// <value>
        /// The client ip address.
        /// </value>
        [DataMember]
        [MaxLength( 45 )]
        public string ClientIpAddress { get; set; }

        /// <summary>
        /// Gets or sets the registration data.
        /// </summary>
        /// <value>
        /// The registration data.
        /// </value>
        [DataMember]
        public string RegistrationData { get; set; }

        /// <summary>
        /// Gets or sets the payment gateway reference.
        /// </summary>
        /// <value>
        /// The payment gateway reference.
        /// </value>
        [DataMember]
        [MaxLength( 36 )]
        public string PaymentGatewayReference { get; set; }

        /// <summary>
        /// Gets or sets the session status.
        /// </summary>
        /// <value>
        /// The session status.
        /// </value>
        [DataMember]
        public SessionStatus SessionStatus { get; set; }

        /// <summary>
        /// Gets or sets the registration identifier.
        /// </summary>
        /// <value>
        /// The registration identifier.
        /// </value>
        [DataMember]
        public int? RegistrationId { get; set; }


        #endregion Entity Properties

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the registration instance.
        /// </summary>
        /// <value>
        /// The registration instance.
        /// </value>
        [DataMember]
        public virtual RegistrationInstance RegistrationInstance { get; set; }

        /// <summary>
        /// Gets or sets the registration.
        /// </summary>
        /// <value>
        /// The registration.
        /// </value>
        [DataMember]
        public virtual Registration Registration { get; set; }

        #endregion Virtual Properties
    }

    #region Entity Configuration

    /// <summary>
    /// Initializes a new instance of the <see cref="RegistrationSessionConfiguration"/> class.
    /// </summary>
    public partial class RegistrationSessionConfiguration : EntityTypeConfiguration<RegistrationSession>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RegistrationSessionConfiguration"/> class.
        /// </summary>
        public RegistrationSessionConfiguration()
        {
             this.HasRequired( t => t.RegistrationInstance ).WithMany().HasForeignKey( t => t.RegistrationInstanceId ).WillCascadeOnDelete( false );
             this.HasOptional( t => t.Registration ).WithMany().HasForeignKey( t => t.RegistrationId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration

    #region Enums
    /// <summary>
    /// The status of the RegistrationSession
    /// </summary>
    public enum SessionStatus
    {
        /// <summary>
        /// Transient
        /// </summary>
        Transient = 0,

        /// <summary>
        /// Payment Pending
        /// </summary>
        PaymentPending = 1,

        /// <summary>
        /// Completed
        /// </summary>
        Completed = 2
    }

    #endregion Enums
}
