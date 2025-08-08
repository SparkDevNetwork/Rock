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
    public partial class AddCoreSteps : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddCoreStepsUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddCoreStepsDown();
        }

        #region KH: Add Core Steps and Convert eRA to a Core Step

        private void AddCoreStepsUp()
        {
            AddColumn( "dbo.StepType", "IsSystem", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.StepStatus", "IsSystem", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.StepProgram", "IsSystem", c => c.Boolean( nullable: false ) );

            Sql( @"
INSERT INTO [StepProgram] (
    [Name],
    [IconCssClass],
    [DefaultListView],
    [IsActive],
    [IsSystem],
    [Order],
    [Guid]
)
VALUES (
    'Core Steps',
    'ti ti-map',
    0,
    1,
    1,
    0,
    '898972DA-E58C-4EE1-BCA0-CA3343470B09'
);

DECLARE @StepProgramId INT;
SET @StepProgramId = SCOPE_IDENTITY();

INSERT INTO [StepType] (
    [StepProgramId],
    [Name],
    [IconCssClass],
    [AllowMultiple],
    [HasEndDate],
    [ShowCountOnBadge],
    [AllowManualEditing],
    [HighlightColor],
    [IsActive],
    [Order],
    [Guid],
    [IsDateRequired],
    [IsSystem]
)
VALUES (
    @StepProgramId,
    'eRA',
    'ti ti-calendar-star',
    1,
    1,
    0,
    1,
    '#5ac8fa',
    1,
    0,
    'E57468BE-15BF-48B6-AAB2-F8E2B02720F3',
    1,
    1
);

INSERT INTO [StepStatus] (
	[Name],
	[StepProgramId],
	[IsCompleteStatus],
	[StatusColor],
	[IsActive],
	[Order],
    [IsSystem],
	[Guid]
)
VALUES 
	(
		'In Progress',
		@StepProgramId,
		0,
		'#FFC870',
		1,
		0,
        1,
		'8013C752-31AA-46C6-9B55-BCFBE57C0577'
	),
	(
		'Complete',
		@StepProgramId,
		1,
		'#16C98D',
		1,
		1,
        1,
		'359D3CE0-E144-491E-8C3B-2A2BCE55C04B'
	);

-- Baptism Step Updates

-- Check if the Baptism StepType exists
IF EXISTS (
    SELECT 1
    FROM [dbo].[StepType]
    WHERE [Guid] = '801cc43c-0641-4271-939e-75e428f31d06'
)
BEGIN
    UPDATE [dbo].[StepType]
    SET
        [IsSystem] = 1,
        [HighlightColor] = '#007aff'
    WHERE [Guid] = '801cc43c-0641-4271-939e-75e428f31d06';
END
ELSE
BEGIN
    -- Insert original StepType
    INSERT INTO [dbo].[StepType] (
        [StepProgramId],
        [Name],
        [Description],
        [IconCssClass],
        [AllowMultiple],
        [HasEndDate],
        [ShowCountOnBadge],
        [AllowManualEditing],
        [HighlightColor],
        [IsActive],
        [Order],
        [Guid],
        [IsDateRequired],
        [IsSystem]
    )
    VALUES (
        @StepProgramId,
        'Baptism',
        'An act of obedience symbolizing the believer''s faith in a crucified, buried, and risen Savior, the believer''s death to sin, the burial of the old life, and the resurrection to walk in newness of life in Christ Jesus. It is a testimony to the believer''s faith in the final resurrection of the dead.',
        'ti ti-droplet',
        0,
        0,
        0,
        1,
        '#007aff',
        1,
        0,
        '801cc43c-0641-4271-939e-75e428f31d06',
        1,
        1
    );
END

-- Small Group Step Updates

-- Check if the Small Group StepType exists
IF EXISTS (
    SELECT 1
    FROM [dbo].[StepType]
    WHERE [Guid] = 'EFA15A4F-5666-4153-B92F-AF3ECD73C504'
)
BEGIN
    UPDATE [dbo].[StepType]
    SET
        [IsSystem] = 1,
        [HighlightColor] = '#ff2d55'
    WHERE [Guid] = 'EFA15A4F-5666-4153-B92F-AF3ECD73C504';
END
ELSE
BEGIN
    -- Insert original StepType
    INSERT INTO [dbo].[StepType] (
        [StepProgramId],
        [Name],
        [Description],
        [IconCssClass],
        [AllowMultiple],
        [HasEndDate],
        [ShowCountOnBadge],
        [AllowManualEditing],
        [HighlightColor],
        [IsActive],
        [Order],
        [Guid],
        [IsDateRequired],
        [IsSystem]
    )
    VALUES (
        @StepProgramId,
        'Small Group',
        'A Small Group is an intentional gathering, meeting regularly for the purpose of joining God''s mission.',
        'ti ti-users',
        1,
        1,
        0,
        1,
        '#ff2d55',
        1,
        1,
        'EFA15A4F-5666-4153-B92F-AF3ECD73C504',
        1,
        1
    );
END

-- Serve Step Updates

-- Check if the Serve StepType exists
IF EXISTS (
    SELECT 1
    FROM [dbo].[StepType]
    WHERE [Guid] = '71E66730-8F7D-4EEF-9C53-524C4BDE5E59'
)
BEGIN
    UPDATE [dbo].[StepType]
    SET
        [IsSystem] = 1,
        [HighlightColor] = '#5856D6'
    WHERE [Guid] = '71E66730-8F7D-4EEF-9C53-524C4BDE5E59';
END
ELSE
BEGIN
    -- Insert original StepType
    INSERT INTO [dbo].[StepType] (
        [StepProgramId],
        [Name],
        [Description],
        [IconCssClass],
        [AllowMultiple],
        [HasEndDate],
        [ShowCountOnBadge],
        [AllowManualEditing],
        [HighlightColor],
        [IsActive],
        [Order],
        [Guid],
        [IsDateRequired],
        [IsSystem]
    )
    VALUES (
        @StepProgramId,
        'Serve',
        'This means leveraging gifts, talents and abilities that God has given each of us to serve, engage in our church and local community and have fun impacting others for God''s kingdom.',
        'ti ti-bell',
        1,
        1,
        1,
        1,
        '#5856D6',
        1,
        2,
        '71E66730-8F7D-4EEF-9C53-524C4BDE5E59',
        1,
        1
    );
END
" );
            var baptismStepTypeIdSql = "SELECT TOP 1 [Id] FROM [dbo].[StepType] WHERE [Guid] = '801cc43c-0641-4271-939e-75e428f31d06'";
            var baptismStepTypeId = SqlScalar( baptismStepTypeIdSql ).ToIntSafe();

            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.Step",
                SystemGuid.FieldType.BOOLEAN,
                "StepTypeId",
                baptismStepTypeId.ToString(),
                "Baptized Here",
                "Baptized Here",
                "Indicates whether the person was baptized at this church.",
                0,
                string.Empty,
                "343A557E-92CC-4A75-B251-0DA732F5EB4E",
                "BaptizedHere" );

            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v18.0 - Migrate eRA Records To Step Records",
                description: "This job will migrate existing eRA records from the History table to the Steps table.",
                jobType: "Rock.Jobs.PostV18MigrateERAToCoreSteps",
                cronExpression: "0 0 2 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_180_MIGRATE_ERA_DATA_TO_STEPS );
        }

        private void AddCoreStepsDown()
        {
            DropColumn( "dbo.StepProgram", "IsSystem" );
            DropColumn( "dbo.StepStatus", "IsSystem" );
            DropColumn( "dbo.StepType", "IsSystem" );
            Sql( @"
DELETE FROM [StepProgram] WHERE [Guid] = '898972DA-E58C-4EE1-BCA0-CA3343470B09';
DELETE FROM [StepType] WHERE [Guid] ='E57468BE-15BF-48B6-AAB2-F8E2B02720F3';
DELETE FROM [StepStatus] WHERE [Guid] IN ('8013C752-31AA-46C6-9B55-BCFBE57C0577', '359D3CE0-E144-491E-8C3B-2A2BCE55C04B');
" );
            RockMigrationHelper.DeleteAttribute( "343A557E-92CC-4A75-B251-0DA732F5EB4E" );
        }

        #endregion
    }
}
