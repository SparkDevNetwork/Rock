using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock;
using Rock.Plugin;
using Rock.Web.Cache;
namespace com.centralaz.Baptism.Migrations
{
    [MigrationNumber( 4, "1.3.0" )]
    public class AddCampusCategoriesSchedulesAndGroups : Migration
    {
        public override void Up()
        {
            // Add
            // Under Schedules->Baptism Blackout Times, add "<Campus> Blackout" categories
            // Under Schedules->Service Times, add "<Campus> Service Times" categories
            Sql( @"
-- Baptism Schedule stuff
DECLARE @Now DateTime = GetDate();
DECLARE @BlackoutParentCategoryId int = (SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = 'FFC06491-1BE9-4F3B-AC76-81A47E17D0AE' )
DECLARE @ServiceTimesParentCategoryId int = (SELECT TOP 1 [Id] FROM [Category] WHERE [Name] = 'Service Times' AND [ParentCategoryId] IS NULL)
DECLARE @ScheduleEntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '0B2C38A7-D79C-4F85-9757-F1B045D32C8A')

-- Add Campus Blackout Categories: <Campus> Blackout
INSERT INTO [Category]
			([IsSystem]
			,[ParentCategoryId]
			,[EntityTypeId]
			,[EntityTypeQualifierColumn]
			,[EntityTypeQualifierValue]
			,[Name]
			,[IconCssClass]
			,[Guid]
			,[Order]
			,[Description]
			,[CreatedDateTime]
			,[ModifiedDateTime])
	SELECT
			0
			,@BlackoutParentCategoryId
			,@ScheduleEntityTypeId
			,N''
			,N''
			,[Name] + ' Blackout'
			,N'fa fa-ban'
			,NewId()
			,0
			,NULL
			,@Now
			,@Now
  FROM 
     [Campus] Ca
  WHERE
     [IsSystem] = 0
     AND NOT EXISTS (SELECT * 
                     FROM [Category] C
                     WHERE C.[Name] = Ca.[Name] + ' Blackout' )


-- Add Campus Service Time Categories: <Campus> Service Times
INSERT INTO [Category]
			([IsSystem]
			,[ParentCategoryId]
			,[EntityTypeId]
			,[EntityTypeQualifierColumn]
			,[EntityTypeQualifierValue]
			,[Name]
			,[IconCssClass]
			,[Guid]
			,[Order]
			,[Description]
			,[CreatedDateTime]
			,[ModifiedDateTime])
	SELECT
			0
			,@BlackoutParentCategoryId
			,@ScheduleEntityTypeId
			,N''
			,N''
			,[Name] + ' Service Times'
			,N'fa fa-bell'
			,NewId()
			,0
			,NULL
			,@Now
			,@Now
  FROM 
     [Campus] Ca
  WHERE
     [IsSystem] = 0
     AND NOT EXISTS ( SELECT * 
                     FROM [Category] C
                     WHERE C.[Name] = Ca.[Name] + ' Service Times' )


-- Add Campus Baptism Schedule Groups: <Campus> Baptism Schedule
DECLARE @BaptismLocationsGroupTypeId int = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '32F8592C-AE11-44A7-A053-DE43789811D9' )

INSERT INTO [Group]
    ([IsSystem], [GroupTypeId], [CampusId], [Name], [IsSecurityRole], [IsActive], [Order], [Guid])
    SELECT
        0, @BaptismLocationsGroupTypeId, [Id], [Name] + ' Baptism Schedule', 0, 1, 0, NewId()
  FROM 
     [Campus] Ca
  WHERE
     [IsSystem] = 0
     AND NOT EXISTS ( SELECT * 
                     FROM [Group] G
                     WHERE G.[Name] = Ca.[Name] + ' Baptism Schedule' )

-- Configure each new Campus Baptism Schedule Group attributes for
-- 'Blackout Dates' and 'Service Times'
DECLARE @BlackoutDatesAttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'D58F0DB5-09AA-4A5A-BC17-CE3E3985D6F8')
DECLARE @ServiceTimesAttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'B7371337-57CB-4CB3-994C-72258729950F')

-- Blackout
INSERT INTO [AttributeValue]
    ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
  SELECT
        0, @BlackoutDatesAttributeId, G.[Id], Cat.[Guid], NewId()
  FROM 
     [Group] G
  INNER JOIN [Campus] C ON C.Id = G.CampusId
  INNER JOIN [Category] Cat ON Cat.Name = C.Name + ' Blackout'
  WHERE
     G.[IsSystem] = 0
	 AND G.[Name] = C.Name + ' Baptism Schedule'
     AND NOT EXISTS ( SELECT * 
                     FROM [AttributeValue] AV
                     WHERE AV.[EntityId] = G.[Id]
					 AND AV.[AttributeId] = @BlackoutDatesAttributeId )

-- Service Times
INSERT INTO [AttributeValue]
    ( [IsSystem], [AttributeId], [EntityId], [Value], [Guid] )
  SELECT
        0, @ServiceTimesAttributeId, G.[Id], Cat.[Guid], NewId()
  FROM 
     [Group] G
  INNER JOIN [Campus] C ON C.Id = G.CampusId
  INNER JOIN [Category] Cat ON Cat.Name = C.Name + ' Service Times'
  WHERE
     G.[IsSystem] = 0
	 AND G.[Name] = C.Name + ' Baptism Schedule'
     AND NOT EXISTS ( SELECT * 
                     FROM [AttributeValue] AV
                     WHERE AV.[EntityId] = G.[Id]
					 AND AV.[AttributeId] = @ServiceTimesAttributeId )
" );

            // Under Group Viewer -> add "<Campus> Baptism Schedule" group of type "Baptism Location" for each campus

            // 
        }

        public override void Down()
        {
        }
    }
}