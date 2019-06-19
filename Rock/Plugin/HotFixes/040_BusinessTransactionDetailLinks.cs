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
    /// 
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 40, "1.7.0" )]
    public class BusinessTransactionDetailLinks : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //// Attrib Value for Block:Transaction List, Attribute:Accounts Page: Business Detail, Site: Rock RMS
            //RockMigrationHelper.AddBlockAttributeValue( "0A567E24-80BE-4906-B303-77D1A5FB89DE", "95621093-9BBF-4467-9828-4456D5E01E1D", @"" );
            //// Attrib Value for Block:Transaction List, Attribute:Transaction Types Page: Business Detail, Site: Rock RMS
            //RockMigrationHelper.AddBlockAttributeValue( "0A567E24-80BE-4906-B303-77D1A5FB89DE", "293F8A3E-020A-4260-8817-3E368CF31ABB", @"2d607262-52d6-4724-910d-5c6e8fb89acc" );
            //// Attrib Value for Block:Transaction List, Attribute:Batch Page Page: Business Detail, Site: Rock RMS
            //RockMigrationHelper.AddBlockAttributeValue( "0A567E24-80BE-4906-B303-77D1A5FB89DE", "683C4694-4FAC-4AFC-8987-F062EE491BC3", @"606bda31-a8fe-473a-b3f8-a00ecf7e06ec" );
            //// Attrib Value for Block:Transaction List, Attribute:Default Transaction View Page: Business Detail, Site: Rock RMS
            //RockMigrationHelper.AddBlockAttributeValue( "0A567E24-80BE-4906-B303-77D1A5FB89DE", "8D067930-6355-4DC7-98E1-3619C871AC83", @"Transactions" );
            //// Attrib Value for Block:Transaction List, Attribute:Show Account Summary Page: Business Detail, Site: Rock RMS
            //RockMigrationHelper.AddBlockAttributeValue( "0A567E24-80BE-4906-B303-77D1A5FB89DE", "4C92974B-FB99-4E89-B215-A457646D77E1", @"False" );
            //// Attrib Value for Block:Transaction Detail, Attribute:Carry Over Account Page: Transaction Detail, Site: Rock RMS
            //RockMigrationHelper.AddBlockAttributeValue( "CB2D4AA2-62BE-4F88-8368-2ED30215F3F9", "52431D4B-EA02-4D70-8B3F-74E1AD8EB3D8", @"True" );
            //// Attrib Value for Block:Transaction Detail, Attribute:Refund Batch Name Suffix Page: Transaction Detail, Site: Rock RMS
            //RockMigrationHelper.AddBlockAttributeValue( "CB2D4AA2-62BE-4F88-8368-2ED30215F3F9", "2FC63A9E-5A5B-46E9-9B49-306571526440", @"- Refund" );
            //// Attrib Value for Block:Transaction Detail, Attribute:Registration Detail Page Page: Transaction Detail, Site: Rock RMS
            //RockMigrationHelper.AddBlockAttributeValue( "CB2D4AA2-62BE-4F88-8368-2ED30215F3F9", "59461EEC-5E7D-42E5-BC0C-29CC8950AFAC", @"fc81099a-2f98-4eba-ac5a-8300b2fe46c4" );
            //// Attrib Value for Block:Transaction Detail, Attribute:Scheduled Transaction Detail Page Page: Transaction Detail, Site: Rock RMS
            //RockMigrationHelper.AddBlockAttributeValue( "CB2D4AA2-62BE-4F88-8368-2ED30215F3F9", "1990EBAE-BA67-48B0-95C2-B9EAB24E7ED9", @"f1c3bbd3-ee91-4ddd-8880-1542ebcd8041" );
            //// Attrib Value for Block:Transaction Detail, Attribute:Batch Detail Page Page: Transaction Detail, Site: Rock RMS
            //RockMigrationHelper.AddBlockAttributeValue( "CB2D4AA2-62BE-4F88-8368-2ED30215F3F9", "4B9CA04E-ED2A-45CC-9B62-D2D0A46EF7E7", @"606bda31-a8fe-473a-b3f8-a00ecf7e06ec" );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
