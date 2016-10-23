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
    public partial class ExceptionUpdates : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
            IF EXISTS ( SELECT * FROM sys.indexes WHERE name='IX_CreatedDateTime' AND object_id = OBJECT_ID(N'[dbo].[ExceptionLog]') )
            DROP INDEX [IX_CreatedDateTime] ON [dbo].[ExceptionLog]
            GO

            CREATE NONCLUSTERED INDEX [IX_CreatedDateTime] ON [dbo].[ExceptionLog] ( [CreatedDateTime] ASC )
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
            GO
            " );

            // Page: Exception Occurrences
            RockMigrationHelper.AddPage( "21DA6141-0A03-4F00-B0A8-3B110FBE2438", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Exception Occurrences", "", "F95539C3-03C8-422B-B586-EF4C2FE91CF4", "" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Exception Occurrences", "Lists all exception occurrences.", "~/Blocks/Core/ExceptionOccurrences.ascx", "Core", "E3486885-FA88-4B67-88B6-472F1FE4E5E4" );
            RockMigrationHelper.AddBlock( "F95539C3-03C8-422B-B586-EF4C2FE91CF4", "", "E3486885-FA88-4B67-88B6-472F1FE4E5E4", "Exception Occurrences", "Main", "", "", 0, "C35385C1-0A53-4E57-91B8-652336932433" );
            RockMigrationHelper.AddBlockTypeAttribute( "E3486885-FA88-4B67-88B6-472F1FE4E5E4", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "FB05C222-A20F-4F36-BD30-BBA48AD68F92" );
            RockMigrationHelper.AddBlockAttributeValue( "C35385C1-0A53-4E57-91B8-652336932433", "FB05C222-A20F-4F36-BD30-BBA48AD68F92", @"f1f58172-e03e-4299-910a-ed34f857dafb" ); // Detail Page

            // Tie existing pages to new page
            RockMigrationHelper.AddBlockAttributeValue( "557E75A4-1841-4CBE-B976-F36DF209AA17", "A742376A-0148-4777-B704-E47841879337", @"f95539c3-03c8-422b-b586-ef4c2fe91cf4" ); // Detail Page
            RockMigrationHelper.MovePage( "F1F58172-E03E-4299-910A-ED34F857DAFB", "F95539C3-03C8-422B-B586-EF4C2FE91CF4" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Restitch old pages together
            RockMigrationHelper.AddBlockAttributeValue( "557E75A4-1841-4CBE-B976-F36DF209AA17", "A742376A-0148-4777-B704-E47841879337", @"f1f58172-e03e-4299-910a-ed34f857dafb" );
            RockMigrationHelper.MovePage( "F1F58172-E03E-4299-910A-ED34F857DAFB", "21DA6141-0A03-4F00-B0A8-3B110FBE2438" );

            RockMigrationHelper.DeleteAttribute( "FB05C222-A20F-4F36-BD30-BBA48AD68F92" );
            RockMigrationHelper.DeleteBlock( "C35385C1-0A53-4E57-91B8-652336932433" );
            RockMigrationHelper.DeleteBlockType( "E3486885-FA88-4B67-88B6-472F1FE4E5E4" );
            RockMigrationHelper.DeletePage( "F95539C3-03C8-422B-B586-EF4C2FE91CF4" ); //  Page: Exception Occurrences
            DropIndex( "ExceptionLog", "IX_CreatedDateTime" );
        }
    }
}
