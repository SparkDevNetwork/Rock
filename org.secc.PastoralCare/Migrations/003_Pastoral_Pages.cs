// <copyright>
// Copyright Southeast Christian Church
//
// Licensed under the  Southeast Christian Church License (the "License");
// you may not use this file except in compliance with the License.
// A copy of the License shoud be included with this file.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Plugin;
using Rock.Web.Cache;

namespace org.secc.PastoralCare.Migrations
{
    [MigrationNumber( 3, "1.2.0" )]
    class Pastoral_Pages : Migration
    {
        public override void Up()
        {
            PageService pageService = new PageService( new RockContext() );

            // Page: Pastoral Care
            if ( !pageService.Queryable().Any(p => p.Guid == new Guid( "A64D2C30-1205-41D4-B8FA-CCE4FBC47906" ) ) ) { 
                RockMigrationHelper.AddPage( "5B6DBC42-8B03-4D15-8D92-AAFA28FD8616", "D65F783D-87A9-4CC9-8110-E83466A0EADB","Pastoral Care","","A64D2C30-1205-41D4-B8FA-CCE4FBC47906","fa fa-heartbeat"); // Site:Rock RMS
                RockMigrationHelper.UpdateBlockType("Page Menu","Renders a page menu based on a root page and liquid template.","~/Blocks/Cms/PageMenu.ascx","CMS","CACB9D1A-A820-4587-986A-D66A69EE9948");
                RockMigrationHelper.AddBlock("A64D2C30-1205-41D4-B8FA-CCE4FBC47906","","CACB9D1A-A820-4587-986A-D66A69EE9948","Page Menu","Main","","",0,"D1855171-CB5D-42D2-A951-E12E988E1032");   
                RockMigrationHelper.AddBlockAttributeValue("D1855171-CB5D-42D2-A951-E12E988E1032","7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22",@""); // CSS File  
                RockMigrationHelper.AddBlockAttributeValue("D1855171-CB5D-42D2-A951-E12E988E1032","EEE71DDE-C6BC-489B-BAA5-1753E322F183",@"False"); // Include Current Parameters  
                RockMigrationHelper.AddBlockAttributeValue("D1855171-CB5D-42D2-A951-E12E988E1032","1322186A-862A-4CF1-B349-28ECB67229BA",@"{% include '~~/Assets/Lava/PageListAsBlocks.lava' %}"); // Template  
                RockMigrationHelper.AddBlockAttributeValue("D1855171-CB5D-42D2-A951-E12E988E1032","6C952052-BC79-41BA-8B88-AB8EA3E99648",@"1"); // Number of Levels  
                RockMigrationHelper.AddBlockAttributeValue("D1855171-CB5D-42D2-A951-E12E988E1032","E4CF237D-1D12-4C93-AFD7-78EB296C4B69",@"False"); // Include Current QueryString  
                RockMigrationHelper.AddBlockAttributeValue("D1855171-CB5D-42D2-A951-E12E988E1032","2EF904CD-976E-4489-8C18-9BA43885ACD9",@"False"); // Enable Debug  
                RockMigrationHelper.AddBlockAttributeValue("D1855171-CB5D-42D2-A951-E12E988E1032","C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2",@"False"); // Is Secondary Block  
                RockMigrationHelper.AddBlockAttributeValue("D1855171-CB5D-42D2-A951-E12E988E1032","0A49DABE-42EE-40E5-9E06-0E6530944865",@""); // Include Page List  
            }
            // Page: Dashboard
            if ( !pageService.Queryable().Any(p => p.Guid == new Guid("FFF16609-1DEA-4B76-9CDC-49277587B470") ) )
            {
                RockMigrationHelper.AddPage( "A64D2C30-1205-41D4-B8FA-CCE4FBC47906", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Dashboard", "", "FFF16609-1DEA-4B76-9CDC-49277587B470", "fa fa-dashboard" ); // Site:Rock RMS
                RockMigrationHelper.UpdateBlockType( "Redirect", "Redirects the page to the URL provided.", "~/Blocks/Cms/Redirect.ascx", "CMS", "B97FB779-5D3E-4663-B3B5-3C2C227AE14A" );
                RockMigrationHelper.AddBlock( "FFF16609-1DEA-4B76-9CDC-49277587B470", "", "B97FB779-5D3E-4663-B3B5-3C2C227AE14A", "Redirect", "Main", "", "", 0, "53BDE0F5-F8A0-4706-9027-DDA6C99649CE" );
                RockMigrationHelper.AddBlockAttributeValue( "53BDE0F5-F8A0-4706-9027-DDA6C99649CE", "964D33F4-27D0-4715-86BE-D30CEB895044", @"/unknown" ); // Url
            }
            // Page: Hospitalization List
            if ( !pageService.Queryable().Any(p => p.Guid == new Guid("E9B6F39A-276B-4F31-91C4-5EE9FF8AEF20") ) )
            {
                RockMigrationHelper.AddPage( "FFF16609-1DEA-4B76-9CDC-49277587B470", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Hospitalization List", "", "E9B6F39A-276B-4F31-91C4-5EE9FF8AEF20", "fa fa-dashboard" ); // Site:Rock RMS
                RockMigrationHelper.UpdateBlockType( "Hospital List", "A summary of all the current hospitalizations that have been reported to Southeast.", "~/Plugins/org_secc/PastoralCare/HospitalList.ascx", "SECC > Pastoral Care", "230AEE8C-A2B7-465C-93BE-A92F23364082" );
                RockMigrationHelper.AddBlock( "E9B6F39A-276B-4F31-91C4-5EE9FF8AEF20", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Main", "", "", 1, "86D97641-6117-4490-BDFF-0E976CFDA05A" );
                RockMigrationHelper.AddBlock( "E9B6F39A-276B-4F31-91C4-5EE9FF8AEF20", "", "230AEE8C-A2B7-465C-93BE-A92F23364082", "Hospital List", "Main", "", "", 2, "FDAD2205-CC8D-4088-BFAB-30838D751757" );
                RockMigrationHelper.AddBlockTypeAttribute( "230AEE8C-A2B7-465C-93BE-A92F23364082", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Hospital Admission Workflow", "HospitalAdmissionWorkflow", "", "", 0, @"", "44690B0A-6868-4B10-8085-F3D12EB5931A" );
                RockMigrationHelper.AddBlockAttributeValue( "FDAD2205-CC8D-4088-BFAB-30838D751757", "44690B0A-6868-4B10-8085-F3D12EB5931A", @"314cc992-c90c-4d7d-aec6-09c0fb4c7a38" ); // Hospital Admission Workflow
                RockMigrationHelper.AddBlockAttributeValue( "86D97641-6117-4490-BDFF-0E976CFDA05A", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" ); // CSS File
                RockMigrationHelper.AddBlockAttributeValue( "86D97641-6117-4490-BDFF-0E976CFDA05A", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" ); // Include Current Parameters
                RockMigrationHelper.AddBlockAttributeValue( "86D97641-6117-4490-BDFF-0E976CFDA05A", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsTabs.lava' %}<br />" ); // Template
                RockMigrationHelper.AddBlockAttributeValue( "86D97641-6117-4490-BDFF-0E976CFDA05A", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"fff16609-1dea-4b76-9cdc-49277587b470" ); // Root Page
                RockMigrationHelper.AddBlockAttributeValue( "86D97641-6117-4490-BDFF-0E976CFDA05A", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" ); // Number of Levels
                RockMigrationHelper.AddBlockAttributeValue( "86D97641-6117-4490-BDFF-0E976CFDA05A", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" ); // Include Current QueryString
                RockMigrationHelper.AddBlockAttributeValue( "86D97641-6117-4490-BDFF-0E976CFDA05A", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" ); // Enable Debug
                RockMigrationHelper.AddBlockAttributeValue( "86D97641-6117-4490-BDFF-0E976CFDA05A", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" ); // Is Secondary Block
                RockMigrationHelper.AddBlockAttributeValue( "86D97641-6117-4490-BDFF-0E976CFDA05A", "0A49DABE-42EE-40E5-9E06-0E6530944865", @"" ); // Include Page List
                this.Sql( "UPDATE AttributeValue SET Value = '/page/'+(SELECT CAST(Id AS varchar(max)) FROM Page WHERE Guid = 'E9B6F39A-276B-4F31-91C4-5EE9FF8AEF20') WHERE EntityId = (SELECT id FROM Block WHERE Guid = '53BDE0F5-F8A0-4706-9027-DDA6C99649CE') AND AttributeId = (SELECT Id FROM Attribute WHERE Guid = '964D33F4-27D0-4715-86BE-D30CEB895044')" );
            }


            // Page: Nursing Home List     
            if ( !pageService.Queryable().Any(p => p.Guid == new Guid( "EE90E17C-6AC6-4304-91A6-FCC92406AA0A" ) ) )
            {
                RockMigrationHelper.AddPage( "FFF16609-1DEA-4B76-9CDC-49277587B470", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Nursing Home List", "", "EE90E17C-6AC6-4304-91A6-FCC92406AA0A", "fa fa-dashboard" ); // Site:Rock RMS
                RockMigrationHelper.UpdateBlockType( "Nursing Home List", "A summary of all the current nursing home residents that have been reported to Southeast.", "~/Plugins/org_secc/PastoralCare/NursingHomeList.ascx", "SECC > Pastoral Care", "AE9E0F36-3511-4536-966E-172275C94E06" );
                RockMigrationHelper.AddBlock( "EE90E17C-6AC6-4304-91A6-FCC92406AA0A", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Main", "", "", 0, "4D4CD48C-E201-44E6-A674-8E9E7F74E7BC" );
                RockMigrationHelper.AddBlock( "EE90E17C-6AC6-4304-91A6-FCC92406AA0A", "", "AE9E0F36-3511-4536-966E-172275C94E06", "Homebound List", "Main", "", "", 1, "3F21AF65-F5B3-404B-97DE-18837D273E06" );
                RockMigrationHelper.AddBlockTypeAttribute( "AE9E0F36-3511-4536-966E-172275C94E06", "BC48720C-3610-4BCF-AE66-D255A17F1CDF", "Nursing Home List", "NursingHomeList", "", "", 0, @"", "E4419877-5A1B-448F-BD07-4A34A1BDBD7E" );
                RockMigrationHelper.AddBlockTypeAttribute( "AE9E0F36-3511-4536-966E-172275C94E06", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Nursing Home Resident Workflow", "NursingHomeResidentWorkflow", "", "", 0, @"", "B27E47F0-3FB6-4417-90C9-3BE099056330" );
                RockMigrationHelper.AddBlockAttributeValue( "3F21AF65-F5B3-404B-97DE-18837D273E06", "E4419877-5A1B-448F-BD07-4A34A1BDBD7E", @"89C2E347-BDEF-4BF2-8A25-9D4EE2E9B405" ); // Nursing Home List  
                RockMigrationHelper.AddBlockAttributeValue( "3F21AF65-F5B3-404B-97DE-18837D273E06", "B27E47F0-3FB6-4417-90C9-3BE099056330", @"7818dfd9-e347-43b2-95e3-8fbf83ab962d" ); // Nursing Home Resident Workflow 
                RockMigrationHelper.AddBlockAttributeValue( "4D4CD48C-E201-44E6-A674-8E9E7F74E7BC", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" ); // CSS File
                RockMigrationHelper.AddBlockAttributeValue( "4D4CD48C-E201-44E6-A674-8E9E7F74E7BC", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" ); // Include Current Parameters
                RockMigrationHelper.AddBlockAttributeValue( "4D4CD48C-E201-44E6-A674-8E9E7F74E7BC", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsTabs.lava' %}<br />" ); // Template
                RockMigrationHelper.AddBlockAttributeValue( "4D4CD48C-E201-44E6-A674-8E9E7F74E7BC", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"fff16609-1dea-4b76-9cdc-49277587b470" ); // Root Page
                RockMigrationHelper.AddBlockAttributeValue( "4D4CD48C-E201-44E6-A674-8E9E7F74E7BC", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" ); // Number of Levels
                RockMigrationHelper.AddBlockAttributeValue( "4D4CD48C-E201-44E6-A674-8E9E7F74E7BC", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" ); // Include Current QueryString
                RockMigrationHelper.AddBlockAttributeValue( "4D4CD48C-E201-44E6-A674-8E9E7F74E7BC", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" ); // Enable Debug
                RockMigrationHelper.AddBlockAttributeValue( "4D4CD48C-E201-44E6-A674-8E9E7F74E7BC", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" ); // Is Secondary Block
                RockMigrationHelper.AddBlockAttributeValue( "4D4CD48C-E201-44E6-A674-8E9E7F74E7BC", "0A49DABE-42EE-40E5-9E06-0E6530944865", @"" ); // Include Page List
            }

            // Page: Homebound List     
            if ( !pageService.Queryable().Any(p => p.Guid == new Guid( "43A64916-0DE1-470D-B000-3D2104BB49D0" ) ) )
            {
                RockMigrationHelper.AddPage( "FFF16609-1DEA-4B76-9CDC-49277587B470", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Homebound List", "", "43A64916-0DE1-470D-B000-3D2104BB49D0", "fa fa-dashboard" ); // Site:Rock RMS
                RockMigrationHelper.UpdateBlockType( "Homebound List", "A summary of all the current homebound residents that have been reported to Southeast.", "~/Plugins/org_secc/PastoralCare/HomeboundList.ascx", "SECC > Pastoral Care", "A21E269D-F50B-42C6-882A-126F3D29D6B2" );
                RockMigrationHelper.AddBlock( "43A64916-0DE1-470D-B000-3D2104BB49D0", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Main", "", "", 0, "5C923D9C-D1F6-442F-81D2-F50D47CE0138" );
                RockMigrationHelper.AddBlock( "43A64916-0DE1-470D-B000-3D2104BB49D0", "", "A21E269D-F50B-42C6-882A-126F3D29D6B2", "Homebound List", "Main", "", "", 1, "2357409B-E45C-4178-AB0B-6EEA947D3B36" );
                RockMigrationHelper.AddBlockTypeAttribute( "A21E269D-F50B-42C6-882A-126F3D29D6B2", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Homebound Person Workflow", "HomeboundPersonWorkflow", "", "", 0, @"", "00EB0501-6B5C-4036-AD9B-1A29A0BC657B" );
                RockMigrationHelper.AddBlockAttributeValue( "2357409B-E45C-4178-AB0B-6EEA947D3B36", "00EB0501-6B5C-4036-AD9B-1A29A0BC657B", @"3621645f-fbd0-4741-90ec-e032354aa375" ); // Homebound Person Workflow  
                RockMigrationHelper.AddBlockAttributeValue( "5C923D9C-D1F6-442F-81D2-F50D47CE0138", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" ); // CSS File
                RockMigrationHelper.AddBlockAttributeValue( "5C923D9C-D1F6-442F-81D2-F50D47CE0138", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" ); // Include Current Parameters
                RockMigrationHelper.AddBlockAttributeValue( "5C923D9C-D1F6-442F-81D2-F50D47CE0138", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsTabs.lava' %}<br />" ); // Template
                RockMigrationHelper.AddBlockAttributeValue( "5C923D9C-D1F6-442F-81D2-F50D47CE0138", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"fff16609-1dea-4b76-9cdc-49277587b470" ); // Root Page
                RockMigrationHelper.AddBlockAttributeValue( "5C923D9C-D1F6-442F-81D2-F50D47CE0138", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" ); // Number of Levels
                RockMigrationHelper.AddBlockAttributeValue( "5C923D9C-D1F6-442F-81D2-F50D47CE0138", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" ); // Include Current QueryString
                RockMigrationHelper.AddBlockAttributeValue( "5C923D9C-D1F6-442F-81D2-F50D47CE0138", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" ); // Enable Debug
                RockMigrationHelper.AddBlockAttributeValue( "5C923D9C-D1F6-442F-81D2-F50D47CE0138", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" ); // Is Secondary Block
                RockMigrationHelper.AddBlockAttributeValue( "5C923D9C-D1F6-442F-81D2-F50D47CE0138", "0A49DABE-42EE-40E5-9E06-0E6530944865", @"" ); // Include Page List
            }

            // Page: Communion List          
            if ( !pageService.Queryable().Any(p => p.Guid == new Guid( "9DC7E3BB-1C07-4964-8DD5-F6F1C25B3375" ) ) )
            {
                RockMigrationHelper.AddPage( "FFF16609-1DEA-4B76-9CDC-49277587B470", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Communion List", "", "9DC7E3BB-1C07-4964-8DD5-F6F1C25B3375", "fa fa-dashboard" ); // Site:Rock RMS
                RockMigrationHelper.UpdateBlockType( "Communion List", "A list of all the pastoral care patients/residents that have been requested communion.", "~/Plugins/org_secc/PastoralCare/CommunionList.ascx", "SECC > Pastoral Care", "F1991D17-D7A5-46C2-B7D4-C9CC0FAC460E" );
                RockMigrationHelper.AddBlock( "9DC7E3BB-1C07-4964-8DD5-F6F1C25B3375", "", "CACB9D1A-A820-4587-986A-D66A69EE9948", "Page Menu", "Main", "", "", 0, "881C39C5-E362-4DCC-823C-5B2F393BEEFA" );
                RockMigrationHelper.AddBlock( "9DC7E3BB-1C07-4964-8DD5-F6F1C25B3375", "", "F1991D17-D7A5-46C2-B7D4-C9CC0FAC460E", "Communion List", "Main", "", "", 1, "7FA3CC8F-D81B-442A-9210-4A277450185D" );
                RockMigrationHelper.AddBlockTypeAttribute( "F1991D17-D7A5-46C2-B7D4-C9CC0FAC460E", "BC48720C-3610-4BCF-AE66-D255A17F1CDF", "Hospital List", "HospitalList", "", "", 0, @"", "54E62971-BA92-4B6E-88EA-CDB74471A8F2" );
                RockMigrationHelper.AddBlockTypeAttribute( "F1991D17-D7A5-46C2-B7D4-C9CC0FAC460E", "BC48720C-3610-4BCF-AE66-D255A17F1CDF", "Nursing Home List", "NursingHomeList", "", "", 0, @"", "0CBDF379-F0E3-497B-8BC6-0BE0DE0BBE60" );
                RockMigrationHelper.AddBlockTypeAttribute( "F1991D17-D7A5-46C2-B7D4-C9CC0FAC460E", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Nursing Home Resident Workflow", "NursingHomeResidentWorkflow", "", "", 0, @"", "1B11C118-AC1D-4ED0-849E-F59DAA197B95" );
                RockMigrationHelper.AddBlockTypeAttribute( "F1991D17-D7A5-46C2-B7D4-C9CC0FAC460E", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Homebound Person Workflow", "HomeboundPersonWorkflow", "", "", 0, @"", "BBCE6E27-2334-4404-A816-934E9A465A6C" );
                RockMigrationHelper.AddBlockTypeAttribute( "F1991D17-D7A5-46C2-B7D4-C9CC0FAC460E", "46A03F59-55D3-4ACE-ADD5-B4642225DD20", "Hospital Admission Workflow", "HospitalAdmissionWorkflow", "", "", 0, @"", "75D12313-BD94-414B-84EE-7A0940ABA1EB" );
                RockMigrationHelper.AddBlockAttributeValue( "7FA3CC8F-D81B-442A-9210-4A277450185D", "54E62971-BA92-4B6E-88EA-CDB74471A8F2", @"0913F7A9-A2BF-479C-96EC-6CDB56310A83" ); // Hospital List  
                RockMigrationHelper.AddBlockAttributeValue( "7FA3CC8F-D81B-442A-9210-4A277450185D", "0CBDF379-F0E3-497B-8BC6-0BE0DE0BBE60", @"89C2E347-BDEF-4BF2-8A25-9D4EE2E9B405" ); // Nursing Home List  
                RockMigrationHelper.AddBlockAttributeValue( "7FA3CC8F-D81B-442A-9210-4A277450185D", "1B11C118-AC1D-4ED0-849E-F59DAA197B95", @"7818dfd9-e347-43b2-95e3-8fbf83ab962d" ); // Nursing Home Resident Workflow  
                RockMigrationHelper.AddBlockAttributeValue( "7FA3CC8F-D81B-442A-9210-4A277450185D", "BBCE6E27-2334-4404-A816-934E9A465A6C", @"3621645f-fbd0-4741-90ec-e032354aa375" ); // Homebound Person Workflow  
                RockMigrationHelper.AddBlockAttributeValue( "7FA3CC8F-D81B-442A-9210-4A277450185D", "75D12313-BD94-414B-84EE-7A0940ABA1EB", @"314cc992-c90c-4d7d-aec6-09c0fb4c7a38" ); // Hospital Admission Workflow  
                RockMigrationHelper.AddBlockAttributeValue( "881C39C5-E362-4DCC-823C-5B2F393BEEFA", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" ); // CSS File
                RockMigrationHelper.AddBlockAttributeValue( "881C39C5-E362-4DCC-823C-5B2F393BEEFA", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"False" ); // Include Current Parameters
                RockMigrationHelper.AddBlockAttributeValue( "881C39C5-E362-4DCC-823C-5B2F393BEEFA", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include '~~/Assets/Lava/PageListAsTabs.lava' %}<br />" ); // Template
                RockMigrationHelper.AddBlockAttributeValue( "881C39C5-E362-4DCC-823C-5B2F393BEEFA", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"fff16609-1dea-4b76-9cdc-49277587b470" ); // Root Page
                RockMigrationHelper.AddBlockAttributeValue( "881C39C5-E362-4DCC-823C-5B2F393BEEFA", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"1" ); // Number of Levels
                RockMigrationHelper.AddBlockAttributeValue( "881C39C5-E362-4DCC-823C-5B2F393BEEFA", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"False" ); // Include Current QueryString
                RockMigrationHelper.AddBlockAttributeValue( "881C39C5-E362-4DCC-823C-5B2F393BEEFA", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" ); // Enable Debug
                RockMigrationHelper.AddBlockAttributeValue( "881C39C5-E362-4DCC-823C-5B2F393BEEFA", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" ); // Is Secondary Block
                RockMigrationHelper.AddBlockAttributeValue( "881C39C5-E362-4DCC-823C-5B2F393BEEFA", "0A49DABE-42EE-40E5-9E06-0E6530944865", @"" ); // Include Page List
            }
            // Page: Add Hospitalization    
            if ( !pageService.Queryable().Any(p => p.Guid == new Guid( "46416DF6-6157-42E3-9330-2BDE8E79DA4C" ) ) )
            {
                RockMigrationHelper.AddPage( "A64D2C30-1205-41D4-B8FA-CCE4FBC47906", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Add Hospitalization", "", "46416DF6-6157-42E3-9330-2BDE8E79DA4C", "fa fa-hospital-o" ); // Site:Rock RMS
                RockMigrationHelper.AddPageRoute( "46416DF6-6157-42E3-9330-2BDE8E79DA4C", "Pastoral/Hospitalization/{WorkflowId}" );
                RockMigrationHelper.AddPageRoute( "46416DF6-6157-42E3-9330-2BDE8E79DA4C", "Pastoral/Hospitalization/" );
                RockMigrationHelper.UpdateBlockType( "Workflow Entry", "Used to enter information for a workflow form entry action.", "~/Blocks/WorkFlow/WorkflowEntry.ascx", "WorkFlow", "A8BD05C8-6F89-4628-845B-059E686F089A" );
                RockMigrationHelper.AddBlock( "46416DF6-6157-42E3-9330-2BDE8E79DA4C", "", "A8BD05C8-6F89-4628-845B-059E686F089A", "Hospitalization Entry", "Main", "", "", 0, "F2610A46-19FF-43E2-ACEC-AD0EAE57839B" );
                RockMigrationHelper.AddBlockAttributeValue( "F2610A46-19FF-43E2-ACEC-AD0EAE57839B", "2F1D98C4-A8EF-4680-9F64-11BFC28D5597", @"314cc992-c90c-4d7d-aec6-09c0fb4c7a38" ); // Workflow Type  
            }

            // Page: Add Nursing Home Resident    
            if ( !pageService.Queryable().Any(p => p.Guid == new Guid( "368DA4E7-E353-49BF-8AFD-862A66AC6878" ) ) )
            {
                RockMigrationHelper.AddPage( "A64D2C30-1205-41D4-B8FA-CCE4FBC47906", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Add Nursing Home Resident", "", "368DA4E7-E353-49BF-8AFD-862A66AC6878", "fa fa-wheelchair" ); // Site:Rock RMS
                RockMigrationHelper.AddPageRoute( "368DA4E7-E353-49BF-8AFD-862A66AC6878", "Pastoral/NursingHome/" );
                RockMigrationHelper.AddPageRoute( "368DA4E7-E353-49BF-8AFD-862A66AC6878", "Pastoral/NursingHome/{WorkflowId}" );
                RockMigrationHelper.UpdateBlockType( "Workflow Entry", "Used to enter information for a workflow form entry action.", "~/Blocks/WorkFlow/WorkflowEntry.ascx", "WorkFlow", "A8BD05C8-6F89-4628-845B-059E686F089A" );
                RockMigrationHelper.AddBlock( "368DA4E7-E353-49BF-8AFD-862A66AC6878", "", "A8BD05C8-6F89-4628-845B-059E686F089A", "Nursing Home Resident Entry", "Main", "", "", 0, "F391E7AA-A0F6-4981-957E-B22897C56BE8" );
                RockMigrationHelper.AddBlockAttributeValue( "F391E7AA-A0F6-4981-957E-B22897C56BE8", "2F1D98C4-A8EF-4680-9F64-11BFC28D5597", @"7818dfd9-e347-43b2-95e3-8fbf83ab962d" ); // Workflow Type  
            }

            // Page: Add Homebound Resident          
            if ( !pageService.Queryable().Any(p => p.Guid == new Guid( "F6EBCC3B-208D-472A-A0D1-2051D16DB9FB" ) ) )
            {
                RockMigrationHelper.AddPage( "A64D2C30-1205-41D4-B8FA-CCE4FBC47906", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Add Homebound Resident", "", "F6EBCC3B-208D-472A-A0D1-2051D16DB9FB", "fa fa-bed" ); // Site:Rock RMS
                RockMigrationHelper.AddPageRoute( "F6EBCC3B-208D-472A-A0D1-2051D16DB9FB", "Pastoral/Homebound" );
                RockMigrationHelper.AddPageRoute( "F6EBCC3B-208D-472A-A0D1-2051D16DB9FB", "Pastoral/Homebound/{WorkflowId}" );
                RockMigrationHelper.UpdateBlockType( "Workflow Entry", "Used to enter information for a workflow form entry action.", "~/Blocks/WorkFlow/WorkflowEntry.ascx", "WorkFlow", "A8BD05C8-6F89-4628-845B-059E686F089A" );
                RockMigrationHelper.AddBlock( "F6EBCC3B-208D-472A-A0D1-2051D16DB9FB", "", "A8BD05C8-6F89-4628-845B-059E686F089A", "Homebound Resident Entry", "Main", "", "", 0, "96DB3ADF-6631-4F1C-BEB6-59BAC576078A" );
                RockMigrationHelper.AddBlockAttributeValue( "96DB3ADF-6631-4F1C-BEB6-59BAC576078A", "2F1D98C4-A8EF-4680-9F64-11BFC28D5597", @"3621645f-fbd0-4741-90ec-e032354aa375" ); // Workflow Type  
            }

            // Page: Pastoral      
            if ( !pageService.Queryable().Any(p => p.Guid == new Guid( "C3784593-107E-4BE8-92D1-C11F0143724D" ) ) )
            {
                RockMigrationHelper.AddPage( "BF04BB7E-BE3A-4A38-A37C-386B55496303", "F66758C6-3E3D-4598-AF4C-B317047B5987", "Pastoral", "", "C3784593-107E-4BE8-92D1-C11F0143724D", "" ); // Site:Rock RMS
                RockMigrationHelper.AddPageRoute( "C3784593-107E-4BE8-92D1-C11F0143724D", "Person/{PersonId}/Pastoral" );
                RockMigrationHelper.AddBlock( "C3784593-107E-4BE8-92D1-C11F0143724D", "", "230AEE8C-A2B7-465C-93BE-A92F23364082", "Hospital List", "SectionC1", "", "", 0, "F2C5B8F9-157F-4252-B162-9B34D02DBC4D" );
                RockMigrationHelper.AddBlock( "C3784593-107E-4BE8-92D1-C11F0143724D", "", "AE9E0F36-3511-4536-966E-172275C94E06", "Nursing Home List", "SectionC1", "", "", 1, "E1C3F37B-A409-45D2-A01A-193A5DC8C4A7" );
                RockMigrationHelper.AddBlock( "C3784593-107E-4BE8-92D1-C11F0143724D", "", "A21E269D-F50B-42C6-882A-126F3D29D6B2", "Homebound List", "SectionC1", "", "", 2, "1838271C-723C-46C5-B61F-14FB126132D1" );
                RockMigrationHelper.AddBlockAttributeValue( "F2C5B8F9-157F-4252-B162-9B34D02DBC4D", "44690B0A-6868-4B10-8085-F3D12EB5931A", @"314cc992-c90c-4d7d-aec6-09c0fb4c7a38" ); // Hospital Admission Workflow  
                RockMigrationHelper.AddBlockAttributeValue( "E1C3F37B-A409-45D2-A01A-193A5DC8C4A7", "E4419877-5A1B-448F-BD07-4A34A1BDBD7E", @"89C2E347-BDEF-4BF2-8A25-9D4EE2E9B405" ); // Nursing Home List  
                RockMigrationHelper.AddBlockAttributeValue( "E1C3F37B-A409-45D2-A01A-193A5DC8C4A7", "B27E47F0-3FB6-4417-90C9-3BE099056330", @"7818dfd9-e347-43b2-95e3-8fbf83ab962d" ); // Nursing Home Resident Workflow  
                RockMigrationHelper.AddBlockAttributeValue( "1838271C-723C-46C5-B61F-14FB126132D1", "00EB0501-6B5C-4036-AD9B-1A29A0BC657B", @"3621645f-fbd0-4741-90ec-e032354aa375" ); // Homebound Person Workflow  
                                                                                                                                                                                        // Add/Update PageContext for Page:Pastoral, Entity: Rock.Model.Person, Parameter: PersonId              
                RockMigrationHelper.UpdatePageContext( "C3784593-107E-4BE8-92D1-C11F0143724D", "Rock.Model.Person", "PersonId", "8E696522-9D33-4410-A850-89A08B586365" );
            }

            // Page: New Family
            if ( !pageService.Queryable().Any(p => p.Guid == new Guid( "7A0E0BC4-3658-4FDA-A7F6-137D8A367AFC" ) ) )
            {
                RockMigrationHelper.AddPage( "A64D2C30-1205-41D4-B8FA-CCE4FBC47906", "7CFA101B-2D20-4523-9EC5-3F30502797A5", "New Family", "", "7A0E0BC4-3658-4FDA-A7F6-137D8A367AFC", "" ); // Site:Rock RMS
                RockMigrationHelper.AddPageRoute( "7A0E0BC4-3658-4FDA-A7F6-137D8A367AFC", "Pastoral/NewFamily" );
                RockMigrationHelper.AddBlock( "7A0E0BC4-3658-4FDA-A7F6-137D8A367AFC", "", "DE156975-597A-4C55-A649-FE46712F91C3", "Add Family", "Main", "", "", 0, "C09B6DED-31BD-4E6A-8C47-A6C650B2C570" );

                Sql( "UPDATE [Page] SET DisplayInNavWhen = 2 WHERE Guid = '7A0E0BC4-3658-4FDA-A7F6-137D8A367AFC';" );
            }
        }
        public override void Down()
        {
        }
    }
}

