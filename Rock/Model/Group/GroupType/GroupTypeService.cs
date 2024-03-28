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
using System.Linq;

using Rock.Attribute;
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

        #region Methods for CheckinAreas (which are GroupTypes)

        /// <summary>
        /// Gets the checkin area descendants.
        /// </summary>
        /// <param name="parentCheckinAreaGroupTypeId">The parent checkin area group type identifier.</param>
        /// <returns></returns>
        public List<GroupTypeCache> GetCheckinAreaDescendants( int parentCheckinAreaGroupTypeId )
        {
            List<GroupTypeCache> checkinAreaDescendants = new List<GroupTypeCache>();
            BuildCheckinAreaDescendants( GroupTypeCache.Get( parentCheckinAreaGroupTypeId ), ref checkinAreaDescendants );
            return checkinAreaDescendants;
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="GroupTypeCache">GroupType</see> that are descendants of a specified root group type
        /// and ordered by the group type's Order in the hierarchy.
        /// </summary>
        /// <param name="rootCheckinAreaGroupTypeId">The root checkin area group type identifier.</param>
        /// <returns>
        /// An enumerable collection of <see cref="GroupTypeCache">GroupType</see>.
        /// </returns>
        public IEnumerable<GroupTypeCache> GetCheckinAreaDescendantsOrdered( int rootCheckinAreaGroupTypeId )
        {
            var checkinAreaDescendantsPath = GetCheckinAreaDescendantsPath( rootCheckinAreaGroupTypeId );
            var ordered = checkinAreaDescendantsPath.OrderBy( a => a.HierarchyPathString ).Select( a => GroupTypeCache.Get( a.GroupTypeId ) ).AsEnumerable();
            return ordered;
        }

        /// <summary>
        /// Gets the checkin area descendants path.
        /// </summary>
        /// <param name="rootCheckinAreaGroupTypeId">The root checkin area group type identifier.</param>
        /// <returns></returns>
        public List<CheckinAreaPath> GetCheckinAreaDescendantsPath( int rootCheckinAreaGroupTypeId )
        {
            var rootCheckinAreaGroupType = GroupTypeCache.Get( rootCheckinAreaGroupTypeId );
            List<GroupTypeCache> checkinAreaDescendants = GetCheckinAreaDescendants( rootCheckinAreaGroupTypeId ).OrderBy( a => a.Id ).ToList();
            var checkinAreaDescendantsPath = checkinAreaDescendants.Select( a => new CheckinAreaPath( a, rootCheckinAreaGroupType ) ).ToList();
            return checkinAreaDescendantsPath;
        }

        /// <summary>
        /// Gets all checkin area that paths
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CheckinAreaPath> GetAllCheckinAreaPaths()
        {
            List<CheckinAreaPath> result = new List<CheckinAreaPath>();

            // limit to show only GroupTypes that have a group type purpose of Checkin Template
            int groupTypePurposeCheckInTemplateId = DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE ) ).Id;
            var checkinTemplates = GroupTypeCache.All().Where( a => a.GroupTypePurposeValueId == groupTypePurposeCheckInTemplateId ).ToList();

            foreach ( var rootCheckinAreaGroupTypeId in checkinTemplates.Select( a => a.Id ) )
            {
                var checkinAreaDescendantsPath = GetCheckinAreaDescendantsPath( rootCheckinAreaGroupTypeId );
                foreach ( var checkinAreaDescendantPath in checkinAreaDescendantsPath )
                {
                    // just in case multiple checkin areas share a child group type, check for duplicates
                    var alreadyExists = result.Any( x => x.GroupTypeId == checkinAreaDescendantPath.GroupTypeId );
                    if ( !alreadyExists )
                    {
                        result.Add( checkinAreaDescendantPath );
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the checkin area descendants.
        /// </summary>
        /// <param name="parentGroupTypeGuid">The parent group type unique identifier.</param>
        /// <returns></returns>
        public IEnumerable<GroupTypeCache> GetCheckinAreaDescendants( Guid parentGroupTypeGuid )
        {
            return this.GetCheckinAreaDescendants( this.Get( parentGroupTypeGuid ).Id );
        }

        /// <summary>
        /// Gets all related (ancestor, sibling and descendant) check-in areas for the provided check-in area,
        /// based on its ancestor check-in configuration.
        /// </summary>
        /// <param name="checkInArea">The check-in area for which to get related check-in areas.</param>
        /// <returns>All related (ancestor, sibling and descendant) check-in areas for the provided check-in area.</returns>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.16.2" )]
        public List<GroupTypeCache> GetRelatedCheckInAreas( GroupTypeCache checkInArea )
        {
            var checkInConfiguration = this.GetCheckInConfiguration( checkInArea );
            if ( checkInConfiguration == null )
            {
                return null;
            }

            return this.GetCheckinAreaDescendants( checkInConfiguration.Id );
        }

        /// <summary>
        /// Gets the check-in configuration (the first ancestor group type with purpose == "Check-in Template")
        /// for the specified check-in area.
        /// </summary>
        /// <param name="checkInArea">The check-in area for which to get the check-in configuration.</param>
        /// <returns>The check-in configuration for the specified check-in area, or <c>null</c> if not found.</returns>
        /// <remarks>
        ///     <para>
        ///         <strong>This is an internal API</strong> that supports the Rock
        ///         infrastructure and not subject to the same compatibility standards
        ///         as public APIs. It may be changed or removed without notice in any
        ///         release and should therefore not be directly used in any plug-ins.
        ///     </para>
        /// </remarks>
        [RockInternal( "1.16.2" )]
        public GroupTypeCache GetCheckInConfiguration( GroupTypeCache checkInArea )
        {
            var alreadyEncounteredGroupTypeIds = new List<int>();
            return this.FindAncestorCheckInConfiguration( checkInArea, ref alreadyEncounteredGroupTypeIds );
        }

        /// <summary>
        /// Recursively searches the ancestor group type path to find the first one whose purpose == "Check-in Template".
        /// </summary>
        /// <param name="checkInArea">The current [check-in area] group type whose ancestors should be searched.</param>
        /// <param name="alreadyEncounteredGroupTypeIds">The list of group type IDs we've already encountered and searched,
        /// to prevent infinite loops caused by circular references.</param>
        /// <returns>The first ancestor group type whose purpose == "Check-in Template".</returns>
        private GroupTypeCache FindAncestorCheckInConfiguration( GroupTypeCache checkInArea, ref List<int> alreadyEncounteredGroupTypeIds )
        {
            GroupTypeCache checkInConfiguration = null;
            var checkInTemplatePurposeValueId = DefinedValueCache.GetId( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid() );

            foreach ( var parentGroupType in checkInArea.ParentGroupTypes )
            {
                // If we've already encountered this group type, we have a circular reference; continue to the next one.
                if ( alreadyEncounteredGroupTypeIds.Contains( parentGroupType.Id ) )
                {
                    continue;
                }

                // Take note of this group type's ID so we only check it once.
                alreadyEncounteredGroupTypeIds.Add( parentGroupType.Id );

                if ( parentGroupType.GroupTypePurposeValueId == checkInTemplatePurposeValueId )
                {
                    // We found it; set it and break out of this loop.
                    checkInConfiguration = parentGroupType;
                    break;
                }

                // Continue recursively up the group type path.
                checkInConfiguration = this.FindAncestorCheckInConfiguration( parentGroupType, ref alreadyEncounteredGroupTypeIds );

                // If we found it recursively, no need to continue searching.
                if ( checkInConfiguration != null )
                {
                    break;
                }
            }

            return checkInConfiguration;
        }

        #endregion Methods for CheckinAreas (which are GroupTypes)

        #region Obsolete - Replaced with the above GetCheckinAreaDescendant.. methods

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.GroupType">GroupType</see> that are descendants of a specified group type.
        /// WARNING: It is possible for a user to create a circular reference in the GroupTypeAssociation table that will cause this query to get stuck.
        /// The MaxRecursion number will control the depth and prevent this.
        /// </summary>
        /// <param name="parentGroupTypeId">The parent group type identifier.</param>
        /// <param name="maxRecursion">The maximum recursion.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.GroupType">GroupType</see>.
        /// </returns>
        [RockObsolete( "1.12" )]
        [Obsolete( "This is misleading and could cause an exception. It should only be used GroupTypes that are used as Checkin Areas. Use GetCheckinAreaDescendants instead." )]
        public IEnumerable<GroupType> GetAllAssociatedDescendents( int parentGroupTypeId, int maxRecursion )
        {
            /* 2020-09-02 MDP
             * This method is confusing/misleading because it only applies when GroupTypes are used as Checkin Areas.
             * Also, it could cause an circular reference exception
             * To address this issue, we decided to create new GetCheckinAreaDescendants methods to make it more obvious
             * this is only applies to GroupType CheckinAreas, and to make GetCheckinAreaDescendants safe from circular reference problems.
             */

            return this.ExecuteQuery(
                $@"
                WITH CTE ([RecursionLevel], [GroupTypeId], [ChildGroupTypeId])
                AS (
                    SELECT
                          0 AS [RecursionLevel]
                        , [GroupTypeId]
                        , [ChildGroupTypeId]
                    FROM [GroupTypeAssociation]
                    WHERE [GroupTypeId] = {parentGroupTypeId}

                    UNION ALL

                    SELECT acte.[RecursionLevel] + 1 AS [RecursionLevel]
                        , [a].[GroupTypeId]
                        , [a].[ChildGroupTypeId]
                    FROM [GroupTypeAssociation] [a]
                    JOIN CTE acte ON acte.[ChildGroupTypeId] = [a].[GroupTypeId]
                    WHERE acte.[ChildGroupTypeId] <> acte.[GroupTypeId]
                        AND [a].[ChildGroupTypeId] <> acte.[GroupTypeId] -- and the child group type can't be a parent group type
                        AND (acte.RecursionLevel + 1 ) < {maxRecursion}
                )

                SELECT *
                FROM [GroupType]
                WHERE [Id] IN ( SELECT [ChildGroupTypeId] FROM CTE )
                OPTION ( MAXRECURSION {maxRecursion} )" );
        }

        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.GroupType">GroupType</see> that are descendants of a specified group type.
        /// WARNING: This has MAXRECURSION set to 10 to prevent the query from getting stuck on a circular reference in the GroupTypeAssociation table.
        /// </summary>
        /// <param name="parentGroupTypeId">The parent group type identifier.</param>
        /// <returns>
        /// An enumerable collection of <see cref="Rock.Model.GroupType">GroupType</see>.
        /// </returns>
        [RockObsolete( "1.12" )]
        [Obsolete( "This is misleading and could cause an exception. It should only be used GroupTypes are that are used as Checkin Areas. Use GetCheckinAreaDescendants instead." )]
        public IEnumerable<GroupType> GetAllAssociatedDescendents( int parentGroupTypeId )
        {
            /* 2020-09-02 MDP
             * This method is confusing/misleading because it only applies when GroupTypes are used as Checkin Areas.
             * Also, it could cause an circular reference exception
             * To address this issue, we decided to create new GetCheckinAreaDescendants methods to make it more obvious
             * this is only applies to GroupType CheckinAreas, and to make GetCheckinAreaDescendants safe from circular reference problems.
             */

            return GetAllAssociatedDescendents( parentGroupTypeId, 10 );
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
        [RockObsolete( "1.12" )]
        [Obsolete( "This is misleading and could cause an exception. It should only be used GroupTypes are that are used as Checkin Areas. Use GetCheckinAreaDescendants instead." )]
        public IEnumerable<GroupType> GetAllAssociatedDescendentsOrdered( int parentGroupTypeId )
        {
            // We're basically building a hierarchy ordering path using padded zeros of the GroupType's order
            // such that the results of the HierarchyOrder looks something like this:
            //
            //// |000
            //// |000
            //// |001
            //// |002
            //// |002|000
            //// |002|000|000
            //// |002|001
            //// |003
            //// |004

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
				ORDER BY CONVERT(nvarchar(500), CTE.HierarchyOrder + '|' + RIGHT(1000 + GT3.[Order], 3) )",
                parentGroupTypeId );
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
        [RockObsolete( "1.12" )]
        [Obsolete( "This is misleading and could cause an exception. It should only be used GroupTypes are that are used as Checkin Areas. Use GetCheckinAreaDescendants instead." )]
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
                INNER JOIN [GroupType] GT3 ON GT3.[Id] = CTE.[ChildGroupTypeId]",
                parentGroupTypeId );
        }

        /// <summary>
        /// Gets all checkin group type paths.
        /// </summary>
        /// <returns></returns>
        [RockObsolete( "1.12" )]
        [Obsolete( "Use GetAllCheckinAreaPaths instead" )]
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
        [RockObsolete( "1.12" )]
        [Obsolete( "This is misleading and could cause an exception. It should only be used GroupTypes are that are used as Checkin Areas. Use GetCheckinAreaDescendants instead." )]
        public IEnumerable<GroupType> GetAllAssociatedDescendents( Guid parentGroupTypeGuid )
        {
            return this.GetAllAssociatedDescendents( this.Get( parentGroupTypeGuid ).Id );
        }

        #endregion Obsolete

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

        /// <summary>
        /// Gets a list of inactive reasons allowed for the group type
        /// </summary>
        /// <param name="groupTypeId">The group type identifier.</param>
        /// <returns></returns>
        public List<DefinedValueCache> GetInactiveReasonsForGroupType( int groupTypeId )
        {
            return GetInactiveReasonsForGroupType( GroupTypeCache.Get( groupTypeId ).Guid );
        }

        /// <summary>
        /// Gets a list of inactive reasons allowed for the group type
        /// </summary>
        /// <param name="groupTypeGuid">The group type unique identifier.</param>
        /// <returns></returns>
        public List<DefinedValueCache> GetInactiveReasonsForGroupType( Guid groupTypeGuid )
        {
            var inactiveDefinedTypeGuid = Rock.SystemGuid.DefinedType.GROUPTYPE_INACTIVE_REASONS.AsGuid();
            string key = Rock.SystemKey.GroupTypeAttributeKey.INACTIVE_REASONS_GROUPTYPE_FILTER;

            return DefinedTypeCache.Get( inactiveDefinedTypeGuid )
                .DefinedValues
                .Where( r => !r.GetAttributeValues( key ).Any() || r.GetAttributeValues( key ).Contains( groupTypeGuid.ToString() ) )
                .ToList();
        }

        #region Private Methods

        /// <summary>
        /// Builds the checkin area descendants.
        /// </summary>
        /// <param name="parentCheckinAreaGroupType">Type of the parent checkin area group.</param>
        /// <param name="checkinAreaDescendants">The checkin area descendants.</param>
        private void BuildCheckinAreaDescendants( GroupTypeCache parentCheckinAreaGroupType, ref List<GroupTypeCache> checkinAreaDescendants )
        {
            // get list of child group types (checkin areas) that aren't the same group type as the parent
            var childGroupTypeList = parentCheckinAreaGroupType.ChildGroupTypes
                .Where( a => a.Id != parentCheckinAreaGroupType.Id )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();

            foreach ( var childGroupType in childGroupTypeList )
            {
                if ( checkinAreaDescendants.Any( x => x.Id == childGroupType.Id ) )
                {
                    // if we already saw this groupTypeId, we got a circular reference. So just return the path so far
                    continue;
                }

                checkinAreaDescendants.Add( childGroupType );
                BuildCheckinAreaDescendants( childGroupType, ref checkinAreaDescendants );
            }
        }

        #endregion
    }
}
