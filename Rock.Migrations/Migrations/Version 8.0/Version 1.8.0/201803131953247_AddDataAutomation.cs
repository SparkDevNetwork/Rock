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
    public partial class AddDataAutomation : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Delete the original page added to general settings.
            RockMigrationHelper.DeleteBlock( "816CBFA2-B0CD-4EAB-8E42-7F77216815DA" );
            RockMigrationHelper.DeleteBlockType( "BA4292F9-AB6A-4464-9F0B-FC580B92C4BF" ); // Data Integrity Settings
            RockMigrationHelper.DeletePage( "4818E7C6-4D21-4657-B4E7-464B61160EB2" ); //  Page: Data Integrity Settings, Layout: Full Width, Site: Rock RMS

            // Add the new page
            RockMigrationHelper.AddPage( true, "84FD84DF-F58B-4B9D-A407-96276C40AB7E", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Data Integrity Settings", "", "A2D5F989-1E30-47B9-AAFC-F7EC627AFF21", "fa fa-tachometer" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Data Automation Settings", "Block used to set values specific to data automation (NCOA, Updating Person Status, Family Campus, Etc).", "~/Blocks/Administration/DataAutomationSettings.ascx", "Administration", "E34C45E9-97CA-4902-803B-1EFAC9174083" );
            RockMigrationHelper.AddBlock( true, "A2D5F989-1E30-47B9-AAFC-F7EC627AFF21", "", "E34C45E9-97CA-4902-803B-1EFAC9174083", "Data Automation Settings", "Main", @"", @"", 0, "AD705C56-1451-4FD6-BDC3-66072F54034D" );

            RockMigrationHelper.UpdateDefinedValue( "E17D5988-0372-4792-82CF-9E37C79F7319", "No Activity", "The individual has not participated in any recent activity.", "64014FE6-943D-4ACF-8014-FED9F9169AE8", true );

            // Add the job if it does not exist
            Sql( @"IF NOT EXISTS(SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.DataAutomation')
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
        ,'Data Automation'
        ,'Updates person/family information based on data automation settings.'
        ,'Rock.Jobs.DataAutomation'
        ,'0 0 4 ? * TUE *'
        ,1
        ,'059DC06D-39F1-4113-A6C3-94622A40F1CE');
END" );

            // JE: Makes the interaction channel for Short Links a well known channel, changes the name of the channel to something more readable 
            // and adds formatting to the lists and details that one would expect.
            Sql( Plugin.HotFixes.HotFixMigrationResource._047_DataAutomation_ShortLinkInteractionFix );

            // SK: Fixes #2761 Mapped case worker correct address  while printing Benevolence Request
            Sql( Plugin.HotFixes.HotFixMigrationResource._047_DataAutomation_FixBenevolenceCaseWorker );


        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
