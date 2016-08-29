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
    public partial class DigitalSignature4 : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.AddPage( "BC8E5377-0F6C-457A-9CF0-0F0A0AB2A418", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Document", "", "C6503D6B-F61A-4A8A-BDD1-11F9FB65B66F", "" ); // Site:Rock RMS

            // Add Block to Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlock( "BC8E5377-0F6C-457A-9CF0-0F0A0AB2A418", "", "256F6FDB-B241-4DE6-9C38-0E9DA0270A22", "Signature Document List", "SectionC1", "", "", 3, "69E78065-3BFC-452E-97C3-C104497CF7EB" );
            // Add Block to Page: Document, Site: Rock RMS
            RockMigrationHelper.AddBlock( "C6503D6B-F61A-4A8A-BDD1-11F9FB65B66F", "", "01D23E86-51DC-496D-BB3E-0CEF5094F304", "Signature Document Detail", "Main", "", "", 0, "622A6D6A-BC2F-4232-8D27-5495126C7B95" );

            // Attrib Value for Block:Signature Document List, Attribute:Detail Page Page: History, Site: Rock RMS
            RockMigrationHelper.AddBlockAttributeValue( "69E78065-3BFC-452E-97C3-C104497CF7EB", "5E982756-72C0-4397-BB16-5D1C8D0DA85D", @"c6503d6b-f61a-4a8a-bdd1-11f9fb65b66f" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Remove Block: Signature Document Detail, from Page: Document, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "622A6D6A-BC2F-4232-8D27-5495126C7B95" );
            // Remove Block: Signature Document List, from Page: History, Site: Rock RMS
            RockMigrationHelper.DeleteBlock( "69E78065-3BFC-452E-97C3-C104497CF7EB" );

            RockMigrationHelper.DeletePage( "C6503D6B-F61A-4A8A-BDD1-11F9FB65B66F" ); //  Page: Document, Layout: Full Width, Site: Rock RMS			        
        }
    }
}
