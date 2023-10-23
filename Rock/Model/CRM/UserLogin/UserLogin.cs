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
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Represents a user's Rock login and authentication credentials.
    /// </summary>
    [RockDomain( "CRM" )]
    [Table( "UserLogin" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "0FA592F1-728C-4885-BE38-60ED6C0D834F")]
    public partial class UserLogin : Model<UserLogin>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> for the authentication service that this UserLogin user will use.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> that this DataView reports on.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int? EntityTypeId { get; set; }
        
        /// <summary>
        /// Gets or sets the UserName that is associated with this UserLogin. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the UserName that is associated with this UserLogin.
        /// </value>
        [Required]
        [MaxLength( 255 )]
        [DataMember( IsRequired = true )]
        public string UserName { get; set; }
        
        /// <summary>
        /// Gets or sets the Password.  Stored as a BCrypt hash for Rock Database Auth, but possibly a different hashtype for other ServiceTypes
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the password. 
        /// </value>
        [MaxLength( 128 )]
        [HideFromReporting]
        public string Password { get; set; }
        
        /// <summary>
        /// Gets or sets a flag indicating if the UserLogin has been confirmed.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the UserLogin has been confirmed; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool? IsConfirmed { get; set; }

        /// <summary>
        /// Gets or sets the date and time of the last activity (login, password change, etc.) performed with this UserLogin.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time of the last activity associated with this UserLogin.
        /// </value>
        [NotAudited]
        [DataMember]
        public DateTime? LastActivityDateTime { get; set; }

        /// <summary>
        /// Gets or sets the most recent date and time that a user successfully logged in using this UserLogin.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the most recent date and time that a user successfully logged in with this UserLogin.
        /// </value>
        [NotAudited]
        [DataMember]
        public DateTime? LastLoginDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time that the password was successfully changed.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing when the password was last changed.
        /// </value>
        [NotAudited]
        [DataMember]
        public DateTime? LastPasswordChangedDateTime { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the user is currently online and logged in to the system.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the user is currently online and logged in with this UserLogin; otherwise <c>false</c>.
        /// </value>
        [NotAudited]
        [DataMember]
        public bool? IsOnLine { get; set; }
        
        /// <summary>
        /// Gets or sets a flag indicating if the UserLogin is currently locked out.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if the UserLogin is currently locked out; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool? IsLockedOut { get; set; }

        /// <summary>
        /// Gets or sets the is password change required.
        /// </summary>
        /// <value>
        /// The is password change required.
        /// </value>
        [DataMember]
        public bool? IsPasswordChangeRequired { get; set; }

        /// <summary>
        /// Gets or sets date and time that the UserLogin was last locked out.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that the user login was last locked out.
        /// </value>
        [NotAudited]
        [DataMember]
        public DateTime? LastLockedOutDateTime { get; set; }
        
        /// <summary>
        /// Gets or sets the number of failed password attempts within the failed password attempt window.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the failed password attempts during the attempt window.
        /// </value>
        [DataMember]
        public int? FailedPasswordAttemptCount { get; set; }

        /// <summary>
        /// Gets or sets the failed password attempt window start date time.
        /// </summary>
        /// <value>
        /// The failed password attempt window start date time.
        /// </value>
        [DataMember]
        public DateTime? FailedPasswordAttemptWindowStartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the last time that user was notified about their password expiring.
        /// </summary>
        /// <value>
        /// The last password expiration warning date time.
        /// </value>
        [DataMember]
        public DateTime? LastPasswordExpirationWarningDateTime { get; set; }

        /// <summary>
        /// Gets or sets the API key associated with the UserLogin
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the API key that is associated with the UserLogin
        /// </value>
        [MaxLength( 50 )]
        [HideFromReporting]
        [DataMember]
        public string ApiKey { get; set; }
        
        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.Person"/> who this UserLogin belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.Person"/> that this UserLogin belongs to.
        /// </value>
        [DataMember]
        public int? PersonId { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Person"/> that this UserLogin is associated with.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Person"/> that this UserLogin is associated with.
        /// </value>
        [LavaVisible]
        public virtual Model.Person Person { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> for the authentication service that this UserLogin user.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> that this DataView reports on.
        /// </value>
        [DataMember]
        [LavaHidden]
        public virtual EntityType EntityType { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> containing the UserName that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> containing the UserName that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.UserName;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// User Configuration class.
    /// </summary>
    public partial class UserLoginConfiguration : EntityTypeConfiguration<UserLogin>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserLoginConfiguration"/> class.
        /// </summary>
        public UserLoginConfiguration()
        {
            this.HasOptional( p => p.Person ).WithMany( p => p.Users ).HasForeignKey( p => p.PersonId ).WillCascadeOnDelete(true);
            this.HasRequired( p => p.EntityType ).WithMany().HasForeignKey( p => p.EntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
