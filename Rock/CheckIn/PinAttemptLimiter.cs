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

using Rock.Attribute;
using Rock.Web.Cache;

namespace Rock.CheckIn
{
    /*
        10/15/2026 - MSE

        A check-in PIN is looked up as a user name and is the whole credential,
        so a wrong guess never matches a login and the normal per-login lockout
        can never apply. This class counts wrong guesses per client IP address
        instead. An attempt is counted before the PIN is looked up, in the same
        step as the limit check, so a burst of parallel guesses cannot go over
        the limit. The caller then gives the attempt back if it was not a wrong
        guess, or resets the count when the PIN was correct. Counts are kept in
        memory on each web node.

        Reason: Prevent check-in PINs from being guessed by brute force.
    */
    /// <summary>
    /// Limits the number of wrong check-in PIN guesses that can be made from
    /// a single client IP address. This is shared by the check-in supervisor
    /// screens, the check-in override and the legacy kiosk manager login so
    /// that switching between them does not increase the budget.
    /// </summary>
    [RockInternal( "17.12", true )]
    public static class PinAttemptLimiter
    {
        /// <summary>
        /// The rate limiter key shared by every check-in PIN login.
        /// </summary>
        private const string Key = "checkin-pin-failures";

        /// <summary>
        /// The window that failed attempts are counted in. The window starts
        /// with the first attempt reserved from the client IP address, even
        /// one that later succeeds, and a reset does not restart it.
        /// </summary>
        public static readonly TimeSpan Window = TimeSpan.FromMinutes( 10 );

        /// <summary>
        /// The maximum number of failed attempts allowed from a single client
        /// IP address within the <see cref="Window"/>.
        /// </summary>
        public const int MaxFailuresPerIpAddress = 10;

        /// <summary>
        /// The longest client IP address that is used as a key. Longer values
        /// are cut to this length.
        /// </summary>
        internal const int MaxIpAddressKeyLength = 64;

        /// <summary>
        /// Checks whether another PIN attempt is allowed from the client IP
        /// address and, if it is, counts it as a failed attempt in the same
        /// step. Call this before looking up the PIN. If the attempt turns out
        /// not to be a wrong guess, call <see cref="ReleaseAttempt(string)"/>
        /// or <see cref="Reset(string)"/>.
        /// </summary>
        /// <param name="ipAddress">The client IP address.</param>
        /// <returns><c>true</c> if the attempt is allowed; <c>false</c> if too many failed attempts have been made.</returns>
        public static bool TryReserveAttempt( string ipAddress )
        {
            return RateLimiterCache.TryRecordAction( Key, GetIpAddressKey( ipAddress ), Window, MaxFailuresPerIpAddress );
        }

        /// <summary>
        /// Gives back an attempt counted by <see cref="TryReserveAttempt(string)"/>
        /// that should not count as a failed attempt, such as a PIN that
        /// matched a login that is not confirmed or is locked out.
        /// </summary>
        /// <param name="ipAddress">The client IP address.</param>
        public static void ReleaseAttempt( string ipAddress )
        {
            RateLimiterCache.ReleaseAction( Key, GetIpAddressKey( ipAddress ) );
        }

        /// <summary>
        /// Clears the failed attempts for the client IP address. Call this
        /// after a successful PIN login.
        /// </summary>
        /// <param name="ipAddress">The client IP address.</param>
        public static void Reset( string ipAddress )
        {
            RateLimiterCache.ResetAction( Key, GetIpAddressKey( ipAddress ) );
        }

        /// <summary>
        /// Gets the key used for the client IP address. The value is trimmed,
        /// made lower case and cut to <see cref="MaxIpAddressKeyLength"/>
        /// characters so the same address always uses one count and a long
        /// forwarded header value cannot make a large cache key. A null or
        /// empty value uses the empty key.
        /// </summary>
        /// <param name="ipAddress">The client IP address.</param>
        /// <returns>The key to count attempts under.</returns>
        internal static string GetIpAddressKey( string ipAddress )
        {
            var key = ipAddress?.Trim().ToLowerInvariant() ?? string.Empty;

            return key.Length > MaxIpAddressKeyLength
                ? key.Substring( 0, MaxIpAddressKeyLength )
                : key;
        }
    }
}
