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

using System.Collections.Generic;
using System.Configuration;
using System.Web.Configuration;

namespace Rock.Configuration
{
    /// <summary>
    /// The WebForms implementation of <see cref="InitializationSettings"/>.
    /// </summary>
    internal class WebFormsInitializationSettings : InitializationSettings
    {
        /// <summary>
        /// Creates a new <see cref="WebFormsInitializationSettings"/> instance
        /// and loads all the settings from the web.config file.
        /// </summary>
        public WebFormsInitializationSettings( IConnectionStringProvider connectionStringProvider )
            : base( connectionStringProvider )
        {
            var configuration = WebConfigurationManager.OpenWebConfiguration( "~" );
            var settings = configuration.AppSettings.Settings;

            IsRunScheduledJobsEnabled = settings["RunJobsInIISContext"]?.Value.AsBoolean() ?? false;
            OrganizationTimeZone = settings["OrgTimeZone"]?.Value.ToStringSafe();
            PasswordKey = settings["PasswordKey"]?.Value.ToStringSafe();
            DataEncryptionKey = settings["DataEncryptionKey"]?.Value.ToStringSafe();
            RockStoreUrl = settings["RockStoreUrl"]?.Value.ToStringSafe();
            IsDuplicateGroupMemberRoleAllowed = settings["AllowDuplicateGroupMembers"]?.Value.AsBoolean() ?? false;
            IsCacheStatisticsEnabled = settings["CacheManagerEnableStatistics"]?.Value.AsBoolean() ?? false;
            ObservabilityServiceName = settings["ObservabilityServiceName"]?.Value.ToStringSafe();
            AzureSignalREndpoint = settings["AzureSignalREndpoint"]?.Value.ToStringSafe();
            AzureSignalRAccessKey = settings["AzureSignalRAccessKey"]?.Value.ToStringSafe();
            SparkApiUrl = settings["SparkApiUrl"]?.Value.ToStringSafe();
            NodeName = settings["NodeName"]?.Value.ToStringSafe();

            // Load old password keys.
            var oldPasswordKeys = new List<string>();
            for (int i = 0; ; i++)
            {
                var passwordKey = settings[$"OldPasswordKey{i}"]?.Value;

                if (passwordKey.IsNullOrWhiteSpace())
                {
                    break;
                }

                oldPasswordKeys.Add( passwordKey );
            }

            // Load old decryption keys.
            var oldDataEncryptionKeys = new List<string>();
            for ( int i = 0; ; i++ )
            {
                var dataEncryptionKey = settings[$"OldDataEncryptionKey{i}"]?.Value;

                if ( dataEncryptionKey.IsNullOrWhiteSpace() )
                {
                    break;
                }

                oldDataEncryptionKeys.Add( dataEncryptionKey );
            }

            OldPasswordKeys = oldPasswordKeys;
            OldDataEncryptionKeys = oldDataEncryptionKeys;
        }

        /// <inheritdoc/>
        public override void Save()
        {
            var rockWebConfig = WebConfigurationManager.OpenWebConfiguration( "~" );
            var settings = rockWebConfig.AppSettings.Settings;
            var webConfigModified = false;

            // Use |= as a bitwise or so that SetValue gets called regardless
            // of the value in modified variable.
            webConfigModified |= SetValue( settings, "RunJobsInIISContext", IsRunScheduledJobsEnabled );
            webConfigModified |= SetValue( settings, "OrgTimeZone", OrganizationTimeZone );
            webConfigModified |= SetValue( settings, "PasswordKey", PasswordKey );
            webConfigModified |= SetValue( settings, "DataEncryptionKey", DataEncryptionKey );
            webConfigModified |= SetValue( settings, "RockStoreUrl", RockStoreUrl );
            webConfigModified |= SetValue( settings, "AllowDuplicateGroupMembers", IsDuplicateGroupMemberRoleAllowed );
            webConfigModified |= SetValue( settings, "CacheManagerEnableStatistics", IsCacheStatisticsEnabled );
            webConfigModified |= SetValue( settings, "ObservabilityServiceName", ObservabilityServiceName );
            webConfigModified |= SetValue( settings, "AzureSignalREndpoint", AzureSignalREndpoint );
            webConfigModified |= SetValue( settings, "AzureSignalRAccessKey", AzureSignalRAccessKey );
            webConfigModified |= SetValue( settings, "SparkApiUrl", SparkApiUrl );
            webConfigModified |= SetValue( settings, "NodeName", NodeName );

            // Note: Updating connection strings is not currently supported.

            // Only save if any value actually changed. Otherwise we might
            // trigger a restart when we shouldn't.
            if ( webConfigModified )
            {
                rockWebConfig.Save();
            }
        }

        /// <summary>
        /// Updates or adds the key and value to the settings collection.
        /// </summary>
        /// <param name="settings">The settings to be modified.</param>
        /// <param name="key">The key to add or update.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if any change was actually made; otherwise <c>false</c>.</returns>
        private bool SetValue( KeyValueConfigurationCollection settings, string key, string value )
        {
            if ( settings[key] == null )
            {
                // If the key does not already exist in the app settings and the
                // new value is null or empty then we can skip it. This keeps us
                // from polluting a stock web.config with extra values that are
                // just empty.
                if ( !value.IsNullOrWhiteSpace() )
                {
                    settings.Add( key, value );

                    return true;
                }
            }
            else
            {
                if ( settings[key].Value != value )
                {
                    settings[key].Value = value;

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Updates or adds the key and value to the settings collection.
        /// </summary>
        /// <param name="settings">The settings to be modified.</param>
        /// <param name="key">The key to add or update.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if any change was actually made; otherwise <c>false</c>.</returns>
        private bool SetValue( KeyValueConfigurationCollection settings, string key, bool value )
        {
            if ( settings[key] == null )
            {
                // If the key does not already exist in the app settings and the
                // new value is false then we can skip it. This keeps us from
                // polluting a stock web.config with extra values that are
                // just empty.
                if ( value )
                {
                    settings.Add( key, value.ToString() );

                    return true;
                }
            }
            else
            {
                if ( settings[key].Value.AsBoolean() != value )
                {
                    settings[key].Value = value.ToString();

                    return true;
                }
            }

            return false;
        }
    }
}
