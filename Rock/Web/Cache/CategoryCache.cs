//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Runtime.Serialization;
using System.Web.UI;

using Rock.Field;
using Rock.Model;
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a category that is cached by Rock. 
    /// </summary>
    [Serializable]
    public class CategoryCache : CachedModel<Category>
    {
        #region constructors

        private CategoryCache()
        {
        }

        private CategoryCache( Rock.Model.Category model )
        {
            CopyFromModel( model );
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the parent category id.
        /// </summary>
        /// <value>
        /// The parent category id.
        /// </value>
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
        public string EntityTypeQualifierColumn { get; set; }

        /// <summary>
        /// Gets or sets the entity type qualifier value.
        /// </summary>
        /// <value>
        /// The entity type qualifier value.
        /// </value>
        public string EntityTypeQualifierValue { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the icon small file id.
        /// </summary>
        /// <value>
        /// The icon small file id.
        /// </value>
        public int? IconSmallFileId { get; set; }

        /// <summary>
        /// Gets or sets the icon large file id.
        /// </summary>
        /// <value>
        /// The icon large file id.
        /// </value>
        public int? IconLargeFileId { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        public string IconCssClass { get; set; }

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
                    return CategoryCache.Read( ParentCategoryId.Value );
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the categories.
        /// </summary>
        /// <value>
        /// The categories.
        /// </value>
        public List<CategoryCache> Categories
        {
            get
            {
                List<CategoryCache> categories = new List<CategoryCache>();

                if ( categoryIds != null )
                {
                    foreach ( int id in categoryIds.ToList() )
                    {
                        categories.Add( CategoryCache.Read( id ) );
                    }
                }
                else
                {
                    categoryIds = new List<int>();

                    CategoryService categoryService = new CategoryService();
                    foreach ( Category category in categoryService.Get( this.Id, this.EntityTypeId ) )
                    {
                        categoryIds.Add( category.Id );
                        categories.Add( CategoryCache.Read( category ) );
                    }
                }

                return categories;
            }
        }
        private List<int> categoryIds = null;

        #endregion

        #region Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="category">The category.</param>
        public override void CopyFromModel( Data.IEntity model )
        {
            base.CopyFromModel( model );

            if ( model is Category )
            {
                var category = (Category)model;
                this.IsSystem = category.IsSystem;
                this.ParentCategoryId = category.ParentCategoryId;
                this.EntityTypeId = category.EntityTypeId;
                this.EntityTypeQualifierColumn = category.EntityTypeQualifierColumn;
                this.EntityTypeQualifierValue = category.EntityTypeQualifierValue;
                this.Name = category.Name;
                this.IconSmallFileId = category.IconSmallFileId;
                this.IconLargeFileId = category.IconLargeFileId;
                this.IconCssClass = category.IconCssClass;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

        #region Static Methods

        private static string CacheKey( int id )
        {
            return string.Format( "Rock:Category:{0}", id );
        }

        /// <summary>
        /// Returns Category object from cache.  If category does not already exist in cache, it
        /// will be read and added to cache
        /// </summary>
        /// <param name="id">The id of the Category to read</param>
        /// <returns></returns>
        public static CategoryCache Read( int id )
        {
            string cacheKey = CategoryCache.CacheKey( id );

            ObjectCache cache = MemoryCache.Default;
            CategoryCache category = cache[cacheKey] as CategoryCache;

            if ( category != null )
            {
                return category;
            }
            else
            {
                var categoryService = new Rock.Model.CategoryService();
                var categoryModel = categoryService.Get( id );
                if ( categoryModel != null )
                {
                    category = new CategoryCache( categoryModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( cacheKey, category, cachePolicy );
                    cache.Set( category.Guid.ToString(), category.Id, cachePolicy );

                    return category;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public static CategoryCache Read( Guid guid )
        {
            ObjectCache cache = MemoryCache.Default;
            object cacheObj = cache[guid.ToString()];

            if ( cacheObj != null )
            {
                return Read( (int)cacheObj );
            }
            else
            {
                var categoryService = new CategoryService();
                var categoryModel = categoryService.Get(guid);

                if ( categoryModel != null )
                {
                    categoryModel.LoadAttributes();
                    var category = new CategoryCache( categoryModel );

                    var cachePolicy = new CacheItemPolicy();
                    cache.Set( CategoryCache.CacheKey( category.Id ), category, cachePolicy );
                    cache.Set( category.Guid.ToString(), category.Id, cachePolicy );

                    return category;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Adds Category model to cache, and returns cached object
        /// </summary>
        /// <param name="categoryModel">The categoryModel to cache</param>
        /// <returns></returns>
        public static CategoryCache Read( Rock.Model.Category categoryModel )
        {
            string cacheKey = CategoryCache.CacheKey( categoryModel.Id );

            ObjectCache cache = MemoryCache.Default;
            CategoryCache category = cache[cacheKey] as CategoryCache;

            if ( category != null )
            {
                return category;
            }
            else
            {
                category = new CategoryCache( categoryModel );

                var cachePolicy = new CacheItemPolicy();
                cache.Set( cacheKey, category, cachePolicy );
                cache.Set( category.Guid.ToString(), category.Id, cachePolicy );
                
                return category;
            }
        }

        /// <summary>
        /// Removes category from cache
        /// </summary>
        /// <param name="id">The id of the category to remove from cache</param>
        public static void Flush( int id )
        {
            ObjectCache cache = MemoryCache.Default;
            cache.Remove( CategoryCache.CacheKey( id ) );
        }

        #endregion


    }
}