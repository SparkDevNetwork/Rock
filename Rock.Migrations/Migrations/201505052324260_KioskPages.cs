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
    public partial class KioskPages : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // add the geographic area group location type
            RockMigrationHelper.AddDefinedValue( "2E68D37C-FB7B-4AA5-9E09-3785D52156CB", "Geographic Area", "Used to defined a geofenced area.", "44990C3F-C45B-EDA3-4B65-A238A581A26F", true );
            RockMigrationHelper.AddDefinedValueAttributeValue( "44990C3F-C45B-EDA3-4B65-A238A581A26F", "85ED6E0F-9087-4F0F-993B-4BD5FCA9DCB9", "True" );
            
            // define kiosk device
            Sql(@"
                UPDATE [DefinedValue] SET [Value] = 'Self-Service Kiosk' WHERE [Guid] = '64A1DBE5-10AD-42F1-A9BA-646A781D4112'

                DECLARE @DeviceTypeValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '64A1DBE5-10AD-42F1-A9BA-646A781D4112')
                DECLARE @LocationId int = (SELECT TOP 1 [Id] FROM [Location] WHERE [Guid] = 'D5171D44-C801-4B7D-9335-D23FB4EA0E60')

                

                INSERT INTO [Device]
	                ([Name], [Description], [DeviceTypeValueId], [LocationId], [IPAddress], [Guid], [PrintFrom], [PrintToOverride])
                VALUES
	                ('Self-Service Kiosk', 'Sample self-service/giving kiosk.', @DeviceTypeValueId, @LocationId, '::1', '8638F176-DF1E-659B-4E76-CE4227D95081', 0, 0)

                DECLARE @DeviceId int = SCOPE_IDENTITY()

                INSERT INTO [DeviceLocation]
	                ([DeviceId], [LocationId])
                VALUES
	                (@DeviceId, @LocationId)");

            // define anonymous giver	
            Sql(@"
                DECLARE @FamilyGroupTypeId int = (SELECT TOP 1 [Id] FROM [GroupType] WHERE [Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E')

                INSERT INTO [Group]
	                ([IsSystem], [GroupTypeId], [CampusId], [Name], [IsSecurityRole], [IsActive], [Order], [Guid])
                VALUES
	                (0, @FamilyGroupTypeId, 1, 'Anonymous Family', 0, 1, 0, 'E846F6EE-93E9-E287-428D-95D139D23B35')

                DECLARE @FamilyId int = SCOPE_IDENTITY()

                DECLARE @RecordTypeValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '36CF10D6-C695-413D-8E7C-4546EFEF385E')
                DECLARE @RecordStatusValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E')
                DECLARE @ConnectionStatusValueId int = (SELECT TOP 1 [Id] FROM [DefinedValue] WHERE [Guid] = '8EBC0CEB-474D-4C1B-A6BA-734C3A9AB061')

                INSERT INTO [Person]
	                ([IsSystem], [RecordTypeValueId], [RecordStatusValueId], [ConnectionStatusValueId], [FirstName], [NickName], [LastName], [Guid], [Gender])
                VALUES
	                (0, @RecordTypeValueId, @RecordStatusValueId,@ConnectionStatusValueId, 'Giver', 'Giver', 'Anonymous', '802235DC-3CA5-94B0-4326-AACE71180F48', 0)

                DECLARE @PersonId int = SCOPE_IDENTITY()

                DECLARE @PersonGuid uniqueidentifier = (SELECT TOP 1 [Guid] FROM [Person] WHERE [Id] = @PersonId)

                INSERT INTO [PersonAlias]
                    ([PersonId], [AliasPersonId], [AliasPersonGuid] , [Guid])
                 VALUES
                    (@PersonId, @PersonId, @PersonGuid, '377C6F44-7E16-CFB5-4B8E-033BA20A900F')

                INSERT INTO [GroupMember]
	                ([IsSystem], [GroupId], [PersonId], [GroupRoleId], [GroupMemberStatus], [Guid])
                VALUES
	                (0, @FamilyId, @PersonId, 3, 1, '6D6B4CBE-F17C-D9B8-49CC-B8D3AE782357')");
	
	
            // system emails for kiosk
            Sql(@"
                  DECLARE @CategoryId int = (SELECT TOP 1 [Id] FROM [Category] WHERE [Guid] = '673D13E6-0161-4AC2-B265-DF3783DE3B41')
                  INSERT INTO [SystemEmail]
	                ([IsSystem], [Title], [Subject], [Body], [Guid], [CategoryId])
                  VALUES
	                (0, 'Kiosk Giving Receipt', 'Giving Receipt from {{ GlobalAttribute.OrganizationName}}', '{{ GlobalAttribute.EmailHeader }}

                <p>
                    Thank you {{ FirstNames }} for your generous contribution. Below is the confirmation number and 
                    details for your gift.
                </p>

                <p><strong>Confirmation Number:</strong> {{ TransactionCode }}</p>

                <table>
                {% for amount in Amounts %}
	                <tr>
		                <td>{{ amount.AccountName }}</td>
		                <td>{{ ''Global'' | Attribute:''CurrencySymbol'' }} {{ amount.Amount }}</td>
	                </tr>
                {% endfor %}
                    <tr>
                        <td><strong>Total:</strong></td>
                        <td><strong>{{ ''Global'' | Attribute:''CurrencySymbol'' }} {{ TotalAmount }}</strong></td>
                    </tr>
                </table>


                {{ GlobalAttribute.EmailFooter }}', '7DBF229E-7DEE-A684-4929-6C37312A0039', @CategoryId )

                  INSERT INTO [SystemEmail]
	                ([IsSystem], [Title], [To], [Subject], [Body], [Guid], [CategoryId])
                  VALUES
	                (0, 'Kiosk Info Update', 'alisha@rocksolidchurchdemo.com', 'Kiosk Info Update', '{{ GlobalAttribute.EmailHeader }}

                <p>
                    Below is an update from the self-service kiosk.
                </p>

                {% if PersonId != '' %}
                    <p>
                        <a href=""{{ GlobalAttribute.InternalApplicationRoot }}/Person/{{ PersonId }}"">{{ FirstName }} {{ LastName }}''s Record</a>
                    </p>
                {% endif %}

                <strong>First Name:</strong> {{ FirstName }} <br/>
                <strong>Last Name:</strong> {{ LastName }} <br/>

                {% if BirthDate != '''' %}
                    <p><strong>Birth Date:</strong> {{ BirthDate }} </p>
                {% endif %}

                {% if Email != '''' %}
                    <p><strong>Email:</strong> {{ Email }} </p>
                {% endif %}

                {% if HomePhone != '''' or MobilePhone != '''' %}
                    <p>
                    {% if HomePhone != '''' %}
                        <strong>Home Phone:</strong> {{ HomePhone }} <br/>
                    {% endif %}
                    {% if MobilePhone != '''' %}
                        <strong>Mobile Phone:</strong> {{ MobilePhone }} <br/>
                    {% endif %}
                    </p>
                {% endif %}

                {% if StreetAddress != '''' %}
                    <p>
                        <strong>Address:</strong><br/> 
                        {{ StreetAddress }} <br />
                        {{ City }}, {{ State }} {{ PostalCode }} {{ County }}
                    </p>
                {% endif %}

                {% if OtherUpdates != '''' %}
                    <p>
                        <strong>Other Updates:</strong> {{ OtherUpdates }}
                    </p>
                {% endif %}

                <p>&nbsp;</p>

                {{ GlobalAttribute.EmailFooter }}', 'BC490DD4-ABBB-7DBA-4A9E-74F07F4B5881', @CategoryId )");

            //
            // pages and block settings
            //
            RockMigrationHelper.AddSite( "Self-Service Kiosk (Preview)", "Site for the self-service kiosk.", "KioskStark", "05E96F7B-B75E-4987-825A-B6F51F8D9CAA" );
            
            RockMigrationHelper.AddLayout("05E96F7B-B75E-4987-825A-B6F51F8D9CAA","FullWidth","Full Width","","8153F8AD-4116-49FD-84B8-CFF6703D7A19"); // Site:Self-Service Kiosk (Preview)
            RockMigrationHelper.AddLayout("05E96F7B-B75E-4987-825A-B6F51F8D9CAA","RightSidebar","Right Sidebar","","9F7CAB6E-33C9-460E-A644-E96F7CC56F9F"); // Site:Self-Service Kiosk (Preview)
            RockMigrationHelper.AddPage( "", "9F7CAB6E-33C9-460E-A644-E96F7CC56F9F", "Self-Service Kiosk Homepage", "", "AB045324-60A4-4972-8936-7B319FF5D2CE", "" ); // Site:Self-Service Kiosk (Preview) HOMEPAGE
            RockMigrationHelper.AddPage("AB045324-60A4-4972-8936-7B319FF5D2CE","8153F8AD-4116-49FD-84B8-CFF6703D7A19","Prayer Requests","","85E7D913-3025-4202-9F7A-D7C93A268570",""); // Site:Self-Service Kiosk (Preview)
            RockMigrationHelper.AddPage("AB045324-60A4-4972-8936-7B319FF5D2CE","8153F8AD-4116-49FD-84B8-CFF6703D7A19","Give","","FEFA6623-395F-4B43-AF76-465FE13CFBA1",""); // Site:Self-Service Kiosk (Preview)
            RockMigrationHelper.AddPage("AB045324-60A4-4972-8936-7B319FF5D2CE","8153F8AD-4116-49FD-84B8-CFF6703D7A19","Update Info","","086B6891-00C8-43F7-BB53-D42B8AA30504",""); // Site:Self-Service Kiosk (Preview)
            RockMigrationHelper.AddPageRoute("AB045324-60A4-4972-8936-7B319FF5D2CE","kiosk");// for Page:Self-Service Kiosk (Preview) Home Page
            
            // add homepage as the sites default page
            Sql( @"  UPDATE [Site] SET
	                    [DefaultPageId] = (SELECT TOP 1 [Id] FROM [Page] WHERE [Guid] = 'AB045324-60A4-4972-8936-7B319FF5D2CE')
	                    WHERE [Guid] = '05E96F7B-B75E-4987-825A-B6F51F8D9CAA'" );
            
            
            RockMigrationHelper.UpdateBlockType("Group Member Add From URL","Adds a person to a group based on inputs from the URL query string.","~/Blocks/Groups/GroupMemberAddFromUrl.ascx","Groups","42CF3822-A70C-4E07-9394-21607EED7018");
            RockMigrationHelper.UpdateBlockType("Report Data","Block to display a report with options to edit the filter","~/Blocks/Reporting/ReportData.ascx","Reporting","C7C069DB-9EEE-4245-9DF2-34E3A1FF4CCB");
            // Add Block to Page: Self-Service Kiosk (Preview) Home Page, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlock("AB045324-60A4-4972-8936-7B319FF5D2CE","","19B61D65-37E3-459F-A44F-DEF0089118A3","Welcome","Feature","","",0,"BF096DA2-1670-4956-916A-09047209CD83"); 

            // Add Block to Page: Self-Service Kiosk (Preview) Home Page, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlock("AB045324-60A4-4972-8936-7B319FF5D2CE","","0B9C6253-C72E-4CAA-B38A-BA359BC712E4","Campus Context Setter - Device","Main","","",0,"5E7E59E7-08AA-4617-9480-821A5B3B350B"); 

            // Add Block to Page: Self-Service Kiosk (Preview) Home Page, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlock("AB045324-60A4-4972-8936-7B319FF5D2CE","","CACB9D1A-A820-4587-986A-D66A69EE9948","Page Menu","Sidebar1","","",0,"5F380A94-8A5C-4F56-801C-70C64CCB7F53"); 

            // Add Block to Page: Prayer Requests, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlock("85E7D913-3025-4202-9F7A-D7C93A268570","","9D8ED334-F1F5-4377-9E27-B8C0852CF34D","Prayer Request Entry - Kiosk","Main","","",0,"EBD36C2B-8ED0-4B90-9D45-F7580459F6EA"); 

            // Add Block to Page: Update Info, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlock("086B6891-00C8-43F7-BB53-D42B8AA30504","","61C5C8F2-6F76-4583-AB97-228878A6AB65","Person Update - Kiosk","Main","","",0,"EAA18327-C4B9-4CC6-8BA1-86D785D48016"); 

            // Add Block to Page: Give, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlock("FEFA6623-395F-4B43-AF76-465FE13CFBA1","","D10900A8-C2C1-4414-A443-3781A5CF371C","Transaction Entry - Kiosk","Main","","",0,"FF9F7BC0-9508-4C97-A8B9-C059BC985EBF"); 

            // Add Block to Page: Give, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlock("FEFA6623-395F-4B43-AF76-465FE13CFBA1","","49FC4B38-741E-4B0B-B395-7C1929340D88","Idle Redirect","Main","","",1,"354DDBAC-5C43-4447-BEFA-C80D53B8DDCC"); 

            // Add Block to Page: Prayer Requests, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlock("85E7D913-3025-4202-9F7A-D7C93A268570","","49FC4B38-741E-4B0B-B395-7C1929340D88","Idle Redirect","Main","","",1,"0A0733C5-42F3-4BB5-AB0C-DA6E703F0F7A"); 

            // Add Block to Page: Update Info, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlock("086B6891-00C8-43F7-BB53-D42B8AA30504","","49FC4B38-741E-4B0B-B395-7C1929340D88","Idle Redirect","Main","","",1,"591A4FB7-25A0-4FF6-8CB7-7C24BC0754D6"); 

            // Add/Update HtmlContent for Block: Welcome
            RockMigrationHelper.UpdateHtmlContentBlock("BF096DA2-1670-4956-916A-09047209CD83",@"<h1>Welcome!</h1>","8734191A-DFF4-4783-9BAC-CABE6E39DC9D"); 

            // Attrib for BlockType: Campus Context Setter - Device:Display Lava
            RockMigrationHelper.AddBlockTypeAttribute("0B9C6253-C72E-4CAA-B38A-BA359BC712E4","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Display Lava","DisplayLava","","The Lava template to use when displaying the current campus.",0,@"{% if Device %}

    {% if Campus %}
        Campus: {{Campus.Name}}
    {% else %}
        Could not determine the campus from the device {{ Device.Name }}.
    {% endif %}
    
{% else %}
    <div class='alert alert-danger'>
        Unable to determine the device. Please check the device settings.
        <br/>
        IP Address: {{ ClientIp }}
    </div>
{% endif %}","CDC656E5-B181-4B9B-93AF-5B5F8CD032C8");

            // Attrib for BlockType: Campus Context Setter - Device:Context Scope
            RockMigrationHelper.AddBlockTypeAttribute("0B9C6253-C72E-4CAA-B38A-BA359BC712E4","7525C4CB-EE6B-41D4-9B64-A08048D5A5C0","Context Scope","ContextScope","","The scope of context to set",0,@"Site","C61D160F-89CA-4697-ABC0-76E6480DA261");

            // Attrib for BlockType: Campus Context Setter - Device:Enable Debug
            RockMigrationHelper.AddBlockTypeAttribute("0B9C6253-C72E-4CAA-B38A-BA359BC712E4","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Debug","EnableDebug","","Shows the fields available to merge in lava.",0,@"False","8165539E-9A6C-4ABD-91B9-D11D9E96244C");

            // Attrib for BlockType: Campus Context Setter - Device:Device Type
            RockMigrationHelper.AddBlockTypeAttribute("0B9C6253-C72E-4CAA-B38A-BA359BC712E4","59D5A94C-94A0-4630-B80A-BB25697D74C7","Device Type","DeviceType","","Optional filter to limit to specific device types.",0,@"","8965B47E-B2F0-4690-B07E-6308E37EF434");

            // Attrib for BlockType: Prayer Request Entry - Kiosk:Default Allow Comments Setting
            RockMigrationHelper.AddBlockTypeAttribute("9D8ED334-F1F5-4377-9E27-B8C0852CF34D","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Default Allow Comments Setting","DefaultAllowCommentsSetting","","This is the default setting for the 'Allow Comments' on prayer requests. If you enable the 'Comments Flag' below, the requestor can override this default setting.",5,@"True","9B8A9F1A-EE80-49F8-91F9-A9B18493502E");

            // Attrib for BlockType: Prayer Request Entry - Kiosk:Enable Public Display Flag
            RockMigrationHelper.AddBlockTypeAttribute("9D8ED334-F1F5-4377-9E27-B8C0852CF34D","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Public Display Flag","EnablePublicDisplayFlag","","If enabled, requestors will be able set whether or not they want their request displayed on the public website.",8,@"False","4AFA694B-1DA9-48B6-846D-91AA8D68E233");

            // Attrib for BlockType: Prayer Request Entry - Kiosk:Category Selection
            RockMigrationHelper.AddBlockTypeAttribute("9D8ED334-F1F5-4377-9E27-B8C0852CF34D","309460EF-0CC5-41C6-9161-B3837BA3D374","Category Selection","GroupCategoryId","","A top level category. This controls which categories the person can choose from when entering their prayer request.",1,@"","D991C19A-BF19-4C33-B90E-1D2BFD6463F8");

            // Attrib for BlockType: Prayer Request Entry - Kiosk:Default Category
            RockMigrationHelper.AddBlockTypeAttribute("9D8ED334-F1F5-4377-9E27-B8C0852CF34D","309460EF-0CC5-41C6-9161-B3837BA3D374","Default Category","DefaultCategory","","If categories are not being shown, choose a default category to use for all new prayer requests.",2,@"4B2D88F5-6E45-4B4B-8776-11118C8E8269","AACE4CE8-55A8-48D5-BEE2-ED1E20D8AB01");

            // Attrib for BlockType: Prayer Request Entry - Kiosk:Enable Auto Approve
            RockMigrationHelper.AddBlockTypeAttribute("9D8ED334-F1F5-4377-9E27-B8C0852CF34D","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Auto Approve","EnableAutoApprove","","If enabled, prayer requests are automatically approved; otherwise they must be approved by an admin before they can be seen by the prayer team.",3,@"True","9B40CDE4-748C-46A8-A32F-02E01F4EEA87");

            // Attrib for BlockType: Prayer Request Entry - Kiosk:Expires After (Days)
            RockMigrationHelper.AddBlockTypeAttribute("9D8ED334-F1F5-4377-9E27-B8C0852CF34D","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Expires After (Days)","ExpireDays","","Number of days until the request will expire (only applies when auto-approved is enabled).",4,@"14","D29191BC-297D-454B-93FD-5BE0501A95CD");

            // Attrib for BlockType: Prayer Request Entry - Kiosk:Enable Urgent Flag
            RockMigrationHelper.AddBlockTypeAttribute("9D8ED334-F1F5-4377-9E27-B8C0852CF34D","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Urgent Flag","EnableUrgentFlag","","If enabled, requestors will be able to flag prayer requests as urgent.",6,@"False","C09CA30F-86C3-4CB8-B592-F47CFC9D6F91");

            // Attrib for BlockType: Prayer Request Entry - Kiosk:Enable Comments Flag
            RockMigrationHelper.AddBlockTypeAttribute("9D8ED334-F1F5-4377-9E27-B8C0852CF34D","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Comments Flag","EnableCommentsFlag","","If enabled, requestors will be able set whether or not they want to allow comments on their requests.",7,@"False","7D39F766-1FB2-4A63-BCEC-0EAE3AD0E056");

            // Attrib for BlockType: Prayer Request Entry - Kiosk:Homepage
            RockMigrationHelper.AddBlockTypeAttribute("9D8ED334-F1F5-4377-9E27-B8C0852CF34D","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Homepage","Homepage","","Homepage of the kiosk.",13,@"","A3F7CB73-5749-4B2D-A0A0-68557CA86351");

            // Attrib for BlockType: Prayer Request Entry - Kiosk:Character Limit
            RockMigrationHelper.AddBlockTypeAttribute("9D8ED334-F1F5-4377-9E27-B8C0852CF34D","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Character Limit","CharacterLimit","","If set to something other than 0, this will limit the number of characters allowed when entering a new prayer request.",9,@"250","1EE14EB2-7CE4-4BFD-93C1-54D624771CB1");

            // Attrib for BlockType: Prayer Request Entry - Kiosk:Autofill User Info
            RockMigrationHelper.AddBlockTypeAttribute("9D8ED334-F1F5-4377-9E27-B8C0852CF34D","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Autofill User Info","AutofillUserInfo","","When enabled will autofill the user's info. This is generally no wanted on a public kioks.",10,@"False","7D7EE41D-091A-4FD3-938D-AC169496CC41");

            // Attrib for BlockType: Prayer Request Entry - Kiosk:Navigate To Parent On Save
            RockMigrationHelper.AddBlockTypeAttribute("9D8ED334-F1F5-4377-9E27-B8C0852CF34D","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Navigate To Parent On Save","NavigateToParentOnSave","","If enabled, on successful save control will redirect back to the parent page.",11,@"False","F48640CA-4CFD-486C-B65C-D12DEA9EA17A");

            // Attrib for BlockType: Prayer Request Entry - Kiosk:Save Success Text
            RockMigrationHelper.AddBlockTypeAttribute("9D8ED334-F1F5-4377-9E27-B8C0852CF34D","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Save Success Text","SaveSuccessText","","Text to display upon successful save. (Only applies if not navigating to parent page on save.) <span class='tip tip-html'>",12,@"<p>Thank you for allowing us to pray for you.</p>","C6E2167D-C430-403E-ACA4-83084A07EE74");

            // Attrib for BlockType: Person Update - Kiosk:Minimum Phone Number Length
            RockMigrationHelper.AddBlockTypeAttribute("61C5C8F2-6F76-4583-AB97-228878A6AB65","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Minimum Phone Number Length","MinimumPhoneNumberLength","","Minimum length for phone number searches (defaults to 4).",6,@"4","5F646F8E-BA81-41C8-88D1-9652A7966AB7");

            // Attrib for BlockType: Person Update - Kiosk:Workflow Type
            RockMigrationHelper.AddBlockTypeAttribute("61C5C8F2-6F76-4583-AB97-228878A6AB65","46A03F59-55D3-4ACE-ADD5-B4642225DD20","Workflow Type","WorkflowType","",@"The workflow type to launch when an update is made. The following attribute keys should be available on the workflow:
                            <ul>
                                <li>PersonId (Integer)</li>
                                <li>FirstName (Text)</li>
                                <li>LastName (Text)</li>
                                <li>Email (Email)</li>
                                <li>BirthDate (Date)</li>
                                <li>StreetAddress (Text)</li>
                                <li>City (Text)</li>
                                <li>State (Text)</li>
                                <li>PostalCode (Text)</li>
                                <li>Country (Text - optional)</li>
                                <li>HomePhone (Text)</li>
                                <li>MobilePhone (Text)</li>
                                <li>OtherUpdates (Memo)</li>
                            </ul>",12,@"","53BC0F6C-900A-49C9-8502-6BC041E44852");

            // Attrib for BlockType: Person Update - Kiosk:Homepage
            RockMigrationHelper.AddBlockTypeAttribute("61C5C8F2-6F76-4583-AB97-228878A6AB65","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Homepage","Homepage","","Homepage of the kiosk.",2,@"","08FA38F6-A8EE-4504-BDC8-FE17F3F2B675");

            // Attrib for BlockType: Person Update - Kiosk:Maximum Phone Number Length
            RockMigrationHelper.AddBlockTypeAttribute("61C5C8F2-6F76-4583-AB97-228878A6AB65","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Maximum Phone Number Length","MaximumPhoneNumberLength","","Maximum length for phone number searches (defaults to 10).",7,@"10","B02E02AD-0177-44EC-86A4-D5CAC785B632");

            // Attrib for BlockType: Person Update - Kiosk:Enable Debug
            RockMigrationHelper.AddBlockTypeAttribute("61C5C8F2-6F76-4583-AB97-228878A6AB65","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Debug","EnableDebug","","Shows the fields available to merge in lava.",13,@"False","9D40F664-23F7-4544-8114-32D45D0A03DC");

            // Attrib for BlockType: Person Update - Kiosk:Search Regex
            RockMigrationHelper.AddBlockTypeAttribute("61C5C8F2-6F76-4583-AB97-228878A6AB65","9C204CD0-1233-41C5-818A-C5DA439445AA","Search Regex","SearchRegex","","Regular Expression to run the search input through before searching. Useful for stripping off characters.",8,@"","7EA712EC-B03B-4FF9-B74F-124DC635180A");

            // Attrib for BlockType: Person Update - Kiosk:Update Message
            RockMigrationHelper.AddBlockTypeAttribute("61C5C8F2-6F76-4583-AB97-228878A6AB65","C28C7BF3-A552-4D77-9408-DEDCF760CED0","Update Message","UpdateMessage","","Message to show on the profile form. Leaving this blank will hide the message.",9,@"Please provide only the information that needs to be updated.","5847C50F-3333-4927-AE44-608266406380");

            // Attrib for BlockType: Person Update - Kiosk:Complete Message Lava
            RockMigrationHelper.AddBlockTypeAttribute("61C5C8F2-6F76-4583-AB97-228878A6AB65","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Complete Message Lava","CompleteMessageLava","","Message to display when complete.",10,@"<div class='alert alert-success'>We have recuived your updated information. Thank you for helping us keep your information current.</div>","DB16EAFA-99BE-423A-96C7-2BCB7F65AF97");

            // Attrib for BlockType: Person Update - Kiosk:Update Email
            RockMigrationHelper.AddBlockTypeAttribute("61C5C8F2-6F76-4583-AB97-228878A6AB65","08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF","Update Email","UpdateEmail","","The system email to use to send the updated information.",11,@"","CE4779C6-C498-4EB5-8F6E-6617B28C2B64");

            // Attrib for BlockType: Transaction Entry - Kiosk:Credit Card Gateway
            RockMigrationHelper.AddBlockTypeAttribute("D10900A8-C2C1-4414-A443-3781A5CF371C","7B34F9D8-6BBA-423E-B50E-525ABB3A1013","Credit Card Gateway","CreditCardGateway","","The payment gateway to use for Credit Card transactions",0,@"","A9095004-559E-4FF3-A1ED-88D1808B9EC4");

            // Attrib for BlockType: Transaction Entry - Kiosk:Anonymous Person
            RockMigrationHelper.AddBlockTypeAttribute("D10900A8-C2C1-4414-A443-3781A5CF371C","E4EAB7B2-0B76-429B-AFE4-AD86D7428C70","Anonymous Person","AnonymousPerson","","Person in the database to assign anonymous giving to.",3,@"","288C6C0C-EFB0-495D-8151-AF0C1BECA7B1");

            // Attrib for BlockType: Transaction Entry - Kiosk:Connection Status
            RockMigrationHelper.AddBlockTypeAttribute("D10900A8-C2C1-4414-A443-3781A5CF371C","59D5A94C-94A0-4630-B80A-BB25697D74C7","Connection Status","ConnectionStatus","","The connection status to use when creating a new individual.",4,@"8EBC0CEB-474D-4C1B-A6BA-734C3A9AB061","C85BBCE0-D963-4190-8D5D-08C16B0CD1F3");

            // Attrib for BlockType: Transaction Entry - Kiosk:Record Status
            RockMigrationHelper.AddBlockTypeAttribute("D10900A8-C2C1-4414-A443-3781A5CF371C","59D5A94C-94A0-4630-B80A-BB25697D74C7","Record Status","RecordStatus","","The record status to use when creating a new individual.",5,@"283999EC-7346-42E3-B807-BCE9B2BABB49","22B889C6-EA55-43D7-B195-777F8A8F6618");

            // Attrib for BlockType: Transaction Entry - Kiosk:Minimum Phone Number Length
            RockMigrationHelper.AddBlockTypeAttribute("D10900A8-C2C1-4414-A443-3781A5CF371C","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Minimum Phone Number Length","MinimumPhoneNumberLength","","Minimum length for phone number searches (defaults to 4).",6,@"4","3039BB40-FF96-495B-A53B-B1A89A94D497");

            // Attrib for BlockType: Transaction Entry - Kiosk:Maximum Phone Number Length
            RockMigrationHelper.AddBlockTypeAttribute("D10900A8-C2C1-4414-A443-3781A5CF371C","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Maximum Phone Number Length","MaximumPhoneNumberLength","","Maximum length for phone number searches (defaults to 10).",7,@"10","6876B697-E516-434A-BB83-8720574A3228");

            // Attrib for BlockType: Transaction Entry - Kiosk:Search Regex
            RockMigrationHelper.AddBlockTypeAttribute("D10900A8-C2C1-4414-A443-3781A5CF371C","9C204CD0-1233-41C5-818A-C5DA439445AA","Search Regex","SearchRegex","","Regular Expression to run the search input through before searching. Useful for stripping off characters.",8,@"","BE430B39-5E9D-45A6-A1B1-87453B3D68AA");

            // Attrib for BlockType: Transaction Entry - Kiosk:Receipt Lava
            RockMigrationHelper.AddBlockTypeAttribute("D10900A8-C2C1-4414-A443-3781A5CF371C","1D0D3794-C210-48A8-8C68-3FBEC08A6BA5","Receipt Lava","ReceiptLava","","Lava to display for the receipt panel.",9,@"{% include '~~/Assets/Lava/KioskGivingReceipt.lava' %}","0A4FE6C1-A71E-4673-9C5C-EF586A413D65");

            // Attrib for BlockType: Transaction Entry - Kiosk:Enable Debug
            RockMigrationHelper.AddBlockTypeAttribute("D10900A8-C2C1-4414-A443-3781A5CF371C","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Enable Debug","EnableDebug","","Shows the fields available to merge in lava.",10,@"False","1AADC7D8-8B73-4524-8C26-DA984028AA14");

            // Attrib for BlockType: Transaction Entry - Kiosk:Receipt Email
            RockMigrationHelper.AddBlockTypeAttribute("D10900A8-C2C1-4414-A443-3781A5CF371C","08F3003B-F3E2-41EC-BDF1-A2B7AC2908CF","Receipt Email","ReceiptEmail","","The system email to use to send the receipt.",11,@"","AC32787A-1BBA-4148-8308-2043E5746EC1");

            // Attrib for BlockType: Transaction Entry - Kiosk:Source
            RockMigrationHelper.AddBlockTypeAttribute("D10900A8-C2C1-4414-A443-3781A5CF371C","59D5A94C-94A0-4630-B80A-BB25697D74C7","Source","Source","","The Financial Source Type to use when creating transactions",1,@"260EEA80-821A-4F79-973F-49DF79C955F7","9649B004-C4DE-4DDE-9DF7-1EB350C357F9");

            // Attrib for BlockType: Transaction Entry - Kiosk:Homepage
            RockMigrationHelper.AddBlockTypeAttribute("D10900A8-C2C1-4414-A443-3781A5CF371C","BD53F9C9-EBA9-4D3F-82EA-DE5DD34A8108","Homepage","Homepage","","Homepage of the kiosk.",2,@"","E5ACB952-0D74-49B9-8627-C6B5C9F71CD3");

            // Attrib for BlockType: Transaction Entry - Kiosk:Accounts
            RockMigrationHelper.AddBlockTypeAttribute("D10900A8-C2C1-4414-A443-3781A5CF371C","17033CDD-EF97-4413-A483-7B85A787A87F","Accounts","Accounts","","Accounts to allow giving. This list will be filtered by campus context when displayed.",1,@"","DC40082D-B776-42FB-A463-0EEEB302101A");

            // Attrib for BlockType: Transaction Entry - Kiosk:Batch Name Prefix
            RockMigrationHelper.AddBlockTypeAttribute("D10900A8-C2C1-4414-A443-3781A5CF371C","9C204CD0-1233-41C5-818A-C5DA439445AA","Batch Name Prefix","BatchNamePrefix","","The prefix to add to the financial batch.",2,@"Kiosk Giving","D88BA3F7-DBEF-4C2C-B26F-5D0E60250FE5");

            // Attrib for BlockType: Notes:Allow Backdated Notes
            RockMigrationHelper.AddBlockTypeAttribute("2E9F32D4-B4FC-4A5F-9BE1-B2E3EA624DD3","1EDAFDED-DFE6-4334-B019-6EECBA89E05A","Allow Backdated Notes","AllowBackdatedNotes","","",12,@"False","6184511D-CC68-4FF2-90CB-3AD0AFD59D61");

            // Attrib for BlockType: Prayer Request List:Expires After (Days)
            RockMigrationHelper.AddBlockTypeAttribute("4D6B686A-79DF-4EFC-A8BA-9841C248BF74","A75DFC58-7A1B-4799-BF31-451B2BBE38FF","Expires After (Days)","ExpireDays","","Number of days until the request will expire.",1,@"14","7FEB9A26-6AE6-41F8-B6B5-D5E432C832D0");

            // Attrib for BlockType: Prayer Comment List:Category Selection
            RockMigrationHelper.AddBlockTypeAttribute("DF0F5743-2BFF-40C0-8BEE-39F1DE7B4C22","309460EF-0CC5-41C6-9161-B3837BA3D374","Category Selection","PrayerRequestCategory","","A top level category. Only prayer requests comments under this category will be shown.",1,@"","960D7BBF-E737-42A4-B372-0E7A24050BF3");

            // Attrib Value for Block:Campus Context Setter - Device, Attribute:Display Lava Page: Self-Service Kiosk (Preview) Home Page, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("5E7E59E7-08AA-4617-9480-821A5B3B350B","CDC656E5-B181-4B9B-93AF-5B5F8CD032C8",@"{% if Device %}

    {% if Campus %}
        Campus: {{Campus.Name}}
    {% else %}
        Could not determine the campus from the device {{ Device.Name }}.
    {% endif %}
    
{% else %}
    <div class='alert alert-danger'>
        Unable to determine the self-service kiosk device. Please check the device settings.
        <br/>
        IP Address: {{ ClientIp }}
    </div>
{% endif %}");

            // Attrib Value for Block:Campus Context Setter - Device, Attribute:Context Scope Page: Self-Service Kiosk (Preview) Home Page, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("5E7E59E7-08AA-4617-9480-821A5B3B350B","C61D160F-89CA-4697-ABC0-76E6480DA261",@"Site");

            // Attrib Value for Block:Campus Context Setter - Device, Attribute:Enable Debug Page: Self-Service Kiosk (Preview) Home Page, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("5E7E59E7-08AA-4617-9480-821A5B3B350B","8165539E-9A6C-4ABD-91B9-D11D9E96244C",@"False");

            // Attrib Value for Block:Campus Context Setter - Device, Attribute:Device Type Page: Self-Service Kiosk (Preview) Home Page, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("5E7E59E7-08AA-4617-9480-821A5B3B350B","8965B47E-B2F0-4690-B07E-6308E37EF434",@"64a1dbe5-10ad-42f1-a9ba-646a781d4112");

            // Attrib Value for Block:Prayer Request Entry - Kiosk, Attribute:Default Allow Comments Setting Page: Prayer Requests, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("EBD36C2B-8ED0-4B90-9D45-F7580459F6EA","9B8A9F1A-EE80-49F8-91F9-A9B18493502E",@"True");

            // Attrib Value for Block:Prayer Request Entry - Kiosk, Attribute:Enable Public Display Flag Page: Prayer Requests, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("EBD36C2B-8ED0-4B90-9D45-F7580459F6EA","4AFA694B-1DA9-48B6-846D-91AA8D68E233",@"False");

            // Attrib Value for Block:Prayer Request Entry - Kiosk, Attribute:Category Selection Page: Prayer Requests, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("EBD36C2B-8ED0-4B90-9D45-F7580459F6EA","D991C19A-BF19-4C33-B90E-1D2BFD6463F8",@"5a94e584-35f0-4214-91f1-d72531cc6325");

            // Attrib Value for Block:Prayer Request Entry - Kiosk, Attribute:Default Category Page: Prayer Requests, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("EBD36C2B-8ED0-4B90-9D45-F7580459F6EA","AACE4CE8-55A8-48D5-BEE2-ED1E20D8AB01",@"4b2d88f5-6e45-4b4b-8776-11118c8e8269");

            // Attrib Value for Block:Prayer Request Entry - Kiosk, Attribute:Enable Auto Approve Page: Prayer Requests, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("EBD36C2B-8ED0-4B90-9D45-F7580459F6EA","9B40CDE4-748C-46A8-A32F-02E01F4EEA87",@"False");

            // Attrib Value for Block:Prayer Request Entry - Kiosk, Attribute:Expires After (Days) Page: Prayer Requests, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("EBD36C2B-8ED0-4B90-9D45-F7580459F6EA","D29191BC-297D-454B-93FD-5BE0501A95CD",@"14");

            // Attrib Value for Block:Prayer Request Entry - Kiosk, Attribute:Enable Urgent Flag Page: Prayer Requests, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("EBD36C2B-8ED0-4B90-9D45-F7580459F6EA","C09CA30F-86C3-4CB8-B592-F47CFC9D6F91",@"False");

            // Attrib Value for Block:Prayer Request Entry - Kiosk, Attribute:Enable Comments Flag Page: Prayer Requests, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("EBD36C2B-8ED0-4B90-9D45-F7580459F6EA","7D39F766-1FB2-4A63-BCEC-0EAE3AD0E056",@"False");

            // Attrib Value for Block:Prayer Request Entry - Kiosk, Attribute:Homepage Page: Prayer Requests, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("EBD36C2B-8ED0-4B90-9D45-F7580459F6EA","A3F7CB73-5749-4B2D-A0A0-68557CA86351",@"ab045324-60a4-4972-8936-7b319ff5d2ce");

            // Attrib Value for Block:Prayer Request Entry - Kiosk, Attribute:Character Limit Page: Prayer Requests, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("EBD36C2B-8ED0-4B90-9D45-F7580459F6EA","1EE14EB2-7CE4-4BFD-93C1-54D624771CB1",@"250");

            // Attrib Value for Block:Prayer Request Entry - Kiosk, Attribute:Autofill User Info Page: Prayer Requests, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("EBD36C2B-8ED0-4B90-9D45-F7580459F6EA","7D7EE41D-091A-4FD3-938D-AC169496CC41",@"False");

            // Attrib Value for Block:Prayer Request Entry - Kiosk, Attribute:Navigate To Parent On Save Page: Prayer Requests, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("EBD36C2B-8ED0-4B90-9D45-F7580459F6EA","F48640CA-4CFD-486C-B65C-D12DEA9EA17A",@"False");

            // Attrib Value for Block:Prayer Request Entry - Kiosk, Attribute:Save Success Text Page: Prayer Requests, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("EBD36C2B-8ED0-4B90-9D45-F7580459F6EA","C6E2167D-C430-403E-ACA4-83084A07EE74",@"<p>Thank you for allowing us to pray for you.</p>");

            // Attrib Value for Block:Person Update - Kiosk, Attribute:Minimum Phone Number Length Page: Update Info, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("EAA18327-C4B9-4CC6-8BA1-86D785D48016","5F646F8E-BA81-41C8-88D1-9652A7966AB7",@"4");

            // Attrib Value for Block:Person Update - Kiosk, Attribute:Homepage Page: Update Info, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("EAA18327-C4B9-4CC6-8BA1-86D785D48016","08FA38F6-A8EE-4504-BDC8-FE17F3F2B675",@"ab045324-60a4-4972-8936-7b319ff5d2ce");

            // Attrib Value for Block:Person Update - Kiosk, Attribute:Maximum Phone Number Length Page: Update Info, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("EAA18327-C4B9-4CC6-8BA1-86D785D48016","B02E02AD-0177-44EC-86A4-D5CAC785B632",@"10");

            // Attrib Value for Block:Person Update - Kiosk, Attribute:Enable Debug Page: Update Info, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("EAA18327-C4B9-4CC6-8BA1-86D785D48016","9D40F664-23F7-4544-8114-32D45D0A03DC",@"False");

            // Attrib Value for Block:Person Update - Kiosk, Attribute:Search Regex Page: Update Info, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("EAA18327-C4B9-4CC6-8BA1-86D785D48016","7EA712EC-B03B-4FF9-B74F-124DC635180A",@"");

            // Attrib Value for Block:Person Update - Kiosk, Attribute:Update Message Page: Update Info, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("EAA18327-C4B9-4CC6-8BA1-86D785D48016","5847C50F-3333-4927-AE44-608266406380",@"Please provide only the information that needs to be updated.");

            // Attrib Value for Block:Person Update - Kiosk, Attribute:Complete Message Lava Page: Update Info, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("EAA18327-C4B9-4CC6-8BA1-86D785D48016","DB16EAFA-99BE-423A-96C7-2BCB7F65AF97",@"<div class='alert alert-success'>We have recuived your updated information. Thank you for helping us keep your information current.</div>");

            // Attrib Value for Block:Person Update - Kiosk, Attribute:Update Email Page: Update Info, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("EAA18327-C4B9-4CC6-8BA1-86D785D48016","CE4779C6-C498-4EB5-8F6E-6617B28C2B64",@"bc490dd4-abbb-7dba-4a9e-74f07f4b5881");

            // Attrib Value for Block:Transaction Entry - Kiosk, Attribute:Credit Card Gateway Page: Give, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue( "FF9F7BC0-9508-4C97-A8B9-C059BC985EBF", "A9095004-559E-4FF3-A1ED-88D1808B9EC4", @"6432d2d2-32ff-443d-b5b3-fb6c8414c3ad" );

            // Attrib Value for Block:Transaction Entry - Kiosk, Attribute:Anonymous Person Page: Give, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("FF9F7BC0-9508-4C97-A8B9-C059BC985EBF","288C6C0C-EFB0-495D-8151-AF0C1BECA7B1",@"377c6f44-7e16-cfb5-4b8e-033ba20a900f");

            // Attrib Value for Block:Transaction Entry - Kiosk, Attribute:Connection Status Page: Give, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("FF9F7BC0-9508-4C97-A8B9-C059BC985EBF","C85BBCE0-D963-4190-8D5D-08C16B0CD1F3",@"8ebc0ceb-474d-4c1b-a6ba-734c3a9ab061");

            // Attrib Value for Block:Transaction Entry - Kiosk, Attribute:Record Status Page: Give, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("FF9F7BC0-9508-4C97-A8B9-C059BC985EBF","22B889C6-EA55-43D7-B195-777F8A8F6618",@"283999ec-7346-42e3-b807-bce9b2babb49");

            // Attrib Value for Block:Transaction Entry - Kiosk, Attribute:Minimum Phone Number Length Page: Give, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("FF9F7BC0-9508-4C97-A8B9-C059BC985EBF","3039BB40-FF96-495B-A53B-B1A89A94D497",@"4");

            // Attrib Value for Block:Transaction Entry - Kiosk, Attribute:Maximum Phone Number Length Page: Give, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("FF9F7BC0-9508-4C97-A8B9-C059BC985EBF","6876B697-E516-434A-BB83-8720574A3228",@"10");

            // Attrib Value for Block:Transaction Entry - Kiosk, Attribute:Search Regex Page: Give, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("FF9F7BC0-9508-4C97-A8B9-C059BC985EBF","BE430B39-5E9D-45A6-A1B1-87453B3D68AA",@"");

            // Attrib Value for Block:Transaction Entry - Kiosk, Attribute:Receipt Lava Page: Give, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("FF9F7BC0-9508-4C97-A8B9-C059BC985EBF","0A4FE6C1-A71E-4673-9C5C-EF586A413D65",@"{% include '~~/Assets/Lava/KioskGivingReceipt.lava' %}");

            // Attrib Value for Block:Transaction Entry - Kiosk, Attribute:Enable Debug Page: Give, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("FF9F7BC0-9508-4C97-A8B9-C059BC985EBF","1AADC7D8-8B73-4524-8C26-DA984028AA14",@"False");

            // Attrib Value for Block:Transaction Entry - Kiosk, Attribute:Receipt Email Page: Give, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("FF9F7BC0-9508-4C97-A8B9-C059BC985EBF","AC32787A-1BBA-4148-8308-2043E5746EC1",@"7dbf229e-7dee-a684-4929-6c37312a0039");

            // Attrib Value for Block:Transaction Entry - Kiosk, Attribute:Source Page: Give, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("FF9F7BC0-9508-4C97-A8B9-C059BC985EBF","9649B004-C4DE-4DDE-9DF7-1EB350C357F9",@"260eea80-821a-4f79-973f-49df79c955f7");

            // Attrib Value for Block:Transaction Entry - Kiosk, Attribute:Homepage Page: Give, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("FF9F7BC0-9508-4C97-A8B9-C059BC985EBF","E5ACB952-0D74-49B9-8627-C6B5C9F71CD3",@"ab045324-60a4-4972-8936-7b319ff5d2ce");

            // Attrib Value for Block:Transaction Entry - Kiosk, Attribute:Accounts Page: Give, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("FF9F7BC0-9508-4C97-A8B9-C059BC985EBF","DC40082D-B776-42FB-A463-0EEEB302101A",@"4410306f-3fb5-4a57-9a80-09a3f9d40d0c,67c6181c-1d8c-44d7-b262-b81e746f06d8");

            // Attrib Value for Block:Transaction Entry - Kiosk, Attribute:Batch Name Prefix Page: Give, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("FF9F7BC0-9508-4C97-A8B9-C059BC985EBF","D88BA3F7-DBEF-4C2C-B26F-5D0E60250FE5",@"Kiosk Giving");

            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Give, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("354DDBAC-5C43-4447-BEFA-C80D53B8DDCC","2254B67B-9CB1-47DE-A63D-D0B56051ECD4",@"/kiosk");

            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Give, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("354DDBAC-5C43-4447-BEFA-C80D53B8DDCC","1CAC7B16-041A-4F40-8AEE-A39DFA076C14",@"20");

            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Prayer Requests, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("0A0733C5-42F3-4BB5-AB0C-DA6E703F0F7A","1CAC7B16-041A-4F40-8AEE-A39DFA076C14",@"20");

            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Prayer Requests, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("0A0733C5-42F3-4BB5-AB0C-DA6E703F0F7A","2254B67B-9CB1-47DE-A63D-D0B56051ECD4",@"/kiosk");

            // Attrib Value for Block:Idle Redirect, Attribute:Idle Seconds Page: Update Info, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("591A4FB7-25A0-4FF6-8CB7-7C24BC0754D6","1CAC7B16-041A-4F40-8AEE-A39DFA076C14",@"20");

            // Attrib Value for Block:Idle Redirect, Attribute:New Location Page: Update Info, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.AddBlockAttributeValue("591A4FB7-25A0-4FF6-8CB7-7C24BC0754D6","2254B67B-9CB1-47DE-A63D-D0B56051ECD4",@"/kiosk");

            // fix rss lava
            RockMigrationHelper.AddDefinedValueAttributeValue( "D6149581-9EFC-40D8-BD38-E92C0717BEDA", "1E13E409-B568-45D0-B4B6-556C87D61232", @"{% assign timezone = 'Now' | Date:'zzz' | Replace:':','' -%}
<?xml version=""1.0"" encoding=""utf-8""?>
<rss version=""2.0"" xmlns:atom=""http://www.w3.org/2005/Atom"">

<channel>
    <title>{{ Channel.Name | Escape }}</title>
    <link>{{ Channel.ChannelUrl }}</link>
    <description>{{ Channel.Description | Escape }}</description>
    <language>en-us</language>
    <ttl>{{ Channel.TimeToLive }}</ttl>
    <lastBuildDate>{{ 'Now' | Date:'ddd, dd MMM yyyy HH:mm:00' }} {{ timezone }}</lastBuildDate>
{% for item in Items -%}
    <item>
        <title>{{ item.Title | Escape }}</title>
        <guid>{{ Channel.ItemUrl }}?Item={{ item.Id }}</guid>
        <link>{{ Channel.ItemUrl }}?Item={{ item.Id }}</link>
        <pubDate>{{ item.StartDateTime | Date:'ddd, dd MMM yyyy HH:mm:00' }} {{ timezone }}</pubDate>
        <description>{{ item.Content | Escape }}</description>
    </item>
{% endfor -%}

</channel>
</rss>" );

        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // add the geographic area group location type
            RockMigrationHelper.DeleteDefinedValue( "44990C3F-C45B-EDA3-4B65-A238A581A26F" );

            
            // delete kiosk
            Sql("DELETE FROM [Device] WHERE [Guid] = '8638F176-DF1E-659B-4E76-CE4227D95081'");

            // delete anonymous giver
            Sql(@"
                DELETE FROM [GroupMember] WHERE [Guid] = '6D6B4CBE-F17C-D9B8-49CC-B8D3AE782357'
                DELETE FROM [PersonViewed] WHERE [TargetPersonAliasId] IN (SELECT [Id] FROM [PersonAlias] WHERE [AliasPersonId] = (SELECT [Id] FROM [Person] WHERE [Guid] = '802235DC-3CA5-94B0-4326-AACE71180F48'))
                DELETE FROM [PersonAlias] WHERE [AliasPersonId] = (SELECT [Id] FROM [Person] WHERE [Guid] = '802235DC-3CA5-94B0-4326-AACE71180F48')
                DELETE FROM [Person] WHERE [Guid] = '802235DC-3CA5-94B0-4326-AACE71180F48'
                DELETE FROM [Group] WHERE [Guid] = 'E846F6EE-93E9-E287-428D-95D139D23B35'");

            // delete system emails
            Sql( @"
                DELETE FROM [SystemEmail] WHERE [Guid] = '7DBF229E-7DEE-A684-4929-6C37312A0039'
                DELETE FROM [SystemEmail] WHERE [Guid] = 'BC490DD4-ABBB-7DBA-4A9E-74F07F4B5881'" );

            // remove pages and block settings
            // Attrib for BlockType: Prayer Comment List:Category Selection
            RockMigrationHelper.DeleteAttribute("960D7BBF-E737-42A4-B372-0E7A24050BF3");
            // Attrib for BlockType: Prayer Request List:Expires After (Days)
            RockMigrationHelper.DeleteAttribute("7FEB9A26-6AE6-41F8-B6B5-D5E432C832D0");
            // Attrib for BlockType: Notes:Allow Backdated Notes
            RockMigrationHelper.DeleteAttribute("6184511D-CC68-4FF2-90CB-3AD0AFD59D61");
            // Attrib for BlockType: Transaction Entry - Kiosk:Batch Name Prefix
            RockMigrationHelper.DeleteAttribute("D88BA3F7-DBEF-4C2C-B26F-5D0E60250FE5");
            // Attrib for BlockType: Transaction Entry - Kiosk:Accounts
            RockMigrationHelper.DeleteAttribute("DC40082D-B776-42FB-A463-0EEEB302101A");
            // Attrib for BlockType: Transaction Entry - Kiosk:Homepage
            RockMigrationHelper.DeleteAttribute("E5ACB952-0D74-49B9-8627-C6B5C9F71CD3");
            // Attrib for BlockType: Transaction Entry - Kiosk:Source
            RockMigrationHelper.DeleteAttribute("9649B004-C4DE-4DDE-9DF7-1EB350C357F9");
            // Attrib for BlockType: Transaction Entry - Kiosk:Receipt Email
            RockMigrationHelper.DeleteAttribute("AC32787A-1BBA-4148-8308-2043E5746EC1");
            // Attrib for BlockType: Transaction Entry - Kiosk:Enable Debug
            RockMigrationHelper.DeleteAttribute("1AADC7D8-8B73-4524-8C26-DA984028AA14");
            // Attrib for BlockType: Transaction Entry - Kiosk:Receipt Lava
            RockMigrationHelper.DeleteAttribute("0A4FE6C1-A71E-4673-9C5C-EF586A413D65");
            // Attrib for BlockType: Transaction Entry - Kiosk:Search Regex
            RockMigrationHelper.DeleteAttribute("BE430B39-5E9D-45A6-A1B1-87453B3D68AA");
            // Attrib for BlockType: Transaction Entry - Kiosk:Maximum Phone Number Length
            RockMigrationHelper.DeleteAttribute("6876B697-E516-434A-BB83-8720574A3228");
            // Attrib for BlockType: Transaction Entry - Kiosk:Minimum Phone Number Length
            RockMigrationHelper.DeleteAttribute("3039BB40-FF96-495B-A53B-B1A89A94D497");
            // Attrib for BlockType: Transaction Entry - Kiosk:Record Status
            RockMigrationHelper.DeleteAttribute("22B889C6-EA55-43D7-B195-777F8A8F6618");
            // Attrib for BlockType: Transaction Entry - Kiosk:Connection Status
            RockMigrationHelper.DeleteAttribute("C85BBCE0-D963-4190-8D5D-08C16B0CD1F3");
            // Attrib for BlockType: Transaction Entry - Kiosk:Anonymous Person
            RockMigrationHelper.DeleteAttribute("288C6C0C-EFB0-495D-8151-AF0C1BECA7B1");
            // Attrib for BlockType: Transaction Entry - Kiosk:Credit Card Gateway
            RockMigrationHelper.DeleteAttribute("A9095004-559E-4FF3-A1ED-88D1808B9EC4");
            // Attrib for BlockType: Person Update - Kiosk:Update Email
            RockMigrationHelper.DeleteAttribute("CE4779C6-C498-4EB5-8F6E-6617B28C2B64");
            // Attrib for BlockType: Person Update - Kiosk:Complete Message Lava
            RockMigrationHelper.DeleteAttribute("DB16EAFA-99BE-423A-96C7-2BCB7F65AF97");
            // Attrib for BlockType: Person Update - Kiosk:Update Message
            RockMigrationHelper.DeleteAttribute("5847C50F-3333-4927-AE44-608266406380");
            // Attrib for BlockType: Person Update - Kiosk:Search Regex
            RockMigrationHelper.DeleteAttribute("7EA712EC-B03B-4FF9-B74F-124DC635180A");
            // Attrib for BlockType: Person Update - Kiosk:Enable Debug
            RockMigrationHelper.DeleteAttribute("9D40F664-23F7-4544-8114-32D45D0A03DC");
            // Attrib for BlockType: Person Update - Kiosk:Maximum Phone Number Length
            RockMigrationHelper.DeleteAttribute("B02E02AD-0177-44EC-86A4-D5CAC785B632");
            // Attrib for BlockType: Person Update - Kiosk:Homepage
            RockMigrationHelper.DeleteAttribute("08FA38F6-A8EE-4504-BDC8-FE17F3F2B675");
            // Attrib for BlockType: Person Update - Kiosk:Workflow Type
            RockMigrationHelper.DeleteAttribute("53BC0F6C-900A-49C9-8502-6BC041E44852");
            // Attrib for BlockType: Person Update - Kiosk:Minimum Phone Number Length
            RockMigrationHelper.DeleteAttribute("5F646F8E-BA81-41C8-88D1-9652A7966AB7");
            // Attrib for BlockType: Prayer Request Entry - Kiosk:Save Success Text
            RockMigrationHelper.DeleteAttribute("C6E2167D-C430-403E-ACA4-83084A07EE74");
            // Attrib for BlockType: Prayer Request Entry - Kiosk:Navigate To Parent On Save
            RockMigrationHelper.DeleteAttribute("F48640CA-4CFD-486C-B65C-D12DEA9EA17A");
            // Attrib for BlockType: Prayer Request Entry - Kiosk:Autofill User Info
            RockMigrationHelper.DeleteAttribute("7D7EE41D-091A-4FD3-938D-AC169496CC41");
            // Attrib for BlockType: Prayer Request Entry - Kiosk:Character Limit
            RockMigrationHelper.DeleteAttribute("1EE14EB2-7CE4-4BFD-93C1-54D624771CB1");
            // Attrib for BlockType: Prayer Request Entry - Kiosk:Homepage
            RockMigrationHelper.DeleteAttribute("A3F7CB73-5749-4B2D-A0A0-68557CA86351");
            // Attrib for BlockType: Prayer Request Entry - Kiosk:Enable Comments Flag
            RockMigrationHelper.DeleteAttribute("7D39F766-1FB2-4A63-BCEC-0EAE3AD0E056");
            // Attrib for BlockType: Prayer Request Entry - Kiosk:Enable Urgent Flag
            RockMigrationHelper.DeleteAttribute("C09CA30F-86C3-4CB8-B592-F47CFC9D6F91");
            // Attrib for BlockType: Prayer Request Entry - Kiosk:Expires After (Days)
            RockMigrationHelper.DeleteAttribute("D29191BC-297D-454B-93FD-5BE0501A95CD");
            // Attrib for BlockType: Prayer Request Entry - Kiosk:Enable Auto Approve
            RockMigrationHelper.DeleteAttribute("9B40CDE4-748C-46A8-A32F-02E01F4EEA87");
            // Attrib for BlockType: Prayer Request Entry - Kiosk:Default Category
            RockMigrationHelper.DeleteAttribute("AACE4CE8-55A8-48D5-BEE2-ED1E20D8AB01");
            // Attrib for BlockType: Prayer Request Entry - Kiosk:Category Selection
            RockMigrationHelper.DeleteAttribute("D991C19A-BF19-4C33-B90E-1D2BFD6463F8");
            // Attrib for BlockType: Prayer Request Entry - Kiosk:Enable Public Display Flag
            RockMigrationHelper.DeleteAttribute("4AFA694B-1DA9-48B6-846D-91AA8D68E233");
            // Attrib for BlockType: Prayer Request Entry - Kiosk:Default Allow Comments Setting
            RockMigrationHelper.DeleteAttribute("9B8A9F1A-EE80-49F8-91F9-A9B18493502E");
            // Attrib for BlockType: Campus Context Setter - Device:Device Type
            RockMigrationHelper.DeleteAttribute("8965B47E-B2F0-4690-B07E-6308E37EF434");
            // Attrib for BlockType: Campus Context Setter - Device:Enable Debug
            RockMigrationHelper.DeleteAttribute("8165539E-9A6C-4ABD-91B9-D11D9E96244C");
            // Attrib for BlockType: Campus Context Setter - Device:Context Scope
            RockMigrationHelper.DeleteAttribute("C61D160F-89CA-4697-ABC0-76E6480DA261");
            // Attrib for BlockType: Campus Context Setter - Device:Display Lava
            RockMigrationHelper.DeleteAttribute("CDC656E5-B181-4B9B-93AF-5B5F8CD032C8");
            // Remove Block: Idle Redirect, from Page: Update Info, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.DeleteBlock("591A4FB7-25A0-4FF6-8CB7-7C24BC0754D6");
            // Remove Block: Idle Redirect, from Page: Prayer Requests, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.DeleteBlock("0A0733C5-42F3-4BB5-AB0C-DA6E703F0F7A");
            // Remove Block: Idle Redirect, from Page: Give, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.DeleteBlock("354DDBAC-5C43-4447-BEFA-C80D53B8DDCC");
            // Remove Block: Transaction Entry - Kiosk, from Page: Give, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.DeleteBlock("FF9F7BC0-9508-4C97-A8B9-C059BC985EBF");
            // Remove Block: Person Update - Kiosk, from Page: Update Info, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.DeleteBlock("EAA18327-C4B9-4CC6-8BA1-86D785D48016");
            // Remove Block: Prayer Request Entry - Kiosk, from Page: Prayer Requests, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.DeleteBlock("EBD36C2B-8ED0-4B90-9D45-F7580459F6EA");
            // Remove Block: Page Menu, from Page: Self-Service Kiosk (Preview) Home Page, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.DeleteBlock("5F380A94-8A5C-4F56-801C-70C64CCB7F53");
            // Remove Block: Campus Context Setter - Device, from Page: Self-Service Kiosk (Preview) Home Page, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.DeleteBlock("5E7E59E7-08AA-4617-9480-821A5B3B350B");
            // Remove Block: Welcome, from Page: Self-Service Kiosk (Preview) Home Page, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.DeleteBlock("BF096DA2-1670-4956-916A-09047209CD83");
            RockMigrationHelper.DeleteBlockType("C7C069DB-9EEE-4245-9DF2-34E3A1FF4CCB"); // Report Data
            RockMigrationHelper.DeleteBlockType("42CF3822-A70C-4E07-9394-21607EED7018"); // Group Member Add From URL

            Sql( @"UPDATE [SITE] SET [DefaultPageId] = null WHERE [Guid] = '05E96F7B-B75E-4987-825A-B6F51F8D9CAA'" );

            RockMigrationHelper.DeletePage("086B6891-00C8-43F7-BB53-D42B8AA30504"); //  Page: Update Info, Layout: Full Width, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.DeletePage("FEFA6623-395F-4B43-AF76-465FE13CFBA1"); //  Page: Give, Layout: Full Width, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.DeletePage("85E7D913-3025-4202-9F7A-D7C93A268570"); //  Page: Prayer Requests, Layout: Full Width, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.DeletePage("AB045324-60A4-4972-8936-7B319FF5D2CE"); //  Page: Self-Service Kiosk (Preview) Home Page, Layout: Right Sidebar, Site: Self-Service Kiosk (Preview)

            RockMigrationHelper.DeleteLayout( "9F7CAB6E-33C9-460E-A644-E96F7CC56F9F" ); //  Layout: Right Sidebar, Site: Self-Service Kiosk (Preview)
            RockMigrationHelper.DeleteLayout( "8153F8AD-4116-49FD-84B8-CFF6703D7A19" ); //  Layout: Full Width, Site: Self-Service Kiosk (Preview)

            Sql( @"DELETE FROM [Site] WHERE [Guid] = '05E96F7B-B75E-4987-825A-B6F51F8D9CAA'" );
        }
    }
}
