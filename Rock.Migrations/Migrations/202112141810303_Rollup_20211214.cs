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
    public partial class Rollup_20211214 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddJobToProcessElevatedSecurity_Up();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddJobToProcessElevatedSecurity_Down();
        }

        /// <summary>
        /// CR: AddJobToProcessElevatedSecurity
        /// </summary>
        public void AddJobToProcessElevatedSecurity_Up()
        {
            // add ServiceJob: Process Elevated Security
            // Code Generated using Rock\Dev Tools\Sql\CodeGen_ServiceJobWithAttributes_ForAJob.sql
            Sql( @"IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.ProcessElevatedSecurity' AND [Guid] = 'A1AF9D7D-E968-4AF6-B203-6BB4FD625714' ) 
                    BEGIN 
                    INSERT INTO [ServiceJob] ([IsSystem],[IsActive],[Name],[Description],[Class],[CronExpression],[NotificationStatus],[Guid] ) 
                    VALUES ( 0,1,'Process Elevated Security','Updates the person account protection profiles.','Rock.Jobs.ProcessElevatedSecurity','0 15 3 1/1 * ? *',1,'A1AF9D7D-E968-4AF6-B203-6BB4FD625714');
                    END" );
        }
        
        /// <summary>
        /// CR: AddJobToProcessElevatedSecurity
        /// </summary>
        public void AddJobToProcessElevatedSecurity_Down()
        {
            // Code Generated using Rock\Dev Tools\Sql\CodeGen_ServiceJobWithAttributes_ForAJob.sql  
            // remove ServiceJob: Process Elevated Security
            Sql( @"IF EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.ProcessElevatedSecurity' AND [Guid] = 'A1AF9D7D-E968-4AF6-B203-6BB4FD625714' )
                    BEGIN
                    DELETE [ServiceJob]  WHERE [Guid] = 'A1AF9D7D-E968-4AF6-B203-6BB4FD625714';
                    END" );
        }
    }
}
