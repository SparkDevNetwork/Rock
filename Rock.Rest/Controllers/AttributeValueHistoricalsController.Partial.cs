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
using System.Net;
using System.Web.Http;
using System.Web.Http.OData;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// AttributeValueHistoricals REST API
    /// </summary>
    public partial class AttributeValueHistoricalsController
    {
        /*
            10/15/2026 - MSE

            History rows carry the earlier values of history enabled attributes, so
            they follow the same read rule as api/AttributeValues: a row is only
            returned when its attribute value is readable by the person (see
            AttributeValuesController.WhereReadable). The check becomes one EXISTS
            subquery in SQL, and is skipped when nothing is hidden from the person.

            The method signatures must match ApiController exactly, because custom
            REST security rules are keyed by the action signature.

            Reason: Stop REST readers from reading secrets and restricted values through api/AttributeValueHistoricals.
        */

        /// <summary>
        /// Gets the attribute value history rows the current person is allowed to read.
        /// </summary>
        /// <returns>A queryable of readable attribute value history rows.</returns>
        [Authenticate, Secured]
        [EnableQuery]
        public override IQueryable<AttributeValueHistorical> Get()
        {
            return WhereReadable( base.Get() );
        }

        /// <summary>
        /// Gets the attribute value history row with the specified identifier when the current person is allowed to read it.
        /// </summary>
        /// <param name="id">The identifier of the attribute value history row.</param>
        /// <returns>The attribute value history row.</returns>
        /// <exception cref="HttpResponseException">Thrown with a 404 status when the row does not exist or the current person is not allowed to read it.</exception>
        [Authenticate, Secured]
        [ActionName( "GetById" )]
        public override AttributeValueHistorical GetById( int id )
        {
            var attributeValueHistorical = WhereReadable( base.Get() ).FirstOrDefault( h => h.Id == id );

            if ( attributeValueHistorical == null )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            return attributeValueHistorical;
        }

        /// <summary>
        /// Gets the attribute value history row with the specified key when the current person is allowed to read it.
        /// </summary>
        /// <param name="key">The identifier of the attribute value history row.</param>
        /// <returns>The attribute value history row.</returns>
        /// <exception cref="HttpResponseException">Thrown with a 404 status when the row does not exist or the current person is not allowed to read it.</exception>
        [Authenticate, Secured]
        [EnableQuery]
        public override AttributeValueHistorical Get( [FromODataUri] int key )
        {
            return GetById( key );
        }

        /// <summary>
        /// Gets the attribute value history rows that have a particular attribute value and that the current person is allowed to read.
        /// </summary>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="value">The attribute value to match.</param>
        /// <param name="caseSensitive">If set to <c>true</c>, the value is matched case sensitively.</param>
        /// <returns>A queryable of matching readable attribute value history rows.</returns>
        [Authenticate, Secured]
        [ActionName( "GetByAttributeValue" )]
        [EnableQuery]
        public override IQueryable<AttributeValueHistorical> GetByAttributeValue( [FromUri] int? attributeId = null, [FromUri] string attributeKey = null, [FromUri] string value = null, [FromUri] bool caseSensitive = false )
        {
            return WhereReadable( base.GetByAttributeValue( attributeId, attributeKey, value, caseSensitive ) );
        }

        /// <summary>
        /// Gets the attribute value history rows in the selected data view that the current person is allowed to read.
        /// </summary>
        /// <param name="id">The data view identifier.</param>
        /// <returns>A queryable of readable attribute value history rows.</returns>
        [Authenticate, Secured]
        [ActionName( "DataView" )]
        [EnableQuery]
        public override IQueryable<AttributeValueHistorical> GetDataView( int id )
        {
            return WhereReadable( base.GetDataView( id ) );
        }

        /// <summary>
        /// Determines if the attribute value history row is in the data view and the current person is allowed to read it.
        /// </summary>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <param name="entityId">The attribute value history row identifier.</param>
        /// <returns><c>true</c> if the row is in the data view and the current person is allowed to read it; otherwise <c>false</c>.</returns>
        [Authenticate, Secured]
        [ActionName( "InDataView" )]
        [EnableQuery]
        [HttpGet]
        public override bool InDataView( int dataViewId, int entityId )
        {
            return base.InDataView( dataViewId, entityId )
                && WhereReadable( base.Get() ).Any( h => h.Id == entityId );
        }

        /// <summary>
        /// Gets the attribute value history rows followed by the specified person that the current person is allowed to read.
        /// </summary>
        /// <param name="personId">The identifier of the person whose followed rows are returned.</param>
        /// <param name="personAliasId">The alias identifier of the person whose followed rows are returned, used when <paramref name="personId"/> is not provided.</param>
        /// <returns>A queryable of readable followed attribute value history rows.</returns>
        [Authenticate, Secured]
        [ActionName( "FollowedItems" )]
        [EnableQuery]
        public override IQueryable<AttributeValueHistorical> GetFollowedItems( int? personId = null, int? personAliasId = null )
        {
            return WhereReadable( base.GetFollowedItems( personId, personAliasId ) );
        }

        /// <summary>
        /// Limits the query to the history rows whose attribute value the current person is allowed to read.
        /// </summary>
        /// <param name="qry">The attribute value history query.</param>
        /// <returns>The query limited to readable history rows.</returns>
        private IQueryable<AttributeValueHistorical> WhereReadable( IQueryable<AttributeValueHistorical> qry )
        {
            var rockContext = ( RockContext ) Service.Context;
            var attributeValueQry = new AttributeValueService( rockContext ).Queryable();
            var readableAttributeValueQry = AttributeValuesController.WhereReadable( attributeValueQry, GetPerson(), rockContext, out var isFiltered );

            // Nothing is hidden from this person (for example Rock Administration),
            // so the subquery is skipped.
            if ( !isFiltered )
            {
                return qry;
            }

            var readableAttributeValueIds = readableAttributeValueQry.Select( v => v.Id );

            return qry.Where( h => readableAttributeValueIds.Contains( h.AttributeValueId ) );
        }
    }
}
