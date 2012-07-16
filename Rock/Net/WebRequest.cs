using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net;
using System.Text;

namespace Rock.Net
{
    public static class WebRequest
    {
        public static WebResponse Send( string requestUriString, string method, Dictionary<string, string> queryStringData, Dictionary<string, string> formData )
        {
            string uri = requestUriString;

            if ( queryStringData != null )
            {
                string parms = queryStringData.Join( "&" );
                if ( parms.Trim() != string.Empty )
                    uri += ( uri.Contains( "?" ) ? "&" : "?" ) + parms;
            }

            HttpWebRequest request = ( HttpWebRequest )HttpWebRequest.Create( uri );
            
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
                HttpWebResponse response = ( HttpWebResponse )request.GetResponse();
                return new WebResponse( response.StatusCode, GetResponseString( response.GetResponseStream() ) );
            }
            catch ( WebException webException )
            {
                string message = GetResponseString( webException.Response.GetResponseStream() );
                throw new Exception( webException.Message + " - " + message );
            }
        }

        static private string GetResponseString(Stream responseStream)
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

    public class WebResponse
    {
        public HttpStatusCode HttpStatusCode { get; internal set; }
        public string Message { get; internal set; }

        internal WebResponse( HttpStatusCode httpStatusCode, string message )
        {
            HttpStatusCode = httpStatusCode;
            Message = message;
        }
    }
}