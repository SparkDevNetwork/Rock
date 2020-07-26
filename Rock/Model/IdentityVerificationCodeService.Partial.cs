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
using System.Linq;
using Rock.Data;
using Rock.Logging;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.IdentityVerificationCode"/> entities.
    /// </summary>
    public partial class IdentityVerificationCodeService
    {
        private static readonly Object _obj = new object();
        private static readonly Random _random = new Random( Guid.NewGuid().GetHashCode() );

        /// <summary>
        /// Gets the random identity verification code.
        /// </summary>
        /// <returns></returns>
        public static IdentityVerificationCode GetRandomIdentityVerificationCode()
        {
            lock ( _obj )
            {
                using ( var rockContext = new RockContext() )
                {
                    IdentityVerificationCode verificationCode = null;
                    rockContext.WrapTransaction( () =>
                    {
                        var identityVerificationService = new IdentityVerificationCodeService( rockContext );
                        var availableCodeCount = identityVerificationService
                                                    .Queryable()
                                                    .Where( pvc => pvc.LastIssueDateTime < RockDateTime.Today || pvc.LastIssueDateTime == null )
                                                    .Count();

                        var hasUsableRecords = availableCodeCount > 0;

                        if ( !hasUsableRecords )
                        {
                            RockLogger.Log.Warning( RockLogDomains.Core, "All of the available phone verification codes have been used." );
                            ExceptionLogService.LogException( "All of the available phone verification codes have been used." );

                            availableCodeCount = identityVerificationService
                                                    .Queryable()
                                                    .Count();
                        }

                        var itemIndex = _random.Next( availableCodeCount );

                        verificationCode = identityVerificationService
                                .Queryable()
                                .OrderBy( pvc => pvc.LastIssueDateTime )
                                .Where( pvc => !hasUsableRecords || pvc.LastIssueDateTime < RockDateTime.Today || pvc.LastIssueDateTime == null )
                                .Skip( itemIndex )
                                .FirstOrDefault();

                        if ( verificationCode != null )
                        {
                            verificationCode.LastIssueDateTime = RockDateTime.Now;
                            rockContext.SaveChanges();
                        }
                    } );

                    return verificationCode;
                }
            }
        }
    }
}
