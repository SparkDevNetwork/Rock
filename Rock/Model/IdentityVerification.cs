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

using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Used to verify individuals by phone number.
    /// </summary>
    public partial class IdentityVerification : Model<IdentityVerification>
    {
        /// <summary>
        /// Gets or sets the reference number.
        /// </summary>
        /// <value>
        /// The reference number.
        /// </value>
        [DataMember()]
        [MaxLength( 150 )]
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the issue date time.
        /// </summary>
        /// <value>
        /// The issue date time.
        /// </value>
        [DataMember]
        public DateTime IssueDateTime { get; set; }

        /// <summary>
        /// Gets or sets the request ip address.
        /// </summary>
        /// <value>
        /// The request ip address.
        /// </value>
        [DataMember()]
        [MaxLength( 45 )]
        public string RequestIpAddress { get; set; }

        /// <summary>
        /// Gets or sets the identity verification code identifier.
        /// </summary>
        /// <value>
        /// The identity verification code identifier.
        /// </value>
        [DataMember()]
        public int IdentityVerificationCodeId { get; set; }

        /// <summary>
        /// Gets or sets the identity verification code.
        /// </summary>
        /// <value>
        /// The identity verification code.
        /// </value>
        public virtual IdentityVerificationCode IdentityVerificationCode { get; set; }
    }

    #region Entity Configuration

    /// <summary>
    /// Identity Verification Configuration class.
    /// </summary>
    public partial class IdentityVerificationConfiguration : EntityTypeConfiguration<IdentityVerification>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityVerificationConfiguration"/> class.
        /// </summary>
        public IdentityVerificationConfiguration()
        {
            this.HasRequired( a => a.IdentityVerificationCode ).WithMany().HasForeignKey( a => a.IdentityVerificationCodeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
