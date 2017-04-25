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
using System.Linq;
using System.Web.Http;

using Rock.Model;
using Rock.Rest.Filters;
using System.Net.Http;

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

        /// <summary>
        /// Posts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        public override HttpResponseMessage Post( [FromBody] Model.Attribute value )
        {
            // if any Categories are included in the Post, we'll need to fetch them from the database so that that EF inserts them into AttributeCategory correct
            if ( value.Categories != null && value.Categories.Any())
            {
                var fetchedCategories = new CategoryService( Service.Context as Rock.Data.RockContext ).GetByIds( value.Categories.Select( a => a.Id ).ToList() ).ToList();
                value.Categories.Clear();
                foreach ( var cat in fetchedCategories )
                {
                    value.Categories.Add( cat );
                }
            }

            return base.Post( value );
        }
    }
}
