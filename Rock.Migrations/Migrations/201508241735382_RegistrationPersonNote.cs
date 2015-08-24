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
    public partial class RegistrationPersonNote : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    UPDATE FinancialPaymentDetail 
    SET [BillingLocationId] = NULL
    WHERE [BillingLocationId] NOT IN ( 
        SELECT [Id] 
        FROM [Location]
    )
" );

            AddColumn("dbo.RegistrationTemplate", "AddPersonNote", c => c.Boolean(nullable: false));
            CreateIndex("dbo.FinancialPaymentDetail", "BillingLocationId");
            AddForeignKey("dbo.FinancialPaymentDetail", "BillingLocationId", "dbo.Location", "Id");

            // JE: Update Family Manager Config
            RockMigrationHelper.AddDefinedValueAttributeValue( "8BB6A099-1592-4E7F-AE8E-7FDEE63E040C", "ACC82748-157F-BF8E-4E34-FFD3C05269B3", @"{
	""familyAttributes"": [
	],
	""personAttributes"": [
		{""attributeId"": 715, ""filter"":""child"", ""required"": false },
		{""attributeId"": 676, ""filter"":""child"", ""required"": false }
	],
	
	""defaultState"" : ""AZ"",
	""requireFamilyAddress"": false,
	""requireChildBirthdate"": true,
	""requireAdultBirthdate"": false,
	""requireChildGrade"": true,
	""firstVisitPrompt"": false,
	
	""visualSettings"": 
	{
	    ""BackgroundColor"" : ""#dcd5cbff"",
		""LogoURL"" : ""~/Assets/FamilyManagerThemes/RockDefault/rock-logo.png"",
		""AdultMaleNoPhoto"" : ""~/Assets/FamilymanagerThemes/RockDefault/photo-adult-male.png"",
		""AdultFemaleNoPhoto"" : ""~/Assets/FamilyManagerThemes/RockDefault/photo-adult-female.png"",
		""ChildMaleNoPhoto"" : ""~/Assets/FamilyManagerThemes/RockDefault/photo-child-male.png"",
		""ChildFemaleNoPhoto"" : ""~/Assets/FamilyManagerThemes/RockDefault/photo-child-female.png"",
		
		""TopHeaderBGColor"" : ""#282526FF"",
		""TopHeaderTextColor"" : ""#d3cec5FF"",
		
		""FooterBGColor"" : ""#c2b8a7FF"",
		""FooterTextColor"" : ""#847f77FF"",
		
		""SidebarBGColor"" : ""#c2b8a7FF"",
	
		""SelectedPersonColor"" : ""#ee7624FF"",
		
		""PhotoOutlineColor"" : ""#555555FF"",
		
		""LargeFontSize"" : ""18"",
		""MediumFontSize"" : ""20"",
		""SmallFontSize"" : ""14"",
				
		""DefaultButtonStyle"" :
		{
			""BackgroundColor"" : ""#d6d6d6FF"",
			""TextColor"" : ""#524c43FF"",
			""BorderColor"" : ""#c2b8a7FF"",
			""BorderWidth"" : ""1"",
			""CornerRadius"" : ""4""
		 },
		 ""PrimaryButtonStyle"" : 
		 {
		 	""BackgroundColor"" : ""#ee7624FF"",
			""TextColor"" : ""#FFFFFFFF"",
			""BorderColor"" : ""#ee7624ff"",
			""BorderWidth"" : ""1"",
			""CornerRadius"" : ""4""
		 },
		 ""FamilyCellStyle"":
		 {
		 	""BackgroundColor"" : ""#00000000"",
			""AddFamilyButtonBGColor"" : ""#c2b8a7FF"",
			""AddFamilyButtonTextColor"" : ""#554e51FF"",
			""EntryBGColor"" : ""#c2b8a7FF"",
			""EntryTextColor"" : ""#554e51FF"",
		 },
		 ""ToggleStyle"" :
		 {
		 	""ActiveColor"" : ""#ee7624FF"",
			 ""InActiveColor"" : ""#777777FF"",
			 ""TextColor"" : ""#FFFFFFFF"",
			 ""CornerRadius"" : ""4"",
			 ""BorderWidth"" : ""1"",
			 ""BorderColor"" : ""#b1b1b1FF""
		 },
		 ""DatePickerStyle"" :
		 {
			""BackgroundColor"" : ""#c2b8a7FF"",
			""TextColor"" : ""#3b3b3bFF"",
			""Size"" : ""22"",
			""Font"" : ""OpenSans-Regular""
		 },
		 ""LabelStyle"" :
		 {
		 	""TextColor"" : ""#6a6a6aFF"",
		 	""Size"": ""14""
		 },
		 ""SwitchStyle"" :
		 {
		 	""OnColor"" : ""#FFFFFFFF""
		 },
		 ""TextFieldStyle"" :
		 {
		 	""TextColor"" : ""#555555FF"",
			""PlaceHolderColor"" : ""#AAAAAAFF"",
			""BorderColor"" : ""#969696FF"",
			""BorderWidth"" : ""1"",
			""CornerRadius"" : ""4"",
		 },
		 ""SearchResultStyle"":
		 {
		 	""BackgroundColor"" : ""#c2b8a7FF"",
			""TextColor"" : ""#3b3b3bFF""
		 }
	}
}" );
            // MP: Add Pledge List to Person Contribution Tab
            // Add Block to Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlock( "53CF4CBE-85F9-4A50-87D7-0D72A3FB2892", "", "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "Pledge List", "SectionC1", "", "", 0, "212EB093-026A-4177-ACE4-25EA9E1DDD41" );
            // update block order for pages with new blocks
            Sql( @"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = '212EB093-026A-4177-ACE4-25EA9E1DDD41'" );  // Page: Contributions,  Zone: SectionC1,  Block: Pledge List
            Sql( @"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = 'B33DF8C4-29B2-4DC5-B182-61FC255B01C0'" );  // Page: Contributions,  Zone: SectionC1,  Block: Finance - Giving Profile List
            Sql( @"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '9382B285-3EF6-47F7-94BB-A47C498196A3'" );  // Page: Contributions,  Zone: SectionC1,  Block: Transaction List
            Sql( @"UPDATE [Block] SET [Order] = 3 WHERE [Guid] = '7C698D61-81C9-4942-BFE3-9839130C1A3E'" );  // Page: Contributions,  Zone: SectionC1,  Block: Bank Account List
            // Attrib for BlockType: Pledge List:Entity Type
            RockMigrationHelper.AddBlockTypeAttribute( "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "3549BAB6-FE1B-4333-AFC4-C5ACA01BB8EB", "Entity Type", "ContextEntityType", "", "The type of entity that will provide context for this block", 0, @"", "E9245CFD-4B11-4CE2-A120-BB3AC47C0974" );
            // Attrib Value for Block:Pledge List, Attribute:Entity Type Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "212EB093-026A-4177-ACE4-25EA9E1DDD41", "E9245CFD-4B11-4CE2-A120-BB3AC47C0974", @"72657ed8-d16e-492e-ac12-144c5e7567e7" );
            // Attrib Value for Block:Pledge List, Attribute:Detail Page Page: Contributions, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "212EB093-026A-4177-ACE4-25EA9E1DDD41", "3E26B7DF-7A7F-4829-987F-47304C0F845E", @"ef7aa296-ca69-49bc-a28b-901a8aaa9466" );

            // TC: Add new CalendarLava block attributes (Up)
            RockMigrationHelper.DeleteAttribute( "C1259B83-C1AF-4880-81A3-16204D502DC9" );
            RockMigrationHelper.AddBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Campus Filter Display Mode", "CampusFilterDisplayMode", "", "", 3, @"1", "38202B12-44C1-40AF-A5A2-544CBE2B73D9" );
            RockMigrationHelper.AddBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "Category Filter Display Mode", "CategoryFilterDisplayMode", "", "", 4, @"1", "FE093D2C-0F00-4FFB-8ED2-6469D16797F7" );
            RockMigrationHelper.AddAttributeQualifier( "38202B12-44C1-40AF-A5A2-544CBE2B73D9", "fieldtype", "rb", "C83F7215-E310-4505-B844-CF5B343E2526" );
            RockMigrationHelper.AddAttributeQualifier( "FE093D2C-0F00-4FFB-8ED2-6469D16797F7", "fieldtype", "rb", "8921267B-DB8B-43D2-9C9A-6EE88FADF717" );
            RockMigrationHelper.AddAttributeQualifier( "38202B12-44C1-40AF-A5A2-544CBE2B73D9", "values", "1^Hidden, 2^Plain, 3^Panel Open, 4^Panel Closed", "32432AB1-E3D4-4930-9458-02E1E8807D18" );
            RockMigrationHelper.AddAttributeQualifier( "FE093D2C-0F00-4FFB-8ED2-6469D16797F7", "values", "1^Hidden, 2^Plain, 3^Panel Open, 4^Panel Closed", "8F6F3E7D-56CB-4E48-9E47-2229AA9AA5CB" );
            RockMigrationHelper.AddBlockAttributeValue( "0ADEEFE5-8293-48AC-AFA9-E0F0E363FCE7", "38202B12-44C1-40AF-A5A2-544CBE2B73D9", @"1" ); // Campus Filter Display Mode
            RockMigrationHelper.AddBlockAttributeValue( "0ADEEFE5-8293-48AC-AFA9-E0F0E363FCE7", "FE093D2C-0F00-4FFB-8ED2-6469D16797F7", @"1" ); // Category Filter Display Mode

            // JE: Add Check-in Power Search Type
            Sql( @"
    DECLARE @CheckInSearchTypeId int = (SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = '1EBCDB30-A89A-4C14-8580-8289EC2C7742')
    IF NOT EXISTS(SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '93773B0A-6E7F-1AA0-4F1D-9A4D6ACE930F')
    BEGIN
        INSERT INTO [DefinedValue]
	        ([IsSystem], [DefinedTypeId], [Order], [Value], [Description], [Guid])
        VALUES
	        (1, @CheckInSearchTypeId, 3, 'Name & Phone', 'This option allows you to search by phone and name at the same time. If the input has at least one non-numeric character the search will assume to be name. Otherwise phone number will be used.', '93773B0A-6E7F-1AA0-4F1D-9A4D6ACE930F')
    END
" );

            // JE: Rename 'Pledge List' page to 'Pledges' 
            Sql( @"
  UPDATE [Page]
  SET [InternalName] = 'Pledges', [BrowserTitle] = 'Pledges', [PageTitle] = 'Pledges'
  WHERE [Guid] = '1570D2AF-4FE2-4FC7-BED9-F20EBCBE9867'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // TC: Add new CalendarLava block attributes (Down)
            RockMigrationHelper.AddBlockTypeAttribute( "8760D668-8ADF-48C8-9D90-09461FB75B88", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Date Range Filter", "ShowDateRangeFilter", "", "Determines whether the date range filters are shown", 6, @"False", "C1259B83-C1AF-4880-81A3-16204D502DC9" );
            RockMigrationHelper.AddBlockAttributeValue( "0ADEEFE5-8293-48AC-AFA9-E0F0E363FCE7", "C1259B83-C1AF-4880-81A3-16204D502DC9", @"False" ); // Show Date Range Filter
            RockMigrationHelper.DeleteAttribute( "FE093D2C-0F00-4FFB-8ED2-6469D16797F7" );
            RockMigrationHelper.DeleteAttribute( "38202B12-44C1-40AF-A5A2-544CBE2B73D9" );

            DropForeignKey( "dbo.FinancialPaymentDetail", "BillingLocationId", "dbo.Location" );
            DropIndex("dbo.FinancialPaymentDetail", new[] { "BillingLocationId" });
            DropColumn("dbo.RegistrationTemplate", "AddPersonNote");
        }
    }
}
