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
using System.Data.Entity;
using System.Linq;

namespace Rock.Model
{
    internal static class RemoteAuthenticationSessionExtensions
    {
        internal static IQueryable<RemoteAuthenticationSession> WasCreatedToday( this IQueryable<RemoteAuthenticationSession> queryable )
        {
            return queryable.Where( s => s.CreatedDateTime.Value >= RockDateTime.Today );
        }

        internal static IQueryable<RemoteAuthenticationSession> WhereUsingCode( this IQueryable<RemoteAuthenticationSession> queryable, string code, DateTime codeIssueDate, TimeSpan codeLifetime )
        {
            var codeLifetimeInMinutes = ( int ) codeLifetime.TotalMinutes;
            if ( codeLifetimeInMinutes < 0 )
            {
                // Set to max if there was an overflow when casting the double to an int.
                codeLifetimeInMinutes = int.MaxValue;
            }

            var codeExpirationDate = codeIssueDate.AddMinutes( codeLifetimeInMinutes );
            /*
                12/16/2022 - JMH
             
                Given Rock has an existing code, ABCD, that was issued at 3 and expires at 6,
                that code on a unitless timeline would look like this:

                      [     )       (code in Rock: ABCD)
                -------------------
                0 1 2 3 4 5 6 7 8 9

                Note: issue date 3 is inclusive as indicated by square bracket "[" and expiration date 6 is exclusive as indicated by closing parens ")".


                We can determine that a code is available if:

                    - the code being checked does not have a matching code in Rock

                    or

                    - the lifetime of the code being checked is outside the lifetime of other matching codes


                The first condition is self-explanatory; e.g., if code "WXYZ" is not in the table, then the code is available.

                The second condition is less trivial, so let's look at the 4 possibilities that satisfy it.


                Possibility 1: Code being checked expires before an existing code is issued
                          
                [   )               (code being checked: ABCD)
                      [     )       (code in Rock: ABCD)
                -------------------
                0 1 2 3 4 5 6 7 8 9


                Possibility 2: Code being checked expires when an existing code is issued
            
                [     )             (code being checked: ABCD)
                      [     )       (code in Rock: ABCD)
                -------------------
                0 1 2 3 4 5 6 7 8 9


                Possibility 3: Code being checked is issued after an existing code expires
                          
                              [   ) (code being checked: ABCD)
                      [     )       (code in Rock: ABCD)
                -------------------
                0 1 2 3 4 5 6 7 8 9


                Possibility 4: Code being checked is issued when an existing code expires
                                      
                            [     ) (code being checked: ABCD)
                      [     )       (code in Rock: ABCD)
                -------------------
                0 1 2 3 4 5 6 7 8 9


                Breaking down our earlier conditions, we can determine that a code is AVAILABLE if:

                    - the code being checked does not have a matching code in Rock

                    or

                    - the expiration date of the code being checked is the same as or earlier than a matching code's issue date (Possibility 1 and Possibility 2)

                    or

                    - the issue date of the code being checked is the same as or later than a matching code's end date (Possibility 3 and Possibility 4)


                Finally, we can invert this logic to determine that a code is UNAVAILABLE if:

                    - the code being checked has a match

                    and

                    - the expiration date of the code being checked is later than a matching code's issue date

                    and

                    - the issue date of the code being checked is earlier than a matching code's end date


                The conditions to find an unavailable (in use) code is easier to query, and we can negate the result to find out if a code is usable.

                Reason: Passwordless Sign In
             */

            return queryable
                .Where( s => code == s.Code )
                .Where( s => codeExpirationDate > s.SessionStartDateTime.Value )
                //// Use the session's end date if it exists, otherwise use a calculated end date.
                .Where( s =>
                    ( s.SessionEndDateTime.HasValue && codeIssueDate < s.SessionEndDateTime.Value )
#if REVIEW_NET5_0_OR_GREATER
                    || ( !s.SessionEndDateTime.HasValue && codeIssueDate < s.SessionStartDateTime.Value.AddMinutes( codeLifetimeInMinutes ) ) );
#else
                    || ( !s.SessionEndDateTime.HasValue && codeIssueDate < DbFunctions.AddMinutes( s.SessionStartDateTime.Value, codeLifetimeInMinutes ).Value ) );
#endif
        }

        internal static IQueryable<RemoteAuthenticationSession> WhereIsActive( this IQueryable<RemoteAuthenticationSession> queryable, TimeSpan codeLifetime, DateTime? dateTimeToCheckIfCodeActive = null )
        {
            var codeLifetimeInMinutes = ( int ) codeLifetime.TotalMinutes;
            if ( codeLifetimeInMinutes < 0 )
            {
                // Set to max if there was an overflow when casting the double to an int.
                codeLifetimeInMinutes = int.MaxValue;
            }

            if ( !dateTimeToCheckIfCodeActive.HasValue )
            {
                dateTimeToCheckIfCodeActive = RockDateTime.Now;
            }

            return queryable
                .Where( s => !s.SessionEndDateTime.HasValue )
                .Where( s => s.SessionStartDateTime.HasValue )
                .Where( s => dateTimeToCheckIfCodeActive.Value >= s.SessionStartDateTime.Value )
#if REVIEW_NET5_0_OR_GREATER
                .Where( s => dateTimeToCheckIfCodeActive.Value < s.SessionStartDateTime.Value.AddMinutes( codeLifetimeInMinutes ) );
#else
                .Where( s => dateTimeToCheckIfCodeActive.Value < DbFunctions.AddMinutes( s.SessionStartDateTime.Value, codeLifetimeInMinutes ).Value );
#endif
        }
    }
}
