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
    /// <summary>
    /// Moves the TV Applications pages to the proper parent page (Digital Tools).
    /// </summary>
    public partial class TvApplicationPages : Rock.Migrations.RockMigration
    {
        private const string TvApplicationPageGuid = "452D2C48-8802-4449-9C57-DAC3876BF5DC";
        private const string DigitalToolsPageGuid = "A6D78C4F-958F-4196-B8FF-527A10F5F047";
        private const string ContentChannelsPageGuid = "8ADCE4B2-8E95-4FA3-89C4-06A883E8145E";
        private const string WebsitesPageGuid = "7596D389-4EAB-4535-8BEE-229737F46F44";
        private const string MobileApplicationsPageGuid = "784259EC-46B7-4DE3-AC37-E8BFDB0B90A6";
        private const string MediaAccountsPageGuid = "07CB7BB5-1465-4E75-8DD4-28FA6EA48222";
        private const string ShortLinksPageGuid = "8C0114FF-31CF-443E-9278-3F9E6087140C";
        private const string AdaptiveMessagePageGuid = "73112D38-E051-4452-AEF9-E473EEDD0BCB";

        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            UpdateTvApplicationPage();
            ReorderDigitalToolPages();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RevertTvApplicationPageChanges();
        }

        /// <summary>
        /// Updates the TV Apps page and moves it to the "Digital Tools" parent page.
        /// </summary>
        private void UpdateTvApplicationPage()
        {
            // Rename the page to "TV Applications"
            Sql( $@"UPDATE [dbo].[Page]
                SET [InternalName] = 'TV Applications',
                [PageTitle] = 'TV Applications',
                [BrowserTitle] = 'TV Applications'
                WHERE [Page].[Guid] = '{TvApplicationPageGuid}'" );

            RockMigrationHelper.MovePage( TvApplicationPageGuid, DigitalToolsPageGuid );
        }

        /// <summary>
        /// Reorders the digital tools pages.
        /// </summary>
        private void ReorderDigitalToolPages()
        {
            Sql( $@"UPDATE [dbo].[Page]
SET [Order] = 1
WHERE [Guid] = '{ContentChannelsPageGuid}'" );

            Sql( $@"UPDATE [dbo].[Page]
SET [Order] = 2
WHERE [Page].[Guid] = '{WebsitesPageGuid}'" );

            Sql( $@"UPDATE [dbo].[Page]
SET [Order] = 3
WHERE [Guid] = '{MobileApplicationsPageGuid}'" );

            Sql( $@"UPDATE [dbo].[Page]
SET [Order] = 4
WHERE [Page].[Guid] = '{TvApplicationPageGuid}'" );

            Sql( $@"UPDATE [dbo].[Page]
SET [Order] = 5
WHERE [Guid] = '{MediaAccountsPageGuid}'" );

            Sql( $@"UPDATE [dbo].[Page]
SET [Order] = 6
WHERE [Guid] = '{ShortLinksPageGuid}'" );

            Sql( $@"UPDATE [dbo].[Page]
SET [Order] = 7
WHERE [Guid] = '{AdaptiveMessagePageGuid}'" );
        }

        /// <summary>
        /// Moves the TV Applications page back to the CMS category.
        /// </summary>
        private void RevertTvApplicationPageChanges()
        {
            // Rename the page to "TV Applications"
            Sql( $@"UPDATE [dbo].[Page]
                SET [InternalName] = 'TV Apps',
                [PageTitle] = 'TV Apps',
                [BrowserTitle] = 'TV Apps'
                WHERE [Page].[Guid] = '{TvApplicationPageGuid}'" );

            RockMigrationHelper.MovePage( TvApplicationPageGuid, "B4A24AB7-9369-4055-883F-4F4892C39AE3" );
        }
    }
}