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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Rock.Logging;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace Rock.Net
{
    /// <summary>
    /// Downloads images from URLs supplied by outside parties (such as an
    /// identity provider's picture claim) without letting the request reach
    /// private or internal network addresses.
    /// </summary>
    internal static class RemoteImageDownloader
    {
        private const int MaxRedirects = 3;

        private const int MaxImageBytes = 5 * 1024 * 1024;

        private static readonly HttpClient _httpClient = new HttpClient( new HttpClientHandler
        {
            AllowAutoRedirect = false,
            UseCookies = false
        } )
        {
            Timeout = TimeSpan.FromSeconds( 10 )
        };

        /// <summary>
        /// Downloads a JPEG, PNG or GIF image from a public HTTPS URL. Each
        /// redirect is validated the same way as the original URL.
        /// </summary>
        /// <param name="url">The URL of the image.</param>
        /// <param name="bytes">On success, the image content.</param>
        /// <param name="mimeType">On success, the MIME type detected from the image content.</param>
        /// <param name="fileExtension">On success, the file extension without a leading period.</param>
        /// <returns><c>true</c> if a valid image was downloaded; otherwise <c>false</c>.</returns>
        internal static bool TryDownloadImage( string url, out byte[] bytes, out string mimeType, out string fileExtension )
        {
            bytes = null;
            mimeType = null;
            fileExtension = null;

            var task = Task.Run( () => DownloadImageAsync( url ) );

            // This is not ideal, but is a dependable way to wait for the
            // asynchronous task to complete within this synchronous context.
            while ( !task.IsCompleted )
            {
                Thread.Sleep( 50 );
            }

            // When completed, the task will be in one of the three final states:
            // RanToCompletion, Faulted, or Canceled.
            if ( task.IsFaulted || task.IsCanceled || task.Result.Bytes == null )
            {
                return false;
            }

            bytes = task.Result.Bytes;
            mimeType = task.Result.MimeType;
            fileExtension = task.Result.FileExtension;

            return true;
        }

        /// <summary>
        /// Downloads and validates the image for <see cref="TryDownloadImage"/>.
        /// </summary>
        /// <param name="url">The URL of the image.</param>
        /// <returns>The image details, with <c>Bytes</c> set to <c>null</c> if the URL or image was rejected.</returns>
        private static async Task<(byte[] Bytes, string MimeType, string FileExtension)> DownloadImageAsync( string url )
        {
            try
            {
                var bytes = await DownloadBytesAsync( url );

                if ( bytes != null )
                {
                    var format = Image.DetectFormat( bytes );

                    if ( format == JpegFormat.Instance )
                    {
                        return (bytes, "image/jpeg", "jpg");
                    }

                    if ( format == PngFormat.Instance )
                    {
                        return (bytes, "image/png", "png");
                    }

                    if ( format == GifFormat.Instance )
                    {
                        return (bytes, "image/gif", "gif");
                    }
                }
            }
            catch ( Exception ex )
            {
                RockLogger.LoggerFactory.CreateLogger( typeof( RemoteImageDownloader ).FullName )
                    .LogDebug( ex, "Unable to download remote image." );
            }

            return default;
        }

        /// <summary>
        /// Downloads the response body from the URL, following up to
        /// <see cref="MaxRedirects"/> redirects.
        /// </summary>
        /// <param name="url">The URL to download.</param>
        /// <returns>The body bytes, or <c>null</c> if the URL, a redirect or the response was rejected.</returns>
        private static async Task<byte[]> DownloadBytesAsync( string url )
        {
            if ( !Uri.TryCreate( url, UriKind.Absolute, out var uri ) )
            {
                return null;
            }

            for ( var redirectCount = 0; ; redirectCount++ )
            {
                if ( !IsAllowedUri( uri ) )
                {
                    return null;
                }

                using ( var response = await _httpClient.GetAsync( uri, HttpCompletionOption.ResponseHeadersRead ) )
                {
                    var statusCode = ( int ) response.StatusCode;

                    if ( statusCode == 301 || statusCode == 302 || statusCode == 303 || statusCode == 307 || statusCode == 308 )
                    {
                        if ( redirectCount >= MaxRedirects || response.Headers.Location == null )
                        {
                            return null;
                        }

                        uri = response.Headers.Location.IsAbsoluteUri
                            ? response.Headers.Location
                            : new Uri( uri, response.Headers.Location );

                        continue;
                    }

                    if ( response.StatusCode != HttpStatusCode.OK || response.Content.Headers.ContentLength > MaxImageBytes )
                    {
                        return null;
                    }

                    return await ReadLimitedContentAsync( response.Content );
                }
            }
        }

        /// <summary>
        /// Determines whether the URI uses HTTPS on the default port and every
        /// address its host resolves to is public.
        /// </summary>
        /// <param name="uri">The URI to check.</param>
        /// <returns><c>true</c> if the URI may be requested; otherwise <c>false</c>.</returns>
        private static bool IsAllowedUri( Uri uri )
        {
            if ( uri.Scheme != Uri.UriSchemeHttps || uri.Port != 443 || !string.IsNullOrEmpty( uri.UserInfo ) )
            {
                return false;
            }

            var addresses = IPAddress.TryParse( uri.DnsSafeHost, out var literalAddress )
                ? new[] { literalAddress }
                : Dns.GetHostAddresses( uri.DnsSafeHost );

            return addresses.Length > 0 && addresses.All( IsPublicAddress );
        }

        /// <summary>
        /// Determines whether the address is routable on the public internet.
        /// </summary>
        /// <param name="ip">The address to check.</param>
        /// <returns><c>true</c> if the address is public; otherwise <c>false</c>.</returns>
        internal static bool IsPublicAddress( IPAddress ip )
        {
            if ( ip.IsIPv4MappedToIPv6 )
            {
                ip = ip.MapToIPv4();
            }

            if ( IPAddress.IsLoopback( ip )
                || ip.Equals( IPAddress.Any )
                || ip.Equals( IPAddress.IPv6Any )
                || ip.Equals( IPAddress.None ) )
            {
                return false;
            }

            var b = ip.GetAddressBytes();

            if ( ip.AddressFamily == AddressFamily.InterNetworkV6 )
            {
                // fc00::/7 is unique local.
                return !ip.IsIPv6LinkLocal
                    && !ip.IsIPv6SiteLocal
                    && !ip.IsIPv6Multicast
                    && ( b[0] & 0xFE ) != 0xFC;
            }

            if ( ip.AddressFamily != AddressFamily.InterNetwork )
            {
                return false;
            }

            return !( b[0] == 0
                || b[0] == 10
                || ( b[0] == 100 && b[1] >= 64 && b[1] <= 127 )
                || b[0] == 127
                || ( b[0] == 169 && b[1] == 254 )
                || ( b[0] == 172 && b[1] >= 16 && b[1] <= 31 )
                || ( b[0] == 192 && b[1] == 0 && b[2] == 0 )
                || ( b[0] == 192 && b[1] == 168 )
                || ( b[0] == 198 && ( b[1] == 18 || b[1] == 19 ) )
                || b[0] >= 224 );
        }

        /// <summary>
        /// Reads the response body, giving up once it exceeds the size limit.
        /// </summary>
        /// <param name="content">The response content.</param>
        /// <returns>The body bytes, or <c>null</c> if the body is too large.</returns>
        private static async Task<byte[]> ReadLimitedContentAsync( HttpContent content )
        {
            using ( var stream = await content.ReadAsStreamAsync() )
            using ( var memoryStream = new MemoryStream() )
            {
                var buffer = new byte[81920];
                int read;

                while ( ( read = await stream.ReadAsync( buffer, 0, buffer.Length ) ) > 0 )
                {
                    memoryStream.Write( buffer, 0, read );

                    if ( memoryStream.Length > MaxImageBytes )
                    {
                        return null;
                    }
                }

                return memoryStream.ToArray();
            }
        }
    }
}
