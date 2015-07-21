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
using System.Text;

namespace Rock.Apps.CheckScannerUtility
{
    /// <summary>
    /// 
    /// </summary>
    internal sealed partial class RockConfig : ApplicationSettingsBase
    {
        /// <summary>
        /// The password, stored for the session, but not in the config file
        /// </summary>
        private static string sessionPassword = null;
        
        /// <summary>
        /// The default instance
        /// </summary>
        private static RockConfig defaultInstance = (RockConfig)ApplicationSettingsBase.Synchronized( new RockConfig() );

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
        /// Gets or sets the password (not stored in config, just session)
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password
        {
            get
            {
                return sessionPassword;
            }

            set
            {
                sessionPassword = value;
            }
        }

        /// <summary>
        /// Gets or sets the type of the image color.
        /// </summary>
        /// <value>
        /// The type of the image color.
        /// </value>
        [DefaultSettingValueAttribute( "0" )]
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
        /// Gets or sets the tender type value unique identifier.
        /// </summary>
        /// <value>
        /// The tender type value unique identifier.
        /// </value>
        [DefaultSettingValueAttribute( "" )]
        [UserScopedSetting]
        public string TenderTypeValueGuid
        {
            get
            {
                string result = this["TenderTypeValueGuid"] as string;
                if ( string.IsNullOrWhiteSpace( result ) )
                {
                    result = Rock.Client.SystemGuid.DefinedValue.CURRENCY_TYPE_CHECK;
                }

                return result;
            }

            set
            {
                this["TenderTypeValueGuid"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the source type value unique identifier.
        /// </summary>
        /// <value>
        /// The source type value unique identifier.
        /// </value>
        [DefaultSettingValueAttribute( "" )]
        [UserScopedSetting]
        public string SourceTypeValueGuid
        {
            get
            {
                string result = this["SourceTypeValueGuid"] as string;
                if ( string.IsNullOrWhiteSpace( result ) )
                {
                    result = Rock.Client.SystemGuid.DefinedValue.FINANCIAL_SOURCE_TYPE_ONSITE_COLLECTION;
                }

                return result;
            }

            set
            {
                this["SourceTypeValueGuid"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the scanner should scan both the front and rear sides (applies only to Ranger)
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable rear image]; otherwise, <c>false</c>.
        /// </value>
        [DefaultSettingValueAttribute( "true" )]
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
        /// Gets or sets a value indicating whether the UI should prompt to have the user scan the rear side (applies only to MagTek)
        /// </summary>
        /// <value>
        /// <c>true</c> if [prompt to scan rear image]; otherwise, <c>false</c>.
        /// </value>
        [DefaultSettingValueAttribute( "false" )]
        [UserScopedSetting]
        public bool PromptToScanRearImage
        {
            get
            {
                return this["PromptToScanRearImage"] as bool? ?? false;
            }

            set
            {
                this["PromptToScanRearImage"] = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the scanner should have "DoubleDocDetection" enabled
        /// </summary>
        /// <value>
        /// <c>true</c> if [enable double document detection]; otherwise, <c>false</c>.
        /// </value>
        [DefaultSettingValueAttribute( "true" )]
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
        /// Gets or sets a value indicating whether check scanning should warn when a bad MICR is detected. 
        /// This should normally be set to true, but they might want to set it to false if they are scanning a mixture of checks and envelopes, etc
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable smart scan]; otherwise, <c>false</c>.
        /// </value>
        [DefaultSettingValueAttribute( "true" )]
        [UserScopedSetting]
        public bool EnableSmartScan
        {
            get
            {
                return this["EnableSmartScan"] as bool? ?? true;
            }

            set
            {
                this["EnableSmartScan"] = value;
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
