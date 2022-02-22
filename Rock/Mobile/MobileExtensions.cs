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
}
