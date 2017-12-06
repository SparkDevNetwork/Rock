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
    public partial class AddSocialFieldType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Attrib for BlockType: Person Bio:Social Media Category
            RockMigrationHelper.UpdateBlockTypeAttribute( "0F5922BB-CD68-40AC-BF3C-4AAB1B98760C", "309460EF-0CC5-41C6-9161-B3837BA3D374", "Social Media Category", "SocialMediaCategory", "", "The Attribute Category to display attributes from", 14, @"DD8F467D-B83C-444F-B04C-C681167046A1", "FD51EC2E-D660-4B79-95C7-39214D4BEA8E" );
    
            RockMigrationHelper.UpdateFieldType( "Social Media Account", "Used to configure and display the social Network accounts.", "Rock", "Rock.Field.Types.SocialMediaAccountFieldType", Rock.SystemGuid.FieldType.SOCIAL_MEDIA_ACCOUNT );

            Sql( string.Format( @"
    DECLARE @socialAccountFieldType int = ( SELECT [Id] FROM [FieldType] WHERE [Guid] = '{0}' )
    UPDATE [Attribute] SET 
        [IconCssClass] = '',
        [FieldTypeId] = @socialAccountFieldType
    WHERE [Guid] in ('{1}','{2}','{3}')
",          Rock.SystemGuid.FieldType.SOCIAL_MEDIA_ACCOUNT, 
            Rock.SystemGuid.Attribute.PERSON_FACEBOOK,
            Rock.SystemGuid.Attribute.PERSON_TWITTER, 
            Rock.SystemGuid.Attribute.PERSON_INSTAGRAM ) );

            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_FACEBOOK, "name", "Facebook", "ECFCD6FC-6E2C-40CC-8F60-DFA256C29C7A" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_FACEBOOK, "iconcssclass", "fa fa-facebook", "FC271703-58E5-45EC-8A48-B0D5B58C606A" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_FACEBOOK, "color", "#44619d", "4FF8C097-9C2D-4022-BF83-A110A1DEE56C" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_FACEBOOK, "texttemplate", "<a href=''{{value}}'' target=''_blank''>{{ value | Url:''segments'' | Last }}</a>", "BC8F9FEF-59D6-4BC4-84B3-BC6EC52CECED" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_FACEBOOK, "baseurl", "http://www.facebook.com/", "204A1861-D7DA-4C0C-94E7-81B19DDCD4F4" );

            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_TWITTER, "name", "Twitter", "E8B59009-DD08-4B8E-8A86-8602D45E2BDA" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_TWITTER, "iconcssclass", "fa fa-twitter", "5FC5E5F8-EB4B-44F0-943C-0CD487191FF3" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_TWITTER, "color", "#55acee", "5F9E35DA-B8E9-4D0A-8FEB-F5B5BB9A43E7" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_TWITTER, "texttemplate", "<a href=''{{value}}'' target=''_blank''>{{ value | Url:''segments'' | Last }}</a>", "6FFF488B-C7A8-410A-ADC2-3D9D21706511" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_TWITTER, "baseurl", "http://www.twitter.com/", "57FB78D1-A1F3-48F0-AE38-4EAC65732140" );

            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_INSTAGRAM, "name", "Instagram", "D848B8A8-E8AA-42FE-92A6-218135DDC426" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_INSTAGRAM, "iconcssclass", "fa fa-instagram", "FB1E8A30-7299-4208-8AAF-2C5051456D46" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_INSTAGRAM, "color", "#39688f", "A8CE78E3-AE6F-4F60-A63E-4E44BD420A4E" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_INSTAGRAM, "texttemplate", "<a href=''{{value}}'' target=''_blank''>{{ value | Url:''segments'' | Last }}</a>", "02820F4F-476A-448F-A869-14206625670C" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_INSTAGRAM, "baseurl", "http://www.instagram.com/", "A00E1DE5-F37F-4CBE-8FE4-F89CBC1AE055" );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.SOCIAL_MEDIA_ACCOUNT, Rock.SystemGuid.Category.PERSON_ATTRIBUTES_SOCIAL, "Snapchat", "Snapchat", "", "Link to person's Snapchat page", 3, "", Rock.SystemGuid.Attribute.PERSON_SNAPCHAT );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_SNAPCHAT, "name", "Snapchat", "19283998-F3D7-41A1-B9F5-36FE17CC4566" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_SNAPCHAT, "iconcssclass", "fa fa-snapchat-ghost text-shadow", "E9168011-2719-40EB-A082-9337B5F52233" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_SNAPCHAT, "color", "#FFFC00", "C396FB2E-E38B-4D9E-9DFC-3BC9F2D04C9A" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_SNAPCHAT, "texttemplate", "<a href=''{{value}}'' target=''_blank''>{{ value | Url:''segments'' | Last }}</a>", "7B3650EF-8F42-40DF-A729-9BEF19941DD8" );
            RockMigrationHelper.UpdateAttributeQualifier( Rock.SystemGuid.Attribute.PERSON_SNAPCHAT, "baseurl", "http://www.snapchat.com/", "E2115559-2B43-4630-8A0F-7F1B0141D62C" );
        }
             
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.PERSON_SNAPCHAT );
            RockMigrationHelper.DeleteAttribute( "FD51EC2E-D660-4B79-95C7-39214D4BEA8E" );
        }
    }
}
