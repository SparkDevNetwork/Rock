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
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class PowerBiReportViewer : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // BI category for defined types
            RockMigrationHelper.UpdateCategory( "6028D502-79F4-4A74-9323-525E90F900C7", "BI", "fa fa-bar-chart", "Category for Business Intelligence defined types.",  Rock.SystemGuid.Category.POWERBI_DEFINED_TYPE );

            // Defined Types: Power BI Accounts
            RockMigrationHelper.AddDefinedType( "BI", "Power BI Accounts", "List of Power BI accounts and tokens.",  Rock.SystemGuid.DefinedType.POWERBI_ACCOUNTS );
            RockMigrationHelper.AddDefinedTypeAttribute( Rock.SystemGuid.DefinedType.POWERBI_ACCOUNTS, Rock.SystemGuid.FieldType.TEXT, "Client ID", "ClientId", "The client id provided when you registered your app (Rock) at https://dev.powerbi.com/apps?type=web.", 0, "", "60EEBE25-83B3-F7B9-4481-076373529D80" );
            RockMigrationHelper.AddDefinedTypeAttribute( Rock.SystemGuid.DefinedType.POWERBI_ACCOUNTS, Rock.SystemGuid.FieldType.TEXT, "Client Secret", "ClientSecret", "The client secret provided when you registered your app (Rock) at https://dev.powerbi.com/apps?type=web.", 1, "", "7DEF24AA-C538-88BB-4781-10E161FE448C" );
            RockMigrationHelper.AddDefinedTypeAttribute( Rock.SystemGuid.DefinedType.POWERBI_ACCOUNTS, Rock.SystemGuid.FieldType.TEXT, "Access Token", "AccessToken", "The access token retrieved when the account was created.", 3, "", "61D179A4-122A-99AF-4FA3-F21618E48CEE" );
            RockMigrationHelper.AddDefinedTypeAttribute( Rock.SystemGuid.DefinedType.POWERBI_ACCOUNTS, Rock.SystemGuid.FieldType.TEXT, "Refresh Token", "RefreshToken", "The refresh token for the account.", 4, "", "6693F13D-54A3-408C-4CFE-3F57AF8BBD52" );
            RockMigrationHelper.AddDefinedTypeAttribute( Rock.SystemGuid.DefinedType.POWERBI_ACCOUNTS, Rock.SystemGuid.FieldType.TEXT, "Redirect URL", "RedirectUrl", "The redirect URL defined for the account registration.", 2, "", "B2104D6A-903A-95BB-4EA5-6B54EF0EEBA6" );

            // Change Defined Type to make it a non-system
            Sql( string.Format( "UPDATE [DefinedType] SET [IsSystem] = 0 WHERE [Guid] = '{0}'",  Rock.SystemGuid.DefinedType.POWERBI_ACCOUNTS ) );

            /* NOTE: Commented this out because this was incorrect. It is done correctly in a later migration 201711011841422_GatewayTransactionKey.cs
            // register add account block
            RockMigrationHelper.UpdateBlockType( "Power Bi Account Register", "This block registers a Power BI account for Rock to use.", "~/Plugins/com_mineCartStudio/Bi/PowerBiAccountRegister.ascx", "Mine Cart Studio > BI",  Rock.SystemGuid.BlockType.POWERBI_ACCOUNT_REGISTRATION );

            // create a new page for the block (if it doesn't already exist)
            RockMigrationHelper.AddPage( true, "5B6DBC42-8B03-4D15-8D92-AAFA28FD8616", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Power BI Register Account", "",  Rock.SystemGuid.Page.POWERBI_ACCOUNT_REGISTRATION, "fa fa-bar-chart" );

            // add block to page (if it doesn't already exist)
            RockMigrationHelper.AddBlock( true, Rock.SystemGuid.Page.POWERBI_ACCOUNT_REGISTRATION, "",  Rock.SystemGuid.BlockType.POWERBI_ACCOUNT_REGISTRATION, "Account Register", "Main", "", "", 0, "A530FD84-95D2-288C-453C-999DF71D40AE" );

            // add route to page for redirect usage
            RockMigrationHelper.AddPageRoute(  Rock.SystemGuid.Page.POWERBI_ACCOUNT_REGISTRATION, "PowerBiAccountRedirect" );
            */

            
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteDefinedType(  Rock.SystemGuid.DefinedType.POWERBI_ACCOUNTS );

            // NOTE: The UP for these Power BI Blocks/Pages was commented out because is was incorrect. It is done correctly in a later migration 201711011841422_GatewayTransactionKey.cs
            RockMigrationHelper.DeleteBlock( "A530FD84-95D2-288C-453C-999DF71D40AE" );
            RockMigrationHelper.DeletePage( Rock.SystemGuid.Page.POWERBI_ACCOUNT_REGISTRATION );
            
            RockMigrationHelper.DeleteBlockType(  Rock.SystemGuid.BlockType.POWERBI_ACCOUNT_REGISTRATION );
        }
    }
}
