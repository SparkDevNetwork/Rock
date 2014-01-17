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
using System.Linq;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.Category"/> objects.
    /// </summary>
    public partial class CategoryService
    {
        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Category">Categories</see> by parent <see cref="Rock.Model.Category"/> and <see cref="Rock.Model.EntityType"/>.
        /// </summary>
        /// <param name="ParentId">A <see cref="System.Int32"/> representing the CategoryID of the parent <see cref="Rock.Model.Category"/> to search by. To find <see cref="Rock.Model.Category">Categories</see>
        /// that do not inherit from a parent category, this value will be null.</param>
        /// <param name="entityTypeId">A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> to search by.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.Category">Categories</see> that meet the specified criteria. </returns>
        public IQueryable<Category> Get( int? ParentId, int? entityTypeId )
        {
            var query = Repository.AsQueryable()
                .Where( c => (c.ParentCategoryId ?? 0) == (ParentId ?? 0) );

            if ( entityTypeId.HasValue )
            {
                query = query.Where( c => c.EntityTypeId == entityTypeId.Value );
            }

            return query
                .OrderBy( t => t.Order )
                .ThenBy( t => t.Name );
        }

        /// <summary>
        /// Returns a queryable collection of <see cref="Rock.Model.Category">Categories</see> by Name, <see cref="Rock.Model.EntityType"/>, Qualifier Column and Qualifier Value.
        /// </summary>
        /// <param name="name">A <see cref="System.String"/> representing the name to search by.</param>
        /// <param name="entityTypeId">A <see cref="System.Int32"/> representing the EntityType of the <see cref="Rock.Model.EntityType"/> to search by.</param>
        /// <param name="entityTypeQualifierColumn">A <see cref="System.String"/> representing the name of the Qualifier Column to search by.</param>
        /// <param name="entityTypeQualifierValue">A <see cref="System.String"/> representing the name of the Qualifier Value to search by.</param>
        /// <returns>A queryable collection of <see cref="Rock.Model.Category">Categories</see> that meet the search criteria.</returns>
        public IQueryable<Category> Get( string name, int? entityTypeId, string entityTypeQualifierColumn, string entityTypeQualifierValue )
        {
            return Repository.AsQueryable()
                .Where( c =>
                    string.Compare( c.Name, name, true ) == 0 &&
                    c.EntityTypeId == entityTypeId &&
                    c.EntityTypeQualifierColumn == entityTypeQualifierColumn &&
                    c.EntityTypeQualifierValue == entityTypeQualifierValue );
        }

        /// <summary>
        /// Returns a enumerable collection of <see cref="Rock.Model.Category">Categories</see> by <see cref="Rock.Model.EntityType"/>
        /// </summary>
        /// <param name="entityTypeId">A <see cref="System.Int32"/> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.Category">Categories</see> are used for the specified <see cref="Rock.Model.Category"/>.</returns>
        public IQueryable<Category> GetByEntityTypeId( int? entityTypeId )
        {
            return Repository.AsQueryable()
                .Where( t => ( t.EntityTypeId == entityTypeId || ( !entityTypeId.HasValue ) ))
                .OrderBy( t => t.Order)
                .ThenBy( t => t.Name);
        }

    }
}
