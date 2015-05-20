// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a category that is cached by Rock. 
    /// </summary>
    [Serializable]
    public class CategoryCache : CachedModel<Category>
    {
        #region constructors

        private CategoryCache( Rock.Model.Category model )
        {
            CopyFromModel( model );
        }

        #endregion

        #region Properties

        private object _obj = new object();

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
                    return CategoryCache.Read( ParentCategoryId.Value );
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
                            categoryIds = new Model.CategoryService( rockContext )
                                .Get( this.Id, this.EntityTypeId )
                                .Select( c => c.Id )
                                .ToList();
                        }
                    }
                }

                foreach ( int id in categoryIds )
                {
                    var category = CategoryCache.Read( id );
                    if ( category != null )
                    {
                        categories.Add( category );
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
        /// <param name="model">The model.</param>
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
                this.Description = category.Description;
                this.Order = category.Order;
                this.IconCssClass = category.IconCssClass;
                this.HighlightColor = category.HighlightColor;
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
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static CategoryCache Read( int id, RockContext rockContext = null )
        {
            return GetOrAddExisting( CategoryCache.CacheKey( id ),
                () => LoadById( id, rockContext ) );
        }

        private static CategoryCache LoadById( int id, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return LoadById2( id, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return LoadById2( id, rockContext2 );
            }
        }

        private static CategoryCache LoadById2( int id, RockContext rockContext )
        {
            var categoryService = new Rock.Model.CategoryService( rockContext );
            var categoryModel = categoryService.Get( id );
            if ( categoryModel != null )
            {
                categoryModel.LoadAttributes( rockContext );
                return new CategoryCache( categoryModel );
            }

            return null;
        }

        /// <summary>
        /// Reads the specified GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static CategoryCache Read( Guid guid, RockContext rockContext = null )
        {
            int id = GetOrAddExisting( guid.ToString(),
                () => LoadByGuid( guid, rockContext ) );

            return Read( id, rockContext );
        }

        private static int LoadByGuid( Guid guid, RockContext rockContext )
        {
            if ( rockContext != null )
            {
                return LoadByGuid2( guid, rockContext );
            }

            using ( var rockContext2 = new RockContext() )
            {
                return LoadByGuid2( guid, rockContext2 );
            }
        }

        private static int LoadByGuid2( Guid guid, RockContext rockContext )
        {
            var categoryService = new CategoryService( rockContext );
            return categoryService
                .Queryable().AsNoTracking()
                .Where( c => c.Guid.Equals( guid ) )
                .Select( c => c.Id )
                .FirstOrDefault();
        }

        /// <summary>
        /// Adds Category model to cache, and returns cached object
        /// </summary>
        /// <param name="categoryModel">The categoryModel to cache</param>
        /// <returns></returns>
        public static CategoryCache Read( Rock.Model.Category categoryModel )
        {
            return GetOrAddExisting( CampusCache.CacheKey( categoryModel.Id ),
                () => LoadByModel( categoryModel ) );
        }

        private static CategoryCache LoadByModel( Rock.Model.Category categoryModel )
        {
            if ( categoryModel != null )
            {
                return new CategoryCache( categoryModel );
            }
            return null;
        }

        /// <summary>
        /// Removes category from cache
        /// </summary>
        /// <param name="id">The id of the category to remove from cache</param>
        public static void Flush( int id )
        {
            FlushCache( CategoryCache.CacheKey( id ) );
        }

        #endregion
    }
}