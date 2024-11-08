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
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

using Rock.Achievement;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class AchievementType
    {
        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return AchievementTypeCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            AchievementTypeCache.UpdateCachedEntity( Id, entityState );
        }

        #endregion ICacheable

        #region Overrides

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        public override bool IsValid
        {
            get
            {
                var isValid = base.IsValid;

                if ( MaxAccomplishmentsAllowed > 1 && AllowOverAchievement )
                {
                    ValidationResults.Add( new ValidationResult( $"{nameof( MaxAccomplishmentsAllowed )} cannot be greater than 1 if {nameof( AllowOverAchievement )} is set." ) );
                    isValid = false;
                }

                return isValid;
            }
        }

        #endregion Overrides

        #region Methods

        /// <summary>
        /// Updates the <see cref="TargetCount"/> property for this instance
        /// by asking the component.
        /// </summary>
        /// <param name="rockContext">The rock context to use for database access.</param>
        internal void UpdateTargetCount( RockContext rockContext )
        {
            var componentEntityType = EntityTypeCache.Get( ComponentEntityTypeId, rockContext );
            var component = componentEntityType != null ? AchievementContainer.GetComponent( componentEntityType.Name ) : null;

            if ( component != null )
            {
                TargetCount = component.GetTargetCount( this );
            }
        }

        #endregion
    }
}
