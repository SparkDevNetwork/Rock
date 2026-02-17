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
using System.Linq;

using Rock.Data;
using Rock.Model.Event.RegistrationInstance.Options;

namespace Rock.Model
{
    public partial class RegistrationSessionService
    {
        /// <summary>
        /// Tries to renew the registration session. This method locks tables
        /// during processing. After returning, you must check the <see cref="RegistrationSession.RegistrationCount"/>
        /// property to ensure that enough spots still exist. This can happen if
        /// the session was expired but was able to be partially renewed.
        /// </summary>
        /// <param name="sessionGuid">The session unique identifier.</param>
        /// <returns>The <see cref="RegistrationSession"/> that was renewed or <c>null</c> if it could not be found.</returns>
        public static RegistrationSession TryToRenewSession( Guid sessionGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var registrationSessionService = new RegistrationSessionService( rockContext );
                var registrationService = new RegistrationService( rockContext );
                var registrationInstanceService = new RegistrationInstanceService( rockContext );
                RegistrationSession registrationSession = null;

                var wasRenewed = rockContext.WrapTransactionIf( () =>
                {
                    /*
                     * This is an anti-pattern. Do not just copy this and use it
                     * as a pattern elsewhere. Discuss with the team before trying
                     * to use this anywhere else.
                     * 
                     * This single use-case was discussed between Jon and Daniel on
                     * 9/17/2021 and deemed the best choice for the moment. In the
                     * future we might convert this to a helper method that doesn't
                     * require custom SQL each place it is used - but we really
                     * shouldn't do full table locks as a matter of practice.
                     * 
                     * Daniel Hazelbaker 9/17/2021
                     */

                    // Initiate a full table lock so nothing else can query data,
                    // otherwise they might get a count that will no longer be
                    // valid after our transaction is committed.
                    rockContext.Database.ExecuteSqlCommand( "SELECT TOP 1 Id FROM [RegistrationSession] WITH (TABLOCKX, HOLDLOCK)" );

                    // Try to find the session to renew, if we can't find it then
                    // return a failure indication.
                    registrationSession = registrationSessionService.Get( sessionGuid );

                    if ( registrationSession is null )
                    {
                        return false;
                    }

                    // Attempt to get the context that describes the registration
                    // and the number of available slots.
                    var context = registrationService.GetRegistrationContext( registrationSession.RegistrationInstanceId, registrationSession.RegistrationId, out var errorMessage );

                    if ( errorMessage.IsNotNullOrWhiteSpace() )
                    {
                        return false;
                    }

                    // Check if the session has expired already before we set the new
                    // expiration window.
                    var wasExpired = registrationSession.ExpirationDateTime < RockDateTime.Now;

                    // Set the new expiration
                    registrationSession.ExpirationDateTime = context.RegistrationSettings.TimeoutMinutes.HasValue
                        ? RockDateTime.Now.AddMinutes( context.RegistrationSettings.TimeoutMinutes.Value )
                        : RockDateTime.Now.Add( RegistrationInstance.DefaultTimeoutLength );

                    // If the session was expired then the number of reserved spots
                    // might no longer be valid. Check if there are fewer spots
                    // actually available and update the count.
                    if ( wasExpired )
                    {
                        var spotsRemaining = registrationInstanceService.GetSpotsAvailable( new GetSpotsAvailableOptions
                        {
                            ExcludeReservedSpotsForRegistrationSessionGuid = registrationSession.Guid,
                            IsTimeoutEnabled = context.RegistrationSettings.IsTimeoutEnabled,
                            IsWaitListExcluded = true, // Exclude waitlisted for session renewal.
                            MaxAttendees = context.RegistrationSettings.MaxAttendees,
                            RegistrationInstanceId = context.RegistrationSettings.RegistrationInstanceId
                        } ) ?? 0; // Default to 0 spots remaining if null.
                        
                        if ( spotsRemaining < registrationSession.RegistrationCount )
                        {
                            registrationSession.RegistrationCount = spotsRemaining;
                        }
                    }

                    // Persist the new session information to the database.
                    rockContext.SaveChanges();

                    return true;
                } );

                return wasRenewed ? registrationSession : null;
            }
        }

        /// <summary>
        /// Creates or update a registration session. This method operates
        /// inside a database lock to prevent other sessions from being
        /// modified at the same time.
        /// </summary>
        /// <param name="sessionGuid">The session unique identifier.</param>
        /// <param name="createSession">The method to call to get a new <see cref="RegistrationSession"/> object that will be persisted to the database.</param>
        /// <param name="updateSession">The method to call to update an existing <see cref="RegistrationSession"/> object with any new information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>The <see cref="RegistrationSession"/> that was created or updated; or <c>null</c> if an error occurred.</returns>
        public static RegistrationSession CreateOrUpdateSession( Guid sessionGuid, Func<RegistrationSession> createSession, Action<RegistrationSession> updateSession, out string errorMessage )
        {
            using ( var rockContext = new RockContext() )
            {
                var registrationSessionService = new RegistrationSessionService( rockContext );
                var registrationInstanceService = new RegistrationInstanceService( rockContext );
                RegistrationSession registrationSession = null;
                string internalErrorMessage = null;

                rockContext.WrapTransactionIf( () =>
                {
                    /*
                     * This is an anti-pattern. Do not just copy this and use it
                     * as a pattern elsewhere. Discuss with the team before trying
                     * to use this anywhere else.
                     * 
                     * This single use-case was discussed between Jon and Daniel on
                     * 9/17/2021 and deemed the best choice for the moment. In the
                     * future we might convert this to a helper method that doesn't
                     * require custom SQL each place it is used - but we really
                     * shouldn't do full table locks as a matter of practice.
                     * 
                     * Daniel Hazelbaker 9/17/2021
                     */

                    // Initiate a full table lock so nothing else can query data,
                    // otherwise they might get a count that will no longer be
                    // valid after our transaction is committed.
                    rockContext.Database.ExecuteSqlCommand( "SELECT TOP 1 Id FROM [RegistrationSession] WITH (TABLOCKX, HOLDLOCK)" );

                    var registrationService = new RegistrationService( rockContext );

                    // Load the registration session and determine if it was expired already.
                    registrationSession = registrationSessionService.Get( sessionGuid );
                    var wasExpired = registrationSession != null && registrationSession.ExpirationDateTime < RockDateTime.Now;
                    bool isNewRegistrationSession;

                    // If the session didn't exist then create a new one, otherwise
                    // update the existing one.
                    //
                    // The session was created for one of two reasons:
                    // 1. The registration instance has limited capacity and has sessions enabled to manage reserving spots.
                    // 2. A redirect payment gateway is being used and needs to save a session (no slot management needed).
                    // 
                    // That being said, do not throw an error here if sessions are not enabled on the instance;
                    // just create the session and move on since it might be required for payments.

                    if ( registrationSession == null )
                    {
                        registrationSession = createSession();
                        isNewRegistrationSession = true;
                        registrationSessionService.Add( registrationSession );
                    }
                    else
                    {
                        isNewRegistrationSession = false;
                        updateSession( registrationSession );
                    }
                    
                    if ( registrationSession.RegistrationCount == 0 )
                    {
                        // No registrants (all waitlisted?) so no need to create a new session.
                        internalErrorMessage = "No registrant spots are being reserved.";

                        if ( !isNewRegistrationSession )
                        {
                            // Drop the old session since there are no spots being reserved.
                            registrationSessionService.Delete( registrationSession );
                            rockContext.SaveChanges();
                            return true;
                        }

                        return false;
                    }

                    // Get the context information about the registration, specifically
                    // the timeout and spots available.
                    var timeoutSettings = registrationInstanceService.Queryable()
                        .Where( ri => ri.Id == registrationSession.RegistrationInstanceId )
                        .Select( ri => new
                        {
                            ri.TimeoutLengthMinutes,
                            ri.TimeoutIsEnabled,
                            ri.MaxAttendees
                        } )
                        .FirstOrDefault();

                    // Set the new expiration date.
                    registrationSession.ExpirationDateTime = timeoutSettings?.TimeoutLengthMinutes.HasValue == true
                        ? RockDateTime.Now.AddMinutes( timeoutSettings.TimeoutLengthMinutes.Value )
                        : RockDateTime.Now.Add( RegistrationInstance.DefaultTimeoutLength );

                    // Handle the possibility that there is a change in the number of
                    // registrants in the session.
                    var spotsRemaining = registrationInstanceService.GetSpotsAvailable( new GetSpotsAvailableOptions
                    {
                        RegistrationInstanceId = registrationSession.RegistrationInstanceId,
                        ExcludeReservedSpotsForRegistrationSessionGuid = registrationSession.Guid,
                        IsTimeoutEnabled = timeoutSettings?.TimeoutIsEnabled ?? false,
                        IsWaitListExcluded = true,
                        MaxAttendees = timeoutSettings.MaxAttendees
                    } );

                    if ( spotsRemaining.HasValue && ( spotsRemaining.Value < registrationSession.RegistrationCount ) )
                    {
                        internalErrorMessage = "There is not enough capacity remaining for this many registrants.";

                        if ( !isNewRegistrationSession )
                        {
                            registrationSessionService.Delete( registrationSession );
                            rockContext.SaveChanges();
                            return true;
                        }

                        return false;
                    }

                    rockContext.SaveChanges();
                    internalErrorMessage = string.Empty;

                    return true;
                } );

                errorMessage = internalErrorMessage;

                return errorMessage.IsNullOrWhiteSpace() ? registrationSession : null;
            }
        }

        /// <summary>
        /// Deletes the session(s) for the given GUID. Call this method when the Registion process has been completed.
        /// </summary>
        /// <param name="sessionGuid">The session unique identifier.</param>
        public static void CloseAndRemoveSession( Guid sessionGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                try
                {
                    var registrationSessionService = new RegistrationSessionService( rockContext );
                    var sessionToDeleteQuery = registrationSessionService.Queryable().Where( s => s.Guid == sessionGuid );

                    registrationSessionService.DeleteRange( sessionToDeleteQuery );
                    rockContext.SaveChanges();
                }
                catch ( Exception e )
                {
                    ExceptionLogService.LogException( e );
                }
            }
        }
    }
}
