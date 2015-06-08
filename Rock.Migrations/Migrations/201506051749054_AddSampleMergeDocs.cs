// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class AddSampleMergeDocs : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // general category
            RockMigrationHelper.UpdateCategory("CD1DB988-6891-4B0F-8D1B-B0A311A3BC3E", "Samples", "fa fa-file-text-o", "Sample merge documents.", "CAA86576-901B-C4A6-4F62-70EB0A2B32A8", 0);
            Sql(RockMigration.MigrationSQL._201506051749054_AddSampleMergeDocs);


            // make the self-service kiosk non-system
            Sql(@"UPDATE [Site] 
	SET [IsSystem] = 0
	WHERE [Guid] = '05E96F7B-B75E-4987-825A-B6F51F8D9CAA'");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // envelope
            Sql(@"DELETE FROM [MergeTemplate] WHERE [Guid] = '7730FDA8-3A1F-79AF-4F79-1F7AEE5BCB9C'");
            Sql(@"DELETE FROM [BinaryFile] WHERE [Guid] = '425298cc-d22d-7e92-42fb-00f96b725f29'");

            // name tags
            Sql(@"DELETE FROM [MergeTemplate] WHERE [Guid] = 'CF853643-9A06-C49B-4223-DF23AE1FAF99'");
            Sql(@"DELETE FROM [BinaryFile] WHERE [Guid] = '07037994-FC97-848E-45DA-3E00E6194B7D'");

            // sample letter
            Sql(@"DELETE FROM [MergeTemplate] WHERE [Guid] = '9FEE4B0F-E5A4-6997-4620-60CAA0964D19'");
            Sql(@"DELETE FROM [BinaryFile] WHERE [Guid] = '7F7275F9-546F-0C83-413A-2465D969AF32'");

            // mailing labels
            Sql(@"DELETE FROM [MergeTemplate] WHERE [Guid] = '111E35D5-57B9-44A9-4934-B234CF9AFAF1'");
            Sql(@"DELETE FROM [BinaryFile] WHERE [Guid] = '288A6849-D348-8983-43B1-65C750F51E54'");

            // certificate
            Sql(@"DELETE FROM [MergeTemplate] WHERE [Guid] = '62604C89-BCD7-09A7-4F04-93C93F03F8D5'");
            Sql(@"DELETE FROM [BinaryFile] WHERE [Guid] = 'ABFAA387-4E4F-C183-448F-78A22708A6D7'");

            RockMigrationHelper.DeleteCategory("CAA86576-901B-C4A6-4F62-70EB0A2B32A8");
        }
    }
}
