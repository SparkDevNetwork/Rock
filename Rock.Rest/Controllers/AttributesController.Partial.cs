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
using System.Net;
using System.Web.Http;

using Rock.Model;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Attributes REST API
    /// </summary>
    public partial class AttributesController : IHasCustomRoutes
    {
        /// <summary>
        /// Add Custom route for flushing cached attributes
        /// </summary>
        /// <param name="routes"></param>
        public void AddRoutes( System.Web.Routing.RouteCollection routes )
        {
            routes.MapHttpRoute(
                name: "AttributeFlush",
                routeTemplate: "api/attributes/flush/{id}",
                defaults: new
                {
                    controller = "attributes",
                    action = "flush",
                    id = System.Web.Http.RouteParameter.Optional
                } );
        }

        /// <summary>
        /// Flushes an attributes from cache.
        /// </summary>
        [HttpPut]
        public void Flush( int id )
        {
            Rock.Web.Cache.AttributeCache.Flush( id );
        }

        /// <summary>
        /// Flushes all global attributes from cache.
        /// </summary>
        [HttpPut]
        public void Flush()
        {
            Rock.Web.Cache.GlobalAttributesCache.Flush();
        }
    }
}
