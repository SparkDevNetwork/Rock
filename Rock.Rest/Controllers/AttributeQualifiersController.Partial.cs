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
using System.Net;
using System.Web.Http;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// AttributeQualifiers REST API
    /// </summary>
    public partial class AttributeQualifiersController
    {
        /// <inheritdoc/>
        protected override void CheckCanEdit( AttributeQualifier entity )
        {
            base.CheckCanEdit( entity );

            if ( entity == null )
            {
                throw new HttpResponseException( HttpStatusCode.BadRequest );
            }

            CheckCanAdministerAttribute( entity.AttributeId );
        }

        /// <inheritdoc/>
        protected override void CheckCanEditAfterUpdate( AttributeQualifier entity )
        {
            base.CheckCanEditAfterUpdate( entity );

            CheckCanAdministerAttribute( entity.AttributeId );
        }

        /// <summary>
        /// Checks that the current person can administrate the attribute that owns the qualifier.
        /// </summary>
        /// <param name="attributeId">The attribute identifier.</param>
        /// <exception cref="HttpResponseException"></exception>
        private void CheckCanAdministerAttribute( int attributeId )
        {
            /*
                10/16/2026 - MSE

                AttributeQualifier is not secured, so the generic v1 endpoints wrote qualifiers
                with no permission check at all. Qualifiers hold attribute configuration, such
                as a Single-Select values list that may run SQL. In the UI, editing attribute
                configuration is effectively a CMS/block-level gate (the admin-only Attributes
                block) that a direct REST endpoint cannot replicate, so we require ADMINISTRATE
                on the parent attribute, the attribute's own ISecured lever and the same one the
                Attributes block uses for its security action (Rock.Blocks/Core/Attributes.cs:315).
                EDIT is not enough because staff inherit EDIT on every Person attribute through
                the seeded Person entity type rules.

                Reason: Attribute configuration is an administrator setting.
            */
            var attribute = AttributeCache.Get( attributeId );
            if ( attribute == null )
            {
                throw new HttpResponseException( HttpStatusCode.BadRequest );
            }

            if ( !attribute.IsAuthorized( Rock.Security.Authorization.ADMINISTRATE, GetPerson() ) )
            {
                throw new HttpResponseException( HttpStatusCode.Unauthorized );
            }
        }
    }
}
