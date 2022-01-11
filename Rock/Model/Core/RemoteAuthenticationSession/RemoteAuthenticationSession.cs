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
    /// 
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "RemoteAuthenticationSession" )]
    [DataContract]
    public partial class RemoteAuthenticationSession : Model<RemoteAuthenticationSession>, IRockEntity
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        /// <value>
        /// The code.
        /// </value>
        [Required]
        [MaxLength( 20 )]
        [DataMember( IsRequired = true )]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the authorized person alias identifier.
        /// </summary>
        /// <value>
        /// The authorized person alias identifier.
        /// </value>
        [DataMember]
        public int? AuthorizedPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the session start date time.
        /// </summary>
        /// <value>
        /// The session start date time.
        /// </value>
        [DataMember]
        public DateTime? SessionStartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the session authenticated date time.
        /// </summary>
        /// <value>
        /// The session authenticated date time.
        /// </value>
        [DataMember]
        public DateTime? SessionAuthenticatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the session end date time.
        /// </summary>
        /// <value>
        /// The session end date time.
        /// </value>
        [DataMember]
        public DateTime? SessionEndDateTime { get; set; }

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
        /// Gets or sets the authentication ip address.
        /// </summary>
        /// <value>
        /// The authentication ip address.
        /// </value>
        [DataMember]
        [MaxLength( 45 )]
        public string AuthenticationIpAddress { get; set; }

        /// <summary>
        /// Gets or sets the device unique identifier.
        /// </summary>
        /// <value>
        /// The device unique identifier.
        /// </value>
        [DataMember]
        [MaxLength( 45 )]
        public string DeviceUniqueIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the site identifier.
        /// </summary>
        /// <value>
        /// The site identifier.
        /// </value>
        [DataMember]
        public int? SiteId { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the site.
        /// </summary>
        /// <value>
        /// The site.
        /// </value>
        [DataMember]
        public virtual Site Site { get; set; }

        /// <summary>
        /// Gets or sets the authorized person alias.
        /// </summary>
        /// <value>
        /// The authorized person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias AuthorizedPersonAlias { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// 
    /// </summary>
    public partial class RemoteAuthenticationSessionConfiguration : EntityTypeConfiguration<RemoteAuthenticationSession>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteAuthenticationSessionConfiguration"/> class.
        /// </summary>
        public RemoteAuthenticationSessionConfiguration()
        {
            this.HasOptional( s => s.Site ).WithMany().HasForeignKey( s => s.SiteId ).WillCascadeOnDelete( false );
            this.HasOptional( s => s.AuthorizedPersonAlias ).WithMany().HasForeignKey( s => s.AuthorizedPersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
