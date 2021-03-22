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
    public partial class Rollup_0804 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateRockShopFrontpageUI();
            AddPrayerRequestCommentsNotificationEmailTemplateUp();
            RegistrationLinkage();
            FixContentChannelItemAssociationOrder();
            AddFindGroupBlockIncludePendingAttribute();
            UpdatePersonalDeviceLavaTemplate();
            AddCommunicationApprovalTemplateWithSettingBlock();
            CodeGenMigrationsUp();
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CodeGenMigrationsDown();
        }

                /// <summary>
        /// Script generated "Up" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsUp()
        {
            
            // Add/Update Mobile Block Type:Structured Content View
            RockMigrationHelper.UpdateMobileBlockType("Structured Content View", "Displays a structured content channel item for the user to view and fill out.", "Rock.Blocks.Types.Mobile.Cms.StructuredContentView", "Mobile > Cms", "E7DF2367-931D-43BA-B792-4C8E7C5A1819");

            // Add/Update Mobile Block Type:Calendar Event Item Occurrence View
            RockMigrationHelper.UpdateMobileBlockType("Calendar Event Item Occurrence View", "Displays a particular calendar event item occurrence.", "Rock.Blocks.Types.Mobile.Events.CalendarEventItemOccurrenceView", "Mobile > Events", "F094CE87-42D7-436A-9B66-82F5804CE7F0");

            // Attribute for BlockType: Calendar Event Item Occurrence View:Registration Url
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F094CE87-42D7-436A-9B66-82F5804CE7F0", "9C204CD0-1233-41C5-818A-C5DA439445AA", "Registration Url", "RegistrationUrl", "Registration Url", @"The base URL to use when linking to the registration page.", 0, @"", "BEF3C7F9-8785-4649-9FBF-B40CA1CC2C48" );

            // Attribute for BlockType: Calendar Event Item Occurrence View:Template
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "F094CE87-42D7-436A-9B66-82F5804CE7F0", "CCD73456-C83B-4D6E-BD69-8133D2EB996D", "Template", "Template", "Template", @"The template to use when rendering the event.", 1, @"6593D4EB-2B7A-4C24-8D30-A02991D26BC0", "3B2D6815-8B49-454E-B055-33E10473682D" );

            // Attribute for BlockType: Content Channel View:Enable Tag List
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "143A2345-3E26-4ED0-A2FE-42AAF11B4C0F", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Enable Tag List", "EnableTagList", "Enable Tag List", @"Determines if the ItemTagList lava parameter will be populated.", 0, @"False", "1A48D456-9EC7-43F7-B261-8592B56235AF" );

            // Attribute for BlockType: Workflow Entry:Disable Passing WorkflowId
            RockMigrationHelper.AddOrUpdateBlockTypeAttribute( "A8BD05C8-6F89-4628-845B-059E686F089A", "1EDAFDED-DFE6-4334-B019-6EECBA89E05A", "Disable Passing WorkflowId", "DisablePassingWorkflowId", "Disable Passing WorkflowId", @"If disabled, prevents the use of a Workflow Id (WorkflowId=) from being passed in and only accepts a WorkflowGuid.", 4, @"False", "890676BC-18D3-445F-A6FA-CC2F515F1930" );
            RockMigrationHelper.UpdateFieldType("Interaction Channel Interaction Component","","Rock","Rock.Field.Types.InteractionChannelInteractionComponentFieldType","299F8444-BB47-4B6C-B523-235156BF96DC");

        }

        /// <summary>
        /// Script generated "Down" migrations for pages, blocks, and block attributes.
        /// </summary>
        private void CodeGenMigrationsDown()
        {
            
            // Disable Passing WorkflowId Attribute for BlockType: Workflow Entry
            RockMigrationHelper.DeleteAttribute("890676BC-18D3-445F-A6FA-CC2F515F1930");

            // Enable Tag List Attribute for BlockType: Content Channel View
            RockMigrationHelper.DeleteAttribute("1A48D456-9EC7-43F7-B261-8592B56235AF");

            // Template Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("3B2D6815-8B49-454E-B055-33E10473682D");

            // Registration Url Attribute for BlockType: Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteAttribute("BEF3C7F9-8785-4649-9FBF-B40CA1CC2C48");

            // Delete BlockType Calendar Event Item Occurrence View
            RockMigrationHelper.DeleteBlockType("F094CE87-42D7-436A-9B66-82F5804CE7F0"); // Calendar Event Item Occurrence View

            // Delete BlockType Structured Content View
            RockMigrationHelper.DeleteBlockType("E7DF2367-931D-43BA-B792-4C8E7C5A1819"); // Structured Content View
        }

        /// <summary>
        /// GJ: Added two additional blocks to the Rock Shop page. From hotfix 93
        /// </summary>
        private void UpdateRockShopFrontpageUI()
        {
                        // Add/Update HtmlContent for Block: Store Control Panel
            RockMigrationHelper.UpdateHtmlContentBlock("6CE75972-1204-4DE7-8BBB-C779973BFDFD",@"<div class=""panel panel-block"">
    <div class=""panel-heading"">
        <h1 class=""panel-title""><i class=""fa fa-gift""></i> Store Links</h1>
    </div>
    <ul class=""list-group"">
        <li class=""list-group-item""><a href=""~/RockShop/Purchases"">Purchases</a></li>
        <li class=""list-group-item""><a href=""https://www.rockrms.com/rockshop/support"">Support</a></li>
        <li class=""list-group-item""><a href=""~/RockShop/Account"">Account</a></li>
    </ul>
</div>","E4381B48-166F-45C8-923C-DCA75FCD033C");

            // Add Block to Page: Rock Shop Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "B093E7A0-5E7E-4A5F-AEF3-CE397D342BFA".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"B8F1B648-8C5F-4529-8F8B-B564C2A19061".AsGuid(), "Sponsored Apps","Main",@"",@"",3,"FA0152C9-71E1-47FF-9704-8D5EB39261DA");
            // Add Block to Page: Rock Shop Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "B093E7A0-5E7E-4A5F-AEF3-CE397D342BFA".AsGuid(),null,"C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(),"B8F1B648-8C5F-4529-8F8B-B564C2A19061".AsGuid(), "Top Free","Main",@"",@"",4,"C9CFEF38-AF44-4F7D-B2A6-EBC4986527D8");
            // update block order for pages with new blocks if the page,zone has multiple blocks
            Sql(@"UPDATE [Block] SET [Order] = 1 WHERE [Guid] = '5C5D96E2-7EA1-40A4-8C70-CDDCC66DD955'");  // Page: Rock Shop,  Zone: Main,  Block: Promo Rotator
            Sql(@"UPDATE [Block] SET [Order] = 0 WHERE [Guid] = 'A239E904-3E32-462E-B97D-388E7E87C37F'");  // Page: Rock Shop,  Zone: Main,  Block: Package Category Header List
            Sql(@"UPDATE [Block] SET [Order] = 2 WHERE [Guid] = '8D23BB71-69D9-4409-8368-1D965A3C5128'");  // Page: Rock Shop,  Zone: Main,  Block: Featured Promos
            Sql(@"UPDATE [Block] SET [Order] = 3 WHERE [Guid] = 'FA0152C9-71E1-47FF-9704-8D5EB39261DA'");  // Page: Rock Shop,  Zone: Main,  Block: Sponsored Apps
            Sql(@"UPDATE [Block] SET [Order] = 4 WHERE [Guid] = 'C9CFEF38-AF44-4F7D-B2A6-EBC4986527D8'");  // Page: Rock Shop,  Zone: Main,  Block: Top Free
            // Attrib Value for Block:Featured Promos, Attribute:Detail Page Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("8D23BB71-69D9-4409-8368-1D965A3C5128","96D8DDDB-A253-445A-A6A8-4F124977A788",@"d6dc6afe-70d9-43cf-9d76-eaee2317fb14");
            // Attrib Value for Block:Featured Promos, Attribute:Lava Template Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("8D23BB71-69D9-4409-8368-1D965A3C5128","40C7B22F-DF83-4BB6-8004-381FCF23398E",@"{% include '~/Assets/Lava/Store/PromoList.lava' %}");
            // Attrib Value for Block:Featured Promos, Attribute:Promo Type Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("8D23BB71-69D9-4409-8368-1D965A3C5128","14926702-E8C8-4833-B430-19CA640E9877",@"Featured");
            // Attrib Value for Block:Featured Promos, Attribute:Category Id Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("8D23BB71-69D9-4409-8368-1D965A3C5128","C1BB7381-F29C-4F24-B455-F31009BE1046",@"");
            // Attrib Value for Block:Store Control Panel, Attribute:Cache Duration Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("6CE75972-1204-4DE7-8BBB-C779973BFDFD","4DFDB295-6D0F-40A1-BEF9-7B70C56F66C4",@"3600");
            // Attrib Value for Block:Store Control Panel, Attribute:Require Approval Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("6CE75972-1204-4DE7-8BBB-C779973BFDFD","EC2B701B-4C1D-4F3F-9C77-A73C75D7FF7A",@"False");
            // Attrib Value for Block:Store Control Panel, Attribute:Context Name Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("6CE75972-1204-4DE7-8BBB-C779973BFDFD","466993F7-D838-447A-97E7-8BBDA6A57289",@"store-controlpanel");
            // Attrib Value for Block:Store Control Panel, Attribute:Enable Versioning Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("6CE75972-1204-4DE7-8BBB-C779973BFDFD","7C1CE199-86CF-4EAE-8AB3-848416A72C58",@"False");
            // Attrib Value for Block:Store Control Panel, Attribute:Context Parameter Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("6CE75972-1204-4DE7-8BBB-C779973BFDFD","3FFC512D-A576-4289-B648-905FD7A64ABB",@"");
            // Attrib Value for Block:Store Control Panel, Attribute:Start in Code Editor mode Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("6CE75972-1204-4DE7-8BBB-C779973BFDFD","0673E015-F8DD-4A52-B380-C758011331B2",@"True");
            // Attrib Value for Block:Store Control Panel, Attribute:Document Root Folder Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("6CE75972-1204-4DE7-8BBB-C779973BFDFD","3BDB8AED-32C5-4879-B1CB-8FC7C8336534",@"~/Content");
            // Attrib Value for Block:Store Control Panel, Attribute:User Specific Folders Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("6CE75972-1204-4DE7-8BBB-C779973BFDFD","9D3E4ED9-1BEF-4547-B6B0-CE29FE3835EE",@"False");
            // Attrib Value for Block:Store Control Panel, Attribute:Image Root Folder Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("6CE75972-1204-4DE7-8BBB-C779973BFDFD","26F3AFC6-C05B-44A4-8593-AFE1D9969B0E",@"~/Content");
            // Attrib Value for Block:Package Categories, Attribute:Lava Template Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("061820D6-5CDE-428F-AB29-7E2A7AE90600","15C3DABE-7B8F-48CC-8EC6-EBDBD8E04C53",@"{% include '~/Assets/Lava/Store/PackageCategoryListSidebar.lava' %}");
            // Attrib Value for Block:Package Categories, Attribute:Detail Page Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("061820D6-5CDE-428F-AB29-7E2A7AE90600","CFEA2CB4-2303-429C-B096-CBD2413AD56B",@"50d17fe7-88db-46b2-9c58-df8c0de376a4");
            // Attrib Value for Block:Sponsored Apps, Attribute:Lava Template Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("FA0152C9-71E1-47FF-9704-8D5EB39261DA","40C7B22F-DF83-4BB6-8004-381FCF23398E",@"{% include '~/Assets/Lava/Store/PromoListSponsored.lava' %}");
            // Attrib Value for Block:Sponsored Apps, Attribute:Detail Page Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("FA0152C9-71E1-47FF-9704-8D5EB39261DA","96D8DDDB-A253-445A-A6A8-4F124977A788",@"d6dc6afe-70d9-43cf-9d76-eaee2317fb14");
            // Attrib Value for Block:Sponsored Apps, Attribute:Promo Type Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("FA0152C9-71E1-47FF-9704-8D5EB39261DA","14926702-E8C8-4833-B430-19CA640E9877",@"Top Paid");
            // Attrib Value for Block:Top Free, Attribute:Promo Type Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("C9CFEF38-AF44-4F7D-B2A6-EBC4986527D8","14926702-E8C8-4833-B430-19CA640E9877",@"Top Free");
            // Attrib Value for Block:Top Free, Attribute:Detail Page Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("C9CFEF38-AF44-4F7D-B2A6-EBC4986527D8","96D8DDDB-A253-445A-A6A8-4F124977A788",@"d6dc6afe-70d9-43cf-9d76-eaee2317fb14");
            // Attrib Value for Block:Top Free, Attribute:Lava Template Page: Rock Shop, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue("C9CFEF38-AF44-4F7D-B2A6-EBC4986527D8","40C7B22F-DF83-4BB6-8004-381FCF23398E",@"{% include '~/Assets/Lava/Store/PromoListTopFree.lava' %}");

        }

        /// <summary>
        /// Adds the prayer request comments notification email template up. From hotfix 95
        /// </summary>
        private void AddPrayerRequestCommentsNotificationEmailTemplateUp()
        {
            const string prayerRequestCommentsNotificationTemplateGuid = "FAEA9DE5-62CE-4EEE-960B-C06103E97AA9";

            RockMigrationHelper.UpdateSystemEmail( "System", "Prayer Request Comments Digest", "", "", "", "", "", "Prayer Request Update",
        @"
{{ 'Global' | Attribute:'EmailHeader' }}
{% assign firstName = FirstName  %}
{% if PrayerRequest.RequestedByPersonAlias.Person.NickName %}
   {% assign firstName = PrayerRequest.RequestedByPersonAlias.Person.NickName %}
{% endif %}

<p>
{{ firstName }}, below are recent comments from the prayer request you submitted on {{ PrayerRequest.EnteredDateTime | Date:'dddd, MMMM dd' }}.
</p>
<p>
<strong>Request</strong>
<br/>
{{ PrayerRequest.Text }}
</p>
<p>
<strong>Comments</strong>
<br/>
{% for comment in Comments %}
<i>{{ comment.CreatedByPersonName }} ({{ comment.CreatedDateTime | Date:'sd' }} - {{ comment.CreatedDateTime | Date:'h:mmtt' }})</i><br/>
{{ comment.Text }}<br/><br/>
{% endfor %}
</p>

{{ 'Global' | Attribute:'EmailFooter' }}
",
                                                    prayerRequestCommentsNotificationTemplateGuid );
        }

        /// <summary>
        /// Registrations the linkage. From hotfix 115
        /// </summary>
        private void RegistrationLinkage()
        {
            RockMigrationHelper.MovePage(
                SystemGuid.Page.REGISTRATION_INSTANCE_LINKAGE,
                SystemGuid.Page.REGISTRATION_INSTANCE_LINKAGES );
        }
    
        /// <summary>
        /// Fixes the content channel item association order. From hotfix 122
        /// </summary>
        private void FixContentChannelItemAssociationOrder()
        {
            // There was a bug in ContentChannelItemDetail where ChildItems were updating and sorting child items on the ContentChannelItemAssociation.ContentChannelItem.Order
            // instead of the ContentChannelItemAssociation.Order. Now that ContentChannelItemDetail is fixed, we'll need to update the ContentChannelItemAssociation.Order
            // values to what order they were seeing when it was ordering by ContentChannelItem.Order
            Sql( @"
                UPDATE a
                SET a.[Order] = i.[Order]
                FROM ContentChannelItemAssociation a
                JOIN ContentChannelItem i ON a.ChildContentChannelItemId = i.Id
                WHERE a.[Order] != i.[Order]" );
        }
    
        /// <summary>
        /// MB: Add GroupFinder Block IncludePending Attribute
        /// </summary>
        private void AddFindGroupBlockIncludePendingAttribute()
        {
            RockMigrationHelper.AddBlockTypeAttribute( "9F8F2D68-DEEA-4686-810F-AB32923F855E", SystemGuid.FieldType.BOOLEAN, "Include Pending", "IncludePending", "", "", 0, @"True", "36362869-E8C1-4C29-BEB7-91EB0B38DD08", false );
        }

        /// <summary>
        /// GJ: Update Personal Devices Block Lava Template Defaults
        /// </summary>
        private void UpdatePersonalDeviceLavaTemplate()
        {
            // Update Attrib for BlockType: Personal Devices:Lava Template
            RockMigrationHelper.UpdateBlockTypeAttribute( "2D90562E-7332-46DB-9100-0C4106151CA1", "1D0D3794-C210-48A8-8C68-3FBEC08A6BA5", "Lava Template", "LavaTemplate", "", "Lava template to use to display content", 2, @"
<div class=""panel panel-block"">
    <div class=""panel-heading"">
        <h4 class=""panel-title"">
            <div class=''panel-panel-title''><i class=""fa fa-mobile""></i> {{ Person.FullName }}</div>
        </h4>
    </div>
    <div class=""panel-body"">
        <div class=""row row-eq-height-md flex-wrap"">
            {%- for item in PersonalDevices -%}
                <div class=""col-md-3 col-sm-4 mb-4"">
                    <div class=""well mb-0 rollover-container h-100"">
                        <a class=""pull-right rollover-item btn btn-xs btn-square btn-danger"" href=""#"" onclick=""Rock.dialogs.confirm(''Are you sure you want to delete this device?'', function (result) { if (result ){{ item.PersonalDevice.Id | Postback:''DeleteDevice'' }}}) ""><i class=""fa fa-times""></i></a>
                        <div style=""min-height: 120px;"">
                            <h3 class=""my-0"">
                                {%- if item.DeviceIconCssClass != '''' -%}
                                    <i class=""fa {{ item.DeviceIconCssClass }}""></i>
                                {%- endif -%}
                                {%- if item.PersonalDevice.NotificationsEnabled == true -%}
                                    <i class=""fa fa-comment-o""></i>
                                {%- endif -%}
                            </h3>
                            <dl>
                                {%- if item.PlatformValue != '''' -%}<dt>{{ item.PlatformValue }} {{ item.PersonalDevice.DeviceVersion }}</dt>{%- endif -%}
                                {%- if item.PersonalDevice.CreatedDateTime != null -%}<dt>Discovered</dt><dd>{{ item.PersonalDevice.CreatedDateTime }}</dd>{%- endif -%}
                                {%- if item.PersonalDevice.MACAddress != '' and item.PersonalDevice.MACAddress != null -%}<dt>MAC Address</dt><dd>{{ item.PersonalDevice.MACAddress }}</dd>{%- endif -%}
                            </dl>
                        </div>
                        {%- if LinkUrl != '''' -%}
                            <a href=""{{ LinkUrl | Replace:''[Id]'',item.PersonalDevice.Id }}"" class=""btn btn-default btn-xs""> Interactions</a>
                        {%- endif -%}
                    </div>
                </div>
            {%- endfor -%}
        </div>
    </div>
</div>
", "24CAD424-3DAD-407C-9EA5-90FAD6293F81" );


            //Update Attrib Value for Block:Personal Devices, Attribute:Lava Template Page: Personal Device, Site: Rock RMS
            RockMigrationHelper.UpdateBlockAttributeValue( "B5A94C63-869C-4B4C-B129-9E098EF5537C", "24CAD424-3DAD-407C-9EA5-90FAD6293F81", @"
<div class=""panel panel-block"">
    <div class=""panel-heading"">
        <h4 class=""panel-title"">
            <div class=''panel-panel-title''><i class=""fa fa-mobile""></i> {{ Person.FullName }}</div>
        </h4>
    </div>
    <div class=""panel-body"">
        <div class=""row row-eq-height-md flex-wrap"">
            {%- for item in PersonalDevices -%}
                <div class=""col-md-3 col-sm-4 mb-4"">
                    <div class=""well mb-0 rollover-container h-100"">
                        <a class=""pull-right rollover-item btn btn-xs btn-square btn-danger"" href=""#"" onclick=""Rock.dialogs.confirm(''Are you sure you want to delete this device?'', function (result) { if (result ){{ item.PersonalDevice.Id | Postback:''DeleteDevice'' }}}) ""><i class=""fa fa-times""></i></a>
                        <div style=""min-height: 120px;"">
                            <h3 class=""my-0"">
                                {%- if item.DeviceIconCssClass != '''' -%}
                                    <i class=""fa {{ item.DeviceIconCssClass }}""></i>
                                {%- endif -%}
                                {%- if item.PersonalDevice.NotificationsEnabled == true -%}
                                    <i class=""fa fa-comment-o""></i>
                                {%- endif -%}
                            </h3>
                            <dl>
                                {%- if item.PlatformValue != '''' -%}<dt>{{ item.PlatformValue }} {{ item.PersonalDevice.DeviceVersion }}</dt>{%- endif -%}
                                {%- if item.PersonalDevice.CreatedDateTime != null -%}<dt>Discovered</dt><dd>{{ item.PersonalDevice.CreatedDateTime }}</dd>{%- endif -%}
                                {%- if item.PersonalDevice.MACAddress != '''' and item.PersonalDevice.MACAddress != null -%}<dt>MAC Address</dt><dd>{{ item.PersonalDevice.MACAddress }}</dd>{%- endif -%}
                            </dl>
                        </div>
                        {%- if LinkUrl != '''' -%}
                            <a href=""{{ LinkUrl | Replace:''[Id]'',item.PersonalDevice.Id }}"" class=""btn btn-default btn-xs""> Interactions</a>
                        {%- endif -%}
                    </div>
                </div>
            {%- endfor -%}
        </div>
    </div>
</div>
",
@"
<div class=""panel panel-block"">
    <div class=""panel-heading"">
        <h4 class=""panel-title"">
            <i class=""fa fa-mobile""></i>
            {{ Person.FullName }}
        </h4>
    </div>
    <div class=""panel-body"">
        <div class=""row row-eq-height-md"">
            {% for item in PersonalDevices %}
                <div class=""col-md-3 col-sm-4"">
                    <div class=""well margin-b-none rollover-container"">
                        <a class=""pull-right rollover-item btn btn-xs btn-danger"" href=""#"" onclick=""Rock.dialogs.confirm(''Are you sure you want to delete this Device?'', function (result) { if (result ){{ item.PersonalDevice.Id | Postback:''DeleteDevice'' }}}) ""><i class=""fa fa-times""></i></a>
                        <div style=""min-height: 120px;"">
                            <h3 class=""margin-v-none"">
                                {% if item.DeviceIconCssClass != '''' %}
                                    <i class=""fa {{ item.DeviceIconCssClass }}""></i>
                                {% endif %}
                                {% if item.PersonalDevice.NotificationsEnabled == true %}
                                    <i class=""fa fa-comment-o""></i>
                                {% endif %}
                            </h3>
                            <dl>
                                {% if item.PlatformValue != '''' %}<dt>{{ item.PlatformValue }} {{ item.PersonalDevice.DeviceVersion }}</dt>{% endif %}
                                {% if item.PersonalDevice.CreatedDateTime != null %}<dt>Discovered</dt><dd>{{ item.PersonalDevice.CreatedDateTime }}</dd>{% endif %}
                                {% if item.PersonalDevice.MACAddress != '''' and item.PersonalDevice.MACAddress != null %}<dt>MAC Address</dt><dd>{{ item.PersonalDevice.MACAddress }}</dd>{% endif %}
                            </dl>
                        </div>
                        {% if LinkUrl != '''' %}
                            <a href=""{{ LinkUrl | Replace:''[Id]'',item.PersonalDevice.Id }}"" class=""btn btn-default btn-xs""> Interactions</a>
                        {% endif %}
                    </div>
                </div>
            {% endfor %}
        </div>
    </div>
</div>" );
        }

        /// <summary>
        /// SK: Add Communication Approval Template With Setting Block
        /// </summary>
        private void AddCommunicationApprovalTemplateWithSettingBlock()
        {
            RockMigrationHelper.UpdateSystemCommunication( "Communication",
                "Communication Approval Email",
                "", // from
                "", // fromName
                "", // to
                "", // cc
                "", // bcc
                "Pending Communication Requires Approval", // subject
                                                           // body
                @"{{ 'Global' | Attribute:'EmailHeader' }}
                            
<p>{{ Approver.NickName }}:</p>
<p>A new communication requires approval. Information about this communication can be found below.</p>
<p>
    <strong>From:</strong> {{ Communication.SenderPersonAlias.Person.FullName }}<br />
    <strong>Type:</strong> {{ Communication.CommunicationType }}<br />
    {% if Communication.CommunicationType == 'Email' %}
        <strong>From Name:</strong> {{ Communication.FromName }}<br/>
        <strong>From Address:</strong> {{ Communication.FromEmail }}<br/>
        <strong>Subject:</strong> {{ Communication.Subject }}<br/>
    {% elseif Communication.CommunicationType == 'SMS' %}
        {% if Communication.SMSFromDefinedValue != null and Communication.SMSFromDefinedValue != '' %}
            <strong>SMS Number:</strong> {{ Communication.SMSFromDefinedValue.Description }} ({{ Communication.SMSFromDefinedValue.Value }})<br/>
        {% endif %}
    {% elseif Communication.CommunicationType == 'PushNotification' %}
        <strong>Title:</strong> {{ Communication.PushTitle }}<br/>
    {% endif %}
    <strong>Recipient Count:</strong> {{ RecipientsCount }}<br />
</p>
<p>
    <a href='{{ ApprovalPageUrl }}'>View Communication</a>
</p>
    
{{ 'Global' | Attribute:'EmailFooter' }}",
                SystemGuid.SystemCommunication.COMMUNICATION_APPROVAL_EMAIL );

            // Add Page Communications Settings to Site:Rock RMS
            RockMigrationHelper.AddPage( true, "199DC522-F4D6-4D82-AF44-3C16EE9D2CDA", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Communications Settings", "", "436D1D6F-2EB9-43F3-8EEE-359DC0B09360", "fa fa-wrench" );
            // Add/Update BlockType Communication Settings
            RockMigrationHelper.UpdateBlockType( "Communication Settings", "Block used to set values specific to communication.", "~/Blocks/Communication/CommunicationSettings.ascx", "Communication", "ED6447A6-F7E0-4680-BFD1-B45527C17156" );
            // Add Block Communication Settings to Page: Communications Settings, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "436D1D6F-2EB9-43F3-8EEE-359DC0B09360".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "ED6447A6-F7E0-4680-BFD1-B45527C17156".AsGuid(), "Communication Settings", "Main", @"", @"", 0, "47FE8F7E-96F1-4C31-A59D-2096F8B817CF" );

            RockMigrationHelper.UpdateSystemSettingIfNullOrBlank( Rock.SystemKey.SystemSetting.COMMUNICATION_SETTING_APPROVAL_TEMPLATE, SystemGuid.SystemCommunication.COMMUNICATION_APPROVAL_EMAIL );
        }
    }
}
