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
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.OData;

using Rock.Data;
using Rock.Model;
using Rock.Rest.Filters;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// AttributeValues REST API
    /// </summary>
    public partial class AttributeValuesController
    {
        /*
            10/15/2026 - MSE

            The base read actions returned every attribute value with no attribute
            security check, including component settings (payment gateway credentials,
            the SMTP password, provider keys) and person attributes an admin restricted.
            Every read action now only returns rows whose attribute the person can VIEW,
            and rows of sensitive attributes (password fields and component settings)
            only when the person can also EDIT the attribute, or for a financial gateway
            setting, EDIT on that gateway (the same rule as loadAttributes and the
            Gateway Detail block). Filtering the queryable
            before EnableQuery applies $filter, $select and $expand means no projection
            can reach a hidden row, and a hidden row looks the same as a missing one.

            The method signatures must match ApiController exactly, because custom
            REST security rules are keyed by the action signature.

            Reason: Stop REST readers from reading secrets and restricted values through api/AttributeValues.
        */

        /// <summary>
        /// Gets the attribute values the current person is allowed to read.
        /// </summary>
        /// <returns>A queryable of readable attribute values.</returns>
        [Authenticate, Secured]
        [EnableQuery]
        public override IQueryable<AttributeValue> Get()
        {
            return WhereReadable( base.Get(), GetPerson(), ( RockContext ) Service.Context );
        }

        /// <summary>
        /// Gets the attribute value with the specified identifier when the current person is allowed to read it.
        /// </summary>
        /// <param name="id">The identifier of the attribute value.</param>
        /// <returns>The attribute value.</returns>
        /// <exception cref="HttpResponseException">Thrown with a 404 status when the attribute value does not exist or the current person is not allowed to read it.</exception>
        [Authenticate, Secured]
        [ActionName( "GetById" )]
        public override AttributeValue GetById( int id )
        {
            var attributeValue = WhereReadable( base.Get(), GetPerson(), ( RockContext ) Service.Context ).FirstOrDefault( v => v.Id == id );

            if ( attributeValue == null )
            {
                throw new HttpResponseException( HttpStatusCode.NotFound );
            }

            return attributeValue;
        }

        /// <summary>
        /// Gets the attribute value with the specified key when the current person is allowed to read it.
        /// </summary>
        /// <param name="key">The identifier of the attribute value.</param>
        /// <returns>The attribute value.</returns>
        /// <exception cref="HttpResponseException">Thrown with a 404 status when the attribute value does not exist or the current person is not allowed to read it.</exception>
        [Authenticate, Secured]
        [EnableQuery]
        public override AttributeValue Get( [FromODataUri] int key )
        {
            return GetById( key );
        }

        /// <summary>
        /// Gets the attribute values that have a particular attribute value and that the current person is allowed to read.
        /// </summary>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <param name="attributeKey">The attribute key.</param>
        /// <param name="value">The attribute value to match.</param>
        /// <param name="caseSensitive">If set to <c>true</c>, the value is matched case sensitively.</param>
        /// <returns>A queryable of matching readable attribute values.</returns>
        [Authenticate, Secured]
        [ActionName( "GetByAttributeValue" )]
        [EnableQuery]
        public override IQueryable<AttributeValue> GetByAttributeValue( [FromUri] int? attributeId = null, [FromUri] string attributeKey = null, [FromUri] string value = null, [FromUri] bool caseSensitive = false )
        {
            return WhereReadable( base.GetByAttributeValue( attributeId, attributeKey, value, caseSensitive ), GetPerson(), ( RockContext ) Service.Context );
        }

        /// <summary>
        /// Gets the attribute values in the selected data view that the current person is allowed to read.
        /// </summary>
        /// <param name="id">The data view identifier.</param>
        /// <returns>A queryable of readable attribute values.</returns>
        [Authenticate, Secured]
        [ActionName( "DataView" )]
        [EnableQuery]
        public override IQueryable<AttributeValue> GetDataView( int id )
        {
            return WhereReadable( base.GetDataView( id ), GetPerson(), ( RockContext ) Service.Context );
        }

        /// <summary>
        /// Determines if the attribute value is in the data view and the current person is allowed to read it.
        /// </summary>
        /// <param name="dataViewId">The data view identifier.</param>
        /// <param name="entityId">The attribute value identifier.</param>
        /// <returns><c>true</c> if the attribute value is in the data view and the current person is allowed to read it; otherwise <c>false</c>.</returns>
        [Authenticate, Secured]
        [ActionName( "InDataView" )]
        [EnableQuery]
        [HttpGet]
        public override bool InDataView( int dataViewId, int entityId )
        {
            return base.InDataView( dataViewId, entityId )
                && WhereReadable( base.Get(), GetPerson(), ( RockContext ) Service.Context ).Any( v => v.Id == entityId );
        }

        /// <summary>
        /// Gets the attribute values followed by the specified person that the current person is allowed to read.
        /// </summary>
        /// <param name="personId">The identifier of the person whose followed attribute values are returned.</param>
        /// <param name="personAliasId">The alias identifier of the person whose followed attribute values are returned, used when <paramref name="personId"/> is not provided.</param>
        /// <returns>A queryable of readable followed attribute values.</returns>
        [Authenticate, Secured]
        [ActionName( "FollowedItems" )]
        [EnableQuery]
        public override IQueryable<AttributeValue> GetFollowedItems( int? personId = null, int? personAliasId = null )
        {
            return WhereReadable( base.GetFollowedItems( personId, personAliasId ), GetPerson(), ( RockContext ) Service.Context );
        }

        /// <summary>
        /// Limits the query to the attribute values the person is allowed to read.
        /// A value is readable when the person has VIEW on its attribute and, for a
        /// sensitive attribute (see <see cref="SensitiveAttributeHelper.IsSensitiveAttribute(AttributeCache)"/>),
        /// EDIT on its attribute or, for a financial gateway setting, EDIT on the gateway.
        /// </summary>
        /// <param name="qry">The attribute value query.</param>
        /// <param name="person">The person reading the values, or <c>null</c> for an anonymous or app key caller.</param>
        /// <param name="rockContext">The context used to load the financial gateways when needed.</param>
        /// <returns>The query limited to readable attribute values.</returns>
        internal static IQueryable<AttributeValue> WhereReadable( IQueryable<AttributeValue> qry, Person person, RockContext rockContext )
        {
            return WhereReadable( qry, person, rockContext, out _ );
        }

        /// <summary>
        /// Limits the query to the attribute values the person is allowed to read.
        /// See <see cref="WhereReadable(IQueryable{AttributeValue}, Person, RockContext)"/>.
        /// </summary>
        /// <param name="qry">The attribute value query.</param>
        /// <param name="person">The person reading the values, or <c>null</c> for an anonymous or app key caller.</param>
        /// <param name="rockContext">The context used to load the financial gateways when needed.</param>
        /// <param name="isFiltered">Set to <c>true</c> when some values are hidden from the person, or <c>false</c> when the query is returned unchanged.</param>
        /// <returns>The query limited to readable attribute values.</returns>
        internal static IQueryable<AttributeValue> WhereReadable( IQueryable<AttributeValue> qry, Person person, RockContext rockContext, out bool isFiltered )
        {
            var gatewayEntityTypeId = EntityTypeCache.GetId<FinancialGateway>();
            var accessByAttributeId = new Dictionary<int, AttributeValueAccess>();

            // Read the attribute cache once so an attribute created during this
            // request cannot appear in the grouping below without an access entry.
            var attributes = AttributeCache.All();

            foreach ( var attribute in attributes )
            {
                accessByAttributeId[attribute.Id] = GetAccess( attribute, person, gatewayEntityTypeId );
            }

            // A gateway setting the person cannot edit is still readable on the
            // gateways the person can edit. Those gateways are loaded once, and
            // only when such a setting exists, instead of being checked per row.
            var gatewayEditorAttributeIds = accessByAttributeId
                .Where( a => a.Value == AttributeValueAccess.GatewayEditors )
                .Select( a => a.Key )
                .ToList();
            var editableGatewayIds = new List<int>();

            if ( gatewayEditorAttributeIds.Any() )
            {
                editableGatewayIds = new FinancialGatewayService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .ToList()
                    .Where( g => g.IsAuthorized( Rock.Security.Authorization.EDIT, person ) )
                    .Select( g => g.Id )
                    .ToList();

                if ( !editableGatewayIds.Any() )
                {
                    foreach ( var attributeId in gatewayEditorAttributeIds )
                    {
                        accessByAttributeId[attributeId] = AttributeValueAccess.Hidden;
                    }

                    gatewayEditorAttributeIds.Clear();
                }
            }

            var hiddenAttributeIds = new List<int>();
            var hiddenEntityTypeIds = new List<int>();

            // Group by the owning entity type so a type whose attributes are all
            // hidden (for example a component type full of settings) becomes one
            // entity type id in the SQL instead of one id per attribute.
            foreach ( var entityTypeAttributes in attributes.GroupBy( a => a.EntityTypeId ) )
            {
                var entityTypeAttributeList = entityTypeAttributes.ToList();
                var hiddenIds = entityTypeAttributeList
                    .Where( a => accessByAttributeId[a.Id] == AttributeValueAccess.Hidden )
                    .Select( a => a.Id )
                    .ToList();

                if ( entityTypeAttributes.Key.HasValue && hiddenIds.Count > 1 && hiddenIds.Count == entityTypeAttributeList.Count )
                {
                    hiddenEntityTypeIds.Add( entityTypeAttributes.Key.Value );
                }
                else
                {
                    hiddenAttributeIds.AddRange( hiddenIds );
                }
            }

            if ( hiddenAttributeIds.Any() )
            {
                qry = qry.Where( v => !hiddenAttributeIds.Contains( v.AttributeId ) );
            }

            if ( hiddenEntityTypeIds.Any() )
            {
                qry = qry.Where( v => !v.Attribute.EntityTypeId.HasValue || !hiddenEntityTypeIds.Contains( v.Attribute.EntityTypeId.Value ) );
            }

            if ( gatewayEditorAttributeIds.Any() )
            {
                qry = qry.Where( v => !gatewayEditorAttributeIds.Contains( v.AttributeId )
                    || ( v.EntityId.HasValue && editableGatewayIds.Contains( v.EntityId.Value ) ) );
            }

            isFiltered = hiddenAttributeIds.Any() || hiddenEntityTypeIds.Any() || gatewayEditorAttributeIds.Any();

            return qry;
        }

        /// <summary>
        /// Determines which values of the attribute the person may read.
        /// </summary>
        /// <param name="attribute">The attribute.</param>
        /// <param name="person">The person.</param>
        /// <param name="gatewayEntityTypeId">The identifier of the financial gateway entity type.</param>
        /// <returns>The access the person has to the values of the attribute.</returns>
        private static AttributeValueAccess GetAccess( AttributeCache attribute, Person person, int? gatewayEntityTypeId )
        {
            if ( !attribute.IsAuthorized( Rock.Security.Authorization.VIEW, person ) )
            {
                return AttributeValueAccess.Hidden;
            }

            if ( !SensitiveAttributeHelper.IsSensitiveAttribute( attribute )
                || attribute.IsAuthorized( Rock.Security.Authorization.EDIT, person ) )
            {
                return AttributeValueAccess.Readable;
            }

            return gatewayEntityTypeId.HasValue && attribute.EntityTypeId == gatewayEntityTypeId
                ? AttributeValueAccess.GatewayEditors
                : AttributeValueAccess.Hidden;
        }

        /// <summary>
        /// The access a person has to the values of an attribute.
        /// </summary>
        private enum AttributeValueAccess
        {
            /// <summary>
            /// Every value is readable.
            /// </summary>
            Readable,

            /// <summary>
            /// No value is readable.
            /// </summary>
            Hidden,

            /// <summary>
            /// Only the values on financial gateways the person can edit are readable.
            /// </summary>
            GatewayEditors
        }
    }
}
