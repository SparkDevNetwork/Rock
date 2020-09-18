// <copyright>
// Copyright by BEMA Software Services
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
using System;
using System.Data.Entity;
using System.Linq;
using System.Text;
using com.bemaservices.RoomManagement.ReportTemplates;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Plugin;
using Rock.Web.Cache;

namespace com.bemaservices.RoomManagement.Migrations
{
    /// <summary>
    /// Migration for the RoomManagement system.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 22, "1.9.4" )]
    public class ReservationReports : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //throw new NotImplementedException();
            UpdateFieldTypeByGuid( "Report Template", "", "com.bemaservices.RoomManagement", "com.bemaservices.RoomManagement.Field.Types.ReportTemplateFieldType", "6B88A513-4B4C-403B-ADFA-82C3A2B1C3B8" );

            RockMigrationHelper.UpdateCategory( Rock.SystemGuid.EntityType.DEFINED_TYPE, "Room Management", "", "", "731C5F16-62EA-4DE0-A1FC-6EE2263BF816" );
            RockMigrationHelper.AddDefinedType( "Room Management", "Printable Reports", "Printable Reports used by the Room Management System", "13B169EA-A090-45FF-8B11-A9E02776E35E", @"" );
            RockMigrationHelper.AddDefinedTypeAttribute( "13B169EA-A090-45FF-8B11-A9E02776E35E", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava", "Lava", "If the Lava Template is selected, this is the lava that will be used in the report", 3, "", "2F0BEBBA-B890-46B1-8C36-A3F7CE9A36B9" );
            RockMigrationHelper.AddDefinedTypeAttribute( "13B169EA-A090-45FF-8B11-A9E02776E35E", "6B88A513-4B4C-403B-ADFA-82C3A2B1C3B8", "Report Template", "ReportTemplate", "", 0, "", "1C2F3975-B1E2-4F8A-B2A2-FEF8D1A37E6C" );
            RockMigrationHelper.AddDefinedTypeAttribute( "13B169EA-A090-45FF-8B11-A9E02776E35E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Report Font", "ReportFont", "", 1, "", "98F113C0-8497-48BC-9DA3-C51D163206CB" );
            RockMigrationHelper.AddDefinedTypeAttribute( "13B169EA-A090-45FF-8B11-A9E02776E35E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Report Logo", "ReportLogo", "URL to the logo (PNG) to display in the printed report.", 2, "", "E907AB6D-642C-4079-AD08-0641B4C84B16" );
            RockMigrationHelper.AddAttributeQualifier( "2F0BEBBA-B890-46B1-8C36-A3F7CE9A36B9", "editorHeight", "", "4178EED6-EA71-41AF-ABD5-29D58E4626DD" );
            RockMigrationHelper.AddAttributeQualifier( "2F0BEBBA-B890-46B1-8C36-A3F7CE9A36B9", "editorMode", "3", "C1723802-3281-47CA-B8C6-B64573782A23" );
            RockMigrationHelper.AddAttributeQualifier( "2F0BEBBA-B890-46B1-8C36-A3F7CE9A36B9", "editorTheme", "0", "9B2F9E78-24F1-49CE-A1DF-AECC61678CDB" );
            RockMigrationHelper.AddAttributeQualifier( "98F113C0-8497-48BC-9DA3-C51D163206CB", "ispassword", "False", "FB3955AE-16D1-4051-8A18-E53028F70958" );
            RockMigrationHelper.AddAttributeQualifier( "98F113C0-8497-48BC-9DA3-C51D163206CB", "maxcharacters", "", "FFF9CA66-0867-4843-B82E-051A58A26F0D" );
            RockMigrationHelper.AddAttributeQualifier( "98F113C0-8497-48BC-9DA3-C51D163206CB", "showcountdown", "False", "E59E009B-372D-4143-A655-4F1BA4F54D1C" );
            RockMigrationHelper.AddAttributeQualifier( "E907AB6D-642C-4079-AD08-0641B4C84B16", "ispassword", "False", "E103657B-F3A5-41D2-A940-D9D44A6FD70A" );
            RockMigrationHelper.AddAttributeQualifier( "E907AB6D-642C-4079-AD08-0641B4C84B16", "maxcharacters", "", "C7A4FF45-C785-4D54-B345-9A3B01D38141" );
            RockMigrationHelper.AddAttributeQualifier( "E907AB6D-642C-4079-AD08-0641B4C84B16", "showcountdown", "False", "21FA831C-483A-478B-AFAC-88926843C0D5" );

            RockMigrationHelper.AddDefinedType( "Room Management", "Reservation Views", "Views used by the Room Management System", "32EC3B34-01CF-4513-BC2E-58ECFA91D010", @"" );
            RockMigrationHelper.AddDefinedTypeAttribute( "32EC3B34-01CF-4513-BC2E-58ECFA91D010", "27718256-C1EB-4B1F-9B4B-AC53249F78DF", "Lava", "Lava", "", 1, "", "466DC361-B813-445A-8883-FED7E5D4229B" );
            RockMigrationHelper.AddDefinedTypeAttribute( "32EC3B34-01CF-4513-BC2E-58ECFA91D010", "4BD9088F-5CC6-89B1-45FC-A2AAFFC7CC0D", "Lava Commands", "LavaCommands", "", 0, "", "EE70E271-EAE1-446B-AFA8-EE2D299B8D7F" );
            RockMigrationHelper.AddAttributeQualifier( "466DC361-B813-445A-8883-FED7E5D4229B", "editorHeight", "", "094118F6-AA86-4306-9973-874FD8470D23" );
            RockMigrationHelper.AddAttributeQualifier( "466DC361-B813-445A-8883-FED7E5D4229B", "editorMode", "0", "D3D5FFBC-966B-42BA-A8AF-EF20E0DAB93E" );
            RockMigrationHelper.AddAttributeQualifier( "466DC361-B813-445A-8883-FED7E5D4229B", "editorTheme", "0", "8A881BC5-C13D-45D6-8552-71EC1DF1178D" );

            var blockIdSql = SqlScalar( @"
                Select Id
                From Block
                Where [Guid] = 'F71B7715-EBF5-4CDF-867E-B1018B2AECD5'
                " );
            int? blockId = blockIdSql.ToString().AsIntegerOrNull();

            RockMigrationHelper.UpdateBlockTypeAttribute( "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Report Font", "ReportFont", "", "", 11, @"Gotham", "B9DA1FF2-10EA-4466-BD67-A4D62E03D703" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Report Logo", "ReportLogo", "", "URL to the logo (PNG) to display in the printed report.", 12, @"~/Plugins/com_bemaservices/RoomManagement/Assets/Icons/Central_Logo_Black_rgb_165_90.png", "C05E0362-6C49-4D59-8DB7-7DCADAF19FDD" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635", "6B88A513-4B4C-403B-ADFA-82C3A2B1C3B8", "Report Template", "ReportTemplate", "", "The template for the printed report. The Default and Advanced Templates will generate a printed report based on the templates' hardcoded layout. The Lava Template will generate a report based on the lava provided below in the Report Lava Setting. Any other custom templates will format based on their developer's documentation.", 13, @"9b74314a-37e0-40f2-906c-2862c93f8888", "8E2EE6F2-54FC-4C3A-9C9A-54CEA34544F7" );
            RockMigrationHelper.UpdateBlockTypeAttribute( "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Report Lava", "ReportLava", "", "If the Lava Template is selected, this is the lava that will be used in the report", 14, @"{% include '~/Plugins/com_bemaservices/RoomManagement/Assets/Lava/ReservationReport.lava' %}", "69131013-4E48-468E-B2C2-CF19CEA26590" );

            var defaultFont = GetAttributeValueFromBlock( blockId, "B9DA1FF2-10EA-4466-BD67-A4D62E03D703".AsGuid() );
            var defaultLogo = GetAttributeValueFromBlock( blockId, "C05E0362-6C49-4D59-8DB7-7DCADAF19FDD".AsGuid() );
            var selectedReportTemplateGuid = GetAttributeValueFromBlock( blockId, "8E2EE6F2-54FC-4C3A-9C9A-54CEA34544F7".AsGuid() );
            var defaultLava = GetAttributeValueFromBlock( blockId, "69131013-4E48-468E-B2C2-CF19CEA26590".AsGuid() );

            var organizationName = GlobalAttributesCache.Get().GetValue( "OrganizationName" );
            var lavaTemplate = GetAttributeValueFromBlock( blockId, "52C3F839-A092-441F-B3F9-10617BE391EC".AsGuid() );
            var lavaCommands = GlobalAttributesCache.Get().GetValue( "DefaultEnabledLavaCommands" );

            var selectedDefinedValueGuid = "";
            AttributeCache.RemoveEntityAttributes();
            var allReportTemplates = ReportTemplateContainer.Instance.Components.Values
                .Where( v => v.Value.IsActive == true )
                .Select( v => v.Value.EntityType );

            var reportTemplateList = allReportTemplates
                .ToList();

            if ( reportTemplateList.Any() )
            {
                foreach ( EntityTypeCache reportTemplate in reportTemplateList )
                {
                    var definedValueGuidString = Guid.NewGuid().ToString();
                    var reportTemplateGuidString = reportTemplate.Guid.ToString();
                    var valueName = "";
                    var description = "";
                    var lavaCode = "";

                    switch ( reportTemplateGuidString )
                    {
                        case "9b74314a-37e0-40f2-906c-2862c93f8888": // Event Based
                            valueName = "Event-Based Report";
                            description = "This is default report that can be printed out.";
                            definedValueGuidString = "5D53E2F0-BA82-4154-B996-085C979FACB0";
                            break;
                        case "97a7ffda-1b75-473f-a680-c9a7602b5c60": // Location Based
                            valueName = "Location-Based Report";
                            description = "Meant primarily for facilities teams, this report has a line item for each reservation location containing information about requested layouts.";
                            definedValueGuidString = "46C855B0-E50E-49E7-8B99-74561AFB3DD2";
                            break;
                        case "7ef82cca-7874-4b8d-adb7-896f05095354": // Lava
                            valueName = "Lava Report";
                            description = "This is a generic Lava Report.";
                            lavaCode = defaultLava;
                            definedValueGuidString = "71CEBC9E-D9BA-432D-B1C9-9B3D5CB8E7ED";
                            break;
                        default:
                            valueName = reportTemplate.FriendlyName;
                            break;

                    }

                    if ( selectedReportTemplateGuid == reportTemplateGuidString )
                    {
                        selectedDefinedValueGuid = definedValueGuidString;
                    }

                    RockMigrationHelper.UpdateDefinedValue( "13B169EA-A090-45FF-8B11-A9E02776E35E", valueName, description, definedValueGuidString, true );
                    RockMigrationHelper.AddDefinedValueAttributeValue( definedValueGuidString, "1C2F3975-B1E2-4F8A-B2A2-FEF8D1A37E6C", reportTemplateGuidString );
                    RockMigrationHelper.AddDefinedValueAttributeValue( definedValueGuidString, "2F0BEBBA-B890-46B1-8C36-A3F7CE9A36B9", lavaCode );
                    RockMigrationHelper.AddDefinedValueAttributeValue( definedValueGuidString, "98F113C0-8497-48BC-9DA3-C51D163206CB", defaultFont );
                    RockMigrationHelper.AddDefinedValueAttributeValue( definedValueGuidString, "E907AB6D-642C-4079-AD08-0641B4C84B16", defaultLogo );
                }
            }

            var visibleViewGuid = "67EA36B0-D861-4399-998E-3B69F7700DC0";
            if ( lavaTemplate.Trim() != "{% include '~/Plugins/com_bemaservices/RoomManagement/Assets/Lava/Reservation.lava' %}" )
            {
                visibleViewGuid = "50AFFB1C-8AE9-4BC0-AECE-96C530A78497";
                RockMigrationHelper.UpdateDefinedValue( "32EC3B34-01CF-4513-BC2E-58ECFA91D010", organizationName, "", "50AFFB1C-8AE9-4BC0-AECE-96C530A78497", false );
                RockMigrationHelper.AddDefinedValueAttributeValue( "50AFFB1C-8AE9-4BC0-AECE-96C530A78497", "466DC361-B813-445A-8883-FED7E5D4229B", lavaTemplate );
                RockMigrationHelper.AddDefinedValueAttributeValue( "50AFFB1C-8AE9-4BC0-AECE-96C530A78497", "EE70E271-EAE1-446B-AFA8-EE2D299B8D7F", lavaCommands );
            }

            RockMigrationHelper.UpdateDefinedValue( "32EC3B34-01CF-4513-BC2E-58ECFA91D010", "Default", "The default view for the Room Management home page.", "67EA36B0-D861-4399-998E-3B69F7700DC0", true );
            RockMigrationHelper.AddDefinedValueAttributeValue( "67EA36B0-D861-4399-998E-3B69F7700DC0", "466DC361-B813-445A-8883-FED7E5D4229B", @"{% include '~/Plugins/com_bemaservices/RoomManagement/Assets/Lava/Reservation.lava' %}" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "67EA36B0-D861-4399-998E-3B69F7700DC0", "EE70E271-EAE1-446B-AFA8-EE2D299B8D7F", lavaCommands );

            RockMigrationHelper.UpdateBlockTypeAttribute( "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Visible Printable Report Options", "VisiblePrintableReportOptions", "", "The Printable Reports that the user is able to select", 21, @"5D53E2F0-BA82-4154-B996-085C979FACB0,46C855B0-E50E-49E7-8B99-74561AFB3DD2", "BB36C64E-E379-4B34-BC91-BD65FCEEBBF7" );

            StringBuilder sb = new StringBuilder();
            sb.Append( "46C855B0-E50E-49E7-8B99-74561AFB3DD2," );
            sb.Append( selectedDefinedValueGuid );
            RockMigrationHelper.AddBlockAttributeValue( "F71B7715-EBF5-4CDF-867E-B1018B2AECD5", "BB36C64E-E379-4B34-BC91-BD65FCEEBBF7", sb.ToString() );

            // Attrib for BlockType: Reservation Lava:Visible Reservation View Options
            RockMigrationHelper.UpdateBlockTypeAttribute( "D0EC5F69-5BB1-4BCA-B0F0-3FE2B9267635", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Visible Reservation View Options", "VisibleReservationViewOptions", "", "The Reservation Views that the user is able to select", 22, @"67EA36B0-D861-4399-998E-3B69F7700DC0", "9A9C55FA-4E79-4003-B95A-7F12686889EC" );
            // Attrib Value for Block:Reservation Lava, Attribute:Visible Reservation View Options Page: Room Management, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "F71B7715-EBF5-4CDF-867E-B1018B2AECD5", "9A9C55FA-4E79-4003-B95A-7F12686889EC", visibleViewGuid );


            // Page: Printable Reports
            RockMigrationHelper.AddPage( true, "0FF1D7F4-BF6D-444A-BD71-645BD764EC40", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Printable Reports", "", "267C476B-16F7-4201-9453-5784E2B4C98F", "fa fa-file-alt" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockTypeByGuid( "Defined Type Detail", "Displays the details of the given defined type.", "~/Blocks/Core/DefinedTypeDetail.ascx", "Core", "08C35F15-9AF7-468F-9D50-CDFD3D21220C" );
            RockMigrationHelper.UpdateBlockTypeByGuid( "Defined Value List", "Block for viewing values for a defined type.", "~/Blocks/Core/DefinedValueList.ascx", "Core", "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE" );
            // Add Block to Page: Printable Reports, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "267C476B-16F7-4201-9453-5784E2B4C98F", "", "08C35F15-9AF7-468F-9D50-CDFD3D21220C", "Defined Type Detail", "Main", "", "", 0, "41F4477E-2D32-4202-8BD8-60A473D96DF7" );
            // Add Block to Page: Printable Reports, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "267C476B-16F7-4201-9453-5784E2B4C98F", "", "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "Defined Value List", "Main", "", "", 1, "B22FB153-BFEE-486C-85CC-7FE9D375E35F" );
            // Attrib for BlockType: Defined Type Detail:Defined Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "08C35F15-9AF7-468F-9D50-CDFD3D21220C", "BC48720C-3610-4BCF-AE66-D255A17F1CDF", "Defined Type", "DefinedType", "", "If a Defined Type is set, only details for it will be displayed (regardless of the querystring parameters).", 0, @"", "0305EF98-C791-4626-9996-F189B9BB674C" );
            // Attrib for BlockType: Defined Value List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", "", 0, @"", "87DAF7ED-AAF5-4D5C-8339-CB30B16CC9FF" );
            // Attrib for BlockType: Defined Value List:Defined Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "BC48720C-3610-4BCF-AE66-D255A17F1CDF", "Defined Type", "DefinedType", "", "If a Defined Type is set, only its Defined Values will be displayed (regardless of the querystring parameters).", 0, @"", "9280D61F-C4F3-4A3E-A9BB-BCD67FF78637" );
            // Attrib for BlockType: Defined Value List:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.UpdateBlockTypeAttribute( "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.CustomGridEnableStickyHeaders", "core.CustomGridEnableStickyHeaders", "", "", 0, @"False", "2CD75CE0-D3C8-470D-8DE1-A2964AB98887" );
            // Attrib Value for Block:Defined Type Detail, Attribute:Defined Type Page: Printable Reports, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "41F4477E-2D32-4202-8BD8-60A473D96DF7", "0305EF98-C791-4626-9996-F189B9BB674C", @"13B169EA-A090-45FF-8B11-A9E02776E35E" );
            // Attrib Value for Block:Defined Value List, Attribute:Defined Type Page: Printable Reports, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B22FB153-BFEE-486C-85CC-7FE9D375E35F", "9280D61F-C4F3-4A3E-A9BB-BCD67FF78637", @"13B169EA-A090-45FF-8B11-A9E02776E35E" );
            // Attrib Value for Block:Defined Value List, Attribute:core.CustomGridColumnsConfig Page: Printable Reports, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B22FB153-BFEE-486C-85CC-7FE9D375E35F", "87DAF7ED-AAF5-4D5C-8339-CB30B16CC9FF", @"" );
            // Attrib Value for Block:Defined Value List, Attribute:core.CustomGridEnableStickyHeaders Page: Printable Reports, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B22FB153-BFEE-486C-85CC-7FE9D375E35F", "2CD75CE0-D3C8-470D-8DE1-A2964AB98887", @"False" );

            // Page: Reservation Views
            RockMigrationHelper.AddPage( true, "0FF1D7F4-BF6D-444A-BD71-645BD764EC40", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Reservation Views", "", "A5EB7D31-A0F0-4EE7-8366-A5EBC0DE6335", "fa fa-file-alt" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockTypeByGuid( "Defined Type Detail", "Displays the details of the given defined type.", "~/Blocks/Core/DefinedTypeDetail.ascx", "Core", "08C35F15-9AF7-468F-9D50-CDFD3D21220C" );
            RockMigrationHelper.UpdateBlockTypeByGuid( "Defined Value List", "Block for viewing values for a defined type.", "~/Blocks/Core/DefinedValueList.ascx", "Core", "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE" );
            // Add Block to Page: Reservation Views, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "A5EB7D31-A0F0-4EE7-8366-A5EBC0DE6335", "", "08C35F15-9AF7-468F-9D50-CDFD3D21220C", "Defined Type Detail", "Main", "", "", 0, "B01CE8D3-4915-44B2-9553-7039F34888BE" );
            // Add Block to Page: Reservation Views, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "A5EB7D31-A0F0-4EE7-8366-A5EBC0DE6335", "", "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "Defined Value List", "Main", "", "", 1, "CB1EB73E-3DE7-4326-A915-BE37A6CE0FF9" );
            // Attrib for BlockType: Defined Type Detail:Defined Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "08C35F15-9AF7-468F-9D50-CDFD3D21220C", "BC48720C-3610-4BCF-AE66-D255A17F1CDF", "Defined Type", "DefinedType", "", "If a Defined Type is set, only details for it will be displayed (regardless of the querystring parameters).", 0, @"", "0305EF98-C791-4626-9996-F189B9BB674C" );
            // Attrib for BlockType: Defined Value List:core.CustomGridColumnsConfig
            RockMigrationHelper.UpdateBlockTypeAttribute( "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "9C204CD0-1233-41C5-818A-C5DA439445AA", "core.CustomGridColumnsConfig", "core.CustomGridColumnsConfig", "", "", 0, @"", "87DAF7ED-AAF5-4D5C-8339-CB30B16CC9FF" );
            // Attrib for BlockType: Defined Value List:Defined Type
            RockMigrationHelper.UpdateBlockTypeAttribute( "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "BC48720C-3610-4BCF-AE66-D255A17F1CDF", "Defined Type", "DefinedType", "", "If a Defined Type is set, only its Defined Values will be displayed (regardless of the querystring parameters).", 0, @"", "9280D61F-C4F3-4A3E-A9BB-BCD67FF78637" );
            // Attrib for BlockType: Defined Value List:core.CustomGridEnableStickyHeaders
            RockMigrationHelper.UpdateBlockTypeAttribute( "0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "core.CustomGridEnableStickyHeaders", "core.CustomGridEnableStickyHeaders", "", "", 0, @"False", "2CD75CE0-D3C8-470D-8DE1-A2964AB98887" );
            // Attrib Value for Block:Defined Type Detail, Attribute:Defined Type Page: Reservation Views, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "B01CE8D3-4915-44B2-9553-7039F34888BE", "0305EF98-C791-4626-9996-F189B9BB674C", @"32EC3B34-01CF-4513-BC2E-58ECFA91D010" );
            // Attrib Value for Block:Defined Value List, Attribute:Defined Type Page: Reservation Views, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "CB1EB73E-3DE7-4326-A915-BE37A6CE0FF9", "9280D61F-C4F3-4A3E-A9BB-BCD67FF78637", @"32EC3B34-01CF-4513-BC2E-58ECFA91D010" );
            // Attrib Value for Block:Defined Value List, Attribute:core.CustomGridColumnsConfig Page: Reservation Views, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "CB1EB73E-3DE7-4326-A915-BE37A6CE0FF9", "87DAF7ED-AAF5-4D5C-8339-CB30B16CC9FF", @"" );
            // Attrib Value for Block:Defined Value List, Attribute:core.CustomGridEnableStickyHeaders Page: Reservation Views, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "CB1EB73E-3DE7-4326-A915-BE37A6CE0FF9", "2CD75CE0-D3C8-470D-8DE1-A2964AB98887", @"False" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version.
        /// </summary>
        public override void Down()
        {

        }

        private string GetAttributeValueFromBlock( int? blockId, Guid attributeGuid )
        {
            object valueSql = null;
            if ( blockId.HasValue )
            {
                valueSql = SqlScalar( String.Format( @"
                Select av.Value
                From AttributeValue av
                Join Attribute a on av.AttributeId = a.Id
                Where av.EntityId = {0}
                And a.Guid = '{1}'
                ",
                blockId.Value,
                attributeGuid.ToString() ) );
            }

            if ( valueSql == null )
            {
                valueSql = SqlScalar( String.Format( @"
                Select a.DefaultValue
                From Attribute a
                Where a.Guid = '{0}'
                ",
                attributeGuid.ToString() ) );
            }

            if ( valueSql != null )
            {
                return valueSql.ToString();
            }
            else
            {
                return string.Empty;
            }

        }

        public void UpdateFieldTypeByGuid( string name, string description, string assembly, string className, string guid, bool IsSystem = true )
        {
            Sql( string.Format( @"
                IF EXISTS ( SELECT [Id] FROM [FieldType] WHERE [Guid] = '{4}' )
                BEGIN
                    UPDATE [FieldType] SET
                        [Name] = '{0}',
                        [Description] = '{1}',
                        [Guid] = '{4}',
                        [IsSystem] = {5},
                        [Assembly] = '{2}',
                        [Class] = '{3}'
                    WHERE [Guid] = '{4}'
                END
                ELSE
                BEGIN
                    DECLARE @Id int
                    SET @Id = (SELECT [Id] FROM [FieldType] WHERE [Assembly] = '{2}' AND [Class] = '{3}')
                    IF @Id IS NULL
                    BEGIN
                        INSERT INTO [FieldType] (
                            [Name],[Description],[Assembly],[Class],[Guid],[IsSystem])
                        VALUES(
                            '{0}','{1}','{2}','{3}','{4}',{5})
                    END
                    ELSE
                    BEGIN
                        UPDATE [FieldType] SET
                            [Name] = '{0}',
                            [Description] = '{1}',
                            [Guid] = '{4}',
                            [IsSystem] = {5}
                        WHERE [Assembly] = '{2}'
                        AND [Class] = '{3}'
                    END
                END
",
                    name.Replace( "'", "''" ),
                    description.Replace( "'", "''" ),
                    assembly,
                    className,
                    guid,
                    IsSystem ? "1" : "0" ) );
        }

    }
}