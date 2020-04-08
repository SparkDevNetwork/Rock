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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents an instance where a <see cref="Rock.Model.Person"/> followed an instance of an entity
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "FollowingSuggested" )]
    [DataContract]
    public partial class FollowingSuggested : Model<FollowingSuggested>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the entity type identifier.
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        [DataMember]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        [DataMember]
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the PersonAliasId of the person that is following the Entity
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the suggestion type identifier.
        /// </summary>
        /// <value>
        /// The suggestion type identifier.
        /// </value>
        [DataMember]
        public int SuggestionTypeId { get; set; }

        /// <summary>
        /// Gets or sets the last promoted date time.
        /// </summary>
        /// <value>
        /// The last promoted date time.
        /// </value>
        [DataMember]
        public DateTime? LastPromotedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the status changed date time.
        /// </summary>
        /// <value>
        /// The status changed date time.
        /// </value>
        [DataMember]
        public DateTime StatusChangedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [DataMember]
        public FollowingSuggestedStatus Status { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the type of the suggestion.
        /// </summary>
        /// <value>
        /// The type of the suggestion.
        /// </value>
        [DataMember]
        public virtual FollowingSuggestionType SuggestionType { get; set; }
        
        #endregion

        #region Public Methods

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// File Configuration class.
    /// </summary>
    public partial class FollowingSuggestedConfiguration : EntityTypeConfiguration<FollowingSuggested>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FollowingSuggestedConfiguration"/> class.
        /// </summary>
        public FollowingSuggestedConfiguration()
        {
            this.HasRequired( f => f.EntityType ).WithMany().HasForeignKey( f => f.EntityTypeId ).WillCascadeOnDelete( true );
            this.HasRequired( f => f.PersonAlias ).WithMany().HasForeignKey( f => f.PersonAliasId ).WillCascadeOnDelete( true );
            this.HasRequired( f => f.SuggestionType ).WithMany().HasForeignKey( f => f.SuggestionTypeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// The person's email preference
    /// </summary>
    public enum FollowingSuggestedStatus
    {
        /// <summary>
        /// The pending notification
        /// </summary>
        PendingNotification = 0,

        /// <summary>
        /// Emails can be sent to person
        /// </summary>
        Suggested = 1,

        /// <summary>
        /// No Mass emails should be sent to person
        /// </summary>
        Ignored = 2,
    }

    #endregion


}
