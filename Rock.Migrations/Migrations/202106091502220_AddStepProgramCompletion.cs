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
    public partial class AddStepProgramCompletion : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.StepProgramCompletion",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StepProgramId = c.Int(nullable: false),
                        PersonAliasId = c.Int(nullable: false),
                        CampusId = c.Int(),
                        StartDateTime = c.DateTime(nullable: false),
                        EndDateTime = c.DateTime(),
                        StartDateKey = c.Int(nullable: false),
                        EndDateKey = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Campus", t => t.CampusId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId)
                .ForeignKey("dbo.StepProgram", t => t.StepProgramId)
                .Index(t => t.StepProgramId)
                .Index(t => t.PersonAliasId)
                .Index(t => t.CampusId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            AddColumn("dbo.Step", "StepProgramCompletionId", c => c.Int());
            CreateIndex("dbo.Step", "StepProgramCompletionId");
            AddForeignKey("dbo.Step", "StepProgramCompletionId", "dbo.StepProgramCompletion", "Id", cascadeDelete: true);

            AddPostV125UpdateStepProgramCompletionJob();
        }

        /// <summary>
        /// SK: "Rock Update Helper v13.0
        /// </summary>
        private void AddPostV125UpdateStepProgramCompletionJob()
        {
            Sql( $@"IF NOT EXISTS (
  SELECT[Id]
  FROM[ServiceJob]
  WHERE[Class] = 'Rock.Jobs.PostV125UpdateStepProgramCompletion'
   AND[Guid] = '{SystemGuid.ServiceJob.POST_V125_UPDATE_STEP_PROGRAM_COMPLETION}'
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
  ,'Rock.Jobs.PostV125UpdateStepProgramCompletion'
  ,'0 0 21 1/1 * ? *'
  ,1
  ,'{SystemGuid.ServiceJob.POST_V125_UPDATE_STEP_PROGRAM_COMPLETION}'
  );
        END" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Step", "StepProgramCompletionId", "dbo.StepProgramCompletion");
            DropForeignKey("dbo.StepProgramCompletion", "StepProgramId", "dbo.StepProgram");
            DropForeignKey("dbo.StepProgramCompletion", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.StepProgramCompletion", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.StepProgramCompletion", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.StepProgramCompletion", "CampusId", "dbo.Campus");
            DropIndex("dbo.StepProgramCompletion", new[] { "Guid" });
            DropIndex("dbo.StepProgramCompletion", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.StepProgramCompletion", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.StepProgramCompletion", new[] { "CampusId" });
            DropIndex("dbo.StepProgramCompletion", new[] { "PersonAliasId" });
            DropIndex("dbo.StepProgramCompletion", new[] { "StepProgramId" });
            DropIndex("dbo.Step", new[] { "StepProgramCompletionId" });
            DropColumn("dbo.Step", "StepProgramCompletionId");
            DropTable("dbo.StepProgramCompletion");
        }
    }
}
