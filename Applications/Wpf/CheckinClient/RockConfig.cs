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
using System.Configuration;

namespace CheckinClient
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
        /// Gets or sets the checkin address.
        /// </summary>
        /// <value>
        /// The checkin address.
        /// </value>
        [DefaultSettingValueAttribute( "" )]
        [UserScopedSetting]
        public string CheckinAddress
        {
            get
            {
                string checkinAddress = this["CheckinAddress"] as string;
                if (string.IsNullOrWhiteSpace(checkinAddress))
                {
                    checkinAddress = "http://yourserver.com/checkin";
                }

                return checkinAddress;
            }

            set
            {
                this["CheckinAddress"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the printer override ip.
        /// </summary>
        /// <value>
        /// The printer override ip.
        /// </value>
        [DefaultSettingValueAttribute( "" )]
        [UserScopedSetting]
        public string PrinterOverrideIp
        {
            get
            {
                return ( this["PrinterOverrideIp"] as string ) ?? "";
            }

            set
            {
                this["PrinterOverrideIp"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the printer override local.
        /// </summary>
        /// <value>
        /// The printer override local.
        /// </value>
        [DefaultSettingValueAttribute( "" )]
        [UserScopedSetting]
        public string PrinterOverrideLocal
        {
            get
            {
                return ( this["PrinterOverrideLocal"] as string ) ?? "";
            }

            set
            {
                this["PrinterOverrideLocal"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the duration of the cache label.
        /// </summary>
        /// <value>
        /// The duration of the cache label.
        /// </value>
        [DefaultSettingValueAttribute( "" )]
        [UserScopedSetting]
        public int CacheLabelDuration
        {
            get
            {
                int? cacheLabelDuration = this["CacheLabelDuration"] as int?;
                if (!cacheLabelDuration.HasValue || cacheLabelDuration <= 0)
                {
                    cacheLabelDuration = 1440;
                }
                
                return cacheLabelDuration.Value;
            }

            set
            {
                this["CacheLabelDuration"] = value;
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
