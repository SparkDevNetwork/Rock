using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

using com.ccvonline.CommandCenter.Model;

using Rock.Rest;
using Rock.Rest.Filters;

namespace com.ccvonline.CommandCenter.Rest
{
    /// <summary>
    /// Recordings REST API
    /// </summary>
    /// 
    public partial class RecordingsController : Rock.Rest.ApiController<Recording>, IHasCustomRoutes
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecordingsController" /> class.
        /// </summary>
        public RecordingsController() : base( new RecordingService( new Data.CommandCenterContext() ) ) { }

        /// <summary>
        /// Adds the routes.
        /// </summary>
        /// <param name="routes">The routes.</param>
        public void AddRoutes( System.Web.Routing.RouteCollection routes )
        {
            routes.MapHttpRoute(
                name: "com.ccvonline.CommandCenter.Recording",
                routeTemplate: "api/Recordings/{action}/{campusId}/{venue}/{label}/{app}/{stream}/{recording}",
                defaults: new
                {
                    controller = "recordings"
                } );

            routes.MapHttpRoute(
                name: "com.ccvonline.CommandCenter.RecordingDate",
                routeTemplate: "api/Recordings/dates/{qualifier}",
                defaults: new
                {
                    controller = "recordings",
                    action = "dates"
                } );
        }

        /// <summary>
        /// Starts the specified campus id.
        /// </summary>
        /// <param name="campusId">The campus id.</param>
        /// <param name="venue">The venue.</param>
        /// <param name="label">The label.</param>
        /// <param name="app">The app.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="recording">The recording.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [HttpGet]
        [Authenticate]
        public Recording Start( int campusId, string venue, string label, string app, string stream, string recording )
        {
            var RecordingService = new RecordingService( new Data.CommandCenterContext() );
            var Recording = RecordingService.StartRecording( campusId, venue, label, app, stream, recording );

            if ( Recording != null )
                return Recording;
            else
                throw new HttpResponseException( HttpStatusCode.BadRequest );
        }

        /// <summary>
        /// Stops the specified campus id.
        /// </summary>
        /// <param name="campusId">The campus id.</param>
        /// <param name="venue">The venue.</param>
        /// <param name="label">The label.</param>
        /// <param name="app">The app.</param>
        /// <param name="stream">The stream.</param>
        /// <param name="recording">The recording.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [HttpGet]
        [Authenticate]
        public Recording Stop( int campusId, string venue, string label, string app, string stream, string recording )
        {
            var RecordingService = new RecordingService( new Data.CommandCenterContext() );
            var Recording = RecordingService.StopRecording( campusId, venue, label, app, stream, recording );

            if ( Recording != null )
                return Recording;
            else
                throw new HttpResponseException( HttpStatusCode.BadRequest );
        }

        /// <summary>
        /// Dateses the specified qualifier.
        /// </summary>
        /// <param name="qualifier">The qualifier.</param>
        /// <returns></returns>
        /// <exception cref="System.Web.Http.HttpResponseException"></exception>
        [HttpGet]
        [Authenticate]
        public IEnumerable<DateTime> Dates( string qualifier )
        {
            var dates = Service.Queryable()
                .Where( r => r.StartTime.HasValue )
                .OrderByDescending( r => r.StartTime.Value )
                .Select( r => r.StartTime.Value )
                .ToList();

            if ( string.Equals( qualifier, "distinct", StringComparison.CurrentCultureIgnoreCase ) )
                return dates.Select( d => d.Date ).Distinct();
            else
                return dates;
        }

    }
}
