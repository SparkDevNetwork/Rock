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

            RockMigrationHelper.AddDefinedType( "Global", "Protect My Ministry DVR Jurisdiction Codes", "The jurisdication codes available for MVR requests from Protect My Ministry.", "2F8821E8-05B9-4CD5-9FA4-303662AAC85D" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "AL", "Alabama" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "AK", "Alaska" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "AZ39M", "Arizona (39 month)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "AZ5Y", "Arizona 5 Year (Commercial use only)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "ARC", "Arkansas (Commercial)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "ARD", "Arkansas (Driver Check)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "ARI", "Arkansas (Insurance)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "CA", "California" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "CO", "Colorado" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "CT", "Connecticut" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "DE", "Delaware" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "DC", "District of Columbia" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "FL3", "Florida (3 Year)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "FL7", "Florida (7 Year)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "FLU", "Florida (Unlimited)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "GA3Y", "Georgia (3 Year)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "GA7Y", "Georgia (7 Year)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "GAL", "Georgia LRI" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "HI", "Hawaii" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "ID", "Idaho" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "IL", "Illinois" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "IN", "Indiana" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "IA", "Iowa" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "KS", "Kansas" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "KY", "Kentucky" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "LA", "Louisiana" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "ME", "Maine" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "MD", "Maryland" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "MA", "Massachusetts" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "MI", "Michigan" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "MN1M", "Minnesota (1 Month)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "MN1Y", "Minnesota (1 Year)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "MNC", "Minnesota (Complete)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "MS", "Mississippi" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "MO1M", "Missouri (1 Month)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "MO1Y", "Missouri (1 Year)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "MOC", "Missouri (Complete)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "MT", "Montana" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "NE", "Nebraska" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "NV", "Nevada" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "NH", "New Hampshire" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "NJ", "New Jersey" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "NM", "New Mexico" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "NY", "New York" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "NC3Y", "North Carolina (3 Year)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "NC7Y", "North Carolina (7 Year)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "ND", "North Dakota" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "OH", "Ohio" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "OK", "Oklahoma" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "OR", "Oregon" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "ORI", "Oregon (Insurance)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "PA10Y", "Pennsylvania (10 Year)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "PA3Y", "Pennsylvania (3 Year Insurance)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "RI", "Rhode Island" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "SC10Y", "South Carolina (10 Year)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "SC3Y", "South Carolina (3 Year)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "SD", "South Dakota" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "TN3Y", "Tennessee (3 Year)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "TNL", "Tennessee (Limited MVR)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "TX3", "Texas (3 Year)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "TXDC", "Texas (Driver Check)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "UT", "Utah" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "VT", "Vermont" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "VA", "Virginia" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "WA", "Washington" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "WADC", "Washington (Driver Check)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "WV", "West Virginia" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "WI", "Wisconsin" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "WIDC", "Wisconsin (Driver Check)" );
            RockMigrationHelper.AddDefinedValue( "2F8821E8-05B9-4CD5-9FA4-303662AAC85D", "WY", "Wyoming" );

            // Background Check Package Defined Type and attributes
            RockMigrationHelper.AddDefinedType( "Global", "Background Check Types", "The type of background checks that are available to be used.", "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF" );
            RockMigrationHelper.AddDefinedTypeAttribute( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Package Name", "PMMPackageName", "The exact name (case is important) of the PMM Package to submit.", 0, "", "01E4D61D-6E23-4EF3-8AE1-6919590B0E70" );
            RockMigrationHelper.AddDefinedTypeAttribute( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "County Criminal Search Default County", "DefaultCounty", "Include a valid county name to request a County Criminal Search (a default county or state is required when using the PLUS package).", 1, "", "39AEB614-BA28-485D-B033-3DD52055DC20" );
            RockMigrationHelper.AddDefinedTypeAttribute( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Home Address County for County search", "SendHomeCounty", "If the person's home address includes a county, use that instead of the default county.", 2, "False", "EC942623-3B32-4EF4-A371-D4312A7AF3F8" );
            RockMigrationHelper.AddDefinedTypeAttribute( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Statewide Criminal Search Default State", "DefaultState", "Include a valid state to request a Statewide Criminal Search (a default state or county is required when using the PLUS package).", 3, "", "17093E08-F287-4A77-87B7-5FFA2337A8B7" );
            RockMigrationHelper.AddDefinedTypeAttribute( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Home Address State For Statewide Search", "SendHomeState", "If the person's home address includes a state, use that instead of the default state.", 4, "False", "96B3451B-4851-4509-8130-231A2042A4A1" );
            RockMigrationHelper.AddDefinedTypeAttribute( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "MVR Jurisdiction Code", "MVRJurisdiction", "Select an MVR Jurisdiction code to request a Motor Vehicle Record search.", 5, "", "1169005D-662B-4380-9FFD-BD6177037329" );
            RockMigrationHelper.AddDefinedTypeAttribute( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Use Home State for MVR Search", "SendHomeStateMVR", "If the person's home address includes a state, use that instead of state from jurisdication code.", 6, "False", "9F7FD96A-BE7C-4CA3-91F8-10A0D1D6C1D0" );

            RockMigrationHelper.AddAttributeQualifier( "1169005D-662B-4380-9FFD-BD6177037329", "allowmultiple", "False", "DD29AF91-A5F2-4D61-8674-963C107EB465" );
            RockMigrationHelper.AddAttributeQualifier( "1169005D-662B-4380-9FFD-BD6177037329", "definedtype", "", "9599BD09-1B79-4E00-8FC6-D9DC19010E56" );
            RockMigrationHelper.AddAttributeQualifier( "1169005D-662B-4380-9FFD-BD6177037329", "displaydescription", "True", "3F7E913C-CE65-4B67-A248-E99028FCD18F" );

            // Seven Year Auto Package with attribute values
            RockMigrationHelper.UpdateDefinedValue( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "Seven Year Automatic",
                "The Seven Year Automatic package is the premier screening option and is the recommended package for all staff and pastors serving at your organization. Some churches also use this package for all volunteers.", "8470F648-58B6-405A-8C4D-CD661F6678DB", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8470F648-58B6-405A-8C4D-CD661F6678DB", "01E4D61D-6E23-4EF3-8AE1-6919590B0E70", "Auto County 7 yrs" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8470F648-58B6-405A-8C4D-CD661F6678DB", "39AEB614-BA28-485D-B033-3DD52055DC20", "" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8470F648-58B6-405A-8C4D-CD661F6678DB", "EC942623-3B32-4EF4-A371-D4312A7AF3F8", "False" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8470F648-58B6-405A-8C4D-CD661F6678DB", "17093E08-F287-4A77-87B7-5FFA2337A8B7", "" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8470F648-58B6-405A-8C4D-CD661F6678DB", "96B3451B-4851-4509-8130-231A2042A4A1", "False" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8470F648-58B6-405A-8C4D-CD661F6678DB", "1169005D-662B-4380-9FFD-BD6177037329", "" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "8470F648-58B6-405A-8C4D-CD661F6678DB", "9F7FD96A-BE7C-4CA3-91F8-10A0D1D6C1D0", "False" );

            // BASIC Package with attribute values
            RockMigrationHelper.UpdateDefinedValue( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "BASIC",
                "The Basic Package is the minimum recommended package for all volunteer and staff screenings. It includes SSN Verification and Address History, National Criminal Database Search, National Sex Offender Search, Re-verification of criiminal records, Alias Names",
                "B091BE26-1EEA-4601-A65A-A3A75CDD7506", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B091BE26-1EEA-4601-A65A-A3A75CDD7506", "01E4D61D-6E23-4EF3-8AE1-6919590B0E70", "BASIC" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B091BE26-1EEA-4601-A65A-A3A75CDD7506", "39AEB614-BA28-485D-B033-3DD52055DC20", "" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B091BE26-1EEA-4601-A65A-A3A75CDD7506", "EC942623-3B32-4EF4-A371-D4312A7AF3F8", "False" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B091BE26-1EEA-4601-A65A-A3A75CDD7506", "17093E08-F287-4A77-87B7-5FFA2337A8B7", "" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B091BE26-1EEA-4601-A65A-A3A75CDD7506", "96B3451B-4851-4509-8130-231A2042A4A1", "False" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B091BE26-1EEA-4601-A65A-A3A75CDD7506", "1169005D-662B-4380-9FFD-BD6177037329", "" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "B091BE26-1EEA-4601-A65A-A3A75CDD7506", "9F7FD96A-BE7C-4CA3-91F8-10A0D1D6C1D0", "False" );

            // PLUS Package with attribute values
            RockMigrationHelper.UpdateDefinedValue( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "PLUS",
                "Depending on your state, it may be recommended to use the PLUS package instead of the Basic. The PLUS package includes everything in the BASIC package with the addition of one county or statewide criminal court search.",
                "C542EFC7-1D22-4DBD-AF09-5C583FCD4FEF", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C542EFC7-1D22-4DBD-AF09-5C583FCD4FEF", "01E4D61D-6E23-4EF3-8AE1-6919590B0E70", "PLUS" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C542EFC7-1D22-4DBD-AF09-5C583FCD4FEF", "39AEB614-BA28-485D-B033-3DD52055DC20", "" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C542EFC7-1D22-4DBD-AF09-5C583FCD4FEF", "EC942623-3B32-4EF4-A371-D4312A7AF3F8", "False" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C542EFC7-1D22-4DBD-AF09-5C583FCD4FEF", "17093E08-F287-4A77-87B7-5FFA2337A8B7", "AZ" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C542EFC7-1D22-4DBD-AF09-5C583FCD4FEF", "96B3451B-4851-4509-8130-231A2042A4A1", "True" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C542EFC7-1D22-4DBD-AF09-5C583FCD4FEF", "1169005D-662B-4380-9FFD-BD6177037329", "" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "C542EFC7-1D22-4DBD-AF09-5C583FCD4FEF", "9F7FD96A-BE7C-4CA3-91F8-10A0D1D6C1D0", "False" );

            // PA 153 Package with attribute values
            RockMigrationHelper.UpdateDefinedValue( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "Pennsylvania Act 153",
                "If your organization is located in Pennsylvania, This package should be used when screening any volunteers or staff that will interact with children. This package includes all of the screening and reporting requirements mandated by Pennsylvania Act 153.",
                "AD47AECE-6779-41C5-A5C4-D3A9C1F849BEF", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AD47AECE-6779-41C5-A5C4-D3A9C1F849BE", "01E4D61D-6E23-4EF3-8AE1-6919590B0E70", "PA Package" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AD47AECE-6779-41C5-A5C4-D3A9C1F849BE", "39AEB614-BA28-485D-B033-3DD52055DC20", "" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AD47AECE-6779-41C5-A5C4-D3A9C1F849BE", "EC942623-3B32-4EF4-A371-D4312A7AF3F8", "False" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AD47AECE-6779-41C5-A5C4-D3A9C1F849BE", "17093E08-F287-4A77-87B7-5FFA2337A8B7", "" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AD47AECE-6779-41C5-A5C4-D3A9C1F849BE", "96B3451B-4851-4509-8130-231A2042A4A1", "False" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AD47AECE-6779-41C5-A5C4-D3A9C1F849BE", "1169005D-662B-4380-9FFD-BD6177037329", "" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "AD47AECE-6779-41C5-A5C4-D3A9C1F849BE", "9F7FD96A-BE7C-4CA3-91F8-10A0D1D6C1D0", "False" );

            // MVR Only
            RockMigrationHelper.UpdateDefinedValue( "BC2FDF9A-93B8-4325-8DE9-2F7B1943BFDF", "Motor Vehicle Record Search", "An A la carte Motor Vehicle Record (MVR) search.", "D27F591E-0016-4924-BC8D-F3F488DF3F8C", false );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D27F591E-0016-4924-BC8D-F3F488DF3F8C", "01E4D61D-6E23-4EF3-8AE1-6919590B0E70", "" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D27F591E-0016-4924-BC8D-F3F488DF3F8C", "39AEB614-BA28-485D-B033-3DD52055DC20", "" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D27F591E-0016-4924-BC8D-F3F488DF3F8C", "EC942623-3B32-4EF4-A371-D4312A7AF3F8", "False" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D27F591E-0016-4924-BC8D-F3F488DF3F8C", "17093E08-F287-4A77-87B7-5FFA2337A8B7", "" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D27F591E-0016-4924-BC8D-F3F488DF3F8C", "96B3451B-4851-4509-8130-231A2042A4A1", "False" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D27F591E-0016-4924-BC8D-F3F488DF3F8C", "1169005D-662B-4380-9FFD-BD6177037329", "" );
            RockMigrationHelper.AddDefinedValueAttributeValue( "D27F591E-0016-4924-BC8D-F3F488DF3F8C", "9F7FD96A-BE7C-4CA3-91F8-10A0D1D6C1D0", "True" );

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

            // Attrib for BlockType: Request List:Workflow Detail Page
            RockMigrationHelper.AddBlockTypeAttribute( "A25BE440-6A54-4A8C-9359-74DB5AE7E5F3", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Workflow Detail Page", "WorkflowDetailPage", "", "The page to view details about the background check workflow", 0, @"", "EBD0D19C-E73D-41AE-82D4-C89C21C35998" );

            // Attrib Value for Block:Request List, Attribute:Workflow Detail Page Page: Protect My Ministry, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "29BDA0D1-7595-4ABC-ABA3-5E3E6409B21F", "EBD0D19C-E73D-41AE-82D4-C89C21C35998", @"ba547eed-5537-49cf-bd4e-c583d760788c,0018e88f-1fea-48e2-93d6-dc254a0c4005" );

            RockMigrationHelper.UpdatePersonAttribute( Rock.SystemGuid.FieldType.TEXT, "4D1E1EBA-ABF2-4A7C-8ADF-65CB5AAE94E2", "Driver''s License Number", "com.sparkdevnetwork.DLNumber", "", "Driver''s License Number used for Motor Vehicle Record searches", 5, "", "04549D93-C674-441C-A6CF-ACDFA0EDB163" );

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
