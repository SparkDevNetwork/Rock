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
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    ///
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "ScheduleCategoryExclusion" )]
    [DataContract]
    public partial class ScheduleCategoryExclusion : Model<ScheduleCategoryExclusion>, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the category identifier.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember(IsRequired =true)]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public DateTime EndDate { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The report.
        /// </value>
        [LavaInclude]
        public virtual Category Category { get; set; }

        #endregion

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            // doesn't apply
            return null;
        }

        /// <summary>
        /// Updates any Cache Objects that are dependent on this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            // Update CategoryCache ScheduleExclusions
            CategoryCache.UpdateCachedEntity( this.CategoryId, entityState );
        }

        #endregion

    }

    /// <summary>
    ///
    /// </summary>
    public partial class ScheduleCategoryExclusionConfiguration : EntityTypeConfiguration<ScheduleCategoryExclusion>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleCategoryExclusionConfiguration"/> class.
        /// </summary>
        public ScheduleCategoryExclusionConfiguration()
        {
            this.HasRequired( p => p.Category ).WithMany().HasForeignKey( p => p.CategoryId ).WillCascadeOnDelete( true );
        }
    }

}
