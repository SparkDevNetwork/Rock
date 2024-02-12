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
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Rock.Security
{
    /// <summary>
    /// A grant for additional security to an individual they would not
    /// normally have. This can be used, for example, by UI controls that
    /// need to call an API to get information the user wouldn't normally
    /// have access to, but the block has explicitely given them permission.
    /// </summary>
    public class SecurityGrant
    {
        #region Fields

        /// <summary>
        /// The rules that make up this security grant.
        /// </summary>
        private List<SecurityGrantRule> _rules;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the version of the security grant.
        /// </summary>
        /// <value>The version of the security grant. This value is currently always 1.</value>
        [JsonProperty( "v" )]
        public int Version { get; private set; }

        /// <summary>
        /// Gets the date and time this security grant was created.
        /// </summary>
        /// <value>The date and time this security grant was created.</value>
        [JsonProperty( "cdt" )]
        public DateTime CreatedDateTime { get; private set; }

        /// <summary>
        /// Gets the date time this security grant expires.
        /// </summary>
        /// <value>The date time this security grant expires.</value>
        [JsonProperty( "edt" )]
        public DateTime ExpiresDateTime { get; private set; }

        /// <summary>
        /// Gets the rules that make up this security grant.
        /// </summary>
        /// <value>The rules that make up this security grant.</value>
        [JsonProperty( "r" )]
        public IReadOnlyList<SecurityGrantRule> Rules
        {
            get => _rules.AsReadOnly();
            private set => _rules = new List<SecurityGrantRule>( value );
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SecurityGrant"/> class.
        /// </summary>
        public SecurityGrant()
        {
            Version = 1;
            CreatedDateTime = RockDateTime.Now;
            ExpiresDateTime = RockDateTime.Now.AddMinutes( GetDefaultDurationInMinutes() );
            _rules = new List<SecurityGrantRule>();
        }

        /// <summary>
        /// Creates a security grant from a security grant token that was previously
        /// created by a call to the <see cref="ToToken"/> method.
        /// </summary>
        /// <param name="token">The security grant token.</param>
        /// <returns>A new instance of <see cref="SecurityGrant"/> that represents the information in the token.</returns>
        public static SecurityGrant FromToken( string token )
        {
            if ( token.IsNullOrWhiteSpace() )
            {
                return null;
            }

            var segments = token.Split( ';' );

            var json = Encryption.DecryptString( segments[2] );

            return JsonConvert.DeserializeObject<SecurityGrant>( json );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Encodes the security grant into a token that can be passed to another
        /// system and then later sent back here for decoding.
        /// </summary>
        /// <returns>A string that represents the security grant as an opaque token.</returns>
        public string ToToken()
        {
            var json = JsonConvert.SerializeObject( this );

            return $"{Version};{ExpiresDateTime.ToRockDateTimeOffset():O};{Encryption.EncryptString( json )}";
        }

        /// <summary>
        /// Adds a new rule to the security grant.
        /// </summary>
        /// <param name="rule">The rule to be added to the grant.</param>
        /// <returns>The instance that was called to allow chaining.</returns>
        public SecurityGrant AddRule( SecurityGrantRule rule )
        {
            _rules.Add( rule );

            return this;
        }

        /// <summary>
        /// Removes an existing rule from the security grant.
        /// </summary>
        /// <param name="rule">The rule to be removed from the grant.</param>
        /// <returns>The instance that was called to allow chaining.</returns>
        public SecurityGrant RemoveRule( SecurityGrantRule rule )
        {
            _rules.Remove( rule );

            return this;
        }

        /// <summary>
        /// Determines whether this instance is expired.
        /// </summary>
        /// <returns><c>true</c> if this instance is expired; otherwise, <c>false</c>.</returns>
        protected bool IsExpired()
        {
            var grantTokenEarliestDate = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.SECURITY_GRANT_TOKEN_EARLIEST_DATE ).AsDateTime();

            if ( grantTokenEarliestDate.HasValue && ExpiresDateTime < grantTokenEarliestDate.Value )
            {
                return false;
            }

            return RockDateTime.Now >= ExpiresDateTime;
        }

        /// <summary>
        /// Gets the default duration in minutes for new security grant tokens.
        /// </summary>
        /// <returns>An integer that represents the number of minutes a new grant token should be valid for.</returns>
        private static int GetDefaultDurationInMinutes()
        {
            return Math.Max( Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.DEFAULT_SECURITY_GRANT_TOKEN_DURATION ).AsIntegerOrNull() ?? 4320, 60 );
        }

        /// <summary>
        /// Determines whether the object should be granted access from any
        /// rules contained within this security grant.
        /// </summary>
        /// <param name="obj">The object to be checked for permission.</param>
        /// <param name="action">The security action being checked, such as VIEW or EDIT.</param>
        /// <returns><c>true</c> if any of the rules explicitely grant access to the object; otherwise, <c>false</c>.</returns>
        public bool IsAccessGranted( object obj, string action )
        {
            if ( IsExpired() )
            {
                return false;
            }

            foreach ( var rule in _rules )
            {
                if ( rule.Action == action && rule.IsAccessGranted( obj, action ) )
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}