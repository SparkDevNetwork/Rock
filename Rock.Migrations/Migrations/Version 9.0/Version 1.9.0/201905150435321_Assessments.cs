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
            AddColumn( "dbo.Attribute", "AbbreviatedName", c => c.String( maxLength: 100 ) );
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
            UpdateAdaptiveDiscAttributeCategoryUp();
            UpdateNaturalDiscAttributesUp();
            UpdateMotivatorDefinedType();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateNatrualDiscAttributesDown();
            UpdateAdaptiveDiscAttributeCategoryDown();
            AddDiscProfilePersonAttributeDown();
            UpdateSpiritualGiftsDefinedValuesDown();
            DefinedTypeCategoryTrueWiringToPersonalityAssessmentsDown();
            PagesBlocksAndAttributesDown();
            AssessmentRemindersServiceJobDown();
            RockMigrationHelper.DeleteSystemEmail( "41FF4269-7B48-40CD-81D4-C11370A13DED" ); // Assessment Request System Email
            AddConflictProfileDefinedTypeAndAttributesDown();
            CreateTablesDown();
            DropColumn( "dbo.Attribute", "AbbreviatedName" );
            AddFieldTypeConditionalScaleDown();
        }

        private void CreateTablesUp()
        {
            CreateTable(
                "dbo.Assessment",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    PersonAliasId = c.Int( nullable: false ),
                    AssessmentTypeId = c.Int( nullable: false ),
                    RequesterPersonAliasId = c.Int(),
                    RequestedDateTime = c.DateTime(),
                    RequestedDueDate = c.DateTime(),
                    Status = c.Int( nullable: false ),
                    CompletedDateTime = c.DateTime(),
                    AssessmentResultData = c.String(),
                    LastReminderDate = c.DateTime(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.AssessmentType", t => t.AssessmentTypeId, cascadeDelete: true )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.PersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.RequesterPersonAliasId )
                .Index( t => t.PersonAliasId )
                .Index( t => t.AssessmentTypeId )
                .Index( t => t.RequesterPersonAliasId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.AssessmentType",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Title = c.String( nullable: false, maxLength: 100 ),
                    Description = c.String( nullable: false, maxLength: 100 ),
                    AssessmentPath = c.String( nullable: false, maxLength: 250 ),
                    AssessmentResultsPath = c.String( maxLength: 250 ),
                    IsActive = c.Boolean( nullable: false ),
                    RequiresRequest = c.Boolean( nullable: false ),
                    MinimumDaysToRetake = c.Int( nullable: false ),
                    ValidDuration = c.Int( nullable: false ),
                    IsSystem = c.Boolean( nullable: false ),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );
        }

        private void CreateTablesDown()
        {
            DropForeignKey( "dbo.Assessment", "RequesterPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Assessment", "PersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Assessment", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Assessment", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.Assessment", "AssessmentTypeId", "dbo.AssessmentType" );
            DropForeignKey( "dbo.AssessmentType", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.AssessmentType", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropIndex( "dbo.AssessmentType", new[] { "Guid" } );
            DropIndex( "dbo.AssessmentType", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.AssessmentType", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.Assessment", new[] { "Guid" } );
            DropIndex( "dbo.Assessment", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.Assessment", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.Assessment", new[] { "RequesterPersonAliasId" } );
            DropIndex( "dbo.Assessment", new[] { "AssessmentTypeId" } );
            DropIndex( "dbo.Assessment", new[] { "PersonAliasId" } );
            DropTable( "dbo.AssessmentType" );
            DropTable( "dbo.Assessment" );
        }

        private void AddFieldTypeConditionalScaleUp()
        {
            RockMigrationHelper.UpdateFieldType( "Conditional Scale", "", "Rock", "Rock.Field.Types.ConditionalScaleFieldType", "E73B9F41-8325-4229-8EA5-75180066680C" );
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
                    '{guid}')" );
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
            RockMigrationHelper.UpdatePersonAttributeCategory( "Conflict Engagement", "fa fa-handshake", "", "EDD33F72-ECED-49BC-AC49-3643B60AD736" );

            var categories = new System.Collections.Generic.List<string> { "EDD33F72-ECED-49BC-AC49-3643B60AD736" };

            // Person Attribute "Conflict Mode: Winning"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", categories, @"Conflict Mode: Winning", @"Mode: Winning", @"core_ConflictModeWinning", @"", @"", 3, @"", @"7147F706-388E-45E6-BE21-893FC7D652AA" );
            RockMigrationHelper.AddAttributeQualifier( @"7147F706-388E-45E6-BE21-893FC7D652AA", @"ConfigurationJSON", @"[{""Guid"":""951e4864-78c7-4d9c-8548-fc7d6a5cc91b"",""RangeIndex"":0,""Label"":""High"",""Color"":""#b21f22"",""HighValue"":null,""LowValue"":67.0},{""Guid"":""ca9fd375-5adf-4a56-ae84-5941015f454e"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#e15759"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""2f91c1ea-3d13-4a5c-9f33-ed8948e7fb1c"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#f0adae"",""HighValue"":33.0,""LowValue"":0.0}]", @"33306923-B938-4156-A4D6-4ABF75146B9D" );
            AddReadWriteSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_CONFLICT_MODE_WINNING );

            // Person Attribute "Conflict Mode: Resolving"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", categories, @"Conflict Mode: Resolving", @"Mode: Resolving", @"core_ConflictModeResolving", @"", @"", 4, @"", @"5B811EAC-51B2-41F2-A55A-C966D9DB05EE" );
            RockMigrationHelper.AddAttributeQualifier( @"5B811EAC-51B2-41F2-A55A-C966D9DB05EE", @"ConfigurationJSON", @"[{""Guid"":""022c15d2-9e23-46aa-a18b-3e9a49183adf"",""RangeIndex"":0,""Label"":""High"",""Color"":""#43678e"",""HighValue"":null,""LowValue"":67.0},{""Guid"":""7ca1d560-fc1e-4f20-b4e8-38ae3ab1385b"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#789abf"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""11d01d8f-d555-409f-b563-964461382a17"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#bdcee0"",""HighValue"":33.0,""LowValue"":0.0}]", @"67985444-26DE-46F1-8A93-41EE55052170" );
            AddReadWriteSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_CONFLICT_MODE_RESOLVING );

            // Person Attribute "Conflict Mode: Compromising"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", categories, @"Conflict Mode: Compromising", @"Mode: Compromising", @"core_ConflictModeCompromising", @"", @"", 5, @"", @"817D6B13-E4AA-4E93-8547-FE711A0065F2" );
            RockMigrationHelper.AddAttributeQualifier( @"817D6B13-E4AA-4E93-8547-FE711A0065F2", @"ConfigurationJSON", @"[{""Guid"":""b08b08fd-88a9-46cc-87c3-cb17368d4b56"",""RangeIndex"":0,""Label"":""High"",""Color"":""#43678e"",""HighValue"":null,""LowValue"":67.0},{""Guid"":""80c8f457-9afc-4dd1-8d18-a86037dba6c9"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#789abf"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""eba49b52-17c4-4813-9688-e316b767f295"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#bdcee0"",""HighValue"":33.0,""LowValue"":0.0}]", @"AACC439C-A2DC-40EC-BC22-96A1ABA4BB72" );
            AddReadWriteSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_CONFLICT_MODE_COMPROMISING );

            // Person Attribute "Conflict Mode: Avoiding"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", categories, @"Conflict Mode: Avoiding", @"Mode: Avoiding", @"core_ConflictModeAvoiding", @"", @"", 6, @"", @"071A8EFA-AD1C-436A-8E1E-23D215617004" );
            RockMigrationHelper.AddAttributeQualifier( @"071A8EFA-AD1C-436A-8E1E-23D215617004", @"ConfigurationJSON", @"[{""Guid"":""412a6f33-eee1-4179-a762-1df8a8b1ac6b"",""RangeIndex"":0,""Label"":""High"",""Color"":""#50aa3c"",""HighValue"":null,""LowValue"":67.0},{""Guid"":""25633115-9868-4c2a-89c7-97a15509c727"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#8cd17d"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""7e6bf4ac-16fa-47e4-a60d-956924ffc474"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#cdebc6"",""HighValue"":33.0,""LowValue"":0.0}]", @"1835F432-A316-4347-8840-2DA5C3E24D2C" );
            AddReadWriteSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_CONFLICT_MODE_AVOIDING );

            // Person Attribute "Conflict Mode: Yielding"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", categories, @"Conflict Mode: Yielding", @"Mode: Yielding", @"core_ConflictModeYielding", @"", @"", 7, @"", @"D30A33AD-7A60-43E0-84DA-E23600156BF7" );
            RockMigrationHelper.AddAttributeQualifier( @"D30A33AD-7A60-43E0-84DA-E23600156BF7", @"ConfigurationJSON", @"[{""Guid"":""542a5291-7d2d-40c1-bde1-fff6167e145a"",""RangeIndex"":0,""Label"":""High"",""Color"":""#50aa3c"",""HighValue"":null,""LowValue"":67.0},{""Guid"":""5efa046b-f235-4ce3-9568-4daafb4860a4"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#8cd17d"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""f765b351-8e60-4d49-a8c4-d14ab761cbb9"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#cdebc6"",""HighValue"":33.0,""LowValue"":0.0}]", @"45C648A3-26A9-47D8-B380-3B916FAAF701" );
            AddReadWriteSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_CONFLICT_MODE_YIELDING );

            // Person Attribute "Conflict Theme: Accommodating"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", categories, @"Conflict Theme: Accommodating", @"Theme: Accommodating", @"core_ConflictThemeAccommodating", @"", @"", 2, @"", @"404A64FB-7396-4896-9C94-84DE21E995CA" );
            RockMigrationHelper.AddAttributeQualifier( @"404A64FB-7396-4896-9C94-84DE21E995CA", @"ConfigurationJSON", @"[{""Guid"":""c7a0cf27-032a-4ce4-be51-228f56750ba7"",""RangeIndex"":0,""Label"":""High"",""Color"":""#50aa3c"",""HighValue"":null,""LowValue"":67.0},{""Guid"":""fe2c3be8-75e5-4ee1-8559-8f6fa531bf07"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#8cd17d"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""1fa912d5-772d-48d4-8d35-5a3791096777"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#cdebc6"",""HighValue"":33.0,""LowValue"":0.0}]", @"4ACECE69-D2E6-494B-96A2-4C5D15392935" );
            AddReadWriteSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_CONFLICT_THEME_ACCOMMODATING );

            // Person Attribute "Conflict Theme: Winning"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", categories, @"Conflict Theme: Winning", @"Theme: Winning", @"core_ConflictThemeWinning", @"", @"", 0, @"", @"6DE5878D-7CDB-404D-93A7-27CFF5E98C3B" );
            RockMigrationHelper.AddAttributeQualifier( @"6DE5878D-7CDB-404D-93A7-27CFF5E98C3B", @"ConfigurationJSON", @"[{""Guid"":""4185359a-af32-4902-82b4-d87e5729bd81"",""RangeIndex"":0,""Label"":""High"",""Color"":""#b21f22"",""HighValue"":null,""LowValue"":67.0},{""Guid"":""c22b5ba0-06c9-4d68-814a-3b7ac603f39c"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#e15759"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""56923739-16b5-4c58-8cc4-961800117bb5"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#f0adae"",""HighValue"":33.0,""LowValue"":0.0}]", @"F43DEF27-ABE2-4015-8F3A-BB52953C4389" );
            AddReadWriteSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_CONFLICT_THEME_WINNING );

            // Person Attribute "Conflict Theme: Solving"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", categories, @"Conflict Theme: Solving", @"Theme: Solving", @"core_ConflictThemeSolving", @"", @"", 1, @"", @"33235605-D8BB-4C1E-B231-6F085970A14F" );
            RockMigrationHelper.AddAttributeQualifier( @"33235605-D8BB-4C1E-B231-6F085970A14F", @"ConfigurationJSON", @"[{""Guid"":""fbd84cfb-bd89-4ed5-a1b3-834e2118ae9b"",""RangeIndex"":0,""Label"":""High"",""Color"":""#43678e"",""HighValue"":null,""LowValue"":67.0},{""Guid"":""c43e78c1-b675-48a2-8039-5784aadb8502"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#789abf"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""b2757ecb-8d72-4ccd-af6e-d8f022b2473d"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#bdcee0"",""HighValue"":33.0,""LowValue"":0.0}]", @"34483B88-0BDF-4E23-8E56-4183FA8DF1A1" );
            AddReadWriteSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_CONFLICT_THEME_SOLVING );
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
            RockMigrationHelper.UpdatePersonAttributeCategory( "EQ", "fa fa-theater-masks", "", "0B6C7001-2D3A-4195-86CA-85C6DCBF2023" );
            var categories = new System.Collections.Generic.List<string> { "0B6C7001-2D3A-4195-86CA-85C6DCBF2023" };

            // Person Attribute "EQ: Self Awareness"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", categories, @"EQ: Self Awareness", @"Self Awareness", @"core_EQSelfAwareness", @"", @"", 0, @"", @"A5EFCE3E-EA41-4FEC-99F6-DD748A7D5BB5" );
            RockMigrationHelper.AddAttributeQualifier( @"A5EFCE3E-EA41-4FEC-99F6-DD748A7D5BB5", @"ConfigurationJSON", @"[{""Guid"":""382940e4-3083-409f-a2af-dcd0d951ef81"",""RangeIndex"":0,""Label"":""High"",""Color"":""#709ac7"",""HighValue"":null,""LowValue"":67.0},{""Guid"":""e5abd3c2-f123-40cb-84e7-e2f8c2e9cb40"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#91b1d4"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""cc2b28ae-3ff8-436d-a83d-4f559c441ce3"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#b6cbe2"",""HighValue"":33.0,""LowValue"":0.0}]", @"FBC45EB9-6E38-4C43-96AA-622D530A1206" );
            AddReadWriteSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_EQ_CONSTRUCTS_SELF_AWARENESS );

            // Person Attribute "EQ: Self Regulating"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", categories, @"EQ: Self Regulating", @"Self Regulating", @"core_EQSelfRegulating", @"", @"", 1, @"", @"149CD0CD-3CD6-44B6-8D84-A17A477A8978" );
            RockMigrationHelper.AddAttributeQualifier( @"149CD0CD-3CD6-44B6-8D84-A17A477A8978", @"ConfigurationJSON", @"[{""Guid"":""0cb16450-f31e-48cc-8b56-6b851416551e"",""RangeIndex"":0,""Label"":""High"",""Color"":""#709ac7"",""HighValue"":null,""LowValue"":67.0},{""Guid"":""902c2340-3f11-46fc-8701-1c5bfa687464"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#91b1d4"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""690d4410-dfb1-448e-804f-d76d9ee11e02"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#b6cbe2"",""HighValue"":33.0,""LowValue"":0.0}]", @"9C17DBBA-820A-4FDE-A425-F52CE6592B69" );
            AddReadWriteSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_EQ_CONSTRUCTS_SELF_REGULATING );

            // Person Attribute "EQ: Others Awareness"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", categories, @"EQ: Others Awareness", @"Others Awareness", @"core_EQOthersAwareness", @"", @"", 2, @"", @"A6AF0BE5-E93A-49EB-AFEA-3520B7C41C78" );
            RockMigrationHelper.AddAttributeQualifier( @"A6AF0BE5-E93A-49EB-AFEA-3520B7C41C78", @"ConfigurationJSON", @"[{""Guid"":""0213bd4e-1187-4990-84d8-79cd9c8046ad"",""RangeIndex"":0,""Label"":""High"",""Color"":""#709ac7"",""HighValue"":null,""LowValue"":67.0},{""Guid"":""b8b10704-f7ca-45f7-b907-5e152c0e6ce1"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#91b1d4"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""8d6f3051-e640-4268-a3d8-f990676e74b7"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#b6cbe2"",""HighValue"":33.0,""LowValue"":0.0}]", @"99A8D4E4-9D48-48C4-9661-6AB1AAF2279F" );
            AddReadWriteSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_EQ_CONSTRUCTS_OTHERS_AWARENESS );

            // Person Attribute "EQ: Others Regulating"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", categories, @"EQ: Others Regulating", @"Others Regulating", @"core_EQOthersRegulating", @"", @"", 3, @"", @"129C108E-CE61-4DFB-A9A8-1EBC3462022E" );
            RockMigrationHelper.AddAttributeQualifier( @"129C108E-CE61-4DFB-A9A8-1EBC3462022E", @"ConfigurationJSON", @"[{""Guid"":""d7eaa798-e069-4216-a07f-e652173730d8"",""RangeIndex"":0,""Label"":""High"",""Color"":""#709ac7"",""HighValue"":null,""LowValue"":67.0},{""Guid"":""dc01f800-204a-487a-8014-19c2b992cfbe"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#91b1d4"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""017f9b50-943a-4707-9547-b4648759ba75"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#b6cbe2"",""HighValue"":33.0,""LowValue"":0.0}]", @"AED8EA0E-CD43-47C8-8984-92790BC7BCDF" );
            AddReadWriteSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_EQ_CONSTRUCTS_OTHERS_REGULATING );

            // Person Attribute "EQ: Problem Solving"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", categories, @"EQ: Problem Solving", @"Problem Solving", @"core_EQProblemSolving", @"", @"", 4, @"", @"B598BF9C-7A0C-467E-B467-13B40DAC9F8D" );
            RockMigrationHelper.AddAttributeQualifier( @"B598BF9C-7A0C-467E-B467-13B40DAC9F8D", @"ConfigurationJSON", @"[{""Guid"":""c185d175-cd75-4c63-a5b2-0e7ab0f804a2"",""RangeIndex"":0,""Label"":""High"",""Color"":""#709ac7"",""HighValue"":null,""LowValue"":67.0},{""Guid"":""becd7784-307e-4ecd-be95-e6dacdeba53a"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#91b1d4"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""593cf5b9-315c-4216-8ef0-8bd17bdbadde"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#b6cbe2"",""HighValue"":33.0,""LowValue"":0.0}]", @"8AFD4B98-09F3-42EE-8D7C-81B89D543A43" );
            AddReadWriteSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_EQ_SCALES_PROBLEM_SOLVING );

            // Person Attribute "EQ: Under Stress"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", categories, @"EQ: Under Stress", @"Under Stress", @"core_EQUnderStress", @"", @"", 5, @"", @"C3CB8FB5-34A2-48C8-B1FC-7CEBA670C1ED" );
            RockMigrationHelper.AddAttributeQualifier( @"C3CB8FB5-34A2-48C8-B1FC-7CEBA670C1ED", @"ConfigurationJSON", @"[{""Guid"":""908a1346-378f-4cad-b032-ef419cc9bc1d"",""RangeIndex"":0,""Label"":""High"",""Color"":""#709ac7"",""HighValue"":null,""LowValue"":67.0},{""Guid"":""5dfeac9f-7060-4203-b345-bc5ecec16f31"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#91b1d4"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""2ed1ac01-ca3b-44d6-927f-a8e2b878a4e3"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#b6cbe2"",""HighValue"":33.0,""LowValue"":0.0}]", @"3064309F-8011-41A3-A89C-43F2FB612857" );
            AddReadWriteSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_EQ_SCALES_UNDER_STRESS );
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
{{ 'Global' | Attribute:'EmailFooter' }}", "41FF4269-7B48-40CD-81D4-C11370A13DED" );
        }

        private void AssessmentRemindersServiceJobUp()
        {
            // add ServiceJob: Send Assessment Reminders
            // Code Generated using Rock\Dev Tools\Sql\CodeGen_ServiceJobWithAttributes_ForAJob.sql
            Sql( @"IF NOT EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.SendAssessmentReminders' AND [Guid] = 'E3F48F24-E9FC-4A93-95B5-DE7BEDB95B99' )
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
            RockMigrationHelper.DeleteAttribute( "635122EA-3694-44A2-B7C3-4C4D19F9873C" ); // Rock.Jobs.SendAssessmentReminders: Reminder Every
            RockMigrationHelper.DeleteAttribute( "F4312D3D-26B6-41C7-9842-0CDD319C747C" ); // Rock.Jobs.SendAssessmentReminders: Cut off Days
            RockMigrationHelper.DeleteAttribute( "4BF49C30-ED3B-41AF-AF05-BDE2BA2C0056" ); // Rock.Jobs.SendAssessmentReminders: Assessment Reminder System Email

            // remove ServiceJob: Send Assessment Reminders
            Sql( @"IF EXISTS( SELECT [Id] FROM [ServiceJob] WHERE [Class] = 'Rock.Jobs.SendAssessmentReminders' AND [Guid] = 'E3F48F24-E9FC-4A93-95B5-DE7BEDB95B99' )
            BEGIN
               DELETE [ServiceJob]  WHERE [Guid] = 'E3F48F24-E9FC-4A93-95B5-DE7BEDB95B99';
            END" );
        }

        private void CreateRequestAssessmentWorkflow()
        {
            #region FieldTypes

            RockMigrationHelper.UpdateFieldType( "Assessment Types", "", "Rock", "Rock.Field.Types.AssessmentTypesFieldType", "C263513A-30BE-4823-ABF1-AC12A56F9644" );

            #endregion FieldTypes

            #region EntityTypes

            RockMigrationHelper.UpdateEntityType( "Rock.Model.Workflow", "3540E9A7-FE30-43A9-8B0A-A372B63DFC93", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActivity", "2CB52ED0-CB06-4D62-9E2C-73B60AFA4C9F", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Model.WorkflowActionType", "23E3273A-B137-48A3-9AFF-C8DC832DDCA6", true, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.CompleteWorkflow", "EEDA4318-F014-4A46-9C76-4C052EF81AA1", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.CreateAssessmentRequest", "7EDCCA06-C539-4B5B-B6E4-400A19655898", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.RunSQL", "A41216D6-6FB0-4019-B222-2C29B4519CF4", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SendEmail", "66197B01-D1F0-4924-A315-47AD54E030DE", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SendSystemEmail", "4487702A-BEAF-4E5A-92AD-71A1AD48DFCE", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeFromEntity", "972F19B9-598B-474B-97A4-50E56E7B59D2", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeToCurrentPerson", "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.SetAttributeValue", "C789E457-0783-44B3-9D8F-2EBAB5F11110", false, true );
            RockMigrationHelper.UpdateEntityType( "Rock.Workflow.Action.UserEntryForm", "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", false, true );
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "DE9CB292-4785-4EA3-976D-3826F91E9E98" ); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Person Attribute", "PersonAttribute", "The attribute to set to the currently logged in person.", 0, @"", "BBED8A83-8BB2-4D35-BAFB-05F67DCAD112" ); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Person Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "89E9BCED-91AB-47B0-AD52-D78B0B7CB9E8" ); // Rock.Workflow.Action.SetAttributeToCurrentPerson:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "4487702A-BEAF-4E5A-92AD-71A1AD48DFCE", "08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF", "System Email", "SystemEmail", "A system email to send.", 0, @"", "C879B8B4-574C-4BCE-BC4D-0C7245AF19D4" ); // Rock.Workflow.Action.SendSystemEmail:System Email
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "4487702A-BEAF-4E5A-92AD-71A1AD48DFCE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "BD6978CE-EDBF-45A9-A548-96630DFF52C1" ); // Rock.Workflow.Action.SendSystemEmail:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "4487702A-BEAF-4E5A-92AD-71A1AD48DFCE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Save Communication History", "SaveCommunicationHistory", "Should a record of this communication be saved to the recipient's profile", 2, @"False", "9C5436E6-7EF2-4BD4-B87A-D3E980E55DE3" ); // Rock.Workflow.Action.SendSystemEmail:Save Communication History
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "4487702A-BEAF-4E5A-92AD-71A1AD48DFCE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Send To Email Addresses|Attribute Value", "Recipient", "The email addresses or an attribute that contains the person or email address that email should be sent to. <span class='tip tip-lava'></span>", 1, @"", "E58E9280-77CF-4DBB-BF66-87F29D0BF707" ); // Rock.Workflow.Action.SendSystemEmail:Send To Email Addresses|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "4487702A-BEAF-4E5A-92AD-71A1AD48DFCE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "A52C2EBD-D1CC-469F-803C-FF4C5326D456" ); // Rock.Workflow.Action.SendSystemEmail:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE" ); // Rock.Workflow.Action.UserEntryForm:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "C178113D-7C86-4229-8424-C6D0CF4A7E23" ); // Rock.Workflow.Action.UserEntryForm:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Body", "Body", "The body of the email that should be sent. <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 4, @"", "4D245B9E-6B03-46E7-8482-A51FBA190E4D" ); // Rock.Workflow.Action.SendEmail:Body
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "36197160-7D3D-490D-AB42-7E29105AFE91" ); // Rock.Workflow.Action.SendEmail:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Save Communication History", "SaveCommunicationHistory", "Should a record of this communication be saved to the recipient's profile", 8, @"False", "65E69B78-37D8-4A88-B8AC-71893D2F75EF" ); // Rock.Workflow.Action.SendEmail:Save Communication History
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment One", "AttachmentOne", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 5, @"", "C2C7DA55-3018-4645-B9EE-4BCD11855F2C" ); // Rock.Workflow.Action.SendEmail:Attachment One
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment Three", "AttachmentThree", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 7, @"", "A059767A-5592-4926-948A-1065AF4E9748" ); // Rock.Workflow.Action.SendEmail:Attachment Three
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attachment Two", "AttachmentTwo", "Workflow attribute that contains the email attachment. Note file size that can be sent is limited by both the sending and receiving email services typically 10 - 25 MB.", 6, @"", "FFD9193A-451F-40E6-9776-74D5DCAC1450" ); // Rock.Workflow.Action.SendEmail:Attachment Two
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Send to Group Role", "GroupRole", "An optional Group Role attribute to limit recipients to if the 'Send to Email Address' is a group or security role.", 2, @"", "D43C2686-7E02-4A70-8D99-3BCD8ECAFB2F" ); // Rock.Workflow.Action.SendEmail:Send to Group Role
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "From Email Address|Attribute Value", "From", "The email address or an attribute that contains the person or email address that email should be sent from (will default to organization email). <span class='tip tip-lava'></span>", 0, @"", "9F5F7CEC-F369-4FDF-802A-99074CE7A7FC" ); // Rock.Workflow.Action.SendEmail:From Email Address|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Send To Email Addresses|Attribute Value", "To", "The email addresses or an attribute that contains the person or email address that email should be sent to. <span class='tip tip-lava'></span>", 1, @"", "0C4C13B8-7076-4872-925A-F950886B5E16" ); // Rock.Workflow.Action.SendEmail:Send To Email Addresses|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Subject", "Subject", "The subject that should be used when sending email. <span class='tip tip-lava'></span>", 3, @"", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386" ); // Rock.Workflow.Action.SendEmail:Subject
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "66197B01-D1F0-4924-A315-47AD54E030DE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "D1269254-C15A-40BD-B784-ADCC231D3950" ); // Rock.Workflow.Action.SendEmail:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "7EDCCA06-C539-4B5B-B6E4-400A19655898", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "D686BDFF-03C8-4F7C-A3FC-89C42DF74714" ); // Rock.Workflow.Action.CreateAssessmentRequest:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "7EDCCA06-C539-4B5B-B6E4-400A19655898", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Assessment Types", "AssessmentTypesKey", "The attribute that contains the selected list of assessments being requested.", 0, @"", "B672E4D0-14DE-424A-BC38-7A91F5385A18" ); // Rock.Workflow.Action.CreateAssessmentRequest:Assessment Types
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "7EDCCA06-C539-4B5B-B6E4-400A19655898", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Due Date", "DueDate", "The attribute that contains the Due Date (if any) for the requests.", 2, @"", "1010F5DA-565B-4A86-B5C6-E5CE4C26F330" ); // Rock.Workflow.Action.CreateAssessmentRequest:Due Date
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "7EDCCA06-C539-4B5B-B6E4-400A19655898", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Person", "Person", "The attribute containing the person being requested to take the assessment(s).", 1, @"", "9E2360BE-4C22-4817-8D2B-5796426D6192" ); // Rock.Workflow.Action.CreateAssessmentRequest:Person
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "7EDCCA06-C539-4B5B-B6E4-400A19655898", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Requested By", "RequestedBy", "The attribute containing the person requesting the test be taken.", 2, @"", "5494809A-B0CC-44D9-BFD9-B60D514D020F" ); // Rock.Workflow.Action.CreateAssessmentRequest:Requested By
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "7EDCCA06-C539-4B5B-B6E4-400A19655898", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "46FB6CFC-28A3-4822-94BB-B01B8F2D5ED3" ); // Rock.Workflow.Action.CreateAssessmentRequest:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "By default this action will set the attribute value equal to the guid (or id) of the entity that was passed in for processing. If you include a lava template here, the action will instead set the attribute value to the output of this template. The mergefield to use for the entity is 'Entity.' For example, use {{ Entity.Name }} if the entity has a Name property. <span class='tip tip-lava'></span>", 4, @"", "00D8331D-3055-4531-B374-6B98A9A71D70" ); // Rock.Workflow.Action.SetAttributeFromEntity:Lava Template
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1" ); // Rock.Workflow.Action.SetAttributeFromEntity:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Entity Is Required", "EntityIsRequired", "Should an error be returned if the entity is missing or not a valid entity type?", 2, @"True", "DDEFB300-0A4F-4086-99BE-A32761928F5E" ); // Rock.Workflow.Action.SetAttributeFromEntity:Entity Is Required
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Id instead of Guid", "UseId", "Most entity attribute field types expect the Guid of the entity (which is used by default). Select this option if the entity's Id should be used instead (should be rare).", 3, @"False", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B" ); // Rock.Workflow.Action.SetAttributeFromEntity:Use Id instead of Guid
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to set the value of.", 1, @"", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7" ); // Rock.Workflow.Action.SetAttributeFromEntity:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "972F19B9-598B-474B-97A4-50E56E7B59D2", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "AD4EFAC4-E687-43DF-832F-0DC3856ABABB" ); // Rock.Workflow.Action.SetAttributeFromEntity:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "SQLQuery", "SQLQuery", "The SQL query to run. <span class='tip tip-lava'></span>", 0, @"", "F3B9908B-096F-460B-8320-122CF046D1F9" ); // Rock.Workflow.Action.RunSQL:SQLQuery
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "A18C3143-0586-4565-9F36-E603BC674B4E" ); // Rock.Workflow.Action.RunSQL:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Continue On Error", "ContinueOnError", "Should processing continue even if SQL Error occurs?", 3, @"False", "9A567F6A-2A77-4ECD-80F7-BBD7D54E843C" ); // Rock.Workflow.Action.RunSQL:Continue On Error
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Result Attribute", "ResultAttribute", "An optional attribute to set to the scaler result of SQL query.", 2, @"", "56997192-2545-4EA1-B5B2-313B04588984" ); // Rock.Workflow.Action.RunSQL:Result Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "73B02051-0D38-4AD9-BF81-A2D477DE4F70", "Parameters", "Parameters", "The parameters to supply to the SQL query. <span class='tip tip-lava'></span>", 1, @"", "EA9A026A-934F-4920-97B1-9734795127ED" ); // Rock.Workflow.Action.RunSQL:Parameters
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "A41216D6-6FB0-4019-B222-2C29B4519CF4", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "FA7C685D-8636-41EF-9998-90FFF3998F76" ); // Rock.Workflow.Action.RunSQL:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "D7EAA859-F500-4521-9523-488B12EAA7D2" ); // Rock.Workflow.Action.SetAttributeValue:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Attribute", "Attribute", "The attribute to set the value of.", 0, @"", "44A0B977-4730-4519-8FF6-B0A01A95B212" ); // Rock.Workflow.Action.SetAttributeValue:Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Text Value|Attribute Value", "Value", "The text or attribute to set the value from. <span class='tip tip-lava'></span>", 1, @"", "E5272B11-A2B8-49DC-860D-8D574E2BC15C" ); // Rock.Workflow.Action.SetAttributeValue:Text Value|Attribute Value
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C789E457-0783-44B3-9D8F-2EBAB5F11110", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "57093B41-50ED-48E5-B72B-8829E62704C8" ); // Rock.Workflow.Action.SetAttributeValue:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C" ); // Rock.Workflow.Action.CompleteWorkflow:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20", "Status|Status Attribute", "Status", "The status to set the workflow to when marking the workflow complete. <span class='tip tip-lava'></span>", 0, @"Completed", "07CB7DBC-236D-4D38-92A4-47EE448BA89A" ); // Rock.Workflow.Action.CompleteWorkflow:Status|Status Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "EEDA4318-F014-4A46-9C76-4C052EF81AA1", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "25CAD4BE-5A00-409D-9BAB-E32518D89956" ); // Rock.Workflow.Action.CompleteWorkflow:Order

            #endregion

            #region Categories

            RockMigrationHelper.UpdateCategory( "C9F3C4A5-1526-474D-803F-D6C7A45CBBAE", "Data Integrity", "fa fa-magic", "", "BBAE05FD-8192-4616-A71E-903A927E0D90", 0 ); // Data Integrity

            #endregion

            #region Request Assessment

            RockMigrationHelper.UpdateWorkflowType( false, true, "Request Assessment", "", "BBAE05FD-8192-4616-A71E-903A927E0D90", "Work", "icon-fw fa fa-bar-chart", 0, true, 0, "31DDC001-C91A-4418-B375-CAB1475F7A62", 0 ); // Request Assessment
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "31DDC001-C91A-4418-B375-CAB1475F7A62", "C263513A-30BE-4823-ABF1-AC12A56F9644", "Assessments To Take", "AssessmentsToTake", "", 0, @"", "69E8513A-D9E4-4C98-B938-48B1B24F9C08", false ); // Request Assessment:Assessments To Take
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "31DDC001-C91A-4418-B375-CAB1475F7A62", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Person", "Person", "The person who will take the assessments", 1, @"", "A201EB57-0AD0-4B98-AD44-9D3A7C0F16BA", false ); // Request Assessment:Person
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "31DDC001-C91A-4418-B375-CAB1475F7A62", "E4EAB7B2-0B76-429B-AFE4-AD86D7428C70", "Requested By", "RequestedBy", "The person who requested the assessments.", 2, @"", "66B8DEC5-1B55-4AD1-8E4B-C719279A1947", false ); // Request Assessment:Requested By
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "31DDC001-C91A-4418-B375-CAB1475F7A62", "6B6AA175-4758-453F-8D83-FCD8044B5F36", "Due Date", "DueDate", "When all the selected assessments should be completed.", 3, @"", "7FB54D8C-B6FC-4864-9F14-EEC155CF6D4C", false ); // Request Assessment:Due Date
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "31DDC001-C91A-4418-B375-CAB1475F7A62", "9C204CD0-1233-41C5-818A-C5DA439445AA", "No Email Warning", "NoEmailWarning", "Warning message when the person does not have an email address.", 4, @"", "B13D6F19-1436-4689-B644-FB70805C255B", false ); // Request Assessment:No Email Warning
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "31DDC001-C91A-4418-B375-CAB1475F7A62", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Custom Message", "CustomMessage", "A custom message you would like to include in the request.  Otherwise the default will be used.", 5, @"We're each a unique creation. We'd love to learn more about you through a simple and quick online personality profile. Your results will help us tailor our ministry to you and can also be used for building healthier teams and groups.", "DBFB3F53-7AE1-4923-A286-3D69B60BA639", false ); // Request Assessment:Custom Message
            RockMigrationHelper.AddAttributeQualifier( "69E8513A-D9E4-4C98-B938-48B1B24F9C08", "includeInactive", @"False", "8FD003E0-9FBD-407F-86A8-FC72E5BAE552" ); // Request Assessment:Assessments To Take:includeInactive
            RockMigrationHelper.AddAttributeQualifier( "69E8513A-D9E4-4C98-B938-48B1B24F9C08", "repeatColumns", @"", "B0FDE78C-7E2D-42B2-857E-FF96D3AE1C3C" ); // Request Assessment:Assessments To Take:repeatColumns
            RockMigrationHelper.AddAttributeQualifier( "A201EB57-0AD0-4B98-AD44-9D3A7C0F16BA", "EnableSelfSelection", @"False", "80BC8D0A-A585-457E-89C9-FB171C464060" ); // Request Assessment:Person:EnableSelfSelection
            RockMigrationHelper.AddAttributeQualifier( "66B8DEC5-1B55-4AD1-8E4B-C719279A1947", "EnableSelfSelection", @"False", "0E1769ED-811C-49A2-BE9C-E7A1D6772F3B" ); // Request Assessment:Requested By:EnableSelfSelection
            RockMigrationHelper.AddAttributeQualifier( "7FB54D8C-B6FC-4864-9F14-EEC155CF6D4C", "datePickerControlType", @"Date Picker", "D47E8D70-47E1-46B1-B319-0C0BFBF5CD62" ); // Request Assessment:Due Date:datePickerControlType
            RockMigrationHelper.AddAttributeQualifier( "7FB54D8C-B6FC-4864-9F14-EEC155CF6D4C", "displayCurrentOption", @"False", "7F4653B4-1155-4BF4-A1B6-1F3B0B6F49A5" ); // Request Assessment:Due Date:displayCurrentOption
            RockMigrationHelper.AddAttributeQualifier( "7FB54D8C-B6FC-4864-9F14-EEC155CF6D4C", "displayDiff", @"False", "228A9BD3-1BE5-4873-92D3-AB44591F2414" ); // Request Assessment:Due Date:displayDiff
            RockMigrationHelper.AddAttributeQualifier( "7FB54D8C-B6FC-4864-9F14-EEC155CF6D4C", "format", @"", "2ECC60A7-6D8C-4268-BC37-3C6A85E9F182" ); // Request Assessment:Due Date:format
            RockMigrationHelper.AddAttributeQualifier( "7FB54D8C-B6FC-4864-9F14-EEC155CF6D4C", "futureYearCount", @"", "0EFFF1E6-D874-4F00-BB90-1E49EA5B0411" ); // Request Assessment:Due Date:futureYearCount
            RockMigrationHelper.AddAttributeQualifier( "B13D6F19-1436-4689-B644-FB70805C255B", "ispassword", @"False", "5D5AA32F-C81E-4A26-9C25-ADA8653A9A47" ); // Request Assessment:No Email Warning:ispassword
            RockMigrationHelper.AddAttributeQualifier( "B13D6F19-1436-4689-B644-FB70805C255B", "maxcharacters", @"", "836050D6-3053-47EA-9E53-59AE03D967C9" ); // Request Assessment:No Email Warning:maxcharacters
            RockMigrationHelper.AddAttributeQualifier( "B13D6F19-1436-4689-B644-FB70805C255B", "showcountdown", @"False", "D6933BFF-E9A9-45EF-B59D-0CFCAEC331E3" ); // Request Assessment:No Email Warning:showcountdown
            RockMigrationHelper.AddAttributeQualifier( "DBFB3F53-7AE1-4923-A286-3D69B60BA639", "allowhtml", @"False", "6E365CCA-06EF-4165-925B-3EFB94F5472B" ); // Request Assessment:Custom Message:allowhtml
            RockMigrationHelper.AddAttributeQualifier( "DBFB3F53-7AE1-4923-A286-3D69B60BA639", "maxcharacters", @"", "11E73FDC-AD71-4C62-8CB2-F380D3E153BC" ); // Request Assessment:Custom Message:maxcharacters
            RockMigrationHelper.AddAttributeQualifier( "DBFB3F53-7AE1-4923-A286-3D69B60BA639", "numberofrows", @"6", "24411F53-15A6-4E70-8BD6-13CC6FB2A7C8" ); // Request Assessment:Custom Message:numberofrows
            RockMigrationHelper.AddAttributeQualifier( "DBFB3F53-7AE1-4923-A286-3D69B60BA639", "showcountdown", @"False", "C04C3BCB-B37C-4120-A5F1-188BEBE81C5D" ); // Request Assessment:Custom Message:showcountdown
            RockMigrationHelper.UpdateWorkflowActivityType( "31DDC001-C91A-4418-B375-CAB1475F7A62", true, "Launch From Person Profile", "When this workflow is initiated from the Person Profile page, the \"Entity\" will have a value so the first action will run successfully, and the workflow will then be persisted.", true, 0, "41C1D8A6-570C-49D2-A818-08F631FCDBAD" ); // Request Assessment:Launch From Person Profile
            RockMigrationHelper.UpdateWorkflowActivityType( "31DDC001-C91A-4418-B375-CAB1475F7A62", true, "Save And Send", "", false, 1, "88373EA3-CF09-4B5C-8582-73F331CD1FB4" ); // Request Assessment:Save And Send
            RockMigrationHelper.UpdateWorkflowActionForm( @"{{ Workflow | Attribute:'NoEmailWarning' }}

Assign assessments to {{ Workflow | Attribute: 'Person' }}.", @"", "Send^fdc397cd-8b4a-436e-bea1-bce2e6717c03^88373EA3-CF09-4B5C-8582-73F331CD1FB4^Assessment(s) have been sent successfully.|", "88C7D1CC-3478-4562-A301-AE7D4D7FFF6D", true, "", "A56DA6B0-60A1-4998-B3F0-6BFA6F342167" ); // Request Assessment:Launch From Person Profile:User Entry
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "A56DA6B0-60A1-4998-B3F0-6BFA6F342167", "69E8513A-D9E4-4C98-B938-48B1B24F9C08", 0, true, false, true, false, @"", @"", "86E38BD3-21C8-493F-8A5C-FBAEEBB9AEE8" ); // Request Assessment:Launch From Person Profile:User Entry:Assessments To Take
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "A56DA6B0-60A1-4998-B3F0-6BFA6F342167", "A201EB57-0AD0-4B98-AD44-9D3A7C0F16BA", 3, false, true, false, false, @"", @"", "8608396B-E634-4E5C-91C7-DECABC22CD56" ); // Request Assessment:Launch From Person Profile:User Entry:Person
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "A56DA6B0-60A1-4998-B3F0-6BFA6F342167", "66B8DEC5-1B55-4AD1-8E4B-C719279A1947", 4, false, true, false, false, @"", @"", "961BF92D-6FCB-461F-BA9A-41E7D0CE2205" ); // Request Assessment:Launch From Person Profile:User Entry:Requested By
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "A56DA6B0-60A1-4998-B3F0-6BFA6F342167", "7FB54D8C-B6FC-4864-9F14-EEC155CF6D4C", 2, true, false, false, false, @"", @"", "FF867549-A68F-45A7-8495-4AA102FA586B" ); // Request Assessment:Launch From Person Profile:User Entry:Due Date
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "A56DA6B0-60A1-4998-B3F0-6BFA6F342167", "B13D6F19-1436-4689-B644-FB70805C255B", 5, false, true, false, false, @"", @"", "DA087B2B-986B-4721-A712-A013D403F357" ); // Request Assessment:Launch From Person Profile:User Entry:No Email Warning
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "A56DA6B0-60A1-4998-B3F0-6BFA6F342167", "DBFB3F53-7AE1-4923-A286-3D69B60BA639", 1, true, false, false, false, @"", @"", "18048430-8A97-40CC-8186-CA0C16A912EC" ); // Request Assessment:Launch From Person Profile:User Entry:Custom Message
            RockMigrationHelper.UpdateWorkflowActionType( "41C1D8A6-570C-49D2-A818-08F631FCDBAD", "Set Person", 0, "972F19B9-598B-474B-97A4-50E56E7B59D2", true, false, "", "", 1, "", "B5ED3032-8B0B-4ADC-A2A1-522F8C0086CF" ); // Request Assessment:Launch From Person Profile:Set Person
            RockMigrationHelper.UpdateWorkflowActionType( "41C1D8A6-570C-49D2-A818-08F631FCDBAD", "Set Requested By", 1, "24B7D5E6-C30F-48F4-9D7E-AF45A342CF3A", true, false, "", "", 1, "", "6A48CC08-D9AE-4508-A270-FDC7343F461B" ); // Request Assessment:Launch From Person Profile:Set Requested By
            RockMigrationHelper.UpdateWorkflowActionType( "41C1D8A6-570C-49D2-A818-08F631FCDBAD", "Set No Email Warning", 2, "A41216D6-6FB0-4019-B222-2C29B4519CF4", true, false, "", "", 1, "", "D947CBE0-437A-4FCE-8898-0141CC03ACEC" ); // Request Assessment:Launch From Person Profile:Set No Email Warning
            RockMigrationHelper.UpdateWorkflowActionType( "41C1D8A6-570C-49D2-A818-08F631FCDBAD", "Set No Email Warning Message", 3, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "B13D6F19-1436-4689-B644-FB70805C255B", 1, "False", "215FC2CD-AC85-46C7-9B03-826DDF72923D" ); // Request Assessment:Launch From Person Profile:Set No Email Warning Message
            RockMigrationHelper.UpdateWorkflowActionType( "41C1D8A6-570C-49D2-A818-08F631FCDBAD", "User Entry", 4, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, true, "A56DA6B0-60A1-4998-B3F0-6BFA6F342167", "", 1, "", "E9C27804-A31D-4121-A963-9F52BEAE7404" ); // Request Assessment:Launch From Person Profile:User Entry
            RockMigrationHelper.UpdateWorkflowActionType( "88373EA3-CF09-4B5C-8582-73F331CD1FB4", "Save Assessment Requests", 0, "7EDCCA06-C539-4B5B-B6E4-400A19655898", true, false, "", "", 1, "", "E871D1D7-04DB-472D-9DF4-6C0389FE2FFC" ); // Request Assessment:Save And Send:Save Assessment Requests
            RockMigrationHelper.UpdateWorkflowActionType( "88373EA3-CF09-4B5C-8582-73F331CD1FB4", "Send Assessment System Email", 1, "4487702A-BEAF-4E5A-92AD-71A1AD48DFCE", true, false, "", "DBFB3F53-7AE1-4923-A286-3D69B60BA639", 32, "", "41E75458-C86C-430C-B7C8-4189419D69C6" ); // Request Assessment:Save And Send:Send Assessment System Email
            RockMigrationHelper.UpdateWorkflowActionType( "88373EA3-CF09-4B5C-8582-73F331CD1FB4", "Send Assessment Custom Message Email", 2, "66197B01-D1F0-4924-A315-47AD54E030DE", true, false, "", "DBFB3F53-7AE1-4923-A286-3D69B60BA639", 64, "", "C8DE19DB-106A-4AFA-9069-C420B45B976C" ); // Request Assessment:Save And Send:Send Assessment Custom Message Email
            RockMigrationHelper.UpdateWorkflowActionType( "88373EA3-CF09-4B5C-8582-73F331CD1FB4", "Workflow Complete", 3, "EEDA4318-F014-4A46-9C76-4C052EF81AA1", true, true, "", "", 1, "", "8F2AC810-83E1-48D2-B306-8F396539343D" ); // Request Assessment:Save And Send:Workflow Complete
            RockMigrationHelper.AddActionTypeAttributeValue( "B5ED3032-8B0B-4ADC-A2A1-522F8C0086CF", "9392E3D7-A28B-4CD8-8B03-5E147B102EF1", @"False" ); // Request Assessment:Launch From Person Profile:Set Person:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "B5ED3032-8B0B-4ADC-A2A1-522F8C0086CF", "61E6E1BC-E657-4F00-B2E9-769AAA25B9F7", @"a201eb57-0ad0-4b98-ad44-9d3a7c0f16ba" ); // Request Assessment:Launch From Person Profile:Set Person:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "B5ED3032-8B0B-4ADC-A2A1-522F8C0086CF", "DDEFB300-0A4F-4086-99BE-A32761928F5E", @"True" ); // Request Assessment:Launch From Person Profile:Set Person:Entity Is Required
            RockMigrationHelper.AddActionTypeAttributeValue( "B5ED3032-8B0B-4ADC-A2A1-522F8C0086CF", "1246C53A-FD92-4E08-ABDE-9A6C37E70C7B", @"False" ); // Request Assessment:Launch From Person Profile:Set Person:Use Id instead of Guid
            RockMigrationHelper.AddActionTypeAttributeValue( "6A48CC08-D9AE-4508-A270-FDC7343F461B", "DE9CB292-4785-4EA3-976D-3826F91E9E98", @"False" ); // Request Assessment:Launch From Person Profile:Set Requested By:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "6A48CC08-D9AE-4508-A270-FDC7343F461B", "BBED8A83-8BB2-4D35-BAFB-05F67DCAD112", @"66b8dec5-1b55-4ad1-8e4b-c719279a1947" ); // Request Assessment:Launch From Person Profile:Set Requested By:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "D947CBE0-437A-4FCE-8898-0141CC03ACEC", "F3B9908B-096F-460B-8320-122CF046D1F9", @"DECLARE @PersonAliasGuid uniqueidentifier = '{{ Workflow | Attribute:'Person','RawValue' }}'

SELECT  CASE
    WHEN EXISTS ( SELECT 1
        FROM [Person] P
        INNER JOIN [PersonAlias] PA ON PA.[Guid] = @PersonAliasGuid AND P.[Id] = PA.[PersonId]
        WHERE P.[IsEmailActive] <> 0 AND P.[Email] IS NOT NULL AND P.[Email] != '' )
    THEN ''
    ELSE 'False'
    END" ); // Request Assessment:Launch From Person Profile:Set No Email Warning:SQLQuery
            RockMigrationHelper.AddActionTypeAttributeValue( "D947CBE0-437A-4FCE-8898-0141CC03ACEC", "A18C3143-0586-4565-9F36-E603BC674B4E", @"False" ); // Request Assessment:Launch From Person Profile:Set No Email Warning:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "D947CBE0-437A-4FCE-8898-0141CC03ACEC", "56997192-2545-4EA1-B5B2-313B04588984", @"b13d6f19-1436-4689-b644-fb70805c255b" ); // Request Assessment:Launch From Person Profile:Set No Email Warning:Result Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "D947CBE0-437A-4FCE-8898-0141CC03ACEC", "9A567F6A-2A77-4ECD-80F7-BBD7D54E843C", @"False" ); // Request Assessment:Launch From Person Profile:Set No Email Warning:Continue On Error
            RockMigrationHelper.AddActionTypeAttributeValue( "215FC2CD-AC85-46C7-9B03-826DDF72923D", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // Request Assessment:Launch From Person Profile:Set No Email Warning Message:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "215FC2CD-AC85-46C7-9B03-826DDF72923D", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"b13d6f19-1436-4689-b644-fb70805c255b" ); // Request Assessment:Launch From Person Profile:Set No Email Warning Message:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "215FC2CD-AC85-46C7-9B03-826DDF72923D", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"<div class=""alert alert-warning margin-t-sm"">{{ Workflow | Attribute:'Person' }} does not have an active email address. Please add an address to their record.</div>" ); // Request Assessment:Launch From Person Profile:Set No Email Warning Message:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "E9C27804-A31D-4121-A963-9F52BEAE7404", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // Request Assessment:Launch From Person Profile:User Entry:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "E871D1D7-04DB-472D-9DF4-6C0389FE2FFC", "D686BDFF-03C8-4F7C-A3FC-89C42DF74714", @"False" ); // Request Assessment:Save And Send:Save Assessment Requests:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "E871D1D7-04DB-472D-9DF4-6C0389FE2FFC", "B672E4D0-14DE-424A-BC38-7A91F5385A18", @"69e8513a-d9e4-4c98-b938-48b1b24f9c08" ); // Request Assessment:Save And Send:Save Assessment Requests:Assessment Types
            RockMigrationHelper.AddActionTypeAttributeValue( "E871D1D7-04DB-472D-9DF4-6C0389FE2FFC", "9E2360BE-4C22-4817-8D2B-5796426D6192", @"a201eb57-0ad0-4b98-ad44-9d3a7c0f16ba" ); // Request Assessment:Save And Send:Save Assessment Requests:Person
            RockMigrationHelper.AddActionTypeAttributeValue( "E871D1D7-04DB-472D-9DF4-6C0389FE2FFC", "5494809A-B0CC-44D9-BFD9-B60D514D020F", @"66b8dec5-1b55-4ad1-8e4b-c719279a1947" ); // Request Assessment:Save And Send:Save Assessment Requests:Requested By
            RockMigrationHelper.AddActionTypeAttributeValue( "E871D1D7-04DB-472D-9DF4-6C0389FE2FFC", "1010F5DA-565B-4A86-B5C6-E5CE4C26F330", @"7fb54d8c-b6fc-4864-9f14-eec155cf6d4c" ); // Request Assessment:Save And Send:Save Assessment Requests:Due Date
            RockMigrationHelper.AddActionTypeAttributeValue( "41E75458-C86C-430C-B7C8-4189419D69C6", "C879B8B4-574C-4BCE-BC4D-0C7245AF19D4", @"41ff4269-7b48-40cd-81d4-c11370a13ded" ); // Request Assessment:Save And Send:Send Assessment System Email:System Email
            RockMigrationHelper.AddActionTypeAttributeValue( "41E75458-C86C-430C-B7C8-4189419D69C6", "BD6978CE-EDBF-45A9-A548-96630DFF52C1", @"False" ); // Request Assessment:Save And Send:Send Assessment System Email:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "41E75458-C86C-430C-B7C8-4189419D69C6", "E58E9280-77CF-4DBB-BF66-87F29D0BF707", @"a201eb57-0ad0-4b98-ad44-9d3a7c0f16ba" ); // Request Assessment:Save And Send:Send Assessment System Email:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "41E75458-C86C-430C-B7C8-4189419D69C6", "9C5436E6-7EF2-4BD4-B87A-D3E980E55DE3", @"False" ); // Request Assessment:Save And Send:Send Assessment System Email:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "C8DE19DB-106A-4AFA-9069-C420B45B976C", "36197160-7D3D-490D-AB42-7E29105AFE91", @"False" ); // Request Assessment:Save And Send:Send Assessment Custom Message Email:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "C8DE19DB-106A-4AFA-9069-C420B45B976C", "0C4C13B8-7076-4872-925A-F950886B5E16", @"a201eb57-0ad0-4b98-ad44-9d3a7c0f16ba" ); // Request Assessment:Save And Send:Send Assessment Custom Message Email:Send To Email Addresses|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "C8DE19DB-106A-4AFA-9069-C420B45B976C", "5D9B13B6-CD96-4C7C-86FA-4512B9D28386", @"Assessments Are Ready To Take" ); // Request Assessment:Save And Send:Send Assessment Custom Message Email:Subject
            RockMigrationHelper.AddActionTypeAttributeValue( "C8DE19DB-106A-4AFA-9069-C420B45B976C", "4D245B9E-6B03-46E7-8482-A51FBA190E4D", @"{% capture assessmentsLink %}{{ 'Global' | Attribute:'PublicApplicationRoot' }}/assessments?{{ Person.ImpersonationParameter }}{% endcapture %}
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
{{ 'Global' | Attribute:'EmailFooter' }}" ); // Request Assessment:Save And Send:Send Assessment Custom Message Email:Body
            RockMigrationHelper.AddActionTypeAttributeValue( "C8DE19DB-106A-4AFA-9069-C420B45B976C", "65E69B78-37D8-4A88-B8AC-71893D2F75EF", @"False" ); // Request Assessment:Save And Send:Send Assessment Custom Message Email:Save Communication History
            RockMigrationHelper.AddActionTypeAttributeValue( "8F2AC810-83E1-48D2-B306-8F396539343D", "0CA0DDEF-48EF-4ABC-9822-A05E225DE26C", @"False" ); // Request Assessment:Save And Send:Workflow Complete:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "8F2AC810-83E1-48D2-B306-8F396539343D", "07CB7DBC-236D-4D38-92A4-47EE448BA89A", @"Completed" ); // Request Assessment:Save And Send:Workflow Complete:Status|Status Attribute

            #endregion

            #region Edit bio block list of workflow actions

            // Add Request Assessment
            Sql( @"
                DECLARE @bioWFActionsAttributeValueId TABLE 
			  (
				Id INT
			  )

			  INSERT INTO @bioWFActionsAttributeValueId
                SELECT v.[Id]
                FROM [dbo].[attribute] a
                JOIN [AttributeValue] v ON a.id = v.AttributeId
                WHERE a.[EntityTypeId] = 9
                    AND a.[EntityTypeQualifierColumn] = 'BlockTypeId'
                    AND a.[Key] = 'WorkflowActions'
                    AND a.[EntityTypeQualifierValue] = (SELECT [Id] FROM [dbo].[BlockType] WHERE [Guid] = '0F5922BB-CD68-40AC-BF3C-4AAB1B98760C')
                    AND v.[Value] NOT LIKE '%31DDC001-C91A-4418-B375-CAB1475F7A62%'

                IF EXISTS (SELECT * FROM @bioWFActionsAttributeValueId)
                BEGIN
                    UPDATE AV
                    SET [Value] = [Value] + ',31DDC001-C91A-4418-B375-CAB1475F7A62'
                    FROM [dbo].[AttributeValue] av
						INNER JOIN @bioWFActionsAttributeValueId b on av.Id = b.Id
                END" );

            // Remove legacy DISC Request
            Sql( @"
				DECLARE @bioWFActionsAttributeValueId TABLE 
				(
					Id INT
				)

				INSERT INTO @bioWFActionsAttributeValueId
                SELECT v.[Id]
                FROM [dbo].[attribute] a
                JOIN [AttributeValue] v ON a.id = v.AttributeId
                WHERE a.[EntityTypeId] = 9
                    AND a.[EntityTypeQualifierColumn] = 'BlockTypeId'
                    AND a.[Key] = 'WorkflowActions'
                    AND a.[EntityTypeQualifierValue] = (SELECT [Id] FROM [dbo].[BlockType] WHERE [Guid] = '0F5922BB-CD68-40AC-BF3C-4AAB1B98760C')
                    AND v.[Value] LIKE '%885CBA61-44EA-4B4A-B6E1-289041B6A195%'

                IF EXISTS (SELECT * FROM @bioWFActionsAttributeValueId)
                BEGIN
                    UPDATE av
                    SET [Value] = REPLACE([Value], ',885CBA61-44EA-4B4A-B6E1-289041B6A195', '')
					FROM [dbo].[AttributeValue] av
						INNER JOIN @bioWFActionsAttributeValueId b ON av.Id = b.Id
                END" );

            #endregion Edit bio block list of workflow actions
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
            RockMigrationHelper.AddPage( true, "EBAA5140-4B8F-44B8-B1E8-C73B654E4B22", "5FEAF34C-7FB6-4A11-8A1E-C452EC7849BD", "Assessments", "", "FCF44690-D74C-4FB7-A01B-0EFCA6EA9E1E", "fa fa-bar-chart" ); // Site:External Website
            RockMigrationHelper.AddSecurityAuthForPage( "FCF44690-D74C-4FB7-A01B-0EFCA6EA9E1E", 1, "View", false, "", 3, "420BADED-31C8-4870-AF12-6E20ABEDB9E7" ); // Page:Assessments
            RockMigrationHelper.AddSecurityAuthForPage( "FCF44690-D74C-4FB7-A01B-0EFCA6EA9E1E", 0, "View", true, "", 2, "E9DD5267-41BD-404D-A064-F1066396E6B9" ); // Page:Assessments
            RockMigrationHelper.AddPageRoute( "FCF44690-D74C-4FB7-A01B-0EFCA6EA9E1E", "Assessments", "F2873F65-617C-4BD3-94E0-48E2408EBDBD" );// for Page:Assessments
            RockMigrationHelper.UpdateBlockType( "Assessment List", "Allows you to view and take any available assessments.", "~/Blocks/Crm/AssessmentList.ascx", "CRM", "0AD1D108-4ABF-4AED-B3B7-4AAEA16D10E4" );
            RockMigrationHelper.AddBlock( true, "FCF44690-D74C-4FB7-A01B-0EFCA6EA9E1E".AsGuid(), null, "F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(), "0AD1D108-4ABF-4AED-B3B7-4AAEA16D10E4".AsGuid(), "Assessment List", "Main", @"", @"", 0, "0E22E6CB-1634-41CA-83EF-4BC7CE52F314" ); // Add Block to Page: Assessments Site: External Website
            RockMigrationHelper.AddBlock( true, "C0854F84-2E8B-479C-A3FB-6B47BE89B795".AsGuid(), null, "F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(), "0AD1D108-4ABF-4AED-B3B7-4AAEA16D10E4".AsGuid(), "Assessment List", "Sidebar1", @"", @"", 2, "37D4A991-9F9A-47CE-9084-04466F166B6A" ); // Add Block to Page: My Account Site: External Website

            // Attrib for BlockType: Assessment List:Only Show Requested
            RockMigrationHelper.UpdateBlockTypeAttribute( "0AD1D108-4ABF-4AED-B3B7-4AAEA16D10E4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Only Show Requested", "OnlyShowRequested", "", @"If enabled, limits the list to show only assessments that have been requested or completed.", 0, @"True", "7A10C446-B0F3-43F0-9FEB-78B689593736" );
            // Attrib for BlockType: Assessment List:Hide If No Active Requests
            RockMigrationHelper.UpdateBlockTypeAttribute( "0AD1D108-4ABF-4AED-B3B7-4AAEA16D10E4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide If No Active Requests", "HideIfNoActiveRequests", "", @"If enabled, nothing will be shown if there are not pending (waiting to be taken) assessment requests.", 1, @"False", "305AD0A5-6E35-402A-A6A2-50474733368A" );
            // Attrib for BlockType: Assessment List:Hide If No Requests
            RockMigrationHelper.UpdateBlockTypeAttribute( "0AD1D108-4ABF-4AED-B3B7-4AAEA16D10E4", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Hide If No Requests", "HideIfNoRequests", "", @"If enabled, nothing will be shown where there are no requests (pending or completed).", 2, @"False", "1E5EE52F-DFD5-4406-A517-4B76E2800D2A" );

            #region Attrib for BlockType: Assessment List:Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "0AD1D108-4ABF-4AED-B3B7-4AAEA16D10E4", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", @"The lava template to use to format the entire block.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 3, @"<div class='panel panel-default'>
    <div class='panel-heading'>Assessments</div>
    <div class='panel-body'>
            {% for assessmenttype in AssessmentTypes %}
                {% if assessmenttype.LastRequestObject %}
                    {% if assessmenttype.LastRequestObject.Status == 'Complete' %}
                        <div class='panel panel-success'>
                            <div class='panel-heading'>{{ assessmenttype.Title }}<br />
                                Completed: {{ assessmenttype.LastRequestObject.CompletedDate | Date:'M/d/yyyy'}} <br />
                                <a href='{{ assessmenttype.AssessmentResultsPath}}'>View Results</a>
                                &nbsp;&nbsp;{{ assessmenttype.AssessmentRetakeLinkButton }}
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
</div>", "044D444A-ECDC-4B7A-8987-91577AAB227C" );
            #endregion Attrib for BlockType: Assessment List:Lava Template

            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '87068AAB-16A7-42CC-8A31-5A957D6C4DD5'" );  // Page: My Account,  Zone: Sidebar1,  Block: Actions
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '8C513CAC-FB3F-40A2-A0F6-D4C50FF72EC8'" );  // Page: My Account,  Zone: Sidebar1,  Block: Group List Personalized Lava
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '37D4A991-9F9A-47CE-9084-04466F166B6A'" );  // Page: My Account,  Zone: Sidebar1,  Block: Assessment List
            Sql( @"UPDATE [Block] SET [Order] = 3 WHERE [Guid] = 'E5596525-B176-4753-A337-25F1F9B83FCE'" );  // Page: My Account,  Zone: Sidebar1,  Block: Recent Registrations

            // Attrib Value for Block:Assessment List, Attribute:Only Show Requested Page: My Account, Site: External Website
            RockMigrationHelper.AddBlockAttributeValue( "37D4A991-9F9A-47CE-9084-04466F166B6A", "7A10C446-B0F3-43F0-9FEB-78B689593736", @"False" );
        }

        private void ConflictProfileAssessmentPageBlockAttributes()
        {
            RockMigrationHelper.AddLayout( "F3F82256-2D66-432B-9D67-3552CD2F4C2B", "FullWidthNarrow", "Full Width Narrow", "", "BE15B7BC-6D64-4880-991D-FDE962F91196" ); // Site:External Website

            // Conflict Profile Assessment
            RockMigrationHelper.AddPage( true, "FCF44690-D74C-4FB7-A01B-0EFCA6EA9E1E", "BE15B7BC-6D64-4880-991D-FDE962F91196", "Conflict Profile Assessment", "", "37F17AD8-8103-4F85-865C-94E76B4470BB", "" ); // Site:External Website
            RockMigrationHelper.AddPageRoute( "37F17AD8-8103-4F85-865C-94E76B4470BB", "ConflictProfile", "B843AFE4-9198-49DE-904B-8D6440158DAC" );// for Page:Conflict Profile Assessment
            RockMigrationHelper.AddPageRoute( "37F17AD8-8103-4F85-865C-94E76B4470BB", "ConflictProfile/{rckipid}", "AFD90575-B363-4862-B4A6-1283D5C00AD9" );// for Page:Conflict Profile Assessment
            RockMigrationHelper.UpdateBlockType( "Conflict Profile", "Allows you to take a conflict profile test and saves your conflict profile score.", "~/Blocks/Crm/ConflictProfile.ascx", "CRM", "91473D2F-607D-4260-9C6A-DD3762FE472D" );
            RockMigrationHelper.AddBlock( true, "37F17AD8-8103-4F85-865C-94E76B4470BB".AsGuid(), null, "F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(), "91473D2F-607D-4260-9C6A-DD3762FE472D".AsGuid(), "Conflict Profile", "Main", @"", @"", 0, "D005E292-25F8-45D4-A713-2A5C811F0219" ); // Add Block to Page: Conflict Profile Assessment Site: External Website
            #region Attrib for BlockType: Conflict Profile:Instructions
            RockMigrationHelper.UpdateBlockTypeAttribute( "91473D2F-607D-4260-9C6A-DD3762FE472D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Instructions", "Instructions", "", @"The text (HTML) to display at the top of the instructions section.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 0, @"
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
</p>", "2E455190-2BAE-4E9F-8505-F393BCE52342" );
            #endregion Attrib for BlockType: Conflict Profile:Instructions
            #region Attrib for BlockType: Conflict Profile:Results Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "91473D2F-607D-4260-9C6A-DD3762FE472D", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Results Message", "ResultsMessage", "", @"The text (HTML) to display at the top of the results section.<span class='tip tip-lava'></span><span class='tip tip-html'></span>", 1, @"
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
", "1A855117-6489-4A15-846A-5A99F54E9747" );
            #endregion Attrib for BlockType: Conflict Profile:Results Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "91473D2F-607D-4260-9C6A-DD3762FE472D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Set Page Title", "SetPageTitle", "", @"The text to display as the heading.", 2, @"Conflict Profile", "C5698564-7178-43BA-B4A3-58B13DDC3AF0" ); // Attrib for BlockType: Conflict Profile:Set Page Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "91473D2F-607D-4260-9C6A-DD3762FE472D", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Set Page Icon", "SetPageIcon", "", @"The css class name to use for the heading icon.", 3, @"fa fa-handshake", "D5ABBD1A-61F1-4C48-8AD9-C26AC7F5CAEF" ); // Attrib for BlockType: Conflict Profile:Set Page Icon
            RockMigrationHelper.UpdateBlockTypeAttribute( "91473D2F-607D-4260-9C6A-DD3762FE472D", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Questions", "NumberofQuestions", "", @"The number of questions to show per page while taking the test", 4, @"7", "6CBCA505-E5BA-4FE9-9DD8-7F3C507B12B8" ); // Attrib for BlockType: Conflict Profile:Number of Questions
            RockMigrationHelper.UpdateBlockTypeAttribute( "91473D2F-607D-4260-9C6A-DD3762FE472D", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Retakes", "AllowRetakes", "", @"If enabled, the person can retake the test after the minimum days passes.", 5, @"True", "E3965E46-603C-40E5-AB28-1B53E44561DE" ); // Attrib for BlockType: Conflict Profile:Allow Retakes
        }

        private void EmotionalIntelligenceAssessmentPageBlockAttributes()
        {

            // Emotional Intelligence Assessment
            RockMigrationHelper.AddPage( true, "FCF44690-D74C-4FB7-A01B-0EFCA6EA9E1E", "BE15B7BC-6D64-4880-991D-FDE962F91196", "Emotional Intelligence Assessment", "", "BE5F3984-C25E-47CA-A602-EE1CED99E9AC", "" ); // Site:External Website
            RockMigrationHelper.AddPageRoute( "BE5F3984-C25E-47CA-A602-EE1CED99E9AC", "EQ", "8C5F1CF8-8AC1-4123-B7FD-E57EA36CFBBF" );// for Page:Emotional Intelligence Assessment
            RockMigrationHelper.AddPageRoute( "BE5F3984-C25E-47CA-A602-EE1CED99E9AC", "EQ/{rckipid}", "C97D4D5A-F082-4F2B-A873-71F734B539CC" );// for Page:Emotional Intelligence Assessment
            RockMigrationHelper.UpdateBlockType( "EQ Assessment", "Allows you to take a EQ Inventory test and saves your EQ Inventory score.", "~/Blocks/Crm/EQInventory.ascx", "CRM", "040CFD6D-5155-4BC9-BAEE-A53219A7BECE" );
            RockMigrationHelper.AddBlock( true, "BE5F3984-C25E-47CA-A602-EE1CED99E9AC".AsGuid(), null, "F3F82256-2D66-432B-9D67-3552CD2F4C2B".AsGuid(), "040CFD6D-5155-4BC9-BAEE-A53219A7BECE".AsGuid(), "EQ Assessment", "Main", @"", @"", 0, "71BE6A7A-7D51-4149-AFB1-3307DF04B2DF" ); // Add Block to Page: Emotional Intelligence Assessment Site: External Website
            #region Attrib for BlockType: EQ Assessment:Instructions
            RockMigrationHelper.UpdateBlockTypeAttribute( "040CFD6D-5155-4BC9-BAEE-A53219A7BECE", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Instructions", "Instructions", "", @"The text (HTML) to display at the top of the instructions section.  <span class='tip tip-lava'></span> <span class='tip tip-html'></span>", 0, @"
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
</p>", "6C00C171-E6DC-4027-B587-0AB63AC939E3" );
            #endregion Attrib for BlockType: EQ Assessment:Instructions
            #region Attrib for BlockType: EQ Assessment:Results Message

            RockMigrationHelper.UpdateBlockTypeAttribute( "040CFD6D-5155-4BC9-BAEE-A53219A7BECE", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Results Message", "ResultsMessage", "", @"The text (HTML) to display at the top of the results section.<span class='tip tip-lava'></span><span class='tip tip-html'></span>", 1, @"

{%- assign chartColor = '#709AC7' -%}
{%- assign chartHeight = '100px' -%}

<h1 class='text-center'>EQ Inventory Assessment Results</h1>

<p>
    {{ Person.NickName }}, here are your emotional intelligence results. This is a snapshot
    in time and may change through intentional effort and practice. You will rank high, medium
    or low in each of the following six areas.
</p>

<h3 id='eq-SelfAwareness'>Self Awareness</h3>
<p>
    Self-Awareness is being aware of what emotions you are experiencing and why you
    are experiencing them. This skill is demonstrated in real time. In other words,
    when you are in the midst of a discussion or even a disagreement with someone else,
    ask yourself these questions:
    <ul>
        <li>Are you aware of what emotions you are experiencing?</li>
        <li>Are you aware of why you are experiencing these emotions?</li>
    </ul>
</p>

<!-- Graph -->
{[ chart type:'horizontalBar' legendshow:'false' tooltipshow:'false' chartheight:'{{chartHeight}}' xaxistype:'linearhorizontal0to100' ]}
    [[ dataitem label:'Self Awareness' value:'{{SelfAwareness}}' fillcolor:'{{chartColor}}' ]] [[ enddataitem ]]
{[ endchart ]}

<blockquote>
    Your responses to the items on the Self Awareness scale indicate the score for the
    ability to be aware of your own emotions is equal to or better than {{ SelfAwareness }}%
    of those who completed this instrument.
</blockquote>

<h3 id='eq-selfregulating'>Self-Regulating</h3>
<p>
    Self-Regulating is appropriately expressing your emotions in the context of the relationships
    around you. This doesn’t indicate suppressing emotions; rather the ability to express your
    emotions appropriately. Healthy human beings experience a full range of emotions and these are
    important for family, friends, and co-workers to understand. Self-Regulating is learning to
    tell others what you are feeling in the moment.
</p>

    {[ chart type:'horizontalBar' legendshow:'false' tooltipshow:'false' chartheight:'{{chartHeight}}' xaxistype:'linearhorizontal0to100' ]}
        [[ dataitem label:'Self Regulating' value:'{{SelfRegulating}}' fillcolor:'{{chartColor}}']] [[ enddataitem ]]
    {[ endchart ]}

<blockquote>
    Your responses to the items on the Self Regulation scale indicate the score for the
    ability to appropriately express your own emotions is equal to or better than {{ SelfRegulating }}%
    of those who completed this instrument.
</blockquote>


<h3 id='eq-othersawareness'>Others-Awareness</h3>
<p>
    Others-Awareness is being aware of what emotions others are experiencing around you and
    why they are experiencing these emotions. As with understanding your own emotions, this
    skill is knowing in real time what another person is experiencing. This skill involves
    reading cues to their emotional state through their eyes, facial expressions, body
    posture, the tone of voice and many other ways.
</p>

    {[ chart type:'horizontalBar' legendshow:'false' tooltipshow:'false' chartheight:'{{chartHeight}}' xaxistype:'linearhorizontal0to100' ]}
        [[ dataitem label:'Others Awareness' value:'{{OthersAwareness}}' fillcolor:'{{chartColor}}' ]] [[ enddataitem ]]
    {[ endchart ]}

<blockquote>
    Your responses to the items on the Others-Awareness scale indicate the score for the
    ability to be aware of others emotions is equal to or better than {{ OthersAwareness }}%
    of those who completed this instrument.
</blockquote>


<h3 id='eq-othersregulating'>Others-Regulating</h3>
<p>
    Others-Regulating is helping those around you express their emotions appropriately
    in the context of your relationship with them. This skill centers on helping others
    know what emotions they are experiencing and then asking questions or giving them
    permission to freely and appropriately express their emotions in the context of
    your relationship.
</p>

    {[ chart type:'horizontalBar' legendshow:'false' tooltipshow:'false' chartheight:'{{chartHeight}}' xaxistype:'linearhorizontal0to100' ]}
        [[ dataitem label:'Others Regulating' value:'{{OthersRegulating}}' fillcolor:'{{chartColor}}' ]] [[ enddataitem ]]
    {[ endchart ]}

<blockquote>
    Your responses to the items on the Others-Regulation scale indicate the score for
    the ability to enable others to appropriately express their emotions in the context
    of your relationship is equal to or better than {{OthersRegulating}}% of those who
    completed this instrument.
</blockquote>

<h3 id='eq-problemsolving'>EQ in Problem Solving</h3>
<p>
    EQ in Problem Solving identifies how proficient you are at using emotions to solve
    problems. This skill requires first being aware of what emotions are involved in
    the problem and what is the source of those emotions. It also includes helping
    others (and yourself) express those emotions appropriate in the context of
    the situation.
</p>

    {[ chart type:'horizontalBar' legendshow:'false' tooltipshow:'false' chartheight:'{{chartHeight}}' xaxistype:'linearhorizontal0to100' ]}
        [[ dataitem label:'EQ in Problem Solving' value:'{{EQinProblemSolving}}' fillcolor:'{{chartColor}}' ]] [[ enddataitem ]]
    {[ endchart ]}

<blockquote>
    Your responses to the items on the EQ in Problem Solving scale indicate the score for
    the ability to use emotions in resolving problems is equal to or better than {{ EQinProblemSolving }}%
    of those who completed this instrument.
</blockquote>


<h3 id='eq-understress'>EQ Under Stress</h3>
<p>
    EQ Under Stress identifies how capable you are of keeping high EQ under high-stress
    moments; which is particularly challenging. This skill requires highly developed
    Self- and Others-Awareness to understand the impact of the current stress. It also
    involves being able to articulate the appropriate emotions under pressure which
    may be different from articulating them when not under stress.
</p>

    {[ chart type:'horizontalBar' legendshow:'false' tooltipshow:'false' chartheight:'{{chartHeight}}' xaxistype:'linearhorizontal0to100' ]}
        [[ dataitem label:'EQ Under Stress' value:'{{EQUnderStress}}' fillcolor:'{{chartColor}}' ]] [[ enddataitem ]]
    {[ endchart ]}

<blockquote>
    Your responses to the items on the EQ in Under Stress scale indicate the score
    for the ability to maintain EQ under significant stress is equal to or better than {{ EQUnderStress }}%
    of those who completed this instrument.
</blockquote>", "5B6219CE-84B5-4F68-BE5B-C3187EDFF2A6" );
            #endregion Attrib for BlockType: EQ Assessment:Results Message
            RockMigrationHelper.UpdateBlockTypeAttribute( "040CFD6D-5155-4BC9-BAEE-A53219A7BECE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Set Page Title", "SetPageTitle", "", @"The text to display as the heading.", 2, @"EQ Inventory Assessment", "E99F01A7-AF8F-4010-A456-3A9048347859" ); // Attrib for BlockType: EQ Assessment:Set Page Title
            RockMigrationHelper.UpdateBlockTypeAttribute( "040CFD6D-5155-4BC9-BAEE-A53219A7BECE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Set Page Icon", "SetPageIcon", "", @"The css class name to use for the heading icon.", 3, @"fa fa-theater-masks", "D5CF91C1-2CC8-46BF-8CC6-DD6AD8B07518" ); // Attrib for BlockType: EQ Assessment:Set Page Icon
            RockMigrationHelper.UpdateBlockTypeAttribute( "040CFD6D-5155-4BC9-BAEE-A53219A7BECE", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Questions", "NumberofQuestions", "", @"The number of questions to show per page while taking the test", 4, @"7", "8D2C5502-0AAB-4FE6-ABE9-05900439827D" ); // Attrib for BlockType: EQ Assessment:Number of Questions
            RockMigrationHelper.UpdateBlockTypeAttribute( "040CFD6D-5155-4BC9-BAEE-A53219A7BECE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Retakes", "AllowRetakes", "", @"If enabled, the person can retake the test after the minimum days passes.", 5, @"True", "A0905767-79C9-4567-BA76-A3FFEE71E0B3" ); // Attrib for BlockType: EQ Assessment:Allow Retakes
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

            RockMigrationHelper.UpdateFieldType( "Assessment Types", "", "Rock", "Rock.Field.Types.AssessmentTypesFieldType", "C263513A-30BE-4823-ABF1-AC12A56F9644" );
        }

        private void PagesBlocksAndAttributesDown()
        {
            // Attrib for BlockType: EQ Assessment:Instructions
            RockMigrationHelper.DeleteAttribute( "6C00C171-E6DC-4027-B587-0AB63AC939E3" );
            // Attrib for BlockType: EQ Assessment:Allow Retakes
            RockMigrationHelper.DeleteAttribute( "A0905767-79C9-4567-BA76-A3FFEE71E0B3" );
            // Attrib for BlockType: EQ Assessment:Set Page Icon
            RockMigrationHelper.DeleteAttribute( "D5CF91C1-2CC8-46BF-8CC6-DD6AD8B07518" );
            // Attrib for BlockType: EQ Assessment:Set Page Title
            RockMigrationHelper.DeleteAttribute( "E99F01A7-AF8F-4010-A456-3A9048347859" );
            // Attrib for BlockType: EQ Assessment:Results Message
            RockMigrationHelper.DeleteAttribute( "5B6219CE-84B5-4F68-BE5B-C3187EDFF2A6" );
            // Attrib for BlockType: EQ Assessment:Number of Questions
            RockMigrationHelper.DeleteAttribute( "8D2C5502-0AAB-4FE6-ABE9-05900439827D" );
            // Attrib for BlockType: Conflict Profile:Allow Retakes
            RockMigrationHelper.DeleteAttribute( "E3965E46-603C-40E5-AB28-1B53E44561DE" );
            // Attrib for BlockType: Conflict Profile:Number of Questions
            RockMigrationHelper.DeleteAttribute( "6CBCA505-E5BA-4FE9-9DD8-7F3C507B12B8" );
            // Attrib for BlockType: Conflict Profile:Results Message
            RockMigrationHelper.DeleteAttribute( "1A855117-6489-4A15-846A-5A99F54E9747" );
            // Attrib for BlockType: Conflict Profile:Set Page Icon
            RockMigrationHelper.DeleteAttribute( "D5ABBD1A-61F1-4C48-8AD9-C26AC7F5CAEF" );
            // Attrib for BlockType: Conflict Profile:Instructions
            RockMigrationHelper.DeleteAttribute( "2E455190-2BAE-4E9F-8505-F393BCE52342" );
            // Attrib for BlockType: Conflict Profile:Set Page Title
            RockMigrationHelper.DeleteAttribute( "C5698564-7178-43BA-B4A3-58B13DDC3AF0" );
            // Attrib for BlockType: Assessment List:Hide If No Active Requests
            RockMigrationHelper.DeleteAttribute( "305AD0A5-6E35-402A-A6A2-50474733368A" );
            // Attrib for BlockType: Assessment List:Lava Template
            RockMigrationHelper.DeleteAttribute( "044D444A-ECDC-4B7A-8987-91577AAB227C" );
            // Attrib for BlockType: Assessment List:Hide If No Requests
            RockMigrationHelper.DeleteAttribute( "1E5EE52F-DFD5-4406-A517-4B76E2800D2A" );
            // Attrib for BlockType: Assessment List:Only Show Requested
            RockMigrationHelper.DeleteAttribute( "7A10C446-B0F3-43F0-9FEB-78B689593736" );
            // Remove Block: Conflict Profile, from Page: Conflict Profile Assessment, Site: External Website
            RockMigrationHelper.DeleteBlock( "D005E292-25F8-45D4-A713-2A5C811F0219" );
            // Remove Block: EQ Assessment, from Page: Emotional Intelligence Assessment, Site: External Website
            RockMigrationHelper.DeleteBlock( "71BE6A7A-7D51-4149-AFB1-3307DF04B2DF" );
            // Remove Block: Assessment List, from Page: My Account, Site: External Website
            RockMigrationHelper.DeleteBlock( "37D4A991-9F9A-47CE-9084-04466F166B6A" );
            // Remove Block: Assessment List, from Page: Assessments, Site: External Website
            RockMigrationHelper.DeleteBlock( "0E22E6CB-1634-41CA-83EF-4BC7CE52F314" );
            RockMigrationHelper.DeleteBlockType( "040CFD6D-5155-4BC9-BAEE-A53219A7BECE" ); // EQ Assessment
            RockMigrationHelper.DeleteBlockType( "91473D2F-607D-4260-9C6A-DD3762FE472D" ); // Conflict Profile
            RockMigrationHelper.DeleteBlockType( "0AD1D108-4ABF-4AED-B3B7-4AAEA16D10E4" ); // Assessment List

            // Use Assessment List page as the parent page for existing assessments
            RockMigrationHelper.MovePage( "C8CEF4B0-4A09-46D2-9B6B-CD2B6D3078B1", "EBAA5140-4B8F-44B8-B1E8-C73B654E4B22" );// Move DISC page back to Support Pages
            RockMigrationHelper.MovePage( "06410598-3DA4-4710-A047-A518157753AB", "EBAA5140-4B8F-44B8-B1E8-C73B654E4B22" );// Move gifts page back to Support Pages

            // Remove the security for the assessments page.
            RockMigrationHelper.DeleteSecurityAuthForPage( "FCF44690-D74C-4FB7-A01B-0EFCA6EA9E1E" );

            RockMigrationHelper.DeletePage( "F44A6424-8B9C-4B44-91A8-4BB6F683D4B6" ); //  Page: Motivators Assessment, Layout: FullWidth, Site: External Website
            RockMigrationHelper.DeletePage( "BE5F3984-C25E-47CA-A602-EE1CED99E9AC" ); //  Page: Emotional Intelligence Assessment, Layout: FullWidth, Site: External Website
            RockMigrationHelper.DeletePage( "37F17AD8-8103-4F85-865C-94E76B4470BB" ); //  Page: Conflict Profile Assessment, Layout: FullWidth, Site: External Website
            RockMigrationHelper.DeletePage( "FCF44690-D74C-4FB7-A01B-0EFCA6EA9E1E" ); //  Page: Assessments, Layout: FullWidth, Site: External Website
        }

        private void AddMotivatorsAttributes()
        {
            RockMigrationHelper.UpdatePersonAttributeCategory( "Motivators", "fa fa-key", "", "CEAA3D59-D53C-40EC-B7B8-E7BBB758BD4D" );

            var categories = new System.Collections.Generic.List<string> { "CEAA3D59-D53C-40EC-B7B8-E7BBB758BD4D" };

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Believing", "core_MotivatorBelieving", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_BELIVING );
            RockMigrationHelper.DeleteSecurityAuthForAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_BELIVING );
            AddDenyToAllSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_BELIVING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Caring", "core_MotivatorCaring", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_CARING );
            RockMigrationHelper.DeleteSecurityAuthForAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_CARING );
            AddDenyToAllSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_CARING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Communicating", "core_MotivatorCommunicating", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_EXPRESSING );
            RockMigrationHelper.DeleteSecurityAuthForAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_EXPRESSING );
            AddDenyToAllSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_EXPRESSING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Empowering", "core_MotivatorEmpowering", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_EMPOWERING );
            RockMigrationHelper.DeleteSecurityAuthForAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_EMPOWERING );
            AddDenyToAllSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_EMPOWERING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Flexing", "core_MotivatorFlexing", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_ADAPTING );
            RockMigrationHelper.DeleteSecurityAuthForAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_ADAPTING );
            AddDenyToAllSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_ADAPTING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Gathering", "core_MotivatorGathering", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_GATHERING );
            RockMigrationHelper.DeleteSecurityAuthForAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_GATHERING );
            AddDenyToAllSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_GATHERING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Innovating", "core_MotivatorInnovating", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_INNOVATING );
            RockMigrationHelper.DeleteSecurityAuthForAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_INNOVATING );
            AddDenyToAllSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_INNOVATING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Leading", "core_MotivatorLeading", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_LEADING );
            RockMigrationHelper.DeleteSecurityAuthForAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_LEADING );
            AddDenyToAllSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_LEADING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Learning", "core_MotivatorLearning", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_LEARNING );
            RockMigrationHelper.DeleteSecurityAuthForAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_LEARNING );
            AddDenyToAllSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_LEARNING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Maximizing", "core_MotivatorMaximizing", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_MAXIMIZING );
            RockMigrationHelper.DeleteSecurityAuthForAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_MAXIMIZING );
            AddDenyToAllSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_MAXIMIZING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Organizing", "core_MotivatorOrganizing", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_ORGANIZING );
            RockMigrationHelper.DeleteSecurityAuthForAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_ORGANIZING );
            AddDenyToAllSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_ORGANIZING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Pacing", "core_MotivatorPacing", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_PACING );
            RockMigrationHelper.DeleteSecurityAuthForAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_PACING );
            AddDenyToAllSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_PACING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Perceiving", "core_MotivatorPerceiving", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_PERCEIVING );
            RockMigrationHelper.DeleteSecurityAuthForAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_PERCEIVING );
            AddDenyToAllSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_PERCEIVING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Relating", "core_MotivatorRelating", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_RELATING );
            RockMigrationHelper.DeleteSecurityAuthForAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_RELATING );
            AddDenyToAllSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_RELATING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Serving", "core_MotivatorServing", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_SERVING );
            RockMigrationHelper.DeleteSecurityAuthForAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_SERVING );
            AddDenyToAllSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_SERVING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Thinking", "core_MotivatorThinking", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_THINKING );
            RockMigrationHelper.DeleteSecurityAuthForAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_THINKING );
            AddDenyToAllSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_THINKING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Transforming", "core_MotivatorTransforming", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_TRANSFORMING );
            RockMigrationHelper.DeleteSecurityAuthForAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_TRANSFORMING );
            AddDenyToAllSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_TRANSFORMING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Uniting", "core_MotivatorUniting", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_UNITING );
            RockMigrationHelper.DeleteSecurityAuthForAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_UNITING );
            AddDenyToAllSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_UNITING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Persevering", "core_MotivatorPersevering", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_PERSEVERING );
            RockMigrationHelper.DeleteSecurityAuthForAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_PERSEVERING );
            AddDenyToAllSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_PERSEVERING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Venturing", "core_MotivatorVenturing", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_RISKING );
            RockMigrationHelper.DeleteSecurityAuthForAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_RISKING );
            AddDenyToAllSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_RISKING );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Visioning", "core_MotivatorVisioning", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_VISIONING );
            RockMigrationHelper.DeleteSecurityAuthForAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_VISIONING );
            AddDenyToAllSecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_VISIONING );

            // Person Attribute "Motivators: Growth Propensity"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", categories, @"Motivators: Growth Propensity", @"Growth Propensity", @"core_MotivatorGrowthPropensity", @"", @"", 4, @"", Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_GROWTHPROPENSITY );
            RockMigrationHelper.AddAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_GROWTHPROPENSITY, @"ConfigurationJSON", @"[{""Guid"":""72536eae-bc85-41e6-a1b2-19755d7fd15e"",""RangeIndex"":0,""Label"":""Very High"",""Color"":""#3f56a1"",""HighValue"":null,""LowValue"":99.0},{""Guid"":""487cc20c-456d-439c-8beb-b61572b02c2a"",""RangeIndex"":1,""Label"":""High"",""Color"":""#3f56a1"",""HighValue"":98.0,""LowValue"":85.0},{""Guid"":""c7c1bb14-1b8d-44f5-911a-5acf2c94a1ef"",""RangeIndex"":2,""Label"":""Medium"",""Color"":""#0e9445"",""HighValue"":84.0,""LowValue"":17.0},{""Guid"":""a58108b7-3841-408f-a1b9-a0c3fc4daa8a"",""RangeIndex"":3,""Label"":""Somewhat Low"",""Color"":""#f0e3ba"",""HighValue"":16.0,""LowValue"":3.0},{""Guid"":""97e4f6d6-8225-439a-83bb-ca2469f917e8"",""RangeIndex"":4,""Label"":""Low"",""Color"":""#f13c1f"",""HighValue"":2.0,""LowValue"":0.0}]", @"537E9E56-C705-4736-B250-57D23305DF5B" );
            AddReadOnlySecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_GROWTHPROPENSITY );
            
            // Person Attribute "Motivators: Relational Theme"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", categories, @"Motivators: Relational Theme", @"Relational Theme", @"core_MotivatorsRelationalTheme", @"", @"", 3, @"", Rock.SystemGuid.Attribute.PERSON_MOTIVATORS_RELATIONAL_THEME );
            RockMigrationHelper.AddAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_MOTIVATORS_RELATIONAL_THEME, @"ConfigurationJSON", @"[{""Guid"":""bd0a2d33-4bb0-44ee-a343-cfe80e0c3a79"",""RangeIndex"":0,""Label"":""High"",""Color"":""#80bb7c"",""HighValue"":null,""LowValue"":67.0},{""Guid"":""f3aef44f-177f-408e-8739-3b2f6d4fbb8f"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#a0cc9e"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""d101199c-c095-4778-92ea-e2ea9957d7c5"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#c1debf"",""HighValue"":33.0,""LowValue"":0.0}]", @"FF41610F-7ABA-46E8-95A4-4A0CF6273A41" );
            AddReadOnlySecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATORS_RELATIONAL_THEME );

            // Person Attribute "Motivators: Directional Theme"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", categories, @"Motivators: Directional Theme", @"Directional Theme", @"core_MotivatorsDirectionalTheme", @"", @"", 0, @"", Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_DIRECTIONAL_THEME );
            RockMigrationHelper.AddAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_DIRECTIONAL_THEME, @"ConfigurationJSON", @"[{""Guid"":""baac657a-f51f-4f74-a086-c8b412b9d1f3"",""RangeIndex"":0,""Label"":""High"",""Color"":""#f26863"",""HighValue"":null,""LowValue"":67.0},{""Guid"":""542db66c-6c2e-4164-9d1d-e1a8ccd6e1c0"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#f58d89"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""ebc180c7-4fda-47b3-b11c-06535359c217"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#fac4c2"",""HighValue"":33.0,""LowValue"":0.0}]", @"9D534653-2DDE-4A04-9996-58476DE77099" );
            AddReadOnlySecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_DIRECTIONAL_THEME );

            // Person Attribute "Motivators: Intellectual Theme"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", categories, @"Motivators: Intellectual Theme", @"Intellectual Theme", @"core_MotivatorsIntellectualTheme", @"", @"", 1, @"", Rock.SystemGuid.Attribute.PERSON_MOTIVATORS_INTELLECTUAL_THEME );
            RockMigrationHelper.AddAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_MOTIVATORS_INTELLECTUAL_THEME, @"ConfigurationJSON", @"[{""Guid"":""202c572c-79cd-4c66-9f14-28fdd81e3b7b"",""RangeIndex"":0,""Label"":""High"",""Color"":""#709ac7"",""HighValue"":null,""LowValue"":67.0},{""Guid"":""f963047a-1d9a-4665-90bb-aa0aa46877a9"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#91b1d4"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""76d6dfab-f9d9-4ee3-ae28-20b687c76840"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#b6cbe2"",""HighValue"":33.0,""LowValue"":0.0}]", @"463BAEE0-CB9D-4CCC-9ABF-6420FCFA74BE" );
            AddReadOnlySecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATORS_INTELLECTUAL_THEME );

            // Person Attribute "Motivators: Positional Theme"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", categories, @"Motivators: Positional Theme", @"Positional Theme", @"core_MotivatorsPositionalTheme", @"", @"", 2, @"", Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_POSITIONAL_THEME );
            RockMigrationHelper.AddAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_POSITIONAL_THEME, @"ConfigurationJSON", @"[{""Guid"":""94cc75f7-177a-43a0-9dc4-37f0216771fe"",""RangeIndex"":0,""Label"":""High"",""Color"":""#f4cf68"",""HighValue"":null,""LowValue"":67.0},{""Guid"":""9b66a1f0-5333-4da8-9c17-78595357f66d"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#f6d988"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""d15fb222-ff88-4adf-818b-a361f94f58d4"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#f8e1a0"",""HighValue"":33.0,""LowValue"":0.0}]", @"38A0329C-AABE-4378-9508-3BCCBAC5722E" );
            AddReadOnlySecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_POSITIONAL_THEME );

            // Person Attribute "Motivators: Top 5 Motivators"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"59D5A94C-94A0-4630-B80A-BB25697D74C7", categories, @"Motivators: Top 5 Motivators", @"Top 5 Motivators", @"core_MotivatorsTop5Motivators", @"", @"", 1031, @"", Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_TOP_5_MOTIVATORS );
            RockMigrationHelper.AddAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_TOP_5_MOTIVATORS, @"allowmultiple", @"True", @"76F52958-E013-4C28-B659-E0C89BE41AA0" );
            RockMigrationHelper.AddAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_TOP_5_MOTIVATORS, @"definedtype", @"79", @"E3ADC996-626F-460B-9F20-635EE5FFF881" );
            RockMigrationHelper.AddAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_TOP_5_MOTIVATORS, @"displaydescription", @"False", @"932A4520-FED4-443D-B8BA-6F6ABE30C1CE" );
            RockMigrationHelper.AddAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_TOP_5_MOTIVATORS, @"enhancedselection", @"False", @"53101DB1-EC99-4F11-BA33-6087534ABF81" );
            RockMigrationHelper.AddAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_TOP_5_MOTIVATORS, @"includeInactive", @"False", @"769F36BD-5281-43C2-98EB-AA8E1238F9D5" );
            AddReadOnlySecurityToAttribute( Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_TOP_5_MOTIVATORS );
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
                    AND [Name] = 'Personality Assessments' )

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
</p>", "973511D4-7C77-42E0-8FDC-23AE5DF61177" );
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
            RockMigrationHelper.UpdateBlockTypeAttribute( "18CF8DA8-5DE0-49EC-A279-D5507CFA5713", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Set Page Icon", "SetPageIcon", "", @"The css class name to use for the heading icon.", 3, @"fa fa-key", "7471495D-4C68-45EA-874D-6778608E81B2" );
            // Attrib for BlockType: Motivators Assessment:Number of Questions
            RockMigrationHelper.UpdateBlockTypeAttribute( "18CF8DA8-5DE0-49EC-A279-D5507CFA5713", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Number of Questions", "NumberofQuestions", "", @"The number of questions to show per page while taking the test", 4, @"7", "02489F19-384F-45BE-BBC4-D2ECC63D0992" );
            // Attrib for BlockType: Motivators Assessment:Allow Retakes
            RockMigrationHelper.UpdateBlockTypeAttribute( "18CF8DA8-5DE0-49EC-A279-D5507CFA5713", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Allow Retakes", "AllowRetakes", "", @"If enabled, the person can retake the test after the minimum days passes.", 5, @"True", "3A07B385-A3C1-4C0B-80F9-F50432503C0A" );

            RockMigrationHelper.AddPageRoute( "0E6AECD6-675F-4908-9FA3-C7E46040527C", "Motivators", "7D00FD4E-9E6C-42B1-BB25-7F417DF25CA4" );// for Page:Motivators Assessment
            RockMigrationHelper.AddPageRoute( "0E6AECD6-675F-4908-9FA3-C7E46040527C", "Motivators/{rckipid}", "9299B437-38C6-421F-B705-B0F2BCEC2CD0" );// for Page:Motivators Assessment
        }

        private void AddMotivatorClusterDefinedType()
        {
            RockMigrationHelper.AddDefinedType( "Personality Assessments", "Motivators Theme", "Used by Rock's Motivator Personality Assessment to hold the four Themes.", "354715FA-564A-420A-8324-0411988AE7AB", @"" );
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
            RockMigrationHelper.UpdateDefinedValue( "354715FA-564A-420A-8324-0411988AE7AB", "Relational", "How you relate to people.", "840C414E-A261-4243-8302-6117E8949FE4", false );
            RockMigrationHelper.UpdateDefinedValue( "354715FA-564A-420A-8324-0411988AE7AB", "Intellectual", "How your mind operates.", "58FEF15F-561D-420E-8937-6CF51D296F0E", false );
            RockMigrationHelper.UpdateDefinedValue( "354715FA-564A-420A-8324-0411988AE7AB", "Positional", "How you relate to structure.", "84322020-4E27-44EF-88F2-EAFDB7286A01", false );
            RockMigrationHelper.UpdateDefinedValue( "354715FA-564A-420A-8324-0411988AE7AB", "Directional", "How you lead a team or organization.", "112A35BE-3108-48D9-B057-125A788AB531", false );

            RockMigrationHelper.AddDefinedValueAttributeValue( "112A35BE-3108-48D9-B057-125A788AB531", "07E85FA1-8F86-4414-8DC3-43D303C55457", @"<p>This theme describes how you lead a team or an organization. The motivators in this theme can be seen in the type of behavior you demonstrate as it relates to the direction or health of the organization or team in which you are engaged. The greater the number of the motivators from this cluster you possess in your top five, the more effective you will be in providing direction within the organization.</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "112A35BE-3108-48D9-B057-125A788AB531", "8B5F72E4-5A49-4224-9437-82B1F23D8896", @"#f26863" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "112A35BE-3108-48D9-B057-125A788AB531", "CE3F126E-B56A-438A-BA45-8EC8437BB961", @"core_MotivatorsDirectionalTheme" );

            RockMigrationHelper.AddDefinedValueAttributeValue( "58FEF15F-561D-420E-8937-6CF51D296F0E", "07E85FA1-8F86-4414-8DC3-43D303C55457", @"<p>This theme describes how you focus your mind. These motivators can be seen in the way you think or the kind of mental activities you naturally pursue. The way you view your mental activity will be directly influenced by the motivators in this theme. Your conversations will be greatly influenced by these motivators that are in the top five of your profile.</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "58FEF15F-561D-420E-8937-6CF51D296F0E", "8B5F72E4-5A49-4224-9437-82B1F23D8896", @"#709ac7" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "58FEF15F-561D-420E-8937-6CF51D296F0E", "CE3F126E-B56A-438A-BA45-8EC8437BB961", @"core_MotivatorsIntellectualTheme" );

            RockMigrationHelper.AddDefinedValueAttributeValue( "840C414E-A261-4243-8302-6117E8949FE4", "07E85FA1-8F86-4414-8DC3-43D303C55457", @"<p>This theme describes how you relate to others. These motivators can best be seen as the reasons you build relationships with the people around you, and influence what you value in relationships. The greater the number of the motivators from this cluster you possess in your top five, the more strongly you will be focused on building healthy relationships.</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "840C414E-A261-4243-8302-6117E8949FE4", "8B5F72E4-5A49-4224-9437-82B1F23D8896", @"#80bb7c" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "840C414E-A261-4243-8302-6117E8949FE4", "CE3F126E-B56A-438A-BA45-8EC8437BB961", @"core_MotivatorsRelationalTheme" );

            RockMigrationHelper.AddDefinedValueAttributeValue( "84322020-4E27-44EF-88F2-EAFDB7286A01", "07E85FA1-8F86-4414-8DC3-43D303C55457", @"<p>This theme describes how you execute your role or position within the team. The motivators in this theme can be seen in the way you approach activity, moment by moment. They dramatically influence what you value and how you spend your time or effort at work. When others look at the way you act, your behavior will be greatly determined by the motivators from this cluster that are found in your top five.</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "84322020-4E27-44EF-88F2-EAFDB7286A01", "8B5F72E4-5A49-4224-9437-82B1F23D8896", @"#f4cf68" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "84322020-4E27-44EF-88F2-EAFDB7286A01", "CE3F126E-B56A-438A-BA45-8EC8437BB961", @"core_MotivatorsPositionalTheme" );
        }

        private void AddMotivatorDefinedType()
        {
            RockMigrationHelper.AddDefinedType( "Personality Assessments", "Motivator", "Used by Rock's Motivator Personality Assessment to hold all the motivator values.", "1DFF1804-0055-491E-9559-54EA3F8F89D1", @"" );
            RockMigrationHelper.AddDefinedTypeAttribute( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Theme", "Theme", "", 1025, "", "A20E6DB1-B830-4D41-9003-43A184E4C910" );
            RockMigrationHelper.AddDefinedTypeAttribute( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Attribute Score Key", "AttributeScoreKey", "", 1022, "", "55FDABC3-22AE-4EE4-9883-8234E3298B99" );
            RockMigrationHelper.AddDefinedTypeAttribute( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "MotivatorId", "MotivatorId", "", 1023, "", "8158A336-8129-4E82-8B61-8C0E883CB91A" );
            RockMigrationHelper.AddDefinedTypeAttribute( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "D747E6AE-C383-4E22-8846-71518E3DD06F", "Color", "Color", "", 1024, "", "9227E7D4-5725-49BD-A0B1-43B769E0A529" );
            RockMigrationHelper.AddDefinedTypeAttribute( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Challenge", "Challenge", "", 1030, "", "5C3A012C-19A2-4EC7-8440-7534FE175591" );
            RockMigrationHelper.AddDefinedTypeAttribute( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Influence", "Influence", "", 1029, "", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6" );
            RockMigrationHelper.AddAttributeQualifier( "55FDABC3-22AE-4EE4-9883-8234E3298B99", "ispassword", "False", "DFF0DDA7-8467-491A-9D50-849DB09787D4" );
            RockMigrationHelper.AddAttributeQualifier( "55FDABC3-22AE-4EE4-9883-8234E3298B99", "maxcharacters", "", "0215F71D-00E2-41BE-9D69-04375C11CF64" );
            RockMigrationHelper.AddAttributeQualifier( "55FDABC3-22AE-4EE4-9883-8234E3298B99", "showcountdown", "False", "26967EB4-A1A4-4C46-9106-EE985E5BEDFF" );
            RockMigrationHelper.AddAttributeQualifier( "5C3A012C-19A2-4EC7-8440-7534FE175591", "allowhtml", "False", "6B234CEF-4FE5-48B7-9E0B-B7D62B39DE1F" );
            RockMigrationHelper.AddAttributeQualifier( "5C3A012C-19A2-4EC7-8440-7534FE175591", "maxcharacters", "", "8ADD305F-BC38-4DF1-B015-78369361E7DE" );
            RockMigrationHelper.AddAttributeQualifier( "5C3A012C-19A2-4EC7-8440-7534FE175591", "numberofrows", "", "D45502CD-FA5B-4E61-9AF0-AE2B7CF81FC4" );
            RockMigrationHelper.AddAttributeQualifier( "5C3A012C-19A2-4EC7-8440-7534FE175591", "showcountdown", "False", "E5E3E46B-D866-4CCF-A8FE-B10CE3A0054B" );
            RockMigrationHelper.AddAttributeQualifier( "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", "allowhtml", "False", "24F2A452-4124-495B-BF89-15161FFA2CD0" );
            RockMigrationHelper.AddAttributeQualifier( "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", "maxcharacters", "", "39FF77F8-D9E0-4952-B399-99B02E314FDB" );
            RockMigrationHelper.AddAttributeQualifier( "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", "numberofrows", "", "3224F5BE-9873-4ED0-BF6B-E4954F829601" );
            RockMigrationHelper.AddAttributeQualifier( "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", "showcountdown", "False", "BCEF63E8-8332-409A-9A27-8EBAE479E30F" );
            RockMigrationHelper.AddAttributeQualifier( "8158A336-8129-4E82-8B61-8C0E883CB91A", "ispassword", "False", "7BE1FF02-DCDB-42DC-891F-60F632915F0B" );
            RockMigrationHelper.AddAttributeQualifier( "8158A336-8129-4E82-8B61-8C0E883CB91A", "maxcharacters", "", "BEAA8022-951F-4A7D-B9E8-0EADF134106B" );
            RockMigrationHelper.AddAttributeQualifier( "8158A336-8129-4E82-8B61-8C0E883CB91A", "showcountdown", "False", "ECD77226-33DC-4184-9DD4-FB9110C38566" );
            RockMigrationHelper.AddAttributeQualifier( "9227E7D4-5725-49BD-A0B1-43B769E0A529", "selectiontype", "Color Picker", "95CCC80C-DCA9-467B-9E42-E3734B6129BC" );
            RockMigrationHelper.AddAttributeQualifier( "A20E6DB1-B830-4D41-9003-43A184E4C910", "allowmultiple", "False", "929F009E-F42F-4893-BD46-34E5945D46BC" );
            RockMigrationHelper.AddAttributeQualifier( "A20E6DB1-B830-4D41-9003-43A184E4C910", "definedtype", "78", "6C084DB5-5EC0-4E73-BAE7-775AE429C852" );
            RockMigrationHelper.AddAttributeQualifier( "A20E6DB1-B830-4D41-9003-43A184E4C910", "displaydescription", "False", "5CA3CB93-B7F0-4C31-8101-0F0AC78AED16" );
            RockMigrationHelper.AddAttributeQualifier( "A20E6DB1-B830-4D41-9003-43A184E4C910", "enhancedselection", "False", "FD29FB8E-E349-4A0C-BF62-856B1AC851E1" );
            RockMigrationHelper.AddAttributeQualifier( "A20E6DB1-B830-4D41-9003-43A184E4C910", "includeInactive", "False", "0008F665-2B5A-49CB-9699-58F72FAC12EF" );

            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Adapting", "You change quickly when circumstances require it. You have the capacity to be adaptable in diverse situations. You understand when the need for transformation is necessary before others do. You enjoy change. You have a desire to be on the front edge of any movement.", "BD5D99E7-E0FF-4535-8B26-BF73EF9B9F89", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Believing", "You pursue the principles in which you believe with dogged determination. You have a tremendous capacity to be tenacious in pursuing principles in which you trust. You clearly know what you believe and are able to articulate this to others. You have expectations for yourself and others according to these standards. You are confident that you have formed your beliefs through wise experience, counsel and judgment.", "99F598E0-E0AC-4B4B-BEAF-589D41764EE1", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Caring", "You are motivated to meet the needs of others. You genuinely care for other people, especially when they are hurting. You have a large capacity for supporting others and are seen as empathetic, especially for those in difficult situations. You may find it easy to identify with the pain that others experience.", "FFD7EF9C-5D68-40D2-A362-416B2D660D51", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Empowering", "You equip others to do what they are gifted to do. You have the capacity to equip and release individuals. You enjoy developing mentoring relationships and modeling leadership. You find it easy to see the strengths of others and are also aware of the areas they need to develop.", "C171D01E-C607-488B-A550-1E341081210B", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Engaging", "This motivator influences you to become deeply involved in the needs of the community. You understand the community, and have a keen sense of how to meet their needs. You are frustrated when you see these needs go unmet. You have a desire to be involved in various organizations that are making an impact in your community.", "5635E95B-3A07-43B7-837A-0F131EF1DA97", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Visioning", "You dream of things that don’t yet exist. You have the capacity to picture what could be possible in the future. You understand that you can’t accomplish this by yourself; therefore, you enjoy attracting others to your picture of the future. You have a desire for your organization to be much more than it currently is and want to bring that into reality.", "EE1603BA-41AE-4CFA-B220-065768996501", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Gathering", "You bring people together. You have the capacity to engage people and find they want to be around you. You understand what people want in a relationship and are able to meet those needs. You enjoy bringing people together. You have a desire to influence those who are drawn to you.", "73087DD2-B892-4367-894F-8922477B2F10", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Growth Propensity", "Growth Propensity measures your perceived mindset on a continuum between a growth mindset and fixed mindset. These are two ends of a spectrum about how we view our own capacity and potential.", "605F3702-6AE7-4545-BEBE-23693E60031C", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Innovating", "You continuously look for new ways to do everything. You have the capacity to see what could be done better. You understand why something isn’t working, and you can figure out how to do it differently. You enjoy finding new solutions and have a desire to create something from nothing. You may be frustrated if you are not challenged. You never look for the easy answer.", "D84E58E4-87FC-4CEB-B83E-A2C6D186366C", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Leading", "You bring others together to accomplish a task. You have the capacity to take responsibility for others to achieve something together. You understand what needs to happen in most situations and can mobilize others to work together to undertake it. You enjoy when others follow your lead, and you have a desire to make an impact working together with others.", "6A2354C6-3FA4-4BAD-89A8-7359FEC48FE3", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Learning", "You continually seek out opportunities to understand new things. You have the capacity to continue learning through various media. You understand that there is so much more to know about our world, and you feel stagnant if you are not growing in some manner. You have a desire to learn all that you can about everything. Every opportunity is an opportunity to learn and grow.", "7EA44A56-58CB-4E40-9779-CC0A79772926", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Maximizing", "You only invest your time, money or effort in areas that can give you a significant rate of return. You have the capacity to see opportunities where a substantial benefit will result. You understand what is necessary to make an impact using your resources.", "3F678404-5844-494F-BDB0-DD9FEEBC98C9", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Organizing", "You seek opportunities that allow you to bring chaos under control. You have the capacity to bring order out of disorder. You understand that the “devil is in the details,” so you work on creating systems to maintain control of those details. You enjoy when everything flows as planned. You have a desire to organize various pieces into one coherent whole.", "85459C0F-65A5-48F9-86F3-40B03F9C53E9", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Pacing", "You keep a consistent and stable structure in your life and work. You have the capacity to know when your life is getting out of balance. You understand how much you can handle and what has to change for your work/life balance to achieve healthy stability. You enjoy living in a structured and consistent manner. You desire to create beneficial boundaries in all areas of your life.", "9F771853-2EBA-47A2-9AC5-26EBEA0A3B25", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Perceiving", "You discern in others what is not readily apparent. You have the capacity to observe behaviors in others which is not consistent with what they are saying. You understand when things don’t turn out to be as they were initially described. You enjoy trying to understand others. You have a desire to find out if your intuitions are correct.", "4C898A5C-B48E-4BAE-AB89-835F25A451BF", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Persevering", "You stay the course when trying to accomplish a goal, long after others have given up. You have the capacity to ride the ups and downs of circumstances without being defeated. You understand that all forward progress is accomplished through grit and hard work. You see demanding times as a test of your abilities. You have a desire to succeed in spite of resistance.", "A027F6B2-56DD-4724-962D-F865606AEAB8", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Expressing", "You seek opportunities to speak in a variety of environments. You have the capacity to speak effectively, and enjoy talking and engaging others about your areas of expertise. You find it easy to articulate your thoughts and to share them in a credible manner. You are not intimidated by speaking in large or small groups, and you find pleasure in persuading others to your perspective.", "FA70E27D-6642-4162-AF17-530F66B507E7", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Relating", "You seek others with whom you can build relationships. You have the capacity to draw people into your sphere of trust and rapport. You understand what others want or need relationally and are able to provide that in a healthy manner. You enjoy the close relationships that develop others. You have a desire to ensure everyone is connected to other people through an interpersonal network.", "D7F9BDE2-8BEB-469E-BAD9-AA4DEBD3D995", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Risking", "You seek out opportunities involving risk and challenge. You have the capacity to thrive in environments which entail some level of risk. You understand how to handle uncertainty in the assignments you pursue. You enjoy the thrill of being challenged to attempt something you have not previously tried. You have a desire to do things that test you.", "4D0A1A6D-3F5A-476E-A633-04EAEF457645", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Serving", "You attend to the needs of others while pursuing their best, not your own. You have the capacity to work behind the scenes when others are not even aware of what you are doing. You understand what needs to be done long before others do. You enjoy doing the little things which may have gone unattended. You have a desire to serve others so they can flourish.", "D8430EAD-7A38-4AD1-B21A-B2119EE0F1CD", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Thinking", "You are intentionally aware of your thoughts at any given moment. You have the capacity to consciously evaluate about your patterns of thought. You understand what is going on in your mind and why you are thinking the way you are. You enjoy internal reflection. You have a desire to understand why others respond as they do and to fit that into your logical mental framework.", "0D82DC77-334C-44B0-84A6-989910907DD4", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Transforming", "You try to improve the organizations to which you belong. You have the capacity to identify and understand crucial changes that need to be made to bring about progress. You understand how to motivate people to accept change, and working together through that process. You have a desire to help people embrace the differences that are best for the team.", "2393C3CE-8E49-46FE-A75B-D5D624A37B49", false );
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Uniting", "You naturally connect people around a common cause. You have the capacity to help people develop ownership of a vision. You understand how to deal with individual needs in order for the team to win. You enjoy helping others feel like a part of the group. You have a desire for everyone on the team to feel responsible for the success of the team.", "D7601B56-7495-4D7B-A916-8C48F78675E3", false );

            RockMigrationHelper.AddDefinedValueAttributeValue( "0D82DC77-334C-44B0-84A6-989910907DD4", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorThinking" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0D82DC77-334C-44B0-84A6-989910907DD4", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F17" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0D82DC77-334C-44B0-84A6-989910907DD4", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#709ac7" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0D82DC77-334C-44B0-84A6-989910907DD4", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"58fef15f-561d-420e-8937-6cf51d296f0e" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0D82DC77-334C-44B0-84A6-989910907DD4", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You can get lost in your thoughts rather than making a decision and moving forward." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0D82DC77-334C-44B0-84A6-989910907DD4", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by your insight into what others may be." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2393C3CE-8E49-46FE-A75B-D5D624A37B49", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorTransforming" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2393C3CE-8E49-46FE-A75B-D5D624A37B49", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F18" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2393C3CE-8E49-46FE-A75B-D5D624A37B49", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f26863" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2393C3CE-8E49-46FE-A75B-D5D624A37B49", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"112a35be-3108-48d9-b057-125a788ab531" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2393C3CE-8E49-46FE-A75B-D5D624A37B49", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may experience the discomfort of leaving people behind who do not want to change, while feeling the need to keep moving ahead with the change." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2393C3CE-8E49-46FE-A75B-D5D624A37B49", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by enabling them to feel comfortable and committed to organizational transformation." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3F678404-5844-494F-BDB0-DD9FEEBC98C9", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorMaximizing" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3F678404-5844-494F-BDB0-DD9FEEBC98C9", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F11" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3F678404-5844-494F-BDB0-DD9FEEBC98C9", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f4cf68" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3F678404-5844-494F-BDB0-DD9FEEBC98C9", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"84322020-4e27-44ef-88f2-eafdb7286a01" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3F678404-5844-494F-BDB0-DD9FEEBC98C9", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may find it difficult to serve others on the team rather than maximizing your own time and effort." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3F678404-5844-494F-BDB0-DD9FEEBC98C9", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others through your strategic sense of when and where to invest resources for maximum impact." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4C898A5C-B48E-4BAE-AB89-835F25A451BF", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorPerceiving" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4C898A5C-B48E-4BAE-AB89-835F25A451BF", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F14" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4C898A5C-B48E-4BAE-AB89-835F25A451BF", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#709ac7" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4C898A5C-B48E-4BAE-AB89-835F25A451BF", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"58fef15f-561d-420e-8937-6cf51d296f0e" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4C898A5C-B48E-4BAE-AB89-835F25A451BF", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may not know what to do with your insights and may find yourself sharing with others who do not need to know." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4C898A5C-B48E-4BAE-AB89-835F25A451BF", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others through your observations that can be harnessed to help the team be more effective in working together." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4D0A1A6D-3F5A-476E-A633-04EAEF457645", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorRisking" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4D0A1A6D-3F5A-476E-A633-04EAEF457645", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F21" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4D0A1A6D-3F5A-476E-A633-04EAEF457645", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f26863" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4D0A1A6D-3F5A-476E-A633-04EAEF457645", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"112a35be-3108-48d9-b057-125a788ab531" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4D0A1A6D-3F5A-476E-A633-04EAEF457645", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You will seldom be satisfied with the status quo and will therefore be easily bored." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4D0A1A6D-3F5A-476E-A633-04EAEF457645", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by stretching them to try something new." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5635E95B-3A07-43B7-837A-0F131EF1DA97", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorEngaging" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5635E95B-3A07-43B7-837A-0F131EF1DA97", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F05" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5635E95B-3A07-43B7-837A-0F131EF1DA97", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#80bb7c" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5635E95B-3A07-43B7-837A-0F131EF1DA97", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"840c414e-a261-4243-8302-6117e8949fe4" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5635E95B-3A07-43B7-837A-0F131EF1DA97", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may not always see issues which need addressing in your life because you are so focused on the community." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5635E95B-3A07-43B7-837A-0F131EF1DA97", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by engaging your community’s needs in real and tangible ways to make a difference." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "605F3702-6AE7-4545-BEBE-23693E60031C", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorGrowthPropensity" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "605F3702-6AE7-4545-BEBE-23693E60031C", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"PS01" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "605F3702-6AE7-4545-BEBE-23693E60031C", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#6400cb" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6A2354C6-3FA4-4BAD-89A8-7359FEC48FE3", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorLeading" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6A2354C6-3FA4-4BAD-89A8-7359FEC48FE3", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F09" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6A2354C6-3FA4-4BAD-89A8-7359FEC48FE3", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f26863" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6A2354C6-3FA4-4BAD-89A8-7359FEC48FE3", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"112a35be-3108-48d9-b057-125a788ab531" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6A2354C6-3FA4-4BAD-89A8-7359FEC48FE3", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may not feel comfortable just being “one of the team” when you are not the sole leader." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6A2354C6-3FA4-4BAD-89A8-7359FEC48FE3", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by your ability to inspire and engage them to accomplish more together than they could have accomplished individually." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "73087DD2-B892-4367-894F-8922477B2F10", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorGathering" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "73087DD2-B892-4367-894F-8922477B2F10", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F07" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "73087DD2-B892-4367-894F-8922477B2F10", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#80bb7c" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "73087DD2-B892-4367-894F-8922477B2F10", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"840c414e-a261-4243-8302-6117e8949fe4" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "73087DD2-B892-4367-894F-8922477B2F10", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may enjoy bringing people together more than actually accomplishing something together." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "73087DD2-B892-4367-894F-8922477B2F10", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by bringing them along with you wherever you go." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7EA44A56-58CB-4E40-9779-CC0A79772926", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorLearning" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7EA44A56-58CB-4E40-9779-CC0A79772926", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F10" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7EA44A56-58CB-4E40-9779-CC0A79772926", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#709ac7" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7EA44A56-58CB-4E40-9779-CC0A79772926", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"58fef15f-561d-420e-8937-6cf51d296f0e" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7EA44A56-58CB-4E40-9779-CC0A79772926", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may enjoy learning so much that there is little effort to actually apply what you learn." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7EA44A56-58CB-4E40-9779-CC0A79772926", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by sharing what you are learning in one area and helping them apply it in different areas." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "85459C0F-65A5-48F9-86F3-40B03F9C53E9", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorOrganizing" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "85459C0F-65A5-48F9-86F3-40B03F9C53E9", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F12" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "85459C0F-65A5-48F9-86F3-40B03F9C53E9", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f4cf68" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "85459C0F-65A5-48F9-86F3-40B03F9C53E9", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"84322020-4e27-44ef-88f2-eafdb7286a01" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "85459C0F-65A5-48F9-86F3-40B03F9C53E9", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may be resistant to change because it could bring unwanted chaos." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "85459C0F-65A5-48F9-86F3-40B03F9C53E9", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by bringing random fragments together to meet your goals." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99F598E0-E0AC-4B4B-BEAF-589D41764EE1", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorBelieving" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99F598E0-E0AC-4B4B-BEAF-589D41764EE1", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F01" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99F598E0-E0AC-4B4B-BEAF-589D41764EE1", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#709ac7" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99F598E0-E0AC-4B4B-BEAF-589D41764EE1", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"58fef15f-561d-420e-8937-6cf51d296f0e" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99F598E0-E0AC-4B4B-BEAF-589D41764EE1", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"Some may see you as inflexible." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99F598E0-E0AC-4B4B-BEAF-589D41764EE1", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others through your convictions." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9F771853-2EBA-47A2-9AC5-26EBEA0A3B25", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorPacing" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9F771853-2EBA-47A2-9AC5-26EBEA0A3B25", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F13" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9F771853-2EBA-47A2-9AC5-26EBEA0A3B25", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f4cf68" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9F771853-2EBA-47A2-9AC5-26EBEA0A3B25", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"84322020-4e27-44ef-88f2-eafdb7286a01" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9F771853-2EBA-47A2-9AC5-26EBEA0A3B25", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may resist a temporary imbalance that is required to complete a task with excellence." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9F771853-2EBA-47A2-9AC5-26EBEA0A3B25", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by modeling long-term sustainable margins within life and work." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A027F6B2-56DD-4724-962D-F865606AEAB8", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorPersevering" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A027F6B2-56DD-4724-962D-F865606AEAB8", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F20" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A027F6B2-56DD-4724-962D-F865606AEAB8", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f4cf68" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A027F6B2-56DD-4724-962D-F865606AEAB8", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"84322020-4e27-44ef-88f2-eafdb7286a01" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A027F6B2-56DD-4724-962D-F865606AEAB8", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may not be able to walk away from those situations that are simply not worth the effort." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A027F6B2-56DD-4724-962D-F865606AEAB8", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others through your resilience and perseverance in difficult times." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BD5D99E7-E0FF-4535-8B26-BF73EF9B9F89", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorAdapting" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BD5D99E7-E0FF-4535-8B26-BF73EF9B9F89", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F06" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BD5D99E7-E0FF-4535-8B26-BF73EF9B9F89", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f4cf68" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BD5D99E7-E0FF-4535-8B26-BF73EF9B9F89", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"84322020-4e27-44ef-88f2-eafdb7286a01" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BD5D99E7-E0FF-4535-8B26-BF73EF9B9F89", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You are so quick to adjust and adapt that you may miss the positive results of perseverance." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BD5D99E7-E0FF-4535-8B26-BF73EF9B9F89", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by incorporating change so that you are better prepared to handle challenges." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C171D01E-C607-488B-A550-1E341081210B", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorEmpowering" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C171D01E-C607-488B-A550-1E341081210B", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F04" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C171D01E-C607-488B-A550-1E341081210B", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f26863" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C171D01E-C607-488B-A550-1E341081210B", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"112a35be-3108-48d9-b057-125a788ab531" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C171D01E-C607-488B-A550-1E341081210B", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may not always address negative issues in the lives of those you are developing." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C171D01E-C607-488B-A550-1E341081210B", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by investing in them to do so much more than they could do without your intentional effort." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7601B56-7495-4D7B-A916-8C48F78675E3", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorUniting" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7601B56-7495-4D7B-A916-8C48F78675E3", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F19" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7601B56-7495-4D7B-A916-8C48F78675E3", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#80bb7c" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7601B56-7495-4D7B-A916-8C48F78675E3", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"840c414e-a261-4243-8302-6117e8949fe4" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7601B56-7495-4D7B-A916-8C48F78675E3", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may spend so long unifying the team that it hurts the critical progress or momentum." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7601B56-7495-4D7B-A916-8C48F78675E3", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by creating a sense of belonging for every member of the team." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7F9BDE2-8BEB-469E-BAD9-AA4DEBD3D995", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorRelating" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7F9BDE2-8BEB-469E-BAD9-AA4DEBD3D995", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F15" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7F9BDE2-8BEB-469E-BAD9-AA4DEBD3D995", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#80bb7c" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7F9BDE2-8BEB-469E-BAD9-AA4DEBD3D995", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"840c414e-a261-4243-8302-6117e8949fe4" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7F9BDE2-8BEB-469E-BAD9-AA4DEBD3D995", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may form relationships with so many people that you simply cannot maintain integrity and depth in them all." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7F9BDE2-8BEB-469E-BAD9-AA4DEBD3D995", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by building strong ties with those who are socially connected to you." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D8430EAD-7A38-4AD1-B21A-B2119EE0F1CD", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorServing" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D8430EAD-7A38-4AD1-B21A-B2119EE0F1CD", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F16" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D8430EAD-7A38-4AD1-B21A-B2119EE0F1CD", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f26863" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D8430EAD-7A38-4AD1-B21A-B2119EE0F1CD", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"112a35be-3108-48d9-b057-125a788ab531" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D8430EAD-7A38-4AD1-B21A-B2119EE0F1CD", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You and those around you may undervalue your contribution to the team." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D8430EAD-7A38-4AD1-B21A-B2119EE0F1CD", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by helping them so they can function within their strengths." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D84E58E4-87FC-4CEB-B83E-A2C6D186366C", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorInnovating" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D84E58E4-87FC-4CEB-B83E-A2C6D186366C", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F08" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D84E58E4-87FC-4CEB-B83E-A2C6D186366C", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f4cf68" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D84E58E4-87FC-4CEB-B83E-A2C6D186366C", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"84322020-4e27-44ef-88f2-eafdb7286a01" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D84E58E4-87FC-4CEB-B83E-A2C6D186366C", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may enjoy creating so much that there is no execution of a plan to bring that innovation to reality." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D84E58E4-87FC-4CEB-B83E-A2C6D186366C", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by energetically tackling something that may never have been done before." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EE1603BA-41AE-4CFA-B220-065768996501", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorVisioning" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EE1603BA-41AE-4CFA-B220-065768996501", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F22" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EE1603BA-41AE-4CFA-B220-065768996501", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f26863" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EE1603BA-41AE-4CFA-B220-065768996501", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"112a35be-3108-48d9-b057-125a788ab531" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EE1603BA-41AE-4CFA-B220-065768996501", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You can tend to live in the future and get frustrated with the realities of the current situation." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EE1603BA-41AE-4CFA-B220-065768996501", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by inspiring and encouraging them to see much more than their current reality." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FA70E27D-6642-4162-AF17-530F66B507E7", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorExpressing" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FA70E27D-6642-4162-AF17-530F66B507E7", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F03" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FA70E27D-6642-4162-AF17-530F66B507E7", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#709ac7" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FA70E27D-6642-4162-AF17-530F66B507E7", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"58fef15f-561d-420e-8937-6cf51d296f0e" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FA70E27D-6642-4162-AF17-530F66B507E7", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may spend more time speaking to people rather than listening to them." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FA70E27D-6642-4162-AF17-530F66B507E7", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others through speaking and sharing your perspective." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FFD7EF9C-5D68-40D2-A362-416B2D660D51", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorCaring" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FFD7EF9C-5D68-40D2-A362-416B2D660D51", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F02" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FFD7EF9C-5D68-40D2-A362-416B2D660D51", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#80bb7c" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FFD7EF9C-5D68-40D2-A362-416B2D660D51", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"840c414e-a261-4243-8302-6117e8949fe4" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FFD7EF9C-5D68-40D2-A362-416B2D660D51", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may become so consumed with meeting immediate needs that you miss long term solutions." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FFD7EF9C-5D68-40D2-A362-416B2D660D51", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others with your care and compassion." );
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

            RockMigrationHelper.DeleteSecurityAuthForCategory( "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969" );
            AddDenyToAllSecurityToAttribute( "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969" );
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
            var categories = new System.Collections.Generic.List<string> { "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969" };
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( "59D5A94C-94A0-4630-B80A-BB25697D74C7", categories, "DISC: DISC Profile", "DISC Profile", "core_DISCDISCProfile", "", "", 1029, "", "6EAC3DF8-CA81-41A5-B1CF-A8DD7BD42F8D" );
        }

        private void AddDiscProfilePersonAttributeDown()
        {
            RockMigrationHelper.DeleteAttribute( "6EAC3DF8-CA81-41A5-B1CF-A8DD7BD42F8D" );
        }

        //Change the "Adaptive" DISC person attributes to category Personality Assessment Data.
        private void UpdateAdaptiveDiscAttributeCategoryUp()
        {
            var categories = new System.Collections.Generic.List<string> { "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969" }; //"Personality Assessment Data"

            // Person Attribute "Adaptive D"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"A75DFC58-7A1B-4799-BF31-451B2BBE38FF", categories, @"Adaptive D", @"", @"AdaptiveD", @"fa fa-bar-chart", @"Adaptive Dominance: is bottom line oriented, makes quick decisions, wants direct answers.", 1, @"", @"EDE5E199-37BE-424F-A788-5CDCC064157C" );
            AddDenyToAllSecurityToAttribute( "EDE5E199-37BE-424F-A788-5CDCC064157C" );

            // Person Attribute "Adaptive I"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"A75DFC58-7A1B-4799-BF31-451B2BBE38FF", categories, @"Adaptive I", @"", @"AdaptiveI", @"fa fa-bar-chart", @"Adaptive Influence: very people oriented, has a lot of friends, wants opportunity to talk.", 2, @"", @"7F0A1794-0150-413B-9AE1-A6B0D6373DA6" );
            AddDenyToAllSecurityToAttribute( "7F0A1794-0150-413B-9AE1-A6B0D6373DA6" );

            // Person Attribute "Adaptive S"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"A75DFC58-7A1B-4799-BF31-451B2BBE38FF", categories, @"Adaptive S", @"", @"AdaptiveS", @"fa fa-bar-chart", @"Adaptive Steadiness: does not like change, wants limited responsibility and sincere appreciation.", 3, @"", @"2512DAC6-BBC4-4D0E-A01D-E92F94C534BD" );
            AddDenyToAllSecurityToAttribute( "2512DAC6-BBC4-4D0E-A01D-E92F94C534BD" );

            // Person Attribute "Adaptive C"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"A75DFC58-7A1B-4799-BF31-451B2BBE38FF", categories, @"Adaptive C", @"", @"AdaptiveC", @"fa fa-bar-chart", @"Adaptive Cautiousness: is detail oriented, wants no sudden changes, won't make decision.", 4, @"", @"4A2E1539-4ECC-40B9-9EBD-C0C84EC8DA36" );
            AddDenyToAllSecurityToAttribute( "4A2E1539-4ECC-40B9-9EBD-C0C84EC8DA36" );
        }

        //Revert Change the "Adaptive" DISC person attributes to category Personality Assessment Data.
        private void UpdateAdaptiveDiscAttributeCategoryDown()
        {
            var categories = new System.Collections.Generic.List<string> { "0B187C81-2106-4875-82B6-FBF1277AE23B" };

            // Person Attribute "Adaptive D"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"A75DFC58-7A1B-4799-BF31-451B2BBE38FF", categories, @"Adaptive D", @"", @"AdaptiveD", @"fa fa-bar-chart", @"Adaptive Dominance: is bottom line oriented, makes quick decisions, wants direct answers.", 1, @"", @"EDE5E199-37BE-424F-A788-5CDCC064157C" );
            SecurityViewAllEditStaffAdmin( "EDE5E199-37BE-424F-A788-5CDCC064157C" );

            // Person Attribute "Adaptive I"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"A75DFC58-7A1B-4799-BF31-451B2BBE38FF", categories, @"Adaptive I", @"", @"AdaptiveI", @"fa fa-bar-chart", @"Adaptive Influence: very people oriented, has a lot of friends, wants opportunity to talk.", 2, @"", @"7F0A1794-0150-413B-9AE1-A6B0D6373DA6" );
            SecurityViewAllEditStaffAdmin( "7F0A1794-0150-413B-9AE1-A6B0D6373DA6" );

            // Person Attribute "Adaptive S"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"A75DFC58-7A1B-4799-BF31-451B2BBE38FF", categories, @"Adaptive S", @"", @"AdaptiveS", @"fa fa-bar-chart", @"Adaptive Steadiness: does not like change, wants limited responsibility and sincere appreciation.", 3, @"", @"2512DAC6-BBC4-4D0E-A01D-E92F94C534BD" );
            SecurityViewAllEditStaffAdmin( "2512DAC6-BBC4-4D0E-A01D-E92F94C534BD" );

            // Person Attribute "Adaptive C"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"A75DFC58-7A1B-4799-BF31-451B2BBE38FF", categories, @"Adaptive C", @"", @"AdaptiveC", @"fa fa-bar-chart", @"Adaptive Cautiousness: is detail oriented, wants no sudden changes, won't make decision.", 4, @"", @"4A2E1539-4ECC-40B9-9EBD-C0C84EC8DA36" );
            SecurityViewAllEditStaffAdmin( "4A2E1539-4ECC-40B9-9EBD-C0C84EC8DA36" );
        }

        private void UpdateNaturalDiscAttributesUp()
        {
            var categories = new System.Collections.Generic.List<string> { "0B187C81-2106-4875-82B6-FBF1277AE23B" };

            // Person Attribute "DISC: D Value"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", categories, @"DISC: D Value", @"D Value", @"NaturalD", @"fa fa-bar-chart", @"Natural Dominance: is bottom line oriented, makes quick decisions, wants direct answers.", 5, @"", @"86670F7D-07BA-4ECE-9BB9-9D94B5FB5F26" );
            RockMigrationHelper.AddAttributeQualifier( @"86670F7D-07BA-4ECE-9BB9-9D94B5FB5F26", @"ConfigurationJSON", @"[{""Guid"":""4f7c1ea0-415d-4d89-86b1-a3b343d3115f"",""RangeIndex"":0,""Label"":""High"",""Color"":""#709ac7"",""HighValue"":null,""LowValue"":67.0},{""Guid"":""a5e8c10e-c360-4083-a3d4-fa18a096b68a"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#91b1d4"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""8f4ebbf2-d2d8-479d-9330-8ca2baab37a5"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#b6cbe2"",""HighValue"":33.0,""LowValue"":0.0}]", @"AC4B49ED-4BDE-4838-B310-25697DC4804B" );
            AddReadWriteSecurityToAttribute( "86670F7D-07BA-4ECE-9BB9-9D94B5FB5F26" );

            // Person Attribute "DISC: I Value"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", categories, @"DISC: I Value", @"I Value", @"NaturalI", @"fa fa-bar-chart", @"Natural Influence: very people oriented, has a lot of friends, wants opportunity to talk", 6, @"", @"3EFF4FEF-EE4C-40E2-8DBD-80F3276852DA" );
            RockMigrationHelper.AddAttributeQualifier( @"3EFF4FEF-EE4C-40E2-8DBD-80F3276852DA", @"ConfigurationJSON", @"[{""Guid"":""353c14d3-96a1-441f-ac86-d663aa86ef37"",""RangeIndex"":0,""Label"":""High"",""Color"":""#f4cf68"",""HighValue"":null,""LowValue"":67.0},{""Guid"":""b8aa394f-6755-4f01-b625-3702825fcd00"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#f6d988"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""e71c0ac8-2a4b-4cd6-a5ac-0285b24d9b1a"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#f8e1a0"",""HighValue"":33.0,""LowValue"":0.0}]", @"C652B942-935C-4614-90EC-331D57A0DDE4" );
            AddReadWriteSecurityToAttribute( "3EFF4FEF-EE4C-40E2-8DBD-80F3276852DA" );

            // Person Attribute "DISC: S Value"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", categories, @"DISC: S Value", @"S Value", @"NaturalS", @"fa fa-bar-chart", @"Natural Steadiness: does not like change, wants limited responsibility and sincere appreciation.", 7, @"", @"FA4341B4-28C7-409E-A101-548BB5759BE6" );
            RockMigrationHelper.AddAttributeQualifier( @"FA4341B4-28C7-409E-A101-548BB5759BE6", @"ConfigurationJSON", @"[{""Guid"":""da2a311d-e5d2-4127-8683-beb4f00a82d1"",""RangeIndex"":0,""Label"":""High"",""Color"":""#80bb7c"",""HighValue"":null,""LowValue"":67.0},{""Guid"":""e115b2a1-f5dd-49bf-b5a7-11992e63fd8e"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#a0cc9e"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""27ab08d6-464d-42cf-92b8-c4cf4b3ec492"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#c1debf"",""HighValue"":33.0,""LowValue"":0.0}]", @"06E123D6-F149-421B-BE6C-CA1612A562E8" );
            AddReadWriteSecurityToAttribute( "FA4341B4-28C7-409E-A101-548BB5759BE6" );

            // Person Attribute "DISC: C Value"
            RockMigrationHelper.AddOrUpdatePersonAttributeByGuid( @"E73B9F41-8325-4229-8EA5-75180066680C", categories, @"DISC: C Value", @"C Value", @"NaturalC", @"fa fa-bar-chart", @"Natural Cautiousness: is detail oriented, wants no sudden changes, won't make decision.", 8, @"", @"3A10ECFB-8CAB-4CCA-8B29-298756CD3251" );
            RockMigrationHelper.AddAttributeQualifier( @"3A10ECFB-8CAB-4CCA-8B29-298756CD3251", @"ConfigurationJSON", @"[{""Guid"":""ee28fca6-4ccc-4cf1-a166-69171b34eb17"",""RangeIndex"":0,""Label"":""High"",""Color"":""#f26863"",""HighValue"":null,""LowValue"":67.0},{""Guid"":""786bfb8a-0b87-4d01-b71f-031f91b229fa"",""RangeIndex"":1,""Label"":""Medium"",""Color"":""#f58d89"",""HighValue"":66.0,""LowValue"":34.0},{""Guid"":""5d864986-1b74-4bd9-b99e-7ea177b348e6"",""RangeIndex"":2,""Label"":""Low"",""Color"":""#fac4c2"",""HighValue"":33.0,""LowValue"":0.0}]", @"C312B380-703B-4F84-A720-C6FC7FC92059" );
            AddReadWriteSecurityToAttribute( "3A10ECFB-8CAB-4CCA-8B29-298756CD3251" );
        }

        private void UpdateNatrualDiscAttributesDown()
        {
            // Person Attribute "Natural D"
            RockMigrationHelper.UpdatePersonAttribute( @"A75DFC58-7A1B-4799-BF31-451B2BBE38FF", @"0B187C81-2106-4875-82B6-FBF1277AE23B", @"Natural D", @"NaturalD", @"fa fa-bar-chart", @"Natural Dominance: is bottom line oriented, makes quick decisions, wants direct answers.", 5, @"", @"86670F7D-07BA-4ECE-9BB9-9D94B5FB5F26" );
            SecurityViewAllEditStaffAdmin( "86670F7D-07BA-4ECE-9BB9-9D94B5FB5F26" );

            // Person Attribute "Natural I"
            RockMigrationHelper.UpdatePersonAttribute( @"A75DFC58-7A1B-4799-BF31-451B2BBE38FF", @"0B187C81-2106-4875-82B6-FBF1277AE23B", @"Natural I", @"NaturalI", @"fa fa-bar-chart", @"Natural Influence: very people oriented, has a lot of friends, wants opportunity to talk", 6, @"", @"3EFF4FEF-EE4C-40E2-8DBD-80F3276852DA" );
            SecurityViewAllEditStaffAdmin( "3EFF4FEF-EE4C-40E2-8DBD-80F3276852DA" );

            // Person Attribute "Natural S"
            RockMigrationHelper.UpdatePersonAttribute( @"A75DFC58-7A1B-4799-BF31-451B2BBE38FF", @"0B187C81-2106-4875-82B6-FBF1277AE23B", @"Natural S", @"NaturalS", @"fa fa-bar-chart", @"Natural Steadiness: does not like change, wants limited responsibility and sincere appreciation.", 7, @"", @"FA4341B4-28C7-409E-A101-548BB5759BE6" );
            SecurityViewAllEditStaffAdmin( "FA4341B4-28C7-409E-A101-548BB5759BE6" );

            // Person Attribute "Natural C"
            RockMigrationHelper.UpdatePersonAttribute( @"A75DFC58-7A1B-4799-BF31-451B2BBE38FF", @"0B187C81-2106-4875-82B6-FBF1277AE23B", @"Natural C", @"NaturalC", @"fa fa-bar-chart", @"Natural Cautiousness: is detail oriented, wants no sudden changes, won't make decision.", 8, @"", @"3A10ECFB-8CAB-4CCA-8B29-298756CD3251" );
            SecurityViewAllEditStaffAdmin( "3A10ECFB-8CAB-4CCA-8B29-298756CD3251" );
        }

        /// <summary>
        /// SK: Update Motivator Defined Type and related changes in person attributes
        /// </summary>
        private void UpdateMotivatorDefinedType()
        {
            //RockMigrationHelper.AddDefinedTypeAttribute( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Influence", "Influence", "", 1029, "", "A6CEFE92-C546-45FA-9A87-1955AAAC1723" );
            //RockMigrationHelper.AddDefinedTypeAttribute( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Challenge", "Challenge", "", 1030, "", "010DDB3F-CE6B-44D1-B4EA-E9BC59EFE6DD" );
            //RockMigrationHelper.AddAttributeQualifier( "010DDB3F-CE6B-44D1-B4EA-E9BC59EFE6DD", "ispassword", "False", "DC269710-000A-4BCA-898B-3318622E9C8E" );
            //RockMigrationHelper.AddAttributeQualifier( "010DDB3F-CE6B-44D1-B4EA-E9BC59EFE6DD", "maxcharacters", "", "2E81B54A-ACBC-4F31-A9C9-A3CA0E106CCC" );
            //RockMigrationHelper.AddAttributeQualifier( "010DDB3F-CE6B-44D1-B4EA-E9BC59EFE6DD", "showcountdown", "False", "44530C2D-DF1E-4D7B-99B1-B2796EAC34E0" );

            //RockMigrationHelper.AddAttributeQualifier( "A6CEFE92-C546-45FA-9A87-1955AAAC1723", "ispassword", "False", "F201E2B1-E756-4C05-B907-25F57DF0858A" );
            //RockMigrationHelper.AddAttributeQualifier( "A6CEFE92-C546-45FA-9A87-1955AAAC1723", "maxcharacters", "", "ECD1F895-D363-4F38-A169-E1549620DC20" );
            //RockMigrationHelper.AddAttributeQualifier( "A6CEFE92-C546-45FA-9A87-1955AAAC1723", "showcountdown", "False", "614D876D-EDC9-424F-9563-A54A1D221957" );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "0D82DC77-334C-44B0-84A6-989910907DD4", "010DDB3F-CE6B-44D1-B4EA-E9BC59EFE6DD", @"You can get lost in your thoughts rather than making a decision and moving forward." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "0D82DC77-334C-44B0-84A6-989910907DD4", "A6CEFE92-C546-45FA-9A87-1955AAAC1723", @"You influence others by your insight into what others may be." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "2393C3CE-8E49-46FE-A75B-D5D624A37B49", "010DDB3F-CE6B-44D1-B4EA-E9BC59EFE6DD", @"You may experience the discomfort of leaving people behind who do not want to change, while feeling the need to keep moving ahead with the change." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "2393C3CE-8E49-46FE-A75B-D5D624A37B49", "A6CEFE92-C546-45FA-9A87-1955AAAC1723", @"You influence others by enabling them to feel comfortable and committed to organizational transformation." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "3F678404-5844-494F-BDB0-DD9FEEBC98C9", "010DDB3F-CE6B-44D1-B4EA-E9BC59EFE6DD", @"You may find it difficult to serve others on the team rather than maximizing your own impact." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "3F678404-5844-494F-BDB0-DD9FEEBC98C9", "A6CEFE92-C546-45FA-9A87-1955AAAC1723", @"You influence others through your strategic sense of when and where to invest resources for maximum impact." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "4C898A5C-B48E-4BAE-AB89-835F25A451BF", "010DDB3F-CE6B-44D1-B4EA-E9BC59EFE6DD", @"You may not know what to do with your insights and may find yourself sharing gossip." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "4C898A5C-B48E-4BAE-AB89-835F25A451BF", "A6CEFE92-C546-45FA-9A87-1955AAAC1723", @"You influence others through your observations that can be harnessed to help the team be more effective in working together." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "4D0A1A6D-3F5A-476E-A633-04EAEF457645", "010DDB3F-CE6B-44D1-B4EA-E9BC59EFE6DD", @"You will seldom be satisfied with the status quo and will therefore be easily bored." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "4D0A1A6D-3F5A-476E-A633-04EAEF457645", "A6CEFE92-C546-45FA-9A87-1955AAAC1723", @"You influence others by stretching them to try something new." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "5635E95B-3A07-43B7-837A-0F131EF1DA97", "010DDB3F-CE6B-44D1-B4EA-E9BC59EFE6DD", @"You may not always see issues which need addressing in your life because you are so focused on the community." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "5635E95B-3A07-43B7-837A-0F131EF1DA97", "A6CEFE92-C546-45FA-9A87-1955AAAC1723", @"You influence others by engaging your community’s needs in real and tangible ways to make a difference." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "73087DD2-B892-4367-894F-8922477B2F10", "010DDB3F-CE6B-44D1-B4EA-E9BC59EFE6DD", @"You may enjoy bringing people together more than actually accomplishing something together." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "73087DD2-B892-4367-894F-8922477B2F10", "A6CEFE92-C546-45FA-9A87-1955AAAC1723", @"You influence others by bringing them along with you wherever you go." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "7EA44A56-58CB-4E40-9779-CC0A79772926", "010DDB3F-CE6B-44D1-B4EA-E9BC59EFE6DD", @"You may not feel comfortable just being “one of the team” when you are not the sole leader." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "7EA44A56-58CB-4E40-9779-CC0A79772926", "A6CEFE92-C546-45FA-9A87-1955AAAC1723", @"You influence others by your ability to inspire and engage them to accomplish more together than they could have accomplished individually." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "85459C0F-65A5-48F9-86F3-40B03F9C53E9", "010DDB3F-CE6B-44D1-B4EA-E9BC59EFE6DD", @"You may be resistant to change because it could bring unwanted chaos." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "85459C0F-65A5-48F9-86F3-40B03F9C53E9", "A6CEFE92-C546-45FA-9A87-1955AAAC1723", @"You influence others by bringing random fragments together to meet your goals." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "99F598E0-E0AC-4B4B-BEAF-589D41764EE1", "010DDB3F-CE6B-44D1-B4EA-E9BC59EFE6DD", @"Some may see you as inflexible." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "99F598E0-E0AC-4B4B-BEAF-589D41764EE1", "A6CEFE92-C546-45FA-9A87-1955AAAC1723", @"You influence others through your convictions." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "9F771853-2EBA-47A2-9AC5-26EBEA0A3B25", "010DDB3F-CE6B-44D1-B4EA-E9BC59EFE6DD", @"You may resist a temporary imbalance that is required to complete a task with excellence." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "9F771853-2EBA-47A2-9AC5-26EBEA0A3B25", "A6CEFE92-C546-45FA-9A87-1955AAAC1723", @"You influence others by modeling long-term sustainable margins within life and work." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "A027F6B2-56DD-4724-962D-F865606AEAB8", "010DDB3F-CE6B-44D1-B4EA-E9BC59EFE6DD", @"You may not be able to walk away from those situations that are simply not worth the effort." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "A027F6B2-56DD-4724-962D-F865606AEAB8", "A6CEFE92-C546-45FA-9A87-1955AAAC1723", @"You influence others through your resilience and perseverance in difficult times." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "BD5D99E7-E0FF-4535-8B26-BF73EF9B9F89", "010DDB3F-CE6B-44D1-B4EA-E9BC59EFE6DD", @"You are so quick to adjust and adapt that you may miss the positive results of perseverance." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "BD5D99E7-E0FF-4535-8B26-BF73EF9B9F89", "A6CEFE92-C546-45FA-9A87-1955AAAC1723", @"You influence others by incorporating change so that you are better prepared to handle challenges." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "C171D01E-C607-488B-A550-1E341081210B", "010DDB3F-CE6B-44D1-B4EA-E9BC59EFE6DD", @"You may not always address negative issues in the lives of those you are developing." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "C171D01E-C607-488B-A550-1E341081210B", "A6CEFE92-C546-45FA-9A87-1955AAAC1723", @"You influence others by investing in them to do so much more than they could do without your intentional effort." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "D7601B56-7495-4D7B-A916-8C48F78675E3", "010DDB3F-CE6B-44D1-B4EA-E9BC59EFE6DD", @"You may spend so long unifying the team that it hurts the critical progress or momentum." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "D7601B56-7495-4D7B-A916-8C48F78675E3", "A6CEFE92-C546-45FA-9A87-1955AAAC1723", @"You influence others by creating a sense of belonging for every member of the team." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "D7F9BDE2-8BEB-469E-BAD9-AA4DEBD3D995", "010DDB3F-CE6B-44D1-B4EA-E9BC59EFE6DD", @"You may form relationships with so many people that you simply cannot maintain integrity and depth in them all." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "D7F9BDE2-8BEB-469E-BAD9-AA4DEBD3D995", "A6CEFE92-C546-45FA-9A87-1955AAAC1723", @"You influence others by building strong ties with those who are socially connected to you." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "D8430EAD-7A38-4AD1-B21A-B2119EE0F1CD", "010DDB3F-CE6B-44D1-B4EA-E9BC59EFE6DD", @"You and those around you may undervalue your contribution to the team." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "D8430EAD-7A38-4AD1-B21A-B2119EE0F1CD", "A6CEFE92-C546-45FA-9A87-1955AAAC1723", @"You influence others by helping them so they can function within their strengths." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "D84E58E4-87FC-4CEB-B83E-A2C6D186366C", "010DDB3F-CE6B-44D1-B4EA-E9BC59EFE6DD", @"You may enjoy creating so much that there is no execution of a plan to bring that innovation to reality." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "D84E58E4-87FC-4CEB-B83E-A2C6D186366C", "A6CEFE92-C546-45FA-9A87-1955AAAC1723", @"You influence others by energetically tackling something that may never have been done before." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "EE1603BA-41AE-4CFA-B220-065768996501", "010DDB3F-CE6B-44D1-B4EA-E9BC59EFE6DD", @"You can tend to live in the future and get frustrated with the realities of the current situation." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "EE1603BA-41AE-4CFA-B220-065768996501", "A6CEFE92-C546-45FA-9A87-1955AAAC1723", @"You influence others by inspiring and encouraging them to see much more than their current reality." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "FA70E27D-6642-4162-AF17-530F66B507E7", "010DDB3F-CE6B-44D1-B4EA-E9BC59EFE6DD", @"You may spend more time convincing people than listening to them." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "FA70E27D-6642-4162-AF17-530F66B507E7", "A6CEFE92-C546-45FA-9A87-1955AAAC1723", @"You influence others through speaking and convincing them of your position." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "FFD7EF9C-5D68-40D2-A362-416B2D660D51", "010DDB3F-CE6B-44D1-B4EA-E9BC59EFE6DD", @"You may become so consumed with meeting immediate needs that you miss long term solutions." );
            //RockMigrationHelper.AddDefinedValueAttributeValue( "FFD7EF9C-5D68-40D2-A362-416B2D660D51", "A6CEFE92-C546-45FA-9A87-1955AAAC1723", @"You influence others with your care and compassion." );

            Sql( $@"
                    UPDATE
	                    [Attribute]
                    SET [Key]='core_MotivatorPersevering',
                    [Name]='Motivator Persevering',
					[AbbreviatedName]='Motivator Persevering'
                    WHERE [Guid]='{Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_PERSEVERING}'

                    UPDATE
	                    [Attribute]
                    SET [Key]='core_MotivatorRisking',
                    [Name]='Motivator Risking',
					[AbbreviatedName]='Motivator Risking'
                    WHERE [Guid]='{Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_RISKING}'

                    UPDATE
	                    [Attribute]
                    SET [Key]='core_MotivatorExpressing',
                    [Name]='Motivator Expressing',
					[AbbreviatedName]='Motivator Expressing'
                    WHERE [Guid]='{Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_EXPRESSING}'
                    
                    UPDATE
	                    [Attribute]
                    SET [Key]='core_MotivatorAdapting',
                    [Name]='Motivator Adapting',
                    [AbbreviatedName]='Motivator Adapting'
                    WHERE [Guid]='{Rock.SystemGuid.Attribute.PERSON_MOTIVATOR_ADAPTING}'" );
        }

        #region Security Methods
        private void AddDenyToAllSecurityToAttribute( string attributeGuid )
        {
            RockMigrationHelper.AddSecurityAuthForAttribute(
                attributeGuid,
                0,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                ( int ) Rock.Model.SpecialRole.AllUsers,
                Guid.NewGuid().ToString() );

            RockMigrationHelper.AddSecurityAuthForAttribute(
                attributeGuid,
                0,
                Rock.Security.Authorization.EDIT,
                false,
                null,
                ( int ) Rock.Model.SpecialRole.AllUsers,
                Guid.NewGuid().ToString() );
        }

        /// <summary>
        /// Removes any existing security settings on the attribute.
        /// Allow View access to all users.
        /// Allow Edit access to Staff, Staff-Like, and Administrators
        /// </summary>
        private void SecurityViewAllEditStaffAdmin( string attributeGuid )
        {
            RockMigrationHelper.DeleteSecurityAuthForAttribute( attributeGuid );

            RockMigrationHelper.AddSecurityAuthForAttribute(
                attributeGuid,
                0,
                Rock.Security.Authorization.VIEW,
                true,
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
               1,
               Rock.Security.Authorization.EDIT,
               true,
               Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS,
               ( int ) Rock.Model.SpecialRole.None,
               Guid.NewGuid().ToString() );

            RockMigrationHelper.AddSecurityAuthForAttribute(
               attributeGuid,
               2,
               Rock.Security.Authorization.EDIT,
               true,
               Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
               ( int ) Rock.Model.SpecialRole.None,
               Guid.NewGuid().ToString() );
        }

        /// <summary>
        /// Adds the security to attribute. Deny View/Edit AllUsers.
        /// Grant View/Edit Administrators, Staff, Staff Like
        /// </summary>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        private void AddReadWriteSecurityToAttribute( string attributeGuid )
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
               1,
               Rock.Security.Authorization.VIEW,
               true,
               Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS,
               ( int ) Rock.Model.SpecialRole.None,
               Guid.NewGuid().ToString() );

            RockMigrationHelper.AddSecurityAuthForAttribute(
               attributeGuid,
               2,
               Rock.Security.Authorization.VIEW,
               true,
               Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
               ( int ) Rock.Model.SpecialRole.None,
               Guid.NewGuid().ToString() );

            RockMigrationHelper.AddSecurityAuthForAttribute(
                attributeGuid,
                3,
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

        /// <summary>
        /// Deny Read/Edit for all users.
        /// Allow View for Administrators, Staff, and Staff-Like.
        /// </summary>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        private void AddReadOnlySecurityToAttribute( string attributeGuid )
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
               1,
               Rock.Security.Authorization.VIEW,
               true,
               Rock.SystemGuid.Group.GROUP_STAFF_MEMBERS,
               ( int ) Rock.Model.SpecialRole.None,
               Guid.NewGuid().ToString() );

            RockMigrationHelper.AddSecurityAuthForAttribute(
               attributeGuid,
               2,
               Rock.Security.Authorization.VIEW,
               true,
               Rock.SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS,
               ( int ) Rock.Model.SpecialRole.None,
               Guid.NewGuid().ToString() );

            RockMigrationHelper.AddSecurityAuthForAttribute(
                attributeGuid,
                3,
                Rock.Security.Authorization.VIEW,
                false,
                null,
                ( int ) Rock.Model.SpecialRole.AllUsers,
                Guid.NewGuid().ToString() );

            RockMigrationHelper.AddSecurityAuthForAttribute(
                attributeGuid,
                0,
                Rock.Security.Authorization.EDIT,
                false,
                null,
                ( int ) Rock.Model.SpecialRole.AllUsers,
                Guid.NewGuid().ToString() );
        }

        #endregion Security Methods
    }
}
