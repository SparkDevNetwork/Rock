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
using Rock.Data;
using Rock.Lava;
using Rock.Web.Cache;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// WorkflowActionFormAttribute Logic
    /// </summary>
    public partial class WorkflowActionFormAttribute
    {
        /// <summary>
        /// Gets or sets the field visibility rules.
        /// </summary>
        /// <value>
        /// The field visibility rules.
        /// </value>
        [NotMapped]
        public virtual Field.FieldVisibilityRules FieldVisibilityRules
        {
            get
            {
                if ( FieldVisibilityRulesJSON.IsNullOrWhiteSpace() )
                {
                    return new Field.FieldVisibilityRules();
                }
                return FieldVisibilityRulesJSON.FromJsonOrNull<Field.FieldVisibilityRules>() ?? new Field.FieldVisibilityRules();
            }
            set
            {
                if ( value == null || value.RuleList.Count == 0 )
                {
                    FieldVisibilityRulesJSON = null;
                }
                else
                {
                    FieldVisibilityRulesJSON = value.ToJson();
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
            return WorkflowActionFormAttributeCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Data.DbContext dbContext )
        {
            WorkflowActionFormCache.UpdateCachedEntity( this.WorkflowActionFormId, EntityState.Modified );
            WorkflowActionFormAttributeCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion
    }
}

