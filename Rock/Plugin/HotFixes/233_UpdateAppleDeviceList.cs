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
    [MigrationNumber(233, "1.15.5")]
    public class UpdateAppleDeviceListFourV15_6 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateDefinedValue(SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,9", "Apple Watch Series 10 46mm case (GPS)", "18e187f2-34c8-4330-b9e0-1a0e7ff8b599", true);
            RockMigrationHelper.UpdateDefinedValue(SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,8", "Apple Watch Series 10 42mm case (GPS)", "6edb5969-30fb-4fdb-a2fc-cd2af942e998", true);
            RockMigrationHelper.UpdateDefinedValue(SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,11", "Apple Watch Series 10 46mm case (GPS+Cellular)", "543a507c-7159-45c4-a5ff-207cdf644334", true);
            RockMigrationHelper.UpdateDefinedValue(SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,10", "Apple Watch Series 10 42mm case (GPS+Cellular)", "99f249f1-4ba4-49fd-b083-f127e2ade93e", true);

            RockMigrationHelper.UpdateDefinedValue(SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone17,4", "iPhone 16 Plus", "c28f3390-4a58-4f60-8cf4-60776d46a7d6", true);
            RockMigrationHelper.UpdateDefinedValue(SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone17,3", "iPhone 16", "3fc5491e-745a-4a3e-b7f4-e7adbfd953ea", true);
            RockMigrationHelper.UpdateDefinedValue(SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone17,2", "iPhone 16 Pro Max", "87f5b85d-af18-467a-9281-70447b023ba8", true);
            RockMigrationHelper.UpdateDefinedValue(SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone17,1", "iPhone 16 Pro", "9ab15493-f22e-456e-b0a1-1002e163a234", true);

            RockMigrationHelper.UpdateDefinedValue(SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,11", "iPad Air 7th Gen", "adf63539-0e5b-4e3a-b05d-11e1cdee3eca", true);
            RockMigrationHelper.UpdateDefinedValue(SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,10", "iPad Air 7th Gen", "26d0ce42-b175-40cd-a0b2-97f669df924c", true);

            RockMigrationHelper.UpdateDefinedValue(SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad16,6", "iPad Pro 12.9 inch 7th Gen", "2f91e1b5-41ad-48f0-a715-a30a9f2eb39c", true);
            RockMigrationHelper.UpdateDefinedValue(SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad16,5", "iPad Pro 12.9 inch 7th Gen", "38abeb94-ece3-45b2-ba80-1cfc79fa47e2", true);
            RockMigrationHelper.UpdateDefinedValue(SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad16,4", "iPad Pro 11 inch 5th Gen", "1d75e178-b4aa-4cd6-86e4-9faf6136fd00", true);
            RockMigrationHelper.UpdateDefinedValue(SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad16,3", "iPad Pro 11 inch 5th Gen", "0e849e28-6620-4ed1-93a0-e03cd7eba2fc", true);
            RockMigrationHelper.UpdateDefinedValue(SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad16,2", "iPad mini 7th Gen (WiFi+Cellular)", "9c75ca94-bc9a-40ec-87f1-c96bb13214f0", true);
            RockMigrationHelper.UpdateDefinedValue(SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad16,1", "iPad mini 7th Gen (WiFi)", "bb35effd-e516-4555-87e5-3778e00f621d", true);
            RockMigrationHelper.UpdateDefinedValue(SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,9", "iPad Air 6th Gen", "63d18171-3304-4b7a-bffb-76ed35db6178", true);
            RockMigrationHelper.UpdateDefinedValue(SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPad14,8", "iPad Air 6th Gen", "a3d055fa-7117-403d-82b1-a6fa0fe59039", true);


            Sql(@"
                DECLARE @AppleDeviceDefinedTypeId INT = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = 'DAE31F78-7AB9-4ACE-9EE1-C1E6A734562C')

                UPDATE [PersonalDevice] SET [Model] = dv.[Description]
                FROM [PersonalDevice] pd
                JOIN [DefinedValue] dv ON pd.[Model] = dv.[Value]
                WHERE pd.[Manufacturer] = 'Apple'
                  AND pd.[Model] like '%,%'
                  AND dv.DefinedTypeId = @AppleDeviceDefinedTypeId");
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}