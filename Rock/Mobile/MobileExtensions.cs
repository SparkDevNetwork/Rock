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
using Rock.Enums.Mobile;
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

        #endregion

        #region Email Preference

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

        #region Keyboard Input Mode

        /// <summary>
        /// Converts the <see cref="Rock.Enums.Core.KeyboardInputMode" /> to the mobile counterpart.
        /// </summary>
        /// <param name="keyboardInputMode"></param>
        /// <returns></returns>
        public static Rock.Common.Mobile.Enums.KeyboardInputMode ToMobile( this Rock.Enums.Core.KeyboardInputMode keyboardInputMode )
        {
            switch ( keyboardInputMode )
            {
                case Enums.Core.KeyboardInputMode.Default:
                    return Rock.Common.Mobile.Enums.KeyboardInputMode.Default;
                case Enums.Core.KeyboardInputMode.Email:
                    return Rock.Common.Mobile.Enums.KeyboardInputMode.Email;
                case Enums.Core.KeyboardInputMode.Numeric:
                    return Rock.Common.Mobile.Enums.KeyboardInputMode.Numeric;
                case Enums.Core.KeyboardInputMode.Decimal:
                    return Rock.Common.Mobile.Enums.KeyboardInputMode.Decimal;
                case Enums.Core.KeyboardInputMode.Telephone:
                    return Rock.Common.Mobile.Enums.KeyboardInputMode.Telephone;
                case Enums.Core.KeyboardInputMode.Text:
                    return Rock.Common.Mobile.Enums.KeyboardInputMode.Text;
                case Enums.Core.KeyboardInputMode.Url:
                    return Rock.Common.Mobile.Enums.KeyboardInputMode.Url;
                default:
                    return Rock.Common.Mobile.Enums.KeyboardInputMode.Default;
            }
        }

        #endregion

        #region Mobile Page Type

        ///<summary>
        /// Converts the specified Rock.Enums.Cms.MobilePageType to Rock.Common.Mobile.Enums.MobilePageType.
        ///</summary>
        ///<param name="mobilePageType">The Rock.Enums.Cms.MobilePageType value to convert.</param>
        ///<returns>The equivalent Rock.Common.Mobile.Enums.MobilePageType value.</returns>
        public static Rock.Common.Mobile.Enums.MobilePageType ToMobile( this Rock.Enums.Cms.MobilePageType mobilePageType )
        {
            switch ( mobilePageType )
            {
                case Rock.Enums.Cms.MobilePageType.NativePage:
                    return Rock.Common.Mobile.Enums.MobilePageType.NativePage;
                case Rock.Enums.Cms.MobilePageType.InternalWebPage:
                    return Rock.Common.Mobile.Enums.MobilePageType.InternalWebPage;
                case Rock.Enums.Cms.MobilePageType.ExternalWebPage:
                    return Rock.Common.Mobile.Enums.MobilePageType.ExternalWebPage;
                default:
                    return Rock.Common.Mobile.Enums.MobilePageType.NativePage;
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
        public static Rock.Common.Mobile.Enums.ConnectionState ToMobile( this Rock.Model.ConnectionState state )
        {
            switch ( state )
            {
                case Model.ConnectionState.Active:
                    return Rock.Common.Mobile.Enums.ConnectionState.Active;
                case Model.ConnectionState.Inactive:
                    return Rock.Common.Mobile.Enums.ConnectionState.Inactive;
                case Model.ConnectionState.FutureFollowUp:
                    return Rock.Common.Mobile.Enums.ConnectionState.FutureFollowUp;
                case Model.ConnectionState.Connected:
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
        public static Rock.Model.ConnectionState ToNative( this Rock.Common.Mobile.Enums.ConnectionState state )
        {
            switch ( state )
            {
                case Rock.Common.Mobile.Enums.ConnectionState.Active:
                    return Model.ConnectionState.Active;
                case Rock.Common.Mobile.Enums.ConnectionState.Inactive:
                    return Model.ConnectionState.Inactive;
                case Rock.Common.Mobile.Enums.ConnectionState.FutureFollowUp:
                    return Model.ConnectionState.FutureFollowUp;
                case Rock.Common.Mobile.Enums.ConnectionState.Connected:
                    return Model.ConnectionState.Connected;
                default:
                    return Model.ConnectionState.Active;
            }
        }

    }

    /// <summary>
    /// Extension methods for <see cref="Rock.Common.Mobile.Enums.Beacon.LocationPermissionStatus"/>.
    /// </summary>
    public static class MobileExtensionLocationPermissionStatus
    {
        /// <summary>
        /// Converts a mobile <see cref="Rock.Common.Mobile.Enums.Beacon.LocationPermissionStatus"/> to a web native.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static LocationPermissionStatus ToNative( this Rock.Common.Mobile.Enums.Beacon.LocationPermissionStatus status )
        {
            switch ( status )
            {
                case Rock.Common.Mobile.Enums.Beacon.LocationPermissionStatus.Always:
                    return LocationPermissionStatus.Always;
                case Rock.Common.Mobile.Enums.Beacon.LocationPermissionStatus.WhenInUse:
                    return LocationPermissionStatus.WhenInUse;
                case Rock.Common.Mobile.Enums.Beacon.LocationPermissionStatus.Denied:
                    return LocationPermissionStatus.Denied;
                case Rock.Common.Mobile.Enums.Beacon.LocationPermissionStatus.NotGranted:
                    return LocationPermissionStatus.NotGranted;
                default:
                    return LocationPermissionStatus.Always;
            }
        }

        /// <summary>
        /// Converts a web native <see cref="LocationPermissionStatus"/> to a mobile <see cref="Rock.Common.Mobile.Enums.Beacon.LocationPermissionStatus"/>.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static Rock.Common.Mobile.Enums.Beacon.LocationPermissionStatus ToMobile( this LocationPermissionStatus status )
        {
            switch ( status )
            {
                case LocationPermissionStatus.Always:
                    return Rock.Common.Mobile.Enums.Beacon.LocationPermissionStatus.Always;
                case LocationPermissionStatus.WhenInUse:
                    return Rock.Common.Mobile.Enums.Beacon.LocationPermissionStatus.WhenInUse;
                case LocationPermissionStatus.Denied:
                    return Rock.Common.Mobile.Enums.Beacon.LocationPermissionStatus.Denied;
                case LocationPermissionStatus.NotGranted:
                    return Rock.Common.Mobile.Enums.Beacon.LocationPermissionStatus.NotGranted;
                default:
                    return Rock.Common.Mobile.Enums.Beacon.LocationPermissionStatus.Always;
            }
        }
    }

    /// <summary>
    /// Extension methods for <see cref="Rock.Common.Mobile.Enums.RelationshipStrength"/>.
    /// </summary>
    internal static class MobileExtensionRelationshipStrength
    {
        /// <summary>
        /// Converts a web native <see cref="Enums.Engagement.RelationshipStrength"/> to a mobile <see cref="Rock.Common.Mobile.Enums.RelationshipStrength"/>.
        /// </summary>
        /// <param name="strength"></param>
        /// <returns></returns>
        public static Enums.Engagement.RelationshipStrength ToNative( this Rock.Common.Mobile.Enums.RelationshipStrength strength )
        {
            switch ( strength )
            {
                case Rock.Common.Mobile.Enums.RelationshipStrength.GettingToKnowThem:
                    return Enums.Engagement.RelationshipStrength.GettingToKnowThem;
                case Rock.Common.Mobile.Enums.RelationshipStrength.CasualAcquaintance:
                    return Enums.Engagement.RelationshipStrength.CasualAcquaintance;
                case Rock.Common.Mobile.Enums.RelationshipStrength.GrowingConnection:
                    return Enums.Engagement.RelationshipStrength.GrowingConnection;
                case Rock.Common.Mobile.Enums.RelationshipStrength.TrustedRelationship:
                    return Enums.Engagement.RelationshipStrength.TrustedRelationship;
                case Rock.Common.Mobile.Enums.RelationshipStrength.DeepAndMeaningfulConnection:
                    return Enums.Engagement.RelationshipStrength.DeepAndMeaningfulConnection;
                default:
                    return Enums.Engagement.RelationshipStrength.GettingToKnowThem;
            }
        }

        /// <summary>
        /// Converts a web native <see cref="Enums.Engagement.RelationshipStrength"/> to a mobile <see cref="Rock.Common.Mobile.Enums.RelationshipStrength"/>.
        /// </summary>
        /// <param name="strength"></param>
        /// <returns></returns>
        public static Rock.Common.Mobile.Enums.RelationshipStrength ToMobile( this Enums.Engagement.RelationshipStrength strength )
        {
            switch ( strength )
            {
                case Enums.Engagement.RelationshipStrength.GettingToKnowThem:
                    return Rock.Common.Mobile.Enums.RelationshipStrength.GettingToKnowThem;
                case Enums.Engagement.RelationshipStrength.CasualAcquaintance:
                    return Rock.Common.Mobile.Enums.RelationshipStrength.CasualAcquaintance;
                case Enums.Engagement.RelationshipStrength.GrowingConnection:
                    return Rock.Common.Mobile.Enums.RelationshipStrength.GrowingConnection;
                case Enums.Engagement.RelationshipStrength.TrustedRelationship:
                    return Rock.Common.Mobile.Enums.RelationshipStrength.TrustedRelationship;
                case Enums.Engagement.RelationshipStrength.DeepAndMeaningfulConnection:
                    return Rock.Common.Mobile.Enums.RelationshipStrength.DeepAndMeaningfulConnection;
                default:
                    return Rock.Common.Mobile.Enums.RelationshipStrength.GettingToKnowThem;
            }
        }
    }

    /// <summary>
    /// Extension methods for <see cref="Rock.Common.Mobile.Enums.RelationshipFocus"/>.
    /// </summary>
    internal static class MobileExtensionRelationShipFocus
    {
        /// <summary>
        /// Converts a mobile <see cref="Rock.Common.Mobile.Enums.RelationshipFocus"/> to a web native <see cref="Enums.Engagement.RelationshipFocus"/>.
        /// </summary>
        /// <param name="focus"></param>
        /// <returns></returns>
        public static Enums.Engagement.RelationshipFocus ToNative( this Rock.Common.Mobile.Enums.RelationshipFocus focus )
        {
            switch ( focus )
            {
                case Rock.Common.Mobile.Enums.RelationshipFocus.InvitationToFaith:
                    return Enums.Engagement.RelationshipFocus.InvitationToFaith;
                case Rock.Common.Mobile.Enums.RelationshipFocus.DeepeningFaith:
                    return Enums.Engagement.RelationshipFocus.DeepeningFaith;
                case Rock.Common.Mobile.Enums.RelationshipFocus.EncouragementAndCare:
                    return Enums.Engagement.RelationshipFocus.EncouragementAndCare;
                case Rock.Common.Mobile.Enums.RelationshipFocus.RestorationAndHealing:
                    return Enums.Engagement.RelationshipFocus.RestorationAndHealing;
                default:
                    return Enums.Engagement.RelationshipFocus.InvitationToFaith;
            }
        }

        /// <summary>
        /// Converts a web native <see cref="Enums.Engagement.RelationshipFocus"/> to a mobile <see cref="Rock.Common.Mobile.Enums.RelationshipFocus"/>.
        /// </summary>
        /// <param name="focus"></param>
        /// <returns></returns>
        public static Rock.Common.Mobile.Enums.RelationshipFocus ToMobile( this Enums.Engagement.RelationshipFocus focus )
        {
            switch ( focus )
            {
                case Enums.Engagement.RelationshipFocus.InvitationToFaith:
                    return Rock.Common.Mobile.Enums.RelationshipFocus.InvitationToFaith;
                case Enums.Engagement.RelationshipFocus.DeepeningFaith:
                    return Rock.Common.Mobile.Enums.RelationshipFocus.DeepeningFaith;
                case Enums.Engagement.RelationshipFocus.EncouragementAndCare:
                    return Rock.Common.Mobile.Enums.RelationshipFocus.EncouragementAndCare;
                case Enums.Engagement.RelationshipFocus.RestorationAndHealing:
                    return Rock.Common.Mobile.Enums.RelationshipFocus.RestorationAndHealing;
                default:
                    return Rock.Common.Mobile.Enums.RelationshipFocus.InvitationToFaith;
            }
        }
    }

    /// <summary>
    /// Extension methods for <see cref="Rock.Common.Mobile.Enums.OutreachCadence"/>.
    /// </summary>
    internal static class MobileExtensionOutreachCadence
    {
        /// <summary>
        /// Converts a mobile <see cref="Rock.Common.Mobile.Enums.OutreachCadence"/> to a web native <see cref="Enums.Engagement.OutreachCadence"/>.
        /// </summary>
        /// <param name="cadence"></param>
        /// <returns></returns>
        public static Enums.Engagement.OutreachCadence ToNative( this Rock.Common.Mobile.Enums.OutreachCadence cadence )
        {
            switch ( cadence )
            {
                case Rock.Common.Mobile.Enums.OutreachCadence.Weekly:
                    return Enums.Engagement.OutreachCadence.Weekly;
                case Rock.Common.Mobile.Enums.OutreachCadence.EveryOtherWeek:
                    return Enums.Engagement.OutreachCadence.EveryOtherWeek;
                case Rock.Common.Mobile.Enums.OutreachCadence.Monthly:
                    return Enums.Engagement.OutreachCadence.Monthly;
                case Rock.Common.Mobile.Enums.OutreachCadence.EveryOtherMonth:
                    return Enums.Engagement.OutreachCadence.EveryOtherMonth;
                case Rock.Common.Mobile.Enums.OutreachCadence.Quarterly:
                    return Enums.Engagement.OutreachCadence.Quarterly;
                case Rock.Common.Mobile.Enums.OutreachCadence.Paused:
                    return Enums.Engagement.OutreachCadence.Paused;
                case Rock.Common.Mobile.Enums.OutreachCadence.Daily:
                    return Enums.Engagement.OutreachCadence.Daily;
                default:
                    return Enums.Engagement.OutreachCadence.EveryOtherWeek;
            }
        }

        /// <summary>
        /// Converts a web native <see cref="Enums.Engagement.OutreachCadence"/> to a mobile <see cref="Rock.Common.Mobile.Enums.OutreachCadence"/>.
        /// </summary>
        /// <param name="cadence"></param>
        /// <returns></returns>
        public static Rock.Common.Mobile.Enums.OutreachCadence ToMobile( this Enums.Engagement.OutreachCadence cadence )
        {
            switch ( cadence )
            {
                case Enums.Engagement.OutreachCadence.Weekly:
                    return Rock.Common.Mobile.Enums.OutreachCadence.Weekly;
                case Enums.Engagement.OutreachCadence.EveryOtherWeek:
                    return Rock.Common.Mobile.Enums.OutreachCadence.EveryOtherWeek;
                case Enums.Engagement.OutreachCadence.Monthly:
                    return Rock.Common.Mobile.Enums.OutreachCadence.Monthly;
                case Enums.Engagement.OutreachCadence.EveryOtherMonth:
                    return Rock.Common.Mobile.Enums.OutreachCadence.EveryOtherMonth;
                case Enums.Engagement.OutreachCadence.Quarterly:
                    return Rock.Common.Mobile.Enums.OutreachCadence.Quarterly;
                case Enums.Engagement.OutreachCadence.Paused:
                    return Rock.Common.Mobile.Enums.OutreachCadence.Paused;
                case Enums.Engagement.OutreachCadence.Daily:
                    return Rock.Common.Mobile.Enums.OutreachCadence.Daily;
                default:
                    return Rock.Common.Mobile.Enums.OutreachCadence.EveryOtherWeek;
            }

        }
    }

    /// <summary>
    /// Extension methods for <see cref="Rock.Common.Mobile.Enums.TouchpointType"/>.
    /// </summary>
    internal static class MobileExtensionTouchpointType
    {
        /// <summary>
        /// Converts a mobile <see cref="Rock.Common.Mobile.Enums.TouchpointType"/> to a web native <see cref="Enums.Engagement.TouchpointType"/>.
        /// </summary>
        /// <param name="touchpointType"></param>
        /// <returns></returns>
        public static Enums.Engagement.TouchpointType ToNative( this Rock.Common.Mobile.Enums.TouchpointType touchpointType )
        {
            switch ( touchpointType )
            {
                case Rock.Common.Mobile.Enums.TouchpointType.Prayer:
                    return Enums.Engagement.TouchpointType.Prayer;
                case Rock.Common.Mobile.Enums.TouchpointType.Connection:
                    return Enums.Engagement.TouchpointType.Connection;
                case Rock.Common.Mobile.Enums.TouchpointType.Reminder:
                    return Enums.Engagement.TouchpointType.Reminder;
                case Rock.Common.Mobile.Enums.TouchpointType.Pulse:
                    return Enums.Engagement.TouchpointType.Pulse;
                case Rock.Common.Mobile.Enums.TouchpointType.Birthday:
                    return Enums.Engagement.TouchpointType.Birthday;
                case Rock.Common.Mobile.Enums.TouchpointType.WeddingAnniversary:
                    return Enums.Engagement.TouchpointType.WeddingAnniversary;
                case Rock.Common.Mobile.Enums.TouchpointType.BaptismAnniversary:
                    return Enums.Engagement.TouchpointType.BaptismAnniversary;
                case Rock.Common.Mobile.Enums.TouchpointType.SalvationAnniversary:
                    return Enums.Engagement.TouchpointType.SalvationAnniversary;
                default:
                    return Enums.Engagement.TouchpointType.Prayer;
            }
        }

        /// <summary>
        /// Converts a web native <see cref="Enums.Engagement.TouchpointType"/> to a mobile <see cref="Rock.Common.Mobile.Enums.TouchpointType"/>.
        /// </summary>
        /// <param name="touchpointType"></param>
        /// <returns></returns>
        public static Rock.Common.Mobile.Enums.TouchpointType ToMobile( this Enums.Engagement.TouchpointType touchpointType )
        {
            switch ( touchpointType )
            {
                case Enums.Engagement.TouchpointType.Prayer:
                    return Rock.Common.Mobile.Enums.TouchpointType.Prayer;
                case Enums.Engagement.TouchpointType.Connection:
                    return Rock.Common.Mobile.Enums.TouchpointType.Connection;
                case Enums.Engagement.TouchpointType.Reminder:
                    return Rock.Common.Mobile.Enums.TouchpointType.Reminder;
                case Enums.Engagement.TouchpointType.Pulse:
                    return Rock.Common.Mobile.Enums.TouchpointType.Pulse;
                case Enums.Engagement.TouchpointType.Birthday:
                    return Rock.Common.Mobile.Enums.TouchpointType.Birthday;
                case Enums.Engagement.TouchpointType.WeddingAnniversary:
                    return Rock.Common.Mobile.Enums.TouchpointType.WeddingAnniversary;
                case Enums.Engagement.TouchpointType.BaptismAnniversary:
                    return Rock.Common.Mobile.Enums.TouchpointType.BaptismAnniversary;
                case Enums.Engagement.TouchpointType.SalvationAnniversary:
                    return Rock.Common.Mobile.Enums.TouchpointType.SalvationAnniversary;
                default:
                    return Rock.Common.Mobile.Enums.TouchpointType.Prayer;
            }
        }
    }

    /// <summary>
    /// Extension methods for <see cref="Rock.Common.Mobile.Enums.TouchpointCommunicationMedium"/>.
    /// </summary>
    internal static class MobileExtensionTouchpointCommuncationMedium
    {
        /// <summary>
        /// Converts a mobile <see cref="Rock.Common.Mobile.Enums.TouchpointCommunicationMedium"/> to a web native <see cref="Enums.Engagement.TouchpointCommunicationMedium"/>.
        /// </summary>
        /// <param name="medium"></param>
        /// <returns></returns>
        public static Enums.Engagement.TouchpointCommunicationMedium ToNative( this Rock.Common.Mobile.Enums.TouchpointCommunicationMedium medium )
        {
            switch ( medium )
            {
                case Rock.Common.Mobile.Enums.TouchpointCommunicationMedium.Call:
                    return Enums.Engagement.TouchpointCommunicationMedium.Call;
                case Rock.Common.Mobile.Enums.TouchpointCommunicationMedium.Text:
                    return Enums.Engagement.TouchpointCommunicationMedium.Text;
                case Rock.Common.Mobile.Enums.TouchpointCommunicationMedium.Email:
                    return Enums.Engagement.TouchpointCommunicationMedium.Email;
                case Rock.Common.Mobile.Enums.TouchpointCommunicationMedium.InPerson:
                    return Enums.Engagement.TouchpointCommunicationMedium.InPerson;
                default:
                    return Enums.Engagement.TouchpointCommunicationMedium.Call;
            }
        }

        /// <summary>
        /// Converts a web native <see cref="Enums.Engagement.TouchpointCommunicationMedium"/> to a mobile <see cref="Rock.Common.Mobile.Enums.TouchpointCommunicationMedium"/>.
        /// </summary>
        /// <param name="medium"></param>
        /// <returns></returns>
        public static Common.Mobile.Enums.TouchpointCommunicationMedium ToMobile( this Enums.Engagement.TouchpointCommunicationMedium medium )
        {
            switch ( medium )
            {
                case Enums.Engagement.TouchpointCommunicationMedium.Call:
                    return Rock.Common.Mobile.Enums.TouchpointCommunicationMedium.Call;
                case Enums.Engagement.TouchpointCommunicationMedium.Text:
                    return Rock.Common.Mobile.Enums.TouchpointCommunicationMedium.Text;
                case Enums.Engagement.TouchpointCommunicationMedium.Email:
                    return Rock.Common.Mobile.Enums.TouchpointCommunicationMedium.Email;
                case Enums.Engagement.TouchpointCommunicationMedium.InPerson:
                    return Rock.Common.Mobile.Enums.TouchpointCommunicationMedium.InPerson;
                default:
                    return Rock.Common.Mobile.Enums.TouchpointCommunicationMedium.Call;
            }
        }
    }

    /// <summary>
    /// Extension methods for <see cref="Rock.Common.Mobile.Enums.OutreachNotificationTimeOfDay"/>.
    /// </summary>
    internal static class MobileExtensionOutreachNotificationTimeOfDay
    {
        /// <summary>
        /// Converts a mobile <see cref="Rock.Common.Mobile.Enums.OutreachNotificationTimeOfDay"/> to a web native <see cref="Enums.Engagement.OutreachNotificationTimeOfDay"/>.
        /// </summary>
        /// <param name="timeOfDay"></param>
        /// <returns></returns>
        public static Enums.Engagement.OutreachNotificationTimeOfDay ToNative( this Rock.Common.Mobile.Enums.OutreachNotificationTimeOfDay timeOfDay )
        {
            switch ( timeOfDay )
            {
                case Rock.Common.Mobile.Enums.OutreachNotificationTimeOfDay.Morning:
                    return Enums.Engagement.OutreachNotificationTimeOfDay.Morning;
                case Rock.Common.Mobile.Enums.OutreachNotificationTimeOfDay.Afternoon:
                    return Enums.Engagement.OutreachNotificationTimeOfDay.Afternoon;
                case Rock.Common.Mobile.Enums.OutreachNotificationTimeOfDay.Evening:
                    return Enums.Engagement.OutreachNotificationTimeOfDay.Evening;
                default:
                    return Enums.Engagement.OutreachNotificationTimeOfDay.Morning;
            }
        }

        /// <summary>
        /// Converts a web native <see cref="Enums.Engagement.OutreachNotificationTimeOfDay"/> to a mobile <see cref="Rock.Common.Mobile.Enums.OutreachNotificationTimeOfDay"/>.
        /// </summary>
        /// <param name="timeOfDay"></param>
        /// <returns></returns>
        public static Rock.Common.Mobile.Enums.OutreachNotificationTimeOfDay ToMobile( this Enums.Engagement.OutreachNotificationTimeOfDay timeOfDay )
        {
            switch ( timeOfDay )
            {
                case Enums.Engagement.OutreachNotificationTimeOfDay.Morning:
                    return Rock.Common.Mobile.Enums.OutreachNotificationTimeOfDay.Morning;
                case Enums.Engagement.OutreachNotificationTimeOfDay.Afternoon:
                    return Rock.Common.Mobile.Enums.OutreachNotificationTimeOfDay.Afternoon;
                case Enums.Engagement.OutreachNotificationTimeOfDay.Evening:
                    return Rock.Common.Mobile.Enums.OutreachNotificationTimeOfDay.Evening;
                default:
                    return Rock.Common.Mobile.Enums.OutreachNotificationTimeOfDay.Morning;
            }
        }
    }
}