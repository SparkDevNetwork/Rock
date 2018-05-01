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
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a category that is cached by Rock. 
    /// </summary>
    [Serializable]
    [Obsolete( "Use Rock.Cache.CacheCategory instead" )]
    public class CategoryCache : CachedModel<Category>
    {
        #region constructors

        private CategoryCache( CacheCategory cacheCategory )
        {
            CopyFromNewCache( cacheCategory );
        }

        #endregion

        #region Properties

        private readonly object _obj = new object();

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the parent category id.
        /// </summary>
        /// <value>
        /// The parent category id.
        /// </value>
        [DataMember]
        public int? ParentCategoryId { get; set; }

        /// <summary>
        /// Gets or sets the entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier column.
        /// </summary>
        /// <value>
        /// The entity type qualifier column.
        /// </value>
        [DataMember]
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier value.
        /// </summary>
        /// <value>
        /// The entity type qualifier value.
        /// </value>
        [DataMember]
        public string EntityTypeQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [DataMember]
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the color of the highlight.
        /// </summary>
        /// <value>
        /// The color of the highlight.
        /// </value>
        [DataMember]
        public string HighlightColor { get; set; }

        /// <summary>
        /// Gets the parent category.
        /// </summary>
        /// <value>
        /// The parent category.
        /// </value>
        public CategoryCache ParentCategory
        {
            get
            {
                if ( ParentCategoryId != null && ParentCategoryId.Value != 0 )
                {
                    return Read( ParentCategoryId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the categories.
        /// </summary>
        /// <value>
        /// The categories.
        /// </value>
        [DataMember]
        public List<CategoryCache> Categories
        {
            get
            {
                var categories = new List<CategoryCache>();

                lock ( _obj )
                {
                    if ( categoryIds == null )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            categoryIds = new CategoryService( rockContext )
                                .Get( Id, EntityTypeId )
                                .Select( c => c.Id )
                                .ToList();
                        }
                    }
                }

                foreach ( var id in categoryIds )
                {
                    var category = Read( id );
                    if ( category != null )
                    {
                        categories.Add( category );
                    }
                }

                return categories;
            }
        }
        private List<int> categoryIds;

        /// <summary>
        /// Gets the schedule exclusions.
        /// </summary>
        /// <value>
        /// The schedule exclusions.
        /// </value>
        [DataMember]
        public List<DateRange> ScheduleExclusions
        {
            get
            {
                lock ( _obj )
                {
                    if ( scheduleExclusions != null ) return scheduleExclusions;
                    scheduleExclusions = new List<DateRange>();

                    using ( var rockContext = new RockContext() )
                    {
                        foreach ( var exclusion in new ScheduleCategoryExclusionService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( e => e.CategoryId == Id )
                            .ToList() )
                        {
                            scheduleExclusions.Add( new DateRange( exclusion.StartDate, exclusion.EndDate ) );
                        }
                    }
                }

                return scheduleExclusions;
            }
        }
        private List<DateRange> scheduleExclusions;

        /// <summary>
        /// Gets the parent authority.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override ISecured ParentAuthority => ParentCategory ?? base.ParentAuthority;

        #endregion

        #region Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="model">The model.</param>
        public override void CopyFromModel( IEntity model )
        {
            base.CopyFromModel( model );

            if ( !( model is Category ) ) return;

            var category = (Category)model;
            IsSystem = category.IsSystem;
            ParentCategoryId = category.ParentCategoryId;
            EntityTypeId = category.EntityTypeId;
            EntityTypeQualifierColumn = category.EntityTypeQualifierColumn;
            EntityTypeQualifierValue = category.EntityTypeQualifierValue;
            Name = category.Name;
            Description = category.Description;
            Order = category.Order;
            IconCssClass = category.IconCssClass;
            HighlightColor = category.HighlightColor;
        }

        /// <summary>
        /// Copies properties from a new cached entity
        /// </summary>
        /// <param name="cacheEntity">The cache entity.</param>
        protected sealed override void CopyFromNewCache( IEntityCache cacheEntity )
        {
            base.CopyFromNewCache( cacheEntity );

            if ( !( cacheEntity is CacheCategory ) ) return;

            var category = (CacheCategory)cacheEntity;
            IsSystem = category.IsSystem;
            ParentCategoryId = category.ParentCategoryId;
            EntityTypeId = category.EntityTypeId;
            EntityTypeQualifierColumn = category.EntityTypeQualifierColumn;
            EntityTypeQualifierValue = category.EntityTypeQualifierValue;
            Name = category.Name;
            Description = category.Description;
            Order = category.Order;
            IconCssClass = category.IconCssClass;
            HighlightColor = category.HighlightColor;
        }


        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Returns Category object from cache.  If category does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The id of the Category to read</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static CategoryCache Read( int id, RockContext rockContext = null )
        {
            return new CategoryCache( CacheCategory.Get( id, rockContext ) );
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        [Obsolete( "Use Rock.Cache.CacheCategory.Get instead" )]
        public static CategoryCache Read( Guid guid, RockContext rockContext = null )
        {
            return new CategoryCache( CacheCategory.Get( guid, rockContext ) );
        }

        /// <summary>
        /// Adds Category model to cache, and returns cached object
        /// </summary>
        /// <param name="categoryModel">The categoryModel to cache</param>
        /// <returns></returns>
        public static CategoryCache Read( Category categoryModel )
        {
            return new CategoryCache( CacheCategory.Get( categoryModel ) );
        }

        /// <summary>
        /// Removes category from cache
        /// </summary>
        /// <param name="id">The id of the category to remove from cache</param>
        public static void Flush( int id )
        {
            CacheCategory.Remove( id );
        }

        #endregion
    }
}