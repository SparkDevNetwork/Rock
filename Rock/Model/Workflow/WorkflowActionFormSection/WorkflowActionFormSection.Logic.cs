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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class WorkflowActionFormSection
    {
        /// <summary>
        /// Gets or sets the section visibility rules.
        /// </summary>
        /// <value>
        /// The section visibility rules.
        /// </value>
        [NotMapped]
        public virtual Field.FieldVisibilityRules SectionVisibilityRules
        {
            get
            {
                if ( SectionVisibilityRulesJSON.IsNullOrWhiteSpace() )
                {
                    return new Field.FieldVisibilityRules();
                }
                return SectionVisibilityRulesJSON.FromJsonOrNull<Field.FieldVisibilityRules>() ?? new Field.FieldVisibilityRules();
            }
            set
            {
                if ( value == null || value.RuleList.Count == 0 )
                {
                    SectionVisibilityRulesJSON = null;
                }
                else
                {
                    SectionVisibilityRulesJSON = value.ToJson();
                }
            }
        }

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return WorkflowActionFormSectionCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            WorkflowActionFormSectionCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion
    }
}
