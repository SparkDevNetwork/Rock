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
    /// <summary>
    /// The original Child and Parent known relationships were not marked IsSystem.
    /// If they don't exist we need to create them again, if they still exist
    /// then we mark them as IsSystem=true.
    /// </summary>
    public partial class RestoreChildAndParentKnownRelationships : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
DECLARE @InverseRelationshipAttributeId INT = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'c91148d9-d663-493a-86e8-5000bd281852')
DECLARE @LastOrder INT = ISNULL((SELECT MAX([GTR].[Order]) FROM [GroupTypeRole] AS [GTR] INNER JOIN [GroupType] AS [GT] ON [GT].[Id] = [GTR].[GroupTypeId] WHERE [GT].[Guid] = 'E0C5A0E2-B7B3-4EF4-820D-BBF7F9A374EF'), 0)
DECLARE @ChildRoleGuid UNIQUEIDENTIFIER = 'f87df00f-e86d-4771-a3ae-dbf79b78cf5d'
DECLARE @ParentRoleGuid UNIQUEIDENTIFIER = '6f3fadc4-6320-4b54-9cf6-02ef9586a660'

IF NOT EXISTS (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = @ChildRoleGuid)
BEGIN
    INSERT INTO [GroupTypeRole]
        ([IsSystem], [Guid], [GroupTypeId], [Name], [Description], [Order], [IsLeader])
        VALUES
        (1, @ChildRoleGuid, 11, 'Child', 'An adult child of the owner of this known relationship group', @LastOrder + 1, 0)

    DECLARE @ChildRoleId INT = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = @ChildRoleGuid)

    INSERT INTO [AttributeValue]
        ([AttributeId], [EntityId], [Value], [IsSystem], [Guid])
        VALUES
        (@InverseRelationshipAttributeId, @ChildRoleId, @ParentRoleGuid, 0, NEWID())
END

IF NOT EXISTS (SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = @ParentRoleGuid)
BEGIN
    INSERT INTO [GroupTypeRole]
        ([IsSystem], [Guid], [GroupTypeId], [Name], [Description], [Order], [IsLeader])
        VALUES
        (1, @ParentRoleGuid, 11, 'Parent', 'The parent of the owner of this known relationship group', @LastOrder + 2, 0)

    DECLARE @ParentRoleId INT = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = @ParentRoleGuid)

    INSERT INTO [AttributeValue]
        ([AttributeId], [EntityId], [Value], [IsSystem], [Guid])
        VALUES
        (@InverseRelationshipAttributeId, @ParentRoleId, @ChildRoleGuid, 0, NEWID())
END

UPDATE [GroupTypeRole] SET [IsSystem] = 1 WHERE [Guid] IN (@ChildRoleGuid, @ParentRoleGuid)
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
