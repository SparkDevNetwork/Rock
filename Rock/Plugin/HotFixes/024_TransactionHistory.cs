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
using System;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Migration to add the Fundraising feature data bits.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 24, "1.6.3" )]
    public class TransactionHistory : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Add Block to Page: Transaction Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "97716641-D003-4663-9EA2-D9BB94E7955B", "", "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0", "History Log", "Main", "", "", 1, "03D47CB4-05BB-4E76-B9CD-3D7CA6C84CEB" );
            RockMigrationHelper.AddBlockAttributeValue( "03D47CB4-05BB-4E76-B9CD-3D7CA6C84CEB", "8FB690EC-5299-46C5-8695-AAD23168E6E1", @"2c1cb26b-ab22-42d0-8164-aedee0dae667" );
            RockMigrationHelper.AddBlockAttributeValue( "03D47CB4-05BB-4E76-B9CD-3D7CA6C84CEB", "614CD413-DCB7-4DA2-80A0-C7ABE5A11047", @"Transaction History" );
            RockMigrationHelper.UpdatePageContext( "97716641-D003-4663-9EA2-D9BB94E7955B", "Rock.Model.FinancialTransaction", "TransactionId", "A696D2B5-7E0A-4702-8902-A81781B20F6B" );

            // Add Block to Page: Transaction Detail, Site: Rock RMS
            RockMigrationHelper.AddBlock( "B67E38CB-2EF1-43EA-863A-37DAA1C7340F", "", "C6C2DF41-A50D-4975-B21C-4EFD6FF3E8D0", "History Log", "Main", "", "", 1, "8B11F28B-124D-474D-8EF2-BAE1AC7E19BF" );
            RockMigrationHelper.AddBlockAttributeValue( "8B11F28B-124D-474D-8EF2-BAE1AC7E19BF", "8FB690EC-5299-46C5-8695-AAD23168E6E1", @"2c1cb26b-ab22-42d0-8164-aedee0dae667" );
            RockMigrationHelper.AddBlockAttributeValue( "8B11F28B-124D-474D-8EF2-BAE1AC7E19BF", "614CD413-DCB7-4DA2-80A0-C7ABE5A11047", @"Transaction History" );
            RockMigrationHelper.UpdatePageContext( "B67E38CB-2EF1-43EA-863A-37DAA1C7340F", "Rock.Model.FinancialTransaction", "TransactionId", "117B901D-1E76-4DE9-857C-2B32209AC35F" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            RockMigrationHelper.DeleteBlock( "03D47CB4-05BB-4E76-B9CD-3D7CA6C84CEB" );
            RockMigrationHelper.DeleteBlock( "8B11F28B-124D-474D-8EF2-BAE1AC7E19BF" );
        }
    }
}
