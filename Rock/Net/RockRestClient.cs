// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;

namespace Rock.Net
{
    /// <summary>
    /// from http://stackoverflow.com/questions/4740752/how-to-login-with-webclient-c-sharp
    /// and http://stackoverflow.com/questions/1777221/using-cookiecontainer-with-webclient-class
    /// Used by Apps.CheckScannerUtility
    /// </summary>
    [System.ComponentModel.DesignerCategory( "Code" )]
    [Obsolete("The RestSharp library should be used instead of this class.  This class will eventually be removed.")]
    public class RockRestClient : WebClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RockRestClient"/> class.
        /// </summary>
        [Obsolete( "The RestSharp library should be used instead of this class.  This class will eventually be removed." )]
        public RockRestClient( string rockBaseUrl )
            : this( rockBaseUrl, new CookieContainer() )
        {
            // intentionally blank
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockRestClient" /> class.
        /// </summary>
        /// <param name="rockBaseUrl">The rock base URL.</param>
        /// <param name="c">The c.</param>
        [Obsolete( "The RestSharp library should be used instead of this class.  This class will eventually be removed." )]
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
        /// <param name="rockLoginUrl">The rock login URL.</param>
        public void Login( string username, string password, string rockLoginUrl = "api/auth/login" )
        {
            this.Headers[HttpRequestHeader.ContentType] = "application/json";
            var loginParameters = new { Username = username, Password = password };
            this.UploadString( new Uri( rockBaseUri, rockLoginUrl ), loginParameters.ToJson() );
        }

        /// <summary>
        /// 
        /// </summary>
        public class IdResult
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }
        }

        /// <summary>
        /// Gets the identifier from unique identifier.
        /// </summary>
        /// <param name="getPath">The get path.</param>
        /// <param name="guid">The unique identifier.</param>
        /// <returns></returns>
        /// <exception cref="Rock.Net.HttpErrorException"></exception>
        public int GetIdFromGuid( string getPath, Guid guid )
        {
            HttpClient httpClient = new HttpClient( new HttpClientHandler { CookieContainer = this.CookieContainer } );

            Uri requestUri = new Uri( rockBaseUri, string.Format( "{0}?$filter=Guid eq guid'{1}'&$select=Id", getPath, guid ) );

            HttpContent resultContent;
            HttpError httpError = null;
            int result = 0;

            httpClient.GetAsync( requestUri ).ContinueWith( ( postTask ) =>
            {
                resultContent = postTask.Result.Content;

                if ( postTask.Result.IsSuccessStatusCode )
                {
                    resultContent.ReadAsStringAsync().ContinueWith( s =>
                    {
                        var stringResult = s.Result;
                        var oResult = JsonConvert.DeserializeObject<List<IdResult>>( s.Result ).FirstOrDefault();
                        result = oResult.Id;
                    } ).Wait();
                }
                else
                {
                    resultContent.ReadAsStringAsync().ContinueWith( s =>
                    {
                        httpError = GetHttpError( requestUri, httpError, postTask, s );
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
        /// Gets the HTTP error.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="httpError">The HTTP error.</param>
        /// <param name="postTask">The post task.</param>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        private static HttpError GetHttpError( Uri requestUri, HttpError httpError, Task<HttpResponseMessage> postTask, Task<string> s )
        {
            string errorMessage = requestUri != null ? requestUri.AbsolutePath : string.Empty;
            errorMessage += "\n\n" + postTask.Result.ReasonPhrase;

#if DEBUG
            errorMessage += "\n\n" + s.Result;
#endif
            httpError = new HttpError( errorMessage );
            return httpError;
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
                            httpError = GetHttpError( requestUri, httpError, postTask, s );
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
        /// Posts the data. Use this for Adding a new record
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="postPath">The post path.</param>
        /// <param name="data">The data.</param>
        public void PostData<T>( string postPath, T data )
        {
            PostPutData( postPath, data, HttpMethod.Post );
        }

        /// <summary>
        /// Puts the data. Use this for Updating an existing record
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="postPath">The post path.</param>
        /// <param name="data">The data.</param>
        public void PutData<T>( string postPath, T data )
        {
            PostPutData( postPath, data, HttpMethod.Put );
        }

        /// <summary>
        /// Posts or Puts the data depending on httpMethod
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="postPath">The post path.</param>
        /// <param name="data">The data.</param>
        /// <param name="httpMethod">The HTTP method.</param>
        /// <exception cref="Rock.Net.HttpErrorException"></exception>
        private void PostPutData<T>( string postPath, T data, HttpMethod httpMethod )
        {
            Uri requestUri = new Uri( rockBaseUri, postPath );

            HttpClient httpClient = new HttpClient( new HttpClientHandler { CookieContainer = this.CookieContainer } );
            HttpError httpError = null;
            HttpResponseMessage httpMessage = null;

            Action<Task<HttpResponseMessage>> handleContinue = new Action<Task<HttpResponseMessage>>( postTask =>
            {
                if ( postTask.Result.IsSuccessStatusCode )
                {
                    postTask.Result.Content.ReadAsAsync<HttpError>().ContinueWith( c =>
                    {
                        httpError = c.Result;
                    } ).Wait();

                    postTask.Result.Content.ReadAsStringAsync().ContinueWith( c =>
                    {
                        var contentResult = c.Result;
                    } ).Wait();

                    httpMessage = postTask.Result;
                }
                else
                {
                    postTask.Result.Content.ReadAsStringAsync().ContinueWith( s =>
                    {
                        httpError = GetHttpError( requestUri, httpError, postTask, s );
                    } ).Wait();
                }
            } );

            if ( httpMethod == HttpMethod.Post )
            {
                // POST is for INSERTs
                httpClient.PostAsJsonAsync<T>( requestUri.ToString(), data ).ContinueWith( handleContinue ).Wait();
            }
            else
            {
                int? id = data.GetPropertyValue( "Id" ) as int?;
                if ( id != null )
                {
                    // PUT is for UPDATEs
                    Uri putRequestUri = new Uri( requestUri, string.Format( "{0}", id ) );

                    httpClient.PutAsJsonAsync<T>( putRequestUri.ToString(), data ).ContinueWith( handleContinue ).Wait();
                }
                else
                {
                    throw new Exception( "Data must be have an 'Id' property to do PUTS" );
                }
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

        /// <summary>
        /// Posts or Puts the data depending on httpMethod
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="postPath">The post path.</param>
        /// <param name="data">The data.</param>
        /// <exception cref="Rock.Net.HttpErrorException"></exception>
        public void PostNonIEntityData<T>( string postPath, T data )
        {
            Uri requestUri = new Uri( rockBaseUri, postPath );

            HttpClient httpClient = new HttpClient( new HttpClientHandler { CookieContainer = this.CookieContainer } );
            HttpError httpError = null;
            HttpResponseMessage httpMessage = null;

            Action<Task<HttpResponseMessage>> handleContinue = new Action<Task<HttpResponseMessage>>( postTask =>
            {
                if ( postTask.Result.IsSuccessStatusCode )
                {
                    postTask.Result.Content.ReadAsAsync<HttpError>().ContinueWith( c =>
                    {
                        httpError = c.Result;
                    } ).Wait();

                    postTask.Result.Content.ReadAsStringAsync().ContinueWith( c =>
                    {
                        var contentResult = c.Result;
                    } ).Wait();

                    httpMessage = postTask.Result;
                }
                else
                {
                    postTask.Result.Content.ReadAsStringAsync().ContinueWith( s =>
                    {
                        httpError = GetHttpError( requestUri, httpError, postTask, s );
                    } ).Wait();
                }
            } );

            // POST is for INSERTs
            httpClient.PostAsJsonAsync<T>( requestUri.ToString(), data ).ContinueWith( handleContinue ).Wait();

            if ( httpError != null )
            {
                throw new HttpErrorException( httpError );
            }

            if ( httpMessage != null )
            {
                httpMessage.EnsureSuccessStatusCode();
            }
        }

        /// <summary>
        /// Posts the data with result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="postPath">The post path.</param>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        /// <exception cref="Rock.Net.HttpErrorException"></exception>
        public R PostDataWithResult<T, R>( string postPath, T data ) where R : new()
        {
            Uri requestUri = new Uri( rockBaseUri, postPath );

            HttpClient httpClient = new HttpClient( new HttpClientHandler { CookieContainer = this.CookieContainer } );
            HttpResponseMessage httpMessage = null;
            string contentResult = null;

            Action<Task<HttpResponseMessage>> handleContinue = new Action<Task<HttpResponseMessage>>( p =>
            {
                p.Result.Content.ReadAsStringAsync().ContinueWith( c =>
                {
                    contentResult = c.Result;
                } ).Wait();

                httpMessage = p.Result;
            } );

            httpClient.PostAsJsonAsync<T>( requestUri.ToString(), data ).ContinueWith( handleContinue ).Wait();

            if ( httpMessage != null )
            {
                httpMessage.EnsureSuccessStatusCode();
            }

            R result = JsonConvert.DeserializeObject<R>( contentResult );

            return result;
        }

        /// <summary>
        /// Gets the XML.
        /// </summary>
        /// <param name="getPath">The get path.</param>
        /// <param name="maxWaitMilliseconds">The maximum wait milliseconds.</param>
        /// <param name="odataFilter">The odata filter.</param>
        /// <returns></returns>
        public string GetXml( string getPath, int maxWaitMilliseconds = -1, string odataFilter = null )
        {
            HttpClient httpClient = new HttpClient( new HttpClientHandler { CookieContainer = this.CookieContainer } );
            httpClient.DefaultRequestHeaders.Accept.Add( new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue( "application/xml" ) );

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
            string result = null;

            try
            {
                httpClient.GetAsync( requestUri ).ContinueWith( ( postTask ) =>
                {
                    if ( postTask.Result.IsSuccessStatusCode )
                    {
                        resultContent = postTask.Result.Content;
                        resultContent.ReadAsStringAsync().ContinueWith( s =>
                        {
                            result = s.Result;
                        } ).Wait( maxWaitMilliseconds );
                    }
                    else
                    {
                        throw new HttpErrorException( new HttpError( postTask.Result.ReasonPhrase ) );
                    }
                } ).Wait();
            }
            catch ( AggregateException ex )
            {
                throw ex.Flatten();
            }

            return result;
        }

        /// <summary>
        /// Uploads the binary file returning the resulting binaryFile.Id
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileTypeGuid">The file type unique identifier.</param>
        /// <param name="fileData">The file data.</param>
        /// <param name="isTemporary">if set to <c>true</c> [is temporary].</param>
        /// <returns></returns>
        public int UploadBinaryFile( string fileName, Guid fileTypeGuid, byte[] fileData, bool isTemporary )
        {
            RestSharp.RestClient restClient = new RestSharp.RestClient( this.rockBaseUri );
            restClient.CookieContainer = this.CookieContainer;
            RestSharp.RestRequest request = new RestSharp.RestRequest( "FileUploader.ashx", RestSharp.Method.POST );
            request.AddQueryParameter( "isBinaryFile", "true" );
            request.AddQueryParameter( "fileTypeGuid", fileTypeGuid.ToString() );
            request.AddQueryParameter( "isTemporary", isTemporary.ToString() );
            request.AddFile( "file0", fileData, fileName );
            var response = restClient.Execute( request ).Content;

            dynamic responseObj = JsonConvert.DeserializeObject( response );
            return responseObj.Id;
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
