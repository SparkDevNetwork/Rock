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

using Rock.CheckIn.v2;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.ViewModels.CheckIn;
using Rock.ViewModels.Rest.CheckIn;
using Rock.Web.Cache;


#if WEBFORMS
using FromBodyAttribute = System.Web.Http.FromBodyAttribute;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using IActionResult = System.Web.Http.IHttpActionResult;
using RouteAttribute = System.Web.Http.RouteAttribute;
using RoutePrefixAttribute = System.Web.Http.RoutePrefixAttribute;
#endif

namespace Rock.Rest.v2.Controllers
{
    /// <summary>
    /// Provides API interfaces for the Check-in system in Rock.
    /// </summary>
    /// <seealso cref="Rock.Rest.ApiControllerBase" />
    [RoutePrefix( "api/v2/checkin" )]
    [Rock.SystemGuid.RestControllerGuid( "52b3c68a-da8d-4374-a199-8bc8368a22bc" )]
    public sealed class CheckInController : ApiControllerBase
    {
        private readonly RockContext _rockContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckInController"/> class.
        /// </summary>
        /// <param name="rockContext">The database context to use for this request.</param>
        public CheckInController( RockContext rockContext )
        {
            _rockContext = rockContext;
        }

        /// <summary>
        /// Gets the configuration items available to be selected.
        /// </summary>
        /// <param name="options">The options that describe the request.</param>
        /// <returns>A bag that contains all the configuration items.</returns>
        [HttpPost]
        [Authenticate]
        //[Secured]
        [Route( "Configuration" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( ConfigurationResponseBag ) )]
        [SystemGuid.RestActionGuid( "200dd82f-6532-4437-9ba4-a289408b0eb8" )]
        public IActionResult PostConfiguration( [FromBody] ConfigurationOptionsBag options )
        {
            var helper = new CheckInDirector( _rockContext );
            DeviceCache kiosk = null;

            if ( options.KioskGuid.HasValue )
            {
                kiosk = DeviceCache.Get( options.KioskGuid.Value );

                if ( kiosk == null )
                {
                    return BadRequest( "Kiosk was not found." );
                }
            }

            try
            {
                return Ok( new ConfigurationResponseBag
                {
                    Templates = helper.GetConfigurationTemplateBags(),
                    Areas = helper.GetCheckInAreaSummaries( kiosk, null )
                } );
            }
            catch ( CheckInMessageException ex )
            {
                return BadRequest( ex.Message );
            }
        }

        /// <summary>
        /// Performs a search for matching families that are valid for check-in.
        /// </summary>
        /// <param name="options">The options that describe the request.</param>
        /// <returns>A bag that contains all the matched families.</returns>
        [HttpPost]
        [Authenticate]
        //[Secured]
        [Route( "SearchForFamilies" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( SearchForFamiliesResponseBag ) )]
        [SystemGuid.RestActionGuid( "2c587733-0e08-4e93-8f2b-3e2518362768" )]
        public IActionResult PostSearchForFamilies( [FromBody] SearchForFamiliesOptionsBag options )
        {
            var configuration = GroupTypeCache.Get( options.ConfigurationTemplateGuid, _rockContext )?.GetCheckInConfiguration( _rockContext );
            CampusCache sortByCampus = null;

            if ( configuration == null )
            {
                return BadRequest( "Configuration was not found." );
            }

            if ( options.KioskGuid.HasValue && options.PrioritizeKioskCampus )
            {
                var kiosk = DeviceCache.Get( options.KioskGuid.Value );

                if ( kiosk == null )
                {
                    return BadRequest( "Kiosk was not found." );
                }

                var campusId = kiosk.GetCampusId();

                if ( campusId.HasValue )
                {
                    sortByCampus = CampusCache.Get( campusId.Value, _rockContext );
                }
            }

            try
            {
                var director = new CheckInDirector( _rockContext );
                var session = director.CreateSession( configuration );
                var families = session.SearchForFamilies( options.SearchTerm,
                    options.SearchType,
                    sortByCampus );

                return Ok( new SearchForFamiliesResponseBag
                {
                    Families = families
                } );
            }
            catch ( CheckInMessageException ex )
            {
                return BadRequest( ex.Message );
            }
        }

        /// <summary>
        /// Performs a search for matching families that are valid for check-in.
        /// </summary>
        /// <param name="options">The options that describe the request.</param>
        /// <returns>A bag that contains all the matched families.</returns>
        [HttpPost]
        [Authenticate]
        //[Secured]
        [Route( "FamilyMembers" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( FamilyMembersResponseBag ) )]
        [SystemGuid.RestActionGuid( "2bd5afdf-da57-48bb-a6db-7dd9ad1ab8da" )]
        public IActionResult PostFamilyMembers( [FromBody] FamilyMembersOptionsBag options )
        {
            var configuration = GroupTypeCache.Get( options.ConfigurationTemplateGuid, _rockContext )?.GetCheckInConfiguration( _rockContext );
            var kiosk = DeviceCache.Get( options.KioskGuid, _rockContext );
            var areas = options.AreaGuids.Select( guid => GroupTypeCache.Get( guid, _rockContext ) ).ToList();

            if ( configuration == null )
            {
                return BadRequest( "Configuration was not found." );
            }

            if ( kiosk == null )
            {
                return BadRequest( "Kiosk was not found." );
            }

            try
            {
                var director = new CheckInDirector( _rockContext );
                var session = director.CreateSession( configuration );

                session.LoadAndPrepareAttendeesForFamily( options.FamilyGuid, areas, kiosk, null );

                return Ok( new FamilyMembersResponseBag
                {
                    FamilyGuid = options.FamilyGuid,
                    People = session.GetAttendeeBags(),
                    CurrentlyCheckedInAttendances = session.GetCurrentAttendanceBags()
                } );
            }
            catch ( CheckInMessageException ex )
            {
                return BadRequest( ex.Message );
            }
        }

        /// <summary>
        /// Gets the available check-in opportunities for a single attendee.
        /// </summary>
        /// <param name="options">The options that describe the request.</param>
        /// <returns>A bag that contains all the opportunities.</returns>
        [HttpPost]
        [Authenticate]
        //[Secured]
        [Route( "AttendeeOpportunities" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( AttendeeOpportunitiesResponseBag ) )]
        [SystemGuid.RestActionGuid( "2bd5afdf-da57-48bb-a6db-7dd9ad1ab8da" )]
        public IActionResult PostAttendeeOpportunities( [FromBody] AttendeeOpportunitiesOptionsBag options )
        {
            var configuration = GroupTypeCache.Get( options.ConfigurationTemplateGuid, _rockContext )?.GetCheckInConfiguration( _rockContext );
            var areas = options.AreaGuids.Select( guid => GroupTypeCache.Get( guid, _rockContext ) ).ToList();
            var kiosk = DeviceCache.Get( options.KioskGuid, _rockContext );

            if ( configuration == null )
            {
                return BadRequest( "Configuration was not found." );
            }

            if ( kiosk == null )
            {
                return BadRequest( "Kiosk was not found." );
            }

            try
            {
                var director = new CheckInDirector( _rockContext );
                var session = director.CreateSession( configuration );

                session.LoadAndPrepareAttendeesForPerson( options.PersonGuid, options.FamilyGuid, areas, kiosk, null );

                if ( session.Attendees.Count == 0 )
                {
                    return BadRequest( "Individual was not found or is not available for check-in." );
                }

                return Ok( new AttendeeOpportunitiesResponseBag
                {
                    Opportunities = session.GetOpportunityCollectionBag( session.Attendees[0].Opportunities )
                } );
            }
            catch ( CheckInMessageException ex )
            {
                return BadRequest( ex.Message );
            }
        }

        /// <summary>
        /// Saves the attendance for the specified requests.
        /// </summary>
        /// <param name="options">The options that describe the request.</param>
        /// <returns>The results from the save operation.</returns>
        [HttpPost]
        [Authenticate]
        //[Secured]
        [Route( "SaveAttendance" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( SaveAttendanceResponseBag ) )]
        [SystemGuid.RestActionGuid( "7ef059cb-99ba-4cf1-b7d5-3723eb320a99" )]
        public IActionResult PostSaveAttendance( [FromBody] SaveAttendanceOptionsBag options )
        {
            var configuration = GroupTypeCache.Get( options.TemplateGuid, _rockContext )?.GetCheckInConfiguration( _rockContext );
            DeviceCache kiosk = null;

            if ( configuration == null )
            {
                return BadRequest( "Configuration was not found." );
            }

            if ( options.KioskGuid.HasValue )
            {
                kiosk = DeviceCache.Get( options.KioskGuid.Value, _rockContext );

                if ( kiosk == null )
                {
                    return BadRequest( "Kiosk was not found." );
                }
            }

            try
            {
                var director = new CheckInDirector( _rockContext );
                var session = director.CreateSession( configuration );
                var sessionRequest = new AttendanceSessionRequest( options.Session );

                var result = session.SaveAttendance( sessionRequest, options.Requests, kiosk, RockRequestContext.ClientInformation.IpAddress );

                return Ok( new SaveAttendanceResponseBag
                {
                    Messages = result.Messages,
                    Attendances = result.Attendances
                } );
            }
            catch ( CheckInMessageException ex )
            {
                return BadRequest( ex.Message );
            }
        }

        /// <summary>
        /// Confirms the pending attendance records for a session.
        /// </summary>
        /// <param name="options">The options that describe the request.</param>
        /// <returns>The results from the confirm operation.</returns>
        [HttpPost]
        [Authenticate]
        //[Secured]
        [Route( "ConfirmAttendance" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( ConfirmAttendanceResponseBag ) )]
        [SystemGuid.RestActionGuid( "52070226-289b-442d-a8fe-a8323c0f922c" )]
        public IActionResult PostConfirmAttendance( [FromBody] ConfirmAttendanceOptionsBag options )
        {
            var configuration = GroupTypeCache.Get( options.TemplateGuid, _rockContext )?.GetCheckInConfiguration( _rockContext );

            if ( configuration == null )
            {
                return BadRequest( "Configuration was not found." );
            }

            try
            {
                var director = new CheckInDirector( _rockContext );
                var session = director.CreateSession( configuration );

                var result = session.ConfirmAttendance( options.SessionGuid );

                return Ok( new ConfirmAttendanceResponseBag
                {
                    Messages = result.Messages,
                    Attendances = result.Attendances
                } );
            }
            catch ( CheckInMessageException ex )
            {
                return BadRequest( ex.Message );
            }
        }

        #region Temporary Benchmark

        /// <summary>
        /// Performs a set of benchmark runs to determine timings.
        /// </summary>
        /// <returns>The results of the benchmarks.</returns>
        [HttpPost]
        [Authenticate]
        [Route( "Benchmark" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( object ) )]
        [SystemGuid.RestActionGuid( "1eb635a9-6a6a-4445-a0a2-bb59a5a08982" )]
        public IActionResult PostBenchmark( [FromBody] BenchmarkOptionsBag options )
        {
            var configuration = GroupTypeCache.Get( options.ConfigurationTemplateGuid, _rockContext )?.GetCheckInConfiguration( _rockContext );
            var kiosk = DeviceCache.Get( options.KioskGuid, _rockContext );
            var areas = options.AreaGuids.Select( guid => GroupTypeCache.Get( guid, _rockContext ) ).ToList();
            var bench = new Rock.Utility.Performance.MicroBench();
            var validBenchmarks = new List<string> { "empty", "familySearch", "getFamilyMembers", "getFamilyMemberBags", "getAllOpportunities", "cloneOpportunities", "filterOpportunities" };

            bench.RepititionMode = Rock.Enums.Core.BenchmarkRepititionMode.Fast;

            if ( configuration == null )
            {
                return BadRequest( "Configuration was not found." );
            }

            if ( kiosk == null )
            {
                return BadRequest( "Kiosk was not found." );
            }

            if ( options.Benchmarks.Count == 1 && options.Benchmarks[0] == "all" )
            {
                options.Benchmarks = validBenchmarks;
            }

            if ( options.Benchmarks.Any( b => !validBenchmarks.Contains( b ) ) )
            {
                return BadRequest( "Invalid benchmark specified." );
            }

            var results = new Dictionary<string, object>();

            foreach ( var benchmark in options.Benchmarks )
            {
                if ( benchmark == "empty" )
                {
                    var result = bench.Benchmark( () =>
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            var director = new CheckInDirector( rockContext );
                        }
                    } );

                    results.Add( benchmark, result.NormalizedStatistics.ToString() );
                }
                else if ( benchmark == "familySearch" )
                {
                    var result = bench.Benchmark( () =>
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            var director = new CheckInDirector( rockContext );
                            var session = director.CreateSession( configuration );

                            var families = session.SearchForFamilies( "5553322",
                                Enums.CheckIn.FamilySearchMode.PhoneNumber,
                                null );
                        }
                    } );

                    results.Add( benchmark, result.NormalizedStatistics.ToString() );
                }
                else if ( benchmark == "getFamilyMembers" )
                {
                    var result = bench.Benchmark( () =>
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            var director = new CheckInDirector( rockContext );
                            var session = director.CreateSession( configuration );
                            var familyMembersQry = session.GetGroupMembersQueryForFamily( options.FamilyGuid );
                        }
                    } );

                    results.Add( benchmark, result.NormalizedStatistics.ToString() );
                }
                else if ( benchmark == "getFamilyMemberBags" )
                {
                    IEnumerable<GroupMember> familyMembers;
                    PersonBag familyMemberBag;

                    using ( var rockContext = new RockContext() )
                    {
                        var director = new CheckInDirector( rockContext );
                        var session = director.CreateSession( configuration );
                        var familyMembersQry = session.GetGroupMembersQueryForFamily( options.FamilyGuid );

                        familyMembers = familyMembersQry
                            .Include( fm => fm.Person )
                            .Include( fm => fm.Person.PrimaryFamily )
                            .Include( fm => fm.GroupRole )
                            .ToList();

                        familyMemberBag = session.GetPersonBags( options.FamilyGuid, familyMembers ).First( fm => fm.FirstName == "Noah" );
                    }

                    var result = bench.Benchmark( () =>
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            var director = new CheckInDirector( rockContext );
                            var session = director.CreateSession( configuration );

                            var bags = session.GetPersonBags( options.FamilyGuid, familyMembers );
                        }
                    } );

                    results.Add( benchmark, result.NormalizedStatistics.ToString() );
                }
                else if ( benchmark == "getAllOpportunities" )
                {
                    var result = bench.Benchmark( () =>
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            var director = new CheckInDirector( rockContext );

                            var opportunities = director.GetAllOpportunities( areas, kiosk, null );
                        }
                    } );

                    results.Add( benchmark, result.NormalizedStatistics.ToString() );
                }
                else if ( benchmark == "cloneOpportunities" )
                {
                    OpportunityCollection mainOpportunities;

                    using ( var rockContext = new RockContext() )
                    {
                        var director = new CheckInDirector( rockContext );

                        mainOpportunities = director.GetAllOpportunities( areas, kiosk, null );
                    }

                    var result = bench.Benchmark( () =>
                    {
                        var clonedOpportunities = mainOpportunities.Clone();
                    } );

                    results.Add( benchmark, result.NormalizedStatistics.ToString() );
                }
                else if ( benchmark == "filterOpportunities" )
                {
                    OpportunityCollection mainOpportunities;
                    PersonBag familyMemberBag;

                    using ( var rockContext = new RockContext() )
                    {
                        var director = new CheckInDirector( rockContext );
                        var session = director.CreateSession( configuration );
                        var familyMembersQry = session.GetGroupMembersQueryForFamily( options.FamilyGuid );

                        familyMemberBag = session.GetPersonBags( options.FamilyGuid, familyMembersQry ).First( fm => fm.FirstName == "Noah" );
                        mainOpportunities = director.GetAllOpportunities( areas, kiosk, null );
                    }

                    var result = bench.Benchmark( () =>
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            var director = new CheckInDirector( rockContext );
                            var session = director.CreateSession( configuration );

                            var person = new Attendee
                            {
                                Person = familyMemberBag,
                                Opportunities = mainOpportunities.Clone()
                            };

                            session.FilterPersonOpportunities( person );
                        }
                    } );

                    results.Add( benchmark, result.NormalizedStatistics.ToString() );
                }
            }

            return Ok( results );
        }

        /// <summary>
        /// Temporary, used by benchmark action.
        /// </summary>
        public class BenchmarkOptionsBag : FamilyMembersOptionsBag
        {
            /// <summary>
            /// Gets or sets the benchmarks.
            /// </summary>
            /// <value>The benchmarks.</value>
            public List<string> Benchmarks { get; set; }
        }

        #endregion
    }
}