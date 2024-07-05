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
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Labels;
using Rock.Data;
using Rock.Rest.Filters;
using Rock.Utility;
using Rock.ViewModels.Rest.CheckIn;
using Rock.Web.Cache;


#if WEBFORMS
using FromBodyAttribute = System.Web.Http.FromBodyAttribute;
using FromQueryAttribute = System.Web.Http.FromUriAttribute;
using HttpGetAttribute = System.Web.Http.HttpGetAttribute;
using HttpPostAttribute = System.Web.Http.HttpPostAttribute;
using IActionResult = System.Web.Http.IHttpActionResult;
using RouteAttribute = System.Web.Http.RouteAttribute;
using RoutePrefixAttribute = System.Web.Http.RoutePrefixAttribute;
#endif

namespace Rock.Rest.v2
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
        [Secured]
        [Route( "Configuration" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( ConfigurationResponseBag ) )]
        [SystemGuid.RestActionGuid( "200dd82f-6532-4437-9ba4-a289408b0eb8" )]
        public IActionResult PostConfiguration( [FromBody] ConfigurationOptionsBag options )
        {
            var helper = new CheckInDirector( _rockContext );
            DeviceCache kiosk = null;

            if ( options.KioskId.IsNotNullOrWhiteSpace() )
            {
                kiosk = DeviceCache.GetByIdKey( options.KioskId, _rockContext );

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
        /// Gets current status that the check-in kiosk should be in as well
        /// as when it should open or close.
        /// </summary>
        /// <param name="options">The options that describe the request.</param>
        /// <returns>A bag that contains the status.</returns>
        [HttpPost]
        [Authenticate]
        [Secured]
        [Route( "KioskStatus" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( KioskStatusResponseBag ) )]
        [SystemGuid.RestActionGuid( "7fb87711-1ecf-49ca-90cb-3e2e1b02a933" )]
        public IActionResult PostKioskStatus( [FromBody] KioskStatusOptionsBag options )
        {
            var director = new CheckInDirector( _rockContext );
            var kiosk = DeviceCache.GetByIdKey( options.KioskId, _rockContext );
            List<GroupTypeCache> areas = null;

            if ( options.AreaIds != null )
            {
                var areaIdNumbers = options.AreaIds
                    .Select( id => IdHasher.Instance.GetId( id ) )
                    .Where( id => id.HasValue )
                    .Select( id => id.Value )
                    .ToList();

                areas = GroupTypeCache.GetMany( areaIdNumbers, _rockContext ).ToList();
            }

            if ( kiosk == null )
            {
                return BadRequest( "Kiosk was not found." );
            }

            if ( areas == null )
            {
                return BadRequest( "Area list cannot be null." );
            }

            return Ok( new KioskStatusResponseBag
            {
                Status = director.GetKioskStatus( areas, kiosk, null )
            } );
        }

        /// <summary>
        /// Performs a search for matching families that are valid for check-in.
        /// </summary>
        /// <param name="options">The options that describe the request.</param>
        /// <returns>A bag that contains all the matched families.</returns>
        [HttpPost]
        [Authenticate]
        [Secured]
        [Route( "SearchForFamilies" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( SearchForFamiliesResponseBag ) )]
        [SystemGuid.RestActionGuid( "2c587733-0e08-4e93-8f2b-3e2518362768" )]
        public IActionResult PostSearchForFamilies( [FromBody] SearchForFamiliesOptionsBag options )
        {
            var configuration = GroupTypeCache.GetByIdKey( options.ConfigurationTemplateId, _rockContext )?.GetCheckInConfiguration( _rockContext );
            CampusCache sortByCampus = null;

            if ( configuration == null )
            {
                return BadRequest( "Configuration was not found." );
            }

            if ( options.KioskId.IsNotNullOrWhiteSpace() && options.PrioritizeKioskCampus )
            {
                var kiosk = DeviceCache.GetByIdKey( options.KioskId, _rockContext );

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
        [Secured]
        [Route( "FamilyMembers" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( FamilyMembersResponseBag ) )]
        [SystemGuid.RestActionGuid( "2bd5afdf-da57-48bb-a6db-7dd9ad1ab8da" )]
        public IActionResult PostFamilyMembers( [FromBody] FamilyMembersOptionsBag options )
        {
            var configuration = GroupTypeCache.GetByIdKey( options.ConfigurationTemplateId, _rockContext )?.GetCheckInConfiguration( _rockContext );
            var kiosk = DeviceCache.GetByIdKey( options.KioskId, _rockContext );
            var areas = options.AreaIds.Select( id => GroupTypeCache.GetByIdKey( id, _rockContext ) ).ToList();

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

                session.LoadAndPrepareAttendeesForFamily( options.FamilyId, areas, kiosk, null );

                return Ok( new FamilyMembersResponseBag
                {
                    FamilyId = options.FamilyId,
                    PossibleSchedules = session.GetAllPossibleScheduleBags(),
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
        [Secured]
        [Route( "AttendeeOpportunities" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( AttendeeOpportunitiesResponseBag ) )]
        [SystemGuid.RestActionGuid( "6e77e23d-cccb-46b7-a8e9-95706bbb269a" )]
        public IActionResult PostAttendeeOpportunities( [FromBody] AttendeeOpportunitiesOptionsBag options )
        {
            var configuration = GroupTypeCache.GetByIdKey( options.ConfigurationTemplateId, _rockContext )?.GetCheckInConfiguration( _rockContext );
            var areas = options.AreaIds.Select( id => GroupTypeCache.GetByIdKey( id, _rockContext ) ).ToList();
            var kiosk = DeviceCache.GetByIdKey( options.KioskId, _rockContext );

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

                session.LoadAndPrepareAttendeesForPerson( options.PersonId, options.FamilyId, areas, kiosk, null );

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
        [Secured]
        [Route( "SaveAttendance" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( SaveAttendanceResponseBag ) )]
        [SystemGuid.RestActionGuid( "7ef059cb-99ba-4cf1-b7d5-3723eb320a99" )]
        public async Task<IActionResult> PostSaveAttendance( [FromBody] SaveAttendanceOptionsBag options )
        {
            var configuration = GroupTypeCache.GetByIdKey( options.TemplateId, _rockContext )?.GetCheckInConfiguration( _rockContext );
            DeviceCache kiosk = null;

            if ( configuration == null )
            {
                return BadRequest( "Configuration was not found." );
            }

            if ( options.KioskId.IsNotNullOrWhiteSpace() )
            {
                kiosk = DeviceCache.GetByIdKey( options.KioskId, _rockContext );

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

                if ( !options.Session.IsPending )
                {
                    var cts = new CancellationTokenSource( 5000 );
                    await director.LabelProvider.RenderAndPrintCheckInLabelsAsync( result, kiosk, new LabelPrintProvider(), cts.Token );
                }

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
        [Secured]
        [Route( "ConfirmAttendance" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( ConfirmAttendanceResponseBag ) )]
        [SystemGuid.RestActionGuid( "52070226-289b-442d-a8fe-a8323c0f922c" )]
        public async Task<IActionResult> PostConfirmAttendance( [FromBody] ConfirmAttendanceOptionsBag options )
        {
            var configuration = GroupTypeCache.GetByIdKey( options.TemplateId, _rockContext )?.GetCheckInConfiguration( _rockContext );
            DeviceCache kiosk = null;

            if ( configuration == null )
            {
                return BadRequest( "Configuration was not found." );
            }

            if ( options.KioskId.IsNotNullOrWhiteSpace() )
            {
                kiosk = DeviceCache.GetByIdKey( options.KioskId, _rockContext );

                if ( kiosk == null )
                {
                    return BadRequest( "Kiosk was not found." );
                }
            }

            try
            {
                var director = new CheckInDirector( _rockContext );
                var session = director.CreateSession( configuration );

                var result = session.ConfirmAttendance( options.SessionGuid );

                var cts = new CancellationTokenSource( 5000 );
                await director.LabelProvider.RenderAndPrintCheckInLabelsAsync( result, kiosk, new LabelPrintProvider(), cts.Token );

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

        /// <summary>
        /// Saves the attendance for the specified requests.
        /// </summary>
        /// <param name="options">The options that describe the request.</param>
        /// <returns>The results from the save operation.</returns>
        [HttpPost]
        [Authenticate]
        [Secured]
        [Route( "Checkout" )]
        [ProducesResponseType( HttpStatusCode.OK, Type = typeof( CheckoutResponseBag ) )]
        [SystemGuid.RestActionGuid( "733be2ee-dec6-4f7f-92bd-df367c20543d" )]
        public async Task<IActionResult> PostCheckout( [FromBody] CheckoutOptionsBag options )
        {
            var configuration = GroupTypeCache.GetByIdKey( options.TemplateId, _rockContext )?.GetCheckInConfiguration( _rockContext );
            DeviceCache kiosk = null;

            if ( configuration == null )
            {
                return BadRequest( "Configuration was not found." );
            }

            if ( options.KioskId.IsNotNullOrWhiteSpace() )
            {
                kiosk = DeviceCache.GetByIdKey( options.KioskId, _rockContext );

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

                var result = session.Checkout( sessionRequest, options.AttendanceIds, kiosk );

                var cts = new CancellationTokenSource( 5000 );
                await director.LabelProvider.RenderAndPrintCheckoutLabelsAsync( result, kiosk, new LabelPrintProvider(), cts.Token );

                return Ok( result );
            }
            catch ( CheckInMessageException ex )
            {
                return BadRequest( ex.Message );
            }
        }

        /// <summary>
        /// Establishes a connection from the printer proxy service to this
        /// Rock instance.
        /// </summary>
        /// <param name="deviceId">The identifier of the proxy Device in Rock as either a Guid or an IdKey.</param>
        /// <param name="name">The name of the proxy for UI presentation.</param>
        /// <param name="priority">The priority for this proxy when choosing between multiple proxies.</param>
        /// <returns>The result of the operation.</returns>
        [HttpGet]
        [Route( "PrinterProxy/{deviceId}" )]
        [ProducesResponseType( HttpStatusCode.SwitchingProtocols )]
        [SystemGuid.RestActionGuid( "1b4b1d0d-a872-40f7-a49d-666092cf8816" )]
        public IActionResult GetPrinterProxy( string deviceId, [FromQuery] string name = null, [FromQuery] int priority = 1 )
        {
            if ( !System.Web.HttpContext.Current.IsWebSocketRequest )
            {
                return BadRequest( "This API may only be used with websocket connections." );
            }

            DeviceCache device = null;

            if ( IdHasher.Instance.TryGetId( deviceId, out var deviceIdNumber ) )
            {
                device = DeviceCache.Get( deviceIdNumber, _rockContext );
            }
            else if ( Guid.TryParse( deviceId, out var deviceGuid ) )
            {
                device = DeviceCache.Get( deviceGuid, _rockContext );
            }

            if ( device == null )
            {
                return BadRequest( "Device not found." );
            }

            System.Web.HttpContext.Current.AcceptWebSocketRequest( ctx =>
            {
                var proxy = new PrinterProxySocket( ctx.WebSocket, device.Id, name ?? device.Name, priority );

                return proxy.RunAsync( CancellationToken.None );
            } );

            return ResponseMessage( Request.CreateResponse( HttpStatusCode.SwitchingProtocols ) );
        }
    }
}