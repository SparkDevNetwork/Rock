using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;

using church.ccv.CommandCenter.Model;

using Rock.Rest;
using Rock.Rest.Filters;

namespace church.ccv.CommandCenter.Rest
{
    /// <summary>
    /// Recordings REST API
    /// </summary>
    /// 
    public partial class RecordingsController : Rock.Rest.ApiController<Recording>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RecordingsController" /> class.
        /// </summary>
        public RecordingsController() : base( new RecordingService( new Data.CommandCenterContext() ) ) { }

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
        [System.Web.Http.Route( "api/Recordings/Start/{campusId}/{venue}/{label}/{app}/{stream}/{recording}" )]
        [Secured]
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
        [System.Web.Http.Route( "api/Recordings/Stop/{campusId}/{venue}/{label}/{app}/{stream}/{recording}")]
        [Secured]
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
        [System.Web.Http.Route( "api/Recordings/dates/{qualifier}" )]
        [Secured]
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
