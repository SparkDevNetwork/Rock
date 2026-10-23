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
    /// Rollup of migrations for v21.0.2: converts the Currency Type and Transaction Source
    /// defined value icons from Font Awesome to Tabler.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 329, "21.0" )]
    public class MigrationRollupsForV21_0_2 : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            JN_ConvertCurrencyAndTransactionSourceIconsToTabler_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }

        #region JN_ConvertCurrencyAndTransactionSourceIconsToTabler

        /// <summary>
        /// JN: Currency &amp; Transaction Types Defined Values - Icons need converted to Tabler.
        /// Only values and defaults still holding the originally seeded icon are changed, so
        /// customized icons are preserved.
        /// </summary>
        private void JN_ConvertCurrencyAndTransactionSourceIconsToTabler_Up()
        {
            Sql( @"
-- Convert the Currency Type and Transaction Source icons from Font Awesome to Tabler.
-- Only rows still holding the originally seeded value are changed, so customized icons are preserved.
UPDATE [av]
SET [av].[Value] = [m].[NewValue],
    [av].[IsPersistedValueDirty] = 1
FROM [AttributeValue] AS [av]
INNER JOIN [Attribute] AS [a] ON [a].[Id] = [av].[AttributeId]
INNER JOIN [DefinedValue] AS [dv] ON [dv].[Id] = [av].[EntityId]
INNER JOIN ( VALUES
    -- Currency Type > Icon CSS Class attribute
    ( '9617D1DC-6561-4314-83EB-7F0ACBA2E259', '8B086A19-405A-451F-8D44-174E92D6B402', 'fa-money-check', 'ti ti-cash-banknote' ),        -- Check
    ( '9617D1DC-6561-4314-83EB-7F0ACBA2E259', 'F3ADC889-1EE8-4EB6-B3FD-8C10F3C8AF93', 'fa-money-bill', 'ti ti-cash' ),                 -- Cash
    ( '9617D1DC-6561-4314-83EB-7F0ACBA2E259', '928A2E04-C77B-4282-888F-EC549CEE026A', 'fa-credit-card', 'ti ti-credit-card' ),         -- Credit Card
    ( '9617D1DC-6561-4314-83EB-7F0ACBA2E259', 'DABEE8FD-AEDF-43E1-8547-4C97FA14D9B6', 'fa-university', 'ti ti-building-bank' ),        -- ACH
    ( '9617D1DC-6561-4314-83EB-7F0ACBA2E259', 'D42C4DF7-1AE9-4DDE-ADA2-774B866B798C', 'fa-apple-pay', 'ti ti-brand-apple' ),           -- Apple Pay
    ( '9617D1DC-6561-4314-83EB-7F0ACBA2E259', '6151F6E0-3223-46BA-A59E-E091BE4AF75C', 'fa-google-pay', 'ti ti-brand-google' ),         -- Google Pay
    ( '9617D1DC-6561-4314-83EB-7F0ACBA2E259', '7950FF66-80EE-E8AB-4A77-4A13EDEB7513', 'fa-file-contract', 'ti ti-file-dollar' ),       -- Non-Cash Asset
    ( '9617D1DC-6561-4314-83EB-7F0ACBA2E259', '56C9AE9C-B5EB-46D5-9650-2EF86B14F856', 'fa-question', 'ti ti-question-mark' ),          -- Unknown
    ( '9617D1DC-6561-4314-83EB-7F0ACBA2E259', '0FDF0BB3-B483-4C0A-9DFF-A35ABE3B688D', 'fa-dollar-sign', 'ti ti-currency-dollar' ),     -- Other
    -- Transaction Source > Icon CSS Class attribute
    ( 'CB1E9401-E1FD-4DBB-B15F-4E6994602723', '7D705CE7-7B11-4342-A58E-53617C5B4E69', 'fa-desktop', 'ti ti-device-desktop' ),          -- Website
    ( 'CB1E9401-E1FD-4DBB-B15F-4E6994602723', '260EEA80-821A-4F79-973F-49DF79C955F7', 'fa-cash-register', 'ti ti-cash-register' ),     -- Kiosk
    ( 'CB1E9401-E1FD-4DBB-B15F-4E6994602723', '8ADCEC72-63FC-4F08-A4CC-72BCE470172C', 'fa-mobile-alt', 'ti ti-device-mobile' ),        -- Mobile Application
    ( 'CB1E9401-E1FD-4DBB-B15F-4E6994602723', 'BE7ECF50-52BC-4774-808D-574BA842DB98', 'fa-church', 'ti ti-building-church' ),          -- On-Site
    ( 'CB1E9401-E1FD-4DBB-B15F-4E6994602723', '61E46A46-7399-4817-A6EC-3D8495E2316E', 'fa-money-check', 'ti ti-cash-banknote' ),       -- Bank Checks
    ( 'CB1E9401-E1FD-4DBB-B15F-4E6994602723', '8BA95E24-D291-499E-A535-4DCAC365689B', 'fa-sms', 'ti ti-device-mobile-message' )        -- SMS Gift
) AS [m] ( [AttributeGuid], [DefinedValueGuid], [OldValue], [NewValue] )
    ON [m].[AttributeGuid] = [a].[Guid]
    AND [m].[DefinedValueGuid] = [dv].[Guid]
    AND [m].[OldValue] = [av].[Value];

-- Convert the attribute default (fallback) icons, only if still the originally seeded value.
UPDATE [Attribute]
SET [DefaultValue] = 'ti ti-credit-card'
WHERE [Guid] = '9617D1DC-6561-4314-83EB-7F0ACBA2E259' -- Currency Type > Icon CSS Class
    AND [DefaultValue] = 'fa-credit-card';

UPDATE [Attribute]
SET [DefaultValue] = 'ti ti-heart-dollar'
WHERE [Guid] = 'CB1E9401-E1FD-4DBB-B15F-4E6994602723' -- Transaction Source > Icon CSS Class
    AND [DefaultValue] = 'fa-hand-holding-usd';" );
        }

        #endregion JN_ConvertCurrencyAndTransactionSourceIconsToTabler
    }
}
