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
using System.Net;
using System.Text;

namespace Rock.Net
{
    /// <summary>
    /// Used by 3rd Party Plugins
    /// </summary>
    public static class RockWebRequest
    {
        /// <summary>
        /// Gets the specified request URI string.
        /// </summary>
        /// <param name="requestUriString">The request URI string.</param>
        /// <returns></returns>
        public static RockWebResponse Get( string requestUriString )
        {
            return Send( requestUriString, "GET", null, null );
        }

        /// <summary>
        /// Sends the specified request URI string.
        /// </summary>
        /// <param name="requestUriString">The request URI string.</param>
        /// <param name="method">The method.</param>
        /// <param name="queryStringData">The query string data.</param>
        /// <param name="formData">The form data.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public static RockWebResponse Send( string requestUriString, string method, Dictionary<string, string> queryStringData, Dictionary<string, string> formData )
        {
            string uri = requestUriString;

            if ( queryStringData != null )
            {
                string parms = queryStringData.Join( "&" );
                if ( parms.Trim() != string.Empty )
                    uri += ( uri.Contains( "?" ) ? "&" : "?" ) + parms;
            }

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create( uri );

            request.Method = method;

            if ( formData != null && formData.Count > 0 )
            {
                byte[] postData = ASCIIEncoding.ASCII.GetBytes( formData.Join( "&" ) );

                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = postData.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write( postData, 0, postData.Length );
                requestStream.Close();
            }

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                return new RockWebResponse( response.StatusCode, GetResponseString( response.GetResponseStream() ) );
            }
            catch ( WebException webException )
            {
                string message = GetResponseString( webException.Response.GetResponseStream() );
                throw new Exception( webException.Message + " - " + message );
            }
        }

        /// <summary>
        /// Gets the response string.
        /// </summary>
        /// <param name="responseStream">The response stream.</param>
        /// <returns></returns>
        static private string GetResponseString( Stream responseStream )
        {
            Stream receiveStream = responseStream;
            Encoding encode = System.Text.Encoding.GetEncoding( "utf-8" );
            StreamReader readStream = new StreamReader( receiveStream, encode );

            StringBuilder sb = new StringBuilder();
            Char[] read = new Char[8192];
            int count = 0;
            do
            {
                count = readStream.Read( read, 0, 8192 );
                String str = new String( read, 0, count );
                sb.Append( str );
            }
            while ( count > 0 );

            return sb.ToString();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RockWebResponse
    {
        /// <summary>
        /// Gets the HTTP status code.
        /// </summary>
        /// <value>
        /// The HTTP status code.
        /// </value>
        public HttpStatusCode HttpStatusCode { get; internal set; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockWebResponse" /> class.
        /// </summary>
        /// <param name="httpStatusCode">The HTTP status code.</param>
        /// <param name="message">The message.</param>
        internal RockWebResponse( HttpStatusCode httpStatusCode, string message )
        {
            HttpStatusCode = httpStatusCode;
            Message = message;
        }
    }
}