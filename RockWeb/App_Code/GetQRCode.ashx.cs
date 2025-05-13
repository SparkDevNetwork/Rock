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
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using ImageResizer;
using QRCoder;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using System.Net.Http.Headers;

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
                context.RewritePath( context.Server.HtmlDecode( context.Request.UrlProxySafe().PathAndQuery ) );

                var data = context.Request.QueryString["data"];
                var outputType = context.Request.QueryString["outputType"] ?? string.Empty;
                var foregroundColor = context.Request.QueryString["foreground"] ?? "000000";
                var backgroundColor = context.Request.QueryString["background"] ?? "ffffff";

                if ( data.IsNullOrWhiteSpace() )
                {
                    context.Response.StatusCode = System.Net.HttpStatusCode.BadRequest.ConvertToInt();
                    context.Response.StatusDescription = "data must be specified";
                    context.ApplicationInstance.CompleteRequest();
                    return;
                }

                var pixelsPerModule = context.Request.QueryString["pixelsPerModule"].AsIntegerOrNull() ?? 20;

                using ( var qrGenerator = new QRCodeGenerator() )
                {
                    var qrCodeData = qrGenerator.CreateQrCode( data, QRCodeGenerator.ECCLevel.Q );
                    var responseStream = GetResponseStream( qrCodeData, outputType, pixelsPerModule, backgroundColor, foregroundColor );

                    responseStream.CopyTo( context.Response.OutputStream );
                    context.Response.ContentType = outputType.Equals( "svg", StringComparison.OrdinalIgnoreCase ) ? "image/svg+xml" : "image/png";
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
        /// Generates a stream containing the QR code graphic based on the provided data and output type.
        /// </summary>
        /// <param name="qrCodeData">The QR code data to be rendered.</param>
        /// <param name="outputType">The type of output to generate ("svg" or "png").</param>
        /// <param name="pixelsPerModule">The number of pixels per module in the QR code graphic.</param>
        /// <param name="backgroundColor">The background color for the QR code, in hex format.</param>
        /// <param name="foregroundColor">The foreground color for the QR code, in hex format.</param>
        /// <returns>A <see cref="Stream"/> containing the generated QR code graphic.</returns>
        /// <exception cref="ArgumentException">Thrown when the output type is not recognized or when color hex values are invalid.</exception>
        private Stream GetResponseStream( QRCodeData qrCodeData, string outputType, int pixelsPerModule, string backgroundColor, string foregroundColor )
        {
            if ( outputType.Equals( "svg", StringComparison.OrdinalIgnoreCase ) )
            {
                // Ensure colors for SVG have a hash using a ternary operator
                foregroundColor = foregroundColor.StartsWith( "#" ) ? foregroundColor : "#" + foregroundColor;
                backgroundColor = backgroundColor.StartsWith( "#" ) ? backgroundColor : "#" + backgroundColor;

                var svgQRCode = new SvgQRCode( qrCodeData );
                var svgXml = svgQRCode.GetGraphic( pixelsPerModule, foregroundColor, backgroundColor );
                return svgXml.ToMemoryStream();
            }
            else
            {
                // For PNG, convert hex colors to byte arrays directly
                var lightColor = HexToByteArray( backgroundColor );
                var darkColor = HexToByteArray( foregroundColor );
                var qrCodeImage = new PngByteQRCode( qrCodeData ).GetGraphic( pixelsPerModule, darkColor, lightColor );
                return new MemoryStream( qrCodeImage );
            }
        }

        /// <summary>
        /// Converts a hex color string to a byte array representing the RGBA color.
        /// </summary>
        /// <param name="hex">The hex color string (6 or 8 digits, with or without a leading '#').</param>
        /// <returns>A byte array representing the RGBA color.</returns>
        /// <exception cref="ArgumentException">Thrown when the hex color string is not a valid 6 or 8 digit hex value.</exception>
        private byte[] HexToByteArray( string hex )
        {
            if ( !Regex.IsMatch( hex, @"^#?([0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$" ) )
            {
                throw new ArgumentException( "Color must be a valid 6 or 8 digit hex value." );
            }

            hex = hex.StartsWith( "#" ) ? hex.Substring( 1 ) : hex;

            //RGBA
            byte[] bytes;
            if ( hex.Length == 6 )
            {
                bytes = new byte[]
                {
                    Convert.ToByte(hex.Substring( 0, 2 ), 16 ),
                    Convert.ToByte(hex.Substring( 2, 2 ), 16 ),
                    Convert.ToByte(hex.Substring( 4, 2 ), 16 ),
                    255
                };
            }
            else // 8 digits for RGBA
            {
                bytes = new byte[]
                {
                    Convert.ToByte( hex.Substring( 0, 2 ), 16 ),
                    Convert.ToByte( hex.Substring( 2, 2 ), 16 ),
                    Convert.ToByte( hex.Substring( 4, 2 ), 16 ),
                    Convert.ToByte( hex.Substring( 6, 2 ), 16 )
                };
            }

            return bytes;
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
