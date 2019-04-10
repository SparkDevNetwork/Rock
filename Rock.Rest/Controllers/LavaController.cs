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
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

using Rock.Rest.Filters;
using Rock.Web.Cache;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// Controller of misc utility functions that are used by Rock controls
    /// </summary>
    public class LavaController : ApiControllerBase
    {

        /// <summary>
        /// Renders the template.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="additionalMergeObjects">Any additional merge objects as a comma-delimited-list of EntityTypeId|MergeKey|EntityId</param>
        /// <returns></returns>
        [System.Web.Http.Route( "api/Lava/RenderTemplate" )]
        [HttpPost]
        [Authenticate, Secured]
        public string RenderTemplate( [NakedBody] string template, [FromUri] string additionalMergeObjects = null )
        {
            Rock.Lava.CommonMergeFieldsOptions lavaOptions = new Lava.CommonMergeFieldsOptions();
            lavaOptions.GetPageContext = false;
            lavaOptions.GetPageParameters = false;
            lavaOptions.GetCurrentPerson = true;
            lavaOptions.GetCampuses = true;
            lavaOptions.GetLegacyGlobalMergeFields = false;
            var currentPerson = GetPerson();

            Dictionary<string, object> mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, currentPerson, lavaOptions );

            if ( additionalMergeObjects != null )
            {
                var additionalMergeObjectList = additionalMergeObjects.Split( ',' ).Select( a => a.Split( '|' ) ).Where( a => a.Length == 3 ).Select( a => new
                {
                    EntityTypeId = a[0].AsInteger(),
                    MergeKey = a[1],
                    EntityId = a[2].AsInteger()
                } ).ToList();

                foreach ( var additionalMergeObject in additionalMergeObjectList )
                {
                    var entityTypeType = EntityTypeCache.Get( additionalMergeObject.EntityTypeId )?.GetEntityType();
                    if ( entityTypeType != null )
                    {
                        var dbContext = Rock.Reflection.GetDbContextForEntityType( entityTypeType );
                        var serviceInstance = Rock.Reflection.GetServiceForEntityType( entityTypeType, dbContext );
                        if ( serviceInstance != null )
                        {
                            System.Reflection.MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( int ) } );
                            var mergeObjectEntity = getMethod.Invoke( serviceInstance, new object[] { additionalMergeObject.EntityId } ) as Rock.Data.IEntity;

                            if ( mergeObjectEntity != null )
                            {
                                bool canView = true;
                                if ( mergeObjectEntity is Rock.Security.ISecured )
                                {
                                    canView = ( mergeObjectEntity as Rock.Security.ISecured ).IsAuthorized( Rock.Security.Authorization.VIEW, currentPerson );
                                }

                                if ( canView )
                                {
                                    mergeFields.Add( additionalMergeObject.MergeKey, mergeObjectEntity );
                                }
                            }
                        }
                    }
                }

            }

            return template.ResolveMergeFields( mergeFields, currentPerson );
        }
    }
}
