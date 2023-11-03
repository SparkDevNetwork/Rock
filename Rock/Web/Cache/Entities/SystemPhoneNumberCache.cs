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
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
#if REVIEW_WEBFORMS
using System.Data.Entity.Spatial;
#endif
using Rock.Lava;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Cache for the <see cref="SystemPhoneNumber"/> objects to provide fast
    /// access since these will be used quite often with SMS conversations.
    /// </summary>
    [Serializable]
    [DataContract]
    public class SystemPhoneNumberCache : ModelCache<SystemPhoneNumberCache, SystemPhoneNumber>
    {
        #region Properties

        /// <summary>
        /// Gets the friendly name of the phone number.
        /// </summary>
        /// <value>
        /// The friendly name of the phone number.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets the phone number. This should be in E.123 format,
        /// such as <c>+16235553324</c>.
        /// </summary>
        /// <value>
        /// The phone number.
        /// </value>
        [DataMember]
        public string Number { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this phone number is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this phone number is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; private set; }

        /// <summary>
        /// Gets or sets the sort and display order of the <see cref="SystemPhoneNumberCache"/>.
        /// This is an ascending order, so the lower the value the higher the
        /// sort priority.
        /// </summary>
        /// <value>
        /// A <see cref="int"/> representing the sort order of the <see cref="SystemPhoneNumberCache"/>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        /// <summary>
        /// Gets the identifier of the person alias who should receive
        /// responses to the SMS number. This person must have a phone number
        /// with SMS enabled or no response will be sent.
        /// </summary>
        /// <value>The person alias identifier who should receive responses.</value>
        [DataMember]
        public int? AssignedToPersonAliasId { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this instance support SMS.
        /// </summary>
        /// <value><c>true</c> if this instance supports SMS; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsSmsEnabled { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this phone number will
        /// forward incoming messages to <see cref="AssignedToPersonAliasId"/>.
        /// </summary>
        /// <value><c>true</c> if this phohe number will forward incoming messages; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsSmsForwardingEnabled { get; private set; }

        /// <summary>
        /// Gets the identifier of the workflow type that will be
        /// launched when an SMS message is received on this number.
        /// </summary>
        /// <value>The workflow type identifier to be launched when an SMS message is received.</value>
        [DataMember]
        public int? SmsReceivedWorkflowTypeId { get; private set; }

        /// <summary>
        /// Gets the notification group identifier. Active members of
        /// this group will be notified when a new SMS message comes in to
        /// this phone number.
        /// </summary>
        /// <value>The SMS notification group identifier.</value>
        [DataMember]
        public int? SmsNotificationGroupId { get; private set; }

        /// <summary>
        /// Gets the mobile application site identifier. This is
        /// used when determining what devices to send push notifications to.
        /// </summary>
        /// <value>The mobile application site identifier.</value>
        [DataMember]
        public int? MobileApplicationSiteId { get; private set; }

        /// <summary>
        /// Gets the provider identifier.
        /// </summary>
        /// <value>
        /// The provider identifier.
        /// </value>
        [DataMember]
        public string ProviderIdentifier { get; private set; }

        #endregion

        #region Related Cache

        /// <summary>
        /// Gets the workflow type that will be launched when an
        /// SMS message is received on this number.
        /// </summary>
        /// <value>The workflow type to be launched when an SMS message is received.</value>
        public virtual WorkflowTypeCache SmsReceivedWorkflowType => SmsReceivedWorkflowTypeId.HasValue ? WorkflowTypeCache.Get( SmsReceivedWorkflowTypeId.Value ) : null;

        /// <summary>
        /// Gets the SMS mobile application site. This is used
        /// when determining what devices to send push notifications to.
        /// </summary>
        /// <value>The SMS mobile application site.</value>
        public virtual SiteCache MobileApplicationSite => MobileApplicationSiteId.HasValue ? SiteCache.Get( MobileApplicationSiteId.Value ) : null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            if ( !( entity is SystemPhoneNumber systemPhoneNumber ) )
            {
                return;
            }

            Name = systemPhoneNumber.Name;
            Description = systemPhoneNumber.Description;
            Number = systemPhoneNumber.Number;
            IsActive = systemPhoneNumber.IsActive;
            Order = systemPhoneNumber.Order;
            AssignedToPersonAliasId = systemPhoneNumber.AssignedToPersonAliasId;
            IsSmsEnabled = systemPhoneNumber.IsSmsEnabled;
            IsSmsForwardingEnabled = systemPhoneNumber.IsSmsForwardingEnabled;
            SmsReceivedWorkflowTypeId = systemPhoneNumber.SmsReceivedWorkflowTypeId;
            SmsNotificationGroupId = systemPhoneNumber.SmsNotificationGroupId;
            MobileApplicationSiteId = systemPhoneNumber.MobileApplicationSiteId;
            ProviderIdentifier = systemPhoneNumber.ProviderIdentifier;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Returns all system phone numbers from cache ordered by the order property.
        /// </summary>
        /// <returns></returns>
        public static new List<SystemPhoneNumberCache> All()
        {
            // Calls the method below including inactive campuses too.
            return All( true );
        }

        /// <summary>
        /// Returns all campuses from cache ordered by the campus's order property.
        /// </summary>
        /// <param name="includeInactive">if set to true to include inactive campuses; otherwise set to false to exclude them.</param>
        /// <returns></returns>
        public static List<SystemPhoneNumberCache> All( bool includeInactive )
        {
            // WARNING: We need to call the All(RockContext) static method here, not the All(bool) or All() static method. Otherwise a stack overflow loop will occur.
            var allSystemPhoneNumbers = All( null );

            if ( includeInactive )
            {
                return allSystemPhoneNumbers.OrderBy( c => c.Order ).ToList();
            }

            return allSystemPhoneNumbers.Where( c => c.IsActive ).OrderBy( c => c.Order ).ToList();
        }

        #endregion
    }
}