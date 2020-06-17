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
using System.Web;
using System.Web.Http;

using Rock.CheckIn;
using Rock.Data;
using Rock.Model;
using Rock.Utility;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Rest.ApiControllerBase" />
    public class CheckinController : ApiControllerBase
    {
        /// <summary>
        /// Gets the configuration status of a checkin device
        /// </summary>
        /// <param name="localDeviceConfiguration">The local device configuration.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [HttpPost]
        [System.Web.Http.Route( "api/checkin/configuration/status" )]
        public LocalDeviceConfigurationStatus GetConfigurationStatus( LocalDeviceConfiguration localDeviceConfiguration )
        {
            if ( localDeviceConfiguration?.CurrentKioskId == null || localDeviceConfiguration?.CurrentCheckinTypeId == null )
            {
                var response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent( "LocalDeviceConfiguration with a valid KioskId and Checkin Type  is required" )
                };

                throw new HttpResponseException( response );
            }

            LocalDeviceConfigurationStatus localDeviceConfigurationStatus = CheckinConfigurationHelper.GetLocalDeviceConfigurationStatus( localDeviceConfiguration, HttpContext.Current.Request );

            return localDeviceConfigurationStatus;
        }

        /// <summary>
        /// Prints the session labels for the specified sessions.
        /// </summary>
        /// <param name="session">The session(s) to be printed.</param>
        /// <param name="kioskId">The kiosk identifier requesting the labels.</param>
        /// <returns>
        /// A collection of <see cref="CheckInLabel">labels</see> that should be printed by the client.
        /// </returns>
        [HttpGet]
        [System.Web.Http.Route( "api/checkin/printsessionlabels" )]
        public PrintSessionLabelsResponse PrintSessionLabels([FromUri] string session, [FromUri] int? kioskId = null )
        {
            List<Guid?> sessionGuids;

            //
            // The session data is a comma separated list of Guid values.
            //
            try
            {
                sessionGuids = session.SplitDelimitedValues()
                    .Select( a =>
                    {
                        var guid = a.AsGuidOrNull();

                        //
                        // Check if this is a standard Guid format.
                        //
                        if ( guid.HasValue )
                        {
                            return guid.Value;
                        }
                        
                        return GuidHelper.FromShortStringOrNull( a );

                    } )
                    .ToList();
            }
            catch
            {
                sessionGuids = null;
            }

            //
            // If no session guids were found or an error occurred trying to
            // parse them, then return an error.
            //
            if ( sessionGuids == null || sessionGuids.Count == 0 )
            {
                return new PrintSessionLabelsResponse
                {
                    Labels = new List<CheckInLabel>(),
                    Messages = new List<string>
                    {
                        "No check-in sessions were specified."
                    }
                };
            }

            using ( var rockContext = new RockContext() )
            {
                KioskDevice printer = null;

                //
                // If they specified a kiosk, attempt to load the printer for
                // that kiosk.
                //
                if ( kioskId.HasValue && kioskId.Value != 0 )
                {
                    var kiosk = KioskDevice.Get( kioskId.Value, null );
                    if ( ( kiosk?.Device?.PrinterDeviceId ).HasValue )
                    {
                        //
                        // We aren't technically loading a kiosk, but this lets us
                        // load the printer IP address from cache rather than hitting
                        // the database each time.
                        //
                        printer = KioskDevice.Get( kiosk.Device.PrinterDeviceId.Value, null );
                    }
                }

                var attendanceService = new AttendanceService( rockContext );

                //
                // Retrieve all session label data from the database, then deserialize
                // it, then make one giant list of labels and finally order it.
                //
                var labels = attendanceService.Queryable()
                    .AsNoTracking()
                    .Where( a => sessionGuids.Contains( a.AttendanceCheckInSession.Guid ) )
                    .DistinctBy( a => a.AttendanceCheckInSessionId )
                    .Select( a => a.AttendanceData.LabelData )
                    .ToList()
                    .Select( a => a.FromJsonOrNull<List<CheckInLabel>>() )
                    .Where( a => a != null )
                    .SelectMany( a => a )
                    .OrderBy( a => a.PersonId )
                    .ThenBy( a => a.Order )
                    .ToList();

                foreach ( var label in labels )
                {
                    //
                    // If the label is being printed to the kiosk and client printing
                    // is enabled and the client has specified their device
                    // identifier then we need to update the label data to have the
                    // new printer information.
                    //
                    if ( label.PrintTo == PrintTo.Kiosk && label.PrintFrom == PrintFrom.Client && printer != null )
                    {
                        label.PrinterDeviceId = printer.Device.Id;
                        label.PrinterAddress = printer.Device.IPAddress;
                    }
                }

                var response = new PrintSessionLabelsResponse
                {
                    Labels = labels.Where( l => l.PrintFrom == PrintFrom.Client ).ToList()
                };

                //
                // Update any labels to convert the relative URL path to an absolute URL
                // path for client printing.
                //
                if ( response.Labels.Any() )
                {
                    var urlRoot = Request.RequestUri.GetLeftPart( UriPartial.Authority );

                    if ( Request.Headers.Contains( "X-Forwarded-Proto" ) && Request.Headers.Contains( "X-Forwarded-Host" ) )
                    {
                        urlRoot = $"{Request.Headers.GetValues( "X-Forwarded-Proto" ).First()}://{Request.Headers.GetValues( "X-Forwarded-Host" ).First()}";
                    }
#if DEBUG
                    // This is extremely useful when debugging with ngrok and an iPad on the local network.
                    // X-Original-Host will contain the name of your ngrok hostname, therefore the labels will
                    // get a LabelFile url that will actually work with that iPad.
                    if ( Request.Headers.Contains( "X-Forwarded-Proto" ) && Request.Headers.Contains( "X-Original-Host" ) )
                    {
                        urlRoot = $"{Request.Headers.GetValues( "X-Forwarded-Proto" ).First()}://{Request.Headers.GetValues( "X-Original-Host" ).First()}";
                    }
#endif
                    response.Labels.ForEach( l => l.LabelFile = urlRoot + l.LabelFile );
                }

                var printFromServer = labels.Where( l => l.PrintFrom == PrintFrom.Server ).ToList();

                if ( printFromServer.Any() )
                {
                    var messages = ZebraPrint.PrintLabels( printFromServer );
                    response.Messages = messages;
                }

                return response;
            }
        }

        #region Support Classes

        /// <summary>
        /// The response data used with the <see cref="PrintSessionLabels(string, int?)" />
        /// API endpoint.
        /// </summary>
        public class PrintSessionLabelsResponse
        {
            /// <summary>
            /// Gets or sets the labels to be printed by the client.
            /// </summary>
            /// <value>
            /// The labels to be printed by the client.
            /// </value>
            public List<CheckInLabel> Labels { get; set; }

            /// <summary>
            /// Gets or sets the errors encountered by the server.
            /// </summary>
            /// <value>
            /// The errors encountered by the server.
            /// </value>
            public List<string> Messages { get; set; }
        }

        #endregion
    }
}
