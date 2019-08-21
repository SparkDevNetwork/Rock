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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using Rock.Model;

    /// <summary>
    ///
    /// </summary>
    public partial class AddWeekendServicesCheckin : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddGroupType( "Service Attendance", "Used for tracking the attendance for people who attend the 'weekend' or 'weekly' service.", "Group", "Member", false, false, false, "fa fa-walking", 0, null, 0, "4A406CB0-495B-4795-B788-52BDFDE00B01", "77713830-AE5E-4B1A-94FA-E145DFF85035", true );
            Sql( string.Format(@"
                DECLARE @GroupTypeId INT = (SELECT [Id] FROM [GroupType] WHERE [Guid]='77713830-AE5E-4B1A-94FA-E145DFF85035')

                UPDATE
                    [GroupType]
                SET [AttendanceCountsAsWeekendService] = 1,
                    [GroupViewLavaTemplate] = '{0}'
                WHERE [Id] = @GroupTypeId

                DECLARE @AttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Key]='core_checkin_PreventDuplicateCheckin')

                IF NOT EXISTS(SELECT [Id] FROM [dbo].[AttributeValue] WHERE [AttributeId] = @AttributeId AND [EntityId] = @GroupTypeId)
                                BEGIN
                                    INSERT INTO [AttributeValue] (
                                          [IsSystem]
		                                , [AttributeId]
		                                , [EntityId]
		                                , [Value]
		                                , [Guid])
                                    VALUES(
                                          1
		                                , @AttributeId
		                                , @GroupTypeId
		                                , 'True'
		                                , 'A27A7E68-7FBA-4648-B71D-FAA324752850')
                                END
",new GroupType().GroupViewLavaTemplate.Replace( "'", "''" ) ) );
            RockMigrationHelper.AddGroupType( "Services", "", "Group", "Member", true, false, false, "", 0, "6E7AD783-7614-4721-ABC1-35842113EF59", 0, null, "235BAE2B-5760-4763-AADF-3938F34BA100", true );

            Sql( string.Format( @"
    DECLARE @AttendanceServiceGroupTypeId int = ( SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '77713830-AE5E-4B1A-94FA-E145DFF85035' )
    DECLARE @ServicesGroupTypeId int = ( SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '235BAE2B-5760-4763-AADF-3938F34BA100' )
        IF @ServicesGroupTypeId IS NOT NULL
        BEGIN
            INSERT INTO [GroupTypeAssociation] ( [GroupTypeId], [ChildGroupTypeId] )
	        VALUES ( @AttendanceServiceGroupTypeId, @ServicesGroupTypeId )
        END

   -- Set AttendanceCountsAsWeekendService to true for Services
      UPDATE
           [GroupType]
      SET [AttendanceCountsAsWeekendService] = 1,[GroupViewLavaTemplate] = '{0}'
      WHERE [Id] = @ServicesGroupTypeId

    -- Add a default 'Member' role to any group type that does not have any roles
    INSERT INTO [GroupTypeRole] ( [IsSystem], [GroupTypeId], [Name], [Description], [Order], [IsLeader], [Guid], [CanView], [CanEdit] )
    SELECT 0, [Id], 'Member', 'Member of group', 0, 0, NEWID(), 0, 0 
    FROM [GroupType] WHERE [Id] NOT IN (
        SELECT [GroupTypeId] FROM [GroupTypeRole]
    )
    -- Set the group type's default role to the newly created group type role
	UPDATE T SET [DefaultGroupRoleId] = R.[Id]
	FROM [GroupType] T
	INNER JOIN [GroupTypeRole] R ON R.[GroupTypeId] = T.[Id] AND R.[Name] = 'Member'
	WHERE T.[DefaultGroupRoleId] IS NULL

    --Add Weekend Service Group
    DECLARE @GroupTypeId INT = (SELECT [Id] FROM [GroupType] WHERE [Guid]='235BAE2B-5760-4763-AADF-3938F34BA100')

    INSERT INTO
	     [Group]
     ([GroupTypeId], [Name],[IsSystem],[IsActive],[IsSecurityRole], [Guid], [IsPublic],[Order])
    VALUES        (@GroupTypeId,'Weekend Service',1,1,0,'E000800B-B358-416F-BCD5-90C4CAC65AA3',1,0)

    --Add Weekend Service Group Locations
    DECLARE @GroupId INT = (SELECT [Id] FROM [Group] WHERE [Guid]='E000800B-B358-416F-BCD5-90C4CAC65AA3')
    IF @GroupId IS NOT NULL
    BEGIN
        INSERT INTO [GroupLocation]
        ([LocationId],[GroupId],[IsMailingLocation],[IsMappedLocation], [Guid])
        SELECT [LocationId], @GroupId, 0 , 0, NEWID()
        FROM [Campus] 
        WHERE [IsActive] = 1
        GROUP BY [LocationId] ORDER BY MAX([Name])
    END
", new GroupType().GroupViewLavaTemplate.Replace( "'", "''" ) ) );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteGroup( "E000800B-B358-416F-BCD5-90C4CAC65AA3");
            Sql( @"
UPDATE
    [GroupType]
SET [DefaultGroupRoleId] = NULL
WHERE [Guid] IN ('235BAE2B-5760-4763-AADF-3938F34BA100','77713830-AE5E-4B1A-94FA-E145DFF85035')
" );
            RockMigrationHelper.DeleteGroupType( "235BAE2B-5760-4763-AADF-3938F34BA100" );
            Sql( @"DELETE FROM [AttributeValue] WHERE [Guid]='a27a7e68-7fba-4648-b71d-faa324752850'" );
            RockMigrationHelper.DeleteGroupType( "77713830-AE5E-4B1A-94FA-E145DFF85035" );
        }
    }
}
