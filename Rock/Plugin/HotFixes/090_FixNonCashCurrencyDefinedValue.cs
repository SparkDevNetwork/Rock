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
    [MigrationNumber( 90, "1.9.0" )]
    public class FixNonCashCurrencyDefinedValue : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //FixNonCashDefinedValue();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // Not yet used by hotfix migrations.
        }

        /// <summary>
        /// ED: Fix NonCash defined value for currency type defined type
        /// </summary>
        private void FixNonCashDefinedValue()
        {
            Sql( @"
                -- Known GUID FINANCIAL_CURRENCY_TYPE   1D1304DE-E83A-44AF-B11D-0C66DD600B81;
                -- Known GUID CURRENCY_TYPE_NONCASH     7950FF66-80EE-E8AB-4A77-4A13EDEB7513;

                -- First get the currency type defined type ID
                DECLARE @currencyTypeDefinedTypeId INT = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '1D1304DE-E83A-44AF-B11D-0C66DD600B81')

				-- Check if the Non-Cash DefinedValue exists
                IF EXISTS (SELECT * FROM [DefinedValue] WHERE [Guid] = '7950FF66-80EE-E8AB-4A77-4A13EDEB7513')
                BEGIN
                 -- Make sure this is flagged as a system DefinedValue so it cannot be deleted using the UI
                 UPDATE [DefinedValue] SET [IsSystem] = 1, [IsActive] = 1, [Value] = 'Non-Cash Asset' WHERE [Guid] = '7950FF66-80EE-E8AB-4A77-4A13EDEB7513'
                END
                ELSE 
				BEGIN
                 -- Insert the core version of the Non-Cash Asset currency type
                 INSERT INTO [DefinedValue] ([IsSystem], [DefinedTypeId], [Order], [Value], [Description], [Guid], [IsActive])
                 VALUES(1, @currencyTypeDefinedTypeId, 99, 'Non-Cash Asset', 'Used to track non-cash transactions.', '7950FF66-80EE-E8AB-4A77-4A13EDEB7513', 1)
                END" );
        }
    }
}
