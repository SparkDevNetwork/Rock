// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Rock.Apps.CheckScannerUtility
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class RockConfig : ApplicationSettingsBase
    {
        /// <summary>
        /// The default instance
        /// </summary>
        private static RockConfig defaultInstance = ( (RockConfig)( ApplicationSettingsBase.Synchronized( new RockConfig() ) ) );

        /// <summary>
        /// Gets the default.
        /// </summary>
        /// <value>
        /// The default.
        /// </value>
        public static RockConfig Default
        {
            get
            {
                return defaultInstance;
            }
        }

        /// <summary>
        /// Gets or sets the rock base URL.
        /// </summary>
        /// <value>
        /// The rock base URL.
        /// </value>
        [DefaultSettingValueAttribute( "" )]
        [UserScopedSetting]
        public string RockBaseUrl
        {
            get
            {
                return this["RockBaseUrl"] as string;
            }

            set
            {
                this["RockBaseUrl"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        [DefaultSettingValueAttribute( "" )]
        [UserScopedSetting]
        public string Username
        {
            get
            {
                return this["Username"] as string;
            }

            set
            {
                this["Username"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        [DefaultSettingValueAttribute( "" )]
        [UserScopedSetting]
        public string PasswordEncryptedBase64
        {
            get
            {
                return this["PasswordEncryptedBase64"] as string;

            }

            set
            {
                this["PasswordEncryptedBase64"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the password encrypted.
        /// </summary>
        /// <value>
        /// The password encrypted.
        /// </value>
        private byte[] PasswordEncrypted
        {
            get
            {
                return Convert.FromBase64String( PasswordEncryptedBase64 );
            }

            set
            {
                PasswordEncryptedBase64 = Convert.ToBase64String( value );
            }
        }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password
        {
            get
            {
                try
                {
                    byte[] clearTextPasswordBytes = ProtectedData.Unprotect( PasswordEncrypted ?? new byte[] { 0 }, null, DataProtectionScope.CurrentUser );
                    return Encoding.Unicode.GetString( clearTextPasswordBytes );
                }
                catch ( CryptographicException )
                {
                    return string.Empty;
                }
            }

            set
            {
                try
                {
                    byte[] clearTextPasswordBytes = Encoding.Unicode.GetBytes( value );
                    PasswordEncrypted = ProtectedData.Protect( clearTextPasswordBytes, null, DataProtectionScope.CurrentUser );
                }
                catch ( CryptographicException )
                {
                    PasswordEncrypted = new byte[] { 0 };
                }
            }
        }

        /// <summary>
        /// Gets or sets the type of the image color.
        /// </summary>
        /// <value>
        /// The type of the image color.
        /// </value>
        [DefaultSettingValueAttribute( "1" ) ]
        [UserScopedSetting]
        public ImageColorType ImageColorType
        {
            get
            {
                return (ImageColorType)this["ImageColorType"];

            }

            set
            {
                this["ImageColorType"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the MICR image COM port.
        /// </summary>
        /// <value>
        /// The MICR image COM port.
        /// </value>
        [DefaultSettingValueAttribute( "1" )]
        [UserScopedSetting]
        public short MICRImageComPort
        {
            get
            {
                return (short)this["MICRImageComPort"];

            }

            set
            {
                this["MICRImageComPort"] = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public enum InterfaceType
        {
            RangerApi = 0,
            MICRImageRS232 = 1
        }

        /// <summary>
        /// Gets or sets the type of the scanner interface.
        /// </summary>
        /// <value>
        /// The type of the scanner interface.
        /// </value>
        [DefaultSettingValueAttribute( "0" )]
        [UserScopedSetting]
        public InterfaceType ScannerInterfaceType
        {
            get
            {
                return (InterfaceType)this["ScannerInterfaceType"];

            }

            set
            {
                this["ScannerInterfaceType"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the tender type unique identifier.
        /// </summary>
        /// <value>
        /// The tender type unique identifier.
        /// </value>
        [UserScopedSetting]
        public Guid TenderTypeValueGuid
        {
            get
            {
                return ( this["TenderTypeValueGuid"] as string ).AsGuidOrNull() ?? Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK.AsGuid();

            }

            set
            {
                this["TenderTypeValueGuid"] = value.ToString();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the scanner should scan both the front and rear sides
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable rear image]; otherwise, <c>false</c>.
        /// </value>
        [UserScopedSetting]
        public bool EnableRearImage
        {
            get
            {
                return this["EnableRearImage"] as bool? ?? true;
            }

            set
            {
                this["EnableRearImage"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the scanner should have "DoubleDocDetection" enabled
        /// </summary>
        /// <value>
        /// <c>true</c> if [enable double document detection]; otherwise, <c>false</c>.
        /// </value>
        [UserScopedSetting]
        public bool EnableDoubleDocDetection
        {
            get
            {
                return this["EnableDoubleDocDetection"] as bool? ?? true;
            }

            set
            {
                this["EnableDoubleDocDetection"] = value;
            }
        }

        /// <summary>
        /// Loads this instance.
        /// </summary>
        /// <returns></returns>
        public static RockConfig Load()
        {
            return RockConfig.Default;
        }
    }
}
