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
    public partial class AddWorkflowRelatedModel : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddDefinedType( "Workflow", "Section Type", "Used by some components in Rock to control the styling used when building UI.", "A72D940B-2A69-44B8-931C-7FE99824D84C", @"" );
            RockMigrationHelper.AddDefinedTypeAttribute( "A72D940B-2A69-44B8-931C-7FE99824D84C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "CSS Class", "CSSClass", "The CSS class that will be appended on the div element.", 2013, "", "967B5819-B712-4EEC-AA0A-BE0EE08A199A" );
            RockMigrationHelper.AddAttributeQualifier( "967B5819-B712-4EEC-AA0A-BE0EE08A199A", "ispassword", "False", "8D23E6CD-825E-410D-8049-E1D40ECBB7D9" );
            RockMigrationHelper.AddAttributeQualifier( "967B5819-B712-4EEC-AA0A-BE0EE08A199A", "maxcharacters", "", "3DD28B4B-2A60-4708-AB4F-E72C7BFFF971" );
            RockMigrationHelper.AddAttributeQualifier( "967B5819-B712-4EEC-AA0A-BE0EE08A199A", "showcountdown", "False", "A63B274F-75C0-4AA7-B4B0-59559831D70C" );
            RockMigrationHelper.UpdateDefinedValue( "A72D940B-2A69-44B8-931C-7FE99824D84C", "No Style", "No additional styling", "85CA07EE-6888-43FD-B8BF-24E4DD35C725", false );
            RockMigrationHelper.UpdateDefinedValue( "A72D940B-2A69-44B8-931C-7FE99824D84C", "Well", "This will apply a 'well' with a muted background color and some padding.", "2D6369C1-3B39-4E94-8122-78C55A962C33", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2D6369C1-3B39-4E94-8122-78C55A962C33", "967B5819-B712-4EEC-AA0A-BE0EE08A199A", @"well" );

            CreateTable(
                "dbo.WorkflowActionFormSection",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 500),
                        Description = c.String(),
                        ShowHeadingSeparator = c.Boolean(nullable: false),
                        SectionVisibilityRulesJSON = c.String(),
                        Order = c.Int(nullable: false),
                        WorkflowActionFormId = c.Int(nullable: false),
                        SectionTypeValueId = c.Int(),
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
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.DefinedValue", t => t.SectionTypeValueId)
                .ForeignKey("dbo.WorkflowActionForm", t => t.WorkflowActionFormId, cascadeDelete: true)
                .Index(t => t.WorkflowActionFormId)
                .Index(t => t.SectionTypeValueId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.WorkflowFormBuilderTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Description = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        FormHeader = c.String(),
                        FormFooter = c.String(),
                        AllowPersonEntry = c.Boolean(nullable: false),
                        PersonEntrySettingsJson = c.String(),
                        ConfirmationEmailSettingsJson = c.String(),
                        CompletionSettingsJson = c.String(),
                        IsLoginRequired = c.Boolean(nullable: false),
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
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            AddColumn("dbo.WorkflowType", "FormBuilderTemplateId", c => c.Int());
            AddColumn("dbo.WorkflowType", "IsFormBuilder", c => c.Boolean(nullable: false));
            AddColumn("dbo.WorkflowType", "FormBuilderSettingsJson", c => c.String());
            AddColumn("dbo.WorkflowType", "FormStartDateTime", c => c.DateTime());
            AddColumn("dbo.WorkflowType", "FormEndDateTime", c => c.DateTime());
            AddColumn("dbo.WorkflowType", "WorkflowExpireDateTime", c => c.DateTime());
            AddColumn("dbo.WorkflowType", "IsLoginRequired", c => c.Boolean(nullable: false));
            AddColumn("dbo.WorkflowActionFormAttribute", "ColumnSize", c => c.Int());
            AddColumn("dbo.WorkflowActionFormAttribute", "ActionFormSectionId", c => c.Int());
            CreateIndex("dbo.WorkflowType", "FormBuilderTemplateId");
            CreateIndex("dbo.WorkflowActionFormAttribute", "ActionFormSectionId");
            AddForeignKey("dbo.WorkflowActionFormAttribute", "ActionFormSectionId", "dbo.WorkflowActionFormSection", "Id");
            AddForeignKey("dbo.WorkflowType", "FormBuilderTemplateId", "dbo.WorkflowFormBuilderTemplate", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.WorkflowType", "FormBuilderTemplateId", "dbo.WorkflowFormBuilderTemplate");
            DropForeignKey("dbo.WorkflowFormBuilderTemplate", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.WorkflowFormBuilderTemplate", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.WorkflowActionFormAttribute", "ActionFormSectionId", "dbo.WorkflowActionFormSection");
            DropForeignKey("dbo.WorkflowActionFormSection", "WorkflowActionFormId", "dbo.WorkflowActionForm");
            DropForeignKey("dbo.WorkflowActionFormSection", "SectionTypeValueId", "dbo.DefinedValue");
            DropForeignKey("dbo.WorkflowActionFormSection", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.WorkflowActionFormSection", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.WorkflowFormBuilderTemplate", new[] { "Guid" });
            DropIndex("dbo.WorkflowFormBuilderTemplate", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.WorkflowFormBuilderTemplate", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.WorkflowActionFormSection", new[] { "Guid" });
            DropIndex("dbo.WorkflowActionFormSection", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.WorkflowActionFormSection", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.WorkflowActionFormSection", new[] { "SectionTypeValueId" });
            DropIndex("dbo.WorkflowActionFormSection", new[] { "WorkflowActionFormId" });
            DropIndex("dbo.WorkflowActionFormAttribute", new[] { "ActionFormSectionId" });
            DropIndex("dbo.WorkflowType", new[] { "FormBuilderTemplateId" });
            DropColumn("dbo.WorkflowActionFormAttribute", "ActionFormSectionId");
            DropColumn("dbo.WorkflowActionFormAttribute", "ColumnSize");
            DropColumn("dbo.WorkflowType", "IsLoginRequired");
            DropColumn("dbo.WorkflowType", "WorkflowExpireDateTime");
            DropColumn("dbo.WorkflowType", "FormEndDateTime");
            DropColumn("dbo.WorkflowType", "FormStartDateTime");
            DropColumn("dbo.WorkflowType", "FormBuilderSettingsJson");
            DropColumn("dbo.WorkflowType", "IsFormBuilder");
            DropColumn("dbo.WorkflowType", "FormBuilderTemplateId");
            DropTable("dbo.WorkflowFormBuilderTemplate");
            DropTable("dbo.WorkflowActionFormSection");

            RockMigrationHelper.DeleteAttribute( "967B5819-B712-4EEC-AA0A-BE0EE08A199A" ); // CSSClass
            RockMigrationHelper.DeleteDefinedValue( "2D6369C1-3B39-4E94-8122-78C55A962C33" ); // Well
            RockMigrationHelper.DeleteDefinedValue( "85CA07EE-6888-43FD-B8BF-24E4DD35C725" ); // No Style
            RockMigrationHelper.DeleteDefinedType( "A72D940B-2A69-44B8-931C-7FE99824D84C" ); // Section Type
        }
    }
}
