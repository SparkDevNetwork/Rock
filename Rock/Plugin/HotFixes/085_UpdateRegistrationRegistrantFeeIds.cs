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
//
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plugin Migration. The migration number jumps to 83 because 75-82 were moved to EF migrations and deleted.
    /// </summary>
    [MigrationNumber( 85, "1.9.0" )]
    public class UpdateRegistrationRegistrantFeeIds : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            UpdateRegistrationTemplateFeeItemId();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // Not yet used by hotfix migrations.
        }


        /// <summary>
        /// Corrects the value in [RegistrationRegistrantFee].[RegistrationTemplateFeeItemId] from null to the correct ID.
        /// </summary>
        private void UpdateRegistrationTemplateFeeItemId()
        {
            // This fixes systems that updated to 9.0 prior to the similar fix that was added to PostV90DataMigrations

            Sql( $@"
                -- UPDATE for single Fee Types 
                UPDATE [RegistrationRegistrantFee]
                SET [RegistrationRegistrantFee].[RegistrationTemplateFeeItemId] = (
	                SELECT TOP 1 [RegistrationTemplateFeeItem].[Id] 
	                FROM [RegistrationTemplateFeeItem] 
	                WHERE [RegistrationRegistrantFee].[RegistrationTemplateFeeId] = [RegistrationTemplateFeeItem].[RegistrationTemplateFeeId])
                WHERE [RegistrationRegistrantFee].[RegistrationTemplateFeeItemId] IS NULL
	                AND ( [RegistrationRegistrantFee].[Option] IS NULL
		                OR [RegistrationRegistrantFee].[Option] = '')

                -- UPDATE for multiple Fee Types
                UPDATE [RegistrationRegistrantFee]
                SET [RegistrationRegistrantFee].[RegistrationTemplateFeeItemId] = (
	                SELECT TOP 1[RegistrationTemplateFeeItem].[Id] 
	                FROM [RegistrationTemplateFeeItem] 
	                WHERE [RegistrationRegistrantFee].[RegistrationTemplateFeeId] = [RegistrationTemplateFeeItem].[RegistrationTemplateFeeId]
		                AND [RegistrationRegistrantFee].[Option] = [RegistrationTemplateFeeItem].[Name])
                WHERE [RegistrationRegistrantFee].[RegistrationTemplateFeeItemId] IS NULL
	                AND ( [RegistrationRegistrantFee].[Option] IS NOT NULL
		                OR LEN([RegistrationRegistrantFee].[Option]) > 0)" );
        }
    }
}
