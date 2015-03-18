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
    public partial class MergeTemplate : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.MergeTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        TemplateBinaryFileId = c.Int(nullable: false),
                        MergeTemplateProviderEntityTypeId = c.Int(nullable: false),
                        CategoryId = c.Int(nullable: false),
                        PersonAliasId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Category", t => t.CategoryId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.EntityType", t => t.MergeTemplateProviderEntityTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId)
                .ForeignKey("dbo.BinaryFile", t => t.TemplateBinaryFileId)
                .Index(t => t.TemplateBinaryFileId)
                .Index(t => t.MergeTemplateProviderEntityTypeId)
                .Index(t => t.CategoryId)
                .Index(t => t.PersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);

            RockMigrationHelper.UpdateBinaryFileType( Rock.SystemGuid.EntityType.STORAGE_PROVIDER_DATABASE, "Merge Template", "Usually either a MS Word Docx or Html file", "fa fa-files-o", Rock.SystemGuid.BinaryFiletype.MERGE_TEMPLATE, false, true );
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.MERGE_TEMPLATE, 0, "Edit", true, Rock.SystemGuid.Group.GROUP_ADMINISTRATORS, Model.SpecialRole.None, "D5D0FB59-993D-4B70-851C-18787AE8DE09" );
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.MERGE_TEMPLATE, 1, "Edit", true, Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS, Model.SpecialRole.None, "00615556-29EB-4085-A774-F1D203ECE9D0" );
            RockMigrationHelper.AddSecurityAuthForBinaryFileType( Rock.SystemGuid.BinaryFiletype.MERGE_TEMPLATE, 2, "Edit", true, Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS, Model.SpecialRole.None, "E7300CED-B48F-4803-B27C-8851B2905668" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.MergeTemplate", "TemplateBinaryFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.MergeTemplate", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MergeTemplate", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MergeTemplate", "MergeTemplateProviderEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.MergeTemplate", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.MergeTemplate", "CategoryId", "dbo.Category");
            DropIndex("dbo.MergeTemplate", new[] { "Guid" });
            DropIndex("dbo.MergeTemplate", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.MergeTemplate", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.MergeTemplate", new[] { "PersonAliasId" });
            DropIndex("dbo.MergeTemplate", new[] { "CategoryId" });
            DropIndex("dbo.MergeTemplate", new[] { "MergeTemplateProviderEntityTypeId" });
            DropIndex("dbo.MergeTemplate", new[] { "TemplateBinaryFileId" });
            DropTable("dbo.MergeTemplate");
        }
    }
}
