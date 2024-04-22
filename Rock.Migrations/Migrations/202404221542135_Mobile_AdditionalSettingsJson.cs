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
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Define actions for rollback here, if necessary.
        }
    }
}
