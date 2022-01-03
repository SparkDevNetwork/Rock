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
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

using Rock.Data;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    public partial class Category
    {
        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return CategoryCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            CategoryCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion

        /// <summary>
        /// Gets the parent authority where security authorizations are being inherited from.
        /// </summary>
        /// <value>
        /// The parent authority.
        /// </value>
        public override ISecured ParentAuthority
        {
            get
            {
                if ( ParentCategory != null )
                {
                    return ParentCategory;
                }

                return base.ParentAuthority;
            }
        }

        /// <summary>
        /// A dictionary of actions that this class supports and the description of each.
        /// </summary>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                var entityTypeCache = EntityTypeCache.Get( this.EntityTypeId );
                if ( entityTypeCache == null && this.EntityType != null )
                {
                    entityTypeCache = EntityTypeCache.Get( this.EntityType.Id );
                }

                if ( entityTypeCache != null )
                {
                    switch ( entityTypeCache.Name )
                    {
                        case "Rock.Model.Tag":
                            {
                                var supportedActions = new Dictionary<string, string>();
                                supportedActions.Add( Authorization.VIEW, "The roles and/or users that have access to view." );
                                supportedActions.Add( Authorization.TAG, "The roles and/or users that have access to tag items." );
                                supportedActions.Add( Authorization.EDIT, "The roles and/or users that have access to edit." );
                                supportedActions.Add( Authorization.ADMINISTRATE, "The roles and/or users that have access to administrate." );
                                return supportedActions;
                            }
                    }
                }

                return base.SupportedActions;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public override bool IsValid
        {
            get
            {
                var result = base.IsValid;
                if ( result )
                {
                    // make sure it isn't getting saved with a recursive parent hierarchy
                    var parentIds = new List<int>();
                    parentIds.Add( this.Id );
                    var parent = this.ParentCategoryId.HasValue ? ( this.ParentCategory ?? new CategoryService( new RockContext() ).Get( this.ParentCategoryId.Value ) ) : null;
                    while ( parent != null )
                    {
                        if ( parentIds.Contains( parent.Id ) )
                        {
                            this.ValidationResults.Add( new ValidationResult( "Parent Category cannot be a child of this category (recursion)" ) );
                            return false;
                        }
                        else
                        {
                            parentIds.Add( parent.Id );
                            parent = parent.ParentCategory;
                        }
                    }
                }

                return result;
            }
        }
    }
}
