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
    /// Data access/service class for <see cref="Rock.Model.GroupType"/> objects.
    /// </summary>
    public partial class GroupTypeService
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.GroupType"/> entities by the Id of their <see cref="Rock.Model.GroupTypeRole"/>.
        /// </summary>
        /// <param name="defaultGroupRoleId">An <see cref="System.Int32"/> representing the Id of the <see cref="Rock.Model.GroupTypeRole"/> to search by.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.GroupType">GroupTypes</see> that use the provided <see cref="Rock.Model.GroupTypeRole"/> as the 
        /// default GroupRole for their member Groups.</returns>
        public IEnumerable<GroupType> GetByDefaultGroupRoleId( int? defaultGroupRoleId )
        {
            return Queryable().Where( t => ( t.DefaultGroupRoleId == defaultGroupRoleId || ( defaultGroupRoleId == null && t.DefaultGroupRoleId == null ) ) );
        }

        /// <summary>
        /// Gets the child group types.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public IQueryable<GroupType> GetChildGroupTypes( int groupTypeId )
        {
            return Queryable().Where( t => t.ParentGroupTypes.Select( p => p.Id ).Contains( groupTypeId ) );
        }

        /// <summary>
        /// Gets the child group types.
        /// </summary>
        /// <param name="groupTypeGuid">The group type unique identifier.</param>
        /// <returns></returns>
        public IQueryable<GroupType> GetChildGroupTypes( Guid groupTypeGuid )
        {
            return Queryable().Where( t => t.ParentGroupTypes.Select( p => p.Guid ).Contains( groupTypeGuid ) );
        }

        /// <summary>
        /// Gets the parent group types.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public IQueryable<GroupType> GetParentGroupTypes( int groupTypeId )
        {
            return Queryable().Where( t => t.ChildGroupTypes.Select( p => p.Id ).Contains( groupTypeId ) );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.GroupType">GroupType</see> that are descendants of a specified group type.
        /// WARNING: This will fail if their is a circular reference in the GroupTypeAssociation table.
        /// </summary>
        /// <param name="parentGroupTypeId">The parent group type identifier.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.GroupType">GroupType</see>.
        /// </returns>
        public IEnumerable<GroupType> GetAllAssociatedDescendents( int parentGroupTypeId )
        {
            return this.ExecuteQuery(
                @"
                WITH CTE AS (
		            SELECT [GroupTypeId],[ChildGroupTypeId] FROM [GroupTypeAssociation] WHERE [GroupTypeId] = {0}
		            UNION ALL
		            SELECT [a].[GroupTypeId],[a].[ChildGroupTypeId] FROM [GroupTypeAssociation] [a]
		            JOIN CTE acte ON acte.[ChildGroupTypeId] = [a].[GroupTypeId]
                 )
                SELECT *
                FROM [GroupType]
                WHERE [Id] IN ( SELECT [ChildGroupTypeId] FROM CTE )
                ", parentGroupTypeId );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.GroupType">GroupType</see> that are descendants of a specified group type.
        /// WARNING: This will fail if their is a circular reference in the GroupTypeAssociation table.
        /// </summary>
        /// <param name="parentGroupTypeGuid">The parent group type unique identifier.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.GroupType">GroupType</see>.
        /// </returns>
        public IEnumerable<GroupType> GetAllAssociatedDescendents( Guid parentGroupTypeGuid )
        {
            return this.GetAllAssociatedDescendents( this.Get( parentGroupTypeGuid ).Id );
        }

        /// <summary>
        /// Deletes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        public override bool Delete( GroupType item )
        {
            string message;
            if ( !CanDelete( item, out message ) )
            {
                return false;
            }

            return base.Delete( item );
        }
    }
}
