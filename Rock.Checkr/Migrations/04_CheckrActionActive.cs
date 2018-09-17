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
using Rock.Checkr.Constants;
using Rock.Plugin;
using Rock.SystemGuid;

namespace Rock.Migrations
{
    [MigrationNumber( 4, "1.8.0" )]
    public class Checkr_ActionActive : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C4DAE3D6-931F-497F-AC00-60BAFA87B758", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "6BEBD4BE-EDC7-4757-B597-445FC60DB6ED" ); // Rock.Workflow.Action.BackgroundCheckRequest:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C4DAE3D6-931F-497F-AC00-60BAFA87B758", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Billing Code Attribute", "BillingCodeAttribute", "The attribute that contains the billing code to use when submitting background check.", 4, @"", "232B2F98-3B2F-4C53-81FC-061A92675C41" ); // Rock.Workflow.Action.BackgroundCheckRequest:Billing Code Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C4DAE3D6-931F-497F-AC00-60BAFA87B758", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Person Attribute", "PersonAttribute", "The Person attribute that contains the person who the background check should be submitted for.", 1, @"", "077A9C4E-86E7-42F6-BEC3-DBC8F57E6A13" ); // Rock.Workflow.Action.BackgroundCheckRequest:Person Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C4DAE3D6-931F-497F-AC00-60BAFA87B758", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Request Type Attribute", "RequestTypeAttribute", "The attribute that contains the type of background check to submit (Specific to provider).", 3, @"", "EC759165-949E-4966-BAFD-68A656A4EBF7" ); // Rock.Workflow.Action.BackgroundCheckRequest:Request Type Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C4DAE3D6-931F-497F-AC00-60BAFA87B758", "33E6DF69-BDFA-407A-9744-C175B60643AE", "SSN Attribute", "SSNAttribute", "The attribute that contains the Social Security Number of the person who the background check should be submitted for ( Must be an 'Encrypted Text' attribute )", 2, @"", "2631E72B-1D9B-40E8-B857-8B1D41943451" ); // Rock.Workflow.Action.BackgroundCheckRequest:SSN Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C4DAE3D6-931F-497F-AC00-60BAFA87B758", "A7486B0E-4CA2-4E00-A987-5544C7DABA76", "Background Check Provider", "Provider", "The Background Check provider to use", 0, @"", "6E2366B4-9F0E-454A-9DB1-E06263749C12" ); // Rock.Workflow.Action.BackgroundCheckRequest:Background Check Provider
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "C4DAE3D6-931F-497F-AC00-60BAFA87B758", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "3936E931-CC27-4C38-9AA5-AAA502057333" ); // Rock.Workflow.Action.BackgroundCheckRequest:Order

            RockMigrationHelper.AddActionTypeAttributeValue( "70DABC23-6587-4F18-8551-C655AA285F44", "6BEBD4BE-EDC7-4757-B597-445FC60DB6ED", @"False" ); // Background Check:Submit Request:Submit Request:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "70DABC23-6587-4F18-8551-C655AA285F44", "6E2366B4-9F0E-454A-9DB1-E06263749C12", @"8d9de88a-c649-47b2-ba5c-92a24f60ae61" ); // Background Check:Submit Request:Submit Request:Background Check Provider
            RockMigrationHelper.AddActionTypeAttributeValue( "70DABC23-6587-4F18-8551-C655AA285F44", "3936E931-CC27-4C38-9AA5-AAA502057333", @"" ); // Background Check:Submit Request:Submit Request:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "70DABC23-6587-4F18-8551-C655AA285F44", "077A9C4E-86E7-42F6-BEC3-DBC8F57E6A13", @"2d977682-2589-47bb-94e6-906a9587ee7c" ); // Background Check:Submit Request:Submit Request:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "70DABC23-6587-4F18-8551-C655AA285F44", "2631E72B-1D9B-40E8-B857-8B1D41943451", @"" ); // Background Check:Submit Request:Submit Request:SSN Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "70DABC23-6587-4F18-8551-C655AA285F44", "EC759165-949E-4966-BAFD-68A656A4EBF7", @"00b8c76c-0fff-4827-8abc-48215004686f" ); // Background Check:Submit Request:Submit Request:Request Type Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "70DABC23-6587-4F18-8551-C655AA285F44", "232B2F98-3B2F-4C53-81FC-061A92675C41", @"c4c12a1e-26b7-4580-b70c-baf64497f3e8" ); // Background Check:Submit Request:Submit Request:Billing Code Attribute

            RockMigrationHelper.AddEntityAttribute( "Rock.Checkr.Checkr", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "", "", "Active", "", "Should Service be used?", 0, "", "0ac1596b-0143-4939-aacd-0b14f6f74322" );
            RockMigrationHelper.AddAttributeValue( "0ac1596b-0143-4939-aacd-0b14f6f74322", 0, "True", "554468D0-A891-5281-4D08-FED46D756E28" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
