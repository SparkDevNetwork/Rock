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
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Rock.CheckIn.v2.Labels;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Lava.Blocks
{
    /// <summary>
    /// Lava block that prints raw ZPL to a network printer via TCP (default port 9100).
    /// Parameters:
    /// - deviceid: integer/idkey/guid of a Rock Device (preferred if both provided)
    /// - ipaddress: "x.x.x.x" or "x.x.x.x:9100"
    /// Outputs nothing on success. Outputs an error message to the Lava render only on failure.
    ///
    /// Usage:
    /// {% printzpl deviceid:'12' %}
    /// ^XA ... ^XZ
    /// {% endprintzpl %}
    /// Or:
    /// {% printzpl ipaddress:'192.168.1.145' %}
    /// ^XA ... ^XZ
    /// {% endprintzpl %}
    /// Or:
    /// {% printzpl ipaddress:'192.168.1.145:9100' %}
    /// ^XA ... ^XZ
    /// {% endprintzpl %}
    /// </summary>
    public class PrintZplBlock : LavaBlockBase, ILavaSecured
    {
        private const int DefaultTimeoutMs = 5000;
        private string _markup = string.Empty;

        /// <summary>
        /// Initializes the block with the raw markup.
        /// </summary>
        /// <param name="tagName">The Lava tag name.</param>
        /// <param name="markup">The tag markup (parameters).</param>
        /// <param name="tokens">The tokenized Lava content.</param>
        public override void OnInitialize( string tagName, string markup, System.Collections.Generic.List<string> tokens )
        {
            _markup = markup;
            base.OnInitialize( tagName, markup, tokens );
        }

        /// <summary>
        /// Renders the block by sending the rendered body (ZPL) to the resolved printer endpoint.
        /// Writes to the output only when an error occurs.
        /// </summary>
        /// <param name="context">The Lava render context.</param>
        /// <param name="result">The output writer.</param>
        public override void OnRender( ILavaRenderContext context, TextWriter result )
        {
            if ( !IsAuthorized( context ) )
            {
                result.Write( string.Format( LavaBlockBase.NotAuthorizedMessage, SourceElementName ) );
                return;
            }

            var parms = GetAttributesFromMarkup( _markup, context ).Attributes;

            var bodyWriter = new StringWriter();
            base.OnRender( context, bodyWriter );
            var zpl = ( bodyWriter.ToString() ?? string.Empty ).Trim();

            if ( string.IsNullOrWhiteSpace( zpl ) )
            {
                result.Write( "PrintZpl: No ZPL content was provided in the block body." );
                return;
            }

            var deviceIdValue = parms["deviceid"];
            var ipAddressValue = parms["ipaddress"];

            using ( var cts = new CancellationTokenSource( DefaultTimeoutMs ) )
            {
                try
                {
                    /*
                         3/4/2026 - NA

                         Lava rendering runs synchronously, but the label printing pipeline is asynchronous.
                         Attempting to directly block on PrintAsync() using GetAwaiter().GetResult() caused the
                         thread to hang in certain execution contexts:

                            var errors = PrintAsync( zpl, deviceIdValue, ipAddressValue, cts.Token ).GetAwaiter().GetResult();
                            WriteErrorsIfAny( result, errors );

                         To safely bridge the synchronous Lava rendering flow with the async printing process,
                         the async call is wrapped in Task.Run() and explicitly waited on. This avoids common
                         deadlock patterns associated with blocking async code (such as .Result or
                         GetAwaiter().GetResult()) while still allowing the synchronous pipeline to complete
                         before continuing.
                    */
                    var task = Task.Run( async () => await PrintAsync( zpl, deviceIdValue, ipAddressValue, cts.Token ) );
                    task.Wait();
                    var printErrors = task.Result; // safe after Wait()
                    WriteErrorsIfAny( result, printErrors );
                }
                catch ( Exception ex )
                {
                    result.Write( $"PrintZpl: {ex.Message}" );
                }
            }
        }

        private static async Task<List<string>> PrintAsync( string zpl, string deviceIdValue, string ipAddressValue, CancellationToken cancellationToken )
        {
            var printProvider = new LabelPrintProvider();

            if ( !deviceIdValue.IsNullOrWhiteSpace() )
            {
                var device = DeviceCache.Get( deviceIdValue, true );
                if ( device == null )
                {
                    return new List<string> { "Invalid deviceid." };
                }

                var renderedLabels = BuildLabels( zpl, device );
                return await printProvider.PrintLabelsAsync( renderedLabels, cancellationToken );
            }
            else if ( !ipAddressValue.IsNullOrWhiteSpace() )
            {
                var renderedLabels = BuildLabels( zpl, null );
                var labelContents = renderedLabels.Select( l => l.Data ).ToList();
                return await LabelPrintProvider.PrintToIpEndpointAsync( ipAddressValue, labelContents, cancellationToken );
            }

            return new List<string> { "You must provide a valid deviceid or ipaddress (deviceid is preferred if both are provided)." };
        }

        private static void WriteErrorsIfAny( TextWriter result, List<string> errors )
        {
            if ( errors == null || !errors.Any() )
            {
                return;
            }

            result.Write( $"PrintZpl: {errors.JoinStringsWithCommaAnd()}" );
        }

        internal static List<RenderedLabel> BuildLabels( string zpl, DeviceCache device )
        {
            // LabelPrintProvider expects bytes; ZPL is ASCII-compatible, but UTF-8 is safe and consistent.
            var payload = Encoding.UTF8.GetBytes( EnsureEndsWithNewline( zpl ) );
            return new List<RenderedLabel>
            {
                new RenderedLabel
                {
                    PrintFrom = PrintFrom.Server,
                    Data = payload,
                    PrintTo = device
                }
            };
        }

        private static string EnsureEndsWithNewline( string s )
        {
            if ( string.IsNullOrEmpty( s ) )
            {
                return "\n";
            }

            if ( s.EndsWith( "\n" ) || s.EndsWith( "\r\n" ) )
            {
                return s;
            }

            return s + "\n";
        }
        internal static LavaElementAttributes GetAttributesFromMarkup( string markup, ILavaRenderContext context )
        {
            var attributes = LavaElementAttributes.NewFromMarkup( markup, context );
            attributes.AddOrIgnore( "deviceid", "" );
            attributes.AddOrIgnore( "ipaddress", "" );
            return attributes;
        }

        /// <summary>
        /// Gets the permission key required to execute this block.
        /// </summary>
        public string RequiredPermissionKey => "PrintZpl";
    }
}