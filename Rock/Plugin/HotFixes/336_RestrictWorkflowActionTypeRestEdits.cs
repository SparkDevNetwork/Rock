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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Restricts inherited Edit access to the v1 workflow action type REST controller while preserving custom security.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 336, "17.8" )]
    public class RestrictWorkflowActionTypeRestEdits : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /*
                10/25/2026 - MSE

                Workflow action type writes inherit staff controller permissions by default.
                Append an administrator allowance and a terminal denial only when this controller
                has no terminal Edit rule. Existing specific rules and action rules continue to win.
                Register a missing controller by its generated identity so a fresh installation
                cannot miss this restriction before controller discovery runs.

                Reason: Creating workflow actions must not inherit general staff REST Edit access.
            */
            Sql( @"
DECLARE @ControllerClassName NVARCHAR(500) = 'Rock.Rest.Controllers.WorkflowActionTypesController';
DECLARE @ControllerGuid UNIQUEIDENTIFIER = 'E8C004E7-86F1-4AC0-9B43-C7F381ECD4BA';
DECLARE @RestControllerEntityTypeId INT = ( SELECT [Id] FROM [EntityType] WHERE [Guid] = '65CDFD5B-A9AA-48FA-8D22-669612D5EA7D' );
DECLARE @AdministrationGroupId INT = ( SELECT [Id] FROM [Group] WHERE [Guid] = '628C51A8-4613-43ED-A18D-4A6FB999273E' );

IF @RestControllerEntityTypeId IS NULL OR @AdministrationGroupId IS NULL
BEGIN
    THROW 51000, 'Workflow action REST security requires the RestController entity type and Rock Administration role.', 1;
END;

IF ( SELECT COUNT(*) FROM [RestController] WHERE [ClassName] = @ControllerClassName ) > 1
BEGIN
    THROW 51000, 'Workflow action REST security found ambiguous controller registrations.', 1;
END;

DECLARE @ControllerId INT = ( SELECT [Id] FROM [RestController] WHERE [ClassName] = @ControllerClassName );

IF EXISTS
(
    SELECT 1 FROM [RestController]
    WHERE [Guid] = @ControllerGuid
        AND ( @ControllerId IS NULL OR [Id] <> @ControllerId )
)
BEGIN
    THROW 51000, 'Workflow action REST security found a conflicting controller identifier.', 1;
END;

IF @ControllerId IS NULL
BEGIN
    INSERT INTO [RestController] ( [Name], [ClassName], [Guid] )
    VALUES ( 'WorkflowActionTypes', @ControllerClassName, @ControllerGuid );

    SET @ControllerId = CONVERT( INT, SCOPE_IDENTITY() );
END;

IF NOT EXISTS
(
    SELECT 1 FROM [Auth]
    WHERE [EntityTypeId] = @RestControllerEntityTypeId
        AND [EntityId] = @ControllerId
        AND [Action] = 'Edit'
        AND [SpecialRole] = 1
)
BEGIN
    DECLARE @AdminAllowGuid UNIQUEIDENTIFIER = 'A7FF7224-1764-4532-9497-9715FF8D1646';
    DECLARE @AllUsersDenyGuid UNIQUEIDENTIFIER = 'B11BBE93-A9C4-430F-A489-38463F51286C';
    DECLARE @NeedsAdminAllow BIT = CASE WHEN EXISTS
    (
        SELECT 1 FROM [Auth]
        WHERE [EntityTypeId] = @RestControllerEntityTypeId
            AND [EntityId] = @ControllerId
            AND [Action] = 'Edit'
            AND [AllowOrDeny] = 'A'
            AND [SpecialRole] = 0
            AND [GroupId] = @AdministrationGroupId
    ) THEN 0 ELSE 1 END;
    DECLARE @NextOrder BIGINT = ISNULL
    (
        ( SELECT MAX( CONVERT( BIGINT, [Order] ) ) FROM [Auth]
          WHERE [EntityTypeId] = @RestControllerEntityTypeId
            AND [EntityId] = @ControllerId
            AND [Action] = 'Edit' ), -1
    ) + 1;

    IF @NextOrder + CONVERT( INT, @NeedsAdminAllow ) > 2147483647
    BEGIN
        THROW 51000, 'Workflow action REST security cannot append rules beyond the maximum security order.', 1;
    END;

    DECLARE @Now DATETIME = GETDATE();

    IF @NeedsAdminAllow = 1
    BEGIN
        INSERT INTO [Auth]
        (
            [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny],
            [SpecialRole], [GroupId], [PersonAliasId], [Guid], [CreatedDateTime], [ModifiedDateTime]
        )
        VALUES
        (
            @RestControllerEntityTypeId, @ControllerId, @NextOrder, 'Edit', 'A',
            0, @AdministrationGroupId, NULL, @AdminAllowGuid, @Now, @Now
        );

        SET @NextOrder = @NextOrder + 1;
    END;

    INSERT INTO [Auth]
    (
        [EntityTypeId], [EntityId], [Order], [Action], [AllowOrDeny],
        [SpecialRole], [GroupId], [PersonAliasId], [Guid], [CreatedDateTime], [ModifiedDateTime]
    )
    VALUES
    (
        @RestControllerEntityTypeId, @ControllerId, @NextOrder, 'Edit', 'D',
        1, NULL, NULL, @AllUsersDenyGuid, @Now, @Now
    );
END;
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}
