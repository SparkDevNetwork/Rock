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
    public partial class IntranetPagesEditable : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // make intranet pages non-system
            Sql( @"
UPDATE 
       [Page]
SET
       [IsSystem] = 0
WHERE
       [Guid] in ('0C4B3F4D-53FD-4A65-8C93-3868CE4DA137', '7F2581A1-941E-4D51-8A9D-5BE9B881B003', 'FBC16153-897B-457C-A35F-28FDFDC466B6', '895F58FB-C1C4-4399-A4D8-A9A10225EA09')
" );

            // make block non-system
            Sql(@"
UPDATE 
       [Block]
SET
       [IsSystem] = 0
WHERE
       [Guid] in ('718C516F-0A1D-4DBC-A939-1D9777208FEC')");


        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
