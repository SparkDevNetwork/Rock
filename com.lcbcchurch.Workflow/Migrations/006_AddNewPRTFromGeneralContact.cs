// <copyright>
// Copyright by LCBC Church
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock;
using Rock.Plugin;
namespace com.lcbcchurch.Workflow.Migrations
{
    [MigrationNumber( 6, "1.0.14" )]
    public class AddNewPRTFromGeneralContact : Migration
    {
        public override void Up()
        {
            //Update the General Contact workflow, created in migration 001_GeneralContactWorkflow
            //Adds PRT create fuctionality with the addition of an activity and modified user entry button

            #region EntityTypes

            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F95C376A-714E-41FE-B27A-683F9E9078ED", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Active", "Active", "Should Service be used?", 0, @"False", "E595C6D3-054C-4D69-BCAC-E3FD2B74BD8F" ); // Rock.Workflow.Action.CreateConnectionRequest:Active
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F95C376A-714E-41FE-B27A-683F9E9078ED", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Campus Attribute", "CampusAttribute", "An optional attribute that contains the campus to use for the request.", 4, @"", "482C00E0-EA5F-4C5F-9C5F-EC09BA27AC87" ); // Rock.Workflow.Action.CreateConnectionRequest:Campus Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F95C376A-714E-41FE-B27A-683F9E9078ED", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Connection Comment Attribute", "ConnectionCommentAttribute", "An optional attribute that contains the comment to use for the request.", 5, @"", "A593FB30-ECF8-46C0-8B26-E9997174B271" ); // Rock.Workflow.Action.CreateConnectionRequest:Connection Comment Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F95C376A-714E-41FE-B27A-683F9E9078ED", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Connection Opportunity Attribute", "ConnectionOpportunityAttribute", "The attribute that contains the type of connection opportunity to create.", 1, @"", "67A54E5E-9804-4105-917F-A705790C1F7C" ); // Rock.Workflow.Action.CreateConnectionRequest:Connection Opportunity Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F95C376A-714E-41FE-B27A-683F9E9078ED", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Connection Request Attribute", "ConnectionRequestAttribute", "An optional connection request attribute to store the request that is created.", 6, @"", "9D7D651F-C7BF-4EF6-8003-89E11FB23F70" ); // Rock.Workflow.Action.CreateConnectionRequest:Connection Request Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F95C376A-714E-41FE-B27A-683F9E9078ED", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Connection Status Attribute", "ConnectionStatusAttribute", "The attribute that contains the connection status to use for the new request.", 2, @"", "F2090B64-6454-46CE-87AB-AA6A928C5CAD" ); // Rock.Workflow.Action.CreateConnectionRequest:Connection Status Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F95C376A-714E-41FE-B27A-683F9E9078ED", "33E6DF69-BDFA-407A-9744-C175B60643AE", "Person Attribute", "PersonAttribute", "The Person attribute that contains the person that connection request should be created for.", 0, @"", "DE52C3B1-2AA0-4305-ACCD-9617C613C394" ); // Rock.Workflow.Action.CreateConnectionRequest:Person Attribute
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F95C376A-714E-41FE-B27A-683F9E9078ED", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Order", "Order", "The order that this service should be used (priority)", 0, @"", "9629975D-722B-4A3C-8B6E-174F7AB946DC" ); // Rock.Workflow.Action.CreateConnectionRequest:Order
            RockMigrationHelper.UpdateWorkflowActionEntityAttribute( "F95C376A-714E-41FE-B27A-683F9E9078ED", "EC381D5D-729F-4581-A8F7-8C1FCE44DA98", "Connection Status", "ConnectionStatus", "The connection status to use for the new request (when Connection Status Attribute is not specified or invalid). If neither this setting or the Connection Status Attribute setting are set, the default status will be used.", 3, @"", "751713AC-210F-4825-9D6A-F2DDC1262ABC" ); // Rock.Workflow.Action.CreateConnectionRequest:Connection Status

            #endregion

            

            #region General Contact
            
            RockMigrationHelper.UpdateWorkflowActivityType( "9A5541BD-A914-4EE2-9CC0-DC6258D7D17D", true, "Create PRT Request", "Creates a Pastoral Response Team request from information found in a general contact form.", false, 5, "A2B80300-222D-44FE-8467-852FDF1A157C" ); // General Contact:Create PRT Request
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute( "A2B80300-222D-44FE-8467-852FDF1A157C", "B188B729-FE6D-498B-8871-65AB8FD1E11E", "PRT Connection Opportunity", "PRTConnectionOpportunity", "", 0, @"0F1B2894-4BF5-455C-AA5C-4FFDC6440D24", "EDA2C10D-DB54-44EC-B34C-0D32B906506E" ); // General Contact:Create PRT Request:PRT Connection Opportunity
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute( "A2B80300-222D-44FE-8467-852FDF1A157C", "C28C7BF3-A552-4D77-9408-DEDCF760CED0", "PRT Connection Comment", "PRTConnectionComment", "", 1, @"", "57475E8A-C0DD-415C-A6EA-257F01545B7D" ); // General Contact:Create PRT Request:PRT Connection Comment
            RockMigrationHelper.UpdateWorkflowActivityTypeAttribute( "A2B80300-222D-44FE-8467-852FDF1A157C", "9C204CD0-1233-41C5-818A-C5DA439445AA", "PRT Command Text", "PRTCommandText", "", 2, @"", "CF2A1BC7-7A3C-423E-B07F-655F0B54D34F" ); // General Contact:Create PRT Request:PRT Command Text

            RockMigrationHelper.AddAttributeQualifier( "EDA2C10D-DB54-44EC-B34C-0D32B906506E", "connectionTypeFilter", @"", "6AACCDD3-AE29-4D88-B024-F2D898105181" ); // General Contact:PRT Connection Opportunity:connectionTypeFilter
            RockMigrationHelper.AddAttributeQualifier( "EDA2C10D-DB54-44EC-B34C-0D32B906506E", "includeInactive", @"False", "DF7481DB-DF33-476A-9E45-2B542F635144" ); // General Contact:PRT Connection Opportunity:includeInactive
            RockMigrationHelper.AddAttributeQualifier( "57475E8A-C0DD-415C-A6EA-257F01545B7D", "allowhtml", @"False", "2BDC86E3-CBF5-431E-9DCF-E95CF02B8C64" ); // General Contact:PRT Connection Comment:allowhtml
            RockMigrationHelper.AddAttributeQualifier( "57475E8A-C0DD-415C-A6EA-257F01545B7D", "maxcharacters", @"", "41143731-61D3-4A18-AB97-170EC848BA6A" ); // General Contact:PRT Connection Comment:maxcharacters
            RockMigrationHelper.AddAttributeQualifier( "57475E8A-C0DD-415C-A6EA-257F01545B7D", "numberofrows", @"", "C066FAE2-074D-4818-B2F3-1073507D02A7" ); // General Contact:PRT Connection Comment:numberofrows
            RockMigrationHelper.AddAttributeQualifier( "57475E8A-C0DD-415C-A6EA-257F01545B7D", "showcountdown", @"False", "B7C03741-570C-41BF-98F2-EC42C51899CD" ); // General Contact:PRT Connection Comment:showcountdown
            RockMigrationHelper.AddAttributeQualifier( "CF2A1BC7-7A3C-423E-B07F-655F0B54D34F", "ispassword", @"False", "C3D61464-0DA7-40A1-A35F-F6FA6DE2DD7F" ); // General Contact:PRT Command Text:ispassword
            RockMigrationHelper.AddAttributeQualifier( "CF2A1BC7-7A3C-423E-B07F-655F0B54D34F", "maxcharacters", @"", "14375674-2681-4E92-B50A-B1950BB43858" ); // General Contact:PRT Command Text:maxcharacters
            RockMigrationHelper.AddAttributeQualifier( "CF2A1BC7-7A3C-423E-B07F-655F0B54D34F", "showcountdown", @"False", "BD6D88EC-524E-4E1E-8F8A-44DE6398D35C" ); // General Contact:PRT Command Text:showcountdown

            //Add 'Create PRT' button to Capture Notes user form
            RockMigrationHelper.UpdateWorkflowActionForm( @"<h4>{{ Workflow | Attribute:'Topic' }} Inquiry from {{ Workflow | Attribute:'FirstName' }} {{ Workflow | Attribute:'LastName' }}</h4> <p>The following inquiry has been submitted by a visitor to our website.</p>", @"", "Update^638beee0-2f8f-4706-b9a4-5bab70386697^FC5EED23-D70D-403A-BD2D-A8924B6BCBC8^The information you entered has been saved.|Complete^fdc397cd-8b4a-436e-bea1-bce2e6717c03^8BEF8579-3281-4DEE-85E2-DE912135534C^|Create Prayer Request^3c026b37-29d4-47cb-bb6e-da43afe779fe^20430220-25CB-47C2-ACCB-044037A52A4F^|Create PRT Request^3c026b37-29d4-47cb-bb6e-da43afe779fe^A2B80300-222D-44FE-8467-852FDF1A157C^|", "88C7D1CC-3478-4562-A301-AE7D4D7FFF6D", true, "", "5FB12E48-0F07-4B31-B17E-0609C93DC5E8" ); // General Contact:Open:Capture Notes


            RockMigrationHelper.UpdateWorkflowActionForm( @"", @"", "Submit^fdc397cd-8b4a-436e-bea1-bce2e6717c03^^PRT Request Will Be Created.|Cancel^5683e775-b9f3-408c-80ac-94de0e51cf3a^^|", "88C7D1CC-3478-4562-A301-AE7D4D7FFF6D", true, "CF2A1BC7-7A3C-423E-B07F-655F0B54D34F", "FD0C567D-E812-4ADC-8231-F0610F94EA9B" ); // General Contact:Create PRT Request:User Entry for PRT

            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FD0C567D-E812-4ADC-8231-F0610F94EA9B", "EDA2C10D-DB54-44EC-B34C-0D32B906506E", 12, false, true, false, false, @"", @"", "6F344DDD-A6BB-4663-B94D-A935AAA616F1" ); // General Contact:Create PRT Request:User Entry for PRT:PRT Connection Opportunity
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FD0C567D-E812-4ADC-8231-F0610F94EA9B", "C6C6A13C-8ABC-4C14-97C1-61E1D4093125", 0, true, true, false, false, @"", @"", "EDB908FA-1BFA-479E-B1EA-BB2ED69C9EE1" ); // General Contact:Create PRT Request:User Entry for PRT:Requester
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FD0C567D-E812-4ADC-8231-F0610F94EA9B", "E58F6294-52DE-4993-AD00-8332D5FCC908", 1, true, true, false, false, @"", @"", "58F44168-FFBE-4B06-B3F3-E5933213F760" ); // General Contact:Create PRT Request:User Entry for PRT:First Name
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FD0C567D-E812-4ADC-8231-F0610F94EA9B", "57475E8A-C0DD-415C-A6EA-257F01545B7D", 13, true, false, true, false, @"", @"", "5F6C75E4-FE66-4992-AFB7-A787137519C8" ); // General Contact:Create PRT Request:User Entry for PRT:PRT Connection Comment
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FD0C567D-E812-4ADC-8231-F0610F94EA9B", "CF2A1BC7-7A3C-423E-B07F-655F0B54D34F", 14, false, true, false, false, @"", @"", "841902A1-2796-4554-93B3-54DCCED429C9" ); // General Contact:Create PRT Request:User Entry for PRT:PRT Command Text
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FD0C567D-E812-4ADC-8231-F0610F94EA9B", "A105DC11-A1D1-4E10-A582-342F508F4D58", 2, true, true, false, false, @"", @"", "9116AD50-0BB2-4DC4-B514-1C22AEF2083D" ); // General Contact:Create PRT Request:User Entry for PRT:Last Name
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FD0C567D-E812-4ADC-8231-F0610F94EA9B", "648FDC57-ECAE-4C76-98FD-8C25F5EEC09E", 3, false, true, false, false, @"", @"", "88E0B433-3817-4061-A0EA-966440FE4892" ); // General Contact:Create PRT Request:User Entry for PRT:Email Address
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FD0C567D-E812-4ADC-8231-F0610F94EA9B", "98F50ADB-F37F-4E4E-A4C2-F3C19D5DCAB3", 4, false, true, false, false, @"", @"", "ACE2FC69-D10C-4592-AE1F-8B569C59840F" ); // General Contact:Create PRT Request:User Entry for PRT:Phone (optional)
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FD0C567D-E812-4ADC-8231-F0610F94EA9B", "5FC49A8A-B7F9-4443-A460-ABE2CE33B5D1", 5, false, true, false, false, @"", @"", "D81F4E88-81F7-482C-9076-3D4D1651454F" ); // General Contact:Create PRT Request:User Entry for PRT:Inquiry
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FD0C567D-E812-4ADC-8231-F0610F94EA9B", "30DB30CB-0890-4212-A5EE-E0BC4E42BAFB", 6, true, true, false, false, @"", @"", "E321E5CE-1B98-4508-82F3-8147E61D8BC4" ); // General Contact:Create PRT Request:User Entry for PRT:Campus
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FD0C567D-E812-4ADC-8231-F0610F94EA9B", "B5A5D6B4-592B-4CF0-8C20-CF488805608B", 7, false, true, false, false, @"", @"", "61D945FC-8CA5-46F5-8E26-FD0389948E29" ); // General Contact:Create PRT Request:User Entry for PRT:Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FD0C567D-E812-4ADC-8231-F0610F94EA9B", "01DD1CDA-D3E2-49FB-9D1C-E945718C5EB3", 8, false, true, false, false, @"", @"", "AB4488D1-0A02-47FD-800E-D7C941847BF4" ); // General Contact:Create PRT Request:User Entry for PRT:Assign New Worker
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FD0C567D-E812-4ADC-8231-F0610F94EA9B", "0DC37C7D-075F-4067-B276-B4CCC24E1562", 9, true, true, false, false, @"", @"", "18CA64EB-25B0-4A44-8ED1-D12F1FD0ABB9" ); // General Contact:Create PRT Request:User Entry for PRT:Notes
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FD0C567D-E812-4ADC-8231-F0610F94EA9B", "6E9BA6A0-E7BD-47D0-ADFF-3883BE6C8614", 10, false, true, false, false, @"", @"", "BB104A53-8FB6-49EF-A1C5-B1188E541D05" ); // General Contact:Create PRT Request:User Entry for PRT:Admin Group
            RockMigrationHelper.UpdateWorkflowActionFormAttribute( "FD0C567D-E812-4ADC-8231-F0610F94EA9B", "59F2F9B3-CA30-4B77-8BB3-FDF035D3E63B", 11, false, true, false, false, @"", @"", "65A196B3-44D3-4C3C-9CFB-63159D5389FA" ); // General Contact:Create PRT Request:User Entry for PRT:WorkerGuid


            RockMigrationHelper.UpdateWorkflowActionType( "A2B80300-222D-44FE-8467-852FDF1A157C", "Pre-Fill Comment", 0, "C789E457-0783-44B3-9D8F-2EBAB5F11110", true, false, "", "", 1, "", "EE37E852-E94C-4212-AEAF-35A34B1F0483" ); // General Contact:Create PRT Request:Pre-Fill Comment
            RockMigrationHelper.UpdateWorkflowActionType( "A2B80300-222D-44FE-8467-852FDF1A157C", "User Entry for PRT", 1, "486DC4FA-FCBC-425F-90B0-E606DA8A9F68", true, false, "FD0C567D-E812-4ADC-8231-F0610F94EA9B", "", 1, "", "29752057-1FB3-46CB-82EA-F41D26C15879" ); // General Contact:Create PRT Request:User Entry for PRT
            RockMigrationHelper.UpdateWorkflowActionType( "A2B80300-222D-44FE-8467-852FDF1A157C", "Create PRT Connection Request", 2, "F95C376A-714E-41FE-B27A-683F9E9078ED", true, false, "", "CF2A1BC7-7A3C-423E-B07F-655F0B54D34F", 8, "Submit", "4E8CDAF8-0B27-4C90-A599-C2D2F93A1AE4" ); // General Contact:Create PRT Request:Create PRT Connection Request
            RockMigrationHelper.UpdateWorkflowActionType( "A2B80300-222D-44FE-8467-852FDF1A157C", "Return to Open", 3, "38907A90-1634-4A93-8017-619326A4A582", true, true, "", "", 1, "", "F61166F5-0A06-45E1-AA92-BF082363B5B0" ); // General Contact:Create PRT Request:Return to Open



            RockMigrationHelper.AddActionTypeAttributeValue( "EE37E852-E94C-4212-AEAF-35A34B1F0483", "D7EAA859-F500-4521-9523-488B12EAA7D2", @"False" ); // General Contact:Create PRT Request:Pre-Fill Comment:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "EE37E852-E94C-4212-AEAF-35A34B1F0483", "44A0B977-4730-4519-8FF6-B0A01A95B212", @"57475e8a-c0dd-415c-a6ea-257f01545b7d" ); // General Contact:Create PRT Request:Pre-Fill Comment:Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "EE37E852-E94C-4212-AEAF-35A34B1F0483", "57093B41-50ED-48E5-B72B-8829E62704C8", @"" ); // General Contact:Create PRT Request:Pre-Fill Comment:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "EE37E852-E94C-4212-AEAF-35A34B1F0483", "E5272B11-A2B8-49DC-860D-8D574E2BC15C", @"5fc49a8a-b7f9-4443-a460-abe2ce33b5d1" ); // General Contact:Create PRT Request:Pre-Fill Comment:Text Value|Attribute Value
            RockMigrationHelper.AddActionTypeAttributeValue( "29752057-1FB3-46CB-82EA-F41D26C15879", "234910F2-A0DB-4D7D-BAF7-83C880EF30AE", @"False" ); // General Contact:Create PRT Request:User Entry for PRT:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "29752057-1FB3-46CB-82EA-F41D26C15879", "C178113D-7C86-4229-8424-C6D0CF4A7E23", @"" ); // General Contact:Create PRT Request:User Entry for PRT:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "4E8CDAF8-0B27-4C90-A599-C2D2F93A1AE4", "DE52C3B1-2AA0-4305-ACCD-9617C613C394", @"c6c6a13c-8abc-4c14-97c1-61e1d4093125" ); // General Contact:Create PRT Request:Create PRT Connection Request:Person Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "4E8CDAF8-0B27-4C90-A599-C2D2F93A1AE4", "9629975D-722B-4A3C-8B6E-174F7AB946DC", @"" ); // General Contact:Create PRT Request:Create PRT Connection Request:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "4E8CDAF8-0B27-4C90-A599-C2D2F93A1AE4", "E595C6D3-054C-4D69-BCAC-E3FD2B74BD8F", @"False" ); // General Contact:Create PRT Request:Create PRT Connection Request:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "4E8CDAF8-0B27-4C90-A599-C2D2F93A1AE4", "67A54E5E-9804-4105-917F-A705790C1F7C", @"eda2c10d-db54-44ec-b34c-0d32b906506e" ); // General Contact:Create PRT Request:Create PRT Connection Request:Connection Opportunity Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "4E8CDAF8-0B27-4C90-A599-C2D2F93A1AE4", "F2090B64-6454-46CE-87AB-AA6A928C5CAD", @"" ); // General Contact:Create PRT Request:Create PRT Connection Request:Connection Status Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "4E8CDAF8-0B27-4C90-A599-C2D2F93A1AE4", "751713AC-210F-4825-9D6A-F2DDC1262ABC", @"" ); // General Contact:Create PRT Request:Create PRT Connection Request:Connection Status
            RockMigrationHelper.AddActionTypeAttributeValue( "4E8CDAF8-0B27-4C90-A599-C2D2F93A1AE4", "482C00E0-EA5F-4C5F-9C5F-EC09BA27AC87", @"30db30cb-0890-4212-a5ee-e0bc4e42bafb" ); // General Contact:Create PRT Request:Create PRT Connection Request:Campus Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "4E8CDAF8-0B27-4C90-A599-C2D2F93A1AE4", "A593FB30-ECF8-46C0-8B26-E9997174B271", @"57475e8a-c0dd-415c-a6ea-257f01545b7d" ); // General Contact:Create PRT Request:Create PRT Connection Request:Connection Comment Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "4E8CDAF8-0B27-4C90-A599-C2D2F93A1AE4", "9D7D651F-C7BF-4EF6-8003-89E11FB23F70", @"" ); // General Contact:Create PRT Request:Create PRT Connection Request:Connection Request Attribute
            RockMigrationHelper.AddActionTypeAttributeValue( "F61166F5-0A06-45E1-AA92-BF082363B5B0", "E8ABD802-372C-47BE-82B1-96F50DB5169E", @"False" ); // General Contact:Create PRT Request:Return to Open:Active
            RockMigrationHelper.AddActionTypeAttributeValue( "F61166F5-0A06-45E1-AA92-BF082363B5B0", "3809A78C-B773-440C-8E3F-A8E81D0DAE08", @"" ); // General Contact:Create PRT Request:Return to Open:Order
            RockMigrationHelper.AddActionTypeAttributeValue( "F61166F5-0A06-45E1-AA92-BF082363B5B0", "02D5A7A5-8781-46B4-B9FC-AF816829D240", @"FC5EED23-D70D-403A-BD2D-A8924B6BCBC8" ); // General Contact:Create PRT Request:Return to Open:Activity

            #endregion

            #region DefinedValue AttributeType qualifier helper

            Sql( @" UPDATE [aq] SET [key] = 'definedtype', [Value] = CAST( [dt].[Id] as varchar(5) ) FROM [AttributeQualifier] [aq] INNER JOIN [Attribute] [a] ON [a].[Id] = [aq].[AttributeId] INNER JOIN [FieldType] [ft] ON [ft].[Id] = [a].[FieldTypeId] INNER JOIN [DefinedType] [dt] ON CAST([dt].[guid] AS varchar(50) ) = [aq].[value] WHERE [ft].[class] = 'Rock.Field.Types.DefinedValueFieldType' AND [aq].[key] = 'definedtypeguid' " );

            #endregion
        }
        public override void Down()
        {
           //down
        }

        
    }
}
