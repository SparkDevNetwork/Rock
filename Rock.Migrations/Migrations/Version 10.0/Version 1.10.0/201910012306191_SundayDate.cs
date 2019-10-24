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
    using System.Data.SqlTypes;

    /// <summary>
    ///
    /// </summary>
    public partial class SundayDate : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropIndex( "dbo.AttendanceOccurrence", new[] { "SundayDate" } );
            DropColumn( "dbo.AttendanceOccurrence", "SundayDate" );
            AddColumn( "dbo.AttendanceOccurrence", "SundayDate", c => c.DateTime( nullable: false, storeType: "date", defaultValue: SqlDateTime.MinValue.Value ) );
            CreateIndex( "dbo.AttendanceOccurrence", "SundayDate" );

            DropIndex( "dbo.FinancialTransaction", new[] { "SundayDate" } );
            DropColumn( "dbo.FinancialTransaction", "SundayDate" );
            AddColumn( "dbo.FinancialTransaction", "SundayDate", c => c.DateTime(nullable:true, storeType:"date", defaultValue:SqlDateTime.MinValue.Value ) );


            CreateIndex( "dbo.FinancialTransaction", "SundayDate" );

            // add ServiceJob: SundayDate Data Migrations for v10.0
            // Code Generated using Rock\Dev Tools\Sql\CodeGen_ServiceJobWithAttributes_ForAJob.sql
            Sql( @"IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.PostV100DataMigrationsSundayDate' AND [Guid] = 'CC263453-B290-4393-BB91-1C1C87CAE291' )
            BEGIN
               INSERT INTO [ServiceJob] (
                  [IsSystem]
                  ,[IsActive]
                  ,[Name]
                  ,[Description]
                  ,[Class]
                  ,[CronExpression]
                  ,[NotificationStatus]
                  ,[Guid] )
               VALUES ( 
                  0
                  ,1
                  ,'SundayDate Data Migrations for v10.0'
                  ,'This job will take care of any data migrations to SundayDate on AttendanceOccurrence and FinancialTransaction that need to occur after updating to v10.0. After all the operations are done, this job will delete itself.'
                  ,'Rock.Jobs.PostV100DataMigrationsSundayDate'
                  ,'0 0 2 1/1 * ? *'
                  ,1
                  ,'CC263453-B290-4393-BB91-1C1C87CAE291'
                  );
            END" );
            
            // Attribute: Rock.Jobs.PostV100DataMigrationsSundayDate: Command Timeout
            RockMigrationHelper.AddOrUpdateEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "Rock.Jobs.PostV100DataMigrationsSundayDate", "Command Timeout", "Command Timeout", @"Maximum amount of time (in seconds) to wait for each SQL command to complete. On a large database with lots of transactions, this could take several minutes or more.", 0, @"3600", "13D4DAF9-BDD0-4D1A-B511-F69AAF565DCD", "CommandTimeout" );
            RockMigrationHelper.AddAttributeValue( "13D4DAF9-BDD0-4D1A-B511-F69AAF565DCD", 56, @"3600", "13D4DAF9-BDD0-4D1A-B511-F69AAF565DCD" ); // SundayDate Data Migrations for v10.0: Command Timeout
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Code Generated using Rock\Dev Tools\Sql\CodeGen_ServiceJobWithAttributes_ForAJob.sql
            RockMigrationHelper.DeleteAttribute( "13D4DAF9-BDD0-4D1A-B511-F69AAF565DCD" ); // Rock.Jobs.PostV100DataMigrationsSundayDate: Command Timeout

            // remove ServiceJob: SundayDate Data Migrations for v10.0
            Sql( @"IF EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.PostV100DataMigrationsSundayDate' AND [Guid] = 'CC263453-B290-4393-BB91-1C1C87CAE291' )
            BEGIN
               DELETE [ServiceJob]  WHERE [Guid] = 'CC263453-B290-4393-BB91-1C1C87CAE291';
            END" );

        }
    }
}
