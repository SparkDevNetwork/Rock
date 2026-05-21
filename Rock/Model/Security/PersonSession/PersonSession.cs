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
using Rock.Enums;
using Rock.Enums.Security;
using Rock.Security;
using Rock.Utility;

namespace Rock.Model;

/// <summary>
/// Authoritative record of an authenticated session. The <c>.ROCK</c>
/// authentication cookie is reduced to a reference (a row pointer plus
/// integrity bits); this row carries the lifecycle, recency, and policy
/// data the platform uses to make authorization decisions.
/// </summary>
/// <remarks>
/// Industry-standard literature calls this concept "UserSession"; the
/// entity is named <c>PersonSession</c> because Rock names tables after the
/// non-null parent relationship and <c>PersonAliasId</c> is required while
/// <c>UserLoginId</c> is nullable. Readers searching for "user session"
/// should still find this entity via the design spec and this comment.
/// </remarks>
[RockDomain( "Security" )]
[Table( "PersonSession" )]
[DataContract]
[CodeGenExclude( CodeGenFeature.DefaultRestController )]
[CodeGenerateRest( CodeGenerateRestEndpoint.ReadOnly, DisableEntitySecurity = true )]
[Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.PERSON_SESSION )]
public partial class PersonSession : Model<PersonSession>, IHasAdditionalSettings
{
    #region Entity Properties

    /// <summary>
    /// The Id of the <see cref="Rock.Model.PersonAlias"/> that owns this session.
    /// </summary>
    [Required]
    [DataMember( IsRequired = true )]
    public int PersonAliasId { get; set; }

    /// <summary>
    /// The Id of the <see cref="Rock.Model.UserLogin"/> that the session is
    /// associated with, when one exists.
    /// </summary>
    /// <remarks>
    /// Null for impersonation tokens, passwordless flows, and other cases
    /// where there is no concrete <c>UserLogin</c>.
    /// </remarks>
    [DataMember]
    public int? UserLoginId { get; set; }

    /// <summary>
    /// A flag indicating whether the session is currently active. This is not
    /// the only determinant of session validity but is a quick check to avoid
    /// the more expensive checks, such as <see cref="ExpiresDateTime"/>.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>. When this flag flips to <c>false</c>, the
    /// <see cref="InactiveDateTime"/> column is stamped automatically.
    /// </remarks>
    [Required]
    [DataMember( IsRequired = true )]
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// The date and time the session was created.
    /// </summary>
    [Required]
    [DataMember( IsRequired = true )]
    public DateTime IssuedDateTime { get; set; }

    /// <summary>
    /// The date and time the session was deactivated. This is set automatically
    /// when the <see cref="IsActive"/> flag is flipped to false.
    /// </summary>
    [DataMember]
    public DateTime? InactiveDateTime { get; private set; }

    /// <summary>
    /// The date and time that the session will no longer be valid. This allows
    /// a session to be created with an explicit lifetime, after which it will
    /// no longer work.
    /// </summary>
    [DataMember]
    public DateTime? ExpiresDateTime { get; set; }

    /// <summary>
    /// The most recent activity date and time for the session.
    /// </summary>
    [Required]
    [DataMember( IsRequired = true )]
    public DateTime LastActivityDateTime { get; set; }

    /// <summary>
    /// The most recent date and time the person successfully provided any
    /// credential (password, SMS, TOTP) during this session.
    /// </summary>
    [DataMember]
    public DateTime? LastStepUpAuthenticationDateTime { get; set; }

    /// <summary>
    /// The most recent date and time MFA happened on this session.
    /// </summary>
    /// <remarks>
    /// Only updated when MFA is used <em>concurrently</em>: password and
    /// TOTP entered together qualifies; password followed by a TOTP-only
    /// prompt later does not. (Industry-standard semantics.)
    /// </remarks>
    [DataMember]
    public DateTime? LastMultiFactorAuthenticationDateTime { get; set; }

    /// <summary>
    /// A flag indicating whether the session was created from a "remember me"
    /// login.
    /// </summary>
    [Required]
    [DataMember( IsRequired = true )]
    public bool IsPersistent { get; set; }

    /// <summary>
    /// The Id of the <see cref="Rock.Model.InteractionDeviceType"/> that
    /// represents the UserAgent information that was captured when the session
    /// was first created.
    /// </summary>
    [DataMember]
    public int? InteractionDeviceTypeId { get; set; }

    /// <summary>
    /// The Id of the <see cref="Rock.Model.EntityType"/> representing the
    /// <c>AuthenticationComponent</c> used for initial authentication.
    /// </summary>
    [DataMember]
    public int? AuthenticationComponentId { get; set; }

    /// <summary>
    /// The source of the session - how it was created.
    /// </summary>
    [Required]
    [DataMember( IsRequired = true )]
    public PersonSessionCreationSource CreationSource { get; set; }

    /// <inheritdoc/>
    [DataMember]
    [StringValidation( StringValidationProfile.Unrestricted )]
    public string AdditionalSettingsJson { get; set; }

    #endregion Entity Properties

    #region Navigation Properties

    /// <summary>
    /// The <see cref="Rock.Model.PersonAlias"/> that owns this session.
    /// </summary>
    public virtual PersonAlias PersonAlias { get; set; }

    /// <summary>
    /// The <see cref="Rock.Model.UserLogin"/> that the was used to perform the
    /// login, when one exists.
    /// </summary>
    public virtual UserLogin UserLogin { get; set; }

    /// <summary>
    /// The <see cref="Rock.Model.InteractionDeviceType"/> representing the
    /// UserAgent information that was captured when the session was first
    /// created.
    /// </summary>
    public virtual InteractionDeviceType InteractionDeviceType { get; set; }

    /// <summary>
    /// The <see cref="Rock.Model.EntityType"/> representing the
    /// <c>AuthenticationComponent</c> used for initial authentication.
    /// </summary>
    public virtual EntityType AuthenticationComponentEntityType { get; set; }

    #endregion Navigation Properties
}

#region Entity Configuration

/// <summary>
/// PersonSession Configuration class.
/// </summary>
public partial class PersonSessionConfiguration : EntityTypeConfiguration<PersonSession>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PersonSessionConfiguration"/> class.
    /// </summary>
    public PersonSessionConfiguration()
    {
        this.HasRequired( s => s.PersonAlias ).WithMany().HasForeignKey( s => s.PersonAliasId ).WillCascadeOnDelete( false );

        // The migration manually adds ON DELETE SET NULL for UserLoginId so deleting a
        // UserLogin (which is also how an API key is revoked) leaves the historical
        // PersonSession row in place rather than cascading the deletion through all of
        // the user's sessions.
        this.HasOptional( s => s.UserLogin ).WithMany().HasForeignKey( s => s.UserLoginId ).WillCascadeOnDelete( false );

        this.HasOptional( s => s.InteractionDeviceType ).WithMany().HasForeignKey( s => s.InteractionDeviceTypeId ).WillCascadeOnDelete( false );
        this.HasOptional( s => s.AuthenticationComponentEntityType ).WithMany().HasForeignKey( s => s.AuthenticationComponentId ).WillCascadeOnDelete( false );
    }
}

#endregion Entity Configuration
