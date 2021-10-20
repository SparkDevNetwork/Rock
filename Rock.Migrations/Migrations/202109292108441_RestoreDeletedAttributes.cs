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
    public partial class RestoreDeletedAttributes : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /* This migration fixes attributes that accidentally got deleted due to a buggy version of 202109090037012_GivingJourneyStages prior to this fix https://github.com/SparkDevNetwork/Rock/commit/04a33271cb6932a6883ed661feb720c8180c43c7 */
            RestoreBlockTypeAttributesAndValues();

            RestoreBlockTemplateAttributes();
            RestoreBlockTemplateAttributeValues();

            RestoreMotivatorAttributes();
        }

        public void RestoreMotivatorAttributes()
        {
            RockMigrationHelper.AddDefinedTypeAttribute( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Theme", "Theme", "", 1025, "", "A20E6DB1-B830-4D41-9003-43A184E4C910" );
            RockMigrationHelper.AddDefinedTypeAttribute( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Attribute Score Key", "AttributeScoreKey", "", 1022, "", "55FDABC3-22AE-4EE4-9883-8234E3298B99" );
            RockMigrationHelper.AddDefinedTypeAttribute( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "9C204CD0-1233-41C5-818A-C5DA439445AA", "MotivatorId", "MotivatorId", "", 1023, "", "8158A336-8129-4E82-8B61-8C0E883CB91A" );
            RockMigrationHelper.AddDefinedTypeAttribute( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Challenge", "Challenge", "", 1030, "", "5C3A012C-19A2-4EC7-8440-7534FE175591" );
            RockMigrationHelper.AddDefinedTypeAttribute( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Influence", "Influence", "", 1029, "", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6" );
            RockMigrationHelper.AddDefinedTypeAttribute( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "D747E6AE-C383-4E22-8846-71518E3DD06F", "Color", "Color", "", 1024, "", "9227E7D4-5725-49BD-A0B1-43B769E0A529" );
            RockMigrationHelper.AddDefinedTypeAttribute( "354715FA-564A-420A-8324-0411988AE7AB", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Attribute Score Key", "AttributeScoreKey", "", 1026, "", "CE3F126E-B56A-438A-BA45-8EC8437BB961" );
            RockMigrationHelper.AddDefinedTypeAttribute( "354715FA-564A-420A-8324-0411988AE7AB", "D747E6AE-C383-4E22-8846-71518E3DD06F", "Color", "Color", "", 1028, "", "8B5F72E4-5A49-4224-9437-82B1F23D8896" );
            RockMigrationHelper.AddDefinedTypeAttribute( "354715FA-564A-420A-8324-0411988AE7AB", "DD7ED4C0-A9E0-434F-ACFE-BD4F56B043DF", "Summary", "Summary", "", 1027, "", "07E85FA1-8F86-4414-8DC3-43D303C55457" );
            RockMigrationHelper.AddAttributeQualifier( "07E85FA1-8F86-4414-8DC3-43D303C55457", "documentfolderroot", "", "A6D6A112-01E9-4675-8D4E-2214219B1B59" );
            RockMigrationHelper.AddAttributeQualifier( "07E85FA1-8F86-4414-8DC3-43D303C55457", "imagefolderroot", "", "94BA3FFE-3DD9-4827-9779-54DE393467BE" );
            RockMigrationHelper.AddAttributeQualifier( "07E85FA1-8F86-4414-8DC3-43D303C55457", "toolbar", "Light", "AB148217-518F-419C-93BE-7CB0C49B9511" );
            RockMigrationHelper.AddAttributeQualifier( "07E85FA1-8F86-4414-8DC3-43D303C55457", "userspecificroot", "False", "B97AC79C-E703-45E7-B802-13BE25413576" );
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
            RockMigrationHelper.AddAttributeQualifier( "8B5F72E4-5A49-4224-9437-82B1F23D8896", "selectiontype", "Color Picker", "96D1A8DA-7E63-41FE-8B0A-0E6239828DBA" );
            RockMigrationHelper.AddAttributeQualifier( "9227E7D4-5725-49BD-A0B1-43B769E0A529", "selectiontype", "Color Picker", "95CCC80C-DCA9-467B-9E42-E3734B6129BC" );
            RockMigrationHelper.AddAttributeQualifier( "A20E6DB1-B830-4D41-9003-43A184E4C910", "allowmultiple", "False", "929F009E-F42F-4893-BD46-34E5945D46BC" );
            RockMigrationHelper.AddAttributeQualifier( "A20E6DB1-B830-4D41-9003-43A184E4C910", "definedtype", "78", "6C084DB5-5EC0-4E73-BAE7-775AE429C852" );
            RockMigrationHelper.AddAttributeQualifier( "A20E6DB1-B830-4D41-9003-43A184E4C910", "displaydescription", "False", "5CA3CB93-B7F0-4C31-8101-0F0AC78AED16" );
            RockMigrationHelper.AddAttributeQualifier( "A20E6DB1-B830-4D41-9003-43A184E4C910", "enhancedselection", "False", "FD29FB8E-E349-4A0C-BF62-856B1AC851E1" );
            RockMigrationHelper.AddAttributeQualifier( "A20E6DB1-B830-4D41-9003-43A184E4C910", "includeInactive", "False", "0008F665-2B5A-49CB-9699-58F72FAC12EF" );
            RockMigrationHelper.AddAttributeQualifier( "CE3F126E-B56A-438A-BA45-8EC8437BB961", "ispassword", "False", "3F4357E7-4644-4CBD-93A8-A98DD1879814" );
            RockMigrationHelper.AddAttributeQualifier( "CE3F126E-B56A-438A-BA45-8EC8437BB961", "maxcharacters", "", "A7F4A5FA-43CF-4B86-B5B0-BE33E98B1C20" );
            RockMigrationHelper.AddAttributeQualifier( "CE3F126E-B56A-438A-BA45-8EC8437BB961", "showcountdown", "False", "20381B01-3B39-4008-83AC-048FF796DF75" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0D82DC77-334C-44B0-84A6-989910907DD4", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorThinking" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0D82DC77-334C-44B0-84A6-989910907DD4", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You can get lost in your thoughts rather than making a decision and moving forward." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0D82DC77-334C-44B0-84A6-989910907DD4", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by your insight into what others may be." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0D82DC77-334C-44B0-84A6-989910907DD4", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F17" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0D82DC77-334C-44B0-84A6-989910907DD4", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#709ac7" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0D82DC77-334C-44B0-84A6-989910907DD4", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"58fef15f-561d-420e-8937-6cf51d296f0e" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "112A35BE-3108-48D9-B057-125A788AB531", "07E85FA1-8F86-4414-8DC3-43D303C55457", @"<p>This theme describes how you lead a team or an organization. The motivators in this theme can be seen in the type of behavior you demonstrate as it relates to the direction or health of the organization or team in which you are engaged. The greater the number of motivators in this theme that you possess in your top five, the more effective you will be in providing direction within the organization.</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "112A35BE-3108-48D9-B057-125A788AB531", "8B5F72E4-5A49-4224-9437-82B1F23D8896", @"#f26863" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "112A35BE-3108-48D9-B057-125A788AB531", "CE3F126E-B56A-438A-BA45-8EC8437BB961", @"core_MotivatorsDirectionalTheme" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2393C3CE-8E49-46FE-A75B-D5D624A37B49", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorTransforming" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2393C3CE-8E49-46FE-A75B-D5D624A37B49", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may experience the discomfort of leaving people behind who do not want to change, while feeling the need to keep moving ahead with the change." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2393C3CE-8E49-46FE-A75B-D5D624A37B49", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by enabling them to feel comfortable and committed to organizational transformation." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2393C3CE-8E49-46FE-A75B-D5D624A37B49", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F18" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2393C3CE-8E49-46FE-A75B-D5D624A37B49", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f26863" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2393C3CE-8E49-46FE-A75B-D5D624A37B49", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"112a35be-3108-48d9-b057-125a788ab531" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3F678404-5844-494F-BDB0-DD9FEEBC98C9", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorMaximizing" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3F678404-5844-494F-BDB0-DD9FEEBC98C9", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may find it difficult to serve others on the team rather than maximizing your own time and effort." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3F678404-5844-494F-BDB0-DD9FEEBC98C9", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others through your strategic sense of when and where to invest resources for maximum impact." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3F678404-5844-494F-BDB0-DD9FEEBC98C9", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F11" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3F678404-5844-494F-BDB0-DD9FEEBC98C9", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f4cf68" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "3F678404-5844-494F-BDB0-DD9FEEBC98C9", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"84322020-4e27-44ef-88f2-eafdb7286a01" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4C898A5C-B48E-4BAE-AB89-835F25A451BF", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorPerceiving" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4C898A5C-B48E-4BAE-AB89-835F25A451BF", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may not know what to do with your insights and may find yourself sharing with others who do not need to know." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4C898A5C-B48E-4BAE-AB89-835F25A451BF", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others through your observations that can be harnessed to help the team be more effective in working together." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4C898A5C-B48E-4BAE-AB89-835F25A451BF", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F14" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4C898A5C-B48E-4BAE-AB89-835F25A451BF", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#709ac7" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4C898A5C-B48E-4BAE-AB89-835F25A451BF", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"58fef15f-561d-420e-8937-6cf51d296f0e" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4D0A1A6D-3F5A-476E-A633-04EAEF457645", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorRisking" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4D0A1A6D-3F5A-476E-A633-04EAEF457645", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You will seldom be satisfied with the status quo and will therefore be easily bored." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4D0A1A6D-3F5A-476E-A633-04EAEF457645", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by stretching them to try something new." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4D0A1A6D-3F5A-476E-A633-04EAEF457645", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F21" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4D0A1A6D-3F5A-476E-A633-04EAEF457645", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f26863" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4D0A1A6D-3F5A-476E-A633-04EAEF457645", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"112a35be-3108-48d9-b057-125a788ab531" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5635E95B-3A07-43B7-837A-0F131EF1DA97", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorEngaging" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5635E95B-3A07-43B7-837A-0F131EF1DA97", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may not always see issues which need addressing in your life because you are so focused on the community." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5635E95B-3A07-43B7-837A-0F131EF1DA97", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by engaging your community’s needs in real and tangible ways to make a difference." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5635E95B-3A07-43B7-837A-0F131EF1DA97", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F05" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5635E95B-3A07-43B7-837A-0F131EF1DA97", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#80bb7c" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5635E95B-3A07-43B7-837A-0F131EF1DA97", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"840c414e-a261-4243-8302-6117e8949fe4" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "58FEF15F-561D-420E-8937-6CF51D296F0E", "07E85FA1-8F86-4414-8DC3-43D303C55457", @"<p>This theme describes how you focus your mind. These motivators can be seen in the way you think or the kind of mental activities you naturally pursue. The way you view your mental activity will be directly influenced by the motivators in this theme. Your conversations will be greatly influenced by these motivators that are in the top five of your profile.</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "58FEF15F-561D-420E-8937-6CF51D296F0E", "8B5F72E4-5A49-4224-9437-82B1F23D8896", @"#709ac7" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "58FEF15F-561D-420E-8937-6CF51D296F0E", "CE3F126E-B56A-438A-BA45-8EC8437BB961", @"core_MotivatorsIntellectualTheme" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "605F3702-6AE7-4545-BEBE-23693E60031C", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorGrowthPropensity" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "605F3702-6AE7-4545-BEBE-23693E60031C", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"PS01" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "605F3702-6AE7-4545-BEBE-23693E60031C", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#6400cb" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6A2354C6-3FA4-4BAD-89A8-7359FEC48FE3", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorLeading" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6A2354C6-3FA4-4BAD-89A8-7359FEC48FE3", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may not feel comfortable just being “one of the team” when you are not the sole leader." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6A2354C6-3FA4-4BAD-89A8-7359FEC48FE3", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by your ability to inspire and engage them to accomplish more together than they could have accomplished individually." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6A2354C6-3FA4-4BAD-89A8-7359FEC48FE3", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F09" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6A2354C6-3FA4-4BAD-89A8-7359FEC48FE3", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f26863" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6A2354C6-3FA4-4BAD-89A8-7359FEC48FE3", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"112a35be-3108-48d9-b057-125a788ab531" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "73087DD2-B892-4367-894F-8922477B2F10", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorGathering" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "73087DD2-B892-4367-894F-8922477B2F10", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may enjoy bringing people together more than actually accomplishing something together." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "73087DD2-B892-4367-894F-8922477B2F10", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by bringing them along with you wherever you go." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "73087DD2-B892-4367-894F-8922477B2F10", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F07" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "73087DD2-B892-4367-894F-8922477B2F10", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#80bb7c" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "73087DD2-B892-4367-894F-8922477B2F10", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"840c414e-a261-4243-8302-6117e8949fe4" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7EA44A56-58CB-4E40-9779-CC0A79772926", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorLearning" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7EA44A56-58CB-4E40-9779-CC0A79772926", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may enjoy learning so much that there is little effort to actually apply what you learn." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7EA44A56-58CB-4E40-9779-CC0A79772926", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by sharing what you are learning in one area and helping them apply it in different areas." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7EA44A56-58CB-4E40-9779-CC0A79772926", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F10" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7EA44A56-58CB-4E40-9779-CC0A79772926", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#709ac7" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "7EA44A56-58CB-4E40-9779-CC0A79772926", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"58fef15f-561d-420e-8937-6cf51d296f0e" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "840C414E-A261-4243-8302-6117E8949FE4", "07E85FA1-8F86-4414-8DC3-43D303C55457", @"<p>This theme describes how you relate to others. These motivators can best be seen as the reasons you build relationships with the people around you, and influence what you value in relationships. The greater the number of motivators in this that you possess in your top five, the more strongly you will be focused on building healthy relationships.</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "840C414E-A261-4243-8302-6117E8949FE4", "8B5F72E4-5A49-4224-9437-82B1F23D8896", @"#80bb7c" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "840C414E-A261-4243-8302-6117E8949FE4", "CE3F126E-B56A-438A-BA45-8EC8437BB961", @"core_MotivatorsRelationalTheme" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "84322020-4E27-44EF-88F2-EAFDB7286A01", "07E85FA1-8F86-4414-8DC3-43D303C55457", @"<p>This theme describes how you execute your role or position within the team. The motivators in this theme can be seen in the way you approach activity, moment by moment. They dramatically influence what you value and how you spend your time or effort at work. When others look at the way you act, your behavior will be greatly determined by the motivators in this theme that are found in your top five.</p>" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "84322020-4E27-44EF-88F2-EAFDB7286A01", "8B5F72E4-5A49-4224-9437-82B1F23D8896", @"#f4cf68" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "84322020-4E27-44EF-88F2-EAFDB7286A01", "CE3F126E-B56A-438A-BA45-8EC8437BB961", @"core_MotivatorsPositionalTheme" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "85459C0F-65A5-48F9-86F3-40B03F9C53E9", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorOrganizing" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "85459C0F-65A5-48F9-86F3-40B03F9C53E9", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may be resistant to change because it could bring unwanted chaos." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "85459C0F-65A5-48F9-86F3-40B03F9C53E9", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by bringing random fragments together to meet your goals." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "85459C0F-65A5-48F9-86F3-40B03F9C53E9", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F12" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "85459C0F-65A5-48F9-86F3-40B03F9C53E9", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f4cf68" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "85459C0F-65A5-48F9-86F3-40B03F9C53E9", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"84322020-4e27-44ef-88f2-eafdb7286a01" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99F598E0-E0AC-4B4B-BEAF-589D41764EE1", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorBelieving" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99F598E0-E0AC-4B4B-BEAF-589D41764EE1", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"Some may see you as inflexible." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99F598E0-E0AC-4B4B-BEAF-589D41764EE1", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others through your convictions." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99F598E0-E0AC-4B4B-BEAF-589D41764EE1", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F01" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99F598E0-E0AC-4B4B-BEAF-589D41764EE1", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#709ac7" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "99F598E0-E0AC-4B4B-BEAF-589D41764EE1", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"58fef15f-561d-420e-8937-6cf51d296f0e" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9F771853-2EBA-47A2-9AC5-26EBEA0A3B25", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorPacing" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9F771853-2EBA-47A2-9AC5-26EBEA0A3B25", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may resist a temporary imbalance that is required to complete a task with excellence." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9F771853-2EBA-47A2-9AC5-26EBEA0A3B25", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by modeling long-term sustainable margins within life and work." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9F771853-2EBA-47A2-9AC5-26EBEA0A3B25", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F13" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9F771853-2EBA-47A2-9AC5-26EBEA0A3B25", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f4cf68" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "9F771853-2EBA-47A2-9AC5-26EBEA0A3B25", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"84322020-4e27-44ef-88f2-eafdb7286a01" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A027F6B2-56DD-4724-962D-F865606AEAB8", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorPersevering" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A027F6B2-56DD-4724-962D-F865606AEAB8", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may not be able to walk away from those situations that are simply not worth the effort." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A027F6B2-56DD-4724-962D-F865606AEAB8", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others through your resilience and perseverance in difficult times." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A027F6B2-56DD-4724-962D-F865606AEAB8", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F20" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A027F6B2-56DD-4724-962D-F865606AEAB8", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f4cf68" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "A027F6B2-56DD-4724-962D-F865606AEAB8", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"84322020-4e27-44ef-88f2-eafdb7286a01" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BD5D99E7-E0FF-4535-8B26-BF73EF9B9F89", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorAdapting" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BD5D99E7-E0FF-4535-8B26-BF73EF9B9F89", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You are so quick to adjust and adapt that you may miss the positive results of perseverance." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BD5D99E7-E0FF-4535-8B26-BF73EF9B9F89", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by incorporating change so that you are better prepared to handle challenges." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BD5D99E7-E0FF-4535-8B26-BF73EF9B9F89", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F06" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BD5D99E7-E0FF-4535-8B26-BF73EF9B9F89", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f4cf68" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BD5D99E7-E0FF-4535-8B26-BF73EF9B9F89", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"84322020-4e27-44ef-88f2-eafdb7286a01" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C171D01E-C607-488B-A550-1E341081210B", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorEmpowering" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C171D01E-C607-488B-A550-1E341081210B", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may not always address negative issues in the lives of those you are developing." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C171D01E-C607-488B-A550-1E341081210B", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by investing in them to do so much more than they could do without your intentional effort." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C171D01E-C607-488B-A550-1E341081210B", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F04" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C171D01E-C607-488B-A550-1E341081210B", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f26863" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C171D01E-C607-488B-A550-1E341081210B", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"112a35be-3108-48d9-b057-125a788ab531" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7601B56-7495-4D7B-A916-8C48F78675E3", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorUniting" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7601B56-7495-4D7B-A916-8C48F78675E3", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may spend so long unifying the team that it hurts the critical progress or momentum." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7601B56-7495-4D7B-A916-8C48F78675E3", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by creating a sense of belonging for every member of the team." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7601B56-7495-4D7B-A916-8C48F78675E3", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F19" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7601B56-7495-4D7B-A916-8C48F78675E3", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#80bb7c" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7601B56-7495-4D7B-A916-8C48F78675E3", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"840c414e-a261-4243-8302-6117e8949fe4" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7F9BDE2-8BEB-469E-BAD9-AA4DEBD3D995", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorRelating" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7F9BDE2-8BEB-469E-BAD9-AA4DEBD3D995", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may form relationships with so many people that you simply cannot maintain integrity and depth in them all." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7F9BDE2-8BEB-469E-BAD9-AA4DEBD3D995", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by building strong ties with those who are socially connected to you." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7F9BDE2-8BEB-469E-BAD9-AA4DEBD3D995", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F15" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7F9BDE2-8BEB-469E-BAD9-AA4DEBD3D995", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#80bb7c" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D7F9BDE2-8BEB-469E-BAD9-AA4DEBD3D995", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"840c414e-a261-4243-8302-6117e8949fe4" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D8430EAD-7A38-4AD1-B21A-B2119EE0F1CD", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorServing" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D8430EAD-7A38-4AD1-B21A-B2119EE0F1CD", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You and those around you may undervalue your contribution to the team." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D8430EAD-7A38-4AD1-B21A-B2119EE0F1CD", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by helping them so they can function within their strengths." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D8430EAD-7A38-4AD1-B21A-B2119EE0F1CD", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F16" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D8430EAD-7A38-4AD1-B21A-B2119EE0F1CD", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f26863" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D8430EAD-7A38-4AD1-B21A-B2119EE0F1CD", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"112a35be-3108-48d9-b057-125a788ab531" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D84E58E4-87FC-4CEB-B83E-A2C6D186366C", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorInnovating" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D84E58E4-87FC-4CEB-B83E-A2C6D186366C", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may enjoy creating so much that there is no execution of a plan to bring that innovation to reality." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D84E58E4-87FC-4CEB-B83E-A2C6D186366C", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by energetically tackling something that may never have been done before." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D84E58E4-87FC-4CEB-B83E-A2C6D186366C", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F08" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D84E58E4-87FC-4CEB-B83E-A2C6D186366C", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f4cf68" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D84E58E4-87FC-4CEB-B83E-A2C6D186366C", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"84322020-4e27-44ef-88f2-eafdb7286a01" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EE1603BA-41AE-4CFA-B220-065768996501", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorVisioning" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EE1603BA-41AE-4CFA-B220-065768996501", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You can tend to live in the future and get frustrated with the realities of the current situation." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EE1603BA-41AE-4CFA-B220-065768996501", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others by inspiring and encouraging them to see much more than their current reality." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EE1603BA-41AE-4CFA-B220-065768996501", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F22" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EE1603BA-41AE-4CFA-B220-065768996501", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#f26863" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "EE1603BA-41AE-4CFA-B220-065768996501", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"112a35be-3108-48d9-b057-125a788ab531" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FA70E27D-6642-4162-AF17-530F66B507E7", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorExpressing" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FA70E27D-6642-4162-AF17-530F66B507E7", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may spend more time speaking to people rather than listening to them." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FA70E27D-6642-4162-AF17-530F66B507E7", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others through speaking and sharing your perspective." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FA70E27D-6642-4162-AF17-530F66B507E7", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F03" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FA70E27D-6642-4162-AF17-530F66B507E7", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#709ac7" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FA70E27D-6642-4162-AF17-530F66B507E7", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"58fef15f-561d-420e-8937-6cf51d296f0e" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FFD7EF9C-5D68-40D2-A362-416B2D660D51", "55FDABC3-22AE-4EE4-9883-8234E3298B99", @"core_MotivatorCaring" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FFD7EF9C-5D68-40D2-A362-416B2D660D51", "5C3A012C-19A2-4EC7-8440-7534FE175591", @"You may become so consumed with meeting immediate needs that you miss long term solutions." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FFD7EF9C-5D68-40D2-A362-416B2D660D51", "7825BDD3-E130-4D63-B0A6-79DB1F97EFD6", @"You influence others with your care and compassion." );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FFD7EF9C-5D68-40D2-A362-416B2D660D51", "8158A336-8129-4E82-8B61-8C0E883CB91A", @"F02" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FFD7EF9C-5D68-40D2-A362-416B2D660D51", "9227E7D4-5725-49BD-A0B1-43B769E0A529", @"#80bb7c" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FFD7EF9C-5D68-40D2-A362-416B2D660D51", "A20E6DB1-B830-4D41-9003-43A184E4C910", @"840c414e-a261-4243-8302-6117e8949fe4" );

        }
             

        private void RestoreBlockTypeAttributesAndValues()
        {

            // Attribute for BlockType: Pledge List:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", @"", 0, @"", "3E26B7DF-7A7F-4829-987F-47304C0F845E" );


            // Attribute for BlockType: Scheduled Transaction List:Add Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "694FF260-8C6F-4A59-93C9-CF3793FE30E6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Add Page", "AddPage", "", @"", 0, @"", "9BCE3FD8-9014-4120-9DCC-06C4936284BA" );


            // Attribute for BlockType: Scheduled Transaction List:Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "694FF260-8C6F-4A59-93C9-CF3793FE30E6", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", @"The type of entity that will provide context for this block", 0, @"", "375F7220-04C6-4E41-B99A-A2CE494FD74A" );


            // Attribute for BlockType: Scheduled Transaction List:View Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "694FF260-8C6F-4A59-93C9-CF3793FE30E6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "View Page", "ViewPage", "", @"", 0, @"", "47B99CD1-FB63-44D7-8586-45BDCDF51137" );


            // Attribute for BlockType: Pledge List:Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", @"The type of entity that will provide context for this block", 0, @"", "E9245CFD-4B11-4CE2-A120-BB3AC47C0974" );


            // Attribute for BlockType: Contribution Statement List Lava:Lava Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "22BF5B51-6511-4D31-8A48-4978A454C386", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", @"The Lava template to use for the contribution statement.", 3, @"{% assign currentYear = 'Now' | Date:'yyyy' %}

<h4>Available Contribution Statements</h4>

<div class=""margin-b-md"">
{% for statementyear in StatementYears %}
    {% if currentYear == statementyear.Year %}
        <a href=""{{ DetailPage }}?StatementYear={{ statementyear.Year }}"" class=""btn btn-primary"">{{ statementyear.Year }} <small>YTD</small></a>
    {% else %}
        <a href=""{{ DetailPage }}?StatementYear={{ statementyear.Year }}"" class=""btn btn-primary"">{{ statementyear.Year }}</a>
    {% endif %}
{% endfor %}
</div>", "7B554631-3CD5-40C4-8E67-ECED56D4D7C1" );


            // Attribute for BlockType: Contribution Statement List Lava:Accounts
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "22BF5B51-6511-4D31-8A48-4978A454C386", "17033CDD-EF97-4413-A483-7B85A787A87F", "Accounts", "Accounts", "", @"A selection of accounts to use for checking if transactions for the current user exist. If no accounts are provided then all tax-deductible accounts will be considered.", 0, @"", "AC1EF7F3-7B06-4978-84DD-B38025FC2E7B" );


            // Attribute for BlockType: Contribution Statement List Lava:Max Years To Display
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "22BF5B51-6511-4D31-8A48-4978A454C386", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Max Years To Display", "MaxYearsToDisplay", "", @"The maximum number of years to display (including the current year).", 1, @"3", "346384B5-1ECE-4949-BFF4-712E1FAA4335" );


            // Attribute for BlockType: Contribution Statement List Lava:Detail Page
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "22BF5B51-6511-4D31-8A48-4978A454C386", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Detail Page", "DetailPage", "", @"The statement detail page.", 2, @"", "5B439A86-D2AD-4223-8D1E-A50FF883D7C2" );


            // Attribute for BlockType: Contribution Statement List Lava:Use Person Context
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "22BF5B51-6511-4D31-8A48-4978A454C386", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Person Context", "UsePersonContext", "", @"Determines if the person context should be used instead of the CurrentPerson.", 5, @"False", "F37EB885-416A-4B70-B48E-8A25557C7B12" );


            // Attribute for BlockType: Contribution Statement List Lava:Entity Type
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "22BF5B51-6511-4D31-8A48-4978A454C386", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", @"The type of entity that will provide context for this block", 0, @"", "F9A168F1-3E59-4C5F-8019-7B17D00B94C9" );


            // Attribute for BlockType: Scheduled Transaction List:Person Token Usage Limit
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "694FF260-8C6F-4A59-93C9-CF3793FE30E6", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Person Token Usage Limit", "PersonTokenUsageLimit", "", @"When adding a new scheduled transaction from a person detail page, the maximum number of times the person token for the transaction can be used.", 4, @"1", "A6B71434-FD9B-45EC-AA50-07AE5D6BA384" );


            // Attribute for BlockType: Scheduled Transaction List:Person Token Expire Minutes
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "694FF260-8C6F-4A59-93C9-CF3793FE30E6", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Person Token Expire Minutes", "PersonTokenExpireMinutes", "", @"When adding a new scheduled transaction from a person detail page, the number of minutes the person token for the transaction is valid after it is issued.", 3, @"60", "ADC80D72-976B-4DFD-B50C-5B2BAC3FFD17" );


            // Attribute for BlockType: Group Finder:Campus Types
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Types", "CampusTypes", "Campus Types", @"Allows selecting which campus types to filter campuses by.", 0, @"", "1E750CAC-4DFF-4FA3-8624-E6DA6BE60C50" );


            // Attribute for BlockType: Group Finder:Campus Statuses
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Campus Statuses", "CampusStatuses", "Campus Statuses", @"This allows selecting which campus statuses to filter campuses by.", 0, @"", "EE4FE3D6-E31F-4CBC-A733-6E3E8D07B049" );


            // Add Block Attribute Value
            //   Block: Pledge List
            //   BlockType: Pledge List
            //   Block Location: Page=Pledges, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: ef7aa296-ca69-49bc-a28b-901a8aaa9466 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "ABEE9BA4-55E4-435E-8CA2-7D1626C57847", "3E26B7DF-7A7F-4829-987F-47304C0F845E", @"ef7aa296-ca69-49bc-a28b-901a8aaa9466" );


            // Add Block Attribute Value
            //   Block: Scheduled Contributions
            //   BlockType: Scheduled Transaction List
            //   Block Location: Page=Scheduled Transactions, Site=Rock RMS
            //   Attribute: View Page
            /*   Attribute Value: 996f5541-d2e1-47e4-8078-80a388203cec */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "32A7BA7B-968E-4BFD-BEA3-042CF863D751", "47B99CD1-FB63-44D7-8586-45BDCDF51137", @"996f5541-d2e1-47e4-8078-80a388203cec" );


            // Add Block Attribute Value
            //   Block: Scheduled Contributions
            //   BlockType: Scheduled Transaction List
            //   Block Location: Page=Scheduled Transactions, Site=Rock RMS
            //   Attribute: Person Token Usage Limit
            /*   Attribute Value: 1 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "32A7BA7B-968E-4BFD-BEA3-042CF863D751", "A6B71434-FD9B-45EC-AA50-07AE5D6BA384", @"1" );


            // Add Block Attribute Value
            //   Block: Scheduled Contributions
            //   BlockType: Scheduled Transaction List
            //   Block Location: Page=Scheduled Transactions, Site=Rock RMS
            //   Attribute: Person Token Expire Minutes
            /*   Attribute Value: 60 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "32A7BA7B-968E-4BFD-BEA3-042CF863D751", "ADC80D72-976B-4DFD-B50C-5B2BAC3FFD17", @"60" );


            // Add Block Attribute Value
            //   Block: Contribution Statement List Lava
            //   BlockType: Contribution Statement List Lava
            //   Block Location: Page=Giving History, Site=External Website
            //   Attribute: Entity Type
            /*   Attribute Value: 72657ed8-d16e-492e-ac12-144c5e7567e7 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "639943D6-75C2-46B4-B044-F4FD7E42E936", "F9A168F1-3E59-4C5F-8019-7B17D00B94C9", @"72657ed8-d16e-492e-ac12-144c5e7567e7" );


            // Add Block Attribute Value
            //   Block: Contribution Statement List Lava
            //   BlockType: Contribution Statement List Lava
            //   Block Location: Page=Giving History, Site=External Website
            //   Attribute: Lava Template
            /*   Attribute Value: ... */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "639943D6-75C2-46B4-B044-F4FD7E42E936", "7B554631-3CD5-40C4-8E67-ECED56D4D7C1", @"{% assign currentYear = 'Now' | Date:'yyyy' %}
<h4>Available Contribution Statements</h4>
<div class=""margin-b-md"">
{% for statementyear in StatementYears %}
 {% if currentYear == statementyear.Year %}
 <a href=""{{ DetailPage }}?StatementYear={{ statementyear.Year }}&PersonGuid={{ PersonGuid }}"" class=""btn btn-primary"">{{ statementyear.Year }} <small>YTD</small></a>
 {% else %}
 <a href=""{{ DetailPage }}?StatementYear={{ statementyear.Year }}&PersonGuid={{ PersonGuid }}"" class=""btn btn-primary"">{{ statementyear.Year }}</a>
 {% endif %}
{% endfor %}
</div>" );


            // Add Block Attribute Value
            //   Block: Contribution Statement List Lava
            //   BlockType: Contribution Statement List Lava
            //   Block Location: Page=Giving History, Site=External Website
            //   Attribute: Accounts
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "639943D6-75C2-46B4-B044-F4FD7E42E936", "AC1EF7F3-7B06-4978-84DD-B38025FC2E7B", @"" );


            // Add Block Attribute Value
            //   Block: Contribution Statement List Lava
            //   BlockType: Contribution Statement List Lava
            //   Block Location: Page=Giving History, Site=External Website
            //   Attribute: Max Years To Display
            /*   Attribute Value: 3 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "639943D6-75C2-46B4-B044-F4FD7E42E936", "346384B5-1ECE-4949-BFF4-712E1FAA4335", @"3" );


            // Add Block Attribute Value
            //   Block: Contribution Statement List Lava
            //   BlockType: Contribution Statement List Lava
            //   Block Location: Page=Giving History, Site=External Website
            //   Attribute: Detail Page
            /*   Attribute Value: fc44fc7f-5ea2-4f0e-8182-d8d6c9c75e28 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "639943D6-75C2-46B4-B044-F4FD7E42E936", "5B439A86-D2AD-4223-8D1E-A50FF883D7C2", @"fc44fc7f-5ea2-4f0e-8182-d8d6c9c75e28" );


            // Add Block Attribute Value
            //   Block: Contribution Statement List Lava
            //   BlockType: Contribution Statement List Lava
            //   Block Location: Page=Giving History, Site=External Website
            //   Attribute: Use Person Context
            /*   Attribute Value: True */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "639943D6-75C2-46B4-B044-F4FD7E42E936", "F37EB885-416A-4B70-B48E-8A25557C7B12", @"True" );


            // Add Block Attribute Value
            //   Block: Scheduled Transaction List
            //   BlockType: Scheduled Transaction List
            //   Block Location: Page=Business Detail, Site=Rock RMS
            //   Attribute: View Page
            /*   Attribute Value: 591204da-b586-454c-8bd5-85652ceaa553 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "91850A29-BB1A-4E92-A798-DE7D6E09E671", "47B99CD1-FB63-44D7-8586-45BDCDF51137", @"591204da-b586-454c-8bd5-85652ceaa553" );


            // Add Block Attribute Value
            //   Block: Scheduled Transaction List
            //   BlockType: Scheduled Transaction List
            //   Block Location: Page=Business Detail, Site=Rock RMS
            //   Attribute: Add Page
            /*   Attribute Value: b1ca86dc-9890-4d26-8ebd-488044e1b3dd */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "91850A29-BB1A-4E92-A798-DE7D6E09E671", "9BCE3FD8-9014-4120-9DCC-06C4936284BA", @"b1ca86dc-9890-4d26-8ebd-488044e1b3dd" );


            // Add Block Attribute Value
            //   Block: Scheduled Transaction List
            //   BlockType: Scheduled Transaction List
            //   Block Location: Page=Business Detail, Site=Rock RMS
            //   Attribute: Entity Type
            /*   Attribute Value: 72657ed8-d16e-492e-ac12-144c5e7567e7 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "91850A29-BB1A-4E92-A798-DE7D6E09E671", "375F7220-04C6-4E41-B99A-A2CE494FD74A", @"72657ed8-d16e-492e-ac12-144c5e7567e7" );


            // Add Block Attribute Value
            //   Block: Contribution Statement List Lava
            //   BlockType: Contribution Statement List Lava
            //   Block Location: Page=Business Detail, Site=Rock RMS
            //   Attribute: Lava Template
            /*   Attribute Value: ... */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "13EF2086-37D4-42FD-B629-6D4292495BC8", "7B554631-3CD5-40C4-8E67-ECED56D4D7C1", @"{% assign yearCount = StatementYears | Size %}
{% if yearCount > 0 %}<hr /><p class=""margin-t-md"">
    <strong><i class='fa fa-file-text-o'></i> Available Contribution Statements</strong>
</p>



{% assign currentYear = 'Now' | Date:'yyyy' %}

<div>
{% for statementyear in StatementYears %}
    {% if currentYear == statementyear.Year %}
        <a href=""{{ DetailPage }}?PersonGuid={{ PersonGuid }}&StatementYear={{ statementyear.Year }}"" class=""btn btn-sm btn-default"">{{ statementyear.Year }} <small>YTD</small></a>
    {% else %}
        <a href=""{{ DetailPage }}?PersonGuid={{ PersonGuid }}&StatementYear={{ statementyear.Year }}"" class=""btn btn-sm btn-default"">{{ statementyear.Year }}</a>
    {% endif %}
{% endfor %}
</div>
{% endif %}" );


            // Add Block Attribute Value
            //   Block: Contribution Statement List Lava
            //   BlockType: Contribution Statement List Lava
            //   Block Location: Page=Business Detail, Site=Rock RMS
            //   Attribute: Accounts
            /*   Attribute Value:  */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "13EF2086-37D4-42FD-B629-6D4292495BC8", "AC1EF7F3-7B06-4978-84DD-B38025FC2E7B", @"" );


            // Add Block Attribute Value
            //   Block: Contribution Statement List Lava
            //   BlockType: Contribution Statement List Lava
            //   Block Location: Page=Business Detail, Site=Rock RMS
            //   Attribute: Use Person Context
            /*   Attribute Value: True */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "13EF2086-37D4-42FD-B629-6D4292495BC8", "F37EB885-416A-4B70-B48E-8A25557C7B12", @"True" );


            // Add Block Attribute Value
            //   Block: Contribution Statement List Lava
            //   BlockType: Contribution Statement List Lava
            //   Block Location: Page=Business Detail, Site=Rock RMS
            //   Attribute: Entity Type
            /*   Attribute Value: 72657ed8-d16e-492e-ac12-144c5e7567e7 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "13EF2086-37D4-42FD-B629-6D4292495BC8", "F9A168F1-3E59-4C5F-8019-7B17D00B94C9", @"72657ed8-d16e-492e-ac12-144c5e7567e7" );


            // Add Block Attribute Value
            //   Block: Contribution Statement List Lava
            //   BlockType: Contribution Statement List Lava
            //   Block Location: Page=Business Detail, Site=Rock RMS
            //   Attribute: Max Years To Display
            /*   Attribute Value: 3 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "13EF2086-37D4-42FD-B629-6D4292495BC8", "346384B5-1ECE-4949-BFF4-712E1FAA4335", @"3" );


            // Add Block Attribute Value
            //   Block: Contribution Statement List Lava
            //   BlockType: Contribution Statement List Lava
            //   Block Location: Page=Business Detail, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: 98ebadaf-cca9-4893-9dd3-d8201d8bd7fa */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "13EF2086-37D4-42FD-B629-6D4292495BC8", "5B439A86-D2AD-4223-8D1E-A50FF883D7C2", @"98ebadaf-cca9-4893-9dd3-d8201d8bd7fa" );


            // Add Block Attribute Value
            //   Block: Pledge List
            //   BlockType: Pledge List
            //   Block Location: Page=Business Detail, Site=Rock RMS
            //   Attribute: Entity Type
            /*   Attribute Value: 72657ed8-d16e-492e-ac12-144c5e7567e7 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "39A2DA08-1995-4A39-A6AF-5F8B8DE7372F", "E9245CFD-4B11-4CE2-A120-BB3AC47C0974", @"72657ed8-d16e-492e-ac12-144c5e7567e7" );


            // Add Block Attribute Value
            //   Block: Pledge List
            //   BlockType: Pledge List
            //   Block Location: Page=Business Detail, Site=Rock RMS
            //   Attribute: Detail Page
            /*   Attribute Value: ef7aa296-ca69-49bc-a28b-901a8aaa9466 */
            //   Skip If Already Exists: true
            RockMigrationHelper.AddBlockAttributeValue( true, "39A2DA08-1995-4A39-A6AF-5F8B8DE7372F", "3E26B7DF-7A7F-4829-987F-47304C0F845E", @"ef7aa296-ca69-49bc-a28b-901a8aaa9466" );


        }

        /// <summary>
        /// Restores the block template attributes.
        /// </summary>
        private void RestoreBlockTemplateAttributes()
        {
            RockMigrationHelper.AddDefinedTypeAttribute( "A6E267E2-66A4-44D7-A5C9-9399666CBF95", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Template Block", "TemplateBlock", "", 1034, "", "0AAFF537-7EC6-4AA9-A987-68DA1FF8511D" );
            RockMigrationHelper.AddDefinedTypeAttribute( "A6E267E2-66A4-44D7-A5C9-9399666CBF95", "97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D", "Icon", "Icon", "", 1035, "", "831403EB-262E-4BC5-8B5E-F16153493BF5" );
            RockMigrationHelper.AddAttributeQualifier( "0AAFF537-7EC6-4AA9-A987-68DA1FF8511D", "allowmultiple", "False", "92AA03B0-D134-4C81-B6F9-982E520CD3B0" );
            Sql( string.Format( @"  DECLARE @AttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '{0}')
  DECLARE @DefinedTypeId int = (SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = '{1}')
                  IF NOT EXISTS (
		                SELECT *
		                FROM AttributeQualifier
		                WHERE [AttributeId] = @AttributeId
                        AND [Key] = 'definedtype'
		                )
                  BEGIN                  
  INSERT INTO [AttributeQualifier] ([IsSystem], [AttributeId], [Key], [Value], [Guid])
  VALUES
  (0, @AttributeId, 'definedtype', @DefinedTypeId, 'B7D9102E-0CEB-4C5A-88FB-CA5C86D06859')
END ", "0AAFF537-7EC6-4AA9-A987-68DA1FF8511D", "0F8E2B71-985E-44C4-BF5A-2FB1AAF3E183" ) );
            RockMigrationHelper.AddAttributeQualifier( "0AAFF537-7EC6-4AA9-A987-68DA1FF8511D", "displaydescription", "False", "325925BF-0A0A-49DD-838E-06D40DDAD77A" );
            RockMigrationHelper.AddAttributeQualifier( "0AAFF537-7EC6-4AA9-A987-68DA1FF8511D", "enhancedselection", "False", "E8C0D449-1CA1-40A1-BB1B-6856A0A3AF11" );
            RockMigrationHelper.AddAttributeQualifier( "0AAFF537-7EC6-4AA9-A987-68DA1FF8511D", "includeInactive", "False", "2BFE679D-9085-4579-9DC4-9269A989E96B" );
            RockMigrationHelper.AddAttributeQualifier( "831403EB-262E-4BC5-8B5E-F16153493BF5", "binaryFileType", "c1142570-8cd6-4a20-83b1-acb47c1cd377", "AE0CCDCC-018D-4F7A-8266-529085A3D97E" );
            RockMigrationHelper.AddAttributeQualifier( "831403EB-262E-4BC5-8B5E-F16153493BF5", "formatAsLink", "False", "74CF1266-5CC0-4694-805B-76220D9293BC" );
            RockMigrationHelper.AddAttributeQualifier( "831403EB-262E-4BC5-8B5E-F16153493BF5", "img_tag_template", "", "3EF02D92-C542-4C1E-AD09-B7D963CF5885" );
        }

        private void RestoreBlockTemplateAttributeValues()
        {
            // Restore the "Template Block" attribute values.
            RockMigrationHelper.AddDefinedValueAttributeValue( "6207af10-b6c9-40b5-8aa5-4c11fa6d0966", "0AAFF537-7EC6-4AA9-A987-68DA1FF8511D", "559346FB-C684-42CF-8F4C-CF4A1C278AD6" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "f093a516-6d95-429e-8eeb-1dfb0303df71", "0AAFF537-7EC6-4AA9-A987-68DA1FF8511D", "09053C7C-9374-4489-8A7B-71F02E3E7D89" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "e3a4aa4e-2a61-4e63-b636-93b86e493d95", "0AAFF537-7EC6-4AA9-A987-68DA1FF8511D", "248587C7-5CE3-46B7-8728-2E03E725D0B2" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2b0f4548-8da7-4236-9bf9-5fa3c07d762f", "0AAFF537-7EC6-4AA9-A987-68DA1FF8511D", "6F1F6BAB-B403-48D1-BF6A-52B16361279C" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "674cf1e3-561c-430d-b4a8-39957ac1bcf1", "0AAFF537-7EC6-4AA9-A987-68DA1FF8511D", "E5618730-9E50-4BDA-9E13-D27697F83980" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "6593d4eb-2b7a-4c24-8d30-a02991d26bc0", "0AAFF537-7EC6-4AA9-A987-68DA1FF8511D", "128F7350-97FD-4ECA-9C79-D02DE0C434EB" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "39b8b16d-d213-46fd-9b8f-710453806193", "0AAFF537-7EC6-4AA9-A987-68DA1FF8511D", "0D588D84-111C-4350-98DE-460C194F5DE5" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "323d1996-c27f-4b48-b0c7-82fda440d950", "0AAFF537-7EC6-4AA9-A987-68DA1FF8511D", "26944B71-7B69-4943-8EC2-3506F728D943" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "91c29610-1d77-49a8-a46b-5a35ec67c551", "0AAFF537-7EC6-4AA9-A987-68DA1FF8511D", "D13256E3-D9ED-45C2-8EF7-C4AABCF4B2B7" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "ded26289-4746-4233-a5bd-d4095248023d", "0AAFF537-7EC6-4AA9-A987-68DA1FF8511D", "198F3006-5F0F-48AB-9EA0-2FA56F633753" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "08009450-92a5-4d4a-8e31-fcc1e4cbdf16", "0AAFF537-7EC6-4AA9-A987-68DA1FF8511D", "30ECA4B6-0869-4656-A4CD-B8729CB29E76" );

            // Restore the "Icon" attribute values.
            RestoreBlockTemplateIcon( "6207af10-b6c9-40b5-8aa5-4c11fa6d0966" );
            RestoreBlockTemplateIcon( "f093a516-6d95-429e-8eeb-1dfb0303df71" );
            RestoreBlockTemplateIcon( "e3a4aa4e-2a61-4e63-b636-93b86e493d95" );
            RestoreBlockTemplateIcon( "2b0f4548-8da7-4236-9bf9-5fa3c07d762f" );
            RestoreBlockTemplateIcon( "674cf1e3-561c-430d-b4a8-39957ac1bcf1" );
            RestoreBlockTemplateIcon( "6593d4eb-2b7a-4c24-8d30-a02991d26bc0" );
            RestoreBlockTemplateIcon( "39b8b16d-d213-46fd-9b8f-710453806193" );
            RestoreBlockTemplateIcon( "323d1996-c27f-4b48-b0c7-82fda440d950" );
            RestoreBlockTemplateIcon( "91c29610-1d77-49a8-a46b-5a35ec67c551" );
            RestoreBlockTemplateIcon( "ded26289-4746-4233-a5bd-d4095248023d" );
            RestoreBlockTemplateIcon( "08009450-92a5-4d4a-8e31-fcc1e4cbdf16" );
        }

        private const string STANDARD_ICON_SVG = "PD94bWwgdmVyc2lvbj0iMS4wIiBlbmNvZGluZz0iVVRGLTgiIHN0YW5kYWxvbmU9Im5vIj8+CjwhRE9DVFlQRSBzdmcgUFVCTElDICItLy9XM0MvL0RURCBTVkcgMS4xLy9FTiIgImh0dHA6Ly93d3cudzMub3JnL0dyYXBoaWNzL1NWRy8xLjEvRFREL3N2ZzExLmR0ZCI+Cjxzdmcgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgdmlld0JveD0iMCAwIDY0MCAyNDAiIHZlcnNpb249IjEuMSIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIiB4bWxuczp4bGluaz0iaHR0cDovL3d3dy53My5vcmcvMTk5OS94bGluayIgeG1sOnNwYWNlPSJwcmVzZXJ2ZSIgeG1sbnM6c2VyaWY9Imh0dHA6Ly93d3cuc2VyaWYuY29tLyIgc3R5bGU9ImZpbGwtcnVsZTpldmVub2RkO2NsaXAtcnVsZTpldmVub2RkO3N0cm9rZS1saW5lam9pbjpyb3VuZDtzdHJva2UtbWl0ZXJsaW1pdDoyOyI+CiAgICA8ZyB0cmFuc2Zvcm09Im1hdHJpeCgxLjEwMTU1LDAsMCwxLC0zMC44NDM0LC0zMSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTY1NTc3LDAsMCwxLC0yNy4wMzYxLDEyKSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wMjA2NSwwLDAsMSwtMjguNTc4Myw1NSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDAuOTg0NTA5LDAsMCwxLC0yNy41NjYzLDk4KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgogICAgPGcgdHJhbnNmb3JtPSJtYXRyaXgoMS4wNTY4LDAsMCwxLC0yOS41OTA0LDE0MSkiPgogICAgICAgIDxyZWN0IHg9IjI4IiB5PSIzMSIgd2lkdGg9IjU4MSIgaGVpZ2h0PSIxOCIgc3R5bGU9ImZpbGw6cmdiKDIzMSwyMzEsMjMxKTsiLz4KICAgIDwvZz4KICAgIDxnIHRyYW5zZm9ybT0ibWF0cml4KDEuMDc5MTcsMCwwLDEsLTMwLjIxNjksMTg0KSI+CiAgICAgICAgPHJlY3QgeD0iMjgiIHk9IjMxIiB3aWR0aD0iNTgxIiBoZWlnaHQ9IjE4IiBzdHlsZT0iZmlsbDpyZ2IoMjMxLDIzMSwyMzEpOyIvPgogICAgPC9nPgo8L3N2Zz4K";

        private void RestoreBlockTemplateIcon( string definedValueGuid )
        {
            // Lifted from MigrationHelper.AddOrUpdateTemplateBlockTemplate
            Sql( $@"
    DECLARE @TemplateDefinedValueId INT = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '{definedValueGuid}')
    DECLARE @IconAttributeId [int] = (SELECT [Id] FROM [Attribute] WHERE ([Guid] = '{SystemGuid.Attribute.DEFINED_TYPE_TEMPLATE_ICON}'))
    DECLARE @DefaultBinaryFileTypeId [int] = (SELECT [Id] FROM [BinaryFileType] WHERE ([Guid] = '{SystemGuid.BinaryFiletype.DEFAULT}'))
    DECLARE @DatabaseStorageEntityTypeId [int] = (SELECT [Id] FROM [EntityType] WHERE ([Guid] = '{SystemGuid.EntityType.STORAGE_PROVIDER_DATABASE}'))
    DECLARE @Now [datetime] = (SELECT GETDATE())
    DECLARE @Base64IconData [nvarchar] (max) = '{STANDARD_ICON_SVG}'
    DECLARE @BinaryFileName [nvarchar] (255) = 'standard-template.svg'
    DECLARE @BinaryFileMimeType [nvarchar] (255) = 'image/svg+xml'
    DECLARE @BinaryFileWidth [int] = NULL
    DECLARE @BinaryFileHeight [int] = NULL

    -----------------------------------------------------------------------------
    -- Manage the 'Icon' records for this Template Block 'Template' DefinedValue
    -----------------------------------------------------------------------------

    DECLARE @IconAttributeValueId [int] = (SELECT [Id] FROM [AttributeValue] WHERE ([AttributeId] = @IconAttributeId AND [EntityId] = @TemplateDefinedValueId))
        , @BinaryIcon [varbinary] (max)
        , @BinaryFileId [int]
        , @AddNewBinaryFile [bit] = 0
        , @BinaryFileDataId [int]
        , @AddNewBinaryFileData [bit] = 0;

    -- Attempt to convert the supplied Icon data to binary
    IF (LEN(@Base64IconData) > 0)
        SET @BinaryIcon = (SELECT CAST(N'' as xml).value('xs:base64Binary(sql:variable(""@Base64IconData""))', 'varbinary(max)'));

    IF (@IconAttributeValueId IS NOT NULL)
    BEGIN
        -- Update (or delete) the existing AttributeValue, BinaryFile and BinaryFileData records
        SET @BinaryFileId = (SELECT [Id]
                             FROM [BinaryFile]
                             WHERE ([Guid] = (SELECT [Value]
                                              FROM [AttributeValue]
                                              WHERE ([Id] = @IconAttributeValueId))));

        -- The [BinaryFileData].[Id] should be the same as the [BinaryFile].[Id], but we still need to check if the BinaryFileData record exists
        IF (@BinaryFileId IS NOT NULL)
            SET @BinaryFileDataId = (SELECT [Id] FROM [BinaryFileData] WHERE ([Id] = @BinaryFileId));

        IF (@BinaryIcon IS NULL)
        BEGIN
            -- If Icon records exist and the caller did not provide a valid Icon, delete the existing records
            IF (@BinaryFileDataId IS NOT NULL)
                DELETE FROM [BinaryFileData] WHERE ([Id] = @BinaryFileDataId);

            IF (@BinaryFileId IS NOT NULL)
                DELETE FROM [BinaryFile] WHERE ([Id] = @BinaryFileId);

            DELETE FROM [AttributeValue] WHERE [Id] = @IconAttributeValueId;
        END
        ELSE
        BEGIN
            -- The caller provided a valid Icon; update the existing records
            -- Also, ensure the BinaryFile record is still using the correct BinaryFileType and StorageEntityType values
            -- If either of the records are not found, set a flag to add them below
            IF (@BinaryFileId IS NOT NULL)
                UPDATE [BinaryFile]
                SET [BinaryFileTypeId] = @DefaultBinaryFileTypeId
                    , [FileName] = @BinaryFileName
                    , [MimeType] = @BinaryFileMimeType
                    , [StorageEntityTypeId] = @DatabaseStorageEntityTypeId
                    , [ModifiedDateTime] = @Now
                    , [Width] = @BinaryFileWidth
                    , [Height] = @BinaryFileHeight
                WHERE ([Id] = @BinaryFileId);
            ELSE
            BEGIN
                SET @AddNewBinaryFile = 1;
                SET @AddNewBinaryFileData = 1;
            END

            IF (@AddNewBinaryFileData = 0 AND @BinaryFileDataId IS NOT NULL)
                UPDATE [BinaryFileData]
                SET [Content] = @BinaryIcon
                    , [ModifiedDateTime] = @Now
                WHERE ([Id] = @BinaryFileDataId);
            ELSE
                SET @AddNewBinaryFileData = 1;
        END
    END
    ELSE
    BEGIN
        IF (@BinaryIcon IS NOT NULL)
        BEGIN
            -- Set flags to add new BinaryFile and BinaryFileData records below
            -- After these are added, we'll add a new AttributeValue record
            SET @AddNewBinaryFile = 1;
            SET @AddNewBinaryFileData = 1;
        END
    END

    IF (@AddNewBinaryFile = 1)
    BEGIN
        -- Add a new BinaryFile record
        DECLARE @BinaryFileGuid [uniqueidentifier] = NEWID();

        INSERT INTO [BinaryFile]
        (
            [IsTemporary]
            , [IsSystem]
            , [BinaryFileTypeId]
            , [FileName]
            , [MimeType]
            , [StorageEntityTypeId]
            , [Guid]
            , [CreatedDateTime]
            , [ModifiedDateTime]
            , [ContentLastModified]
            , [StorageEntitySettings]
            , [Path]
            , [Width]
            , [Height]
        )
        VALUES
        (
            0
            , 1
            , @DefaultBinaryFileTypeId
            , @BinaryFileName
            , @BinaryFileMimeType
            , @DatabaseStorageEntityTypeId
            , @BinaryFileGuid
            , @Now
            , @Now
            , @Now
            , '{{}}'
            , '~/GetImage.ashx?guid=' + (SELECT CONVERT([nvarchar] (50), @BinaryFileGuid))
            , @BinaryFileWidth
            , @BinaryFileHeight
        );

        -- Take note of the newly-added BinaryFile ID
        SET @BinaryFileId = (SELECT SCOPE_IDENTITY());
    END

    
    IF (@AddNewBinaryFileData = 1 AND @BinaryFileId IS NOT NULL)
    BEGIN
        -- Add a new BinaryFileData record whose ID matches the BinaryFile record
        INSERT INTO [BinaryFileData]
        (
            [Id]
            , [Content]
            , [Guid]
            , [CreatedDateTime]
            , [ModifiedDateTime]
        )
        VALUES
        (
            @BinaryFileId
            , @BinaryIcon
            , NEWID()
            , @Now
            , @Now
        );
    END

    -- Finally, add the 'Icon' AttributeValue now that we have added the BinaryFile/Data records
    IF (@IconAttributeValueId IS NULL AND @BinaryFileId IS NOT NULL)
        INSERT INTO [AttributeValue]
        (
            [IsSystem]
            , [AttributeId]
            , [EntityId]
            , [Value]
            , [Guid]
            , [CreatedDateTime]
            , [ModifiedDateTime]
        )
        VALUES
        (
            1
            , @IconAttributeId
            , @TemplateDefinedValueId
            , CONVERT([nvarchar] (50), (SELECT [Guid] FROM [BinaryFile] WHERE ([Id] = @BinaryFileId)))
            , NEWID()
            , @Now
            , @Now
        );
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }

    }
}
