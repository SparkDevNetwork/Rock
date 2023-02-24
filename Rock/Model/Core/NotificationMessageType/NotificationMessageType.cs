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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a type that defines various aspects about a
    /// <see cref="NotificationMessage"/> in the system.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "NotificationMessageType" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "36FB1038-8836-429F-BAD4-04D32892D6D0" )]
    public partial class NotificationMessageType : Model<NotificationMessageType>, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.EntityType"/> component
        /// that handles logic for this instance.
        /// </summary>
        /// <value>
        /// A <see cref="int"/> representing the Id of the <see cref="Rock.Model.EntityType"/>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the key that identifies this instance to the component.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> that identifies this instance.
        /// </value>
        [Required]
        [MaxLength( 200 )]
        [Index]
        [DataMember( IsRequired = true )]
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the component data json. This data is only understood
        /// by the component itself and should not be modified elsewhere.
        /// </summary>
        /// <value>The component data json.</value>
        [DataMember]
        public string ComponentDataJson { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether messages are deleted instead
        /// of being marked as read.
        /// </summary>
        /// <value><c>true</c> if this messages are deleted instead of being marked as read; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsDeletedOnRead { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether messages are supported
        /// on web sites.
        /// </summary>
        /// <value><c>true</c> if this messages are supported on web; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsWebSupported { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether messages are supported
        /// on mobile applications.
        /// </summary>
        /// <value><c>true</c> if messages are supported on mobile; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsMobileApplicationSupported { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether messages are supported
        /// on TV applications.
        /// </summary>
        /// <value><c>true</c> if messages are supported on TV; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsTvApplicationSupported { get; set; }

        /// <summary>
        /// Gets or sets the related web site identifier. If specified then
        /// messages will only show up on this website. Otherwise messages
        /// will show up on all websites. This does not affect other site types.
        /// </summary>
        /// <value>The related web site identifier.</value>
        [DataMember]
        public int? RelatedWebSiteId { get; set; }

        /// <summary>
        /// Gets or sets the related mobile site identifier. If specified then
        /// messages will only show up on this mobile application. Otherwise
        /// messages will show up on all mobile applications. This does not
        /// affect other site types.
        /// </summary>
        /// <value>The related mobile site identifier.</value>
        [DataMember]
        public int? RelatedMobileApplicationSiteId { get; set; }

        /// <summary>
        /// Gets or sets the related TV site identifier. If specified then
        /// messages will only show up on this TV application. Otherwise
        /// messages will show up on all TV applications. This does not
        /// affect other site types.
        /// </summary>
        /// <value>The related TV site identifier.</value>
        [DataMember]
        public int? RelatedTvApplicationSiteId { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> of the component
        /// that handles logic for this instance.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> of the component.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the related web site. If specified then messages
        /// will only show up on this website. Otherwise messages will show
        /// up on all websites. This does not affect other site types.
        /// </summary>
        /// <value>The related web site identifier.</value>
        [DataMember]
        public virtual Site RelatedWebSite { get; set; }

        /// <summary>
        /// Gets or sets the related mobile site. If specified then messages
        /// will only show up on this mobile application. Otherwise messages
        /// will show up on all mobile applications. This does not affect
        /// other site types.
        /// </summary>
        /// <value>The related mobile site identifier.</value>
        [DataMember]
        public virtual Site RelatedMobileApplicationSite { get; set; }

        /// <summary>
        /// Gets or sets the related TV site. If specified then messages
        /// will only show up on this TV application. Otherwise messages
        /// will show up on all TV applications. This does not affect other
        /// site types.
        /// </summary>
        /// <value>The related TV site identifier.</value>
        [DataMember]
        public virtual Site RelatedTvApplicationSite { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="NotificationMessage"/>
        /// objects that belong to this <see cref="NotificationMessageType"/>.
        /// </summary>
        /// <remarks>
        /// This should only be used for LINQ queries in Where clauses.
        /// </remarks>
        /// <value>
        /// A collection of <see cref="NotificationMessage"/> objects.
        /// </value>
        [DataMember]
        public virtual ICollection<NotificationMessage> NotificationMessages { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var entityTypeCache = EntityTypeCache.Get( EntityTypeId );

            return entityTypeCache != null
                ? $"{entityTypeCache.Name}-{Key}"
                : Key;
        }

        #endregion
    }

    #region Entity Configuration    

    /// <summary>
    /// Notification Message Type Configuration class.
    /// </summary>
    public partial class NotificationMessageTypeConfiguration : EntityTypeConfiguration<NotificationMessageType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationMessageTypeConfiguration"/> class.
        /// </summary>
        public NotificationMessageTypeConfiguration()
        {
            this.HasRequired( nmt => nmt.EntityType ).WithMany().HasForeignKey( nmt => nmt.EntityTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( nmt => nmt.RelatedWebSite ).WithMany().HasForeignKey( nmt => nmt.RelatedWebSiteId ).WillCascadeOnDelete( false );
            this.HasOptional( nmt => nmt.RelatedMobileApplicationSite ).WithMany().HasForeignKey( nmt => nmt.RelatedMobileApplicationSiteId ).WillCascadeOnDelete( false );
            this.HasOptional( nmt => nmt.RelatedTvApplicationSite ).WithMany().HasForeignKey( nmt => nmt.RelatedTvApplicationSiteId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
