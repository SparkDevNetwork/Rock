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
    /// Model that represents the scopes that can be used by OIDC clients.
    /// </summary>
    /// <seealso cref="Model{AuthScope}" />
    /// <seealso cref="Rock.Data.IHasActiveFlag" />
    [RockDomain( "Core" )]
    [Table( "AuthScope" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "8926FE3B-5AB6-4E48-8191-09EB1682F743")]
    public class AuthScope : Model<AuthScope>, IHasActiveFlag
    {
        /// <summary>
        /// Gets or sets a flag indicating if this item is active or not.
        /// </summary>
        /// <value>
        /// Active.
        /// </value>
        [Required]
        [DataMember]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [Required]
        [DataMember]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [DataMember]
        [MaxLength( 50 )]
        [Index( IsUnique = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the public.
        /// </summary>
        /// <value>
        /// The name of the public.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string PublicName { get; set; }
    }

    #region Entity Configuration

    /// <summary>
    /// AuthScope Configuration Class
    /// </summary>
    public partial class AuthScopeConfiguration : EntityTypeConfiguration<AuthScope>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthScopeConfiguration"/> class.
        /// </summary>
        public AuthScopeConfiguration()
        {
            // Empty constructor. This is required to tell EF that this model exists.
        }
    }

    #endregion Entity Configuration
}
