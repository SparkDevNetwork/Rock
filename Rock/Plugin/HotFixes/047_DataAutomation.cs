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
    /// 
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 47, "1.7.0" )]
    public class DataAutomation : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {

            // Added to core migration: 201803131953247_AddDataAutomation

            // Data Automation Settings
//            RockMigrationHelper.AddPage( true, "84FD84DF-F58B-4B9D-A407-96276C40AB7E", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Data Integrity Settings", "Configure how the data automation job should update your data.", "A2D5F989-1E30-47B9-AAFC-F7EC627AFF21", "fa fa-tachometer" ); // Site:Rock RMS
//            RockMigrationHelper.UpdateBlockType( "Data Automation Settings", "Block used to set values specific to data automation (NCOA, Updating Person Status, Family Campus, Etc).", "~/Blocks/Administration/DataAutomationSettings.ascx", "Administration", "E34C45E9-97CA-4902-803B-1EFAC9174083" );
//            RockMigrationHelper.AddBlock( true, "A2D5F989-1E30-47B9-AAFC-F7EC627AFF21", "", "E34C45E9-97CA-4902-803B-1EFAC9174083", "Data Automation Settings", "Main", @"", @"", 0, "AD705C56-1451-4FD6-BDC3-66072F54034D" );

//            RockMigrationHelper.AddDefinedTypeAttribute( "E17D5988-0372-4792-82CF-9E37C79F7319", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Automated Reactivation", "AllowAutomatedReactivation", "", 1001, "True", "E47870C0-17C7-4556-A922-D7866DFC2C57" );
//            RockMigrationHelper.AddAttributeQualifier( "E47870C0-17C7-4556-A922-D7866DFC2C57", "falsetext", "No", "0EBAAE3D-DC46-4834-8EE7-F44CA07D43E6" );
//            RockMigrationHelper.AddAttributeQualifier( "E47870C0-17C7-4556-A922-D7866DFC2C57", "truetext", "Yes", "C322A622-C1FE-45C7-87B8-60A357BDC2D8" );

//            RockMigrationHelper.UpdateDefinedValue( "E17D5988-0372-4792-82CF-9E37C79F7319", "Does not attend with family", "The individual has not attended with family.", "2BDE800A-C562-4077-9636-5C68770D9676", false );
//            RockMigrationHelper.AddDefinedValueAttributeValue( "05D35BC4-5816-4210-965F-1BF44F35A16A", "E47870C0-17C7-4556-A922-D7866DFC2C57", @"False" );
//            RockMigrationHelper.AddDefinedValueAttributeValue( "2BDE800A-C562-4077-9636-5C68770D9676", "E47870C0-17C7-4556-A922-D7866DFC2C57", @"False" );

//            RockMigrationHelper.UpdateDefinedValue( "E17D5988-0372-4792-82CF-9E37C79F7319", "No Activity", "The individual has not participated in any recent activity.", "64014FE6-943D-4ACF-8014-FED9F9169AE8", true );

//            Sql( @"IF NOT EXISTS(SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.DataAutomation')
//BEGIN
//	INSERT INTO [ServiceJob] (
//         [IsSystem]
//        ,[IsActive]
//        ,[Name]
//        ,[Description]
//        ,[Class]
//        ,[CronExpression]
//        ,[NotificationStatus]
//        ,[Guid] )
//    VALUES (
//         0
//        ,1
//        ,'Data Automation'
//        ,'Updates person/family information based on data automation settings.'
//        ,'Rock.Jobs.DataAutomation'
//        ,'0 0 4 ? * TUE *'
//        ,1
//        ,'059DC06D-39F1-4113-A6C3-94622A40F1CE');
//END" );

            // JE: Makes the interaction channel for Short Links a well known channel, changes the name of the channel to something more readable 
            // and adds formatting to the lists and details that one would expect.
            //Sql( HotFixMigrationResource._047_DataAutomation_ShortLinkInteractionFix );

            // SK: Fixes #2761 Mapped case worker correct address  while printing Benevolence Request
            //Sql( HotFixMigrationResource._047_DataAutomation_FixBenevolenceCaseWorker );

        }


        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {

        }
    }
}
