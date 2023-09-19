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

using Rock.Data;
using Rock.Model;
using Rock.Security;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a category that is cached by Rock. 
    /// </summary>
    [Serializable]
    [DataContract]
    public class CategoryCache : ModelCache<CategoryCache, Category>
    {

        #region Properties

        private readonly object _obj = new object();

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; private set; }

        /// <summary>
        /// Gets or sets the parent category identifier.
        /// </summary>
        /// <value>
        /// The parent category identifier.
        /// </value>
        [DataMember]
        public int? ParentCategoryId { get; private set; }

        /// <summary>
        /// Gets or sets the entity type identifier.
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; private set; }

        /// <summary>
        /// Gets or sets the entity type qualifier column.
        /// </summary>
        /// <value>
        /// The entity type qualifier column.
        /// </value>
        [DataMember]
        public string EntityTypeQualifierColumn { get; private set; }

        /// <summary>
        /// Gets or sets the entity type qualifier value.
        /// </summary>
        /// <value>
        /// The entity type qualifier value.
        /// </value>
        [DataMember]
        public string EntityTypeQualifierValue { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; private set; }

        /// <summary>
        /// Gets or sets the icon CSS class.
        /// </summary>
        /// <value>
        /// The icon CSS class.
        /// </value>
        [DataMember]
        public string IconCssClass { get; private set; }

        /// <summary>
        /// Gets or sets the color of the highlight.
        /// </summary>
        /// <value>
        /// The color of the highlight.
        /// </value>
        [DataMember]
        public string HighlightColor { get; private set; }

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
                    return Get( ParentCategoryId.Value );
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the child categories.
        /// </summary>
        /// <value>
        /// The categories.
        /// </value>
        public List<CategoryCache> Categories
        {
            get
            {
                var categories = new List<CategoryCache>();

                lock ( _obj )
                {
                    if ( _categoryIds == null )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            _categoryIds = new CategoryService( rockContext )
                                .Get( Id, EntityTypeId )
                                .Select( c => c.Id )
                                .ToList();
                        }
                    }
                }

                if ( _categoryIds == null ) return categories;

                foreach ( var id in _categoryIds )
                {
                    var category = Get( id );
                    if ( category != null )
                    {
                        categories.Add( category );
                    }
                }
                return categories;
            }
        }
        private List<int> _categoryIds;

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
                if ( _scheduleExclusions != null ) return _scheduleExclusions;
                lock ( _obj )
                {
                    if ( _scheduleExclusions != null ) return _scheduleExclusions;
                    _scheduleExclusions = new List<DateRange>();

                    using ( var rockContext = new RockContext() )
                    {
                        foreach ( var exclusion in new ScheduleCategoryExclusionService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( e => e.CategoryId == Id )
                            .ToList() )
                        {
                            _scheduleExclusions.Add( new DateRange( exclusion.StartDate, exclusion.EndDate ) );
                        }
                    }
                }

                return _scheduleExclusions;
            }
            private set
            {
                _scheduleExclusions = value;
            }
        }
        private volatile List<DateRange> _scheduleExclusions;

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check access to the parent authority specified by this property.
        /// </summary>
        public override ISecured ParentAuthority => ParentCategory ?? base.ParentAuthority;

        #endregion

        #region Methods

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var category = entity as Category;
            if ( category == null ) return;

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

        /// <summary>
        /// Gets all <seealso cref="CategoryCache">Categories</seealso> for a specific entityTypeId.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <returns></returns>
        public static CategoryCache[] AllForEntityType( int entityTypeId )
        {
            return GetByEntityType( entityTypeId );
        }

        /// <summary>
        /// Gets a list of all <seealso cref="CategoryCache">Categories</seealso> for a specific entityType.
        /// </summary>
        /// <returns></returns>
        public static CategoryCache[] AllForEntityType<T>()
        {
            var entityTypeId = EntityTypeCache.Get<T>()?.Id;
            return AllForEntityType( entityTypeId ?? 0 );
        }

        /// <summary>
        /// Gets all <seealso cref="CategoryCache">Categories</seealso> for a specific entityTypeId.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <returns></returns>
        internal static CategoryCache[] GetByEntityType( int? entityTypeId )
        {
            var rockContext = new RockContext();
            var Categorieservice = new CategoryService(rockContext);
            var categoryIdList = Categorieservice.GetByEntityTypeId( entityTypeId )
               .Select(c => c.Id)
               .ToList();

            var categories = GetMany( categoryIdList, rockContext )
                .ToArray();

            return categories;
        }

        #endregion

    }
}