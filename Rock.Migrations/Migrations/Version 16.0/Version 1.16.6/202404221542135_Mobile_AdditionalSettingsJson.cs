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
using System.Collections.Generic;
using System.Data;

using Rock.Data;

namespace Rock.Migrations
{
    /// <summary>
    /// Migrates additional settings into a new JSON column for compatibility.
    /// </summary>
    public partial class Mobile_AdditionalSettingsJson : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            TvMobilePageAdditionalSettingsUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            TvMobilePageAdditionalSettingsDown();
        }

        /// <summary>
        /// Maps the legacy AdditionalSettings property to the new AdditionalSettingsJson property 
        /// for mobile and TV pages.
        /// </summary>
        private void TvMobilePageAdditionalSettingsUp()
        {
            //
            // Select all mobile and TV pages with additional settings.
            //
            string sql = @"
SELECT p.[Id], p.[AdditionalSettings], s.[SiteType]
FROM [Page] p
INNER JOIN [Layout] l ON l.[Id] = p.[LayoutId]
INNER JOIN [Site] s ON s.[Id] = l.[SiteId]
WHERE p.[AdditionalSettings] IS NOT NULL AND (s.[SiteType] = 1 OR s.[SiteType] = 2)";

            var externalAppPages = DbService.GetDataTable( sql, CommandType.Text, new Dictionary<string, object>() );

            //
            // For each page, convert the additional settings to JSON and store it in the new column.
            //
            foreach ( DataRow row in externalAppPages.Rows )
            {
                var additionalSettings = row["AdditionalSettings"].ToString();
                var siteType = row["SiteType"].ToStringSafe().ConvertToEnumOrNull<Rock.Model.SiteType>();
                var id = row["Id"].ToStringSafe().AsIntegerOrNull();

                //
                // If the additional settings is empty or for SOME reason our data is invalid, skip this row.
                //
                if ( additionalSettings.IsNullOrWhiteSpace() || siteType == null || id == null )
                {
                    continue;
                }

                var additionalSettingsJson = string.Empty;

                // Depending on our site type, convert the old additional
                // settings to the new JSON format.
                if ( siteType == Rock.Model.SiteType.Mobile )
                {
                    additionalSettingsJson = ConvertPageSettingsToJson<Mobile.AdditionalPageSettings>( additionalSettings );
                }
                else if ( siteType == Rock.Model.SiteType.Tv )
                {
                    additionalSettingsJson = ConvertPageSettingsToJson<Tv.AppleTvPageSettings>( additionalSettings );
                }

                if ( additionalSettingsJson.IsNullOrWhiteSpace() )
                {
                    continue;
                }

                //
                // Update the page with the new JSON settings.
                //
                DbService.ExecuteCommand( "UPDATE [Page] SET [AdditionalSettingsJson] = @json WHERE [Id] = @id", CommandType.Text, new Dictionary<string, object>
                {
                    ["json"] = additionalSettingsJson,
                    ["id"] = id
                } );

                //
                // Clear the old additional settings.
                //
                DbService.ExecuteCommand( "UPDATE [Page] SET [AdditionalSettings] = NULL WHERE [Id] = @id", CommandType.Text, new Dictionary<string, object>
                {
                    ["id"] = id
                } );
            }
        }

        /// <summary>
        /// Takes the AdditionalSettingsJson and maps it back to the legacy AdditionalSettings property.
        /// </summary>
        private void TvMobilePageAdditionalSettingsDown()
        {
            //
            // Select all mobile and TV pages with additional settings JSON.
            //
            string sql = @"
SELECT p.[Id], p.[AdditionalSettingsJson], s.[SiteType]
FROM [Page] p
INNER JOIN [Layout] l ON l.[Id] = p.[LayoutId]
INNER JOIN [Site] s ON s.[Id] = l.[SiteId]
WHERE p.[AdditionalSettingsJson] IS NOT NULL AND (s.[SiteType] = 1 OR s.[SiteType] = 2)";

            var externalAppPages = DbService.GetDataTable( sql, CommandType.Text, new Dictionary<string, object>() );

            //
            // For each page, convert the additional settings to JSON and store it in the new column.
            //
            foreach ( DataRow row in externalAppPages.Rows )
            {
                var additionalSettings = row["AdditionalSettingsJson"].ToString();
                var siteType = row["SiteType"].ToStringSafe().ConvertToEnumOrNull<Rock.Model.SiteType>();
                var id = row["Id"].ToStringSafe().AsIntegerOrNull();

                //
                // If the additional settings is empty or for SOME reason our data is invalid, skip this row.
                //
                if ( additionalSettings.IsNullOrWhiteSpace() || siteType == null || id == null )
                {
                    continue;
                }

                // Depending on our site type, convert the old additional
                // settings to the new JSON format.
                object settings = null;

                if ( siteType == Rock.Model.SiteType.Mobile )
                {
                    settings = GetPageSettingsObject<Mobile.AdditionalPageSettings>( additionalSettings );
                }
                else if ( siteType == Rock.Model.SiteType.Tv )
                {
                    settings = GetPageSettingsObject<Tv.AppleTvPageSettings>( additionalSettings );
                }

                if ( settings == null )
                {
                    continue;
                }

                //
                // Update the page with the new JSON settings.
                //
                DbService.ExecuteCommand( "UPDATE [Page] SET [AdditionalSettings] = @json WHERE [Id] = @id", CommandType.Text, new Dictionary<string, object>
                {
                    ["json"] = settings.ToJson(),
                    ["id"] = id
                } );
            }
        }

        /// <summary>
        /// Converts old settings to JSON format based on site type.
        /// </summary>
        /// <summary>
        /// Converts old settings to JSON format based on site type using a generic type parameter.
        /// </summary>
        private string ConvertPageSettingsToJson<T>( string settings ) where T : class
        {
            var settingsObject = settings.FromJsonOrNull<T>();
            if ( settingsObject != null )
            {
                return new Dictionary<string, object>
                {
                    [typeof( T ).Name] = settingsObject
                }.ToJson();
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the settings object from the JSON string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="settings"></param>
        /// <returns></returns>
        private T GetPageSettingsObject<T>( string settings ) where T : class
        {
            var settingsObject = settings.FromJsonOrNull<Dictionary<string, object>>();

            if ( settingsObject != null )
            {
                return ( settingsObject.GetValueOrNull( typeof( T ).Name ) as T );

            }

            return null;
        }
    }
}
