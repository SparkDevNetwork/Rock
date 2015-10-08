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
    public partial class ConnectionModel : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.ConnectionRequest",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ConnectionOpportunityId = c.Int(nullable: false),
                        PersonAliasId = c.Int(nullable: false),
                        Comments = c.String(),
                        ConnectionStatusId = c.Int(nullable: false),
                        ConnectionState = c.Int(nullable: false),
                        FollowupDate = c.DateTime(),
                        CampusId = c.Int(),
                        AssignedGroupId = c.Int(),
                        ConnectorPersonAliasId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Group", t => t.AssignedGroupId)
                .ForeignKey("dbo.Campus", t => t.CampusId)
                .ForeignKey("dbo.ConnectionOpportunity", t => t.ConnectionOpportunityId, cascadeDelete: true)
                .ForeignKey("dbo.ConnectionStatus", t => t.ConnectionStatusId)
                .ForeignKey("dbo.PersonAlias", t => t.ConnectorPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId)
                .Index(t => t.ConnectionOpportunityId)
                .Index(t => t.PersonAliasId)
                .Index(t => t.ConnectionStatusId)
                .Index(t => t.CampusId)
                .Index(t => t.AssignedGroupId)
                .Index(t => t.ConnectorPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.ConnectionOpportunity",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        PublicName = c.String(nullable: false, maxLength: 50),
                        Description = c.String(nullable: false),
                        PhotoId = c.Int(),
                        ConnectionTypeId = c.Int(nullable: false),
                        GroupTypeId = c.Int(nullable: false),
                        ConnectorGroupId = c.Int(),
                        IconCssClass = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        GroupMemberRoleId = c.Int(),
                        GroupMemberStatusId = c.Int(),
                        UseAllGroupsOfType = c.Boolean(nullable: false),
                        GroupMemberStatus = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ConnectionType", t => t.ConnectionTypeId)
                .ForeignKey("dbo.Group", t => t.ConnectorGroupId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.GroupTypeRole", t => t.GroupMemberRoleId)
                .ForeignKey("dbo.GroupType", t => t.GroupTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.BinaryFile", t => t.PhotoId)
                .Index(t => t.PhotoId)
                .Index(t => t.ConnectionTypeId)
                .Index(t => t.GroupTypeId)
                .Index(t => t.ConnectorGroupId)
                .Index(t => t.GroupMemberRoleId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.ConnectionOpportunityCampus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ConnectionOpportunityId = c.Int(nullable: false),
                        CampusId = c.Int(nullable: false),
                        ConnectorGroupId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Campus", t => t.CampusId, cascadeDelete: true)
                .ForeignKey("dbo.ConnectionOpportunity", t => t.ConnectionOpportunityId, cascadeDelete: true)
                .ForeignKey("dbo.Group", t => t.ConnectorGroupId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.ConnectionOpportunityId)
                .Index(t => t.CampusId)
                .Index(t => t.ConnectorGroupId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.ConnectionOpportunityGroup",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ConnectionOpportunityId = c.Int(nullable: false),
                        GroupId = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ConnectionOpportunity", t => t.ConnectionOpportunityId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.Group", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.ConnectionOpportunityId)
                .Index(t => t.GroupId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.ConnectionType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(nullable: false),
                        IconCssClass = c.String(),
                        EnableFutureFollowup = c.Boolean(nullable: false),
                        EnableFullActivityList = c.Boolean(nullable: false),
                        OwnerPersonAliasId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.OwnerPersonAliasId)
                .Index(t => t.OwnerPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.ConnectionActivityType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        ConnectionTypeId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ConnectionType", t => t.ConnectionTypeId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.ConnectionTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.ConnectionStatus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(nullable: false),
                        ConnectionTypeId = c.Int(),
                        IsCritical = c.Boolean(nullable: false),
                        IsDefault = c.Boolean(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ConnectionType", t => t.ConnectionTypeId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.ConnectionTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.ConnectionWorkflow",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ConnectionTypeId = c.Int(),
                        ConnectionOpportunityId = c.Int(),
                        WorkflowTypeId = c.Int(nullable: false),
                        TriggerType = c.Int(nullable: false),
                        QualifierValue = c.String(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ConnectionOpportunity", t => t.ConnectionOpportunityId, cascadeDelete: true)
                .ForeignKey("dbo.ConnectionType", t => t.ConnectionTypeId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.WorkflowType", t => t.WorkflowTypeId, cascadeDelete: true)
                .Index(t => t.ConnectionTypeId)
                .Index(t => t.ConnectionOpportunityId)
                .Index(t => t.WorkflowTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.ConnectionRequestActivity",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ConnectionRequestId = c.Int(nullable: false),
                        ConnectionActivityTypeId = c.Int(nullable: false),
                        ConnectorPersonAliasId = c.Int(),
                        ConnectionOpportunityId = c.Int(nullable: false),
                        Note = c.String(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ConnectionActivityType", t => t.ConnectionActivityTypeId)
                .ForeignKey("dbo.ConnectionOpportunity", t => t.ConnectionOpportunityId)
                .ForeignKey("dbo.ConnectionRequest", t => t.ConnectionRequestId)
                .ForeignKey("dbo.PersonAlias", t => t.ConnectorPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.ConnectionRequestId)
                .Index(t => t.ConnectionActivityTypeId)
                .Index(t => t.ConnectorPersonAliasId)
                .Index(t => t.ConnectionOpportunityId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.ConnectionRequestWorkflow",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ConnectionRequestId = c.Int(nullable: false),
                        ConnectionWorkflowId = c.Int(nullable: false),
                        WorkflowId = c.Int(nullable: false),
                        TriggerType = c.Int(nullable: false),
                        TriggerQualifier = c.String(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ConnectionRequest", t => t.ConnectionRequestId, cascadeDelete: true)
                .ForeignKey("dbo.ConnectionWorkflow", t => t.ConnectionWorkflowId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.Workflow", t => t.WorkflowId)
                .Index(t => t.ConnectionRequestId)
                .Index(t => t.ConnectionWorkflowId)
                .Index(t => t.WorkflowId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);

            RockMigrationHelper.AddSecurityRoleGroup( "RSR - Involvement Administration", "Group of individuals who can administrate the various parts of the involvement functionality.", Rock.SystemGuid.Group.GROUP_CONNECTION_ADMINISTRATORS );

            Sql( @"
            INSERT [dbo].[ConnectionType]
            ([Name], [Description], [IconCssClass], [EnableFutureFollowup], [EnableFullActivityList], [OwnerPersonAliasId], [Guid], [ForeignId])
            VALUES (N'Involvement', N'A connection type for church member involvement.', N'fa fa-leaf', 1, 0, NULL, N'dd565087-a4be-4943-b123-bf22777e8426', NULL)
            " );           
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.ConnectionRequest", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionRequest", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionRequest", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionRequest", "ConnectorPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionRequest", "ConnectionStatusId", "dbo.ConnectionStatus");
            DropForeignKey("dbo.ConnectionRequestWorkflow", "WorkflowId", "dbo.Workflow");
            DropForeignKey("dbo.ConnectionRequestWorkflow", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionRequestWorkflow", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionRequestWorkflow", "ConnectionWorkflowId", "dbo.ConnectionWorkflow");
            DropForeignKey("dbo.ConnectionRequestWorkflow", "ConnectionRequestId", "dbo.ConnectionRequest");
            DropForeignKey("dbo.ConnectionRequestActivity", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionRequestActivity", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionRequestActivity", "ConnectorPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionRequestActivity", "ConnectionRequestId", "dbo.ConnectionRequest");
            DropForeignKey("dbo.ConnectionRequestActivity", "ConnectionOpportunityId", "dbo.ConnectionOpportunity");
            DropForeignKey("dbo.ConnectionRequestActivity", "ConnectionActivityTypeId", "dbo.ConnectionActivityType");
            DropForeignKey("dbo.ConnectionRequest", "ConnectionOpportunityId", "dbo.ConnectionOpportunity");
            DropForeignKey("dbo.ConnectionOpportunity", "PhotoId", "dbo.BinaryFile");
            DropForeignKey("dbo.ConnectionOpportunity", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionOpportunity", "GroupTypeId", "dbo.GroupType");
            DropForeignKey("dbo.ConnectionOpportunity", "GroupMemberRoleId", "dbo.GroupTypeRole");
            DropForeignKey("dbo.ConnectionOpportunity", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionOpportunity", "ConnectorGroupId", "dbo.Group");
            DropForeignKey("dbo.ConnectionOpportunity", "ConnectionTypeId", "dbo.ConnectionType");
            DropForeignKey("dbo.ConnectionType", "OwnerPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionType", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionType", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionWorkflow", "WorkflowTypeId", "dbo.WorkflowType");
            DropForeignKey("dbo.ConnectionWorkflow", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionWorkflow", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionWorkflow", "ConnectionTypeId", "dbo.ConnectionType");
            DropForeignKey("dbo.ConnectionWorkflow", "ConnectionOpportunityId", "dbo.ConnectionOpportunity");
            DropForeignKey("dbo.ConnectionStatus", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionStatus", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionStatus", "ConnectionTypeId", "dbo.ConnectionType");
            DropForeignKey("dbo.ConnectionActivityType", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionActivityType", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionActivityType", "ConnectionTypeId", "dbo.ConnectionType");
            DropForeignKey("dbo.ConnectionOpportunityGroup", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionOpportunityGroup", "GroupId", "dbo.Group");
            DropForeignKey("dbo.ConnectionOpportunityGroup", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionOpportunityGroup", "ConnectionOpportunityId", "dbo.ConnectionOpportunity");
            DropForeignKey("dbo.ConnectionOpportunityCampus", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionOpportunityCampus", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.ConnectionOpportunityCampus", "ConnectorGroupId", "dbo.Group");
            DropForeignKey("dbo.ConnectionOpportunityCampus", "ConnectionOpportunityId", "dbo.ConnectionOpportunity");
            DropForeignKey("dbo.ConnectionOpportunityCampus", "CampusId", "dbo.Campus");
            DropForeignKey("dbo.ConnectionRequest", "CampusId", "dbo.Campus");
            DropForeignKey("dbo.ConnectionRequest", "AssignedGroupId", "dbo.Group");
            DropIndex("dbo.ConnectionRequestWorkflow", new[] { "ForeignId" });
            DropIndex("dbo.ConnectionRequestWorkflow", new[] { "Guid" });
            DropIndex("dbo.ConnectionRequestWorkflow", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ConnectionRequestWorkflow", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ConnectionRequestWorkflow", new[] { "WorkflowId" });
            DropIndex("dbo.ConnectionRequestWorkflow", new[] { "ConnectionWorkflowId" });
            DropIndex("dbo.ConnectionRequestWorkflow", new[] { "ConnectionRequestId" });
            DropIndex("dbo.ConnectionRequestActivity", new[] { "ForeignId" });
            DropIndex("dbo.ConnectionRequestActivity", new[] { "Guid" });
            DropIndex("dbo.ConnectionRequestActivity", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ConnectionRequestActivity", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ConnectionRequestActivity", new[] { "ConnectionOpportunityId" });
            DropIndex("dbo.ConnectionRequestActivity", new[] { "ConnectorPersonAliasId" });
            DropIndex("dbo.ConnectionRequestActivity", new[] { "ConnectionActivityTypeId" });
            DropIndex("dbo.ConnectionRequestActivity", new[] { "ConnectionRequestId" });
            DropIndex("dbo.ConnectionWorkflow", new[] { "ForeignId" });
            DropIndex("dbo.ConnectionWorkflow", new[] { "Guid" });
            DropIndex("dbo.ConnectionWorkflow", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ConnectionWorkflow", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ConnectionWorkflow", new[] { "WorkflowTypeId" });
            DropIndex("dbo.ConnectionWorkflow", new[] { "ConnectionOpportunityId" });
            DropIndex("dbo.ConnectionWorkflow", new[] { "ConnectionTypeId" });
            DropIndex("dbo.ConnectionStatus", new[] { "ForeignId" });
            DropIndex("dbo.ConnectionStatus", new[] { "Guid" });
            DropIndex("dbo.ConnectionStatus", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ConnectionStatus", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ConnectionStatus", new[] { "ConnectionTypeId" });
            DropIndex("dbo.ConnectionActivityType", new[] { "ForeignId" });
            DropIndex("dbo.ConnectionActivityType", new[] { "Guid" });
            DropIndex("dbo.ConnectionActivityType", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ConnectionActivityType", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ConnectionActivityType", new[] { "ConnectionTypeId" });
            DropIndex("dbo.ConnectionType", new[] { "ForeignId" });
            DropIndex("dbo.ConnectionType", new[] { "Guid" });
            DropIndex("dbo.ConnectionType", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ConnectionType", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ConnectionType", new[] { "OwnerPersonAliasId" });
            DropIndex("dbo.ConnectionOpportunityGroup", new[] { "ForeignId" });
            DropIndex("dbo.ConnectionOpportunityGroup", new[] { "Guid" });
            DropIndex("dbo.ConnectionOpportunityGroup", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ConnectionOpportunityGroup", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ConnectionOpportunityGroup", new[] { "GroupId" });
            DropIndex("dbo.ConnectionOpportunityGroup", new[] { "ConnectionOpportunityId" });
            DropIndex("dbo.ConnectionOpportunityCampus", new[] { "ForeignId" });
            DropIndex("dbo.ConnectionOpportunityCampus", new[] { "Guid" });
            DropIndex("dbo.ConnectionOpportunityCampus", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ConnectionOpportunityCampus", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ConnectionOpportunityCampus", new[] { "ConnectorGroupId" });
            DropIndex("dbo.ConnectionOpportunityCampus", new[] { "CampusId" });
            DropIndex("dbo.ConnectionOpportunityCampus", new[] { "ConnectionOpportunityId" });
            DropIndex("dbo.ConnectionOpportunity", new[] { "ForeignId" });
            DropIndex("dbo.ConnectionOpportunity", new[] { "Guid" });
            DropIndex("dbo.ConnectionOpportunity", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ConnectionOpportunity", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ConnectionOpportunity", new[] { "GroupMemberRoleId" });
            DropIndex("dbo.ConnectionOpportunity", new[] { "ConnectorGroupId" });
            DropIndex("dbo.ConnectionOpportunity", new[] { "GroupTypeId" });
            DropIndex("dbo.ConnectionOpportunity", new[] { "ConnectionTypeId" });
            DropIndex("dbo.ConnectionOpportunity", new[] { "PhotoId" });
            DropIndex("dbo.ConnectionRequest", new[] { "ForeignId" });
            DropIndex("dbo.ConnectionRequest", new[] { "Guid" });
            DropIndex("dbo.ConnectionRequest", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.ConnectionRequest", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.ConnectionRequest", new[] { "ConnectorPersonAliasId" });
            DropIndex("dbo.ConnectionRequest", new[] { "AssignedGroupId" });
            DropIndex("dbo.ConnectionRequest", new[] { "CampusId" });
            DropIndex("dbo.ConnectionRequest", new[] { "ConnectionStatusId" });
            DropIndex("dbo.ConnectionRequest", new[] { "PersonAliasId" });
            DropIndex("dbo.ConnectionRequest", new[] { "ConnectionOpportunityId" });
            DropTable("dbo.ConnectionRequestWorkflow");
            DropTable("dbo.ConnectionRequestActivity");
            DropTable("dbo.ConnectionWorkflow");
            DropTable("dbo.ConnectionStatus");
            DropTable("dbo.ConnectionActivityType");
            DropTable("dbo.ConnectionType");
            DropTable("dbo.ConnectionOpportunityGroup");
            DropTable("dbo.ConnectionOpportunityCampus");
            DropTable("dbo.ConnectionOpportunity");
            DropTable("dbo.ConnectionRequest");
        }
    }
}
