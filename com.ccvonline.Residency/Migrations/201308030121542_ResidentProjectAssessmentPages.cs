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
namespace com.ccvonline.Residency.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class ResidentProjectAssessmentPages : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ////////////DONE


            // Update new pages to use Residency Site
            Sql( @"
DECLARE @SiteId int

SET @SiteId = (select [Id] from [Site] where [Guid] = '960F1D98-891A-4508-8F31-3CF206F5406E')

-- Update Resident pages to use new site (default layout)
UPDATE [Page] SET 
    [SiteId] = @SiteId,
    [Layout] = 'Default'
WHERE [Guid] in (
'162927F6-E503-43C4-B075-55F1E592E96E',
'BDA4C473-01CD-449E-97D4-4B054E3F0959')

" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: com_ccvonline - Residency - Resident Project Assessment List:Show Grid Title
            DeleteAttribute( "F5DAD59C-AC05-4BCA-ACC3-171924039872" );
            // Attrib for BlockType: com_ccvonline - Residency - Resident Project Assessment List:Show Project Column
            DeleteAttribute( "F0390AAB-D114-4367-8349-35EEA8EDACB8" );
            // Attrib for BlockType: com_ccvonline - Residency - Resident Project Assessment List:Show Competency Column
            DeleteAttribute( "BFE19B2F-D62D-4C87-B928-FFFAE65BD12E" );

            DeleteBlock( "1DB06C26-B318-46CD-9E9F-219FC1EF6338" ); // Resident Project Assessment Detail
            DeleteBlock( "B459F23A-9C32-4537-BA93-637A81ACB35A" ); // Resident Project Assessment List
            DeleteBlock( "3938E111-C9FF-49E9-B1B8-A2AA89080F51" ); // Page Xslt Transformation
            DeleteBlock( "4EFF1322-6A9A-44A0-B3B8-CB547CB09C0B" ); // Current Person
            DeleteBlock( "D07780FC-0ED5-4881-8B76-24F6FAE8A897" ); // Page Xslt Transformation
            DeleteBlock( "E517DDD7-73DB-4475-87A4-83CBCD7657F1" ); // Current Person
            DeletePage( "BDA4C473-01CD-449E-97D4-4B054E3F0959" ); // Project Assessment Detail
            DeletePage( "162927F6-E503-43C4-B075-55F1E592E96E" ); // Assessments

            // Un-Move the Resident Competency List down under the new Page Nav pills 
            Sql( "Update [Block] set [Order] = 1 where [Guid] = 'EE97ABE8-A124-4437-B962-805C1D0C18D4'" );

            // Un-Update Resident Home page name,title to 'Coursework' since it is part of the new Page Nav pills
            Sql( "Update [Page] set [Name] = 'Resident Home', [Title] = 'Resident Home' where [Guid] = '826C0BFF-C831-4427-98F9-57FF462D82F5'" );
        }
    }
}
/* Skipped Operations for tables that are not part of ResidencyContext: Review these comments to verify the proper things were skipped */
/* To disable skipping, edit your Migrations\Configuration.cs so that CodeGenerator = new RockCSharpMigrationCodeGenerator<ResidencyContext>(false); */

// Up()...
// AddColumnOperation for TableName BinaryFile, column StorageEntityTypeId.
// AddColumnOperation for TableName BinaryFileType, column StorageEntityTypeId.
// AddColumnOperation for TableName Campus, column ShortCode.
// CreateIndexOperation for TableName BinaryFileType, column StorageEntityTypeId.
// CreateIndexOperation for TableName BinaryFile, column StorageEntityTypeId.
// AddForeignKeyOperation for TableName BinaryFileType, column StorageEntityTypeId.
// AddForeignKeyOperation for TableName BinaryFile, column StorageEntityTypeId.

// Down()...
// DropForeignKeyOperation for TableName BinaryFile, column StorageEntityTypeId.
// DropForeignKeyOperation for TableName BinaryFileType, column StorageEntityTypeId.
// DropIndexOperation for TableName BinaryFile, column StorageEntityTypeId.
// DropIndexOperation for TableName BinaryFileType, column StorageEntityTypeId.
// DropColumnOperation for TableName Campus, column ShortCode.
// DropColumnOperation for TableName BinaryFileType, column StorageEntityTypeId.
// DropColumnOperation for TableName BinaryFile, column StorageEntityTypeId.
