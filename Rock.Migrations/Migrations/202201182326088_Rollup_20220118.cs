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
    public partial class Rollup_20220118 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateStatementGeneratorDownloadLinkUp();
            UpdatePersonalDevices();
            AddMissingRoutes();
            FullWorkSurface();
            SetConnectionWebViewBlockTypes();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateStatementGeneratorDownloadLinkDown();
        }


        /// <summary>
        /// DV: Statement Generator Download Location - Updates the statement generator download link up.
        /// </summary>
        private void UpdateStatementGeneratorDownloadLinkUp()
        {
	        Sql( @"
		        DECLARE @statementGeneratorDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = '54E1EBCC-5A5A-4B26-9CCB-36E7CEB49C3C')
		        DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')

		        UPDATE [AttributeValue]
		        SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/statementgenerator/1.13.1/statementgenerator.msi'
		        WHERE AttributeId = @downloadUrlAttributeId and EntityId = @statementGeneratorDefinedValueId" );
        }

        /// <summary>
        /// DV: Statement Generator Download Location - Updates the statement generator download link down.
        /// </summary>
        private void UpdateStatementGeneratorDownloadLinkDown()
        {
	        Sql( @"
		        DECLARE @statementGeneratorDefinedValueId INT = (SELECT Id FROM [DefinedValue] WHERE [Guid] = '54E1EBCC-5A5A-4B26-9CCB-36E7CEB49C3C')
		        DECLARE @downloadUrlAttributeId INT = (SELECT [Id] FROM [Attribute] WHERE [Guid] = 'E0AF9B30-15EA-413B-BAC4-25B286D91FD9')

		        UPDATE [AttributeValue]
		        SET [Value] = 'https://storage.rockrms.com/externalapplications/sparkdevnetwork/statementgenerator/1.12.8/statementgenerator.msi'
		        WHERE AttributeId = @downloadUrlAttributeId and EntityId = @statementGeneratorDefinedValueId" );
        }

        /// <summary>
        /// GJ: Update Personal Devices
        /// </summary>
        private void UpdatePersonalDevices()
        {
            // Add Block Attribute Value
            //   Block: Personal Devices
            //   BlockType: Personal Devices
            //   Category: CRM
            //   Block Location: Page=Personal Devices, Site=Rock RMS
            //   Attribute: Lava Template
            /*   Attribute Value: ... */
            RockMigrationHelper.AddBlockAttributeValue("B5A94C63-869C-4B4C-B129-9E098EF5537C","24CAD424-3DAD-407C-9EA5-90FAD6293F81",@"<div class=""panel panel-block"">
    <div class=""panel-heading"">
        <h4 class=""panel-title"">
            <i class=""fa fa-mobile""></i>
            {{ Person.FullName }}
        </h4>
    </div>
    <div class=""panel-body"">
        <div class=""row d-flex flex-wrap"">
            {% for item in PersonalDevices %}
                <div class=""col-xs-6 col-sm-4 col-md-3 mb-4"">
                    <div class=""well h-100 mb-0 rollover-container"">
                        <a class=""pull-right rollover-item btn btn-xs btn-danger"" href=""#"" onclick=""Rock.dialogs.confirm('Are you sure you want to delete this Device?', function (result) { if (result ){{ item.PersonalDevice.Id | Postback:'DeleteDevice' }}}) ""><i class=""fa fa-times""></i></a>
                        <div style=""min-height: 120px;"">
                            <h3 class=""margin-v-none"">
                                {% if item.DeviceIconCssClass != '' %}
                                    <i class=""fa {{ item.DeviceIconCssClass }}""></i>
                                {% endif %}
                                {% if item.PersonalDevice.NotificationsEnabled == true %}
                                    <i class=""fa fa-comment-o""></i>
                                {% endif %}
                            </h3>
                            <dl>
                                {% if item.PlatformValue != '' %}<dt>{{ item.PlatformValue }} {{ item.PersonalDevice.DeviceVersion }}</dt>{% endif %}
                                {% if item.PersonalDevice.CreatedDateTime != null %}<dt>Discovered</dt><dd>{{ item.PersonalDevice.CreatedDateTime }}</dd>{% endif %}
                                {% if item.PersonalDevice.MACAddress != '' and item.PersonalDevice.MACAddress != null %}<dt>MAC Address</dt><dd>{{ item.PersonalDevice.MACAddress }}</dd>{% endif %}
                            </dl>
                        </div>
                        {% if LinkUrl != '' %}
                            <a href=""{{ LinkUrl | Replace:'[Id]',item.PersonalDevice.Id }}"" class=""btn btn-default btn-xs""> Interactions</a>
                        {% endif %}
                    </div>
                </div>
            {% endfor %}
        </div>
    </div>
</div>");
        }

        /// <summary>
        /// GJ: Add Missing Routes
        /// </summary>
        private void AddMissingRoutes()
        {
            Sql( @"
                DECLARE @PageId int

                -- Page: Benevolence Type Detail
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = 'DBFC432E-F0A4-457E-BA5B-572C49B899D1')
                IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'finance/benevolence/types/{BenevolenceTypeId}')
                                    INSERT INTO [PageRoute] (
                                        [IsSystem],[PageId],[Route],[Guid])
                                    VALUES(
                                        1, @PageId, 'finance/benevolence/types/{BenevolenceTypeId}', '95DEE816-F3BF-4FA0-941B-937F8A2734CB' )

                -- Page: Security Settings
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '0EF3DE1C-CB97-431E-A066-DDF8BD2D8283')
                IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/security/security-settings')
                                    INSERT INTO [PageRoute] (
                                        [IsSystem],[PageId],[Route],[Guid])
                                    VALUES(
                                        1, @PageId, 'admin/security/security-settings', '3A3C2C78-C758-4B02-9C95-C8D201F83E6C' )

                -- Page: Security Change Audit
                SET @PageId = (SELECT [Id] FROM [Page] WHERE [Guid] = '340C6E2C-7006-4490-9FD4-14D58784519B')
                IF @PageId IS NOT NULL AND NOT EXISTS(SELECT [Id] FROM [PageRoute] WHERE [PageId] = @PageId AND [Route] = 'admin/security/change-audit')
                                    INSERT INTO [PageRoute] (
                                        [IsSystem],[PageId],[Route],[Guid])
                                    VALUES(
                                        1, @PageId, 'admin/security/change-audit', '25A24576-F2BA-41DB-AB62-387748B31EF0' )" );
        }

        /// <summary>
        /// GJ: Full Worksurface Migrations
        /// </summary>
        private void FullWorkSurface()
        {
            Sql( @"
                -- Page: Connections Board to Full Worksurface
                UPDATE [Page]
                SET LayoutId = (
		                SELECT TOP 1 Id
		                FROM Layout
		                WHERE Guid = 'C2467799-BB45-4251-8EE6-F0BF27201535'
		                )
                WHERE [Guid] = '4FBCEB52-8892-4035-BDEA-112A494BE81F'

                -- Page: SMS Conversations to Full Worksurface
                UPDATE [Page]
                SET LayoutId = (
		                SELECT TOP 1 Id
		                FROM Layout
		                WHERE Guid = 'C2467799-BB45-4251-8EE6-F0BF27201535'
		                )
                WHERE [Guid] = '275A5175-60E0-40A2-8C63-4E9D9CD39036'

                -- Page: Group Scheduler to Full Worksurface
                UPDATE [Page]
                SET LayoutId = (
		                SELECT TOP 1 Id
		                FROM Layout
		                WHERE Guid = 'C2467799-BB45-4251-8EE6-F0BF27201535'
		                )
                WHERE [Guid] = '1815D8C6-7C4A-4C05-A810-CF23BA937477'" );
        }

        /// <summary>
        /// CH: Add Connection WebView BlockTypes
        /// </summary>
        private void SetConnectionWebViewBlockTypes() {

            // Add/Update WebView : Connection Opportunity List
            RockMigrationHelper.UpdateBlockType( "Connection Opportunity List", "Displays the connection type opportunities in a lava formatted block.", "~/Blocks/Connection/WebConnectionOpportunityListLava.ascx", "Connection > WebView", "B2E0E4E3-30B1-45BD-B808-C55BCD540894" );

            // Add/Update WebView : Connection Request List
            RockMigrationHelper.UpdateBlockType( "Connection Request List", "Displays the connection request in a lava formatted block.", "~/Blocks/Connection/WebConnectionRequestListLava.ascx", "Connection > WebView", "E6BAA42C-D799-4189-ABC9-4A8CA1B91D5A" );
  
            // Add/Update WebView : Connection Type List
            RockMigrationHelper.UpdateBlockType( "Connection Type List", "Displays the connection types in a lava formatted block.", "~/Blocks/Connection/WebConnectionTypeListLava.ascx", "Connection > WebView", "887F66AF-944F-4959-87F0-087E3999BAC3" );
        }
    }
}
