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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plug-in migration
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 131, "1.11.0" )]
    public class MigrationRollupsFor11_1_1 : Migration
    {

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            //AddFindGroupBlockIncludePendingAttribute();
            //UpdatePersonalDeviceLavaTemplate();
            //AddCommunicationApprovalTemplateWithSettingBlock();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
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
