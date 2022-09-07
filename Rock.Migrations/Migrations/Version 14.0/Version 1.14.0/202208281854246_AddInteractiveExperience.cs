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
    public partial class AddInteractiveExperience : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.InteractiveExperienceAction",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        InteractiveExperienceId = c.Int(nullable: false),
                        ActionEntityTypeId = c.Int(nullable: false),
                        ResponseVisualEntityTypeId = c.Int(),
                        IsModerationRequired = c.Boolean(nullable: false),
                        IsMultipleSubmissionAllowed = c.Boolean(nullable: false),
                        IsResponseAnonymous = c.Boolean(nullable: false),
                        ActionSettingsJson = c.String(),
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
                .ForeignKey("dbo.EntityType", t => t.ActionEntityTypeId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.InteractiveExperience", t => t.InteractiveExperienceId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.EntityType", t => t.ResponseVisualEntityTypeId)
                .Index(t => t.InteractiveExperienceId)
                .Index(t => t.ActionEntityTypeId)
                .Index(t => t.ResponseVisualEntityTypeId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.InteractiveExperience",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        IsActive = c.Boolean(nullable: false),
                        Description = c.String(),
                        PublicLabel = c.String(maxLength: 100),
                        PhotoBinaryFileId = c.Int(),
                        PushNotificationType = c.Int(nullable: false),
                        WelcomeTitle = c.String(maxLength: 100),
                        WelcomeMessage = c.String(maxLength: 1000),
                        WelcomeHeaderImageBinaryFileId = c.Int(),
                        NoActionTitle = c.String(maxLength: 100),
                        NoActionMessage = c.String(maxLength: 1000),
                        NoActionHeaderImageBinaryFileId = c.Int(),
                        ActionBackgroundColor = c.String(maxLength: 25),
                        ActionTextColor = c.String(maxLength: 25),
                        ActionPrimaryButtonColor = c.String(maxLength: 25),
                        ActionPrimaryButtonTextColor = c.String(maxLength: 25),
                        ActionSecondaryButtonColor = c.String(maxLength: 25),
                        ActionSecondaryButtonTextColor = c.String(maxLength: 25),
                        ActionBackgroundImageBinaryFileId = c.Int(),
                        ActionCustomCss = c.String(),
                        AudienceBackgroundColor = c.String(maxLength: 25),
                        AudienceTextColor = c.String(maxLength: 25),
                        AudiencePrimaryColor = c.String(maxLength: 25),
                        AudienceSecondaryColor = c.String(maxLength: 25),
                        AudienceAccentColor = c.String(maxLength: 25),
                        AudienceBackgroundImageBinaryFileId = c.Int(),
                        AudienceCustomCss = c.String(),
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
                .ForeignKey("dbo.BinaryFile", t => t.ActionBackgroundImageBinaryFileId)
                .ForeignKey("dbo.BinaryFile", t => t.AudienceBackgroundImageBinaryFileId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.BinaryFile", t => t.NoActionHeaderImageBinaryFileId)
                .ForeignKey("dbo.BinaryFile", t => t.PhotoBinaryFileId)
                .ForeignKey("dbo.BinaryFile", t => t.WelcomeHeaderImageBinaryFileId)
                .Index(t => t.PhotoBinaryFileId)
                .Index(t => t.WelcomeHeaderImageBinaryFileId)
                .Index(t => t.NoActionHeaderImageBinaryFileId)
                .Index(t => t.ActionBackgroundImageBinaryFileId)
                .Index(t => t.AudienceBackgroundImageBinaryFileId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.InteractiveExperienceSchedule",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        InteractiveExperienceId = c.Int(nullable: false),
                        ScheduleId = c.Int(nullable: false),
                        DataViewId = c.Int(),
                        GroupId = c.Int(),
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
                .ForeignKey("dbo.DataView", t => t.DataViewId)
                .ForeignKey("dbo.Group", t => t.GroupId)
                .ForeignKey("dbo.InteractiveExperience", t => t.InteractiveExperienceId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.Schedule", t => t.ScheduleId)
                .Index(t => t.InteractiveExperienceId)
                .Index(t => t.ScheduleId)
                .Index(t => t.DataViewId)
                .Index(t => t.GroupId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.InteractiveExperienceScheduleCampus",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        InteractiveExperienceScheduleId = c.Int(nullable: false),
                        CampusId = c.Int(nullable: false),
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
                .ForeignKey("dbo.Campus", t => t.CampusId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.InteractiveExperienceSchedule", t => t.InteractiveExperienceScheduleId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.InteractiveExperienceScheduleId)
                .Index(t => t.CampusId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.InteractiveExperienceAnswer",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        InteractiveExperienceScheduleId = c.Int(nullable: false),
                        InteractiveExperienceActionId = c.Int(nullable: false),
                        CampusId = c.Int(),
                        Response = c.String(),
                        ResponseDateTime = c.DateTime(nullable: false),
                        PersonAliasId = c.Int(),
                        InteractionSessionId = c.Int(),
                        ApprovalStatus = c.Int(nullable: false),
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
                .ForeignKey("dbo.Campus", t => t.CampusId)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.InteractiveExperienceAction", t => t.InteractiveExperienceActionId)
                .ForeignKey("dbo.InteractiveExperienceSchedule", t => t.InteractiveExperienceScheduleId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId)
                .Index(t => t.InteractiveExperienceScheduleId)
                .Index(t => t.InteractiveExperienceActionId)
                .Index(t => t.CampusId)
                .Index(t => t.PersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.InteractiveExperienceAnswer", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractiveExperienceAnswer", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractiveExperienceAnswer", "InteractiveExperienceScheduleId", "dbo.InteractiveExperienceSchedule");
            DropForeignKey("dbo.InteractiveExperienceAnswer", "InteractiveExperienceActionId", "dbo.InteractiveExperienceAction");
            DropForeignKey("dbo.InteractiveExperienceAnswer", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractiveExperienceAnswer", "CampusId", "dbo.Campus");
            DropForeignKey("dbo.InteractiveExperienceAction", "ResponseVisualEntityTypeId", "dbo.EntityType");
            DropForeignKey("dbo.InteractiveExperienceAction", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractiveExperienceAction", "InteractiveExperienceId", "dbo.InteractiveExperience");
            DropForeignKey("dbo.InteractiveExperience", "WelcomeHeaderImageBinaryFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.InteractiveExperience", "PhotoBinaryFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.InteractiveExperience", "NoActionHeaderImageBinaryFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.InteractiveExperience", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractiveExperienceSchedule", "ScheduleId", "dbo.Schedule");
            DropForeignKey("dbo.InteractiveExperienceSchedule", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractiveExperienceScheduleCampus", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractiveExperienceScheduleCampus", "InteractiveExperienceScheduleId", "dbo.InteractiveExperienceSchedule");
            DropForeignKey("dbo.InteractiveExperienceScheduleCampus", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractiveExperienceScheduleCampus", "CampusId", "dbo.Campus");
            DropForeignKey("dbo.InteractiveExperienceSchedule", "InteractiveExperienceId", "dbo.InteractiveExperience");
            DropForeignKey("dbo.InteractiveExperienceSchedule", "GroupId", "dbo.Group");
            DropForeignKey("dbo.InteractiveExperienceSchedule", "DataViewId", "dbo.DataView");
            DropForeignKey("dbo.InteractiveExperienceSchedule", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractiveExperience", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractiveExperience", "AudienceBackgroundImageBinaryFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.InteractiveExperience", "ActionBackgroundImageBinaryFileId", "dbo.BinaryFile");
            DropForeignKey("dbo.InteractiveExperienceAction", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.InteractiveExperienceAction", "ActionEntityTypeId", "dbo.EntityType");
            DropIndex("dbo.InteractiveExperienceAnswer", new[] { "Guid" });
            DropIndex("dbo.InteractiveExperienceAnswer", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.InteractiveExperienceAnswer", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.InteractiveExperienceAnswer", new[] { "PersonAliasId" });
            DropIndex("dbo.InteractiveExperienceAnswer", new[] { "CampusId" });
            DropIndex("dbo.InteractiveExperienceAnswer", new[] { "InteractiveExperienceActionId" });
            DropIndex("dbo.InteractiveExperienceAnswer", new[] { "InteractiveExperienceScheduleId" });
            DropIndex("dbo.InteractiveExperienceScheduleCampus", new[] { "Guid" });
            DropIndex("dbo.InteractiveExperienceScheduleCampus", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.InteractiveExperienceScheduleCampus", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.InteractiveExperienceScheduleCampus", new[] { "CampusId" });
            DropIndex("dbo.InteractiveExperienceScheduleCampus", new[] { "InteractiveExperienceScheduleId" });
            DropIndex("dbo.InteractiveExperienceSchedule", new[] { "Guid" });
            DropIndex("dbo.InteractiveExperienceSchedule", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.InteractiveExperienceSchedule", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.InteractiveExperienceSchedule", new[] { "GroupId" });
            DropIndex("dbo.InteractiveExperienceSchedule", new[] { "DataViewId" });
            DropIndex("dbo.InteractiveExperienceSchedule", new[] { "ScheduleId" });
            DropIndex("dbo.InteractiveExperienceSchedule", new[] { "InteractiveExperienceId" });
            DropIndex("dbo.InteractiveExperience", new[] { "Guid" });
            DropIndex("dbo.InteractiveExperience", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.InteractiveExperience", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.InteractiveExperience", new[] { "AudienceBackgroundImageBinaryFileId" });
            DropIndex("dbo.InteractiveExperience", new[] { "ActionBackgroundImageBinaryFileId" });
            DropIndex("dbo.InteractiveExperience", new[] { "NoActionHeaderImageBinaryFileId" });
            DropIndex("dbo.InteractiveExperience", new[] { "WelcomeHeaderImageBinaryFileId" });
            DropIndex("dbo.InteractiveExperience", new[] { "PhotoBinaryFileId" });
            DropIndex("dbo.InteractiveExperienceAction", new[] { "Guid" });
            DropIndex("dbo.InteractiveExperienceAction", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.InteractiveExperienceAction", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.InteractiveExperienceAction", new[] { "ResponseVisualEntityTypeId" });
            DropIndex("dbo.InteractiveExperienceAction", new[] { "ActionEntityTypeId" });
            DropIndex("dbo.InteractiveExperienceAction", new[] { "InteractiveExperienceId" });
            DropTable("dbo.InteractiveExperienceAnswer");
            DropTable("dbo.InteractiveExperienceScheduleCampus");
            DropTable("dbo.InteractiveExperienceSchedule");
            DropTable("dbo.InteractiveExperience");
            DropTable("dbo.InteractiveExperienceAction");
        }
    }
}
