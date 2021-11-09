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
    public partial class UpdateBusinessDetailsPage : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Remove the Pledge List block
            RockMigrationHelper.DeleteBlock( "91850A29-BB1A-4E92-A798-DE7D6E09E671" );
            RockMigrationHelper.DeleteBlock( "39A2DA08-1995-4A39-A6AF-5F8B8DE7372F" );
            RockMigrationHelper.DeleteBlock( "21D15CAB-EC52-4DEB-87DB-38F6C393FBE7" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.AddBlock( "D2B43273-C64F-4F57-9AAE-9571E1982BAC", null, "694FF260-8C6F-4A59-93C9-CF3793FE30E6", "Scheduled Transaction List", "Main", "", "", 5, "91850A29-BB1A-4E92-A798-DE7D6E09E671" );
            RockMigrationHelper.AddBlock( "D2B43273-C64F-4F57-9AAE-9571E1982BAC", null, "7011E792-A75F-4F22-B17E-D3A58C0EDB6D", "Pledge List", "Main", "", "", 4, "39A2DA08-1995-4A39-A6AF-5F8B8DE7372F" );
            RockMigrationHelper.AddBlock( "D2B43273-C64F-4F57-9AAE-9571E1982BAC", null, "C4191011-0391-43DF-9A9D-BE4987C679A4", "Bank Account List", "Main", "", "", 7, "21D15CAB-EC52-4DEB-87DB-38F6C393FBE7" );

            // Attrib Value for Page/BlockBusiness Detail/Pledge List:Show Group Column (FieldType: Boolean)
            RockMigrationHelper.AddBlockAttributeValue("39A2DA08-1995-4A39-A6AF-5F8B8DE7372F","F2476138-7C16-404C-A4B6-600E39602601",@"False");
            // Attrib Value for Page/BlockBusiness Detail/Pledge List:Show Date Range Filter (FieldType: Boolean)
            RockMigrationHelper.AddBlockAttributeValue("39A2DA08-1995-4A39-A6AF-5F8B8DE7372F","0049EC69-9814-4322-833F-BD82F92C64E9",@"True");
            // Attrib Value for Page/BlockBusiness Detail/Pledge List:Show Person Filter (FieldType: Boolean)
            RockMigrationHelper.AddBlockAttributeValue("39A2DA08-1995-4A39-A6AF-5F8B8DE7372F","807B41A4-4286-434C-918A-FE3942A75F7B",@"True");
            // Attrib Value for Page/BlockBusiness Detail/Pledge List:Show Account Column (FieldType: Boolean)
            RockMigrationHelper.AddBlockAttributeValue("39A2DA08-1995-4A39-A6AF-5F8B8DE7372F","63A83579-C73A-4387-B317-D9852F6647F3",@"True");
            // Attrib Value for Page/BlockBusiness Detail/Pledge List:Show Account Filter (FieldType: Boolean)
            RockMigrationHelper.AddBlockAttributeValue("39A2DA08-1995-4A39-A6AF-5F8B8DE7372F","B16A3F35-C8A4-47B3-BA7A-E20098E7B028",@"True");
            // Attrib Value for Page/BlockBusiness Detail/Pledge List:Show Last Modified Date Column (FieldType: Boolean)
            RockMigrationHelper.AddBlockAttributeValue("39A2DA08-1995-4A39-A6AF-5F8B8DE7372F","B27608E5-E5BF-4AC4-8C7E-C2A26456480B",@"True");
            // Attrib Value for Page/BlockBusiness Detail/Pledge List:Limit Pledges To Current Person (FieldType: Boolean)
            RockMigrationHelper.AddBlockAttributeValue("39A2DA08-1995-4A39-A6AF-5F8B8DE7372F","6A056518-3E38-4E78-AF6F-16D5C23A057D",@"False");
            // Attrib Value for Page/BlockBusiness Detail/Pledge List:Show Last Modified Filter (FieldType: Boolean)
            RockMigrationHelper.AddBlockAttributeValue("39A2DA08-1995-4A39-A6AF-5F8B8DE7372F","550E6B86-98BF-4DA7-9B54-634ADE0EE466",@"True");
            // Attrib Value for Page/BlockBusiness Detail/Pledge List:Detail Page (FieldType: Page Reference)
            RockMigrationHelper.AddBlockAttributeValue("39A2DA08-1995-4A39-A6AF-5F8B8DE7372F","3E26B7DF-7A7F-4829-987F-47304C0F845E",@"EF7AA296-CA69-49BC-A28B-901A8AAA9466");
            // Attrib Value for Page/BlockBusiness Detail/Pledge List:Entity Type (FieldType: EntityType)
            RockMigrationHelper.AddBlockAttributeValue("39A2DA08-1995-4A39-A6AF-5F8B8DE7372F","E9245CFD-4B11-4CE2-A120-BB3AC47C0974",@"72657ED8-D16E-492E-AC12-144C5E7567E7");

            // Attrib Value for Page/BlockBusiness Detail/Scheduled Transaction List:Add Page (FieldType: Page Reference)
            RockMigrationHelper.AddBlockAttributeValue("91850A29-BB1A-4E92-A798-DE7D6E09E671","9BCE3FD8-9014-4120-9DCC-06C4936284BA",@"b1ca86dc-9890-4d26-8ebd-488044e1b3dd");
            // Attrib Value for Page/BlockBusiness Detail/Scheduled Transaction List:Entity Type (FieldType: EntityType)
            RockMigrationHelper.AddBlockAttributeValue("91850A29-BB1A-4E92-A798-DE7D6E09E671","375F7220-04C6-4E41-B99A-A2CE494FD74A",@"72657ed8-d16e-492e-ac12-144c5e7567e7");
            // Attrib Value for Page/BlockBusiness Detail/Scheduled Transaction List:View Page (FieldType: Page Reference)
            RockMigrationHelper.AddBlockAttributeValue("91850A29-BB1A-4E92-A798-DE7D6E09E671","47B99CD1-FB63-44D7-8586-45BDCDF51137",@"591204da-b586-454c-8bd5-85652ceaa553");
        }
    }
}
