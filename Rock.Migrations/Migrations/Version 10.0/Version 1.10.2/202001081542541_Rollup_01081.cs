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
    public partial class Rollup_01081 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateAssessmentMotivatorThinking();
            SmsSettingsUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            SmsSettingsDown();
        }

        /// <summary>
        /// ED: Update Person Motivator value "Thinking" to "Meta-Thinking"
        /// </summary>
        private void UpdateAssessmentMotivatorThinking()
        {
            RockMigrationHelper.UpdateDefinedValue( "1DFF1804-0055-491E-9559-54EA3F8F89D1", "Meta-Thinking", "You are intentionally aware of your thoughts at any given moment. You have the capacity to consciously evaluate about your patterns of thought. You understand what is going on in your mind and why you are thinking the way you are. You enjoy internal reflection. You have a desire to understand why others respond as they do and to fit that into your logical mental framework.", "0D82DC77-334C-44B0-84A6-989910907DD4", false );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.DECIMAL, "B08A3096-FCFA-4DA0-B95D-1F3F11CC9969", "Motivator Meta-Thinking", "core_MotivatorThinking", "", "", 0, "", SystemGuid.Attribute.PERSON_MOTIVATOR_THINKING );
        }

        /// <summary>
        /// SK: Block Template Picker Control, Field Type & Attribute
        /// </summary>
        private void SmsSettingsUp()
        {
            RockMigrationHelper.AddDefinedType( "CMS Settings", "Template Block", "List of blocks which have built-in support for BlockTemplateFields. The block attribute uses the well-known Guid of a particular defined value like this: <code>[BlockTemplateField( \"ABC12345-1111-1111-1111-ABC123000000\", \"Format Templates\", \"This template controls the...\" )]</code>", "0F8E2B71-985E-44C4-BF5A-2FB1AAF3E183", @"" );

            RockMigrationHelper.AddDefinedType( "CMS Settings", "Template", "List of Templates that can be used by the corresponding Template Blocks that Rock supports.", "A6E267E2-66A4-44D7-A5C9-9399666CBF95", @"" );
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
        
        /// <summary>
        /// SK: Block Template Picker Control, Field Type & Attribute
        /// </summary>
        private void SmsSettingsDown()
        {
            RockMigrationHelper.DeleteAttribute( "0AAFF537-7EC6-4AA9-A987-68DA1FF8511D" ); // TemplateBlock
            RockMigrationHelper.DeleteAttribute( "831403EB-262E-4BC5-8B5E-F16153493BF5" ); // Icon
            RockMigrationHelper.DeleteDefinedType( "A6E267E2-66A4-44D7-A5C9-9399666CBF95" ); // Template

            RockMigrationHelper.DeleteDefinedType( "0F8E2B71-985E-44C4-BF5A-2FB1AAF3E183" ); // Template Block
        }
    }
}
