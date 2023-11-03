﻿// <copyright>
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
    [MigrationNumber( 189, "1.15.1" )]
    public class UpdateAppleDeviceList : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone15,4", "iPhone 15", "d97cdd93-276c-487e-9a91-7fb8023856cc", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone15,5", "iPhone 15 Plus", "a8d06af7-9baf-4ec8-91c2-0e0eca171b5a", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone16,1", "iPhone 15 Pro", "e4f9fa97-d51f-47ba-9795-79320e025817", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "iPhone16,2", "iPhone 15 Pro Max", "91e5aa06-6b34-4d08-91a7-7c75a891041d", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,1", "Apple Watch Series 9 41mm case (GPS)", "f5468959-14a5-46c8-bcfd-201ad56a7008", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,2", "Apple Watch Series 9 45mm case (GPS)", "c5ef18a9-cd62-446f-bede-9f28b0907d41", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,3", "Apple Watch Series 9 41mm case (GPS+Cellular)", "7b902398-25f3-4250-95b1-1c5469a81330", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,4", "Apple Watch Series 9 45mm case (GPS+Cellular)", "f9c8ca02-db1b-4525-bace-7a1afbec335c", true );
            RockMigrationHelper.UpdateDefinedValue( SystemGuid.DefinedType.APPLE_DEVICE_MODELS, "Watch7,5", "Apple Watch Ultra 2", "baef68c7-274a-4ba3-ac4a-8d781ea57b42", true );

            // Remove invalid values. No need to update Device Modles since the description is correct.
            RockMigrationHelper.DeleteDefinedValue( "F902AB78-90EB-46E8-839D-8082F537AF22" ); //iPad14,6-A
            RockMigrationHelper.DeleteDefinedValue( "8417E05B-C79A-4CCE-B781-F38908E509C9" ); //iPad14,6-B

            Sql( @"
                DECLARE @AppleDeviceDefinedTypeId INT = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = 'DAE31F78-7AB9-4ACE-9EE1-C1E6A734562C')

                UPDATE [PersonalDevice] SET [Model] = dv.[Description]
                FROM [PersonalDevice] pd
                JOIN [DefinedValue] dv ON pd.[Model] = dv.[Value]
                WHERE pd.[Manufacturer] = 'Apple'
                  AND pd.[Model] like '%,%'
                  AND dv.DefinedTypeId = @AppleDeviceDefinedTypeId" );
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
