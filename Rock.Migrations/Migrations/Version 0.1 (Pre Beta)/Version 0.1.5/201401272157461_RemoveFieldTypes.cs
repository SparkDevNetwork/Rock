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
    public partial class RemoveFieldTypes : Rock.Migrations.RockMigration3
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            DeleteFieldType( "50EABC9A-A29D-4A65-984A-87891B230533" );  // Currency
            DeleteFieldType( "11B0FC7B-6FD9-4D6C-AE41-C8B6EF6D4892" );  // Document
            DeleteFieldType( "85B95F22-587B-4968-851D-9196FA1FA03F" );  // Url
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            UpdateFieldType( "URL", "A URL Field", "Rock", "Rock.Field.Types.UrlFieldType", "85B95F22-587B-4968-851D-9196FA1FA03F" );
            UpdateFieldType( "Document", "A Document Field", "Rock", "Rock.Field.Types.DocumentFieldType", "11B0FC7B-6FD9-4D6C-AE41-C8B6EF6D4892" );
            UpdateFieldType( "Currency", "A Currency Field", "Rock", "Rock.Field.Types.CurrencyFieldType", "50EABC9A-A29D-4A65-984A-87891B230533" );
        }
    }
}
