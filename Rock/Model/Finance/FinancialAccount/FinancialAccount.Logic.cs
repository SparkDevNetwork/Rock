﻿// <copyright>
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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// FinancialAccount Logic
    /// </summary>
    public partial class FinancialAccount
    {
        #region Properties

        /// <summary>
        /// Returns an enumerable collection of the <see cref="Rock.Model.FinancialAccount" /> Ids that are ancestors of a specified accountId sorted starting with the most immediate parent
        /// </summary>
        /// <value>
        /// The parent account ids.
        /// </value>
        [LavaVisible]
        public List<int> ParentAccountIds
        {
            get
            {
                return FinancialAccountCache.Get( this.Id )?.GetAncestorFinancialAccountIds().ToList() ?? new List<int>();
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
                    var parent = this.ParentAccountId.HasValue ? FinancialAccountCache.Get( this.ParentAccountId.Value ).ParentAccount : null;
                    while ( parent != null )
                    {
                        if ( parentIds.Contains( parent.Id ) )
                        {
                            this.ValidationResults.Add( new ValidationResult( "Parent Account cannot be a child of this Account (recursion)" ) );
                            return false;
                        }
                        else
                        {
                            parentIds.Add( parent.Id );
                            parent = parent.ParentAccount;
                        }
                    }
                }

                return result;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this FinancialAccount.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this FinancialAccount.
        /// </returns>
        public override string ToString()
        {
            return this.PublicName;
        }

        #endregion Public Methods

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return FinancialAccountCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            FinancialAccountCache.UpdateCachedEntity( this.Id, entityState );

            if ( this.ParentAccountId.HasValue )
            {
                FinancialAccountCache.UpdateCachedEntity( this.ParentAccountId.Value, EntityState.Detached );
            }

            if ( _originalParentAccountId.HasValue && _originalParentAccountId != this.ParentAccountId )
            {
                FinancialAccountCache.UpdateCachedEntity( _originalParentAccountId.Value, EntityState.Detached );
            }
        }

        #endregion ICacheable
    }
}