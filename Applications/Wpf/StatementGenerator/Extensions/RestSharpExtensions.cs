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
using System.Net;

using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace Rock.Apps.StatementGenerator
{
    /// <summary>
    /// 
    /// </summary>
    public static class RestSharpExtensions
    {
        /// <summary>
        /// Logs in to rock.
        /// </summary>
        /// <param name="restClient">The rest client.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        /// <exception cref="Exception">
        /// Invalid Login
        /// </exception>
        public static RestClient LoginToRock( this RestClient restClient, string userName, string password )
        {
            restClient.UseNewtonsoftJson();

            var rockLoginRequest = new RestRequest( "api/auth/login" );
            rockLoginRequest.Method = Method.POST;
            var loginParameters = new { Username = userName, Password = password };
            rockLoginRequest.AddJsonBody( loginParameters );

            restClient.CookieContainer = new System.Net.CookieContainer();
            var rockLoginResponse = restClient.Execute( rockLoginRequest );

            if ( rockLoginResponse.StatusCode.Equals( HttpStatusCode.Unauthorized ) )
            {
                throw new Exception( "Invalid Login", rockLoginResponse.ErrorException );
            }

            if ( rockLoginResponse.StatusCode != HttpStatusCode.NoContent && rockLoginResponse.StatusCode != HttpStatusCode.OK )
            {
                string message;
                if ( rockLoginResponse.ErrorException != null )
                {
                    message = rockLoginResponse.ErrorException.Message;
                    if ( rockLoginResponse.ErrorException.InnerException != null )
                    {
                        message += "\n" + rockLoginResponse.ErrorException.InnerException.Message;
                    }
                }
                else
                {
                    message = $"Error: { rockLoginResponse.StatusCode}";
                }

                throw new Exception( message, rockLoginResponse.ErrorException );
            }

            return restClient;
        }
    }
}
