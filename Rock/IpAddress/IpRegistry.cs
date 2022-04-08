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

using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using RestSharp;
using Rock.Attribute;

namespace Rock.IpAddress
{
    /// <summary>
    /// IP detail service from IpRegistry
    /// </summary>
    [Description( "IP detail service from IpRegistry" )]
    [Export( typeof( IpAddressLookupComponent ) )]
    [ExportMetadata( "ComponentName", "IpRegistry" )]

    [TextField(
        "API Key",
        Description = "The IpRegistry API Key.",
        IsRequired = true,
        Order = 0,
        Key = AttributeKey.APIKey )]
    public class IpRegistry : IpAddressLookupComponent
    {
        #region Keys

        /// <summary>
        /// Keys to use for the attributes
        /// </summary>
        public static class AttributeKey
        {
            /// <summary>
            /// The API Key
            /// </summary>
            public const string APIKey = "APIKey";
        }

        #endregion Keys

        /// <summary>
        /// Gets all the IP Address result through IPRegistry
        /// </summary>
        public override List<LookupResult> Lookup( List<string> ipAddresses, out string resultMsg )
        {
            var lookupResults = new List<LookupResult>();
            resultMsg = string.Empty;
            var apiKey = GetAttributeValue( AttributeKey.APIKey );
            var client = new RestClient( string.Format( "https://api.ipregistry.co?key={0}", apiKey ) );
            var request = new RestRequest( Method.POST );
            request.RequestFormat = DataFormat.Json;
            request.AddHeader( "Accept", "application/json" );
            request.AddBody( ipAddresses.ToJson() );
            var response = client.Execute( request );

            if ( response.StatusCode == HttpStatusCode.OK )
            {
                var lookupResult = JsonConvert.DeserializeObject( response.Content, typeof( List<LookupResult> ) ) as List<LookupResult>;
                if ( lookupResult.Any() )
                {
          
                }
                else
                {
                    resultMsg = "No Match";
                }
            }
            else
            {
                resultMsg = response.StatusDescription;
            }

            return lookupResults;
        }
    }
}
