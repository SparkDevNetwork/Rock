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
    public partial class AddStepProgramCompletionJob : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.Step", "StepProgramCompletionId", "dbo.StepProgramCompletion");
            AddForeignKey("dbo.Step", "StepProgramCompletionId", "dbo.StepProgramCompletion", "Id");

            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", Rock.SystemGuid.FieldType.BOOLEAN, "Class", "Rock.Jobs.CalculateFamilyAnalytics", "Write eRA History", "Write eRA History", "If enabled will write eRA records to the History table.", 5, "true", "DEF426E3-2A86-493D-BEE9-AC1CF0D3D066", "WriteEraHistory" );

            RockMigrationHelper.AddServiceJobAttributeValue( "623F4751-C654-FEB7-45B7-59685B1F60AE", "DEF426E3-2A86-493D-BEE9-AC1CF0D3D066", "false" );

            AddUpdateStepProgramCompletionsJob();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Step", "StepProgramCompletionId", "dbo.StepProgramCompletion");
            AddForeignKey("dbo.Step", "StepProgramCompletionId", "dbo.StepProgramCompletion", "Id", cascadeDelete: true);
            RockMigrationHelper.DeleteAttribute( "DEF426E3-2A86-493D-BEE9-AC1CF0D3D066" );
            DeleteUpdateStepProgramCompletionsJob();
        }

        private void AddUpdateStepProgramCompletionsJob()
        {
            var jobClass = "Rock.Jobs.UpdateStepProgramCompletions";
            var cronSchedule = "0 0 2 1/1 * ? *"; // 2am daily.

            Sql( $@"
            IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = '{jobClass}' AND [Guid] = '{SystemGuid.ServiceJob.UPDATE_STEP_PROGRAM_COMPLETIONS}' )
            BEGIN
                INSERT INTO [ServiceJob] (
                    [IsSystem],
                    [IsActive],
                    [Name],
                    [Description],
                    [Class],
                    [CronExpression],
                    [NotificationStatus],
                    [Guid] )
                VALUES (
                    0,
                    1,
                    'Update Step Program Completions',
                    'Job that updates Step Program Completion Data.',
                    '{jobClass}',
                    '{cronSchedule}',
                    1,
                    '{SystemGuid.ServiceJob.UPDATE_STEP_PROGRAM_COMPLETIONS}' );
            END
            ELSE
            BEGIN
	            UPDATE	[ServiceJob]
	            SET
		              [IsSystem] = 1
		            , [IsActive] = 1
		            , [Name] = 'Update Step Program Completions'
		            , [Description] = 'Job that updates Step Program Completion Data.'
		            , [Class] = '{jobClass}'
		            , [CronExpression] = '{cronSchedule}'
		            , [NotificationStatus] = 1
	            WHERE
		              [Guid] = '{SystemGuid.ServiceJob.UPDATE_STEP_PROGRAM_COMPLETIONS}';
            END" );
        }

        private void DeleteUpdateStepProgramCompletionsJob()
        {
            var jobClass = "Rock.Jobs.UpdateStepProgramCompletions";
            Sql( $"DELETE [ServiceJob] WHERE [Class] = '{jobClass}' AND [Guid] = '{SystemGuid.ServiceJob.UPDATE_STEP_PROGRAM_COMPLETIONS}'" );
        }
    }
}
