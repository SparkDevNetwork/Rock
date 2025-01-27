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

using Rock.Lava;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class ConnectionWorkflow
    {
        #region Properties

        /// <summary>
        /// Gets the type of the cache workflow.
        /// </summary>
        /// <value>
        /// The type of the cache workflow.
        /// </value>
        [LavaVisible]
        public virtual WorkflowTypeCache WorkflowTypeCache
        {
            get
            {
                if ( WorkflowTypeId.HasValue && WorkflowTypeId.Value > 0 )
                {
                    return WorkflowTypeCache.Get( WorkflowTypeId.Value );
                }
                return null;
            }
        }

        #endregion

        #region ISecured

        /// <inheritdoc/>
        public override ISecured ParentAuthority => ConnectionType ?? ConnectionOpportunity ?? base.ParentAuthority;

        #endregion
    }
}
