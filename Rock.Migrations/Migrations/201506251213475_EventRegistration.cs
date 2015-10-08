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
    public partial class EventRegistration : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DropForeignKey("dbo.EventItemAudience", "EventItemId", "dbo.EventItem");
            DropForeignKey("dbo.EventCalendarItem", "EventItemId", "dbo.EventItem");
            DropForeignKey("dbo.EventItemCampus", "EventItemId", "dbo.EventItem");
            DropForeignKey("dbo.EventCalendarItem", "EventCalendarId", "dbo.EventCalendar");
            DropForeignKey("dbo.EventItemSchedule", "EventItemCampusId", "dbo.EventItemCampus");
            DropIndex("dbo.EventItemCampus", new[] { "ContactPersonAliasId" });
            DropIndex("dbo.EventItemCampus", "IX_Email");
            CreateTable(
                "dbo.EventItemCampusGroupMap",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EventItemCampusId = c.Int(),
                        RegistrationInstanceId = c.Int(),
                        GroupId = c.Int(),
                        PublicName = c.String(maxLength: 200),
                        UrlSlug = c.String(maxLength: 200),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.EventItemCampus", t => t.EventItemCampusId, cascadeDelete: true)
                .ForeignKey("dbo.Group", t => t.GroupId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.RegistrationInstance", t => t.RegistrationInstanceId, cascadeDelete: true)
                .Index(t => t.EventItemCampusId)
                .Index(t => t.RegistrationInstanceId)
                .Index(t => t.GroupId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.RegistrationInstance",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        RegistrationTemplateId = c.Int(nullable: false),
                        StartDateTime = c.DateTime(),
                        EndDateTime = c.DateTime(),
                        Details = c.String(),
                        MaxAttendees = c.Int(nullable: false),
                        AccountId = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        ContactName = c.String(maxLength: 200),
                        ContactEmail = c.String(maxLength: 200),
                        AdditionalReminderDetails = c.String(),
                        AdditionalConfirmationDetails = c.String(),
                        ReminderSentDateTime = c.DateTime(),
                        ConfirmationSentDateTime = c.DateTime(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.FinancialAccount", t => t.AccountId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.RegistrationTemplate", t => t.RegistrationTemplateId, cascadeDelete: true)
                .Index(t => t.RegistrationTemplateId)
                .Index(t => t.AccountId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.Registration",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RegistrationInstanceId = c.Int(nullable: false),
                        PersonAliasId = c.Int(),
                        FirstName = c.String(maxLength: 50),
                        LastName = c.String(maxLength: 50),
                        ConfirmationEmail = c.String(maxLength: 75),
                        GroupId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.Group", t => t.GroupId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId)
                .ForeignKey("dbo.RegistrationInstance", t => t.RegistrationInstanceId)
                .Index(t => t.RegistrationInstanceId)
                .Index(t => t.PersonAliasId)
                .Index(t => t.GroupId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.RegistrationRegistrant",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RegistrationId = c.Int(nullable: false),
                        PersonAliasId = c.Int(),
                        GroupMemberId = c.Int(),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.GroupMember", t => t.GroupMemberId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId)
                .ForeignKey("dbo.Registration", t => t.RegistrationId, cascadeDelete: true)
                .Index(t => t.RegistrationId)
                .Index(t => t.PersonAliasId)
                .Index(t => t.GroupMemberId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.RegistrationRegistrantFee",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RegistrationRegistrantId = c.Int(nullable: false),
                        RegistrationTemplateFeeId = c.Int(nullable: false),
                        Quantity = c.Int(nullable: false),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 2),
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
                .ForeignKey("dbo.RegistrationRegistrant", t => t.RegistrationRegistrantId, cascadeDelete: true)
                .ForeignKey("dbo.RegistrationTemplateFee", t => t.RegistrationTemplateFeeId)
                .Index(t => t.RegistrationRegistrantId)
                .Index(t => t.RegistrationTemplateFeeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.RegistrationTemplateFee",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        RegistrationTemplateId = c.Int(nullable: false),
                        FeeType = c.Int(nullable: false),
                        CostValue = c.String(maxLength: 400),
                        DiscountApplies = c.Boolean(nullable: false),
                        AllowMultiple = c.Boolean(nullable: false),
                        Order = c.Int(nullable: false),
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
                .ForeignKey("dbo.RegistrationTemplate", t => t.RegistrationTemplateId, cascadeDelete: true)
                .Index(t => t.RegistrationTemplateId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.RegistrationTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        CategoryId = c.Int(),
                        GroupTypeId = c.Int(),
                        GroupMemberRoleId = c.Int(),
                        GroupMemberStatus = c.Int(nullable: false),
                        NotifyGroupLeaders = c.Boolean(nullable: false),
                        FeeTerm = c.String(maxLength: 100),
                        RegistrantTerm = c.String(maxLength: 100),
                        RegistrationTerm = c.String(maxLength: 100),
                        DiscountCodeTerm = c.String(maxLength: 100),
                        UseDefaultConfirmationEmail = c.Boolean(nullable: false),
                        ConfirmationEmailTemplate = c.String(),
                        ReminderEmailTemplate = c.String(),
                        Cost = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MinimumInitialPayment = c.Decimal(nullable: false, precision: 18, scale: 2),
                        LoginRequired = c.Boolean(nullable: false),
                        RegistrantsSameFamily = c.Int(nullable: false),
                        RequestEntryName = c.String(),
                        SuccessTitle = c.String(),
                        SuccessText = c.String(),
                        AllowMultipleRegistrants = c.Boolean(nullable: false),
                        MaxRegistrants = c.Int(nullable: false),
                        FinancialGatewayId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Category", t => t.CategoryId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.FinancialGateway", t => t.FinancialGatewayId)
                .ForeignKey("dbo.GroupType", t => t.GroupTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CategoryId)
                .Index(t => t.GroupTypeId)
                .Index(t => t.FinancialGatewayId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.RegistrationTemplateDiscount",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(nullable: false, maxLength: 100),
                        RegistrationTemplateId = c.Int(nullable: false),
                        DiscountPercentage = c.Double(nullable: false),
                        DiscountAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Order = c.Int(nullable: false),
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
                .ForeignKey("dbo.RegistrationTemplate", t => t.RegistrationTemplateId, cascadeDelete: true)
                .Index(t => t.RegistrationTemplateId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.RegistrationTemplateForm",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        RegistrationTemplateId = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
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
                .ForeignKey("dbo.RegistrationTemplate", t => t.RegistrationTemplateId, cascadeDelete: true)
                .Index(t => t.RegistrationTemplateId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            CreateTable(
                "dbo.RegistrationTemplateFormField",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RegistrationTemplateFormId = c.Int(nullable: false),
                        FieldSource = c.Int(nullable: false),
                        PersonFieldType = c.Int(nullable: false),
                        AttributeId = c.Int(),
                        IsSharedValue = c.Boolean(nullable: false),
                        ShowCurrentValue = c.Boolean(nullable: false),
                        PreText = c.String(),
                        PostText = c.String(),
                        IsGridField = c.Boolean(nullable: false),
                        IsRequired = c.Boolean(nullable: false),
                        Order = c.Int(nullable: false),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Attribute", t => t.AttributeId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.RegistrationTemplateForm", t => t.RegistrationTemplateFormId, cascadeDelete: true)
                .Index(t => t.RegistrationTemplateFormId)
                .Index(t => t.AttributeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId);
            
            AddColumn("dbo.EventItem", "IsApproved", c => c.Boolean(nullable: false));
            AddColumn("dbo.EventItem", "ApprovedByPersonAliasId", c => c.Int());
            AddColumn("dbo.EventItem", "ApprovedOnDateTime", c => c.DateTime());
            AlterColumn("dbo.BinaryFileType", "IconCssClass", c => c.String(maxLength: 100));
            AlterColumn("dbo.GroupType", "IconCssClass", c => c.String(maxLength: 100));
            AlterColumn("dbo.WorkflowType", "IconCssClass", c => c.String(maxLength: 100));
            AlterColumn("dbo.Attribute", "IconCssClass", c => c.String(maxLength: 100));
            AlterColumn("dbo.Page", "IconCssClass", c => c.String(maxLength: 100));
            AlterColumn("dbo.Metric", "IconCssClass", c => c.String(maxLength: 100));
            AlterColumn("dbo.EventItem", "DetailsUrl", c => c.String(maxLength: 200));
            AlterColumn("dbo.EventItem", "IsActive", c => c.Boolean(nullable: false));
            AlterColumn("dbo.EventCalendar", "IconCssClass", c => c.String(maxLength: 100));
            AlterColumn("dbo.EventItemCampus", "Location", c => c.String(maxLength: 200));
            AlterColumn("dbo.EventItemCampus", "ContactPersonAliasId", c => c.Int());
            AlterColumn("dbo.EventItemCampus", "ContactPhone", c => c.String(maxLength: 20));
            AlterColumn("dbo.EventItemCampus", "ContactEmail", c => c.String(maxLength: 75));
            AlterColumn("dbo.EventItemSchedule", "ScheduleName", c => c.String(nullable: false, maxLength: 100));
            CreateIndex("dbo.EventItem", "ApprovedByPersonAliasId");
            CreateIndex("dbo.EventItemCampus", "ContactPersonAliasId");
            CreateIndex("dbo.EventItemCampus", "ContactEmail", name: "IX_Email");
            AddForeignKey("dbo.EventItem", "ApprovedByPersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.EventItemAudience", "EventItemId", "dbo.EventItem", "Id", cascadeDelete: true);
            AddForeignKey("dbo.EventCalendarItem", "EventItemId", "dbo.EventItem", "Id", cascadeDelete: true);
            AddForeignKey("dbo.EventItemCampus", "EventItemId", "dbo.EventItem", "Id", cascadeDelete: true);
            AddForeignKey("dbo.EventCalendarItem", "EventCalendarId", "dbo.EventCalendar", "Id", cascadeDelete: true);
            AddForeignKey("dbo.EventItemSchedule", "EventItemCampusId", "dbo.EventItemCampus", "Id", cascadeDelete: true);
            DropColumn("dbo.EventItemCampus", "RegistrationUrl");

            Sql( @"
    DECLARE @MaxAccountOrder int = ( SELECT TOP 1 [Order] FROM [FinancialAccount] WHERE [ParentAccountId] IS NULL )
    INSERT INTO [dbo].[FinancialAccount] ( [Name], [IsTaxDeductible], [Order], [IsActive], [Guid]) 
    VALUES ( 'Event Registration', 0, COALESCE(@MaxAccountOrder + 1, 0), 1, '2A6F9E5F-6859-44F1-AB0E-CE9CF6B08EE5')
" );

            RockMigrationHelper.UpdateEntityType( "Rock.Model.RegistrationTemplate", "A01E3E99-A8AD-4C6C-BAAC-98795738BA70", true, true );

            RockMigrationHelper.AddPage( "7FB33834-F40A-4221-8849-BB8C06903B04", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Campus Detail", "", "4B0C44EE-28E3-4753-A95B-8C57CD958FD1", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "F7105BFE-B28C-41B6-9CE6-F1018D77DD8F", "0CB60906-6B74-44FD-AB25-026050EF70EB", "Event Registration", "", "614AF351-6C48-4B6B-B50E-9F7E03BC00A4", "fa fa-clipboard" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "614AF351-6C48-4B6B-B50E-9F7E03BC00A4", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Registration Instance", "", "844DC54B-DAEC-47B3-A63A-712DD6D57793", "fa fa-file-o" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "844DC54B-DAEC-47B3-A63A-712DD6D57793", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Registration Detail", "", "FC81099A-2F98-4EBA-AC5A-8300B2FE46C4", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "844DC54B-DAEC-47B3-A63A-712DD6D57793", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Linkage", "", "DE4B12F0-C3E6-451C-9E35-7E9E66A01F4E", "" ); // Site:Rock RMS

            Sql( @"
    UPDATE [Page] SET [BreadCrumbDisplayName] = 0 
    WHERE [GUID] IN ( '614AF351-6C48-4B6B-B50E-9F7E03BC00A4', '844DC54B-DAEC-47B3-A63A-712DD6D57793', 'FC81099A-2F98-4EBA-AC5A-8300B2FE46C4' )
" );

            RockMigrationHelper.UpdateBlockType( "Calendar Item Campus Detail", "Displays the details of a given calendar item at a campus.", "~/Blocks/Event/CalendarItemCampusDetail.ascx", "Event", "C18CB1DC-B2BC-4D3F-918A-A047183E4024" );
            RockMigrationHelper.UpdateBlockType( "Calendar Item Campus List", "Displays the campus details for a given calendar item.", "~/Blocks/Event/CalendarItemCampusList.ascx", "Event", "94230E7A-8EB7-4407-9B8E-888B54C71E39" );
            RockMigrationHelper.UpdateBlockType( "Registration Detail", "Displays the details of a given registration.", "~/Blocks/Event/RegistrationDetail.ascx", "Event", "A1C967B2-EEDA-416F-A53C-7BE46D6DA4E1" );
            RockMigrationHelper.UpdateBlockType( "Registration Instance Detail", "Template block for editing an event registration instance.", "~/Blocks/Event/RegistrationInstanceDetail.ascx", "Event", "22B67EDB-6D13-4D29-B722-DF45367AA3CB" );
            RockMigrationHelper.UpdateBlockType( "Registration Instance Linkage Detail", "Template block for editing a linkage associated to an event registration instance.", "~/Blocks/Event/RegistrationInstanceLinkageDetail.ascx", "Event", "D341EF12-406B-477D-8A85-16EBDDF2B04B" );
            RockMigrationHelper.UpdateBlockType( "Registration Instance List", "Lists all the instances of the given registration template.", "~/Blocks/Event/RegistrationInstanceList.ascx", "Event", "632F63A9-5629-4731-BE6A-AB534EDD9BC9" );
            RockMigrationHelper.UpdateBlockType( "Registration Template Detail", "Displays the details of the given registration template.", "~/Blocks/Event/RegistrationTemplateDetail.ascx", "Event", "91354899-304E-44C7-BD0D-55F42E6505D3" );
            
            // Add Block to Page: Calendar Item, Site: Rock RMS
            RockMigrationHelper.AddBlock( "7FB33834-F40A-4221-8849-BB8C06903B04", "", "94230E7A-8EB7-4407-9B8E-888B54C71E39", "Calendar Item Campus List", "Main", "", "", 1, "828C8FE3-D5F8-4C22-BA81-844D704842EA" );
            // Add Block to Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "4B0C44EE-28E3-4753-A95B-8C57CD958FD1", "", "C18CB1DC-B2BC-4D3F-918A-A047183E4024", "Calendar Item Campus Detail", "Main", "", "", 0, "55564BCB-CC7D-40CE-BED9-B84BB9B88BDC" );
            // Add Block to Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlock( "614AF351-6C48-4B6B-B50E-9F7E03BC00A4", "", "ADE003C7-649B-466A-872B-B8AC952E7841", "Category Tree View", "Sidebar1", "", "", 0, "C9C18C22-6D23-4F96-AB40-296E66EE4142" );
            // Add Block to Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlock( "614AF351-6C48-4B6B-B50E-9F7E03BC00A4", "", "7BC54887-21C2-4688-BD1D-C1C8B9C86F7C", "Category Detail", "Main", "", "", 0, "FD6F7E46-E5E6-444B-B4DE-E1BE190130A1" );
            // Add Block to Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlock( "614AF351-6C48-4B6B-B50E-9F7E03BC00A4", "", "91354899-304E-44C7-BD0D-55F42E6505D3", "Registration Template Detail", "Main", "", "", 1, "D6372D00-9FA3-49BF-B0F2-0BE67B5F5D39" );
            // Add Block to Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlock( "614AF351-6C48-4B6B-B50E-9F7E03BC00A4", "", "632F63A9-5629-4731-BE6A-AB534EDD9BC9", "Registration Instance List", "Main", "", "", 2, "467FA6BC-7F52-4AB7-87CC-B16518B0FF6F" );
            // Add Block to Page: Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlock( "844DC54B-DAEC-47B3-A63A-712DD6D57793", "", "22B67EDB-6D13-4D29-B722-DF45367AA3CB", "Registration Instance Detail", "Main", "", "", 0, "5F44A3A8-500B-4C89-95CA-8C4246B53C3F" );
            // Add Block to Page: Registration Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "FC81099A-2F98-4EBA-AC5A-8300B2FE46C4", "", "A1C967B2-EEDA-416F-A53C-7BE46D6DA4E1", "Registration Detail", "Main", "", "", 0, "0E0AE4CB-E348-435E-A0CA-9E3B2FC6BA19" );
            // Add Block to Page: Linkage, Site: Rock RMS
            RockMigrationHelper.AddBlock( "DE4B12F0-C3E6-451C-9E35-7E9E66A01F4E", "", "D341EF12-406B-477D-8A85-16EBDDF2B04B", "Registration Instance Linkage Detail", "Main", "", "", 0, "C20CD139-2969-4481-A14F-603718ABD511" );

            // Attrib for BlockType: Calendar Item Campus List:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "94230E7A-8EB7-4407-9B8E-888B54C71E39", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "5F916A32-3503-4661-BDF5-03F0057BDADE" );
            // Attrib for BlockType: Registration Instance List:Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "632F63A9-5629-4731-BE6A-AB534EDD9BC9", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", "", 0, @"", "E4886210-4889-46A2-9667-DD0D9F340ADF" );
            // Attrib for BlockType: Registration Instance Detail:Linkage Page
            RockMigrationHelper.AddBlockTypeAttribute( "22B67EDB-6D13-4D29-B722-DF45367AA3CB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Linkage Page", "LinkagePage", "", "The page for editing registration linkages", 2, @"", "209F38DD-E18A-48EC-A3D9-EA7D7345E9ED" );
            // Attrib for BlockType: Registration Instance Detail:Registration Page
            RockMigrationHelper.AddBlockTypeAttribute( "22B67EDB-6D13-4D29-B722-DF45367AA3CB", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Registration Page", "RegistrationPage", "", "The page for editing registration and registrant information", 1, @"", "EFE0C033-74A7-45BA-A42E-DE9AEA2D8528" );
            // Attrib for BlockType: Registration Instance Detail:Default Account
            RockMigrationHelper.AddBlockTypeAttribute( "22B67EDB-6D13-4D29-B722-DF45367AA3CB", "434D7B6F-F8DD-45B7-8C3E-C76EF10BE56A", "Default Account", "DefaultAccount", "", "The default account to use for new registration instances", 0, @"2A6F9E5F-6859-44F1-AB0E-CE9CF6B08EE5", "4FA1CCF7-9921-41EE-BC5F-DE358A1E5A89" );
            // Attrib Value for Block:Calendar Item Campus List, Attribute:Detail Page Page: Calendar Item, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "828C8FE3-D5F8-4C22-BA81-844D704842EA", "5F916A32-3503-4661-BDF5-03F0057BDADE", @"4b0c44ee-28e3-4753-a95b-8c57cd958fd1" );
            // Attrib Value for Block:Category Tree View, Attribute:Page Parameter Key Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C9C18C22-6D23-4F96-AB40-296E66EE4142", "AA057D3E-00CC-42BD-9998-600873356EDB", @"RegistrationTemplateId" );
            // Attrib Value for Block:Category Tree View, Attribute:Entity Type Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C9C18C22-6D23-4F96-AB40-296E66EE4142", "06D414F0-AA20-4D3C-B297-1530CCD64395", @"a01e3e99-a8ad-4c6c-baac-98795738ba70" );
            // Attrib Value for Block:Category Tree View, Attribute:Detail Page Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C9C18C22-6D23-4F96-AB40-296E66EE4142", "AEE521D8-124D-4BB3-8A80-5F368E5CEC15", @"614af351-6c48-4b6b-b50e-9f7e03bc00a4" );
            // Attrib Value for Block:Category Tree View, Attribute:Entity Type Friendly Name Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C9C18C22-6D23-4F96-AB40-296E66EE4142", "07213E2C-C239-47CA-A781-F7A908756DC2", @"" );
            // Attrib Value for Block:Category Tree View, Attribute:Show Unnamed Entity Items Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C9C18C22-6D23-4F96-AB40-296E66EE4142", "C48600CD-2C65-46EF-84E8-975F0DE8C28E", @"True" );
            // Attrib Value for Block:Category Tree View, Attribute:Exclude Categories Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C9C18C22-6D23-4F96-AB40-296E66EE4142", "61398707-FCCE-4AFD-8374-110BCA275F34", @"" );
            // Attrib Value for Block:Category Tree View, Attribute:Root Category Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C9C18C22-6D23-4F96-AB40-296E66EE4142", "2080E00B-63E4-4874-B06A-ADE1B4F3B3AD", @"" );
            // Attrib Value for Block:Category Tree View, Attribute:Entity Type Qualifier Property Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C9C18C22-6D23-4F96-AB40-296E66EE4142", "2D5FF74A-D316-4924-BCD2-6AA338D8DAAC", @"" );
            // Attrib Value for Block:Category Tree View, Attribute:Entity type Qualifier Value Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "C9C18C22-6D23-4F96-AB40-296E66EE4142", "F76C5EEF-FD45-4BD6-A903-ED5AB53BB928", @"" );
            // Attrib Value for Block:Category Detail, Attribute:Entity Type Qualifier Property Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FD6F7E46-E5E6-444B-B4DE-E1BE190130A1", "620957FF-BC28-4A89-A74F-C917DA5CFD47", @"" );
            // Attrib Value for Block:Category Detail, Attribute:Entity Type Qualifier Value Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FD6F7E46-E5E6-444B-B4DE-E1BE190130A1", "985128EE-D40C-4598-B14B-7AD728ECCE38", @"" );
            // Attrib Value for Block:Category Detail, Attribute:Entity Type Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FD6F7E46-E5E6-444B-B4DE-E1BE190130A1", "FF3A33CF-8897-4FC6-9C16-64FA25E6C297", @"a01e3e99-a8ad-4c6c-baac-98795738ba70" );
            // Attrib Value for Block:Category Detail, Attribute:Exclude Categories Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FD6F7E46-E5E6-444B-B4DE-E1BE190130A1", "AB542995-876F-4B8F-8417-11D83369289E", @"" );
            // Attrib Value for Block:Category Detail, Attribute:Root Category Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "FD6F7E46-E5E6-444B-B4DE-E1BE190130A1", "C3B72ADF-93D7-42CF-A103-8A7661A6926B", @"" );
            // Attrib Value for Block:Registration Instance List, Attribute:Detail Page Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "467FA6BC-7F52-4AB7-87CC-B16518B0FF6F", "E4886210-4889-46A2-9667-DD0D9F340ADF", @"844dc54b-daec-47b3-a63a-712dd6d57793" );
            // Attrib Value for Block:Registration Instance Detail, Attribute:Linkage Page Page: Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5F44A3A8-500B-4C89-95CA-8C4246B53C3F", "209F38DD-E18A-48EC-A3D9-EA7D7345E9ED", @"de4b12f0-c3e6-451c-9e35-7e9e66a01f4e" );
            // Attrib Value for Block:Registration Instance Detail, Attribute:Registration Page Page: Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5F44A3A8-500B-4C89-95CA-8C4246B53C3F", "EFE0C033-74A7-45BA-A42E-DE9AEA2D8528", @"fc81099a-2f98-4eba-ac5a-8300b2fe46c4" );
            // Attrib Value for Block:Registration Instance Detail, Attribute:Default Account Page: Registration Instance, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "5F44A3A8-500B-4C89-95CA-8C4246B53C3F", "4FA1CCF7-9921-41EE-BC5F-DE358A1E5A89", @"2a6f9e5f-6859-44f1-ab0e-ce9cf6b08ee5" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {

            // Attrib for BlockType: Registration Instance Detail:Default Account
            RockMigrationHelper.DeleteAttribute("4FA1CCF7-9921-41EE-BC5F-DE358A1E5A89");
            // Attrib for BlockType: Registration Instance Detail:Registration Page
            RockMigrationHelper.DeleteAttribute("EFE0C033-74A7-45BA-A42E-DE9AEA2D8528");
            // Attrib for BlockType: Registration Instance Detail:Linkage Page
            RockMigrationHelper.DeleteAttribute("209F38DD-E18A-48EC-A3D9-EA7D7345E9ED");
            // Attrib for BlockType: Registration Instance List:Detail Page
            RockMigrationHelper.DeleteAttribute("E4886210-4889-46A2-9667-DD0D9F340ADF");
            // Attrib for BlockType: Calendar Item Campus List:Detail Page
            RockMigrationHelper.DeleteAttribute("5F916A32-3503-4661-BDF5-03F0057BDADE");

            // Remove Block: Registration Instance Linkage Detail, from Page: Linkage, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("C20CD139-2969-4481-A14F-603718ABD511");
            // Remove Block: Registration Detail, from Page: Registration Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("0E0AE4CB-E348-435E-A0CA-9E3B2FC6BA19");
            // Remove Block: Registration Instance Detail, from Page: Registration Instance, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("5F44A3A8-500B-4C89-95CA-8C4246B53C3F");
            // Remove Block: Registration Instance List, from Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("467FA6BC-7F52-4AB7-87CC-B16518B0FF6F");
            // Remove Block: Registration Template Detail, from Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("D6372D00-9FA3-49BF-B0F2-0BE67B5F5D39");
            // Remove Block: Category Detail, from Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("FD6F7E46-E5E6-444B-B4DE-E1BE190130A1");
            // Remove Block: Category Tree View, from Page: Event Registration, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("C9C18C22-6D23-4F96-AB40-296E66EE4142");
            // Remove Block: Calendar Item Campus Detail, from Page: Campus Detail, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("55564BCB-CC7D-40CE-BED9-B84BB9B88BDC");
            // Remove Block: Calendar Item Campus List, from Page: Calendar Item, Site: Rock RMS
            RockMigrationHelper.DeleteBlock("828C8FE3-D5F8-4C22-BA81-844D704842EA");

            RockMigrationHelper.DeleteBlockType("91354899-304E-44C7-BD0D-55F42E6505D3"); // Registration Template Detail
            RockMigrationHelper.DeleteBlockType("632F63A9-5629-4731-BE6A-AB534EDD9BC9"); // Registration Instance List
            RockMigrationHelper.DeleteBlockType("D341EF12-406B-477D-8A85-16EBDDF2B04B"); // Registration Instance Linkage Detail
            RockMigrationHelper.DeleteBlockType("22B67EDB-6D13-4D29-B722-DF45367AA3CB"); // Registration Instance Detail
            RockMigrationHelper.DeleteBlockType("A1C967B2-EEDA-416F-A53C-7BE46D6DA4E1"); // Registration Detail
            RockMigrationHelper.DeleteBlockType("94230E7A-8EB7-4407-9B8E-888B54C71E39"); // Calendar Item Campus List
            RockMigrationHelper.DeleteBlockType("C18CB1DC-B2BC-4D3F-918A-A047183E4024"); // Calendar Item Campus Detail
            
            RockMigrationHelper.DeletePage("DE4B12F0-C3E6-451C-9E35-7E9E66A01F4E"); //  Page: Linkage, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage("FC81099A-2F98-4EBA-AC5A-8300B2FE46C4"); //  Page: Registration Detail, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage("844DC54B-DAEC-47B3-A63A-712DD6D57793"); //  Page: Registration Instance, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage("614AF351-6C48-4B6B-B50E-9F7E03BC00A4"); //  Page: Event Registration, Layout: Left Sidebar, Site: Rock RMS
            RockMigrationHelper.DeletePage("4B0C44EE-28E3-4753-A95B-8C57CD958FD1"); //  Page: Campus Detail, Layout: Full Width, Site: Rock RMS
            
            Sql( @"
    DELETE [dbo].[Registration]

    DELETE [dbo].[RegistrationTemplate]

    DELETE [FinancialTransaction]
    WHERE [Id] IN ( 
	    SELECT d.[TransactionId]
	    FROM [FinancialTransactionDetail] d
	    INNER JOIN [FinancialAccount] a ON a.[Id] = d.[AccountId]
	    WHERE a.[Guid] = '2A6F9E5F-6859-44F1-AB0E-CE9CF6B08EE5'
    )

    DELETE [dbo].[FinancialAccount] WHERE [Guid] = '2A6F9E5F-6859-44F1-AB0E-CE9CF6B08EE5'
" ); 
            
            AddColumn( "dbo.EventItemCampus", "RegistrationUrl", c => c.String() );
            DropForeignKey("dbo.EventItemSchedule", "EventItemCampusId", "dbo.EventItemCampus");
            DropForeignKey("dbo.EventCalendarItem", "EventCalendarId", "dbo.EventCalendar");
            DropForeignKey("dbo.EventItemCampus", "EventItemId", "dbo.EventItem");
            DropForeignKey("dbo.EventCalendarItem", "EventItemId", "dbo.EventItem");
            DropForeignKey("dbo.EventItemAudience", "EventItemId", "dbo.EventItem");
            DropForeignKey("dbo.EventItemCampusGroupMap", "RegistrationInstanceId", "dbo.RegistrationInstance");
            DropForeignKey("dbo.RegistrationInstance", "RegistrationTemplateId", "dbo.RegistrationTemplate");
            DropForeignKey("dbo.Registration", "RegistrationInstanceId", "dbo.RegistrationInstance");
            DropForeignKey("dbo.RegistrationRegistrant", "RegistrationId", "dbo.Registration");
            DropForeignKey("dbo.RegistrationRegistrant", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RegistrationRegistrant", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RegistrationRegistrant", "GroupMemberId", "dbo.GroupMember");
            DropForeignKey("dbo.RegistrationRegistrantFee", "RegistrationTemplateFeeId", "dbo.RegistrationTemplateFee");
            DropForeignKey("dbo.RegistrationTemplateFee", "RegistrationTemplateId", "dbo.RegistrationTemplate");
            DropForeignKey("dbo.RegistrationTemplate", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RegistrationTemplate", "GroupTypeId", "dbo.GroupType");
            DropForeignKey("dbo.RegistrationTemplateForm", "RegistrationTemplateId", "dbo.RegistrationTemplate");
            DropForeignKey("dbo.RegistrationTemplateForm", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RegistrationTemplateFormField", "RegistrationTemplateFormId", "dbo.RegistrationTemplateForm");
            DropForeignKey("dbo.RegistrationTemplateFormField", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RegistrationTemplateFormField", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RegistrationTemplateFormField", "AttributeId", "dbo.Attribute");
            DropForeignKey("dbo.RegistrationTemplateForm", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RegistrationTemplate", "FinancialGatewayId", "dbo.FinancialGateway");
            DropForeignKey("dbo.RegistrationTemplateDiscount", "RegistrationTemplateId", "dbo.RegistrationTemplate");
            DropForeignKey("dbo.RegistrationTemplateDiscount", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RegistrationTemplateDiscount", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RegistrationTemplate", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RegistrationTemplate", "CategoryId", "dbo.Category");
            DropForeignKey("dbo.RegistrationTemplateFee", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RegistrationTemplateFee", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RegistrationRegistrantFee", "RegistrationRegistrantId", "dbo.RegistrationRegistrant");
            DropForeignKey("dbo.RegistrationRegistrantFee", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RegistrationRegistrantFee", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RegistrationRegistrant", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Registration", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Registration", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Registration", "GroupId", "dbo.Group");
            DropForeignKey("dbo.Registration", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RegistrationInstance", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RegistrationInstance", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RegistrationInstance", "AccountId", "dbo.FinancialAccount");
            DropForeignKey("dbo.EventItemCampusGroupMap", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.EventItemCampusGroupMap", "GroupId", "dbo.Group");
            DropForeignKey("dbo.EventItemCampusGroupMap", "EventItemCampusId", "dbo.EventItemCampus");
            DropForeignKey("dbo.EventItemCampusGroupMap", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.EventItem", "ApprovedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.RegistrationTemplateFormField", new[] { "ForeignId" });
            DropIndex("dbo.RegistrationTemplateFormField", new[] { "Guid" });
            DropIndex("dbo.RegistrationTemplateFormField", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.RegistrationTemplateFormField", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.RegistrationTemplateFormField", new[] { "AttributeId" });
            DropIndex("dbo.RegistrationTemplateFormField", new[] { "RegistrationTemplateFormId" });
            DropIndex("dbo.RegistrationTemplateForm", new[] { "ForeignId" });
            DropIndex("dbo.RegistrationTemplateForm", new[] { "Guid" });
            DropIndex("dbo.RegistrationTemplateForm", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.RegistrationTemplateForm", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.RegistrationTemplateForm", new[] { "RegistrationTemplateId" });
            DropIndex("dbo.RegistrationTemplateDiscount", new[] { "ForeignId" });
            DropIndex("dbo.RegistrationTemplateDiscount", new[] { "Guid" });
            DropIndex("dbo.RegistrationTemplateDiscount", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.RegistrationTemplateDiscount", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.RegistrationTemplateDiscount", new[] { "RegistrationTemplateId" });
            DropIndex("dbo.RegistrationTemplate", new[] { "ForeignId" });
            DropIndex("dbo.RegistrationTemplate", new[] { "Guid" });
            DropIndex("dbo.RegistrationTemplate", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.RegistrationTemplate", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.RegistrationTemplate", new[] { "FinancialGatewayId" });
            DropIndex("dbo.RegistrationTemplate", new[] { "GroupTypeId" });
            DropIndex("dbo.RegistrationTemplate", new[] { "CategoryId" });
            DropIndex("dbo.RegistrationTemplateFee", new[] { "ForeignId" });
            DropIndex("dbo.RegistrationTemplateFee", new[] { "Guid" });
            DropIndex("dbo.RegistrationTemplateFee", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.RegistrationTemplateFee", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.RegistrationTemplateFee", new[] { "RegistrationTemplateId" });
            DropIndex("dbo.RegistrationRegistrantFee", new[] { "ForeignId" });
            DropIndex("dbo.RegistrationRegistrantFee", new[] { "Guid" });
            DropIndex("dbo.RegistrationRegistrantFee", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.RegistrationRegistrantFee", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.RegistrationRegistrantFee", new[] { "RegistrationTemplateFeeId" });
            DropIndex("dbo.RegistrationRegistrantFee", new[] { "RegistrationRegistrantId" });
            DropIndex("dbo.RegistrationRegistrant", new[] { "ForeignId" });
            DropIndex("dbo.RegistrationRegistrant", new[] { "Guid" });
            DropIndex("dbo.RegistrationRegistrant", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.RegistrationRegistrant", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.RegistrationRegistrant", new[] { "GroupMemberId" });
            DropIndex("dbo.RegistrationRegistrant", new[] { "PersonAliasId" });
            DropIndex("dbo.RegistrationRegistrant", new[] { "RegistrationId" });
            DropIndex("dbo.Registration", new[] { "ForeignId" });
            DropIndex("dbo.Registration", new[] { "Guid" });
            DropIndex("dbo.Registration", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.Registration", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.Registration", new[] { "GroupId" });
            DropIndex("dbo.Registration", new[] { "PersonAliasId" });
            DropIndex("dbo.Registration", new[] { "RegistrationInstanceId" });
            DropIndex("dbo.RegistrationInstance", new[] { "ForeignId" });
            DropIndex("dbo.RegistrationInstance", new[] { "Guid" });
            DropIndex("dbo.RegistrationInstance", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.RegistrationInstance", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.RegistrationInstance", new[] { "AccountId" });
            DropIndex("dbo.RegistrationInstance", new[] { "RegistrationTemplateId" });
            DropIndex("dbo.EventItemCampusGroupMap", new[] { "ForeignId" });
            DropIndex("dbo.EventItemCampusGroupMap", new[] { "Guid" });
            DropIndex("dbo.EventItemCampusGroupMap", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.EventItemCampusGroupMap", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.EventItemCampusGroupMap", new[] { "GroupId" });
            DropIndex("dbo.EventItemCampusGroupMap", new[] { "RegistrationInstanceId" });
            DropIndex("dbo.EventItemCampusGroupMap", new[] { "EventItemCampusId" });
            DropIndex("dbo.EventItemCampus", "IX_Email");
            DropIndex("dbo.EventItemCampus", new[] { "ContactPersonAliasId" });
            DropIndex("dbo.EventItem", new[] { "ApprovedByPersonAliasId" });
            AlterColumn("dbo.EventItemSchedule", "ScheduleName", c => c.String(nullable: false));
            AlterColumn("dbo.EventItemCampus", "ContactEmail", c => c.String(nullable: false, maxLength: 75));
            AlterColumn("dbo.EventItemCampus", "ContactPhone", c => c.String(nullable: false));
            AlterColumn("dbo.EventItemCampus", "ContactPersonAliasId", c => c.Int(nullable: false));
            AlterColumn("dbo.EventItemCampus", "Location", c => c.String(nullable: false));
            AlterColumn("dbo.EventCalendar", "IconCssClass", c => c.String());
            AlterColumn("dbo.EventItem", "IsActive", c => c.Boolean());
            AlterColumn("dbo.EventItem", "DetailsUrl", c => c.String());
            AlterColumn("dbo.Metric", "IconCssClass", c => c.String());
            AlterColumn("dbo.Page", "IconCssClass", c => c.String());
            AlterColumn("dbo.Attribute", "IconCssClass", c => c.String());
            AlterColumn("dbo.WorkflowType", "IconCssClass", c => c.String());
            AlterColumn("dbo.GroupType", "IconCssClass", c => c.String());
            AlterColumn("dbo.BinaryFileType", "IconCssClass", c => c.String());
            DropColumn("dbo.EventItem", "ApprovedOnDateTime");
            DropColumn("dbo.EventItem", "ApprovedByPersonAliasId");
            DropColumn("dbo.EventItem", "IsApproved");
            DropTable("dbo.RegistrationTemplateFormField");
            DropTable("dbo.RegistrationTemplateForm");
            DropTable("dbo.RegistrationTemplateDiscount");
            DropTable("dbo.RegistrationTemplate");
            DropTable("dbo.RegistrationTemplateFee");
            DropTable("dbo.RegistrationRegistrantFee");
            DropTable("dbo.RegistrationRegistrant");
            DropTable("dbo.Registration");
            DropTable("dbo.RegistrationInstance");
            DropTable("dbo.EventItemCampusGroupMap");
            CreateIndex("dbo.EventItemCampus", "ContactEmail", name: "IX_Email");
            CreateIndex("dbo.EventItemCampus", "ContactPersonAliasId");
            AddForeignKey("dbo.EventItemSchedule", "EventItemCampusId", "dbo.EventItemCampus", "Id");
            AddForeignKey("dbo.EventCalendarItem", "EventCalendarId", "dbo.EventCalendar", "Id");
            AddForeignKey("dbo.EventItemCampus", "EventItemId", "dbo.EventItem", "Id");
            AddForeignKey("dbo.EventCalendarItem", "EventItemId", "dbo.EventItem", "Id");
            AddForeignKey("dbo.EventItemAudience", "EventItemId", "dbo.EventItem", "Id");
        }
    }
}
