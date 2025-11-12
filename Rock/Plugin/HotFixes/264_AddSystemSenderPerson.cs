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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 264, "19.0" )]
    public class AddSystemSenderPerson : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JPH_AddSystemSenderPerson_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            JPH_AddSystemSenderPerson_Down();
        }

        /// <summary>
        /// JPH: Add 'System Sender' person - up.
        /// </summary>
        private void JPH_AddSystemSenderPerson_Up()
        {
            Sql( $@"
IF NOT EXISTS (
    SELECT 1
    FROM [Person]
    WHERE [Guid] = '{SystemGuid.Person.SYSTEM_SENDER}'
)
BEGIN
    DECLARE @PersonRecordTypePersonValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '{SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON}')
        , @PersonRecordStatusActiveValueId INT = (SELECT TOP 1 [Id] from [DefinedValue] WHERE [Guid] = '{SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE}')
        , @PersonConnectionStatusParticipantValueId INT = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '{SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PARTICIPANT}')
        , @FamilyGroupTypeId INT = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '{SystemGuid.GroupType.GROUPTYPE_FAMILY}')
        , @AdultGroupTypeRoleId INT = (SELECT TOP 1 [Id] FROM [GroupTypeRole] WHERE [Guid] = '{SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT}')
        , @SystemPersonGuid UNIQUEIDENTIFIER = '{SystemGuid.Person.SYSTEM_SENDER}'
        , @SystemPersonId INT
        , @SystemFamilyGroupId INT
        , @Now DATETIME = (SELECT GETDATE());

    -- Create person.
    INSERT INTO [Person]
    (
        [IsSystem]
        , [RecordTypeValueId]
        , [RecordStatusValueId]
        , [ConnectionStatusValueId]
        , [IsDeceased]
        , [FirstName]
        , [NickName]
        , [LastName]
        , [Gender]
        , [IsEmailActive]
        , [Guid]
        , [CreatedDateTime]
        , [AgeClassification]
    )
    VALUES
    (
        1                                               --IsSystem
        , @PersonRecordTypePersonValueId                --RecordTypeValueId
        , @PersonRecordStatusActiveValueId              --RecordStatusValueId
        , @PersonConnectionStatusParticipantValueId     --ConnectionStatusValueId
        , 0                                             --IsDeceased
        , 'System'                                      --FirstName
        , 'System'                                      --NickName
        , 'Sender'                                      --LastName
        , 0                                             --Gender (Unknown)
        , 0                                             --IsEmailActive
        , @SystemPersonGuid                             --Guid
        , @Now                                          --CreatedDateTime
        , 1                                             --AgeClassification (Adult)
    );

    SET @SystemPersonId = SCOPE_IDENTITY();

    -- Create primary alias.
    INSERT INTO [PersonAlias]
    (
        [PersonId]
        , [AliasPersonId]
        , [AliasPersonGuid]
        , [Guid]
    )
    VALUES
    (
        @SystemPersonId
        , @SystemPersonId
        , @SystemPersonGuid
        , NEWID()
    );

    -- Create family.
    INSERT INTO [Group]
    (
        [IsSystem]
        , [GroupTypeId]
        , [Name]
        , [IsSecurityRole]
        , [IsActive]
        , [Order]
        , [Guid]
        , [CreatedDateTime]
    )
    VALUES
    (
        1                                               --IsSystem
        , @FamilyGroupTypeId                            --GroupTypeId
        , 'Sender Family'                               --Name
        , 0                                             --IsSecurityRole
        , 1                                             --IsActive
        , 0                                             --Order
        , NEWID()                                       --Guid
        , @Now                                          --CreatedDateTime
    );

    SET @SystemFamilyGroupId = SCOPE_IDENTITY();

    -- Create family member.
    INSERT INTO [GroupMember] (
        [IsSystem]
        , [GroupId]
        , [PersonId]
        , [GroupRoleId]
        , [GroupMemberStatus]
        , [Guid]
        , [CreatedDateTime]
        , [DateTimeAdded]
        , [GroupTypeId]
       )
    VALUES (
        1                                               --IsSystem
        , @SystemFamilyGroupId                          --GroupId
        , @SystemPersonId                               --PersonId
        , @AdultGroupTypeRoleId                         --GroupRoleId
        , 1                                             --GroupMemberStatus (Active)
        , NEWID()                                       --Guid
        , @Now                                          --CreatedDateTime
        , @Now                                          --DateTimeAdded
        , @FamilyGroupTypeId                            --GroupTypeId
    );
END" );
        }

        /// <summary>
        /// JPH: Add 'System Sender' person - down.
        /// </summary>
        private void JPH_AddSystemSenderPerson_Down()
        {
            Sql( $@"
UPDATE [Person]
SET [PrimaryFamilyId] = NULL
WHERE [Guid] = '{SystemGuid.Person.SYSTEM_SENDER}';

DELETE
FROM [Group]
WHERE [Id] IN (
    SELECT [GroupId]
    FROM [GroupMember]
    WHERE [PersonId] IN (
        SELECT [Id]
        FROM [Person]
        WHERE [Guid] = '{SystemGuid.Person.SYSTEM_SENDER}'
    )
);

DELETE
FROM [PersonAlias]
WHERE [PersonId] IN (
    SELECT [Id]
    FROM [Person]
    WHERE [Guid] = '{SystemGuid.Person.SYSTEM_SENDER}'
);

DELETE
FROM [Person]
WHERE [Guid] = '{SystemGuid.Person.SYSTEM_SENDER}';" );
        }
    }
}