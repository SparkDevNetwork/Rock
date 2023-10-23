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
            9/13/2023 - PA

            WorkflowType should never be having a ParentAuthority or a ParentAuthorityPre.
            WorflowType is the ParentAuthority of Workflow. Let say, if we were to set the ParentAuthority on the
            WorkflowType to be Category, then anyone who has the edit on the Category would be able to edit the running
            Workflows in the Category which is not the desired behavior. Similarly the ParentAuthorityPre were to be set to
            Category, we would run into issues like https://github.com/SparkDevNetwork/Rock/issues/5537 where the forms in
            the sub categories become uneditable.

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
