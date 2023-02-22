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
using System.Runtime.Serialization;

using Rock.Communication.SmsActions;
using Rock.Core;
using Rock.Data;
using Rock.Lava;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a NoteType that is cached by Rock. 
    /// </summary>
    [Serializable]
    [DataContract]
    public class NotificationMessageTypeCache : ModelCache<NotificationMessageTypeCache, NotificationMessageType>
    {
        #region Properties

        /// <summary>
        /// Gets the Id of the <see cref="Rock.Model.EntityType"/> that
        /// handles logic for this instance.
        /// </summary>
        /// <value>
        /// A <see cref="int"/> representing the Id of the <see cref="Rock.Model.EntityType"/>.
        /// </value>
        [DataMember]
        public int EntityTypeId { get; private set; }

        /// <summary>
        /// Gets the key that identifies this instance to the component.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> that identifies this instance.
        /// </value>
        [DataMember()]
        public string Key { get; private set; }

        /// <summary>
        /// Gets the component data json. This data is only understood
        /// by the component itself and should not be modified elsewhere.
        /// </summary>
        /// <value>The component data json.</value>
        [DataMember]
        public string ComponentDataJson { get; private set; }

        /// <summary>
        /// Gets a value indicating whether messages are deleted instead
        /// of being marked as read.
        /// </summary>
        /// <value><c>true</c> if this messages are deleted instead of being marked as read; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsDeletedOnRead { get; private set; }

        /// <summary>
        /// Gets a value indicating whether messages are supported
        /// on web sites.
        /// </summary>
        /// <value><c>true</c> if this messages are supported on web; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsWebSupported { get; private set; }

        /// <summary>
        /// Gets a value indicating whether messages are supported
        /// on mobile applications.
        /// </summary>
        /// <value><c>true</c> if messages are supported on mobile; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsMobileApplicationSupported { get; private set; }

        /// <summary>
        /// Gets a value indicating whether messages are supported
        /// on TV applications.
        /// </summary>
        /// <value><c>true</c> if messages are supported on TV; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsTvApplicationSupported { get; private set; }

        /// <summary>
        /// Gets the related web site identifier. If specified then
        /// messages will only show up on this website. Otherwise messages
        /// will show up on all websites. This does not affect other site types.
        /// </summary>
        /// <value>The related web site identifier.</value>
        [DataMember]
        public int? RelatedWebSiteId { get; private set; }

        /// <summary>
        /// Gets the related mobile site identifier. If specified then
        /// messages will only show up on this mobile application. Otherwise
        /// messages will show up on all mobile applications. This does not
        /// affect other site types.
        /// </summary>
        /// <value>The related mobile site identifier.</value>
        [DataMember]
        public int? RelatedMobileApplicationSiteId { get; private set; }

        /// <summary>
        /// Gets the related TV site identifier. If specified then
        /// messages will only show up on this TV application. Otherwise
        /// messages will show up on all TV applications. This does not
        /// affect other site types.
        /// </summary>
        /// <value>The related TV site identifier.</value>
        [DataMember]
        public int? RelatedTvApplicationSiteId { get; private set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets the <see cref="Rock.Model.EntityType"/> of the component
        /// that handles logic for this instance.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> of the component.
        /// </value>
        [DataMember]
        public virtual EntityTypeCache EntityType => EntityTypeCache.Get( EntityTypeId );

        /// <summary>
        /// Gets the related web site. If specified then messages
        /// will only show up on this website. Otherwise messages will show
        /// up on all websites. This does not affect other site types.
        /// </summary>
        /// <value>The related web site identifier.</value>
        [DataMember]
        public virtual SiteCache RelatedWebSite => RelatedWebSiteId.HasValue ? SiteCache.Get( RelatedWebSiteId.Value ) : null;

        /// <summary>
        /// Gets the related mobile site. If specified then messages
        /// will only show up on this mobile application. Otherwise messages
        /// will show up on all mobile applications. This does not affect
        /// other site types.
        /// </summary>
        /// <value>The related mobile site identifier.</value>
        [DataMember]
        public virtual SiteCache RelatedMobileApplicationSite => RelatedMobileApplicationSiteId.HasValue ? SiteCache.Get( RelatedMobileApplicationSiteId.Value ) : null;

        /// <summary>
        /// Gets the related TV site. If specified then messages
        /// will only show up on this TV application. Otherwise messages
        /// will show up on all TV applications. This does not affect other
        /// site types.
        /// </summary>
        /// <value>The related TV site identifier.</value>
        [DataMember]
        public virtual SiteCache RelatedTvApplicationSite => RelatedTvApplicationSiteId.HasValue ? SiteCache.Get( RelatedTvApplicationSiteId.Value ) : null;

        /// <summary>
        /// Gets the field.
        /// </summary>
        /// <value>
        /// The field.
        /// </value>
        internal NotificationMessageTypeComponent Component
        {
            get
            {
                if ( _component == null )
                {
                    var entityTypeCache = EntityTypeCache.Get( EntityTypeId );

                    if ( entityTypeCache != null )
                    {
                        _component = NotificationMessageTypeContainer.GetComponent( entityTypeCache.GetEntityType() );
                    }
                }

                return _component;
            }
        }
        private NotificationMessageTypeComponent _component = null;

        #endregion

        #region Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            if ( !( entity is NotificationMessageType notificationMessageType ) )
            {
                return;
            }

            EntityTypeId = notificationMessageType.EntityTypeId;
            Key = notificationMessageType.Key;
            ComponentDataJson = notificationMessageType.ComponentDataJson;
            IsDeletedOnRead = notificationMessageType.IsDeletedOnRead;
            IsWebSupported = notificationMessageType.IsWebSupported;
            IsMobileApplicationSupported = notificationMessageType.IsMobileApplicationSupported;
            IsTvApplicationSupported = notificationMessageType.IsTvApplicationSupported;
            RelatedWebSiteId = notificationMessageType.RelatedWebSiteId;
            RelatedMobileApplicationSiteId = notificationMessageType.RelatedMobileApplicationSiteId;
            RelatedTvApplicationSiteId = notificationMessageType.RelatedTvApplicationSiteId;
        }

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
}
