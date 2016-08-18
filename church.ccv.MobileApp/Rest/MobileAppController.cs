// <copyright>
// Copyright by the Spark Development Network
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
using System.IO;
using System.Net.Http;
using System.Text;
using Rock.Model;
using Newtonsoft.Json;
using System.Net;
using church.ccv.MobileApp;
using church.ccv.MobileApp.Models;
using System.Web.Http;
using System.Web.Routing;
using Rock.Rest.Filters;

namespace chuch.ccv.MobileApp.Rest
{
    public class MobileAppController : Rock.Rest.ApiControllerBase
    {
        [System.Web.Http.HttpGet]
        [System.Web.Http.Route( "api/MobileApp/LaunchData" )]
        public HttpResponseMessage GetLaunchData(  )
        {
            LaunchData launchData = MobileAppUtil.GetLaunchData( );

            StringContent restContent = new StringContent( JsonConvert.SerializeObject( launchData ), Encoding.UTF8, "application/json" );
            return new HttpResponseMessage()
            {
                Content = restContent
            };
        }
        
        [System.Web.Http.HttpPost]
        [System.Web.Http.Route( "api/MobileApp/GroupRegistration" )]
        public HttpResponseMessage GroupRegistration( [FromBody] GroupRegModel regModel )
        {
            bool success = MobileAppUtil.AddPersonToGroup( regModel );
            
            return new HttpResponseMessage( success == true ? HttpStatusCode.OK : HttpStatusCode.NotFound );
        }
        
        // Inherit StringWriter so we can set the encoding, which is protected
        public sealed class StringWriterWithEncoding : StringWriter
        {
            private readonly Encoding encoding;

            public StringWriterWithEncoding (Encoding encoding)
            {
                this.encoding = encoding;
            }

            public override Encoding Encoding
            {
                get { return encoding; }
            }
        }
    }
}
