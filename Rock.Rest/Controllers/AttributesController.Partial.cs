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
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Attributes REST API
    /// </summary>
    [RockGuid( "b59a45fd-fb96-426f-b777-061a5331ac51" )]
    public partial class AttributesController
    {
        /// <summary>
        /// Flushes an attributes from cache. Usually no need to do this since attribute cache is managed automatically.
        /// </summary>
        [Authenticate, Secured]
        [HttpPut]
        [System.Web.Http.Route( "api/attributes/flush/{id}" )]
        [RockGuid( "6dc23228-36f6-43b4-8d0e-33fc0cc49798" )]
        public void Flush( int id )
        {
            Rock.Web.Cache.AttributeCache.Remove( id );
            Rock.Web.Cache.AttributeCache.Get( id );
        }

        /// <summary>
        /// Flushes all global attributes from cache. Usually no need to do this since global attribute cache is managed automatically.
        /// </summary>
        [Authenticate, Secured]
        [HttpPut]
        [System.Web.Http.Route( "api/attributes/flush" )]
        [RockGuid( "039968e8-ee2d-4f06-8e36-773816114c98" )]
        public void Flush()
        {
            GlobalAttributesCache.Remove();
        }

        /// <summary>
        /// Posts the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [RockGuid( "fa7847e1-f1f7-4537-8804-e1b7afecc2d2" )]
        public override HttpResponseMessage Post( [FromBody] Model.Attribute value )
        {
            // if any Categories are included in the Post, we'll need to fetch them from the database so that that EF inserts them into AttributeCategory correct
            if ( value.Categories != null && value.Categories.Any() )
            {
                var fetchedCategories = new CategoryService( Service.Context as Rock.Data.RockContext ).GetByIds( value.Categories.Select( a => a.Id ).ToList() ).ToList();
                value.Categories.Clear();
                foreach ( var cat in fetchedCategories )
                {
                    value.Categories.Add( cat );
                }

                // Since changes to Categories isn't tracked by ChangeTracker, set the ModifiedDateTime just in case Categories changed
                value.ModifiedDateTime = RockDateTime.Now;
            }

            var result = base.Post( value );

            return result;
        }

        /// <summary>
        /// Deletes the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        [Authenticate, Secured]
        [RockGuid( "728e6445-6bd7-444a-9af2-b18d619d3abe" )]
        public override void Delete( int id )
        {
            base.Delete( id );
        }

        /// <summary>
        /// Puts the specified identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="value">The value.</param>
        [Authenticate, Secured]
        [RockGuid( "e7a07174-b49f-43ec-90dc-1c471ac8a6a0" )]
        public override void Put( int id, [FromBody] Model.Attribute value )
        {
            base.Put( id, value );
        }
    }
}
