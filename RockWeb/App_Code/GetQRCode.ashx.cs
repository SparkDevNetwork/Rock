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
using System.Collections.Specialized;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using ImageResizer;
using QRCoder;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace RockWeb
{
    public class GetQRCode : IHttpHandler
    {
        /// <summary>
        /// Enables processing of HTTP Web requests by a custom HttpHandler that implements the <see cref="T:System.Web.IHttpHandler" /> interface.
        /// </summary>
        /// <param name="context">An <see cref="T:System.Web.HttpContext" /> object that provides references to the intrinsic server objects (for example, Request, Response, Session, and Server) used to service HTTP requests.</param>
        public void ProcessRequest( HttpContext context )
        {
            try
            {
                context.Response.Clear();
                context.RewritePath( context.Server.HtmlDecode( context.Request.Url.PathAndQuery ) );

                var data = context.Request.QueryString["data"];
                var outputType = context.Request.QueryString["outputType"] ?? string.Empty;

                if ( data.IsNullOrWhiteSpace() )
                {
                    context.Response.StatusCode = System.Net.HttpStatusCode.BadRequest.ConvertToInt();
                    context.Response.StatusDescription = "data must be specified";
                    context.ApplicationInstance.CompleteRequest();
                    return;
                }

                // 
                var pixelsPerModule = context.Request.QueryString["pixelsPerModule"].AsIntegerOrNull() ?? 20;

                using ( QRCodeGenerator qrGenerator = new QRCodeGenerator() )
                {
                    // ECCLevel is the Error Correction setting. see https://www.qrcode.com/en/about/error_correction.html
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode( data, QRCodeGenerator.ECCLevel.Q );

                    Stream responseStream;

                    if ( outputType.Equals( "svg", StringComparison.OrdinalIgnoreCase ) )
                    {
                        var svgQRCode = new SvgQRCode( qrCodeData );
                        context.Response.ContentType = "image/svg+xml";

                        var svgXml = svgQRCode.GetGraphic( pixelsPerModule );
                        responseStream = svgXml.ToMemoryStream();
                    }
                    else
                    {
                        context.Response.ContentType = "image/png";

                        var pngByteQRCode = new PngByteQRCode( qrCodeData );
                        responseStream = new MemoryStream( pngByteQRCode.GetGraphic( pixelsPerModule ) );
                    }

                    responseStream.CopyTo( context.Response.OutputStream );

                    context.Response.Flush();
                    context.ApplicationInstance.CompleteRequest();
                }
            }
            catch ( Exception )
            {
                if ( !context.Response.IsClientConnected )
                {
                    // if client disconnected, ignore
                }
                else
                {
                    throw;
                }
            }
        }


        /// <summary>
        /// Gets a value indicating whether another request can use the <see cref="T:System.Web.IHttpHandler" /> instance.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Web.IHttpHandler" /> instance is reusable; otherwise, false.</returns>
        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}