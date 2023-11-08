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
using Rock.Web.Cache;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace Rock.Model
{
    public partial class Campus
    {
        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return CampusCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            CampusCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion

        /// <summary>
        /// Gets the current date time basd on the <see cref="Campus.TimeZoneId" />.
        /// </summary>
        /// <value>
        /// The current date time.
        /// </value>
        [NotMapped]
        public virtual DateTime CurrentDateTime
        {
            get
            {
                if ( TimeZoneId.IsNotNullOrWhiteSpace() )
                {
                    var campusTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById( TimeZoneId );
                    if ( campusTimeZoneInfo != null )
                    {
                        return TimeZoneInfo.ConvertTime( DateTime.UtcNow, campusTimeZoneInfo );
                    }
                }

                return RockDateTime.Now;
            }
        }

        /// <summary>
        /// Gets the condensed name of the campus. This will be the short code
        /// if set, otherwise the name of the campus by stripping off any
        /// trailing " Campus" text.
        /// </summary>
        /// <value>
        /// The condensed name of the campus.
        /// </value>
        [NotMapped]
        public string CondensedName => ShortCode.IfEmpty( Name.ReplaceIfEndsWith( " Campus", string.Empty ) );
    }
}
