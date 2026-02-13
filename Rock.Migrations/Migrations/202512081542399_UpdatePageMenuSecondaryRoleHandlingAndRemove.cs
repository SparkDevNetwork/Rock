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
    public partial class UpdatePageMenuSecondaryRoleHandlingAndRemove : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Set existing Page Menu blocks to use the standard Secondary role override (4)
            Sql( @"
                UPDATE b
                SET [Role] = 4
                FROM AttributeValue av 
                JOIN Attribute a
                    ON a.Id = av.AttributeId
                JOIN BlockType bt
                    ON bt.Id = a.EntityTypeQualifierValue
                JOIN [Block] b
                    ON b.Id = av.EntityId
                WHERE a.[Guid] = 'C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2'
                    AND a.[Key] = 'IsSecondaryBlock'
                    AND bt.[Guid] = 'CACB9D1A-A820-4587-986A-D66A69EE9948'
                    AND av.[Value] = 'True' 
            " );

            // Remove the IsSecondaryBlock attribute from Page Menu since
            // it is no longer needed.
            RockMigrationHelper.DeleteAttribute( "C80209A8-D9E0-4877-A8E3-1F7DBF64D4C2" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
