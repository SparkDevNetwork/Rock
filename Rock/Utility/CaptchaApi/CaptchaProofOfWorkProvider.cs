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
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Rock.Enums.Cms;
using Rock.Security;
using Rock.SystemKey;
using Rock.Web;

namespace Rock.Utility.CaptchaApi
{
    /// <summary>
    /// Provides functionality to generate and verify CAPTCHA challenges using proof-of-work mechanisms.
    /// </summary>
    internal sealed class CaptchaProofOfWorkProvider : ICaptchaProvider
    {
        #region Constants

        private const int DefaultChallengeCount = 50;
        private const int DefaultChallengeSize = 32;
        private const int DefaultChallengeDifficulty = 4;

        #endregion Constants

        #region ICaptchaProvider Methods

        /// <inheritdoc/>
        public Task<CaptchaInitializeResult> InitializeAsync()
        {
            // Seed the challenge. We encrypt this so the client can echo it back.
            var seed = new ChallengeSeed
            {
                Guid = Guid.NewGuid(),
                ChallengeCount = DefaultChallengeCount,
                ChallengeSize = DefaultChallengeSize,
                ChallengeDifficulty = DefaultChallengeDifficulty
            };

            var challengeToken = Encryption.EncryptString( seed.ToJson() );

            return Task.FromResult( new CaptchaInitializeResult
            {
                Pow = new CaptchaInitializeProofOfWorkResult
                {
                    ChallengeToken = challengeToken,
                    ChallengeCount = seed.ChallengeCount,
                    ChallengeSize = seed.ChallengeSize,
                    ChallengeDifficulty = seed.ChallengeDifficulty
                }
            } );
        }

        /// <inheritdoc/>
        public Task<CaptchaVerifyResult> VerifyAsync( CaptchaVerifyOptions options )
        {
            var powOptions = options.PowOptions;

            if ( powOptions?.ChallengeSolutions == null || powOptions.ChallengeSolutions.Count == 0 )
            {
                return Task.FromResult( CaptchaVerifyResult.Fail( "No solutions provided." ) );
            }

            // Rehydrate original seed from encrypted token.
            var seed = Encryption.DecryptString( powOptions.ChallengeToken )
                .FromJsonOrNull<ChallengeSeed>();

            if ( seed == null )
            {
                return Task.FromResult( CaptchaVerifyResult.Fail( "Invalid challenge token." ) );
            }

            if ( seed.ChallengeCount <= 0 || seed.ChallengeSize <= 0 || seed.ChallengeDifficulty <= 0 )
            {
                return Task.FromResult( CaptchaVerifyResult.Fail( "Invalid challenge configuration." ) );
            }

            if ( powOptions.ChallengeSolutions.Count != seed.ChallengeCount )
            {
                return Task.FromResult( CaptchaVerifyResult.Fail( "Invalid solution count." ) );
            }

            // Verify each mini-puzzle deterministically. This must match what the client did.
            var allValid = Enumerable.Range( 0, seed.ChallengeCount ).All( i =>
            {
                var salt = GetPseudoRandomString( $"{powOptions.ChallengeToken}{i + 1}", seed.ChallengeSize );
                var target = GetPseudoRandomString( $"{powOptions.ChallengeToken}{i + 1}d", seed.ChallengeDifficulty );
                var solution = powOptions.ChallengeSolutions[i];

                var hash = GetSha256Hex( salt + solution );
                return hash.StartsWith( target, StringComparison.InvariantCultureIgnoreCase );
            } );

            if ( !allValid )
            {
                return Task.FromResult( CaptchaVerifyResult.Fail( "Invalid challenge solution." ) );
            }

            var captchaToken = new CaptchaToken
            {
                Guid = Guid.NewGuid(),
                TokenIssueDate = RockDateTime.Now,

                /*
                     10/20/2025 - JMH

                     We're setting the CAPTCHA token expiration to just under 24 hours to accommodate two constraints:
                     1. We don't want an expiration in this first iteration
                        because we want individuals to have ample time to complete forms
                        without the CAPTCHA expiring mid-process.
                        However, the CAPTCHA client library requires an expiration value.
                     2. The library also throws an error if the expiration is exactly 24 hours
                        due to potential clock skew on the viewer's device.

                     Reason: Enforced a max-safe token expiration to meet library constraints while minimizing user-facing disruptions.
                */
                TokenLifetime = TimeSpan.FromDays( 1 ).Add( TimeSpan.FromMinutes( -2 ) )
            };

            var encryptedToken = Encryption.EncryptString( captchaToken.ToJson() );

            return Task.FromResult( CaptchaVerifyResult.Pass( encryptedToken, captchaToken.TokenIssueDate, captchaToken.TokenLifetime ) );
        }

        /// <inheritdoc/>
        public Task<bool> IsTokenValidAsync( string token )
        {
            var mode = SystemSettings.GetValue( SystemSetting.CAPTCHA_MODE )
                .ConvertToEnum<CaptchaMode>( CaptchaMode.Visible );

            if ( mode == CaptchaMode.Disabled )
            {
                return Task.FromResult( true );
            }

            if ( token.IsNullOrWhiteSpace() )
            {
                return Task.FromResult( false );
            }

            var payload = Encryption.DecryptString( token ).FromJsonOrNull<CaptchaToken>();

            if (payload == null )
            {
                return Task.FromResult( false );
            }

            return Task.FromResult( RockDateTime.Now < payload.TokenIssueDate.Add( payload.TokenLifetime ) );
        }

        #endregion ICaptchaProvider Methods

        #region Private Methods

        private static string GetPseudoRandomString( string seed, int length )
        {
            if ( seed == null )
            {
                throw new ArgumentNullException( nameof( seed ) );
            }

            if ( length <= 0 )
            {
                throw new ArgumentOutOfRangeException( nameof( length ) );
            }

            var state = GetDeterministicHash( seed );
            var sb = new StringBuilder();

            while ( sb.Length < length )
            {
                state = GetNextState( state );
                sb.Append( state.ToString( "x8", CultureInfo.InvariantCulture ) );
            }

            return sb.ToString().Substring( 0, length );
        }

        private static string GetSha256Hex( string input )
        {
            using ( var sha = SHA256.Create() )
            {
                var bytes = sha.ComputeHash( Encoding.UTF8.GetBytes( input ) );
                return BitConverter.ToString( bytes ).Replace( "-", string.Empty );
            }
        }

        private static uint GetDeterministicHash( string input )
        {
            uint hash = 2166136261;

            foreach ( var c in input )
            {
                hash ^= c;
                hash += ( hash << 1 ) + ( hash << 4 ) + ( hash << 7 ) + ( hash << 8 ) + ( hash << 24 );
            }

            return hash;
        }

        private static uint GetNextState( uint state )
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state;
        }

        #endregion Private Methods

        #region Helper Types

        /// <summary>
        /// Represents a seed for a challenge, containing information about its
        /// unique identifier, completion count, size, and difficulty level.
        /// </summary>
        private sealed class ChallengeSeed
        {
            /// <summary>
            /// Gets or sets the unique identifier for the challenge seed.
            /// </summary>
            public Guid Guid { get; set; }

            /// <summary>
            /// Gets or sets the number of challenges completed by the user.
            /// </summary>
            public int ChallengeCount { get; set; }

            /// <summary>
            /// Gets or sets the size of the challenge.
            /// </summary>
            public int ChallengeSize { get; set; }

            /// <summary>
            /// Gets or sets the difficulty level of the challenge.
            /// </summary>
            public int ChallengeDifficulty { get; set; }
        }

        /// <summary>
        /// Represents a token used for captcha validation, containing a unique identifier and an expiration time.
        /// </summary>
        private class CaptchaToken
        {
            /// <summary>
            /// Gets or sets the unique identifier for the captcha token.
            /// </summary>
            public Guid Guid { get; set; } = Guid.NewGuid();

            /// <summary>
            /// Gets or sets the token issue date.
            /// </summary>
            public DateTime TokenIssueDate { get; set; }

            /// <summary>
            /// Gets or sets the token lifetime.
            /// </summary>
            public TimeSpan TokenLifetime { get; set; }
        }

        #endregion Helper Types
    }
}
