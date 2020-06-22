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
    using Rock.Logging;
    using Rock.SystemKey;
    using Rock.Utility;

    /// <summary>
    ///
    /// </summary>
    public partial class AddRockLoggingConfigurationAndViewer : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddRockLogLevelDefinedValues();

            AddRockLogSystemSettings();

            AddLogPage();
        }

        private void AddRockLogLevelDefinedValues()
        {
            RockMigrationHelper.AddDefinedType( "System Settings",
                "Logging Domains",
                "Domains that can be logged.",
                Rock.SystemGuid.DefinedType.LOGGING_DOMAINS );

            RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.LOGGING_DOMAINS,
                "CMS",
                string.Empty,
                Rock.SystemGuid.DefinedValue.LOGGING_DOMAIN_CMS );

            RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.LOGGING_DOMAINS,
                "Event",
                string.Empty,
                Rock.SystemGuid.DefinedValue.LOGGING_DOMAIN_EVENT );

            RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.LOGGING_DOMAINS,
                "Reporting",
                string.Empty,
                Rock.SystemGuid.DefinedValue.LOGGING_DOMAIN_REPORTING );

            RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.LOGGING_DOMAINS,
                "Communications",
                string.Empty,
                Rock.SystemGuid.DefinedValue.LOGGING_DOMAIN_COMMUNICATIONS );

            RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.LOGGING_DOMAINS,
                "Finance",
                string.Empty,
                Rock.SystemGuid.DefinedValue.LOGGING_DOMAIN_FINANCE );

            RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.LOGGING_DOMAINS,
                "Steps",
                string.Empty,
                Rock.SystemGuid.DefinedValue.LOGGING_DOMAIN_STEPS );

            RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.LOGGING_DOMAINS,
                "Connection",
                string.Empty,
                Rock.SystemGuid.DefinedValue.LOGGING_DOMAIN_CONNECTION );

            RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.LOGGING_DOMAINS,
                "Group",
                string.Empty,
                Rock.SystemGuid.DefinedValue.LOGGING_DOMAIN_GROUP );

            RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.LOGGING_DOMAINS,
                "Streaks",
                string.Empty,
                Rock.SystemGuid.DefinedValue.LOGGING_DOMAIN_STREAKS );

            RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.LOGGING_DOMAINS,
                "Core",
                string.Empty,
                Rock.SystemGuid.DefinedValue.LOGGING_DOMAIN_CORE );

            RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.LOGGING_DOMAINS,
                "Jobs",
                string.Empty,
                Rock.SystemGuid.DefinedValue.LOGGING_DOMAIN_JOBS );

            RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.LOGGING_DOMAINS,
                "Workflow",
                string.Empty,
                Rock.SystemGuid.DefinedValue.LOGGING_DOMAIN_WORKFLOW );

            RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.LOGGING_DOMAINS,
                "CRM",
                string.Empty,
                Rock.SystemGuid.DefinedValue.LOGGING_DOMAIN_CRM );

            RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.LOGGING_DOMAINS,
                "Prayer",
                string.Empty,
                Rock.SystemGuid.DefinedValue.LOGGING_DOMAIN_PRAYER );

            RockMigrationHelper.UpdateDefinedValue(
                Rock.SystemGuid.DefinedType.LOGGING_DOMAINS,
                "Other",
                string.Empty,
                Rock.SystemGuid.DefinedValue.LOGGING_DOMAIN_OTHER );
        }

        private void AddRockLogSystemSettings()
        {
            var defaultRockLogSystemSettings = new RockLogSystemSettings
            {
                DomainsToLog = new System.Collections.Generic.List<string>(),
                LogLevel = RockLogLevel.Off,
                NumberOfLogFiles = 20,
                MaxFileSize = 20
            };

            var serializedRockLog = defaultRockLogSystemSettings.ToJson();

            RockMigrationHelper.AddGlobalAttribute( Rock.SystemGuid.FieldType.TEXT,
                Rock.Model.Attribute.SYSTEM_SETTING_QUALIFIER,
                string.Empty,
                SystemSetting.ROCK_LOGGING_SETTINGS,
                "Rock Logging System Settings",
                0,
                serializedRockLog,
                SystemGuid.Attribute.DEFINED_VALUE_LOG_SYSTEM_SETTINGS,
                SystemSetting.ROCK_LOGGING_SETTINGS );
        }

        private void AddLogPage()
        {
            RockMigrationHelper.UpdateBlockType( "Logs",
                "Block to view system logs.",
                "~/Blocks/Administration/LogViewer.ascx",
                "Administration",
                SystemGuid.BlockType.LOG_VIEWER );

            RockMigrationHelper.AddPage( true,
                SystemGuid.Page.SYSTEM_SETTINGS,
                SystemGuid.Layout.FULL_WIDTH_INTERNAL_SITE,
                "Rock Logs",
                "",
                SystemGuid.Page.LOG_VIEWER,
                "fa fa-file-medical-alt" ); // Site:Rock RMS

            // Add Block to Page: Rock Logs Site: Rock RMS
            RockMigrationHelper.AddBlock( true,
                SystemGuid.Page.LOG_VIEWER.AsGuid(),
                null,
                SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(),
                SystemGuid.BlockType.LOG_VIEWER.AsGuid(),
                "Rock Logs",
                "Main",
                "",
                "",
                0,
                SystemGuid.Block.LOG_VIEWER );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RemoveRockLogSystemSettings();

            RemoveLogPage();
        }

        private void RemoveRockLogSystemSettings()
        {
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.DEFINED_VALUE_LOG_SYSTEM_SETTINGS );
        }

        private void RemoveLogPage()
        {
            // Remove Block: Rock Logs, from Page: Rock Logs, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( SystemGuid.Block.LOG_VIEWER );

            //  Page: Rock Logs, Layout: Full Width, Site: Rock RMS
            RockMigrationHelper.DeletePage( SystemGuid.Page.LOG_VIEWER );

            // Logs
            RockMigrationHelper.DeleteBlockType( SystemGuid.BlockType.LOG_VIEWER );
        }
    }
}
