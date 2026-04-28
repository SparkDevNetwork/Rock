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
using System.Data.Entity;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class StepType
    {
        #region ISecured

        /*
             3/12/2026 - NA

             ⚠ SECURITY NOTICE ⚠

             If the model implements custom ISecured behavior, the corresponding
             {Entity}Cache class MUST implement the same security logic.

             ModelCache<T>.SetFromEntity() only snapshots SupportedActions. Security
             methods such as ParentAuthority, ParentAuthorityPre, IsAuthorized, and
             IsAllowedByDefault are NOT copied automatically. If the cache does not
             override them, it will fall back to ModelCache defaults and may evaluate
             permissions differently than the model.

             Reason: Prevent security mismatches between model entities and cache objects.
        */

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check the default authorization on the current type, and
        /// then the authorization on the Rock.Security.GlobalDefault entity
        /// </summary>
        public override ISecured ParentAuthority
        {
            get
            {
                return this.StepProgram ?? base.ParentAuthority;
            }
        }

        #endregion ISecured

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return StepTypeCache.Get( Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            StepTypeCache.UpdateCachedEntity( Id, entityState );
        }

        #endregion ICacheable
    }
}
