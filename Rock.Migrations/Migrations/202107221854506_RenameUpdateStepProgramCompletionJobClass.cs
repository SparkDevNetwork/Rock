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
    public partial class RenameUpdateStepProgramCompletionJobClass : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Fix renamed Job Class (in case an old version of the AddStepProgramCompletion migration ran before it was changed)
            Sql( $@"UPDATE ServiceJob
SET [Class] = 'Rock.Jobs.PostV125DataMigrationsUpdateStepProgramCompletion'
WHERE [Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_125_UPDATE_STEP_PROGRAM_COMPLETION}'" );

            // Migration Rollup - MP: Rename 'Contribution Statements' page to 'Financial Settings'
            Sql( $@" 
UPDATE [Page]
SET InternalName = 'Financial Settings'
    , PageTitle = 'Financial Settings'
    , BrowserTitle = 'Financial Settings'
WHERE [Guid] = '{SystemGuid.Page.FINANCIAL_SETTINGS}'" );

        }

        /// <summary>
        /// SK: "Rock Update Helper v13.0
        /// </summary>
        private void AddPostV125UpdateStepProgramCompletionJob()
        {
            Sql( $@"IF NOT EXISTS (
  SELECT[Id]
  FROM[ServiceJob]
  WHERE[Class] = 'Rock.Jobs.PostV125DataMigrationsUpdateStepProgramCompletion'
   AND[Guid] = '{SystemGuid.ServiceJob.DATA_MIGRATIONS_125_UPDATE_STEP_PROGRAM_COMPLETION}'
  )
BEGIN
 INSERT INTO[ServiceJob](
  [IsSystem]
  ,[IsActive]
  ,[Name]
  ,[Description]
  ,[Class]
  ,[CronExpression]
  ,[NotificationStatus]
  ,[Guid]
  )
 VALUES(
  0
  ,1
  ,'Rock Update Helper v12.5 - Update Step Program Completion'
  ,'Populates Step Program Completion records using existing Step data'
  ,'Rock.Jobs.PostV125DataMigrationsUpdateStepProgramCompletion'
  ,'0 0 21 1/1 * ? *'
  ,1
  ,'{SystemGuid.ServiceJob.DATA_MIGRATIONS_125_UPDATE_STEP_PROGRAM_COMPLETION}'
  );
        END" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
