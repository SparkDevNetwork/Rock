using System;
using System.Net;
using System.Net.Http;
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
        /// <param name="getPath">The get path, for example "api/BinaryFileTypes</param>
        /// <returns></returns>
        public T GetData<T>( string getPath )
        {
            HttpClient httpClient = new HttpClient( new HttpClientHandler { CookieContainer = this.CookieContainer } );

            Uri requestUri = new Uri( rockBaseUri, getPath );
            HttpContent resultContent;
            T result = default( T );

            httpClient.GetAsync( requestUri ).ContinueWith( ( postTask ) =>
                {
                    resultContent = postTask.Result.Content;
                    resultContent.ReadAsAsync<T>().ContinueWith( ( readResult ) =>
                        {
                            result = readResult.Result;
                        } ).Wait();
                } ).Wait();

            return result;
        }

        /// <summary>
        /// Posts the data.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="postPath">The post path.</param>
        /// <param name="data">The data.</param>
        public void PostData<T>( string postPath, T data )
        {
            Uri requestUri = new Uri( rockBaseUri, postPath );

            HttpClient httpClient = new HttpClient( new HttpClientHandler { CookieContainer = this.CookieContainer } );
            httpClient.PostAsJsonAsync<T>( requestUri.ToString(), data ).ContinueWith( p =>
            {
                p.Result.EnsureSuccessStatusCode();
            } ).Wait();
        }
    }
}
