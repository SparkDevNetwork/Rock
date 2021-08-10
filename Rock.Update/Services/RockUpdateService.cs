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

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using Newtonsoft.Json;
using RestSharp;
using Rock.Update.Interfaces;
using Rock.Update.Models;
using Rock.Web.Cache;

namespace Rock.Update.Services
{
    /// <summary>
    /// Rock Update Service
    /// </summary>
    public class RockUpdateService : IRockUpdateService
    {
        private const string GET_RELEASE_LIST_URL = "api/RockUpdate/GetReleasesList";
        private const string GET_RELEASE_LIST_SINCE_URL = "api/RockUpdate/GetReleasesListSinceVersion";
        private const string EARLY_ACCESS_URL = "api/RockUpdate/GetEarlyAccessStatus";
        private const string EARLY_ACCESS_REQUEST_URL = "earlyaccessissues?RockInstanceId=";

        private string BaseUrl
        {
            get => ConfigurationManager.AppSettings["RockStoreUrl"].EnsureTrailingForwardslash();
        }

        /// <summary>
        /// Gets the releases list from the rock server.
        /// </summary>
        /// <returns></returns>
        public List<RockRelease> GetReleasesList( Version version )
        {
            var request = new RestRequest( Method.GET );

            request.RequestFormat = DataFormat.Json;

            request.AddParameter( "rockInstanceId", Web.SystemSettings.GetRockInstanceId() );
            request.AddParameter( "releaseProgram", GetRockReleaseProgram().ToString().ToLower() );

            if ( version != null )
            {
                request.AddParameter( "sinceVersion", version.ToString() );
            }

            var client = new RestClient( BaseUrl + ( version != null ? GET_RELEASE_LIST_SINCE_URL : GET_RELEASE_LIST_URL ) );
            var response = client.Execute( request );

            if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
            {
                return JsonConvert.DeserializeObject<List<RockRelease>>( response.Content );
            }

            return new List<RockRelease>();
        }

        /// <summary>
        /// Checks the early access status of this organization.
        /// </summary>
        /// <returns></returns>
        public bool IsEarlyAccessInstance()
        {
            var client = new RestClient( BaseUrl + EARLY_ACCESS_URL );
            var request = new RestRequest( Method.GET );
            request.RequestFormat = DataFormat.Json;

            request.AddParameter( "rockInstanceId", Web.SystemSettings.GetRockInstanceId() );
            IRestResponse response = client.Execute( request );
            if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
            {
                return response.Content.AsBoolean();
            }

            return false;
        }

        /// <summary>
        /// Gets the rock early access request URL.
        /// </summary>
        /// <returns></returns>
        public string GetRockEarlyAccessRequestUrl()
        {
            return $"{BaseUrl}{EARLY_ACCESS_REQUEST_URL}{Web.SystemSettings.GetRockInstanceId()}";
        }

        /// <summary>
        /// Gets the rock release program.
        /// </summary>
        /// <returns></returns>
        public RockReleaseProgram GetRockReleaseProgram()
        {
            var releaseProgram = RockReleaseProgram.Production;

            var updateUrl = GlobalAttributesCache.Get().GetValue( "UpdateServerUrl" );
            if ( updateUrl.Contains( RockReleaseProgram.Alpha.ToString().ToLower() ) )
            {
                releaseProgram = RockReleaseProgram.Alpha;
            }
            else if ( updateUrl.Contains( RockReleaseProgram.Beta.ToString().ToLower() ) )
            {
                releaseProgram = RockReleaseProgram.Beta;
            }

            return releaseProgram;
        }
    }
}
