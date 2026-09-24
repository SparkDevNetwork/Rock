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

using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// WorkflowType Logic
    /// </summary>
    public partial class WorkflowType
    {
        #region ISecured

        /*
             8/7/2026 - NA

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
        /// Gets the parent security authority for the WorkflowType which is its Category
        /// </summary>
        /// <value>
        /// The parent authority of the WorkflowType.
        /// </value>
        public override Security.ISecured ParentAuthority => Category ?? base.ParentAuthority;

        /*
            8/7/2026 - NA

            Historical note: an earlier version of this file stated that WorkflowType
            should never have a ParentAuthority. That policy was intentionally reversed
            by the work for GitHub issue #6712, which added Category as the
            ParentAuthority above so Workflow Type security honors its Category. See:
            https://www.rockrms.com/tech-bulletin/workflow-form-builder-security-now-honors-category-permissions

            Do not remove the ParentAuthority override without also updating that bulletin.

            (Note: only ParentAuthority is set to the Category; ParentAuthorityPre is
            intentionally NOT set, to avoid the sub-category editability problem
            described in GitHub issue #5537.)
        */

        #endregion ISecured

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this instance has active forms.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance has active forms; otherwise, <c>false</c>.
        /// </value>
        public bool HasActiveForms
        {
            get
            {
                return ActivityTypes
                    .Where( t => t.IsActive.HasValue && t.IsActive.Value )
                    .SelectMany( t => t.ActionTypes )
                    .Where( a => a.WorkflowFormId.HasValue )
                    .Any();
            }
        }

        #endregion Properties

        /*
            8/7/2026 - NA

            Historical note: an earlier version of this file stated that WorkflowType
            should never have a ParentAuthority. That policy was intentionally reversed
            by the work for GitHub issue #6712, which added Category as the
            ParentAuthority above so Workflow Type security honors its Category. See:
            https://www.rockrms.com/tech-bulletin/workflow-form-builder-security-now-honors-category-permissions

            Do not remove the ParentAuthority override without also updating that bulletin.

            (Note: only ParentAuthority is set to the Category; ParentAuthorityPre is
            intentionally NOT set, to avoid the sub-category editability problem
            described in GitHub issue #5537.)
        */

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this WorkflowType.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this WorkflowType.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion Public Methods

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return WorkflowTypeCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            WorkflowTypeCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion ICacheable
    }
}
