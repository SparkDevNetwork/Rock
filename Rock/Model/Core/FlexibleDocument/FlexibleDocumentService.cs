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

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Data access and service class for <see cref="Rock.Model.FlexibleDocument"/> entity objects.
    /// </summary>
    public partial class FlexibleDocumentService
    {
        /// <summary>
        /// Returns the documents belonging to the <see cref="Rock.Model.FlexibleDocumentModel"/>
        /// with the specified unique key.
        /// </summary>
        /// <param name="modelKey">The unique key of the model the documents belong to (e.g. <c>AgentMemory</c>).</param>
        /// <returns>An <see cref="IQueryable{T}"/> of the documents of that model, empty if no model has that key.</returns>
        public IQueryable<FlexibleDocument> GetByModelKey( string modelKey )
        {
            return Queryable().Where( d => d.FlexibleDocumentModel.Key == modelKey );
        }

        /// <summary>
        /// Returns the documents linked to the specified target entity through a
        /// <see cref="Rock.Model.RelatedEntity"/> row with the specified purpose key.
        /// The document is always the source side of the link.
        /// </summary>
        /// <param name="targetEntityTypeId">The Id of the <see cref="Rock.Model.EntityType"/> of the linked entity.</param>
        /// <param name="targetEntityId">The Id of the linked entity.</param>
        /// <param name="purposeKey">The purpose of the link, typically <see cref="RelatedEntityPurposeKey.FlexibleDocumentPrimary"/>.</param>
        /// <returns>An <see cref="IQueryable{T}"/> of the documents linked to that entity.</returns>
        public IQueryable<FlexibleDocument> GetByTargetEntity( int targetEntityTypeId, int targetEntityId, string purposeKey )
        {
            var documentEntityTypeId = EntityTypeCache.Get<FlexibleDocument>().Id;

            var relatedEntityQry = new RelatedEntityService( ( RockContext ) Context ).Queryable()
                .Where( re => re.SourceEntityTypeId == documentEntityTypeId
                    && re.TargetEntityTypeId == targetEntityTypeId
                    && re.TargetEntityId == targetEntityId
                    && re.PurposeKey == purposeKey );

            return Queryable().Where( d => relatedEntityQry.Any( re => re.SourceEntityId == d.Id ) );
        }
    }
}
