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
    public partial class WorkflowFormEntry : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Update checklist block to only be visible by Rock Administrators
            DeleteSecurityAuthForBlock( "62B1DBE6-B3D9-4C0B-BD12-1DD8C4F2C6EB" );
            AddSecurityAuthForBlock( "62B1DBE6-B3D9-4C0B-BD12-1DD8C4F2C6EB", 0, "View", true, "628C51A8-4613-43ED-A18D-4A6FB999273E", Model.SpecialRole.None, "B26794E2-7AB6-4F14-9E18-FEBA8A037C5F" );
            AddSecurityAuthForBlock( "62B1DBE6-B3D9-4C0B-BD12-1DD8C4F2C6EB", 1, "View", false, null, Model.SpecialRole.AllUsers, "DF120B3D-7AD2-4B16-A79A-D2156CC1DD9F" );

            CreateTable(
                "dbo.WorkflowActionFormAttribute",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        WorkflowActionFormId = c.Int(nullable: false),
                        AttributeId = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
                        IsVisible = c.Boolean(nullable: false),
                        IsReadOnly = c.Boolean(nullable: false),
                        IsRequired = c.Boolean(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Attribute", t => t.AttributeId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.WorkflowActionForm", t => t.WorkflowActionFormId, cascadeDelete: true)
                .Index(t => t.WorkflowActionFormId)
                .Index(t => t.AttributeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.WorkflowActionForm",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Header = c.String(),
                        Footer = c.String(),
                        InactiveMessage = c.String(),
                        Actions = c.String(maxLength: 300),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            AddColumn("dbo.WorkflowAction", "FormAction", c => c.String(maxLength: 20));
            AddColumn("dbo.WorkflowActionType", "WorkflowFormId", c => c.Int());
            CreateIndex("dbo.WorkflowActionType", "WorkflowFormId");
            AddForeignKey("dbo.WorkflowActionType", "WorkflowFormId", "dbo.WorkflowActionForm", "Id");

            // Remove workflow list block from workflow type detail page
            DeleteBlock( "B8DE3F9A-5C84-4D13-8B90-F1E768740269" );

            Sql( @"

    -- remove the 'IsSystem' from sample groups in install
    UPDATE [Group] SET
    [IsSystem] = 0
    WHERE [Guid] IN ('C05E60C4-6DFC-420D-8DBA-28DBB5F0E3F9', '3CBC1A3F-DD26-430A-9B53-B476CB385ABC')

    -----------------------------------------------------------------------------------------------
    -- START script for DataView: Pending People
    -----------------------------------------------------------------------------------------------
    DECLARE @PropertyFilterEntityTypeId INT
    SET @PropertyFilterEntityTypeId = (SELECT [Id] FROM [EntityType] WHERE [Guid] = '03F0D6AC-D181-48B6-B4BC-1F2652B55323')

    -- parent DataViewFilter for DataView: Pending People
    DECLARE @ParentDataViewFilterId INT
    INSERT INTO [DataViewFilter]
        ([ExpressionType]
        ,[Guid]
        ,[CreatedDateTime]
        ,[ModifiedDateTime])
    VALUES
        (1
        ,'9eb9455c-adba-435b-869b-264178484944'
        ,'4/29/2014 12:26:07 PM'
        ,'4/29/2014 12:26:07 PM')
    SET @ParentDataViewFilterId = SCOPE_IDENTITY()

    
        -- child DataViewFilters for DataView: Pending People
        INSERT INTO [DataViewFilter]
            ([ExpressionType]
            ,[ParentId]
            ,[EntityTypeId]
            ,[Selection]
            ,[Guid]
            ,[CreatedDateTime]
            ,[ModifiedDateTime])
        VALUES
            (0
            ,@ParentDataViewFilterId
            ,@PropertyFilterEntityTypeId
            ,N'[
  ""RecordStatusValueId"",
  ""[\r\n  \""5\""\r\n]""
]'
            ,'a3e1eb8e-cb2a-4ee0-9a27-3499392d5a1d'
            ,'4/29/2014 12:26:07 PM'
            ,'4/29/2014 12:26:07 PM')
            
    -- add DataView: Pending People
    DECLARE @CategoryId INT
    SET @CategoryId = (SELECT [Id] FROM [Category] WHERE [Guid] = 'bdd2c36f-7575-48a8-8b70-3a566e3811ed')

    DECLARE @DataViewId INT
    INSERT INTO [DataView]
        ([IsSystem]
        ,[Name]
        ,[Description]
        ,[CategoryId]
        ,[EntityTypeId]
        ,[DataViewFilterId]
        ,[TransformEntityTypeId]
        ,[Guid]
        ,[CreatedDateTime]
        ,[ModifiedDateTime])
    VALUES
        (0
        ,N'Pending People'
        ,N'Includes all people who have a status of Pending.'
        ,@CategoryId
        ,15
        ,@ParentDataViewFilterId
        ,NULL
        ,'6374c50d-4bbf-4b12-aa2c-9c2c92e5f32e'
        ,'4/29/2014 12:26:07 PM'
        ,'4/29/2014 12:26:07 PM')
    SET @DataViewId = SCOPE_IDENTITY()
    
    -----------------------------------------------------------------------------------------------
    -- END script for DataView: Pending People
    -----------------------------------------------------------------------------------------------
" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
-----------------------------------------------------------------------------------------------
    -- Uninstall DataView: Pending People
    DELETE [DataView] WHERE [Guid] = '6374c50d-4bbf-4b12-aa2c-9c2c92e5f32e'
    DELETE [DataViewFilter] WHERE [Guid] = 'a3e1eb8e-cb2a-4ee0-9a27-3499392d5a1d'
    DELETE [DataViewFilter] WHERE [Guid] = '9eb9455c-adba-435b-869b-264178484944'
-----------------------------------------------------------------------------------------------
" );
            DropForeignKey("dbo.WorkflowActionType", "WorkflowFormId", "dbo.WorkflowActionForm");
            DropForeignKey("dbo.WorkflowActionFormAttribute", "WorkflowActionFormId", "dbo.WorkflowActionForm");
            DropForeignKey("dbo.WorkflowActionForm", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.WorkflowActionForm", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.WorkflowActionFormAttribute", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.WorkflowActionFormAttribute", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.WorkflowActionFormAttribute", "AttributeId", "dbo.Attribute");
            DropIndex("dbo.WorkflowActionType", new[] { "WorkflowFormId" });
            DropIndex("dbo.WorkflowActionForm", new[] { "Guid" });
            DropIndex("dbo.WorkflowActionForm", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.WorkflowActionForm", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.WorkflowActionFormAttribute", new[] { "Guid" });
            DropIndex("dbo.WorkflowActionFormAttribute", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.WorkflowActionFormAttribute", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.WorkflowActionFormAttribute", new[] { "AttributeId" });
            DropIndex("dbo.WorkflowActionFormAttribute", new[] { "WorkflowActionFormId" });
            DropColumn("dbo.WorkflowActionType", "WorkflowFormId");
            DropColumn("dbo.WorkflowAction", "FormAction");
            DropTable("dbo.WorkflowActionForm");
            DropTable("dbo.WorkflowActionFormAttribute");
        }
    }
}
