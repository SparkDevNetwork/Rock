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

namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class Assessments : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Attribute", "AbbreviatedName", c => c.String(maxLength: 100));
            AddFieldTypeConditionalScaleUp();
            CreateTablesUp();
            AddAssessmentTypes();
            AddConflictProfileDefinedTypeAndAttributesUp();
            ConvertDiscToAssessment();
            ConvertSpirtualGiftsToAssessment();
            AddEmotionalIntelligenceDefinedTypeAndAttributes();
            CreateAssessmentRequestSystemEmail();
            AssessmentRemindersServiceJobUp();
            CreateRequestAssessmentWorkflow();
            UpdateSpirtualGiftsResultsMessageBlockAttribute();
            PagesBlocksAndAttributesUp();
            AddMotivatorsAttributes();
            AddMotivatorsDefinedTypes();
            AddMotivatorsAssessmenPage();
            UpdateGiftsPageLayout();
            DefinedTypeCategoryTrueWiringToPersonalityAssessmentsUp();
            RenamePersonAttributeCategoryTrueWiring();
            UpdateSpiritualGiftsDefinedValuesUp();
            AddDiscProfilePersonAttributeUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            AddDiscProfilePersonAttributeDown();
            UpdateSpiritualGiftsDefinedValuesDown();
            DefinedTypeCategoryTrueWiringToPersonalityAssessmentsDown();
            PagesBlocksAndAttributesDown();
            AssessmentRemindersServiceJobDown();
            RockMigrationHelper.DeleteSystemEmail("41FF4269-7B48-40CD-81D4-C11370A13DED"); // Assessment Request System Email
            AddConflictProfileDefinedTypeAndAttributesDown();
            CreateTablesDown();
            DropColumn("dbo.Attribute", "AbbreviatedName");
            AddFieldTypeConditionalScaleDown();
        }

        private void CreateTablesUp()
        {
            CreateTable(
                "dbo.Assessment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonAliasId = c.Int(nullable: false),
                        AssessmentTypeId = c.Int(nullable: false),
                        RequesterPersonAliasId = c.Int(),
                        RequestedDateTime = c.DateTime(),
                        RequestedDueDate = c.DateTime(),
                        Status = c.Int(nullable: false),
                        CompletedDateTime = c.DateTime(),
                        AssessmentResultData = c.String(),
                        LastReminderDate = c.DateTime(),
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
                .ForeignKey("dbo.AssessmentType", t => t.AssessmentTypeId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.RequesterPersonAliasId)
                .Index(t => t.PersonAliasId)
                .Index(t => t.AssessmentTypeId)
                .Index(t => t.RequesterPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.AssessmentType",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false, maxLength: 100),
                        Description = c.String(nullable: false, maxLength: 100),
                        AssessmentPath = c.String(nullable: false, maxLength: 250),
                        AssessmentResultsPath = c.String(maxLength: 250),
                        IsActive = c.Boolean(nullable: false),
                        RequiresRequest = c.Boolean(nullable: false),
                        MinimumDaysToRetake = c.Int(nullable: false),
                        ValidDuration = c.Int(nullable: false),
                        IsSystem = c.Boolean(nullable: false),
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
        }

        private void CreateTablesDown()
        {
            DropForeignKey("dbo.Assessment", "RequesterPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Assessment", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Assessment", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Assessment", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Assessment", "AssessmentTypeId", "dbo.AssessmentType");
            DropForeignKey("dbo.AssessmentType", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.AssessmentType", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.AssessmentType", new[] { "Guid" });
            DropIndex("dbo.AssessmentType", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.AssessmentType", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.Assessment", new[] { "Guid" });
            DropIndex("dbo.Assessment", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.Assessment", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.Assessment", new[] { "RequesterPersonAliasId" });
            DropIndex("dbo.Assessment", new[] { "AssessmentTypeId" });
            DropIndex("dbo.Assessment", new[] { "PersonAliasId" });
            DropTable("dbo.AssessmentType");
            DropTable("dbo.Assessment");
        }

        private void AddFieldTypeConditionalScaleUp()
        {
            RockMigrationHelper.UpdateFieldType("Conditional Scale","","Rock","Rock.Field.Types.ConditionalScaleFieldType","E73B9F41-8325-4229-8EA5-75180066680C");
        }

        private void AddFieldTypeConditionalScaleDown()
        {
            RockMigrationHelper.DeleteFieldType( "E73B9F41-8325-4229-8EA5-75180066680C" );
        }

        private void AddAssessmentTypes()
        {
            AddAssessmentType( "DISC", "/DISC", SystemGuid.AssessmentType.DISC );
            AddAssessmentType( "Spiritual Gifts", "/SpiritualGifts", SystemGuid.AssessmentType.GIFTS );
            AddAssessmentType( "Conflict Profile", "/ConflictProfile", SystemGuid.AssessmentType.CONFLICT );
            AddAssessmentType( "Emotional Intelligence", "/EQ", SystemGuid.AssessmentType.EQ );
            AddAssessmentType( "Motivators", "/Motivators", SystemGuid.AssessmentType.MOTIVATORS );
        }

        private void AddAssessmentType( string title, string assessmentPath, string guid )
        {
            Sql( $@"
                INSERT INTO [dbo].[AssessmentType] (
                    [Title],
                    [Description],
                    [AssessmentPath],
                    [AssessmentResultsPath],
                    [IsActive], 
                    [RequiresRequest],
                    [MinimumDaysToRetake],
                    [ValidDuration],
                    [IsSystem], 
                    [Guid])
                VALUES (
                    '{title}',
                    '',
                    '{assessmentPath}',
                    '{assessmentPath}',
                    1,
                    0,
                    365,
                    730,
                    1,
                    '{guid}')");
        }

        private void AddConflictProfileDefinedTypeAndAttributesUp()
        {
            RockMigrationHelper.AddDefinedType( "Global", "Conflict Profile", "", Rock.SystemGuid.DefinedType.ASSESSMENT_CONFLICT_PROFILE, @"" );
            RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.ASSESSMENT_CONFLICT_PROFILE, "Avoiding", "Avoiding is not pursuing your own rights or those of the other person.", "663B0F4A-DE1F-46BE-8BDD-D7C98863DDC4", true );
            RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.ASSESSMENT_CONFLICT_PROFILE, "Compromising", "Compromising is finding a middle ground in the conflict.", "CF78D6B1-38AA-4FF7-9A4B-E900438FA85A", true );
            RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.ASSESSMENT_CONFLICT_PROFILE, "Resolving", "Resolving is attempting to work with the other person in depth to find the best solution regardless of where it may lie on the continuum.", "DF7B1EB2-7E7E-4F91-BD26-C6DFD88E38DF", true );
            RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.ASSESSMENT_CONFLICT_PROFILE, "Winning", "Winning is you believe you have the right answer and you must prove you are right whatever it takes.", "56300095-86AD-43FE-98D2-50829E9223C2", true );
            RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.ASSESSMENT_CONFLICT_PROFILE, "Yielding", "Yielding is neglecting your own interests and giving in to those of the other person.", "4AB06A6F-F5B1-4385-9365-199EA7969E50", true );
            
            // Create Conflict Engagement person attribute category
            RockMigrationHelper.UpdatePersonAttributeCategory( "Conflict Engagement", "", "", "EDD33F72-ECED-49BC-AC49-3643B60AD736" );

            // Person Attribute "Conflict Mode: Winning"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", @"EDD33F72-ECED-49BC-AC49-3643B60AD736", @"Conflict Mode: Winning", @"Mode: Winning", @"core_ConflictModeWinning", @"", @"", 0, @"", @"7147F706-388E-45E6-BE21-893FC7D652AA" );
            RockMigrationHelper.AddAttributeQualifier( @"7147F706-388E-45E6-BE21-893FC7D652AA", @"ConfigurationJSON", @"[{""Guid"":""951e4864-78c7-4d9c-8548-fc7d6a5cc91b"",""RangeIndex"":0,""Label"":""High"",""Color"":""#b21f22"",""HighValue"":100.0,""LowValue"":67.0},{""Guid"":""ca9fd375-5adf-4a56-ae84-5941015f454e"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#e15759"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""2f91c1ea-3d13-4a5c-9f33-ed8948e7fb1c"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#f0adae"",""HighValue"":33.0,""LowValue"":0.0}]", @"33306923-B938-4156-A4D6-4ABF75146B9D" );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_CONFLICT_MODE_WINNING );
            
            // Person Attribute "Conflict Mode: Resolving"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", @"EDD33F72-ECED-49BC-AC49-3643B60AD736", @"Conflict Mode: Resolving", @"Mode: Resolving", @"core_ConflictModeResolving", @"", @"", 0, @"", @"5B811EAC-51B2-41F2-A55A-C966D9DB05EE" );
            RockMigrationHelper.AddAttributeQualifier( @"5B811EAC-51B2-41F2-A55A-C966D9DB05EE", @"ConfigurationJSON", @"[{""Guid"":""022c15d2-9e23-46aa-a18b-3e9a49183adf"",""RangeIndex"":0,""Label"":""High"",""Color"":""#43678e"",""HighValue"":100.0,""LowValue"":67.0},{""Guid"":""7ca1d560-fc1e-4f20-b4e8-38ae3ab1385b"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#789abf"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""11d01d8f-d555-409f-b563-964461382a17"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#bdcee0"",""HighValue"":33.0,""LowValue"":0.0}]", @"67985444-26DE-46F1-8A93-41EE55052170" );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_CONFLICT_MODE_RESOLVING );
            
            // Person Attribute "Conflict Mode: Compromising"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", @"EDD33F72-ECED-49BC-AC49-3643B60AD736", @"Conflict Mode: Compromising", @"Mode: Compromising", @"core_ConflictModeCompromising", @"", @"", 0, @"", @"817D6B13-E4AA-4E93-8547-FE711A0065F2" );
            RockMigrationHelper.AddAttributeQualifier( @"817D6B13-E4AA-4E93-8547-FE711A0065F2", @"ConfigurationJSON", @"[{""Guid"":""b08b08fd-88a9-46cc-87c3-cb17368d4b56"",""RangeIndex"":0,""Label"":""High"",""Color"":""#43678e"",""HighValue"":100.0,""LowValue"":67.0},{""Guid"":""80c8f457-9afc-4dd1-8d18-a86037dba6c9"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#789abf"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""eba49b52-17c4-4813-9688-e316b767f295"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#bdcee0"",""HighValue"":33.0,""LowValue"":0.0}]", @"AACC439C-A2DC-40EC-BC22-96A1ABA4BB72" );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_CONFLICT_MODE_COMPROMISING );
            
            // Person Attribute "Conflict Mode: Avoiding"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", @"EDD33F72-ECED-49BC-AC49-3643B60AD736", @"Conflict Mode: Avoiding", @"Mode: Avoiding", @"core_ConflictModeAvoiding", @"", @"", 0, @"", @"071A8EFA-AD1C-436A-8E1E-23D215617004" );
            RockMigrationHelper.AddAttributeQualifier( @"071A8EFA-AD1C-436A-8E1E-23D215617004", @"ConfigurationJSON", @"[{""Guid"":""412a6f33-eee1-4179-a762-1df8a8b1ac6b"",""RangeIndex"":0,""Label"":""High"",""Color"":""#50aa3c"",""HighValue"":100.0,""LowValue"":67.0},{""Guid"":""25633115-9868-4c2a-89c7-97a15509c727"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#8cd17d"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""7e6bf4ac-16fa-47e4-a60d-956924ffc474"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#cdebc6"",""HighValue"":33.0,""LowValue"":0.0}]", @"1835F432-A316-4347-8840-2DA5C3E24D2C" );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_CONFLICT_MODE_AVOIDING );
            
            // Person Attribute "Conflict Mode: Yielding"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", @"EDD33F72-ECED-49BC-AC49-3643B60AD736", @"Conflict Mode: Yielding", @"Mode: Yielding", @"core_ConflictModeYielding", @"", @"", 0, @"", @"D30A33AD-7A60-43E0-84DA-E23600156BF7" );
            RockMigrationHelper.AddAttributeQualifier( @"D30A33AD-7A60-43E0-84DA-E23600156BF7", @"ConfigurationJSON", @"[{""Guid"":""542a5291-7d2d-40c1-bde1-fff6167e145a"",""RangeIndex"":0,""Label"":""High"",""Color"":""#50aa3c"",""HighValue"":100.0,""LowValue"":67.0},{""Guid"":""5efa046b-f235-4ce3-9568-4daafb4860a4"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#8cd17d"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""f765b351-8e60-4d49-a8c4-d14ab761cbb9"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#cdebc6"",""HighValue"":33.0,""LowValue"":0.0}]", @"45C648A3-26A9-47D8-B380-3B916FAAF701" );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_CONFLICT_MODE_YIELDING );
            
            // Person Attribute "Conflict Theme: Accommodating"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", @"EDD33F72-ECED-49BC-AC49-3643B60AD736", @"Conflict Theme: Accommodating", @"Theme: Accommodating", @"core_ConflictThemeAccommodating", @"", @"", 0, @"", @"404A64FB-7396-4896-9C94-84DE21E995CA" );
            RockMigrationHelper.AddAttributeQualifier( @"404A64FB-7396-4896-9C94-84DE21E995CA", @"ConfigurationJSON", @"[{""Guid"":""c7a0cf27-032a-4ce4-be51-228f56750ba7"",""RangeIndex"":0,""Label"":""High"",""Color"":""#50aa3c"",""HighValue"":100.0,""LowValue"":67.0},{""Guid"":""fe2c3be8-75e5-4ee1-8559-8f6fa531bf07"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#8cd17d"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""1fa912d5-772d-48d4-8d35-5a3791096777"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#cdebc6"",""HighValue"":33.0,""LowValue"":0.0}]", @"4ACECE69-D2E6-494B-96A2-4C5D15392935" );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_CONFLICT_THEME_ACCOMMODATING );
            
            // Person Attribute "Conflict Theme: Winning"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", @"EDD33F72-ECED-49BC-AC49-3643B60AD736", @"Conflict Theme: Winning", @"Theme: Winning", @"core_ConflictThemeWinning", @"", @"", 0, @"", @"6DE5878D-7CDB-404D-93A7-27CFF5E98C3B" );
            RockMigrationHelper.AddAttributeQualifier( @"6DE5878D-7CDB-404D-93A7-27CFF5E98C3B", @"ConfigurationJSON", @"[{""Guid"":""4185359a-af32-4902-82b4-d87e5729bd81"",""RangeIndex"":0,""Label"":""High"",""Color"":""#b21f22"",""HighValue"":100.0,""LowValue"":67.0},{""Guid"":""c22b5ba0-06c9-4d68-814a-3b7ac603f39c"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#e15759"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""56923739-16b5-4c58-8cc4-961800117bb5"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#f0adae"",""HighValue"":33.0,""LowValue"":0.0}]", @"F43DEF27-ABE2-4015-8F3A-BB52953C4389" );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_CONFLICT_THEME_WINNING );

            // Person Attribute "Conflict Theme: Solving"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", @"EDD33F72-ECED-49BC-AC49-3643B60AD736", @"Conflict Theme: Solving", @"Theme: Solving", @"core_ConflictThemeSolving", @"", @"", 0, @"", @"33235605-D8BB-4C1E-B231-6F085970A14F" );
            RockMigrationHelper.AddAttributeQualifier( @"33235605-D8BB-4C1E-B231-6F085970A14F", @"ConfigurationJSON", @"[{""Guid"":""fbd84cfb-bd89-4ed5-a1b3-834e2118ae9b"",""RangeIndex"":0,""Label"":""High"",""Color"":""#43678e"",""HighValue"":100.0,""LowValue"":67.0},{""Guid"":""c43e78c1-b675-48a2-8039-5784aadb8502"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#789abf"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""b2757ecb-8d72-4ccd-af6e-d8f022b2473d"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#bdcee0"",""HighValue"":33.0,""LowValue"":0.0}]", @"34483B88-0BDF-4E23-8E56-4183FA8DF1A1" );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_CONFLICT_THEME_SOLVING );
        }

        /// <summary>
        /// Adds the security to attribute. Deny View/Edit AllUsers.
        /// Grant View/Edit Administrators, Staff, Staff Like
        /// </summary>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        private void AddSecurityToAttribute( string attributeGuid )
        {
            RockMigrationHelper.AddSecurityAuthForAttribute(
               attributeGuid,
               0,
               Rock.Security.Authorization.VIEW,
               true,
               Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
               ( int ) Rock.Model.SpecialRole.None,
               Guid.NewGuid().ToString() );

            RockMigrationHelper.AddSecurityAuthForAttribute(
               attributeGuid,
               0,
               Rock.Security.Authorization.VIEW,
               true,
               Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS,
               ( int ) Rock.Model.SpecialRole.None,
               Guid.NewGuid().ToString() );

            RockMigrationHelper.AddSecurityAuthForAttribute(
               attributeGuid,
               1,
               Rock.Security.Authorization.VIEW,
               true,
               Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
               ( int ) Rock.Model.SpecialRole.None,
               Guid.NewGuid().ToString() );

            RockMigrationHelper.AddSecurityAuthForAttribute(
                attributeGuid,
                2,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                ( int ) Rock.Model.SpecialRole.AllUsers,
                Guid.NewGuid().ToString() );

            RockMigrationHelper.AddSecurityAuthForAttribute(
               attributeGuid,
               0,
               Rock.Security.Authorization.EDIT,
               true,
               Rock.SystemGuid.Group.GROUP_ADMINISTRATORS,
               ( int ) Rock.Model.SpecialRole.None,
               Guid.NewGuid().ToString() );

            RockMigrationHelper.AddSecurityAuthForAttribute(
               attributeGuid,
               0,
               Rock.Security.Authorization.EDIT,
               true,
               Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS,
               ( int ) Rock.Model.SpecialRole.None,
               Guid.NewGuid().ToString() );

            RockMigrationHelper.AddSecurityAuthForAttribute(
               attributeGuid,
               1,
               Rock.Security.Authorization.EDIT,
               true,
               Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
               ( int ) Rock.Model.SpecialRole.None,
               Guid.NewGuid().ToString() );

            RockMigrationHelper.AddSecurityAuthForAttribute(
                attributeGuid,
                2,
                Rock.Security.Authorization.EDIT,
                false,
                null,
                ( int ) Rock.Model.SpecialRole.AllUsers,
                Guid.NewGuid().ToString() );
        }

        private void AddConflictProfileDefinedTypeAndAttributesDown()
        {
            RockMigrationHelper.DeleteDefinedType( Rock.SystemGuid.DefinedType.ASSESSMENT_CONFLICT_PROFILE );

            RockMigrationHelper.DeleteAttribute( Rock.SystemGuid.Attribute.PERSON_CONFLICT_MODE_WINNING );
            RockMigrationHelper.DeleteAttribute( Rock.SystemGuid.Attribute.PERSON_CONFLICT_MODE_RESOLVING );
            RockMigrationHelper.DeleteAttribute( Rock.SystemGuid.Attribute.PERSON_CONFLICT_MODE_COMPROMISING );
            RockMigrationHelper.DeleteAttribute( Rock.SystemGuid.Attribute.PERSON_CONFLICT_MODE_AVOIDING );
            RockMigrationHelper.DeleteAttribute( Rock.SystemGuid.Attribute.PERSON_CONFLICT_MODE_YIELDING );
            RockMigrationHelper.DeleteAttribute( Rock.SystemGuid.Attribute.PERSON_CONFLICT_THEME_ACCOMMODATING );
            RockMigrationHelper.DeleteAttribute( Rock.SystemGuid.Attribute.PERSON_CONFLICT_THEME_WINNING );
            RockMigrationHelper.DeleteAttribute( Rock.SystemGuid.Attribute.PERSON_CONFLICT_THEME_SOLVING );
        }

        private void ConvertDiscToAssessment()
        {
            int completeStatusValue = Rock.Model.AssessmentRequestStatus.Complete.ConvertToInt();

            Sql( $@"
                -- Convert existing DISC test to an Assessment record
                DECLARE @DiscLastSaveDateAttributeId INT = ( SELECT TOP 1 [Id] FROM [dbo].[Attribute] where [Guid]='{SystemGuid.Attribute.PERSON_DISC_LAST_SAVE_DATE}' )
                DECLARE @DiscAssessmentTypeId INT = ( SELECT TOP 1 [Id] FROM [dbo].[AssessmentType] where [Guid]='{SystemGuid.AssessmentType.DISC}' )

                INSERT INTO Assessment ([PersonAliasId], [AssessmentTypeId], [Status], [CompletedDateTime], [Guid])
                SELECT DISTINCT
                      B.[Id]
                    , @DiscAssessmentTypeId
                    , {completeStatusValue}
                    , [ValueAsDateTime]
                    , NEWID()
                FROM [dbo].[AttributeValue] A
                INNER JOIN [dbo].[PersonAlias] B ON A.[EntityId] = B.[PersonId]
                WHERE A.[AttributeId] = @DiscLastSaveDateAttributeId
	                AND A.[ValueAsDateTime] IS NOT NULL
                    AND	A.[EntityId] NOT IN (
                        SELECT D.[PersonId]
                        FROM [dbo].[Assessment] C
                        INNER JOIN [dbo].[PersonAlias] D ON C.[PersonAliasId] = D.[Id]
                        WHERE C.[AssessmentTypeId] = @DiscAssessmentTypeId)
	                " );
        }

        private void ConvertSpirtualGiftsToAssessment()
        {
            int completeStatusValue = Rock.Model.AssessmentRequestStatus.Complete.ConvertToInt();

            Sql( $@"
                -- Convert existing Spiritual Gifts test to an Assessment record
                DECLARE @GiftLastSaveDateAttributeId INT = ( SELECT TOP 1 [Id] FROM [dbo].[Attribute] where [Guid] = '{SystemGuid.Attribute.PERSON_SPIRITUAL_GIFTS_LAST_SAVE_DATE}' )
                DECLARE @GiftAssessmentTypeId INT = ( SELECT TOP 1 [Id] FROM [dbo].[AssessmentType] where [Guid] = '{SystemGuid.AssessmentType.GIFTS}' )

                INSERT INTO [dbo].[Assessment] ([PersonAliasId], [AssessmentTypeId], [Status], [CompletedDateTime], [Guid])
                SELECT DISTINCT
                      B.[Id]
                    , @GiftAssessmentTypeId
                    , {completeStatusValue}
                    , [ValueAsDateTime]
                    , NEWID()
                FROM [dbo].[AttributeValue] A
                INNER JOIN [dbo].[PersonAlias] B ON A.[EntityId] = B.[PersonId]
                WHERE A.[AttributeId] = @GiftLastSaveDateAttributeId
	                AND A.[ValueAsDateTime] IS NOT NULL
                    AND A.[EntityId] NOT IN (
                        SELECT D.[PersonId]
                        FROM [dbo].[Assessment] C
                        INNER JOIN [dbo].[PersonAlias] D ON C.[PersonAliasId] = D.[Id]
                        WHERE C.[AssessmentTypeId] = @GiftAssessmentTypeId)" );
        }

        private void AddEmotionalIntelligenceDefinedTypeAndAttributes()
        {
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "EQ: Self Aware", "core_EQSelfAware", "", "", 0, "", SystemGuid.Attribute.PERSON_EQ_CONSTRUCTS_SELF_AWARE );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_EQ_CONSTRUCTS_SELF_AWARE );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "EQ: Self Regulate", "core_EQSelfRegulate", "", "", 0, "", SystemGuid.Attribute.PERSON_EQ_CONSTRUCTS_SELF_REGULATE );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_EQ_CONSTRUCTS_SELF_REGULATE );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "EQ: Others Aware", "core_EQOthersAware", "", "", 0, "", SystemGuid.Attribute.PERSON_EQ_CONSTRUCTS_OTHERS_AWARE );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_EQ_CONSTRUCTS_OTHERS_AWARE );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "EQ: Others Regulate", "core_EQOthersRegulate", "", "", 0, "", SystemGuid.Attribute.PERSON_EQ_CONSTRUCTS_OTHERS_REGULATE );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_EQ_CONSTRUCTS_OTHERS_REGULATE );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "EQ: In Problem Solving", "core_EQProblemSolving", "", "", 0, "", SystemGuid.Attribute.PERSON_EQ_SCALES_IN_PROBLEM_SOLVING );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_EQ_SCALES_IN_PROBLEM_SOLVING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "EQ: Under Stress", "core_EQUnderStress", "", "", 0, "", SystemGuid.Attribute.PERSON_EQ_SCALES_UNDER_STRESS );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_EQ_SCALES_UNDER_STRESS );
        }

        private void CreateAssessmentRequestSystemEmail()
        {
		            // Assessment Request
            RockMigrationHelper.UpdateSystemEmail( "System", "Assessment Request", "", "", "", "", "", "Assessments Are Ready To Take", @"{% capture assessmentsLink %}{{ 'Global' | Attribute:'PublicApplicationRoot' }}/Assessments?{{ Person.ImpersonationParameter }}{% endcapture %}

{{ 'Global' | Attribute:'EmailHeader' }}
{{ Person.NickName }},

<p>
    We're each a unique creation. We'd love to learn more about you through a simple and quick online personality profile. Your results will help us tailor our ministry to you and can also be used for building healthier teams and groups.
</p>
<p>
	<div><!--[if mso]>
	  <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ assessmentsLink }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#e76812"" fillcolor=""#ee7624"">
		<w:anchorlock/>
		<center style=""color:#ffffff;font-family:sans-serif;font-size:13px;font-weight:normal;"">Take Assessment</center>
	  </v:roundrect>
	<![endif]--><a href=""{{ assessmentsLink }}""
	style=""background-color:#ee7624;border:1px solid #e76812;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:13px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">Take Assessment</a></div>
</p>
<br />
<br />
{{ 'Global' | Attribute:'EmailFooter' }}", "41FF4269-7B48-40CD-81D4-C11370A13DED");
        }

        private void AssessmentRemindersServiceJobUp()
        {
            // add ServiceJob: Send Assessment Reminders
            // Code Generated using Rock\Dev Tools\Sql\CodeGen_ServiceJobWithAttributes_ForAJob.sql
            Sql(@"IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.SendAssessmentReminders' AND [Guid] = 'E3F48F24-E9FC-4A93-95B5-DE7BEDB95B99' )
            BEGIN
               INSERT INTO [ServiceJob] (
                  [IsSystem]
                  ,[IsActive]
                  ,[Name]
                  ,[Description]
                  ,[Class]
                  ,[CronExpression]
                  ,[NotificationStatus]
                  ,[Guid] )
               VALUES ( 
                  0
                  ,1
                  ,'Send Assessment Reminders'
                  ,'Sends reminders to persons with pending assessments if the created date/time is less than the calculated cut off date and the last reminder date is greater than the calculated reminder date.'
                  ,'Rock.Jobs.SendAssessmentReminders'
                  ,'0 0 8 1/1 * ? *'
                  ,1
                  ,'E3F48F24-E9FC-4A93-95B5-DE7BEDB95B99'
                  );
            END" );

            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "Rock.Jobs.SendAssessmentReminders", "Reminder Every", "The number of days between reminder emails.", 0, @"7", "635122EA-3694-44A2-B7C3-4C4D19F9873C", "ReminderEveryDays" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "Rock.Jobs.SendAssessmentReminders", "Cut off Days", "The number of days past the initial requested date to stop sending reminders. After this cut-off, reminders will need to be sent manually by a person.", 1, @"60", "F4312D3D-26B6-41C7-9842-0CDD319C747C", "CutoffDays" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "Class", "Rock.Jobs.SendAssessmentReminders", "Assessment Reminder System Email", "", 2, @"41FF4269-7B48-40CD-81D4-C11370A13DED", "4BF49C30-ED3B-41AF-AF05-BDE2BA2C0056", "AssessmentSystemEmail" );
            RockMigrationHelper.AddAttributeValue( "635122EA-3694-44A2-B7C3-4C4D19F9873C", 48, @"7", "635122EA-3694-44A2-B7C3-4C4D19F9873C" ); // Send Assessment Reminders: Reminder Every
            RockMigrationHelper.AddAttributeValue( "F4312D3D-26B6-41C7-9842-0CDD319C747C", 48, @"60", "F4312D3D-26B6-41C7-9842-0CDD319C747C" ); // Send Assessment Reminders: Cut off Days
            RockMigrationHelper.AddAttributeValue( "4BF49C30-ED3B-41AF-AF05-BDE2BA2C0056", 48, @"41ff4269-7b48-40cd-81d4-c11370a13ded", "4BF49C30-ED3B-41AF-AF05-BDE2BA2C0056" ); // Send Assessment Reminders: Assessment Reminder System Email
        }

        private void AssessmentRemindersServiceJobDown()
        {
            // Code Generated using Rock\Dev Tools\Sql\CodeGen_ServiceJobWithAttributes_ForAJob.sql
            RockMigrationHelper.DeleteAttribute("635122EA-3694-44A2-B7C3-4C4D19F9873C"); // Rock.Jobs.SendAssessmentReminders: Reminder Every
            RockMigrationHelper.DeleteAttribute("F4312D3D-26B6-41C7-9842-0CDD319C747C"); // Rock.Jobs.SendAssessmentReminders: Cut off Days
            RockMigrationHelper.DeleteAttribute("4BF49C30-ED3B-41AF-AF05-BDE2BA2C0056"); // Rock.Jobs.SendAssessmentReminders: Assessment Reminder System Email

            // remove ServiceJob: Send Assessment Reminders
            Sql(@"IF EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.SendAssessmentReminders' AND [Guid] = 'E3F48F24-E9FC-4A93-95B5-DE7BEDB95B99' )
            BEGIN
               DELETE [ServiceJob]  WHERE [Guid] = 'E3F48F24-E9FC-4A93-95B5-DE7BEDB95B99';
            END" );
        }

        private void CreateRequestAssessmentWorkflow()
        {
            #region FieldTypes

            RockMigrationHelper.UpdateFieldType("Assessment Types","","Rock","Rock.Field.Types.AssessmentTypesFieldType","C263513A-30BE-4823-ABF1-AC12A56F9644");

            #endregion FieldTypes

            #region EntityTypes

            RockMigrationHelper.UpdateEntityType("Rock.Model.Workflow", "3540E9A7-FE30-43A9-8B0A-A372B63DFC93", true, true);
            RockMigrationHelper.UpdateEntityType("Rock.Model.WorkflowActivity", "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F", true, true);
            RockMigrationHelper.UpdateEntityType("Rock.Model.WorkflowActionType", "23E3273A-B137-48A3-9AFF-C8DC832DDCA6", true, true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.CompleteWorkflow","EEDA4318-F014-4A46-9C76-4C052EF81AA1",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.CreateAssessmentRequest","7EDCCA06-C539-4B5B-B6E4-400A19655898",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.RunSQL","A41216D6-6FB0-4019-B222-2C29B4519CF4",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SendEmail","66197B01-D1F0-4924-A315-47AD54E030DE",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SendSystemEmail","4487702A-BEAF-4E5A-92AD-71A1AD48DFCE",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SetAttributeFromEntity","972F19B9-598B-474B-97A4-50E56E7B59D2",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SetAttributeToCurrentPerson","24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.SetAttributeValue","C789E457-0783-44B3-9D8F-2EBAB5F11110",false,true);
            RockMigrationHelper.UpdateEntityType("Rock.Workflow.Action.UserEntryForm","486DC4FA-FCBC-425F-90B0-E606DA8A9F68",false,true);
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","DE9CB292-4785-4EA3-976D-3826F91E9E98"); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A","33E6DF69-BDFA-407A-9744-C175B60643AE","Person Attribute","PersonAttribute","The attribute to set to the currently logged in person.",0,@"","BBED8A83-8BB2-4D35-BAFB-05F67DCAD112"); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Person Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8"); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("4487702A-BEAF-4E5A-92AD-71A1AD48DFCE","08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF","System Email","SystemEmail","A system email to send.",0,@"","C879B8B4-574C-4BCE-BC4D-0C7245AF19D4"); // Rock.Workflow.Action.SendSystemEmail:System Email
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("4487702A-BEAF-4E5A-92AD-71A1AD48DFCE","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","BD6978CE-EDBF-45A9-A548-96630DFF52C1"); // Rock.Workflow.Action.SendSystemEmail:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("4487702A-BEAF-4E5A-92AD-71A1AD48DFCE","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Save Communication History","SaveCommunicationHistory","Should a record of this communication be saved to the recipient's profile",2,@"False","9C5436E6-7EF2-4BD4-B87A-D3E980E55DE3"); // Rock.Workflow.Action.SendSystemEmail:Save Communication History
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("4487702A-BEAF-4E5A-92AD-71A1AD48DFCE","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Send To Email Addresses|Attribute Value","Recipient","The email addresses or an attribute that contains the person or email address that email should be sent to. <span class='tip tip-lava'></span>",1,@"","E58E9280-77CF-4DBB-BF66-87F29D0BF707"); // Rock.Workflow.Action.SendSystemEmail:Send To Email Addresses|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("4487702A-BEAF-4E5A-92AD-71A1AD48DFCE","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","A52C2EBD-D1CC-469F-803C-FF4C5326D456"); // Rock.Workflow.Action.SendSystemEmail:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("486DC4FA-FCBC-425F-90B0-E606DA8A9F68","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","234910F2-A0DB-4D7D-BAF7-83C880EF30AE"); // Rock.Workflow.Action.UserEntryForm:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("486DC4FA-FCBC-425F-90B0-E606DA8A9F68","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","C178113D-7C86-4229-8424-C6D0CF4A7E23"); // Rock.Workflow.Action.UserEntryForm:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Body","Body","The body of the email that should be sent. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",4,@"","4D245B9E-6B03-46E7-8482-A51FBA190E4D"); // Rock.Workflow.Action.SendEmail:Body
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","36197160-7D3D-490D-AB42-7E29105AFE91"); // Rock.Workflow.Action.SendEmail:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Save Communication History","SaveCommunicationHistory","Should a record of this communication be saved to the recipient's profile",8,@"False","65E69B78-37D8-4A88-B8AC-71893D2F75EF"); // Rock.Workflow.Action.SendEmail:Save Communication History
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","33E6DF69-BDFA-407A-9744-C175B60643AE","Attachment One","AttachmentOne","Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.",5,@"","C2C7DA55-3018-4645-B9EE-4BCD11855F2C"); // Rock.Workflow.Action.SendEmail:Attachment One
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","33E6DF69-BDFA-407A-9744-C175B60643AE","Attachment Three","AttachmentThree","Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.",7,@"","A059767A-5592-4926-948A-1065AF4E9748"); // Rock.Workflow.Action.SendEmail:Attachment Three
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","33E6DF69-BDFA-407A-9744-C175B60643AE","Attachment Two","AttachmentTwo","Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.",6,@"","FFD9193A-451F-40E6-9776-74D5DCAC1450"); // Rock.Workflow.Action.SendEmail:Attachment Two
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","33E6DF69-BDFA-407A-9744-C175B60643AE","Send to Group Role","GroupRole","An optional Group Role attribute to limit recipients to if the 'Send to Email Address' is a group or security role.",2,@"","D43C2686-7E02-4A70-8D99-3BCD8ECAFB2F"); // Rock.Workflow.Action.SendEmail:Send to Group Role
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","From Email Address|Attribute Value","From","The email address or an attribute that contains the person or email address that email should be sent from (will default to organization email). <span class='tip tip-lava'></span>",0,@"","9F5F7CEC-F369-4FDF-802A-99074CE7A7FC"); // Rock.Workflow.Action.SendEmail:From Email Address|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Send To Email Addresses|Attribute Value","To","The email addresses or an attribute that contains the person or email address that email should be sent to. <span class='tip tip-lava'></span>",1,@"","0C4C13B8-7076-4872-925A-F950886B5E16"); // Rock.Workflow.Action.SendEmail:Send To Email Addresses|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","9C204CD0-1233-41C5-818A-C5DA439445AA","Subject","Subject","The subject that should be used when sending email. <span class='tip tip-lava'></span>",3,@"","5D9B13B6-CD96-4C7C-86FA-4512B9D28386"); // Rock.Workflow.Action.SendEmail:Subject
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("66197B01-D1F0-4924-A315-47AD54E030DE","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","D1269254-C15A-40BD-B784-ADCC231D3950"); // Rock.Workflow.Action.SendEmail:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("7EDCCA06-C539-4B5B-B6E4-400A19655898","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","D686BDFF-03C8-4F7C-A3FC-89C42DF74714"); // Rock.Workflow.Action.CreateAssessmentRequest:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("7EDCCA06-C539-4B5B-B6E4-400A19655898","33E6DF69-BDFA-407A-9744-C175B60643AE","Assessment Types","AssessmentTypesKey","The attribute that contains the selected list of assessments being requested.",0,@"","B672E4D0-14DE-424A-BC38-7A91F5385A18"); // Rock.Workflow.Action.CreateAssessmentRequest:Assessment Types
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("7EDCCA06-C539-4B5B-B6E4-400A19655898","33E6DF69-BDFA-407A-9744-C175B60643AE","Due Date","DueDate","The attribute that contains the Due Date (if any) for the requests.",2,@"","1010F5DA-565B-4A86-B5C6-E5CE4C26F330"); // Rock.Workflow.Action.CreateAssessmentRequest:Due Date
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("7EDCCA06-C539-4B5B-B6E4-400A19655898","33E6DF69-BDFA-407A-9744-C175B60643AE","Person","Person","The attribute containing the person being requested to take the assessment(s).",1,@"","9E2360BE-4C22-4817-8D2B-5796426D6192"); // Rock.Workflow.Action.CreateAssessmentRequest:Person
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("7EDCCA06-C539-4B5B-B6E4-400A19655898","33E6DF69-BDFA-407A-9744-C175B60643AE","Requested By","RequestedBy","The attribute containing the person requesting the test be taken.",2,@"","5494809A-B0CC-44D9-BFD9-B60D514D020F"); // Rock.Workflow.Action.CreateAssessmentRequest:Requested By
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("7EDCCA06-C539-4B5B-B6E4-400A19655898","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","46FB6CFC-28A3-4822-94BB-B01B8F2D5ED3"); // Rock.Workflow.Action.CreateAssessmentRequest:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("972F19B9-598B-474B-97A4-50E56E7B59D2","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Lava Template","LavaTemplate","By default this action will set the attribute value equal to the guid (or id) of the entity that was passed in for processing. If you include a lava template here, the action will instead set the attribute value to the output of this template. The mergefield to use for the entity is 'Entity.' For example, use {{ Entity.Name }} if the entity has a Name property. <span class='tip tip-lava'></span>",4,@"","00D8331D-3055-4531-B374-6B98A9A71D70"); // Rock.Workflow.Action.SetAttributeFromEntity:Lava Template
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("972F19B9-598B-474B-97A4-50E56E7B59D2","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","9392E3D7-A28B-4CD8-8B03-5E147B102EF1"); // Rock.Workflow.Action.SetAttributeFromEntity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("972F19B9-598B-474B-97A4-50E56E7B59D2","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Entity Is Required","EntityIsRequired","Should an error be returned if the entity is missing or not a valid entity type?",2,@"True","DDEFB300-0A4F-4086-99BE-A32761928F5E"); // Rock.Workflow.Action.SetAttributeFromEntity:Entity Is Required
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("972F19B9-598B-474B-97A4-50E56E7B59D2","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Use Id instead of Guid","UseId","Most entity attribute field types expect the Guid of the entity (which is used by default). Select this option if the entity's Id should be used instead (should be rare).",3,@"False","1246C53A-FD92-4E08-ABDE-9A6C37E70C7B"); // Rock.Workflow.Action.SetAttributeFromEntity:Use Id instead of Guid
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("972F19B9-598B-474B-97A4-50E56E7B59D2","33E6DF69-BDFA-407A-9744-C175B60643AE","Attribute","Attribute","The attribute to set the value of.",1,@"","61E6E1BC-E657-4F00-B2E9-769AAA25B9F7"); // Rock.Workflow.Action.SetAttributeFromEntity:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("972F19B9-598B-474B-97A4-50E56E7B59D2","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","AD4EFAC4-E687-43DF-832F-0DC3856ABABB"); // Rock.Workflow.Action.SetAttributeFromEntity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("A41216D6-6FB0-4019-B222-2C29B4519CF4","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","SQLQuery","SQLQuery","The SQL query to run. <span class='tip tip-lava'></span>",0,@"","F3B9908B-096F-460B-8320-122CF046D1F9"); // Rock.Workflow.Action.RunSQL:SQLQuery
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("A41216D6-6FB0-4019-B222-2C29B4519CF4","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","A18C3143-0586-4565-9F36-E603BC674B4E"); // Rock.Workflow.Action.RunSQL:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("A41216D6-6FB0-4019-B222-2C29B4519CF4","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Continue On Error","ContinueOnError","Should processing continue even if SQL Error occurs?",3,@"False","9A567F6A-2A77-4ECD-80F7-BBD7D54E843C"); // Rock.Workflow.Action.RunSQL:Continue On Error
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("A41216D6-6FB0-4019-B222-2C29B4519CF4","33E6DF69-BDFA-407A-9744-C175B60643AE","Result Attribute","ResultAttribute","An optional attribute to set to the scaler result of SQL query.",2,@"","56997192-2545-4EA1-B5B2-313B04588984"); // Rock.Workflow.Action.RunSQL:Result Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("A41216D6-6FB0-4019-B222-2C29B4519CF4","73B02051-0D38-4AD9-BF81-A2D477DE4F70","Parameters","Parameters","The parameters to supply to the SQL query. <span class='tip tip-lava'></span>",1,@"","EA9A026A-934F-4920-97B1-9734795127ED"); // Rock.Workflow.Action.RunSQL:Parameters
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("A41216D6-6FB0-4019-B222-2C29B4519CF4","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","FA7C685D-8636-41EF-9998-90FFF3998F76"); // Rock.Workflow.Action.RunSQL:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("C789E457-0783-44B3-9D8F-2EBAB5F11110","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","D7EAA859-F500-4521-9523-488B12EAA7D2"); // Rock.Workflow.Action.SetAttributeValue:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("C789E457-0783-44B3-9D8F-2EBAB5F11110","33E6DF69-BDFA-407A-9744-C175B60643AE","Attribute","Attribute","The attribute to set the value of.",0,@"","44A0B977-4730-4519-8FF6-B0A01A95B212"); // Rock.Workflow.Action.SetAttributeValue:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("C789E457-0783-44B3-9D8F-2EBAB5F11110","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Text Value|Attribute Value","Value","The text or attribute to set the value from. <span class='tip tip-lava'></span>",1,@"","E5272B11-A2B8-49DC-860D-8D574E2BC15C"); // Rock.Workflow.Action.SetAttributeValue:Text Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("C789E457-0783-44B3-9D8F-2EBAB5F11110","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","57093B41-50ED-48E5-B72B-8829E62704C8"); // Rock.Workflow.Action.SetAttributeValue:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("EEDA4318-F014-4A46-9C76-4C052EF81AA1","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Active","Active","Should Service be used?",0,@"False","0CA0DDEF-48EF-4ABC-9822-A05E225DE26C"); // Rock.Workflow.Action.CompleteWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("EEDA4318-F014-4A46-9C76-4C052EF81AA1","3B1D93D7-9414-48F9-80E5-6A3FC8F94C20","Status|Status Attribute","Status","The status to set the workflow to when marking the workflow complete. <span class='tip tip-lava'></span>",0,@"Completed","07CB7DBC-236D-4D38-92A4-47EE448BA89A"); // Rock.Workflow.Action.CompleteWorkflow:Status|Status Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute("EEDA4318-F014-4A46-9C76-4C052EF81AA1","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Order","Order","The order that this service should be used (priority)",0,@"","25CAD4BE-5A00-409D-9BAB-E32518D89956"); // Rock.Workflow.Action.CompleteWorkflow:Order

            #endregion

            #region Categories

            RockMigrationHelper.UpdateCategory("C9F3C4A5-1526-474D-803F-D6C7A45CBBAE","Data Integrity","fa fa-magic","","BBAE05FD-8192-4616-A71E-903A927E0D90",0); // Data Integrity

            #endregion

            #region Request Assessment

            RockMigrationHelper.UpdateWorkflowType(false,true,"Request Assessment","","BBAE05FD-8192-4616-A71E-903A927E0D90","Work","icon-fw fa fa-bar-chart",0,true,0,"31DDC001-C91A-4418-B375-CAB1475F7A62",0); // Request Assessment
            RockMigrationHelper.UpdateWorkflowTypeAttribute("31DDC001-C91A-4418-B375-CAB1475F7A62","C263513A-30BE-4823-ABF1-AC12A56F9644","Assessments To Take","AssessmentsToTake","",0,@"","69E8513A-D9E4-4C98-B938-48B1B24F9C08", false); // Request Assessment:Assessments To Take
            RockMigrationHelper.UpdateWorkflowTypeAttribute("31DDC001-C91A-4418-B375-CAB1475F7A62","E4EAB7B2-0B76-429B-AFE4-AD86D7428C70","Person","Person","The person who will take the assessments",1,@"","A201EB57-0AD0-4B98-AD44-9D3A7C0F16BA", false); // Request Assessment:Person
            RockMigrationHelper.UpdateWorkflowTypeAttribute("31DDC001-C91A-4418-B375-CAB1475F7A62","E4EAB7B2-0B76-429B-AFE4-AD86D7428C70","Requested By","RequestedBy","The person who requested the assessments.",2,@"","66B8DEC5-1B55-4AD1-8E4B-C719279A1947", false); // Request Assessment:Requested By
            RockMigrationHelper.UpdateWorkflowTypeAttribute("31DDC001-C91A-4418-B375-CAB1475F7A62","6B6AA175-4758-453F-8D83-FCD8044B5F36","Due Date","DueDate","When all the selected assessments should be completed.",3,@"","7FB54D8C-B6FC-4864-9F14-EEC155CF6D4C", false); // Request Assessment:Due Date
            RockMigrationHelper.UpdateWorkflowTypeAttribute("31DDC001-C91A-4418-B375-CAB1475F7A62","9C204CD0-1233-41C5-818A-C5DA439445AA","No Email Warning","NoEmailWarning","Warning message when the person does not have an email address.",4,@"","B13D6F19-1436-4689-B644-FB70805C255B", false); // Request Assessment:No Email Warning
            RockMigrationHelper.UpdateWorkflowTypeAttribute("31DDC001-C91A-4418-B375-CAB1475F7A62","C28C7BF3-A552-4D77-9408-DEDCF760CED0","Custom Message","CustomMessage","A custom message you would like to include in the request.  Otherwise the default will be used.",5,@"We're each a unique creation. We'd love to learn more about you through a simple and quick online personality profile. Your results will help us tailor our ministry to you and can also be used for building healthier teams and groups.","DBFB3F53-7AE1-4923-A286-3D69B60BA639", false); // Request Assessment:Custom Message
            RockMigrationHelper.AddAttributeQualifier("69E8513A-D9E4-4C98-B938-48B1B24F9C08","includeInactive",@"False","8FD003E0-9FBD-407F-86A8-FC72E5BAE552"); // Request Assessment:Assessments To Take:includeInactive
            RockMigrationHelper.AddAttributeQualifier("69E8513A-D9E4-4C98-B938-48B1B24F9C08","repeatColumns",@"","B0FDE78C-7E2D-42B2-857E-FF96D3AE1C3C"); // Request Assessment:Assessments To Take:repeatColumns
            RockMigrationHelper.AddAttributeQualifier("A201EB57-0AD0-4B98-AD44-9D3A7C0F16BA","EnableSelfSelection",@"False","80BC8D0A-A585-457E-89C9-FB171C464060"); // Request Assessment:Person:EnableSelfSelection
            RockMigrationHelper.AddAttributeQualifier("66B8DEC5-1B55-4AD1-8E4B-C719279A1947","EnableSelfSelection",@"False","0E1769ED-811C-49A2-BE9C-E7A1D6772F3B"); // Request Assessment:Requested By:EnableSelfSelection
            RockMigrationHelper.AddAttributeQualifier("7FB54D8C-B6FC-4864-9F14-EEC155CF6D4C","datePickerControlType",@"Date Picker","D47E8D70-47E1-46B1-B319-0C0BFBF5CD62"); // Request Assessment:Due Date:datePickerControlType
            RockMigrationHelper.AddAttributeQualifier("7FB54D8C-B6FC-4864-9F14-EEC155CF6D4C","displayCurrentOption",@"False","7F4653B4-1155-4BF4-A1B6-1F3B0B6F49A5"); // Request Assessment:Due Date:displayCurrentOption
            RockMigrationHelper.AddAttributeQualifier("7FB54D8C-B6FC-4864-9F14-EEC155CF6D4C","displayDiff",@"False","228A9BD3-1BE5-4873-92D3-AB44591F2414"); // Request Assessment:Due Date:displayDiff
            RockMigrationHelper.AddAttributeQualifier("7FB54D8C-B6FC-4864-9F14-EEC155CF6D4C","format",@"","2ECC60A7-6D8C-4268-BC37-3C6A85E9F182"); // Request Assessment:Due Date:format
            RockMigrationHelper.AddAttributeQualifier("7FB54D8C-B6FC-4864-9F14-EEC155CF6D4C","futureYearCount",@"","0EFFF1E6-D874-4F00-BB90-1E49EA5B0411"); // Request Assessment:Due Date:futureYearCount
            RockMigrationHelper.AddAttributeQualifier("B13D6F19-1436-4689-B644-FB70805C255B","ispassword",@"False","5D5AA32F-C81E-4A26-9C25-ADA8653A9A47"); // Request Assessment:No Email Warning:ispassword
            RockMigrationHelper.AddAttributeQualifier("B13D6F19-1436-4689-B644-FB70805C255B","maxcharacters",@"","836050D6-3053-47EA-9E53-59AE03D967C9"); // Request Assessment:No Email Warning:maxcharacters
            RockMigrationHelper.AddAttributeQualifier("B13D6F19-1436-4689-B644-FB70805C255B","showcountdown",@"False","D6933BFF-E9A9-45EF-B59D-0CFCAEC331E3"); // Request Assessment:No Email Warning:showcountdown
            RockMigrationHelper.AddAttributeQualifier("DBFB3F53-7AE1-4923-A286-3D69B60BA639","allowhtml",@"False","6E365CCA-06EF-4165-925B-3EFB94F5472B"); // Request Assessment:Custom Message:allowhtml
            RockMigrationHelper.AddAttributeQualifier("DBFB3F53-7AE1-4923-A286-3D69B60BA639","maxcharacters",@"","11E73FDC-AD71-4C62-8CB2-F380D3E153BC"); // Request Assessment:Custom Message:maxcharacters
            RockMigrationHelper.AddAttributeQualifier("DBFB3F53-7AE1-4923-A286-3D69B60BA639","numberofrows",@"6","24411F53-15A6-4E70-8BD6-13CC6FB2A7C8"); // Request Assessment:Custom Message:numberofrows
            RockMigrationHelper.AddAttributeQualifier("DBFB3F53-7AE1-4923-A286-3D69B60BA639","showcountdown",@"False","C04C3BCB-B37C-4120-A5F1-188BEBE81C5D"); // Request Assessment:Custom Message:showcountdown
            RockMigrationHelper.UpdateWorkflowActivityType("31DDC001-C91A-4418-B375-CAB1475F7A62",true,"Launch From Person Profile","When this workflow is initiated from the Person Profile page, the \"Entity\" will have a value so the first action will run successfully, and the workflow will then be persisted.",true,0,"41C1D8A6-570C-49D2-A818-08F631FCDBAD"); // Request Assessment:Launch From Person Profile
            RockMigrationHelper.UpdateWorkflowActivityType("31DDC001-C91A-4418-B375-CAB1475F7A62",true,"Save And Send","",false,1,"88373EA3-CF09-4B5C-8582-73F331CD1FB4"); // Request Assessment:Save And Send
            RockMigrationHelper.UpdateWorkflowActionForm(@"{{ Workflow | Attribute:'NoEmailWarning' }}

Assign assessments to {{ Workflow | Attribute: 'Person' }}.",@"","Send^fdc397cd-8b4a-436e-bea1-bce2e6717c03^88373EA3-CF09-4B5C-8582-73F331CD1FB4^Assessment(s) have been sent successfully.|","88C7D1CC-3478-4562-A301-AE7D4D7FFF6D",true,"","A56DA6B0-60A1-4998-B3F0-6BFA6F342167"); // Request Assessment:Launch From Person Profile:User Entry
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("A56DA6B0-60A1-4998-B3F0-6BFA6F342167","69E8513A-D9E4-4C98-B938-48B1B24F9C08",0,true,false,true,false,@"",@"","86E38BD3-21C8-493F-8A5C-FBAEEBB9AEE8"); // Request Assessment:Launch From Person Profile:User Entry:Assessments To Take
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("A56DA6B0-60A1-4998-B3F0-6BFA6F342167","A201EB57-0AD0-4B98-AD44-9D3A7C0F16BA",3,false,true,false,false,@"",@"","8608396B-E634-4E5C-91C7-DECABC22CD56"); // Request Assessment:Launch From Person Profile:User Entry:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("A56DA6B0-60A1-4998-B3F0-6BFA6F342167","66B8DEC5-1B55-4AD1-8E4B-C719279A1947",4,false,true,false,false,@"",@"","961BF92D-6FCB-461F-BA9A-41E7D0CE2205"); // Request Assessment:Launch From Person Profile:User Entry:Requested By
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("A56DA6B0-60A1-4998-B3F0-6BFA6F342167","7FB54D8C-B6FC-4864-9F14-EEC155CF6D4C",2,true,false,false,false,@"",@"","FF867549-A68F-45A7-8495-4AA102FA586B"); // Request Assessment:Launch From Person Profile:User Entry:Due Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("A56DA6B0-60A1-4998-B3F0-6BFA6F342167","B13D6F19-1436-4689-B644-FB70805C255B",5,false,true,false,false,@"",@"","DA087B2B-986B-4721-A712-A013D403F357"); // Request Assessment:Launch From Person Profile:User Entry:No Email Warning
            RockMigrationHelper.UpdateWorkflowActionFormAttribute("A56DA6B0-60A1-4998-B3F0-6BFA6F342167","DBFB3F53-7AE1-4923-A286-3D69B60BA639",1,true,false,false,false,@"",@"","18048430-8A97-40CC-8186-CA0C16A912EC"); // Request Assessment:Launch From Person Profile:User Entry:Custom Message
            RockMigrationHelper.UpdateWorkflowActionType("41C1D8A6-570C-49D2-A818-08F631FCDBAD","Set Person",0,"972F19B9-598B-474B-97A4-50E56E7B59D2",true,false,"","",1,"","B5ED3032-8B0B-4ADC-A2A1-522F8C0086CF"); // Request Assessment:Launch From Person Profile:Set Person
            RockMigrationHelper.UpdateWorkflowActionType("41C1D8A6-570C-49D2-A818-08F631FCDBAD","Set Requested By",1,"24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A",true,false,"","",1,"","6A48CC08-D9AE-4508-A270-FDC7343F461B"); // Request Assessment:Launch From Person Profile:Set Requested By
            RockMigrationHelper.UpdateWorkflowActionType("41C1D8A6-570C-49D2-A818-08F631FCDBAD","Set No Email Warning",2,"A41216D6-6FB0-4019-B222-2C29B4519CF4",true,false,"","",1,"","D947CBE0-437A-4FCE-8898-0141CC03ACEC"); // Request Assessment:Launch From Person Profile:Set No Email Warning
            RockMigrationHelper.UpdateWorkflowActionType("41C1D8A6-570C-49D2-A818-08F631FCDBAD","Set No Email Warning Message",3,"C789E457-0783-44B3-9D8F-2EBAB5F11110",true,false,"","B13D6F19-1436-4689-B644-FB70805C255B",1,"False","215FC2CD-AC85-46C7-9B03-826DDF72923D"); // Request Assessment:Launch From Person Profile:Set No Email Warning Message
            RockMigrationHelper.UpdateWorkflowActionType("41C1D8A6-570C-49D2-A818-08F631FCDBAD","User Entry",4,"486DC4FA-FCBC-425F-90B0-E606DA8A9F68",true,true,"A56DA6B0-60A1-4998-B3F0-6BFA6F342167","",1,"","E9C27804-A31D-4121-A963-9F52BEAE7404"); // Request Assessment:Launch From Person Profile:User Entry
            RockMigrationHelper.UpdateWorkflowActionType("88373EA3-CF09-4B5C-8582-73F331CD1FB4","Save Assessment Requests",0,"7EDCCA06-C539-4B5B-B6E4-400A19655898",true,false,"","",1,"","E871D1D7-04DB-472D-9DF4-6C0389FE2FFC"); // Request Assessment:Save And Send:Save Assessment Requests
            RockMigrationHelper.UpdateWorkflowActionType("88373EA3-CF09-4B5C-8582-73F331CD1FB4","Send Assessment System Email",1,"4487702A-BEAF-4E5A-92AD-71A1AD48DFCE",true,false,"","DBFB3F53-7AE1-4923-A286-3D69B60BA639",32,"","41E75458-C86C-430C-B7C8-4189419D69C6"); // Request Assessment:Save And Send:Send Assessment System Email
            RockMigrationHelper.UpdateWorkflowActionType("88373EA3-CF09-4B5C-8582-73F331CD1FB4","Send Assessment Custom Message Email",2,"66197B01-D1F0-4924-A315-47AD54E030DE",true,false,"","DBFB3F53-7AE1-4923-A286-3D69B60BA639",64,"","C8DE19DB-106A-4AFA-9069-C420B45B976C"); // Request Assessment:Save And Send:Send Assessment Custom Message Email
            RockMigrationHelper.UpdateWorkflowActionType("88373EA3-CF09-4B5C-8582-73F331CD1FB4","Workflow Complete",3,"EEDA4318-F014-4A46-9C76-4C052EF81AA1",true,true,"","",1,"","8F2AC810-83E1-48D2-B306-8F396539343D"); // Request Assessment:Save And Send:Workflow Complete
            RockMigrationHelper.AddActionTypeAttributeValue("B5ED3032-8B0B-4ADC-A2A1-522F8C0086CF","9392E3D7-A28B-4CD8-8B03-5E147B102EF1",@"False"); // Request Assessment:Launch From Person Profile:Set Person:Active
            RockMigrationHelper.AddActionTypeAttributeValue("B5ED3032-8B0B-4ADC-A2A1-522F8C0086CF","61E6E1BC-E657-4F00-B2E9-769AAA25B9F7",@"a201eb57-0ad0-4b98-ad44-9d3a7c0f16ba"); // Request Assessment:Launch From Person Profile:Set Person:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("B5ED3032-8B0B-4ADC-A2A1-522F8C0086CF","DDEFB300-0A4F-4086-99BE-A32761928F5E",@"True"); // Request Assessment:Launch From Person Profile:Set Person:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue("B5ED3032-8B0B-4ADC-A2A1-522F8C0086CF","1246C53A-FD92-4E08-ABDE-9A6C37E70C7B",@"False"); // Request Assessment:Launch From Person Profile:Set Person:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue("6A48CC08-D9AE-4508-A270-FDC7343F461B","DE9CB292-4785-4EA3-976D-3826F91E9E98",@"False"); // Request Assessment:Launch From Person Profile:Set Requested By:Active
            RockMigrationHelper.AddActionTypeAttributeValue("6A48CC08-D9AE-4508-A270-FDC7343F461B","BBED8A83-8BB2-4D35-BAFB-05F67DCAD112",@"66b8dec5-1b55-4ad1-8e4b-c719279a1947"); // Request Assessment:Launch From Person Profile:Set Requested By:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("D947CBE0-437A-4FCE-8898-0141CC03ACEC","F3B9908B-096F-460B-8320-122CF046D1F9",@"DECLARE @PersonAliasGuid uniqueidentifier = '{{ Workflow | Attribute:'Person','RawValue' }}'

SELECT  CASE
    WHEN EXISTS ( SELECT 1
        FROM [Person] P
        INNER JOIN [PersonAlias] PA ON PA.[Guid] = @PersonAliasGuid AND P.[Id] = PA.[PersonId]
        WHERE P.[IsEmailActive] <> 0 AND P.[Email] IS NOT NULL AND P.[Email] != '' )
    THEN ''
    ELSE 'False'
    END"); // Request Assessment:Launch From Person Profile:Set No Email Warning:SQLQuery
            RockMigrationHelper.AddActionTypeAttributeValue("D947CBE0-437A-4FCE-8898-0141CC03ACEC","A18C3143-0586-4565-9F36-E603BC674B4E",@"False"); // Request Assessment:Launch From Person Profile:Set No Email Warning:Active
            RockMigrationHelper.AddActionTypeAttributeValue("D947CBE0-437A-4FCE-8898-0141CC03ACEC","56997192-2545-4EA1-B5B2-313B04588984",@"b13d6f19-1436-4689-b644-fb70805c255b"); // Request Assessment:Launch From Person Profile:Set No Email Warning:Result Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("D947CBE0-437A-4FCE-8898-0141CC03ACEC","9A567F6A-2A77-4ECD-80F7-BBD7D54E843C",@"False"); // Request Assessment:Launch From Person Profile:Set No Email Warning:Continue On Error
            RockMigrationHelper.AddActionTypeAttributeValue("215FC2CD-AC85-46C7-9B03-826DDF72923D","D7EAA859-F500-4521-9523-488B12EAA7D2",@"False"); // Request Assessment:Launch From Person Profile:Set No Email Warning Message:Active
            RockMigrationHelper.AddActionTypeAttributeValue("215FC2CD-AC85-46C7-9B03-826DDF72923D","44A0B977-4730-4519-8FF6-B0A01A95B212",@"b13d6f19-1436-4689-b644-fb70805c255b"); // Request Assessment:Launch From Person Profile:Set No Email Warning Message:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue("215FC2CD-AC85-46C7-9B03-826DDF72923D","E5272B11-A2B8-49DC-860D-8D574E2BC15C",@"<div class=""alert alert-warning margin-t-sm"">{{ Workflow | Attribute:'Person' }} does not have an active email address. Please add an address to their record.</div>"); // Request Assessment:Launch From Person Profile:Set No Email Warning Message:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("E9C27804-A31D-4121-A963-9F52BEAE7404","234910F2-A0DB-4D7D-BAF7-83C880EF30AE",@"False"); // Request Assessment:Launch From Person Profile:User Entry:Active
            RockMigrationHelper.AddActionTypeAttributeValue("E871D1D7-04DB-472D-9DF4-6C0389FE2FFC","D686BDFF-03C8-4F7C-A3FC-89C42DF74714",@"False"); // Request Assessment:Save And Send:Save Assessment Requests:Active
            RockMigrationHelper.AddActionTypeAttributeValue("E871D1D7-04DB-472D-9DF4-6C0389FE2FFC","B672E4D0-14DE-424A-BC38-7A91F5385A18",@"69e8513a-d9e4-4c98-b938-48b1b24f9c08"); // Request Assessment:Save And Send:Save Assessment Requests:Assessment Types
            RockMigrationHelper.AddActionTypeAttributeValue("E871D1D7-04DB-472D-9DF4-6C0389FE2FFC","9E2360BE-4C22-4817-8D2B-5796426D6192",@"a201eb57-0ad0-4b98-ad44-9d3a7c0f16ba"); // Request Assessment:Save And Send:Save Assessment Requests:Person
            RockMigrationHelper.AddActionTypeAttributeValue("E871D1D7-04DB-472D-9DF4-6C0389FE2FFC","5494809A-B0CC-44D9-BFD9-B60D514D020F",@"66b8dec5-1b55-4ad1-8e4b-c719279a1947"); // Request Assessment:Save And Send:Save Assessment Requests:Requested By
            RockMigrationHelper.AddActionTypeAttributeValue("E871D1D7-04DB-472D-9DF4-6C0389FE2FFC","1010F5DA-565B-4A86-B5C6-E5CE4C26F330",@"7fb54d8c-b6fc-4864-9f14-eec155cf6d4c"); // Request Assessment:Save And Send:Save Assessment Requests:Due Date
            RockMigrationHelper.AddActionTypeAttributeValue("41E75458-C86C-430C-B7C8-4189419D69C6","C879B8B4-574C-4BCE-BC4D-0C7245AF19D4",@"41ff4269-7b48-40cd-81d4-c11370a13ded"); // Request Assessment:Save And Send:Send Assessment System Email:System Email
            RockMigrationHelper.AddActionTypeAttributeValue("41E75458-C86C-430C-B7C8-4189419D69C6","BD6978CE-EDBF-45A9-A548-96630DFF52C1",@"False"); // Request Assessment:Save And Send:Send Assessment System Email:Active
            RockMigrationHelper.AddActionTypeAttributeValue("41E75458-C86C-430C-B7C8-4189419D69C6","E58E9280-77CF-4DBB-BF66-87F29D0BF707",@"a201eb57-0ad0-4b98-ad44-9d3a7c0f16ba"); // Request Assessment:Save And Send:Send Assessment System Email:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("41E75458-C86C-430C-B7C8-4189419D69C6","9C5436E6-7EF2-4BD4-B87A-D3E980E55DE3",@"False"); // Request Assessment:Save And Send:Send Assessment System Email:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue("C8DE19DB-106A-4AFA-9069-C420B45B976C","36197160-7D3D-490D-AB42-7E29105AFE91",@"False"); // Request Assessment:Save And Send:Send Assessment Custom Message Email:Active
            RockMigrationHelper.AddActionTypeAttributeValue("C8DE19DB-106A-4AFA-9069-C420B45B976C","0C4C13B8-7076-4872-925A-F950886B5E16",@"a201eb57-0ad0-4b98-ad44-9d3a7c0f16ba"); // Request Assessment:Save And Send:Send Assessment Custom Message Email:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue("C8DE19DB-106A-4AFA-9069-C420B45B976C","5D9B13B6-CD96-4C7C-86FA-4512B9D28386",@"Assessments Are Ready To Take"); // Request Assessment:Save And Send:Send Assessment Custom Message Email:Subject
            RockMigrationHelper.AddActionTypeAttributeValue("C8DE19DB-106A-4AFA-9069-C420B45B976C","4D245B9E-6B03-46E7-8482-A51FBA190E4D",@"{% capture assessmentsLink %}{{ 'Global' | Attribute:'PublicApplicationRoot' }}/assessments?{{ Person.ImpersonationParameter }}{% endcapture %}
{% assign assessmentsCount = Workflow | Attribute:'AssessmentsToTake' | Split:', ' | Size %}
{% assign buttonText = 'Take Assessment' | PluralizeForQuantity:assessmentsCount %}

{{ 'Global' | Attribute:'EmailHeader' }}
{{ Person.NickName }},

<p>{{ Workflow | Attribute:'CustomMessage' | NewlineToBr }}</p>
<p>
	<div><!--[if mso]>
	  <v:roundrect xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:w=""urn:schemas-microsoft-com:office:word"" href=""{{ assessmentsLink }}"" style=""height:38px;v-text-anchor:middle;width:175px;"" arcsize=""11%"" strokecolor=""#e76812"" fillcolor=""#ee7624"">
		<w:anchorlock/>
		<center style=""color:#ffffff;font-family:sans-serif;font-size:13px;font-weight:normal;"">{{ buttonText }}</center>
	  </v:roundrect>
	<![endif]--><a href=""{{ assessmentsLink }}""
	style=""background-color:#ee7624;border:1px solid #e76812;border-radius:4px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:13px;font-weight:normal;line-height:38px;text-align:center;text-decoration:none;width:175px;-webkit-text-size-adjust:none;mso-hide:all;"">{{ buttonText }}</a></div>
</p>
<br />
<br />
{{ 'Global' | Attribute:'EmailFooter' }}"); // Request Assessment:Save And Send:Send Assessment Custom Message Email:Body
            RockMigrationHelper.AddActionTypeAttributeValue("C8DE19DB-106A-4AFA-9069-C420B45B976C","65E69B78-37D8-4A88-B8AC-71893D2F75EF",@"False"); // Request Assessment:Save And Send:Send Assessment Custom Message Email:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue("8F2AC810-83E1-48D2-B306-8F396539343D","0CA0DDEF-48EF-4ABC-9822-A05E225DE26C",@"False"); // Request Assessment:Save And Send:Workflow Complete:Active
            RockMigrationHelper.AddActionTypeAttributeValue("8F2AC810-83E1-48D2-B306-8F396539343D","07CB7DBC-236D-4D38-92A4-47EE448BA89A",@"Completed"); // Request Assessment:Save And Send:Workflow Complete:Status|Status Attribute

            #endregion

            #region Edit bio block list of workflow actions

            // Add Request Assessment
            Sql( @"
                DECLARE @bioWFActionsAttributeValueId INT = 
                (SELECT v.[Id]
                FROM [dbo].[attribute] a
                JOIN [AttributeValue] v ON a.id = v.AttributeId
                WHERE a.[EntityTypeId] = 9
                    AND a.[EntityTypeQualifierColumn] = 'BlockTypeId'
                    AND a.[Key] = 'WorkflowActions'
                    AND a.[EntityTypeQualifierValue] = (SELECT [Id] FROM [dbo].[BlockType] WHERE [Name] = 'person bio')
                    AND v.[Value] NOT LIKE '%31DDC001-C91A-4418-B375-CAB1475F7A62%')

                IF (@bioWFActionsAttributeValueId IS NOT NULL)
                BEGIN
                    UPDATE [dbo].[AttributeValue]
                    SET [Value] = [Value] + ',31DDC001-C91A-4418-B375-CAB1475F7A62'
                    WHERE [Id] = @bioWFActionsAttributeValueId
                END" );

            // Remove legacy DISC Request
            Sql( @"
                DECLARE @bioWFActionsAttributeValueId INT = 
                (SELECT v.[Id]
                FROM [dbo].[attribute] a
                JOIN [AttributeValue] v ON a.id = v.AttributeId
                WHERE a.[EntityTypeId] = 9
                    AND a.[EntityTypeQualifierColumn] = 'BlockTypeId'
                    AND a.[Key] = 'WorkflowActions'
                    AND a.[EntityTypeQualifierValue] = (SELECT [Id] FROM [dbo].[BlockType] WHERE [Name] = 'person bio')
                    AND v.[Value] LIKE '%885CBA61-44EA-4B4A-B6E1-289041B6A195%')

                IF (@bioWFActionsAttributeValueId IS NOT NULL)
                BEGIN
                    UPDATE [dbo].[AttributeValue]
                    SET [Value] = REPLACE([Value], ',885CBA61-44EA-4B4A-B6E1-289041B6A195', '')
                    WHERE [Id] = @bioWFActionsAttributeValueId
                END" );

            #endregion Edit bio block list of workflow actions

            // Set old DISC workflow to inactive.
            Sql( @"
                UPDATE [dbo].[WorkflowType]
                SET [IsActive] = 0
                WHERE [Guid] = '885CBA61-44EA-4B4A-B6E1-289041B6A195'" );
        }

        private void UpdateSpirtualGiftsResultsMessageBlockAttribute()
        {
            RockMigrationHelper.DeleteBlockAttribute( "44272FB2-27DC-452D-8BBB-2F76266FA92E" );// Remove Spiritual Gifts "Min Days To Retake" block attribute.
            
            RockMigrationHelper.UpdateBlockTypeAttribute( "A7E86792-F0ED-46F2-988D-25EBFCD1DC96", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Results Message", "ResultsMessage", "", @"The text (HTML) to display at the top of the results section.<span class='tip tip-lava'></span><span class='tip tip-html'></span>", 0, @"
<div class='row'>
    <div class='col-md-12'>
    <h2 class='h2'> Dominant Gifts</h2>
    </div>
    <div class='col-md-9'>
    <table class='table table-bordered table-responsive'>
    <thead>
        <tr>
            <th>
                Spiritual Gift
            </th>
            <th>
                You are uniquely wired to:
            </th>
        </tr>
    </thead>
    <tbody>
        {% if DominantGifts != empty %}
            {% for dominantGift in DominantGifts %}
                <tr>
                    <td>
                        {{ dominantGift.Value }}
                    </td>
                    <td>
                        {{ dominantGift.Description }}    
                    </td>
                </tr>
            {% endfor %}
        {% else %}
            <tr>
                <td colspan='2'>
                    You did not have any Dominant Gifts
                </td>
            </tr>
        {% endif %}
    </tbody>
    </table>
    </div>
</div>
    
<div class='row'>
    <div class='col-md-12'>
        <h2 class='h2'> Supportive Gifts</h2>
    </div>
    <div class='col-md-9'>
        <table class='table table-bordered table-responsive'>
            <thead>
                <tr>
                   <th>
                    Spiritual Gift
                    </th>
                    <th>
                    You are uniquely wired to:
                    </th>
                </tr>
            </thead>
            <tbody>
                {% if SupportiveGifts != empty %}
                    {% for supportiveGift in SupportiveGifts %}
                        <tr>
                            <td>
                                {{ supportiveGift.Value }}
                            </td>
                            <td>
                                {{ supportiveGift.Description }}
                            </td>
                        </tr>
                    {% endfor %}
                {% else %}
                    <tr>
                        <td colspan='2'>
                            You did not have any Supportive Gifts
                        </td>
                    </tr>
                {% endif %}
            </tbody>
        </table>
    </div>
</div?
<div class='row'>
    <div class='col-md-12'>
        <h2 class='h2'> Other Gifts</h2>
    </div>
    <div class='col-md-9'>
        <table class='table table-bordered table-responsive'>
            <thead>
                <tr>
                   <th>
                    Spiritual Gift
                    </th>
                    <th>
                    You are uniquely wired to:
                    </th>
                </tr>
            </thead>
            <tbody>
                {% if OtherGifts != empty %}
                    {% for otherGift in OtherGifts %}
                        <tr>
                            <td>
                                {{ otherGift.Value }}
                            </td>
                            <td>
                                {{ otherGift.Description }}
                            </td>
                        </tr>
                    {% endfor %}
                {% else %}
                    <tr>
                        <td colspan='2'>
                            You did not have any Other Gifts
                        </td>
                    </tr>
                {% endif %}
           </tbody>
        </table>
    </div>
</div>", "85256610-56EB-4E6F-B62B-A5517B54B39E" );
        }

        private void AssessmentListPageBlockAttributes()
        {
            // Assessments list page, this is where the link in the emails will go.
            RockMigrationHelper.AddPage( true, "EBAA5140-4B8F-44B8-B1E8-C73B654E4B22","5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD","Assessments","","FCF44690-D74C-4FB7-A01B-0EFCA6EA9E1E","fa fa-bar-chart"); // Site:External Website
            RockMigrationHelper.AddSecurityAuthForPage( "FCF44690-D74C-4FB7-A01B-0EFCA6EA9E1E", 1, "View", false, "", 3, "420BADED-31C8-4870-AF12-6E20ABEDB9E7" ); // Page:Assessments
            RockMigrationHelper.AddSecurityAuthForPage( "FCF44690-D74C-4FB7-A01B-0EFCA6EA9E1E", 0, "View", true, "", 2, "E9DD5267-41BD-404D-A064-F1066396E6B9" ); // Page:Assessments
            RockMigrationHelper.AddPageRoute("FCF44690-D74C-4FB7-A01B-0EFCA6EA9E1E","Assessments","F2873F65-617C-4BD3-94E0-48E2408EBDBD");// for Page:Assessments
            RockMigrationHelper.UpdateBlockType("Assessment List","Allows you to view and take any available assessments.","~/Blocks/Crm/AssessmentList.ascx","CRM","0AD1D108-4ABF-4AED-B3B7-4AAEA16D10E4");
            RockMigrationHelper.AddBlock( true, "FCF44690-D74C-4FB7-A01B-0EFCA6EA9E1E".AsGuid(),null,"F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(),"0AD1D108-4ABF-4AED-B3B7-4AAEA16D10E4".AsGuid(), "Assessment List","Main",@"",@"",0,"0E22E6CB-1634-41CA-83EF-4BC7CE52F314"); // Add Block to Page: Assessments Site: External Website
            RockMigrationHelper.AddBlock( true, "C0854F84-2E8B-479C-A3FB-6B47BE89B795".AsGuid(),null,"F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(),"0AD1D108-4ABF-4AED-B3B7-4AAEA16D10E4".AsGuid(), "Assessment List","Sidebar1",@"",@"",2,"37D4A991-9F9A-47CE-9084-04466F166B6A"); // Add Block to Page: My Account Site: External Website
            
            // Attrib for BlockType: Assessment List:Only Show Requested
            RockMigrationHelper.UpdateBlockTypeAttribute("0AD1D108-4ABF-4AED-B3B7-4AAEA16D10E4","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Only Show Requested","OnlyShowRequested","",@"If enabled, limits the list to show only assessments that have been requested or completed.",0,@"True","7A10C446-B0F3-43F0-9FEB-78B689593736");
            // Attrib for BlockType: Assessment List:Hide If No Active Requests
            RockMigrationHelper.UpdateBlockTypeAttribute("0AD1D108-4ABF-4AED-B3B7-4AAEA16D10E4","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Hide If No Active Requests","HideIfNoActiveRequests","",@"If enabled, nothing will be shown if there are not pending (waiting to be taken) assessment requests.",1,@"False","305AD0A5-6E35-402A-A6A2-50474733368A");
            // Attrib for BlockType: Assessment List:Hide If No Requests
            RockMigrationHelper.UpdateBlockTypeAttribute("0AD1D108-4ABF-4AED-B3B7-4AAEA16D10E4","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Hide If No Requests","HideIfNoRequests","",@"If enabled, nothing will be shown where there are no requests (pending or completed).",2,@"False","1E5EE52F-DFD5-4406-A517-4B76E2800D2A");

            #region Attrib for BlockType: Assessment List:Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute("0AD1D108-4ABF-4AED-B3B7-4AAEA16D10E4","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Lava Template","LavaTemplate","",@"The lava template to use to format the entire block.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",3,@"<div class='panel panel-default'>
    <div class='panel-heading'>Assessments</div>
    <div class='panel-body'>
            {% for assessmenttype in AssessmentTypes %}
                {% if assessmenttype.LastRequestObject %}
                    {% if assessmenttype.LastRequestObject.Status == 'Complete' %}
                        <div class='panel panel-success'>
                            <div class='panel-heading'>{{ assessmenttype.Title }}<br />
                                Completed: {{ assessmenttype.LastRequestObject.CompletedDate | Date:'M/d/yyyy'}} <br />
                                <a href='{{ assessmenttype.AssessmentResultsPath}}'>View Results</a>
                            </div>
                        </div>
                    {% elseif assessmenttype.LastRequestObject.Status == 'Pending' %}
                        <div class='panel panel-warning'>
                            <div class='panel-heading'> {{ assessmenttype.Title }}<br />
                                Requested: {{assessmenttype.LastRequestObject.Requester}} ({{ assessmenttype.LastRequestObject.RequestedDate | Date:'M/d/yyyy'}})<br />
                                <a href='{{ assessmenttype.AssessmentPath}}'>Start Assessment</a>
                            </div>
                        </div>
                    {% endif %}
                    {% else %}
                        <div class='panel panel-default'>
                            <div class='panel-heading'> {{ assessmenttype.Title }}<br />
                                Available<br />
                                <a href='{{ assessmenttype.AssessmentPath}}'>Start Assessment</a>
                            </div>
                        </div>
                {% endif %}
            {% endfor %}
    </div>
</div>","044D444A-ECDC-4B7A-8987-91577AAB227C");
            #endregion Attrib for BlockType: Assessment List:Lava Template
            
            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql(@"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '87068AAB-16A7-42CC-8A31-5A957D6C4DD5'");  // Page: My Account,  Zone: Sidebar1,  Block: Actions
            Sql(@"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '8C513CAC-FB3F-40A2-A0F6-D4C50FF72EC8'");  // Page: My Account,  Zone: Sidebar1,  Block: Group List Personalized Lava
            Sql(@"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '37D4A991-9F9A-47CE-9084-04466F166B6A'");  // Page: My Account,  Zone: Sidebar1,  Block: Assessment List
            Sql(@"UPDATE [Block] SET [Order] = 3 WHERE [Guid] = 'E5596525-B176-4753-A337-25F1F9B83FCE'");  // Page: My Account,  Zone: Sidebar1,  Block: Recent Registrations

            // Attrib Value for Block:Assessment List, Attribute:Only Show Requested Page: My Account, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue("37D4A991-9F9A-47CE-9084-04466F166B6A","7A10C446-B0F3-43F0-9FEB-78B689593736",@"False");
        }

        private void ConflictProfileAssessmentPageBlockAttributes()
        {
            RockMigrationHelper.AddLayout( "F3F82256-2D66-432B-9D67-3552CD2F4C2B", "FullWidthNarrow", "Full Width Narrow", "", "BE15B7BC-6D64-4880-991D-FDE962F91196" ); // Site:External Website

            // Conflict Profile Assessment
            RockMigrationHelper.AddPage( true, "FCF44690-D74C-4FB7-A01B-0EFCA6EA9E1E","BE15B7BC-6D64-4880-991D-FDE962F91196","Conflict Profile Assessment","","37F17AD8-8103-4F85-865C-94E76B4470BB",""); // Site:External Website
            RockMigrationHelper.AddPageRoute("37F17AD8-8103-4F85-865C-94E76B4470BB","ConflictProfile","B843AFE4-9198-49DE-904B-8D6440158DAC");// for Page:Conflict Profile Assessment
            RockMigrationHelper.AddPageRoute("37F17AD8-8103-4F85-865C-94E76B4470BB","ConflictProfile/{rckipid}","AFD90575-B363-4862-B4A6-1283D5C00AD9");// for Page:Conflict Profile Assessment
            RockMigrationHelper.UpdateBlockType("Conflict Profile","Allows you to take a conflict profile test and saves your conflict profile score.","~/Blocks/Crm/ConflictProfile.ascx","CRM","91473D2F-607D-4260-9C6A-DD3762FE472D");
            RockMigrationHelper.AddBlock( true, "37F17AD8-8103-4F85-865C-94E76B4470BB".AsGuid(),null,"F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(),"91473D2F-607D-4260-9C6A-DD3762FE472D".AsGuid(), "Conflict Profile","Main",@"",@"",0,"D005E292-25F8-45D4-A713-2A5C811F0219"); // Add Block to Page: Conflict Profile Assessment Site: External Website
            #region Attrib for BlockType: Conflict Profile:Instructions
            RockMigrationHelper.UpdateBlockTypeAttribute("91473D2F-607D-4260-9C6A-DD3762FE472D","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Instructions","Instructions","",@"The text (HTML) to display at the top of the instructions section.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",0,@"
<h2>Welcome to the Conflict Profile Assessment</h2>
<p>
    {{ Person.NickName }}, this assessment was developed and researched by Dr. Gregory A. Wiens and Al Ells and is based on the work and writings of Kenneth Thomas and Ralph Kilmann. When dealing with conflict, one way isn’t always the right way to solve a problem. All five of the modes evaluated in this assessment are appropriate at different times. The challenge is to know which approach is appropriate at what times. It is also important to understand how to use each approach.
</p>
<p>
   Most people feel comfortable using a few approaches and these approaches are often what we saw demonstrated in our culture of origin. This may not have been a healthy method of dealing with conflict.
</p>
<p>
    Before you begin, please take a moment and pray that the Holy Spirit would guide your thoughts,
    calm your mind, and help you respond to each item as honestly as you can. Don't spend much time
    on each item. Your first instinct is probably your best response.
</p>","2E455190-2BAE-4E9F-8505-F393BCE52342");
            #endregion Attrib for BlockType: Conflict Profile:Instructions
            #region Attrib for BlockType: Conflict Profile:Results Message
            RockMigrationHelper.UpdateBlockTypeAttribute("91473D2F-607D-4260-9C6A-DD3762FE472D","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Results Message","ResultsMessage","",@"The text (HTML) to display at the top of the results section.<span class='tip tip-lava'></span><span class='tip tip-html'></span>",1,@"
<p>
   Your scores on this report are how YOU see yourself currently dealing with conflict in the environment chosen. This may or may not be accurate depending on how you are aware of yourself in the midst of conflict. It is most helpful to discuss your scores with someone who understands both you and this assessment.  Remember, in the future, the way you approach conflict should be dictated by the situation, not just how you are used to dealing with conflict. In doing so, everyone benefits, including you.
</p>

<h2>Conflict Engagement Profile</h2>
{[ chart type:'pie' ]}
    [[ dataitem label:'Solving' value:'{{EngagementProfileSolving}}' fillcolor:'#FFCD56' ]] [[ enddataitem ]]
    [[ dataitem label:'Accommodating' value:'{{EngagementProfileAccommodating}}' fillcolor:'#4BC0C0' ]] [[ enddataitem ]]
    [[ dataitem label:'Winning' value:'{{EngagementProfileWinning}}' fillcolor:'#FF3D67' ]] [[ enddataitem ]]
{[ endchart ]}

<h4>Brief Definitions for Conflict Engagement Modes</h4>

<p>
    <b>SOLVING</b> describes those who seek to use both RESOLVING and COMPROMISING modes for solving conflict. By combining these two modes, those who seek to solve problems as a team. Their leadership styles are highly cooperative and empowering for the benefit of the entire group.<br>
    <b>ACCOMMODATING</b> combines AVOIDING and YIELDING modes for solving conflict. Those who are ACCOMMODATING are most effective in roles where allowing others to have their way is better for the team. They are often most effective in support roles or roles where an emphasis on the contribution of others is significant.<br>
    A <b>WINNING</b> engagement profile relates to the WINNING mode for solving conflict. Depending on your role, WINNING engagement is important for times when quick decisions need to be made and essential for sole-proprietors.
</p>

<h2>Your Results Across All Five Modes</h2>
{[ chart type:'bar' ]}
    [[ dataitem label:'Winning' value:'{{Winning}}' fillcolor:'#FF3D67' ]] [[ enddataitem ]]
    [[ dataitem label:'Resolving' value:'{{Resolving}}' fillcolor:'#059BFF' ]] [[ enddataitem ]]
    [[ dataitem label:'Compromising' value:'{{Compromising}}' fillcolor:'#4BC0C0' ]] [[ enddataitem ]]
    [[ dataitem label:'Avoiding' value:'{{Avoiding}}' fillcolor:'#FFCD56' ]] [[ enddataitem ]]
    [[ dataitem label:'Yielding' value:'{{Yielding}}' fillcolor:'#880D37' ]] [[ enddataitem ]]
{[ endchart ]}

<h4>Brief Definitions for Conflict Profile Modes</h4>
<p>
    <b>WINNING</b> is competing and uncooperative. You believe you have the right answer and you must prove you are right whatever it takes. This may be standing up for your own rights, beliefs or position.<br>
    <b>RESOLVING</b> is attempting to work with the other person in depth to find the best solution regardless of where it may lie on the continuum. This involves digging beneath the presenting issue to find a way out that benefits both parties.<br>
    <b>COMPROMISING</b> is finding a middle ground in the conflict. This often involves meeting in the middle or finding some mutually agreeable point between both positions and is useful for quick solutions.<br>
    <b>AVOIDING</b> is not pursuing your own rights or those of the other person. You do not address the conflict. This may be diplomatically sidestepping an issue or avoiding a threatening situation.<br>
    <b>YIELDING</b> is neglecting your own interests and giving in to those of the other person. This is self-sacrifice and may be charity, serving or choosing to obey another when you prefer not to.
</p>
","1A855117-6489-4A15-846A-5A99F54E9747");
            #endregion Attrib for BlockType: Conflict Profile:Results Message
            RockMigrationHelper.UpdateBlockTypeAttribute("91473D2F-607D-4260-9C6A-DD3762FE472D","9C204CD0-1233-41C5-818A-C5DA439445AA","Set Page Title","SetPageTitle","",@"The text to display as the heading.",2,@"Conflict Profile","C5698564-7178-43BA-B4A3-58B13DDC3AF0"); // Attrib for BlockType: Conflict Profile:Set Page Title
            RockMigrationHelper.UpdateBlockTypeAttribute("91473D2F-607D-4260-9C6A-DD3762FE472D","9C204CD0-1233-41C5-818A-C5DA439445AA","Set Page Icon","SetPageIcon","",@"The css class name to use for the heading icon.",3,@"fa fa-gift","D5ABBD1A-61F1-4C48-8AD9-C26AC7F5CAEF"); // Attrib for BlockType: Conflict Profile:Set Page Icon
            RockMigrationHelper.UpdateBlockTypeAttribute("91473D2F-607D-4260-9C6A-DD3762FE472D","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Number of Questions","NumberofQuestions","",@"The number of questions to show per page while taking the test",4,@"7","6CBCA505-E5BA-4FE9-9DD8-7F3C507B12B8"); // Attrib for BlockType: Conflict Profile:Number of Questions
            RockMigrationHelper.UpdateBlockTypeAttribute("91473D2F-607D-4260-9C6A-DD3762FE472D","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Allow Retakes","AllowRetakes","",@"If enabled, the person can retake the test after the minimum days passes.",5,@"True","E3965E46-603C-40E5-AB28-1B53E44561DE"); // Attrib for BlockType: Conflict Profile:Allow Retakes
        }

        private void EmotionalIntelligenceAssessmentPageBlockAttributes()
        {
             
            // Emotional Intelligence Assessment
            RockMigrationHelper.AddPage( true, "FCF44690-D74C-4FB7-A01B-0EFCA6EA9E1E","BE15B7BC-6D64-4880-991D-FDE962F91196","Emotional Intelligence Assessment","","BE5F3984-C25E-47CA-A602-EE1CED99E9AC",""); // Site:External Website
            RockMigrationHelper.AddPageRoute("BE5F3984-C25E-47CA-A602-EE1CED99E9AC","EQ","8C5F1CF8-8AC1-4123-B7FD-E57EA36CFBBF");// for Page:Emotional Intelligence Assessment
            RockMigrationHelper.AddPageRoute("BE5F3984-C25E-47CA-A602-EE1CED99E9AC","EQ/{rckipid}","C97D4D5A-F082-4F2B-A873-71F734B539CC");// for Page:Emotional Intelligence Assessment
            RockMigrationHelper.UpdateBlockType("EQ Assessment","Allows you to take a EQ Inventory test and saves your EQ Inventory score.","~/Blocks/Crm/EQInventory.ascx","CRM","040CFD6D-5155-4BC9-BAEE-A53219A7BECE");
            RockMigrationHelper.AddBlock( true, "BE5F3984-C25E-47CA-A602-EE1CED99E9AC".AsGuid(),null,"F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(),"040CFD6D-5155-4BC9-BAEE-A53219A7BECE".AsGuid(), "EQ Assessment","Main",@"",@"",0,"71BE6A7A-7D51-4149-AFB1-3307DF04B2DF"); // Add Block to Page: Emotional Intelligence Assessment Site: External Website
            #region Attrib for BlockType: EQ Assessment:Instructions
            RockMigrationHelper.UpdateBlockTypeAttribute("040CFD6D-5155-4BC9-BAEE-A53219A7BECE","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Instructions","Instructions","",@"The text (HTML) to display at the top of the instructions section.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>",0,@"
<h2>Welcome to the EQ Inventory Assessment</h2>
<p>
    {{ Person.NickName }}, this assessment was developed and researched by Dr. Gregory A. Wiens.
</p>
<p>
 Our TrueWiring Emotional Intelligence Inventory (EQ-W) assesses your developed skills in two domains:
   <ol>
      <li> understanding your own emotions </li>
      <li> understanding the emotions of others. This instrument identifies your ability to appropriately express your emotions while encouraging others to do the same. </li>
   </ol>
</p>
<p>
    Before you begin, please take a moment and pray that the Holy Spirit would guide your thoughts,
    calm your mind, and help you respond to each item as honestly as you can. Don't spend much time
    on each item. Your first instinct is probably your best response.
</p>","6C00C171-E6DC-4027-B587-0AB63AC939E3");
            #endregion Attrib for BlockType: EQ Assessment:Instructions
            #region Attrib for BlockType: EQ Assessment:Results Message
            RockMigrationHelper.UpdateBlockTypeAttribute("040CFD6D-5155-4BC9-BAEE-A53219A7BECE","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Results Message","ResultsMessage","",@"The text (HTML) to display at the top of the results section.<span class='tip tip-lava'></span><span class='tip tip-html'></span>",1,@"

<h2>EQ Inventory Assessment</h2>

<h3>Self Awareness</h3>
<p>
    Self Awareness is being aware of what emotions you are experiencing and why you
    are experiencing these emotions. This skill is demonstrated in real time. In other
    words, when you are in the midst of a discussion or even a disagreement with someone
    else, ask yourself these questions:
    <ul>
        <li>Are you aware of what emotions you are experiencing?</li>
        <li>Are you aware of why you are experiencing these emotions?</li>
    </ul>

    More than just knowing you are angry, our goal is to understand what has caused the
    anger, such as frustration, hurt, pain, confusion, etc.
</p>

<!-- Graph -->
{[ chart type:'horizontalBar' legendshow:'false' tooltipshow:'false' chartheight:'50' xaxistype:'linear0to100' ]}
    [[ dataitem label:'Self Awareness' value:'{{SelfAwareness}}' fillcolor:'#5c8ae7' ]] [[ enddataitem ]]
{[ endchart ]}
<p class='text-center'><cite>Source: https://healthygrowingleaders.com</cite></p>

<blockquote>
    Your responses to the items on the Self Awareness scale indicate the score for the
    ability to be aware of your own emotions is equal to or better than {{ SelfAwareness }}%
    of those who completed this instrument.
</blockquote>

<h3>Self Regulating</h3>
<p>
    Self Regulating is appropriately expressing your emotions in the context of relationships
    around you. Don’t confuse this with learning to suppress your emotions; rather, think of Self
    Regulating as the ability to express your emotions appropriately. Healthy human beings
    experience a full range of emotions and these are important for family, friends, and
    co-workers to understand. Self Regulating is learning to tell others what you are
    feeling in the moment.
</p>

    {[ chart type:'horizontalBar' legendshow:'false' tooltipshow:'false' chartheight:'50' xaxistype:'linear0to100' ]}
        [[ dataitem label:'Self Regulating' value:'{{SelfRegulating}}' fillcolor:'#175c2d']] [[ enddataitem ]]
    {[ endchart ]}
<p class='text-center'><cite>Source: https://healthygrowingleaders.com</cite></p>

<blockquote>
    Your responses to the items on the Self Regulation scale indicate the score for the
    the ability to appropriately express your own emotions is equal to or better than {{ SelfRegulating }}%
    of those who completed this instrument.
</blockquote>


<h3>Others Awareness</h3>
<p>
    Others Awareness is being aware of what emotions others are experiencing around you and
    why they are experiencing these emotions. As with understanding your own emotions, this
    skill is knowing in real time what another is experiencing. This skill involves reading
    cues to their emotional state through their eyes, facial expressions, body posture, the
    tone of voice or many other ways. It is critical you learn to pay attention to these
    cues for you to enhance your awareness of others' emotions.
</p>

    {[ chart type:'horizontalBar' legendshow:'false' tooltipshow:'false' chartheight:'50' xaxistype:'linear0to100' ]}
        [[ dataitem label:'Others Awareness' value:'{{OthersAwareness}}' fillcolor:'#2e2e5e' ]] [[ enddataitem ]]
    {[ endchart ]}
<p class='text-center'><cite>Source: https://healthygrowingleaders.com</cite></p>

<blockquote>
    Your responses to the items on the Others Awareness scale indicate the score for the
    ability to be aware of others emotions is equal to or better than {{ OthersAwareness }}%
    of those who completed this instrument.
</blockquote>


<h3>Others Regulating</h3>
<p>
    Others Regulating is helping those around you express their emotions appropriately
    in the context of your relationship with them. This skill centers on helping others
    know what emotions they are experiencing and then asking questions or giving permission
    to them to freely and appropriately express their emotions in the context of your relationship.
</p>

    {[ chart type:'horizontalBar' legendshow:'false' tooltipshow:'false' chartheight:'50' xaxistype:'linear0to100' ]}
        [[ dataitem label:'Others Regulating' value:'{{OthersRegulating}}' fillcolor:'#5c5c2d' ]] [[ enddataitem ]]
    {[ endchart ]}
<p class='text-center'><cite>Source: https://healthygrowingleaders.com</cite></p>

<blockquote>
    Your responses to the items on the Others Regulation scale indicate the score for
    the ability to enable others to appropriately express their emotions in the context
    of your relationship is equal to or better than {{OthersRegulating}}% of those who
    completed this instrument.
</blockquote>


<h2>Additional Scales</h2>
<p>
    The EQ*i includes two additional scales which are particularly useful for those in
    leadership roles: 1) EQ in Problem Solving and 2) EQ under stress. Frequently we
    find it difficult to appreciate that conflicting emotions exacerbate most problems
    we experience in life. The solution, therefore, must account for these emotions, not
    just logic or doing the “right” thing.
</p>

<h3>EQ in Problem Solving</h3>
<p>
    The EQ in Problem Solving identifies how proficient you are in using emotions to
    solve problems. This skill requires first being aware of what emotions are involved
    in the problem and what is the source of those emotions. It also includes helping
    others (and yourself) express those emotions within the discussion.
</p>

    {[ chart type:'horizontalBar' legendshow:'false' tooltipshow:'false' chartheight:'50' xaxistype:'linear0to100' ]}
        [[ dataitem label:'EQ in Problem Solving' value:'{{EQinProblemSolving}}' fillcolor:'#5b2d09' ]] [[ enddataitem ]]
    {[ endchart ]}
<p class='text-center'><cite>Source: https://healthygrowingleaders.com</cite></p>

<blockquote>
    Your responses to the items on the EQ in Problem Solving scale indicate the score for
    the ability to use emotions in resolving problems is equal to or better than {{ EQinProblemSolving }}%
    of those who completed this instrument.
</blockquote>


<h3>EQ Under Stress</h3>
<p>
    It is more difficult to maintain high EQ under high stress than at any other time,
    so EQ Under Stress identifies how capable you are to keep high EQ under high-stress
    moments. This skill requires highly developed Self and Others awareness to understand
    how the stress is impacting yourself and others. It also involves being able to
    articulate the appropriate emotions under pressure which may be different from
    articulating them when not under stress.
</p>

    {[ chart type:'horizontalBar' legendshow:'false' tooltipshow:'false' chartheight:'50' xaxistype:'linear0to100' ]}
        [[ dataitem label:'EQ Under Stress' value:'{{EQUnderStress}}' fillcolor:'#8a5c2d' ]] [[ enddataitem ]]
    {[ endchart ]}
<p class='text-center'><cite>Source: https://healthygrowingleaders.com</cite></p>

<blockquote>
    Your responses to the items on the EQ in Under Stress scale indicate the score
    for the ability to maintain EQ under significant stress is equal to or better than {{ EQUnderStress }}%
    of those who completed this instrument.
</blockquote>
","5B6219CE-84B5-4F68-BE5B-C3187EDFF2A6");
            #endregion Attrib for BlockType: EQ Assessment:Results Message
            RockMigrationHelper.UpdateBlockTypeAttribute("040CFD6D-5155-4BC9-BAEE-A53219A7BECE","9C204CD0-1233-41C5-818A-C5DA439445AA","Set Page Title","SetPageTitle","",@"The text to display as the heading.",2,@"EQ Inventory Assessment","E99F01A7-AF8F-4010-A456-3A9048347859"); // Attrib for BlockType: EQ Assessment:Set Page Title
            RockMigrationHelper.UpdateBlockTypeAttribute("040CFD6D-5155-4BC9-BAEE-A53219A7BECE","9C204CD0-1233-41C5-818A-C5DA439445AA","Set Page Icon","SetPageIcon","",@"The css class name to use for the heading icon.",3,@"fa fa-gift","D5CF91C1-2CC8-46BF-8CC6-DD6AD8B07518"); // Attrib for BlockType: EQ Assessment:Set Page Icon
            RockMigrationHelper.UpdateBlockTypeAttribute("040CFD6D-5155-4BC9-BAEE-A53219A7BECE","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Number of Questions","NumberofQuestions","",@"The number of questions to show per page while taking the test",4,@"7","8D2C5502-0AAB-4FE6-ABE9-05900439827D"); // Attrib for BlockType: EQ Assessment:Number of Questions
            RockMigrationHelper.UpdateBlockTypeAttribute("040CFD6D-5155-4BC9-BAEE-A53219A7BECE","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Allow Retakes","AllowRetakes","",@"If enabled, the person can retake the test after the minimum days passes.",5,@"True","A0905767-79C9-4567-BA76-A3FFEE71E0B3"); // Attrib for BlockType: EQ Assessment:Allow Retakes
        }

        private void PagesBlocksAndAttributesUp()
        {
            RockMigrationHelper.DeleteBlockAttribute( "3162c5cd-1244-4cb9-9099-bc484ce090d3" );// Remove DISC test "Min Days To Retake" block attribute.

            RockMigrationHelper.AddLayout( "F3F82256-2D66-432B-9D67-3552CD2F4C2B", "FullWidthNarrow", "Full Width Narrow", "", "BE15B7BC-6D64-4880-991D-FDE962F91196" ); // Site:External Website

            AssessmentListPageBlockAttributes();
            ConflictProfileAssessmentPageBlockAttributes();
            EmotionalIntelligenceAssessmentPageBlockAttributes();

            // Use Assessment List page as the parent page for existing assessments
            RockMigrationHelper.MovePage( "C8CEF4B0-4A09-46D2-9B6B-CD2B6D3078B1", "FCF44690-D74C-4FB7-A01B-0EFCA6EA9E1E" );// Move DISC page to new parent Assessment Tests
            RockMigrationHelper.MovePage( "06410598-3DA4-4710-A047-A518157753AB", "FCF44690-D74C-4FB7-A01B-0EFCA6EA9E1E" );// Move gifts page to new parent Assessment Tests

            RockMigrationHelper.UpdateFieldType("Assessment Types","","Rock","Rock.Field.Types.AssessmentTypesFieldType","C263513A-30BE-4823-ABF1-AC12A56F9644");
        }

        private void PagesBlocksAndAttributesDown()
        {
            // Attrib for BlockType: EQ Assessment:Instructions
            RockMigrationHelper.DeleteAttribute("6C00C171-E6DC-4027-B587-0AB63AC939E3");
            // Attrib for BlockType: EQ Assessment:Allow Retakes
            RockMigrationHelper.DeleteAttribute("A0905767-79C9-4567-BA76-A3FFEE71E0B3");
            // Attrib for BlockType: EQ Assessment:Set Page Icon
            RockMigrationHelper.DeleteAttribute("D5CF91C1-2CC8-46BF-8CC6-DD6AD8B07518");
            // Attrib for BlockType: EQ Assessment:Set Page Title
            RockMigrationHelper.DeleteAttribute("E99F01A7-AF8F-4010-A456-3A9048347859");
            // Attrib for BlockType: EQ Assessment:Results Message
            RockMigrationHelper.DeleteAttribute("5B6219CE-84B5-4F68-BE5B-C3187EDFF2A6");
            // Attrib for BlockType: EQ Assessment:Number of Questions
            RockMigrationHelper.DeleteAttribute("8D2C5502-0AAB-4FE6-ABE9-05900439827D");
            // Attrib for BlockType: Conflict Profile:Allow Retakes
            RockMigrationHelper.DeleteAttribute("E3965E46-603C-40E5-AB28-1B53E44561DE");
            // Attrib for BlockType: Conflict Profile:Number of Questions
            RockMigrationHelper.DeleteAttribute("6CBCA505-E5BA-4FE9-9DD8-7F3C507B12B8");
            // Attrib for BlockType: Conflict Profile:Results Message
            RockMigrationHelper.DeleteAttribute("1A855117-6489-4A15-846A-5A99F54E9747");
            // Attrib for BlockType: Conflict Profile:Set Page Icon
            RockMigrationHelper.DeleteAttribute("D5ABBD1A-61F1-4C48-8AD9-C26AC7F5CAEF");
            // Attrib for BlockType: Conflict Profile:Instructions
            RockMigrationHelper.DeleteAttribute("2E455190-2BAE-4E9F-8505-F393BCE52342");
            // Attrib for BlockType: Conflict Profile:Set Page Title
            RockMigrationHelper.DeleteAttribute("C5698564-7178-43BA-B4A3-58B13DDC3AF0");
            // Attrib for BlockType: Assessment List:Hide If No Active Requests
            RockMigrationHelper.DeleteAttribute("305AD0A5-6E35-402A-A6A2-50474733368A");
            // Attrib for BlockType: Assessment List:Lava Template
            RockMigrationHelper.DeleteAttribute("044D444A-ECDC-4B7A-8987-91577AAB227C");
            // Attrib for BlockType: Assessment List:Hide If No Requests
            RockMigrationHelper.DeleteAttribute("1E5EE52F-DFD5-4406-A517-4B76E2800D2A");
            // Attrib for BlockType: Assessment List:Only Show Requested
            RockMigrationHelper.DeleteAttribute("7A10C446-B0F3-43F0-9FEB-78B689593736");
            // Remove Block: Conflict Profile, from Page: Conflict Profile Assessment, Site: External Website
            RockMigrationHelper.DeleteBlock("D005E292-25F8-45D4-A713-2A5C811F0219");
            // Remove Block: EQ Assessment, from Page: Emotional Intelligence Assessment, Site: External Website
            RockMigrationHelper.DeleteBlock("71BE6A7A-7D51-4149-AFB1-3307DF04B2DF");
            // Remove Block: Assessment List, from Page: My Account, Site: External Website
            RockMigrationHelper.DeleteBlock("37D4A991-9F9A-47CE-9084-04466F166B6A");
            // Remove Block: Assessment List, from Page: Assessments, Site: External Website
            RockMigrationHelper.DeleteBlock("0E22E6CB-1634-41CA-83EF-4BC7CE52F314");
            RockMigrationHelper.DeleteBlockType("040CFD6D-5155-4BC9-BAEE-A53219A7BECE"); // EQ Assessment
            RockMigrationHelper.DeleteBlockType("91473D2F-607D-4260-9C6A-DD3762FE472D"); // Conflict Profile
            RockMigrationHelper.DeleteBlockType("0AD1D108-4ABF-4AED-B3B7-4AAEA16D10E4"); // Assessment List

            // Use Assessment List page as the parent page for existing assessments
            RockMigrationHelper.MovePage( "C8CEF4B0-4A09-46D2-9B6B-CD2B6D3078B1", "EBAA5140-4B8F-44B8-B1E8-C73B654E4B22" );// Move DISC page back to Support Pages
            RockMigrationHelper.MovePage( "06410598-3DA4-4710-A047-A518157753AB", "EBAA5140-4B8F-44B8-B1E8-C73B654E4B22" );// Move gifts page back to Support Pages

            // Remove the security for the assessments page.
            RockMigrationHelper.DeleteSecurityAuthForPage( "FCF44690-D74C-4FB7-A01B-0EFCA6EA9E1E" );
           
            RockMigrationHelper.DeletePage("F44A6424-8B9C-4B44-91A8-4BB6F683D4B6"); //  Page: Motivators Assessment, Layout: FullWidth, Site: External Website
            RockMigrationHelper.DeletePage("BE5F3984-C25E-47CA-A602-EE1CED99E9AC"); //  Page: Emotional Intelligence Assessment, Layout: FullWidth, Site: External Website
            RockMigrationHelper.DeletePage("37F17AD8-8103-4F85-865C-94E76B4470BB"); //  Page: Conflict Profile Assessment, Layout: FullWidth, Site: External Website
            RockMigrationHelper.DeletePage("FCF44690-D74C-4FB7-A01B-0EFCA6EA9E1E"); //  Page: Assessments, Layout: FullWidth, Site: External Website
        }
    
        private void AddMotivatorsAttributes()
        {
            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator: Believing", "core_MotivatorBelieving", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_BELIVING );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_BELIVING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator: Caring", "core_MotivatorCaring", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_CARING );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_CARING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator: Communicating", "core_MotivatorCommunicating", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_EXPRESSING );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_EXPRESSING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator: Empowering", "core_MotivatorEmpowering", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_EMPOWERING );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_EMPOWERING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator: Flexing", "core_MotivatorFlexing", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_ADAPTING );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_ADAPTING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator: Gathering", "core_MotivatorGathering", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_GATHERING );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_GATHERING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator: Innovating", "core_MotivatorInnovating", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_INNOVATING );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_INNOVATING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator: Leading", "core_MotivatorLeading", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_LEADING );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_LEADING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator: Learning", "core_MotivatorLearning", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_LEARNING );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_LEARNING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator: Maximizing", "core_MotivatorMaximizing", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_MAXIMIZING );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_MAXIMIZING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator: Organizing", "core_MotivatorOrganizing", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_ORGANIZING );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_ORGANIZING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator: Pacing", "core_MotivatorPacing", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_PACING );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_PACING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator: Perceiving", "core_MotivatorPerceiving", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_PERCEIVING );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_PERCEIVING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator: Relating", "core_MotivatorRelating", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_RELATING );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_RELATING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator: Serving", "core_MotivatorServing", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_SERVING );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_SERVING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator: Thinking", "core_MotivatorThinking", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_THINKING );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_THINKING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator: Transforming", "core_MotivatorTransforming", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_TRANSFORMING );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_TRANSFORMING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator: Uniting", "core_MotivatorUniting", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_UNITING );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_UNITING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator: Unwavering", "core_MotivatorUnwavering", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_PERSEVERING );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_PERSEVERING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator: Venturing", "core_MotivatorVenturing", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_RISKING );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_RISKING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator: Visioning", "core_MotivatorVisioning", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_VISIONING );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_VISIONING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator: Growth Propensity", "core_MotivatorGrowthPropensity", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_GROWTHPROPENSITY );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_GROWTHPROPENSITY );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Cluster: Influential", "core_MotivatorClusterInfluential", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_CLUSTER_INFLUENTIAL );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_CLUSTER_INFLUENTIAL );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Cluster: Organizational", "core_MotivatorClusterOrganizational", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_CLUSTER_ORGANIZATIONAL );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_CLUSTER_ORGANIZATIONAL );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Cluster: Intellectual", "core_MotivatorClusterIntellectual", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_CLUSTER_INTELLECTUAL );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_CLUSTER_INTELLECTUAL );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Cluster: Operational", "core_MotivatorClusterOperational", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_CLUSTER_OPERATIONAL );
            AddSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_CLUSTER_OPERATIONAL );
        }

        private void AddMotivatorsDefinedTypes()
        {
            //Add Motivator Cluster Defined Type
            AddMotivatorClusterDefinedType();

            //Add Motivator Defined Type
            AddMotivatorDefinedType();

            Sql( $@" DECLARE @DefinedTypeEntityTypeId int = (
                    SELECT TOP 1 [Id]
                    FROM [EntityType]
                    WHERE [Name] = 'Rock.Model.DefinedType' )
                    
                    DECLARE @CategoryId int = (
                    SELECT TOP 1 [Id] FROM [Category]
                    WHERE [EntityTypeId] = @DefinedTypeEntityTypeId
                    AND [Name] = 'TrueWiring' )

                    UPDATE [DefinedType]
                    SET [CategoryId] = @CategoryId
                    WHERE [Guid] IN ( '{SystemGuid.DefinedType.ASSESSMENT_CONFLICT_PROFILE}'
                        ,'{SystemGuid.DefinedType.DISC_RESULTS_TYPE}'
                        ,'{SystemGuid.DefinedType.SPIRITUAL_GIFTS}' )" );
        }

        private void AddMotivatorsAssessmenPage()
        {
            RockMigrationHelper.AddPage( true, "FCF44690-D74C-4FB7-A01B-0EFCA6EA9E1E", "BE15B7BC-6D64-4880-991D-FDE962F91196", "Motivators Assessment", "", "0E6AECD6-675F-4908-9FA3-C7E46040527C" ); // Site:External Website
            RockMigrationHelper.UpdateBlockType( "Motivators Assessment", "Allows you to take a Motivators Assessment test and saves your results.", "~/Blocks/Crm/Motivators.ascx", "CRM", "18CF8DA8-5DE0-49EC-A279-D5507CFA5713" );
            
            // Add Block to Page: Motivator Assessment Site: External Website
            RockMigrationHelper.AddBlock( true, "0E6AECD6-675F-4908-9FA3-C7E46040527C".AsGuid(), null, "F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(), "18CF8DA8-5DE0-49EC-A279-D5507CFA5713".AsGuid(), "Motivators Assessment", "Main", @"", @"", 0, "92C58130-9CE7-44E0-8F22-DF358A0F69C2" );

            #region Attrib for BlockType: Motivators Assessment:Instructions
            RockMigrationHelper.UpdateBlockTypeAttribute( "18CF8DA8-5DE0-49EC-A279-D5507CFA5713", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Instructions", "Instructions", "", @"The text (HTML) to display at the top of the instructions section.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 0, @"<h2>Welcome to the Motivators Assessment</h2>
<p>
    {{ Person.NickName }}, this assessment was developed and researched by Dr. Gregory A. Wiens and is intended to help identify the things that you value. These motivators influence your personal, professional, social and every other part of your life because they influence what you view as important and what should or should not be paid attention to. They impact the way you lead or even if you lead. They directly sway how you view your current situation.
</p>
<p>
   We all have internal mechanisms that cause us to view life very differently from others. Some of this could be attributed to our personality. However, a great deal of research has been done to identify different values, motivators or internal drivers which cause each of us to have a different perspective on people, places, and events. These values cause you to construe one situation very differently from another who value things differently.
</p>
<p>
    Before you begin, please take a moment and pray that the Holy Spirit would guide your thoughts, calm your mind, and help you respond to each item as honestly as you can. Don't spend much time on each item. Your first instinct is probably your best response.
</p>" , "973511D4-7C77-42E0-8FDC-23AE5DF61177" );
            #endregion Attrib for BlockType: Motivators Assessment:Instructions
            #region Attrib for BlockType: Motivators Assessment:Results Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "18CF8DA8-5DE0-49EC-A279-D5507CFA5713", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Results Message", "ResultsMessage", "", @"The text (HTML) to display at the top of the results section.<span class='tip tip-lava'></span><span class='tip tip-html'></span>", 1, @"<p>
   This assessment identifies 22 different motivators (scales) which illustrate different things to which we all assign importance. These motivators listed in descending order on the report from the highest to the lowest. No one motivator is better than another. They are all critical and essential for the health of an organization. There are over 1,124,000,727,777,607,680,000 different combinations of these 22 motivators so we would hope you realize that your exceptional combination is clearly unique. We believe it is as important for you to know the motivators which are at the top as well as the ones at the bottom of your list. This is because you would best be advised to seek roles and responsibilities where your top motivators are needed. On the other hand, it would be advisable to <i>avoid roles or responsibilities where your bottom motivators would be required</i>. 
</p>

<h2>Influential, Organizational, Intellectual, and Operational</h2>
<p>
Each of the 22 motivators are grouped into one of four clusters: Influential, Organizational, Intellectual, and Operational. The clusters, graphed below, include the motivators that fall within each grouping.
</p>
<!--  Cluster Chart -->
    <div class=""panel panel-default"">
      <div class=""panel-heading"">
        <h2 class=""panel-title""><b>Composite Score</b></h2>
      </div>
      <div class=""panel-body"">
    {[chart type:'horizontalBar' chartheight:'200px' ]}
    {% for motivatorClusterScore in MotivatorClusterScores %}
        [[dataitem label:'{{ motivatorClusterScore.DefinedValue.Value }}' value:'{{ motivatorClusterScore.Value }}' fillcolor:'{{ motivatorClusterScore.DefinedValue | Attribute:'Color' }}' ]] 
        [[enddataitem]]
    {% endfor %}
    {[endchart]}
    
        Source: <a href=""https://healthygrowingleaders.com"">https://healthygrowingleaders.com</a>
      </div>
    </div>
<p>
This graph is based on the average composite score for each cluster of Motivators.
</p>
{% for motivatorClusterScore in MotivatorClusterScores %}
<p>
<b>{{ motivatorClusterScore.DefinedValue.Value }}</b>
</br>
{{ motivatorClusterScore.DefinedValue.Description }}
</br>
{{ motivatorClusterScore.DefinedValue | Attribute:'Summary' }}
</p>

 {% endfor %}
<p>
   The following graph shows your motivators ranked from top to bottom.
</p>

  <div class=""panel panel-default"">
    <div class=""panel-heading"">
      <h2 class=""panel-title""><b>Ranked Motivators</b></h2>
    </div>
    <div class=""panel-body"">

      {[ chart type:'horizontalBar' ]}
        {% for motivatorScore in MotivatorScores %}
        {% assign cluster = motivatorScore.DefinedValue | Attribute:'Cluster' %}
            {% if cluster and cluster != empty %}
                [[dataitem label:'{{ motivatorScore.DefinedValue.Value }}' value:'{{ motivatorScore.Value }}' fillcolor:'{{ motivatorScore.DefinedValue | Attribute:'Color' }}' ]] 
                [[enddataitem]]
            {% endif %}
        {% endfor %}
        {[endchart]}
    </div>
  </div>
<p>
    Your motivators will no doubt shift and morph throughout your life.For instance, #4 may drop to #7 and vice versa.  However, it is very doubtful that #22 would ever become #1. For that reason, read through all of the motivators and appreciate the ones that you have. Seek input from those who know you to see if they agree or disagree with these results.
</p>", "BA51DFCD-B174-463F-AE3F-6EEE73DD9338" );
            #endregion Attrib for BlockType: Motivators Assessment:Results Message
            // Attrib for BlockType: Motivators Assessment:Set Page Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "18CF8DA8-5DE0-49EC-A279-D5507CFA5713", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Set Page Title", "SetPageTitle", "", @"The text to display as the heading.", 2, @"EQ Inventory Assessment", "4CE9D93E-2002-425A-A8FD-679CCEE991D7" );
            // Attrib for BlockType: Motivators Assessment:Set Page Icon
            RockMigrationHelper.UpdateBlockTypeAttribute( "18CF8DA8-5DE0-49EC-A279-D5507CFA5713", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Set Page Icon", "SetPageIcon", "", @"The css class name to use for the heading icon.", 3, @"", "7471495D-4C68-45EA-874D-6778608E81B2" );
            // Attrib for BlockType: Motivators Assessment:Number of Questions
            RockMigrationHelper.UpdateBlockTypeAttribute( "18CF8DA8-5DE0-49EC-A279-D5507CFA5713", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Questions", "NumberofQuestions", "", @"The number of questions to show per page while taking the test", 4, @"7", "02489F19-384F-45BE-BBC4-D2ECC63D0992" );
            // Attrib for BlockType: Motivators Assessment:Allow Retakes
            RockMigrationHelper.UpdateBlockTypeAttribute( "18CF8DA8-5DE0-49EC-A279-D5507CFA5713", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Retakes", "AllowRetakes", "", @"If enabled, the person can retake the test after the minimum days passes.", 5, @"True", "3A07B385-A3C1-4C0B-80F9-F50432503C0A" );

            RockMigrationHelper.AddPageRoute("0E6AECD6-675F-4908-9FA3-C7E46040527C","Motivators","7D00FD4E-9E6C-42B1-BB25-7F417DF25CA4");// for Page:Motivators Assessment
            RockMigrationHelper.AddPageRoute("0E6AECD6-675F-4908-9FA3-C7E46040527C","Motivators/{rckipid}","9299B437-38C6-421F-B705-B0F2BCEC2CD0");// for Page:Motivators Assessment
        }

        private void AddMotivatorClusterDefinedType()
        {
            RockMigrationHelper.AddDefinedType( "TrueWiring", "Motivator Cluster", "Used by Rock's TrueWiring Motivator assessment to hold the four cluster groupings.", "354715FA-564A-420A-8324-0411988AE7AB", @"" );
            RockMigrationHelper.AddDefinedTypeAttribute( "354715FA-564A-420A-8324-0411988AE7AB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Attribute Score Key", "AttributeScoreKey", "", 1026, "", "CE3F126E-B56A-438A-BA45-8EC8437BB961" );
            RockMigrationHelper.AddDefinedTypeAttribute( "354715FA-564A-420A-8324-0411988AE7AB", "D747E6AE-C383-4E22-8846-71518E3DD06F", "Color", "Color", "", 1028, "", "8B5F72E4-5A49-4224-9437-82B1F23D8896" );
            RockMigrationHelper.AddDefinedTypeAttribute( "354715FA-564A-420A-8324-0411988AE7AB", "DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", "Summary", "Summary", "", 1027, "", "07E85FA1-8F86-4414-8DC3-43D303C55457" );
            RockMigrationHelper.AddAttributeQualifier( "07E85FA1-8F86-4414-8DC3-43D303C55457", "documentfolderroot", "", "A6D6A112-01E9-4675-8D4E-2214219B1B59" );
            RockMigrationHelper.AddAttributeQualifier( "07E85FA1-8F86-4414-8DC3-43D303C55457", "imagefolderroot", "", "94BA3FFE-3DD9-4827-9779-54DE393467BE" );
            RockMigrationHelper.AddAttributeQualifier( "07E85FA1-8F86-4414-8DC3-43D303C55457", "toolbar", "Light", "AB148217-518F-419C-93BE-7CB0C49B9511" );
            RockMigrationHelper.AddAttributeQualifier( "07E85FA1-8F86-4414-8DC3-43D303C55457", "userspecificroot", "False", "B97AC79C-E703-45E7-B802-13BE25413576" );
            RockMigrationHelper.AddAttributeQualifier( "8B5F72E4-5A49-4224-9437-82B1F23D8896", "selectiontype", "Color Picker", "96D1A8DA-7E63-41FE-8B0A-0E6239828DBA" );
            RockMigrationHelper.AddAttributeQualifier( "CE3F126E-B56A-438A-BA45-8EC8437BB961", "ispassword", "False", "3F4357E7-4644-4CBD-93A8-A98DD1879814" );
            RockMigrationHelper.AddAttributeQualifier( "CE3F126E-B56A-438A-BA45-8EC8437BB961", "maxcharacters", "", "A7F4A5FA-43CF-4B86-B5B0-BE33E98B1C20" );
            RockMigrationHelper.AddAttributeQualifier( "CE3F126E-B56A-438A-BA45-8EC8437BB961", "showcountdown", "False", "20381B01-3B39-4008-83AC-048FF796DF75" );
            RockMigrationHelper.UpdateDefinedValue( "354715FA-564A-420A-8324-0411988AE7AB", "Influential", "How you relate to people.", "840C414E-A261-4243-8302-6117E8949FE4", false );
            RockMigrationHelper.UpdateDefinedValue( "354715FA-564A-420A-8324-0411988AE7AB", "Intellectual", "How your mind operates.", "58FEF15F-561D-420E-8937-6CF51D296F0E", false );
            RockMigrationHelper.UpdateDefinedValue( "354715FA-564A-420A-8324-0411988AE7AB", "Operational", "How you relate to structure.", "84322020-4E27-44EF-88F2-EAFDB7286A01", false );
            RockMigrationHelper.UpdateDefinedValue( "354715FA-564A-420A-8324-0411988AE7AB", "Organizational", "How you lead a team or organization.", "112A35BE-3108-48D9-B057-125A788AB531", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "112A35BE-3108-48D9-B057-125A788AB531", "07E85FA1-8F86-4414-8DC3-43D303C55457", @"<p>This cluster of motivators concentrates on how you connect to or lead an organization or team with which you are associated. The Organizational Cluster focuses on how you connect within the organization compared to the Operational Cluster which focuses on how you connect based on your role. The motivators in this cluster can be seen in the type of behavior you demonstrate as it relates to the long-term health of the organization or team. What you value in regard to your organization will be directly related to your top motivators from within this cluster. The greater the number of the motivators from this cluster you possess at the top third of your profile, the more dedicated you will be to making an impact on the organization or team.</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "112A35BE-3108-48D9-B057-125A788AB531", "8B5F72E4-5A49-4224-9437-82B1F23D8896", @"#0067cb" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "112A35BE-3108-48D9-B057-125A788AB531", "CE3F126E-B56A-438A-BA45-8EC8437BB961", @"core_MotivatorClusterOrganizational" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "58FEF15F-561D-420E-8937-6CF51D296F0E", "07E85FA1-8F86-4414-8DC3-43D303C55457", @"<p>This cluster of motivators concentrates on how you use your cognitive faculties throughout your life. These motivators can be seen in the way you think or the kind of mental activities you naturally pursue. The way you view your mental activity and the way you think about your thinking will be directly influenced by the motivators in this cluster. Your conversations with yourself and others will be greatly influenced by the motivators from this cluster which are in the top third of your profile.  Others will generally see these motivators through how you talk, what you read or how you use your discretionary time.</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "58FEF15F-561D-420E-8937-6CF51D296F0E", "8B5F72E4-5A49-4224-9437-82B1F23D8896", @"#cb0002" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "58FEF15F-561D-420E-8937-6CF51D296F0E", "CE3F126E-B56A-438A-BA45-8EC8437BB961", @"core_MotivatorClusterIntellectual" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "840C414E-A261-4243-8302-6117E8949FE4", "07E85FA1-8F86-4414-8DC3-43D303C55457", @"<p>This cluster of motivators describes how you relate to others. These motivators can best be seen as the reason you build relationships with people around you. Most of us are motivated to develop connections with others for more than one reason, but the motivators in this cluster will no doubt influence what you value in relationships. The greater the number of the motivators from this cluster you possess at the top third of your profile, the more strongly you will be focused on building healthy relationships for a variety of reasons.</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "840C414E-A261-4243-8302-6117E8949FE4", "8B5F72E4-5A49-4224-9437-82B1F23D8896", @"#00cb64" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "840C414E-A261-4243-8302-6117E8949FE4", "CE3F126E-B56A-438A-BA45-8EC8437BB961", @"core_MotivatorClusterInfluential" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "84322020-4E27-44EF-88F2-EAFDB7286A01", "07E85FA1-8F86-4414-8DC3-43D303C55457", @"<p>This cluster of motivators concentrates on how you execute the role you prefer. While the Organizational Cluster focuses on your connection with the organization the Operational Cluster focuses on your connection to your role. The motivators in this cluster can be seen in the way you carry out the activities of what you do, moment by moment. They dramatically influence what you value and how you spend your time or effort at work or in the tasks you perform throughout the day. When others look at the way you work, your behavior will be greatly influenced by the motivators from this cluster which are in the top third of your profile.</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "84322020-4E27-44EF-88F2-EAFDB7286A01", "8B5F72E4-5A49-4224-9437-82B1F23D8896", @"#cb6400" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "84322020-4E27-44EF-88F2-EAFDB7286A01", "CE3F126E-B56A-438A-BA45-8EC8437BB961", @"core_MotivatorClusterOperational" );
        }

        private void AddMotivatorDefinedType()
        {
            RockMigrationHelper.AddDefinedType( "TrueWiring", "Motivator", "Used by Rock's TrueWiring Motivator assessment to hold all the motivator values.", "1DFF1804-0055-491E-9559-54EA3F8F89D1", @"" );
            RockMigrationHelper.AddDefinedTypeAttribute( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Cluster", "Cluster", "", 1025, "", "A20E6DB1-B830-4D41-9003-43A184E4C910" );
            RockMigrationHelper.AddDefinedTypeAttribute( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Attribute Score Key", "AttributeScoreKey", "", 1022, "", "55FDABC3-22AE-4EE4-9883-8234E3298B99" );
            RockMigrationHelper.AddDefinedTypeAttribute( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "MotivatorId", "MotivatorId", "", 1023, "", "8158A336-8129-4E82-8B61-8C0E883CB91A" );
            RockMigrationHelper.AddDefinedTypeAttribute( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "D747E6AE-C383-4E22-8846-71518E3DD06F", "Color", "Color", "", 1024, "", "9227E7D4-5725-49BD-A0B1-43B769E0A529" );
            RockMigrationHelper.AddAttributeQualifier( "55FDABC3-22AE-4EE4-9883-8234E3298B99", "ispassword", "False", "DFF0DDA7-8467-491A-9D50-849DB09787D4" );
            RockMigrationHelper.AddAttributeQualifier( "55FDABC3-22AE-4EE4-9883-8234E3298B99", "maxcharacters", "", "0215F71D-00E2-41BE-9D69-04375C11CF64" );
            RockMigrationHelper.AddAttributeQualifier( "55FDABC3-22AE-4EE4-9883-8234E3298B99", "showcountdown", "False", "26967EB4-A1A4-4C46-9106-EE985E5BEDFF" );
            RockMigrationHelper.AddAttributeQualifier( "8158A336-8129-4E82-8B61-8C0E883CB91A", "ispassword", "False", "7BE1FF02-DCDB-42DC-891F-60F632915F0B" );
            RockMigrationHelper.AddAttributeQualifier( "8158A336-8129-4E82-8B61-8C0E883CB91A", "maxcharacters", "", "BEAA8022-951F-4A7D-B9E8-0EADF134106B" );
            RockMigrationHelper.AddAttributeQualifier( "8158A336-8129-4E82-8B61-8C0E883CB91A", "showcountdown", "False", "ECD77226-33DC-4184-9DD4-FB9110C38566" );
            RockMigrationHelper.AddAttributeQualifier( "9227E7D4-5725-49BD-A0B1-43B769E0A529", "selectiontype", "Color Picker", "95CCC80C-DCA9-467B-9E42-E3734B6129BC" );
            RockMigrationHelper.AddAttributeQualifier( "A20E6DB1-B830-4D41-9003-43A184E4C910", "allowmultiple", "False", "929F009E-F42F-4893-BD46-34E5945D46BC" );
            RockMigrationHelper.AddAttributeQualifier( "A20E6DB1-B830-4D41-9003-43A184E4C910", "definedtype", "78", "6C084DB5-5EC0-4E73-BAE7-775AE429C852" );
            RockMigrationHelper.AddAttributeQualifier( "A20E6DB1-B830-4D41-9003-43A184E4C910", "displaydescription", "False", "5CA3CB93-B7F0-4C31-8101-0F0AC78AED16" );
            RockMigrationHelper.AddAttributeQualifier( "A20E6DB1-B830-4D41-9003-43A184E4C910", "enhancedselection", "False", "FD29FB8E-E349-4A0C-BF62-856B1AC851E1" );
            RockMigrationHelper.AddAttributeQualifier( "A20E6DB1-B830-4D41-9003-43A184E4C910", "includeInactive", "False", "0008F665-2B5A-49CB-9699-58F72FAC12EF" );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Believing", "This motivator influences you to pursue the principles which you believe in with dogged determination. You have a tremendous capacity to be tenacious in pursuing principles. You clearly know what you believe and are able to articulate these beliefs with others. You have expectations for yourself and others regarding those beliefs. You know that you have formed your beliefs through wise experience, counsel, and judgment. You influence others through your convictions. The challenge of this motivator is that some may see you as a black and white person.", "99F598E0-E0AC-4B4B-BEAF-589D41764EE1", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Caring", "This motivator influences you to pursue meeting the needs of others. You genuinely care for others especially when they are hurting. You have a large capacity for supporting others. Others would see you as empathetic; especially for individuals in difficult situations. You may find it easy to identify with the pain that others experience. You influence others through your care and compassion. The challenge to this motivator is that you may become so consumed in the needs that you miss long term solutions.", "FFD7EF9C-5D68-40D2-A362-416B2D660D51", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Communicating", "This motivator influences you to seek opportunities to speak in a variety of environments. You have the capacity to speak effectively. You enjoy talking and engaging others with what you know. You find it easy to articulate your thoughts and to share them in a credible manner. You are not intimidated by speaking in front of others, and you find pleasure in persuading others to your perspective. You influence others through speaking and convincing others of your position. The challenge to this motivator is you may feel if you talk enough people will understand.", "FA70E27D-6642-4162-AF17-530F66B507E7", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Empowering", "This motivator influences you to equip others to do what they are gifted to do. You have the capacity to equip and release individuals. You enjoy developing relationships with individuals who you can mentor and model leadership. You find it easy to see the strengths of others and also know what they need to develop. You will influence others through your investing in others to do so much more than they could have done without your intentional effort. The challenge to this motivator is you may not always address negative issues in the lives of those you are developing.", "C171D01E-C607-488B-A550-1E341081210B", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Engaging", "This motivator influences you to become deeply involved in the needs of the community. You understand the community, and you have a keen sense of how to meet those needs. You are frustrated when you see community needs go unmet. You have a desire to be involved in various community organizations which are making an impact in your community. You will influence others through your engaging the community needs in real and tangible ways to make a difference. The challenge with this motivator is you may not always see issues which need addressing in your life because you are so focused on the community.", "5635E95B-3A07-43B7-837A-0F131EF1DA97", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Flexing", "This motivator influences you to change quickly when circumstances require it. You have the capacity to be adaptable in diverse situations. You understand when the need for transformation is necessary before others. You enjoy change. You have a desire to be on the front edge of any movement. You will influence others by modifying what is currently not working so that you are better prepared to handle challenges. The challenge with this motivator is that you are so quick to adjust and adapt that you may miss the positive results of perseverance.", "BD5D99E7-E0FF-4535-8B26-BF73EF9B9F89", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Gathering", "This motivator influences you to bring people together.  You have the capacity to engage people, so they want to be around you. You understand what people want in a relationship and are able to meet those needs. You enjoy gathering people around you. You have a desire to influence those who are drawn to you. You will influence others by bringing them onboard with you wherever you go. The challenge with this motivator is that you may enjoy bringing people together more than actually accomplishing something together.", "73087DD2-B892-4367-894F-8922477B2F10", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Growth Propensity", "The Growth Propensity Scale (GPS) measures your perceived mindset on a continuum between a Growth Mindset (higher the number) or Fixed Mindset. Carol Dweck’s book, Mindset*, provides a framework to describe your state of mind in this context. She points to these mindsets as being two ends of a spectrum in how we view our own capacity and potential to grow or change. Most of us have general tendencies toward one type or another and may even display different mindsets in different areas of our lives.  You may display a fixed mindset in one area of your life, or in your life in general, but you are not destined to stay there. We are all products of our past, but we are not prisoners! No matter where your score is on this spectrum, it will influence what you do with the results of the previous twenty-two scales. Let me encourage you to start by reading Carol’s book and invest time in the exercises she outlines.  Remember, you may be a product of the past, but not a prisoner of your past...start changing today!", "605F3702-6AE7-4545-BEBE-23693E60031C", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Innovating", "This motivator influences you to look for new ways to do everything continuously. You have the capacity to see what could be done better. You understand why something isn’t working, and you can figure out how to do it differently. You enjoy finding new ways of doing something and have a desire to create something from nothing. You may be frustrated if something isn’t difficult for you. You will influence others through starting something with a high level of energy which may never have been done before. The challenge with this motivator is that you may enjoy creating so much that there is no execution of a plan to bring innovation to reality.", "D84E58E4-87FC-4CEB-B83E-A2C6D186366C", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Leading", "This motivator influences you to bring others together to accomplish a task. You have the capacity to take responsibility for others to achieve something together. You understand what needs to happen in most situations and can mobilize others to work together to undertake it. You enjoy others following your lead, and you have a desire to make an impact through others working with you. You will influence others by your ability to inspire and engage them to accomplish more together than they could have accomplished individually. The challenge with this motivator is that you may not feel comfortable as “just being one of a team” where you are not the sole leader.", "6A2354C6-3FA4-4BAD-89A8-7359FEC48FE3", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Learning", "This motivator influences you to seek out opportunities to understand new things continually. You have the capacity to continue learning through various media. You understand there is so much more to know about our world, and you feel stagnant if you are not growing in some manner. You have a desire to learn all that you can about everything. Every opportunity is an opportunity to learn and grow. You will influence others through what you are learning in one area and helping them apply it in different areas. The challenge with this motivator is that you may enjoy learning so much that there is little effort to actually do something with what you learn.", "7EA44A56-58CB-4E40-9779-CC0A79772926", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Maximizing", "This motivator influences you to only invest your time, money or effort in areas that can give you a significant rate of return for your investment. You have the capacity to see opportunities where a substantial benefit will result. You understand what is necessary to make an impact using your resources. You enjoy seeing a high return from your investment, and you have a desire to make it even higher. You will influence others through your strategic sense of when and where to invest resources for maximum impact. The challenge with this motivator is that at times you may need to serve others on the team rather than maximizing your own impact.", "3F678404-5844-494F-BDB0-DD9FEEBC98C9", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Organizing", "This motivator influences you to seek opportunities where you can bring disarray under control. You have the capacity to bring order out of disorder. You understand that the “devil is in the details,” so you work on creating systems to maintain control of the details. You enjoy when everything flows as planned. You have a desire to organize various pieces into one coherent whole. You will influence others by bringing disparate fragments together in alignment with your objectives. The challenge with this motivator is that you may be resistance to change because it will bring unwanted chaos.", "85459C0F-65A5-48F9-86F3-40B03F9C53E9", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Pacing", "This motivator influences you to keep a consistent and stable structure in your life and work. You have the capacity to know when your life is getting out of balanced. You understand how much you can handle and what has to change for your work/life balance to achieve healthy stability. You enjoy living in a structured and consistent manner. You desire to create beneficial boundaries in all areas of your life. You will influence others by your modeling and espousing the long-term sustainable margins within life and work. The challenge with this motivator is that you may resist times when an imbalance is required to complete a task with excellence.", "9F771853-2EBA-47A2-9AC5-26EBEA0A3B25", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Perceiving", "This motivator influences to discern in others what is not readily apparent. You have the capacity to observe behaviors in other which is not consistent with what they are saying. You understand when things don’t sometimes turn out to be as they were initially described. You enjoy trying to understand others. You have a desire to find out if your intuitions are correct. You will influence others through your ability to know things that will help the team be more effective in working together. The challenge with this motivator is you may not know what to do with what you are perceiving, so you gossip to others about your insights.", "4C898A5C-B48E-4BAE-AB89-835F25A451BF", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Relating", "This motivator influences you to seek others with whom you can build relationships. You have the capacity to draw people into your sphere of trust and rapport. You understand what others want or need relationally and are able to provide that in a healthy manner. You enjoy the close relationships that develop others. You have a desire to ensure everyone is tied into others through an interpersonal network. You will influence others by building strong ties with others who are socially connected to you. The challenge with this motivator is you may form relationships with so many people that you simply cannot maintain integrity and depth in them all.", "D7F9BDE2-8BEB-469E-BAD9-AA4DEBD3D995", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Serving", "This motivator influences to attend to others or the team for their best, not your own. You have the capacity to work behind the scenes when others are not even aware of what you are doing. You understand what needs to be done long before others. You enjoy doing the little things which may have gone unattended. You have a desire to serve others so they can flourish. You will influence others by helping them so they can function within their primary motivators. The challenge with this motivator is that you and/or others may undervalue your contribution to the group or team.", "D8430EAD-7A38-4AD1-B21A-B2119EE0F1CD", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Thinking", "This motivator influences you to be intentionally aware of your thinking at any given moment. You have the capacity to consciously think about your thinking. You understand what is going on in your mind and why you are thinking the way you are thinking. You enjoy reflecting on why you think as you do. You have a desire to understand the patterns in why others respond as they do based upon what you understand about your thinking. You will influence others by your insight into what others may be thinking in various situations. The challenge with this motivator is you can get lost in your thinking rather than making a decision and moving forward.", "0D82DC77-334C-44B0-84A6-989910907DD4", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Transforming", "This motivator influences you to try to improve organizations which you are a part of. You have the capacity to know what are the crucial changes which need to be made to bring about progress. You understand how to bring people along with the necessary changes, and you enjoy walking with people through those essential changes. You have a desire to help people embrace differences which are best for the team. You will influence others by enabling them to feel comfortable and commit to organizational transformation. The challenge with this motivator is you may experience the discomfort of leaving people behind who do not want to change while feeling the need to keep moving ahead with the change.", "2393C3CE-8E49-46FE-A75B-D5D624A37B49", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Uniting", "This motivator influences you to continually connect people around a common cause. You have the capacity to get people to gain ownership of a vision. You understand how to deal with the individual needs in order for the team to win. You enjoy helping others feel a part of the group. You have a desire to have everyone on the team to feel responsible for the success of the team. You will influence others by creating a sense of belonging for every member of the team. The challenge with this motivator is you may spend so long unifying the team, that it hurts the critical progress or momentum.", "D7601B56-7495-4D7B-A916-8C48F78675E3", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Unwavering", "This motivator influences you to maintain trying to accomplish the goal long after others have given up. You have the capacity to ride the ups and downs of circumstances without being defeated. You understand that all forward progress is accomplished through grit and hard work. You see demanding times as a test of your abilities. You have a desire to succeed in spite of resistance. You will influence others through your resilience and perseverance in difficult times. The challenge with this motivator is you may not walk away from some situations which simply are not worth the effort.", "A027F6B2-56DD-4724-962D-F865606AEAB8", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Venturing", "This motivator influences you to seek out opportunities involving risk and challenge. You have the capacity to thrive in environments which entail some level of risk. You understand how to handle uncertainty in the assignments you pursue. You enjoy the thrill of being challenged to attempt something you have not previously tried. You have a desire to do things that test you. You will influence others through your continued stretching the team to try something new. The challenge with this motivator is you will seldom be satisfied with the status quo and therefore easily bored.", "4D0A1A6D-3F5A-476E-A633-04EAEF457645", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Visioning", "This motivator influences you to dream of things which don’t exist yet. You have the capacity to picture what things could be in the future for your team. You understand that you can’t accomplish this by yourself; therefore, you enjoy attracting others to your preferred picture of the future. You have a desire for your organization to be much more than it currently is and want to bring that into reality. You will influence others through your inspiring and encouraging them to see much more than their current reality. The challenge with this motivator is you can tend to live in the future and get frustrated with the realities of the current situation.", "EE1603BA-41AE-4CFA-B220-065768996501", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0D82DC77-334C-44B0-84A6-989910907DD4", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorThinking" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0D82DC77-334C-44B0-84A6-989910907DD4", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F17" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0D82DC77-334C-44B0-84A6-989910907DD4", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#e50002" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0D82DC77-334C-44B0-84A6-989910907DD4", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"58fef15f-561d-420e-8937-6cf51d296f0e" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2393C3CE-8E49-46FE-A75B-D5D624A37B49", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorTransforming" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2393C3CE-8E49-46FE-A75B-D5D624A37B49", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F18" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2393C3CE-8E49-46FE-A75B-D5D624A37B49", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#005ab2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2393C3CE-8E49-46FE-A75B-D5D624A37B49", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"112a35be-3108-48d9-b057-125a788ab531" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3F678404-5844-494F-BDB0-DD9FEEBC98C9", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorMaximizing" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3F678404-5844-494F-BDB0-DD9FEEBC98C9", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F11" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3F678404-5844-494F-BDB0-DD9FEEBC98C9", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#b25700" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3F678404-5844-494F-BDB0-DD9FEEBC98C9", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"84322020-4e27-44ef-88f2-eafdb7286a01" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4C898A5C-B48E-4BAE-AB89-835F25A451BF", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorPerceiving" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4C898A5C-B48E-4BAE-AB89-835F25A451BF", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F14" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4C898A5C-B48E-4BAE-AB89-835F25A451BF", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#cb0002" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4C898A5C-B48E-4BAE-AB89-835F25A451BF", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"58fef15f-561d-420e-8937-6cf51d296f0e" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4D0A1A6D-3F5A-476E-A633-04EAEF457645", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorVenturing" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4D0A1A6D-3F5A-476E-A633-04EAEF457645", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F21" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4D0A1A6D-3F5A-476E-A633-04EAEF457645", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#0081fe" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4D0A1A6D-3F5A-476E-A633-04EAEF457645", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"112a35be-3108-48d9-b057-125a788ab531" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5635E95B-3A07-43B7-837A-0F131EF1DA97", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorEngaging" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5635E95B-3A07-43B7-837A-0F131EF1DA97", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F05" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5635E95B-3A07-43B7-837A-0F131EF1DA97", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#00984b" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5635E95B-3A07-43B7-837A-0F131EF1DA97", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"840c414e-a261-4243-8302-6117e8949fe4" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "605F3702-6AE7-4545-BEBE-23693E60031C", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorGrowthPropensity" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "605F3702-6AE7-4545-BEBE-23693E60031C", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"PS01" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "605F3702-6AE7-4545-BEBE-23693E60031C", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#6400cb" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6A2354C6-3FA4-4BAD-89A8-7359FEC48FE3", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorLeading" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6A2354C6-3FA4-4BAD-89A8-7359FEC48FE3", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F09" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6A2354C6-3FA4-4BAD-89A8-7359FEC48FE3", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#004d98" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6A2354C6-3FA4-4BAD-89A8-7359FEC48FE3", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"112a35be-3108-48d9-b057-125a788ab531" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "73087DD2-B892-4367-894F-8922477B2F10", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorGathering" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "73087DD2-B892-4367-894F-8922477B2F10", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F07" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "73087DD2-B892-4367-894F-8922477B2F10", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#00b257" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "73087DD2-B892-4367-894F-8922477B2F10", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"840c414e-a261-4243-8302-6117e8949fe4" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7EA44A56-58CB-4E40-9779-CC0A79772926", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorLearning" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7EA44A56-58CB-4E40-9779-CC0A79772926", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F10" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7EA44A56-58CB-4E40-9779-CC0A79772926", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#b20002" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7EA44A56-58CB-4E40-9779-CC0A79772926", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"58fef15f-561d-420e-8937-6cf51d296f0e" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "85459C0F-65A5-48F9-86F3-40B03F9C53E9", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorOrganizing" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "85459C0F-65A5-48F9-86F3-40B03F9C53E9", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F12" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "85459C0F-65A5-48F9-86F3-40B03F9C53E9", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#cb6400" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "85459C0F-65A5-48F9-86F3-40B03F9C53E9", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"84322020-4e27-44ef-88f2-eafdb7286a01" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99F598E0-E0AC-4B4B-BEAF-589D41764EE1", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorBelieving" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99F598E0-E0AC-4B4B-BEAF-589D41764EE1", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F01" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99F598E0-E0AC-4B4B-BEAF-589D41764EE1", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#7f0001" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99F598E0-E0AC-4B4B-BEAF-589D41764EE1", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"58fef15f-561d-420e-8937-6cf51d296f0e" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9F771853-2EBA-47A2-9AC5-26EBEA0A3B25", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorPacing" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9F771853-2EBA-47A2-9AC5-26EBEA0A3B25", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F13" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9F771853-2EBA-47A2-9AC5-26EBEA0A3B25", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#e57100" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9F771853-2EBA-47A2-9AC5-26EBEA0A3B25", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"84322020-4e27-44ef-88f2-eafdb7286a01" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A027F6B2-56DD-4724-962D-F865606AEAB8", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorUnwavering" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A027F6B2-56DD-4724-962D-F865606AEAB8", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F20" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A027F6B2-56DD-4724-962D-F865606AEAB8", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#fe7d00" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A027F6B2-56DD-4724-962D-F865606AEAB8", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"84322020-4e27-44ef-88f2-eafdb7286a01" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BD5D99E7-E0FF-4535-8B26-BF73EF9B9F89", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorFlexing" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BD5D99E7-E0FF-4535-8B26-BF73EF9B9F89", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F06" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BD5D99E7-E0FF-4535-8B26-BF73EF9B9F89", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#7f3e00" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BD5D99E7-E0FF-4535-8B26-BF73EF9B9F89", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"84322020-4e27-44ef-88f2-eafdb7286a01" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C171D01E-C607-488B-A550-1E341081210B", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorEmpowering" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C171D01E-C607-488B-A550-1E341081210B", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F04" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C171D01E-C607-488B-A550-1E341081210B", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#00407f" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C171D01E-C607-488B-A550-1E341081210B", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"112a35be-3108-48d9-b057-125a788ab531" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7601B56-7495-4D7B-A916-8C48F78675E3", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorUniting" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7601B56-7495-4D7B-A916-8C48F78675E3", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F19" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7601B56-7495-4D7B-A916-8C48F78675E3", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#00e571" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7601B56-7495-4D7B-A916-8C48F78675E3", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"840c414e-a261-4243-8302-6117e8949fe4" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7F9BDE2-8BEB-469E-BAD9-AA4DEBD3D995", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorRelating" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7F9BDE2-8BEB-469E-BAD9-AA4DEBD3D995", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F15" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7F9BDE2-8BEB-469E-BAD9-AA4DEBD3D995", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#00cb64" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7F9BDE2-8BEB-469E-BAD9-AA4DEBD3D995", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"840c414e-a261-4243-8302-6117e8949fe4" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D8430EAD-7A38-4AD1-B21A-B2119EE0F1CD", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorServing" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D8430EAD-7A38-4AD1-B21A-B2119EE0F1CD", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F16" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D8430EAD-7A38-4AD1-B21A-B2119EE0F1CD", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#0074e5" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D8430EAD-7A38-4AD1-B21A-B2119EE0F1CD", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"112a35be-3108-48d9-b057-125a788ab531" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D84E58E4-87FC-4CEB-B83E-A2C6D186366C", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorInnovating" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D84E58E4-87FC-4CEB-B83E-A2C6D186366C", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F08" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D84E58E4-87FC-4CEB-B83E-A2C6D186366C", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#984b00" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D84E58E4-87FC-4CEB-B83E-A2C6D186366C", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"84322020-4e27-44ef-88f2-eafdb7286a01" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EE1603BA-41AE-4CFA-B220-065768996501", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorVisioning" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EE1603BA-41AE-4CFA-B220-065768996501", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F22" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EE1603BA-41AE-4CFA-B220-065768996501", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#0067cb" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EE1603BA-41AE-4CFA-B220-065768996501", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"112a35be-3108-48d9-b057-125a788ab531" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FA70E27D-6642-4162-AF17-530F66B507E7", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorCommunicating" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FA70E27D-6642-4162-AF17-530F66B507E7", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F03" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FA70E27D-6642-4162-AF17-530F66B507E7", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#980001" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FA70E27D-6642-4162-AF17-530F66B507E7", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"58fef15f-561d-420e-8937-6cf51d296f0e" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FFD7EF9C-5D68-40D2-A362-416B2D660D51", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorCaring" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FFD7EF9C-5D68-40D2-A362-416B2D660D51", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F02" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FFD7EF9C-5D68-40D2-A362-416B2D660D51", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#007f3e" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FFD7EF9C-5D68-40D2-A362-416B2D660D51", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"840c414e-a261-4243-8302-6117e8949fe4" );
        }

        private void UpdateGiftsPageLayout()
        {
            string updatePageSQL = @"

                DECLARE @LayoutId int = ( SELECT [Id] FROM [Layout] WHERE [Guid] = 'BE15B7BC-6D64-4880-991D-FDE962F91196' )
                
                UPDATE [Page]
                SET [LayoutId] = @LayoutId
                WHERE [Guid] = ('06410598-3DA4-4710-A047-A518157753AB')";

            Sql( updatePageSQL );
            
        }

        
        /// <summary>
        /// ED: Updates the Category Name of the TrueWiring Defined Types to 'Personality Assessments' 
        /// </summary>
        private void DefinedTypeCategoryTrueWiringToPersonalityAssessmentsUp()
        {
            RockMigrationHelper.UpdateCategory( "6028D502-79F4-4A74-9323-525E90F900C7", "Personality Assessments", "", "", "6A259E9A-232F-4835-B3F0-B06376A13997" );
        }

        /// <summary>
        /// Reverts - ED: Updates the Category Name of the TrueWiring Defined Types to 'Personality Assessments' 
        /// </summary>
        private void DefinedTypeCategoryTrueWiringToPersonalityAssessmentsDown()
        {
            RockMigrationHelper.UpdateCategory( "6028D502-79F4-4A74-9323-525E90F900C7", "TrueWiring", "", "", "6A259E9A-232F-4835-B3F0-B06376A13997" );
        }

        private void RenamePersonAttributeCategoryTrueWiring()
        {
            RockMigrationHelper.UpdatePersonAttributeCategory( "Personality Assessment Data", "fa fa-directions", string.Empty, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969" );

         //   RockMigrationHelper.DeleteSecurityAuthForCategory( "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969" );
         //   RockMigrationHelper.AddSecurityAuthForCategory(
         //               "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969",
         //               0,
         //               Rock.Security.Authorization.VIEW,
         //               false,
         //               null,
         //               ( int ) Rock.Model.SpecialRole.AllUsers,
         //               Guid.NewGuid().ToString() );
         //RockMigrationHelper.AddSecurityAuthForCategory(
         //               "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969",
         //               0,
         //               Rock.Security.Authorization.EDIT,
         //               false,
         //               null,
         //               ( int ) Rock.Model.SpecialRole.AllUsers,
         //               Guid.NewGuid().ToString() );

        }

        private void UpdateSpiritualGiftsDefinedValuesUp()
        {
            Sql( @"
                UPDATE [dbo].[DefinedValue] SET [Value] = 'Service' WHERE [Guid] = '13C40209-F41D-4C1D-83D3-2EC530588245'
                UPDATE [dbo].[DefinedValue] SET [Value] = 'Shepherding' WHERE [Guid] = 'FC4F1B46-F0C3-45B0-9FD9-D15F4FD05A31'
                UPDATE [dbo].[DefinedValue] SET [Value] = 'Mentoring' WHERE [Guid] = 'C7291F22-05F0-4EF9-A7C2-2CFEBFEBCB45'" );
        }

        private void UpdateSpiritualGiftsDefinedValuesDown()
        {
            Sql( @"
                UPDATE [dbo].[DefinedValue] SET [Value] = 'Helps/Service' WHERE [Guid] = '13C40209-F41D-4C1D-83D3-2EC530588245'
                UPDATE [dbo].[DefinedValue] SET [Value] = 'Pastor-Shepherd' WHERE [Guid] = 'FC4F1B46-F0C3-45B0-9FD9-D15F4FD05A31'
                UPDATE [dbo].[DefinedValue] SET [Value] = 'Pastor-Teacher' WHERE [Guid] = 'C7291F22-05F0-4EF9-A7C2-2CFEBFEBCB45'" );
        }

        private void AddDiscProfilePersonAttributeUp()
        {
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( "59D5A94C-94A0-4630-B80A-BB25697D74C7", "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "DISC: DISC Profile", "DISC Profile", "core_DISCDISCProfile", "", "", 1029, "", "6EAC3DF8-CA81-41A5-B1CF-A8DD7BD42F8D" );
        }

        private void AddDiscProfilePersonAttributeDown()
        {
            RockMigrationHelper.DeleteAttribute( "6EAC3DF8-CA81-41A5-B1CF-A8DD7BD42F8D" );
        }

    }
}
