//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

using Rock.Data;
using Rock.Security;

namespace Rock.Net
{
    /// <summary>
    /// from http://stackoverflow.com/questions/4740752/how-to-login-with-webclient-c-sharp
    /// and http://stackoverflow.com/questions/1777221/using-cookiecontainer-with-webclient-class
    /// Used by Apps.CheckScannerUtility
    /// </summary>
    public class RockRestClient : WebClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RockRestClient"/> class.
        /// </summary>
        public RockRestClient( string rockBaseUrl )
            : this( rockBaseUrl, new CookieContainer() )
        {
            // intentionally blank
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockRestClient"/> class.
        /// </summary>
        /// <param name="c">The c.</param>
        public RockRestClient( string rockBaseUrl, CookieContainer c )
        {
            this.CookieContainer = c;
            this.rockBaseUri = new Uri( rockBaseUrl );
        }

        /// <summary>
        /// Gets or sets the rock base URI.
        /// </summary>
        /// <value>
        /// The rock base URI.
        /// </value>
        public Uri rockBaseUri { get; set; }

        /// <summary>
        /// Gets or sets the cookie container.
        /// </summary>
        /// <value>
        /// The cookie container.
        /// </value>
        public CookieContainer CookieContainer { get; set; }

        /// <summary>
        /// Returns a <see cref="T:System.Net.WebRequest" /> object for the specified resource.
        /// </summary>
        /// <param name="address">A <see cref="T:System.Uri" /> that identifies the resource to request.</param>
        /// <returns>
        /// A new <see cref="T:System.Net.WebRequest" /> object for the specified resource.
        /// </returns>
        protected override WebRequest GetWebRequest( Uri address )
        {
            WebRequest request = base.GetWebRequest( address );

            var castRequest = request as HttpWebRequest;
            if ( castRequest != null )
            {
                castRequest.CookieContainer = this.CookieContainer;
            }

            return request;
        }

        /// <summary>
        /// Returns the <see cref="T:System.Net.WebResponse" /> for the specified <see cref="T:System.Net.WebRequest" /> using the specified <see cref="T:System.IAsyncResult" />.
        /// </summary>
        /// <param name="request">A <see cref="T:System.Net.WebRequest" /> that is used to obtain the response.</param>
        /// <param name="result">An <see cref="T:System.IAsyncResult" /> object obtained from a previous call to <see cref="M:System.Net.WebRequest.BeginGetResponse(System.AsyncCallback,System.Object)" /> .</param>
        /// <returns>
        /// A <see cref="T:System.Net.WebResponse" /> containing the response for the specified <see cref="T:System.Net.WebRequest" />.
        /// </returns>
        protected override WebResponse GetWebResponse( WebRequest request, IAsyncResult result )
        {
            WebResponse response = base.GetWebResponse( request, result );
            ReadCookies( response );
            return response;
        }

        /// <summary>
        /// Returns the <see cref="T:System.Net.WebResponse" /> for the specified <see cref="T:System.Net.WebRequest" />.
        /// </summary>
        /// <param name="request">A <see cref="T:System.Net.WebRequest" /> that is used to obtain the response.</param>
        /// <returns>
        /// A <see cref="T:System.Net.WebResponse" /> containing the response for the specified <see cref="T:System.Net.WebRequest" />.
        /// </returns>
        protected override WebResponse GetWebResponse( WebRequest request )
        {
            WebResponse response = base.GetWebResponse( request );
            ReadCookies( response );
            return response;
        }

        /// <summary>
        /// Reads the cookies.
        /// </summary>
        /// <param name="r">The r.</param>
        private void ReadCookies( WebResponse r )
        {
            var response = r as HttpWebResponse;
            if ( response != null )
            {
                CookieCollection cookies = response.Cookies;
                CookieContainer.Add( cookies );
            }
        }

        /// <summary>
        /// Logins the specified username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="rockBaseUrl">The rock base URL.</param>
        /// <param name="rockLoginUrl">The rock login URL.</param>
        public void Login( string username, string password, string rockLoginUrl = "api/auth/login" )
        {
            this.Headers[HttpRequestHeader.ContentType] = "application/json";
            LoginParameters loginParameters = new LoginParameters { Username = username, Password = password };
            this.UploadString( new Uri( rockBaseUri, rockLoginUrl ), loginParameters.ToJson() );
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="getPath">The get path, for example "api/BinaryFileTypes"</param>
        /// <param name="odataFilter">The odata filter.</param>
        /// <returns></returns>
        public T GetData<T>( string getPath, string odataFilter = null )
        {
            HttpClient httpClient = new HttpClient( new HttpClientHandler { CookieContainer = this.CookieContainer } );

            Uri requestUri;

            if ( !string.IsNullOrWhiteSpace( odataFilter ) )
            {
                string queryParam = "?$filter=" + odataFilter;
                requestUri = new Uri( rockBaseUri, getPath + queryParam );
            }
            else
            {
                requestUri = new Uri( rockBaseUri, getPath );
            }

            HttpContent resultContent;
            HttpError httpError = null;
            T result = default( T );
            

            httpClient.GetAsync( requestUri ).ContinueWith( ( postTask ) =>
                {
                    resultContent = postTask.Result.Content;

                    if ( postTask.Result.IsSuccessStatusCode )
                    {
                        resultContent.ReadAsAsync<T>().ContinueWith( ( readResult ) =>
                            {
                                result = readResult.Result;
                            } ).Wait();
                    }
                    else
                    {
                        resultContent.ReadAsStringAsync().ContinueWith( s =>
                        {
#if DEBUG
                            string debugResult = s.Result;
                            httpError = new HttpError( debugResult );
#else                            
                            // just get the simple error message, don't expose exception details to user
                            httpError = new HttpError( postTask.Result.ReasonPhrase );
#endif
                        } ).Wait();
                    }
                } ).Wait();

            if ( httpError != null )
            {
                throw new HttpErrorException( httpError );
            }

            return result;
        }

        /// <summary>
        /// Gets the data by GUID.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="getPath">The get path.</param>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public T GetDataByGuid<T>( string getPath, Guid guid ) where T : Rock.Data.IEntity
        {
            return GetData<List<T>>( getPath, string.Format( "Guid eq guid'{0}'", guid ) ).FirstOrDefault();
        }

        /// <summary>
        /// Gets the data by enum.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="getPath">The get path.</param>
        /// <param name="enumFieldName">Name of the enum field.</param>
        /// <param name="enumVal">The enum val.</param>
        /// <returns></returns>
        public T GetDataByEnum<T>( string getPath, string enumFieldName, Enum enumVal )
        {
            return GetData<T>( getPath, string.Format( "{0} eq '{1}'", enumFieldName, enumVal ) );
        }

        /// <summary>
        /// Posts the data 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="postPath">The post path.</param>
        /// <param name="data">The data.</param>
        public void PostData<T>( string postPath, T data ) where T : IEntity
        {
            Uri requestUri = new Uri( rockBaseUri, postPath );

            HttpClient httpClient = new HttpClient( new HttpClientHandler { CookieContainer = this.CookieContainer } );
            HttpError httpError = null;
            HttpResponseMessage httpMessage = null;

            Action<Task<HttpResponseMessage>> handleContinue = new Action<Task<HttpResponseMessage>>( p =>
            {
                p.Result.Content.ReadAsAsync<HttpError>().ContinueWith( c =>
                {
                    httpError = c.Result;
                } ).Wait();

                p.Result.Content.ReadAsStringAsync().ContinueWith( c =>
                {
                    var contentResult = c.Result;
                } ).Wait();

                httpMessage = p.Result;
            } );

            if ( data.Id.Equals( 0 ) )
            {
                // POST is for INSERTs
                httpClient.PostAsJsonAsync<T>( requestUri.ToString(), data ).ContinueWith( handleContinue ).Wait();
            }
            else
            {
                // PUT is for UPDATEs
                Uri putRequestUri = new Uri( requestUri, string.Format( "{0}", data.Id ) );

                httpClient.PutAsJsonAsync<T>( putRequestUri.ToString(), data ).ContinueWith( handleContinue ).Wait();
            }

            if ( httpError != null )
            {
                throw new HttpErrorException( httpError );
            }

            if ( httpMessage != null )
            {
                httpMessage.EnsureSuccessStatusCode();
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class HttpErrorException : Exception
    {
        /// <summary>
        /// Gets or sets the _message.
        /// </summary>
        /// <value>
        /// The _message.
        /// </value>
        private string _message { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpErrorException"/> class.
        /// </summary>
        /// <param name="httpError">The HTTP error.</param>
        public HttpErrorException( HttpError httpError )
            : base()
        {
            _message = string.Empty;

            foreach ( var error in httpError )
            {
                _message += error.Value.ToString() + Environment.NewLine;
            }
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <returns>The error message that explains the reason for the exception, or an empty string("").</returns>
        public override string Message
        {
            get
            {
                return _message;
            }
        }
    }
}
