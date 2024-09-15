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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Rock.Data;
using Rock.Model;
using Rock.Utility.Enums;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Security
{
    /// <summary>
    /// Class for handling rock security settings.
    /// </summary>
    public class SecuritySettingsService
    {
        private readonly List<ValidationResult> _validationResults;

        /// <summary>
        /// Gets the validation results.
        /// </summary>
        /// <value>
        /// The validation results.
        /// </value>
        public virtual List<ValidationResult> ValidationResults
        {
            get { return _validationResults; }
        }

        /// <summary>
        /// Gets the security settings.
        /// </summary>
        /// <value>
        /// The security settings.
        /// </value>
        public SecuritySettings SecuritySettings { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SecuritySettingsService"/> class.
        /// </summary>
        public SecuritySettingsService()
        {
            _validationResults = new List<ValidationResult>();
            var securitySettings = SystemSettings.GetValue( Rock.SystemKey.SystemSetting.ROCK_SECURITY_SETTINGS ).FromJsonOrNull<SecuritySettings>();
            if ( securitySettings == null )
            {
                securitySettings = GetDefaultSecuritySettings();
                this.SecuritySettings = securitySettings;

                try
                {
                    Save();
                }
                catch ( Exception ex )
                {
                    if ( IsInsertDuplicateKeySqlException( ex ) )
                    {
                        // A security settings record already exists if this exception was thrown,
                        // so get the latest security settings and move on.
                        securitySettings = SystemSettings.GetValue( Rock.SystemKey.SystemSetting.ROCK_SECURITY_SETTINGS ).FromJsonOrThrow<SecuritySettings>();
                        RefreshSecurityGroups( securitySettings );
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            else
            {
                RefreshSecurityGroups( securitySettings );
            }

            this.SecuritySettings = securitySettings;
        }

        /// <summary>
        /// Determines whether the <paramref name="ex"/> or one of its inner exceptions is a duplicate key insertion SQL exception.
        /// </summary>
        /// <param name="ex">The exception.</param>
        /// <returns>
        ///   <c>true</c> if the exception or one of its inner exceptions is a duplicate key insersion SQL exception; otherwise, <c>false</c>.
        /// </returns>
        private bool IsInsertDuplicateKeySqlException( Exception ex )
        {
            var exception = ex;

            while ( exception != null )
            {
                if ( exception is System.Data.SqlClient.SqlException sqlException && sqlException.Number == 2601 )
                {
                    return true;
                }

                exception = exception.InnerException;
            }

            return false;
        }

        /// <summary>
        /// Gets the default security settings.
        /// </summary>
        /// <returns></returns>
        private SecuritySettings GetDefaultSecuritySettings()
        {
            return new SecuritySettings
            {
                AccountProtectionProfilesForDuplicateDetectionToIgnore = new List<AccountProtectionProfile>
                {
                    AccountProtectionProfile.Extreme,
                    AccountProtectionProfile.High,
                    AccountProtectionProfile.Medium
                },
                AccountProtectionProfileSecurityGroup = new Dictionary<AccountProtectionProfile, RoleCache>
                {
                    { AccountProtectionProfile.Extreme, RoleCache.Get( SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() ) },
                    { AccountProtectionProfile.High, RoleCache.Get( SystemGuid.Group.GROUP_DATA_INTEGRITY_WORKER.AsGuid() ) }
                },
                DisablePasswordlessSignInForAccountProtectionProfiles = new List<AccountProtectionProfile>
                {
                    AccountProtectionProfile.Extreme
                },
                DisableTokensForAccountProtectionProfiles = new List<AccountProtectionProfile>
                {
                    AccountProtectionProfile.Extreme
                },
                RequireTwoFactorAuthenticationForAccountProtectionProfiles = new List<AccountProtectionProfile>(),
                PasswordlessSignInDailyIpThrottle = SecuritySettings.PasswordlessSignInDailyIpThrottleDefaultValue,
                PasswordlessConfirmationCommunicationTemplateGuid = Rock.SystemGuid.SystemCommunication.SECURITY_CONFIRM_LOGIN_PASSWORDLESS.AsGuid(),
                PasswordlessSignInSessionDuration = SecuritySettings.PasswordlessSignInSessionDurationDefaultValue,
                DisablePredictableIds = false
            };
        }

        /// <summary>
        /// Refreshes the security groups.
        /// </summary>
        /// <param name="securitySettings">The security settings.</param>
        private void RefreshSecurityGroups(SecuritySettings securitySettings)
        {
            var keys = securitySettings.AccountProtectionProfileSecurityGroup.Keys.ToList();
            foreach ( var key in keys )
            {
                var roleCache = securitySettings.AccountProtectionProfileSecurityGroup[key];
                securitySettings.AccountProtectionProfileSecurityGroup[key] = RoleCache.Get( roleCache.Id );
            }
        }

        /// <summary>
        /// Saves the SecuritySettings data.
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            if ( Validate() )
            {
                SystemSettings.SetValue( Rock.SystemKey.SystemSetting.ROCK_SECURITY_SETTINGS, this.SecuritySettings.ToJson(), SystemGuid.Attribute.SYSTEM_SECURITY_SETTINGS.AsGuid() );
                return true;
            }

            return false;
        }

        /// <summary>
        /// Validates the SecuritySettings data.
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            var valContext = new ValidationContext( this.SecuritySettings, serviceProvider: null, items: null );
            var isValid = Validator.TryValidateObject( this.SecuritySettings, valContext, _validationResults, true );

            if ( SecuritySettings?.AccountProtectionProfilesForDuplicateDetectionToIgnore == null )
            {
                ValidationResults.Add( new ValidationResult( "The account protection profile list is null." ) );
                isValid = false;
            }

            // Validate Groups are security groups.
            var securityGroupsToValidate = SecuritySettings?.AccountProtectionProfileSecurityGroup?.Values.ToList();
            if ( securityGroupsToValidate == null )
            {
                // The only way invalidGroups would be null is if the SecuritySettings or property is null.
                ValidationResults.Add( new ValidationResult( "The account protection profile security group list is null." ) );
                isValid = false;
            }

            return isValid;
        }
    }
}
