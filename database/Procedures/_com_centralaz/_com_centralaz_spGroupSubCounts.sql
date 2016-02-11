
/*
<doc>
	<summary>
		This procedure can be used to provide child group sub totals of 
		all active descendant groups of the given type and active 
		group members that exist under each of the given root group's
		child groups.

		It's handy to use this with a Dynamic Data Block on the
		Group Viewer page to see these rolled-up stats for the 
		immediate child groups of the selected group.
	</summary>

	<returns>
		* GroupId
		* ParentGroupId
		* Name (of Group)
		* Groups (int; rolled up number of groups)
		* Members (int; rolled up number of members)
	</returns>
	<param name='GroupId' datatype='int'>The group id of the root group you're reporting on.</param>
	<param name='GroupTypeId' datatype='int'>The group type id of the child groups you want included in the statistics.</param>
	<remarks>	
		Example Lava code
           {% for row in rows %} 
           <tr>
              <td>{{ row.Name }}</td><td>{{ row.Groups }}</td><td>{{ row.Members }}</td>
           </tr>
           {% endfor %}
	</remarks>
	<code>
		EXEC [dbo].[_com_centralaz_spGroupSubCounts] 174064, 42 -- groups under Life Groups that are of type Life Group
	</code>
</doc>
*/

IF EXISTS (select * from dbo.sysobjects where id = object_id(N'[dbo].[_com_centralaz_spGroupSubCounts]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[_com_centralaz_spGroupSubCounts]
GO

CREATE            PROC [dbo].[_com_centralaz_spGroupSubCounts]
@GroupId int
,@GroupTypeId int
AS

;WITH ChildrenCTE AS (
  SELECT  RootID = G.ID, ID = G.ID, G.GroupTypeId, G.IsActive
  FROM    [Group] G
  WHERE ParentGroupId = @GroupId
  UNION ALL
  SELECT  CTE.RootID, g.ID, g.GroupTypeId, g.IsActive
  FROM    ChildrenCTE CTE
          INNER JOIN [Group] g ON g.ParentGroupId = CTE.ID
)
SELECT  g.ID, g.ParentGroupId, g.Name, cnt.Groups, mmr.Members
FROM    [Group] g
        INNER JOIN (
          SELECT  ID = RootID, Groups = COUNT(DISTINCT ID)
          FROM    ChildrenCTE
		  WHERE GroupTypeId = @GroupTypeId
		  AND IsActive = 1
          GROUP BY RootID
        ) cnt ON cnt.ID = g.ID
        INNER JOIN (
          SELECT  RootID, Members = COUNT(1)
          FROM    ChildrenCTE
		  INNER JOIN [GroupMember] GM ON GM.GroupId = ChildrenCTE.ID
		  WHERE GroupTypeId = @GroupTypeId
		  AND IsActive = 1 AND GM.GroupMemberStatus = 1
          GROUP BY RootID
        ) mmr ON mmr.RootID = g.ID
AND g.IsActive = 1
ORDER BY Name

GO


