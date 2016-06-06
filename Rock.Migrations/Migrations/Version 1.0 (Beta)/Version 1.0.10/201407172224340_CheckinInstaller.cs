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
    public partial class CheckinInstaller : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddDefinedValue_pre20140819( "1FAC459C-5F62-4E7C-8933-61FF9FE2DFEF", "Rock Windows Check-in Client", "The Windows Check-in Client hosts the Rock check-in screens and allows client based printing to either networked or USB printers.", "C162F21E-7D86-4BB4-A72C-7F4A0E5B02C3" );
            
            Sql( @"
declare
  @attributeIconId int = (select [Id] from [Attribute] where [Guid] = 'C6E82AF0-2128-492B-B5CB-7915630DDA0B'),
  @attributeVendorId int = (select [Id] from [Attribute] where [Guid] = 'E9AAE4D6-B4DC-4AA2-BD86-D63B2B4D26F3'),
  @attributeDownloadUrlId int = (select [Id] from [Attribute] where [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9'),
  @definedValueCheckinClientId int = (select [Id] from [DefinedValue] where [Guid] = 'C162F21E-7D86-4BB4-A72C-7F4A0E5B02C3')

-- SVG Icons
INSERT INTO [dbo].[BinaryFile] ([IsTemporary], [IsSystem], [BinaryFileTypeId], [Url], [FileName], [MimeType], [Description], [StorageEntityTypeId], [Guid]) 
    VALUES 
        (0, 0, 3, N'~/GetFile.ashx?guid=D128A532-A99A-430A-955B-C1444E87C053', N'checkin-client.svg', N'image/svg+xml', null, 51, N'D128A532-A99A-430A-955B-C1444E87C053')

EXEC(N'INSERT INTO [dbo].[BinaryFileData] ([Id], [Content], [Guid]) VALUES ((select [Id] from BinaryFile where [Guid] = N''D128A532-A99A-430A-955B-C1444E87C053''), 0x3C3F786D6C2076657273696F6E3D22312E302220656E636F64696E673D227574662D38223F3E0D0A3C212D2D2047656E657261746F723A2041646F626520496C6C7573747261746F722031362E302E302C20535647204578706F727420506C75672D496E202E205356472056657273696F6E3A20362E3030204275696C6420302920202D2D3E0D0A3C21444F435459504520737667205055424C494320222D2F2F5733432F2F4454442053564720312E312F2F454E222022687474703A2F2F7777772E77332E6F72672F47726170686963732F5356472F312E312F4454442F73766731312E647464223E0D0A3C7376672076657273696F6E3D22312E31222069643D224C617965725F312220786D6C6E733D22687474703A2F2F7777772E77332E6F72672F323030302F7376672220786D6C6E733A786C696E6B3D22687474703A2F2F7777772E77332E6F72672F313939392F786C696E6B2220783D223070782220793D22307078220D0A092077696474683D223230392E35707822206865696768743D223138342E3430367078222076696577426F783D22302030203230392E35203138342E3430362220656E61626C652D6261636B67726F756E643D226E657720302030203230392E35203138342E3430362220786D6C3A73706163653D227072657365727665223E0D0A3C673E0D0A093C636972636C652066696C6C3D2223464646464646222063783D223130372E383233222063793D2239342E3835312220723D2236352E353431222F3E0D0A093C706174682066696C6C3D22233235323532352220643D224D3130362E3732312C3137322E373636632D34342E3236382C302D38302E3138342D33352E3931362D38302E3138342D38302E31383363302D34342E3236382C33352E3931362D38302E3138342C38302E3138342D38302E3138340D0A09096334342E3236372C302C38302E3138332C33352E3931362C38302E3138332C38302E313834433138362E3930332C3133362E38352C3135302E3938372C3137322E3736362C3130362E3732312C3137322E3736367A204D3130362E3732312C33352E3738370D0A0909632D33312E3332322C302D35362E3739372C32352E3437352D35362E3739372C35362E37393763302C33312E33322C32352E3437352C35362E3739352C35362E3739372C35362E3739356333312E33322C302C35362E3739352D32352E3437352C35362E3739352D35362E3739350D0A0909433136332E3531362C36312E3236322C3133382E3034312C33352E3738372C3130362E3732312C33352E3738377A222F3E0D0A093C706174682066696C6C3D22233742433336342220643D224D3130362E3837372C3132392E303436632D322E36312C322E3630392D362E3738372C322E3630392D392E3339372C304C36362E3738352C39382E333531632D322E36312D322E3630392D322E36312D362E3738372C302D392E3339360D0A09096C31302E36352D31302E36343963322E36312D322E3630392C362E3738362D322E3630392C392E3339362C306C31352E3334382C31352E3334376C32382E3731312D32382E37313163322E3630392D322E36312C362E3738372D322E36312C392E3339362C306C31302E3634392C31302E3634390D0A090963322E3630392C322E3630392C322E3630392C362E3738372C302C392E3339364C3130362E3837372C3132392E3034367A222F3E0D0A3C2F673E0D0A3C2F7376673E0D0A, N''F9533ACE-CD63-407E-8877-CA9B98E724FF'')')

INSERT INTO [dbo].[AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
     VALUES 
        (1, @attributeIconId, @definedValueCheckinClientId, 0, (select Id from BinaryFile where Guid = 'D128A532-A99A-430A-955B-C1444E87C053'), 'AE028EAA-E366-4602-AC79-517A5F0D5CA9')

-- Vendor
INSERT INTO [dbo].[AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
     VALUES 
        (1, @attributeVendorId, @definedValueCheckinClientId, 0, 'Spark Development Network', '553E580E-B37D-4503-A845-55B8244E80B7')

-- DownloadUrl
INSERT INTO [dbo].[AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
     VALUES 
        (1, @attributeDownloadUrlId, @definedValueCheckinClientId, 0, 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/windowscheckin/1.0.10/checkinclient.exe', '7ADC1B5B-D374-4B77-9DE1-4D788B572A10')
" );

            // update new location of statementgenerator installer
            Sql( "UPDATE [AttributeValue] set [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/statementgenerator/1.0.10/statementgenerator.exe' where [Guid] = '10BE2E03-7827-41B5-8CB2-DEB473EA107A'" );

            // Attrib for BlockType: Business Detail:Person Profile Page
            RockMigrationHelper.AddBlockTypeAttribute( "3CB1F9F0-11B2-4A46-B9D1-464811E5015C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "", "The page used to view the details of a business contact", 0, @"", "19BA2FF7-09ED-4CD3-9B13-006650EC6C67" );
            
            // Attrib Value for Block:Business Detail, Attribute:Person Profile Page Page: Business Detail, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "77AB2D30-FCBE-45E9-9757-401AE2676A7F", "19BA2FF7-09ED-4CD3-9B13-006650EC6C67", @"08dbd8a5-2c35-4146-b4a8-0f7652348b25" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // un-update new location of statementgenerator installer
            Sql( "UPDATE [AttributeValue] set [Value] = 'http://storage.rockrms.com/externalapplications/sparkdevnetwork/statementgenerator/1.0.3/statementgenerator.exe' where [Guid] = '10BE2E03-7827-41B5-8CB2-DEB473EA107A'" );
            
            Sql( @"DELETE FROM [AttributeValue] where [Guid] = '7ADC1B5B-D374-4B77-9DE1-4D788B572A10'" );
            Sql( @"DELETE FROM [AttributeValue] where [Guid] = '553E580E-B37D-4503-A845-55B8244E80B7'" );
            Sql( @"DELETE FROM [AttributeValue] where [Guid] = 'AE028EAA-E366-4602-AC79-517A5F0D5CA9'" );
            Sql( @"DELETE FROM [BinaryFile] where [Guid] = 'D128A532-A99A-430A-955B-C1444E87C053'" );
            Sql( @"DELETE FROM [DefinedValue] where [Guid] = 'C162F21E-7D86-4BB4-A72C-7F4A0E5B02C3'" );

            // Attrib for BlockType: Business Detail:Person Profile Page
            RockMigrationHelper.DeleteAttribute( "19BA2FF7-09ED-4CD3-9B13-006650EC6C67" );
        }
    }
}
