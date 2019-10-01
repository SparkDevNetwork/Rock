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
using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Data;
using Rock.Web.Cache;

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
        /// WARNING: This will fail (max recursion) if there is a circular reference in the GroupTypeAssociation table.
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
                    WHERE acte.[ChildGroupTypeId] <> acte.[GroupTypeId]
					-- and the child group type can't be a parent group type
					AND [a].[ChildGroupTypeId] <> acte.[GroupTypeId]
                 )
                SELECT *
                FROM [GroupType]
                WHERE [Id] IN ( SELECT [ChildGroupTypeId] FROM CTE )
                ", parentGroupTypeId );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.GroupType">GroupType</see> that are descendants of a specified group type
        /// and ordered by the group type's Order in the hierarchy.
        /// WARNING: This will fail (max recursion) if there is a circular reference in the GroupTypeAssociation table.
        /// </summary>
        /// <param name="parentGroupTypeId">The parent group type identifier.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.GroupType">GroupType</see>.
        /// </returns>
        public IEnumerable<GroupType> GetAllAssociatedDescendentsOrdered( int parentGroupTypeId )
        {
            // We're basically building a hierarchy ordering path using padded zeros of the GroupType's order
            // such that the results of the HierarchyOrder looks something like this:
            //
            //|000
            //|000
            //|001
            //|002
            //|002|000
            //|002|000|000
            //|002|001
            //|003
            //|004

            return this.ExecuteQuery(
                @"
                -- Get GroupTypes ordered by their association GroupType's Order
                WITH CTE (ChildGroupTypeId,GroupTypeId, HierarchyOrder) AS
                (
                      SELECT [ChildGroupTypeId], [GroupTypeId], CONVERT(nvarchar(500),'')
                      FROM   [GroupTypeAssociation] GTA
		                INNER JOIN [GroupType] GT ON GT.[Id] = GTA.[GroupTypeId]
                      WHERE  [GroupTypeId] = {0}
                      UNION ALL 
                      SELECT
                            GTA.[ChildGroupTypeId], GTA.[GroupTypeId], CONVERT(nvarchar(500), CTE.HierarchyOrder + '|' + RIGHT(1000 + GT2.[Order], 3)  )
                      FROM
                            GroupTypeAssociation GTA
		                INNER JOIN CTE ON CTE.[ChildGroupTypeId] = GTA.[GroupTypeId]
		                INNER JOIN [GroupType] GT2 ON GT2.[Id] = GTA.[GroupTypeId]
                      WHERE CTE.[ChildGroupTypeId] <> CTE.[GroupTypeId]
					  -- and the child group type can't be a parent group type
					  AND GTA.[ChildGroupTypeId] <> CTE.[GroupTypeId]
                )
                SELECT GT3.*
                FROM CTE
                INNER JOIN [GroupType] GT3 ON GT3.[Id] = CTE.[ChildGroupTypeId]
				ORDER BY CONVERT(nvarchar(500), CTE.HierarchyOrder + '|' + RIGHT(1000 + GT3.[Order], 3) ) 
                ", parentGroupTypeId );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.GroupTypePath">GroupTypePath</see> objects that are
        /// associated descendants of a specified group type.
        /// WARNING: This will fail if there is a circular reference in the GroupTypeAssociation table.
        /// </summary>
        /// <param name="parentGroupTypeId">The parent group type identifier.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.GroupTypePath">GroupTypePath</see> objects.
        /// </returns>
        public IEnumerable<GroupTypePath> GetAllAssociatedDescendentsPath( int parentGroupTypeId )
        {
            return this.Context.Database.SqlQuery<GroupTypePath>(
                @"
                -- Get GroupType association hierarchy with GroupType ancestor path information
                WITH CTE (ChildGroupTypeId,GroupTypeId, HierarchyPath) AS
                (
                      SELECT [ChildGroupTypeId], [GroupTypeId], CONVERT(nvarchar(500),'')
                      FROM   [GroupTypeAssociation] GTA
		                INNER JOIN [GroupType] GT ON GT.[Id] = GTA.[GroupTypeId]
                      WHERE  [GroupTypeId] = {0}
                      UNION ALL 
                      SELECT
                            GTA.[ChildGroupTypeId], GTA.[GroupTypeId], CONVERT(nvarchar(500), CTE.HierarchyPath + ' > ' + GT2.Name)
                      FROM
                            GroupTypeAssociation GTA
		                INNER JOIN CTE ON CTE.[ChildGroupTypeId] = GTA.[GroupTypeId]
		                INNER JOIN [GroupType] GT2 ON GT2.[Id] = GTA.[GroupTypeId]
                      WHERE CTE.[ChildGroupTypeId] <> CTE.[GroupTypeId]
					  -- and the child group type can't be a parent group type
					  AND GTA.[ChildGroupTypeId] <> CTE.[GroupTypeId]
                )
                SELECT GT3.Id as 'GroupTypeId', SUBSTRING( CONVERT(nvarchar(500), CTE.HierarchyPath + ' > ' + GT3.Name), 4, 500) AS 'Path'
                FROM CTE
                INNER JOIN [GroupType] GT3 ON GT3.[Id] = CTE.[ChildGroupTypeId]
                ", parentGroupTypeId );
        }

        /// <summary>
        /// Gets all checkin group type paths.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<GroupTypePath> GetAllCheckinGroupTypePaths()
        {
            List<GroupTypePath> result = new List<GroupTypePath>();

            GroupTypeService groupTypeService = this;

            var qry = groupTypeService.Queryable();

            // limit to show only GroupTypes that have a group type purpose of Checkin Template
            int groupTypePurposeCheckInTemplateId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE ) ).Id;
            qry = qry.Where( a => a.GroupTypePurposeValueId == groupTypePurposeCheckInTemplateId );

            foreach ( var groupTypeId in qry.Select( a => a.Id ) )
            {

                result.AddRange( groupTypeService.GetAllAssociatedDescendentsPath( groupTypeId ) );
            }

            return result;
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

        /// <summary>
        /// Does a direct Bulk Delete of group history for all groups and group members of the specified group type and commits the changes to the database.
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        public void BulkDeleteGroupHistory( int groupTypeId )
        {
            var rockContext = this.Context as RockContext;
            var groupHistoryRecordsToDelete = new GroupHistoricalService( rockContext ).Queryable().Where( a => a.GroupTypeId == groupTypeId );
            var groupMemberHistoryRecordsToDelete = new GroupMemberHistoricalService( rockContext ).Queryable().Where( a => a.Group.GroupTypeId == groupTypeId );

            rockContext.BulkDelete( groupHistoryRecordsToDelete );
            rockContext.BulkDelete( groupMemberHistoryRecordsToDelete );
        }

        /// <summary>
        /// Gets the Guid for the GroupType that has the specified Id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Guid? GetGuid( int id )
        {
            var cacheItem = GroupTypeCache.Get( id );
            if ( cacheItem != null )
            {
                return cacheItem.Guid;
            }

            return null;
        }
    }

    /// <summary>
    /// Represents a GroupTypePath object in Rock.
    /// </summary>
    public class GroupTypePath
    {
        /// <summary>
        /// Gets or sets the ID of the GroupType.
        /// </summary>
        /// <value>
        /// ID of the GroupType.
        /// </value>
        public int GroupTypeId { get; set; }

        /// <summary>
        /// Gets or sets the full associated ancestor path (of group type associations). 
        /// </summary>
        /// <value>
        /// Full path of the ancestor group type associations. 
        /// </value>
        public string Path { get; set; }

        /// <summary>
        /// Returns the Path of the GroupTypePath
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Path;
        }
    }
}
