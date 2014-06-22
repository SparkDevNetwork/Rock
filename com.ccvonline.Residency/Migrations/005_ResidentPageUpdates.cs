using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.ccvonline.Residency.Migrations
{
    /// <summary>
    /// 
    /// </summary>
    [MigrationNumber( 5, "1.0.8" )]
    public class ResidentPageUpdates : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Attrib Value for Block:Resident Coursework Page Menu, Attribute:CSS File Page: Coursework, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "72CA76E1-F938-4EE6-A060-FBBC39FF5D68", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" );

            // Attrib Value for Block:Resident Coursework Page Menu, Attribute:Enable Debug Page: Coursework, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "72CA76E1-F938-4EE6-A060-FBBC39FF5D68", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" );

            // Attrib Value for Block:Resident Coursework Page Menu, Attribute:Include Current Parameters Page: Coursework, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "72CA76E1-F938-4EE6-A060-FBBC39FF5D68", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"True" );

            // Attrib Value for Block:Resident Coursework Page Menu, Attribute:Include Current QueryString Page: Coursework, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "72CA76E1-F938-4EE6-A060-FBBC39FF5D68", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"True" );

            // Attrib Value for Block:Resident Coursework Page Menu, Attribute:Is Secondary Block Page: Coursework, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "72CA76E1-F938-4EE6-A060-FBBC39FF5D68", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Attrib Value for Block:Resident Coursework Page Menu, Attribute:Number of Levels Page: Coursework, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "72CA76E1-F938-4EE6-A060-FBBC39FF5D68", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"3" );

            // Attrib Value for Block:Resident Coursework Page Menu, Attribute:Root Page Page: Coursework, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "72CA76E1-F938-4EE6-A060-FBBC39FF5D68", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"f98b0061-8327-4b96-8a5e-b3c58d899b31" );

            // Attrib Value for Block:Resident Coursework Page Menu, Attribute:Template Page: Coursework, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "72CA76E1-F938-4EE6-A060-FBBC39FF5D68", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include 'PageListAsTabs' %}
" );

            // Attrib Value for Block:Resident Assessments Page Menu, Attribute:CSS File Page: Assessments, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D3BA196E-69C4-4480-A474-EFFFFEDD8D55", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" );

            // Attrib Value for Block:Resident Assessments Page Menu, Attribute:Enable Debug Page: Assessments, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D3BA196E-69C4-4480-A474-EFFFFEDD8D55", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" );

            // Attrib Value for Block:Resident Assessments Page Menu, Attribute:Include Current Parameters Page: Assessments, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D3BA196E-69C4-4480-A474-EFFFFEDD8D55", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"True" );

            // Attrib Value for Block:Resident Assessments Page Menu, Attribute:Include Current QueryString Page: Assessments, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D3BA196E-69C4-4480-A474-EFFFFEDD8D55", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"True" );

            // Attrib Value for Block:Resident Assessments Page Menu, Attribute:Is Secondary Block Page: Assessments, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D3BA196E-69C4-4480-A474-EFFFFEDD8D55", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Attrib Value for Block:Resident Assessments Page Menu, Attribute:Number of Levels Page: Assessments, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D3BA196E-69C4-4480-A474-EFFFFEDD8D55", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"3" );

            // Attrib Value for Block:Resident Assessments Page Menu, Attribute:Root Page Page: Assessments, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D3BA196E-69C4-4480-A474-EFFFFEDD8D55", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"f98b0061-8327-4b96-8a5e-b3c58d899b31" );

            // Attrib Value for Block:Resident Assessments Page Menu, Attribute:Template Page: Assessments, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "D3BA196E-69C4-4480-A474-EFFFFEDD8D55", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include 'PageListAsTabs' %}

" );

            // Attrib Value for Block:Resident Project Page Menu, Attribute:CSS File Page: Projects, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9431600F-43EC-4024-A228-0EE51CBCFAB9", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" );

            // Attrib Value for Block:Resident Project Page Menu, Attribute:Enable Debug Page: Projects, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9431600F-43EC-4024-A228-0EE51CBCFAB9", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" );

            // Attrib Value for Block:Resident Project Page Menu, Attribute:Include Current Parameters Page: Projects, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9431600F-43EC-4024-A228-0EE51CBCFAB9", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"True" );

            // Attrib Value for Block:Resident Project Page Menu, Attribute:Include Current QueryString Page: Projects, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9431600F-43EC-4024-A228-0EE51CBCFAB9", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"True" );

            // Attrib Value for Block:Resident Project Page Menu, Attribute:Is Secondary Block Page: Projects, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9431600F-43EC-4024-A228-0EE51CBCFAB9", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Attrib Value for Block:Resident Project Page Menu, Attribute:Number of Levels Page: Projects, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9431600F-43EC-4024-A228-0EE51CBCFAB9", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"3" );

            // Attrib Value for Block:Resident Project Page Menu, Attribute:Root Page Page: Projects, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9431600F-43EC-4024-A228-0EE51CBCFAB9", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"826c0bff-c831-4427-98f9-57ff462d82f5" );

            // Attrib Value for Block:Resident Project Page Menu, Attribute:Template Page: Projects, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "9431600F-43EC-4024-A228-0EE51CBCFAB9", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include 'PageListAsTabs' %}" );

            // Attrib Value for Block:Resident Project Goals Page Menu, Attribute:CSS File Page: Goals, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "978A376A-7DC9-41BC-85B9-CFD9895ADCE1", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" );

            // Attrib Value for Block:Resident Project Goals Page Menu, Attribute:Enable Debug Page: Goals, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "978A376A-7DC9-41BC-85B9-CFD9895ADCE1", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" );

            // Attrib Value for Block:Resident Project Goals Page Menu, Attribute:Include Current Parameters Page: Goals, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "978A376A-7DC9-41BC-85B9-CFD9895ADCE1", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"True" );

            // Attrib Value for Block:Resident Project Goals Page Menu, Attribute:Include Current QueryString Page: Goals, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "978A376A-7DC9-41BC-85B9-CFD9895ADCE1", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"True" );

            // Attrib Value for Block:Resident Project Goals Page Menu, Attribute:Is Secondary Block Page: Goals, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "978A376A-7DC9-41BC-85B9-CFD9895ADCE1", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Attrib Value for Block:Resident Project Goals Page Menu, Attribute:Number of Levels Page: Goals, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "978A376A-7DC9-41BC-85B9-CFD9895ADCE1", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"3" );

            // Attrib Value for Block:Resident Project Goals Page Menu, Attribute:Root Page Page: Goals, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "978A376A-7DC9-41BC-85B9-CFD9895ADCE1", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"826c0bff-c831-4427-98f9-57ff462d82f5" );

            // Attrib Value for Block:Resident Project Goals Page Menu, Attribute:Template Page: Goals, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "978A376A-7DC9-41BC-85B9-CFD9895ADCE1", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include 'PageListAsTabs' %}
" );

            // Attrib Value for Block:Resident Project Notes Page Menu, Attribute:CSS File Page: Notes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8678D9C4-ECDF-48DB-8D27-87616415FF6F", "7A2010F0-0C0C-4CC5-A29B-9CBAE4DE3A22", @"" );

            // Attrib Value for Block:Resident Project Notes Page Menu, Attribute:Enable Debug Page: Notes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8678D9C4-ECDF-48DB-8D27-87616415FF6F", "2EF904CD-976E-4489-8C18-9BA43885ACD9", @"False" );

            // Attrib Value for Block:Resident Project Notes Page Menu, Attribute:Include Current Parameters Page: Notes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8678D9C4-ECDF-48DB-8D27-87616415FF6F", "EEE71DDE-C6BC-489B-BAA5-1753E322F183", @"True" );

            // Attrib Value for Block:Resident Project Notes Page Menu, Attribute:Include Current QueryString Page: Notes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8678D9C4-ECDF-48DB-8D27-87616415FF6F", "E4CF237D-1D12-4C93-AFD7-78EB296C4B69", @"True" );

            // Attrib Value for Block:Resident Project Notes Page Menu, Attribute:Is Secondary Block Page: Notes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8678D9C4-ECDF-48DB-8D27-87616415FF6F", "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2", @"False" );

            // Attrib Value for Block:Resident Project Notes Page Menu, Attribute:Number of Levels Page: Notes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8678D9C4-ECDF-48DB-8D27-87616415FF6F", "6C952052-BC79-41BA-8B88-AB8EA3E99648", @"3" );

            // Attrib Value for Block:Resident Project Notes Page Menu, Attribute:Root Page Page: Notes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8678D9C4-ECDF-48DB-8D27-87616415FF6F", "41F1C42E-2395-4063-BD4F-031DF8D5B231", @"826c0bff-c831-4427-98f9-57ff462d82f5" );

            // Attrib Value for Block:Resident Project Notes Page Menu, Attribute:Template Page: Notes, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "8678D9C4-ECDF-48DB-8D27-87616415FF6F", "1322186A-862A-4CF1-B349-28ECB67229BA", @"{% include 'PageListAsTabs' %}" );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            //
        }
    }
}
