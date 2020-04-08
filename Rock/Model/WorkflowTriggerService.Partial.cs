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

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.WorkflowTrigger"/> entity objects.
    /// </summary>
    public partial class WorkflowTriggerService 
    {
        /// <summary>
        /// Returns a queryable collection of  the <see cref="Rock.Model.WorkflowTrigger"/> by <see cref="Rock.Model.EntityType"/> name
        /// and <see cref="Rock.Model.WorkflowTriggerType"/>
        /// </summary>
        /// <param name="entityTypeName">A <see cref="System.String"/> representing the name of the <see cref="Rock.Model.EntityType"/> to filter by.</param>
        /// <param name="triggerType">The <see cref="Rock.Model.WorkflowTriggerType"/> to filter by.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.WorkflowTrigger"/> entities that meet the specified criteria.</returns>
        public IQueryable<WorkflowTrigger> Get( string entityTypeName, WorkflowTriggerType triggerType )
        {
            return Queryable()
                .Where( t =>
                    t.EntityType.Name == entityTypeName &&
                    t.WorkflowTriggerType == triggerType );
        }
    }
}
