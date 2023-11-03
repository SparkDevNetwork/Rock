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
using Rock.Common.Mobile.Enums;
using Rock.Web.Cache;

namespace Rock.Mobile
{
    /// <summary>
    /// Extension methods to various standard Rock classes to help with Mobile usage.
    /// </summary>
    public static class MobileExtensions
    {
        #region DevicePlatform

        /// <summary>
        /// Gets the defined value identifier that matches this <see cref="DevicePlatform"/>.
        /// </summary>
        /// <param name="devicePlatform">The device platform.</param>
        /// <returns>A defined value identifier.</returns>
        public static int GetDevicePlatformValueId( this DevicePlatform devicePlatform )
        {
            if ( devicePlatform == DevicePlatform.iOS )
            {
                return DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSONAL_DEVICE_PLATFORM_IOS ).Id;
            }
            else if ( devicePlatform == DevicePlatform.Android )
            {
                return DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSONAL_DEVICE_PLATFORM_ANDROID ).Id;
            }
            else
            {
                return DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSONAL_DEVICE_PLATFORM_OTHER ).Id;
            }
        }

        #endregion

        #region Gender

        /// <summary>
        /// Converts the Rock <see cref="Rock.Model.Gender"/> enumeration to the mobile counterpart.
        /// </summary>
        /// <param name="gender">The gender.</param>
        /// <returns>The <see cref="Gender"/> equivalent.</returns>
        public static Gender ToMobile( this Rock.Model.Gender gender )
        {
            switch ( gender )
            {
                case Model.Gender.Male:
                    return Gender.Male;

                case Model.Gender.Female:
                    return Gender.Female;

                default:
                    return Gender.Unknown;
            }
        }

        /// <summary>
        /// Converts a <see cref="Rock.Model.DisplayInNavWhen"/> to a mobile <see cref="Rock.Common.Mobile.Enums.DisplayInNavWhen"/>.
        /// </summary>
        /// <param name="displayInNavWhen">The display in nav when.</param>
        /// <returns>DisplayInNavWhen.</returns>
        public static DisplayInNavWhen ToMobile( this Rock.Model.DisplayInNavWhen displayInNavWhen )
        {
            switch ( displayInNavWhen )
            {
                case Model.DisplayInNavWhen.Always:
                    return DisplayInNavWhen.Always;
                case Model.DisplayInNavWhen.Never:
                    return DisplayInNavWhen.Never;
                case Model.DisplayInNavWhen.WhenAllowed:
                    return DisplayInNavWhen.WhenAllowed;
                default:
                    return DisplayInNavWhen.Never;
            }
        }

        #endregion

        #region Communication Type

        /// <summary>
        /// Converts the communication type enum to mobile.
        /// </summary>
        /// <param name="communicationType">Type of the communication.</param>
        /// <returns>Rock.Common.Mobile.Enums.CommunicationType.</returns>
        public static Rock.Common.Mobile.Enums.CommunicationType ToMobile( this Rock.Model.CommunicationType communicationType )
        {
            switch ( communicationType )
            {
                case Model.CommunicationType.PushNotification:
                    return CommunicationType.PushNotification;

                case Model.CommunicationType.SMS:
                    return CommunicationType.Sms;

                case Model.CommunicationType.Email:
                    return CommunicationType.Email;

                default:
                    return CommunicationType.RecipientPreference;
            }
        }

        /// <summary>
        /// Convert the Email Preference Enum to Mobile Specific Email Preference
        /// </summary>
        /// <param name="emailPreference"></param>
        /// <returns></returns>
        public static Rock.Common.Mobile.Enums.EmailPreference ToMobile( this Model.EmailPreference emailPreference )
        {
            switch ( emailPreference )
            {
                case Model.EmailPreference.DoNotEmail:
                    return EmailPreference.DoNotEmail;
                case Model.EmailPreference.NoMassEmails:
                    return EmailPreference.NoMassEmails;

                default:
                    return EmailPreference.EmailAllowed;
            }
        }

        #endregion
    }

    /// <summary>
    /// Extension methods for <see cref="Rock.Common.Mobile.Enums.Gender"/>.
    /// </summary>
    public static class MobileExtensionsGender
    {
        /// <summary>
        /// Converts to mobile <see cref="Gender"/> to a web native <see cref="Rock.Model.Gender"/>.
        /// </summary>
        /// <param name="gender">The gender to be converted.</param>
        /// <returns>The local gender value.</returns>
        public static Rock.Model.Gender ToNative( this Gender gender )
        {
            switch ( gender )
            {
                case Gender.Male:
                    return Model.Gender.Male;

                case Gender.Female:
                    return Model.Gender.Female;

                default:
                    return Model.Gender.Unknown;
            }
        }
    }


    /// <summary>
    /// Extension methods for <see cref="Rock.Common.Mobile.Enums.CommunicationType"/>.
    /// </summary>
    public static class MobileExtensionsCommunicationPreference
    {
        /// <summary>
        /// Converts a mobile <see cref="EmailPreference"/> to a web native.
        /// </summary>
        /// <param name="communicationType">The email preference.</param>
        /// <returns>Rock.Model.CommunicationType.</returns>
        public static Rock.Model.CommunicationType ToNative( this CommunicationType communicationType )
        {
            switch ( communicationType )
            {
                case CommunicationType.RecipientPreference:
                    return Model.CommunicationType.RecipientPreference;
                case CommunicationType.PushNotification:
                    return Model.CommunicationType.PushNotification;
                case CommunicationType.Email:
                    return Model.CommunicationType.Email;
                case CommunicationType.Sms:
                    return Model.CommunicationType.SMS;
                default:
                    return Model.CommunicationType.RecipientPreference;
            }
        }
    }

    /// <summary>
    /// Extension methods for <see cref="Rock.Common.Mobile.Enums.EmailPreference"/>.
    /// </summary>
    public static class MobileExtensionsEmailPreference
    {
        /// <summary>
        /// Converts a mobile <see cref="EmailPreference"/> to a web native.
        /// </summary>
        /// <param name="emailPreference">The email preference.</param>
        /// <returns>Rock.Model.EmailPreference.</returns>
        public static Rock.Model.EmailPreference ToNative( this EmailPreference emailPreference )
        {
            switch ( emailPreference )
            {
                case EmailPreference.DoNotEmail:
                    return Model.EmailPreference.DoNotEmail;
                case EmailPreference.NoMassEmails:
                    return Model.EmailPreference.NoMassEmails;
                default:
                    return Model.EmailPreference.EmailAllowed;
            }
        }
    }

    /// <summary>
    /// Extension methods for the <see cref="Rock.Common.Mobile.Enums.ConnectionState" />.
    /// </summary>
    public static class MobileExtensionsConnectionState
    {
        /// <summary>
        /// Converts a web native <see cref="ConnectionState"/> to a mobile <see cref="Rock.Common.Mobile.Enums.ConnectionState"/>.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static Rock.Common.Mobile.Enums.ConnectionState ToMobile( this ConnectionState state )
        {
            switch ( state )
            {
                case ConnectionState.Active:
                    return Rock.Common.Mobile.Enums.ConnectionState.Active;
                case ConnectionState.Inactive:
                    return Rock.Common.Mobile.Enums.ConnectionState.Inactive;
                case ConnectionState.FutureFollowUp:
                    return Rock.Common.Mobile.Enums.ConnectionState.FutureFollowUp;
                case ConnectionState.Connected:
                    return Rock.Common.Mobile.Enums.ConnectionState.Connected;
                default:
                    return Rock.Common.Mobile.Enums.ConnectionState.Active;
            }
        }

        /// <summary>
        /// Converts a mobile <see cref="Rock.Common.Mobile.Enums.ConnectionState"/> to a web native.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static ConnectionState ToNative( this Rock.Common.Mobile.Enums.ConnectionState state )
        {
            switch ( state )
            {
                case Rock.Common.Mobile.Enums.ConnectionState.Active:
                    return ConnectionState.Active;
                case Rock.Common.Mobile.Enums.ConnectionState.Inactive:
                    return ConnectionState.Inactive;
                case Rock.Common.Mobile.Enums.ConnectionState.FutureFollowUp:
                    return ConnectionState.FutureFollowUp;
                case Rock.Common.Mobile.Enums.ConnectionState.Connected:
                    return ConnectionState.Connected;
                default:
                    return ConnectionState.Active;
            }
        }
    }
}
