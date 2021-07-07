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
    using Rock;

    /// <summary>
    ///
    /// </summary>
    public partial class AddLanguageDefinedType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Create Defined Type for Languages
            RockMigrationHelper.AddDefinedType( "Global", "Languages", "Languages supported for translation.", SystemGuid.DefinedType.LANGUAGES );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.LANGUAGES, SystemGuid.FieldType.TEXT, "ISO639-1", SystemKey.LanguageDefinedValueAttributeKey.ISO639_1, "The ISO639-1 language code.", 0, true, string.Empty, false, true, SystemGuid.Attribute.ISO639_1 );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.LANGUAGES, SystemGuid.FieldType.TEXT, "ISO639-2", SystemKey.LanguageDefinedValueAttributeKey.ISO639_2, "The ISO639-2 language code.", 0, true, string.Empty, false, true, SystemGuid.Attribute.ISO639_2 );
            RockMigrationHelper.AddDefinedTypeAttribute( SystemGuid.DefinedType.LANGUAGES, SystemGuid.FieldType.TEXT, "Native Language Name", SystemKey.LanguageDefinedValueAttributeKey.NativeLanguageName, "The name of the language in the native language.", 0, true, string.Empty, false, true, SystemGuid.Attribute.NativeLanguageName );

            // Add Defined Values

            // English
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.LANGUAGES, "English", "", "DF0A29A7-A61E-E4A7-4F3D-58CFDD3D3871" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DF0A29A7-A61E-E4A7-4F3D-58CFDD3D3871", SystemGuid.Attribute.ISO639_1, "en" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DF0A29A7-A61E-E4A7-4F3D-58CFDD3D3871", SystemGuid.Attribute.ISO639_2, "eng" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DF0A29A7-A61E-E4A7-4F3D-58CFDD3D3871", SystemGuid.Attribute.NativeLanguageName, "English" );

            // Spanish
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.LANGUAGES, "Spanish", "", "C93CB430-8554-E599-4F49-D7F3CED2B2C7" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C93CB430-8554-E599-4F49-D7F3CED2B2C7", SystemGuid.Attribute.ISO639_1, "es" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C93CB430-8554-E599-4F49-D7F3CED2B2C7", SystemGuid.Attribute.ISO639_2, "spa" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C93CB430-8554-E599-4F49-D7F3CED2B2C7", SystemGuid.Attribute.NativeLanguageName, "Español" );

            // Danish 
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.LANGUAGES, "Danish", "", "467B688C-6EAE-3281-472A-7F39D7A3B45E" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "467B688C-6EAE-3281-472A-7F39D7A3B45E", SystemGuid.Attribute.ISO639_1, "da" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "467B688C-6EAE-3281-472A-7F39D7A3B45E", SystemGuid.Attribute.ISO639_2, "dan" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "467B688C-6EAE-3281-472A-7F39D7A3B45E", SystemGuid.Attribute.NativeLanguageName, "Dansk" );

            // German
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.LANGUAGES, "German", "", "0450DC04-6F03-268A-4FE8-CBBDDD3F1F2A" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0450DC04-6F03-268A-4FE8-CBBDDD3F1F2A", SystemGuid.Attribute.ISO639_1, "de" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0450DC04-6F03-268A-4FE8-CBBDDD3F1F2A", SystemGuid.Attribute.ISO639_2, "deu" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "0450DC04-6F03-268A-4FE8-CBBDDD3F1F2A", SystemGuid.Attribute.NativeLanguageName, "Deutsch" );

            // French
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.LANGUAGES, "French", "", "2E1A430A-BA5A-229E-4CA8-D0C73DBDEA21" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2E1A430A-BA5A-229E-4CA8-D0C73DBDEA21", SystemGuid.Attribute.ISO639_1, "fr" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2E1A430A-BA5A-229E-4CA8-D0C73DBDEA21", SystemGuid.Attribute.ISO639_2, "fra" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2E1A430A-BA5A-229E-4CA8-D0C73DBDEA21", SystemGuid.Attribute.NativeLanguageName, "Française" );

            // Italian
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.LANGUAGES, "Italian", "", "01E4748A-185D-DDAD-4748-3848112B5512" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "01E4748A-185D-DDAD-4748-3848112B5512", SystemGuid.Attribute.ISO639_1, "it" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "01E4748A-185D-DDAD-4748-3848112B5512", SystemGuid.Attribute.ISO639_2, "ita" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "01E4748A-185D-DDAD-4748-3848112B5512", SystemGuid.Attribute.NativeLanguageName, "Italiano" );

            // Japanese 
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.LANGUAGES, "Japanese", "", "DDD324F0-AD96-8D9C-4FF2-746B2A0DBA76" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DDD324F0-AD96-8D9C-4FF2-746B2A0DBA76", SystemGuid.Attribute.ISO639_1, "ja" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DDD324F0-AD96-8D9C-4FF2-746B2A0DBA76", SystemGuid.Attribute.ISO639_2, "jpn" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "DDD324F0-AD96-8D9C-4FF2-746B2A0DBA76", SystemGuid.Attribute.NativeLanguageName, "日本語" );

            // Korean 
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.LANGUAGES, "Korean", "", "FD1421BB-8FCB-4AA4-4831-D7D0B1F51496" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FD1421BB-8FCB-4AA4-4831-D7D0B1F51496", SystemGuid.Attribute.ISO639_1, "ko" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FD1421BB-8FCB-4AA4-4831-D7D0B1F51496", SystemGuid.Attribute.ISO639_2, "kor" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FD1421BB-8FCB-4AA4-4831-D7D0B1F51496", SystemGuid.Attribute.NativeLanguageName, "한국어" );

            // Dutch 
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.LANGUAGES, "Dutch", "", "2033D2B8-B3E4-CF8E-4837-CC42D2F632A9" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2033D2B8-B3E4-CF8E-4837-CC42D2F632A9", SystemGuid.Attribute.ISO639_1, "nl" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2033D2B8-B3E4-CF8E-4837-CC42D2F632A9", SystemGuid.Attribute.ISO639_2, "nld" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2033D2B8-B3E4-CF8E-4837-CC42D2F632A9", SystemGuid.Attribute.NativeLanguageName, "Nederlands" );

            // Norwegian 
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.LANGUAGES, "Norwegian", "", "59635125-7BCF-D193-424A-DC231D9489BF" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "59635125-7BCF-D193-424A-DC231D9489BF", SystemGuid.Attribute.ISO639_1, "no" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "59635125-7BCF-D193-424A-DC231D9489BF", SystemGuid.Attribute.ISO639_2, "nor" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "59635125-7BCF-D193-424A-DC231D9489BF", SystemGuid.Attribute.NativeLanguageName, "Norsk" );

            // Portuguese 
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.LANGUAGES, "Portuguese", "", "671FF957-75EB-52BD-4B88-72B67B348C07" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "671FF957-75EB-52BD-4B88-72B67B348C07", SystemGuid.Attribute.ISO639_1, "pt" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "671FF957-75EB-52BD-4B88-72B67B348C07", SystemGuid.Attribute.ISO639_2, "por" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "671FF957-75EB-52BD-4B88-72B67B348C07", SystemGuid.Attribute.NativeLanguageName, "Português" );

            // Russian 
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.LANGUAGES, "Russian", "", "C46ACD20-EF01-8AB3-4F7F-237A6A72979E" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C46ACD20-EF01-8AB3-4F7F-237A6A72979E", SystemGuid.Attribute.ISO639_1, "ru" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C46ACD20-EF01-8AB3-4F7F-237A6A72979E", SystemGuid.Attribute.ISO639_2, "rus" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C46ACD20-EF01-8AB3-4F7F-237A6A72979E", SystemGuid.Attribute.NativeLanguageName, "Pусский" );

            // Swedish 
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.LANGUAGES, "Swedish", "", "ADB6A75C-5CCB-66BB-4601-8A9AF971B4FC" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "ADB6A75C-5CCB-66BB-4601-8A9AF971B4FC", SystemGuid.Attribute.ISO639_1, "sv" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "ADB6A75C-5CCB-66BB-4601-8A9AF971B4FC", SystemGuid.Attribute.ISO639_2, "swe" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "ADB6A75C-5CCB-66BB-4601-8A9AF971B4FC", SystemGuid.Attribute.NativeLanguageName, "Svenska" );

            // Chinese 
            RockMigrationHelper.AddDefinedValue( SystemGuid.DefinedType.LANGUAGES, "Chinese", "", "4A1B270F-47A0-088A-4083-C74D7B8F63BD" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4A1B270F-47A0-088A-4083-C74D7B8F63BD", SystemGuid.Attribute.ISO639_1, "zh" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4A1B270F-47A0-088A-4083-C74D7B8F63BD", SystemGuid.Attribute.ISO639_2, "zho" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "4A1B270F-47A0-088A-4083-C74D7B8F63BD", SystemGuid.Attribute.NativeLanguageName, "中文" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.ISO639_1 );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.ISO639_2 );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.NativeLanguageName );
            RockMigrationHelper.DeleteDefinedType( SystemGuid.DefinedType.LANGUAGES );
        }
    }
}
