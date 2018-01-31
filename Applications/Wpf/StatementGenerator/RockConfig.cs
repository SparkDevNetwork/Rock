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
using System.Configuration;

namespace Rock.Apps.StatementGenerator
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
        [DefaultSettingValueAttribute("")]
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
        /// Gets or sets the person selection option.
        /// </summary>
        /// <value>
        /// The person selection option.
        /// </value>
        public PersonSelectionOption PersonSelectionOption { get; set; } = PersonSelectionOption.AllIndividuals;

        /// <summary>
        /// Gets or sets a value indicating whether [show tax deductible accounts].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show tax deductible accounts]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowTaxDeductibleAccounts { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether [show non tax deductible accounts].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show non tax deductible accounts]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowNonTaxDeductibleAccounts { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether [show inactive accounts].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show inactive accounts]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowInactiveAccounts { get; set; } = false;

        /// <summary>
        /// Gets or sets the layout defined value unique identifier.
        /// </summary>
        /// <value>
        /// The layout defined value unique identifier.
        /// </value>
        [DefaultSettingValueAttribute( "" )]
        [UserScopedSetting]
        public Guid? LayoutDefinedValueGuid
        {
            get
            {
                return this["LayoutDefinedValueGuid"] as Guid?;
            }

            set
            {
                this["LayoutDefinedValueGuid"] = value;
            }
        }

        public static RockConfig Load()
        {
            return RockConfig.Default;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum PersonSelectionOption
    {
        /// <summary>
        /// All individuals
        /// </summary>
        AllIndividuals,

        /// <summary>
        /// The data view
        /// </summary>
        DataView,

        /// <summary>
        /// The single individual
        /// </summary>
        SingleIndividual
    }
}
