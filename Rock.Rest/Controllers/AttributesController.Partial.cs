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
    public partial class AttributesController
    {
        /// <summary>
        /// Flushes an attributes from cache. Usually no need to do this since attribute cache is managed automatically.
        /// </summary>
        [Authenticate, Secured]
        [HttpPut]
        [System.Web.Http.Route( "api/attributes/flush/{id}" )]
        [Rock.SystemGuid.RestActionGuid( "6DC23228-36F6-43B4-8D0E-33FC0CC49798" )]
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
        [Rock.SystemGuid.RestActionGuid( "039968E8-EE2D-4F06-8E36-773816114C98" )]
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
        public override void Put( int id, [FromBody] Model.Attribute value )
        {
            base.Put( id, value );
        }
    }
}