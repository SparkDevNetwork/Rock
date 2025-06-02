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
    ///
    /// </summary>
    public partial class UpdatePrinterDpiAttributeName : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.FinancialAccount", "UsesCampusChildAccounts", c => c.Boolean( nullable: false, defaultValue: false ) );

            Sql( @"
UPDATE [Attribute]
SET [Name] = 'Label Resolution (DPI)',
    [Description] = 'Used by the check-in label designer to render the labels at the correct dots per inch (DPI) for the printer.'
WHERE [Guid] = 'd2339f8f-71c0-4a43-b7ef-5f89bde1da72'" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            Sql( @"
UPDATE [Attribute]
SET [Name] = 'DPI',
    [Description] = 'Used by the check-in label designer to render the labels at the correct size for the printer.'
WHERE [Guid] = 'd2339f8f-71c0-4a43-b7ef-5f89bde1da72'" );

            DropColumn( "dbo.FinancialAccount", "UsesCampusChildAccounts" );
        }
    }
}
