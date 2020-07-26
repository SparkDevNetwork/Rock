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
using System.Data.Entity;
using System.Linq;
using Rock.Data;

namespace Rock.Model
{
    public partial class IdentityVerificationService
    {

        /// <summary>
        /// Verifies the identity verification code.
        /// </summary>
        /// <param name="referenceNumber">The reference number.</param>
        /// <param name="timeLimit">The time limit.</param>
        /// <param name="verificationCode">The verification code.</param>
        /// <returns></returns>
        public bool VerifyIdentityVerificationCode( string referenceNumber, int timeLimit, string verificationCode )
        {
            var minDateTime = RockDateTime.Now.AddMinutes( -timeLimit );
            var identityVerification = Queryable()
                                        .Include( c => c.IdentityVerificationCode )
                                        .Where( ivs => ivs.ReferenceNumber == referenceNumber )
                                        .Where( ivs => ivs.IssueDateTime > minDateTime )
                                        .OrderByDescending( ivs => ivs.IssueDateTime )
                                        .FirstOrDefault();

            return identityVerification != null
                    && identityVerification.IdentityVerificationCode.Code == verificationCode;
        }

        /// <summary>
        /// Creates the identity verification record.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="ipLimit">The ip limit.</param>
        /// <param name="phoneNumber">The phone number.</param>
        /// <returns></returns>
        public IdentityVerification CreateIdentityVerificationRecord( string ipAddress, int ipLimit, string phoneNumber )
        {
            ValidateIpCountWithinLimits( ipAddress, ipLimit );

            var verificationCode = IdentityVerificationCodeService.GetRandomIdentityVerificationCode();
            if ( verificationCode != null )
            {
                var identityVerification = new IdentityVerification
                {
                    ReferenceNumber = phoneNumber,
                    IssueDateTime = RockDateTime.Now,
                    IdentityVerificationCode = verificationCode,
                    RequestIpAddress = ipAddress
                };
                Add( identityVerification );
                ( ( RockContext ) Context ).IdentityVerificationCodes.Attach( verificationCode );
                Context.SaveChanges();
                return identityVerification;
            }

            return null;
        }

        private void ValidateIpCountWithinLimits( string ipAddress, int ipLimit )
        {
            int currentCount = Queryable()
                .Where( pv => pv.RequestIpAddress == ipAddress )
                .Where( pv => pv.IssueDateTime >= RockDateTime.Today )
                .Count();

            if ( currentCount >= ipLimit )
            {
                throw new IdentityVerificationIpLimitReachedException();
            }
        }
    }

    /// <summary>
    /// This exception is thrown when the Phone Verification Ip throttle limit has been reached.
    /// </summary>
    /// <seealso cref="System.ArgumentOutOfRangeException" />
    public class IdentityVerificationIpLimitReachedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IdentityVerificationIpLimitReachedException"/> class.
        /// </summary>
        public IdentityVerificationIpLimitReachedException() :
            base( "Your IP address is over the maximum number of requests per day. Please request assistance from the organization administrator." )
        { }
    }
}
