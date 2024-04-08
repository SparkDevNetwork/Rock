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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents an Authentication Client
    /// </summary>

    [RockDomain( "Core" )]
    [Table( "AuthClient" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "CBD66C3A-959A-4A0B-926C-C3ADE43066B1")]
    public class AuthClient : Model<AuthClient>, IHasActiveFlag
    {

        /// <summary>
        /// Gets or sets a value indicating whether [allow user API access].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow user API access]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowUserApiAccess { get; set; }

        /// <summary>
        /// Gets or sets the allowed claims.
        /// </summary>
        /// <value>
        /// The allowed claims.
        /// </value>
        [DataMember]
        public string AllowedClaims { get; set; }

        /// <summary>
        /// Gets or sets the allowed scopes.
        /// </summary>
        /// <value>
        /// The allowed scopes.
        /// </value>
        [DataMember]
        public string AllowedScopes { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this item is active or not.
        /// </summary>
        /// <value>
        /// Active.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        /// <value>
        /// The client identifier.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
        [Index( IsUnique = true )]
        [MaxLength(50)]
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret hash.
        /// </summary>
        /// <value>
        /// The client secret hash.
        /// </value>
        [HideFromReporting]
        public string ClientSecretHash { get; set; }

        /// <summary>
        /// Gets or sets the redirect URL.
        /// </summary>
        /// <value>
        /// The redirect URL.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
        public string RedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the post logout redirect URI.
        /// </summary>
        /// <value>
        /// The post logout redirect URI.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
        public string PostLogoutRedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the scope approval expiration in days.
        /// </summary>
        /// <value>
        /// The scope approval expiration in days.
        /// </value>
        [DataMember( IsRequired = true )]
        [Required]
        [Range( 0, int.MaxValue )]
        public int ScopeApprovalExpiration { get; set; } = 365;
    }

    #region Entity Configuration

    /// <summary>
    /// AuthClient Configuration Class
    /// </summary>
    public partial class AuthClientConfiguration : EntityTypeConfiguration<AuthClient>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthClientConfiguration"/> class.
        /// </summary>
        public AuthClientConfiguration()
        {
            // Empty constructor. This is required to tell EF that this model exists.
        }
    }

    #endregion Entity Configuration
}
