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
    /// Represents a Auth Audit Log
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "AuthAuditLog" )]
    [DataContract]
    public class AuthAuditLog : Entity<AuthAuditLog>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the EntityTypeId of the <see cref="Rock.Model.EntityType"/> for the entity that the Auth object applies to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> of the entity that the Auth object applies to.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the EntityId of the entity that this Auth entity applies to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityId of the entity that this Auth entity applies to.
        /// </value>
        [DataMember]
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the name of action that this Auth entity covers (i.e. view, edit, administrate, etc.).
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the name of the action that is covered by this Auth entity.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the change type.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public ChangeType ChangeType { get; set; }

        /// <summary>
        /// Gets or sets the change datetime.
        /// </summary>
        /// <value>
        /// The change datetime.
        /// </value>
        [DataMember]
        public DateTime ChangeDateTime { get; set; }

        /// <summary>
        /// Gets or sets the PersonAliasId that changed the auth.
        /// </summary>
        /// <value>
        /// The changed by person alias identifier.
        /// </value>
        [DataMember]
        public int? ChangeByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the Auth entity was pre allowed or denied this action for the role.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> value that equals A for allow and D for deny.
        /// </value>
        [MaxLength( 1 )]
        [DataMember]
        public string PreAllowOrDeny { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the Auth entity was post allowed or denied this action for the role.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> value that equals A for allow and D for deny.
        /// </value>
        [MaxLength( 1 )]
        [DataMember]
        public string PostAllowOrDeny { get; set; }

        /// <summary>
        /// Gets or sets the pre order or priority of the Auth entity. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the pre order of the Auth entity.
        /// </value>
        [DataMember]
        public int? PreOrder { get; set; }

        /// <summary>
        /// Gets or sets the post order or priority of the Auth entity. 
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the post order of the Auth entity.
        /// </value>
        [DataMember]
        public int? PostOrder { get; set; }

        /// <summary>
        /// Gets or sets the GroupId of the Security Role <see cref="Rock.Model.Group"/> that the Auth entity allowed or denied access to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the GroupId of the <see cref="Rock.Model.Person" /> that the Auth entity allowed or denied access to.
        /// If group based Authorization is not used this value will be null.
        /// </value>
        [DataMember]
        public int? GroupId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.PersonAlias"/> that this Auth entity allows or denies access to. This is used for user based authorization
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.PersonAlias"/> that this Auth entity allows or denies access to. This applies to user/person based authorization.
        /// If user/person based Authorization is not used this value will be null.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the special role that the Auth entity applies to.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.SpecialRole"/> enum indicating the special role that this Auth entity applies to.
        /// If the Auth entity does not apply to any special role then the value will be <c>SpecialRole.None</c> or (0); 
        /// If the Auth entity applies to All Users (authenticated and unauthenticated) then the value will be <c>SpecialRole.AllUsers</c> or 1;
        /// If the Auth entity applies to All Authenticated Users then the value will be <c>SpecialRole.AllAuthenticatedUsers</c> or 2;
        /// If the Auth entity applies to All Un-authenticated Users then this value will be <c>SpecialRole.AllUnAuthenticatedUsers</c> or 3.
        /// </value>
        [DataMember]
        public SpecialRole? SpecialRole { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the the <see cref="Rock.Model.EntityType"/> of the entity that is being secured.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> of of the entity that is being secured.
        /// </value>
        [LavaVisible]
        public virtual Model.EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the Security Role <see cref="Rock.Model.Group"/> that the Auth entity allowed or denied access to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Group"/> that the Auth entity allowed or denied access to. If group based authorization is not used, this value will be null.
        /// </value>
        [LavaVisible]
        public virtual Model.Group Group { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> that this Auth entity allows or denies access to. This is used for Person based authorization.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.PersonAlias"/> that this Auth entity allows or denies access to. If person based authorization is not used, this value will be null.
        /// </value>
        [LavaVisible]
        public virtual Model.PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> that changed the auth.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.PersonAlias"/> the changed the auth.
        /// </value>
        [LavaVisible]
        public virtual Model.PersonAlias ChangeByPersonAlias { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Auth Audit Log Configuration class.
    /// </summary>
    public partial class AuthAuditLogConfiguration : EntityTypeConfiguration<AuthAuditLog>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthAuditLogConfiguration"/> class.
        /// </summary>
        public AuthAuditLogConfiguration()
        {
            this.HasRequired( p => p.EntityType ).WithMany().HasForeignKey( p => p.EntityTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Group ).WithMany().HasForeignKey( p => p.GroupId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.ChangeByPersonAlias ).WithMany().HasForeignKey( p => p.ChangeByPersonAliasId ).WillCascadeOnDelete( true );
            this.HasOptional( p => p.PersonAlias ).WithMany().HasForeignKey( p => p.PersonAliasId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
