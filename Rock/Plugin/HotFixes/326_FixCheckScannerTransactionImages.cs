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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Fixes the Check Scanner so the finance security roles can save scanned check images
    /// and adds a post update job to store existing check images with an image mime type.
    /// Fix for issue #7083.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 326, "17.8" )]
    public class FixCheckScannerTransactionImages : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddFinancialTransactionImageEditAuthUp();
            AddFixTransactionImageMimeTypesJobUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        /// <summary>
        /// Grants EDIT on FinancialTransactionImage to the finance security roles. Inserts made
        /// through the v1 REST API now require EDIT on the new entity, and unlike FinancialTransaction,
        /// FinancialBatch and FinancialPaymentDetail, this entity type never had EDIT rules for
        /// these roles.
        /// </summary>
        private void AddFinancialTransactionImageEditAuthUp()
        {
            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionImage",
                0,
                Security.Authorization.EDIT,
                true,
                SystemGuid.Group.GROUP_FINANCE_ADMINISTRATORS,
                ( int ) Model.SpecialRole.None,
                "003703EF-7BF7-4F3A-BCB6-8C79B14ECB01" );

            RockMigrationHelper.AddSecurityAuthForEntityType( "Rock.Model.FinancialTransactionImage",
                1,
                Security.Authorization.EDIT,
                true,
                SystemGuid.Group.GROUP_FINANCE_USERS,
                ( int ) Model.SpecialRole.None,
                "8733BF9D-8F5C-46B0-A78A-0F8A0E72C3C5" );
        }

        /// <summary>
        /// Adds a post update job to set the mime type of check images uploaded by the Check
        /// Scanner to image/png. The scanner uploads PNG files as application/octet-stream,
        /// which GetImage.ashx refuses to serve. This is done in a job rather than in the
        /// migration because large databases can have hundreds of thousands of these records.
        /// </summary>
        private void AddFixTransactionImageMimeTypesJobUp()
        {
            RockMigrationHelper.AddPostUpdateServiceJob(
                name: "Rock Update Helper v17.10 - Fix Transaction Image Mime Types",
                description: "This job will set the mime type of check images uploaded by the Check Scanner to image/png so that they can be displayed.",
                jobType: "Rock.Jobs.PostV1710FixTransactionImageMimeTypes",
                cronExpression: "0 0 21 1/1 * ? *",
                guid: Rock.SystemGuid.ServiceJob.DATA_MIGRATIONS_1710_FIX_TRANSACTION_IMAGE_MIME_TYPES );
        }
    }
}
