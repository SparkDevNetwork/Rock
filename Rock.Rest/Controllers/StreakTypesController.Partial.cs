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
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;
using Rock.Badge.Component;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// StreakTypes REST API
    /// </summary>
    public partial class StreakTypesController
    {
        /// <summary>
        /// Gets recent streak engagement data. Returns an array of bits representing "unitCount" units (days or weeks)
        /// with the last bit representing today. This is used for the <see cref="StreakEngagement" /> badge.
        /// </summary>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/StreakTypes/RecentEngagement/{streakTypeId}/{personId}" )]
        [Rock.SystemGuid.RestActionGuid( "76B588F0-362A-4385-96BB-D23961BEA2A8" )]
        public OccurrenceEngagement[] GetRecentEngagement( int streakTypeId, int personId, [FromUri] int unitCount = 24 )
        {
            var service = Service as StreakTypeService;
            var occurrenceEngagement = service.GetRecentEngagementBits( streakTypeId, personId, unitCount, out var errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, errorMessage );
                throw new HttpResponseException( errorResponse );
            }

            if ( occurrenceEngagement == null )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.InternalServerError, "The data calculation was not successful but no error was specified" );
                throw new HttpResponseException( errorResponse );
            }

            return occurrenceEngagement;
        }

        /// <summary>
        /// Gets recent streak engagement data. Returns an array of bits representing "unitCount" units (days or weeks)
        /// with the last bit representing today.
        /// </summary>
        /// <param name="streakTypeId">The streak type identifier.</param>
        /// <param name="personId">The person identifier. Defaults to the current person.</param>
        /// <param name="unitCount">The unit count.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException">
        /// </exception>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/StreakTypes/RecentEngagement/{streakTypeId}" )]
        [Rock.SystemGuid.RestActionGuid( "96D8CEE6-3F2D-4871-80EB-D54DF33D574C" )]
        public OccurrenceEngagement[] GetRecentEngagement( int streakTypeId, [FromUri] int? personId = null, [FromUri] int? unitCount = 24 )
        {
            // If not specified, use the current person id
            if ( !personId.HasValue )
            {
                personId = GetCurrentPersonId();
            }

            return GetRecentEngagement( streakTypeId, personId.Value, unitCount ?? 24 );
        }

        /// <summary>
        /// Enroll the currently logged-in user into the streak type.
        /// </summary>
        /// <param name="streakTypeId"></param>
        /// <param name="personId">Defaults to the current person</param>
        /// <param name="enrollmentDate">Defaults to the current date if omitted</param>
        /// <param name="locationId">Defaults to the person's campus if omitted</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/StreakTypes/Enroll/{streakTypeId}" )]
        [Rock.SystemGuid.RestActionGuid( "B8C5E76C-4127-4879-B296-6EB7C47BEE99" )]
        public virtual HttpResponseMessage Enroll( int streakTypeId, [FromUri] int? personId = null, [FromUri] DateTime? enrollmentDate = null, [FromUri] int? locationId = null )
        {
            // Make sure the streak type exists
            var streakTypeCache = StreakTypeCache.Get( streakTypeId );

            if ( streakTypeCache == null )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.NotFound, "The streakTypeId did not resolve" );
                throw new HttpResponseException( errorResponse );
            }

            // If not specified, use the current person id
            var rockContext = Service.Context as RockContext;

            if ( !personId.HasValue )
            {
                personId = GetCurrentPersonId();
            }

            // Create the enrollment
            var streakTypeService = Service as StreakTypeService;
            var streak = streakTypeService.Enroll( streakTypeCache, personId.Value, out var errorMessage, enrollmentDate, locationId );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, errorMessage );
                throw new HttpResponseException( errorResponse );
            }

            if ( streak == null )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.InternalServerError, "The enrollment was not successful but no error was specified" );
                throw new HttpResponseException( errorResponse );
            }

            // Save to the DB and tell the user the new id
            rockContext.SaveChanges();
            return ControllerContext.Request.CreateResponse( HttpStatusCode.Created, streak.Id );
        }

        /// <summary>
        /// Returns a listing of locations, including geofence data, for the streak. These locations are determined from the
        /// structure type of the streak type.
        /// </summary>
        /// <param name="streakTypeId"></param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [EnableQuery]
        [System.Web.Http.Route( "api/StreakTypes/Locations/{streakTypeId}" )]
        [Rock.SystemGuid.RestActionGuid( "3FAAF9AE-F244-4E3E-B47D-F21266DC52C9" )]
        public virtual IQueryable<Location> GetLocations( int streakTypeId )
        {
            // Make sure the streak type exists
            var streakTypeCache = StreakTypeCache.Get( streakTypeId );

            if ( streakTypeCache == null )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.NotFound, "The streakTypeId did not resolve" );
                throw new HttpResponseException( errorResponse );
            }

            // Get the locations from the service
            var streakTypeService = Service as StreakTypeService;
            var locations = streakTypeService.GetLocations( streakTypeCache, out var errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, errorMessage );
                throw new HttpResponseException( errorResponse );
            }

            if ( locations == null )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.InternalServerError, "The location retrieval was not successful but no error was specified" );
                throw new HttpResponseException( errorResponse );
            }

            return locations;
        }

        /// <summary>
        /// Returns a listing of schedules, including iCal data, for the streak type and specified location.
        /// </summary>
        /// <param name="streakTypeId"></param>
        /// <param name="locationId"></param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [EnableQuery]
        [System.Web.Http.Route( "api/StreakTypes/LocationSchedules/{streakTypeId}/{locationId}" )]
        [Rock.SystemGuid.RestActionGuid( "CE54BF07-97B2-4025-89B8-6E9CFD69F3B8" )]
        public virtual IQueryable<Schedule> GetLocationSchedules( int streakTypeId, int locationId )
        {
            // Make sure the streak type exists
            var streakTypeCache = StreakTypeCache.Get( streakTypeId );

            if ( streakTypeCache == null )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.NotFound, "The streakTypeId did not resolve" );
                throw new HttpResponseException( errorResponse );
            }

            // Get the schedules from the service
            var streakTypeService = Service as StreakTypeService;
            var schedules = streakTypeService.GetLocationSchedules( streakTypeCache, locationId, out var errorMessage );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, errorMessage );
                throw new HttpResponseException( errorResponse );
            }

            if ( schedules == null )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.InternalServerError, "The schedule retrieval was not successful but no error was specified" );
                throw new HttpResponseException( errorResponse );
            }

            return schedules;
        }

        /// <summary>
        /// Returns the currently logged-in user or the person indicated's streak information.
        /// The id list is used like the following: api/StreakTypes/StreakData/1,2,3?personId=4
        /// </summary>
        /// <param name="streakTypeIdList">The comma separated list of streak type identifiers</param>
        /// <param name="personId">Defaults to the current person</param>
        /// <param name="startDate">Defaults to the streak type start date</param>
        /// <param name="endDate">Defaults to the last elapsed frequency unit (yesterday or last week)</param>
        /// <param name="createObjectArray">Defaults to false. This may be a costly operation if enabled.</param>
        /// <param name="includeBitMaps">Defaults to false. This may be a costly operation if enabled.</param>
        /// <param name="maxStreaksToReturn">Specify the maximum number of streak objects "ComputedStreaks" to include in the response</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/StreakTypes/StreakData/{streakTypeIdList}" )]
        [Rock.SystemGuid.RestActionGuid( "EEC82FE7-4F4A-4C90-9BA6-3C5F98BF61DA" )]
        public virtual List<StreakData> GetStreakData( string streakTypeIdList,
            [FromUri] int? personId = null, [FromUri] DateTime? startDate = null, [FromUri] DateTime? endDate = null,
            [FromUri] bool createObjectArray = false, [FromUri] bool includeBitMaps = false, [FromUri] int? maxStreaksToReturn = null )
        {
            if ( streakTypeIdList.IsNullOrWhiteSpace() )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, "The streak type identifier list is required" );
                throw new HttpResponseException( errorResponse );
            }

            var streakTypeIds = streakTypeIdList.SplitDelimitedValues().AsIntegerList().Distinct().ToList();

            if ( !streakTypeIds.Any() )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, "At least one streak type identifier is required" );
                throw new HttpResponseException( errorResponse );
            }

            // If not specified, use the current person id
            if ( !personId.HasValue )
            {
                personId = GetCurrentPersonId();
            }

            // Return a list of the results (one for each id)
            var streakTypeService = Service as StreakTypeService;
            var streakDataList = new List<StreakData>( streakTypeIds.Count );

            foreach ( var streakTypeId in streakTypeIds )
            {
                // Make sure the streak type exists
                var streakTypeCache = StreakTypeCache.Get( streakTypeId );

                if ( streakTypeCache == null )
                {
                    var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.NotFound, $"The streak type id '{streakTypeId}' did not resolve" );
                    throw new HttpResponseException( errorResponse );
                }

                // Get the data from the service
                var streakData = streakTypeService.GetStreakData( streakTypeCache, personId.Value, out var errorMessage,
                    startDate, endDate, createObjectArray, includeBitMaps, maxStreaksToReturn );

                if ( !errorMessage.IsNullOrWhiteSpace() )
                {
                    var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, errorMessage );
                    throw new HttpResponseException( errorResponse );
                }

                if ( streakData == null )
                {
                    var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.InternalServerError, "The streak data calculation was not successful but no error was specified" );
                    throw new HttpResponseException( errorResponse );
                }

                streakDataList.Add( streakData );
            }

            return streakDataList;
        }

        /// <summary>
        /// Notes that the person has engaged through interaction. This will update the occurrence map and also add an
        /// interaction record (if enabled).
        /// </summary>
        /// <param name="streakTypeId">The streak type identifier.</param>
        /// <param name="interactionEngagementArgs">Data used to create an interaction record if enabled in the streak type</param>
        /// <param name="personId">Defaults to the current person</param>
        /// <param name="dateOfEngagement">Defaults to now</param>
        /// <param name="returnAchievements">if set to <c>true</c> [return achievements].</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException">
        /// </exception>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/StreakTypes/MarkInteractionEngagement/{streakTypeId}" )]
        [Rock.SystemGuid.RestActionGuid( "0EE72F4B-17BC-4657-B7E7-E924A6BFAA8F" )]
        public virtual HttpResponseMessage MarkInteractionEngagement( int streakTypeId, [FromBody] InteractionEngagementArgs interactionEngagementArgs,
            [FromUri] int? personId = null, [FromUri] DateTime? dateOfEngagement = null, [FromUri] bool returnAchievements = false )
        {
            // Make sure the streak type exists
            var streakTypeCache = StreakTypeCache.Get( streakTypeId );

            if ( streakTypeCache == null )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.NotFound, "The streak type id did not resolve" );
                throw new HttpResponseException( errorResponse );
            }

            // If not specified, use the current person id
            if ( !personId.HasValue )
            {
                personId = GetCurrentPersonId();
            }

            // Mark the engagement
            var streakTypeService = Service as StreakTypeService;
            streakTypeService.MarkInteractionEngagement( streakTypeCache, personId.Value, interactionEngagementArgs, out var errorMessage, dateOfEngagement );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, errorMessage );
                throw new HttpResponseException( errorResponse );
            }

            // Save to the DB
            var rockContext = Service.Context as RockContext;
            var result = rockContext.SaveChanges( new SaveChangesArgs { IsAchievementsEnabled = returnAchievements } );

            if ( returnAchievements )
            {
                return ControllerContext.Request.CreateResponse( HttpStatusCode.Created, new MarkEngagementResponse( result.AchievementAttempts ) );
            }
            else
            {
                return ControllerContext.Request.CreateResponse( HttpStatusCode.Created );
            }
        }

        /// <summary>
        /// Notes that the person has engaged through attendance. This will update the occurrence map and also add an
        /// attendance record (if enabled).
        /// </summary>
        /// <param name="streakTypeId">The streak type identifier.</param>
        /// <param name="attendanceEngagementArgs">Optional data used to create an attendance record if enabled in the streak type</param>
        /// <param name="personId">Defaults to the current person</param>
        /// <param name="dateOfEngagement">Defaults to now</param>
        /// <param name="returnAchievements">if set to <c>true</c> [return achievements].</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException">
        /// </exception>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/StreakTypes/MarkAttendanceEngagement/{streakTypeId}" )]
        [Rock.SystemGuid.RestActionGuid( "21121485-E80C-4B7E-810F-F34F986FECDB" )]
        public virtual HttpResponseMessage MarkAttendanceEngagement( int streakTypeId, [FromBody] AttendanceEngagementArgs attendanceEngagementArgs,
            [FromUri] int? personId = null, [FromUri] DateTime? dateOfEngagement = null, [FromUri] bool returnAchievements = false )
        {
            // Make sure the streak type exists
            var streakTypeCache = StreakTypeCache.Get( streakTypeId );

            if ( streakTypeCache == null )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.NotFound, "The streak type id did not resolve" );
                throw new HttpResponseException( errorResponse );
            }

            // If not specified, use the current person id
            if ( !personId.HasValue )
            {
                personId = GetCurrentPersonId();
            }

            // Mark the engagement
            var streakTypeService = Service as StreakTypeService;
            streakTypeService.MarkAttendanceEngagement( streakTypeCache, personId.Value, attendanceEngagementArgs, out var errorMessage, dateOfEngagement );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, errorMessage );
                throw new HttpResponseException( errorResponse );
            }

            // Save to the DB
            var rockContext = Service.Context as RockContext;
            var result = rockContext.SaveChanges( new SaveChangesArgs { IsAchievementsEnabled = returnAchievements } );

            if ( returnAchievements )
            {
                return ControllerContext.Request.CreateResponse( HttpStatusCode.Created, new MarkEngagementResponse( result.AchievementAttempts ) );
            }
            else
            {
                return ControllerContext.Request.CreateResponse( HttpStatusCode.Created );
            }
        }

        /// <summary>
        /// Gets the current person identifier.
        /// </summary>
        /// <returns></returns>
        private int GetCurrentPersonId()
        {
            var rockContext = Service.Context as RockContext;
            var personId = GetPerson( rockContext )?.Id;

            if ( !personId.HasValue )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, "The person id for the current user did not resolve" );
                throw new HttpResponseException( errorResponse );
            }

            return personId.Value;
        }

        /// <summary>
        /// Mark Engagement Response
        /// </summary>
        public sealed class MarkEngagementResponse
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="MarkEngagementResponse"/> class.
            /// </summary>
            /// <param name="achievementAttempts">The achievement attempts.</param>
            public MarkEngagementResponse( List<AchievementAttempt> achievementAttempts )
            {
                if ( achievementAttempts == null )
                {
                    AchievementAttempts = new List<AttemptListItem>();
                }
                else
                {
                    AchievementAttempts = achievementAttempts.Select( aa => new AttemptListItem( aa ) ).ToList();
                }
            }

            /// <summary>
            /// Gets the achievement attempts.
            /// </summary>
            public List<AttemptListItem> AchievementAttempts { get; private set; }

            /// <summary>
            /// Attempt List Item
            /// </summary>
            public sealed class AttemptListItem
            {
                /// <summary>
                /// Initializes a new instance of the <see cref="MarkEngagementResponse"/> class.
                /// </summary>
                public AttemptListItem( AchievementAttempt achievementAttempt )
                {
                    AchievementAttemptId = achievementAttempt.Id;
                    AchievementTypeId = achievementAttempt.AchievementTypeId;
                    AchievementTypeGuid = achievementAttempt.AchievementType?.Guid ?? AchievementTypeCache.Get( AchievementTypeId )?.Guid;
                    Progress = achievementAttempt.Progress;
                    IsComplete = Progress >= 1;
                }

                /// <summary>
                /// Gets or sets the achievement attempt identifier.
                /// </summary>
                public int AchievementAttemptId { get; private set; }

                /// <summary>
                /// Gets or sets the achievement type identifier.
                /// </summary>
                public int AchievementTypeId { get; private set; }

                /// <summary>
                /// Gets or sets the achievement type unique identifier.
                /// </summary>
                public Guid? AchievementTypeGuid { get; private set; }

                /// <summary>
                /// Gets or sets the progress.
                /// </summary>
                public decimal Progress { get; private set; }

                /// <summary>
                /// Gets or sets a value indicating whether this instance is complete.
                /// </summary>
                public bool IsComplete { get; private set; }
            }
        }
    }
}