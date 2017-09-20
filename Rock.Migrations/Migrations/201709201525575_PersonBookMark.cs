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
    public partial class PersonBookMark : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.PersonBookmark",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Url = c.String(nullable: false, maxLength: 2083),
                        Order = c.Int(),
                        CategoryId = c.Int(),
                        PersonAliasId = c.Int(),
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
                .ForeignKey("dbo.Category", t => t.CategoryId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId)
                .Index(t => t.CategoryId)
                .Index(t => t.PersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);

            RockMigrationHelper.UpdateEntityType( "Rock.Model.PersonBookmark", "E07A503C-7398-4677-8E91-BEF1994244F3", true, true );
            RockMigrationHelper.UpdateBlockType( "Bookmark Detail", "Displays the details of the given person bookmark.", "~/Blocks/Core/BookmarkDetail.ascx", "Core", "97F3CC15-7B47-4B3B-BF34-BDB518062A40" );
            RockMigrationHelper.UpdateBlockType( "Bookmark", "Displays bookmark specific to the currently logged in user along with options to add new to the list.", "~/Blocks/Core/Bookmarks.ascx", "Core", "C33C8084-DA9F-4DD9-8961-A3ED63F2A512" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "C33C8084-DA9F-4DD9-8961-A3ED63F2A512", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Manage Page", "ManagePage", "", "", 0, @"", "E1AFD35A-2456-4F1D-AA65-1668EEBA53DE" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "C33C8084-DA9F-4DD9-8961-A3ED63F2A512", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "EntityType", "", "Display categories associated with this type of entity", 0, @"", "8DB86D58-4B39-4B49-B8A2-A3A9F5A49E6D" );

            RockMigrationHelper.AddPage( "936C90C4-29CF-4665-A489-7C687217F7B8", "0CB60906-6B74-44FD-AB25-026050EF70EB", "Bookmarks", "", "20C56F03-9812-4DCD-8F96-2C082A4F39A0", "" ); // Site:Rock RMS

            RockMigrationHelper.AddBlock( "20C56F03-9812-4DCD-8F96-2C082A4F39A0", "", "ADE003C7-649B-466A-872B-B8AC952E7841", "Category Tree View", "Sidebar1", @"", @"", 0, "F991BA0E-F790-4C02-BE78-EA1661EF1E7E" );
            RockMigrationHelper.AddBlockAttributeValue( "F991BA0E-F790-4C02-BE78-EA1661EF1E7E", "D2596ADF-4455-42A4-848F-6DFD816C2867", @"fa fa-bookmark-o" );
            RockMigrationHelper.AddBlockAttributeValue( "F991BA0E-F790-4C02-BE78-EA1661EF1E7E", "AA057D3E-00CC-42BD-9998-600873356EDB", @"PersonBookmarkId" );
            RockMigrationHelper.AddBlockAttributeValue( "F991BA0E-F790-4C02-BE78-EA1661EF1E7E", "06D414F0-AA20-4D3C-B297-1530CCD64395", @"e07a503c-7398-4677-8e91-bef1994244f3" );
            RockMigrationHelper.AddBlockAttributeValue( "F991BA0E-F790-4C02-BE78-EA1661EF1E7E", "AEE521D8-124D-4BB3-8A80-5F368E5CEC15", @"20c56f03-9812-4dcd-8f96-2c082a4f39a0" );
            RockMigrationHelper.AddBlockAttributeValue( "F991BA0E-F790-4C02-BE78-EA1661EF1E7E", "2D5FF74A-D316-4924-BCD2-6AA338D8DAAC", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "F991BA0E-F790-4C02-BE78-EA1661EF1E7E", "F76C5EEF-FD45-4BD6-A903-ED5AB53BB928", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "F991BA0E-F790-4C02-BE78-EA1661EF1E7E", "07213E2C-C239-47CA-A781-F7A908756DC2", @"Bookmark" );
            RockMigrationHelper.AddBlockAttributeValue( "F991BA0E-F790-4C02-BE78-EA1661EF1E7E", "C48600CD-2C65-46EF-84E8-975F0DE8C28E", @"True" );
            RockMigrationHelper.AddBlockAttributeValue( "F991BA0E-F790-4C02-BE78-EA1661EF1E7E", "61398707-FCCE-4AFD-8374-110BCA275F34", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "F991BA0E-F790-4C02-BE78-EA1661EF1E7E", "2080E00B-63E4-4874-B06A-ADE1B4F3B3AD", @"" );

            RockMigrationHelper.AddBlock( "20C56F03-9812-4DCD-8F96-2C082A4F39A0", "", "7BC54887-21C2-4688-BD1D-C1C8B9C86F7C", "Category Detail", "Main", @"", @"", 0, "CF42F151-A47C-4839-86BF-BB1FEEDEAD23" );
            RockMigrationHelper.AddBlockAttributeValue( "CF42F151-A47C-4839-86BF-BB1FEEDEAD23", "AB542995-876F-4B8F-8417-11D83369289E", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "CF42F151-A47C-4839-86BF-BB1FEEDEAD23", "C3B72ADF-93D7-42CF-A103-8A7661A6926B", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "CF42F151-A47C-4839-86BF-BB1FEEDEAD23", "620957FF-BC28-4A89-A74F-C917DA5CFD47", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "CF42F151-A47C-4839-86BF-BB1FEEDEAD23", "985128EE-D40C-4598-B14B-7AD728ECCE38", @"" );
            RockMigrationHelper.AddBlockAttributeValue( "CF42F151-A47C-4839-86BF-BB1FEEDEAD23", "FF3A33CF-8897-4FC6-9C16-64FA25E6C297", @"e07a503c-7398-4677-8e91-bef1994244f3" );
            RockMigrationHelper.AddBlockAttributeValue( "CF42F151-A47C-4839-86BF-BB1FEEDEAD23", "1089B083-8606-4807-A448-781A693DFAC7", @"True" );

            RockMigrationHelper.AddBlock( "20C56F03-9812-4DCD-8F96-2C082A4F39A0", "", "97F3CC15-7B47-4B3B-BF34-BDB518062A40", "Bookmark Detail", "Main", @"", @"", 1, "AFB747BE-CF47-4643-91DB-2C879E7E9FC7" );

            RockMigrationHelper.AddBlock( "", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "C33C8084-DA9F-4DD9-8961-A3ED63F2A512", "Bookmark", "Header", @"", @"", 1, "78848772-CCA8-4D3B-B981-7E4E3E4FECDB" );
            RockMigrationHelper.AddBlockAttributeValue( "78848772-CCA8-4D3B-B981-7E4E3E4FECDB", "E1AFD35A-2456-4F1D-AA65-1668EEBA53DE", @"20c56f03-9812-4dcd-8f96-2c082a4f39a0" );
            RockMigrationHelper.AddBlockAttributeValue( "78848772-CCA8-4D3B-B981-7E4E3E4FECDB", "8DB86D58-4B39-4B49-B8A2-A3A9F5A49E6D", @"e07a503c-7398-4677-8e91-bef1994244f3" );

            RockMigrationHelper.AddBlock( "", "0CB60906-6B74-44FD-AB25-026050EF70EB", "C33C8084-DA9F-4DD9-8961-A3ED63F2A512", "Bookmark", "Header", @"", @"", 1, "64DAC55B-0AC6-4EB2-A736-78E5222FD33B" );
            RockMigrationHelper.AddBlockAttributeValue( "64DAC55B-0AC6-4EB2-A736-78E5222FD33B", "E1AFD35A-2456-4F1D-AA65-1668EEBA53DE", @"20c56f03-9812-4dcd-8f96-2c082a4f39a0" );
            RockMigrationHelper.AddBlockAttributeValue( "64DAC55B-0AC6-4EB2-A736-78E5222FD33B", "8DB86D58-4B39-4B49-B8A2-A3A9F5A49E6D", @"e07a503c-7398-4677-8e91-bef1994244f3" );

            RockMigrationHelper.AddBlock( "", "22D220B5-0D34-429A-B9E3-59D80AE423E7", "C33C8084-DA9F-4DD9-8961-A3ED63F2A512", "Bookmark", "Header", @"", @"", 1, "94B65EC4-CE22-4566-B3DE-5DA450CBD7A5" );
            RockMigrationHelper.AddBlockAttributeValue( "94B65EC4-CE22-4566-B3DE-5DA450CBD7A5", "E1AFD35A-2456-4F1D-AA65-1668EEBA53DE", @"20c56f03-9812-4dcd-8f96-2c082a4f39a0" );
            RockMigrationHelper.AddBlockAttributeValue( "94B65EC4-CE22-4566-B3DE-5DA450CBD7A5", "8DB86D58-4B39-4B49-B8A2-A3A9F5A49E6D", @"e07a503c-7398-4677-8e91-bef1994244f3" );

            RockMigrationHelper.AddBlock( "", "EDFE06F4-D329-4340-ACD8-68A60CD112E6", "C33C8084-DA9F-4DD9-8961-A3ED63F2A512", "Bookmark", "Header", @"", @"", 1, "EDDE158D-078D-4A50-9726-85530078C26B" );
            RockMigrationHelper.AddBlockAttributeValue( "EDDE158D-078D-4A50-9726-85530078C26B", "E1AFD35A-2456-4F1D-AA65-1668EEBA53DE", @"20c56f03-9812-4dcd-8f96-2c082a4f39a0" );
            RockMigrationHelper.AddBlockAttributeValue( "EDDE158D-078D-4A50-9726-85530078C26B", "8DB86D58-4B39-4B49-B8A2-A3A9F5A49E6D", @"e07a503c-7398-4677-8e91-bef1994244f3" );

            RockMigrationHelper.AddBlock( "", "F66758C6-3E3D-4598-AF4C-B317047B5987", "C33C8084-DA9F-4DD9-8961-A3ED63F2A512", "Bookmark", "Header", @"", @"", 1, "6038FFF9-B21B-4459-B3DE-10DD41E1E86F" );
            RockMigrationHelper.AddBlockAttributeValue( "6038FFF9-B21B-4459-B3DE-10DD41E1E86F", "E1AFD35A-2456-4F1D-AA65-1668EEBA53DE", @"20c56f03-9812-4dcd-8f96-2c082a4f39a0" );
            RockMigrationHelper.AddBlockAttributeValue( "6038FFF9-B21B-4459-B3DE-10DD41E1E86F", "8DB86D58-4B39-4B49-B8A2-A3A9F5A49E6D", @"e07a503c-7398-4677-8e91-bef1994244f3" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "8DB86D58-4B39-4B49-B8A2-A3A9F5A49E6D" );
            RockMigrationHelper.DeleteAttribute( "E1AFD35A-2456-4F1D-AA65-1668EEBA53DE" );

            RockMigrationHelper.DeleteBlock( "CF42F151-A47C-4839-86BF-BB1FEEDEAD23" );
            RockMigrationHelper.DeleteBlock( "F991BA0E-F790-4C02-BE78-EA1661EF1E7E" );
            RockMigrationHelper.DeleteBlock( "AFB747BE-CF47-4643-91DB-2C879E7E9FC7" );
            RockMigrationHelper.DeleteBlock( "78848772-CCA8-4D3B-B981-7E4E3E4FECDB" );
            RockMigrationHelper.DeleteBlock( "64DAC55B-0AC6-4EB2-A736-78E5222FD33B" );
            RockMigrationHelper.DeleteBlock( "94B65EC4-CE22-4566-B3DE-5DA450CBD7A5" );
            RockMigrationHelper.DeleteBlock( "EDDE158D-078D-4A50-9726-85530078C26B" );
            RockMigrationHelper.DeleteBlock( "6038FFF9-B21B-4459-B3DE-10DD41E1E86F" );

            RockMigrationHelper.DeletePage( "20C56F03-9812-4DCD-8F96-2C082A4F39A0" );

            RockMigrationHelper.DeleteBlockType( "97F3CC15-7B47-4B3B-BF34-BDB518062A40" );
            RockMigrationHelper.DeleteBlockType( "C33C8084-DA9F-4DD9-8961-A3ED63F2A512" );

            DropForeignKey( "dbo.PersonBookmark", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PersonBookmark", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PersonBookmark", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.PersonBookmark", "CategoryId", "dbo.Category");
            DropIndex("dbo.PersonBookmark", new[] { "Guid" });
            DropIndex("dbo.PersonBookmark", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.PersonBookmark", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.PersonBookmark", new[] { "PersonAliasId" });
            DropIndex("dbo.PersonBookmark", new[] { "CategoryId" });
            DropTable("dbo.PersonBookmark");
        }
    }
}
