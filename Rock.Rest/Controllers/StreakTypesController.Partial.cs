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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.OData;
using Rock.Data;
using Rock.Model;
using Rock.PersonProfile.Badge;
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
        public virtual HttpResponseMessage Enroll( int streakTypeId, [FromUri]int? personId = null, [FromUri] DateTime? enrollmentDate = null, [FromUri] int? locationId = null )
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
                personId = GetPerson( rockContext )?.Id;

                if ( !personId.HasValue )
                {
                    var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, "The personId for the current user did not resolve" );
                    throw new HttpResponseException( errorResponse );
                }
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
        /// <param name="endDate">Defaults to now</param>
        /// <param name="createObjectArray">Defaults to false. This may be a costly operation if enabled.</param>
        /// <param name="includeBitMaps">Defaults to false. This may be a costly operation if enabled.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpGet]
        [System.Web.Http.Route( "api/StreakTypes/StreakData/{streakTypeIdList}" )]
        public virtual List<StreakData> GetStreakData( string streakTypeIdList,
            [FromUri]int? personId = null, [FromUri]DateTime? startDate = null, [FromUri]DateTime? endDate = null,
            [FromUri]bool createObjectArray = false, [FromUri]bool includeBitMaps = false )
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
                var rockContext = Service.Context as RockContext;
                personId = GetPerson( rockContext )?.Id;

                if ( !personId.HasValue )
                {
                    var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, "The personId for the current user did not resolve" );
                    throw new HttpResponseException( errorResponse );
                }
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
                var streakData = streakTypeService.GetStreakData( streakTypeCache, personId.Value, out var errorMessage, startDate, endDate, createObjectArray, includeBitMaps );

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
        /// Notes that the person is present. This will update the occurrence map and also attendance (if enabled).
        /// </summary>
        /// <param name="streakTypeId"></param>
        /// <param name="personId">Defaults to the current person</param>
        /// <param name="dateOfEngagement">Defaults to now</param>
        /// <param name="groupId">This is required for marking attendance unless the streak type is a group structure type</param>
        /// <param name="locationId"></param>
        /// <param name="scheduleId"></param>
        /// <returns></returns>
        [Authenticate, Secured]
        [HttpPost]
        [System.Web.Http.Route( "api/StreakTypes/MarkEngagement/{streakTypeId}" )]
        public virtual HttpResponseMessage MarkEngagement( int streakTypeId, [FromUri]int? personId = null,
            [FromUri]DateTime? dateOfEngagement = null, [FromUri]int? groupId = null, [FromUri]int? locationId = null, [FromUri]int? scheduleId = null )
        {
            // Make sure the streak type exists
            var streakTypeCache = StreakTypeCache.Get( streakTypeId );

            if ( streakTypeCache == null )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.NotFound, "The streak type id did not resolve" );
                throw new HttpResponseException( errorResponse );
            }

            // If not specified, use the current person id
            var rockContext = Service.Context as RockContext;

            if ( !personId.HasValue )
            {
                personId = GetPerson( rockContext )?.Id;

                if ( !personId.HasValue )
                {
                    var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, "The personId for the current user did not resolve" );
                    throw new HttpResponseException( errorResponse );
                }
            }

            // Get the data from the service
            var streakTypeService = Service as StreakTypeService;
            streakTypeService.MarkEngagement( streakTypeCache, personId.Value, out var errorMessage,
                dateOfEngagement, groupId, locationId, scheduleId );

            if ( !errorMessage.IsNullOrWhiteSpace() )
            {
                var errorResponse = ControllerContext.Request.CreateErrorResponse( HttpStatusCode.BadRequest, errorMessage );
                throw new HttpResponseException( errorResponse );
            }

            // Save to the DB
            rockContext.SaveChanges();
            return ControllerContext.Request.CreateResponse( HttpStatusCode.Created );
        }
    }
}