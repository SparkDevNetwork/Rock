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
    public partial class LiquidTemplateDefinedType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddDefinedType( "Global", "Liquid Templates", "Defined type to store various liquid templates for features in Rock.", "C3D44004-6951-44D9-8560-8567D705A48B" );
            RockMigrationHelper.AddDefinedTypeAttribute( "C3D44004-6951-44D9-8560-8567D705A48B", "9C204CD0-1233-41C5-818A-C5DA439445AA", "MIME Type", "MimeType", "The MIME type that should be returned when using the template.", 1, "", "4FBF9D1A-06A4-4941-B9F4-85D2C4C12F2A" );
            RockMigrationHelper.AddDefinedTypeAttribute( "C3D44004-6951-44D9-8560-8567D705A48B", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Template", "Template", "", 2, "", "1E13E409-B568-45D0-B4B6-556C87D61232" );
            RockMigrationHelper.AddAttributeQualifier( "1E13E409-B568-45D0-B4B6-556C87D61232", "editorMode", "3", "F28A4FD3-BFEE-4617-88A3-2132E71ADAD5" );
            RockMigrationHelper.AddAttributeQualifier( "1E13E409-B568-45D0-B4B6-556C87D61232", "editorTheme", "0", "9E9DFC12-81CB-4C71-A7E8-9F74E29CEA46" );
            RockMigrationHelper.AddAttributeQualifier( "1E13E409-B568-45D0-B4B6-556C87D61232", "editorHeight", "600", "6543D502-A6C5-4754-9755-AC6F88DF6CEA" );

            RockMigrationHelper.AddDefinedValue( "C3D44004-6951-44D9-8560-8567D705A48B", "Default RSS Channel", "The default RSS channel template used by the RSS provider if one is not provided.", "D6149581-9EFC-40D8-BD38-E92C0717BEDA" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D6149581-9EFC-40D8-BD38-E92C0717BEDA", "4FBF9D1A-06A4-4941-B9F4-85D2C4C12F2A", "application/rss+xml" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D6149581-9EFC-40D8-BD38-E92C0717BEDA", "1E13E409-B568-45D0-B4B6-556C87D61232", @"{% assign timezone = 'Now' | Date:'zzz' | Replace:':','' -%}
<?xml version=""1.0"" encoding=""utf-8""?>
<rss version=""2.0"" xmlns:atom=""http://www.w3.org/2005/Atom"">

<channel>
    <title>{{ Channel.Name }}</title>
    <link>{{ Channel.ChannelUrl }}</link>
    <description>{{ Channel.Description }}</description>
    <language>en-us</language>
    <ttl>{{ Channel.TimeToLive }}</ttl>
    <lastBuildDate>{{ 'Now' | Date:'ddd, dd MMM yyyy HH:mm:00' }} {{ timezone }}</lastBuildDate>
{% for item in Items -%}
    <item>
        <title>{{ item.Title }}</title>
        <guid>{{ Channel.ItemUrl }}?Item={{ item.Id }}</guid>
        <link>{{ Channel.ItemUrl }}?Item={{ item.Id }}</link>
        <pubDate>{{ item.StartDateTime | Date:'ddd, dd MMM yyyy HH:mm:00' }} {{ timezone }}</pubDate>
        <description>{{ item.Content | Escape }}</description>
    </item>
{% endfor -%}

</channel>
</rss>" );

            // update new location of statementgenerator installer
            Sql( "UPDATE [AttributeValue] set [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/statementgenerator/1.1.2/statementgenerator.exe' where [Guid] = '10BE2E03-7827-41B5-8CB2-DEB473EA107A'" );

            // update to computed column to handle ISO 8601 formatted values
            Sql( @"
    ALTER TABLE [dbo].[AttributeValue] DROP COLUMN [ValueAsDateTime]
    ALTER TABLE [dbo].[AttributeValue] ADD [ValueAsDateTime] AS CASE WHEN [value] LIKE '____-__-__T__:__:__%' THEN CONVERT(datetime, CONVERT(datetimeoffset, [value])) ELSE CASE WHEN (LEN([value]) < 50 AND ISDATE( [value]) = 1) THEN CONVERT(datetime, [value]) ELSE NULL END END
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute("4FBF9D1A-06A4-4941-B9F4-85D2C4C12F2A");
            RockMigrationHelper.DeleteAttribute( "1E13E409-B568-45D0-B4B6-556C87D61232" );
            RockMigrationHelper.DeleteDefinedType("C3D44004-6951-44D9-8560-8567D705A48B");
        }
    }
}
