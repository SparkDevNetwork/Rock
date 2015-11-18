// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
    public partial class ProtectMyMinistry : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.BackgroundCheck",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PersonAliasId = c.Int(nullable: false),
                        WorkflowId = c.Int(),
                        RequestDate = c.DateTime(nullable: false),
                        ResponseDate = c.DateTime(),
                        RecordFound = c.Boolean(),
                        ResponseXml = c.String(),
                        ResponseDocumentId = c.Int(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.PersonAliasId, cascadeDelete: true)
                .ForeignKey("dbo.BinaryFile", t => t.ResponseDocumentId)
                .ForeignKey("dbo.Workflow", t => t.WorkflowId, cascadeDelete: true)
                .Index(t => t.PersonAliasId)
                .Index(t => t.WorkflowId)
                .Index(t => t.ResponseDocumentId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.ForeignId)
                .Index(t => t.ForeignGuid)
                .Index(t => t.ForeignKey);
            
            AddColumn("dbo.Location", "County", c => c.String(maxLength: 50));
            // Background Check Package Defined Type and attributes
            RockMigrationHelper.AddDefinedType( "Global", "Background Package", "The type of background check packages that are available to be used.", "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF" );
            RockMigrationHelper.AddDefinedTypeAttribute( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "PMM Package Name", "PMMPackageName", "The exact name (case is important) of the PMM Package to submit.", 0, "BASIC", "01E4D61D-6E23-4EF3-8AE1-6919590B0E70" );
            RockMigrationHelper.AddDefinedTypeAttribute( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Default County", "DefaultCounty", "The County to include with the background check package.", 1, "", "39AEB614-BA28-485D-B033-3DD52055DC20" );
            RockMigrationHelper.AddDefinedTypeAttribute( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Send County From Home Address", "SendHomeCounty", "Flag indicating if the county from the person's home address should be used instead (when available).", 2, "False", "EC942623-3B32-4EF4-A371-D4312A7AF3F8" );
            RockMigrationHelper.AddDefinedTypeAttribute( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Default State", "DefaultState", "The County to include with the background check package.", 3, "", "17093E08-F287-4A77-87B7-5FFA2337A8B7" );
            RockMigrationHelper.AddDefinedTypeAttribute( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Send State From Home Address", "SendHomeState", "Flag indicating if the state from the person's home address should be used instead (when available).", 4, "False", "96B3451B-4851-4509-8130-231A2042A4A1" );
            RockMigrationHelper.AddDefinedTypeAttribute( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Include MVR Info", "IncludeMVR", "The County to include with the background check package.", 5, "False", "1169005D-662B-4380-9FFD-BD6177037329" );

            // Seven Year Auto Package with attribute values
            RockMigrationHelper.UpdateDefinedValue( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "Seven Year Automatic",
                "The Seven Year Automatic package is the premier screening option and is the recommended package for all staff and pastors serving at your organization. Some churches also use this package for all volunteers.", "8470F648-58B6-405A-8C4D-CD661F6678DB", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8470F648-58B6-405A-8C4D-CD661F6678DB", "01E4D61D-6E23-4EF3-8AE1-6919590B0E70", "Auto County 7 yrs" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8470F648-58B6-405A-8C4D-CD661F6678DB", "39AEB614-BA28-485D-B033-3DD52055DC20", "" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8470F648-58B6-405A-8C4D-CD661F6678DB", "EC942623-3B32-4EF4-A371-D4312A7AF3F8", "False" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8470F648-58B6-405A-8C4D-CD661F6678DB", "17093E08-F287-4A77-87B7-5FFA2337A8B7", "" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8470F648-58B6-405A-8C4D-CD661F6678DB", "96B3451B-4851-4509-8130-231A2042A4A1", "False" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8470F648-58B6-405A-8C4D-CD661F6678DB", "1169005D-662B-4380-9FFD-BD6177037329", "False" );

            // BASIC Package with attribute values
            RockMigrationHelper.UpdateDefinedValue( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "BASIC",
                "The Basic Package is the minimum recommended package for all volunteer and staff screenings. It includes SSN Verification and Address History, National Criminal Database Search, National Sex Offender Search, Re-verification of criiminal records, Alias Names",
                "B091BE26-1EEA-4601-A65A-A3A75CDD7506", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B091BE26-1EEA-4601-A65A-A3A75CDD7506", "01E4D61D-6E23-4EF3-8AE1-6919590B0E70", "BASIC" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B091BE26-1EEA-4601-A65A-A3A75CDD7506", "39AEB614-BA28-485D-B033-3DD52055DC20", "" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B091BE26-1EEA-4601-A65A-A3A75CDD7506", "EC942623-3B32-4EF4-A371-D4312A7AF3F8", "False" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B091BE26-1EEA-4601-A65A-A3A75CDD7506", "17093E08-F287-4A77-87B7-5FFA2337A8B7", "" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B091BE26-1EEA-4601-A65A-A3A75CDD7506", "96B3451B-4851-4509-8130-231A2042A4A1", "False" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B091BE26-1EEA-4601-A65A-A3A75CDD7506", "1169005D-662B-4380-9FFD-BD6177037329", "False" );

            // PLUS Package with attribute values
            RockMigrationHelper.UpdateDefinedValue( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "PLUS",
                "Depending on your state, it may be recommended to use the PLUS package instead of the Basic. The PLUS package includes everything in the BASIC package with the addition of one county or statewide criminal court search.",
                "C542EFC7-1D22-4DBD-AF09-5C583FCD4FEF", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C542EFC7-1D22-4DBD-AF09-5C583FCD4FEF", "01E4D61D-6E23-4EF3-8AE1-6919590B0E70", "PLUS" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C542EFC7-1D22-4DBD-AF09-5C583FCD4FEF", "39AEB614-BA28-485D-B033-3DD52055DC20", "" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C542EFC7-1D22-4DBD-AF09-5C583FCD4FEF", "EC942623-3B32-4EF4-A371-D4312A7AF3F8", "False" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C542EFC7-1D22-4DBD-AF09-5C583FCD4FEF", "17093E08-F287-4A77-87B7-5FFA2337A8B7", "AZ" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C542EFC7-1D22-4DBD-AF09-5C583FCD4FEF", "96B3451B-4851-4509-8130-231A2042A4A1", "True" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C542EFC7-1D22-4DBD-AF09-5C583FCD4FEF", "1169005D-662B-4380-9FFD-BD6177037329", "False" );

            // PA 153 Package with attribute values
            RockMigrationHelper.UpdateDefinedValue( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "Pennsylvania Act 153",
                "If your organization is located in Pennsylvania, This package should be used when screening any volunteers or staff that will interact with children. This package includes all of the screening and reporting requirements mandated by Pennsylvania Act 153.",
                "AD47AECE-6779-41C5-A5C4-D3A9C1F849BEF", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AD47AECE-6779-41C5-A5C4-D3A9C1F849BE", "01E4D61D-6E23-4EF3-8AE1-6919590B0E70", "PA Package" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AD47AECE-6779-41C5-A5C4-D3A9C1F849BE", "39AEB614-BA28-485D-B033-3DD52055DC20", "" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AD47AECE-6779-41C5-A5C4-D3A9C1F849BE", "EC942623-3B32-4EF4-A371-D4312A7AF3F8", "False" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AD47AECE-6779-41C5-A5C4-D3A9C1F849BE", "17093E08-F287-4A77-87B7-5FFA2337A8B7", "" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AD47AECE-6779-41C5-A5C4-D3A9C1F849BE", "96B3451B-4851-4509-8130-231A2042A4A1", "False" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AD47AECE-6779-41C5-A5C4-D3A9C1F849BE", "1169005D-662B-4380-9FFD-BD6177037329", "False" );

            // Change the workflow package attribute to be a defined value field and add the qualifiers
            RockMigrationHelper.UpdateWorkflowTypeAttribute( "16D12EF7-C546-4039-9036-B73D118EDC90", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Type", "PackageType", "Value should be the type of background check to request from the vendor.", 9, @"", "A4CB9461-D77F-40E0-8DFF-C7838D78F2EC" ); // Background Check:Type
            RockMigrationHelper.AddAttributeQualifier( "A4CB9461-D77F-40E0-8DFF-C7838D78F2EC", "definedtype", @"BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "D03418DA-BC29-47C3-AA36-1051841C62F9" );
            RockMigrationHelper.AddAttributeQualifier( "A4CB9461-D77F-40E0-8DFF-C7838D78F2EC", "allowmultiple", @"False", "CE080F26-ACDA-45FB-BE30-0D88688FF99B" );
            RockMigrationHelper.AddAttributeQualifier( "A4CB9461-D77F-40E0-8DFF-C7838D78F2EC", "displaydescription", @"False", "101C2F35-28F8-4E20-B1E2-812D8F784E8F" );

            RockMigrationHelper.AddPage( "C831428A-6ACD-4D49-9B2D-046D399E3123", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Protect My Ministry", "", "E7F4B733-60FF-4FA3-AB17-0832E123F6F2", "fa fa-shield", "21DA6141-0A03-4F00-B0A8-3B110FBE2438" ); // Site:Rock RMS
            RockMigrationHelper.UpdateBlockType( "Protect My Ministry Settings", "Block for updating the settings used by the Protect My Ministry integration.", "~/Blocks/Security/BackgroundCheck/ProtectMyMinistrySettings.ascx", "Security  > Background Check", "AF36FA7E-BD2A-42A3-AF30-2FEBC1C46663" );
            RockMigrationHelper.UpdateBlockType( "Request List", "Lists all the background check requests.", "~/Blocks/Security/BackgroundCheck/RequestList.ascx", "Security > Background Check", "A25BE440-6A54-4A8C-9359-74DB5AE7E5F3" );
            RockMigrationHelper.AddBlock( "E7F4B733-60FF-4FA3-AB17-0832E123F6F2", "", "AF36FA7E-BD2A-42A3-AF30-2FEBC1C46663", "Protect My Ministry Settings", "Main", "", "", 0, "63AA839B-B6A1-4A57-A0DC-2F5B6DDA71BE" );
            RockMigrationHelper.AddBlock( "E7F4B733-60FF-4FA3-AB17-0832E123F6F2", "", "A25BE440-6A54-4A8C-9359-74DB5AE7E5F3", "Request List", "Main", "", "", 1, "29BDA0D1-7595-4ABC-ABA3-5E3E6409B21F" );

            Sql( MigrationSQL._201511182310362_ProtectMyMinistry );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlock( "29BDA0D1-7595-4ABC-ABA3-5E3E6409B21F" );
            RockMigrationHelper.DeleteBlock( "63AA839B-B6A1-4A57-A0DC-2F5B6DDA71BE" );
            RockMigrationHelper.DeleteBlockType( "AF36FA7E-BD2A-42A3-AF30-2FEBC1C46663" ); // Protect My Ministry Settings
            RockMigrationHelper.DeleteBlockType( "A25BE440-6A54-4A8C-9359-74DB5AE7E5F3" ); // Request List
            RockMigrationHelper.DeletePage( "E7F4B733-60FF-4FA3-AB17-0832E123F6F2" ); //  Page: Protect My Ministry, Layout: Full Width, Site: Rock RMS

            RockMigrationHelper.DeleteAttribute( "01E4D61D-6E23-4EF3-8AE1-6919590B0E70" );
            RockMigrationHelper.DeleteAttribute( "39AEB614-BA28-485D-B033-3DD52055DC20" );
            RockMigrationHelper.DeleteAttribute( "EC942623-3B32-4EF4-A371-D4312A7AF3F8" );
            RockMigrationHelper.DeleteAttribute( "17093E08-F287-4A77-87B7-5FFA2337A8B7" );
            RockMigrationHelper.DeleteAttribute( "96B3451B-4851-4509-8130-231A2042A4A1" );
            RockMigrationHelper.DeleteAttribute( "1169005D-662B-4380-9FFD-BD6177037329" );
            RockMigrationHelper.DeleteAttribute( "20A0DCC9-1C56-4801-9A38-2E6F37DA0252" );

            RockMigrationHelper.DeleteDefinedType( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF" );

            DropForeignKey("dbo.BackgroundCheck", "WorkflowId", "dbo.Workflow");
            DropForeignKey("dbo.BackgroundCheck", "ResponseDocumentId", "dbo.BinaryFile");
            DropForeignKey("dbo.BackgroundCheck", "PersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.BackgroundCheck", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.BackgroundCheck", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.BackgroundCheck", new[] { "ForeignKey" });
            DropIndex("dbo.BackgroundCheck", new[] { "ForeignGuid" });
            DropIndex("dbo.BackgroundCheck", new[] { "ForeignId" });
            DropIndex("dbo.BackgroundCheck", new[] { "Guid" });
            DropIndex("dbo.BackgroundCheck", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.BackgroundCheck", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.BackgroundCheck", new[] { "ResponseDocumentId" });
            DropIndex("dbo.BackgroundCheck", new[] { "WorkflowId" });
            DropIndex("dbo.BackgroundCheck", new[] { "PersonAliasId" });
            DropColumn("dbo.Location", "County");
            DropTable("dbo.BackgroundCheck");
        }
    }
}
