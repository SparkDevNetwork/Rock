//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net;
using System.Text;

namespace Rock.Net
{
    /// <summary>
    /// 
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