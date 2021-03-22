using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using RestSharp;

namespace Rock.Services
{
    /// <summary>
    /// Rock Update Service
    /// </summary>
    public class RockUpdateService
    {
        /// <summary>
        /// Gets the releases list from the rock server.
        /// </summary>
        /// <returns></returns>
        public List<RockRelease> GetReleasesList( RockReleaseProgram rockReleaseProgram, Version version )
        {
            var releaseUrl = "http://localhost:57822/api/RockUpdate/GetReleasesList";

            var request = new RestRequest( Method.GET );
            
            request.RequestFormat = DataFormat.Json;

            request.AddParameter( "rockInstanceId", Web.SystemSettings.GetRockInstanceId() );
            request.AddParameter( "releaseProgram", rockReleaseProgram.ToString().ToLower() );

            if ( version != null )
            {
                releaseUrl = "http://localhost:57822/api/RockUpdate/GetReleasesListSinceVersion";
                request.AddParameter( "sinceVersion", version.ToString() );
            }

            var client = new RestClient( releaseUrl );
            var response = client.Execute( request );

            if ( response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted )
            {
                return JsonConvert.DeserializeObject<List<RockRelease>>( response.Content );
            }

            return new List<RockRelease>();
        }

    }
}
