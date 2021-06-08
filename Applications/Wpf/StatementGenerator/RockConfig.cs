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
        private static RockConfig defaultInstance = ( ( RockConfig ) ( ApplicationSettingsBase.Synchronized( new RockConfig() ) ) );

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
        [DefaultSettingValue( "" )]
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
        [DefaultSettingValue( "" )]
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
        /// Gets or sets the temporary directory.
        /// </summary>
        /// <value>
        /// The temporary directory.
        /// </value>
        [DefaultSettingValue( "" )]
        [UserScopedSetting]
        public string TemporaryDirectory
        {
            get
            {
                return this["TemporaryDirectory"] as string;
            }

            set
            {
                this["TemporaryDirectory"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the individual save options json.
        /// </summary>
        /// <value>
        /// The individual save options json.
        /// </value>
        [DefaultSettingValue( "" )]
        [UserScopedSetting]
        public string IndividualSaveOptionsJson
        {
            get => this["IndividualSaveOptionsJson"] as string;
            set => this["IndividualSaveOptionsJson"] = value;
        }

        /// <summary>
        /// Gets or sets the report configuration list json.
        /// </summary>
        /// <value>
        /// The report configuration list json.
        /// </value>
        [DefaultSettingValue( "" )]
        [UserScopedSetting]
        public string ReportConfigurationListJson
        {
            get => this["ReportConfigurationListJson"] as string;
            set => this["ReportConfigurationListJson"] = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether [enable page count predetermination].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable page count predetermination]; otherwise, <c>false</c>.
        /// </value>
        [UserScopedSetting]
        public bool EnablePageCountPredetermination
        {
            get => this["EnablePageCountPredetermination"] as bool? ?? false;
            set => this["EnablePageCountPredetermination"] = value;
        }

        /// <summary>
        /// Gets or sets the person selection option.
        /// </summary>
        /// <value>
        /// The person selection option.
        /// </value>
        public PersonSelectionOption PersonSelectionOption { get; set; } = PersonSelectionOption.AllIndividuals;

        /// <summary>
        /// Gets or sets the layout defined value unique identifier.
        /// </summary>
        /// <value>
        /// The layout defined value unique identifier.
        /// </value>
        [DefaultSettingValueAttribute( "" )]
        [UserScopedSetting]
        public Guid? FinancialStatementTemplateGuid
        {
            get
            {
                return this["FinancialStatementTemplateGuid"] as Guid?;
            }

            set
            {
                this["FinancialStatementTemplateGuid"] = value;
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
