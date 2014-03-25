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
    public partial class CommunicationStatus : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.CommunicationRecipientActivity",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CommunicationRecipientId = c.Int(nullable: false),
                        ActivityDateTime = c.DateTime(nullable: false),
                        ActivityType = c.String(maxLength: 20),
                        ActivityDetail = c.String(maxLength: 200),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CommunicationRecipient", t => t.CommunicationRecipientId, cascadeDelete: true)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CommunicationRecipientId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId);
            
            AddColumn("dbo.Person", "EmailPreference", c => c.Int(nullable: false));
            AddColumn("dbo.PhoneNumber", "CountryCode", c => c.String(maxLength: 3));
            AddColumn("dbo.CommunicationRecipient", "OpenedDateTime", c => c.DateTime());
            AddColumn("dbo.CommunicationRecipient", "OpenedClient", c => c.String(maxLength: 200));

            AddDefinedType( "Communication", "Phone Country Code", "Defines phone number country codes and how they should be formatted.", "45E9EF7C-91C7-45AB-92C1-1D6219293847" );
            AddDefinedValue( "45E9EF7C-91C7-45AB-92C1-1D6219293847", "1", "North American Phone Number without Area Code", "A02DFE36-1A47-47C5-8F08-7BCA0C3FBB61" );
            AddDefinedValue( "45E9EF7C-91C7-45AB-92C1-1D6219293847", "1", "North American Phone Number with Area Code", "2131F4AF-8E17-4FD9-9610-A3C02100EF6A" );
            AddDefinedValue( "45E9EF7C-91C7-45AB-92C1-1D6219293847", "1", "North American Phone Number with Country Code and Area Code", "B3F916CA-2833-4ECD-82AC-171473972C20" );

            Sql( @"
    DECLARE @DefinedTypeId int = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '45E9EF7C-91C7-45AB-92C1-1D6219293847')
    DECLARE @EntityTypeId int = (SELECT [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.DefinedValue')
    DECLARE @FieldTypeId int = (SELECT [Id] FROM [FieldType] WHERE [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA')
    DECLARE @MatchAttributeId int
    DECLARE @FormatAttributeId int

    -- Match RegEx Attribute
    INSERT INTO [Attribute] (
        [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
        [Key],[Name],[Description],
        [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[Guid])
    VALUES(
        1,@FieldTypeId,@EntityTypeid,'DefinedTypeId',CAST(@DefinedTypeId AS varchar),
        'MatchRegEx','Match Expression','The Regular Expression used to match a phone number (non-numeric characters are removed first)',
        0,1,'',0,0,'756CB747-9143-4A7D-9787-FEECF535C939')
    SET @MatchAttributeID = @@IDENTITY

    -- Replace RegEx Attribute
    INSERT INTO [Attribute] (
        [IsSystem],[FieldTypeId],[EntityTypeId],[EntityTypeQualifierColumn],[EntityTypeQualifierValue],
        [Key],[Name],[Description],
        [Order],[IsGridColumn],[DefaultValue],[IsMultiValue],[IsRequired],[Guid])
    VALUES(
        1,@FieldTypeId,@EntityTypeid,'DefinedTypeId',CAST(@DefinedTypeId AS varchar),
        'FormatRegEx','Format Expression','The Regular Expression used to format the values found by match',
        1,1,'',0,0,'26E44877-8297-4DBC-9E5C-BCFE38291A4B')
    SET @FormatAttributeID = @@IDENTITY

    -- Attribute values for North American Phone Number without Area Code 
    DECLARE @EntityId int = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'A02DFE36-1A47-47C5-8F08-7BCA0C3FBB61')
    INSERT INTO [AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
    VALUES( 0, @MatchAttributeID, @EntityId, 0, '^(\d{3})(\d{4})$','6AA75342-1A49-429F-8663-27C128B2EB17')
    INSERT INTO [AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
    VALUES( 0, @FormatAttributeID, @EntityId, 0, '$1-$2','0FB80ED6-978B-4E1A-92BE-7DD4851CC953')

    -- Attribute values for North American Phone Number with Area Code 
    SET @EntityId = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '2131F4AF-8E17-4FD9-9610-A3C02100EF6A ')
    INSERT INTO [AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
    VALUES( 0, @MatchAttributeID, @EntityId, 0, '^(\d{3})(\d{3})(\d{4})$','26C0B28D-51BF-495C-A7AE-65E0AC3C6220')
    INSERT INTO [AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
    VALUES( 0, @FormatAttributeID, @EntityId, 0, '($1) $2-$3','B00816B6-C0E1-4084-BED5-CA14A8CE7436')

    -- Attribute values for North American Phone Number with Country Code and Area Code
    SET @EntityId = (SELECT [Id] FROM [DefinedValue] WHERE [Guid] = 'B3F916CA-2833-4ECD-82AC-171473972C20 ')
    INSERT INTO [AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
    VALUES( 0, @MatchAttributeID, @EntityId, 0, '^1(\d{3})(\d{3})(\d{4})$','63A56DE6-A7F6-47C4-9DD7-5CDFAAFCF526')
    INSERT INTO [AttributeValue] ([IsSystem],[AttributeId],[EntityId],[Order],[Value],[Guid])
    VALUES( 0, @FormatAttributeID, @EntityId, 0, '($1) $2-$3','DDB0CCA9-4ABF-4F66-B415-2DBDBD8C5D17')

    -- Add label before history block
    UPDATE [Block] SET [PreHtml] = '<h4>Change Log</h4>' WHERE [Guid] = 'F98649D7-E522-46CB-8F67-01DB7F59E3AA'

    -- Update communication template page
    UPDATE [Page] SET [IconCssClass] = 'fa fa-comment' WHERE [Guid] = 'EA611245-7A5E-4995-A3C6-EB97C6FD7C8D'

    -- Change title of Communication List to Communication History
    UPDATE [Page] SET
        [InternalName] = 'Communication History',
        [PageTitle] = 'Communication History',
        [BrowserTitle] = 'Communication History'
    WHERE [Guid] = 'CADB44F2-2453-4DB5-AB11-DADA5162AB79'

    -- Update pages that use GroupDetail to to not show breadcrumbs
    UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] IN ('4E237286-B715-4109-A578-C1445EC02707','48AAD428-A9C9-4BBB-A80F-B85F28D31240')
" );
            // Delete the GroupDetail block that is not used
            DeleteBlock("D025306E-6820-42A6-8BF6-8606582D3DF5");
            DeletePage( "54F6365A-4E4C-4E2A-9498-70B09E062C69" );

            // Delete Contact Info block (not used)
            DeleteBlockType( "D0D57B02-6EE7-4E4B-B80B-A8640D326572" );

            // Update Admin checklist for new address location service
            DeleteDefinedValue( "B1CE8ECA-6584-45DC-A022-40490FC753E8" );
            DeleteDefinedValue( "B3FAB89D-4EAD-4ED8-AD7F-06E150E4CD6B" );
            AddDefinedValue( "4BF34677-37E9-4E71-BD03-252B66C9373D", "Setup Location Service", @"
<p>Your attendees’ addresses are very valuable, so it’s important that they are formatted correctly and validated 
through the USPS database. Also, in order for these addresses to be used with the latest mapping technologies it’s 
important that they also be converted into latitude and longitude points through a process called geocoding. Fortunately, 
Rock makes both of these tasks simple (and free...)</p>

<p>You can set up these services under <span class=""navigation-tip"">Admin Tools > System Settings > Location Services</span>.
See the <a href=""http://www.rockrms.com/Rock/BookContent/9#addressstandardizationandgeocoding"">Admin Hero Guide</a> for 
additional details.</p>
", "ED6271B4-D5AF-413E-9207-1B6F5F91D6F5", false );
            Sql( @"
UPDATE [DefinedValue] SET [Order] = 0 WHERE [Guid] = 'ABC4457F-3C2A-4856-9FA8-D4B303F23B93' --Update Your Install
UPDATE [DefinedValue] SET [Order] = 1 WHERE [Guid] = '4C94C85B-2180-48CD-A1B6-62327B407245' --Create Your Account
UPDATE [DefinedValue] SET [Order] = 2 WHERE [Guid] = 'ED6271B4-D5AF-413E-9207-1B6F5F91D6F5' --Setup Location Service
UPDATE [DefinedValue] SET [Order] = 3 WHERE [Guid] = '9CB1677A-2016-41B0-B3FE-D2979C52FC7E' --Add Organization Address
UPDATE [DefinedValue] SET [Order] = 4 WHERE [Guid] = '92FA16FA-39E3-4364-9412-AA322C9EF01A' --Input Google Maps Key
UPDATE [DefinedValue] SET [Order] = 5 WHERE [Guid] = '0B922507-8CCB-48C3-A41E-0E790D6B5702' --Update Welcome Message
UPDATE [DefinedValue] SET [Order] = 6 WHERE [Guid] = '5808DF4C-4F5D-4488-9C54-B05DE20B5FCF' --Define Person Attributes
UPDATE [DefinedValue] SET [Order] = 7 WHERE [Guid] = 'B0E46522-921F-47AA-B548-F0072F22B903' --Setup Group Types
" );

            UpdateFieldType( "Site", "", "Rock", "Rock.Field.Types.SiteFieldType", "BB7AB90F-4DE9-4804-A852-F5593A35C8A0" );

            AddPage( "B0F4B33D-DD11-4CCC-B79D-9342831B8701", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Group Mapper", "", "EF540401-0DF0-4C87-A1E1-319E1EC7315B", "" ); 
            AddPage( "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Communication Templates", "", "39F75137-90D2-4E6F-8613-F19344767594", "fa fa-comment" ); 
            AddPage( "39F75137-90D2-4E6F-8613-F19344767594", "195BCD57-1C10-4969-886F-7324B6287B75", "Template Detail", "", "924036BE-6B89-4C60-96ED-0A9AF1201CC4", "" ); 

            UpdateBlockType( "Active Users", "Displays a list of active users of a website.", "~/Blocks/Cms/ActiveUsers.ascx", "CMS", "3E7033EE-31A3-4484-AFA9-240C856A500C" );
            UpdateBlockType( "Group Mapper", "Displays groups on a map.", "~/Blocks/Groups/GroupMapper.ascx", "Groups", "215A49D0-A94F-40F0-9374-3D601CF97769" );
            
            // Add Block to Page: Internal Homepage, Site: Rock RMS
            AddBlock( "20F97A93-7949-4C2A-8A5E-C756FE8585CA", "", "3E7033EE-31A3-4484-AFA9-240C856A500C", "Active Users", "Sidebar1", "", "", 1, "CB8F9152-08BB-4576-B7A1-B0DDD9880C44" );
            // Add Block to Page: Internal Homepage, Site: Rock RMS
            AddBlock( "20F97A93-7949-4C2A-8A5E-C756FE8585CA", "", "3E7033EE-31A3-4484-AFA9-240C856A500C", "Active Users", "Sidebar1", "", "", 2, "03FCBF5A-42E0-4F45-B670-BC8E324BD573" );
            // Add Block to Page: Group Mapper, Site: Rock RMS
            AddBlock( "EF540401-0DF0-4C87-A1E1-319E1EC7315B", "", "215A49D0-A94F-40F0-9374-3D601CF97769", "Group Mapper", "Main", "", "", 0, "7238AB8D-C32B-4A67-AE73-D92C7CF05EE3" );
            // Add Block to Page: Communication Templates, Site: Rock RMS
            AddBlock( "39F75137-90D2-4E6F-8613-F19344767594", "", "EACDBBD4-C355-4D38-B604-779BC55D3876", "Template List", "Main", "", "", 0, "69185EDF-C743-4013-894F-8F8BE53A149E" );
            // Add Block to Page: Template Detail, Site: Rock RMS
            AddBlock( "924036BE-6B89-4C60-96ED-0A9AF1201CC4", "", "BFDCA2E2-DAA1-4FA6-B33C-C53C7CF23C5D", "Template Detail", "Main", "", "", 0, "5CF2A07E-BD73-42A9-A024-46E69C8E3550" );

            // Attrib for BlockType: Active Users:Site
            AddBlockTypeAttribute( "3E7033EE-31A3-4484-AFA9-240C856A500C", "BB7AB90F-4DE9-4804-A852-F5593A35C8A0", "Site", "Site", "", "Site to show current active users for.", 0, @"", "73E6A9D1-3D3C-40D0-B685-0123D13A345E" );
            // Attrib for BlockType: Active Users:Person Profile Page
            AddBlockTypeAttribute( "3E7033EE-31A3-4484-AFA9-240C856A500C", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "", "Page reference to the person profil page you would like to use as a link. Not providing a reference will suppress the creation of a link.", 0, @"", "0537EBF7-2B62-4482-BA75-1091DF94D3F9" );
            // Attrib for BlockType: Active Users:Page View Count
            AddBlockTypeAttribute( "3E7033EE-31A3-4484-AFA9-240C856A500C", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Page View Count", "PageViewCount", "", "The number of past page views to show on roll-over. A value of 0 will disable the roll-over.", 0, @"5", "941D0BF8-E2D3-4D25-9AA1-4E5A03AAAA64" );
            // Attrib for BlockType: Active Users:Show Site Name As Title
            AddBlockTypeAttribute( "3E7033EE-31A3-4484-AFA9-240C856A500C", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Site Name As Title", "ShowSiteNameAsTitle", "", "Detmine whether to show the name of the site as a title above the list.", 0, @"True", "8D05182B-6B2C-42C5-BC02-29623EF49AFD" );

            // Attrib for BlockType: Group Mapper:Info Window Contents
            AddBlockTypeAttribute( "215A49D0-A94F-40F0-9374-3D601CF97769", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Info Window Contents", "InfoWindowContents", "", "Liquid template for the info window. To suppress the window provide a blank template.", 8, @"<div class='clearfix'>
    <h4 class='pull-left' style='margin-top: 0;'>{{GroupName}}</h4> 
    <span class='label label-campus pull-right'>{{GroupCampus}}</span>
</div>

<div class='clearfix'>
    <div class='pull-left' style='padding-right: 24px'>
        <strong>{{GroupLocation.Name}}</strong><br>
        {{GroupLocation.Street1}}
        <br>{{GroupLocation.City}}, {{GroupLocation.State}} {{GroupLocation.Zip}}
        {% for attribute in Attributes %}
            {% if forloop.first %}<br/>{% endif %}
            <br/><strong>{{attribute.Name}}:</strong> {{ attribute.Value }}
        {% endfor %}
    </div>
    <div class='pull-left'>
        <strong>{{GroupMemberTerm}}s</strong><br>
        {% for GroupMember in GroupMembers -%}
            {% if PersonProfilePage != '' %}
                <a href='{{PersonProfilePage}}{{GroupMember.Id}}'>{{GroupMember.NickName}} {{GroupMember.LastName}}</a>
            {% else %}
                {{GroupMember.NickName}} {{GroupMember.LastName}}
            {% endif %}
            - {{GroupMember.Email}}
            <br>
        {% endfor -%}
    </div>
</div>

{% if GroupDetailPage != '' %}
    <br>
    <a class='btn btn-xs btn-action' href='{{GroupDetailPage}}'>View Group</a>
{% endif %}

", "9FDF7A85-C56E-4DDA-BB6C-C583C8DD1883" );
            // Attrib for BlockType: Group Mapper:Person Profile Page
            AddBlockTypeAttribute( "215A49D0-A94F-40F0-9374-3D601CF97769", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Person Profile Page", "PersonProfilePage", "", "Page to use as a link to the person profile page (optional).", 4, @"", "395CF4A2-6324-4B02-99C4-0E90EE8C3B40" );
            // Attrib for BlockType: Group Mapper:Show Map Info Window
            AddBlockTypeAttribute( "215A49D0-A94F-40F0-9374-3D601CF97769", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Show Map Info Window", "ShowMapInfoWindow", "", "Control whether a info window should be displayed when clicking on a map point.", 5, @"True", "1C0DC06B-1515-4A56-9C8B-1439E39E763E" );
            // Attrib for BlockType: Group Mapper:Group Type
            AddBlockTypeAttribute( "215A49D0-A94F-40F0-9374-3D601CF97769", "18E29E23-B43B-4CF7-AE41-C85672C09F50", "Group Type", "GroupType", "", "The type of group to map.", 0, @"", "D618AF42-DFEC-4F76-BF44-548EFE1C026A" );
            // Attrib for BlockType: Group Mapper:Enable Debug
            AddBlockTypeAttribute( "215A49D0-A94F-40F0-9374-3D601CF97769", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Debug", "EnableDebug", "", "Enabling debug will display the fields of the first 5 groups to help show you wants available for your liquid.", 9, @"False", "CA10959A-D097-48EE-95F9-F7556975548C" );
            // Attrib for BlockType: Group Mapper:Location Type
            AddBlockTypeAttribute( "215A49D0-A94F-40F0-9374-3D601CF97769", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Location Type", "LocationType", "", "The location type to use for the map.", 1, @"", "B526F882-28A2-4BCE-A05D-2D6B930134CE" );
            // Attrib for BlockType: Group Mapper:Map Height
            AddBlockTypeAttribute( "215A49D0-A94F-40F0-9374-3D601CF97769", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Map Height", "MapHeight", "", "Height of the map in pixels (default value is 600px)", 2, @"600", "55BAF4FC-F54E-4735-AFFD-3550D4F6D8E5" );
            // Attrib for BlockType: Group Mapper:Group Detail Page
            AddBlockTypeAttribute( "215A49D0-A94F-40F0-9374-3D601CF97769", "BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108", "Group Detail Page", "GroupDetailPage", "", "Page to use as a link to the group details (optional).", 3, @"", "B86B3E0C-6611-4880-A8A2-EA6182385267" );
            // Attrib for BlockType: Group Mapper:Attributes
            AddBlockTypeAttribute( "215A49D0-A94F-40F0-9374-3D601CF97769", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Attributes", "Attributes", "", "Comma delimited list of attribute keys to include values for in the map info window (e.g. 'StudyTopic,MeetingTime').", 6, @"", "E7FA1140-8790-476A-B88D-841631FBECA7" );
            // Attrib for BlockType: Group Mapper:MapStyle
            AddBlockTypeAttribute( "215A49D0-A94F-40F0-9374-3D601CF97769", "59D5A94C-94A0-4630-B80A-BB25697D74C7", "Map Style", "MapStyle", "", "The map theme that should be used for styling the map.", 7, @"BFC46259-FB66-4427-BF05-2B030A582BEA", "BBA98E02-F475-4A03-A182-4D4DC51D50C0" );

            // Attrib Value for Block:Template List, Attribute:Detail Page Page: Communication Templates, Site: Rock RMS
            AddBlockAttributeValue( "69185EDF-C743-4013-894F-8F8BE53A149E", "08C596A3-CDC5-42B8-9D2B-382254DDBCE5", @"924036be-6b89-4c60-96ed-0a9af1201cc4" );

            // Attrib Value for Block:Active Users, Attribute:Site Page: Internal Homepage, Site: Rock RMS
            AddBlockAttributeValue( "CB8F9152-08BB-4576-B7A1-B0DDD9880C44", "73E6A9D1-3D3C-40D0-B685-0123D13A345E", @"1" );
            // Attrib Value for Block:Active Users, Attribute:Page View Count Page: Internal Homepage, Site: Rock RMS
            AddBlockAttributeValue( "CB8F9152-08BB-4576-B7A1-B0DDD9880C44", "941D0BF8-E2D3-4D25-9AA1-4E5A03AAAA64", @"5" );
            // Attrib Value for Block:Active Users, Attribute:Person Profile Page Page: Internal Homepage, Site: Rock RMS
            AddBlockAttributeValue( "CB8F9152-08BB-4576-B7A1-B0DDD9880C44", "0537EBF7-2B62-4482-BA75-1091DF94D3F9", @"08dbd8a5-2c35-4146-b4a8-0f7652348b25,7e97823a-78a8-4e8e-a337-7a20f2da9e52" );
            // Attrib Value for Block:Active Users, Attribute:Show Site Name As Title Page: Internal Homepage, Site: Rock RMS
            AddBlockAttributeValue( "CB8F9152-08BB-4576-B7A1-B0DDD9880C44", "8D05182B-6B2C-42C5-BC02-29623EF49AFD", @"True" );
            // Attrib Value for Block:Active Users, Attribute:Site Page: Internal Homepage, Site: Rock RMS
            AddBlockAttributeValue( "03FCBF5A-42E0-4F45-B670-BC8E324BD573", "73E6A9D1-3D3C-40D0-B685-0123D13A345E", @"3" );
            // Attrib Value for Block:Active Users, Attribute:Page View Count Page: Internal Homepage, Site: Rock RMS
            AddBlockAttributeValue( "03FCBF5A-42E0-4F45-B670-BC8E324BD573", "941D0BF8-E2D3-4D25-9AA1-4E5A03AAAA64", @"5" );
            // Attrib Value for Block:Active Users, Attribute:Person Profile Page Page: Internal Homepage, Site: Rock RMS
            AddBlockAttributeValue( "03FCBF5A-42E0-4F45-B670-BC8E324BD573", "0537EBF7-2B62-4482-BA75-1091DF94D3F9", @"08dbd8a5-2c35-4146-b4a8-0f7652348b25,7e97823a-78a8-4e8e-a337-7a20f2da9e52" );
            // Attrib Value for Block:Active Users, Attribute:Show Site Name As Title Page: Internal Homepage, Site: Rock RMS
            AddBlockAttributeValue( "03FCBF5A-42E0-4F45-B670-BC8E324BD573", "8D05182B-6B2C-42C5-BC02-29623EF49AFD", @"True" );

            // Attrib Value for Block:Group Mapper, Attribute:Group Type Page: Group Mapper, Site: Rock RMS
            AddBlockAttributeValue( "7238AB8D-C32B-4A67-AE73-D92C7CF05EE3", "D618AF42-DFEC-4F76-BF44-548EFE1C026A", @"50fcfb30-f51a-49df-86f4-2b176ea1820b" );
            // Attrib Value for Block:Group Mapper, Attribute:Location Type Page: Group Mapper, Site: Rock RMS
            AddBlockAttributeValue( "7238AB8D-C32B-4A67-AE73-D92C7CF05EE3", "B526F882-28A2-4BCE-A05D-2D6B930134CE", @"96d540f5-071d-4bbd-9906-28f0a64d39c4" );
            // Attrib Value for Block:Group Mapper, Attribute:Map Height Page: Group Mapper, Site: Rock RMS
            AddBlockAttributeValue( "7238AB8D-C32B-4A67-AE73-D92C7CF05EE3", "55BAF4FC-F54E-4735-AFFD-3550D4F6D8E5", @"600" );
            // Attrib Value for Block:Group Mapper, Attribute:Group Detail Page Page: Group Mapper, Site: Rock RMS
            AddBlockAttributeValue( "7238AB8D-C32B-4A67-AE73-D92C7CF05EE3", "B86B3E0C-6611-4880-A8A2-EA6182385267", @"4e237286-b715-4109-a578-c1445ec02707,2bc75af5-44ad-4ba3-90d3-15d936f722e8" );
            // Attrib Value for Block:Group Mapper, Attribute:Person Profile Page Page: Group Mapper, Site: Rock RMS
            AddBlockAttributeValue( "7238AB8D-C32B-4A67-AE73-D92C7CF05EE3", "395CF4A2-6324-4B02-99C4-0E90EE8C3B40", @"08dbd8a5-2c35-4146-b4a8-0f7652348b25,7e97823a-78a8-4e8e-a337-7a20f2da9e52" );
            // Attrib Value for Block:Group Mapper, Attribute:Show Map Info Window Page: Group Mapper, Site: Rock RMS
            AddBlockAttributeValue( "7238AB8D-C32B-4A67-AE73-D92C7CF05EE3", "1C0DC06B-1515-4A56-9C8B-1439E39E763E", @"True" );
            // Attrib Value for Block:Group Mapper, Attribute:Info Window Contents Page: Group Mapper, Site: Rock RMS
            AddBlockAttributeValue( "7238AB8D-C32B-4A67-AE73-D92C7CF05EE3", "9FDF7A85-C56E-4DDA-BB6C-C583C8DD1883", @"<div class='clearfix'>
    <h4 class='pull-left' style='margin-top: 0;'>{{GroupName}}</h4> 
    <span class='label label-campus pull-right'>{{GroupCampus}}</span>
</div>

<div class='clearfix'>
    <div class='pull-left' style='padding-right: 24px'>
        <strong>{{GroupLocation.Name}}</strong><br>
        {{GroupLocation.Street1}}
        <br>{{GroupLocation.City}}, {{GroupLocation.State}} {{GroupLocation.Zip}}
        {% for attribute in Attributes %}
            {% if forloop.first %}<br/>{% endif %}
            <br/><strong>{{attribute.Name}}:</strong> {{ attribute.Value }}
        {% endfor %}
    </div>
    <div class='pull-left'>
        <strong>{{GroupMemberTerm}}s</strong><br>
        {% for GroupMember in GroupMembers -%}
            {% if PersonProfilePage != '' %}
                <a href='{{PersonProfilePage}}{{GroupMember.Id}}'>{{GroupMember.NickName}} {{GroupMember.LastName}}</a>
            {% else %}
                {{GroupMember.NickName}} {{GroupMember.LastName}}
            {% endif %}
            - {{GroupMember.Email}}
            <br>
        {% endfor -%}
    </div>
</div>

{% if GroupDetailPage != '' %}
    <br>
    <a class='btn btn-xs btn-action' href='{{GroupDetailPage}}'>View Group</a>
{% endif %}
" );
            // Attrib Value for Block:Group Mapper, Attribute:Enable Debug Page: Group Mapper, Site: Rock RMS
            AddBlockAttributeValue( "7238AB8D-C32B-4A67-AE73-D92C7CF05EE3", "CA10959A-D097-48EE-95F9-F7556975548C", @"False" );
            // Attrib Value for Block:Group Mapper, Attribute:Attributes Page: Group Mapper, Site: Rock RMS
            AddBlockAttributeValue( "7238AB8D-C32B-4A67-AE73-D92C7CF05EE3", "E7FA1140-8790-476A-B88D-841631FBECA7", @"StudyTopic,MeetingTime" );
            // Attrib Value for Block:Group Mapper, Attribute:MapStyle Page: Group Mapper, Site: Rock RMS
            AddBlockAttributeValue( "7238AB8D-C32B-4A67-AE73-D92C7CF05EE3", "BBA98E02-F475-4A03-A182-4D4DC51D50C0", @"BFC46259-FB66-4427-BF05-2B030A582BEA" );

            Sql( @"
    -- Communication settings page order
    UPDATE [Page] SET [Order] = 0 WHERE [Guid] = 'E6F5F06B-65EE-4949-AA56-1FE4E2933C63'
    UPDATE [Page] SET [Order] = 1 WHERE [Guid] = '39F75137-90D2-4E6F-8613-F19344767594'
    UPDATE [Page] SET [Order] = 2 WHERE [Guid] = '89B7A631-EA6F-4DA3-9380-04EE67B63E9E'
    UPDATE [Page] SET [Order] = 3 WHERE [Guid] = '6FF35C53-F89F-4601-8543-2E2328C623F8'
    UPDATE [Page] SET [Order] = 4 WHERE [Guid] = '29CC8A0B-6476-4200-8B93-DC9BA8767D59'

    -- Communication Template Detail
    UPDATE [Page] SET [BreadCrumbDisplayName] = 0 WHERE [Guid] = '924036BE-6B89-4C60-96ED-0A9AF1201CC4'
" );
            // Set security for Contributions and Security pages on person detail
            DeleteSecurityAuthForPage( "0E56F56E-FB32-4827-A69A-B90D43CB47F5" );
            DeleteSecurityAuthForPage( "53CF4CBE-85F9-4A50-87D7-0D72A3FB2892" );
            AddSecurityAuthForPage( "53CF4CBE-85F9-4A50-87D7-0D72A3FB2892", 0, "View", "628C51A8-4613-43ED-A18D-4A6FB999273E", Model.SpecialRole.None, "752EB915-9575-4ECA-8608-B025AC0D34E7" );
            AddSecurityAuthForPage( "53CF4CBE-85F9-4A50-87D7-0D72A3FB2892", 1, "View", "6246A7EF-B7A3-4C8C-B1E4-3FF114B84559", Model.SpecialRole.None, "7055E0BE-05DB-4A33-B139-29C62B374FE9" );
            AddSecurityAuthForPage( "53CF4CBE-85F9-4A50-87D7-0D72A3FB2892", 2, "View", "2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9", Model.SpecialRole.None, "6C3EACE3-E806-4BD1-8950-C044CB787E6C" );
            AddSecurityAuthForPage( "53CF4CBE-85F9-4A50-87D7-0D72A3FB2892", 3, "View", null, Model.SpecialRole.AllUsers, "86B329FF-A451-4ABF-A2B0-199162EA0954" );
            AddSecurityAuthForPage( "0E56F56E-FB32-4827-A69A-B90D43CB47F5", 0, "View", "628C51A8-4613-43ED-A18D-4A6FB999273E", Model.SpecialRole.None, "ACA8C084-1896-4256-BEB0-5DE54B1551B6" );
            AddSecurityAuthForPage( "0E56F56E-FB32-4827-A69A-B90D43CB47F5", 1, "View", null, Model.SpecialRole.AllUsers, "D97F79B7-400E-4533-B6D2-5BCDE376D368" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Attrib for BlockType: Group Mapper:Attributes
            DeleteAttribute( "E7FA1140-8790-476A-B88D-841631FBECA7" );
            // Attrib for BlockType: Group Mapper:Group Detail Page
            DeleteAttribute( "B86B3E0C-6611-4880-A8A2-EA6182385267" );
            // Attrib for BlockType: Group Mapper:Map Height
            DeleteAttribute( "55BAF4FC-F54E-4735-AFFD-3550D4F6D8E5" );
            // Attrib for BlockType: Group Mapper:Location Type
            DeleteAttribute( "B526F882-28A2-4BCE-A05D-2D6B930134CE" );
            // Attrib for BlockType: Group Mapper:Enable Debug
            DeleteAttribute( "CA10959A-D097-48EE-95F9-F7556975548C" );
            // Attrib for BlockType: Group Mapper:Group Type
            DeleteAttribute( "D618AF42-DFEC-4F76-BF44-548EFE1C026A" );
            // Attrib for BlockType: Group Mapper:Show Map Info Window
            DeleteAttribute( "1C0DC06B-1515-4A56-9C8B-1439E39E763E" );
            // Attrib for BlockType: Group Mapper:Person Profile Page
            DeleteAttribute( "395CF4A2-6324-4B02-99C4-0E90EE8C3B40" );
            // Attrib for BlockType: Group Mapper:Info Window Contents
            DeleteAttribute( "9FDF7A85-C56E-4DDA-BB6C-C583C8DD1883" );

            // Attrib for BlockType: Active Users:Show Site Name As Title
            DeleteAttribute( "8D05182B-6B2C-42C5-BC02-29623EF49AFD" );
            // Attrib for BlockType: Active Users:Page View Count
            DeleteAttribute( "941D0BF8-E2D3-4D25-9AA1-4E5A03AAAA64" );
            // Attrib for BlockType: Active Users:Person Profile Page
            DeleteAttribute( "0537EBF7-2B62-4482-BA75-1091DF94D3F9" );
            // Attrib for BlockType: Active Users:Site
            DeleteAttribute( "73E6A9D1-3D3C-40D0-B685-0123D13A345E" );

            // Remove Block: Communication Templates, Site: Rock RMS
            DeleteBlock( "69185EDF-C743-4013-894F-8F8BE53A149E" );
            // Remove Block: Template Detail, Site: Rock RMS
            DeleteBlock( "5CF2A07E-BD73-42A9-A024-46E69C8E3550" );
            // Remove Block: Group Mapper, from Page: Group Mapper, Site: Rock RMS
            DeleteBlock( "7238AB8D-C32B-4A67-AE73-D92C7CF05EE3" );
            // Remove Block: Active Users, from Page: Internal Homepage, Site: Rock RMS
            DeleteBlock( "03FCBF5A-42E0-4F45-B670-BC8E324BD573" );
            // Remove Block: Active Users, from Page: Internal Homepage, Site: Rock RMS
            DeleteBlock( "CB8F9152-08BB-4576-B7A1-B0DDD9880C44" );

            DeleteBlockType( "215A49D0-A94F-40F0-9374-3D601CF97769" ); // Group Mapper
            DeleteBlockType( "3E7033EE-31A3-4484-AFA9-240C856A500C" ); // Active Users

            DeletePage( "924036BE-6B89-4C60-96ED-0A9AF1201CC4" ); // Page: Communication Templates Detail: Full Width, Site: Rock RMS
            DeletePage( "39F75137-90D2-4E6F-8613-F19344767594" ); // Page: Communication Templates Layout: Full Width, Site: Rock RMS
            DeletePage( "EF540401-0DF0-4C87-A1E1-319E1EC7315B" ); // Page: Group Mapper Layout: Full Width, Site: Rock RMS

            DeleteDefinedValue( "4BF34677-37E9-4E71-BD03-252B66C9373D" );

            DeleteAttribute( "26E44877-8297-4DBC-9E5C-BCFE38291A4B" );
            DeleteAttribute( "756CB747-9143-4A7D-9787-FEECF535C939" );
            DeleteDefinedType( "45E9EF7C-91C7-45AB-92C1-1D6219293847" );

            DropForeignKey("dbo.CommunicationRecipientActivity", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.CommunicationRecipientActivity", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.CommunicationRecipientActivity", "CommunicationRecipientId", "dbo.CommunicationRecipient");
            DropIndex("dbo.CommunicationRecipientActivity", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.CommunicationRecipientActivity", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.CommunicationRecipientActivity", new[] { "CommunicationRecipientId" });
            DropColumn("dbo.CommunicationRecipient", "OpenedClient");
            DropColumn("dbo.CommunicationRecipient", "OpenedDateTime");
            DropColumn("dbo.PhoneNumber", "CountryCode");
            DropColumn("dbo.Person", "EmailPreference");
            DropTable("dbo.CommunicationRecipientActivity");
        }
    }
}
