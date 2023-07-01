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
using Rock.Web.Cache;

namespace Rock.Communication
{
    /// <summary>
    /// A root-level class for accessing core Rock Communications configuration, services and features.
    /// </summary>
    public static class CommunicationServicesHost
    {
        #region Communication Mediums

        /// <summary>
        /// Gets the configured Communication Medium Component for Email.
        /// </summary>
        /// <returns></returns>
        public static Rock.Communication.Medium.Email GetCommunicationMediumEmail()
        {
            return GetCommunicationMedium<Rock.Communication.Medium.Email>( SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() );
        }

        /// <summary>
        /// Gets the configured Communication Medium Component for Push Notification.
        /// </summary>
        /// <returns></returns>
        public static Rock.Communication.Medium.PushNotification GetCommunicationMediumPushNotification()
        {
            return GetCommunicationMedium<Rock.Communication.Medium.PushNotification>( SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() );
        }

        /// <summary>
        /// Gets the configured Communication Medium Component for SMS.
        /// </summary>
        /// <returns></returns>
        public static Rock.Communication.Medium.Sms GetCommunicationMediumSms()
        {
            return GetCommunicationMedium<Rock.Communication.Medium.Sms>( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() );
        }

        /// <summary>
        /// Gets the configured Communication Medium Component for SMS.
        /// </summary>
        /// <returns></returns>
        private static T GetCommunicationMedium<T>( Guid communicationMediumEntityTypeGuid )
            where T : MediumComponent
        {
            T medium = null;

            var mediumEntityId = EntityTypeCache.GetId( communicationMediumEntityTypeGuid );
            if ( mediumEntityId != null )
            {
                medium = MediumContainer.GetComponentByEntityTypeId( mediumEntityId ) as T;
            }

            return medium;
        }

        #endregion
    }
}
