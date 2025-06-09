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
using Rock.Enums.Security;

namespace Rock.Model
{
    /// <summary>
    /// Represents a history that is entered in Rock and associated with a user login, for both successful and failed login attempts.
    /// Can represent internal and external (OpenID Connect) logins.
    /// </summary>
    [RockDomain( "Security " )]
    [NotAudited]
    [Table( "HistoryLogin" )]
    [DataContract]
    [CodeGenerateRest( Enums.CodeGenerateRestEndpoint.ReadOnly )]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.HISTORY_LOGIN )]
    public partial class HistoryLogin : Model<HistoryLogin>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the UserName.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the UserName.
        /// </value>
        [MaxLength( 255 )]
        [DataMember]
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="UserLogin"/> that is associated with this login history.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="UserLogin"/> that is associated with this login history.
        /// </value>
        [DataMember]
        [IgnoreCanDelete]
        public int? UserLoginId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="PersonAlias"/> that is associated with this login history.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> representing the Id of the <see cref="PersonAlias"/> that is associated with this login history.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the login attempt date time.
        /// </summary>
        /// <value>
        /// A <see cref="DateTime"/> representing the login attempt date time.
        /// </value>S
        /// <remarks>
        /// If this is not specified, <see cref="RockDateTime.Now"/> will be used.
        /// </remarks>
        [DataMember]
        public DateTime LoginAttemptDateTime { get; set; } = RockDateTime.Now;

        /// <summary>
        /// Gets or sets the client IP address from which this login history originated.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the client IP address from which this login history originated.
        /// </value>
        [MaxLength( 45 )]
        [DataMember]
        public string ClientIpAddress { get; set; }

        /// <summary>
        /// Gets or sets the client identifier for the authorization client that is associated with this login history.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the client identifier for the authorization client that is associated with this login history.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string AuthClientClientId { get; set; }

        /// <summary>
        /// Gets or sets the name of the external source that is associated with this login history.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the name of the external source that is associated with this login history.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string ExternalSource { get; set; }

        /// <summary>
        /// Gets or sets the Id of the source <see cref="Site"/> that is associated with this login history.
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32"/> that represents the Id of the source <see cref="Site"/> that is associated with this login history.
        /// </value>
        [DataMember]
        [IgnoreCanDelete]
        public int? SourceSiteId { get; set; }

        /// <summary>
        /// Gets or sets any related data.
        /// <para>
        /// DO NOT read from or write to this property directly. Instead, use the <see cref="GetRelatedDataOrNull()"/>
        /// and <see cref="SetRelatedDataJson(Security.HistoryLoginRelatedData)"/> methods to ensure data is properly
        /// serialized and deserialized to and from this property.
        /// </para>
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents any related data.
        /// </value>
        [DataMember]
        public string RelatedDataJson { get; set; }

        /// <summary>
        /// Gets or sets the destination URL.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the destination URL.
        /// </value>
        [MaxLength( 2048 )]
        [DataMember]
        public string DestinationUrl { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the login was successful.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that represents a flag indicating if the login was successful.
        /// </value>
        [DataMember]
        public bool WasLoginSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the login failure reason, if login was unsuccessful.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Enums.Security.LoginFailureReason"/> that represents the login failure reason, if login was unsuccessful.
        /// </value>
        [DataMember]
        public LoginFailureReason? LoginFailureReason { get; set; }

        /// <summary>
        /// Gets or sets the login failure message, if login was unsuccessful.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the login failure message, if login was unsuccessful.
        /// </value>
        [DataMember]
        public string LoginFailureMessage { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.UserLogin"/> that is associated with this login history.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.UserLogin"/> that is associated with this login history.
        /// </value>
        public virtual UserLogin UserLogin { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> that is associated with this login history.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.PersonAlias"/> that is associated with this login history.
        /// </value>
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Site"/> that is associated with this login history.
        /// </summary>
        /// <value>
        /// The <see cref="Site"/> that is associated with this login history.
        /// </value>
        public virtual Site SourceSite { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// History Login Configuration class.
    /// </summary>
    public partial class HistoryLoginConfiguration : EntityTypeConfiguration<HistoryLogin>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HistoryLoginConfiguration"/> class;
        /// </summary>
        public HistoryLoginConfiguration()
        {
            this.HasOptional( a => a.PersonAlias ).WithMany().HasForeignKey( a => a.PersonAliasId ).WillCascadeOnDelete( false );

            // The migration will manually add ON DELETE SET NULL for UserLoginId and SourceSiteId.
            this.HasOptional( a => a.UserLogin ).WithMany().HasForeignKey( a => a.UserLoginId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.SourceSite ).WithMany().HasForeignKey( a => a.SourceSiteId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
