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
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

using Rock.CheckIn;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Rest.ApiControllerBase" />
    public class CheckinController : ApiControllerBase
    {
        /// <summary>
        /// Gets the configuration status of a checkin device
        /// </summary>
        /// <param name="localDeviceConfiguration">The local device configuration.</param>
        /// <returns></returns>
        /// <exception cref="HttpResponseException"></exception>
        [HttpPost]
        [System.Web.Http.Route( "api/checkin/configuration/status" )]
        public LocalDeviceConfigurationStatus GetConfigurationStatus( LocalDeviceConfiguration localDeviceConfiguration )
        {
            if ( localDeviceConfiguration?.CurrentKioskId == null || localDeviceConfiguration?.CurrentCheckinTypeId == null )
            {
                var response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent( "LocalDeviceConfiguration with a valid KioskId and Checkin Type  is required" )
                };

                throw new HttpResponseException( response );
            }

            LocalDeviceConfigurationStatus localDeviceConfigurationStatus = CheckinConfigurationHelper.GetLocalDeviceConfigurationStatus( localDeviceConfiguration, HttpContext.Current.Request );

            return localDeviceConfigurationStatus;
        }
    }
}
