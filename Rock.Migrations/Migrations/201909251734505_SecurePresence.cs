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
    
    /// <summary>
    ///
    /// </summary>
    public partial class SecurePresence : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RestSecurePresenceUser();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

        /// <summary>
        /// ED: Presence User Security Changes
        /// </summary>
        private void RestSecurePresenceUser()
        {
            Sql( @"
                IF NOT EXISTS (SELECT [Id] FROM [RestController] WHERE [ClassName] = 'Rock.Rest.Controllers.AttendancesController') 
                BEGIN
                    INSERT INTO [RestController] ( [Name], [ClassName], [Guid] )
                    VALUES ( 'Attendances', 'Rock.Rest.Controllers.AttendancesController', NEWID() )
                END

                IF NOT EXISTS (SELECT [Id] FROM [RestController] WHERE [ClassName] = 'Rock.Rest.Controllers.GroupsController') 
                BEGIN
                    INSERT INTO [RestController] ( [Name], [ClassName], [Guid] )
                    VALUES ( 'Groups', 'Rock.Rest.Controllers.GroupsController', NEWID() )
                END

                IF NOT EXISTS (SELECT [Id] FROM [RestController] WHERE [ClassName] = 'Rock.Rest.Controllers.PersonAliasController') 
                BEGIN
                    INSERT INTO [RestController] ( [Name], [ClassName], [Guid] )
                    VALUES ( 'PersonAlias', 'Rock.Rest.Controllers.PersonAliasController', NEWID() )
                END

                IF NOT EXISTS (SELECT [Id] FROM [RestAction] WHERE [ApiId] = 'GETapi/Groups/GroupTypeCheckinConfiguration/{groupTypeGuid}')
                BEGIN
                    INSERT INTO [RestAction] ( [ControllerId], [Method], [ApiId], [Path], [Guid] )
                    SELECT [Id], 'GET', 'GETapi/Groups/GroupTypeCheckinConfiguration/{groupTypeGuid}', 'api/Groups/GroupTypeCheckinConfiguration/{groupTypeGuid}', NEWID()
                    FROM [RestController] WHERE [ClassName] = 'Rock.Rest.Controllers.GroupsController'
                END

                IF NOT EXISTS (SELECT [Id] FROM [RestAction] WHERE [ApiId] = 'GETapi/PersonAlias') 
                BEGIN
                    INSERT INTO [RestAction] ( [ControllerId], [Method], [ApiId], [Path], [Guid] )
                    SELECT [Id], 'GET', 'GETapi/PersonAlias', 'api/PersonAlias', NEWID() 
                    FROM [RestController] WHERE [ClassName] = 'Rock.Rest.Controllers.PersonAliasController'
                END

                IF NOT EXISTS (SELECT [Id] FROM [RestAction] WHERE [ApiId] = 'GETapi/PersonAlias/{id}') 
                BEGIN
                    INSERT INTO [RestAction] ( [ControllerId], [Method], [ApiId], [Path], [Guid] )
                    SELECT [Id], 'GET', 'GETapi/PersonAlias/{id}', 'api/PersonAlias/{id}', NEWID()
                    FROM [RestController] WHERE [ClassName] = 'Rock.Rest.Controllers.PersonAliasController'
                END

                IF NOT EXISTS (SELECT [Id] FROM [RestAction] WHERE [ApiId] = 'POSTapi/Attendances/Import') 
                BEGIN
                    INSERT INTO [RestAction] ( [ControllerId], [Method], [ApiId], [Path], [Guid] )
                    SELECT [Id], 'POST', 'POSTapi/Attendances/Import', 'api/Attendances/Import', NEWID()
                    FROM [RestController] WHERE [ClassName] = 'Rock.Rest.Controllers.AttendancesController'
                END

                IF NOT EXISTS (SELECT [Id] FROM [RestAction] WHERE [ApiId] = 'PUTapi/Attendances/AddAttendance?groupId={groupId}&locationId={locationId}&scheduleId={scheduleId}&occurrenceDate={occurrenceDate}&personId={personId}&personAliasId={personAliasId}') 
                BEGIN
                    INSERT INTO [RestAction] ( [ControllerId], [Method], [ApiId], [Path], [Guid] )
                        SELECT [Id], 'PUT', 'PUTapi/Attendances/AddAttendance?groupId={groupId}&locationId={locationId}&scheduleId={scheduleId}&occurrenceDate={occurrenceDate}&personId={personId}&personAliasId={personAliasId}', 'api/Attendances/AddAttendance?groupId={groupId}&locationId={locationId}&scheduleId={scheduleId}&occurrenceDate={occurrenceDate}&personId={personId}&personAliasId={personAliasId}', NEWID()
                        FROM [RestController] WHERE [ClassName] = 'Rock.Rest.Controllers.AttendancesController'
                END

                DECLARE @RestActionEntityTypeId INT = (SELECT [Id] FROM [EntityType] WHERE [Guid] = 'D4F7F055-5351-4ADF-9F8D-4802CAD6CC9D')
                DECLARE @GetPersonAliasIdRestActionId INT = (SELECT [Id] FROM [RestAction] WHERE [ApiId] = 'GETapi/PersonAlias/{id}')
                DECLARE @GetPersonAliasRestActionId INT = (SELECT [Id] FROM [RestAction] WHERE [ApiId] = 'GETapi/PersonAlias')
                DECLARE @GetGroupTypeCheckinConfigurationId INT = (SELECT [Id] FROM [RestAction] WHERE [ApiId] = 'GETapi/Groups/GroupTypeCheckinConfiguration/{groupTypeGuid}')
                DECLARE @PutAddAttendanceId INT = (SELECT [Id] FROM [RestAction] WHERE [ApiId] = 'PUTapi/Attendances/AddAttendance?groupId={groupId}&locationId={locationId}&scheduleId={scheduleId}&occurrenceDate={occurrenceDate}&personId={personId}&personAliasId={personAliasId}')
                DECLARE @PostAttendancesImportId INT = (SELECT [Id] FROM [RestAction] WHERE [ApiId] = 'POSTapi/Attendances/Import')
                DECLARE @presencePersonAliasId INT = (SELECT [Id] FROM [PersonAlias] WHERE [AliasPersonGuid] = '86CF11D9-66BC-4CE0-9037-F8AFCBCD608A')

                -- If there is already user defined security on GETapi/PersonAlias/{id} don't change it.
                IF NOT EXISTS(SELECT * FROM Auth WHERE EntityTypeId = @RestActionEntityTypeId AND EntityId = @GetPersonAliasIdRestActionId)
                BEGIN
                    INSERT INTO [Auth] ([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid], [PersonAliasId]) 
                        VALUES 
                            (@RestActionEntityTypeId, @GetPersonAliasIdRestActionId, 0, 'View', 'A', 0, NULL, 'CF437995-FE0F-4639-9324-1BB789919E52', @presencePersonAliasId),
                            (@RestActionEntityTypeId, @GetPersonAliasIdRestActionId, 0, 'Edit', 'D', 0, NULL, 'C7F7DA0C-9DE7-456B-B47F-64C8B4C067B4', @presencePersonAliasId)
                END

                -- If there is already user defined security on GETapi/PersonAlias don't change it.
                IF NOT EXISTS(SELECT * FROM Auth WHERE EntityTypeId = @RestActionEntityTypeId AND EntityId = @GetPersonAliasRestActionId)
                BEGIN
                    INSERT INTO [Auth] ([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid], [PersonAliasId]) 
                        VALUES 
                            (@RestActionEntityTypeId, @GetPersonAliasRestActionId, 0, 'View', 'A', 0, NULL, 'CECDFE27-4B65-4994-B0F8-008537CAFDF3', @presencePersonAliasId),
                            (@RestActionEntityTypeId, @GetPersonAliasRestActionId, 0, 'Edit', 'D', 0, NULL, 'E359ADEB-DEF5-49AE-8302-3BA25D26BF5C', @presencePersonAliasId)
                END

                -- If there is already user defined security on GETapi/Groups/GroupTypeCheckinConfiguration/{groupTypeGuid} don't change it.
                IF NOT EXISTS(SELECT * FROM Auth WHERE EntityTypeId = @RestActionEntityTypeId AND EntityId = @GetGroupTypeCheckinConfigurationId)
                BEGIN
                    INSERT INTO [Auth] ([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid], [PersonAliasId]) 
                        VALUES
                            (@RestActionEntityTypeId, @GetGroupTypeCheckinConfigurationId, 0, 'View', 'A', 0, NULL, '7D0EE1A2-CE6A-43A6-85A9-9D9EF3F587DF', @presencePersonAliasId),
                            (@RestActionEntityTypeId, @GetGroupTypeCheckinConfigurationId, 0, 'Edit', 'D', 0, NULL, '9025319E-E4ED-4AD3-8F17-4D60E297557F', @presencePersonAliasId)
                END

                -- If there is already user defined security on PUTapi/AddAttendance?... don't change it.
                IF NOT EXISTS(SELECT * FROM Auth WHERE EntityTypeId = @RestActionEntityTypeId AND EntityId = @PutAddAttendanceId)
                BEGIN
                    INSERT INTO [Auth] ([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid], [PersonAliasId]) 
                        VALUES
                            (@RestActionEntityTypeId, @PutAddAttendanceId, 0, 'View', 'A', 0, NULL, 'F4040219-3837-4AD8-B4DD-745B63C3797A', @presencePersonAliasId),
                            (@RestActionEntityTypeId, @PutAddAttendanceId, 0, 'Edit', 'A', 0, NULL, '8D290EA6-53D3-4FA2-B95F-957E9A75F94E', @presencePersonAliasId)
                END

                -- If there is already user defined security on POSTapi/Attendances/Import don't change it.
                IF NOT EXISTS(SELECT * FROM Auth WHERE EntityTypeId = @RestActionEntityTypeId AND EntityId = @PostAttendancesImportId)
                BEGIN
                    INSERT INTO [Auth] ([EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny], [SpecialRole], [GroupId], [Guid], [PersonAliasId]) 
                        VALUES
                            (@RestActionEntityTypeId, @PostAttendancesImportId, 0, 'View', 'A', 0, NULL, '0D8711EC-453D-4B0D-8A93-E09E41B215A8', @presencePersonAliasId),
                            (@RestActionEntityTypeId, @PostAttendancesImportId, 0, 'Edit', 'A', 0, NULL, 'E5497FA8-DEDF-4896-B312-6470BD631691', @presencePersonAliasId)
                END" );
        }
    }
}
