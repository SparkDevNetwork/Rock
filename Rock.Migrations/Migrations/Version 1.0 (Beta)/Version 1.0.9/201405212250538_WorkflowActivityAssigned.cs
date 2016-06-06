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
    public partial class WorkflowActivityAssigned : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.WorkflowActivity", "AssignedPersonAliasId", c => c.Int() );
            AddColumn( "dbo.WorkflowActivity", "AssignedGroupId", c => c.Int() );
            CreateIndex( "dbo.WorkflowActivity", "AssignedPersonAliasId" );
            CreateIndex( "dbo.WorkflowActivity", "AssignedGroupId" );
            AddForeignKey( "dbo.WorkflowActivity", "AssignedGroupId", "dbo.Group", "Id" );
            AddForeignKey( "dbo.WorkflowActivity", "AssignedPersonAliasId", "dbo.PersonAlias", "Id" );

            RockMigrationHelper.AddDefinedType_pre201409101843015( "Global", "CSS Classes", "Contain common css class definitions", "407A3A73-A3EF-4970-B856-2A33F62AC72E", @"" );
            RockMigrationHelper.AddDefinedTypeAttribute( "407A3A73-A3EF-4970-B856-2A33F62AC72E", "9C204CD0-1233-41C5-818A-C5DA439445AA", "CSS Class", "CSSClass", "The css class", 28, "", "6FF59F53-28EA-4BFE-AFE1-A459CC588495" );
            RockMigrationHelper.AddDefinedValue_pre20140819( "407A3A73-A3EF-4970-B856-2A33F62AC72E", "Primary Button", "", "FDC397CD-8B4A-436E-BEA1-BCE2E6717C03", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FDC397CD-8B4A-436E-BEA1-BCE2E6717C03", "6FF59F53-28EA-4BFE-AFE1-A459CC588495", @"btn btn-primary" );
            RockMigrationHelper.AddDefinedValue_pre20140819( "407A3A73-A3EF-4970-B856-2A33F62AC72E", "Red Button", "", "FDEB8E6C-70C3-4033-B307-7D0DEE1AC29D", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "FDEB8E6C-70C3-4033-B307-7D0DEE1AC29D", "6FF59F53-28EA-4BFE-AFE1-A459CC588495", @"btn btn-danger" );

            RockMigrationHelper.AddDefinedType_pre201409101843015( "Location", "Countries", "Defines how addresses are displayed and formatted for each country", "D7979EA1-44E9-46E2-BF37-DDAF7F741378", @"" );
            RockMigrationHelper.AddDefinedTypeAttribute( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Abbreviation", "Abbreviation", "The abbreviation for the country", 0, "", "DA46DC37-5398-4520-B6A5-6E57C9C46F7A" );
            RockMigrationHelper.AddDefinedTypeAttribute( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Postal Code Label", "PostalCodeLabel", "The label to use for the Postal Code (Zip) field", 2, "Zip", "7D785A5D-53CA-4FEC-BC88-DFBD7439B547" );
            RockMigrationHelper.AddDefinedTypeAttribute( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "9C204CD0-1233-41C5-818A-C5DA439445AA", "State Label", "StateLabel", "The label to use for the 'state' field", 1, "State", "A4E00B14-8CFF-4719-A43F-462851C7BBEF" );
            RockMigrationHelper.AddDefinedTypeAttribute( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "Address Format", "AddressFormat", "The Liquid syntax to use for formatting addresses", 3, @"
{{ Street1 }} 
{{ Street2 }}
{{ City }}, {{ State }} {{ Zip }}
", "B6EF4138-C488-4043-A628-D35F91503843" );
            RockMigrationHelper.AddDefinedValue_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "United States", "United States", "F4DAEB01-A0E5-426A-A425-7F6D21DF1CE7", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "F4DAEB01-A0E5-426A-A425-7F6D21DF1CE7", "DA46DC37-5398-4520-B6A5-6E57C9C46F7A", @"US" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "F4DAEB01-A0E5-426A-A425-7F6D21DF1CE7", "A4E00B14-8CFF-4719-A43F-462851C7BBEF", @"State" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "F4DAEB01-A0E5-426A-A425-7F6D21DF1CE7", "7D785A5D-53CA-4FEC-BC88-DFBD7439B547", @"Zip" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "F4DAEB01-A0E5-426A-A425-7F6D21DF1CE7", "B6EF4138-C488-4043-A628-D35F91503843", @"{{ Street1 }} 
{{ Street2 }}
{{ City }}, {{ State }} {{ Zip }}" );

            RockMigrationHelper.AddDefinedValue_pre20140819( "D7979EA1-44E9-46E2-BF37-DDAF7F741378", "Canada", "Canada", "60D86635-8E4A-4F4B-8052-26048DAE0335", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "60D86635-8E4A-4F4B-8052-26048DAE0335", "DA46DC37-5398-4520-B6A5-6E57C9C46F7A", @"CA" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "60D86635-8E4A-4F4B-8052-26048DAE0335", "A4E00B14-8CFF-4719-A43F-462851C7BBEF", @"Province" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "60D86635-8E4A-4F4B-8052-26048DAE0335", "7D785A5D-53CA-4FEC-BC88-DFBD7439B547", @"Postal Code" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "60D86635-8E4A-4F4B-8052-26048DAE0335", "B6EF4138-C488-4043-A628-D35F91503843", @"{{ Street1 }} 
{{ Street2 }}
{{ City }}, {{ State }} {{ Zip }}
{{ Country }}" );

            RockMigrationHelper.AddDefinedTypeAttribute( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Country", "Country", "The country that the 'state' belongs to", 29, "f4daeb01-a0e5-426a-a425-7f6d21df1ce7", "3B234A62-B87D-47CD-A33F-32CC6C840A02" );
            RockMigrationHelper.AddAttributeQualifier( "3B234A62-B87D-47CD-A33F-32CC6C840A02", "allowmultiple", "False", "0177244E-D113-4752-8C89-2DD05BA5FAE0" );
            RockMigrationHelper.AddAttributeQualifier( "3B234A62-B87D-47CD-A33F-32CC6C840A02", "definedtype", "46", "50019CC0-BBD9-4CE8-B04D-15DD9698F8BD" );

            RockMigrationHelper.AddDefinedValue_pre20140819( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "AB", "Alberta", "50ABDD73-C7BF-4439-994F-EE3ADD60910B" );
            RockMigrationHelper.AddDefinedValue_pre20140819( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "BC", "British Columbia", "2651DEE8-30D6-477D-88CA-48EBA769683E" );
            RockMigrationHelper.AddDefinedValue_pre20140819( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "MB", "Manitoba", "5A59DC0E-97E0-45EA-83CD-586EB9D4BD49" );
            RockMigrationHelper.AddDefinedValue_pre20140819( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "NB", "New Brunswick", "C975FD3B-2A25-473A-8CD4-ABBCD0757CA0" );
            RockMigrationHelper.AddDefinedValue_pre20140819( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "NL", "Newfoundland and Labrador", "C8ABE4D8-6B88-47ED-80DB-CDF8EEF47A7F" );
            RockMigrationHelper.AddDefinedValue_pre20140819( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "NT", "Northwest Territories", "67E0BC8B-BE23-40B0-ADB1-ACA50CEF8825" );
            RockMigrationHelper.AddDefinedValue_pre20140819( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "NS", "Nova Scotia", "F91359C1-D6F9-495E-A475-9D7DEF7AA67B" );
            RockMigrationHelper.AddDefinedValue_pre20140819( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "NU", "Nunavut", "B6AC22E5-676E-4FB9-909D-4E047A21B703" );
            RockMigrationHelper.AddDefinedValue_pre20140819( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "ON", "Ontario", "0EF0F132-69C3-480F-801A-59145F417277" );
            RockMigrationHelper.AddDefinedValue_pre20140819( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "PE", "Prince Edward Island", "25DE8C9B-9F69-4B4E-A21F-D976FE1D0D41" );
            RockMigrationHelper.AddDefinedValue_pre20140819( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "QC", "Quebec", "BE1A052A-ABCD-48C2-AA04-BDE4F394D001" );
            RockMigrationHelper.AddDefinedValue_pre20140819( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "SK", "Saskatchewan", "63B48AA0-8904-4415-9F5E-7218454D3D0F" );
            RockMigrationHelper.AddDefinedValue_pre20140819( "C3A20D2D-AEAF-4E2B-A1D9-2E072CEFC2BB", "YT", "Yukon", "91D87671-F309-4908-ABE1-57FA27B2C7F6" );

            RockMigrationHelper.AddDefinedValueAttributeValue( "0EF0F132-69C3-480F-801A-59145F417277", "3B234A62-B87D-47CD-A33F-32CC6C840A02", "60D86635-8E4A-4F4B-8052-26048DAE0335" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "BE1A052A-ABCD-48C2-AA04-BDE4F394D001", "3B234A62-B87D-47CD-A33F-32CC6C840A02", "60D86635-8E4A-4F4B-8052-26048DAE0335" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "F91359C1-D6F9-495E-A475-9D7DEF7AA67B", "3B234A62-B87D-47CD-A33F-32CC6C840A02", "60D86635-8E4A-4F4B-8052-26048DAE0335" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C975FD3B-2A25-473A-8CD4-ABBCD0757CA0", "3B234A62-B87D-47CD-A33F-32CC6C840A02", "60D86635-8E4A-4F4B-8052-26048DAE0335" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "5A59DC0E-97E0-45EA-83CD-586EB9D4BD49", "3B234A62-B87D-47CD-A33F-32CC6C840A02", "60D86635-8E4A-4F4B-8052-26048DAE0335" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "2651DEE8-30D6-477D-88CA-48EBA769683E", "3B234A62-B87D-47CD-A33F-32CC6C840A02", "60D86635-8E4A-4F4B-8052-26048DAE0335" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "25DE8C9B-9F69-4B4E-A21F-D976FE1D0D41", "3B234A62-B87D-47CD-A33F-32CC6C840A02", "60D86635-8E4A-4F4B-8052-26048DAE0335" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "63B48AA0-8904-4415-9F5E-7218454D3D0F", "3B234A62-B87D-47CD-A33F-32CC6C840A02", "60D86635-8E4A-4F4B-8052-26048DAE0335" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "50ABDD73-C7BF-4439-994F-EE3ADD60910B", "3B234A62-B87D-47CD-A33F-32CC6C840A02", "60D86635-8E4A-4F4B-8052-26048DAE0335" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C8ABE4D8-6B88-47ED-80DB-CDF8EEF47A7F", "3B234A62-B87D-47CD-A33F-32CC6C840A02", "60D86635-8E4A-4F4B-8052-26048DAE0335" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "67E0BC8B-BE23-40B0-ADB1-ACA50CEF8825", "3B234A62-B87D-47CD-A33F-32CC6C840A02", "60D86635-8E4A-4F4B-8052-26048DAE0335" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "91D87671-F309-4908-ABE1-57FA27B2C7F6", "3B234A62-B87D-47CD-A33F-32CC6C840A02", "60D86635-8E4A-4F4B-8052-26048DAE0335" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B6AC22E5-676E-4FB9-909D-4E047A21B703", "3B234A62-B87D-47CD-A33F-32CC6C840A02", "60D86635-8E4A-4F4B-8052-26048DAE0335" );

            Sql( @"
    UPDATE [Attribute] SET [IsGridColumn] = 1
    WHERE [Guid] IN ( '6FF59F53-28EA-4BFE-AFE1-A459CC588495', 'DA46DC37-5398-4520-B6A5-6E57C9C46F7A', '7D785A5D-53CA-4FEC-BC88-DFBD7439B547', 'A4E00B14-8CFF-4719-A43F-462851C7BBEF', '3B234A62-B87D-47CD-A33F-32CC6C840A02')
" );

            RockMigrationHelper.AddPage( "98163C8B-5C91-4A68-BB79-6AD948A604CE", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Workflow", "", "CDB27DB2-977C-415A-AED5-D0751DFD5DF2", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "CDB27DB2-977C-415A-AED5-D0751DFD5DF2", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Workflows", "", "6510AB6B-DFB4-4DBF-9F0F-7EA598E4AC54", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "6510AB6B-DFB4-4DBF-9F0F-7EA598E4AC54", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Manage Workflows", "", "61E1B4B6-EACE-42E8-A2FB-37465E6D0004", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "61E1B4B6-EACE-42E8-A2FB-37465E6D0004", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Workflow Entry", "", "0550D2AA-A705-4400-81FF-AB124FDF83D7", "" ); // Site:Rock RMS
            RockMigrationHelper.AddPage( "61E1B4B6-EACE-42E8-A2FB-37465E6D0004", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Workflow Detail", "", "BA547EED-5537-49CF-BD4E-C583D760788C", "" ); // Site:Rock RMS

            RockMigrationHelper.UpdateBlockType( "Workflow Entry", "Used to enter information for a workflow form entry action.", "~/Blocks/Core/WorkflowEntry.ascx", "Core", "A8BD05C8-6F89-4628-845B-059E686F089A" );
            RockMigrationHelper.UpdateBlockType( "Workflow Navigation", "Block for navigating workflow types and launching and/or managing workflows.", "~/Blocks/Core/WorkflowNavigation.ascx", "Core", "DDC6B004-9ED1-470F-ABF5-041250082168" );

            // Add Block to Page: Workflow Entry, Site: Rock RMS
            RockMigrationHelper.AddBlock( "0550D2AA-A705-4400-81FF-AB124FDF83D7", "", "A8BD05C8-6F89-4628-845B-059E686F089A", "Workflow Entry", "Main", "", "", 0, "29D1A3BC-9D08-4782-8B01-FE5DC6FCF367" );
            // Add Block to Page: Manage Workflows, Site: Rock RMS
            RockMigrationHelper.AddBlock( "61E1B4B6-EACE-42E8-A2FB-37465E6D0004", "", "C86C80DF-F2FD-47F8-81CF-7C5EA4100C3B", "Workflow List", "Main", "", "", 0, "BCC61035-DA99-47EE-A376-71D430455DB4" );
            // Add Block to Page: Workflows, Site: Rock RMS
            RockMigrationHelper.AddBlock( "6510AB6B-DFB4-4DBF-9F0F-7EA598E4AC54", "", "DDC6B004-9ED1-470F-ABF5-041250082168", "Workflow Navigation", "Main", "", "", 0, "2D20CEC4-328E-4C2B-8059-78DFC49D8E35" );

            // Attrib for BlockType: Workflow Entry:Workflow Type
            RockMigrationHelper.AddBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Workflow Type", "WorkflowType", "", "Type of workflow to start.", 0, @"", "2F1D98C4-A8EF-4680-9F64-11BFC28D5597" );
            // Attrib for BlockType: Workflow Navigation:Manage Page
            RockMigrationHelper.AddBlockTypeAttribute( "DDC6B004-9ED1-470F-ABF5-041250082168", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Manage Page", "ManagePage", "", "Page used to manage workflows of the selected type.", 0, @"", "6B8E6B05-87E6-4CA0-9A44-861184E3A34C" );
            // Attrib for BlockType: Workflow Navigation:Entry Page
            RockMigrationHelper.AddBlockTypeAttribute( "DDC6B004-9ED1-470F-ABF5-041250082168", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Entry Page", "EntryPage", "", "Page used to launch a new workflow of the selected type.", 0, @"", "DABA0448-C967-4E9D-863E-59C95059935A" );
            // Attrib for BlockType: Workflow List:Entry Page
            RockMigrationHelper.AddBlockTypeAttribute( "C86C80DF-F2FD-47F8-81CF-7C5EA4100C3B", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Entry Page", "EntryPage", "", "Page used to launch a new workflow of the selected type.", 0, @"", "630AF4C8-6DA1-4BC2-8D38-283D7EF3DD43" );
            // Attrib Value for Block:Attribute Values, Attribute:Category Page: Extended Attributes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "DCA9E640-B5EA-4C73-90BC-4A91330528D5", "EC43CF32-3BDF-4544-8B6A-CE9208DD7C81", @"dd8f467d-b83c-444f-b04c-c681167046a1" );
            // Attrib Value for Block:Workflow Entry, Attribute:Workflow Type Page: Workflow Entry, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "29D1A3BC-9D08-4782-8B01-FE5DC6FCF367", "2F1D98C4-A8EF-4680-9F64-11BFC28D5597", @"" );
            // Attrib Value for Block:Workflow List, Attribute:Entry Page Page: Manage Workflows, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BCC61035-DA99-47EE-A376-71D430455DB4", "630AF4C8-6DA1-4BC2-8D38-283D7EF3DD43", @"0550d2aa-a705-4400-81ff-ab124fdf83d7" );
            // Attrib Value for Block:Workflow List, Attribute:Detail Page Page: Manage Workflows, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "BCC61035-DA99-47EE-A376-71D430455DB4", "C0BA339B-10C5-4609-B806-D192C733FFF1", @"ba547eed-5537-49cf-bd4e-c583d760788c" );
            // Attrib Value for Block:Workflow Navigation, Attribute:Manage Page Page: Workflows, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "2D20CEC4-328E-4C2B-8059-78DFC49D8E35", "6B8E6B05-87E6-4CA0-9A44-861184E3A34C", @"61e1b4b6-eace-42e8-a2fb-37465e6d0004" );
            // Attrib Value for Block:Workflow Navigation, Attribute:Entry Page Page: Workflows, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "2D20CEC4-328E-4C2B-8059-78DFC49D8E35", "DABA0448-C967-4E9D-863E-59C95059935A", @"0550d2aa-a705-4400-81ff-ab124fdf83d7" );
            RockMigrationHelper.UpdateFieldType( "Comparison", "", "Rock", "Rock.Field.Types.ComparisonFieldType", "3C742B81-3C55-48B8-A7CD-E7762EA5BB91" );
            RockMigrationHelper.UpdateFieldType( "Metric Entity", "", "Rock", "Rock.Field.Types.MetricEntityFieldType", "3A7FB32E-1CCD-4F79-B085-BDBADEB56CCF" );
            RockMigrationHelper.UpdateFieldType( "Workflow Activity", "", "Rock", "Rock.Field.Types.WorkflowActivityFieldType", "739FD425-5B8C-4605-B775-7E4D9D4C11DB" );
            RockMigrationHelper.UpdateFieldType( "Workflow Attribute", "", "Rock", "Rock.Field.Types.WorkflowAttributeFieldType", "33E6DF69-BDFA-407A-9744-C175B60643AE" );
            RockMigrationHelper.UpdateFieldType( "Metrics", "", "Rock", "Rock.Field.Types.MetricsFieldType", "3AF9AD35-9F3E-4497-BFDE-60C6C1827653" );
            RockMigrationHelper.UpdateFieldType( "Entity", "", "Rock", "Rock.Field.Types.EntityFieldType", "B50968BD-7643-4288-9237-6E89D2065363" );
            RockMigrationHelper.UpdateFieldType( "Metric Categories", "", "Rock", "Rock.Field.Types.MetricCategoriesFieldType", "F5334A8E-B7E2-415C-A6EC-A6D8FA5341C4" );
            RockMigrationHelper.UpdateFieldType( "Workflow Text Or Attribute", "", "Rock", "Rock.Field.Types.WorkflowTextOrAttributeFieldType", "3B1D93D7-9414-48F9-80E5-6A3FC8F94C20" );
            RockMigrationHelper.UpdateFieldType( "Sliding Date Range", "", "Rock", "Rock.Field.Types.SlidingDateRangeFieldType", "55810BC5-45EA-4044-B783-0CCE0A445C6F" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteAttribute( "3B234A62-B87D-47CD-A33F-32CC6C840A02" );
            RockMigrationHelper.DeleteAttribute( "B6EF4138-C488-4043-A628-D35F91503843" );
            RockMigrationHelper.DeleteAttribute( "A4E00B14-8CFF-4719-A43F-462851C7BBEF" );
            RockMigrationHelper.DeleteAttribute( "DA46DC37-5398-4520-B6A5-6E57C9C46F7A" );
            RockMigrationHelper.DeleteAttribute( "7D785A5D-53CA-4FEC-BC88-DFBD7439B547" );
            RockMigrationHelper.DeleteAttribute( "6FF59F53-28EA-4BFE-AFE1-A459CC588495" );

            RockMigrationHelper.DeleteDefinedType( "407A3A73-A3EF-4970-B856-2A33F62AC72E" );
            RockMigrationHelper.DeleteDefinedType( "D7979EA1-44E9-46E2-BF37-DDAF7F741378" );

            DropForeignKey( "dbo.WorkflowActivity", "AssignedPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.WorkflowActivity", "AssignedGroupId", "dbo.Group" );
            DropIndex( "dbo.WorkflowActivity", new[] { "AssignedGroupId" } );
            DropIndex( "dbo.WorkflowActivity", new[] { "AssignedPersonAliasId" } );
            DropColumn( "dbo.WorkflowActivity", "AssignedGroupId" );
            DropColumn( "dbo.WorkflowActivity", "AssignedPersonAliasId" );
        }
    }
}
