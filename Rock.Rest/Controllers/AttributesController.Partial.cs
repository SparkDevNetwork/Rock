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
using System.Web.Http;

using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Attributes REST API
    /// </summary>
    public partial class AttributesController
    {
        /// <summary>
        /// Flushes an attributes from cache.
        /// </summary>
        [Authenticate, Secured]
        [HttpPut]
        [System.Web.Http.Route( "api/attributes/flush/{id}" )]
        public void Flush( int id )
        {
            Rock.Web.Cache.AttributeCache.Flush( id );
        }

        /// <summary>
        /// Flushes all global attributes from cache.
        /// </summary>
        [Authenticate, Secured]
        [HttpPut]
        [System.Web.Http.Route( "api/attributes/flush" )]
        public void Flush()
        {
            Rock.Web.Cache.GlobalAttributesCache.Flush();
        }
    }
}
