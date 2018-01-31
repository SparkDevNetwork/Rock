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
    public partial class UpdateBlockTypeDescriptions : Rock.Migrations.RockMigration2
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
            UPDATE [BlockType] SET [Name]='Login', [Description]='Prompts user for login credentials.', [Category]='Security' WHERE [Guid]='7B83D513-1178-429E-93FF-E76430E038E4'
            UPDATE [BlockType] SET [Name]='HTML Content', [Description]='Adds an editable HTML fragment to the page.', [Category]='CMS' WHERE [Guid]='19B61D65-37E3-459F-A44F-DEF0089118A3'
            UPDATE [BlockType] SET [Name]='Zone Blocks', [Description]='Displays the blocks for a given zone.', [Category]='Administration' WHERE [Guid]='72CAAF77-A015-45F0-A549-F941B9AB4D75'
            UPDATE [BlockType] SET [Name]='Block Properties', [Description]='Allows you to administrate a block''s properties.', [Category]='Core' WHERE [Guid]='5EC45388-83D4-4E99-BF25-3FA00327F08B'
            UPDATE [BlockType] SET [Name]='Security', [Description]='Displays security settings for a specific entity.', [Category]='Administration' WHERE [Guid]='20474B3D-0DE7-4B63-B7B9-E042DBEF788C'
            UPDATE [BlockType] SET [Name]='Pages', [Description]='Lists pages in Rock.', [Category]='Administration' WHERE [Guid]='AEFC2DBE-37B6-4CAB-882C-B214F587BF2E'
            UPDATE [BlockType] SET [Name]='Page Properties', [Description]='Displays the page properties.', [Category]='Administration' WHERE [Guid]='C7988C3E-822D-4E73-882E-9B7684398BAA'
            UPDATE [BlockType] SET [Name]='Redirect', [Description]='Redirects the page to the URL provided.', [Category]='CMS' WHERE [Guid]='B97FB779-5D3E-4663-B3B5-3C2C227AE14A'
            UPDATE [BlockType] SET [Name]='Login Status', [Description]='Displays the currently logged in user''s name along with options to Login, Logout, or manage account.', [Category]='Security' WHERE [Guid]='04712F3D-9667-4901-A49D-4507573EF7AD'
            UPDATE [BlockType] SET [Name]='Attributes', [Description]='Allows for the managing of attribues.', [Category]='Core' WHERE [Guid]='E5EA2F6D-43A2-48E0-B59C-4409B78AC830'
            UPDATE [BlockType] SET [Name]='Confirm Account', [Description]='Block for user to confirm a newly created login account, usually from an email that was sent to them.', [Category]='Security' WHERE [Guid]='734DFF21-7465-4E02-BFC3-D40F7A65FB60'
            UPDATE [BlockType] SET [Name]='New Account', [Description]='Block allows users to create a new login account.', [Category]='Security' WHERE [Guid]='99362B60-71A5-44C6-BCFE-DDA9B00CC7F3'
            UPDATE [BlockType] SET [Name]='Change Password', [Description]='Block for a user to change their password.', [Category]='Security' WHERE [Guid]='3C12DE99-2D1B-40F2-A9B8-6FE7C2524B37'
            UPDATE [BlockType] SET [Name]='Forgot Username', [Description]='Allows a user to get their forgotten username information emailed to them.', [Category]='Security' WHERE [Guid]='02B3D7D1-23CE-4154-B602-F4A15B321757'
            UPDATE [BlockType] SET [Name]='Components', [Description]='Block to administrate MEF plugins.', [Category]='Core' WHERE [Guid]='21F5F466-59BC-40B2-8D73-7314D936C3CB'
            UPDATE [BlockType] SET [Name]='Email Templates', [Description]='Allows the administration of email templates.', [Category]='Communication' WHERE [Guid]='10DC44E9-ECC1-4679-8A07-C098A0DCD82E'
            UPDATE [BlockType] SET [Name]='System Information', [Description]='Displays system information on the installed version of Rock.', [Category]='Administration' WHERE [Guid]='DE08EFD7-4CF9-4BD5-9F72-C0151FD08523'
            UPDATE [BlockType] SET [Name]='Plugin Manager', [Description]='Allows installed plugins to be viewed or removed and new ones to be added from the Rock Quarry server.', [Category]='Core' WHERE [Guid]='F80268E6-2625-4565-AA2E-790C5E40A119'
            UPDATE [BlockType] SET [Name]='Person Search', [Description]='Displays list of people that match a given search type and term.', [Category]='CRM' WHERE [Guid]='764D3E67-2D01-437A-9F45-9F8C97878434'
            UPDATE [BlockType] SET [Name]='Person Bio', [Description]='Person biographic/demographic information and picture (Person detail page).', [Category]='CRM > Person Detail' WHERE [Guid]='0F5922BB-CD68-40AC-BF3C-4AAB1B98760C'
            UPDATE [BlockType] SET [Name]='Person Contact Info', [Description]='Person contact information(Person Detail Page).', [Category]='CRM > Person Detail' WHERE [Guid]='D0D57B02-6EE7-4E4B-B80B-A8640D326572'
            UPDATE [BlockType] SET [Name]='Person Key Attributes', [Description]='Person key attributes (Person Detail Page).', [Category]='CRM > Person Detail' WHERE [Guid]='23CE11A0-6C5C-4189-8E8C-6F3C9C9E4178'
            UPDATE [BlockType] SET [Name]='Tags', [Description]='Add tags to current context object.', [Category]='Core' WHERE [Guid]='351004FF-C2D6-4169-978F-5888BEFF982F'
            UPDATE [BlockType] SET [Name]='Group List', [Description]='Lists all groups for the configured group types.', [Category]='Groups' WHERE [Guid]='3D7FB6BE-6BBD-49F7-96B4-96310AF3048A'
            UPDATE [BlockType] SET [Name]='Site Map', [Description]='Displays a site map in a tree view.', [Category]='CMS' WHERE [Guid]='2700A1B8-BD1A-40F1-A660-476DA86D0432'
            UPDATE [BlockType] SET [Name]='Entity Types', [Description]='Administer the IEntity entity types.', [Category]='Core' WHERE [Guid]='8098DF5D-4B87-4FAF-BA65-E017C5A93353'
            UPDATE [BlockType] SET [Name]='Group Detail', [Description]='Displays the details of the given group.', [Category]='Groups' WHERE [Guid]='582BEEA1-5B27-444D-BC0A-F60CEB053981'
            UPDATE [BlockType] SET [Name]='Notes', [Description]='Context aware block for adding notes to an entity.', [Category]='Core' WHERE [Guid]='2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3'
            UPDATE [BlockType] SET [Name]='Group Tree View', [Description]='Creates a navigation tree for groups of the configured group type(s).', [Category]='Groups' WHERE [Guid]='2D26A2C4-62DC-4680-8219-A52EB2BC0F65'
            UPDATE [BlockType] SET [Name]='Disc', [Description]='Allows you to take a DISC test and saves your DISC score.', [Category]='CRM > DiscAssessment' WHERE [Guid]='A161D12D-FEA7-422F-B00E-A689629680E4'
            UPDATE [BlockType] SET [Name]='Campus Detail', [Description]='Displays the details of a particular campus.', [Category]='Core' WHERE [Guid]='E30354A1-A1B8-4BE5-ADCE-43EEDDEF6C65'
            UPDATE [BlockType] SET [Name]='Campus List', [Description]='Displays a list of all campuses.', [Category]='Core' WHERE [Guid]='C93D614A-6EBC-49A1-A80D-F3677D2B86A0'
            UPDATE [BlockType] SET [Name]='Scheduled Job Detail', [Description]='Displays the details of the given scheduled job.', [Category]='Core' WHERE [Guid]='C5EC90C9-26C4-493A-84AC-4B5DEF9EA472'
            UPDATE [BlockType] SET [Name]='Scheduled Job List', [Description]='Lists all scheduled jobs.', [Category]='Core' WHERE [Guid]='6D3F924E-BDD0-4C78-981E-B698351E75AD'
            UPDATE [BlockType] SET [Name]='Site Detail', [Description]='Displays the details of a specific site.', [Category]='CMS' WHERE [Guid]='2AC06C36-869F-45F7-8C14-802781C5F70E'
            UPDATE [BlockType] SET [Name]='Site List', [Description]='Lists sites defined in the system.', [Category]='CMS' WHERE [Guid]='441D5A71-C250-4FF5-90C3-DEEAD3AC028D'
            UPDATE [BlockType] SET [Name]='Group Type Detail', [Description]='Displays the details of the given group type for editing.', [Category]='Groups' WHERE [Guid]='78B8EE69-71A7-43C1-B00B-ED13828FE104'
            UPDATE [BlockType] SET [Name]='Group Type List', [Description]='Lists all group types with filtering by purpose and system group types.', [Category]='Groups' WHERE [Guid]='80306BB1-FE4B-436F-AC7A-691CF0BC0F5E'
            UPDATE [BlockType] SET [Name]='Marketing Campaign - Ad Type Detail', [Description]='Displays the details for an ad type.', [Category]='CMS' WHERE [Guid]='9373F86E-88C8-40E8-9D70-AE6CFC5DD980'
            UPDATE [BlockType] SET [Name]='Marketing Campaign - Ad Type List', [Description]='Lists ad types in the system.', [Category]='CMS' WHERE [Guid]='7C70D1F6-78BB-4196-8A35-276DD06F8AFE'
            UPDATE [BlockType] SET [Name]='Defined Type Detail', [Description]='Displays the details of the given defined type.', [Category]='Core' WHERE [Guid]='08C35F15-9AF7-468F-9D50-CDFD3D21220C'
            UPDATE [BlockType] SET [Name]='Defined Type List', [Description]='Lists all the defined types and allows for managing them and their values.', [Category]='Core' WHERE [Guid]='5470C9C4-09C1-439F-AA56-3524047497EE'
            UPDATE [BlockType] SET [Name]='Administration', [Description]='Check-in Administration block', [Category]='Check-in' WHERE [Guid]='3B5FBE9A-2904-4220-92F3-47DD16E805C0'
            UPDATE [BlockType] SET [Name]='Welcome', [Description]='Welcome screen for check-in.', [Category]='Check-in' WHERE [Guid]='E1BBB48E-9E9A-4B69-B25C-820ABD9DCDEE'
            UPDATE [BlockType] SET [Name]='Search', [Description]='Displays keypad for searching on phone numbers.', [Category]='Check-in' WHERE [Guid]='E3A99534-6FD9-49AD-AC52-32D53B2CEDD7'
            UPDATE [BlockType] SET [Name]='Family Select', [Description]='Displays a list of families to select for checkin.', [Category]='Check-in' WHERE [Guid]='6B050E12-A232-41F6-94C5-B190F4520607'
            UPDATE [BlockType] SET [Name]='Person Select', [Description]='Lists people who match the selected family to pick to checkin.', [Category]='Check-in' WHERE [Guid]='34B48E0F-5E37-425E-9588-E612ED34DB03'
            UPDATE [BlockType] SET [Name]='Group Type Select', [Description]='Displays a list of group types the person is configured to checkin to.', [Category]='Check-in' WHERE [Guid]='7E20E97E-63F2-413D-9C2C-16FF34023F70'
            UPDATE [BlockType] SET [Name]='Location Select', [Description]='Displays a list of locations a person is able to checkin to.', [Category]='Check-in' WHERE [Guid]='FFDBBAB5-78E1-4865-8A48-EF70DDC6B3F6'
            UPDATE [BlockType] SET [Name]='Group Select', [Description]='Displays a list of groups that a person is configured to checkin to.', [Category]='Check-in' WHERE [Guid]='933418C1-448E-4825-8D3D-BDE23E968483'
            UPDATE [BlockType] SET [Name]='Time Select', [Description]='Displays a list of times to checkin for.', [Category]='Check-in' WHERE [Guid]='D2348D51-B13A-4069-97AD-369D9615A711'
            UPDATE [BlockType] SET [Name]='Success', [Description]='Displays the details of a successful checkin.', [Category]='Check-in' WHERE [Guid]='18911F1B-294E-48D6-9E6B-0F72BF6C9491'
            UPDATE [BlockType] SET [Name]='Idle Redirect', [Description]='Redirects user to a new url after a specific number of idle seconds.', [Category]='Utility' WHERE [Guid]='0DF27F26-691D-41F8-B0F7-987E4FEC375C'
            UPDATE [BlockType] SET [Name]='Marketing Campaign - Campaign Detail', [Description]='Displays the details for campaign.', [Category]='CMS' WHERE [Guid]='841A0529-31A6-4065-854F-E52C516B9237'
            UPDATE [BlockType] SET [Name]='Marketing Campaign - Campaign List', [Description]='Lists marketing campaigns.', [Category]='CMS' WHERE [Guid]='F6592890-1FD0-4BF4-AC57-F71FA900FCB5'
            UPDATE [BlockType] SET [Name]='Route Detail', [Description]='Displays the details of a page route.', [Category]='CMS' WHERE [Guid]='E6E7333A-C4A6-4DE7-9A37-CC2641320C98'
            UPDATE [BlockType] SET [Name]='Route List', [Description]='Displays a list of page routes.', [Category]='CMS' WHERE [Guid]='E92E3C51-EB14-414D-BC68-9061FEB92A22'
            UPDATE [BlockType] SET [Name]='Workflow Type Detail', [Description]='Displays the details of the given workflow type.', [Category]='Core' WHERE [Guid]='E1FF677D-5E52-4259-90C7-5560ECBBD82B'
            UPDATE [BlockType] SET [Name]='Category Detail', [Description]='Displays the details of a given category.', [Category]='Core' WHERE [Guid]='7BC54887-21C2-4688-BD1D-C1C8B9C86F7C'
            UPDATE [BlockType] SET [Name]='Ad List', [Description]='Renders a filtered list of ads for use on public sites.', [Category]='CMS' WHERE [Guid]='5A880084-7237-449A-9855-3FA02B6BD79F'
            UPDATE [BlockType] SET [Name]='Workflow List', [Description]='Lists all the workflows.', [Category]='Core' WHERE [Guid]='C86C80DF-F2FD-47F8-81CF-7C5EA4100C3B'
            UPDATE [BlockType] SET [Name]='Marketing Campaign - Ad Detail', [Description]='Displays the details for an Ad.', [Category]='CMS' WHERE [Guid]='3025D1FF-8022-4E0F-8918-515D07D50335'
            UPDATE [BlockType] SET [Name]='Marketing Campaign - Ad List', [Description]='Lists ads for a given campaign, or all ads.', [Category]='CMS' WHERE [Guid]='0A690902-A0A1-4AB1-AFEC-001BA5FD124B'
            UPDATE [BlockType] SET [Name]='Data View Detail', [Description]='Shows the details of the given data view.', [Category]='Reporting' WHERE [Guid]='EB279DF9-D817-4905-B6AC-D9883F0DA2E4'
            UPDATE [BlockType] SET [Name]='Group Member Detail', [Description]='Displays the details of the given group member for editing role, status, etc.', [Category]='Groups' WHERE [Guid]='AAE2E5C3-9279-4AB0-9682-F4D19519D678'
            UPDATE [BlockType] SET [Name]='Group Member List', [Description]='Lists all the members of the given group.', [Category]='Groups' WHERE [Guid]='88B7EFA9-7419-4D05-9F88-38B936E61EDD'
            UPDATE [BlockType] SET [Name]='Category Tree View', [Description]='Displays a tree of categories for the configured entity type.', [Category]='Core' WHERE [Guid]='ADE003C7-649B-466A-872B-B8AC952E7841'
            UPDATE [BlockType] SET [Name]='Block Type Detail', [Description]='Shows the details of a selected block type.', [Category]='Core' WHERE [Guid]='A3E648CC-0F19-455F-AF1D-B70A8205802D'
            UPDATE [BlockType] SET [Name]='Block Type List', [Description]='Lists all the block types registered in Rock.', [Category]='Core' WHERE [Guid]='78A31D91-61F6-42C3-BB7D-676EDC72F35F'
            UPDATE [BlockType] SET [Name]='Prayer Request List', [Description]='Displays a list of prayer requests for the configured top-level group category.', [Category]='Prayer > Admin' WHERE [Guid]='4D6B686A-79DF-4EFC-A8BA-9841C248BF74'
            UPDATE [BlockType] SET [Name]='Prayer Request Detail', [Description]='Displays the details of a given Prayer Request for viewing or editing.', [Category]='Prayer > Admin' WHERE [Guid]='F791046A-333F-4B2A-9815-73B60326162D'
            UPDATE [BlockType] SET [Name]='Prayer Comment List', [Description]='Displays a list of prayer comments for the configured top-level group category.', [Category]='Prayer > Admin' WHERE [Guid]='DF0F5743-2BFF-40C0-8BEE-39F1DE7B4C22'
            UPDATE [BlockType] SET [Name]='Prayer Comment Detail', [Description]='Shows a list of prayer comments and allows the noteId that is passed in (via querystring) to be editable.', [Category]='Prayer > Admin' WHERE [Guid]='4F3778DF-A25C-4E59-9242-B1D6813311E1'
            UPDATE [BlockType] SET [Name]='Dynamic Data', [Description]='Block to display dynamic report, html, xml, or transformed xml based on a SQL query or stored procedure.', [Category]='Reporting' WHERE [Guid]='E31E02E9-73F6-4B3E-98BA-E0E4F86CA126'
            UPDATE [BlockType] SET [Name]='Sql Command', [Description]='Block to execute a sql command and display the result (if any).', [Category]='Reporting' WHERE [Guid]='89EAFE90-7082-4FF2-BC87-F50BFDB53298'
            UPDATE [BlockType] SET [Name]='Report Detail', [Description]='Displays the details of the given report.', [Category]='Reporting' WHERE [Guid]='E431DBDF-5C65-45DC-ADC5-157A02045CCD'
            UPDATE [BlockType] SET [Name]='Binary File Type Detail', [Description]='Displays all details of a binary file type.', [Category]='Core' WHERE [Guid]='02D0A037-446B-403B-9719-5EF7D98239EF'
            UPDATE [BlockType] SET [Name]='Binary File Type List', [Description]='Displays a list of all binary file types.', [Category]='Core' WHERE [Guid]='0926B82C-CBA2-4943-962E-F788C8A80037'
            UPDATE [BlockType] SET [Name]='Workflow Trigger Detail', [Description]='Displays the details of the given workflow trigger.', [Category]='Core' WHERE [Guid]='5D58BF6A-3914-420C-9013-53CE8A15E390'
            UPDATE [BlockType] SET [Name]='Workflow Trigger List', [Description]='Lists all the workflow triggers.', [Category]='Core' WHERE [Guid]='72F48121-2CE2-4696-840C-CF404EAF7EEE'
            UPDATE [BlockType] SET [Name]='Device Detail', [Description]='Displays the details of the given device.', [Category]='Core' WHERE [Guid]='8CD3C212-B9EE-4258-904C-91BA3570EE11'
            UPDATE [BlockType] SET [Name]='Device List', [Description]='Lists all the devices.', [Category]='Core' WHERE [Guid]='32183AD6-01CB-4533-858B-1BDA5120AAD5'
            UPDATE [BlockType] SET [Name]='Binary File List', [Description]='Shows a list of all binary files.', [Category]='Core' WHERE [Guid]='26541C8A-9E54-4723-A739-21FAA5191014'
            UPDATE [BlockType] SET [Name]='Binary File Detail', [Description]='Shows the details of a particular binary file item.', [Category]='Core' WHERE [Guid]='B97B2E51-5C9C-459B-999F-C7797DAD8B69'
            UPDATE [BlockType] SET [Name]='Pledge List', [Description]='Generic list of all pledges in the system.', [Category]='Finance' WHERE [Guid]='7011E792-A75F-4F22-B17E-D3A58C0EDB6D'
            UPDATE [BlockType] SET [Name]='Pledge Detail', [Description]='Allows the details of a given pledge to be edited.', [Category]='Finance' WHERE [Guid]='E08504ED-B84C-4115-A058-03AAB8E8A307'
            UPDATE [BlockType] SET [Name]='Batch List', [Description]='Lists all financial batches and provides filtering by campus, status, etc.', [Category]='Finance' WHERE [Guid]='AB345CE7-5DC6-41AF-BBDC-8D23D52AFE25'
            UPDATE [BlockType] SET [Name]='Batch Detail', [Description]='Displays the details of the given financial batch.', [Category]='Finance' WHERE [Guid]='CE34CE43-2CCF-4568-9AEB-3BE203DB3470'
            UPDATE [BlockType] SET [Name]='Transaction Detail', [Description]='Displays the details of the given transaction for editing.', [Category]='Finance' WHERE [Guid]='1DE16F87-4A49-4A3C-A03E-B8488ECBEEBE'
            UPDATE [BlockType] SET [Name]='Transaction List', [Description]='Builds a list of all financial transactions which can be filtered by date, account, transaction type, etc.', [Category]='Finance' WHERE [Guid]='E04320BC-67C3-452D-9EF6-D74D8C177154'
            UPDATE [BlockType] SET [Name]='Create Pledge', [Description]='Allows a website visitor to create pledge for the configured accounts, start and end date. This block also creates a new person record if a matching person could not be found.', [Category]='Finance' WHERE [Guid]='20B5568E-A010-4E15-9127-E63CF218D6E5'
            UPDATE [BlockType] SET [Name]='Communication', [Description]='Used for creating and sending communications such as email, SMS, etc. to people.', [Category]='Communication' WHERE [Guid]='D9834641-7F39-4CFA-8CB2-E64068127565'
            UPDATE [BlockType] SET [Name]='Communication List', [Description]='Lists the status of all previously created communications.', [Category]='Communication' WHERE [Guid]='56ABBD0F-8F62-4094-88B3-161E71F21419'
            UPDATE [BlockType] SET [Name]='Exception List', [Description]='Lists all exceptions.', [Category]='Core' WHERE [Guid]='6302B319-9830-4BE3-A402-17801C88F7E4'
            UPDATE [BlockType] SET [Name]='Family Members', [Description]='Allows you to view the members of a family.', [Category]='CRM > Person Detail' WHERE [Guid]='FC137BDA-4F05-4ECE-9899-A249C90D11FC'
            UPDATE [BlockType] SET [Name]='Relationships', [Description]='Allows you to view relationships of a particular person.', [Category]='CRM > Person Detail' WHERE [Guid]='77E409D4-11CD-4009-B4CD-4B75DF2CC9FD'
            UPDATE [BlockType] SET [Name]='Badges', [Description]='Handles displaying badges for a person.', [Category]='CRM > Person Detail' WHERE [Guid]='FC8AF928-C4AF-40C7-A667-4B24390F03A1'
            UPDATE [BlockType] SET [Name]='Schedule List', [Description]='Lists all the schedules.', [Category]='Core' WHERE [Guid]='C1B934D1-2139-471E-B2B8-B22FF4499B2F'
            UPDATE [BlockType] SET [Name]='Schedule Detail', [Description]='Displays the details of the given schedule.', [Category]='Core' WHERE [Guid]='59C9C862-570C-4410-99B6-DD9064B5E594'
            UPDATE [BlockType] SET [Name]='Exception Detail', [Description]='Displays the details of the given exception.', [Category]='Core' WHERE [Guid]='B9E704E8-2097-491D-A216-8011012AA84E'
            UPDATE [BlockType] SET [Name]='Edit Person', [Description]='Allows you to edit a person.', [Category]='CRM > Person Detail' WHERE [Guid]='0A15F28C-4828-4B38-AF66-58AC5BDE48E0'
            UPDATE [BlockType] SET [Name]='Rock Control Gallery', [Description]='Allows you to see and try various Rock UI controls.', [Category]='Examples' WHERE [Guid]='55468258-18B9-4FAE-90E8-F173F7704E23'
            UPDATE [BlockType] SET [Name]='Giving Profile List', [Description]='Lists scheduled transactions for current or selected user (if context for person is not configured, will display for currently logged in person).', [Category]='Finance' WHERE [Guid]='694FF260-8C6F-4A59-93C9-CF3793FE30E6'
            UPDATE [BlockType] SET [Name]='Attribute Categories', [Description]='Allows attribute categories to be managed.', [Category]='Core' WHERE [Guid]='1FC50941-A883-47A2-ABE9-13528BCC4D1B'
            UPDATE [BlockType] SET [Name]='Add Family', [Description]='Allows for adding new families.', [Category]='CRM > Person Detail' WHERE [Guid]='DE156975-597A-4C55-A649-FE46712F91C3'
            UPDATE [BlockType] SET [Name]='Attribute Values', [Description]='Allows for editing the value(s) of a set of attributes for person.', [Category]='CRM > Person Detail' WHERE [Guid]='D70A59DC-16BE-43BE-9880-59598FA7A94C'
            UPDATE [BlockType] SET [Name]='Edit Family', [Description]='Allows you to edit a family.', [Category]='CRM > Person Detail' WHERE [Guid]='B4EB68FE-1A73-40FD-8236-78C9A015BDDE'
            UPDATE [BlockType] SET [Name]='Group Search', [Description]='Handles displaying group search results and redirects to the group detail page (via route ~/Group/) when only one match was found.', [Category]='Groups' WHERE [Guid]='F1E188A5-2F9D-4BA6-BCA1-82B2450DAC1C'
            UPDATE [BlockType] SET [Name]='Schedule Builder', [Description]='Helps to build schedules to be used for checkin.', [Category]='Check-in' WHERE [Guid]='8CDB6E8D-A8DF-4144-99F8-7F78CC1AF7E4'
            UPDATE [BlockType] SET [Name]='Current Person', [Description]='Displays the name of the currenly logged in user.', [Category]='CMS' WHERE [Guid]='F7193487-1234-49D7-9CEC-7F5F452B7E3F'
            UPDATE [BlockType] SET [Name]='Group Type List', [Description]='Lists groups types that are available for checkin.', [Category]='Check-in' WHERE [Guid]='12E586CF-DB55-4654-A13E-1F825BBA1C7C'
            UPDATE [BlockType] SET [Name]='Ability Level Select', [Description]='Check-in Ability Level Select block', [Category]='Check-in' WHERE [Guid]='605389F5-5BC5-438F-8757-110328B0CED3'
            UPDATE [BlockType] SET [Name]='Mobile Entry', [Description]='Helps to configure the checkin for mobile devices.', [Category]='Check-in' WHERE [Guid]='67E9E493-1D11-4C73-8E59-6D3C2C25CA25'
            UPDATE [BlockType] SET [Name]='Add Prayer Request', [Description]='Allows prayer requests to be added via visitors on the website.', [Category]='Prayer' WHERE [Guid]='4C32F2CD-5A88-4C3A-ADEA-CF94E85D20A6'
            UPDATE [BlockType] SET [Name]='Linq Grid', [Description]='Block to execute a linq command and display the result (if any).', [Category]='Reporting' WHERE [Guid]='584DC3C4-5B58-4467-BF58-3E49FDA05655'
            UPDATE [BlockType] SET [Name]='Ad Detail', [Description]='Displays the details of an ad for public consuption.', [Category]='CMS' WHERE [Guid]='98D27912-C4BD-4E94-AEE1-AFBF688D7264'
            UPDATE [BlockType] SET [Name]='RockUpdate', [Description]='Handles checking for and performing upgrades to the Rock system.', [Category]='Core' WHERE [Guid]='B3F7A325-24DB-4A80-ADFD-1E8E1C85217D'
            UPDATE [BlockType] SET [Name]='Tag Detail', [Description]='Block for administrating a tag.', [Category]='Core' WHERE [Guid]='B3E4584A-D3C3-4F68-9B7C-D1641B9B08CF'
            UPDATE [BlockType] SET [Name]='Tag List', [Description]='Block for viewing a list of tags.', [Category]='Core' WHERE [Guid]='C6DFE5AE-8C4C-49AD-8EC9-11CE03146F53'
            UPDATE [BlockType] SET [Name]='Tag Report', [Description]='Block for viewing entities with a selected tag', [Category]='Core' WHERE [Guid]='005E5980-E2D2-4958-ACB6-BECBC6D1F5C4'
            UPDATE [BlockType] SET [Name]='Check-in Configuration', [Description]='Helps to configure the check-in workflow.', [Category]='Check-in' WHERE [Guid]='2506B048-F62C-4945-B09A-1E053F66C592'
            UPDATE [BlockType] SET [Name]='Defined Value List', [Description]='Block for viewing values for a defined type.', [Category]='Core' WHERE [Guid]='0AB2D5E9-9272-47D5-90E4-4AA838D2D3EE'
            UPDATE [BlockType] SET [Name]='Add Transaction', [Description]='Creates a new financial transaction or scheduled transaction.', [Category]='Finance' WHERE [Guid]='74EE3481-3E5A-4971-A02E-D463ABB45591'
            UPDATE [BlockType] SET [Name]='Metric List', [Description]='Displays a list of metrics defined in the system.', [Category]='Administration' WHERE [Guid]='AE70E6C4-A34C-4D05-A13C-CE0ABE2A6B5B'
            UPDATE [BlockType] SET [Name]='Metric Detail', [Description]='Displays the details of a specific metric.', [Category]='Administration' WHERE [Guid]='CE769F0A-722F-4745-A6A8-7F00548CD1D2'
            UPDATE [BlockType] SET [Name]='Metric Value List', [Description]='Displays a list of metric values for a specific metric.', [Category]='Administration' WHERE [Guid]='819C3597-4A93-4974-B1E9-4D065989EA25'
            UPDATE [BlockType] SET [Name]='Page Menu', [Description]='Renders a page menu based on a root page and liquid template.', [Category]='CMS' WHERE [Guid]='CACB9D1A-A820-4587-986A-D66A69EE9948'
            UPDATE [BlockType] SET [Name]='Check-in Administration', [Description]='Check-In Administration block', [Category]='Check-in > Attended' WHERE [Guid]='2C51230E-BA2E-4646-BB10-817B26C16218'
            UPDATE [BlockType] SET [Name]='Family Select', [Description]='Attended Check-In Search block', [Category]='Check-in > Attended' WHERE [Guid]='645D3F2F-0901-44FE-93E9-446DBC8A1680'
            UPDATE [BlockType] SET [Name]='Family Select', [Description]='Attended Check-In Family Select Block', [Category]='Check-in > Attended' WHERE [Guid]='4D48B5F0-F0B2-4C10-8498-DAF690761A80'
            UPDATE [BlockType] SET [Name]='Confirmation Block', [Description]='Attended Check-In Confirmation Block', [Category]='Check-in > Attended' WHERE [Guid]='5B1D4187-9B34-4AB6-AC57-7E2CF67B266F'
            UPDATE [BlockType] SET [Name]='Activity Select', [Description]='Attended Check-In Activity Select Block', [Category]='Check-in > Attended' WHERE [Guid]='78E2AB4A-FDF7-4864-92F7-F052050BC4BB'
            UPDATE [BlockType] SET [Name]='Layout List', [Description]='Lists layouts for a site.', [Category]='CMS' WHERE [Guid]='5996BF81-F2E2-4702-B401-B0B1B6667DAE'
            UPDATE [BlockType] SET [Name]='Layout Detail', [Description]='Displays the details for a specific layout.', [Category]='CMS' WHERE [Guid]='68B9D63D-D714-473A-89F2-62EB1602E00A'
            UPDATE [BlockType] SET [Name]='Group Simple Register', [Description]='Prompts for name and email, creates a person record if none exists, and adds the person to a group.', [Category]='Groups' WHERE [Guid]='82A285C1-0D6B-41E0-B1AA-DD356021BDBF'
            UPDATE [BlockType] SET [Name]='Group Simple Register Confirm', [Description]='Confirmation block that will update a group member''s status to active. (Use with Group Simple Register block).', [Category]='Groups' WHERE [Guid]='B71FE9F2-0F90-497F-90FA-5A6148E8E116'
            UPDATE [BlockType] SET [Name]='Defined Type Check List', [Description]='Used for managing the values of a defined type as a checklist.', [Category]='Utility' WHERE [Guid]='15572974-DD86-43C8-BBBF-5181EE76E2C9'
            UPDATE [BlockType] SET [Name]='External File List', [Description]='Will list all of the binary files with the type of External File.  This provides a way for users to select any one of these files.', [Category]='Administration' WHERE [Guid]='850A0541-D31A-4559-94D1-9DAD5F52EFDF'
            UPDATE [BlockType] SET [Name]='Account Detail', [Description]='Displays the details of the given financial account.', [Category]='Finance' WHERE [Guid]='DCD63280-B661-48AA-8DEB-F5ED63C7AB77'
            UPDATE [BlockType] SET [Name]='Account List', [Description]='Block for viewing list of financial accounts.', [Category]='Finance' WHERE [Guid]='87A5A2E4-E6BA-4F3C-A6F2-ED046A04062E'
            UPDATE [BlockType] SET [Name]='HTML Content Approval', [Description]='Lists HTML content blocks that need approval.', [Category]='CMS' WHERE [Guid]='79E4D7D2-3F18-43A9-9A62-E02F09C6051C'
            UPDATE [BlockType] SET [Name]='Prayer Session', [Description]='Allows a user to start a session to pray for active, approved prayer requests.', [Category]='Prayer' WHERE [Guid]='FD294789-3B72-4D83-8006-FA50B5087D06'
            UPDATE [BlockType] SET [Name]='Rest Controller Detail', [Description]='Displays the details of the given rest controller.', [Category]='Core' WHERE [Guid]='20AD75DD-0DF3-49E9-9DB1-8537C12B1664'
            UPDATE [BlockType] SET [Name]='Rest Controller List', [Description]='Lists all the rest controllers.', [Category]='Core' WHERE [Guid]='7BF616C1-CE1D-4EF0-B56F-B9810B811192'
            UPDATE [BlockType] SET [Name]='Audit Information List', [Description]='An audit log of all the entities and their properties that have been added, updated, or deleted.', [Category]='Core' WHERE [Guid]='D3B7C96B-DF1F-40AF-B09F-AB468E0E726D'
            UPDATE [BlockType] SET [Name]='Categories', [Description]='Block for managing categories for a specific, configured entity type.', [Category]='Core' WHERE [Guid]='620FC4A2-6587-409F-8972-22065919D9AC'
            UPDATE [BlockType] SET [Name]='User Logins', [Description]='Block for displaying logins.  By default displays all logins, but can be configured to use person context to display logins for a specific person.', [Category]='Security' WHERE [Guid]='CE06640D-C1BA-4ACE-AF03-8D733FD3247C'
            UPDATE [BlockType] SET [Name]='Tags By Letter', [Description]='Lists tags grouped by the first letter of the name with counts for people to select.', [Category]='Core' WHERE [Guid]='784C84CF-28B0-45B5-A3ED-D7D9B2A26A5B'
            UPDATE [BlockType] SET [Name]='Giving Profile Detail', [Description]='Edit an existing scheduled transaction.', [Category]='Finance' WHERE [Guid]='5171C4E5-7698-453E-9CC8-088D362296DE'
            UPDATE [BlockType] SET [Name]='Stark', [Description]='Template block for developers to use to start a new block.', [Category]='Utility' WHERE [Guid]='EF4DE627-DD03-4EBA-990B-F2F4CC754548'
            UPDATE [BlockType] SET [Name]='Page List', [Description]='Lists pages for a site.', [Category]='CMS' WHERE [Guid]='D9847FE8-5279-4CC4-BD69-8A71B2F1ED70'
            UPDATE [BlockType] SET [Name]='CkEditor FileBrowser', [Description]='Block to be used as part of the RockFileBrowser CKEditor Plugin', [Category]='Utility' WHERE [Guid]='17A1687B-A2C7-4160-BF2B-2424DF69E9D5'
            UPDATE [BlockType] SET [Name]='CkEditor MergeField', [Description]='Block to be used as part of the RockMergeField CKEditor Plugin', [Category]='Utility' WHERE [Guid]='90187FFA-5474-40E0-BA0C-9C7E631CC46C'
            UPDATE [BlockType] SET [Name]='Person History', [Description]='Block for displaying the history of changes to a particular user.', [Category]='CRM > Person Detail' WHERE [Guid]='854C7AE2-6FA4-4D1A-BBB5-012484EA436E'
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
