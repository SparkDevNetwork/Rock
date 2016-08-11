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
    public partial class CheckinEnhancement : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.Location", "Barcode", c => c.String( maxLength: 40 ) );
            AddColumn( "dbo.Location", "SoftRoomThreshold", c => c.Int() );
            AddColumn( "dbo.Location", "FirmRoomThreshold", c => c.Int() );

            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "GroupTypePurposeValueId", "", "Check-in Type", "", 0, "1", "90C34D24-7CFB-4A52-B39C-DFF05A40997C", "core_checkin_CheckInType" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "GroupTypePurposeValueId", "", "Enable Manager Option", "", 0, "True", "5BF4C3CD-052F-4A21-B677-21811C5ABEDD", "core_checkin_EnableManagerOption" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "GroupTypePurposeValueId", "", "Enable Override", "", 0, "True", "745154D6-E108-41C2-9001-7AD543CFC75D", "core_checkin_EnableOverride" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "GroupTypePurposeValueId", "", "Security Code Length", "", 0, "3", "712CFC8A-7B67-4793-A71E-E2EEB2D1048D", "core_checkin_SecurityCodeLength" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "GroupTypePurposeValueId", "", "Reuse Same Code", "", 0, "False", "2B1E044B-6BD7-4F91-86A1-2D854A3BBF2D", "core_checkin_ReuseSameCode" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "GroupTypePurposeValueId", "", "One Parent Label", "", 0, "False", "EC7FA927-95D0-44A8-8AB3-2D74A9FA2F26", "core_checkin_OneParentLabel" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "GroupTypePurposeValueId", "", "Search Type", "", 0, "f3f66040-c50f-4d13-9652-780305fffe23", "BBF88ADA-3C2E-4A0B-A9AE-5B07D4557129", "core_checkin_SearchType" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "9C204CD0-1233-41C5-818A-C5DA439445AA", "GroupTypePurposeValueId", "", "Regular Expression Filter", "", 0, "", "DE32D84F-5653-41F9-9B34-D04BA9024670", "core_checkin_RegularExpressionFilter" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "GroupTypePurposeValueId", "", "Max Search Results", "", 0, "100", "24A8A4B0-F54D-490A-89F6-476B044CD114", "core_checkin_MaxSearchResults" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "GroupTypePurposeValueId", "", "Minimum Phone Search Length", "", 0, "4", "DA3417AC-7138-4219-9363-7AB37D614350", "core_checkin_MinimumPhoneSearchLength" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "GroupTypePurposeValueId", "", "Maximum Phone Search Length", "", 0, "10", "93CA081A-6128-4BBE-BF2B-DF55B7AA817C", "core_checkin_MaximumPhoneSearchLength" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "GroupTypePurposeValueId", "", "Phone Search Type", "", 0, "1", "34D0971A-53AB-4D43-94EA-E251081D7F93", "core_checkin_PhoneSearchType" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "GroupTypePurposeValueId", "", "Refresh Interval", "", 0, "10", "C99D34BF-711B-4865-84B4-B0929C92D16C", "core_checkin_RefreshInterval" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "GroupTypePurposeValueId", "", "Age Required", "", 0, "True", "46C8DC94-D57E-4B9A-8FB9-1A797DD3D525", "core_checkin_AgeRequired" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "GroupTypePurposeValueId", "", "Display Location Count", "", 0, "True", "17DA47FF-EC64-4A97-B46B-394626C25100", "core_checkin_DisplayLocationCount" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.GroupType", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "GroupTypePurposeValueId", "", "Auto Select Days Back", "", 0, "10", "5BA86237-B327-4A2E-8992-6AE784B2A41C", "core_checkin_AutoSelectDaysBack" );

            RockMigrationHelper.UpdateAttributeQualifier( "90C34D24-7CFB-4A52-B39C-DFF05A40997C", "fieldtype", "ddl", "6AF5B4B1-8195-4516-B1FE-79705847B8D0" );
            RockMigrationHelper.UpdateAttributeQualifier( "90C34D24-7CFB-4A52-B39C-DFF05A40997C", "values", "0^Individual,1^Family", "1594AB67-5C71-44C6-ABEA-397741193C13" );
            RockMigrationHelper.UpdateAttributeQualifier( "5BF4C3CD-052F-4A21-B677-21811C5ABEDD", "falsetext", "No", "2B6CC1F3-7406-4E98-81A1-6A8E1D5498BC" );
            RockMigrationHelper.UpdateAttributeQualifier( "5BF4C3CD-052F-4A21-B677-21811C5ABEDD", "truetext", "Yes", "DD7C6C2F-5A9A-463E-93CA-F2D586FD66D9" );
            RockMigrationHelper.UpdateAttributeQualifier( "745154D6-E108-41C2-9001-7AD543CFC75D", "falsetext", "No", "4550571C-FBEC-4AAA-B4FB-DE46A026D339" );
            RockMigrationHelper.UpdateAttributeQualifier( "745154D6-E108-41C2-9001-7AD543CFC75D", "truetext", "Yes", "79C7E1F1-2E72-49F9-8997-C0843B991FBC" );
            RockMigrationHelper.UpdateAttributeQualifier( "2B1E044B-6BD7-4F91-86A1-2D854A3BBF2D", "falsetext", "No", "82E9F4E5-46CB-4CDF-960D-5A51E8CB45B7" );
            RockMigrationHelper.UpdateAttributeQualifier( "2B1E044B-6BD7-4F91-86A1-2D854A3BBF2D", "truetext", "Yes", "7FCE46BD-9A5D-4690-8C18-35F1944847F5" );
            RockMigrationHelper.UpdateAttributeQualifier( "EC7FA927-95D0-44A8-8AB3-2D74A9FA2F26", "falsetext", "No", "2E70DFC8-8060-4434-A9A3-5296A6B93DBF" );
            RockMigrationHelper.UpdateAttributeQualifier( "EC7FA927-95D0-44A8-8AB3-2D74A9FA2F26", "truetext", "Yes", "06829171-7914-4586-B9B7-561F7BE4CF25" );
            RockMigrationHelper.UpdateAttributeQualifier( "BBF88ADA-3C2E-4A0B-A9AE-5B07D4557129", "allowmultiple", "False", "47685D73-D7FD-47E7-B1AE-7CB288EBA68D" );
            RockMigrationHelper.UpdateAttributeQualifier( "BBF88ADA-3C2E-4A0B-A9AE-5B07D4557129", "definedtype", "20", "504A6A25-40D1-4D6C-AECD-F27445DEA07D" );
            RockMigrationHelper.UpdateAttributeQualifier( "BBF88ADA-3C2E-4A0B-A9AE-5B07D4557129", "displaydescription", "False", "130B1BA3-BD02-4298-B313-CC98549D0414" );
            RockMigrationHelper.UpdateAttributeQualifier( "DE32D84F-5653-41F9-9B34-D04BA9024670", "ispassword", "False", "0431ED1A-73A3-4BC9-8E41-C394EA92B69B" );
            RockMigrationHelper.UpdateAttributeQualifier( "34D0971A-53AB-4D43-94EA-E251081D7F93", "fieldtype", "ddl", "B38CF989-BAAD-4636-BFD0-6C620E764591" );
            RockMigrationHelper.UpdateAttributeQualifier( "34D0971A-53AB-4D43-94EA-E251081D7F93", "values", "0^Contains,1^Ends With", "1A60F90C-5ADE-454E-9FDE-741E9F879840" );
            RockMigrationHelper.UpdateAttributeQualifier( "46C8DC94-D57E-4B9A-8FB9-1A797DD3D525", "falsetext", "No", "8F5FF24C-21B6-49A2-B7CF-C1D014CA8102" );
            RockMigrationHelper.UpdateAttributeQualifier( "46C8DC94-D57E-4B9A-8FB9-1A797DD3D525", "truetext", "Yes", "22B1DADA-835D-45A8-99DE-315FF277122F" );
            RockMigrationHelper.UpdateAttributeQualifier( "17DA47FF-EC64-4A97-B46B-394626C25100", "falsetext", "No", "183E12A4-6123-4955-88F5-7696D8398410" );
            RockMigrationHelper.UpdateAttributeQualifier( "17DA47FF-EC64-4A97-B46B-394626C25100", "truetext", "Yes", "CE99805D-AAA1-440A-B0AF-DBF22E77CC49" );

            Sql( MigrationSQL._201604251529438_CheckinEnhancement );

            // Remove old page/blocks
            RockMigrationHelper.DeleteBlock( "883CE93C-2AF9-443B-9531-B8E5277D3CEA" );
            RockMigrationHelper.DeleteBlock( "5F2EA21F-CB8A-4A6B-9E33-A3D8570DC716" );
            RockMigrationHelper.DeletePage( "4AB679AF-C8CC-427C-A615-0BF9F52E8E3E" );

            RockMigrationHelper.UpdateBlockType( "Check-in Type Detail", "Displays the details of a particular Check-in Type.", "~/Blocks/CheckIn/Config/CheckinTypeDetail.ascx", "Check-in > Configuration", "6CB1416A-3B25-41FD-8E60-1B94F4A64AE6" );
            RockMigrationHelper.UpdateBlockType( "Check-in Types", "Displays the check-in types.", "~/Blocks/CheckIn/Config/CheckinTypes.ascx", "Check-in > Configuration", "50029382-75A6-4B73-9644-880845B3116A" );
            RockMigrationHelper.UpdateBlockType( "Check-in Areas", "Configure Check-in areas and groups.", "~/Blocks/CheckIn/Config/CheckinAreas.ascx", "Check-in > Configuration", "B7CD296F-3AAB-4BA3-902C-44DB96C79798" );

            // Add Block to Page: Check-in Configuration, Site: Rock RMS
            RockMigrationHelper.AddBlock( "C646A95A-D12D-4A67-9BE6-C9695C0267ED", "", "50029382-75A6-4B73-9644-880845B3116A", "Check-in Types", "Main", "", "", 0, "72578C6C-3970-4AE7-A528-AFC761EA578A" );
            // Add Block to Page: Check-in Type, Site: Rock RMS
            RockMigrationHelper.AddBlock( "C646A95A-D12D-4A67-9BE6-C9695C0267ED", "", "6CB1416A-3B25-41FD-8E60-1B94F4A64AE6", "Check-in Type Detail", "Main", "", "", 0, "71C3B7F8-E35B-498A-B03E-3C547794C881" );
            // Add Block to Page: Check-in Type, Site: Rock RMS
            RockMigrationHelper.AddBlock( "C646A95A-D12D-4A67-9BE6-C9695C0267ED", "", "B7CD296F-3AAB-4BA3-902C-44DB96C79798", "Check-in Areas", "Main", "", "", 1, "DB03DADC-36D8-4135-B339-DCE1A02772A8" );

            // Attrib for BlockType: Check-in Type Detail:Schedule Page
            RockMigrationHelper.AddBlockTypeAttribute( "6CB1416A-3B25-41FD-8E60-1B94F4A64AE6", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Schedule Page", "SchedulePage", "", "Page used to manage schedules for the check-in type.", 0, @"", "2819D6CE-C8BF-4E7A-88DF-7F1E4E322AFC" );
            // Attrib Value for Block:Check-in Type Detail, Attribute:Schedule Page Page: Check-in Type, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "71C3B7F8-E35B-498A-B03E-3C547794C881", "2819D6CE-C8BF-4E7A-88DF-7F1E4E322AFC", @"a286d16b-fddf-4d89-b98f-d51188b611e6" );

            // Migration Rollups
            Sql( MigrationSQL._201604251529438_UpdateLegacyLava );

            // add new global attribute to determine Lava Support Level
            RockMigrationHelper.AddGlobalAttribute( SystemGuid.FieldType.SINGLE_SELECT, "", "", "Lava Support Level", 
@"Legacy - Old Lava syntax will still be supported without any warnings
LegacyWithWarning - Old Lava syntax will be supported, but a warning will be logged to the exceptions log to help identify lava that needs to be updated
NoLegacy - Old Lava syntax will be ignored and not loaded. (Best Performance)",
                0, "Legacy", "C8E30F2B-7476-4B02-86D4-3E5057F03FD5", "core.LavaSupportLevel" );

            Sql( @"  
    DECLARE @LavaSupportLevelAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'C8E30F2B-7476-4B02-86D4-3E5057F03FD5')
    INSERT INTO [AttributeQualifier] 
     ([IsSystem], [AttributeId], [Key], [Value], [Guid])
     VALUES
      (0, @LavaSupportLevelAttributeId, 'fieldtype', 'ddl', newid())
    INSERT INTO [AttributeQualifier] 
     ([IsSystem], [AttributeId], [Key], [Value], [Guid])
     VALUES
      (0, @LavaSupportLevelAttributeId, 'values', 'Legacy, LegacyWithWarning, NoLegacy', newid())
" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.Location", "FirmRoomThreshold");
            DropColumn("dbo.Location", "SoftRoomThreshold");
            DropColumn("dbo.Location", "Barcode");

            RockMigrationHelper.DeleteAttribute( "2819D6CE-C8BF-4E7A-88DF-7F1E4E322AFC" );
            RockMigrationHelper.DeleteBlock( "DB03DADC-36D8-4135-B339-DCE1A02772A8" );
            RockMigrationHelper.DeleteBlock( "71C3B7F8-E35B-498A-B03E-3C547794C881" );
            RockMigrationHelper.DeleteBlock( "72578C6C-3970-4AE7-A528-AFC761EA578A" );

            RockMigrationHelper.DeleteBlockType( "B7CD296F-3AAB-4BA3-902C-44DB96C79798" );
            RockMigrationHelper.DeleteBlockType( "50029382-75A6-4B73-9644-880845B3116A" );
            RockMigrationHelper.DeleteBlockType( "6CB1416A-3B25-41FD-8E60-1B94F4A64AE6" );

            RockMigrationHelper.DeleteAttribute( "90C34D24-7CFB-4A52-B39C-DFF05A40997C" );
            RockMigrationHelper.DeleteAttribute( "5BF4C3CD-052F-4A21-B677-21811C5ABEDD" );
            RockMigrationHelper.DeleteAttribute( "745154D6-E108-41C2-9001-7AD543CFC75D" );
            RockMigrationHelper.DeleteAttribute( "712CFC8A-7B67-4793-A71E-E2EEB2D1048D" );
            RockMigrationHelper.DeleteAttribute( "2B1E044B-6BD7-4F91-86A1-2D854A3BBF2D" );
            RockMigrationHelper.DeleteAttribute( "EC7FA927-95D0-44A8-8AB3-2D74A9FA2F26" );
            RockMigrationHelper.DeleteAttribute( "BBF88ADA-3C2E-4A0B-A9AE-5B07D4557129" );
            RockMigrationHelper.DeleteAttribute( "DE32D84F-5653-41F9-9B34-D04BA9024670" );
            RockMigrationHelper.DeleteAttribute( "24A8A4B0-F54D-490A-89F6-476B044CD114" );
            RockMigrationHelper.DeleteAttribute( "DA3417AC-7138-4219-9363-7AB37D614350" ); 
            RockMigrationHelper.DeleteAttribute( "93CA081A-6128-4BBE-BF2B-DF55B7AA817C" );
            RockMigrationHelper.DeleteAttribute( "34D0971A-53AB-4D43-94EA-E251081D7F93" );
            RockMigrationHelper.DeleteAttribute( "C99D34BF-711B-4865-84B4-B0929C92D16C" );
            RockMigrationHelper.DeleteAttribute( "46C8DC94-D57E-4B9A-8FB9-1A797DD3D525" );
            RockMigrationHelper.DeleteAttribute( "17DA47FF-EC64-4A97-B46B-394626C25100" );
            RockMigrationHelper.DeleteAttribute( "5BA86237-B327-4A2E-8992-6AE784B2A41C" );
        }
    }
}
